// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / EveningLedgerUI
//
// The end-of-day summary panel. Shows accomplishments, memories held, coin
// balance, and the 3 Save Slot rows + Autosave. Closing it advances the day.
//
// ── Phase 60 — Arabic Localization MVP ──────────────────────────
// Day title + coin balance use ledger.title_fmt / ledger.coin_fmt; bullet
// list uses ledger.bullet_fmt; summary prose is shaped for RTL by the
// caller (it's localized at the Mission director level).

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

            UIReadabilityHelper.ApplyHeadline (dayLabel,        min: 56, max: 110);
            UIReadabilityHelper.ApplyMonetary (coinLabel,       min: 28, max: 56);
            UIReadabilityHelper.ApplyBody     (summaryProse,    min: 26, max: 38);
            UIReadabilityHelper.ApplyBody     (heldMemoriesList, min: 24, max: 34);
            UIReadabilityHelper.ApplyButtonLabel(saveSlot1Label, min: 20, max: 30);
            UIReadabilityHelper.ApplyButtonLabel(saveSlot2Label, min: 20, max: 30);
            UIReadabilityHelper.ApplyButtonLabel(saveSlot3Label, min: 20, max: 30);

            if (summaryProse != null)     UIReadabilityHelper.AddDarkWash(summaryProse.rectTransform, padding: 18f);
            if (heldMemoriesList != null) UIReadabilityHelper.AddDarkWash(heldMemoriesList.rectTransform, padding: 18f);
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
            var loc = ServiceLocator.Get<LocalizationService>();
            bool rtl = loc != null && loc.IsRightToLeft;
            if (root != null) root.SetActive(true);

            // Phase 60 — Localized labels. Falls back to English on missing
            // service or missing key.
            if (dayLabel != null && vs != null)
            {
                string s = loc != null
                    ? loc.Format("ledger.title_fmt", vs.currentDayIndex)
                    : $"Day {vs.currentDayIndex}";
                dayLabel.text = rtl ? ArabicTextShaper.Shape(s) : s;
                dayLabel.isRightToLeftText = rtl;
            }
            if (coinLabel != null && vs != null)
            {
                string s = loc != null
                    ? loc.Format("ledger.coin_fmt", vs.coin)
                    : $"{vs.coin} c";
                coinLabel.text = rtl ? ArabicTextShaper.Shape(s) : s;
                coinLabel.isRightToLeftText = rtl;
            }
            if (summaryProse != null)
            {
                // The summary string comes localized from the Mission director
                // (or falls back to English) — we still shape it for RTL display.
                summaryProse.text = rtl ? ArabicTextShaper.Shape(summary ?? string.Empty) : (summary ?? string.Empty);
                summaryProse.isRightToLeftText = rtl;
                summaryProse.alignment = rtl ? TMPro.TextAlignmentOptions.TopRight : TMPro.TextAlignmentOptions.TopLeft;
            }
            if (heldMemoriesList != null)
            {
                var sb = new System.Text.StringBuilder();
                string bulletFmt = loc != null ? loc.Get("ledger.bullet_fmt") : "• {0}";
                foreach (var t in heldMemoryTitles)
                {
                    string line;
                    try { line = string.Format(bulletFmt, t); }
                    catch { line = "• " + t; }
                    sb.AppendLine(line);
                }
                string final = sb.ToString();
                heldMemoriesList.text = rtl ? ArabicTextShaper.Shape(final) : final;
                heldMemoriesList.isRightToLeftText = rtl;
                heldMemoriesList.alignment = rtl ? TMPro.TextAlignmentOptions.TopRight : TMPro.TextAlignmentOptions.TopLeft;
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
