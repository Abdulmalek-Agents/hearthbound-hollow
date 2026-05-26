// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Player / Interactable
//
// Base class for every interactable in the world: villagers, memory orbs,
// doors, herbs, kettles. The player's interaction raycast asks the nearest
// Interactable to display its prompt + accept Activate().

using UnityEngine;

namespace HearthboundHollow.Player
{
    [DisallowMultipleComponent]
    public abstract class Interactable : MonoBehaviour
    {
        [Header("Interactable")]
        [SerializeField] private string promptText = "Interact";
        [SerializeField] private float promptWorldOffsetY = 1.2f;
        [SerializeField] private bool enabledForInteraction = true;

        public string PromptText => promptText;
        public Vector3 PromptWorldPosition => transform.position + Vector3.up * promptWorldOffsetY;
        public bool IsInteractable => enabledForInteraction && isActiveAndEnabled;

        public void SetInteractable(bool value) => enabledForInteraction = value;

        public abstract void Activate(GameObject player);

        /// <summary>Optional: subclasses can override to provide a dynamic prompt.</summary>
        public virtual string GetDynamicPromptText() => promptText;
    }
}
