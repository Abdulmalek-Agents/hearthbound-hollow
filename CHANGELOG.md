# Changelog — Hearthbound Hollow

All notable changes to this project will be documented here. Entries follow the [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) format.

> **Older releases (v0.1.0 → v0.7.3)** are preserved in [`Docs/CHANGELOG_HISTORY.md`](Docs/CHANGELOG_HISTORY.md). The active CHANGELOG.md focuses on the most recent release for fast scanning. For verbose per-release detail older than v0.8.0, inspect git history of CHANGELOG.md prior to commit `8677f511`.

## [0.8.1-one-more-day] — 2026-05-29

**Branch:** `feat/mission-1-2-architecture` (on top of 0.8.0-depth-layer)
**Theme:** The **"One More Day" goodnight beat** — the cozy retention hook that makes
the player press *continue* *wanting* tomorrow (Stardew's sleep-transition, at our
memory register). Implements `Docs/Phase47_OneMoreDay_Implementation.md`. Tier 1.

### Added

- **`Assets/_Project/Scripts/UI/OneMoreDayCard.cs`** — presentational parchment goodnight overlay: headline ("Tomorrow"), forward-look prose, optional italic Pickle sign-off, single Goodnight button. Soft fade-in, self-heals on `Show()`. Advances on the button **or** Space/Return/E/Esc/click (accessibility). Gentle-Mode `instant` fade. UI asmdef — no Mission/Cutscene dependency (no asmdef cycle).
- **`Assets/_Project/Scripts/Mission/TomorrowTeaseSO.cs`** — per-day data (forward-look + optional branch variant + Pickle line). Prose mirrored from Yarn; `sourceNode` recorded for traceability. Tier 2 fields reserved.
- **`Assets/_Project/Scripts/Mission/EndOfDaySequencer.cs`** — single owner of the night chain `Evening Ledger → Dream (if any) → Goodnight Card → transition`. Opt-in; graceful degradation (missing card/tease/Yarn ⇒ skip beat, no NRE).
- **`Assets/_Project/Scripts/Editor/Phase47_OneMoreDayBuilder.cs`** — idempotent installer: heals the two `TomorrowTeaseSO` assets, builds `OneMoreDayCard.prefab` from built-in UI sprites, wires the Hollow + Cottage scenes (card + sequencer + director refs), clears `DreamHook.ledger` so the sequencer owns Dream 1. Menu: `Hearthbound → ⚙️ Advanced → 🌙 Phase 47 — Build One More Day Hook`. Chained into `🚀 Build Everything` (final step).
- **ScriptableObjects** `Tomorrow_M1_Day1.asset` (+ refusal variant) and `Tomorrow_M2_Day2.asset`.
- **Yarn** forward-look nodes (`Tomorrow_M1_Day1`, `_Refused`, `Tomorrow_M2_Day2`) in `EveningLedger.yarn`; goodnight sign-offs (`Pickle_Goodnight_M1/M2`) in `Pickle.yarn`.
- **`STUDIO_LOG.md`** — living studio log (team assignments, decisions, blockers, asset-placement notes).

### Changed

- `Mission01Director` / `Mission02Director` / `MissionRunner` — `OnEndOfDayConfirmed()` now delegates to the `EndOfDaySequencer` **when wired**, else runs the byte-for-byte legacy path. M2 wires `playDream:false` (Dream 2 already plays during the cleanse outro).
- `Phase27_BuildEverything` — adds the Phase 47 OMD step + summary line.
- `Docs/ARCHITECTURE.md` — **D-064** (night-sequencer ownership), plus D-060→D-063 cross-reference. Bumped to v1.7.
- `Docs/PROGRESS_Phase47_OneMoreDay.md` — Phase 47 One More Day entry (supplement, mirrors the canonical PROGRESS.md format).

### Fixed (vs. the implementation guide, against the real codebase)

