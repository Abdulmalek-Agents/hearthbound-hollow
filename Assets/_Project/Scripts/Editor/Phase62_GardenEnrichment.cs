// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase62_GardenEnrichment
//
// PHASE 62.D — Mission 2 Garden: environment, lighting, medieval dressing + the
// "I don't know where to go" wayfinding fix.
//
// The 8-minute playtest video showed the Garden as a big, flat, over-saturated
// GREEN plane with a few bare trellises — and the player wandering it, unable to
// find the way to Gerrold's cottage (the reported "stuck at Mission 2"). This
// builder fixes all of that, idempotently + reversibly, following the proven
// Phase 60 enrichment protocol (a single managed root, prefabs scored from the
// Medieval Village pack, raycast-grounded, collider'd):
//
//   1) GROUND  — retexture the flat green `Ground`/`GardenSoilGround` with a warm
//      autumn material (a pack grass/soil texture if one exists, else a muted
//      olive-soil URP Lit colour) so it reads as a real meadow, not a green slab.
//   2) LIGHTING — a warm, low autumn sun + cozy ambient (RenderSettings) so the
//      scene is golden, not flat-bright.
//   3) DRESSING — pine-tree perimeter + composed prop clusters (market table,
//      barrels, crates, sacks, baskets, terra pots, a well) from the Medieval
//      Village pack, grounded + collider'd, under "_Phase62_GardenEnrich".
//   4) WAYFINDING — a row of glowing street-lanterns leading from the player's
//      spawn toward `Garden_Exit_Trigger`, plus a soft beacon at the exit, so the
//      route to the cottage is unmistakable. This is the un-stuck fix.
//
// FULLY DEFENSIVE: every step is wrapped so a missing prefab / object can never
// break the scene; the whole layer is one root you can delete to revert. Chained
// into 🚀 Build Everything (runs after the scene builders).

