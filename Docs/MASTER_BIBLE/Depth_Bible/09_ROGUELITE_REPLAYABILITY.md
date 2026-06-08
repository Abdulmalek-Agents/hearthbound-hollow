# Codex 09 — **Roguelite & Replayability Director**
### Specialist: **Ari Sukenobu, Roguelite & Replayability Director**
*(11 yrs · ex-Supergiant (Hades), ex-Motion Twin (Dead Cells), narrative roguelite specialist)*

> *"A cozy game's deepest replayability problem is that the cozy player's first playthrough is sacred. They will not 'reset' it casually. If we want them back for hours 60–200, we need to give them reasons to return that do not violate their first-playthrough's emotional finality. That is the design problem. Roguelite layered onto cozy. Done right, it doubles lifetime hours. Done wrong, it cheapens the village. Here is how to do it right."*

---

## 1.0 Mission

Push median lifetime hours from **25 → 45–60** (Codex 04 KPI). Build four replayability layers — **Procedural Memory Generation, the Drifter Mode, the Weekly Memory Market, and New-Game+ Threads** — that each give the cozy player a reason to return without devaluing the canonical playthrough.

This codex's central tension (with Esme Cordray, Codex 08): **procedural content risks diluting choice weight.** The resolution is the *Hand-Sealed / Procedural split* (defined here) and the *Threaded NG+* (also here) — neither cheats the first playthrough's gravity.

---

## 2.0 The Four Replayability Layers

| # | Layer | Audience | Time Footprint |
|---|---|---|---|
| 1 | **Procedural Memory Generation** | All players, first playthrough | Continuous (background) |
| 2 | **The Weekly Memory Market** | Mid-late game players | Weekly beat |
| 3 | **The Drifter Mode** | Post-credits / new-character players | Standalone short loop (5–8 hrs) |
| 4 | **New-Game+ Threads** | Returning players | 20-40 hrs per Thread |

---

## 3.0 Layer 1 — Procedural Memory Generation

(Mechanically detailed in Codex 02 § 7 — Vignette Library.)

This layer ensures that **no two players have the same memory inventory.** Even within the 12 Sealed Villagers, the 14 hand-written memory slots are not all available every playthrough — each Sealed Villager has **3–5 *floating* memory slots** that are filled by the procedural assembler from a per-villager-tagged sub-library of ~22 candidates each.

### 3.1 The Sealed-Villager floating slots

| Sealed Villager | Hand-sealed slots | Floating slots | Total |
|---|---|---|---|
| Doris | 9 | 5 | 14 |
| Sebastian | 10 | 4 | 14 |
| Wren | 11 | 3 | 14 |
| Caspar | 9 | 5 | 14 |
| Lavinia | 10 | 4 | 14 |
| Idris | 11 | 3 | 14 |
| Halia | 12 | 2 | 14 |
| Tomek | 9 | 5 | 14 |
| Brother Anselm | 11 | 3 | 14 |
| Ms. Inkwell | 9 | 5 | 14 |
| Ren | 10 | 4 | 14 |
| Old Mariska | 13 | 1 | 14 |

Total Sealed memories: **168 hand + 44 procedural = 212 per playthrough.**

For the 18 Procedural Villagers, the floating ratio is inverted: **4 hand-anchored + 10 procedural = 14 each.** Total Procedural: **72 hand + 180 procedural per playthrough.**

**Grand total per playthrough: 240 hand + 224 procedural = 464 memories.**

Across two playthroughs (the median expected): **240 hand + 448 procedural-but-unique = 688 memories experienced.** This is the perceived narrative density the original GAME_DESIGN.md was aiming for, achieved with the *production* budget the review demanded.

### 3.2 Floating-slot rules

1. Each floating slot is filled at the start of the playthrough from a *deterministic but unique* roll based on (player_save_id × seed_year).
2. Once filled, the floating memory is **canonical for that save** — it does not re-roll.
3. The procedural assembler obeys villager voice constraints (Codex 02 § 7.1).
4. **Echo continuity is preserved** — if a floating memory mentions a relationship (e.g., Doris speaking of a niece), the niece either is or becomes a procedural villager in the same playthrough.

### 3.3 Why this is novel

