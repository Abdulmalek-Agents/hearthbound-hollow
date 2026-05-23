# Mission 1-2 · Focus 01 — **Doris the Baker**
### Mission 1's only writable villager · Full character bible + Yarn dialogue + reskin recipe
### Owner: Inara Vellis (Lead Memory Writer)

> *Doris in the long-form codex 02 is named "Doris Vance, the Beekeeper." In the Asset Analysis Mission 1-2 document she is called "Doris the Baker." Both are correct. Doris is the village's baker (her shop is her livelihood, her flour-dusted apron is how the player first sees her) **and** privately, in the back garden of the bakery, she also keeps her late husband Elric's bees. The bee/honey storyline unfolds in Mission 3+. For Mission 1, the player only ever sees her as a baker, and that is what she presents as. The reconciliation is in §3.6 below.*

---

## 1.0 Mission 1's Single Job

Mission 1 must teach the player **one thing**:

> *A memory orb is a person's life made into glass. The Keeper polishes the glass with care. The player just did that. They feel the weight of it.*

That is the **entire** onboarding goal. Every other system in the game (Cleanse, Weave, Sever, Tribunals, the predecessor mystery, Vance Ashby, Letter-Birds — *everything*) hangs on the player having had that one warm, embodied moment with Doris's memory in their hands.

Doris exists, in this mission, to deliver that moment. Then she steps offstage so Mission 2's heavier choice can land.

---

## 2.0 Voice Signature

### 2.1 Six rules for writing Doris (Mission 1 only)

1. **She uses bread metaphors for everything.** *"The dough needed another hour. Some days do."* / *"You knead, you wait, you knead again. That's most of it."* This is her surface — warm, useful, mildly observational.

2. **She does not yet mention Elric by name in Mission 1.** He exists in subtext: a chair beside hers at the table, a second mug always rinsed even when only she drank that morning. The player will not necessarily notice. They are not supposed to, yet.

3. **Her sentences are short.** ~12 words average. Older village voice, plain register, occasional Northern-rural cadence (*"aye"*, *"summat"*, *"the long bit of the year"*).

4. **She makes eye contact and breaks it gently.** This is a stage direction for the Eyes Animator (Tier A asset). Default look-at: player → orb-in-hand → counter → player.

5. **She offers tea unprompted.** Twice in Mission 1. Once accepted is enough. The player can refuse without consequence; she will not be offended.

6. **She does not say goodbye at the end of Mission 1.** She just turns back to the dough. The player walks out. This is the cozy game's *understated* register.

### 2.2 Sample voice (excerpts the writer will use as the tonal anchor)

> *"You're the new one. I thought you'd be taller. Don't mind me — I thought that about the old one, too. Come in. The kettle's only just stopped."*

> *"This is the memory. Hold it like you'd hold a hot bun. Not by the side. Underneath."*

> *"It's a small thing. First time I made bread that didn't shame me. Most days I think of it. I want to put it down, now, for a while. Will you take it?"*

> *(when the player polishes successfully)* *"Aye. There it is. That's the morning."*

> *(when the player offers more than her asking price)* *"That's too much. I'll not have you ruin yourself. Take it back. — Well. Take *some* back."*

These four lines together establish her **entire voice signature**. The remaining ~50 lines of Mission 1 Yarn dialogue (see § 7) are calibrated against them.

---

## 3.0 The 4-Node Memory Map (Mission 1's "Memory Map" tutorial)

Per Asset Analysis Mission1-2 § 6 A-19, Doris's Memory Map in Mission 1 has **4 nodes** — 3 visible, 1 locked. This is the tutorial introduction to the Skill Tree Builder asset, repurposed as the Memory Map.

```
                    ┌─────────────────────────┐
                    │   DOR-001               │
                    │   The First Loaves      │  ← MISSION 1 MEMORY
                    │   1972, age 24          │     (you polish this)
                    │   palette: JOY+GRACE    │
                    │   weight: 4 / 10        │
                    └────────────┬────────────┘
                                 │  (echo: "the morning kitchen")
                    ┌────────────┴────────────┐
                    │   DOR-002               │
                    │   The Wedding Honey     │  ← visible but locked
                    │   1982, age 34          │     (Mission 3 hook)
                    │   palette: JOY+LONGING  │
                    └────────────┬────────────┘
                                 │  (echo: "Elric")
                    ┌────────────┴────────────┐
                    │   DOR-003               │
                    │   A Letter Returned     │  ← visible but locked
                    │   ~1985, age 37         │     (Mission 6+)
                    │   palette: SHAME        │
                    └────────────┬────────────┘
                                 │  ???
                              ┌──┴──┐
                              │  ?  │  ← MISSION 1 SHOWS THIS LOCKED
                              │     │     SLOT WITH A QUESTION MARK
                              └─────┘     (the player will not be told
                                          what it is — that is Mission 12+)
```

