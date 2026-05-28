// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / Localization / RtlLayoutMirror
//
// Phase 60 — Arabic Localization MVP.
//
// Attach to a UI GameObject hosting a HorizontalLayoutGroup,
// HorizontalOrVerticalLayoutGroup, or any RectTransform whose anchored
// X-direction needs to mirror when the active locale is right-to-left.
//
// What it does on LocaleChangedEvent:
//   1. Flips `HorizontalLayoutGroup.reverseArrangement` so child order is
//      visually reversed (the first child renders on the right in RTL).
//   2. Mirrors the anchored RectTransform.pivot and anchors along X so a
//      panel pinned to the screen's left edge in LTR is pinned to the
//      right edge in RTL.
//   3. Optionally calls Refresh() on every child LocalizedText so embedded
//      labels re-align in the same frame.
//
// Designer ergonomics:
//   • Add to the parent of a control row (e.g. the Save Slot row, the
//     Dialogue Choice container, the Settings tab strip).
//   • For a label that should ALWAYS center-align even in RTL, set the
//     child LocalizedText's `mirrorAlignment = false` — RtlLayoutMirror
//     doesn't touch text alignment, only structural layout.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HearthboundHollow.Core;

namespace HearthboundHollow.UI
{
    [DisallowMultipleComponent]
    public class RtlLayoutMirror : MonoBehaviour
    {
        [Tooltip("Flip child order on RTL? Use for horizontal layout groups " +
                 "(e.g. Save Slot row, Dialogue Choice strip, Settings tab " +
                 "bar). Vertical groups should leave this OFF.")]
        public bool reverseHorizontalLayout = true;

        [Tooltip("Mirror the anchored X of this RectTransform on RTL? Use " +
                 "for side-pinned panels (e.g. the ControlHintsHUD parchment " +
                 "chips at bottom-left — they move to bottom-right in RTL).")]
        public bool mirrorAnchors = false;

        // Cache the LTR-default values so we restore cleanly when locale
        // flips back to English.
        private bool _captured;
        private bool _origReverseArrangement;
        private Vector2 _origAnchorMin;
        private Vector2 _origAnchorMax;
        private Vector2 _origPivot;
        private Vector2 _origAnchoredPos;

        private HorizontalOrVerticalLayoutGroup _group;
        private RectTransform _rt;

        private void Awake()
        {
            _group = GetComponent<HorizontalOrVerticalLayoutGroup>();
            _rt    = transform as RectTransform;
            Capture();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<LocaleChangedEvent>(OnLocaleChanged);
            Apply();
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<LocaleChangedEvent>(OnLocaleChanged);
        }

        private void OnLocaleChanged(LocaleChangedEvent _) => Apply();

        private void Capture()
        {
            if (_captured) return;
            if (_group != null) _origReverseArrangement = _group.reverseArrangement;
            if (_rt != null)
            {
                _origAnchorMin   = _rt.anchorMin;
                _origAnchorMax   = _rt.anchorMax;
                _origPivot       = _rt.pivot;
                _origAnchoredPos = _rt.anchoredPosition;
            }
            _captured = true;
        }

        /// <summary>
        /// Apply (or revert) the RTL mirror based on the active locale.
        /// Idempotent. Safe to call from custom scene directors.
        /// </summary>
        public void Apply()
        {
            Capture();

            var loc = ServiceLocator.Get<LocalizationService>();
            bool rtl = loc != null && loc.IsRightToLeft;

            if (reverseHorizontalLayout && _group != null)
            {
                _group.reverseArrangement = rtl ? !_origReverseArrangement : _origReverseArrangement;
            }

            if (mirrorAnchors && _rt != null)
            {
                if (rtl)
                {
                    _rt.anchorMin = new Vector2(1f - _origAnchorMax.x, _origAnchorMin.y);
                    _rt.anchorMax = new Vector2(1f - _origAnchorMin.x, _origAnchorMax.y);
                    _rt.pivot     = new Vector2(1f - _origPivot.x, _origPivot.y);
                    _rt.anchoredPosition = new Vector2(-_origAnchoredPos.x, _origAnchoredPos.y);
                }
                else
                {
                    _rt.anchorMin        = _origAnchorMin;
                    _rt.anchorMax        = _origAnchorMax;
                    _rt.pivot            = _origPivot;
                    _rt.anchoredPosition = _origAnchoredPos;
                }
            }

            // Belt-and-braces: refresh any LocalizedText children so the
            // alignment mirror lands in the same frame as the layout flip.
            var labels = GetComponentsInChildren<LocalizedText>(includeInactive: true);
            for (int i = 0; i < labels.Length; i++) labels[i].Refresh();
        }
    }
}
