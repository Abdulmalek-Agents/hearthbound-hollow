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

            Hh.Log(LogCategory.Boot, $"GameManager bootstrapped. Day {villageState.currentDayIndex}, Coin {villageState.coin}.");
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
