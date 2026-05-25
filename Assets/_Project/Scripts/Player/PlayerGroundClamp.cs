// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Player / PlayerGroundClamp
//
// PHASE 28 DEFINITIVE FIX (2026-05-25)
// =====================================================================
// HISTORY:
//   • Phase 26 — first introduced a runtime ground-clamp that read
//     SkinnedMeshRenderer.localBounds.
//   • Phase 27.1 — moved the file into HearthboundHollow.Player asmdef
//     and added it to the Phase 13 prefab + every scene Player.
//   • Phase 27.2 — embedded a copy of the logic in PlayerController so
//     fresh pulls didn't need a re-run of the Phase 26 capstone.
//   • PHASE 28 (this rev) — root-cause the lingering "half body still
//     sinks" report: SkinnedMeshRenderer.localBounds returned by some
//     character rigs (BoZo CharacterCreator variants in particular) is
//     the renderer's *cull AABB*, not the actual bind-pose bottom. It
//     is artificially padded so the renderer is never frustum-culled
//     during stretched animations — which produced an alignment delta
//     that left the feet roughly 30-50 cm in the floor.
//
// THE PHASE 28 ALGORITHM
// ----------------------
// 1. PREFER live world-space `Renderer.bounds.min.y`. This reflects the
//    current pose *after* the Animator has updated, so it's the truth
//    about where the visible feet are right now.
// 2. Run alignment EVERY frame for the first 0.75 s, then settle to a
//    cheaper periodic re-check (every 0.5 s) so the Animator's first
//    couple of frames (bind-pose → idle blend) don't leave a residual
//    offset. A `clampActiveDuration = 0` disables periodic re-checks
//    completely if a perfectly-static rig is in use.
// 3. Only apply a shift when the delta exceeds `epsilon` (0.5 cm), so
//    floating-point chatter doesn't twitch the mesh.
// 4. Optional override hooks: drag a `footAnchor` Transform onto a toe
//    bone for surgical precision. When set, the world Y of that
//    transform is used instead of bounds scanning.
//
// On a rig where the mesh origin sits at the hips and the feet hang
// roughly 0.9 m below it, the Phase 28 pass shifts the Body child
// localPosition.y by +0.9 m on frame 1 and never has to move it again.
// On a rig that's already correctly authored at-the-feet, the delta is
// < epsilon so nothing happens.
//
// IMPORTANT: when this component is present on the Player root,
// PlayerController detects it and skips its own intrinsic clamp to
// avoid double-shifting.

using UnityEngine;
using HearthboundHollow.Core;

namespace HearthboundHollow.Player
{
    [DisallowMultipleComponent]
    // Execute after PlayerController (which is -50) so the CC is initialised
    // and after Mixamo / BoZo Animators have driven the rig for one frame.
    [DefaultExecutionOrder(-40)]
    public class PlayerGroundClamp : MonoBehaviour
    {
        // ───── Targets ─────────────────────────────────────────────

        [Header("Targets")]
        [Tooltip("The child GameObject holding the character mesh. Auto-found " +
                 "on Awake by looking for a child named 'Body' (matches the " +
                 "Phase 13 BoZo wrapper layout).")]
        public Transform body;

        [Tooltip("OPTIONAL: drag a toe/foot bone here to use its world Y as the " +
                 "'mesh bottom' instead of scanning the renderer bounds. " +
                 "Most surgical option — use this on rigs that have weird " +
                 "padded localBounds.")]
        public Transform footAnchor;

        // ───── Behaviour ───────────────────────────────────────────

        [Header("Behaviour")]
        [Tooltip("Run Align() automatically in Start(). Recommended.")]
        public bool alignOnStart = true;

        [Tooltip("How many seconds after Start the clamp keeps re-aligning " +
                 "every frame. Mixamo's bind-to-idle blend usually completes " +
                 "in ~0.5 s; the default leaves a small safety margin. Set 0 " +
                 "to disable continuous correction.")]
        [Range(0f, 3f)]
        public float continuousDuration = 0.75f;

        [Tooltip("After the continuous window, how often to re-check (seconds). " +
                 "Set 0 to disable periodic checks once the continuous window " +
                 "expires.")]
        [Range(0f, 5f)]
        public float periodicInterval = 0.5f;

        [Tooltip("Tolerance — only shift the body if the delta exceeds this " +
                 "(metres). Default 0.5 cm prevents floating-point chatter.")]
        [Range(0.001f, 0.05f)]
        public float epsilon = 0.005f;

        [Tooltip("Prefer the world-space `Renderer.bounds` (current pose) " +
                 "over `SkinnedMeshRenderer.localBounds` (bind pose). Strongly " +
                 "recommended — localBounds is often artificially padded for " +
                 "culling and gives wrong answers on BoZo rigs.")]
        public bool preferWorldBounds = true;

        [Tooltip("Manual fudge — positive lifts the mesh, negative pushes it " +
                 "further into the floor. Useful for cozy scenes where the " +
                 "character should plant a couple of cm into grass.")]
        [Range(-0.2f, 0.2f)]
        public float bias = 0f;

        // ───── Debug ───────────────────────────────────────────────

        [Header("Debug")]
        [Tooltip("Print every alignment shift to the Console. Useful for " +
                 "verifying the half-body-in-floor fix during playtests.")]
        public bool verbose = false;

        [Tooltip("Total accumulated localY shift applied — read-only diagnostic.")]
        [SerializeField] private float _appliedShiftDebug;

        // ───── Internals ───────────────────────────────────────────

