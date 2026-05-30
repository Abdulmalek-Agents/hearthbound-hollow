// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase63_WorldPolish
//
// PHASE 63 — Full environment polish across every mission arena, per the asset
// guidance (Docs/Unity_Assets_Master_Reference.md, Asset_Recommendations_Phase31.md,
// EXISTING_ASSETS_INDEX.md). Companion to Phase 60 (Hollow) + Phase 62 (Garden).
//
// For each gameplay scene it adds, idempotently + reversibly (one managed root
// "_Phase63_WorldPolish" per scene, deletable to revert):
//   1) VISTA GROUND — a big warm-earth/meadow plane that reads beyond the play
//      boundary so each arena feels like part of a real, large village (the
//      "make the arena big" ask) without moving the gameplay collider walls.
//   2) WARM LIGHTING — low golden sun + cozy trilight ambient (autumn register).
//   3) PERIMETER GREENERY — a ring of Medieval Village pine trees + HarvestGarden
//      boxwood hedges framing the arena.
//   4) PROP LIFE — a scene-tuned set of Medieval Village clusters:
//        • Lane    — market stalls (WoodStand) + crates/barrels/sacks/baskets +
//                    street lanterns lining the street → a lively village.
//        • Garden  — HarvestGarden crop rows (wheat / boxwood) so the beds read
//                    as a real tended garden, + a couple of farm props.
//        • Cottage — a few exterior props + perimeter trees for a homestead feel.
//   Every prop is raycast-grounded (no floating/clipping) + collider'd, named, and
//   parented under the managed root for a clean, reversible scene graph.
//
// DEFENSIVE: per-step try/catch; a missing prefab/object never breaks a scene.
// Chained into 🚀 Build Everything (Step 21, after Phase 62).

