// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase60_HollowDressingEnrichment
//
// PHASE 60 (D-077) — Environment enrichment + placement accuracy.
//
// User directive (2026-05-29):
//   "Enhance the environment by adding more assets from the available assets in
//    the project — scan them well — with strong placement accuracy."
//
// Two responsible, idempotent passes (the joint QA + Technical-Artist + 3D-Modeler
// placement protocol from STUDIO_LOG):
//
//   1) RED-QUAD RETIRE (safe, all gameplay scenes). Mirrors the Phase 55
//      "Fix Trigger-Zone Quads" pass but runs SILENTLY (no dialog) so it can live
//      inside 🚀 Build Everything: any MeshRenderer on a clearly-named trigger
//      zone that also carries a trigger Collider has its renderer disabled — this
//      kills the red placeholder floor quads the QA video flagged, with zero
//      gameplay impact (the trigger still fires).
//
//   2) BAKER'S-CORNER DRESSING (Hollow interior). Builds a small, deliberately
//      composed cluster of cozy props sourced from the Medieval Village pack
//      already in the project (barrel, grain sack, crate, wood-log pile, jar,
//      bread, cloth) next to the Workbench. EVERY prop is:
//        • parented under a single managed root  "_Phase60_HollowDressing"
//          (found-and-destroyed then rebuilt each run → fully idempotent),
//        • RAYCAST-GROUNDED to the floor below it (no floating / no clipping),
//        • given a Collider if its prefab shipped without one,
//        • named + organised in the hierarchy for a clean scene graph.
//      Because the layer is a single managed root, it is trivially reversible:
//      delete "_Phase60_HollowDressing" (or re-run) and the scene is unchanged.
//
// WHY ANCHOR-RELATIVE + RAYCAST rather than hard-coded coordinates: the dressing
// reads the live scene graph in-Editor, finds the Workbench, offsets a tight
// footprint to one side, and grounds each prop against the actual floor collider
// — so placement stays accurate even if the interior is re-laid-out later.
//
// NOTE TO QA: final visual sign-off happens in-Editor after 🚀 Build Everything
// (the studio cannot see the viewport from CI). Placement is conservative and
// reversible by design; nudge offsets in PlacementPlan() if the corner wants it.