        private CharacterController _cc;
        private float _continuousElapsed;
        private float _nextPeriodicCheck;
        private bool _firstAlignDone;

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
            // Continuous-correction window — every frame for the first
            // `continuousDuration` seconds, since the Animator hasn't fully
            // settled to its idle pose yet on frame 1.
            if (_continuousElapsed < continuousDuration)
            {
                _continuousElapsed += Time.deltaTime;
                Align();
                return;
            }

            // Periodic check after the continuous window — costs ~0.05 ms.
            if (periodicInterval > 0f && Time.time >= _nextPeriodicCheck)
            {
                _nextPeriodicCheck = Time.time + periodicInterval;
                Align();
            }
        }

        // ───── Manual entry-point ──────────────────────────────────

        [ContextMenu("Align Body to Ground")]
        public void Align()
        {
            if (body == null) body = ResolveBody();
            if (body == null)
            {
                if (verbose) Hh.Warn(LogCategory.State,
                    $"PlayerGroundClamp on '{name}' has no Body child to align.");
                return;
            }

            float meshBottomWorldY = ComputeMeshBottomWorldY();
            if (float.IsPositiveInfinity(meshBottomWorldY))
            {
                if (verbose) Hh.Warn(LogCategory.State,
                    $"PlayerGroundClamp on '{name}': could not determine mesh bottom (no renderers).");
                return;
            }

            float ccBottomWorldY = ComputeCcBottomWorldY();
            float diff = ccBottomWorldY - meshBottomWorldY + bias;

            // Tolerance guard — don't twitch on FP noise.
            if (Mathf.Abs(diff) < epsilon)
            {
                _firstAlignDone = true;
                return;
            }

            var lp = body.localPosition;
            body.localPosition = new Vector3(lp.x, lp.y + diff, lp.z);
            _appliedShiftDebug += diff;
            _firstAlignDone = true;

            if (verbose)
                Hh.Log(LogCategory.State,
                    $"PlayerGroundClamp on '{name}': shifted Body Δy={diff:F3} m " +
                    $"(mesh bottom Y={meshBottomWorldY:F3}, CC bottom Y={ccBottomWorldY:F3}, " +
                    $"bias={bias:F3}, cumulative={_appliedShiftDebug:F3}).");
        }

        // ───── Computations ────────────────────────────────────────

        private float ComputeMeshBottomWorldY()
        {
            // Most surgical option — explicit foot bone anchor.
            if (footAnchor != null) return footAnchor.position.y;

            if (preferWorldBounds)
            {
                // Use the live world-space AABB — this reflects the CURRENT
                // pose after the Animator has updated.
                float minY = float.PositiveInfinity;
                var renderers = body.GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < renderers.Length; i++)
                {
                    var r = renderers[i];
                    if (r == null || !r.enabled) continue;
                    // Skip particle systems / decals — they often have huge bounds.
                    if (r is ParticleSystemRenderer) continue;
                    if (r is LineRenderer) continue;
                    if (r is TrailRenderer) continue;

                    Bounds b = r.bounds;
                    // Defensive — uninitialised SkinnedMeshRenderer bounds can
                    // be (0,0,0) ± big. Sanity check.
                    if (b.size.sqrMagnitude < 0.0001f) continue;
                    if (b.min.y < minY) minY = b.min.y;
                }
                return minY;
            }

            // Fallback — local bounds path. Kept for parity with old behaviour
            // / very low-end rigs where world bounds are wrong.
            return ComputeMeshBottomFromLocalBounds();
        }

        private float ComputeMeshBottomFromLocalBounds()
        {
            float minY = float.PositiveInfinity;
            var renderers = body.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                if (r == null) continue;

                Bounds lb;
                if (r is SkinnedMeshRenderer smr) lb = smr.localBounds;
                else if (r is MeshRenderer && r.TryGetComponent<MeshFilter>(out var mf) && mf.sharedMesh != null)
                    lb = mf.sharedMesh.bounds;
                else
                    lb = r.bounds;

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
            return minY;
        }

        private float ComputeCcBottomWorldY()
        {
            if (_cc == null) _cc = GetComponent<CharacterController>();
            if (_cc != null)
            {
                // Capsule's geometric bottom in world space.
                // skinWidth is NOT added — it is a penetration tolerance for
                // the physics resolver, not an offset of the collision surface.
                return transform.position.y + _cc.center.y - _cc.height * 0.5f;
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

        // ───── Editor gizmos & utilities ──────────────────────────

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            var cc = GetComponent<CharacterController>();
            if (cc != null)
            {
                float ccBottomY = transform.position.y + cc.center.y - cc.height * 0.5f;
                Gizmos.color = new Color(0.4f, 1f, 0.4f, 0.85f);  // GREEN = CC bottom (target)
                DrawDisk(new Vector3(transform.position.x, ccBottomY, transform.position.z), cc.radius + 0.05f);
            }

            if (body != null)
            {
                if (footAnchor != null)
                {
                    Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.95f); // YELLOW = foot anchor
                    Gizmos.DrawWireSphere(footAnchor.position, 0.05f);
                }
                else
                {
                    var rs = body.GetComponentsInChildren<Renderer>();
                    if (rs.Length > 0)
                    {
                        float meshY = float.PositiveInfinity;
                        foreach (var r in rs)
                        {
                            if (r == null) continue;
                            if (r is ParticleSystemRenderer) continue;
                            if (r.bounds.size.sqrMagnitude < 0.0001f) continue;
                            if (r.bounds.min.y < meshY) meshY = r.bounds.min.y;
                        }
                        if (!float.IsPositiveInfinity(meshY))
                        {
                            Gizmos.color = new Color(1f, 0.5f, 0.3f, 0.85f);  // ORANGE = current mesh bottom
                            DrawDisk(new Vector3(transform.position.x, meshY, transform.position.z), 0.4f);
                        }
                    }
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
