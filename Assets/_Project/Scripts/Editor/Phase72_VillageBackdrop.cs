// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase72_VillageBackdrop
//
// PHASE 72 — "Make the arenas feel like a real, big village." Adds a generous,
// reversible BACKDROP + LIFE + ATMOSPHERE layer on every gameplay arena, on top
// of Phase 60 (Hollow), 62 (Garden) and 63 (World Polish vista/greenery). Where
// Phase 63 frames the play space, Phase 72 builds the *world around it* so each
// scene reads as one corner of a living autumn town rather than a small stage.
//
// Per arena, under a single managed root "_Phase72_Backdrop" (delete to revert):
//   1) BACKDROP VILLAGE — a ring of the project's authored cottages
//      (Cottage_A_Bakery / B_Plain / C_Gabled / D_Corner) placed FAR outside the
//      gameplay boundary, facing inward, so a believable skyline of homes sits
//      beyond the walls. Background scenery → no gameplay colliders, can't block.
//   2) AUTUMN TREE BELT — two depth rings of Medieval Village fall alders + pines
//      for a warm, layered horizon.
//   3) GROUND-COVER LIFE — Medieval Village grass-foliage tufts scattered across
//      the meadow/lane (deterministic Perlin) so the ground reads as living, not
//      a slab — complements the Phase 71 green-grass material.
//   4) ATMOSPHERE — warm dusk point-lights + flickering torch/candle fire on the
//      backdrop + drifting chimney smoke (VoluSmoke if present, else a soft
//      built-in plume) + a high fake-cloud plane for sky depth.
//   5) MARKET LIFE (Lane) — extra shop stands dressed with produce crates, food
//      sacks, a signboard and a festival garland string → a lived-in market row.
//
// FULLY DEFENSIVE + REVERSIBLE: every prop is raycast-grounded (no floating/
// clipping), every step is try/catch-wrapped so a missing prefab can never break
// a scene, and the whole layer is one deletable root. Background props carry NO
// colliders (they're scenery beyond the play area); only nothing here touches the
// gameplay collision walls (Phase 47) or the Phase 71 progression net. Chained
// into 🚀 Build Everything (Step 22, after Phase 63).

