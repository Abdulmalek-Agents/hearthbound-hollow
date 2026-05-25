// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / PlayerAnimatorControllerBuilder
//
// Builds the player AnimatorController procedurally:
//
//   Hearthbound_Player.controller    (Assets/_Project/Animations/)
//   ├── Parameters
//   │     Speed       (float, default 0)   — 0 idle, 1 walk, 2 run
//   │     MoveX       (float, default 0)   — strafe blend (-1..1)
//   │     MoveY       (float, default 0)   — fwd/back blend (-1..1)
//   │     VelocityY   (float, default 0)   — vertical velocity for jump blend
//   │     IsGrounded  (bool,  default 1)
//   │     IsSprinting (bool,  default 0)
//   │     Jump        (trigger)
//   └── Base Layer
//         ├── Locomotion  (1D Blend Tree on Speed)
//         │     ├── Idle  threshold 0   ← BoZo BMAC_M_Idle or Mixamo_Idle
//         │     ├── Walk  threshold 1   ← BoZo BMAC_M_Walk or Mixamo_Walking
//         │     └── Run   threshold 2   ← Mixamo_Running or fallback to Walk
//         ├── Jump        (transitions in on Jump trigger; exits when IsGrounded)
//         └── Fall        (entered when !IsGrounded && VelocityY < -0.5)
//
// AUTO-DETECTION
//   The builder scans the project's animation library and picks the best
//   available clip per role. Detection order:
//     1. Mixamo_* clip (anything tagged with humanoid + matching name token)
//     2. BoZo BMAC_M_Idle/BMAC_M_Walk (always present after Phase 13)
//     3. The first humanoid AnimationClip we can find with a matching name
//     4. Null (the blend tree still works — Unity just won't move bones for
//        that slot. The state remains valid for retargeting later).
//
// USAGE
//   • Menu: `Hearthbound → Phase 26 — Build Player Animator Controller`
//   • Programmatic: PlayerAnimatorControllerBuilder.BuildOrUpdate()
//
// IDEMPOTENT — re-running rebuilds the asset from scratch but preserves the
// expected path so all references stay valid.

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace HearthboundHollow.EditorTools
{
    public static class PlayerAnimatorControllerBuilder
    {
        public const string AnimationsDir = "Assets/_Project/Animations";
        public const string ControllerPath = AnimationsDir + "/Hearthbound_Player.controller";

        // The names we look for, in priority order, per locomotion role.
        // Mixamo's naming is "Mixamo_Idle", "Idle", "M_Idle", "Female_Idle", etc.
        // BoZo names are "BMAC_M_Idle" / "BMAC_F_Idle" / "BMAC_M_Walk" / "BMAC_F_Walk".
        private static readonly string[] IdleKeywords  = { "idle", "stand", "breathing_idle" };
        private static readonly string[] WalkKeywords  = { "walking", "walk_in_place", "walk" };
        private static readonly string[] RunKeywords   = { "running", "run_in_place", "jog", "run" };
        private static readonly string[] JumpKeywords  = { "jump_in_place", "jump", "leap" };
        private static readonly string[] FallKeywords  = { "fall", "falling", "in_air" };
        private static readonly string[] LandKeywords  = { "landing", "land_soft", "land" };

        [MenuItem("Hearthbound/Phase 26 — Build Player Animator Controller", priority = 209)]
        public static void BuildOrUpdate()
        {
            EnsureFolder(AnimationsDir);

            // Pick clips.
            var clipIdle = FindBestClip(IdleKeywords);
            var clipWalk = FindBestClip(WalkKeywords);
            var clipRun  = FindBestClip(RunKeywords);
            var clipJump = FindBestClip(JumpKeywords);
            var clipFall = FindBestClip(FallKeywords);
            var clipLand = FindBestClip(LandKeywords);

            // Build / overwrite the controller.
            var controller = CreateOrReplaceController(ControllerPath);

            // Parameters.
            controller.AddParameter("Speed",        AnimatorControllerParameterType.Float);
            controller.AddParameter("MoveX",        AnimatorControllerParameterType.Float);
            controller.AddParameter("MoveY",        AnimatorControllerParameterType.Float);
            controller.AddParameter("VelocityY",    AnimatorControllerParameterType.Float);
            controller.AddParameter("IsGrounded",   AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsSprinting",  AnimatorControllerParameterType.Bool);
            controller.AddParameter("Jump",         AnimatorControllerParameterType.Trigger);

            // Default IsGrounded = true so the rig stays in Locomotion at boot.
            controller.parameters[4] = new AnimatorControllerParameter
            {
                name = "IsGrounded", type = AnimatorControllerParameterType.Bool, defaultBool = true,
            };

            var layer = controller.layers[0];
            var sm = layer.stateMachine;

            // ── Locomotion blend tree ──
            var locomotion = sm.AddState("Locomotion", new Vector3(260, 0, 0));
            var blendTree = new BlendTree
            {
                name = "Locomotion_Blend",
                blendType = BlendTreeType.Simple1D,
                blendParameter = "Speed",
                hideFlags = HideFlags.HideInHierarchy,
            };
            AssetDatabase.AddObjectToAsset(blendTree, controller);

            blendTree.AddChild(clipIdle, 0f);
            blendTree.AddChild(clipWalk != null ? clipWalk : clipIdle, 1f);
            blendTree.AddChild(clipRun  != null ? clipRun  : (clipWalk != null ? clipWalk : clipIdle), 2f);

            locomotion.motion = blendTree;
            sm.defaultState = locomotion;

            // ── Jump state ──
            var jump = sm.AddState("Jump", new Vector3(540, -120, 0));
            jump.motion = clipJump;

            var locoToJump = locomotion.AddTransition(jump);
            locoToJump.AddCondition(AnimatorConditionMode.If, 0, "Jump");
            locoToJump.hasExitTime = false;
            locoToJump.duration = 0.05f;

            // ── Fall state ──
            var fall = sm.AddState("Fall", new Vector3(540, 60, 0));
            fall.motion = clipFall != null ? clipFall : clipJump; // fall back to jump pose

            var jumpToFall = jump.AddTransition(fall);
            jumpToFall.AddCondition(AnimatorConditionMode.Less, -0.5f, "VelocityY");
            jumpToFall.AddCondition(AnimatorConditionMode.IfNot, 0, "IsGrounded");
            jumpToFall.hasExitTime = false;
            jumpToFall.duration = 0.1f;

            var locoToFall = locomotion.AddTransition(fall);
            locoToFall.AddCondition(AnimatorConditionMode.Less, -1.5f, "VelocityY");
            locoToFall.AddCondition(AnimatorConditionMode.IfNot, 0, "IsGrounded");
            locoToFall.hasExitTime = false;
            locoToFall.duration = 0.15f;

            // ── Land back to locomotion ──
            var land = sm.AddState("Land", new Vector3(260, 180, 0));
            land.motion = clipLand != null ? clipLand : clipIdle;

            var fallToLand = fall.AddTransition(land);
            fallToLand.AddCondition(AnimatorConditionMode.If, 0, "IsGrounded");
            fallToLand.hasExitTime = false;
            fallToLand.duration = 0.08f;

            var jumpToLand = jump.AddTransition(land);
            jumpToLand.AddCondition(AnimatorConditionMode.If, 0, "IsGrounded");
            jumpToLand.AddCondition(AnimatorConditionMode.Less, 0.0f, "VelocityY");
            jumpToLand.hasExitTime = false;
            jumpToLand.duration = 0.08f;

            var landToLoco = land.AddTransition(locomotion);
            landToLoco.hasExitTime = true;
            landToLoco.exitTime = 0.8f;
            landToLoco.duration = 0.1f;

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Log what we picked.
            var log = new System.Text.StringBuilder();
            log.AppendLine($"[Hearthbound/Phase 26] Built {ControllerPath}");
            log.AppendLine($"  Idle  → {AssetPathOr(clipIdle, "<missing>")}");
            log.AppendLine($"  Walk  → {AssetPathOr(clipWalk, "<falling back to Idle>")}");
            log.AppendLine($"  Run   → {AssetPathOr(clipRun,  "<falling back to Walk>")}");
            log.AppendLine($"  Jump  → {AssetPathOr(clipJump, "<missing — Jump state will be a hold>")}");
            log.AppendLine($"  Fall  → {AssetPathOr(clipFall, "<falling back to Jump pose>")}");
            log.AppendLine($"  Land  → {AssetPathOr(clipLand, "<falling back to Idle>")}");
            Debug.Log(log.ToString());
        }

        // ───── Asset helpers ───────────────────────────────────────

        private static AnimatorController CreateOrReplaceController(string path)
        {
            // If a prior controller exists, delete it and start fresh — keeping
            // the path stable so prefabs that reference it stay wired.
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(path) != null)
            {
                AssetDatabase.DeleteAsset(path);
            }
            return AnimatorController.CreateAnimatorControllerAtPath(path);
        }

        private static AnimationClip FindBestClip(string[] keywords)
        {
            // Search project-wide for AnimationClips matching any keyword.
            // Humanoid clips win — we tag-score them up.
            var guids = AssetDatabase.FindAssets("t:AnimationClip");
            var candidates = new List<(AnimationClip clip, int score, string path)>();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if (clip == null) continue;

                var lowerName = clip.name.ToLowerInvariant();
                var lowerPath = path.ToLowerInvariant();

                int score = 0;
                foreach (var kw in keywords)
                {
                    if (lowerName.Contains(kw)) score += 25;
                    else if (lowerPath.Contains(kw)) score += 10;
                }
                if (score == 0) continue;

                if (lowerPath.Contains("mixamo")) score += 30;        // Mixamo wins ties
                if (lowerPath.Contains("/bozo")) score += 15;         // BoZo is our chosen baseline
                if (lowerPath.Contains("/_project/animations")) score += 12;

                if (clip.isHumanMotion) score += 20;                  // Humanoid retargetable

                if (lowerName.Contains("_iconcapture")) score -= 1000; // never the editor icon-capture clips
                if (lowerPath.Contains("/demo/")) score += 4;

                candidates.Add((clip, score, path));
            }

            candidates.Sort((a, b) => b.score.CompareTo(a.score));
            for (int i = 0; i < Mathf.Min(3, candidates.Count); i++)
                Debug.Log($"[Hearthbound/Phase 26] Candidate '{candidates[i].clip.name}' (score {candidates[i].score}): {candidates[i].path}");

            return candidates.Count > 0 ? candidates[0].clip : null;
        }

        private static string AssetPathOr(AnimationClip clip, string fallback)
        {
            return clip != null ? AssetDatabase.GetAssetPath(clip) : fallback;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            var leaf = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        // ───── Public lookup ──────────────────────────────────────

        public static AnimatorController TryGetController() =>
            AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
    }
}
