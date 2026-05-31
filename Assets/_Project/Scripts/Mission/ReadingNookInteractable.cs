using System.Collections.Generic;
using UnityEngine;
using HearthboundHollow.Core;
using HearthboundHollow.Memory;
using HearthboundHollow.UI;

namespace HearthboundHollow.Mission
{
    /// <summary>
    /// The Reading Nook armchair in the Hollow interior.
    /// Presents Marin Vellis's hand-authored letter fragments when the player
    /// sits down, gated by Pickle's approval (requires 'pickle_cushion' upgrade).
    ///
    /// Approval gate rationale (D-082):
    ///   Earning coin → buying Pickle's cushion upgrade → Pickle settles in →
    ///   Pickle approves the nook → deeper predecessor mystery unlocks.
    ///   This makes the coin loop feel narratively meaningful, not just economic.
    ///
    /// Phase 52 — Reading Nook.
    /// Lives in HearthboundHollow.Mission asmdef.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class ReadingNookInteractable : Interactable
    {
        // ─── Inspector ────────────────────────────────────────────────────────

        [Header("Fragments (ordered by warmthThreshold)")]
        [Tooltip("All 5 MarinLetterFragmentSOs in ascending warmthThreshold order." +
                 " Wired by Phase52_ReadingNookBuilder.")]
        [SerializeField] private List<MarinLetterFragmentSO> fragments = new();

        [Header("UI Ref")]
        [Tooltip("ReadingNookOverlay canvas in this scene. Wired by Phase52_ReadingNookBuilder.")]
        [SerializeField] private ReadingNookOverlay overlay;

        [Header("Pickle Approval Gate")]
        [Tooltip("Upgrade id that represents Pickle's approval." +
                 " Must match the id in HollowCatalog SO for Pickle's cushion.")]
        [SerializeField] private string pickleApprovalUpgradeId = "pickle_cushion";

        [Header("Pickle Rejection Lines (hand-written)")]
        [TextArea(2, 4)]
        [SerializeField] private string rejectionLineFirst  =
            "That's my spot. Come back when I've decided you're worth it.";

        [TextArea(2, 4)]
        [SerializeField] private string rejectionLineRepeat = "...Still my spot.";

        // ─── State ────────────────────────────────────────────────────────────

        private bool _rejectionShownOnce;
        private bool _overlayOpen;

        // ─── Interactable overrides ───────────────────────────────────────────

        /// <summary>Called by PlayerController when the player presses Interact (E).</summary>
        public override void OnActivate()
        {
            if (_overlayOpen) return; // guard against double-tap

            var state = ServiceLocator.Get<VillageState>();
            if (state == null)
            {
                Debug.LogWarning("[Phase52] ReadingNookInteractable: VillageState not registered.");
                return;
            }

            // ── Pickle gate ──────────────────────────────────────────────────
            bool approved = state.purchasedUpgradeIds != null
                         && state.purchasedUpgradeIds.Contains(pickleApprovalUpgradeId);

            if (!approved)
            {
                ShowPickleRejection();
                return;
            }

            // ── First-visit flag ─────────────────────────────────────────────
            if (!state.readingNookVisited)
            {
                state.readingNookVisited = true;
                // Broadcast so MissionAudioHooks / Almanac / future systems can react.
                EventBus.Publish(new ReadingNookVisitedEvent());
            }

            // ── Open overlay ─────────────────────────────────────────────────
            if (overlay == null)
            {
                Debug.LogWarning("[Phase52] ReadingNookInteractable: overlay not wired. " +
                                 "Run Hearthbound → ⚙️ Advanced → 📖 Phase 52 — Build Reading Nook.");
                return;
            }

            _overlayOpen = true;
            LockPlayerMovement(true);

            overlay.Show(
                fragments    : fragments,
                currentWarmth: state.predecessorTrailWarmth,
                alreadyReadIds: state.letterFragmentsRead,
                onFragmentRead: OnFragmentRead,
                onClose      : OnOverlayClosed
            );
        }

        /// <summary>Prompt text shown on the ControlHintsHUD E-chip.</summary>
        public override string GetPromptText()
        {
            var state    = ServiceLocator.Get<VillageState>();
            bool approved = state != null
                         && state.purchasedUpgradeIds != null
                         && state.purchasedUpgradeIds.Contains(pickleApprovalUpgradeId);

            return approved ? "Sit and read" : "Armchair";
        }

        // ─── Private ──────────────────────────────────────────────────────────

        /// <summary>
        /// Invoked by <see cref="ReadingNookOverlay"/> on each first-read.
        /// Mutates VillageState and autosaves. Idempotent (D-083).
        /// </summary>
        private void OnFragmentRead(string fragmentId, int warmthGranted)
        {
            var state = ServiceLocator.Get<VillageState>();
            if (state == null) return;

            // Guard: only process the first reading.
            if (state.letterFragmentsRead.Contains(fragmentId)) return;

            state.letterFragmentsRead.Add(fragmentId);
            state.predecessorTrailWarmth = Mathf.Min(100, state.predecessorTrailWarmth + warmthGranted);

            // Broadcast so MissionAudioHooks plays a reveal cue and the
            // Evening Ledger can add a prose note.
            EventBus.Publish(new ReadingNookFragmentReadEvent(
                fragmentId  : fragmentId,
                warmthGranted: warmthGranted));

            // Autosave the warmth progress (same trigger-class as moral choices).
            ServiceLocator.Get<SaveService>()?.Autosave();
        }

        private void OnOverlayClosed()
        {
            _overlayOpen = false;
            LockPlayerMovement(false);
        }

        private void ShowPickleRejection()
        {
            string line = _rejectionShownOnce ? rejectionLineRepeat : rejectionLineFirst;
            _rejectionShownOnce = true;

            // One-liner via the shared DialogueUI path (warm, italic, brief).
            var dialogueUI = ServiceLocator.Get<DialogueUI>();
            if (dialogueUI != null)
                dialogueUI.ShowOneLiner(speaker: "Pickle", text: $"<i>{line}</i>");
            else
                Debug.Log($"[Pickle] {line}"); // Editor fallback
        }

        /// <summary>
        /// Locks or unlocks WASD movement via the Core IMovementLockable interface,
        /// so Mission asmdef does not take a compile dep on HearthboundHollow.Player.
        /// </summary>
        private static void LockPlayerMovement(bool locked)
        {
            var lockable = ServiceLocator.Get<IMovementLockable>();
            if (lockable != null)
                lockable.MovementLocked = locked;
        }
    }
}