using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HearthboundHollow.EditorTools
{
    public static class Phase62_GardenEnrichment
    {
        private const string MVRoot       = "Assets/MeshingunStudio";
        private const string GardenScene  = "Assets/_Project/Scenes/04_Mission02_Garden.unity";
        private const string EnrichRoot   = "_Phase62_GardenEnrich";

        [MenuItem("Hearthbound/⚙️ Advanced/🌿 Phase 62 — Garden Enrichment + Wayfinding", priority = 620)]
        public static void Build()
        {
            if (!System.IO.File.Exists(GardenScene)) { Debug.LogWarning("[Phase62] Garden scene missing — skip."); return; }
            string original = EditorSceneManager.GetActiveScene().path;
            var log = new StringBuilder("═══ Phase 62 — Garden Enrichment ═══\n");

            var scene = EditorSceneManager.OpenScene(GardenScene, OpenSceneMode.Single);

            // Rebuild the managed root for idempotency.
            var existing = FindInScene(scene, EnrichRoot);
            if (existing != null) Object.DestroyImmediate(existing);
            var root = new GameObject(EnrichRoot);
            SceneManager.MoveGameObjectToScene(root, scene);
            root.transform.position = Vector3.zero;

            int n = 0;
            try { n += ImproveGround(scene, log); }     catch (System.Exception e) { log.AppendLine("  ground: " + e.Message); }
            try { ImproveLighting(scene, log); }         catch (System.Exception e) { log.AppendLine("  light:  " + e.Message); }
            try { n += ScatterTrees(scene, root.transform, log); } catch (System.Exception e) { log.AppendLine("  trees:  " + e.Message); }
            try { n += PlaceClusters(scene, root.transform, log); } catch (System.Exception e) { log.AppendLine("  props:  " + e.Message); }
            try { n += LayWayfinding(scene, root.transform, log); } catch (System.Exception e) { log.AppendLine("  way:    " + e.Message); }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            if (!string.IsNullOrEmpty(original) && System.IO.File.Exists(original))
                EditorSceneManager.OpenScene(original, OpenSceneMode.Single);

            log.AppendLine($"\nPlaced {n} object(s). Delete '{EnrichRoot}' to revert dressing.");
            Debug.Log(log.ToString());
        }

        // ── 1) Ground ─────────────────────────────────────
        private static int ImproveGround(Scene scene, StringBuilder log)
        {
            var mat = MakeGroundMaterial();
            int painted = 0;
            foreach (var go in new[] { "GardenSoilGround", "Ground" })
            {
                var t = FindDeepInScene(scene, go);
                if (t == null) continue;
                foreach (var mr in t.GetComponentsInChildren<MeshRenderer>(true))
                {
                    var mats = mr.sharedMaterials;
                    for (int i = 0; i < mats.Length; i++) mats[i] = mat;
                    mr.sharedMaterials = mats;
                    painted++;
                }
            }
            log.AppendLine($"  ground: repainted {painted} renderer(s) warm meadow.");
            return 0;
        }

        private static Material MakeGroundMaterial()
        {
            // Prefer a real grass/soil texture from the pack if present.
            foreach (var kw in new[] { "grass", "soil", "dirt", "ground", "field", "meadow" })
            {
                foreach (var g in AssetDatabase.FindAssets("t:Material " + kw, new[] { MVRoot }))
                {
                    var m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g));
                    if (m != null) return m;
                }
            }
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader) { name = "Phase62_Meadow" };
            var c = new Color(0.34f, 0.38f, 0.24f); // muted autumn olive-grass (not neon green)
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
            if (mat.HasProperty("_Color"))     mat.SetColor("_Color", c);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.05f);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.05f);
            return mat;
        }

        // ── 2) Lighting ──────────────────────────────────
        private static void ImproveLighting(Scene scene, StringBuilder log)
        {
            Light sun = null;
            foreach (var root in scene.GetRootGameObjects())
                foreach (var l in root.GetComponentsInChildren<Light>(true))
                    if (l.type == LightType.Directional) { sun = l; break; }
            if (sun == null)
            {
                var go = new GameObject("Phase62_Sun");
                SceneManager.MoveGameObjectToScene(go, scene);
                sun = go.AddComponent<Light>();
                sun.type = LightType.Directional;
            }
            sun.color = new Color(1.0f, 0.93f, 0.78f);     // warm gold
            sun.intensity = Mathf.Max(sun.intensity, 1.15f);
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(38f, 32f, 0f);  // low autumn angle

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor    = new Color(0.62f, 0.66f, 0.74f);
            RenderSettings.ambientEquatorColor= new Color(0.50f, 0.46f, 0.40f);
            RenderSettings.ambientGroundColor = new Color(0.28f, 0.26f, 0.22f);
            log.AppendLine("  light: warm autumn sun + trilight ambient set.");
        }

        // ── 3) Trees + prop clusters ────────────────────────────
        private static int ScatterTrees(Scene scene, Transform parent, StringBuilder log)
        {
            var trees = new GameObject("TreePerimeter"); trees.transform.SetParent(parent, false);
            var prefab = FindMVPrefab(new[] { "PineTree", "Tree" });
            if (prefab == null) { log.AppendLine("  trees: none in pack."); return 0; }

            // A ring of trees well outside the play area (perimeter framing).
            int placed = 0;
            float r = 16f;
            for (int i = 0; i < 14; i++)
            {
                float a = (i / 14f) * Mathf.PI * 2f;
                var pos = new Vector3(Mathf.Cos(a) * r * (0.9f + 0.2f * Mathf.PerlinNoise(i, 0.3f)),
                                      6f,
                                      6f + Mathf.Sin(a) * r * (0.9f + 0.2f * Mathf.PerlinNoise(0.7f, i)));
                placed += Spawn(prefab, trees.transform, $"Pine_{i}", pos,
                                Quaternion.Euler(0f, i * 47f, 0f), 1.0f + 0.4f * Mathf.PerlinNoise(i, 9f));
            }
            log.AppendLine($"  trees: {placed} pines around the perimeter.");
            return placed;
        }

        private static int PlaceClusters(Scene scene, Transform parent, StringBuilder log)
        {
            var dress = new GameObject("MarketDressing"); dress.transform.SetParent(parent, false);
            // A little market corner + scattered farm props, kept to the edges so the
            // play space stays open. Positions are conservative; grounded per-prop.
            (string[] kw, Vector3 pos, float yaw, float s, string nick)[] plan =
            {
                (new[]{"WoodTable","Table"},   new Vector3(-6.5f, 3f, 10.0f),  20f, 1f, "MarketTable"),
                (new[]{"Barrel_01","Barrel"},  new Vector3(-7.4f, 3f,  9.2f), -10f, 1f, "Barrel_A"),
                (new[]{"Barrel_01","Barrel"},  new Vector3(-7.7f, 3f, 10.6f),  40f, 1f, "Barrel_B"),
                (new[]{"Crate_01a","Crate"},   new Vector3(-5.6f, 3f, 10.8f),  15f, 1f, "Crate_A"),
                (new[]{"WickerBasket","Basket"},new Vector3(-6.0f, 3f,  9.4f), -30f, 1f, "Basket_A"),
                (new[]{"TerraPots","Pot"},     new Vector3(-5.2f, 3f,  9.8f),   0f, 1f, "Pots_A"),
                (new[]{"Grain_Sack","Sack"},   new Vector3(-6.8f, 3f, 11.2f),  25f, 1f, "Sacks_A"),
                (new[]{"well_01","Well"},       new Vector3( 7.0f, 3f,  4.0f),   0f, 1f, "Well"),
                (new[]{"Crate_01a","Crate"},   new Vector3( 6.2f, 3f,  3.0f),  30f, 1f, "Crate_B"),
                (new[]{"Barrel_01","Barrel"},  new Vector3( 7.6f, 3f,  3.2f), -20f, 1f, "Barrel_C"),
                (new[]{"BenchConcrete","Bench"},new Vector3( 5.0f, 3f, 11.0f),-15f, 1f, "Bench"),
            };
            int placed = 0;
            foreach (var p in plan)
            {
                var prefab = FindMVPrefab(p.kw);
                if (prefab == null) continue;
                placed += Spawn(prefab, dress.transform, "HH_" + p.nick, p.pos, Quaternion.Euler(0f, p.yaw, 0f), p.s, ground: true, collider: true);
            }
            log.AppendLine($"  props: {placed} market/farm props placed.");
            return placed;
        }

        // ── 4) Wayfinding to the cottage exit ────────────────────────
        private static int LayWayfinding(Scene scene, Transform parent, StringBuilder log)
        {
            var exit = FindDeepInScene(scene, "Garden_Exit_Trigger");
            Vector3 exitPos = exit != null ? exit.position : new Vector3(0f, 0f, 22f);

            Vector3 start = FindSpawn(scene);
            var way = new GameObject("CottageWayfinding"); way.transform.SetParent(parent, false);

            var lantern = FindMVPrefab(new[] { "StreetLantern", "Lantern" });
            int placed = 0;
            // Lay lanterns in pairs along the start→exit line so the path glows.
            Vector3 dir = exitPos - start; dir.y = 0f;
            float len = dir.magnitude;
            if (len > 1f)
            {
                Vector3 ndir = dir / len;
                Vector3 side = Vector3.Cross(Vector3.up, ndir);
                int steps = Mathf.Clamp(Mathf.RoundToInt(len / 4f), 2, 7);
                for (int i = 1; i <= steps; i++)
                {
                    float t = i / (float)(steps + 1);
                    Vector3 c = start + ndir * (len * t);
                    if (lantern != null)
                    {
                        placed += Spawn(lantern, way.transform, $"Lantern_L_{i}", c + side * 1.6f + Vector3.up * 3f, Quaternion.identity, 1f, ground: true, collider: false);
                        placed += Spawn(lantern, way.transform, $"Lantern_R_{i}", c - side * 1.6f + Vector3.up * 3f, Quaternion.identity, 1f, ground: true, collider: false);
                    }
                    placed += AddGlow(way.transform, $"PathGlow_{i}", c + Vector3.up * 0.4f, new Color(1f, 0.85f, 0.55f), 3.2f, 1.1f);
                }
            }

            // A clear, brighter beacon at the exit itself.
            placed += AddGlow(way.transform, "CottageBeacon", exitPos + Vector3.up * 1.4f, new Color(1f, 0.78f, 0.45f), 7f, 2.6f);

            log.AppendLine($"  way: {placed} wayfinding light(s) from spawn → exit ({(exit != null ? "found exit" : "default exit")}).");
            return placed;
        }

        private static Vector3 FindSpawn(Scene scene)
        {
            foreach (var nm in new[] { "PlayerStart", "SpawnPoint", "Spawn", "PlayerSpawn", "Player" })
            {
                var t = FindDeepInScene(scene, nm);
                if (t != null) return t.position;
            }
            return new Vector3(0f, 0f, -4f);
        }

        private static int AddGlow(Transform parent, string name, Vector3 pos, Color color, float range, float intensity)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = color; l.range = range; l.intensity = intensity; l.shadows = LightShadows.None;
            return 1;
        }

        // ── Shared helpers (mirrors Phase60) ───────────────────────
        private static int Spawn(GameObject prefab, Transform parent, string name, Vector3 pos, Quaternion rot, float scale,
                                 bool ground = true, bool collider = false)
        {
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (inst == null) return 0;
            inst.name = name;
            inst.transform.SetParent(parent, true);
            inst.transform.position = pos;
            inst.transform.rotation = rot;
            inst.transform.localScale = Vector3.one * scale;
            if (ground) GroundToFloor(inst.transform, fallbackY: 0f);
            if (collider) EnsureCollider(inst);
            return 1;
        }

        private static void GroundToFloor(Transform t, float fallbackY)
        {
            var rends = t.GetComponentsInChildren<Renderer>(true);
            if (rends.Length == 0) { var p0 = t.position; p0.y = fallbackY; t.position = p0; return; }
            Bounds b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
            Vector3 castFrom = new Vector3(b.center.x, b.max.y + 0.5f, b.center.z);
            float floorY = Physics.Raycast(castFrom, Vector3.down, out RaycastHit hit, 50f, ~0, QueryTriggerInteraction.Ignore)
                         ? hit.point.y : fallbackY;
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

        private static GameObject FindMVPrefab(string[] keywords)
        {
            if (!System.IO.Directory.Exists(MVRoot)) return null;
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { MVRoot });
            var best = new List<(int score, GameObject prefab)>();
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var lower = path.ToLowerInvariant();
                if (lower.Contains("/scenes/") || lower.Contains("preview") || lower.Contains("editor")) continue;
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
