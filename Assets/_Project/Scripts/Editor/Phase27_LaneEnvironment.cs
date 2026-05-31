// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase27_LaneEnvironment
//
// Phase 27.2 — Lane environment polish builder.
//
// Replaces the procedural "flat ground + cube door" baseline in
// 02_Mission01_Lane.unity with a fully-dressed cozy autumn village lane:
//   • cobble path tiles centred on the player walk
//   • 3 cottages framing the camera composition
//   • 2 MV trees (Alder Fall) + 3 Stylized Weather CommonTrees
//   • bushes, rocks, grass tufts
//   • Hollow-door 3-step + sign frame + hanging lantern
//   • fence segments along the south edge
//   • well + bench + crate pile in the village foreground
//   • Banner on a cottage wall + Falling leaves + Soft fog
//   • Lumen Sunshafts aimed at the directional light
//
// All spawns are parented under `_Phase27Env_Lane` so re-running the
// builder cleanly removes the previous pass and rebuilds — idempotent.
//
// USE: Menu → Hearthbound → ⚙️ Advanced → 🌳 Phase 27.2 — Polish Lane Environment
//
// Demoted to ⚙️ Advanced/… in Phase 32 (menu collapse). The user-facing
// entry point is `Hearthbound → 🚀 Build Everything`, which Phase 32
// supersedes via Phase 32.2 (Lane Environment v2) — the v2 builder
// stacks on top of the v1 _Phase27Env_Lane parent (idempotent siblings).
//
// Architecture notes:
//   - Per D-007 we never check the .unity file into git. The builder
//     edits the scene in-place and saves; the user pulls the script and
//     re-runs the menu.
//   - Per D-035 (asmdef-locality) this script lives in the Editor asmdef
//     which references everything — no compile-time concerns.
//   - Per D-027 each Phase exposes TryGet*() lookups; this builder uses
//     Phase 15's MedievalVillageBindings.asset as the primary source of
//     truth for cottage/well/fence/lamp/tree prefabs, falling back to
//     fresh keyword searches when bindings are missing.

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HearthboundHollow.EditorTools
{
    public static class Phase27_LaneEnvironment
    {
        private const string SceneLane = "Assets/_Project/Scenes/02_Mission01_Lane.unity";

        // Asset-source roots — used for fuzzy prefab lookups when the
        // Phase 15 bindings asset is missing or has gaps.
        private const string MVRoot           = "Assets/MeshingunStudio";
        private const string StylizedWeather  = "Assets/Unluck Software/Stylized Weather/Prefabs";
        private const string LumenPrefabs     = "Packages/com.distantlands.lumen/Content/Prefabs";

        // Single parent GameObject so re-running cleans up its own work.
        private const string EnvParentName = "_Phase27Env_Lane";

        [MenuItem("Hearthbound/⚙️ Advanced/🌳 Phase 27.2 — Polish Lane Environment", priority = 6)]
        public static void Build()
        {
            if (!System.IO.File.Exists(SceneLane))
            {
                EditorUtility.DisplayDialog("Phase 27.2",
                    "02_Mission01_Lane.unity is missing. Run Phase 23 (capstone) first.", "OK");
                return;
            }

            var scene = EditorSceneManager.OpenScene(SceneLane, OpenSceneMode.Single);

            // Wipe any previous Phase 27 pass so this is idempotent.
            var existing = GameObject.Find(EnvParentName);
            if (existing != null) Object.DestroyImmediate(existing);

            var envRoot = new GameObject(EnvParentName);

            // ─── Lookup the Phase 15 prefab bindings (best-effort) ────
            var mv = Phase15_MedievalVillageBuilder.TryGetBindings();
            if (mv == null)
            {
                Debug.LogWarning("[Hearthbound/Phase 27.2] Phase 15 bindings missing. " +
                                 "Running Phase 15 first to populate them …");
                Phase15_MedievalVillageBuilder.Build();
                mv = Phase15_MedievalVillageBuilder.TryGetBindings();
            }

            int placed = 0;

            // ─── Replace the placeholder HollowDoor cube ───────────
            placed += UpgradeHollowDoor(envRoot.transform, mv);

            // ─── Cobble path corridor ──────────────────────────
            placed += LayCobblePath(envRoot.transform);

            // ─── Three cottages framing the lane ─────────────────
            placed += PlaceCottages(envRoot.transform, mv);

            // ─── Trees, bushes, rocks, grass ────────────────────
            placed += PlaceFoliage(envRoot.transform);

            // ─── Fence south + well + bench + crate pile + sign ────────
            placed += PlaceVillageDressing(envRoot.transform, mv);

            // ─── Hanging lantern at the door + Lumen halo ──────────
            placed += PlaceHangingLantern(envRoot.transform, mv);

            // ─── Atmosphere — leaves, fog, sunshafts ───────────────
            placed += PlaceAtmosphere(envRoot.transform);

            // Optional ambient extras — silent villager standing by the fence
            placed += PlaceSilentVillagerExtra(envRoot.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog("Phase 27.2 — Lane polished",
                $"✨ Lane environment built. {placed} prefab instances placed under " +
                $"'_Phase27Env_Lane'.\n\n" +
                "Re-run this menu any time to refresh. The capstone (Phase 27.4) " +
                "calls this builder + the Hollow builder + atmosphere setup.",
                "OK");
        }

        // ───────────────────────────────────────────────────────────────
        // SECTION 1 — door upgrade
        // ───────────────────────────────────────────────────────────────

        private static int UpgradeHollowDoor(Transform envRoot, MedievalVillageBindings mv)
        {
            int placed = 0;
            // Find existing HollowDoor placeholder (cube) and read its position.
            var oldDoor = GameObject.Find("HollowDoor");
            Vector3 doorPos = new Vector3(0f, 1.5f, 8f);
            string targetScene = "03_Mission01_Hollow";
            if (oldDoor != null)
            {
                doorPos = oldDoor.transform.position;
                var existingDoor = oldDoor.GetComponent<HearthboundHollow.Player.HollowDoorInteractable>();
                if (existingDoor != null && !string.IsNullOrEmpty(existingDoor.targetSceneName))
                    targetScene = existingDoor.targetSceneName;
            }

            // Find a real door prefab.
            var doorPrefab = mv?.doorPrefab ?? FindMVPrefab(new[] { "wooddoor", "door_01", "door_02" });
            if (doorPrefab != null)
            {
                if (oldDoor != null) Object.DestroyImmediate(oldDoor);
                var newDoor = (GameObject)PrefabUtility.InstantiatePrefab(doorPrefab);
                newDoor.name = "HollowDoor";
                newDoor.transform.SetParent(envRoot, true);
                newDoor.transform.position = new Vector3(doorPos.x, 0f, doorPos.z);
                newDoor.transform.rotation = Quaternion.Euler(0f, 180f, 0f); // face the player

                // Wire HollowDoorInteractable so E still loads the Hollow scene.
                if (newDoor.GetComponent<HearthboundHollow.Player.HollowDoorInteractable>() == null)
                {
                    var hi = newDoor.AddComponent<HearthboundHollow.Player.HollowDoorInteractable>();
                    hi.targetSceneName = targetScene;
                }
                EnsureInteractCollider(newDoor, new Vector3(2.4f, 3.2f, 0.6f));
                placed++;

                // 3-tier step in front of the door so the player feels the threshold.
                var step = FindMVPrefab(new[] { "stepstair", "step_stair", "stairs_01" });
                if (step != null)
                {
                    var s = (GameObject)PrefabUtility.InstantiatePrefab(step);
                    s.name = "DoorStep";
                    s.transform.SetParent(envRoot, true);
                    s.transform.position = new Vector3(doorPos.x, 0f, doorPos.z - 1.0f);
                    placed++;
                }

                // Door sign — TextMeshPro 3D framed against the wall.
                var signTag = new GameObject("DoorSign");
                signTag.transform.SetParent(envRoot, false);
                signTag.transform.position = new Vector3(doorPos.x + 0.9f, 1.85f, doorPos.z - 0.05f);
                signTag.transform.rotation = Quaternion.Euler(0, 180, 0);
                var tmp = signTag.AddComponent<TMPro.TextMeshPro>();
                tmp.text = "The Hollow";
                tmp.fontSize = 1.6f;
                tmp.alignment = TMPro.TextAlignmentOptions.Center;
                tmp.color = new Color(0.96f, 0.86f, 0.58f);
                placed++;
            }
            return placed;
        }

        // ───────────────────────────────────────────────────────────────
        // SECTION 2 — cobble path
        // ───────────────────────────────────────────────────────────────

        private static int LayCobblePath(Transform envRoot)
        {
            int placed = 0;
            // Central cobble corridor along Z from -8 to +6, X = 0.
            var cobble = FindMVPrefab(new[] { "cobble", "cornercobble", "stonepath" });
            if (cobble != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    var c = (GameObject)PrefabUtility.InstantiatePrefab(cobble);
                    c.name = $"Cobble_{i:00}";
                    c.transform.SetParent(envRoot, false);
                    c.transform.position = new Vector3(0f, 0.02f, -8f + i * 2.0f);
                    placed++;
                }
            }

            // Soft-edge stones from Stylized Weather (RockPath_Round_*) to break the line.
            for (int i = 0; i < 6; i++)
            {
                var ix = i % 3 == 0 ? "RockPath_Round_Small_1"
                       : i % 3 == 1 ? "RockPath_Round_Small_2"
                       :              "RockPath_Round_Small_3";
                var sp = FindWeatherPrefab(ix);
                if (sp == null) continue;
                var s = (GameObject)PrefabUtility.InstantiatePrefab(sp);
                s.name = $"PathStone_{i:00}";
                s.transform.SetParent(envRoot, false);
                float zSpread = -7f + i * 2.5f;
                float xJitter = (i % 2 == 0 ? 1.4f : -1.4f);
                s.transform.position = new Vector3(xJitter, 0.0f, zSpread);
                s.transform.localRotation = Quaternion.Euler(0, i * 47f, 0);
                placed++;
            }
            return placed;
        }

        // ───────────────────────────────────────────────────────────────
        // SECTION 3 — cottages
        // ───────────────────────────────────────────────────────────────

        private static int PlaceCottages(Transform envRoot, MedievalVillageBindings mv)
        {
            int placed = 0;
            var cottage = mv?.cottagePrefab ?? FindMVPrefab(new[] { "house", "cottage", "hut", "building" });
            if (cottage == null) return 0;

            var layout = new (string name, Vector3 pos, Quaternion rot)[]
            {
                ("Cottage_01_Left",  new Vector3(-9f, 0f, -2f), Quaternion.Euler(0f, 20f, 0f)),
                ("Cottage_02_Back",  new Vector3(-9f, 0f,  4f), Quaternion.Euler(0f, 180f + 20f, 0f)),
                ("Cottage_03_Right", new Vector3( 9f, 0f,  4f), Quaternion.Euler(0f, -20f, 0f)),
            };

            foreach (var (n, p, r) in layout)
            {
                var c = (GameObject)PrefabUtility.InstantiatePrefab(cottage);
                c.name = n; c.transform.SetParent(envRoot, true);
                c.transform.position = p; c.transform.rotation = r;
                placed++;
            }

            // Warm window glow from inside cottage 01 — Lumen `Light Surface`
            var winGlow = FindLumenPrefab("Light Surface");
            if (winGlow != null)
            {
                var lg = (GameObject)PrefabUtility.InstantiatePrefab(winGlow);
                lg.name = "Cottage01_WindowGlow";
                lg.transform.SetParent(envRoot, true);
                lg.transform.position = new Vector3(-7.8f, 1.8f, -1.6f);
                placed++;
            }
            return placed;
        }

        // ───────────────────────────────────────────────────────────────
        // SECTION 4 — foliage
        // ───────────────────────────────────────────────────────────────

        private static int PlaceFoliage(Transform envRoot)
        {
            int placed = 0;

            // Two MV autumn alders for the close canopy.
            var alder = FindMVPrefab(new[] { "alder_fall", "alder_autumn", "alder_winter" })
                     ?? FindMVPrefab(new[] { "alder" });
            if (alder != null)
            {
                placed += SpawnAt(alder, envRoot, "Alder_NearDoor", new Vector3(-2.5f, 0f, 5.5f));
                placed += SpawnAt(alder, envRoot, "Alder_East", new Vector3(2.8f, 0f, 5.5f), Quaternion.Euler(0, 90, 0));
            }

            // Three Stylized Weather trees for distance + variety.
            for (int i = 1; i <= 5; i++)
            {
                var t = FindWeatherPrefab($"CommonTree_{i}");
                if (t == null) continue;
                var pos = new Vector3((i % 2 == 0 ? 1 : -1) * (5.5f + i * 0.4f), 0f, -1.5f + i * 0.7f);
                placed += SpawnAt(t, envRoot, $"CommonTree_{i}", pos);
                if (placed > 14) break;
            }

            // Bushes + flowers.
            var bush = FindWeatherPrefab("Bush_Common_Flowers");
            if (bush != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    var pos = new Vector3(-7f + i * 4.5f, 0f, -5f + (i % 2 == 0 ? 0.5f : -0.5f));
                    placed += SpawnAt(bush, envRoot, $"Bush_{i:00}", pos);
                }
            }

            // Rocks.
            for (int i = 1; i <= 3; i++)
            {
                var r = FindWeatherPrefab($"Rock_Medium_{i}");
                if (r == null) continue;
                var pos = new Vector3((i - 2) * 4f, 0f, -3f + i * 0.6f);
                placed += SpawnAt(r, envRoot, $"Rock_{i}", pos, Quaternion.Euler(0, i * 73f, 0));
            }

            // Grass tufts sparse along the path edge.
            var grass = FindWeatherPrefab("Grass_Common_Short");
            if (grass != null)
            {
                for (int i = 0; i < 12; i++)
                {
                    var jitterX = ((i * 17) % 100) / 100f * 1.4f - 0.7f;
                    var pos = new Vector3((i % 2 == 0 ? 1.3f : -1.3f) + jitterX, 0f, -7f + i * 1.2f);
                    placed += SpawnAt(grass, envRoot, $"Grass_{i:00}", pos);
                }
            }

            return placed;
        }

        // ───────────────────────────────────────────────────────────────
        // SECTION 5 — fence, well, bench, crates, sign
        // ───────────────────────────────────────────────────────────────

        private static int PlaceVillageDressing(Transform envRoot, MedievalVillageBindings mv)
        {
            int placed = 0;
            var fence = mv?.fencePrefab ?? FindMVPrefab(new[] { "fence", "rail", "palisade" });
            if (fence != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    var f = (GameObject)PrefabUtility.InstantiatePrefab(fence);
                    f.name = $"Fence_{i:00}";
                    f.transform.SetParent(envRoot, true);
                    f.transform.position = new Vector3(-7f + i * 2f, 0f, -8.5f);
                    placed++;
                }
            }

            var well = mv?.wellPrefab ?? FindMVPrefab(new[] { "well", "fountain", "pump" });
            placed += well != null ? SpawnAt(well, envRoot, "Well", new Vector3(5f, 0f, -3f)) : 0;

            var bench = FindMVPrefab(new[] { "bench" });
            placed += bench != null ? SpawnAt(bench, envRoot, "Bench",
                                              new Vector3(4.5f, 0f, 1.5f),
                                              Quaternion.Euler(0, 90, 0)) : 0;

            var woodLog = FindMVPrefab(new[] { "woodlog_03a", "pile_woodlog", "woodstand" });
            placed += woodLog != null ? SpawnAt(woodLog, envRoot, "WoodLogPile",
                                                new Vector3(6.0f, 0f, -2.5f)) : 0;

            var basket = FindMVPrefab(new[] { "wickerbasket", "basket_02a", "basket_01" });
            placed += basket != null ? SpawnAt(basket, envRoot, "WellBasket",
                                               new Vector3(4.0f, 0f, -2.6f)) : 0;

            var sign = FindMVPrefab(new[] { "text_open", "sign_01", "sign_open" });
            placed += sign != null ? SpawnAt(sign, envRoot, "VillageSign",
                                             new Vector3(-3f, 0f, -3.5f)) : 0;

            // Banner on cottage_02 wall — gives Zephyr/wind something to sway.
            var banner = FindMVPrefab(new[] { "flag_gurland", "flag_01", "banner_01" });
            placed += banner != null ? SpawnAt(banner, envRoot, "CottageBanner",
                                               new Vector3(-7.4f, 2.6f, 4.3f),
                                               Quaternion.Euler(0, -20, 0)) : 0;
            return placed;
        }

        // ───────────────────────────────────────────────────────────────
        // SECTION 6 — hanging lantern + Lumen halo + interactable
        // ───────────────────────────────────────────────────────────────

        private static int PlaceHangingLantern(Transform envRoot, MedievalVillageBindings mv)
        {
            int placed = 0;
            var lantern = mv?.lampPostPrefab ?? FindMVPrefab(new[] { "lantern", "lamp", "torch" });
            if (lantern != null)
            {
                var l = (GameObject)PrefabUtility.InstantiatePrefab(lantern);
                l.name = "DoorLantern";
                l.transform.SetParent(envRoot, true);
                l.transform.position = new Vector3(-1.4f, 2.6f, 7.85f);
                placed++;

                // LanternInteractable is added by Phase 27.4 (runtime script
                // not yet in this scene at this point in the pipeline).
                // The capstone wires it explicitly after both 27.2 and 27.3 run.

                // Lumen lantern effect — child halo prefab.
                var halo = FindLumenPrefab("Lantern Effect");
                if (halo != null)
                {
                    var h = (GameObject)PrefabUtility.InstantiatePrefab(halo);
                    h.name = "DoorLantern_Halo";
                    h.transform.SetParent(l.transform, false);
                    h.transform.localPosition = Vector3.zero;
                    placed++;
                }

                // A real Light component as a fallback in case Lumen halo
                // doesn't include one (URP needs a Light source for shadows).
                var bulbGO = new GameObject("DoorLantern_Bulb");
                bulbGO.transform.SetParent(l.transform, false);
                bulbGO.transform.localPosition = Vector3.zero;
                var pl = bulbGO.AddComponent<Light>();
                pl.type = LightType.Point;
                pl.color = new Color(1.0f, 0.78f, 0.42f);
                pl.intensity = 2.8f;
                pl.range = 6.5f;
                pl.shadows = LightShadows.Soft;
                placed++;
            }
            return placed;
        }

        // ───────────────────────────────────────────────────────────────
        // SECTION 7 — atmosphere (leaves, fog, sunshafts)
        // ───────────────────────────────────────────────────────────────

        private static int PlaceAtmosphere(Transform envRoot)
        {
            int placed = 0;

            var atmosphere = new GameObject("_Atmosphere");
            atmosphere.transform.SetParent(envRoot, false);

            // Falling leaves — scene-wide.
            var leaves = FindWeatherPrefab("Leaves");
            if (leaves != null)
            {
                var lf = (GameObject)PrefabUtility.InstantiatePrefab(leaves);
                lf.name = "_Leaves";
                lf.transform.SetParent(atmosphere.transform, false);
                lf.transform.position = new Vector3(0f, 4.5f, 0f);
                placed++;
            }

            // Soft time-of-day fog.
            var fog = FindWeatherPrefab("Fog Soft Time") ?? FindWeatherPrefab("Fog Low");
            if (fog != null)
            {
                var f = (GameObject)PrefabUtility.InstantiatePrefab(fog);
                f.name = "_Fog_Soft";
                f.transform.SetParent(atmosphere.transform, false);
                f.transform.position = Vector3.zero;
                placed++;
            }

            // Sunshafts (Lumen) aimed at the directional light direction (-Z, downward 35°).
            var sunshafts = FindLumenPrefab("Sunshafts Effect");
            if (sunshafts != null)
            {
                var ss = (GameObject)PrefabUtility.InstantiatePrefab(sunshafts);
                ss.name = "_Sunshafts";
                ss.transform.SetParent(atmosphere.transform, false);
                ss.transform.position = new Vector3(0f, 6f, 2f);
                ss.transform.rotation = Quaternion.Euler(35f, 30f, 0f);
                placed++;
            }

            // Subtle WindZone so any Wind-aware shaders pick it up.
            var wzGO = new GameObject("_WindZone");
            wzGO.transform.SetParent(atmosphere.transform, false);
            var wz = wzGO.AddComponent<WindZone>();
            wz.mode = WindZoneMode.Directional;
            wz.windMain = 0.45f;
            wz.windPulseMagnitude = 0.10f;
            wz.windPulseFrequency = 0.15f;
            wzGO.transform.rotation = Quaternion.Euler(0f, 110f, 0f);
            placed++;

            // Reflection probe so the lantern + window glow read clean on metal/wet surfaces.
            var rpGO = new GameObject("_ReflectionProbe");
            rpGO.transform.SetParent(atmosphere.transform, false);
            rpGO.transform.position = new Vector3(0f, 2f, 0f);
            var rp = rpGO.AddComponent<ReflectionProbe>();
            rp.size = new Vector3(40, 12, 40);
            rp.intensity = 0.85f;
            rp.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.OnAwake;
            placed++;

            return placed;
        }

        // ───────────────────────────────────────────────────────────────
        // SECTION 8 — silent villager extra (optional ambient life)
        // ───────────────────────────────────────────────────────────────

        private static int PlaceSilentVillagerExtra(Transform envRoot)
        {
            // Only spawn if a BoZo prefab is available — falls back silently otherwise.
            var prefab = Phase13_BoZoCharacterBuilder.TryGetDorisPrefab();
            if (prefab == null) return 0;

            var extra = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            extra.name = "SilentVillager_01";
            extra.transform.SetParent(envRoot, true);
            extra.transform.position = new Vector3(-6.5f, 0f, -6.5f);
            extra.transform.rotation = Quaternion.Euler(0f, 25f, 0f);

            // No PlayerController, no Interactable — just visual life. Disable
            // any director scripts that might autoplay.
            foreach (var mb in extra.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mb is HearthboundHollow.Player.PlayerController) mb.enabled = false;
                if (mb is HearthboundHollow.Player.Interactable) mb.enabled = false;
            }
            return 1;
        }

        // ───────────────────────────────────────────────────────────────
        // Helpers — prefab finders + spawning
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

        /// <summary>
        /// Best-match search inside MeshingunStudio/ for any of the keyword tokens.
        /// Score = +18 per keyword in the prefab name; +12 per keyword in the path.
        /// Returns the highest-scoring prefab or null if no prefab scores > 0.
        /// </summary>
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

        /// <summary>Exact-name match in the Stylized Weather prefab folder.</summary>
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

        /// <summary>Exact-name match in the Lumen package prefab folder.</summary>
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

        /// <summary>Ensure the GameObject has a BoxCollider sized for raycast pickup.</summary>
        private static void EnsureInteractCollider(GameObject go, Vector3 size)
        {
            if (go.GetComponentInChildren<Collider>(true) != null) return;
            var bc = go.AddComponent<BoxCollider>();
            bc.size = size;
            bc.center = new Vector3(0, size.y * 0.5f, 0);
        }
    }
}
