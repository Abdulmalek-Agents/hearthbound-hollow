// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Player / KettleInteractable
//
// The Hollow's tea kettle on the workbench. Interaction opens the TeaBrewingUI
// (Phase 5). Once tea is brewed for a given villager, the kettle resets after
// the dialogue completes.

using UnityEngine;
using HearthboundHollow.Core;

namespace HearthboundHollow.Player
{
    public class KettleInteractable : Interactable
    {
        [Header("Visuals")]
        public GameObject steamVfxObject;   // VoluSmokeFX prefab; null OK in M1-2

        public override string GetDynamicPromptText() => "Brew tea";

        public override void Activate(GameObject player)
        {
            Hh.Log(LogCategory.UI, "Kettle activated — opening TeaBrewingUI.");
            OnBrewingRequested?.Invoke(this);
        }

        public event System.Action<KettleInteractable> OnBrewingRequested;

        public void SetSteaming(bool steaming)
        {
            if (steamVfxObject != null) steamVfxObject.SetActive(steaming);
        }
    }
}
