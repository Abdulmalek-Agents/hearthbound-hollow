// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase54_HollowGlyphsBuilder
//
// PHASE 54 (D-070) — bake an on-brand GOLD glyph TMP Sprite Asset ("HollowGlyphs")
// so the onboarding / control-hint emoji render crisply in TextMesh Pro instead of
// as missing-glyph "tofu" boxes, then register it as the TMP default sprite asset
// AND an emoji fallback so both `<sprite name="…">` tags (HollowGlyphs.Format) and
// raw Unicode emoji resolve.
//
// WHY a builder (not a committed binary): the studio pipeline is "Build Everything"
// (CLAUDE.md §3.4) and we avoid committing generated textures. This builder is
// idempotent (load-or-create + heal) and FULLY DEFENSIVE — every step is wrapped so
// it can never break the Build Everything chain. If anything fails it logs and the
// game falls back to HollowGlyphs' clean-text path (no tofu, ever).
//
// The glyphs are warm gold line-icons drawn procedurally (no external art): they sit
// better on the parchment UI than full-colour emoji. To use richer art instead, drop
// a TMP Sprite Asset named "HollowGlyphs" at the path below and this builder will
// leave it alone.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore;
using TMPro;

namespace HearthboundHollow.EditorTools
{
    public static class Phase54_HollowGlyphsBuilder
    {
        private const string AssetDir  = "Assets/_Project/Art/UI";
        private const string AssetPath = AssetDir + "/HollowGlyphs.asset";
        private const string TexPath   = AssetDir + "/HollowGlyphs_Atlas.png";
        private const int    Cell      = 64;          // px per glyph cell
        private const int    Cols      = 4;
        private const int    Rows      = 3;

        // name, drawId — order defines the atlas cell index. Unicode is mapped in Build().
        private static readonly (string name, int unicode)[] Glyphs =
        {
            ("lantern",  0x1FA94),
            ("walk",     0x1F6B6),
            ("hand",     0x270B),
            ("sparkle",  0x2728),
            ("candle",   0x1F56F),
            ("leaf",     0x1F342),
            ("question", 0x2753),
            ("key",      0x1F511),
            ("coin",     0x1FA99),
            ("teapot",   0x1FAD6),
        };

        [MenuItem("Hearthbound/⚙️ Advanced/Phase 54 — Build Hollow Glyphs (emoji)")]
        public static void BuildMenu() { Build(); }

        // Auto-install once on editor load when the asset is missing, so a fresh
        // pull renders the onboarding/hint emoji without any manual step (and
        // without needing to touch the Build Everything chain). Idempotent: once
        // the asset exists it is never rebuilt automatically. Fully defensive.
        [InitializeOnLoadMethod]
        private static void AutoInstallIfMissing()
        {
            EditorApplication.delayCall += () =>
            {
                try
                {
                    if (AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(AssetPath) == null)
                        Build();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[Phase54] HollowGlyphs auto-install skipped (non-fatal): {ex.Message}");
                }
            };
        }

        /// <summary>Idempotent. Returns the built/loaded sprite asset, or null on failure.</summary>
        public static TMP_SpriteAsset Build()
        {
            try
            {
                if (!AssetDatabase.IsValidFolder(AssetDir))
                {
                    System.IO.Directory.CreateDirectory(AssetDir);
                    AssetDatabase.Refresh();
                }

                // 1) Atlas texture (load-or-create + always re-bake pixels so a
                //    palette/icon tweak re-renders on the next Build Everything).
                var tex = BakeAtlasTexture();
                if (tex == null) { Debug.LogWarning("[Phase54] glyph atlas bake failed; skipping."); return null; }

                // 2) Sprite asset (load-or-create).
                var spriteAsset = AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(AssetPath);
                bool created = false;
                if (spriteAsset == null)
                {
                    spriteAsset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
                    spriteAsset.name = HollowGlyphsName;
                    AssetDatabase.CreateAsset(spriteAsset, AssetPath);
                    created = true;
                }
                spriteAsset.spriteSheet = tex;

                // 3) Material (TMP sprite shader).
                var shader = Shader.Find("TextMeshPro/Sprite");
                if (shader != null)
                {
                    var mat = spriteAsset.material;
                    if (mat == null) { mat = new Material(shader) { name = "HollowGlyphs Material" }; spriteAsset.material = mat; }
                    mat.SetTexture("_MainTex", tex);
                    if (created || AssetDatabase.GetAssetPath(mat) == "")
                        AssetDatabase.AddObjectToAsset(mat, spriteAsset);
                }

                // 4) Glyph + character tables.
                PopulateTables(spriteAsset, tex);

                // 5) Lookups + save.
                spriteAsset.UpdateLookupTables();
                EditorUtility.SetDirty(spriteAsset);
                AssetDatabase.SaveAssets();

                // 6) Register as TMP default sprite asset + emoji fallback (best-effort).
                RegisterWithTmpSettings(spriteAsset);

                Debug.Log($"[Phase54] HollowGlyphs sprite asset {(created ? "created" : "healed")} ({Glyphs.Length} glyphs) and registered with TMP.");
                return spriteAsset;
            }
            catch (Exception ex)
            {
                // NEVER break Build Everything — clean-text fallback covers us.
                Debug.LogWarning($"[Phase54] HollowGlyphs builder failed (non-fatal): {ex.Message}");
                return null;
            }
        }

