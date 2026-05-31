// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase27_EnvironmentCapstone
//
// Phase 27.4 — the orchestrator. One menu click that:
//   1. Runs Phase 27.2 (Lane environment).
//   2. Runs Phase 27.3 (Hollow interior).
//   3. Wires the runtime LanternInteractable onto every lantern the
//      two builders produced (DoorLantern, HollowLantern, EastWallLantern).
//   4. Wires HearthAmbianceTrigger onto the Hearth_AmbianceZone trigger
//      in the Hollow + spawns the AudioSource for the hearth crackle loop.
//   5. Adds the Marin's Note hook (re-runs Phase 26) so Marin's parchment
//      survives the rebuild.
//
// USE: Menu → Hearthbound → 🌳 Phase 27 — Polish Mission 1 Environment

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using HearthboundHollow.Player;

namespace HearthboundHollow.EditorTools
{
    public static class Phase27_EnvironmentCapstone
    {
        private const string SceneLane = "Assets/_Project/Scenes/02_Mission01_Lane.unity";
        private const string SceneHollow = "Assets/_Project/Scenes/03_Mission01_Hollow.unity";

        [MenuItem("Hearthbound/⚙️ Advanced/🌳 Phase 27 — Polish Mission 1 Environment", priority = 4)]
        public static void Build()
        {
            EditorUtility.DisplayProgressBar("Hearthbound · Phase 27", "Running Phase 27.2 (Lane)…", 0.10f);
            try
            {
                Phase27_LaneEnvironment.Build();

                EditorUtility.DisplayProgressBar("Hearthbound · Phase 27", "Running Phase 27.3 (Hollow)…", 0.40f);
                Phase27_HollowInterior.Build();

                EditorUtility.DisplayProgressBar("Hearthbound · Phase 27", "Wiring lanterns (Lane)…", 0.60f);
                WireLanternsInLane();

                EditorUtility.DisplayProgressBar("Hearthbound · Phase 27", "Wiring lanterns + hearth (Hollow)…", 0.75f);
                WireInteractablesInHollow();

                EditorUtility.DisplayProgressBar("Hearthbound · Phase 27", "Re-running Phase 26 narrative hooks…", 0.90f);
                SafeRun("Phase 26 — Wire Narrative Hooks", () => Phase26_NarrativeHooks.Wire());

                EditorUtility.DisplayProgressBar("Hearthbound · Phase 27", "Done.", 1f);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            EditorUtility.DisplayDialog("Phase 27 — Mission 1 environment polished",
                "✨ Mission 1 environment is fully dressed.\n\n" +
                "Lane:\n" +
                "  • 3 cottages, cobble path, 5+ trees, bushes, rocks, grass\n" +
                "  • Fence, well, bench, log pile, basket, village sign\n" +
                "  • Hanging lantern at the door (interact with E)\n" +
                "  • Falling leaves + soft fog + Lumen sunshafts\n\n" +
                "Hollow:\n" +
                "  • Wood plank floor, 4 walls, ceiling beams\n" +
                "  • Hearth with crossfade ambience (walk past to feel it)\n" +
                "  • Shelves with pots + baskets + candle stubs\n" +
                "  • Hanging lantern + east-wall lantern (both interactable)\n" +
                "  • Wool rug, sack pile, crate, banner, window glow\n\n" +
                "Press Play and explore.",
                "OK");
        }

        private static void WireLanternsInLane()
        {
            var scene = EditorSceneManager.OpenScene(SceneLane, OpenSceneMode.Single);

            int wired = 0;
            wired += TryWireLantern("DoorLantern", "LANTERN_DOOR_HOLLOW");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[Hearthbound/Phase 27.4] Lane: wired {wired} lantern interactable(s).");
        }

        private static void WireInteractablesInHollow()
        {
            var scene = EditorSceneManager.OpenScene(SceneHollow, OpenSceneMode.Single);

            int wiredLanterns = 0;
            wiredLanterns += TryWireLantern("HollowLantern", "LANTERN_CEILING_HOLLOW");
            wiredLanterns += TryWireLantern("EastWallLantern", "LANTERN_EASTWALL_HOLLOW");

            int wiredHearth = TryWireHearthAmbiance("Hearth", "Hearth_AmbianceZone");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[Hearthbound/Phase 27.4] Hollow: wired {wiredLanterns} lantern(s) + {wiredHearth} hearth ambiance trigger.");
        }

        private static int TryWireLantern(string lanternName, string lanternId)
        {
            var lantern = GameObject.Find(lanternName);
            if (lantern == null)
            {
                Debug.LogWarning($"[Hearthbound/Phase 27.4] '{lanternName}' not found — skipping interactable wire.");
                return 0;
            }

            if (lantern.GetComponentInChildren<Collider>(true) == null)
            {
                var bc = lantern.AddComponent<BoxCollider>();
                bc.size = new Vector3(0.7f, 1.0f, 0.7f);
                bc.center = new Vector3(0f, 0.5f, 0f);
            }

            var li = lantern.GetComponent<LanternInteractable>();
            if (li == null) li = lantern.AddComponent<LanternInteractable>();
            li.lanternId = lanternId;

            li.bulb = lantern.GetComponentInChildren<Light>(true);

            return 1;
        }

        private static int TryWireHearthAmbiance(string hearthName, string triggerName)
        {
            var hearth = GameObject.Find(hearthName);
            if (hearth == null)
            {
                Debug.LogWarning($"[Hearthbound/Phase 27.4] '{hearthName}' not found in Hollow.");
                return 0;
            }

            var triggerT = hearth.transform.Find(triggerName);
            GameObject triggerGO = triggerT != null ? triggerT.gameObject : null;
            if (triggerGO == null)
            {
                Debug.LogWarning($"[Hearthbound/Phase 27.4] Trigger '{triggerName}' missing under '{hearthName}'.");
                return 0;
            }

            var audio = hearth.GetComponentInChildren<AudioSource>(true);
            if (audio == null)
            {
                var audioGO = new GameObject("Hearth_Crackle");
                audioGO.transform.SetParent(hearth.transform, false);
                audioGO.transform.localPosition = Vector3.zero;
                audio = audioGO.AddComponent<AudioSource>();
                audio.loop = true;
                audio.playOnAwake = true;
                audio.spatialBlend = 1f;
                audio.minDistance = 1.5f;
                audio.maxDistance = 12f;
                audio.volume = 0.2f;

                var clip = FindHearthClip();
                if (clip != null) audio.clip = clip;
            }

            var hat = triggerGO.GetComponent<HearthAmbianceTrigger>();
            if (hat == null) hat = triggerGO.AddComponent<HearthAmbianceTrigger>();
            hat.hearthSource = audio;
            hat.playerTag = "Player";

            return 1;
        }

        private static AudioClip FindHearthClip()
        {
            string[] guids = AssetDatabase.FindAssets("t:AudioClip");
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var name = System.IO.Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
                if (name.Contains("fire") || name.Contains("hearth") || name.Contains("crackle") || name.Contains("ember"))
                {
                    return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                }
            }
            return null;
        }

        private static void SafeRun(string label, System.Action action)
        {
            try { action(); }
            catch (System.Exception e)
            {
                Debug.LogError($"[Hearthbound/Phase 27.4] '{label}' failed: {e.Message}");
            }
        }
    }
}