Procedural narrative is usually **either short-form** (Hades's room-vignettes) **or grand-scale** (Dwarf Fortress's history). A *cozy persistent-narrative game with a procedural memory-floating layer that obeys hand-sealed canon* — to my knowledge, this has not shipped. It is the unification of cozy-permanence + roguelite-variety.

---

## 4.0 Layer 2 — The Weekly Memory Market

### 4.1 What it is

Every Saturday in-game, **the market in Embershade Square** hosts a memory exchange. Visitors from elsewhere — *the Visitors faction* (Codex 08 § 6) — bring memories from outside the valley. The player can buy, sell, or barter.

### 4.2 Why this is a replayability engine

- **Each Saturday's stock is unique.** Procedurally drawn from a 600-clause library of "outside memories" plus 80 hand-authored special weekly memories.
- **Rare memories appear weekly.** ~3% of weekly markets contain a *Predecessor Fragment* (Codex 03 § 10.1). Returning players may go market-hunting.
- **Player-traded memories enter the market.** A memory the player sells is *added to the market's stock for other (or future) players' weeks*, with a salted seed so no two players see the same player-traded memory at the same time.

### 4.3 Market mechanics

```
SATURDAY MARKET (4 in-game hours, ~6 real-time minutes if fully engaged)
  Stalls: 6 (3 fixed visitor characters + 3 rotating)
  Memories on offer per stall: 3–8
  Pricing: market-driven (Codex 10 § 6 — supply/demand engine)
  Special events:
    - Auction Day (1× per season): high-value memories on bid
    - Forgetting Day (rare): some memories sold here are *false* — composted artifacts of the Stock Exchange
```

### 4.4 The Auction Day mini-arc

Once per season, **the Auction Day** brings a known high-value memory to public bid. Examples:

- *"The Last Recipe of the Predecessor"* — a memory containing one of Marin's bee-keeping techniques. Bid against 3 NPC bidders; Vance Ashby is sometimes one.
- *"A Letter from the City"* — a forged memory created in Wells; buying it is a moral trap.
- *"The Singing Hive's First Song"* — buying it gives the Garden Track a 1-time boost.

Auctions are **bid-by-tea** — the player commits memory teas + coin in escrow. Lose the auction → tea is returned. Win → tea is consumed. *No real-time auction pressure (cozy contract).*

---

## 5.0 Layer 3 — The Drifter Mode

### 5.1 What it is

A standalone mode, unlocked at the *end* of a playthrough (any of the 6 endings), in which the player becomes **a Drifter** — a traveling memory broker without a fixed Hollow.

### 5.2 The Drifter's loop

The Drifter visits a procedurally-generated village (different from Embershade) for 5–8 in-game days. They:

- Set up a temporary stall in the village square.
- Trade with 6–12 procedural villagers (drawn from a 60-villager template library).
- Solve **one** village-specific small mystery (3–5 hours).
- Leave on Day 8.

### 5.3 Why this is valuable

- **Short-form session.** A Drifter run is 5–8 hours. Perfect for a cozy player whose canonical save is on hold.
- **Procedurally varied.** No two Drifter villages are identical (templates × seeds = ~140 distinct combinations).
- **Mechanically new.** The Drifter has no shop. They have a backpack with 6 orb slots. They cannot Cleanse without local water. They are *constrained* in a way the Hollow player is not. This forces creative play.
- **Lore-connected.** Each Drifter village has a 1% chance of revealing a piece of *the city of Wells's encroachment* — the Memory Stock Exchange's expansion. The Drifter Mode is where the player most directly **resists Vance Ashby's wider scheme.**

### 5.4 Sample Drifter villages (5 of 14 templates)

| Village | Setting | Mystery |
|---|---|---|
| **Thatchby** | Roadside on the trade route | A traveling player has been selling false memories |
| **Lakefoot** | Lakeside fishing village | Three villagers all claim to be the rightful owner of one memory |
| **Hollybridge** | Bridge town between Wells and the rural valleys | A memory of the bridge's construction is going missing |
| **Quiet Reach** | A monastery | A monk has been keeping a memory she should have returned |
| **Brackenwell** | A village near the city | Vance's franchise office has opened. The villagers are uneasy. |

Each template has 4 hand-written variants. **Total Drifter content: 56 hand-written village-mysteries, plus procedural villagers within them.** Writing budget: ~120,000 words across the bible's lifetime — added as **post-launch content** (Year 1 free updates per Codex 16 § 2).

