// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Audio / MumbleVoicePlayer
//
// Runtime per-character "mumble VO" — Animal-Crossing-style syllabic
// playback synced to dialogue typewriter reveals.
//
// Phase 38 — subscribes to DialogueLineStartedEvent + DialogueLineEndedEvent
// from the EventBus, so any caller of `DialogueUI.PresentLine` triggers the
// matching character's syllable bank without UI taking a direct Audio dep.
//
// Phase 45 — self-heals library reference via Resources.Load if the
// Inspector-wired ref is null (e.g. the user pulled the branch but hasn't
// re-run Phase 38's WireBootstrapAudioRig yet). Also adds verbose
// diagnostic logging on the first failed lookup so the cause of silent
// dialogue is visible in the Console.
//
// ── Phase 32.10 (2026-05-27) ─────────────────────────────────────
// OnDialogueLineStarted now reads the new HasVoiceClip flag on the
// event payload. When true (DialogueUI just kicked off a real voice
// clip via VoicePlayer), the procedural mumble bank is SUPPRESSED for
// THIS line so we don't stack two voices. When false (legacy / silent-
// dialogue path), the original mumble-always-plays behaviour runs.
//
// Usage (manual call, if needed):
//   var mumble = ServiceLocator.Get<MumbleVoicePlayer>();
//   mumble?.SpeakLine("doris", "You're the new one.", typewriterDuration);

using System.Collections;
using UnityEngine;
using HearthboundHollow.Core;

namespace HearthboundHollow.Audio
{
    public class MumbleVoicePlayer : MonoBehaviour
    {
        // Phase 45 — Resources-folder filename (no extension, no folder
        // prefix). Phase 37 writes the library to
        // Assets/_Project/Audio/Resources/MumbleVoiceLibrary.asset so this
        // Resources.Load call resolves at runtime even when no scene-side
        // wiring is present.
        public const string ResourcesLibraryName = "MumbleVoiceLibrary";

        [Header("Library")]
        public MumbleVoiceLibrarySO library;

        [Header("Behaviour")]
        [Range(0f, 1f)] public float globalScale = 0.5f;
        [Tooltip("If true, skip mumble entirely when SettingsService.MumbleEnabled is false.")]
        public bool honorMumbleSetting = true;
        [Tooltip("Phase 45 — log per-line diagnostics (which bank, syllable count, " +
                 "interval). Default OFF to avoid Console spam in production.")]
        public bool verboseLogging = false;

        [Header("Audio Mixer (optional)")]
        public UnityEngine.Audio.AudioMixerGroup mixerGroup;

        private AudioSource _src;
        private Coroutine _activeLineCoroutine;
        private SettingsService _settings;
        private float _settingsVoiceEffective = 1f;
        // Phase 45 — single-shot warning flag so a null bank only logs once
        // per character, not on every dialogue line.
        private static readonly System.Collections.Generic.HashSet<string> _warnedCharacters = new();
        private static bool _warnedLibraryNull;

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

            // Phase 45 — self-heal: if the Inspector reference is null
            // (e.g. Phase 38 wiring not yet run, or scene reference broken),
            // try to load the canonical library from Resources/.
            if (library == null)
            {
                library = Resources.Load<MumbleVoiceLibrarySO>(ResourcesLibraryName);
                if (library != null)
                {
                    Hh.Log(LogCategory.Audio,
                        $"MumbleVoicePlayer: self-healed library reference from " +
                        $"Resources/{ResourcesLibraryName}.asset. Phase 38 wiring " +
                        $"would have been preferred — re-run `Hearthbound → 🚀 Build Everything`.");
                }
            }

            if (library == null)
            {
                if (!_warnedLibraryNull)
                {
                    _warnedLibraryNull = true;
                    Hh.Err(LogCategory.Audio,
                        $"MumbleVoicePlayer: NO LIBRARY WIRED. Dialogue will be silent. " +
                        $"Run `Hearthbound → 🚀 Build Everything` (specifically Phase 37 — " +
                        $"Procedural Audio Studio) to generate the library at " +
                        $"Resources/{ResourcesLibraryName}.asset.");
                }
            }

            // Phase 38 — subscribe to DialogueLineStartedEvent so we play
            // mumble syllables whenever DialogueUI presents a line. No
            // direct UI-asmdef reference required.
            EventBus.Subscribe<DialogueLineStartedEvent>(OnDialogueLineStarted);
            EventBus.Subscribe<DialogueLineEndedEvent>(OnDialogueLineEnded);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<MumbleVoicePlayer>();
            if (_settings != null) _settings.OnSettingsChanged -= OnSettingsChanged;
            EventBus.Unsubscribe<DialogueLineStartedEvent>(OnDialogueLineStarted);
            EventBus.Unsubscribe<DialogueLineEndedEvent>(OnDialogueLineEnded);
        }

