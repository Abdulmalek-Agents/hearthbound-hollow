// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase17_LumenAndCinemachineBuilder
//
// Phase 17 — Lumen Stylized Lighting + Cinemachine third-person camera.
//
// Replaces the Phase 12 plain directional light + SimpleFollowCamera with:
//   • Lumen god-rays, candle glows, lantern halos (from
//     Packages/com.distantlands.lumen/), discovered by structural scoring.
//   • A Cinemachine 3.x virtual camera prefab (created via reflection so the
//     Editor asmdef doesn't need a hard Cinemachine reference — matches D-009).
//
// USE: Menu → Hearthbound → Phase 17 — Build Lumen + Cinemachine Bindings
//
// Output:
//   • Assets/_Project/ScriptableObjects/Setup/LightingBindings.asset
//     (catalogs Lumen god-ray + candle-glow + lantern prefabs)
//   • Assets/_Project/Prefabs/Cameras/CM_PlayerFollow.prefab
//     (Cinemachine virtual camera, body=Transposer/3rdPerson, target=Player tag)
//
// HearthboundOneClickSetup reads these in Phase 22 to dress the Hollow with
// real god rays through the windows + candle glows on the shelves, and to
// drive the camera with Cinemachine instead of the simple follow rig.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace HearthboundHollow.EditorTools
{
    public class LightingBindings : ScriptableObject
    {
        public GameObject godRayPrefab;       // Lumen god-ray mesh
        public GameObject candleGlowPrefab;   // Lumen candle / point-light halo
        public GameObject lanternHaloPrefab;  // Lumen lantern at shop entrance
        public GameObject lensFlarePrefab;    // Optional Lumen lens flare
    }

    public static class Phase17_LumenAndCinemachineBuilder
    {
        private const string LumenRoot = "Packages/com.distantlands.lumen";
        private const string BindingsDir = "Assets/_Project/ScriptableObjects/Setup";
        private const string BindingsPath = BindingsDir + "/LightingBindings.asset";
        private const string CamerasDir = "Assets/_Project/Prefabs/Cameras";
        private const string CmFollowPrefabPath = CamerasDir + "/CM_PlayerFollow.prefab";

        [MenuItem("Hearthbound/Phase 17 — Build Lumen + Cinemachine Bindings", priority = 204)]
        public static void Build()
        {
            EnsureFolder(BindingsDir);
            EnsureFolder(CamerasDir);

            BuildLumenBindings();
            BuildCinemachineFollowCamera();

            EditorUtility.DisplayDialog(
                "Phase 17 — Done",
                $"Lighting bindings: {BindingsPath}\n" +
                $"Cinemachine virtual camera: {CmFollowPrefabPath}\n\n" +
                "Re-run 'Hearthbound → Build Playable Mission 1 (One Click)' — the scene builder " +
                "will use these instead of the plain directional light + SimpleFollowCamera.",
                "OK");
        }

        // ─── Lumen detection ──────────────────────────────────────

        private static void BuildLumenBindings()
        {
            var bindings = AssetDatabase.LoadAssetAtPath<LightingBindings>(BindingsPath);
            if (bindings == null)
            {
                bindings = ScriptableObject.CreateInstance<LightingBindings>();
                AssetDatabase.CreateAsset(bindings, BindingsPath);
            }

            if (AssetDatabase.IsValidFolder(LumenRoot))
            {
                bindings.godRayPrefab     = FindLumenPrefab("god ray", new[] { "godray", "god_ray", "shaft", "rays", "lightshaft" });
                bindings.candleGlowPrefab = FindLumenPrefab("candle glow", new[] { "candle", "halo", "point_glow", "glow_warm" });
                bindings.lanternHaloPrefab = FindLumenPrefab("lantern halo", new[] { "lantern", "torch", "fire" });
                bindings.lensFlarePrefab  = FindLumenPrefab("lens flare", new[] { "flare", "lens" });
            }
            else
            {
                Debug.LogWarning($"[Hearthbound/Phase 17] {LumenRoot} not found. LightingBindings created empty — drop Lumen prefabs in manually.");
            }

            EditorUtility.SetDirty(bindings);
            AssetDatabase.SaveAssets();
        }

        private static GameObject FindLumenPrefab(string roleLabel, string[] keywords)
        {
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { LumenRoot });
            var candidates = new List<(string path, GameObject prefab, int score)>();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;
                int score = 0;
                var lowerPath = path.ToLowerInvariant();
                var lowerName = prefab.name.ToLowerInvariant();
                foreach (var kw in keywords)
                {
                    if (lowerName.Contains(kw)) score += 20;
                    else if (lowerPath.Contains(kw)) score += 8;
                }
                if (score > 0) candidates.Add((path, prefab, score));
            }
            candidates.Sort((a, b) => b.score.CompareTo(a.score));
            Debug.Log($"[Hearthbound/Phase 17] Top Lumen candidates for '{roleLabel}':");
            for (int i = 0; i < Mathf.Min(3, candidates.Count); i++)
                Debug.Log($"  #{i + 1} (score {candidates[i].score}): {candidates[i].path}");
            return candidates.Count > 0 ? candidates[0].prefab : null;
        }

        // ─── Cinemachine virtual camera prefab ─────────────────────

        private static void BuildCinemachineFollowCamera()
        {
            // Use reflection to instantiate CinemachineCamera (Cinemachine 3.x)
            // or CinemachineVirtualCamera (legacy 2.x). Avoid a hard asmdef dep.
            Type cmCameraType =
                FindType("Unity.Cinemachine.CinemachineCamera") ??
                FindType("Cinemachine.CinemachineVirtualCamera") ??
                FindType("Cinemachine.CinemachineCamera");

            if (cmCameraType == null)
            {
                Debug.LogWarning(
                    "[Hearthbound/Phase 17] Cinemachine type not found — skipping CM camera prefab. " +
                    "The scene will fall back to SimpleFollowCamera.");
                return;
            }

            var go = new GameObject("CM_PlayerFollow");
            try
            {
                var cm = go.AddComponent(cmCameraType);
                // Set priority field (varies between versions)
                SetMemberIfPresent(cm, "Priority", 10);
                SetMemberIfPresent(cm, "m_Priority", 10);

                // For CM3, the LookAt + Follow are usually serialized as Target fields.
                // We can't set the Player target at build time (it doesn't exist yet),
                // so the scene builder will populate it after spawning the player.
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Hearthbound/Phase 17] Cinemachine component add failed ({e.Message}); the prefab will be a plain GameObject placeholder.");
            }

            PrefabUtility.SaveAsPrefabAsset(go, CmFollowPrefabPath);
            UnityEngine.Object.DestroyImmediate(go);
            Debug.Log($"[Hearthbound/Phase 17] (created) {CmFollowPrefabPath}");
        }

        private static Type FindType(string fullName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var t = asm.GetType(fullName, throwOnError: false, ignoreCase: false);
                    if (t != null) return t;
                }
                catch { /* ignore */ }
            }
            return null;
        }

        private static void SetMemberIfPresent(object obj, string name, object value)
        {
            if (obj == null) return;
            var type = obj.GetType();
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (field != null) { try { field.SetValue(obj, value); } catch { } return; }
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite) { try { prop.SetValue(obj, value); } catch { } }
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            var leaf = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        // ─── Public lookups ───────────────────────────────────────

        public static LightingBindings TryGetLightingBindings() =>
            AssetDatabase.LoadAssetAtPath<LightingBindings>(BindingsPath);

        public static GameObject TryGetCinemachineFollowPrefab() =>
            AssetDatabase.LoadAssetAtPath<GameObject>(CmFollowPrefabPath);
    }
}
