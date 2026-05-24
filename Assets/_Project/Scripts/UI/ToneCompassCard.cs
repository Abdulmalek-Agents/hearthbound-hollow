// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / ToneCompassCard
//
// First-launch 90-second content + tone primer. Per Focus 07 § 7 + Codex 06.
// Mandatory: skippable from frame 1, Gentle Mode prompt visible.
//
// ── Phase 25 hotfix ─────────────────────────────────────────────
// Show() previously called StartCoroutine() before ensuring the script's
// host GameObject was active in the hierarchy. When Phase 22's
// HearthboundOneClickSetup wired the script onto the same GameObject it
// then deactivated (BuildToneCompass), the result was a crash:
//
//     "Coroutine couldn't be started because the the game object
//      'ToneCompass' is inactive!"
//
// Show() now self-heals — it activates its own GameObject, activates the
// (possibly-separate) panel root, and only StartCoroutine()s when the
// host is genuinely active-in-hierarchy. A safety fallback enables the
// Continue button immediately when the coroutine path is unavailable
// (e.g. when a parent Canvas is disabled), so the user is never trapped
// in an un-skippable modal. Behaviour-equivalent for the normal case.

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HearthboundHollow.Core;

namespace HearthboundHollow.UI
{
    public class ToneCompassCard : MonoBehaviour
    {
        [Header("Root")]
        [Tooltip("The visible panel. May be the same GameObject as this script, " +
                 "or a child GameObject. Show() activates whichever it is.")]
        public GameObject root;

        [Header("Card content")]
        public TextMeshProUGUI bodyText;

        [Header("Controls")]
        public Button continueButton;
        public Button gentleModeToggleButton;
        public Toggle gentleModeToggle;
        public TextMeshProUGUI gentleModeLabel;

        [Header("Defaults")]
        [TextArea(6, 14)]
        public string defaultBody =
            "Welcome to the Hollow.\n\n" +
            "This is a quiet game about memory, choice, and care.\n" +
            "There is no combat. There are no failure screens. There are only choices.\n\n" +
            "Some scenes are heavy. Some are warm. The pace is yours.\n" +
            "Tools at the corner of the screen let you auto-complete any mini-game.\n" +
            "Gentle Mode softens the harder moments. You can toggle it any time.";

        public event System.Action OnAcknowledged;

        private void Awake()
        {
            // Hide the visual panel by default. We deliberately do NOT touch
            // gameObject.activeSelf here — the script-host MUST remain active
            // so Show() can run a coroutine when called.
            if (root != null && root != gameObject) root.SetActive(false);

            if (continueButton != null) continueButton.onClick.AddListener(Acknowledge);
            if (gentleModeToggle != null)
                gentleModeToggle.onValueChanged.AddListener(OnGentleToggleChanged);
        }

        public void Show()
        {
            // Self-heal: the script's own GameObject MUST be active before we
            // can StartCoroutine. Phase 22's procedural builder previously
            // deactivated this GameObject after wiring `root`, which broke
            // the coroutine path. Re-activate defensively here.
            if (!gameObject.activeSelf) gameObject.SetActive(true);

            // Then activate the visual panel (might be the same GameObject).
            if (root != null && !root.activeSelf) root.SetActive(true);

            if (bodyText != null) bodyText.text = defaultBody;

            var vs = ServiceLocator.Get<VillageState>();
            if (vs != null && gentleModeToggle != null)
                gentleModeToggle.isOn = vs.gentleModeEnabled;

            // Disable the Continue button for one frame so the player can't
            // skip the card without seeing it — but ONLY when we can actually
            // run a coroutine. Otherwise enable immediately to avoid trapping.
            if (continueButton != null) continueButton.interactable = false;
            if (gameObject.activeInHierarchy && isActiveAndEnabled)
            {
                StartCoroutine(EnableSkipAfterFrame());
            }
            else
            {
                // Defensive fallback — parent canvas may be disabled. Don't
                // trap the player in an un-skippable modal.
                if (continueButton != null) continueButton.interactable = true;
                Hh.Warn(LogCategory.UI,
                    "ToneCompassCard.Show called while inactive-in-hierarchy. " +
                    "Continue enabled immediately (no skip-grace).");
            }
        }

        private IEnumerator EnableSkipAfterFrame()
        {
            yield return null;
            if (continueButton != null) continueButton.interactable = true;
        }

        private void OnGentleToggleChanged(bool isOn)
        {
            var vs = ServiceLocator.Get<VillageState>();
            if (vs != null) vs.gentleModeEnabled = isOn;
            if (gentleModeLabel != null) gentleModeLabel.text = isOn ? "Gentle Mode: ON" : "Gentle Mode: off";
        }

        private void Acknowledge()
        {
            var vs = ServiceLocator.Get<VillageState>();
            if (vs != null) vs.toneCompassAcknowledged = true;
            if (root != null && root != gameObject) root.SetActive(false);
            // Always hide self too — the card lives on a child of the main
            // menu canvas and should not steal raycasts once acknowledged.
            // (Doesn't deactivate forever; OnOpenTheHollow only calls Show
            //  once per session and the menu unloads on scene change.)
            gameObject.SetActive(false);
            OnAcknowledged?.Invoke();
            Hh.Log(LogCategory.UI, "Tone Compass acknowledged.");
        }
    }
}
