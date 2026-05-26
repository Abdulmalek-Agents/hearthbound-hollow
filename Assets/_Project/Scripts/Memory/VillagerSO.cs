// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Memory / VillagerSO
//
// Per-villager identity, portrait, voice motif, and the runtime-link to their
// Memory Map. In Mission 1-2 we author three: Doris, Gerrold, SilentLane.
// Schema supports the full 30 villagers from the Narrative Bible (Codex 02).

using UnityEngine;

namespace HearthboundHollow.Memory
{
    [CreateAssetMenu(menuName = "Hearthbound/Villager/Villager", fileName = "Villager")]
    public class VillagerSO : ScriptableObject
    {
        [Header("Identity")]
        public string villagerId;          // "doris", "gerrold", "silentlane"
        public string displayName;         // "Doris" (no surname in M1-2)
        public string archetypeBoZo;       // "Cleric", "Bard", "Warrior" — for prefab reskin reference

        [Header("Portrait")]
        public Sprite portraitNeutral;
        public Sprite portraitWarm;
        public Sprite portraitDowncast;

        [Header("Voice & Music")]
        public string voiceActorId;        // for VO routing (post-casting)
        public string motifCueId;          // composer cue identifier (e.g. "M-DOR-01")

        [Header("Yarn entry node")]
        [Tooltip("Default Yarn node to start when this villager is greeted (e.g. 'Doris_M1_Start').")]
        public string defaultYarnNode;

        [Header("Memory Map")]
        public VillagerMemoryMapSO memoryMap;

        [Header("Dialogue tuning")]
        [Tooltip("Lines-per-second cadence used by the DialogueUI typewriter effect.")]
        [Range(20, 90)] public int dialogueCps = 45;

        [Header("Eyes Animator hints")]
        [Tooltip("Blink rate (blinks/min) at neutral. Doris ~14, Gerrold ~6 (sad). M1-2 only.")]
        [Range(2, 30)] public int blinkPerMin = 14;
    }
}
