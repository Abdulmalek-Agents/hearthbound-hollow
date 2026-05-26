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
// ── Phase 25 hotfix ─────────────────────────────────────────────
// Phase 23's procedural builder previously deactivated the script-host
// GameObject (rootGO.SetActive(false)). That made Update() stop running
// → the Escape key never opened the pause menu in any gameplay scene.
//
// The script now keeps its own GameObject ACTIVE always, and only the
// visual `root` child is toggled on/off. Pause() self-heals if the host
// was ever deactivated externally. The Phase 23 builder has been
// updated to wire `root` to a child Panel (see Phase23 patch).

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

        /// <summary>Fired when the player opens Settings from the pause menu.</summary>
        public event Action OnSettingsRequested;

        /// <summary>Fired immediately before the menu starts the Save & Quit flow.</summary>
        public event Action OnSaveAndQuitRequested;

        public bool IsPaused { get; private set; }

        private float _savedTimeScale = 1f;

        private void Awake()
        {
            // Hide only the visual panel — keep the host GameObject active so
            // Update() can listen for Escape.
            if (root != null && root != gameObject) root.SetActive(false);

            if (titleLabel != null && string.IsNullOrEmpty(titleLabel.text))
                titleLabel.text = "Paused";
            if (hintLabel != null && string.IsNullOrEmpty(hintLabel.text))
                hintLabel.text = "Take a breath. The Hollow will wait.";

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

            // Defensive self-heal in case something deactivated us externally.
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

            // Restore time scale before scene transition so the new scene runs normally.
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
            // Defensive: never leave the game paused if the menu is destroyed mid-pause.
            if (IsPaused && pauseTimeScale) Time.timeScale = _savedTimeScale;
            AudioListener.pause = false;
        }
    }
}
