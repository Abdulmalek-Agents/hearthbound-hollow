// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase16_MemoryOrbMasterBuilder
//
// Phase 16 — MemoryOrb_Master material.
//
// Replaces the Phase 12 URP/Lit fallback on the memory orb with a richer
// AllIn1ShaderNodes-driven material that produces the glass-refraction +
// fresnel glow + emissive clarity look described in Asset_Analysis_Mission1-2
// § 5 S-5 ("the single most important shader pack we own").
//
// USE: Menu → Hearthbound → Phase 16 — Build MemoryOrb_Master Material
//
// Approach: AllIn1ShaderNodes ships pre-built shaders (e.g. "AllIn1Effect").
// Authoring a custom Shader Graph from code is impractical — instead we
// create a Material that uses the AllIn1 shader and wire the properties
// our `MemoryOrbInteractable` already drives:
//   _Clarity          → controlled by Polish mini-game
//   _CrackIntensity   → controlled by Cleanse mini-game
//   _PaletteTint      → set per memory's EmotionalTone
//   _BaseColor        → URP/Lit fallback (still useful for emission)
//   _EmissionColor    → URP/Lit fallback
//
// If the AllIn1 shader isn't found, we fall back to a hand-tuned URP/Lit
// material — same visual quality as Phase 12 but with more glow.

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HearthboundHollow.EditorTools
{
    public static class Phase16_MemoryOrbMasterBuilder
    {
        private const string MaterialsDir = "Assets/_Project/Art/Materials";
        private const string MasterMaterialPath = MaterialsDir + "/MemoryOrb_Master.mat";

        // Candidate AllIn1 shader names — varies by pack version
        private static readonly string[] AllIn1ShaderCandidates =
        {
            "AllIn1Vfx/AllIn1Shader",
            "AllIn1VfxToolkit/AllIn1Shader",
            "AllIn1SpriteShader",
            "AllIn1/AllIn1Shader",
            "AllIn1/AllIn1Effect",
        };

        [MenuItem("Hearthbound/⚙️ Advanced/Phase 16 — Build MemoryOrb_Master Material", priority = 203)]
        public static void Build()
        {
            EnsureFolder(MaterialsDir);

            Shader chosen = null;
            foreach (var name in AllIn1ShaderCandidates)
            {
                var s = Shader.Find(name);
                if (s != null) { chosen = s; break; }
            }

            // Also search the AllIn1 package paths if Shader.Find didn't match.
            if (chosen == null)
            {
                chosen = FindShaderUnderFolder("Assets/Plugins/AllIn1ShaderNodes");
                if (chosen == null) chosen = FindShaderUnderFolder("Assets/Plugins");
            }

            bool isAllIn1 = chosen != null;

            // Fallback: URP/Lit
            if (chosen == null) chosen = Shader.Find("Universal Render Pipeline/Lit");
            if (chosen == null) chosen = Shader.Find("URP/Lit");
            if (chosen == null) chosen = Shader.Find("Standard");
            if (chosen == null)
            {
                EditorUtility.DisplayDialog("Phase 16 — No shader found",
                    "Could not locate any usable shader (AllIn1, URP/Lit, or Standard).\n\n" +
                    "Please import 'All In 1 Shader Nodes' or switch to URP first.",
                    "OK");
                return;
            }

            var existing = AssetDatabase.LoadAssetAtPath<Material>(MasterMaterialPath);
            Material mat = existing ?? new Material(chosen);
            mat.shader = chosen;

            // Apply our orb defaults
            ConfigureOrbDefaults(mat, isAllIn1);

            if (existing == null) AssetDatabase.CreateAsset(mat, MasterMaterialPath);
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[Hearthbound/Phase 16] MemoryOrb_Master.mat → shader = {chosen.name} (isAllIn1={isAllIn1})");

            EditorUtility.DisplayDialog(
                "Phase 16 — Done",
                $"MemoryOrb_Master.mat created at:\n  {MasterMaterialPath}\n\n" +
                $"Shader: {chosen.name}\n" +
                (isAllIn1
                    ? "Using AllIn1 — orb will render with glass refraction + fresnel glow + emissive clarity.\n"
                    : "AllIn1 shader not found — using URP/Lit fallback (still glows via _BaseColor + _EmissionColor).\n") +
                "\nRe-run 'Hearthbound → Build Playable Mission 1 (One Click)' to apply this material " +
                "to the memory orb in the scene.",
                "OK");
        }

        private static Shader FindShaderUnderFolder(string folderPath)
        {
            if (!AssetDatabase.IsValidFolder(folderPath)) return null;
            var guids = AssetDatabase.FindAssets("t:Shader", new[] { folderPath });
            // Prefer shaders whose path / name include "AllIn1"
            Shader best = null;
            int bestScore = 0;
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var sh = AssetDatabase.LoadAssetAtPath<Shader>(path);
                if (sh == null) continue;
                int score = 0;
                if (path.ToLowerInvariant().Contains("allin1")) score += 10;
                if (sh.name.ToLowerInvariant().Contains("allin1")) score += 12;
                if (sh.name.ToLowerInvariant().Contains("effect")) score += 4;
                if (score > bestScore) { best = sh; bestScore = score; }
            }
            return best;
        }

        private static void ConfigureOrbDefaults(Material mat, bool isAllIn1)
        {
            // Hearthbound orb defaults — warm amber, dim base, gentle emission.
            var tint = new Color(1.00f, 0.78f, 0.42f); // Joy tone
            var dim  = new Color(0.45f, 0.42f, 0.36f);

            // URP/Lit standard properties (works on both URP/Lit and as a fallback path on AllIn1 if it inherits)
            if (mat.HasProperty("_BaseColor"))    mat.SetColor("_BaseColor", dim);
            if (mat.HasProperty("_Color"))        mat.SetColor("_Color", dim);
            if (mat.HasProperty("_EmissionColor")) {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", tint * 0.4f);
            }
            if (mat.HasProperty("_Smoothness"))   mat.SetFloat("_Smoothness", 0.85f);
            if (mat.HasProperty("_Metallic"))     mat.SetFloat("_Metallic", 0f);

            // Bespoke properties our MemoryOrbInteractable drives
            if (mat.HasProperty("_Clarity"))         mat.SetFloat("_Clarity", 0.4f);
            if (mat.HasProperty("_CrackIntensity"))  mat.SetFloat("_CrackIntensity", 0f);
            if (mat.HasProperty("_DissolveProgress")) mat.SetFloat("_DissolveProgress", 0f);
            if (mat.HasProperty("_GlowStrength"))    mat.SetFloat("_GlowStrength", 1.4f);
            if (mat.HasProperty("_PaletteTint"))     mat.SetColor("_PaletteTint", tint);

            // AllIn1 commonly exposes these for glass/glow looks
            if (isAllIn1)
            {
                if (mat.HasProperty("_FresnelColor"))    mat.SetColor("_FresnelColor", tint);
                if (mat.HasProperty("_FresnelPower"))    mat.SetFloat("_FresnelPower", 2.2f);
                if (mat.HasProperty("_FresnelStrength")) mat.SetFloat("_FresnelStrength", 0.85f);
                if (mat.HasProperty("_GlowColor"))       mat.SetColor("_GlowColor", tint);
                if (mat.HasProperty("_GlowPower"))       mat.SetFloat("_GlowPower", 1.6f);
                if (mat.HasProperty("_DistortionStrength")) mat.SetFloat("_DistortionStrength", 0.12f);
                if (mat.HasProperty("_RefractionStrength")) mat.SetFloat("_RefractionStrength", 0.18f);

                // Enable common AllIn1 toggles via keywords when present
                TryEnable(mat, "FRESNEL_ON", "_FRESNEL_ON");
                TryEnable(mat, "GLOW_ON", "_GLOW_ON");
                TryEnable(mat, "EMISSION_ON", "_EMISSION_ON");
            }
            mat.enableInstancing = true;
        }

        private static void TryEnable(Material m, params string[] keywordCandidates)
        {
            foreach (var k in keywordCandidates)
            {
                try { m.EnableKeyword(k); } catch { /* ignore */ }
            }
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            var leaf = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        public static Material TryGetOrbMaterial() =>
            AssetDatabase.LoadAssetAtPath<Material>(MasterMaterialPath);
    }
}
