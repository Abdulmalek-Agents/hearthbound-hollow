// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Core / DialogueLocalization
//
// PHASE 57 (D-074) — runtime accessor for the Arabic dialogue-text table.
//
// Loads Resources/DialogueLocalization_ar once (cached). TryGetArabic returns
// false when the table is absent OR the line is untranslated, so callers fall
// back to the canonical English line. Pure plumbing — ships with an empty table
// (Pillar 1 / D-065: the Arabic prose is a human translation pass).

using System;
using System.Collections.Generic;
using UnityEngine;

namespace HearthboundHollow.Core
{
    public static class DialogueLocalization
    {
        public const string ResourcesName = "DialogueLocalization_ar";

        private static DialogueLocalizationSO _table;
        private static bool _loaded;

        /// <summary>
        /// True + the Arabic text (logical order) when a human translation exists
        /// for <paramref name="lineId"/>; otherwise false (caller keeps English).
        /// </summary>
        public static bool TryGetArabic(string lineId, out string text)
        {
            text = null;
            if (string.IsNullOrEmpty(lineId)) return false;
            if (!_loaded)
            {
                _table = Resources.Load<DialogueLocalizationSO>(ResourcesName);
                _loaded = true;
            }
            // 1) Human-authored SO override (translator pass) wins when present.
            if (_table != null && _table.TryGet(lineId, out text)) return true;
            // 2) D-077 machine-translated stopgap (explicit Pillar 1 override by
            //    user request; replace with the human pass before ship).
            return DialogueLocalizationData.Ar.TryGetValue(lineId, out text) &&
                   !string.IsNullOrEmpty(text);
        }

        // Speaker-name transliterations for the dialogue box (D-077 stopgap).
        private static readonly Dictionary<string, string> _speakerAr =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["Doris"]    = "دوريس",
                ["Gerrold"]  = "جيرولد",
                ["Marin"]    = "مارين",
                ["Pickle"]   = "بيكل",
                ["Narrator"] = "الراوي",
            };

        /// <summary>Arabic display name for a known speaker; false otherwise.</summary>
        public static bool TryGetSpeakerArabic(string speaker, out string ar)
        {
            ar = null;
            return !string.IsNullOrEmpty(speaker) && _speakerAr.TryGetValue(speaker, out ar);
        }

        /// <summary>Drop the cached table (e.g. after a rebuild or a test).</summary>
        public static void Invalidate() { _table = null; _loaded = false; }
    }
}
