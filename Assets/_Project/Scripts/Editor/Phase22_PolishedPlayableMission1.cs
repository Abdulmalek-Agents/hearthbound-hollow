// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase22_PolishedPlayableMission1
//
// THE CAPSTONE. One menu click that:
//   1. Runs every Phase 13-21 builder in sequence (idempotent — phases skip
//      if their outputs already exist).
//   2. Re-runs HearthboundOneClickSetup with all phase outputs available,
//      so each phase's prefab/binding replaces the Phase 12 primitive.
//   3. Post-processes the scene to wire phase outputs that the original
//      OneClickSetup doesn't know about yet:
//        - Phase 15 dressing: instantiate Medieval Village workbench + door
//        - Phase 16 orb material: apply MemoryOrb_Master.mat to the orb
//        - Phase 17 lighting + Cinemachine: spawn god rays + CM vcam
//        - Phase 18 audio: spawn SfxPlayer + PolishAudioBinder
//        - Phase 19 atmosphere: spawn weather manager + wind zone
//        - Phase 20 Yarn: spawn YarnRunner (if Yarn installed)
//        - Phase 21 dream rig: spawn MemoryDreamRig + hook EveningLedger
//   4. Opens 00_Bootstrap so the user can press Play immediately.
//
// USE: Menu → Hearthbound → 🎮 Build POLISHED Playable Mission 1 (Phase 22)

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using HearthboundHollow.Audio;
using HearthboundHollow.Cutscene;
using HearthboundHollow.MiniGames;
using HearthboundHollow.Mission;
using HearthboundHollow.Player;
using HearthboundHollow.UI;

namespace HearthboundHollow.EditorTools
{
    public static class Phase22_PolishedPlayableMission1
    {
        private const string SceneMission01Hollow = "Assets/_Project/Scenes/03_Mission01_Hollow.unity";
        private const string SceneBootstrap = "Assets/_Project/Scenes/00_Bootstrap.unity";

