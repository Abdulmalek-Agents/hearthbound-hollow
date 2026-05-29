// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / ToneCompassCard
//
// First-launch 90-second content + tone primer. Per Focus 07 § 7 + Codex 06.
// Mandatory: skippable from frame 1, Gentle Mode prompt visible.
//
// ── Phase 60 — Arabic Localization MVP ──────────────────────────
// The 6-paragraph primer + Continue button + Gentle Mode label come from
// loc.<iso>.json via LocalizationService. Falls back to `defaultBody`
// when the service isn't yet registered (e.g. EditMode test).

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
        [Tooltip("Canonical 6-paragraph Tone Compass primer (English fallback " +
                 "when LocalizationService isn't registered). Phase 60 reads " +
                 "the runtime text from `tone_compass.body` in loc.<iso>.json.")]
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
            if (root != null && root != gameObject) root.SetActive(false);

            if (continueButton != null) continueButton.onClick.AddListener(Acknowledge);
            if (gentleModeToggle != null)
                gentleModeToggle.onValueChanged.AddListener(OnGentleToggleChanged);

            UIReadabilityHelper.ApplyBody       (bodyText,        min: 22, max: 34);
            UIReadabilityHelper.ApplyButtonLabel(gentleModeLabel, min: 18, max: 26);
            if (bodyText != null) UIReadabilityHelper.AddDarkWash(bodyText.rectTransform, padding: 16f);
        }

        public void Show()
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            if (root != null && !root.activeSelf) root.SetActive(true);

            // Phase 60 — Localized 6-paragraph primer + Continue button label.
            var loc = ServiceLocator.Get<LocalizationService>();
            bool rtl = loc != null && loc.IsRightToLeft;
            string body = loc != null ? loc.Get("tone_compass.body") : defaultBody;

            if (bodyText != null)
            {
                bodyText.text = rtl ? ArabicTextShaper.Shape(body) : body;
                bodyText.isRightToLeftText = rtl;
                bodyText.alignment = rtl
                    ? TMPro.TextAlignmentOptions.TopRight
                    : TMPro.TextAlignmentOptions.TopLeft;
            }

            if (continueButton != null && loc != null)
            {
                var lbl = continueButton.GetComponentInChildren<TextMeshProUGUI>();
                if (lbl != null)
                {
                    string s = loc.Get("tone_compass.cta.continue");
                    lbl.text = rtl ? ArabicTextShaper.Shape(s) : s;
                    lbl.isRightToLeftText = rtl;
                }
            }

            var vs = ServiceLocator.Get<VillageState>();
            if (vs != null && gentleModeToggle != null)
                gentleModeToggle.isOn = vs.gentleModeEnabled;

            if (continueButton != null) continueButton.interactable = false;
            if (gameObject.activeInHierarchy && isActiveAndEnabled)
            {
                StartCoroutine(EnableSkipAfterFrame());
            }
            else
            {
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
            // Phase 60 — Localized "Gentle Mode: ON / off" label.
            if (gentleModeLabel != null)
            {
                var loc = ServiceLocator.Get<LocalizationService>();
                string s = loc != null
                    ? loc.Get(isOn ? "tone_compass.gentle_label.on" : "tone_compass.gentle_label.off")
                    : (isOn ? "Gentle Mode: ON" : "Gentle Mode: off");
                bool rtl = loc != null && loc.IsRightToLeft;
                gentleModeLabel.text = rtl ? ArabicTextShaper.Shape(s) : s;
                gentleModeLabel.isRightToLeftText = rtl;
            }
        }

        private void Acknowledge()
        {
            var vs = ServiceLocator.Get<VillageState>();
            if (vs != null) vs.toneCompassAcknowledged = true;
            if (root != null && root != gameObject) root.SetActive(false);
            gameObject.SetActive(false);
            OnAcknowledged?.Invoke();
            Hh.Log(LogCategory.UI, "Tone Compass acknowledged.");
        }
    }
}