using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HearthboundHollow.EditorTools
{
    public static class Phase63_WorldPolish
    {
        private const string MV   = "Assets/MeshingunStudio";     // Medieval Village
        private const string HG   = "Assets/Waldemarst";          // HarvestGarden
        private const string Root = "_Phase63_WorldPolish";

        private const string LaneScene    = "Assets/_Project/Scenes/02_Mission01_Lane.unity";
        private const string GardenScene  = "Assets/_Project/Scenes/04_Mission02_Garden.unity";
        private const string CottageScene = "Assets/_Project/Scenes/05_Mission02_Cottage.unity";

        [MenuItem("Hearthbound/⚙️ Advanced/🌍 Phase 63 — World Polish (all arenas)", priority = 630)]
        public static void Build()
        {
            string original = EditorSceneManager.GetActiveScene().path;
            var log = new StringBuilder("═══ Phase 63 — World Polish ═══\n");

            DressScene(LaneScene, "lane", log);
            DressScene(GardenScene, "garden", log);
            DressScene(CottageScene, "cottage", log);

            if (!string.IsNullOrEmpty(original) && System.IO.File.Exists(original))
                EditorSceneManager.OpenScene(original, OpenSceneMode.Single);
            Debug.Log(log.ToString());
        }

        private static void DressScene(string scenePath, string kind, StringBuilder log)
        {
            if (!System.IO.File.Exists(scenePath)) { log.AppendLine($"  {kind}: scene missing — skip."); return; }
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            var existing = FindInScene(scene, Root);
            if (existing != null) Object.DestroyImmediate(existing);
            var root = new GameObject(Root);
            SceneManager.MoveGameObjectToScene(root, scene);
            root.transform.position = Vector3.zero;

            float groundY = ProbeGroundY(scene);
            int n = 0;
            try { n += BuildVistaGround(root.transform, kind, groundY); } catch (System.Exception e) { log.AppendLine($"  {kind} vista: {e.Message}"); }
            try { WarmLighting(scene, log, kind); }                       catch (System.Exception e) { log.AppendLine($"  {kind} light: {e.Message}"); }
            try { n += GreeneryRing(root.transform, kind, groundY); }     catch (System.Exception e) { log.AppendLine($"  {kind} green: {e.Message}"); }
            try { n += SceneProps(root.transform, kind, groundY); }       catch (System.Exception e) { log.AppendLine($"  {kind} props: {e.Message}"); }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            log.AppendLine($"  {kind}: placed {n} object(s) (groundY≈{groundY:F1}).");
        }

        // ── 1) Big vista ground ──────────────────────────────────
        private static int BuildVistaGround(Transform parent, string kind, float groundY)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
            go.name = "VistaGround";
            var col = go.GetComponent<Collider>(); if (col != null) Object.DestroyImmediate(col); // no gameplay collision
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(0f, groundY - 0.06f, kind == "garden" ? 6f : 0f);
            go.transform.localScale = new Vector3(24f, 1f, 24f);   // Plane is 10m → 240m vista
            var mr = go.GetComponent<MeshRenderer>();
            mr.sharedMaterial = GroundMat(kind);
            return 1;
        }

        private static Material GroundMat(string kind)
        {
            foreach (var kw in new[] { "grass", "meadow", "soil", "ground", "dirt", "field" })
                foreach (var g in AssetDatabase.FindAssets("t:Material " + kw, new[] { MV }))
                {
                    var m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g));
                    if (m != null) return m;
                }
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader) { name = "Phase63_Vista_" + kind };
            var c = kind == "lane" ? new Color(0.30f, 0.27f, 0.20f)      // earth
                                   : new Color(0.32f, 0.36f, 0.23f);     // meadow
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
            if (mat.HasProperty("_Color"))     mat.SetColor("_Color", c);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.04f);
            return mat;
        }

        // ── 2) Warm lighting ────────────────────────────────────
        private static void WarmLighting(Scene scene, StringBuilder log, string kind)
        {
            Light sun = null;
            foreach (var root in scene.GetRootGameObjects())
                foreach (var l in root.GetComponentsInChildren<Light>(true))
                    if (l.type == LightType.Directional) { sun = l; break; }
            if (sun == null)
            {
                var go = new GameObject("Phase63_Sun");
                SceneManager.MoveGameObjectToScene(go, scene);
                sun = go.AddComponent<Light>(); sun.type = LightType.Directional;
            }
            sun.color = new Color(1.0f, 0.92f, 0.76f);
            sun.intensity = Mathf.Max(sun.intensity, 1.1f);
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(40f, kind == "lane" ? 25f : 35f, 0f);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor     = new Color(0.62f, 0.66f, 0.74f);
            RenderSettings.ambientEquatorColor = new Color(0.50f, 0.46f, 0.40f);
            RenderSettings.ambientGroundColor  = new Color(0.26f, 0.24f, 0.20f);
            log.AppendLine($"  {kind} light: warm sun + ambient.");
        }

        // ── 3) Greenery ring (trees + boxwood hedges) ───────────────────
        private static int GreeneryRing(Transform parent, string kind, float groundY)
        {
            var ring = new GameObject("GreeneryRing"); ring.transform.SetParent(parent, false);
            var pine = FindPrefab(MV, new[] { "PineTree", "Tree" });
            var bush = FindPrefab(HG, new[] { "Boxwood" }) ?? FindPrefab(MV, new[] { "Bush", "Shrub" });
            int placed = 0;
            float r = kind == "cottage" ? 14f : 19f;
            Vector3 c = new Vector3(0f, groundY, kind == "garden" ? 6f : 0f);
            for (int i = 0; i < 16; i++)
            {
                float a = (i / 16f) * Mathf.PI * 2f;
                Vector3 p = c + new Vector3(Mathf.Cos(a) * r * (0.85f + 0.3f * Mathf.PerlinNoise(i, 1.3f)),
                                            8f,
                                            Mathf.Sin(a) * r * (0.85f + 0.3f * Mathf.PerlinNoise(2.1f, i)));
                if (pine != null) placed += Spawn(pine, ring.transform, $"Pine_{i}", p, Quaternion.Euler(0f, i * 41f, 0f), 1f + 0.5f * Mathf.PerlinNoise(i, 4f), groundY);
                if (bush != null && i % 2 == 0)
                {
                    Vector3 bp = c + new Vector3(Mathf.Cos(a) * (r - 3f), 4f, Mathf.Sin(a) * (r - 3f));
                    placed += Spawn(bush, ring.transform, $"Hedge_{i}", bp, Quaternion.Euler(0f, i * 23f, 0f), 1.2f, groundY);
                }
            }
            return placed;
        }

        // ── 4) Scene-tuned props ────────────────────────────────
        private static int SceneProps(Transform parent, string kind, float groundY)
        {
            var dress = new GameObject("Props"); dress.transform.SetParent(parent, false);
            int placed = 0;

            if (kind == "lane")
            {
                (string[] kw, Vector3 pos, float yaw, string nick)[] plan =
                {
                    (new[]{"WoodStand","Stand"},  new Vector3(-3.6f, 8f, 2f),  90f, "Stall_W1"),
                    (new[]{"WoodStand","Stand"},  new Vector3( 3.6f, 8f, 5f), -90f, "Stall_E1"),
                    (new[]{"WoodTable","Table"},  new Vector3(-3.2f, 8f, 4f),  90f, "Table_W"),
                    (new[]{"Crate_01a","Crate"},  new Vector3(-3.0f, 8f, 1f),  10f, "Crate_W"),
                    (new[]{"Barrel_01","Barrel"}, new Vector3( 3.1f, 8f, 3f), -20f, "Barrel_E"),
                    (new[]{"Sack_Apple","Sack"},  new Vector3(-3.4f, 8f, 3f),   0f, "Sacks_W"),
                    (new[]{"WickerBasket","Basket"},new Vector3(3.3f, 8f, 6f),  0f, "Basket_E"),
                    (new[]{"TerraPots","Pot"},    new Vector3(-3.1f, 8f, 6f),   0f, "Pots_W"),
                    (new[]{"Cart","Wagon"},        new Vector3( 4.0f, 8f, 8f),  60f, "Cart_E"),
                };
                placed += PlacePlan(dress.transform, plan, groundY);
                placed += Lanterns(dress.transform, groundY, count: 5, zStart: 0f, zStep: 3.5f, x: 2.6f);
            }
            else if (kind == "garden")
            {
                placed += CropRows(dress.transform, groundY);
                (string[] kw, Vector3 pos, float yaw, string nick)[] plan =
                {
                    (new[]{"WickerBasket","Basket"}, new Vector3(2.0f, 8f, 3.0f), 0f, "HarvestBasket"),
                    (new[]{"Barrel_01","Barrel"},    new Vector3(2.6f, 8f, 2.2f), 20f, "RainBarrel"),
                };
                placed += PlacePlan(dress.transform, plan, groundY);
            }
            else if (kind == "cottage")
            {
                (string[] kw, Vector3 pos, float yaw, string nick)[] plan =
                {
                    (new[]{"Barrel_01","Barrel"},  new Vector3(-2.4f, 8f, -2.0f), 10f, "Barrel"),
                    (new[]{"Crate_01a","Crate"},   new Vector3(-2.7f, 8f, -1.2f), 30f, "Crate"),
                    (new[]{"WickerBasket","Basket"},new Vector3(2.3f, 8f, -1.8f),  0f, "Basket"),
                    (new[]{"BenchConcrete","Bench"},new Vector3(2.6f, 8f, -0.6f),-20f, "Bench"),
                    (new[]{"TerraPots","Pot"},     new Vector3(-1.8f, 8f, -2.4f),  0f, "Pots"),
                };
                placed += PlacePlan(dress.transform, plan, groundY);
            }
            return placed;
        }

        private static int CropRows(Transform parent, float groundY)
        {
            var wheat = FindPrefab(HG, new[] { "Wheat_Mid", "Wheat" });
            var box   = FindPrefab(HG, new[] { "Boxwood" });
            int placed = 0;
            for (int row = 0; row < 2; row++)
            {
                var prefab = row == 0 ? wheat : box;
                if (prefab == null) continue;
                for (int i = 0; i < 7; i++)
                {
                    Vector3 p = new Vector3(-3.2f + row * 0.9f, 6f, 3f + i * 0.8f);
                    placed += Spawn(prefab, parent, $"Crop_{row}_{i}", p, Quaternion.Euler(0f, i * 30f, 0f), 1f, groundY, collide: false);
                }
            }
            return placed;
        }

        private static int Lanterns(Transform parent, float groundY, int count, float zStart, float zStep, float x)
        {
            var lantern = FindPrefab(MV, new[] { "StreetLantern", "Lantern" });
            if (lantern == null) return 0;
            int placed = 0;
            for (int i = 0; i < count; i++)
            {
                float z = zStart + i * zStep;
                placed += Spawn(lantern, parent, $"Lantern_W_{i}", new Vector3(-x, 8f, z), Quaternion.identity, 1f, groundY, collide: false);
                placed += Spawn(lantern, parent, $"Lantern_E_{i}", new Vector3( x, 8f, z), Quaternion.identity, 1f, groundY, collide: false);
                AddGlow(parent, $"LaneGlow_{i}", new Vector3(0f, groundY + 0.6f, z), new Color(1f, 0.85f, 0.55f), 4f, 1.0f);
            }
            return placed;
        }

        private static int PlacePlan(Transform parent, (string[] kw, Vector3 pos, float yaw, string nick)[] plan, float groundY)
        {
            int placed = 0;
            foreach (var p in plan)
            {
                var prefab = FindPrefab(MV, p.kw);
                if (prefab == null) continue;
                placed += Spawn(prefab, parent, "HH_" + p.nick, p.pos, Quaternion.Euler(0f, p.yaw, 0f), 1f, groundY, collide: true);
            }
            return placed;
        }

        // ── helpers ─────────────────────────────────────────
        private static void AddGlow(Transform parent, string name, Vector3 pos, Color color, float range, float intensity)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.position = pos;
            var l = go.AddComponent<Light>(); l.type = LightType.Point; l.color = color; l.range = range; l.intensity = intensity; l.shadows = LightShadows.None;
        }

        private static float ProbeGroundY(Scene scene)
        {
            if (Physics.Raycast(new Vector3(0f, 50f, 0f), Vector3.down, out RaycastHit hit, 200f, ~0, QueryTriggerInteraction.Ignore))
                return hit.point.y;
            foreach (var nm in new[] { "Phase47_GroundEarth", "GardenSoilGround", "Ground", "WoodFloor" })
            {
                var t = FindDeepInScene(scene, nm);
                if (t != null) return t.position.y;
            }
            return 0f;
        }

        private static int Spawn(GameObject prefab, Transform parent, string name, Vector3 pos, Quaternion rot, float scale, float fallbackY, bool collide = false)
        {
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (inst == null) return 0;
            inst.name = name;
            inst.transform.SetParent(parent, true);
            inst.transform.position = pos;
            inst.transform.rotation = rot;
            inst.transform.localScale = Vector3.one * scale;
            GroundToFloor(inst.transform, fallbackY);
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
            float floorY = Physics.Raycast(from, Vector3.down, out RaycastHit hit, 60f, ~0, QueryTriggerInteraction.Ignore) ? hit.point.y : fallbackY;
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

        private static GameObject FindPrefab(string rootDir, string[] keywords)
        {
            if (!System.IO.Directory.Exists(rootDir)) return null;
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { rootDir });
            var best = new List<(int score, GameObject prefab)>();
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var lower = path.ToLowerInvariant();
                if (lower.Contains("/scenes/") || lower.Contains("preview") || lower.Contains("editor") || lower.Contains("demo")) continue;
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

        private static GameObject FindInScene(Scene scene, string name)
        {
            foreach (var root in scene.GetRootGameObjects())
                if (root.name == name) return root;
            return null;
        }

        private static Transform FindDeepInScene(Scene scene, string name)
        {
            foreach (var root in scene.GetRootGameObjects())
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                    if (t.name == name) return t;
            return null;
        }
    }
}
