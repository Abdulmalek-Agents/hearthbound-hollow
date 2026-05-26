# 🎮 Gameplay Guides — Mission 1 & 2

> Player-facing walkthroughs for the polished playable in `feat/mission-1-2-architecture`.

---

## 📘 Available guides

| Guide | What it covers | When to read |
|---|---|---|
| [`MISSION_01_GAMEPLAY_GUIDE.md`](./MISSION_01_GAMEPLAY_GUIDE.md) | **Mission 1 — Opening the Hollow.** Doris the baker, the Polish mini-game, your first memory orb, the predecessor (Marin) breadcrumbs, the Memory Dream 1 cutscene. | Read **before** pressing Play on a fresh save. |
| [`MISSION_02_GAMEPLAY_GUIDE.md`](./MISSION_02_GAMEPLAY_GUIDE.md) | **Mission 2 — The Widower's Request.** Gerrold Pell, the Echo Web's first activation, the herb garden + tea brewing, the 4-option moral choice (Erase / Cleanse / Listen / Defer), the Cleanse mini-game with 4 outcome states, the 5 Memory Dream 2 variants. | Read **after** completing Mission 1, ideally before sitting down with Gerrold for the first time. |

---

## 🗺️ The game's full Mission 1+2 flow at a glance

```
[Tone Compass] → [Main Menu — "Open The Hollow"]
                          │
                          ▼
                  ═══ MISSION 1 ═══
                  Day 1 — Opening the Hollow
                          │
            ┌─────────────┴─────────────┐
            │                           │
   02_Mission01_Lane            03_Mission01_Hollow
   ─ Lane spawn                 ─ Marin's Note (E×4)
   ─ Meet Doris                 ─ Workbench
   ─ Bakery interior            ─ Polish mini-game (~80s)
   ─ Receive orb DOR-001        ─ Pickle's first line
   ─ Walk to Hollow door        ─ Doris's after-polish line
                                ─ Evening Ledger Day 1
                                          │
                                          ▼
                              [Memory Dream 1 — cutscene]
                                          │
                                          ▼
                  ═══ MISSION 2 ═══
                  Day 2 — The Widower's Request
                                          │
                          ┌───────────────┴───────────────┐
                          │                               │
                03_Mission01_Hollow                       │
                ─ Wake up Day 2                           │
                ─ Gerrold knocks                          │
                ─ Echo Web first activation               │
                ─ Decide where to talk                    │
                          │                               │
              ┌───────────┴───────────────┐               │
              │                           │               │
       04_Mission02_Garden          (skip garden)         │
       ─ Lavender or Valerian           │                 │
       ─ Brew tea (~12s)                │                 │
              │                         │                 │
              └───────────┬─────────────┘                 │
                          ▼                               │
                05_Mission02_Cottage                      │
                ─ Sit in chair                            │
                ─ Offer tea                               │
                ─ 4-option Moral Choice ────────────────┐ │
                          │                              ▼│
              ┌───────────┼───────────┬──────────┐       ││
              ▼           ▼           ▼          ▼       ││
       [A — Erase]   [B — Cleanse] [C — Listen] [D — Defer]
              │           │           │          │       ││
              ▼           ▼           ▼          ▼       ││
       Cleanse mini  Cleanse mini  3-min cutscene  Mission ends; ──┘│
        (aggressive)  (careful)    (no mini-game)  M3 reopens choice
              │           │           │                              │
              └─────┬─────┴───────────┴──────────────────────────────┘
                    ▼
       [Memory Dream 2 — 5 variants by outcome]
                    ▼
       [Evening Ledger Day 2 — 5 prose variants]
                    ▼
       (Optional) Return handkerchief to Gerrold
                    ▼
              [Main Menu / Mission 3 future]
```

---

## ⚡ Quickest possible "I just want to know what to press" path

For impatient first-time players, the **fastest cozy-honoring path**:

