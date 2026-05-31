// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase19_WeatherAndWindBuilder
//
// Phase 19 — Atmosphere (Stylized Weather + Zephyr Wind).
//
// Builds a `WeatherBindings.asset` SO that catalogs:
//   • A Stylized Weather System manager prefab (from Unluck Software)
//   • A Zephyr global wind zone prefab (from Distant Lands)
//
// HearthboundOneClickSetup spawns these in each scene at Phase 22 to give
// the autumn lane its soft evening fog + foliage breeze.
//
// USE: Menu → Hearthbound → Phase 19 — Build Weather + Wind Bindings

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HearthboundHollow.EditorTools
{
    public class WeatherBindings : ScriptableObject
    {
        public GameObject stylizedWeatherManagerPrefab;  // Unluck Software / Stylized Weather
        public GameObject zephyrWindZonePrefab;          // Distant Lands / Zephyr global wind
        [Tooltip("Optional fog volume override — only used if the weather manager doesn't include one.")]
        public GameObject fogVolumePrefab;
    }

    public static class Phase19_WeatherAndWindBuilder
    {
        private const string StylizedWeatherRoot = "Assets/Unluck Software/Stylized Weather";
        private const string ZephyrPackageRoot = "Packages/com.distantlands.zephyr";
        private const string ZephyrAssetsRoot = "Assets/Distant Lands/Zephyr"; // fallback if installed via .unitypackage
        private const string BindingsDir = "Assets/_Project/ScriptableObjects/Setup";
        private const string BindingsPath = BindingsDir + "/WeatherBindings.asset";

        [MenuItem("Hearthbound/⚙️ Advanced/Phase 19 — Build Weather + Wind Bindings", priority = 206)]
        public static void Build()
        {
            EnsureFolder(BindingsDir);

            var bindings = AssetDatabase.LoadAssetAtPath<WeatherBindings>(BindingsPath);
            if (bindings == null)
            {
                bindings = ScriptableObject.CreateInstance<WeatherBindings>();
                AssetDatabase.CreateAsset(bindings, BindingsPath);
            }

            bindings.stylizedWeatherManagerPrefab = FindPrefab(
                new[] { StylizedWeatherRoot, "Assets/Unluck Software" },
                roleLabel: "Stylized Weather manager",
                nameKeywords: new[] { "manager", "controller", "weather_system", "stylizedweather" },
                pathKeywords: new[] { "stylized", "weather" });

            bindings.zephyrWindZonePrefab = FindPrefab(
                new[] { ZephyrPackageRoot, ZephyrAssetsRoot, "Packages/com.distantlands.zephyr" },
                roleLabel: "Zephyr global wind zone",
                nameKeywords: new[] { "wind", "zephyr", "global_wind", "windzone" },
                pathKeywords: new[] { "zephyr", "wind" });

            bindings.fogVolumePrefab = FindPrefab(
                new[] { StylizedWeatherRoot, "Assets" },
                roleLabel: "fog volume",
                nameKeywords: new[] { "fog", "mist", "haze" },
                pathKeywords: new[] { "fog", "weather" });

            EditorUtility.SetDirty(bindings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Phase 19 — Done",
                $"Weather bindings at:\n  {BindingsPath}\n\n" +
                $"Stylized Weather manager: {NameOrMissing(bindings.stylizedWeatherManagerPrefab)}\n" +
                $"Zephyr wind zone:         {NameOrMissing(bindings.zephyrWindZonePrefab)}\n" +
                $"Fog volume (optional):    {NameOrMissing(bindings.fogVolumePrefab)}\n\n" +
                "Empty slots can be manually filled by selecting the bindings asset and " +
                "dragging the right prefab in.\n\n" +
                "Re-run 'Hearthbound → Build Playable Mission 1 (One Click)' — the scene builder " +
                "will spawn the weather manager + wind zone with light-evening-fog presets.",
                "OK");
        }

        // ─── Detection helpers ────────────────────────────────────

        private static string NameOrMissing(GameObject go) => go != null ? go.name : "MISSING";

        private static GameObject FindPrefab(string[] searchRoots, string roleLabel,
                                             string[] nameKeywords, string[] pathKeywords)
        {
            // Build the list of valid roots that actually exist.
            var validRoots = new List<string>();
            foreach (var r in searchRoots) if (AssetDatabase.IsValidFolder(r)) validRoots.Add(r);
            if (validRoots.Count == 0)
            {
                Debug.LogWarning($"[Hearthbound/Phase 19] No valid asset root found for '{roleLabel}' (tried: {string.Join(", ", searchRoots)})");
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
            Debug.Log($"[Hearthbound/Phase 19] Top candidates for '{roleLabel}':");
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

        public static WeatherBindings TryGetBindings() =>
            AssetDatabase.LoadAssetAtPath<WeatherBindings>(BindingsPath);
    }
}
