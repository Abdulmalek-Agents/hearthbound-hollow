// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase26_NpcAnimatorCapstone
//
// Phase 26 — one-click capstone that finishes the NPC Animator + dialogue
// "IsTalking" wiring across every speaking villager prefab in the project.
//
// What it does (idempotent):
//   1. Calls NpcAnimatorControllerBuilder.BuildIfMissing() to ensure
//      Assets/_Project/Animations/Hearthbound_NPC.controller exists.
//   2. Walks the 3 NPC prefabs (Doris, Gerrold, SilentLaneVillager — all
//      authored by Phase 13) and:
//        - Adds Animator if missing.
//        - Sets the Animator.runtimeAnimatorController to the NPC controller.
//        - Sets applyRootMotion = false (so the prefab doesn't drift).
//        - Adds NpcAnimatorBridge if missing (auto-toggles IsTalking on
//          DialogueStartedEvent / DialogueEndedEvent that match its
//          serialized VillagerSO).
//        - Wires bridge.villager from the prefab's NpcController.
//   3. Saves the prefab back to disk.
//
// USE: Menu → Hearthbound → Phase 26 — Wire NPC Animators

using HearthboundHollow.Mission;
using HearthboundHollow.Player;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace HearthboundHollow.EditorTools
{
    public static class Phase26_NpcAnimatorCapstone
    {
        [MenuItem("Hearthbound/⚙️ Advanced/🎭 Phase 26 — Wire NPC Animators", priority = 2)]
        public static void Build()
        {
            int wired = 0, skipped = 0;
            // Make sure the controller asset exists.
            var controller = NpcAnimatorControllerBuilder.BuildIfMissing();
            if (controller == null)
            {
                EditorUtility.DisplayDialog("Phase 26 — NPC Animator",
                    "Failed to build Hearthbound_NPC.controller. " +
                    "Check the Console — likely no BoZo F_Idle clip is in the project yet.\n\n" +
                    "Run Phase 13 (BoZo characters) first, then retry this step.",
                    "OK");
                return;
            }

            // Walk every BoZo-derived NPC prefab the Phase 13 builder produced.
            string[] candidatePaths =
            {
                Phase13_BoZoCharacterBuilder.DorisPrefabPath,
                Phase13_BoZoCharacterBuilder.GerroldPrefabPath,
                Phase13_BoZoCharacterBuilder.SilentLaneVillagerPrefabPath,
            };

            foreach (var path in candidatePaths)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    Debug.LogWarning($"[Hearthbound/Phase 26] NPC prefab missing: '{path}'. Run Phase 13 first.");
                    skipped++;
                    continue;
                }

                if (WirePrefab(path, controller)) wired++;
                else skipped++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Phase 26 — NPC Animators wired",
                $"🎭 NPC animators wired on {wired} prefab(s). Skipped {skipped}.\n\n" +
                "Each speaking NPC now drives IsTalking from the dialogue stream.\n\n" +
                "Press Play — Doris and Gerrold will visibly shift between Idle and Talking " +
                "during conversation. The transition is subtle (Mixamo Talking.fbx upgrade " +
                "lives in /Animations/Mixamo/ — drop one in and re-run to upgrade).",
                "OK");
        }

        private static bool WirePrefab(string prefabPath, AnimatorController controller)
        {
            using var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath);
            var root = scope.prefabContentsRoot;
            if (root == null) return false;

            // Animator on the root
            var animator = root.GetComponent<Animator>() ?? root.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            if (animator.avatar == null)
            {
                // Try to find the avatar on a SkinnedMeshRenderer's bones —
                // we can't always synthesize one but if the prefab was made
                // with a Humanoid model, Unity has one we can reuse.
                var animatorOnRig = root.GetComponentInChildren<Animator>(true);
                if (animatorOnRig != null && animatorOnRig.avatar != null && animatorOnRig != animator)
                    animator.avatar = animatorOnRig.avatar;
            }

            // NpcAnimatorBridge on the root (or where the NpcController lives).
            var bridge = root.GetComponent<NpcAnimatorBridge>() ?? root.AddComponent<NpcAnimatorBridge>();
            var npcController = root.GetComponentInChildren<NpcController>(true);
            if (npcController != null && npcController.villager != null)
            {
                bridge.villager = npcController.villager;
            }
            bridge.animator = animator;

            EditorUtility.SetDirty(root);
            return true;
        }
    }
}
