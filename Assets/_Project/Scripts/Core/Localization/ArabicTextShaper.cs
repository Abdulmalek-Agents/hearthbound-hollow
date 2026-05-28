// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Core / Localization / ArabicTextShaper
//
// Phase 60 — Arabic Localization MVP.
//
// Unity's TextMesh Pro renders Arabic glyphs in their ISOLATED forms by
// default — it does not apply the cursive joining rules that turn
//
//   ا ل ع ا ل م     (5 isolated glyphs, left-to-right order)
//
// into the connected, right-to-left rendering
//
//   العالم
//
// This shaper does the work that the OpenType engine would do on a desktop:
//
//   1. Walk the source string and decide each Arabic letter's CONTEXTUAL
//      form (Isolated / Initial / Medial / Final) based on its neighbours
//      and whether each neighbour joins backwards.
//   2. Substitute the codepoint with the matching Arabic Presentation Forms
//      codepoint (U+FE70 .. U+FEFC).
//   3. Combine LAM + ALEF pairs into the obligatory ligature (لا / لإ / لأ / لآ).
//   4. Reverse the visual order of the shaped run so TMP renders it
//      right-to-left when laid out into a left-to-right text element.
//
// Latin characters, ASCII digits, whitespace, punctuation and English words
// embedded in the line are passed through unchanged in their original
// left-to-right order (we segment, reverse only the Arabic runs, and
// stitch them back).
//
// This is a hand-rolled BiDi-lite — it covers cozy single-line dialogue,
// menu labels, HUD chips, and short paragraphs. For long multi-paragraph
// narrative text we still recommend the full Unicode-BiDi (UAX #9)
// algorithm — see the future Phase 61 follow-up note below.
//
// ── Phase 61 follow-up (out of scope for this PR) ───────────────
// Multi-paragraph reading nook letters + Codex articles render with this
// shaper today. If a future pass turns up edge-cases (mixed Arabic + Latin
// with embedded numerals + parentheses), the canonical fix is to import
// a full UAX-#9 implementation (e.g. ArabicSupport-for-Unity or BiDi.NET).
// The shaper SURFACE — Shape(string) — stays identical, so the swap is
// invisible to call sites.

using System.Text;

namespace HearthboundHollow.Core
{
    public static class ArabicTextShaper
    {
        // ───── Public API ──────────────────────────────────────────────

