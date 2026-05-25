// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Player / PlayerController
//
// PHASE 26 UPGRADE (2026-05-24):
// ─────────────────────────────────────────────────────────────────────────
// The cozy walk-only baseline of v0.2.0 has been generalised into a robust
// third-person character controller suitable for a Mixamo-Humanoid rig:
//
//   • Camera-relative WASD movement (uses MainCamera.forward when present;
//     falls back to world-axis input when no camera is wired — preserves the
//     old behaviour for headless EditMode tests).
//   • Smooth acceleration + analog-input ramp (gamepad sticks feel right).
//   • Toggleable SPRINT (Left Shift / Gamepad LStick click). Off in
//     Gentle Mode and during locked dialogue.
//   • Optional JUMP (Space / Gamepad south). Off in Gentle Mode.
//     Coyote-time + jump-buffer windows for forgiving feel.
//   • Manual gravity integration on CharacterController.Move() so jumps and
//     ledge falls work; SimpleMove is no longer used (it owned gravity
//     internally and conflicted with the jump impulse).
//   • Smooth turn-toward-velocity rotation, with optional "always face
//     camera-forward" mode for over-the-shoulder shots (deferred for now).
//   • Animator parameter bridge with safe defaults — sets:
//       Speed         (float 0..2  : 0=idle, 1=walk, 2=run)
//       MoveX, MoveY  (floats -1..1 for blend trees that need split axes)
//       VelocityY     (float, for jump up/down blend)
//       IsGrounded    (bool)
//       IsSprinting   (bool)
//       Jump          (trigger)
//   • Public API preserved (`MovementLocked`, `CurrentFocus`,
//     `CurrentMoveInput`, `TryActivateFocus`) so existing
//     Mission01Director / Mission02Director references still compile.
//
// Compatibility notes:
//   • Existing scenes were saved with the old serialized field set. All new
//     fields default to safe values so re-opening an old scene does not
//     regress (no jumps will fire because `enableJump` defaults to true
//     but `Space` only does anything if grounded).
//   • The Phase 26 Editor builder re-wires the runtime Animator parameter
//     names. If a scene still references the old "Speed" param it keeps
//     working — Speed is the canonical walk/run scalar in both the old
//     and new controller.
//
// See Docs/ANIMATION_REQUIREMENTS.md for the matching Animator Controller
// graph + the Mixamo / BoZo animation source map.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using HearthboundHollow.Core;

namespace HearthboundHollow.Player
{
    [DefaultExecutionOrder(-50)]
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        // ───── Movement tuning ─────────────────────────────────────

        [Header("Movement — speeds (m/s)")]
        [Tooltip("Walk speed (full WASD). The cozy default.")]
        [SerializeField] private float walkSpeed = 2.4f;

        [Tooltip("Sprint speed (Left Shift held). Multiplier × walk would feel " +
                 "abrupt — an absolute value yields a controllable feel curve.")]
        [SerializeField] private float sprintSpeed = 4.2f;

        [Tooltip("How quickly horizontal velocity ramps to the target. " +
                 "Higher = snappier; lower = floatier.")]
        [Range(2f, 40f)]
        [SerializeField] private float acceleration = 16f;

        [Tooltip("Deceleration when input is released. Slightly higher than " +
                 "acceleration prevents the character from sliding on stop.")]
        [Range(2f, 40f)]
        [SerializeField] private float deceleration = 22f;

        [Header("Movement — rotation")]
        [Tooltip("Slerp rate at which the body turns toward the input direction.")]
        [Range(1f, 30f)]
        [SerializeField] private float turnLerp = 12f;

        [Tooltip("Reference camera transform. If left unassigned the controller " +
                 "uses Camera.main; if neither is present, input is treated as " +
                 "world-axis. Wired by Phase 26 builder + scene spawn helpers.")]
        [SerializeField] private Transform cameraReference;

        // ───── Jump ────────────────────────────────────────────────

        [Header("Jump")]
        [Tooltip("Enable the optional jump action. Off in Gentle Mode at runtime " +
                 "(SettingsService.GentleMode == true). The cozy game does not " +
                 "require a jump anywhere in M1-2 — this exists to feel right " +
                 "on small obstacles and for accessibility tests.")]
        [SerializeField] private bool enableJump = true;

