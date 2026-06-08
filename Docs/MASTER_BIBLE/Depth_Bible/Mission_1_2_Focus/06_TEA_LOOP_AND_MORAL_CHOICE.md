# Mission 1-2 · Focus 06 — **The Tea Loop + The Moral Choice**
### Mission 2's chain: Herb harvest → Tea brew → Conversation modifier → The 4-option moral choice
### Owners: Saija Korhonen (Progression) + Esme Cordray (Choice)

> *Mission 2's middle act. After Gerrold's first visit but before the cottage scene, the player exits the Hollow into the back garden, harvests an herb, returns to the kitchen, brews a tea, and chooses whether to offer it to Gerrold. The tea conditions the conversation. The conversation leads to the choice. The choice leads to the dream. This document specifies that ~12-minute chain end-to-end.*

---

## 1.0 The Mission 2 Mid-Act Flow

```
   Gerrold offers the orb at the Hollow counter.
                  │
                  ▼
   Player either:
   (A) Walks to the garden alone to brew a tea.
   (B) Skips the garden and goes straight to the cottage.
   (C) Defers and ends Mission 2.
                  │
                  ▼
   [If A]  Herb garden — pick Lavender (Calm) or Valerian (Sleep)
                  │
                  ▼
   Return to the workbench room — Brew tea at the kettle
                  │
                  ▼
   Pour into a cup — Carry to the cottage in a small wooden box
                  │
                  ▼
   Arrive at the cottage — Offer the tea to Gerrold (one button) or set it down
                  │
                  ▼
   [Cottage interior dialogue with tea modifier active]
                  │
                  ▼
   The 4-option moral choice screen
                  │
                  ▼
   The Cleanse mini-game (or Listen scene or Defer)
                  │
                  ▼
   Memory Dream 2 plays.
```

This document covers each step in granular detail.

---

## 2.0 Step 1 — The Herb Garden Choice

(Scene specifications in Focus 03 § 4.) When the player exits to the garden, the camera transitions to the brighter outdoor lighting (no cinematic; live transition through the open door).

### 2.1 The two herbs available in Mission 2

| Herb | ScriptableObject ID | Effect ID | Mechanical effect | Tonal connotation |
|---|---|---|---|---|
| **Lavender** | `MemoryHerb_Lavender` | `Effect.Calm` | When brewed and offered, the **Calm modifier** unlocks one extra Listen dialogue line from Gerrold (he opens up more). | The cozy game's default kind herb. |
| **Valerian** | `MemoryHerb_Valerian` | `Effect.Sleep` | When brewed and offered, the **Sleep modifier** tilts the Cleanse mini-game's outcome zones — *core region becomes slightly larger* (Gerrold relaxes; the memory becomes less defended). This is the *aggressive* option. | Tonally: a sleeping-pill metaphor. Pickle disapproves. |

Beds 3 and 4 are *empty.* The player cannot plant in M1-2 (planting is M3+ content; the garden is read-only).

### 2.2 The harvest interaction

| Step | Player input | Effect |
|---|---|---|
| 1. Approach a plant | Walk within 1.2m of any growing herb | UI prompt appears: *"Harvest [Lavender / Valerian] (E)"* |
| 2. Press the harvest button | Single button press | ACS upper-body "pluck" animation (0.8s) plays |
| 3. Herb is added to inventory | (automatic) | Inventory shows: 1× Lavender or 1× Valerian |
| 4. Plant disappears from bed | (automatic) | The plot becomes empty (regrowth in M3+) |

**Asset:** Harvest Garden (S-2) plants + custom `MemoryHerb` ScriptableObject layer.

### 2.3 Allowed harvests in Mission 2

- The player can harvest **up to 2 herbs total** in Mission 2 (one of each, or two of one).
- Per cup of tea: **1 herb required.** So 2 herbs = up to 2 cups, but the player only ever offers 1 cup in M2.
- **Default state at start of M2:** 3 lavender plants, 3 valerian plants. The player walks past with options.

### 2.4 Pickle's garden line (rare)

If `pickle_approval >= 75` (very rare in M1-2 — would require the player to have done multiple kind acts in M1, which is technically possible but not the default path), Pickle has followed the player into the garden. Her one line:

> *"You picked the wrong one. Or the right one. I don't know herbs. I am a cat. Carry on."*

