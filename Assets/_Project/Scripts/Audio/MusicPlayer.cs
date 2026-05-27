// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Audio / MusicPlayer
//
// A persistent, scene-spanning music player that crossfades between cues.
// Spawned by `Phase38_AudioAndCutsceneWiring` on the Bootstrap scene with
// `surviveSceneLoad = true`. Each gameplay scene tells the MusicPlayer
// which cue to play; the MusicPlayer handles the crossfade so transitions
// (lane → hollow → garden → cottage → menu) feel continuous rather than
// hard-cut.
//
// Codex 14 § 4 — Vertical-Mix Dynamic Score: this is the v1 implementation.
// The full per-villager motif layering + tension layer + dream layer system
// is DEFERRED to post-M2. v1 supports a single base layer + crossfade.
//
// Volume gating: `SettingsService.EffectiveVolume(Music)` is honoured every
// frame. Mute/restore via `SetMute(bool)`.
//
// Phase 38 — subscribes to `SceneAudioRequestedEvent` so each scene's
// `SceneAudioBeacon` triggers a crossfade automatically on Start.
//
// Phase 43 — exposes `CurrentId` so SaveService can snapshot what's
// playing, and DreamAudioBinder can capture the pre-dream cue.
//
// Phase 45 — self-heals library reference via Resources.Load on Awake
// if the Inspector ref is null (fresh-clone / no-build-everything path).
//
// ── Phase 32.10 (2026-05-27) ─────────────────────────────────────
// Subscribes to VoiceClipStartedEvent / VoiceClipEndedEvent so music
// ducks down to `duckScale * normal` for the duration of each dialogue
// voice clip. Hit cozy games (Spiritfarer, Coffee Talk, Disco Elysium)
// all do this — dialogue beats land cleaner against a quieter score.
// Pure runtime tween (no AudioMixer dependency); restores on clip end
// or after a safety timer that matches the clip length.

using System.Collections;
using UnityEngine;
using HearthboundHollow.Core;

namespace HearthboundHollow.Audio
{
    public class MusicPlayer : MonoBehaviour
    {
        // Phase 45 — Resources-folder filename (no extension, no folder
        // prefix). Phase 37 writes the library to
        // Assets/_Project/Audio/Resources/MusicLibrary.asset so this
        // Resources.Load call resolves at runtime even when no scene-side
        // wiring is present.
        public const string ResourcesLibraryName = "MusicLibrary";

        [Header("Library")]
        public MusicLibrarySO library;

        [Header("Behaviour")]
        public bool surviveSceneLoad = true;
        [Range(0f, 1f)] public float globalScale = 0.85f;

        [Header("Audio Mixer (optional)")]
        public UnityEngine.Audio.AudioMixerGroup mixerGroup;

        [Header("Phase 32.10 — Voice ducking")]
        [Tooltip("How quiet the music gets while a voice clip is playing. " +
                 "0.55 = music drops to 55% of its normal volume — quiet enough " +
                 "for dialogue to feel cinematic, loud enough not to drop out.")]
        [Range(0f, 1f)] public float voiceDuckScale = 0.55f;
        [Tooltip("Seconds for the down-fade when a voice clip starts.")]
        [Range(0.01f, 1f)] public float voiceDuckInSec = 0.20f;
        [Tooltip("Seconds for the up-fade when the voice clip ends.")]
        [Range(0.01f, 2f)] public float voiceDuckOutSec = 0.45f;

        // Two AudioSources — one current, one fading out — for crossfade.
        private AudioSource _srcA;
        private AudioSource _srcB;
        private bool _activeIsA = true;

        private string _currentId;
        private float _currentBaseVolume = 1f;
        private SettingsService _settings;
        private float _settingsMusicEffective = 1f;
        private bool _muted;

        // Phase 32.10 — current voice-duck multiplier (smoothly tweened
        // from 1.0 to voiceDuckScale on VoiceClipStartedEvent, back to 1.0
        // on VoiceClipEndedEvent).
        private float _voiceDuck = 1f;
        private Coroutine _voiceDuckCo;

