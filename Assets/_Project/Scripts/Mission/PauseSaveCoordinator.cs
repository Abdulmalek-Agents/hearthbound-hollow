// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / PauseSaveCoordinator
//
// Lives in every gameplay scene next to the PauseMenuUI. When the player
// chooses "Save & Quit" from the pause menu, this coordinator triggers an
// autosave to slot -1 before the PauseMenuUI transitions to the Main Menu.
//
// Also handles the in-game autosave on DayEndedEvent so the autosave is
// always up to date when the player ends a mission.

using UnityEngine;
using UnityEngine.SceneManagement;
using HearthboundHollow.Core;
using HearthboundHollow.Save;
using HearthboundHollow.UI;

namespace HearthboundHollow.Mission
{
    public class PauseSaveCoordinator : MonoBehaviour
    {
        [Header("Wired by Phase 23 builder")]
        public PauseMenuUI pauseMenu;

        [Header("Autosave on day-end")]
        public bool autosaveOnDayEnded = true;

        private SaveService _save;

        private void Awake()
        {
            _save = ServiceLocator.Get<SaveService>();
            if (_save == null)
            {
                _save = new SaveService();
                ServiceLocator.Register(_save);
            }
        }

        private void OnEnable()
        {
            if (pauseMenu != null) pauseMenu.OnSaveAndQuitRequested += OnSaveAndQuit;
            if (autosaveOnDayEnded) EventBus.Subscribe<DayEndedEvent>(OnDayEnded);
        }

        private void OnDisable()
        {
            if (pauseMenu != null) pauseMenu.OnSaveAndQuitRequested -= OnSaveAndQuit;
            if (autosaveOnDayEnded) EventBus.Unsubscribe<DayEndedEvent>(OnDayEnded);
        }

        private void OnSaveAndQuit()
        {
            var vs = ServiceLocator.Get<VillageState>();
            if (vs == null || _save == null) return;

            // Capture current scene so Continue can return here.
            vs.lastSceneName = SceneManager.GetActiveScene().name;
            _save.Save(-1, vs);
            Hh.Log(LogCategory.Save, $"Pause → Save & Quit: autosaved (scene={vs.lastSceneName}).");
        }

        private void OnDayEnded(DayEndedEvent e)
        {
            var vs = ServiceLocator.Get<VillageState>();
            if (vs == null || _save == null) return;
            vs.lastSceneName = SceneManager.GetActiveScene().name;
            _save.Save(-1, vs);
            Hh.Log(LogCategory.Save, $"Autosaved on DayEnded (day {e.DayIndex}, scene {vs.lastSceneName}).");
        }
    }
}
