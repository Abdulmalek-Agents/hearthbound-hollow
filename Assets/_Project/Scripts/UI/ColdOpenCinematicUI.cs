// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / ColdOpenCinematicUI
//
// PHASE 48 — The Cold Open Cinematic UI.
//
// A pure-procedural cinematic that runs INSIDE THE BOOTSTRAP SCENE before
// the MainMenu loads. ~75-second mood piece that gives the player a hook
// from frame 1: a candle ignites in darkness, a letter from Marin (the
// previous Keeper) types itself onto parchment, a fast Memory Montage of
// three impressionistic phrases flashes by, Pickle's amber eyes glow open
// in the dark, the title "Hearthbound Hollow" crests, and the cozy tagline
// settles underneath. Then the player chooses BEGIN (new save) or
// CONTINUE (resume).
//
// Discipline (Codex 06 § 4 — Comfort) — fully skippable. Press Space, Esc,
// click, or any key from frame 1 to jump to the Title Card → MainMenu.
// Gentle Mode auto-skips the Montage stage entirely (the three flashed
// phrases can be too quick for some players).
//
// Production constraint (Krieg § 5) — ZERO new asset packages. Every
// visual is a Unity UI primitive (Image + TextMeshProUGUI) with procedural
// animation; the candle "flame" is a soft-radial gradient Image driven by
// PingPong noise; the parchment is a tinted rectangle with a subtle inner
// shadow. This matters because the hook ships on every platform without
// art-pipeline coupling.
//
// Save-aware (D-058 + D-055) — `seenColdOpen` lives on VillageState and
// is set to `true` the moment the player advances past the BEGIN/CONTINUE
// gate. Subsequent boots skip the cinematic and go straight to MainMenu.
// The Pause-menu's "Replay Cold Open" toggle (Phase 50) clears the flag.
//
// Public API:
//   - `Play(VillageState, Action<ColdOpenChoice> onComplete)`
//   - `SkipNow()` — invoked by the always-on input poll on any key
//
// Coupling:
//   - VillageState (Core) — `seenColdOpen`, `coldOpenLastVariant`
//   - DOES NOT depend on the Mission directors. The Bootstrap scene owns it.

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HearthboundHollow.Core;

namespace HearthboundHollow.UI
{
    public enum ColdOpenChoice { Begin, Continue }

    public class ColdOpenCinematicUI : MonoBehaviour
    {
        // ─── Inspector wiring (built procedurally by Phase 48) ─────

        [Header("Roots")]
        public CanvasGroup rootGroup;
        public CanvasGroup vignetteGroup;

        [Header("Stage layers")]
        public Image candleFlameImage;       // single soft-radial gradient
        public Image candleHaloImage;        // wider glow under the flame
        public RectTransform candleEmbersRoot;
        public Image[] embers;               // small dots that drift upward

        [Header("Parchment letter")]
        public CanvasGroup letterGroup;
        public TextMeshProUGUI letterBodyText;
        public TextMeshProUGUI letterSignature;

        [Header("Memory montage")]
        public CanvasGroup montageGroup;
        public TextMeshProUGUI montagePhraseText;

        [Header("Pickle eye-glow")]
        public CanvasGroup pickleEyesGroup;
        public Image pickleEyeLeft;
        public Image pickleEyeRight;

        [Header("Title")]
        public CanvasGroup titleGroup;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI subtitleText;

        [Header("Choice gate")]
        public CanvasGroup choiceGroup;
        public Button beginButton;
        public Button continueButton;
        public TextMeshProUGUI skipHintLabel;

        [Header("Tunables")]
        [Range(0.5f, 3.0f)] public float candleIgniteDuration = 1.8f;
        [Range(2f, 12f)]   public float letterTypeDuration = 7.0f;
        [Range(2f, 10f)]   public float montageStageDuration = 5.5f;
        [Range(2f, 8f)]    public float pickleEyesDuration = 3.2f;
        [Range(2f, 8f)]    public float titleHoldDuration = 4.0f;
        public bool autoSkipMontageInGentleMode = true;

        // ─── Marin's letter (full text — Vellis-tier voice signature) ──
        //
        // ~72 words. Reads ~36 s at the human auditory comfort rate of
        // 120 WPM, which lines up with the typewriter's targetCps=22.
        // The letter is also voiced by Narrator in Phase 53 (procedural
        // audio) when the runtime can play it; pure-text fallback is
        // shipped here so the cinematic works on every install.

        public string letterBodyDefault =
            "Whoever you are —\n" +
            "if you are reading this, the candle in the Hollow lit again.\n" +
            "I did not blow it out.\n" +
            "I went after the Forgotten Year, and I will not be back\n" +
            "before you have settled. Polish the orbs slowly. Trust\n" +
            "the bread before you trust the men. And do not — do not —\n" +
            "open the locked drawer until the cat tells you to.\n";
        public string letterSignatureDefault = "— Marin, Keeper of the Hollow before you";

