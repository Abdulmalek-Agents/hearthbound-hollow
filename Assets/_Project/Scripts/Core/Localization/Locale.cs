// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Core / Localization / Locale
//
// Phase 60 — Arabic Localization.
//
// The Locale enum is the canonical "what language is the game in?" key. It
// drives:
//   • LocalizationService string lookups (UI, HUD, menus, prompts)
//   • LocalizedDialogueTable lookups (per-lineId text per language)
//   • VoicePlayer per-language clip resolution (Audio/Voice/<lang>/<char>/<lineId>.wav)
//   • RTL text shaping + UI layout mirroring
//
// Adding a third language (e.g. French, Spanish, Japanese) is intentionally a
// one-line enum addition + a translation-table fill-in. The runtime is
// language-count-agnostic — no switch statements over Locale outside of
// fallback defaults.
//
// D-060 (Phase 60 — Arabic Localization MVP):
//   The English authored text is the SOURCE OF TRUTH. Every other locale is
//   a translation against that source. If a translation is missing at
//   runtime, the English string is rendered and a one-time warning is logged
//   — the game NEVER ships with an empty label.

namespace HearthboundHollow.Core
{
    /// <summary>
    /// Supported in-game languages. Add entries here, fill in the translation
    /// tables (Assets/_Project/Localization/&lt;name&gt;.json), and the
    /// language selector picks them up automatically.
    /// </summary>
    public enum Locale
    {
        /// <summary>English (United Kingdom). Source-of-truth locale.</summary>
        English = 0,

        /// <summary>العربية الفصحى — Modern Standard Arabic with cozy literary register.</summary>
        Arabic  = 1,
    }

    /// <summary>
    /// Static helpers around <see cref="Locale"/>.
    /// </summary>
    public static class LocaleInfo
    {
        /// <summary>
        /// ISO 639-1 language code (used for PlayerPrefs persistence, voice
        /// folder naming, and Yarn line metadata).
        /// </summary>
        public static string IsoCode(Locale locale) => locale switch
        {
            Locale.Arabic  => "ar",
            _              => "en",
        };

        /// <summary>
        /// Parse an ISO 639-1 code back into a Locale. Unknown codes fall
        /// back to English (the source-of-truth locale).
        /// </summary>
        public static Locale FromIsoCode(string iso)
        {
            if (string.IsNullOrWhiteSpace(iso)) return Locale.English;
            iso = iso.Trim().ToLowerInvariant();
            if (iso.StartsWith("ar")) return Locale.Arabic;
            return Locale.English;
        }

        /// <summary>
        /// Native-name (displayed in the language toggle on the main menu —
        /// always shown in the target language's own script, never translated).
        /// </summary>
        public static string NativeName(Locale locale) => locale switch
        {
            Locale.Arabic  => "العربية",
            _              => "English",
        };

        /// <summary>
        /// True for languages that render right-to-left. The UI layer uses
        /// this to mirror horizontal layout groups, swap text alignment, and
        /// shape Arabic glyphs into their connected presentation forms.
        /// </summary>
        public static bool IsRightToLeft(Locale locale) => locale == Locale.Arabic;

        /// <summary>
        /// Folder name used under <c>Assets/_Project/Audio/Voice/&lt;locale&gt;/</c>
        /// for per-language voice clips. Mirrors the Piper-TTS voice generation
        /// pipeline (Tools/generate_voices.sh).
        /// </summary>
        public static string VoiceFolder(Locale locale) => locale switch
        {
            Locale.Arabic  => "ar",
            _              => "en",
        };
    }
}
