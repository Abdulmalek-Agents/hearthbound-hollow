# Mission 1-2 · Focus 05 — **Memory Dream 1 + Memory Dream 2**
### The two Memory Dreams shipping in Mission 1-2 · Cutscene Engine timelines · Choice-driven Dream 2 variants
### Owner: Sven Aleko (Memory Dream Director)

> *Two dreams. Two illustrated short films. The first one is **warm** (~60 seconds, Doris's First Loaves). The second one **branches** based on the player's Mission 2 choice (~75–90 seconds; four narrative variants sharing assets). Total Memory Dream production for Mission 1-2: **~$48k** of art + audio. This is **<13% of the long-term $403k Memory Dream art budget** in Codex 11 — exactly proportional to the ~12% of total game time Mission 1-2 represents.*

---

## 1.0 The Mission 1-2 Memory Dream Scope

Per Codex 11's parameterized engine + Focus 00's re-scoping, Mission 1-2 ships:

| Feature | Scope in Mission 1-2 | Cost in M1-2 budget |
|---|---|---|
| Set-piece scenes used | **2 of 30** (kitchen at first light, bedroom of a sick person) | ~$12k |
| Emotion lenses used | **3 of 9** (JOY, GRIEF, GRACE) | ~$8k |
| Mood lightings used | **2 of 5** (morning, dim-amber-evening) | ~$10k |
| Cast figures | **3 sprites** (young Doris age 24, Margery, Gerrold) | ~$6k |
| Audio beds | **2** (Doris motif, Margery motif) | (included in $120k composer envelope) |
| Long Dreams | **0** (Long Dreams begin Mission 5+) | $0 |
| Dream Cinema | **0** (requires Hollow Level 9, deferred) | $0 |
| Dream Duels | **0** (deferred) | $0 |
| **Total Mission 1-2 Memory Dream art** | | **~$48k** |

This is exactly enough to deliver two **fully felt** dreams without overbuilding the engine that the rest of the game will inherit.

---

## 2.0 Memory Dream 1 — *The First Loaves* (Mission 1)

### 2.1 Specification

| Field | Value |
|---|---|
| **Triggered by** | Player sleeps at the end of Mission 1 Day 1 |
| **Source memory** | `DOR-001 — The First Loaves` (Focus 01 § 3.1) |
| **Set-piece scene** | #18 — A Kitchen at First Light (Codex 11 § 3) |
| **Emotion lens** | JOY (primary) + GRACE (secondary tint) |
| **Mood lighting** | Morning rig (pale gold, low warmth) |
| **Cast figures** | Doris-young (age 24), alone |
| **Duration** | ~60 real-time seconds |
| **Player interactivity** | None. **Witness mode only.** (No Dream Duel in M1-2.) |
| **Variants** | 1 (the single canonical dream) |
| **Skippable** | Yes (one button; long-press to confirm) |
| **Lingerable** | Yes (long-press to play in slow motion; +50% length) |

### 2.2 The Cutscene Engine timeline (full 60-second breakdown)

```
TIMELINE: Memory_Dream_01_FirstLoaves
Total duration: 60.0 seconds

  Track 1 (Background plate):
    0.0s — 60.0s: SetPiece_18_KitchenAtFirstLight.png (hand-painted, 4-layer parallax)

  Track 2 (Lens overlay):
    0.0s — 60.0s: Lens_JOY.shader (color-grade LUT)
    0.0s — 3.0s:  fade in from black
    55.0s — 60.0s: fade out to amber

  Track 3 (Cast figure — young Doris):
    0.0s — 8.0s:   not visible
    8.0s — 12.0s:  fade in (off-camera, standing at oven, back to viewer)
    12.0s — 20.0s: ACS animation: "checking oven through slot"
    20.0s — 30.0s: ACS animation: "pulling loaves out, setting on rack"
    30.0s — 40.0s: ACS animation: "wiping hands on apron, looking at door"
    40.0s — 55.0s: ACS animation: "standing still, watching the door, alone"
    55.0s — 60.0s: subtle smile begins to form

  Track 4 (Particle systems):
    0.0s — 60.0s:  Dust motes (Lens_JOY's particle system, low density)
    25.0s — 35.0s: Steam from the loaves (VoluSmokeFX micro wisp, low intensity)
    40.0s — 55.0s: Flour dust visible in the low sun shaft (Lumen god ray + extra particles)

  Track 5 (Camera — Cinemachine):
    0.0s — 8.0s:   wide establishing shot, slow drift inward
    8.0s — 25.0s:  medium shot, Doris's back at the oven
    25.0s — 40.0s: closer shot, Doris's hands as she sets the loaves down
    40.0s — 55.0s: pull back to wide; Doris small in the middle of her bakery; the door visible
    55.0s — 60.0s: hold

  Track 6 (Audio — composer):
    0.0s — 3.0s:   silence (the previous scene's audio fades)
    3.0s — 15.0s:  Doris motif, solo cello, sparse — Iva's commissioned cue D-MD01-A
    15.0s — 35.0s: motif develops, light piano enters
    35.0s — 55.0s: motif simplifies again, single cello, warmer
    55.0s — 60.0s: final unresolved-but-warm sustain

  Track 7 (Foley):
    8.0s — 25.0s:  oven hum (low frequency, ambient)
    20.0s — 30.0s: bread crust crackle (subtle, satisfying)
    30.0s — 40.0s: apron wipe (cloth-on-cloth)
    50.0s — 55.0s: one bird outside (distant)

  Track 8 (Text overlay — the dream's title):
    2.0s — 7.0s:   "The First Loaves" (Bamao parchment frame, small, lower-third)
    52.0s — 57.0s: "— 1972" (small, lower-third)
```

This is a **fully specified Cutscene Engine timeline**. The implementation team drops this into the Cutscene Engine asset (S-7), populates the asset references, and the dream is built. The senior cinematic designer's hands-on time: **~12 hours of timeline authoring + 6 hours of polish.**

### 2.3 The single illustrated frame

The Set-Piece scene #18 (Kitchen at First Light) is the **single hand-painted illustration** that anchors this dream. Specifications:

| Aspect | Specification |
|---|---|
| Dimensions | 3840 × 2160 (4K), but optimized for 1920×1080 final render |
| Style | Hand-painted, Spiritfarer-adjacent (warm, slightly oversaturated golds) |
| Layers (for parallax) | 4 (back wall + window, middle ground with oven, foreground with Doris's table, depth-of-field bloom layer) |
| Reusability | Will be lensed JOY for Doris's wedding-honey dream (M3+), and lensed GRIEF for the predecessor's last-morning dream (M11+) |
| Cost | $5,500 (~80 hours of senior illustrator time at $70/hr) |

This single illustration is **used in at least 3 dreams** across the full game's lifespan. M1-2 pays for it; M3+ benefits.

### 2.4 The composer cue D-MD01-A (Doris motif)

| Aspect | Specification |
|---|---|
| Length | 60 seconds, full piece |
| Tempo | 64 BPM (slow) |
| Key | F major |
| Instrumentation | Solo cello (primary) + soft piano (entering at 0:15) + light upright bass (entering at 0:25) |
| Theme | A 5-note motif (the universal Hearthbound motif from Codex 14 § 7) played in F major as a *first statement.* |
| Cost | ~$3,200 (part of the $120k composer envelope; this is dream cue 1 of 9) |

Iva's brief to the composer: *"This is the cozy player's first impression of the game's emotional vocabulary. The whole bible-of-feeling that this game commands is in these 60 seconds. Do not over-arrange. The cello is alone for the first 15 seconds. Trust the silence. — IS"*

### 2.5 What the player feels

The cozy player has just polished an orb for the first time. They have set it on a shelf. They have closed an Evening Ledger. They have gone to bed. The screen has faded to amber.

Then a kitchen appears. It is 1972. A young woman is standing at an oven. They watch her for sixty seconds. She is alone. She is afraid. She is doing the thing she has always wanted to do, for the first time. She pulls the loaves out and they are golden. She does not yet smile. She wipes her hands on her apron. The door does not open. But it will.

The screen fades out.

The cozy player goes back to bed in-game.

In the morning, when they wake, **Doris is in the bakery kneading more dough.** The connection is unspoken. The player feels they have just been *given* something.

That is the entire Mission 1 emotional payoff. **Sixty seconds of illustrated quiet.** It is the cozy game's single most important moment.

---

## 3.0 Memory Dream 2 — *The Widower's Choice* (Mission 2)

### 3.1 Specification

| Field | Value |
|---|---|
| **Triggered by** | Player sleeps after the Mission 2 moral choice (whichever choice they made) |
| **Source memory** | `GER-007 — The Last Week` (Focus 02 § 3.2) — but **lensed by player choice** |
| **Set-piece scene** | #14 — A Bedroom of a Sick Person (Codex 11 § 3) |
| **Emotion lens** | Varies by choice (see § 3.3 below) |
| **Mood lighting** | Dim-amber-evening rig |
| **Cast figures** | Gerrold (older, age 60) + Margery (off-screen presence in most variants) |
| **Duration** | 75–90 seconds depending on variant |
| **Player interactivity** | None. Witness mode. *(Dream Duels DEFERRED.)* |
| **Variants** | **4** (Erase / Cleanse-Perfect / Cleanse-Crossed / Listen) |
| **Skippable** | Yes |
| **Lingerable** | Yes |

### 3.2 The single set-piece, lensed four ways

Set-Piece scene #14 — *"A Bedroom of a Sick Person"* — is hand-painted once and **never re-painted.** All 4 variants of Memory Dream 2 use the same illustration, with different emotion lenses + cast-figure visibility + audio bed + camera blocking.

This is the **Codex 11 § 2 efficiency** in concrete demonstration. One painting → four perceived dreams.

Painting specifications:

| Aspect | Specification |
|---|---|
| Dimensions | 3840 × 2160 |
| Style | Same hand-painted register as Set-Piece #18 |
| Layers | 5 (window with snow outside, back wall with framed photograph, mid-ground with bedside table and lamp, foreground with the bed [empty space where Margery's outline goes], depth-of-field bloom) |
| Notable detail | The bedside table has a single folded white handkerchief on it — *the very handkerchief Gerrold carries in M2.* The cozy cross-reference. |
| Reusability | Used again in M5 (Gerrold's grief continues), M8 (the recovery arc resolves), and once in the long-form Marin DLC | 
| Cost | $5,800 (~85 hours of senior illustrator time) |

### 3.3 The 4 variants of Memory Dream 2

| Variant | Lens | Margery visible? | Length | Composer cue |
|---|---|---|---|---|
| **A — Cleanse Perfect** (the canonical) | GRIEF (primary) + GRACE (secondary, warm) | Yes — clearly. Her face is visible, sitting up against pillows, holding the handkerchief, smiling at it. | 85s | M-MD02-A (Margery motif, full warmth) |
| **B — Cleanse Acceptable / Sloppy** | GRIEF (primary) + GRACE (faint) + SHAME (trace) | Yes, but her face is *slightly* softened by lens-blur. Her smile is visible. | 80s | M-MD02-B (Margery motif, slightly hesitant) |
| **C — Cleanse Crossed-Core** | GRIEF (heavy) + GRACE (sparse) | **Margery's face is faded — the player can see her shape, the handkerchief, the smile, but not the features clearly.** | 90s | M-MD02-C (Margery motif in minor, ending on an unresolved chord) |
| **D — Refused / Listen** | GRIEF (gentle) + GRACE (full) + WONDER (a soft trace, unique to this variant) | The dream is NOT of Margery directly. It is of **Gerrold telling the player the memory, in real-time**, seated in his chair. Margery is *not* in the dream. *He is.* The dream is of *being listened to.* | 75s | M-MD02-D (Margery motif transposed for solo cello, no other instruments; the most spare cue in the game) |
| **E — Deferred** | GRACE only | The dream is of an empty chair beside an empty bed. Nothing else. ~30 seconds, holds on the empty chair. | 30s | M-MD02-E (a single sustained note, slowly fading) |

**Total variants: 5.** (The Deferred variant is the lightest.) Each is a distinct emotional payoff for the player's choice.

### 3.4 Variant A — Cleanse Perfect — Cutscene Engine timeline

```
TIMELINE: Memory_Dream_02_Variant_A_CleansePerfect
Total duration: 85.0 seconds

  Track 1 (Background plate):
    0.0s — 85.0s: SetPiece_14_BedroomOfSickPerson.png (5-layer parallax)

  Track 2 (Lens overlay):
    0.0s — 85.0s: Lens_GRIEF_with_GRACE_blend.shader

  Track 3 (Cast figure — Margery):
    0.0s — 5.0s:   not visible
    5.0s — 10.0s:  fade in, sitting up in the bed, soft-focused
    10.0s — 25.0s: picks up the handkerchief from the bedside table
    25.0s — 40.0s: looks at the embroidery on the corner
    40.0s — 55.0s: small slow smile begins
    55.0s — 70.0s: holds the handkerchief, eyes closing softly (falling asleep)
    70.0s — 80.0s: at peace, in the chair (or pillow); the handkerchief is in her hand
    80.0s — 85.0s: held; the camera pulls slowly back

  Track 4 (Cast figure — Gerrold, brief):
    50.0s — 55.0s: appears briefly in the doorway (silhouette only), nods, exits
    (His brief appearance is the warm beat — he was there for her, and the dream remembers it.)

  Track 5 (Particle systems):
    0.0s — 85.0s: Snow falling gently outside the window (Stylized Weather System particles)
    25.0s — 50.0s: Slow dust motes in the lamp light (Lens_GRACE particle layer)

  Track 6 (Camera — Cinemachine):
    0.0s — 10.0s:  wide establishing, slow drift toward the bed
    10.0s — 40.0s: medium-close on Margery's hands and the handkerchief
    40.0s — 55.0s: extreme close on the embroidered "M" in the corner of the handkerchief
    55.0s — 80.0s: pull out to medium; Margery at peace
    80.0s — 85.0s: pull to wide; the bedroom held; the snow outside

  Track 7 (Audio — composer cue M-MD02-A):
    0.0s — 5.0s:   silence + faint wind outside
    5.0s — 25.0s:  Margery motif begins, solo violin
    25.0s — 40.0s: motif gentles; piano enters
    40.0s — 70.0s: motif resolves to its warmest statement; light cello underpinning
    70.0s — 85.0s: motif simplifies; ends on a held, warm major chord

  Track 8 (Foley):
    0.0s — 85.0s: faint wall-clock tick (off-frequency from the village clock)
    5.0s — 10.0s: bed-sheet shift (subtle)
    10.0s — 15.0s: handkerchief unfold (cloth foley)
    50.0s — 55.0s: a wood-floor creak (Gerrold's footstep — never said aloud)

  Track 9 (Text overlay):
    2.0s — 7.0s:   "The Last Week" (Bamao parchment, lower-third)
    78.0s — 83.0s: "— Saltlight, last winter" (small)
```

This is the canonical Mission 2 dream timeline. The implementation specifies the Cutscene Engine asset to drop in.

### 3.5 Variant C — Cleanse Crossed-Core — the difference

To show how minimally the variants differ, here is what changes from Variant A:

| Track | Variant A | Variant C (Crossed-Core) |
|---|---|---|
| Track 2 (Lens) | GRIEF + GRACE blend | GRIEF + traces of GRACE — *the lens is heavier* |
| Track 3 (Margery) | Fully visible, face clear, smile clear | **Face is fog-blurred. Smile barely readable. The handkerchief is clear; she is not.** |
| Track 4 (Gerrold doorway) | Silhouette nods and exits | Silhouette *lingers* longer, looking in. He does not nod. |
| Track 6 (Camera) | Extreme close on the embroidered M (40-55s) | Extreme close on the *handkerchief, not on Margery's face* (40-55s). The camera is choosing to *protect* the player from her absence. |
| Track 7 (Composer cue) | M-MD02-A (warm resolution) | M-MD02-C (the same melody, but ending on an unresolved minor seventh) |

That is the **entire difference between Variant A and Variant C.** No new art assets. Different lens, different camera blocking, different audio cue. The Cutscene Engine swaps these three tracks based on the global `$cleanse_quality` variable.

**Variant authoring cost: ~$1,600 each beyond Variant A** (the audio cue is the main expense; the lens and camera changes are timeline-edits).

### 3.6 Variant D — Refused / Listen — the unique variant

The Listen-path Dream 2 is the most narratively different. Instead of dreaming Margery's last week, the player dreams **Gerrold's telling of it.**

| Aspect | Variant D specification |
|---|---|
| Set-piece used | **NOT #14.** Instead, a *small subset* of Set-Piece #14 is used — just Gerrold's chair, alone, in the empty bedroom. The bed is mostly off-frame. |
| Camera | Tight on Gerrold's chair. Then on his hands. Then on his face. *Margery is never visible.* |
| Cast figure | Gerrold only |
| Lens | GRIEF (gentle) + GRACE (full) + WONDER (a soft golden trace — the wonder of being heard) |
| Composer cue | M-MD02-D — solo cello, no other instruments, the most spare cue in the entire game |
| Length | 75s |
| Final beat | Gerrold's eyes close softly, briefly. Open again. He looks directly at the camera — at the player, who is dreaming him. **One small smile.** Held for 4 seconds. Cut to black. |

The Listen variant is the cozy game's **first moment of the player being directly thanked.** It is the design's strongest argument for choosing the Listen path. It is also a relatively cheap variant: it reuses the chair element from Set-Piece #14, requires only the Gerrold sprite (already authored for the village scene), and uses one composer cue.

**Variant D extra cost: ~$2,400** (mostly the composer cue + camera authoring).

### 3.7 Variant E — Deferred — the lightest

If the player Deferred (option D in the choice tree), Memory Dream 2 plays a **30-second held shot** of an empty chair beside an empty bed. The lens is GRACE only. No music after the first 6 seconds — just silence and the wind. No cast figures.

This is the cozy game's **most restrained dream** — the dream of *not knowing yet.* It is appropriate to the player's choice to wait.

**Variant E extra cost: ~$600** (essentially just timeline authoring + a single audio cue).

### 3.8 Memory Dream 2 — total cost across all 5 variants

| Cost item | Subtotal |
|---|---|
| Set-Piece #14 hand-painted illustration (one-time) | $5,800 |
| GRIEF + GRACE + WONDER lens authoring (one-time; shared across all variants) | $4,500 |
| Cast figure: Margery sprite (M-1 specific; reused in M5+M8) | $3,200 |
| Cast figure: Gerrold sprite — *shared with village scene*, no extra cost | $0 (shared) |
| Composer cues M-MD02-A through E (5 cues) | $9,200 |
| Cutscene Engine timeline authoring (5 timelines × ~6 hours each) | ~$6,800 |
| Foley + Wwise integration | $1,800 |
| QA + variant testing (each variant validated) | ~$3,200 |
| **Total Memory Dream 2 production cost** | **~$34,500** |

### 3.9 The variant-switching code

The Mission 2 dream's variant is determined by a single ScriptableObject reference + a switch statement in `MemoryDream2_Sequencer.cs`:

```csharp
public class MemoryDream2_Sequencer : MonoBehaviour {
    [SerializeField] private CutsceneEngineTimeline variantA_CleansePerfect;
    [SerializeField] private CutsceneEngineTimeline variantB_CleanseAcceptable;
    [SerializeField] private CutsceneEngineTimeline variantC_CleanseCrossed;
    [SerializeField] private CutsceneEngineTimeline variantD_Listen;
    [SerializeField] private CutsceneEngineTimeline variantE_Deferred;

    public void TriggerDream(VillageStateService state) {
        var choice = state.GetVariable("gerrold_choice");
        var quality = state.GetVariable("cleanse_quality"); // may be null if not cleansed

        CutsceneEngineTimeline timeline = choice switch {
            "erase"   when quality is "perfect" or "acceptable" => variantA_CleansePerfect,
            "erase"   when quality is "crossed_core"             => variantC_CleanseCrossed,
            "cleanse" when quality is "perfect"                  => variantA_CleansePerfect,
            "cleanse" when quality is "acceptable" or "sloppy"   => variantB_CleanseAcceptable,
            "cleanse" when quality is "crossed_core"             => variantC_CleanseCrossed,
            "listen"                                             => variantD_Listen,
            "defer"                                              => variantE_Deferred,
            _                                                    => variantE_Deferred
        };

        CutsceneEngine.Play(timeline);
    }
}
```

Simple, deterministic, single-point-of-decision. The senior engineer can implement and test this in **~6 hours.**

---

## 4.0 The Sleep Transition (shared between both dreams)

Both Memory Dreams are entered through a **2.5-second sleep transition** sequence:

| Time | What plays |
|---|---|
| 0.0s | Player closes the Evening Ledger. The book gently closes audibly. |
| 0.2s | The shop's audio softens to ~30%. |
| 0.5s | Camera tilts upward toward the Hollow's ceiling, finding a single beam of moonlight. |
| 0.9s | Pickle stretches and curls; visible. |
| 1.2s | Screen begins fading to warm amber. |
| 1.8s | Audio fully silent. Composer cue begins (whichever dream is queued). |
| 2.5s | Amber fade completes; dream's first frame appears. |

This **shared sleep transition** is itself a small piece of asset, used for both dreams and every future sleep in the game. **Cost: ~$1,800.** **Used: ~80 times per playthrough.** Extreme efficiency.

---

## 5.0 The Morning Wake (shared)

Both dreams end with a **3.0-second wake transition**:

| Time | What plays |
|---|---|
| 0.0s | Dream's final frame holds. |
| 0.5s | Screen begins fading to bright morning gold. |
| 1.4s | A single bird outside the Hollow's window. |
| 1.8s | Pickle's stretch + tail-flick. |
| 2.2s | Composer's "morning cue" enters softly (a different cue per day — Day 2's is Doris's motif faintly heard from her bakery; the player will not consciously identify it). |
| 3.0s | The Hollow interior renders fully. Player has control. |

This is the cozy game's **morning ritual.** It plays every in-game morning. **Cost: ~$1,400.**

---

## 6.0 Production Schedule for the Two Dreams

| Asset | Weeks |
|---|---|
| Set-Piece #18 (kitchen at first light) | 2 weeks (senior illustrator) |
| Set-Piece #14 (bedroom of sick person) | 2 weeks (senior illustrator, in parallel) |
| Lenses JOY + GRIEF + GRACE + WONDER (4 master lenses) | 1.5 weeks (tech artist) |
| Mood lighting rigs (morning + dim-amber) | 0.5 weeks (tech artist) |
| Cast figure: young Doris | 0.5 weeks (character artist) |
| Cast figure: Margery | 0.5 weeks (character artist) |
| Cast figure: Gerrold (shared from village scene) | 0 (shared) |
| Composer cues (D-MD01-A + M-MD02-A through E) | 4 weeks (composer, in parallel with art) |
| Cutscene Engine timelines (Dream 1 + 5 Dream 2 variants) | 2 weeks (cinematic designer) |
| Sleep transition + Morning wake | 0.5 weeks (cinematic designer) |
| QA + iteration | 1 week (whole team) |
| **Total elapsed time (parallel work)** | **~6 weeks** |

This fits within Mission 1-2's overall ~4-month build window (the dreams are not on the critical path; villager dialogue and the workbench mini-games are).

---

## 7.0 Why This Works — The Aleko Discipline

> *"Two dreams. One painting per. Five variants of one of them. A single 5-note motif underlying both composer cues so the player feels them harmonize. Sixty hand-illustrated seconds + ninety hand-illustrated seconds = the entire emotional crest of Mission 1-2."*

The 17-codex bible designed a 1,350-dream parameterized engine. **Mission 1-2 uses 6 of those dreams (Dream 1 + 5 variants of Dream 2).** The engine is built once and amortized across the rest of the game. The cozy player at the end of Mission 2 will not know they have seen a parameterized system. They will know they have seen Gerrold's last week and chosen what to do with it. And they will remember.

That memory is what brings them back for Mission 3.

— *Sven Aleko*
*Mission 1-2 Focus 05 · v1.0*

> Next: `06_TEA_LOOP_AND_MORAL_CHOICE.md` — the herb-harvest → tea-brew → moral-choice flow.
