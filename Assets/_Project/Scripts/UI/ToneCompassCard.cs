// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / ToneCompassCard
//
// First-launch 90-second content + tone primer. Per Focus 07 § 7 + Codex 06.
// Mandatory: skippable from frame 1, Gentle Mode prompt visible.

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
            if (root != null) root.SetActive(false);
            if (continueButton != null) continueButton.onClick.AddListener(Acknowledge);
            if (gentleModeToggle != null)
                gentleModeToggle.onValueChanged.AddListener(OnGentleToggleChanged);
        }

        public void Show()
        {
            if (root != null) root.SetActive(true);
            if (bodyText != null) bodyText.text = defaultBody;
            var vs = ServiceLocator.Get<VillageState>();
            if (vs != null && gentleModeToggle != null) gentleModeToggle.isOn = vs.gentleModeEnabled;
            StartCoroutine(EnableSkipAfterFrame());
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
            if (root != null) root.SetActive(false);
            OnAcknowledged?.Invoke();
            Hh.Log(LogCategory.UI, "Tone Compass acknowledged.");
        }
    }
}
