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

        [Tooltip("Phase 57 (D-076) — Arabic tips, index-matched to `tips`. Used when " +
                 "Arabic is the active language (shaped at display). Leave an entry " +
                 "empty to fall back to the English tip at that index.")]
        public string[] tipsAr = new[]
        {
            "بعض الذكريات تريد أن تُباع. وبعضها لا.",
            "لمّع بدوائر بطيئة.",
            "غطِّ كل الجوانب — للجوهرة أربعة أوجه.",
            "المركز الدافئ هو الذكرى نفسها.",
            "الشاي يفتح باب الحديث.",
            "لا توجد طريقة خاطئة لحفظ ذكرى.",
            "ما تختار تذكّره هو ما تصير إليه.",
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

            // Phase 57 (D-076) — localize the main-menu CTAs so they follow the
            // selected language. The menu shipped with only the auto-built
            // Settings button localized, so Arabic showed "الإعدادات" beside an
            // English "Open The Hollow" / "Quit". LocalizedText re-pulls + shapes
            // on every language change.
            LocalizeButton(openTheHollowButton, "menu.open_hollow");
            LocalizeButton(continueButton,      "menu.continue");
            LocalizeButton(settingsButton,      "menu.settings");
            LocalizeButton(creditsButton,       "menu.credits");
            LocalizeButton(quitButton,          "menu.quit");

            if (tipLabel != null && tips != null && tips.Length > 0)
            {
                int i = UnityEngine.Random.Range(0, tips.Length);
                string tip = (LocalizationService.IsRightToLeft && tipsAr != null &&
                              i < tipsAr.Length && !string.IsNullOrEmpty(tipsAr[i]))
                    ? tipsAr[i] : tips[i];
                tipLabel.text = LocalizationService.Shape(tip);
            }

            // Continue button is dim until a coordinator confirms an autosave exists.
            // The coordinator (Mission asmdef) will call SetContinueEnabled(true) on Start
            // if SaveService finds a non-empty autosave slot.
            SetContinueEnabled(false);

            // Phase 57 (D-075): make sure Settings (language / reset / customise) is
            // reachable from the FIRST screen. The main-menu scene shipped with both
            // settingsButton AND systemMenu unwired, so the player had no way to pick
            // العربية before starting. Self-heal: find the SystemMenuUI in the scene
            // and, if there is no Settings button, build one (styled like the menu's
            // other buttons) wired to OnSettings.
            EnsureSettingsAccess();
        }

        // Phase 57 (D-076) — bind a Bamao button's label to the localization
        // table via LocalizedText so it follows language changes (mirrors the
        // auto-built Settings button path). No-op if the button/label is missing.
        private static void LocalizeButton(Button b, string key)
        {
            if (b == null) return;
            var lbl = b.GetComponentInChildren<TextMeshProUGUI>(true);
            if (lbl == null) return;
            var lt = lbl.GetComponent<LocalizedText>();
            if (lt == null) lt = lbl.gameObject.AddComponent<LocalizedText>();
            lt.SetKey(key);
        }

        /// <summary>
        /// Self-healing Settings access (D-075). Finds the SystemMenuUI if unassigned
        /// and creates a Settings button when one is missing, so language/reset are
        /// always reachable from the main menu. Idempotent + defensive.
        /// </summary>
        private void EnsureSettingsAccess()
        {
            try
            {
                if (systemMenu == null)
#if UNITY_2023_1_OR_NEWER
                    systemMenu = FindFirstObjectByType<SystemMenuUI>(FindObjectsInactive.Include);
#else
                    systemMenu = FindObjectOfType<SystemMenuUI>(true);
#endif

                if (settingsButton != null) return; // already present

                var template = openTheHollowButton != null ? openTheHollowButton : quitButton;
                if (template == null) return;       // nothing to clone — leave as-is

                var clone = Instantiate(template.gameObject, template.transform.parent);
                clone.name = "SettingsButton (auto)";
                clone.SetActive(true);

                // Compact, top-right corner so it never overlaps the cozy centred CTAs.
                var rt = clone.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
                    rt.pivot = new Vector2(1f, 1f);
                    rt.anchoredPosition = new Vector2(-28f, -28f);
                    rt.sizeDelta = new Vector2(Mathf.Clamp(rt.sizeDelta.x, 180f, 300f),
                                               Mathf.Clamp(rt.sizeDelta.y, 48f, 70f));
                    rt.localScale = Vector3.one;
                }

                settingsButton = clone.GetComponent<Button>();
                if (settingsButton != null)
                {
                    settingsButton.onClick.RemoveAllListeners(); // drop any cloned runtime listeners
                    settingsButton.onClick.AddListener(OnSettings);
                    settingsButton.interactable = true;
                }

                // Localised label ("Settings" / "الإعدادات") that follows language changes.
                var lbl = clone.GetComponentInChildren<TextMeshProUGUI>(true);
                if (lbl != null)
                {
                    var lt = lbl.GetComponent<LocalizedText>();
                    if (lt != null) lt.SetKey("menu.settings");
                    else lbl.text = LocalizationService.Get("menu.settings");
                    lbl.enableAutoSizing = true;
                }

                Hh.Log(LogCategory.UI, "MainMenu: ensured a Settings button (D-075).");
            }
            catch (System.Exception ex)
            {
                Hh.Warn(LogCategory.UI, $"MainMenu EnsureSettingsAccess skipped: {ex.Message}");
            }
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
