// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / ChoiceCardUI
//
// The 4-option moral choice card (Focus 02 § 5): Erase / Cleanse / Listen /
// Defer. Each option shows a tariff preview (coin + vow integrity arrows).
//
// ── Phase 25 hotfix ─────────────────────────────────────────────
// Show() now self-heals (activates own GameObject if dormant) and the
// inactive-panel pattern is preserved. Tile prefab activation is also
// defensive — Instantiate of an inactive template should produce an
// active clone for display.

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

            // Phase 29 — force word-wrap + auto-size on header labels so the
            // moral-choice card never clips long memory titles or the prompt.
            UIAutoFitText.ApplyToLabel(promptLine, minSize: 14, maxSize: 24);
            UIAutoFitText.ApplyToButtonLabel(memoryTitle, minSize: 18, maxSize: 32);
        }

        public void Show(MemoryNodeSO memory, string promptText, IReadOnlyList<TariffSO> tariffs)
        {
            // Self-heal — keep the host alive so coroutines (if any) and tile
            // raycasts both work.
            if (!gameObject.activeSelf) gameObject.SetActive(true);

            _currentMemory = memory;
            if (root != null) root.SetActive(true);
            if (promptLine != null) promptLine.text = promptText;
            if (memoryTitle != null && memory != null) memoryTitle.text = memory.title;

            ClearTiles();
            if (choiceContainer == null || choiceTilePrefab == null) return;
            foreach (var tariff in tariffs)
            {
                if (tariff == null) continue;
                var go = Instantiate(choiceTilePrefab, choiceContainer);
                go.SetActive(true); // template prefab may be inactive
                _spawned.Add(go);
                WireTile(go, tariff);
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
            if (label != null) label.text = tariff.displayLabel;
            if (cost != null) cost.text = tariff.costPreviewProse;
            if (icon != null) { icon.sprite = tariff.choiceIcon; icon.color = tariff.choiceColor; }
            if (btn != null) btn.onClick.AddListener(() => Confirm(tariff.choice));

            // Phase 29 — defensive autofit on instantiated tile labels so the
            // tariff displayLabel + costPreviewProse can never overflow.
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
