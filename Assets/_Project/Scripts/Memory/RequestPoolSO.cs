// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Memory / RequestPoolSO  (Engagement Pillar P2 — Phase 61.7)
//
// Authored container of every RequestSO. RequestBoardService samples eligible
// entries each morning. If this asset is absent or empty, the service falls back
// to a built-in cozy roster, so the loop works with zero authored content.

using System.Collections.Generic;
using UnityEngine;

namespace HearthboundHollow.Memory
{
    [CreateAssetMenu(menuName = "Hearthbound/Requests/Request Pool", fileName = "RequestPool")]
    public class RequestPoolSO : ScriptableObject
    {
        public List<RequestSO> allRequests = new();
    }
}
