# 📊 PROGRESS — Phase 61: Engagement Review & Loop Foundation

> Companion to `Docs/PROGRESS.md` (this is the per-phase file, matching the repo's
> `PROGRESS_PhaseNN_*.md` convention). Newest sub-phase on top.
> **Engine:** Unity 6000.4.4f1 · **Branch:** `feat/mission-1-2-architecture`.

---

## Why this phase exists

Owner request: assign 20+ senior specialists to read the entire project, write a candid
critique of why the game is *boring and will not succeed*, design how to make it as engaging
and replayable as **Stardew Valley / cozy games**, produce deep content + step-by-step
implementation guidance, update key docs, remove junk docs, and push in phases — then keep
implementing the deferred content phase by phase.

---

## Phase 61.7–61.9 — Data layer + Request Board + visible progression 🟢 (2026-05-30)

All additive + compile-conservative; the SOs are inert data, the services are
self-installing observers, and nothing edits the existing M1/M2 flow.

### 61.7 — Data layer (P2/P3/P6 contracts) — `Memory` asmdef
| File | Role |
|---|---|
| `RequestSO.cs` (+meta) | A Request Board entry (villager + memory + teaser + gating + weight + pinned). |
| `RequestPoolSO.cs` (+meta) | Authored container of requests (optional; service falls back if absent). |
| `EchoSO.cs` (+meta) | An Echo thread (type + threshold + members + completion rewards) for the Memory Wall. |
| `HollowUpgradeSO.cs` (+meta) | A Hollow upgrade (category + cost + flavor + prereqs + scene marker + capacity). |
> Pure data; references existing `VillagerSO`/`MemoryNodeSO`. Inert until their services (Phases 62–64) consume them.

### 61.8 — Request Board service (P2) — `Mission` asmdef
| File | Role |
|---|---|
| `RequestBoardService.cs` (+meta) | Self-installing observer. On `DayStartedEvent` it appends **rotating villager teasers** to `DailyLoopService.CurrentAgenda.visitors` *before* the Agenda card renders, so each morning shows a different "who needs me today?". Uses an optional `Resources/RequestPool` asset, else a built-in cozy roster (zero-content-safe). |
> **Honest scope:** this feeds the **Agenda card's visitor list** (variety on the morning card). It does **not** yet spawn interactive visits — the playable day is still the scripted M1/M2 flow. The data-driven `VisitDirector` that turns a request into a full visit is **Phase 62**. Day-gating only (flag/echo gating lands with the VisitDirector) so it can never hide a needed beat.

### 61.9 — Visible progression (P6 / D-076) — `UI` asmdef
| File | Change |
|---|---|
| `CollectionGlanceUI.cs` (+meta) | A cozy on-demand **journal** toggled by **[J]**: Day · Coin · Memories kept · Echoes discovered. Hidden by default (no HUD overlap), self-installing + self-building (mirrors `AgendaCardUI`). Celebratory abundance counts only; D-068 safety net. **J is unused elsewhere** (Tab/H taken). |
| `AgendaCardUI.cs` | Compose() now appends a footer: *"Your Hollow so far: N memories · M echoes · C coin"* + *"(press J any time for your journal)"* — passive visible progression every morning + advertises the journal. |

### Why this directly answers the critique
Root Cause #2 was *all feedback is hidden*. 61.9 makes growth **visible** (D-076) — the morning
footer + the J journal — without anxiety-inducing numbers. 61.8 begins Root-Cause-#1/#4's fix
(content variety) by making the morning *different every day*.

### How to validate (authored without an in-engine compile)
1. Pull → `Hearthbound → 🚀 Build Everything` → Play.
2. Enter the Hollow → the Agenda card lists **rotating** villagers + a *"Your Hollow so far…"* footer.
3. Press **J** anytime in a gameplay scene → the **journal** panel shows Day/Coin/Memories/Echoes. J or Esc closes it.
4. Play Day 1 → Day 2: the morning visitors differ. Report any console error for the normal pull→test→fix loop.

### Follow-ups
- **Phase 62 (P2 full):** `VisitDirector` turns Request entries into interactive visits; author a real `RequestPool`; re-express Doris/Gerrold as data.
- **Phase 63 (P6):** `EchoWebService` + promote `MemoryWebOverlay` into the Memory Wall (uses `EchoSO`).
- **Phase 64 (P3):** `HollowProgressionService` + shop UI (uses `HollowUpgradeSO`); wire transactions → coin.

---

## Phase 61.6 — The day flows through the loop (D-077) 🟢 (2026-05-30)

`GameManager.EndDay()` delegates to `DailyLoopService.EndDay()` (single owner of `currentDayIndex`
+ `DayEndedEvent`; legacy inline path kept only as a no-service fallback). All three end-of-day
callers funnel through `GameManager.EndDay()` → one increment, one event. `AgendaCardUI` wakes on
the first gameplay scene of each day. `DayCycleManager.autoAdvance` deliberately deferred to a
lighting phase (asmdef boundary + authored-lighting conflict; the loop only needs logical day
phases). **P1 complete end-to-end.**

## Phase 61.5 — Morning bookend live 🟢 (2026-05-30)
`AgendaCardUI` — self-installing morning Agenda card that drives `DailyLoopService.BeginDay()`.
The evening bookend already existed (Evening Ledger + `OneMoreDayCard`).

## Phase 61.4 — P1 code scaffolding 🟢 (2026-05-30)
`EngagementEvents.cs`, `DayAgenda.cs`, `DailyLoopService.cs` (Core asmdef).

## Phase 61.1–61.3 — The Engagement Bible (docs) 🟢 (2026-05-30)
`Docs/Engagement_Bible/` (`00`–`10`): critique, master plan, daily-loop design, six per-system
implementation docs, roadmap. Root `ENGAGEMENT_REVIEW.md`. `CLAUDE.md` updated. Junk removed.

---

## Decisions adopted

- **D-075** — Engagement Bible un-defers the loop-critical subset of Depth-Bible codices 04/08/09/10/13. Cozy Contract + no-dark-monetization remain inviolable.
- **D-076** — Cordray Principle relaxed to "no *anxiety-inducing* numbers." Cozy, opt-in, celebratory progression feedback is now required (61.9 delivers the first of it).
- **D-077** *(Phase 61.6)* — `DailyLoopService.EndDay()` is the single owner of `currentDayIndex` + `DayEndedEvent`; `GameManager.EndDay()` delegates.

---

*Last updated: 2026-05-30 — Phase 61.9. See `Docs/Engagement_Bible/10_IMPLEMENTATION_ROADMAP.md` for the full forward plan.*
