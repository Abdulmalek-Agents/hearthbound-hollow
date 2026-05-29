// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / ControlHintsHUD
//
// PHASE 30 — Persistent context-aware control hints.
//
// The HelpOverlayUI is one-page-on-demand (H key). The OnboardingOverlay is
// once-per-save walkthrough. Neither one is *always* on screen. This HUD is
// the third leg: a tiny strip of key-chips at the bottom-left of the screen
// that's *always* visible and *context-aware* — when the player is near an
// interactable, the [E] chip becomes prominent. Otherwise it shows a generic
// move / interact / help hint and dims to a non-distracting alpha.
//
// ── Phase 60 — Arabic Localization MVP ──────────────────────────
// Chip captions ("🚶 Move", "✋ Interact", "❓ Help") come from
// loc.<iso>.json so both English ("🚶 Move") and Arabic ("🚶 تحرّك")
// see the same warm cozy-emoji visuals. Key glyphs (WASD / E / H)
// stay Latin in both locales because they're physical keyboard keys.
// Interactable PromptText is also routed through HasKey lookup.
//
// ASMDEF NOTE: lives in `HearthboundHollow.Mission` (not UI) because it
// queries `HearthboundHollow.Player.PlayerController.CurrentFocus`. UI
// asmdef cannot see Player; Mission can. (D-035 — asmdef-locality.)

using TMPro;
using UnityEngine;
using HearthboundHollow.Core;
using HearthboundHollow.Player;
using HearthboundHollow.UI;

namespace HearthboundHollow.Mission
{
    [DisallowMultipleComponent]
    public class ControlHintsHUD : MonoBehaviour
    {
        [Header("Root")]
        [Tooltip("Visual root toggled by SetVisible. Should be a child of the " +
                 "script-host so Update() keeps running while hidden.")]
        public GameObject root;

        [Header("Chip rows (3 default chips: Move / Interact / Help)")]
        public TextMeshProUGUI chipMoveLabel;
        public TextMeshProUGUI chipMoveCaption;
        public TextMeshProUGUI chipInteractLabel;
        public TextMeshProUGUI chipInteractCaption;
        public TextMeshProUGUI chipHelpLabel;
        public TextMeshProUGUI chipHelpCaption;

        [Header("Behaviour")]
        public CanvasGroup canvasGroup;
        [Range(0f, 1f)] public float idleAlpha = 0.45f;
        [Range(0f, 1f)] public float emphasisAlpha = 1f;
        [Range(2f, 20f)] public float alphaLerp = 8f;
        public Color emphasisColor = new Color(0.97f, 0.85f, 0.62f, 1f);
        public Color idleColor = new Color(0.78f, 0.72f, 0.62f, 1f);

        public void SetVisible(bool visible)
        {
            if (root != null) root.SetActive(visible);
        }

        private PlayerController _player;
        private float _currentAlpha;
        private string _cachedInteractDefault = "✋ Interact";

        private void Awake()
        {
            UIAutoFitText.ApplyToButtonLabel(chipMoveLabel,        minSize: 14, maxSize: 26);
            UIAutoFitText.ApplyToLabel(chipMoveCaption,             minSize: 10, maxSize: 16);
            UIAutoFitText.ApplyToButtonLabel(chipInteractLabel,    minSize: 14, maxSize: 26);
            UIAutoFitText.ApplyToLabel(chipInteractCaption,         minSize: 10, maxSize: 16);
            UIAutoFitText.ApplyToButtonLabel(chipHelpLabel,        minSize: 14, maxSize: 26);
            UIAutoFitText.ApplyToLabel(chipHelpCaption,             minSize: 10, maxSize: 16);

            // Phase 60 — localized chip captions (cozy emoji captions from
            // Phase 32.20 come through the loc.<iso>.json table now —
            // 'hud.chip.move' = '🚶 Move' / '🚶 تحرّك', etc.). Refreshed
            // on every locale change. The key glyphs (WASD / E / H) stay
            // in Latin in both locales because they're physical keys.
            ApplyLocalization();

            _currentAlpha = idleAlpha;
            if (canvasGroup != null) canvasGroup.alpha = _currentAlpha;
        }

