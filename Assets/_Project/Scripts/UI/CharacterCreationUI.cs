// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / CharacterCreationUI
//
// Phase 53 — the cozy "who keeps the Hollow?" character creator shown on a
// fresh New Game (and re-openable from Settings → Customize Character).
// Player picks skin tone, outfit colour, an accessory, and a name. Choices
// persist to SettingsService (PlayerPrefs); CharacterAppearanceApplier reads
// them in every gameplay scene.
//
// Presentational + palette-agnostic: the builder creates and COLOURS the
// swatch buttons (from Mission/CharacterPalette). This script only tracks the
// selected indices, drives a live preview, writes SettingsService, and raises
// OnConfirmed. UI → Core only (no Mission dependency, no asmdef cycle).

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HearthboundHollow.Core;

namespace HearthboundHollow.UI
{
    public class CharacterCreationUI : MonoBehaviour
    {
        [Header("Root")]
        public GameObject root;
        public CanvasGroup canvasGroup;

        [Header("Swatch rows (created + coloured by the builder)")]
        public Button[] skinSwatches;
        public Button[] outfitSwatches;
        public Button[] accessoryButtons;

        [Header("Preview + fields")]
        public Image previewOutfit;     // big swatch echoing the chosen outfit
        public Image previewSkin;       // small swatch echoing the chosen skin
        public TMP_InputField nameInput;
        public Button beginButton;
        public Button randomButton;

        [Header("Selection highlight")]
        public Color selectedOutline = new Color(0.97f, 0.85f, 0.50f, 1f);

        public event Action OnConfirmed;

        private int _skin, _outfit, _accessory;

        private void Awake()
        {
            WireSwatches(skinSwatches, i => { _skin = i; RefreshPreview(); HighlightRow(skinSwatches, i); });
            WireSwatches(outfitSwatches, i => { _outfit = i; RefreshPreview(); HighlightRow(outfitSwatches, i); });
            WireSwatches(accessoryButtons, i => { _accessory = i; HighlightRow(accessoryButtons, i); });

            if (randomButton != null) randomButton.onClick.AddListener(Randomize);
            if (beginButton != null)  beginButton.onClick.AddListener(Confirm);

            if (root != null && root != gameObject) root.SetActive(false);
        }

        private void WireSwatches(Button[] row, Action<int> onPick)
        {
            if (row == null) return;
            for (int i = 0; i < row.Length; i++)
            {
                int idx = i;
                if (row[i] != null) row[i].onClick.AddListener(() => onPick(idx));
            }
        }

        /// <summary>Open the creator, seeding from any previously-saved choices.</summary>
        public void Show()
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true);   // self-heal
            if (root != null) root.SetActive(true);

            var s = ServiceLocator.Get<SettingsService>();
            _skin      = s != null ? s.PlayerSkinTone  : SettingsService.DefaultSkinTone;
            _outfit    = s != null ? s.PlayerOutfit     : SettingsService.DefaultOutfit;
            _accessory = s != null ? s.PlayerAccessory  : SettingsService.DefaultAccessory;
            if (nameInput != null) nameInput.text = s != null ? s.PlayerName : SettingsService.DefaultPlayerName;

            HighlightRow(skinSwatches, _skin);
            HighlightRow(outfitSwatches, _outfit);
            HighlightRow(accessoryButtons, _accessory);
            RefreshPreview();

            if (canvasGroup != null) canvasGroup.alpha = 1f;
        }

        public void Hide()
        {
            if (root != null && root != gameObject) root.SetActive(false);
        }

        private void Randomize()
        {
            if (skinSwatches != null && skinSwatches.Length > 0)   _skin = UnityEngine.Random.Range(0, skinSwatches.Length);
            if (outfitSwatches != null && outfitSwatches.Length > 0) _outfit = UnityEngine.Random.Range(0, outfitSwatches.Length);
            if (accessoryButtons != null && accessoryButtons.Length > 0) _accessory = UnityEngine.Random.Range(0, accessoryButtons.Length);
            HighlightRow(skinSwatches, _skin);
            HighlightRow(outfitSwatches, _outfit);
            HighlightRow(accessoryButtons, _accessory);
            RefreshPreview();
        }

        private void Confirm()
        {
            var s = ServiceLocator.Get<SettingsService>();
            if (s != null)
            {
                s.PlayerSkinTone  = _skin;
                s.PlayerOutfit     = _outfit;
                s.PlayerAccessory  = _accessory;
                if (nameInput != null && !string.IsNullOrWhiteSpace(nameInput.text))
                    s.PlayerName = nameInput.text;
                s.CharacterCreated = true;
            }
            Hh.Log(LogCategory.UI, $"Character created — skin={_skin} outfit={_outfit} accessory={_accessory}.");
            Hide();
            OnConfirmed?.Invoke();
        }

        private void RefreshPreview()
        {
            if (previewOutfit != null && outfitSwatches != null && _outfit < outfitSwatches.Length && outfitSwatches[_outfit] != null)
            {
                var img = outfitSwatches[_outfit].GetComponent<Image>();
                if (img != null) previewOutfit.color = img.color;
            }
            if (previewSkin != null && skinSwatches != null && _skin < skinSwatches.Length && skinSwatches[_skin] != null)
            {
                var img = skinSwatches[_skin].GetComponent<Image>();
                if (img != null) previewSkin.color = img.color;
            }
        }

        private void HighlightRow(Button[] row, int selected)
        {
            if (row == null) return;
            for (int i = 0; i < row.Length; i++)
            {
                if (row[i] == null) continue;
                var outline = row[i].GetComponent<Outline>();
                if (outline == null) outline = row[i].gameObject.AddComponent<Outline>();
                bool on = i == selected;
                outline.effectColor = on ? selectedOutline : new Color(0, 0, 0, 0);
                outline.effectDistance = on ? new Vector2(4, 4) : Vector2.zero;
            }
        }
    }
}
