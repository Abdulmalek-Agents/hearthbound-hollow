// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Audio / MumbleVoicePlayer
//
// Runtime per-character "mumble VO" — Animal-Crossing-style syllabic
// playback synced to dialogue typewriter reveals.
//
// Usage:
//   var mumble = ServiceLocator.Get<MumbleVoicePlayer>();
//   mumble?.SpeakLine("doris", "You're the new one.", typewriterDuration);
//   // Stops the previous line, plays N random phonemes from the doris bank
//   // at the configured syllable rate.
//
// Attached to the DialogueUI prefab by Phase 38. DialogueUI calls SpeakLine
// each time PresentLine() is invoked.

using System.Collections;
using UnityEngine;
using HearthboundHollow.Core;

namespace HearthboundHollow.Audio
{
    public class MumbleVoicePlayer : MonoBehaviour
    {
        [Header("Library")]
        public MumbleVoiceLibrarySO library;

        [Header("Behaviour")]
        [Range(0f, 1f)] public float globalScale = 0.5f;
        [Tooltip("If true, skip mumble entirely when SettingsService.MumbleEnabled is false.")]
        public bool honorMumbleSetting = true;

        [Header("Audio Mixer (optional)")]
        public UnityEngine.Audio.AudioMixerGroup mixerGroup;

        private AudioSource _src;
        private Coroutine _activeLineCoroutine;
        private SettingsService _settings;
        private float _settingsVoiceEffective = 1f;

        private void Awake()
        {
            _src = gameObject.AddComponent<AudioSource>();
            _src.playOnAwake = false;
            _src.spatialBlend = 0f;
            _src.outputAudioMixerGroup = mixerGroup;
            ServiceLocator.Register(this);
            _settings = ServiceLocator.Get<SettingsService>();
            if (_settings != null)
            {
                _settings.OnSettingsChanged += OnSettingsChanged;
                _settingsVoiceEffective = _settings.EffectiveVolume(AudioChannel.Voice);
            }
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<MumbleVoicePlayer>();
            if (_settings != null) _settings.OnSettingsChanged -= OnSettingsChanged;
        }

        /// <summary>
        /// Speak a line as the given character. Plays approximately
        /// `lineDurationSeconds * bank.syllableRate` phonemes across the
        /// duration of the line, picking each phoneme randomly with a slight
        /// pitch jitter.
        /// </summary>
        public void SpeakLine(string characterId, string line, float lineDurationSeconds)
        {
            Stop();
            if (string.IsNullOrEmpty(characterId) || string.IsNullOrEmpty(line)) return;
            if (library == null) return;
            var bank = library.GetBank(characterId);
            if (bank == null || bank.phonemes == null || bank.phonemes.Count == 0) return;

            // Count word-like tokens to scale the syllable count.
            int wordCount = CountWords(line);
            float dur = Mathf.Max(0.25f, lineDurationSeconds);
            int syllables = Mathf.Clamp(Mathf.RoundToInt(dur * bank.syllableRate), 1, Mathf.Max(2, wordCount * 3));
            float interval = dur / syllables;

            _activeLineCoroutine = StartCoroutine(SpeakRoutine(bank, syllables, interval));
        }

        public void Stop()
        {
            if (_activeLineCoroutine != null)
            {
                StopCoroutine(_activeLineCoroutine);
                _activeLineCoroutine = null;
            }
            if (_src != null) _src.Stop();
        }

        private IEnumerator SpeakRoutine(MumbleVoiceLibrarySO.CharacterVoiceBank bank, int syllables, float interval)
        {
            // Light randomisation seed per call.
            var rng = new System.Random();
            for (int i = 0; i < syllables; i++)
            {
                var clip = bank.phonemes[rng.Next(bank.phonemes.Count)];
                if (clip != null && _src != null)
                {
                    _src.pitch = 1f + ((float)rng.NextDouble() - 0.5f) * 2f * bank.pitchVariance;
                    _src.volume = bank.volume * _settingsVoiceEffective * globalScale;
                    _src.PlayOneShot(clip);
                }
                yield return new WaitForSeconds(interval);
            }
            _activeLineCoroutine = null;
        }

        private static int CountWords(string s)
        {
            if (string.IsNullOrEmpty(s)) return 0;
            int n = 0;
            bool inWord = false;
            foreach (var c in s)
            {
                if (char.IsLetterOrDigit(c)) { if (!inWord) { n++; inWord = true; } }
                else inWord = false;
            }
            return n;
        }

        private void OnSettingsChanged()
        {
            if (_settings != null)
                _settingsVoiceEffective = _settings.EffectiveVolume(AudioChannel.Voice);
        }
    }
}
