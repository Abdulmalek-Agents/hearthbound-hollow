// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Player / SimpleFollowCamera
//
// A small third-person follow camera that doesn't require Cinemachine.
//
// ── PHASE 26 STATUS ─────────────────────────────────────────────
// **Superseded** by `SmoothFollowCamera.cs` (same folder).
//
// The new camera offers the same basic damped-follow API plus:
//   • Spring-damped position (SmoothDamp) — visibly smoother at sprint speeds
//   • Mouse-orbit (RMB hold) + scroll-zoom
//   • Sphere-cast wall-clip protection
//
// SimpleFollowCamera is **kept** so:
//   1. Existing scene assets that have it serialised continue to work.
//   2. The Phase 17 / 22 / 24 builders that attach `SimpleFollowCamera`
//      still resolve the symbol — Phase 26's builder then upgrades the
//      component to `SmoothFollowCamera` in-place.
//
// Do NOT add new fields here; use the SmoothFollowCamera for any new
// behaviour. This class is intentionally minimal.
//
// !! IMPORTANT (D-032) !! This was previously a nested class inside
// HearthboundOneClickSetup (Editor asmdef, includePlatforms = ["Editor"]).
// That broke the runtime — Unity couldn't resolve the type when scenes
// loaded in Play mode, so the camera stayed frozen at the origin and the
// game looked "not playable".
//
// Moving it here (HearthboundHollow.Player, runtime asmdef) fixes the bug
// permanently. The Editor builders (HearthboundOneClickSetup, Phase 22,
// Phase 24) still attach this same class to the scene camera — they just
// resolve it via the `HearthboundHollow.Player` namespace now.

using UnityEngine;

namespace HearthboundHollow.Player
{
    [DisallowMultipleComponent]
    public class SimpleFollowCamera : MonoBehaviour
    {
        public Transform target;
        public float height = 4.5f;
        public float behind = 5.5f;
        public float lookAheadY = 1.2f;
        public float damping = 6f;

        private void LateUpdate()
        {
            if (target == null) return;
            var desired = target.position + Vector3.up * height + (-target.forward) * behind;
            transform.position = Vector3.Lerp(transform.position, desired, Time.deltaTime * damping);
            transform.LookAt(target.position + Vector3.up * lookAheadY);
        }
    }
}
