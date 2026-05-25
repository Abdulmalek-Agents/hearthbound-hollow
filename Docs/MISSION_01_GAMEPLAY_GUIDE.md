# 🫖 Mission 1 — Gameplay Guide: *Opening the Hollow*

> **Player-facing walkthrough · Mission 1 · Day 1 in the Hollow**
> Sourced from `GAME_DESIGN.md`, `Docs/Depth_Bible/Mission_1_2_Focus/01_DORIS_THE_BAKER.md`, `Docs/Depth_Bible/Mission_1_2_Focus/03_SCENES_*.md`, `Docs/Depth_Bible/Mission_1_2_Focus/04_POLISH_AND_CLEANSE_MINIGAMES.md`, `Docs/Depth_Bible/Mission_1_2_Focus/05_DREAM1_AND_DREAM2.md`, and the runtime `Mission01Director.cs`.
> Pair with `MISSION_02_GAMEPLAY_GUIDE.md` after you finish Day 1.

---

## 1. Mission Purpose (What Mission 1 is Trying to Teach You)

Mission 1 has **one job**:

> *To teach you, by your own hands, what a memory orb is. A villager's life made into glass. You polish it. You feel the weight of it.*

Everything else the game ever asks you to do — cleansing widowers, listening to grieving children, brewing memory teas, solving the mystery of the previous Keeper — rests on this one warm moment landing for you. Mission 1 deliberately holds itself to a **single mechanic** (Polish) and a **single villager** (Doris) so the moment can land cleanly.

### The cozy contract for Day 1
- ✅ **No combat. No death. No grind.**
- ✅ **No fail states.** You cannot lose Mission 1.
- ✅ **No time pressure.** Sit at the workbench for an hour if you want. Doris will wait.
- ✅ **Every choice is honored.** Refuse Doris's orb. Underpay her. Polish badly. Skip the workbench. All valid.
- ✅ **Auto-Complete is always one click away** if Polish isn't for you.

---

## 2. Mission Map (Top-Down)

Mission 1 takes place across **two scenes** plus the Main Menu:

```
                 [Main Menu]
                       │
                       ▼
            ┌──── 02_Mission01_Lane ────┐
            │                             │
            │   • Player spawn (south)    │
            │   • Cobble path north       │
            │   • 3 cottages framing      │
            │   • Doris kneading outside  │
            │     her bakery (E to talk)  │
            │   • Hollow door at far end  │
            │     (E to enter)            │
            │   • Door lantern (E to dim) │
            │   • Trees, well, fence,     │
            │     leaves, fog, sunshafts  │
            │                             │
            └─────────────┬───────────────┘
                          │  (walk through door)
                          ▼
          ┌─── 03_Mission01_Hollow ────┐
          │                               │
          │   • Hearth (back-left)        │
          │   • Shelves (west wall)       │
          │   • Workbench (right side)    │
          │     └─ Marin's Note ⨯ E       │
          │     └─ Memory Orb DOR-001     │
          │   • Doris (back of room       │
          │     after she hands you orb)  │
          │   • Pickle the cat            │
          │     (windowsill, watching)    │
          │   • Hanging lantern (E)       │
          │   • East-wall lantern (E)     │
          │   • Window glow from outside  │
          │                               │
          └─────────────┬─────────────────┘
                        │  (after Evening Ledger)
                        ▼
                  [Memory Dream 1
                   — cutscene —]
                        │
                        ▼
            [Mission 2 — The Garden]
```

---

## 3. Controls Cheat-Sheet

| Action | Keyboard / Mouse | Gamepad |
|---|---|---|
| Move | WASD / Arrows | Left stick |
| Sprint (optional) | Left Shift | Left-stick click |
| Jump (optional) | Space | South button |
| Look (camera orbit) | Hold Right Mouse + drag | Right stick |
| Zoom camera | Mouse scroll | LB / RB |
| Interact | **E** | **South button** |
| Advance dialogue | Click / Space / Enter | South button |
| **Polish orb** | Hold left mouse, draw slow circles | Right stick (slow circles) |
| Pause menu | Esc | Start |
| Help / controls card | H | Select |

> Press **H** at any time to see this in-game. The card is in-fiction — Marin (the previous Keeper) wrote it.

