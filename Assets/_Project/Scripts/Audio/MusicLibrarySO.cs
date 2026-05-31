// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Audio / MusicLibrarySO
//
// Catalog of music cues — main theme, per-scene themes, dream stems,
// per-villager motifs. Built by Phase 37 (Procedural Audio Studio) and
// consumed at runtime by `MusicPlayer` + `MemoryDreamSequencer` + the
// scene builders.
//
// Per `Docs/Phase35_Continuation_Audit.md` § 5 (D-053), the audio assets
// referenced by this catalog live under `Assets/_Project/Audio/Generated/`
// (procedurally synthesised; deterministic re-generation by Phase 37) or
// `Assets/_Project/Audio/Music/` (human-authored composer drops, override
// the procedural baseline).

using System;
using System.Collections.Generic;
using UnityEngine;

namespace HearthboundHollow.Audio
{
    [CreateAssetMenu(menuName = "Hearthbound/Audio/Music Library", fileName = "MusicLibrary")]
    public class MusicLibrarySO : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public string id;             // canonical key: "scene_lane", "dream_doris_motif", etc.
            public AudioClip clip;
            [Range(0f, 1f)] public float volume;
            public bool loop;
            [Range(0f, 10f)] public float fadeInSeconds;
            [Range(0f, 10f)] public float fadeOutSeconds;
        }

        public List<Entry> entries = new();

        public AudioClip Get(string id, out float volume, out bool loop, out float fadeIn, out float fadeOut)
        {
            foreach (var e in entries)
            {
                if (e.id == id)
                {
                    volume = e.volume;
                    loop = e.loop;
                    fadeIn = e.fadeInSeconds;
                    fadeOut = e.fadeOutSeconds;
                    return e.clip;
                }
            }
            volume = 1f;
            loop = true;
            fadeIn = 2f;
            fadeOut = 2f;
            return null;
        }

        public bool Has(string id)
        {
            foreach (var e in entries) if (e.id == id) return true;
            return false;
        }
    }
}
