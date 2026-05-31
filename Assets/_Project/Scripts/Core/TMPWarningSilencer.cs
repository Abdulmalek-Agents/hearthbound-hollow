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

using System.Reflection;
using TMPro;
using UnityEngine;

namespace HearthboundHollow.Core
{
    internal static class TMPWarningSilencer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Silence()
        {
            // Phase 32.21 — TMP_Settings.warningsDisabled is read-only in
            // the TMP package shipped with Unity 6 (it's a static getter
            // backed by a private serialized field on TMP_Settings.instance).
            // We poke the field directly via reflection so this works
            // across every TMP version without depending on a public setter.
            try
            {
                var instance = TMP_Settings.instance;
                if (instance == null) return;

                // The field has slightly different names across TMP
                // versions — probe the common ones.
                var t = typeof(TMP_Settings);
                var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                FieldInfo field =
                    t.GetField("m_warningsDisabled", flags) ??
                    t.GetField("m_WarningsDisabled", flags) ??
                    t.GetField("warningsDisabled",   flags);
                if (field != null)
                {
                    field.SetValue(instance, true);
                    return;
                }

                // Some TMP forks expose a static cache instead of an
                // instance field — try that as a fallback.
                FieldInfo staticCache =
                    t.GetField("s_WarningsDisabled", BindingFlags.Static | BindingFlags.NonPublic) ??
                    t.GetField("s_warningsDisabled", BindingFlags.Static | BindingFlags.NonPublic);
                if (staticCache != null)
                {
                    staticCache.SetValue(null, true);
                }
            }
            catch (System.Exception)
            {
                // Swallow — if reflection can't find the field on this TMP
                // version the warnings will still log but boot is unaffected.
                // The artist-side fix (bake an emoji TMP_FontAsset and add
                // it as a fallback in TMP Settings) remains the permanent
                // solution.
            }
        }
    }
}
