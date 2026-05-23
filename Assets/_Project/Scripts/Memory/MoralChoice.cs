// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Memory / MoralChoice + CleanseOutcome
//
// The 4 moral-choice outcomes from Focus 02 § 5 (Gerrold's choice card) and
// the 4 cleanse outcomes from Focus 04 § 3.5. Both enums are cast to int
// when published via GameEvents to keep the Core asmdef independent of this.

namespace HearthboundHollow.Memory
{
    /// <summary>The four moral-choice paths on a memory transaction.</summary>
    public enum MoralChoice
    {
        Erase = 0,    // Aggressive Cleanse — forget entirely
        Cleanse = 1,  // Careful Cleanse — soften the trauma but keep the memory
        Listen = 2,   // Refuse to take, sit with the villager instead
        Defer = 3,    // Take but don't act; the orb stays on the counter
    }

    /// <summary>Cleanse mini-game outcomes (Focus 04 § 3.5).</summary>
    public enum CleanseOutcome
    {
        Perfect = 0,      // All 4 cracks sealed cleanly; 0 core crossings
        Acceptable = 1,   // 3 of 4 cracks sealed; 0 core crossings
        Sloppy = 2,       // 4 cracks sealed but 1–2 brief core touches
        CrossedCore = 3,  // 3+ core touches OR sustained crossing — memory permanently altered
    }
}