### 5.5 The Drifter's currency

The Drifter uses **Stranger's Coin** — a separate currency that does not flow back to Embershade. Earned in the field; spent on travel + supplies. This **economically isolates** the Drifter Mode from the canonical playthrough, preserving the canonical playthrough's stakes.

---

## 6.0 Layer 4 — New-Game+ Threads

### 6.1 What it is

When a player completes any ending and starts a new game, they may choose to begin a **Thread**: a new playthrough with a **specific lens** that changes which memories surface, which villagers come forward, and which arcs are emphasized.

### 6.2 The Threads

| Thread | Lens | What Changes |
|---|---|---|
| **Thread of Doris** | Doris-focused | Doris and her web of relationships are central. Halia gets less emphasis. The Mayor's arc is delayed. |
| **Thread of the Mayor** | Civic | Sebastian, Anselm, Lavinia central. The Lawsuit fires earlier. The Forgotten Year is harder to access. |
| **Thread of the Lost Year** | Forgotten Year-forward | Ren approaches you on Day 5. The orb is findable by Week 4. Halia's arc dominates. |
| **Thread of the Apprentice** | Ren-forward | Apprenticeship begins on Day 12 (not Day 70). Teaching mechanics deepen. |
| **Thread of the Visitor** | Vance-forward | Vance arrives on Day 8 (not Spire-Month Week 1). The Memory Stock Exchange's reach is larger. The Lawsuit and Vance entangle. |
| **Thread of the Echo** | Marin-forward | The Echo Hologram is fully active from Day 1. Predecessor fragments are more findable. |
| **Thread of the Quiet** | Comfort-forward | Soft Days are unlimited. No Tribunals fire unless the player triggers them. *The most cozy thread.* |
| **Thread of the Thief** | Mystery-forward | Halia's thief arc is foregrounded. The Black Market is the spine. |
| **Thread of the Bees** | Garden-forward | The Memory Bees produce honey from Day 1. The Singing Hive can appear early. |
| **Thread of the Lawsuit** | Procedural law-arc | The Magistrate is *the* central NPC. Multiple Lawsuits across the year. |

**10 Threads total.** Each is hand-authored and shifts 12–25 narrative beats. A *Thread* takes 20-40 hours to fully experience.

### 6.3 Why Threads, not "Difficulty"

The cozy player does not want "harder." The cozy player wants *different.* Threads are **different framings of the same village.** Each is canon. Each gives the player a new emotional throughline. **None is "endgame difficulty."**

### 6.4 Thread interaction with Procedural Memory Generation

Threads bias the procedural assembler's outputs. In *Thread of Doris*, ~40% of procedural floating slots resolve to memories that touch Doris's web. The bias is *narrative*, not mechanical — it makes the village *feel* tilted toward the Thread's protagonist.

---

## 7.0 The Sukenobu/Cordray Resolution — the Hand-Sealed/Procedural Compact

Esme Cordray (Codex 08) was correct to be concerned that procedural content dilutes choice weight. The compact:

1. **All Tariff-bearing choices fire on hand-sealed memories.** Procedural memories do not carry Tariff weight on player decisions — they are *texture*, not *consequence*.
2. **Procedural memories can change** the *Memory Integrity* dimension of a villager, but never *Trust* or *Vow*. The cozy player's first playthrough's moral spine is hand-sealed.
3. **In NG+ Threads**, the Tariff weight is *reduced by 40%* for repeated choices, reflecting that the player has been here before. This is the *replayability decay* that prevents NG+ from feeling identical.

This compact protects the first playthrough's gravity while opening the door to replayability.

---

## 8.0 The Thief Path (the "darker" mode)

> ⚠ Lore-sensitive. Spoiler.

For players who want to **break the Code**: an opt-in path that activates a Thief Mode within the canonical playthrough. The player becomes the Thief Halia was on her way to becoming — *but more skilled.*

### 8.1 How to unlock

Vow 1 (Consent) < 30 + Public Standing < 20 + Pickle Approval < 10 + having Severed at least 3 memories without owner consent.

### 8.2 What it adds

