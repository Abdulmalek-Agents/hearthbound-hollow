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

            // Phase 32.19 — readability pass. Headline pops with cream
            // outline + drop-shadow; body prose gets ink-dark on cream so
            // the controls list reads cleanly against any parchment.
            UIReadabilityHelper.ApplyHeadline (titleLabel,    min: 48, max: 88);
            UIReadabilityHelper.ApplySubtitle (subtitleLabel, min: 22, max: 32);
            UIReadabilityHelper.ApplyBody     (bodyText,      min: 22, max: 34);
            if (bodyText != null) UIReadabilityHelper.AddDarkWash(bodyText.rectTransform, padding: 16f);
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
            // Phase 32.20 — friendly emoji glyphs on the title/subtitle so
            // the card feels warm before the player has even read a word.
            // (Existing user-set inspector text is preserved.)
            if (titleLabel != null && string.IsNullOrEmpty(titleLabel.text))
                titleLabel.text = "🪔  Welcome to the Hollow  🪔";
            if (subtitleLabel != null && string.IsNullOrEmpty(subtitleLabel.text))
                subtitleLabel.text = "✒  A quick word from Marin's notes …";

            // Always re-build the body so toggling Gentle Mode at runtime is
            // immediately reflected on the next H-open.
            if (bodyText != null) bodyText.text = BuildBody();
        }

        private static string BuildBody()
        {
            bool gentle = false;
            var s = ServiceLocator.Get<SettingsService>();
            if (s != null) gentle = s.GentleMode;

            // Phase 32.20 — readability + warmth pass on the controls card.
            // Each action gets a cozy emoji glyph (a torch instead of a
            // generic dot) and the verb is wrapped in a warm gold colour
            // so the eye snaps to the verb before reading the input. The
            // closing quote is hand-spaced for centre-alignment under the
            // signature line.
            const string ink   = "<color=#221208>";
            const string gold  = "<color=#7a4f10>";
            const string brown = "<color=#5a3a18>";
            const string dim   = "<color=#705a3a>";
            const string end   = "</color>";

            string Row(string emoji, string verb, string binding)
                => $"{gold}{emoji}{end}  {brown}<b>{verb}</b>{end}   {ink}{binding}{end}";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine(Row("🚶", "Move",     "WASD / Arrow Keys / Left Stick"));
            if (!gentle)
            {
                sb.AppendLine(Row("🏃", "Sprint",   "Left Shift / Left-stick click"));
                sb.AppendLine(Row("⤴",  "Jump",     "Space / Gamepad south"));
            }
            sb.AppendLine(Row("✋", "Interact", "E / Gamepad □"));
            sb.AppendLine(Row("▶",  "Advance",  "Click / Space / Enter"));
            sb.AppendLine(Row("✨", "Polish",   "Hold left mouse, draw slow circles"));
            sb.AppendLine($"      {dim}— cover all sides of the orb{end}");
            sb.AppendLine($"      {dim}— slower is better{end}");
            sb.AppendLine(Row("👁", "Look",     "Hold Right Mouse + drag (or Right Stick)"));
            sb.AppendLine(Row("🔍", "Zoom",     "Mouse scroll / Gamepad LB-RB"));
            sb.AppendLine(Row("❓", "Help",     "H to toggle this card"));
            sb.AppendLine(Row("⏸",  "Pause",    "Esc"));
            sb.AppendLine();
            sb.AppendLine($"{brown}<i>“There is no wrong way to keep a memory.</i>{end}");
            sb.AppendLine($"{brown}<i>There is only the gentle way, and the others.”</i>{end}");
            sb.Append($"                                                {gold}— M.{end}");
            return sb.ToString();
        }
    }
}
