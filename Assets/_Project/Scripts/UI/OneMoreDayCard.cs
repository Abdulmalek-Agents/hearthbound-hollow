// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / OneMoreDayCard
//
// Phase 47 — the "One More Day" goodnight beat. Shown after the Evening
// Ledger (and after the night's Dream, if any) and before the next scene
// loads. A warm forward-look line + an optional Pickle sign-off + a single
// "Goodnight" button. No numbers, no fail state, fully skippable.
//
// Presentational only: the EndOfDaySequencer resolves the prose strings and
// passes them in, so this script takes no dependency on the Mission asmdef
// (UI references only Core/Memory/Audio — see ARCHITECTURE §3, D-035).
//
// ── Studio additions over the Phase 47 implementation guide ──────────────
//   • Gentle-Mode safe: Show(..., instant: true) skips the fade entirely so
//     the beat is identical-content but zero-stress (Cozy Contract / D-062).
//     The *sequencer* reads VillageState.gentleModeEnabled and passes it in,
//     keeping this script free of any game-state dependency.
//   • Accessibility: while the card is visible, Space / Return / E / Esc /
//     left-click also advance it (matches the directors' advance affordance,
//     D-049). The Goodnight button remains the primary, visible affordance.
//   • Self-heals in Show() like every other cozy overlay (D-034).

using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HearthboundHollow.UI
{
    public class OneMoreDayCard : MonoBehaviour
    {
        [Header("Root")]
        public GameObject root;
        public CanvasGroup canvasGroup;     // for the soft fade-in

        [Header("Labels")]
        public TextMeshProUGUI headlineLabel;
        public TextMeshProUGUI forwardLookLabel;
        public TextMeshProUGUI pickleLabel;

        [Header("Continue")]
        public Button goodnightButton;

        [Header("Feel")]
        [Range(0.1f, 1.5f)] public float fadeInSeconds = 0.6f;
        public string headlineText = "Tomorrow";

        [Tooltip("While the card is visible, Space / Return / E / Esc / click " +
                 "also advance it. The Goodnight button stays the primary " +
                 "affordance; this is a keyboard/gamepad convenience (D-049).")]
        public bool allowKeyboardAdvance = true;

        /// <summary>Raised when the player presses Goodnight (or any advance
        /// key). The sequencer awaits this before running the transition.</summary>
        public event Action OnContinue;

        private bool _visible;
        private bool _advanced;

        private void Awake()
        {
            if (root != null && root != gameObject) root.SetActive(false);
            // Phase 53.1 (D-068): the CanvasGroup sits on the always-active host;
            // a stranded blocksRaycasts=true would silently eat every click.
            SetBlocking(false);

            if (goodnightButton != null)
                goodnightButton.onClick.AddListener(Advance);

            // High-contrast typography to match the Evening Ledger parchment.
            UIReadabilityHelper.ApplyHeadline(headlineLabel, min: 48, max: 96);
            UIReadabilityHelper.ApplyBody(forwardLookLabel, min: 26, max: 38);
            UIReadabilityHelper.ApplyBody(pickleLabel, min: 22, max: 32);

            if (forwardLookLabel != null)
                UIReadabilityHelper.AddDarkWash(forwardLookLabel.rectTransform, padding: 18f);
            if (pickleLabel != null)
                UIReadabilityHelper.AddDarkWash(pickleLabel.rectTransform, padding: 14f);
        }

        /// <param name="forwardLook">Resolved forward-look prose (Cordray).</param>
        /// <param name="pickleSignOff">Optional Pickle goodnight; empty hides the line.</param>
        /// <param name="instant">Gentle-Mode: skip the fade and show at full
        /// alpha immediately (identical content, zero stress).</param>
        public void Show(string forwardLook, string pickleSignOff, bool instant = false)
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true);   // self-heal
            if (root != null) root.SetActive(true);
            SetBlocking(true);

            _advanced = false;

            if (headlineLabel != null) headlineLabel.text = headlineText;
            if (forwardLookLabel != null) forwardLookLabel.text = forwardLook;

            if (pickleLabel != null)
            {
                bool has = !string.IsNullOrWhiteSpace(pickleSignOff);
                pickleLabel.gameObject.SetActive(has);
                if (has) pickleLabel.text = $"<i>{pickleSignOff}</i>";
            }

            if (canvasGroup != null)
            {
                StopAllCoroutines();
                if (instant) canvasGroup.alpha = 1f;
                else StartCoroutine(FadeIn());
            }

            _visible = true;
        }

        private void Update()
        {
            if (!_visible || !allowKeyboardAdvance) return;

            // Mirror the directors' advance affordance so keyboard / gamepad
            // players never get stranded on the card (Cozy Contract: skippable).
            if (Input.GetMouseButtonDown(0) ||
                Input.GetKeyDown(KeyCode.Space) ||
                Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.KeypadEnter) ||
                Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetKeyDown(KeyCode.E))
            {
                Advance();
            }
        }

        private void Advance()
        {
            if (_advanced) return;          // single-fire guard
            _advanced = true;
            _visible = false;
            Hide();
            OnContinue?.Invoke();
        }

        private IEnumerator FadeIn()
        {
            float t = 0f;
            canvasGroup.alpha = 0f;
            while (t < fadeInSeconds)
            {
                t += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(t / fadeInSeconds);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        public void Hide()
        {
            _visible = false;
            SetBlocking(false);
            if (root != null && root != gameObject) root.SetActive(false);
        }

        private void SetBlocking(bool on)
        {
            if (canvasGroup == null) return;
            canvasGroup.blocksRaycasts = on;
            canvasGroup.interactable = on;
        }

        // Safety net (D-068): a fully transparent card must never block clicks,
        // even if a coroutine was stopped mid-flight before Hide() ran.
        private void LateUpdate()
        {
            if (canvasGroup != null && canvasGroup.blocksRaycasts && canvasGroup.alpha <= 0.001f)
                SetBlocking(false);
        }
    }
}
