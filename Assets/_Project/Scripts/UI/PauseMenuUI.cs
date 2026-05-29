// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / PauseMenuUI
//
// Pause overlay shown on Escape in any gameplay scene. Pauses time, shows:
//   • Resume
//   • Settings (delegates to ComfortToolsMenu or external settings panel)
//   • Save & Quit to Main Menu (triggers autosave then loads MainMenu)
//   • Quit to Desktop
//
// Cozy-by-default: does NOT block players — it's a soft pause with the option
// to step away. Per Codex 06 (Cozy Comfort + Accessibility).
//
// ── Phase 60 — Arabic Localization MVP ──────────────────────────
// Every static label + button caption is pulled from loc.<iso>.json via
// LocalizationService. Subscribes to LocaleChangedEvent for live language
// flips (no scene reload needed).

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HearthboundHollow.Core;

namespace HearthboundHollow.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("Root")]
        [Tooltip("The visual root that gets toggled. SHOULD be a child of the " +
                 "script-host GameObject — never set this to the same GameObject " +
                 "as the script, or Update() won't run and Escape won't toggle.")]
        public GameObject root;

        [Header("Buttons")]
        public Button resumeButton;
        public Button settingsButton;
        public Button saveAndQuitButton;
        public Button quitToDesktopButton;

        [Header("Labels")]
        public TextMeshProUGUI titleLabel;
        public TextMeshProUGUI hintLabel;

        [Header("Behaviour")]
        public KeyCode toggleKey = KeyCode.Escape;
        [Tooltip("If true, sets Time.timeScale = 0 while paused.")]
        public bool pauseTimeScale = true;
        public string mainMenuSceneName = "01_MainMenu";

        public event Action OnSettingsRequested;
        public event Action OnSaveAndQuitRequested;

        public bool IsPaused { get; private set; }

        private float _savedTimeScale = 1f;

        private void Awake()
        {
            if (root != null && root != gameObject) root.SetActive(false);

            // Phase 60 — Localized title + hint (fallback to English on
            // missing key). Re-applied on LocaleChangedEvent in OnEnable.
            ApplyLocalization();

            if (resumeButton != null)        resumeButton.onClick.AddListener(Resume);
            if (settingsButton != null)      settingsButton.onClick.AddListener(OnSettingsClicked);
            if (saveAndQuitButton != null)   saveAndQuitButton.onClick.AddListener(OnSaveAndQuitClicked);
            if (quitToDesktopButton != null) quitToDesktopButton.onClick.AddListener(OnQuitToDesktop);
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                if (IsPaused) Resume();
                else Pause();
            }
        }

        public void Pause()
        {
            if (IsPaused) return;

            if (!gameObject.activeSelf) gameObject.SetActive(true);

            IsPaused = true;
            if (root != null) root.SetActive(true);
            if (pauseTimeScale)
            {
                _savedTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }
            AudioListener.pause = true;
            Hh.Log(LogCategory.UI, "Pause menu opened.");
        }

        public void Resume()
        {
            if (!IsPaused) return;
            IsPaused = false;
            if (root != null && root != gameObject) root.SetActive(false);
            if (pauseTimeScale) Time.timeScale = _savedTimeScale;
            AudioListener.pause = false;
            Hh.Log(LogCategory.UI, "Pause menu closed.");
        }

        private void OnSettingsClicked()
        {
            Hh.Log(LogCategory.UI, "Pause → Settings requested.");
            OnSettingsRequested?.Invoke();
        }

        private void OnSaveAndQuitClicked()
        {
            Hh.Log(LogCategory.UI, "Pause → Save & Quit to Main Menu.");
            OnSaveAndQuitRequested?.Invoke();

            if (pauseTimeScale) Time.timeScale = _savedTimeScale;
            AudioListener.pause = false;
            IsPaused = false;

            var gm = GameManager.Instance;
            if (gm != null) gm.LoadScene(mainMenuSceneName);
        }

        private void OnQuitToDesktop()
        {
            Hh.Log(LogCategory.UI, "Pause → Quit to Desktop.");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnDestroy()
        {
            if (IsPaused && pauseTimeScale) Time.timeScale = _savedTimeScale;
            AudioListener.pause = false;
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
        /// Phase 60 — Pull localized strings for every static label + button.
        /// Idempotent; called from Awake, on every locale change, and OnEnable.
        /// </summary>
        private void ApplyLocalization()
        {
            var loc = ServiceLocator.Get<LocalizationService>();
            if (loc == null) return;
            bool rtl = loc.IsRightToLeft;

            SetLabel(titleLabel, loc.Get("pause.title"), rtl);
            SetLabel(hintLabel, loc.Get("pause.hint"), rtl);
            SetButtonLabel(resumeButton, loc.Get("pause.cta.resume"), rtl);
            SetButtonLabel(settingsButton, loc.Get("pause.cta.settings"), rtl);
            SetButtonLabel(saveAndQuitButton, loc.Get("pause.cta.save_and_quit"), rtl);
            SetButtonLabel(quitToDesktopButton, loc.Get("pause.cta.quit_desktop"), rtl);
        }

        private static void SetLabel(TextMeshProUGUI t, string s, bool rtl)
        {
            if (t == null) return;
            t.text = rtl ? ArabicTextShaper.Shape(s) : s;
            t.isRightToLeftText = rtl;
        }

        private static void SetButtonLabel(Button b, string s, bool rtl)
        {
            if (b == null) return;
            var t = b.GetComponentInChildren<TextMeshProUGUI>();
            if (t == null) return;
            t.text = rtl ? ArabicTextShaper.Shape(s) : s;
            t.isRightToLeftText = rtl;
        }
    }
}
