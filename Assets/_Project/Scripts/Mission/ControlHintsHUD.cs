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
// Cozy framing:
//   • Default alpha is 0.45 — readable but not loud.
//   • Highlight color matches the warm parchment palette (no jarring blue).
//   • Tooltip text auto-shrinks via UIAutoFitText.
//   • Players who find it noisy can disable it via Settings → "Show control hints".
//
// ASMDEF NOTE: lives in `HearthboundHollow.Mission` (not UI) because it
// queries `HearthboundHollow.Player.PlayerController.CurrentFocus`. UI
// asmdef cannot see Player; Mission can. (D-035 — asmdef-locality.)
//
// USAGE — drop on the same Canvas that hosts the Pause / Help overlays. The
// Phase 30 capstone wires it onto every gameplay scene (Lane, Hollow,
// Garden, Cottage).

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
        [Tooltip("Read alpha from this CanvasGroup. The HUD modulates it based " +
                 "on context.")]
        public CanvasGroup canvasGroup;

        [Tooltip("Resting alpha when no specific hint is active.")]
        [Range(0f, 1f)]
        public float idleAlpha = 0.45f;

        [Tooltip("Alpha when a context-specific hint (interactable in range) " +
                 "is being emphasised.")]
        [Range(0f, 1f)]
        public float emphasisAlpha = 1f;

        [Tooltip("Lerp speed between idle and emphasis alphas.")]
        [Range(2f, 20f)]
        public float alphaLerp = 8f;

        [Tooltip("Tint applied to the highlighted chip caption.")]
        public Color emphasisColor = new Color(0.97f, 0.85f, 0.62f, 1f);

        [Tooltip("Tint applied to non-highlighted chip captions.")]
        public Color idleColor = new Color(0.78f, 0.72f, 0.62f, 1f);

        // ───── Public API ────────────────────────────────────────

        public void SetVisible(bool visible)
        {
            if (root != null) root.SetActive(visible);
        }

        // ───── Internals ─────────────────────────────────────────

        private PlayerController _player;
        private float _currentAlpha;
        private string _interactDefault = "Interact";

        // ───── Lifecycle ─────────────────────────────────────────

        private void Awake()
        {
            // Phase 29 — autofit every TMP label so chip text never clips.
            UIAutoFitText.ApplyToButtonLabel(chipMoveLabel,        minSize: 14, maxSize: 26);
            UIAutoFitText.ApplyToLabel(chipMoveCaption,             minSize: 10, maxSize: 16);
            UIAutoFitText.ApplyToButtonLabel(chipInteractLabel,    minSize: 14, maxSize: 26);
            UIAutoFitText.ApplyToLabel(chipInteractCaption,         minSize: 10, maxSize: 16);
            UIAutoFitText.ApplyToButtonLabel(chipHelpLabel,        minSize: 14, maxSize: 26);
            UIAutoFitText.ApplyToLabel(chipHelpCaption,             minSize: 10, maxSize: 16);

            // Phase 32.20 / Phase 54 — cozy glyph captions under each key chip.
            // Glyphs route through HollowGlyphs.Format so they render as on-brand
            // gold icons in TMP (and degrade to clean text — never a tofu box).
            _interactDefault = HollowGlyphs.Format("✋ Interact");
            if (chipMoveLabel != null)     chipMoveLabel.text = "WASD";
            if (chipMoveCaption != null)   chipMoveCaption.text = HollowGlyphs.Format("🚶 Move");
            if (chipInteractLabel != null) chipInteractLabel.text = "E";
            if (chipInteractCaption != null) chipInteractCaption.text = _interactDefault;
            if (chipHelpLabel != null)     chipHelpLabel.text = "H";
            if (chipHelpCaption != null)   chipHelpCaption.text = HollowGlyphs.Format("❓ Help");

            _currentAlpha = idleAlpha;
            if (canvasGroup != null) canvasGroup.alpha = _currentAlpha;
        }

        private void OnEnable()
        {
            // Re-resolve the player every time we're enabled; the Player
            // GameObject is sometimes destroyed and re-spawned on scene change.
            _player = null;
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
                        chipInteractCaption.text = _player.CurrentFocus.PromptText;
                }
                else if (chipInteractCaption != null && chipInteractCaption.text != _interactDefault)
                {
                    chipInteractCaption.text = _interactDefault;
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
