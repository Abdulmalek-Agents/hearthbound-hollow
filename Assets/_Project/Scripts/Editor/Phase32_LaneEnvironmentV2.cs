// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase32_LaneEnvironmentV2
//
// Phase 32.2 — Lane environment polish v2.
//
// Builds on top of Phase 27.2 (which used SM_ShopStand_01a as the cottage
// fallback). This pass:
//   1. Uses the 4 cottage prefabs that Phase 32.1 authored to populate the
//      lane with 8 properly-assembled residential houses.
//   2. Wraps the Hollow door with a 4×3 m facade so the shop reads as a real
//      building (not a floating door frame).
//   3. Adds 3 extra lantern posts along the cobble path for evening warmth.
//   4. Drops Doris's beehive prop (a stacked TerraPot proxy) by Cottage_A_Bakery.
//   5. Adds a hay bale + apple basket for cozy bakery dressing.
//   6. Smoking chimneys on every cottage via Stylized Weather Dust prefab
//      as a quick wisp stand-in (until VoluSmokeFX is imported).
//   7. Improves the cobble path with rocky borders and wider coverage.
//   8. Adds a `_Phase32Env_Lane` parent so re-runs are idempotent.
//
// USE: Menu → Hearthbound → 🏘️ Phase 32.2 — Polish Lane Environment V2
//
// IMPORTANT: This builder ADDS to Phase 27.2's output (doesn't delete it).
// Phase 27.2 builds the bare lane skeleton; Phase 32.2 layers the v2 props
// on top. Re-running Phase 27.2 after Phase 32.2 will leave the v2 layer
// intact (different parent GameObject).
//
// Architecture notes:
//   - Per D-007: scene file is edited in-place + saved.
//   - Per D-027: TryGet* lookups via Phase 32.1's bindings.
//   - Per D-035: Editor-only.

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HearthboundHollow.EditorTools
{
    public static class Phase32_LaneEnvironmentV2
    {
        private const string SceneLane       = "Assets/_Project/Scenes/02_Mission01_Lane.unity";
        private const string MVRoot          = "Assets/MeshingunStudio";
        private const string StylizedWeather = "Assets/Unluck Software/Stylized Weather/Prefabs";
        private const string LumenPrefabs    = "Packages/com.distantlands.lumen/Content/Prefabs";

        private const string EnvParentName   = "_Phase32Env_Lane";

        [MenuItem("Hearthbound/🏘️ Phase 32.2 — Polish Lane Environment V2", priority = 32)]
        public static void Build()
        {
            if (!System.IO.File.Exists(SceneLane))
            {
                EditorUtility.DisplayDialog("Phase 32.2",
                    $"Scene not found: {SceneLane}\n\n" +
                    "Run Phase 23 (capstone) first so the scene exists.", "OK");
                return;
            }

            // Ensure cottage prefabs exist; build them on demand if not.
            var cottages = Phase32_MedievalCottageBuilder.TryGetAllCottagePrefabs();
            if (cottages.Length < 2)
            {
                Debug.Log("[Hearthbound/Phase 32.2] Cottage prefabs missing — running Phase 32.1 …");
                Phase32_MedievalCottageBuilder.Build();
                cottages = Phase32_MedievalCottageBuilder.TryGetAllCottagePrefabs();
            }
            if (cottages.Length == 0)
            {
                EditorUtility.DisplayDialog("Phase 32.2 — No cottage prefabs",
                    "Could not author cottage prefabs (Medieval Village pieces missing).\n\n" +
                    "Phase 32.2 will skip the cottage layout but still place atmosphere.",
                    "OK");
            }

            // Ensure extended bindings exist.
            var binds = Phase32_VillageBindingsExtension.TryGetBindings();
            if (binds == null)
            {
                Phase32_VillageBindingsExtension.Build();
                binds = Phase32_VillageBindingsExtension.TryGetBindings();
            }

            var scene = EditorSceneManager.OpenScene(SceneLane, OpenSceneMode.Single);

            // Wipe previous Phase 32.2 pass — idempotent.
            var existing = GameObject.Find(EnvParentName);
            if (existing != null) Object.DestroyImmediate(existing);

            var envRoot = new GameObject(EnvParentName);

            int placed = 0;

            placed += PlaceCottagesV2(envRoot.transform, cottages);
            placed += PlaceHollowFacade(envRoot.transform, binds);
            placed += PlaceExtraLanterns(envRoot.transform, binds);
            placed += PlaceBeehiveAndBakeryDressing(envRoot.transform, binds);
            placed += PlaceChimneySmoke(envRoot.transform);
            placed += PlaceExtraCobble(envRoot.transform, binds);
            placed += PlaceExtraAtmosphere(envRoot.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog("Phase 32.2 — Lane V2 polished",
                $"🏘️ Lane v2 environment built. {placed} additions placed under " +
                $"'_Phase32Env_Lane'.\n\n" +
                "Re-run any time. Phase 27.2's '_Phase27Env_Lane' is preserved.\n\n" +
                "Next: Hearthbound → 🏠 Phase 32.3 — Polish Hollow Interior V2",
                "OK");
        }

        // ───────────────────────────────────────────────────────────────
        // SECTION 1 — 8 cottages along the lane (replaces 27.2's 3 shopstands)
        // ───────────────────────────────────────────────────────────────

        private static int PlaceCottagesV2(Transform envRoot, GameObject[] cottages)
        {
            int placed = 0;
            if (cottages == null || cottages.Length == 0) return 0;

            // Pick rotation around the cottage's facade so the front (south wall
            // with windows / sign) faces toward the path centre (x = 0).
            // Cottages spawned at -X face east (yaw +90); at +X face west (yaw -90).

            var layout = new (string label, int variantIndex, Vector3 pos, float yaw)[]
            {
                // Back row (closest to the Hollow door — z = +4 to +7)
                ("Cottage_NorthWest", 1, new Vector3(-10f, 0f,  4.0f),  90f), // Plain
                ("Cottage_NorthEast", 2, new Vector3( 10f, 0f,  6.0f), -90f), // Gabled

                // Middle row — Doris's bakery is the hero on the player's right
                ("Cottage_Bakery_Doris",  0, new Vector3(  9f, 0f, -1.0f), -90f), // Bakery
                ("Cottage_MiddleWest",    3, new Vector3(-10f, 0f, -1.0f),  90f), // Corner

                // Forward row (closer to player spawn — z = -5 to -3)
                ("Cottage_ForwardWest", 1, new Vector3(-11f, 0f, -5.0f),  90f), // Plain
                ("Cottage_ForwardEast", 2, new Vector3( 11f, 0f, -4.5f), -90f), // Gabled

                // Distant row (background depth — z = +10 to +12)
                ("Cottage_DistantWest", 3, new Vector3(-13f, 0f, 10.0f),  60f), // Corner
                ("Cottage_DistantEast", 0, new Vector3( 13f, 0f, 11.0f), -60f), // Bakery
            };

            var villageRoot = new GameObject("Cottages");
            villageRoot.transform.SetParent(envRoot, false);

            foreach (var (label, vi, pos, yaw) in layout)
            {
                int safeIdx = Mathf.Clamp(vi, 0, cottages.Length - 1);
                var prefab = cottages[safeIdx];
                if (prefab == null) continue;

                var c = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                c.name = label;
                c.transform.SetParent(villageRoot.transform, true);
                c.transform.position = pos;
                c.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
                placed++;

                // For Doris's bakery, add a Lumen window-glow so it reads warm-at-dusk.
                if (label == "Cottage_Bakery_Doris")
                {
                    var glow = FindLumenPrefab("Light Surface");
                    if (glow != null)
                    {
                        var g = (GameObject)PrefabUtility.InstantiatePrefab(glow);
                        g.name = "Bakery_WindowGlow";
                        g.transform.SetParent(c.transform, false);
                        g.transform.localPosition = new Vector3(0f, 1.6f, -1.9f);
                        placed++;
                    }
                }

                // For the cottage in front of the player on the west — also light a window.
                if (label == "Cottage_ForwardWest" || label == "Cottage_NorthWest")
                {
                    var glow = FindLumenPrefab("Light Surface");
                    if (glow != null)
                    {
                        var g = (GameObject)PrefabUtility.InstantiatePrefab(glow);
                        g.name = $"{label}_WindowGlow";
                        g.transform.SetParent(c.transform, false);
                        g.transform.localPosition = new Vector3(0f, 1.6f, -1.9f);
                        placed++;
                    }
                }
            }
            return placed;
        }

        // ───────────────────────────────────────────────────────────────
        // SECTION 2 — Hollow facade (wraps the door so it reads as a building)
        // ───────────────────────────────────────────────────────────────

        private static int PlaceHollowFacade(Transform envRoot, MedievalVillageBindingsV2 binds)
        {
            int placed = 0;
            if (binds == null || binds.solidWallPrefab == null || binds.windowWallPrefab == null)
                return 0;

            var facade = new GameObject("HollowFacade");
            facade.transform.SetParent(envRoot, false);
            facade.transform.position = new Vector3(0f, 0f, 9.5f); // Behind the existing door at z=8
            facade.transform.rotation = Quaternion.identity;

            // 4×3 m facade — front faces the player (south, -Z).
            // Wall_S_Left | Wall_S_Door (gap for the existing door) | Wall_S_Right
            // Side walls extend back into the village.
            SpawnLocal(binds.solidWallPrefab,  facade.transform, "Facade_S_Left",
                new Vector3(-2.5f, 0f, -1.5f), Quaternion.Euler(0, 0, 0));
            SpawnLocal(binds.windowWallPrefab, facade.transform, "Facade_S_Right_Win",
                new Vector3( 2.5f, 0f, -1.5f), Quaternion.Euler(0, 0, 0));
            placed += 2;

            // Side walls (east + west)
            SpawnLocal(binds.solidWallPrefab,  facade.transform, "Facade_E",
                new Vector3( 4.0f, 0f, 0.5f), Quaternion.Euler(0, -90, 0));
            SpawnLocal(binds.solidWallPrefab,  facade.transform, "Facade_W",
                new Vector3(-4.0f, 0f, 0.5f), Quaternion.Euler(0, 90, 0));
            placed += 2;

            // Back wall (with a small window so we can hint at the Hollow interior)
            SpawnLocal(binds.windowWallPrefab, facade.transform, "Facade_N_Win",
                new Vector3(0f, 0f, 2.5f), Quaternion.Euler(0, 180, 0));
            placed += 1;

            // Roof — two pitched tiles
            if (binds.roofTilePrefab != null)
            {
                SpawnLocal(binds.roofTilePrefab, facade.transform, "Facade_Roof_W",
                    new Vector3(-1f, 3.0f, 0.5f), Quaternion.Euler(0f, 90f, 25f));
                SpawnLocal(binds.roofTilePrefab, facade.transform, "Facade_Roof_E",
                    new Vector3( 1f, 3.0f, 0.5f), Quaternion.Euler(0f, -90f, 25f));
                placed += 2;
            }

            // Chimney
            if (binds.chimneyPrefab != null)
            {
                SpawnLocal(binds.chimneyPrefab, facade.transform, "Facade_Chimney",
                    new Vector3(1.4f, 4.4f, 1.0f), Quaternion.identity);
                placed += 1;
            }

            // "The Hollow" 3D text above the doorway (Z=-1.6 so it's just in front of facade wall)
            var signGO = new GameObject("HollowSign");
            signGO.transform.SetParent(facade.transform, false);
            signGO.transform.localPosition = new Vector3(0f, 2.4f, -1.55f);
            signGO.transform.localRotation = Quaternion.Euler(0, 0, 0);
            var tmp = signGO.AddComponent<TMPro.TextMeshPro>();
            tmp.text = "The Hollow";
            tmp.fontSize = 1.7f;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.color = new Color(0.96f, 0.86f, 0.58f);
            tmp.fontStyle = TMPro.FontStyles.Italic;
            placed += 1;

            // Warm window-glow visible from outside (Lumen)
            var winGlow = FindLumenPrefab("Light Surface");
            if (winGlow != null)
            {
                var g = (GameObject)PrefabUtility.InstantiatePrefab(winGlow);
                g.name = "Facade_S_Right_Glow";
                g.transform.SetParent(facade.transform, false);
                g.transform.localPosition = new Vector3(2.5f, 1.6f, -1.4f);
                placed += 1;
            }

            return placed;
        }

        // ───────────────────────────────────────────────────────────────
        // SECTION 3 — Extra lantern posts along the cobble path
        // ───────────────────────────────────────────────────────────────

        private static int PlaceExtraLanterns(Transform envRoot, MedievalVillageBindingsV2 binds)
        {
            int placed = 0;
            var post = binds?.lanternPostPrefab ?? FindMVPrefab(new[] { "streetlantern_01a", "streetlight_01a", "lantern" });
            if (post == null) return 0;

            var group = new GameObject("ExtraLanterns");
            group.transform.SetParent(envRoot, false);

            var positions = new[]
            {
                ("Lantern_LaneS",   new Vector3(-3.5f, 0f, -5.5f)),
                ("Lantern_LaneMid", new Vector3( 3.5f, 0f, -1.0f)),
                ("Lantern_LaneN",   new Vector3(-3.5f, 0f,  4.5f)),
            };

            foreach (var (name, pos) in positions)
            {
                var p = (GameObject)PrefabUtility.InstantiatePrefab(post);
                p.name = name;
                p.transform.SetParent(group.transform, true);
                p.transform.position = pos;
                placed++;

                // Add a real Light component for shadow-quality (URP needs a real Light).
                var bulbGO = new GameObject($"{name}_Bulb");
                bulbGO.transform.SetParent(p.transform, false);
                bulbGO.transform.localPosition = new Vector3(0f, 2.4f, 0f); // approximate lantern head height
                var pl = bulbGO.AddComponent<Light>();
                pl.type = LightType.Point;
                pl.color = new Color(1.0f, 0.78f, 0.42f);
                pl.intensity = 2.4f;
                pl.range = 5.5f;
                pl.shadows = LightShadows.None; // mobile-safe — 4+ lanterns can't all shadow
                placed++;

                // Lumen halo child for the cozy bloom-friendly glow.
                var halo = FindLumenPrefab("Lantern Effect");
                if (halo != null)
                {
                    var h = (GameObject)PrefabUtility.InstantiatePrefab(halo);
                    h.name = $"{name}_Halo";
                    h.transform.SetParent(p.transform, false);
                    h.transform.localPosition = new Vector3(0f, 2.4f, 0f);
                    placed++;
                }
            }
            return placed;
        }

        // ───────────────────────────────────────────────────────────────
        // SECTION 4 — Doris's beehive + bakery dressing
        // ───────────────────────────────────────────────────────────────

        private static int PlaceBeehiveAndBakeryDressing(Transform envRoot, MedievalVillageBindingsV2 binds)
        {
            int placed = 0;

            // Find Doris's bakery cottage in the village layout (was named
            // "Cottage_Bakery_Doris" above) — anchor props relative to it.
            // Defensive: if it doesn't exist (e.g. cottages couldn't build),
            // just spawn the props at the documented world coordinates.
            Vector3 anchor = new Vector3(9f, 0f, -1.0f);
            var bakery = GameObject.Find("Cottage_Bakery_Doris");
            if (bakery != null) anchor = bakery.transform.position;

            var dressing = new GameObject("BakeryDressing");
            dressing.transform.SetParent(envRoot, false);

            // Beehive proxy — stacked TerraPots (proxy until a proper beehive is imported).
            if (binds?.hangingPotPrefab != null)
            {
                var bee1 = (GameObject)PrefabUtility.InstantiatePrefab(binds.hangingPotPrefab);
                bee1.name = "BeehiveBase";
                bee1.transform.SetParent(dressing.transform, true);
                bee1.transform.position = anchor + new Vector3(-2.5f, 0f, -1.5f);
                placed++;

                var bee2 = (GameObject)PrefabUtility.InstantiatePrefab(binds.hangingPotPrefab);
                bee2.name = "BeehiveTop";
                bee2.transform.SetParent(dressing.transform, true);
                bee2.transform.position = anchor + new Vector3(-2.5f, 0.7f, -1.5f);
                bee2.transform.localScale = Vector3.one * 0.85f;
                placed++;
            }

            // Hay bale (proxy: sack)
            if (binds?.hayBalePrefab != null)
            {
                var hay = (GameObject)PrefabUtility.InstantiatePrefab(binds.hayBalePrefab);
                hay.name = "HayBale";
                hay.transform.SetParent(dressing.transform, true);
                hay.transform.position = anchor + new Vector3(-1.8f, 0f, -2.5f);
                hay.transform.rotation = Quaternion.Euler(0f, 23f, 0f);
                placed++;
            }

            // Apple basket
            var basket = FindMVPrefab(new[] { "wickerbasket_01d", "wickerbasket_01", "basket" });
            if (basket != null)
            {
                var b = (GameObject)PrefabUtility.InstantiatePrefab(basket);
                b.name = "AppleBasket";
                b.transform.SetParent(dressing.transform, true);
                b.transform.position = anchor + new Vector3(-1.2f, 0f, -2.8f);
                placed++;
            }

            // 2-3 apples spilling out of basket — atmosphere
            for (int i = 0; i < 3; i++)
            {
                var apple = FindMVPrefab(new[] { "apple_01a", "apple_01b", "apple" });
                if (apple == null) break;
                var a = (GameObject)PrefabUtility.InstantiatePrefab(apple);
                a.name = $"BakeryApple_{i}";
                a.transform.SetParent(dressing.transform, true);
                a.transform.position = anchor + new Vector3(-0.9f + i * 0.15f, 0.05f, -3.0f - i * 0.08f);
                a.transform.localRotation = Quaternion.Euler(0, i * 89f, 0);
                placed++;
            }

            // Wood log pile beside the bakery (firewood for the oven)
            var logPile = FindMVPrefab(new[] { "pile_woodlog_01a", "pile_woodlog", "woodlog_01a" });
            if (logPile != null)
            {
                var l = (GameObject)PrefabUtility.InstantiatePrefab(logPile);
                l.name = "BakeryFirewood";
                l.transform.SetParent(dressing.transform, true);
                l.transform.position = anchor + new Vector3(1.5f, 0f, -2.5f);
                l.transform.rotation = Quaternion.Euler(0f, -45f, 0f);
                placed++;
            }

            return placed;
        }

        // ───────────────────────────────────────────────────────────────
        // SECTION 5 — Smoking chimneys (Stylized Weather Dust as wisp proxy)
        // ───────────────────────────────────────────────────────────────

        private static int PlaceChimneySmoke(Transform envRoot)
        {
            int placed = 0;
            var smoke = FindWeatherPrefab("Dust") ?? FindWeatherPrefab("Dust Devil -C");
            if (smoke == null) return 0;

            var group = new GameObject("ChimneySmoke");
            group.transform.SetParent(envRoot, false);

            // Find every cottage placed by Phase 32.2's cottage layout — light up smoke on each chimney.
            var cottageNames = new[]
            {
                "Cottage_NorthWest", "Cottage_NorthEast",
                "Cottage_Bakery_Doris", "Cottage_MiddleWest",
                "Cottage_ForwardWest", "Cottage_ForwardEast",
                "Cottage_DistantWest", "Cottage_DistantEast",
            };

            foreach (var name in cottageNames)
            {
                var cottage = GameObject.Find(name);
                if (cottage == null) continue;

                // Find a "Chimney" child inside (cottage prefabs all have one).
                var chimneyT = FindChildByName(cottage.transform, "Chimney");
                if (chimneyT == null) continue;

                var wisp = (GameObject)PrefabUtility.InstantiatePrefab(smoke);
                wisp.name = $"{name}_Smoke";
                wisp.transform.SetParent(chimneyT, false);
                wisp.transform.localPosition = new Vector3(0f, 1.0f, 0f);
                wisp.transform.localScale = Vector3.one * 0.35f;
                placed++;
            }

            return placed;
        }

        private static Transform FindChildByName(Transform parent, string nameContains)
        {
            foreach (var t in parent.GetComponentsInChildren<Transform>(true))
                if (t.name.IndexOf(nameContains, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return t;
            return null;
        }

        // ───────────────────────────────────────────────────────────────
        // SECTION 6 — Extra cobble + path borders (extends 27.2's path)
        // ───────────────────────────────────────────────────────────────

        private static int PlaceExtraCobble(Transform envRoot, MedievalVillageBindingsV2 binds)
        {
            int placed = 0;
            var cobble = binds?.cobbleTilePrefab ?? FindMVPrefab(new[] { "cornercobble", "cobble" });

            // Extend the cobble corridor further south (-12) and north (+10) of 27.2's range.
            if (cobble != null)
            {
                var pathGroup = new GameObject("ExtraCobble");
                pathGroup.transform.SetParent(envRoot, false);

                // Far south extension
                for (int i = 0; i < 2; i++)
                {
                    var c = (GameObject)PrefabUtility.InstantiatePrefab(cobble);
                    c.name = $"CobbleS_{i:00}";
                    c.transform.SetParent(pathGroup.transform, true);
                    c.transform.position = new Vector3(0f, 0.02f, -12f + i * 2.0f);
                    placed++;
                }
                // Far north extension
                for (int i = 0; i < 2; i++)
                {
                    var c = (GameObject)PrefabUtility.InstantiatePrefab(cobble);
                    c.name = $"CobbleN_{i:00}";
                    c.transform.SetParent(pathGroup.transform, true);
                    c.transform.position = new Vector3(0f, 0.02f, 8f + i * 2.0f);
                    placed++;
                }
            }

            // Stone-brick border on the path edge — every 2 m
            var brick = binds?.stoneBrickPrefab ?? FindMVPrefab(new[] { "stonebrick_01", "stone_01" });
            if (brick != null)
            {
                for (int i = 0; i < 10; i++)
                {
                    var br = (GameObject)PrefabUtility.InstantiatePrefab(brick);
                    br.name = $"PathBrick_E_{i:00}";
                    br.transform.SetParent(envRoot, true);
                    br.transform.position = new Vector3(1.6f, 0f, -10f + i * 2.0f);
                    br.transform.rotation = Quaternion.Euler(0, i * 23f, 0);
                    placed++;
                }
                for (int i = 0; i < 10; i++)
                {
                    var br = (GameObject)PrefabUtility.InstantiatePrefab(brick);
                    br.name = $"PathBrick_W_{i:00}";
                    br.transform.SetParent(envRoot, true);
                    br.transform.position = new Vector3(-1.6f, 0f, -10f + i * 2.0f);
                    br.transform.rotation = Quaternion.Euler(0, 180f + i * 31f, 0);
                    placed++;
                }
            }
            return placed;
        }

        // ───────────────────────────────────────────────────────────────
        // SECTION 7 — Extra atmosphere (autumn alders, more grass, pebbles)
        // ───────────────────────────────────────────────────────────────

        private static int PlaceExtraAtmosphere(Transform envRoot)
        {
            int placed = 0;
            var group = new GameObject("ExtraAtmosphere");
            group.transform.SetParent(envRoot, false);

            // 3 autumn alders in the background — frames the camera composition
            var alder = FindMVPrefab(new[] { "alder_fall_01a", "alder_fall_01b", "alder_fall" });
            if (alder != null)
            {
                placed += SpawnAt(alder, group.transform, "Alder_BG_W", new Vector3(-13f, 0f,  8.0f));
                placed += SpawnAt(alder, group.transform, "Alder_BG_E", new Vector3( 14f, 0f,  9.0f), Quaternion.Euler(0, 60, 0));
                placed += SpawnAt(alder, group.transform, "Alder_BG_FarN", new Vector3(  2f, 0f, 14.0f), Quaternion.Euler(0, 130, 0));
            }

            // More pebbles for ground texture variety
            for (int i = 0; i < 6; i++)
            {
                var pebble = FindMVPrefab(new[] { "pebble_01a", "pebble_02a", "pebble_03a", "pebble_04a", "pebble" });
                if (pebble == null) break;
                var p = (GameObject)PrefabUtility.InstantiatePrefab(pebble);
                p.name = $"Pebble_{i:00}";
                p.transform.SetParent(group.transform, true);
                p.transform.position = new Vector3((i % 2 == 0 ? -1 : 1) * (2.2f + (i * 0.13f)),
                                                    0f,
                                                    -9f + i * 3.2f);
                p.transform.rotation = Quaternion.Euler(0, i * 41f, 0);
                placed++;
            }

            // 8 more grass tufts spread further from the path
            var grass = FindWeatherPrefab("Grass_Common_Short");
            if (grass != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    var jitter = ((i * 23) % 100) / 100f * 2.4f - 1.2f;
                    var pos = new Vector3((i % 2 == 0 ? 3.0f : -3.0f) + jitter,
                                          0f,
                                          -8f + i * 2.4f);
                    placed += SpawnAt(grass, group.transform, $"GrassExtra_{i:00}", pos);
                }
            }

            // 4 mushrooms (cozy autumn detail)
            var mushroom = FindWeatherPrefab("Mushroom_Laetiporus");
            if (mushroom != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    var pos = new Vector3(-6f + i * 4f, 0f, -7.5f + (i % 2 == 0 ? 0.6f : -0.4f));
                    placed += SpawnAt(mushroom, group.transform, $"Mushroom_{i:00}", pos,
                                       Quaternion.Euler(0, i * 67f, 0));
                }
            }

            // Dead tree in the distance for autumn mood
            var deadTree = FindMVPrefab(new[] { "deadtree_01a", "deadtree" });
            if (deadTree != null)
            {
                placed += SpawnAt(deadTree, group.transform, "DeadTree_Far",
                    new Vector3(15f, 0f, -8f), Quaternion.Euler(0, 200, 0));
            }

            return placed;
        }

        // ───────────────────────────────────────────────────────────────
        // Helpers
        // ───────────────────────────────────────────────────────────────

        private static int SpawnAt(GameObject prefab, Transform parent, string name, Vector3 pos)
            => SpawnAt(prefab, parent, name, pos, Quaternion.identity);

        private static int SpawnAt(GameObject prefab, Transform parent, string name, Vector3 pos, Quaternion rot)
        {
            if (prefab == null) return 0;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.name = name;
            go.transform.SetParent(parent, true);
            go.transform.position = pos;
            go.transform.rotation = rot;
            return 1;
        }

        private static void SpawnLocal(GameObject prefab, Transform parent, string name, Vector3 localPos, Quaternion localRot)
        {
            if (prefab == null) return;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = localRot;
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

        private static GameObject FindWeatherPrefab(string exactName)
        {
            var guids = AssetDatabase.FindAssets($"{exactName} t:Prefab", new[] { StylizedWeather });
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go != null && go.name == exactName) return go;
            }
            return null;
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
