// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Memory / HollowUpgradeSO  (Engagement Pillar P3 — Phase 61.7)
//
// Data contract for a Hollow upgrade (Docs/Engagement_Bible/06) — the compounding
// spend-sink that makes coin meaningful and the shop feel owned and growing.
// Pure data; inert until a future HollowProgressionService consumes it. Upgrades
// are warmth, not power (Cozy Contract); never required to progress.

using System.Collections.Generic;
using UnityEngine;

namespace HearthboundHollow.Memory
{
    public enum UpgradeCategory { Shelf, Room, Tool, Decor, GardenBed }

    [CreateAssetMenu(menuName = "Hearthbound/Progression/Hollow Upgrade", fileName = "Upgrade_")]
    public class HollowUpgradeSO : ScriptableObject
    {
        public string upgradeId = "";
        public UpgradeCategory category = UpgradeCategory.Shelf;
        public string displayName = "";
        [TextArea] public string flavor = "";
        public int coinCost = 10;
        public List<string> requiresUpgradeIds = new();

        [Tooltip("Named GameObject a builder pre-places (hidden); revealed on purchase.")]
        public string sceneMarkerId = "";

        [Tooltip("Capacity delta read by the relevant system (e.g. +3 shelf slots, +1 garden bed).")]
        public int capacityDelta = 0;
    }
}
