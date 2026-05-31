// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / HollowUpgradeMarker
//
// PHASE 73 — the tiny data tag that makes "buying an upgrade visibly changes the
// Hollow" actually work.
//
// HollowProgressionService used to reveal an upgrade's scene object via
// GameObject.Find(sceneMarkerId) — but Unity's GameObject.Find CANNOT locate an
// INACTIVE object, so a marker hidden until purchase was unfindable and the
// reveal silently no-op'd (the audit's Engagement-Engine-3 gap).
//
// FIX: Phase 73's editor builder pre-places each upgrade's cozy prop INACTIVE in
// the Hollow scene, tagged with this component carrying the upgrade's markerId.
// The service finds these by *component* (FindObjectsByType + FindObjectsInactive
// .Include) — which DOES see inactive objects — and activates the matching one on
// purchase (and re-activates owned ones on every scene load). Data-only; no
// behaviour, no dependencies → safe in the Mission asmdef next to the service.

using UnityEngine;

namespace HearthboundHollow.Mission
{
    [DisallowMultipleComponent]
    public class HollowUpgradeMarker : MonoBehaviour
    {
        [Tooltip("Must equal the HollowCatalog upgrade's sceneMarkerId (e.g. " +
                 "'_HollowUpgrade_ShelfWindow'). The owning GameObject starts " +
                 "INACTIVE and is revealed by HollowProgressionService when the " +
                 "matching upgrade is purchased.")]
        public string markerId = "";
    }
}