        [Tooltip("Initial upward velocity (m/s) on jump.")]
        [Range(2f, 12f)]
        [SerializeField] private float jumpVelocity = 5.2f;

        [Tooltip("Gravity acceleration (negative = downward).")]
        [SerializeField] private float gravity = -22f;

        [Tooltip("Coyote time — seconds after walking off a ledge during which a " +
                 "jump is still allowed. Forgiving.")]
        [Range(0f, 0.4f)]
        [SerializeField] private float coyoteTime = 0.15f;

        [Tooltip("Jump buffer — if Space is pressed slightly before landing, the " +
                 "buffered jump fires on touchdown.")]
        [Range(0f, 0.4f)]
        [SerializeField] private float jumpBuffer = 0.12f;

        [Tooltip("Vertical velocity reset value while grounded (small negative " +
                 "value keeps the CharacterController firmly on slopes).")]
        [SerializeField] private float groundedStickVelocity = -2f;

        // ───── Sprint ──────────────────────────────────────────────

        [Header("Sprint")]
        [Tooltip("Enable the sprint modifier (Left Shift / LStick click).")]
        [SerializeField] private bool enableSprint = true;

        // ───── Interaction raycast ─────────────────────────────────

        [Header("Interaction raycast")]
        [SerializeField] private float interactRange = 2.2f;
        [SerializeField] private float interactRadius = 0.45f;
        [SerializeField] private LayerMask interactMask = ~0;
        [SerializeField] private Transform interactOrigin;

        // ───── Animator hookup ─────────────────────────────────────

        [Header("Animator")]
        [Tooltip("Optional. If the BoZo / Mixamo character has an Animator on " +
                 "this GameObject or any child, set it here. The controller will " +
                 "drive Speed / IsGrounded / VelocityY / IsSprinting / Jump.")]
        [SerializeField] private Animator animator;
        [SerializeField] private string animSpeedParam      = "Speed";
        [SerializeField] private string animMoveXParam      = "MoveX";
        [SerializeField] private string animMoveYParam      = "MoveY";
        [SerializeField] private string animVelocityYParam  = "VelocityY";
        [SerializeField] private string animIsGroundedParam = "IsGrounded";
        [SerializeField] private string animIsSprintingParam = "IsSprinting";
        [SerializeField] private string animJumpTriggerParam = "Jump";
        [Tooltip("Damping applied when feeding the Speed parameter into the " +
                 "Animator — keeps the blend-tree from twitching.")]
        [Range(0.01f, 0.5f)]
        [SerializeField] private float animatorDamp = 0.1f;

        // ───── Input ───────────────────────────────────────────────

