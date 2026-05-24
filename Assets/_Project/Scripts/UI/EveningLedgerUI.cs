// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / EveningLedgerUI
//
// The end-of-day summary panel. Shows accomplishments, memories held, coin
// balance, and the 3 Save Slot rows + Autosave. Closing it advances the day.
//
// ── Phase 25 hotfix ─────────────────────────────────────────────
// Show() now self-heals (activates own GameObject if dormant) — matches
// the rest of the UI hotfix family.

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HearthboundHollow.Core;

namespace HearthboundHollow.UI
{
    public class EveningLedgerUI : MonoBehaviour
    {
        [Header("Root")]
        public GameObject root;

        [Header("Header")]
        public TextMeshProUGUI dayLabel;
        public TextMeshProUGUI coinLabel;

        [Header("Summary")]
        public TextMeshProUGUI summaryProse;
        public TextMeshProUGUI heldMemoriesList;

        [Header("Save buttons")]
        public Button saveSlot1;
        public Button saveSlot2;
        public Button saveSlot3;
        public Button autosaveButton;
        public TextMeshProUGUI saveSlot1Label;
        public TextMeshProUGUI saveSlot2Label;
        public TextMeshProUGUI saveSlot3Label;

        [Header("Close")]
        public Button confirmEndOfDayButton;

        public event System.Action<int> OnSaveSlotChosen;     // -1 = autosave
        public event System.Action OnEndOfDayConfirmed;

        private void Awake()
        {
            if (root != null && root != gameObject) root.SetActive(false);
            WireButtons();
        }

        private void WireButtons()
        {
            if (saveSlot1 != null) saveSlot1.onClick.AddListener(() => SaveSlotPressed(0));
            if (saveSlot2 != null) saveSlot2.onClick.AddListener(() => SaveSlotPressed(1));
            if (saveSlot3 != null) saveSlot3.onClick.AddListener(() => SaveSlotPressed(2));
            if (autosaveButton != null) autosaveButton.onClick.AddListener(() => SaveSlotPressed(-1));
            if (confirmEndOfDayButton != null) confirmEndOfDayButton.onClick.AddListener(() =>
            {
                Hide();
                OnEndOfDayConfirmed?.Invoke();
            });
        }

        public void Show(string summary, IReadOnlyList<string> heldMemoryTitles)
        {
            // Self-heal.
            if (!gameObject.activeSelf) gameObject.SetActive(true);

            var vs = ServiceLocator.Get<VillageState>();
            if (root != null) root.SetActive(true);
            if (dayLabel != null && vs != null) dayLabel.text = $"Day {vs.currentDayIndex}";
            if (coinLabel != null && vs != null) coinLabel.text = $"{vs.coin} c";
            if (summaryProse != null) summaryProse.text = summary;
            if (heldMemoriesList != null)
            {
                var sb = new System.Text.StringBuilder();
                foreach (var t in heldMemoryTitles) sb.AppendLine($"• {t}");
                heldMemoriesList.text = sb.ToString();
            }
        }

        public void SetSlotLabel(int slot, string label)
        {
            switch (slot)
            {
                case 0: if (saveSlot1Label != null) saveSlot1Label.text = label; break;
                case 1: if (saveSlot2Label != null) saveSlot2Label.text = label; break;
                case 2: if (saveSlot3Label != null) saveSlot3Label.text = label; break;
            }
        }

        public void Hide()
        {
            if (root != null && root != gameObject) root.SetActive(false);
        }

        private void SaveSlotPressed(int slot)
        {
            Hh.Log(LogCategory.Save, $"Player chose save slot {slot}.");
            OnSaveSlotChosen?.Invoke(slot);
        }
    }
}