        private const string HollowGlyphsName = "HollowGlyphs";

        // ───── Atlas painting ─────────────────────────────────────────

        private static Texture2D BakeAtlasTexture()
        {
            int w = Cols * Cell, h = Rows * Cell;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var clear = new Color(1f, 1f, 1f, 0f);
            var px = new Color[w * h];
            for (int i = 0; i < px.Length; i++) px[i] = clear;
            tex.SetPixels(px);

            var gold = new Color(0.97f, 0.82f, 0.49f, 1f);
            for (int i = 0; i < Glyphs.Length; i++)
            {
                int col = i % Cols, row = i / Cols;
                // Atlas rows are bottom-up in Unity texture space.
                int ox = col * Cell;
                int oy = (Rows - 1 - row) * Cell;
                DrawGlyph(tex, Glyphs[i].name, ox, oy, Cell, gold);
            }

            tex.Apply();
            // Persist as PNG so the asset has a stable on-disk texture.
            try
            {
                System.IO.File.WriteAllBytes(TexPath, tex.EncodeToPNG());
                AssetDatabase.ImportAsset(TexPath);
                var imp = AssetImporter.GetAtPath(TexPath) as TextureImporter;
                if (imp != null)
                {
                    imp.textureType = TextureImporterType.Default;
                    imp.alphaIsTransparency = true;
                    imp.mipmapEnabled = false;
                    imp.SaveAndReimport();
                }
                var loaded = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath);
                return loaded != null ? loaded : tex;
            }
            catch { return tex; }
        }

