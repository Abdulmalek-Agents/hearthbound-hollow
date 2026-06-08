# 👨‍💻 System 04 — The Living Day & Time (Pillar P1)

> **Owner:** Lead Unity Architect + Senior Unity Dev. **Implements:** `03 §1–§6`.
> **Goal:** turn `Day 1 → Day 2 → credits` into an **open, repeatable day cycle** with a
> morning Agenda, a free middle, an evening recap, and a tomorrow-tease — the skeleton every
> other pillar hangs on. **Lowest cost, highest structural impact → build this first.**

---

## 1. What exists already (reuse, don't rebuild)

| Existing | Where | How we use it |
|---|---|---|
| `DayCycleManager.cs` | `Scripts/Player/` | The clock. Already lerps sun/ambient by `dayProgress01` and broadcasts `OnTimeOfDayChanged`. We drive it instead of leaving it static. |
| `VillageState.currentDayIndex` | `Scripts/Core/VillageState.cs` | The persistent day counter. Already saved/loaded. |
| `DayEndedEvent(int DayIndex)` | `Scripts/Core/GameEvents.cs` | Already published at end of day; audio layer already listens. We add `DayStartedEvent`. |
| `EveningLedgerUI` + `OnEndOfDayConfirmed` | `Scripts/UI/`, `MissionRunner` | The evening beat. We extend its content, keep its plumbing. |
| `TomorrowTeaseSO` | `Scripts/Mission/` | The tomorrow-tease data. Currently underused; becomes the day-closer. |
| `GameManager.EndDay()` / `LoadScene()` | `Scripts/Core/` | Day-advance + scene routing already exist. |

**Net new code:** one Core service (`DailyLoopService`), one data model (`DayAgenda`), two
events, one UI (`AgendaCardUI`), and one Editor builder. That's it for the whole skeleton.

---

## 2. New events (add to a NEW file `Scripts/Core/EngagementEvents.cs` — never edit `GameEvents.cs`)

```csharp
// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Core / EngagementEvents
// New EventBus messages for the Engagement Bible loop (Phase 61+).
// Kept in a separate file so GameEvents.cs is never touched (clean diffs).
namespace HearthboundHollow.Core
{
    /// <summary>Fired once at the start of every in-game day, after VillageState is ready.</summary>
    public readonly struct DayStartedEvent
    {
        public readonly int DayIndex;
        public DayStartedEvent(int day) { DayIndex = day; }
    }

    /// <summary>Fired when the morning Agenda has been assembled and is ready for the UI.</summary>
    public readonly struct AgendaReadyEvent
    {
        public readonly int DayIndex;
        public AgendaReadyEvent(int day) { DayIndex = day; }
    }
}
```

> Why a separate file: `GameEvents.cs` is touched by many systems; adding to it risks merge
> noise and the asmdef stays identical (both files live in `HearthboundHollow.Core`).

---

## 3. `DailyLoopService` (the day's brain) — `Scripts/Core/DailyLoopService.cs`

A plain `MonoBehaviour` singleton in the **Core** asmdef (depends only on `UnityEngine` +
Core types → zero asmdef risk). It owns the day's lifecycle and is registered in
`ServiceLocator` so any system can query "what day is it / what phase are we in."

```csharp
// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Core / DailyLoopService  (Pillar P1)
using System;
using UnityEngine;

namespace HearthboundHollow.Core
{
    public enum DayPhase { Waking, OpenDay, Evening, Asleep }

    /// <summary>
    /// Owns the in-game day lifecycle: Waking → OpenDay → Evening → Asleep → (next day).
    /// Registered in ServiceLocator. Persists across scene loads (DontDestroyOnLoad).
    /// Idempotent: self-installs via RuntimeInitializeOnLoad if no scene rig exists.
    /// </summary>
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

        /// <summary>Call when a gameplay (home/shop) scene finishes loading for the day.</summary>
        public void BeginDay(VillageState vs)
        {
            _vs = vs;
            CurrentAgenda = new DayAgenda { dayIndex = DayIndex };
            SetPhase(DayPhase.Waking);
            EventBus.Publish(new DayStartedEvent(DayIndex));
            Hh.Log(LogCategory.Mission, $"[DailyLoop] Day {DayIndex} begins.");
            // RequestBoardService + GardenService listen for DayStartedEvent and populate
            // CurrentAgenda; then the AgendaCardUI shows on AgendaReadyEvent.
            EventBus.Publish(new AgendaReadyEvent(DayIndex));
            SetPhase(DayPhase.OpenDay);
        }

        /// <summary>Called by the EveningLedger flow (OnEndOfDayConfirmed) before the night transition.</summary>
        public void EndDay()
        {
            SetPhase(DayPhase.Evening);
            EventBus.Publish(new DayEndedEvent(DayIndex));   // existing event — audio already listens
            if (_vs != null) _vs.currentDayIndex += 1;       // the compounding counter
            SetPhase(DayPhase.Asleep);
            Hh.Log(LogCategory.Mission, $"[DailyLoop] Day {DayIndex} ends; advancing.");
        }

        private void SetPhase(DayPhase p)
        {
            if (Phase == p) return;
            Phase = p;
            OnPhaseChanged?.Invoke(p);
        }
    }
}
```

