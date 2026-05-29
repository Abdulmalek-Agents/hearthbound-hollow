// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / ComfortToolsMenu
//
// The Comfort Tools modal. Surfaces Auto-Complete toggles, Gentle Mode,
// color-blind palette, subtitle size, one-hand control. Per Codex 06.
//
// ── Phase 60 — Arabic Localization MVP ──────────────────────────
// Adds a two-button LANGUAGE PICKER (English / العربية) at the top of
// the modal. Every section header, toggle label, slider tier, and
// palette button text is pulled from loc.<iso>.json. Native-name
// language labels are intentionally never translated — a player who
// picked the wrong language can recognise their own again.

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HearthboundHollow.Core;

namespace HearthboundHollow.UI
{
    public class ComfortToolsMenu : MonoBehaviour
    {
        [Header("Root")]
        public GameObject root;

        [Header("Toggles")]
        public Toggle gentleMode;
        public Toggle autoCompletePolish;
        public Toggle autoCompleteCleanse;
        public Toggle oneHandMode;

        [Header("Sliders")]
        [Tooltip("0 = small, 1 = medium, 2 = large, 3 = huge")]
        public Slider subtitleSize;
        public TextMeshProUGUI subtitleSizeLabel;

        [Header("Color palette")]
        public Button paletteDefault;
        public Button paletteProtanopia;
        public Button paletteDeuteranopia;
        public Button paletteTritanopia;

        [Header("Language (Phase 60 — Arabic Localization MVP)")]
        [Tooltip("Two-button language picker. Default builder spawns one " +
                 "button per supported Locale. The button's child TMP label is " +
                 "set to the locale's NATIVE name (English / العربية) — " +
                 "intentionally NOT translated so a player who picked the " +
                 "wrong language can recognise their own again.")]
        public Button languageEnglishButton;
        public Button languageArabicButton;
        public TextMeshProUGUI languageCurrentLabel;

        // ── Labels for each section / toggle / slider (Phase 60) ──
        [Header("Localized labels (auto-wired by Phase 60 builder)")]
        public TextMeshProUGUI titleLabel;
        public TextMeshProUGUI sectionAudioLabel;
        public TextMeshProUGUI sectionTextLabel;
        public TextMeshProUGUI sectionComfortLabel;
        public TextMeshProUGUI sectionLanguageLabel;
        public TextMeshProUGUI gentleModeLabel;
        public TextMeshProUGUI autoPolishLabel;
        public TextMeshProUGUI autoCleanseLabel;
        public TextMeshProUGUI oneHandLabel;
        public TextMeshProUGUI subtitleSizeSectionLabel;

        public bool AutoCompletePolish { get; private set; } = false;
        public bool AutoCompleteCleanse { get; private set; } = false;
        public bool OneHandMode { get; private set; } = false;
        public int SubtitleSizeTier { get; private set; } = 1;
        public string ColorPaletteId { get; private set; } = "default";

        private void Awake()
        {
            if (root != null && root != gameObject) root.SetActive(false);
            if (gentleMode != null) gentleMode.onValueChanged.AddListener(OnGentleMode);
            if (autoCompletePolish != null) autoCompletePolish.onValueChanged.AddListener(v => AutoCompletePolish = v);
            if (autoCompleteCleanse != null) autoCompleteCleanse.onValueChanged.AddListener(v => AutoCompleteCleanse = v);
            if (oneHandMode != null) oneHandMode.onValueChanged.AddListener(v => OneHandMode = v);
            if (subtitleSize != null) subtitleSize.onValueChanged.AddListener(OnSubtitleSize);
            if (paletteDefault != null) paletteDefault.onClick.AddListener(() => ColorPaletteId = "default");
            if (paletteProtanopia != null) paletteProtanopia.onClick.AddListener(() => ColorPaletteId = "protanopia");
            if (paletteDeuteranopia != null) paletteDeuteranopia.onClick.AddListener(() => ColorPaletteId = "deuteranopia");
            if (paletteTritanopia != null) paletteTritanopia.onClick.AddListener(() => ColorPaletteId = "tritanopia");

            // Phase 60 — language picker wiring. Each button calls
            // LocalizationService.SetLocale, which publishes a
            // LocaleChangedEvent. Every other UI subscribes and refreshes.
            if (languageEnglishButton != null)
                languageEnglishButton.onClick.AddListener(() => OnLanguagePicked(Locale.English));
            if (languageArabicButton != null)
                languageArabicButton.onClick.AddListener(() => OnLanguagePicked(Locale.Arabic));
        }