        /// <summary>
        /// The id of the cue currently playing (or fading in). Empty when
        /// nothing has been requested yet. Phase 43 — surfaced for save
        /// persistence; the SaveService snapshots this string and Phase 43's
        /// resume hook calls Play(CurrentId) after a load.
        /// </summary>
        public string CurrentId => _currentId ?? string.Empty;

        private void Awake()
        {
            if (surviveSceneLoad) DontDestroyOnLoad(gameObject);
            _srcA = MakeSource("MusicA");
            _srcB = MakeSource("MusicB");
            ServiceLocator.Register(this);
            _settings = ServiceLocator.Get<SettingsService>();
            if (_settings != null)
            {
                _settings.OnSettingsChanged += OnSettingsChanged;
                _settingsMusicEffective = _settings.EffectiveVolume(AudioChannel.Music);
            }
            // Phase 38 — subscribe to scene-driven music swaps.
            EventBus.Subscribe<SceneAudioRequestedEvent>(OnSceneAudioRequested);
            // Phase 32.10 — subscribe to voice duck events.
            EventBus.Subscribe<VoiceClipStartedEvent>(OnVoiceStarted);
            EventBus.Subscribe<VoiceClipEndedEvent>(OnVoiceEnded);

            // Phase 45 — self-heal: if the Inspector reference is null
            // (e.g. Phase 38 wiring not yet run), try to load the canonical
            // library from Resources/.
            if (library == null)
            {
                library = Resources.Load<MusicLibrarySO>(ResourcesLibraryName);
                if (library != null)
                    Hh.Log(LogCategory.Audio,
                        $"MusicPlayer: self-healed library reference from " +
                        $"Resources/{ResourcesLibraryName}.asset.");
            }
            if (library == null)
            {
                Hh.Warn(LogCategory.Audio,
                    $"MusicPlayer: NO LIBRARY WIRED. Music will be silent. " +
                    $"Run `Hearthbound → 🚀 Build Everything` to generate " +
                    $"Resources/{ResourcesLibraryName}.asset.");
            }
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<MusicPlayer>();
            if (_settings != null) _settings.OnSettingsChanged -= OnSettingsChanged;
            EventBus.Unsubscribe<SceneAudioRequestedEvent>(OnSceneAudioRequested);
            EventBus.Unsubscribe<VoiceClipStartedEvent>(OnVoiceStarted);
            EventBus.Unsubscribe<VoiceClipEndedEvent>(OnVoiceEnded);
        }

        private void OnSceneAudioRequested(SceneAudioRequestedEvent ev)
        {
            if (!string.IsNullOrEmpty(ev.MusicId)) Play(ev.MusicId);
        }

        // ── Phase 32.10 — voice ducking ────────────────────────────

        private void OnVoiceStarted(VoiceClipStartedEvent _)
        {
            if (_voiceDuckCo != null) StopCoroutine(_voiceDuckCo);
            _voiceDuckCo = StartCoroutine(TweenDuck(voiceDuckScale, voiceDuckInSec));
        }

        private void OnVoiceEnded(VoiceClipEndedEvent _)
        {
            if (_voiceDuckCo != null) StopCoroutine(_voiceDuckCo);
            _voiceDuckCo = StartCoroutine(TweenDuck(1f, voiceDuckOutSec));
        }

        private IEnumerator TweenDuck(float target, float duration)
        {
            float start = _voiceDuck;
            float t = 0f;
            float dur = Mathf.Max(0.01f, duration);
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                _voiceDuck = Mathf.Lerp(start, target, Mathf.Clamp01(t / dur));
                yield return null;
            }
            _voiceDuck = target;
            _voiceDuckCo = null;
        }

        private AudioSource MakeSource(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var src = go.AddComponent<AudioSource>();
            src.loop = true;
            src.playOnAwake = false;
            src.spatialBlend = 0f;  // 2D
            src.outputAudioMixerGroup = mixerGroup;
            return src;
        }

