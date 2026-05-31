// =============================================================================
// VillagerProfileSO.cs — Hearthbound Hollow
// Data bible for a single Saltmere villager across all 30 missions.
// Phase 76 — 30-Mission Architecture.
// =============================================================================
using System.Collections.Generic;
using UnityEngine;

namespace HearthboundHollow.Memory
{
    /// <summary>
    /// Everything the studio knows about a single villager.
    /// Created by Phase76_MissionArchitectureBuilder for all 20+ named characters.
    /// </summary>
    [CreateAssetMenu(
        fileName = "Villager_",
        menuName  = "Hearthbound/Villager Profile",
        order     = 2)]
    public class VillagerProfileSO : ScriptableObject
    {
        // ─── Identity ────────────────────────────────────────────────────────

        [Header("Identity")]
        public string villagerId;      // stable, e.g. "doris_baker"
        public string displayName;     // "Doris"
        public string fullName;        // "Doris Fen"
        public int    age;
        public string occupation;

        // ─── Narrative voice ─────────────────────────────────────────────────

        [Header("Narrative Voice")]
        [TextArea(2, 6)]
        [Tooltip("Voice signature: 2-3 sentences describing how this character sounds on the page.")]
        public string voiceSignature;

        [TextArea(2, 4)]
        [Tooltip("Their defining emotional wound or gift (not their mission plot — the human beneath it).")]
        public string emotionalCore;

        // ─── Memory Map ──────────────────────────────────────────────────────

        [Header("Memory Map")]
        [Tooltip("All known memories for this villager (12-20 for major characters).")]
        public List<MemoryMapEntry> memoryMap = new();

        [Tooltip("Which mission index this villager is introduced in (0-based).")]
        public int introducedInMission;

        [Tooltip("True if this villager is one of the 6 sealed-fragment holders.")]
        public bool isSealedFragmentHolder;
        public int  sealedFragmentIndex = -1;

        // ─── Voice acting ────────────────────────────────────────────────────

        [Header("Voice Acting")]
        [Tooltip("Piper TTS model for this character, e.g. en_US-kathleen-medium.")]
        public string piperModel;
        [Tooltip("espeak-ng voice variant fallback, e.g. en+f3.")]
        public string espeakVariant;

        // ─── Relationships ───────────────────────────────────────────────────

        [Header("Relationships")]
        public List<VillagerRelationship> relationships = new();

        // ─── UI ──────────────────────────────────────────────────────────────

        [Header("UI")]
        [Tooltip("Warm amber hex tint for this villager's dialogue portrait background.")]
        public string portraitTintHex = "#D4860A";
    }

    [System.Serializable]
    public class MemoryMapEntry
    {
        public string          memoryId;
        public string          memoryTitle;
        public MemoryOrbColor  color;
        [Range(0, 1)] public float clarity = 0.6f;
        [Range(0, 5)] public int   crackCount = 1;
        [Range(0, 1)] public float weight = 0.5f;
        [TextArea(1, 3)]
        public string          summary;
        public int             unlocksInMission = -1; // -1 = from day 1
        public bool            isSealedFragment;
    }

    [System.Serializable]
    public class VillagerRelationship
    {
        public string otherVillagerId;
        [Range(-100, 100)] public int relationshipScore;
        [TextArea(1, 2)]
        public string note;
    }
}
