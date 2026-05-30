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
two no-fail mini-games** — it scores **0/7** against Stardew's seven retention engines. Reject
as product; greenlight as foundation. Full canon in `Docs/Engagement_Bible/`.

---

## Phase 61.6 — The day flows through the loop (D-077) 🟢 (2026-05-30)

### What shipped
| File | Change |
|---|---|
| `Scripts/Core/DailyLoopService.cs` | `EndDay()` is now the **single owner** of `currentDayIndex` + `DayEndedEvent` (fetches `VillageState` via `ServiceLocator` if needed). Header updated: the service is LIVE. |
| `Scripts/Core/GameManager.cs` | `EndDay()` now **delegates** to `DailyLoopService.Instance.EndDay()` when present (always — it self-installs); legacy inline increment kept only as a no-service fallback. |
| `Scripts/UI/AgendaCardUI.cs` | **Wake-placement tuning:** the morning Agenda fires on the FIRST gameplay scene of each day (Lane/Hollow/Garden/Cottage), Lane skipped during first-play onboarding. |

### Why there's no double-increment (audited before editing)
The only writers of `currentDayIndex` / publishers of `DayEndedEvent` are `GameManager.EndDay()`
and `DailyLoopService.EndDay()`. **All three** end-of-day callers — `MissionRunner`,
`Mission01Director`, `Mission02Director` — invoke `gm.EndDay()`. So routing GameManager →
DailyLoopService makes the loop service the single funnel: exactly one increment, one event.
`DayEndedEvent` keeps the pre-increment day index (unchanged semantics; the audio layer's
listener is unaffected).

### Deliberate decision on `DayCycleManager.autoAdvance` (the deferred cosmetic part)
The 61.6 scope listed "optional `DayCycleManager.autoAdvance` mood-only." **Decision: NOT enabled
this phase — by design, not a punt.** Rationale:
1. **Asmdef boundary** — `DayCycleManager` lives in `HearthboundHollow.Player`; `DailyLoopService`
   lives in `Core`. Core must not reference Player (would reverse the dependency). A day/night
   visual driver belongs in a Player- or Mission-asmdef component, not Core.
2. **Authored lighting conflict** — the scenes use hand-authored Lumen + Phase 32 cozy URP
   volumes + Phase 47 autumn skybox/lighting. Auto-rotating the sun would fight that look. It is
   a cosmetic *art-lighting* task, not a *loop* task.
3. **The loop doesn't need it** — `DailyLoopService` already advances the **logical** day phase
   (Waking→OpenDay→Evening→Asleep). The visual sun cycle is pure mood and is split into a future
   dedicated lighting pass.

So the *substantive* day-cycle (the logical phases + the morning/evening bookends) is applied;
the *cosmetic* sun auto-rotation is intentionally sequenced to a lighting phase.

### How to validate (authored without an in-engine compile)
1. Pull → `Hearthbound → 🚀 Build Everything` → Play.
2. Day 1: enter the Hollow → Agenda card ("Day 1"). Finish the day at the Evening Ledger → sleep.
3. Day 2 (M2): wake in the Hollow → Agenda card now reads **"Day 2"** (the counter advanced through `DailyLoopService.EndDay()`).
4. Confirm the day advances **once** (no skip from Day 1 → Day 3). Report any console error.

### Follow-ups
- **Phase 62 (P2 — Request Board):** populates `DayAgenda.visitors` so the card lists real rotating villagers (the card already reads `CurrentAgenda`).
- Dedicated lighting phase: optional day/night visual cycle via a Player/Mission-asmdef driver.

---

## Phase 61.5 — Wire the Living Day live (the morning bookend) 🟢 (2026-05-30)

### What shipped
| File | Role |
|---|---|
| `Assets/_Project/Scripts/UI/AgendaCardUI.cs` (+.meta) | **The morning Agenda card** — self-installing, self-building parchment overlay. Calls `DailyLoopService.BeginDay()` once per in-game day and renders the day label + visitors + garden + a Marin nudge. |

### Why this was the right 61.5 scope
- The **evening** bookend already exists (`EveningLedgerUI` recap + `OneMoreDayCard` "tomorrow" tease). The **missing** beat was the **morning**. This adds it and makes `DailyLoopService` actually run — so the Living Day (P1) loop spine is now live end-to-end.

### Why the drop is safe
- **Self-installing** via `[RuntimeInitializeOnLoadMethod]` (mirrors `RuntimeAudioBootstrap`): **no scene edit, no Editor builder, no `Phase27_BuildEverything` change.** Runs on Play; nothing in Advanced.
- **Self-builds its canvas** mirroring `MiniGameTutorialUI`; TMP default font via `UIAutoFitText`. UI asmdef already references Core (no asmdef change, D-035).
- **Cozy Contract:** non-blocking, dismissable (button/Space/Enter/E/Esc/click), no pause, no fail, D-068 safety net.

---

## Phase 61.4 — P1 code scaffolding (the Living Day spine) 🟢 (2026-05-30)

Added (Core asmdef, compile-safe):
- `Scripts/Core/EngagementEvents.cs` (+meta) — 6 new EventBus structs.
- `Scripts/Core/DayAgenda.cs` (+meta) — the morning-agenda model.
- `Scripts/Core/DailyLoopService.cs` (+meta) — the day-lifecycle owner.
> Verified against the real `EventBus` / `ServiceLocator` / `Hh` / `VillageState` APIs.

---

## Phase 61.1–61.3 — The Engagement Bible (docs) 🟢 (2026-05-30)

Added `Docs/Engagement_Bible/` (`00`–`10`): critique, master plan, daily-loop design, six
per-system implementation-guidance docs (paste-ready C# + acceptance criteria), and the
roadmap. Root `ENGAGEMENT_REVIEW.md` pointer. `CLAUDE.md` updated (D-075/D-076). Removed junk
(`~$PERT_REVIEW_AR (1).docx`, root `.DS_Store`).

---

## Decisions adopted

- **D-075** — Engagement Bible un-defers the loop-critical subset of Depth-Bible codices 04/08/09/10/13. Out-of-Scope Wall revised. Cozy Contract + no-dark-monetization remain inviolable.
- **D-076** — Cordray Principle relaxed to "no *anxiety-inducing* numbers." Cozy, opt-in, celebratory progression feedback is now required.
- **D-077** *(Phase 61.6)* — `DailyLoopService.EndDay()` is the single owner of `currentDayIndex` + `DayEndedEvent`; `GameManager.EndDay()` delegates to it. No double-increment.

---

*Last updated: 2026-05-30 — Phase 61.6 (day flows through the loop). See `Docs/Engagement_Bible/10_IMPLEMENTATION_ROADMAP.md` for the full forward plan.*
