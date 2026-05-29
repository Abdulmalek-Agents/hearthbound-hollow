// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / MainMenuController
//
// Bamao-driven main menu. Wires the cozy CTA ("Open The Hollow") to the
// GameManager scene transition; exposes events for Continue / Save / Settings
// so a Mission-level coordinator can attach load/save behaviour without
// pulling the Save asmdef into UI.

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HearthboundHollow.Core;

namespace HearthboundHollow.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Buttons")]
        public Button openTheHollowButton;
        public Button continueButton;
        public Button settingsButton;
        public Button creditsButton;
        public Button quitButton;

        [Header("Panels (optional — assigned at scene build time)")]
        public GameObject settingsPanel;
        public GameObject creditsPanel;
        public ComfortToolsMenu comfortToolsMenu;
        public ToneCompassCard toneCompass;

        [Header("Phase 53 — Polish Menu Layer (optional)")]
        [Tooltip("System settings panel (language / customize / reset). Shown from Settings.")]
        public SystemMenuUI systemMenu;

        /// <summary>
        /// Phase 53 — optional New-Game gate. A Mission-asmdef coordinator assigns
        /// this; if it returns true the gate took over (e.g. opened the character
        /// creator) and BeginGame stops, letting the coordinator load the first
        /// scene once the player presses Begin. Null = start immediately.
        /// </summary>
        public System.Func<bool> NewGameGate;

        [Header("Scene routing")]
        public string firstMissionScene = "02_Mission01_Lane";

        [Header("Cozy tip strings (rotates each open)")]
        public TextMeshProUGUI tipLabel;
        public string[] tips = new[]
        {
            "Some memories want to be sold. Some don't.",
            "Polish in slow circles.",
            "Cover all sides — the orb has four faces.",
            "The warm center is the memory itself.",
            "Tea opens the way to talk.",
            "There is no wrong way to keep a memory.",
            "What you choose to remember is what you become.",
        };

        /// <summary>
        /// Fired when the player clicks Continue. A Save-aware coordinator
        /// (spawned by Phase 23) listens to this and loads the autosave.
        /// If no listener exists we simply log a warning.
        /// </summary>
        public event Action OnContinueRequested;

        /// <summary>
        /// Fired when the player clicks Settings (after the Tone Compass).
        /// Coordinator opens the settings panel + ComfortToolsMenu.
        /// </summary>
        public event Action OnSettingsRequested;

        private void Awake()
        {
            if (openTheHollowButton != null) openTheHollowButton.onClick.AddListener(OnOpenTheHollow);
            if (continueButton != null) continueButton.onClick.AddListener(OnContinue);
            if (settingsButton != null) settingsButton.onClick.AddListener(OnSettings);
            if (creditsButton != null) creditsButton.onClick.AddListener(() => { if (creditsPanel != null) creditsPanel.SetActive(true); });
            if (quitButton != null) quitButton.onClick.AddListener(OnQuit);

            if (tipLabel != null && tips != null && tips.Length > 0)
                tipLabel.text = tips[UnityEngine.Random.Range(0, tips.Length)];

            // Continue button is dim until a coordinator confirms an autosave exists.
            // The coordinator (Mission asmdef) will call SetContinueEnabled(true) on Start
            // if SaveService finds a non-empty autosave slot.
            SetContinueEnabled(false);
        }

        /// <summary>
        /// Coordinator calls this on Start once it has queried SaveService.
        /// </summary>
        public void SetContinueEnabled(bool enabled)
        {
            if (continueButton == null) return;
            continueButton.interactable = enabled;
            var labelGo = continueButton.transform.Find("Label");
            var label = labelGo != null ? labelGo.GetComponent<TextMeshProUGUI>() : continueButton.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null && !enabled) label.alpha = 0.4f;
            else if (label != null) label.alpha = 1f;
        }

        private void OnOpenTheHollow()
        {
            var vs = ServiceLocator.Get<VillageState>();
            if (vs != null && !vs.toneCompassAcknowledged && toneCompass != null)
            {
                toneCompass.OnAcknowledged += BeginGame;
                toneCompass.Show();
                return;
            }
            BeginGame();
        }

        private void BeginGame()
        {
            if (toneCompass != null) toneCompass.OnAcknowledged -= BeginGame;

            // Phase 53 — give the New-Game gate (character creator) a chance to
            // run first. If it handles the start, it loads the scene itself.
            if (NewGameGate != null && NewGameGate.Invoke()) return;

            var gm = GameManager.Instance;
            if (gm == null) { Hh.Err(LogCategory.UI, "GameManager missing — cannot start new game."); return; }
            gm.LoadScene(firstMissionScene);
        }

        private void OnContinue()
        {
            Hh.Log(LogCategory.UI, "Main Menu — Continue clicked.");
            if (OnContinueRequested == null)
            {
                Hh.Warn(LogCategory.UI, "Continue requested but no save coordinator is attached.");
                return;
            }
            OnContinueRequested.Invoke();
        }

        private void OnSettings()
        {
            Hh.Log(LogCategory.UI, "Main Menu — Settings clicked.");
            if (settingsPanel != null) settingsPanel.SetActive(true);
            if (comfortToolsMenu != null) comfortToolsMenu.Show();
            if (systemMenu != null) systemMenu.Show();   // Phase 53 — language / customize / reset
            OnSettingsRequested?.Invoke();
        }

        private void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