Else, Pickle stays at the Hollow. No garden commentary.

### 2.5 The "skip the garden" path

The player may **walk straight from the Hollow to the cottage without harvesting.** This is a valid path. The Mission 2 dialogue at the cottage simply uses the **No-Tea Default** branch (see § 4 below). No penalty.

The cozy contract: kindness without coercion. The player who skips the garden is not punished; they just enter the conversation without an extra option.

---

## 3.0 Step 2 — The Tea Brew

### 3.1 The kettle interaction

The kettle sits on the wood stove in the workbench room. **Pre-warmed by Marin's note technology** (Focus 03 § 3.4) — the kettle is *always* near-boiling.

| Step | Player input | Effect |
|---|---|---|
| 1. Approach the stove with herb in inventory | Walk within 1m of the stove | UI prompt: *"Brew tea (E)"* |
| 2. Press button | Single press | Brewing UI overlay appears |
| 3. Pour the herb in | Drag-and-drop the herb icon into the kettle in the UI | (Bamao parchment-frame UI) |
| 4. Wait 90 seconds in-game (~12 real-time seconds) | Time passes via a *gentle* visual loop — VoluSmokeFX steam rises from the spout | Pickle's eyes track the steam if she's present |
| 5. Pour into a cup | Drag the kettle icon to the cup icon | One soft "pour" foley + a gentle composer chime |
| 6. Box up the cup | Automatic — the cup is placed in a small wooden box (a tea-carrier; ~30 polys, custom mesh) | Inventory shows: 1× Tea (Lavender) or 1× Tea (Valerian) |

**Asset:** Harvest Garden kettle prefab + Bamao UI + custom tea-carrier mesh + Game UI & Puzzle SFX Pack foley.

### 3.2 The pour cue (a small audio moment)

The pour is a **3-second composer-mixed micro-cue**: a kettle pour sound + a single sustained note that resolves into a soft major chord (lavender = C major; valerian = D minor). The cozy player will register this as warmth or unease without consciously identifying why.

### 3.3 The brewing wait — 12 real-time seconds of cozy

During the 90-in-game-second wait, the screen does *not* fade or skip. The player stands in the workbench room. The composer's main theme plays softly. The kettle whistles gently. Pickle, if visible, yawns once.

This is **the cozy game's intentional pacing.** 12 real-time seconds of *nothing happening* is not dead time. It is **the rhythm.** Many cozy players will report this as a favorite moment in playtest feedback.

### 3.4 The Pickle commentary on brewing

