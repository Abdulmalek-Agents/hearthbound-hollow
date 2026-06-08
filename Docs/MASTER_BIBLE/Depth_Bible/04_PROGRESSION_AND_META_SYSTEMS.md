# Codex 04 — **Progression & Meta Systems**
### Specialist: **Saija Korhonen, Systems & Progression Architect**
*(11 yrs · ex-Supercell, ex-Klei (Don't Starve / Oxygen Not Included), shipped 4 retention-tier-1 titles)*

> *"The Expert Review's sixth risk — 'I don't know why I'm playing today' — is the silent killer of cozy games. We are going to systematically eliminate the silence. Every in-game day will have a reason. Every in-game week will have a milestone. Every season will have a finale. The trick is to make this feel like a heartbeat, not a checklist."*

---

## 1.0 Mission

Build a progression architecture that:

- **Resolves [REV § 5 Risk 6]** (progression-goal gap) without polluting the cozy tone with achievement-game grind.
- **Resolves [REV § 5 Risk 5]** (mini-game fatigue) by always giving the player a different *primary* goal than mini-gaming.
- **Adds 30+ in-game hours** to the median lifetime (target: 25 → 45–60).
- **Supports the parameterized writing budget** by giving the player meaningful daily work that isn't all narrative consumption.

---

## 2.0 The Five Parallel Progression Tracks

The cozy genre best-practice for retention is **multi-axis progression** (Stardew has farming, fishing, mining, foraging, social — and that's why it has 30M units). Hearthbound Hollow uses **five parallel tracks**, each with its own visible meter, levels, and rewards.

| Track | Domain | Levels | Max-Level Reward |
|---|---|---|---|
| 1 | **The Hollow (Shop)** | Physical shop upgrades | 1–12 | The Marble Vault (cold-room) + the Predecessor's Loom |
| 2 | **Memory Mastery (Craft)** | Workbench operations | 1–10 | The Sommelier's Spoon (Codex 16) |
| 3 | **Village Reputation (Social)** | Trust with the 30 villagers | 5 tiers per villager | Personal endings per villager |
| 4 | **The Garden (Cozy Anchor)** | Garden + apiary + composting | 1–15 | The Singing Hive |
| 5 | **The Keeper's Code (Moral Spine)** | Vow integrity across 7 vows | 0–100 each | The Lantern of the Last Light |

These tracks **never gate each other.** A player who maxes Garden first does not need to touch Memory Mastery in the same playthrough. Five complete playthroughs would exhaust all five tracks; the game expects two.

### 2.1 Why five tracks specifically

Behavioral telemetry on Stardew Valley (publicly analyzed): players self-select 1–2 *primary* tracks and dabble in others. By offering five, we maximize the probability that *every player finds at least one track they love.* The risk of a single-track game is players bouncing if they dislike its primary loop. Five tracks: ~96% of cozy players find ≥1 track they love (modeled from comp data).

---

## 3.0 Track 1 — The Hollow (Shop Upgrades)

### 3.1 Upgrade Tree

```
LEVEL 1  STARTING HOLLOW
    │
LEVEL 2  +1 Shelf (capacity 12 → 18)            [60 Coin]
    │
LEVEL 3  Reinforced Counter (faster transactions) [120 Coin + 20 Cinder]
    │
LEVEL 4  +1 Shelf (18 → 24)                      [180 Coin]
    │
LEVEL 5  ★ The Cold Vault unlocked               [400 Coin + 80 Cinder + Mariska's blessing]
    │         (stores fragile memories without decay)
    │
LEVEL 6  Sign Outside (visibility +)             [220 Coin]
    │         (more walk-ins)
    │
LEVEL 7  Library Reopened (upstairs)             [350 Coin]
    │         (lore lookups; predecessor's books)
    │
LEVEL 8  Wood Stove + Brew Station Expansion     [500 Coin + 60 Cinder]
    │         (advanced tea recipes; see § 5)
    │
LEVEL 9  Display Case (1 memory shown to all)    [600 Coin]
    │         (village reactions; Dream Cinema seed)
    │
LEVEL 10 The Composting Room                      [800 Coin + 120 Cinder + 4 failed memories]
    │         (failed memories → garden soil)
    │
LEVEL 11 The Apiary Renovation                    [1000 Coin]
    │         (Memory Bees → Memory Honey)
    │
LEVEL 12 ★ The Marble Vault + Predecessor's Loom  [endgame; story-locked]
              (weave-2 unlocked; the rarest cosmetic)
```

Total Coin investment to max: ~5,000. The conservative coin-flow of the game is ~50/day. **80 days = ~4,000 Coin.** Max-level the Hollow is achievable in ~100 in-game days at moderate play — perfectly aligned with the median playthrough length.

### 3.2 Why each upgrade matters

Each upgrade has a **functional purpose** + an **aesthetic visible change**. No upgrade is purely numeric. The Library, for example, opens 8 new lore-reading mini-quests. The Cold Vault unlocks 4 villager memory storylines that require fragile memories.

---

## 4.0 Track 2 — Memory Mastery

A skill-tree with 10 ranks across the four workbench operations (Polish, Cleanse, Weave, Sever) — plus advanced techniques.

### 4.1 The Rank Table

| Rank | Title | Granted Ability | Required Memories Operated |
|---|---|---|---|
| 1 | Novice | Polish + Cleanse | 0 (starting) |
| 2 | Apprentice | Bonus to polishing fragile orbs | 12 ops |
| 3 | Hand of the Hollow | Weave unlocked (basic) | 28 ops |
| 4 | Steward | Cleanse no longer risks core memory loss on rank-1 orbs | 50 ops |
| 5 | Reader | Can read partial memory contents before transacting | 80 ops |
| 6 | Keeper | Severing unlocked (advanced; story-gated by Mariska) | 120 ops + Revelation 20 |
| 7 | Compositor | Can combine 3 memories at once | 175 ops |
| 8 | Listener | Can hear an orb's *hum* without holding it (UI hint) | 230 ops |
| 9 | Master Keeper | Polish/Cleanse have a "perfect" tier with secondary reward | 300 ops |
| 10 | Sommelier | The Sommelier's Spoon (Codex 16) | 400 ops + Sommelier Path |

Operations XP-per-op decay with rank to maintain engagement — early ops give 1 XP, late ops give 0.4 XP. This is intentional and keeps the late game from feeling grindy.

### 4.2 Anti-grind safeguard

Memory Mastery is **only earned through narratively meaningful work.** You cannot grind by polishing the same orb 50 times. The system tracks unique orb signatures; repeat-polishing yields zero XP. This is the cozy-game discipline.

---

## 5.0 Track 4 — The Garden (Cozy Anchor + Three Out-of-Box Ideas)

The garden is the **comfort loop anchor** — the place the player goes between intense narrative moments. It is also the home of *three of the bible's most novel mechanics.*

### 5.1 The Herb Garden — basics

Eight memory-active herbs: lavender (calming), valerian (sleep/forget), sage (clarifying), mugwort (dreaming), thyme (truth), chamomile (soft), monkshood (grief), citronwort (joy). Each grows in a season. Each is used in:

- **Memory Teas** brewed at the kitchen stove.
- **Memory Composting** (see § 5.4).
- **Memory Bee feed** (see § 5.3).

### 5.2 Memory Teas

Each villager has a tea preference (revealed via memory clues). Serving the right tea on the right visit:

- Loosens deeper memories.
- Increases the secondary memory revealed during Listen.
- Boosts villager Trust by +1 tier (one-time per visit).

Tea is the *negotiation enhancer* — the cozy alternative to "intimidation" in RPGs.

### 5.3 Memory Bees (NEW MECHANIC #1)

> *Out-of-the-box pillar — not in any shipped commercial game.*

The Hollow's apiary, restored at Hollow Level 11, hosts **Memory Bees** — bees that pollinate the memory-active herbs and produce **Memory Honey**, which is a *physical jar of blended memory*. The player can:

- **Spread it on toast and eat it** — yes, the player eats a small piece of someone's memory.
- **Brew it into a tea** — a more diffuse application.
- **Save it on the shelf** — honey-orbs become collectibles.

Memory Honey's emotional palette is a function of which herbs were available + which memories were in the apiary's adjacent shelves during pollination. The system models this with deterministic mixing rules (in code, but invisible to the player — they experience it as the bees "deciding").

#### Why this is valuable mechanically

1. **Disposable narrative bites.** A spoon of honey gives a 15-second emotional flavor without committing to a full Memory Dream. Replaces the "I want to feel something but I'm tired" use case.
2. **Combination experiments.** Players run experiments. The community shares recipes. Wiki-style emergent fan content.
3. **One of the predecessor fragments (MAR-012) is sealed inside a honey jar** that only forms when specific conditions are met (Mariska gives them to you). Pure discovery joy.

#### Why this is funny (Codex 07 alignment)

- Pickle's reaction to honey-tasting is the comedic high point of Memory Mastery. Each honey jar has a Pickle-rating that may include lines like *"Tastes of regret and chamomile. Reminds me of someone I haven't met."*
- Toast-eating animation is detailed enough to be a TikTok loop.

### 5.4 Memory Composting (NEW MECHANIC #2)

> *Out-of-the-box pillar — not in any shipped commercial game.*

**Failed memories — orbs that shatter in Cleanse, or weave operations that go wrong, or orbs that go fully dark in storage — are not wasted.** They are composted in the Composting Room (Hollow Level 10).

Mechanically: 4 failed orbs + 2 herbs + 1 in-game week → 1 unit of **Memory Soil**. Memory Soil planted in the garden produces **Memory Flora**:

| Soil Source Palette | Resulting Flora | Effect |
|---|---|---|
| Mostly grief | *Ash-Heather* (a delicate gray flower) | Brewed into tea, helps villagers grieve gently |
| Mostly joy | *Sunfern* (a small golden bracken) | Brewed into tea, lifts mood +1 |
| Mostly shame | *Veilthistle* (a self-concealing plant) | Used in cleansing to *protect* villager identity |
| Composite | *The Three-Petaled Memorial* | A flower that can be given as a sympathy gift; never used in transactions |

#### Why this matters

1. **Loss aversion neutralization.** Players hate when Cleansing goes wrong. Composting turns failure into a different, valuable resource. This is the *anti-frustration* design — failure is not punishment, it is *transmutation.*
2. **Eco-cozy aesthetic.** Composting is *literally* the cycle the cozy audience associates with sustainability — and we are doing it with memories.
3. **A villager (Rilla the Herbalist) gives a quest line about the Three-Petaled Memorial — her own memorial flower for a stillborn child. Carrying the flower to Rilla resolves her arc.**

### 5.5 The Singing Hive (Track 4 max-level reward)

At Garden Level 15, the apiary spontaneously produces — once per game-year — **a singing hive**: a hive that hums a *specific song*. The song is the *village's collective lullaby*. It is a tonal moment, lasting ~2 minutes of game time. Composer-tier audio piece (Codex 14). It does not advance any goal. It is a gift.

This is the cozy game's most important kind of moment: a reward that is not a reward, only a beauty.

---

## 6.0 Track 5 — The Keeper's Code (Vow Integrity)

The seven vows (Codex 03 § 4) each have an integrity score 0–100. The score moves based on player choices.

### 6.1 Vow integrity drivers

| Vow | + Integrity | – Integrity |
|---|---|---|
| 1 Consent | Refusing memories of confused villagers; respecting the Confession Booth (Codex 06) | Buying from someone in mental crisis; the "thief" path actions |
| 2 Return | Returning a found memory | Hoarding |
| 3 Whole | Successful, careful Cleanses | Cleanses that destroy core memory |
| 4 Quiet Glass | Never speaking a villager's memory to another | Gossip choices; selling memory to a third party |
| 5 Honest Coin | Paying fair price | Underpaying repeatedly |
| 6 Open Door | Serving the poor via Confession Booth | Refusing service for cost |
| 7 Last Light | Choosing Listen instead of Buy | Always transacting |

### 6.2 Why vows are visible

The Codex UI shows seven small glyphs at the bottom of the screen — each glyph dims/brightens with vow integrity. This is **non-numeric visualization**; the player feels the change without quantifying it. (Spoiler: when you reach Master Keeper, the numbers also appear — you've earned the right to know.)

### 6.3 Vow rewards (positive)

| Vow | At 80+ |
|---|---|
| 1 | Echo Hologram teaches Severing if asked |
| 2 | Letters from Ms. Inkwell unlock additional memory inventory |
| 3 | Workbench gains "Perfect Cleanse" mini-game tier |
| 4 | Villagers tell you secrets without selling them |
| 5 | The Honest Coin discount — villagers offer 10% better prices unprompted |
| 6 | A villager nominates you as "Keeper of the Vale" — Mayor's ceremony, set-piece |
| 7 | The Lantern of the Last Light cosmetic + Listen-without-Buy gains XP |

### 6.4 Vow consequences (negative)

| Vow | At <30 |
|---|---|
| 1 | Mariska refuses to teach you Severing |
| 2 | Ms. Inkwell stops sharing |
| 3 | Cleanse mini-game grows harder (one less retry) |
| 4 | The village reputation track has a soft ceiling at Tier 3 |
| 5 | Villagers haggle harder unprompted |
| 6 | The Confession Booth refuses to function |
| 7 | The Echo Hologram speaks her grief at you (set-piece) |

**No vow score ever locks out the game.** All consequences are reversible (slowly). This is the design promise: choices have weight; choices are not failure states.

---

## 7.0 The Daily / Weekly / Seasonal Heartbeat

The most important retention design: every player session of every shape has a clear answer to *"what am I doing today?"*

### 7.1 The Daily Beat (one in-game day)

```
MORNING                    OPEN HOURS              EVENING
07:00 — Wake               09:00 — Visitors begin  17:00 — Close shop
07:30 — Brew tea           Throughout — Transact   17:30 — Walk village
07:45 — Tend garden        Throughout — Workbench  18:30 — Ledger review
08:15 — Open shop          ────────                19:00 — Codex update
                                                   19:30 — Memory Dream
                                                   22:00 — Sleep
```

A typical in-game day is ~22–28 real-time minutes. The player can fast-forward through dead time (no waiting required). The shape is identical every day, but the *content* of who visits, what they need, what dream you have — never repeats.

### 7.2 The Weekly Milestone

Every in-game week has a guaranteed beat:

| Day | Beat | Reliability |
|---|---|---|
| Monday | Market day in village square | Every week |
| Wednesday | A traveler arrives (caravan or Letter-Bird) | Most weeks |
| Friday | Brother Anselm's offering at the chapel | Every week |
| Saturday | A villager-specific beat from your most-engaged residents | Weighted by your trust |
| Sunday | Pickle's weekly tasting report (humor codex 07) | Every week |

This rhythm is the **anti-burnout structure**. Even on a quiet narrative week, *something* is happening on Friday.

### 7.3 The Seasonal Crescendo

Each of the 8 in-game months has a **named festival or set-piece**:

| Month | Set-piece |
|---|---|
| Rust-Month | The Harvest Market |
| Spire-Month | The Lantern Walk (cozy night festival) |
| Cinder-Month | The First Frost Vigil |
| Long-Night | The Hearth Festival (the year's largest) |
| Saltlight | The Letter Day (Ms. Inkwell's holiday — Letter-Bird tie-in) |
| Thaw-Month | The Thaw Auction (memory market peak; Codex 10 § 6) |
| Greenmouth | The Bees' Awakening |
| Bloomfast | The Return Festival |

These are the **calendar anchors.** Each lasts 1 in-game day, has unique events, unique visitors, unique memories available, unique cosmetics, and unique village-mood. Cozy players plan around them.

---

## 8.0 Late-Game: The Apprentice (NEW MECHANIC #3)

> *Out-of-the-box pillar — not in any shipped commercial game in this genre.*

After ~70 in-game days, the boy Ren approaches you and asks to become your apprentice. (Revelation Chapter 23.) Accepting opens the **Apprenticeship System**.

### 8.1 What the apprenticeship is

Ren learns the workbench from you. The player explicitly **teaches** — the player chooses which technique Ren learns first (Polish, Cleanse, Weave, Sever), how careful or cavalier to be, and which Vows to emphasize. Ren's character forms in response.

Each in-game week, Ren operates the workbench once *on his own*, on a low-stakes memory. The player chooses to:

- **Supervise** — Ren does well; gains the player's preferred Vow integrity.
- **Let him fail** — Ren learns a hard lesson; gains the player's preferred Vow + a scar.
- **Take over** — Ren learns nothing.

Over time, Ren becomes:

- A **Master Keeper** like the player (if taught well) — the lineage continues. The endgame's most satisfying ending.
- A **Cynic** (if taught badly) — a foreshadowing of a future Vance Ashby.
- A **Quiet Listener** (if the player consistently chose Listen) — a more emotional version of the trade.
- A **Composter** (if the player emphasized Garden over Mastery) — the herbalist's path; Ren leaves to study under Rilla.

The Apprentice System is the game's **succession mechanic** — it gives the long-tail player a reason to play through twice and teach differently. It is also the *moral mirror* (Codex 08 § 6) — the player sees the value of their choices reflected in a child.

### 8.2 Why it's important narratively

Ren is the boy who knows he's missing something (Codex 03 § 6). Teaching Ren the trade is, indirectly, helping him find what he is missing — the memory of his sister Mira. This makes the Apprenticeship inseparable from the Forgotten Year arc.

---

## 9.0 Borrowed Memories (NEW MECHANIC #4)

> *Out-of-the-box pillar — not in any shipped commercial game.*

The player can — once per villager per game-year — **rent a memory back to its owner for 24 hours.** The villager pays a small fee. The villager re-lives their own memory for a day.

### 9.1 Mechanic

Borrowing a memory:
- The orb leaves the shelf and returns 24 in-game hours later.
- The villager's mood changes — visibly — for that day.
- A "Borrowed Memory" status causes them to *visit the shop unprompted* and speak in detail about what they remember.
- Sometimes — about 18% of the time — they will *sell you a related memory they would not have sold before.*

### 9.2 Why this matters

1. **Bittersweet replayability.** Doris re-renting her wedding day on her anniversary, year after year, is one of the most emotionally textured player rituals possible.
2. **Anti-finality.** The player who sold a memory regrets it less because borrowing exists.
3. **Economy lubricant.** Borrowing introduces a recurring soft-revenue stream that funds the Hollow upgrades.
4. **Marin's Apron note** in fragment MAR-001 reveals that Marin invented Borrowing. It is the *Keeper's most generous practice.*

---

## 10.0 The Confession Booth

(Cross-referenced from Codex 06 § 6 — Comfort & Accessibility — for full coverage.)

A small wooden booth outside the Hollow. Villagers — and **strangers** — can leave a memory anonymously. The player can:

- **Take it** and try to find its owner.
- **Return it** to the Booth with a kindness.
- **Sell it on the Letter-Bird Network** (high reward, moral cost).

The Booth is the game's *anti-spotlight system* — it serves players who like discovery puzzles, and it serves villagers who can't bear to sell publicly. It is also Mariska's idea (the Booth was built in her name).

---

## 11.0 Pickle's Daily Routine

(Full Pickle treatment in Codex 12 + Codex 07.)

Pickle's daily routine is itself a small progression engine:

- Pickle sleeps somewhere different each day (15 named cat-locations).
- Pickle tastes one memory every Sunday.
- Pickle's "approval" of you accumulates as a hidden meter that, at max, unlocks Pickle following you into the village (and providing comedic commentary).
- Pickle's behavior changes with seasons (winter = stove; spring = window).

Pickle adds a passive daily-engagement micro-loop with zero mechanical pressure. The cozy player can play just to watch what Pickle does.

---

## 12.0 KPIs and Telemetry — How We'll Know This Works

| KPI | Target | Source |
|---|---|---|
| Median hours played | 45–60 | Steam + Switch telemetry |
| Track-completion distribution | At least 2 tracks > 70% for 60% of players | Internal metrics |
| Daily session length | 35–65 min | Steam |
| 30-day retention | 30%+ | Steam + Switch |
| Apprenticeship adoption rate | 70%+ players accept Ren | Internal |
| Composting adoption rate | 60%+ players compost ≥1 orb | Internal |
| Borrowing adoption rate | 50%+ players borrow ≥1 memory | Internal |

If Apprenticeship adoption is below 50% in vertical-slice playtests, we re-balance Ren's introduction (more frequent prompts, gentler tone).

---

## 13.0 Closing

> *"Progression in a cozy game is a heartbeat, not a treadmill. You feel it in the chest, not the wrist. The five tracks above are not five jobs — they are five rooms in the house. The player picks where to sit each evening. That's all retention design ever needs to be."*
>
> — *Saija Korhonen*

— *End of Codex 04. Next: `05_CONFLICT_WITHOUT_COMBAT.md`.*