        [Header("Input (Unity Input System asset references)")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference interactAction;
        [Tooltip("Optional. Sprint hold action (e.g. Left Shift / Gamepad " +
                 "left-stick click). If unset, falls back to Input.GetKey(LeftShift).")]
        [SerializeField] private InputActionReference sprintAction;
        [Tooltip("Optional. Jump press action (e.g. Space / Gamepad south). " +
                 "If unset, falls back to Input.GetKeyDown(Space).")]
        [SerializeField] private InputActionReference jumpAction;

        // ───── Public state (read-only API) ────────────────────────

        /// <summary>The closest interactable currently focussed by the player.</summary>
        public Interactable CurrentFocus { get; private set; }

        /// <summary>Raw input as a planar XZ vector (no camera transformation).</summary>
        public Vector3 CurrentMoveInput { get; private set; }

        /// <summary>World-space velocity actually fed to CharacterController.Move().</summary>
        public Vector3 CurrentVelocity { get; private set; }

        /// <summary>True while the controller is grounded (CharacterController.isGrounded).</summary>
        public bool IsGrounded { get; private set; }

        /// <summary>True while the player is sprinting (Shift held + permission).</summary>
        public bool IsSprinting { get; private set; }

        /// <summary>Locks all movement + sprint + jump. Set by dialogue / mini-game runners.</summary>
        public bool MovementLocked { get; set; } = false;

        // ───── Internals ───────────────────────────────────────────

        private static readonly Collider[] _hits = new Collider[8];
        private readonly List<Interactable> _candidates = new(4);

        private CharacterController _cc;
        private int _animSpeedHash, _animMoveXHash, _animMoveYHash;
        private int _animVelocityYHash, _animIsGroundedHash, _animIsSprintingHash, _animJumpHash;

        private float _verticalVelocity;
        private float _coyoteTimer;
        private float _jumpBufferTimer;
        private bool  _wasGroundedLastFrame;

        // ───── Lifecycle ───────────────────────────────────────────

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            if (interactOrigin == null) interactOrigin = transform;

            // Try to auto-pick up the animator from common rig layouts. BoZo's
            // animator sits on the "Body" child created by Phase 13. Defensive:
            // we don't fail if the rig isn't there yet — the controller still
            // moves, the animator just isn't driven.
            if (animator == null) animator = GetComponentInChildren<Animator>(true);

            _animSpeedHash       = Animator.StringToHash(animSpeedParam);
            _animMoveXHash       = Animator.StringToHash(animMoveXParam);
            _animMoveYHash       = Animator.StringToHash(animMoveYParam);
            _animVelocityYHash   = Animator.StringToHash(animVelocityYParam);
            _animIsGroundedHash  = Animator.StringToHash(animIsGroundedParam);
            _animIsSprintingHash = Animator.StringToHash(animIsSprintingParam);
            _animJumpHash        = Animator.StringToHash(animJumpTriggerParam);
        }

        private void OnEnable()
        {
            EnableAction(moveAction);
            EnableAction(sprintAction);

            if (interactAction != null && interactAction.action != null)
            {
                interactAction.action.performed += OnInteractPerformed;
                interactAction.action.Enable();
            }
            if (jumpAction != null && jumpAction.action != null)
            {
                jumpAction.action.performed += OnJumpPerformed;
                jumpAction.action.Enable();
            }
        }

        private void OnDisable()
        {
            if (interactAction != null && interactAction.action != null)
                interactAction.action.performed -= OnInteractPerformed;
            if (jumpAction != null && jumpAction.action != null)
                jumpAction.action.performed -= OnJumpPerformed;
        }

        private static void EnableAction(InputActionReference r)
        {
            if (r != null && r.action != null && !r.action.enabled) r.action.Enable();
        }

        // ───── Frame update ────────────────────────────────────────

        private void Update()
        {
            // 1) Grounded probe — CharacterController updates this every frame.
            IsGrounded = _cc != null && _cc.isGrounded;

            // 2) Movement-lock fast-path (dialogue, ledger, etc.).
            if (MovementLocked)
            {
                ApplyMovementLockedFrame();
                ScanForInteractable();
                return;
            }

            // 3) Read planar input + sprint intent.
            Vector2 rawXY = ReadMoveInput();
            CurrentMoveInput = new Vector3(rawXY.x, 0f, rawXY.y);
            bool sprintRequested = enableSprint && IsSprintHeld() && !IsGentleMode();
            IsSprinting = sprintRequested && rawXY.sqrMagnitude > 0.05f;

            // 4) Camera-relative direction.
            Vector3 worldInput = ResolveWorldSpaceInput(CurrentMoveInput);

            // 5) Horizontal velocity with acceleration + deceleration.
            Vector3 desired = worldInput * (IsSprinting ? sprintSpeed : walkSpeed);
            Vector3 currentHoriz = new Vector3(CurrentVelocity.x, 0f, CurrentVelocity.z);
            float rate = (desired.sqrMagnitude > currentHoriz.sqrMagnitude) ? acceleration : deceleration;
            Vector3 newHoriz = Vector3.MoveTowards(currentHoriz, desired, rate * Time.deltaTime);

            // 6) Gravity + jump.
            TickJumpTimers();
            HandleJumpAndGravity();

            CurrentVelocity = new Vector3(newHoriz.x, _verticalVelocity, newHoriz.z);

            // 7) Apply to CharacterController.
            if (_cc != null && _cc.enabled)
                _cc.Move(CurrentVelocity * Time.deltaTime);
            else
                transform.position += CurrentVelocity * Time.deltaTime;

            // 8) Rotate body toward movement direction (planar only).
            if (worldInput.sqrMagnitude > 0.01f)
            {
                var target = Quaternion.LookRotation(worldInput, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, target, turnLerp * Time.deltaTime);
            }

            // 9) Animator bridge + interaction probe.
            UpdateAnimator(rawXY);
            ScanForInteractable();

            _wasGroundedLastFrame = IsGrounded;
        }

