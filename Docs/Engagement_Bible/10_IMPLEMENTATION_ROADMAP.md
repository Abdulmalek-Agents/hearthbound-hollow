# 🐙 System 10 — The Implementation Roadmap & Phase Log

> **Owner:** GitHub PM + Lead Unity Dev. **Implements:** the build order for Pillars P1–P7.
> This is both the **forward plan** (Phases 61 → 72) and the **canonical log** of the
> Engagement Bible work done on `feat/mission-1-2-architecture`.

---

## 1. Principles for every phase

1. **Each phase is independently shippable + idempotent** (heal-or-create, re-runnable).
2. **Each phase chains into `🚀 Build Everything`** (D-074 — single entry point) *or* self-installs at runtime (needs no builder).
3. **Each phase updates `PROGRESS.md`** (+ a `D-0xx` if it makes an architectural decision).
4. **Each phase preserves the Cozy Contract + no-dark-monetization** (Golden Rules 1 & 2).
5. **Build the spine before the limbs:** P1 → P2 → P6 → P3 → P4 → P5 → P7 (per `02 §6`).
6. **Reuse before you build:** most pillars are mostly *wiring assets/systems already in the repo* (HarvestGarden, DayCycleManager, MemoryWebOverlay, empty garden plots, Reading Nook seam, Phase 32 dressing).

---

## 2. The phase plan

| Phase | Pillar | Deliverable | Status |
|---|---|---|---|
| **61.1** | — | Engagement review: critique + master plan + daily-loop design (`00`–`03`); junk removal | ✅ Done |
| **61.2** | — | Master Plan + Cozy Daily Loop docs | ✅ Done |
| **61.3** | — | Per-system implementation-guidance docs (`04`–`09`) | ✅ Done |
| **61.4** | P1 | **Code scaffolding:** `EngagementEvents.cs`, `DayAgenda.cs`, `DailyLoopService.cs` (+ metas) — inert, compile-safe | ✅ Done |
| **61.5** | P1 | **Morning bookend live:** `AgendaCardUI` (self-installing, self-building) drives `DailyLoopService.BeginDay()` once per in-game day on Hollow entry. Evening recap + tease already existed (Ledger + `OneMoreDayCard`). | ✅ Done |
| **61.6** | P1 | **Day flows through the loop (D-077):** `GameManager.EndDay()` delegates to `DailyLoopService.EndDay()` (single counter owner); `AgendaCardUI` wakes on the first gameplay scene of the day. `DayCycleManager.autoAdvance` deliberately deferred to a lighting phase (asmdef + authored-lighting). | ✅ Done |
| **61.7** | P2/P3/P6 | **Data layer:** `RequestSO`, `RequestPoolSO`, `EchoSO`, `HollowUpgradeSO` (+metas) — pure-data contracts, inert until their services land. | ✅ Done |
| **61.8** | P2 | **Request Board (teaser layer):** `RequestBoardService` self-installs and fills the morning Agenda with **rotating villagers** each day (built-in roster or optional `Resources/RequestPool`). Feeds the card's visitor list; interactive visits = Phase 62. | ✅ Done |
| **61.9** | P6 | **Visible progression (D-076):** `CollectionGlanceUI` — a `[J]` journal (Day · Coin · Memories · Echoes) + an Agenda progression footer. | ✅ Done |
| **62** | P2 | `RequestSO`/`RequestPoolSO`, `RequestBoardService`, `RequestBoardUI`, generalize directors → `VisitDirector`, re-express Doris+Gerrold as data, `AlmanacSO` | ⬜ Next |
| **63** | P6 | `EchoSO`/`EchoWebService`, promote `MemoryWebOverlay` → Memory Wall, wire thread rewards | ⬜ |
| **64** | P3 | `HollowUpgradeSO` + catalog, `HollowProgressionService`, `HollowShopUI`, coin-purse HUD, pre-placed hidden upgrade markers, wire transactions→coin | ⬜ |
| **65** | P4 | `GardenBedState` + `GardenService`, swap to HarvestGarden staged-growth, teas-as-tools, seed/tea economy | ⬜ |
| **66** | P5 | Juice pass on Polish/Cleanse, `CraftVerb` + `acceptedVerbs`, Sort + Steep verbs, Keeper's Hand mastery flavor | ⬜ |
| **67** | P7 | Almanac festivals (Honey / Hearth), Market Day cart, birthdays, bard visits; redress scenes | ⬜ |
| **68** | P6+ | Author 4–6 procedural villagers via the Vignette Library; expand the Request pool | ⬜ |
| **69** | all | Cozy feedback pass (D-076): coin purse, collection %, agenda — all celebratory, comfort-gated | ⬜ |
| **70** | all | **G-Engage playtest build** — 3 looped days; instrument the "voluntary Day 4" metric | ⬜ |
| **71** | all | Tune from playtest; pace heavy beats (Comedy/Grief radius across the loop) | ⬜ |
| **72** | all | Marketing-truth pass: update README/Steam copy to match the now-real loop (kill the refund-driving mismatch) | ⬜ |

