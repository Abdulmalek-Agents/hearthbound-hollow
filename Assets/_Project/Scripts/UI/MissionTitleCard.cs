// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / MissionTitleCard
//
// Fades in a parchment title card at the start of a mission scene, holds for a
// few seconds, fades out. Cozy intro — no skip required, but click/key advances.
//
// Display: Mission name + tone-one-line. Pulled from MissionSO when present;
// falls back to inspector-set strings.

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
            if (root != null) root.SetActive(false);
        }

        private void Start()
        {
            if (playOnStart) Play();
        }

        public void Play()
        {
            if (_co != null) StopCoroutine(_co);
            _co = StartCoroutine(PlayCoroutine());
        }

        private IEnumerator PlayCoroutine()
        {
            ApplyContent();
            if (root != null) root.SetActive(true);
            if (canvasGroup == null) yield break;

            // Fade in.
            yield return FadeTo(0f, 1f, fadeInDuration);

            // Hold (with optional skip).
            float t = 0f;
            while (t < holdDuration)
            {
                if (allowSkipOnInput && AnyAdvanceInput()) break;
                t += Time.unscaledDeltaTime;
                yield return null;
            }

            // Fade out.
            yield return FadeTo(1f, 0f, fadeOutDuration);
            if (root != null) root.SetActive(false);
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
            if (dayLabel != null)
                dayLabel.text = vs != null ? $"Day {vs.currentDayIndex + 1}" : fallbackDayText;
            if (titleLabel != null)
                titleLabel.text = mission != null && !string.IsNullOrEmpty(mission.displayName) ? mission.displayName : fallbackTitleText;
            if (toneLabel != null)
                toneLabel.text = mission != null && !string.IsNullOrEmpty(mission.toneOneLine) ? mission.toneOneLine : fallbackToneText;
        }
    }
}
