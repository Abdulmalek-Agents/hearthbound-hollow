// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Core / CraftEvents  (Engagement Pillar P5 — Phase 66)
//
// EventBus messages + shared verb helpers for the Living Workbench
// (Docs/Engagement_Bible/08). Kept in their own Core file (no churn to
// GameEvents/EngagementEvents). CraftVerbs lives here so BOTH the UI screen and
// the Mission service share one deterministic verb assignment (no UI→Mission dep).

namespace HearthboundHollow.Core
{
    /// <summary>
    /// INTENT: the player chose to tend a kept memory at the workbench. Published by
    /// the UI (WorkbenchUI); consumed by the Mission WorkbenchService, which applies
    /// the gentle reward + mastery and publishes MemoryTendedEvent. (UI never refs
    /// Mission — D-035.)
    /// </summary>
    public readonly struct CraftRequestedEvent
    {
        public readonly string MemoryId;
        public readonly string Verb;   // "polish" | "cleanse" | "sort" | "steep"
        public CraftRequestedEvent(string memoryId, string verb) { MemoryId = memoryId; Verb = verb; }
    }

    /// <summary>
    /// Fired when a memory has been tended (crafted) at the bench. The bool is the
    /// celebratory "Perfect" outcome (cosmetic shimmer); Acceptable is never punished.
    /// Consumed by juice/FX, Pickle reactions, and the Ledger.
    /// </summary>
    public readonly struct MemoryTendedEvent
    {
        public readonly string MemoryId;
        public readonly string Verb;
        public readonly bool Perfect;
        public MemoryTendedEvent(string memoryId, string verb, bool perfect)
        { MemoryId = memoryId; Verb = verb; Perfect = perfect; }
    }

    /// <summary>Shared, dependency-free craft-verb helpers (used by UI + Mission).</summary>
    public static class CraftVerbs
    {
        public static readonly string[] Verbs = { "polish", "cleanse", "sort", "steep" };

        /// <summary>Deterministic verb for a memory id — the same memory always asks the same craft.</summary>
        public static string VerbFor(string memoryId)
        {
            if (string.IsNullOrEmpty(memoryId)) return "polish";
            int h = 0; foreach (char c in memoryId) h = unchecked(h * 31 + c);
            return Verbs[((h % Verbs.Length) + Verbs.Length) % Verbs.Length];
        }

        public static string Label(string verb) => verb switch
        {
            "polish"  => "Polish to a clear shine",
            "cleanse" => "Cleanse a hairline crack",
            "sort"    => "Sort its tangled facets",
            "steep"   => "Steep it in a soft light",
            _          => "Tend it",
        };

        public static string Flavor(string verb) => verb switch
        {
            "polish"  => "Slow circles, and the day inside warms.",
            "cleanse" => "Trace the crack, gentle around the core.",
            "sort"    => "The pieces fall into their right order.",
            "steep"   => "A breath of tea-light, and it settles.",
            _          => "A little care, and it holds.",
        };

        /// <summary>Warm flavor for the hidden Keeper's-Hand mastery — never a number in emotional UI.</summary>
        public static string MasteryFlavor(int count)
        {
            if (count <= 0)  return "Your hands are still learning the work.";
            if (count < 4)   return "You're finding the rhythm of it.";
            if (count < 10)  return "Your hands know this work now.";
            if (count < 20)  return "The orbs settle the moment you touch them.";
            return "Marin would have nodded at this.";
        }
    }
}
