// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Audio / AmbientAudio
//
// A persistent looping ambient bed for a scene (e.g. autumn wind in the lane,
// hearth crackle in the Hollow). Pulls its clip from an SfxLibrarySO entry id
// or accepts a direct AudioClip reference. Volume is gated by
// SettingsService.EffectiveVolume(Ambient).
//
// ── Phase 32.10 (2026-05-27) ─────────────────────────────────────
// Subscribes to VoiceClipStartedEvent / VoiceClipEndedEvent so the
// ambient bed dips slightly while a voice clip plays. Subtle — the
// ambient is already at low volume; we drop it 25% to ~0.75× during
// voice. Same cinematic ducking pattern as MusicPlayer, just gentler.

using System.Collections;
using UnityEngine;
using HearthboundHollow.Core;

namespace HearthboundHollow.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class AmbientAudio : MonoBehaviour
    {
        [Header("Library lookup (preferred)")]
        public SfxLibrarySO library;
        public string libraryEntryId = "ambient_autumn_loop";

        [Header("Direct clip (fallback)")]
        public AudioClip directClip;

        [Header("Behaviour")]
        [Range(0f, 1f)] public float baseVolume = 0.4f;
        public bool playOnStart = true;
        public bool surviveSceneLoad = false;
        [Range(0f, 5f)] public float fadeInSeconds = 2.0f;

        [Header("Phase 32.10 — Voice ducking")]
        [Tooltip("How quiet the ambient bed gets while a voice clip is playing. " +
                 "0.75 = ambient drops to 75% of its normal volume. Subtle — " +
                 "the ambient is already quiet, so a gentle dip is enough.")]
        [Range(0f, 1f)] public float voiceDuckScale = 0.75f;
        [Tooltip("Seconds for the down-fade when a voice clip starts.")]
        [Range(0.01f, 1f)] public float voiceDuckInSec = 0.20f;
        [Tooltip("Seconds for the up-fade when the voice clip ends.")]
        [Range(0.01f, 2f)] public float voiceDuckOutSec = 0.45f;

        private AudioSource _src;
        private SettingsService _settings;
        private float _settingsCachedAmbientEffective = 1f;
        private float _fadeStartTime;

        // Phase 32.10 — current voice-duck multiplier.
        private float _voiceDuck = 1f;
        private Coroutine _voiceDuckCo;

        private void Awake()
        {
            _src = GetComponent<AudioSource>();
            _src.loop = true;
            _src.spatialBlend = 0f; // 2D
            _src.playOnAwake = false;

            if (surviveSceneLoad) DontDestroyOnLoad(gameObject);

            _settings = ServiceLocator.Get<SettingsService>();
            if (_settings != null)
            {
                _settings.OnSettingsChanged += OnSettingsChanged;
                _settingsCachedAmbientEffective = _settings.EffectiveVolume(AudioChannel.Ambient);
            }

            // Phase 32.10 — subscribe to voice duck events.
            EventBus.Subscribe<VoiceClipStartedEvent>(OnVoiceStarted);
            EventBus.Subscribe<VoiceClipEndedEvent>(OnVoiceEnded);
        }

        private void Start()
        {
            if (playOnStart) Play();
        }

        public void Play()
        {
            var clip = ResolveClip();
            if (clip == null)
            {
                Hh.Warn(LogCategory.Audio, $"AmbientAudio: no clip for '{libraryEntryId}'. Silent.");
                return;
            }

            _src.clip = clip;
            _src.volume = 0f;
            _src.Play();
            _fadeStartTime = Time.time;
            Hh.Log(LogCategory.Audio, $"AmbientAudio playing '{clip.name}' loop.");
        }

        public void Stop()
        {
            if (_src != null) _src.Stop();
        }

        private void Update()
        {
            if (_src == null || !_src.isPlaying) return;

            // Phase 32.10 — apply the voice-duck multiplier on top of the
            // fade-in + settings.
            float target = baseVolume * _settingsCachedAmbientEffective * _voiceDuck;
            float t = (Time.time - _fadeStartTime) / Mathf.Max(0.001f, fadeInSeconds);
            _src.volume = Mathf.Lerp(0f, target, Mathf.Clamp01(t));
        }

        private AudioClip ResolveClip()
        {
            if (directClip != null) return directClip;
            if (library == null) return null;
            foreach (var e in library.entries)
            {
                if (e.id == libraryEntryId) return e.clip;
            }
            return null;
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

        private void OnSettingsChanged()
        {
            if (_settings != null)
                _settingsCachedAmbientEffective = _settings.EffectiveVolume(AudioChannel.Ambient);
        }

        private void OnDestroy()
        {
            if (_settings != null) _settings.OnSettingsChanged -= OnSettingsChanged;
            EventBus.Unsubscribe<VoiceClipStartedEvent>(OnVoiceStarted);
            EventBus.Unsubscribe<VoiceClipEndedEvent>(OnVoiceEnded);
        }
    }
}
