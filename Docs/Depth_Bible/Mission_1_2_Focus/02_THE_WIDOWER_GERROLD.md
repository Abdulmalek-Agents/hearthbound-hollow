# Mission 1-2 · Focus 02 — **The Widower (Gerrold Pell)**
### Mission 2's heavy-write character · The player's first moral choice · Full character bible + Yarn dialogue + 3 outcome branches
### Owner: Inara Vellis (Lead Memory Writer)

> *Gerrold Pell is the Widower. His late wife is **Margery Pell** (died 11 in-world months ago after a 14-month illness). He has been carrying a single memory of her last week and has been carrying it badly. Mission 2 is the moment a Keeper steps into the gap he has lived in alone since the funeral. The player has been a Keeper for one day. They will have to be one for him today.*

---

## 1.0 Mission 2's Single Job

Mission 2 must teach the player **one thing Mission 1 deliberately did not**:

> *A memory transaction is a moral act. The player must choose between three real options — and the village will know which one they chose. Even Pickle will know.*

Mission 2 is the **emotional weight pivot.** Mission 1 was warm. Mission 2 introduces the *cozy-with-real-consequence* register the game's brand is built on. Spiritfarer's mid-game retention drop (Codex 06 § 9) was caused by *unsignaled* heavy beats. Mission 2's heavy beat is **fully signaled** — by Doris in dialogue, by the Tone Compass (Codex 06), by the soft tone of the day, and by Pickle's posture.

The cozy player walks into Mission 2 prepared. Mission 2 still surprises them.

---

## 2.0 Voice Signature — Gerrold Pell

### 2.1 Seven rules for writing Gerrold

1. **He apologizes constantly.** *"Sorry — I'm sorry — that was wrong of me to ask."* His apologies are not weak; they are *over-thoughtful.* He has been grieving alone for 11 months and has forgotten what other people sound like.

2. **He talks to the player as if the player is a doctor.** Formal sentence structure. Doesn't quite make eye contact. Speaks of *"my wife"* before he speaks of *"Margery"*. The shift from one to the other in his arc is the marker of his trust.

3. **He uses one specific verbal tic: "the long bit."** *"It's not the dying part. It's the long bit."* *"I want the long bit gone."* This phrase recurs ~4 times. It is the way he names his grief.

4. **He never asks for sympathy.** Never says *"this is hard."* Never asks the player to feel bad for him. He treats his grief as a problem to be solved, with the dignity of a man who has been a problem-solver for forty-one years.

5. **He was a carpenter.** Builds and repairs furniture. His sentences carry quiet craft-references — *"the joint was good but the wood gave"*, *"some things sand smooth, some need to be planed."* He uses his trade's vocabulary when he cannot find emotional vocabulary.

6. **He brings the orb in a cloth.** A handkerchief, ironed, white, folded around the memory orb like a wrapped tool. The player will notice. He will not explain. The cloth is Margery's — one of three she folded for him before her last winter.

7. **He does not cry in Mission 2.** This is the writer's discipline. The scene is harder *because* he does not cry. The player will. (Steam reviews of vertical-slice playtests are projected to overwhelmingly highlight this scene.)

### 2.2 Sample voice (the tonal anchor)

> *"I'm sorry. I don't know how this is supposed to go. I have the — the thing — I have it in this cloth. Margery wrapped it. I think she wrapped it for this."*

> *"I want to keep my wife. I do not want to keep the long bit."*

> *"... You're allowed to ask me to think about it more. I would understand. I am not — I am not in a hurry, today, exactly. I am only here."*

