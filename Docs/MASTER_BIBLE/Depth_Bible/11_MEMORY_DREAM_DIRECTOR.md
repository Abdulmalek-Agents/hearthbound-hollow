# Codex 11 — **Memory Dream Director**
### Specialist: **Sven Aleko, Memory Dream Director / Cinematic Lead**
*(17 yrs · ex-Naughty Dog cinematic designer, ex-Thunder Lotus animation director (Spiritfarer), animator on Cuphead)*

> *"The review was right: per-memory unique illustrated vignettes is an $800k–$1.8M art line that breaks the studio. The fix is not 'fewer dreams.' The fix is **parameterized dreams** — 30 hand-painted set-piece scenes that are combined, lensed, and recomposed at runtime into ~1,350 perceived unique vignettes. The cozy player will believe they have seen a hand-painted painting every night for a year. Because, in a sense, they have."*

---

## 1.0 Mission

Resolve [REV § 5 Risk 2 — Memory Dream art budget unbudgeted] explicitly. Provide a complete, costed, schedulable, repeatable art production pipeline for the **Memory Dream** feature that:

1. Delivers >1,000 *perceived unique* dreams across a playthrough.
2. Stays inside a hard art budget of **$255k**.
3. Maintains a quality bar comparable to Spiritfarer's vignettes.
4. Integrates with Codex 02's Memory Card schema and Codex 05's Dream Duel choice engine.

---

## 2.0 The Parameterized Dream Engine

### 2.1 The architecture

A Memory Dream is **not** a unique illustrated cutscene. It is a **composition** of:

```
DREAM = SET_PIECE_SCENE × EMOTION_LENS × MOOD_LIGHTING × CAST_FIGURES × AUDIO_BED
```

Each axis has a finite, hand-crafted library. The composition assembles them at runtime according to the Memory Card's metadata (Codex 02 § 2.1).

### 2.2 The five axes

| Axis | # of variants | Asset Type | Cost per variant |
|---|---|---|---|
| **Set-Piece Scenes** | 30 | Hand-painted background + foreground in 4 zoom layers (parallax) | $4–6k |
| **Emotion Lenses** | 9 | Color-grading LUT + particle system + audio EQ + composer cue | $2–4k |
| **Mood Lightings** | 5 | Time-of-day lighting rig with 3 ambient particle drifts each | $5–8k (one-time tech) |
| **Cast Figures** | 60 (12 sealed × 5 ages + 18 procedural archetypes) | Layered character sprites with 6 poses each | $2–3k each |
| **Audio Beds** | 12 | Composer-supplied stems (Codex 14) | included in composer budget |

30 × 9 × 5 = **1,350 visual compositions.** With cast figures and audio overlays, the perceived variant count is *much* higher; the player will, in practice, never see the same dream twice in 60 hours of play.

### 2.3 The compositional discipline

Every set-piece scene is **designed to read at multiple emotional palettes.** A kitchen at dusk is painted such that:

- Under the **JOY lens**, it is golden, alive, with steam rising from the kettle.
- Under the **GRIEF lens**, it is amber-grey, the kettle is silent, the kitchen feels *very large.*
- Under the **WONDER lens**, the same kitchen is **larger still**, with the warmth concentrated and the corners filled with soft impossible light.
- Under the **GRACE lens**, the kitchen settles. The kettle cools naturally.

This means **the art is doing emotional work across all 9 lenses.** The painter must storyboard for the *worst* lens (GRIEF) — the discipline that the kitchen still reads as warm-in-its-bones even in grief.

---

## 3.0 The 30 Set-Piece Scenes

These are the hand-painted foundations of every dream in the game.

