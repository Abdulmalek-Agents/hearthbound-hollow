// =============================================================================
// LevelProgressionService.cs — Hearthbound Hollow
// Manages the 30-mission progression gate. Decides what the player can access.
// Phase 76 — 30-Mission Architecture.
// =============================================================================
using System.Collections.Generic;
using UnityEngine;
using HearthboundHollow.Core;
using HearthboundHollow.Memory;

namespace HearthboundHollow.Mission
{
    /// <summary>
    /// Central authority on:
    ///   - Which mission the player is currently on.
    ///   - Which missions are unlocked.
    ///   - Act transitions.
    ///   - Season transitions.
    ///
    /// Register this with ServiceLocator on game start.
    /// Phase 76.
    /// </summary>
    public class LevelProgressionService : MonoBehaviour
    {
        // ─── Constants ───────────────────────────────────────────────────────

        private const int TotalMissions = 30;
        private const int Act2Start     = 10; // mission index 10 = M11
        private const int Act3Start     = 20; // mission index 20 = M21

        // ─── Cached refs ─────────────────────────────────────────────────────

        private VillageState               _state;
        private List<NarrativeLevelConfigSO> _configs = new();

        // ─── Unity lifecycle ─────────────────────────────────────────────────

        private void Awake()
        {
            ServiceLocator.Register(this);
            _state = ServiceLocator.Get<VillageState>();

            // Load all 30 configs from Resources/Missions/.
            var loaded = Resources.LoadAll<NarrativeLevelConfigSO>("Missions");
            foreach (var c in loaded)
                _configs.Add(c);
            _configs.Sort((a, b) => a.missionIndex.CompareTo(b.missionIndex));

            Debug.Log($"[LevelProgressionService] Loaded {_configs.Count} mission configs.");
        }

        // ─── Public API ───────────────────────────────────────────────────────

        public int CurrentMissionIndex => _state?.currentMissionIndex ?? 0;
        public int CurrentAct         => _state?.currentAct ?? 1;

        public NarrativeLevelConfigSO GetCurrentConfig()
        {
            int idx = CurrentMissionIndex;
            return idx < _configs.Count ? _configs[idx] : null;
        }

        public NarrativeLevelConfigSO GetConfig(int missionIndex)
        {
            if (missionIndex < 0 || missionIndex >= _configs.Count) return null;
            // Find by index in case they're not densely packed.
            foreach (var c in _configs)
                if (c.missionIndex == missionIndex) return c;
            return null;
        }

        public bool IsMissionUnlocked(int missionIndex)
        {
            if (_state == null) return missionIndex == 0;
            return missionIndex <= _state.currentMissionIndex;
        }

        /// <summary>Called when a mission is completed to advance state.</summary>
        public void AdvanceMission()
        {
            if (_state == null) return;

            int next = _state.currentMissionIndex + 1;
            if (next >= TotalMissions)
            {
                Debug.Log("[LevelProgressionService] All 30 missions complete!");
                return;
            }

            _state.currentMissionIndex = next;
            _state.currentDayIndex     = 0;

            // Check act transitions.
            int newAct = next >= Act3Start ? 3 : next >= Act2Start ? 2 : 1;
            if (newAct != _state.currentAct)
            {
                _state.currentAct = newAct;
                EventBus.Publish(new ActTransitionEvent { NewAct = newAct });
            }

            // Check season transitions (every 7-8 missions roughly).
            UpdateSeason(next);
        }

        private void UpdateSeason(int missionIndex)
        {
            Season season;
            if      (missionIndex < 10) season = Season.Autumn;
            else if (missionIndex < 18) season = Season.Winter;
            else if (missionIndex < 26) season = Season.Spring;
            else                        season = Season.Summer;

            if (season != _state.currentSeason)
            {
                _state.currentSeason = season;
                EventBus.Publish(new SeasonChangedEvent { NewSeason = season });
            }
        }

        public float GetProgressPercent() =>
            TotalMissions > 0 ? (float)CurrentMissionIndex / TotalMissions : 0f;

        public bool IsGameComplete() => CurrentMissionIndex >= TotalMissions;
    }
}
