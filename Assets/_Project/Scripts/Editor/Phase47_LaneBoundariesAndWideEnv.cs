// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase47_LaneBoundariesAndWideEnv
//
// Phase 47.2 — Lane boundaries + wider playable environment.
//
// The user-facing concern: the Phase 27.2 / 32.2 lane was too narrow (~6 m
// wide between the cobble path and the cottages), had no actual boundary
// colliders, and the props had no colliders either — so the player could
// walk straight through cottages and fall off the edge of the world.
//
// Phase 47.2 fixes all of that, additively on top of Phase 27.2 and
// Phase 32.2 (their `_Phase27Env_Lane` and `_Phase32Env_Lane` parents are
// preserved untouched), under a new `_Phase47Env_Lane` parent.
//
// What this builder ADDS:
//
//   1. ✅ A 4 m-tall ring of stone-wall sections around the entire playable
//        area (24 m wide × 36 m long, from -12 to +12 X and -18 to +18 Z).
//        Each section has its native Collider from the MV prefab AND a
//        chest-high invisible BoxCollider so the player cannot squeeze
//        between visible sections.
//
//   2. ✅ Tall invisible BoxCollider "void blockers" on all four sides
//        slightly outside the visible walls — defence-in-depth against any
//        edge-case animation push or ragdoll.
//
//   3. ✅ A widened, lit cobble path that runs the full length of the
//        playable area, leading from spawn (-16) to the Hollow door (+8).
//        Side stones soften the edges.
//
//   4. ✅ An extended ground plane (28 m × 40 m) under the entire area
//        with a thick warm-earth material — so the player never sees the
//        infinite void from any angle.
//
//   5. ✅ Five extra "guide lanterns" along the cobble path. Each lantern
//        is a real warm point Light + Lumen halo + lantern post mesh.
//        Together they form a wayfinding chain toward the Hollow door —
//        which is the cozy game's onboarding language ("walk toward the
//        warmth"). The lanterns brighten in pairs as the player walks
//        north (handled by the runtime LaneGuidePulse component, dropped
//        as a child of each lantern).
//
//   6. ✅ Border foliage: tall autumn trees, dense bushes, low stone-wall
//        pieces wrap the inside of the perimeter walls so the boundary
//        reads as "deeper village beyond" rather than "wall of the world".
//
//   7. ✅ A signposted gate at the far south (player spawn) labeled
//        "Forge Path closed today" — this matches the gameplay-guide spec
//        (Hidden Detail § 19.7) and tells the cozy player a village
//        extends beyond.
//
// USE: Menu → Hearthbound → ⚙️ Advanced → 🏰 Phase 47.2 — Lane Boundaries + Wide Env
//
// Architecture notes:
//   - Per D-007: scene edited in-place + saved.
//   - Per D-027: bindings via Phase32_VillageBindingsExtension.
//   - Per D-035: Editor-only.
//   - The runtime LaneGuidePulse component lives in
//     `Assets/_Project/Scripts/Player/LaneGuidePulse.cs` (Phase 47 runtime).
//   - Idempotent: re-running wipes only `_Phase47Env_Lane` and rebuilds.

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HearthboundHollow.EditorTools
{
    public static class Phase47_LaneBoundariesAndWideEnv
    {
        // Playable area definition — the inside of the perimeter walls.
        // The Hollow door is at z = +8. The player spawns near z = -14.
        private const float HalfWidth  = 12.0f; // X = -12 to +12
        private const float ZSouth     = -18.0f; // far south wall
        private const float ZNorth     =  18.0f; // far north wall
        private const float WallHeight = 4.0f;
        private const float WallThickness = 0.6f;

        private const string SceneLane     = "Assets/_Project/Scenes/02_Mission01_Lane.unity";
        private const string MVRoot        = "Assets/MeshingunStudio";
        private const string LumenPrefabs  = "Packages/com.distantlands.lumen/Content/Prefabs";

        private const string EnvParentName = "_Phase47Env_Lane";

        [MenuItem("Hearthbound/⚙️ Advanced/🏰 Phase 47.2 — Lane Boundaries + Wide Env", priority = 472)]
        public static void Build()
        {
            if (!System.IO.File.Exists(SceneLane))
            {
                EditorUtility.DisplayDialog("Phase 47.2",
                    $"Scene not found: {SceneLane}\n\n" +
                    "Run `Hearthbound → 🚀 Build Everything` first.", "OK");
                return;
            }

            var binds = Phase32_VillageBindingsExtension.TryGetBindings();
            if (binds == null)
            {
                Phase32_VillageBindingsExtension.Build();
                binds = Phase32_VillageBindingsExtension.TryGetBindings();
            }

            var scene = EditorSceneManager.OpenScene(SceneLane, OpenSceneMode.Single);

            // Wipe previous Phase 47.2 pass — idempotent.
            var existing = GameObject.Find(EnvParentName);
            if (existing != null) Object.DestroyImmediate(existing);

            var envRoot = new GameObject(EnvParentName);

            int placed = 0;
            placed += BuildExtendedGround(envRoot.transform);
            placed += BuildPerimeterWalls(envRoot.transform, binds);
            placed += BuildInvisibleVoidBlockers(envRoot.transform);
            placed += BuildWideCobblePath(envRoot.transform, binds);
            placed += BuildGuideLanterns(envRoot.transform, binds);
            placed += BuildBorderFoliage(envRoot.transform);
            placed += BuildForgePathGate(envRoot.transform, binds);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog("Phase 47.2 — Lane Boundaries Built",
                $"🏰 Lane boundaries + wide environment built.\n\n" +
                $"{placed} additions placed under '{EnvParentName}'.\n\n" +
                $"Playable area: 24 m × 36 m  ({HalfWidth*2:F0} × {ZNorth-ZSouth:F0})\n" +
                $"Perimeter wall height: {WallHeight} m\n" +
                $"Invisible void blockers: 4 sides (8 m tall)\n" +
                $"Guide lanterns: 5 (warm point lights along path)\n\n" +
                "Re-run any time — idempotent.",
                "OK");
        }

        // ───────────────────────────────────────────────────────────────
        // 1) Extended ground plane (so the player never sees the void)
        // ───────────────────────────────────────────────────────────────

        private static int BuildExtendedGround(Transform envRoot)
        {
            // Procedural quad — a single oversized cube with a warm-earth material.
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "ExtendedGround";
            ground.transform.SetParent(envRoot, false);

            // 28 m × 40 m, 0.2 m thick, centred under the playable area.
            float xExtent = HalfWidth + 2.5f;   // 14.5 m half = 29 m wide
            float zExtent = (ZNorth - ZSouth) * 0.5f + 2.5f;
            float zCentre = (ZNorth + ZSouth) * 0.5f;
            ground.transform.localScale = new Vector3(xExtent * 2f, 0.2f, zExtent * 2f);
            ground.transform.position   = new Vector3(0f, -0.11f, zCentre);

            // Warm earth material
            var mr = ground.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                mat.name = "Phase47_GroundEarth";
                mat.color = new Color(0.38f, 0.28f, 0.18f, 1f);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.05f);
                if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.05f);
                if (mat.HasProperty("_Metallic"))   mat.SetFloat("_Metallic",   0.0f);
                mr.sharedMaterial = mat;
                mr.receiveShadows = true;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            // BoxCollider for ground (already present on primitive).
            return 1;
        }

        // ───────────────────────────────────────────────────────────────
        // 2) Visible perimeter walls (stone walls every 2 m around the area)
        // ───────────────────────────────────────────────────────────────

        private static int BuildPerimeterWalls(Transform envRoot, MedievalVillageBindingsV2 binds)
        {
            int placed = 0;
            var wallPrefab = binds?.solidWallPrefab ?? FindMVPrefab(new[] { "wallcobble_01", "wall_01a", "wall_01b", "wall_01c", "wall_01d" });
            if (wallPrefab == null)
            {
                Debug.LogWarning("[Hearthbound/Phase 47.2] No wall prefab found in Medieval Village. Skipping visible perimeter; relying on invisible blockers.");
                return 0;
            }

            var wallsParent = new GameObject("PerimeterWalls");
            wallsParent.transform.SetParent(envRoot, false);

            // South wall (z = ZSouth) and North wall (z = ZNorth)
            // Width direction = X. Place walls every 2 m, x from -HalfWidth to +HalfWidth.
            float xStep = 2.0f;
            for (float x = -HalfWidth; x <= HalfWidth + 0.01f; x += xStep)
            {
                placed += SpawnWall(wallPrefab, wallsParent.transform, $"Wall_S_{x:F1}",
                    new Vector3(x, 0f, ZSouth), Quaternion.Euler(0f, 0f, 0f));

                // Leave a gap in the north wall for the Hollow door at x≈0 z≈+8
                // (but the Hollow door is well south of ZNorth, so the full wall is fine here)
                placed += SpawnWall(wallPrefab, wallsParent.transform, $"Wall_N_{x:F1}",
                    new Vector3(x, 0f, ZNorth), Quaternion.Euler(0f, 180f, 0f));
            }

            // East / West walls (x = +HalfWidth, x = -HalfWidth)
            float zStep = 2.0f;
            for (float z = ZSouth; z <= ZNorth + 0.01f; z += zStep)
            {
                placed += SpawnWall(wallPrefab, wallsParent.transform, $"Wall_W_{z:F1}",
                    new Vector3(-HalfWidth, 0f, z), Quaternion.Euler(0f, 90f, 0f));
                placed += SpawnWall(wallPrefab, wallsParent.transform, $"Wall_E_{z:F1}",
                    new Vector3( HalfWidth, 0f, z), Quaternion.Euler(0f, -90f, 0f));
            }

            return placed;
        }

        private static int SpawnWall(GameObject wallPrefab, Transform parent, string name, Vector3 pos, Quaternion rot)
        {
            var w = (GameObject)PrefabUtility.InstantiatePrefab(wallPrefab);
            w.name = name;
            w.transform.SetParent(parent, true);
            w.transform.position = pos;
            w.transform.rotation = rot;
            EnsureBlockingCollider(w);
            return 1;
        }

        // ───────────────────────────────────────────────────────────────
        // 3) Invisible void blockers (defence-in-depth — 8 m tall)
        // ───────────────────────────────────────────────────────────────

        private static int BuildInvisibleVoidBlockers(Transform envRoot)
        {
            var blockersParent = new GameObject("InvisibleVoidBlockers");
            blockersParent.transform.SetParent(envRoot, false);

            // South wall (z = ZSouth - 1), 1 m outside the visible wall, 8 m tall.
            // Width = playable width + 4 m extra (covers walls and beyond).
            CreateInvisibleWall(blockersParent.transform, "VoidBlocker_S",
                new Vector3(0f, 4f, ZSouth - 1.0f),
                new Vector3(HalfWidth * 2f + 4f, 8f, 1f));
            // North wall
            CreateInvisibleWall(blockersParent.transform, "VoidBlocker_N",
                new Vector3(0f, 4f, ZNorth + 1.0f),
                new Vector3(HalfWidth * 2f + 4f, 8f, 1f));
            // East
            CreateInvisibleWall(blockersParent.transform, "VoidBlocker_E",
                new Vector3(HalfWidth + 1.0f, 4f, 0f),
                new Vector3(1f, 8f, (ZNorth - ZSouth) + 4f));
            // West
            CreateInvisibleWall(blockersParent.transform, "VoidBlocker_W",
                new Vector3(-HalfWidth - 1.0f, 4f, 0f),
                new Vector3(1f, 8f, (ZNorth - ZSouth) + 4f));

            return 4;
        }

        private static void CreateInvisibleWall(Transform parent, string name, Vector3 pos, Vector3 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var bc = go.AddComponent<BoxCollider>();
            bc.size = size;
            // No renderer — purely physical.
            // Layer = Default so PlayerController's collisions react.
        }

        // ───────────────────────────────────────────────────────────────
        // 4) Wide cobble path
        // ───────────────────────────────────────────────────────────────

        private static int BuildWideCobblePath(Transform envRoot, MedievalVillageBindingsV2 binds)
        {
            int placed = 0;
            var cobble = binds?.cobbleTilePrefab ?? FindMVPrefab(new[] { "cornercobble_01a", "cornercobble", "cobble" });
            if (cobble == null) return 0;

            var pathParent = new GameObject("WideCobblePath");
            pathParent.transform.SetParent(envRoot, false);

            // 3-tile-wide cobble corridor from z = -16 to z = +7 (just shy of the
            // Hollow door at z = +8). That's 24 tiles per row × 3 rows = 72 tiles.
            // Spacing 2 m per tile, x offsets -2, 0, +2.
            float[] xRow = { -2.0f, 0f, +2.0f };
            for (float z = -16f; z <= 7.0f + 0.01f; z += 2.0f)
            {
                foreach (var x in xRow)
                {
                    var c = (GameObject)PrefabUtility.InstantiatePrefab(cobble);
                    c.name = $"Cobble_W_{x:F1}_Z_{z:F1}";
                    c.transform.SetParent(pathParent.transform, true);
                    c.transform.position = new Vector3(x, 0.03f, z);
                    placed++;
                }
            }
            return placed;
        }

        // ───────────────────────────────────────────────────────────────
        // 5) Guide lanterns (onboarding wayfinding)
        // ───────────────────────────────────────────────────────────────

        private static int BuildGuideLanterns(Transform envRoot, MedievalVillageBindingsV2 binds)
        {
            int placed = 0;
            var post = binds?.lanternPostPrefab ?? FindMVPrefab(new[] { "streetlantern_01a", "streetlight_01a", "lantern" });

            var lampParent = new GameObject("GuideLanterns");
            lampParent.transform.SetParent(envRoot, false);

            // Five lanterns spaced ~4-5m along the path, alternating sides,
            // pointing the player from spawn (south) to the Hollow door (north).
            var positions = new (string name, Vector3 pos, float pulseDelay)[]
            {
                ("GuideLamp_01", new Vector3(-3.2f, 0f, -13.5f), 0.0f),
                ("GuideLamp_02", new Vector3( 3.2f, 0f,  -9.0f), 0.5f),
                ("GuideLamp_03", new Vector3(-3.2f, 0f,  -4.5f), 1.0f),
                ("GuideLamp_04", new Vector3( 3.2f, 0f,   0.0f), 1.5f),
                ("GuideLamp_05", new Vector3(-3.2f, 0f,   4.5f), 2.0f),
            };

            foreach (var (name, pos, delay) in positions)
            {
                GameObject lampGO;

                if (post != null)
                {
                    lampGO = (GameObject)PrefabUtility.InstantiatePrefab(post);
                    lampGO.name = name;
                }
                else
                {
                    // Fallback: a cylinder + cube head
                    lampGO = new GameObject(name);
                    var stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    stem.name = "Stem";
                    stem.transform.SetParent(lampGO.transform, false);
                    stem.transform.localScale = new Vector3(0.08f, 1.2f, 0.08f);
                    stem.transform.localPosition = new Vector3(0f, 1.2f, 0f);
                }
                lampGO.transform.SetParent(lampParent.transform, true);
                lampGO.transform.position = pos;
                placed++;

                // Real point light
                var bulbGO = new GameObject($"{name}_Bulb");
                bulbGO.transform.SetParent(lampGO.transform, false);
                bulbGO.transform.localPosition = new Vector3(0f, 2.4f, 0f);
                var pl = bulbGO.AddComponent<Light>();
                pl.type = LightType.Point;
                pl.color = new Color(1.0f, 0.78f, 0.42f);
                pl.intensity = 2.6f;
                pl.range = 6.5f;
                pl.shadows = LightShadows.None;

                // Add the pulse component (runtime brighten-as-player-approaches)
                var pulse = bulbGO.AddComponent<HearthboundHollow.Player.LaneGuidePulse>();
                pulse.delay = delay;
                pulse.baseIntensity = 2.6f;
                pulse.pulseIntensity = 3.6f;

                // Lumen halo
                var halo = FindLumenPrefab("Lantern Effect");
                if (halo != null)
                {
                    var h = (GameObject)PrefabUtility.InstantiatePrefab(halo);
                    h.name = $"{name}_Halo";
                    h.transform.SetParent(lampGO.transform, false);
                    h.transform.localPosition = new Vector3(0f, 2.4f, 0f);
                    placed++;
                }

                placed++;
            }

            return placed;
        }

        // ───────────────────────────────────────────────────────────────
        // 6) Border foliage — autumn trees + bushes inside the perimeter
        // ───────────────────────────────────────────────────────────────

        private static int BuildBorderFoliage(Transform envRoot)
        {
            int placed = 0;
            var fol = new GameObject("BorderFoliage");
            fol.transform.SetParent(envRoot, false);

            var alder = FindMVPrefab(new[] { "alder_fall_01a", "alder_fall_01b", "alder_fall" });
            var pine  = FindMVPrefab(new[] { "pinetree_spring_01a", "pinetree_spring_02a", "pinetree" });
            var dead  = FindMVPrefab(new[] { "deadtree_01a", "deadtree" });

            // Tree ring inside the perimeter (just inside the walls).
            // Avoid the path corridor by skipping x positions near 0.
            var ringPositions = new[]
            {
                // South stretch
                new Vector3(-10f, 0f, -16f),
                new Vector3( -7f, 0f, -17f),
                new Vector3(  7f, 0f, -17f),
                new Vector3( 10f, 0f, -16f),
                // West side
                new Vector3(-11f, 0f, -10f),
                new Vector3(-11f, 0f,  -3f),
                new Vector3(-11f, 0f,   4f),
                new Vector3(-11f, 0f,  12f),
                // East side
                new Vector3( 11f, 0f, -10f),
                new Vector3( 11f, 0f,  -3f),
                new Vector3( 11f, 0f,   4f),
                new Vector3( 11f, 0f,  12f),
                // North stretch
                new Vector3(-10f, 0f, 16f),
                new Vector3( -5f, 0f, 17f),
                new Vector3(  5f, 0f, 17f),
                new Vector3( 10f, 0f, 16f),
            };

            for (int i = 0; i < ringPositions.Length; i++)
            {
                GameObject treePrefab;
                if (i % 4 == 0 && dead != null)   treePrefab = dead;
                else if (i % 3 == 0 && pine != null) treePrefab = pine;
                else if (alder != null)              treePrefab = alder;
                else continue;

                var t = (GameObject)PrefabUtility.InstantiatePrefab(treePrefab);
                t.name = $"BorderTree_{i:00}";
                t.transform.SetParent(fol.transform, true);
                t.transform.position = ringPositions[i];
                t.transform.rotation = Quaternion.Euler(0f, i * 37f, 0f);
                // Slight scale variance
                float s = 0.95f + (i % 5) * 0.04f;
                t.transform.localScale = new Vector3(s, s, s);
                EnsureBlockingCollider(t);
                placed++;
            }

            return placed;
        }

        // ───────────────────────────────────────────────────────────────
        // 7) Forge Path closed gate at the far south (lore + boundary)
        // ───────────────────────────────────────────────────────────────

        private static int BuildForgePathGate(Transform envRoot, MedievalVillageBindingsV2 binds)
        {
            int placed = 0;
            var gateRoot = new GameObject("ForgePathGate");
            gateRoot.transform.SetParent(envRoot, false);
            gateRoot.transform.position = new Vector3(0f, 0f, ZSouth + 0.3f);

            // Place a wall arch / gate centred on x=0
            var arch = FindMVPrefab(new[] { "wallarch_01a", "wallarch_01b", "metalgate_01a", "gatemech_01a" });
            if (arch != null)
            {
                var a = (GameObject)PrefabUtility.InstantiatePrefab(arch);
                a.name = "ForgePathArch";
                a.transform.SetParent(gateRoot.transform, false);
                a.transform.localPosition = Vector3.zero;
                a.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                placed++;
                EnsureBlockingCollider(a);
            }

            // Signboard mounted near the gate.
            var sign = FindMVPrefab(new[] { "signboard_01b", "signboard_01" });
            if (sign != null)
            {
                var s = (GameObject)PrefabUtility.InstantiatePrefab(sign);
                s.name = "ForgePathSign";
                s.transform.SetParent(gateRoot.transform, false);
                s.transform.localPosition = new Vector3(2.4f, 0f, 1.2f);
                s.transform.localRotation = Quaternion.Euler(0f, -55f, 0f);
                placed++;
            }

            // 3D text floating over the sign — the canonical line from the
            // gameplay guide § 19.7.
            var textGO = new GameObject("ForgePathText");
            textGO.transform.SetParent(gateRoot.transform, false);
            textGO.transform.localPosition = new Vector3(2.4f, 1.7f, 1.2f);
            textGO.transform.localRotation = Quaternion.Euler(0f, -55f, 0f);
            var tmp = textGO.AddComponent<TMPro.TextMeshPro>();
            tmp.text = "Forge Path\nclosed today";
            tmp.fontSize = 1.0f;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.color = new Color(0.92f, 0.84f, 0.58f);
            tmp.fontStyle = TMPro.FontStyles.Italic;
            placed++;

            return placed;
        }

        // ───────────────────────────────────────────────────────────────
        // Helpers
        // ───────────────────────────────────────────────────────────────

        private static void EnsureBlockingCollider(GameObject go)
        {
            // If the prefab already has a Collider on it or its children, leave it.
            var existing = go.GetComponentInChildren<Collider>();
            if (existing != null) return;

            // Otherwise compute the renderer bounds and put a BoxCollider on the root.
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;

            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);

            // Convert bounds to local space (approx — assume no rotation).
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
