// =============================================================================
// Interactable.cs — Hearthbound Hollow
// Base class for everything the player can press E on.
// Phase 76 — 30-Mission Architecture.
// =============================================================================
using UnityEngine;

namespace HearthboundHollow.Core
{
    /// <summary>
    /// Override <see cref="OnActivate"/> to respond when the player presses Interact.
    /// Override <see cref="GetPromptText"/> to customise the HUD chip label.
    /// Optionally set <see cref="interactionRange"/> for trigger detection radius.
    /// </summary>
    public abstract class Interactable : MonoBehaviour
    {
        [Header("Interactable Settings")]
        [Tooltip("Shown on the ControlHintsHUD E-chip when in range.")]
        [SerializeField] protected string promptText = "Interact";

        [Tooltip("Trigger radius used by PlayerController to detect the closest interactable.")]
        [SerializeField] protected float interactionRange = 1.8f;

        [Tooltip("If false the player cannot interact (e.g. locked-state or cutscene running).")]
        [SerializeField] protected bool isInteractable = true;

        // ─── Overrideable API ─────────────────────────────────────────────────

        /// <summary>Called by PlayerController when the player presses Interact (E / Gamepad □).</summary>
        public abstract void OnActivate();

        /// <summary>Text shown on the HUD E-chip. Override for dynamic text.</summary>
        public virtual string GetPromptText() => promptText;

        /// <summary>Whether interaction is currently allowed.</summary>
        public virtual bool IsInteractable => isInteractable;

        // ─── Range helper ─────────────────────────────────────────────────────

        public float InteractionRange => interactionRange;

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
#endif
    }
}
