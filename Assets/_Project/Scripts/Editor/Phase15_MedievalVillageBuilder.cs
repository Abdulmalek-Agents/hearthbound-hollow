// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase15_MedievalVillageBuilder
//
// Phase 15 — Medieval Village Environment.
//
// Replaces the Phase 12 plane-ground + cube-workbench + cube-door placeholders
// with real Medieval Village Megapack prefabs. We don't author wrapper prefabs
// here (Medieval Village pieces are already self-contained); instead we
// build a `MedievalVillageBindings.asset` ScriptableObject that catalogs the
// best vendor prefabs by role. HearthboundOneClickSetup reads the bindings
// and instantiates the vendor prefabs directly into the scene.
//
// USE: Menu → Hearthbound → Phase 15 — Build Medieval Village Bindings

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HearthboundHollow.EditorTools
{
    public class MedievalVillageBindings : ScriptableObject
    {
        public GameObject workbenchPrefab;   // table / bench / desk / counter
        public GameObject doorPrefab;        // standalone door for the Hollow entrance
        public GameObject cottagePrefab;     // house with walls + roof (full structure)
        public GameObject fencePrefab;       // wall / fence segment
        public GameObject wellPrefab;        // well / pump / fountain
        public GameObject treePrefab;        // tree / oak / pine for the lane
        public GameObject shelfPrefab;       // shelf / cabinet
        public GameObject lampPostPrefab;    // lamp / lantern / streetlight
    }

    public static class Phase15_MedievalVillageBuilder
    {
        private const string MVRoot = "Assets/MeshingunStudio";
        private const string BindingsDir = "Assets/_Project/ScriptableObjects/Setup";
        private const string BindingsPath = BindingsDir + "/MedievalVillageBindings.asset";

        [MenuItem("Hearthbound/Phase 15 — Build Medieval Village Bindings", priority = 202)]
        public static void Build()
        {
            if (!AssetDatabase.IsValidFolder(MVRoot))
            {
                EditorUtility.DisplayDialog(
                    "Phase 15 — Medieval Village not found",
                    $"Could not find {MVRoot}/.\n\n" +
                    "Please import the 'Medieval Village Megapack' asset, then re-run this menu item.",
                    "OK");
                return;
            }

            EnsureFolder(BindingsDir);

            var bindings = AssetDatabase.LoadAssetAtPath<MedievalVillageBindings>(BindingsPath);
            if (bindings == null)
            {
                bindings = ScriptableObject.CreateInstance<MedievalVillageBindings>();
                AssetDatabase.CreateAsset(bindings, BindingsPath);
            }

            bindings.workbenchPrefab = FindPrefab("workbench / table / counter",
                new[] { "workbench", "table", "bench", "desk", "counter", "anvil" },
                preferNoRoof: true);
            bindings.doorPrefab = FindPrefab("door",
                new[] { "door" }, excludeWords: new[] { "doorway" }, preferShape: PrefabShape.TallSmall);
            bindings.cottagePrefab = FindPrefab("cottage / house",
                new[] { "house", "cottage", "hut", "home", "building" },
                preferShape: PrefabShape.LargeBox);
            bindings.fencePrefab = FindPrefab("fence segment",
                new[] { "fence", "rail", "palisade" });
            bindings.wellPrefab = FindPrefab("well",
                new[] { "well", "fountain", "pump" });
            bindings.treePrefab = FindPrefab("tree / foliage",
                new[] { "tree", "oak", "pine", "birch", "elm" });
            bindings.shelfPrefab = FindPrefab("shelf / cabinet",
                new[] { "shelf", "cabinet", "cupboard", "rack" });
            bindings.lampPostPrefab = FindPrefab("lamp / lantern post",
                new[] { "lamp", "lantern", "torch_post", "streetlight" });

            EditorUtility.SetDirty(bindings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Phase 15 — Bindings saved",
                $"Medieval Village bindings catalogued at:\n  {BindingsPath}\n\n" +
                $"workbench:  {NameOrMissing(bindings.workbenchPrefab)}\n" +
                $"door:       {NameOrMissing(bindings.doorPrefab)}\n" +
                $"cottage:    {NameOrMissing(bindings.cottagePrefab)}\n" +
                $"fence:      {NameOrMissing(bindings.fencePrefab)}\n" +
                $"well:       {NameOrMissing(bindings.wellPrefab)}\n" +
                $"tree:       {NameOrMissing(bindings.treePrefab)}\n" +
                $"shelf:      {NameOrMissing(bindings.shelfPrefab)}\n" +
                $"lamp post:  {NameOrMissing(bindings.lampPostPrefab)}\n\n" +
                "Any 'MISSING' slots can be manually filled by selecting the bindings asset and " +
                "dragging the right prefab into the field.\n\n" +
                "Re-run 'Hearthbound → Build Playable Mission 1 (One Click)' — the scene builder " +
                "will use these prefabs to dress the Hollow and the lane.",
                "OK");
        }

        // ─── Detection helpers ────────────────────────────────────

        private enum PrefabShape { Any, TallSmall, LargeBox }

        private static string NameOrMissing(GameObject go) => go != null ? go.name : "MISSING";

        private static GameObject FindPrefab(string roleLabel, string[] keywords,
                                             string[] excludeWords = null,
                                             PrefabShape preferShape = PrefabShape.Any,
                                             bool preferNoRoof = false)
        {
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { MVRoot });
            var candidates = new List<(string path, GameObject prefab, int score)>();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var lowerPath = path.ToLowerInvariant();

                if (excludeWords != null)
                {
                    bool excluded = false;
                    foreach (var ex in excludeWords)
                        if (lowerPath.Contains(ex)) { excluded = true; break; }
                    if (excluded) continue;
                }

                // Skip obvious utility / character prefabs
                if (lowerPath.Contains("/character/")) continue;
                if (lowerPath.Contains("/scenes/")) continue;
                if (lowerPath.Contains("preview")) continue;
                if (lowerPath.Contains("editor")) continue;

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                int score = 0;
                foreach (var kw in keywords)
                {
                    if (lowerPath.Contains(kw)) score += 12;
                    if (prefab.name.ToLowerInvariant().Contains(kw)) score += 18;
                }
                if (score == 0) continue;

                // Shape heuristics via mesh bounds estimation
                var mf = prefab.GetComponentInChildren<MeshFilter>(true);
                if (mf != null && mf.sharedMesh != null)
                {
                    var b = mf.sharedMesh.bounds;
                    var size = b.size;
                    switch (preferShape)
                    {
                        case PrefabShape.TallSmall:
                            // Tall narrow object — like a door
                            if (size.y > size.x && size.y > size.z * 0.7f && size.x < 3f) score += 8;
                            break;
                        case PrefabShape.LargeBox:
                            // Large structure — like a house
                            if (size.x > 4f && size.y > 3f && size.z > 4f) score += 8;
                            if (size.x < 1.5f) score -= 8; // too small to be a house
                            break;
                    }
                }

                // For workbenches we prefer something that doesn't look like a roofed house.
                if (preferNoRoof && lowerPath.Contains("roof")) score -= 12;

                candidates.Add((path, prefab, score));
            }

            candidates.Sort((a, b) => b.score.CompareTo(a.score));
            Debug.Log($"[Hearthbound/Phase 15] Top candidates for '{roleLabel}':");
            for (int i = 0; i < Mathf.Min(3, candidates.Count); i++)
                Debug.Log($"  #{i + 1} (score {candidates[i].score}): {candidates[i].path}");

            return candidates.Count > 0 ? candidates[0].prefab : null;
        }

        // ─── Folder helpers ───────────────────────────────────────

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            var leaf = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        public static MedievalVillageBindings TryGetBindings() =>
            AssetDatabase.LoadAssetAtPath<MedievalVillageBindings>(BindingsPath);
    }
}
