// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / SystemMenuUI
//
// Phase 53 — the "polish" settings additions the player asked for, usable from
// both the Main Menu settings panel and the in-game Pause menu:
//   • Language selector (English / العربية) — applies live via LocalizationService.
//   • Customize Character — re-opens the character creator (event).
//   • Reset Game — wipes saved progress and returns to the title. Guarded by an
//     in-panel confirm (no editor dialogs at runtime), Cozy-Contract gentle copy.
//
// Presentational: language is handled here (UI → Core); reset + customize are
// raised as events so a Mission-asmdef coordinator can touch Save + scene loads
// without dragging Save into the UI asmdef.

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HearthboundHollow.Core;

namespace HearthboundHollow.UI
{
    public class SystemMenuUI : MonoBehaviour
    {
        [Header("Root")]
        public GameObject root;

        [Header("Language")]
        public Button englishButton;
        public Button arabicButton;
        public Color  selectedTint = new Color(0.97f, 0.85f, 0.50f, 1f);
        public Color  normalTint   = new Color(0.94f, 0.86f, 0.66f, 1f);

        [Header("Actions")]
        public Button customizeButton;
        public Button resetButton;
        public Button closeButton;

        [Header("Reset confirm (in-panel)")]
        public GameObject resetConfirmPanel;   // shown on first Reset click
        public Button     resetConfirmYes;
        public Button     resetConfirmNo;

        /// <summary>Raised when the player confirms Reset Game.</summary>
        public event Action OnResetConfirmed;
        /// <summary>Raised when the player taps Customize Character.</summary>
        public event Action OnCustomizeRequested;

        private void Awake()
        {
            if (englishButton != null) englishButton.onClick.AddListener(() => SetLanguage(GameLanguage.English));
            if (arabicButton  != null) arabicButton.onClick.AddListener(() => SetLanguage(GameLanguage.Arabic));
            if (customizeButton != null) customizeButton.onClick.AddListener(() => { Hide(); OnCustomizeRequested?.Invoke(); });

            if (resetButton != null) resetButton.onClick.AddListener(ShowResetConfirm);
            if (resetConfirmNo  != null) resetConfirmNo.onClick.AddListener(HideResetConfirm);
            if (resetConfirmYes != null) resetConfirmYes.onClick.AddListener(() =>
            {
                HideResetConfirm();
                Hide();
                Hh.Log(LogCategory.UI, "System menu → Reset Game confirmed.");
                OnResetConfirmed?.Invoke();
            });
            if (closeButton != null) closeButton.onClick.AddListener(Hide);

            if (root != null && root != gameObject) root.SetActive(false);
            HideResetConfirm();
        }

        public void Show()
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true);  // self-heal
            if (root != null) root.SetActive(true);
            HideResetConfirm();
            RefreshLanguageHighlight();
        }

        public void Hide()
        {
            if (root != null && root != gameObject) root.SetActive(false);
        }

        private void SetLanguage(GameLanguage lang)
        {
            LocalizationService.SetLanguage(lang);
            RefreshLanguageHighlight();
        }

        private void RefreshLanguageHighlight()
        {
            bool ar = LocalizationService.IsRightToLeft;
            Tint(englishButton, !ar);
            Tint(arabicButton, ar);
        }

        private void Tint(Button b, bool selected)
        {
            if (b == null) return;
            var img = b.GetComponent<Image>();
            if (img != null) img.color = selected ? selectedTint : normalTint;
        }

        private void ShowResetConfirm() { if (resetConfirmPanel != null) resetConfirmPanel.SetActive(true); }
        private void HideResetConfirm() { if (resetConfirmPanel != null) resetConfirmPanel.SetActive(false); }
    }
}
