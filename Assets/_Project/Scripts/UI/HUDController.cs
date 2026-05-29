// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / HUDController
//
// Minimal in-mission HUD. Renders day-of-week, coin balance, and the held
// memory icon if the player is carrying one.
//
// ── Phase 60 — Arabic Localization MVP ──────────────────────────
// Day + coin labels are localized via hud.day_label_fmt / hud.coin_label_fmt
// (Western digits per Codex 17 RTL conventions — they read cleanly on
// the small HUD chip even in Arabic).

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
            EventBus.Subscribe<LocaleChangedEvent>(OnLocaleChanged);
            Refresh();
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<MemoryPolishedEvent>(OnPolished);
            EventBus.Unsubscribe<DayEndedEvent>(OnDayEnded);
            EventBus.Unsubscribe<LocaleChangedEvent>(OnLocaleChanged);
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
            var loc = ServiceLocator.Get<LocalizationService>();
            bool rtl = loc != null && loc.IsRightToLeft;

            if (vs != null)
            {
                // Phase 60 — Localized HUD day + coin labels.
                if (dayLabel != null)
                {
                    string s = loc != null
                        ? loc.Format("hud.day_label_fmt", vs.currentDayIndex)
                        : $"Day {vs.currentDayIndex}";
                    dayLabel.text = rtl ? ArabicTextShaper.Shape(s) : s;
                    dayLabel.isRightToLeftText = rtl;
                }
                if (coinLabel != null)
                {
                    string s = loc != null
                        ? loc.Format("hud.coin_label_fmt", vs.coin)
                        : $"{vs.coin} c";
                    coinLabel.text = rtl ? ArabicTextShaper.Shape(s) : s;
                    coinLabel.isRightToLeftText = rtl;
                }
            }
            if (heldMemoryGroup != null) heldMemoryGroup.SetActive(_held != null);
            if (heldMemoryIcon != null && _held != null)
            {
                heldMemoryIcon.sprite = _held.setpieceThumbnail;
                heldMemoryIcon.color = _held.EffectiveTint;
            }
        }

        // Phase 60 — refresh when the player switches language mid-mission.
        private void OnLocaleChanged(LocaleChangedEvent _) => Refresh();
    }
}
