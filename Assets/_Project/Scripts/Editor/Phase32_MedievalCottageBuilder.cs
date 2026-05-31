// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase32_MedievalCottageBuilder
//
// Phase 32.1 — Modular Cottage Assembler.
//
// The Medieval Village Megapack from MeshingunStudio ships *modular* building
// pieces (walls, windows, doors, roof tiles, chimneys) — NOT complete houses.
// Phase 15's fuzzy "find a house prefab" search therefore falls back to
// SM_ShopStand_01a, which is a market stand, not a cottage. This makes the
// Lane scene read as a market square rather than a residential village.
//
// Phase 32.1 fixes that. It assembles complete cottage prefabs from MV's
// modular kit and saves them under Assets/_Project/Prefabs/Environment/.
// Phase 32.2 (Lane v2) then instantiates these along the lane to give the
// player a real residential village.
//
// FOUR COTTAGE VARIANTS (different rotation/window layouts so the lane
// reads as hand-authored, not copy-pasted):
//   • Cottage_A_Bakery   — Doris's bakery, has a "Bakery" sign + chimney
//   • Cottage_B          — neutral cottage with two windows
//   • Cottage_C          — narrower cottage, gabled, one window
//   • Cottage_D          — corner cottage with door + flag banner
//
// Each cottage is ~4×4 m footprint, 3 m tall walls + pitched roof, built
// from these MV prefabs:
//   • SM_Wall_01d_1            — solid wall segment (each side)
//   • SM_WallWindow_01a_1      — wall with window cutout (front/back)
//   • SM_WallDoor_03a          — wall with door cutout (for Cottage_D)
//   • SM_Rooftiles_01a..p      — angled roof slabs (we pick 2 best-shape ones)
//   • SM_Chimney_01a           — chimney prop on the roof
//   • SM_Floor_04x04_02a       — inner floor (so the cottage isn't hollow)
//
// USE: Menu → Hearthbound → 🏘️ Phase 32.1 — Assemble Cottage Prefabs
//
// Architecture notes:
//   - Per D-007, .prefab files generated here ARE committed to git (they're
//     project authored content, not Unity scene state).
//   - Per D-027, each cottage is a single self-contained prefab. The Lane
//     v2 builder (Phase 32.2) just instantiates them at the right positions
//     with the right rotations — zero per-instance hierarchy work needed.
//   - Per D-035, this is Editor-only — no runtime asmdef concerns.

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HearthboundHollow.EditorTools
{
    public static class Phase32_MedievalCottageBuilder
    {
        private const string MVRoot      = "Assets/MeshingunStudio";
        private const string OutputDir   = "Assets/_Project/Prefabs/Environment";

        // The four cottage variants we author.
        public static readonly string[] CottageVariants =
        {
            "Cottage_A_Bakery",
            "Cottage_B_Plain",
            "Cottage_C_Gabled",
            "Cottage_D_Corner",
        };

        [MenuItem("Hearthbound/⚙️ Advanced/🏘️ Phase 32.1 — Assemble Cottage Prefabs", priority = 31)]
        public static void Build()
        {
            if (!AssetDatabase.IsValidFolder(MVRoot))
            {
                EditorUtility.DisplayDialog("Phase 32.1 — Medieval Village not found",
                    "Cannot find Assets/MeshingunStudio/.\n\n" +
                    "Import the Medieval Village Megapack first, then re-run this menu item.",
                    "OK");
                return;
            }

            EnsureFolder(OutputDir);

            int built = 0;
            built += BuildBakeryCottage();
            built += BuildPlainCottage();
            built += BuildGabledCottage();
            built += BuildCornerCottage();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Phase 32.1 — Cottage prefabs assembled",
                $"✨ {built}/{CottageVariants.Length} cottage prefab(s) saved to:\n" +
                $"   {OutputDir}/\n\n" +
                string.Join("\n", CottageVariants) + "\n\n" +
                "Next step: Hearthbound → 🏘️ Phase 32.2 — Polish Lane Environment V2 " +
                "(uses these prefabs to populate the village).",
                "OK");
        }

        public static GameObject TryGetCottagePrefab(string variantName)
        {
            var path = $"{OutputDir}/{variantName}.prefab";
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        public static GameObject[] TryGetAllCottagePrefabs()
        {
            var list = new List<GameObject>();
            foreach (var v in CottageVariants)
            {
                var p = TryGetCottagePrefab(v);
                if (p != null) list.Add(p);
            }
            return list.ToArray();
        }

        // ───────────────────────────────────────────────────────────────
        // Cottage A — Doris's Bakery (has "Bakery" sign + chimney + smoke)
        // ───────────────────────────────────────────────────────────────

        private static int BuildBakeryCottage()
        {
            var root = new GameObject("Cottage_A_Bakery");
            try
            {
                AddFloor(root.transform, "Floor", Vector3.zero);

                AddSolidWall(root.transform, "Wall_W",     new Vector3(-2.0f, 0f,  0.0f), 90f);
                AddSolidWall(root.transform, "Wall_W2",    new Vector3(-2.0f, 0f,  2.0f), 90f);
                AddWindowWall(root.transform, "Wall_S_Win", new Vector3( 0.0f, 0f, -2.0f), 0f);
                AddSolidWall(root.transform, "Wall_S2",    new Vector3( 2.0f, 0f, -2.0f), 0f);
                AddSolidWall(root.transform, "Wall_E",     new Vector3( 2.0f, 0f,  0.0f), -90f);
                AddSolidWall(root.transform, "Wall_E2",    new Vector3( 2.0f, 0f,  2.0f), -90f);
                AddWindowWall(root.transform, "Wall_N_Win", new Vector3( 0.0f, 0f,  4.0f), 180f);
                AddSolidWall(root.transform, "Wall_N2",    new Vector3(-2.0f, 0f,  4.0f), 180f);

                AddRoof(root.transform, "Roof", new Vector3(0f, 3.0f, 1f));
                AddChimney(root.transform, "Chimney", new Vector3(-1.4f, 4.4f, -0.4f));

                AddBakeryText(root.transform, "BakerySign",
                    new Vector3(0f, 2.4f, -2.05f),
                    Quaternion.Euler(0, 0, 0));

                AddDecoratorProp(root.transform, "FrontBread_01",
                    new[] { "bread_01a", "bread" },
                    new Vector3(-0.45f, 1.45f, -1.95f), Quaternion.Euler(0, 5, 0));
                AddDecoratorProp(root.transform, "FrontBread_02",
                    new[] { "bread_01a", "bread" },
                    new Vector3( 0.25f, 1.45f, -1.95f), Quaternion.Euler(0, -3, 0));

                return SaveAsPrefab(root, "Cottage_A_Bakery");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static int BuildPlainCottage()
        {
            var root = new GameObject("Cottage_B_Plain");
            try
            {
                AddFloor(root.transform, "Floor", Vector3.zero);

                AddSolidWall(root.transform, "Wall_W",      new Vector3(-2.0f, 0f, 0.0f), 90f);
                AddSolidWall(root.transform, "Wall_W2",     new Vector3(-2.0f, 0f, 2.0f), 90f);
                AddWindowWall(root.transform, "Wall_S_Win", new Vector3( 0.0f, 0f, -2.0f), 0f);
                AddWindowWall(root.transform, "Wall_S2",    new Vector3( 2.0f, 0f, -2.0f), 0f);
                AddSolidWall(root.transform, "Wall_E",      new Vector3( 2.0f, 0f, 0.0f), -90f);
                AddSolidWall(root.transform, "Wall_E2",     new Vector3( 2.0f, 0f, 2.0f), -90f);
                AddSolidWall(root.transform, "Wall_N",      new Vector3( 0.0f, 0f, 4.0f), 180f);
                AddSolidWall(root.transform, "Wall_N2",     new Vector3(-2.0f, 0f, 4.0f), 180f);

                AddRoof(root.transform, "Roof", new Vector3(0f, 3.0f, 1f));
                AddChimney(root.transform, "Chimney", new Vector3(1.4f, 4.4f, 2.4f));

                AddDecoratorProp(root.transform, "FrontFlag",
                    new[] { "flag_gurland", "banner_variant_01" },
                    new Vector3(1.4f, 2.6f, -2.06f), Quaternion.identity);

                return SaveAsPrefab(root, "Cottage_B_Plain");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static int BuildGabledCottage()
        {
            var root = new GameObject("Cottage_C_Gabled");
            try
            {
                AddFloor(root.transform, "Floor", Vector3.zero);

                AddSolidWall(root.transform, "Wall_W",      new Vector3(-2.0f, 0f, 0.0f), 90f);
                AddSolidWall(root.transform, "Wall_W2",     new Vector3(-2.0f, 0f, 2.0f), 90f);
                AddSolidWall(root.transform, "Wall_S",      new Vector3( 0.0f, 0f, -2.0f), 0f);
                AddWindowWall(root.transform, "Wall_S_Win", new Vector3( 2.0f, 0f, -2.0f), 0f);
                AddSolidWall(root.transform, "Wall_E",      new Vector3( 2.0f, 0f, 0.0f), -90f);
                AddSolidWall(root.transform, "Wall_E2",     new Vector3( 2.0f, 0f, 2.0f), -90f);
                AddSolidWall(root.transform, "Wall_N",      new Vector3( 0.0f, 0f, 4.0f), 180f);
                AddSolidWall(root.transform, "Wall_N2",     new Vector3(-2.0f, 0f, 4.0f), 180f);

                var triangle = FindMVPrefab(new[] { "wall_triangle_a", "wall_triangle" });
                if (triangle != null)
                {
                    SpawnAt(triangle, root.transform, "Wall_Gable_S",
                        new Vector3(0f, 3.0f, -2.0f), Quaternion.identity);
                }

                AddRoof(root.transform, "Roof", new Vector3(0f, 3.0f, 1f));
                AddChimney(root.transform, "Chimney", new Vector3(0f, 4.4f, 3.4f));

                return SaveAsPrefab(root, "Cottage_C_Gabled");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static int BuildCornerCottage()
        {
            var root = new GameObject("Cottage_D_Corner");
            try
            {
                AddFloor(root.transform, "Floor", Vector3.zero);

                AddSolidWall(root.transform, "Wall_W",       new Vector3(-2.0f, 0f, 0.0f), 90f);
                AddSolidWall(root.transform, "Wall_W2",      new Vector3(-2.0f, 0f, 2.0f), 90f);

                AddDoorWall(root.transform, "Wall_S_Door", new Vector3(0.0f, 0f, -2.0f), 0f);
                AddSolidWall(root.transform, "Wall_S2",      new Vector3( 2.0f, 0f, -2.0f), 0f);

                AddSolidWall(root.transform, "Wall_E",       new Vector3( 2.0f, 0f, 0.0f), -90f);
                AddWindowWall(root.transform, "Wall_E_Win",  new Vector3( 2.0f, 0f, 2.0f), -90f);
                AddSolidWall(root.transform, "Wall_N",       new Vector3( 0.0f, 0f, 4.0f), 180f);
                AddSolidWall(root.transform, "Wall_N2",      new Vector3(-2.0f, 0f, 4.0f), 180f);

                AddRoof(root.transform, "Roof", new Vector3(0f, 3.0f, 1f));
                AddChimney(root.transform, "Chimney", new Vector3(-1.4f, 4.4f, 2.4f));

                AddDecoratorProp(root.transform, "Garland",
                    new[] { "flag_gurland_01", "flag_gurland" },
                    new Vector3(0f, 2.8f, -2.06f), Quaternion.identity);

                return SaveAsPrefab(root, "Cottage_D_Corner");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        // ───────────────────────────────────────────────────────────────
        // Builder helpers — wall pieces
        // ───────────────────────────────────────────────────────────────

        private static void AddSolidWall(Transform parent, string name, Vector3 pos, float yaw)
        {
            var prefab = FindMVPrefab(new[] { "wall_01d_1", "wall_01d", "wall_01a", "wall_01" });
            if (prefab == null) return;
            SpawnAt(prefab, parent, name, pos, Quaternion.Euler(0, yaw, 0));
        }

        private static void AddWindowWall(Transform parent, string name, Vector3 pos, float yaw)
        {
            var prefab = FindMVPrefab(new[] { "wallwindow_01a_1", "wallwindow_01a", "wallwindow" });
            if (prefab == null)
            {
                AddSolidWall(parent, name, pos, yaw);
                return;
            }
            SpawnAt(prefab, parent, name, pos, Quaternion.Euler(0, yaw, 0));
        }

        private static void AddDoorWall(Transform parent, string name, Vector3 pos, float yaw)
        {
            var prefab = FindMVPrefab(new[] { "walldoor_03a", "walldoor_t_01a", "walldoor" });
            if (prefab == null)
            {
                AddSolidWall(parent, name, pos, yaw);
                return;
            }
            SpawnAt(prefab, parent, name, pos, Quaternion.Euler(0, yaw, 0));
        }

        private static void AddFloor(Transform parent, string name, Vector3 pos)
        {
            var prefab = FindMVPrefab(new[] { "floor_04x04_02", "floor_04x04", "floor_03x03" });
            if (prefab == null) return;
            SpawnAt(prefab, parent, name, pos, Quaternion.identity);
        }

        // ───────────────────────────────────────────────────────────────
        // Roof + chimney
        // ───────────────────────────────────────────────────────────────

        private static void AddRoof(Transform parent, string name, Vector3 ridgePos)
        {
            var prefab = FindMVPrefab(new[] { "rooftiles_01a", "rooftiles_01b", "rooftiles_01" });
            if (prefab == null) return;

            var roofRoot = new GameObject(name);
            roofRoot.transform.SetParent(parent, false);
            roofRoot.transform.localPosition = ridgePos;

            SpawnAt(prefab, roofRoot.transform, "Roof_W",
                new Vector3(-1.0f, 0f, 1f), Quaternion.Euler(0f, 90f, 25f));
            SpawnAt(prefab, roofRoot.transform, "Roof_E",
                new Vector3(1.0f, 0f, 1f), Quaternion.Euler(0f, -90f, 25f));
        }

        private static void AddChimney(Transform parent, string name, Vector3 pos)
        {
            var prefab = FindMVPrefab(new[] { "chimney_01a", "chimney_01b", "chimney" });
            if (prefab == null) return;
            SpawnAt(prefab, parent, name, pos, Quaternion.identity);
        }

        private static void AddDecoratorProp(Transform parent, string name, string[] keywords,
                                              Vector3 pos, Quaternion rot)
        {
            var prefab = FindMVPrefab(keywords);
            if (prefab == null) return;
            SpawnAt(prefab, parent, name, pos, rot);
        }

        private static void AddBakeryText(Transform parent, string name, Vector3 pos, Quaternion rot)
        {
            var prefab = FindMVPrefab(new[] { "text_bakehouse", "bakehouse", "text_open" });
            if (prefab != null)
            {
                SpawnAt(prefab, parent, name, pos, rot);
                return;
            }

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = rot;
            var tmp = go.AddComponent<TMPro.TextMeshPro>();
            tmp.text = "BAKERY";
            tmp.fontSize = 1.4f;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.color = new Color(0.94f, 0.78f, 0.42f);
            tmp.fontStyle = TMPro.FontStyles.Bold;
        }

        // ───────────────────────────────────────────────────────────────
        // Spawn + prefab IO helpers
        // ───────────────────────────────────────────────────────────────

        private static void SpawnAt(GameObject prefab, Transform parent, string name,
                                     Vector3 pos, Quaternion rot)
        {
            if (prefab == null) return;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = rot;
        }

        private static int SaveAsPrefab(GameObject root, string fileName)
        {
            var path = $"{OutputDir}/{fileName}.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) AssetDatabase.DeleteAsset(path);
            PrefabUtility.SaveAsPrefabAsset(root, path, out var success);
            if (!success)
            {
                Debug.LogError($"[Hearthbound/Phase 32.1] Failed to save prefab '{fileName}' to '{path}'.");
                return 0;
            }
            Debug.Log($"[Hearthbound/Phase 32.1] ✓ Saved cottage prefab → {path}");
            return 1;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            var leaf = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
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
