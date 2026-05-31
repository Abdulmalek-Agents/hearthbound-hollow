// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase47_HollowBoundariesAndInterior
//
// Phase 47.3 — Hollow interior wall colliders + cozy interior polish.
//
// The Phase 27.3 / 32.3 Hollow interior had wall meshes but in many cases
// the MeshCollider wasn't present (or was non-convex on a complex MV mesh
// and got rejected by URP at runtime). Phase 47.3:
//   1. Walks every Wall_* GameObject in the Hollow and ensures it has a
//      working blocking collider (BoxCollider, sized to the mesh bounds).
//   2. Adds an invisible 4 m-tall room-boundary BoxCollider around the
//      whole interior so the player can't escape through any seam.
//   3. Drops a warm wool rug under the workbench.
//   4. Adds a small bookcase against the east wall with stacked books.
//   5. Adds a second reading-corner chair beside the existing one.
//   6. Adds a small writing desk near the entrance counter with a quill +
//      inkpot + open ledger.
//   7. Mounts two wall-candle sconces above the entry door for extra
//      candle warmth.
//   8. Adds a hanging laundry-line of dried lavender (foreshadows the
//      Mission 2 garden / cleanse herb).
//
// All additions are parented under `_Phase47Env_Hollow` — Phase 27.3 and
// Phase 32.3 outputs are preserved.
//
// USE: Menu → Hearthbound → ⚙️ Advanced → 🏠 Phase 47.3 — Hollow Boundaries + Interior Polish

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HearthboundHollow.EditorTools
{
    public static class Phase47_HollowBoundariesAndInterior
    {
        private const string SceneHollow    = "Assets/_Project/Scenes/03_Mission01_Hollow.unity";
        private const string MVRoot         = "Assets/MeshingunStudio";
        private const string EnvParentName  = "_Phase47Env_Hollow";

        [MenuItem("Hearthbound/⚙️ Advanced/🏠 Phase 47.3 — Hollow Boundaries + Interior Polish", priority = 473)]
        public static void Build()
        {
            if (!System.IO.File.Exists(SceneHollow))
            {
                EditorUtility.DisplayDialog("Phase 47.3",
                    $"Scene not found: {SceneHollow}", "OK");
                return;
            }

            var binds = Phase32_VillageBindingsExtension.TryGetBindings();
            if (binds == null)
            {
                Phase32_VillageBindingsExtension.Build();
                binds = Phase32_VillageBindingsExtension.TryGetBindings();
            }

            var scene = EditorSceneManager.OpenScene(SceneHollow, OpenSceneMode.Single);

            var existing = GameObject.Find(EnvParentName);
            if (existing != null) Object.DestroyImmediate(existing);
            var envRoot = new GameObject(EnvParentName);

            int placed = 0;
            placed += HardenWallColliders();
            placed += BuildRoomBoundary(envRoot.transform);
            placed += PlaceRug(envRoot.transform);
            placed += PlaceBookcase(envRoot.transform, binds);
            placed += PlaceReadingChair(envRoot.transform, binds);
            placed += PlaceWritingDesk(envRoot.transform, binds);
            placed += PlaceWallSconces(envRoot.transform, binds);
            placed += PlaceHangingLavender(envRoot.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog("Phase 47.3 — Hollow polished",
                $"🏠 Hollow boundaries + interior polish built.\n\n" +
                $"{placed} additions placed under '{EnvParentName}'.\n\n" +
                "Walls now block player movement.\n" +
                "Re-run any time — idempotent.",
                "OK");
        }

        private static int HardenWallColliders()
        {
            int hardened = 0;
#if UNITY_2023_1_OR_NEWER
            var allObjs = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
#else
            var allObjs = Object.FindObjectsOfType<GameObject>();
#endif
            foreach (var go in allObjs)
            {
                var lower = go.name.ToLowerInvariant();
                if (!lower.Contains("wall") && !lower.Contains("floor") && !lower.Contains("ceiling")) continue;
                if (lower.Contains("door")) continue;
                if (go.GetComponent<Collider>() != null) continue;
                var mf = go.GetComponentInChildren<MeshFilter>();
                if (mf == null || mf.sharedMesh == null) continue;
                var bc = go.AddComponent<BoxCollider>();
                bc.size = mf.sharedMesh.bounds.size;
                bc.center = mf.sharedMesh.bounds.center;
                hardened++;
            }
            return hardened;
        }

        private static int BuildRoomBoundary(Transform envRoot)
        {
            var boundary = new GameObject("InvisibleRoomBoundary");
            boundary.transform.SetParent(envRoot, false);
            float halfW = 9.5f;
            float halfL = 6.0f;
            float zCentre = 0f;
            CreateInvisibleWall(boundary.transform, "RoomBound_N",
                new Vector3(0f, 2f, zCentre + halfL),
                new Vector3(halfW * 2f + 2f, 4f, 0.5f));
            CreateInvisibleWall(boundary.transform, "RoomBound_S",
                new Vector3(0f, 2f, zCentre - halfL),
                new Vector3(halfW * 2f + 2f, 4f, 0.5f));
            CreateInvisibleWall(boundary.transform, "RoomBound_E",
                new Vector3(halfW, 2f, zCentre),
                new Vector3(0.5f, 4f, halfL * 2f + 2f));
            CreateInvisibleWall(boundary.transform, "RoomBound_W",
                new Vector3(-halfW, 2f, zCentre),
                new Vector3(0.5f, 4f, halfL * 2f + 2f));
            return 4;
        }

        private static void CreateInvisibleWall(Transform parent, string name, Vector3 pos, Vector3 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var bc = go.AddComponent<BoxCollider>();
            bc.size = size;
        }

        private static int PlaceRug(Transform envRoot)
        {
            var rug = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rug.name = "WorkbenchRug";
            rug.transform.SetParent(envRoot, false);
            var workbench = GameObject.Find("Workbench") ?? GameObject.Find("WorkBench");
            Vector3 pos = workbench != null
                ? workbench.transform.position + new Vector3(0f, 0.005f, 0.5f)
                : new Vector3(-3.5f, 0.005f, 1.5f);
            rug.transform.position = pos;
            rug.transform.localScale = new Vector3(2.4f, 0.02f, 1.6f);
            var col = rug.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);
            var mr = rug.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                mat.name = "Phase47_RugWool";
                mat.color = new Color(0.55f, 0.20f, 0.18f, 1f);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.15f);
                if (mat.HasProperty("_Metallic"))   mat.SetFloat("_Metallic", 0f);
                mr.sharedMaterial = mat;
            }
            return 1;
        }

        private static int PlaceBookcase(Transform envRoot, MedievalVillageBindingsV2 binds)
        {
            int placed = 0;
            var cupboard = binds?.cupboardPrefab ?? FindMVPrefab(new[] { "cabinet", "cupboard", "wallshelf", "shelf" });
            if (cupboard == null) return 0;
            var bc = (GameObject)PrefabUtility.InstantiatePrefab(cupboard);
            bc.name = "Bookcase";
            bc.transform.SetParent(envRoot, true);
            bc.transform.position = new Vector3(2.4f, 0f, -1.0f);
            bc.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
            EnsureBlockingCollider(bc);
            placed++;
            var book = FindMVPrefab(new[] { "book_01", "book", "ledger" });
            if (book != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    var b = (GameObject)PrefabUtility.InstantiatePrefab(book);
                    b.name = $"BookcaseBook_{i:00}";
                    b.transform.SetParent(bc.transform, false);
                    b.transform.localPosition = new Vector3(-0.4f + i * 0.18f, 0.95f + (i % 2) * 0.04f, -0.18f);
                    b.transform.localRotation = Quaternion.Euler(0f, i * 12f, 0f);
                    placed++;
                }
            }
            return placed;
        }

        private static int PlaceReadingChair(Transform envRoot, MedievalVillageBindingsV2 binds)
        {
            int placed = 0;
            var chair = binds?.stoolPrefab ?? FindMVPrefab(new[] { "chair_01a", "chair_01", "stool_01a", "stool" });
            if (chair == null) return 0;
            var c = (GameObject)PrefabUtility.InstantiatePrefab(chair);
            c.name = "ReadingChair_Second";
            c.transform.SetParent(envRoot, true);
            c.transform.position = new Vector3(1.8f, 0f, 0.5f);
            c.transform.rotation = Quaternion.Euler(0f, 135f, 0f);
            EnsureBlockingCollider(c);
            placed++;
            var blanket = FindMVPrefab(new[] { "grain_sack_01a", "foodsac_01b", "sack_potato" });
            if (blanket != null)
            {
                var b = (GameObject)PrefabUtility.InstantiatePrefab(blanket);
                b.name = "ChairBlanket";
                b.transform.SetParent(c.transform, false);
                b.transform.localPosition = new Vector3(0f, 0.5f, -0.15f);
                b.transform.localScale = Vector3.one * 0.4f;
                placed++;
            }
            return placed;
        }

        private static int PlaceWritingDesk(Transform envRoot, MedievalVillageBindingsV2 binds)
        {
            int placed = 0;
            var table = FindMVPrefab(new[] { "woodtable_01a", "woodtable_02a", "woodtable", "table" });
            if (table != null)
            {
                var t = (GameObject)PrefabUtility.InstantiatePrefab(table);
                t.name = "WritingDesk";
                t.transform.SetParent(envRoot, true);
                t.transform.position = new Vector3(2.5f, 0f, -3.5f);
                t.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
                t.transform.localScale = Vector3.one * 0.8f;
                EnsureBlockingCollider(t);
                placed++;
                var book = FindMVPrefab(new[] { "book_01", "book", "ledger" });
                if (book != null)
                {
                    var b = (GameObject)PrefabUtility.InstantiatePrefab(book);
                    b.name = "Desk_OpenLedger";
                    b.transform.SetParent(t.transform, false);
                    b.transform.localPosition = new Vector3(0f, 0.85f, 0f);
                    b.transform.localRotation = Quaternion.Euler(0f, 25f, 0f);
                    placed++;
                }
                var jar = FindMVPrefab(new[] { "jar_01a", "bottle_01a", "bottle" });
                if (jar != null)
                {
                    var j = (GameObject)PrefabUtility.InstantiatePrefab(jar);
                    j.name = "Desk_Inkpot";
                    j.transform.SetParent(t.transform, false);
                    j.transform.localPosition = new Vector3(0.25f, 0.85f, 0.18f);
                    j.transform.localScale = Vector3.one * 0.5f;
                    placed++;
                }
            }
            return placed;
        }

        private static int PlaceWallSconces(Transform envRoot, MedievalVillageBindingsV2 binds)
        {
            int placed = 0;
            var candle = binds?.candleabraPrefab ?? FindMVPrefab(new[] { "candleabra_02a", "thickcandle_01a", "thickcandle" });
            if (candle == null) return 0;
            for (int i = 0; i < 2; i++)
            {
                var c = (GameObject)PrefabUtility.InstantiatePrefab(candle);
                c.name = $"DoorSconce_{i:00}";
                c.transform.SetParent(envRoot, true);
                float x = i == 0 ? -1.3f : 1.3f;
                c.transform.position = new Vector3(x, 2.0f, -5.8f);
                c.transform.localScale = Vector3.one * 0.7f;
                placed++;
                var bulb = new GameObject($"Sconce_{i:00}_Bulb");
                bulb.transform.SetParent(c.transform, false);
                bulb.transform.localPosition = new Vector3(0f, 0.3f, 0f);
                var l = bulb.AddComponent<Light>();
                l.type = LightType.Point;
                l.color = new Color(1.0f, 0.78f, 0.42f);
                l.intensity = 1.4f;
                l.range = 4.5f;
                l.shadows = LightShadows.None;
                placed++;
            }
            return placed;
        }

        private static int PlaceHangingLavender(Transform envRoot)
        {
            var lav = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lav.name = "HangingLavender";
            lav.transform.SetParent(envRoot, false);
            lav.transform.position = new Vector3(0f, 2.55f, 0.5f);
            lav.transform.localScale = new Vector3(0.6f, 0.08f, 0.15f);
            var col = lav.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);
            var mr = lav.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                mat.name = "Phase47_DriedLavender";
                mat.color = new Color(0.62f, 0.48f, 0.74f, 1f);
                mr.sharedMaterial = mat;
            }
            return 1;
        }

        private static void EnsureBlockingCollider(GameObject go)
        {
            if (go.GetComponentInChildren<Collider>() != null) return;
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;
            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
            var bc = go.AddComponent<BoxCollider>();
            bc.size   = go.transform.InverseTransformVector(b.size);
            bc.center = go.transform.InverseTransformPoint(b.center);
        }

        private static GameObject FindMVPrefab(string[] keywords)
        {
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { MVRoot });
            var best = new List<(int score, GameObject prefab)>();
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var lower = path.ToLowerInvariant();
                if (lower.Contains("/scenes/") || lower.Contains("preview") || lower.Contains("editor")) continue;
                int score = 0;
                foreach (var kw in keywords)
                {
                    string k = kw.ToLowerInvariant();
                    if (lower.Contains(k)) score += 12;
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab != null && prefab.name.ToLowerInvariant().Contains(k)) score += 18;
                }
                if (score == 0) continue;
                best.Add((score, AssetDatabase.LoadAssetAtPath<GameObject>(path)));
            }
            best.Sort((a, b) => b.score.CompareTo(a.score));
            return best.Count > 0 ? best[0].prefab : null;
        }
    }
}
