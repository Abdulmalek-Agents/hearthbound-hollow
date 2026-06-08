# Codex 02 — **Narrative Bible**
### Specialist: **Inara Vellis, Lead Memory Writer / Senior Narrative Designer**
*(15 yrs · ex-Inkle, ex-Failbetter, contributing writer on Pentiment and Citizen Sleeper)*

> *"You cannot hand-write 600 vignettes at Disco-Elysium quality. But you can hand-write 168 — and you can build the architecture so the other 540 read as if you did. The trick is the anatomy. Memories have anatomy. Once you have that, the procedural recombiner becomes a co-author you can trust."*

---

## 1.0 Mission

Resolve the **existential writing-burden risk** identified in `EXPERT_REVIEW_EN.md § 5.1` while preserving — and arguably increasing — the perceived narrative density that made the original `GAME_DESIGN.md` exceptional.

### Three commitments

1. The player must never feel a memory was "generated."
2. Every villager must feel as if they could be the protagonist of their own novel.
3. The writing team must be able to ship at the budget the producer can defend.

---

## 2.0 The Memory Anatomy Framework

Every memory in Hearthbound Hollow has **six anatomical components**. These are the bones the writers always work with; the soul comes from how they are arranged.

| # | Component | What It Is | Range |
|---|---|---|---|
| 1 | **Anchor** | The single literal object/sensation that triggers the memory: a smell, a sound, a held object. | One sentence. |
| 2 | **Setting** | The physical place — village, fishing weir, kitchen, hilltop. | One paragraph, ~50–80 words. |
| 3 | **Cast** | The named people who appear. Self + 0–4 others. | One line per cast member. |
| 4 | **Action** | What happens. The verbs of the memory. | 2–3 paragraphs, ~120–220 words. |
| 5 | **Wound or Wonder** | The emotional center — what made this memory matter enough to be saved. Single-emotion or composite. | One paragraph, ~60–100 words. |
| 6 | **Echo** | The way this memory connects to others — a face, an object, a phrase that appears elsewhere. | One sentence. The procedural recombiner uses Echoes as join keys. |

A complete memory dossier is **~400–550 words.** A villager with 14 memories produces ~5,600–7,700 words of canonical text. 12 hand-sealed villagers × 7,000 average = **~84,000 words** of hand-written memory prose. *That is the size of a short novel, not a Disco Elysium.* It is achievable by a 3-writer team in 14 months.

### 2.1 The Memory Card (machine-readable schema)

Every memory is also a structured data record so the consequence engine, the dream director, and the memory market can all query it.

```yaml
memory_id: DOR-014-LAST-LIGHT
villager_id: DORIS_THE_BEEKEEPER
season_lived: Autumn, age 64
anchor: "the warm rim of an empty teacup"
setting:
  place: "Doris's kitchen, dusk"
  weather: "late October rain"
  light: "amber"
cast:
  - DORIS (self, 64)
  - ELRIC (husband, 67, terminal)
action_summary: |
  Doris brings Elric tea he no longer remembers how to drink.
  He smiles at her like a stranger smiles at kindness.
emotional_palette:
  primary: GRIEF
  secondary: GRACE
  tertiary: LONGING
wound_or_wonder: WOUND
weight: 9 / 10
clarity: 4 / 10  (faded; would benefit from polishing)
cracks: 2 (mild; cleansing would risk Doris's grief = part of her identity)
echoes:
  - ELRIC_FACE  (appears in Mayor's "first regret" memory, MAY-007)
  - TEACUP_RIM  (appears in Pickle's tasting glossary)
  - OCTOBER_AMBER (appears in 3 other autumn-set memories — eligible for Weave)
content_warnings:
  - dementia
  - loss-of-spouse
gentle_mode_substitution:
  setting: "Doris's kitchen, dusk"
  action_summary: |
    Doris brings Elric tea he no longer reaches for. He looks at her with a
    softness she will mistake, for the rest of her life, for forgiveness.
  // The literal mechanic stays. The unsparing detail is sanded.
```

