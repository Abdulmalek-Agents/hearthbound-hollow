// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / HHGroundMaterials
//
// PHASE 71 — shared "cozy ground material" helper for the environment
// enrichment builders (Phase 62 Garden, Phase 63 World Polish).
//
// Player report on the latest build: "green garden not available". Root cause:
// both builders did a blind `AssetDatabase.FindAssets("t:Material grass")` and
// returned the FIRST hit — which, in the Medieval Village pack, can be the
// *dried* (brown) grass material (`M_DriedGrass_Fo_01a`) — or, failing that,
// fell back to a flat olive colour. Either way the meadow read as dead/brown,
// not green.
//
// FIX: build a fresh URP Lit material backed by a known GREEN grass *diffuse*
// texture (`T_Grass_Fo_01a_D`, the albedo map — not the `_OR`/`_ORS` roughness
// maps, and never the `Dried` variant) with sensible tiling, and only ever
// fall back to a *lush green* colour (never brown) for the garden. Earth
// grounds (the Lane) get a warm soil texture / colour. Pure editor-time; the
// material is assigned by the calling builder and saved into the scene, so it
// applies on the next 🚀 Build Everything.
//
// SOLID: single responsibility (ground-material selection), no scene side
// effects, no dependency on either builder — both call the same entry point so
// the two grounds can never drift apart again.

using UnityEditor;
using UnityEngine;

namespace HearthboundHollow.EditorTools
{
    public static class HHGroundMaterials
    {
        // Packs we are allowed to mine textures from (keeps the scan fast and
        // avoids pulling a _Project art asset by accident).
        private static readonly string[] PackRoots =
        {
            "Assets/MeshingunStudio",
            "Assets/Waldemarst",
            "Assets/Unluck Software",
        };

        /// <summary>
        /// Build a cozy ground material. <paramref name="kind"/> "garden"/"meadow"
        /// → lush green grass; "lane"/"earth"/"dirt" → warm soil. Never returns a
        /// brown/dried meadow.
        /// </summary>
        public static Material MakeCozyGround(string kind)
        {
            bool earth = kind == "lane" || kind == "earth" || kind == "dirt";
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader) { name = "HH_Ground_" + kind };

            // Garden/meadow: use flat colour only — foliage-card textures (T_Grass_Fo_*)
            // tile badly as large ground planes and were producing the tiled-grass-blade grid
            // artifact. Lane/earth paths can use a soil texture since they tile more naturally.
            Texture2D tex = null;
            if (earth)
            {
                tex = FindGroundTexture(
                    prefer:  new[] { "groundpatch", "t_ground", "ground", "dirt", "soil", "path", "cobble" },
                    exclude: new[] { "grass", "snow", "_n.", "_normal", "_or", "ors", "mask", "rough", "metal", "height", "_ao", "_fo_", "_fo.", "foliage" });
            }
            // For garden/meadow we intentionally leave tex = null → solid flat colour
            // which looks clean and cozy instead of a tiled foliage-card grid.

            // Flat colours: earth = warm soil, garden = rich lush green (#385723-ish).
            var baseTint  = earth ? new Color(0.55f, 0.46f, 0.33f)   // warm soil
                                  : new Color(0.80f, 0.88f, 0.68f);  // unused for garden
            var flatColor = earth ? new Color(0.30f, 0.25f, 0.18f)   // warm earth
                                  : new Color(0.22f, 0.34f, 0.14f);  // #385722 earthy olive-green

            if (tex != null)
            {
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
                float tile = 8f;
                mat.mainTextureScale = new Vector2(tile, tile);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseTint);
                if (mat.HasProperty("_Color"))     mat.SetColor("_Color", baseTint);
            }
            else
            {
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", flatColor);
                if (mat.HasProperty("_Color"))     mat.SetColor("_Color", flatColor);
            }

            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.04f);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.04f);
            return mat;
        }

        /// <summary>
        /// Score textures in the vendored packs by name, preferring albedo/diffuse
        /// ("_D") maps and excluding normal/roughness/mask maps and the dried
        /// variants. Returns the best match or null.
        /// </summary>
        private static Texture2D FindGroundTexture(string[] prefer, string[] exclude)
        {
            Texture2D best = null;
            int bestScore = 0;
            foreach (var g in AssetDatabase.FindAssets("t:Texture2D", PackRoots))
            {
                var path  = AssetDatabase.GUIDToAssetPath(g);
                var lower = path.ToLowerInvariant();

                bool skip = false;
                foreach (var ex in exclude) if (lower.Contains(ex)) { skip = true; break; }
                if (skip) continue;

                int score = 0;
                for (int i = 0; i < prefer.Length; i++)
                    if (lower.Contains(prefer[i])) score += (prefer.Length - i) * 10;
                if (score <= 0) continue;

                // Strongly prefer the colour/albedo map over ambiguous siblings.
                if (lower.Contains("_d.") || lower.Contains("_basecolor") ||
                    lower.Contains("albedo") || lower.Contains("diffuse")) score += 30;

                if (score > bestScore)
                {
                    var t = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    if (t != null) { best = t; bestScore = score; }
                }
            }
            return best;
        }
    }
}
