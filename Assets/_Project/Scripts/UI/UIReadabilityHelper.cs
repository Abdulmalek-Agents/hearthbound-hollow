// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / UIReadabilityHelper
//
// PHASE 32.19 — Crank readability across every parchment panel.
//
// User report (screenshot of Evening Ledger): "make this text and UI very
// clear and eye attractive and appealing and fix any other UI has same
// issue". The Bamao parchment-book background is beautiful but it competes
// with text — the cream prose and the parchment paint a similar value, so
// the eye has to work to read the day summary.
//
// Fix — central readability primitives used by every cozy panel:
//
//   * `ApplyHeadline()`   — big bold ink TMP with a 1-pixel cream-paper
//                           outline + dark drop-shadow so the title pops.
//   * `ApplyBody()`       — readable prose: 24-32 pt, dark ink, outline
//                           for contrast against busy backgrounds, word-
//                           wrap enabled, autosize bounded.
//   * `ApplyMonetary()`   — gold numeric readout (coin balance, %).
//   * `ApplyButtonLabel()`— bold cream-on-dark for action buttons.
//   * `AddDarkWash()`     — drops a soft dark gradient image behind a
//                           text block so even a richly-painted parchment
//                           sprite never washes the text out. Two-stop
//                           gradient: 60 % alpha at the centre, 0 at
//                           edges — looks like the parchment is shaded
//                           rather than blocked.
//
// All helpers are idempotent and safe to re-apply. They also call into
// UIAutoFitText so the existing word-wrap / autosize discipline holds.

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HearthboundHollow.UI
{
    public static class UIReadabilityHelper
    {
        // Cozy palette — high-contrast against parchment.
        public static readonly Color InkDark    = new Color(0.13f, 0.08f, 0.04f, 1f);  // near-black brown
        public static readonly Color InkBrown   = new Color(0.32f, 0.18f, 0.07f, 1f);
        public static readonly Color CreamBright = new Color(0.98f, 0.94f, 0.82f, 1f);
        public static readonly Color CreamPaper  = new Color(0.94f, 0.86f, 0.66f, 1f);
        public static readonly Color GoldEmber   = new Color(0.92f, 0.70f, 0.34f, 1f);
        public static readonly Color GoldRich    = new Color(0.97f, 0.85f, 0.50f, 1f);
        public static readonly Color WashDark    = new Color(0.05f, 0.03f, 0.02f, 0.74f);

        /// <summary>
        /// Title-sized, bold, dark-ink with a cream outline + drop-shadow.
        /// Used for "Day 0" / "Polish the memory" / "Cleanse the orb" etc.
        /// </summary>
        public static void ApplyHeadline(TextMeshProUGUI t, int min = 48, int max = 96)
        {
            if (t == null) return;
            t.color = InkDark;
            t.fontStyle = FontStyles.Bold;
            t.alignment = TextAlignmentOptions.Center;
            t.outlineColor = CreamBright;
            t.outlineWidth = 0.22f;
            t.fontMaterial.SetColor("_UnderlayColor", new Color(0f, 0f, 0f, 0.65f));
            t.fontMaterial.SetFloat("_UnderlayOffsetX", 1.5f);
            t.fontMaterial.SetFloat("_UnderlayOffsetY", -1.5f);
            t.fontMaterial.SetFloat("_UnderlaySoftness", 0.45f);
            UIAutoFitText.ApplyToButtonLabel(t, min, max);
        }

        /// <summary>
        /// Body-sized prose — 24-36 pt. Phase 47.1: bright cream ink with a
        /// strong dark outline. Every ApplyBody call in the project is paired
        /// with AddDarkWash, and the OLD dark-ink body painted near-black text
        /// ON TOP of that dark wash → the Evening Ledger prose read as "not
        /// visible". Cream-on-dark-wash pops; the dark outline keeps it legible
        /// on the rare unwashed parchment too.
        /// </summary>
        public static void ApplyBody(TextMeshProUGUI t, int min = 22, int max = 36)
        {
            if (t == null) return;
            t.color = CreamBright;
            t.fontStyle = FontStyles.Normal;
            t.alignment = TextAlignmentOptions.TopLeft;
            t.outlineColor = InkDark;
            t.outlineWidth = 0.22f;
            t.enableWordWrapping = true;
            UIAutoFitText.ApplyToLabel(t, min, max);
        }

        /// <summary>
        /// Numeric / monetary readout — gold, bold, sized like a headline
        /// but right-aligned. Used for coin counts, % readouts.
        /// </summary>
        public static void ApplyMonetary(TextMeshProUGUI t, int min = 28, int max = 56)
        {
            if (t == null) return;
            t.color = GoldRich;
            t.fontStyle = FontStyles.Bold;
            t.alignment = TextAlignmentOptions.MidlineRight;
            t.outlineColor = InkDark;
            t.outlineWidth = 0.22f;
            UIAutoFitText.ApplyToButtonLabel(t, min, max);
        }

        /// <summary>
        /// Action-button label — bold cream-on-dark for "Save · Slot 1",
        /// "Sleep — End Day", "Skip · Auto-Complete", etc.
        /// </summary>
        public static void ApplyButtonLabel(TextMeshProUGUI t, int min = 18, int max = 32)
        {
            if (t == null) return;
            t.color = InkDark;
            t.fontStyle = FontStyles.Bold;
            t.alignment = TextAlignmentOptions.Center;
            t.outlineColor = CreamBright;
            t.outlineWidth = 0.15f;
            UIAutoFitText.ApplyToButtonLabel(t, min, max);
        }

        /// <summary>
        /// Italic stage-direction / subtitle text — slightly smaller,
        /// dim ink, italic. Used for column headers like "On the shelves:".
        /// </summary>
        public static void ApplySubtitle(TextMeshProUGUI t, int min = 20, int max = 30)
        {
            if (t == null) return;
            t.color = InkBrown;
            t.fontStyle = FontStyles.Italic | FontStyles.Bold;
            t.alignment = TextAlignmentOptions.TopLeft;
            t.outlineColor = CreamBright;
            t.outlineWidth = 0.10f;
            UIAutoFitText.ApplyToLabel(t, min, max);
        }

        /// <summary>
        /// Adds a soft dark wash Image behind a text RectTransform so the
        /// text reads clearly even against a busy parchment / book sprite.
        /// Idempotent — no-op if a wash already exists.
        /// </summary>
        public static void AddDarkWash(RectTransform target, float padding = 12f)
        {
            if (target == null) return;
            var existing = target.parent != null ? target.parent.Find($"_DarkWash_{target.name}") : null;
            if (existing != null) return;

            var washGO = new GameObject($"_DarkWash_{target.name}", typeof(RectTransform), typeof(Image));
            washGO.transform.SetParent(target.parent, false);
            washGO.transform.SetSiblingIndex(target.GetSiblingIndex()); // sits BEHIND target
            var img = washGO.GetComponent<Image>();
            img.color = WashDark;
            img.raycastTarget = false;

            var washRT = washGO.GetComponent<RectTransform>();
            washRT.anchorMin = target.anchorMin;
            washRT.anchorMax = target.anchorMax;
            washRT.pivot = target.pivot;
            washRT.anchoredPosition = target.anchoredPosition;
            washRT.sizeDelta = target.sizeDelta + new Vector2(padding * 2f, padding * 2f);
        }
    }
}