---

## 3. The dependency graph (what unblocks what)

```
  P1 Living Day (61.4–61.6 ✅)
     ├──► P2 Request Board (61.8 teaser ✅ → 62 interactive) ──► P7 Almanac/Festivals (67)
     │         └──► P6 Echo Wall (63) ◄── feeds new arcs back into P2
     ├──► P3 Hollow Progression (64) ◄── coin from P2 transactions
     └──► P4 Garden & Tea (65) ──► P5 Workbench variety (66, teas as tools)
```

P1 is the root. Nothing else can compound without "tomorrow." **P1 is complete (61.4–61.6); the
P2 teaser layer + P6 visible-progression seed landed in 61.7–61.9; the full interactive P2
(VisitDirector, Phase 62) is next.**

---

## 4. Per-phase Definition of Done (the QA gate each phase must pass)

- [ ] Compiles; existing EditMode tests still green.
- [ ] Chained into `🚀 Build Everything` (or self-installs at runtime); re-running Build Everything twice is a no-op (idempotent).
- [ ] The system's own acceptance criteria (in its `0X` doc) all pass.
- [ ] Cozy Contract intact: no fail states, refusal honored, Auto-Complete present, comfort toggles respected.
- [ ] `PROGRESS.md` updated; `STUDIO_LOG.md` entry; `CHANGELOG.md` bumped.

---

## 5. Risk register (engagement work)

