// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Core / DailyLoopService  (Engagement Pillar P1)
//
// Owns the in-game day lifecycle: Waking -> OpenDay -> Evening -> Asleep -> (next day).
// This is the spine of the cozy compounding loop (Docs/Engagement_Bible/04).
//
// LIVE as of Phase 61.5 + 61.6:
//   • BeginDay() is driven by AgendaCardUI when a gameplay scene loads (61.5) —
//     it builds the morning Agenda + fires DayStartedEvent / AgendaReadyEvent.
//   • EndDay() is the SINGLE owner of the day counter + DayEndedEvent (D-077, 61.6);
//     GameManager.EndDay() delegates here, so all three end-of-day callers
//     (MissionRunner, Mission01/02Director) funnel through one increment.
// Self-installs a DontDestroyOnLoad host (matching the established
// RuntimeAudioBootstrap / MissionAudioHooks self-install pattern).
//
// Depends only on HearthboundHollow.Core + UnityEngine -> no asmdef risk (D-035).

using System;
using UnityEngine;

namespace HearthboundHollow.Core
{
    public enum DayPhase { Waking, OpenDay, Evening, Asleep }

    [DisallowMultipleComponent]
    public class DailyLoopService : MonoBehaviour
    {
        public static DailyLoopService Instance { get; private set; }

        public DayPhase Phase { get; private set; } = DayPhase.Waking;
        public int DayIndex => _vs != null ? _vs.currentDayIndex : 0;
        public DayAgenda CurrentAgenda { get; private set; }
        public event Action<DayPhase> OnPhaseChanged;

        private VillageState _vs;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInstall()
        {
            if (Instance != null) return;
            var go = new GameObject("_HHDailyLoop");
            DontDestroyOnLoad(go);
            go.AddComponent<DailyLoopService>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            ServiceLocator.Register(this);
            EventBus.Subscribe<VillageStateLoadedEvent>(OnStateLoaded);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<VillageStateLoadedEvent>(OnStateLoaded);
            if (Instance == this) Instance = null;
        }

        private void OnStateLoaded(VillageStateLoadedEvent e) => _vs = e.VillageState as VillageState;

        /// <summary>
        /// Call when a gameplay (home/shop) scene finishes loading for a new day.
        /// Builds a fresh Agenda, fires DayStartedEvent (loop services populate the
        /// Agenda), then AgendaReadyEvent (the AgendaCardUI shows it).
        /// </summary>
        public void BeginDay(VillageState vs)
        {
            if (vs != null) _vs = vs;
            CurrentAgenda = new DayAgenda { dayIndex = DayIndex };
            SetPhase(DayPhase.Waking);
            EventBus.Publish(new DayStartedEvent(DayIndex));
            Hh.Log(LogCategory.Mission, $"[DailyLoop] Day {DayIndex} begins.");
            EventBus.Publish(new AgendaReadyEvent(DayIndex));
            SetPhase(DayPhase.OpenDay);
        }

        /// <summary>
        /// Call from the Evening Ledger confirm flow before the night transition.
        /// D-077: this is the SINGLE place the persistent day counter advances and
        /// the SINGLE publisher of DayEndedEvent. GameManager.EndDay() delegates here.
        /// The caller is responsible for the single-fire guard (see
        /// MissionRunner._endOfDayHandled / the directors' end-of-day guards).
        /// </summary>
        public void EndDay()
        {
            if (_vs == null) _vs = ServiceLocator.Get<VillageState>();
            int day = DayIndex;
            SetPhase(DayPhase.Evening);
            EventBus.Publish(new DayEndedEvent(day));          // single owner (D-077); audio layer listens
            if (_vs != null) _vs.currentDayIndex = day + 1;    // the compounding counter — advanced HERE only
            SetPhase(DayPhase.Asleep);
            Hh.Log(LogCategory.Mission, $"[DailyLoop] Day {day} ended; advancing to day {day + 1}.");
        }

        private void SetPhase(DayPhase p)
        {
            if (Phase == p) return;
            Phase = p;
            OnPhaseChanged?.Invoke(p);
        }
    }
}