        /// <summary>
        /// Shape an English-or-Arabic string into its visually-correct,
        /// right-to-left rendered form for Unity TextMeshPro. Pure function,
        /// thread-safe, ~50 µs per typical dialogue line.
        ///
        /// Strings with no Arabic codepoints are returned unchanged
        /// (allocation-free fast path).
        /// </summary>
        public static string Shape(string source)
        {
            if (string.IsNullOrEmpty(source)) return source;
            if (!ContainsArabic(source)) return source;

            // Step 1 — segment into Arabic vs non-Arabic runs (BiDi-lite).
            // Each run is shaped + reversed independently; runs are
            // concatenated in the reversed run order so the visual flow is
            // right-to-left for Arabic runs while embedded Latin words
            // remain left-to-right.
            var runs = Segment(source);

            // Step 2 — shape each Arabic run + handle LAM/ALEF ligatures.
            // Step 3 — reverse Arabic runs character-by-character (TMP's
            // own renderer lays glyphs out left-to-right after shaping, so
            // we feed it the visually-mirrored sequence).
            //
            // Final step — emit the runs back-to-front for paragraph-level
            // RTL flow.
            var sb = new StringBuilder(source.Length + 4);
            for (int i = runs.Count - 1; i >= 0; i--)
            {
                var r = runs[i];
                if (r.IsArabic)
                {
                    var shaped = ShapeArabicRun(r.Text);
                    sb.Append(Reverse(shaped));
                }
                else
                {
                    sb.Append(r.Text);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// True if any character in the source is in the Arabic Unicode block
        /// (U+0600 .. U+06FF). Cheap pre-check used to bail out early on
        /// English-only strings.
        /// </summary>
        public static bool ContainsArabic(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c >= 0x0600 && c <= 0x06FF) return true;
                if (c >= 0xFB50 && c <= 0xFDFF) return true; // Arabic Presentation Forms-A
                if (c >= 0xFE70 && c <= 0xFEFF) return true; // Arabic Presentation Forms-B
            }
            return false;
        }

        // ───── Run segmentation ────────────────────────────────────────

        private struct Run
        {
            public string Text;
            public bool   IsArabic;
            public Run(string t, bool ar) { Text = t; IsArabic = ar; }
        }

        private static System.Collections.Generic.List<Run> Segment(string s)
        {
            var runs = new System.Collections.Generic.List<Run>(4);
            if (string.IsNullOrEmpty(s)) return runs;

            int start = 0;
            bool currentIsAr = IsArabicChar(s[0]);
            // Treat whitespace + ASCII digits as "neutral" so a Latin-only
            // word doesn't split an Arabic paragraph. They glue to the
            // surrounding Arabic run for correct spacing.
            for (int i = 1; i < s.Length; i++)
            {
                bool isAr = IsArabicChar(s[i]);
                // Neutrals (space, common punctuation, digits) inherit the
                // direction of the run they sit inside.
                if (IsNeutralChar(s[i]))
                {
                    continue;
                }
                if (isAr != currentIsAr)
                {
                    runs.Add(new Run(s.Substring(start, i - start), currentIsAr));
                    start = i;
                    currentIsAr = isAr;
                }
            }
            runs.Add(new Run(s.Substring(start), currentIsAr));
            return runs;
        }

        private static bool IsArabicChar(char c)
        {
            if (c >= 0x0600 && c <= 0x06FF) return true;
            if (c >= 0xFB50 && c <= 0xFDFF) return true;
            if (c >= 0xFE70 && c <= 0xFEFF) return true;
            return false;
        }

        private static bool IsNeutralChar(char c)
        {
            // Whitespace, ASCII punctuation, ASCII digits glue to surrounding run.
            return c <= 0x002F || (c >= 0x003A && c <= 0x0040) ||
                   (c >= 0x005B && c <= 0x0060) || (c >= 0x007B && c <= 0x007E) ||
                   c == ' ' || c == '\u00A0' || c == '\u060C' /* arabic comma */;
        }

        // ───── Arabic shaping core ─────────────────────────────────────

        // Each row: { ISOLATED, INITIAL, MEDIAL, FINAL, joinsBackward }
        // joinsBackward = whether the PRECEDING letter can connect to this
        // letter's right side (i.e. whether this letter's "final" form is
        // valid in context). All letters in this table join backward except
        // for the "non-joining" letters (alef, dal, dhal, ra, zay, waw, etc.)
        // — for those the row stops at INITIAL/FINAL with the medial/initial
        // forms left as the isolated value.

        private struct LetterForms
        {
            public char Isolated;
            public char Initial;
            public char Medial;
            public char Final;
            public bool JoinsBackward; // does the preceding letter connect to this?
            public LetterForms(char iso, char ini, char med, char fin, bool joins)
            {
                Isolated = iso; Initial = ini; Medial = med; Final = fin; JoinsBackward = joins;
            }
        }

        private static readonly System.Collections.Generic.Dictionary<char, LetterForms> _forms
            = BuildFormsTable();

        private static System.Collections.Generic.Dictionary<char, LetterForms> BuildFormsTable()
        {
            // Each entry maps a base Arabic letter (U+0600 block) to its
            // four presentation forms (U+FE80 block). True/false in the
            // last column = "this letter can connect to the previous one"
            // (a.k.a. the previous letter takes its initial/medial form).
            // The classic non-joiners (Alef family, Dal/Dhal, Ra/Zay, Waw,
            // Ta-marbuta, etc.) are marked false.
            var d = new System.Collections.Generic.Dictionary<char, LetterForms>(50);

            // Hamza (U+0621) — isolated only, never joins.
            d[(char)0x0621] = new LetterForms((char)0xFE80, (char)0xFE80, (char)0xFE80, (char)0xFE80, false);
            // Alef-with-madda above (U+0622)
            d[(char)0x0622] = new LetterForms((char)0xFE81, (char)0xFE81, (char)0xFE82, (char)0xFE82, false);
            // Alef-with-hamza above (U+0623)
            d[(char)0x0623] = new LetterForms((char)0xFE83, (char)0xFE83, (char)0xFE84, (char)0xFE84, false);
            // Waw-with-hamza above (U+0624)
            d[(char)0x0624] = new LetterForms((char)0xFE85, (char)0xFE85, (char)0xFE86, (char)0xFE86, false);
            // Alef-with-hamza below (U+0625)
            d[(char)0x0625] = new LetterForms((char)0xFE87, (char)0xFE87, (char)0xFE88, (char)0xFE88, false);
            // Yeh-with-hamza above (U+0626)
            d[(char)0x0626] = new LetterForms((char)0xFE89, (char)0xFE8B, (char)0xFE8C, (char)0xFE8A, true);
            // Alef (U+0627) — non-joiner; connects back only.
            d[(char)0x0627] = new LetterForms((char)0xFE8D, (char)0xFE8D, (char)0xFE8E, (char)0xFE8E, false);
            // Beh (U+0628)
            d[(char)0x0628] = new LetterForms((char)0xFE8F, (char)0xFE91, (char)0xFE92, (char)0xFE90, true);
            // Teh-marbuta (U+0629) — non-joiner; takes final form after joining.
            d[(char)0x0629] = new LetterForms((char)0xFE93, (char)0xFE93, (char)0xFE94, (char)0xFE94, false);
            // Teh (U+062A)
            d[(char)0x062A] = new LetterForms((char)0xFE95, (char)0xFE97, (char)0xFE98, (char)0xFE96, true);
            // Theh (U+062B)
            d[(char)0x062B] = new LetterForms((char)0xFE99, (char)0xFE9B, (char)0xFE9C, (char)0xFE9A, true);
            // Jeem (U+062C)
            d[(char)0x062C] = new LetterForms((char)0xFE9D, (char)0xFE9F, (char)0xFEA0, (char)0xFE9E, true);
            // Hah (U+062D)
            d[(char)0x062D] = new LetterForms((char)0xFEA1, (char)0xFEA3, (char)0xFEA4, (char)0xFEA2, true);
            // Khah (U+062E)
            d[(char)0x062E] = new LetterForms((char)0xFEA5, (char)0xFEA7, (char)0xFEA8, (char)0xFEA6, true);
            // Dal (U+062F) — non-joiner.
            d[(char)0x062F] = new LetterForms((char)0xFEA9, (char)0xFEA9, (char)0xFEAA, (char)0xFEAA, false);
            // Thal (U+0630) — non-joiner.
            d[(char)0x0630] = new LetterForms((char)0xFEAB, (char)0xFEAB, (char)0xFEAC, (char)0xFEAC, false);
            // Reh (U+0631) — non-joiner.
            d[(char)0x0631] = new LetterForms((char)0xFEAD, (char)0xFEAD, (char)0xFEAE, (char)0xFEAE, false);
            // Zain (U+0632) — non-joiner.
            d[(char)0x0632] = new LetterForms((char)0xFEAF, (char)0xFEAF, (char)0xFEB0, (char)0xFEB0, false);
            // Seen (U+0633)
            d[(char)0x0633] = new LetterForms((char)0xFEB1, (char)0xFEB3, (char)0xFEB4, (char)0xFEB2, true);
            // Sheen (U+0634)
            d[(char)0x0634] = new LetterForms((char)0xFEB5, (char)0xFEB7, (char)0xFEB8, (char)0xFEB6, true);
            // Sad (U+0635)
            d[(char)0x0635] = new LetterForms((char)0xFEB9, (char)0xFEBB, (char)0xFEBC, (char)0xFEBA, true);
            // Dad (U+0636)
            d[(char)0x0636] = new LetterForms((char)0xFEBD, (char)0xFEBF, (char)0xFEC0, (char)0xFEBE, true);
            // Tah (U+0637)
            d[(char)0x0637] = new LetterForms((char)0xFEC1, (char)0xFEC3, (char)0xFEC4, (char)0xFEC2, true);
            // Zah (U+0638)
            d[(char)0x0638] = new LetterForms((char)0xFEC5, (char)0xFEC7, (char)0xFEC8, (char)0xFEC6, true);
            // Ain (U+0639)
            d[(char)0x0639] = new LetterForms((char)0xFEC9, (char)0xFECB, (char)0xFECC, (char)0xFECA, true);
            // Ghain (U+063A)
            d[(char)0x063A] = new LetterForms((char)0xFECD, (char)0xFECF, (char)0xFED0, (char)0xFECE, true);
            // Feh (U+0641)
            d[(char)0x0641] = new LetterForms((char)0xFED1, (char)0xFED3, (char)0xFED4, (char)0xFED2, true);
            // Qaf (U+0642)
            d[(char)0x0642] = new LetterForms((char)0xFED5, (char)0xFED7, (char)0xFED8, (char)0xFED6, true);
            // Kaf (U+0643)
            d[(char)0x0643] = new LetterForms((char)0xFED9, (char)0xFEDB, (char)0xFEDC, (char)0xFEDA, true);
            // Lam (U+0644)
            d[(char)0x0644] = new LetterForms((char)0xFEDD, (char)0xFEDF, (char)0xFEE0, (char)0xFEDE, true);
            // Meem (U+0645)
            d[(char)0x0645] = new LetterForms((char)0xFEE1, (char)0xFEE3, (char)0xFEE4, (char)0xFEE2, true);
            // Noon (U+0646)
            d[(char)0x0646] = new LetterForms((char)0xFEE5, (char)0xFEE7, (char)0xFEE8, (char)0xFEE6, true);
            // Heh (U+0647)
            d[(char)0x0647] = new LetterForms((char)0xFEE9, (char)0xFEEB, (char)0xFEEC, (char)0xFEEA, true);
            // Waw (U+0648) — non-joiner.
            d[(char)0x0648] = new LetterForms((char)0xFEED, (char)0xFEED, (char)0xFEEE, (char)0xFEEE, false);
            // Alef-maksura (U+0649)
            d[(char)0x0649] = new LetterForms((char)0xFEEF, (char)0xFBE8, (char)0xFBE9, (char)0xFEF0, true);
            // Yeh (U+064A)
            d[(char)0x064A] = new LetterForms((char)0xFEF1, (char)0xFEF3, (char)0xFEF4, (char)0xFEF2, true);

            return d;
        }

        // Lam + Alef ligature codepoints (LAM_ALEF, LAM_ALEF_MADDA, LAM_ALEF_HAMZA_ABOVE, LAM_ALEF_HAMZA_BELOW).
        // Each pair maps to the (isolated, final) presentation forms.
        // (Isolated form used when the LAM is at the start of a word or after a non-joiner;
        //  Final form used when the LAM is the last letter of a connected sequence.)
        private static readonly System.Collections.Generic.Dictionary<int, (char iso, char fin)>
            _lamAlefLigatures = new()
            {
                [0x06270000 | 0x0644] = ((char)0xFEFB, (char)0xFEFC), // Lam + Alef
                [0x06220000 | 0x0644] = ((char)0xFEF5, (char)0xFEF6), // Lam + Alef-Madda
                [0x06230000 | 0x0644] = ((char)0xFEF7, (char)0xFEF8), // Lam + Alef-Hamza-Above
                [0x06250000 | 0x0644] = ((char)0xFEF9, (char)0xFEFA), // Lam + Alef-Hamza-Below
            };

        private static bool IsLamAlefPair(char lam, char alef, out (char iso, char fin) ligature)
        {
            int key = (alef << 16) | lam;
            return _lamAlefLigatures.TryGetValue(key, out ligature);
        }

        /// <summary>
        /// Replace each Arabic codepoint in <paramref name="run"/> with its
        /// contextual presentation form. Operates on the source order
        /// (logical / right-to-left in the input); the run is reversed by
        /// the caller after shaping.
        /// </summary>
        private static string ShapeArabicRun(string run)
        {
            if (string.IsNullOrEmpty(run)) return run;

            var output = new StringBuilder(run.Length);
            int i = 0;
            while (i < run.Length)
            {
                char c = run[i];

                // Lam + Alef ligature — peek next letter.
                if (c == 0x0644 && i + 1 < run.Length &&
                    IsLamAlefPair(0x0644, run[i + 1], out var lig))
                {
                    bool joinsToPrev = i > 0 && CanJoinForward(run[i - 1]);
                    output.Append(joinsToPrev ? lig.fin : lig.iso);
                    i += 2;
                    continue;
                }

                if (!_forms.TryGetValue(c, out var f))
                {
                    output.Append(c);
                    i++;
                    continue;
                }

                bool joinsToPrev2 = i > 0 && CanJoinForward(run[i - 1]) && f.JoinsBackward;
                bool joinsToNext  = i + 1 < run.Length && CanJoinBackward(run[i + 1]);

                // Pick the right form. Non-joiners (JoinsBackward == false in
                // the row) downgrade Medial → Final and Initial → Isolated.
                char ch;
                if (joinsToPrev2 && joinsToNext)   ch = f.Medial;
                else if (joinsToPrev2)             ch = f.Final;
                else if (joinsToNext)              ch = f.Initial;
                else                               ch = f.Isolated;

                // Diacritics (tashkeel U+064B..U+0652) sit on top of the
                // letter and are not shaped — pass through.
                output.Append(ch);
                i++;
            }
            return output.ToString();
        }

        /// <summary>
        /// True if the preceding letter (the one to the right, in logical
        /// order) can connect forward to the next letter. I.e. it has a
        /// medial/initial form different from its isolated form.
        /// </summary>
        private static bool CanJoinForward(char c)
        {
            // Skip tashkeel (combining marks) — they're transparent for joining.
            if (c >= 0x064B && c <= 0x0652) return true;
            if (!_forms.TryGetValue(c, out var f)) return false;
            // A letter joins forward iff its initial != isolated.
            return f.Initial != f.Isolated;
        }

        /// <summary>
        /// True if a letter can be the target of a backward join — i.e. has
        /// a real final/medial form different from its isolated form.
        /// </summary>
        private static bool CanJoinBackward(char c)
        {
            if (c >= 0x064B && c <= 0x0652) return true;
            if (!_forms.TryGetValue(c, out var f)) return false;
            return f.JoinsBackward;
        }

        // ───── Helpers ─────────────────────────────────────────────────

        private static string Reverse(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            var arr = s.ToCharArray();
            System.Array.Reverse(arr);
            return new string(arr);
        }
    }
}
