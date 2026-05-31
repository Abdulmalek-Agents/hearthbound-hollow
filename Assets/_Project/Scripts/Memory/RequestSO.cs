// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Memory / RequestSO  (Engagement Pillar P2 — Phase 61.7)
//
// Data contract for a villager request on the Request Board
// (Docs/Engagement_Bible/05). Pure data; inert until RequestBoardService and a
// future VisitDirector consume it. Lives in the Memory asmdef (references the
// existing VillagerSO + MemoryNodeSO).

using System.Collections.Generic;
using UnityEngine;

namespace HearthboundHollow.Memory
{
    public enum RequestKind { TakeMemory, ReturnMemory, RestoreMemory, FindMemory, JustVisit }

    [CreateAssetMenu(menuName = "Hearthbound/Requests/Request", fileName = "Request_")]
    public class RequestSO : ScriptableObject
    {
        [Header("Identity")]
        public string requestId = "";
        public VillagerSO villager;
        public MemoryNodeSO memory;
        public RequestKind kind = RequestKind.TakeMemory;

        [Header("Board presentation (fiction-voice, no numbers)")]
        public string boardTeaser = "";
        [TextArea] public string openingLine = "";

        [Header("Gating (all optional — empty = always eligible)")]
        public int minDayIndex = 0;
        public List<string> requiresFlags = new();
        public List<string> requiresEchoIds = new();
        public List<string> blockedByFlags = new();

        [Header("Weighting")]
        [Tooltip("Higher = more likely to surface on a given morning when eligible.")]
        public float weight = 1f;
        [Tooltip("Hand-sealed arc beats: always appear when eligible (pinned).")]
        public bool pinnedArcBeat = false;

        /// <summary>Display name for the board/agenda, with a graceful fallback.</summary>
        public string DisplayName =>
            villager != null && !string.IsNullOrEmpty(villager.displayName) ? villager.displayName : "Someone";
    }
}
