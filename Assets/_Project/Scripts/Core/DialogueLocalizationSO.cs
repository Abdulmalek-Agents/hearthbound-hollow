// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Core / DialogueLocalizationSO
//
// PHASE 57 (D-074) — Arabic dialogue-text PLUMBING (not content).
//
// Maps a dialogue lineId (e.g. "doris_m1_greet_01" — the same stable id the
// directors pass to DialogueUI.PresentLine and VoicePlayer.Play) to its Arabic
// translation. This asset ships EMPTY: per Pillar 1 / D-065 the Arabic prose is
// a HUMAN translation pass — a translator fills these entries (or a CSV importer
// populates them). When a lineId has Arabic text AND the language is Arabic,
// DialogueUI shows it (ArabicShaper-shaped); otherwise it falls back to the
// canonical English line. No AI-authored dialogue ever lives here.
//
// Loaded at runtime via Resources.Load<DialogueLocalizationSO>("DialogueLocalization_ar").
// Created (empty) + kept by Phase57_ArabicLocalizationScaffold.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace HearthboundHollow.Core
{
    [CreateAssetMenu(menuName = "Hearthbound/Localization/Dialogue Arabic Table",
                     fileName = "DialogueLocalization_ar")]
    public class DialogueLocalizationSO : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            [Tooltip("Stable line id — matches the directors' PresentLine/Play lineId " +
                     "(e.g. \"doris_m1_greet_01\").")]
            public string lineId;

            [TextArea]
            [Tooltip("Arabic translation in LOGICAL order (normal typing). It is " +
                     "contextually joined + RTL-ordered at display by ArabicShaper — " +
                     "do NOT pre-shape it here.")]
            public string text;
        }

        [Tooltip("lineId → Arabic translation. Filled by a human translator (Pillar 1). " +
                 "Empty = the line stays canonical English.")]
        public Entry[] entries;

        private Dictionary<string, string> _map;

        /// <summary>True + Arabic text when this lineId has a non-empty translation.</summary>
        public bool TryGet(string lineId, out string text)
        {
            text = null;
            if (string.IsNullOrEmpty(lineId)) return false;
            if (_map == null)
            {
                _map = new Dictionary<string, string>();
                if (entries != null)
                    foreach (var e in entries)
                        if (!string.IsNullOrEmpty(e.lineId) && !string.IsNullOrEmpty(e.text))
                            _map[e.lineId] = e.text;
            }
            return _map.TryGetValue(lineId, out text) && !string.IsNullOrEmpty(text);
        }
    }
}