        private void ApplyMovementLockedFrame()
        {
            CurrentMoveInput = Vector3.zero;
            IsSprinting = false;
            // While locked, gravity still pulls — otherwise the player can
            // float if a dialogue starts mid-air. CharacterController.Move()
            // with only the vertical component keeps things on the ground.
            if (IsGrounded && _verticalVelocity < 0f) _verticalVelocity = groundedStickVelocity;
            else _verticalVelocity += gravity * Time.deltaTime;

            CurrentVelocity = new Vector3(0f, _verticalVelocity, 0f);
            if (_cc != null && _cc.enabled) _cc.Move(CurrentVelocity * Time.deltaTime);

            if (animator != null)
            {
                animator.SetFloat(_animSpeedHash, 0f, animatorDamp, Time.deltaTime);
                animator.SetFloat(_animMoveXHash, 0f, animatorDamp, Time.deltaTime);
                animator.SetFloat(_animMoveYHash, 0f, animatorDamp, Time.deltaTime);
                animator.SetBool(_animIsSprintingHash, false);
                animator.SetBool(_animIsGroundedHash, IsGrounded);
                animator.SetFloat(_animVelocityYHash, _verticalVelocity);
            }
        }

        private void TickJumpTimers()
        {
            // Coyote: count down only after we left the ground.
            if (IsGrounded) _coyoteTimer = coyoteTime;
            else _coyoteTimer = Mathf.Max(0f, _coyoteTimer - Time.deltaTime);

            // Buffered jump (set by OnJumpPerformed or legacy Space-keydown).
            _jumpBufferTimer = Mathf.Max(0f, _jumpBufferTimer - Time.deltaTime);
            if (enableJump && !IsGentleMode() && Input.GetKeyDown(KeyCode.Space) && jumpAction == null)
                _jumpBufferTimer = jumpBuffer; // legacy keyboard fallback
        }

        private void HandleJumpAndGravity()
        {
            if (IsGrounded)
            {
                if (_verticalVelocity < 0f) _verticalVelocity = groundedStickVelocity;

                // Consume a buffered jump if conditions permit.
                if (enableJump && !IsGentleMode() && _jumpBufferTimer > 0f)
                {
                    _verticalVelocity = jumpVelocity;
                    _jumpBufferTimer = 0f;
                    if (animator != null) animator.SetTrigger(_animJumpHash);
                    Hh.Log(LogCategory.Input, "Jump (from ground).");
                }
            }
            else
            {
                // Coyote: ledge-edge jump still allowed for a moment.
                if (enableJump && !IsGentleMode() && _jumpBufferTimer > 0f && _coyoteTimer > 0f)
                {
                    _verticalVelocity = jumpVelocity;
                    _jumpBufferTimer = 0f;
                    _coyoteTimer = 0f;
                    if (animator != null) animator.SetTrigger(_animJumpHash);
                    Hh.Log(LogCategory.Input, "Jump (from coyote window).");
                }
                _verticalVelocity += gravity * Time.deltaTime;
            }
        }

        // ───── Input resolution ────────────────────────────────────

        private Vector2 ReadMoveInput()
        {
            if (moveAction != null && moveAction.action != null && moveAction.action.enabled)
                return moveAction.action.ReadValue<Vector2>();
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }

        private bool IsSprintHeld()
        {
            if (sprintAction != null && sprintAction.action != null && sprintAction.action.enabled)
                return sprintAction.action.IsPressed();
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }

        private static bool IsGentleMode()
        {
            // Gentle Mode is the SettingsService bool — we route through
            // ServiceLocator so this class stays decoupled from the Settings
            // implementation. Defensive: when SettingsService isn't registered
            // (EditMode tests, smoke scenes) we treat it as "not gentle".
            var s = ServiceLocator.Get<SettingsService>();
            return s != null && s.GentleMode;
        }

