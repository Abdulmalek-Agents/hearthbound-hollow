// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / SandboxBootstrapper
//
// Minimal stand-in for GameManager in the sandbox prototype scene
// (06_SandboxProto.unity).  GameManager lives on 00_Bootstrap and auto-
// loads MainMenu — we don't want that chain in a raw prototype, so this
// tiny MonoBehaviour does just the two things the rest of the codebase
// actually needs:
//   1. Register VillageState with ServiceLocator (systems call
//      ServiceLocator.Get<VillageState>() — they need it there).
//   2. Reset VillageState to day-0 defaults so every Editor Play run
//      starts clean (no leftover flags from a previous session).
//
// Execution order -900 guarantees it runs before SandboxVillagerDirector
// (-800) and every other script in the scene.

using UnityEngine;
using HearthboundHollow.Core;

namespace HearthboundHollow.Mission
{
    [DefaultExecutionOrder(-900)]
    public class SandboxBootstrapper : MonoBehaviour
    {
        [Tooltip("Drag Assets/_Project/ScriptableObjects/State/VillageState.asset here.")]
        public VillageState villageState;

        private void Awake()
        {
            if (villageState == null)
            {
                Hh.Err(LogCategory.Boot,
                    "[Sandbox] SandboxBootstrapper: no VillageState assigned. " +
                    "Drag VillageState.asset onto this component in the Inspector.");
                return;
            }

            villageState.ResetToDefault();
            ServiceLocator.Register(villageState);
            EventBus.Publish(new VillageStateLoadedEvent(villageState));

            Hh.Log(LogCategory.Boot,
                "[Sandbox] VillageState registered (day 0, coin 0). " +
                "All systems can now call ServiceLocator.Get<VillageState>().");
        }

        private void OnDestroy()
        {
            // Clear registrations when Play mode ends so they don't bleed
            // into the next run.
            ServiceLocator.Clear();
            EventBus.ClearAll();
        }
    }
}