If Pickle is in the workbench room (default — she's almost always there in M2 because the player has been in the workbench room for the orb inspection), she speaks **one line** during the brew:

> *(at the 6-second mark of the wait)* *"Tea for a man who has not asked for tea is the rarest kindness. Try not to overthink it. The man is also a cat about this. We will accept."*

This is **Pickle's 3rd line of the game** (after her M1 polish line and her M2 first-visit reaction to Gerrold). Slim. Memorable.

---

## 4.0 Step 3 — The Tea's Effect on the Conversation

(Cross-referenced from Focus 02 § 7 — Gerrold's Yarn dialogue.)

When the player arrives at the cottage, **whether they carry tea, and which tea, conditions Gerrold's dialogue.** Three branches:

### 4.1 No-Tea Default branch

The player has no tea. Gerrold is reserved but cordial. He speaks the standard Mission 2 dialogue from Focus 02 § 7.

| Effect on the choice screen |
|---|
| Standard 4 options. No bonus dialogue. |

### 4.2 Lavender (Calm) branch

The player offers Gerrold a cup of lavender tea. He accepts it (one button press). He takes one sip.

He then speaks **one extra dialogue line** that the Default branch does not have, which deepens the conversation:

> *(after taking a sip)* *"Margery and I had this tea on our last winter together. We had it every evening. She said it tasted like the inside of a folded sweater. I knew what she meant. I still do."*

This line:
- Reveals Margery's voice (her metaphor for the tea).
- Adds an emotional bond to the cleansing decision.
- Increases `trust_gerrold` by +2 immediately.

**Mechanical effect on the choice:** the **Listen** option (option C in the choice tree) is presented *first* in the choice screen ordering. Pickle's commentary on the choice softens. *Lavender nudges the cozy player toward gentleness.*

### 4.3 Valerian (Sleep) branch

The player offers Gerrold valerian tea. He accepts. He drinks more readily.

His extra dialogue:

> *(after taking a sip)* *"Aye. This is the one she used to make me when I could not sleep. She would put a sprig of it on the pillow when I — when I could not stop thinking. I have not had this in eleven months. ... Thank you. You are very kind."*

This line:
- Also reveals Margery's voice but in a gentler register.
- Specifically primes Gerrold for *the heavier choices* — he is now slightly less defended. The Cleanse mini-game's core region is mechanically larger (per § 2.1).
- Increases `trust_gerrold` by +2.

**Mechanical effect on the choice:** the **Erase** option (option A) is no longer hidden behind a confirmation; it presents at the same level as the others. The Cleanse mini-game's outcomes are slightly more forgiving (in difficulty) but the *Crossed-Core threshold is closer* (in narrative risk).

Pickle's commentary on this branch: *"You gave him the sleeping one. He drank it without thinking. I am not — I am not displeased. I am only observing."* (A noticeable Pickle hesitation — she does not commit to approval.)

### 4.4 Why this matters

The tea is a **soft modifier** that gives a player a meaningful *texture* to their approach without dictating their final decision. The cozy player who offers lavender is more likely to choose Listen; the cozy player who offers valerian is more likely to choose Cleanse. But **either tea can lead to any final choice.** The system honors the player's agency at the choice screen.

This is the **Korhonen/Cordray collaboration** — progression mechanics (the tea) hook into the choice mechanics (the dialogue tree) at a single small point, and the rest is the player's free will.

---

## 5.0 Step 4 — The Moral Choice Screen

The choice screen UI (Focus 02 § 5):

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

### 5.1 UI architecture

| Element | Source |
|---|---|
| Background frame | Bamao Fantasy GUI (S-4) parchment scroll |
| Option text | Crimson Pro / TextMeshPro |
| Hover state | Subtle warm-amber glow (Lumen) on the highlighted option |
| Confirmation | One button to select; one second confirmation pause; one button to confirm. *Cancelable* during the confirmation pause. |
| Backdrop dimming | Cottage interior slightly dimmed to 65% brightness during the choice — the dream is in the player's head |
| Pickle | Visible at the cottage window during the screen if she followed (very rare) |
| **No timer** | Per Codex 13's discipline — the choice has no time pressure |

### 5.2 The confirmation flow

1. Player highlights an option.
2. Player presses confirm.
3. **A 1.5-second pause** happens. The option's preview text expands slightly. Pickle's "are you sure" line plays (only for the Erase option — see § 5.3).
4. Player can press a second time to confirm, or press cancel to return.
5. After confirm, the choice is locked. The Yarn dialogue branches.

This 2-button confirm flow is **only used for the heaviest option** (Erase). The other three options confirm with one button. *The friction increases with the weight of the choice.*

### 5.3 The Pickle pre-choice commentary

Pickle has **one line per option** if she's present (which she may or may not be — depends on `pickle_approval`):

| Option | Pickle's pre-confirm line |
|---|---|
| A (Erase) | *"You are about to erase a week of a life. That is — that is a large thing. I am not your conscience. I am only telling you what the verb is."* |
| B (Cleanse) | *"This is the verb the Keeper before us liked best. I think — I think she would be pleased. Or angry. She had moods."* |
| C (Listen) | *"This is the older craft. The one nobody pays for. The good keepers all do it eventually."* |
| D (Defer) | *"A day is a fair thing to ask for. I have asked for many."* |

If Pickle is NOT present (Pickle Approval < 50), **no commentary plays.** The player makes the choice in pure silence with Gerrold. This is the more common path in M1-2 — most players will not have built Pickle Approval high enough by Mission 2.

The cozy game's design generosity: **Pickle's commentary is not a tutorial.** It is a reward for the player who has earned her.

---

## 6.0 Step 5 — The Choice Resolves

(Detailed in Focus 02 § 7 and Focus 04 — the Cleanse mini-game.)

| Choice | What happens immediately |
|---|---|
| A — Erase | Player returns to the Hollow alone. Cleanse mini-game runs at aggressive-difficulty profile (forgiving core size, less forgiving on outcome). Memory Dream 2 plays variant A or C (depending on mini-game outcome). |
| B — Cleanse | Player returns to the Hollow alone. Cleanse mini-game runs at careful-difficulty profile. Memory Dream 2 plays variant A, B, or C. |
| C — Listen | **No mini-game.** Player sits in Gerrold's chair while he speaks. Cutscene Engine plays a ~3-minute scene of Gerrold telling Margery's last week aloud. Memory Dream 2 plays variant D. |
| D — Defer | Player returns to the Hollow alone. Gerrold takes the orb home. Mission 2 ends with the choice still open. Memory Dream 2 plays variant E. |

---

## 7.0 The Listen Scene (Choice C) — Special Spec

The Listen path is the **only** choice path that does *not* go through the Cleanse mini-game. Instead, it plays a **3-minute Cutscene Engine sequence** of Gerrold telling Margery's memory aloud while the player sits across from him in his chair.

### 7.1 Why it deserves its own scene

The Listen path is the cozy game's **highest-Vow-7 reward.** It is the proof that the game considers *being present* mechanically equivalent to *doing the work.* The Cleanse mini-game is the work; the Listen scene is the witness.

If we shortchange Listen, the player learns that the option was a lesser choice. We must not.

### 7.2 The 3-minute Listen Cutscene

```
TIMELINE: Mission2_Listen_Scene
Total duration: 180.0 seconds

  0:00–0:15  Player sits down in Gerrold's chair (the one on the left).
             Gerrold takes Margery's chair (the one on the right).
             *Pickle, if present, sits on the windowsill.*
             *The fire continues to crackle.*
             *Gerrold holds the orb in both hands, in his lap.*

  0:15–0:45  Gerrold begins. He describes the room (the bedside table, the
             lamp, the snow outside). His voice is steady. He has rehearsed
             these sentences alone for eleven months.

  0:45–1:30  Gerrold tells the middle of the week — the small motions, the
             handkerchief, the way Margery looked at the embroidery.
             His voice begins to shift; longer pauses; slower delivery.
             Camera holds tight on his hands and the orb.

  1:30–2:15  Gerrold tells the seventh morning. *He does not cry.*
             *He gets through it.* "The snow had stopped. Doris was in the kitchen.
             I made tea. I brought it in. She — was already gone."

  2:15–2:45  Gerrold falls silent. Holds the orb. Looks at the player —
             *for the first time in eleven months, looks someone directly in the eyes
             while speaking about her.*
             "Thank you. I did not know it would be — easier — when said aloud.
             It is still heavy. It is no longer alone."

  2:45–3:00  Gerrold rises slowly. Walks to the mantel. Sets the orb beside the
             wedding photograph. *He keeps the orb.*
             Returns to his chair. *The fire crackles.*
             Player can stand or remain seated. No prompt. *Just held space.*
```

### 7.3 What this requires

- **Audio:** ~3 minutes of Gerrold voice acting (the most demanding single scene in M1-2 for the VO actor).
- **Animation:** Gerrold sitting still, hands on orb; eye direction shifts; one rise to mantel, one return to chair. No facial-mocap; subtle Eyes Animator work + ACS upper-body hand-tracking.
- **Camera:** Three Cinemachine shots (wide, hands close, face close) blended slowly.
- **Composer:** A single sustained cue (M-MD02-D-prelude) playing throughout at very low volume.

**Cost:** ~$22,000 (mostly VO + composer).

This is **the most expensive single scene in Mission 1-2.** It is also the one most likely to be quoted in vertical-slice playtester reviews.

---

## 8.0 Mission 2 Outro — Return to the Hollow

After the choice resolves (whichever path):

1. Player walks back to the Hollow alone (or with Gerrold's blessing if Listen path).
2. Day fades to evening.
3. Player closes the Evening Ledger.
4. Memory Dream 2 plays.
5. Mission 2 ends with a black fade + Cutscene Engine outro (~30 sec).
6. Save flow runs (handled in Focus 07).

The outro Cutscene Engine sequence:

```
TIMELINE: Mission02_Outro
Total duration: 30.0 seconds

  0:00–0:08  Slow pan through the Hollow.
             *The chairs sit empty.*
             *Pickle sleeps on the windowsill.*
             *The fire in the stove is low.*

  0:08–0:18  Camera finds the workbench.
             *Gerrold's handkerchief, folded, on the bench beside the orb cradle.*
             *(If the player returned the handkerchief, it is at Gerrold's cottage —
              the workbench is empty. Different framing.)*

  0:18–0:25  Camera finds the shelves.
             *DOR-001 (Doris's First Loaves) is on the shelf — softly glowing.*
             *If Cleanse path: GER-007 (cleansed) is beside it.*
             *If Erase path: GER-007 (clouded, partial) is beside it.*
             *If Listen path: GER-007 is NOT on the shelf — it is in Gerrold's cottage.*
             *If Defer path: GER-007 in its cloth on the counter, untouched.*

  0:25–0:30  Camera pulls back. Text overlay (Bamao parchment frame):
             *"Day 2 ended. Tomorrow, the village will know."*
             Fade to black.

  Composer cue: Hearthbound main theme, final 20 seconds (full statement).
```

The outro is **identical in length for all paths** but **its visual content differs** based on the player's choices. This is the cozy game's first **branching outro.** Cost: ~$3,400.

---

## 9.0 Production Cost Summary — the Tea + Choice Chain

| Item | Hours / Cost |
|---|---|
| Herb harvest interactions (2 plants) | 6 hours (engineering) |
| Tea brewing UI + 90-sec wait | 12 hours (engineering + UI) |
| Tea-carrier wooden box prop | $400 (modeling) |
| Tea-modifier dialogue branches (Yarn lines) | 8 hours (writing) — already in Focus 02's 270 lines |
| Pickle commentary (4 new lines) | 2 hours (writing) + $600 (VO) |
| Choice screen UI (Bamao integration) | 16 hours (UI) |
| Confirmation flow + Pickle pre-choice commentary | 8 hours (UI + writing) |
| Listen scene (full 3-minute cutscene) | $22,000 (VO + composer + animation) |
| Cleanse mini-game variant difficulty profiles | 12 hours (engineering) — already in Focus 04 |
| Memory Dream 2 variant switching | 6 hours (engineering) — already in Focus 05 |
| Mission 2 outro Cutscene Engine sequence (5 variants) | $3,400 |
| QA + playtest iteration | 32 hours |
| **Total Mission 2 mid-act + outro cost** | **~$29,400 + ~102 engineering hours** |

---

## 10.0 The KPI Targets for the Tea + Choice Chain

| KPI | Target | Why |
|---|---|---|
| Players who harvest at least 1 herb | 60–75% | Most players will try the garden because it's *visible* |
| Players who offer tea to Gerrold | 50–65% | The harvesters mostly carry through |
| Tea split — Lavender | 65% | Default cozy choice |
| Tea split — Valerian | 35% | The braver / curious choice |
| Choice distribution — Erase | 18% | The "I want him to be okay" path |
| Choice distribution — Cleanse | 42% | The canonical "careful Keeper" path; the largest |
| Choice distribution — Listen | 28% | The cozy-mature path; rewarded by Vow 7 and the Pickle line |
| Choice distribution — Defer | 12% | A meaningful minority; sets up Mission 3 |
| "First moral choice felt heavy" mentions in playtest | >55% | The Codex 06 § 9 metric — *did the cozy player feel something?* |
| "I want to know what happens to Gerrold" mentions | >45% | The retention check — *did the game make them care?* |

---

## 11.0 The Final Discipline — *do not signpost*

The Mission 2 mid-act chain is full of soft modifiers (the tea, the chair-choice, the handkerchief, Pickle's presence). **None of these are explicitly explained to the player.** The cozy game's discipline (Codex 08 § 9 — Cordray Principle):

> *The player infers. They are never told.*

The player who offers Lavender does not know that Lavender unlocks an extra line of dialogue. They simply find themselves in a slightly warmer conversation. The player who sits in Margery's chair does not know they have done something poignant. They simply notice that Gerrold's posture shifts slightly. The player who keeps Pickle approved does not know they have unlocked her pre-choice commentary. They are simply accompanied.

**This is the cozy game's most important non-design design.** What the game is doing is invisible. *That is exactly the point.*

— *Saija Korhonen + Esme Cordray*
*Mission 1-2 Focus 06 · v1.0*

> Next: `07_DIALOGUE_AND_LEDGER_UI.md` — the Yarn Spinner architecture and the Evening Ledger save flow.