        private void OnDialogueLineStarted(DialogueLineStartedEvent ev)
        {
            // Phase 32.10 — if DialogueUI just kicked off a real voice clip
            // for this line via VoicePlayer, suppress the procedural mumble
            // bank so we don't stack two voices on top of each other.
            // Backward-compat: ev.HasVoiceClip defaults to false for any
            // legacy publisher, so existing scenes that don't have a
            // VoiceLibrarySO entry for the line still get mumble.
            if (ev.HasVoiceClip)
            {
                if (verboseLogging)
                    Hh.Log(LogCategory.Audio,
                        $"MumbleVoicePlayer: suppressed for '{ev.Speaker}' " +
                        $"(real voice clip is playing).");
                return;
            }
            SpeakLine(ev.Speaker, ev.LineText, ev.EstimatedDurationSec);
        }

        private void OnDialogueLineEnded(DialogueLineEndedEvent _)
        {
            Stop();
        }

        /// <summary>
        /// Speak a line as the given character. Plays approximately
        /// `lineDurationSeconds * bank.syllableRate` phonemes across the
        /// duration of the line, picking each phoneme randomly with a slight
        /// pitch jitter.
        ///
        /// Phase 45 — adds detailed diagnostic logging on each failure mode
        /// (null library, unknown character, empty bank, null clips) so a
        /// silent dialogue is no longer mysterious.
        /// </summary>
        public void SpeakLine(string characterId, string line, float lineDurationSeconds)
        {
            Stop();
            if (string.IsNullOrEmpty(characterId)) return;
            if (string.IsNullOrEmpty(line)) return;

            // Phase 45 — graceful library re-fetch attempt (in case Phase 37
            // ran after this Awake and the Resources path now resolves).
            if (library == null)
            {
                library = Resources.Load<MumbleVoiceLibrarySO>(ResourcesLibraryName);
            }
            if (library == null)
            {
                // Already warned once in Awake; don't spam.
                return;
            }

            var bank = library.GetBank(characterId);
            if (bank == null)
            {
                if (_warnedCharacters.Add(characterId))
                {
                    Hh.Warn(LogCategory.Audio,
                        $"MumbleVoicePlayer.SpeakLine('{characterId}'): bank not found in " +
                        $"library. Known banks: {DescribeBanks()}. (Speaker is lower-cased by " +
                        $"DialogueUI; library bank ids are stored as authored.)");
                }
                return;
            }
            if (bank.phonemes == null || bank.phonemes.Count == 0)
            {
                if (_warnedCharacters.Add(characterId + "::phonemes"))
                {
                    Hh.Warn(LogCategory.Audio,
                        $"MumbleVoicePlayer.SpeakLine('{characterId}'): bank has no phonemes. " +
                        $"Re-run Phase 37 to regenerate the Mumble VO clips.");
                }
                return;
            }

            // Count word-like tokens to scale the syllable count.
            int wordCount = CountWords(line);
            float dur = Mathf.Max(0.25f, lineDurationSeconds);
            int syllables = Mathf.Clamp(Mathf.RoundToInt(dur * bank.syllableRate), 1, Mathf.Max(2, wordCount * 3));
            float interval = dur / syllables;

            if (verboseLogging)
            {
                Hh.Log(LogCategory.Audio,
                    $"MumbleVoicePlayer: '{characterId}' x{syllables} @ {interval * 1000:F0} ms " +
                    $"(line='{(line.Length > 40 ? line.Substring(0, 40) + "…" : line)}')");
            }

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
            int nullClipCount = 0;
            for (int i = 0; i < syllables; i++)
            {
                var clip = bank.phonemes[rng.Next(bank.phonemes.Count)];
                if (clip != null && _src != null)
                {
                    _src.pitch = 1f + ((float)rng.NextDouble() - 0.5f) * 2f * bank.pitchVariance;
                    _src.volume = bank.volume * _settingsVoiceEffective * globalScale;
                    _src.PlayOneShot(clip);
                }
                else if (clip == null)
                {
                    nullClipCount++;
                }
                yield return new WaitForSeconds(interval);
            }
            if (nullClipCount > 0 && _warnedCharacters.Add(bank.characterId + "::nullclips"))
            {
                Hh.Warn(LogCategory.Audio,
                    $"MumbleVoicePlayer: bank '{bank.characterId}' has {nullClipCount}/{syllables} " +
                    $"null phoneme clips. Re-run Phase 37 to regenerate the .wav files.");
            }
            _activeLineCoroutine = null;
        }

        private string DescribeBanks()
        {
            if (library == null || library.banks == null || library.banks.Count == 0) return "<none>";
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < library.banks.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append("'").Append(library.banks[i].characterId).Append("'");
            }
            return sb.ToString();
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