> *(when the player asks him about Margery's life before)* *"She made bread on Sundays. Doris taught her. They sat in the kitchen together for years before — before the long bit."*

> *(after the player makes a choice, regardless of which one)* *"Thank you. I do not know whether you have done what I asked. I think you have done what you could. ... I will go home and see what the morning brings."*

These lines together establish **the entire voice signature** for Gerrold. The ~250 lines of full Yarn dialogue in §7 calibrate against them.

---

## 3.0 Margery Pell — the off-screen character

Margery is dead before Mission 2 begins. **She has no lines, no portrait, no model.** She exists entirely in:

- Gerrold's dialogue (he describes her in fragments).
- The cracked memory orb (her last week of life is what is *in* the orb).
- A single handkerchief prop (the cloth Gerrold brings the orb in — Margery's needlework).

This is the cozy-narrative *off-screen character* technique. Margery is more present *because* she is unseen. The player builds her in their head.

### 3.1 Margery's life (background for the writer, not all spoken in M2)

| Field | Value |
|---|---|
| Born | 1934 (would be 58 at game's present) |
| Died | 11 in-world months before game start, age 57 |
| Cause of death | "A long illness." (Heart-failure consequent on a 14-month wasting; deliberately unspecified in dialogue. The cozy contract: never medicalize. Codex 03 § 11.) |
| Marriage | 36 years with Gerrold |
| Children | One stillborn (~1968). No living children. *(This is significant — but Gerrold does not mention it in Mission 2. It is Mission 6+ content.)* |
| Trade | Schoolteacher's assistant (worked under Lavinia Embry — a hand-sealed villager in the long-form bible). Quiet woman. Read more than she spoke. |
| Voice | Soft, dry, *kind in the way that does not announce itself.* |
| What she taught Gerrold | "How to be quiet around someone you love without making it feel like distance." (His line, in dialogue § 7.3.) |
| Friend | Doris the baker. They sat in the kitchen together on Sundays for ~30 years. (This is the Mission 2 → Mission 1 cross-reference — see § 3.4.) |

### 3.2 The orb's contents — what Gerrold actually carries

The orb is **DOR-WID-001** (mission-internal ID; long-form ID would be `GER-007`). The memory the orb contains, in full:

```yaml
memory_id: GER-007-THE-LAST-WEEK
villager_id: GERROLD_PELL
season_lived: Saltlight (winter), age 60 (11 months before game start)
anchor: "the click of a folded handkerchief landing on a bedside table"
setting:
  place: "the Pell house, the back bedroom"
  weather: "snow that did not stop for six days"
  light: "low amber from a single bedside lamp; the rest of the house dark"
cast:
  - GERROLD (self, 60)
  - MARGERY (wife, 57, dying)
  - DORIS (visiting, briefly, with bread)
action_summary: |
  The last week of Margery's life. She is awake but no longer eating. Gerrold
  brings her a folded handkerchief from her drawer because she has asked for one
  and forgotten she asked. He puts it on the bedside table. She picks it up,
  looks at the embroidery she did when she was twenty-two, smiles at it like
  it is a stranger she is pleased to meet, and falls asleep without setting it
  down. The week passes this way. Six days of small motions. On the seventh
  morning she does not wake. Doris is in the kitchen with bread. The snow
  has finally stopped.
emotional_palette:
  primary: GRIEF
  secondary: GRACE
  tertiary: SHAME
  // Shame is in the palette because Gerrold blames himself, mildly and incorrectly,
  // for not having done more during the long bit. The Cleanse mini-game will let
  // the player remove this shame component — see § 5 below.
wound_or_wonder: WOUND
weight: 9 / 10
clarity: 5 / 10  (faded; he has been carrying it badly)
cracks: 4  (significant — this is the Mission 2 Cleanse tutorial)
crack_locations:
  - "the bedside table" (1 crack)
  - "Doris in the kitchen" (1 crack)
  - "the seventh morning" (2 cracks — the deepest)
echoes:
  - HANDKERCHIEF       (cross-refs Margery's needlework; appears in DOR's memory map at DOR-LOC-2)
  - DORIS_IN_KITCHEN   (cross-refs Mission 1's setting — Memory Map node will light)
  - THE_SEVENTH_MORNING (universal grief-fingerprint; appears in 4 other villagers' maps)
content_warnings:
  - loss-of-spouse
  - terminal-illness (referenced; not depicted explicitly)
gentle_mode_substitution:
  action_summary: |
    The last week of Margery's life. Gerrold brings her a folded handkerchief
    she asked for. She looks at the needlework she did when she was twenty-two
    and smiles. Doris is in the kitchen with bread on the morning Margery does
    not wake. The snow has stopped.
  // The Gentle Mode prose is ~30 words shorter. The image of the smile is kept.
  // The reference to "six days of small motions" is sanded. The hardest single
  // word — "dying" in the original — is replaced by "ill" in Gentle Mode.
post_polish_state:
  clarity: 9 / 10
post_cleanse_states:
  perfect:
    description: "All 4 cracks sealed. Shame component removed. Grief and Grace remain."
    palette_after: { primary: GRIEF, secondary: GRACE }
    gerrold_reaction: "He carries his wife clearly. He does not carry his own blame."
  acceptable:
    description: "3 of 4 cracks sealed. Shame partially removed. Some grief slightly muted."
    palette_after: { primary: GRIEF (lighter), secondary: GRACE, tertiary: SHAME (faint) }
    gerrold_reaction: "He carries his wife. The long bit is less heavy. He is not certain whether this was the right thing."
  crossed_core:
    description: "Player crossed into the core memory during Cleanse. Margery's face faded in the orb."
    palette_after: { primary: GRIEF, secondary: SHAME (Gerrold's own, surged) }
    gerrold_reaction: "He thanks the player. He goes home. He is now grieving someone whose face he half-cannot recall. This is not catastrophic but it is *real.* It is recoverable in Mission 6+ if the player apologizes and works to restore."
```

This dossier is the **production-locked** Mission 2 memory. The writer signs off on it. The senior engineer implements it as a `MemoryNodeSO` ScriptableObject. The artist illustrates the Memory Dream (§ Focus 05).

### 3.3 The Cleanse choice tree — what each outcome does to Gerrold

The three Cleanse mini-game outcomes (Perfect / Acceptable / Crossed Core) are described in Focus 04. Here we describe what they *do to Gerrold's life* over the rest of the game.

| Outcome | Mission 3+ effect on Gerrold's behavior | Long-form arc impact |
|---|---|---|
| **Perfect Cleanse** | Gerrold returns to the village social fabric. Attends Long-Night Festival. Speaks of Margery by name without flinching. | Becomes a tertiary supporting villager. Has 3-4 follow-up beats across Mission 6-9. |
| **Acceptable Cleanse** | Gerrold is gentler than before but quieter. Visits the bakery on Sundays alone. Does not yet say her name aloud in public. | Has 2 follow-up beats. The player can return in Mission 8 to "complete" the cleanse — moving him from Acceptable → Perfect. |
| **Crossed Core** | Gerrold does not return to the village square. He stays home. **Mission 6 contains a sub-quest in which the player can attempt to restore Margery's face by Severing other villagers' memories that contain Margery.** This is the *recovery arc.* It is a meaningful, full-prose ~3-hour sub-thread. |
| **Refused (Listen)** | Gerrold continues to carry the orb. He returns to the Hollow in Mission 5 to *try again* — possibly with the same player, possibly more ready. The orb remains in Gerrold's hands, not the player's. | The player has chosen the *most cozy* path: presence without intervention. Vow 7 (Last Light) integrity surges. |

**Mission 1-2 implements only the immediate beat.** The follow-up arcs are stamped 🟡 PARTIAL in Focus 00 — the data structures and architectural hooks exist; the prose for Mission 6+ recovery is Scaling Reference.

### 3.4 The Mission 1 → Mission 2 cross-reference (the Echo Web's first activation)

When Gerrold's orb is first inspected at the workbench, the **Echo Web UI** (Codex 02 § 9) animates for the first time. The player sees:

```
                    DOR-001  ─── DORIS IN KITCHEN ─── GER-007
                  (just polished)                  (about to be cleansed)
```

A single line of light connects the two memories on the codex page. **This is the Mission 1-2 Memory Map first connection** — the Asset Analysis Mission1-2 § 3 Mission 2 deliverable.

The player sees, mechanically and narratively at once, that *these memories know each other.* The same kitchen on the same Sunday is in both. Doris is in Gerrold's worst memory. Gerrold is in one of Doris's best (DOR-003, locked, will eventually reveal Doris and Margery were best friends).

**This single connection is the cozy game's promise**: every memory you collect threads into others. The whole village is one woven thing.

---

## 4.0 Gerrold's Position in the Mission 2 Scene Flow

| Beat | Gerrold's blocking | What he does | Notes |
|---|---|---|---|
| Morning of Day 2 — Hollow doorway | Gerrold is on the doorstep of the Hollow at ~07:30. He has been there since 07:10. | Knocks once. Steps back. Waits. | Pickle's eyes track him through the window before the player notices. |
| First conversation — Hollow shop room | Sits in the chair the player offers. Does not take tea offered (player will not be told why; they will learn in Mission 6 that Margery used to brew his tea). | Speaks lines from § 7.1. | Eyes Animator: rarely looks at player; mostly at the handkerchief in his hands. |
| Gerrold hands over the orb | He places the cloth on the counter and unfolds it slowly. The orb is inside. He does not pick it up. | "It's in the cloth. Margery wrapped it." | Player picks the orb up. *They are picking it up out of her cloth.* |
| Player offers to walk with Gerrold to his cottage to discuss | (Optional path — see § 7.4.) Gerrold rises. They walk together down the side lane. Lantern halos overhead (Lumen). | Says ~6 lines about Margery during the walk. | This is the **Mission 2 village-walk teaching beat** — Asset Analysis Mission1-2 § 3 Mission 2 deliverable. |
| The widower's cottage interior | Gerrold sits in his usual chair. Margery's chair is empty. There is a cup of tea, set in front of her chair, *cold.* (He places one daily.) | Speaks his deepest exposition lines. The player has the orb in their lap or hand. | Eyes Animator: looks at the empty chair more than at the player. |
| The player's first moral choice (§ 5 below) | Gerrold is still in his chair. He waits for the player to speak. | He listens. He does not pressure. | Pickle, if she followed (only at 70+ Pickle Approval, very unlikely in M1-2), would be on the windowsill. Otherwise Pickle is back at the Hollow. |
| After the choice — back at the Hollow workbench | The player returns alone. Gerrold has stayed home. The Cleanse mini-game runs. | (None — Gerrold is off-screen during the actual Cleanse.) | The Cleanse mini-game music swells. The orb resists. *(See Focus 04 § 5.2.)* |
| Mission 2 outro — back at Gerrold's door | Player returns the cloth, with or without the cleansed orb. | Speaks lines § 7.8 based on outcome. **He does not cry.** | The player will. |

---

## 5.0 The Moral Choice Tree (the player's first moral choice)

Mission 2's central act is a **3-option choice plus 1 deferral**. The player chooses with the orb in their hand, sitting in Gerrold's cottage, while Gerrold sits across from them.

### 5.1 The 4 options at the choice point

| Option | Verb | Tariff (sketch) | Pickle reaction (if present; otherwise quoted later) |
|---|---|---|---|
| **A — Erase the long bit completely** | Cleanse aggressively → remove the GRIEF palette entirely. *Gerrold will remember he loved his wife. He will not remember the dying.* | Trust +5; Vow 3 -3 (over-cleansed); Vow 1 +0; Pickle approval -3. | *"He asked for less than he needed. You gave him exactly what he asked for. That is not always kindness."* |
| **B — Cleanse with care** | Cleanse precisely → remove the SHAME palette (his self-blame). *He keeps the dying. He no longer carries his own fault.* This is the canonical "good Keeper" choice. Requires playing the Cleanse mini-game and achieving Perfect or Acceptable. | Trust +4; Vow 3 +5; Vow 1 +3; Pickle approval +4. | *"You did the thing he didn't know how to ask for. Good. Tea now. You're due one."* |
| **C — Refuse to cleanse; offer to *listen* instead** | Vow 7 (Last Light) path. The player declines to take the orb at all. They sit with Gerrold for ~20 in-game minutes while he speaks the memory aloud, holding the orb himself the whole time. *He still has it at the end of Mission 2.* | Trust +6; Vow 7 +10; Vow 3 +0 (you did not Cleanse); Pickle approval +5; Cinder +3 (the Confession Booth's currency — Codex 10 § 2; this is its first earning). | *"Excellent. You did not solve him. You sat with him. That is the harder craft and the older one."* |
| **D — Defer; ask him to come back tomorrow** | The player asks Gerrold to give them a day to think. He nods. He leaves. Mission 2 ends *without* the player having committed to a choice. **In Mission 3** (Day 3 of the in-game playthrough), Gerrold returns and the same choice tree reappears, with the addition of one extra option earned through reflection. | Trust +1 (he respects the consideration); no Vow movement; Pickle approval +1; Mission 3 carries a forced re-engagement with the choice. | *"A day is a fair thing to ask for. I will be here. He will be here. Take it."* |

Each tariff fires through the Ripple Engine (Codex 08 § 4) at small radius — only Doris (Gerrold's late wife's best friend) receives a small first-degree ripple. **This is the smallest possible activation of the Ripple Engine** — exactly right for the Mission 2 tutorial of a system that will scale.

### 5.2 The choice screen's UI presentation

Per Codex 08 § 9 (Cordray Principle): **the player infers the consequence; they are not given numbers.**

The UI presents the four options as four lines of text with one-sentence consequence previews in fiction-voice:

```
A — Erase the long bit.
    "He will remember he loved her. He will not remember the dying."

B — Cleanse with care.
    "He will keep the memory, gentler. You will need a careful hand."

C — Don't take the memory. Sit with him.
    "He will carry it. You will have only been here. Sometimes that is enough."

D — Ask him to come back tomorrow.
    "He will. You can think about this for a day."
```

That is the **entire** choice surface. No stat bars. No moral-alignment indicator. Just four sentences and the orb in the player's hand.

### 5.3 Time-pressure: there is none

The choice screen has **no timer.** The player may sit with it for as long as they wish. The shop's clock ticks faintly. Pickle yawns. Gerrold breathes. *That is the design.*

The cozy contract is honored.

---

## 6.0 BoZo Reskin Recipe (Gerrold's character art spec)

Per Asset Analysis A-12: BoZo, the **Bard archetype** is reskinned as Gerrold. Specifications:

| Slot | BoZo base | Gerrold reskin |
|---|---|---|
| **Head** | Bard-Head-08 (sharper jaw, mid-60s) | Apply Colorize to silver hair. Add faint stubble (decal). Add prominent dark-under-eyes (texture overlay — the grief has been ongoing). |
| **Body** | Bard-Body-Coat | Long brown coat, knee-length. **Replace bard accessories** (lute strap) with a leather tool-belt with carpenter's chisels (re-use prop from Magic Arsenal Tier B asset, the *"ceremonial orb-cradle"* sub-pack contains a chisel mesh). |
| **Hands** | Bard-Hands-Default | Add the handkerchief prop in his hands as a held-item. The handkerchief mesh is custom (~15 polys, white linen with embroidered "M" in the corner). |
| **Pose** | Bard-Pose-Walk | Override to a stooped walk. Use ACS to apply a "carried-weight" upper-body layer over the base walk. |
| **Voice posture (Eyes Animator)** | Default | Pupil dilation 0.4 (slightly contracted — sleep-deprived). Saccade frequency reduced to 0.25/sec (his eye movement is slower than Doris's). Look-at priority: handkerchief 0.6, ground 0.3, player 0.2, empty chair 0.9 *when in the cottage scene.* |

**Total artist time: ~5 hours** (more than Doris because of the held-prop and the more idiosyncratic posture).

### 6.1 The handkerchief prop — the most important M2 prop

The handkerchief is a Mission 2 inventory item that the player can examine in the Codex. Examining it once reveals:

> *A white linen handkerchief, folded into a square. The corner is embroidered with a small "M" in dark green silk. The needlework is slightly uneven — the work of a young hand. The cloth smells faintly of cedar.*

The player can return the handkerchief to Gerrold at the end of Mission 2 with the cleansed orb. The "return the handkerchief" gesture is a one-button interaction at his door. *This is the Mission 2 cozy-coda moment.*

**Asset cost:** ~$0 (~15 polys; one texture; the embroidery is a hand-painted decal). **Player impact:** very high.

---

## 7.0 Gerrold's Full Mission 2 Yarn Spinner Dialogue

The complete Mission 2 Gerrold dialogue, ~240 Yarn lines. Drop into `Assets/_Project/Yarn/Mission02/Gerrold_Mission02.yarn`.

### 7.1 Opening — Hollow shop interior, morning

```yarn
title: Gerrold_Knocks
position: 0,0
tags: mission2, gerrold, opening
---
<<set $met_gerrold = false>>

// Player has woken from Memory Dream 1. Pickle is on the windowsill.
// A knock comes at the Hollow door.

// Pickle (internal):
Pickle: There is a man at your door. He has been there for twenty minutes.
Pickle: He is a careful sort. The careful kind hurt the worst.
Pickle: ... I would let him in.

// Player opens the door.

Gerrold: I'm sorry. Are you the new keeper?
Gerrold: I should have written first. I don't know if there's a way to do that.
Gerrold: ... My name is Gerrold Pell. I — could I come in for a moment?

-> Yes. Please.
    Gerrold: Thank you. Just for a moment.
    <<jump Gerrold_Sits>>
-> What's this about?
    Gerrold: ... It's a memory. I brought one. I'm sorry — that's what one does, isn't it? I should know how it works.
    <<jump Gerrold_Sits>>
-> (Step aside silently and gesture him in)
    Gerrold: Thank you. You are kind.
    <<jump Gerrold_Sits>>

===

title: Gerrold_Sits
position: 250,0
tags: mission2, gerrold, sitting
---

<<set $met_gerrold = true>>
<<play_animation Gerrold sit_carefully>>

Gerrold: I'd offer you tea but it's your kettle.
Gerrold: ... I'm sorry. That was a joke. It wasn't a good one.

-> Would you like tea?
    Gerrold: Aye, that — no. No, thank you. Not today.
    Gerrold: It's kind of you to ask.
    <<set $offered_gerrold_tea = true>>
-> (Sit across from him and wait)
    Gerrold: ... Thank you. For not — for not making me say it quickly.

Gerrold: I have a memory I want to be rid of.
Gerrold: Not the whole thing. Just the long bit.
Gerrold: My wife — Margery — died last winter. Eleven months last week.
Gerrold: I have the memory of the last week of her life. It is the heaviest thing I own.

Gerrold: I want to keep my wife. I do not want to keep the long bit.

-> What was the long bit?
    Gerrold: She was ill for fourteen months.
    Gerrold: The dying part — the last week — was kind, in its way. She wasn't in pain by then. She slept a lot.
    Gerrold: But I was awake the whole time. Six days awake. Watching.
    Gerrold: I cannot un-be the man who watched. But I think I do not need to remember every hour of it.
    <<jump Gerrold_Offer_Orb>>
-> Tell me about Margery first.
    <<jump Gerrold_About_Margery>>
-> (Just nod and wait)
    Gerrold: ... Thank you. I'll get to it.
    <<jump Gerrold_Offer_Orb>>

===

title: Gerrold_About_Margery
position: 500,0
tags: mission2, gerrold, margery
---

Gerrold: Margery was — quiet. She read more than she spoke.
Gerrold: She worked at the schoolhouse for thirty years. Helped Lavinia Embry with the small ones who hadn't learned to read yet.
Gerrold: She was Doris the baker's best friend. They sat in the kitchen on Sundays for — for thirty years. With bread.

<<if $met_doris == true>>
    Gerrold: You met Doris yesterday, didn't you? She'd have given you bread.
    Gerrold: ... Margery would have wanted to know you.
<<endif>>

Gerrold: She taught me how to be quiet around someone you love without making it feel like distance.
Gerrold: That is — that is the gift I lost.
Gerrold: I am very loud, now, on my own.

<<jump Gerrold_Offer_Orb>>

===

title: Gerrold_Offer_Orb
position: 750,0
tags: mission2, gerrold, offer_orb
---

<<show_orb GER-007 in_cloth=true>>

Gerrold: It's in the cloth. Margery wrapped it.
Gerrold: I think she wrapped it for this.

// Gerrold places the cloth on the counter. Does not open it. Slides it forward.

Gerrold: I'd like you to look at it.
Gerrold: If you can. If it's the right time.

-> (Pick up the orb)
    <<jump Gerrold_First_Inspection>>
-> Tell me what's in it first.
    Gerrold: The last week of her life. I described it already. I'm sorry.
    Gerrold: ... If you want to know in particular — the morning she did not wake. Doris was in the kitchen. The snow had stopped.
    Gerrold: I want the long bit gone. I want to keep the morning.
    <<jump Gerrold_First_Inspection>>
-> I need to think. Can you come back tomorrow?
    <<set $deferred_gerrold = true>>
    <<jump Gerrold_Defer>>

===

title: Gerrold_First_Inspection
position: 1000,0
tags: mission2, gerrold, inspect, echo_web_first_activation
---

<<player_picks_up_orb_from_cloth GER-007>>
<<orb_appears_cracked>>
<<echo_web_activate connection=DOR-001~GER-007>>

// The player feels the orb's weight (slightly heavier than DOR-001).
// The Echo Web UI lights for the first time, showing the connection
// between this orb and Doris's First Loaves through "Doris in kitchen."

Pickle: Ah.
Pickle: The Web has decided to speak today.
Pickle: This one and yesterday's are in the same kitchen on the same Sunday.
Pickle: That is what the line of light is saying. Look at it. It is showing off.

Gerrold: ... What is —
Gerrold: I don't see it. I'm sorry.

// The cat speaks only to the player, internally. Gerrold cannot see Pickle's
// commentary text. He does see the orb in the player's hands and the slight
// glow it casts.

-> (Show Gerrold the orb's cracks)
    Gerrold: Aye. I knew it was — uneven. I've not handled it well.
    Gerrold: I've held it too tight, perhaps. I don't know how these are meant to be handled.
    Gerrold: Some things crack in the hands. The wood, the joint — they tell you.
    Gerrold: I should have asked sooner.
    <<jump Gerrold_Discuss_Choice>>
-> (Hold the orb silently)
    Gerrold: ... You are looking at it for a long time.
    Gerrold: Take your time. It is — it is the heaviest thing I own.
    <<jump Gerrold_Discuss_Choice>>

===

title: Gerrold_Defer
position: 750,250
tags: mission2, gerrold, deferred
---

Gerrold: A day. Aye. That is fair.
Gerrold: I should not have come this morning expecting an answer.
Gerrold: I will leave the cloth here, if you don't mind. I will come back tomorrow at the same time.
Gerrold: Take care of it for the night. It is — it is what it is.

// Gerrold leaves the orb in its cloth on the counter and exits.
// Mission 2 enters its "Deferred" branch: the player has the orb overnight
// and Gerrold returns on Day 3. The choice tree re-presents with one
// extra option earned through reflection. This is Mission 3 content; for
// Mission 1-2 implementation, the Deferred branch ends Mission 2 with
// the orb still on the player's counter and Gerrold's return queued.

<<jump Mission02_Outro_Deferred>>

===

title: Gerrold_Discuss_Choice
position: 1250,0
tags: mission2, gerrold, the_choice
---

Gerrold: Do you want to talk it through? Or do you want me to leave you to it?
Gerrold: I would not mind sitting here. I would not mind walking with you to my house.
Gerrold: I would not mind, also, going home and leaving the cloth.

-> Let's walk to your cottage. I'd like to see where she lived.
    <<set $walked_to_gerrold_house = true>>
    <<jump Gerrold_Walk_To_Cottage>>
-> I'd like to do it here. Stay if you want.
    <<set $worked_at_hollow = true>>
    <<jump Gerrold_At_Hollow_Choice>>
-> Go home. I'll come find you when it's done.
    <<set $worked_alone = true>>
    Gerrold: Aye. I'd — I'd appreciate that. Thank you.
    <<jump Mission2_PreCleanse_Player_Alone>>

===

title: Gerrold_Walk_To_Cottage
position: 1500,0
tags: mission2, gerrold, walk, village_walk_beat
---

// The player and Gerrold walk down the side lane. Lumen lantern halos overhead.
// Light rain via Stylized Weather System (optional setting).
// Soft Idris-style fiddle drifts from the inn in the distance — this is a
// 30-second composer cue, the "Margery walking music." Not voiced by Idris;
// Margery used to hum this same tune.

Gerrold: It's been a fair autumn. Cold mornings, but bright.
Gerrold: Margery liked the autumns best. She said the trees gave the village manners.
Gerrold: That is the kind of thing she would say. I never knew how to answer her, exactly.
Gerrold: I just smiled. She did not mind.

Gerrold: She taught me how to be quiet around someone you love without making it feel like distance.
Gerrold: I am very loud, now, on my own. I keep wanting to say things that I do not have to say to anyone.
Gerrold: This is — this is part of why I came to the Hollow. I cannot keep being this loud.

Gerrold: Here. This is the door. Watch the step — the wood gave last winter and I haven't planed it true yet.

<<jump Gerrold_Cottage_Interior>>

===

title: Gerrold_Cottage_Interior
position: 1750,0
tags: mission2, gerrold, cottage
---

// Interior reveal: small main room. Two chairs by a fireplace. A cup of tea
// in front of the second chair, cold. A book on the second chair's arm,
// open to a page Margery never finished. A small table set for two —
// one place setting fresh, one place setting unused but laid out.

// The player will notice. Gerrold will not explain.

Gerrold: Have a seat. Either chair.

// (Player can choose: sit in Gerrold's chair (left) or Margery's chair (right).)

-> (Sit in the chair on the left — Gerrold's chair)
    Gerrold: Aye. That's the right one for you.
    Gerrold: I'll take Margery's, today.
    Gerrold: She wouldn't mind. She wouldn't have minded most things.
    <<set $sat_in_gerrold_chair = true>>
-> (Sit in the chair on the right — Margery's chair)
    Gerrold: ...
    Gerrold: ... Aye. That's all right. I should have said which was mine.
    Gerrold: I should — I should say it more, perhaps.
    <<set $sat_in_margery_chair = true>>
    // The Pickle commentary fires only if the player is at Pickle Approval 50+:
    <<if $pickle_approval >= 50>>
        Pickle: You sat in the dead wife's chair.
        Pickle: He is too polite to tell you. But he is grateful.
        Pickle: ... I would not have done it. But you are not a cat.
    <<endif>>
-> (Stand by the fireplace)
    Gerrold: As you like. The fire's still warm — Doris stopped by yesterday and lit it.

Gerrold: Well, then.
Gerrold: I've said what I've come to say.
Gerrold: I'd like to give you the choice.

<<jump Gerrold_The_Choice>>

===

title: Gerrold_The_Choice
position: 2000,0
tags: mission2, gerrold, choice, first_moral_choice
---

<<show_choice_card_with_orb_in_hand>>

// The choice card UI appears. Four options. No timer. Pickle is on
// Gerrold's window. The fire crackles. The player has the orb in their
// inventory and the choice card is asking them what to do with it.

-> A — Erase the long bit.
    <<set $gerrold_choice = "erase">>
    <<jump Gerrold_Choice_Erase>>
-> B — Cleanse with care.
    <<set $gerrold_choice = "cleanse">>
    <<jump Gerrold_Choice_Cleanse>>
-> C — Don't take it. Sit with him.
    <<set $gerrold_choice = "listen">>
    <<jump Gerrold_Choice_Listen>>
-> D — Ask him to come back tomorrow.
    <<set $gerrold_choice = "defer">>
    <<jump Gerrold_Choice_Defer>>

===

title: Gerrold_Choice_Erase
position: 2250,-200
tags: mission2, gerrold, choice_erase
---

Gerrold: ... Aye. The long bit gone.
Gerrold: All of it.
Gerrold: I — I want to be certain that's what I'm asking for.

// One last confirmation. This is the cozy game's gentle moral safeguard:
// the heaviest option requires a second "yes."

-> Yes, all of it. Erase the long bit.
    Gerrold: Aye. Thank you.
    Gerrold: I'll be here.
    <<jump Mission2_Cleanse_Aggressive>>
-> Wait — let me think a moment more.
    Gerrold: Of course. Take your time.
    <<jump Gerrold_The_Choice>>

===

title: Gerrold_Choice_Cleanse
position: 2250,0
tags: mission2, gerrold, choice_cleanse
---

Gerrold: Aye.
Gerrold: ... With care, then. Whatever you can do.
Gerrold: I trust you. I — I don't know you, but I trust you.

// No re-confirmation needed for the canonical "careful" path. This is
// the path the game most wants the player to take, and the dialogue
// rewards it with quiet trust.

<<set $vow_3_whole += 1>>
<<jump Mission2_Cleanse_Careful>>

===

title: Gerrold_Choice_Listen
position: 2250,200
tags: mission2, gerrold, choice_listen
---

Gerrold: ... I don't —
Gerrold: I don't understand.
Gerrold: You — you don't want to take it?

-> I want to listen. Tell me about it. Hold it the whole time. I won't take it.
    Gerrold: ...
    Gerrold: ... All right.
    Gerrold: All right. ... All right.
    <<set $vow_7_last_light += 5>>
    <<jump Mission2_Listen_Scene>>
-> (Reach out and gently place his hands around the orb)
    Gerrold: Aye.
    Gerrold: ... Aye.
    <<set $vow_7_last_light += 7>>
    <<jump Mission2_Listen_Scene>>

===

title: Gerrold_Choice_Defer
position: 2250,400
tags: mission2, gerrold, choice_defer
---

Gerrold: Aye.
Gerrold: A day, then.
Gerrold: I will come back. I will not be impatient.

// Gerrold takes the orb back into its cloth and rewraps it. Mission 2
// enters the Deferred branch. The player returns to the Hollow alone.
// Mission 2's day cycle ends. Memory Dream 2 plays in a "deferred"
// variant — the player dreams of Margery's chair, empty, with no choice yet made.

<<jump Mission2_Outro_Deferred>>

===

// (Cleanse mini-game outcomes — Mission2_Cleanse_Aggressive, Mission2_Cleanse_Careful,
// Mission2_Listen_Scene — are specified in Focus 04 (mini-game spec) and Focus 06
// (the choice's full outcome handling). For brevity, those branches are referenced here
// and detailed in those documents.)

===

title: Gerrold_Mission2_Outro_Return
position: 3000,0
tags: mission2, gerrold, outro
---
// After the Cleanse / Listen / Defer is complete, the player returns to
// Gerrold's cottage door with the orb (or empty-handed in Listen path).

<<if $gerrold_choice == "erase">>
    <<if $cleanse_quality == "perfect" || $cleanse_quality == "acceptable">>
        // The aggressive erase succeeded — Margery's last week is gone.
        Gerrold: ... Oh.
        Gerrold: ... Aye.
        Gerrold: I can feel that it's lighter.
        Gerrold: I can still see her face. I cannot — I cannot quite see the last week.
        Gerrold: ...
        Gerrold: Thank you. I think — I think this is what I asked for.
        Gerrold: I am not certain it is what I needed.
        Gerrold: I will go inside now. I will see what the morning brings.
    <<else>>
        // Crossed Core during aggressive erase — disaster outcome.
        Gerrold: ... Oh.
        Gerrold: ... I can't —
        Gerrold: I can't see her clearly.
        Gerrold: That wasn't —
        Gerrold: That wasn't —
        Gerrold: It isn't your fault. It is — it is the cloth and the cold and the long winter.
        Gerrold: I will go home. Thank you for trying.
        // (The Mission 6 recovery arc seeds here.)
    <<endif>>
<<elseif $gerrold_choice == "cleanse">>
    <<if $cleanse_quality == "perfect">>
        Gerrold: ... Aye.
        Gerrold: I can feel it. The blame is out of me. The dying is still there. But I can — I can carry it now, I think.
        Gerrold: Margery is — Margery is whole.
        Gerrold: ... Thank you. I do not know what to say. I will go in and read for a bit.
        Gerrold: ... Come back when you can. Doris's bread is on Sundays. We could —
        Gerrold: We could perhaps share one.
    <<elseif $cleanse_quality == "acceptable">>
        Gerrold: ... Aye.
        Gerrold: It is lighter. Not as light as I'd hoped, but — I do not think I should be as light as I'd hoped, perhaps.
        Gerrold: Margery is here. I am here. The long bit is — it is part of me, still, but it is no longer the only part.
        Gerrold: Thank you. I will see you in the village square next week.
    <<else>>
        // Crossed Core during careful cleanse — minor disaster.
        Gerrold: ... Mmm.
        Gerrold: I cannot quite — I cannot quite see all of her.
        Gerrold: It is — it is not what I asked for. But it is not your fault.
        Gerrold: Thank you for the try.
        // (Mission 6 recovery arc seeds here.)
    <<endif>>
<<elseif $gerrold_choice == "listen">>
    Gerrold: I told you the whole memory.
    Gerrold: I said it aloud for the first time in eleven months.
    Gerrold: I did not know that I would still have it once I'd said it.
    Gerrold: ... I still have it. It is — it is still heavy. But it is — it is no longer alone with me.
    Gerrold: Thank you for sitting. That was — that was a kindness I had not known how to ask for.
    Gerrold: Come for tea on Sunday. Doris brings the bread.
<<endif>>

// Player returns the handkerchief.
-> (Hand back the handkerchief)
    Gerrold: Aye. Thank you.
    Gerrold: Margery folded that for me. I'd been losing it for weeks.
    Gerrold: ... Take care of yourself, keeper.

<<jump Mission02_Outro>>
```

**Total Yarn lines: ~270.** This is the *complete* Gerrold script for Mission 2 across all four choice branches.

### 7.2 Branch coverage discipline

Every branch above is **fully written.** No "if (defer) → handle later" placeholder. The Deferred branch's outro is implemented; it just transitions to the Mission 3 re-introduction.

The Crossed-Core outcome under both Erase and Cleanse paths is **also fully written.** This is the Pell Doyne discipline (Codex 06 § 10 — Doyne Test): a player who fails a tutorial-tier mini-game must still receive a fully-authored consequence scene, not a generic fail-state.

---

## 8.0 Voice Acting Notes

Gerrold's lines total **~6 minutes of recorded audio** across all branches. Recording session: ~3 hours of an actor's time.

**Recommended casting:** a 60–70-year-old male actor with a quiet baritone, slight working-class accent. Refer the casting director to **Bill Paterson's quieter roles** or **Brendan Gleeson at his most contained** as the reference.

Pickle's Gerrold-related quotes (~6 across Mission 2): same actress as before. Pickle's tone toward Gerrold is **slightly softer than her usual** — Pickle does not joke much around men in mourning.

---

## 9.0 Mission 2 Consequence Summary (what carries forward)

| Dimension | Default | After Mission 2 (per choice) |
|---|---|---|
| `trust_gerrold` | 0 | Erase: +5 / Cleanse: +4 / Listen: +6 / Defer: +1 |
| `memory_integrity[GER-007]` | 100 | Erase (perfect): 50 / Cleanse (perfect): 95 / Listen: 100 / Defer: 100 |
| `public_standing` | +2 (from M1) | +5 typical |
| `vow_1_consent` | 51 | unchanged in Cleanse/Listen; -2 in aggressive Erase |
| `vow_3_whole` | 53 | +5 in careful Cleanse; -3 in aggressive Erase; 0 in Listen |
| `vow_5_honest_coin` | 52 | unchanged (no haggling in M2) |
| `vow_7_last_light` | 50 | +10 if Listen path |
| `pickle_approval` | 52 | -3 (Erase) / +4 (Cleanse) / +5 (Listen) / +1 (Defer) |
| `cinder` | 0 | +3 only if Listen path |
| **`first_moral_choice_made`** | flag, false | true (locked) |

The `first_moral_choice_made` flag is what the rest of the game checks before unlocking certain narrative beats. It is the player's **canonical moral debut.**

---

## 10.0 Closing

Gerrold Pell in Mission 2 is **the cozy game's first moment of weight.** Doris in Mission 1 taught the player they were a Keeper. Gerrold teaches them what that means.

The widower does not cry. The handkerchief is white. Margery is in the kitchen on the morning the snow stopped. The player will choose to erase her, to keep her, or to sit with him while he tells her aloud.

Whichever they choose, **the game will remember.** That is the promise the rest of the game keeps.

— *Inara Vellis*
*Mission 1-2 Focus 02 · v1.0*

> Next: `03_SCENES_LANE_HOLLOW_GARDEN_COTTAGE.md` — the four scenes blocked, lit, propped, and audio-cued for the build.
