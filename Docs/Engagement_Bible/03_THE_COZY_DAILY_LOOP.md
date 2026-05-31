# 🗺️ The Cozy Daily Loop — *The Day the Player Wakes Into*

> **Owner:** Lead Game & Level Designer. **Depends on:** `02_ENGAGEMENT_MASTER_PLAN.md`.
> This document specifies the **moment-to-moment day** in detail — the rhythm, the beats,
> the screens, and the *psychology of why each beat makes you want the next one.* It is the
> design contract that `04`–`09` implement.

---

## 1. The shape of a day (and why it's shaped this way)

A Hearthbound day is **~12–18 real minutes** once the loop is mature — short enough to play
"just one more," long enough to feel like a day. It has **five beats**, in a fixed *frame*
but with a **free middle** (the player chooses the order of the three middle activities).

```
  [1] WAKE & AGENDA   →   [2] THE FREE DAY (player-ordered)   →   [3] EVENING LEDGER
   (fixed, ~60s)            shop · garden · village · bench         (fixed, ~90s)
                                                                         │
                                                              [4] OPTIONAL DREAM
                                                                         │
                                                              [5] TOMORROW TEASE → sleep
```

The **fixed frame** (wake + ledger) gives the cozy ritual its comforting sameness — the
"every morning the kettle's on" feeling. The **free middle** gives agency. This is exactly
Stardew's structure (wake on the farm → do whatever → pass out at 2 a.m. → recap → repeat),
translated to a memory-keeper's life.

---

## 2. Beat 1 — Wake & Agenda (the hook of the morning)

**What the player sees, every morning:**

1. A short wake transition (fade from sleep; Pickle on the windowsill; the hearth relit).
2. The **Agenda Card** slides in (Bamao parchment, dismissable) — *the single most important
   new UI in the game.* It is the cozy answer to "what shall I do today?":

```
   ┌────────────────────────────────────────────┐
   │  ☀  Spire-Month, Day 4 — a bright cold morning │
   ├────────────────────────────────────────────┤
   │  At your door today:                          │
   │    • Doris  — "something sweet to ask"   🍞   │
   │    • A stranger with a heavy coat        ❓   │
   │                                               │
   │  In the garden:                               │
   │    • Lavender — ready to harvest         🌿   │
   │    • Valerian — 1 day to ripen                │
   │                                               │
   │  Marin's margin-note suggests:                │
   │    "The shelf by the window is bare. A shop   │
   │     with full shelves keeps better company."  │
   │                                               │
   │              [ Open the day ]                 │
   └────────────────────────────────────────────┘
```

**Why it works:** It is *not* a quest log with checkboxes (which would feel like work and
break cozy). It's a **gentle briefing in fiction-voice** that (a) tells you who's waiting
(anticipation), (b) shows the garden state (the compounding payoff of yesterday), and
(c) *suggests — never demands —* a self-set goal via Marin's margin-notes. The player reads
it and *forms an intention.* That intention is the engagement.

> **Cozy guardrail:** the Agenda never shows a deadline, a fail risk, or a number going
> down. It shows opportunity, not obligation. (D-076.)

---

## 3. Beat 2 — The Free Day (the four activities)

The player walks out of the Hollow's quarters into the shop and chooses what to do. Four
activity "stations," all optional, all reachable in any order:

### 2A — Open the Shop (the heart: take a memory)
- A villager from the Request Board is waiting (or you ring the open-sign to call the next).
- The visit is the existing dialogue + transaction flow, *now repeatable with rotating villagers/memories* (Pillar P2).
- Outcome: you receive a memory orb (to craft), earn/spend coin, shift trust, maybe discover an Echo.
- **This is where the writing lives.** Every visit is a small, warm, sometimes-weighted story.

### 2B — Tend the Garden (the supply loop: Pillar P4)
- Water beds, harvest ripe herbs, plant new seeds.
- Herbs → brew **teas** at the kettle → teas are **tools**: a Lavender tea relaxes a guarded
  villager (more memory revealed); a Valerian tea steadies your hand (easier Cleanse), etc.
- The garden *grows over days* — the visible compounding the critique demanded.

### 2C — Walk the Village (the world: Pillar P2/P6)
- Deliver finished memories, gather wild echoes (foraging analogue), chat with residents for
  ambient lines, find seasonal items.
- The village is **persistent**: a villager you helped yesterday waves; a festival stall
  appears as the Almanac date nears.

### 2D — Work the Bench (the craft: Pillar P5)
- Polish / Cleanse / (later) Weave / Sort / Listen the memories you've taken.
- Now with **gentle skill feedback** and **variety** so the 20th craft still satisfies.
- Finished memories go on **the Wall** (Pillar P6) — your growing, connectable collection.

**The player weaves between these freely.** A typical Day 4 might be: open shop → take Doris's
memory → walk to garden → harvest lavender → brew tea → back to shop → second visitor → bench
to polish both → arrange the new orbs on the Wall → spot an Echo completing → spend coin on a
new shelf → close the ledger. **That is a Stardew day, in a memory shop.**

