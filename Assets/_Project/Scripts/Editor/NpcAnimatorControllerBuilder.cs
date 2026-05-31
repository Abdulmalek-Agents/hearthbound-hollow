// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / NpcAnimatorControllerBuilder
//
// PHASE 29 EXPANSION (2026-05-26) — full NPC animator graph:
//
//   Hearthbound_NPC_Doris.controller     ← Female BoZo (Cleric reskin)
//   Hearthbound_NPC_Gerrold.controller   ← Male BoZo (Bard reskin)
//   Hearthbound_NPC_SilentLane.controller← Male BoZo (Warrior reskin, old man)
//   Hearthbound_NPC.controller           ← Generic fallback (kept for back-compat)
//
//   Each controller layout:
//   ├── Parameters
//   │     Speed              (float, default 0)   — for locomotion blend
//   │     IsTalking          (bool,  default false)
//   │     ChoreographyState  (int,   default 0)   — drives the scripted-beat
//   │                                                state machine via trigger
//   │                                                indirection. Each scripted
//   │                                                beat also has a Trigger of
//   │                                                the same name so directors
//   │                                                can `SetTrigger("OfferBox")`.
//   ├── Triggers              — fired by directors to enter a one-shot beat:
//   │     OfferBox            — Doris hands the orb over (Focus 01 § 5.1)
//   │     Kneading            — Doris on stool kneading dough loop
//   │     WipeApron           — Doris dusts flour after rising
//   │     SitDown             — Gerrold sits in chair (Focus 02 § 4.0)
//   │     StandUp             — Gerrold rises from chair
//   │     Disbelief           — Gerrold reacts to a "Crossed Core" outcome
//   │     Reading             — SilentLane villager reads a letter on the bench
//   │     Wave                — SilentLane villager nods/waves when greeted
//   └── Base Layer
//         ├── Locomotion  (1D Blend Tree on Speed)
//         │     ├── Idle  threshold 0
//         │     └── Walk  threshold 1
//         ├── Talking     (entered when IsTalking == true)
//         └── Choreography states  (one per Trigger above; ExitTime returns to
//                                   Locomotion or to Sit "rest" state)
//
// All scripted-beat states default to the Idle clip until Mixamo-licensed
// gesture clips land (see Docs/GAME_ANIMATION_GUIDELINES.md). The state
// machine wiring is the architectural promise — clips can hot-swap in
// without code changes.
//
// USAGE
//   • Menu: `Hearthbound → ⚙️ Advanced → Phase 29 — Build NPC Animator Controllers`
//   • Programmatic: NpcAnimatorControllerBuilder.BuildAllOrUpdate()
//                   NpcAnimatorControllerBuilder.BuildOrUpdate() (legacy)
//
// Demoted to ⚙️ Advanced/… in Phase 32 (menu collapse). The user-facing
// entry point is now `Hearthbound → 🚀 Build Everything`, which chains
// these controllers via Phase 26 (NPC Animators) capstone.
//
// IDEMPOTENT — re-running overwrites the assets at the same paths so prefab
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

        // Generic fallback (kept for back-compat with any code that hard-codes
        // the path). New NPC prefabs should use the per-character variants.
        public const string ControllerPath = AnimationsDir + "/Hearthbound_NPC.controller";

        // Per-character variants — each tuned for the right BoZo sex idle.
        public const string DorisControllerPath      = AnimationsDir + "/Hearthbound_NPC_Doris.controller";
        public const string GerroldControllerPath    = AnimationsDir + "/Hearthbound_NPC_Gerrold.controller";
        public const string SilentLaneControllerPath = AnimationsDir + "/Hearthbound_NPC_SilentLane.controller";

        // Trigger names — exposed as constants so runtime code (Mission01/02
        // Directors, NpcAnimatorChoreographer) can reference them.
        public const string TriggerOfferBox  = "OfferBox";
        public const string TriggerKneading  = "Kneading";
        public const string TriggerWipeApron = "WipeApron";
        public const string TriggerSitDown   = "SitDown";
        public const string TriggerStandUp   = "StandUp";
        public const string TriggerDisbelief = "Disbelief";
        public const string TriggerReading   = "Reading";
        public const string TriggerWave      = "Wave";

        // ── Clip search keywords ───────────────────────────────────

        // BoZo male/female idle conventions — F_* for Doris, M_* for Gerrold/Silent.
        private static readonly string[] FemaleIdleKeywords  = { "f_idle", "female_idle", "idle_female", "breathing_idle", "idle" };
        private static readonly string[] MaleIdleKeywords    = { "m_idle", "male_idle", "idle_male",   "breathing_idle", "idle" };
        private static readonly string[] FemaleWalkKeywords  = { "f_walk", "female_walk", "walking_female", "walking", "walk" };
        private static readonly string[] MaleWalkKeywords    = { "m_walk", "male_walk", "walking_male",     "walking", "walk" };
        // Scripted-beat search — these will usually miss in M1+M2 and fall
        // back to the idle clip; that is the design.
        private static readonly string[] OfferKeywords    = { "offer", "hand_off", "handover", "give" };
        private static readonly string[] KneadKeywords    = { "knead", "dough", "bake" };
        private static readonly string[] SitKeywords      = { "sitting_idle", "sit_idle", "sit" };
        private static readonly string[] StandUpKeywords  = { "standing_up", "stand_up", "rise" };
        private static readonly string[] DisbeliefKeywords= { "disbelief", "shock", "shake_head" };
        private static readonly string[] ReadKeywords     = { "reading", "read_book", "read_letter" };
        private static readonly string[] WaveKeywords     = { "wave", "nod", "greet" };
        private static readonly string[] TalkKeywords     = { "talking", "talk_idle", "talk", "speak", "conversation" };

        // PHASE 29 — explicit preferred-asset lookup paths (same pattern as
        // PlayerAnimatorControllerBuilder). Mixamo first, then BoZo BMAC.
        private static readonly string[] PreferredFemaleIdle = {
            "Assets/_Project/Animations/Mixamo/Female_Idle.anim",
            "Assets/_Project/Animations/Mixamo/Breathing Idle F.anim",
            "Assets/BoZo_StylizedModularCharacters/Animations/BMAC_F_Idle.anim",
            "Assets/BoZo_StylizedModularCharacters/Animations/BMAC_M_Idle.anim", // last-resort
        };
        private static readonly string[] PreferredMaleIdle = {
            "Assets/_Project/Animations/Mixamo/Male_Idle.anim",
            "Assets/_Project/Animations/Mixamo/Breathing Idle M.anim",
            "Assets/BoZo_StylizedModularCharacters/Animations/BMAC_M_Idle.anim",
            "Assets/BoZo_StylizedModularCharacters/Animations/BMAC_F_Idle.anim", // last-resort
        };
        private static readonly string[] PreferredFemaleWalk = {
            "Assets/_Project/Animations/Mixamo/Female_Walking.anim",
            "Assets/BoZo_StylizedModularCharacters/Animations/BMAC_F_Walk.anim",
            "Assets/BoZo_StylizedModularCharacters/Animations/BMAC_M_Walk.anim",
        };
        private static readonly string[] PreferredMaleWalk = {
            "Assets/_Project/Animations/Mixamo/Male_Walking.anim",
            "Assets/BoZo_StylizedModularCharacters/Animations/BMAC_M_Walk.anim",
            "Assets/BoZo_StylizedModularCharacters/Animations/BMAC_F_Walk.anim",
        };

        // ──────────────────────────────────────────────────────────
        // Menu entries
        // ──────────────────────────────────────────────────────────

        [MenuItem("Hearthbound/⚙️ Advanced/Phase 29 — Build NPC Animator Controllers", priority = 211)]
        public static void BuildAllOrUpdate()
        {
            EnsureFolder(AnimationsDir);
            EnsureFolder(AnimationsDir + "/Mixamo");

            // Generic fallback (kept for back-compat).
            BuildOrUpdate();

            // Per-character variants.
            BuildVariant(DorisControllerPath,      isFemale: true,  hasSitState: false, hasReadingState: false);
            BuildVariant(GerroldControllerPath,    isFemale: false, hasSitState: true,  hasReadingState: false);
            BuildVariant(SilentLaneControllerPath, isFemale: false, hasSitState: true,  hasReadingState: true);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Hearthbound/Phase 29 NPC] Built 4 NPC controllers (generic + Doris + Gerrold + SilentLane).");
        }

        [MenuItem("Hearthbound/⚙️ Advanced/Phase 26 — Build NPC Animator Controller", priority = 210)]
        public static void BuildOrUpdate()
        {
            // Generic fallback — keeps any old prefab references valid.
            // Defaults to male idle (most cozy NPCs we'll add later are male
            // in BoZo's chibi roster), with Speed + IsTalking + the full
            // trigger set.
            EnsureFolder(AnimationsDir);
            BuildVariant(ControllerPath, isFemale: false, hasSitState: true, hasReadingState: true);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        // ──────────────────────────────────────────────────────────
        // Per-variant builder
        // ──────────────────────────────────────────────────────────

        private static void BuildVariant(string path, bool isFemale, bool hasSitState, bool hasReadingState)
        {
            // Find best clips for this variant.
            var idleClip = (isFemale ? TryPreferred(PreferredFemaleIdle) : TryPreferred(PreferredMaleIdle))
                         ?? FindBestClip(isFemale ? FemaleIdleKeywords : MaleIdleKeywords);
            var walkClip = (isFemale ? TryPreferred(PreferredFemaleWalk) : TryPreferred(PreferredMaleWalk))
                         ?? FindBestClip(isFemale ? FemaleWalkKeywords : MaleWalkKeywords)
                         ?? idleClip;   // fall back so the blend doesn't crash if BoZo Walk is absent
            var talkClip      = FindBestClip(TalkKeywords)      ?? idleClip;
            var offerClip     = FindBestClip(OfferKeywords)     ?? idleClip;
            var kneadClip     = FindBestClip(KneadKeywords)     ?? idleClip;
            var wipeClip      = FindBestClip(new[] { "wipe", "dust" }) ?? idleClip;
            var sitClip       = FindBestClip(SitKeywords)       ?? idleClip;
            var standUpClip   = FindBestClip(StandUpKeywords)   ?? idleClip;
            var disbeliefClip = FindBestClip(DisbeliefKeywords) ?? idleClip;
            var readingClip   = FindBestClip(ReadKeywords)      ?? sitClip;
            var waveClip      = FindBestClip(WaveKeywords)      ?? idleClip;

            // Create / replace controller.
            var controller = CreateOrReplaceController(path);

            // Parameters.
            controller.AddParameter("Speed",     AnimatorControllerParameterType.Float);
            controller.AddParameter("IsTalking", AnimatorControllerParameterType.Bool);

            controller.AddParameter(TriggerOfferBox,  AnimatorControllerParameterType.Trigger);
            controller.AddParameter(TriggerKneading,  AnimatorControllerParameterType.Trigger);
            controller.AddParameter(TriggerWipeApron, AnimatorControllerParameterType.Trigger);
            controller.AddParameter(TriggerSitDown,   AnimatorControllerParameterType.Trigger);
            controller.AddParameter(TriggerStandUp,   AnimatorControllerParameterType.Trigger);
            controller.AddParameter(TriggerDisbelief, AnimatorControllerParameterType.Trigger);
            controller.AddParameter(TriggerReading,   AnimatorControllerParameterType.Trigger);
            controller.AddParameter(TriggerWave,      AnimatorControllerParameterType.Trigger);

            var sm = controller.layers[0].stateMachine;

            // ── Locomotion blend tree (Speed: 0=Idle, 1=Walk) ──
            var locomotion = sm.AddState("Locomotion", new Vector3(260, 0, 0));
            var blendTree = new BlendTree
            {
                name = "NPC_Locomotion_Blend",
                blendType = BlendTreeType.Simple1D,
                blendParameter = "Speed",
                useAutomaticThresholds = false,
                minThreshold = 0f,
                maxThreshold = 1f,
                hideFlags = HideFlags.HideInHierarchy,
            };
            AssetDatabase.AddObjectToAsset(blendTree, controller);
            blendTree.AddChild(idleClip, 0f);
            blendTree.AddChild(walkClip, 1f);
            // Lock thresholds (Unity 6 quirk).
            var bc = blendTree.children;
            if (bc.Length >= 2) { bc[0].threshold = 0f; bc[1].threshold = 1f; blendTree.children = bc; }
            locomotion.motion = blendTree;
            sm.defaultState = locomotion;

            // ── Talking ──
            var talking = sm.AddState("Talking", new Vector3(540, 0, 0));
            talking.motion = talkClip;

            var locoToTalk = locomotion.AddTransition(talking);
            locoToTalk.AddCondition(AnimatorConditionMode.If, 0, "IsTalking");
            locoToTalk.hasExitTime = false; locoToTalk.duration = 0.18f;

            var talkToLoco = talking.AddTransition(locomotion);
            talkToLoco.AddCondition(AnimatorConditionMode.IfNot, 0, "IsTalking");
            talkToLoco.hasExitTime = false; talkToLoco.duration = 0.22f;

            // ── Choreography states (one-shot beats) ──
            // Each scripted beat is `AnyState → State` on its trigger so the
            // mission director can `SetTrigger("OfferBox")` from anywhere.
            // After the clip plays, the state ExitTime returns to Locomotion
            // (or to Sit "rest" for SitDown beats).
            AddChoreoState(sm, controller, "OfferBox",   offerClip,     TriggerOfferBox,  new Vector3(820,  -160, 0), returnTo: locomotion, exitTime: 0.95f, fixedDuration: 0.25f);
            AddChoreoState(sm, controller, "Kneading",   kneadClip,     TriggerKneading,  new Vector3(820,   -80, 0), returnTo: locomotion, exitTime: 0.95f, fixedDuration: 0.3f, looping: true);
            AddChoreoState(sm, controller, "WipeApron",  wipeClip,      TriggerWipeApron, new Vector3(820,     0, 0), returnTo: locomotion, exitTime: 0.95f, fixedDuration: 0.2f);

            if (hasSitState)
            {
                // Sit goes into a holding state (SittingIdle) — we don't auto-
                // exit; the director calls StandUp explicitly later.
                var sit = AddChoreoState(sm, controller, "SitDown", sitClip, TriggerSitDown,
                    new Vector3(820, 80, 0), returnTo: null, exitTime: 0.9f, fixedDuration: 0.25f);

                var sittingIdle = sm.AddState("SittingIdle", new Vector3(1060, 80, 0));
                sittingIdle.motion = sitClip;
                var sitToHold = sit.AddTransition(sittingIdle);
                sitToHold.hasExitTime = true; sitToHold.exitTime = 0.95f; sitToHold.duration = 0.1f;

                var standUp = AddChoreoState(sm, controller, "StandUp", standUpClip, TriggerStandUp,
                    new Vector3(820, 160, 0), returnTo: locomotion, exitTime: 0.9f, fixedDuration: 0.25f);
                // Wire SittingIdle → StandUp on the StandUp trigger.
                var sittingToStand = sittingIdle.AddTransition(standUp);
                sittingToStand.AddCondition(AnimatorConditionMode.If, 0, TriggerStandUp);
                sittingToStand.hasExitTime = false; sittingToStand.duration = 0.15f;

                if (hasReadingState)
                {
                    var reading = AddChoreoState(sm, controller, "Reading", readingClip, TriggerReading,
                        new Vector3(1060, 160, 0), returnTo: sittingIdle, exitTime: 0.95f, fixedDuration: 0.2f);
                    // While sitting, the Reading trigger can also be fired
                    // directly to enter Reading from SittingIdle.
                    var sitToRead = sittingIdle.AddTransition(reading);
                    sitToRead.AddCondition(AnimatorConditionMode.If, 0, TriggerReading);
                    sitToRead.hasExitTime = false; sitToRead.duration = 0.18f;
                }
            }

            AddChoreoState(sm, controller, "Disbelief", disbeliefClip, TriggerDisbelief, new Vector3(820, 240, 0), returnTo: locomotion, exitTime: 0.9f, fixedDuration: 0.2f);
            AddChoreoState(sm, controller, "Wave",      waveClip,      TriggerWave,      new Vector3(820, 320, 0), returnTo: locomotion, exitTime: 0.9f, fixedDuration: 0.2f);

            EditorUtility.SetDirty(controller);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"[Hearthbound/Phase 29 NPC] Built {path} ({(isFemale ? "FEMALE" : "MALE")})");
            sb.AppendLine($"  Idle → {AssetPathOr(idleClip, "<missing>")}");
            sb.AppendLine($"  Walk → {AssetPathOr(walkClip, "<falls back to Idle>")}");
            sb.AppendLine($"  Talk → {AssetPathOr(talkClip, "<falls back to Idle>")}");
            Debug.Log(sb.ToString());
        }

        /// <summary>
        /// Adds a "any-state on trigger → choreo state → return-to-default"
        /// triplet. Returns the choreo state so callers can wire follow-up
        /// transitions (e.g. SitDown → SittingIdle holding state).
        /// </summary>
        private static AnimatorState AddChoreoState(
            AnimatorStateMachine sm,
            AnimatorController controller,
            string stateName,
            AnimationClip motion,
            string triggerName,
            Vector3 position,
            AnimatorState returnTo,
            float exitTime,
            float fixedDuration,
            bool looping = false)
        {
            var state = sm.AddState(stateName, position);
            state.motion = motion;
            // Looped scripted beats (Kneading) run via state speed > 0; AnyState
            // re-entry is gated by the trigger pattern.

            // AnyState → state on its trigger. This lets the director fire the
            // beat from Locomotion / Talking / Sitting / wherever.
            var anyToState = sm.AddAnyStateTransition(state);
            anyToState.AddCondition(AnimatorConditionMode.If, 0, triggerName);
            anyToState.hasExitTime = false;
            anyToState.duration = fixedDuration;
            anyToState.canTransitionToSelf = false;

            // state → returnTo (if specified) on ExitTime. Looping states
            // intentionally have no auto-exit (e.g. Kneading loops until the
            // director calls a different beat).
            if (returnTo != null && !looping)
            {
                var stateToReturn = state.AddTransition(returnTo);
                stateToReturn.hasExitTime = true;
                stateToReturn.exitTime = exitTime;
                stateToReturn.duration = 0.18f;
            }

            return state;
        }

        // ──────────────────────────────────────────────────────────
        // Asset helpers
        // ──────────────────────────────────────────────────────────

        private static AnimatorController CreateOrReplaceController(string path)
        {
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(path) != null)
                AssetDatabase.DeleteAsset(path);
            return AnimatorController.CreateAnimatorControllerAtPath(path);
        }

        private static AnimationClip TryPreferred(string[] preferredPaths)
        {
            for (int i = 0; i < preferredPaths.Length; i++)
            {
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(preferredPaths[i]);
                if (clip != null) return clip;
            }
            return null;
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

                // PHASE 29 — same hard penalties as the player builder.
                if (lowerName.Contains("_iconcapture")) score -= 1000;
                if (lowerName.Contains("face")) score -= 500;
                if (lowerName.Contains("expression")) score -= 500;
                if (lowerName.Contains("blink")) score -= 500;
                if (lowerName.Contains("t-pose") || lowerName.Contains("tpose")) score -= 800;
                if (lowerPath.Contains("/expressions/")) score -= 500;
                if (lowerPath.Contains("/ui/")) score -= 800;
                if (lowerPath.Contains("/demo/")) score -= 6;

                if (score > bestScore) { best = clip; bestScore = score; bestPath = path; }
            }
            if (best != null) Debug.Log($"[Hearthbound/Phase 29 NPC] Picked '{best.name}' (score {bestScore}): {bestPath}");
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

        // Back-compat public APIs.
        public static AnimatorController TryGetController() =>
            AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);

        public static AnimatorController TryGetDorisController() =>
            AssetDatabase.LoadAssetAtPath<AnimatorController>(DorisControllerPath);

        public static AnimatorController TryGetGerroldController() =>
            AssetDatabase.LoadAssetAtPath<AnimatorController>(GerroldControllerPath);

        public static AnimatorController TryGetSilentLaneController() =>
            AssetDatabase.LoadAssetAtPath<AnimatorController>(SilentLaneControllerPath);
    }
}