        private Vector3 ResolveWorldSpaceInput(Vector3 planarLocal)
        {
            // No input → no work.
            if (planarLocal.sqrMagnitude < 0.0001f) return Vector3.zero;

            Transform cam = cameraReference != null ? cameraReference
                          : Camera.main != null ? Camera.main.transform
                          : null;

            if (cam == null) return planarLocal.normalized; // world-axis fallback

            Vector3 fwd = cam.forward; fwd.y = 0; fwd.Normalize();
            Vector3 right = cam.right;  right.y = 0; right.Normalize();
            return (fwd * planarLocal.z + right * planarLocal.x).normalized;
        }

        // ───── Animator bridge ─────────────────────────────────────

        private void UpdateAnimator(Vector2 rawXY)
        {
            if (animator == null) return;

            // Speed: 0 = idle, 1 = walk, 2 = run. The blend tree thresholds
            // are 0 / 1 / 2 in Phase 26's builder.
            float walkMag = Mathf.Clamp01(rawXY.magnitude);
            float runScale = IsSprinting ? 2f : 1f;
            float speedParam = walkMag * runScale;

            animator.SetFloat(_animSpeedHash, speedParam, animatorDamp, Time.deltaTime);
            animator.SetFloat(_animMoveXHash, rawXY.x, animatorDamp, Time.deltaTime);
            animator.SetFloat(_animMoveYHash, rawXY.y, animatorDamp, Time.deltaTime);
            animator.SetFloat(_animVelocityYHash, _verticalVelocity);
            animator.SetBool(_animIsGroundedHash, IsGrounded);
            animator.SetBool(_animIsSprintingHash, IsSprinting);
        }

        // ───── Action callbacks ────────────────────────────────────

        private void OnInteractPerformed(InputAction.CallbackContext _) => TryActivateFocus();

        private void OnJumpPerformed(InputAction.CallbackContext _)
        {
            if (!enableJump || IsGentleMode() || MovementLocked) return;
            _jumpBufferTimer = jumpBuffer;
        }

        public void TryActivateFocus()
        {
            if (CurrentFocus == null || !CurrentFocus.IsInteractable) return;
            Hh.Log(LogCategory.Input, $"Player activated '{CurrentFocus.PromptText}' on {CurrentFocus.name}");
            CurrentFocus.Activate(gameObject);
        }

        // ───── Interaction probe ───────────────────────────────────

        private void ScanForInteractable()
        {
            int count = Physics.OverlapSphereNonAlloc(
                interactOrigin.position + transform.forward * (interactRange * 0.5f),
                interactRadius + interactRange * 0.5f,
                _hits, interactMask, QueryTriggerInteraction.Collide);
            _candidates.Clear();
            for (int i = 0; i < count; i++)
            {
                if (_hits[i] == null) continue;
                var inter = _hits[i].GetComponentInParent<Interactable>();
                if (inter == null || !inter.IsInteractable) continue;
                _candidates.Add(inter);
            }
            CurrentFocus = ClosestCandidate();
        }

        private Interactable ClosestCandidate()
        {
            Interactable best = null;
            float bestDistSqr = float.MaxValue;
            for (int i = 0; i < _candidates.Count; i++)
            {
                var c = _candidates[i];
                float d = (c.transform.position - interactOrigin.position).sqrMagnitude;
                if (d < bestDistSqr) { best = c; bestDistSqr = d; }
            }
            return best;
        }

        // ───── Editor convenience ──────────────────────────────────

        /// <summary>Allow the Phase 26 builder + scene helpers to point the controller at a camera.</summary>
        public void SetCameraReference(Transform t) => cameraReference = t;

        /// <summary>Allow the builder + tests to swap the Animator at runtime.</summary>
        public void SetAnimator(Animator a) => animator = a;

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            var origin = interactOrigin != null ? interactOrigin : transform;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(origin.position + origin.forward * (interactRange * 0.5f), interactRadius + interactRange * 0.5f);
        }
#endif
    }
}