- **Stealth mini-game** (the *only* such mini-game in the game; opt-in only). Sneak into a villager's home at night and Sever a fragment.
- **Black-market sales** to Vance, at premium prices.
- **The Echo Hologram refuses to speak to you.**
- **Pickle does not return.**
- **Mariska visits once, speaks once, never again.**
- **The endings collapse to *The Vance Victory* or *The Keeper's Hoard* only.**

### 8.3 Why this exists

A subset of cozy-narrative players want to know *what the dark version of this game would be.* Citizen Sleeper proved this audience exists (~14% of players). The Thief Path is a *complete, hand-crafted alternative.* It does not feel like an Easter egg; it feels like a different game.

### 8.4 Why this is not "evil mode"

It is not punishment-design. It is *thematic-design.* It tells a coherent story of a Keeper who became what the Memory Stock Exchange wanted. It has its own emotional weight. Pell Doyne (Codex 06) signed off on it as a content-warning-gated opt-in.

---

## 9.0 Procedural Beats — the system-level integrations

Procedural content also fires across:

| System | Procedural Beat |
|---|---|
| Letter-Bird Network (Codex 15) | Random memory arrivals from async players |
| Pickle's tastings (Codex 12) | Procedural assessment of any procedural memory |
| Memory Honey (Codex 04 § 5) | Procedural emotional blends |
| Festivals (Codex 16 § 2) | Procedurally varied festival vignettes |
| Echo Web (Codex 02 § 9) | Procedural memories obey deterministic echo seeds |
| The Drifter Mode (this codex) | Procedural villages |

The discipline: **procedural content is everywhere; procedural *choice* is nowhere.** That is the cozy game's rule.

---

## 10.0 Lifetime Hours Modeling

| Player Persona | First Playthrough | NG+ / Threads | Drifter Runs | Weekly Markets | Total |
|---|---|---|---|---|---|
| *Single-arc cozy player* | 28 hrs | 0 | 0 | included in 28 | **28** |
| *Median cozy-narrative player* | 32 hrs | 18 hrs (one Thread) | 6 hrs (one Drifter run) | included | **56** |
| *Deep-engaged player* | 40 hrs | 60 hrs (3 Threads) | 24 hrs (4 Drifters) | included | **124** |
| *Long-tail cozy player* | 38 hrs | 120 hrs (all 10 Threads partial) | 56 hrs | 12 hrs (auction hunter) | **226** |

**Median across the cozy audience (modeled): ~52 hours.** Above the 45–60 KPI target.

---

## 11.0 The Cosmetic & Cosmetic-Earning Layer

Replayability needs visible reward without grind. Cosmetics earned across systems:

| Cosmetic | Earned via | Visual |
|---|---|---|
| **Marin's Apron** | Day 1 — given | Wearable |
| **The Brass Spoon** | Polish-mastery rank 5 | Inventory item |
| **Mariska's Shawl** | Vow 6 > 80 | Wearable |
| **The Lantern of the Last Light** | Vow 7 max | Hung in shop |
| **The Singing Hive's Honey Spoon** | Garden Track max | Inventory; gold-rimmed |
| **Pickle's Tea Cup** | Pickle Approval > 80 | Cup is visibly chipped (in honor) |
| **The Sommelier's Spoon** | Memory Mastery max | Endgame; never used, only displayed |
| **Letter-Bird Perch (Gilded)** | 100 letter-birds received | Shop decor |
| **The Predecessor's Loom** | All 17 fragments collected | Shop decor; spins gently |
| **The Three Apples Mug** | Tomek Trust > 80 | Drinking from it adds 1 line of dialogue |
| **The Schoolhouse Bell** | Lavinia's final lecture | Hung outside the shop |
| **The Thief's Cloak** | Thief Path completed | (Dark cosmetic) |

20+ total. Each cosmetic is **earned through narrative, never through grind.** Cosmetics-via-time-played are forbidden.

---

## 12.0 Closing

> *"Roguelite design in a cozy game is a stilettos-and-cotton problem. You can wear both, but it takes thought. The trick is that the first playthrough must feel hand-made and the second playthrough must feel *generously hand-made for someone else.* Threads do that. Drifter Mode does that. The Weekly Market does that. The procedural floating slots do that. None of them shout. All of them whisper: come back when you're ready. There's more village than you saw."*
>
> — *Ari Sukenobu*

— *End of Codex 09. Next: `10_ECONOMY_AND_REPUTATION.md`.*
