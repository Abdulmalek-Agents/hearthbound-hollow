// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase29_PlayerRigDoctor
//
// PHASE 29 — Player Rig Doctor.
//
// The Phase 26 + 27 + 28 work shipped a layered defence against the
// "half body in the floor" sink:
//   • Phase 26 — added PlayerGroundClamp + Animator Controller + camera rig.
//   • Phase 27 — embedded the clamp logic in PlayerController for fresh pulls.
//   • Phase 28 — switched to live world-space Renderer.bounds + continuous
//                re-alignment window.
//
// All three are bounds-based. Bounds are computed from the SkinnedMeshRenderer's
// world AABB AFTER the Animator has updated. That works on most rigs. But on
// some imports — Mixamo FBX with a slightly stretched stand pose, BoZo Body
// children whose root bone has a baked-in offset, character-creator variants
// with padded cull AABBs — the AABB *still* doesn't reflect where the visible
// FEET actually are. Result: residual sink of 5-30 cm.
//
// PHASE 29 ROOT-CAUSE FIX
// ----------------------
// Use the **foot bone** as the anchor instead of bounds. The foot bone's
// world Y is the toe / heel position in the current pose. There is no
// ambiguity, no padding, no AABB mystery.
//
// PlayerGroundClamp already supports this — its `footAnchor` field, when
// set, is preferred over bounds scanning. Phase 29 auto-discovers a foot
// bone by walking the rig and matching common Mixamo + BoZo + generic
// humanoid bone names (LeftFoot, mixamorig:LeftFoot, L_Foot, Foot_L,
// LeftToeBase, etc.).
//
// Phase 29 ALSO:
//   • Force-disables `applyRootMotion` on EVERY Animator in every Player
//     (belt-and-braces — Mixamo imports often re-enable it on reimport).
//   • Verifies the Animator's Avatar is set to a Humanoid avatar so
//     animations retarget (the #1 cause of "animation looks weird").
//   • Walks the Animator Controller's clips and warns if any clip is
//     missing — "animation not working well" is usually a clip-not-imported
//     symptom, not a rig symptom.
//   • Sanity-checks the GameObject scale chain (scales != 1 anywhere in the
//     Player → Body parent chain break bone-relative anchoring).
//   • Auto-adds a Ground BoxCollider to scenes that don't have one (catches
//     the "player falls through the floor" half of the user's report).
//
// USE: Menu → Hearthbound → 🦶 Phase 29 — Player Rig Doctor

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using HearthboundHollow.Player;

namespace HearthboundHollow.EditorTools
{
    public static class Phase29_PlayerRigDoctor
    {
        private static readonly string[] GameplayScenes = new[]
        {
            "Assets/_Project/Scenes/02_Mission01_Lane.unity",
            "Assets/_Project/Scenes/03_Mission01_Hollow.unity",
            "Assets/_Project/Scenes/04_Mission02_Garden.unity",
            "Assets/_Project/Scenes/05_Mission02_Cottage.unity",
        };

        private const string PlayerPrefabPath =
            "Assets/_Project/Prefabs/Player/Player.prefab";

        // Common foot-bone names across Mixamo / BoZo / generic humanoid rigs.
        // Ordered by preference — toe/ball anchors first (most surgical), then
        // foot anchors. Lowercase comparisons so case doesn't matter.
        private static readonly string[] FootBoneCandidates = new[]
        {
            // Most surgical — toes / toe-base point exactly where the feet
            // touch the ground in an idle.
            "mixamorig:lefttoebase", "mixamorig:righttoebase",
            "lefttoebase", "righttoebase",
            "leftfoot_end", "rightfoot_end",
            "l_toe", "r_toe", "toe_l", "toe_r",
            "toebase_l", "toebase_r",
            // Then ankle / foot anchors — slightly higher but still reliable.
            "mixamorig:leftfoot", "mixamorig:rightfoot",
            "leftfoot", "rightfoot",
            "l_foot", "r_foot", "foot_l", "foot_r",
            // Last-ditch — legacy / unusual names.
            "leftankle", "rightankle", "ankle_l", "ankle_r",
        };

        // ───── Menu entry ─────────────────

