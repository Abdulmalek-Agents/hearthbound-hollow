// =============================================================================
// NarrativeLevelConfigSO.cs — Hearthbound Hollow
// Data-driven definition of a single mission ("level") out of the 30 total.
// Each mission has exactly one of these SOs under Resources/Missions/.
// Phase 76 — 30-Mission Architecture.
// =============================================================================
using System.Collections.Generic;
using UnityEngine;

namespace HearthboundHollow.Memory
{
    /// <summary>
    /// Defines everything a mission director needs to know to load and run a mission.
    /// All 30 SOs are created by Phase76_MissionArchitectureBuilder.
    /// </summary>
    [CreateAssetMenu(
        fileName = "Mission_XX_Config",
        menuName  = "Hearthbound/Mission/Level Config",
        order     = 1)]
    public class NarrativeLevelConfigSO : ScriptableObject
    {
        // ─── Identity ────────────────────────────────────────────────────────

        [Header("Identity")]
        [Tooltip("0-based mission index (0 = M01, 29 = M30).")]
        public int missionIndex;

        [Tooltip("Short internal id, e.g. 'M01_OpeningTheHollow'. Stable — never change.")]
        public string missionId;

        [Tooltip("Player-facing title shown on the day card.")]
        public string missionTitle;

        [Tooltip("One-sentence subtitle shown on the mission card.")]
        public string missionSubtitle;

        [Header("Act & Season")]
        public int act;           // 1, 2, or 3
        public Season season;

        // ─── Villager ────────────────────────────────────────────────────────

        [Header("Primary Villager")]
        [Tooltip("The main villager SO for this mission.")]
        public VillagerProfileSO primaryVillager;

        [Tooltip("Optional secondary villagers present in this mission.")]
        public List<VillagerProfileSO> secondaryVillagers = new();

        // ─── Memory Orb ──────────────────────────────────────────────────────

        [Header("Memory Orb")]
        public string primaryOrbId;
        public MemoryOrbColor orbColor;
        public float          orbClarity01        = 0.6f;
        public int            orbCrackCount       = 2;
        public float          orbWeight01         = 0.5f;

        [Tooltip("True if this mission introduces a sealed fragment.")]
        public bool           containsSealedFragment = false;
        public int            sealedFragmentIndex    = -1; // 0-8

        // ─── Moral Choice ────────────────────────────────────────────────────

        [Header("Moral Choice")]
        [Tooltip("Which of the 4 moral paths are available in this mission.")]
        public List<MoralChoiceType> availableChoices = new()
            { MoralChoiceType.Erase, MoralChoiceType.Cleanse,
              MoralChoiceType.Listen, MoralChoiceType.Defer };

        [Tooltip("Prose consequences for each choice path (for Evening Ledger).")]
        public List<MoralChoiceConsequence> choiceConsequences = new();

        // ─── Memory Dream ────────────────────────────────────────────────────

        [Header("Memory Dream")]
        [Tooltip("Yarn node name for the dream that plays after this mission.")]
        public string dreamYarnNode;
        public float  dreamDurationSeconds = 45f;

        // ─── Yarn ────────────────────────────────────────────────────────────

        [Header("Yarn Dialogue")]
        [Tooltip("Main Yarn node to start when the villager arrives.")]
        public string yarnStartNode;
        public string yarnFile;  // e.g. "Mission_01_Doris"

        // ─── Scene ───────────────────────────────────────────────────────────

        [Header("Scene")]
        [Tooltip("Scene path for this mission's primary scene.")]
        public string scenePath;

        [Tooltip("True if this mission requires the Garden scene.")]
        public bool requiresGarden = false;

        // ─── Almanac ─────────────────────────────────────────────────────────

        [Header("Almanac")]
        public bool          isAlmanacEvent = false;
        public AlmanacEventType almanacEventType;
        public string        almanacHeadline;

        // ─── Echo Web ────────────────────────────────────────────────────────

        [Header("Echo Web")]
        [Tooltip("Echo connections that become discoverable after this mission completes.")]
        public List<EchoConnectionDef> newEchoConnections = new();

        // ─── Unlock / Requirements ───────────────────────────────────────────

        [Header("Requirements")]
        [Tooltip("Predecessor trail warmth required to unlock this mission's Reading Nook tier.")]
        public int requiredWarmth = 0;
        [Tooltip("Minimum coin balance for this mission's optional path.")]
        public int optionalCoinGate = 0;

        // ─── Completion rewards ──────────────────────────────────────────────

        [Header("Rewards")]
        public int  coinRewardBase    = 8;
        public int  trustGrant        = 5;
        public int  warmthGrant       = 0; // if > 0, predecessor trail advances
    }

    // ─── Sub-types ────────────────────────────────────────────────────────────

    [System.Serializable]
    public class MoralChoiceConsequence
    {
        public MoralChoiceType choice;
        [TextArea(2, 5)]
        public string eveningLedgerProse;
        public int    trustDelta;
        public int    coinDelta;
        public int    warmthDelta;
        public string villageStateFlagToSet; // e.g. "gerroldMoralChoiceId = cleanse"
    }

    [System.Serializable]
    public class EchoConnectionDef
    {
        public string orbIdA;
        public string orbIdB;
        public string sharedFacet;
    }

    public enum MemoryOrbColor
    {
        Joy, Grief, Shame, Awe, Longing, Dread, Guilt, Pride,
        Regret, Nostalgia, Fear, Wonder, Tenderness, Bitterness
    }
}
