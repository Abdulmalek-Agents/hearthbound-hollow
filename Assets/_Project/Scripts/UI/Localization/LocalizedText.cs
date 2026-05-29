// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / Localization / LocalizedText
//
// Phase 60 — Arabic Localization MVP.
//
// Attach to any GameObject hosting a TextMeshProUGUI. On Enable, the
// component:
//
//   1. Reads its `key` field (e.g. "menu.main.cta.open_hollow").
//   2. Asks the LocalizationService for the current-locale string.
//   3. Pipes it through the ArabicTextShaper if the locale is RTL.
//   4. Sets the target TMP label's text + alignment + isRightToLeftText.
//
// Live language switching: the component subscribes to LocaleChangedEvent
// on Enable and refreshes itself when the player flips Arabic/English.
//
// Designer ergonomics:
//   • If the GameObject already has a TMP_Text the inspector picks it up.
//   • If the key is empty the original TMP text is preserved (no overwrite),
//     so adding the component is non-destructive on legacy prefabs.
//
// ── How to use on a legacy prefab ──────────────────────────────────
//   1. Add LocalizedText next to the existing TextMeshProUGUI.
//   2. Set `key` to the canonical key (see loc.en.json).
//   3. Leave `formatArgs` empty unless the key has placeholders ({0}, {1}).
//   4. Press Play. Switch language in the comfort menu — TMP updates live.

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using HearthboundHollow.Core;

namespace HearthboundHollow.UI
{
    /// <summary>
    /// Auto-localized TextMeshPro label. Re-reads its string on every
    /// <see cref="LocaleChangedEvent"/> and applies Arabic glyph shaping +
    /// RTL alignment when the active locale is RTL.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedText : MonoBehaviour
    {
        [Tooltip("Canonical localization key. e.g. 'menu.main.cta.open_hollow', " +
                 "'pause.title', 'hud.day_label_fmt'. Look up the full list in " +
                 "Assets/_Project/Localization/Resources/loc.en.json.")]
        public string key;

        [Tooltip("Optional format-string arguments. If set and the localized " +
                 "string contains {0}/{1}/... placeholders, string.Format is " +
                 "applied before shaping.")]
        public string[] formatArgs;

        [Tooltip("Mirror text alignment for RTL languages? Default ON. Turn " +
                 "OFF for labels you want kept centered (e.g. day-of-week chip).")]
        public bool mirrorAlignment = true;

        [Tooltip("If the key is empty and this is true, the original TMP text " +
                 "is left as a hardcoded fallback (no warning). Use for placeholders " +
                 "during a partial migration.")]
        public bool allowEmptyKey = true;

        private TMP_Text _tmp;
        private TextAlignmentOptions _originalAlignment;
        private bool _originalAlignmentCached;

        private void Awake()
        {
            _tmp = GetComponent<TMP_Text>();
            if (_tmp != null)
            {
                _originalAlignment = _tmp.alignment;
                _originalAlignmentCached = true;
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<LocaleChangedEvent>(OnLocaleChanged);
            Refresh();
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<LocaleChangedEvent>(OnLocaleChanged);
        }

        private void OnLocaleChanged(LocaleChangedEvent _) => Refresh();

        /// <summary>
        /// Force a re-pull of the localized text. Public so callers (e.g. a
        /// scene director that just changed `formatArgs`) can request an
        /// update without flipping the locale.
        /// </summary>
        public void Refresh()
        {
            if (_tmp == null) _tmp = GetComponent<TMP_Text>();
            if (_tmp == null) return;

            if (string.IsNullOrEmpty(key))
            {
                if (allowEmptyKey) return;
                Hh.Warn(LogCategory.UI, $"LocalizedText on '{name}' has an empty key.");
                return;
            }

            var loc = ServiceLocator.Get<LocalizationService>();
            if (loc == null)
            {
                // Bootstrap may not have run in a stand-alone EditMode scene.
                // Leave the existing text alone rather than nuking it.
                return;
            }

            string text = (formatArgs != null && formatArgs.Length > 0)
                ? loc.Format(key, BoxArgs(formatArgs))
                : loc.Get(key);

            // Pipe through Arabic shaper when the locale is RTL.
            if (loc.IsRightToLeft) text = ArabicTextShaper.Shape(text);

            _tmp.text = text;
            ApplyRtlAlignment(loc.IsRightToLeft);
        }

        /// <summary>
        /// Programmatic key change — useful for dynamic chips like the
        /// ControlHintsHUD's interact-prompt label that updates per
        /// hovered interactable.
        /// </summary>
        public void SetKey(string newKey)
        {
            if (key == newKey) return;
            key = newKey;
            Refresh();
        }

        private void ApplyRtlAlignment(bool rtl)
        {
            if (!mirrorAlignment || _tmp == null || !_originalAlignmentCached) return;

            // TMP gives us per-axis alignment options; we mirror only the
            // horizontal component. The vertical (top/middle/bottom) is
            // preserved exactly so a centered header stays centered.
            var alignment = _originalAlignment;
            if (rtl)
            {
                alignment = MirrorHorizontal(alignment);
            }
            _tmp.alignment = alignment;
            _tmp.isRightToLeftText = rtl;
        }

        private static TextAlignmentOptions MirrorHorizontal(TextAlignmentOptions a)
        {
            // TMP's TextAlignmentOptions are bitfields combining horizontal
            // + vertical anchors. We swap Left ↔ Right on each row of the
            // 4×4 anchor grid.
            switch (a)
            {
                case TextAlignmentOptions.TopLeft:        return TextAlignmentOptions.TopRight;
                case TextAlignmentOptions.TopRight:       return TextAlignmentOptions.TopLeft;
                case TextAlignmentOptions.Left:           return TextAlignmentOptions.Right;
                case TextAlignmentOptions.Right:          return TextAlignmentOptions.Left;
                case TextAlignmentOptions.BottomLeft:     return TextAlignmentOptions.BottomRight;
                case TextAlignmentOptions.BottomRight:    return TextAlignmentOptions.BottomLeft;
                case TextAlignmentOptions.BaselineLeft:   return TextAlignmentOptions.BaselineRight;
                case TextAlignmentOptions.BaselineRight:  return TextAlignmentOptions.BaselineLeft;
                case TextAlignmentOptions.MidlineLeft:    return TextAlignmentOptions.MidlineRight;
                case TextAlignmentOptions.MidlineRight:   return TextAlignmentOptions.MidlineLeft;
                case TextAlignmentOptions.CaplineLeft:    return TextAlignmentOptions.CaplineRight;
                case TextAlignmentOptions.CaplineRight:   return TextAlignmentOptions.CaplineLeft;
                default: return a;
            }
        }

        private static object[] BoxArgs(string[] args)
        {
            var boxed = new object[args.Length];
            for (int i = 0; i < args.Length; i++) boxed[i] = args[i] ?? string.Empty;
            return boxed;
        }
    }
}
