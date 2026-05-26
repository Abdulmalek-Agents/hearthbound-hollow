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

    /// <summary>
    /// Published every time `DialogueUI.PresentLine` renders a new line —
    /// gives `MumbleVoicePlayer` (Audio asmdef) a chance to play the
    /// matching character's syllable bank without UI taking a dep on Audio.
    /// `Speaker` is the canonical character id ("doris", "gerrold", "pickle",
    /// "marin", …); UI normalises the speaker name to lowercase before publish.
    /// `EstimatedDurationSec` is the typewriter duration the UI is about to
    /// run — the mumble player uses it to scale syllable count.
    /// </summary>
    public readonly struct DialogueLineStartedEvent
    {
        public readonly string Speaker;
        public readonly string LineText;
        public readonly float EstimatedDurationSec;
        public DialogueLineStartedEvent(string speaker, string text, float dur)
        {
            Speaker = speaker;
            LineText = text;
            EstimatedDurationSec = dur;
        }
    }

    /// <summary>
    /// Published when a dialogue line finishes its typewriter reveal. Mumble
    /// VO uses this to cut off any leftover syllables.
    /// </summary>
    public readonly struct DialogueLineEndedEvent
    {
        public readonly string Speaker;
        public DialogueLineEndedEvent(string speaker) { Speaker = speaker; }
    }

    /// <summary>
    /// Published when a scene wants its background music + ambient bed to
    /// switch. `MusicPlayer` + `AmbientAudio` subscribe and crossfade. Phase
    /// 38's scene-builder injection raises this on `Start()` of each gameplay
    /// scene.
    /// </summary>
    public readonly struct SceneAudioRequestedEvent
    {
        public readonly string MusicId;
        public readonly string AmbienceId;
        public SceneAudioRequestedEvent(string music, string ambience) { MusicId = music; AmbienceId = ambience; }
    }
}