        private void OnEnable()
        {
            _player = null;
            EventBus.Subscribe<LocaleChangedEvent>(OnLocaleChanged);
            ApplyLocalization();
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<LocaleChangedEvent>(OnLocaleChanged);
        }

        private void OnLocaleChanged(LocaleChangedEvent _) => ApplyLocalization();

        /// <summary>
        /// Phase 60 — Pull localized text for each chip. The interact-caption
        /// gets overwritten by Update() when a focused interactable's
        /// PromptText is in range — we cache the idle default here so the
        /// Update() restore-path lands the locale-correct string.
        /// </summary>
        private void ApplyLocalization()
        {
            var loc = ServiceLocator.Get<LocalizationService>();
            // Latin key glyphs stay verbatim in both locales (keyboard keys).
            if (chipMoveLabel != null)     chipMoveLabel.text = "WASD";
            if (chipInteractLabel != null) chipInteractLabel.text = "E";
            if (chipHelpLabel != null)     chipHelpLabel.text = "H";

            string moveCap     = loc != null ? loc.Get("hud.chip.move")     : "🚶 Move";
            string interactCap = loc != null ? loc.Get("hud.chip.interact") : "✋ Interact";
            string helpCap     = loc != null ? loc.Get("hud.chip.help")     : "❓ Help";
            bool rtl = loc != null && loc.IsRightToLeft;

            if (chipMoveCaption != null)
            {
                chipMoveCaption.text = rtl ? ArabicTextShaper.Shape(moveCap) : moveCap;
                chipMoveCaption.isRightToLeftText = rtl;
            }
            if (chipInteractCaption != null)
            {
                chipInteractCaption.text = rtl ? ArabicTextShaper.Shape(interactCap) : interactCap;
                chipInteractCaption.isRightToLeftText = rtl;
            }
            if (chipHelpCaption != null)
            {
                chipHelpCaption.text = rtl ? ArabicTextShaper.Shape(helpCap) : helpCap;
                chipHelpCaption.isRightToLeftText = rtl;
            }
            _cachedInteractDefault = chipInteractCaption != null ? chipInteractCaption.text : "✋ Interact";
        }

        private void Update()
        {
            if (_player == null)
            {
                var go = GameObject.FindGameObjectWithTag("Player");
                _player = go != null ? go.GetComponent<PlayerController>() : null;
            }

            bool emphasise = false;
            bool emphasiseInteract = false;

            if (_player != null)
            {
                // Emphasise the [E] chip when an interactable is in range.
                if (_player.CurrentFocus != null && _player.CurrentFocus.IsInteractable)
                {
                    emphasiseInteract = true;
                    emphasise = true;
                    if (chipInteractCaption != null && _player.CurrentFocus.PromptText != null)
                    {
                        // Phase 60 — Translate + shape the interactable's
                        // prompt for Arabic when the active locale is RTL.
                        // Interactables wired in Phase 60+ pass localization
                        // keys directly; legacy callers pass English source
                        // which is preserved through HasKey-not-found path.
                        var loc = ServiceLocator.Get<LocalizationService>();
                        string prompt = _player.CurrentFocus.PromptText;
                        if (loc != null && loc.HasKey(prompt)) prompt = loc.Get(prompt);
                        if (loc != null && loc.IsRightToLeft)
                            prompt = ArabicTextShaper.Shape(prompt);
                        chipInteractCaption.text = prompt;
                    }
                }
                else if (chipInteractCaption != null && chipInteractCaption.text != _cachedInteractDefault)
                {
                    chipInteractCaption.text = _cachedInteractDefault;
                }
            }

            // Animate alpha toward emphasis when needed.
            float target = emphasise ? emphasisAlpha : idleAlpha;
            _currentAlpha = Mathf.Lerp(_currentAlpha, target, alphaLerp * Time.unscaledDeltaTime);
            if (canvasGroup != null) canvasGroup.alpha = _currentAlpha;

            // Tint per-chip captions based on context.
            if (chipInteractCaption != null) chipInteractCaption.color = emphasiseInteract ? emphasisColor : idleColor;
            if (chipMoveCaption != null)     chipMoveCaption.color     = idleColor;
            if (chipHelpCaption != null)     chipHelpCaption.color     = idleColor;
        }
    }
}