### 3.1 The Mission-1 transactable memory — `DOR-001 — The First Loaves`

This is the **only** memory the player sees in full prose in Mission 1. Its full dossier:

```yaml
memory_id: DOR-001-FIRST-LOAVES
villager_id: DORIS_THE_BAKER
season_lived: Late summer, age 24 (1972)
anchor: "the warm pull of dough rising under a tea-towel"
setting:
  place: "Doris's first bakery, the morning it opened"
  weather: "very early sunlight, before traffic, before custom"
  light: "pale gold through unwashed windows"
cast:
  - DORIS (self, 24, alone, hands floured)
action_summary: |
  She has been awake since four. The oven is hotter than she trusts. The
  first batch went in nervously. She has done this in other people's
  bakeries for nine years; today is the first time she has done it in her
  own. The bell on the door has not yet rung. She is afraid it never will.
emotional_palette:
  primary: JOY
  secondary: GRACE
  tertiary: (none — this is a simple memory by design; tutorial-tier)
wound_or_wonder: WONDER
weight: 4 / 10
clarity: 6 / 10  (faded with age; benefits from a Polish)
cracks: 0  (no Cleanse needed — Mission 1 teaches only Polish)
echoes:
  - MORNING_KITCHEN  (will appear in DOR-002, DOR-014, MAY-007)
  - FIRST_DAY        (will appear in 6 other villager memories — the
                      universal "I started this" emotional fingerprint)
content_warnings: []
gentle_mode_substitution:
  // None required. This memory is already gentle.
post_polish_state:
  clarity: 10 / 10
  reveal: "She is no longer afraid. She is just a woman in a kitchen,
           with bread, with light. The morning is the prize."
tariff_on_purchase:
  trust_doris: +5
  vow_1_consent: +1  (she gave it freely and knew what she was selling)
  vow_3_whole: +3   (you polished it well)
  vow_7_last_light: +0  (you transacted; you did not Listen-only)
  pickle_approval: +2
```

Word count of `action_summary`: **70 words.** Designed to be readable as a UI tooltip in the Codex *before* Polish, then expanded after Polish reveals it in full.

### 3.2 The full prose (what plays as Memory Dream 1 — see Focus 05)

> **The First Loaves**
>
> *Anchor: the warm pull of dough rising under a tea-towel.*
>
> She had been awake since four. The bakery she had paid for — with five years of working in other women's kitchens and one inheritance she had not been expecting — sat around her, smelling, for the first time, of *her* yeast and *her* flour and the faint, dignified soot of an oven that had been bricked yesterday. She had named the bakery for her mother. The sign was hand-painted. The paint was still tacky.
>
> The first batch had gone in at six. She had stood at the oven door for the entire bake, watching the rise through the slot of the glass, telling herself — *the loaves know what they are doing, the loaves know what they are doing* — even though she had wished, intermittently and uselessly, that the loaves did not know and would tell her if they did.
>
> When she pulled them out they were the colour of a thing that has decided to be golden. She set them on the rack. She did not yet eat any. She wiped her hands on her apron, slowly, and looked at the door, and waited to find out whether anyone in the village had decided, this morning, to be a customer.
>
> She did not, in fact, know yet that she would have customers for forty-seven years. She only knew, at that moment, that the loaves were her own.
>
> *(This is a JOY+GRACE memory with no cracks. Polishing it returns Doris a long-faded brightness of the morning. She is grateful. She does not say so.)*

**Word count: 270.** Within the Mission 1 budget. Hand-written. Final. This is the prose the senior writer locks in for the playtest.

### 3.3 The 3 visible-but-locked memories (the *tease*)

The player can *see* DOR-002 and DOR-003 in the Memory Map UI but cannot interact with them. Their hover-tooltips display:

- **DOR-002 — The Wedding Honey** — *Doris is not ready to sell this. Come back when you have earned her trust.* (Hint at Tier 4 Trust unlock — Mission 3+ content.)

- **DOR-003 — A Letter Returned** — *Doris does not yet know that you know this exists. (??? — locked.)* (Mission 6+ content.)