        // Simple primitive drawing — warm gold line-icons. Coordinates are cell-local
        // (0..size) with origin at the cell's bottom-left in atlas space.
        private static void DrawGlyph(Texture2D t, string name, int ox, int oy, int size, Color c)
        {
            float cx = ox + size * 0.5f, cy = oy + size * 0.5f;
            float r = size * 0.34f, th = Mathf.Max(2f, size * 0.06f);
            switch (name)
            {
                case "sparkle":
                    // 4-point star = two crossed tapered diamonds.
                    Line(t, cx, oy + size * 0.12f, cx, oy + size * 0.88f, th, c);
                    Line(t, ox + size * 0.12f, cy, ox + size * 0.88f, cy, th, c);
                    Line(t, cx - r * 0.5f, cy - r * 0.5f, cx + r * 0.5f, cy + r * 0.5f, th * 0.6f, c);
                    Line(t, cx - r * 0.5f, cy + r * 0.5f, cx + r * 0.5f, cy - r * 0.5f, th * 0.6f, c);
                    break;
                case "leaf":
                    FillEllipse(t, cx, cy, r * 0.7f, r, c, hollow: false, rot: 30f);
                    Line(t, cx - r * 0.4f, cy - r * 0.6f, cx + r * 0.4f, cy + r * 0.6f, th * 0.8f, new Color(0.5f, 0.38f, 0.18f, 1f));
                    break;
                case "candle":
                    FillRect(t, cx - size * 0.10f, oy + size * 0.18f, size * 0.20f, size * 0.46f, c);
                    Teardrop(t, cx, oy + size * 0.74f, size * 0.12f, new Color(1f, 0.72f, 0.34f, 1f));
                    break;
                case "lantern":
                    RectOutline(t, cx - size * 0.20f, oy + size * 0.22f, size * 0.40f, size * 0.46f, th, c);
                    Line(t, cx, oy + size * 0.68f, cx, oy + size * 0.80f, th, c);            // hook
                    Teardrop(t, cx, oy + size * 0.40f, size * 0.10f, new Color(1f, 0.72f, 0.34f, 1f));
                    break;
                case "coin":
                    FillEllipse(t, cx, cy, r, r, c, hollow: false);
                    FillEllipse(t, cx, cy, r * 0.62f, r * 0.62f, new Color(0.78f, 0.6f, 0.28f, 1f), hollow: false);
                    break;
                case "key":
                    FillEllipse(t, cx - r * 0.5f, cy, r * 0.42f, r * 0.42f, c, hollow: true, ringTh: th);
                    Line(t, cx - r * 0.1f, cy, cx + r, cy, th, c);                            // shaft
                    Line(t, cx + r * 0.7f, cy, cx + r * 0.7f, cy - size * 0.14f, th, c);      // tooth
                    Line(t, cx + r, cy, cx + r, cy - size * 0.10f, th, c);                    // tooth
                    break;
                case "teapot":
                    FillEllipse(t, cx, cy - size * 0.04f, r, r * 0.7f, c, hollow: false);
                    Line(t, cx + r * 0.8f, cy, cx + r * 1.3f, cy + size * 0.08f, th, c);      // spout
                    Line(t, cx, cy + r * 0.6f, cx, cy + r * 0.85f, th, c);                    // knob
                    break;
                case "hand":
                    FillRect(t, cx - size * 0.16f, oy + size * 0.20f, size * 0.32f, size * 0.30f, c); // palm
                    for (int f = 0; f < 4; f++)                                              // fingers
                    {
                        float fx = cx - size * 0.135f + f * size * 0.09f;
                        FillRect(t, fx - size * 0.03f, oy + size * 0.48f, size * 0.06f, size * 0.22f, c);
                    }
                    FillRect(t, cx - size * 0.26f, oy + size * 0.30f, size * 0.10f, size * 0.06f, c); // thumb
                    break;
                case "walk":
                    FillEllipse(t, cx, oy + size * 0.74f, size * 0.10f, size * 0.10f, c, hollow: false); // head
                    Line(t, cx, oy + size * 0.64f, cx, oy + size * 0.40f, th, c);            // torso
                    Line(t, cx, oy + size * 0.40f, cx - size * 0.16f, oy + size * 0.18f, th, c); // back leg
                    Line(t, cx, oy + size * 0.40f, cx + size * 0.16f, oy + size * 0.20f, th, c); // front leg
                    Line(t, cx, oy + size * 0.58f, cx + size * 0.16f, oy + size * 0.48f, th * 0.8f, c); // arm
                    break;
                case "question":
                default:
                    // Stylised "?" — an arc + a dot.
                    Arc(t, cx, cy + size * 0.06f, r * 0.7f, 200f, 20f, th, c);
                    Line(t, cx + r * 0.25f, cy + size * 0.02f, cx, cy - size * 0.10f, th, c);
                    FillEllipse(t, cx, oy + size * 0.18f, size * 0.05f, size * 0.05f, c, hollow: false);
                    break;
            }
        }

        // ───── Primitive helpers ─────────────────────────────────────

        private static void Plot(Texture2D t, int x, int y, Color c)
        {
            if (x < 0 || y < 0 || x >= t.width || y >= t.height) return;
            t.SetPixel(x, y, c);
        }

        private static void Line(Texture2D t, float x0, float y0, float x1, float y1, float th, Color c)
        {
            float len = Mathf.Max(1f, Mathf.Sqrt((x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0)));
            int steps = Mathf.CeilToInt(len * 1.5f);
            float hr = th * 0.5f;
            for (int i = 0; i <= steps; i++)
            {
                float u = i / (float)steps;
                float px = Mathf.Lerp(x0, x1, u), py = Mathf.Lerp(y0, y1, u);
                Disc(t, px, py, hr, c);
            }
        }

