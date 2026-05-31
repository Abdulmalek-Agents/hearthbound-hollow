// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase56_ArabicFontInstaller
//
// PHASE 56 (D-073) — fix Arabic UI rendering as "tofu" boxes.
//
// The body font (LiberationSans SDF) has NO Arabic glyphs, so every Arabic
// label rendered as missing-glyph boxes (QA video). This builder installs an
// Arabic-capable TMP Font Asset and registers it as a FALLBACK on the default
// font + the TMP Settings global fallback list — so Arabic glyphs resolve
// everywhere with zero per-label wiring, while Latin keeps the cozy body font.
//
// Source font priority (no binary committed to the repo):
//   1. Any .ttf/.otf the team drops in  Assets/_Project/Art/Fonts/  (ship this —
//      use an SIL-OFL font: Noto Naskh/Sans Arabic, Amiri, Cairo, Scheherazade).
//   2. Otherwise an OS font that contains Arabic (Tahoma/Arial/Segoe UI/Dubai on
//      Windows, Arial on macOS, Noto on Linux) — copied in for immediate testing.
//      ⚠️ System fonts are generally NOT redistributable; for a shipped build,
//      drop an OFL font in the Fonts folder (option 1) and re-run this.
//
// The TMP asset is DYNAMIC (glyphs rasterise on demand from the source font),
// which is essential for Arabic's large glyph set. Pair with ArabicShaper
// (Core) which joins letters + orders RTL — this builder fixes the glyphs;
// the shaper fixes the shaping/direction.
//
// Idempotent · auto-installs on editor load when missing · fully defensive
// (never breaks Build Everything). Menu: Hearthbound → ⚙️ Advanced → Phase 56.

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using TMPro;

namespace HearthboundHollow.EditorTools
{
    public static class Phase56_ArabicFontInstaller
    {
        private const string FontsDir  = "Assets/_Project/Art/Fonts";
        private const string AssetPath = FontsDir + "/HollowArabic SDF.asset";
        private const string CopyPath  = FontsDir + "/HollowArabic_Source.ttf";
        private const string AssetName = "HollowArabic SDF";

        // Project font names that hint Arabic coverage (preferred over generic).
        private static readonly string[] ArabicHints =
            { "arab", "noto", "amiri", "cairo", "dubai", "scheherazade", "lateef", "tajawal", "almarai", "naskh" };

        [MenuItem("Hearthbound/⚙️ Advanced/Phase 56 — Install Arabic Font (fix tofu)")]
        public static void InstallMenu() { Install(); }

