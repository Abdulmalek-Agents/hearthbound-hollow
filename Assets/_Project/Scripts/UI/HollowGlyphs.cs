// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / HollowGlyphs
//
// PHASE 54 (D-070) — make the onboarding / hint emoji render correctly in
// TextMesh Pro, on every machine, with NO "tofu" (▯) fallback boxes.
//
// THE PROBLEM
// ───────────
// The onboarding cards + control-hint chips decorate their copy with Unicode
// emoji (🪔 🚶 ✋ ✨ 🕯 🍂 ❓ …). TMP only renders those if the active font has
// the glyphs OR an emoji sprite/fallback is configured. Hearthbound's body font
// has none, and `TMP Settings` shipped with empty fallback lists — so every
// emoji rendered as a missing-glyph box.
//
// THE FIX
// ───────
// `Phase54_HollowGlyphsBuilder` (Editor, chained into 🚀 Build Everything) bakes
// a small on-brand GOLD glyph TMP Sprite Asset named "HollowGlyphs" and registers
// it as the TMP default sprite asset + emoji fallback. This helper rewrites the
// decorative emoji in any string into the matching TMP `<sprite name="…">` tag —
// but ONLY when that sprite asset is actually installed. When it is not (e.g. a
// fresh clone before Build Everything has run), the emoji are stripped to clean
// typography instead, so the copy is always readable and never shows a tofu box.
//
// Cozy/brand note: the baked glyphs are warm gold line-icons (not rainbow emoji),
// which sit better on the parchment UI than full-colour emoji would.

using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace HearthboundHollow.UI
{
    public static class HollowGlyphs
    {
        /// <summary>The sprite-asset name the builder bakes + registers.</summary>
        public const string SpriteAssetName = "HollowGlyphs";

        // Decorative emoji → (sprite name in HollowGlyphs asset, clean text fallback).
        // The clean fallback is what shows before the sprite asset is built — never
        // a tofu box. Empty fallback = the emoji simply vanishes (headline reads clean).
        private static readonly Dictionary<string, (string sprite, string fallback)> Map = new()
        {
            { "🪔", ("lantern",  "") },   // welcome / hollow
            { "🚶", ("walk",     "") },   // movement
            { "✋", ("hand",     "") },   // interact
            { "✨", ("sparkle",  "") },   // polish
            { "🕯", ("candle",   "") },   // comfort tools
            { "🕯️", ("candle",  "") },   // candle + VS16
            { "🍂", ("leaf",     "") },   // ready / goodbye
            { "❓", ("question", "") },   // help
            { "❔", ("question", "") },
            { "🔑", ("key",      "") },   // keys / shop
            { "🪙", ("coin",     "") },   // economy
            { "🫖", ("teapot",   "") },   // tea
            { "☕", ("teapot",   "") },
        };

        private static bool _checked;
        private static bool _spritesAvailable;

        /// <summary>
        /// True when the baked "HollowGlyphs" TMP sprite asset is registered as the
        /// TMP default sprite asset (i.e. Build Everything has run). Cached after the
        /// first query; call <see cref="Refresh"/> to re-evaluate.
        /// </summary>
        public static bool SpritesAvailable
        {
            get { if (!_checked) Refresh(); return _spritesAvailable; }
        }

        public static void Refresh()
        {
            _checked = true;
            _spritesAvailable = false;
            var def = TMP_Settings.defaultSpriteAsset;
            if (def != null && def.name == SpriteAssetName) { _spritesAvailable = true; return; }
            // Also accept it being present as an emoji fallback even if not the default.
            var settings = TMP_Settings.instance;
            if (settings != null && def != null && def.fallbackSpriteAssets != null)
            {
                foreach (var fb in def.fallbackSpriteAssets)
                    if (fb != null && fb.name == SpriteAssetName) { _spritesAvailable = true; return; }
            }
        }

        /// <summary>
        /// Rewrite decorative emoji in <paramref name="text"/> into TMP sprite tags
        /// when the glyph asset is installed, or strip them to clean text otherwise.
        /// Safe on null/empty. Never produces a tofu box.
        /// </summary>
        public static string Format(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            // Phase 58.2 — STRIP decorative emoji to clean text; do NOT emit a TMP
            // <sprite> tag. A sprite text-element mixed with the Arabic font fallback
            // in a single mesh trips Unity 6 TMP's GenerateTextMesh
            // InvalidCastException ((TMP_SpriteCharacter) cast fails at
            // TextMeshProUGUI.cs:2886) — which FROZE the game on the Help /
            // onboarding overlays in Arabic mode. Re-enable sprite glyphs only once
            // the sprite-asset / TMP version issue is resolved.
            foreach (var kv in Map)
                if (text.Contains(kv.Key)) text = text.Replace(kv.Key, kv.Value.fallback);

            // Belt-and-braces: drop any remaining surrogate-pair emoji + variation
            // selectors so no raw pictograph reaches TMP's emoji sprite fallback
            // (the same crash path).
            text = StripSurrogateEmoji(text);

            // Tidy any doubled spaces / leading spaces left by stripped emoji.
            text = text.Replace("  ", " ").TrimStart();
            return text;
        }

        // Remove surrogate-pair emoji (U+1F000+) and variation selectors so they
        // can never resolve to a TMP sprite. UI copy has no legitimate surrogate
        // text, so this is safe.
        private static string StripSurrogateEmoji(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            var sb = new System.Text.StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (char.IsHighSurrogate(c) && i + 1 < s.Length && char.IsLowSurrogate(s[i + 1])) { i++; continue; }
                if (c >= 0xFE00 && c <= 0xFE0F) continue; // variation selectors
                sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