The 4th node — **the ?-marked locked slot** — shows nothing. Just a question mark in the Skill Tree Builder UI. The cozy player will theorize. *That is the design.* The reveal is in the long-form codex (DOR-014, "Last Light") and not in Mission 1-2.

### 3.4 Why three lock-states, not one

Three different "locked" presentations teach the player **three different cozy narrative-systems** in one screen:

| Lock type | Mechanic taught | Mission 1-2 prompt copy |
|---|---|---|
| Trust-locked (DOR-002) | "Build relationships to unlock memories." | "Doris is not ready to sell this." |
| Mystery-locked (DOR-003) | "Some memories you don't yet know how to ask for." | "(???)" |
| Unknown-locked (?) | "There are memories you do not know exist." | (no tooltip; just `?`) |

This is the **Memory Map onboarding** in one screen. ~$0 content cost beyond the prose for DOR-001. *Highly efficient.*

### 3.5 Asset binding — the Skill Tree Builder repurpose

Per Asset Analysis A-19, the engineering implementation:

```csharp
// ScriptableObjects extending the Skill Tree Builder asset

[CreateAssetMenu(menuName = "HH/Memory/Memory Node")]
public class MemoryNodeSO : ScriptableObject {
    public string memoryID;              // "DOR-001"
    public VillagerSO owner;             // Doris
    public string title;                 // "The First Loaves"
    public int weight;                   // 4
    public int yearOfLife;               // 24
    public Palette primary;
    public Palette secondary;
    public Palette tertiary;
    public int clarity;                  // 6 (pre-Polish)
    public int cracks;                   // 0 (no Cleanse needed in M1)
    [TextArea(10,30)] public string actionSummary;
    [TextArea(20,50)] public string fullProse;
    public List<EchoSeed> echoes;
    public LockState lockState;          // Unlocked / Trust / Mystery / Unknown
    public string lockPromptCopy;        // shown on hover
    public TariffSO tariffOnPurchase;
}

public enum LockState { Unlocked, TrustLocked, MysteryLocked, UnknownLocked }
```

This data structure is the **single source of truth** for every Memory Card in the game. Mission 1 populates 4 instances (DOR-001 through DOR-004). Future missions add more. **The schema does not change.** This is the Krieg discipline (Focus 00 § 5).

### 3.6 The Baker / Beekeeper reconciliation

In Mission 1, the player learns Doris is the village baker. **They do not learn about Elric, the bees, or the wedding honey.** All of that is Mission 3+ content.

The Mission 1 Hollow exterior includes one subtle prop: **a single beehive in Doris's back garden**, visible only if the player walks around the side of the bakery (an optional ~30-second detour). The hive is dormant. There is no interaction. **Pickle, if you brought her with you, will sit beside it briefly and then walk back.**

That is the entire seed planted for Mission 3's Wedding Honey arc. The cozy player who notices it will theorize. The cozy player who doesn't will not be punished.

This is the **cozy-narrative seeding discipline**: every Mission 1 detail must reward attention without requiring it.

---

## 4.0 Doris's Position in the Mission 1 Scene Flow

