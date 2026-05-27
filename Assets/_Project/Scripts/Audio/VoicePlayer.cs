// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Audio / VoicePlayer
//
// Phase 32 — Voice Acting MVP.
//
// Runtime singleton that plays a Doris (or any character) voice clip looked
// up by lineId in `VoiceLibrarySO`. Plays through a non-spatial 2D
// AudioSource so dialogue is heard consistently regardless of where the NPC
// is in the world.
//
// Wiring (canonical):
//   * Scene-baked rig (preferred): Phase 38's Bootstrap audio rig (or the
//     Phase 45 RuntimeAudioBootstrap fallback) hosts the VoicePlayer
//     alongside MusicPlayer / MumbleVoicePlayer / AmbientAudio.
//   * Belt-and-braces: `GameManager.Awake()` parents a fresh VoicePlayer
//     under itself if Instance is still null after one frame. This means
//     even a fresh clone with no scene wiring + no Phase 38 builder ever
//     run will get a voice rig.
//
// Decoupling:
//   * DialogueUI calls `VoicePlayer.Instance.Play(lineId)` from its
//     PresentLine() overload. If the singleton is null OR the library is
//     null OR the lineId is missing OR the clip is null, Play() returns 0f
//     and DialogueUI falls back to its previous typewriter-only behaviour.
//     Zero regression risk on installs without HearthboundVoiceLibrary.
//
// D-058: voice clips live under Assets/_Project/Audio/Voice/{character}/{lineId}.wav.
// Any TTS that produces 22 kHz mono PCM16 .wav drops in without code changes.
//
// ── Phase 32.10 (2026-05-27) ────────────────────────────────────
// Interactive polish for the "best game" feel — VoicePlayer now publishes
// VoiceClipStartedEvent / VoiceClipEndedEvent on every Play / Stop.
// MusicPlayer + AmbientAudio subscribe and tween their volume down for
// the duration of the clip, then back up when the clip ends. Gives the
// dialogue beat the cinematic ducked feel that hit cozy games rely on
// (Disco Elysium / Spiritfarer / Coffee Talk). End-event also fires
// automatically when the clip's natural duration elapses (Update loop
// watches `source.isPlaying` flip false).

using UnityEngine;
using HearthboundHollow.Core;

namespace HearthboundHollow.Audio
{
    [DefaultExecutionOrder(-20)]
    public class VoicePlayer : MonoBehaviour
    {
        /// <summary>
        /// Resources-path key (sans extension and "Resources/" prefix) under
        /// which the canonical VoiceLibrarySO asset is saved. The runtime
        /// auto-loader uses this when no inspector reference is wired.
        /// </summary>
        public const string ResourcesLibraryName = "HearthboundVoiceLibrary";

        [Tooltip("If null, auto-loaded from Resources/HearthboundVoiceLibrary on Awake.")]
        public VoiceLibrarySO library;

        [Tooltip("The 2D AudioSource through which dialogue plays. " +
                 "Auto-created on Awake if null.")]
        public AudioSource source;

        [Tooltip("Master volume scalar applied on top of each entry's volume. " +
                 "Persisted via SettingsService.MasterVoiceVolume in a future hook.")]
        [Range(0f, 1f)] public float masterVolume = 0.9f;

        public static VoicePlayer Instance { get; private set; }

        // Phase 32.10 — track the clip we last started so the natural-end
        // detector can publish the matching VoiceClipEndedEvent.
        private string _activeLineId;
        private bool   _activeWasPlaying;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                // A second VoicePlayer was spawned (e.g. scene-baked rig +
                // GameManager fallback). Keep the first, discard the rest.
                Destroy(gameObject);
                return;
            }
            Instance = this;

            EnsureAudioSource();

