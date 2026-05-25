// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / UIAutoFitText
//
// PHASE 29 — Universal "text never gets cut off" helper.
//
// User report on first playtest (after Phase 27 build):
//   "the cards and UI is not appearing well and text is cut off"
//
// ROOT CAUSE — three classes of bug in Phase 14/22/23 UI builders:
//
//   1. Dialogue / Help / Pause / Ledger TMP labels were created without
//      `enableWordWrapping = true`, so multi-line strings overflowed
//      the panel horizontally and clipped at the canvas mask edge.
//
//   2. Several fixed `fontSize` values (24, 26, 30) were larger than
//      the parent RectTransform could hold at 16:9 with the panel
//      padding the builder applied. On a 1080p canvas the right-edge
//      letters disappeared into the parchment border.
//
//   3. The DialogueBox's `ChoicesContainer` was placed at anchorMin.y =
//      1.05 and anchorMax.y = 2.10 — i.e. ABOVE the parent prefab
//      bounds. The scene builder was supposed to reposition it but in
//      practice the anchors stuck and the choice scrolls rendered off-
//      screen above the dialogue box.
//
// FIX — attach this component to any TMP label. On Awake (and again
// after any text content change) it:
//   • Forces `enableWordWrapping = true` if multi-line content is
//     present, so the text always wraps inside the rect.
//   • If `enableAutoSize` is on, configures `fontSizeMin`/`fontSizeMax`
//     and `enableAutoSizing = true` so the engine shrinks the text to
//     fit. The `fontSize` field then becomes the *target* size.
//   • Adds an `OverflowMode = Ellipsis` fallback when auto-size still
//     can't fit (e.g. extremely tiny rects).
//
// USAGE — drop the component on any TextMeshProUGUI rect that's at risk
// of clipping, or call `UIAutoFitText.Apply(label, opts)` statically
// from a builder script. The Phase 29 capstone (Phase29_UIPolish) walks
// every UI prefab and applies sane defaults via Apply().

using TMPro;
using UnityEngine;

namespace HearthboundHollow.UI
{
    /// <summary>
    /// Configures a TextMeshProUGUI for guaranteed-no-clipping behaviour:
    /// word-wrap + auto-size between fontSizeMin and fontSizeMax + ellipsis
    /// fallback. Idempotent. Lightweight (no per-frame work).
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TextMeshProUGUI))]
    [DefaultExecutionOrder(-30)]
    public class UIAutoFitText : MonoBehaviour
    {
        [Header("Word wrap")]
        [Tooltip("Force `enableWordWrapping = true` on the TextMeshProUGUI.")]
        public bool forceWordWrap = true;

        [Header("Auto size")]
        [Tooltip("Enable TMP auto-size. Highly recommended for any UI text " +
                 "that comes from a dialogue / villager string source.")]
        public bool enableAutoSize = true;

        [Tooltip("Minimum permitted font size when auto-shrinking. Anything " +
                 "smaller than this would be illegible.")]
        [Range(8f, 48f)]
        public float fontSizeMin = 14f;

        [Tooltip("Maximum permitted font size — typically the original design " +
                 "intent. The text will auto-grow up to this if there's room.")]
        [Range(10f, 96f)]
        public float fontSizeMax = 28f;

        [Header("Overflow fallback")]
        [Tooltip("Overflow mode applied when even fontSizeMin can't fit. " +
                 "Ellipsis (…) is the cozy-game-friendly choice.")]
        public TextOverflowModes overflowMode = TextOverflowModes.Ellipsis;

        [Header("Margins")]
        [Tooltip("Optional extra inset (pixels) added to the TMP `margin` " +
                 "field. Useful when the text rect is sized to a parchment " +
                 "sprite that has decorative borders.")]
        public Vector4 extraMargin = Vector4.zero;

        private TextMeshProUGUI _tmp;

        private void Awake()
        {
            _tmp = GetComponent<TextMeshProUGUI>();
            Apply(_tmp);
        }

        private void OnValidate()
        {
            if (_tmp == null) _tmp = GetComponent<TextMeshProUGUI>();
            Apply(_tmp);
        }

        /// <summary>
        /// Public re-apply hook. Call after changing the text content if
        /// the new content is significantly longer than the old.
        /// </summary>
        public void Apply()
        {
            if (_tmp == null) _tmp = GetComponent<TextMeshProUGUI>();
            Apply(_tmp);
        }

        private void Apply(TextMeshProUGUI tmp)
        {
            if (tmp == null) return;

            if (forceWordWrap) tmp.enableWordWrapping = true;

            if (enableAutoSize)
            {
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = fontSizeMin;
                tmp.fontSizeMax = fontSizeMax;
                // Preserve the designer's font size if it sits inside the
                // [min, max] range — TMP treats fontSize as the *upper bound*
                // for the auto-sizer.
                if (tmp.fontSize < fontSizeMin || tmp.fontSize > fontSizeMax)
                    tmp.fontSize = fontSizeMax;
            }

            tmp.overflowMode = overflowMode;

            if (extraMargin != Vector4.zero) tmp.margin += extraMargin;
        }

        // ───── Static helpers for builders ─────────────────────────

        /// <summary>
        /// Configure a TMP label for guaranteed-no-clipping behaviour. Can be
        /// called from any editor / runtime builder without forcing the
        /// component to be added (lighter than attaching UIAutoFitText
        /// everywhere). Returns the same label for chaining.
        /// </summary>
        public static TextMeshProUGUI ApplyToLabel(TextMeshProUGUI tmp,
            float minSize = 14f, float maxSize = 28f,
            TextOverflowModes overflow = TextOverflowModes.Ellipsis,
            bool wordWrap = true)
        {
            if (tmp == null) return null;
            tmp.enableWordWrapping = wordWrap;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = minSize;
            tmp.fontSizeMax = maxSize;
            if (tmp.fontSize < minSize || tmp.fontSize > maxSize) tmp.fontSize = maxSize;
            tmp.overflowMode = overflow;
            return tmp;
        }

        /// <summary>
        /// Convenience for short labels (button captions etc.) where wrap
        /// makes layouts wobble. Single-line + ellipsis on overflow.
        /// </summary>
        public static TextMeshProUGUI ApplyToButtonLabel(TextMeshProUGUI tmp,
            float minSize = 14f, float maxSize = 24f)
        {
            if (tmp == null) return null;
            tmp.enableWordWrapping = false;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = minSize;
            tmp.fontSizeMax = maxSize;
            if (tmp.fontSize < minSize || tmp.fontSize > maxSize) tmp.fontSize = maxSize;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            return tmp;
        }
    }
}
