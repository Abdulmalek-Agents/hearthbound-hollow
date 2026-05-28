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
// ── Phase 60 — Arabic Localization MVP ──────────────────────────
// Play(lineId) is now LOCALE-AWARE. When Locale.Arabic is active and the
// entry has a non-null `clipAr` slot, the Arabic clip plays instead of
// the English `clip`. Falls back to English silently when the Arabic
// clip hasn't been recorded yet — the subtitle stays in Arabic (D-064).
//
// ── Phase 32.10 (2026-05-27) ────────────────────────────────────
// Interactive polish for the "best game" feel — VoicePlayer now publishes
// VoiceClipStartedEvent / VoiceClipEndedEvent on every Play / Stop.
// MusicPlayer + AmbientAudio subscribe and tween their volume down for
// the duration of the clip, then back up when the clip ends.

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
                Destroy(gameObject);
                return;
            }
            Instance = this;

            EnsureAudioSource();
            EnsureAudioListenerExists();

            if (library == null)
            {
                library = Resources.Load<VoiceLibrarySO>(ResourcesLibraryName);
                if (library == null)
                {
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
            source.spatialBlend = 0f;
            source.priority = 64;
            source.bypassEffects = false;
            source.bypassListenerEffects = false;
        }

        private void EnsureAudioListenerExists()
        {
            if (AudioListener.volume <= 0.001f) AudioListener.volume = 1f;
            AudioListener.pause = false;

#if UNITY_2022_3_OR_NEWER
            var existing = Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            var existing = Object.FindObjectsOfType<AudioListener>(includeInactive: true);
#endif
            if (existing != null && existing.Length > 0)
            {
                if (existing.Length > 1)
                {
                    for (int i = 1; i < existing.Length; i++)
                    {
                        if (existing[i] != null) Destroy(existing[i]);
                    }
                    Hh.Warn(LogCategory.Audio,
                        $"VoicePlayer: pruned {existing.Length - 1} duplicate AudioListener(s); " +
                        $"kept the one on '{existing[0].gameObject.name}'.");
                }
                return;
            }

            var host = Camera.main != null ? Camera.main.gameObject : gameObject;
            host.AddComponent<AudioListener>();
            Hh.Log(LogCategory.Audio,
                $"VoicePlayer: no AudioListener found — attached one to '{host.name}'. " +
                "Voice + ambient + SFX will now be audible.");
        }

        private void OnEnable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene,
                                   UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            EnsureAudioListenerExists();
        }

        /// <summary>
        /// Play the voice line associated with <paramref name="lineId"/>.
        /// Returns the clip length in seconds, or 0 if nothing was played.
        ///
        /// Phase 60 — Locale-aware. When Locale.Arabic is active and the
        /// entry's `clipAr` slot is non-null, the Arabic clip plays
        /// instead of `clip`. Falls back to English silently when the
        /// Arabic clip hasn't been recorded yet. The DialogueUI subtitle
        /// is ALWAYS translated by LocalizationService.GetDialogue —
        /// voice and subtitle can be at different stages of localization
        /// (D-064 / graceful degrade).
        /// </summary>
        public float Play(string lineId)
        {
            if (string.IsNullOrEmpty(lineId)) return 0f;
            if (library == null || source == null) return 0f;
            if (!library.TryGet(lineId, out var e)) return 0f;

            // Phase 60 — Locale-aware clip pick. Arabic locale prefers the
            // entry's `clipAr` slot; if absent, falls back to the English
            // `clip` so playback never breaks during translation work.
            // The subtitle is ALWAYS translated by DialogueUI — only the
            // audio degrades to English when the Arabic clip hasn't been
            // recorded yet.
            AudioClip chosen = e.clip;
            var loc = ServiceLocator.Get<LocalizationService>();
            if (loc != null && loc.CurrentLocale == Locale.Arabic && e.clipAr != null)
            {
                chosen = e.clipAr;
            }
            if (chosen == null) return 0f;

            // If there's a previous clip still playing, end it cleanly
            // (publishes the end event so duckers restore before we duck again).
            if (source.isPlaying && !string.IsNullOrEmpty(_activeLineId))
            {
                EventBus.Publish(new VoiceClipEndedEvent(_activeLineId));
            }

            source.Stop();
            source.clip   = chosen;
            source.volume = masterVolume * (e.volume <= 0f ? 1f : e.volume);
            source.pitch  = e.pitch  <= 0f ? 1f : e.pitch;
            source.Play();

            _activeLineId     = lineId;
            _activeWasPlaying = true;

            // Phase 32.10 — duck music + ambient for the clip's duration.
            EventBus.Publish(new VoiceClipStartedEvent(lineId, chosen.length));

            return chosen.length;
        }

        /// <summary>
        /// Immediately stop any voice playback. Safe to call any time.
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

        private void Update()
        {
            if (source == null) return;
            bool playing = source.isPlaying;
            if (_activeWasPlaying && !playing && !string.IsNullOrEmpty(_activeLineId))
            {
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