        private void OnLanguagePicked(Locale loc)
        {
            var svc = ServiceLocator.Get<LocalizationService>();
            if (svc == null) return;
            svc.SetLocale(loc);
            // Native-name labels are intentional: never translated — see field tooltip.
            if (languageCurrentLabel != null)
                languageCurrentLabel.text = LocaleInfo.NativeName(svc.CurrentLocale);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<LocaleChangedEvent>(OnLocaleChanged);
            ApplyLocalization();
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<LocaleChangedEvent>(OnLocaleChanged);
        }

        private void OnLocaleChanged(LocaleChangedEvent _) => ApplyLocalization();

        private void ApplyLocalization()
        {
            var loc = ServiceLocator.Get<LocalizationService>();
            if (loc == null) return;
            bool rtl = loc.IsRightToLeft;

            SetLabel(titleLabel,                loc.Get("settings.title"),               rtl);
            SetLabel(sectionAudioLabel,         loc.Get("settings.section.audio"),       rtl);
            SetLabel(sectionTextLabel,          loc.Get("settings.section.text"),        rtl);
            SetLabel(sectionComfortLabel,       loc.Get("settings.section.comfort"),     rtl);
            SetLabel(sectionLanguageLabel,      loc.Get("settings.section.language"),    rtl);
            SetLabel(gentleModeLabel,           loc.Get("settings.gentle_mode"),         rtl);
            SetLabel(autoPolishLabel,           loc.Get("settings.auto_polish"),         rtl);
            SetLabel(autoCleanseLabel,          loc.Get("settings.auto_cleanse"),        rtl);
            SetLabel(oneHandLabel,              loc.Get("settings.one_hand"),            rtl);
            SetLabel(subtitleSizeSectionLabel,  loc.Get("settings.subtitle_size"),       rtl);

            // Native names on language buttons — NEVER translated.
            if (languageEnglishButton != null)
            {
                var t = languageEnglishButton.GetComponentInChildren<TextMeshProUGUI>();
                if (t != null) t.text = LocaleInfo.NativeName(Locale.English);
            }
            if (languageArabicButton != null)
            {
                var t = languageArabicButton.GetComponentInChildren<TextMeshProUGUI>();
                if (t != null) t.text = LocaleInfo.NativeName(Locale.Arabic);
            }
            if (languageCurrentLabel != null)
                languageCurrentLabel.text = LocaleInfo.NativeName(loc.CurrentLocale);

            // Refresh the subtitle-size tier label too.
            OnSubtitleSize(subtitleSize != null ? subtitleSize.value : SubtitleSizeTier);
        }

        private static void SetLabel(TextMeshProUGUI t, string s, bool rtl)
        {
            if (t == null) return;
            t.text = rtl ? ArabicTextShaper.Shape(s) : s;
            t.isRightToLeftText = rtl;
        }

        public void Show()
        {
            // Self-heal.
            if (!gameObject.activeSelf) gameObject.SetActive(true);

            if (root != null) root.SetActive(true);
            var vs = ServiceLocator.Get<VillageState>();
            if (vs != null && gentleMode != null) gentleMode.isOn = vs.gentleModeEnabled;
        }

        public void Hide()
        {
            if (root != null && root != gameObject) root.SetActive(false);
        }

        private void OnGentleMode(bool v)
        {
            var vs = ServiceLocator.Get<VillageState>();
            if (vs != null) vs.gentleModeEnabled = v;
            Hh.Log(LogCategory.UI, $"Gentle Mode set to {v}.");
        }

        private void OnSubtitleSize(float v)
        {
            SubtitleSizeTier = Mathf.Clamp((int)v, 0, 3);
            if (subtitleSizeLabel == null) return;
            // Phase 60 — localized "Small/Medium/Large/Huge".
            var loc = ServiceLocator.Get<LocalizationService>();
            string key = "settings.subtitle_size." + SubtitleSizeTier;
            string s = loc != null
                ? loc.Get(key)
                : SubtitleSizeTier switch { 0 => "Small", 1 => "Medium", 2 => "Large", _ => "Huge" };
            bool rtl = loc != null && loc.IsRightToLeft;
            subtitleSizeLabel.text = rtl ? ArabicTextShaper.Shape(s) : s;
            subtitleSizeLabel.isRightToLeftText = rtl;
        }
    }
}
