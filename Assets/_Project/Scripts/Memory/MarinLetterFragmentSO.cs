using UnityEngine;

namespace HearthboundHollow.Memory
{
    /// <summary>
    /// A single hand-authored letter fragment from Marin Vellis, the previous Hollow-keeper.
    /// Found in the Reading Nook armchair; unlocked progressively as
    /// VillageState.predecessorTrailWarmth crosses each fragment's warmthThreshold.
    ///
    /// Phase 52 — Reading Nook subsystem.
    /// Lives in HearthboundHollow.Memory asmdef (→ Core only).
    /// </summary>
    [CreateAssetMenu(
        fileName  = "MarinLetter_",
        menuName  = "Hearthbound/Memory/Marin Letter Fragment",
        order     = 12)]
    public class MarinLetterFragmentSO : ScriptableObject
    {
        // ─── Identity ──────────────────────────────────────────────────────────

        [Tooltip("Stable id used to track whether this fragment has been read.\n" +
                 "Stored in VillageState.letterFragmentsRead. Never change after shipping.")]
        public string fragmentId;

        [Tooltip("Short title shown in the nook's fragment list, e.g. \"Day One\".")]
        public string title;

        // ─── Unlock Condition ─────────────────────────────────────────────────

        [Header("Unlock Condition")]
        [Tooltip("Minimum predecessorTrailWarmth required to read this fragment.\n" +
                 "Fragments above the player's current warmth appear greyed-out as 'Not yet...'\n" +
                 "Set to 0 to unlock from day one.")]
        [Range(0, 100)]
        public int warmthThreshold;

        // ─── Content ──────────────────────────────────────────────────────────

        [Header("Content")]
        [Tooltip("The full letter text. Vellis-tier hand-written prose only.\n" +
                 "AI-generated dialogue is forbidden (GAME_DESIGN §9 Pillar 1).")]
        [TextArea(6, 30)]
        public string letterText;

        // ─── Rewards ─────────────────────────────────────────────────────────

        [Header("Rewards")]
        [Tooltip("Predecessor-trail warmth granted on FIRST reading of this fragment.\n" +
                 "Re-reading grants no additional warmth (idempotent). D-083.")]
        [Range(0, 20)]
        public int warmthGranted = 5;

        [Tooltip("Optional: id of an Echo connection to reveal when this letter is read for the first time.\n" +
                 "Leave blank for letters with no direct Memory Wall link.")]
        public string echoConnectionId;
    }
}
