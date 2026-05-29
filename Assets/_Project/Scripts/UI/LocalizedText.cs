// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / LocalizedText
//
// Phase 53 — binds a TextMeshProUGUI label to a LocalizationService key.
// On enable (and whenever the language changes) it pulls the localized string
// and, for right-to-left languages (Arabic), flips the paragraph alignment so
// the text reads naturally. Drop this on any chrome label the builder creates.
//
// Phase 56 (D-073) — for Arabic it also runs ArabicShaper so letters JOIN into
// real words and read right-to-left (TMP's classic path does neither). Combined
// with the Arabic fallback font from Phase56_ArabicFontInstaller this turns the
// old "tofu boxes" into correct, connected Arabic.
//
// Presentational only: UI → Core dependency (LocalizationService / ArabicShaper)
// is already in the asmdef graph; no cycle.

using TMPro;
using UnityEngine;
using HearthboundHollow.Core;

namespace HearthboundHollow.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LocalizedText : MonoBehaviour
    {
        [Tooltip("LocalizationService key, e.g. \"menu.settings\".")]
        public string key;

        [Tooltip("If set, the resolved string is wrapped/joined with this prefix " +
                 "(useful for \"Name: {value}\" style labels). Optional.")]
        public string prefix = "";

        private TextMeshProUGUI _tmp;

        private void Awake() => _tmp = GetComponent<TextMeshProUGUI>();

        private void OnEnable()
        {
            LocalizationService.OnLanguageChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            LocalizationService.OnLanguageChanged -= Refresh;
        }

        public void SetKey(string newKey)
        {
            key = newKey;
            Refresh();
        }

        public void Refresh()
        {
            if (_tmp == null) _tmp = GetComponent<TextMeshProUGUI>();
            if (_tmp == null) return;

            if (!string.IsNullOrEmpty(key))
            {
                string resolved = prefix + LocalizationService.Get(key);

                // Phase 56 (D-073): Arabic needs contextual letter-joining + RTL
                // ordering before TMP can show "real words" (TMP's classic path
                // does neither). ArabicShaper returns a display-ready, visually
                // ordered string, so we keep TMP's native RTL OFF to avoid a
                // double reversal. Non-Arabic text is returned untouched.
                if (LocalizationService.IsRightToLeft)
                    resolved = ArabicShaper.Shape(resolved);

                _tmp.isRightToLeftText = false;
                _tmp.text = resolved;
            }

            // RTL: right-align for Arabic, left for LTR. TMP renders the Unicode
            // as authored; the alignment flip is the visible cozy polish. Labels
            // authored centered are left centered.
            if (_tmp.horizontalAlignment == HorizontalAlignmentOptions.Center) return;
            _tmp.horizontalAlignment = LocalizationService.IsRightToLeft
                ? HorizontalAlignmentOptions.Right
                : HorizontalAlignmentOptions.Left;
        }
    }
}
