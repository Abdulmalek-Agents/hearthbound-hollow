// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / ComfortToolsMenu
//
// The Comfort Tools modal. Surfaces Auto-Complete toggles, Gentle Mode,
// color-blind palette, subtitle size, one-hand control. Per Codex 06.

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HearthboundHollow.Core;

namespace HearthboundHollow.UI
{
    public class ComfortToolsMenu : MonoBehaviour
    {
        [Header("Root")]
        public GameObject root;

        [Header("Toggles")]
        public Toggle gentleMode;
        public Toggle autoCompletePolish;
        public Toggle autoCompleteCleanse;
        public Toggle oneHandMode;

        [Header("Sliders")]
        [Tooltip("0 = small, 1 = medium, 2 = large, 3 = huge")]
        public Slider subtitleSize;
        public TextMeshProUGUI subtitleSizeLabel;

        [Header("Color palette")]
        public Button paletteDefault;
        public Button paletteProtanopia;
        public Button paletteDeuteranopia;
        public Button paletteTritanopia;

        public bool AutoCompletePolish { get; private set; } = false;
        public bool AutoCompleteCleanse { get; private set; } = false;
        public bool OneHandMode { get; private set; } = false;
        public int SubtitleSizeTier { get; private set; } = 1;
        public string ColorPaletteId { get; private set; } = "default";

        private void Awake()
        {
            if (root != null) root.SetActive(false);
            if (gentleMode != null) gentleMode.onValueChanged.AddListener(OnGentleMode);
            if (autoCompletePolish != null) autoCompletePolish.onValueChanged.AddListener(v => AutoCompletePolish = v);
            if (autoCompleteCleanse != null) autoCompleteCleanse.onValueChanged.AddListener(v => AutoCompleteCleanse = v);
            if (oneHandMode != null) oneHandMode.onValueChanged.AddListener(v => OneHandMode = v);
            if (subtitleSize != null) subtitleSize.onValueChanged.AddListener(OnSubtitleSize);
            if (paletteDefault != null) paletteDefault.onClick.AddListener(() => ColorPaletteId = "default");
            if (paletteProtanopia != null) paletteProtanopia.onClick.AddListener(() => ColorPaletteId = "protanopia");
            if (paletteDeuteranopia != null) paletteDeuteranopia.onClick.AddListener(() => ColorPaletteId = "deuteranopia");
            if (paletteTritanopia != null) paletteTritanopia.onClick.AddListener(() => ColorPaletteId = "tritanopia");
        }

        public void Show()
        {
            if (root != null) root.SetActive(true);
            var vs = ServiceLocator.Get<VillageState>();
            if (vs != null && gentleMode != null) gentleMode.isOn = vs.gentleModeEnabled;
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
        }

        private void OnGentleMode(bool v)
        {
            var vs = ServiceLocator.Get<VillageState>();
            if (vs != null) vs.gentleModeEnabled = v;
            Hh.Log(LogCategory.UI, $"Gentle Mode set to {v}.");
        }

        private void OnSubtitleSize(float v)
        {
            SubtitleSizeTier = Mathf.Clamp((int)v, 0, 3);
            if (subtitleSizeLabel != null)
                subtitleSizeLabel.text = SubtitleSizeTier switch
                {
                    0 => "Small", 1 => "Medium", 2 => "Large", _ => "Huge",
                };
        }
    }
}
