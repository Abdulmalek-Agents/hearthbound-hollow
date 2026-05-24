// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Audio / SfxLibrarySO
//
// A catalog of all audio clips referenced by gameplay events. Built once
// per project (by Phase 18's Editor menu) and consumed at runtime by the
// SfxPlayer MonoBehaviour. This indirection means:
//
//   * Designers can swap clips without touching code.
//   * The runtime never hardcodes paths — every clip is referenced by a
//     stable string key (e.g. "polish_rub_loop") that matches the events
//     the mini-games and dialogue raise.
//
// Lives in the HearthboundHollow.Audio asmdef so any subsystem can call
// SfxPlayer.Play("...") without taking a dep on Memory / Player / etc.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace HearthboundHollow.Audio
{
    [CreateAssetMenu(menuName = "Hearthbound/Audio/Sfx Library", fileName = "SfxLibrary")]
    public class SfxLibrarySO : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public string id;             // canonical key — "polish_rub_loop", "ui_click", etc.
            public AudioClip clip;
            [Range(0f, 1f)] public float volume;
            public bool loop;
        }

        public List<Entry> entries = new();

        public AudioClip Get(string id, out float volume, out bool loop)
        {
            foreach (var e in entries)
            {
                if (e.id == id) { volume = e.volume; loop = e.loop; return e.clip; }
            }
            volume = 1f; loop = false;
            return null;
        }

        public bool Has(string id)
        {
            foreach (var e in entries) if (e.id == id) return true;
            return false;
        }
    }
}
