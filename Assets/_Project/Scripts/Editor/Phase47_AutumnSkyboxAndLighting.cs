// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase47_AutumnSkyboxAndLighting
//
// Phase 47.1 — Autumn-evening procedural skybox + sun + ambient lighting.
//
// Authors a single skybox material `HearthboundAutumnSky.mat` using Unity's
// built-in `Skybox/Procedural` shader, tuned for the autumn-evening cozy
// register (warm amber zenith, peach horizon, low warm sun, deep blue
// upper sky). The material is assigned to RenderSettings.skybox of all
// four gameplay scenes, plus the sun directional light is bound, and
// ambient/reflection lighting is configured to a warm gradient.
//
// Why a procedural skybox instead of a cubemap:
//   • Procedural is mobile-cheap (no texture memory).
//   • Procedural respects the scene's Sun direction so we can angle the
//     sun differently in each scene (Lane = west-low dusk;
//     Garden = high noon; Cottage = west afternoon).
//   • Procedural integrates cleanly with URP's environment lighting.
//
// USE: Menu → Hearthbound → ⚙️ Advanced → 🌇 Phase 47.1 — Autumn Skybox + Lighting
//
// Architecture notes:
//   - Per D-007: scenes edited + saved.
//   - Per D-035: Editor-only.
//   - Mobile-safe: no cubemap, no HDR cubemap import.

