// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Core / DailyLoopService  (Engagement Pillar P1)
//
// Owns the in-game day lifecycle: Waking -> OpenDay -> Evening -> Asleep -> (next day).
// This is the spine of the cozy compounding loop (Docs/Engagement_Bible/04).
//
// SAFETY NOTE (Phase 61.4 scaffolding): this component is INERT until the rest of
// the loop is wired. It self-installs a DontDestroyOnLoad host (matching the
// established RuntimeAudioBootstrap / MissionAudioHooks self-install pattern) and
// subscribes to VillageStateLoadedEvent, but BeginDay()/EndDay() are not yet called
// by any director, so it introduces ZERO behaviour change to the shipping M1/M2
// slice. Wiring happens in the Phase 61.5 builder (see Docs/Engagement_Bible/04 §6
// and 10_IMPLEMENTATION_ROADMAP.md).
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
        /// Advances the persistent day counter exactly once. The caller is
        /// responsible for the single-fire guard (see MissionRunner._endOfDayHandled).
        /// </summary>
        public void EndDay()
        {
            SetPhase(DayPhase.Evening);
            EventBus.Publish(new DayEndedEvent(DayIndex));   // existing event; audio layer already listens
            if (_vs != null) _vs.currentDayIndex += 1;       // the compounding counter
            SetPhase(DayPhase.Asleep);
            Hh.Log(LogCategory.Mission, $"[DailyLoop] Day {DayIndex} reached; sleeping.");
        }

        private void SetPhase(DayPhase p)
        {
            if (Phase == p) return;
            Phase = p;
            OnPhaseChanged?.Invoke(p);
        }
    }
}
