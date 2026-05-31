// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase42_ListenSceneCameraBuilder
//
// Phase 42 — Listen Scene Camera Authoring.
//
// Drops a `ListenSceneCameraDirector` onto the Cottage scene and wires it
// to the existing `ListenSceneSequencer` (which Phase 36 placed as a child
// of `MemoryDreamRig.prefab` — at runtime it's instantiated into the
// scene). The director animates Camera.main through 4 preset waypoints
// during the 180-second Listen monologue:
//
//   [0-30 s]   Wide establishing
//   [30-90 s]  Tight on Gerrold's chair
//   [90-150 s] Tight on Gerrold's hands
//   [150-180 s] Slow pull-back → fade to dream
//
// IDEMPOTENT — uses `FindFirstObjectByType<ListenSceneCameraDirector>()`
// to detect prior installs and reuse them. Re-running this builder leaves
// any user-tweaked waypoint Transforms alone.
//
// USE: Menu → Hearthbound → ⚙️ Advanced → 🎥 Phase 42 — Wire Listen Scene Camera
//
// Chained from Phase 27 Build Everything (step 12/12).

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using HearthboundHollow.Cutscene;

namespace HearthboundHollow.EditorTools
{
    public static class Phase42_ListenSceneCameraBuilder
    {
        private const string CottageScenePath = "Assets/_Project/Scenes/05_Mission02_Cottage.unity";
        private const string DirectorObjectName = "_HHAudio_ListenSceneCameraDirector";

        [MenuItem("Hearthbound/⚙️ Advanced/🎥 Phase 42 — Wire Listen Scene Camera", priority = 222)]
        public static void Build()
        {
            if (!System.IO.File.Exists(CottageScenePath))
            {
                Debug.LogWarning($"[Hearthbound/Phase 42] Cottage scene not found at {CottageScenePath} — run Phase 24 first.");
                return;
            }

            var scene = EditorSceneManager.OpenScene(CottageScenePath, OpenSceneMode.Single);

            // Find or create the director GameObject. Owned-by-Phase-42 by name.
            var director = Object.FindFirstObjectByType<ListenSceneCameraDirector>();
            if (director == null)
            {
                var go = GameObject.Find(DirectorObjectName);
                if (go == null)
                {
                    go = new GameObject(DirectorObjectName);
                }
                director = go.GetComponent<ListenSceneCameraDirector>();
                if (director == null) director = go.AddComponent<ListenSceneCameraDirector>();
            }

            // Try to wire the sequencer. At runtime, ListenSceneSequencer is
            // instantiated under MemoryDreamRig (Phase 36 prefab); at Editor
            // time, we can find an existing instance in the scene if Phase
            // 24 / Phase 38 dropped one. Otherwise, the director will
            // resolve at runtime via Start() once the rig instantiates.
            var sequencer = Object.FindFirstObjectByType<ListenSceneSequencer>();
            if (sequencer != null)
            {
                director.sequencer = sequencer;
                Debug.Log($"[Hearthbound/Phase 42] ✓ Wired director.sequencer → {sequencer.gameObject.name}");
            }
            else
            {
                Debug.Log("[Hearthbound/Phase 42] No ListenSceneSequencer in scene at Editor time — " +
                          "director will resolve via Start() at runtime (this is fine; the rig " +
                          "spawns from MemoryDreamRig.prefab on scene load).");
            }

            // Try to find a Gerrold transform for the synthesised-waypoint
            // fallback. Phase 22/24 scene builder names him "Gerrold" — but
            // any GameObject containing "gerrold" is a safe candidate.
            var gerrold = FindGerroldFallback();
            if (gerrold != null)
            {
                director.gerroldFocus = gerrold.transform;
                Debug.Log($"[Hearthbound/Phase 42] ✓ Wired director.gerroldFocus → {gerrold.name}");
            }
            else
            {
                Debug.Log("[Hearthbound/Phase 42] No Gerrold found in cottage scene — " +
                          "director will use Camera.main forward vector as the synth focus.");
            }

            // Default beat timings (sum = 180 s).
            director.beat0_WideEstablish = 30f;
            director.beat1_TightChair    = 60f;
            director.beat2_TightHands    = 60f;
            director.beat3_PullBack      = 30f;
            director.easeExponent        = 1.6f;

            EditorUtility.SetDirty(director);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog(
                "Phase 42 — Listen Scene Camera",
                "Listen Scene camera director wired.\n\n" +
                "• 4-waypoint camera path across the 180-second monologue\n" +
                "• Beats: 30 / 60 / 60 / 30 seconds\n" +
                "• Synth fallback active — drag Inspector waypoint Transforms\n" +
                "  to override with hand-authored shots\n\n" +
                "When the player picks 'Listen' in Mission 2's moral choice,\n" +
                "the camera now animates around Gerrold automatically.",
                "OK");
        }

        private static GameObject FindGerroldFallback()
        {
            // Scan all root GameObjects in the open scene for one named "Gerrold"
            // (case-insensitive substring). The Phase 24 cottage builder names
            // it exactly "Gerrold" or a child of "NPCs".
            foreach (var root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (root == null) continue;
                if (ContainsGerroldName(root.name)) return root;
                // Walk children.
                foreach (var t in root.GetComponentsInChildren<Transform>(includeInactive: true))
                {
                    if (t != null && ContainsGerroldName(t.gameObject.name)) return t.gameObject;
                }
            }
            return null;
        }

        private static bool ContainsGerroldName(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            return s.IndexOf("gerrold", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
