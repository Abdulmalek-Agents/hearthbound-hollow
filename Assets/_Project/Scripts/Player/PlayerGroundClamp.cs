// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Player / PlayerGroundClamp
//
// FIX for the "half body sunk in the floor" issue reported on first playtest
// of the Phase 26 player.
//
// ROOT CAUSE
// ──────────
// The Phase 13 BoZo wrapper nests the BoZo character prefab as a child "Body"
// of the Player root with `localPosition = Vector3.zero`. That's only correct
// if BoZo's mesh origin sits exactly at the feet (Y=0 of its local space).
// In practice BoZo's mesh origin is offset upward (hip/pelvis level for the
// BSMC character base), so when the CharacterController settles on the floor
// the visible mesh ends up clipped into the ground:
//
//     CC capsule (collider)         BoZo mesh (visual)
//     ┌─────────────┐               ┌─────────────┐
//     │   head      │               │   head      │   ← visible
//     │   ...       │               │   ...       │   ← visible
//     │   waist     │  ◀── floor ──▶│  waist Y=0  │   ← waterline (floor)
//     │   legs      │   below this  │   legs      │   ← INVISIBLE (in floor)
//     │   feet  Y=0 │  the CC isn't │   feet -Y   │   ← INVISIBLE (in floor)
//     └─────────────┘               └─────────────┘
//
// Pressing WASD made it temporarily pop up because the first
// `CharacterController.Move()` triggers Unity's snap-to-collider sweep, which
// shifted the GameObject for one frame — but as soon as input released, the
// CC slid back into the same half-sunk pose because the *root cause* (Body
// localPosition mismatch) was unchanged.
//
// WHAT THIS COMPONENT DOES
// ─────────────────────────
// On Start (and on demand via the context menu / public Align() call):
//
//   1. Finds the Body child (or the first non-Player child in the hierarchy).
//   2. Gathers every Renderer under it (SkinnedMeshRenderer included).
//   3. Computes the lowest Y of the combined renderer bounds — i.e. where
//      the visible "feet" of the mesh actually are in world space.
//   4. Computes where the CharacterController capsule bottom is in world
//      space — `transform.position.y + cc.center.y - cc.height/2 + cc.skinWidth`.
//   5. Shifts `body.localPosition.y` by the difference so the mesh bottom and
//      the CC bottom coincide.
//
// After this runs the mesh always stands exactly on the collider's footprint,
// so when gravity settles the CC onto the floor, the mesh feet land on the
// floor too — no clipping, no pop, no rubber-banding.
//
// USAGE
// ─────
//   • Attach this component to the Player root (the same GameObject that has
//     PlayerController + CharacterController).
//   • Leave `body` empty — it auto-locates the first child named "Body" (the
//     Phase 13 BoZo wrapper layout) or the first child if none match.
//   • The Phase 26 capstone now adds this component automatically.
//
// EDIT-TIME TUNING
// ────────────────
// Right-click the component in the Inspector → "Align Body to Ground" to
// re-snap manually. Use the `bias` field to push the mesh up or down a few
// cm if you want the character to plant slightly into the ground (typical
// for cozy-game grass).

using UnityEngine;
using HearthboundHollow.Core;

