// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / MissionTitleCard
//
// Fades in a parchment title card at the start of a mission scene, holds for a
// few seconds, fades out. Cozy intro — no skip required, but click/key advances.
//
// Display: Mission name + tone-one-line. Pulled from MissionSO when present;
// falls back to inspector-set strings.
//
// ── Phase 60 — Arabic Localization MVP ──────────────────────────
// Day label uses hud.day_label_fmt. MissionSO authors can register
// stable localization keys (e.g. "mission.title.day1") as the
// `displayName` / `toneOneLine` fields — runtime resolves them
// transparently via LocalizationService.HasKey + Get.

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HearthboundHollow.Core;

namespace HearthboundHollow.UI
{
    public class MissionTitleCard : MonoBehaviour
    {
        [Header("Root")]
        public CanvasGroup canvasGroup;
        public GameObject root;

        [Header("Labels")]
        public TextMeshProUGUI dayLabel;
        public TextMeshProUGUI titleLabel;
        public TextMeshProUGUI toneLabel;

        [Header("Optional Mission asset (overrides inspector strings)")]
        public MissionSO mission;

        [Header("Fallback strings (used when mission == null)")]
        public string fallbackDayText = "Day 1";
        public string fallbackTitleText = "Opening the Hollow";
        public string fallbackToneText = "Warm, slightly dusty, late afternoon light.";

        [Header("Timing (seconds)")]
        public float fadeInDuration = 1.2f;
        public float holdDuration = 3.5f;
        public float fadeOutDuration = 1.5f;

        [Header("Behaviour")]
        public bool playOnStart = true;
        public bool allowSkipOnInput = true;

        private Coroutine _co;

        private void Awake()
        {
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            if (root != null && root != gameObject) root.SetActive(false);
        }

        private void Start()
        {
            if (playOnStart) Play();
        }

        public void Play()
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true);

            if (_co != null) StopCoroutine(_co);
            if (gameObject.activeInHierarchy && isActiveAndEnabled)
            {
                _co = StartCoroutine(PlayCoroutine());
            }
            else
            {
                ApplyContent();
                if (root != null) root.SetActive(true);
                if (canvasGroup != null) canvasGroup.alpha = 1f;
                Hh.Warn(LogCategory.UI,
                    "MissionTitleCard.Play called while inactive-in-hierarchy. " +
                    "Snapping to shown state without animation.");
            }
        }

        private IEnumerator PlayCoroutine()
        {
            ApplyContent();
            if (root != null) root.SetActive(true);
            if (canvasGroup == null) yield break;

            yield return FadeTo(0f, 1f, fadeInDuration);

            float t = 0f;
            while (t < holdDuration)
            {
                if (allowSkipOnInput && AnyAdvanceInput()) break;
                t += Time.unscaledDeltaTime;
                yield return null;
            }

            yield return FadeTo(1f, 0f, fadeOutDuration);
            if (root != null && root != gameObject) root.SetActive(false);
        }

        private IEnumerator FadeTo(float from, float to, float duration)
        {
            if (canvasGroup == null) yield break;
            float t = 0f;
            while (t < duration)
            {
                if (allowSkipOnInput && AnyAdvanceInput()) break;
                t += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / duration));
                yield return null;
            }
            canvasGroup.alpha = to;
        }

        private static bool AnyAdvanceInput()
        {
            return Input.GetMouseButtonDown(0)
                || Input.GetKeyDown(KeyCode.Space)
                || Input.GetKeyDown(KeyCode.Return)
                || Input.GetKeyDown(KeyCode.E)
                || Input.GetKeyDown(KeyCode.Escape);
        }

        private void ApplyContent()
        {
            var vs = ServiceLocator.Get<VillageState>();
            // Phase 60 — Localized title card. The DayLabel uses the
            // canonical "hud.day_label_fmt" key; the title falls back to
            // the inspector's `fallbackTitleText` when no localized key
            // is set on the MissionSO.
            var loc = ServiceLocator.Get<LocalizationService>();
            bool rtl = loc != null && loc.IsRightToLeft;

            if (dayLabel != null)
            {
                string s = (loc != null && vs != null)
                    ? loc.Format("hud.day_label_fmt", vs.currentDayIndex + 1)
                    : (vs != null ? $"Day {vs.currentDayIndex + 1}" : fallbackDayText);
                dayLabel.text = rtl ? ArabicTextShaper.Shape(s) : s;
                dayLabel.isRightToLeftText = rtl;
            }
            if (titleLabel != null)
            {
                string s = mission != null && !string.IsNullOrEmpty(mission.displayName)
                    ? mission.displayName : fallbackTitleText;
                // Treat the title as a localization key when it starts with
                // "mission.title." — lets MissionSO designers register
                // localizable keys without the runtime caring whether the
                // string is canon English or a key.
                if (loc != null && loc.HasKey(s)) s = loc.Get(s);
                titleLabel.text = rtl ? ArabicTextShaper.Shape(s) : s;
                titleLabel.isRightToLeftText = rtl;
            }
            if (toneLabel != null)
            {
                string s = mission != null && !string.IsNullOrEmpty(mission.toneOneLine)
                    ? mission.toneOneLine : fallbackToneText;
                if (loc != null && loc.HasKey(s)) s = loc.Get(s);
                toneLabel.text = rtl ? ArabicTextShaper.Shape(s) : s;
                toneLabel.isRightToLeftText = rtl;
            }
        }
    }
}
