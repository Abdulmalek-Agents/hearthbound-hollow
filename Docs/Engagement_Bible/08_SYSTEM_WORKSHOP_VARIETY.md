# 🧩 System 08 — The Living Workbench: Variety, Juice & Mastery (Pillar P5)

> **Owner:** Mini-Game Designer + Gameplay Programmer. **Implements:** the fix for Root Cause
> #3 (mechanics are "experiences," not "skills"). **Goal:** make the core craft — the thing
> the player does most — *satisfying on the 50th repetition*, through variety, gentle mastery,
> and game-feel "juice." Without breaking the no-fail Cozy Contract.

---

## 1. The problem, precisely

Polish (draw circles) and Cleanse (trace cracks) are the *only* verbs, and both are:
- **No-fail + auto-completable + single-pace** → no skill expression, no improvement curve.
- **Visually flat** → little particle/audio reward feedback ("juice").
- **Identical every time** → the 2nd time is the 20th time.

A cozy mechanic doesn't need difficulty. It needs **texture**: small variations, a gentle
"I'm getting better / this one's special" feeling, and a *delicious* moment of completion.
Compare Stardew fishing (gentle skill curve), Coffee Talk latte art (expression), Unpacking
(tactile satisfaction), PowerWash Simulator (the *juice* of completion).

---

## 2. Three levers (none of them add punishment)

### Lever A — Variety: a verb library, not two verbs
The Depth Bible already designed **12 mini-game families** (`13_PUZZLE_AND_MINIGAME_LIBRARY`)
and deferred 10. Un-defer a cozy subset so different memories ask for different craft:

| Verb | What it is | When a memory needs it | Status |
|---|---|---|---|
| **Polish** | Slow circles, restore clarity | Faded memories | ✅ ships |
| **Cleanse** | Trace cracks, avoid core | Cracked/traumatic memories | ✅ ships |
| **Sort** | Drag facets into the right order (a gentle jigsaw) | Jumbled memories | ⬜ new, cheap |
| **Weave** | Tessellate two memories into one | Echo-linked pairs | ⬜ designed (Codex 13) |
| **Listen** | No mini-game; hold space with the villager | Memories that shouldn't be altered | ✅ exists (M2) |
| **Steep** | Time a tea-infusion for an emotional tint | Memories needing softening | ⬜ ties to garden |

The memory's data (`MemoryNodeSO`) declares which verb(s) it accepts. The Request Board now
naturally produces *variety of activity*, not just variety of story. **`MiniGameBase` already
exists** as the abstraction — new verbs are new subclasses, no architecture change.

### Lever B — Gentle mastery (a skill ceiling, never a skill floor)
Keep **no-fail** (Cozy Contract), but add **a quality spectrum the player can chase**:
- A **"clarity/grace meter"** that rewards smoothness, coverage, and calm (already partially in
  Polish via clarity). Hitting "Perfect" feels earned; "Acceptable" is always fine.
- A subtle **Keeper's Hand** stat (hidden, surfaced only as warm flavor): the more you craft,
  the more often Pickle compliments a clean job, the more "Perfect"s unlock cosmetic shimmer.
- Tools (P3) and teas (P4) *gently* widen tolerances — the player *builds* their ease over
  time. **Improvement is real but optional.** Auto-Complete always yields "Acceptable."

> This is the Stardew-fishing lesson: most players never "master" it, but the *possibility* of
> mastery makes every cast feel like it matters. The skill is there for those who want it,
> invisible to those who don't.

### Lever C — Juice (the cheapest, highest-impact lever)
The current craft is mechanically fine but *under-rewarded* sensorily. Add (using owned packs —
`AllIn1ShaderNodes`, `Game UI & Puzzle SFX`, particle systems already imported):
- **Build-up:** the orb hums louder, glows warmer, set-piece scene resolves *inside* the glass as you progress (Polish already teases this — amplify it).
- **The pop:** completion = a soft amber bloom flash (respecting Reduce-Screen-Flash setting), a satisfying chime swell (already wired via `MissionAudioHooks`), Pickle's tail flick, the orb settling with a gentle bounce.
- **Tactility:** orb wobble/squash under the cursor, particle motes following the polish stroke, a faint resistance "give" when a crack seals.

**Juice is 80% of "feel" for 5% of the cost.** It's the single fastest win in this whole plan.

---

## 3. The "satisfying repetition" checklist

A craft mini-game is loop-worthy only if:
- [ ] **Completion feels great** (juice: visual + audio + Pickle reaction).
- [ ] **There's a quality you can chase** (Perfect vs Acceptable) — but Acceptable is never punished.
- [ ] **It varies** (different memories → different verbs, different crack layouts, different set-piece reveals).
- [ ] **It's skippable** (Auto-Complete from frame 1) and **one-handed / accessible** (Pao 5-test).
- [ ] **It pays into the loop** (finished memory → Wall + coin + possible Dream + Echo).

---

## 4. Implementation guidance

- New verbs subclass the existing **`MiniGameBase`** in `Scripts/MiniGames/`; they publish a
  completion event the directors already consume (`MemoryPolishedEvent` pattern). Add
  `MemorySortedEvent` / `MemoryWovenEvent` to `EngagementEvents.cs`.
- `MemoryNodeSO` gains `List<CraftVerb> acceptedVerbs` so data declares the craft.
- Juice is added inside the existing mini-game `Update`/completion paths — particle prefabs +
  `SfxPlayer.PlayOneShot` calls (the audio system is already reactive via Phase 41).
- Respect every comfort toggle: Reduce Particle Intensity, Reduce Screen Flash, Auto-Complete,
  Gentle Mode (longer/forgiving). These already exist in `ComfortToolsMenu`.

---

## 5. Step-by-step build order (Phase 66 in the roadmap)

1. **Juice pass first** (cheapest, biggest feel win): amplify Polish/Cleanse completion FX + audio + Pickle reactions, all behind comfort toggles.
2. Add `CraftVerb` enum + `acceptedVerbs` to `MemoryNodeSO`.
3. Implement **Sort** (a gentle drag-order jigsaw) as the third verb (`MiniGameBase` subclass).
4. Implement **Steep** (tea-infusion timing) linking garden teas to craft.
5. Add the hidden **Keeper's Hand** mastery flavor + "Perfect" cosmetic shimmer.
6. (Later) Implement **Weave** for Echo-linked pairs (ties to System 09).
7. Chain builders; update `PROGRESS.md`.

---

## 6. Acceptance criteria

- [ ] Completing any craft produces a *noticeably* rewarding moment (juice), tunable via comfort toggles.
- [ ] At least 3 distinct craft verbs exist; memories route to the right one via data.
- [ ] "Perfect" is achievable and feels earned; "Acceptable"/Auto-Complete is never punished.
- [ ] Every verb passes the Pao 5-test (no fail-lock, no sub-350ms reaction, one-handed, audio-optional, tutorial-in-fiction).
- [ ] Repetition feels good at 20+ crafts in a playtest (the real test).

---

## 7. Why this matters most for the *core*

The player will craft memories more than they do anything else. If the core verb is flat, the
whole loop is flat no matter how good the surrounding systems are. **Juice + variety + gentle
mastery turn the most-repeated action into the most-loved one** — the difference between
"I have to polish another orb" and "ooh, a cracked one — let me do this *right*."

---

*System 08 v1.0 — Next: `09_SYSTEM_CODEX_ECHO_METAGAME.md`.*
