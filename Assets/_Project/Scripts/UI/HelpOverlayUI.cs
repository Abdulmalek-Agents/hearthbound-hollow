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
// ── Phase 60 — Arabic Localization MVP ──────────────────────────
// Title, subtitle, every row, and the closing Marin quote come from
// loc.<iso>.json via LocalizationService. The Phase 32.20 cozy emoji +
// colour-tag styling lives in the loc tables so both English and Arabic
// players see the same warm visuals around their translated copy.

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
            // Phase 60 — Localized title / subtitle / body. The Phase 32.20
            // cozy emoji + colour-tag styling lives in the loc.<iso>.json
            // tables now (e.g. `help.title` = "🪔  Welcome to the Hollow  🪔"
            // for English and "🪔  أهلًا بكَ في الجَوْف  🪔" for Arabic) so
            // both languages get the same warm visuals around their
            // translated copy. Falls back to the original English when the
            // LocalizationService isn't yet registered (e.g. EditMode test).
            var loc = ServiceLocator.Get<LocalizationService>();
            bool rtl = loc != null && loc.IsRightToLeft;

            string title    = loc != null ? loc.Get("help.title")    : "🪔  Welcome to the Hollow  🪔";
            string subtitle = loc != null ? loc.Get("help.subtitle") : "✒  A quick word from Marin's notes …";

            if (titleLabel != null)
            {
                titleLabel.text = rtl ? ArabicTextShaper.Shape(title) : title;
                titleLabel.isRightToLeftText = rtl;
            }
            if (subtitleLabel != null)
            {
                subtitleLabel.text = rtl ? ArabicTextShaper.Shape(subtitle) : subtitle;
                subtitleLabel.isRightToLeftText = rtl;
            }

            // Always re-build the body so toggling Gentle Mode at runtime is
            // immediately reflected on the next H-open.
            if (bodyText != null)
            {
                string body = BuildBody();
                if (rtl) body = ArabicTextShaper.Shape(body);
                bodyText.text = body;
                bodyText.isRightToLeftText = rtl;
                bodyText.alignment = rtl
                    ? TMPro.TextAlignmentOptions.TopRight
                    : TMPro.TextAlignmentOptions.TopLeft;
            }
        }

        /// <summary>
        /// Build the controls-reference body. Honors Gentle Mode (hides
        /// Sprint + Jump rows) and the active locale (Phase 60). Cozy emoji
        /// + colour tags from Phase 32.20 are embedded in the loc tables so
        /// the same TMP rich-text renders in both languages.
        /// </summary>
        private static string BuildBody()
        {
            bool gentle = false;
            var s = ServiceLocator.Get<SettingsService>();
            if (s != null) gentle = s.GentleMode;

            var loc = ServiceLocator.Get<LocalizationService>();

            string G(string key, string englishFallback)
                => loc != null ? loc.Get(key) : englishFallback;

            // Phase 32.20 source-of-truth fallback strings — used only when
            // the LocalizationService hasn't yet registered (rare). Real
            // runtime path goes through `loc.Get(…)` which reads loc.<iso>.json.
            const string gold  = "<color=#7a4f10>";
            const string brown = "<color=#5a3a18>";
            const string ink   = "<color=#221208>";
            const string dim   = "<color=#705a3a>";
            const string end   = "</color>";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine(G("help.row.move",      $"{gold}🚶{end}  {brown}<b>Move</b>{end}   {ink}WASD / Arrow Keys / Left Stick{end}"));
            if (!gentle)
            {
                sb.AppendLine(G("help.row.sprint", $"{gold}🏃{end}  {brown}<b>Sprint</b>{end}   {ink}Left Shift / Left-stick click{end}"));
                sb.AppendLine(G("help.row.jump",   $"{gold}⤴{end}  {brown}<b>Jump</b>{end}   {ink}Space / Gamepad south{end}"));
            }
            sb.AppendLine(G("help.row.interact",  $"{gold}✋{end}  {brown}<b>Interact</b>{end}   {ink}E / Gamepad □{end}"));
            sb.AppendLine(G("help.row.advance",   $"{gold}▶{end}  {brown}<b>Advance</b>{end}   {ink}Click / Space / Enter{end}"));
            sb.AppendLine(G("help.row.polish_1",  $"{gold}✨{end}  {brown}<b>Polish</b>{end}   {ink}Hold left mouse, draw slow circles{end}"));
            sb.AppendLine(G("help.row.polish_2",  $"      {dim}— cover all sides of the orb{end}"));
            sb.AppendLine(G("help.row.polish_3",  $"      {dim}— slower is better{end}"));
            sb.AppendLine(G("help.row.look",      $"{gold}👁{end}  {brown}<b>Look</b>{end}   {ink}Hold Right Mouse + drag (or Right Stick){end}"));
            sb.AppendLine(G("help.row.zoom",      $"{gold}🔍{end}  {brown}<b>Zoom</b>{end}   {ink}Mouse scroll / Gamepad LB-RB{end}"));
            sb.AppendLine(G("help.row.help",      $"{gold}❓{end}  {brown}<b>Help</b>{end}   {ink}H to toggle this card{end}"));
            sb.AppendLine(G("help.row.pause",     $"{gold}⏸{end}  {brown}<b>Pause</b>{end}   {ink}Esc{end}"));
            sb.AppendLine();
            sb.AppendLine(G("help.row.signature",
                $"{brown}<i>\u201cThere is no wrong way to keep a memory.\n" +
                $"There is only the gentle way, and the others.\u201d</i>{end}\n" +
                $"                                                {gold}— M.{end}"));
            return sb.ToString();
        }

        private void OnLocaleChanged(LocaleChangedEvent _) => ApplyContent();

        private void OnEnable()
        {
            EventBus.Subscribe<LocaleChangedEvent>(OnLocaleChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<LocaleChangedEvent>(OnLocaleChanged);
        }
    }
}
