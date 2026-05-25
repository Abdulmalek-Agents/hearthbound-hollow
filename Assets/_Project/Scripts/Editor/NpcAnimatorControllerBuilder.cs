// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / NpcAnimatorControllerBuilder
//
// PHASE 26 polish layer — builds a tiny shared NPC AnimatorController so
// Doris and Gerrold have an `IsTalking` toggle the dialogue runtime can
// drive during M1 + M2.
//
// Built asset:
//
//   Assets/_Project/Animations/Hearthbound_NPC.controller
//   ├── Parameters
//   │     IsTalking (bool, default false)
//   │     Speed     (float, default 0)   — for future NPC locomotion
//   └── Base Layer
//         ├── Idle      (default state, BoZo F_Idle or M_Idle clip)
//         └── Talking   (entered when IsTalking == true; exits when false)
//
// The Talking clip is just the same Idle clip in M1+M2 (no separate
// "talking" body language Mixamo clip is in scope yet). The state exists
// so Mission directors can drive the bool now — when a richer clip lands,
// only the clip needs to swap, not the wiring.
//
// USAGE
//   • Menu: `Hearthbound → Phase 26 — Build NPC Animator Controller`
//   • Programmatic: NpcAnimatorControllerBuilder.BuildOrUpdate()
//
// The Mission 1 / Mission 2 directors and the NpcAnimatorBridge runtime use
// `ServiceLocator`-style discovery to find the controller — they read it
// off the Animator.runtimeAnimatorController so this asset is opt-in.
//
// IDEMPOTENT — re-running overwrites the asset at the same path so prefab
// references stay valid.

using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace HearthboundHollow.EditorTools
{
    public static class NpcAnimatorControllerBuilder
    {
        public const string AnimationsDir = "Assets/_Project/Animations";
        public const string ControllerPath = AnimationsDir + "/Hearthbound_NPC.controller";

        // Search priority for the Idle clip: same heuristic as the player
        // builder but biased toward BoZo F_* (Doris is reskinned F per Phase 13).
        private static readonly string[] IdleKeywords = { "f_idle", "idle", "breathing_idle", "stand" };
        private static readonly string[] TalkKeywords = { "talk", "talking", "speak", "conversation" };

        [MenuItem("Hearthbound/Phase 26 — Build NPC Animator Controller", priority = 210)]
        public static void BuildOrUpdate()
        {
            EnsureFolder(AnimationsDir);

            var clipIdle = FindBestClip(IdleKeywords);
            var clipTalk = FindBestClip(TalkKeywords);
            // Fallback: Talk reuses Idle if no Talk clip exists in the project.
            if (clipTalk == null) clipTalk = clipIdle;

            var controller = CreateOrReplaceController(ControllerPath);

            controller.AddParameter("IsTalking", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);

            var layer = controller.layers[0];
            var sm = layer.stateMachine;

            // ── Idle ──
            var idle = sm.AddState("Idle", new Vector3(260, 0, 0));
            idle.motion = clipIdle;
            sm.defaultState = idle;

            // ── Talking ──
            var talking = sm.AddState("Talking", new Vector3(540, 0, 0));
            talking.motion = clipTalk;

            var idleToTalk = idle.AddTransition(talking);
            idleToTalk.AddCondition(AnimatorConditionMode.If, 0, "IsTalking");
            idleToTalk.hasExitTime = false;
            idleToTalk.duration = 0.18f;

            var talkToIdle = talking.AddTransition(idle);
            talkToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsTalking");
            talkToIdle.hasExitTime = false;
            talkToIdle.duration = 0.22f;

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"[Hearthbound/Phase 26 NPC] Built {ControllerPath}");
            sb.AppendLine($"  Idle    → {AssetPathOr(clipIdle, "<missing — controller will play empty motion>")}");
            sb.AppendLine($"  Talking → {AssetPathOr(clipTalk, "<falls back to Idle clip>")}");
            Debug.Log(sb.ToString());
        }

        // ───── Helpers ─────────────────────────────────────────────

        private static AnimatorController CreateOrReplaceController(string path)
        {
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(path) != null)
                AssetDatabase.DeleteAsset(path);
            return AnimatorController.CreateAnimatorControllerAtPath(path);
        }

        private static AnimationClip FindBestClip(string[] keywords)
        {
            var guids = AssetDatabase.FindAssets("t:AnimationClip");
            AnimationClip best = null; int bestScore = 0; string bestPath = null;
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

                if (lowerPath.Contains("mixamo")) score += 20;
                if (lowerPath.Contains("/bozo")) score += 12;
                if (clip.isHumanMotion) score += 20;
                if (lowerName.Contains("_iconcapture")) score -= 1000;

                if (score > bestScore) { best = clip; bestScore = score; bestPath = path; }
            }
            if (best != null) Debug.Log($"[Hearthbound/Phase 26 NPC] Picked '{best.name}' (score {bestScore}): {bestPath}");
            return best;
        }

        private static string AssetPathOr(AnimationClip clip, string fallback) =>
            clip != null ? AssetDatabase.GetAssetPath(clip) : fallback;

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            var leaf = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        public static AnimatorController TryGetController() =>
            AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
    }
}