namespace HearthboundHollow.Player
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-40)] // run after PlayerController.Awake (-50) so the CC is initialised
    public class PlayerGroundClamp : MonoBehaviour
    {
        [Header("Targets")]
        [Tooltip("The child GameObject holding the character mesh. Auto-found " +
                 "on Awake by looking for a child named 'Body' (matches the " +
                 "Phase 13 BoZo wrapper layout).")]
        public Transform body;

        [Header("Behaviour")]
        [Tooltip("Run Align() automatically in Start(). Recommended.")]
        public bool alignOnStart = true;

        [Tooltip("Re-snap one extra time in the first LateUpdate after Start so " +
                 "the Animator has had a chance to apply its bind-pose / idle " +
                 "transform offsets. Recommended.")]
        public bool resnapInFirstLateUpdate = true;

        [Tooltip("Use the Renderer's pose-independent localBounds (transformed " +
                 "by its local-to-world matrix) instead of the current world " +
                 "bounds. localBounds is deterministic — it doesn't shift with " +
                 "animation. Recommended unless you have a non-skinned mesh.")]
        public bool useLocalBounds = true;

        [Tooltip("Manual fudge — positive lifts the mesh, negative pushes it " +
                 "further into the floor. Useful if you want the cozy character " +
                 "to plant a couple of cm into the grass.")]
        [Range(-0.2f, 0.2f)]
        public float bias = 0f;

        [Header("Debug")]
        [Tooltip("Print the computed offset to the Console when Align() runs.")]
        public bool verbose = true;

        private CharacterController _cc;
        private bool _didLateResnap;

        // ───── Lifecycle ───────────────────────────────────────────

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            if (body == null) body = ResolveBody();
        }

        private void Start()
        {
            if (alignOnStart) Align();
        }

        private void LateUpdate()
        {
            if (resnapInFirstLateUpdate && !_didLateResnap)
            {
                Align();
                _didLateResnap = true;
            }
        }

        // ───── Manual entry-point ──────────────────────────────────

        [ContextMenu("Align Body to Ground")]
        public void Align()
        {
            if (body == null) body = ResolveBody();
            if (body == null)
            {
                Hh.Warn(LogCategory.State, $"PlayerGroundClamp on '{name}' has no Body child to align.");
                return;
            }

            var renderers = body.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                Hh.Warn(LogCategory.State, $"PlayerGroundClamp on '{name}' found no renderers under '{body.name}'.");
                return;
            }

            float meshBottomWorldY = ComputeBottomWorldY(renderers);
            float ccBottomWorldY   = ComputeCcBottomWorldY();

            float diff = ccBottomWorldY - meshBottomWorldY + bias;

            // Apply as a local-Y shift to the Body so the visible mesh bottom
            // ends up at the CC bottom.
            var lp = body.localPosition;
            body.localPosition = new Vector3(lp.x, lp.y + diff, lp.z);

            if (verbose)
                Hh.Log(LogCategory.State,
                    $"PlayerGroundClamp: aligned '{body.name}' by Δy={diff:F3} " +
                    $"(mesh bottom Y={meshBottomWorldY:F3}, CC bottom Y={ccBottomWorldY:F3}, bias={bias:F3}).");
        }

        // ───── Computations ────────────────────────────────────────

        private float ComputeBottomWorldY(Renderer[] renderers)
        {
            float minY = float.PositiveInfinity;

            if (useLocalBounds)
            {
                // Walk the 8 corners of each renderer's local-space bounds and
                // transform them into world space. This is pose-independent —
                // a SkinnedMeshRenderer's localBounds reflects the bind pose,
                // so the answer doesn't change between bind and idle.
                foreach (var r in renderers)
                {
                    if (r == null) continue;
                    Bounds lb;
                    if (r is SkinnedMeshRenderer smr) lb = smr.localBounds;
                    else if (r is MeshRenderer mr && r.TryGetComponent<MeshFilter>(out var mf) && mf.sharedMesh != null)
                        lb = mf.sharedMesh.bounds;
                    else
                        lb = r.bounds; // fallback to world bounds — less ideal

                    Vector3 c = lb.center, e = lb.extents;
                    for (int sx = -1; sx <= 1; sx += 2)
                        for (int sy = -1; sy <= 1; sy += 2)
                            for (int sz = -1; sz <= 1; sz += 2)
                            {
                                var w = r.transform.TransformPoint(
                                    new Vector3(c.x + sx * e.x, c.y + sy * e.y, c.z + sz * e.z));
                                if (w.y < minY) minY = w.y;
                            }
                }
            }
            else
            {
                foreach (var r in renderers)
                    if (r != null && r.bounds.min.y < minY) minY = r.bounds.min.y;
            }

            // Sanity: if we somehow didn't find anything, fall back to the
            // Body's own world Y so we don't introduce NaN-induced jumps.
            if (float.IsPositiveInfinity(minY)) minY = body.position.y;
            return minY;
        }

        private float ComputeCcBottomWorldY()
        {
            if (_cc == null) _cc = GetComponent<CharacterController>();
            if (_cc != null)
            {
                // Capsule bottom = centre.y - height/2, plus a sliver for skinWidth
                // (Unity treats the skin as the floor-contact surface).
                return transform.position.y
                     + _cc.center.y
                     - _cc.height * 0.5f
                     + _cc.skinWidth;
            }
            return transform.position.y;
        }

        // ───── Body auto-discovery ────────────────────────────────

        private Transform ResolveBody()
        {
            // Phase 13 BoZo wrapper convention: child named "Body".
            var t = transform.Find("Body");
            if (t != null) return t;

            // Fallback — first child that owns a renderer.
            for (int i = 0; i < transform.childCount; i++)
            {
                var c = transform.GetChild(i);
                if (c.GetComponentInChildren<Renderer>(true) != null) return c;
            }
            return null;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Show the CC bottom + the mesh bottom as two horizontal disks so
            // the offset is visible in the Scene view at edit time.
            var cc = GetComponent<CharacterController>();
            if (cc != null)
            {
                float ccBottomY = transform.position.y + cc.center.y - cc.height * 0.5f + cc.skinWidth;
                Gizmos.color = new Color(0.4f, 1f, 0.4f, 0.85f);  // green = CC bottom (the target)
                DrawDisk(new Vector3(transform.position.x, ccBottomY, transform.position.z), cc.radius + 0.05f);
            }

            if (body != null)
            {
                var rs = body.GetComponentsInChildren<Renderer>();
                if (rs.Length > 0)
                {
                    float meshY = ComputeBottomWorldY(rs);
                    Gizmos.color = new Color(1f, 0.5f, 0.3f, 0.85f);  // orange = current mesh bottom
                    DrawDisk(new Vector3(transform.position.x, meshY, transform.position.z), 0.4f);
                }
            }
        }

        private static void DrawDisk(Vector3 centre, float radius)
        {
            const int segs = 32;
            Vector3 prev = centre + new Vector3(radius, 0, 0);
            for (int i = 1; i <= segs; i++)
            {
                float a = (i / (float)segs) * Mathf.PI * 2f;
                var next = centre + new Vector3(Mathf.Cos(a) * radius, 0, Mathf.Sin(a) * radius);
                Gizmos.DrawLine(prev, next);
                prev = next;
            }
        }
#endif
    }
}
