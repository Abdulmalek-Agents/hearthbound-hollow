// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Audio / AmbientAudio
//
// A persistent looping ambient bed for a scene (e.g. autumn wind in the lane,
// hearth crackle in the Hollow). Pulls its clip from an SfxLibrarySO entry id
// or accepts a direct AudioClip reference. Volume is gated by
// SettingsService.EffectiveVolume(Ambient).

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

        private AudioSource _src;
        private SettingsService _settings;
        private float _settingsCachedAmbientEffective = 1f;
        private float _fadeStartTime;

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

            float target = baseVolume * _settingsCachedAmbientEffective;
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

        private void OnSettingsChanged()
        {
            if (_settings != null)
                _settingsCachedAmbientEffective = _settings.EffectiveVolume(AudioChannel.Ambient);
        }

        private void OnDestroy()
        {
            if (_settings != null) _settings.OnSettingsChanged -= OnSettingsChanged;
        }
    }
}
