// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / EveningLedgerUI
//
// The end-of-day summary panel. Shows accomplishments, memories held, coin
// balance, and the 3 Save Slot rows + Autosave. Closing it advances the day.
//
// ── Phase 25 hotfix ─────────────────────────────────────────────
// Show() now self-heals (activates own GameObject if dormant) — matches
// the rest of the UI hotfix family.
//
// ── Phase 54 hotfix (D-069) — SHIP BLOCKER: end-of-day soft-lock ──
// QA video review ("Gameplay video testing.mp4", ~3:24) found the game
// freezes after pressing "Sleep — End Day": the ledger never disappears,
// the night beats (dream + goodnight card) play *behind* it, and the
// still-on-top panel keeps eating clicks so the player is stranded.
//
// ROOT CAUSE: in UI_EveningLedger_Bamao.prefab the `root` field points at
// the SAME GameObject the component lives on (root == gameObject). The old
// Hide() guard `root != null && root != gameObject` therefore made Hide() a
// silent no-op — the panel could be shown but never closed.
//
// FIX:
//   • Hide() now closes the panel even when root == gameObject (via a
//     CanvasGroup + self-deactivate fallback) and drops raycast-blocking so
//     it can never eat clicks while invisible.
//   • A single-fire `_confirmed` guard stops the "Sleep — End Day" button
//     re-entering the night chain if it is clicked again before the panel
//     tears down.

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

        [Header("Phase 54 — hide safety (D-069)")]
        [Tooltip("Optional CanvasGroup used to guarantee the panel can be fully " +
                 "hidden + made non-blocking even when 'root' is this same " +
                 "GameObject. Auto-found in Awake if left empty.")]
        public CanvasGroup canvasGroup;

        public event System.Action<int> OnSaveSlotChosen;     // -1 = autosave
        public event System.Action OnEndOfDayConfirmed;

        // Single-fire guard: once the player confirms end-of-day, the button
        // must not be able to re-enter the night chain (D-069).
        private bool _confirmed;

        private void Awake()
        {
            // Auto-discover a CanvasGroup so Hide() always has a way to fully
            // suppress the panel — even in the prefab's root==gameObject layout.
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null && root != null) canvasGroup = root.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

            if (root != null && root != gameObject) root.SetActive(false);
            else if (canvasGroup != null) { canvasGroup.alpha = 0f; canvasGroup.blocksRaycasts = false; canvasGroup.interactable = false; }
            WireButtons();

            // Phase 32.19 — high-contrast typography across the parchment.
            // Title-sized day banner, prose body for the summary, gold coin
            // count, bold cream-on-dark for buttons. Each helper internally
            // bounds autosize so the layout still adapts to canvas size.
            UIReadabilityHelper.ApplyHeadline (dayLabel,        min: 56, max: 110);
            UIReadabilityHelper.ApplyMonetary (coinLabel,       min: 28, max: 56);
            UIReadabilityHelper.ApplyBody     (summaryProse,    min: 26, max: 38);
            UIReadabilityHelper.ApplyBody     (heldMemoriesList, min: 24, max: 34);
            UIReadabilityHelper.ApplyButtonLabel(saveSlot1Label, min: 20, max: 30);
            UIReadabilityHelper.ApplyButtonLabel(saveSlot2Label, min: 20, max: 30);
            UIReadabilityHelper.ApplyButtonLabel(saveSlot3Label, min: 20, max: 30);

            // Drop a soft dark wash behind the two prose columns so the
            // Bamao parchment-book background's decorative star + ornament
            // never washes the text out (the user-reported readability
            // issue).
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
                if (_confirmed) return;          // single-fire guard (D-069)
                _confirmed = true;
                SetButtonsInteractable(false);   // freeze the panel's own buttons
                Hide();
                OnEndOfDayConfirmed?.Invoke();
            });
        }

        public void Show(string summary, IReadOnlyList<string> heldMemoryTitles)
        {
            // Re-arm for a fresh end-of-day (e.g. a later in-mission day).
            _confirmed = false;
            SetButtonsInteractable(true);

            // Self-heal.
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            if (canvasGroup != null) { canvasGroup.alpha = 1f; canvasGroup.blocksRaycasts = true; canvasGroup.interactable = true; }

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

        /// <summary>
        /// Fully close the ledger. Works in every wiring layout, including the
        /// shipped prefab where <see cref="root"/> IS this GameObject (the
        /// D-069 soft-lock root cause). Always drops raycast-blocking so a
        /// hidden panel can never eat clicks intended for the night beats.
        /// </summary>
        public void Hide()
        {
            // 1) Never let an invisible panel block input.
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }

            // 2) Deactivate the visual root. If root is a *separate* child we
            //    deactivate it (keeps this component alive to be re-Shown). If
            //    root IS this GameObject (prefab layout) we deactivate self —
            //    Show() self-heals by re-activating. If there is no root at all,
            //    the CanvasGroup above has already suppressed the panel.
            if (root != null)
            {
                if (root != gameObject) root.SetActive(false);
                else gameObject.SetActive(false);
            }
        }

        private void SetButtonsInteractable(bool on)
        {
            if (saveSlot1 != null)            saveSlot1.interactable = on;
            if (saveSlot2 != null)            saveSlot2.interactable = on;
            if (saveSlot3 != null)            saveSlot3.interactable = on;
            if (autosaveButton != null)       autosaveButton.interactable = on;
            if (confirmEndOfDayButton != null) confirmEndOfDayButton.interactable = on;
        }

        private void SaveSlotPressed(int slot)
        {
            Hh.Log(LogCategory.Save, $"Player chose save slot {slot}.");
            OnSaveSlotChosen?.Invoke(slot);
        }
    }
}
