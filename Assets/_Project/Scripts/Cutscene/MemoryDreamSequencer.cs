// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Cutscene / MemoryDreamSequencer
//
// Spawns the Memory Dream Timeline for a given memory at end-of-day or on
// trigger. Variant-aware: Dream 2 has 5 variants keyed off the player's
// moral choice + cleanse quality (Focus 05 § 3.9).
//
// ── Phase 31.1 hotfix (2026-05-25) ─────────────────────────────────────
// User playtest screenshot showed the DreamCanvas (black letterbox bars +
// "The kitchen at first light…" prose) was visible in normal gameplay,
// not just during a dream timeline. Root cause: Phase 21 built the rig
// prefab with a Canvas + GraphicRaycaster active by default. The
// ActivationTrack on the Timeline never had a binding to actually flip
// the canvas, so the overlay was permanently on. Worse, the GraphicRaycaster
// + full-screen letterbox bars could intercept clicks meant for the
// dialogue box, making advance-on-click feel unresponsive.
//
// FIX — this sequencer now auto-discovers the DreamCanvas child in Awake()
// and force-hides it. PlayDream*() shows it for the duration of the
// timeline, and OnDirectorStopped re-hides it. The letterbox bars and
// prose label also have their raycastTarget zeroed so they can never
// block dialogue clicks even if the canvas is shown.

using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using HearthboundHollow.Core;
using HearthboundHollow.Memory;

namespace HearthboundHollow.Cutscene
{
    public class MemoryDreamSequencer : MonoBehaviour
    {
        [Header("Timeline player")]
        public PlayableDirector director;

        [Header("Dream UI (auto-discovered if null)")]
        [Tooltip("The dream's letterbox + prose canvas. Hidden until a dream " +
                 "timeline starts. If null, auto-discovered as the first " +
                 "child named 'DreamCanvas' under this rig.")]
        public Canvas dreamCanvas;

        [Header("Dream 1 (Doris's First Loaves) — single variant")]
        public PlayableAsset dream1;

        [Header("Dream 2 (Gerrold's seventh morning) — 5 variants")]
        public PlayableAsset dream2_VariantA_EraseClean;
        public PlayableAsset dream2_VariantB_CleansePartial;
        public PlayableAsset dream2_VariantC_CrossedCore;
        public PlayableAsset dream2_VariantD_Listen;
        public PlayableAsset dream2_VariantE_Defer;

        public event System.Action OnDreamFinished;

        private void Awake()
        {
            // Phase 31.1 — auto-discover + force-hide the dream canvas so
            // the letterbox + dream-prose overlay never bleeds into normal
            // gameplay.
            if (dreamCanvas == null)
            {
                var canvases = GetComponentsInChildren<Canvas>(includeInactive: true);
                foreach (var c in canvases)
                {
                    if (c != null && c.name == "DreamCanvas") { dreamCanvas = c; break; }
                }
            }
            if (dreamCanvas != null)
            {
                dreamCanvas.gameObject.SetActive(false);

                // Belt-and-braces: zero raycastTarget on every Graphic under
                // the dream canvas so even if it's accidentally re-enabled
                // it cannot intercept clicks meant for the dialogue box.
                foreach (var g in dreamCanvas.GetComponentsInChildren<Graphic>(includeInactive: true))
                {
                    if (g != null) g.raycastTarget = false;
                }
            }
        }

        public void PlayDream1() => Play(dream1);

        public void PlayDream2(MoralChoice choice, CleanseOutcome outcome)
        {
            PlayableAsset asset = choice switch
            {
                MoralChoice.Erase => dream2_VariantA_EraseClean,
                MoralChoice.Cleanse => outcome == CleanseOutcome.CrossedCore ? dream2_VariantC_CrossedCore
                                       : dream2_VariantB_CleansePartial,
                MoralChoice.Listen => dream2_VariantD_Listen,
                MoralChoice.Defer => dream2_VariantE_Defer,
                _ => dream2_VariantE_Defer,
            };
            Play(asset);
        }

        private void Play(PlayableAsset asset)
        {
            if (director == null || asset == null)
            {
                Hh.Warn(LogCategory.Cutscene, "MemoryDreamSequencer: director or asset null.");
                OnDreamFinished?.Invoke();
                return;
            }
            if (dreamCanvas != null) dreamCanvas.gameObject.SetActive(true);
            director.playableAsset = asset;
            director.stopped += OnDirectorStopped;
            director.Play();
            Hh.Log(LogCategory.Cutscene, $"Playing dream timeline '{asset.name}'.");
        }

        private void OnDirectorStopped(PlayableDirector d)
        {
            d.stopped -= OnDirectorStopped;
            if (dreamCanvas != null) dreamCanvas.gameObject.SetActive(false);
            OnDreamFinished?.Invoke();
            Hh.Log(LogCategory.Cutscene, "Dream timeline finished.");
        }
    }
}
