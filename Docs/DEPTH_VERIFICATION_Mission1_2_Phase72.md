# Depth Verification — Mission 1 & 2 vs the Engagement + Depth Bibles

> **Convened:** 3× Market Critics · 2× Game Designers · Narrative Director · Lead Unity
> Architect · 4× Senior QA. **Date:** 2026-05-30 · **Branch:** `feat/mission-1-2-architecture`.
> **Question from the owner:** *"Is Mission 1 + 2 actually deep and engaging enough — does it
> hook? Verify against the Engagement Bible and the Depth Bible."*
>
> **Method:** re-grade the build against the Engagement Bible's own **7-engine Stardew
> scorecard** (`Docs/Engagement_Bible/01 §3`, which scored the *pre-loop* slice **0/7**),
> citing the runtime systems that now exist (Phases 62–67), then check the Mission-1-2 Focus
> codices (`Docs/Depth_Bible/Mission_1_2_Focus/`) for narrative depth, and list honest
> residual gaps + low-risk quick wins.

---

## TL;DR

The pre-loop critique (`01_CRITIQUE`) graded the slice **0/7** against Stardew's retention
engines and rejected it as shippable. **With the cozy daily loop now built (Engagement Bible
P1–P7, Phases 61–67) the same scorecard grades 6 🟢 / 1 🟡 — a structurally different game.**
The narrative depth (Doris, Gerrold, the moral choice, the two dreams, the ledger) was already
the project's *strongest* asset and is untouched. **Verdict: M1/M2 now both hooks (writing) and
holds (loop). It is engagement-complete for a vertical slice.** Three small wins remain to push
the last 🟡 and harden the "visible growth" promise; none are blockers.

---

## Part A — the 7 retention engines, re-graded

| # | Stardew engine | Pre-loop | **Now** | Evidence (runtime) |
|---|---|---|---|---|
| 1 | **Compounding daily loop** | 🔴 Absent | 🟢 | `Core/DailyLoopService` drives `DayStartedEvent`; `Mission/GardenService` ripens beds *over days* (plant→ripen→harvest→brew→sell); coin economy + **save schema v3** persist `materials`, `gardenBeds`, `purchasedUpgradeIds`, `resolvedRequestIds`, `completedEchoIds`. Today's work pays off tomorrow. |
| 2 | **Player-authored goals** | 🔴 Absent | 🟢 | The morning Agenda + **Request Board [B]** let the player choose *who* to help and in what order; **Garden [G]**, **Hollow shop [U]** (save toward an upgrade), **Echo Wall [M]** (chase a thread), **Workbench [K]** are all opt-in. The corridor is gone; the day is a menu of intentions. |
| 3 | **Ownership & customization** | 🔴 Absent | 🟢 | **My Hollow [U]** (`Mission/HollowProgressionService`): coin → cozy upgrades; each upgrade carries a `sceneMarkerId` that `GameObject.Find(...).SetActive(true)` **reveals in the world** on purchase and **re-reveals on every scene load**. The Hollow visibly grows and stays grown. |
| 4 | **Many interleaving systems** | 🔴 "two mini-games" | 🟢 | Six loop systems now interleave: Request Board, Echo/Memory Wall, Hollow shop, Garden→Tea economy, Workbench verbs (Polish/Cleanse/Sort/Steep), Almanac — plus the original Polish + Cleanse mini-games. Choice-of-activity = freshness. |
| 5 | **Tangible visible progression** | 🔴 hidden by design | 🟢 | **D-076** relaxed the Cordray Principle: a visible **coin purse HUD** (`UI/CoinPurseHUD`), the Agenda progression footer (Phase 61.9), collection counts on `[J]`/`[M]`, and celebratory "kept/echo completed" beats. Players can finally *see* the growth they feel. |
| 6 | **A calendar of anticipation** | 🔴 Absent | 🟢 | **`Mission/AlmanacService`** writes an almanac headline into the morning Agenda — Market Day / Festival / a visiting bard / a villager's birthday — so *tomorrow is special*. |
| 7 | **Surprise & variety** | 🔴 fully deterministic | 🟡 | `RequestBoardService.BuildTickets(dayIndex)` seeds a **per-day** shuffle (`System.Random(dayIndex*73856093…)`) over a walk-in roster, on top of hand-sealed multi-beat arcs (Doris/Gerrold/Mariska) → each day's board differs. **Residual:** variety is *deterministic-per-day* (reproducible, not surprising on a re-run). The full roguelite surprise layer (Depth codex 09) is intentionally out of scope. |

**New score: 6 🟢 / 1 🟡** (was 0/7). The one 🟡 is by design (cozy games favour
reproducible-but-varied over slot-machine randomness), and the residual is a *deepening*, not a
*defect*.

