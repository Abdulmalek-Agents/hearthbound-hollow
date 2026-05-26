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

using System.Collections;
using UnityEngine;
using HearthboundHollow.Core;

namespace HearthboundHollow.Audio
{
    public class MusicPlayer : MonoBehaviour
    {
        [Header("Library")]
        public MusicLibrarySO library;

        [Header("Behaviour")]
        public bool surviveSceneLoad = true;
        [Range(0f, 1f)] public float globalScale = 0.85f;

        [Header("Audio Mixer (optional)")]
        public UnityEngine.Audio.AudioMixerGroup mixerGroup;

        // Two AudioSources — one current, one fading out — for crossfade.
        private AudioSource _srcA;
        private AudioSource _srcB;
        private bool _activeIsA = true;

        private string _currentId;
        private float _currentBaseVolume = 1f;
        private SettingsService _settings;
        private float _settingsMusicEffective = 1f;
        private bool _muted;

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
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<MusicPlayer>();
            if (_settings != null) _settings.OnSettingsChanged -= OnSettingsChanged;
            EventBus.Unsubscribe<SceneAudioRequestedEvent>(OnSceneAudioRequested);
        }

        private void OnSceneAudioRequested(SceneAudioRequestedEvent ev)
        {
            if (!string.IsNullOrEmpty(ev.MusicId)) Play(ev.MusicId);
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
            return _currentBaseVolume * _settingsMusicEffective * globalScale;
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
