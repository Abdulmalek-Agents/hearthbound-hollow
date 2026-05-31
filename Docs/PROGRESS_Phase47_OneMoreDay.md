# 🌙 PROGRESS — Phase 47 "One More Day" Goodnight Beat (supplement)

> Companion to `Docs/PROGRESS.md` (Phase 0 → 47 level-polish) and
> `Docs/PROGRESS_Phase48_51_Supplement.md` (Depth Layer 48 → 51). This file
> covers the **One More Day** goodnight beat from
> `Docs/Phase47_OneMoreDay_Implementation.md`. Format mirrors the canonical
> PROGRESS.md so a future merge-back is mechanical.

---

## Legend
✅ Done & merged · 🟢 Done in branch (awaiting pull + `🚀 Build Everything`) · 🟡 In progress · ⬜ Not started

---

## 🌙 Phase 47 — "One More Day" Goodnight Beat 🟢 (2026-05-29)

The cozy "press continue *wanting* tomorrow" retention hook — a warm
forward-look card shown after the Evening Ledger (and after the night's Dream,
if any), before the next scene loads. Tier 1. Decision: **D-064** (the
implementation guide proposed D-060, which collided with the Depth Layer's
D-060; renumbered).

### Files added / edited

| Path | LOC | Status |
|---|---|---|
| `Scripts/UI/OneMoreDayCard.cs` | ~150 | NEW — presentational parchment overlay (UI asmdef) |
| `Scripts/Mission/TomorrowTeaseSO.cs` | ~45 | NEW — per-day data (Mission asmdef) |
| `Scripts/Mission/EndOfDaySequencer.cs` | ~135 | NEW — owns Ledger→Dream→Card→load |
| `Scripts/Editor/Phase47_OneMoreDayBuilder.cs` | ~320 | NEW — idempotent builder + Build-Everything step 15 |
| `Scripts/Mission/Mission01Director.cs` | +18 | EDIT — opt-in delegation, guarded |
| `Scripts/Mission/Mission02Director.cs` | +16 | EDIT — opt-in delegation, `playDream:false` |
| `Scripts/Mission/MissionRunner.cs` | +12 | EDIT — opt-in delegation, no dream rig |
| `Scripts/Editor/Phase27_BuildEverything.cs` | +9 | EDIT — chains Phase 47 OMD last |
| `ScriptableObjects/Missions/Tomorrow_M1_Day1.asset` | — | NEW (hand-authored, builder-healed) |
| `ScriptableObjects/Missions/Tomorrow_M2_Day2.asset` | — | NEW (hand-authored, builder-healed) |
| `Yarn/EveningLedger.yarn` | +3 nodes | EDIT — forward-look prose (Cordray) |
| `Yarn/Pickle.yarn` | +2 nodes | EDIT — goodnight sign-offs (Tannenbaum) |

### Night-order decision (D-064)

`Evening Ledger confirm → Dream (if any) → Goodnight Card → scene load`, owned by
one `EndOfDaySequencer` when wired, replacing the previous director + DreamHook
race. **Opt-in / zero-regression:** an unwired sequencer ⇒ the exact legacy
day-end path runs. The Phase 47 builder wires the Hollow + Cottage scenes and
clears `DreamHook.ledger` so the sequencer owns Dream 1 (no double-play).
Mission 2 wires `playDream:false` because Dream 2 already plays during the
cleanse outro (before the ledger).

### Deviations from the implementation guide (intentional, documented)

1. **Day-index fix** — tease resolves on `currentDayIndex + 1` (the 0-based index
   is only bumped by `EndDay()` *after* the card resolves); single-tease scenes
   always resolve. (The guide compared the raw 0-based index ⇒ the card would
   never have shown.)
2. **`refusedDorisOrb`** is the real VillageState branch flag (the guide cited the
   nonexistent `dorisRefused`) ⇒ the refusal path correctly shows the `_Refused`
   forward-look line.
3. **Accessibility** — the card also advances on Space / Return / E / Esc / click;
   Gentle-Mode instant-fade is passed in by the sequencer (the card stays
   game-state-free).

### Acceptance criteria

1. Regression gate: with `endOfDaySequencer` unwired, day-end is byte-for-byte today. ✅ (guarded)
2. Day 1 confirm → (Dream 1) → Tomorrow card → next, in order. 🟢 (verify after Build Everything)
3. Missing Yarn/SO degrades gracefully (forward-look only; no NRE). ✅ (designed)
4. `🔍 Diagnose Build` clean; EditMode + PlayMode smoke green. 🟢 (verify)
5. Boot → Day 1 → card → next, zero NRE. 🟢 (verify)

### Follow-ups

- **HH-OMD-T2** — Tier 2 visitor silhouette + Echo-thread glimmer (`TomorrowTeaseSO` fields reserved).
- **HH-OMD-T3** — Tier 3 next-morning payoff cue in the world on load.
- **HH-OMD-YARN** — runtime Yarn-dispatcher to replace the SO text mirror.
- **P47-OMD-VO** — optional composer goodnight sting under the fade.

*Doc cascade: this supplement · `ARCHITECTURE.md` (D-064) · `CHANGELOG.md`
([0.8.1-one-more-day]) · `STUDIO_LOG.md` (Phase 47-OMD entry).*

---

*Last updated: 2026-05-29 — Phase 47 One More Day goodnight beat.*
