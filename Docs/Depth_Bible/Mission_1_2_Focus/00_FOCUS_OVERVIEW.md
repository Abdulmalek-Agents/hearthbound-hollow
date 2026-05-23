# 🎯 Mission 1-2 Focus — Re-Scoping Note
### *The Production-Ready Depth for the Playable Opening*

> *Edited by Mara Ostlund (Editorial Director) — appended to the Depth Bible v1.0 after producer review.*

---

## 0. Why This Folder Exists

The 17 codices in `Docs/Depth_Bible/` (`00_INDEX.md` through `16_LIVEOPS_SEASONAL_ENDGAME.md`) deliver the **full long-term design canon** for a 30-hour cozy narrative game with 30 villagers, 17 predecessor fragments, an ARG, Letter-Bird Network, Memory Bees, Sommelier endgame, and 16 months of post-launch LiveOps.

That canon is correct. It is **not what we build in the next 4 months.**

`Docs/Asset_Analysis_Mission1-2.md` (already in the repo) locks the **immediate production scope** to:

- **Mission 1 — "Opening the Hollow"** — ~25–35 in-real-time minutes. Tutorial. One villager (Doris). Polish mini-game. First Memory Dream. The shop, the lane, a glimpse of garden.
- **Mission 2 — "The Widower's Request"** — ~30–40 in-real-time minutes. Herb garden + tea. Cleanse mini-game. **The player's first moral choice.** Second Memory Dream. The widower's cottage.

That is **~55–75 minutes of playable content** between the two missions. The 17-codex bible designed for **~50 hours of player time across 30 months of development.** The mismatch is the problem this folder solves.

---

## 1. The Re-Classification

Every codex 02–16 is now stamped one of three labels for production scheduling:

| Label | Meaning |
|---|---|
| 🟢 **IN-SCOPE (1-2)** | The system ships in Mission 1 or Mission 2. Its full canonical design is needed *now*. |
| 🟡 **PARTIAL (1-2)** | Only a small subset of the system is used in 1-2; the rest is Scaling Reference. |
| 🔴 **DEFERRED (Scaling Reference)** | The system is *not* used in Mission 1-2. Its codex remains in the repo as the long-form design for when we expand. **Do not implement until the producer greenlights post-Mission 2 scaling.** |

### Codex-by-codex classification

