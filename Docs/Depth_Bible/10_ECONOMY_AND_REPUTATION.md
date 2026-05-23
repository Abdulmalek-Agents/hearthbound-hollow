# Codex 10 — **Economy & Reputation Systems**
### Specialist: **Beatrix Holm, Economy & Reputation Systems Designer**
*(13 yrs · ex-EVE Online economist (CCP), ex-Stardew port consultant on Switch, ex-Klei senior systems on Don't Starve Together)*

> *"The economy of a cozy game is the bloodstream of its player engagement. It is not 'gold-grind.' It is **the friction-balance between the warm and the achievable.** Too much friction and the cozy player feels punished. Too little and the shop upgrades feel given. Below is the bloodstream — three currencies, two markets, five villager-faction price modifiers, and one strict anti-grind rule that protects the cozy contract."*

---

## 1.0 Mission

Design an economy and reputation system that:

1. Supports the Hollow upgrade track (Codex 04 § 3) — the player can afford max-level by Day 100 of moderate play.
2. Creates **price tension** without exploitation pressure.
3. Honors the cozy contract — *no grinding the same orb 50 times for coin.*
4. Models supply/demand for the Weekly Memory Market (Codex 09 § 4).
5. Reinforces the village state machine (Codex 08) without turning villagers into vending machines.

---

## 2.0 The Three Currencies

### 2.1 Coin

The standard village currency. ~50/day average flow. Spent on:

- Hollow upgrades (Codex 04 § 3).
- Memory teas and herbs (Garden — Codex 04 § 5).
- Market purchases at the Weekly Market.
- Travel costs in Drifter Mode (Codex 09).
- Pickle's preferred fish (yes, a recurring expense, ~3 Coin/week).

### 2.2 Cinder

A soft prestige currency, earned by **non-transactional service** to the village. Cinder cannot be earned by selling memories. It is earned by:

- Listening to a villager without buying (the Vow 7 reward — Codex 04 § 6).
- Returning a lost memory (Vow 2).
- Completing a Tribunal in honor.
- Helping at festivals.
- Composting memory fragments.
- Sitting with a grieving villager.

Cinder is spent on:
- Specialty Hollow upgrades (Levels 3, 8, 10 — Codex 04 § 3).
- The Confession Booth's deeper functions.
- Mariska's lessons (Codex 13 § 2 — Severing).
- The Apprenticeship's milestone moments.
- Composer's commissions (Codex 14 — diegetic music unlocks).

**~5 Cinder/week** is the median accrual rate. Cinder is the **moral economy** of the game — it accumulates with virtue, not effort.

### 2.3 Stranger's Coin (Drifter Mode only)

Earned and spent only in Drifter Mode (Codex 09 § 5.5). Economically isolated. No exchange rate with Coin or Cinder.

### 2.4 The decision *not* to have a fourth currency

This codex considered a "Memory Essence" rarity currency. **Rejected.** The cozy player resents currencies that look like crafting-game grind tokens. Three currencies, all named in-fiction (no "gems," no "tokens," no "shards"). This is the Holm Rule.

---

## 3.0 Memory Pricing — the Heart of the Economy

### 3.1 The pricing formula

Every memory has a base price determined by:

```
base_price = weight^1.4 × clarity_factor × palette_modifier × narrative_density
```

Where:
- **weight** (1–10): the significance to the seller (Codex 02 § 2.1).
- **clarity_factor** (0.5–1.5): cleaner orbs fetch more.
- **palette_modifier** (0.7–1.3): some palettes (longing, awe) are rare and command premium; others (mundane joy) are common.
- **narrative_density**: how many Echoes the memory contains (more echoes = more value).

Base prices range from **2 Coin** (a small unweighted memory) to **80 Coin** (a deep, weight-9 memory). Predecessor fragments are priced separately (§ 7).

### 3.2 The five faction price modifiers

Each faction (Codex 08 § 6) modifies prices when a villager from that faction is the *seller* or *buyer*:

| Faction | Modifier (selling to player) | Modifier (buying from player) |
|---|---|---|
| Civic Elders | ×0.95 (they discount in good faith) | ×1.15 (they pay premium for honor) |
| Makers' Guild | ×1.10 (they know value) | ×1.05 |
| Carers | ×0.85 (they undervalue their own memories) | ×0.90 |
| Quiet Folk | ×1.20 (memories are sacred) | ×0.80 (they don't want to enrich the buyer) |
| Visitors | ×1.30 (rare; from outside) | ×1.40 (they want exotic) |

The **Honor the Price** option in the negotiation (Codex 02 § 8.1) pays the *unmodified* base price — a meaningful generosity to Carers, a meaningful self-restraint with Visitors.

### 3.3 Trust price elasticity

Villager Trust (Codex 08 § 2) also affects pricing:

| Trust Tier | Selling Price Modifier (the price the villager *asks*) | Buying Price (when player wants to buy from villager) |
|---|---|---|
| 1 (Stranger) | ×1.25 (defensive) | ×0.70 (won't sell deep memories at all) |
| 2 (Acquaintance) | ×1.10 | ×0.85 |
| 3 (Friend) | ×1.00 | ×1.00 |
| 4 (Confidant) | ×0.90 | ×1.10 |
| 5 (Family) | ×0.80 (they want you to have it) | ×1.25 (you're trusted with the deepest) |

The player **buying cheap from a Tier-5 villager** technically saves Coin but loses Vow 5 (Honest Coin) integrity if done repeatedly. This is the *moral price elasticity.*

---

## 4.0 The Anti-Grind Discipline

The cozy player must **never feel the game wants them to "farm Coin."** Five rules:

### 4.1 The Five Anti-Grind Rules

1. **No memory generates Coin if re-traded with the same villager.** A villager who buys back their own memory pays *less* than market, never more.
2. **The Hollow generates no passive Coin.** No "sell-while-you-sleep" overnight stock-clearance income.
3. **Cosmetic and upgrade Coin costs are bounded.** Total Coin for full Hollow max-level: ~5,000. Modeled Coin earned in 100 in-game days at moderate play: ~5,000. *Perfectly aligned.*
4. **Coin-earning that requires repeated identical actions is forbidden.** No "polish 12 orbs / day for 1 Coin each." The Holm Test (§ 4.2) is applied to every Coin source.
5. **Coin is not a victory metric.** The endings (Codex 08 § 7) do not reward Coin accumulation. The richest player is *not* the winning player.

### 4.2 The Holm Test

Every Coin source must answer YES to:

- Is this Coin earned through a *narratively unique* moment? ✓
- Will the player remember earning this Coin? ✓
- Is this Coin earned at a rate that does not exceed the upgrade cost curve? ✓

If a source fails any test, it is restructured or removed.

---

## 5.0 The Weekly Market Economy

(See Codex 09 § 4 for the player-facing description.)

### 5.1 Supply / demand model

The Weekly Market is driven by a small economic simulation:

- **6 stall vendors** (3 fixed Visitor characters + 3 rotating).
- Each vendor has a **stock** drawn from a tagged library.
- **Demand** is influenced by: village mood, season, recent player sales (the village wants what is rare).
- **Prices** drift week-over-week based on demand × scarcity.

### 5.2 Price drift example

If the player has sold 4 grief-palette memories to outside Visitors recently, grief-palette memories become *more expensive* at the next market (supply down). Conversely, joy-palette memories become *cheaper* (perceived oversupply).

This creates a **market dynamic the player can observe** but never *must* exploit. Players who do exploit it (the "memory arbitrageur" archetype) are catered to — but they also get a quiet Pickle commentary about it.

### 5.3 The Auction Day specifics

| Auction Memory | Base Price | NPC Bidders | Typical Winning Bid |
|---|---|---|---|
| "The Last Recipe of the Predecessor" | 320 Coin | Vance, Mariska, Esher | 480–620 Coin + 12 Cinder |
| "A Letter from the City" (forged) | 80 Coin | Vance only | varies wildly |
| "The Singing Hive's First Song" | 280 Coin | Doris, Idris, Mariska | 360–500 Coin |
| Random rare memory | 100–250 Coin | 2–3 NPCs | 1.5× base |

Bidding mechanic: **commit Coin + 1 tea**. The tea you commit signals *how badly you want it.* Strong tea = visible commitment to other bidders. *Mariska's tea is undefeatable.* (The player can never out-bid Mariska in a memory she truly wants. This is canonical lore — Mariska does not lose auctions.)

---

## 6.0 Reputation Tiers — beyond Trust

A villager's **Trust** (Codex 08 § 2) is the relationship layer. But the village also has a **reputation tier system** that affects the player's standing.

### 6.1 Public Standing tiers

| Standing | Range | Description |
|---|---|---|
| **Unknown** | 0–19 | A stranger. New arrival. |
| **Tolerated** | 20–39 | A presence. Greeted but not invited. |
| **Welcomed** | 40–59 | A neighbor. Default mid-game. |
| **Honored** | 60–79 | Named in the village's stories. |
| **Beloved** | 80–100 | Spoken of at distance. The village is *yours*. |

Public Standing affects:
- Walk-in frequency to the Hollow (Beloved = ~2× Welcomed).
- Festival invitations.
- Mayor's blessings.
- The Tribunal's default lean (Beloved players are presumed honest).

### 6.2 Faction tier rewards

(Cross-referenced from Codex 08 § 6.) Each faction at 70+:

| Faction | Reward |
|---|---|
| Civic Elders | The Mayor's Hollow Level 12 blessing |
| Makers' Guild | Hollow upgrades at 15% discount |
| Carers | Composted memories yield rarer flora |
| Quiet Folk | Mariska teaches Severing earlier |
| Visitors | Letter-Bird network expanded |

---

## 7.0 Predecessor Fragment Pricing

The 17 MAR fragments (Codex 03 § 10.1) are **not** for sale on the Memory Market. They are:

- **Gifts** (some are given by villagers who trust you).
- **Quest rewards** (Revelation Chapters).
- **Found objects** (Library, schoolhouse).
- **Honey-formed** (the apiary).
- **Bee-deposited** (in Memory Honey).
- **Async-received** (Letter-Birds from other players).
- **Severed from yourself** (the endgame).

Some can be *purchased* from Vance Ashby (he has acquired three of the 17 through his network). Buying from him costs **240 Coin per fragment + 1 Vow 1 integrity hit per purchase.** This is a moral expense, not a Coin expense.

The economy of fragments is **deliberately not Coin-driven.** This is the design discipline that elevates them above commodities.

---

## 8.0 The Mobile Premium-Economy Discipline

(Anchoring [REV § 5 Risk 7].) Mobile players will be offered:

- **The full game at $9.99**, no F2P contamination.
- **Cosmetic shelf-decor pack at $1.99** — purely visual, no Coin, no Cinder, no progression.
- **Soundtrack at $4.99** in-game (also on Spotify/Bandcamp).
- **NO** energy systems, daily login rewards, gacha, time-skips, season-pass, currency packs, ads.

This is the **non-negotiable mobile compact.** Breaking it kills the brand. **It is enforced at the publisher contract level.**

---

## 9.0 The Compost Economy (NEW LAYER)

(Cross-referenced from Codex 04 § 5.4.)

Composted memories produce Memory Soil, which produces Memory Flora. The Flora can be:

- **Brewed** into specialty teas (Codex 04 § 5.2).
- **Gifted** to villagers (Rilla's memorial; Sister Marrow's offerings).
- **Sold** at the Weekly Market (rare; high Cinder yield, modest Coin).

The Compost Economy is the **circular economy of failure**. It is also a soft critique of Vance Ashby's extractive model: the Hollow *renews* what it cannot save. The City of Wells *liquidates* what it cannot use. This is, structurally, a politics in mechanics.

---

## 10.0 Memory Honey Pricing

Memory Honey (Codex 04 § 5.3) is **never sold to villagers.** It is a personal collectible / dietary. The bees produce it; the player consumes it; Pickle taste-tests it; some jars contain predecessor fragments.

Memory Honey is **off-market** by lore (the Keeper does not sell their own production — Vow 5 elaborated).

---

## 11.0 The Borrowed Memory Pricing

(Cross-referenced from Codex 04 § 9.)

When a villager *rents* their own memory back for a day:

- **Base rental fee:** weight × 2 Coin/day (e.g., a weight-8 memory rents at 16 Coin).
- **Trust modifier:** -10% per Trust Tier above 1.
- **Family tier (Tier 5):** the rental is *free*. Villager pays in tea instead.

Borrowing is **profitable, but only modestly**. ~20 Coin/week if the player runs 3 borrowings. This is the *recurring soft-revenue stream* that helps the Hollow's upgrade pacing — and it has an emotional dimension to it (the villager's mood-shift on that day).

---

## 12.0 The Coin-Flow Modeling

| Source | Avg Coin/day | Notes |
|---|---|---|
| Daily transactions (4–6 visitors) | ~25 | Hand-crafted variability |
| Weekly Market (averaged daily) | ~10 | Saturdays only |
| Borrowed Memory rentals | ~3 | Mid/late game |
| Memory Honey sale (offline lore: none) | 0 | Off-market |
| Compost-Flora sales | ~1 | Niche; ~5/week |
| Festival income | ~10 (averaged) | Seasonal |
| Pickle's hunting (yes, the cat occasionally brings home a mouse-with-coin-in-its-mouth) | ~1 | Comedy beat |
| **Total avg daily Coin** | **~50** | Per § 1 KPI |

Coin spend rate at moderate engagement:
- Hollow upgrades: ~30/day
- Teas: ~5/day
- Festivals: ~5/day
- Pickle's fish: ~0.4/day
- Drifter travel: variable
- Memory Market buys: ~10/day

**Net: ~0/day at equilibrium.** This is the **anti-hoarding design** — the cozy player does not accumulate huge reserves; the cozy player *circulates.*

---

## 13.0 Reputation Decay (and lack thereof)

| Dimension | Decay Rate | Why |
|---|---|---|
| Trust (per villager) | -1/week without interaction | People drift |
| Public Standing | -0.2/week without participation | Reputation lasts |
| Faction Trust | -0.5/week | Communities forget slowly |
| Vow Integrity | No decay | Vows are character, not mood |
| Pickle Approval | -0.1/week | Pickle is loyal but observant |

Decay is **gentle**. The cozy player who takes a week off does not return to a shattered reputation. The **gentle decay rule** is a comfort-layer discipline (Codex 06).

---

## 14.0 The "Free Service" Mechanic

Vow 6 (Open Door): no villager shall be turned away for inability to pay.

Mechanically: the player can perform Polish/Cleanse work *for free* on any villager who cannot afford it. The transaction:

- Earns 0 Coin.
- Earns 2 Cinder.
- Increases Trust by +3.
- Increases Public Standing by +1.
- Pickle approves +2.
- The villager will, later, often *gift* the player something unprompted.

The free-service mechanic is **financially viable in the game economy.** The player who serves the village for free is *not punished.* Quite the opposite: they are *the canonical Keeper.* This is the bible's moral spine made visible in numbers.

---

## 15.0 The Black Market Economy (the Thief Path)

(Cross-referenced from Codex 09 § 8.) The Black Market — accessed only by the Thief Path — has:

- **High prices** (×2.5 base).
- **No reputation cost on villagers** (because they don't know).
- **Massive Vow integrity cost** (every sale: -10 Vow 1, -5 Vow 4).
- **Pickle leaves.** This is the price.

The Black Market is **economically optimal in pure Coin terms** and **emotionally catastrophic.** This is the design test of the player's character.

---

## 16.0 KPIs

| KPI | Target |
|---|---|
| % of players who reach Hollow Level 12 in playthrough 1 | 55% |
| Median Coin balance at end of Year 1 (in-game) | ~120 |
| % of transactions at Honor-the-Price | 35%+ |
| % of players who use Compost-Flora at least once | 60% |
| % of players who Borrow-Memory at least once | 50% |
| Average Cinder spent per playthrough | ≥80 |
| Black Market path adoption rate | 8–14% |

---

## 17.0 Closing

> *"A cozy game's economy succeeds when the player cannot tell it is an economy. They simply notice that the things they want to buy are exactly the things they have just earned, and that nothing about the earning made them feel like a worker on someone else's farm. The bloodstream is in balance. The body is well."*
>
> — *Beatrix Holm*

— *End of Codex 10. Next: `11_MEMORY_DREAM_DIRECTOR.md`.*
