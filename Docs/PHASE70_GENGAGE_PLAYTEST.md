# 🧩 Phase 70 — The G-Engage Playtest Build & Instrumentation Plan

> **Owner:** QA Lead + Systems Architect + 3× Market Critics. **Gate:** the new success metric
> from `Engagement_Bible/02 §4`. **Status:** plan ready; run after pull + `🚀 Build Everything`.

---

## 1. The gate (G-Engage)

> **20 cozy-target testers play three in-game days (~90 min) of the looped build. At least
> 15/20 (75%) voluntarily start a 4th day without being prompted, and ≥60% can name at least
> one self-set goal they want to pursue** ("I want to finish Doris's thread," "I want the shelf
> by the window," "I want to grow valerian").

Self-directed continuation is the only true measure of a cozy loop. Day 4 by *choice* = a game.

## 2. What the loop now offers a tester (the things to observe)

The build now has, reachable from any gameplay scene via the morning Agenda's key map:

- **[B] Request Board** — a different set of villagers each morning; Keep/Listen/Not-today.
- **[M] Memory Wall** — Echo threads filling toward completion (a visible goal).
- **[U] Hollow Shop** — coin → shelves / a relit room / a garden bed (visible ownership).
- **[G] Garden** — plant → ripen over days → harvest → brew → use/sell (the daily check).
- **[K] Workbench** — tend kept memories with varied verbs + gentle "Perfect" mastery.
- **Coin purse HUD** + **[J] Journal** — visible, celebratory progression (D-076).
- **Almanac** — Market Day / Festival / bard / birthday anticipation.

## 3. The 3-day script the tester should be able to live

- **Day 1:** wake → Agenda → [B] keep Doris's memory (+coin, purse ticks) → [K] tend it
  ("Perfect"?) → [G] see lavender ripening → close ledger → tomorrow tease → sleep.
- **Day 2:** a *different* board → keep another → [M] a thread shows 1/2 → [U] buy the window
  shelf with the coin earned → [G] harvest lavender, brew it → close ledger.
- **Day 3:** brewed tea helps a visit (+bonus) → complete an Echo thread on the Wall (+coin,
  celebratory beat) → buy a garden bed → an Almanac countdown ("Market Day in 1 day").
- **The hook:** the Day-3 tease + a half-finished thread + a ripening herb + a Market Day
  tomorrow should make the 4th day a *choice the tester makes*.

## 4. Instrumentation (lightweight, privacy-safe, local)

Add a tiny opt-in dev log (Editor/Development builds only) that records, with no PII:

| Signal | Source already in code |
|---|---|
| Day reached / voluntary Day-4 start | `DayStartedEvent` (count distinct days; flag if `DayIndex ≥ 3` reached by choice) |
| Visits resolved + outcome | `RequestResolvedEvent` (taken/listened/deferred/refused) |
| Coin earned vs spent | `CoinChangedEvent` (sum by `Reason`) |
| Echo threads completed | `EchoThreadCompletedEvent` |
| Upgrades purchased | `HollowUpgradePurchasedEvent` |
| Crafts + Perfect rate | `MemoryTendedEvent` |
| Almanac events seen | `AlmanacEventEvent` |

> Implementation note: a single `Editor`/dev-only `EngagementTelemetry` MonoBehaviour can subscribe
> to these existing EventBus structs and append a line to `Application.persistentDataPath`. It is a
> *measurement* tool, not a shipping feature, and must be gated behind `DEVELOPMENT_BUILD || UNITY_EDITOR`
> and an explicit tester opt-in (no dark patterns, Golden Rule 2).

## 5. Exit-interview questions (the ≥60% self-set-goal check)

1. Without looking — what do you want to do next time you play?
2. Did anything feel like *yours*? (the Hollow, the garden, a thread, a villager)
3. Was there ever a moment you felt pushed or behind? (must be **no** — Cozy Contract)
4. What was the best 30 seconds?

## 6. Tuning levers if Day-4 < 15/20 (Phase 71)

- Raise/lower visit coin (`RequestBoardService` reward; `RequestVisitService`).
- Cheaper first shelf so Day-2 ownership lands (`HollowProgressionService` catalog cost).
- Shorter first herb ripen (`GardenService` `growDays`).
- Stronger tomorrow tease specificity (`TomorrowTeaseSO` / Almanac countdown).
- Pace heavy beats: when a Gerrold-tier beat resolves, hold the board quiet ~15 in-game min
  (Comedy/Grief radius, `GDD §13.3`) so warmth and weight alternate.

---

*Phase 70 plan v1.0 — run this, report the numbers, then tune (Phase 71).*
