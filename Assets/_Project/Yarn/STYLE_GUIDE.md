# 📝 Yarn Spinner — Writers' Style Guide
### Mission 1-2 dialogue authoring standards & cross-reference index

> *"The cozy game lives in its words. Every Yarn line that Doris speaks, every prose sentence in the Evening Ledger, every Codex tooltip on an examined prop, every Pickle quip — these are the surfaces the cozy player will read. Make sure every one is worth reading. No filler. No procedural stitching. Every sentence is a hand-craft."*
>
> — Pell Doyne + Esme Cordray, Focus 07 § 11

This guide is the **single source of truth** for anyone authoring or editing Yarn dialogue in Mission 1-2. It exists alongside the canonical `.yarn` files in this directory.

**Authored by:** Inara Vellis (Lead Memory Writer) + Tobias Marrow (Worldbuilding & Lore Master)
**Maintained by:** any senior writer + the Critic & Review Board

---

## 0. The 7 Voice Signatures of Mission 1-2

Every character in M1-2 has a strict voice signature. Edits that break the signature are **rejected** by the Review Board.

### Doris the Baker (Mission 1)
- ✅ Uses bread metaphors for everything (`"Hold it like you'd hold a hot bun"`).
- ✅ Does NOT mention Elric by name in M1 (he appears in subtext only).
- ✅ Short sentences, ~12 words avg. Northern-rural cadence (*"aye," "summat"*).
- ✅ Offers tea unprompted. Twice.
- ❌ Never says goodbye at end of M1 (`"Sleep tonight. Dreams come."` is the canonical exit).

### Gerrold Pell (Mission 2)
- ✅ Apologises constantly — over-thoughtfully, never weakly. *"I'm sorry"* appears 8+ times in M2.
- ✅ Verbal tic: **"the long bit."** Recurs exactly 4 times.
- ✅ Carpenter metaphors when emotion fails him (*"the wood, the joint — they tell you"*).
- ✅ Calls the player *"keeper"* ONLY at the end of M2 (after the moral choice).
- ❌ He does NOT cry on-screen. On any path. The player will. He won't.
- ❌ Never asks for sympathy. Never says "this is hard."

### Pickle (the cat)
- ✅ Italic, lower-opacity rendering. NO Bamao box around her text.
- ✅ Speaks ONLY to the player — internal monologue. NPCs cannot hear her.
- ✅ Mission 1-2 budget: exactly **4 canonical lines + 4 conditional pre-choice + 3 contextual + 2 hints**. The other ~210 quotes wait for M3+.
- ✅ Gentler around men in mourning. Never jokes around grief (Codex 07 § 4.2).
- ✅ Sass intensity scales with `$pickle_sass_intensity` (1 / 3 / 5 differentiated in M1-2; 2 and 4 collapse to nearest).

### Marin (the predecessor) — off-screen voice
- ✅ Signs every note `"— M."` (never "Marin," never her full name in M1-2).
- ✅ Name spoken aloud ONLY in Mission 2 if `$asked_about_predecessor == true`.
- ✅ Mission 1-2 visible content: 2 notes (Polish + Cleanse), 3 welcome orbs, 1 apron, 1 worn tool-roll, 1 leather Ledger, 1 perpetual-warm teapot, 1 chapter-marked book of folklore.

### Margery Pell — off-screen entirely
- ✅ NO model. NO portrait. NO lines. Lives ONLY in: Gerrold's dialogue, the cracked memory orb (GER-007), and the embroidered handkerchief.
- ✅ The handkerchief's monogram is **dark green silk** "M," uneven — *"the work of a young hand"* (age 22, year of her wedding).

### The Silent Villager (lane bench)
- ✅ 0 lines in M1-2. Nods only.
- ✅ Will speak in Mission 4+. Plant the recognition now.

### The Keeper (the player)
- ✅ Silent protagonist. Canonically nameless and ageless in M1-2.
- ✅ Dialogue choices are READ on screen but NEVER voiced.

---

## 1. The Yarn File Roster (Mission 1-2)

| File | Owner | Mission | What it contains |
|---|---|---|---|
| `Doris_M1.yarn` | Vellis | M1 + M2 morning | Doris's 180+ lines, the price-negotiation, the refusal path, Day 2 Marin reveal |
| `Gerrold_M2.yarn` | Vellis | M2 | All 270 lines, all 4 choice paths, the cottage interior, the handkerchief outro |
| `Pickle.yarn` | Tannenbaum (peer-review: Vellis) | M1 + M2 | All 4 canonical + 4 conditional + 3 contextual + 2 hints |
| `Marin_Notes.yarn` | Marrow | M1 + M2 | 2 pinned-note paragraphs (Polish + Cleanse), 1 locked door |
| `Codex.yarn` | Marrow | M1 + M2 | All 28+ examinable prop tooltips |
| `EveningLedger.yarn` | Cordray | M1 + M2 | Day 1 (2 variants) + Day 2 (5 variants) + 7 Vow reflections |
| `Dreams.yarn` | Aleko | M1 + M2 | Dream 1 (1 variant) + Dream 2 (5 variants) + shared sleep/wake transitions |
| `ChoiceCards.yarn` | Cordray | M1 + M2 | Choice-card framing + option labels + previews + sub-choices |