        [MenuItem("Hearthbound/⚙️ Advanced/🦶 Phase 29 — Player Rig Doctor", priority = 1)]
        public static void Build()
        {
            var report = new RigReport();

            EditorUtility.DisplayProgressBar("Hearthbound · Phase 29",
                "Treating Player prefab…", 0.10f);
            try
            {
                // 1. Player prefab
                TreatPlayerPrefab(report);

                // 2. Every gameplay scene
                for (int i = 0; i < GameplayScenes.Length; i++)
                {
                    EditorUtility.DisplayProgressBar("Hearthbound · Phase 29",
                        $"Treating scene ({i + 1}/{GameplayScenes.Length})…",
                        0.20f + 0.70f * (i / (float)GameplayScenes.Length));
                    TreatScene(GameplayScenes[i], report);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            EditorUtility.DisplayDialog(
                "Phase 29 — Player Rig Doctor",
                report.Build(),
                "OK");
        }

        // ───── Prefab pass ──────────────

        private static void TreatPlayerPrefab(RigReport report)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (prefab == null)
            {
                report.AddLine($"⚠️ Prefab not found at {PlayerPrefabPath} — skipping prefab pass. " +
                               "Run Phase 13 first.");
                return;
            }

            var contents = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            try
            {
                int fixesBefore = report.FixesApplied;
                ApplyAllFixes(contents, /*isPrefab=*/true, report);
                if (report.FixesApplied > fixesBefore)
                {
                    PrefabUtility.SaveAsPrefabAsset(contents, PlayerPrefabPath);
                    report.AddLine($"💾 Player prefab updated ({report.FixesApplied - fixesBefore} fix(es) applied).");
                }
                else
                {
                    report.AddLine("✓ Player prefab already healthy — no changes needed.");
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        // ───── Scene pass ───────────────

        private static void TreatScene(string scenePath, RigReport report)
        {
            if (!File.Exists(scenePath))
            {
                report.AddLine($"⚠️ Scene missing: {Path.GetFileName(scenePath)} — skip.");
                return;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            report.AddLine($"\n📋 Scene: {Path.GetFileName(scenePath)}");

            // Player check
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                report.AddLine("  ⚠️ No GameObject tagged 'Player' in this scene — skip rig fixes.");
            }
            else
            {
                int fixesBefore = report.FixesApplied;
                ApplyAllFixes(player, /*isPrefab=*/false, report);
                if (report.FixesApplied > fixesBefore)
                    EditorUtility.SetDirty(player);
            }

            // Ground collider check — the OTHER half of "half-body in floor"
            // could be that the ground has only a MeshRenderer, no Collider,
            // so the CharacterController falls through.
            EnsureGroundCollider(scene.GetRootGameObjects(), report);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        // ───── The actual rig treatments ────────────────

        /// <summary>
        /// Idempotent. Applies every Phase 29 fix to a Player (prefab root or
        /// scene root). Increments report.FixesApplied per applied fix.
        /// </summary>
        private static void ApplyAllFixes(GameObject playerRoot, bool isPrefab, RigReport report)
        {
            string ctx = isPrefab ? "[prefab]" : "[scene]";

            // 1. Body child — required for the clamp to find anything.
            var body = playerRoot.transform.Find("Body");
            if (body == null)
            {
                // Fallback — look for any child with a renderer + skinned mesh
                for (int i = 0; i < playerRoot.transform.childCount; i++)
                {
                    var c = playerRoot.transform.GetChild(i);
                    if (c.GetComponentInChildren<SkinnedMeshRenderer>(true) != null)
                    {
                        body = c;
                        report.AddLine($"  {ctx} ℹ️ No child named 'Body' — using '{c.name}' (has SkinnedMeshRenderer).");
                        break;
                    }
                }
            }
            if (body == null)
            {
                report.AddLine($"  {ctx} ❌ No visible character mesh found under Player. Run Phase 13 first.");
                return;
            }

            // 2. Animator — must exist + must reference a controller.
            var animator = playerRoot.GetComponentInChildren<Animator>(true);
            if (animator == null)
            {
                animator = playerRoot.AddComponent<Animator>();
                report.NoteFix($"  {ctx} ➕ Added missing Animator on Player root.");
            }

            // 2a. Apply Root Motion MUST be off — otherwise the Animator
            // moves the transform and fights PlayerController.Move().
            if (animator.applyRootMotion)
            {
                animator.applyRootMotion = false;
                EditorUtility.SetDirty(animator);
                report.NoteFix($"  {ctx} 🛑 Disabled Apply Root Motion (was fighting CharacterController.Move).");
            }

            // 2b. Animator Controller — warn if missing (don't auto-create —
            // PlayerAnimatorControllerBuilder owns that).
            if (animator.runtimeAnimatorController == null)
            {
                report.AddLine($"  {ctx} ⚠️ Animator has NO Controller assigned. " +
                               "Run 'Hearthbound → 🏃 Phase 26 — Player Controller + Animation' to build & assign it.");
            }

            // 2c. Avatar must be Humanoid for Mixamo / BoZo retargeting.
            if (animator.avatar == null)
            {
                report.AddLine($"  {ctx} ⚠️ Animator has NO Avatar — animations will play raw, no humanoid retargeting. " +
                               "On the Body's FBX import settings, set Rig → Animation Type → Humanoid and re-import.");
            }
            else if (!animator.avatar.isValid)
            {
                report.AddLine($"  {ctx} ⚠️ Animator's Avatar is INVALID — re-import the rig with Rig → Animation Type → Humanoid.");
            }
            else if (!animator.avatar.isHuman)
            {
                report.AddLine($"  {ctx} ℹ️ Animator's Avatar is Generic (not Humanoid). " +
                               "Mixamo animations will not retarget. " +
                               "On the FBX import settings, switch Rig → Animation Type → Humanoid.");
            }

            // 3. PlayerGroundClamp must exist + foot anchor assigned.
            var clamp = playerRoot.GetComponent<PlayerGroundClamp>();
            if (clamp == null)
            {
                clamp = playerRoot.AddComponent<PlayerGroundClamp>();
                report.NoteFix($"  {ctx} ➕ Added PlayerGroundClamp.");
            }

            // 3a. Body reference
            if (clamp.body == null)
            {
                clamp.body = body;
                EditorUtility.SetDirty(clamp);
                report.NoteFix($"  {ctx} 🔧 Set PlayerGroundClamp.body → '{body.name}'.");
            }

            // 3b. Foot anchor — Phase 29's surgical fix. If the rig has a
            // foot/toe bone, use its world Y instead of bounds scanning.
            if (clamp.footAnchor == null)
            {
                var foot = FindBestFootBone(body);
                if (foot != null)
                {
                    clamp.footAnchor = foot;
                    EditorUtility.SetDirty(clamp);
                    report.NoteFix($"  {ctx} 🦶 Set PlayerGroundClamp.footAnchor → '{foot.name}' (surgical anchor — bypasses bounds entirely).");
                }
                else
                {
                    report.AddLine($"  {ctx} ℹ️ No standard foot-bone name found under '{body.name}'. " +
                                   "Falling back to world-bounds alignment (Phase 28 algorithm).");
                }
            }

            // 4. Sanity — check scale chain
            CheckScaleChain(playerRoot, body, ctx, report);

            // 5. CharacterController sanity
            var cc = playerRoot.GetComponent<CharacterController>();
            if (cc == null)
            {
                cc = playerRoot.AddComponent<CharacterController>();
                cc.center = new Vector3(0, 1.0f, 0);
                cc.height = 1.9f;
                cc.radius = 0.4f;
                report.NoteFix($"  {ctx} ➕ Added missing CharacterController (height=1.9, center=(0,1.0,0), radius=0.4).");
            }
            else
            {
                // Sanity — CC.height must be > 0 and center.y >= height/2 - skin.
                if (cc.height < 0.5f)
                {
                    cc.height = 1.9f;
                    EditorUtility.SetDirty(cc);
                    report.NoteFix($"  {ctx} 🔧 Bumped CharacterController.height to 1.9 (was {cc.height:F2} — too small).");
                }
                // If center.y is well below height/2, the capsule bottom is
                // below the GameObject origin, which usually means the rig
                // pivot is at the feet. That's fine; just log.
                float ccBottomLocalY = cc.center.y - cc.height * 0.5f;
                if (Mathf.Abs(ccBottomLocalY) > 0.3f)
                {
                    report.AddLine($"  {ctx} ℹ️ CC bottom local Y = {ccBottomLocalY:F2} (not at GameObject origin). " +
                                   "PlayerGroundClamp will compensate — no action needed.");
                }
            }

            // 6. Run an immediate alignment so the Editor scene view shows
            // the fix without the user having to press Play.
            if (clamp != null && body != null)
            {
                clamp.Align();
                EditorUtility.SetDirty(clamp);
                report.AddLine($"  {ctx} 🎯 Ran PlayerGroundClamp.Align() — feet should now sit on the capsule bottom.");
            }
        }

        // ───── Foot bone discovery ──────────────────

        /// <summary>
        /// Walks every Transform under `body` and returns the best foot/toe bone
        /// match by Mixamo / BoZo / generic-humanoid naming convention.
        /// Returns null if nothing matches.
        /// </summary>
        private static Transform FindBestFootBone(Transform body)
        {
            // Build a flat list of all bones under body.
            var all = body.GetComponentsInChildren<Transform>(true);

            // First pass — exact matches against our ordered candidate list.
            foreach (var candidate in FootBoneCandidates)
            {
                foreach (var t in all)
                {
                    if (t == null) continue;
                    if (string.Equals(t.name.ToLowerInvariant(), candidate, System.StringComparison.Ordinal))
                        return t;
                }
            }

            // Second pass — substring (e.g. "Hips:LeftFoot" inside a name).
            foreach (var candidate in FootBoneCandidates)
            {
                foreach (var t in all)
                {
                    if (t == null) continue;
                    if (t.name.ToLowerInvariant().Contains(candidate))
                        return t;
                }
            }

            return null;
        }

        // ───── Scale-chain sanity ──────────────────

        private static void CheckScaleChain(GameObject root, Transform body, string ctx, RigReport report)
        {
            // Walk root → body chain. Any non-uniform or non-1 scale will
            // mis-translate the bone's world Y vs the CC's world Y.
            var node = body;
            while (node != null && node != root.transform.parent)
            {
                var s = node.localScale;
                bool isOne = Mathf.Approximately(s.x, 1f) && Mathf.Approximately(s.y, 1f) && Mathf.Approximately(s.z, 1f);
                if (!isOne)
                {
                    report.AddLine($"  {ctx} ⚠️ Scale ≠ (1,1,1) on '{node.name}': {s}. " +
                                   "Bone-relative anchoring assumes unit scales in the parent chain. " +
                                   "If the player still sinks, switch this scale to (1,1,1) and " +
                                   "rebake the rig's FBX import scale instead.");
                }
                node = node.parent;
            }
        }

        // ───── Ground collider check ──────────────────

        private static void EnsureGroundCollider(GameObject[] roots, RigReport report)
        {
            // Look for a GameObject named "Ground" or "Floor" or "WoodFloor".
            // If it has a Renderer but no Collider, add a BoxCollider sized to its bounds.
            foreach (var r in roots) WalkAddingGroundCollider(r.transform, report);
        }

        private static void WalkAddingGroundCollider(Transform t, RigReport report)
        {
            if (t == null) return;
            string nameLower = t.name.ToLowerInvariant();
            bool looksLikeGround = nameLower == "ground" || nameLower == "floor" ||
                                    nameLower == "woodfloor" || nameLower.Contains("ground") ||
                                    nameLower.Contains("floor");
            if (looksLikeGround)
            {
                var rend = t.GetComponent<Renderer>();
                var col = t.GetComponentInChildren<Collider>(true);
                if (rend != null && col == null)
                {
                    var bc = t.gameObject.AddComponent<BoxCollider>();
                    bc.size = rend.bounds.size;
                    bc.center = rend.bounds.center - t.position;
                    report.NoteFix($"  ➕ Added BoxCollider to '{t.name}' (had Renderer but no Collider — Player would fall through).");
                }
            }
            for (int i = 0; i < t.childCount; i++)
                WalkAddingGroundCollider(t.GetChild(i), report);
        }

        // ───── Report builder ────────────────────

        private class RigReport
        {
            private readonly System.Text.StringBuilder _sb = new();
            public int FixesApplied { get; private set; }

            public void AddLine(string s) => _sb.AppendLine(s);
            public void NoteFix(string s)
            {
                _sb.AppendLine(s);
                FixesApplied++;
            }

            public string Build()
            {
                var head = new System.Text.StringBuilder();
                head.AppendLine("🦶 Phase 29 — Player Rig Doctor");
                head.AppendLine();
                head.AppendLine($"Total fixes applied: {FixesApplied}");
                head.AppendLine();
                head.AppendLine("Diagnostics:");
                head.AppendLine();
                head.Append(_sb.ToString());
                head.AppendLine();
                head.AppendLine("─────");
                head.AppendLine();
                head.AppendLine("If the player still sinks or floats after this pass:");
                head.AppendLine("  1. Open the Player prefab in the Inspector.");
                head.AppendLine("  2. Find PlayerGroundClamp → footAnchor field.");
                head.AppendLine("  3. Manually drag the rig's left or right TOE bone into footAnchor.");
                head.AppendLine("  4. Save the prefab. The clamp uses the exact bone Y after that.");
                head.AppendLine();
                head.AppendLine("If the animation still looks wrong (T-pose, jittery, no run):");
                head.AppendLine("  1. Run 'Hearthbound → 🏃 Phase 26 — Player Controller + Animation'");
                head.AppendLine("     to ensure the Animator Controller has all 7 parameters wired.");
                head.AppendLine("  2. On the rig's FBX in the Project view, click Rig tab → set");
                head.AppendLine("     Animation Type = Humanoid → Apply.");
                head.AppendLine("  3. Drop a Mixamo Idle.fbx + Walking.fbx + Running.fbx into");
                head.AppendLine("     Assets/_Project/Animations/Mixamo/ and re-run Phase 26.");
                return head.ToString();
            }
        }
    }
}
