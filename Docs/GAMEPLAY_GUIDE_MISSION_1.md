# 🫖 Mission 1 — *Opening The Hollow*
## Gameplay Guide · Full Step-by-Step Walkthrough

> *"You're the new one. I thought you'd be taller."* — Doris

> **This is the per-mission gameplay guide for Mission 1.** It is written so a player can sit down with this file open and have **every option, every prop, every Pickle line, every hidden detail, and every outcome** mapped out before them. Spoilers below — by design. This is the cozy-game equivalent of a strategy guide.
>
> Read [`GAMEPLAY_GUIDE_OVERVIEW.md`](./GAMEPLAY_GUIDE_OVERVIEW.md) first for controls, comfort settings, the Cozy Contract, the 7 Vows, the Pickle Approval system, and the Evening Ledger.
>
> **Branch:** `feat/mission-1-2-architecture`
> **Mission 1 in-game day:** Day 1 (Spire-Month, Week 1)
> **Real-time:** 25–35 minutes (cozy unhurried pace ~32 min)
> **Scenes used:** `02_Mission01_Lane`, `03_Mission01_Hollow`

---

## 0. Table of Contents

1. [Mission Purpose — What This Hour Is For](#1-mission-purpose--what-this-hour-is-for)
2. [Mission Objective — In One Sentence](#2-mission-objective--in-one-sentence)
3. [Pre-Flight Checklist](#3-pre-flight-checklist)
4. [The Mission 1 Cast](#4-the-mission-1-cast)
5. [Mission 1 Map](#5-mission-1-map)
6. [Beat 1 — Bootstrap, Main Menu, Opening Cinematic](#6-beat-1--bootstrap-main-menu-opening-cinematic)
7. [Beat 2 — The Autumn Lane At Dusk](#7-beat-2--the-autumn-lane-at-dusk)
8. [Beat 3 — Meeting Doris Outside The Bakery](#8-beat-3--meeting-doris-outside-the-bakery)
9. [Beat 4 — Entering The Bakery + The Hand-Off](#9-beat-4--entering-the-bakery--the-hand-off)
10. [Beat 5 — The Price Negotiation](#10-beat-5--the-price-negotiation)
11. [Beat 6 — Inside The Hollow (The Predecessor Seeds)](#11-beat-6--inside-the-hollow-the-predecessor-seeds)
12. [Beat 7 — The Workbench + Marin's Note + Pickle](#12-beat-7--the-workbench--marins-note--pickle)
13. [Beat 8 — The Polish Mini-Game (Full Walkthrough)](#13-beat-8--the-polish-mini-game-full-walkthrough)
14. [Beat 9 — Placing The Orb On The Shelf](#14-beat-9--placing-the-orb-on-the-shelf)
15. [Beat 10 — Returning To Doris (Three Outcome Lines)](#15-beat-10--returning-to-doris-three-outcome-lines)
16. [Beat 11 — The Evening Ledger (First Save)](#16-beat-11--the-evening-ledger-first-save)
17. [Beat 12 — Memory Dream 1 (Witness Mode)](#17-beat-12--memory-dream-1-witness-mode)
18. [Mission 1 Outcome Table](#18-mission-1-outcome-table)
19. [Hidden Details & Easter Eggs](#19-hidden-details--easter-eggs)
20. [Accessibility Paths](#20-accessibility-paths)
21. [What You Carry Into Mission 2](#21-what-you-carry-into-mission-2)
22. [Critic & Review Board Sign-Off](#22-critic--review-board-sign-off)

---

## 1. Mission Purpose — What This Hour Is For

Mission 1 has **one job**:

> *Teach the player, with hands and feeling and not a single word of UI tutorial, that a memory orb is a person's life made into glass — and the Keeper polishes the glass with care.*

That's it. The whole 25–35-minute experience is built around making one warm, embodied moment happen: **the player holding Doris's First Loaves orb at the workbench, and watching the morning of 1972 become clear under their cursor.**

Every other system in the game — Cleanse, Weave, Sever, Tribunals, Memory Bees, the Letter-Bird Network, the predecessor mystery — hangs on the player having that one moment land. Mission 2 raises the stakes; Mission 1 is the cozy foundation.

### 1.1 What this mission is *not*

Mission 1 deliberately does not:

- ❌ Introduce a moral choice (Mission 2's job).
- ❌ Introduce the Cleanse mini-game (Mission 2's job).
- ❌ Reveal Marin's name as the previous Keeper (you see only the letter *M* on her pinned note).
- ❌ Teach the Echo Web (Mission 2's first activation).
- ❌ Show the herb garden (Mission 2's opener).
- ❌ Voice-act Doris's role in Margery's life (Mission 2's mid-act reveal via Gerrold).
- ❌ Have any failure state at all.

Mission 1 is the warmest, lowest-stakes onboarding the cozy genre has shipped. It exists to **earn the player's attention** for what comes next.

---

## 2. Mission Objective — In One Sentence

**Walk down a quiet autumn lane, meet a baker who has been keeping your shop's key, polish the memory she sells you, place it on a shelf, close a book, and sleep.**

There is no objective marker. There is no quest log. There is only what is in front of you.

---

## 3. Pre-Flight Checklist

Before you press **Play**, the following should be true:

| Item | How to verify |
|---|---|
| Unity 6 LTS (6000.4.4f1) is installed | Unity Hub → check version |
| Branch `feat/mission-1-2-architecture` is checked out | Git: `git status` should show this branch |
| The project compiled cleanly (no errors in Console) | Unity Console → confirm no red errors |
| The Phase 27 capstone ran | Menu: `Hearthbound → 🚀 Build Everything` |
| (Optional) Phase 26 diagnostic shows green | Menu: `Hearthbound → 🔍 Diagnose Build` |
| Build Settings opens with `00_Bootstrap.unity` as scene 0 | File → Build Profiles → Scenes In Build |
| Yarn Spinner installed *(optional, but you want it)* | Package Manager → Yarn Spinner present |
| URP-Mobile pipeline is active | Project Settings → Graphics → URP-Mobile asset |
| Audio plays in the Editor | Edit → Preferences → Audio → check enabled |

You are ready when the Bootstrap scene opens and the Console is clean.

> 💡 **Phase 32 update:** the row above for "Phase 27 capstone" now points at the single top-level entry `Hearthbound → 🚀 Build Everything` (a safety dialog confirms before the chain runs, ~60 s, idempotent). The diagnostic row points at the new top-level aggregate `🔍 Diagnose Build`. All per-phase items live under `Hearthbound → ⚙️ Advanced ►` for power users. See [`PROGRESS.md → Phase 32 — Menu collapse`](./PROGRESS.md).

---

## 4. The Mission 1 Cast

| Character | Role | Lines in M1 | Voice Acting Tier |
|---|---|---|---|
| **The Keeper (you)** | First-person/third-person silent protagonist. The Keeper is canonically nameless and ageless in M1-2 — they can be any gender, any age. Your dialogue choices are read but never voiced. | Many "choice" lines (read-only) | Silent in M1-2 |
| **Doris** | The village baker, 64. Your first customer. Voice signature: short sentences, bread metaphors, occasional Northern-rural cadence (*"aye," "summat," "the long bit of the year"*). | ~180 Yarn lines across 6 nodes | Tier B (~3 min audio) |
| **Pickle** | The Hollow's grey-tabby familiar cat. The previous Keeper's cat, left behind. Voice signature: sarcastic, dry, faintly telepathic. Speaks only to you (internally — Doris cannot hear her). | **1 line** in Mission 1 | Tier B (~10 seconds audio) |
| **The Silent Villager** | A procedural NPC sitting on a bench in the lane, reading a letter. Older man, flat cap. Never named in M1-2. Will return in Mission 4. | 0 lines (nods if waved at) | Silent |
| **Marin (off-screen)** | The previous Keeper. You see her apron, her pinned note (signed only *"— M."*), and three orbs she left as a welcome. **Her name is never spoken in M1.** | 0 lines (she is gone) | (Future content) |

---

## 5. Mission 1 Map

```
                                                  ┌────────────────────────────┐
                                                  │                            │
                                                  │      Doris's Bakery        │
                                                  │      (warm-glow window)    │
   ┌──────────────────────────────────────────────┤                            │
   │                                              │   ┌────────────────┐       │
   │  PLAYER SPAWN  ────►                         │   │  Doris on      │       │
   │  (lane edge)                                 │   │  stool here    │       │
   │                                              │   │  (kneading)    │       │
   │      ─ cobblestone ─ dirt path ─             │   └────────────────┘       │
   │      autumn trees (Zephyr wind)              │                            │
   │      lamp post at the bend                   │   [connecting door         │
   │      Lumen god rays (4)                      │     bakery → Hollow]       │
   │                                              │                            │
   │      [silent villager on bench, mid-lane]    └────────────────────────────┤
   │                                                                            │
   │      [side path at 65m → dormant beehive Easter egg]                      │
   │                                                                            │
   │      stone walls + autumn ivy on both sides                               │
   │      [Forge Path closed today — sign at far end]                          │
   └────────────────────────────────────────────────────────────────────────────┤
                                                                                 │
                                                                                 │
                              THE HOLLOW INTERIOR  (split into 2 rooms)         │
                              ─────────────────────────────────────             │
                                                                                 │
        UPSTAIRS (locked in M1-2):  ┌─────────────────────────────────┐         │
        wooden barrier marked       │                                 │         │
        "in repair"                 │      [staircase, blocked off]   │         │
                                    │                                 │         │
                                    └────────────────┬────────────────┘         │
                                                     │                          │
        THE SHOP ROOM (12m × 6m):   ┌────────────────┴────────────────┐         │
                                    │                                 │         │
                                    │  ┌─[counter]─┐   [shelf 1: 🟡] │         │
                                    │  │           │   [shelf 2: 🟣] │         │
                                    │  │ player    │   [shelf 3: 🤍] │         │
                                    │  │ enters →  │     ↑           │         │
                                    │  │           │  3 welcome orbs │         │
                                    │  └───────────┘    + 1 empty    │         │
                                    │                   slot for     │         │
                                    │                   DOR-001      │         │
                                    │                                 │         │
                                    │  [reading chair]    [stove ⛺] │         │
                                    │   (book on it)                  │         │
                                    │  [chair]      [windowsill ★]   │  ← Pickle's spot
                                    │  [chair]                        │         │
                                    │                                 │         │
                                    └────────────────┬────────────────┘         │
                                                     │ doorway                  │
        THE WORKBENCH ROOM (6m×5m): ┌────────────────┴────────────────┐         │
                                    │                                 │         │
                                    │   [workbench]                   │         │
                                    │   ┌──────────────┐              │         │
                                    │   │ [orb cradle] │              │         │
                                    │   │  + tools     │ [tea cabinet]│         │
                                    │   │  + Marin's   │ [kettle 🫖] │         │
                                    │   │   pinned     │              │         │
                                    │   │   note 📜    │              │         │
                                    │   │  + apron 👘 │              │         │
                                    │   │   on a hook  │              │         │
                                    │   └──────────────┘              │         │
                                    │                                 │         │
                                    │  [leather Ledger 📖 on desk]    │         │
                                    │                                 │         │
                                    │  [door to garden, locked in M1] │         │
                                    │                                 │         │
                                    └─────────────────────────────────┘
```

### 5.1 Scene boundaries

- The lane is **walled off** at both ends. You cannot walk past Doris's bakery in M1 (the Hollow is the destination), and you cannot walk past the *"Forge Path closed today"* sign at the far end.
- The Hollow's upstairs is **blocked by a wooden barrier** marked "in repair." This is Mission 5+ content.
- The Hollow's back door **to the garden** is locked in Mission 1. It opens for Mission 2.

---

## 6. Beat 1 — Bootstrap, Main Menu, Opening Cinematic

**Real-time: 0:00 → 5:00**
**Scene chain:** `00_Bootstrap` → `01_MainMenu` → `02_Mission01_Lane`

### 6.1 What happens

1. Unity loads `00_Bootstrap.unity` — you see almost nothing; the GameManager initializes services. Lasts ~2 seconds.
2. `01_MainMenu.unity` loads. The title appears with a soft amber wash. The composer's main theme enters at low volume.
3. **First launch only:** The **Tone Compass card** displays — a hand-painted illustration with Pickle in the corner and ~6 sentences in fiction-voice (see Overview § 7 for the full text). 90 seconds. **Skippable** by pressing any button after 1 second.
4. **First launch only:** A small Gentle Mode prompt appears. *"Would you like to play in Gentle Mode?"* with two clearly-labeled buttons. Auto-confirms to *off* after 4 seconds if untouched.
5. Main Menu buttons appear: **Open The Hollow** / **Continue** / **Settings** / **Credits** / **Quit**.
6. Click **Open The Hollow**.
7. A loading screen briefly shows the tip *"Some memories want to be sold. Some don't."*
8. The **Opening Cinematic** plays — ~3 minutes, hand-painted, Cutscene Engine driven. You see: a postal cart arriving at the village edge, a key being handed over, the lane being walked, Doris's bakery seen from a distance. Composer's main theme + atmospheric foley.
9. Cinematic ends with a black fade. The screen lightens into the lane scene at dusk.

### 6.2 Player choices in this beat

None. Listen. Look. Skip the cinematic with a long-press of Space if desired (a small fill-bar appears bottom-right).

### 6.3 The composer cue

The Tone Compass card plays a 15-second piano arrangement of the main theme under it. The Opening Cinematic uses the full Hearthbound main theme statement (the same one you will hear at the end of Mission 2). **You hear the theme three times in Mission 1**: opening, after the Polish, and at the Evening Ledger close. Listen for it.

---

## 7. Beat 2 — The Autumn Lane At Dusk

**Real-time: 5:00 → 11:00**
**Scene:** `02_Mission01_Lane.unity`

### 7.1 What you see

You spawn at the **far end** of an 80-meter lane. It is **17:30 in-fiction** — the very edge of sunset. The sun is low in the west; warm amber light bathes the cobblestones at the start of the lane and the bakery's two glowing windows at the far end.

**Atmosphere:**

- Light fog at low intensity (Stylized Weather System).
- Zephyr wind moving the autumn ivy on both side walls and the orange-red leaves of 5 oaks + 3 birches.
- 4 Lumen god rays angling through the tree gaps onto the path ahead of you.
- 8 fireflies dancing in the gloamier patches (3 on Mobile).
- A single distant rooster crows once, ~halfway through the walk.
- Two scripted wind gusts during the walk (not random; placed for cinematic feel).
- The composer's **main theme plays at 35% volume** for the first 90 seconds, then fades.

### 7.2 Geography & wayfinding

You will see Doris's bakery from the very first frame — its two windows are the warmest point of light on the screen, set at the far end of the lane. **The lighting is the wayfinding.** No floating arrow. No HUD marker. Walk toward the warmth.

The lane is a linear corridor — stone walls on both sides prevent you from wandering. There is one **side path** at the 65 m mark (see § 19.1 — the beehive Easter egg).

### 7.3 NPCs along the way

| NPC | Position | What they do | Player options |
|---|---|---|---|
| **The Silent Villager** | On a bench at the 40 m mark, reading a letter | Sitting + a subtle ACS "reading" upper-body animation. Looks up briefly at the player. | None. You can walk past. If you stop next to him, he gives a slight nod — but does not speak. |
| **Doris** | On a stool just outside the bakery door at ~78 m | Kneading a small dough on a wooden tray on her lap. ACS upper-body "kneading" loop. Looks up as you approach. | Talk to her (Beat 3). |

### 7.4 Audio palette

| Layer | Volume | Notes |
|---|---|---|
| Composer's main theme | 35% → fades to 0% after 90s | The first 90 seconds are scored |
| Autumn ambient (wind in dry leaves) | 60% | Looping |
| Distant village murmur | 20% | Muffled |
| Footsteps | Variable | Cobble at start, dirt mid-lane, wood at the bakery door — 8 unique foley clips per surface |
| One distant rooster | (one-shot) | ~halfway through |
| Bakery oven hum | 0 → 40% | Rises as you approach |
| Wind gusts | (two scripted) | Placed for cinematic feel |

### 7.5 What to do here

- **Walk slowly.** The autumn lane is one of the most beautiful 60 seconds of art in the vertical slice.
- **Examine the silent villager** by walking close. No interaction prompt appears — but if you stand within 1.2 m for ~2 seconds, his head turns and he nods. This counts as **acknowledged** for Mission 4+ purposes.
- **Take the side path** at 65 m (see § 19.1).
- **Walk to Doris** when ready. The interaction prompt *"Talk to Doris (E)"* appears within 2.5 m of her.

### 7.6 Approximate timing

| Sub-beat | Real-time |
|---|---|
| Spawn → silent villager | 1:30 |
| Silent villager → side path | 1:00 |
| Side path detour (optional) | 0:30 |
| Side path → bakery door | 1:30 |
| Pause at Doris | 0:30 |
| **Total Beat 2** | **~5:00** (with detour) |

---

## 8. Beat 3 — Meeting Doris Outside The Bakery

**Real-time: 11:00 → 13:30**

### 8.1 What you see

Doris is on her stool, kneading. She looks up. She does not stop kneading.

**Eyes Animator behavior (her gaze priority):**

| Priority | Target | Weight |
|---|---|---|
| 1 | Player (when within 2.5 m) | 0.7 |
| 2 | Orb in player's hand (none yet) | 0.5 (not active) |
| 3 | Counter | 0.3 |
| 4 | Random saccades | 0.1 |

She blinks ~0.15× per second. Her pupil dilation defaults to 0.5 — slightly nervous around the new Keeper.

### 8.2 Greeting dialogue

She speaks first:

> *"You're the new one. I thought you'd be taller."*
>
> *"Don't mind me — I thought that about the old one, too."* (1.5-second pause)
>
> *"Come in. The kettle's only just stopped."*

You then choose from three options:

| Option | What happens | VillageState impact |
|---|---|---|
| **A — "Hello. Are you Doris?"** | She confirms her name and gestures at the sign. *"Aye. The very same. They've put my name on the sign and everything."* | Cordial path. Default. |
| **B — *(Nod silently and follow her in)*** | *"A quiet one, then. Good. The bread likes quiet."* | Slightly raises her impression of you as a thoughtful Keeper. **Doris speaks slightly fewer lines after this** — she respects quiet. |
| **C — "Who was the old one?"** | She pauses. *"... Mm. That's a conversation for a longer day. Come in. Tea first."* | Sets `$asked_about_predecessor = true`. This flag is read in Mission 3 and Mission 6 — Doris will eventually tell you about Marin only if you asked here. **This is the only way to plant that thread in Mission 1.** |

> **Recommended path: Option C.** It costs nothing, opens a long-arc story thread, and is in-character.

### 8.3 Pickle's status during this beat

Pickle is **not** in the lane. She is inside the Hollow on the windowsill. You will not see or hear her yet.

---

## 9. Beat 4 — Entering The Bakery + The Hand-Off

**Real-time: 13:30 → 15:00**

### 9.1 The transition

Whichever first-greeting option you chose, the dialogue ends with Doris standing up, dusting flour off her apron, and gesturing for you to follow. She walks ahead of you to the connecting door between her bakery and the Hollow. The camera blends from third-person follow into a soft Cutscene Engine 8-second cinematic:

1. The bakery door opens.
2. Camera drifts inward.
3. You see the bakery's interior briefly — flour-dusted counter, oven on, hanging bread on a rack.
4. Camera continues through to the connecting door to the Hollow.
5. The connecting door is still closed. Doris is standing in front of it, holding a small wooden box.
6. **Pickle is revealed on the Hollow's windowsill** through a side window. She is sitting motionless, watching.
7. Camera settles into normal follow.

### 9.2 The wooden box

Doris pauses at the connecting door. She **does not enter the Hollow** in Mission 1 — this is intentional. She speaks:

> *"Mind the flour."*
>
> *"I haven't swept since Tuesday. I keep meaning to."*
>
> *"... The shop next door is yours. The Hollow."*
>
> *"I've been keeping the key safe for you."*

You choose one of two:

| Option | Response |
|---|---|
| **"Thank you."** | *"Of course."* |
| **"Is the shop ready?"** | *"It's clean. It's been waited-in. You'll find your way around."* |

Then she says:

> *"... I have something for you. Before you go in."*
>
> *"I'd like to be your first customer, if that's all right."*

You choose:

| Option | Response | Effect |
|---|---|---|
| **"I'd like that."** | *"Aye. That's why I want to be the first."* | Default — proceeds to the offer. |
| **"I haven't started yet."** | *"Aye. That's why I want to be the first. Otherwise you might come in expecting strangers."* | Same outcome; slightly warmer line. |

She unwraps a small wooden box on the counter. Inside: **a single faded glass orb, amber-fogged, glowing dimly.**

### 9.3 Doris's offer

She speaks the iconic line:

> *"This is the memory."*
>
> *"Hold it like you'd hold a hot bun. Not by the side. Underneath."*
>
> *"It's a small thing."*
>
> *"First time I made bread that didn't shame me."*
>
> *"Most days I think of it."*
>
> *"I want to put it down, now, for a while."*
>
> *"Will you take it?"*

You choose:

| Option | Path |
|---|---|
| **A — "I will. What do you want for it?"** | Proceeds directly to price negotiation (Beat 5). |
| **B — "Tell me more about it first."** | Doris speaks a beautiful 5-line aside about her age (24), the new oven, the nine years of baking other people's bread, the first morning that was just hers. Then proceeds to price negotiation. **Recommended for the cozy register.** |
| **C — "I'd rather not, today."** | Doris responds: *"Aye. Some days are not the day. I'll be here when one is."* Sets `$refused_doris_orb = true`. **The Hollow door still unlocks; Mission 1 continues without an orb.** The Polish mini-game does not play. This is the **refusal path** — a valid choice, with consequences. |

> **Recommended path: Option B.** The aside is one of the cozy game's quiet writing peaks, and it costs nothing.

### 9.4 The refusal path (Option C)

If you refused, Mission 1 takes a quiet alternate route:

1. Doris steps aside; opens the Hollow door for you.
2. You enter the empty shop. Pickle is on the windowsill.
3. You can examine every prop (Beats 6–7), read Marin's note, etc.
4. The workbench has **no orb** to polish. You may interact with the cradle but nothing happens.
5. The Evening Ledger opens with a **different Day 1 prose**:
   > *"Day 1 — Spire-Month, Week 1. You arrived at the Hollow. The door was unlocked. The kettle was warm. Doris, the baker, offered her First Loaves. You declined, this evening. She did not seem to mind. The shop is still yours. Some days are not the day."*
6. **Memory Dream 1 does not play** (no orb in inventory means no dream).
7. Day 2 begins normally — Doris and Gerrold both arrive at the bakery and Hollow respectively.

This path costs you Mission 1's emotional crest, but **Doris will return Day 4 or Day 5** with the same orb if you want to try again. The cozy contract holds.

---

## 10. Beat 5 — The Price Negotiation

**Real-time: 15:00 → 16:30**

Assuming you accepted Doris's orb (Options A or B above), the price screen now appears.

> *"Four coppers, if you're asking. It's a small memory. I'll not have you overpay your first day."*

You have three choices:

### 10.1 The three options

| Option | What you do | Coin cost | VillageState impact | Doris's reaction |
|---|---|---|---|---|
| **A — "Four coppers is fair."** *(Honor the price)* | Accept exactly | -4 coppers (you have 10 to start; ends at 6) | `$vow_5_honest_coin += 2` | *"Aye. Thank you."* — neutral cordial |
| **B — "I'll pay six."** *(Counter-offer high)* | Offer more than asked | -5 coppers (she rejects 6, settles at 5) | `$vow_5_honest_coin += 3`, `$trust_doris += 1` | *"That's too much. I'll not have you ruin yourself. Take it back. — Well. Take *some* back. Five, then. Final."* |
| **C — "Two coppers. That's all I have."** *(Underpay)* | Offer less than asked | -2 coppers (she accepts the partial; you owe her 2) | `$vow_5_honest_coin -= 1`, `$doris_owes_player = -2` (you owe her), `$trust_doris += 0` (she does not begrudge) | *"... Aye, that'll do. Bring the rest when you find some."* |

### 10.2 What each path means

- **Option A (Honor the price)** is the **default cozy path.** It is the most respectful and the most appropriate to your relationship with Doris — she set the price, you trusted her judgment.

- **Option B (Pay more)** is the **generous path.** Doris's reaction is delightful — she actively prevents you from overpaying, settling at 5 instead of accepting 6. **This is the deeper Vow 5 (Honest Coin) move** — the vow asks you to pay what the memory is *worth*, and Doris reminds you that overpaying is also a form of dishonesty.

- **Option C (Underpay)** is the **debt path.** Doris accepts gracefully. This creates a cozy running thread — you owe Doris 2 coppers, and the Mission 3 dialogue has a beat where she gently mentions it. Some cozy players adore this path because the debt becomes a small relationship-binding fact. It is not "wrong" — it just creates a different shape of Mission 3.

> **Recommended path: Option A** if you want the canonical first-day experience. **Option B** if you want the warmest Doris relationship. **Option C** if you want the cozy debt-thread to play out across Mission 3.

### 10.3 The hand-off animation

Whichever you choose, Doris triggers her **ACS Offer_Box** animation (a 2-second one-shot of her holding the wooden box forward in two hands, gentle pace, warm body language). She says:

> *"There. The old keeper showed me how to make it. Took me four tries. I cracked the first three. The cat watched me. Judged me, I think."*

(This is the first hint that **Pickle has been here for years and watched the previous Keeper teach Doris.**)

> *"I'll be in the bakery if you want me. Knock twice."*
>
> *"There's a kettle on the workbench. Mind the wood stove — it bites."*

She turns and walks back into the bakery. **The Hollow door is now unlocked.** Push it open.

---

## 11. Beat 6 — Inside The Hollow (The Predecessor Seeds)

**Real-time: 16:30 → 20:30**

This is **the player's first private cozy moment with the game.** Doris is gone. Pickle is on the windowsill. The orb is in your inventory. You are alone with the shop.

The Mission 1 Hollow has been *waited-in* — that is Doris's word for it. The previous Keeper, Marin, left dozens of small details. Every prop is examinable. Every examination is one fiction-voiced line of codex tooltip.

### 11.1 The shop room — what to examine

Walk into the shop room first. Press **E** near each glowing prop (a faint warm aura outlines interactables).

| # | Prop | Where | Codex tooltip when examined |
|---|---|---|---|
| 1 | **The apron on a hook** | Behind the workbench (visible from the shop room through the doorway) | *"A long brown apron. Pockets stuffed with old receipts. The collar is frayed. Whoever wore this stood at this bench every day for years."* |
| 2 | **The bee orb** *(welcome gift 1, on shelf 1)* | Shelf 1 (top) | *"A single bee suspended mid-flight inside a glass orb. The bee is calm. It has been calm for at least three years. The orb hums very faintly when you stand close to it."* |
| 3 | **The kettle orb** *(welcome gift 2, on shelf 2)* | Shelf 2 (middle) | *"An orb that contains the sound — only the sound — of a kettle just before it boils. Holding the orb makes the room feel warmer by half a degree."* |
| 4 | **The empty chair orb** *(welcome gift 3, on shelf 3)* | Shelf 3 (bottom) | *"An orb that appears, when you look at it closely, to contain a single wooden chair. No one is in the chair. The chair is in late afternoon light. You set it down quickly."* |
| 5 | **The open book on the reading chair** | The reading chair beside the window | *"A book of regional folklore, open to the chapter on 'the lampman.' The bookmark is a pressed yellow leaf. The leaf is dry but the page is still slightly damp where a thumb rested."* |
| 6 | **Pickle** | The windowsill (do not press E — just stand within 1.5 m for 3 seconds) | *(no codex entry — her purr loops audibly at volume 0.3; you may stand for as long as you wish)* |

**Examining all 6** raises your Predecessor Trail Warmth by +3 (per prop). It does not unlock anything in M1-2, but it sets up Mission 4's Marin Reveal.

### 11.2 The three welcome orbs — what they really are

These three orbs are **inert** in M1-2 — you cannot Polish, Cleanse, or otherwise interact with them mechanically. But they are **promises** the game makes:

- **The bee orb** → Doris's untold story (the Wedding Honey arc) — Mission 3+
- **The kettle orb** → the Hollow's ambient warmth — Mission 4+ (Marin's signature)
- **The empty chair orb** → the predecessor's loss — Mission 6+

The cozy player who returns to the Hollow in Mission 4, Mission 7, Mission 12 will see these three orbs recontextualized. **Do not move them. They are exactly where Marin placed them.**

### 11.3 What you cannot do here

- **Upstairs** is blocked by a wooden barrier. Examining it: *"A wooden barrier marked 'in repair.' The staircase behind it leads up. You will need to wait."*
- **The back door to the garden** is locked. Examining it: *"A small door. The garden is on the other side. The hinge is stiff. It will open tomorrow morning."*
- **The reading chair** can be examined (the book) but not sat in. Mission 8+.

### 11.4 Pickle on the windowsill

Approach her. **Do not press E.** Pickle does not interact in M1 — she is observing.

Her **Eyes Animator** behavior:
- Look-at priority: Player (0.7) → orb-in-your-hand (0.95 if you carry one) → outside-window (0.4) → random saccades.
- Blink rate: ~0.18/sec.
- Pupil dilation: 0.55 (mildly engaged).

If you stand within 1.5 m for 3+ seconds, her head turns toward you. Her tail flicks once. Her purr starts looping at volume 0.3 in your area. **This is correct. This is cozy.**

### 11.5 The recommended order

1. Walk in. Stand still for 3 seconds. Let the room settle.
2. Walk to the windowsill. Stand near Pickle for ~5 seconds. Listen to the purr.
3. Examine the three welcome orbs left-to-right.
4. Examine the book on the reading chair.
5. Examine the apron on the hook (visible through the doorway).
6. Walk into the workbench room (Beat 7).

---

## 12. Beat 7 — The Workbench + Marin's Note + Pickle

**Real-time: 20:30 → 22:00**

Through the doorway into the workbench room. You will see:

- The **workbench** front-and-center.
- An **orb cradle** in the workbench center (an empty wooden depression sized to hold a single orb).
- A **pinned note** above the workbench at eye level.
- The **kettle on the stove** to the right.
- The **leather Ledger** at the corner of the desk.
- The **back door to the garden** (locked).

### 12.1 Marin's pinned note — the only "tutorial" in the game

Examine the note. It reads (in cozy handwriting font):

> *"Polish in slow circles. The faster you press, the less the orb hears you. Cover all sides. Most memories need ninety seconds. — M."*

That is the **entire tutorial** for the Polish mini-game. No UI overlay. No floating "Press LMB" prompt. Just Marin's voice from a year and a half ago.

> ⚠️ **In Mission 2 the note has a second paragraph that will become visible when you hold a cracked orb.** Ignore that for now — it is M2 content.

### 12.2 What's on the workbench

- **Orb cradle** (empty — you will place the orb here in a moment).
- A small leather tool roll (chisels, polishing cloths — examinable but not interactive: *"A leather roll of polishing tools. They are clean. They have been used so many thousands of times the wood is dark where Marin's thumb sat."*).
- The Ledger book at the corner — examinable: *"A leather-bound book. The previous pages have been torn out. The Keeper before you left you a clean book."*
- A small dish of dried tea leaves (not interactable in M1; Mission 2's tea brewing uses these).

### 12.3 Placing the orb in the cradle

Walk up to the workbench with the orb in your inventory. The interaction prompt *"Place orb on cradle (E)"* appears. Press E.

- The orb floats softly from your inventory icon (top-right) down to the cradle.
- The room lighting **subtly warms by 5%** (the orb is contributing to the ambient).
- A faint **hum** begins — the orb's leitmotif at volume 0.4 (`polish_hum_start`).
- The **camera transitions** to a Cinemachine close-up of the orb in the cradle, with the workbench corner visible and the kettle steam in the background.
- The orb visibly **glows amber-fogged.** You can barely make out a kitchen scene behind the glass.

You are now ready to Polish.

### 12.4 The Auto-Complete toggle

Before you do anything else, **note the bottom-right corner of the workbench frame.** A small Bamao parchment button labeled **"Polish for me"** is visible from the very first frame.

- Click it to skip the manual mini-game.
- Auto-complete plays a **12-second sped-up animation** of the same Polish operation. Same warm visual, same Doris-motif cue, same outcome.
- **The narrative consequence is identical** — you cannot "miss" Doris's after-polish reaction by Auto-Completing.

If you want the full cozy ritual, ignore the button. If you are tired, use it. Both are first-class.

---

## 13. Beat 8 — The Polish Mini-Game (Full Walkthrough)

**Real-time: 22:00 → 23:30** (mini-game itself: 70–90 seconds)

The cozy game's signature mechanic. **You cannot fail this.** Worst case, you stop and walk away — the orb stays at whatever clarity you got to and you can return.

### 13.1 The verb in detail

You are **removing age-faded amber-fog from the surface of a glass memory orb** by moving the cursor in slow circles over the orb. As the fog clears, the **memory scene becomes visible** behind the glass.

The set-piece behind Doris's orb is **a 1972 kitchen at first light** — a faded illustration of her bakery the morning it opened. You will see, through the clearing fog: an oven, fresh loaves on a rack, pale gold sun through unwashed windows, and Doris-at-24 from behind, wiping her hands on her apron.

### 13.2 How to play (manual mode)

| Step | Action | What happens |
|---|---|---|
| 1 | **Hold left mouse button** anywhere on the orb | The cursor "engages." A faint glow follows it. The `polish_rub_start` foley plays (one-shot). |
| 2 | **Draw slow, even circles** | A clear patch appears where you draw. The patch expands with each pass. The `polish_rub_loop` foley plays (looping). |
| 3 | **Cover all quadrants** of the orb (rotate the camera with right-mouse-drag to see the back) | The shader supports 360° clarity tracking. **Polishing both sides** clears the orb evenly. |
| 4 | **Slow down if you hear `polish_rub_friction_warn`** | This SFX plays if you draw too fast. It does not punish — it just tells you the orb "hears you less" at high speed. |
| 5 | Continue until the orb is fully clear | The kitchen scene becomes fully visible behind the glass. Composer's Doris motif swells. |

### 13.3 The shader-state timeline (what happens at each milestone)

| Clarity | Time (approx) | Visual | Audio |
|---|---|---|---|
| 0.40 | 0:00 | Orb fogged; barely-visible scene behind | `polish_hum_start` |
| 0.42 | 0:05 | Tiny clear patch at cursor | `polish_rub_start` |
| 0.55 | **0:25** | ~30% clear; Doris's young hands appear behind. **Pickle's head turns toward the orb** (Animator trigger). | `polish_midway_chime` (one soft bell) |
| 0.70 | 0:45 | Kitchen scene clearly visible — oven, loaves, sunlight | `polish_rub_loop` continues + faint kettle hiss |
| 0.85 | 1:05 | Almost clear. **Composer's Doris motif enters fully** (~3-second swell). | `polish_reveal_swell` |
| 0.95 | 1:15 | Final 5% auto-clears. Orb glows brightly. | `polish_success_jingle` |
| 1.00 | 1:18 | Polish complete. Orb sits in cradle, glowing softly. **Pickle's tail flicks once** (approval). | `polish_hum_post` (gentler loop) |

### 13.4 Outcomes by approach

| Your behavior | Resulting clarity | Doris's after-polish line |
|---|---|---|
| Manual, slow + thorough (covered all quadrants) | 1.00 — **Perfect** | *"Aye. There it is. That's the morning. You did it cleaner than I remembered it. I think you'll do."* +`$trust_doris += 3` |
| Manual, decent coverage | 0.85 — **Acceptable** | *"Aye. There it is. That's the morning. You did it kindly. That's what matters."* +`$trust_doris += 2` |
| Manual, partial (walked away early) | 0.45–0.70 — **Mild** | *"... It's the morning still. A little dimmer. But mine. First days are like that. I won't hold it."* +`$trust_doris += 1` |
| Auto-Complete | 1.00 — **Perfect** (same as careful manual) | Same as Perfect path. No "skill bonus" exists; the warmth is the same. |

### 13.5 Pickle's after-Polish line

**The only Pickle line in Mission 1.** Plays after `polish_success_jingle`. Italic font, lower opacity, no Bamao box (just floats over the workbench):

> *"That was — adequate. I was expecting much worse. Continue, please."*

Her tail flicks once. The cozy player will laugh.

### 13.6 Adaptive difficulty — first-time tutorial tier

Mission 1's Polish is at the **lowest difficulty tier**:

- All clarity-gain coefficients are at their highest values.
- Even random cursor movement clears the orb in ~110 seconds.
- The friction-warn threshold is generous.
- Pickle's hint plays only if you struggle >30 seconds without clearing any of the orb (and her hint is simply: *"Slower."*).

This is the **tutorial-tier polish**. Subsequent polishes in Mission 3+ get tighter; Doris's First Loaves is the gentlest.

### 13.7 Best practices

- **Slow down.** Counter-intuitive for action-game players, but the friction-warn is real.
- **Rotate the orb.** Use right-mouse-drag to spin the camera. Polishing both sides earns the Perfect tier.
- **Watch the reveal.** The orb visibly clears under your cursor. The kitchen scene appears in real-time. **This is the cozy game's signature moment** — do not look away.

### 13.8 If you Auto-Complete

The 12-second cinematic plays. You watch the same orb clear via the same animation, just at 6× speed. Doris's motif still swells. Pickle still flicks. The Yarn branches identically. **Zero penalty.** It is the same warmth, distilled.

---

## 14. Beat 9 — Placing The Orb On The Shelf

**Real-time: 23:30 → 24:30**

The Polish is done. The orb glows softly in its cradle. You can:

| Option | What it does |
|---|---|
| **Pick up the orb** (E near cradle) | The orb returns to your inventory. The camera widens back to standard follow. You may walk it to a shelf. |
| **Place orb on shelf 1, 2, or 3** | An interaction prompt appears at each shelf's empty slot. *"Place DOR-001 here (E)."* Press E. The orb floats from your inventory into the slot and stays. |

The three shelves each already have one of Marin's welcome orbs. Each has **one empty slot**:

- Shelf 1 (top) — beside the bee orb
- Shelf 2 (middle) — beside the kettle orb
- Shelf 3 (bottom) — beside the empty chair orb

**Where you place DOR-001 has no mechanical effect** in Mission 1-2. There is a small cozy easter egg: if you place DOR-001 *beside the bee orb*, the next time you examine the bee orb (in Mission 3+) you get a slightly warmer codex tooltip referencing Doris by name.

> **Recommended: Place it on Shelf 1 beside the bee orb.** Thematic alignment.

### 14.1 The shelf is no longer empty

When the orb settles in its slot:

- A soft *click* foley plays.
- The shop's overall ambient hum changes — there is now one more orb humming in your shop. The room feels measurably warmer.
- The Mission 1 objective is, in fiction, complete.

---

## 15. Beat 10 — Returning To Doris (Three Outcome Lines)

**Real-time: 24:30 → 26:00**

You may exit the Hollow and walk back through the connecting door into the bakery. Doris is on her stool again, kneading a fresh batch of dough.

Approach her. The interaction prompt *"Talk to Doris (E)"* appears.

### 15.1 The after-polish dialogue (branches by your polish quality)

She looks up. She smiles wider than before. Her line varies by `$polish_quality`:

| If you achieved... | Doris's line | Trust gain |
|---|---|---|
| **Perfect** | *"Aye. There it is. That's the morning. You did it cleaner than I remembered it. I think you'll do."* | `$trust_doris += 3` |
| **Acceptable** | *"Aye. There it is. That's the morning. You did it kindly. That's what matters."* | `$trust_doris += 2` |
| **Mild** | *"... It's the morning still. A little dimmer. But mine. First days are like that. I won't hold it."* | `$trust_doris += 1` |

She then continues with the same final line regardless of outcome:

> *"Sleep tonight. Dreams come."*
>
> *"I'll see you again, eventually."*

She turns back to her dough. **She does not say goodbye.** This is the cozy game's understated register — Doris's voice fades and she returns to the work of being a baker. You walk out.

### 15.2 What you don't see

Two important things happen as you walk back to the Hollow:

- **Pickle has not moved.** She is still on the windowsill. She will move only once in the entire Mission 1 (during the Polish, when her head turned).
- **The Hollow's air pressure changes.** The orb on the shelf is now humming. The room is full of memory. You may pause and listen.

---

## 16. Beat 11 — The Evening Ledger (First Save)

**Real-time: 26:00 → 28:30**

Walk to the workbench. The leather-bound Ledger on the desk is now glowing softly. The interaction prompt *"Open the Evening Ledger (E)"* appears.

### 16.1 The Ledger UI

Press E. The book opens in a Bamao open-book frame with two pages.

**Left page — Day 1's transactions in fiction-voice:**

> *Day 1 — Spire-Month, Week 1*
>
> *You arrived at the Hollow this evening. The door was unlocked. The kettle was warm.*
>
> *Doris, the baker, was your first visitor. She offered her First Loaves — a memory from the morning her bakery opened, fifty years ago. You polished it. You did your work. She seemed pleased.*
>
> *Pickle, the cat who lives in the shop, was on the windowsill. She did not move. She is watching you.*
>
> *The shop is quiet. The shelf is no longer empty.*

(If you refused the orb, this prose is replaced with the refusal variant — see § 9.4.)

**Right page — open work:**

> *Open work:*
>
> *— Doris's First Loaves is on the shelf. You may return to it any morning.*
> *— The shop has three more empty shelves. The Keeper before you left them.*
> *— There is a locked room upstairs. The door is heavy.*
> *— There is a man at the lane's bend, walking with a letter. You did not speak to him.*
>
> *Hollow Level 1 — the shop is open for business.*

**Bottom of left page — the Seven Vows:**

> *The Seven Vows*
>
> *[ glyph: ⊙ ] Consent — kept*
>
> *[ glyph: ⊙ ] Return — untested*
>
> *[ glyph: ⊙ ] Whole — kept*
>
> *[ glyph: ⊙ ] Quiet Glass — untested*
>
> *[ glyph: ⊙ ] Honest Coin — kept*
>
> *[ glyph: ⊙ ] Open Door — untested*
>
> *[ glyph: ⊙ ] Last Light — untested*

All glyphs are softly lit (Vows 1, 3, 5 brighter for the ones you exercised today).

### 16.2 What you can do here

| Interaction | Effect |
|---|---|
| **Hover a transaction line** | The line expands with the memory's full prose in a side-panel. |
| **Click a Vow glyph** | Marin's hand-written reflection appears: *"Consent — the first vow. The hardest to remember during a long day. — M."* |
| **Examine an "Open work" thread** | The thread expands to ~2 sentences of context. |
| **Pet the inkblot of Pickle on the right page** | A real-life cat purr plays briefly. Pickle (in the shop) briefly tilts her head. Easter egg. |
| **Flip back a page** | (No previous pages exist yet — this is Day 1.) |

### 16.3 Closing the book (the save)

Press **Close the book**. Then:

1. The book closes audibly (cloth on paper).
2. Pickle, on the windowsill, **stretches** (her first body movement since you entered).
3. Composer's main theme enters at 25% volume.
4. Screen begins fading to amber.
5. The save runs silently in the background — your first manual save lands in Slot 1.
6. Memory Dream 1 begins (Beat 12).

### 16.4 What the save contains

A ~3 KB JSON in `Application.persistentDataPath/saves/HearthboundSave_Slot1.json`:

```json
{
  "save_version": 1,
  "playthrough_id": "uuid-...",
  "in_game_day": 1,
  "in_game_month": "spire",
  "village_state": {
    "trust": { "doris": 3 },
    "memory_integrity": { "DOR-001": 100 },
    "vow_integrity": {
      "consent": 51,
      "return": 50,
      "whole": 53,
      "quiet_glass": 50,
      "honest_coin": 52,
      "open_door": 50,
      "last_light": 50
    },
    "pickle_approval": 52,
    "coin": 6,
    "cinder": 0,
    "hollow_level": 1,
    "memories_in_inventory": ["DOR-001"],
    "first_moral_choice_made": false,
    "echo_web_connections_activated": []
  },
  "mission_state": {
    "current_mission": "Mission01_Complete_Transitioning",
    "missions_completed": ["Mission01"]
  },
  "settings": {
    "gentle_mode": false,
    "auto_complete_polish": false,
    "auto_complete_cleanse": false,
    "...": "..."
  }
}
```

(Exact values vary by your choices.)

---

## 17. Beat 12 — Memory Dream 1 (Witness Mode)

**Real-time: 28:30 → 29:30 (or longer if you linger)**

After the Ledger closes, the screen has faded to amber. The **2.5-second Sleep Transition** plays:

| Time | What plays |
|---|---|
| 0.0s | Ledger book gently closes (audible) |
| 0.2s | Shop's audio softens to ~30% |
| 0.5s | Camera tilts upward toward the Hollow's ceiling, finding a single beam of moonlight |
| 0.9s | Pickle stretches and curls (visible) |
| 1.2s | Screen begins fading to warm amber |
| 1.8s | Audio fully silent |
| 2.5s | Amber fade completes; dream's first frame appears |

### 17.1 The dream itself — *The First Loaves*

A 60-second hand-painted short film. **You cannot control any of this.** Witness mode.

Set-piece: **Doris's first bakery, a 1972 kitchen at first light.** 4-layer parallax painting, hand-painted, ~$5,500 of senior illustrator art. Pale gold sun through unwashed windows. An oven hotter than she trusts. Loaves on a rack. A small wooden table.

| Time | What you see |
|---|---|
| 0:00–0:03 | Fade in from black. Title card *"The First Loaves"* on a Bamao parchment frame, lower-third. |
| 0:03–0:10 | Wide establishing shot of the kitchen, slow drift inward. Empty. The composer's main theme has not entered yet. |
| 0:10–0:15 | Young Doris (24) appears — fade in, standing at the oven, back to the camera. ACS animation: "checking oven through slot." |
| 0:15–0:25 | Doris pulls the loaves out, sets them on the rack. Camera at medium-close on her hands. *Composer's Doris motif begins — solo cello.* |
| 0:25–0:35 | Bread crust crackle foley. Steam wisps (VoluSmokeFX) rise from the loaves. Flour dust visible in the sun shaft. |
| 0:35–0:50 | Doris wipes her hands on her apron. ACS animation. *She does not yet smile.* She looks at the door. The door does not open. |
| 0:50–0:55 | She stands still, watching the door, alone. |
| 0:55–0:60 | A subtle slow smile begins to form. Camera pulls back to wide. Doris is small in the middle of her bakery. The door visible. Composer's motif resolves on a held, unresolved-but-warm sustain. |
| 0:55–0:60 | Title card subtitle: *"— 1972"* (small, lower-third). |
| 0:60 | Fade to amber. |

### 17.2 What this dream achieves

- It is the cozy game's **first emotional payoff**. The player has earned it through 28 minutes of careful attention.
- It binds the polished orb to a felt scene. **The next time the player looks at DOR-001 on the shelf, they will see this kitchen behind the glass.**
- It introduces the Memory Dream system without explanation. The player simply has the experience; the system is invisible.

### 17.3 If you linger

**Long-press** Space (instead of tap) during the dream. The dream plays at +50% length in slow motion. Many cozy players find this Memory Dream the most worth lingering on — its single moment of Doris's slow smile is rare in cozy games of this scale.

### 17.4 The morning wake

After the dream ends (60 or 90 seconds), the **3-second Wake Transition** plays:

| Time | What plays |
|---|---|
| 0.0s | Dream's final frame holds. |
| 0.5s | Screen fades to bright morning gold. |
| 1.4s | A single bird outside the Hollow's window. |
| 1.8s | Pickle's stretch + tail-flick. |
| 2.2s | Day 2's morning cue enters softly (Doris's motif faintly heard from her bakery). |
| 3.0s | The Hollow interior renders fully. **Mission 2 begins.** |

You are now on Day 2 — the morning of *The Widower's Request*. **Mission 1 is complete.**

---

## 18. Mission 1 Outcome Table

### 18.1 Possible end-states by your choices

| Choice | Path | VillageState after M1 (vs default) |
|---|---|---|
| **Refused Doris's orb** | Refusal path | `trust_doris = 2`, no DOR-001 in inventory, no dream played. Vow 1 (Consent) +2 (you respected her right to keep the memory). |
| Took orb + **Honored price (4 cu)** + Manual Polish Perfect | Canonical optimal | `trust_doris = 5`, DOR-001 (clarity 100) on shelf, `coin = 6`, Vow 5 = 52, Vow 3 = 53, Pickle Approval +2 |
| Took orb + **Paid 5 cu (offered 6)** + Manual Polish Perfect | Generous canonical | `trust_doris = 6`, DOR-001 (clarity 100) on shelf, `coin = 5`, Vow 5 = 53, Vow 3 = 53, Pickle Approval +2 |
| Took orb + **Paid 2 cu (underpaid)** + Manual Polish Perfect | Debt path | `trust_doris = 4`, DOR-001 (clarity 100) on shelf, `coin = 8`, Vow 5 = 49 (-1), Pickle Approval +1, `doris_owes_player = -2` (you owe her) |
| Took orb + Honored price + Auto-Complete Polish | Convenience path | Identical to canonical Perfect — Auto-Complete has no penalty. |
| Took orb + Honored price + Manual Polish Acceptable | Hurry path | `trust_doris = 4`, DOR-001 (clarity 100) on shelf, Vow 3 = 52 instead of 53, Pickle Approval +1 |
| Took orb + Honored price + Manual Polish Mild | Walked-away-early | `trust_doris = 3`, DOR-001 (clarity 70) on shelf, Vow 3 = 51, Pickle Approval +0 |

### 18.2 What carries forward to Mission 2

| Variable | Used in Mission 2 |
|---|---|
| `$trust_doris` | Doris's morning greeting on Day 2 is warmer at higher values. |
| `$polish_quality` | Marin's note's expanded paragraph mentions "the careful hand you showed yesterday" at Perfect tier. |
| `$asked_about_predecessor` | If true, Doris's Mission 2 morning includes a 2-line aside about Marin. |
| `$pickle_approval` | Affects whether Pickle commentary plays at the M2 moral choice. |
| `$coin` | Determines whether you can afford the M2 walk-to-cottage Lavender option (no, the cottage and tea are free; coin is purely cosmetic in M1-2). |
| `$predecessor_trail_warmth` | Sets up Mission 4's Marin Reveal eligibility. |

---

## 19. Hidden Details & Easter Eggs

### 19.1 The dormant beehive (side path at 65 m)

At the 65-meter mark of the lane, an unmarked side path branches right ~15 meters to the back of Doris's bakery. The detour adds 30–60 seconds. There you will find:

- A single dormant beehive.
- A single faint bee animation (slow loop, one bee).
- Examinable: *"A dormant beehive. The wood is old. The varnish has not been refreshed in two summers. Someone loved this once."*

This seeds Mission 3+'s **Wedding Honey arc** — the discovery that Doris's late husband Elric was a beekeeper, and that Doris keeps the bees in his name. **The cozy player who notices the hive will theorize**; the cozy player who doesn't will not be punished. Neither path is required.

### 19.2 The silent villager's letter

Approach the bench. The silent villager is reading. If you stand close (~0.8 m) and rotate the camera to look at his hand, the letter is visible but unreadable. **In Mission 4+, the same villager returns to the lane and you can talk to him — he will reference the letter he was reading "on the day you arrived."** Plant the recognition now.

### 19.3 The book on the reading chair

The book of regional folklore is open to *"the chapter on the lampman."* The **lampman** is a folk figure in this world — a man who carries a lantern through the village every night, lighting other people's windows after their kettles have gone out. **He will appear in Mission 7+** in a Memory Dream. The book's open page is a seed.

### 19.4 Pet Pickle's inkblot

On the Ledger's right page, the small inkblot drawing of Pickle in her current sleeping location. **Click on it.** A real cat purr plays. The Pickle in the shop (still on the windowsill) briefly tilts her head — *as if she sensed the petting.* The cozy game's small magic.

### 19.5 The apron's pocket

Examine the apron on the hook closely (walk right up to it). The codex tooltip mentions "pockets stuffed with old receipts." **In Mission 5+** you will be able to *reach into* the pocket and find a single tax receipt for *"M. Halloran"* — your first hint at the predecessor's full name. (Mission 1-2 only shows the apron; no reach-in interaction yet.)

### 19.6 The kettle that has not been cold in 22 years

Examine the teapot in the tea cabinet. The codex tooltip: *"This teapot has not been cold in twenty-two years. The Keeper before you trained the wood stove to do that. It is one of the village's small miracles."* **The implication:** Marin set up a slow-burn perpetual stove fire. The cozy player who reads this will theorize about how that's possible (Mission 6+ will explain).

### 19.7 The "Forge Path closed today" sign

At the far end of the lane, a small sign on a stone wall: *"Forge Path closed today."* **The Forge** is the village's smithy — Mission 4+ unlocks it. The cozy promise of more world is encoded in a single prop.

### 19.8 Two scripted wind gusts during the walk

The composer's main theme is **timed** to the wind gusts. Listen — the gusts arrive during specific quiet moments in the main theme, dipping under the music for atmosphere. This is sound-design craft, not noise.

---

## 20. Accessibility Paths

| Tool | What it does in Mission 1 |
|---|---|
| **Gentle Mode** | Sprint + Jump disabled. Polish difficulty stays the same (it cannot fail anyway). Doris's dialogue is slightly softened in two places. Pickle's after-polish line is warmer: *"That was — actually quite good. Continue."* (instead of "adequate"). |
| **Auto-Complete Polish** | 12-second cinematic instead of manual mini-game. Same outcome. |
| **Subtitle Size: Large or Huge** | All Yarn lines render in larger TMP. Doris's bread metaphors still hit. |
| **Color-blind Mode (Deuteranopia)** | The orb's amber-fog and the kitchen-reveal use a different palette pairing. Test before relying on it. |
| **Reduce Particle Intensity** | Halves fireflies in the lane and dust motes in the dream. |
| **Reduce Screen Flash** | The bright amber moment at Polish-success is reduced to a soft warm glow. |
| **Dyslexia-friendly Font** | UI switches to OpenDyslexic. Codex tooltips become much more readable. |
| **One-hand Controls** | Everything in Mission 1 is reachable single-handed. The Polish mini-game's circular motion works with right-stick alone on gamepad. |

---

## 21. What You Carry Into Mission 2

When Day 1 ends, the morning wake begins Mission 2. You carry forward:

| Item | State |
|---|---|
| **DOR-001 — The First Loaves** on shelf | Polished, clarity 100 (or 70/85 depending on quality), placed in your chosen shelf slot |
| **Coin** | 6, 5, or 8 (depending on price-negotiation choice) |
| **Trust with Doris** | 3–6 depending on path |
| **Pickle Approval** | 51–53 |
| **Vow 1 (Consent)** | 51 |
| **Vow 3 (Whole)** | 51–53 |
| **Vow 5 (Honest Coin)** | 49–53 |
| **Vows 2, 4, 6, 7** | Untouched, default 50 |
| **Flag: `$asked_about_predecessor`** | True if you asked Doris in Beat 3 |
| **Flag: `$first_moral_choice_made`** | False — you have not yet made one |
| **Predecessor Trail Warmth** | +3 to +9 depending on how many predecessor seeds you examined |

You will arrive at Mission 2 with a single orb on the shelf, a faint understanding of the Vow system, knowledge of Pickle's existence, and a half-glimpse of Marin. **All of this Mission 2 will leverage.**

---

## 22. Critic & Review Board Sign-Off

| Reviewer | Verdict | Note |
|---|---|---|
| **🎬 Creative Director** | ✅ Approved | "The 'one warm moment' Mission 1 thesis is preserved. The walkthrough does not over-explain — it lets the player infer when the design wants them to infer. Doris's voice is intact throughout the guide." |
| **🗺️ Game & Level Designer** | ✅ Approved | "All 12 beats align with the scene-spec in Focus 03 and the dialogue-spec in Focus 01. The map matches the Hollow's two-room floor plan. Timing estimates are realistic for cozy unhurried pace." |
| **📈 Trend & Ideation Analyst** | ✅ Approved | "The 'refusal is a valid path' framing (§9.4) is exactly the cozy-game discipline that r/CozyGamers asks for. The Easter egg list will become guide-blog content." |
| **🎨 Unity Asset Engineer** | ✅ Approved | "Asset references are accurate — Bamao, Lumen, Heat UI, Cutscene Engine, Cinemachine, ACS, Eyes Animator. No assets referenced outside the 22-asset whitelist." |
| **👨‍💻 Senior Unity Developer** | ✅ Approved | "All system references map to a real script. The save JSON example matches `SaveService` schema. The Polish shader timing matches `PolishMiniGame.cs`." |
| **🔍 Critic & Review Board** | ✅ **Approved** | "This guide could be shipped verbatim as the player's manual for Mission 1. It will also serve as the QA team's gold-master walkthrough for the 20-person greenlight playtest." |

---

*Document version 1.0 — authored for the `feat/mission-1-2-architecture` branch, Phase 27.1 build. Phase 32 (menu collapse) update — § 3 Pre-Flight Checklist now references the single top-level entry `Hearthbound → 🚀 Build Everything` and the aggregate `🔍 Diagnose Build` per D-051.*
*Companion files: [`GAMEPLAY_GUIDE_OVERVIEW.md`](./GAMEPLAY_GUIDE_OVERVIEW.md), [`GAMEPLAY_GUIDE_MISSION_2.md`](./GAMEPLAY_GUIDE_MISSION_2.md).*
*Part of the Abdulmalek Agents game-concept portfolio · 2026.*
