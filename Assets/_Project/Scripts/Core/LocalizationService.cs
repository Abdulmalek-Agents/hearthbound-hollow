// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Core / LocalizationService
//
// Phase 53 — runtime UI localization (English / Arabic, extensible).
//
// A tiny, dependency-free localization service:
//   • Holds the current language (persisted via SettingsService.Language).
//   • Maps string KEYS → per-language text via an in-code table (seeded with
//     the menu / settings / character-creation / common-UI chrome).
//   • Raises OnLanguageChanged so bound TMP labels (LocalizedText) refresh live.
//   • Exposes IsRightToLeft so UI can flip alignment for Arabic.
//
// Scope note (Cozy Contract / GAME_DESIGN §9 Pillar 1): the hand-written
// *dialogue* prose (Yarn) stays the canonical English voice for now — full
// dialogue localization is a dedicated translation pass with the writers, not
// a machine pass. This service localizes the UI chrome + adds the language
// selector the player asked for; missing keys fall back to the key's English
// (or the key itself) so nothing ever renders blank. (D-065.)

using System;
using System.Collections.Generic;

namespace HearthboundHollow.Core
{
    public enum GameLanguage { English = 0, Arabic = 1 }

    public static class LocalizationService
    {
        /// <summary>Raised whenever the language changes. UI binders re-pull their text.</summary>
        public static event Action OnLanguageChanged;

        private static GameLanguage _current = GameLanguage.English;
        private static bool _initialised;

        public static GameLanguage Current
        {
            get { EnsureInit(); return _current; }
        }

        /// <summary>Arabic is RTL; English (and future LTR languages) are not.</summary>
        public static bool IsRightToLeft => Current == GameLanguage.Arabic;

        public static string CurrentCode => Current == GameLanguage.Arabic ? "ar" : "en";

        // ───── Init / language switching ───────────────────────────

        private static void EnsureInit()
        {
            if (_initialised) return;
            _initialised = true;
            var s = ServiceLocator.Get<SettingsService>();
            _current = ParseCode(s != null ? s.Language : "en");
        }

        public static GameLanguage ParseCode(string code) =>
            string.Equals(code, "ar", StringComparison.OrdinalIgnoreCase)
                ? GameLanguage.Arabic : GameLanguage.English;

        public static void SetLanguage(GameLanguage lang)
        {
            EnsureInit();
            if (_current == lang) { OnLanguageChanged?.Invoke(); return; }
            _current = lang;
            var s = ServiceLocator.Get<SettingsService>();
            if (s != null) s.Language = (lang == GameLanguage.Arabic) ? "ar" : "en";
            Hh.Log(LogCategory.UI, $"Language set to {lang}.");
            OnLanguageChanged?.Invoke();
        }

        public static void Toggle() =>
            SetLanguage(Current == GameLanguage.English ? GameLanguage.Arabic : GameLanguage.English);

        // ───── Lookup ──────────────────────────────────────────────

        /// <summary>
        /// Resolve a localized string by key for the current language. Falls back
        /// to English, then to the key itself, so the UI is never blank.
        /// </summary>
        public static string Get(string key)
        {
            EnsureInit();
            if (string.IsNullOrEmpty(key)) return "";
            if (Table.TryGetValue(key, out var pair))
                return Current == GameLanguage.Arabic && !string.IsNullOrEmpty(pair.ar) ? pair.ar : pair.en;
            return key; // unknown key → show the key (visible, debuggable, never blank)
        }

        public static string Get(string key, GameLanguage lang)
        {
            if (Table.TryGetValue(key, out var pair))
                return lang == GameLanguage.Arabic && !string.IsNullOrEmpty(pair.ar) ? pair.ar : pair.en;
            return key;
        }

        // ───── String table (UI chrome) ───────────────────────────
        // key → (English, Arabic). Add freely — this is the canonical UI
        // translation table for Mission 1-2. Dialogue prose is NOT here.

        private static readonly Dictionary<string, (string en, string ar)> Table = new()
        {
            // Main menu
            ["menu.open_hollow"]   = ("Open The Hollow", "افتح الجوف"),
            ["menu.new_game"]      = ("New Game", "لعبة جديدة"),
            ["menu.continue"]      = ("Continue", "متابعة"),
            ["menu.settings"]      = ("Settings", "الإعدادات"),
            ["menu.credits"]       = ("Credits", "شكر وتقدير"),
            ["menu.quit"]          = ("Quit", "خروج"),

            // Settings / system menu
            ["settings.title"]     = ("Settings", "الإعدادات"),
            ["settings.language"]  = ("Language", "اللغة"),
            ["settings.english"]   = ("English", "الإنجليزية"),
            ["settings.arabic"]    = ("العربية", "العربية"),
            ["settings.gentle"]    = ("Gentle Mode", "الوضع اللطيف"),
            ["settings.customize"] = ("Customize Character", "تخصيص الشخصية"),
            ["settings.reset"]     = ("Reset Game", "إعادة ضبط اللعبة"),
            ["settings.reset_confirm_title"] = ("Reset Game?", "إعادة ضبط اللعبة؟"),
            ["settings.reset_confirm_body"]  = (
                "This clears your saved progress and returns to the title. Your settings are kept. The Hollow will be new again.",
                "سيؤدي هذا إلى مسح تقدّمك المحفوظ والعودة إلى الشاشة الرئيسية. تبقى إعداداتك كما هي. سيعود الجوف جديدًا من جديد."),
            ["common.back"]        = ("Back", "رجوع"),
            ["common.confirm"]     = ("Confirm", "تأكيد"),
            ["common.cancel"]      = ("Cancel", "إلغاء"),
            ["common.close"]       = ("Close", "إغلاق"),
            ["common.resume"]      = ("Resume", "استئناف"),

            // Character creation
            ["char.title"]         = ("Who Will Keep The Hollow?", "مَن سيحرس الجوف؟"),
            ["char.name"]          = ("Your name", "اسمك"),
            ["char.skin"]          = ("Skin", "البشرة"),
            ["char.outfit"]        = ("Outfit", "الزي"),
            ["char.accessory"]     = ("Accessory", "الإكسسوار"),
            ["char.random"]        = ("Surprise me", "فاجئني"),
            ["char.begin"]         = ("Begin", "ابدأ"),
            ["char.acc_none"]      = ("None", "بدون"),
            ["char.acc_cap"]       = ("Cap", "قبعة"),
            ["char.acc_flower"]    = ("Flower", "زهرة"),
            ["char.acc_scarf"]     = ("Scarf", "وشاح"),

            // Pause
            ["pause.title"]        = ("Paused", "إيقاف مؤقت"),
            ["pause.save_quit"]    = ("Save & Quit to Title", "حفظ والخروج للرئيسية"),
        };
    }
}
