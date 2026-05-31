// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase26_NpcAnimatorCapstone
//
// PHASE 26 polish layer — wires the NPC Animator pipeline:
//
//   1. Builds Hearthbound_NPC.controller via NpcAnimatorControllerBuilder.
//   2. Walks the Doris / Gerrold / SilentLane prefabs (built by Phase 13)
//      and:
//        a. Ensures each has an Animator referencing Hearthbound_NPC.controller
//           (or, if BoZo's nested prefab already supplies an Animator, just
//           swap the controller and keep the avatar).
//        b. Ensures each has a NpcAnimatorBridge component with `villager`
//           set to the matching VillagerSO seed asset.
//
// IDEMPOTENT — safe to re-run after any other capstone.
//
// USE: Menu → Hearthbound → 🎭 Phase 26 — Wire NPC Animators

using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using HearthboundHollow.Memory;
using HearthboundHollow.Mission;

namespace HearthboundHollow.EditorTools
{
    public static class Phase26_NpcAnimatorCapstone
    {
        // Prefab paths (created by Phase 13).
        private const string DorisPrefabPath       = "Assets/_Project/Prefabs/NPCs/Doris.prefab";
        private const string GerroldPrefabPath     = "Assets/_Project/Prefabs/NPCs/Gerrold.prefab";
        private const string SilentLanePrefabPath  = "Assets/_Project/Prefabs/NPCs/SilentLaneVillager.prefab";

        // Seed VillagerSO paths (created by SeedAssetGenerator).
        private const string DorisVillagerPath     = "Assets/_Project/ScriptableObjects/Villagers/Doris.asset";
        private const string GerroldVillagerPath   = "Assets/_Project/ScriptableObjects/Villagers/Gerrold.asset";
        private const string SilentLaneVillagerPath = "Assets/_Project/ScriptableObjects/Villagers/SilentLane.asset";

        [MenuItem("Hearthbound/⚙️ Advanced/\ud83c\udfad Phase 26 — Wire NPC Animators", priority = 2)]
        public static void Build()
        {
            EditorUtility.DisplayProgressBar("Hearthbound · Phase 26 NPC", "Building Hearthbound_NPC.controller …", 0.15f);
            try
            {
                NpcAnimatorControllerBuilder.BuildOrUpdate();
                var ctrl = NpcAnimatorControllerBuilder.TryGetController();
                if (ctrl == null)
                {
                    EditorUtility.DisplayDialog(
                        "Phase 26 — NPC Animators",
                        "Hearthbound_NPC.controller could not be built. Check the Console.",
                        "OK");
                    return;
                }

                int wired = 0;
                EditorUtility.DisplayProgressBar("Hearthbound · Phase 26 NPC", "Wiring Doris …", 0.40f);
                if (WirePrefab(DorisPrefabPath, DorisVillagerPath, ctrl)) wired++;

                EditorUtility.DisplayProgressBar("Hearthbound · Phase 26 NPC", "Wiring Gerrold …", 0.65f);
                if (WirePrefab(GerroldPrefabPath, GerroldVillagerPath, ctrl)) wired++;

                EditorUtility.DisplayProgressBar("Hearthbound · Phase 26 NPC", "Wiring SilentLaneVillager …", 0.85f);
                if (WirePrefab(SilentLanePrefabPath, SilentLaneVillagerPath, ctrl)) wired++;

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog(
                    "Phase 26 — NPC Animators",
                    $"✓ Hearthbound_NPC.controller built.\n" +
                    $"✓ {wired} of 3 NPC prefabs wired (Doris / Gerrold / SilentLane).\n\n" +
                    "Behaviour at runtime:\n" +
                    "  • NpcAnimatorBridge listens for DialogueStartedEvent / DialogueEndedEvent.\n" +
                    "  • Sets Animator.IsTalking accordingly — soft 0.18 s / 0.22 s transitions.\n\n" +
                    "If a prefab couldn't be wired, it likely hasn't been built yet:\n" +
                    "  Hearthbound → Phase 13 — Build BoZo Character Prefabs",
                    "OK");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        // ───── Per-prefab wiring ──────────────────────────────────

        private static bool WirePrefab(string prefabPath, string villagerSoPath, AnimatorController controller)
        {
            if (!File.Exists(prefabPath))
            {
                Debug.LogWarning($"[Hearthbound/Phase 26 NPC] (skip) {prefabPath} not found — run Phase 13 first.");
                return false;
            }
            var villager = AssetDatabase.LoadAssetAtPath<VillagerSO>(villagerSoPath);
            if (villager == null)
            {
                Debug.LogWarning($"[Hearthbound/Phase 26 NPC] (skip) {villagerSoPath} not found — run 'Create Mission 1-2 Seed Assets' first.");
                return false;
            }

            var contents = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                // 1) Animator on the prefab (BoZo wrapper puts it on a Body child).
                var animator = contents.GetComponentInChildren<Animator>(true);
                if (animator == null)
                {
                    animator = contents.AddComponent<Animator>();
                    Debug.Log($"[Hearthbound/Phase 26 NPC] Added Animator to {Path.GetFileName(prefabPath)}.");
                }
                if (animator.runtimeAnimatorController != controller)
                {
                    animator.runtimeAnimatorController = controller;
                    Debug.Log($"[Hearthbound/Phase 26 NPC] {Path.GetFileName(prefabPath)}: Animator → Hearthbound_NPC.controller.");
                }
                animator.applyRootMotion = false;

                // 2) NpcAnimatorBridge — one per prefab, on root for simplicity.
                var bridge = contents.GetComponent<NpcAnimatorBridge>();
                if (bridge == null) bridge = contents.AddComponent<NpcAnimatorBridge>();
                bridge.villager = villager;
                bridge.animator = animator;

                PrefabUtility.SaveAsPrefabAsset(contents, prefabPath);
                Debug.Log($"[Hearthbound/Phase 26 NPC] Saved {prefabPath}.");
                return true;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }
    }
}