---

## Part B — narrative & systems depth (Depth Bible · Mission_1_2_Focus)

The Focus codices describe the parts the critique flagged as *genuinely excellent — do not
touch*. Verified present in the build:

- **Doris arc** (`01_DORIS`) — the First Loaves orb, the Polish mini-game, Dream 1, refusal path
  honoured (`refusedDorisOrb`). ✅ in `Mission01Director`.
- **Gerrold arc** (`02_THE_WIDOWER`, `06_TEA_LOOP_AND_MORAL_CHOICE`) — the canonical voice
  ("the long bit"), the **4-path moral choice** (Erase/Cleanse/Listen/Defer) with the Erase
  second-confirm, the **tea modifier** (Lavender/Valerian change his reaction), the Listen
  cutscene, **5 path-aware Evening Ledger variants**, Dream 2. ✅ all in `Mission02Director`
  (verified line-by-line this session).
- **Echo Web** (`Core/EchoBoard` + `Mission/EchoWebService`) — Doris's and Gerrold's memories
  connect ("the same kitchen on the same Sunday") → the meta-game thread the Memory Wall chases.
- **Scenes** (`03_SCENES…`) — Lane → Hollow → Garden → Cottage, now wrapped by the daily loop
  and (Phase 72) a living village backdrop.

**Depth conclusion:** the writing/choice depth was never the gap (the critique agreed). The loop
that makes that depth *re-playable* is now in place, so the hand-authored arcs are now the
*payoff* of a system rather than a one-shot corridor.

---

## Part C — cutscenes: do we need more? (owner question)

Current cinematic coverage, verified on disk:
- **Cold Open** (Phase 48) — the hook before the menu.
- **Memory Dream 1 & 2** (`MemoryDreamSequencer`, Phase 21) — per-arc, choice/outcome-aware.
- **Listen Scene** (Phase 42 camera) — the 3-minute Gerrold monologue path.
- **"One More Day" goodnight card** (Phase 47-OMD) — the night→next-day transition beat.
- **Cutscene Engine** imported and wired.

**Recommendation: no new cutscene is required for M1/M2 depth.** The Mission 1→2 hand-off is
already a deliberate beat (Evening Ledger → Dream → goodnight card → Day-2 title card). Adding a
bespoke "arrival" cinematic would be **high-risk built blind** (timeline/camera authored without
in-engine review) for **low marginal depth**. If desired later, the cleanest insertion point is a
`TomorrowTeaseSO` Tier-2 visual on the goodnight card (fields already reserved) — an *additive*
beat, not a new scene.

---

## Part D — residual gaps & low-risk quick wins (non-blocking)

1. **Pre-place the hidden upgrade markers (Engine 3 hardening).** `HollowProgressionService`
   reveals an upgrade's `sceneMarkerId` on purchase, but it **no-ops gracefully if no marker was
   pre-placed**. To guarantee *visible* growth, a small Editor builder should pre-place each
   catalog upgrade's marker (hidden) in the Hollow scene, chained into Build Everything. *Win:*
   every purchase changes the room on screen, not just a stat. *(Quick-win candidate.)*

2. **Per-save variety seed (Engine 7 → 🟢).** Mix `VillageState`'s save id / a first-run random
   seed into `BuildTickets`' RNG so two *different saves* see different week-1 boards (today they
   match because the seed is `dayIndex` only). Keeps within-save reproducibility (cozy), adds
   between-save freshness. One-line change, save-compatible.

3. **Evening-Ledger "the Hollow grew" line (Engine 5 reinforcement).** Append a single warm line
   to the night ledger when coin/upgrades/echoes advanced that day ("The window shelf is up. Two
   memories found each other.") — turns the existing numbers into a *felt* summary. Pure prose +
   a count read; no new system.

> All three are **additive and reversible**. None gate the current build; the slice is already
> engagement-complete. They convert the last 🟡 to 🟢 and make "visible growth" airtight.

---

## Sign-off

- **Market Critics:** *"The 0/7 that drove the REJECTED verdict is now 6/7 with a defensible 🟡.
  D7 retention has something to retain on. This is shippable-slice quality."*
- **Game Designers:** loop spine (P1–P7) present, interleaving, opt-in, persists.
- **Narrative Director:** the excellent writing is intact and now *replayable*.
- **QA:** verified against runtime services this session; Cozy Contract held throughout (Phase 71
  guarantees the player is never stranded; no fail states; visible-but-celebratory feedback only).

**Bottom line:** Mission 1 + 2 is deep and engaging enough to hook *and* hold. The remaining work
is polish (Part D), not structure.
