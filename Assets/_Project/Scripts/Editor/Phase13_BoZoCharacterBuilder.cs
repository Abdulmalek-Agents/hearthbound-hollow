// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase13_BoZoCharacterBuilder
//
// Phase 13 — BoZo Character Integration.
//
// Replaces the Phase 12 primitive placeholders (capsule = player,
// cylinder = Doris, etc.) with real BoZo modular character prefabs.
//
// USE: Menu → Hearthbound → Phase 13 — Build BoZo Character Prefabs
//
// Auto-detects the BoZo base character prefab inside
// Assets/BoZo_StylizedModularCharacters/, then creates wrapper prefabs:
//
//   Assets/_Project/Prefabs/Player/Player.prefab
//   Assets/_Project/Prefabs/NPCs/Doris.prefab
//   Assets/_Project/Prefabs/NPCs/Gerrold.prefab
//   Assets/_Project/Prefabs/NPCs/SilentLaneVillager.prefab
//
// Each wrapper has:
//   * A root GameObject with tag/components/colliders set appropriately
//   * A child "Body" that is a nested instance of the BoZo character prefab
//
// Why a wrapper (not a direct duplicate / unpacked clone)? Three reasons:
//   1. Nested prefab instance preserves all BoZo internals — its rig,
//      its animator, its outfit layers — without us having to understand
//      or maintain the BoZo internals.
//   2. If BoZo ever updates, the wrapper inherits the upgrade automatically.
//   3. The wrapper is the only thing the rest of the studio code touches,
//      so character art changes never break gameplay refs.
//
// After this phase, HearthboundOneClickSetup automatically uses these
// prefabs in place of its primitive fallbacks (see SpawnCharacter helper).
//
// ── 2026-05-25 update ────────────────────────────────────────────────────────────
// The Player wrapper now ALSO carries a PlayerGroundClamp component +
// CharacterController defaults aligned so the BoZo mesh feet end up at
// the capsule bottom on Start. This is the build-time defensive layer
// for the "half body in floor" fix. The runtime auto-correct in
// PlayerGroundClamp will recover even if this prefab is overridden, but
// shipping the prefab with sensible defaults avoids one frame of pose
// flicker when entering a scene.

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using HearthboundHollow.Player;

namespace HearthboundHollow.EditorTools
{
    public static class Phase13_BoZoCharacterBuilder
    {
        private const string BoZoRoot = "Assets/BoZo_StylizedModularCharacters";
        public const string PlayerPrefabPath = "Assets/_Project/Prefabs/Player/Player.prefab";
        public const string DorisPrefabPath = "Assets/_Project/Prefabs/NPCs/Doris.prefab";
        public const string GerroldPrefabPath = "Assets/_Project/Prefabs/NPCs/Gerrold.prefab";
        public const string SilentLaneVillagerPrefabPath = "Assets/_Project/Prefabs/NPCs/SilentLaneVillager.prefab";

        // ─── Menu ──────────────────────────────────────────────────

        [MenuItem("Hearthbound/⚙️ Advanced/Phase 13 — Build BoZo Character Prefabs", priority = 200)]
        public static void Build()
        {
            if (!AssetDatabase.IsValidFolder(BoZoRoot))
            {
                EditorUtility.DisplayDialog(
                    "Phase 13 — BoZo not found",
                    $"Could not find {BoZoRoot}/.\n\n" +
                    "Please import the 'BoZo Stylized Modular Characters' asset pack from the Asset Store, " +
                    "then re-run this menu item.",
                    "OK");
                return;
            }

            var bozoBase = FindBoZoCharacterPrefab();
            if (bozoBase == null)
            {
                EditorUtility.DisplayDialog(
                    "Phase 13 — Auto-detect failed",
                    "Found the BoZo folder but couldn't auto-detect a base character prefab.\n\n" +
                    "BoZo character prefabs vary in location depending on version. After this dialog, " +
                    "the menu will:\n" +
                    "  • Open a file picker so you can manually point to the desired character prefab.\n" +
                    "  • Once selected, all 4 wrappers (Player, Doris, Gerrold, SilentLane) will be built from it.",
                    "Continue");
                bozoBase = ManualSelectBoZoPrefab();
                if (bozoBase == null)
                {
                    Debug.LogWarning("[Hearthbound/Phase 13] Aborted — no BoZo prefab selected.");
                    return;
                }
            }

            EnsureFolder("Assets/_Project/Prefabs");
            EnsureFolder("Assets/_Project/Prefabs/Player");
            EnsureFolder("Assets/_Project/Prefabs/NPCs");

            BuildPlayer(bozoBase);
            BuildDoris(bozoBase);
            BuildGerrold(bozoBase);
            BuildSilentLane(bozoBase);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[Hearthbound/Phase 13] Built 4 character prefabs from BoZo base: " +
                      $"{AssetDatabase.GetAssetPath(bozoBase)}");