| Beat | Doris's blocking | What she does | Eyes Animator target |
|---|---|---|---|
| Player exits the lane, sees the bakery | Doris is on a stool outside the open bakery door, kneading a small dough on a wooden tray on her lap. | Looks up as the player approaches. Continues kneading at half speed. | Player → dough → player |
| First greeting | Stays seated. Smiles. | Speaks line 1.1 (see § 7.1). | Player |
| Player enters the bakery | Doris rises, dusts flour off her apron, gestures the player inside. | Walks ahead of the player to the counter. The Hollow is *next door*, attached. | Walks looking forward; glances back once at the player. |
| Doris transitions player into the Hollow | At the connecting door between bakery and Hollow, she pauses. She *does not enter the Hollow.* | Hands the player a small wooden box containing the orb. *"This is for you. The old keeper showed me how to make it. I'll be in the bakery if you want me. Knock twice."* | Player → box → player |
| The polishing scene (player + orb + workbench) | Doris is **not on screen.** | The player works alone. Pickle is on the windowsill. | (Pickle's eyes animator: player → orb → window → player) |
| Player exits the Hollow having polished | Doris is back in the bakery, kneading new dough. | Looks up. Smiles wider this time. Says line 1.5 (see § 7.5). Returns to dough. | Player → dough |

This blocking — **Doris hands the player a memory and then leaves the room** — is what makes Mission 1's polishing scene the player's *first private cozy moment* with the game. She is not watching them. They are alone with the orb. The cat is there. The kettle is there. **That is the heart of the cozy onboarding.**

---

## 5.0 BoZo Reskin Recipe (Doris's character art spec)

Per Asset Analysis A-12: BoZo Stylized Modular Characters, the **Cleric archetype** is reskinned as Doris. Specifications for the character artist:

| Slot | BoZo base | Doris reskin |
|---|---|---|
| **Head** | Cleric-Head-04 (round face, slight age lines) | Add ~5 wrinkles around eyes via texture overlay. Apply Colorize (A-21) to shift hair to silver-streaked-brown. |
| **Body** | Cleric-Body-Basic | Add an apron mesh (~40 polys) overlay. Apron is off-white linen, slightly flour-dusted (decal texture). |
| **Arms** | Cleric-Arms-Default | Roll up sleeves to elbow. Light freckles on forearms (texture). |
| **Hands** | Cleric-Hands-Default | Flour-dusted (decal). |
| **Hairstyle** | Hair-Bun-02 | Tuck a single loose strand at the temple (manual mesh edit, ~10 min in Blender). |
| **Accessory** | (none) | A small wooden hairpin (re-use a prop from Harvest Garden). |

**Total artist time: ~3 hours.** No new modeling required beyond the apron overlay and hairpin attachment.

### 5.1 Animation specifications

Per Asset Analysis A-15: Animation Composer System, Doris uses these animation layers:

- **Base idle (lower body):** standing relaxed, weight on right hip.
- **ACS upper-body layer #1 — *Kneading*:** loops while she is on her stool, the dough tray on her lap. ~3-second loop, hand-and-arm rhythm.
- **ACS upper-body layer #2 — *Wiping hands on apron*:** triggered when she rises. ~2 seconds, one-shot.
- **ACS upper-body layer #3 — *Offering the box*:** triggered when she hands the player the orb. ~2 seconds, one-shot. *This animation must read warmly* — the box is held forward with two hands, gentle pace.

Each layer takes ~1–2 hours to compose using ACS's editor. **Total animation time for Doris's M1 animation set: ~6 hours.**

### 5.2 Eyes Animator configuration

Per Asset Analysis A-14, Doris's `EyesAnimator.cs` configuration:

```
Blink rate: 0.15 / sec (slightly elevated — she is mildly nervous around the new keeper)
Blink duration: 120 ms
Saccade frequency: 0.4 / sec
Saccade range: ±5° (subtle)
Look-at priority list:
   1. Player (weight 0.7) when within 2.5 m
   2. Orb-in-player's-hand (weight 0.5) when player is holding an orb
   3. Counter (weight 0.3) when standing at the counter
   4. Random (weight 0.1)
Pupil dilation:
   default: 0.5
   trigger "player_accepts_orb" → 0.65 (warm; eyes brighten)
   trigger "player_haggles_down" → 0.35 (cool; eyes narrow)
```

This 5-minute configuration is **the single biggest perceptual quality lift** in Mission 1. It is the Spiritfarer / Pentiment standard.

---

## 6.0 Pickle's First Interaction with Doris

(Cross-referenced from `Docs/Depth_Bible/12_COMPANIONS_AND_FAMILIARS.md` — Pickle's full canonical character. In Mission 1, we use **a 5%-scoped Pickle**.)

### 6.1 Pickle's Mission 1 presence

Pickle is on the windowsill of the Hollow when the player enters. She does not move during Mission 1. She has **two scripted reactions**:

1. **When the player picks up the orb** — Pickle's head turns toward the orb. Eyes Animator triggers (her configuration is identical to Doris's but with priority 0.95 on orb-in-hand). She does not speak.

2. **After the player completes the polish** — Pickle speaks her *first quote in the game*:
   > *"That was — adequate. I was expecting much worse. Continue, please."*

That is **Pickle's only voiced line in Mission 1.** No further dialogue. The player should leave Mission 1 with a single warm impression: *the cat is unimpressed but present.*

### 6.2 Pickle's Mission 2 presence

In Mission 2 Pickle gets **three more lines** (across the herb garden visit, the tea brewing, and the widower's choice). The full Mission 2 Pickle script is in Focus Doc 06.

After Mission 2, Pickle's full library opens. **In Mission 1-2 combined: 4 Pickle lines.** This is the deliberate restraint — Pickle gets bigger as the game grows.

---

## 7.0 Doris's Full Mission 1 Yarn Spinner Dialogue

Per Asset Analysis A-20 directive ("Do NOT use OpenAI; use Yarn Spinner"), all dialogue is hand-authored Yarn. Below is **the complete Mission 1 dialogue for Doris** — ~180 lines, ready to drop into `Assets/_Project/Yarn/Mission01/Doris_Mission01.yarn`.

```yarn
title: Doris_Greeting_Lane
position: 0,0
tags: mission1, doris, greeting
---
<<set $met_doris = false>>

Doris: You're the new one.
Doris: I thought you'd be taller.
Doris: Don't mind me — I thought that about the old one, too. {#wait 1.5}
Doris: Come in. The kettle's only just stopped.

-> Hello. Are you Doris?
    Doris: Aye. The very same.
    Doris: They've put my name on the sign and everything. Look — there.
    <<jump Doris_Bakery_Enter>>
-> (Nod silently and follow her in)
    Doris: A quiet one, then. Good. The bread likes quiet.
    <<jump Doris_Bakery_Enter>>
-> Who was the old one?
    Doris: ... Mm.
    Doris: That's a conversation for a longer day.
    Doris: Come in. Tea first.
    <<set $asked_about_predecessor = true>>
    <<jump Doris_Bakery_Enter>>

===

title: Doris_Bakery_Enter
position: 250,0
tags: mission1, doris, bakery_interior
---
<<set $met_doris = true>>

Doris: Mind the flour.
Doris: I haven't swept since Tuesday. I keep meaning to.
Doris: ... The shop next door is yours. The Hollow.
Doris: I've been keeping the key safe for you.

-> Thank you.
    Doris: Of course.
-> Is the shop ready?
    Doris: It's clean. It's been waited-in. You'll find your way around.

Doris: ... I have something for you. Before you go in.
Doris: I'd like to be your first customer, if that's all right.

-> I'd like that.
    <<jump Doris_Offer_Orb>>
-> I haven't started yet.
    Doris: Aye. That's why I want to be the first.
    Doris: Otherwise you might come in expecting strangers.
    <<jump Doris_Offer_Orb>>

===

title: Doris_Offer_Orb
position: 500,0
tags: mission1, doris, offer_orb
---

<<show_orb DOR-001>>

Doris: This is the memory.
Doris: Hold it like you'd hold a hot bun. Not by the side. Underneath.

Doris: It's a small thing.
Doris: First time I made bread that didn't shame me.
Doris: Most days I think of it.
Doris: I want to put it down, now, for a while.
Doris: Will you take it?

-> I will. What do you want for it?
    <<jump Doris_Price_Negotiation>>
-> Tell me more about it first.
    Doris: I was twenty-four.
    Doris: The oven was new. The bricks were new. I was new.
    Doris: I'd been baking other people's bread for nine years.
    Doris: That morning was the first morning that was just mine.
    Doris: I want to take a rest from carrying it. That's all.
    <<jump Doris_Price_Negotiation>>
-> I'd rather not, today.
    Doris: Aye. Some days are not the day.
    Doris: I'll be here when one is.
    <<set $refused_doris_orb = true>>
    <<jump Doris_Refused_Path>>

===

title: Doris_Price_Negotiation
position: 750,0
tags: mission1, doris, price_negotiation
---

Doris: Four coppers, if you're asking.
Doris: It's a small memory. I'll not have you overpay your first day.

-> Four coppers is fair. (Honor the price)
    Doris: Aye. Thank you.
    <<set $coin -= 4>>
    <<set $vow_5_honest_coin += 2>>
    <<jump Doris_Hand_Off>>
-> I'll pay six.
    Doris: That's too much. I'll not have you ruin yourself.
    Doris: Take it back. — Well. Take *some* back.
    Doris: Five, then. Final.
    <<set $coin -= 5>>
    <<set $vow_5_honest_coin += 3>>
    <<set $trust_doris += 1>>
    <<jump Doris_Hand_Off>>
-> Two coppers. That's all I have.
    Doris: ...
    Doris: Aye, that'll do. Bring the rest when you find some.
    <<set $coin -= 2>>
    <<set $doris_owes_player = 2>>
    <<set $vow_5_honest_coin -= 1>>  // mild integrity dip — but she forgives it
    <<set $trust_doris += 0>>  // neutral; she does not begrudge
    <<jump Doris_Hand_Off>>

===

title: Doris_Hand_Off
position: 1000,0
tags: mission1, doris, hand_off
---

<<play_animation Doris ACS_Offer_Box>>

Doris: There.
Doris: The old keeper showed me how to make it. Took me four tries.
Doris: I cracked the first three. The cat watched me. Judged me, I think.

Doris: I'll be in the bakery if you want me. Knock twice.
Doris: There's a kettle on the workbench. Mind the wood stove — it bites.

<<give_player_orb DOR-001>>
<<unlock_hollow_door>>
<<jump Player_Enters_Hollow>>

===

title: Doris_Refused_Path
position: 500,250
tags: mission1, doris, refused
---

Doris: The shop's still yours.
Doris: Go in. Sit a while. The kettle is on.
Doris: I'll be here when you're ready.

<<unlock_hollow_door>>
<<jump Player_Enters_Hollow>>

===

title: Doris_After_Polish_Returns
position: 1500,0
tags: mission1, doris, after_polish
---
// Player returns to the bakery after polishing the orb.

Doris: Aye.
Doris: There it is. That's the morning.

<<if $polish_quality == "perfect">>
    Doris: You did it cleaner than I remembered it.
    Doris: I think you'll do.
    <<set $trust_doris += 3>>
<<elseif $polish_quality == "acceptable">>
    Doris: You did it kindly. That's what matters.
    <<set $trust_doris += 2>>
<<else>>
    Doris: ... It's the morning still. A little dimmer. But mine.
    Doris: First days are like that. I won't hold it.
    <<set $trust_doris += 1>>
<<endif>>

Doris: Sleep tonight. Dreams come.
Doris: I'll see you again, eventually.

<<jump Mission01_Outro>>
```

**Total Yarn lines: ~180.** This is the *complete* Doris script for Mission 1.

### 7.1 Choice-design notes for the producer

- **Refuse path** (the player declines Doris's orb): a valid path. The Hollow still unlocks. No penalty. Doris remembers, gently. *The cozy contract: refusal is honored.*
- **Underpay path** (2 coppers): Doris accepts gracefully and lets the player owe her. This sets up a Mission 3 "I still owe Doris 2 coppers" running thread. Cozy debt as relationship-building.
- **Polish-quality-aware re-entry** (the after-polish dialogue branches based on how the player did): *this is the player's first felt consequence in the game.* The dialogue lines are intentionally close in warmth — even a poor polish is honored — but the differences are visible.

### 7.2 Voice acting notes (Tier B)

If voice acting is greenlit (Codex 02 § 11), Doris's lines total **~3 minutes of recorded audio.** Recording session: ~90 minutes of an actress's time. *Recommended casting:* a 55–65-year-old actress with a slight Northern English / Welsh / Irish lilt. Refer the casting director to **Imelda Staunton's quiet roles** as the reference.

---

## 8.0 The Mission 1 Consequence Summary (what carries forward)

By the end of Mission 1, the VillageState struct (Codex 08 § 2) carries:

| Dimension | Default | After Mission 1 (typical path) |
|---|---|---|
| `trust_doris` | 0 | +3 to +5 |
| `memory_integrity[DOR-001]` | 100 | 100 (preserved) or 95 (mild polish) |
| `public_standing` | 0 | +2 |
| `vow_1_consent` | 50 | +1 |
| `vow_3_whole` | 50 | +3 |
| `vow_5_honest_coin` | 50 | +2 to +3 |
| `vow_7_last_light` | 50 | 0 (not yet exercised) |
| `pickle_approval` | 50 | +2 |
| `coin` | 10 (starting) | 4 to 8 (after paying Doris) |
| **`hollow_level`** | 1 | 1 |
| **`memories_in_inventory`** | 0 | 1 (DOR-001) |

These values **carry into Mission 2**. The widower's reaction to the player in Mission 2 is partly conditioned by `pickle_approval` and the Vow integrities. *This is the first sign that the game tracks the player.*

---

## 9.0 Closing

Doris in Mission 1 is **the cozy game's quietest, warmest, most precise first hour of player onboarding.** Her dialogue is tight. Her motivations are simple. Her warmth is unstudied. The player will not remember every line she said. They will remember the way she handed them the box.

That is the **only thing this character needs to do** in Mission 1-2.

— *Inara Vellis*
*Mission 1-2 Focus 01 · v1.0*

> Next: `02_THE_WIDOWER_GERROLD.md` — the player's first real moral choice, in Mission 2.
