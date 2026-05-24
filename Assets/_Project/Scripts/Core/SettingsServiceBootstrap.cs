// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Core / SettingsServiceBootstrap
//
// A runtime MonoBehaviour that lives on the Bootstrap _GameRoot. On Awake
// (very early — execution order -900) it ensures a SettingsService is
// registered with the ServiceLocator so any scene-side script can read
// settings via ServiceLocator.Get<SettingsService>().
//
// This is a runtime component (NOT in the Editor asmdef) because the Editor
// builder uses `AddComponent<SettingsServiceBootstrap>()` to attach it.

using UnityEngine;

namespace HearthboundHollow.Core
{
    [DefaultExecutionOrder(-900)]
    public class SettingsServiceBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            if (ServiceLocator.Get<SettingsService>() == null)
            {
                ServiceLocator.Register(new SettingsService());
                Hh.Log(LogCategory.Boot, "SettingsService registered via SettingsServiceBootstrap.");
            }
        }
    }
}
