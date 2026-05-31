// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Audio / SceneAudioBeacon
//
// Tiny scene-component that on Start() publishes a `SceneAudioRequestedEvent`
// with the music + ambience ids for the current scene. `MusicPlayer` and the
// scene's `AmbientAudio` instance crossfade to the requested cues.
//
// Phase 38 attaches one of these to each gameplay scene via the
// `Phase38_AudioAndCutsceneWiring` Editor builder. Editing the public fields
// at design time lets a future tuner override the auto-built defaults.

using UnityEngine;
using HearthboundHollow.Core;

namespace HearthboundHollow.Audio
{
    public class SceneAudioBeacon : MonoBehaviour
    {
        [Tooltip("Music id (matches an entry in MusicLibrarySO).")]
        public string musicId = "scene_lane";
        [Tooltip("Ambience id (matches an entry in AmbienceLibrarySO).")]
        public string ambienceId = "scene_lane";
        [Tooltip("Delay before publishing the request, in seconds. " +
                 "Lets a fade-in / loading screen settle first.")]
        [Range(0f, 5f)] public float delaySeconds = 0.25f;

        private void Start()
        {
            if (delaySeconds <= 0.001f)
            {
                Publish();
            }
            else
            {
                Invoke(nameof(Publish), delaySeconds);
            }
        }

        private void Publish()
        {
            EventBus.Publish(new SceneAudioRequestedEvent(musicId, ambienceId));
            Hh.Log(LogCategory.Audio,
                $"SceneAudioBeacon: requested music='{musicId}' ambience='{ambienceId}'");
        }
    }
}