        [MenuItem("Hearthbound/\ud83c\udfae Build POLISHED Playable Mission 1 (Phase 22)", priority = 0)]
        public static void Build()
        {
            EditorUtility.DisplayProgressBar("Hearthbound · Phase 22", "Running all phase builders…", 0.05f);
            try
            {
                EnsurePrereqsAndRunAllPhases();

                EditorUtility.DisplayProgressBar("Hearthbound · Phase 22", "Building base scenes (OneClickSetup)…", 0.45f);
                HearthboundOneClickSetup.BuildPlayableMission1();

                EditorUtility.DisplayProgressBar("Hearthbound · Phase 22", "Polishing 03_Mission01_Hollow scene…", 0.75f);
                PolishHollowScene();

                EditorUtility.DisplayProgressBar("Hearthbound · Phase 22", "Opening Bootstrap…", 0.95f);
                EditorSceneManager.OpenScene(SceneBootstrap, OpenSceneMode.Single);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            EditorUtility.DisplayDialog(
                "Hearthbound — POLISHED Mission 1 built",
                BuildSummary(),
                "OK");
        }

        // ─── Phase runners ────────────────────────────────────────

        private static void EnsurePrereqsAndRunAllPhases()
        {
            // Prereqs the OneClickSetup also checks, but run them here too
            // so the long-form Phase 22 doesn't choke partway through.
            if (!(GraphicsSettings.defaultRenderPipeline is UniversalRenderPipelineAsset))
                URPSetupHelper.SetupURPPipeline();
            if (SeedAssetGenerator_FirstAssetMissing())
                SeedAssetGenerator.CreateAllSeedAssets();

            // Each phase is idempotent — re-running re-scans and refreshes.
            SafeRun("Phase 13 — BoZo characters",        () => Phase13_BoZoCharacterBuilder.Build());
            SafeRun("Phase 14 — Bamao UI prefabs",       () => Phase14_BamaoUIBuilder.Build());
            SafeRun("Phase 15 — Medieval Village",       () => Phase15_MedievalVillageBuilder.Build());
            SafeRun("Phase 16 — MemoryOrb_Master mat",   () => Phase16_MemoryOrbMasterBuilder.Build());
            SafeRun("Phase 17 — Lumen + Cinemachine",    () => Phase17_LumenAndCinemachineBuilder.Build());
            SafeRun("Phase 18 — SFX library",            () => Phase18_AudioBuilder.Build());
            SafeRun("Phase 19 — Weather + Zephyr",       () => Phase19_WeatherAndWindBuilder.Build());
            // Phase 20 Yarn install is interactive — skip the install dialog inside the capstone;
            // if Yarn isn't already there, the scene falls back to Mission01Director.
            if (Phase20_YarnSpinnerBuilder.IsYarnSpinnerAvailable())
                SafeRun("Phase 20 — Yarn runner",        () => Phase20_YarnSpinnerBuilder.Build());
            SafeRun("Phase 21 — Memory Dream cutscene",  () => Phase21_MemoryDreamCutsceneBuilder.Build());
        }

        private static void SafeRun(string label, System.Action action)
        {
            try { action(); }
            catch (System.Exception e)
            {
                Debug.LogError($"[Hearthbound/Phase 22] {label} failed: {e}");
            }
        }

        private static bool SeedAssetGenerator_FirstAssetMissing()
        {
            return AssetDatabase.LoadAssetAtPath<HearthboundHollow.Core.VillageState>(
                "Assets/_Project/ScriptableObjects/State/VillageState.asset") == null;
        }

        // ─── Scene polishing ──────────────────────────────────────

        private static void PolishHollowScene()
        {
            var scene = EditorSceneManager.OpenScene(SceneMission01Hollow, OpenSceneMode.Single);
            var roots = scene.GetRootGameObjects();

            ApplyPhase16OrbMaterial(roots);
            ApplyPhase17LightingAndCamera(roots);
            ApplyPhase18Audio(roots);
            ApplyPhase19WeatherAndWind(scene);
            ApplyPhase15MedievalVillageDressing(scene);
            ApplyPhase20YarnRunner(scene);
            ApplyPhase21MemoryDream(scene);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        // ─── Phase 16: orb material ───────────────────────────────

        private static void ApplyPhase16OrbMaterial(GameObject[] roots)
        {
            var mat = Phase16_MemoryOrbMasterBuilder.TryGetOrbMaterial();
            if (mat == null) return;
            foreach (var r in roots)
            {
                foreach (var orb in r.GetComponentsInChildren<MemoryOrbInteractable>(true))
                {
                    if (orb.orbRenderer == null) continue;
                    orb.orbRenderer.sharedMaterial = mat;
                    Debug.Log($"[Hearthbound/Phase 22] Applied MemoryOrb_Master.mat to {orb.name}.");
                }
            }
        }

        // ─── Phase 17: Lumen + Cinemachine ────────────────────────

        private static void ApplyPhase17LightingAndCamera(GameObject[] roots)
        {
            var lighting = Phase17_LumenAndCinemachineBuilder.TryGetLightingBindings();
            if (lighting != null)
            {
                if (lighting.godRayPrefab != null)
                {
                    var rays = (GameObject)PrefabUtility.InstantiatePrefab(lighting.godRayPrefab);
                    rays.transform.position = new Vector3(3.5f, 3.2f, 2.0f);
                    rays.transform.rotation = Quaternion.Euler(35, -45, 0);
                    Debug.Log($"[Hearthbound/Phase 22] Spawned Lumen god rays: {lighting.godRayPrefab.name}");
                }
                if (lighting.candleGlowPrefab != null)
                {
                    // Place a couple on the shelves
                    var c1 = (GameObject)PrefabUtility.InstantiatePrefab(lighting.candleGlowPrefab);
                    c1.transform.position = new Vector3(4f, 1.55f, 3f);
                    var c2 = (GameObject)PrefabUtility.InstantiatePrefab(lighting.candleGlowPrefab);
                    c2.transform.position = new Vector3(2.4f, 1.4f, 2.6f);
                    Debug.Log($"[Hearthbound/Phase 22] Spawned 2 Lumen candle glows.");
                }
                if (lighting.lanternHaloPrefab != null)
                {
                    var lantern = (GameObject)PrefabUtility.InstantiatePrefab(lighting.lanternHaloPrefab);
                    lantern.transform.position = new Vector3(0f, 2.4f, 7.5f);
                    Debug.Log($"[Hearthbound/Phase 22] Spawned Lumen lantern halo.");
                }
            }

            // Cinemachine vcam
            var cmPrefab = Phase17_LumenAndCinemachineBuilder.TryGetCinemachineFollowPrefab();
            var player = FindObjectByTagSafe("Player");
            if (cmPrefab != null && player != null)
            {
                var vcam = (GameObject)PrefabUtility.InstantiatePrefab(cmPrefab);
                vcam.name = "CM_PlayerFollow";

                // Use reflection to set Follow + LookAt to the player.
                foreach (var c in vcam.GetComponents<Component>())
                {
                    if (c == null) continue;
                    var t = c.GetType();
                    if (t.FullName?.Contains("Cinemachine") != true) continue;
                    TrySet(c, "Follow", player.transform);
                    TrySet(c, "LookAt", player.transform);
                    TrySet(c, "m_Follow", player.transform);
                    TrySet(c, "m_LookAt", player.transform);
                }
                Debug.Log($"[Hearthbound/Phase 22] Spawned Cinemachine vcam targeting Player.");
            }
        }

        // ─── Phase 18: audio ──────────────────────────────────────

        private static void ApplyPhase18Audio(GameObject[] roots)
        {
            var lib = Phase18_AudioBuilder.TryGetLibrary();
            if (lib == null) return;

            // SfxPlayer on a fresh GameObject
            var sfxGO = new GameObject("_SfxPlayer");
            var player = sfxGO.AddComponent<SfxPlayer>();
            player.library = lib;

            // PolishAudioBinder on the PolishMiniGame object
            foreach (var r in roots)
            {
                foreach (var pmg in r.GetComponentsInChildren<PolishMiniGame>(true))
                {
                    if (pmg.GetComponent<PolishAudioBinder>() == null)
                        pmg.gameObject.AddComponent<PolishAudioBinder>();
                }
            }
            Debug.Log("[Hearthbound/Phase 22] SfxPlayer + PolishAudioBinder wired.");
        }

        // ─── Phase 19: weather + wind ─────────────────────────────

        private static void ApplyPhase19WeatherAndWind(UnityEngine.SceneManagement.Scene scene)
        {
            var bindings = Phase19_WeatherAndWindBuilder.TryGetBindings();
            if (bindings == null) return;

            if (bindings.stylizedWeatherManagerPrefab != null)
            {
                var wm = (GameObject)PrefabUtility.InstantiatePrefab(bindings.stylizedWeatherManagerPrefab);
                wm.name = "_StylizedWeather";
                Debug.Log($"[Hearthbound/Phase 22] Spawned Stylized Weather: {bindings.stylizedWeatherManagerPrefab.name}");
            }
            if (bindings.zephyrWindZonePrefab != null)
            {
                var wz = (GameObject)PrefabUtility.InstantiatePrefab(bindings.zephyrWindZonePrefab);
                wz.name = "_ZephyrWindZone";
                Debug.Log($"[Hearthbound/Phase 22] Spawned Zephyr wind zone: {bindings.zephyrWindZonePrefab.name}");
            }
        }

        // ─── Phase 15: Medieval Village dressing ──────────────────

        private static void ApplyPhase15MedievalVillageDressing(UnityEngine.SceneManagement.Scene scene)
        {
            var mv = Phase15_MedievalVillageBuilder.TryGetBindings();
            if (mv == null) return;

            // Replace the placeholder cube workbench with a real one.
            var oldBench = GameObject.Find("Workbench");
            if (mv.workbenchPrefab != null && oldBench != null)
            {
                var pos = oldBench.transform.position;
                var newBench = (GameObject)PrefabUtility.InstantiatePrefab(mv.workbenchPrefab);
                newBench.transform.position = pos;
                newBench.name = "Workbench";

                // Re-parent the existing Workbench_Approach_Zone trigger onto the new bench.
                var approach = oldBench.transform.Find("Workbench_Approach_Zone");
                if (approach != null) approach.SetParent(newBench.transform, worldPositionStays: true);

                Object.DestroyImmediate(oldBench);
                Debug.Log($"[Hearthbound/Phase 22] Workbench → Medieval Village: {mv.workbenchPrefab.name}");
            }

            // Sprinkle environment decoration (one cottage, a few trees, a fence segment, a lamp).
            if (mv.cottagePrefab != null) Place(mv.cottagePrefab, new Vector3(-12, 0, 8),  Quaternion.Euler(0, 12, 0));
            if (mv.fencePrefab   != null) Place(mv.fencePrefab,   new Vector3(-6, 0, -8), Quaternion.identity);
            if (mv.fencePrefab   != null) Place(mv.fencePrefab,   new Vector3( 6, 0, -8), Quaternion.identity);
            if (mv.treePrefab    != null) Place(mv.treePrefab,    new Vector3(-8, 0, 6),  Quaternion.identity);
            if (mv.treePrefab    != null) Place(mv.treePrefab,    new Vector3(10, 0, -2), Quaternion.identity);
            if (mv.wellPrefab    != null) Place(mv.wellPrefab,    new Vector3(-2, 0, -6), Quaternion.identity);
            if (mv.lampPostPrefab != null) Place(mv.lampPostPrefab, new Vector3(0, 0, -3), Quaternion.identity);
        }

        private static GameObject Place(GameObject prefab, Vector3 pos, Quaternion rot)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.transform.position = pos;
            go.transform.rotation = rot;
            return go;
        }

        // ─── Phase 20: Yarn runner ────────────────────────────────

        private static void ApplyPhase20YarnRunner(UnityEngine.SceneManagement.Scene scene)
        {
            var prefab = Phase20_YarnSpinnerBuilder.TryGetYarnRunnerPrefab();
            if (prefab == null) return;
            var yr = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            yr.name = "_YarnRunner";
            Debug.Log("[Hearthbound/Phase 22] Spawned YarnRunner — dialogue will run via Yarn Spinner.");
        }

        // ─── Phase 21: memory dream ───────────────────────────────

        private static void ApplyPhase21MemoryDream(UnityEngine.SceneManagement.Scene scene)
        {
            var prefab = Phase21_MemoryDreamCutsceneBuilder.TryGetDreamRigPrefab();
            if (prefab == null) return;
            var rig = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            rig.name = "_MemoryDreamRig";

            // Hook EveningLedger.OnEndOfDayConfirmed → MemoryDreamSequencer.PlayDream1
            // The Mission01Director already listens to that event; we add a second
            // listener so the dream plays before the scene transition.
            //
            // DreamHook is the runtime class (HearthboundHollow.Mission.DreamHook).
            // It was previously a private nested class here, but the Editor asmdef
            // is includePlatforms=["Editor"], which made the component reference
            // unresolvable at runtime → Dream cutscene silently never played.
            var ledger = Object.FindAnyObjectByType<EveningLedgerUI>();
            var seq = rig.GetComponent<MemoryDreamSequencer>();
            if (ledger != null && seq != null)
            {
                var bridgeGO = new GameObject("_DreamHook");
                var bridge = bridgeGO.AddComponent<DreamHook>();
                bridge.ledger = ledger;
                bridge.sequencer = seq;
                Debug.Log("[Hearthbound/Phase 22] Memory Dream hook wired to EveningLedger.");
            }
        }

        // ─── Reflection helpers ───────────────────────────────────

        private static void TrySet(object obj, string member, object value)
        {
            if (obj == null) return;
            var t = obj.GetType();
            var f = t.GetField(member, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (f != null) { try { f.SetValue(obj, value); return; } catch { } }
            var p = t.GetProperty(member, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (p != null && p.CanWrite) { try { p.SetValue(obj, value); } catch { } }
        }

        private static GameObject FindObjectByTagSafe(string tag)
        {
            try { return GameObject.FindGameObjectWithTag(tag); }
            catch { return null; }
        }

        // ─── Summary builder ──────────────────────────────────────

        private static string BuildSummary()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Polished Playable Mission 1 — phase status:\n");
            sb.AppendLine(LineFor("Phase 13 — BoZo characters",          Phase13_BoZoCharacterBuilder.TryGetPlayerPrefab() != null));
            sb.AppendLine(LineFor("Phase 14 — Bamao parchment UI",       Phase14_BamaoUIBuilder.TryGetDialogueBoxPrefab() != null));
            var mv = Phase15_MedievalVillageBuilder.TryGetBindings();
            sb.AppendLine(LineFor("Phase 15 — Medieval Village dressing", mv != null && mv.workbenchPrefab != null));
            sb.AppendLine(LineFor("Phase 16 — MemoryOrb_Master material", Phase16_MemoryOrbMasterBuilder.TryGetOrbMaterial() != null));
            var lighting = Phase17_LumenAndCinemachineBuilder.TryGetLightingBindings();
            sb.AppendLine(LineFor("Phase 17 — Lumen + Cinemachine",      lighting != null && lighting.godRayPrefab != null));
            sb.AppendLine(LineFor("Phase 18 — SFX library",              Phase18_AudioBuilder.TryGetLibrary() != null));
            var weather = Phase19_WeatherAndWindBuilder.TryGetBindings();
            sb.AppendLine(LineFor("Phase 19 — Weather + Zephyr",         weather != null && weather.stylizedWeatherManagerPrefab != null));
            sb.AppendLine(LineFor("Phase 20 — Yarn Spinner",             Phase20_YarnSpinnerBuilder.TryGetYarnRunnerPrefab() != null));
            sb.AppendLine(LineFor("Phase 21 — Memory Dream cutscene",    Phase21_MemoryDreamCutsceneBuilder.TryGetDreamRigPrefab() != null));
            sb.AppendLine();
            sb.AppendLine("Press Play. Mission 1 should now feature:");
            sb.AppendLine("  • Real BoZo character art for the player and Doris");
            sb.AppendLine("  • Bamao parchment dialogue box + open-book Evening Ledger");
            sb.AppendLine("  • Medieval Village workbench, cottage, fence, tree, well, lamp post");
            sb.AppendLine("  • Glass-refraction memory orb (AllIn1 master material)");
            sb.AppendLine("  • Lumen god rays + candle glows + lantern halo");
            sb.AppendLine("  • Cinemachine third-person follow camera");
            sb.AppendLine("  • Full polish audio (9 cues) + ambient + UI clicks");
            sb.AppendLine("  • Stylized Weather System (light evening fog)");
            sb.AppendLine("  • Zephyr wind on foliage");
            sb.AppendLine("  • Yarn Spinner dialogue (if installed)");
            sb.AppendLine("  • Memory Dream cutscene at End-of-Day before MainMenu");
            return sb.ToString();
        }

        private static string LineFor(string label, bool ok) => (ok ? "  ✓ " : "  ⚠ ") + label;
    }
}
