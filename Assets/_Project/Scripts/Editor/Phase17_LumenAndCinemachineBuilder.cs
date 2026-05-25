// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase17_LumenAndCinemachineBuilder
//
// Phase 17 — Lighting + Camera bindings.
//
// Catalogs the prefabs we'll use for cozy lighting (Lumen Stylized Light FX
// 2 from Distant Lands) and the gameplay third-person camera (Cinemachine).
// HearthboundOneClickSetup reads these bindings and spawns:
//   • Lumen sunshafts in the Lane (autumn-evening shafts through trees)
//   • Lumen candle / lantern in the Hollow (warm orange glow on the workbench)
//   • Cinemachine third-person follow camera tracking the Player
//
// USE: Menu → Hearthbound → Phase 17 — Build Lumen + Cinemachine Bindings
//
// Approach: same fuzzy-search pattern as Phase 15. We don't author wrapper
// prefabs — instead we point at the vendor prefab via a single bindings SO.

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HearthboundHollow.EditorTools
{
    public class LumenAndCinemachineBindings : ScriptableObject
    {
        [Tooltip("Stylized sunshaft / god-ray prefab (Lumen). " +
                 "Placed in the Lane for the autumn-dusk light through trees.")]
        public GameObject sunshaftsPrefab;

        [Tooltip("Stylized point/spot light prefab (Lumen). " +
                 "Placed at the Hollow workbench for warm candle glow.")]
        public GameObject cozyLightPrefab;

        [Tooltip("Cinemachine FreeLook or third-person follow prefab. " +
                 "Falls back to a programmatic CinemachineCamera if not found.")]
        public GameObject cinemachineCameraPrefab;
    }

    public static class Phase17_LumenAndCinemachineBuilder
    {
        private const string LumenPackageRoot  = "Packages/com.distantlands.lumen";
        private const string LumenAssetsRoot   = "Assets/Distant Lands/Lumen";
        private const string AssetsRootGeneric = "Assets";
        private const string BindingsDir       = "Assets/_Project/ScriptableObjects/Setup";
        private const string BindingsPath      = BindingsDir + "/LumenAndCinemachineBindings.asset";

        [MenuItem("Hearthbound/⚙️ Advanced/Phase 17 — Build Lumen + Cinemachine Bindings", priority = 204)]
        public static void Build()
        {
            EnsureFolder(BindingsDir);

            var bindings = AssetDatabase.LoadAssetAtPath<LumenAndCinemachineBindings>(BindingsPath);
            if (bindings == null)
            {
                bindings = ScriptableObject.CreateInstance<LumenAndCinemachineBindings>();
                AssetDatabase.CreateAsset(bindings, BindingsPath);
            }

            // ── Lumen sunshafts ─────────────────────────────────
            bindings.sunshaftsPrefab = FindPrefab(
                searchRoots: new[] { LumenPackageRoot, LumenAssetsRoot, AssetsRootGeneric },
                roleLabel: "Lumen sunshafts",
                nameKeywords: new[] { "sunshaft", "godray", "god_ray", "lightshaft", "shaft" },
                pathKeywords: new[] { "lumen", "stylized" });

            // ── Lumen cozy light (candle / lantern halo) ─────────
            bindings.cozyLightPrefab = FindPrefab(
                searchRoots: new[] { LumenPackageRoot, LumenAssetsRoot, AssetsRootGeneric },
                roleLabel: "Lumen cozy light (candle / lantern)",
                nameKeywords: new[] { "candle", "lantern", "halo", "orblight", "pointlight" },
                pathKeywords: new[] { "lumen", "stylized" });

            // ── Cinemachine third-person camera ──────────────────
            bindings.cinemachineCameraPrefab = FindPrefab(
                searchRoots: new[] { "Packages/com.unity.cinemachine", AssetsRootGeneric },
                roleLabel: "Cinemachine third-person camera",
                nameKeywords: new[] { "freelook", "thirdperson", "third_person", "follow", "cmcamera", "cinemachinecamera" },
                pathKeywords: new[] { "cinemachine" });

            EditorUtility.SetDirty(bindings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Phase 17 — Done",
                $"Lumen + Cinemachine bindings at:\n  {BindingsPath}\n\n" +
                $"Lumen sunshafts:           {NameOrMissing(bindings.sunshaftsPrefab)}\n" +
                $"Lumen cozy light:          {NameOrMissing(bindings.cozyLightPrefab)}\n" +
                $"Cinemachine 3rd-person:    {NameOrMissing(bindings.cinemachineCameraPrefab)}\n\n" +
                "Empty slots can be manually filled by selecting the bindings asset and " +
                "dragging the right prefab in.\n\n" +
                "Re-run 'Hearthbound → Build Playable Mission 1 (One Click)' — the scene builder " +
                "will spawn the cozy lights + camera in the Lane and Hollow.",
                "OK");
        }

        // ─── Detection helpers ────────────────────────────────────

        private static string NameOrMissing(GameObject go) => go != null ? go.name : "MISSING";

        private static GameObject FindPrefab(string[] searchRoots, string roleLabel,
                                             string[] nameKeywords, string[] pathKeywords)
        {
            // Build the list of valid roots that actually exist (filter out missing UPM packages etc.).
            var validRoots = new List<string>();
            foreach (var r in searchRoots) if (AssetDatabase.IsValidFolder(r)) validRoots.Add(r);
            if (validRoots.Count == 0)
            {
                Debug.LogWarning($"[Hearthbound/Phase 17] No valid asset root found for '{roleLabel}' (tried: {string.Join(", ", searchRoots)})");
                return null;
            }

            var candidates = new List<(string path, GameObject prefab, int score)>();
            var guids = AssetDatabase.FindAssets("t:Prefab", validRoots.ToArray());
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;
                int score = 0;
                var lowerPath = path.ToLowerInvariant();
                var lowerName = prefab.name.ToLowerInvariant();
                foreach (var kw in nameKeywords)
                {
                    if (lowerName.Contains(kw)) score += 22;
                }
                foreach (var kw in pathKeywords)
                {
                    if (lowerPath.Contains(kw)) score += 8;
                }
                if (score > 0) candidates.Add((path, prefab, score));
            }
            candidates.Sort((a, b) => b.score.CompareTo(a.score));
            Debug.Log($"[Hearthbound/Phase 17] Top candidates for '{roleLabel}':");
            for (int i = 0; i < Mathf.Min(3, candidates.Count); i++)
                Debug.Log($"  #{i + 1} (score {candidates[i].score}): {candidates[i].path}");
            return candidates.Count > 0 ? candidates[0].prefab : null;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            var leaf = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        public static LumenAndCinemachineBindings TryGetBindings() =>
            AssetDatabase.LoadAssetAtPath<LumenAndCinemachineBindings>(BindingsPath);
    }
}
