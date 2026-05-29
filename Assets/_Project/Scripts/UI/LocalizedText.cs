// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / LocalizedText
//
// Phase 53 — binds a TextMeshProUGUI label to a LocalizationService key.
// On enable (and whenever the language changes) it pulls the localized string
// and, for right-to-left languages (Arabic), flips the paragraph alignment so
// the text reads naturally. Drop this on any chrome label the builder creates.
//
// Presentational only: UI → Core dependency (LocalizationService) is already
// in the asmdef graph; no cycle.

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
                _tmp.text = prefix + LocalizationService.Get(key);

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
