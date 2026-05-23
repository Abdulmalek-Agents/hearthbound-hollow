// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Memory / MemoryHerb
//
// Herbs grown in the Hollow garden. M1-2 ships only Lavender + Valerian.
// Schema supports the 18-herb canonical garden (Codex 04 § 4) without migration.

using UnityEngine;

namespace HearthboundHollow.Memory
{
    /// <summary>The runtime effect of a brewed tea on a villager's openness.</summary>
    public enum HerbEffect
    {
        Calm,            // softens grief, slows speech (Valerian)
        OpenUp,          // increases trust delta by +5 on next dialogue (Lavender)
        ForgetBriefly,   // shifts villager memory pointer to lighter memories (Sage — M3+)
        FocusMind,       // tightens Polish/Cleanse cursor tolerance (Mugwort — M3+)
    }

    [CreateAssetMenu(menuName = "Hearthbound/Garden/Herb", fileName = "Herb_")]
    public class MemoryHerb : ScriptableObject
    {
        [Header("Identity")]
        public string herbId;             // "lavender", "valerian"
        public string displayName;        // "Lavender"

        [Header("Effect")]
        public HerbEffect effect = HerbEffect.OpenUp;
        [Range(-20, 20)] public int trustDeltaOnTea = 5;
        public float effectDurationSeconds = 60f;

        [Header("Garden growth (M3+ — set at default for M1-2)")]
        [Range(1, 30)] public int growDays = 4;
        [Range(1, 5)] public int yieldPerHarvest = 1;
        public Sprite icon;
        public GameObject plantPrefab;     // Harvest Garden asset prefab

        [Header("Brew flavor text")]
        [TextArea(1, 3)] public string brewFlavorText;
    }
}
