// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / MainMenuController
//
// The main menu. Heat-driven layout; we wire the "Open The Hollow" button to
// the GameManager scene transition.

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

        [Header("Settings panel")]
        public GameObject settingsPanel;
        public GameObject creditsPanel;
        public ToneCompassCard toneCompass;

        [Header("Scene routing")]
        public string firstMissionScene = "02_Mission01_Lane";

        [Header("Cozy tip strings (rotates each open)")]
        public TextMeshProUGUI tipLabel;
        public string[] tips = new[]
        {
            "Some memories want to be sold. Some don't.",
            "Polish in slow circles.",
            "Cover all sides.",
            "The warm center is the memory itself.",
            "Tea opens the way to talk.",
        };

        private void Awake()
        {
            if (openTheHollowButton != null) openTheHollowButton.onClick.AddListener(OnOpenTheHollow);
            if (continueButton != null) continueButton.onClick.AddListener(OnContinue);
            if (settingsButton != null) settingsButton.onClick.AddListener(() => { if (settingsPanel != null) settingsPanel.SetActive(true); });
            if (creditsButton != null) creditsButton.onClick.AddListener(() => { if (creditsPanel != null) creditsPanel.SetActive(true); });
            if (quitButton != null) quitButton.onClick.AddListener(OnQuit);
            if (tipLabel != null && tips != null && tips.Length > 0)
                tipLabel.text = tips[Random.Range(0, tips.Length)];
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
            var gm = GameManager.Instance;
            if (gm == null) { Hh.Err(LogCategory.UI, "GameManager missing — cannot start new game."); return; }
            gm.LoadScene(firstMissionScene);
        }

        private void OnContinue()
        {
            Hh.Log(LogCategory.UI, "Continue clicked — SaveService dispatch goes here.");
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
