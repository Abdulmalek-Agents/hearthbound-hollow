// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Core / HollowShopBoard  (Engagement Pillar P3 — Phase 64)
//
// Core-side, UI-readable snapshot of the Hollow upgrade catalog + the player's
// purchase/affordability state. The Mission-layer HollowProgressionService owns
// the catalog + purchase logic and writes these views; the UI-layer HollowShopUI
// reads them and renders the "ledger of improvements" — no UI→Mission dep (D-035),
// same bridge pattern as DayAgenda / EchoBoard.
//
// Cozy guardrail (D-076): show abundance + growth. Unaffordable items are GREY
// with a gentle "soon" hint — never red, never a deficit nag.

using System;
using System.Collections.Generic;

namespace HearthboundHollow.Core
{
    public sealed class HollowUpgradeView
    {
        public string upgradeId = "";
        public string displayName = "";
        public string flavor = "";
        public string category = "Shelf";   // Shelf / Room / Tool / Decor / GardenBed
        public int coinCost = 10;
        public bool purchased = false;
        public bool affordable = false;
        public bool locked = false;          // prerequisite not yet met
        public string lockHint = "";         // gentle "soon" copy
    }

    public static class HollowShopBoard
    {
        public static readonly List<HollowUpgradeView> Catalog = new();

        /// <summary>Raised whenever the service rebuilds the views (UI refreshes if open).</summary>
        public static event Action OnChanged;

        public static void Raise() => OnChanged?.Invoke();
    }
}