            if (library == null)
            {
                library = Resources.Load<VoiceLibrarySO>(ResourcesLibraryName);
                if (library == null)
                {
                    // Not fatal — DialogueUI tolerates a null library.
                    Hh.Warn(LogCategory.Audio,
                        "VoicePlayer: no VoiceLibrarySO assigned and none found at " +
                        $"Resources/{ResourcesLibraryName}. Voice playback disabled; " +
                        "typewriter dialogue will still work. " +
                        "Run Hearthbound → ⚙️ Advanced → Phase 32 — Rebuild Voice Library " +
                        "after `bash Tools/generate_voices.sh`.");
                }
                else
                {
                    Hh.Log(LogCategory.Audio,
                        $"VoicePlayer: loaded {library.ValidEntryCount} voice entries from " +
                        $"Resources/{ResourcesLibraryName}.");
                }
            }
        }

        private void EnsureAudioSource()
        {
            if (source != null) return;
            source = GetComponent<AudioSource>();
            if (source == null) source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;       // 2D — dialogue is non-spatial.
            source.priority = 64;            // Above SFX (128 default); music sits higher (~32).
            source.bypassEffects = false;
            source.bypassListenerEffects = false;
        }

        /// <summary>
        /// Play the voice line associated with <paramref name="lineId"/>.
        /// Returns the clip length in seconds, or 0 if nothing was played.
        /// Safe to call with a null/empty lineId.
        ///
        /// Phase 32.10 — publishes <see cref="VoiceClipStartedEvent"/> so
        /// MusicPlayer / AmbientAudio can duck. If a previous clip was
        /// still in flight, fires its end event first (so subscribers
        /// who duck on count don't get confused).
        /// </summary>
        public float Play(string lineId)
        {
            if (string.IsNullOrEmpty(lineId)) return 0f;
            if (library == null || source == null) return 0f;
            if (!library.TryGet(lineId, out var e) || e.clip == null) return 0f;

            // If there's a previous clip still playing, end it cleanly
            // (publishes the end event so duckers restore before we duck again).
            if (source.isPlaying && !string.IsNullOrEmpty(_activeLineId))
            {
                EventBus.Publish(new VoiceClipEndedEvent(_activeLineId));
            }

            source.Stop();
            source.clip   = e.clip;
            source.volume = masterVolume * (e.volume <= 0f ? 1f : e.volume);
            source.pitch  = e.pitch  <= 0f ? 1f : e.pitch;
            source.Play();

            _activeLineId     = lineId;
            _activeWasPlaying = true;

            // Phase 32.10 — duck music + ambient for the clip's duration.
            EventBus.Publish(new VoiceClipStartedEvent(lineId, e.clip.length));

            return e.clip.length;
        }

        /// <summary>
        /// Immediately stop any voice playback. Safe to call any time.
        /// Phase 32.10 — publishes <see cref="VoiceClipEndedEvent"/> so
        /// duckers can restore their channels right away (don't wait for
        /// the Update polling loop).
        /// </summary>
        public void Stop()
        {
            if (source != null && source.isPlaying) source.Stop();
            if (!string.IsNullOrEmpty(_activeLineId))
            {
                EventBus.Publish(new VoiceClipEndedEvent(_activeLineId));
                _activeLineId     = null;
                _activeWasPlaying = false;
            }
        }

        public bool IsPlaying => source != null && source.isPlaying;

        /// <summary>
        /// Phase 32.10 — natural-end detector. AudioSource doesn't fire an
        /// event when its clip finishes; we watch `isPlaying` and publish
        /// <see cref="VoiceClipEndedEvent"/> on the falling edge.
        /// </summary>
        private void Update()
        {
            if (source == null) return;
            bool playing = source.isPlaying;
            if (_activeWasPlaying && !playing && !string.IsNullOrEmpty(_activeLineId))
            {
                // The clip just finished naturally (not via Stop()).
                EventBus.Publish(new VoiceClipEndedEvent(_activeLineId));
                _activeLineId     = null;
                _activeWasPlaying = false;
            }
            _activeWasPlaying = playing;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
