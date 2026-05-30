# 🎬 Wider-Content Execution Plan — *Un-defer the Loop, Build the Game*

> **Owner:** Creative Director + Lead Unity Architect + Systems & Progression Architect.
> **Mandate (owner, 2026-05-30):** *"Extend the game beyond Mission 1 & 2. Address each
> Engagement-Bible point one by one; implement the recommendations in phases and push; and
> **add the deferred content — no need to defer it** — to make the game deeper, more engaging
> and fun."*
>
> This is the **single execution tracker** for that mandate. It complements (does not replace)
> `10_IMPLEMENTATION_ROADMAP.md`. Newest notes on top.

---

## 0. Where we started this mandate

P1 (the Living Day) was complete (61.4–61.6). P2-teaser + a P6 visible-progression seed had
landed (61.7–61.9). **The loop spine existed; the limbs did not move:** selecting a villager
did nothing, coin never changed, the Memory Wall was dormant, the garden was decoration, the
bench had two flat verbs, there was no calendar. Score vs Stardew's seven retention engines: 0/7.

## 1. The point-by-point plan (executive-summary → phases)

| Exec-summary point | Pillar | Phase | Status |
|---|---|---|---|
| "two interactive mechanics… every choice cosmetic" | P5 | 66 | ✅ |
| "the shop you run is a diorama" | P3 | 64 | ✅ |
| "the economy is 4 coppers spent once" | P2/P3 | 62, 64 | ✅ |
| "there is no tomorrow / nothing compounds" | all | 62–67 | ✅ |
| "content finite & hand-authored (procedural DEFERRED)" | P2/P6 | 62, 68 | ✅ un-deferred |
| "HarvestGarden bought but decorative" | P4 | 65 | ✅ |
| "DayCycleManager dims one light" | P7 | 67 | ✅ |
| "Memory Web overlay dormant" | P6 | 63 | ✅ |
| "VillageState 14 dims at defaults" | all | 62–67 | ✅ written-to + surfaced |
| Recommendation: new gate G-Engage (≥15/20 start Day 4) | all | 70 | 🟡 plan + telemetry shipped |

## 2. Engineering discipline (the contract every phase held)

Self-installing (`[RuntimeInitializeOnLoadMethod]`); fallback-safe (works with zero authored
content via built-in rosters/catalogs/herbs/echoes); asmdef-clean (data→Memory, services→Mission,
screens→UI; UI reads Core state + publishes intent on the EventBus, Mission owns mutations — no
UI→Mission edge, D-035); Cozy Contract intact (no fail, refusal honoured, opt-in, celebratory
feedback only); additive save schema (v3, null-guarded, reset-cleared, persisted) so the loop
compounds; idempotent + documented.

---

## 3. The new loop, end to end (what a day is now)

