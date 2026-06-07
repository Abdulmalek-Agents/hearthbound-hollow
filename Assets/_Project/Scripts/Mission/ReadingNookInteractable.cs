// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / ReadingNookInteractable
//
// The Reading Nook armchair in the Hollow interior. Presents Marin Vellis's
// hand-authored letter fragments when the player sits down, gated by Pickle's
// approval (requires the 'DECOR_PICKLE_CUSHION' upgrade).
//
// Approval gate rationale (D-082):
//   Earning coin → buying Pickle's cushion upgrade → Pickle settles in →
//   Pickle approves the nook → deeper predecessor mystery unlocks.
//   This makes the coin loop feel narratively meaningful, not just economic.
//
// ── ASMDEF NOTE (mirrors MarinNoteInteractable, D-035) ─────────────
// Lives in the HearthboundHollow.Mission asmdef — NOT HearthboundHollow.Player
// — because it bridges Player's Interactable base class with UI's overlay +
// DialogueUI presenters. Mission references both Player and UI; Player does not
// reference UI on purpose.
//
// ── COMPILE FIX (Phase 52.1, D-078) ────────────────────────────────
// The original Phase 52 draft referenced an API surface that never shipped:
//   • base class used without `using HearthboundHollow.Player;` (CS0246)
//   • overrode OnActivate()/GetPromptText() — the real base virtuals are
//     Activate(GameObject) / GetDynamicPromptText()
//   • published ReadingNookVisitedEvent / ReadingNookFragmentReadEvent — never
//     defined (now declared here as structs, mirroring EchoHologramHeardEvent)
//   • called DialogueUI.ShowOneLiner — no such method; the real API is
//     PresentLine(speaker, text, portrait)
//   • treated VillageState.letterFragmentsRead (an int count) as an id list;
//     the id set now lives in VillageState.letterFragmentIdsRead
//   • called SaveService.Autosave() — the canonical autosave is Save(-1, state)
//
// ── INTEGRATION FIX (Phase 52.2, D-084) ────────────────────────────
// The Pickle gate id was 'pickle_cushion', but the shipping Hollow catalog
// (HollowProgressionService built-in starter) sells the cushion as
// 'DECOR_PICKLE_CUSHION'. The mismatch meant buying the cushion never opened
// the nook. Aligned the default id to the catalog so the coin→cushion→Pickle→
// Marin's-letters loop actually completes.

using System.Collections.Generic;
using UnityEngine;
using HearthboundHollow.Core;
using HearthboundHollow.Memory;
using HearthboundHollow.Player;   // base class Interactable + PlayerController
using HearthboundHollow.UI;       // ReadingNookOverlay + DialogueUI presenters
using HearthboundHollow.Save;     // SaveService (autosave on first-read)

namespace HearthboundHollow.Mission
{
    /// <summary>
    /// Published on the EventBus the first time the player sits at the Reading
    /// Nook. Lets MissionAudioHooks / Almanac / future systems react without a
    /// hard reference back to this interactable.
    /// </summary>
    public readonly struct ReadingNookVisitedEvent { }

    /// <summary>
    /// Published on each first-read of a Marin letter fragment. Carries the
    /// fragment id and the predecessor-trail warmth it granted so the audio
    /// layer can play a reveal cue and the Evening Ledger can add a prose note.
    /// </summary>
    public readonly struct ReadingNookFragmentReadEvent
    {
        public readonly string FragmentId;
        public readonly int WarmthGranted;

        public ReadingNookFragmentReadEvent(string fragmentId, int warmthGranted)
        {
            FragmentId    = fragmentId;
            WarmthGranted = warmthGranted;
        }
    }

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
        [Tooltip("Upgrade id that represents Pickle's approval. MUST match the id in the " +
                 "Hollow catalog for Pickle's cushion — the built-in starter catalog " +
                 "(HollowProgressionService) ships it as 'DECOR_PICKLE_CUSHION'.")]
        [SerializeField] private string pickleApprovalUpgradeId = "DECOR_PICKLE_CUSHION";

        [Header("Pickle Rejection Lines (hand-written)")]
        [TextArea(2, 4)]
        [SerializeField] private string rejectionLineFirst  =
            "That's my spot. Come back when I've decided you're worth it.";

        [TextArea(2, 4)]
        [SerializeField] private string rejectionLineRepeat = "...Still my spot.";

        // ─── State ────────────────────────────────────────────────────────────

        private bool _rejectionShownOnce;
        private bool _overlayOpen;
        private PlayerController _player;   // cached from Activate() so we can unlock on close

        // ─── Interactable overrides ───────────────────────────────────────────

        /// <summary>
        /// Called by the player's interaction raycast when Interact (E) is pressed.
        /// </summary>
        public override void Activate(GameObject player)
        {
            if (!IsInteractable) return;
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
                                 "Run Hearthbound → 🚀 Build Everything to build the Reading Nook.");
                return;
            }

            _overlayOpen = true;
            _player = player != null ? player.GetComponent<PlayerController>() : null;
            LockPlayerMovement(true);

            state.letterFragmentIdsRead ??= new List<string>();

            overlay.Show(
                fragments     : fragments,
                currentWarmth : state.predecessorTrailWarmth,
                alreadyReadIds: state.letterFragmentIdsRead,
                onFragmentRead: OnFragmentRead,
                onClose       : OnOverlayClosed
            );
        }

        /// <summary>Dynamic prompt text shown on the ControlHintsHUD E-chip.</summary>
        public override string GetDynamicPromptText()
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

            state.letterFragmentIdsRead ??= new List<string>();

            // Guard: only process the first reading of this fragment.
            if (state.letterFragmentIdsRead.Contains(fragmentId)) return;

            state.letterFragmentIdsRead.Add(fragmentId);
            state.letterFragmentsRead = state.letterFragmentIdsRead.Count;
            state.predecessorTrailWarmth =
                Mathf.Min(100, state.predecessorTrailWarmth + warmthGranted);

            // Broadcast so MissionAudioHooks plays a reveal cue and the
            // Evening Ledger can add a prose note.
            EventBus.Publish(new ReadingNookFragmentReadEvent(
                fragmentId   : fragmentId,
                warmthGranted: warmthGranted));

            // Autosave the warmth progress (same trigger-class as moral choices).
            // Canonical autosave = SaveService.Save with slot < 0.
            ServiceLocator.Get<SaveService>()?.Save(-1, state);
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
                dialogueUI.PresentLine("Pickle", $"<i>{line}</i>", portrait: null);
            else
                Debug.Log($"[Pickle] {line}"); // Editor fallback
        }

        /// <summary>
        /// Locks or unlocks WASD movement on the player cached in <see cref="Activate"/>,
        /// via PlayerController.MovementLocked — the same hook the Mission directors use.
        /// </summary>
        private void LockPlayerMovement(bool locked)
        {
            if (_player != null)
                _player.MovementLocked = locked;
        }
    }
}
