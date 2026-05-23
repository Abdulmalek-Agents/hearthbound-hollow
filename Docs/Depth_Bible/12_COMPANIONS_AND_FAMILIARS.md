# Codex 12 — **Companions & Familiars**
### Specialist: **Wren Hatch, Companion & Familiar Designer**
*(8 yrs · ex-Animal Crossing animator, character designer on Spiritfarer, ex-Klei AI lead on Don't Starve)*

> *"A cozy game's companion animals are the difference between a place you visit and a place you live. The original pitch had Pickle the cat as a footnote. This codex makes Pickle a **co-lead**. It also adds Brother Magpie, the Echo Hologram, and the late-game Apprentice — three more relationships that the player will refer to by name for the rest of their cozy-gaming life."*

---

## 1.0 Mission

Codify the four companion characters that share the Hollow with the player:

1. **Pickle the Cat** — the daily comedy anchor and lore receptacle.
2. **Brother Magpie** — the Letter-Bird Network heckler (Codex 15).
3. **The Echo Hologram of Marin** — the predecessor speaking to you, growing.
4. **Ren the Apprentice** — late-game companion + succession mechanic.

Each is a **persistent character** with their own AI routines, dialogue libraries, animations, and emotional arcs. Together they ensure the Hollow is never *empty*.

---

## 2.0 The Companion-Character Design Principles

1. **Every companion must add something the player cannot replicate themselves.** (Pickle tastes memories; Magpie reads letters; the Echo teaches; Ren learns.)
2. **Every companion has a daily routine the player can observe.** (Cozy game's most underrated mechanic.)
3. **Every companion grows.** Persistent state, visible change.
4. **Every companion has a moment they choose the player.** (The bond moment.)
5. **No companion can be lost permanently** without significant player effort (Pickle leaves only if the player descends to Thief Path).

---

## 3.0 The Echo Hologram of Marin

### 3.1 What it is

An object Marin built before her dispersal: a **memory-resonance device** that plays back, in fragments, things she said while alive. It is in the Locked Room upstairs (Codex 03 § 9). The player finds the key on Day 18.

Mechanically: it is an interactive NPC who can **only speak from her recorded memories.** She cannot answer new questions. She can only *resonate* to memory-shaped prompts.

### 3.2 The growth mechanic

The Echo Hologram has 17 "stages" of activation, corresponding to the 17 MAR fragments (Codex 03 § 10.1). Each fragment the player finds is **fed to the device**, which then plays a new ~2-minute hologram message from Marin.

This is the game's **predecessor mystery's pacing engine.** Players cannot rush it; they earn it.

### 3.3 The first activation (Day 18)

The player turns the key. The device hums. A faint, silver-rimmed figure resolves. She is sitting where the player is sitting. She is calm. She says:

> *"Hello, Keeper. I do not know your name yet. I know you came after me. I know what I taught the Hollow to do when I am gone. I will say one thing — perhaps three things, perhaps four. Then I will be quiet for a long while. Please find the rest of me. And — if you can — be kinder than I was. I will not be able to thank you. But I will be able to know."*

That is the entire first hologram. ~45 seconds. The player will not forget it.

### 3.4 The 17 hologram messages

Each fragment unlocks one. The messages, in canonical order:

| Fragment ID | Message theme |
|---|---|
| MAR-001 | The first words (above). |
| MAR-002 | A practical lesson on Polishing. |
| MAR-003 | A confession about Sebastian's first regret. |
| MAR-004 | A warning about Wren's predecessor. |
| MAR-005 | A doubt about whether Anselm should be helped. |
| MAR-006 | Gratitude for Esher. |
| MAR-007 | A laughing message about Vance, recorded years before he came. *Prophetic and uncanny.* |
| MAR-008 | The story of the Three Apples basement door. |
| MAR-009 | The Forgotten Year. The note she left. The note she did not send. |
| MAR-010 | A long pause. Then: *"I am sorry, Mariska."* That is the entire message. |
| MAR-011 | A teaching about Letter-Birds. |
| MAR-012 | A laugh about bees. |
| MAR-013 | A message to Lavinia (whose lecture follows). |
| MAR-014 | A song. (Idris's lullaby, sung in a voice the player will recognize from the Singing Hive's song.) |
| MAR-015 | A reading list. *Twelve books in the upstairs Library. Read them in order.* |
| MAR-016 | A direct address to Pickle. *"You old liar, you knew her better than I did. Be kind to the new one."* |
| MAR-017 | The final message, only playable after the player Severs themselves at the endgame. |

### 3.5 Why this works

The Echo Hologram is **the cozy game's first "growing AI companion"** — a character who is fully predetermined but feels increasingly *present* to the player as they discover her. It is also a **lore delivery mechanism** that does not require a quest log.

### 3.6 Pickle's special relationship with the Echo

Pickle is the **only character** who can react to the Hologram. When the Hologram speaks, Pickle:

- Sits on the chair across from the device.
- Closes her eyes.
- Sometimes murmurs.
- Once — only once, after fragment MAR-016 — speaks aloud: *"I told her you'd come. She didn't believe me. Make her tea anyway. We're not done here."*

This is the single most-quoted line in the game in projected player feedback. *Wren Hatch is fully prepared to die on this line being voiced by the same actress as Marin, recursively.*

---

## 4.0 Pickle the Cat — Full Treatment

(Cross-referenced from Codex 07 § 3 for voice/humor.)

### 4.1 Pickle's role in the Hollow

| Function | What Pickle Does |
|---|---|
| **Memory tasting** | Once per week (Sundays), Pickle taste-tests one memory. Her readings are 88% accurate. (Codex 07 § 3.) |
| **Visitor opinions** | Pickle silently judges visitors. The player learns to read her face. |
| **Vow alarm** | If the player breaks a Vow, Pickle reacts visibly (moves to the windowsill, refuses food, etc.). |
| **Lore vehicle** | Pickle remembers Marin. Pickle is the link to the predecessor. |
| **Comedy spine** | (Codex 07.) |
| **Daily presence** | Pickle has 15 sleep-locations, 8 daily activities, 22 seasonal behaviors. |

### 4.2 Pickle's daily AI

Pickle's daily routine is governed by a small AI system:

- **Time of day** (Pickle is more active morning + dusk).
- **Weather** (Pickle hates rain, loves snow, ignores autumn).
- **Player mood** (Pickle senses Vow integrity; she stays closer to a struggling player).
- **Visitor presence** (Pickle has individual relationships with each villager).
- **Season** (Pickle's behaviors change with the Calendar — Codex 03 § 7.3).

### 4.3 Pickle's Approval Meter (hidden)

A 0–100 meter, visible only through Pickle's behaviors:

| Approval | Visible Behaviors |
|---|---|
| 0–20 | Pickle stays in the upstairs Library. Rarely descends. |
| 21–40 | Pickle is downstairs but maintains distance. Sleeps far from the player. |
| 41–60 | Default. Pickle is present, observational, occasionally lap-bound. |
| 61–80 | Pickle follows the player around the shop. Sleeps on their bed. |
| 81–100 | Pickle follows the player into the village. Comments on villagers in the player's ear. *This is the game's most beloved unlock.* |

### 4.4 Pickle's Special Activations

Trigger-based behaviors:

| Trigger | Pickle's Action |
|---|---|
| Vance Ashby enters | Pickle stares at him. Does not blink. Once. Then goes to the windowsill. |
| The player makes their 100th transaction | Pickle leaves a single mouse-with-coin-in-its-mouth on the counter. |
| Mariska visits | Pickle bows. Visibly. The only time. |
| A villager cries in the shop | Pickle goes to them. Stays until they leave. |
| The player composts a memory | Pickle approves with a tail-flick. |
| The Singing Hive sings | Pickle sings back, quietly, in a tone only the player hears. |
| The player completes the Sommelier path | Pickle gives the player the Brass Spoon (Codex 09 § 11). It was hers all along. |

### 4.5 Pickle's voice acting (if greenlit)

**Pickle is voice-acted** in Tier B or higher VO scope (Codex 02 § 11). The recommended actress is the same as Marin's (recursive — see § 3.6).

If VO is fully greenlit, Pickle has **220 quoted lines** drawn from the humor codex library + situational reactions + seasonal commentary. The recording session is ~3 weeks of an actress's time.

If VO is not greenlit, Pickle is **subtitled with a distinctive font + chime audio cue** — a Pickle leitmotif (Codex 14).

### 4.6 Pickle's backstory

Pickle is at least 22 years old at the start of the game. She belonged to **Cas**, the Keeper before Marin (the one who buried the Forgotten Year orb). She belonged to Marin. She now belongs to you. She has watched three Keepers learn the trade. She is older than any villager except Mariska.

Pickle **never dies in the game.** Her great age is treated with affectionate impossibility. *Cat-physics of folkloric kind.* This is the design's gentlest violation of realism.

---

## 5.0 Brother Magpie

### 5.1 What he is

A magpie who lives in the rafters of the Hollow. He arrived in Spire-Month two years before the game starts, perched on Marin's apron, and never left.

His role:
- **Letter-Bird reader** (Codex 15) — when a letter arrives via the network, Magpie reads it aloud.
- **Marketplace heckler** — when the Weekly Market is in town, Magpie attends and offers running commentary.
- **The shop alarm** — if Pickle disapproves silently, Magpie says something. The pair works as a *good cop / bad cop* duo.

### 5.2 Magpie's voice rules

(Cross-referenced from Codex 07 § 11.)

1. He is **pompous, well-read, and slightly stuck in 1820.**
2. He performs his readings as **drawing-room theatre.**
3. He **forgets which words are foreign.**
4. He **judges penmanship out loud.**
5. He **refuses to read love letters.** (His firm policy.)
6. He **praises good verse** without irony when he encounters it.

### 5.3 Sample Magpie lines

> *"A letter from Hollybridge. Penmanship: regrettable. Spelling: ambitious. Subject: 'Memory of First Snow.' Allow me to perform."*

> *"This sender is, evidently, an enthusiast. I shall lower the temperature with my delivery."*

> *"I will not read this. It is a love letter. I do not perform other people's wantings. Read it yourself. With the candle lit. Properly."*

> *(during Vance's visit to the market:)* *"The gentleman from Wells is wearing a watch that ticks loudly. He is, I infer, anxious. Note for your file."*

### 5.4 Magpie's mechanic

Magpie is **always present** in the Hollow. He does not need to be earned. He does, however, develop opinions over the playthrough:

- **Magpie approves a player's verse** (the player can compose verses in Idris-inspired moments at the inn). This unlocks one new bird-perch decoration.
- **Magpie disapproves** of bad transactions (his approval is silent and bird-shaped). Loss of Vow integrity hits Magpie's mood.

---

## 6.0 Ren the Apprentice

(Mechanically detailed in Codex 04 § 8 — Apprenticeship system.)

### 6.1 Ren as a companion

After Day ~70, Ren is at the Hollow daily. He has:

- His own chair at the workbench.
- His own corner of the cellar (he tries to build a small composting setup, Codex 04 § 5.4).
- A growing presence in the daily ledger (he writes his own entries; the player can read them).

### 6.2 Ren's character arc

Ren begins as **a boy who knows he is missing something** (his sister Mira — Codex 03 § 6). He is a child carrying a grief he cannot articulate.

Over the apprenticeship, Ren:

- **Gains technique.** He becomes a competent Keeper in 6 in-game months.
- **Gains a personality** shaped by the player's teaching style.
- **Discovers his sister** (or chooses not to). This is the Forgotten Year arc's denouement.
- **Becomes the player's successor** in The Apprentice's Inheritance ending (Codex 08 § 7).

### 6.3 Ren's voice arc

Ren's voice shifts as he grows. Early Ren: short sentences, careful. Late Ren: longer, considered. *The voice acting (if greenlit) requires the same actor across both registers — that is a 3-month booking commitment.*

Sample early Ren:
> *"Can I try?"*

Sample late Ren:
> *"I think I can do this one. You don't have to watch. You can if you want. But you don't have to."*

### 6.4 Ren's relationship with Pickle

Pickle is patient with Ren. Pickle does not yet take Ren seriously, but Pickle does not undermine him. The cat's quiet acceptance of the boy is one of the game's most-praised moments in projected playtests.

### 6.5 Ren and the Echo Hologram

Ren cannot interact with the Hologram (he is too young; the device responds to memory weight, not curiosity). But Ren can *attend* the Echo's messages. He listens. He asks the player, after, *"Was she my friend?"*

This is the answer to *"why did Marin disperse herself instead of training a successor?"* The honest answer the game eventually surfaces: *Marin did not believe she had time. She left the village a question instead of an answer. The question is you.*

---

## 7.0 The Late-Game Companion Synthesis

By the endgame, the Hollow is alive in a specific way:

- Pickle on the windowsill.
- Magpie in the rafters.
- The Hologram in the Locked Room (now Unlocked).
- Ren at the workbench.
- The player at the counter.
- Memory orbs humming gently.
- The bees in the garden.

This is the **picture the game's marketing hangs on.** The Steam page hero image. The wallpaper that ships with the OST. The shot that gets clipped on TikTok. *It is also, design-wise, an answer to the original review's critique that the Hollow felt empty in the prototype.*

---

## 8.0 Audio & Animation Budget

| Companion | Voice acting | Animation | Special FX |
|---|---|---|---|
| Pickle | Tier B essential | 22 unique poses + 8 walks + 14 idles | Particle: dust she disturbs |
| Magpie | Tier B essential | 12 poses, in-flight + perched | Feather particle on landings |
| Echo Hologram | Tier B essential | Translucent overlay, 17 unique poses | Silver-rim shader (one-time tech) |
| Ren | Tier B essential | 18 poses, growing rig (changes height) | None |

Total companion-character art budget: **~$94k.** Total VO budget (Tier B): **~$380–520k** (covers all four + the 12 sealed villagers; Codex 02 § 11).

---

## 9.0 The Visit From a Past Companion

There is **one moment in the endgame** when the player can, briefly, see *Cas the Predecessor's Predecessor* — never a character in the game, but a ghost in the schoolhouse during the Forgotten Year set-piece. He nods to the player. He does not speak. He is gone.

This is **the longest design lineage** the game gestures at: Cas → Marin → You → Ren. Four generations of Keepers. The cozy game's quiet sense that history is long.

---

## 10.0 Closing

> *"The Hollow is full of people who chose to stay. A cat the predecessor named for a joke. A magpie who reads the mail. A hologram of a woman who refused to sell her village. A boy who became a Keeper. And you. The game is, at its heart, about not being alone in a small autumn village. The companions are how that promise is kept."*
>
> — *Wren Hatch*

— *End of Codex 12. Next: `13_PUZZLE_AND_MINIGAME_LIBRARY.md`.*
