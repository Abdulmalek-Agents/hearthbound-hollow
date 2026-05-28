// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Core / GameManager
//
// Bootstrap singleton that survives across scenes (DontDestroyOnLoad).
// Responsibilities:
//   * Hold the canonical VillageState ScriptableObject reference
//   * Register itself + other services with the ServiceLocator
//   * Provide scene-transition helpers (SceneManager wrapper with Addressables
//     fallback hooks for Phase 3+)
//   * Boot the first scene (MainMenu) after bootstrap
//   * Phase 32 — auto-spawn HearthboundHollow.Audio.VoicePlayer if it has
//     not been wired into the Bootstrap scene. Belt-and-braces alongside
//     Phase 45's RuntimeAudioBootstrap: even a fresh clone with no audio
//     scene wiring gets a working VoicePlayer the moment GameManager wakes.

using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HearthboundHollow.Core
{
    [DefaultExecutionOrder(-1000)]
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                    Hh.Warn(LogCategory.Boot, "GameManager.Instance accessed before bootstrap.");
                return _instance;
            }
        }

        [Header("State")]
        [Tooltip("The single VillageState ScriptableObject — drag the asset here in the Bootstrap scene.")]
        public VillageState villageState;

        [Header("Scene names")]
        public string mainMenuSceneName = "01_MainMenu";
        public string bootstrapSceneName = "00_Bootstrap";

        [Header("Boot flow")]
        [Tooltip("If true, auto-load MainMenu after one frame of bootstrap.")]
        public bool autoLoadMainMenu = true;

        [Header("Audio (Phase 32 — Voice Acting MVP)")]
        [Tooltip("If true, auto-spawn HearthboundHollow.Audio.VoicePlayer as a " +
                 "child of GameManager when no VoicePlayer.Instance is present " +
                 "by GameManager.Awake. Belt-and-braces alongside the scene-baked " +
                 "rig + Phase 45 RuntimeAudioBootstrap.")]
        public bool autoSpawnVoicePlayer = true;

        public event Action<string> OnSceneLoadStarted;
        public event Action<string> OnSceneLoadCompleted;

        // ───── Lifecycle ─────────────────────────────────────────────

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Hh.Warn(LogCategory.Boot, "Duplicate GameManager found — destroying new instance.");
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            if (villageState == null)
            {
                Hh.Err(LogCategory.Boot, "GameManager has no VillageState assigned. Bootstrap aborted.");
                return;
            }

            ServiceLocator.Register<GameManager>(this);
            ServiceLocator.Register<VillageState>(villageState);

            EventBus.Publish(new VillageStateLoadedEvent(villageState));

            // Phase 32 — Voice Acting MVP: ensure a VoicePlayer exists. This
            // is a no-op when the Bootstrap scene already wires one (preferred),
            // and when Phase 45's RuntimeAudioBootstrap has already installed
            // the audio rig. Belt-and-braces for fresh clones where neither
            // path has run yet. The lookup is reflection-light: we use a fully
            // qualified type name so Core asmdef has zero compile-time dep on
            // Audio asmdef (Audio already references Core, not the other way
            // round — avoiding a cycle).
            if (autoSpawnVoicePlayer) TryAutoSpawnVoicePlayer();

            // Phase 32.18 — auto-spawn the URP material healer. Scans every
            // Renderer on Awake + every sceneLoaded and substitutes URP/Lit
            // for any material whose shader is null / Hidden/InternalErrorShader
            // / not-supported under the current pipeline. Kills the magenta
            // placeholder cubes that ship with packs whose Built-In shaders
            // didn't survive the URP migration. Same pattern as VoicePlayer
            // (Core asmdef hosts the type — no asmdef ref required).
            if (UrpMaterialHealer.Instance == null)
            {
                var go = new GameObject("_UrpMaterialHealer",
                                        typeof(UrpMaterialHealer));
                go.transform.SetParent(transform, false);
            }

            Hh.Log(LogCategory.Boot, $"GameManager bootstrapped. Day {villageState.currentDayIndex}, Coin {villageState.coin}.");
        }

        /// <summary>
        /// Spawn a HearthboundHollow.Audio.VoicePlayer GameObject under us if
        /// the Audio assembly is loaded and no VoicePlayer.Instance is set.
        /// Done via reflection so the Core asmdef does not need a reference
        /// to the Audio asmdef (which would create an asmdef cycle: Audio
        /// already references Core).
        /// </summary>
        private void TryAutoSpawnVoicePlayer()
        {
            // Find the VoicePlayer type via reflection — Audio asmdef is loaded
            // in the same Editor process / build because Mission01Director
            // (Mission asmdef → Audio asmdef) is referenced from the scene.
            var voicePlayerType = Type.GetType("HearthboundHollow.Audio.VoicePlayer, HearthboundHollow.Audio");
            if (voicePlayerType == null)
            {
                Hh.Log(LogCategory.Audio,
                    "GameManager: HearthboundHollow.Audio.VoicePlayer type not found " +
                    "(Audio asmdef not loaded). Skipping auto-spawn.");
                return;
            }

            // Check the static `Instance` property — if non-null, the rig is
            // already up (scene-baked or Phase 45 auto-installed).
            var instanceProp = voicePlayerType.GetProperty(
                "Instance",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            var existing = instanceProp != null ? instanceProp.GetValue(null) : null;
            if (existing != null)
            {
                Hh.Log(LogCategory.Audio,
                    "GameManager: existing VoicePlayer detected; skipping auto-spawn.");
                return;
            }

            var go = new GameObject("_VoicePlayer", voicePlayerType);
            go.transform.SetParent(transform, false);
            Hh.Log(LogCategory.Audio,
                "GameManager: auto-spawned _VoicePlayer (Phase 32 fallback). " +
                "For Inspector control, wire one into the Bootstrap scene.");
        }

        private void Start()
        {
            if (autoLoadMainMenu && SceneManager.GetActiveScene().name == bootstrapSceneName)
            {
                LoadScene(mainMenuSceneName);
            }
        }

        private void OnApplicationQuit()
        {
            ServiceLocator.Clear();
            EventBus.ClearAll();
            _instance = null;
        }

        // ───── Scene transitions ─────────────────────────────────────

        public void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Hh.Err(LogCategory.Boot, "LoadScene called with empty sceneName.");
                return;
            }

            Hh.Log(LogCategory.Boot, $"Loading scene '{sceneName}' (mode {mode}).");
            OnSceneLoadStarted?.Invoke(sceneName);

            var op = SceneManager.LoadSceneAsync(sceneName, mode);
            if (op == null)
            {
                Hh.Err(LogCategory.Boot, $"Scene '{sceneName}' not found in Build Settings.");
                return;
            }
            op.completed += _ =>
            {
                Hh.Log(LogCategory.Boot, $"Scene '{sceneName}' load completed.");
                OnSceneLoadCompleted?.Invoke(sceneName);
            };
        }

        public void LoadSceneAdditive(string sceneName) => LoadScene(sceneName, LoadSceneMode.Additive);

        public void UnloadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return;
            SceneManager.UnloadSceneAsync(sceneName);
        }

        /// <summary>Increment the day index and publish DayEndedEvent.</summary>
        public void EndDay()
        {
            if (villageState == null) return;
            int day = villageState.currentDayIndex;
            villageState.currentDayIndex = day + 1;
            EventBus.Publish(new DayEndedEvent(day));
            Hh.Log(LogCategory.Boot, $"Day {day} ended; advancing to day {day + 1}.");
        }
    }
}