        private static void Disc(Texture2D t, float cx, float cy, float r, Color c)
        {
            int x0 = Mathf.FloorToInt(cx - r), x1 = Mathf.CeilToInt(cx + r);
            int y0 = Mathf.FloorToInt(cy - r), y1 = Mathf.CeilToInt(cy + r);
            float r2 = r * r;
            for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
                if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= r2) Plot(t, x, y, c);
        }

        private static void FillRect(Texture2D t, float x, float y, float w, float h, Color c)
        {
            for (int yy = Mathf.FloorToInt(y); yy < y + h; yy++)
            for (int xx = Mathf.FloorToInt(x); xx < x + w; xx++)
                Plot(t, xx, yy, c);
        }

        private static void RectOutline(Texture2D t, float x, float y, float w, float h, float th, Color c)
        {
            FillRect(t, x, y, w, th, c);
            FillRect(t, x, y + h - th, w, th, c);
            FillRect(t, x, y, th, h, c);
            FillRect(t, x + w - th, y, th, h, c);
        }

        private static void FillEllipse(Texture2D t, float cx, float cy, float rx, float ry, Color c,
                                        bool hollow, float ringTh = 0f, float rot = 0f)
        {
            float rad = rot * Mathf.Deg2Rad, cs = Mathf.Cos(rad), sn = Mathf.Sin(rad);
            int R = Mathf.CeilToInt(Mathf.Max(rx, ry));
            for (int y = -R; y <= R; y++)
            for (int x = -R; x <= R; x++)
            {
                float xr = x * cs + y * sn, yr = -x * sn + y * cs;
                float d = (xr * xr) / (rx * rx) + (yr * yr) / (ry * ry);
                if (d <= 1f)
                {
                    if (!hollow) Plot(t, Mathf.RoundToInt(cx + x), Mathf.RoundToInt(cy + y), c);
                    else
                    {
                        float inner = 1f - (ringTh / Mathf.Min(rx, ry));
                        if (d >= inner * inner) Plot(t, Mathf.RoundToInt(cx + x), Mathf.RoundToInt(cy + y), c);
                    }
                }
            }
        }

        private static void Teardrop(Texture2D t, float cx, float cy, float r, Color c)
        {
            FillEllipse(t, cx, cy, r, r * 1.4f, c, hollow: false);
        }

        private static void Arc(Texture2D t, float cx, float cy, float r, float startDeg, float endDeg, float th, Color c)
        {
            if (endDeg < startDeg) endDeg += 360f;
            int steps = Mathf.CeilToInt((endDeg - startDeg) * 1.2f);
            for (int i = 0; i <= steps; i++)
            {
                float a = Mathf.Lerp(startDeg, endDeg, i / (float)steps) * Mathf.Deg2Rad;
                Disc(t, cx + Mathf.Cos(a) * r, cy + Mathf.Sin(a) * r, th * 0.5f, c);
            }
        }

        // ───── TMP table population ──────────────────────────────────

        private static void PopulateTables(TMP_SpriteAsset asset, Texture2D tex)
        {
            var glyphTable = new List<TMP_SpriteGlyph>();
            var charTable  = new List<TMP_SpriteCharacter>();

            for (uint i = 0; i < Glyphs.Length; i++)
            {
                int col = (int)i % Cols, row = (int)i / Cols;
                int x = col * Cell;
                int y = (Rows - 1 - row) * Cell;

                var glyph = new TMP_SpriteGlyph
                {
                    index = i,
                    metrics = new GlyphMetrics(Cell, Cell, 4f, Cell - 6f, Cell),
                    glyphRect = new GlyphRect(x + 2, y + 2, Cell - 4, Cell - 4),
                    scale = 1f,
                    sprite = null
                };
                glyphTable.Add(glyph);

                var character = new TMP_SpriteCharacter((uint)Glyphs[i].unicode, glyph)
                {
                    name = Glyphs[i].name,
                    scale = 1f
                };
                charTable.Add(character);
            }

            asset.spriteGlyphTable = glyphTable;
            asset.spriteCharacterTable = charTable;
        }

        // ───── TMP Settings registration (best-effort, version-tolerant) ─────

        private static void RegisterWithTmpSettings(TMP_SpriteAsset asset)
        {
            var settings = TMP_Settings.instance;
            if (settings == null) { Debug.LogWarning("[Phase54] No TMP_Settings; skipping registration."); return; }

            var so = new SerializedObject(settings);
            // Default sprite asset (property name is stable across TMP versions).
            var def = so.FindProperty("m_defaultSpriteAsset");
            if (def != null) def.objectReferenceValue = asset;

            // Emoji fallback list (TMP 3.x). Add if present + not already there.
            var emoji = so.FindProperty("m_EmojiFallbackTextAssets");
            if (emoji != null && emoji.isArray)
            {
                bool present = false;
                for (int i = 0; i < emoji.arraySize; i++)
                    if (emoji.GetArrayElementAtIndex(i).objectReferenceValue == asset) { present = true; break; }
                if (!present)
                {
                    emoji.arraySize += 1;
                    emoji.GetArrayElementAtIndex(emoji.arraySize - 1).objectReferenceValue = asset;
                }
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }
    }
}
