# Mission 1-2 · Focus 07 — **Dialogue (Yarn Spinner) + The Evening Ledger UI**
### The dialogue architecture · The Ledger save flow · The first save in the cozy game
### Owners: Pell Doyne (Comfort) + Esme Cordray (Choice)

> *Two surfaces in this document. **Yarn Spinner architecture**: how Doris's and Gerrold's dialogue (already fully written in Focus 01 + 02) is wired into Unity, the UI, and the village state machine. **The Evening Ledger**: the player's only HUD, the player's first save mechanic, and the cozy game's daily-end ritual. Both surfaces ship in Mission 1, ironed in Mission 2.*

---

## 1.0 Why Yarn Spinner (Not the OpenAI Addon)

Per Asset Analysis Mission1-2 § 6 A-20:

> ⚠ The OpenAI Dialogue Addon (A-20) is purchased but **must not be used.**
> The GDD Production Pillar #1 — *"Every villager is fully written — no filler dialog. Every line could be in a novel."* — actively contradicts AI-generated dialogue.
>
> **Approved alternative:** Yarn Spinner (free, https://yarnspinner.dev/).

Yarn Spinner is:

- **Free** (MIT license, installable via Unity Package Manager git URL).
- **Author-friendly** — writers can edit `.yarn` files in any text editor. Plain-text. Version-controllable. Diffable.
- **Branch-aware** — `<<if $variable>>` syntax handles all the conditional branching the game needs.
- **Variable-storage compatible** — Yarn variables map cleanly to the VillageState struct (Codex 08 § 2).
- **Battle-tested** — used by Spritely's *Night in the Woods*, *A Short Hike*, Critical Hit Books' *Coffee Talk*. Cozy-genre default.

The OpenAI addon stays in Asset Inventory 4 tagged `Reference – Do Not Use In Build`. It is not deleted; it is dormant.

---

## 2.0 The Yarn Spinner Integration Architecture

### 2.1 Folder layout

```
Assets/_Project/Yarn/
├── Mission01/
│   ├── Doris_Mission01.yarn         ← Focus 01 § 7 — ~180 lines
│   ├── SilentVillager_Lane.yarn     ← (silent; placeholder for one nod animation hook)
│   ├── Pickle_M1.yarn                ← Pickle's 1 line for Mission 1
│   └── EveningLedger_M1.yarn         ← Day 1 closing prompts
├── Mission02/
│   ├── Gerrold_Mission02.yarn        ← Focus 02 § 7 — ~270 lines
│   ├── Doris_M2_Brief.yarn           ← Doris's morning greeting on Day 2 (~30 lines)
│   ├── Pickle_M2.yarn                 ← Pickle's 3 lines for Mission 2
│   ├── TeaModifiers_M2.yarn           ← The lavender/valerian dialogue insertions
│   └── EveningLedger_M2.yarn          ← Day 2 closing prompts
├── Shared/
│   ├── Codex_Tooltips.yarn            ← Examine-prop short prose entries
│   ├── Marin_Note.yarn                ← The Marin pinned-note text (M1 + M2 expanded version)
│   └── DefaultGreetings.yarn          ← Procedural villager nods / "Hello"
└── README.yarn                        ← Writers' style guide stub
```

### 2.2 The Yarn-to-VillageState bridge

Yarn variables are **prefixed** by their domain. A clean naming convention:

| Yarn variable | Maps to | Codex 08 dimension |
|---|---|---|
| `$trust_doris` | `VillageState.trust[VillagerID.Doris]` | Trust (per villager) |
| `$trust_gerrold` | `VillageState.trust[VillagerID.Gerrold]` | Trust (per villager) |
| `$memory_integrity_GER007` | `VillageState.memoryIntegrity[MemoryID.GER007]` | Memory Integrity |
| `$vow_1_consent` | `VillageState.vowIntegrity[Vow.Consent]` | Vow Integrity |
| `$vow_3_whole` | `VillageState.vowIntegrity[Vow.Whole]` | Vow Integrity |
| `$vow_5_honest_coin` | `VillageState.vowIntegrity[Vow.HonestCoin]` | Vow Integrity |
| `$vow_7_last_light` | `VillageState.vowIntegrity[Vow.LastLight]` | Vow Integrity |
| `$pickle_approval` | `VillageState.pickleApproval` | Pickle Approval |
| `$coin` | `VillageState.coin` | Economy |
| `$cinder` | `VillageState.cinder` | Economy |
| `$polish_quality` | (Mission 1 result variable) | Polish outcome |
| `$cleanse_quality` | (Mission 2 result variable) | Cleanse outcome |
| `$gerrold_choice` | (Mission 2 result variable) | Moral choice |
| `$met_doris` | flag | Onboarding state |
| `$first_moral_choice_made` | flag | Choice atlas state |

The bridge is implemented as a single C# class:

```csharp
public class YarnVillageStateBridge : MonoBehaviour {
    [SerializeField] private VillageState villageState;
    [SerializeField] private VariableStorageBehaviour yarnVariableStorage;

    private void Awake() {
        // Set up bi-directional sync — when Yarn writes a variable, it
        // immediately mirrors to VillageState. When VillageState changes
        // a variable, Yarn sees it next time it queries.
        yarnVariableStorage.SetVariable("$trust_doris", villageState.trust[VillagerID.Doris]);
        yarnVariableStorage.SetVariable("$trust_gerrold", villageState.trust[VillagerID.Gerrold]);
        // ... full list
    }

    public void OnYarnVariableChanged(string name, float value) {
        switch (name) {
            case "$trust_doris": villageState.trust[VillagerID.Doris] = (int)value; break;
            case "$gerrold_choice": villageState.gerroldChoice = (string)value; break;
            // ... full list
        }
        villageState.MarkDirty();
    }
}
```

Senior engineer time to implement: **~14 hours.**

### 2.3 The Yarn Custom Commands (the bridge's verbs)

Yarn's `<<command>>` syntax invokes C# functions. Mission 1-2 uses these custom commands:

| Command | C# behavior |
|---|---|
| `<<show_orb DOR-001>>` | Spawns the orb prefab in the scene at the counter location |
| `<<give_player_orb DOR-001>>` | Adds the orb to inventory + triggers animation |
| `<<unlock_hollow_door>>` | Enables the Hollow's interaction collider |
| `<<play_animation Doris ACS_Offer_Box>>` | Triggers an ACS upper-body animation layer |
| `<<echo_web_activate connection=DOR-001~GER-007>>` | Triggers the Memory Map Echo Web first-light animation |
| `<<player_picks_up_orb_from_cloth GER-007>>` | Compound: triggers the cloth-unwrap animation + adds orb to inventory + Cinemachine zooms |
| `<<orb_appears_cracked>>` | Sets the orb shader's `crack_intensity` to 0.6 |
| `<<show_choice_card_with_orb_in_hand>>` | Invokes the choice card UI (Bamao parchment frame) |
| `<<wait 1.5>>` | Standard Yarn pause |
| `<<jump LabelName>>` | Standard Yarn jump |

There are **~14 custom commands** across Mission 1-2. Each is ~30 lines of C#. Total custom command authoring time: **~12 hours.**

### 2.4 The Pickle commentary system

Pickle's dialogue is **internal** — only the player hears it (Codex 07 § 3.1, rule 7). Visually rendered as italic text in a different font + chime audio cue.

Yarn supports this via a custom `node` type tagged `tags: pickle, internal`. The dialogue UI checks the tags and renders Pickle's lines differently:

| Property | Pickle UI | Standard NPC UI |
|---|---|---|
| Font | TextMeshPro Italic, slightly smaller | TextMeshPro Regular |
| Background | None (text floats over scene at low opacity) | Bamao parchment box |
| Audio cue | Pickle leitmotif chime (8s, very faint) | Standard voice line |
| Speaker name | "Pickle" in faint amber | Speaker name in standard |
| Skippable | Yes | Yes |

This dual-mode dialogue presentation is the cozy game's **first interaction-aware UI**. Cost: ~6 hours of UI design.

---

## 3.0 Writers' Style Guide (Mission 1-2 specific)

To ensure Doris and Gerrold sound coherent across both missions:

| Rule | Doris | Gerrold |
|---|---|---|
| Sentence length | Short (~12 words) | Variable; long when nervous, short when calm |
| Verbal tic | Bread metaphors | "The long bit" |
| Names | Avoids "Elric" in M1 | Says "Margery" by mid-M2 |
| Cultural register | Rural Northern | Rural Northern + slight craftsman formality |
| Profanity | None | None |
| Pet names | Calls the player "keeper" by end of M1 | Calls the player "keeper" only after the choice |
| Apologies | Almost never | Constantly |

These rules are codified in `Assets/_Project/Yarn/Shared/StyleGuide.md` as a writers' onboarding checklist for any future writer joining the team.

---

## 4.0 The Evening Ledger — Architecture

### 4.1 What the Evening Ledger is

A leather-bound book that opens on the desk in the Hollow's workbench room every in-game evening (~17:00). It is **the only HUD the game has** (Codex 08 § 10).

It shows:

1. **The day's transactions** — every memory bought, polished, cleansed, etc.
2. **The day's villagers visited** — short prose summary of each interaction.
3. **The open story threads** — fiction-flavored list of obligations.
4. **The Hollow's state** — Hollow level, garden state, Pickle's mood (visualized through an inkblot drawing of her current sleeping location).
5. **The 7 Vow glyphs** — at the corner of every page, slightly dimmed/brightened by current integrity.

### 4.2 The Ledger UI prefab (Bamao Pack)

Per Asset Analysis S-4: Bamao Fantasy GUI's open-book layout is the Ledger's frame.

```
              ┌──────────────────────────────────────────────────────┐
              │                                                      │
              │   ┌────────────────────┐    ┌────────────────────┐  │
              │   │                    │    │                    │  │
              │   │   LEFT PAGE        │    │   RIGHT PAGE       │  │
              │   │                    │    │                    │  │
              │   │   The day's        │    │   Open threads     │  │
              │   │   transactions     │    │   (the gentle      │  │
              │   │   in fiction-      │    │    quest log)      │  │
              │   │   voice            │    │                    │  │
              │   │                    │    │                    │  │
              │   │   [scrolls if      │    │   [Hollow state    │  │
              │   │   needed]          │    │    + Vow glyphs    │  │
              │   │                    │    │    at bottom]      │  │
              │   │                    │    │                    │  │
              │   └────────────────────┘    └────────────────────┘  │
              │                                                      │
              │    [glyph: 1] [2] [3] [4] [5] [6] [7]                │
              │    (subtly lit / dim per Vow integrity)              │
              │                                                      │
              │           [Close the book] [Examine a page]          │
              │                                                      │
              └──────────────────────────────────────────────────────┘
```

### 4.3 Day 1's Evening Ledger (Mission 1)

After the polish, after the player places the orb on the shelf, after they have walked back to Doris's bakery and returned, the Ledger opens. **This is the player's first save.**

Left page (the day):

> *Day 1 — Spire-Month, Week 1*
>
> *You arrived at the Hollow this evening. The door was unlocked. The kettle was warm.*
>
> *Doris, the baker, was your first visitor. She offered her First Loaves — a memory from the morning her bakery opened, fifty years ago. You polished it. You did your work. She seemed pleased.*
>
> *Pickle, the cat who lives in the shop, was on the windowsill. She did not move. She is watching you.*
>
> *The shop is quiet. The shelf is no longer empty.*

Right page (threads):

> *Open work:*
>
> *— Doris's First Loaves is on the shelf. You may return to it any morning.*
> *— The shop has three more empty shelves. The Keeper before you left them.*
> *— There is a locked room upstairs. The door is heavy.*
> *— There is a man at the lane's bend, walking with a letter. You did not speak to him.*
>
> *Hollow Level 1 — the shop is open for business.*

Bottom of left page:

> *The Seven Vows*
> *[ glyph: ⊙ ] Consent — kept*
> *[ glyph: ⊙ ] Return — untested*
> *[ glyph: ⊙ ] Whole — kept*
> *[ glyph: ⊙ ] Quiet Glass — untested*
> *[ glyph: ⊙ ] Honest Coin — kept*
> *[ glyph: ⊙ ] Open Door — untested*
> *[ glyph: ⊙ ] Last Light — untested*

The glyphs are **all softly lit** at this point (default 50 integrity for untested, +2 or +3 brighter for the ones the player practiced).

### 4.4 Closing the Ledger = Saving

Pressing the **"Close the book"** button:

1. The book gently closes audibly.
2. Pickle, on the windowsill, stretches.
3. The composer's main theme enters at 25% volume.
4. The screen fades to amber.
5. **The save runs silently in the background.** No "Saving..." UI overlay. The player should not feel they have just done a system action.
6. Memory Dream 1 begins (Focus 05 § 2).

This is the cozy game's discipline: **save = the act of closing a book at the end of a day.** Not a menu choice. Not a notification. A gesture inside the fiction.

### 4.5 The save data structure

```
HearthboundSave_Slot1.json:
{
  "save_version": 1,
  "playthrough_id": "uuid-...",
  "in_game_day": 2,
  "in_game_month": "spire",
  "village_state": {
    "trust": { "doris": 5, "gerrold": 0 },
    "memory_integrity": { "DOR-001": 100, "GER-007": null },
    "vow_integrity": { "consent": 51, "whole": 53, ... },
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
    "pickle_sass_intensity": 3,
    "auto_complete_polish": false,
    "auto_complete_cleanse": false,
    ...
  }
}
```

**Save file size: ~3 KB.** Saves on cloud (Steam Cloud / Switch saves) without size pressure.

Save mechanism: **3 rolling save slots + 1 emergency auto-save.** The player can manually save by closing the Ledger; auto-saves occur on scene transitions. The cozy contract: never lose progress.

### 4.6 Day 2's Evening Ledger (Mission 2)

The Ledger after Mission 2 is **the cozy game's most consequential piece of UI.** The left-page transactions narrate the player's moral choice in fiction:

If choice was **Cleanse Perfect**:

> *Day 2 — Spire-Month, Week 1*
>
> *Gerrold Pell, the widower, came to the Hollow at half past seven. He brought a memory wrapped in his wife's handkerchief. You took it. You did the work. The work was clean.*
>
> *You walked to his cottage. You sat in his chair. You returned the handkerchief.*
>
> *Margery Pell, who died last winter, is now a clearer memory than she was this morning. The man who carried her will sleep tonight.*

If choice was **Listen**:

> *Day 2 — Spire-Month, Week 1*
>
> *Gerrold Pell brought a memory. You declined to take it.*
>
> *You sat in his cottage for three hours while he spoke. He told you everything.*
>
> *He still has the memory. He is no longer alone with it.*

If choice was **Erase Crossed Core**:

> *Day 2 — Spire-Month, Week 1*
>
> *Gerrold Pell brought a memory. You took it. The work did not go as planned.*
>
> *He thanked you. He is going home.*
>
> *His wife's face, in his memory, is no longer entirely her own.*

These three narrative summaries are **hand-written for each choice outcome.** They are the player's primary feedback on what they did. Cozy game discipline: no numbers in the Ledger. Just stories.

### 4.7 The Vow glyph visual update

After Mission 2's choice, the Vow glyphs on the Ledger's left page shift:

- **Cleanse Perfect**: Vow 3 (Whole) glyph brightens significantly. Vow 1 (Consent) glyph holds steady.
- **Cleanse Crossed Core**: Vow 3 (Whole) glyph dims slightly. The other glyphs hold.
- **Listen**: Vow 7 (Last Light) glyph brightens significantly — *and a soft hum plays when this happens, the first time the player has heard Vow 7's audio signature.* This is a small reward.

The player does not need to consult the glyphs. The cozy player will check them, sometimes, and feel the weight.

---

## 5.0 The Ledger's Interactivity (small but warm)

The Ledger is **not** a passive end-of-day screen. The cozy player can:

| Interaction | What happens |
|---|---|
| **Hover over a transaction** | The full prose of that memory's `action_summary` is revealed in a side-panel |
| **Click on a Vow glyph** | A small Marin-handwritten reflection on that Vow appears: *"Consent — the first vow. The hardest to remember during a long day. — M."* |
| **Examine the previous day's page** | Flip back (if it exists). The player can re-read their first day. |
| **Pet the inkblot of Pickle** | An easter egg — a small Pickle purr plays. The inkblot does not move. *Pickle herself, in the shop, briefly tilts her head.* |
| **Examine the "Open work" thread list** | Each thread expands to ~2 sentences of context, fiction-voiced. |

These small interactions reward the player who lingers. They are **ungated.** They are **never required.** They are pure cozy bonus.

Total interactive Ledger features for M1-2: **~12 hours of UI implementation.**

---

## 6.0 The Comfort Tools Menu (per Codex 06 § 7)

The Comfort Tools Menu is accessible from:
- The Pause menu (always).
- The Tone Compass intro screen (only on first launch).
- The Ledger's right-page bottom corner (a small *"Settings"* icon).

The menu's content for Mission 1-2:

| Toggle | Default | Implementation status |
|---|---|---|
| Gentle Mode | OFF | ✓ Fully wired |
| Auto-complete Polish | OFF | ✓ Fully wired |
| Auto-complete Cleanse | OFF | ✓ Fully wired |
| Auto-resolve Tribunals | (n/a in M1-2) | 🟡 Stub (no Tribunals in M1-2) |
| Disable Restoration Race timers | (n/a in M1-2) | 🟡 Stub |
| Color-blind mode | OFF | ✓ Fully wired (3 palettes from Heat UI's preset support) |
| Reduce particle intensity | OFF | ✓ Fully wired |
| Subtitle size | Medium | ✓ Fully wired (4 tiers) |
| Subtitle background | Translucent | ✓ Fully wired |
| Reduce screen flash | OFF | ✓ Fully wired |
| Dyslexia-friendly font | OFF | ✓ Fully wired (OpenDyslexic alternative) |
| One-hand controls | OFF | ✓ Fully wired (via Character Controller Pro's input system) |
| Voice volume per character | individual | ✓ Fully wired |
| Pickle sass intensity | 3/5 | ✓ Fully wired (settings 1–5; M1-2 only differentiates 1, 3, 5 — lines 2 and 4 collapse to nearest) |
| Memory hum volume | 5/5 | ✓ Fully wired |
| Animal warning tags | ON | (n/a in M1-2; stub) |
| Heavy theme warning cards | ON | ✓ Fully wired |

**Comfort Tools Menu total implementation: ~24 hours.**

---

## 7.0 The Tone Compass (per Codex 06 § 4)

The Tone Compass is **the player's pre-experience honesty layer.** Shown:

1. **On the Steam page** — a badge: *"This game contains: grief, dementia, loss, themes of memory loss in elders, a meditation on death."* + a hyperlink to the full Content Index.
2. **On first launch** — a 90-second illustrated card shown before the title screen.
3. **In the pause menu** — always accessible, listing every tag.

### 7.1 The Tone Compass card (90 seconds, first launch only)

```
                        ┌──────────────────────────────────────┐
                        │                                      │
                        │       Hearthbound Hollow             │
                        │                                      │
                        │       This game will make you        │
                        │       feel things.                   │
                        │                                      │
                        │       Some of those feelings         │
                        │       are heavy.                     │
                        │                                      │
                        │       This first hour contains:      │
                        │         · The opening of a shop      │
                        │         · A first transaction        │
                        │         · A late-night brewing       │
                        │                                      │
                        │       The second hour contains:      │
                        │         · A widower's grief          │
                        │         · A choice about memory      │
                        │         · A short illustrated dream  │
                        │                                      │
                        │       At any point, you can:         │
                        │         · Take a Soft Day            │
                        │         · Enable Gentle Mode         │
                        │         · Adjust any settings        │
                        │                                      │
                        │       The cat will be there.         │
                        │                                      │
                        │            [ Begin ]                 │
                        │                                      │
                        └──────────────────────────────────────┘
```

The card is **skippable** by pressing any button before the 90 seconds elapse. But by default it plays once. The cozy player who reads it is *prepared* for what comes.

### 7.2 The Tone Compass card production

| Item | Cost |
|---|---|
| Hand-painted illustrated card (a Pickle illustration in the corner) | $2,800 |
| Composer cue underneath (15 seconds of the main theme, slow piano arrangement) | (included in composer envelope) |
| Engineering integration | 8 hours |
| Localization (6 languages) | $1,200 |

This is **the most important UI surface in Mission 1-2** for the refund-rate KPI. Every dollar spent here returns 10x in Steam-refund-rate prevention.

---

## 8.0 First-Launch Flow — the Full Specification

| Time | Beat | Surface |
|---|---|---|
| 0:00 | Title screen with composer's main theme entry | Heat UI's main menu (reskinned in "Hearthbound" warm-parchment theme) |
| 0:05 | Player clicks "Open The Hollow" | Heat UI button |
| 0:06 | Loading screen with tip: *"Some memories want to be sold. Some don't."* | Heat UI loading screen |
| 0:09 | Tone Compass card | (see § 7.1) |
| 1:40 | Tone Compass card ends; "Gentle Mode?" prompt | A small dialog with two options |
| 1:45 | Player chooses Gentle Mode or Default | (stored in settings) |
| 1:47 | Opening cinematic begins | Cutscene Engine — 3 minutes |
| 5:00 | Cinematic ends; player spawns at the lane's edge | Real-time scene loads |
| 5:01 | Composer's main theme reaches the lane's volume (25%) | Player's first walk begins |

This first-launch flow takes **~5 minutes** before the player gains control. Every second is intentional. **The cozy player should not be impatient** because every second is interesting:

- Title screen: composer theme + reskinned Heat UI (warm parchment).
- Loading screen: a quiet tip.
- Tone Compass: a 90-second illustrated read.
- Gentle Mode prompt: 3 seconds.
- Opening cinematic: hand-painted, with composer cue.

Production cost of the first-launch flow: **~$18,000.** Single highest-ROI investment in Mission 1-2.

---

## 9.0 The Localization Plan (Mission 1-2 specific)

Per Codex 02 § 12, the launch languages are 6. For Mission 1-2 development, **all 6 must be ready by the vertical-slice playtest** because international cozy creators are part of the 200-person test.

| Language | Mission 1-2 word count | Translator cost |
|---|---|---|
| English (source) | ~3,200 (Doris+Gerrold dialogue + Codex tooltips + UI text) | (in-house) |
| Spanish (LatAm+EU) | ~3,200 | $640 |
| German | ~3,200 | $720 |
| French | ~3,200 | $720 |
| Japanese | ~3,200 | $960 (premium for fluent literary register) |
| Simplified Chinese | ~3,200 | $480 |

**Total Mission 1-2 localization cost: ~$3,520** (text) + $0 for audio (VO is English-only in M1-2; VO localization deferred to launch).

---

## 10.0 Production Cost Summary — Dialogue + Ledger

| Item | Hours / Cost |
|---|---|
| Yarn Spinner integration (the bridge) | 14 hours engineering |
| Yarn custom commands (~14 commands) | 12 hours engineering |
| Doris's M1 dialogue authoring | 8 hours writing (already in Focus 01) |
| Gerrold's M2 dialogue authoring | 14 hours writing (already in Focus 02) |
| Doris's M2 morning dialogue (~30 lines) | 4 hours writing |
| Tea modifier insertions | 4 hours writing |
| Pickle dialogue (4 lines total in M1-2) | 2 hours writing |
| Codex tooltips for examinable props | 6 hours writing |
| Ledger UI implementation (Bamao integration) | 18 hours UI |
| Ledger interactivity features | 12 hours UI |
| Day 1 Evening Ledger prose | 4 hours writing (multiple branches) |
| Day 2 Evening Ledger prose (across 5 choice outcomes) | 12 hours writing |
| First-launch flow (Tone Compass + Gentle Mode prompt) | $18,000 art + composer + engineering |
| Comfort Tools Menu | 24 hours UI + 8 hours design |
| Save system (rolling 3 slots + autosave) | 16 hours engineering |
| Localization (6 languages) | $3,520 + 8 hours coordination |
| QA + accessibility testing | 32 hours |
| **Total** | **~$23,500 + ~210 engineering+UI+writing hours** |

---

## 11.0 The Final Discipline — Words First

The cozy game lives in its words. Every Yarn line that Doris speaks, every prose sentence in the Evening Ledger, every Codex tooltip on an examined prop, every Pickle quip — these are the surfaces the cozy player will read.

The writer's job is to make sure every one of those surfaces is **worth reading.** No filler. No procedural stitching. Every sentence is a hand-craft.

Mission 1-2's word count — ~3,200 words — is roughly the length of a New Yorker short story. It will be hand-written, hand-edited, hand-localized. It is the studio's first published prose.

The cozy player will read it twice. The first time they will read it for the story. The second time they will read it to see what changed because they made different choices.

— *Pell Doyne + Esme Cordray*
*Mission 1-2 Focus 07 · v1.0*

> Next: `08_PRODUCTION_CHECKLIST.md` — what the engineering, art, and QA teams build, and the Mission 1-2 vertical-slice greenlight criterion.
