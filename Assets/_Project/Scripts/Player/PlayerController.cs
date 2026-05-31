// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Player / PlayerController
//
// PHASE 26 UPGRADE (2026-05-24):
// ─────────────────────────────────────────────────────────────────────────
// The cozy walk-only baseline of v0.2.0 has been generalised into a robust
// third-person character controller suitable for a Mixamo-Humanoid rig.
// See the Phase 26 release notes for the WASD/Sprint/Jump/Animator surface.
//
// PHASE 28 ROBUST FIX (2026-05-25) — "half body in floor" definitive fix:
// ─────────────────────────────────────────────────────────────────────────
// The first playtest report after Phase 27.2 confirmed the mesh STILL sank
// when running on BoZo CharacterCreator variants. Root cause: those rigs
// expose a `SkinnedMeshRenderer.localBounds` that is a *padded culling AABB*
// — large enough to contain any pose, including stretched ones — so the
// "bottom" we measured from localBounds was 30-50 cm below the real bind
// pose feet. The clamp consequently lifted the body by *less* than needed.
//
// Phase 28 switches to live world-space `Renderer.bounds.min.y` (which
// reflects the CURRENT pose after the Animator updates) and runs the
// clamp every frame for the first 0.75 s instead of just once on the
// first LateUpdate. By the time the player can move, the visible feet
// are guaranteed to sit on the capsule bottom regardless of the rig's
// authoring quirks.
//
// The separate PlayerGroundClamp component (also upgraded in Phase 28)
// is still supported and now uses identical logic — its presence is
// detected and the controller skips the intrinsic clamp to avoid double-
// shifting. Existing scenes whose Player has the PlayerGroundClamp keep
// working; new scenes / pulled code without it get the intrinsic fix.
//
// See Docs/ANIMATION_REQUIREMENTS.md for the Animator graph + Mixamo source map.

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

        // ───── Body-to-ground intrinsic alignment (Phase 28 robust fix) ─

        [Header("Body alignment (Phase 28 — fix for half-body-in-floor)")]
        [Tooltip("Run AlignBodyToCcBottom() automatically on Start so the visible " +
                 "BoZo mesh feet land on the CharacterController capsule bottom " +
                 "even when BoZo's mesh origin isn't at the feet. If a separate " +
                 "PlayerGroundClamp component is attached, this is skipped to " +
                 "avoid double-shifting.")]
        [SerializeField] private bool autoAlignBodyOnStart = true;

        [Tooltip("Optional explicit reference to the visible body child. If null, " +
                 "the controller auto-finds a child named 'Body' (matches the " +
                 "Phase 13 BoZo wrapper layout), and otherwise falls back to the " +
                 "first child carrying renderers.")]
        [SerializeField] private Transform bodyOverride;

        [Tooltip("How many seconds after Start the intrinsic clamp keeps " +
                 "re-aligning every frame. The Animator's bind→idle blend " +
                 "settles within ~0.5 s; the default leaves a safety margin.")]
        [Range(0f, 3f)]
        [SerializeField] private float intrinsicAlignContinuousDuration = 0.75f;

        [Tooltip("Manual fudge added to the alignment shift — positive lifts the " +
                 "mesh, negative pushes it further into the floor. Useful for " +
                 "cozy scenes where the character should plant a couple of cm " +
                 "into grass.")]
        [Range(-0.2f, 0.2f)]
        [SerializeField] private float bodyAlignBias = 0f;

        [Tooltip("Alignment tolerance — only shifts the body if the delta " +
                 "exceeds this (metres). 0.5 cm avoids FP chatter.")]
        [Range(0.001f, 0.05f)]
        [SerializeField] private float bodyAlignEpsilon = 0.005f;

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

        [Tooltip("Phase 57 (D-074): ground speed (m/s) the WALK clip was authored " +
                 "for. The controller scales Animator playback so the legs keep " +
                 "pace with actual movement — this kills the 'creep'/foot-slide " +
                 "look that happens when walkSpeed differs from the clip's stride. " +
                 "Lower = faster leg cycle. Tune until the feet stop sliding.")]
        [Range(0.5f, 3f)]
        [SerializeField] private float animationStrideSpeed = 1.35f;

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

        // Body-alignment internals (Phase 28 robust fix)
        private Transform _resolvedBody;
        private float _intrinsicAlignElapsed;
        private bool _externalClampDetected;

        // ───── Lifecycle ───────────────────────────────────────────

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            if (interactOrigin == null) interactOrigin = transform;

            // Phase 32.15 — every PlayerController gets a PlayerSafetyNet
            // (fall-respawn + stuck-nudge + auto-boundary). Idempotent: the
            // [RequireComponent(PlayerController)] on PlayerSafetyNet means
            // Unity will auto-add it on serialise too; this call covers the
            // runtime path for prefabs that pre-date the safety net.
            if (GetComponent<PlayerSafetyNet>() == null)
                gameObject.AddComponent<PlayerSafetyNet>();

            // Try to auto-pick up the animator from common rig layouts. BoZo's
            // animator sits on the "Body" child created by Phase 13. Defensive:
            // we don't fail if the rig isn't there yet — the controller still
            // moves, the animator just isn't driven.
            if (animator == null) animator = GetComponentInChildren<Animator>(true);

            // Phase 57 (D-074): the script owns locomotion (CharacterController.Move),
            // so the Animator must NOT also apply root motion — otherwise the in-place
            // BoZo clips fight the script translation and the walk reads as a slide /
            // "creep". With root motion off, the legs cycle in place and the script
            // moves the body; stride-matching (UpdateAnimator) keeps the feet planted.
            if (animator != null) animator.applyRootMotion = false;

            _animSpeedHash       = Animator.StringToHash(animSpeedParam);
            _animMoveXHash       = Animator.StringToHash(animMoveXParam);
            _animMoveYHash       = Animator.StringToHash(animMoveYParam);
            _animVelocityYHash   = Animator.StringToHash(animVelocityYParam);
            _animIsGroundedHash  = Animator.StringToHash(animIsGroundedParam);
            _animIsSprintingHash = Animator.StringToHash(animIsSprintingParam);
            _animJumpHash        = Animator.StringToHash(animJumpTriggerParam);

            // Body alignment — detect a separate PlayerGroundClamp so we don't
            // double-shift the same Body. The clamp component lives in this
            // same namespace so a direct GetComponent works (no reflection).
            _externalClampDetected = GetComponent<PlayerGroundClamp>() != null;
            _resolvedBody = ResolveBodyTransform();
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

        private void Start()
        {
            // Phase 28 — align the visible BoZo mesh feet with the
            // CharacterController capsule bottom. Skipped if an explicit
            // PlayerGroundClamp component is on the same GameObject (it owns
            // the alignment in that case to avoid double-shifting).
            if (autoAlignBodyOnStart && !_externalClampDetected)
                AlignBodyToCcBottom();
        }

        private void LateUpdate()
        {
            // Phase 28 robust fix: continuous correction window. The Animator
            // bind→idle blend can take several frames to settle and a Mixamo
            // clip with a baked initial offset can leave a residual mismatch.
            // Re-align every frame during the continuous window so the visible
            // feet are *guaranteed* on the capsule bottom by frame 30.
            if (autoAlignBodyOnStart && !_externalClampDetected &&
                _intrinsicAlignElapsed < intrinsicAlignContinuousDuration)
            {
                _intrinsicAlignElapsed += Time.deltaTime;
                AlignBodyToCcBottom();
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

        // ───── Body-to-ground alignment (Phase 28 robust fix) ──────

        /// <summary>
        /// Shifts the visible body child's localPosition.y so the lowest
        /// renderer-bounds Y aligns with the CharacterController capsule
        /// bottom. Idempotent — re-calling it shifts only by any residual
        /// delta (0 if already aligned). Safe to call any time.
        ///
        /// Phase 28: uses the world-space `Renderer.bounds.min.y` (current pose)
        /// rather than `SkinnedMeshRenderer.localBounds` (which is often a
        /// padded cull AABB and gives wrong answers on BoZo rigs).
        /// </summary>
        public void AlignBodyToCcBottom()
        {
            if (_resolvedBody == null) _resolvedBody = ResolveBodyTransform();
            if (_resolvedBody == null) return;

            var renderers = _resolvedBody.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return;

            float meshBottomWorldY = ComputeMeshBottomWorldY(renderers);
            if (float.IsPositiveInfinity(meshBottomWorldY)) return;
            float ccBottomWorldY = ComputeCcBottomWorldY();

            float diff = ccBottomWorldY - meshBottomWorldY + bodyAlignBias;
            if (Mathf.Abs(diff) < bodyAlignEpsilon) return; // already aligned

            var lp = _resolvedBody.localPosition;
            _resolvedBody.localPosition = new Vector3(lp.x, lp.y + diff, lp.z);
        }

        private Transform ResolveBodyTransform()
        {
            if (bodyOverride != null) return bodyOverride;

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

        private float ComputeCcBottomWorldY()
        {
            // The CC capsule's geometric bottom in world space, with NO
            // skinWidth offset — skinWidth is a penetration tolerance, not an
            // offset of the collision surface. The visible mesh feet should
            // align with this point: when the CC settles on the floor, the
            // capsule bottom touches the floor and the mesh feet do too.
            if (_cc != null)
                return transform.position.y + _cc.center.y - _cc.height * 0.5f;
            return transform.position.y;
        }

        private float ComputeMeshBottomWorldY(Renderer[] renderers)
        {
            // Phase 28: use the live world-space bounds (post-Animator) rather
            // than padded localBounds. This is the BoZo half-body-sink fix.
            float minY = float.PositiveInfinity;
            for (int i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i];
                if (r == null || !r.enabled) continue;
                // Skip ranged / unbounded renderers — they have huge AABBs.
                if (r is ParticleSystemRenderer) continue;
                if (r is LineRenderer) continue;
                if (r is TrailRenderer) continue;

                Bounds b = r.bounds;
                // Sanity guard: an uninitialised SkinnedMeshRenderer can
                // briefly report (0,0,0) ± infinity. Skip those.
                if (b.size.sqrMagnitude < 0.0001f) continue;
                if (b.min.y < minY) minY = b.min.y;
            }
            return minY;
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
                // Phase 54 (D-072): while locked (dialogue / Evening Ledger / cards)
                // we must NOT keep a live interaction focus — otherwise the world
                // interaction prompt ("□ a note in Marin's hand") and the [E] hint
                // chip bleed through the modal UI, exactly as seen in the QA video.
                // Clearing focus hides those prompts for the duration of the lock.
                if (CurrentFocus != null) CurrentFocus = null;
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
                animator.speed = 1f; // D-074: reset stride-match so idle/dialogue plays at 1x
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
            var s = ServiceLocator.Get<SettingsService>();
            return s != null && s.GentleMode;
        }

        private Vector3 ResolveWorldSpaceInput(Vector3 planarLocal)
        {
            if (planarLocal.sqrMagnitude < 0.0001f) return Vector3.zero;

            Transform cam = cameraReference != null ? cameraReference
                          : Camera.main != null ? Camera.main.transform
                          : null;

            if (cam == null) return planarLocal.normalized;

            Vector3 fwd = cam.forward; fwd.y = 0; fwd.Normalize();
            Vector3 right = cam.right;  right.y = 0; right.Normalize();
            return (fwd * planarLocal.z + right * planarLocal.x).normalized;
        }

        // ───── Animator bridge ─────────────────────────────────────

        private void UpdateAnimator(Vector2 rawXY)
        {
            if (animator == null) return;

            float walkMag = Mathf.Clamp01(rawXY.magnitude);
            float runScale = IsSprinting ? 2f : 1f;
            float speedParam = walkMag * runScale;

            animator.SetFloat(_animSpeedHash, speedParam, animatorDamp, Time.deltaTime);
            animator.SetFloat(_animMoveXHash, rawXY.x, animatorDamp, Time.deltaTime);
            animator.SetFloat(_animMoveYHash, rawXY.y, animatorDamp, Time.deltaTime);
            animator.SetFloat(_animVelocityYHash, _verticalVelocity);
            animator.SetBool(_animIsGroundedHash, IsGrounded);
            animator.SetBool(_animIsSprintingHash, IsSprinting);

            // Phase 57 (D-074): stride-match. Scale Animator playback so the leg
            // cycle keeps pace with actual ground speed — this is what stops the
            // feet sliding and turns the "creep" into a real walk. Idle / standing
            // stays at 1x; locked states reset to 1x in ApplyMovementLockedFrame.
            float groundSpeed = new Vector2(CurrentVelocity.x, CurrentVelocity.z).magnitude;
            float targetAnimSpeed = (groundSpeed > 0.12f && IsGrounded)
                ? Mathf.Clamp(groundSpeed / Mathf.Max(0.25f, animationStrideSpeed), 0.7f, 2.4f)
                : 1f;
            animator.speed = Mathf.MoveTowards(animator.speed, targetAnimSpeed, 6f * Time.deltaTime);
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

        public void SetCameraReference(Transform t) => cameraReference = t;

        public void SetAnimator(Animator a) => animator = a;

#if UNITY_EDITOR
        [ContextMenu("Align Body to CC Bottom (Phase 28)")]
        private void EditorAlignBodyToCcBottom()
        {
            if (_cc == null) _cc = GetComponent<CharacterController>();
            _resolvedBody = ResolveBodyTransform();
            AlignBodyToCcBottom();
        }

        private void OnDrawGizmosSelected()
        {
            var origin = interactOrigin != null ? interactOrigin : transform;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(origin.position + origin.forward * (interactRange * 0.5f), interactRadius + interactRange * 0.5f);

            // Visual debug — CC capsule bottom (green disk).
            var cc = GetComponent<CharacterController>();
            if (cc != null)
            {
                float yc = transform.position.y + cc.center.y - cc.height * 0.5f;
                Gizmos.color = new Color(0.4f, 1f, 0.4f, 0.85f);
                DrawDisk(new Vector3(transform.position.x, yc, transform.position.z), cc.radius + 0.05f);
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
