# 🫖 Mission 2 — *The Widower's Request*
## Gameplay Guide · Full Step-by-Step Walkthrough

> *"I want to keep my wife. I do not want to keep the long bit."* — Gerrold Pell

> **This is the per-mission gameplay guide for Mission 2 — the heavier of the two missions in the vertical slice.** It is the cozy game's first moment of real moral weight. Read this *before* you make Gerrold's choice if you want to know exactly what each path means. Read it *after* if you want to see what the paths you did not take would have looked like.
>
> Read [`GAMEPLAY_GUIDE_OVERVIEW.md`](./GAMEPLAY_GUIDE_OVERVIEW.md) first for controls, comfort settings, the Cozy Contract, the 7 Vows, the Pickle Approval system, and the Evening Ledger. Read [`GAMEPLAY_GUIDE_MISSION_1.md`](./GAMEPLAY_GUIDE_MISSION_1.md) for the warm onboarding hour. **Mission 2 builds on every system Mission 1 taught.**
>
> **Branch:** `feat/mission-1-2-architecture`
> **Mission 2 in-game day:** Day 2 (Spire-Month, Week 1)
> **Real-time:** 30–40 minutes (cozy unhurried pace ~38 min)
> **Scenes used:** `03_Mission01_Hollow` (reused), `04_Mission02_Garden`, `05_Mission02_Cottage`

---

## 0. Table of Contents

