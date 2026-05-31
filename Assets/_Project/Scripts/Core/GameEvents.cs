// =============================================================================
// GameEvents.cs — Hearthbound Hollow
// Central event catalogue. All EventBus events live here.
// Phase 76 — 30-Mission Architecture.
// =============================================================================
using System.Collections.Generic;
using UnityEngine;

namespace HearthboundHollow.Core
{
    // ─── Mission lifecycle ────────────────────────────────────────────────────
    public struct MissionStartedEvent    { public int MissionIndex; }
    public struct MissionCompletedEvent  { public int MissionIndex; public string OutcomeId; }
    public struct ActTransitionEvent     { public int NewAct; } // 1, 2, or 3

    // ─── Memory operations ───────────────────────────────────────────────────
    public struct MemoryPolishedEvent    { public string OrbId; public float Quality01; }
    public struct MemoryCleansedEvent    { public string OrbId; public CleanseOutcome Outcome; }
    public struct MemoryWeavedEvent      { public string OrbIdA; public string OrbIdB; public string ResultId; }
    public struct MemorySeveredEvent     { public string OrbId; }
    public struct MemoryAcquiredEvent    { public string OrbId; }
    public struct MemoryReleasedEvent    { public string OrbId; }  // drifted, sold, erased

    // ─── Moral choices ───────────────────────────────────────────────────────
    public struct MoralChoiceMadeEvent
    {
        public int MissionIndex;
        public MoralChoiceType Choice;
        public string MemoryOrbId;
    }

    // ─── Dialogue ────────────────────────────────────────────────────────────
    public struct DialogueStartedEvent  { public string Speaker; }
    public struct DialogueEndedEvent    { public string Speaker; }
    public struct DialogueLineStartedEvent { public string Speaker; public string LineId; public bool HasVoiceClip; }
    public struct DialogueLineEndedEvent   { public string LineId; }

    // ─── Voice audio ─────────────────────────────────────────────────────────
    public struct VoiceClipStartedEvent { public string LineId; public float Duration; }
    public struct VoiceClipEndedEvent   { public string LineId; }

    // ─── Garden & Tea ────────────────────────────────────────────────────────
    public struct HerbPlantedEvent   { public string HerbId; public int BedIndex; }
    public struct HerbRipenedEvent   { public string HerbId; public int BedIndex; }
    public struct HerbHarvestedEvent { public string HerbId; }
    public struct TeaBrewedEvent     { public string HerbId; public string TeaId; }
    public struct TeaOfferedEvent    { public string TeaId; public string RecipientId; }

    // ─── Economy ─────────────────────────────────────────────────────────────
    public struct CoinEarnedEvent   { public int Amount; public string Source; }
    public struct CoinSpentEvent    { public int Amount; public string On; }
    public struct UpgradePurchasedEvent { public string UpgradeId; }

    // ─── Echo Web / predecessor trail ────────────────────────────────────────
    public struct EchoConnectionRevealedEvent { public string OrbIdA; public string OrbIdB; public string SharedFacet; }
    public struct EchoThreadCompletedEvent    { public string ThreadId; public int CoinReward; }
    public struct SealedFragmentFoundEvent    { public int FragmentIndex; public string HolderId; }
    public struct SealedMemoryAssembledEvent  { } // all 9 fragments collected

    // ─── Day cycle ───────────────────────────────────────────────────────────
    public struct DayStartedEvent  { public int DayIndex; public int MissionIndex; }
    public struct DayEndedEvent    { public int DayIndex; }
    public struct SeasonChangedEvent { public Season NewSeason; }

    // ─── Request Board ───────────────────────────────────────────────────────
    public struct RequestKeptEvent     { public string VillagerId; public string OrbId; }
    public struct RequestListenedEvent { public string VillagerId; }
    public struct RequestDeferredEvent { public string VillagerId; }

    // ─── Hollow / shop ───────────────────────────────────────────────────────
    public struct HollowUpgradedEvent     { public string UpgradeId; }
    public struct ReadingNookVisitedEvent  { }
    public struct ReadingNookFragmentReadEvent { public string FragmentId; public int WarmthGranted; }

    // ─── Accessibility ───────────────────────────────────────────────────────
    public struct GentleModeChangedEvent { public bool Enabled; }
    public struct LanguageChangedEvent   { public string LanguageCode; } // "en" or "ar"

    // ─── Almanac ─────────────────────────────────────────────────────────────
    public struct AlmanacEventTriggeredEvent { public string EventId; public AlmanacEventType EventType; }

    // ─── Cinematics / Cutscene ───────────────────────────────────────────────
    public struct CutsceneStartedEvent  { public string CutsceneId; }
    public struct CutsceneEndedEvent    { public string CutsceneId; }

    // ─── Player movement lock (cross-asmdef via Core interface) ──────────────
    public struct PlayerMovementLockRequestEvent { public bool Locked; }

    // ─── Sealed memory climax ────────────────────────────────────────────────
    public struct SealedMemoryChoiceMadeEvent
    {
        public SealedMemoryChoice Choice;
        // Return | Keep | Sell | Drift
    }

    // ─── Enums ───────────────────────────────────────────────────────────────
    public enum MoralChoiceType  { Erase, Cleanse, Listen, Defer }
    public enum CleanseOutcome   { Perfect, Acceptable, CrossedCore, Incomplete }
    public enum Season           { Autumn, Winter, Spring, Summer }
    public enum AlmanacEventType { MarketDay, Festival, VisitingBard, Birthday, SealedEcho }
    public enum SealedMemoryChoice { ReturnToVillage, KeepInHollow, SellToCollector, LetDrift }
}