---

## 4. Pre-Mission Checklist (First Run Only)

The very first time you load the game, you'll see:

### 4.1 The Tone Compass card (one-time)

> *"Welcome to the Hollow. This is a quiet game about memory, choice, and care. There is no combat. There are no failure screens. There are only choices. Some scenes are heavy. Some are warm. The pace is yours."*

You'll be offered a **Gentle Mode** toggle here. Pick it if you want:
- Longer mini-game timers
- More forgiving Cleanse tolerances (relevant in Mission 2)
- One-line-shorter prose on the heavier memories

You can toggle Gentle Mode any time from the Pause menu (Esc → Settings). **It is not a difficulty.** It is a tone-control.

### 4.2 The Settings panel (optional)

Worth glancing at before you start:

| Setting | Default | Notes |
|---|---|---|
| Gentle Mode | OFF | Softens hard beats; Mission 1 has none so it's largely cosmetic here |
| Auto-Complete Polish | OFF | Turn on if mini-games aren't your thing |
| Auto-Complete Cleanse | OFF | (Won't be used until Mission 2) |
| Subtitle size | Medium | 4 tiers: Small / Medium / Large / Huge |
| Master / Music / SFX / Ambient | individual sliders | Persisted across sessions |

---

## 5. Step-by-Step Walkthrough

### STEP 1 — Main Menu

You see "Hearthbound Hollow" + a rotating cozy tip line + four buttons:

- **Open The Hollow** — start a new game (first run shows the Tone Compass first)
- **Continue** — dim until an autosave exists; lights up after Day 1 ends
- **Settings** — opens the panel from §4.2
- **Quit**

Click **Open The Hollow**. The Tone Compass card appears (first time only). Acknowledge it. Lane scene loads.

### STEP 2 — The Lane (your first scene)

You spawn at the **south end of the lane**, facing north. A wide cobblestone path stretches in front of you. Late-afternoon light filters through autumn alder branches. Falling leaves drift across the path. There's a faint warm hum from the cottage windows.

A title card fades in for ~3 seconds: **"Day 1 — Opening the Hollow"** with the tone-line *"Warm, slightly dusty, late afternoon light."*

> 💡 **Tip:** Click anywhere or press Space to skip the title card if you've seen it before.

You can:
- **Walk forward** (WASD/stick) along the cobble path
- **Look around** (Right Mouse + drag) — try it now, the village is the prize
- **Press H** to see the controls card (Marin's quote)

You'll see:
- **Doris** sitting on a stool outside her bakery, on the left side of the path, kneading bread on a wooden tray on her lap. She looks up as you approach.
- A **hanging lantern at the Hollow door**, already lit
- A **door sign** reading "The Hollow" in soft amber
- A **bench** to the right, a **well** further right, a **wood log pile** beside it
- **Three trees** scattered around, dropping leaves
- **Bushes and grass tufts** along the path edge

### STEP 3 — Meeting Doris

Walk up to Doris (she's slightly left of the central path). Press **E** when the interaction prompt appears.

Her opening lines:
> **Doris:** *"You're the new one. I thought you'd be taller. Don't mind me — I thought that about the old one, too. Come in. The kettle's only just stopped."*

You get **three reply options**:
| You say | What happens |
|---|---|
| "Hello. Are you Doris?" | She introduces herself. Cordial. |
| *(Nod silently and follow her in)* | She approves: *"A quiet one, then. Good. The bread likes quiet."* |
| **"Who was the old one?"** | ⚠️ **This is the secret breadcrumb.** She deflects gently but a flag is set in your save: `asked_about_predecessor = true`. Mission 6+ uses this. |

> 💡 **Cozy Maximizer Pick:** Ask about the old one. It costs nothing now and unlocks a beat later.

She walks inside the bakery. Follow her. (You can also just walk on past her toward the Hollow door — the Lane will still progress, but you'll miss the bakery interior beats. Not recommended for first run.)

### STEP 4 — Inside the Bakery → Receiving the Orb

Doris speaks more lines about the keeper before you, then hands you a small wooden box wrapped around the memory orb.

She offers you DOR-001 — **The First Loaves**.

You get **three more options**:
| You say | Tariff effect |
|---|---|
| "Tell me more about it first." | She tells you. *No cost. The memory becomes more meaningful to you.* |
| "I will. What do you want for it?" | Skips to the price negotiation. |
| "I'd rather not, today." | ✅ **Valid path.** Doris is unbothered. The Hollow still opens. Re-enter Mission 1 to try again. |

Assuming you accept, **the price negotiation** triggers:

> **Doris:** *"Four coppers, if you're asking. It's a small memory. I'll not have you overpay your first day."*

| Your price | Result | State change |
|---|---|---|
| **"Four coppers is fair."** | Honor the asking price. | coin -4; Vow 5 (Honest Coin) +2 |
| **"I'll pay six."** | She refuses: *"That's too much. I'll not have you ruin yourself. Take it back. — Well. Take *some* back. Five, then. Final."* | coin -5; Vow 5 +3; **trust_doris +1** |
| **"Two coppers. That's all I have."** | She accepts gracefully: *"Aye, that'll do. Bring the rest when you find some."* | coin -2; Vow 5 -1; doris_owes_player = 2 (sets up a Mission 3 thread) |

> 💡 **Recommended for first run:** Pay 4 coppers (honor the price). The +trust_doris from over-paying is nice, but the cozy game rewards honesty in coin slowly across many missions.

She hands you the orb in a small wooden box and tells you the Hollow is yours. Walk back outside and proceed to the Hollow door.

### STEP 5 — Entering the Hollow

Walk up to the Hollow door at the north end of the lane. The hanging lantern above the door is already lit (Marin lit it the night she left).

> 💡 **Side beat:** You can press **E on the lantern** to dim or light it. The first time you toggle a lantern, the game's hidden "predecessor trail warmth" stat ticks up +1. (Marin lit them. Now you're tending them.)

Approach the door, press **E** at the prompt: *"Enter the Hollow"*. Scene loads. You're inside.

### STEP 6 — Inside the Hollow

Another title card fades in: **"Day 1 — Inside the Hollow"**.

You see:
- A **wood plank floor** under your feet
- **Four walls** — north wall has a glowing window
- **Ceiling beams** crossing overhead
- A **hearth/firepit** crackling on the back-left (warm Lumen glow)
- **Shelves** on the west wall with pots, baskets, candle stubs
- A **workbench** to your right (this is where you'll polish)
- A **hanging lantern** in the centre, lighting the room warmly
- An **east-wall lantern** for symmetry
- A **wool rug** at scene centre
- **Pickle the cat** on the windowsill, watching you
- A **sack pile** + **crate** + **east-wall banner** for cozy clutter

**Walk past the hearth** — the crackling sound grows louder. (Walk past again — it fades back. That's the `HearthAmbianceTrigger` rewarding proximity.)

### STEP 7 — Marin's Note (optional but high-value)

On the workbench you'll see a small piece of folded parchment with a glowing tag above it: **"✨ a note in Marin's hand"**.

Press **E** four times to read all four passages of her note. Each press advances one passage:

1. *"If you're reading this, the door let you in. The Hollow keeps a person more than the person keeps the Hollow. There is no wrong way to keep a memory — only the gentle way, and the others."*
2. *"A few things, before the kettle whistles: The orbs on the shelves are not yours. They are entrusted. Polish in slow circles. Cover all four faces. If the seam pulls red, you've gone too deep. Stop."*
3. *"The villagers will come. Doris first, with bread on her breath and a question she does not yet know how to ask. Be kind. Be slow. Listen for the second sentence — it is always the true one."*
4. *"And finally: You may find a door that does not want to be a door. That is for later. For today — tea. The shop opens at four. — M."*

After all four passages: the note retires. Your save records `MARIN_NOTE_01_OPENING` and **predecessor trail warmth ticks +5**.

> 💡 **Why this matters:** Passage 2 is the **Polish tutorial in fiction-voice**. The cozy game does not use UI overlays for tutorials. Marin's note tells you exactly how to Polish before you do it.

### STEP 8 — The Polish Mini-Game (~80 seconds)

Approach the workbench. The orb DOR-001 (Doris's "First Loaves") sits in a small cradle on the bench, glowing soft amber.

When the camera tightens onto the workbench, the polish UI activates. You'll see:
- The orb in the centre of your screen, slightly faded with amber-fog
- A small **"Polish for me"** button in the bottom-right (Auto-Complete)
- Marin's note (already read) pinned above

**To polish:** Hold left mouse and **draw slow circles** over the orb's surface. Cover all four faces (you can rotate by sliding to the orb's edges).

**What you'll hear:**
- 0:00 — soft hum starts as the orb wakes up
- 0:05 — friction sound when you make your first circle
- 0:25 — a soft bell chimes (midway milestone — ~30% clear)
- 0:45 — kettle hiss in the background; Doris's morning kitchen becomes visible inside the orb
- 1:05 — composer's "Doris motif" swells (~85% clear)
- 1:15 — success jingle as the orb hits full clarity

**Pickle's reaction** (her first line in the game) plays once you finish:
> **Pickle (telepathic):** *"That was — adequate. I was expecting much worse. Continue, please."*

#### Outcomes by Polish quality

| Outcome | How you got it | Doris's reaction later |
|---|---|---|
| **Perfect** (clarity 1.0) | Covered all sides slowly for ~80s | *"You did it cleaner than I remembered it. I think you'll do."* trust_doris +3 |
| **Acceptable** (clarity ~0.8) | Hurried or partial coverage, ~50s | *"You did it kindly. That's what matters."* trust_doris +2 |
| **Sloppy** (clarity ~0.55) | <30s, walked away mid-polish | *"... It's the morning still. A little dimmer. But mine. First days are like that."* trust_doris +1 |

> ⚠️ **You cannot fail the Mission 1 Polish.** Even the lowest quality is honored by Doris. The mini-game has no real-time pressure — sit at the workbench for half an hour if you like.

#### If you press "Polish for me"
- A ~12-second sped-up cinematic plays of the polish happening on its own
- Outcome = Acceptable quality
- **No penalty.** Doris's after-polish line is the kind one. Achievements are identical.

> 💡 **Auto-Complete is not a "skip."** It's the cozy game's promise that nobody is locked out of content by skill. Use it freely.

### STEP 9 — After Polish

The orb settles back in its cradle, fully clear, glowing softly. Pickle's tail flicks once (approval).

The shop door slides open and Doris walks back in (or you walk back to her — depends on your earlier blocking).

She speaks her reaction line (from the table above), based on your polish quality.

She tells you:
> *"Sleep tonight. Dreams come. I'll see you again, eventually."*

She turns back to her dough. The cozy contract: no goodbye, no fanfare. Just a return to bread.

### STEP 10 — The Evening Ledger

A parchment scroll fades in over the screen. This is the **Evening Ledger** — your end-of-day save + reflection moment.

You'll see:
- **Day 1** (the day index)
- A short prose passage summarizing what just happened (1 of 3 variants based on polish quality)
- The orb you collected today (**DOR-001 — First Loaves**)
- Your coin balance after Doris paid
- Four save buttons: **Slot 1**, **Slot 2**, **Slot 3**, **Autosave**

Pick a save slot (or rely on Autosave — Continue will work either way). Click **"Sleep — End Day 1"**.

> 💡 **Tip:** Manual save slots persist across full quits. Autosave is the one used by the Continue button on the main menu.

### STEP 11 — Memory Dream 1 (the cutscene)

A ~60-second cinematic plays. You see, from inside the orb:

- A morning bakery, the year 1972
- Doris, 24, alone, hands floured, watching her first batch through the oven door
- The loaves pulled out at "the colour of a thing that has decided to be golden"
- She does not yet eat any. She wipes her hands. She looks at the door, and waits to find out whether anyone will come.

> *"She did not, in fact, know yet that she would have customers for forty-seven years. She only knew, at that moment, that the loaves were her own."*

The cutscene ends with a soft fade. Optional subtitles obey your Subtitle Size setting.

> 💡 **Player feeling check:** If you feel a small lump in your throat here — that is exactly what the writer intended. This is what the rest of the game is built on.

### STEP 12 — Hand-Off to Mission 2

After Dream 1, the game transitions automatically into **Mission 2 — The Garden**. Title card fades in: *"Day 2 — The Widower's Request"*.

The autosave slot is updated. You can quit any time. **Continue** on the Main Menu will pick up at the start of Mission 2.

→ Open `MISSION_02_GAMEPLAY_GUIDE.md` to keep playing.

---

## 6. The Polish Mini-Game — Deep Dive

### 6.1 What you're doing, technically

Polish removes age-faded amber-fog from the orb's surface. As the fog clears, the **memory's set-piece scene** (Doris's morning kitchen in 1972) becomes visible *inside* the glass. The orb is a window into the moment Doris is selling you.

### 6.2 The 8 audio cues you'll hear

| When | Cue | What it tells you |
|---|---|---|
| Pick up orb | hum start | Orb is awake |
| Start polishing | rub start | First circle registered |
| Continuous | rub loop | You're working |
| Too fast | friction warn | Slow down — the orb stops hearing you |
| ~30% clear | midway chime | Good progress, keep going |
| ~85% clear | reveal swell (Doris motif) | You're nearly there |
| 100% | success jingle | Done |
| After | hum post | Orb rests, clear |

### 6.3 Tips

- **Slower is better.** Pressing fast triggers the friction warn and slows clarity gain.
- **Cover all four faces.** The orb has a 360° clarity tracker. Don't just polish the front.
- **Pickle's hint at 30s** plays only if you've made <5% progress. If you hear Pickle say *"Slower."* — that's the hint.
- **Press Esc** any time to pause. Doris waits.

### 6.4 The Auto-Complete in detail

The "Polish for me" button is **always visible from frame 1**. One click:
- Camera holds the workbench
- A ~12-second sped-up animation plays (still pretty)
- Outcome lands at Acceptable quality
- Doris's after-polish line is the kindest one
- No achievement penalty, no Pickle disapproval, no coin difference

---

## 7. Memory DOR-001 — The First Loaves (the orb you polish)

| Field | Value |
|---|---|
| ID | DOR-001-FIRST-LOAVES |
| Owner | Doris |
| Season lived | Late summer, age 24 (1972) |
| Anchor | "the warm pull of dough rising under a tea-towel" |
| Setting | Doris's first bakery, the morning it opened, before any customers |
| Palette | JOY + GRACE (no shame, no grief) |
| Weight | 4 / 10 (light) |
| Pre-polish clarity | 6 / 10 |
| Cracks | 0 (no Cleanse needed) |

After polish, the prose unlocks fully (visible in the Codex):

> *"She had been awake since four. The bakery she had paid for — with five years of working in other women's kitchens and one inheritance she had not been expecting — sat around her, smelling, for the first time, of her yeast and her flour and the faint, dignified soot of an oven that had been bricked yesterday..."*

---

## 8. Doris's 4-Node Memory Map

Open the Codex (assigned in a later tutorial — not visible in Mission 1's UI) to see Doris's full Memory Map. Mission 1 reveals the structure:

```
                    ┌───────────────────────────┐
                    │   DOR-001                 │
                    │   The First Loaves        │  ← YOU JUST POLISHED THIS
                    │   (palette: JOY + GRACE)  │
                    └────────────┬──────────────┘
                                 │
                    ┌────────────┴──────────────┐
                    │   DOR-002                 │
                    │   The Wedding Honey       │  ← VISIBLE BUT LOCKED
                    │   (trust-locked)          │     "Doris is not ready to sell this."
                    └────────────┬──────────────┘
                                 │
                    ┌────────────┴──────────────┐
                    │   DOR-003                 │
                    │   A Letter Returned       │  ← VISIBLE BUT LOCKED
                    │   (mystery-locked)        │     "Doris does not yet know that you know..."
                    └────────────┬──────────────┘
                                 │
                              ┌──┴──┐
                              │  ?  │  ← UNKNOWN-LOCKED
                              └─────┘     no tooltip; just a question mark
```

**Why three different lock-states?** This is one screen teaching three different cozy narrative-systems:
- **Trust-lock** — relationships unlock memories (build trust to see DOR-002)
- **Mystery-lock** — some memories you don't yet know how to ask for (DOR-003 unlocks after a clue elsewhere)
- **Unknown-lock** — some memories you don't know exist (the `?` is the hook for late game)

---

## 9. Doris's Dialogue Choices Reference (Quick Table)

| Dialogue node | Your reply | State effect |
|---|---|---|
| Greeting | "Hello. Are you Doris?" | Cordial, neutral |
| Greeting | *(Nod silently)* | She approves: "The bread likes quiet." |
| Greeting | **"Who was the old one?"** | 🔓 Sets `asked_about_predecessor` — affects Mission 6+ |
| Bakery interior | "Thank you." / "Is the shop ready?" | Both pleasant, no penalty |
| Orb offer | "Tell me more about it first." | She tells you Margery's prose pre-summary |
| Orb offer | "I will. What do you want for it?" | Skips to price |
| Orb offer | "I'd rather not, today." | ✅ Valid refusal; Hollow still unlocks |
| Price | "Four coppers is fair." | coin -4; Vow 5 +2 |
| Price | "I'll pay six." | coin -5; Vow 5 +3; trust_doris +1 |
| Price | "Two coppers. That's all I have." | coin -2; Vow 5 -1; doris_owes_player = 2 |

---

## 10. State That Carries Forward to Mission 2

By the end of Mission 1, your `VillageState` will look something like:

| Dimension | Default | Typical Mission 1 result |
|---|---|---|
| `trust_doris` | 0 | **+3 to +5** depending on polish quality + over-paying |
| `memory_integrity[DOR-001]` | 100 | 100 (perfect polish) or 95 (acceptable) |
| `public_standing` | 0 | +2 |
| `vow_1_consent` | 50 | +1 (you gave Doris a real transaction) |
| `vow_3_whole` | 50 | +3 (you polished her well) |
| `vow_5_honest_coin` | 50 | +2 to +3 depending on price |
| `vow_7_last_light` | 50 | 0 (not exercised in M1) |
| `pickle_approval` | 50 | +2 |
| `predecessorTrailWarmth` | 0 | +5 (Marin's Note) + 1 (lantern) |
| `coin` | 10 | 4 to 8 |
| `hollow_level` | 1 | 1 |
| `memories_in_inventory` | 0 | **1 (DOR-001)** |
| `tutorialCompleted` | false | true (Help overlay was closed) |
| `toneCompassAcknowledged` | false | true |

Mission 2 reads `trust_doris`, `pickle_approval`, and `predecessorTrailWarmth` to subtly modify Gerrold's first conversation. **The game is watching from now on.**

---

## 11. Optional Discoveries (Worth Your Time on Day 1)

| Discovery | How to find it | Why bother |
|---|---|---|
| Marin's Note (workbench) | Press E on the parchment in the Hollow | Reveals Polish tutorial + sets `MARIN_NOTE_01_OPENING` |
| Hanging lantern toggle (Lane) | Press E on the door lantern | First-time toggle adds +1 predecessor trail warmth |
| Hollow lanterns (Hollow) | Press E on the ceiling lantern + east-wall lantern | Each first toggle adds +1 |
| Hearth crackle | Walk past the hearth slowly | Ambient audio crossfades up — pure mood, no stat |
| Doris's beehive | Walk around the side of the bakery | Subtle visual prop; seed for Mission 3+ Wedding Honey arc |
| Ask Doris about "the old one" | Use that dialogue option in the greeting | Flags `asked_about_predecessor` — affects later missions |
| Over-pay Doris | Pick "I'll pay six" | +1 trust_doris that compounds across Mission 6+ visits |

> 💡 **Cozy completionist tip:** Doing all of these on Day 1 costs nothing and unlocks compound rewards later. Estimate: ~12 extra minutes vs. the fast-path.

---

## 12. Common Mistakes (and Why They're Fine)

| "Mistake" | What actually happens |
|---|---|
| I walked past Doris without talking | The Hollow door still opens. You miss the orb purchase but Doris is unbothered. Re-enter Mission 1 to try again. |
| I picked "I'd rather not, today" | Hollow still unlocks. Doris waits. Mission 1 progresses without the orb on your shelf. You can come back. |
| I polished for only 20 seconds and walked away | Orb stays at low clarity. Doris's third reaction line plays ("First days are like that. I won't hold it."). No penalty. |
| I pressed Auto-Complete | Nothing bad happens. Doris's reaction is identical to the acceptable-manual path. |
| I didn't read Marin's Note | The Note is still readable later (it appears on the workbench every time you re-enter the Hollow until you finish reading it). |
| I sat in the Hollow for 20 minutes doing nothing | Doris doesn't get impatient. Pickle yawns. The hearth keeps crackling. **This is allowed.** |

**There are no Mission 1 failures.** The cozy contract is absolute.

---

## 13. Accessibility & Comfort Tools (Pause Menu → Settings)

| Tool | What it does |
|---|---|
| **Gentle Mode** | Longer mini-game timers; softer Cleanse tolerances (relevant in Mission 2); 1-line-shorter prose on heavy memories. No effect on Mission 1's lightness. |
| **Auto-Complete Polish** | Skip the Polish mini-game cleanly. |
| **Auto-Complete Cleanse** | Skip the Cleanse mini-game (Mission 2+). |
| **Subtitle Size** | 4 tiers. Settings persists across sessions. |
| **Master / Music / SFX / Ambient volumes** | Independent sliders. |
| **Tone Compass** | Reopen the first-run card from Settings if you want to re-read it. |

> 💡 **All accessibility tools cost zero achievement progress.** They are not a "harder/easier" difficulty. They are tone-controls.

---

## 14. Pickle the Cat — Your First Companion

Pickle is the Hollow's resident cat. She sits on the windowsill watching you. In Mission 1 she has **exactly one voiced line** — after your polish completes:

> **Pickle:** *"That was — adequate. I was expecting much worse. Continue, please."*

That is it. Her library opens slowly across the game. **By Mission 6 she has ~50 voice lines.** Her affection (`pickle_approval`) is a hidden stat — it influences which lines she speaks in later moments. Mission 1 ends with Pickle at ~52.

> 💡 **Pickle is not a quest character.** You don't pet her. She doesn't follow you. She is the steady, sceptical, slightly aristocratic centre of the Hollow. The cozy game's wry conscience.

---

## 15. Glossary (Mission 1 Terms)

| Term | Meaning |
|---|---|
| **The Hollow** | The memory-brokerage shop you've inherited |
| **Marin** | The previous Keeper; left signed notes ("— M.") around the shop |
| **Memory orb** | A villager's life-moment made into glass; tradable, polishable |
| **Polish** | The mini-game for restoring clarity to a faded memory |
| **Tariff** | The price-and-consequence package on a memory transaction |
| **Vow Integrity** | 7 ethical dimensions you accumulate or lose across choices |
| **Pickle** | The Hollow's cat; speaks telepathically; arch and rare |
| **Pickle Approval** | Hidden 0–100 stat; gates her commentary lines later |
| **Predecessor Trail Warmth** | Hidden 0–100 stat tracking how connected you feel to Marin |
| **Tone Compass** | The first-run card explaining the cozy contract |
| **Gentle Mode** | An accessibility tone-control (longer timers, softer prose) |
| **Codex** | The collection menu where your gathered memories live |

---

## 16. Mission 1 Summary (One-Page Quick Reference)

```
GOAL: Polish Doris's "First Loaves" memory.

PATH:
  Lane spawn → walk to Doris → ask about Marin (optional) → enter bakery →
  receive orb → pay 4-6 coppers → exit → enter Hollow → read Marin's Note →
  Polish at workbench (~80s) → Pickle's line → talk to Doris → Evening Ledger
  → Memory Dream 1 → Mission 2.

DURATION: 25-40 real-time minutes.

NEW MECHANICS: Move, Interact, Dialogue Choice, Polish mini-game, Save.

NO FAIL STATES.  AUTO-COMPLETE ALWAYS AVAILABLE.

SECRET BREADCRUMBS:
  • "Who was the old one?" dialogue choice
  • Reading Marin's Note (all 4 passages)
  • Lighting/dimming the door lantern at least once
  • Walking around to Doris's beehive

NEXT: Memory Dream 1 → MISSION_02_GAMEPLAY_GUIDE.md
```

---

*Player guide v1.0 — sourced from Depth Bible Mission_1_2_Focus 01, 03, 04, 05, 06, 07 + runtime Mission01Director.cs + Yarn script Doris_M1.yarn*

*Have a question this guide didn't answer? File an issue on the repo with the tag `gameplay-guide-m1`.*
