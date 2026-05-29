// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / MainMenuController
//
// Bamao-driven main menu. Wires the cozy CTA ("Open The Hollow") to the
// GameManager scene transition; exposes events for Continue / Save / Settings
// so a Mission-level coordinator can attach load/save behaviour without
// pulling the Save asmdef into UI.
//
// ── Phase 60 — Arabic Localization MVP ──────────────────────────
// Every button label + rotating cozy tip is pulled from loc.<iso>.json
// via LocalizationService. Subscribes to LocaleChangedEvent for live
// language flips (no scene reload needed).

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

        public event Action OnContinueRequested;
        public event Action OnSettingsRequested;

        private void Awake()
        {
            if (openTheHollowButton != null) openTheHollowButton.onClick.AddListener(OnOpenTheHollow);
            if (continueButton != null) continueButton.onClick.AddListener(OnContinue);
            if (settingsButton != null) settingsButton.onClick.AddListener(OnSettings);
            if (creditsButton != null) creditsButton.onClick.AddListener(() => { if (creditsPanel != null) creditsPanel.SetActive(true); });
            if (quitButton != null) quitButton.onClick.AddListener(OnQuit);

            // Phase 60 — Localize button labels + cozy tip. Re-applied on
            // locale change via OnEnable's subscription.
            ApplyLocalization();
            // Continue button is dim until a coordinator confirms an autosave exists.
            SetContinueEnabled(false);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<LocaleChangedEvent>(OnLocaleChanged);
            ApplyLocalization();
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<LocaleChangedEvent>(OnLocaleChanged);
        }

        private void OnLocaleChanged(LocaleChangedEvent _) => ApplyLocalization();

        /// <summary>
        /// Phase 60 — Pull localized strings for every static label.
        /// </summary>
        private void ApplyLocalization()
        {
            var loc = ServiceLocator.Get<LocalizationService>();
            if (loc == null)
            {
                // Bootstrap not yet up — render the original English tip.
                if (tipLabel != null && tips != null && tips.Length > 0)
                    tipLabel.text = tips[UnityEngine.Random.Range(0, tips.Length)];
                return;
            }
            bool rtl = loc.IsRightToLeft;

            SetButtonLabel(openTheHollowButton, loc.Get("menu.main.cta.open_hollow"), rtl);
            SetButtonLabel(continueButton,      loc.Get("menu.main.cta.continue"),    rtl);
            SetButtonLabel(settingsButton,      loc.Get("menu.main.cta.settings"),    rtl);
            SetButtonLabel(creditsButton,       loc.Get("menu.main.cta.credits"),     rtl);
            SetButtonLabel(quitButton,          loc.Get("menu.main.cta.quit"),        rtl);

            // Rotate the cozy tip from the localized table (7 tips).
            if (tipLabel != null)
            {
                int idx = UnityEngine.Random.Range(0, 7);
                string tip = loc.Get($"menu.main.tip.{idx}");
                tipLabel.text = rtl ? ArabicTextShaper.Shape(tip) : tip;
                tipLabel.isRightToLeftText = rtl;
                tipLabel.alignment = rtl
                    ? TMPro.TextAlignmentOptions.TopRight
                    : TMPro.TextAlignmentOptions.TopLeft;
            }
        }

        private static void SetButtonLabel(Button b, string s, bool rtl)
        {
            if (b == null) return;
            var t = b.GetComponentInChildren<TextMeshProUGUI>();
            if (t == null) return;
            t.text = rtl ? ArabicTextShaper.Shape(s) : s;
            t.isRightToLeftText = rtl;
        }

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
