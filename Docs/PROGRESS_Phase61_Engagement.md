# 📊 PROGRESS — Phase 61: Engagement Review & Loop Foundation

> Companion to `Docs/PROGRESS.md` (this is the per-phase file, matching the repo's
> `PROGRESS_PhaseNN_*.md` convention). Newest sub-phase on top.
> **Engine:** Unity 6000.4.4f1 · **Branch:** `feat/mission-1-2-architecture`.

---

## Why this phase exists

Owner request: assign 20+ senior specialists to read the entire project, write a candid
critique of why the game is *boring and will not succeed*, design how to make it as engaging
and replayable as **Stardew Valley / cozy games**, produce deep content + step-by-step
implementation guidance, update key docs, remove junk docs, and push in phases.

**Finding:** the narrative-complete M1+M2 slice is a polished **75-minute linear corridor with
two no-fail mini-games** — it scores **0/7** against Stardew's seven retention engines (no
compounding loop, no player-set goals, no ownership, no visible progression, no calendar, no
variety, no "tomorrow"). Reject as product; greenlight as foundation. Full canon in
`Docs/Engagement_Bible/`.

---

## Phase 61.4 — P1 code scaffolding (the Living Day spine) 🟢 (2026-05-30)

### Files added (Core asmdef — inert, compile-safe, ZERO behaviour change)
| File | Role |
|---|---|
| `Assets/_Project/Scripts/Core/EngagementEvents.cs` (+.meta) | 6 new EventBus `readonly struct`s: `DayStartedEvent`, `AgendaReadyEvent`, `RequestResolvedEvent`, `HollowUpgradePurchasedEvent`, `EchoThreadCompletedEvent`, `MemorySortedEvent`. Kept separate from `GameEvents.cs` (clean diffs). |
| `Assets/_Project/Scripts/Core/DayAgenda.cs` (+.meta) | Transient per-day model (visitors / garden notes / Marin suggestion). Plain class, not an SO. |
| `Assets/_Project/Scripts/Core/DailyLoopService.cs` (+.meta) | The day-lifecycle owner (Waking→OpenDay→Evening→Asleep). Self-installs via `RuntimeInitializeOnLoad` (matches `RuntimeAudioBootstrap`), registers in `ServiceLocator`, subscribes to `VillageStateLoadedEvent`. `BeginDay()`/`EndDay()` exist but are **not yet called by any director** → inert. |

### Why this is safe to land now
- **Core asmdef only** (depends on `UnityEngine` + `Unity.TextMeshPro`); no new asmdef refs → D-035 satisfied, no cycle.
- Verified against the real APIs: `EventBus.Subscribe/Publish/Unsubscribe<T> where T:struct`, `ServiceLocator.Register/Get<T> where T:class`, `Hh.Log(LogCategory,…)`, `VillageState.currentDayIndex`.
- `GameEvents.cs` and `VillageState.cs` **deliberately untouched** this phase (no save-schema/serialization risk). New `VillageState` fields (`purchasedUpgradeIds`, `gardenBeds`, etc.) land additively in later phases per their system docs.
- `DailyLoopService` is inert: nothing calls `BeginDay`/`EndDay`, so the shipping M1/M2 flow is byte-for-byte unchanged at runtime.

### Follow-up (Phase 61.5 — next)
Wire P1 live: `AgendaCardUI` (clone `OneMoreDayCard` + D-068 raycast safety net),
`Phase61_DailyLoopBuilder` (idempotent; drops the Agenda card, sets `DayCycleManager.autoAdvance`),
route `GameManager.EndDay()` → `DailyLoopService.EndDay()` (single counter owner — **D-077**),
extend `EveningLedgerUI` into the celebratory recap, make `TomorrowTeaseSO` the day-closer.
Chain the builder into `🚀 Build Everything`.

---

## Phase 61.1–61.3 — The Engagement Bible (docs) 🟢 (2026-05-30)

Added `Docs/Engagement_Bible/`:
- `00_INDEX.md`, `01_CRITIQUE_WHY_IT_IS_BORING.md`, `02_ENGAGEMENT_MASTER_PLAN.md`, `03_THE_COZY_DAILY_LOOP.md`.
- `04`–`09` — per-system implementation guidance (Daily Loop, Request Board, Hollow Progression, Garden/Tea, Workbench variety, Codex/Echo meta) with paste-ready C# + acceptance criteria.
- `10_IMPLEMENTATION_ROADMAP.md` — phase plan (61→72) + dependency graph + decisions ledger + phase log.
- Root `ENGAGEMENT_REVIEW.md` pointer.
- `CLAUDE.md` updated (D-075/D-076 + Engagement Bible in the key map + P1→P7 priority).

### Housekeeping
- Removed `~$PERT_REVIEW_AR (1).docx` (Office temp lock file) and root `.DS_Store` (macOS junk).

---

## Decisions adopted

- **D-075** — Engagement Bible un-defers the loop-critical subset of Depth-Bible codices 04/08/09/10/13. Out-of-Scope Wall revised. Cozy Contract + no-dark-monetization remain inviolable.
- **D-076** — Cordray Principle relaxed to "no *anxiety-inducing* numbers." Cozy, opt-in, celebratory progression feedback is now required.
- **D-077** *(reserved, Phase 61.5)* — `DailyLoopService.EndDay()` is the single owner of `currentDayIndex`; `GameManager`/`MissionRunner` delegate (no double-increment).

---

*Last updated: 2026-05-30 — Phase 61.4. See `Docs/Engagement_Bible/10_IMPLEMENTATION_ROADMAP.md` for the full forward plan.*
