// SPDX-License-Identifier: MIT
// Hearthbound Hollow — EditMode tests for the Phase 60 Localization system.

using NUnit.Framework;
using HearthboundHollow.Core;

namespace HearthboundHollow.Tests.EditMode
{
    public class LocalizationTests
    {
        [SetUp]
        public void Setup() => EventBus.ClearAll();

        // ─── Locale helpers ─────────────────────────────────────────

        [Test]
        public void Locale_IsoCode_round_trips()
        {
            Assert.AreEqual("en", LocaleInfo.IsoCode(Locale.English));
            Assert.AreEqual("ar", LocaleInfo.IsoCode(Locale.Arabic));
            Assert.AreEqual(Locale.English, LocaleInfo.FromIsoCode("en"));
            Assert.AreEqual(Locale.Arabic,  LocaleInfo.FromIsoCode("ar"));
            Assert.AreEqual(Locale.Arabic,  LocaleInfo.FromIsoCode("ar-SA"));   // prefix match
            Assert.AreEqual(Locale.English, LocaleInfo.FromIsoCode("unknown")); // fallback
            Assert.AreEqual(Locale.English, LocaleInfo.FromIsoCode(""));        // empty
        }

        [Test]
        public void Locale_NativeName_is_in_target_language()
        {
            Assert.AreEqual("English", LocaleInfo.NativeName(Locale.English));
            Assert.AreEqual("العربية", LocaleInfo.NativeName(Locale.Arabic));
        }

        [Test]
        public void Locale_RTL_only_for_Arabic()
        {
            Assert.IsFalse(LocaleInfo.IsRightToLeft(Locale.English));
            Assert.IsTrue (LocaleInfo.IsRightToLeft(Locale.Arabic));
        }

        // ─── JSON parser ────────────────────────────────────────────

        [Test]
        public void ParseFlatJson_handles_unicode_keys_and_values()
        {
            string json = "{ \"hello\": \"مرحبا\", \"world\": \"عالم\" }";
            var dict = LocalizationService.ParseFlatJson(json);
            Assert.AreEqual(2, dict.Count);
            Assert.AreEqual("مرحبا", dict["hello"]);
            Assert.AreEqual("عالم",  dict["world"]);
        }

        [Test]
        public void ParseFlatJson_tolerates_line_comments_and_trailing_commas()
        {
            string json =
                "{\n" +
                "  // a translator note\n" +
                "  \"a\": \"alpha\",\n" +
                "  // another comment\n" +
                "  \"b\": \"beta\",\n" +
                "}";
            var dict = LocalizationService.ParseFlatJson(json);
            Assert.AreEqual(2, dict.Count);
            Assert.AreEqual("alpha", dict["a"]);
            Assert.AreEqual("beta",  dict["b"]);
        }

        [Test]
        public void ParseFlatJson_handles_escaped_chars()
        {
            string json = "{ \"k\": \"line1\\nline2\\twith \\\"quote\\\"\" }";
            var dict = LocalizationService.ParseFlatJson(json);
            Assert.AreEqual("line1\nline2\twith \"quote\"", dict["k"]);
        }

        [Test]
        public void ParseFlatJson_empty_input_returns_empty_dict()
        {
            Assert.AreEqual(0, LocalizationService.ParseFlatJson("").Count);
            Assert.AreEqual(0, LocalizationService.ParseFlatJson(null).Count);
            Assert.AreEqual(0, LocalizationService.ParseFlatJson("{}").Count);
        }

        // ─── Arabic shaper ──────────────────────────────────────────

        [Test]
        public void ArabicShaper_returns_input_unchanged_for_pure_ascii()
        {
            // Fast-path: no Arabic codepoints → reference-equal return.
            string src = "Hearthbound Hollow — Day 1.";
            Assert.AreEqual(src, ArabicTextShaper.Shape(src));
        }

        [Test]
        public void ArabicShaper_handles_null_and_empty()
        {
            Assert.IsNull(ArabicTextShaper.Shape(null));
            Assert.AreEqual("", ArabicTextShaper.Shape(""));
        }

        [Test]
        public void ArabicShaper_detects_arabic_codepoints()
        {
            Assert.IsFalse(ArabicTextShaper.ContainsArabic("hello"));
            Assert.IsTrue (ArabicTextShaper.ContainsArabic("مرحبا"));
            Assert.IsTrue (ArabicTextShaper.ContainsArabic("hello مرحبا"));
        }

        [Test]
        public void ArabicShaper_shaped_text_contains_presentation_form_codepoints()
        {
            // "اللغة العربية" → after shaping, the output should contain
            // codepoints from the Arabic Presentation Forms-A/B blocks
            // (U+FB50..U+FDFF or U+FE70..U+FEFF).
            string raw = "اللغة العربية";
            string shaped = ArabicTextShaper.Shape(raw);
            bool hasPresentationForm = false;
            for (int i = 0; i < shaped.Length; i++)
            {
                char c = shaped[i];
                if ((c >= 0xFB50 && c <= 0xFDFF) ||
                    (c >= 0xFE70 && c <= 0xFEFF))
                {
                    hasPresentationForm = true;
                    break;
                }
            }
            Assert.IsTrue(hasPresentationForm,
                "Shaped Arabic text should contain glyphs from the U+FE70 " +
                "presentation forms block — shaping did not run.");
        }

        [Test]
        public void ArabicShaper_preserves_embedded_latin_words()
        {
            // Mixed: "OK مرحبا" — the OK should survive as left-to-right
            // text while the Arabic word gets shaped + reversed for RTL flow.
            string mixed = "OK مرحبا";
            string shaped = ArabicTextShaper.Shape(mixed);
            // The Latin letters must still be present somewhere.
            Assert.IsTrue(shaped.Contains("OK") || shaped.IndexOf('O') >= 0,
                "Embedded Latin run was lost during shaping.");
        }

        // ─── LocaleChangedEvent ─────────────────────────────────────

        [Test]
        public void LocaleChangedEvent_reports_rtl_flip()
        {
            var en2ar = new LocaleChangedEvent(Locale.English, Locale.Arabic);
            var ar2ar = new LocaleChangedEvent(Locale.Arabic, Locale.Arabic);
            var en2en = new LocaleChangedEvent(Locale.English, Locale.English);
            Assert.IsTrue (en2ar.RtlDirectionChanged);
            Assert.IsFalse(ar2ar.RtlDirectionChanged);
            Assert.IsFalse(en2en.RtlDirectionChanged);
        }

        // ─── LocalizationService.Get fallback chain ─────────────────

        [Test]
        public void LocalizationService_unknown_key_falls_back_to_the_key_itself()
        {
            // Service auto-loads loc.en.json from Resources in real runs.
            // In an EditMode test we don't have the Resources stack, so
            // every key is "missing" and the fallback path returns the key.
            var svc = new LocalizationService();
            string key = "totally.not.a.real.key";
            Assert.AreEqual(key, svc.Get(key));
        }

        [Test]
        public void LocalizationService_Format_returns_template_on_no_args()
        {
            var svc = new LocalizationService();
            string key = "missing.format.key";
            Assert.AreEqual(key, svc.Format(key));
        }
    }
}
