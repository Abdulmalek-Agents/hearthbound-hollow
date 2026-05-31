// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Audio / MumbleVoiceLibrarySO
//
// Per-character bank of mumble phoneme clips. Each character gets ~12 short
// (0.08-0.26 s) syllable clips synthesised by Phase 37 from a base-pitch +
// vowel-brightness + breath signature (per Codex 14 § 5.1 + Phase 35
// audit § "Per-character voice").
//
// At runtime, `MumbleVoicePlayer` picks one random phoneme per syllable
// during a dialogue line's typewriter reveal — gives the cozy
// Animal-Crossing / Hollow-Knight feel without requiring full VO recording.
//
// Per D-054: any future replacement is a pure asset swap — drop human-VO
// `.wav` files into `Assets/_Project/Audio/Voice/<character>/` with the
// same naming convention and the player automatically prefers them.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace HearthboundHollow.Audio
{
    [CreateAssetMenu(menuName = "Hearthbound/Audio/Mumble Voice Library", fileName = "MumbleVoiceLibrary")]
    public class MumbleVoiceLibrarySO : ScriptableObject
    {
        [Serializable]
        public class CharacterVoiceBank
        {
            public string characterId;         // "doris", "gerrold", "pickle", "marin"
            public List<AudioClip> phonemes = new();
            [Range(0f, 1f)] public float volume = 0.55f;
            [Range(0.5f, 2.0f)] public float pitchVariance = 0.08f;
            [Tooltip("Syllables per second when this character speaks. " +
                     "Higher = chattier (Pickle). Lower = thoughtful (Gerrold).")]
            [Range(2f, 16f)] public float syllableRate = 8.0f;
        }

        public List<CharacterVoiceBank> banks = new();

        public CharacterVoiceBank GetBank(string characterId)
        {
            foreach (var b in banks)
            {
                if (string.Equals(b.characterId, characterId, StringComparison.OrdinalIgnoreCase))
                    return b;
            }
            return null;
        }
    }
}
