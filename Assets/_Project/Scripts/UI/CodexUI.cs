// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / CodexUI
//
// On-prop hover tooltip + the in-game Memory Map view. M1-2 only renders the
// 4-node Doris map and the first Echo connection.
//
// ── Phase 60 — Arabic Localization MVP ──────────────────────────
// Tooltips + memory map node labels look up LocalizationService.HasKey
// on each string — if the string matches a registered key it's resolved
// to the localized form; otherwise the English source falls through
// unchanged. Both surfaces are shaped for RTL.

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HearthboundHollow.Core;
using HearthboundHollow.Memory;

namespace HearthboundHollow.UI
{
    public class CodexUI : MonoBehaviour
    {
        [Header("Tooltip")]
        public GameObject tooltipRoot;
        public TextMeshProUGUI tooltipText;
        public Vector2 tooltipScreenOffset = new Vector2(24f, 24f);

        [Header("Memory Map")]
        public GameObject memoryMapRoot;
        public RectTransform memoryMapCanvas;
        public GameObject memoryMapNodePrefab;
        public GameObject memoryMapEdgePrefab;

        private VillagerMemoryMapSO _shownMap;
        private readonly List<GameObject> _spawned = new();

        private void Awake()
        {
            HideTooltip();
            HideMemoryMap();
        }

        public void ShowTooltip(string text, Vector2 screenPos)
        {
            if (tooltipRoot == null || tooltipText == null) return;
            tooltipRoot.SetActive(true);
            // Phase 60 — Treat text as a localization key if it matches an
            // entry in the table; otherwise pass through verbatim. Shape
            // for RTL if the active locale is Arabic.
            var loc = ServiceLocator.Get<LocalizationService>();
            if (loc != null)
            {
                if (loc.HasKey(text)) text = loc.Get(text);
                if (loc.IsRightToLeft) text = ArabicTextShaper.Shape(text);
                tooltipText.isRightToLeftText = loc.IsRightToLeft;
                tooltipText.alignment = loc.IsRightToLeft
                    ? TextAlignmentOptions.TopRight
                    : TextAlignmentOptions.TopLeft;
            }
            tooltipText.text = text;
            tooltipRoot.transform.position = screenPos + tooltipScreenOffset;
        }

        public void HideTooltip()
        {
            if (tooltipRoot != null) tooltipRoot.SetActive(false);
        }

        public void ShowMemoryMap(VillagerMemoryMapSO map, VillagerMemoryRuntime runtime)
        {
            _shownMap = map;
            if (memoryMapRoot != null) memoryMapRoot.SetActive(true);
            ClearMap();
            if (map == null || memoryMapCanvas == null || memoryMapNodePrefab == null) return;

            var loc = ServiceLocator.Get<LocalizationService>();
            bool rtl = loc != null && loc.IsRightToLeft;

            foreach (var n in map.nodes)
            {
                bool revealed = runtime != null && runtime.IsRevealed(n.memory) || n.revealedAtStart;
                var node = Instantiate(memoryMapNodePrefab, memoryMapCanvas);
                node.transform.localPosition = n.graphPosition;
                _spawned.Add(node);
                var label = node.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                {
                    string s = revealed && n.memory != null ? n.memory.title : "—";
                    if (loc != null && loc.HasKey(s)) s = loc.Get(s);
                    if (rtl) s = ArabicTextShaper.Shape(s);
                    label.text = s;
                    label.isRightToLeftText = rtl;
                }
                var img = node.GetComponentInChildren<Image>();
                if (img != null && n.memory != null)
                    img.color = revealed ? n.memory.EffectiveTint : new Color(1, 1, 1, 0.2f);
            }
        }

        public void HideMemoryMap()
        {
            if (memoryMapRoot != null) memoryMapRoot.SetActive(false);
            ClearMap();
        }

        public void ShowEcho(MemoryConnectionSO conn)
        {
            Hh.Log(LogCategory.UI, $"Echo revealed: {(conn != null ? conn.connectionId : "<null>")}");
        }

        private void ClearMap()
        {
            foreach (var g in _spawned) if (g != null) Destroy(g);
            _spawned.Clear();
        }
    }
}
