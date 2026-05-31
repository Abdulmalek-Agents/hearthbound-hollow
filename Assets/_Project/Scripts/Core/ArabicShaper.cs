// SPDX-License-Identifier: MIT
// Hearthbound Hollow - Core / ArabicShaper
//
// PHASE 56 (D-073) - Arabic display shaping for TextMesh Pro.
//
// TMP's classic component renders Unicode code points "as authored": it does
// NOT apply Arabic contextual joining (initial/medial/final/isolated forms) or
// right-to-left ordering. Feeding raw logical-order Arabic to a TMP label -
// even once an Arabic font is present - yields DISCONNECTED, left-to-right
// letters ("not real Arabic words", per the QA video).
//
// This pure-C# helper converts a logical-order Arabic string into a
// display-ready string:
//   1. picks each letter's contextual presentation form (Arabic Presentation
//      Forms-B, U+FE70..U+FEFF) from its neighbours' joining behaviour,
//   2. merges Lam+Alef into their ligatures,
//   3. reverses to visual order for a normal (LTR) TMP field, keeping embedded
//      Latin/number runs readable.
//
// The Presentation Forms-B glyphs for each letter are CONTIGUOUS from its
// isolated codepoint (iso, final[, initial, medial]) so the table stores only
// the base char, the isolated form codepoint, and whether it is dual-joining.
//
// Strings with no Arabic are returned untouched, so English/mixed UI is safe.
// Pair with Phase56_ArabicFontInstaller (the font fixes the "tofu boxes"; this
// fixes the joining + direction).

using System.Collections.Generic;
using System.Text;

namespace HearthboundHollow.Core
{
    public static class ArabicShaper
    {
        private struct Form { public int isoForm; public bool dual; }

        private static readonly Dictionary<int, Form> Map = Build();
        private const int Lam = 0x0644;

        // Lam + Alef variant -> (isolated ligature, final ligature) codepoints.
        private static readonly Dictionary<int, (int iso, int fin)> LamAlef = new()
        {
            { 0x0622, (0xFEF5, 0xFEF6) }, // Alef Madda
            { 0x0623, (0xFEF7, 0xFEF8) }, // Alef Hamza Above
            { 0x0625, (0xFEF9, 0xFEFA) }, // Alef Hamza Below
            { 0x0627, (0xFEFB, 0xFEFC) }, // Alef
        };

        /// <summary>
        /// Convert a logical-order Arabic string to a TMP display string
        /// (presentation forms + visual RTL order). Non-Arabic input is returned
        /// unchanged. Safe on null/empty.
        /// </summary>
        public static string Shape(string s)
        {
            if (string.IsNullOrEmpty(s) || !ContainsArabic(s)) return s;

            char[] a = s.ToCharArray();
            var logical = new List<char>(a.Length);

            for (int i = 0; i < a.Length; i++)
            {
                int c = a[i];

                // Lam + Alef ligature.
                if (c == Lam && i + 1 < a.Length && LamAlef.TryGetValue(a[i + 1], out var lig))
                {
                    int prev = PrevLetter(a, i);
                    bool before = prev != 0 && JoinsLeft(prev);
                    logical.Add((char)(before ? lig.fin : lig.iso));
                    i++; // consume the alef
                    continue;
                }

                if (Map.TryGetValue(c, out var f))
                {
                    int prev = PrevLetter(a, i);
                    int next = NextLetter(a, i);
                    bool before = prev != 0 && JoinsLeft(prev);            // current connects to previous
                    bool after  = f.dual && next != 0 && JoinsRight(next); // current connects to next
                    // Contiguous forms: iso, final, initial, medial.
                    int form = (before && after) ? f.isoForm + 3
                             : before            ? f.isoForm + 1
                             : after             ? f.isoForm + 2
                             :                     f.isoForm;
                    logical.Add((char)form);
                }
                else
                {
                    logical.Add((char)c); // spaces, digits, Latin, Arabic punctuation, marks
                }
            }

            return ToVisualOrder(logical);
        }