| Risk | Mitigation |
|---|---|
| Loop dilutes the writing's emotional impact | Pace heavy beats with the Comedy/Grief radius (`03 §8`); hand-sealed villagers stay hand-sealed. |
| Scope creep (we're un-deferring a lot) | Strict pillar priority + each phase shippable; stop at G-Engage and validate before P7+. |
| Procedural content feels samey | Vignette Library quality bar; hand-sealed pins always present; weighting tuned. |
| Save schema churn (new `VillageState` fields) | Add fields additively, default-valued, null-guarded in `OnEnable` (existing pattern); bump save schema only when needed. |
| Idempotency regressions in scene builders | Follow the established `FindFirstObjectByType ?? create` + wipe-own-parent patterns; pre-place hidden markers rather than mutate shared scene objects. |
| Untested-in-engine code (authored without a Unity compile) | Keep drops additive + self-installing + compile-conservative; rely on the project's pull→Build Everything→Play→report→hotfix loop (the whole repo history works this way). |

---

## 6. Phase log (what landed, newest first)

### Phase 61.7–61.9 — Data layer + Request Board teasers + visible progression 🟢 (2026-05-30)
**61.7** — four pure-data SOs (`RequestSO`, `RequestPoolSO`, `EchoSO`, `HollowUpgradeSO`) materialize
the P2/P3/P6 contracts (inert until Phases 62–64). **61.8** — `RequestBoardService` (self-installing,
observer-only) fills the morning Agenda with rotating villager teasers each day, from an optional
`Resources/RequestPool` or a built-in cozy roster (honest scope: card teasers, not yet interactive
visits — that's Phase 62). **61.9** — `CollectionGlanceUI` adds a `[J]` journal (Day · Coin · Memories ·
Echoes) and the Agenda card gains a celebratory progression footer — the first delivery of D-076
(visible growth), directly answering the critique's Root Cause #2. All additive, self-installing,
no scene/builder/flow edits.

### Phase 61.6 — Day flows through the loop (D-077) 🟢 (2026-05-30)
`GameManager.EndDay()` now delegates to `DailyLoopService.EndDay()` (the single owner of
`currentDayIndex` + `DayEndedEvent`; legacy inline path kept only as a no-service fallback).
Audited: all three end-of-day callers (`MissionRunner`, `Mission01/02Director`) route through
`GameManager.EndDay()` → exactly one increment, one event. `AgendaCardUI` tuned to wake on the
first gameplay scene of each day (Lane skipped during first-play onboarding). `DayCycleManager.autoAdvance`
**deliberately deferred** to a lighting phase (Core can't reference Player's `DayCycleManager`;
auto-rotating the sun would fight authored Lumen/URP lighting; the loop only needs the logical
day phases). **P1 (the Living Day) is now complete end-to-end.**

### Phase 61.5 — Morning bookend live (the Living Day runs) 🟢 (2026-05-30)
Added `Scripts/UI/AgendaCardUI.cs` (+meta) — a self-installing, self-building parchment
**morning Agenda card** that calls `DailyLoopService.BeginDay()` once per in-game day on
Hollow entry and renders the day label + visitors + garden + a Marin nudge. The evening
bookend already existed (`EveningLedgerUI` recap + `OneMoreDayCard` tease), so the P1 loop
spine is now live end-to-end. **No scene edit, no builder, no `Phase27` change** (self-installs
on Play) → reachable purely via pull + `🚀 Build Everything` + Play. Did **not** touch
`GameManager.EndDay`/`DayCycleManager` (deferred to 61.6 / D-077). Authored without an
in-engine compile → validate via Play (enter the Hollow); report errors for the normal
pull→test→fix loop.

### Phase 61.4 — P1 code scaffolding 🟢 (2026-05-30)
Added (Core asmdef, inert, compile-safe, zero behaviour change):
- `Scripts/Core/EngagementEvents.cs` (+meta) — 6 new EventBus structs.
- `Scripts/Core/DayAgenda.cs` (+meta) — the morning-agenda model.
- `Scripts/Core/DailyLoopService.cs` (+meta) — the day-lifecycle owner (self-installing; `BeginDay` now driven by AgendaCardUI in 61.5).
> Verified against the real `EventBus` / `ServiceLocator` / `Hh` / `VillageState` APIs.
> `GameEvents.cs` and `VillageState.cs` deliberately **not** touched (no save-schema risk this phase).

### Phase 61.3 — System implementation-guidance docs 🟢 (2026-05-30)
`Docs/Engagement_Bible/04`–`09` — code-level specs for P1, P2/P7, P3, P4, P5, P6 with
paste-ready C# and acceptance criteria.

### Phase 61.2 — Master plan + daily loop 🟢 (2026-05-30)
`02_ENGAGEMENT_MASTER_PLAN.md` (7 pillars, new loop, priority, G-Engage gate),
`03_THE_COZY_DAILY_LOOP.md` (the day, beat by beat).

### Phase 61.1 — Critique + foundation 🟢 (2026-05-30)
`00_INDEX.md`, `01_CRITIQUE_WHY_IT_IS_BORING.md`. Removed junk
(`~$PERT_REVIEW_AR (1).docx`, root `.DS_Store`). Introduced **D-075** (un-defer the
loop-critical Depth-Bible subset) and **D-076** (relax the Cordray Principle to allow
cozy, opt-in progression feedback).

---

## 7. Decisions ledger (engagement work)

- **D-075** — The Engagement Bible un-defers the loop-critical subset of Depth-Bible codices 04/08/09/10/13. The Out-of-Scope Wall is revised; the Daily Loop, Request Board, Hollow Progression, Garden→Tea, Workbench variety, and Codex/Echo meta-game are in-scope. Cozy Contract + no-dark-monetization remain inviolable.
- **D-076** — The "Cordray Principle" is relaxed to "no *anxiety-inducing* numbers." Cozy, opt-in, celebratory progression feedback (coin, shop level via art, collection %, agenda) is now **required**. Heavy/emotional UI stays number-free.
- **D-077 (adopted, Phase 61.6)** — One owner of `currentDayIndex`: `DailyLoopService.EndDay()` is the single place the counter advances + the single `DayEndedEvent` publisher; `GameManager.EndDay()` delegates to it (no double-increment).

---

## 8. The finish line

When Phases 61–70 land, a player can:
*wake to an Agenda → choose their day → meet rotating villagers → keep & craft memories →
grow the garden → brew teas that matter → spend coin to grow their Hollow → watch the Memory
Wall fill → chase Echo threads toward the Marin mystery → close the ledger on visible growth →
and **want to wake up tomorrow.*** That is the game the store page already promises — and the
moment ≥15/20 testers voluntarily start Day 4, we have turned a 75-minute vignette into a
cozy home players live in.

---

*Roadmap v1.0 — `feat/mission-1-2-architecture` · 2026. End of the Engagement Bible.*
