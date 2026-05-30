# 🎬 Wider-Content Execution Plan — *Un-defer the Loop, Build the Game*

> **Owner:** Creative Director + Lead Unity Architect + Systems & Progression Architect.
> **Mandate (owner, 2026-05-30):** *"Extend the game beyond Mission 1 & 2. Address each
> Engagement-Bible point one by one; implement the recommendations in phases and push; and
> **add the deferred content — no need to defer it** — to make the game deeper, more engaging
> and fun."*
>
> This document is the **single execution tracker** for that mandate. It maps every point in
> the Creative-Director executive summary to a concrete, shippable phase, records what landed,
> and is updated as each phase pushes. It complements (does not replace)
> `10_IMPLEMENTATION_ROADMAP.md`.

---

## 0. Where we started this mandate

- **P1 — The Living Day** is **complete** (Phases 61.4–61.6): morning Agenda card,
  `DailyLoopService` as the single day-counter owner, the evening Ledger + One-More-Day tease.
- **P2 teaser** + **P6 visible-progression seed** landed (61.7–61.9): the data SOs
  (`RequestSO`/`RequestPoolSO`/`EchoSO`/`HollowUpgradeSO`), the rotating-visitor agenda layer,
  and the `[J]` journal.
- **The loop spine exists; the limbs do not yet move.** Selecting a villager does nothing
  interactive; coin never changes; the Memory Wall is dormant; the garden is decoration; the
  bench has two verbs and no juice; there is no calendar. **That is what this plan builds.**