        // ----- neighbour helpers -----

        // Adjacent Arabic letter only (a space / non-letter breaks joining).
        private static int PrevLetter(char[] a, int i) =>
            (i > 0 && Map.ContainsKey(a[i - 1])) ? a[i - 1] : 0;

        private static int NextLetter(char[] a, int i) =>
            (i + 1 < a.Length && Map.ContainsKey(a[i + 1])) ? a[i + 1] : 0;

        private static bool JoinsRight(int c) => Map.ContainsKey(c);                       // any Arabic letter links to a preceding letter
        private static bool JoinsLeft(int c)  => Map.TryGetValue(c, out var f) && f.dual;  // only dual-joining links to a following letter

        private static bool ContainsArabic(string s)
        {
            foreach (char c in s)
                if (c >= 0x0600 && c <= 0x06FF) return true;
            return false;
        }

        // Reverse to visual order, but keep contiguous Latin/number runs readable.
        private static string ToVisualOrder(List<char> logical)
        {
            var sb = new StringBuilder(logical.Count);
            int i = logical.Count - 1;
            while (i >= 0)
            {
                if (IsNeutralLtr(logical[i]))
                {
                    int j = i;
                    while (j >= 0 && IsNeutralLtr(logical[j])) j--;
                    for (int k = j + 1; k <= i; k++) sb.Append(logical[k]); // forward order
                    i = j;
                }
                else
                {
                    sb.Append(logical[i]);
                    i--;
                }
            }
            return sb.ToString();
        }

        private static bool IsNeutralLtr(char c) =>
            (c >= '0' && c <= '9') || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');

        // ----- table -----

        private static Dictionary<int, Form> Build()
        {
            var m = new Dictionary<int, Form>();
            void R(int b, int iso) => m[b] = new Form { isoForm = iso, dual = false }; // right-joining (iso, final)
            void D(int b, int iso) => m[b] = new Form { isoForm = iso, dual = true };  // dual-joining (iso, final, initial, medial)

            R(0x0621, 0xFE80); // Hamza (non-joining; only isolated)
            R(0x0622, 0xFE81); // Alef Madda
            R(0x0623, 0xFE83); // Alef Hamza Above
            R(0x0624, 0xFE85); // Waw Hamza
            R(0x0625, 0xFE87); // Alef Hamza Below
            D(0x0626, 0xFE89); // Yeh Hamza
            R(0x0627, 0xFE8D); // Alef
            D(0x0628, 0xFE8F); // Beh
            R(0x0629, 0xFE93); // Teh Marbuta
            D(0x062A, 0xFE95); // Teh
            D(0x062B, 0xFE99); // Theh
            D(0x062C, 0xFE9D); // Jeem
            D(0x062D, 0xFEA1); // Hah
            D(0x062E, 0xFEA5); // Khah
            R(0x062F, 0xFEA9); // Dal
            R(0x0630, 0xFEAB); // Thal
            R(0x0631, 0xFEAD); // Reh
            R(0x0632, 0xFEAF); // Zain
            D(0x0633, 0xFEB1); // Seen
            D(0x0634, 0xFEB5); // Sheen
            D(0x0635, 0xFEB9); // Sad
            D(0x0636, 0xFEBD); // Dad
            D(0x0637, 0xFEC1); // Tah
            D(0x0638, 0xFEC5); // Zah
            D(0x0639, 0xFEC9); // Ain
            D(0x063A, 0xFECD); // Ghain
            D(0x0641, 0xFED1); // Feh
            D(0x0642, 0xFED5); // Qaf
            D(0x0643, 0xFED9); // Kaf
            D(0x0644, 0xFEDD); // Lam
            D(0x0645, 0xFEE1); // Meem
            D(0x0646, 0xFEE5); // Noon
            D(0x0647, 0xFEE9); // Heh
            R(0x0648, 0xFEED); // Waw
            R(0x0649, 0xFEEF); // Alef Maksura
            D(0x064A, 0xFEF1); // Yeh
            return m;
        }
    }
}