using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HearthboundHollow.EditorTools
{
    public static class Phase60_HollowDressingEnrichment
    {
        private const string MVRoot         = "Assets/MeshingunStudio";
        private const string DressingRoot   = "_Phase60_HollowDressing";
        private const string HollowScene    = "Assets/_Project/Scenes/03_Mission01_Hollow.unity";

        private static readonly string[] GameplayScenes =
        {
            "Assets/_Project/Scenes/02_Mission01_Lane.unity",
            "Assets/_Project/Scenes/03_Mission01_Hollow.unity",
            "Assets/_Project/Scenes/04_Mission02_Garden.unity",
            "Assets/_Project/Scenes/05_Mission02_Cottage.unity",
        };

        private static readonly string[] ZoneKeywords =
            { "Zone", "Trigger", "Approach", "Ambiance", "Ambience", "Marker", "Volume" };

        // ── A composed baker's corner. Offsets are LOCAL to the cluster origin
        //    (origin = workbench position pushed to one clear side). Y is set by a
        //    raycast to the floor, so the Y in the offset is only the drop height.
        private struct Prop
        {
            public string[] keywords;   // scored against Medieval Village prefab names
            public Vector3 offset;      // local XZ placement; Y unused (grounded)
            public float yaw;           // degrees
            public float scale;         // uniform
            public string nick;         // hierarchy name
        }

        private static Prop[] PlacementPlan() => new[]
        {
            new Prop { keywords = new[]{ "Barrel_01" , "Barrel"     }, offset = new Vector3( 0.00f, 0f,  0.00f), yaw =  15f, scale = 1.0f, nick = "FlourBarrel"  },
            new Prop { keywords = new[]{ "Grain_Sack", "FoodSac"    }, offset = new Vector3( 0.62f, 0f,  0.10f), yaw = -25f, scale = 1.0f, nick = "GrainSack"    },
            new Prop { keywords = new[]{ "Crate_01a" , "Crate"      }, offset = new Vector3(-0.60f, 0f,  0.16f), yaw =  35f, scale = 1.0f, nick = "Crate"        },
            new Prop { keywords = new[]{ "Pile_WoodLog", "WoodLog"  }, offset = new Vector3(-0.70f, 0f, -0.55f), yaw =   0f, scale = 1.0f, nick = "FirewoodPile" },
            new Prop { keywords = new[]{ "Jar_01"    , "Jar", "Pot" }, offset = new Vector3( 0.40f, 0f, -0.42f), yaw =   0f, scale = 1.0f, nick = "ClayJar"      },
            new Prop { keywords = new[]{ "Bread_01"  , "Bread"      }, offset = new Vector3( 0.10f, 0f, -0.50f), yaw =  10f, scale = 1.0f, nick = "BreadLoaf"    },
            new Prop { keywords = new[]{ "ClothSet"  , "Cloth"      }, offset = new Vector3(-0.20f, 0f,  0.40f), yaw =  20f, scale = 1.0f, nick = "FoldedCloth"  },
        };

        public static void Build()
        {
            string original = EditorSceneManager.GetActiveScene().path;
            var log = new StringBuilder("═══ Phase 60 — Environment Enrichment ═══\n");

            int quadsHidden = 0;
            foreach (var scenePath in GameplayScenes)
            {
                if (!System.IO.File.Exists(scenePath)) continue;
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                int hid = HideTriggerZoneQuads(scene);
                quadsHidden += hid;

                int dressed = 0;
                if (scenePath == HollowScene)
                    dressed = DressHollow(scene, log);

                if (hid > 0 || dressed > 0)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
                log.AppendLine($"  {System.IO.Path.GetFileNameWithoutExtension(scenePath)}: hid {hid} quad(s)" +
                               (scenePath == HollowScene ? $", placed {dressed} prop(s)." : "."));
            }

            if (!string.IsNullOrEmpty(original) && System.IO.File.Exists(original))
                EditorSceneManager.OpenScene(original, OpenSceneMode.Single);

            log.Insert(log.Length, $"\nTotal trigger-zone quads hidden: {quadsHidden}");
            Debug.Log(log.ToString());
        }

        // ── 1) Silent red-quad retire ────────────────────────────────────
        private static int HideTriggerZoneQuads(Scene scene)
        {
            int fixedCount = 0;
            foreach (var root in scene.GetRootGameObjects())
            foreach (var mr in root.GetComponentsInChildren<MeshRenderer>(true))
            {
                var go = mr.gameObject;
                var col = go.GetComponent<Collider>();
                if (mr.enabled && IsZoneName(go.name) && col != null && col.isTrigger)
                {
                    mr.enabled = false;
                    EditorUtility.SetDirty(mr);
                    fixedCount++;
                }
            }
            return fixedCount;
        }

        // ── 2) Baker's-corner dressing on the Hollow interior ───────────────────
        private static int DressHollow(Scene scene, StringBuilder log)
        {
            // Rebuild the managed root for idempotency.
            GameObject existing = FindInScene(scene, DressingRoot);
            if (existing != null) Object.DestroyImmediate(existing);

            Transform anchor = FindWorkbench(scene);
            if (anchor == null)
            {
                log.AppendLine("  [Hollow] Workbench anchor not found — skipping dressing (safe).");
                return 0;
            }

            var rootGo = new GameObject(DressingRoot);
            SceneManager.MoveGameObjectToScene(rootGo, scene);
            rootGo.transform.position = Vector3.zero;

            // Cluster origin: push to the anchor's left+back so we don't crowd the
            // player's interaction face of the workbench. Grounded per-prop.
            Vector3 clusterOrigin = anchor.position
                                  - anchor.right   * 1.7f
                                  - anchor.forward * 0.2f;

            int placed = 0;
            foreach (var p in PlacementPlan())
            {
                GameObject prefab = FindMVPrefab(p.keywords);
                if (prefab == null) { log.AppendLine($"  [Hollow] no prefab for {p.nick} ({string.Join("/", p.keywords)})."); continue; }

                var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                if (inst == null) continue;
                inst.name = $"HH_{p.nick}";
                inst.transform.SetParent(rootGo.transform, true);

                // Place using the anchor's orientation so the corner faces sensibly.
                Vector3 world = clusterOrigin
                              + anchor.right   * p.offset.x
                              + anchor.forward * p.offset.z
                              + Vector3.up     * 2.0f;          // start high, drop down
                inst.transform.position = world;
                inst.transform.rotation = Quaternion.Euler(0f, anchor.eulerAngles.y + p.yaw, 0f);
                inst.transform.localScale = Vector3.one * p.scale;

                GroundToFloor(inst.transform, fallbackY: anchor.position.y);
                EnsureCollider(inst);
                placed++;
            }

            log.AppendLine($"  [Hollow] baker's corner placed near '{anchor.name}' ({placed} props, grounded + collider'd).");
            return placed;
        }

        // ── Grounding: drop the object so its renderer-bounds bottom rests on the
        //    first collider below it. Falls back to a known floor Y if nothing hit. ─
        private static void GroundToFloor(Transform t, float fallbackY)
        {
            var rends = t.GetComponentsInChildren<Renderer>(true);
            if (rends.Length == 0) { var p0 = t.position; p0.y = fallbackY; t.position = p0; return; }

            Bounds b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);

            Vector3 castFrom = new Vector3(b.center.x, b.max.y + 0.5f, b.center.z);
            float floorY;
            if (Physics.Raycast(castFrom, Vector3.down, out RaycastHit hit, 50f, ~0, QueryTriggerInteraction.Ignore))
                floorY = hit.point.y;
            else
                floorY = fallbackY;

            // Shift the whole object so the bounds bottom sits ~1 cm above the floor.
            float delta = (floorY + 0.01f) - b.min.y;
            t.position += new Vector3(0f, delta, 0f);
        }

        private static void EnsureCollider(GameObject go)
        {
            if (go.GetComponentInChildren<Collider>(true) != null) return;
            var rends = go.GetComponentsInChildren<Renderer>(true);
            if (rends.Length == 0) return;
            // A single box collider sized to the combined renderer bounds is plenty
            // for a static prop (cozy game — no physics simulation needed).
            Bounds b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
            var bc = go.AddComponent<BoxCollider>();
            bc.center = go.transform.InverseTransformPoint(b.center);
            bc.size = new Vector3(
                b.size.x / Mathf.Max(0.0001f, go.transform.lossyScale.x),
                b.size.y / Mathf.Max(0.0001f, go.transform.lossyScale.y),
                b.size.z / Mathf.Max(0.0001f, go.transform.lossyScale.z));
        }

        // ── helpers ─────────────────────────────────────────────
        private static bool IsZoneName(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            foreach (var k in ZoneKeywords)
                if (name.IndexOf(k, System.StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }

        private static Transform FindWorkbench(Scene scene)
        {
            // Prefer an object named exactly "Workbench"; never the approach zone
            // or the rug. Fall back to the first name that contains "Workbench".
            Transform contains = null;
            foreach (var root in scene.GetRootGameObjects())
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                string n = t.name;
                if (n == "Workbench") return t;
                if (contains == null && n.IndexOf("Workbench", System.StringComparison.OrdinalIgnoreCase) >= 0
                    && n.IndexOf("Zone", System.StringComparison.OrdinalIgnoreCase) < 0
                    && n.IndexOf("Rug", System.StringComparison.OrdinalIgnoreCase) < 0)
                    contains = t;
            }
            return contains;
        }

        private static GameObject FindInScene(Scene scene, string name)
        {
            foreach (var root in scene.GetRootGameObjects())
                if (root.name == name) return root;
            return null;
        }

        // Name-scored Medieval Village prefab discovery (mirrors Phase32_HollowInteriorV2).
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
                int score = 0;
                string pn = prefab.name.ToLowerInvariant();
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
    }
}
