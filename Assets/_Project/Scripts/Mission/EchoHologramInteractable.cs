// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / EchoHologramInteractable
//
// PHASE 49 — The first Echo Hologram of the predecessor (Marin).
//
// Codex 03 § Predecessor + Codex 12 § Echo Hologram describe the Echo
// system as a sequence of "automated proto-AI memory recordings" Marin
// left behind. They speak directly to the player, decay over multiple
// visits, and slowly accumulate (post-M2) into the Memory Web.
//
// M1-2 ships exactly ONE Echo Hologram: "Welcome, Whoever You Are."
// Placed on the Hollow workbench next to Marin's parchment Note (which
// Phase 26 wired). On first interaction:
//
//   1. The orb on the workbench softly glows + emits a single chord.
//   2. The hologram fades in (translucent grey-blue silhouette UI image
//      next to the orb). VoicePlayer plays `echo_marin_welcome_01`.
//   3. A 17-second monologue plays. Three lines:
//        Marin: "If the Hollow lit again, I am very sorry. You inherit
//        it the way I did — without ceremony, without a manual."
//        Marin: "There is a candle on the workbench. There is a cat at
//        your knee. There is a baker next door who has been waiting."
//        Marin: "Trust the bread. Polish slowly. And if Pickle sniffs
//        a memory and walks away — you should walk away too."
//   4. The hologram dissolves. predecessorTrailWarmth += 12.
//      echoHologramHeard = true; echoHologramsFound += 1.
//
// On subsequent visits the hologram has a faint hum-only state (Pickle:
// "Marin's voice doesn't return easily. You heard her once. Be patient.")
// to confirm the player remembers what they heard.
//
// Inputs:
//   - `hologramVisual` is the UI Image (slot wired by Phase 49 builder)
//     that fades in for the recording.
//   - `chordSfxId` / `voiceLineId` (both resolved against existing
//     libraries; missing assets play silent).
//
// Output:
//   - Publishes `EchoHologramHeardEvent` for the audio + Pickle systems
//     to react.
//   - Sets the persistent VillageState flags.
//
// Idempotent — `echoHologramHeard == true` short-circuits the monologue
// path so re-interacting just plays the faint hum-state.
//
// ── Hotfix 2026-05-28 ──────────────────────────────────────────────
// Replaced `ServiceLocator.Resolve<T>()` with `ServiceLocator.Get<T>()`
// (the canonical Core API). 2 call sites corrected in this file. See
// the companion fix on MemoryWebOverlay.cs for the full audit summary.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using HearthboundHollow.Core;
using HearthboundHollow.Player;
using HearthboundHollow.UI;

namespace HearthboundHollow.Mission
{
    /// <summary>
    /// Published on the EventBus when the player hears (or re-hears) an
    /// Echo Hologram recording. Consumed by MissionAudioHooks (chord SFX +
    /// music duck), Pickle (reaction line eligibility), and the Codex
    /// (entry unlock).
    /// </summary>
    public readonly struct EchoHologramHeardEvent
    {
        public readonly string HologramId;
        public readonly bool IsFirstHearing;
        public EchoHologramHeardEvent(string id, bool first) { HologramId = id; IsFirstHearing = first; }
    }

    public class EchoHologramInteractable : Interactable
    {
        [Header("Identity")]
        [Tooltip("Stable id of this hologram for save-state + EventBus payloads.")]
        public string hologramId = "marin_welcome_01";
        [Tooltip("On-screen E-prompt label shown by ControlHintsHUD.")]
        public string promptFirst = "Listen to the Echo";
        public string promptRepeat = "Listen again";

        [Header("Spoken monologue (3 lines — Vellis-tier voice signature)")]
        [TextArea(2, 5)] public string line01 =
            "If the Hollow lit again, I am very sorry. You inherit it the way I did — without ceremony, without a manual.";
        [TextArea(2, 5)] public string line02 =
            "There is a candle on the workbench. There is a cat at your knee. There is a baker next door who has been waiting.";
        [TextArea(2, 5)] public string line03 =
            "Trust the bread. Polish slowly. And if Pickle sniffs a memory and walks away — you should walk away too.";

        [Header("Audio lineIds (D-058)")]
        public string voiceLineIdA = "echo_marin_welcome_01";
        public string voiceLineIdB = "echo_marin_welcome_02";
        public string voiceLineIdC = "echo_marin_welcome_03";
        [Tooltip("SfxLibrarySO id of the soft chord that plays as the hologram fades in.")]
        public string chordSfxId = "echo_marin_chord";

        [Header("Visual (wired by Phase 49 builder)")]
        public Image hologramVisual;
        public Light hologramLight;
        public DialogueUI dialogueUI;
        public CanvasGroup hologramGroup;

