// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / KeepAliveOnLoad
//
// PHASE 51 helper — marks a Bootstrap-scene GameObject as
// DontDestroyOnLoad on Awake. Used by the Memory Web overlay so it's
// available in every gameplay scene without needing to be re-built
// per-scene.

using UnityEngine;

namespace HearthboundHollow.Mission
{
    /// <summary>
    /// Attach to any GameObject that should survive scene loads.
    /// Idempotent: a duplicate Awake is harmless because Unity collapses
    /// duplicate DontDestroyOnLoad calls on the same object.
    /// </summary>
    public class KeepAliveOnLoad : MonoBehaviour
    {
        private void Awake()
        {
            Object.DontDestroyOnLoad(gameObject);
        }
    }
}