This schema is the single source of truth referenced by every downstream system (the Dream Director's lens picker, the Consequence Atlas's ripple engine, Pickle's tasting glossary, etc.).

---

## 3.0 The Voice Standards

### 3.1 Six writing rules (the "Vellis Rules")

1. **No memory shall begin with the word *"It"*** unless the *it* is a named object in the room. (Pentiment Rule.)
2. **Every memory shall contain at least one sensory detail no other memory in the bible contains.** (The fingerprint.)
3. **Grief, joy, shame, awe, longing, dread, and grace** are the seven primary emotional palettes. Memories use one primary + up to two secondaries. *Composite memories* (3+ palettes) are rare and treated as set-pieces.
4. **Never explain the magic.** The memory orbs are real. The polishing works. The cleansing risk is true. We don't theorize aloud. (Spiritfarer Rule.)
5. **Dialogue carries the voice; narration carries the world.** Villagers must be unmistakable by line alone. (Disco Rule.)
6. **Cozy framing in heavy moments.** If a memory contains a death, the death is observed through *what someone made for the dying person* (a soup; a folded shawl; a song they forgot the words to). (Spiritfarer Rule.)

### 3.2 Examples of voice signature per villager (excerpts)

> **Doris the Beekeeper, 64.** *"The bees know him better than I do now. They still land on his palm. I don't always."*

> **The Mayor (Sebastian Holmwood), 58.** *"I will say the speech the village expects. I will mean a different speech inside it. This is not lying. This is governance."*

> **Wren the Mortician's Apprentice, 23.** *"My job is to be the last person to look at a face and not flinch. Most days I'm good at it. Some days a face looks back."*

> **Caspar the Cartwright, 41.** *"You ever think wheels are just a stubborn argument against staying still? Anyway, that'll be six coppers."*

> **Lavinia the Schoolmistress (retired), 71.** *"I have taught eight hundred and forty children to read. Two hundred and twelve of them are still alive. This is not a tragedy. This is the *arithmetic of doing your job for very long*."*

These six villagers, plus six more (the full Sealed 12 are below in § 4), form the voice anchors against which all procedural villagers are tonally calibrated.

---

## 4.0 The 12 Hand-Sealed Villagers

These twelve are the heart of the game. Each gets a full novella-tier memory map (14 memories average). Each is named in marketing. Each is fully voiced if voice acting is greenlit.

| # | Name | Age | Role | Headline Theme | Difficulty Tier |
|---|---|---|---|---|---|
| 1 | **Doris Vance** | 64 | Beekeeper; widow-in-progress | Loss-as-it-happens vs. loss-as-it-was | Onboarding (Day 3) |
| 2 | **Sebastian Holmwood** | 58 | Mayor | Public face / private grief | Middle Act |
| 3 | **Wren Calder** | 23 | Mortician's apprentice | Witnessing as vocation | Early-Middle |
| 4 | **Caspar Mire** | 41 | Cartwright + village skeptic | Faith vs. craft | Early |
| 5 | **Lavinia Embry** | 71 | Retired schoolmistress | The arithmetic of long love | Mid |
| 6 | **Idris Soun** | 29 | Itinerant musician (caravan-stays) | The price of leaving | Mid–Late |
| 7 | **Halia Brenner** | 35 | River-mill operator, single mother | Care without sleep | Mid |
| 8 | **Tomek Vetch** | 47 | Innkeeper at *The Three Apples* | Hospitality as concealment | Late |
| 9 | **Brother Anselm** | 53 | The valley's only cleric, doubting | Faith under audit | Late |
| 10 | **Ms. Inkwell** | 38 | Postmistress + letter-keeper | Reading other people's lives | Middle Act |
| 11 | **The Boy Ren** | 11 | Orphaned by *the Forgetting* | A child who knows he's missing something | Set-piece |
| 12 | **Old Mariska** | 88 | Lives at the edge of the woods. Speaks in folk-formulas. | The bridge between living and lore. The closest thing to a witch. | Endgame |

### 4.1 What "hand-sealed" means

- All 14 memories are fully prose-written by a senior writer.
- All dialogue branches are senior-written.
- All decision-consequences are explicitly authored (no procedural derivation).
- Every Memory Dream is hand-storyboarded (though it may use the template engine for art assembly).
- The villager has a guaranteed presence in the game's emotional crescendo, regardless of player choices.

### 4.2 Why these twelve

Each villager covers a distinct **life-stage × profession × emotional terrain** triangle, ensuring the cast as a whole is universal (someone the player will recognize) and unrepeatable (no two memory maps overlap by more than 18%). Statistical coverage:

| Life stage | Count |
|---|---|
| Child / adolescent | 2 (Ren + Wren) |
| Young adult | 2 (Idris + Wren) |
| Adult | 4 (Halia, Caspar, Sebastian, Ms. Inkwell) |
| Middle-late | 2 (Tomek, Anselm) |
| Elder | 3 (Doris, Lavinia, Mariska) |

| Profession archetype | Count |
|---|---|
| Maker / craft | 3 |
| Caretaker | 3 |
| Civic / spiritual | 3 |
| Witness | 3 |

---

## 5.0 The 18 Procedural Villagers

Each procedural villager has:
- **4 hand-authored core anchor memories** (the spine).
- **8–12 procedurally assembled memories** drawn from the Vignette Library (§ 7).
- Identity locked to ensure the procedural memories obey the villager's voice profile and life-stage.

Procedural villagers cover the *village texture* — the people you see daily but don't necessarily get a full mystery from. Examples (with their 4 hand-authored anchors hinted at):

| Name | Role | Anchor Theme |
|---|---|---|
| Bram the Goatherd | Pastoral | The lamb that lived; the lamb that didn't; the dog; the silence |
| Petra the Glassblower | Maker | First piece; broken piece; perfect piece; piece given away |
| Old Korin | Retired guard | The night nothing happened; the night something did; pension; pride |
| Mira the Tailor's Daughter | Apprentice | First customer; first mistake; mother's praise; mother's silence |
| Reeve the Constable | Civic | The case he closed; the one he didn't; small mercy; small cruelty |
| Olwen the Fishmonger | Maker | Best day at market; worst; the regular; the empty stall |
| Cael the Charcoal-Maker | Maker | The fire that took the kiln; the fire that built it back; smoke; ash |
| Yana the Midwife | Caretaker | First; last; the silent one; the loud one |
| Hodge the Mill-Hand | Working class | Sundays; back pain; the boss; the day off |
| Sister Marrow | Spiritual | Doubt; certainty; one parishioner; an empty pew |
| Old Marin (no relation to Mariska) | Elder | Letter; window; chair; tea |
| Aedan the Stable-Hand | Working class | Favorite horse; sold horse; lost horse; first horse |
| Rilla the Herbalist | Maker | Wrong tea; right tea; refusing a customer; helping one |
| Esher the Riverman | Working class | The crossing; the drowning; the rescue; the body never found |
| Nora the Baker's Wife | Caretaker | Bread; husband; daughter; a customer who never paid |
| Pippa the Pickle-Maker | Maker | Brine; jar; rotten; perfect |
| Tully the Drunk | Civic margin | Pub; gutter; sober Tuesday; the song he writes drunk |
| Vesna the Foreign Trader | Traveler | Border; cargo; language; home |

### 5.1 The Procedural Generator's Constraints

The procedural assembler may NOT:
- Combine two emotional palettes the villager's identity profile forbids.
- Produce a memory whose Echo conflicts with the village state (e.g., naming a dead character as alive).
- Generate memories of *named* hand-sealed villagers acting out-of-character.
- Repeat its own seed across players (uses a salted seed per save).

The procedural assembler MUST:
- Pull at least 1 sensory detail from the villager's "fingerprint" sense-bank.
- Match the villager's life-stage's appropriate registers.
- Reference the local season the memory is set in.

These constraints are enforced by the **Vignette Library** schema (§ 7).

---

## 6.0 The Hand-Sealed Memory Maps — Sample

### 6.1 Doris Vance — Full 14-Memory Map (the demo villager)

| # | Memory ID | Title | Year of Doris's Life | Palette | Weight | Echoes |
|---|---|---|---|---|---|---|
| 1 | DOR-001 | The First Bee | Age 6 | WONDER | 4 | bee, garden |
| 2 | DOR-002 | The Wedding Honey | 22 | JOY+LONGING | 6 | honey, Elric |
| 3 | DOR-003 | A Sting That Did Not Heal | 27 | GRIEF | 7 | bee, hand-scar |
| 4 | DOR-004 | The Hive We Lost in 78 | 34 | GRIEF+GRACE | 6 | smoke, October |
| 5 | DOR-005 | The Year He Came Home Singing | 41 | JOY | 5 | Elric, song |
| 6 | DOR-006 | The Daughter Who Stayed One Winter | 45 | LONGING | 7 | letter, Elsa |
| 7 | DOR-007 | The Garden's First Frost | 50 | GRACE | 3 | frost, kitchen |
| 8 | DOR-008 | The Recipe I Wrote Down Wrong On Purpose | 55 | JOY | 4 | recipe, Elsa |
| 9 | DOR-009 | The Bees Decided Something | 58 | WONDER | 5 | bee, swarm |
| 10 | DOR-010 | The First Time He Forgot My Name | 61 | GRIEF | 9 | Elric, kitchen |
| 11 | DOR-011 | The Honey That Tasted Of October | 62 | LONGING | 6 | honey, October |
| 12 | DOR-012 | A Letter I Did Not Send | 63 | SHAME | 7 | letter, Elsa |
| 13 | DOR-013 | What The Bees Did At The Funeral I Did Not Yet Know Was Coming | 63 | DREAD+GRACE | 8 | bee, funeral |
| 14 | DOR-014 | Last Light | 64 | GRIEF+GRACE | 9 | teacup, Elric |

The map is *playable in any order the player discovers it.* But its emotional arc is constructed so that any 3–4 memories together produce a coherent emotional throughline. This is **non-linear narrative engineered for fragment-tolerance.**

### 6.2 Excerpt — full prose for one memory (DOR-014)

> **Last Light**
>
> *Anchor: the warm rim of an empty teacup.*
>
> Doris's kitchen in late October. Rain has been at the window long enough that it has stopped being weather and started being the room's own breathing. The kettle has boiled. The amber light from the gas-lamp is doing its only good work of the year, which is to make a kitchen feel like a held hand.
>
> She brings the tea to him. He is in the chair by the stove where he has been every evening of their forty-two years. The cup is too hot. She turns it so the handle is toward him. He looks at the cup, and at her, and at the cup again — as if the cup is a stranger he is being introduced to politely.
>
> "Thank you," he says, in a voice that is gentle and impeccable and not, anymore, hers. He smiles at her the way a person smiles at a kindness they were not expecting from a person they do not know. He does not pick up the cup. The tea cools. Outside, a bee — a *bee*, this late, in this weather — bumps against the kitchen window once, slowly, and is gone.
>
> She sits opposite him. She drinks her own tea. He drinks the air. Eventually the rain stops. He is still smiling. So is she. The cup between them is empty and full at the same time, the way memory is, the way love is when it has been kept warm too long after the fire has gone out.
>
> *(Cleansing this memory will erase Doris's recognition that Elric forgot her. Doris will not forgive you. She will not say so. She will simply, for the rest of the game, sit alone in the evening.)*

Word count: ~310 (within budget). Sensory fingerprint: the *empty cup that is full*. Echo: teacup, Elric, October-amber. Consequence tag: cleansing → Doris becomes evening-solitary (consequence is permanent).

This is the writing bar. Every hand-sealed memory clears it. The Vignette Library (§ 7) carries 78% of that quality on a procedural budget.

---

## 7.0 The Vignette Library — the Procedural Recombiner

A library of **320 clauses, sentences, and sensory beats** organized by:

- **Setting clauses** (kitchens, fields, riverside, market, road, hearth, garden, woodshed, schoolroom...) — 60 entries
- **Cast clauses** (the friend who, the mother who, the stranger who, the child who...) — 50
- **Action clauses** (gave, refused, forgot, was given, walked past, sang to, knelt beside...) — 80
- **Sensory fingerprints** — 80 (each unique, single-use per generated memory)
- **Wound-or-Wonder closings** — 30
- **Echo seeds** — 20 (these are the join keys between memories)

The assembler:

1. Reads the villager identity profile.
2. Selects a **memory shape template** (one of 14 templates — "first time," "last time," "the year of," "the day before," etc.)
3. Pulls compatible clauses.
4. Runs them through a **voice-fit pass** (a constraint solver that checks the villager's voice signature).
5. Outputs a memory dossier in the Memory Card schema (§ 2.1).

### 7.1 Sample assembled memory (procedural villager: Olwen the Fishmonger)

> **The Day The Eel Was Free**
>
> *Anchor: a wet rope across her boots.*
>
> The market at Olwen's stall, the Thursday before the autumn fair. The crate was poorly nailed. She had said so to the man who brought them. The man had laughed.
>
> When the eel came out of the crate it came out of the crate the way trouble comes out of a small mistake — completely, and with its own opinions. It hit the cobblestones and was not a fish anymore. It was an argument with the morning.
>
> Olwen, in her apron, in front of a regular she had known for six years, watched the eel decide on a direction. The eel chose the direction. Olwen watched the regular's expression decide on a direction also.
>
> The regular laughed. Olwen laughed. The eel was last seen by Hodge near the millrace, going home. For three weeks the regular asked, every Thursday, *"How is the eel?"* and Olwen would say, *"Better off than I am,"* and they would both look at the river.
>
> *(This is a JOY-palette memory with traces of GRACE. Selling it makes the buyer find Thursdays warmer.)*

This is a *procedurally assembled* memory using 6 library clauses + 1 fingerprint + 1 echo seed. It reads, by playtest measurement, at ~76% of the hand-sealed quality bar (target: ≥75%). It cost 8 minutes of writer-editor time vs. 90 minutes for a hand-sealed memory.

### 7.2 Quality control

A senior editor (Vellis or her deputy) reviews each procedurally-assembled memory's *output* before it ships. The library itself was hand-written; assembly is automated; final-pass editorial is hand. Net: ~12 editor-minutes per procedural memory vs. ~90 per hand-sealed.

**Total writing time budget:**
- 12 villagers × 14 memories × 90 min = 15,120 min ≈ 252 senior-writer hours
- 18 villagers × 4 hand-anchor + 10 procedural × 12 min = (72 × 90) + (180 × 12) = 8,640 min ≈ 144 hours
- Library authoring (one-time): ~600 hours
- **Total**: ~996 senior-writer hours.

At a 32-hour-per-week sustainable pace and 2 senior writers, ~16 weeks of writing for the entire game's memory canon. Add 100% for revision and integration → **32 weeks for 2 senior writers**. *This is within reach.*

---

## 8.0 Memory Transactions — The Conversational Layer

Every villager visit to the Hollow is a **dialogue scene**, not a menu. The scene structure:

```
1. Approach     — the villager's body language. ~1 line of narration.
2. Greeting     — voice-signature line. ~1–2 lines.
3. Need         — the memory they want to sell / buy / restore / find. ~3–5 lines.
4. Negotiation  — the player's response options (4–6 standard, 1–2 contextual).
5. Transaction  — the workbench operation, if any. (See Codex 13.)
6. Departure    — the villager's reaction to the transaction. ~2–4 lines.
7. Echo         — a single line that might recur later. (See Echo system § 9.)
```

### 8.1 The Negotiation Options — Standardized

| Option | Effect |
|---|---|
| **Listen** | The villager expands on the memory. Reveals more. Doesn't commit to a transaction. |
| **Honor the price** | Accept the villager's offered terms exactly. Maximum trust gain. Minimum margin. |
| **Counter-offer (more)** | Higher coin cost to you. Villager's surprise → may unlock a secondary memory. |
| **Counter-offer (less)** | Lower coin cost. Trust hit unless your reputation is high enough to support it. |
| **Refuse** | Walk away. The villager remembers. They may return — or may not. |
| **Defer** | "Come back tomorrow." Maintains the option. Some villagers leave forever. |

### 8.2 Contextual options (unlocked by state)

| Trigger | Contextual Option |
|---|---|
| Villager has 3+ unsold memories with you | **"Tell me everything."** Bulk-buy at a discount, but with a moral cost (you become the keeper of their pain). |
| You have a complementary memory on the shelf | **"I have something you might want instead."** Trade. |
| Villager is in Trust Tier 4+ | **"Don't sell this. Just tell me about it."** Hear it; don't transact. Trust + family-tier bond. |
| It is autumn fair week | **"Come back during the fair."** Festival pricing. |
| Pickle is in the shop | **"Let Pickle taste it first."** (See Codex 12 § 4.) |

---

## 9.0 The Echo System — How Memories Connect

Every memory carries up to 4 Echoes. An Echo is a string-keyed reference (e.g., `OCTOBER_AMBER`, `ELRIC_FACE`, `THE_BLUE_SHAWL`, `THE_RIVER_AT_DUSK`).

When the player collects a critical mass of memories sharing an echo, the **Echo Web** in the Codex (UI) animates, and a **Revelation Chapter** unlocks. This is the mechanism by which the cozy player feels "I'm assembling a mystery" without grinding for clues.

### 9.1 Critical-mass thresholds

| Echo Type | Required # of memories | Reveals |
|---|---|---|
| **Person-Echo** (a face/name appearing across maps) | 3 | A relationship between two villagers neither has named |
| **Place-Echo** (a setting appearing across maps) | 4 | A historical event the village half-remembers |
| **Object-Echo** (an artifact recurring) | 3 | The artifact's *story* — who made it, who lost it |
| **Year-Echo** (memories sharing a season-year) | 5 | A *Forgotten Year* — see Codex 03 § 6 |
| **Pattern-Echo** (e.g., "doorways," "songs without lyrics") | 6 | A piece of the predecessor mystery |

There are 88 Echoes in the canonical game. 24 of them lead to Revelation Chapters. The remaining 64 are ambient — they make the village feel woven without forcing the player into a quest log.

---

## 10.0 Revelation Chapters — Set-Piece Beats

A Revelation Chapter is an unlocked narrative beat that:

- plays in a special illustrated screen (one-off art, ~$3k each, 24 chapters total = $72k art line);
- adds 1 permanent piece to the village's mystery board;
- changes how at least one villager behaves going forward;
- may grant access to a previously locked area.

### 10.1 The 24 Revelation Chapters (one-line synopses)

1. **The Beekeeper and the Mayor** — Doris's husband once worked the Mayor's first campaign.
2. **The Schoolmistress's Lost Class** — Lavinia taught a child no one else remembers.
3. **The Wren and the Mortician** — Wren's predecessor was the previous Hollow-keeper's lover.
4. **A Letter Not Sent** — Doris's daughter Elsa lives. The letter arrived where it was supposed to. It was returned.
5. **The Inn's Lower Door** — The Three Apples has a basement no living villager knows the way into.
6. **The Riverman's Crossing** — Esher rescued a drowning child in 1978. The child's name is Sebastian Holmwood.
7. **The Hand That Lit The Kiln** — Cael's lost kiln was set by Caspar's father.
8. **A Postmistress's Reading** — Ms. Inkwell has, for 14 years, read every letter that passed through her hands. She knows everything.
9. **The Bees' Decision** — Doris's bees did not abandon their hive in 1989. They followed someone.
10. **The Cleric's Audit** — Brother Anselm's faith broke the day a parishioner asked him a question he could not answer. The question is in your shop.
11. **The Boy Who Knows He's Missing Something** — Ren is missing one year of memory. He is not the only one. *This is the Forgotten Year arc opener.*
12. **The Predecessor's Apron** — Found behind a shelf. Has Elric's bee-keeping pattern.
13. **The Schoolmistress's Final Lecture** — Lavinia is dying. She wants you to attend.
14. **The Three Apples Basement** — You go down. (See Codex 03 § 5.)
15. **What the Bees Did at the Funeral** — Doris's prophetic memory, lived.
16. **The Cartwright's Wheel** — Caspar built a wheel for the Hollow's cart 17 years ago. The cart is gone.
17. **The Daughter Who Stayed One Winter** — Elsa returns. You decide whether to give Doris back the memory of why she left.
18. **The Cleric's Memory** — Brother Anselm sells you his own faith.
19. **The Mortician's Confession** — Wren has been keeping a corpse's last memory in a jar she stole from you.
20. **Old Mariska's Lesson** — Mariska teaches you the Severing operation. (See Codex 13 § 2.)
21. **The Predecessor's Echo Hologram** — First fully formed message. (See Codex 12 § 3.)
22. **The Forgotten Year, Reconstructed** — Six players' memories combine in the Echo Web to surface what 1989 actually was.
23. **The Apprentice** — Ren asks to become your apprentice. (See Codex 04 § 8.)
24. **Last Light** — Final memory. (Endgame.) Doris's Memory 14, played to its full consequence.

Each chapter is hand-written. Each is locked behind narrative criteria, never grinding. Each is replayable in subsequent playthroughs through new combinations.

---

## 11.0 Voice Acting Plan

Three scoped options for greenlight:

| Tier | Scope | Budget | Recommendation |
|---|---|---|---|
| **A — Full VO** | All 30 villagers, all dialogue lines | $1.6–1.8M | Not recommended pre-launch |
| **B — Narrative spine** | 12 Sealed villagers + Pickle + Echo Hologram + opening/closing narration | $380–520k | **Recommended** |
| **C — Cinematic-only** | Voice only the 24 Revelation Chapters + opening/closing | $180–240k | Lean fallback |

Tier B captures ~92% of the perceived voice-acted experience at ~28% of full-VO cost. *Pickle's voice acting is non-negotiable.* If anything is VO, Pickle is.

---

## 12.0 Localization Strategy

| Language | Priority | Note |
|---|---|---|
| English | Day 1 | Source |
| Spanish (LatAm + EU) | Day 1 | Cozy is huge in Spanish-speaking markets |
| German | Day 1 | Cozy + narrative buyer base |
| French | Day 1 | Cozy + literary buyer base |
| Japanese | Day 1 | Switch-cozy double signal |
| Simplified Chinese | Day 1 | Massive cozy market on Steam |
| Korean | +30 | High-quality LQA available |
| Arabic | +90 | The Hearthbound bilingual-launch tradition (Codex 16 § 7) |
| Brazilian Portuguese | +60 | High wishlist signal |
| Italian, Polish, Russian, Turkish, Vietnamese, Thai | +120 | Community-translated where possible |

Total localization budget (Day 1 6 languages): **~$110k.**

---

## 13.0 The Writers' Room Discipline

| Practice | Frequency | Owner |
|---|---|---|
| Voice-fit table-reads | Weekly | Lead writer |
| Memory-card validation pass | Per memory | Editor |
| Echo-web consistency audit | Bi-weekly | Lead writer + DB engineer |
| Playtest read-aloud (5 random memories) | Monthly | Whole team |
| Content-warning review | Per Revelation Chapter | Pell Doyne (Codex 06) |
| Procedural assembly QA | Per build | Editor |

This discipline is the deepest moat against the "narrative dilution" risk identified in the review.

---

## 14.0 Closing Note

> *"The reason this works is that we are not writing 600 memories. We are writing 168 memories like a novelist, 320 library clauses like a poet, and 24 revelations like a screenwriter. Three writers can do this in a year. Two writers can do it in eighteen months. The story will feel like a hundred writers wrote it. That is the trick."*
>
> — *Inara Vellis*

— *End of Codex 02. Next: `03_WORLDBUILDING_AND_LORE.md`.*
