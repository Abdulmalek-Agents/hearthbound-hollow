// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Memory / TariffSO
//
// Per-choice tariff: the economic + reputational + vow-integrity consequences
// of each MoralChoice path. Authored once per choice; consumed by RippleEngine
// (Phase 8).

using UnityEngine;

namespace HearthboundHollow.Memory
{
    [CreateAssetMenu(menuName = "Hearthbound/Tariff/Tariff", fileName = "Tariff_")]
    public class TariffSO : ScriptableObject
    {
        [Header("Identity")]
        public MoralChoice choice;
        public string displayLabel;
        [TextArea(2, 4)] public string costPreviewProse;

        [Header("Coin")]
        public int coinDelta = 0;             // negative = cost

        [Header("Trust deltas — applied to choice target villager")]
        [Range(-30, 30)] public int trustDeltaTarget = 0;

        [Header("Trust ripples — applied to nearby villagers")]
        [Range(-15, 15)] public int trustRippleNeighbors = 0;

        [Header("Memory integrity delta — applied to choice target memory")]
        [Range(-100, 30)] public int memoryIntegrityDelta = 0;

        [Header("Vow integrity deltas")]
        [Range(-30, 30)] public int vow1Delta = 0;     // Honor the named
        [Range(-30, 30)] public int vow3Delta = 0;     // Refuse no one's grief
        [Range(-30, 30)] public int vow7Delta = 0;     // Keep the Hollow lit

        [Header("Village grief average delta")]
        [Range(-15, 15)] public int villageGriefDelta = 0;

        [Header("UI presentation")]
        public Color choiceColor = Color.white;
        public Sprite choiceIcon;
    }
}
