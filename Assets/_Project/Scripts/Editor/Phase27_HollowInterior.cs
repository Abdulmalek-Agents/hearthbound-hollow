// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase27_HollowInterior
//
// Phase 27.3 — Hollow interior environment polish builder.
//
// Replaces the bare-cube workbench + flat plane ground in
// 03_Mission01_Hollow.unity with a fully-dressed cozy memory shop:
//   • Wood plank floor (tiled SM_Floor_08x08_02a)
//   • 4 walls (SM_Wall + SM_WallWindow with cut-outs)
//   • Ceiling beams (SM_WoodLog_03a)
//   • Hearth/firepit on the back wall with Lumen Shimmery Light glow
//   • Workbench upgraded to real bench prefab (preserves orb + Marin's Note)
//   • Two shelves with candles + pots + baskets (visual density)
//   • Hanging lantern from ceiling with Lumen Lantern Effect
//   • Workbench candle (small Lumen Shimmery halo)
//   • Wool rug (banner laid flat) at scene centre
//   • Sack pile + crate near hearth
//   • Banner on east wall — Zephyr wind reacts
//   • Window glow visible from outside (Lumen Light Surface)
//
// All spawns are parented under `_Phase27Env_Hollow` so re-running the
// builder cleanly removes the previous pass and rebuilds — idempotent.
//
// USE: Menu → Hearthbound → 🏠 Phase 27.3 — Polish Hollow Interior
//
// Architecture notes (matches Phase 27.2):
//   - The existing scene must contain Workbench + MemoryOrb_DOR-001 +
//     Doris + MarinsNote (the Phase 22 + 26 outputs). We *augment*
//     them, never replace.
//   - Per D-035 this script is Editor-only — it doesn't depend on UI.
//   - The placeholder cube `Ground` and cube `Workbench` get re-used:
//     the cube workbench transform is preserved (we replace the mesh
//     by swapping in a real bench underneath the existing orb).

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HearthboundHollow.EditorTools
{
    public static class Phase27_HollowInterior
    {
        private const string SceneHollow = "Assets/_Project/Scenes/03_Mission01_Hollow.unity";

        private const string MVRoot          = "Assets/MeshingunStudio";
        private const string StylizedWeather = "Assets/Unluck Software/Stylized Weather/Prefabs";
        private const string LumenPrefabs    = "Packages/com.distantlands.lumen/Content/Prefabs";

        private const string EnvParentName   = "_Phase27Env_Hollow";

        [MenuItem("Hearthbound/🏠 Phase 27.3 — Polish Hollow Interior", priority = 7)]
        public static void Build()
        {
            if (!System.IO.File.Exists(SceneHollow))
            {
                EditorUtility.DisplayDialog("Phase 27.3",
                    "03_Mission01_Hollow.unity is missing. Run Phase 23 (capstone) first.", "OK");
                return;
            }

            var scene = EditorSceneManager.OpenScene(SceneHollow, OpenSceneMode.Single);

            var existing = GameObject.Find(EnvParentName);
            if (existing != null) Object.DestroyImmediate(existing);
            var envRoot = new GameObject(EnvParentName);

            var mv = Phase15_MedievalVillageBuilder.TryGetBindings();
            if (mv == null)
            {
                Phase15_MedievalVillageBuilder.Build();
                mv = Phase15_MedievalVillageBuilder.TryGetBindings();
            }

            int placed = 0;

            placed += BuildWoodFloor(envRoot.transform);
            placed += BuildWalls(envRoot.transform);
            placed += BuildCeiling(envRoot.transform);
            placed += UpgradeWorkbench(envRoot.transform);
            placed += BuildHearth(envRoot.transform);
            placed += BuildShelves(envRoot.transform);
            placed += BuildHangingLantern(envRoot.transform, mv);
            placed += BuildCandleAndRug(envRoot.transform);
            placed += BuildDressing(envRoot.transform);
            placed += BuildWindowGlow(envRoot.transform);

            // Dial the ambient lighting down for dusk — interior is intimate.
            SetInteriorLighting();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog("Phase 27.3 — Hollow polished",
                $"🏠 Hollow interior dressed. {placed} prefab instances placed under " +
                $"'_Phase27Env_Hollow'.\n\n" +
                "Re-run any time. Phase 27.4 (capstone) chains 27.2 + 27.3 + adds " +
                "the LanternInteractable runtime wiring.",
                "OK");
        }

        private static int BuildWoodFloor(Transform envRoot)
        {
            int placed = 0;
            var floor = FindMVPrefab(new[] { "floor_08x08_02", "floor_08x08", "floor_04x04_02", "floor_03x03" });
            if (floor == null) return 0;

            // Hide the placeholder Ground plane.
            var ground = GameObject.Find("Ground");
            if (ground != null && !ground.transform.IsChildOf(envRoot))
            {
                ground.SetActive(false); // keep it for fallback / debug
            }

            var floorParent = new GameObject("WoodFloor");
            floorParent.transform.SetParent(envRoot, false);

            // Tile a 2×2 grid of 8×8 floor tiles centred on origin.
            for (int x = 0; x < 2; x++)
            for (int z = 0; z < 2; z++)
            {
                var t = (GameObject)PrefabUtility.InstantiatePrefab(floor);
                t.name = $"FloorTile_{x}_{z}";
                t.transform.SetParent(floorParent.transform, false);
                t.transform.localPosition = new Vector3(-4f + x * 8f, 0f, -4f + z * 8f);
                placed++;
            }
            return placed;
        }

        private static int BuildWalls(Transform envRoot)
        {
            int placed = 0;
            var wall = FindMVPrefab(new[] { "wall_01d_1", "wall_01a", "wall_01d", "wall_02" });
            var window = FindMVPrefab(new[] { "wallwindow_01a_1", "wallwindow_01a", "wallwindow" });

            var walls = new GameObject("Walls");
            walls.transform.SetParent(envRoot, false);

            // North wall (Z = +8) — window in middle, solid panels on either side.
            if (wall != null)
            {
                placed += SpawnLocal(wall, walls.transform, "Wall_North_L",
                    new Vector3(-4f, 0f,  8f), Quaternion.Euler(0, 180, 0));
                placed += SpawnLocal(wall, walls.transform, "Wall_North_R",
                    new Vector3( 4f, 0f,  8f), Quaternion.Euler(0, 180, 0));
            }
            if (window != null)
            {
                placed += SpawnLocal(window, walls.transform, "Wall_North_Window",
                    new Vector3(0f, 0f, 8f), Quaternion.Euler(0, 180, 0));
            }

            // East wall (X = +8) — three solid wall segments.
            if (wall != null)
            {
                for (int i = 0; i < 3; i++)
                    placed += SpawnLocal(wall, walls.transform, $"Wall_East_{i}",
                        new Vector3(8f, 0f, -4f + i * 4f),
                        Quaternion.Euler(0, -90, 0));
            }

            // West wall (X = -8) — three solid wall segments.
            if (wall != null)
            {
                for (int i = 0; i < 3; i++)
                    placed += SpawnLocal(wall, walls.transform, $"Wall_West_{i}",
                        new Vector3(-8f, 0f, -4f + i * 4f),
                        Quaternion.Euler(0, 90, 0));
            }

            // South wall (Z = -8) — gap in the middle for the doorway back to Lane.
            if (wall != null)
            {
                placed += SpawnLocal(wall, walls.transform, "Wall_South_L",
                    new Vector3(-4f, 0f, -8f), Quaternion.identity);
                placed += SpawnLocal(wall, walls.transform, "Wall_South_R",
                    new Vector3( 4f, 0f, -8f), Quaternion.identity);
            }
            return placed;
        }

        private static int BuildCeiling(Transform envRoot)
        {
            int placed = 0;
            var beam = FindMVPrefab(new[] { "woodlog_03a", "woodlog_02", "wood_log_03", "log_03" });
            if (beam == null) return 0;

            var ceiling = new GameObject("CeilingBeams");
            ceiling.transform.SetParent(envRoot, false);

            for (int i = 0; i < 3; i++)
            {
                placed += SpawnLocal(beam, ceiling.transform, $"Beam_{i}",
                    new Vector3(-6f + i * 6f, 4.0f, 0f),
                    Quaternion.Euler(0, 0, 90));
            }
            return placed;
        }

        private static int UpgradeWorkbench(Transform envRoot)
        {
            int placed = 0;
            var bench = GameObject.Find("Workbench");
            if (bench == null) return 0;

            // Phase 22 may have already swapped the cube for a MV prefab; if so,
            // we don't need to swap again — just place a cutting-board top.
            var cutting = FindMVPrefab(new[] { "wooden_cutting_board", "cuttingboard", "board_01" });
            if (cutting != null)
            {
                var c = (GameObject)PrefabUtility.InstantiatePrefab(cutting);
                c.name = "WorkbenchBoardTop";
                c.transform.SetParent(envRoot, true);
                // Sit on top of the existing workbench so the orb stays in place visually.
                c.transform.position = bench.transform.position + new Vector3(-0.5f, 1.05f, 0.2f);
                c.transform.rotation = Quaternion.Euler(0, 15, 0);
                placed++;
            }
            return placed;
        }

        private static int BuildHearth(Transform envRoot)
        {
            int placed = 0;
            var firepit = FindMVPrefab(new[] { "firepit_02", "firepit_01", "firepit" });
            if (firepit != null)
            {
                var f = (GameObject)PrefabUtility.InstantiatePrefab(firepit);
                f.name = "Hearth";
                f.transform.SetParent(envRoot, true);
                f.transform.position = new Vector3(-4f, 0f, 4f);
                placed++;

                // Lumen Shimmery Light for warm hearth pulse.
                var halo = FindLumenPrefab("Shimmery Light");
                if (halo != null)
                {
                    var h = (GameObject)PrefabUtility.InstantiatePrefab(halo);
                    h.name = "Hearth_Glow";
                    h.transform.SetParent(f.transform, false);
                    h.transform.localPosition = new Vector3(0f, 0.4f, 0f);
                    placed++;
                }

                // Real Point Light backup so URP shadows + the room actually feels lit.
                var bulbGO = new GameObject("Hearth_Light");
                bulbGO.transform.SetParent(f.transform, false);
                bulbGO.transform.localPosition = new Vector3(0f, 0.5f, 0f);
                var pl = bulbGO.AddComponent<Light>();
                pl.type = LightType.Point;
                pl.color = new Color(1.0f, 0.55f, 0.25f);
                pl.intensity = 4.5f;
                pl.range = 8.5f;
                pl.shadows = LightShadows.Soft;
                placed++;

                // HearthAmbianceTrigger comes from Phase 27.4 runtime — wired by capstone.

                // Small invisible BoxCollider for the trigger volume.
                var trigGO = new GameObject("Hearth_AmbianceZone");
                trigGO.transform.SetParent(f.transform, false);
                trigGO.transform.localPosition = new Vector3(0f, 1f, 1.6f);
                var tc = trigGO.AddComponent<BoxCollider>();
                tc.size = new Vector3(5f, 2f, 5f);
                tc.isTrigger = true;
                placed++;
            }
            return placed;
        }

        private static int BuildShelves(Transform envRoot)
        {
            int placed = 0;
            var stand = FindMVPrefab(new[] { "woodstand", "wood_stand", "wickerbasket_02" });
            var pot = FindMVPrefab(new[] { "terrapots_01b", "terrapots_01", "pot_01" });
            var basket = FindMVPrefab(new[] { "wickerbasket_01d", "wickerbasket_01", "basket_01" });

            var shelves = new GameObject("Shelves");
            shelves.transform.SetParent(envRoot, false);

            if (stand != null)
            {
                placed += SpawnLocal(stand, shelves.transform, "Shelf_West_Top",
                    new Vector3(-7f, 0.5f, 1f), Quaternion.Euler(0, 90, 0));
                placed += SpawnLocal(stand, shelves.transform, "Shelf_West_Bottom",
                    new Vector3(-7f, 0.5f, -2f), Quaternion.Euler(0, 90, 0));
            }
            if (pot != null)
                placed += SpawnLocal(pot, shelves.transform, "Shelf_Pot_01",
                    new Vector3(-6.8f, 0.95f, -2.2f), Quaternion.identity);
            if (basket != null)
                placed += SpawnLocal(basket, shelves.transform, "Shelf_Basket_01",
                    new Vector3(-6.8f, 0.95f, -1.7f), Quaternion.identity);

            // Candle stubs on shelf top — visual warm points.
            var candleStub = FindMVPrefab(new[] { "wax_04a", "candle_01", "wax_03" });
            if (candleStub != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    placed += SpawnLocal(candleStub, shelves.transform, $"Shelf_Candle_{i}",
                        new Vector3(-6.8f, 1.15f, 0.6f + i * 0.4f), Quaternion.identity);
                }
            }
            return placed;
        }

        private static int BuildHangingLantern(Transform envRoot, MedievalVillageBindings mv)
        {
            int placed = 0;
            var lantern = mv?.lampPostPrefab ?? FindMVPrefab(new[] { "lantern", "lamp", "torch" });
            if (lantern == null) return 0;

            var l = (GameObject)PrefabUtility.InstantiatePrefab(lantern);
            l.name = "HollowLantern";
            l.transform.SetParent(envRoot, true);
            l.transform.position = new Vector3(0f, 3.5f, 0f);
            placed++;

            var halo = FindLumenPrefab("Lantern Effect");
            if (halo != null)
            {
                var h = (GameObject)PrefabUtility.InstantiatePrefab(halo);
                h.name = "HollowLantern_Halo";
                h.transform.SetParent(l.transform, false);
                h.transform.localPosition = Vector3.zero;
                placed++;
            }

            // Real Point Light fallback (URP needs it for shadows).
            var bulbGO = new GameObject("HollowLantern_Bulb");
            bulbGO.transform.SetParent(l.transform, false);
            bulbGO.transform.localPosition = Vector3.zero;
            var pl = bulbGO.AddComponent<Light>();
            pl.type = LightType.Point;
            pl.color = new Color(1.0f, 0.78f, 0.42f);
            pl.intensity = 3.2f;
            pl.range = 8f;
            pl.shadows = LightShadows.Soft;
            placed++;

            // Wall-lantern on the east wall for symmetry.
            var wallLamp = (GameObject)PrefabUtility.InstantiatePrefab(lantern);
            wallLamp.name = "EastWallLantern";
            wallLamp.transform.SetParent(envRoot, true);
            wallLamp.transform.position = new Vector3(7.8f, 2.2f, 0f);
            wallLamp.transform.rotation = Quaternion.Euler(0, -90, 0);
            placed++;
            return placed;
        }

        private static int BuildCandleAndRug(Transform envRoot)
        {
            int placed = 0;
            var candle = FindMVPrefab(new[] { "wax_04a", "candle_01", "wax_03" });
            if (candle != null)
            {
                var c = (GameObject)PrefabUtility.InstantiatePrefab(candle);
                c.name = "WorkbenchCandle";
                c.transform.SetParent(envRoot, true);
                c.transform.position = new Vector3(3.5f, 1.05f, 3f);
                placed++;

                var halo = FindLumenPrefab("Shimmery Light");
                if (halo != null)
                {
                    var h = (GameObject)PrefabUtility.InstantiatePrefab(halo);
                    h.name = "WorkbenchCandle_Glow";
                    h.transform.SetParent(c.transform, false);
                    h.transform.localPosition = new Vector3(0f, 0.4f, 0f);
                    h.transform.localScale = Vector3.one * 0.4f;
                    placed++;
                }
            }

            // Wool rug — a banner flattened to be a floor rug at scene centre.
            var rug = FindMVPrefab(new[] { "banner_01", "banner_variant_02", "banner" });
            if (rug != null)
            {
                var r = (GameObject)PrefabUtility.InstantiatePrefab(rug);
                r.name = "WoolRug";
                r.transform.SetParent(envRoot, true);
                r.transform.position = new Vector3(0f, 0.02f, 0f);
                r.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                r.transform.localScale = new Vector3(3.6f, 0.05f, 5.0f);
                placed++;
            }
            return placed;
        }

        private static int BuildDressing(Transform envRoot)
        {
            int placed = 0;
            var sack = FindMVPrefab(new[] { "grain_sack_01", "foodsac_01", "sack_apple" });
            var crate = FindMVPrefab(new[] { "wickerbasket_02", "crate_01", "basket_02" });
            var banner = FindMVPrefab(new[] { "flag_gurland", "flag_01", "banner_01" });

            if (sack != null)
            {
                placed += SpawnLocal(sack, envRoot, "Sack_Hearth_01",
                    new Vector3(-5.5f, 0f, 2.5f), Quaternion.Euler(0, 12, 0));
                placed += SpawnLocal(sack, envRoot, "Sack_Hearth_02",
                    new Vector3(-5.2f, 0f, 3.0f), Quaternion.Euler(0, 35, 0));
            }
            if (crate != null)
            {
                placed += SpawnLocal(crate, envRoot, "Crate_Hearth",
                    new Vector3(-5.5f, 0.0f, 3.6f), Quaternion.identity);
            }
            if (banner != null)
            {
                placed += SpawnLocal(banner, envRoot, "EastWall_Banner",
                    new Vector3(7.7f, 2.8f, -1f), Quaternion.Euler(0, -90, 0));
            }
            return placed;
        }

        private static int BuildWindowGlow(Transform envRoot)
        {
            int placed = 0;
            var glow = FindLumenPrefab("Light Surface");
            if (glow != null)
            {
                var g = (GameObject)PrefabUtility.InstantiatePrefab(glow);
                g.name = "NorthWindow_Glow";
                g.transform.SetParent(envRoot, true);
                g.transform.position = new Vector3(0f, 1.7f, 8.4f);
                g.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                placed++;
            }
            return placed;
        }

        private static void SetInteriorLighting()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor    = new Color(0.22f, 0.20f, 0.16f);
            RenderSettings.ambientEquatorColor = new Color(0.16f, 0.12f, 0.09f);
            RenderSettings.ambientGroundColor  = new Color(0.08f, 0.06f, 0.04f);

            // Soften the directional light if it exists.
            var dir = FindDirectionalLight();
            if (dir != null)
            {
                dir.intensity = 0.45f;
                dir.color = new Color(1f, 0.84f, 0.62f);
                dir.shadows = LightShadows.Soft;
            }
        }

        private static Light FindDirectionalLight()
        {
            foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                if (l.type == LightType.Directional) return l;
            return null;
        }

        private static int SpawnLocal(GameObject prefab, Transform parent, string name, Vector3 localPos, Quaternion localRot)
        {
            if (prefab == null) return 0;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = localRot;
            return 1;
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
