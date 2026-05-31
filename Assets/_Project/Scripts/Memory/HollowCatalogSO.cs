// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Memory / HollowCatalogSO  (Engagement Pillar P3 — Phase 64)
//
// Authored container of every HollowUpgradeSO. HollowProgressionService loads an
// optional Resources/HollowCatalog asset; if absent/empty it falls back to a
// built-in starter catalog so the shop works with zero authored data.

using System.Collections.Generic;
using UnityEngine;

namespace HearthboundHollow.Memory
{
    [CreateAssetMenu(menuName = "Hearthbound/Progression/Hollow Catalog", fileName = "HollowCatalog")]
    public class HollowCatalogSO : ScriptableObject
    {
        public List<HollowUpgradeSO> upgrades = new();
    }
}
