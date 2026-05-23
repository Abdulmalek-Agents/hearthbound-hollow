// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Player / PlayerController
//
// The walk-only player controller. Wraps Lightbug's Character Controller Pro
// (Normal state only — Jump/Dash/Climb disabled per Asset Analysis § A-13).

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using HearthboundHollow.Core;

namespace HearthboundHollow.Player
{
    [DefaultExecutionOrder(-50)]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 2.4f;
        [SerializeField] private float turnLerp = 12f;

        [Header("Interaction raycast")]
        [SerializeField] private float interactRange = 2.2f;
        [SerializeField] private float interactRadius = 0.45f;
        [SerializeField] private LayerMask interactMask = ~0;
        [SerializeField] private Transform interactOrigin;

        [Header("Animator")]
        [SerializeField] private Animator animator;
        [SerializeField] private string animSpeedParam = "Speed";

        [Header("Input (optional)")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference interactAction;

        public Interactable CurrentFocus { get; private set; }
        public Vector3 CurrentMoveInput { get; private set; }
        public bool MovementLocked { get; set; } = false;

        private static readonly Collider[] _hits = new Collider[8];
        private readonly List<Interactable> _candidates = new(4);
        private CharacterController _fallbackController;
        private int _animSpeedHash;

        private void Awake()
        {
            if (interactOrigin == null) interactOrigin = transform;
            _animSpeedHash = Animator.StringToHash(animSpeedParam);
            _fallbackController = GetComponent<CharacterController>();
        }

        private void OnEnable()
        {
            if (moveAction != null && moveAction.action != null) moveAction.action.Enable();
            if (interactAction != null && interactAction.action != null)
            {
                interactAction.action.performed += OnInteractPerformed;
                interactAction.action.Enable();
            }
        }

        private void OnDisable()
        {
            if (interactAction != null && interactAction.action != null)
                interactAction.action.performed -= OnInteractPerformed;
        }

        private void Update()
        {
            if (MovementLocked)
            {
                CurrentMoveInput = Vector3.zero;
                if (animator != null) animator.SetFloat(_animSpeedHash, 0f, 0.1f, Time.deltaTime);
                ScanForInteractable();
                return;
            }

            Vector2 raw = ReadMoveInput();
            CurrentMoveInput = new Vector3(raw.x, 0f, raw.y);

            if (_fallbackController != null && _fallbackController.enabled)
                _fallbackController.SimpleMove(CurrentMoveInput * walkSpeed);
            else
                transform.position += CurrentMoveInput * walkSpeed * Time.deltaTime;

            if (CurrentMoveInput.sqrMagnitude > 0.01f)
            {
                var target = Quaternion.LookRotation(CurrentMoveInput, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, target, turnLerp * Time.deltaTime);
            }

            if (animator != null)
                animator.SetFloat(_animSpeedHash, CurrentMoveInput.magnitude, 0.1f, Time.deltaTime);

            ScanForInteractable();
        }

        private Vector2 ReadMoveInput()
        {
            if (moveAction != null && moveAction.action != null && moveAction.action.enabled)
                return moveAction.action.ReadValue<Vector2>();
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }

        private void OnInteractPerformed(InputAction.CallbackContext _) => TryActivateFocus();

        public void TryActivateFocus()
        {
            if (CurrentFocus == null || !CurrentFocus.IsInteractable) return;
            Hh.Log(LogCategory.Input, $"Player activated '{CurrentFocus.PromptText}' on {CurrentFocus.name}");
            CurrentFocus.Activate(gameObject);
        }

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