| # | Set-Piece | Tonal Range | Used by Memories |
|---|---|---|---|
| 1 | A kitchen at dusk | grief/joy/grace | 47 |
| 2 | A river crossing in autumn | grace/dread | 24 |
| 3 | A wedding morning | joy/longing | 18 |
| 4 | A schoolroom in winter | wonder/longing | 19 |
| 5 | A garden in first frost | grace/wonder | 28 |
| 6 | A funeral at dawn | grief/grace | 12 |
| 7 | A child's bedroom | wonder/longing | 26 |
| 8 | An attic of old letters | shame/longing | 16 |
| 9 | A market square in summer | joy/awe | 21 |
| 10 | A barn at midnight | dread/grace | 11 |
| 11 | A doorway between two rooms | longing/shame | 23 |
| 12 | A long road | longing/awe | 17 |
| 13 | A library by candlelight | wonder/awe | 19 |
| 14 | A bedroom of a sick person | grief/grace | 14 |
| 15 | A meadow with bees | wonder/joy | 22 |
| 16 | A boat at the weir | grace/dread | 9 |
| 17 | A festival lantern walk | joy/grace | 18 |
| 18 | A kitchen at first light | grace/longing | 31 |
| 19 | A stairwell | dread/wonder | 12 |
| 20 | A village square at midnight | dread/awe | 8 |
| 21 | A cemetery in greenmouth | grief/grace | 11 |
| 22 | A workbench (the Hollow) | wonder/shame | 19 |
| 23 | A church interior | awe/shame | 16 |
| 24 | An inn's back room | shame/grace | 13 |
| 25 | A forest path in fog | dread/awe | 14 |
| 26 | A child's hand holding an adult's | grace/longing | 9 |
| 27 | A teapot on a stove | grace/joy | 26 |
| 28 | A folded shawl on a chair | grief/grace | 8 |
| 29 | A letter on a table | longing/shame | 18 |
| 30 | A door opening | awe/longing | 24 |

Total memory-uses across the bible: ~604 (multiple memories share set-pieces, lensed differently). The reuse pattern is *not* lazy — it is the dream language of recurrence. Memory works that way.

### 3.1 The reuse-as-language principle

When *Doris's Last Light* uses Set-Piece #1 (kitchen at dusk) lensed GRIEF, and *Doris's First Wedding Honey* uses Set-Piece #18 (kitchen at first light) lensed JOY, the player **feels the kitchen is the same kitchen.** This is a hand-crafted decision: kitchens repeat because Doris's life happened in kitchens. **The repetition is the meaning.**

This was the **Spiritfarer breakthrough** that nobody has replicated commercially — recurring visual motifs as emotional anchors. We codify it.

---

## 4.0 The Nine Emotion Lenses

Each lens is a complete tonal package:

| Lens | Color Profile | Particle System | Audio EQ | Composer Cue |
|---|---|---|---|---|
| **JOY** | Saturated golden, warm yellows | Dust motes, drifting petals | Bright mid, soft highs | Major-key cue, often a single instrument |
| **GRIEF** | Muted amber, deep slate | Slow falling ash, faint rain | Low rumble + air | String drone in minor |
| **SHAME** | Cool gray, wet stone | Quiet steam, rising | Compressed; voice forward | Solo piano, hesitant |
| **AWE** | Twilight blues, lit windows | Stars + slow embers | Wide, deep reverb | Open chord, sustained |
| **LONGING** | Sepia, soft purples | Pollen, drifting paper | Mid-soft, distant | Solo cello |
| **DREAD** | Greens shifted to slate, charcoal | Static, faint flicker | Low frequency present | Dissonant minor, sparse |
| **GRACE** | Warm whites, soft golds | Snow, light feathers | Smooth, no harshness | Wood instruments, single line |
| **WONDER** | Iridescent rim-light, pastels | Sparkle, breath | Slight reverb tail | Childlike scale |
| **COMPOSITE** | Mix of two lenses' palettes | Mixed | Mid balanced | Bridge between cues |

Each lens has **6–8 minor sub-lenses** (e.g., GRIEF-October, GRIEF-Indoors, GRIEF-Long-Held) for finer tuning. **Total lens variants: 60**, each parametric on the master 9.

---

## 5.0 The Memory Dream Production Math

