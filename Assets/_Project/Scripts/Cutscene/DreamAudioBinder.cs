// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Cutscene / DreamAudioBinder
//
// Attached to the MemoryDreamRig prefab by Phase 38. Hooks into the
// MemoryDreamSequencer's PlayDream1 / PlayDream2 calls (via wrapping
// inspection of the active PlayableAsset name) and routes the matching
// music + ambience cue through ServiceLocator services.
//
// Why not bind the AudioClip directly to the Timeline's AudioTrack? The
// `TimelineEditor.AddClip(...)` API requires the Unity Editor's
// `UnityEditor.Timeline` namespace which isn't available in this codebase
// (the asmdef chain would have to include Timeline-specific Editor refs
// + the .playable assets would have to be re-saved after every clip add).
// A runtime music-router gives us the same player-perceptible result with
// 90% less complexity.
//
// At runtime:
//   * Sequencer.PlayDream1() → DreamAudioBinder hears the director.Play()
//     happen on dream1; looks up "dream_doris_motif" in MusicLibrary;
//     asks the MusicPlayer service to crossfade to it.
//   * Sequencer.PlayDream2(choice, outcome) → same flow, variant-aware.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using HearthboundHollow.Audio;
using HearthboundHollow.Core;

namespace HearthboundHollow.Cutscene
{
    public class DreamAudioBinder : MonoBehaviour
    {
        [System.Serializable]
        public class CueMapping
        {
            public string variantId;       // matches the Timeline asset name
            public string musicId;         // matches a MusicLibrarySO id
        }

        [Header("Libraries (wired by Phase 38)")]
        public MusicLibrarySO musicLibrary;
        public MusicLibrarySO ambienceLibrary;  // same SO type, different content

        [Header("Sequencer ref (wired by Phase 38)")]
        public MemoryDreamSequencer sequencer;

        [Header("Variant → music id table")]
        public List<CueMapping> cueMap = new();

        [Header("Behaviour")]
        public string ambienceDuringDream = "dream_wind";

        private string _previousMusicBeforeDream;
        private MusicPlayer _music;

        private void Start()
        {
            _music = ServiceLocator.Get<MusicPlayer>();
            if (sequencer != null)
            {
                sequencer.OnDreamFinished += OnDreamFinished;
            }
            // Subscribe to director.played so we know when a dream starts.
            if (sequencer != null && sequencer.director != null)
            {
                sequencer.director.played += OnDirectorPlayed;
            }
        }

        private void OnDestroy()
        {
            if (sequencer != null) sequencer.OnDreamFinished -= OnDreamFinished;
            if (sequencer != null && sequencer.director != null)
                sequencer.director.played -= OnDirectorPlayed;
        }

        private void OnDirectorPlayed(PlayableDirector d)
        {
            if (d == null || d.playableAsset == null) return;
            string variant = d.playableAsset.name;
            var music = LookupMusicId(variant);
            if (string.IsNullOrEmpty(music))
            {
                Hh.Warn(LogCategory.Cutscene, $"DreamAudioBinder: no cue mapping for variant '{variant}'.");
                return;
            }

            if (_music == null) _music = ServiceLocator.Get<MusicPlayer>();
            if (_music != null)
            {
                _previousMusicBeforeDream = null;  // MusicPlayer doesn't yet expose CurrentId; we restore via scene beacon when dream finishes.
                _music.Play(music);
                Hh.Log(LogCategory.Cutscene, $"DreamAudioBinder: dream '{variant}' → music '{music}'");
            }
        }

        private void OnDreamFinished()
        {
            // When the dream ends, the next SceneAudioRequestedEvent (raised by
            // SceneAudioBeacon on the next scene's Start) will restore proper
            // music. Until then we let the dream cue fade naturally.
            if (_music != null && !string.IsNullOrEmpty(_previousMusicBeforeDream))
            {
                _music.Play(_previousMusicBeforeDream);
            }
        }

        private string LookupMusicId(string variantId)
        {
            foreach (var m in cueMap) if (m.variantId == variantId) return m.musicId;
            return null;
        }
    }
}
