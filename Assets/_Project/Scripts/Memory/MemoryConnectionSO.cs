// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Memory / MemoryConnectionSO
//
// An Echo Web link. Each connection is its own asset so we can author it once
// and reference it from both endpoints' Memory Map view if needed.
// In Mission 1-2 there is a single connection: DOR-001 ↔ GER-007's
// "Doris in the kitchen" node.

using UnityEngine;

namespace HearthboundHollow.Memory
{
    [CreateAssetMenu(menuName = "Hearthbound/Memory/Echo Connection", fileName = "Echo")]
    public class MemoryConnectionSO : ScriptableObject
    {
        [Header("Identity")]
        public string connectionId;          // "ECHO-DOR001-GER007"

        [Header("Endpoints")]
        public MemoryNodeSO memoryA;
        public MemoryNodeSO memoryB;

        [Header("Strength")]
        [Range(0f, 1f)] public float strength = 0.5f;

        [Header("Reveal condition")]
        [Tooltip("Yarn / runtime trigger that reveals this connection.")]
        public string revealConditionId;

        [Header("Reveal effect")]
        [TextArea(2, 4)] public string revealProse;

        [Header("Dialogue hook")]
        [Tooltip("Yarn node to jump to upon reveal, optional.")]
        public string yarnRevealNode;
    }
}
