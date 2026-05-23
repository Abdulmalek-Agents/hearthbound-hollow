// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Cutscene / MemoryDreamSequencer
//
// Spawns the Memory Dream Timeline for a given memory at end-of-day or on
// trigger. Variant-aware: Dream 2 has 5 variants keyed off the player's
// moral choice + cleanse quality (Focus 05 § 3.9).

using UnityEngine;
using UnityEngine.Playables;
using HearthboundHollow.Core;
using HearthboundHollow.Memory;

namespace HearthboundHollow.Cutscene
{
    public class MemoryDreamSequencer : MonoBehaviour
    {
        [Header("Timeline player")]
        public PlayableDirector director;

        [Header("Dream 1 (Doris's First Loaves) — single variant")]
        public PlayableAsset dream1;

        [Header("Dream 2 (Gerrold's seventh morning) — 5 variants")]
        public PlayableAsset dream2_VariantA_EraseClean;
        public PlayableAsset dream2_VariantB_CleansePartial;
        public PlayableAsset dream2_VariantC_CrossedCore;
        public PlayableAsset dream2_VariantD_Listen;
        public PlayableAsset dream2_VariantE_Defer;

        public event System.Action OnDreamFinished;

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
            director.playableAsset = asset;
            director.stopped += OnDirectorStopped;
            director.Play();
            Hh.Log(LogCategory.Cutscene, $"Playing dream timeline '{asset.name}'.");
        }

        private void OnDirectorStopped(PlayableDirector d)
        {
            d.stopped -= OnDirectorStopped;
            OnDreamFinished?.Invoke();
            Hh.Log(LogCategory.Cutscene, "Dream timeline finished.");
        }
    }
}
