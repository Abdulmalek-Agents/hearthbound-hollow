// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Player / HollowDoorInteractable
//
// The Hollow's shop door. Interaction triggers a scene transition into the
// interior. In Mission 1 this is the first interactive moment after the
// opening lane walk.

using UnityEngine;
using HearthboundHollow.Core;

namespace HearthboundHollow.Player
{
    public class HollowDoorInteractable : Interactable
    {
        [Header("Door")]
        public string targetSceneName = "03_Mission01_Hollow";
        public string animTriggerName = "Open";
        public Animator doorAnimator;
        public float postAnimDelay = 0.6f;

        [Header("Required state")]
        [Tooltip("Optional Yarn variable name that must be true to allow entry.")]
        public string gateYarnVariable;

        public override string GetDynamicPromptText() => "Enter the Hollow";

        public override void Activate(GameObject player)
        {
            if (doorAnimator != null && !string.IsNullOrEmpty(animTriggerName))
                doorAnimator.SetTrigger(animTriggerName);
            SetInteractable(false);
            Invoke(nameof(LoadTargetScene), postAnimDelay);
        }

        private void LoadTargetScene()
        {
            if (string.IsNullOrEmpty(targetSceneName))
            {
                Hh.Warn(LogCategory.Mission, $"{name}: no target scene set on HollowDoorInteractable.");
                return;
            }
            var gm = GameManager.Instance;
            if (gm == null) { Hh.Err(LogCategory.Mission, "GameManager missing; cannot load scene."); return; }
            gm.LoadScene(targetSceneName);
        }
    }
}