        [InitializeOnLoadMethod]
        private static void AutoInstallIfMissing()
        {
            EditorApplication.delayCall += () =>
            {
                try
                {
                    if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetPath) == null)
                        Install();
                    else
                        Register(AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetPath)); // ensure fallback wired
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[Phase56] Arabic font auto-install skipped (non-fatal): {ex.Message}");
                }
            };
        }

        /// <summary>Idempotent. Returns the Arabic TMP font asset, or null on failure.</summary>
        public static TMP_FontAsset Install()
        {
            try
            {
                if (!AssetDatabase.IsValidFolder(FontsDir))
                {
                    Directory.CreateDirectory(FontsDir);
                    AssetDatabase.Refresh();
                }

                // Already built? Just (re)register and return.
                var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetPath);
                if (existing != null) { Register(existing); return existing; }

                Font source = ResolveSourceFont();
                if (source == null)
                {
                    Debug.LogWarning(
                        "[Phase56] No Arabic-capable font found. Drop an SIL-OFL Arabic .ttf " +
                        "(e.g. NotoNaskhArabic-Regular.ttf, Amiri, Cairo) into " +
                        $"'{FontsDir}/' and re-run Hearthbound → ⚙️ Advanced → Phase 56.");
                    return null;
                }

                var fontAsset = TMP_FontAsset.CreateFontAsset(source); // dynamic SDF
                if (fontAsset == null) { Debug.LogWarning("[Phase56] CreateFontAsset returned null."); return null; }
                fontAsset.name = AssetName;

                AssetDatabase.CreateAsset(fontAsset, AssetPath);

                // Persist the atlas texture(s) + material as sub-assets.
                if (fontAsset.atlasTextures != null)
                {
                    for (int i = 0; i < fontAsset.atlasTextures.Length; i++)
                    {
                        var tex = fontAsset.atlasTextures[i];
                        if (tex == null) continue;
                        tex.name = $"{AssetName} Atlas {i}";
                        if (AssetDatabase.GetAssetPath(tex) == "")
                            AssetDatabase.AddObjectToAsset(tex, fontAsset);
                    }
                }
                if (fontAsset.material != null)
                {
                    fontAsset.material.name = $"{AssetName} Material";
                    if (AssetDatabase.GetAssetPath(fontAsset.material) == "")
                        AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
                }

                EditorUtility.SetDirty(fontAsset);
                AssetDatabase.SaveAssets();

                Register(fontAsset);

                Debug.Log($"[Phase56] Arabic TMP font '{AssetName}' built from '{source.name}' and registered as a fallback. Arabic now renders (no tofu).");
                return fontAsset;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Phase56] Arabic font install failed (non-fatal): {ex.Message}");
                return null;
            }
        }

        // ───── fallback registration ────────────────────────────────

        private static void Register(TMP_FontAsset arabic)
        {
            if (arabic == null) return;

            // Register via SerializedObject on both targets so it works regardless
            // of whether the C# fallback-list properties expose a public setter
            // (TMP hardened several setters to internal). 1) the default body font
            // (Latin keeps its font; Arabic resolves through this fallback), and
            // 2) the TMP Settings global fallback (covers fonts with no own table).
            var def = TMP_Settings.defaultFontAsset;
            if (def != null && AppendToArray(def, "m_FallbackFontAssetTable", arabic))
                EditorUtility.SetDirty(def);

            var settings = TMP_Settings.instance;
            if (settings != null && AppendToArray(settings, "m_fallbackFontAssets", arabic))
                EditorUtility.SetDirty(settings);

            AssetDatabase.SaveAssets();
        }

        // Append an object reference to a serialized array property if absent.
        // Returns true if a change was made; false if the property is missing or
        // the value is already present.
        private static bool AppendToArray(UnityEngine.Object owner, string propName, UnityEngine.Object value)
        {
            var so = new SerializedObject(owner);
            var prop = so.FindProperty(propName);
            if (prop == null || !prop.isArray) return false;
            for (int i = 0; i < prop.arraySize; i++)
                if (prop.GetArrayElementAtIndex(i).objectReferenceValue == value) return false;
            prop.arraySize += 1;
            prop.GetArrayElementAtIndex(prop.arraySize - 1).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }

        // ───── source font resolution ───────────────────────────────

        private static Font ResolveSourceFont()
        {
            // 1) A font already in the project Fonts folder (preferred for shipping).
            var projectFont = FindProjectFont();
            if (projectFont != null) return projectFont;

            // 2) Copy in an OS font that contains Arabic (for immediate testing).
            string osPath = FindOsArabicFontFile();
            if (osPath == null) return null;
            try
            {
                File.Copy(osPath, CopyPath, overwrite: true);
                AssetDatabase.ImportAsset(CopyPath);
                Debug.Log($"[Phase56] Imported OS Arabic font '{Path.GetFileName(osPath)}'. " +
                          "NOTE: system fonts are usually not redistributable — for a shipped build " +
                          $"replace it with an SIL-OFL font in '{FontsDir}/'.");
                return AssetDatabase.LoadAssetAtPath<Font>(CopyPath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Phase56] Could not import OS font '{osPath}': {ex.Message}");
                return null;
            }
        }

        private static Font FindProjectFont()
        {
            if (!Directory.Exists(FontsDir)) return null;
            var files = new List<string>();
            files.AddRange(Directory.GetFiles(FontsDir, "*.ttf"));
            files.AddRange(Directory.GetFiles(FontsDir, "*.otf"));

            // Prefer a file whose name hints Arabic coverage.
            string best = null;
            foreach (var f in files)
            {
                string n = Path.GetFileNameWithoutExtension(f).ToLowerInvariant();
                foreach (var h in ArabicHints)
                    if (n.Contains(h)) { best = f; break; }
                if (best != null) break;
            }
            best ??= (files.Count > 0 ? files[0] : null);
            if (best == null) return null;
            string assetPath = best.Replace("\\", "/");
            return AssetDatabase.LoadAssetAtPath<Font>(assetPath);
        }

        private static string FindOsArabicFontFile()
        {
            var candidates = new List<string>();

            // Windows
            try
            {
                string winFonts = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
                if (!string.IsNullOrEmpty(winFonts))
                    foreach (var f in new[] { "tahoma.ttf", "arial.ttf", "segoeui.ttf", "trado.ttf", "DUBAI-REGULAR.TTF", "times.ttf" })
                        candidates.Add(Path.Combine(winFonts, f));
            }
            catch { }

            // macOS
            candidates.Add("/Library/Fonts/Arial.ttf");
            candidates.Add("/System/Library/Fonts/Supplemental/Arial.ttf");
            candidates.Add("/System/Library/Fonts/Supplemental/Tahoma.ttf");

            // Linux
            candidates.Add("/usr/share/fonts/truetype/noto/NotoNaskhArabic-Regular.ttf");
            candidates.Add("/usr/share/fonts/truetype/noto/NotoSansArabic-Regular.ttf");
            candidates.Add("/usr/share/fonts/truetype/amiri/amiri-regular.ttf");
            candidates.Add("/usr/share/fonts/opentype/amiri/Amiri-Regular.ttf");

            foreach (var c in candidates)
            {
                try { if (File.Exists(c)) return c; } catch { }
            }
            return null;
        }
    }
}
