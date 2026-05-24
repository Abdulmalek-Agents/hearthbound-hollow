// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / PauseSaveCoordinator
//
// Lives in every gameplay scene as a small dedicated GameObject (NOT on the
// pause menu itself, which starts disabled). When the player chooses
// "Save & Quit" from the pause menu, this coordinator triggers an autosave
// to slot -1 before the PauseMenuUI transitions to the Main Menu.
//
// Also handles the in-game autosave on DayEndedEvent so the autosave is
// always up to date when the player ends a mission.
//
// SUBSCRIPTIONS: Awake/OnDestroy (NOT OnEnable/OnDisable) — the coordinator
// must remain subscribed to the EventBus even when its GameObject is parented
// to a deactivated pause menu. Previously this lived on the pause menu itself
// and its DayEndedEvent subscription only fired while the pause menu was
// open, breaking the autosave-on-day-end pattern.

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
        private bool _busSubscribed;

        private void Awake()
        {
            _save = ServiceLocator.Get<SaveService>();
            if (_save == null)
            {
                _save = new SaveService();
                ServiceLocator.Register(_save);
            }

            // Subscribe in Awake (not OnEnable) so the event bus binding survives
            // even when this GameObject's parent is deactivated.
            if (pauseMenu != null) pauseMenu.OnSaveAndQuitRequested += OnSaveAndQuit;
            if (autosaveOnDayEnded) { EventBus.Subscribe<DayEndedEvent>(OnDayEnded); _busSubscribed = true; }
        }

        private void OnDestroy()
        {
            if (pauseMenu != null) pauseMenu.OnSaveAndQuitRequested -= OnSaveAndQuit;
            if (_busSubscribed) { EventBus.Unsubscribe<DayEndedEvent>(OnDayEnded); _busSubscribed = false; }
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
