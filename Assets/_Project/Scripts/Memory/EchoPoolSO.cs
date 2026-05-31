// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Memory / EchoPoolSO  (Engagement Pillar P6 — Phase 63)
//
// Authored container of every EchoSO thread. EchoWebService loads an optional
// Resources/EchoPool asset and tallies each thread's progress against the
// player's kept memories. If absent/empty, the service falls back to a built-in
// cozy set of threads so the Memory Wall meta-game works with zero authored data.

using System.Collections.Generic;
using UnityEngine;

namespace HearthboundHollow.Memory
{
    [CreateAssetMenu(menuName = "Hearthbound/Memory/Echo Pool", fileName = "EchoPool")]
    public class EchoPoolSO : ScriptableObject
    {
        public List<EchoSO> allEchoes = new();
    }
}