---

## 4. Beat 3 — The Evening Ledger (the celebration, not just the save)

The existing Evening Ledger becomes the **daily recap + reward beat.** Reframed from a save
screen into the cozy "look what we did today" moment Stardew's shipping-bin + sleep recap
nails:

```
   ┌────────────────────────────────────────────────┐
   │   🌙  Day 4 — The Ledger                       │
   ├────────────────────────────────────────────────┤
   │   Today you:                                   │
   │     · Took Doris's "Wedding Honey"  (+6 coin)  │
   │     · Polished it to a clear shine             │
   │     · Brewed lavender for the stranger         │
   │     · Completed an Echo: "The Sunday Kitchen"  │
   │                                                │
   │   The Hollow grew:                             │
   │     · New shelf by the window  🪟              │
   │     · Garden: valerian ripens tomorrow         │
   │                                                │
   │   Coin purse: 18 ●     Memories kept: 5         │
   │   Echoes found: 3 / 24                          │
   │                                                │
   │           [ Close the book — sleep ]           │
   └──────────────────────────────────────────────┘
```

**Why it works:** it's the dopamine receipt. It *shows the growth* (coin, memories, echoes,
shop changes) — the visible progression the critique said was missing — in a warm, paged,
in-fiction way. It is celebratory, never a scorecard. Closing the book is still the save.

> Implementation note: `EveningLedgerUI` already exists and is wired to `MissionRunner` /
> directors via `OnEndOfDayConfirmed`. We extend its content model, not its plumbing.

---

## 5. Beat 4 — The Optional Dream (the unlockable payoff)

Memory Dreams stop being a forced end-of-mission cutscene and become a **reward the player
earns**: when you've fully polished/connected a memory, that night you *may* dream it. Dreams
become collectible (replayable from the Wall later — the Depth Bible's "Dream Cinema" seed).
This converts the game's most expensive, most beautiful asset class from a *one-time spend*
into a *recurring reward that pulls the loop forward.*

---

## 6. Beat 5 — The Tomorrow Tease (the cliffhanger that gets Day N+1)

The last thing before sleep is a single fiction-voice line that **plants a reason to wake up**:

- *"Tomorrow the bard Idris passes through. He never stays. Memories of the road are rare."*
- *"Gerrold asked if he might come by again. He sounded steadier."*
- *"The valerian will be ready at first light."*
- *"Three days to the Honey Festival. Doris will want her best memory polished for it."*

This is the **`TomorrowTeaseSO`** seed that already exists in the codebase
(`Scripts/Mission/TomorrowTeaseSO.cs`) — currently underused. We make it the closer of every
day. **The tease is the retention hook.** It is *why the player taps "one more day."*

---

## 7. The week & the season (the larger rhythms)

Days nest into a **week** (the Almanac, Pillar P7) and a **season** (the existing 8-month
calendar in `GDD §4.2`). The larger rhythms give the loop a horizon:

- **Weekly:** a recurring visitor (the bard on certain days), a market day, a villager's
  birthday — *reasons certain days differ.*
- **Seasonal:** the festival (Honey Festival, Festival of the Hearth), new herbs in season,
  new villagers arriving, the predecessor-mystery breadcrumbs surfacing on a cadence.

The player isn't told "complete the season." They simply *live it*, and the season's events
give the open-ended loop a gentle narrative spine — the same way Stardew's first-year arc
(community center / Joja) gives its sandbox a shape without forcing it.

---

## 8. How the heavy story still lands (protecting the prize)

The fear: "won't a loop dilute the emotional gut-punches?" **No — if we pace them.** The
Depth Bible's *Comedy/Grief Co-Existence Rule* (`GDD §13.3`) already gives us the tool: when
a heavy memory resolves (a Gerrold-tier beat), the loop **goes quiet** — the next ~15 in-game
minutes suppress comedy, festivals pause, the Request Board shows no new face, and the world
"holds the moment." Then the gentle loop resumes. **Spacing the heavy beats across a loop
makes each one hit *harder*, not softer** — because the player has lived the warm ordinary
days in between, exactly as Spiritfarer spaces its departures across many cozy boat-days.

---

## 9. The daily-loop checklist (the QA contract for "is it a loop yet?")

A day is a complete cozy loop only if **all** of these are true:

- [ ] The player **wakes into a changed world** (garden grew, board reshuffled, a tease paid off).
- [ ] The player **chose the order** of at least 3 activities — no forced corridor.
- [ ] At least one action **compounded** (earned coin spent on lasting growth; a herb planted; an echo progressed).
- [ ] The Evening Ledger **showed visible growth** since yesterday.
- [ ] The Tomorrow Tease **gave a concrete reason** to start the next day.
- [ ] Nothing **punished** the player; every station was **skippable**; Auto-Complete was available.

If any box is unchecked, it's not yet the loop — it's still a corridor with extra rooms.

---

*Daily Loop v1.0 — `feat/mission-1-2-architecture` · 2026. Next: `04_SYSTEM_DAILY_LOOP_AND_TIME.md` (code).*
