// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Core / GardenBoard  (Engagement Pillar P4 — Phase 65)
//
// Core-side, UI-readable snapshot of the garden (beds + the player's herb/tea
// inventory), plus the intent event the Garden UI publishes. The Mission-layer
// GardenService owns growth + brewing and writes these views; the UI-layer
// GardenUI reads them — no UI→Mission dependency (D-035), same bridge pattern as
// DayAgenda / EchoBoard / HollowShopBoard.

using System;
using System.Collections.Generic;

namespace HearthboundHollow.Core
{
    public sealed class GardenBedView
    {
        public string bedId = "";
        public string herbId = "";        // "" = empty
        public string herbName = "";
        public bool empty = true;
        public bool ripe = false;
        public int daysLeft = 0;          // days until ripe (0 = ripe)
        public string stageLabel = "fallow";
    }

    public sealed class GardenBoardData
    {
        public readonly List<GardenBedView> beds = new();
        /// <summary>herbId → count the player holds (harvested, un-brewed).</summary>
        public readonly Dictionary<string, int> herbs = new();
        /// <summary>herbId → count of brewed teas the player holds.</summary>
        public readonly Dictionary<string, int> teas = new();
        /// <summary>The herbs the player can plant right now.</summary>
        public readonly List<string> plantableHerbIds = new();
        /// <summary>A cozy line describing any active brewed-tea effect (or "").</summary>
        public string activeTeaLine = "";
    }

    public static class GardenBoard
    {
        public static readonly GardenBoardData Data = new();

        /// <summary>Raised when the service recomputes (UI refreshes if open).</summary>
        public static event Action OnChanged;
        public static void Raise() => OnChanged?.Invoke();
    }

    /// <summary>
    /// INTENT: a garden action chosen in the UI. Consumed by the Mission GardenService.
    /// Action: "plant" | "harvest" | "water" | "brew" | "sell".
    /// Arg: herbId (plant/brew/sell) or bedId (harvest/water).
    /// </summary>
    public readonly struct GardenActionRequestedEvent
    {
        public readonly string Action;
        public readonly string Arg;
        public GardenActionRequestedEvent(string action, string arg) { Action = action; Arg = arg; }
    }
}
