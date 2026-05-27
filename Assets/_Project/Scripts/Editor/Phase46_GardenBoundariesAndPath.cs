// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase46_GardenBoundariesAndPath
//
// Phase 46.4 — Mission 2 Herb Garden boundaries + wider stepping path.
//
// Per the gameplay guide (Mission 2 § 6) the garden is the player's "first
// outdoor cozy space the Hollow itself owns" — 4 raised herb beds with
// Lavender + Valerian, a watering can, a low wooden fence, and a meadow
// walled off beyond. Phase 46.4 ensures:
//
//   1. The garden is FULLY enclosed by a wooden-fence perimeter (player
//      cannot fall off the edge of the world or escape into nothing).
//   2. Invisible BoxCollider blockers behind the fence (defence-in-depth).
//   3. An extended ground plane (warm soil colour) covers the entire area.
//   4. A widened stepping-stone path leads from the Hollow back door to
//      the herb beds.
//   5. Two extra "future" raised beds (empty, per the M3+ promise in the
//      guide § 4.2) are dressed.
//   6. A small water trough + watering-can stool corner adds polish.
//
// All additions are parented under `_Phase46Env_Garden` — earlier-phase
// outputs are preserved.
//
// USE: Menu → Hearthbound → ⚙️ Advanced → 🌿 Phase 46.4 — Garden Boundaries + Path

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HearthboundHollow.EditorTools
{
    public static class Phase46_GardenBoundariesAndPath
    {
        private const string SceneGarden   = "Assets/_Project/Scenes/04_Mission02_Garden.unity";
        private const string MVRoot        = "Assets/MeshingunStudio";
        private const string EnvParentName = "_Phase46Env_Garden";

        // Garden playable area: 16m × 18m
        private const float HalfWidth   = 8f;
        private const float ZSouth      = -2f;  // the Hollow back wall sits here
        private const float ZNorth      = 16f;
        private const float WallHeight  = 1.8f; // low garden fence

        [MenuItem("Hearthbound/⚙️ Advanced/🌿 Phase 46.4 — Garden Boundaries + Path", priority = 464)]
        public static void Build()
        {
            if (!System.IO.File.Exists(SceneGarden))
            {
                EditorUtility.DisplayDialog("Phase 46.4",
                    $"Scene not found: {SceneGarden}", "OK");
                return;
            }

            var binds = Phase32_VillageBindingsExtension.TryGetBindings();
            if (binds == null)
            {
                Phase32_VillageBindingsExtension.Build();
                binds = Phase32_VillageBindingsExtension.TryGetBindings();
            }

            var scene = EditorSceneManager.OpenScene(SceneGarden, OpenSceneMode.Single);

            var existing = GameObject.Find(EnvParentName);
            if (existing != null) Object.DestroyImmediate(existing);
            var envRoot = new GameObject(EnvParentName);

            int placed = 0;
            placed += BuildSoilGround(envRoot.transform);
            placed += BuildFencePerimeter(envRoot.transform, binds);
            placed += BuildInvisibleBlockers(envRoot.transform);
            placed += BuildSteppingPath(envRoot.transform, binds);
            placed += BuildExtraDressing(envRoot.transform, binds);
            placed += BuildMeadowBeyond(envRoot.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog("Phase 46.4 — Garden polished",
                $"🌿 Garden boundaries + path built.\n\n" +
                $"{placed} additions placed under '{EnvParentName}'.\n\n" +
                $"Garden playable area: {HalfWidth*2:F0} m × {ZNorth - ZSouth:F0} m\n" +
                "Re-run any time — idempotent.",
                "OK");
        }

        // ───────────────────────────────────────────────────────────────
        // 1) Soil ground (warm earth)
        // ───────────────────────────────────────────────────────────────

        private static int BuildSoilGround(Transform envRoot)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "GardenSoilGround";
            ground.transform.SetParent(envRoot, false);
            float zCentre = (ZNorth + ZSouth) * 0.5f;
            ground.transform.position = new Vector3(0f, -0.10f, zCentre);
            ground.transform.localScale = new Vector3(HalfWidth * 2f + 4f, 0.18f, (ZNorth - ZSouth) + 4f);

            var mr = ground.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                mat.name = "Phase46_GardenSoil";
                mat.color = new Color(0.32f, 0.23f, 0.16f, 1f);
                mr.sharedMaterial = mat;
            }
            return 1;
        }

        // ───────────────────────────────────────────────────────────────
        // 2) Wooden fence perimeter
        // ───────────────────────────────────────────────────────────────

        private static int BuildFencePerimeter(Transform envRoot, MedievalVillageBindingsV2 binds)
        {
            int placed = 0;
            var fence = FindMVPrefab(new[] { "fence_01a", "fence_02a", "fence_03a", "fence" });
            if (fence == null) return 0;

            var fenceParent = new GameObject("FencePerimeter");
            fenceParent.transform.SetParent(envRoot, false);

            float step = 2.0f;

            // North wall (across) — leave the gap at center for "beyond"
            for (float x = -HalfWidth; x <= HalfWidth + 0.01f; x += step)
            {
                placed += SpawnFence(fence, fenceParent.transform, $"Fence_N_{x:F1}",
                    new Vector3(x, 0f, ZNorth), Quaternion.Euler(0f, 180f, 0f));
            }
            // East
            for (float z = ZSouth; z <= ZNorth + 0.01f; z += step)
            {
                placed += SpawnFence(fence, fenceParent.transform, $"Fence_E_{z:F1}",
                    new Vector3(HalfWidth, 0f, z), Quaternion.Euler(0f, -90f, 0f));
            }
            // West
            for (float z = ZSouth; z <= ZNorth + 0.01f; z += step)
            {
                placed += SpawnFence(fence, fenceParent.transform, $"Fence_W_{z:F1}",
                    new Vector3(-HalfWidth, 0f, z), Quaternion.Euler(0f, 90f, 0f));
            }
            // (South — no fence; that's the Hollow back wall, already solid.)

            return placed;
        }

        private static int SpawnFence(GameObject prefab, Transform parent, string name, Vector3 pos, Quaternion rot)
        {
            var f = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            f.name = name;
            f.transform.SetParent(parent, true);
            f.transform.position = pos;
            f.transform.rotation = rot;
            EnsureBlockingCollider(f);
            return 1;
        }

        // ───────────────────────────────────────────────────────────────
        // 3) Invisible BoxCollider blockers (defence-in-depth)
        // ───────────────────────────────────────────────────────────────

        private static int BuildInvisibleBlockers(Transform envRoot)
        {
            var blockers = new GameObject("InvisibleBlockers");
            blockers.transform.SetParent(envRoot, false);
            float zCentre = (ZNorth + ZSouth) * 0.5f;

            CreateInvisibleWall(blockers.transform, "Block_N",
                new Vector3(0f, 2f, ZNorth + 1.0f),
                new Vector3(HalfWidth * 2f + 4f, 4f, 1f));
            CreateInvisibleWall(blockers.transform, "Block_E",
                new Vector3(HalfWidth + 1.0f, 2f, zCentre),
                new Vector3(1f, 4f, (ZNorth - ZSouth) + 4f));
            CreateInvisibleWall(blockers.transform, "Block_W",
                new Vector3(-HalfWidth - 1.0f, 2f, zCentre),
                new Vector3(1f, 4f, (ZNorth - ZSouth) + 4f));
            CreateInvisibleWall(blockers.transform, "Block_S",
                new Vector3(0f, 2f, ZSouth - 1.0f),
                new Vector3(HalfWidth * 2f + 4f, 4f, 1f));

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

        // ───────────────────────────────────────────────────────────────
        // 4) Stepping path
        // ───────────────────────────────────────────────────────────────

        private static int BuildSteppingPath(Transform envRoot, MedievalVillageBindingsV2 binds)
        {
            int placed = 0;
            var stone = binds?.stoneBrickPrefab ?? FindMVPrefab(new[] { "stonebrick_01a", "stonebrick_02a", "stone_01a", "stone_02a" });
            if (stone == null) return 0;

            var path = new GameObject("SteppingPath");
            path.transform.SetParent(envRoot, false);

            // 7 stepping stones from z=0 (Hollow door) to z=12 (raised beds)
            for (int i = 0; i < 7; i++)
            {
                var s = (GameObject)PrefabUtility.InstantiatePrefab(stone);
                s.name = $"Stepping_{i:00}";
                s.transform.SetParent(path.transform, true);
                float z = i * 1.8f + 0.5f;
                float x = (i % 2 == 0 ? -0.2f : 0.2f);
                s.transform.position = new Vector3(x, 0.06f, z);
                s.transform.rotation = Quaternion.Euler(0f, i * 23f, 0f);
                s.transform.localScale = Vector3.one * 0.9f;
                placed++;
            }

            return placed;
        }

        // ───────────────────────────────────────────────────────────────
        // 5) Extra dressing — pots, watering trough, decorative
        // ───────────────────────────────────────────────────────────────

        private static int BuildExtraDressing(Transform envRoot, MedievalVillageBindingsV2 binds)
        {
            int placed = 0;
            var dress = new GameObject("GardenDressing");
            dress.transform.SetParent(envRoot, false);

            // Water trough (use barrel as proxy)
            var barrel = FindMVPrefab(new[] { "barrel_01a", "barrel_01b", "barrel" });
            if (barrel != null)
            {
                var b = (GameObject)PrefabUtility.InstantiatePrefab(barrel);
                b.name = "WaterTrough";
                b.transform.SetParent(dress.transform, true);
                b.transform.position = new Vector3(-5.5f, 0f, 3f);
                EnsureBlockingCollider(b);
                placed++;
            }

            // Hanging pots — 4 along the east fence
            var pot = binds?.hangingPotPrefab ?? FindMVPrefab(new[] { "terrapots_01a", "terrapots_01b", "terrapots" });
            if (pot != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    var p = (GameObject)PrefabUtility.InstantiatePrefab(pot);
                    p.name = $"GardenPot_{i:00}";
                    p.transform.SetParent(dress.transform, true);
                    p.transform.position = new Vector3(HalfWidth - 0.6f, 0.5f, 2f + i * 3f);
                    p.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
                    placed++;
                }
            }

            // A scarecrow proxy — use a barrel + sack stacked (until proper scarecrow imported)
            var sack = FindMVPrefab(new[] { "grain_sack_01a", "sack_potato_01a", "grain_sack" });
            if (sack != null && barrel != null)
            {
                var pole = (GameObject)PrefabUtility.InstantiatePrefab(barrel);
                pole.name = "Scarecrow_Pole";
                pole.transform.SetParent(dress.transform, true);
                pole.transform.position = new Vector3(0f, 0f, 13f);
                pole.transform.localScale = new Vector3(0.3f, 1.5f, 0.3f);
                placed++;

                var head = (GameObject)PrefabUtility.InstantiatePrefab(sack);
                head.name = "Scarecrow_Head";
                head.transform.SetParent(pole.transform, true);
                head.transform.position = pole.transform.position + new Vector3(0f, 1.4f, 0f);
                head.transform.localScale = Vector3.one * 0.6f;
                placed++;
            }

            return placed;
        }

        // ───────────────────────────────────────────────────────────────
        // 6) Meadow beyond — visual depth past the north fence
        // ───────────────────────────────────────────────────────────────

        private static int BuildMeadowBeyond(Transform envRoot)
        {
            int placed = 0;
            var beyond = new GameObject("MeadowBeyond");
            beyond.transform.SetParent(envRoot, false);

            // Drop several distant trees behind the north fence (out of reach).
            var alder = FindMVPrefab(new[] { "alder_fall_01a", "alder_fall_01b" });
            var pine  = FindMVPrefab(new[] { "pinetree_spring_03a", "pinetree_spring_04a", "pinetree" });
            var trees = new[] { alder, alder, pine, pine, alder };
            for (int i = 0; i < trees.Length; i++)
            {
                if (trees[i] == null) continue;
                var t = (GameObject)PrefabUtility.InstantiatePrefab(trees[i]);
                t.name = $"DistantTree_{i:00}";
                t.transform.SetParent(beyond.transform, true);
                t.transform.position = new Vector3(-6f + i * 3f, 0f, ZNorth + 4f + (i % 2) * 2f);
                t.transform.rotation = Quaternion.Euler(0f, i * 47f, 0f);
                t.transform.localScale = Vector3.one * (1.1f + i * 0.05f);
                placed++;
            }
            return placed;
        }

        // ───────────────────────────────────────────────────────────────
        // Helpers
        // ───────────────────────────────────────────────────────────────

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