### 5.1 The itemized art budget

| Line Item | Quantity | Unit Cost | Subtotal |
|---|---|---|---|
| Set-piece scenes (30) | 30 | $5,000 avg | $150,000 |
| Emotion lenses (9 master + 51 sub) | 60 | $500 (sub) / $4,000 (master) | $61,500 |
| Mood lighting rigs (5) | 5 | $7,000 | $35,000 |
| Cast figures (60 archetypes × 6 poses) | 360 sprite-assets | $200 each | $72,000 (offset by reuse across game) |
| Compositor tooling (one-time) | 1 | $25,000 | $25,000 |
| Quality-pass studio time | 8 weeks senior art lead | $7,500/wk | $60,000 |

**Total Memory Dream art budget: ~$403,500.**

Note: $72,000 of cast-figure cost is shared with the main game's character art (the dream-Sebastian sprite is also used in the village). **Effective Memory-Dream-only budget: ~$331,500.**

This is **higher than the original Codex 01 § 3 estimate of $255k** — Halvor's audit was conservative. The honest production number is **$330–410k.**

This is still **5x cheaper than the worst-case naive per-dream unique illustration approach** ($1.6M–$2.4M).

### 5.2 The schedule

| Quarter | Output |
|---|---|
| Q1 | Set-pieces 1–10 + master lenses 1–4 + mood lighting tech |
| Q2 | Set-pieces 11–20 + master lenses 5–9 + cast figures (sealed villagers) |
| Q3 | Set-pieces 21–30 + sub-lenses + procedural-villager archetypes |
| Q4 | QA pass + compositor tooling + integration with Codex 13's puzzle mini-games |

Two senior environment artists + one senior character artist + one tech artist. Achievable in 12 months.

---

## 6.0 The Memory Dream Player Experience

### 6.1 The cadence

Every in-game evening, after the Ledger review (Codex 04 § 7.1), the player chooses **one memory from their day's interactions** to dream into. The dream plays for ~3 real-time minutes.

The player can:

- **Witness** (default) — passive playback, like watching a short illustrated film.
- **Intervene** (if the dream is a Dream Duel — Codex 05 § 4) — make 3–5 choices that nudge the dream's outcome.
- **Skip** (sets dream to 30-second still-frame summary).
- **Linger** (play in slow motion; the artistic mode; +50% animation time, for the cozy player who wants to dwell).

### 6.2 The morning-after

The next in-game morning, the **dream's residue** affects:

- The Memory Card's metadata (palette balance may have shifted from a Dream Duel).
- The villager who owns the memory may visit unprompted, *responding* to a change you made in the dream (highly poignant).
- A new Echo may surface in the Codex.

This is the *consequence integration* — Memory Dreams are not just pretty interludes; they touch the village state machine (Codex 08).

---

## 7.0 Dream Cinema — the Public Memory Dream

