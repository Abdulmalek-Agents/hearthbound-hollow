// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Memory / EchoSO  (Engagement Pillar P6 — Phase 61.7)
//
// Data contract for an Echo thread on the Memory Wall (Docs/Engagement_Bible/09).
// When the player keeps `threshold` member memories, the thread completes and
// grants its rewards (a Dream / decor / a new Request arc / a mystery step).
// Pure data; inert until a future EchoWebService consumes it.

using System.Collections.Generic;
using UnityEngine;

namespace HearthboundHollow.Memory
{
    public enum EchoType { Person, Place, Object, Year, Pattern }

    [CreateAssetMenu(menuName = "Hearthbound/Memory/Echo", fileName = "Echo_")]
    public class EchoSO : ScriptableObject
    {
        public string echoId = "";
        public string displayName = "";
        public EchoType type = EchoType.Person;

        [Tooltip("Thread completes when this many member memories are kept.")]
        public int threshold = 3;

        public List<MemoryNodeSO> members = new();

        [Tooltip("Reward ids granted on completion: dreamId / decorId / requestArcId / mysteryStep.")]
        public List<string> rewardsOnComplete = new();
    }
}