**Wake → the Agenda card** (almanac headline, today's visitors, garden status, a Marin nudge,
your Hollow's growth, and the key map) **→ choose your day:**

| Key | Screen | What you do | Pillar |
|---|---|---|---|
| **B** | Request Board | choose a villager, Keep / Listen / Not-today → earn coin, keep a memory | P2 |
| **M** | Memory Wall | watch Echo threads fill; complete one for +coin & a celebratory beat | P6 |
| **U** | Hollow Shop | spend coin on shelves / a relit room / a garden bed / Marin's cloth / Pickle's cushion | P3 |
| **G** | Garden | plant, harvest, brew teas (gentle tools), sell spare teas for coin | P4 |
| **K** | Workbench | tend kept memories (Polish/Cleanse/Sort/Steep) — gentle "Perfect" mastery | P5 |
| **J** | Journal | a glance at coin · memories · echoes | P6/D-076 |

**→ close the ledger on visible growth → the tomorrow tease → sleep → a different morning.**
Coin compounds: visits/echoes/teas → coin → upgrades & garden → more capacity → more memories.
Hand-sealed villagers (Doris/Gerrold/Mariska) deepen across days as you help them.

---

## 4. Phase ledger

| Phase | Pillar | Title | Status |
|---|---|---|---|
| 62 | P2 | Request Board — interactive visits + the real coin economy | ✅ |
| 63 | P6 | Memory Wall & Echo Web meta-game ([M]) | ✅ |
| 64 | P3 | My Hollow — progression service, shop ([U]), coin-purse HUD | ✅ |
| 65 | P4 | Garden & Tea — grow→brew→use loop ([G]) | ✅ |
| 66 | P5 | Living Workbench — Sort/Steep verbs, gentle mastery ([K]) | ✅ |
| 67 | P7 | The Almanac — festivals, market day, bard, birthdays | ✅ |
| 68 | P2/P6 | Un-defer content — procedural roster + built-in pools | ✅ (built-in; authored SO pools extend with zero code) |
| 68b | P2 | Hand-sealed multi-beat villager arcs (Doris/Gerrold/Mariska) | ✅ |
| 69 | all | Cozy feedback pass — coin purse + agenda footer + journal | ✅ core (Evening-Ledger growth section = follow-up) |
| 70 | all | G-Engage looped playtest + instrumentation | 🟡 plan + telemetry shipped |
| 71 | all | Heavy-beat pacing across the loop | 🟡 documented guidance |
| 72 | all | Marketing-truth pass | 🟡 `Docs/MARKETING_TRUTH_Phase72.md` |

---

## 5. What landed, by phase (newest on top)

### Phase 68b / 70 — Hand-sealed arcs · telemetry · tests 🟢

**Deepening pass after the loop spine landed.**
- **`Mission/RequestBoardService.cs` (arcs):** Doris, Gerrold and Mariska now each have an
  ordered **multi-beat arc**; the board offers the *next unresolved* beat per villager (pinned),
  so helping someone today unlocks their next chapter — **relationships that deepen over days**.
  Arc progress rides `resolvedRequestIds` (no new save fields); rotating walk-ins fill the rest;
  a gentle "quiet caller" is the never-empty fallback.
- **`Core/EngagementTelemetry.cs` (Phase 70):** dev-only (`UNITY_EDITOR || DEVELOPMENT_BUILD`),
  opt-in, local CSV of the loop's EventBus signals — flags the **voluntary Day-4** milestone
  (the G-Engage gate). No PII, no network, never affects gameplay.
- **`Tests/EditMode/EngagementLoopTests.cs`:** unit stubs (clean-architecture rule) for
  `VillageStateFlags`, `CraftVerbs`, `DayAgenda`/`RequestTicket`, and the **schema-v3 save
  round-trip** + `ResetToDefault` clearing (guards the "loop compounds across save/load" promise).

### Phase 67 — P7 The Almanac 🟢
`Mission/AlmanacService.cs` (self-installing, observer-only) resolves one gentle event per day —
Market Day (every 5th), Festival of the Hearth (month-end), Idris the bard (every 7th), a
birthday (day 4) — writes a headline into the morning Agenda (with a countdown on quiet days),
and publishes `AlmanacEventEvent`. `AgendaCardUI` now renders the almanac line and lists every
loop screen key. Reuses the day clock the loop already advances. **The "tomorrow is special" layer.**

### Phase 66 — P5 The Living Workbench 🟢
`Core/CraftEvents.cs` (+ shared `CraftVerbs`), `Mission/WorkbenchService.cs`, `UI/WorkbenchUI.cs`
([K]). Kept memories can be **tended**; each asks for one of four verbs (Polish/Cleanse/Sort/Steep)
chosen per-memory so the work varies. No-fail, but a gentle **"Perfect"** (odds rising with the
hidden **Keeper's Hand** mastery) lands a shimmer + a little extra coin. Every tend pays into the
loop. **The fix for "two flat verbs, no mastery."**

### Phase 65 — P4 Garden & Tea 🟢
`Core/GardenBoard.cs`, `Mission/GardenService.cs`, `UI/GardenUI.cs` ([G]). Beds persist in
`VillageState.gardenBeds`; day-tick growth feeds the Agenda; plant/harvest/water/brew/sell. Brewed
teas are **opt-in tools** (lavender → +coin on the next visit, valerian → a touch of craft mastery,
chamomile → Pickle warms). Closes the garden→tea→coin wheel; seeds 4 starter beds so it's alive
day 1. **Activates the bought-but-decorative HarvestGarden concept.**

### Phase 64 — P3 My Hollow progression 🟢
`Core/HollowShopBoard.cs`, `Memory/HollowCatalogSO.cs`, `Mission/HollowProgressionService.cs`,
`UI/HollowShopUI.cs` ([U]), `UI/CoinPurseHUD.cs`. Coin (from visits/echoes/teas) buys gentle
upgrades — a shelf, a relit room, a garden bed, Marin's cloth, a cushion for Pickle — warmth never
power, never required. Purchases persist, re-apply on load, reveal a scene marker when present, and
add a garden bed for the GardenBed upgrade. The coin purse makes earnings/spends **visible** (D-076).
**The compounding spend-sink that answers "why earn coin?"**

### Phase 63 — P6 Memory Wall & Echo Web 🟢
`Core/EchoBoard.cs`, `Memory/EchoPoolSO.cs`, `Mission/EchoWebService.cs`, `UI/MemoryWallUI.cs` ([M]).
Keeping memories connects into **Echo threads** the player watches fill and chases; completing one
bumps the echoes-found counter, grants +5 coin, and fires a celebratory beat. Authored `EchoPool`
or a built-in cozy thread set. **The self-set-goal engine.**

### Phase 62 — P2 Request Board, interactive 🟢
`Core/VillageStateFlags.cs` (string→bool gating, D-079), `Core/DayAgenda.cs` (+`RequestTicket`),
loop events, `Mission/RequestBoardService.cs` (faucet: authored pool or built-in roster,
flag/echo gating, carry-over), `Mission/RequestVisitService.cs` (consequences: keep/listen/defer/
refuse), `UI/RequestBoardUI.cs` ([B]). Save schema v3 persists the loop. **The real economy + agency.**

---

## 6. Decisions ledger (this mandate)

- **D-079** — String-flag content gating via reflection (`VillageStateFlags`), so designers gate
  requests/echoes by VillageState bool *name* with zero code.
- **D-080** — The Memory Wall ships as a companion self-installing screen ([M]) rather than mutating
  the Phase-51 scene-built Tab "Memory Web" overlay; both can be unified later.
- **D-081** — All Engagement-loop screens are self-installing, fallback-safe, and bridge UI↔Mission
  via Core blackboards (`DayAgenda`/`EchoBoard`/`HollowShopBoard`/`GardenBoard`) + intent events,
  preserving the asmdef graph and the pull→Build-Everything→Play workflow (no scene edits required).

---

*Execution Plan v2.1 — `feat/mission-1-2-architecture` · 2026. Maintained by the studio.*