using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace HearthboundHollow.EditorTools
{
    public static class Phase47_AutumnSkyboxAndLighting
    {
        private const string SkyDir     = "Assets/_Project/Art/Sky";
        private const string SkyMatPath = SkyDir + "/HearthboundAutumnSky.mat";

        private static readonly (string scene, float sunPitch, float sunYaw, float sunIntensity, Color sunTint, string label)[] SceneSunConfig =
        {
            ("Assets/_Project/Scenes/02_Mission01_Lane.unity",     22f, 230f, 1.35f, new Color(1.00f, 0.83f, 0.62f), "Lane — autumn dusk, low west-south-west"),
            ("Assets/_Project/Scenes/03_Mission01_Hollow.unity",   22f, 230f, 0.80f, new Color(0.95f, 0.80f, 0.60f), "Hollow — interior, dimmer sun through windows"),
            ("Assets/_Project/Scenes/04_Mission02_Garden.unity",   55f, 145f, 1.55f, new Color(1.00f, 0.95f, 0.85f), "Garden — Day 2 mid-morning bright"),
            ("Assets/_Project/Scenes/05_Mission02_Cottage.unity",  38f, 250f, 1.10f, new Color(1.00f, 0.86f, 0.66f), "Cottage — early afternoon, west window light"),
        };

        [MenuItem("Hearthbound/⚙️ Advanced/🌇 Phase 47.1 — Autumn Skybox + Lighting", priority = 471)]
        public static void Build()
        {
            EnsureFolder(SkyDir);

            // 1) Author the procedural skybox material.
            var skyMat = BuildOrLoadSkyMaterial();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 2) Apply to every gameplay scene.
            int applied = 0, sceneCount = 0;
            foreach (var cfg in SceneSunConfig)
            {
                if (!File.Exists(cfg.scene))
                {
                    Debug.LogWarning($"[Hearthbound/Phase 47.1] Scene not found: {cfg.scene} — skipping.");
                    continue;
                }
                sceneCount++;
                if (ApplyToScene(cfg.scene, skyMat, cfg.sunPitch, cfg.sunYaw, cfg.sunIntensity, cfg.sunTint, cfg.label))
                    applied++;
            }

            EditorUtility.DisplayDialog("Phase 47.1 — Skybox + Lighting",
                $"🌇 Autumn-evening skybox applied.\n\n" +
                $"Material: {SkyMatPath}\n" +
                $"Scenes wired: {applied} / {sceneCount}\n\n" +
                "Each scene gets:\n" +
                "  • Procedural autumn sky as RenderSettings.skybox\n" +
                "  • Sun directional light bound + tuned (intensity / tint / angle)\n" +
                "  • Ambient lighting set to Trilight (sky / equator / ground)\n" +
                "  • Reflection intensity tuned for cozy interiors / outdoors\n\n" +
                "Re-run anytime — idempotent.",
                "OK");
        }

        // ───────────────────────────────────────────────────────────
        // Skybox material authoring
        // ───────────────────────────────────────────────────────────

        private static Material BuildOrLoadSkyMaterial()
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>(SkyMatPath);
            if (existing != null) return TuneSkyMaterial(existing);

            // Unity's built-in procedural skybox shader is the standard
            // mobile-friendly choice.
            var shader = Shader.Find("Skybox/Procedural");
            if (shader == null)
            {
                Debug.LogError("[Hearthbound/Phase 47.1] 'Skybox/Procedural' shader not found in project. " +
                               "Falling back to default skybox.");
                return null;
            }

            var mat = new Material(shader) { name = "HearthboundAutumnSky" };
            AssetDatabase.CreateAsset(mat, SkyMatPath);
            return TuneSkyMaterial(mat);
        }

        /// <summary>
        /// Tunes the procedural-skybox properties to the autumn-evening palette.
        /// See Unity docs for `Skybox/Procedural` property names.
        /// </summary>
        private static Material TuneSkyMaterial(Material mat)
        {
            // _SunSize             — visible sun disk radius
            // _SunSizeConvergence  — sun disk falloff
            // _AtmosphereThickness — adds peach/orange haze at horizon
            // _SkyTint             — overall sky colour shift (warm peach for autumn)
            // _GroundColor         — colour of the lower hemisphere
            // _Exposure            — overall brightness multiplier

            if (mat.HasProperty("_SunSize"))             mat.SetFloat("_SunSize",             0.06f);
            if (mat.HasProperty("_SunSizeConvergence"))  mat.SetFloat("_SunSizeConvergence",  6.0f);
            if (mat.HasProperty("_AtmosphereThickness")) mat.SetFloat("_AtmosphereThickness", 1.35f);
            if (mat.HasProperty("_SkyTint"))             mat.SetColor("_SkyTint",
                new Color(0.95f, 0.78f, 0.62f, 1f));     // warm peach
            if (mat.HasProperty("_GroundColor"))         mat.SetColor("_GroundColor",
                new Color(0.30f, 0.22f, 0.16f, 1f));     // warm earth brown
            if (mat.HasProperty("_Exposure"))            mat.SetFloat("_Exposure", 1.10f);

            EditorUtility.SetDirty(mat);
            return mat;
        }

        // ───────────────────────────────────────────────────────────
        // Per-scene application
        // ───────────────────────────────────────────────────────────

        private static bool ApplyToScene(string scenePath, Material sky, float pitch, float yaw, float intensity, Color sunColor, string label)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // 1) Skybox material
            if (sky != null) RenderSettings.skybox = sky;

            // 2) Sun directional light — find or create.
            var sun = FindDirectionalLight();
            if (sun == null)
            {
                var sunGO = new GameObject("Directional Light");
                sun = sunGO.AddComponent<Light>();
                sun.type = LightType.Directional;
                Debug.Log($"[Hearthbound/Phase 47.1] Created Directional Light in {scene.name}.");
            }

            sun.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
            sun.color = sunColor;
            sun.intensity = intensity;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.65f;
            sun.shadowBias = 0.05f;
            sun.shadowNormalBias = 0.4f;

            // Bind RenderSettings.sun to this light so the procedural skybox follows it.
            RenderSettings.sun = sun;

            // 3) Ambient lighting — trilight gradient
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor     = new Color(0.95f, 0.78f, 0.62f, 1f); // warm peach top
            RenderSettings.ambientEquatorColor = new Color(0.66f, 0.55f, 0.46f, 1f); // mid-warm
            RenderSettings.ambientGroundColor  = new Color(0.30f, 0.25f, 0.20f, 1f); // earth brown
            RenderSettings.ambientIntensity    = 1.0f;

            // 4) Fog — light warm fog for the lane / outdoor scenes only.
            bool isOutdoor = scenePath.Contains("Lane") || scenePath.Contains("Garden");
            RenderSettings.fog              = isOutdoor;
            RenderSettings.fogMode          = FogMode.ExponentialSquared;
            RenderSettings.fogDensity       = isOutdoor ? 0.012f : 0f;
            RenderSettings.fogColor         = new Color(0.78f, 0.66f, 0.55f, 1f);

            // 5) Reflection intensity — softer for interiors so candle warmth dominates.
            RenderSettings.reflectionIntensity = scenePath.Contains("Hollow") || scenePath.Contains("Cottage")
                ? 0.4f
                : 0.85f;
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;

            // 6) Halo strength — cozy bloom-friendly
            RenderSettings.flareStrength = 0.7f;
            RenderSettings.haloStrength  = 0.6f;

            Debug.Log($"[Hearthbound/Phase 47.1] ✓ {label}");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            return true;
        }

        // ───────────────────────────────────────────────────────────
        // Helpers
        // ───────────────────────────────────────────────────────────

        private static Light FindDirectionalLight()
        {
#if UNITY_2023_1_OR_NEWER
            var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
#else
            var lights = Object.FindObjectsOfType<Light>();
#endif
            foreach (var l in lights)
                if (l.type == LightType.Directional) return l;
            return null;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parts = path.Split('/');
            string cur = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = cur + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(cur, parts[i]);
                cur = next;
            }
        }
    }
}