using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HearthboundHollow.EditorTools
{
    public static class Phase72_VillageBackdrop
    {
        private const string MV   = "Assets/MeshingunStudio";          // Medieval Village
        private const string VF   = "Assets/VertexField";              // VoluSmoke FX
        private const string ENV  = "Assets/_Project/Prefabs/Environment"; // authored cottages
        private const string Root = "_Phase72_Backdrop";

        private const string LaneScene    = "Assets/_Project/Scenes/02_Mission01_Lane.unity";
        private const string GardenScene  = "Assets/_Project/Scenes/04_Mission02_Garden.unity";
        private const string CottageScene = "Assets/_Project/Scenes/05_Mission02_Cottage.unity";

        // Warm autumn-dusk palette.
        private static readonly Color Dusk = new Color(1f, 0.82f, 0.52f);

        [MenuItem("Hearthbound/⚙️ Advanced/🏘️ Phase 72 — Village Backdrop + Atmosphere", priority = 640)]
        public static void Build()
        {
            string original = EditorSceneManager.GetActiveScene().path;
            var log = new StringBuilder("═══ Phase 72 — Village Backdrop + Atmosphere ═══\n");

            Dress(LaneScene,    "lane",    log);
            Dress(GardenScene,  "garden",  log);
            Dress(CottageScene, "cottage", log);

            if (!string.IsNullOrEmpty(original) && System.IO.File.Exists(original))
                EditorSceneManager.OpenScene(original, OpenSceneMode.Single);
            Debug.Log(log.ToString());
        }

        private static void Dress(string scenePath, string kind, StringBuilder log)
        {
            if (!System.IO.File.Exists(scenePath)) { log.AppendLine($"  {kind}: scene missing — skip."); return; }
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            var existing = FindRoot(scene, Root);
            if (existing != null) Object.DestroyImmediate(existing);
            var root = new GameObject(Root);
            SceneManager.MoveGameObjectToScene(root, scene);
            root.transform.position = Vector3.zero;

            float gy = ProbeGroundY(scene);
            Vector3 hub = new Vector3(0f, gy, kind == "garden" ? 6f : 0f);

            int n = 0;
            try { n += BackdropVillage(root.transform, kind, hub, gy); } catch (System.Exception e) { log.AppendLine($"  {kind} village: {e.Message}"); }
            try { n += TreeBelt(root.transform, kind, hub, gy); }        catch (System.Exception e) { log.AppendLine($"  {kind} trees: {e.Message}"); }
            try { n += GroundCover(root.transform, kind, hub, gy); }     catch (System.Exception e) { log.AppendLine($"  {kind} grass: {e.Message}"); }
            try { n += Atmosphere(root.transform, kind, hub, gy); }      catch (System.Exception e) { log.AppendLine($"  {kind} atmo: {e.Message}"); }
            if (kind == "lane")
                try { n += MarketLife(root.transform, gy); }            catch (System.Exception e) { log.AppendLine($"  lane market: {e.Message}"); }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            log.AppendLine($"  {kind}: placed {n} backdrop object(s) (groundY≈{gy:F1}).");
        }

        // ── 1) Backdrop village (authored cottages, far out, facing inward) ──────
        private static int BackdropVillage(Transform parent, string kind, Vector3 hub, float gy)
        {
            var holder = new GameObject("BackdropVillage"); holder.transform.SetParent(parent, false);
            var cottages = new List<GameObject>();
            foreach (var nm in new[] { "Cottage_A_Bakery", "Cottage_B_Plain", "Cottage_C_Gabled", "Cottage_D_Corner" })
            {
                var p = AssetDatabase.LoadAssetAtPath<GameObject>($"{ENV}/{nm}.prefab");
                if (p != null) cottages.Add(p);
            }
            if (cottages.Count == 0) return 0;

            // A ring of homes well beyond the play boundary. Cottage scene is small/
            // interior-ish, so a tighter ring; lane/garden get a wide town skyline.
            float r = kind == "cottage" ? 26f : 34f;
            int count = kind == "cottage" ? 5 : 8;
            int placed = 0;
            for (int i = 0; i < count; i++)
            {
                float a = (i / (float)count) * Mathf.PI * 2f + 0.35f;
                float rr = r * (0.9f + 0.18f * Mathf.PerlinNoise(i * 1.7f, 2.3f));
                Vector3 pos = hub + new Vector3(Mathf.Cos(a) * rr, 12f, Mathf.Sin(a) * rr);
                // Face the hub (inward), so rooftops/fronts read toward the player.
                float yaw = Mathf.Atan2(hub.x - pos.x, hub.z - pos.z) * Mathf.Rad2Deg;
                var prefab = cottages[i % cottages.Count];
                // No collider: pure background scenery beyond the gameplay walls.
                placed += Spawn(prefab, holder.transform, $"Home_{i}", pos,
                                Quaternion.Euler(0f, yaw, 0f), 1f, gy, collide: false);
            }
            return placed;
        }

        // ── 2) Autumn tree belt (two depth rings) ────────────────────────────────
        private static int TreeBelt(Transform parent, string kind, Vector3 hub, float gy)
        {
            var holder = new GameObject("TreeBelt"); holder.transform.SetParent(parent, false);
            var trees = new List<GameObject>();
            foreach (var nm in new[] { "SM_Alder_Fall_01a", "SM_Alder_Fall_01b", "SM_PineTree_Spring_01a", "SM_PineTree_Spring_03a" })
            {
                var p = FindByName(MV, nm);
                if (p != null) trees.Add(p);
            }
            if (trees.Count == 0) { var any = FindPrefab(MV, new[] { "Tree", "Alder", "Pine" }); if (any != null) trees.Add(any); }
            if (trees.Count == 0) return 0;

            int placed = 0;
            // Inner ring (between play edge and the cottages) + outer ring (horizon).
            float[] radii = kind == "cottage" ? new[] { 18f, 30f } : new[] { 22f, 40f };
            int[]   counts = new[] { 12, 16 };
            for (int ring = 0; ring < radii.Length; ring++)
            {
                for (int i = 0; i < counts[ring]; i++)
                {
                    float a = (i / (float)counts[ring]) * Mathf.PI * 2f + ring * 0.4f;
                    float rr = radii[ring] * (0.85f + 0.3f * Mathf.PerlinNoise(i * 0.7f, ring + 0.2f));
                    Vector3 pos = hub + new Vector3(Mathf.Cos(a) * rr, 14f, Mathf.Sin(a) * rr);
                    var prefab = trees[(i + ring) % trees.Count];
                    float s = 1.1f + 0.7f * Mathf.PerlinNoise(i * 1.3f, ring * 2.1f);
                    placed += Spawn(prefab, holder.transform, $"Tree_{ring}_{i}", pos,
                                    Quaternion.Euler(0f, i * 53f, 0f), s, gy, collide: false);
                }
            }
            return placed;
        }

        // ── 3) Ground-cover grass tufts across the play meadow ───────────────────
        private static int GroundCover(Transform parent, string kind, Vector3 hub, float gy)
        {
            if (kind == "cottage") return 0; // interior-ish; skip grass
            var holder = new GameObject("GroundCover"); holder.transform.SetParent(parent, false);
            var grass = FindByName(MV, "SM_Grass_01a_Foliage") ?? FindByName(MV, "SM_Grass_01a")
                        ?? FindPrefab(MV, new[] { "Grass", "Foliage" });
            if (grass == null) return 0;

            int placed = 0;
            // Scatter within a comfortable radius of the play area (deterministic).
            float spread = kind == "lane" ? 9f : 8f;
            for (int i = 0; i < 28; i++)
            {
                float nx = Mathf.PerlinNoise(i * 0.37f, 0.11f) * 2f - 1f;
                float nz = Mathf.PerlinNoise(0.53f, i * 0.31f) * 2f - 1f;
                Vector3 pos = hub + new Vector3(nx * spread, 4f, nz * spread);
                float s = 0.7f + 0.6f * Mathf.PerlinNoise(i * 0.9f, 3.3f);
                placed += Spawn(grass, holder.transform, $"Grass_{i}", pos,
                                Quaternion.Euler(0f, i * 71f, 0f), s, gy, collide: false);
            }
            return placed;
        }

        // ── 4) Warm dusk atmosphere ──────────────────────────────────────────────────
        private static int Atmosphere(Transform parent, string kind, Vector3 hub, float gy)
        {
            var holder = new GameObject("Atmosphere"); holder.transform.SetParent(parent, false);
            int placed = 0;

            // Warm rim point-lights around the backdrop for a golden-hour glow.
            int lamps = kind == "cottage" ? 4 : 6;
            float lr = kind == "cottage" ? 16f : 24f;
            for (int i = 0; i < lamps; i++)
            {
                float a = (i / (float)lamps) * Mathf.PI * 2f;
                Vector3 pos = hub + new Vector3(Mathf.Cos(a) * lr, gy + 4.5f, Mathf.Sin(a) * lr);
                placed += AddLight(holder.transform, $"DuskGlow_{i}", pos, Dusk, range: 18f, intensity: 1.4f);
            }

            // Flickering fire on a few torch points (real particle prefabs).
            var torch = FindByName(MV, "PS_TorchFire") ?? FindByName(MV, "PS_FireMain");
            if (torch != null)
            {
                int fires = kind == "cottage" ? 2 : 4;
                float fr = kind == "cottage" ? 12f : 17f;
                for (int i = 0; i < fires; i++)
                {
                    float a = (i / (float)fires) * Mathf.PI * 2f + 0.6f;
                    Vector3 pos = hub + new Vector3(Mathf.Cos(a) * fr, gy + 1.4f, Mathf.Sin(a) * fr);
                    placed += Spawn(torch, holder.transform, $"Torch_{i}", pos, Quaternion.identity, 1f, gy, collide: false, ground: false);
                }
            }

            // Drifting chimney smoke over the backdrop homes (VoluSmoke if present,
            // else a soft built-in plume) — sells "warm autumn dusk".
            var smoke = FindPrefab(VF, new[] { "smoke", "plume", "backdrop" });
            int chimneys = kind == "cottage" ? 2 : 4;
            float cr = kind == "cottage" ? 24f : 32f;
            for (int i = 0; i < chimneys; i++)
            {
                float a = (i / (float)chimneys) * Mathf.PI * 2f + 0.9f;
                Vector3 pos = hub + new Vector3(Mathf.Cos(a) * cr, gy + 6.5f, Mathf.Sin(a) * cr);
                if (smoke != null)
                    placed += Spawn(smoke, holder.transform, $"Chimney_{i}", pos, Quaternion.identity, 1f, gy, collide: false, ground: false);
                else
                    placed += BuiltinSmoke(holder.transform, $"ChimneySmoke_{i}", pos);
            }

            // A high fake-cloud plane for sky depth (lane/garden only — open sky).
            if (kind != "cottage")
            {
                var cloud = FindByName(MV, "SM_FakeCloudPlane");
                if (cloud != null)
                    placed += Spawn(cloud, holder.transform, "SkyClouds", hub + new Vector3(0f, 46f, 0f),
                                    Quaternion.identity, 6f, gy, collide: false, ground: false);
            }
            return placed;
        }

        // ── 5) Lane market life ──────────────────────────────────────────────────
        private static int MarketLife(Transform parent, float gy)
        {
            var holder = new GameObject("MarketRow"); holder.transform.SetParent(parent, false);
            int placed = 0;

            (string name, Vector3 pos, float yaw)[] stands =
            {
                ("SM_ShopStand_01a", new Vector3(-4.2f, 8f, 6.5f),  90f),
                ("SM_ShopStand_01a", new Vector3( 4.2f, 8f, 8.5f), -90f),
            };
            foreach (var s in stands)
            {
                var p = FindByName(MV, s.name) ?? FindPrefab(MV, new[] { "ShopStand", "Stand", "Stall" });
                if (p != null) placed += Spawn(p, holder.transform, "Stall_" + s.pos.x, s.pos, Quaternion.Euler(0f, s.yaw, 0f), 1f, gy, collide: true);
            }

            // Produce + sacks dressing the stalls.
            (string[] kw, Vector3 pos)[] goods =
            {
                (new[]{"Crate_Apple","Crate_Apple_01a"}, new Vector3(-3.8f, 8f, 6.0f)),
                (new[]{"Crate_Orange","Crate_Orange_01a"}, new Vector3(-3.8f, 8f, 7.0f)),
                (new[]{"FoodSac_01","FoodSac"}, new Vector3(-4.6f, 8f, 6.5f)),
                (new[]{"Crate_Lemon","Crate_Lemon_01a"}, new Vector3(3.8f, 8f, 8.0f)),
                (new[]{"FoodSac_02","FoodSac"}, new Vector3(4.6f, 8f, 9.0f)),
                (new[]{"Barrel_01","Barrel"}, new Vector3(3.7f, 8f, 9.2f)),
            };
            foreach (var g in goods)
            {
                var p = FindPrefab(MV, g.kw);
                if (p != null) placed += Spawn(p, holder.transform, "Goods_" + g.pos.x + "_" + g.pos.z, g.pos, Quaternion.identity, 1f, gy, collide: false);
            }

            // A signboard + a festival garland string for a lived-in market.
            var sign = FindByName(MV, "SM_Signboard_01b") ?? FindPrefab(MV, new[] { "Signboard", "Sign" });
            if (sign != null) placed += Spawn(sign, holder.transform, "MarketSign", new Vector3(-4.6f, 8f, 5.5f), Quaternion.Euler(0f, 90f, 0f), 1f, gy, collide: false);
            var garland = FindByName(MV, "SM_FlagGurland_01a") ?? FindPrefab(MV, new[] { "FlagGurland", "Garland", "Bunting", "Flag" });
            if (garland != null)
            {
                placed += Spawn(garland, holder.transform, "Garland_A", new Vector3(0f, gy + 3.2f, 5.0f), Quaternion.identity, 1f, gy, collide: false, ground: false);
                placed += Spawn(garland, holder.transform, "Garland_B", new Vector3(0f, gy + 3.2f, 9.0f), Quaternion.Euler(0f, 18f, 0f), 1f, gy, collide: false, ground: false);
            }
            return placed;
        }

        // ── shared helpers ───────────────────────────────────────────────────────
        private static int AddLight(Transform parent, string name, Vector3 pos, Color color, float range, float intensity)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.position = pos;
            var l = go.AddComponent<Light>();
            l.type = LightType.Point; l.color = color; l.range = range; l.intensity = intensity; l.shadows = LightShadows.None;
            return 1;
        }

        // Soft built-in smoke plume (fallback when VoluSmoke prefab isn't found).
        private static int BuiltinSmoke(Transform parent, string name, Vector3 pos)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.position = pos;
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 4.5f; main.startSpeed = 0.6f; main.startSize = 1.4f;
            main.startColor = new Color(0.8f, 0.78f, 0.74f, 0.5f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 24;
            var em = ps.emission; em.rateOverTime = 4f;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Cone; sh.angle = 8f; sh.radius = 0.2f;
            var col = ps.colorOverLifetime; col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(new Color(0.85f, 0.83f, 0.8f), 0f), new GradientColorKey(new Color(0.7f, 0.7f, 0.72f), 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.45f, 0.2f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;
            var rend = go.GetComponent<ParticleSystemRenderer>();
            var sh2 = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Sprites/Default");
            if (sh2 != null) rend.sharedMaterial = new Material(sh2);
            return 1;
        }

        private static int Spawn(GameObject prefab, Transform parent, string name, Vector3 pos, Quaternion rot,
                                 float scale, float fallbackY, bool collide = false, bool ground = true)
        {
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (inst == null) return 0;
            inst.name = name;
            inst.transform.SetParent(parent, true);
            inst.transform.position = pos;
            inst.transform.rotation = rot;
            inst.transform.localScale = Vector3.one * scale;
            if (ground) GroundToFloor(inst.transform, fallbackY);
            if (collide) EnsureCollider(inst);
            return 1;
        }

        private static void GroundToFloor(Transform t, float fallbackY)
        {
            var rends = t.GetComponentsInChildren<Renderer>(true);
            if (rends.Length == 0) { var p0 = t.position; p0.y = fallbackY; t.position = p0; return; }
            Bounds b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
            Vector3 from = new Vector3(b.center.x, b.max.y + 0.5f, b.center.z);
            float floorY = Physics.Raycast(from, Vector3.down, out RaycastHit hit, 80f, ~0, QueryTriggerInteraction.Ignore) ? hit.point.y : fallbackY;
            t.position += new Vector3(0f, (floorY + 0.01f) - b.min.y, 0f);
        }

        private static void EnsureCollider(GameObject go)
        {
            if (go.GetComponentInChildren<Collider>(true) != null) return;
            var rends = go.GetComponentsInChildren<Renderer>(true);
            if (rends.Length == 0) return;
            Bounds b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
            var bc = go.AddComponent<BoxCollider>();
            bc.center = go.transform.InverseTransformPoint(b.center);
            bc.size = new Vector3(
                b.size.x / Mathf.Max(0.0001f, go.transform.lossyScale.x),
                b.size.y / Mathf.Max(0.0001f, go.transform.lossyScale.y),
                b.size.z / Mathf.Max(0.0001f, go.transform.lossyScale.z));
        }

        private static float ProbeGroundY(Scene scene)
        {
            if (Physics.Raycast(new Vector3(0f, 50f, 0f), Vector3.down, out RaycastHit hit, 200f, ~0, QueryTriggerInteraction.Ignore))
                return hit.point.y;
            foreach (var nm in new[] { "Phase47_GroundEarth", "GardenSoilGround", "Ground", "WoodFloor" })
            {
                var t = FindDeep(scene, nm);
                if (t != null) return t.position.y;
            }
            return 0f;
        }

        private static GameObject FindByName(string rootDir, string exact)
        {
            if (!System.IO.Directory.Exists(rootDir)) return null;
            foreach (var g in AssetDatabase.FindAssets("t:Prefab " + exact, new[] { rootDir }))
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var p = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (p != null && p.name == exact) return p;
            }
            return null;
        }

        private static GameObject FindPrefab(string rootDir, string[] keywords)
        {
            if (!System.IO.Directory.Exists(rootDir)) return null;
            var best = new List<(int score, GameObject prefab)>();
            foreach (var g in AssetDatabase.FindAssets("t:Prefab", new[] { rootDir }))
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var lower = path.ToLowerInvariant();
                if (lower.Contains("/scenes/") || lower.Contains("/demo") || lower.Contains("preview") || lower.Contains("editor")) continue;
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;
                int score = 0; string pn = prefab.name.ToLowerInvariant();
                foreach (var kw in keywords)
                {
                    string k = kw.ToLowerInvariant();
                    if (lower.Contains(k)) score += 12;
                    if (pn.Contains(k))    score += 18;
                }
                if (score == 0) continue;
                best.Add((score, prefab));
            }
            best.Sort((a, b) => b.score.CompareTo(a.score));
            return best.Count > 0 ? best[0].prefab : null;
        }

        private static GameObject FindRoot(Scene scene, string name)
        {
            foreach (var r in scene.GetRootGameObjects())
                if (r.name == name) return r;
            return null;
        }

        private static Transform FindDeep(Scene scene, string name)
        {
            foreach (var r in scene.GetRootGameObjects())
                foreach (var t in r.GetComponentsInChildren<Transform>(true))
                    if (t.name == name) return t;
            return null;
        }
    }
}
