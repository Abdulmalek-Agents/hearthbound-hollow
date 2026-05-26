// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / NpcAnimatorBridge
//
// PHASE 26 polish layer — drives the NPC `IsTalking` Animator bool from
// the dialogue runtime so Doris and Gerrold visibly *act* during dialogue
// beats instead of standing motionless.
//
// LIVES IN THE MISSION ASMDEF because it bridges:
//   • `HearthboundHollow.Core.EventBus` + `DialogueStartedEvent` / `DialogueEndedEvent`
//   • `HearthboundHollow.Memory.VillagerSO` (the dialogue events carry a
//     ScriptableObject which is a VillagerSO at runtime)
//   • The Unity `Animator` (any asmdef can use this — no extra dep)
//
// PLACEMENT
//   • Attach one bridge to each NPC GameObject (or the NPC root prefab).
//   • Set `villager` to the VillagerSO this NPC represents (Doris, Gerrold).
//   • The bridge auto-locates the Animator via GetComponentInChildren.
//
// BEHAVIOUR
//   • On `DialogueStartedEvent` whose Villager matches our villager:
//       Animator.SetBool("IsTalking", true);
//   • On `DialogueEndedEvent` whose Villager matches our villager (or any
//     event with a null villager — defensive):
//       Animator.SetBool("IsTalking", false);
//   • Cleanly unsubscribes on `OnDisable` / `OnDestroy`.
//
// Compatible with Hearthbound_NPC.controller (built by
// NpcAnimatorControllerBuilder.cs). If the NPC's Animator uses a different
// controller that doesn't have an `IsTalking` parameter, the SetBool call
// is a silent no-op — Unity logs no error.

using UnityEngine;
using HearthboundHollow.Core;
using HearthboundHollow.Memory;

namespace HearthboundHollow.Mission
{
    [DisallowMultipleComponent]
    public class NpcAnimatorBridge : MonoBehaviour
    {
        [Header("Identity")]
        [Tooltip("The VillagerSO this NPC represents. Used to match against " +
                 "DialogueStartedEvent / DialogueEndedEvent.")]
        public VillagerSO villager;

        [Header("Animator (auto-found if null)")]
        public Animator animator;

        [Header("Parameter name")]
        [Tooltip("Animator bool driven by this bridge. Matches the param in " +
                 "Hearthbound_NPC.controller built by NpcAnimatorControllerBuilder.")]
        public string isTalkingParam = "IsTalking";

        private int _isTalkingHash;
        private bool _subscribed;

        private void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>(true);
            _isTalkingHash = Animator.StringToHash(isTalkingParam);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<DialogueStartedEvent>(OnDialogueStarted);
            EventBus.Subscribe<DialogueEndedEvent>(OnDialogueEnded);
            _subscribed = true;
        }

        private void OnDisable()
        {
            if (!_subscribed) return;
            EventBus.Unsubscribe<DialogueStartedEvent>(OnDialogueStarted);
            EventBus.Unsubscribe<DialogueEndedEvent>(OnDialogueEnded);
            _subscribed = false;

            // Defensive: leave the animator in a neutral state if we go away
            // mid-dialogue.
            if (animator != null) animator.SetBool(_isTalkingHash, false);
        }

        // ───── Event handlers ──────────────────────────────────────

        private void OnDialogueStarted(DialogueStartedEvent e)
        {
            if (!MatchesUs(e.Villager)) return;
            if (animator == null) return;
            animator.SetBool(_isTalkingHash, true);
            Hh.Log(LogCategory.Dialogue, $"{name}: IsTalking=true (DialogueStartedEvent matched).");
        }

        private void OnDialogueEnded(DialogueEndedEvent e)
        {
            // We accept a null Villager on the end event (defensive — some
            // dialogue runners emit DialogueEndedEvent(null) on global close).
            if (e.Villager != null && !MatchesUs(e.Villager)) return;
            if (animator == null) return;
            animator.SetBool(_isTalkingHash, false);
            Hh.Log(LogCategory.Dialogue, $"{name}: IsTalking=false (DialogueEndedEvent).");
        }

        // ───── Helpers ─────────────────────────────────────────────

        private bool MatchesUs(ScriptableObject so)
        {
            // The dialogue events declare Villager as a ScriptableObject so
            // any subsystem can reference Core without taking a Memory asmdef
            // dep. At runtime it's always a VillagerSO, so an identity check
            // against our serialized villager reference is sufficient.
            return so != null && villager != null && ReferenceEquals(so, villager);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Re-cache the hash in editor when the parameter name changes.
            _isTalkingHash = Animator.StringToHash(isTalkingParam);
        }
#endif
    }
}