        /// <summary>
        /// Plays the music cue with the given id. If a different cue is
        /// already playing, crossfades over `fadeIn` seconds (taken from
        /// the library entry, falling back to 2 s).
        ///
        /// Calling this with the same id as the currently-playing cue is a
        /// no-op — re-entry-safe.
        /// </summary>
        public void Play(string id)
        {
            if (library == null)
            {
                Hh.Warn(LogCategory.Audio, $"MusicPlayer.Play('{id}') — no library wired.");
                return;
            }
            if (_currentId == id) return;

            var clip = library.Get(id, out var vol, out var loop, out var fadeIn, out var fadeOut);
            if (clip == null)
            {
                Hh.Warn(LogCategory.Audio, $"MusicPlayer.Play('{id}') — id not found in library.");
                return;
            }

            // Active source plays the new clip, inactive source fades out.
            var activeSrc = _activeIsA ? _srcA : _srcB;
            var inactiveSrc = _activeIsA ? _srcB : _srcA;

            // Hand off — the *new* clip goes to the inactiveSrc (which will become the new active).
            inactiveSrc.clip = clip;
            inactiveSrc.loop = loop;
            inactiveSrc.volume = 0f;
            inactiveSrc.Play();

            _activeIsA = !_activeIsA;
            _currentId = id;
            _currentBaseVolume = vol;

            StopAllCoroutines();
            // Restart the duck coroutine state if it was running — we
            // stopped it via StopAllCoroutines, so re-snap to the current
            // duck level (any in-flight tween would have been killed).
            // The next voice event will tween from here cleanly.
            StartCoroutine(Crossfade(activeSrc, inactiveSrc, fadeOut, fadeIn));
            Hh.Log(LogCategory.Audio, $"MusicPlayer: → '{id}' (fade {fadeIn:F1}s)");
        }

        public void Stop(float fadeOut = 1.5f)
        {
            _currentId = null;
            var active = _activeIsA ? _srcA : _srcB;
            var prev = _activeIsA ? _srcB : _srcA;
            StopAllCoroutines();
            StartCoroutine(FadeAndStop(active, fadeOut));
            StartCoroutine(FadeAndStop(prev, fadeOut));
        }

        public void SetMute(bool mute) { _muted = mute; }

        private IEnumerator Crossfade(AudioSource fromSrc, AudioSource toSrc, float fadeOut, float fadeIn)
        {
            float t = 0f;
            float dur = Mathf.Max(fadeOut, fadeIn, 0.05f);
            float fromStart = fromSrc != null && fromSrc.isPlaying ? fromSrc.volume : 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / dur);
                float target = ResolvedVolume();
                if (fromSrc != null) fromSrc.volume = Mathf.Lerp(fromStart, 0f, k);
                if (toSrc != null) toSrc.volume = target * k;
                yield return null;
            }
            if (fromSrc != null) { fromSrc.Stop(); fromSrc.clip = null; }
        }

        private IEnumerator FadeAndStop(AudioSource src, float dur)
        {
            if (src == null) yield break;
            float start = src.volume;
            float t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                src.volume = Mathf.Lerp(start, 0f, t / dur);
                yield return null;
            }
            src.Stop();
            src.clip = null;
        }

        private float ResolvedVolume()
        {
            if (_muted) return 0f;
            // Phase 32.10 — apply the voice-duck multiplier on top of the
            // settings + global scale.
            return _currentBaseVolume * _settingsMusicEffective * globalScale * _voiceDuck;
        }

        private void Update()
        {
            // Keep the active source's volume up-to-date with settings drag.
            var active = _activeIsA ? _srcA : _srcB;
            if (active != null && active.isPlaying)
            {
                active.volume = ResolvedVolume();
            }
        }

        private void OnSettingsChanged()
        {
            if (_settings != null)
                _settingsMusicEffective = _settings.EffectiveVolume(AudioChannel.Music);
        }
    }
}