**Total Yarn LOC:** ~830 lines of cozy-narrative-grade fiction-voice prose across 8 files.

---

## 2. Yarn Syntax Conventions (Hearthbound flavour)

### 2.1 Node naming

```
<Character>_<Mission>_<Beat>
e.g. Doris_M1_OfferOrb
     Gerrold_M2_CottageInterior
     Pickle_M2_TeaBrewing
```

For shared nodes (transitions, choice cards): `<System>_<Mission>_<Beat>`

```
e.g. SleepTransition_M1_M2_Shared
     ChoiceCard_M2_OptionA_Erase
     EveningLedger_M2_Day2_VariantD_Listen
```

### 2.2 Tags

Every node MUST carry at least one tag from this list:

| Tag | Meaning |
|---|---|
| `mission1` / `mission2` | Which mission |
| `doris` / `gerrold` / `pickle` / `marin` | Speaker |
| `examine` | Codex/prop tooltip |
| `ledger` | Evening Ledger |
| `dream` | Memory Dream |
| `choice_card` | Choice UI |
| `hint` | Optional Pickle hint |
| `conditional` | Plays only on a variable check |
| `canonical_line_N` | Pickle's 4 canonical lines (numbered) |

### 2.3 Custom commands

Defined in `YarnCustomCommands.cs`. The 14 commands in M1-2:

```yarn
<<eyes_look_at "player" | "orb" | "handkerchief" | "door">>
<<adjust_trust "doris" 5>>                      // delta on trustDoris
<<adjust_coin -4>>                              // delta on coin
<<adjust_vow "vow7" 5>>                         // delta on vowNIntegrity
<<adjust_predecessor_trail 3>>                  // M3+ unlock fuel
<<adjust_pickle_approval 1>>
<<adjust_cinder 3>>                             // Confession Booth currency
<<show_orb "DOR-001" "in_cloth">>
<<give_player_orb "GER-007">>
<<player_picks_up_orb_from_cloth "GER-007">>
<<orb_appears_cracked>>
<<echo_web_activate "DOR-001" "GER-007" "Doris in kitchen on Sunday">>
<<show_heavy_theme_card "Loss of spouse, terminal illness, a man's grief">>
<<show_choice_card_with_orb_in_hand>>
<<set_cleanse_profile "Aggressive" | "Careful" | "Gentle">>
<<start_cleanse_minigame "GER-007">>
<<offer_polish>>
<<offer_tea_brewing>>
<<unlock_hollow_door>>
<<play_animation "Doris" "ACS_Offer_Box">>
<<play_cutscene "Mission2_ListenScene_FullMonologue">>
<<play_sfx "pickle_real_purr_short">>
<<play_sfx_loop "pickle_purr_loop" 0.3>>
<<dialogue_end>>
<<wait 1.5>>
```

### 2.4 Conditional rendering

Pickle's pre-choice lines and conditional contextual lines use Yarn `<<if>>` blocks against the variables exposed in `YarnVillageStateBridge.cs`:

```yarn
<<if $pickle_approval >= 50>>
    Pickle: This is the older craft.
    Pickle: The one nobody pays for.
<<endif>>
```

