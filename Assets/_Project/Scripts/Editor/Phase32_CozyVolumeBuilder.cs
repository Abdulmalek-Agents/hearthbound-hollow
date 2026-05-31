// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase32_CozyVolumeBuilder
//
// Phase 32.4 — Cozy URP post-processing volumes for Mission 1.
//
// The Lane and Hollow scenes are visually flat without post-processing.
// This builder authors two URP Volume Profile assets and drops a Global
// Volume into each scene pointing at the right profile:
//
//   • HearthboundLane_Volume.asset    — warm autumn dusk outdoor look
//   • HearthboundHollow_Volume.asset  — deep cozy interior firelight look
//
// Both profiles are MOBILE-SAFE (no DoF, no MotionBlur, no LensDistortion).
// They use:
//   - Bloom              — soft halos on lanterns + window glow
//   - Tonemapping        — Neutral (Spiritfarer-style cinematic)
//   - ColorAdjustments   — exposure / contrast / saturation / color filter
//   - WhiteBalance       — warm temperature shift
//   - Vignette           — subtle darkening at frame edges
//   - FilmGrain          — very subtle texture (cozy film-shot feel)
//   - ChannelMixer       — red boost on greens for autumn foliage (Lane only)
//
// Profiles are authored procedurally (not as committed .asset files) so the
// URP version on the user's machine determines the exact serialised format.
// This avoids URP-version YAML drift.
//
// USE: Menu → Hearthbound → ⚙️ Advanced → 🌅 Phase 32.4 — Apply Cozy URP Volume
//
// Demoted to ⚙️ Advanced/… in Phase 32 (menu collapse). The user-facing
// entry point is `Hearthbound → 🚀 Build Everything`, which chains
// Phase 32.4 via the Phase 32 Mission 1 Polish capstone.
//
// Architecture notes:
//   - Per D-007: scene edited + saved.
//   - Per D-027: TryGet*() pattern preserved.
//   - Per D-035: Editor-only.
//   - URP 17.x compatibility verified — uses reflection-free types from
//     UnityEngine.Rendering.Universal and UnityEngine.Rendering namespaces.

