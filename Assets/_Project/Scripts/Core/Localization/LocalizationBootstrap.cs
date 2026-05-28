// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Core / Localization / LocalizationBootstrap
//
// Phase 60 — Arabic Localization MVP.
//
// Belt-and-braces installer that guarantees a LocalizationService exists in
// the ServiceLocator before ANY scene's Awake() runs. Mirrors the Phase 32
// VoicePlayer + Phase 45 RuntimeAudioBootstrap pattern: a scene-baked
// SettingsServiceBootstrap GameObject is the preferred install, but a fresh
// clone with no scene wiring (e.g. EditMode test scene, headless CI run)
// still gets the service.
//
// Execution order: BeforeSceneLoad — earlier than every MonoBehaviour's
// Awake, so the first LocalizedText to spin up can already resolve strings.
//
// D-061 (Phase 60):
//   Localization MUST be available before any UI Awake() runs. Period.
//   Any new scene that calls `ServiceLocator.Get<LocalizationService>()`
//   without first installing it via BootstrapInstall() is a bug.

using UnityEngine;

namespace HearthboundHollow.Core
{
    public static class LocalizationBootstrap
    {
        /// <summary>
        /// Auto-runs before any scene loads. Idempotent — safe to call from
        /// SettingsServiceBootstrap.Awake() as well.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void BootstrapInstall()
        {
            if (ServiceLocator.Get<LocalizationService>() != null) return;
            var loc = new LocalizationService();
            ServiceLocator.Register(loc);
            Hh.Log(LogCategory.Boot,
                $"LocalizationBootstrap installed LocalizationService " +
                $"(active locale: {loc.CurrentLocale}, native: {LocaleInfo.NativeName(loc.CurrentLocale)}).");
        }
    }
}
