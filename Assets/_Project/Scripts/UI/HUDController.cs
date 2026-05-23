// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / HUDController
//
// Minimal in-mission HUD. Renders day-of-week, coin balance, and the held
// memory icon if the player is carrying one.

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HearthboundHollow.Core;
using HearthboundHollow.Memory;

namespace HearthboundHollow.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("References")]
        public TextMeshProUGUI dayLabel;
        public TextMeshProUGUI coinLabel;
        public Image heldMemoryIcon;
        public GameObject heldMemoryGroup;

        private MemoryNodeSO _held;

        private void OnEnable()
        {
            EventBus.Subscribe<MemoryPolishedEvent>(OnPolished);
            EventBus.Subscribe<DayEndedEvent>(OnDayEnded);
            Refresh();
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<MemoryPolishedEvent>(OnPolished);
            EventBus.Unsubscribe<DayEndedEvent>(OnDayEnded);
        }

        private void OnDayEnded(DayEndedEvent _) => Refresh();

        private void OnPolished(MemoryPolishedEvent evt)
        {
            _held = evt.Memory as MemoryNodeSO;
            Refresh();
        }

        public void SetHeldMemory(MemoryNodeSO m)
        {
            _held = m;
            Refresh();
        }

        private void Refresh()
        {
            var vs = ServiceLocator.Get<VillageState>();
            if (vs != null)
            {
                if (dayLabel != null) dayLabel.text = $"Day {vs.currentDayIndex}";
                if (coinLabel != null) coinLabel.text = $"{vs.coin} c";
            }
            if (heldMemoryGroup != null) heldMemoryGroup.SetActive(_held != null);
            if (heldMemoryIcon != null && _held != null)
            {
                heldMemoryIcon.sprite = _held.setpieceThumbnail;
                heldMemoryIcon.color = _held.EffectiveTint;
            }
        }
    }
}
