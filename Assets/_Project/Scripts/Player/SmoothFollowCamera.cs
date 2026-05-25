// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Player / SmoothFollowCamera
//
// PHASE 26 — The polished third-person follow camera.
//
// The original `SimpleFollowCamera` (still present alongside this) gave us a
// damped follow + look-at. Useful for the engineering MVP, but visibly
// jittery at sprint speeds, and offered no player control. This camera:
//
//   • Spring-damped position (SmoothDamp on Vector3) with adjustable
//     responsiveness — feels "cinematic" without lag in dialogue scenes.
//   • Spherical orbit around the target driven by mouse (RMB hold) or
//     right gamepad stick. Yaw is unlimited; pitch clamps to a soft range
//     so we never bend over the player's head or under the floor.
//   • Scroll-wheel zoom (or RB/LB on a gamepad) between configurable min/max
//     distances. Persists across scenes only if the Phase 26 builder
//     re-instantiates this component.
//   • Wall-clip protection: a single sphere-cast from the target to the
//     desired camera position; if it hits geometry the camera slides in to
//     the hit point so we never look through walls.
//   • Cinemachine-agnostic — works without Cinemachine installed. The
//     Phase 17 builder still prefers Cinemachine when its package is
//     present; this script is the fallback + cozy-default.
//
// USAGE
//   • Attach to a GameObject that also has a UnityEngine.Camera.
//   • Set `target` to the Player root (NOT the head bone — we frame the
//     whole character).
//   • Optional: assign `lookOffset` to lift the framing toward the chest /
//     face (default 1.2 m above the pivot is fine for the BoZo chibi).
//
// COMPATIBILITY
//   • The Phase 22 / Phase 23 / Phase 24 scene builders attach
//     `SimpleFollowCamera` — they continue to work. Phase 26's builder
//     UPGRADES the camera from SimpleFollowCamera → SmoothFollowCamera in
//     every gameplay scene (Lane, Hollow, Garden, Cottage).

using UnityEngine;
using UnityEngine.InputSystem;
using HearthboundHollow.Core;

