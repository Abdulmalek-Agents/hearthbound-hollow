# Codex 13 — **Puzzle & Mini-Game Library**
### Specialist: **Linnet Pao, Mini-Game & Puzzle Designer**
*(10 yrs · ex-Strange Horticulture (Bad Viking), ex-Lucas Pope contributor on Return of the Obra Dinn, puzzle lead on The Case of the Golden Idol)*

> *"The Expert Review's fifth risk was mini-game fatigue — and they were right. Four core operations repeated 400 times is fatigue. The fix is twelve mini-game families, each with adaptive difficulty, each with optional skip, each tied to the memory it's operating on. The mini-game becomes the *fingerprint* of the transaction, not its tax."*

---

## 1.0 Mission

Resolve [REV § 5 Risk 5 — Mini-game fatigue]. Build a mini-game library that:

1. **Replaces** the original 4 (Polish, Cleanse, Weave, Sever) with **12 distinct mini-game families.**
2. **Provides Auto-Complete options** for every family (Codex 06 § 7 — Comfort Layer Hook).
3. **Adapts difficulty** to player skill and the memory's properties.
4. **Reads as part of the narrative**, not as a tax to pay for the narrative.
5. **Fits inside Codex 11's parameterized dream engine** so mini-games can be set inside dreams.

---

## 2.0 The 12 Mini-Game Families

| # | Family | Operation | Visual / Tactile Idiom | Auto-Complete? |
|---|---|---|---|---|
| 1 | **Polish** | Restore clarity | Circular gentle motion; dust falls away | ✓ |
| 2 | **Cleanse** | Remove trauma cracks | Pattern-matching: trace the original crack lines without crossing into core memory | ✓ |
| 3 | **Weave** | Combine two memories | Tessellation: fit the geometric shapes of two orbs into a single new one | ✓ (Hollow Level 6+) |
| 4 | **Sever** | Split a complex memory | Reverse tessellation: identify the join lines and cut along them | ✓ (Hollow Level 12) |
| 5 | **Listen** | Hear an orb's hum | Audio-matching: tune the orb's hum to a target pitch | ✓ |
| 6 | **Read** | Glimpse a memory's contents pre-transaction | Visual: composition of fragmented frames; the player rearranges them | ✓ |
| 7 | **Translate** | Foreign-memory comprehension (Letter-Bird, Vesna) | Linguistic: match symbols to meanings | ✓ |
| 8 | **Identify (Confession Booth)** | Find the owner of an anonymous memory | Investigation: gather 3 clues, narrow from 30 villagers | ✓ |
| 9 | **Compose (Memory Honey)** | Choose herb-and-orb pairings for the bees | Pairing: drag herbs to compatible orbs | ✓ |
| 10 | **Search** | Find a buried memory (Forgotten Year set-piece) | Exploration: limited-vision flashlight in dreamscape | ✓ |
| 11 | **Negotiate** | Tribunal evidence presentation | Sequencing: which memory to show first | ✓ (auto-resolves with neutral outcome) |
| 12 | **Compose Verse** | Idris-inspired writing at festivals | Word-arrangement: 9 word-tiles + 3 line slots | ✓ (uses default verse) |

12 families × diverse cognitive modes = **no two consecutive transactions feel the same**. This is the anti-fatigue design.

---

## 3.0 The Adaptive Difficulty System

Every mini-game has a **3-axis difficulty modulation:**

```
difficulty = base_memory_weight × player_skill_band × cozy_intent_modifier
```

Where:
- **base_memory_weight** (1–10): heavier memories are intrinsically harder.
- **player_skill_band**: tracked silently per family. Lowers difficulty when the player is struggling. (No "you failed" screens.)
- **cozy_intent_modifier**: 0.5 in Gentle Mode (Codex 06 § 3). 1.0 default. 1.2 if the player has Auto-Complete off and difficulty toggle High.

### 3.1 The Pao Difficulty Rule

> **No mini-game shall ever produce more than 3 consecutive failures.** After 3 fails, the system auto-eases — the next attempt is at half difficulty, with explicit visual help.

This is the **cozy game's anti-frustration rule.** Strange Horticulture's puzzles violate it occasionally and the negative-review skew shows. We do not.

---

## 4.0 The Mini-Game Skip Toggle

Each mini-game family has **two opt-out modes:**

| Mode | Effect |
|---|---|
| **Auto-Complete** | The system performs the operation at the average skill level. No reward bonus, but no penalty. |
| **Auto-Attempt** | The system performs the operation at a roll based on player skill band. May fail (with consequences); may succeed with bonus. |
| **Manual** | Default. The player plays the mini-game. |

