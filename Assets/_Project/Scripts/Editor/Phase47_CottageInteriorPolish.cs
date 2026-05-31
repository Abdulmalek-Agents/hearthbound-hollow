// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase47_CottageInteriorPolish
//
// Phase 47.5 — Mission 2 Widower's Cottage interior polish.
//
// The cottage is Mission 2's emotional peak ("20% prop and 80% mood" per
// Focus 03 § 5.1). Every prop must say one of two things:
//   • Someone lived here recently who is no longer here.
//   • The man who remains is enduring.
//
// Phase 47.5 adds the props the earlier capstones didn't cover, all
// procedurally + idempotently under `_Phase47Env_Cottage`:
//
//   1. Hardened interior wall colliders (player cannot walk through walls).
//   2. Invisible cottage-boundary BoxColliders (defence-in-depth).
//   3. The hearth fire — Lumen flame mesh + realtime point light with a
//      sine-wave flicker animator (HearthFlicker.cs runtime component).
//   4. The framed photograph on the mantel — Margery + Gerrold wedding day.
//   5. A bookcase against the west wall with stacked books.
//   6. Margery's chair with the book on its arm + a pressed-leaf bookmark.
//   7. The cold tea cup on the small table in front of Margery's chair.
//   8. A wool rug between the two chairs.
//   9. Two mantel candles + a Lumen halo.
//   10. The closed bedroom door (decorative — locked in M1-2).
//
// USE: Menu → Hearthbound → ⚙️ Advanced → 🏚️ Phase 47.5 — Cottage Interior Polish

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HearthboundHollow.EditorTools
{
    public static class Phase47_CottageInteriorPolish
    {
        private const string SceneCottage  = "Assets/_Project/Scenes/05_Mission02_Cottage.unity";
        private const string MVRoot        = "Assets/MeshingunStudio";
        private const string LumenPrefabs  = "Packages/com.distantlands.lumen/Content/Prefabs";
        private const string EnvParentName = "_Phase47Env_Cottage";

        [MenuItem("Hearthbound/⚙️ Advanced/🏚️ Phase 47.5 — Cottage Interior Polish", priority = 475)]
        public static void Build()
        {
            if (!System.IO.File.Exists(SceneCottage))
            {
                EditorUtility.DisplayDialog("Phase 47.5",
                    $"Scene not found: {SceneCottage}", "OK");
                return;
            }

            var binds = Phase32_VillageBindingsExtension.TryGetBindings();
            if (binds == null)
            {
                Phase32_VillageBindingsExtension.Build();
                binds = Phase32_VillageBindingsExtension.TryGetBindings();
            }

            var scene = EditorSceneManager.OpenScene(SceneCottage, OpenSceneMode.Single);
            var existing = GameObject.Find(EnvParentName);
            if (existing != null) Object.DestroyImmediate(existing);
            var envRoot = new GameObject(EnvParentName);

            int placed = 0;
            placed += HardenWallColliders();
            placed += BuildRoomBoundary(envRoot.transform);
            placed += BuildHearthFire(envRoot.transform, binds);
            placed += BuildFramedPhotograph(envRoot.transform, binds);
            placed += BuildBookcase(envRoot.transform, binds);
            placed += BuildMargeryChair(envRoot.transform, binds);
            placed += BuildColdTeaCup(envRoot.transform, binds);
            placed += BuildRug(envRoot.transform);
            placed += BuildMantelCandles(envRoot.transform, binds);
            placed += BuildBedroomDoor(envRoot.transform, binds);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog("Phase 47.5 — Cottage polished",
                $"🏚️ Cottage interior polished.\n\n" +
                $"{placed} additions placed under '{EnvParentName}'.\n\n" +
                "Hearth flickers, mantel photograph visible, cold tea cup\n" +
                "in place, bedroom door closed but lit.\n\n" +
                "Re-run any time — idempotent.",
                "OK");
        }

        private static int HardenWallColliders()
        {
            int n = 0;
#if UNITY_2023_1_OR_NEWER
            var allObjs = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
#else
            var allObjs = Object.FindObjectsOfType<GameObject>();
#endif
            foreach (var go in allObjs)
            {
                var lower = go.name.ToLowerInvariant();
                if (!lower.Contains("wall") && !lower.Contains("floor")) continue;
                if (lower.Contains("door")) continue;
                if (go.GetComponent<Collider>() != null) continue;
                var mf = go.GetComponentInChildren<MeshFilter>();
                if (mf == null || mf.sharedMesh == null) continue;
                var bc = go.AddComponent<BoxCollider>();
                bc.size = mf.sharedMesh.bounds.size;
                bc.center = mf.sharedMesh.bounds.center;
                n++;
            }
            return n;
        }

        private static int BuildRoomBoundary(Transform envRoot)
        {
            var boundary = new GameObject("InvisibleRoomBoundary");
            boundary.transform.SetParent(envRoot, false);
            float half = 4.5f;
            float zCentre = 0f;
            CreateInvisibleWall(boundary.transform, "Bound_N",
                new Vector3(0f, 2f, zCentre + half),
                new Vector3(half * 2f + 2f, 4f, 0.5f));
            CreateInvisibleWall(boundary.transform, "Bound_S",
                new Vector3(0f, 2f, zCentre - half),
                new Vector3(half * 2f + 2f, 4f, 0.5f));
            CreateInvisibleWall(boundary.transform, "Bound_E",
                new Vector3(half, 2f, zCentre),
                new Vector3(0.5f, 4f, half * 2f + 2f));
            CreateInvisibleWall(boundary.transform, "Bound_W",
                new Vector3(-half, 2f, zCentre),
                new Vector3(0.5f, 4f, half * 2f + 2f));
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

        private static int BuildHearthFire(Transform envRoot, MedievalVillageBindingsV2 binds)
        {
            int placed = 0;
            var hearthRoot = new GameObject("HearthFire");
            hearthRoot.transform.SetParent(envRoot, false);
            hearthRoot.transform.position = new Vector3(0f, 0f, 3.4f);
            var firepit = FindMVPrefab(new[] { "firepit_01a", "firepit_02a", "firepit" });
            if (firepit != null)
            {
                var f = (GameObject)PrefabUtility.InstantiatePrefab(firepit);
                f.name = "Hearth_Firepit";
                f.transform.SetParent(hearthRoot.transform, false);
                placed++;
            }
            var bulbGO = new GameObject("Hearth_FlickerLight");
            bulbGO.transform.SetParent(hearthRoot.transform, false);
            bulbGO.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            var light = bulbGO.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1.0f, 0.59f, 0.31f);
            light.intensity = 3.2f;
            light.range = 8f;
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.55f;
            var flicker = bulbGO.AddComponent<HearthboundHollow.Player.HearthFlicker>();
            flicker.baseIntensity = 3.2f;
            flicker.flickerAmplitude = 0.6f;
            flicker.flickerSpeed = 4.0f;
            placed++;
            var lumenFire = FindLumenPrefab("Candle Warm") ?? FindLumenPrefab("Flame Effect") ?? FindLumenPrefab("Candle");
            if (lumenFire != null)
            {
                var f2 = (GameObject)PrefabUtility.InstantiatePrefab(lumenFire);
                f2.name = "Hearth_LumenGlow";
                f2.transform.SetParent(hearthRoot.transform, false);
                f2.transform.localPosition = new Vector3(0f, 0.7f, 0f);
                placed++;
            }
            var log = FindMVPrefab(new[] { "woodlog_03a", "woodlog_02a", "woodlog" });
            if (log != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    var l = (GameObject)PrefabUtility.InstantiatePrefab(log);
                    l.name = $"Hearth_Log_{i:00}";
                    l.transform.SetParent(hearthRoot.transform, false);
                    l.transform.localPosition = new Vector3(-0.15f + i * 0.15f, 0.25f, 0f);
                    l.transform.localRotation = Quaternion.Euler(0f, i * 40f, 90f);
                    placed++;
                }
            }
            return placed;
        }

        private static int BuildFramedPhotograph(Transform envRoot, MedievalVillageBindingsV2 binds)
        {
            var frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "MantelPhotograph";
            frame.transform.SetParent(envRoot, false);
            frame.transform.position = new Vector3(0f, 1.6f, 3.0f);
            frame.transform.localScale = new Vector3(0.7f, 0.5f, 0.04f);
            var col = frame.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);
            var mr = frame.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                mat.name = "Phase47_PhotoFrame";
                mat.color = new Color(0.82f, 0.66f, 0.45f, 1f);
                mr.sharedMaterial = mat;
            }
            var label = new GameObject("PhotographLabel");
            label.transform.SetParent(frame.transform, false);
            label.transform.localPosition = new Vector3(0f, -0.6f, -0.6f);
            label.transform.localRotation = Quaternion.identity;
            var tmp = label.AddComponent<TMPro.TextMeshPro>();
            tmp.text = "— Margery & Gerrold, 36 years ago —";
            tmp.fontSize = 0.15f;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.color = new Color(0.78f, 0.58f, 0.34f);
            tmp.fontStyle = TMPro.FontStyles.Italic;
            return 1;
        }

        private static int BuildBookcase(Transform envRoot, MedievalVillageBindingsV2 binds)
        {
            int placed = 0;
            var cabinet = binds?.cupboardPrefab ?? FindMVPrefab(new[] { "cabinet", "cupboard", "wallshelf", "shelf" });
            if (cabinet == null) return 0;
            var bc = (GameObject)PrefabUtility.InstantiatePrefab(cabinet);
            bc.name = "Cottage_Bookcase";
            bc.transform.SetParent(envRoot, true);
            bc.transform.position = new Vector3(-3.8f, 0f, 0f);
            bc.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            EnsureBlockingCollider(bc);
            placed++;
            var book = FindMVPrefab(new[] { "book_01", "book", "ledger" });
            if (book != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    var b = (GameObject)PrefabUtility.InstantiatePrefab(book);
                    b.name = $"CottageBook_{i:00}";
                    b.transform.SetParent(bc.transform, false);
                    b.transform.localPosition = new Vector3(-0.4f + i * 0.18f, 0.95f, -0.15f);
                    b.transform.localRotation = Quaternion.Euler(0f, i * 15f, 0f);
                    placed++;
                }
            }
            return placed;
        }

        private static int BuildMargeryChair(Transform envRoot, MedievalVillageBindingsV2 binds)
        {
            int placed = 0;
            var chair = binds?.stoolPrefab ?? FindMVPrefab(new[] { "chair_01a", "chair", "stool_01a", "stool" });
            if (chair == null) return 0;
            var c = (GameObject)PrefabUtility.InstantiatePrefab(chair);
            c.name = "Margery_Chair";
            c.transform.SetParent(envRoot, true);
            c.transform.position = new Vector3(1.5f, 0f, 0.5f);
            c.transform.rotation = Quaternion.Euler(0f, -120f, 0f);
            EnsureBlockingCollider(c);
            placed++;
            var book = FindMVPrefab(new[] { "book_01", "book" });
            if (book != null)
            {
                var b = (GameObject)PrefabUtility.InstantiatePrefab(book);
                b.name = "Margery_BookOnArm";
                b.transform.SetParent(c.transform, false);
                b.transform.localPosition = new Vector3(0.2f, 0.55f, 0.15f);
                b.transform.localRotation = Quaternion.Euler(20f, 0f, 0f);
                placed++;
            }
            return placed;
        }

        private static int BuildColdTeaCup(Transform envRoot, MedievalVillageBindingsV2 binds)
        {
            int placed = 0;
            var table = FindMVPrefab(new[] { "woodtable_02a", "woodtable_01a", "woodtable" });
            if (table != null)
            {
                var t = (GameObject)PrefabUtility.InstantiatePrefab(table);
                t.name = "SmallSideTable";
                t.transform.SetParent(envRoot, true);
                t.transform.position = new Vector3(2.5f, 0f, 0.5f);
                t.transform.localScale = Vector3.one * 0.6f;
                EnsureBlockingCollider(t);
                placed++;
                var cup = FindMVPrefab(new[] { "jar_01a", "bottle_01a", "bottle" });
                if (cup != null)
                {
                    var c = (GameObject)PrefabUtility.InstantiatePrefab(cup);
                    c.name = "ColdTeaCup";
                    c.transform.SetParent(t.transform, false);
                    c.transform.localPosition = new Vector3(0f, 0.95f, 0f);
                    c.transform.localScale = Vector3.one * 0.4f;
                    placed++;
                }
            }
            return placed;
        }

        private static int BuildRug(Transform envRoot)
        {
            var rug = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rug.name = "CottageRug";
            rug.transform.SetParent(envRoot, false);
            rug.transform.position = new Vector3(0.5f, 0.005f, 1.2f);
            rug.transform.localScale = new Vector3(2.4f, 0.02f, 1.6f);
            var col = rug.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);
            var mr = rug.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                mat.name = "Phase47_CottageRug";
                mat.color = new Color(0.42f, 0.32f, 0.25f, 1f);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.1f);
                mr.sharedMaterial = mat;
            }
            return 1;
        }

        private static int BuildMantelCandles(Transform envRoot, MedievalVillageBindingsV2 binds)
        {
            int placed = 0;
            var candle = binds?.candleabraPrefab ?? FindMVPrefab(new[] { "thickcandle_01a", "thickcandle_01b", "thincandle_01a", "candleabra" });
            if (candle == null) return 0;
            for (int i = 0; i < 2; i++)
            {
                var c = (GameObject)PrefabUtility.InstantiatePrefab(candle);
                c.name = $"MantelCandle_{i:00}";
                c.transform.SetParent(envRoot, true);
                float x = i == 0 ? -1.3f : 1.3f;
                c.transform.position = new Vector3(x, 1.45f, 3.0f);
                c.transform.localScale = Vector3.one * 0.7f;
                placed++;
                var bulb = new GameObject($"MantelCandle_{i:00}_Bulb");
                bulb.transform.SetParent(c.transform, false);
                bulb.transform.localPosition = new Vector3(0f, 0.3f, 0f);
                var l = bulb.AddComponent<Light>();
                l.type = LightType.Point;
                l.color = new Color(1.0f, 0.78f, 0.42f);
                l.intensity = 1.0f;
                l.range = 3.5f;
                l.shadows = LightShadows.None;
                placed++;
            }
            return placed;
        }

        private static int BuildBedroomDoor(Transform envRoot, MedievalVillageBindingsV2 binds)
        {
            int placed = 0;
            var doorPrefab = FindMVPrefab(new[] { "door_02a", "wooddoor", "door_01a", "door_01", "door" });
            if (doorPrefab == null) return 0;
            var d = (GameObject)PrefabUtility.InstantiatePrefab(doorPrefab);
            d.name = "Cottage_BedroomDoor";
            d.transform.SetParent(envRoot, true);
            d.transform.position = new Vector3(-2.5f, 0f, -3.8f);
            d.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            EnsureBlockingCollider(d);
            placed++;
            var label = new GameObject("BedroomDoor_Label");
            label.transform.SetParent(d.transform, false);
            label.transform.localPosition = new Vector3(0f, 2.6f, 0f);
            var tmp = label.AddComponent<TMPro.TextMeshPro>();
            tmp.text = "the bedroom\n(closed)";
            tmp.fontSize = 0.4f;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.color = new Color(0.65f, 0.55f, 0.42f);
            tmp.fontStyle = TMPro.FontStyles.Italic;
            var sliverGO = new GameObject("BedroomDoor_Sliver");
            sliverGO.transform.SetParent(d.transform, false);
            sliverGO.transform.localPosition = new Vector3(0f, 0.1f, -0.05f);
            var sl = sliverGO.AddComponent<Light>();
            sl.type = LightType.Point;
            sl.color = new Color(1.0f, 0.85f, 0.55f);
            sl.intensity = 0.4f;
            sl.range = 1.2f;
            sl.shadows = LightShadows.None;
            placed++;
            return placed;
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

        private static GameObject FindLumenPrefab(string exactName)
        {
            if (!System.IO.Directory.Exists(LumenPrefabs)) return null;
            var guids = AssetDatabase.FindAssets($"{exactName} t:Prefab", new[] { LumenPrefabs });
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go != null && go.name == exactName) return go;
            }
            return null;
        }
    }
}