| Codex | Title | M1-2 Status | What is needed in 1-2 |
|---|---|---|---|
| 01 | Review Response Matrix | 🟢 IN-SCOPE | Risk register; production discipline |
| 02 | Narrative Bible | 🟡 PARTIAL | **2 of 12 hand-sealed villagers** (Doris + the Widower). No procedural villagers. No Vignette Library yet. No Revelation Chapters. |
| 03 | Worldbuilding & Lore | 🟡 PARTIAL | **Only Vows 1, 3, and 7** are introduced. Predecessor lore, Forgotten Year, Vance, Memory Stock Exchange, Mariska, the 17 fragments — all **DEFERRED**. The Locked Room remains locked. |
| 04 | Progression & Meta Systems | 🟡 PARTIAL | **Track 1 (Hollow upgrades)** — only Level 1 visible, no upgrades yet. **Track 4 (Garden)** — only Lavender + Valerian. Other tracks **DEFERRED**. |
| 05 | Conflict Without Combat | 🟡 PARTIAL | **Only the choice surface** of Mission 2's widower request. No Tribunals, Dream Duels, Restoration Race, Thief, Lawsuit, Vance Arc in 1-2. |
| 06 | Cozy Comfort & Accessibility | 🟢 IN-SCOPE | **Tone Compass** + **Gentle Mode toggle** at game start are mandatory for Mission 1. **Comfort Tools menu** ships from Day 1 of build. |
| 07 | Humor & Levity | 🟡 PARTIAL | **Pickle the Cat (4 quotes total in 1-2)** + **Codex marginalia (light)**. Vance, Magpie, Idris, Hayfever, Workshop Mishaps — **DEFERRED**. |
| 08 | Choice & Consequence Atlas | 🟡 PARTIAL | **3 of the 14 dimensions** tracked in 1-2: Trust (Doris + Widower), Memory Integrity (Widower), Vow 1 + Vow 3 integrity. Ripple Engine architecturally present but only used for 2-villager radius. |
| 09 | Roguelite & Replayability | 🔴 DEFERRED | Procedural Memory Generation, Drifter Mode, Weekly Market, NG+ Threads, Thief Path — all post-Mission 2. |
| 10 | Economy & Reputation | 🟡 PARTIAL | **Coin only.** Cinder, Stranger's Coin, Auction Day, Borrowed Memory, Compost Economy, Mobile compact — **DEFERRED**. |
| 11 | Memory Dream Director | 🟡 PARTIAL | **2 dreams only** (one per mission). **Use 2 set-pieces** (#18 "kitchen at first light", #14 "bedroom of a sick person"). **3 emotion lenses** (JOY, GRIEF, GRACE). **No Long Dreams in 1-2.** **No Dream Cinema** — that requires Hollow Level 9. |
| 12 | Companions & Familiars | 🟡 PARTIAL | **Pickle present from Day 1, light AI.** Brother Magpie, Echo Hologram, Ren the Apprentice — all **DEFERRED**. |
| 13 | Puzzle & Mini-Game Library | 🟡 PARTIAL | **Only Polish (M1) + Cleanse (M2)** of the 12 families. Weave, Sever, Listen, Read, Translate, Identify, Compose Honey, Search, Negotiate, Compose Verse — **DEFERRED**. Set-piece mini-games — DEFERRED. |
| 14 | Audio, Music & ASMR | 🟡 PARTIAL | **Main theme + 2 villager motifs + 2 dream stems + the Polish/Cleanse beds.** No full OST. No Memory-Tone shelf-tuning. No Idris bardic library. Composer hired in 1-2 window to deliver these specific pieces. |
| 15 | Community & Async Multiplayer | 🔴 DEFERRED | Letter-Bird Network, Pen-Pal Villages, Dream Cinema community, ARG, Photo Mode — *all* defer to post-Mission 2. |
| 16 | LiveOps, Seasonal & Endgame | 🔴 DEFERRED | The Five-Season roadmap, DLCs, Sommelier endgame, Crisis-Mode Playbook — all post-launch. The Greenlight Gate (G2) is *what we are building toward* — it tests the Mission 1-2 vertical slice. |

**Net effect:**
- **2 codices fully in-scope** (01, 06).
- **10 codices partially in-scope** — only specific systems.
- **5 codices deferred** as Scaling Reference (09, 15, 16 mostly; 03 and 05 partially).

---

## 2. What This Folder Contains

The eight focused documents below replace none of the codices but **distill from them** the production-ready spec for Mission 1-2. They are what the engineering, art, and writing teams build from for the next 4 months.

| # | File | Owner | Purpose |
|---|---|---|---|
| 00 | `00_FOCUS_OVERVIEW.md` | Editorial Director | This file. Re-scoping rationale. Classification of every codex. |
| 01 | `01_DORIS_THE_BAKER.md` | Inara Vellis (Narrative) | Mission 1 villager bible. 4-node Memory Map. Full prose + Yarn dialogue. Pickle's first interaction. BoZo reskin recipe. |
| 02 | `02_THE_WIDOWER_GERROLD.md` | Inara Vellis (Narrative) | Mission 2 character bible. The wife (Margery). The crack-laden memory. The 3-choice moral fork. Full prose + Yarn dialogue. |
| 03 | `03_SCENES_LANE_HOLLOW_GARDEN_COTTAGE.md` | Roan Avellan (Conflict) + Sven Aleko (Cinematics) | Scene-by-scene blocking: village lane at dusk → Hollow interior → herb garden → widower's cottage. Specific Medieval Village Megapack props. Lumen god-ray placement. Lighting cues per beat. |
| 04 | `04_POLISH_AND_CLEANSE_MINIGAMES.md` | Linnet Pao (Mini-games) | Granular spec for the two mini-games shipping in 1-2. Adaptive difficulty curves. Sample failure copy. Auto-Complete UI. SFX per phase. |
| 05 | `05_DREAM1_AND_DREAM2.md` | Sven Aleko (Memory Dream Director) | The two Memory Dreams as Cutscene Engine timelines. Choice-driven Dream 2 variants. Composer cues. |
| 06 | `06_TEA_LOOP_AND_MORAL_CHOICE.md` | Saija Korhonen (Progression) + Esme Cordray (Choice) | The herb-harvest → tea-brew → moral-choice flow. The 3-option Yarn tree. Pickle's reactions to each path. |
| 07 | `07_DIALOGUE_AND_LEDGER_UI.md` | Pell Doyne (Comfort) + Esme Cordray (Choice) | Yarn Spinner integration. Doris's M1 dialogue (full). Widower's M2 dialogue (full). The Evening Ledger UI flow. First save. |
| 08 | `08_PRODUCTION_CHECKLIST.md` | Halvor Krieg (Risk) | The 23 imported assets mapped to Mission 1-2 scenes. The C# scripts the engineering team must write. The QA test plan. The Mission 1-2 vertical-slice greenlight criterion. |

---

## 3. The Ship-or-Defer Test

For every system specced in any codex, the team asks **three questions** before adding it to the Mission 1-2 build:

1. **Is it needed for the player to complete Mission 1 or Mission 2?** If no → **DEFER**.
2. **Does removing it weaken the cozy/onboarding experience by more than 10%?** If no → **DEFER**.
3. **Can it be added in <2 dev-days during a later milestone?** If yes → **DEFER**.

A system passes only if it answers **YES, YES, NO**. Anything else gets stamped 🔴 DEFERRED in §1.

This is the **Mission 1-2 discipline.** It is what differentiates "we are shipping a slice" from "we are scope-creeping into another six months."

---

## 4. What Is *Not* in Mission 1-2 (the Scaling Roadmap)

The deferred systems are **not lost.** They are sequenced into post-Mission 2 development, gated by the Mission 1-2 vertical-slice playtest result.

### Post-Mission 2 sequencing (Scaling Reference)

| When | Scope Unlocked | Codex |
|---|---|---|
| **After M1-2 playtest passes** | Mission 3 onboarding — 2 more villagers, Weave mini-game intro, the first Tribunal seed | 02, 05, 13 |
| **Mission 4–6 window** | The Echo Hologram (Day 18 in-game equivalent), the Locked Room, Pickle's expanded AI, Brother Magpie | 03, 07, 12 |
| **Mission 7+ (mid-act)** | Vance Arc Episodes 1–3, the Confession Booth, Memory Bees, Composting, Borrowed Memories | 03, 04, 05 |
| **Late-game (Acts 2–3)** | Forgotten Year arc, Thief arc, Lawsuit, Mariska's lessons, all 17 MAR fragments | 03, 05, 08 |
| **Endgame layer** | Sommelier path, Apprentice (Ren), 6 canonical endings | 04, 12, 08 |
| **Post-launch Year 1** | Letter-Bird Network, Pen-Pal Villages, Dream Cinema community, Predecessor ARG, 5-season free-update roadmap | 15, 16 |
| **Post-launch Years 2–3** | The Travelers DLC, The Keeper Before You DLC, Anniversary events | 16 |

This roadmap is **not on the team's plate during Mission 1-2.** It exists so the producer knows what comes next, and so the engineering team can build clean architectures that *can extend* into these systems without rework.

---

## 5. The Architectural Discipline (so the Scaling layer doesn't require rewrites)

Even though the 🔴 DEFERRED systems are not implemented in 1-2, the architecture must **leave room** for them. Specifically:

| System | Architectural Hook in M1-2 (no implementation) |
|---|---|
| **Memory Card schema** (Codex 02 § 2.1) | Implement the full YAML/ScriptableObject schema, even though only 2 memories use it. This is *not* extra work — the data structure is the contract. |
| **VillageState struct** (Codex 08 § 2) | Implement all 14 dimensions as fields in the struct. Only 4 are written-to in 1-2. The rest sit at default values. |
| **Echo system** (Codex 02 § 9) | The Echo field on Memory Cards exists. No Echo Web UI yet. The data is captured. |
| **Vow integrity meters** (Codex 03 § 4.2) | All 7 meters exist. Only 1, 3, and 7 are moved by player actions in 1-2. The rest sit at 50 default. |
| **Vignette Library schema** (Codex 02 § 7) | Library schema defined. Library contents minimal (only the M1-2 floating slots populated, ~6 entries). |
| **Letter-Bird backend hook** (Codex 15 § 2.2) | A stub `LetterBirdService` interface exists. Local-only implementation that does nothing in 1-2. The interface is the architectural promise. |

This is the **Krieg Discipline:** *build the bones for what is coming; only flesh out what ships now.* The engineering team gets a clean architecture spec from `08_PRODUCTION_CHECKLIST.md`.

---

## 6. The Two Missions — One-Sentence Summaries

> **Mission 1 — *"Opening the Hollow."*** The player walks down an autumn lane at dusk, meets Doris the baker outside her shop with a memory she is willing to sell, polishes that memory clean at the workbench, places it on the shelf, reviews the day in the Evening Ledger, and dreams Doris's *First Loaves.* The game has taught them: *this is what a Keeper does.*

> **Mission 2 — *"The Widower's Request."*** The next morning the widower Gerrold Pell visits the Hollow with a memory he wants gone. The player learns to brew a tea (Lavender or Valerian) to help him open up, walks to his cottage to sit with him, decides — between erasing, partially cleansing, or refusing to take — what to do with the memory of his wife Margery, and dreams the consequence of that choice. The game has taught them: *every memory transaction is a moral act.*

Together, those two arcs are the **complete onboarding** for the rest of the game.

---

## 7. The Mission 1-2 Greenlight Criterion (the test we ship against)

Distinct from Codex 16's G2 full-game gate, the Mission 1-2 vertical slice has its own milestone:

> **20 cozy-target playtesters complete Mission 1 + Mission 2 (combined ~1 hour). At least 14 (70%) report "I want to play more."**

That is the threshold. The Mission 1-2 producer's job is to hit it. Codex 16's G2 (200-person paid playtest) follows *after* this internal validation succeeds.

The Production Checklist (`08_PRODUCTION_CHECKLIST.md`) breaks this down into 30 specific testable acceptance criteria.

---

## 8. Closing

This re-scoping is the **most important production decision** in the Depth Bible's life. The bible is the *map.* This folder is the *first leg of the journey.*

The team builds Mission 1-2.
The 200-person playtest validates it.
The producer decides whether to scale.
The other 14 codices wait, patient, for their turn.

That is the discipline.

— *Mara Ostlund*
*Mission 1-2 Re-Scoping Note · v1.0*

> Next: open `01_DORIS_THE_BAKER.md` to begin the Mission 1 character bible.