        // ─── Memory montage phrases (3 quick flashes — italic, centred) ──

        public string[] montagePhrases = new[]
        {
            "a wedding ring, set down on a windowsill in the rain",
            "first bread that did not shame her, age twenty-four",
            "a Sunday kitchen, flour on the counter, sun on the floor",
        };

        // ─── Title + subtitle ──────────────────────────────────────────

        public string titleDefault = "Hearthbound Hollow";
        public string subtitleDefault = "Some memories want to be sold. Some don't.";

        // ─── Runtime ───────────────────────────────────────────────────

        private VillageState _state;
        private Action<ColdOpenChoice> _onComplete;
        private Coroutine _co;
        private bool _skipRequested;
        private bool _atChoiceGate;

        private void Awake()
        {
            // Defaults — every group starts hidden.
            if (rootGroup != null) rootGroup.alpha = 0f;
            HideAllStages();
            WireButtons();
        }

        private void WireButtons()
        {
            if (beginButton != null)
            {
                beginButton.onClick.RemoveAllListeners();
                beginButton.onClick.AddListener(() => Complete(ColdOpenChoice.Begin));
            }
            if (continueButton != null)
            {
                continueButton.onClick.RemoveAllListeners();
                continueButton.onClick.AddListener(() => Complete(ColdOpenChoice.Continue));
            }
        }

        private void HideAllStages()
        {
            SetAlpha(vignetteGroup, 1f);   // black always-on under everything
            SetAlpha(letterGroup, 0f);
            SetAlpha(montageGroup, 0f);
            SetAlpha(pickleEyesGroup, 0f);
            SetAlpha(titleGroup, 0f);
            SetAlpha(choiceGroup, 0f);
            if (candleFlameImage != null) { var c = candleFlameImage.color; c.a = 0f; candleFlameImage.color = c; }
            if (candleHaloImage  != null) { var c = candleHaloImage.color;  c.a = 0f; candleHaloImage.color  = c; }
            if (embers != null)
                foreach (var e in embers) if (e != null) { var c = e.color; c.a = 0f; e.color = c; }
        }

        // ─── Public API ─────────────────────────────────────────────────

        public void Play(VillageState state, Action<ColdOpenChoice> onComplete)
        {
            _state = state;
            _onComplete = onComplete;
            _skipRequested = false;
            _atChoiceGate = false;

            // Save-aware: skip entirely if seen already and the bootstrap
            // director didn't ask us to replay. The caller has the choice.
            if (_state != null && _state.seenColdOpen && !forceReplay)
            {
                Hh.Log(LogCategory.Boot, "Cold Open already seen; jumping to choice gate.");
                if (_co != null) StopCoroutine(_co);
                _co = StartCoroutine(JumpToChoiceGate());
                return;
            }

            HideAllStages();
            if (rootGroup != null) rootGroup.alpha = 1f;
            if (_co != null) StopCoroutine(_co);
            _co = StartCoroutine(PlayCoroutine());
        }

        [Header("Replay")]
        public bool forceReplay;

        public void SkipNow()
        {
            if (_atChoiceGate) return; // skip is a no-op at the gate
            _skipRequested = true;
        }

        // ─── Coroutines ─────────────────────────────────────────────────

        private IEnumerator PlayCoroutine()
        {
            // C# iterator-method state machines don't play well with goto
            // across yield boundaries, so we use a chain of if-guards. The
            // _skipRequested flag short-circuits every remaining stage as
            // soon as it's set, dropping us into the choice gate cleanly.

            // Stage 1 — Candle ignite.
            if (!_skipRequested) yield return StartCoroutine(StageCandleIgnite());

            // Stage 2 — Letter typewriter.
            if (!_skipRequested) yield return StartCoroutine(StageLetter());

            // Stage 3 — Memory montage (skipped entirely in Gentle Mode if opted).
            bool gentle = _state != null && _state.gentleModeEnabled;
            if (!_skipRequested && !(gentle && autoSkipMontageInGentleMode))
                yield return StartCoroutine(StageMontage());

            // Stage 4 — Pickle eye-glow.
            if (!_skipRequested) yield return StartCoroutine(StagePickleEyes());

            // Stage 5 — Title + subtitle.
            if (!_skipRequested) yield return StartCoroutine(StageTitle());
            else                 yield return StartCoroutine(StageTitle(0.4f));

            _atChoiceGate = true;
            yield return StartCoroutine(StageChoiceGate());
        }

        private IEnumerator JumpToChoiceGate()
        {
            HideAllStages();
            if (rootGroup != null) rootGroup.alpha = 1f;
            yield return StartCoroutine(StageTitle(0.6f));
            _atChoiceGate = true;
            yield return StartCoroutine(StageChoiceGate());
        }

