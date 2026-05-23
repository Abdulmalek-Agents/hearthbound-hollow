// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Memory / VillagerMemoryMapSO
//
// The per-villager Memory Map — a node graph of all their memories with
// reveal order. In Mission 1, Doris's map has 4 nodes (3 visible, 1 locked).
// In Mission 2, Gerrold's map starts with 2 visible nodes.
// The schema supports the canonical 12-20 node bibliography per villager
// (Codex 02) without migration.

using System.Collections.Generic;
using UnityEngine;

namespace HearthboundHollow.Memory
{
    [CreateAssetMenu(menuName = "Hearthbound/Villager/Memory Map", fileName = "MemoryMap_")]
    public class VillagerMemoryMapSO : ScriptableObject
    {
        [Header("Owner")]
        public VillagerSO villager;

        [Header("Map nodes (authoring order = canonical reveal order)")]
        public List<MemoryMapNode> nodes = new();

        public int VisibleCount(VillagerMemoryRuntime runtime)
        {
            int count = 0;
            foreach (var n in nodes) if (runtime.IsRevealed(n.memory)) count++;
            return count;
        }
    }

    [System.Serializable]
    public struct MemoryMapNode
    {
        public MemoryNodeSO memory;
        public Vector2 graphPosition;        // for the Memory Map UI canvas
        public bool revealedAtStart;
        public string revealConditionId;     // gated by Yarn / runtime triggers
    }

    /// <summary>Tiny per-villager runtime cache; lives in VillageState's runtime list.</summary>
    [System.Serializable]
    public class VillagerMemoryRuntime
    {
        public string villagerId;
        public List<string> revealedMemoryIds = new();

        public bool IsRevealed(MemoryNodeSO m) => m != null && revealedMemoryIds.Contains(m.id);

        public void Reveal(MemoryNodeSO m)
        {
            if (m == null || string.IsNullOrEmpty(m.id)) return;
            if (!revealedMemoryIds.Contains(m.id)) revealedMemoryIds.Add(m.id);
        }
    }
}