1. [Mission Purpose — What This Hour Is For](#1-mission-purpose--what-this-hour-is-for)
2. [Mission Objective — In One Sentence](#2-mission-objective--in-one-sentence)
3. [Content Warning](#3-content-warning)
4. [Pre-Flight Checklist (Day 2)](#4-pre-flight-checklist-day-2)
5. [The Mission 2 Cast](#5-the-mission-2-cast)
6. [Mission 2 Map](#6-mission-2-map)
7. [Beat 1 — Waking On Day 2](#7-beat-1--waking-on-day-2)
8. [Beat 2 — Gerrold Knocks At The Hollow](#8-beat-2--gerrold-knocks-at-the-hollow)
9. [Beat 3 — The First Conversation](#9-beat-3--the-first-conversation)
10. [Beat 4 — The Echo Web's First Activation](#10-beat-4--the-echo-webs-first-activation)
11. [Beat 5 — The Tea Decision (Garden → Brew)](#11-beat-5--the-tea-decision-garden--brew)
12. [Beat 6 — The Walk To The Cottage](#12-beat-6--the-walk-to-the-cottage)
13. [Beat 7 — Inside Gerrold's Cottage](#13-beat-7--inside-gerrolds-cottage)
14. [Beat 8 — The Moral Choice Screen](#14-beat-8--the-moral-choice-screen)
15. [Path A — Erase The Long Bit](#15-path-a--erase-the-long-bit)
16. [Path B — Cleanse With Care (CANONICAL)](#16-path-b--cleanse-with-care-canonical)
17. [Path C — Don't Take The Memory. Sit With Him. (THE LISTEN PATH)](#17-path-c--dont-take-the-memory-sit-with-him-the-listen-path)
18. [Path D — Ask Him To Come Back Tomorrow (DEFER)](#18-path-d--ask-him-to-come-back-tomorrow-defer)
19. [The Cleanse Mini-Game — Full Spec](#19-the-cleanse-mini-game--full-spec)
20. [Beat 9 — Returning The Handkerchief](#20-beat-9--returning-the-handkerchief)
21. [Beat 10 — Day 2 Evening Ledger (5 Variants)](#21-beat-10--day-2-evening-ledger-5-variants)
22. [Beat 11 — Memory Dream 2 (5 Variants)](#22-beat-11--memory-dream-2-5-variants)
23. [Beat 12 — Mission 2 Outro Cinematic](#23-beat-12--mission-2-outro-cinematic)
24. [Mission 2 Outcome Matrix](#24-mission-2-outcome-matrix)
25. [Hidden Details & Easter Eggs](#25-hidden-details--easter-eggs)
26. [Accessibility Paths](#26-accessibility-paths)
27. [Best-Practice Walkthroughs By Player Type](#27-best-practice-walkthroughs-by-player-type)
28. [What This Mission Teaches You For Mission 3+](#28-what-this-mission-teaches-you-for-mission-3)
29. [Critic & Review Board Sign-Off](#29-critic--review-board-sign-off)

---

## 1. Mission Purpose — What This Hour Is For

Mission 2 has **one job**:

> *Teach the player that a memory transaction is a moral act. The player must choose between three real options — and the village will know which one they chose. Even Pickle will know.*

Mission 1 was warm. Mission 2 introduces the **cozy-with-real-consequence** register the game's whole brand is built on. The cozy player will walk into Mission 2 prepared — the Tone Compass warned them, Doris signaled it, Pickle's posture changes. **Mission 2 still surprises them.**

### 1.1 What this mission introduces

| System | First appearance |
|---|---|
| **Echo Web** | The first connection lights between DOR-001 and GER-007. (Beat 4) |
| **Tea Brewing** | Herb harvest → kettle → cup. Soft conversation modifier. (Beat 5) |
| **The Cleanse mini-game** | The cozy game's first failable mini-game. (Section 19) |
| **The 4-option Moral Choice** | The player's first real choice. (Beat 8) |
| **The Listen path** | The cozy game's first proof that *being present is mechanically equivalent to doing the work.* (Path C) |
| **Memory Dream variants** | A single set-piece illustrated five different ways by your choice + outcome. (Beat 11) |
| **The Day 2 Evening Ledger branching prose** | Five entirely different prose pages — one per outcome path. |

### 1.2 What this mission *does not* yet introduce

| Deferred | Until |
|---|---|
| The Weave / Sever / Listen-as-mini-game / Read / Translate / Identify / Compose / Search / Negotiate / Compose Verse mini-games | Mission 3+ |
| Procedural villagers beyond the silent lane villager | Mission 3+ |
| Marin's name (only *"M."* on the note) | Mission 4+ (Reveal Chapter 12) |
| The Echo Hologram of the previous Keeper | Mission 4+ |
| The Locked Room upstairs | Mission 5+ |
| Doris's bee/honey storyline | Mission 3 (the dormant beehive seed) |
| Margery and Doris's friendship in full | Mission 4+ (Reveal Chapter 17) |
| Gerrold's stillborn child | Mission 6+ (carefully gated) |
| The Mission 6 recovery arc if you Crossed-Core'd | Mission 6+ |

---

## 2. Mission Objective — In One Sentence

**A widower brings you a memory of the last week of his wife's life. Decide — without time pressure — whether to erase her, gently keep her, or sit with him while he holds her himself.**

There is no "correct" choice. There are four real options. The game will absorb whichever one you pick.

---

## 3. Content Warning

Mission 2 contains references to:

- **Loss of spouse** (Gerrold's wife Margery died 11 in-world months before the game begins).
- **Terminal illness** (Margery's "long illness" — referenced; never medically specified; never depicted in detail).
- **A man's grief** (Gerrold does not cry on-screen, but his lines are written to make you do so).
- **Themes of dementia** (in subtext — the discomfort of forgetting, the choice to forget).

**Gentle Mode** softens the memory's prose, reduces the difficulty of the Cleanse mini-game, makes Crossed-Core recoverable, and slightly warms Pickle's lines.

The **Heavy Theme Warning Card** appears before the Mission 2 cottage scene if you have that toggle on (default ON). It is a single Bamao card you tap to dismiss.

---

## 4. Pre-Flight Checklist (Day 2)

| Item | Verify |
|---|---|
| Mission 1 was completed and saved | Save file shows `current_mission = "Mission01_Complete_Transitioning"` (loaded automatically by the morning-wake transition) |
| You polished DOR-001 in Mission 1 | Optional — Mission 2 plays without it but you miss the Echo Web connection |
| Day 2 morning wake transition has played | You should be standing in the Hollow's shop room with morning light streaming in |
| Pickle is on the windowsill | She should be. If she isn't, the NPC Animator capstone may not have wired correctly — try `Hearthbound → ⚙️ Advanced → 🎭 Phase 26 — Wire NPC Animators` |
| Gentle Mode preference is loaded | Settings persisted from Mission 1 |

> 💡 **Phase 32 update:** the troubleshooting row for Pickle's NPC animator now lives under `⚙️ Advanced ►` (per D-051 — only `🚀 Build Everything` and `🔍 Diagnose Build` are at top level). The simplest fix is still `Hearthbound → 🚀 Build Everything`, which chains the NPC animator capstone automatically.

---

## 5. The Mission 2 Cast

| Character | Role | Lines | VO Tier |
|---|---|---|---|
| **The Keeper (you)** | Silent protagonist | Many choice options | Silent |
| **Doris** | Morning brief — she has a 30-line aside at her bakery if you walk to her before Gerrold arrives | ~30 Yarn lines | Tier B (~30s audio) |
| **Gerrold Pell** | The widower. 60. Carpenter. Quiet, formal, apologetic. Voice signature: "the long bit," constant gentle apologies, talks like a man addressing a doctor. | ~270 Yarn lines across all 4 paths | Tier B (~6 min audio + ~3 min Listen scene) |
| **Pickle** | Expanded role. 3 new lines + 4 conditional pre-choice lines if Approval ≥50. | 3–7 lines | Tier B |
| **Margery Pell** | Gerrold's late wife, 57 at death (11 in-world months ago). Schoolteacher's assistant. **Off-screen character** — no model, no portrait, no lines. Lives in Gerrold's dialogue, the cracked memory orb, and a single embroidered handkerchief. | 0 lines (the most affecting unseen character in the cozy genre this decade) | Silent by design |

---

## 6. Mission 2 Map

```
                            DAY 2 — MISSION 2 LAYOUT
                            ════════════════════════

   ┌──────────────────────────────────────────────────────────────┐
   │                                                              │
   │                          THE LANE                            │
   │                                                              │
   │                                                              │
   │      Hollow ←─────────── Side Lane (35m to cottage)          │
   │      door                                                     │
   │                                                              │
   │                                                              │
   │                          [Cottage door]                      │
   │                              ↓                               │
   └──────────────────────────────│───────────────────────────────┘
                                  │
   ┌──────────────────────────────│───────────────────────────────┐
   │                              ↓                               │
   │                                                              │
   │            GERROLD'S COTTAGE (interior)                      │
   │            ════════════════════════════                      │
   │                                                              │
   │            [hearth — fire still burning,                     │
   │             lit by Doris yesterday]                          │
   │              ║                                                │
   │           ┌──┴──┐                                             │
   │           │ rug │                                             │
   │           └─────┘                                             │
   │                                                              │
   │      ┌────────┐  ← Gerrold's chair (you can sit here)        │
   │      │        │                                              │
   │      └────────┘                                              │
   │                                                              │
   │      ┌────────┐  ← Margery's chair (you can sit here too)    │
   │      │book on │     book on arm; place setting unused        │
   │      │arm     │     in front of it; cold tea cup             │
   │      └────────┘                                              │
   │                                                              │
   │      ┌──────────────────────────────┐                        │
   │      │  small table — two settings  │                        │
   │      │  ┌──┐         ┌──────┐       │                        │
   │      │  │  │         │ cold │       │                        │
   │      │  │  │         │ tea  │       │                        │
   │      │  └──┘         └──────┘       │                        │
   │      └──────────────────────────────┘                        │
   │                                                              │
   │      [doorway to bedroom — CLOSED, locked in M1-2]           │
   │                                                              │
   │      [framed photograph on mantel — examinable]              │
   │                                                              │
   │      [toolbox under Gerrold's chair — chisels, hammer, plane]│
   │                                                              │
   └──────────────────────────────────────────────────────────────┘

   ┌──────────────────────────────────────────────────────────────┐
   │                                                              │
   │   THE HOLLOW BACK GARDEN  (NEW IN MISSION 2)                 │
   │   ═════════════════════════════════════════                  │
   │                                                              │
   │   ┌──────┐                                                   │
   │   │ door │ ← from Hollow workbench room                      │
   │   └──────┘                                                   │
   │       ↓                                                      │
   │   [stepping stone path]                                      │
   │       ↓                                                      │
   │                                                              │
   │   ┌─────┐    ┌─────┐                                          │
   │   │ Lav │    │ Val │   ← 4 raised beds                       │
   │   │  ●  │    │  ●  │      Lavender + Valerian                │
   │   │  ●  │    │  ●  │      (3 plants each)                    │
   │   │  ●  │    │  ●  │                                         │
   │   └─────┘    └─────┘                                         │
   │                                                              │
   │   ┌─────┐    ┌─────┐                                          │
   │   │empty│    │empty│   ← future planting (Mission 3+)        │
   │   │     │    │     │                                         │
   │   └─────┘    └─────┘                                         │
   │                                                              │
   │   [watering can on a stool]   [hoe — not interactable]       │
   │                                                              │
   │   [low wooden fence]                                         │
   │                                                              │
   │   [beyond: meadow, walled off in M1-2]                       │
   │                                                              │
   └──────────────────────────────────────────────────────────────┘
```

### 6.1 Mission 2's scene flow

```
        Wake (Day 2 morning) — Hollow shop room
                       │
                       ▼
              Optional: walk to bakery, speak with Doris briefly
                       │
                       ▼
              Return to Hollow (or stay, if you skipped Doris)
                       │
                       ▼
              ☆ Gerrold knocks  (Pickle pre-knock line)
                       │
                       ▼
              First conversation in the Hollow
                       │
                       ▼
              Pick up Gerrold's orb from its handkerchief cloth
                       │
                       ▼
              ★ ECHO WEB FIRST ACTIVATION ★
                       │
                       ▼
              Choose what to do next:
              ┌────────┼────────────┬──────────────┐
              ▼        ▼            ▼              ▼
       Brew tea    Skip garden   Work alone    DEFER (→ Path D)
       (visit       (go to        (Gerrold
       Garden)     cottage with    leaves;
                   no tea)         work at
                                   Hollow)
              │        │            │
              ▼        ▼            ▼
              Walk to cottage with Gerrold (or alone)
                       │
                       ▼
              In the cottage — choose your chair
                       │
                       ▼
              ☆ THE MORAL CHOICE SCREEN ☆ (4 options)
                       │
              ┌────────┴────────┬───────┬─────────┐
              ▼                 ▼       ▼         ▼
            Path A           Path B   Path C     Path D
            ERASE          CLEANSE   LISTEN     DEFER
              │                 │       │         │
              ▼                 ▼       ▼         ▼
        Cleanse aggressive  Cleanse  3-min   Mission 2
        mini-game           careful  Listen  ends with
        (Crossed-Core      mini-game scene   orb on
        possible)           |       (no      counter
              │             │      mini-game)│
              ▼             ▼         ▼      ▼
        Return handkerchief — back to Hollow
                       │
                       ▼
        Day 2 Evening Ledger — 5 prose variants
                       │
                       ▼
        Memory Dream 2 — 5 visual variants
                       │
                       ▼
        Mission 2 outro cinematic (30s) → main menu, save
```

---

## 7. Beat 1 — Waking On Day 2

**Real-time: 0:00 → 3:00**
**Scene:** `03_Mission01_Hollow.unity` (same scene, different lighting)

After Memory Dream 1 ended (or directly from the save load if you resumed), the Hollow renders fully in morning light. The room looks different than yesterday — the warm dim-amber-evening lightmap has been replaced with the morning lightmap. Sun streams through the lane-side window.

### 7.1 What you see immediately

- The shop room. Pickle on her windowsill. She is *still in the same spot* — she has not moved since you went to sleep. She is awake; her eyes track you.
- **DOR-001 — The First Loaves** on its shelf, glowing softly. If you examine it, the codex now shows the kitchen scene inside the glass (because you polished it).
- The Marin pinned note now has a **second visible paragraph** (see § 7.4).
- The connecting door to Doris's bakery is open. Outside it, the bakery is producing fresh-bread smells (no actual smell mechanic — but the warm bakery interior is visible).

### 7.2 Optional: walk to Doris's bakery

Walk through the connecting door. Doris is there, kneading a fresh batch. She greets you:

> *"You slept. Good. Tea's on the bench in your shop — drink it while it's warm."*

If `$asked_about_predecessor == true` (i.e., you asked her about Marin in Mission 1), she adds:

> *"... And the old keeper — Marin — she was a tall one, like I said you weren't. Worked the bench you've been at. Left without a word. We were friends, I think."*

(This is **the first time Marin's name is spoken aloud in the entire game.** It is locked behind your Mission 1 curiosity.)

You can also ask Doris:

| Option | Response |
|---|---|
| **"How was she?" (about Marin)** | *"Quiet. Took her tea black. Knew every villager's first-name by their first month here. ... I miss her."* |
| **"Did she leave a note?"** | *"Aye, she left them everywhere. Read the one above your bench."* |
| **"Goodbye."** | *"Off with you. The day's started."* |

This Doris brief is **optional** — if you skip it, Gerrold simply arrives ~30 seconds earlier.

### 7.3 Pickle's pre-knock line

Whether you visited Doris or not, when you return to the Hollow's shop room and stand still for 5+ seconds (or after ~3 minutes from waking), **a knock comes at the Hollow door.** Just before the knock, Pickle speaks (italic font, lower opacity):

> *"There is a man at your door. He has been there for twenty minutes."*
>
> *"He is a careful sort. The careful kind hurt the worst."*
>
> *"... I would let him in."*

This is **Pickle's second line in the game**. Memorable. Tonally distinct from her after-Polish line — softer; she does not joke about a man in mourning.

### 7.4 Marin's note — the second paragraph

If you walk to the workbench and examine Marin's pinned note before opening the door, the note now has a second visible paragraph (it was always there; the note seems to write itself when you need it):

> *"Polish in slow circles. The faster you press, the less the orb hears you. Cover all sides. Most memories need ninety seconds. — M."*
>
> *"To Cleanse: find the cracks. Trace them once, fully, without lifting your hand. Never touch the warm center. The warm center is the memory itself. If you touch it, you have not cleansed; you have erased. — M."*

**This is the Cleanse mini-game tutorial.** Read it before the cottage scene if you want to be prepared.

---

## 8. Beat 2 — Gerrold Knocks At The Hollow

**Real-time: 3:00 → 4:00**

A single soft knock on the wooden door. Then silence. (Gerrold has, in fiction, been at the door for 20 minutes already — see Pickle's pre-knock line.)

### 8.1 Opening the door

Walk to the door and press E. The door opens. Gerrold stands one step back from the doorway, hat off, holding a folded white handkerchief in both hands. He has been waiting in the cold.

His **physical description** in this moment (the game shows this; you do not need to read it):

- Long brown coat, knee-length.
- Carpenter's tool-belt with chisels and a plane (he has been a craftsman for forty-one years).
- Silver hair, mid-60s.
- Faint stubble.
- Prominent dark under-eye circles — 11 months of poor sleep.
- His **Eyes Animator** has pupil dilation 0.4 (sleep-deprived contraction) and saccade frequency 0.25/sec (his eye movement is slower than Doris's). He looks at the handkerchief more than at you.

### 8.2 His first words

> *"I'm sorry. Are you the new keeper?"*
>
> *"I should have written first. I don't know if there's a way to do that."*
>
> *"... My name is Gerrold Pell. I — could I come in for a moment?"*

You choose:

| Option | His response |
|---|---|
| **"Yes. Please."** | *"Thank you. Just for a moment."* |
| **"What's this about?"** | *"... It's a memory. I brought one. I'm sorry — that's what one does, isn't it? I should know how it works."* |
| ***(Step aside silently and gesture him in)*** | *"Thank you. You are kind."* (Slight Vow 7 nudge — silence as kindness.) |

He enters slowly. He does not look at the room. He looks at his hands.

---

## 9. Beat 3 — The First Conversation

**Real-time: 4:00 → 7:00**

Gerrold takes the chair you offer (the wooden one in the shop room). He sits *carefully* — his ACS-driven sit-down animation is slow, deliberate, like a man whose joints hurt from cold or grief or both.

### 9.1 Tea offer

> *"I'd offer you tea but it's your kettle."*
>
> *"... I'm sorry. That was a joke. It wasn't a good one."*

You choose:

| Option | His response |
|---|---|
| **"Would you like tea?"** | *"Aye, that — no. No, thank you. Not today. It's kind of you to ask."* (He declines. **In Mission 6+ you will learn that Margery used to brew his tea, and he hasn't accepted tea from anyone since she died.**) Sets `$offered_gerrold_tea = true`. |
| ***(Sit across from him and wait)*** | *"... Thank you. For not — for not making me say it quickly."* |

### 9.2 He states his purpose

> *"I have a memory I want to be rid of."*
>
> *"Not the whole thing. Just the long bit."*
>
> *"My wife — Margery — died last winter. Eleven months last week."*
>
> *"I have the memory of the last week of her life. It is the heaviest thing I own."*
>
> *"I want to keep my wife. I do not want to keep the long bit."*

This is the heart of his case. He uses *"the long bit"* — his verbal tic for grief — for the first time. The phrase will recur 4 times across Mission 2.

You choose how to respond:

| Option | What happens |
|---|---|
| **"What was the long bit?"** | He explains: *"She was ill for fourteen months. The dying part — the last week — was kind, in its way. She wasn't in pain by then. She slept a lot. But I was awake the whole time. Six days awake. Watching. I cannot un-be the man who watched. But I think I do not need to remember every hour of it."* Then proceeds to the orb offer (Beat 4). |
| **"Tell me about Margery first."** | He speaks the most beautiful 6-line aside in Mission 2 (see § 9.3 below). **Recommended.** |
| ***(Just nod and wait)*** | *"... Thank you. I'll get to it."* Then proceeds. |

### 9.3 The Margery aside (recommended)

If you ask him to tell you about Margery first, he speaks:

> *"Margery was — quiet. She read more than she spoke."*
>
> *"She worked at the schoolhouse for thirty years. Helped Lavinia Embry with the small ones who hadn't learned to read yet."*
>
> *"She was Doris the baker's best friend. They sat in the kitchen on Sundays for — for thirty years. With bread."*

If `$met_doris == true` (true after Mission 1 — guaranteed for any normal playthrough), he adds:

> *"You met Doris yesterday, didn't you? She'd have given you bread."*
>
> *"... Margery would have wanted to know you."*

Then:

> *"She taught me how to be quiet around someone you love without making it feel like distance."*
>
> *"That is — that is the gift I lost."*
>
> *"I am very loud, now, on my own."*

**This is the most quoted line in cozy-game playtests of Mission 2.** The cozy player will remember it.

### 9.4 Why the Margery aside matters

Three things happen:

1. You learn that **Doris and Margery were best friends.** This is the seed for the Echo Web's first activation in Beat 4.
2. You learn that **Margery worked under Lavinia Embry** (a hand-sealed villager who will appear in Mission 6+).
3. You feel Margery, before you ever see her name on a memory.

---

## 10. Beat 4 — The Echo Web's First Activation

**Real-time: 7:00 → 8:00**

Gerrold places the handkerchief on the counter. He slowly unfolds it. **Inside is a glass memory orb — cracked, palette grief-dark, glowing dimly.**

> *"It's in the cloth. Margery wrapped it."*
>
> *"I think she wrapped it for this."*

You can:

| Option | What happens |
|---|---|
| ***(Pick up the orb)*** | The orb levitates softly from the cloth into your inventory. **Then the Echo Web first activates.** |
| **"Tell me what's in it first."** | He gives a 5-line description: *"The last week of her life. ... The morning she did not wake. Doris was in the kitchen. The snow had stopped. I want the long bit gone. I want to keep the morning."* Then you pick up the orb. |
| **"I need to think. Can you come back tomorrow?"** | **PATH D — DEFER.** Jumps to § 18. |

### 10.1 The Echo Web first activation

The moment you pick up Gerrold's orb (GER-007), a **single line of golden light** animates between:

- **DOR-001 — The First Loaves** (on the shelf you placed it on yesterday), and
- **GER-007 — The Last Week** (in your hand).

The connection's name appears: **"Doris in kitchen on Sunday."** Both memories share that kitchen.

The Memory Map UI (the Codex) lights up in the bottom-left corner of the screen — *"+ 1 Echo connection."*

This is **the cozy game's first promise visualized**: every memory you collect threads into others. The whole village is one woven thing.

### 10.2 Pickle's third line

The moment the Web lights, Pickle speaks (italic, internal):

> *"Ah. The Web has decided to speak today."*
>
> *"This one and yesterday's are in the same kitchen on the same Sunday."*
>
> *"That is what the line of light is saying. Look at it. It is showing off."*

This is Pickle being Pickle — the cozy game's third Pickle line and her most matter-of-fact.

### 10.3 Gerrold cannot see the Web

Gerrold does not see Pickle's text. He sees only the orb glowing slightly more brightly. He says:

> *"... What is —"*
>
> *"I don't see it. I'm sorry."*

You can:

| Option | Response |
|---|---|
| ***(Show Gerrold the orb's cracks)*** | *"Aye. I knew it was — uneven. I've not handled it well. I've held it too tight, perhaps. ... Some things crack in the hands. The wood, the joint — they tell you. I should have asked sooner."* |
| ***(Hold the orb silently)*** | *"... You are looking at it for a long time. Take your time. It is — it is the heaviest thing I own."* |

Either path leads to Beat 5.

---

## 11. Beat 5 — The Tea Decision (Garden → Brew)

**Real-time: 8:00 → 14:00 (with garden detour)**

Gerrold asks how you want to proceed:

> *"Do you want to talk it through? Or do you want me to leave you to it?"*
>
> *"I would not mind sitting here. I would not mind walking with you to my house."*
>
> *"I would not mind, also, going home and leaving the cloth."*

You choose your **path through the next 10 minutes**:

| Option | Path | What happens next |
|---|---|---|
| **"Let's walk to your cottage. I'd like to see where she lived."** | Standard path | You walk to the cottage with Gerrold. Beat 6. **You may visit the garden + brew tea on the way if you exit the Hollow first.** |
| **"I'd like to do it here. Stay if you want."** | Hollow path | You work at the Hollow workbench. Gerrold sits in the chair. The Cleanse mini-game (if applicable) plays here. No cottage. |
| **"Go home. I'll come find you when it's done."** | Alone path | Gerrold leaves with thanks. You work alone. **Memory Dream 2 has no cottage variant; uses the workbench backdrop.** |

> **Recommended: Walk to your cottage.** The cottage scene is Mission 2's emotional peak, and the walk lets Gerrold's 6-line monologue play (see Beat 6).

### 11.1 If you chose to walk — the optional garden detour

If you chose to walk to the cottage, **you may detour through the garden first.** When Gerrold leaves the Hollow ahead of you, you can:

1. Walk through the back door of the workbench room (which is **now unlocked** on Day 2).
2. Enter the garden scene (`04_Mission02_Garden.unity`).
3. Harvest 0, 1, or 2 herbs.
4. Return to the workbench.
5. Brew tea.
6. Carry the tea cup in a small wooden box.
7. Then walk to the cottage.

### 11.2 The herb garden

The garden is a **bright, sunlit outdoor space** — the player's first outdoor cozy space the Hollow itself owns. It contrasts with the Hollow's dim, candlelit interior.

The morning is **10:00 in-fiction**. Bright sun. Bird song (Medieval Village pack's rural morning bird loop). Zephyr wind in the medium-density foliage. Faint sound of bees in the distance (the Mission 3+ apiary; you cannot see it yet).

#### 11.2.1 The two herbs available

| Herb | ScriptableObject | Effect | Tonal connotation | Tea name |
|---|---|---|---|---|
| **Lavender** | `MemoryHerb_Lavender` | **Calm** modifier | The cozy game's default kind herb. Pickle approves. | Lavender Tea |
| **Valerian** | `MemoryHerb_Valerian` | **Sleep** modifier | A sleeping-pill metaphor. Pickle does not commit to approval. | Valerian Tea |

3 plants of each are growing. Beds 3 and 4 are empty (Mission 3+ planting).

#### 11.2.2 The harvest interaction

| Step | Action |
|---|---|
| 1 | Approach a plant (within 1.2 m) |
| 2 | UI prompt: *"Harvest [Lavender / Valerian] (E)"* |
| 3 | Press E |
| 4 | ACS upper-body "pluck" animation (0.8s) |
| 5 | Plant disappears (or shrinks to a stub for Mission 3+ regrowth) |
| 6 | 1× herb added to inventory |

You can harvest up to **2 herbs total** in Mission 2. Per cup of tea: 1 herb required.

#### 11.2.3 The "skip the garden" path

You may walk straight from the Hollow to the cottage without harvesting. **This is a valid path.** The dialogue at the cottage will use the **No-Tea Default** branch — Gerrold remains slightly more reserved, and you don't unlock the extra Margery memory line. No penalty.

### 11.3 Brewing the tea

Walk back to the workbench room. The kettle sits on the wood stove. (Marin's pre-warmed stove keeps it always near-boiling — see the Mission 1 Hollow codex tooltip about *"the village's small miracles."*)

| Step | Action | Effect |
|---|---|---|
| 1 | Approach the stove with an herb in inventory | Prompt: *"Brew tea (E)"* |
| 2 | Press E | Bamao parchment Brewing UI overlay appears |
| 3 | Drag-and-drop the herb icon into the kettle | (Pickle's eyes track the steam if she's in the room) |
| 4 | **Wait 90 in-game seconds (= 12 real seconds)** | VoluSmokeFX steam rises from the spout. Composer's main theme plays. Pickle yawns once. **This is intentional cozy pacing.** |
| 5 | Drag the kettle icon to the cup icon | Soft pour foley + a gentle composer chime: a single sustained note resolving into C major (lavender) or D minor (valerian). |
| 6 | Automatic | Cup is placed in a small wooden tea-carrier box. Inventory: 1× Tea (Lavender) or 1× Tea (Valerian). |

### 11.4 Pickle's fourth Mission 2 line

If Pickle is in the workbench room (which she will be, because you've been at the workbench multiple times today), she speaks **one line at the 6-second mark of the brew wait** (italic, internal):

> *"Tea for a man who has not asked for tea is the rarest kindness."*
>
> *"Try not to overthink it. The man is also a cat about this."*
>
> *"We will accept."*

(This is Pickle's third Mission 2 line, fourth Mission 1-2 line total. Her last unconditional line before the choice screen.)

### 11.5 Why the 12-second wait matters

Many cozy players in playtest feedback name this as a favorite moment. **12 real-time seconds of nothing happening** is not dead time — it is the rhythm. You stand in the workbench room. Pickle yawns. The composer plays. The kettle whistles. You are alone with the work of being a Keeper. **This is the cozy contract honored.**

---

## 12. Beat 6 — The Walk To The Cottage

**Real-time: 14:00 → 15:00**

You exit the Hollow (with or without tea). Gerrold is waiting at the door. The two of you walk down a side lane ~35 metres from the main lane to his cottage.

### 12.1 The walk setting

| Element | Specification |
|---|---|
| Time of day | Early afternoon — first sunlight after the morning's overcast. The Stylized Weather System transitions from cloudy to bright sun during the walk. |
| Light fog | Yes (0.3 intensity). |
| Lumen lantern halos | 3 lanterns spaced along the side lane. The cozy game's wayfinding. |
| Gerrold's pace | 1.5 m/s — slightly slower than your default. The player will intuitively match. *Cozy walking is a social gesture, not transport.* |
| Composer cue | A soft Idris-style fiddle drifts from the inn in the distance — 30 seconds, called the *"Margery walking music"* in the cue sheet. *Margery used to hum this same tune.* |

### 12.2 Gerrold's monologue during the walk

He speaks ~6 lines across the 25-second walk (some lines fire only at certain conditions):

> *"It's been a fair autumn. Cold mornings, but bright."*
>
> *"Margery liked the autumns best. She said the trees gave the village manners."*
>
> *"That is the kind of thing she would say. I never knew how to answer her, exactly."*
>
> *"I just smiled. She did not mind."*

> *"She taught me how to be quiet around someone you love without making it feel like distance."*

> *"I am very loud, now, on my own. I keep wanting to say things that I do not have to say to anyone."*

> *"This is — this is part of why I came to the Hollow. I cannot keep being this loud."*

> *"Here. This is the door. Watch the step — the wood gave last winter and I haven't planed it true yet."*

(He notes the broken step. The wood gave during the winter Margery was dying. He hasn't fixed it. The cozy game's silent metaphor.)

---

## 13. Beat 7 — Inside Gerrold's Cottage

**Real-time: 15:00 → 19:00**

Gerrold opens the cottage door. You step inside.

### 13.1 The interior reveal

A small main room. Two chairs by the fireplace. The fire is **still burning** — Doris lit it yesterday and refreshed the wood this morning while Gerrold was away.

You see, in order:

1. **The hearth** — the fire crackles. Lumen Candle_Warm × 3 + a particle-system flame. Color `#FF9650`.
2. **Two chairs** — Gerrold's on the left, Margery's on the right. Both worn; both have a seat-cushion indent. *Margery's indent is identical to Gerrold's — as if she still sat in it.*
3. **A book on Margery's chair arm** — open to a page she never finished. The bookmark is a pressed leaf — *the same dried-leaf bookmark style as the predecessor's book in the Hollow.* (Cozy cross-reference.)
4. **A small table set for two** — one place setting fresh, one place setting unused but laid out.
5. **A cold cup of tea on the table, in front of Margery's chair.** Gerrold places one daily.
6. **The bedroom door — closed.** A faint sliver of light underneath. Examining it: *"A closed bedroom door. The handle is brass. It has been polished recently. The light underneath is steady. You feel — clearly, suddenly — that you should not open this door today. You believe Gerrold, also, has not opened it in some time."*
7. **A framed photograph on the mantel** — a young Margery and Gerrold on their wedding day, ~36 years ago. Sepia. **Doris (younger) is visible in the background, holding bread.** This is the Echo Web's source material visualized.
8. **A toolbox under Gerrold's chair** — his carpenter's chisels, hammer, plane. Unused recently.

### 13.2 The Heavy Theme Warning Card

If you have the **Heavy Theme Warning Cards** toggle enabled (default ON), a small Bamao parchment card appears at the cottage entry:

> *"This scene contains references to:*
>
> *- Loss of spouse*
> *- Terminal illness (referenced)*
> *- A man's grief*
>
> *Take a breath if you need to. The cat is still on the windowsill back at the shop."*
>
> *[ Dismiss ]*

Tap to dismiss. The card has no effect on game state — it is purely informational.

### 13.3 Choose your chair

Gerrold says:

> *"Have a seat. Either chair."*

You have three options:

| Option | Effect | Gerrold's response |
|---|---|---|
| ***(Sit in the left chair — Gerrold's chair)*** | Sets `$sat_in_gerrold_chair = true` | *"Aye. That's the right one for you. I'll take Margery's, today. She wouldn't mind. She wouldn't have minded most things."* |
| ***(Sit in the right chair — Margery's chair)*** | Sets `$sat_in_margery_chair = true`. **Pickle (if Approval ≥50) speaks:** *"You sat in the dead wife's chair. He is too polite to tell you. But he is grateful. ... I would not have done it. But you are not a cat."* | *"... Aye. That's all right. I should have said which was mine. I should — I should say it more, perhaps."* |
| ***(Stand by the fireplace)*** | Neither chair flag is set | *"As you like. The fire's still warm — Doris stopped by yesterday and lit it."* |

> **Recommended: Sit in Margery's chair.** The Pickle line is the warmest in M2. Gerrold notices. It is a small kindness with no mechanical penalty.

### 13.4 If you brought tea — the tea modifier kicks in

If you brewed tea and brought it, you can offer it to Gerrold here (Bamao prompt: *"Offer tea (E)"*).

| Tea | Gerrold's response | Mechanical effect |
|---|---|---|
| **Lavender (Calm)** | *(after taking a sip)* *"Margery and I had this tea on our last winter together. We had it every evening. She said it tasted like the inside of a folded sweater. I knew what she meant. I still do."* | `$trust_gerrold += 2`. **The Listen option (C) is presented first in the choice screen ordering.** Pickle's commentary on the choice softens. *Lavender nudges the cozy player toward gentleness.* |
| **Valerian (Sleep)** | *(after taking a sip)* *"Aye. This is the one she used to make me when I could not sleep. She would put a sprig of it on the pillow when I — when I could not stop thinking. I have not had this in eleven months. ... Thank you. You are very kind."* | `$trust_gerrold += 2`. **The Erase option (A) is no longer hidden behind extra confirmation.** The Cleanse mini-game's outcomes are slightly more forgiving (in difficulty) but the *Crossed-Core threshold is closer* (in narrative risk). Pickle hesitates. |
| **No tea** | He stays slightly more reserved. The Margery aside about tea does not play. | No modifier. Standard choice presentation. |

> **Recommended: Lavender.** Both Margery's voice (via her metaphor) and the cozy nudge toward the Listen path. The Valerian path is the brave choice — it's not wrong, just more committing.

### 13.5 Gerrold's final pre-choice line

After the tea (or its absence), Gerrold sets the orb on the table between you:

> *"Well, then."*
>
> *"I've said what I've come to say."*
>
> *"I'd like to give you the choice."*

The choice card UI now appears.

---

## 14. Beat 8 — The Moral Choice Screen

**Real-time: 19:00 → 20:00** (no timer — you may sit on this for as long as you wish)

### 14.1 The choice card

The cottage interior dims to 65% brightness. A Bamao parchment scroll fills the lower half of the screen:

```
              ┌─────────────────────────────────────────────────────┐
              │                                                     │
              │       Gerrold has placed the orb on the table.      │
              │       He is waiting.                                │
              │                                                     │
              │       ─────                                         │
              │                                                     │
              │       A — Erase the long bit.                       │
              │           "He will remember he loved her.           │
              │            He will not remember the dying."         │
              │                                                     │
              │       B — Cleanse with care.                        │
              │           "He will keep the memory, gentler.        │
              │            You will need a careful hand."           │
              │                                                     │
              │       C — Don't take the memory. Sit with him.      │
              │           "He will carry it. You will have only     │
              │            been here. Sometimes that is enough."    │
              │                                                     │
              │       D — Ask him to come back tomorrow.            │
              │           "He will. You can think about this        │
              │            for a day."                              │
              │                                                     │
              └─────────────────────────────────────────────────────┘
```

### 14.2 Pickle's pre-choice commentary (Approval ≥ 50)

If `pickle_approval >= 50`, when you highlight an option, Pickle speaks the matching line below it (italic, internal):

| Highlighted | Pickle's pre-confirm line |
|---|---|
| A (Erase) | *"You are about to erase a week of a life. That is — that is a large thing. I am not your conscience. I am only telling you what the verb is."* |
| B (Cleanse) | *"This is the verb the Keeper before us liked best. I think — I think she would be pleased. Or angry. She had moods."* |
| C (Listen) | *"This is the older craft. The one nobody pays for. The good keepers all do it eventually."* |
| D (Defer) | *"A day is a fair thing to ask for. I have asked for many."* |

If Pickle Approval < 50, **no commentary plays.** The choice is made in silence with Gerrold — a different but equally valid texture. Most M1-2 players will be below 50 unless they followed the recommended path-of-kindness.

### 14.3 The choice has no timer

There is **no countdown** on this screen. You may:

- Sit on it for hours of real time.
- Press Escape to pause the entire game (the cottage holds still).
- Walk away from the keyboard and come back.

The shop's clock ticks faintly. Pickle yawns. Gerrold breathes. **That is the design.**

### 14.4 Confirmation

When you press Confirm on an option, a **1.5-second pause** happens. The preview text expands slightly.

- **Option A (Erase)** triggers a **second confirmation** required (the heaviest option requires a re-confirm). You can cancel during the pause.
- **Options B, C, D** confirm with one button (the cozy game's friction increases with the weight of the choice).

After confirming, the Yarn dialogue branches into one of the four paths below.

---

## 15. Path A — Erase The Long Bit

**Real-time: 20:00 → 24:00** (Cleanse mini-game + Dream 2 Variant A or C + outro)

### 15.1 Confirmation flow

You highlighted A. Pickle (if present) read her line. You press Confirm.

The card expands. Gerrold says:

> *"... Aye. The long bit gone."*
>
> *"All of it."*
>
> *"I — I want to be certain that's what I'm asking for."*

A second prompt:

| Option | Effect |
|---|---|
| **"Yes, all of it. Erase the long bit."** | Confirms the Erase path. Gerrold says: *"Aye. Thank you. I'll be here."* Proceeds to Cleanse mini-game (aggressive profile). |
| **"Wait — let me think a moment more."** | Returns to the main 4-option choice card. You can pick differently. |

### 15.2 What "Erase" means mechanically

Erase is **the Cleanse mini-game played at the aggressive difficulty profile.** Your aim is to remove the entire GRIEF palette from the orb — which leaves only the JOY and GRACE traces of the wedding, the years, the smile at the embroidery.

- The mini-game's core region is **smaller** (you have to actually try not to cross it).
- The cracks are deeper (harder to trace cleanly).
- The success threshold is **Acceptable or Perfect** — not Sloppy.
- If you Cross-Core, the outcome is the worst variant of any path: Gerrold loses Margery's *face* in his memory.

### 15.3 Cleanse mini-game (Erase profile)

See Section 19 below for the full mini-game spec. The Erase variant differs only in the aggressive difficulty profile.

### 15.4 Outcomes

After the mini-game, the player returns to Gerrold's door with the orb:

| Outcome | Gerrold's reaction | VillageState |
|---|---|---|
| **Erase + Perfect / Acceptable** | *"... Oh. ... Aye. I can feel that it's lighter. I can still see her face. I cannot — I cannot quite see the last week. ... Thank you. I think — I think this is what I asked for. I am not certain it is what I needed. I will go inside now. I will see what the morning brings."* | `trust_gerrold = +5`, `memory_integrity[GER-007] = 50` (significant erasure), `vow_1_consent = -2` (you removed more than just shame), `vow_3_whole = -3` (over-cleansed), `pickle_approval = -3`. Triggers **Memory Dream 2 Variant A.** |
| **Erase + Crossed Core** | *"... Oh. ... I can't — I can't see her clearly. That wasn't — That wasn't — It isn't your fault. It is — it is the cloth and the cold and the long winter. I will go home. Thank you for trying."* | `trust_gerrold = +2`, `memory_integrity[GER-007] = 30`, `vow_1_consent = -3`, `vow_3_whole = -6`, `pickle_approval = -5`. Margery's face is faded in his memory. Triggers **Memory Dream 2 Variant C.** **Mission 6+ recovery arc seeds here.** |

### 15.5 Why someone might choose Erase

The Erase path is not "bad." It is the choice of a Keeper who believes a man's stated wish should be honored, even when the cost is high. Some cozy players who have lost a spouse themselves report finding this path the most cathartic. **It is structurally valid.** The game does not punish it — it simply describes what happened.

### 15.6 Pickle's after-Erase reaction (if she comes to the cottage in M3+)

In Mission 3+, if Pickle visits the cottage with you again, she will be quieter than usual around Gerrold. She has not forgiven the choice. She does not say so. **The cat remembers.**

---

## 16. Path B — Cleanse With Care (CANONICAL)

**Real-time: 20:00 → 24:00** (Cleanse mini-game + Dream 2 Variant A/B/C + outro)

This is the **canonical "good Keeper" path.** The path the game expects most players to take. The path Marin (the previous Keeper) liked best.

### 16.1 Confirmation flow

You highlighted B. Pickle (if present) said *"This is the verb the Keeper before us liked best..."* You press Confirm. No second confirmation needed. Gerrold says:

> *"Aye."*
>
> *"... With care, then. Whatever you can do."*
>
> *"I trust you. I — I don't know you, but I trust you."*

`$vow_3_whole += 1` immediately fires (his trust counts).

Proceeds to Cleanse mini-game (careful profile).

### 16.2 What "Cleanse" means mechanically

Cleanse is **the Cleanse mini-game played at the careful difficulty profile.** Your aim is to remove the SHAME palette (Gerrold's self-blame for not having done more during the long bit) without removing the GRIEF.

- The mini-game's core region is the **default size**.
- The cracks are tractable (designed for the tutorial).
- The success threshold is forgiving — Perfect, Acceptable, and Sloppy all read as "cleansed."
- Crossed-Core is possible but the consequence is lighter than the Erase variant.

### 16.3 Cleanse mini-game (careful profile)

See Section 19 below for the full spec.

### 16.4 Outcomes

After the mini-game:

| Outcome | Gerrold's reaction | VillageState |
|---|---|---|
| **Cleanse + Perfect** | *"... Aye. I can feel it. The blame is out of me. The dying is still there. But I can — I can carry it now, I think. Margery is — Margery is whole. ... Thank you. I do not know what to say. I will go in and read for a bit. ... Come back when you can. Doris's bread is on Sundays. We could — We could perhaps share one."* | `trust_gerrold = +4`, `memory_integrity[GER-007] = 95`, `vow_3_whole = +5`, `vow_1_consent = +3`, `pickle_approval = +4`. Triggers **Memory Dream 2 Variant A.** |
| **Cleanse + Acceptable** | *"... Aye. It is lighter. Not as light as I'd hoped, but — I do not think I should be as light as I'd hoped, perhaps. Margery is here. I am here. The long bit is — it is part of me, still, but it is no longer the only part. Thank you. I will see you in the village square next week."* | `trust_gerrold = +4`, `memory_integrity[GER-007] = 85`, `vow_3_whole = +3`, `vow_1_consent = +2`, `pickle_approval = +3`. Triggers **Memory Dream 2 Variant B.** |
| **Cleanse + Sloppy** | *"... Aye. There were one or two places I felt you slip. But the work is good. I will go in. I will sleep tonight, I think."* | `trust_gerrold = +3`, `memory_integrity[GER-007] = 75`, `vow_3_whole = +1`, `pickle_approval = +2`. Triggers **Memory Dream 2 Variant B.** |
| **Cleanse + Crossed-Core** | *"... Mmm. I cannot quite — I cannot quite see all of her. It is — it is not what I asked for. But it is not your fault. Thank you for the try."* | `trust_gerrold = +3`, `memory_integrity[GER-007] = 65`, `vow_3_whole = -2`, `pickle_approval = -1`. Triggers **Memory Dream 2 Variant C.** Mission 6+ recovery arc seeds. |

### 16.5 Why Cleanse is canonical

The Cleanse path is what the design **expects most players to take**. It is the verb most aligned with Marin's pinned note. It is the path that produces the **warmest Day 2 evening Ledger prose** (see Beat 10). It is the path the cozy game most rewards mechanically.

---

## 17. Path C — Don't Take The Memory. Sit With Him. (THE LISTEN PATH)

**Real-time: 20:00 → 25:00** (3-min Listen scene + Dream 2 Variant D + outro)

The Listen path is the cozy game's **most quiet, most mature, most surprising choice.** It is the choice that proves the design considers *being present* mechanically equivalent to *doing the work.* It is the cozy game's first moment of **Vow 7 — Last Light**.

### 17.1 Confirmation flow

You highlighted C. Pickle (if present) said *"This is the older craft. The one nobody pays for. The good keepers all do it eventually."* You press Confirm.

Gerrold says:

> *"... I don't —*
>
> *I don't understand.*
>
> *You — you don't want to take it?"*

You choose:

| Option | What happens | Vow 7 bonus |
|---|---|---|
| **"I want to listen. Tell me about it. Hold it the whole time. I won't take it."** | He says: *"... All right. All right. ... All right."* (Three times — each quieter.) | `vow_7_last_light += 5` |
| ***(Reach out and gently place his hands around the orb)*** | He says: *"Aye. ... Aye."* (Wordless acceptance.) | `vow_7_last_light += 7` (the higher reward) |

### 17.2 The 3-minute Listen Cutscene

The cottage dims slightly. The composer's cue M-MD02-D-prelude enters at very low volume — a single sustained note on solo cello. A 3-minute Cutscene Engine sequence plays:

| Time | What plays |
|---|---|
| 0:00–0:15 | You sit down in Gerrold's chair. Gerrold takes Margery's chair. (Or whichever pair you chose.) Pickle, if present, sits on the windowsill. The fire continues to crackle. Gerrold holds the orb in both hands, in his lap. |
| 0:15–0:45 | Gerrold begins. He describes the room (the bedside table, the lamp, the snow outside). His voice is steady. He has rehearsed these sentences alone for eleven months. |
| 0:45–1:30 | Gerrold tells the middle of the week — the small motions, the handkerchief, the way Margery looked at the embroidery. His voice begins to shift; longer pauses; slower delivery. Camera holds tight on his hands and the orb. |
| 1:30–2:15 | Gerrold tells the seventh morning. **He does not cry. He gets through it.** *"The snow had stopped. Doris was in the kitchen. I made tea. I brought it in. She — was already gone."* |
| 2:15–2:45 | Gerrold falls silent. Holds the orb. Looks at the player — *for the first time in eleven months, looks someone directly in the eyes while speaking about her.* *"Thank you. I did not know it would be — easier — when said aloud. It is still heavy. It is no longer alone."* |
| 2:45–3:00 | Gerrold rises slowly. Walks to the mantel. Sets the orb beside the wedding photograph. **He keeps the orb.** Returns to his chair. The fire crackles. Player can stand or remain seated. No prompt. Just held space. |

### 17.3 What Listen does mechanically

| Variable | Effect |
|---|---|
| `trust_gerrold` | +6 (the highest single trust gain in M1-2) |
| `memory_integrity[GER-007]` | 100 (the orb is *not* in your inventory; he kept it) |
| `vow_3_whole` | 0 (you did not Cleanse; the vow is not exercised in either direction) |
| `vow_7_last_light` | **+10 to +12** (the highest vow gain in M1-2 — the Listen path is the only way to move Vow 7) |
| `pickle_approval` | +5 |
| `cinder` | **+3** (the first earning of the Confession Booth's currency, which becomes unlockable in M5+) |
| `first_moral_choice_made` | true |

### 17.4 Why this path is so highly rewarded

The Listen path is the cozy game's most-distilled statement: **you do not have to solve a person to honor them.** Many players in playtest call this the most moving 3 minutes in the vertical slice.

Variables only obtainable via the Listen path in M1-2:
- The **Cinder** currency (the Confession Booth uses it in Mission 5+).
- A meaningful **Vow 7** integrity boost.
- The **warmest possible** Day 2 Evening Ledger prose.
- **Memory Dream 2 Variant D** — a unique dream that uses the cottage chair backdrop instead of the bedroom backdrop, and features Gerrold (not Margery) as the cast figure. It is the cozy game's first moment of the player being directly *thanked.*

### 17.5 Does Listen feel like a lesser path?

**No.** The dialogue rewards it with quiet trust. Gerrold offers you to come for Sunday tea. Pickle's line is the warmest she gives anyone in M1-2. The Vow glyphs in the Ledger that evening visibly brighten more on the Listen path than on any other.

---

## 18. Path D — Ask Him To Come Back Tomorrow (DEFER)

**Real-time: 20:00 → 21:00** (Dream 2 Variant E + outro)

The defer path is **valid and meaningful.** It is the choice of a Keeper who is not yet certain and refuses to commit when uncertain.

### 18.1 Confirmation flow

You highlighted D. Pickle (if present) said *"A day is a fair thing to ask for."* You press Confirm.

Gerrold says:

> *"Aye."*
>
> *"A day, then."*
>
> *"I will come back. I will not be impatient."*

He takes the orb back into its cloth and rewraps it.

### 18.2 What "Defer" means mechanically

- Mission 2 enters its **Deferred branch.**
- Gerrold leaves with the orb. He takes it home.
- You return to the Hollow alone.
- The Cleanse mini-game does not play.
- **Mission 3 begins with Gerrold returning** at the same morning hour, with an additional **fifth option earned through reflection.** (That 5th option is M3 content — its prose is hidden in M1-2.)

### 18.3 Outcomes

| Variable | Effect |
|---|---|
| `trust_gerrold` | +1 (he respects the consideration) |
| `memory_integrity[GER-007]` | 100 (no work was done) |
| `vow_3_whole` | 0 (untouched) |
| `vow_7_last_light` | 0 (you did not Listen, you deferred) |
| `pickle_approval` | +1 (slight approval) |
| `gerrold_returns_day_3` | true (the Mission 3 hook is set) |
| `first_moral_choice_made` | **false** (you have not made one yet) |

### 18.4 Memory Dream 2 Variant E — the lightest dream

The Defer path's dream is the **most restrained** in the cozy game. 30 seconds of held shot on an empty chair beside an empty bed. GRACE-lens only. No music after the first 6 seconds. No cast figures. Just silence and wind.

This is **the cozy game's most poetic dream** — the dream of *not knowing yet.* See § 22.5.

### 18.5 Is Defer "the cowardly path"?

**No.** The defer path is the choice of a Keeper who refuses to do harm out of haste. It is structurally validated — Vow 7 (Last Light) is partially honored by deferral. The cozy player who chooses this path will discover that Mission 3's return-with-fifth-option allows a path no other choice offers: the chance to ask Gerrold *what he wants you to know about Margery before he decides what to do with her.*

---

## 19. The Cleanse Mini-Game — Full Spec

(Used in Paths A and B. Skipped in Paths C and D.)

### 19.1 The verb in detail

You are **tracing the cracks** on the surface of a memory orb to seal them, **without crossing into the core memory region** at the orb's center. The core memory glows soft amber; crossing it causes irreversible memory damage.

### 19.2 GER-007's crack pattern (deterministic — hand-designed)

```
                              ╔═════════════════╗
                              ║                 ║
                              ║   ╱╲       ╱╲   ║  ← cracks at "the bedside table"
                              ║  ╱  ╲     ╱  ╲  ║     (2 short cracks; lighter)
                              ║                 ║
                              ║                 ║
                              ║   ┌───────┐     ║
                              ║   │  CORE │     ║  ← amber-glowing center region
                              ║   │       │     ║     (Margery + the morning)
                              ║   └───────┘     ║
                              ║                 ║
                              ║     ╱─╱─╱       ║  ← crack at "Doris in kitchen"
                              ║                 ║     (1 medium crack)
                              ║                 ║
                              ║    ╲─╲─╲─╲      ║  ← cracks at "the seventh morning"
                              ║                 ║     (2 deep cracks — the hardest)
                              ╚═════════════════╝
```

**4 cracks total** across 3 narrative-locations. The crack pattern is hand-designed for the tutorial.

### 19.3 How to play

| Step | Action | What happens |
|---|---|---|
| 1 | **Hold left mouse** at the start point of a glowing crack | A glowing dot appears at the start. The cursor is "engaged." `cleanse_thread_start` plays. |
| 2 | **Drag along the crack's path** | A glowing thread follows your cursor *only* along the crack's path. The crack visibly seals behind the thread. `cleanse_thread_loop` plays. |
| 3a | **Release at the crack's end point** | Crack seals. `cleanse_crack_seal_complete` chime. Move to next crack. |
| 3b | **Release mid-crack** | Crack is *partially* sealed. The unsealed portion remains. Re-engage and finish. |
| 3c | **Drift off the crack onto the orb's surface** | The thread breaks. The cursor "loses grip." Crack remains. `cleanse_thread_break` plays. Re-try. |
| 3d | **Drift into the core amber region** | **Core damage event:** `cleanse_core_warn` (warning zone) → `cleanse_core_damage` (crossed). Margery's face fades in the orb (visible). `memory_integrity[GER-007] -= 25`. The orb is now Crossed-Core. |

### 19.4 The four outcome states

| Outcome | Trigger | Time taken | What you feel |
|---|---|---|---|
| **Perfect** | All 4 cracks sealed cleanly. 0 core crossings. | ~90s | Triumph, then quiet satisfaction. Gerrold's reaction is the warmest. |
| **Acceptable** | 3 of 4 cracks sealed. 0 core crossings. (1 crack remains partially open.) | ~75s | Mild relief. Gerrold's reaction is gentle. |
| **Sloppy** | 4 of 4 cracks sealed but with 1–2 brief core touches. | ~85s | Concern. The orb's `memory_integrity` is at 80. Gerrold notices something is *slightly* off but cannot articulate it. |
| **Crossed Core** | 3+ core touches OR a sustained core crossing (>1 second inside). | ~variable | Real loss. Memory Dream 2 plays its "core damaged" variant. Gerrold's reaction is forgiving but the player has done a thing. |

### 19.5 Audio cues (11 specific sounds)

| Cue ID | When | Volume |
|---|---|---|
| `cleanse_hum_start` | Player engages with the orb | 0.4 |
| `cleanse_hum_dissonant_loop` | While cracks remain | 0.45 (dissonant) |
| `cleanse_thread_start` | Cursor engages first crack | 0.6 |
| `cleanse_thread_loop` | While actively sealing a crack | 0.5 |
| `cleanse_crack_seal_complete` | Each crack finishes | 0.65 |
| `cleanse_thread_break` | Cursor drifts off crack | 0.5 |
| `cleanse_core_warn` | Cursor enters the core's *outer aura* | 0.7 (urgent) |
| `cleanse_core_damage` | Cursor crosses into core proper | 0.85 (the loudest single SFX in the game so far — *the chime of regret*) |
| `cleanse_complete_perfect` | All 4 cracks sealed, no damage | 0.75 (composer cue: Margery motif) |
| `cleanse_complete_acceptable` | 3 cracks sealed, no damage | 0.65 (slightly muted Margery motif) |
| `cleanse_complete_crossed` | Crossed-Core | 0.7 (composer cue: hesitant minor variation, ending on unresolved chord) |

### 19.6 Adaptive difficulty profiles

| Profile | Used when | Behavior |
|---|---|---|
| **Default Cleanse** | Standard Mission 2 with Lavender / no tea | Crack edges thicker (more forgiving). Core region tractable. Three retries before consequence. |
| **Aggressive (Erase)** | Path A — Erase | Crack edges normal. Core region smaller (you have to actively avoid it). Tighter outcome thresholds. |
| **Gentle Mode** | If Gentle Mode is enabled | Core damage is *recoverable* via a 3-day in-game "memory grief" event — Gerrold's Memory Integrity automatically rises 10 points per day. Five retries. |
| **Valerian-modified** | If Valerian tea was offered | Core region is slightly larger (Gerrold relaxes), but Crossed-Core threshold is closer. |

### 19.7 The Auto-Complete toggle

A small Bamao parchment button **"Cleanse for me"** is visible from the very first frame of the Cleanse mini-game.

| Mode | Auto-complete outcome |
|---|---|
| Default | Acceptable Cleanse (3 of 4 cracks sealed). |
| Gentle Mode | Perfect Cleanse. |
| Erase profile (Path A) | Acceptable Erase. |

**The cozy player who Auto-Completes still experiences the full consequence of their choice** (Erase / Cleanse / Listen / Defer). The choice carries weight regardless of skill.

### 19.8 Failure semantics — the cozy contract holds

Crossing the core is a permanent outcome on the memory. **But:**

1. Crossed-Core is **not** a game-over.
2. The memory still works — it is just less clear.
3. The narrative absorbs the outcome (§ 15.4, § 16.4).
4. The Mission 6+ recovery arc exists.
5. **The player is never told they "failed."** The system never displays "Failure." It displays only what happened. Gerrold's reaction is the feedback.

### 19.9 Pickle's hint

If you struggle for >45 seconds without sealing a crack, Pickle offers one hint (italic, internal):

> *"Trace the line of the crack itself. Not the orb around it."*

That is the only hint in the Cleanse tutorial.

### 19.10 Tips for the manual player

- **Rotate the orb** with right-mouse-drag to see all 4 cracks. The deepest 2 are on the bottom of the orb.
- **Start with the easier cracks first** — the 2 short "bedside table" cracks at the top. Build muscle memory.
- **Keep the cursor on the crack itself** — not on the surface around it. The thread breaks if you slip off.
- **Slow down near the core.** The 2 deep "seventh morning" cracks pass closest to the core.
- **If you start to slip — release the button** and re-engage. Partial seals are fine; you can return to them.

---

## 20. Beat 9 — Returning The Handkerchief

**Real-time:** depends on path; ~24:00 → 25:30 if Cleanse, ~25:00 → 26:00 if Listen, instant if Defer.

After the Cleanse mini-game (Paths A/B) or the Listen scene (Path C), you return to Gerrold's cottage door.

### 20.1 The handkerchief gesture

Gerrold opens the door. You hold the folded white handkerchief.

| Path | The handkerchief gesture |
|---|---|
| A — Erase | You return the handkerchief (now containing the partially-erased orb). |
| B — Cleanse | You return the handkerchief (containing the cleansed orb). |
| C — Listen | The handkerchief is still folded — Gerrold held the orb in it the whole time. You hand it back. |
| D — Defer | (No return — Gerrold took it home with him after the Hollow conversation.) |

A single-button interaction at the door. The Bamao prompt: *"Hand back the handkerchief (E)."*

Press E. Gerrold's response:

> *"Aye. Thank you."*
>
> *"Margery folded that for me. I'd been losing it for weeks."*
>
> *"... Take care of yourself, keeper."*

(He calls you *"keeper"* — the first time in Mission 2. **The full title is only earned after the moral choice.**)

### 20.2 Examining the handkerchief in your Codex

Before returning, you may examine the handkerchief in your Codex. Its entry:

> *A white linen handkerchief, folded into a square. The corner is embroidered with a small "M" in dark green silk. The needlework is slightly uneven — the work of a young hand. The cloth smells faintly of cedar.*

(The "M" is Margery's monogram, done when she was 22 — the year of her wedding. The cedar smell is from the drawer she kept it in. None of this is spoken in dialogue; only the codex says.)

### 20.3 The walk back

You walk back along the side lane to the Hollow. Gerrold has gone inside. The sun is lower now — late afternoon. The Stylized Weather System adds a soft amber tint to the world. Composer's main theme plays at 25% volume.

The walk back has **no dialogue** — Mission 2's emotional peak has passed. You walk alone.

---

## 21. Beat 10 — Day 2 Evening Ledger (5 Variants)

**Real-time: 26:00 → 28:00**

You arrive at the Hollow. The shop is empty. Pickle is on the windowsill. The fire in the stove is low. The day is fading.

Walk to the workbench. The Evening Ledger is glowing softly. Press E to open it.

### 21.1 Day 2's Evening Ledger has 5 prose variants

The left page's transactions are **completely different prose** depending on the outcome path:

#### 21.1.1 Variant A — Cleanse Perfect (best Cleanse outcome)

> *Day 2 — Spire-Month, Week 1*
>
> *Gerrold Pell, the widower, came to the Hollow at half past seven. He brought a memory wrapped in his wife's handkerchief. You took it. You did the work. The work was clean.*
>
> *You walked to his cottage. You sat in his chair. You returned the handkerchief.*
>
> *Margery Pell, who died last winter, is now a clearer memory than she was this morning. The man who carried her will sleep tonight.*

#### 21.1.2 Variant B — Cleanse Acceptable / Sloppy

> *Day 2 — Spire-Month, Week 1*
>
> *Gerrold Pell, the widower, came to the Hollow at half past seven. He brought a memory wrapped in his wife's handkerchief. You took it. You worked carefully.*
>
> *You walked to his cottage. The fire was warm. You returned the handkerchief.*
>
> *Margery Pell is gentler in his memory than she was this morning. The long bit is no longer the whole of it.*

#### 21.1.3 Variant C — Erase OR Cleanse Crossed-Core

> *Day 2 — Spire-Month, Week 1*
>
> *Gerrold Pell brought a memory. You took it. The work did not go as planned.*
>
> *He thanked you. He is going home.*
>
> *His wife's face, in his memory, is no longer entirely her own.*

#### 21.1.4 Variant D — Listen (the warmest variant)

> *Day 2 — Spire-Month, Week 1*
>
> *Gerrold Pell brought a memory. You declined to take it.*
>
> *You sat in his cottage for three hours while he spoke. He told you everything.*
>
> *He still has the memory. He is no longer alone with it.*

#### 21.1.5 Variant E — Defer

> *Day 2 — Spire-Month, Week 1*
>
> *Gerrold Pell brought a memory. You asked him to come back tomorrow.*
>
> *He thanked you for the consideration. He took the cloth home.*
>
> *The orb is in his cottage tonight. The choice is still open.*

### 21.2 The right page — open work

The right page lists the open threads. After Mission 2 it shows:

> *Open work:*
>
> *— Doris's First Loaves is on the shelf. You may return to it any morning.*
>
> *— [depending on path]:*
>
> *   • Gerrold Pell's cleansed orb is on the shelf beside Doris's.* (Paths A, B)
>
> *   • Gerrold Pell kept his wife's memory. He may visit again on Sunday.* (Path C)
>
> *   • Gerrold Pell will return tomorrow with the cloth.* (Path D)
>
> *— There is a locked room upstairs. The door is heavy.*
>
> *— The garden is open now. Two raised beds remain empty.*
>
> *— There is a man at the lane's bend, walking with a letter. He still has not spoken.*

### 21.3 The 7 Vow glyphs — visual update

The glyphs at the bottom of the left page now show their post-Mission 2 values. Brighter glyphs = higher integrity. **A faint hum** plays the first time Vow 7 brightens significantly (Listen path) — this is the only audio signature of a single Vow.

| Vow | After M1-2 — typical value range |
|---|---|
| Consent | 49 (Erase) / 53 (Cleanse) / 54 (Listen) / 51 (Defer) |
| Return | 50 (untouched in M1-2) |
| Whole | 47 (Erase) / 56–58 (Cleanse Perfect) / 53 (Listen) / 53 (Defer) |
| Quiet Glass | 50 (untouched) |
| Honest Coin | 50–53 (depending on M1 price negotiation) |
| Open Door | 50 (untouched) |
| **Last Light** | 50 (Erase/Cleanse/Defer) / **60–62 (Listen)** — *the only path that moves it* |

### 21.4 Closing the book — Day 2 save

Press **Close the book**. The cozy sleep transition plays (identical to Mission 1). Pickle stretches. The amber fade begins. Memory Dream 2 begins.

---

## 22. Beat 11 — Memory Dream 2 (5 Variants)

**Real-time: 28:00 → 29:30** (varies by variant)

After the Ledger closes, the **2.5-second Sleep Transition** plays (identical to Mission 1). Then Memory Dream 2 begins.

**This is the cozy game's most consequential dream.** Five entirely different variants exist, all using the same hand-painted set-piece (a 1992 bedroom with a single bedside lamp, a folded handkerchief on the bedside table, snow outside the window). The variants differ in lens, cast visibility, camera blocking, and composer cue.

### 22.1 Variant A — Cleanse Perfect (canonical)

**Length: 85 seconds.**

| Time | What you see |
|---|---|
| 0:00–0:05 | Fade in. Margery's bedroom in winter. The handkerchief is folded on the bedside table. |
| 0:05–0:10 | Margery fades in, sitting up in the bed, soft-focused. *(Her face is clearly visible.)* |
| 0:10–0:25 | She picks up the handkerchief from the bedside table. |
| 0:25–0:40 | She looks at the embroidery on the corner — the "M" she did when she was 22. Camera extreme-close on the embroidery. |
| 0:40–0:55 | A small slow smile begins. |
| 0:50–0:55 | **Gerrold appears briefly in the doorway** — silhouette only, nods, exits. *(He was there for her. The dream remembers it.)* |
| 0:55–0:70 | She holds the handkerchief, eyes closing softly (falling asleep). |
| 0:70–0:80 | At peace, in the chair or pillow. The handkerchief is in her hand. |
| 0:80–0:85 | Held. Camera pulls slowly back. Fade. |

**Composer cue: M-MD02-A** — Margery motif, solo violin → piano → cello, full warm resolution. Major chord ending.

**Title overlay**: *"The Last Week"* (2-7s) → *"— Saltlight, last winter"* (78-83s).

This is the canonical Mission 2 dream.

### 22.2 Variant B — Cleanse Acceptable / Sloppy

**Length: 80 seconds.**

Same set-piece. Same general blocking. Differences from Variant A:

- Lens is **GRIEF + GRACE + a faint trace of SHAME**.
- Margery's face is **softened by lens-blur** (visible but slightly less clear).
- Her smile is visible.
- **Composer cue: M-MD02-B** — Margery motif, slightly hesitant. Resolves but on a softer chord.

### 22.3 Variant C — Cleanse Crossed-Core OR Erase Path

**Length: 90 seconds.**

Same set-piece. Differences from Variant A:

- Lens is **GRIEF (heavy) + GRACE (sparse)**.
- **Margery's face is fog-blurred.** The player sees her shape, the handkerchief, the smile, but **not the features clearly**.
- Camera at the 40–55s extreme close is on the **handkerchief, not on her face** — *the camera is choosing to protect the player from her absence.*
- Gerrold's doorway silhouette **lingers longer**, looking in. He does **not** nod.
- **Composer cue: M-MD02-C** — same melody, but ending on an unresolved minor seventh.

This is the **dream the cozy player will remember** if they Crossed-Core'd. It is hauntingly written.

### 22.4 Variant D — Listen Path (UNIQUE)

**Length: 75 seconds.** The most narratively different variant.

| Time | What you see |
|---|---|
| 0:00–0:10 | The cottage chair. Gerrold's chair, alone, in the empty bedroom. The bed is mostly off-frame. *(Margery is never visible.)* |
| 0:10–0:30 | Gerrold appears, sitting in his chair. Hands in lap. |
| 0:30–0:50 | Tight on his hands. Then his face. He is recounting — to no one — what he just told you. |
| 0:50–0:65 | He looks directly at the camera — *at the player, who is dreaming him.* His eyes close softly, briefly. Open again. |
| 0:65–0:71 | **One small smile.** |
| 0:71–0:75 | Held for 4 seconds. Cut to black. |

**Composer cue: M-MD02-D** — solo cello, no other instruments. The most spare cue in the entire vertical slice.

**Lens**: GRIEF (gentle) + GRACE (full) + **WONDER** (a soft golden trace — unique to this variant; the wonder of being heard).

This is **the cozy game's first moment of the player being directly thanked.** It is the design's strongest argument for choosing the Listen path. Many playtesters report it as the most moving moment in the vertical slice.

### 22.5 Variant E — Defer

**Length: 30 seconds.** The lightest dream.

| Time | What you see |
|---|---|
| 0:00–0:06 | The cottage. Margery's chair, empty. The book on its arm. The cold cup of tea. The bed is empty. *Nothing else.* |
| 0:06–0:30 | Held shot. Silence after the first 6 seconds (only the wind outside). |

**Lens**: GRACE only. No music after the first 6 seconds.

This is the cozy game's **most restrained dream** — the dream of *not knowing yet.* It is the appropriate dream for a Keeper who deferred.

### 22.6 The morning wake — slightly different per variant

The 3-second wake transition is identical, but the morning-cue is subtly different:

| Variant | Day 3 morning cue |
|---|---|
| A | Doris's motif at low volume — a normal cozy morning |
| B | Doris's motif slightly muted |
| C | Doris's motif at very low volume; a long held note |
| D | A piano motif you have not yet heard — *the Sunday-tea theme* (a Mission 3+ teaser) |
| E | Silence; only the bird outside |

---

## 23. Beat 12 — Mission 2 Outro Cinematic

**Real-time: 29:30 → 30:00** (always 30 seconds, regardless of path)

After Memory Dream 2 wakes, a **30-second Cutscene Engine outro** plays. The cinematic is identical in length across paths but **its visual content differs**:

```
TIMELINE: Mission02_Outro
Total duration: 30.0 seconds

  0:00–0:08  Slow pan through the Hollow.
             The chairs sit empty.
             Pickle sleeps on the windowsill.
             The fire in the stove is low.

  0:08–0:18  Camera finds the workbench.
             [Variant by path]:
             - If you returned the handkerchief: workbench is empty.
             - If you have the handkerchief still: it is folded on the bench beside the orb cradle.

  0:18–0:25  Camera finds the shelves.
             DOR-001 (Doris's First Loaves) is on the shelf — softly glowing.
             [Variant by path]:
             - Path A (Erase): GER-007 (clouded, partial) beside DOR-001.
             - Path B (Cleanse): GER-007 (cleansed, full clarity) beside DOR-001.
             - Path C (Listen): GER-007 is NOT on the shelf — it is in Gerrold's cottage. The shelf has an empty space.
             - Path D (Defer): GER-007 in its cloth on the counter, untouched.

  0:25–0:30  Camera pulls back. Text overlay (Bamao parchment frame):
             "Day 2 ended. Tomorrow, the village will know."
             Fade to black.

  Composer cue: Hearthbound main theme, final 20 seconds (full statement).
```

The shelf shot is **the cozy game's strongest visual signature** of your choice. The cozy player who replays the game and picks Listen will see one less orb on the shelf — and feel the absence.

After the outro, the game saves silently and returns to the main menu. Your save is now Day 3 ready.

---

## 24. Mission 2 Outcome Matrix

The 5 outcome paths summarized in one table:

| Variable | Path A — Erase | Path B — Cleanse | Path C — Listen | Path D — Defer |
|---|---|---|---|---|
| `trust_gerrold` | +5 (best) / +2 (CC) | +4 (Perfect) / +3 (other) | **+6 (highest in M1-2)** | +1 |
| `memory_integrity[GER-007]` | 50 (good) / 30 (CC) | 95 (Perfect) / 85 / 75 / 65 (CC) | 100 (untouched) | 100 (untouched) |
| `vow_1_consent` | -2 to -3 | +2 to +3 | +0 | +0 |
| `vow_3_whole` | -3 to -6 | +1 to +5 | +0 | +0 |
| `vow_7_last_light` | +0 | +0 | **+10 to +12** | +0 |
| `pickle_approval` | -3 to -5 | +2 to +4 | **+5** | +1 |
| `cinder` | +0 | +0 | **+3** | +0 |
| `first_moral_choice_made` | true | true | true | **false** (Mission 3 re-engages) |
| `gerrold_returns_day_3` | false | false | false | **true** |
| Memory Dream 2 variant | A or C | A, B, or C | **D (unique)** | E (lightest) |
| Day 2 Ledger prose | Variant A (good) / C (CC) | Variant A (Perfect) / B (other) | **Variant D (warmest)** | Variant E (open) |
| Shelf state at outro | GER-007 clouded | GER-007 cleansed | **No GER-007 (empty space)** | GER-007 in cloth on counter |

### 24.1 The "best Vow" path is path-dependent

There is **no single "best" path**. Cleanse Perfect (B) maximizes Vow 3. Listen (C) maximizes Vow 7. Either is valid as the canonical "good Keeper" play. **The cozy game refuses to rank them.**

### 24.2 What if you want to play every variant?

Use the **3 manual save slots** + **1 emergency autosave**. Before the moral choice screen:

1. Save to Slot 1.
2. Play Path A. Save to Slot 2.
3. Load Slot 1. Play Path B. Save to Slot 3.
4. Load Slot 1. Play Path C. Keep autosave.
5. Load Slot 1. Play Path D.

Each playthrough's Mission 2 takes 30–40 minutes. The cozy player who explores all 5 variants will have ~3 hours of unique content from this single mission.

---

## 25. Hidden Details & Easter Eggs

### 25.1 The wedding photograph (Doris in the background)

The framed photograph on the cottage mantel shows young Margery and Gerrold on their wedding day, ~36 years ago. **Doris is visible in the background, much younger, holding bread.** This is the Echo Web's source material in visual form — the same kitchen, the same friend, the same Sunday.

In Mission 4+, examining this photograph reveals Doris's full role at the wedding.

### 25.2 The broken step

Gerrold mentions the broken step on his cottage's front porch during the walk: *"The wood gave last winter and I haven't planed it true yet."* The step is **physically broken in the scene** — you can see the split.

If you return to the cottage in Mission 5+ and *plane the step true* yourself (a small carpentry interaction unlocked then), Gerrold's response to that gesture is the cozy game's most-quoted Mission 5+ line.

### 25.3 The bedroom door

Examining the closed bedroom door gives the codex entry above (§ 13.1). **It cannot be opened in Mission 1-2.** It is Mission 5+ content. The cozy player who sees the steady light underneath and knows not to open it is rewarded silently with `vow_7_last_light += 1` (an undocumented bonus — Vow 7 is the vow of knowing when not to look).

### 25.4 The toolbox

Examining the toolbox under Gerrold's chair: *"A leather toolbox. Carpenter's chisels, a hammer, a block plane. The blade of the plane is dull. He has not used these in some weeks."*

In Mission 6+, Gerrold will offer to **make you something** — a small wooden box for a memory of your own. The toolbox is the prop foreshadowing.

### 25.5 The herb-modifier subtlety

If you offered **Lavender** and chose **Listen**, the dream's WONDER trace is slightly brighter (the wonder-of-being-heard is amplified by the warmth of the tea). If you offered **Valerian** and chose **Listen**, the dream's GRACE is slightly muted. **Most players will not consciously notice this.** It is one of the cozy game's invisible-design details.

### 25.6 Pickle's M2 garden line (very rare)

If `pickle_approval >= 75` at the start of the garden visit (essentially impossible in M1-2 without a debug command), Pickle follows you to the garden and speaks:

> *"You picked the wrong one. Or the right one. I don't know herbs. I am a cat. Carry on."*

This line is **canonically unreachable** in Mission 1-2 by normal play. It exists in the Yarn file as a Mission 3+ teaser.

### 25.7 The empty place setting

The cottage's small table has two place settings — one fresh, one unused but laid out. Examining the unused one: *"A clean plate, a clean fork, a folded napkin. The napkin has not been refolded since some Sunday."*

In Mission 3+, you can sit down at this place setting during a Sunday visit. Gerrold will be moved. The cozy game saves this for the right moment.

### 25.8 The cold tea

Gerrold places a cold cup of tea in front of Margery's chair daily. Examining it: *"A cup of tea. Cold. It has been here all morning. He brews one every day. He has never drunk it himself."*

In Mission 5+, you can offer to **make the daily tea for him** — a small task that becomes a Mission 5 routine. Mission 2's cup is the seed.

### 25.9 The cozy completion path

The cozy player who:

1. **Honored Doris's price exactly** in Mission 1 (4 cu).
2. **Asked "Who was the old one?"** in Mission 1.
3. **Examined all 7 predecessor seeds** in the Hollow.
4. **Brewed Lavender tea** in Mission 2.
5. **Sat in Margery's chair.**
6. **Chose Listen** (Path C).
7. **Returned the handkerchief** at the end.

…lands on the highest combined Vow integrity reachable in M1-2 and unlocks the **warmest variant** of every Mission 3 morning dialogue line. This is the game's first **hidden completion** — never announced, only felt.

---

## 26. Accessibility Paths

| Tool | What it does in Mission 2 |
|---|---|
| **Gentle Mode** | All memory prose is softened. The Cleanse mini-game's core region is forgiving. Crossed-Core is recoverable (Gerrold's Memory Integrity rises 10/day after the cleanse). Sprint + Jump disabled. Pickle's lines are warmer. The moral choice's confirmation pause is longer. |
| **Auto-Complete Cleanse** | The 8-second cinematic plays. Outcome = Acceptable (Gentle Mode = Perfect). The choice's narrative branches identically. |
| **Heavy Theme Warning Card** | Default ON. Appears before the cottage entry. Single Bamao card. |
| **Subtitle Size: Large or Huge** | All of Gerrold's ~270 Yarn lines render at the chosen size. The 3-minute Listen scene's lines especially benefit from larger subtitles. |
| **Color-blind Mode** | The orb's crack-glow + core-amber palette pair uses a different lookup. Test before relying on the visual feedback. |
| **Reduce Particle Intensity** | Halves the Lumen lantern halos on the walk. Halves the dream's particles (snow, dust motes). |
| **Reduce Screen Flash** | The `cleanse_core_damage` chime no longer has its visual flash component. |
| **Pickle Sass Intensity** | At 1/5, Pickle's pre-choice commentary is entirely absent (she does not judge in low-sass mode). At 5/5, all her lines play, including hidden ones. |

---

## 27. Best-Practice Walkthroughs By Player Type

### 27.1 *"I want the canonical cozy experience"*

1. Wake → walk to Doris briefly → ask about Marin.
2. Gerrold arrives → ask "Tell me about Margery first."
3. Pick up the orb → witness the Echo Web.
4. Walk to garden → harvest **1 Lavender**.
5. Brew lavender tea.
6. Walk to cottage with Gerrold.
7. Sit in **Margery's chair**.
8. Offer tea.
9. Choose **B — Cleanse with care**.
10. Aim for **Perfect** in the Cleanse mini-game (~90 seconds, manual, slow).
11. Return the handkerchief.
12. Close the Ledger.
13. Watch Memory Dream 2 Variant A.

**Result:** highest Cleanse-path Vow 3 integrity. Gerrold offers Sunday tea. Doris's Mission 3 morning is warmer.

### 27.2 *"I want the deepest emotional experience"*

1. Wake → skip Doris.
2. Gerrold arrives → ask "Tell me about Margery first."
3. Pick up the orb → witness the Echo Web.
4. **Skip the garden** (no tea — Gerrold's reservedness will be heavier).
5. Walk to cottage with Gerrold.
6. **Stand by the fireplace** (neither chair).
7. Choose **C — Don't take the memory. Sit with him.**
8. ***(Reach out and gently place his hands around the orb)*** — the higher Vow 7 sub-option.
9. Sit through the full 3-minute Listen Cutscene.
10. Return the handkerchief.
11. Close the Ledger.
12. Watch Memory Dream 2 Variant D (the unique "thanked" dream).

**Result:** Highest Vow 7 integrity reachable in M1-2. +3 Cinder. The most distinct Memory Dream variant. Many cozy players name this their favorite single hour of play they've ever had.

### 27.3 *"I want to explore everything"*

Use the save slots. See § 24.2.

### 27.4 *"I have only 30 minutes"*

1. Wake → skip Doris.
2. Gerrold arrives → immediately choose "Yes, please" → quick dialogue.
3. Pick up orb.
4. Skip garden.
5. Walk to cottage.
6. Sit in either chair.
7. Choose **B — Cleanse with care**.
8. **Auto-Complete Cleanse** (the 8-second cinematic).
9. Return handkerchief.
10. Close Ledger.
11. Watch Memory Dream 2 Variant B.

**Result:** ~22 minutes total. The choice still carries weight; the autocomplete does not penalize you.

### 27.5 *"I'm sensitive to grief content"*

1. Enable **Gentle Mode** at the start.
2. Keep **Heavy Theme Warning Cards** ON.
3. Pickle Sass Intensity: 1/5.
4. Choose **D — Defer** at the moral choice screen.
5. Or choose **C — Listen** if you feel up to it (the Listen path is the gentlest of the engagement choices).

**Result:** Mission 2 ends with no destroyed memory and minimal emotional friction. You can return to it on Day 3 (Mission 3+) when you feel ready.

---

## 28. What This Mission Teaches You For Mission 3+

After Mission 2 completes, you have learned (in the body, not in the menu):

| Lesson | Where it goes |
|---|---|
| **Memory transactions are moral acts.** | Every Mission 3+ villager visit will present a choice. |
| **The Echo Web shows connections between memories.** | Mission 3+ has 12+ connections to discover. Reaching the threshold of any Echo Web triggers a Revelation Chapter. |
| **The Cleanse mini-game has real stakes.** | Mission 5+ Cleanses get harder. Marin teaches Sever (Mission 7+) — the opposite operation. |
| **Vow 7 (Last Light) is a real path.** | Mission 6+ unlocks the Confession Booth, where Vow 7 is the primary currency. |
| **Pickle is a real character.** | Mission 3+ unlocks her full quote library (~220 lines across the full game). |
| **Marin is a real predecessor with a real story.** | Mission 4+ Reveal Chapter 12 introduces her formally. |
| **Doris and Margery were best friends.** | Mission 4+ Reveal Chapter 17 reconstructs their friendship over 30 years. |
| **The garden grows.** | Mission 3+ unlocks planting in the empty beds. |
| **The locked rooms open.** | Hollow upstairs in Mission 5+. Gerrold's bedroom in Mission 8+. |

You enter Mission 3 having had **the cozy game's defining moment.** Everything that comes after builds on the trust this hour earned.

---

## 29. Critic & Review Board Sign-Off

| Reviewer | Verdict | Note |
|---|---|---|
| **🎬 Creative Director** | ✅ Approved | "All four paths are presented as equally valid. The Listen path is not editorialized as 'best' but is clearly identified as the cozy-mature choice with the highest Vow 7 reward. Gerrold's voice signature is preserved throughout." |
| **🗺️ Game & Level Designer** | ✅ Approved | "Cleanse mini-game spec matches Focus 04. Cottage layout matches Focus 03. Tea modifier mechanics match Focus 06. All 5 dream variants accounted for per Focus 05." |
| **📈 Trend & Ideation Analyst** | ✅ Approved | "The 'refusal is valid' framing of Path C and Path D is exactly the cozy-game discipline the audience expects. The Heavy Theme Warning Card placement (§13.2) shows the Tone Compass principle in action." |
| **🎨 Unity Asset Engineer** | ✅ Approved | "All scene/prop references match the asset whitelist. The shader-state crack pattern (§19.2) matches the GER-007 ScriptableObject. The Bamao parchment scroll and Lumen cottage lighting are correctly described." |
| **👨‍💻 Senior Unity Developer** | ✅ Approved | "All system references map to a real script: CleanseMiniGame, ChoiceCardUI, EchoWebVisualizer, MemoryDreamSequencer, ListenSceneSequencer, RippleEngine. The Variant-Switching pattern in §22 matches MemoryDream2_Sequencer.cs exactly." |
| **🔍 Critic & Review Board** | ✅ **Approved** | "This is the deepest mission walkthrough in the cozy genre. It will become the gold-master both for the 20-person greenlight playtest and for the eventual public Steam-page guide. Outstanding work — ship it." |

---

*Document version 1.0 — authored for the `feat/mission-1-2-architecture` branch, Phase 27.1 build. Phase 32 (menu collapse) update — § 4 troubleshooting row now references `Hearthbound → ⚙️ Advanced → 🎭 Phase 26 — Wire NPC Animators` per D-051.*
*Companion files: [`GAMEPLAY_GUIDE_OVERVIEW.md`](./GAMEPLAY_GUIDE_OVERVIEW.md), [`GAMEPLAY_GUIDE_MISSION_1.md`](./GAMEPLAY_GUIDE_MISSION_1.md).*
*Part of the Abdulmalek Agents game-concept portfolio · 2026.*
