// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Player / InteractionPromptUI
//
// Worldspace icon + label that follows the player's CurrentFocus Interactable.
// Mirrors the worldspace prompt convention used in cozy games.

using TMPro;
using UnityEngine;

namespace HearthboundHollow.Player
{
    public class InteractionPromptUI : MonoBehaviour
    {
        [Header("References")]
        public PlayerController player;
        public Canvas worldCanvas;
        public RectTransform promptRoot;
        public TextMeshProUGUI promptLabel;
        public Camera worldCamera;

        [Header("Tuning")]
        public Vector3 worldOffset = new Vector3(0f, 0.4f, 0f);

        private void LateUpdate()
        {
            if (player == null || worldCamera == null || promptRoot == null) return;
            var focus = player.CurrentFocus;
            bool show = focus != null && focus.IsInteractable;
            if (promptRoot.gameObject.activeSelf != show) promptRoot.gameObject.SetActive(show);
            if (!show) return;

            var world = focus.PromptWorldPosition + worldOffset;
            var screen = worldCamera.WorldToScreenPoint(world);
            if (screen.z < 0f) { promptRoot.gameObject.SetActive(false); return; }
            promptRoot.position = screen;

            if (promptLabel != null) promptLabel.text = focus.GetDynamicPromptText();
        }
    }
}
