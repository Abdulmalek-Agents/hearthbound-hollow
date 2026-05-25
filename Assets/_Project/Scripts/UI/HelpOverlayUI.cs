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
//
// ── Phase 25 hotfix ─────────────────────────────────────────────
// Same family of bugs as PauseMenuUI — when Phase 23 deactivated the
// script-host, Update() stopped firing and the H key did nothing. The
// script-host now stays active; only the visual `root` child gets
// toggled. Show() self-heals defensively.
//
// ── Phase 26 update ─────────────────────────────────────────────
// The new PlayerController exposes sprint (Shift), jump (Space) and the
// SmoothFollowCamera exposes orbit (RMB + drag) + zoom (scroll). The
// reference card is rewritten to document them. Gentle Mode-aware: when
// SettingsService.GentleMode == true we strip the sprint + jump lines.

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HearthboundHollow.Core;

namespace HearthboundHollow.UI
{
    public class HelpOverlayUI : MonoBehaviour
    {
        [Header("Root")]
        [Tooltip("The visual root that gets toggled. SHOULD be a child of the " +
                 "script-host GameObject — never set this to the same GameObject " +
                 "as the script, or Update() won't run and H won't toggle.")]
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
            // Hide only the visual panel — keep the host GameObject active so
            // Update() can listen for the toggle key.
            if (root != null && root != gameObject) root.SetActive(false);
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
            // Defensive self-heal — should never be needed once Phase 23 wiring
            // is correct, but cheap to keep.
            if (!gameObject.activeSelf) gameObject.SetActive(true);

            if (root != null) root.SetActive(true);
            ApplyContent();
            IsOpen = true;
            Hh.Log(LogCategory.UI, "Help overlay opened.");
        }

        public void Hide()
        {
            if (root != null && root != gameObject) root.SetActive(false);
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

            // Always re-build the body so toggling Gentle Mode at runtime is
            // immediately reflected on the next H-open.
            if (bodyText != null) bodyText.text = BuildBody();
        }

        private static string BuildBody()
        {
            bool gentle = false;
            var s = ServiceLocator.Get<SettingsService>();
            if (s != null) gentle = s.GentleMode;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("<b>Move</b>      WASD / Arrow Keys / Left Stick");
            if (!gentle)
            {
                sb.AppendLine("<b>Sprint</b>    Left Shift / Left-stick click");
                sb.AppendLine("<b>Jump</b>      Space / Gamepad south");
            }
            sb.AppendLine("<b>Interact</b>  E / Gamepad ▢");
            sb.AppendLine("<b>Advance</b>   Click / Space / Enter");
            sb.AppendLine("<b>Polish</b>    Hold left mouse, draw slow circles");
            sb.AppendLine("                — cover all sides of the orb");
            sb.AppendLine("                — slower is better");
            sb.AppendLine("<b>Look</b>      Hold Right Mouse + drag (or Right Stick)");
            sb.AppendLine("<b>Zoom</b>      Mouse scroll / Gamepad LB-RB");
            sb.AppendLine("<b>Help</b>      H to toggle this card");
            sb.AppendLine("<b>Pause</b>     Esc");
            sb.AppendLine();
            sb.AppendLine("<i>\"There is no wrong way to keep a memory.</i>");
            sb.AppendLine("<i>There is only the gentle way, and the others.\"</i>");
            sb.Append("                                                — M.");
            return sb.ToString();
        }
    }
}
