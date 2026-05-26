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
//
// ── Gameplay-Guide compliance pass (commit 5/10) ────────────────
// QA audit found the default body text was a generic placeholder ("Welcome
// to the Hollow. This is a quiet game about memory...") rather than the
// canonical 6-paragraph primer specified in GAMEPLAY_GUIDE_OVERVIEW.md § 7.1
// and Mission 1 Guide § 6.1. Author: Pell Doyne (Cozy Mechanics & Comfort
// Loop Engineer). The primer is the single highest-ROI text in M1-2 for
// refund-rate prevention (Codex 06 § 4.3 — Spiritfarer refund-rate lesson).

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
        [TextArea(8, 20)]
        [Tooltip("The canonical 6-paragraph Tone Compass primer per Focus 07 § 7.1 " +
                 "and GAMEPLAY_GUIDE_OVERVIEW.md § 7.1. The single highest-ROI text " +
                 "in M1-2 for Steam-refund-rate prevention. Do NOT edit without a " +
                 "Pell Doyne sign-off.")]
        public string defaultBody =
            "This game will make you feel things. Some of those feelings are heavy.\n\n" +
            "This first hour contains: the opening of a shop, a first transaction, a late-night brewing.\n\n" +
            "The second hour contains: a widower's grief, a choice about memory, a short illustrated dream.\n\n" +
            "At any point, you can take a Soft Day, enable Gentle Mode, or adjust any settings.\n\n" +
            "There is no combat. There are no failure screens. There are only choices.\n\n" +
            "The cat will be there.";

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

            // Phase 29 — defensive autofit so the 6-paragraph tone primer
            // never overflows on smaller canvases. Critical for accessibility.
            UIAutoFitText.ApplyToLabel(bodyText, minSize: 14, maxSize: 24);
            UIAutoFitText.ApplyToLabel(gentleModeLabel, minSize: 12, maxSize: 20);
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
