// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Audio / VoiceLibrarySO
//
// Phase 32 — Voice Acting MVP.
//
// A ScriptableObject map from a stable line ID (e.g. "doris_m1_greet_01")
// to an AudioClip. `Mission01Director.Line(...)` now passes the lineId all
// the way through `DialogueUI.PresentLine(...)` to `VoicePlayer.Play(...)`,
// which looks the clip up in this library and plays it through the dialogue
// 2D AudioSource.
//
// D-058: voice clips live under Assets/_Project/Audio/Voice/{character}/{lineId}.wav.
// The generation pipeline (Tools/generate_voices.sh on macOS using `say`) is
// fully decoupled from the runtime. Any TTS that produces 22 kHz mono PCM16
// .wav can drop in (ElevenLabs / XTTS / Piper) — just overwrite the .wav files
// and the SO re-binds on the next OnValidate / editor-utility rescan.
//
// The library asset itself is generated at
// `Assets/_Project/Resources/HearthboundVoiceLibrary.asset` so the runtime
// `VoicePlayer` can load it via `Resources.Load<VoiceLibrarySO>(...)` even when
// no scene-level reference is wired. The asset is populated by the editor
// utility `Phase32_VoiceLibraryBuilder` (Hearthbound → ⚙️ Advanced submenu),
// which scans Audio/Voice/ and writes an entry per .wav file.

using System.Collections.Generic;
using UnityEngine;

namespace HearthboundHollow.Audio
{
    [CreateAssetMenu(menuName = "Hearthbound/Audio/Voice Library",
                     fileName = "HearthboundVoiceLibrary")]
    public class VoiceLibrarySO : ScriptableObject
    {
        [System.Serializable]
        public struct Entry
        {
            [Tooltip("Stable line ID. Matches Mission01Director's Line(lineId:...) calls. " +
                     "By convention the same string is the .wav filename basename.")]
            public string lineId;

            public AudioClip clip;

            [Tooltip("Per-clip volume scalar. 1 = neutral. " +
                     "VoicePlayer.masterVolume multiplies this.")]
            [Range(0.5f, 1.5f)] public float volume;

            [Tooltip("Per-clip pitch scalar. 1 = neutral. Used to colour a TTS " +
                     "voice across characters without regenerating the audio.")]
            [Range(0.7f, 1.3f)] public float pitch;
        }

        [Tooltip("All voice line entries. Edit by hand or auto-populate via " +
                 "Hearthbound → ⚙️ Advanced → Phase 32 — Rebuild Voice Library.")]
        public List<Entry> entries = new();

        // Built lazily on first TryGet — cleared whenever the inspector touches us.
        private Dictionary<string, Entry> _index;

        /// <summary>
        /// Look up a voice entry by line ID. Builds the lookup dictionary on
        /// first call and caches it until <see cref="OnValidate"/> fires.
        /// </summary>
        public bool TryGet(string lineId, out Entry entry)
        {
            entry = default;
            if (string.IsNullOrEmpty(lineId)) return false;
            if (_index == null)
            {
                _index = new Dictionary<string, Entry>(entries != null ? entries.Count : 0);
                if (entries != null)
                {
                    foreach (var e in entries)
                    {
                        if (!string.IsNullOrEmpty(e.lineId))
                            _index[e.lineId] = e;
                    }
                }
            }
            return _index.TryGetValue(lineId, out entry);
        }

        /// <summary>
        /// Count of valid (non-empty lineId) entries. Used by diagnostics.
        /// </summary>
        public int ValidEntryCount
        {
            get
            {
                if (entries == null) return 0;
                int n = 0;
                foreach (var e in entries)
                    if (!string.IsNullOrEmpty(e.lineId)) n++;
                return n;
            }
        }

        private void OnValidate()
        {
            // Invalidate the lookup cache when the inspector edits the list.
            _index = null;
        }
    }
}