**Wiring note:** `GameManager.EndDay()` already increments and routes. Refactor so
`GameManager.EndDay()` calls `DailyLoopService.Instance?.EndDay()` (single source of truth),
or have `DailyLoopService.EndDay()` call into `GameManager`. Keep **one** owner of the counter
to avoid double-increment (the existing `_endOfDayHandled` guard in `MissionRunner` is the pattern).

---

## 4. The Agenda data + UI

### 4A — `DayAgenda` model — `Scripts/Core/DayAgenda.cs`
A plain class (built fresh each morning — no asset needed):

```csharp
// SPDX-License-Identifier: MIT
using System.Collections.Generic;
namespace HearthboundHollow.Core
{
    public sealed class DayAgenda
    {
        public int dayIndex;
        public string seasonLabel;                 // "Spire-Month, Day 4"
        public string moodLine;                    // "a bright cold morning"
        public readonly List<string> visitors = new();    // "Doris — something sweet to ask"
        public readonly List<string> gardenNotes = new(); // "Lavender — ready to harvest"
        public string marinSuggestion;             // the gentle self-goal nudge
    }
}
```

The `RequestBoardService` (System 05) and `GardenService` (System 07) **populate** this on
`DayStartedEvent`; `DailyLoopService` exposes `CurrentAgenda`.

### 4B — `AgendaCardUI` (Bamao parchment, dismissable)
Pattern-match the existing `OneMoreDayCard` / `MissionTitleCard` (CanvasGroup-on-host + the
D-068 `blocksRaycasts` safety net). On `AgendaReadyEvent`, fade in, list the agenda lines,
show one button `[ Open the day ]`, fade out, hand control back to the player.

> **Cozy guardrail (D-076):** no deadlines, no red text, no countdowns. Opportunity, not obligation.

---

## 5. Activating `DayCycleManager`

Today it's static (`autoAdvance = false`, used to dim one cutscene light). Change:
1. In each gameplay scene's builder, set `autoAdvance = true` and `secondsPerDay` to a cozy
   length (e.g. 900 — long enough that time never feels like pressure; the player ends the day
   manually via the Ledger, so the clock is *mood*, not a timer).
2. Subscribe the existing `OnTimeOfDayChanged` to ambient audio swaps (morning birdsong →
   evening crickets) via a tiny binder — the audio layer already has the channels.
3. **Never** let time-of-day gate or fail anything. It is atmosphere only (Cozy Contract).

---

## 6. Step-by-step build order (Phase 61.5 in the roadmap)

1. Add `Scripts/Core/EngagementEvents.cs` (2 events). Compile.
2. Add `Scripts/Core/DayAgenda.cs` + `Scripts/Core/DailyLoopService.cs`. Compile (Core-only deps → safe).
3. Refactor `GameManager.EndDay()` to delegate the counter to `DailyLoopService`. Run existing EditMode tests (`SaveAndRippleTests`, `CoreTests`).
4. Add `Scripts/UI/AgendaCardUI.cs` (clone `OneMoreDayCard` patterns + D-068 safety net).
5. New Editor builder `Scripts/Editor/Phase61_DailyLoopBuilder.cs`: drops `AgendaCardUI` onto the Hollow/shop scene canvas + sets `DayCycleManager.autoAdvance = true`. Idempotent (`FindFirstObjectByType ?? create`). (`_HHDailyLoop` auto-installs, no placement needed.)
6. **Chain it into `🚀 Build Everything`** (`Phase27_BuildEverything.cs` `TryRun(...)`) — per D-074 it must be reachable from the single entry point.
7. Update `Docs/PROGRESS.md` (phase entry + any `D-0xx`).

---

## 7. Acceptance criteria (QA contract)

- [ ] On entering the Hollow for a new day, the Agenda Card appears once and is dismissable.
- [ ] `VillageState.currentDayIndex` increments exactly once per Evening Ledger confirm (no double-count).
- [ ] `DayStartedEvent` and `DayEndedEvent` each fire exactly once per day (verify via `EventBus.SubscriberCount` test harness).
- [ ] The day cycle visibly changes light/ambient but **never** times out or fails the player.
- [ ] Re-running `🚀 Build Everything` twice produces no duplicate `AgendaCardUI` / loop rig.
- [ ] Loading a save resumes on the correct `currentDayIndex` with the correct Agenda.

---

## 8. Why this unlocks everything else

Once there is a **real morning and a real tomorrow**, every other pillar has somewhere to
live: the Request Board reshuffles *each morning*; the garden ripens *overnight*; coin earned
today buys a shelf that's there *tomorrow*; the Tomorrow Tease *pulls* the player into the
next `DayStartedEvent`. **P1 is the loop's spine. Build it first; the rest clips on.**

---

*System 04 v1.0 — Next: `05_SYSTEM_REQUEST_BOARD_AND_VISITORS.md`.*