| Step | Action |
|---|---|
| 1 | Main Menu → "Open The Hollow" |
| 2 | Read Tone Compass → "Continue" |
| 3 | Walk forward in Lane → press **E** on Doris |
| 4 | Pick "Hello. Are you Doris?" → follow her in |
| 5 | Pick "I will. What do you want for it?" → "Four coppers is fair." |
| 6 | Walk out → walk to Hollow door → press **E** |
| 7 | Press **E** 4 times on Marin's Note (the parchment on the workbench) |
| 8 | Click the workbench → hold left mouse + draw slow circles for ~80 seconds (or press "Polish for me") |
| 9 | Walk back to Doris (auto-appears) → "Sleep — End Day 1" |
| 10 | Watch the Dream 1 cutscene (~60 sec) |
| 11 | **MISSION 2 BEGINS** — Gerrold knocks. Open the door. |
| 12 | "Tell me about Margery first" (highly recommended) |
| 13 | Pick up the orb from his cloth |
| 14 | "Let's walk to your cottage" |
| 15 | (Optional) Step into the Garden → harvest 1 Lavender → brew tea → carry |
| 16 | Arrive at cottage → sit in the **left** chair (Gerrold's chair) |
| 17 | Offer the tea if you have one |
| 18 | Pick **"B — Cleanse with care"** (recommended for first run) |
| 19 | Walk back to Hollow → Cleanse mini-game: trace each crack avoiding the warm centre |
| 20 | (Or press "Cleanse for me" — same outcomes narratively) |
| 21 | Walk back to Gerrold's door → return the handkerchief |
| 22 | Evening Ledger → Memory Dream 2 → save/quit |

**Estimated time: 60–90 minutes** for the full Mission 1 + Mission 2 path.

---

## 🎨 Recommended reading order

1. **First-time player** — read just the first 5 sections of `MISSION_01_GAMEPLAY_GUIDE.md` (Mission Purpose, Map, Controls, Pre-Mission Checklist, walkthrough Step 1-4) before pressing Play. Refer to the rest as questions arise.
2. **Returning player who finished M1** — skim `MISSION_02_GAMEPLAY_GUIDE.md` §1-3 (the purpose + state Mission 2 reads from M1) to know what your Day 1 choices already locked in.
3. **Curious about consequences** — read §12 of the Mission 2 guide ("The 4 Mission 3+ Long-Term Effects") to see how Mission 2's choice reshapes Gerrold for the rest of the game.
4. **Designer / contributor** — both guides cross-reference the Depth Bible source documents. Use them as a quick map back to canonical specs.

---

## 🔗 Related documentation

| Document | What's in it |
|---|---|
| `../GAME_DESIGN.md` | Top-level vision, market analysis, monetization (~$44.7M projection) |
| `./ARCHITECTURE.md` | Technical architecture, asmdef graph, save schema |
| `./PROGRESS.md` | Live ledger of every phase shipped to the branch |
| `./Phase27_Environment_Polish_Plan.md` | The asset-pack-to-prefab map for the polished Mission 1 environment |
| `./Depth_Bible/Mission_1_2_Focus/01_DORIS_THE_BAKER.md` | The canonical Doris character bible — voice, blocking, ~180 lines of Yarn |
| `./Depth_Bible/Mission_1_2_Focus/02_THE_WIDOWER_GERROLD.md` | The canonical Gerrold character bible — Margery's life, ~270 lines of Yarn, 4 outcome branches |
| `./Depth_Bible/Mission_1_2_Focus/04_POLISH_AND_CLEANSE_MINIGAMES.md` | The mini-game specs — adaptive difficulty, auto-complete, audio cues |
| `./Depth_Bible/Mission_1_2_Focus/05_DREAM1_AND_DREAM2.md` | The Memory Dream 1 + 2 cutscene specs |
| `./Depth_Bible/Mission_1_2_Focus/06_TEA_LOOP_AND_MORAL_CHOICE.md` | The herb-garden-to-choice-screen chain |

---

## 💡 If a guide doesn't answer your question

The Depth Bible (`Depth_Bible/`) is the canonical source of truth for **every** narrative + system decision. The guides distill what a player needs to know. If you need *why* something is the way it is, the Depth Bible has it.

The codebase itself is the source of truth for *what actually happens* — see `Assets/_Project/Scripts/Mission/Mission01Director.cs` and `Mission02Director.cs` for the runtime behaviour.

---

*Index v1.0 — 2026-05-25 · Pair with the Phase 27 polished environment and the Phase 26 player controller for the canonical Mission 1+2 experience.*
