// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / PrefaceBeatUI
//
// PHASE 50 — The Tone-Personalized Preface Beat.
//
// A 5-second narrator beat that runs ONCE per save on the first frame of
// the Lane scene (02_Mission01_Lane). It's the bridge from the Cold Open
// to gameplay — three letterbox-style overlay lines that establish:
//
//   - the season    (autumn, dusk)
//   - the player's relationship to the village   (you've never been here)
//   - the immediate goal                          (the candle in the window)
//
// The COPY of the preface is keyed off the Tone Compass (Codex 06 §
// Comfort) so each tone gets a different opening cadence:
//
//   GENTLE   "There is a candle in the window. There is no hurry."
//   STANDARD "The lane is quiet. A candle waits in the window of the Hollow."
//   DEEP     "You inherit what you do not choose. The candle is waiting."
//
// Plus a faint "fall of leaves" particle effect (procedural Image-driven
// CanvasGroup) drifts across the top of the screen.
//
// Skip: Esc / Space / click at any time.
//
// Per-save: VillageState.prefaceBeatPlayed = true, prefaceToneBucket =
// "Gentle" | "Standard" | "Deep" | "" (skipped). A re-load of the Lane
// scene on the same save will fast-path this beat — no repeat.

using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HearthboundHollow.Core;

namespace HearthboundHollow.UI
{
    public class PrefaceBeatUI : MonoBehaviour
    {
        [Header("Roots (wired by Phase 50 builder)")]
        public CanvasGroup rootGroup;
        public Image topLetterbox;
        public Image bottomLetterbox;
        public TextMeshProUGUI line1Text;
        public TextMeshProUGUI line2Text;
        public TextMeshProUGUI line3Text;
        public Image leafFallOverlay;

        [Header("Tunables")]
        [Range(0.3f, 2.0f)] public float fadeInDuration = 0.9f;
        [Range(0.3f, 2.0f)] public float fadeOutDuration = 1.4f;
        [Range(1.0f, 5.0f)] public float perLineHoldDuration = 1.8f;

        [Header("Tone copy (overridable in Inspector)")]
        public string[] gentleLines = new[]
        {
            "There is a candle in the window.",
            "There is no hurry.",
            "Walk slowly. Doris is waiting."
        };
        public string[] standardLines = new[]
        {
            "The lane is quiet.",
            "A candle waits in the window of the Hollow.",
            "Doris is on her step. Walk."
        };
        public string[] deepLines = new[]
        {
            "You inherit what you do not choose.",
            "The candle is waiting in the Hollow.",
            "She is on her step. The lane is shorter than you think."
        };

        public string narratorLineIdPrefix = "narrator_preface";

        // ───── Runtime ────────────────────────────────────────────────

        private VillageState _state;
        private Coroutine _co;
        private bool _skipRequested;
        private Action _onComplete;

        private void Awake()
        {
            // Phase 53 (D-066): start fully transparent AND non-blocking. The
            // rootGroup is a full-screen letterbox overlay; a leftover
            // blocksRaycasts=true after the beat would eat every click in the
            // Lane scene while invisible.
            SetBlocking(false);
            if (rootGroup != null) rootGroup.alpha = 0f;
            if (line1Text != null) line1Text.text = "";
            if (line2Text != null) line2Text.text = "";
            if (line3Text != null) line3Text.text = "";
            if (leafFallOverlay != null) leafFallOverlay.color = new Color(1f, 1f, 1f, 0f);
        }

        public void Play(VillageState state, Action onComplete = null)
        {
            _state = state;
            _onComplete = onComplete;
            _skipRequested = false;

            // Per-save: skip if already played.
            if (_state != null && _state.prefaceBeatPlayed)
            {
                Hh.Log(LogCategory.Boot, "Preface Beat already played; skipping.");
                onComplete?.Invoke();
                return;
            }

            string bucket = ResolveToneBucket(state);
            string[] lines = bucket switch
            {
                "Gentle"   => gentleLines,
                "Deep"     => deepLines,
                _          => standardLines,
            };

            if (_co != null) StopCoroutine(_co);
            _co = StartCoroutine(PlayCoroutine(lines, bucket));
        }

