# 📊 PROGRESS Supplement — Phases 62–67: The Cozy Daily Loop (Engagement Bible P2–P7)

> Companion to `Docs/PROGRESS.md`. Supplement file (repo convention, cf.
> `PROGRESS_Phase47_OneMoreDay.md`, `PROGRESS_Phase61_Engagement.md`). Full design rationale +
> the living ledger live in `Docs/Engagement_Bible/11_WIDER_CONTENT_EXECUTION_PLAN.md`.
> Branch: `feat/mission-1-2-architecture`.

---

## Summary

Built the **compounding cozy daily loop** the Engagement Review said was missing — turning the
0/7-vs-Stardew vertical slice into a game with a reason to wake up tomorrow. Six new player
screens + six new runtime services, all **self-installing, fallback-safe, asmdef-clean, and
Cozy-Contract-compliant**, reachable purely by pull + `🚀 Build Everything` + Play (no scene edits
required). Save schema → v3 so the loop **persists and compounds**.

## New runtime files

**Core (data/events/blackboards):**
- `VillageStateFlags.cs` — reflection string→bool gate resolver (D-079).
- `DayAgenda.cs` (+`RequestTicket`), `EngagementEvents.cs` (+RequestSelected/MemoryKept/CoinChanged/
  HollowPurchaseRequested/AlmanacEvent), `EchoBoard.cs`, `HollowShopBoard.cs`, `GardenBoard.cs`,
  `CraftEvents.cs` (+`CraftVerbs`).
- `VillageState.cs` — engagement-loop fields (`resolvedRequestIds`, `purchasedUpgradeIds`,
  `materials`, `completedEchoIds`, `keeperHandCraftCount`, `gardenBeds`+`GardenBedState`).
- `Save/VillageStateSnapshot.cs` — schema v3 round-trips all of the above (v1/v2 forward-compatible).

**Mission (services — own the mutations):**
- `RequestBoardService.cs` (faucet), `RequestVisitService.cs` (visit consequences),
  `EchoWebService.cs` (thread tally/complete), `HollowProgressionService.cs` (upgrades/coin),
  `GardenService.cs` (grow/brew/use), `WorkbenchService.cs` (tend/mastery), `AlmanacService.cs` (calendar).

**UI (presentational — read Core, publish intent):**
- `RequestBoardUI.cs` [B], `MemoryWallUI.cs` [M], `HollowShopUI.cs` [U], `CoinPurseHUD.cs`,
  `GardenUI.cs` [G], `WorkbenchUI.cs` [K]. `AgendaCardUI.cs` extended (almanac line + key map).

## The loop (one day)

Wake → Agenda (almanac · visitors · garden · Marin nudge · growth · key map) → choose:
[B] keep a memory (+coin) · [M] chase Echo threads · [U] grow the Hollow · [G] tend & brew ·
[K] tend kept memories (gentle mastery) → close the ledger on visible growth → tomorrow tease →
a different morning. Coin compounds: visits/echoes/teas → coin → upgrades & garden → capacity →
more memories.

## Cozy Contract / architecture audit

- No fail states, no timers, refusal & "not today" always honoured, Auto-Complete/skip everywhere,
  feedback is celebratory (coin purse, collection counts) never a deficit nag (D-076).
- asmdef graph preserved (D-035): data→Memory, services→Mission, screens→UI; **no UI→Mission edge**
  (UI reads Core blackboards + publishes intent on the EventBus; Mission services subscribe + mutate).
- Every new system works with **zero authored content** (built-in roster/echoes/catalog/herbs) and
  upgrades when an authored `Resources/` SO pool is added — **zero code change** to extend (D-081).

## Un-deferred content (Phase 68)

The procedural-villager texture the Depth Bible deferred now ships via the built-in 7-villager
Request roster + built-in Echo threads + built-in upgrade catalog + built-in herb table. Authored
`RequestPool` / `EchoPool` / `HollowCatalog` / `MemoryHerb` assets extend or override any of it with
no code change (the faucet/services prefer an authored pool when present).

## Decisions

D-079 (string-flag gating) · D-080 (companion Memory Wall screen) · D-081 (self-installing loop
screens bridged via Core blackboards + intent events).

## Verify after pull + 🚀 Build Everything + Play

1. Enter a gameplay scene → morning Agenda lists almanac/visitors/garden + the key map.
2. Press [B] → keep a memory → coin purse ticks up; [J]/[M] show the new memory/echo counts.
3. Press [G] → harvest/brew; [U] → buy the window shelf with earned coin; [K] → tend a memory.
4. End the day → Day 2's board differs; garden advanced; coin persisted across save/load.
5. `🔍 Diagnose Build` clean; zero NRE boot → menu → Day 1 → Day 2.

## Follow-ups (tracked)

- Evening-Ledger "The Hollow grew" section (Phase 69 polish on the scene-wired ledger).
- Optional Editor builder to author the `Resources/RequestPool`/`EchoPool`/`HollowCatalog` assets +
  pre-place hidden upgrade scene markers, chained into `🚀 Build Everything`.
- G-Engage playtest (`Docs/PHASE70_GENGAGE_PLAYTEST.md`) → tune (Phase 71).
