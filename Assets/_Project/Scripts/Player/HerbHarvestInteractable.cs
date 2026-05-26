// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Player / HerbHarvestInteractable
//
// A plant in the garden you can harvest. Reads its MemoryHerb SO, publishes a
// HerbHarvestedEvent on activation, hides the visual until the next day cycle.

using UnityEngine;
using HearthboundHollow.Core;
using HearthboundHollow.Memory;

namespace HearthboundHollow.Player
{
    public class HerbHarvestInteractable : Interactable
    {
        [Header("Herb")]
        public MemoryHerb herb;

        [Header("Visual")]
        public GameObject readyVisual;
        public GameObject harvestedVisual;

        public bool IsReady => herb != null && readyVisual != null && readyVisual.activeSelf;

        private void Awake() => SetReady(true);

        public override string GetDynamicPromptText()
        {
            if (herb == null) return "Harvest";
            if (!IsReady) return $"{herb.displayName} (gone)";
            return $"Harvest {herb.displayName}";
        }

        public override void Activate(GameObject player)
        {
            if (!IsReady || herb == null) return;
            SetReady(false);
            Hh.Log(LogCategory.Memory, $"Player harvested {herb.herbId}.");
            EventBus.Publish(new HerbHarvestedEvent(herb));
            var vs = ServiceLocator.Get<VillageState>();
            if (vs != null && !vs.harvestedHerbIds.Contains(herb.herbId))
                vs.harvestedHerbIds.Add(herb.herbId);
        }

        private void SetReady(bool ready)
        {
            if (readyVisual != null) readyVisual.SetActive(ready);
            if (harvestedVisual != null) harvestedVisual.SetActive(!ready);
            SetInteractable(ready);
        }
    }
}