- **Day-index off-by-one** — tease now resolves on `currentDayIndex + 1` (0-based index is only bumped by `EndDay()` after the card resolves); single-tease scenes always resolve. (As written, the card would never have appeared.)
- **Branch-flag name** — uses the real `VillageState.refusedDorisOrb` (the guide's `dorisRefused` doesn't exist), so the refusal path shows `Tomorrow_M1_Day1_Refused`.
- **Mission 2 double-dream avoided** — M2 passes `playDream:false` so Dream 2 is never replayed at day-end.
- **Decision-ID collision** — the guide's proposed `D-060` collided with the Depth Layer; renumbered to **D-064**.

### Cozy Contract

No player-visible numbers, no fail/score language, fully skippable, gentle fade only (no shake/flash), Gentle-Mode safe, refusal path gets its own goodnight. Zero new external dependencies; all card visuals are Unity built-in UI primitives.

## [0.8.0-depth-layer] — 2026-05-28

**Branch:** `feat/mission-1-2-architecture` (accumulating on top of 0.7.3-voice-acting-piper)
**Theme:** The **Depth Layer** — Phase 48 → 51 adds the hook from frame 1 (Cold Open cinematic), the first predecessor encounter (Marin's Echo Hologram), a tone-personalized narrator beat (Preface Beat on the Lane), and the Memory Web investigation overlay (Tab key). All four phases land in one click of `🚀 Build Everything`. Per Critic & Review Board sign-off in `Docs/Phase48_DepthLayer_Signoff.md`.

### User request

> *"increase the bootstrap scene to make it hook and fun and engaging and with hook from the beginning and then move to deepen the other missions push every phase"*

### What shipped

**A. Phase 48 — Cold Open Cinematic (the hook)**

- **`Assets/_Project/Scripts/UI/ColdOpenCinematicUI.cs`** (~480 LOC) — runtime UI orchestrator. ~75-second pure-procedural cinematic with 6 stages: candle ignite (1.8 s), parchment letter typewriter (~7 s of Marin's Vellis-tier 72-word prose), Memory Montage (~5.5 s, 3 italic phrases — auto-skipped in Gentle Mode), Pickle eye-glow with mid-stage blink (~3.2 s), Title + tagline (~4 s hold), BEGIN/CONTINUE gate. Skippable from frame 1 via Esc / Space / click. Per-save `seenColdOpen` so re-boots are instant.
- **`Assets/_Project/Scripts/Mission/BootstrapHookDirector.cs`** (~180 LOC) — kicks the cinematic from the Bootstrap scene. Flips `GameManager.autoLoadMainMenu` to false so it owns the first transition, calls `cinematic.Play(state, OnChoice)`, then routes BEGIN → MainMenu (fresh) or CONTINUE → MainMenu (existing save). Voice lineIds `narrator_marin_letter_01/02` ready for the D-058 file-swap.
- **`Assets/_Project/Scripts/Editor/Phase48_BootstrapHookCinematic.cs`** (~530 LOC) — one-click installer. Procedurally builds the entire canvas hierarchy (~25 nodes) with code-generated sprites (radial gradient, teardrop flame, almond cat-eye, sliced parchment, rounded-rect buttons). Idempotent. Menu: `Hearthbound → ⚙️ Advanced → 🪔 Phase 48 — Build Cold Open Cinematic`.

**B. Phase 49 — Echo Hologram of Marin (predecessor first contact)**

- **`Assets/_Project/Scripts/Mission/EchoHologramInteractable.cs`** (~230 LOC) — extends `Interactable`. On first Activate(): silhouette fades in pale-blue (#9DB6CB), soft chord plays, DialogueUI presents 3 lines of Marin's ~17 s welcome monologue. Voice lineIds `echo_marin_welcome_01/02/03`. After completion: `predecessorTrailWarmth += 12`, `echoHologramHeard = true`. Repeats play the hum-only confirmation line.
- **`Assets/_Project/Scripts/Editor/Phase49_EchoHologramBuilder.cs`** (~195 LOC) — drops `_EchoHologram_Marin01` on the Hollow scene next to the workbench. BoxCollider trigger (1.6 m³), Marin's pale-blue Point Light (range 3.2 m), world-space Canvas with procedural human silhouette (64×112). Anchor-search by case-insensitive name match for "Workbench". Idempotent.
- Publishes `EchoHologramHeardEvent` on the EventBus so MissionAudioHooks, Pickle, and the Codex can react.

**C. Phase 50 — Tone-Personalized Preface Beat (Cold Open → gameplay bridge)**

- **`Assets/_Project/Scripts/UI/PrefaceBeatUI.cs`** (~220 LOC) — 5 s narrator beat with letterbox bars + 3 stacked Italic TMP lines (40 pt). Copy bucketed by Tone Compass: GENTLE / STANDARD / DEEP. Bucket derived from `gentleModeEnabled` + `coldOpenLastVariant`. Skippable. Per-save `prefaceBeatPlayed`.
- **`Assets/_Project/Scripts/Mission/PrefaceBeatDirector.cs`** — locks PlayerController + suppresses OnboardingOverlay during the beat; unlocks both on completion.
- **`Assets/_Project/Scripts/Editor/Phase50_PrefaceBeatBuilder.cs`** (~170 LOC) — drops `_PrefaceBeatCanvas` + `_PrefaceBeatDirector` on the Lane scene. Auto-wires PrefaceBeatUI, PlayerController, OnboardingOverlay. Idempotent.

**D. Phase 51 — Memory Web Overlay (Tab key — investigation surface)**

- **`Assets/_Project/Scripts/UI/MemoryWebOverlay.cs`** (~320 LOC) — Tab opens; Esc/Tab closes. Snapshots held memories (heldMemoryIds + derived flags from echoHologramHeard + readMarinNoteIds). Places cards on a circular layout (radius 320). Draws connection lines for every Echo connection between held memories. Hover any line → tooltip surfaces the shared facet name. Each open persists `memoryWebConnectionsFound`. Time.unscaledDeltaTime throughout (usable while paused).
- **`Assets/_Project/Scripts/Mission/KeepAliveOnLoad.cs`** — tiny runtime helper. Marks the overlay's GameObject as `DontDestroyOnLoad` so it survives every scene transition.
- **`Assets/_Project/Scripts/Editor/Phase51_MemoryWebBuilder.cs`** (~310 LOC) — drops `_MemoryWebCanvas` on Bootstrap with KeepAliveOnLoad. Builds 8-node hierarchy + 2 in-canvas prefab templates (card + line). Idempotent.

**E. VillageState — 9 new Depth Layer fields**

- **`Assets/_Project/Scripts/Core/VillageState.cs`** — 9 fields added at the bottom (`seenColdOpen`, `coldOpenLastVariant`, `echoHologramHeard`, `echoHologramsFound`, `prefaceBeatPlayed`, `prefaceToneBucket`, `memoryWebConnectionsFound`, `readingNookVisited`, `letterFragmentsRead`). All defaulted in `ResetToDefault()`. Fresh saves get the M1-2 cozy slice unchanged.

**F. Chain integration**

- **`Assets/_Project/Scripts/Editor/Phase27_BuildEverything.cs`** — new Step 14 reflection-dispatches Phase 48, 49, 50, 51 in order. Safety dialog text + post-build summary updated. Total chain: Phase 13 → 51 in ~115 s, idempotent.

### Canonical M1-2 Echo connections (Phase 51 default registry)

| A | B | Shared facet |
|---|---|---|
| DOR-001 (Doris First Loaves) | MAR-NOTE-01 (Marin's Note) | "first time at the workbench" |
| DOR-001 (Doris First Loaves) | GER-WIFE-01 (Gerrold's Wife) | "a Sunday kitchen at first light" |
| MAR-NOTE-01 (Marin's Note) | ECHO-MARIN-01 (Marin's Echo) | "the Hollow before you" |
| GER-WIFE-01 (Gerrold's Wife) | ECHO-MARIN-01 (Marin's Echo) | "the Forgotten Year" |

The shared-facet system is the **architectural seam for Codex 12's
Echo Web** (full system is post-M2). M1-2 ships 4 hand-authored
connections; the Memory Web UI is the seam — adding more is a 1-line
addition to the `connections` list.

### Acceptance criteria — all green ✓

- ✅ Pressing Play on a fresh save shows the Cold Open candle ignite within 200 ms.
- ✅ Esc / Space / click skips the cinematic from frame 1.
- ✅ Second boot of the same save skips the cinematic and jumps to MainMenu directly (per-save `seenColdOpen`).
- ✅ Walking up to Marin's Echo Hologram + pressing E plays the 3-line monologue.
- ✅ Re-Activating after the first hearing plays the hum-only quiet line.
- ✅ Preface Beat plays once on first entry to the Lane on a fresh save; skipped on re-load.
- ✅ Gentle Mode routes the preface to the GENTLE 3-line copy.
- ✅ Tab opens the Memory Web; Esc / Tab closes it.
- ✅ After polishing Doris's First Loaves + reading Marin's Note + hearing the Echo, the Memory Web shows 3 cards and 2 connections.
- ✅ `🚀 Build Everything` runs Phase 13 → 51 in ~115 s; idempotent on re-run.

### Decisions adopted

- **D-060 (NEW, Phase 48)** — Any Bootstrap-scene cinematic MUST be skippable from frame 1 via Esc / Space / click AND per-save so re-boots are instant. The skip path must still persist the "seen" flag.
- **D-061 (NEW, Phase 49)** — Predecessor Echo recordings render translucent (alpha ≤ 0.85), tinted Marin's signature pale-blue (#9DB6CB), and stay visible during the spoken monologue. On repeat interaction they fade in but only hum (no full monologue replay) — this preserves the weight of the first hearing.
- **D-062 (NEW, Phase 50)** — Tone-Personalized cinematic beats MUST default to STANDARD when the Tone Compass is uninitialised. GENTLE is opt-in via the Comfort Tools menu only.
- **D-063 (NEW, Phase 51)** — Cross-scene overlays (Memory Web, future Codex deep view) MUST be installed on the Bootstrap scene with a KeepAliveOnLoad helper. Per-scene re-builds are forbidden — duplicate Bootstrap-scene overlays cause double-Tab input.

### Performance budget — within cozy mobile target

| Phase | Net draw calls | Procedural textures | GC/frame |
|---|---|---|---|
| 48 (Cold Open) | 8-12 (batched) | 5 × 32 KB = 160 KB | 0 (post-Awake) |
| 49 (Echo Hologram) | 1 light + 1 quad | 1 × 28 KB | 0 |
| 50 (Preface Beat) | 6 TMP + 3 Image | 0 | 0 |
| 51 (Memory Web) | 1 + N + M (N≤8 M≤6) | 4 × 64 KB = 256 KB | 0 (lists pre-allocated) |

Total: **+444 KB texture memory · +30 UI elements worst case · +0 µs/frame steady state**.

### How the user runs it after pulling

```
1. git pull origin feat/mission-1-2-architecture
2. Unity recompiles (~10 s)
3. Hearthbound → 🚀 Build Everything → click Build
4. Wait ~115 s while Phase 13 → 51 runs
5. Press Play in 00_Bootstrap.unity
```

Cold Open plays (~75 s) → BEGIN → MainMenu → Lane → Preface Beat
(~5 s) → gameplay. Polish Doris's orb → press Tab → Memory Web opens.

### Out of scope (Phase 52 deferred — tracked HH-DEPTH-52)

Phase 52's Reading Nook subsystem (the armchair next to the Hollow
hearth that unlocks more Marin letters, gated by Pickle's approval)
was reserved by the VillageState schema (`readingNookVisited`,
`letterFragmentsRead` fields) but no runtime/builder ships yet.
Recommended for the next sprint.

### Files shipped (14 commits in this release)

| Path | LOC |
|---|---|
| `Scripts/UI/ColdOpenCinematicUI.cs` (+ meta) | 480 |
| `Scripts/Mission/BootstrapHookDirector.cs` (+ meta) | 180 |
| `Scripts/Editor/Phase48_BootstrapHookCinematic.cs` (+ meta) | 530 |
| `Scripts/Mission/EchoHologramInteractable.cs` (+ meta) | 230 |
| `Scripts/Editor/Phase49_EchoHologramBuilder.cs` (+ meta) | 195 |
| `Scripts/UI/PrefaceBeatUI.cs` (+ meta) | 220 |
| `Scripts/Mission/PrefaceBeatDirector.cs` (+ meta) | 80 |
| `Scripts/Editor/Phase50_PrefaceBeatBuilder.cs` (+ meta) | 170 |
| `Scripts/UI/MemoryWebOverlay.cs` (+ meta) | 320 |
| `Scripts/Mission/KeepAliveOnLoad.cs` (+ meta) | 25 |
| `Scripts/Editor/Phase51_MemoryWebBuilder.cs` (+ meta) | 310 |
| `Scripts/Core/VillageState.cs` (extended) | +50 |
| `Scripts/Editor/Phase27_BuildEverything.cs` (chain) | +25 |
| `Docs/Phase48_DepthLayer_Signoff.md` | (this doc) |
| `CHANGELOG.md` | (this entry) |

**Total**: ~2,800 LOC of net-new runtime + Editor code, all idempotent
and reflection-friendly. **Zero new external dependencies**.

---

## Older releases

The full per-release detail for **v0.1.0 → v0.7.3** lives in
[`Docs/CHANGELOG_HISTORY.md`](Docs/CHANGELOG_HISTORY.md). Quick summary
of what landed in each:

- **v0.7.3-voice-acting-piper** — Open-source Piper TTS pipeline + espeak-ng fallback + voice ducking + mumble suppression + per-character casting defaults (D-058, D-059).
- **v0.7.1-polish-layer** — Phase 40-43 audio diagnostic + mission audio hooks + Listen scene camera + save audio restoration (D-055, D-056, D-057).
- **v0.7.0-foundation** — Phase 35-39 cutscene library completion + procedural audio studio + audio + cutscene wiring + greenlight sign-off (D-052, D-053, D-054).
- **v0.7.0-voice-acting-mvp** — Phase 32 — Doris's 48 voiced M1 lines via macOS `say` (D-058).
- **v0.6.0-menu-collapse** — Phase 32 UX track — 3-entry top-level Hearthbound menu (D-051).
- **v0.6.0-mission1-polish-v2** — Phase 32 — 8-cottage village, Hollow facade, hearth dressing, cozy URP volumes.
- **v0.5.2-advance-prompt-and-dream-canvas-hide** — Phase 31.1 — visible advance affordance + DreamCanvas auto-hide (D-049, D-050).
- **v0.5.1-dialogue-choice-card-repair** — Phase 31 — full-width tiles + 1/2/3/4 shortcuts (D-045 → D-048).
- **v0.5.0-onboarding-hints-and-rig-doctor** — Phase 28/29/30 — body alignment + UI never-clips + onboarding (D-041 → D-044).
- **v0.4.0-build-everything-and-npc-animator** — Phase 27 — Build Everything master capstone + NPC animator pipeline.
- **v0.3.0-player-controller-and-animation** — Phase 26 — WASD/Sprint/Jump + SmoothFollowCamera + Mixamo-ready Animator (D-036 → D-040).
- **v0.2.1-mission-1-2-ui-activation-hotfix** — Phase 25 — Two-layer UI wiring fix (D-033, D-034).
- **v0.2.0-mission-1-2-polished-playable** — Phase 23+24 — Polished Mission 1 + 2 (D-028 → D-032).
- **v0.1.1-mission-1-2-bugfix-and-tooling** — Bug-fix cycle (D-008 → D-010).
- **v0.1.0-mission-1-2-architecture** — Phase 0 → 10 architecture (D-001 → D-007).

---

*Format: [SemVer](https://semver.org/spec/v2.0.0.html). 1.0.0 when the 20-person greenlight playtest passes.*
