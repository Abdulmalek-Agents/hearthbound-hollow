// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / MissionTitleCard
//
// Fades in a parchment title card at the start of a mission scene, holds for a
// few seconds, fades out. Cozy intro — no skip required, but click/key advances.
//
// Display: Mission name + tone-one-line. Pulled from MissionSO when present;
// falls back to inspector-set strings.
//
// ── Phase 25 hotfix ─────────────────────────────────────────────
// Play() previously started a coroutine without guarding for an inactive
// host. Mirrors the ToneCompass family of bugs — when Phase 23 deactivated
// the panel-root (which was wired as the script-host), StartCoroutine
// silently failed. Now Play() self-activates and routes through a
// hierarchy-guarded path. If the host is inactive-in-hierarchy we just
// snap to the fully-faded-in state and rely on Unity to advance on the
// next active frame (rare in practice — only happens if a parent Canvas
// is disabled, in which case we should not pretend to animate anyway).

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
            // Phase 53.1 (D-068): the CanvasGroup lives on the always-active
            // script-host, so a leftover blocksRaycasts=true would eat EVERY
            // click in the scene while the card is invisible. Start hidden AND
            // non-blocking.
            SetBlocking(false);
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            // Only hide a SEPARATE root child — never deactivate the script's
            // own GameObject, otherwise Start() won't fire and the card never
            // appears at all.
            if (root != null && root != gameObject) root.SetActive(false);
        }

        private void SetBlocking(bool on)
        {
            if (canvasGroup == null) return;
            canvasGroup.blocksRaycasts = on;
            canvasGroup.interactable = on;
        }

        // Safety net (D-068): a coroutine stopped mid-flight (host disabled,
        // Play() re-entered, scene torn down) cannot run a finally block, so it
        // could strand an alpha-0 CanvasGroup with blocksRaycasts still on —
        // an invisible, full-screen click eater. Invariant: a fully transparent
        // card must never block raycasts. Cheap, idempotent, bulletproof.
        private void LateUpdate()
        {
            if (canvasGroup != null && canvasGroup.blocksRaycasts && canvasGroup.alpha <= 0.001f)
                SetBlocking(false);
        }

        private void Start()
        {
            if (playOnStart) Play();
        }

        public void Play()
        {
            // Self-heal: ensure the host is active before requesting a coroutine.
            if (!gameObject.activeSelf) gameObject.SetActive(true);

            if (_co != null) StopCoroutine(_co);
            if (gameObject.activeInHierarchy && isActiveAndEnabled)
            {
                _co = StartCoroutine(PlayCoroutine());
            }
            else
            {
                // Defensive: snap to fully-shown state without animation.
                ApplyContent();
                if (root != null) root.SetActive(true);
                if (canvasGroup != null) canvasGroup.alpha = 1f;
                SetBlocking(true);
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
            SetBlocking(true);

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
            SetBlocking(false);
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
            if (dayLabel != null)
                dayLabel.text = LocalizationService.GetShaped("hud.day", vs != null ? vs.currentDayIndex + 1 : 1);
            if (titleLabel != null)
                titleLabel.text = mission != null && !string.IsNullOrEmpty(mission.displayName) ? mission.displayName : fallbackTitleText;
            if (toneLabel != null)
                toneLabel.text = mission != null && !string.IsNullOrEmpty(mission.toneOneLine) ? mission.toneOneLine : fallbackToneText;
        }
    }
}