        [Header("Timing")]
        [Range(0.2f, 4f)] public float fadeInDuration = 1.5f;
        [Range(0.2f, 4f)] public float fadeOutDuration = 1.5f;
        [Range(0.5f, 3f)] public float lineGap = 0.6f;

        [Header("VillageState bump (first hearing)")]
        [Range(0, 50)] public int predecessorTrailWarmthBump = 12;

        // ───── Lifecycle ─────────────────────────────────────────────

        private VillageState _state;
        private bool _busy;

        protected void OnEnable()
        {
            _state = ServiceLocator.Get<VillageState>();
            ApplyHologramState(false);
        }

        public override string GetDynamicPromptText()
        {
            if (_state != null && _state.echoHologramHeard) return promptRepeat;
            return promptFirst;
        }

        public override void Activate(GameObject player)
        {
            if (_busy) return;
            if (_state == null) _state = ServiceLocator.Get<VillageState>();
            bool first = _state == null || !_state.echoHologramHeard;
            StartCoroutine(PlayHologram(first));
        }

        // ───── Coroutines ────────────────────────────────────────────

        private IEnumerator PlayHologram(bool firstHearing)
        {
            _busy = true;

            // Publish the event so the audio/Pickle layers can prepare.
            EventBus.Publish(new EchoHologramHeardEvent(hologramId, firstHearing));

            // 1) Fade visuals in.
            yield return StartCoroutine(FadeHologram(0f, 1f, fadeInDuration));
            ApplyHologramState(true);

            // 2) Play monologue lines. If first time → full 3-line script;
            //    if repeat → 1 quiet line that confirms the state.
            if (firstHearing)
            {
                yield return PlayLine(line01, voiceLineIdA);
                yield return new WaitForSecondsRealtime(lineGap);
                yield return PlayLine(line02, voiceLineIdB);
                yield return new WaitForSecondsRealtime(lineGap);
                yield return PlayLine(line03, voiceLineIdC);
            }
            else
            {
                yield return PlayLine(
                    "(The hologram hums but does not speak. Marin's voice doesn't return easily.)",
                    null);
            }

            // 3) Fade visuals out.
            yield return StartCoroutine(FadeHologram(1f, 0f, fadeOutDuration));
            ApplyHologramState(false);

            // 4) Persist state on first hearing.
            if (firstHearing && _state != null)
            {
                _state.echoHologramHeard = true;
                _state.echoHologramsFound = Mathf.Max(1, _state.echoHologramsFound + 1);
                _state.predecessorTrailWarmth = VillageState.Adjust(
                    _state.predecessorTrailWarmth, predecessorTrailWarmthBump);
                Hh.Log(LogCategory.Mission,
                    $"Echo Hologram '{hologramId}' heard for the first time. " +
                    $"Trail warmth → {_state.predecessorTrailWarmth}.");
            }

            _busy = false;
        }

        private IEnumerator PlayLine(string body, string lineId)
        {
            if (dialogueUI == null)
            {
                // Fall back to silent log — the player still gets the
                // fade visuals.
                Debug.Log("[Echo] " + body);
                yield return new WaitForSecondsRealtime(3.5f);
                yield break;
            }
            dialogueUI.PresentLine("Marin", body, null, lineId);

            // Wait for the dialogue to advance (typewriter ends → player click).
            // We don't block forever — a 14 s cap covers the longest line.
            float t = 0f; const float MAX = 14f;
            while (t < MAX)
            {
                t += Time.unscaledDeltaTime;
                if (dialogueUI != null && !dialogueUI.gameObject.activeSelf) break;
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0) ||
                    Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E))
                {
                    dialogueUI.SkipTypewriter();
                    yield return new WaitForSecondsRealtime(0.4f);
                    break;
                }
                yield return null;
            }
            dialogueUI.Hide();
        }

        private IEnumerator FadeHologram(float from, float to, float dur)
        {
            float t = 0f;
            if (hologramGroup != null) hologramGroup.alpha = from;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / dur);
                if (hologramGroup != null) hologramGroup.alpha = Mathf.Lerp(from, to, k);
                if (hologramLight != null) hologramLight.intensity = Mathf.Lerp(from, to, k) * 1.4f;
                yield return null;
            }
            if (hologramGroup != null) hologramGroup.alpha = to;
            if (hologramLight != null) hologramLight.intensity = to * 1.4f;
        }

        private void ApplyHologramState(bool visible)
        {
            if (hologramVisual != null) hologramVisual.gameObject.SetActive(visible);
            if (hologramLight != null) hologramLight.enabled = visible;
        }
    }
}
