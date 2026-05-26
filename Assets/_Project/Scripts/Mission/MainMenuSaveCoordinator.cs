// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / MainMenuSaveCoordinator
//
// A small bridge that lives in the Main Menu scene and connects the
// UI-only MainMenuController to the Save asmdef. Keeps UI free of any
// Save reference (asmdef discipline).
//
// Responsibilities:
//   1. Create the SaveService (if not already in ServiceLocator) and
//      register it.
//   2. On Start, check whether autosave exists and enable/disable the
//      Continue button accordingly.
//   3. On OnContinueRequested: load the autosave + load the last scene.
//
// This script is spawned by Phase 23 capstone into the Main Menu scene.

using UnityEngine;
using HearthboundHollow.Core;
using HearthboundHollow.Save;
using HearthboundHollow.UI;

namespace HearthboundHollow.Mission
{
    public class MainMenuSaveCoordinator : MonoBehaviour
    {
        [Header("Wired by Phase 23 builder")]
        public MainMenuController menuController;

        [Header("Fallback if no last-scene saved")]
        public string fallbackContinueScene = "02_Mission01_Lane";

        private SaveService _save;

        private void Awake()
        {
            // Ensure SaveService is in the ServiceLocator so any subsystem can use it.
            _save = ServiceLocator.Get<SaveService>();
            if (_save == null)
            {
                _save = new SaveService();
                ServiceLocator.Register(_save);
            }
        }

        private void Start()
        {
            if (menuController == null)
            {
                Hh.Warn(LogCategory.Save, "MainMenuSaveCoordinator: no MainMenuController wired.");
                return;
            }

            // Subscribe to Continue button event.
            menuController.OnContinueRequested += OnContinue;

            // Enable Continue button only if an autosave (slot -1) actually exists.
            bool autosaveExists = _save.SlotExists(-1);
            menuController.SetContinueEnabled(autosaveExists);
            Hh.Log(LogCategory.Save, $"MainMenuSaveCoordinator: autosave exists = {autosaveExists}.");
        }

        private void OnDestroy()
        {
            if (menuController != null) menuController.OnContinueRequested -= OnContinue;
        }

        private void OnContinue()
        {
            var vs = ServiceLocator.Get<VillageState>();
            var gm = GameManager.Instance;
            if (_save == null || vs == null || gm == null)
            {
                Hh.Err(LogCategory.Save, "Continue: SaveService / VillageState / GameManager missing.");
                return;
            }

            bool ok = _save.Load(-1, vs);
            if (!ok)
            {
                Hh.Warn(LogCategory.Save, "Continue: load failed — staying on Main Menu.");
                return;
            }

            string target = !string.IsNullOrEmpty(vs.lastSceneName) ? vs.lastSceneName : fallbackContinueScene;
            Hh.Log(LogCategory.Save, $"Continue: loaded autosave; loading scene '{target}'.");
            gm.LoadScene(target);
        }
    }
}