            EditorUtility.DisplayDialog(
                "Phase 13 — Done",
                "Built 4 BoZo wrapper prefabs:\n" +
                "  • Assets/_Project/Prefabs/Player/Player.prefab\n" +
                "  • Assets/_Project/Prefabs/NPCs/Doris.prefab\n" +
                "  • Assets/_Project/Prefabs/NPCs/Gerrold.prefab\n" +
                "  • Assets/_Project/Prefabs/NPCs/SilentLaneVillager.prefab\n\n" +
                "Re-run 'Hearthbound → Build Playable Mission 1 (One Click)' — the scene builder will pick up these " +
                "prefabs automatically and use them in place of the primitive capsule/cylinder.",
                "OK");
        }

        // ─── BoZo prefab detection ───────────────────────────────────

        private static GameObject FindBoZoCharacterPrefab()
        {
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { BoZoRoot });
            var candidates = new List<(string path, GameObject prefab, int score)>();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);

                // Skip non-character utilities
                if (path.Contains("CharacterCreator")) continue;
                if (path.Contains("BMAC_")) continue;
                if (path.Contains("/Decal_")) continue;
                if (path.Contains("/Common/")) continue;
                if (path.Contains("IconCapture")) continue;
                if (path.Contains("/Outfits/")) continue;        // outfit-only items
                if (path.Contains("/OutfitTypes/")) continue;
                if (path.Contains("/CustomCharacters/_")) continue;

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                var smr = prefab.GetComponentInChildren<SkinnedMeshRenderer>(true);
                if (smr == null) continue;        // not a character

                int score = 0;
                if (prefab.GetComponentInChildren<Animator>(true) != null) score += 20;
                score += Mathf.Min(prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length, 10);
                if (path.Contains("/Demo/")) score += 8;
                if (path.Contains("/CustomCharacters/")) score += 10;
                if (path.Contains("/Prefabs/")) score += 3;

                candidates.Add((path, prefab, score));
            }

            candidates.Sort((a, b) => b.score.CompareTo(a.score));

            // Log the top 3 so the user can see the heuristic at work
            for (int i = 0; i < Mathf.Min(3, candidates.Count); i++)
            {
                Debug.Log($"[Hearthbound/Phase 13] BoZo candidate #{i + 1} (score {candidates[i].score}): {candidates[i].path}");
            }

            return candidates.Count > 0 ? candidates[0].prefab : null;
        }

        private static GameObject ManualSelectBoZoPrefab()
        {
            var pickedPath = EditorUtility.OpenFilePanel(
                "Select BoZo character prefab",
                BoZoRoot, "prefab");
            if (string.IsNullOrEmpty(pickedPath)) return null;

            // Translate absolute → asset-relative path
            var dataPathRoot = Application.dataPath;
            if (!pickedPath.StartsWith(dataPathRoot))
            {
                EditorUtility.DisplayDialog("Phase 13", "Please pick a prefab inside this project's Assets/ folder.", "OK");
                return null;
            }
            var relative = "Assets" + pickedPath.Substring(dataPathRoot.Length);
            return AssetDatabase.LoadAssetAtPath<GameObject>(relative);
        }

        // ─── Per-variant builders ────────────────────────────────────

        private static void BuildPlayer(GameObject source)
        {
            BuildVariant(source, PlayerPrefabPath, "Player", root =>
            {
                root.tag = "Player";

                // CharacterController defaults sized for a BoZo chibi.
                // center.y = height/2 (= 0.95) so the capsule sits with its
                // bottom at the GameObject's local Y=0 — i.e. the same place
                // the BoZo mesh is anchored. PlayerGroundClamp (below) then
                // shifts the Body child if the mesh origin doesn't actually
                // sit at Y=0, so the visible feet always land on the floor.
                var cc = root.AddComponent<CharacterController>();
                cc.center = new Vector3(0f, 0.95f, 0f);   // bottom at local Y=0
                cc.height = 1.9f;
                cc.radius = 0.32f;
                cc.skinWidth = 0.08f;
                cc.stepOffset = 0.3f;
                cc.slopeLimit = 45f;

                root.AddComponent<PlayerController>();

                // PlayerGroundClamp — runtime fix for the "half body in
                // floor" sink. Aligns the Body mesh feet to the CC capsule
                // bottom on Start. Auto-resolves its `body` field to the
                // "Body" child created by BuildVariant.
                var clamp = root.AddComponent<PlayerGroundClamp>();
                var bodyT = root.transform.Find("Body");
                if (bodyT != null) clamp.body = bodyT;
            });
        }

        private static void BuildDoris(GameObject source)
        {
            BuildVariant(source, DorisPrefabPath, "Doris", root =>
            {
                // Greeting trigger zone
                var trigger = new GameObject("Doris_Greeting_Zone");
                trigger.transform.SetParent(root.transform, false);
                var sphere = trigger.AddComponent<SphereCollider>();
                sphere.radius = 1.6f;
                sphere.isTrigger = true;

                // Doris's tone — warm cleric-amber.
                TintCharacterMaterials(root, new Color(0.96f, 0.82f, 0.62f), instanceMaterial: true);
            });
        }

        private static void BuildGerrold(GameObject source)
        {
            BuildVariant(source, GerroldPrefabPath, "Gerrold", root =>
            {
                // Cottage proximity trigger (used in Mission 2)
                var trigger = new GameObject("Gerrold_Cottage_Zone");
                trigger.transform.SetParent(root.transform, false);
                var sphere = trigger.AddComponent<SphereCollider>();
                sphere.radius = 1.8f;
                sphere.isTrigger = true;

                // Gerrold's tone — bardic dusk-blue / grief.
                TintCharacterMaterials(root, new Color(0.46f, 0.50f, 0.62f), instanceMaterial: true);
            });
        }

        private static void BuildSilentLane(GameObject source)
        {
            BuildVariant(source, SilentLaneVillagerPrefabPath, "SilentLaneVillager", root =>
            {
                // Neutral umber.
                TintCharacterMaterials(root, new Color(0.72f, 0.62f, 0.52f), instanceMaterial: true);
            });
        }

        // ─── Variant builder helper ───────────────────────────────────

        private static void BuildVariant(GameObject source, string outputPath, string variantName,
                                         System.Action<GameObject> customize)
        {
            // Create a fresh empty root and nest the BoZo source as a child.
            var root = new GameObject(variantName);

            var body = (GameObject)PrefabUtility.InstantiatePrefab(source, root.transform);
            body.name = "Body";
            body.transform.localPosition = Vector3.zero;
            body.transform.localRotation = Quaternion.identity;

            customize?.Invoke(root);

            // Save as new prefab, overwriting any prior version.
            PrefabUtility.SaveAsPrefabAsset(root, outputPath);
            Object.DestroyImmediate(root);
            Debug.Log($"[Hearthbound/Phase 13] (created) {outputPath}");
        }

        // ─── Material tinting (mobile-friendly via MaterialPropertyBlock) ─

        private static void TintCharacterMaterials(GameObject root, Color tint, bool instanceMaterial)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return;

            if (!instanceMaterial)
            {
                // PropertyBlock approach — doesn't create new materials, but
                // at edit time on a prefab the block isn't persisted.
                var mpb = new MaterialPropertyBlock();
                foreach (var r in renderers)
                {
                    r.GetPropertyBlock(mpb);
                    mpb.SetColor(Shader.PropertyToID("_BaseColor"), tint);
                    mpb.SetColor(Shader.PropertyToID("_Color"), tint);
                    r.SetPropertyBlock(mpb);
                }
                return;
            }

            // For prefab persistence, create an instance material per renderer.
            foreach (var r in renderers)
            {
                if (r.sharedMaterial == null) continue;
                var instMat = new Material(r.sharedMaterial);
                instMat.name = r.sharedMaterial.name + "_HhTinted";
                instMat.SetColor(Shader.PropertyToID("_BaseColor"), tint);
                instMat.SetColor(Shader.PropertyToID("_Color"), tint);
                r.sharedMaterial = instMat;
            }
        }

        // ─── Folder helpers ─────────────────────────────────────────────

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            var leaf = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        // ─── Public lookups (used by HearthboundOneClickSetup) ─────

        public static GameObject TryGetPlayerPrefab() =>
            AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);

        public static GameObject TryGetDorisPrefab() =>
            AssetDatabase.LoadAssetAtPath<GameObject>(DorisPrefabPath);

        public static GameObject TryGetGerroldPrefab() =>
            AssetDatabase.LoadAssetAtPath<GameObject>(GerroldPrefabPath);

        public static GameObject TryGetSilentLanePrefab() =>
            AssetDatabase.LoadAssetAtPath<GameObject>(SilentLaneVillagerPrefabPath);
    }
}
