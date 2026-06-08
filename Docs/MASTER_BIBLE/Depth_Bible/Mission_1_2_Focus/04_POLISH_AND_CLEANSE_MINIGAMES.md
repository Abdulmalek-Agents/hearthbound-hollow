# Mission 1-2 · Focus 04 — **Polish + Cleanse: The Two Shipping Mini-Games**
### Granular spec for Mission 1's Polish + Mission 2's Cleanse · Adaptive difficulty · Auto-Complete · Audio per phase
### Owner: Linnet Pao (Mini-Game & Puzzle Designer)

> *Two mini-games ship in Mission 1-2. **Polish** in Mission 1 (warm, low-stakes, the player's first hands-on interaction). **Cleanse** in Mission 2 (riskier, moral-weight-bearing, the player's first failure-state). The other 10 mini-game families in Codex 13 (Weave, Sever, Listen, Read, Translate, Identify, Compose, Search, Negotiate, Compose Verse) are **DEFERRED**. The Asset Analysis Mission1-2 § 5 S-5 (All In 1 Shader Nodes) is the most important asset in this codex — the memory orb shader does ~70% of the visual work in both mini-games.*

---

## 1.0 Why Just Two Mini-Games in Mission 1-2

The cozy game's onboarding rule (Codex 06 § 8 — Doyne's onboarding bible): **teach one mechanic per ~30 minutes for the first 90 minutes of play, then space out the rest.**

| Real-time minute | Mechanic introduced |
|---|---|
| 0:00–25:00 | Movement, interact, dialogue, choice, Codex |
| 25:00–35:00 | **Polish mini-game** + shelf placement + Evening Ledger + first save |
| 35:00–65:00 | Tea brewing + Memory Map first connection |
| 65:00–80:00 | **Cleanse mini-game** + first moral choice |

That is **two mechanics per hour** at the cozy onboarding pace. Three+ mechanics per hour reliably bounces 18% of cozy players (per the r/CozyGamers scrape Codex 06 § 9). Mission 1-2 obeys.

---

## 2.0 The Polish Mini-Game — Full Spec (Mission 1)

### 2.1 The verb

The player is removing **age-faded amber-fog** from the surface of a glass memory orb by moving the cursor (or finger or thumbstick) in slow circles over the orb. As the fog clears, the memory scene **becomes visible behind the glass.**

This is the cozy game's signature visual moment. The orb is the noun. The Polish is the verb. The reveal — *the memory's set-piece scene appearing through the clearing glass* — is the satisfaction.

### 2.2 Asset binding

| Asset | Used for |
|---|---|
| **All In 1 Shader Nodes** (S-5) | `MemoryOrb_Master` Shader Graph with `clarity` (0–1), `crack_intensity` (0–1), `dissolve_progress` (0–1), `glow_strength`, `palette_tint` properties |
| **Lumen Stylized Light FX 2** (S-6) | The point-light glow around the orb as it clears; a small Lumen "Candle_Warm" glow inside the orb after Polish completes |
| **VoluSmokeFX** (A-18) | A small particle wisp curling off the orb during Polish (the "fog being released") |
| **Game UI & Puzzle SFX Pack** (S-8) | Eight specific audio cues (see § 2.7) |
| **Cinemachine** | Camera moves to a tight workbench shot (the orb centered, the workbench corner visible, the kettle steam in the background) |

### 2.3 The 90-second Polish loop (Doris's orb in Mission 1)