        private string ResolveToneBucket(VillageState state)
        {
            // Map gentleModeEnabled bool + the Cold Open variant into a
            // 3-bucket label. Future work: read the actual Tone Compass
            // choice from a richer Settings field.
            if (state != null && state.gentleModeEnabled) return "Gentle";
            // The "Continue" cold-open path implies the player has been
            // here before — give them the Standard tone, not Deep.
            if (state != null && state.coldOpenLastVariant == "Begin")  return "Deep";
            return "Standard";
        }

        private IEnumerator PlayCoroutine(string[] lines, string bucket)
        {
            if (rootGroup != null) rootGroup.alpha = 0f;
            SetBlocking(true);
            yield return FadeRoot(0f, 1f, fadeInDuration);

            for (int i = 0; i < 3; i++)
            {
                if (_skipRequested) break;
                var tmp = i == 0 ? line1Text : i == 1 ? line2Text : line3Text;
                if (tmp != null) tmp.text = lines[i % lines.Length];

                // Fade up the line (alpha-driven)
                yield return FadeTmp(tmp, 0f, 1f, 0.35f);

                // Soft leaf-fall hint — only on line 1 for the first ambient touch.
                if (i == 0 && leafFallOverlay != null)
                {
                    StartCoroutine(LeafFallPulse());
                }

                float hold = 0f;
                while (hold < perLineHoldDuration && !_skipRequested)
                {
                    hold += Time.unscaledDeltaTime;
                    yield return null;
                }

                yield return FadeTmp(tmp, 1f, 0f, 0.35f);
            }

            yield return FadeRoot(1f, 0f, fadeOutDuration);
            SetBlocking(false);

            if (_state != null)
            {
                _state.prefaceBeatPlayed = true;
                _state.prefaceToneBucket = _skipRequested ? "" : bucket;
            }

            _onComplete?.Invoke();
        }

        private void SetBlocking(bool on)
        {
            if (rootGroup == null) return;
            rootGroup.blocksRaycasts = on;
            rootGroup.interactable = on;
        }

        // Safety net (D-066): a fully transparent overlay must never block
        // clicks, even if the coroutine was stopped before it cleared.
        private void LateUpdate()
        {
            if (rootGroup != null && rootGroup.blocksRaycasts && rootGroup.alpha <= 0.001f)
                SetBlocking(false);
        }

        private IEnumerator FadeRoot(float from, float to, float dur)
        {
            if (rootGroup == null) yield break;
            float t = 0f;
            rootGroup.alpha = from;
            while (t < dur)
            {
                if (_skipRequested && to > 0f) { rootGroup.alpha = to; yield break; }
                t += Time.unscaledDeltaTime;
                rootGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / dur));
                yield return null;
            }
            rootGroup.alpha = to;
        }

        private IEnumerator FadeTmp(TextMeshProUGUI tmp, float from, float to, float dur)
        {
            if (tmp == null) yield break;
            float t = 0f;
            var col = tmp.color;
            col.a = from; tmp.color = col;
            while (t < dur)
            {
                if (_skipRequested && to > 0f) { col.a = to; tmp.color = col; yield break; }
                t += Time.unscaledDeltaTime;
                col.a = Mathf.Lerp(from, to, Mathf.Clamp01(t / dur));
                tmp.color = col;
                yield return null;
            }
            col.a = to; tmp.color = col;
        }

        private IEnumerator LeafFallPulse()
        {
            // Subtle 3-second alpha pulse — the player gets a hint of
            // motion without an explicit ParticleSystem.
            float t = 0f; const float DUR = 3.0f;
            while (t < DUR)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Sin((t / DUR) * Mathf.PI);
                var c = leafFallOverlay.color; c.a = k * 0.5f; leafFallOverlay.color = c;
                yield return null;
            }
            var cc = leafFallOverlay.color; cc.a = 0f; leafFallOverlay.color = cc;
        }

        private void Update()
        {
            if (_skipRequested) return;
            if (Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetKeyDown(KeyCode.Space)  ||
                Input.GetMouseButtonDown(0))
            {
                _skipRequested = true;
            }
        }
    }
}
