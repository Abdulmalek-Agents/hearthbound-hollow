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

## Phase 61.5 — Wire the Living Day live (the morning bookend) 🟢 (2026-05-30)

### What shipped
| File | Role |
|---|---|
| `Assets/_Project/Scripts/UI/AgendaCardUI.cs` (+.meta) | **The morning Agenda card** — self-installing, self-building parchment overlay. On Hollow entry (after a short delay so it never overlaps the scene title card), it calls `DailyLoopService.BeginDay()` (firing `DayStartedEvent` + `AgendaReadyEvent`) and renders the day label + visitors + garden + a Marin margin-note nudge. Once per in-game day. |

### Why this is the right 61.5 scope
- The **evening** bookend already exists (`EveningLedgerUI` recap + `OneMoreDayCard` "tomorrow" tease). The **missing** beat was the **morning**. This adds it and makes `DailyLoopService` (Phase 61.4) actually run — so the Living Day (P1) loop spine is now live end-to-end (morning Agenda → day → evening ledger → goodnight tease).

### Why this drop is safe (no compile/scene risk)
- **Self-installing** via `[RuntimeInitializeOnLoadMethod]` (mirrors `RuntimeAudioBootstrap` / `MissionAudioHooks`): **no scene edit, no Editor builder, no `Phase27_BuildEverything` change.** It runs on Play. So after a pull, the workflow is unchanged: `🚀 Build Everything` → Play. Nothing in Advanced; nothing extra to click.
- **Self-builds its canvas** mirroring the proven `MiniGameTutorialUI` idiom (`CreateOverlayCanvas` + `MakeLabel`); TMP default font via `UIAutoFitText`. UI asmdef already references Core → `DailyLoopService`/`DayAgenda`/`VillageState`/`EventBus` in scope (no asmdef change, D-035).
- **Cozy Contract:** non-blocking, dismissable (button OR Space/Enter/E/Esc/click), no `timeScale` pause, no fail state, no countdown. D-068 safety net (a transparent card never strands `blocksRaycasts`). Legacy `Input.GetKeyDown` used — verified active (the same API `OneMoreDayCard`/`MiniGameTutorialUI` rely on).
- **Deliberately NOT touched this phase:** `GameManager.EndDay` (the `currentDayIndex` owner) and `DayCycleManager.autoAdvance` — deferred to **Phase 61.6 (D-077)** so they can be Play-tested before a counter-ownership refactor.

### How to validate (in-engine — this was authored without a Unity compile)
1. Pull → open in Unity → `Hearthbound → 🚀 Build Everything` → Play.
2. Walk into the Hollow (M1) or wake there (M2). After ~1.4 s a parchment **Agenda card** fades in: *"Spire-Month · Day N — …"*, who's at your door, a Marin nudge.
3. Dismiss with the **Open the day** button or any of Space/Enter/E/Esc/click. It shows **once per in-game day**.
4. Report any console error/visual issue → hotfix (the project's normal pull→test→fix loop).

### Known follow-ups
- **Phase 61.6:** route `GameManager.EndDay()` → `DailyLoopService.EndDay()` (single counter owner, **D-077**); optionally enable `DayCycleManager.autoAdvance` as mood-only.
- **Phase 62 (P2):** the Request Board service will populate `DayAgenda.visitors`, replacing the baseline visitor line automatically (the card already reads `CurrentAgenda`).
- Placement tuning: morning card currently fires on Hollow entry; per-day "wake" placement is refined once the day-advance routing (61.6) lands.

---

## Phase 61.4 — P1 code scaffolding (the Living Day spine) 🟢 (2026-05-30)

### Files added (Core asmdef — inert, compile-safe, ZERO behaviour change)
| File | Role |
|---|---|
| `Assets/_Project/Scripts/Core/EngagementEvents.cs` (+.meta) | 6 new EventBus `readonly struct`s: `DayStartedEvent`, `AgendaReadyEvent`, `RequestResolvedEvent`, `HollowUpgradePurchasedEvent`, `EchoThreadCompletedEvent`, `MemorySortedEvent`. Kept separate from `GameEvents.cs` (clean diffs). |
| `Assets/_Project/Scripts/Core/DayAgenda.cs` (+.meta) | Transient per-day model (visitors / garden notes / Marin suggestion). Plain class, not an SO. |
| `Assets/_Project/Scripts/Core/DailyLoopService.cs` (+.meta) | The day-lifecycle owner (Waking→OpenDay→Evening→Asleep). Self-installs via `RuntimeInitializeOnLoad`, registers in `ServiceLocator`, subscribes to `VillageStateLoadedEvent`. `BeginDay()` is now called live by `AgendaCardUI` (61.5); `EndDay()` awaits the 61.6 routing. |

### Why this was safe to land
- **Core asmdef only** (depends on `UnityEngine` + `Unity.TextMeshPro`); no new asmdef refs → D-035 satisfied, no cycle.
- Verified against the real APIs: `EventBus.Subscribe/Publish/Unsubscribe<T> where T:struct`, `ServiceLocator.Register/Get<T> where T:class`, `Hh.Log(LogCategory,…)`, `VillageState.currentDayIndex`.
- `GameEvents.cs` and `VillageState.cs` **deliberately untouched** (no save-schema/serialization risk). New `VillageState` fields (`purchasedUpgradeIds`, `gardenBeds`, etc.) land additively in later phases per their system docs.

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
- **D-077** *(reserved, Phase 61.6)* — `DailyLoopService.EndDay()` is the single owner of `currentDayIndex`; `GameManager`/`MissionRunner` delegate (no double-increment).

---

*Last updated: 2026-05-30 — Phase 61.5 (Living Day live). See `Docs/Engagement_Bible/10_IMPLEMENTATION_ROADMAP.md` for the full forward plan.*
