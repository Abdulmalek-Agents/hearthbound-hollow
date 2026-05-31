# 🎬 The Engagement Master Plan — *From a 75-Minute Vignette to a 40-Hour Cozy Home*

> **Owners:** Creative Director · Lead Game & Level Designer · Systems & Progression Architect.
> **Premise (from `01_CRITIQUE`):** we score **0/7** against Stardew's retention engines.
> This document is the thesis for getting to **7/7** *without diluting the writing or
> breaking the Cozy Contract.* It defines the **7 Engagement Pillars**, the **core loop**,
> and the **priority order**. Systems `04`–`09` then spec each pillar at code level.

---

## 1. The design thesis: *"A Quiet Practice"*

Stardew is a **farm you tend**. Spiritfarer is a **boat you keep**. Hearthbound should be
a **practice you keep** — the daily, gentle, compounding craft of a memory-keeper running
a real shop in a living village.

The fantasy upgrade is one word: from **"read a memory"** to **"run the Hollow."** You don't
just experience two memories; you *open your shop each morning*, *see who's waiting*,
*choose whose memory to take today*, *tend the garden that supplies your teas*, *do the
craft at the bench*, *arrange and grow your shop*, *connect the collection on your wall*,
and *close the ledger at night having made the village — and your Hollow — a little better
than yesterday.* Then you do it again, and it's a little different, and a little more *yours.*

**That is the whole fix.** Everything below serves it.

---

## 2. The 7 Engagement Pillars (the 0/7 → 7/7 conversion)

Each pillar maps 1:1 to a Stardew retention engine we're missing, and to a buildable system.

| # | Pillar | Replaces (Stardew engine) | Player feeling | Spec doc |
|---|---|---|---|---|
| **P1** | **The Living Day** — a real morning→night cycle you wake into and close out | Compounding daily loop | *"A new day. What shall I do with it?"* | `04` |
| **P2** | **The Request Board** — a rotating queue of villagers + memories, never empty | Surprise & variety + infinite content | *"Who needs me today? Ooh, a new face."* | `05` |
| **P3** | **My Hollow** — a shop you stock, arrange, upgrade, and decorate | Ownership & customization | *"This is mine, and it's getting better."* | `06` |
| **P4** | **The Garden & Tea** — grow herbs, brew teas, use them as tools | Interleaving systems #1 | *"My lavender's ready — time to brew."* | `07` |
| **P5** | **The Living Workbench** — varied, juicy, skill-curved memory craft | Interleaving systems #2 + mastery | *"I'm getting good at this. One more."* | `08` |
| **P6** | **The Memory Wall** — collect, connect, and complete the Echo Web | Player goals + visible collection | *"Two more and I complete Doris's thread!"* | `09` |
| **P7** | **The Almanac** — a calendar of festivals, visitors, and anticipation | Calendar of anticipation | *"The Honey Festival is in 3 days!"* | `05 §6` + `06 §7` |

> **Crucial discipline:** every pillar is **opt-in and unpunishing.** A player can ignore the
> garden, auto-complete every craft, never decorate, and still reach the end. The pillars add
> *pull*, never *push*. That is how we get Stardew's depth and keep the Cozy Contract intact.

---

## 3. The new core loop (the thing we are actually building)

```
                       ╔══════════════════════════════════════╗
                       ║          ☀  A NEW MORNING            ║
                       ║   Wake in the Hollow. Pickle yawns.   ║
                       ║   The day's AGENDA card slides in:    ║
                       ║   • 2–3 villagers on the Request Board║
                       ║   • garden status (herb ready?)       ║
                       ║   • 1 gentle self-set goal suggestion ║
                       ╚═════════════════════╦═════════════════╝
                                            │  player chooses order — NO forced path
        ┌───────────────────────┬───────────┴───────────┬────────────────────────┐
        ▼                       ▼                         ▼                        ▼
  ┌───────────┐         ┌──────────────┐          ┌──────────────┐        ┌──────────────┐
  │ OPEN SHOP │         │ TEND GARDEN  │          │ WALK VILLAGE │        │ DO THE CRAFT │
  │ greet a   │         │ water/harvest│          │ deliver,     │        │ Polish/Cleanse│
  │ visitor,  │◄───────►│ herbs → brew │◄────────►│ gather, chat,│◄──────►│ Weave/Sort/  │
  │ take a    │  tea     │ teas (tools) │  herbs   │ find echoes  │ memory │ Listen at    │
  │ memory    │ helps    └──────┬───────┘          └──────┬───────┘ orbs   │ the bench    │
  └─────┬─────┘ talks           │                         │                └──────┬───────┘
        │                       │                         │                       │
        └─────────────────────────┴───────────────┬─────────────────────────────┘
                                                 ▼
                                      ┌────────────────────┐
                                      │  💰 SPEND / GROW    │
                                      │  buy a shelf, a herb│
                                      │  bed, decor, a tool │  ← the compounding bit
                                      │  arrange the Wall   │
                                      └──────────┬─────────┘
                                                 ▼
                       ╔══════════════════════════════════════╗
                       ║        🌙 CLOSE THE LEDGER           ║
                       ║   Today's deeds (celebratory recap).  ║
                       ║   What grew. What you earned. Tomorrow║
                       ║   tease: "Idris the bard arrives."    ║
                       ║   Sleep → optional Dream → next day.  ║
                       ╚══════════════════════════════════════╝
                                            │
                                            └──────────► back to A NEW MORNING (changed!)
```

