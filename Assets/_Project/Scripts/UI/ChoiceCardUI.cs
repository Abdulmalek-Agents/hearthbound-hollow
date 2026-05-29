// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / ChoiceCardUI
//
// The 4-option moral choice card (Focus 02 § 5): Erase / Cleanse / Listen /
// Defer. Each option shows a tariff preview (coin + vow integrity arrows).
//
// ── Phase 60 — Arabic Localization MVP ──────────────────────────
// Prompt + memory title + per-tile label + cost preview look up
// LocalizationService.HasKey on each string — if the string matches a
// registered key (e.g. "choice.option_a.label") it's resolved to the
// localized form; otherwise the English source falls through unchanged.
// Tile alignment mirrors on RTL.

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HearthboundHollow.Core;
using HearthboundHollow.Memory;

namespace HearthboundHollow.UI
{
    public class ChoiceCardUI : MonoBehaviour
    {
        [Header("Root")]
        public GameObject root;

        [Header("Header")]
        public TextMeshProUGUI promptLine;
        public TextMeshProUGUI memoryTitle;

        [Header("Choices")]
        public Transform choiceContainer;
        public GameObject choiceTilePrefab;

        public event Action<MoralChoice> OnChoiceConfirmed;

        private MemoryNodeSO _currentMemory;
        private readonly List<GameObject> _spawned = new();

        private void Awake()
        {
            if (root != null && root != gameObject) root.SetActive(false);

            UIAutoFitText.ApplyToLabel(promptLine, minSize: 14, maxSize: 24);
            UIAutoFitText.ApplyToButtonLabel(memoryTitle, minSize: 18, maxSize: 32);

            DialogueChoiceLayoutHealer.HealContainer(choiceContainer);
        }

        public void Show(MemoryNodeSO memory, string promptText, IReadOnlyList<TariffSO> tariffs)
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true);

            _currentMemory = memory;
            // Phase 60 — Resolve loc once per Show. Caller may pass a raw
            // English string or a localization key — we transparently look
            // up the key form if it matches our table.
            var loc = ServiceLocator.Get<LocalizationService>();
            bool rtl = loc != null && loc.IsRightToLeft;

            if (root != null) root.SetActive(true);
            if (promptLine != null)
            {
                string s = promptText ?? string.Empty;
                if (loc != null && loc.HasKey(s)) s = loc.Get(s);
                promptLine.text = rtl ? ArabicTextShaper.Shape(s) : s;
                promptLine.isRightToLeftText = rtl;
                promptLine.alignment = rtl ? TextAlignmentOptions.TopRight : TextAlignmentOptions.TopLeft;
            }
            if (memoryTitle != null && memory != null)
            {
                string s = memory.title ?? string.Empty;
                if (loc != null && loc.HasKey(s)) s = loc.Get(s);
                memoryTitle.text = rtl ? ArabicTextShaper.Shape(s) : s;
                memoryTitle.isRightToLeftText = rtl;
            }

            ClearTiles();
            if (choiceContainer == null || choiceTilePrefab == null) return;

            DialogueChoiceLayoutHealer.HealContainer(choiceContainer);

            foreach (var tariff in tariffs)
            {
                if (tariff == null) continue;
                var go = Instantiate(choiceTilePrefab, choiceContainer);
                go.SetActive(true);
                _spawned.Add(go);
                WireTile(go, tariff);
                DialogueChoiceLayoutHealer.HealTile(go);
            }
        }

        public void Hide()
        {
            if (root != null && root != gameObject) root.SetActive(false);
            ClearTiles();
        }

        private void WireTile(GameObject tile, TariffSO tariff)
        {
            var label = tile.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            var cost = tile.transform.Find("CostPreview")?.GetComponent<TextMeshProUGUI>();
            var icon = tile.transform.Find("Icon")?.GetComponent<Image>();
            var btn = tile.GetComponent<Button>();

            // Phase 60 — Localized tile labels.
            // TariffSO authors register a stable localization key as
            // `displayLabel` / `costPreviewProse` when they want translation;
            // otherwise the English source string falls through unchanged.
            var loc = ServiceLocator.Get<LocalizationService>();
            bool rtl = loc != null && loc.IsRightToLeft;

            if (label != null)
            {
                string s = tariff.displayLabel ?? string.Empty;
                if (loc != null && loc.HasKey(s)) s = loc.Get(s);
                label.text = rtl ? ArabicTextShaper.Shape(s) : s;
                label.isRightToLeftText = rtl;
                label.alignment = rtl ? TextAlignmentOptions.MidlineRight : TextAlignmentOptions.MidlineLeft;
            }
            if (cost != null)
            {
                string s = tariff.costPreviewProse ?? string.Empty;
                if (loc != null && loc.HasKey(s)) s = loc.Get(s);
                cost.text = rtl ? ArabicTextShaper.Shape(s) : s;
                cost.isRightToLeftText = rtl;
                cost.alignment = rtl ? TextAlignmentOptions.MidlineRight : TextAlignmentOptions.MidlineLeft;
            }
            if (icon != null) { icon.sprite = tariff.choiceIcon; icon.color = tariff.choiceColor; }
            if (btn != null) btn.onClick.AddListener(() => Confirm(tariff.choice));

            UIAutoFitText.ApplyToLabel(label, minSize: 14, maxSize: 22);
            UIAutoFitText.ApplyToLabel(cost,  minSize: 12, maxSize: 18);
        }

        private void Confirm(MoralChoice choice)
        {
            Hh.Log(LogCategory.UI, $"Player confirmed moral choice: {choice} on '{(_currentMemory != null ? _currentMemory.title : "<null>")}'");
            Hide();
            EventBus.Publish(new MoralChoiceMadeEvent(_currentMemory, (int)choice));
            OnChoiceConfirmed?.Invoke(choice);
        }

        private void ClearTiles()
        {
            foreach (var g in _spawned) if (g != null) Destroy(g);
            _spawned.Clear();
        }
    }
}
