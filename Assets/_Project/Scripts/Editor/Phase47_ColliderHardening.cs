// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase47_ColliderHardening
//
// Phase 47.6 — Walk every gameplay scene and ensure every visible cottage /
// wall / tree / large prop has a working Collider, so the player cannot
// clip through and so push-back animation is reliable.
//
// Strategy:
//   • For each scene (Lane / Hollow / Garden / Cottage):
//     - Find every GameObject whose name matches one of the "should block"
//       keywords (cottage, wall, fence, tree, alder, pine, well, gate,
//       beehive, hay, log, pillar, column).
//     - Skip objects that already have a Collider.
//     - Skip objects under Phase 47's invisible-blocker parents (they
//       already have purpose-built BoxColliders).
//     - For each remaining target: compute mesh bounds and add a BoxCollider
//       sized to the mesh.
//
// Idempotent: re-running adds zero new colliders because the second pass
// short-circuits on the "already has a Collider" check.
//
// USE: Menu → Hearthbound → ⚙️ Advanced → 🛡️ Phase 47.6 — Collider Hardening

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HearthboundHollow.EditorTools
{
    public static class Phase47_ColliderHardening
    {
        private static readonly string[] Scenes =
        {
            "Assets/_Project/Scenes/02_Mission01_Lane.unity",
            "Assets/_Project/Scenes/03_Mission01_Hollow.unity",
            "Assets/_Project/Scenes/04_Mission02_Garden.unity",
            "Assets/_Project/Scenes/05_Mission02_Cottage.unity",
        };

        private static readonly string[] BlockingKeywords =
        {
            "cottage", "wall", "fence", "gate", "arch",
            "alder", "pine", "tree", "stump", "deadtree",
            "well", "bench", "barrel", "crate", "stand",
            "beehive", "haybale", "hay", "log", "woodlog",
            "pillar", "column", "lantern", "lamp", "stoneblock",
            "stonebrick", "stand", "table", "bookcase", "cupboard",
            "shopstand", "chimney",
        };

        // Names containing any of these strings are skipped (already special).
        private static readonly string[] ExclusionKeywords =
        {
            "InvisibleVoidBlockers", "InvisibleBlockers", "InvisibleRoomBoundary",
            "VoidBlocker_", "Block_N", "Block_E", "Block_W", "Block_S",
            "RoomBound_", "Bound_N", "Bound_E", "Bound_S", "Bound_W",
            "PathBrick", "Pebble", "GrassExtra", "Mushroom", "Grass",
            "Label", "Text", "Sign", "Inkblot", "Mark", "Glow", "Halo",
            "Cobble_", // path tiles intentionally walkable, no collider expansion
        };

        [MenuItem("Hearthbound/⚙️ Advanced/🛡️ Phase 47.6 — Collider Hardening", priority = 476)]
        public static void Build()
        {
            int totalHardened = 0;
            int totalScanned  = 0;
            int scenesTouched = 0;

            foreach (var path in Scenes)
            {
                if (!System.IO.File.Exists(path))
                {
                    Debug.LogWarning($"[Hearthbound/Phase 47.6] Scene missing: {path}");
                    continue;
                }
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                int scanned = 0, hardened = 0;
                HardenScene(out scanned, out hardened);
                totalScanned  += scanned;
                totalHardened += hardened;
                scenesTouched++;
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[Hearthbound/Phase 47.6] {path}  scanned={scanned}, hardened={hardened}");
            }

            EditorUtility.DisplayDialog("Phase 47.6 — Collider Hardening",
                $"🛡️ Collider hardening complete.\n\n" +
                $"Scenes touched : {scenesTouched}\n" +
                $"Objects scanned: {totalScanned}\n" +
                $"Colliders added: {totalHardened}\n\n" +
                "Re-run any time — idempotent.",
                "OK");
        }

        private static void HardenScene(out int scanned, out int hardened)
        {
            scanned = 0;
            hardened = 0;

#if UNITY_2023_1_OR_NEWER
            var allObjs = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
#else
            var allObjs = Object.FindObjectsOfType<GameObject>();
#endif
            foreach (var go in allObjs)
            {
                if (go == null) continue;
                if (IsExcluded(go.name)) continue;
                if (!IsBlockingCandidate(go.name)) continue;

                scanned++;
                if (go.GetComponentInChildren<Collider>() != null) continue;

                var renderers = go.GetComponentsInChildren<Renderer>();
                if (renderers.Length == 0) continue;

                Bounds b = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
                if (b.size.sqrMagnitude < 0.01f) continue;

                var bc = go.AddComponent<BoxCollider>();
                bc.size   = go.transform.InverseTransformVector(b.size);
                bc.center = go.transform.InverseTransformPoint(b.center);
                hardened++;
            }
        }

        private static bool IsBlockingCandidate(string name)
        {
            var lower = name.ToLowerInvariant();
            foreach (var kw in BlockingKeywords)
                if (lower.Contains(kw)) return true;
            return false;
        }

        private static bool IsExcluded(string name)
        {
            foreach (var kw in ExclusionKeywords)
                if (name.Contains(kw)) return true;
            return false;
        }
    }
}