namespace HearthboundHollow.Player
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(50)] // run after PlayerController so we follow the latest position
    public class SmoothFollowCamera : MonoBehaviour
    {
        // ───── Targeting ───────────────────────────────────────────

        [Header("Target")]
        [Tooltip("The transform to follow. Usually the Player root. " +
                 "The camera frames `target.position + lookOffset`.")]
        public Transform target;

        [Tooltip("World-space offset applied to the target before any framing — " +
                 "use to raise the look-at point to the chest/face.")]
        public Vector3 lookOffset = new Vector3(0f, 1.4f, 0f);

        // ───── Orbit ───────────────────────────────────────────────

        [Header("Orbit")]
        [Tooltip("Initial pitch (downward angle in degrees) — positive looks down.")]
        [Range(0f, 80f)]
        public float pitch = 28f;

        [Tooltip("Initial yaw (horizontal angle in degrees). The actual yaw " +
                 "drifts at runtime via mouse / right-stick input.")]
        public float yaw = 0f;

        [Tooltip("Minimum allowed pitch.")]
        [Range(0f, 80f)] public float pitchMin = 8f;

        [Tooltip("Maximum allowed pitch.")]
        [Range(0f, 80f)] public float pitchMax = 65f;

        [Tooltip("Yaw sensitivity (degrees per second per unit input).")]
        public float yawSensitivity = 220f;

        [Tooltip("Pitch sensitivity (degrees per second per unit input).")]
        public float pitchSensitivity = 140f;

        [Tooltip("If true, mouse orbit requires holding the Right Mouse Button. " +
                 "If false, the cursor always orbits (FPS-style).")]
        public bool mouseRequiresRMB = true;

        [Tooltip("Invert the vertical look axis (some players prefer this).")]
        public bool invertY = false;

        // ───── Distance + zoom ─────────────────────────────────────

        [Header("Distance / zoom")]
        [Tooltip("Current distance from the look-at point along the (pitch,yaw) " +
                 "orbit direction.")]
        public float distance = 5.5f;

        [Tooltip("Minimum zoom distance.")]
        public float distanceMin = 2.5f;

        [Tooltip("Maximum zoom distance.")]
        public float distanceMax = 8.5f;

        [Tooltip("Per-tick zoom step (units per scroll line / RB-LB press).")]
        public float zoomStep = 0.6f;

        // ───── Smoothing ───────────────────────────────────────────

        [Header("Smoothing")]
        [Tooltip("Position SmoothDamp time. Lower = snappier follow.")]
        [Range(0.02f, 0.6f)]
        public float positionSmoothTime = 0.12f;

        [Tooltip("Rotation Slerp factor (per second). Higher = snappier face-toward-target.")]
        [Range(2f, 30f)]
        public float rotationLerp = 12f;

        // ───── Wall-clip protection ───────────────────────────────

        [Header("Wall-clip")]
        [Tooltip("Whether the camera sphere-casts back from the player to avoid " +
                 "clipping through walls.")]
        public bool clipAgainstGeometry = true;

        [Tooltip("Sphere-cast radius for the clip probe.")]
        public float clipRadius = 0.35f;

        [Tooltip("Layers the clip probe collides with. Set to the same value as " +
                 "the environment layer in the project (default Everything — fine " +
                 "for M1-2; restrict for the bigger missions).")]
        public LayerMask clipMask = ~0;

        // ───── Input ───────────────────────────────────────────────

        [Header("Input — optional Input System action references")]
        [Tooltip("Vector2 action for camera look. Defaults to mouse delta when null.")]
        public InputActionReference cameraLookAction;

        [Tooltip("Float action for camera zoom. Defaults to mouse scroll when null.")]
        public InputActionReference cameraZoomAction;

        [Tooltip("Button action for 'allow look' (e.g. Right Mouse). When the " +
                 "binding is held, mouse delta is fed into yaw/pitch. If unbound, " +
                 "we fall back to Input.GetMouseButton(1).")]
        public InputActionReference allowLookAction;

        // ───── Internal state ──────────────────────────────────────

        private Vector3 _velocityCache;
        private Vector3 _smoothedTargetPos;

        private void OnEnable()
        {
            if (cameraLookAction != null && cameraLookAction.action != null) cameraLookAction.action.Enable();
            if (cameraZoomAction != null && cameraZoomAction.action != null) cameraZoomAction.action.Enable();
            if (allowLookAction != null && allowLookAction.action != null) allowLookAction.action.Enable();
        }

        private void Start()
        {
            if (target != null) _smoothedTargetPos = target.position + lookOffset;
        }

        private void LateUpdate()
        {
            if (target == null) return;

            // 1) Read player look + zoom intent.
            Vector2 look = ReadLookDelta();
            float zoom = ReadZoomDelta();
            distance = Mathf.Clamp(distance - zoom * zoomStep, distanceMin, distanceMax);

            yaw += look.x * yawSensitivity * Time.unscaledDeltaTime;
            float pitchDelta = look.y * pitchSensitivity * Time.unscaledDeltaTime;
            pitch = Mathf.Clamp(pitch + (invertY ? -pitchDelta : pitchDelta), pitchMin, pitchMax);

            // 2) Compute desired transform.
            Vector3 pivot = target.position + lookOffset;
            // Spring-damp the pivot so the camera doesn't snap when the player
            // teleports during scene loads.
            _smoothedTargetPos = Vector3.SmoothDamp(_smoothedTargetPos, pivot, ref _velocityCache, positionSmoothTime);

            // Convert (yaw, pitch, distance) → cartesian offset.
            Quaternion orbit = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 desiredOffset = orbit * (Vector3.back * distance);
            Vector3 desiredPos = _smoothedTargetPos + desiredOffset;

            // 3) Wall-clip protection.
            if (clipAgainstGeometry)
            {
                Vector3 dir = desiredPos - _smoothedTargetPos;
                float dist = dir.magnitude;
                if (dist > 0.01f)
                {
                    Ray r = new Ray(_smoothedTargetPos, dir.normalized);
                    if (Physics.SphereCast(r, clipRadius, out RaycastHit hit, dist, clipMask, QueryTriggerInteraction.Ignore))
                    {
                        desiredPos = hit.point - dir.normalized * clipRadius;
                    }
                }
            }

            // 4) Apply.
            transform.position = desiredPos;
            Quaternion lookRot = Quaternion.LookRotation(_smoothedTargetPos - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, rotationLerp * Time.unscaledDeltaTime);
        }

        // ───── Helpers ─────────────────────────────────────────────

        private Vector2 ReadLookDelta()
        {
            // Allow-look gate.
            bool allow = !mouseRequiresRMB;
            if (allowLookAction != null && allowLookAction.action != null && allowLookAction.action.enabled)
                allow = allow || allowLookAction.action.IsPressed();
            else
                allow = allow || Input.GetMouseButton(1);

            if (!allow) return Vector2.zero;

            // Input System mouse delta is in pixels-per-tick (very large numbers
            // at common DPI). We normalise by a soft constant so the
            // `yawSensitivity` is editor-friendly across mice.
            Vector2 delta = Vector2.zero;
            if (cameraLookAction != null && cameraLookAction.action != null && cameraLookAction.action.enabled)
            {
                delta = cameraLookAction.action.ReadValue<Vector2>();
            }
            else
            {
                delta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
                // Legacy `Mouse X/Y` returns small deltas — scale up so the
                // sensitivity feels right when actions aren't wired.
                delta *= 12f;
            }

            // Per-pixel → degrees-equivalent scale. 1/40 is a comfortable rule.
            return delta * 0.025f;
        }

        private float ReadZoomDelta()
        {
            if (cameraZoomAction != null && cameraZoomAction.action != null && cameraZoomAction.action.enabled)
                return cameraZoomAction.action.ReadValue<float>();
            return Input.mouseScrollDelta.y;
        }

        // ───── Editor helpers ─────────────────────────────────────

        public void SnapToTargetImmediate()
        {
            if (target == null) return;
            _smoothedTargetPos = target.position + lookOffset;
            Quaternion orbit = Quaternion.Euler(pitch, yaw, 0f);
            transform.position = _smoothedTargetPos + orbit * (Vector3.back * distance);
            transform.rotation = Quaternion.LookRotation(_smoothedTargetPos - transform.position, Vector3.up);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (target == null) return;
            Gizmos.color = new Color(0.95f, 0.85f, 0.5f, 0.75f);
            Gizmos.DrawLine(target.position + lookOffset, transform.position);
            Gizmos.DrawWireSphere(target.position + lookOffset, 0.12f);
        }
#endif
    }
}