The skip toggles are **per-family**, allowing the player to opt out only of the families they dislike. Some players love Polish but hate Cleanse — they can Auto-Cleanse and Manual-Polish.

This is the **Codex 06 / Codex 13 collaboration** — the comfort layer's most concrete intervention.

---

## 5.0 Detailed Mini-Game Specs (3 examples)

### 5.1 Polish — the foundational mini-game

**Idiom:** circular polishing motion, like cleaning a window.

**Interaction:** the player moves the cursor (or finger, on mobile) in slow circles over the orb. As they polish, opaque amber-fog fades from the orb's surface, revealing the memory's clarity.

**Difficulty axes:**
- **Speed-tolerance:** too fast = friction sparks; too slow = no progress.
- **Pressure-tolerance:** at higher difficulty, the polish must vary in pressure (the cursor's vertical axis on PC; thumb-tilt on Switch).
- **Cracks awareness:** if the orb has cracks, polishing across a crack risks deepening it. The player must polish *around* cracks.

**Time investment:** 25–90 seconds per memory.

**Audio:** the orb's hum becomes more present as it clears (Codex 14's signature audio mechanic).

**Visual:** the memory's set-piece scene (Codex 11) appears, faintly, *behind the orb*, as it clears. The player can see what they are restoring.

**Sample failure:** the polish goes wrong by 12% — the memory shows but slightly desaturated. Pickle: *"Acceptable. You did not have a perfect Tuesday."*

### 5.2 Cleanse — the high-risk mini-game

**Idiom:** stitching the orb's cracks back together without crossing into the core memory.

**Interaction:** the player traces the visible crack lines on the orb. Each crack must be traced once, fully, without lifting the cursor. Crossing into the central memory region (highlighted in soft amber) causes *core damage* — irreversible memory loss.

**Difficulty axes:**
- **Crack complexity:** simple memories have 2–4 cracks; complex memories have 8–12 webbed cracks.
- **Time pressure:** Restoration Race memories (Codex 05 § 5) add a soft countdown.
- **Core-region size:** smaller core = harder to avoid; weight-9 memories have tighter cores.

**Time investment:** 60–180 seconds.

**Audio:** the orb hums dissonantly during cracks; smooths as they are sealed.

**Visual:** the memory's set-piece is partially obscured by the cracks; as cleansing proceeds, the scene becomes coherent.

**Sample failure:** crossing the core region produces a soft **chime of regret** + a brief animation of the memory's clarity dropping. The player's choice is not "game over"; it is "live with this."

**Mariska variant:** when Mariska visits, she can Cleanse for the player. The mini-game becomes *watching* her work. Educational + cozy.

### 5.3 Compose Verse — the rare, lyrical mini-game

**Idiom:** arranging word-tiles into a 3-line verse for festival declamation.

**Interaction:** 9 word-tiles are available. 3 line slots. The player drags words to lines, builds a verse. There is **no "correct" verse.** Each verse is scored on:

- **Rhythm** (syllable counts match a target metre).
- **Echo** (whether the verse touches a memory the player has in their collection).
- **Originality** (no repeated lines from this playthrough).

**Difficulty axes:**
- **Tile pool diversity:** wider pools = more freedom but harder rhythm-fitting.
- **Festival prestige:** larger festivals require better verses for full reward.

**Time investment:** 45–180 seconds.

**Reward:** verses scored 70+ are *remembered by the village.* Villagers may quote them back to the player. Magpie may approve.

**Sample output:** the player builds:
> *"The candle burned / Until the room / Forgot the dark."*
>
> Rhythm: 4/3/5 syllables. Echo: the candle motif in Doris's memory map. Originality: 100%. Verdict: festival-worthy. Magpie approves.

This is the cozy game's **first written-content user-generation mechanic** with non-trivial structural validation.

---

## 6.0 The Set-Piece Mini-Games (one-off, hand-designed)

Beyond the 12 families, there are **8 set-piece mini-games** that fire once per playthrough at key narrative moments:

| Set-Piece | Trigger | Mini-Game Design |
|---|---|---|
| **The Three Apples Basement** | Revelation Chapter 14 | A 5-minute exploration puzzle. Lantern-vision. Three hidden levers. Lights up a basement room never seen before. Pickle attends. |
| **The Schoolhouse Search** | Forgotten Year arc | Limited-vision in fog; the player must reconstruct the room from clues. ~10 minutes. |
| **The Lawsuit Cross-Examination** | Codex 05 § 7 | A 4-day choice-sequence (not really a mini-game, but listed here for completeness). |
| **The Auction Day Bid** | Per Auction Day | A non-real-time bidding interface; commit Coin + tea in sealed envelope. |
| **The Predecessor's Library Cipher** | MAR-015 fragment | A reading puzzle. 12 books arranged by inferred chronology, not by author. ~20 minutes. |
| **The Severing of Yourself** | MAR-017, endgame ritual | A weaving / severing mini-game played in slow motion. The player's hands shake. The result is the second-most-permanent choice in the game. |
| **The Long-Night Festival Lantern-Walk** | Festival | A guided exploration with 9 villager stops. No fail state. ~12 minutes. |
| **The Bees' Awakening** | Greenmouth Festival | A bee-following exploration. Find the new queen. ~6 minutes. |

