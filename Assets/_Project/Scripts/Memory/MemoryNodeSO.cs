// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Memory / MemoryNodeSO
//
// The canonical "Memory Card" from Codex 02 § 2.1. Each memory is a single
// ScriptableObject. Mission 1-2 authors only 2 (DOR-001, GER-007); the schema
// is the full 30-villager spec so we can expand to Mission 3+ without
// migrating data.

using System.Collections.Generic;
using UnityEngine;

namespace HearthboundHollow.Memory
{
    [CreateAssetMenu(menuName = "Hearthbound/Memory/Memory Node", fileName = "MEM-")]
    public class MemoryNodeSO : ScriptableObject
    {
        [Header("Identity")]
        public string id;                          // "DOR-001"
        public VillagerSO owner;
        public string title;                       // "First Loaves"

        [Header("Prose")]
        [TextArea(4, 12)] public string proseShort;
        [TextArea(6, 24)] public string proseFull;

        [Header("Emotional shape")]
        public EmotionalTone primaryTone = EmotionalTone.Joy;
        public EmotionalTone secondaryTone = EmotionalTone.Grace;
        [Range(0f, 1f)] public float weight = 0.4f;            // significance to giver
        [Range(0f, 1f)] public float initialClarity = 0.4f;    // freshness at shop entry
        [Range(0f, 1f)] public float crackIntensity = 0f;      // 0 for Polish-only, 0.6 for Cleanse

        [Header("Visual presentation")]
        public Sprite setpieceThumbnail;            // painted set-piece (Codex 11)
        public Color overrideTint = Color.clear;    // if != clear, overrides EmotionalTone tint
        [Range(0.5f, 2f)] public float sizeMultiplier = 1f;

        [Header("Dream sequencing")]
        public AudioClip dreamCue;                  // composer cue
        public ScriptableObject dreamTimeline;      // PlayableAsset cast at runtime
        public string dreamSequencerNodeId;         // for the variant table in Dream 2

        [Header("Echo Web — outbound connections")]
        public List<MemoryConnectionSO> echoes = new();

        [Header("Tariffs (Codex 10) — what does this memory cost to manipulate?")]
        public int polishCleanseCost = 5;           // coin cost of materials
        public int erasureCost = 0;                 // erasure is "free" in coin; weighty in vows

        [Header("Marin's note (predecessor hook)")]
        [TextArea(1, 3)] public string marinHandwrittenNote;

        public Color EffectiveTint
        {
            get
            {
                if (overrideTint.a > 0.001f) return overrideTint;
                return primaryTone.GetPaletteTint();
            }
        }
    }
}