        private IEnumerator StageCandleIgnite()
        {
            float t = 0f;
            // Cross-fade candle in from 0 → 1. Halo lags by 0.3 s for depth.
            while (t < candleIgniteDuration)
            {
                if (_skipRequested) break;
                t += Time.unscaledDeltaTime;
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / candleIgniteDuration));
                if (candleFlameImage != null) { var c = candleFlameImage.color; c.a = k; candleFlameImage.color = c; }
                float kh = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - 0.3f) / (candleIgniteDuration - 0.3f)));
                if (candleHaloImage  != null) { var c = candleHaloImage.color;  c.a = kh * 0.55f; candleHaloImage.color  = c; }
                // ember pulse — tiny PingPong shimmer
                if (embers != null)
                {
                    for (int i = 0; i < embers.Length; i++)
                    {
                        if (embers[i] == null) continue;
                        float pulse = 0.35f + 0.65f * Mathf.PingPong(Time.unscaledTime * (1.1f + 0.13f * i), 1f);
                        var c = embers[i].color; c.a = k * pulse * 0.7f; embers[i].color = c;
                    }
                }
                yield return null;
            }
            // hold for a half-beat
            float hold = 0f;
            while (hold < 0.6f && !_skipRequested) { hold += Time.unscaledDeltaTime; yield return null; }
        }

        private IEnumerator StageLetter()
        {
            // Fade the parchment in over 0.5 s, type the body, fade out.
            string body = string.IsNullOrEmpty(letterBodyText.text) ? letterBodyDefault : letterBodyText.text;
            if (!string.IsNullOrEmpty(letterBodyDefault)) body = letterBodyDefault;
            string sig = !string.IsNullOrEmpty(letterSignatureDefault) ? letterSignatureDefault : "— Marin";

            if (letterBodyText != null) letterBodyText.text = "";
            if (letterSignature != null) letterSignature.text = "";
            yield return FadeGroup(letterGroup, 0f, 1f, 0.6f);

            float typeRate = body.Length / Mathf.Max(0.5f, letterTypeDuration);
            float t = 0f; int idx = 0;
            while (idx < body.Length)
            {
                if (_skipRequested) { idx = body.Length; break; }
                t += Time.unscaledDeltaTime;
                int targetIdx = Mathf.Clamp(Mathf.FloorToInt(t * typeRate), 0, body.Length);
                if (targetIdx != idx)
                {
                    idx = targetIdx;
                    if (letterBodyText != null) letterBodyText.text = body.Substring(0, idx);
                }
                yield return null;
            }
            if (letterBodyText != null) letterBodyText.text = body;

            // Signature appears with a 0.4 s flourish.
            float sigT = 0f;
            while (sigT < 0.4f && !_skipRequested)
            {
                sigT += Time.unscaledDeltaTime;
                if (letterSignature != null) letterSignature.text = sig.Substring(0, Mathf.Clamp(Mathf.FloorToInt(sig.Length * sigT / 0.4f), 0, sig.Length));
                yield return null;
            }
            if (letterSignature != null) letterSignature.text = sig;

            // Hold then fade.
            float hold = 0f;
            while (hold < 1.6f && !_skipRequested) { hold += Time.unscaledDeltaTime; yield return null; }
            yield return FadeGroup(letterGroup, 1f, 0f, 0.8f);
        }

        private IEnumerator StageMontage()
        {
            yield return FadeGroup(montageGroup, 0f, 1f, 0.4f);
            int n = montagePhrases != null ? montagePhrases.Length : 0;
            float perPhrase = Mathf.Max(0.6f, (montageStageDuration - 0.8f) / Mathf.Max(1, n));
            for (int i = 0; i < n; i++)
            {
                if (_skipRequested) break;
                if (montagePhraseText != null) montagePhraseText.text = "— " + montagePhrases[i];
                // tiny fade-pulse so each phrase feels distinct
                yield return FadeGroup(montageGroup, 0.0f, 1f, 0.25f);
                float hold = 0f;
                while (hold < perPhrase - 0.5f && !_skipRequested) { hold += Time.unscaledDeltaTime; yield return null; }
                yield return FadeGroup(montageGroup, 1f, 0.0f, 0.25f);
            }
        }

        private IEnumerator StagePickleEyes()
        {
            if (pickleEyesGroup == null) yield break;
            yield return FadeGroup(pickleEyesGroup, 0f, 1f, 0.8f);
            // gentle blink mid-stage
            float t = 0f;
            while (t < pickleEyesDuration - 1.6f)
            {
                if (_skipRequested) break;
                t += Time.unscaledDeltaTime;
                // every ~2.4 s, blink (both eyes alpha → 0 → 1 over 0.18 s)
                if (Mathf.FloorToInt(t / 2.4f) != Mathf.FloorToInt((t - Time.unscaledDeltaTime) / 2.4f))
                {
                    yield return StartCoroutine(BlinkPickle());
                }
                yield return null;
            }
            yield return FadeGroup(pickleEyesGroup, 1f, 0f, 0.8f);
        }

        private IEnumerator BlinkPickle()
        {
            float t = 0f; const float dur = 0.18f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float k = 1f - Mathf.PingPong(t / dur, 1f);
                if (pickleEyeLeft  != null) { var c = pickleEyeLeft.color;  c.a = k; pickleEyeLeft.color  = c; }
                if (pickleEyeRight != null) { var c = pickleEyeRight.color; c.a = k; pickleEyeRight.color = c; }
                yield return null;
            }
            if (pickleEyeLeft  != null) { var c = pickleEyeLeft.color;  c.a = 1f; pickleEyeLeft.color  = c; }
            if (pickleEyeRight != null) { var c = pickleEyeRight.color; c.a = 1f; pickleEyeRight.color = c; }
        }

        private IEnumerator StageTitle(float fadeIn = 1.2f)
        {
            if (titleText != null) titleText.text = string.IsNullOrEmpty(titleText.text) ? titleDefault : titleDefault;
            if (subtitleText != null) subtitleText.text = string.IsNullOrEmpty(subtitleText.text) ? subtitleDefault : subtitleDefault;
            yield return FadeGroup(titleGroup, 0f, 1f, fadeIn);
            float hold = 0f;
            while (hold < titleHoldDuration && !_skipRequested) { hold += Time.unscaledDeltaTime; yield return null; }
        }

        private IEnumerator StageChoiceGate()
        {
            // Pre-fill: if a save exists, Continue is enabled and gets focus.
            bool saveExists = HasSaveOnDisk();
            if (continueButton != null) continueButton.interactable = saveExists;
            if (continueButton != null) continueButton.gameObject.SetActive(saveExists);
            if (beginButton != null) beginButton.gameObject.SetActive(true);
            if (skipHintLabel != null) skipHintLabel.text = saveExists
                ? "Press Enter to Continue · Esc to Begin Anew"
                : "Press Enter / Space to Begin";

            yield return FadeGroup(choiceGroup, 0f, 1f, 0.6f);

            // Wait for input.
            while (true)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    Complete(saveExists ? ColdOpenChoice.Continue : ColdOpenChoice.Begin);
                    yield break;
                }
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Complete(ColdOpenChoice.Begin);
                    yield break;
                }
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Complete(ColdOpenChoice.Begin);
                    yield break;
                }
                yield return null;
            }
        }

        private void Update()
        {
            // Whole-cinematic skip listener (input-system-agnostic).
            if (_atChoiceGate) return;
            if (Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetKeyDown(KeyCode.Space)  ||
                Input.GetMouseButtonDown(0))
            {
                _skipRequested = true;
            }
        }

        // ─── Helpers ────────────────────────────────────────────────────

        private IEnumerator FadeGroup(CanvasGroup g, float from, float to, float dur)
        {
            if (g == null) yield break;
            float t = 0f;
            g.alpha = from;
            while (t < dur)
            {
                if (_skipRequested && to > 0f) { g.alpha = to; yield break; }
                t += Time.unscaledDeltaTime;
                g.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / dur));
                yield return null;
            }
            g.alpha = to;
        }

        private static void SetAlpha(CanvasGroup g, float a) { if (g != null) g.alpha = a; }

        private void Complete(ColdOpenChoice choice)
        {
            if (_state != null)
            {
                _state.seenColdOpen = true;
                _state.coldOpenLastVariant = (choice == ColdOpenChoice.Continue) ? "Continue" : "Begin";
            }
            var cb = _onComplete;
            _onComplete = null;
            if (_co != null) StopCoroutine(_co);
            _co = null;
            // Fade root out first so the MainMenu doesn't slam in.
            StartCoroutine(FadeOutThen(() => cb?.Invoke(choice)));
        }

        private IEnumerator FadeOutThen(Action onDone)
        {
            yield return FadeGroup(rootGroup, rootGroup != null ? rootGroup.alpha : 1f, 0f, 0.6f);
            if (rootGroup != null) rootGroup.gameObject.SetActive(false);
            onDone?.Invoke();
        }

        private bool HasSaveOnDisk()
        {
            // Save asmdef has the canonical answer, but UI must not reference Save.
            // PlayerPrefs is the simplest signal that survives across runs.
            return PlayerPrefs.HasKey("Hh_HasSave") && PlayerPrefs.GetInt("Hh_HasSave", 0) == 1;
        }
    }
}
