// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / HelpOverlayUI
//
// Lightweight controls / help card. Shown automatically on first Mission 1
// start (when VillageState.tutorialCompleted == false) and toggled any time
// with the 'H' key. Cozy framing — no obtrusive tutorial, just a one-page
// reference card.
//
// Content varies by control scheme detected at runtime: Keyboard+Mouse,
// Gamepad, or Touch.

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HearthboundHollow.Core;

namespace HearthboundHollow.UI
{
    public class HelpOverlayUI : MonoBehaviour
    {
        [Header("Root")]
        public GameObject root;

        [Header("Header")]
        public TextMeshProUGUI titleLabel;
        public TextMeshProUGUI subtitleLabel;

        [Header("Body")]
        public TextMeshProUGUI bodyText;

        [Header("Close")]
        public Button closeButton;

        [Header("Behaviour")]
        public KeyCode toggleKey = KeyCode.H;
        [Tooltip("If true, automatically shows on Start when tutorialCompleted == false.")]
        public bool autoShowOnFirstPlay = true;
        [Tooltip("If true, marks tutorialCompleted = true on first close so it never auto-shows again.")]
        public bool markTutorialCompletedOnClose = true;

        public bool IsOpen { get; private set; }

        private void Awake()
        {
            if (root != null) root.SetActive(false);
            if (closeButton != null) closeButton.onClick.AddListener(Hide);
        }

        private void Start()
        {
            ApplyContent();
            if (autoShowOnFirstPlay)
            {
                var vs = ServiceLocator.Get<VillageState>();
                if (vs != null && !vs.tutorialCompleted) Show();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                if (IsOpen) Hide();
                else Show();
            }
        }

        public void Show()
        {
            if (root != null) root.SetActive(true);
            ApplyContent();
            IsOpen = true;
            Hh.Log(LogCategory.UI, "Help overlay opened.");
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
            IsOpen = false;

            if (markTutorialCompletedOnClose)
            {
                var vs = ServiceLocator.Get<VillageState>();
                if (vs != null && !vs.tutorialCompleted)
                {
                    vs.tutorialCompleted = true;
                    Hh.Log(LogCategory.UI, "Tutorial marked complete (Help overlay closed).");
                }
            }
        }

        private void ApplyContent()
        {
            if (titleLabel != null && string.IsNullOrEmpty(titleLabel.text))
                titleLabel.text = "Welcome to the Hollow";
            if (subtitleLabel != null && string.IsNullOrEmpty(subtitleLabel.text))
                subtitleLabel.text = "A quick word from Marin's notes …";

            if (bodyText != null && string.IsNullOrEmpty(bodyText.text))
            {
                bodyText.text =
                    "<b>Move</b>      WASD / Arrow Keys / Left Stick\n" +
                    "<b>Interact</b>  E / Gamepad ▢\n" +
                    "<b>Advance</b>   Click / Space / Enter\n" +
                    "<b>Polish</b>    Hold left mouse, draw slow circles\n" +
                    "                — cover all sides of the orb\n" +
                    "                — slower is better\n" +
                    "<b>Help</b>      H to toggle this card\n" +
                    "<b>Pause</b>     Esc\n" +
                    "\n" +
                    "<i>\"There is no wrong way to keep a memory.</i>\n" +
                    "<i>There is only the gentle way, and the others.\"</i>\n" +
                    "                                                — M.";
            }
        }
    }
}
