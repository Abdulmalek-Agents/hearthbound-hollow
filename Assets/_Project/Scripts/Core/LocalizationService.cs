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

        // ───── Display shaping (Arabic) ────────────────────────────
        // Arabic must be contextually joined + RTL-ordered before a classic TMP
        // label can show real words (see ArabicShaper). These helpers centralise
        // that so any runtime code can localise a label in one call. Multi-line
        // strings are shaped LINE-BY-LINE — shaping reverses to visual order, so
        // shaping a whole block would flip the line order too. Non-RTL languages
        // (and tag-free Latin) pass straight through untouched.

        /// <summary>Display-shape an already-localized string for the current language.</summary>
        public static string Shape(string raw)
        {
            if (!IsRightToLeft || string.IsNullOrEmpty(raw)) return raw;
            if (raw.IndexOf('\n') < 0) return ArabicShaper.Shape(raw);
            var lines = raw.Split('\n');
            for (int i = 0; i < lines.Length; i++) lines[i] = ArabicShaper.Shape(lines[i]);
            return string.Join("\n", lines);
        }

        /// <summary>Localized + display-shaped string for the current language.</summary>
        public static string GetShaped(string key) => Shape(Get(key));

        /// <summary>Localized format string with <paramref name="args"/> substituted, then shaped.</summary>
        public static string GetShaped(string key, params object[] args) => Shape(string.Format(Get(key), args));

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
            ["pause.hint"]         = ("Take a breath. The Hollow will wait.", "خُذ نفسًا. سينتظر الجوف."),
            ["pause.save_quit_menu"] = ("Save & Quit to Main Menu", "حفظ والخروج للقائمة الرئيسية"),
            ["pause.quit_desktop"] = ("Quit to Desktop", "الخروج إلى سطح المكتب"),

            // Comfort Tools (accessibility) panel
            ["comfort.title"]      = ("Settings · Comfort Tools", "الإعدادات · أدوات الراحة"),
            ["comfort.gentle"]     = ("Gentle Mode (longer timers, no fail states)",
                                      "الوضع اللطيف (مؤقّتات أطول، بلا شاشات إخفاق)"),
            ["comfort.autocomplete_polish"]  = ("Auto-Complete Polish mini-game", "إكمال تلقائي للعبة التلميع"),
            ["comfort.autocomplete_cleanse"] = ("Auto-Complete Cleanse mini-game", "إكمال تلقائي للعبة التطهير"),
            ["comfort.subtitle_size"] = ("Subtitle size", "حجم الترجمة"),
            ["comfort.gentle_on"]  = ("Gentle Mode: ON", "الوضع اللطيف: مُفعّل"),
            ["comfort.gentle_off"] = ("Gentle Mode: off", "الوضع اللطيف: مُعطّل"),

            // Subtitle-size tiers (also reusable as generic size words)
            ["size.small"]         = ("Small", "صغير"),
            ["size.medium"]        = ("Medium", "متوسط"),
            ["size.large"]         = ("Large", "كبير"),
            ["size.huge"]          = ("Huge", "ضخم"),

            // Help overlay (header chrome; the controls body stays EN this pass)
            ["help.title"]         = ("Welcome to the Hollow", "أهلًا بك في الجوف"),
            ["help.subtitle"]      = ("A quick word from Marin's notes …", "كلمة وجيزة من ملاحظات مارين …"),
            ["help.close"]         = ("Close (H)", "إغلاق"),

            // HUD / Evening Ledger chrome (format: {0} = number)
            ["hud.day"]            = ("Day {0}", "اليوم {0}"),
            ["hud.coins"]          = ("{0} c", "{0} ع"),

            // Dialogue chrome
            ["dialogue.advance"]   = ("Click or [Space] >", "انقر أو اضغط مسافة"),

            // Cold-open / title cinematic chrome (the game name stays as-is).
            ["coldopen.tagline"]       = ("Some memories want to be sold. Some don't.",
                                          "بعض الذكريات تريد أن تُباع. وبعضها لا."),
            ["coldopen.begin_hint"]    = ("Press Enter / Space to Begin", "اضغط Enter / مسافة للبدء"),
            ["coldopen.continue_hint"] = ("Press Enter to Continue · Esc to Begin Anew",
                                          "اضغط Enter للمتابعة · Esc للبدء من جديد"),

            // Tone Compass — first-launch 90-second tone/content primer (Focus 07 §7.1).
            // Plain text only (shaped line-by-line). EN is the canonical Pell Doyne copy.
            ["tone.body"]          = (
                "This game will make you feel things. Some of those feelings are heavy.\n\n" +
                "This first hour contains: the opening of a shop, a first transaction, a late-night brewing.\n\n" +
                "The second hour contains: a widower's grief, a choice about memory, a short illustrated dream.\n\n" +
                "At any point, you can take a Soft Day, enable Gentle Mode, or adjust any settings.\n\n" +
                "There is no combat. There are no failure screens. There are only choices.\n\n" +
                "The cat will be there.",
                "ستجعلك هذه اللعبة تشعر بأشياء. بعض تلك المشاعر ثقيلة.\n\n" +
                "تحتوي هذه الساعة الأولى على: افتتاح متجر، وأول معاملة، وتحضير شاي في وقت متأخّر من الليل.\n\n" +
                "تحتوي الساعة الثانية على: حزن أرمل، وخيار حول ذكرى، وحلم قصير مرسوم.\n\n" +
                "في أيّ لحظة، يمكنك أخذ يوم هادئ، أو تفعيل الوضع اللطيف، أو تعديل أيّ إعدادات.\n\n" +
                "لا قتال. لا شاشات إخفاق. هناك فقط خيارات.\n\n" +
                "القطة ستكون هناك."),

            // ── Onboarding walkthrough steps (OnboardingOverlay.DefaultSteps) ──
            // EN values MUST match the code copy verbatim (incl. \n and <b> tags);
            // AR values are plain text (shaped per line at display).
            ["onboard.welcome.headline"] = ("Welcome to the Hollow", "أهلًا بك في الجوف"),
            ["onboard.welcome.body"]     = (
                "You've inherited a little memory-shop in a quiet autumn village.\n" +
                "No combat. No fail screens. Just people — and the memories they trust you to keep.",
                "لقد ورثتَ متجرًا صغيرًا للذكريات في قرية خريفية هادئة.\n" +
                "لا قتال. لا شاشات إخفاق. فقط أناس — والذكريات التي يأتمنونك عليها."),
            ["onboard.move.headline"]    = ("Move", "تحرّك"),
            ["onboard.move.body"]        = (
                "Wander at your own pace. Someone is waiting at the door of the Hollow.",
                "تجوّل على راحتك. هناك من ينتظر عند باب الجوف."),
            ["onboard.move.caption"]     = ("Move  ·  Arrows / Left Stick", "تحرّك  ·  الأسهم / العصا اليسرى"),
            ["onboard.interact.headline"] = ("Interact", "تفاعل"),
            ["onboard.interact.body"]    = (
                "A soft golden prompt means you can act — open a door, take a memory, pour the tea.",
                "الإشارة الذهبية اللطيفة تعني أنه يمكنك التصرّف — افتح بابًا، خذ ذكرى، اسكب الشاي."),
            ["onboard.interact.caption"] = ("Interact", "تفاعل"),
            ["onboard.polish.headline"]  = ("Polish a memory", "لمّع ذكرى"),
            ["onboard.polish.body"]      = (
                "Hold the left mouse button and trace slow circles over the glowing orb.\n" +
                "Slower is kinder. Cover all four sides — like a kindness with four corners.",
                "اضغط مع الاستمرار على زر الفأرة الأيسر وارسم دوائر بطيئة فوق الجوهرة المتوهّجة.\n" +
                "الأبطأ ألطف. غطِّ الجوانب الأربعة كلها — كلطفٍ بأربعة أركان."),
            ["onboard.polish.caption"]   = ("Draw slow circles", "ارسم دوائر بطيئة"),
            ["onboard.easy.headline"]    = ("Take it easy", "خُذ الأمور برويّة"),
            ["onboard.easy.body"]        = (
                "Press <b>Esc</b> any time for Settings — Gentle Mode, Auto-Complete, and language.\n" +
                "Press <b>H</b> for the full controls whenever you need them.",
                "اضغط Esc في أي وقت للإعدادات — الوضع اللطيف، الإكمال التلقائي، واللغة.\n" +
                "اضغط H لكل أدوات التحكم وقتما تحتاجها."),
            ["onboard.easy.caption"]     = ("Pause  ·  Settings  ·  Comfort", "إيقاف  ·  إعدادات  ·  راحة"),
            ["onboard.ready.headline"]   = ("You're ready", "أنت جاهز"),
            ["onboard.ready.body"]       = (
                "Walk to the door of the Hollow when you're ready.\n\n" +
                "There is no wrong way to keep a memory.\nThere is only the gentle way, and the others.\n\n— Marin",
                "امشِ إلى باب الجوف عندما تكون مستعدًّا.\n\n" +
                "لا توجد طريقة خاطئة لحفظ ذكرى.\nهناك فقط الطريقة اللطيفة، والطرق الأخرى.\n\n— مارين"),

            // ── Mini-game tutorial (MiniGameTutorialUI) ──
            // EN keeps inline <b> tags (English appearance unchanged); AR is plain.
            ["minigame.generic.headline"]     = ("Work the orb", "اشتغل على الجوهرة"),
            ["minigame.generic.instructions"] = ("Hold <b>Left Mouse</b> and move slowly across the orb.",
                                                 "اضغط مع الاستمرار على زر الفأرة الأيسر وتحرّك ببطء عبر الجوهرة."),
            ["minigame.generic.hint"]         = ("Take your time — the orb cannot break from gentleness.",
                                                 "خُذ وقتك — لا يمكن للجوهرة أن تنكسر من اللطف."),
            ["minigame.polish.headline"]      = ("Polish the memory", "لمّع الذكرى"),
            ["minigame.polish.instructions"]  = ("Hold <b>Left Mouse</b> and draw <b>slow circles</b> around the <b>glowing orb</b>.",
                                                 "اضغط مع الاستمرار على زر الفأرة الأيسر وارسم دوائر بطيئة حول الجوهرة المتوهّجة."),
            ["minigame.polish.hint"]          = ("Slower is better. Cover all four corners.",
                                                 "الأبطأ أفضل. غطِّ الأركان الأربعة كلها."),
            ["minigame.polish.milestone"]     = ("<b>Halfway</b> — the orb is warming.", "في منتصف الطريق — الجوهرة تدفأ."),
            ["minigame.polish.reveal"]        = ("<b>Nearly there</b> — keep the circles gentle.", "اقتربت — أبقِ الدوائر لطيفة."),
            ["minigame.polish.friction"]      = ("<b>Slower</b> — let the orb hear you.", "أبطأ — دع الجوهرة تسمعك."),
            ["minigame.cleanse.headline"]     = ("Cleanse the orb", "طهّر الجوهرة"),
            ["minigame.cleanse.instructions"] = ("Hold <b>Left Mouse</b> · Trace the cracks · <b>Don't cross the core</b>",
                                                 "اضغط مع الاستمرار على زر الفأرة الأيسر · تتبّع الشقوق · لا تعبر القلب"),
            ["minigame.cleanse.hint"]         = ("Trace each crack from end to end. Avoid the warm centre.",
                                                 "تتبّع كل شق من طرفه إلى طرفه. تجنّب المركز الدافئ."),
            ["minigame.cleanse.sealed"]       = ("<b>{0} of 4</b> cracks sealed.", "تم ختم {0} من 4 شقوق."),
            ["minigame.cleanse.friction"]     = ("<b>Pull back</b> — that is the warm centre.", "تراجع — ذلك هو المركز الدافئ."),
            ["minigame.autocomplete_suffix"]  = ("  (Auto-Complete is available below.)", "  (الإكمال التلقائي متاح بالأسفل.)"),
            ["minigame.skip_autocomplete"]    = ("Skip · Auto-Complete", "تخطٍّ · إكمال تلقائي"),
        };
    }
}