For prose variants (e.g. Gerrold's outro per path) use nested `<<if>><<elseif>><<else>><<endif>>` against `$gerrold_choice` and `$cleanse_quality`.

### 2.5 Dual-mode dialogue rendering

Two character types render differently in `DialogueUI.cs`:

| Speaker | Box style | Font | Audio cue |
|---|---|---|---|
| Doris / Gerrold (NPC) | Bamao parchment box w/ portrait | TMP Regular | Voice line |
| Pickle (internal) | NO box, floats over scene | TMP Italic, lower opacity | Pickle leitmotif chime, faint |

The `pickle` tag triggers the italic mode automatically. NEVER explicitly mark internal lines with `Pickle:` in NPC-dialogue contexts.

---

## 3. The Cordray Principle (Choice Cards)

> *"The player infers the consequence; they are never given numbers."*

### Never write:
```yarn
ChoicePreview: He will keep the memory. (Trust +4, Vow 3 +5)
```

### Always write:
```yarn
ChoicePreview: He will keep the memory, gentler.
ChoicePreview: You will need a careful hand.
```

Numbers go into VillageState. Fiction goes onto the parchment.

---

## 4. The Vellis Six Rules (writing prose)

1. **No memory shall begin with the word *"It"*** unless the *it* is a named object in the room. (Pentiment Rule.)
2. **Every memory shall contain at least one sensory detail no other memory in the bible contains.** (The fingerprint.)
3. **Grief, joy, shame, awe, longing, dread, and grace** are the seven primary emotional palettes. Memories use one primary + up to two secondaries.
4. **Never explain the magic.** Memory orbs are real. Polishing works. Cleansing has risk. We don't theorize aloud.
5. **Dialogue carries the voice; narration carries the world.** Villagers must be unmistakable by line alone.
6. **Cozy framing in heavy moments.** If a memory contains a death, the death is observed through *what someone made for the dying person* (a soup; a folded shawl; a song they forgot the words to).

---

## 5. The Marrow Principle (Codex tooltips)

Every prop tooltip must do **one of three things**:

1. **Tell the player something about Marin** (the predecessor). E.g. the apron, the tool-roll, the perpetual-warm teapot.
2. **Seed a long-arc story.** E.g. the dormant beehive (M3+ Wedding Honey arc), the lampman book (M7+ Memory Dream), the bedroom door (M5+).
3. **Warm the room.** E.g. the kettle orb (*"holding the orb makes the room feel warmer by half a degree"*).

A tooltip that just describes geometry (*"A wooden bench."*) is **rejected**.

---

## 6. The Tannenbaum Principle (Pickle)

> *"Pickle's funniest line should NEVER be louder than the warmest moment in the room. She is sarcasm at a low volume, not a stand-up routine."*

| Rule | Why |
|---|---|
| Pickle never speaks during the 3-minute Listen Scene. | Gerrold's grief is the foreground. |
| Pickle's sass intensity scales DOWN around grief. | She is gentler around men in mourning. |
| Pickle gets EXACTLY 4 canonical lines in M1-2. | More dilutes her. |
| Pickle's conditional lines are unlocked by Approval ≥50. | Earned, not given. |

---

## 7. Cross-Reference Index

Some lines reference other parts of the world. These cross-references should be PRESERVED across edits — they form the cozy game's Echo Web at the meta-level.

| Source line | Target | Why |
|---|---|---|
| Doris M1: "The cat watched me. Judged me, I think." | Pickle has been at the Hollow for years. | Marin trained both. |
| Gerrold M2 walk: "the wood gave last winter and I haven't planed it true yet." | The winter Margery was dying. | Silent metaphor. |
| Cottage codex: "A book... pressed leaf, exactly like the one in your shop's reading chair." | Marin's reading-chair book. | The leaf style is Marin's signature. |
| Wedding photograph codex: "In the background, a woman with bread." | Doris in 1957. | Echo Web's source material. |
| Pickle EchoWeb: "This one and yesterday's are in the same kitchen on the same Sunday." | DOR-001 + GER-007 share the Echo. | The Web's first visualization. |
| Margery's handkerchief: "M" embroidered, year 22 (her wedding). | Wedding photo + Gerrold's outro. | Three-prop chain. |

---

## 8. The Localisation Plan (Mission 1-2)

Per Focus 02 § 12 — 6 launch languages. **All 6 must be translated by Week 12** of the build timeline:

| Language | Word count (M1-2) | Translator cost |
|---|---|---|
| English (source) | ~4,200 (Doris + Gerrold + Codex + Ledger + Dreams + ChoiceCards + Pickle) | (in-house) |
| Spanish (LatAm+EU) | ~4,200 | $840 |
| German | ~4,200 | $920 |
| French | ~4,200 | $920 |
| Japanese | ~4,200 | $1,200 (premium for fluent literary register) |
| Simplified Chinese | ~4,200 | $640 |

Total localisation cost: **~$4,520** + 8 hours coordination.

The cozy player who plays in their native language gets the same warmth as the English-source player. That is the contract.

---

## 9. The Editor's Cadence

Per Codex 02 § 13:

| Practice | Frequency | Owner |
|---|---|---|
| Voice-fit table-reads | Weekly | Vellis |
| Memory-card validation pass | Per memory | Marrow |
| Echo-web consistency audit | Bi-weekly | Vellis + DB engineer |
| Playtest read-aloud (5 random nodes) | Monthly | Whole team |
| Content-warning review | Per Heavy Theme Card | Doyne |
| Pickle sass-intensity test | Per Pickle line | Tannenbaum |

This discipline is the deepest moat against the "narrative dilution" risk identified in the original Expert Review.

---

## 10. Critic & Review Board Sign-Off

| Reviewer | Verdict | Note |
|---|---|---|
| **🎬 Creative Director** | ✅ Approved | "The voice-signature contract is documented and enforceable." |
| **🗺️ Game & Level Designer** | ✅ Approved | "Every custom command in § 2.3 maps to a real C# handler in YarnCustomCommands.cs. The node-naming convention will scale." |
| **🎨 Lead Memory Writer (Vellis)** | ✅ Approved | "If a future writer joins the team and reads only this file, they will sound like us within their first week." |
| **🌍 Worldbuilding & Lore Master (Marrow)** | ✅ Approved | "The Marrow Principle is preserved." |
| **🐱 Humor & Levity Designer (Tannenbaum)** | ✅ Approved | "Pickle's 4-canonical / 4-conditional / 3-contextual / 2-hint budget is the right shape for M1-2." |
| **🔍 Critic & Review Board** | ✅ **Approved** | "Ready to onboard the localisation team verbatim." |

---

*Document version 1.0 — authored alongside the gameplay-guide compliance pass on `feat/mission-1-2-architecture`.*
*Part of the Abdulmalek Agents game-concept portfolio · 2026.*
