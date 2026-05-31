// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / TomorrowTeaseSO
//
// Phase 47 — data for one day-end's "One More Day" goodnight card.
// Prose is mirrored from EveningLedger.yarn / Pickle.yarn (sourceNode names
// recorded for traceability — the Yarn file stays the canonical source of
// record per the directors' existing mirroring convention; the runtime
// Yarn-dispatcher is a separate future pass per PLAYTEST_AUDIT.md). Tier 2
// fields are present but unused until the Tier 2 visual-anticipation pass.
//
// NOTE on afterDayIndex: this is the *fiction* day number the card follows
// (1 = Mission 1 "Day 1", 2 = Mission 2 "Day 2"). VillageState.currentDayIndex
// is 0-based and is only incremented by GameManager.EndDay() *after* the card
// has resolved, so EndOfDaySequencer matches against (currentDayIndex + 1).
// See EndOfDaySequencer.ResolveTease().

using UnityEngine;

namespace HearthboundHollow.Mission
{
    [CreateAssetMenu(menuName = "Hearthbound/Tomorrow Tease", fileName = "TomorrowTease")]
    public class TomorrowTeaseSO : ScriptableObject
    {
        [Tooltip("Fiction day this card follows (1 = M1 Day 1, 2 = M2 Day 2). " +
                 "Matched against VillageState.currentDayIndex + 1.")]
        public int afterDayIndex = 1;

        [Header("Forward-look — canonical source: EveningLedger.yarn")]
        public string sourceNode = "Tomorrow_M1_Day1";
        [TextArea(2, 5)] public string forwardLookText;

        [Header("Branch variant (optional)")]
        [Tooltip("VillageState bool field name. When that field is true, the Alt text is used. Empty = always main.")]
        public string branchFlagField = "";
        public string sourceNodeAlt = "";
        [TextArea(2, 5)] public string forwardLookTextAlt;

        [Header("Pickle goodnight (optional, rendered italic)")]
        public string pickleSourceNode = "";
        [TextArea(1, 4)] public string pickleSignOffText;

        [Header("Tier 2 (unused until Tier 2)")]
        public Sprite visitorSilhouette;
        public bool showEchoThreadGlimmer;
    }
}