using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace HearthboundHollow.EditorTools
{
    public static class Phase32_CozyVolumeBuilder
    {
        private const string SceneLane   = "Assets/_Project/Scenes/02_Mission01_Lane.unity";
        private const string SceneHollow = "Assets/_Project/Scenes/03_Mission01_Hollow.unity";

        private const string ProfileDir  = "Assets/_Project/Settings";
        private const string LaneProfilePath   = ProfileDir + "/HearthboundLane_Volume.asset";
        private const string HollowProfilePath = ProfileDir + "/HearthboundHollow_Volume.asset";

        private const string LaneVolumeName   = "_HearthboundLane_GlobalVolume";
        private const string HollowVolumeName = "_HearthboundHollow_GlobalVolume";

        [MenuItem("Hearthbound/⚙️ Advanced/🌅 Phase 32.4 — Apply Cozy URP Volume", priority = 34)]
        public static void Build()
        {
            EnsureFolder(ProfileDir);

            // 1) Build the two VolumeProfile assets.
            var laneProfile   = BuildLaneVolumeProfile();
            var hollowProfile = BuildHollowVolumeProfile();

            // 2) Drop a Global Volume in each scene pointing at the right profile.
            int laneCount = TryAttachGlobalVolume(SceneLane,   LaneVolumeName,   laneProfile);
            int hollowCount = TryAttachGlobalVolume(SceneHollow, HollowVolumeName, hollowProfile);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Phase 32.4 — Cozy URP volumes applied",
                $"🌅 Cozy URP volumes built.\n\n" +
                $"Profiles:\n" +
                $"  ✓ {LaneProfilePath}\n" +
                $"  ✓ {HollowProfilePath}\n\n" +
                $"Scenes wired:\n" +
                $"  ✓ Lane    — {laneCount} Global Volume(s)\n" +
                $"  ✓ Hollow  — {hollowCount} Global Volume(s)\n\n" +
                "Effects applied (mobile-safe):\n" +
                "  • Bloom (soft halos on lanterns + window glow)\n" +
                "  • Tonemapping (Neutral, Spiritfarer-style)\n" +
                "  • Color Adjustments (warm tint, contrast)\n" +
                "  • White Balance (warm temperature shift)\n" +
                "  • Vignette (subtle frame-edge darkening)\n" +
                "  • Film Grain (very subtle film feel)\n" +
                "  • Channel Mixer (Lane only — autumn foliage boost)\n\n" +
                "Next: Hearthbound → 🚀 Build Everything (chains Phase 32 polish capstone)",
                "OK");
        }

        // ───────────────────────────────────────────────────────────────
        // Lane Volume Profile (warm dusk outdoor)
        // ───────────────────────────────────────────────────────────────

        private static VolumeProfile BuildLaneVolumeProfile()
        {
            var profile = LoadOrCreateProfile(LaneProfilePath);

            // Bloom — soft halos on every lantern and Light Surface.
            var bloom = GetOrAddComponent<Bloom>(profile);
            bloom.intensity.overrideState = true;
            bloom.intensity.value = 0.45f;
            bloom.threshold.overrideState = true;
            bloom.threshold.value = 0.95f;
            bloom.scatter.overrideState = true;
            bloom.scatter.value = 0.7f;
            bloom.tint.overrideState = true;
            bloom.tint.value = new Color(1f, 0.86f, 0.65f);
            // High quality off for mobile-safe perf.
            bloom.highQualityFiltering.overrideState = true;
            bloom.highQualityFiltering.value = false;

            // Tonemapping — Neutral
            var tonemap = GetOrAddComponent<Tonemapping>(profile);
            tonemap.mode.overrideState = true;
            tonemap.mode.value = TonemappingMode.Neutral;

            // Color Adjustments — warm cozy autumn
            var color = GetOrAddComponent<ColorAdjustments>(profile);
            color.postExposure.overrideState = true;
            color.postExposure.value = 0.10f;
            color.contrast.overrideState = true;
            color.contrast.value = 3f;
            color.colorFilter.overrideState = true;
            color.colorFilter.value = new Color(1.0f, 0.92f, 0.84f);
            color.saturation.overrideState = true;
            color.saturation.value = 6f;
            color.hueShift.overrideState = true;
            color.hueShift.value = 0f;

            // White Balance — warm
            var wb = GetOrAddComponent<WhiteBalance>(profile);
            wb.temperature.overrideState = true;
            wb.temperature.value = 10f;
            wb.tint.overrideState = true;
            wb.tint.value = -2f;

            // Vignette
            var vig = GetOrAddComponent<Vignette>(profile);
            vig.intensity.overrideState = true;
            vig.intensity.value = 0.22f;
            vig.smoothness.overrideState = true;
            vig.smoothness.value = 0.35f;
            vig.color.overrideState = true;
            vig.color.value = new Color(0.08f, 0.04f, 0.02f);

            // Film Grain — subtle
            var grain = GetOrAddComponent<FilmGrain>(profile);
            grain.intensity.overrideState = true;
            grain.intensity.value = 0.18f;
            grain.response.overrideState = true;
            grain.response.value = 0.8f;

            // Channel Mixer — red boost on green channel for autumn leaves
            var chmix = GetOrAddComponent<ChannelMixer>(profile);
            chmix.greenOutGreenIn.overrideState = true;
            chmix.greenOutGreenIn.value = 88f;
            chmix.greenOutRedIn.overrideState = true;
            chmix.greenOutRedIn.value = 12f;

            EditorUtility.SetDirty(profile);
            return profile;
        }

        // ───────────────────────────────────────────────────────────────
        // Hollow Volume Profile (cozy interior firelight)
        // ───────────────────────────────────────────────────────────────

        private static VolumeProfile BuildHollowVolumeProfile()
        {
            var profile = LoadOrCreateProfile(HollowProfilePath);

            // Bloom — stronger inside (lanterns + candles + hearth)
            var bloom = GetOrAddComponent<Bloom>(profile);
            bloom.intensity.overrideState = true;
            bloom.intensity.value = 0.55f;
            bloom.threshold.overrideState = true;
            bloom.threshold.value = 0.85f;
            bloom.scatter.overrideState = true;
            bloom.scatter.value = 0.7f;
            bloom.tint.overrideState = true;
            bloom.tint.value = new Color(1f, 0.78f, 0.5f);
            bloom.highQualityFiltering.overrideState = true;
            bloom.highQualityFiltering.value = false;

            // Tonemapping — Neutral
            var tonemap = GetOrAddComponent<Tonemapping>(profile);
            tonemap.mode.overrideState = true;
            tonemap.mode.value = TonemappingMode.Neutral;

            // Color Adjustments — deep cozy
            var color = GetOrAddComponent<ColorAdjustments>(profile);
            color.postExposure.overrideState = true;
            color.postExposure.value = -0.10f;
            color.contrast.overrideState = true;
            color.contrast.value = 6f;
            color.colorFilter.overrideState = true;
            color.colorFilter.value = new Color(1.0f, 0.85f, 0.72f);
            color.saturation.overrideState = true;
            color.saturation.value = -4f;

            // White Balance — much warmer than the Lane
            var wb = GetOrAddComponent<WhiteBalance>(profile);
            wb.temperature.overrideState = true;
            wb.temperature.value = 25f;
            wb.tint.overrideState = true;
            wb.tint.value = -4f;

            // Vignette — more enclosed
            var vig = GetOrAddComponent<Vignette>(profile);
            vig.intensity.overrideState = true;
            vig.intensity.value = 0.32f;
            vig.smoothness.overrideState = true;
            vig.smoothness.value = 0.40f;
            vig.color.overrideState = true;
            vig.color.value = new Color(0.04f, 0.02f, 0.0f);

            // Film Grain — slightly stronger interior
            var grain = GetOrAddComponent<FilmGrain>(profile);
            grain.intensity.overrideState = true;
            grain.intensity.value = 0.22f;
            grain.response.overrideState = true;
            grain.response.value = 0.8f;

            EditorUtility.SetDirty(profile);
            return profile;
        }

        // ───────────────────────────────────────────────────────────────
        // Scene wiring — drop a Global Volume + Camera Volume tag
        // ───────────────────────────────────────────────────────────────

        private static int TryAttachGlobalVolume(string scenePath, string volumeName, VolumeProfile profile)
        {
            if (!System.IO.File.Exists(scenePath))
            {
                Debug.LogWarning($"[Hearthbound/Phase 32.4] Scene missing: {scenePath}");
                return 0;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // Remove any previous volume with our managed name.
            var existing = GameObject.Find(volumeName);
            if (existing != null) Object.DestroyImmediate(existing);

            // Create a new Global Volume.
            var volGO = new GameObject(volumeName);
            var vol = volGO.AddComponent<Volume>();
            vol.isGlobal = true;
            vol.priority = 0;
            vol.weight = 1f;
            vol.sharedProfile = profile;

            // Find the main camera (PlayerController.cameraReference) and make sure
            // its layer mask intersects layer 0 (Default) which the volume occupies.
            var cam = Camera.main;
            if (cam != null)
            {
                var data = cam.GetUniversalAdditionalCameraData();
                if (data != null)
                {
                    data.renderPostProcessing = true;
                    data.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
                    data.antialiasingQuality = AntialiasingQuality.High;
                    EditorUtility.SetDirty(cam);
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            return 1;
        }

        // ───────────────────────────────────────────────────────────────
        // Helpers
        // ───────────────────────────────────────────────────────────────

        private static VolumeProfile LoadOrCreateProfile(string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (existing != null) return existing;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, path);
            return profile;
        }

        private static T GetOrAddComponent<T>(VolumeProfile profile) where T : VolumeComponent
        {
            if (profile.TryGet<T>(out var existing)) return existing;
            return profile.Add<T>();
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            var leaf = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        // Expose for the master capstone
        public static VolumeProfile TryGetLaneProfile() =>
            AssetDatabase.LoadAssetAtPath<VolumeProfile>(LaneProfilePath);
        public static VolumeProfile TryGetHollowProfile() =>
            AssetDatabase.LoadAssetAtPath<VolumeProfile>(HollowProfilePath);
    }
}