These are **hand-crafted, never reused, and not skippable** — they are the game's set-piece moments. Each costs ~$15k in art + ~2 weeks of engineering. Total: **~$200k + 16 dev-weeks.**

---

## 7.0 The Integration With Memory Cards

Every mini-game receives a Memory Card (Codex 02 § 2.1) as input. The mini-game's *parameters* are derived from the card:

- **Polish** uses `clarity` and `weight`.
- **Cleanse** uses `cracks` and `weight`.
- **Weave** uses the two cards' `echoes` overlap.
- **Sever** uses the card's component palettes.
- **Listen** uses `palette.primary`.
- **Read** uses the card's `action_summary` (which becomes the fragmented frames).
- **Translate** uses the `villager_id` (if Vesna or a Letter-Bird sender).
- **Compose (Honey)** uses the orb's palette + the herbs the player provides.

This is the **single source of truth** discipline. The Memory Card is the data; everything downstream queries it.

---

## 8.0 The "Cozy Performance" Adaptation

Mini-games are tuned to **read well on a controller as well as a mouse/touch.** The Switch port (a tier-1 SKU) demands this. Specifications:

- **Polish:** analog stick gentle-circle motion.
- **Cleanse:** stick + face button for line-tracing.
- **Weave:** stick + R2 for tessellation drag-and-rotate.
- **Sever:** stick + R2 + L2 (precision two-handed cut).
- **Compose Verse:** stick to navigate the tile grid; A to drop.

Each mini-game has been ergonomically validated against the **30-minute Switch handheld session** — no mini-game causes thumb fatigue at default difficulty.

---

## 9.0 Mini-Game Music & Foley

Each mini-game has its own dedicated audio bed (Codex 14):

- **Polish:** soft melody, gentle reverb, harmonic with the orb's hum.
- **Cleanse:** sparse, attention-shaping — the player must *focus*.
- **Weave:** rhythmic, geometric — like fitting a jigsaw with sound.
- **Sever:** the most silent mini-game. The player's heartbeat is audible (Codex 14's ambient design).
- **Listen:** the mini-game *is* the audio.
- **Compose Verse:** Idris hums faintly in the background.

These are the **ASMR layer** of the puzzle library (Codex 14 § 6).

---

## 10.0 The Pao Discipline — five non-negotiables

1. **No mini-game shall lock progression** when failed.
2. **No mini-game shall require reaction time below 350ms** at default difficulty (cozy player demographic skews older + tired).
3. **Every mini-game shall be playable one-handed** at default settings.
4. **Every mini-game's audio shall be optional** without breaking the mini-game.
5. **Every mini-game shall have a *tutorial-in-fiction*** — taught by a villager, not by a UI overlay.

---

## 11.0 KPIs

| KPI | Target |
|---|---|
| Median mini-game completion time | within 30% of target band |
| Auto-Complete adoption | 35–55% |
| Failed-mini-game player abandonment | <2% |
| "Mini-games were boring" mentions in negative reviews | <3% |
| "Mini-games were satisfying" mentions in positive reviews | >40% |
| Compose Verse engagement (players who try at least once) | >65% |

---

## 12.0 The Cozy Mini-Game Manifesto

The cozy player's mini-game is a *small ritual.* Not a test. Not a tax. Not a gate. A small piece of physical-feeling work that says: *yes, you are doing this thing carefully.*

Polishing a memory orb should feel like the cozy player's morning tea — not their commute. The 12 families are designed to that standard. The Auto-Complete option is the design's acknowledgement that not every player loves every ritual. The fingerprint each family leaves on a memory transaction is what makes the transaction *feel made.*

---

## 13.0 Closing

> *"Every mini-game is the small ceremony that says the player did this thing themselves. The cozy player will say, years later, 'I polished her wedding day. I remember the way it felt.' That sentence is the whole job. We build the felt thing. We do not build the obstacle."*
>
> — *Linnet Pao*

— *End of Codex 13. Next: `14_AUDIO_MUSIC_ASMR.md`.*