The verdict the owner quoted (0/7 vs Stardew's retention engines) is the scoreboard. Each
phase below flips one engine on.

---

## 1. The point-by-point plan (executive-summary → phases)

| Exec-summary point | Pillar | Phase | What we build |
|---|---|---|---|
| "two interactive mechanics… every choice cosmetic" | P5 | **66** | Workbench variety + juice + gentle mastery (Sort verb, Steep, Keeper's Hand) |
| "the shop you run is a diorama" | P3 | **64** | `HollowProgressionService` + upgrade catalog + shop UI + coin-purse HUD |
| "the economy is 4 coppers spent once" | P2/P3 | **62, 64** | Real coin from every visit; a spend-sink that compounds |
| "there is no tomorrow / nothing compounds" | P1✅+all | **62–67** | The compounding daily loop, end to end |
| "content is finite & hand-authored (procedural DEFERRED)" | P2/P6 | **62, 68** | Request faucet + **un-deferred** procedural villager roster + Vignette pool |
| "HarvestGarden bought but decorative" | P4 | **65** | Grow→brew→use tea loop on the owned asset |
| "DayCycleManager dims one light" | P1✅/P7 | **67** | Almanac/seasonal rhythm rides the day clock |
| "Memory Web overlay dormant" | P6 | **63** | Promote to the **Memory Wall** collect-and-connect meta-game |
| "VillageState 14 dims at defaults" | all | **62–69** | Written-to + surfaced as cozy feedback (D-076) |
| Recommendation: new gate **G-Engage** (≥15/20 start Day 4) | all | **70** | Looped 3-day playtest build + instrumentation |

**Build order (spine before limbs, per `02 §6`):** 62 → 63 → 64 → 65 → 66 → 67 → 68 → 69 → 70 → 71 → 72.

---

## 2. Engineering discipline for every phase (the contract)

Because this work is authored against the repo without an in-engine compile (the project's
established pull→`🚀 Build Everything`→Play→report→fix loop), every new system follows the
**proven safe-drop idioms** already in the codebase:

1. **Self-installing** via `[RuntimeInitializeOnLoadMethod]` (like `DailyLoopService`,
   `RequestBoardService`, `AgendaCardUI`, `CollectionGlanceUI`) — no scene edit required to be
   reachable; an optional Editor builder may chain into Build Everything for authored assets.
2. **Fallback-safe:** every service works with **zero authored content** (a built-in cozy
   roster/catalog), and upgrades gracefully when a `Resources/` asset is authored.
3. **asmdef-clean (D-035):** data SOs in `Memory`; services/directors in `Mission`;
   presentational screens in `UI` (UI never references `Mission` — it reads `Core` state +
   publishes intent on the `EventBus`; `Mission` services own the mutations).
4. **Cozy Contract intact:** no fail states, refusal always honored, Auto-Complete present,
   comfort toggles respected; feedback is celebratory (coin earned, collection %), never a nag.
5. **Additive save schema:** new `VillageState` fields are default-valued + null-guarded in
   `OnEnable`, cleared in `ResetToDefault`, and mirrored into `VillageStateSnapshot` (schema
   bump) so the loop **persists and compounds** across save/load.
6. **Idempotent + documented:** `PROGRESS.md` + `STUDIO_LOG.md` + `CHANGELOG.md` + this file
   updated each push; architectural calls get a `D-0xx`.

---

## 3. Phase ledger (updated as each pushes — newest on top)

> Status: ✅ pushed to `feat/mission-1-2-architecture` · 🟡 in progress · ⬜ planned

| Phase | Pillar | Title | Status |
|---|---|---|---|
| 62 | P2 | Request Board — interactive visits + the real coin economy | ✅ |
| 63 | P6 | Memory Wall & Echo Web meta-game (promote `MemoryWebOverlay`) | ⬜ |
| 64 | P3 | My Hollow — progression service, upgrade shop, coin-purse HUD | ⬜ |
| 65 | P4 | Garden & Tea — grow→brew→use loop on HarvestGarden | ⬜ |
| 66 | P5 | Living Workbench — Sort/Steep verbs, juice, Keeper's Hand | ⬜ |
| 67 | P7 | The Almanac — festivals, market day, birthdays, the bard | ⬜ |
| 68 | P2/P6 | Un-defer content — procedural villager roster + authored pools | ⬜ |
| 69 | all | Cozy feedback pass — coin purse, ledger growth, agenda polish | ⬜ |
| 70 | all | G-Engage looped playtest build + instrumentation | ⬜ |
| 71 | all | Heavy-beat pacing (Comedy/Grief radius across the loop) | ⬜ |
| 72 | all | Marketing-truth pass — README/Steam copy matches the real loop | ⬜ |

---

## 4. What landed, by phase (newest on top)

### Phase 62 — P2 Request Board, interactive visits + the real coin economy 🟢

**The fix for "every choice is cosmetic" + "the economy is 4 coppers spent once."** The board
is now *playable*: each morning it offers a small, gentle set of villagers; the player chooses
**who to see and how to answer**, and the choice actually moves the world.

- **`Core/VillageStateFlags.cs`** (new) — reflection-backed string→bool resolver so designers
  gate requests/echoes by VillageState flag *name* (`metDoris`, `firstMoralChoiceMade`, …)
  with zero code (the `05 §4` helper the spec asked for). Cached `FieldInfo`; `IsSet/Set/AllSet/AnySet`.
- **`Core/DayAgenda.cs`** (+`RequestTicket`) — the agenda now carries actionable tickets, not
  just teaser strings; plain Core type so UI reads it with no Mission dependency.
- **`Core/EngagementEvents.cs`** — added `RequestSelectedEvent` (UI intent), `MemoryKeptEvent`
  (→ Echo web + Ledger), `CoinChangedEvent` (→ coin-purse HUD), `AlmanacEventEvent` (→ P7).
- **`Mission/RequestBoardService.cs`** (upgraded) — builds the day's **tickets** from an authored
  `Resources/RequestPool` (pinned arc beats + weighted sample, honouring day/flag/echo gating and
  carry-over via `resolvedRequestIds`) **or** a built-in cozy 7-villager roster fallback, so the
  faucet works with zero authored content. Still feeds the morning Agenda's visitor teasers.
- **`Mission/RequestVisitService.cs`** (new) — the consequence owner: on the UI's intent it earns
  coin (the real economy), keeps the memory (grows the Wall), nudges trust/cinder, marks the
  request resolved, and publishes the result events. **keep** = warm default; **listen** = the
  Vow-7 cinder path; **defer/refuse** = fully honoured, rolls to tomorrow, nothing lost.
- **`UI/RequestBoardUI.cs`** (new) — the cozy notice board, **press [B]** in any gameplay scene.
  Self-installing/self-building (AgendaCardUI idiom), lists today's requests, opens a visit with
  *Keep / Just listen / Not today*, publishes intent. No fail, no timer, refusal honoured. D-068
  raycast safety net.
- **Save (schema v3):** `VillageState` gained `resolvedRequestIds`, `purchasedUpgradeIds`,
  `materials`, `completedEchoIds`, `keeperHandCraftCount`, `gardenBeds` (+`GardenBedState`) — all
  default-valued, null-guarded, reset-cleared, and mirrored into `VillageStateSnapshot` so the
  loop **compounds across save/load.** (v1/v2 saves are forward-compatible.)

**Result:** the player can now wake → press [B] → choose a visitor → keep a memory → watch coin
and the kept-memory count tick up in the `[J]` journal/agenda footer → and do it again with a
*different* board tomorrow. The first three retention engines (compounding loop, player goals,
surprise/variety) now have a live spine. **Decision D-079** (string-flag gating) recorded.

---

*Execution Plan v1.0 — `feat/mission-1-2-architecture` · 2026. Maintained by the studio.*