### Why this loop retains
- **It compounds:** coin → shelves/beds/tools → more capacity → more memories → more coin. Visible, gentle, cozy.
- **It's player-directed:** the morning offers options, never a corridor. You decide today's shape.
- **It varies:** the Request Board reshuffles; the Almanac injects festivals/visitors; the garden cycles.
- **It pays off tomorrow:** planted herbs ripen, a deferred villager returns, a festival approaches.
- **It still tells the story:** Doris, Gerrold, Marin, the Echo Web, the predecessor mystery are now *spread across the loop* as the reasons the loop has meaning — not crammed into 75 minutes.

---

## 4. The "one good cozy day" target (the new vertical-slice success metric)

The old gate: *"20 testers finish M1+M2 (~1 hr); 14 say 'I want to play more.'"* — but the
build then *had nothing more*. The new gate is honest:

> **NEW GATE (G-Engage):** *20 cozy-target testers play **three in-game days** of the looped
> build (~90 min). At least **15/20 (75%)** voluntarily start a 4th day without being
> prompted, and **≥60%** name at least one self-set goal they want to pursue ("I want to
> upgrade my shelf," "I want to finish Doris's thread," "I want to grow valerian").*

Self-directed continuation is the only true measure of a cozy loop. If they *choose* Day 4,
we have a game. If they stop at Day 3, we have a longer vignette.

---

## 5. What changes for the existing content (nothing is thrown away)

| Existing asset | Old role | New role in the loop |
|---|---|---|
| **Doris (M1)** | The tutorial villager, gone after Day 1 | The **first regular**: appears on the Request Board across the season; her 4-node Memory Map unlocks over many visits (Pillar P6). |
| **Gerrold (M2)** | The one moral choice, gone after Day 2 | A **recurring relationship**: the Day-2 choice is the *first* of his arc; he returns on the board, with follow-ups gated by your choice. |
| **Polish / Cleanse** | The only two mechanics | Two of a **growing verb set** (Pillar P5), now with a gentle skill curve + variety. |
| **Memory Dreams** | End-of-mission cutscene | The **reward you unlock** by collecting/connecting memories (Pillar P6) — replayable from the Wall. |
| **Echo Web (DOR↔GER)** | A described line of light | A **real meta-game**: collect memories, discover echoes, complete threads for Dream + lore payoffs (Pillar P6). |
| **Evening Ledger** | A save screen | The **daily recap + celebration + tomorrow-tease** (Pillar P1). |
| **HarvestGarden asset** | Unused / decorative | The **garden loop** (Pillar P4). |
| **`DayCycleManager.cs`** | Dims a cutscene light | The **clock** that drives the Living Day (Pillar P1). |
| **`VillageState` 14 dims** | Mostly default-valued seams | **Now written-to and surfaced** as cozy feedback (D-076). |

> **Translation:** we are not deleting the museum. We are turning the museum into a *town
> you live in*, where the exhibits are the people you visit every day.

---

## 6. Priority order (what to build first, and why)

The board ranked by **(retention impact) ÷ (build cost)**. Build in this order:

| Order | Pillar | Why first | Risk |
|---|---|---|---|
| **1st** | **P1 Living Day** + Evening Ledger recap | The loop's skeleton. Nothing compounds without a "tomorrow." Cheapest — `DayCycleManager` already exists. | Low |
| **2nd** | **P2 Request Board** | Turns 2 villagers into infinite gentle content. The single biggest retention lever. | Med |
| **3rd** | **P6 Memory Wall / Echo meta** | Gives the player a *goal* and makes collecting matter. Reuses existing memory SOs. | Med |
| **4th** | **P3 My Hollow progression** | The compounding spend-sink. Makes coin meaningful. Reuses cottage/prop prefabs. | Med |
| **5th** | **P4 Garden & Tea** | Activates an owned asset; adds a second interleaving system. | Med |
| **6th** | **P5 Workbench variety** | Deepens the core craft once there's a reason to do it often. | Med-High |
| **7th** | **P7 Almanac/festivals** | The anticipation layer; best once the daily loop is solid. | Med |

`10_IMPLEMENTATION_ROADMAP.md` turns this into concrete build phases (61 → 72).

---

## 7. The non-negotiables (the guardrails every pillar inherits)

1. **Cozy Contract holds.** No fail states, no timers (unless opt-in), refusal always honored, kindness is optimal, Auto-Complete on every mini-game.
2. **No dark monetization.** Ever.
3. **Feedback is celebratory, never anxious (D-076).** Show coin, growth, collection %, agenda — warmly. Never show a punishing countdown or a red "behind schedule" number.
4. **Writing quality bar unchanged.** Procedural/recombined content (Request Board variety) uses the Vignette Library at the Depth-Bible quality bar; hand-sealed villagers stay hand-sealed.
5. **Idempotent + single-entry.** Every system ships behind `🚀 Build Everything` (D-074), heal-or-create, and updates `PROGRESS.md`.
6. **Architecture discipline.** Respect the asmdef graph; new state on `VillageState`; new events on the EventBus; data in ScriptableObjects.

---

## 8. The promise

If we build these seven pillars on top of the heart that already exists, the review flips from:

> *"Beautiful, but there's nothing to do."*  →  *"I meant to play one day and it's 1 a.m."*

That sentence is the difference between a 72/100 curio and a cozy-of-the-year contender.
The hook is already the best in the genre. Now we build the **reason to stay.**

---

*Master Plan v1.0 — `feat/mission-1-2-architecture` · 2026. Next: `03_THE_COZY_DAILY_LOOP.md`.*