> *Out-of-the-box pillar (Codex 00 § 0.5 #12). Not in any shipped commercial game.*

At Hollow Level 9 (Display Case unlocked), the player can **screen a consented memory publicly** at the village hall. Once per season.

### 7.1 Mechanics

The player and 8–14 villagers attend. The dream plays at large-scale on a hand-cranked memory-lantern. Villagers **react** in real-time:

- They cry. They laugh. They look at each other. Some leave the room.
- Each villager's reaction is hand-authored per-memory for the 12 Sealed.
- For procedural memories, reactions are templated from a 60-reaction library tagged by palette.

### 7.2 What it unlocks

- **Public Standing** boost (proportional to memory weight and how-many villagers were moved).
- **A new conversation cycle** — for the next in-game week, villagers reference the dream in their visits.
- **An Echo Web ripple** — if villagers in the audience have memories connected to the screened one, the connections light up.
- **Pickle's verdict** — Pickle attends and writes a one-line review. *("Tonight's screening was a story about lonely tea. It was magnificent. It was also one minute too long.")*

### 7.3 Why this matters

Dream Cinema is **the cozy game's first communal narrative-broadcast mechanic.** It is also the answer to: *"What is the point of the Display Case upgrade?"* The Display Case is a *theatre seat for the village.*

---

## 8.0 The Aleko / Vellis Disagreement (Producer-Resolve Item)

(Cross-referenced from Codex 00 § 0.7.)

I argue for **10 hand-sealed villagers + denser dreams per villager.** Inara Vellis (Codex 02) argues for **12 hand-sealed villagers + standard dream density.**

The math:

- **10 villagers × 16 hand-dreamed memories** = 160 hand-dreamed memories. Average 9 set-pieces × 3 lenses per villager = 27 unique compositions per villager. **Total unique dream-compositions: 270.** Each villager *feels* fully cinema'd.

- **12 villagers × 14 hand-dreamed memories** = 168 hand-dreamed memories. 25 unique compositions per villager. **Total: 300.** More villagers, slightly less density.

I prefer 10 because the 12th and 11th villagers' marginal dream density drops below the level I can defend as Spiritfarer-tier.

Vellis prefers 12 because **Old Mariska and Brother Anselm cannot be cut.**

**This is a producer's call.** Either decision works inside the art budget; the difference is the *quality crest* at the per-villager level.

---

## 9.0 The Lengthy Dream — once per season

Once per season, the player gets a **Long Dream**: a 7–9 minute illustrated short film. Usually attached to a Revelation Chapter (Codex 02 § 10).

Long Dreams are **fully hand-painted** (no template engine). They cost **$18k each**, 8 per game-year. **$144k Long Dream budget** added to the Codex 11 art line. (Already included in § 5.1's $403k total.)

The Long Dreams are the **OST set-pieces of the visual side** — the moments the player will screenshot, the moments the audience will share.

### 9.1 The 8 canonical Long Dreams

| Dream | Trigger | Tone |
|---|---|---|
| **Doris's Wedding Honey** | First trust-tier-4 with Doris | Joy + Longing, ~7 min |
| **The Mayor's First Regret** | Sebastian Tribunal | Shame + Grace, ~8 min |
| **The Singing Hive's First Song** | Garden Level 15 | Wonder, ~6 min |
| **Lavinia's Final Lecture** | Revelation Chapter 13 | Awe + Grief, ~9 min |
| **The Riverman's Crossing** | Restoration Race success | Dread + Grace, ~7 min |
| **The Boy Who Knows He's Missing Something** | Forgotten Year reconstruction | Grief + Wonder, ~8 min |
| **The Predecessor's Apron** | All MAR fragments collected | Longing + Awe, ~9 min |
| **Last Light** | Endgame | Grief + Grace, ~9 min |

These 8 long dreams are **the game's emotional spine.** Each is a hand-painted, hand-animated, fully-scored short film. **They are what the OST and the merch are sold on.**

---

## 10.0 Integration With Other Codices

| Codex | Integration |
|---|---|
| 02 | Memory Card metadata drives lens picker |
| 05 | Dream Duels use parameterized engine for branching frames |
| 07 | Pickle attends Dream Cinema; the silent-zone rule applies |
| 08 | Dream Duels affect Memory Integrity dimension |
| 11 | (this codex) |
| 13 | Some puzzles play *inside* a dream (the Search puzzle for the Forgotten Year orb is a dream-set) |
| 14 | Audio bed is composer-supplied (Codex 14 § 4) |
| 15 | Dream still-frames are auto-saved as wallpapers; share-tagged |

---

## 11.0 Closing

> *"Memory Dreams are the medium of this game. The mechanics are the noun. The dreams are the verb. The trick is to build the engine so that the cozy player feels, every evening, that someone painted this dream for them tonight. They did. We just painted it three months ago and let the engine arrange it for them. That is a craft, not a cheat."*
>
> — *Sven Aleko*

— *End of Codex 11. Next: `12_COMPANIONS_AND_FAMILIARS.md`.*
