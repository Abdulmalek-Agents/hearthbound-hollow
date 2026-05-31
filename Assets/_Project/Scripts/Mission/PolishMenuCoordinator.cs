// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / PolishMenuCoordinator
//
// Phase 53 — the Save-aware bridge for the Polish Menu Layer. Lives in the Main
// Menu scene and (via the Phase 53 builder) the gameplay scenes' pause rig.
// Keeps the UI asmdef free of any Save reference (asmdef discipline).
//
// Wires:
//   • SystemMenuUI.OnResetConfirmed  → wipe all save slots + reset VillageState
//        + clear the character-creation flag, then return to the title.
//   • SystemMenuUI.OnCustomizeRequested → open the character creator.
//   • CharacterCreationUI.OnConfirmed → re-apply the look to the live avatar
//        (no-op on menus) and, when the creator gated a New Game, start it.

using UnityEngine;
using HearthboundHollow.Core;
using HearthboundHollow.Save;
using HearthboundHollow.UI;

namespace HearthboundHollow.Mission
{
    public class PolishMenuCoordinator : MonoBehaviour
    {
        [Header("Wired by the Phase 53 builder")]
        public SystemMenuUI systemMenu;
        public CharacterCreationUI characterCreation;

        [Tooltip("Main Menu only — lets the coordinator install the New-Game " +
                 "character-creation gate + the Settings → System menu hook.")]
        public MainMenuController mainMenu;

        [Tooltip("Gameplay scenes only — opens the System menu from Pause → Settings.")]
        public PauseMenuUI pauseMenu;

        [Header("Routing")]
        public string mainMenuScene = "01_MainMenu";

        [Tooltip("When the creator is opened as a New-Game gate, the scene to load " +
                 "once the player presses Begin. Empty = creator is 'customize only' " +
                 "(stay where we are + re-apply to the live avatar).")]
        public string newGameScene = "";

        private SaveService _save;
        private bool _creatorGatesNewGame;

        private void Awake()
        {
            _save = ServiceLocator.Get<SaveService>();
            if (_save == null) { _save = new SaveService(); ServiceLocator.Register(_save); }
        }

        private void OnEnable()
        {
            if (systemMenu != null)
            {
                systemMenu.OnResetConfirmed   += DoReset;
                systemMenu.OnCustomizeRequested += OpenCreatorForCustomize;
            }
            if (characterCreation != null)
                characterCreation.OnConfirmed += OnCreatorConfirmed;

            // Main Menu: install the New-Game gate + the Settings → System hook.
            if (mainMenu != null)
            {
                mainMenu.systemMenu = systemMenu;
                mainMenu.NewGameGate = () => TryGateNewGame(mainMenu.firstMissionScene);
            }

            // Pause menu: open the System menu from Pause → Settings.
            if (pauseMenu != null)
                pauseMenu.OnSettingsRequested += ShowSystemMenu;
        }

        private void OnDisable()
        {
            if (systemMenu != null)
            {
                systemMenu.OnResetConfirmed   -= DoReset;
                systemMenu.OnCustomizeRequested -= OpenCreatorForCustomize;
            }
            if (characterCreation != null)
                characterCreation.OnConfirmed -= OnCreatorConfirmed;
            if (mainMenu != null && mainMenu.NewGameGate != null)
                mainMenu.NewGameGate = null;
            if (pauseMenu != null)
                pauseMenu.OnSettingsRequested -= ShowSystemMenu;
        }

        private void ShowSystemMenu()
        {
            if (systemMenu != null) systemMenu.Show();
        }

        // ───── New-Game gate (called by MainMenuController) ────────

        /// <summary>
        /// Open the creator as a gate before a New Game. Returns true if the
        /// creator was shown (caller should wait); false if the player already
        /// has a character and the game may begin immediately.
        /// </summary>
        public bool TryGateNewGame(string firstScene)
        {
            var s = ServiceLocator.Get<SettingsService>();
            bool created = s != null && s.CharacterCreated;
            if (created || characterCreation == null) return false;

            _creatorGatesNewGame = true;
            newGameScene = firstScene;
            characterCreation.Show();
            return true;
        }

        // ───── Reset ───────────────────────────────────────────────

        private void DoReset()
        {
            // 1) Wipe every save slot (3 manual + autosave).
            if (_save != null)
            {
                _save.DeleteSlot(-1);
                for (int i = 0; i < 3; i++) _save.DeleteSlot(i);
            }
            // 2) Reset the live VillageState to a fresh game.
            var vs = ServiceLocator.Get<VillageState>();
            if (vs != null) vs.ResetToDefault();
            // 3) Clear the character so the next New Game re-opens the creator.
            //    (Language + audio + comfort settings are deliberately kept.)
            var settings = ServiceLocator.Get<SettingsService>();
            if (settings != null) settings.ClearCharacterCreation();

            Hh.Log(LogCategory.Save, "PolishMenuCoordinator: game reset — saves cleared, returning to title.");
            var gm = GameManager.Instance;
            if (gm != null) gm.LoadScene(mainMenuScene);
        }

        // ───── Customize ───────────────────────────────────────────

        private void OpenCreatorForCustomize()
        {
            _creatorGatesNewGame = false;
            if (characterCreation != null) characterCreation.Show();
        }

        private void OnCreatorConfirmed()
        {
            // Re-apply to a live avatar if one exists (no-op on the menu).
            if (GameObject.FindGameObjectWithTag("Player") != null &&
                GetComponent<CharacterAppearanceApplier>() == null)
                gameObject.AddComponent<CharacterAppearanceApplier>();

            if (_creatorGatesNewGame && !string.IsNullOrEmpty(newGameScene))
            {
                _creatorGatesNewGame = false;
                var gm = GameManager.Instance;
                if (gm != null) gm.LoadScene(newGameScene);
            }
        }
    }
}
