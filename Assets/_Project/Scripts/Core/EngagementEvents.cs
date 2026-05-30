// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Core / EngagementEvents
//
// New EventBus message types introduced by the Engagement Bible loop
// (Docs/Engagement_Bible/, Phase 61+). Kept in a SEPARATE file from
// GameEvents.cs so the original event catalogue is never touched (clean diffs,
// identical asmdef — both live in HearthboundHollow.Core).
//
// These are pure readonly structs (EventBus requires `where T : struct`).
// They are inert until the loop services (DailyLoopService, RequestBoardService,
// HollowProgressionService, EchoWebService) publish/subscribe to them — see the
// per-system specs in Docs/Engagement_Bible/04..09.

namespace HearthboundHollow.Core
{
    /// <summary>Fired once at the start of every in-game day, after VillageState is ready.</summary>
    public readonly struct DayStartedEvent
    {
        public readonly int DayIndex;
        public DayStartedEvent(int day) { DayIndex = day; }
    }

    /// <summary>Fired when the morning Agenda has been assembled and is ready for the UI.</summary>
    public readonly struct AgendaReadyEvent
    {
        public readonly int DayIndex;
        public AgendaReadyEvent(int day) { DayIndex = day; }
    }

    /// <summary>Fired when a Request Board visit resolves (taken / deferred / refused).</summary>
    public readonly struct RequestResolvedEvent
    {
        public readonly string RequestId;
        public readonly string Outcome;   // "taken" | "deferred" | "refused"
        public RequestResolvedEvent(string requestId, string outcome) { RequestId = requestId; Outcome = outcome; }
    }

    /// <summary>Fired when the player purchases a Hollow upgrade (shelf / room / tool / decor / garden bed).</summary>
    public readonly struct HollowUpgradePurchasedEvent
    {
        public readonly string UpgradeId;
        public HollowUpgradePurchasedEvent(string upgradeId) { UpgradeId = upgradeId; }
    }

    /// <summary>Fired when an Echo thread reaches its completion threshold on the Memory Wall.</summary>
    public readonly struct EchoThreadCompletedEvent
    {
        public readonly string EchoId;
        public EchoThreadCompletedEvent(string echoId) { EchoId = echoId; }
    }

    /// <summary>Fired when the (new) Sort craft mini-game completes.</summary>
    public readonly struct MemorySortedEvent
    {
        public readonly UnityEngine.ScriptableObject Memory;
        public readonly bool AutoCompleted;
        public MemorySortedEvent(UnityEngine.ScriptableObject m, bool ac) { Memory = m; AutoCompleted = ac; }
    }
}
