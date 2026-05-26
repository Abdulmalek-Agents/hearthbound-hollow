# 🕯️ Mission 2 — Gameplay Guide: *The Widower's Request*

> **Player-facing walkthrough · Mission 2 · Day 2 in the Hollow**
> Sourced from `GAME_DESIGN.md`, `Docs/Depth_Bible/Mission_1_2_Focus/02_THE_WIDOWER_GERROLD.md`, `Docs/Depth_Bible/Mission_1_2_Focus/03_SCENES_*.md`, `Docs/Depth_Bible/Mission_1_2_Focus/04_POLISH_AND_CLEANSE_MINIGAMES.md`, `Docs/Depth_Bible/Mission_1_2_Focus/05_DREAM1_AND_DREAM2.md`, `Docs/Depth_Bible/Mission_1_2_Focus/06_TEA_LOOP_AND_MORAL_CHOICE.md`, and the runtime `Mission02Director.cs`.
> Finish `MISSION_01_GAMEPLAY_GUIDE.md` first — Mission 2 reads your Day 1 state.

---

## 1. Mission Purpose (What Mission 2 is Trying to Teach You)

Mission 1 taught you what a memory orb is. Mission 2 teaches you **what choosing what to do with one means**:

> *A memory transaction is a moral act. There are four valid ways to handle a grieving widower's last week with his wife. Three of them transform the memory. One of them does not. The village will know which one you chose.*

This is the cozy game's **emotional weight pivot.** Mission 1 was warm. Mission 2 is **warm + weighted.** The cozy contract still holds — there is no game-over, no punishment, no "wrong" choice — but the world now responds to your choices in ways you'll feel for hours.

### The cozy contract still holds for Day 2
- ✅ No combat. No death. No grind.
- ✅ **No "failure".** Every outcome — even the worst — is fully narratively absorbed.
- ✅ **No timer on the moral choice.** Sit with it as long as you like.
- ✅ **Auto-Complete is one click away** for the Cleanse mini-game.
- ⚠️ **There is now a real consequence band.** Crossing the core during Cleanse permanently changes Gerrold's memory. The change is recoverable in Mission 6+ but it is real.

---

## 2. Mission Map (Top-Down)

Mission 2 takes place across **two new scenes** plus the Hollow you already know:

```
                  [Mission 1 end — Day 1 Dream]
                              │
                              ▼
                ┌─── 03_Mission01_Hollow ───┐
                │                             │
                │  (you wake up here on       │
                │   Day 2 morning at ~07:30)  │
                │                             │
                │   Gerrold knocks at the     │
                │   door. He has been         │
                │   standing there 20 min.    │
                │                             │
                └──────────────┬──────────────┘
                               │  (3 dialogue sub-paths)
        ┌──────────────────────┼─────────────────────────┐
        │                      │                         │
        ▼                      ▼                         ▼
    [walk back               [work at                [send Gerrold
    Hollow alone]            Hollow with             home; meet
                              Gerrold]                at his cottage]
        │                      │                         │
        └──────────┬───────────┴────────────┬────────────┘
                   │                        │
                   ▼                        ▼
       ┌─── 04_Mission02_Garden ───┐    (skip the garden →
       │                            │    straight to cottage)
       │   • Lavender plot (3)      │
       │   • Valerian plot (3)      │
       │   • Empty plots (2 — M3+)  │
       │   • Kettle on the stove    │
       │     (returns you here      │
       │     after Marin's note)    │
       │                            │
       └──────────────┬─────────────┘
                      │
                      ▼ (with tea in a small wooden box)
            ┌─── 05_Mission02_Cottage ───┐
            │                              │
            │   • Two chairs by fireplace  │
            │   • Cup of cold tea in front │
            │     of Margery's chair       │
            │   • Open book on her chair   │
            │   • The 4-option moral       │
            │     choice screen            │
            │                              │
            └────────────────┬─────────────┘
                             │
        ┌────────────────────┼─────────────────────┐
        │                    │                     │
        ▼                    ▼                     ▼
   [A — Erase]          [B — Cleanse]         [C — Listen]      [D — Defer]
        │                    │                     │                  │
        ▼                    ▼                     ▼                  ▼
   Cleanse mini-game    Cleanse mini-game    3-min Listen       Mission 2 ends.
   (aggressive          (careful             cutscene —        Gerrold returns
    profile)             profile)             no mini-game.    Day 3.
        │                    │                     │                  │
        ▼                    ▼                     ▼                  ▼
   Dream 2 variant      Dream 2 variant      Dream 2 variant   Dream 2 variant
   A or C               B or C               D                 E (deferred)
        │                    │                     │                  │
        └────────────────────┴──────────┬──────────┴──────────────────┘
                                        │
                                        ▼
                           Evening Ledger (5 outcome variants)
                                        │
                                        ▼
                              [Main Menu — Mission 3 future]
```

---

## 3. State Mission 2 Reads From Mission 1

Before any dialogue plays, `Mission02Director` checks these fields from your save:

| Field (from Day 1) | How it influences Mission 2 |
|---|---|
| `trust_doris` (3–5) | Pickle's commentary is unlocked at trust_doris ≥ 3 |
| `pickle_approval` (50–55) | Pickle's pre-choice commentary requires pickle_approval ≥ 50 |
| `asked_about_predecessor` (true/false) | Subtle alternate Gerrold opening line if true |
| `predecessorTrailWarmth` (5–8 typical) | Affects which Pickle line plays on Day 2 |
| `coin` (4–8) | Mission 2 doesn't charge you — Gerrold offers no coin transaction. But your wealth carries to M3+. |
| `memories_in_inventory` (1) | DOR-001 is on your shelf and visible from the workbench all of Day 2 |

