# 🫖 Hearthbound Hollow — Gameplay Guide · Overview

> *"Some memories want to be sold. Some don't."*

> **This file is the master index of the player-facing gameplay guide.** It explains *what* Hearthbound Hollow is, *how* you play it, and *where* to look for the mission-by-mission walkthroughs. It is written for the **player**, not the engineer — but every system referenced here is mapped to its production spec in `Docs/Depth_Bible/` so a curious player can dig as deep as they like.
>
> **Authored by:** Creative Director · Game & Level Designer · Senior Unity Developer · Critic & Review Board
> **Branch:** `feat/mission-1-2-architecture`
> **Scope:** Mission 1 + Mission 2 (the polished playable vertical slice — ~55–75 minutes of cozy)
> **Companion files:**
> - **`GAMEPLAY_GUIDE_MISSION_1.md`** — *Opening The Hollow* (full step-by-step walkthrough)
> - **`GAMEPLAY_GUIDE_MISSION_2.md`** — *The Widower's Request* (full step-by-step walkthrough)

---

## 0. Table of Contents

1. [What This Guide Is](#1-what-this-guide-is)
2. [What Hearthbound Hollow Is — In One Hour](#2-what-hearthbound-hollow-is--in-one-hour)
3. [The Player Fantasy & The Cozy Contract](#3-the-player-fantasy--the-cozy-contract)
4. [Core Verbs — The Six Things You Will Do](#4-core-verbs--the-six-things-you-will-do)
5. [The Six Scenes — Your Whole World For Now](#5-the-six-scenes--your-whole-world-for-now)
6. [Controls — Keyboard, Gamepad, Touch](#6-controls--keyboard-gamepad-touch)
7. [Pre-Game Setup — The Tone Compass & Gentle Mode](#7-pre-game-setup--the-tone-compass--gentle-mode)
8. [Comfort & Accessibility — The Full Menu](#8-comfort--accessibility--the-full-menu)
9. [The 7 Vows — The Game's Moral Compass](#9-the-7-vows--the-games-moral-compass)
10. [Pickle — The Cat Who Will Judge You](#10-pickle--the-cat-who-will-judge-you)
11. [The Echo Web — Memories That Know Each Other](#11-the-echo-web--memories-that-know-each-other)
12. [The Evening Ledger — Your Only HUD, Your Only Save](#12-the-evening-ledger--your-only-hud-your-only-save)
13. [The 14-Dimension Village State — What The Game Tracks About You](#13-the-14-dimension-village-state--what-the-game-tracks-about-you)
14. [Mission Flow — What Plays In What Order](#14-mission-flow--what-plays-in-what-order)
15. [The Six Cozy Rules — What This Game Will Never Do To You](#15-the-six-cozy-rules--what-this-game-will-never-do-to-you)
16. [Best Practices — How To Get The Most From One Hour](#16-best-practices--how-to-get-the-most-from-one-hour)
17. [Troubleshooting — Common Player Questions](#17-troubleshooting--common-player-questions)
18. [Where To Look Next](#18-where-to-look-next)
19. [Critic & Review Board Sign-Off](#19-critic--review-board-sign-off)

---

## 1. What This Guide Is

This guide is a **player's manual** to Mission 1 and Mission 2 of *Hearthbound Hollow*. It is the cozy version of an old-school strategy guide — built so that:

- A first-time player can sit down with it open in a second tab and **never feel lost**.
- A curious player can read it after their first playthrough and **discover everything they missed**.
- A returning player can use it to **try a different choice path** and see exactly what changes.
- A developer or playtester can use it as a **gold-master walkthrough script** to verify the build is in good shape.

It is split into three documents on purpose:

| File | Purpose | Read when |
|---|---|---|
| **`GAMEPLAY_GUIDE_OVERVIEW.md`** *(this file)* | The map. The rules. The setup. The vocabulary. | **Before you press Play for the first time.** |
| **`GAMEPLAY_GUIDE_MISSION_1.md`** | Mission 1 walkthrough — *Opening The Hollow.* | **While playing Mission 1**, or to plan the perfect first impression. |
| **`GAMEPLAY_GUIDE_MISSION_2.md`** | Mission 2 walkthrough — *The Widower's Request.* | **Before or during Mission 2**, when you have to make the game's first real choice. |

The mission files are detailed enough that you could play through each one with the file in hand and see every option, every consequence, every Pickle quote, and every hidden detail. The overview is shorter and lighter — it tells you everything you need to know before the first cutscene fades in.

---

## 2. What Hearthbound Hollow Is — In One Hour

You inherit the Hollow — a memory-brokerage shop in a small autumn village. The previous keeper left without a goodbye. Doris the baker is your first customer. The widower Gerrold is your second. You will not see anyone else in Mission 1 or Mission 2 — they are the entire cast.

In the next **55–75 real-time minutes**, you will:

1. **Walk** down a quiet autumn lane at dusk and meet the woman who has been keeping the shop's key for you.
2. **Polish** a glass orb that contains a memory of a morning she had fifty years ago.
3. **Place** that orb on a shelf, close a leather-bound book, and go to sleep.
4. **Dream** Doris's First Loaves as an illustrated, sixty-second short film.
5. **Wake up** to a knock at the door.
6. **Listen** to a man tell you why he wants the last week of his wife's life removed from his memory.
7. **Harvest** a herb from the garden behind the shop. **Brew** a tea.
8. **Walk** to his cottage. **Sit** in one of two chairs that mean something different.
9. **Choose** — between four genuine options — what to do with his memory.
10. **Perform** the work you chose (or refuse it, with dignity).
11. **Return** a folded white handkerchief.
12. **Dream** the consequence of your choice.
13. **Close** a book again, and sleep.

That is the game's whole emotional vocabulary, established in a single hour. Everything that comes after — Marin's story, the bedroom door at the cottage, the locked upstairs of the Hollow, the bee orb on the shelf, the predecessor's apron — was deliberately seeded into Mission 1-2 and lives in your peripheral vision until the producer greenlights Mission 3.

---

## 3. The Player Fantasy & The Cozy Contract

**The fantasy:** *You are the kind of person a stranger trusts with the heaviest thing they own. You are unhurried. You are careful. You are quiet around grief without making the quiet feel like distance.*

**The Cozy Contract** — six promises the game makes to you, every minute you play:

| # | Promise | What it means in practice |
|---|---|---|
| 1 | **Nothing punishes kindness.** | You will never lose a point, a memory, a relationship, or a save because you were generous. Generosity is the optimal play; it just may not be the highest-margin play. |
| 2 | **Failure is narratively absorbed, not mechanically scored.** | If your Cleanse mini-game crosses the core memory, the game does **not** say *FAILED*. Instead, the man whose memory you were working on responds to what actually happened. The game never makes you feel like you've broken it. |
| 3 | **No time pressure unless you ask for it.** | No countdowns, no decay timers, no missed deadlines. The choice screen in Mission 2 has no timer — you may sit on it for an hour if you wish. |
| 4 | **Refusal is always honored.** | Every transaction in this game can be declined. The villager's reaction will be calibrated to the refusal — they will not begrudge you. Refusing is a valid path. *Refusing is sometimes the best path.* |
| 5 | **No content surprises.** | The Tone Compass (§ 7) tells you exactly what kinds of feelings the next ~2 hours will contain. The heaviest content (the moral choice with Gerrold) has a second-confirmation button. You will not be ambushed. |
| 6 | **Auto-Complete is always available.** | Every mini-game has a button labeled *"Polish for me"* / *"Cleanse for me"* visible from frame one. The narrative consequence is identical. The hand-craft is a gift to those who want it — never a tax on those who don't. |

These six promises are non-negotiable. If you feel even one of them broken at any moment, that is a bug and we want to know.

---

## 4. Core Verbs — The Six Things You Will Do

Hearthbound Hollow has **six verbs** in Mission 1-2. That is the entire mechanical vocabulary. Every other verb in the long-form bible (Weave, Sever, Listen-as-mini-game, Read, Translate, Identify, Compose, Search, Negotiate, Compose Verse, etc.) is reserved for Mission 3 and beyond.

| Verb | When you do it | What it feels like | Where to learn it |
|---|---|---|---|
| **Walk** | Constantly | Camera-relative WASD/stick. Smooth follow camera. Slow autumn pace. Sprint and Jump available unless Gentle Mode is on. | This file § 6 |
| **Listen** | Every dialogue scene | Watch the speaker. Read their lines. Pick one of 2–6 options that almost always include "(silent nod)". You are never required to speak first. | Mission 1 walkthrough |
| **Examine** | At every prop with a glow outline | Press E (or the interact button) near a prop. Read the codex tooltip. Many props seed long-arc story. | Mission 1 walkthrough — *The Hollow Interior* section |
| **Polish** | Once in Mission 1, at the workbench | Hold left mouse, draw slow circles over a faded glass orb. The memory inside becomes visible as you clear it. ~70–90 seconds. | Mission 1 walkthrough — *The Polish Mini-Game* section |
| **Brew** | Optional in Mission 2 | Harvest a herb → bring it to the kettle → wait for it to steep → pour into a cup → carry the cup in a small wooden box. ~12 real seconds of warmth. | Mission 2 walkthrough — *The Tea Loop* section |
| **Cleanse** | Once in Mission 2, at the workbench (only on certain choice paths) | Trace each glowing crack on a memory orb with the cursor, without crossing into the orb's warm core. ~70–100 seconds. Real consequences. | Mission 2 walkthrough — *The Cleanse Mini-Game* section |
| **Choose** | Once in Mission 1 (the price), once in Mission 2 (the moral choice) | Read four sentences. Press a button. Sometimes confirm twice. | Both walkthroughs — *Choice Screens* sections |
| **Sleep / Close The Ledger** | End of every in-game day | Close a leather-bound book. The game saves. A dream plays. The morning comes. | This file § 12 |

Note that there is no **combat verb**. Hearthbound Hollow has no combat. There is no fail-state. There is no health bar. The game's tension is **moral**, not **mechanical** — see § 9.

---

## 5. The Six Scenes — Your Whole World For Now

These are the **only six Unity scenes** that exist in Mission 1-2. Beyond their edges, the world is walled off until Mission 3.

```
00_Bootstrap                                  ← Service init. Player never sees this.
       │
       ▼
01_MainMenu                                   ← "Open The Hollow" / Continue / Settings / Quit
       │  (first-launch only: Tone Compass card + Gentle Mode prompt)
       ▼
02_Mission01_Lane                             ← The autumn lane at dusk. ~80m of cobble + dirt path.
       │  Meet Doris outside the bakery.
       │  Optional 30-sec detour: the dormant beehive in Doris's back garden.
       ▼
03_Mission01_Hollow                           ← Mission 1's main set. The shop + the workbench.
       │  Polish DOR-001 (Doris's First Loaves).
       │  Read Marin's pinned note.
       │  Sleep → Memory Dream 1 (~60s).
       │  Wake → knock at the door.
       ▼
04_Mission02_Garden    ←┐                     ← The herb garden behind the Hollow. 4 raised beds.
       │                │  (Optional skip — you can refuse the garden entirely.)
       ▼                │
       (back to Hollow) │
       │                │
       ▼                │
05_Mission02_Cottage   ─┘                     ← Gerrold's cottage. Two chairs. A bedroom door
       │                                         you will not open today. The choice happens here.
       ▼
       (Cleanse at the Hollow workbench, OR Listen scene in the cottage, OR Defer)
       │
       ▼
03_Mission01_Hollow (Day 2 evening)           ← Same room, different ledger.
       │  Memory Dream 2 plays (one of five variants).
       │  Mission 2 outro cinematic.
       ▼
01_MainMenu                                   ← Saved. Done. Return tomorrow.
```

### 5.1 Brief scene encyclopedia

| Scene | What you do here | Approximate real-time |
|---|---|---|
| **Lane (dusk)** | Walk, meet Doris, optionally find the beehive. | 4–6 minutes |
| **Hollow (shop + workbench)** | Examine, dialogue, polish, place orb, ledger, sleep. Returns for Day 2 morning + Day 2 evening. | 18–28 minutes total across both days |
| **Garden (morning)** | Harvest 1 or 2 herbs. (Or skip.) | 2–4 minutes |
| **Walk to Cottage** | Side lane, 35 m, Gerrold pacing beside you, ~6 lines of his dialogue. | 1 minute |
| **Cottage interior** | Sit, listen, look around the room, choose. | 8–14 minutes |
| **Memory Dreams** | Watch. They cannot be controlled, only witnessed. | 60s (Dream 1) / 30–90s (Dream 2 by variant) |

That's it. Six scenes. Roughly 1,800 square metres of explorable world. Every detail is hand-placed.

---

## 6. Controls — Keyboard, Gamepad, Touch

| Action | Keyboard / Mouse | Gamepad | Touch (mobile) |
|---|---|---|---|
| Move | **WASD** or arrows | Left stick | Virtual left stick |
| **Sprint** *(default on; off in Gentle Mode)* | **Left Shift** | LStick click | (n/a — no sprint on touch) |
| **Jump** *(default on; off in Gentle Mode)* | **Space** | Gamepad south (A on Xbox) | (n/a) |
| Interact / Pick up / Examine | **E** | Gamepad west (X on Xbox) | Tap interact prompt |
| Advance dialogue | **Click**, **Space**, or **Enter** | Gamepad south | Tap |
| **Polish orb** (Mission 1) | **Hold left mouse**, draw slow circles | Hold right trigger, right stick draws circles | Drag finger in circles |
| **Cleanse orb** (Mission 2) | **Hold left mouse**, trace cracks | Hold right trigger, right stick traces | Drag finger along cracks |
| **Camera look** | **Hold Right Mouse + drag** | Right stick | (n/a — fixed camera on touch) |
| **Camera zoom** | **Mouse scroll** | LB / RB (or LT / RT) | Pinch |
| **Open Codex / Memory Map** | **C** | Gamepad north | Codex button bottom-right |
| **Help / Controls card** | **H** | Touchpad / View | "?" button |
| **Pause / Menu** | **Esc** | Start / Menu | Pause button |
| **Skip cutscene** | **Hold Space** for 0.8s | Hold gamepad south | Hold tap |

> **Pro tip:** In every cutscene and Memory Dream, **long-press** instead of tap to *linger* — the dream plays in slow motion at +50% length. Press once briefly to skip. Some players find Memory Dream 1 worth lingering on.

### 6.1 The Help card (the H key)

Press **H** at any moment. A small Bamao parchment overlay appears with all controls visible. Gentle Mode strips the Sprint and Jump rows automatically so the card doesn't promise anything the current settings deliver. Close with **H** again or **Esc**.

---

## 7. Pre-Game Setup — The Tone Compass & Gentle Mode

The very first time you launch the game, you will see — **before the title screen** — a 90-second illustrated card called the **Tone Compass**.

### 7.1 What the Tone Compass shows you

A small painted Pickle in the corner. A few honest sentences in fiction-voice that tell you exactly what kinds of feelings the next ~2 hours will hold:

> *This game will make you feel things. Some of those feelings are heavy.*
>
> *This first hour contains: the opening of a shop, a first transaction, a late-night brewing.*
>
> *The second hour contains: a widower's grief, a choice about memory, a short illustrated dream.*
>
> *At any point, you can take a Soft Day, enable Gentle Mode, or adjust any settings.*
>
> *The cat will be there.*

The card is **skippable** by pressing any button before 90 seconds elapse — but it is also a beautiful piece of art with a soft piano-arrangement of the main theme underneath, and many players linger.

After the card you are offered a binary prompt: **"Gentle Mode?"** with two clearly-labeled buttons. The prompt has a 4-second auto-confirm to *off* if you don't press anything — you will not be trapped on a menu.

### 7.2 Should I turn Gentle Mode on?

| Choose **Gentle Mode** if… | Choose **Default** if… |
|---|---|
| You are playing on a couch after a hard day. | You want the full intended weight of Mission 2's choice. |
| You are sensitive to themes of dementia, loss-of-spouse, or terminal illness. | You are comfortable with quiet grief. |
| You would prefer the puzzles be a ritual rather than a craft. | You want to feel skill in the Cleanse mini-game. |
| You want Pickle to be a little less prickly. | You want her full sarcasm. |
| You want any failure outcomes to be automatically recoverable. | You want your choices to commit. |

**The narrative does not change.** Gentle Mode does not skip content; it softens its register and tightens the safety nets. You can toggle it on or off **at any moment** during play. Achievements are not affected.

### 7.3 The Comfort Tools Menu

The Tone Compass is a one-time intro. The **Comfort Tools Menu** is permanent — open it any time via Pause → Settings. § 8 below catalogs every toggle.

---

## 8. Comfort & Accessibility — The Full Menu

Every toggle below is available **from the Pause menu at any moment** and **from the Settings panel on the main menu**. They are also accessible from a small *Settings* icon in the bottom corner of the Evening Ledger.

| Toggle | Default | What it does |
|---|---|---|
| **Gentle Mode** | OFF | Softens memory prose, disables Sprint + Jump, lengthens Cleanse retry budget, slows the moral-choice confirmation, makes mini-game failure recoverable. The single biggest setting in the game. |
| **Auto-Complete Polish** | OFF | Mission 1 mini-game runs a 12-second sped-up version for you. Same outcome, same trust gain, same dialogue. No penalty. |
| **Auto-Complete Cleanse** | OFF | Mission 2 mini-game runs automatically. Default outcome = Acceptable (Gentle Mode = Perfect). Same Yarn branching. No penalty. |
| **Subtitle Size** | Medium | 4 tiers — Small / Medium / Large / Huge. Renders correctly on Switch handheld and on 4K monitor at all sizes. |
| **Subtitle Background** | Translucent | Translucent / Opaque / None. |
| **Color-blind Mode** | OFF | 3 palette presets — Deuteranopia / Protanopia / Tritanopia. Shifts UI accent colors and memory orb hue tints. |
| **Reduce Particle Intensity** | OFF | Halves Lumen god rays, halves firefly count on the lane, halves dust motes in the dreams. |
| **Reduce Screen Flash** | OFF | Eliminates the bright amber flash on Polish-success and the brief lightening on Cleanse-success. |
| **Dyslexia-friendly Font** | OFF | Swaps to OpenDyslexic across all UI. |
| **One-hand Controls** | OFF | Re-binds the game so every interaction is reachable with one hand on either mouse or stick. |
| **Master / Music / SFX / Ambient Volume** | 5/5 each | Standard 4-bus mix. Settings persist across sessions via PlayerPrefs. |
| **Voice Volume per Character** | individual | Doris, Gerrold, and Pickle each have an independent voice slider. (VO is English-only in Mission 1-2.) |
| **Pickle Sass Intensity** | 3/5 | 1 = warm and gentle. 5 = full sarcasm. Mission 1-2 only differentiates settings 1, 3, and 5; settings 2 and 4 collapse to the nearest. |
| **Memory Hum Volume** | 5/5 | The faint hum every memory orb emits. Some players find it ASMR; some find it busy. Slide accordingly. |
| **Heavy Theme Warning Cards** | ON | Before any heavy content (Mission 2's choice, the cottage entry), a small Bamao card briefly announces the kind of content. Tap to dismiss. |

The single most important thing to know: **none of these settings can be missed by accident**, and **none of them lock you out of content**. They are first-class.

---

## 9. The 7 Vows — The Game's Moral Compass

Every Keeper of the Hollow lives by seven vows. The previous Keeper, Marin, taught the rest of the village what they meant. You will not see all seven exercised in Mission 1-2 — three of them are introduced in detail, and the other four sit at their default integrity (50/100) waiting for your future days.

| Vow # | Name | What it means | Active in Mission 1-2? |
|---|---|---|---|
| 1 | **Consent** | A memory can only be transacted with the willing consent of its bearer. You may not take what is not freely offered. | ✅ Active in M1 (Doris) + M2 (Gerrold) |
| 2 | **Return** | Found memories, anonymous memories, lost memories — these belong to the people they were taken from. | ⬜ Not yet — Confession Booth lives in Mission 4+ |
| 3 | **Whole** | A memory should leave your shop as whole as it can be, or as gently cleansed as the bearer asked for. Never less. | ✅ Active in M1 (Polish) + M2 (Cleanse) |
| 4 | **Quiet Glass** | An orb in the shop should not be displayed, broadcast, or used as currency. | ⬜ Dream Cinema is Mission 9+ |
| 5 | **Honest Coin** | The price you charge should be the price the memory is worth — and not more, even if the bearer offers it. | ✅ Active in M1 (Doris's price negotiation) |
| 6 | **Open Door** | The Hollow is open to anyone, regardless of their past business with you. | ⬜ Untested in M1-2 — comes online in Mission 3+ |
| 7 | **Last Light** | Sometimes the best work a Keeper can do is sit beside the bearer and listen. *Some memories should not be transacted at all.* | ✅ Active in M2 (the Listen path) |

The seven Vows appear as small glyphs at the bottom of every Evening Ledger page. Each glyph is *softly lit* when its integrity is high (50–100), *dimmed* when it is low (0–50). After Mission 2's choice, you will see at least two of them visibly shift.

> **Why this matters:** The Vows are not a "morality bar." They do not unlock or lock content directly. Instead, they shape **how every villager responds to you** over the course of the game. Honor Consent and Whole, and grief-bearing villagers will return to you. Drop them low and visiting villagers will hesitate at the door.

---

## 10. Pickle — The Cat Who Will Judge You

You will meet **Pickle** on the windowsill of the Hollow's shop room the first time you enter, in Mission 1. She does not move during your first day. She speaks to you (internally — only you hear her) exactly once.

Pickle is the cozy game's familiar — a slate-grey tabby with a single white sock, the only living thing the previous Keeper left behind. She is **a fully voiced character** with a strong personality:

- She is unimpressed by most things.
- She is gentler around men in mourning.
- She thinks the previous Keeper was both wonderful and slightly moody.
- She judges your every decision **but never out loud unless you have earned the right to hear her**.

### 10.1 The Pickle Approval system

Every kind, unhurried, considerate action you take increases your **Pickle Approval**. It starts at 50/100. After Mission 1-2 a typical caring player will be at ~52. Polite players who use Listen + Lavender + Refusing-overpayment can reach ~58. Aggressive players (Erase + Valerian + skipped garden) drop to ~46–48.

**Why this matters now:** at Pickle Approval **50 or above**, Pickle's pre-choice commentary plays during the Mission 2 moral choice. Below 50, you face the choice in silence — which is its own meaning. Building Pickle Approval is **not** a chore — it is the natural result of being kind.

### 10.2 Pickle's lines in Mission 1-2 (the only ones she will say)

She has **four** in the entire 55–75 minute vertical slice. Each is a small gift:

| # | Mission | When | Line |
|---|---|---|---|
| 1 | M1 | After your first Polish completes | *"That was — adequate. I was expecting much worse. Continue, please."* |
| 2 | M2 | The moment Gerrold knocks at the door | *"There is a man at your door. He has been there for twenty minutes. He is a careful sort. The careful kind hurt the worst. ... I would let him in."* |
| 3 | M2 | During tea brewing (if Pickle is in the workbench room) | *"Tea for a man who has not asked for tea is the rarest kindness. Try not to overthink it. The man is also a cat about this. We will accept."* |
| 4 | M2 | At the Echo Web's first activation (when you pick up Gerrold's orb at the Hollow counter) | *"Ah. The Web has decided to speak today. This one and yesterday's are in the same kitchen on the same Sunday. That is what the line of light is saying. Look at it. It is showing off."* |

If your Pickle Approval is ≥50 at the moral-choice screen, you also get **one of four extra lines** depending on which option you highlight. See Mission 2 walkthrough § 5.

---

## 11. The Echo Web — Memories That Know Each Other

Every memory orb in the game carries up to four **Echoes** — invisible thread-keys that mark when two memories share a face, a place, an object, or a year.

When you collect enough memories that share an Echo, the **Memory Map UI** (called the **Codex**) lights up with a glowing line between the connected orbs.

### 11.1 The Mission 1-2 first activation

In Mission 2, when you first pick up Gerrold's orb at the Hollow's counter, **the Echo Web animates for the very first time** — drawing a single golden line between:

- **DOR-001 — The First Loaves** (the orb you polished in Mission 1), and
- **GER-007 — The Last Week** (Gerrold's orb in your hand).

They share the Echo **"Doris in kitchen on Sunday"** — Doris was Margery's best friend; they sat in Doris's kitchen on Sundays for ~30 years. You don't yet know this in dialogue. The line of light tells you the same kitchen lives in both memories.

This single connection is **the cozy game's first promise** — every memory you collect threads into others, and the whole village is one woven thing.

### 11.2 What you can do with the Echo Web in Mission 1-2

In M1-2, the Echo Web is **read-only**. You can:

- Open the Codex (press **C**) and see the line of light.
- Hover over either endpoint to read the memory's short summary.
- Examine the connection itself to see the Echo's name ("Doris in kitchen on Sunday").

You **cannot** yet:

- Click a connection to trigger a Revelation Chapter (Mission 4+).
- Trace ambient connections without enough mass (need 3+ memories sharing an Echo; you only have 2 in M1-2).
- Use the Web to unlock locked memories (Mission 3+ Memory Map navigation).

It is a teaching screen for the system that will grow.

---

## 12. The Evening Ledger — Your Only HUD, Your Only Save

Hearthbound Hollow has **no HUD during gameplay**. No quest log floating in the corner. No health bar. No timer. No XP meter. No floating arrows. Nothing.

The only HUD-like surface is **the Evening Ledger** — a leather-bound book that opens on the workbench desk every in-game evening (~17:00). It is the player's daily-end ritual and the **only save mechanism**.

### 12.1 What's in the Ledger

A Bamao open-book frame with two pages:

**Left page — the day:**
- Hand-written prose narrating what happened today, in fiction voice. (E.g., *"Doris, the baker, was your first visitor. She offered her First Loaves — a memory from the morning her bakery opened, fifty years ago. You polished it. You did your work. She seemed pleased."*)
- The 7 Vow glyphs at the bottom, each subtly lit or dimmed based on their current integrity.

**Right page — open threads:**
- A gentle list of work-in-progress. *(E.g., "Doris's First Loaves is on the shelf. You may return to it any morning."* / *"There is a locked room upstairs. The door is heavy.")*
- The Hollow's level (currently 1).
- Pickle's current sleeping location, drawn as a small inkblot.

### 12.2 Saving = Closing the Book

When you press **Close the book**:

1. The book closes audibly (cloth-on-paper).
2. Pickle, on the windowsill, stretches.
3. The composer's main theme enters at 25% volume.
4. Screen fades to amber.
5. **The save runs silently in the background.** No spinner. No "Saving..." overlay.
6. The Memory Dream for that day begins.

This is intentional. The cozy game's discipline: **saving is a gesture inside the fiction.** You are putting down a book at the end of a day.

### 12.3 Save slots

The game ships with:

- **3 manual rolling save slots** (you choose which to overwrite when closing the Ledger).
- **1 emergency autosave** (refreshed at every scene transition).

All saves are atomic-write — they survive a power failure. The Cozy Contract: you will not lose progress.

### 12.4 Examining the Ledger

Before closing, you can:

- **Hover** a transaction to read its full prose.
- **Click** a Vow glyph to read Marin's hand-written reflection on that Vow.
- **Flip back** to previous days' pages.
- **Pet** the inkblot of Pickle for an easter egg (her real-life purr plays briefly).
- **Examine** an "Open work" thread to read 2 more sentences of context.

All of this is ungated, ungrindy, optional cozy lingering.

---

## 13. The 14-Dimension Village State — What The Game Tracks About You

Internally, the game maintains a **VillageState** — a structured record of everything that has happened. In Mission 1-2 only **a few dimensions are written to**, but they all exist from day one.

| # | Dimension | What it is | Active in M1-2? |
|---|---|---|---|
| 1 | **Trust (per villager)** | A 0–100 integer per villager. Doris and Gerrold are the only ones with non-zero values in M1-2. | ✅ Yes |
| 2 | **Memory Integrity** (per memory) | How intact each collected memory is. 100 = perfect; <50 = damaged. | ✅ Yes (DOR-001 + GER-007) |
| 3 | **Vow Integrity** (×7) | 0–100 per vow. Default 50. | ✅ 3 active vows (1, 3, 5, 7), 3 untouched |
| 4 | **Pickle Approval** | 0–100. Default 50. | ✅ Yes |
| 5 | **Coin** | Currency. You start with 10 coppers. | ✅ Yes |
| 6 | **Cinder** | The Confession Booth currency. Earned only by Listen-path actions. | ⬜ Only earnable via M2 Listen |
| 7 | **Hollow Level** | 1 in M1-2. Upgrades come in M5+. | ⬜ Locked at 1 |
| 8 | **Memories in Inventory** | List of memory IDs you currently hold. | ✅ Yes |
| 9 | **Echo Web Connections Activated** | List of memory-pair connections you've discovered. | ✅ Yes (1 in M1-2) |
| 10 | **Read Marin's Note IDs** | Which versions of Marin's pinned note you have read. | ✅ Yes |
| 11 | **First Moral Choice Made** | Boolean flag. Set after Mission 2. | ✅ Yes |
| 12 | **Public Standing** | Village-wide reputation. Default 0. | ✅ Yes |
| 13 | **Predecessor Trail Warmth** | How "near" Marin's story you've gotten. Rises when reading her note, examining the apron, etc. | ✅ Yes |
| 14 | **Day Index** | What day in-game it is. Mission 1 ends Day 1; Mission 2 ends Day 2. | ✅ Yes |

You never see any of these as numbers. The cozy game's **Cordray Principle** (Codex 08 § 9):

> *The player infers the consequence; they are never given numbers.*

The Vow glyphs in the Ledger are the only visualization, and even they are *subtly* lit, not graphed.

---

## 14. Mission Flow — What Plays In What Order

The full Mission 1 + Mission 2 sequence, with rough real-time durations:

```
[BOOTSTRAP] (auto-load)
   ↓
[MAIN MENU] — first launch shows Tone Compass (90s) + Gentle Mode prompt
   ↓
[OPENING CINEMATIC] — 3 min, hand-painted "arrival at the Hollow"
   ↓
═════════════════ MISSION 1 — Opening The Hollow ═════════════════
   ↓
[02_Mission01_Lane] (~6 min)
   ↓ walk down the autumn lane at dusk
   ↓ meet Doris on a stool outside her bakery
   ↓ dialogue (~3 first-greeting options)
   ↓ optional 30-second beehive detour (Easter egg)
   ↓
[03_Mission01_Hollow] (~14 min)
   ↓ enter the shop; Pickle on windowsill
   ↓ Doris hands you the orb in a wooden box
   ↓ price negotiation (3 options) — your first transaction
   ↓ examine the room (predecessor seeds, 3 welcome orbs, Marin's note)
   ↓ POLISH MINI-GAME — Doris's First Loaves (~70-90s)
   ↓ place the orb on a shelf
   ↓ return to Doris; she reacts to your polish quality
   ↓ open the Evening Ledger — first save
   ↓ close the book
   ↓
[MEMORY DREAM 1] (~60s)
   ↓ Doris alone in a 1972 kitchen at first light
   ↓ wake — fade to morning
═══════════════════════════════════════════════════════════════
   ↓
═════════════════ MISSION 2 — The Widower's Request ═════════════════
   ↓
[03_Mission01_Hollow] — Day 2 morning (~3 min)
   ↓ brief Doris greeting (if you walk to the bakery)
   ↓ Gerrold knocks at the Hollow door
   ↓ Pickle's pre-knock line
   ↓ first conversation with Gerrold (~3 min)
   ↓ optional path: defer Mission 2 to Mission 3
   ↓ player picks up Gerrold's orb from its handkerchief cloth
   ↓ ECHO WEB FIRST ACTIVATION — DOR-001 ↔ GER-007 line of light
   ↓
   ↓ Choose: walk to garden (A) / work at Hollow (B) / send Gerrold home (C)
   ↓
[04_Mission02_Garden] (~3 min, optional)
   ↓ pick Lavender or Valerian (or both, or neither)
   ↓
[03_Mission01_Hollow — Workbench] (~3 min)
   ↓ brew tea (90 in-game seconds = 12 real seconds)
   ↓ box up the cup
   ↓
[Walk to cottage] (~25 sec)
   ↓ Gerrold paces beside you; ~6 of his lines play
   ↓
[05_Mission02_Cottage] (~8-14 min)
   ↓ enter the cottage; observe Margery's empty chair
   ↓ choose Gerrold's chair / Margery's chair / fireplace
   ↓ offer the tea (if you brewed one) — modifies dialogue
   ↓
[THE MORAL CHOICE SCREEN]
   ↓ Four options. No timer. Pickle's pre-choice line (if Approval ≥50).
   ↓
   ↓ Branch into one of 5 paths:
   ↓   A1. ERASE → Cleanse mini-game (aggressive) → Variant A or C
   ↓   B1. CLEANSE → Cleanse mini-game (careful) → Variant A, B, or C
   ↓   C1. LISTEN → 3-minute Listen cutscene → Variant D
   ↓   D1. DEFER → orb in cloth on counter → Variant E (30s)
   ↓
[CLEANSE MINI-GAME or LISTEN SCENE] (~75-180s)
   ↓
[Return to cottage door — return the handkerchief]
   ↓
[03_Mission01_Hollow — Day 2 evening]
   ↓ Evening Ledger Day 2 (5 different prose variants by choice path)
   ↓ close the book
   ↓
[MEMORY DREAM 2] (~30-90s, one of 5 variants by your choice + Cleanse outcome)
   ↓
[MISSION 02 OUTRO CINEMATIC] (~30s)
   ↓ slow pan through the Hollow
   ↓ shelves visible — DOR-001 always there, GER-007 different per choice
   ↓ text: "Day 2 ended. Tomorrow, the village will know."
   ↓ fade to black
   ↓
═══════════════════════════════════════════════════════════════
[MAIN MENU] — your save is now Day 3 ready
```

**Total real-time: ~55–75 minutes** for an unhurried player. Faster speedrunners can complete in ~38 minutes. Cozy lingerers will take ~90 minutes.

---

## 15. The Six Cozy Rules — What This Game Will Never Do To You

1. **It will never punish you for kindness.** Refusing to overpay Doris is not "incorrect." Offering Gerrold lavender tea instead of valerian is not "wrong." Sitting with a man rather than working on his memory is not a "lesser" choice — Vow 7 actively rewards it.
2. **It will never use time pressure as a default.** The moral-choice screen has no timer. The Cleanse mini-game has no real-time constraint. You can put the controller down at any moment and the game will wait.
3. **It will never gate you behind a mini-game.** Both Polish and Cleanse have Auto-Complete toggles visible from frame one. Skipping the hand-craft does not skip the consequence.
4. **It will never spring grief on you unannounced.** The Tone Compass tells you in advance what kinds of feelings are in the next ~2 hours. Heavy Theme Warning cards appear before each heavy-content scene.
5. **It will never give you a "FAILED" screen.** Crossing the core during a Cleanse causes a real outcome — but the game describes it as something that happened, not as a player failure. Gerrold's response is calibrated; he forgives you with dignity.
6. **It will never make you feel like you have to grind.** Mission 1-2 is ~70 minutes of hand-crafted content with no XP bars, no daily login bonuses, no resource depletion. Every minute is hand-authored. Every second is intentional.

If you ever feel one of these six rules is broken, please report it — it is a bug and we want to know.

---

## 16. Best Practices — How To Get The Most From One Hour

A cozy game rewards patience the way a brick oven rewards an early-morning baker. Some specific suggestions for getting the deepest experience from Mission 1-2:

### 16.1 On the lane (Mission 1)

- **Walk slowly.** Don't sprint. The Lumen god rays through the autumn trees are a visual you cannot rush.
- **Take the side path** at the 65 m mark. The dormant beehive in Doris's back garden is the seed of Mission 3+'s entire Wedding Honey arc.
- **Wave to the silent villager** on the bench. He nods. He will return in Mission 4.

### 16.2 In the Hollow (Mission 1)

- **Examine every prop with a glow.** The apron on the hook, the pinned note, the kettle, the three welcome orbs, the open book on the chair — each is a story you will not see complete for many missions.
- **Read Marin's note above the workbench.** It contains the only tutorial text in the game and the first hint of the predecessor mystery.
- **Pet Pickle.** Walk close to the windowsill. She doesn't move. Her purr loops audibly. This is correct.
- **Don't rush the Polish.** Watch the orb actually clear under your circles. The reveal of Doris's young hands behind the glass is one of the cozy game's most-quoted moments.

### 16.3 With Doris (Mission 1)

- **Honor her price** on your first transaction. 4 coppers is fair. Paying more makes her uncomfortable. Paying less makes you start the game in debt — which is actually a cozy storytelling thread the game enjoys.
- **Ask "Who was the old one?"** Her response is short but sets the predecessor mystery in motion.

### 16.4 In Mission 2

- **Offer Gerrold a chair before he asks.** A simple kindness.
- **Visit the garden.** Even if you don't brew tea, the bright outdoor space contrasts beautifully with the cottage's dim interior.
- **Try Lavender first.** It is the kinder herb. The valerian path is a brave choice; lavender is the canonical cozy choice.
- **Sit in Margery's chair.** A small kindness. Gerrold notices. (Pickle, if Approval ≥50, notices too — she has a small line about it.)
- **Read the framed photograph** on the mantel. Doris is in the background, holding bread. This is the Echo Web's source material.
- **The Listen path is real.** Choosing C — Don't take the memory. Sit with him. — does not lock you out of anything. It is the cozy game's highest-Vow-7 path and may be the most moving 3 minutes you spend in the entire vertical slice.

### 16.5 At the choice screen

- **Read all four options before highlighting.** The preview sentences for each are written in fiction voice, not in mechanical descriptions. They are meant to be sat with.
- **You can cancel during the confirm pause** on the Erase option. You will not be trapped.
- **There is no wrong choice.** This is structurally true, not aspirational.

### 16.6 After the choice

- **Linger on Memory Dream 2.** Long-press to slow-mo. Especially Variant D (the Listen path) which holds on Gerrold's face for 4 seconds while he looks directly at you.
- **Read every line on the Day 2 Evening Ledger.** Five completely different prose pages exist for the five outcome variants. The cozy game wrote each one by hand.

### 16.7 The hidden completion

The cozy player who:
- Honors Doris's price exactly,
- Reads all 7 examinable props in the Hollow,
- Brews lavender tea,
- Sits in Margery's chair,
- Chooses Listen (option C),
- Returns the handkerchief at the end,

…lands on the highest combined Vow integrity reachable in Mission 1-2 and unlocks the warmest variant of every dialogue line in Mission 3's morning. This is the game's first **hidden completion** — never announced, only felt.

---

## 17. Troubleshooting — Common Player Questions

### Q: I can't find the Hollow door — where is it?

The Hollow is **attached to the side of Doris's bakery**, sharing a wall. Doris walks you through the connecting door after the price negotiation in Mission 1. You will not have to find it on your own.

### Q: The Polish mini-game seems endless. What am I doing wrong?

Nothing. The mini-game cannot fail. Either keep drawing slow circles (it completes in ~78 seconds), or click the **"Polish for me"** button in the bottom-right corner of the workbench frame. Same outcome.

### Q: I accidentally chose "Erase" — can I undo?

The confirmation pause is **cancelable**. If you've already passed the second confirm, the choice is locked — but Memory Dream 2 will play the variant matching your outcome, and Mission 6+ contains a full recovery arc if the Cleanse Crossed-Core'd. No actual play is "wasted."

### Q: Where is the save button?

There isn't one. **Close the Evening Ledger** at the end of each in-game day. That is the save. Autosaves additionally run at every scene transition.

### Q: Pickle isn't talking to me. Why?

Pickle has exactly **four lines** in Mission 1-2 plus four conditional pre-choice lines. If you have Pickle Approval below 50, the pre-choice lines won't play during Mission 2's moral choice. This is intentional — earning her is part of the game.

### Q: The character is half-sunk into the floor / pops up when I press WASD.

This was a Phase 27 bug — already fixed. Pull the latest of `feat/mission-1-2-architecture`, run `Hearthbound → 🚀 Build Everything` from the Unity menu, then press Play. The `PlayerGroundClamp` runtime component auto-corrects mesh alignment.

### Q: The dialogue isn't rendering / appears in Console only.

Yarn Spinner isn't installed. Unity Package Manager → "+" → "Add package from git URL" → `https://github.com/YarnSpinnerTool/YarnSpinner-Unity.git`. The project compiles either way (the YarnVillageStateBridge is guarded), but you need Yarn for the dialogue UI.

### Q: I want to skip the cutscenes.

Hold the **Space** key for 0.8 seconds during any Memory Dream or cinematic. A small bar fills in the bottom-right. Release after the bar fills to skip. Pure tap = no skip (so you don't accidentally lose a moment you wanted to see).

### Q: Is there voice acting?

In the current vertical slice, voice acting is **placeholder/disabled**. The full vertical slice (Week 17 of the build timeline) ships with **Tier B VO** — Doris, Gerrold, and Pickle fully voiced; other content is text-only. Voice volume is configurable per-character in the Comfort Tools Menu.

### Q: Where does Mission 3 start?

It does not yet exist. The vertical slice ends at the Mission 2 outro. The post-greenlight roadmap (see `Docs/Depth_Bible/Mission_1_2_Focus/00_FOCUS_OVERVIEW.md` § 4) sequences Mission 3 after the 20-person playtest passes its criterion.

---

## 18. Where To Look Next

Once you have played Mission 1 and Mission 2:

| Document | What it adds |
|---|---|
| **`GAMEPLAY_GUIDE_MISSION_1.md`** | The step-by-step walkthrough of Mission 1 — every dialogue option, every Pickle line, every prop, every hidden detail. |
| **`GAMEPLAY_GUIDE_MISSION_2.md`** | The step-by-step walkthrough of Mission 2 — all four choice paths in full, the Cleanse mini-game spec, all five Memory Dream 2 variants. |
| **`GAME_DESIGN.md`** (root) | The original pitch, the market gap, the revenue model, the production pillars. |
| **`Docs/ARCHITECTURE.md`** | The full technical spec — every C# script, every asmdef, every ScriptableObject. Read if you want to understand how the systems were built. |
| **`Docs/Depth_Bible/00_INDEX.md`** | The 17-codex master design canon. |
| **`Docs/Depth_Bible/Mission_1_2_Focus/00_FOCUS_OVERVIEW.md`** | The production-ready spec for the vertical slice. The codex this guide was distilled from. |
| **`Docs/Depth_Bible/Mission_1_2_Focus/01_DORIS_THE_BAKER.md`** | Doris's full character bible, voice signature, 4-node Memory Map, complete Yarn dialogue, BoZo reskin recipe. |
| **`Docs/Depth_Bible/Mission_1_2_Focus/02_THE_WIDOWER_GERROLD.md`** | Gerrold's full character bible, Margery's off-screen biography, all four choice tree outcomes, complete Yarn dialogue. |
| **`Docs/Depth_Bible/06_COZY_COMFORT_AND_ACCESSIBILITY.md`** | The complete accessibility & comfort design. The Doyne Test. The Tone Compass research. |
| **`Docs/PROGRESS.md`** | Live development log — what shipped, what hotfixed, what is in flight. |

If you want to go even deeper, every `[CODEX-XX § Y.Z]` and `[FOCUS-NN § Y.Z]` reference in the mission guides points to a specific section of these documents.

---

## 19. Critic & Review Board Sign-Off

| Reviewer | Verdict | Note |
|---|---|---|
| **🎬 Creative Director** | ✅ Approved | "The Cozy Contract is preserved. The Player Fantasy is articulated. The verb list matches scope. The Pickle inset is correctly restrained." |
| **🗺️ Game & Level Designer** | ✅ Approved | "All six scenes are accurately described. The verb list is precise. The 14-dimension VillageState is documented at the right altitude — players know it exists, they don't have to read the numbers." |
| **📈 Trend & Ideation Analyst** | ✅ Approved | "The cozy-game red lines (r/CozyGamers signals — surprise grief, time pressure, mini-game gating, etc.) are explicitly defended in § 15. This is exactly what the audience is looking for." |
| **🎨 Unity Asset Engineer** | ✅ Approved | "No asset is referenced that isn't on the 22-asset whitelist. The controls in § 6 match the Phase 26 PlayerController exactly." |
| **👨‍💻 Senior Unity Developer** | ✅ Approved | "All system references map to a real script — VillageState, EveningLedgerUI, EchoWebVisualizer, PickleAI, etc. Save mechanism is described to spec." |
| **🔍 Critic & Review Board** | ✅ **Approved** | "This is the player-facing manual the vertical slice needs. It can be shared verbatim with the 20-person playtest cohort with no further edits." |

---

*Document version 1.0 — authored for the `feat/mission-1-2-architecture` branch, Phase 27.1 build. Phase 32 (menu collapse) update — § 17 troubleshooting now points at `Hearthbound → 🚀 Build Everything` (the single top-level entry post-D-051).*
*Companion files: `GAMEPLAY_GUIDE_MISSION_1.md`, `GAMEPLAY_GUIDE_MISSION_2.md`.*
*Part of the Abdulmalek Agents game-concept portfolio · 2026.*
