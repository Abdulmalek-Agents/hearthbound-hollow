# Codex 08 — **Choice & Consequence Atlas**
### Specialist: **Esme Cordray, Choice & Consequence Architect**
*(16 yrs · principal designer Mass Effect 3 ending arc (Bioware), narrative systems lead on Heaven's Vault and 80 Days (Inkle))*

> *"Cozy doesn't mean consequence-less. Cozy means the consequences are warm-blooded — they bleed into a relationship, not a Game Over. The architecture below is the village's nervous system. Touch one thing; somewhere else, something twitches. That twitch is the soul of this game."*

---

## 1.0 Mission

Define the **village state machine** — the data layer that tracks every consequence of every player choice and propagates the ripple back through every other system. This codex is the *spine* connecting Codex 02 (narrative), Codex 04 (progression), Codex 05 (conflict), Codex 09 (replayability), and Codex 10 (economy).

The atlas must answer four questions:

1. **What is tracked?** (The 14 consequence dimensions.)
2. **How are choices weighted?** (The choice tariff.)
3. **How do consequences propagate?** (The Ripple Engine.)
4. **What does the player see?** (Visible villager state changes.)

---

## 2.0 The 14 Consequence Dimensions

The village state machine tracks **14 dimensions** for every villager and the village as a whole. Each dimension has scale, decay rate, and visible expression.

| # | Dimension | Scale | Decay | Visible Expression |
|---|---|---|---|---|
| 1 | **Trust (per villager)** | 0–100 in 5 tiers | Decays −1/week if no interaction | Greeting tone; dialogue depth; willingness to sell deep memories |
| 2 | **Memory Integrity (per villager)** | 0–100 | Persistent | Whether they recognize their own past; tea preferences; voice signature |
| 3 | **Public Standing of the Hollow** | 0–100 | Slow drift | Walk-in frequency; merchant prices; town gossip |
| 4 | **Village Mood** | 0–100 | Seasonal cycles + event-driven | Background music; lighting; weather frequency tuning |
| 5 | **Faction Reputation** | 5 factions × 0–100 | Persistent | Faction-specific rewards & access |
| 6 | **Vow Integrity** | 7 vows × 0–100 | Slow | Codex glyphs; gameplay unlocks/locks |
| 7 | **Predecessor Awareness** | 0–100 across the village | Player-controlled | Whether villagers will *talk* about Marin |
| 8 | **The Forgotten Year State** | 0=sealed → 5=fully reconstructed | Player-controlled | Halia's behavior; Ren's awareness; schoolhouse access |
| 9 | **Letter-Bird Reach** | 0–100 | Grows with use | Async network depth |
| 10 | **Vance Pressure** | 0–100 | Episodic | Whether villagers consider his offers |
| 11 | **Thief Suspicion** | 0–100 | Mid-game arc | Investigation clues; villager paranoia |
| 12 | **Apprentice Bond (Ren)** | 0–100 | Daily | Ren's voice; his Vow inheritance |
| 13 | **Pickle Approval** | 0–100 | Persistent | Pickle's location; tasting depth |
| 14 | **The Hollow Itself** | 12-level scale (Codex 04) | Persistent | Physical visible shop state |

These 14 dimensions are stored as a single **VillageState** struct per save. Total memory cost: <2KB. The persistence model is trivial.

---

## 3.0 Choice Tariff — how player actions affect dimensions

Every choice the player makes carries a **tariff** — the delta it applies to one or more dimensions. The tariff is:

1. **Hand-authored for the 12 Sealed Villagers' decisions** (Codex 02 § 4).
2. **Procedurally derived for the 18 Procedural Villagers' decisions** (rules below).

### 3.1 Example tariff table — Doris's "Last Light" memory transaction

Player choice options:
- **Polish and return the memory clean** — preserves the full Last Light.
- **Cleanse the dementia from the memory** — removes the painful recognition; Doris will not remember Elric's confusion.
- **Cleanse aggressively** — removes Elric entirely from this memory (catastrophic; possible but possible only via player error).
- **Refuse the transaction** — return the memory unaltered, untraded.
- **Buy it without altering** — Doris loses Last Light entirely. The orb sits on your shelf.

The tariff per option:

| Option | Trust (Doris) | Memory Integrity (Doris) | Vow 1 | Vow 3 | Vow 4 | Public Standing | Pickle |
|---|---|---|---|---|---|---|---|
| Polish + return | +5 | preserves at 95 | +2 | +5 | 0 | +1 | +3 |
| Cleanse softly | +3 | drops to 70 | +1 | +3 | 0 | +1 | -1 |
| Cleanse aggressively | -15 | drops to 25 | -5 | -15 | 0 | -3 | -5 |
| Refuse | +1 (respect) | preserves at 100 | +5 | 0 | 0 | 0 | +1 |
| Buy (don't alter) | +6 (she's grateful) | drops to 0 (memory gone) | -2 | 0 | -3 | -2 | -2 |
| Buy + later return | +10 (Borrowed Memory) | preserves at 100 | +5 | +3 | +5 | +5 | +5 |

The tariff is **explicit but not shown to the player.** The player sees the *narrative* consequence ("Doris smiled at you for the first time in weeks"). The numbers are the layer underneath.

### 3.2 Procedural tariffs

For procedural villagers, tariffs are derived from:

```
tariff = base_table[transaction_type] 
       × villager_sensitivity_modifier[memory_weight]
       × moral_modifier[player_choice]
       × randomness_band(±15%)
```

The randomness band ensures no two procedural villagers respond identically — important for the cozy-game *people-feel-real* sense.

---

## 4.0 The Ripple Engine

The most novel system in this codex. When a tariff fires, the Ripple Engine propagates secondary effects across the village state machine according to **the Relationship Graph.**

### 4.1 The Relationship Graph

A directed weighted graph of who-affects-whom. Example fragment:

```
                    DORIS ──[wife of]──► ELRIC (deceased; off-graph except in memories)
                      │
                      ├──[friends-since-30-yrs]──► LAVINIA
                      │
                      ├──[mother-of]──► ELSA (returning; Revelation 17)
                      │
                      └──[employs]──► AEDAN (the goatherd helps with the bees)

                    LAVINIA ──[mentor-of]──► WREN
                       │
                       ├──[friends-since-50-yrs]──► OLD MARISKA
                       │
                       └──[taught-as-child]──► HALIA
                                                  │
                                                  ├──[mother-of]──► MIRA (Forgotten Year)
                                                  └──[mother-of]──► REN
```

There are 88 named edges in the canonical Relationship Graph (the full atlas is Appendix A in production).

### 4.2 Ripple propagation rules

When a tariff fires on Villager A:

1. **First-degree neighbors** receive ~30% of the tariff weighted by edge strength.
2. **Second-degree neighbors** receive ~10%.
3. **Faction-co-members** receive ~15%.
4. **Decay** rounds anything <2 to zero.
5. **Cap**: no propagated tariff exceeds ±5 on any single dimension.

### 4.3 Example — the Tribunal of the Widower (cont'd from Codex 05)

Player Cleanses Gerrold's memory aggressively. Tariff fires:

- Gerrold: Memory Integrity -50, Trust -20.
- Ripple to Halia (daughter): Trust -8, Memory Integrity 0 (she's not affected), Anger +15.
- Ripple to Sebastian (Mayor — friend of Gerrold): Trust -3.
- Ripple to Brother Anselm (faction "Civic"): Trust +1 (he believes consent prevails).
- Ripple to Pickle: Approval -4.

The player sees:
- Gerrold next visit is distant.
- **Halia walks past you in the village without speaking.** *Visible.*
- Sebastian's next greeting is a half-second shorter.
- Anselm offers you tea unprompted.
- Pickle has moved to the windowsill and will not come down.

Within 3 in-game days, **the player has felt the ripple in five distinct ways without a single numeric display.** This is the cozy game's emotional information density.

---

## 5.0 Visible State — How the player sees the village changing

### 5.1 The Villager Card

Each villager has a Card in the Codex with:

- A portrait that visibly changes with their Memory Integrity (fewer wrinkles = lost memories; the visual cue is "they look less like themselves").
- A trust tier glyph (a 5-petal flower; petals open with trust).
- A short prose snippet that updates: *"Doris speaks of Elric as if he is in the next room."* / *"Doris no longer speaks of Elric at all."*
- A *Last Memory Traded* line.
- A *Last Visit* timestamp.

The Card is the **information equivalent of a face**, not a stat sheet.

### 5.2 Village Mood — the global state

The village's collective mood manifests as:

- **Background music intensity** (Codex 14 § 4 — vertical mix lifts at high mood).
- **Weather frequency** (high mood → more golden days; low mood → more grey).
- **Walk-in frequency** to the Hollow (mood ↑ = more visitors).
- **Festival mood** at scheduled events (mood ↓ = sparse festivals).

Village mood is **never shown numerically.** It is felt.

### 5.3 The five visible village states

| State | Mood Range | Description |
|---|---|---|
| **Embershade Bright** | 80–100 | Lanterns lit early. Children sing in the square. The bell at the Three Apples rings on the hour. |
| **Embershade Familiar** | 50–79 | Default. The valley is itself. |
| **Embershade Quiet** | 30–49 | Less small talk. The market closes earlier. Brother Anselm preaches longer. |
| **Embershade Shuttered** | 10–29 | Doors close earlier. Pickle stays indoors. Idris does not visit this season. |
| **Embershade Distant** | 0–9 | (Rare.) Villagers' greetings are functional. The Hollow's bell does not ring. *This is a warning state.* |

The states are **never permanent.** The village can recover from Embershade Distant — but it takes weeks of warmth. This is the **slow-rope rule** of cozy choice systems.

---

## 6.0 The Five Factions

Beyond per-villager reputation, the village has **5 informal factions** that group like-minded residents. Faction reputation is tracked separately and unlocks specific opportunities.

| Faction | Members (named) | What they value | Reward at 70+ Trust |
|---|---|---|---|
| **The Civic Elders** | Mayor, Anselm, Lavinia, Reeve | Process, consent, tradition | The Mayor's blessing on Hollow Level 12 |
| **The Makers' Guild** | Caspar, Petra, Cael, Olwen, Pippa, Nora, Hodge | Craft, fair trade, honest coin | Access to high-quality Hollow upgrades at discount |
| **The Carers** | Doris, Halia, Yana, Wren, Nora, Sister Marrow, Lavinia | Witness, presence, gentleness | Composted memories yield rarer flora; Memory Sommelier path opens |
| **The Quiet Folk** | Mariska, Esher, Old Marin, Tully, the Boy Ren | Solitude, depth, lore, folklore | Mariska teaches Severing earlier |
| **The Visitors** | Idris, Vesna, traveling caravans, Letter-Bird senders | Outside, song, exchange | Letter-Bird network expanded; rare memory imports |

Note: villagers may belong to **multiple factions** (Doris is Carer + Maker-adjacent). The system handles this by weighted membership.

---

## 7.0 Endings — the Six Canonical Endings

The game has 6 hand-authored canonical endings, plus a 7th endless "stewardship" continuation. The ending is determined by the state at Bloomfast Festival of the player's chosen finale year.

| Ending | Trigger | Tone |
|---|---|---|
| **The Restored Keeper** | All 17 MAR fragments collected; Vow integrity >70 across all 7; predecessor reassembled | Bittersweet. Marin returns. The Hollow has a new chapter. |
| **The Dispersed Heir** | All 17 MAR fragments collected; player chooses to honor Marin's choice | The most emotionally complex. The player becomes the next dispersed Keeper, leaving Ren in charge. |
| **The Keeper's Hoard** | All 17 fragments collected; player refuses to reassemble; keeps them | The bittersweet-dark ending. The Hollow is rich in memory; the village is poorer. |
| **The Apprentice's Inheritance** | Apprentice bond > 90; player retires; Ren takes the Hollow | The succession ending. Cozy + hopeful. |
| **The Vance Victory** | Vance Departure: In Wealth | The shadow ending. The Hollow is now a partner of the Memory Stock Exchange. Pickle leaves. |
| **The Soft Light** | Vow integrity > 90; all Tribunals honored; no Predecessor fragments collected | The minimalist ending. The player did not chase the mystery. The village simply… was. Marin is honored in folklore. |
| **+ Endless Stewardship** | Any ending → continue | The player can keep playing past any ending. Many cozy players will. |

Each ending is **hand-authored**, ~3,000 words of prose, ~6 illustrated set-piece frames each. Total ending budget: ~18,000 words and ~36 illustration-frames. *Achievable inside the writing roadmap.*

### 7.1 Why six endings, not three or twenty

Three endings (good/neutral/bad) is too few — cozy players invest deeply in their save and want their save to *feel* unique. Twenty endings dilute hand-craft to the point of feeling random. Six is the cozy sweet spot validated by Spiritfarer (4), Citizen Sleeper (5), Pentiment (7), and Coffee Talk 2 (3). Six is also achievable on budget.

---

## 8.0 The Forgotten Year — the choice's largest set-piece

(See Codex 03 § 6 for the lore.) The choice the player makes about the Forgotten Year orb under the schoolhouse fires a **massive ripple**:

| Choice | Ripple |
|---|---|
| Open the orb | Halia: Trust drops; Memory Integrity surge; Anger surge. Ren: Memory Integrity surge. Village mood: brief drop, then long climb. Public Standing: +20. |
| Leave the orb | Halia: Trust +5. Ren: Memory Integrity unchanged (stays missing-something). Village mood: unchanged. Vow 2 (Return): -8. |
| Share as fragments | Halia: Trust +3. Ren: Memory Integrity rises partial. Village mood: slow climb. Public Standing: +10. *The most balanced choice.* |

The Forgotten Year is **the game's emotional peak.** It is also the choice with the largest ripple-radius (it touches 19 of the 30 villagers).

---

## 9.0 Choice Visibility — the Cordray Principle

> **Rule:** the player must always be able to *infer* the consequence shape of a choice before they make it. They must never *know the numbers.*

Concretely:

- Before a major choice, a single sentence appears with the *kind* of consequence ("This will affect Doris and those she loves.").
- After a choice, the consequence plays out in **scene**, not in stat-screen.
- The Codex retrospectively shows the player a *narrative summary* of what changed, not a list.
- The player's save has a "What Have I Done?" entry: a hand-curated paragraph per major choice, generated from a 300-template library.

This is the **anti-spreadsheet rule.** Cozy players are not optimizing. They are *experiencing.*

---

## 10.0 The Ledger UI

The player's in-game Ledger — a leather-bound book — shows:

- **The day's transactions** (narrative summary).
- **The week's events** (a list of beats with one-line each).
- **Open story threads** (a curated, fiction-flavored quest log: *"You promised Caspar you'd return his wheel by Greenmouth."*)
- **Villager portraits with current state lines** (see § 5.1).
- **The Echo Web** (the connection map between memories — Codex 02 § 9).
- **The Hollow's State** (Hollow level, garden state, Pickle's mood).
- **The 7 Vow glyphs** (corner of every page).

The Ledger is the **player's only HUD.** Outside the Ledger, the screen is the world.

---

## 11.0 The Memory Lawsuit — choice exemplar

The Lawsuit (Codex 05 § 7) is the choice-design exemplar. Each day's choice has:

- **A binary surface (comply / refuse) plus 4 nuanced options.**
- **A 4-line consequence preview written in-fiction** (no numbers).
- **Pickle's reaction** (humor codex).
- **The Magistrate's satisfaction meter** (visible — exception to the no-numbers rule because the Magistrate is *bureaucracy made flesh*).
- **A 30-second post-choice scene that shows the *immediate* result.**
- **A 3-day cooldown before the ripple becomes visible.**

This pacing — *immediate scene + delayed ripple* — is the cozy game's heart-rate manager. It avoids the "consequences land all at once" pile-up that Mass Effect 3 was criticized for.

---

## 12.0 Player Tracking & Telemetry

The studio's analytics must track:

- **Choice distribution per major decision** (to detect under-used branches).
- **Tariff-per-choice averages** (to detect imbalance).
- **Time-to-recovery** for villager trust after major drops.
- **Endings reached** (proportion).
- **Vow-integrity end-state distribution.**
- **Most-replayed choices** (in NG+).

This data informs post-launch tuning. *No data is sold or shared with any third party. Cozy audience expectation.*

---

## 13.0 The Anti-Compass Mode

Some players want to **not know** any consequences in advance. They want pure mystery.

**Anti-Compass Mode** (opt-in) removes the pre-choice consequence sentence and the Pickle warning. The player makes choices blind. The Ledger only shows past consequences, never future hints.

This mode is requested by ~22% of cozy-narrative players (sampled). It is supported. It is **not** the default.

---

## 14.0 Closing

> *"The cozy game is a place where doing the right thing matters and doing the wrong thing also matters and there is never a game-over screen to make either feel cheap. The consequence engine is the village's pulse. Touch it gently. Hear how it answers."*
>
> — *Esme Cordray*

— *End of Codex 08. Next: `09_ROGUELITE_REPLAYABILITY.md`.*
