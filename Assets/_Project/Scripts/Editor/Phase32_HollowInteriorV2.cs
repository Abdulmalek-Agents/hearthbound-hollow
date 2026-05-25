// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase32_HollowInteriorV2
//
// Phase 32.3 — Hollow interior polish v2.
//
// Adds the props Phase 27.3 missed:
//   1. Kettle on the hearth (the "kettle just stopped" callback from
//      Doris's opening dialogue)
//   2. Bread loaves on the west shelf (Doris brings bread when she visits)
//   3. Three hanging dried-herb bundles from the ceiling beams (foreshadowing
//      Mission 2's tea-brewing mechanic + lavender/valerian herbs)
//   4. Cupboard against the south end of the east wall (Marin's ledgers)
//   5. Stool beside the hearth (where Marin sat; Pickle may end up on it)
//   6. Candelabra on the workbench (more candle warmth + Lumen halo)
//   7. Bucket beside hearth (water-pail for Mission 2 cleansing)
//   8. Workbench dressing — a couple of small props (cup, apple)
//   9. Improved hearth glow (additional Lumen Shimmery Light for pulse)
//   10. Wall-mounted candle sconces on the west wall above the shelves
//
// All additions are parented under `_Phase32Env_Hollow` — Phase 27.3's
// `_Phase27Env_Hollow` is preserved.
//
// USE: Menu → Hearthbound → 🏠 Phase 32.3 — Polish Hollow Interior V2
//
// Architecture notes:
//   - Per D-007: scene edited + saved.
//   - Per D-027: TryGet* bindings.
//   - Per D-035: Editor-only.

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HearthboundHollow.EditorTools
{
    public static class Phase32_HollowInteriorV2
    {
        private const string SceneHollow     = "Assets/_Project/Scenes/03_Mission01_Hollow.unity";
        private const string MVRoot          = "Assets/MeshingunStudio";
        private const string LumenPrefabs    = "Packages/com.distantlands.lumen/Content/Prefabs";

        private const string EnvParentName   = "_Phase32Env_Hollow";

        [MenuItem("Hearthbound/🏠 Phase 32.3 — Polish Hollow Interior V2", priority = 33)]
        public static void Build()
        {
            if (!System.IO.File.Exists(SceneHollow))
            {
                EditorUtility.DisplayDialog("Phase 32.3",
                    $"Scene not found: {SceneHollow}\n\n" +
                    "Run Phase 23 (capstone) first so the scene exists.", "OK");
                return;
            }

            var binds = Phase32_VillageBindingsExtension.TryGetBindings();
            if (binds == null)
            {
                Phase32_VillageBindingsExtension.Build();
                binds = Phase32_VillageBindingsExtension.TryGetBindings();
            }

            var scene = EditorSceneManager.OpenScene(SceneHollow, OpenSceneMode.Single);

            // Wipe previous Phase 32.3 pass — idempotent.
            var existing = GameObject.Find(EnvParentName);
            if (existing != null) Object.DestroyImmediate(existing);

            var envRoot = new GameObject(EnvParentName);

            int placed = 0;
            placed += PlaceKettleOnHearth(envRoot.transform, binds);
            placed += PlaceBreadOnShelf(envRoot.transform, binds);
            placed += PlaceHangingHerbs(envRoot.transform, binds);
            placed += PlaceCupboardAndStool(envRoot.transform, binds);
            placed += PlaceWorkbenchCandelabra(envRoot.transform, binds);
            placed += PlaceBucketAndWorkbenchProps(envRoot.transform, binds);
            placed += PlaceWallCandleSconces(envRoot.transform, binds);
            placed += AddHearthPulseGlow(envRoot.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog("Phase 32.3 — Hollow V2 polished",
                $"🏠 Hollow v2 interior built. {placed} additions placed under " +
                $"'_Phase32Env_Hollow'.\n\n" +
                "Re-run any time. Phase 27.3's '_Phase27Env_Hollow' is preserved.\n\n" +
                "Next: Hearthbound → 🌅 Phase 32.4 — Apply Cozy URP Volume",
                "OK");
        }

        // ───────────────────────────────────────────────────────────────
        // SECTION 1 — Kettle on the hearth (Doris's "kettle just stopped" callback)
        // ───────────────────────────────────────────────────────────────

        private static int PlaceKettleOnHearth(Transform envRoot, MedievalVillageBindingsV2 binds)
        {
            int placed = 0;

            // Find the existing Hearth in the scene (placed by Phase 27.3).
            var hearth = GameObject.Find("Hearth");
            if (hearth == null)
            {
                Debug.LogWarning("[Hearthbound/Phase 32.3] Hearth not found — placing kettle at fallback location.");
            }

            var kettle = binds?.kettlePrefab ?? FindMVPrefab(new[] { "wooden_pitcher_01", "wooden_pitcher", "pot_01" });
            if (kettle == null) return 0;

            Vector3 kettlePos = hearth != null
                ? hearth.transform.position + new Vector3(0.5f, 0.5f, -0.2f)
                : new Vector3(-3.5f, 0.5f, 3.8f);

            var k = (GameObject)PrefabUtility.InstantiatePrefab(kettle);
            k.name = "Hearth_Kettle";
            k.transform.SetParent(envRoot, true);
            k.transform.position = kettlePos;
            k.transform.rotation = Quaternion.Euler(0f, 35f, 0f);
            placed++;

            // Tiny VFX wisp out of the kettle's mouth (using ParticleSystem on a
            // child empty — no asset needed). Disabled by default so the cozy
            // scene doesn't have a steam stream on first load; user can enable.
            var steamGO = new GameObject("Kettle_SteamWisp");
            steamGO.transform.SetParent(k.transform, false);
            steamGO.transform.localPosition = new Vector3(0f, 0.3f, 0f);
            var ps = steamGO.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 1.8f;
            main.startSpeed = 0.25f;
            main.startSize = 0.18f;
            main.startColor = new Color(1f, 0.95f, 0.85f, 0.18f);
            main.maxParticles = 12;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.rateOverTime = 4f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 8f;
            shape.radius = 0.04f;
            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            var sizeCurve = new AnimationCurve(
                new Keyframe(0f, 0.3f),
                new Keyframe(1f, 1.6f));
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
            // Disable colour-over-lifetime requires Gradient — keep simple.
            ps.Stop();   // Off by default
            placed++;

            return placed;
        }

        // ───────────────────────────────────────────────────────────────
        // SECTION 2 — Bread loaves on the west shelf (Doris's domain)
        // ───────────────────────────────────────────────────────────────

        private static int PlaceBreadOnShelf(Transform envRoot, MedievalVillageBindingsV2 binds)
        {
            int placed = 0;
            var bread = binds?.breadLoafPrefab ?? FindMVPrefab(new[] { "bread_01a", "bread" });
            if (bread == null) return 0;

            // West shelf is at x = -6.8 per Phase 27.3 (Shelf_West_Top / Bottom).
            // Three loaves spread along the shelf top.
            for (int i = 0; i < 3; i++)
            {
                var b = (GameObject)PrefabUtility.InstantiatePrefab(bread);
                b.name = $"Shelf_Bread_{i}";
                b.transform.SetParent(envRoot, true);
                b.transform.position = new Vector3(-6.7f, 0.97f, -0.6f + i * 0.55f);
                b.transform.rotation = Quaternion.Euler(0f, i * 23f, 0f);
                placed++;
            }
            return placed;
        }

        // ───────────────────────────────────────────────────────────────
        // SECTION 3 — Hanging dried-herb bundles (Mission 2 foreshadowing)
        // ───────────────────────────────────────────────────────────────

        private static int PlaceHangingHerbs(Transform envRoot, MedievalVillageBindingsV2 binds)
        {
            int placed = 0;
            // Inverted TerraPot serves as a dried-herb bundle proxy.
            // Color tinting (lavender purple, sage green, valerian yellow) would
            // require material instances — leave that to the artist's pass.
            var herbProxy = binds?.hangingPotPrefab ?? FindMVPrefab(new[] { "terrapots_01b", "terrapots_01" });
            if (herbProxy == null) return 0;

            var group = new GameObject("HangingHerbs");
            group.transform.SetParent(envRoot, false);

            // 3 herb bundles hanging from the ceiling beams (beams are at y=4
            // in Phase 27.3). Hang them at y=3.4 over the workbench.
            var positions = new[]
            {
                ("Herb_Lavender",  new Vector3( 3.0f, 3.4f, 3.0f)),
                ("Herb_Valerian",  new Vector3( 4.0f, 3.4f, 3.0f)),
                ("Herb_Sage",      new Vector3( 5.0f, 3.4f, 3.0f)),
            };

            foreach (var (name, pos) in positions)
            {
                var h = (GameObject)PrefabUtility.InstantiatePrefab(herbProxy);
                h.name = name;
                h.transform.SetParent(group.transform, true);
                h.transform.position = pos;
                h.transform.rotation = Quaternion.Euler(180f, 0f, 0f); // Inverted to hang
                h.transform.localScale = Vector3.one * 0.35f;
                placed++;

                // Thin string-rope from the beam to the herb (Cylinder primitive
                // scaled tall + thin — a quick stand-in).
                var rope = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                rope.name = $"{name}_Rope";
                rope.transform.SetParent(group.transform, true);
                rope.transform.position = pos + new Vector3(0f, 0.55f, 0f);
                rope.transform.localScale = new Vector3(0.02f, 0.5f, 0.02f);
                // Remove the collider (decorative only)
                Object.DestroyImmediate(rope.GetComponent<Collider>());
                placed++;
            }

            return placed;
        }

        // ───────────────────────────────────────────────────────────────
        // SECTION 4 — Cupboard + stool (intimate furniture)
        // ───────────────────────────────────────────────────────────────

        private static int PlaceCupboardAndStool(Transform envRoot, MedievalVillageBindingsV2 binds)
        {
            int placed = 0;

            var cupboard = binds?.cupboardPrefab ?? FindMVPrefab(new[] { "cupboard_01a", "cupboard" });
            if (cupboard != null)
            {
                var c = (GameObject)PrefabUtility.InstantiatePrefab(cupboard);
                c.name = "MarinsCupboard";
                c.transform.SetParent(envRoot, true);
                c.transform.position = new Vector3(7f, 0f, -3.5f); // Southeast corner of interior
                c.transform.rotation = Quaternion.Euler(0f, -90f, 0f); // Face into the room
                placed++;
            }

            var stool = binds?.stoolPrefab ?? FindMVPrefab(new[] { "stool_01a", "chair_01a", "stool" });
            if (stool != null)
            {
                var s = (GameObject)PrefabUtility.InstantiatePrefab(stool);
                s.name = "MarinsStool";
                s.transform.SetParent(envRoot, true);
                s.transform.position = new Vector3(-3f, 0f, 3.2f); // Beside the hearth
                s.transform.rotation = Quaternion.Euler(0f, -45f, 0f);
                placed++;
            }

            return placed;
        }

        // ───────────────────────────────────────────────────────────────
        // SECTION 5 — Candelabra on the workbench (Lumen-friendly bloom)
        // ───────────────────────────────────────────────────────────────

        private static int PlaceWorkbenchCandelabra(Transform envRoot, MedievalVillageBindingsV2 binds)
        {
            int placed = 0;
            var candelabra = binds?.candleabraPrefab ?? FindMVPrefab(new[] { "candleabra_02", "candleabra" });
            if (candelabra == null) return 0;

            var c = (GameObject)PrefabUtility.InstantiatePrefab(candelabra);
            c.name = "WorkbenchCandelabra";
            c.transform.SetParent(envRoot, true);
            c.transform.position = new Vector3(4.5f, 1.05f, 2.5f); // On the workbench top
            c.transform.rotation = Quaternion.Euler(0f, 25f, 0f);
            placed++;

            // Lumen halo at each candle flame (one per arm). Approximate single-
            // flame position above the candelabra centre.
            var halo = FindLumenPrefab("Shimmery Light");
            if (halo != null)
            {
                var h = (GameObject)PrefabUtility.InstantiatePrefab(halo);
                h.name = "WorkbenchCandelabra_Glow";
                h.transform.SetParent(c.transform, false);
                h.transform.localPosition = new Vector3(0f, 0.6f, 0f);
                h.transform.localScale = Vector3.one * 0.45f;
                placed++;
            }

            // Real Point Light for shadow casting (additive to the workbench candle).
            var bulbGO = new GameObject("Candelabra_Light");
            bulbGO.transform.SetParent(c.transform, false);
            bulbGO.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            var pl = bulbGO.AddComponent<Light>();
            pl.type = LightType.Point;
            pl.color = new Color(1.0f, 0.78f, 0.42f);
            pl.intensity = 1.6f;
            pl.range = 3.5f;
            pl.shadows = LightShadows.None;
            placed++;

            return placed;
        }

        // ───────────────────────────────────────────────────────────────
        // SECTION 6 — Bucket beside hearth + workbench cup + apple
        // ───────────────────────────────────────────────────────────────

        private static int PlaceBucketAndWorkbenchProps(Transform envRoot, MedievalVillageBindingsV2 binds)
        {
            int placed = 0;

            // Water-pail by the hearth (Mission 2 cleansing tie-in).
            var bucket = binds?.bucketPrefab ?? FindMVPrefab(new[] { "bucket_01a", "bucket" });
            if (bucket != null)
            {
                var b = (GameObject)PrefabUtility.InstantiatePrefab(bucket);
                b.name = "Hearth_WaterBucket";
                b.transform.SetParent(envRoot, true);
                b.transform.position = new Vector3(-2.5f, 0f, 4.5f);
                b.transform.rotation = Quaternion.Euler(0f, -25f, 0f);
                placed++;
            }

            // Wooden cup on the workbench
            var cup = FindMVPrefab(new[] { "wooden_cup_01", "wooden_cup", "cup_01" });
            if (cup != null)
            {
                var c = (GameObject)PrefabUtility.InstantiatePrefab(cup);
                c.name = "Workbench_Cup";
                c.transform.SetParent(envRoot, true);
                c.transform.position = new Vector3(3.6f, 1.05f, 3.6f);
                c.transform.rotation = Quaternion.Euler(0f, 17f, 0f);
                placed++;
            }

            // Apple on the workbench (small life-detail)
            var apple = FindMVPrefab(new[] { "apple_01a", "apple_01b", "apple_02a", "apple" });
            if (apple != null)
            {
                var a = (GameObject)PrefabUtility.InstantiatePrefab(apple);
                a.name = "Workbench_Apple";
                a.transform.SetParent(envRoot, true);
                a.transform.position = new Vector3(4.3f, 1.05f, 3.55f);
                a.transform.rotation = Quaternion.Euler(0f, 47f, 0f);
                placed++;
            }

            // A small bowl
            var bowl = FindMVPrefab(new[] { "wooden_bowl_01", "wooden_bowl", "bowl_01" });
            if (bowl != null)
            {
                var bw = (GameObject)PrefabUtility.InstantiatePrefab(bowl);
                bw.name = "Workbench_Bowl";
                bw.transform.SetParent(envRoot, true);
                bw.transform.position = new Vector3(3.1f, 1.05f, 2.7f);
                placed++;
            }

            return placed;
        }

        // ───────────────────────────────────────────────────────────────
        // SECTION 7 — Wall candle sconces on the west wall
        // ───────────────────────────────────────────────────────────────

        private static int PlaceWallCandleSconces(Transform envRoot, MedievalVillageBindingsV2 binds)
        {
            int placed = 0;
            var candle = binds?.thickCandlePrefab ?? FindMVPrefab(new[] { "thickcandle_01a", "thickcandle", "wax_04a" });
            if (candle == null) return 0;

            var group = new GameObject("WallCandleSconces");
            group.transform.SetParent(envRoot, false);

            // 2 sconces on the west wall above shelves (at x=-7.7, y=2.2)
            var positions = new[]
            {
                ("Sconce_W_North", new Vector3(-7.7f, 2.2f,  2.0f)),
                ("Sconce_W_South", new Vector3(-7.7f, 2.2f, -2.0f)),
            };

            foreach (var (name, pos) in positions)
            {
                var c = (GameObject)PrefabUtility.InstantiatePrefab(candle);
                c.name = name;
                c.transform.SetParent(group.transform, true);
                c.transform.position = pos;
                c.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                placed++;

                // Lumen halo on each
                var halo = FindLumenPrefab("Shimmery Light");
                if (halo != null)
                {
                    var h = (GameObject)PrefabUtility.InstantiatePrefab(halo);
                    h.name = $"{name}_Glow";
                    h.transform.SetParent(c.transform, false);
                    h.transform.localPosition = new Vector3(0f, 0.3f, 0f);
                    h.transform.localScale = Vector3.one * 0.3f;
                    placed++;
                }
            }
            return placed;
        }

        // ───────────────────────────────────────────────────────────────
        // SECTION 8 — Additional hearth pulse glow (intensifies the warmth)
        // ───────────────────────────────────────────────────────────────

        private static int AddHearthPulseGlow(Transform envRoot)
        {
            int placed = 0;
            var hearth = GameObject.Find("Hearth");
            if (hearth == null) return 0;

            // Add a SECOND Lumen Shimmery Light at the hearth for a stronger
            // pulsing glow (the Phase 27.3 one is small).
            var halo = FindLumenPrefab("Shimmery Light");
            if (halo != null)
            {
                var h = (GameObject)PrefabUtility.InstantiatePrefab(halo);
                h.name = "Hearth_GlowBig";
                h.transform.SetParent(envRoot, true);
                h.transform.position = hearth.transform.position + new Vector3(0f, 0.8f, 0f);
                h.transform.localScale = Vector3.one * 1.4f;
                placed++;
            }

            return placed;
        }

        // ───────────────────────────────────────────────────────────────
        // Helpers
        // ───────────────────────────────────────────────────────────────

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