> 💡 **The game is now visibly tracking you.** Every choice in Mission 1 (even the silent ones — like reading Marin's Note) subtly conditions Mission 2's tone.

---

## 4. Pre-Mission Recap (Skip if You Just Finished M1)

If you're returning from a Continue, here's the gist:

- You inherited **the Hollow**, a memory-brokerage shop, from someone named **Marin** (who left signed notes around the shop)
- On Day 1 you met **Doris the baker**, polished her memory "First Loaves", and put it on your shelf
- **Pickle the cat** sits on the windowsill of the Hollow
- The Tone Compass is acknowledged; Gentle Mode is whatever you set it to
- You have ~4-8 coppers, +3 to +5 trust with Doris, and ~52 Pickle approval

Day 2 begins.

---

## 5. Step-by-Step Walkthrough

### STEP 1 — Waking up in the Hollow (Day 2 morning)

The Dream 1 cutscene fades out. Title card: **"Day 2 — The Widower's Request"** with tone-line *"A grey, snowed-on morning. Smoke from one chimney down the lane."*

You're standing in the Hollow. The hearth is dimmer than last night (you slept through it). The shelves have **DOR-001 — First Loaves** glowing soft amber. Pickle is on the windowsill, watching the door.

**Pickle (telepathic, internal):**
> *"There is a man at your door. He has been there for twenty minutes. He is a careful sort. The careful kind hurt the worst. ... I would let him in."*

You walk to the door. A knock — once. Then silence.

### STEP 2 — Opening the door to Gerrold

Press **E** on the Hollow door (now opening outward instead of inward).

You see **Gerrold Pell** — a man in his 60s, in a long brown coat, holding something white folded in his hands. He has been standing there for 20 minutes. He doesn't quite meet your eyes.

> **Gerrold:** *"I'm sorry. Are you the new keeper? I should have written first. I don't know if there's a way to do that. ... My name is Gerrold Pell. I — could I come in for a moment?"*

**Three opening reply options:**

| You say | What it sets up |
|---|---|
| **"Yes. Please."** | Direct, warm. Gerrold relaxes slightly. |
| "What's this about?" | Slightly clipped. Gerrold apologizes more. *"It's a memory. I brought one. I'm sorry — that's what one does, isn't it?"* |
| ***(Step aside silently and gesture him in)*** | Gentlest path. *"Thank you. You are kind."* Pickle's pre-choice commentary later softens. |

> 💡 **Cozy Maximizer Pick:** Step aside silently. The wordless welcome is what a grieving man needs.

### STEP 3 — Gerrold sits

He walks in carefully, sits in the chair you offer. **He does not take tea.** (You won't know why until Mission 6 — Margery used to brew his tea.)

He places the white handkerchief on the counter and slowly unfolds it. Inside is a **memory orb** — visibly cracked, faded, heavy. This is **GER-007 — The Last Week**.

His first major exposition:
> **Gerrold:** *"I have a memory I want to be rid of. Not the whole thing. Just the long bit. My wife — Margery — died last winter. Eleven months last week. I have the memory of the last week of her life. It is the heaviest thing I own. I want to keep my wife. I do not want to keep the long bit."*

**Three branching responses:**

| You say | What unlocks |
|---|---|
| **"What was the long bit?"** | He explains: 14 months of illness, the last 6 days awake watching. Sets up the cleansing context. |
| **"Tell me about Margery first."** | 🔓 Unlocks the **Margery exposition node** — he describes her quiet voice, her 30-year friendship with Doris, her gift for being quiet around someone she loved without making it feel like distance. **High emotional payoff.** |
| ***(Just nod and wait)*** | He proceeds gently. *"Thank you. I'll get to it."* |

> 💡 **Highly recommended:** Pick "Tell me about Margery first." You'll meet a person who isn't there. The cozy game's most reliable craft is its off-screen characters.

### STEP 4 — Inspecting the orb (the Echo Web's first activation)

Gerrold slides the cloth-wrapped orb forward. You pick it up *from inside her handkerchief* — your first physical contact with Margery's needlework.

**The Echo Web UI lights for the first time.** A line of warm amber light connects **DOR-001** (on your shelf) and **GER-007** (in your hand) on the Codex page. The shared echo: *"Doris in kitchen, Sunday morning."*

**Pickle (telepathic):**
> *"Ah. The Web has decided to speak today. This one and yesterday's are in the same kitchen on the same Sunday. That is what the line of light is saying. Look at it. It is showing off."*

> 💡 **What this means:** Memories know each other. Doris and Margery were best friends for 30 years. Doris was in the Pell kitchen the morning Margery died. **The whole village is one woven thing.** The Echo Web will keep finding connections as you collect more memories.

You can ask Gerrold three things now:

| You say | What happens |
|---|---|
| **(Show Gerrold the orb's cracks)** | He says: *"Aye. I knew it was — uneven. I've not handled it well. I've held it too tight, perhaps."* |
| ***(Hold the orb silently)*** | He says: *"You are looking at it for a long time. Take your time. It is — it is the heaviest thing I own."* |
| **"I need to think. Can you come back tomorrow?"** | 🟡 **Defer path engaged.** See §5 Step 14d. Mission 2 ends here for now; Gerrold returns Day 3. |

### STEP 5 — Where do we do this?

Gerrold gives you three options for where the cleansing decision happens:

| Option | What it means | Recommended |
|---|---|---|
| **"Let's walk to your cottage. I'd like to see where she lived."** | A 90-second walk through the village, Lumen lantern halos overhead, Gerrold telling you 6 lines about Margery. The most emotionally rich path. | ✅ Yes |
| **"I'd like to do it here. Stay if you want."** | Faster. Less emotional context. Some Pickle commentary unlocks here that doesn't in the cottage path. | Mixed |
| **"Go home. I'll come find you when it's done."** | Gerrold leaves. You work alone. Lowest emotional weight. Trust gain reduced. | Not recommended for first run |

Cozy Maximizer Pick: **Walk to the cottage with Gerrold.** The walking-together beat is where Mission 2's narrative density lives.

### STEP 6 — (Optional, only if you stayed at Hollow) Going to the Garden

If you chose to work at the Hollow, you can step out the back to the **Garden scene** (`04_Mission02_Garden.unity`).

The Garden has:
- **3 Lavender plants** (front-left)
- **3 Valerian plants** (front-right)
- **2 empty plots** (saved for Mission 3+ planting)
- A **wood stove with a kettle** (always near-boiling — Marin-magic)
- The exit back to the Hollow

You can:
- **Harvest 1 Lavender** (press E on plant) — adds 1× Lavender to inventory
- **Harvest 1 Valerian** (press E on plant) — adds 1× Valerian to inventory
- **Skip the garden entirely** — walk back inside without an herb. ✅ Valid path. The cottage scene just plays the No-Tea Default branch.

> 💡 **You can harvest at most 2 herbs total in Mission 2** (one of each, or two of one). One cup of tea = one herb. You will only offer one cup. Pick what feels right.

### STEP 7 — Brewing tea (if you harvested an herb)

Approach the kettle on the wood stove. Press **E**: *"Brew tea"*. A Bamao parchment-frame UI appears.

Drag the herb icon into the kettle. **Wait 90 in-game seconds (~12 real-time seconds)**. Steam rises. Pickle, if present, yawns. The composer's main theme plays softly.

At the 6-second mark, **Pickle's only garden-brewing line**:
> *"Tea for a man who has not asked for tea is the rarest kindness. Try not to overthink it. The man is also a cat about this. We will accept."*

Pour into a cup. The pour cue is a 3-second composer micro-cue:
- **Lavender** → resolves into C-major (warm)
- **Valerian** → resolves into D-minor (uneasy)

A small wooden tea-carrier box is placed in your inventory.

> 💡 **The 12 real-time seconds of waiting are intentional.** This is the cozy rhythm. Most playtesters report this as a favorite moment.

### STEP 8 — Walking to Gerrold's cottage

Whether you brewed tea or not, you now walk down the side lane toward Gerrold's cottage. If you accepted Gerrold's "walk with me" offer back in Step 5, he walks beside you.

The walk takes ~30 seconds of real time. **Gerrold speaks 6 lines about Margery during the walk** (only if he's walking with you):

> *"It's been a fair autumn. Cold mornings, but bright. Margery liked the autumns best. She said the trees gave the village manners. That is the kind of thing she would say. I never knew how to answer her, exactly. I just smiled. She did not mind."*

> *"She taught me how to be quiet around someone you love without making it feel like distance. I am very loud, now, on my own. I keep wanting to say things that I do not have to say to anyone."*

> *"This is — this is part of why I came to the Hollow. I cannot keep being this loud."*

This is the **village-walk teaching beat.** You learn more about Margery during this walk than from any other source in Mission 2.

You arrive. Gerrold pauses at the door: *"Here. This is the door. Watch the step — the wood gave last winter and I haven't planed it true yet."*

### STEP 9 — Inside the cottage

The interior reveal:

- A small main room with **two chairs by a fireplace**
- A **cup of cold tea** in front of the right chair (Gerrold places one daily — Margery's chair)
- A **book** on the right chair's arm, open to a page Margery never finished
- A small **table set for two** — one place setting fresh, one unused but laid out

> 💡 **The cup of cold tea is the room's heaviest object.** Gerrold has been placing one in front of Margery's empty chair every day for 11 months. He will not explain.

He says:
> **Gerrold:** *"Have a seat. Either chair."*

**This is a real choice.**

| You sit in | Pickle comments (if pickle_approval ≥ 50) | Gerrold's reaction |
|---|---|---|
| **Left chair (Gerrold's chair)** | *"Aye. That's the right one for you."* | *"I'll take Margery's, today. She wouldn't have minded most things."* |
| **Right chair (Margery's chair)** | *"You sat in the dead wife's chair. He is too polite to tell you. But he is grateful. I would not have done it. But you are not a cat."* | *"... Aye. That's all right. I should have said which was mine. I should — I should say it more, perhaps."* Sets `sat_in_margery_chair = true` |
| **Stand by the fireplace** | (no Pickle line) | *"As you like. The fire's still warm — Doris stopped by yesterday and lit it."* |

> 💡 **Sitting in Margery's chair is the highest-emotional-weight option.** Gerrold is gently moved that you did. The cozy player who notices the chair and chooses it anyway gets the warmest reading from Gerrold's body language.

### STEP 10 — Offering the tea (if you brewed one)

If you carried tea, this is the moment. Press **E** to offer the cup to Gerrold.

He accepts. He takes one sip. Then he speaks one of two Margery-revealing lines:

**Lavender path:**
> *"Margery and I had this tea on our last winter together. We had it every evening. She said it tasted like the inside of a folded sweater. I knew what she meant. I still do."*
> → **+2 trust_gerrold immediately.** The Listen option appears first in the choice screen.

**Valerian path:**
> *"Aye. This is the one she used to make me when I could not sleep. She would put a sprig of it on the pillow when I — when I could not stop thinking. I have not had this in eleven months. ... Thank you. You are very kind."*
> → **+2 trust_gerrold.** The Erase option is no longer hidden behind a confirmation. ⚠️ Cleanse mini-game's core region becomes slightly *larger* (more forgiving to the cursor but with greater narrative risk).

> 💡 **Neither tea is "better."** Lavender nudges you gently toward Listen; Valerian nudges you toward Cleanse/Erase. Either tea can lead to any final choice. The system trusts you.

### STEP 11 — The 4-Option Moral Choice

Gerrold says:
> *"Well, then. I've said what I've come to say. I'd like to give you the choice."*

A parchment scroll fades up over the camera. You see four options, each with a single-sentence consequence preview in fiction-voice. **No timer. No stat numbers. No "morality meter."**

```
       Gerrold has placed the orb on the table.
       He is waiting.

       ─────

       A — Erase the long bit.
           "He will remember he loved her.
            He will not remember the dying."

       B — Cleanse with care.
           "He will keep the memory, gentler.
            You will need a careful hand."

       C — Don't take the memory. Sit with him.
           "He will carry it. You will have only
            been here. Sometimes that is enough."

       D — Ask him to come back tomorrow.
           "He will. You can think about this
            for a day."
```

If Pickle is in the room (`pickle_approval ≥ 50`), she comments on each option *before* you commit (you can hover/highlight to hear her line):

| Option | Pickle's pre-confirm line |
|---|---|
| **A — Erase** | *"You are about to erase a week of a life. That is — that is a large thing. I am not your conscience. I am only telling you what the verb is."* |
| **B — Cleanse** | *"This is the verb the Keeper before us liked best. I think — I think she would be pleased. Or angry. She had moods."* |
| **C — Listen** | *"This is the older craft. The one nobody pays for. The good keepers all do it eventually."* |
| **D — Defer** | *"A day is a fair thing to ask for. I have asked for many."* |

#### Confirmation flow

- **Option A (Erase)** requires a **double-confirm**. The first click expands the preview text and shows a final "Are you sure?" pause. *Then* the second click commits.
- **Options B, C, D** confirm with a single click.

> 💡 **The friction is the design.** The heavier the choice, the more it asks of you. This is the cozy game's gentle moral safeguard.

---

## 6. Branch A — Erase the Long Bit

After confirming twice:

> **Gerrold:** *"... Aye. The long bit gone. All of it. I — I want to be certain that's what I'm asking for. I'll be here."*

You walk back to the Hollow alone. The Cleanse mini-game launches at the **aggressive-difficulty profile**:
- Core region slightly larger
- Crack tolerance slightly tighter
- The composer cue is in a dissonant minor key

### Cleanse outcomes for Erase path

| Mini-game result | What happens to the orb | Gerrold's reaction |
|---|---|---|
| **Perfect or Acceptable** (no core touches) | The long bit is gone. Margery's last week is *erased.* Margery's face is *intact* in the orb. | *"... I can feel that it's lighter. I can still see her face. I cannot — I cannot quite see the last week. Thank you. I think this is what I asked for. I am not certain it is what I needed."* |
| **Crossed Core** (you touched the centre during the trace) | Margery's *face* has faded. Gerrold has erased more than he asked for. | *"... I can't see her clearly. That wasn't — That wasn't — It isn't your fault. It is — it is the cloth and the cold and the long winter. I will go home. Thank you for trying."* ⚠️ **Mission 6 recovery arc seeds here.** |

### Tariff (Erase path)

| Dimension | Change |
|---|---|
| `trust_gerrold` | +5 |
| `vow_1_consent` | -2 (he asked, but you took more than was wise) |
| `vow_3_whole` | -3 (you over-cleansed) |
| `memory_integrity[GER-007]` | 50 (Perfect/Acceptable) or 25 (Crossed Core) |
| `pickle_approval` | -3 |

> 💡 **The Erase path is a valid cozy choice.** Some players choose it as compassion: Gerrold asked to be rid of the long bit, you gave him exactly that. The game does not punish it. But Pickle notes — and you'll feel — that he asked for less than he needed.

---

## 7. Branch B — Cleanse with Care

> **Gerrold:** *"Aye. ... With care, then. Whatever you can do. I trust you. I — I don't know you, but I trust you."*

You walk back to the Hollow alone. The Cleanse mini-game launches at the **careful-difficulty profile** (standard).

### The Cleanse mini-game (detailed)

Approach the workbench. The orb sits in the cradle. Its surface has **4 visible cracks** in 3 regions:

```
                              ╔═════════════════╗
                              ║      THE ORB    ║
                              ║   ╱╲       ╱╲   ║  ← "the bedside table"
                              ║  ╱  ╲     ╱  ╲  ║     (2 short cracks)
                              ║                 ║
                              ║   ┌───────┐     ║
                              ║   │  CORE │     ║  ← amber-glowing centre
                              ║   │       │     ║     (Margery + the morning)
                              ║   └───────┘     ║
                              ║     ╱─╱─╱       ║  ← "Doris in kitchen"
                              ║                 ║     (1 medium crack)
                              ║    ╲─╲─╲─╲      ║  ← "the seventh morning"
                              ║                 ║     (2 deep cracks)
                              ╚═════════════════╝
```

**To Cleanse:** Click at a crack's start point. Drag your cursor along the crack's path without lifting. The crack visibly seals behind your cursor. Release at the crack's end point.

**Crack-by-crack tips:**

1. **Start with "the bedside table" cracks** — they're shortest and let you learn the tolerance
2. **"Doris in kitchen"** is in the middle — straightforward but requires steady hand
3. **"The seventh morning"** is the hardest — 2 deep cracks close to the core. Take your time.

**The core (warm amber centre) is forbidden territory.** If your cursor enters the core's outer aura, you'll hear the **`cleanse_core_warn`** chime — urgent, audible. Pull back immediately.

If you cross fully into the core: **`cleanse_core_damage`** plays — a 1.4-second "chime of regret" — and Margery's face visibly fades in the orb. `memory_integrity[GER-007]` drops 25.

### The four Cleanse outcome states

| Outcome | How you got it | Consequence |
|---|---|---|
| **Perfect** | All 4 cracks sealed cleanly. 0 core crossings. ~90 seconds. | Triumph. Gerrold's warmest reading. |
| **Acceptable** | 3 of 4 cracks sealed. 0 core crossings. (1 crack remains partially open.) ~75 seconds. | Quiet relief. Gerrold's gentle reading. |
| **Sloppy** | 4 of 4 cracks sealed but with 1–2 brief core touches. | `memory_integrity` at 80. Gerrold notices something is *slightly* off but can't articulate it. |
| **Crossed Core** | 3+ core touches OR a sustained core crossing (>1 second inside). | Margery's face fades. Real loss. Mission 6 recovery arc seeded. |

### Gerrold's after-cleanse lines (Branch B)

**Perfect:**
> *"Aye. I can feel it. The blame is out of me. The dying is still there. But I can — I can carry it now, I think. Margery is — Margery is whole. ... Thank you. I do not know what to say. I will go in and read for a bit. ... Come back when you can. Doris's bread is on Sundays. We could — we could perhaps share one."*

**Acceptable:**
> *"Aye. It is lighter. Not as light as I'd hoped, but — I do not think I should be as light as I'd hoped, perhaps. Margery is here. I am here. The long bit is — it is part of me, still, but it is no longer the only part. Thank you. I will see you in the village square next week."*

**Crossed Core:**
> *"... Mmm. I cannot quite — I cannot quite see all of her. It is — it is not what I asked for. But it is not your fault. Thank you for the try."* ⚠️ Mission 6 recovery arc seeds here.

### Tariff (Cleanse path)

| Dimension | Change (Perfect / Acceptable / Crossed) |
|---|---|
| `trust_gerrold` | +4 / +4 / +3 |
| `vow_1_consent` | +3 |
| `vow_3_whole` | +5 / +3 / -1 |
| `memory_integrity[GER-007]` | 95 / 90 / 70 |
| `pickle_approval` | +4 |

> 💡 **Cleanse is the canonical "good Keeper" choice.** It's what the writing nudges toward — what Marin would have done. The mini-game asks you to be careful. The narrative rewards you being careful.

### Auto-Complete for Cleanse

The **"Cleanse for me"** button is in the bottom-right of the screen from frame 1.

| Difficulty | Auto-Complete outcome |
|---|---|
| Default | **Acceptable** (3 of 4 cracks sealed, no core damage) |
| Gentle Mode | **Perfect** |

**There is no penalty for using it.** Gerrold's after-cleanse line uses the Acceptable variant. The narrative beats are identical.

---

## 8. Branch C — Listen (Don't Take the Memory)

This is the **only** choice that does *not* trigger the Cleanse mini-game. Instead, a **3-minute cinematic scene** plays.

> **Gerrold:** *"... I don't — I don't understand. You — you don't want to take it?"*

Two reply options, both lead to the Listen scene:

| You say | Effect |
|---|---|
| **"I want to listen. Tell me about it. Hold it the whole time. I won't take it."** | Direct. `vow_7_last_light` += 5 |
| **(Reach out and gently place his hands around the orb)** | Wordless. `vow_7_last_light` += 7. ⭐ The cozy game's most cherished beat. |

### The 3-Minute Listen Cutscene

Gerrold holds the orb in both hands in his lap. The camera holds tight on his hands and the orb. The composer plays a single sustained cue (the "Margery prelude") at very low volume throughout.

**0:00–0:15** — You sit down. Gerrold sits across. Pickle (if present) sits on the windowsill. The fire crackles.

**0:15–0:45** — Gerrold begins. He describes the room — the bedside table, the lamp, the snow outside. His voice is steady. He has rehearsed these sentences alone for 11 months.

**0:45–1:30** — He tells the middle of the week. The small motions. The handkerchief. The way Margery looked at her own embroidery — *"like a stranger she was pleased to meet."*

**1:30–2:15** — He tells the seventh morning. *He does not cry.* He gets through it.
> *"The snow had stopped. Doris was in the kitchen. I made tea. I brought it in. She — was already gone."*

**2:15–2:45** — He falls silent. Holds the orb. Looks at you — *for the first time in eleven months, looks someone directly in the eyes while speaking about her.*
> *"Thank you. I did not know it would be — easier — when said aloud. It is still heavy. It is no longer alone."*

**2:45–3:00** — He rises. Walks to the mantel. Sets the orb beside the **wedding photograph**. *He keeps the orb.* Returns to his chair. The fire crackles.

You can stand or remain seated. No prompt. Just held space.

### Tariff (Listen path)

| Dimension | Change |
|---|---|
| `trust_gerrold` | +6 (the highest of any path) |
| `vow_7_last_light` | +10 (the cozy game's "presence" virtue) |
| `vow_3_whole` | +0 (you did not Cleanse) |
| `memory_integrity[GER-007]` | 100 (untouched) |
| `pickle_approval` | +5 |
| `cinder` | **+3** (first earning of the Confession Booth's currency, used Mission 4+) |

### Gerrold's reaction (Listen)

> *"I told you the whole memory. I said it aloud for the first time in eleven months. I did not know that I would still have it once I'd said it. ... I still have it. It is — it is still heavy. But it is — it is no longer alone with me. Thank you for sitting. That was — that was a kindness I had not known how to ask for. Come for tea on Sunday. Doris brings the bread."*

> 💡 **This is the cozy-mature path.** The game considers *being present* mechanically equivalent to *doing the work.* The Listen scene is rewarded as fully as the Cleanse — by Trust, by Vow 7, by Pickle, by the unique Cinder currency, and by Gerrold's openness to a future Sunday tea.

---

## 9. Branch D — Defer (Ask Him to Come Back)

> **Gerrold:** *"Aye. A day, then. I will come back. I will not be impatient."*

He gently rewraps the orb in Margery's handkerchief. He leaves the cloth on your counter — *"I will leave the cloth here, if you don't mind. Take care of it for the night."* He goes home.

You walk back to the Hollow alone. Day fades. The Evening Ledger appears (Variant E — see §11). Memory Dream 2 plays its **"deferred"** variant (Variant E — see §10).

### Tariff (Defer path)

| Dimension | Change |
|---|---|
| `trust_gerrold` | +1 (he respects the consideration) |
| All other Vow integrities | unchanged |
| `memory_integrity[GER-007]` | 100 (untouched, still in his cloth on your counter) |
| `pickle_approval` | +1 |

Mission 2 ends. **In Mission 3 (Day 3)** Gerrold returns. The same choice tree reappears — *with one extra option earned through reflection.* (That option is Mission 3+ content.)

> 💡 **Defer is the most generous choice for yourself.** The cozy game does not require you to commit to a heavy decision on a heavy day. Sleep on it. Come back. Gerrold will too.

---

## 10. Memory Dream 2 — Five Variants

After the choice resolves and the Evening Ledger closes, **Memory Dream 2** plays. This is a ~70-second cinematic that depicts Margery's last week from inside the cleansed (or uncleansed) orb. **Which variant plays depends on your choice and your Cleanse outcome:**

| Variant | Trigger | What you see |
|---|---|---|
| **A — Erase, clean** | Erase + Perfect/Acceptable Cleanse | Margery's face is intact, smiling at the embroidered "M" on the handkerchief. The dying week itself is *absent.* Snow outside. Doris in the kitchen. A morning at the end with no death in it. *Bittersweet.* |
| **B — Cleanse, perfect** | Cleanse + Perfect | Margery's face is intact and clear. The week is present — small motions, the handkerchief, the smile at her own needlework — but the *shame* is gone. Gerrold's hands are not Gerrold's hands; they are just hands holding hers. *Quietly devastating.* |
| **C — Cleanse, acceptable / sloppy** | Cleanse + Acceptable | Like B, but the seventh morning is gentler — *blurred* slightly. You don't quite see the moment. You see the snow stopping. You see Doris through a window. *Tender.* |
| **D — Listen** | Listen path | The orb is still in Gerrold's hands at the mantel. The dream is *Gerrold's* dream of the room — but Margery is in it, sitting in the chair where you sat earlier, looking at the empty chair across the fire, **smiling at you** for sitting in her place. *Heart-stopping.* |
| **E — Defer** | Defer path | A single static image of Margery's empty chair with the cold tea in front of it. The handkerchief beside it. No motion. The cozy game's most restrained image. *A question mark in cinematic form.* |

> 💡 **Variant D (Listen) is the most affecting Memory Dream in Mission 1-2.** Most playtesters quote it verbatim in vertical-slice feedback.

---

## 11. The Evening Ledger (5 Variants)

A parchment scroll appears. You see:
- **Day 2** label
- A short prose passage summarizing what just happened (1 of 5 variants depending on your choice + outcome)
- The orb's final state (cleansed / partially cleansed / un-cleansed / Crossed)
- Your coin balance (unchanged in Mission 2 — Gerrold does not pay in coin)
- Save buttons

The 5 variants:

| Variant | Prose excerpt |
|---|---|
| **Erase clean** | *"The widower's long bit is gone. He may not yet know what he kept. The village will, in time."* |
| **Erase Crossed** | *"You went deeper than he asked. Margery is fainter tonight than she was this morning. The work is recoverable. It will take time."* |
| **Cleanse perfect/acceptable** | *"The widower keeps his wife. His blame is out of him. The village will see him on Sunday."* |
| **Listen** | *"You did not take what he brought. You sat with him while he found his own voice. He keeps the orb. He will sleep tonight."* |
| **Defer** | *"You asked for a day. He gave it. The orb is on your counter in her handkerchief. Tomorrow you will know."* |

Click **"Sleep — End Day 2"** to commit.

---

## 12. The 4 Mission 3+ Long-Term Effects

Mission 2's choice **reshapes Gerrold's life across the rest of the game.** Here's the preview:

| Your choice | Mission 3+ Gerrold | Notes |
|---|---|---|
| **Erase (clean)** | Gerrold returns to social life. Attends Long-Night Festival. Speaks of Margery by name. | 3–4 follow-up beats Mission 6–9. |
| **Erase (Crossed Core)** | Gerrold stays home. Margery's face is unclear in his mind. | **Mission 6 unlocks a sub-quest:** restore Margery's face by Severing other villagers' memories that contain her. A meaningful ~3-hour recovery arc. |
| **Cleanse (Perfect/Acceptable)** | Gerrold gentler than before. Visits Doris on Sundays. | 2 follow-up beats. Mission 8 offers a "complete the cleanse" beat to upgrade Acceptable → Perfect. |
| **Listen** | Gerrold returns to the Hollow in Mission 5 to try again, possibly more ready. The orb stays in his hands. | Vow 7 surges. The Cinder currency he gave you is spent in Mission 4+. |
| **Defer** | Mission 3 Day 3 reopens this same choice with one extra option. | See Mission 3 guide (future). |

> 💡 **None of these are "endings."** Mission 2 sets the relationship's tone. The relationship deepens or shifts across many later missions.

---

## 13. The Echo Web First Connection

Independent of which moral choice you make, you triggered **the Echo Web's first activation** the moment you picked up Gerrold's orb (Step 4).

The connection: **DOR-001 ↔ "Doris in kitchen" ↔ GER-007**.

Open the Codex (later mission unlocks the UI button; the data is recorded now) to see the line of warm amber light. This is the game's first **memory-thread**.

The Echo Web grows as you collect memories. By Mission 6 your codex will look like a constellation. By Mission 10 you'll be using it to *find* memories — *"This one connects to that one through the kitchen — show me everyone who sat there in 1972"* — and the game will surface a constellation of villagers who shared moments you've already collected.

The whole village is one woven thing.

---

## 14. The Handkerchief — Returning It

After the cleanse / listen / defer resolves, you can **walk back to Gerrold's cottage door** with the handkerchief in your inventory. Press **E** at the door: *"Hand back the handkerchief."*

> **Gerrold:** *"Aye. Thank you. Margery folded that for me. I'd been losing it for weeks. ... Take care of yourself, keeper."*

The handkerchief leaves your inventory permanently. Mission 2 outro plays.

> 💡 **Returning the handkerchief is the Mission 2 cozy coda.** Cost: 30 extra seconds. Benefit: a specific Gerrold line that does not play if you keep the cloth. **Strongly recommended.**

---

## 15. State That Carries Forward to Mission 3+

By the end of Mission 2, your `VillageState` will look something like:

| Dimension | After typical Mission 2 (chose Cleanse-Perfect) |
|---|---|
| `trust_gerrold` | +4 |
| `memory_integrity[GER-007]` | 95 |
| `public_standing` | +5 typical |
| `vow_1_consent` | 53 |
| `vow_3_whole` | **58** (very high — you Cleansed carefully) |
| `vow_5_honest_coin` | 53 |
| `vow_7_last_light` | 50 (unchanged unless Listen) |
| `pickle_approval` | 56 |
| `cinder` | 0 (only +3 if Listen) |
| **`first_moral_choice_made`** | **true** (locked forever) |
| `memories_in_inventory` | 2 (DOR-001, GER-007) |
| `predecessorTrailWarmth` | 8-10 (cumulative) |

The `first_moral_choice_made` flag is what the rest of the game checks before unlocking certain narrative beats. **It is your canonical moral debut.**

---

## 16. Common Mistakes (and Why They're Fine)

| "Mistake" | What actually happens |
|---|---|
| I crossed the core during Cleanse | Margery's face faded slightly. ⚠️ Real consequence. **But:** the memory still works. Gerrold's reaction is forgiving. Mission 6 unlocks a full restoration arc. |
| I picked Erase without thinking | Gerrold keeps his wife but not the dying week. He's *quieter* in M3+ but okay. Vow 3 dipped. Pickle disapproves. The narrative absorbs it. |
| I didn't bring tea | Lavender's extra dialogue line and Valerian's mini-game tilt are missed, but the cottage scene plays normally. No penalty. |
| I deferred to Day 3 | Mission 2 ends "early" — you got Day 2 prep without the commitment. Mission 3 re-opens the choice. **+1 trust_gerrold** for the consideration. |
| I sat in Margery's chair without realizing | Gerrold notices. He doesn't say anything overt. But trust_gerrold ticks up slightly and his post-choice line is warmer. |
| I refused all four options and just walked out | The game won't let you — the choice screen is modal. Defer (Option D) is the "I don't want to pick today" valve. |

**Mission 2 has no game-over.** Even Crossed-Core is recoverable later. The cozy contract is absolute.

---

## 17. The Pao Five-Test Reassurance (for Anxious Players)

The Cleanse mini-game passes the Pao Discipline:

| Test | Cleanse passes? |
|---|---|
| No mini-game locks progression on failure | ✓ Crossed Core is narratively absorbed, never blocks Mission 3 |
| No reaction time below 350ms | ✓ Cleanse has no real-time pressure — only your own pace |
| Playable one-handed | ✓ Mouse or thumbstick only |
| Audio is optional | ✓ Visual core-warning glows even if audio is off |
| Tutorial-in-fiction (no UI overlay) | ✓ Marin's note + Pickle's optional hint at 45s |

If you struggle for 45 seconds without sealing a crack, **Pickle offers one hint**:
> *"Trace the line of the crack itself. Not the orb around it."*

That's the only hint. Auto-Complete is one click away if it's not for you.

---

## 18. Pickle's Mission 2 Library (4 new lines)

Pickle gets **4 new voiced lines** in Mission 2:

1. **At your door, before opening it to Gerrold** — *"There is a man at your door. He has been there for twenty minutes..."*
2. **After the Echo Web activates** — *"Ah. The Web has decided to speak today..."*
3. **During tea brewing (if you brewed)** — *"Tea for a man who has not asked for tea is the rarest kindness..."*
4. **Pre-choice commentary** — one of 4 options-specific lines (only if `pickle_approval ≥ 50`)

She also has a contextual line if you sit in Margery's chair (`pickle_approval ≥ 50`):
> *"You sat in the dead wife's chair. He is too polite to tell you. But he is grateful. I would not have done it. But you are not a cat."*

**After Mission 2, Pickle's full library opens.** She'll grow into the cozy game's wry conscience over the next ~50 missions.

---

## 19. Glossary (Mission 2 Terms)

| Term | Meaning |
|---|---|
| **Cleanse** | The mini-game for removing cracks (and optionally one palette component like Shame) from a damaged memory |
| **Listen** | The Vow-7 path: sitting with a villager while they speak the memory aloud, without taking it |
| **Defer** | Asking the villager to return another day; valid path, no penalty |
| **Crossed Core** | An adverse Cleanse outcome where the cursor touched the orb's central memory; recoverable in M6+ |
| **Echo Web** | The connection graph between memories; threads form as you collect memories that share echoes |
| **Tariff** | The state-change package on a moral choice (trust, vows, integrity, etc.) |
| **Vow 7 — Last Light** | The cozy game's "presence over solution" virtue; rewarded by the Listen path |
| **Cinder** | A currency earned only through Vow-7 acts; spent at the Mission 4+ Confession Booth |
| **GER-007** | Gerrold's memory ID; full title: *"The Last Week"* |
| **Margery** | Gerrold's late wife; never appears on screen; built entirely through dialogue + the handkerchief |
| **The Handkerchief** | Margery's needlework; returned to Gerrold as the Mission 2 cozy coda |

---

## 20. Mission 2 Summary (One-Page Quick Reference)

```
GOAL: Help Gerrold with his late wife's memory. Choose how.

PATH (typical Cleanse-Perfect run):
  Wake in Hollow → Gerrold knocks → "Tell me about Margery first" →
  pick up orb (Echo Web activates) → walk to cottage with Gerrold →
  (optional: garden → harvest lavender → brew tea → carry) →
  arrive at cottage → sit in Gerrold's left chair → offer tea →
  Choice: Cleanse with care → walk back alone → Cleanse mini-game (4 cracks,
  avoid core) → return to Gerrold with handkerchief → "hand back the
  handkerchief" → Evening Ledger → Memory Dream 2 Variant B →
  Continue/Mission 3.

DURATION: 35-55 real-time minutes.

NEW MECHANICS: Echo Web first connection, Tea brewing, Moral Choice screen,
                Cleanse mini-game, Pickle pre-choice commentary, Cinder currency.

CONSEQUENCE: First moral choice locked. trust_gerrold, vow_3, vow_7, and
              pickle_approval move. Mission 3+ reads them.

SECRET BREADCRUMBS:
  • Asking about Margery before the orb
  • Sitting in Margery's (right) chair
  • Brewing Lavender (warmer convo) vs Valerian (deeper risk)
  • Returning the handkerchief
  • Doris-Margery best-friend echo in the Codex

PATH PICKS QUICK-REF:
  • For warmest possible outcome → Cleanse + Perfect
  • For most thematic outcome → Listen
  • For "I want him healed even at cost" → Erase (Vow 3 dips)
  • For "I need to think" → Defer (Mission 3 reopens it)

NEXT: Memory Dream 2 → Mission 3 (future) or Main Menu
```

---

## 21. A Final Word

Mission 2 is where Hearthbound Hollow stops being a Stardew clone and starts being itself.

Doris's polish was warm. Gerrold's choice is *weighted.* The orb has cracks. Margery is in the kitchen on the morning the snow stopped. The widower does not cry. The handkerchief is white.

Whichever path you choose, the game will remember. That is the promise the rest of the game keeps.

— *Player guide v1.0 — sourced from Depth Bible Mission_1_2_Focus 02, 03, 04, 05, 06, 07 + runtime Mission02Director.cs + Yarn script Gerrold_M2.yarn*

> Pair this guide with `MISSION_01_GAMEPLAY_GUIDE.md`. After Day 2 the game moves into Mission 3+ scope (currently architectural-only).