```
Time     | Player input             | Shader state              | Audio        | Visual reveal
─────────┼──────────────────────────┼───────────────────────────┼──────────────┼──────────────────────────
0:00     | (begins — orb in cradle) | clarity = 0.4 (faded)     | hum start    | The orb glows amber-fogged.
                                     | dissolve = 0              | (palette-A   |  Set-piece "kitchen at
                                     |                           |  hum: JOY    |  first light" is BARELY
                                     |                           |  arpeggio)   |  visible behind the glass.
                                     |                           |
0:05     | Player starts polishing  | clarity climbs by 0.005   | rub_start    | A tiny clear patch where
         | (circles on the surface) | per second of motion       | (soft glass-|  the cursor moves.
                                     |                           |  on-velvet)  |
                                     |                           |              |
0:10     | Player polishing         | clarity = 0.42            | rub_loop     | The clear patch expands
                                     |                           | (continues   |  with each circle.
                                     |                           |  while       |  Composer cue rises.
                                     |                           |  polishing)  |
                                     |                           |              |
0:25     | (half-way milestone)     | clarity = 0.55            | midway_chime |  ~30% of the orb is clear.
                                     |                           | (one soft    |  Doris's young hands
                                     |                           |  bell)       |  visible briefly behind.
                                     |                           |              |  Pickle's head turns toward
                                     |                           |              |  the orb (animation
                                     |                           |              |  trigger).
                                     |                           |              |
0:45     | Player polishing         | clarity = 0.7             | rub_loop +   | The kitchen scene is now
                                     |                           |  faint kettle|  clearly visible inside
                                     |                           |  hiss        |  the orb. The oven, the
                                     |                           |              |  loaves on the rack.
                                     |                           |              |
1:05     | (player covers all      | clarity = 0.95            | reveal_swell | Composer's "Doris motif"
         | quadrants of the orb)    |                           | (full musical|  enters fully. The orb
                                     |                           |  cue: Doris  |  glows brightly. The fog
                                     |                           |  motif       |  is entirely gone.
                                     |                           |  ~3 seconds) |
                                     |                           |              |
1:15     | (auto-complete-       | clarity = 1.0             | success_jingle| The orb is perfectly
         | trigger as final 5%    |                           |              |  clear. The kitchen
         | clears)                |                           |              |  scene is fully visible.
                                     |                           |              |  Pickle's tail flicks
                                     |                           |              |  once (approval).
                                     |                           |              |
1:18     | (end — orb returns to  | clarity = 1.0             | hum_post     | Orb sits in cradle,
         | cradle)                |                           | (palette-A   |  glowing softly. Player
                                     |                           |  hum, gentler|  can pick it up to place
                                     |                           |  now)        |  on a shelf.
```

**Median completion time at Default difficulty: ~78 seconds** (with some players faster, some slower).

### 2.4 Adaptive difficulty (Polish)

Per Codex 13 § 3, the Polish mini-game tunes to player skill:

| Player band | Polish behavior |
|---|---|
| **First polish ever** (Doris's DOR-001 in M1) | All thresholds at the lowest difficulty. The mini-game cannot fail. Even random cursor movement clears the orb in ~110 seconds. **This is the tutorial-tier polish.** |
| **Subsequent polishes** (M3+) | Difficulty scales with `memory_weight`. Weight 4 (DOR-001) = same as tutorial. Weight 7+ memories require ~150 seconds and more precise coverage. |
| **Gentle Mode** | Auto-complete is *suggested* at the start. The Pickle commentary changes to a gently encouraging tone. |

### 2.5 Failure semantics (Polish — tutorial-tier)

In Mission 1, **Polish cannot fail.** Worst case: the player stops polishing partway through and walks away. The orb stays at whatever `clarity` they got to. They can come back to it.

There is **no fail state on the first polish.** This is a deliberate cozy-onboarding choice. The cozy player learns that the mini-game is a *gentle ritual,* not a *test.* This impression carries forward to Cleanse (Mission 2), where actual failure becomes possible.

### 2.6 The Auto-Complete toggle for Polish

Per Codex 06 § 7, the Auto-Complete toggle is **visible in the corner of the screen from the very first frame of the Polish mini-game.** It is not buried in a menu.

UI placement:
- Bottom-right corner of the workbench frame.
- Label: *"Polish for me"* (soft, in fiction-voice — not "Skip Mini-Game").
- One button click triggers the auto-complete.
- Auto-complete plays a ~12-second sped-up version of the same Polish animation (still respects the cozy aesthetic — *the system polishes it,* the player watches).
- Result: same `clarity` outcome as a Default-quality manual Polish (85% perfect).
- Achievements / tariffs: identical. No "manual bonus."

This is the **Codex 06 collaboration: nobody who Auto-Completes misses content or rewards.**

### 2.7 Audio cues (8 specific sounds) — full list

| Cue ID | When | File length | Volume |
|---|---|---|---|
| `polish_hum_start` | When player picks up the orb | 0.5s + loop | 0.4 |
| `polish_hum_loop` | While the orb is in the cradle, not being polished | loop 8s | 0.3 |
| `polish_rub_start` | First circle the cursor makes | 0.3s | 0.5 |
| `polish_rub_loop` | While player is actively polishing | loop 1.4s | 0.45 |
| `polish_rub_friction_warn` | If the player polishes too fast | 0.6s, plays once per friction event | 0.5 |
| `polish_midway_chime` | At `clarity = 0.55` threshold | 0.8s | 0.6 |
| `polish_reveal_swell` | At `clarity = 0.85` — composer's Doris motif | 3.0s | 0.7 (rises to 0.85) |
| `polish_success_jingle` | At `clarity = 1.0` | 1.2s | 0.55 |
| `polish_hum_post` | After Polish, while the orb is in the cradle, clear | loop 8s | 0.3 |

(Nine cues total, not eight — the Asset Analysis estimate of 8 was conservative. Adding the friction-warn is the small QoL beat. All cues are from the Game UI & Puzzle SFX Pack except for the composer's `polish_reveal_swell` motif.)

### 2.8 Pickle's Polish commentary

Pickle speaks **one line during Polish** in Mission 1 (her first line in the game, per Focus 01 § 6.1).

> *(after the orb fully clears)* *"That was — adequate. I was expecting much worse. Continue, please."*

This single line is the cozy game's first comedic beat (Codex 07). It plays after the success jingle. Pickle's tail flicks once. The cozy player will laugh quietly.

### 2.9 Edge cases

| Edge case | Behavior |
|---|---|
| Player polishes the orb's *back* (rotating it) | The shader supports 360° clarity tracking. Both sides clear. Slight bonus for thoroughness. |
| Player polishes for less than 10 seconds total and walks away | Orb stays at `clarity = 0.45`. Doris's after-polish dialogue uses the third branch ("It's the morning still. A little dimmer.") |
| Player polishes for the full 90+ seconds with high coverage | Orb hits `clarity = 1.0` perfect. Doris's "You did it cleaner than I remembered" line plays. |
| Player accidentally triggers Auto-Complete | Auto-Complete plays, no penalty. They learn the toggle exists. |
| Player has Memory Hayfever (Codex 07 § 5.1) | **Cannot happen in M1.** Memory Hayfever requires multiple Cleanses (Codex 13 § 5). M1 has zero Cleanses. |

### 2.10 Polish's tutorial-in-fiction (no UI overlay tutorial)

Per Codex 13 § 10 Rule 5: every mini-game is **taught by a villager, not by a UI overlay.**

The Polish tutorial is Doris's prior dialogue + Marin's note above the workbench:

> *(Marin's note, pinned above the workbench, examinable by the player before they begin)* *"Polish in slow circles. The faster you press, the less the orb hears you. Cover all sides. Most memories need ninety seconds. — M."*

That is the **entire tutorial.** The player reads the note (or doesn't). They sit at the workbench. They see the orb. They have a cursor. They figure it out.

If the player struggles for >30 seconds without clearing any of the orb, **a soft hint appears**:

> *(Pickle, from the windowsill)* *"Slower."*

That is the only hint Pickle offers. The cozy player either gets it or Auto-Completes.

---

## 3.0 The Cleanse Mini-Game — Full Spec (Mission 2)

### 3.1 The verb

The player is **tracing the cracks** on the surface of a memory orb to seal them, *without crossing into the core memory region* in the orb's center. Each crack must be traced once, fully, without lifting the cursor. The core memory glows soft amber; crossing it causes irreversible memory damage.

This is the **cozy game's first real-stakes mini-game.** Polish was warm. Cleanse is careful. Cleanse has a *Crossed Core* failure outcome that is fully narratively absorbed (Focus 02 § 7).

### 3.2 Asset binding

| Asset | Used for |
|---|---|
| **All In 1 Shader Nodes** (S-5) | The same `MemoryOrb_Master` shader, but with `crack_intensity = 0.6` (significant cracks) and a defined "core region" highlighted in soft amber |
| **VoluSmokeFX** (A-18) | A different particle effect for Cleanse: a tracing line of "memory-thread" that follows the cursor along the crack |
| **Game UI & Puzzle SFX Pack** (S-8) | Dedicated Cleanse audio set (see § 3.7) |
| **Cinemachine** | Same workbench shot as Polish, but tighter (the cracks need to be visible) |
| **Cutscene Engine** (S-7) | If the player crosses the core, a brief 4-second cinematic plays (the orb dims, Margery's face fades) |

### 3.3 Gerrold's orb (GER-007) — the 4 cracks

Per Focus 02 § 3.2, GER-007 has 4 cracks across 3 crack-locations:

```
                              ╔═════════════════╗
                              ║                 ║
                              ║      THE ORB    ║  (top view)
                              ║                 ║
                              ║   ╱╲       ╱╲   ║  ← cracks at "the bedside table"
                              ║  ╱  ╲     ╱  ╲  ║     (2 short cracks; lighter)
                              ║                 ║
                              ║                 ║
                              ║   ┌───────┐     ║
                              ║   │  CORE │     ║  ← amber-glowing center region
                              ║   │       │     ║     (Margery + the morning)
                              ║   └───────┘     ║
                              ║                 ║
                              ║     ╱─╱─╱       ║  ← cracks at "Doris in kitchen"
                              ║                 ║     (1 medium crack)
                              ║                 ║
                              ║    ╲─╲─╲─╲      ║  ← cracks at "the seventh morning"
                              ║                 ║     (2 deep cracks — the hardest)
                              ╚═════════════════╝
```

The crack pattern is **deterministic for GER-007** (not procedural). The Cleanse tutorial is a known-shape puzzle. The player learns *how to Cleanse* on a memory whose crack pattern was hand-designed for the tutorial.

### 3.4 The Cleanse interaction model

| Step | Player action | Shader state |
|---|---|---|
| 1 | Player clicks/taps the first crack's start point | A glowing dot appears at the start; the cursor is "engaged" |
| 2 | Player drags along the crack | A glowing thread follows the cursor *only* along the crack's path; the crack visibly seals behind the thread |
| 3a | Player releases at the crack's end point | Crack 1 seals. Soft chime. Move to next crack. |
| 3b | Player releases mid-crack | Crack 1 is *partially* sealed. The unsealed portion remains. The player can re-engage and finish. |
| 3c | Player drifts off the crack onto the orb's surface | The thread breaks. The cursor "loses grip." Crack remains. The player can re-try. |
| 3d | **Player drifts into the core amber region** | **Core damage event:** soft chime of regret, Margery's face fades in the orb (visible to the player), the memory's `memory_integrity` drops by 25 points. The orb is now Crossed-Core. |

### 3.5 The four outcome states

| Outcome | Trigger | Player feels |
|---|---|---|
| **Perfect** | All 4 cracks sealed cleanly. 0 core crossings. ~90 seconds. | Triumph, then quiet satisfaction. Gerrold's reaction is the warmest. |
| **Acceptable** | 3 of 4 cracks sealed. 0 core crossings. (1 crack remains partially open.) ~75 seconds. | Mild relief. Gerrold's reaction is gentle. |
| **Sloppy** | 4 of 4 cracks sealed but with 1–2 brief core touches. ~85 seconds. | Concern. The orb's `memory_integrity` is at 80. Gerrold notices something is *slightly* off but cannot articulate it. |
| **Crossed Core** | 3+ core touches OR a sustained core crossing (>1 second inside). | Real loss. Memory Dream 2 plays its "core damaged" variant. Gerrold's reaction is forgiving but the player has *done a thing.* |

The outcome states each map to one of the Cleanse branches in Focus 02 § 7. The Yarn dialogue branches on the global `$cleanse_quality` variable.

### 3.6 Adaptive difficulty (Cleanse)

| Player band | Cleanse behavior |
|---|---|
| **First cleanse ever** (GER-007) | Crack edges are visually *thicker* (more forgiving cursor tolerance). Core region is *smaller* than it will be in M3+. Three retries before any consequence. |
| **Gentle Mode** | Core damage is *recoverable* via a 3-day in-game "memory grief" event — Gerrold's Memory Integrity automatically rises 10 points per day after the cleanse. |
| **Subsequent cleanses** | Adaptive tightening based on player success rate. Default Cleanse is what M1-2 ships at; difficulty rises later. |

### 3.7 Failure semantics (Cleanse — first stakes)

Unlike Polish, Cleanse **can have a permanent outcome.** A Crossed-Core Cleanse changes the memory permanently. **But the cozy contract holds:**

1. Crossed-Core is *not* a game-over.
2. The memory still works — it is just *less clear.*
3. The narrative absorbs the outcome (Focus 02 § 7.1).
4. The Mission 6+ recovery arc exists (Codex 03 § 5 — Marin's healing techniques).
5. **The player is never told they "failed."** The system never displays "Failure." It displays only what *happened.* Gerrold's reaction is the feedback.

This is the Avellan/Pao discipline: tension without punishment (Codex 05 § 10).

### 3.8 The Auto-Complete toggle for Cleanse

Identical UI placement to Polish. Label: *"Cleanse for me."*

Auto-complete outcome:
- At Default difficulty: Acceptable Cleanse (3 of 4 cracks sealed).
- At Gentle Mode: Perfect Cleanse.
- No bonus for manual.

**The cozy player who Auto-Completes Cleanse still experiences the consequence of their choice (Erase / Cleanse / Listen / Defer) fully.** The choice carries weight regardless of skill.

### 3.9 Audio cues for Cleanse (10 specific sounds)

| Cue ID | When | File length | Volume |
|---|---|---|---|
| `cleanse_hum_start` | When player engages with the orb (different from polish_hum) | 0.8s | 0.4 |
| `cleanse_hum_dissonant_loop` | While cracks remain | loop 8s | 0.45 (dissonant) |
| `cleanse_thread_start` | Cursor engages first crack | 0.4s | 0.6 |
| `cleanse_thread_loop` | While actively sealing a crack | loop 1.2s | 0.5 |
| `cleanse_crack_seal_complete` | Each crack finishes | 0.7s | 0.65 |
| `cleanse_thread_break` | Cursor drifts off crack | 0.3s | 0.5 |
| `cleanse_core_warn` | Cursor enters the core's *outer aura* (the warning zone) | 0.5s | 0.7 (audible, urgent) |
| `cleanse_core_damage` | Cursor crosses into core proper | 1.4s — the *chime of regret* | 0.85 (the loudest single SFX in the game so far) |
| `cleanse_complete_perfect` | All 4 cracks sealed, no damage | 2.0s (composer cue: Margery motif) | 0.75 |
| `cleanse_complete_acceptable` | 3 cracks sealed, no damage | 1.4s (slightly muted Margery motif) | 0.65 |
| `cleanse_complete_crossed` | Player Crossed-Core | 3.0s (composer cue: a hesitant minor variation of Margery motif, ending on an unresolved chord) | 0.7 |

(Eleven cues; the Asset Analysis estimate of 8 for Cleanse was conservative.)

### 3.10 The Mariska variant — **DEFERRED**

Per Codex 13 § 5.2, Mariska can perform Cleanses *for* the player at the workbench. This is **DEFERRED**. Mariska is not in Mission 1-2.

### 3.11 Cleanse's tutorial-in-fiction

The Cleanse tutorial is taught **by Doris in Mission 1's after-polish dialogue + Marin's note above the workbench, expanded.**

The pinned note, examined again on Mission 2 morning, now reveals more text (it was always there; the player just hadn't looked closely):

> *"Polish in slow circles. The faster you press, the less the orb hears you. Cover all sides. Most memories need ninety seconds. — M."*
>
> *"To Cleanse: find the cracks. Trace them once, fully, without lifting your hand. Never touch the warm center. The warm center is the memory itself. If you touch it, you have not cleansed; you have erased. — M."*

The second paragraph **becomes visible** when the player is holding a cracked orb. This is a small Codex-magic effect — the note seems to write itself when the player needs it.

If the player struggles for >45 seconds during a Cleanse without sealing a crack, **Pickle offers a single hint**:

> *(Pickle, from the windowsill)* *"Trace the line of the crack itself. Not the orb around it."*

That is the only hint in the Cleanse tutorial.

### 3.12 Mariska's gentle hand — **DEFERRED placeholder**

The C# code reserves the hook for Mariska's Cleanse-for-the-player mechanic. In Mission 1-2 the hook is unused. The architectural seam is the Krieg Discipline (Focus 00 § 5).

---

## 4.0 Combined Mini-Game KPIs (Mission 1-2 specific)

These KPIs override Codex 13 § 11's longer-term targets and are what the Mission 1-2 vertical-slice playtest measures:

| KPI | Target | Why |
|---|---|---|
| Polish completion (median) | 70–90 seconds | The "felt ritual" range |
| Polish Auto-Complete adoption | 12–22% | Lower than the long-term 35–55% — first-time players want to *try* |
| Polish failure-state encounter rate | 0% | M1 Polish cannot fail |
| Cleanse completion (median) | 70–100 seconds | Longer than Polish; the puzzle has thinking time |
| Cleanse Auto-Complete adoption | 20–30% | Higher than Polish because the stakes are visible |
| Cleanse Crossed-Core rate (Default difficulty) | 8–14% | This is the "real failure" rate; the narrative absorbs it |
| Cleanse Crossed-Core rate (Gentle Mode) | <3% | Generous tolerances in Gentle Mode |
| "Mini-game felt good" mentions in playtest survey | >75% | The cozy ASMR-tier discipline test |
| "Mini-game frustrated me" mentions | <10% | The Doyne / Pao discipline test |
| Playtester who Polishes manually then Auto-Completes Cleanse | 22% (model) | The expected pattern — they enjoy Polish, the moral weight makes Cleanse harder, they opt out of the difficulty but keep the choice |

---

## 5.0 Production Cost — the Two Mini-Games' Full Build

| Item | Hours |
|---|---|
| **Polish** — Shader Graph authoring (`MemoryOrb_Master`) | 14 |
| **Polish** — Cursor input + state machine | 12 |
| **Polish** — Audio integration (9 cues) | 6 |
| **Polish** — Lumen glow + VoluSmokeFX particle | 5 |
| **Polish** — Auto-Complete cinematic (12-second sped-up animation) | 8 |
| **Polish** — Adaptive difficulty curves | 6 |
| **Polish** — Pickle commentary trigger | 2 |
| **Polish** — QA + tuning pass | 12 |
| **Cleanse** — Shader Graph crack-trace + core region | 18 |
| **Cleanse** — Cursor tracking + thread-follow logic | 16 |
| **Cleanse** — Audio integration (11 cues) | 8 |
| **Cleanse** — Outcome state machine (Perfect/Acceptable/Sloppy/Crossed) | 14 |
| **Cleanse** — Crossed-Core 4-second cinematic (Cutscene Engine) | 12 |
| **Cleanse** — Auto-Complete cinematic | 8 |
| **Cleanse** — Adaptive difficulty curves | 8 |
| **Cleanse** — QA + tuning pass | 16 |
| **Combined** — Memory Card SO integration | 6 |
| **Combined** — Playtest iteration (3 rounds) | 24 |
| **Total** | **~195 hours (~5 dev-weeks)** |

This is on schedule for the Mission 1-2 build timeline.

---

## 6.0 The Pao Five-Test Checklist (applied to both mini-games)

Per Codex 13 § 10:

| Test | Polish (M1) | Cleanse (M2) |
|---|---|---|
| 1. No mini-game shall lock progression on failure | ✓ Polish cannot fail | ✓ Cleanse failures are narratively absorbed, never block |
| 2. No reaction time below 350ms at default | ✓ Polish has no timer | ✓ Cleanse has no real-time pressure (only the player's own pace) |
| 3. Playable one-handed at default | ✓ Mouse or thumbstick only | ✓ Mouse or thumbstick only |
| 4. Audio is optional | ✓ Audio off doesn't break Polish | ✓ Audio off doesn't break Cleanse (the visual core-warning still triggers) |
| 5. Tutorial-in-fiction | ✓ Marin's note + Doris's voice | ✓ Marin's expanded note + Pickle's optional hint |

**Both mini-games pass all five.** ✓

---

## 7.0 The Krieg Architectural Hook — Cleanse for Mission 3+

The Cleanse code reserves clean hooks for:

| Hook | Used in M1-2? | When activated |
|---|---|---|
| `CleanseDifficultyProfile` ScriptableObject | yes (single tutorial profile) | M3+ adds 8 more profiles for harder memories |
| `MariskaAssistant.cs` interface | no (stub) | M9+ unlocks Mariska's instructed Cleanses |
| `CleanseTimer.cs` (Restoration Race) | no (stub) | M11+ unlocks Restoration Race (Codex 05 § 5) |
| `CleanseEventBus` for Pickle commentary | yes (1 hint) | M3+ expands Pickle's library to 12 Cleanse-context quotes |

This is the Krieg discipline. The bones extend. The flesh waits.

---

## 8.0 Closing

Two mini-games. One warm, one weighted. Both clearable in 90 seconds. Both Auto-Completable. Both narratively absorbed when they go wrong.

The cozy player at the workbench, polishing Doris's morning, will not be able to articulate why the moment is good. The cozy player tracing Gerrold's cracks, three minutes before the choice, will feel the orb resist them and know the game is asking them to be careful.

Those two moments — the warmth and the weight — are the entire mechanical surface of Mission 1-2. Everything else (Weave, Sever, Listen, Read, Translate, Identify, Compose, Search, Negotiate, Compose Verse, the 8 set-piece mini-games) is Scaling Reference.

— *Linnet Pao*
*Mission 1-2 Focus 04 · v1.0*

> Next: `05_DREAM1_AND_DREAM2.md` — the two Memory Dreams as Cutscene Engine timelines.
