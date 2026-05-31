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
// ── Phase 25 hotfix ────────────────────────────────────
// Same family of bugs as PauseMenuUI — when Phase 23 deactivated the
// script-host, Update() stopped firing and the H key did nothing. The
// script-host now stays active; only the visual `root` child gets
// toggled. Show() self-heals defensively.
//
// ── Phase 26 update ────────────────────────────────────
// The new PlayerController exposes sprint (Shift), jump (Space) and the
// SmoothFollowCamera exposes orbit (RMB + drag) + zoom (scroll). The
// reference card is rewritten to document them. Gentle Mode-aware: when
// SettingsService.GentleMode == true we strip the sprint + jump lines.
//
// ── Phase 58 (D-076) — emoji glyphs + full Arabic ─────────────────
// This is the exact card the QA video froze on at the end. Two fixes:
//   • Every decorative emoji now routes through HollowGlyphs.Format so it
//     renders as an on-brand gold TMP <sprite> (or clean text) — never a
//     tofu box (it previously set RAW unicode straight onto the label).
//   • The header AND the whole controls body are localized (EN/العربية),
//     Arabic-shaped + right-aligned, and rebuilt live on language change.
//     Help strings live inline here (UI-chrome local to this card); the
//     menu/settings chrome stays in LocalizationService (D-065).

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

        private void OnEnable()
        {
            // Rebuild on language change so العربية ⇄ English flips live.
            LocalizationService.OnLanguageChanged += OnLanguageChanged;
        }

        private void OnDisable()
        {
            LocalizationService.OnLanguageChanged -= OnLanguageChanged;
        }

        private void OnLanguageChanged()
        {
            // Cheap to rebuild; only matters visually while the card is shown.
            ApplyContent();
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
            bool rtl = LocalizationService.IsRightToLeft;

            // Header — localized + glyph-routed (never raw unicode → no tofu).
            if (titleLabel != null)
            {
                string lantern = HollowGlyphs.Format("🪔");
                string t = LocalizationService.GetShaped("help.title");
                titleLabel.text = string.IsNullOrEmpty(lantern) ? t : $"{lantern}  {t}";
                titleLabel.horizontalAlignment = HorizontalAlignmentOptions.Center;
            }
            if (subtitleLabel != null)
            {
                subtitleLabel.text = LocalizationService.GetShaped("help.subtitle");
                subtitleLabel.horizontalAlignment = HorizontalAlignmentOptions.Center;
            }

            // Always re-build the body so toggling Gentle Mode / language at
            // runtime is reflected on the next open.
            if (bodyText != null)
            {
                bodyText.text = BuildBody();
                bodyText.horizontalAlignment = rtl ? HorizontalAlignmentOptions.Right
                                                   : HorizontalAlignmentOptions.Left;
            }
        }

        // EN/AR pick + Arabic shaping for an inline help string.
        private static string L(string en, string ar)
        {
            string s = (LocalizationService.IsRightToLeft && !string.IsNullOrEmpty(ar)) ? ar : en;
            return LocalizationService.Shape(s);
        }

        private static string BuildBody()
        {
            bool gentle = false;
            var s = ServiceLocator.Get<SettingsService>();
            if (s != null) gentle = s.GentleMode;

            // Phase 32.20 / 58 — readability + warmth. Each action gets a cozy
            // gold glyph (routed via HollowGlyphs → TMP <sprite>, never tofu) and
            // the verb is wrapped in warm gold so the eye snaps to it. Verb +
            // binding are localized + Arabic-shaped; the colour tags wrap the
            // already-shaped text so RTL never corrupts a tag.
            const string ink   = "<color=#221208>";
            const string gold  = "<color=#7a4f10>";
            const string brown = "<color=#5a3a18>";
            const string dim   = "<color=#705a3a>";
            const string end   = "</color>";

            string Row(string emoji, string verbEn, string verbAr, string bindEn, string bindAr)
            {
                // Phase 58.1 — NO inline emoji/sprite in this list. Mixing many TMP
                // <sprite> glyphs + raw (unmapped) emoji + shaped Arabic + rich-text
                // in one mesh triggered Unity 6 TMP's GenerateTextMesh
                // InvalidCastException, which froze the game on this overlay. The
                // bold gold verb carries the hierarchy; the title keeps its single
                // lantern glyph (which renders fine on its own). `emoji` is ignored.
                string verb = L(verbEn, verbAr);
                string bind = L(bindEn, bindAr);
                return $"{brown}<b>{verb}</b>{end}   {ink}{bind}{end}";
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine(Row("🚶", "Move", "التحرّك", "WASD / Arrow Keys / Left Stick", "WASD / الأسهم / العصا اليسرى"));
            if (!gentle)
            {
                sb.AppendLine(Row("🏃", "Sprint", "العَدْو", "Left Shift / Left-stick click", "Shift الأيسر / ضغط العصا اليسرى"));
                sb.AppendLine(Row("⤴", "Jump", "القفز", "Space / Gamepad south", "مسافة / زر ذراع التحكم"));
            }
            sb.AppendLine(Row("✋", "Interact", "التفاعل", "E / Gamepad □", "E / زر ذراع التحكم"));
            sb.AppendLine(Row("▶", "Advance", "المتابعة", "Click / Space / Enter", "نقر / مسافة / إدخال"));
            sb.AppendLine(Row("✨", "Polish", "التلميع", "Hold left mouse, draw slow circles", "اضغط زر الفأرة الأيسر وارسم دوائر بطيئة"));
            sb.AppendLine($"      {dim}{L("— cover all sides of the orb", "— غطِّ جميع جوانب الجوهرة")}{end}");
            sb.AppendLine($"      {dim}{L("— slower is better", "— الأبطأ أفضل")}{end}");
            sb.AppendLine(Row("👁", "Look", "النظر حول", "Hold Right Mouse + drag (or Right Stick)", "اضغط زر الفأرة الأيمن واسحب (أو العصا اليمنى)"));
            sb.AppendLine(Row("🔍", "Zoom", "التقريب", "Mouse scroll / Gamepad LB-RB", "عجلة الفأرة / LB-RB"));
            sb.AppendLine(Row("❓", "Help", "المساعدة", "H to toggle this card", "H لإظهار/إخفاء هذه البطاقة"));
            sb.AppendLine(Row("⏸", "Pause", "الإيقاف المؤقت", "Esc", "Esc"));

            // Phase 71 — the daily-loop hub keys (Engagement Bible P2–P7). The
            // “full reference” card now lists every place the Hollow's day asks
            // you to visit, so a returning player never forgets a hotkey.
            sb.AppendLine();
            sb.AppendLine($"{gold}<b>{L("Your day's places", "أماكن يومك")}</b>{end}");
            sb.AppendLine(Row("🪔", "Journal", "الدفتر", "J — today's agenda", "J — جدول اليوم"));
            sb.AppendLine(Row("✋", "Requests", "الطلبات", "B — village request board", "B — لوحة طلبات القرية"));
            sb.AppendLine(Row("✨", "Memory Wall", "جدار الذكريات", "M — memories you've kept", "M — الذكريات التي حفظتها"));
            sb.AppendLine(Row("🔑", "Your Hollow", "جوفك", "U — comforts & upgrades", "U — وسائل الراحة والترقيات"));
            sb.AppendLine(Row("🍂", "Garden", "الحديقة", "G — tend beds & brew tea", "G — اعتنِ بالأحواض واصنع الشاي"));
            sb.AppendLine(Row("🫖", "Workbench", "طاولة العمل", "K — craft & mend", "K — اصنع وأصلح"));
            sb.AppendLine();
            sb.AppendLine($"{brown}<i>{L("“There is no wrong way to keep a memory.", "”لا توجد طريقة خاطئة لحفظ ذكرى.")}</i>{end}");
            sb.AppendLine($"{brown}<i>{L("There is only the gentle way, and the others.”", "هناك فقط الطريقة اللطيفة، والطرق الأخرى.“")}</i>{end}");
            sb.Append($"                                                {gold}{L("— M.", "— م.")}{end}");
            return sb.ToString();
        }
    }
}
