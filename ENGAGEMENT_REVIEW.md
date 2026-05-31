# 🔥 Engagement Review — *Is Hearthbound Hollow Actually Fun? (And the Plan to Make It So)*

> **Start here if you want the honest answer to "why isn't this engaging, and how do we fix it?"**
> Full canon lives in **[`Docs/Engagement_Bible/`](./Docs/Engagement_Bible/)**.

---

## The verdict (Critic & Review Board, unanimous)

Hearthbound Hollow is a **beautifully written, beautifully engineered ~75-minute linear
corridor** with exactly **two slow, no-fail, auto-completable mini-games**. It is a *vignette*,
not a game yet. Benchmarked against Stardew Valley's **seven retention engines**, it scores
**0 / 7** — no compounding daily loop, no player-set goals, no ownership, no visible
progression, no calendar of anticipation, no variety, **no tomorrow.**

**Reject as a finished product. Greenlight as a foundation.** The hook ("memory broker"), the
writing, the art, the accessibility, and the architecture are a genuine 9/10. The missing
loop is **buildable on top of what already exists** — and much of it is just *wiring assets
the repo already owns* (the unused HarvestGarden farming pack, the dormant `DayCycleManager`,
the under-used Memory Web overlay, the empty garden plots, the 14 pre-built `VillageState`
dimensions).

Read the full critique: **[`Docs/Engagement_Bible/01_CRITIQUE_WHY_IT_IS_BORING.md`](./Docs/Engagement_Bible/01_CRITIQUE_WHY_IT_IS_BORING.md)**.

---

## The fix: 7 Engagement Pillars (0/7 → 7/7)

| Pillar | What it adds | Stardew engine it restores |
|---|---|---|
| **P1 — The Living Day** | A morning Agenda → free day → evening recap → tomorrow tease | Compounding daily loop |
| **P2 — The Request Board** | A rotating, never-empty queue of villagers who need you | Surprise, variety, infinite content |
| **P3 — My Hollow** | A shop you stock, upgrade, decorate; coin that matters | Ownership & customization |
| **P4 — Garden & Tea** | Grow herbs → brew teas → use them as tools | Interleaving systems |
| **P5 — Living Workbench** | Varied, juicy, gently-masterable memory craft | Mastery + interleaving |
| **P6 — The Memory Wall** | Collect & connect memories; complete Echo threads | Player-set goals + visible collection |
| **P7 — The Almanac** | Festivals, market days, visitors — a calendar to anticipate | Anticipation |

The thesis: *"A Quiet Practice"* — keep every ounce of the heart, and wrap it in a **reason to
play tomorrow.** Read: **[`Docs/Engagement_Bible/02_ENGAGEMENT_MASTER_PLAN.md`](./Docs/Engagement_Bible/02_ENGAGEMENT_MASTER_PLAN.md)**.

---

## The new success gate

> **G-Engage:** 20 cozy-target testers play **three in-game days** (~90 min). **≥15/20 (75%)**
> voluntarily start a **4th day** unprompted, and **≥60%** name a self-set goal they want to
> pursue. *Self-directed continuation is the only true measure of a cozy loop.*

---

## Build order & status

P1 → P2 → P6 → P3 → P4 → P5 → P7. The day spine (**P1**) is the root; everything compounds off it.
- ✅ **Done:** the full Engagement Bible (critique, plan, 6 system specs, roadmap) + the inert,
  compile-safe **P1 code scaffolding** (`EngagementEvents`, `DayAgenda`, `DailyLoopService`).
- ⬜ **Next:** Phase 61.5 — wire P1 live (Agenda card UI + builder + Evening Ledger recap).

Full plan + phase log: **[`Docs/Engagement_Bible/10_IMPLEMENTATION_ROADMAP.md`](./Docs/Engagement_Bible/10_IMPLEMENTATION_ROADMAP.md)**.

---

## Guardrails (unchanged)

Every new system **inherits the Cozy Contract** (no fail states, refusal honored, Auto-Complete
everywhere) and the **no-dark-monetization** rule. Engagement ≠ pressure. The only design change
is **D-076**: cozy, opt-in, *celebratory* progression feedback is now required — players cannot
feel growth they are forbidden to see.

---

*Part of the Abdulmalek Agents game-concept portfolio · 2026 · branch `feat/mission-1-2-architecture`.*
