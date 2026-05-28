// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Core / Localization / LocalizationService
//
// Phase 60 — Arabic Localization MVP.
//
// Central service registered in ServiceLocator. Three concerns:
//
//   1. UI strings — short labels, button text, prompts, HUD chips. Looked up
//      by a stable string KEY. Source of truth lives in
//        Assets/_Project/Localization/Resources/loc.<iso>.json
//      (e.g. loc.en.json, loc.ar.json). The JSON is a flat
//      `{ "key.path": "string", ... }` map; missing keys at runtime fall
//      back to English with a one-time warning (D-060).
//
//   2. Dialogue lines — long sentences spoken by villagers, looked up by the
//      same stable lineId the VoicePlayer uses. Source of truth lives in
//        Assets/_Project/Localization/Resources/dialogue.<iso>.json
//      so a translator can edit them without touching code.
//
//   3. Locale persistence — PlayerPrefs key + auto-detect from
//      Application.systemLanguage on first launch. The chosen locale survives
//      hard-quit + clean-uninstall.
//
// The service is a plain C# class (not a MonoBehaviour) — it is constructed
// by SettingsServiceBootstrap on app start and lives for the whole session.
//
// USAGE (runtime):
//
//   var loc = ServiceLocator.Get<LocalizationService>();
//   string title = loc.Get("menu.main.title");                // UI string
//   string spoken = loc.GetDialogue("doris_m1_greet_01", english); // dialogue
//   loc.SetLocale(Locale.Arabic);
//
// USAGE (component):
//
//   [SerializeField] string locKey = "menu.main.cta";
//   TextMeshProUGUI label;
//   void OnEnable() {
//       EventBus.Subscribe<LocaleChangedEvent>(_ => RefreshText());
//       RefreshText();
//   }
//   void RefreshText() => label.text = ServiceLocator.Get<LocalizationService>().Get(locKey);
//
// See `LocalizedText.cs` (UI asmdef) for the canonical TMP binding.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace HearthboundHollow.Core
{
    public class LocalizationService
    {
        // ───── Constants ───────────────────────────────────────────────

        /// <summary>
        /// PlayerPrefs key — stores the ISO-639-1 code of the player's chosen
        /// language. Persisted across sessions independent of save slot.
        /// </summary>
        private const string K_LocaleIso = "hh.locale.iso";

        /// <summary>
        /// Filename (sans .json) under Resources/ holding UI strings per locale.
        /// </summary>
        private const string ResourceUiPrefix = "Localization/loc.";

        /// <summary>
        /// Filename (sans .json) under Resources/ holding dialogue lines per locale.
        /// </summary>
        private const string ResourceDialoguePrefix = "Localization/dialogue.";

        // ───── State ───────────────────────────────────────────────────

        /// <summary>The active locale. Drives every Get() lookup.</summary>
        public Locale CurrentLocale { get; private set; } = Locale.English;

        /// <summary>True for right-to-left languages (Arabic, future Hebrew/Persian).</summary>
        public bool IsRightToLeft => LocaleInfo.IsRightToLeft(CurrentLocale);

        // Tables keyed by Locale → key → string. Lazy-loaded on first Get().
        private readonly Dictionary<Locale, Dictionary<string, string>> _uiTables = new();
        private readonly Dictionary<Locale, Dictionary<string, string>> _dialogueTables = new();

        // One-time warning de-dup for missing keys (avoid Console spam).
        private readonly HashSet<string> _warnedMissingKeys = new();
        private readonly HashSet<string> _warnedMissingLines = new();

        /// <summary>
        /// Fired immediately when the locale changes — also published on the
        /// global <see cref="EventBus"/> as <see cref="LocaleChangedEvent"/>.
        /// </summary>
        public event Action<Locale, Locale> OnLocaleChanged;

        // ───── Construction ────────────────────────────────────────────

        public LocalizationService()
        {
            CurrentLocale = LoadOrDetectLocale();
            // Eagerly load the English table so missing-key fallbacks work
            // even before the player has touched the language toggle.
            EnsureUiTableLoaded(Locale.English);
            EnsureUiTableLoaded(CurrentLocale);
        }

        // ───── Locale selection + persistence ──────────────────────────

        /// <summary>
        /// Switch the active language and publish the change event. Idempotent.
        /// </summary>
        public void SetLocale(Locale locale)
        {
            if (locale == CurrentLocale) return;
            var prev = CurrentLocale;
            CurrentLocale = locale;
            PlayerPrefs.SetString(K_LocaleIso, LocaleInfo.IsoCode(locale));
            PlayerPrefs.Save();

            // Lazy-load the new locale's table so the first Get() after the
            // switch is hot.
            EnsureUiTableLoaded(locale);
            // Clear missing-key warnings on locale change — different tables
            // legitimately have different coverage during translation work.
            _warnedMissingKeys.Clear();
            _warnedMissingLines.Clear();

            Hh.Log(LogCategory.UI,
                $"LocalizationService: locale changed {prev} → {locale} ({LocaleInfo.NativeName(locale)}).");

            OnLocaleChanged?.Invoke(prev, locale);
            EventBus.Publish(new LocaleChangedEvent(prev, locale));
        }

        /// <summary>
        /// First-launch heuristic — read the persisted ISO code, or if absent,
        /// auto-detect from Application.systemLanguage. Arabic system locale
        /// auto-opts-in; everyone else gets English.
        /// </summary>
        private Locale LoadOrDetectLocale()
        {
            var saved = PlayerPrefs.GetString(K_LocaleIso, null);
            if (!string.IsNullOrEmpty(saved))
                return LocaleInfo.FromIsoCode(saved);

            // First launch — auto-detect from system locale.
            return Application.systemLanguage == SystemLanguage.Arabic
                ? Locale.Arabic
                : Locale.English;
        }

        // ───── UI string lookup ────────────────────────────────────────

        /// <summary>
        /// Look up a UI string by stable key. Falls back to English then to
        /// the key itself, with a one-time warning per missing key.
        ///
        /// Example keys (canonical, registered in loc.en.json):
        ///   menu.main.title                 → "Hearthbound Hollow"
        ///   menu.main.cta.open_hollow       → "Open The Hollow"
        ///   pause.title                     → "Paused"
        ///   pause.hint                      → "Take a breath. The Hollow will wait."
        ///   hud.day_label_fmt               → "Day {0}"
        ///   hud.coin_label_fmt              → "{0} c"
        ///   settings.gentle_mode            → "Gentle Mode"
        /// </summary>
        public string Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;

            // Active locale first.
            EnsureUiTableLoaded(CurrentLocale);
            if (_uiTables.TryGetValue(CurrentLocale, out var t) &&
                t != null && t.TryGetValue(key, out var s) &&
                !string.IsNullOrEmpty(s))
            {
                return s;
            }

            // English fallback (the source of truth).
            if (CurrentLocale != Locale.English)
            {
                EnsureUiTableLoaded(Locale.English);
                if (_uiTables.TryGetValue(Locale.English, out var en) &&
                    en != null && en.TryGetValue(key, out var es) &&
                    !string.IsNullOrEmpty(es))
                {
                    WarnMissingKey(key);
                    return es;
                }
            }

            // Last resort: the key itself. Loud enough that the missing entry
            // is obvious in playtest screenshots, but never a hard crash.
            WarnMissingKey(key);
            return key;
        }

        /// <summary>
        /// Composite-format flavor of <see cref="Get(string)"/>: looks up
        /// the format string, then runs <c>string.Format</c> with the args.
        /// </summary>
        public string Format(string key, params object[] args)
        {
            var fmt = Get(key);
            if (args == null || args.Length == 0) return fmt;
            try { return string.Format(fmt, args); }
            catch (FormatException) { return fmt; }
        }

        /// <summary>True if the active table contains a non-empty value for this key.</summary>
        public bool HasKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;
            EnsureUiTableLoaded(CurrentLocale);
            return _uiTables.TryGetValue(CurrentLocale, out var t) &&
                   t != null && t.TryGetValue(key, out var s) &&
                   !string.IsNullOrEmpty(s);
        }

        // ───── Dialogue line lookup ────────────────────────────────────

        /// <summary>
        /// Look up the translated text of a dialogue line by its stable
        /// <paramref name="lineId"/> (e.g. <c>doris_m1_greet_01</c>). Falls
        /// back to the supplied <paramref name="englishOriginal"/> when no
        /// translation exists — guarantees the typewriter never goes empty.
        /// </summary>
        public string GetDialogue(string lineId, string englishOriginal)
        {
            // English is the source of truth — return the original verbatim.
            if (CurrentLocale == Locale.English || string.IsNullOrEmpty(lineId))
                return englishOriginal;

            EnsureDialogueTableLoaded(CurrentLocale);
            if (_dialogueTables.TryGetValue(CurrentLocale, out var t) &&
                t != null && t.TryGetValue(lineId, out var s) &&
                !string.IsNullOrEmpty(s))
            {
                return s;
            }

            WarnMissingLine(lineId);
            return englishOriginal;
        }

        /// <summary>
        /// Look up a translated speaker name (the label shown above dialogue
        /// lines). Falls back to the English label. Keys follow the pattern
        /// <c>speaker.&lt;lowercase_name&gt;</c>.
        /// </summary>
        public string GetSpeakerName(string englishSpeaker)
        {
            if (string.IsNullOrEmpty(englishSpeaker)) return englishSpeaker;
            var key = "speaker." + englishSpeaker.Trim().ToLowerInvariant();
            // Don't warn on speaker-name misses — many speakers may legitimately
            // share a name across locales (e.g. "Pickle" stays "Pickle").
            EnsureUiTableLoaded(CurrentLocale);
            if (_uiTables.TryGetValue(CurrentLocale, out var t) &&
                t != null && t.TryGetValue(key, out var s) &&
                !string.IsNullOrEmpty(s))
            {
                return s;
            }
            return englishSpeaker;
        }

        // ───── Table loaders (JSON in Resources/Localization/) ─────────

        private void EnsureUiTableLoaded(Locale locale)
        {
            if (_uiTables.ContainsKey(locale)) return;
            var iso = LocaleInfo.IsoCode(locale);
            var resourcePath = ResourceUiPrefix + iso;
            var ta = Resources.Load<TextAsset>(resourcePath);
            if (ta == null)
            {
                Hh.Warn(LogCategory.UI,
                    $"LocalizationService: missing UI table Resources/{resourcePath}.json " +
                    $"for locale {locale}. Strings will fall back to English (or key).");
                _uiTables[locale] = new Dictionary<string, string>();
                return;
            }
            _uiTables[locale] = ParseFlatJson(ta.text);
            Hh.Log(LogCategory.UI,
                $"LocalizationService: loaded {_uiTables[locale].Count} UI strings for {locale} from Resources/{resourcePath}.");
        }

        private void EnsureDialogueTableLoaded(Locale locale)
        {
            if (_dialogueTables.ContainsKey(locale)) return;
            var iso = LocaleInfo.IsoCode(locale);
            var resourcePath = ResourceDialoguePrefix + iso;
            var ta = Resources.Load<TextAsset>(resourcePath);
            if (ta == null)
            {
                // Dialogue tables are optional for non-source locales —
                // missing == "no translation yet" not an error.
                _dialogueTables[locale] = new Dictionary<string, string>();
                return;
            }
            _dialogueTables[locale] = ParseFlatJson(ta.text);
            Hh.Log(LogCategory.UI,
                $"LocalizationService: loaded {_dialogueTables[locale].Count} dialogue lines for {locale} from Resources/{resourcePath}.");
        }

        /// <summary>
        /// Minimal JSON parser for the flat <c>{ "key": "value", ... }</c>
        /// translation table format. We deliberately don't pull a JSON
        /// dependency into Core — UnityEngine.JsonUtility doesn't support
        /// arbitrary-key dictionaries, so we hand-roll a tolerant tokenizer
        /// that handles unicode (Arabic), nested escapes, and trailing
        /// commas.
        /// </summary>
        internal static Dictionary<string, string> ParseFlatJson(string json)
        {
            var dict = new Dictionary<string, string>(256);
            if (string.IsNullOrEmpty(json)) return dict;

            int i = 0;
            int n = json.Length;

            // Skip to first {
            while (i < n && json[i] != '{') i++;
            if (i >= n) return dict;
            i++;

            while (i < n)
            {
                SkipWhitespaceAndCommas(json, ref i, n);
                if (i >= n || json[i] == '}') break;

                if (json[i] != '"') { i++; continue; }
                string key = ReadJsonString(json, ref i, n);

                SkipWhitespaceAndCommas(json, ref i, n);
                if (i >= n || json[i] != ':') break;
                i++;
                SkipWhitespaceAndCommas(json, ref i, n);

                if (i >= n || json[i] != '"') { /* skip non-string values */ continue; }
                string value = ReadJsonString(json, ref i, n);

                if (!string.IsNullOrEmpty(key)) dict[key] = value;
            }

            return dict;
        }

        private static void SkipWhitespaceAndCommas(string s, ref int i, int n)
        {
            while (i < n)
            {
                char c = s[i];
                if (c == ' ' || c == '\t' || c == '\r' || c == '\n' || c == ',') { i++; continue; }
                // Skip line comments — supported in our translation files for
                // translator notes (// this line is a placeholder).
                if (c == '/' && i + 1 < n && s[i + 1] == '/')
                {
                    while (i < n && s[i] != '\n') i++;
                    continue;
                }
                break;
            }
        }

        private static string ReadJsonString(string s, ref int i, int n)
        {
            if (i >= n || s[i] != '"') return string.Empty;
            i++; // opening "
            var sb = new System.Text.StringBuilder(64);
            while (i < n)
            {
                char c = s[i++];
                if (c == '"') return sb.ToString();
                if (c == '\\' && i < n)
                {
                    char esc = s[i++];
                    switch (esc)
                    {
                        case '"':  sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/':  sb.Append('/');  break;
                        case 'n':  sb.Append('\n'); break;
                        case 'r':  sb.Append('\r'); break;
                        case 't':  sb.Append('\t'); break;
                        case 'b':  sb.Append('\b'); break;
                        case 'f':  sb.Append('\f'); break;
                        case 'u':
                            if (i + 4 <= n &&
                                int.TryParse(s.Substring(i, 4),
                                    System.Globalization.NumberStyles.HexNumber,
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    out int code))
                            {
                                sb.Append((char)code);
                                i += 4;
                            }
                            break;
                        default:   sb.Append(esc); break;
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        // ───── Diagnostics ─────────────────────────────────────────────

        private void WarnMissingKey(string key)
        {
            if (_warnedMissingKeys.Add(key))
            {
                Hh.Warn(LogCategory.UI,
                    $"LocalizationService: missing UI key '{key}' for {CurrentLocale}. " +
                    "Falling back to English (or key text). " +
                    "Add it to Assets/_Project/Localization/Resources/loc.<iso>.json.");
            }
        }

        private void WarnMissingLine(string lineId)
        {
            if (_warnedMissingLines.Add(lineId))
            {
                Hh.Warn(LogCategory.UI,
                    $"LocalizationService: missing dialogue translation for lineId '{lineId}' " +
                    $"in locale {CurrentLocale}. Falling back to English original. " +
                    "Add it to Assets/_Project/Localization/Resources/dialogue.<iso>.json.");
            }
        }

        /// <summary>
        /// Test / diagnostic hook — count of loaded entries for the active locale.
        /// </summary>
        public (int uiCount, int dialogueCount) GetLoadedCounts()
        {
            EnsureUiTableLoaded(CurrentLocale);
            EnsureDialogueTableLoaded(CurrentLocale);
            int u = _uiTables.TryGetValue(CurrentLocale, out var t1) && t1 != null ? t1.Count : 0;
            int d = _dialogueTables.TryGetValue(CurrentLocale, out var t2) && t2 != null ? t2.Count : 0;
            return (u, d);
        }
    }
}
