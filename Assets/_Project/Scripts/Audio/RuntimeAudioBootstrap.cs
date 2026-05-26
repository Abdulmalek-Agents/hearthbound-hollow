// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Audio / RuntimeAudioBootstrap
//
// Phase 45 — runtime audio rig auto-installer.
//
// Phase 38's `WireBootstrapAudioRig()` Editor builder drops a
// `_HHAudio_Bootstrap` GameObject into the Bootstrap scene with
// MusicPlayer + MumbleVoicePlayer + AmbientAudio attached. But that
// scene wiring only lands if the user has run
// `Hearthbound → 🚀 Build Everything` since pulling the audio branch.
//
// If the user pulls the branch and presses Play *without* running Build
// Everything first, the audio rig is missing → dialogue mumble silent +
// scene music silent. This was the user-reported "dialogue sound not
// working" bug.
//
// Phase 45's fix: this runtime auto-installer runs via
// `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]` on every game start.
// It checks for an existing audio rig (any GameObject containing a
// MusicPlayer + MumbleVoicePlayer pair) and creates one programmatically
// if none is present. The runtime rig loads the libraries via
// `Resources.Load` — Phase 37 writes them to
// `Assets/_Project/Audio/Resources/` for exactly this reason.
//
// This makes the dialogue + music subsystem **self-healing**: even on a
// fresh clone where the user hasn't pressed Build Everything yet, sound
// works.

using UnityEngine;
using HearthboundHollow.Core;

namespace HearthboundHollow.Audio
{
    /// <summary>
    /// Runtime auto-installer for the audio rig. Idempotent — if Phase 38's
    /// scene-baked rig is already present (in the Bootstrap scene), this
    /// installer no-ops. Otherwise it spawns an equivalent programmatic rig
    /// with libraries loaded from Resources/.
    /// </summary>
    public static class RuntimeAudioBootstrap
    {
        public const string BootstrapGameObjectName = "_HHAudio_Bootstrap_Runtime";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInstall()
        {
            // Scene-loaded rigs aren't visible BeforeSceneLoad; wait for the
            // first frame after scene load to decide whether to install. We
            // do this by registering a one-shot scene-loaded callback.
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoadedOnce;
        }

        private static bool _installed;

        private static void OnSceneLoadedOnce(UnityEngine.SceneManagement.Scene scene,
                                              UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            if (_installed) return;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoadedOnce;
            _installed = true;
            EnsureAudioRig();
        }

        /// <summary>
        /// Public so the Phase 40 audio diagnostic can call it manually if
        /// the user wants to test the auto-install path. Idempotent.
        /// </summary>
        public static void EnsureAudioRig()
        {
            // 1. If Phase 38's scene-baked rig is present, don't duplicate.
            //    We detect by finding any MusicPlayer (only one allowed at
            //    a time per the DontDestroyOnLoad + ServiceLocator pattern).
            var existingMusic = Object.FindFirstObjectByType<MusicPlayer>();
            var existingMumble = Object.FindFirstObjectByType<MumbleVoicePlayer>();
            if (existingMusic != null && existingMumble != null)
            {
                // Both present — the scene wiring is intact.
                Hh.Log(LogCategory.Audio,
                    "RuntimeAudioBootstrap: scene-baked audio rig detected; no install needed.");
                return;
            }

            // 2. Build the programmatic rig.
            var root = new GameObject(BootstrapGameObjectName);
            Object.DontDestroyOnLoad(root);

            // MusicPlayer — its Awake() will Resources.Load the library
            // if we don't pre-assign it.
            if (existingMusic == null)
            {
                var music = root.AddComponent<MusicPlayer>();
                music.surviveSceneLoad = true;
                music.globalScale = 0.75f;
                // Pre-load the library so MusicPlayer.Awake's self-heal can
                // skip the Resources.Load roundtrip.
                music.library = Resources.Load<MusicLibrarySO>(MusicPlayer.ResourcesLibraryName);
            }

            // MumbleVoicePlayer on a child GameObject.
            if (existingMumble == null)
            {
                var mumbleHost = new GameObject("MumbleVoicePlayer");
                mumbleHost.transform.SetParent(root.transform, false);
                var mumble = mumbleHost.AddComponent<MumbleVoicePlayer>();
                mumble.globalScale = 0.55f;
                mumble.library = Resources.Load<MumbleVoiceLibrarySO>(MumbleVoicePlayer.ResourcesLibraryName);
            }

            // AmbientAudio — uses SfxLibrarySO, lives at the canonical
            // (non-Resources) path. AmbientAudio.Awake() already handles
            // a null library gracefully (logs a warning).
            var ambHost = new GameObject("AmbientAudio");
            ambHost.transform.SetParent(root.transform, false);
            ambHost.AddComponent<AudioSource>();
            var amb = ambHost.AddComponent<AmbientAudio>();
            amb.baseVolume = 0.35f;
            amb.playOnStart = false;
            amb.surviveSceneLoad = true;
            // Try Resources first; AmbientAudio also has its own load path.
            amb.library = Resources.Load<SfxLibrarySO>("SfxLibrary");
            amb.libraryEntryId = "ambient_autumn_loop";

            Hh.Log(LogCategory.Audio,
                $"RuntimeAudioBootstrap: programmatic audio rig installed " +
                $"({BootstrapGameObjectName}, DontDestroyOnLoad). " +
                $"Music lib: {(existingMusic == null ? "loaded" : "kept existing")}; " +
                $"Mumble lib: {(existingMumble == null ? "loaded" : "kept existing")}. " +
                $"Re-run `Hearthbound → 🚀 Build Everything` to upgrade to the " +
                $"scene-baked Phase 38 rig for full inspector control.");
        }
    }
}
