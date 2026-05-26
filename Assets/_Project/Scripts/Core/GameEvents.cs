// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Core / GameEvents
//
// Event payload structs published via EventBus. Each is a small `readonly
// struct` so Publish<T>(evt) allocates zero. Only the **payload shape** lives
// here. Subsystems define their own enums (e.g. MoralChoice, CleanseOutcome)
// in their own asmdefs, so this file keeps cross-module coupling minimal.

using UnityEngine;

namespace HearthboundHollow.Core
{
    public readonly struct MissionStartedEvent
    {
        public readonly ScriptableObject Mission;
        public readonly string MissionId;
        public MissionStartedEvent(ScriptableObject mission, string id) { Mission = mission; MissionId = id; }
    }

    public readonly struct MissionCompletedEvent
    {
        public readonly ScriptableObject Mission;
        public readonly string MissionId;
        public readonly string OutcomeSummary;
        public MissionCompletedEvent(ScriptableObject mission, string id, string outcome) { Mission = mission; MissionId = id; OutcomeSummary = outcome; }
    }

    public readonly struct DialogueStartedEvent
    {
        public readonly ScriptableObject Villager;
        public readonly string DialogueNodeId;
        public DialogueStartedEvent(ScriptableObject v, string nodeId) { Villager = v; DialogueNodeId = nodeId; }
    }

    public readonly struct DialogueEndedEvent
    {
        public readonly ScriptableObject Villager;
        public DialogueEndedEvent(ScriptableObject v) { Villager = v; }
    }

    public readonly struct MemoryPolishedEvent
    {
        public readonly ScriptableObject Memory;
        public readonly float Clarity01;
        public readonly bool AutoCompleted;
        public MemoryPolishedEvent(ScriptableObject m, float c, bool ac) { Memory = m; Clarity01 = c; AutoCompleted = ac; }
    }

    public readonly struct MemoryCleansedEvent
    {
        public readonly ScriptableObject Memory;
        public readonly int OutcomeRaw;
        public readonly bool AutoCompleted;
        public MemoryCleansedEvent(ScriptableObject m, int outcomeRaw, bool ac) { Memory = m; OutcomeRaw = outcomeRaw; AutoCompleted = ac; }
    }

    public readonly struct MoralChoiceMadeEvent
    {
        public readonly ScriptableObject Memory;
        public readonly int ChoiceRaw;
        public MoralChoiceMadeEvent(ScriptableObject m, int choiceRaw) { Memory = m; ChoiceRaw = choiceRaw; }
    }

    public readonly struct HerbHarvestedEvent
    {
        public readonly ScriptableObject Herb;
        public HerbHarvestedEvent(ScriptableObject h) { Herb = h; }
    }

    public readonly struct TeaBrewedEvent
    {
        public readonly ScriptableObject Herb;
        public TeaBrewedEvent(ScriptableObject h) { Herb = h; }
    }

    public readonly struct DayEndedEvent
    {
        public readonly int DayIndex;
        public DayEndedEvent(int day) { DayIndex = day; }
    }

    public readonly struct EchoConnectionRevealedEvent
    {
        public readonly ScriptableObject MemoryA;
        public readonly ScriptableObject MemoryB;
        public EchoConnectionRevealedEvent(ScriptableObject a, ScriptableObject b) { MemoryA = a; MemoryB = b; }
    }

    public readonly struct VillageStateLoadedEvent
    {
        public readonly ScriptableObject VillageState;
        public VillageStateLoadedEvent(ScriptableObject vs) { VillageState = vs; }
    }

    public readonly struct VillageStateSavedEvent
    {
        public readonly int SlotIndex;
        public readonly bool IsAutosave;
        public VillageStateSavedEvent(int slot, bool autosave) { SlotIndex = slot; IsAutosave = autosave; }
    }
}
