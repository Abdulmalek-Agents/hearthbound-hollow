// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Core / TMPWarningSilencer
//
// PHASE 32.20 — silence TMP's "character not found" log spam when the
// project intentionally uses cozy emoji decorations on cards that the
// default LiberationSans SDF font doesn't include.
//
// Phase 32.11 dealt with the ▸ glyph (U+25B8) one at a time. Phase 32.20
// adds many more emoji to the Help / Onboarding / Control-Hint cards
// (🚶 ✋ ✨ 🪔 🍂 🕯 …). Each missing glyph spams a per-frame log if the
// font has no fallback that covers it, drowning real warnings.
//
// Fix — `TMP_Settings.warningsDisabled = true` at runtime startup. The
// missing glyphs will still render as the TMP "unknown character" box
// (which is what the user sees in their build screenshots anyway), but
// the console stays clean for real issues. If the artist later bakes a
// proper emoji fallback font asset and assigns it via TMP Settings, the
// boxes turn into real glyphs without code changes.
//
// Auto-runs via [RuntimeInitializeOnLoadMethod] — no scene wiring, no
// component, no game-object cost. Single static call at boot.

using TMPro;
using UnityEngine;

namespace HearthboundHollow.Core
{
    internal static class TMPWarningSilencer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Silence()
        {
            // Defensive — guard against TMP_Settings not yet existing (some
            // package-bootstrap edge cases). Try/catch keeps boot safe.
            try
            {
                TMP_Settings.warningsDisabled = true;
            }
            catch (System.Exception)
            {
                // Swallow — if TMP isn't initialised yet, the warnings will
                // start as soon as it is, and a later editor pass can add an
                // emoji fallback font asset for a proper fix.
            }
        }
    }
}
