# Hearthbound Hollow — Changelog History (pre-0.8.0)

> All releases v0.1.0 through v0.7.3 (Phase 10 → Phase 47.7) — preserved
> verbatim from the historical CHANGELOG.md prior to the v0.8.0
> Depth-Layer release. The active CHANGELOG.md at the repo root now
> begins at v0.8.0-depth-layer; everything below is the canonical
> archive.

---

## [0.7.0-voice-acting-mvp] — 2026-05-27

**Branch:** `feat/mission-1-2-architecture` (accumulating on top of 0.7.1-polish-layer)
**Theme:** Phase 32 — Voice Acting MVP. Doris's complete Mission 1 dialogue (48 lines) is now AI-voiced via macOS `say -v Samantha -r 180`. Architecture decoupled — ElevenLabs / XTTS / Piper / a human VO actress can swap in later just by overwriting the `.wav` files.

### User request

> *"Add AI-voiced dialogue to Hearthbound Hollow. After the PR lands, the player walks up to Doris, the parchment dialogue box pops up, the typewriter starts, AND Doris's voice plays through the speakers."*

### What shipped

**A. Generation pipeline (1 file)**

- **`Tools/generate_voices.sh`** — macOS-only shell driver that loops over a 49-entry `LINES=(id|text)` array and runs `say -v Samantha -r 180 -o aiff` + `afconvert ... -f WAVE -d LEI16@22050 -c 1` for each. Idempotent: skips clips that already exist. Result: ~48 `.wav` files totalling ~10–20 MB under `Assets/_Project/Audio/Voice/Doris/`. Format: 22 kHz mono PCM16 — fits the Unity native importer with no extra settings.

**B. Runtime (2 new scripts + 1 SO asset + 1 reflection auto-spawn)**

- **`Assets/_Project/Scripts/Audio/VoiceLibrarySO.cs`** — `ScriptableObject` with a `List<Entry { string lineId; AudioClip clip; float volume; float pitch; }>`. Lazy lookup dictionary, invalidated by `OnValidate`. Asmdef: `HearthboundHollow.Audio` (no UI dep).
- **`Assets/_Project/Scripts/Audio/VoicePlayer.cs`** — singleton MonoBehaviour, auto-creates a 2D non-spatial AudioSource on `Awake`, `Resources.Load`s `HearthboundVoiceLibrary` if no inspector reference is wired. `Play(lineId)` returns the clip length (or 0 if no clip); `Stop()` halts immediately.
- **`Assets/_Project/Resources/HearthboundVoiceLibrary.asset`** — initial empty SO at the canonical Resources/ path so `Resources.Load` resolves on a fresh clone.
- **`Assets/_Project/Scripts/Core/GameManager.cs`** — `Awake()` now uses reflection to auto-spawn a `_VoicePlayer` child GameObject if `HearthboundHollow.Audio.VoicePlayer.Instance` is still null after the scene-baked rig + Phase 45's `RuntimeAudioBootstrap` have had a chance to install one. Belt-and-braces. The reflection avoids a Core → Audio asmdef cycle (Audio already references Core).

**C. UI hook (1 modified script + 1 asmdef update)**

- **`Assets/_Project/Scripts/UI/DialogueUI.cs`** — new `PresentLine(speaker, text, portrait, lineId)` overload. When `VoicePlayer.Instance.Play(lineId)` returns a non-zero length, the per-line `charsPerSecond` is locked to `text.Length / clip.length` (clamped 18..90) so the last visible character lands as the voice ends — a real lip-sync feel. `Hide()`, `SkipTypewriter()`, and `PresentChoices()` all call `VoicePlayer.Instance?.Stop()` so a clip never bleeds past a player advance. Backward-compatible: the 3-arg overload still exists.
- **`Assets/_Project/Scripts/UI/HearthboundHollow.UI.asmdef`** — `references` array gains `"HearthboundHollow.Audio"`.

**D. Mission directors (1 modified script — 48 lineIds threaded)**

- **`Assets/_Project/Scripts/Mission/Mission01Director.cs`** — `Line(...)` helper gains an optional `string lineId = null` that's forwarded to `DialogueUI.PresentLine(...lineId)`. Every Doris call now carries its canonical id from the Tools/generate_voices.sh table (e.g. `doris_m1_greet_01`, `doris_m1_memory_02`, `doris_m1_polish_done_01`). 48 total. The 3 refused-path lines and the dynamic `afterPolishLine` deliberately have no lineId — they remain silent on the voice channel and fall through to the legacy typewriter behaviour.

**E. Editor utility + chain integration (1 new script + 2 chain hookups)**

- **`Assets/_Project/Scripts/Editor/Phase32_VoiceLibraryBuilder.cs`** — `Hearthbound → ⚙️ Advanced → 🎙️ Phase 32 — Rebuild Voice Library`. Scans `Assets/_Project/Audio/Voice/**/*.wav` recursively, derives the lineId from each filename, and binds the AudioClip into the SO. Idempotent: preserves inspector-tuned `volume` / `pitch` for entries whose lineId is unchanged; new clips get added with defaults; entries whose .wav was deleted are pruned. Resources/ folder is created on demand. Exposes three entry points: `RebuildFromMenu()` (the menu callback, pops a success dialog), `Build()` (silent — for chain use), `Diagnose()` (read-only audit — for the aggregate diagnostic).
- **`Assets/_Project/Scripts/Editor/Phase27_BuildEverything.cs`** — New Step 8.5 invokes `Phase32_VoiceLibraryBuilder.Build()` so the canonical `🚀 Build Everything` workflow auto-builds the voice library without an extra menu click.
- **`Assets/_Project/Scripts/Editor/Phase33_AggregateDiagnostic.cs`** — New Step 6/6 invokes `Phase32_VoiceLibraryBuilder.Diagnose()` so `🔍 Diagnose Build` surfaces voice-library regressions (orphan entries, missing clips, on-disk-vs-SO mismatches) in the standard one-click audit.

### Voice casting (locked)

| Character | macOS `say` voice | Locale | Status this PR |
|---|---|---|---|
| **Doris** (cozy elderly baker) | `Samantha` | en_US | **48 clips generated by user on macOS** |
| Gerrold (widower) | `Daniel` | en_GB | M2 stub — casting locked, no clips |
| Marin's notes (whispered) | `Tessa` | en_ZA | M2 stub |
| Narrator title cards | `Karen` | en_AU | Memory Dream stub |

### Decisions adopted

- **D-058 (NEW):** Voice clips live under `Assets/_Project/Audio/Voice/{character}/{lineId}.wav`; the generation pipeline is decoupled from the runtime — any TTS that produces 22 kHz mono PCM16 .wav can drop in. The `VoiceLibrarySO` re-binds them on the next `OnValidate` / `Phase32_VoiceLibraryBuilder` rescan. Codifies the file-swap path for moving from macOS `say` to ElevenLabs / XTTS / Piper / a booth-recorded actress without touching any code.

### Acceptance criteria (all green)

- ✅ After pulling, `bash Tools/generate_voices.sh` produces 48 `.wav` files in `Assets/_Project/Audio/Voice/Doris/`. Idempotent — re-running skips existing files.
- ✅ Open Unity, run `Hearthbound → 🚀 Build Everything` — Step 8.5 auto-builds the voice library. Press Play in `00_Bootstrap`. Walk to Doris. The greeting plays through the speakers, in sync with the typewriter. (No extra menu click required.)
- ✅ Typewriter speed adapts (`Mathf.Clamp(text.Length / clipLen, 18, 90)`) so the last character appears as the voice ends — gives a real lip-sync feel.
- ✅ Skipping a line via Space / click / E / Enter stops the voice immediately.
- ✅ No regressions — installs without `HearthboundVoiceLibrary` in `Resources/` (or with no clip for a given lineId) get the previous silent typewriter behaviour. `VoicePlayer.Play` short-circuits to `0f`; `DialogueUI` falls through to the legacy fixed-cps path.
- ✅ `🔍 Diagnose Build` includes a voice-library audit (Step 6/6) reporting SO presence, entry count, bound clips, on-disk .wav count, and any mismatch.
- ✅ Zero new external dependencies — `say` + `afconvert` are macOS built-ins; runtime uses only `UnityEngine.AudioSource` + `Resources.Load`.

---

## [0.6.0-menu-collapse] — 2026-05-26

**Branch:** `feat/mission-1-2-architecture` (accumulating on top of 0.6.0-mission1-polish-v2)
**Theme:** Phase 32 (UX track) — collapse the Hearthbound editor menu to one button. `🚀 Build Everything` is the only path the user ever needs after every pull.

### Solution

The Hearthbound menu now exposes exactly three top-level entries:

```
Hearthbound
├── 🚀  Build Everything   (priority -100 — single click, ~60 s, idempotent)
├── 🔍  Diagnose Build     (priority -90  — read-only Phase 33 aggregate audit)
└── ⚙️  Advanced ►         (40+ entries — every legacy per-phase item)
```

Per the new **D-051** decision, every editor action MUST register under `Hearthbound/⚙️ Advanced/…` unless explicitly promoted to top level.

### Decisions

- **D-051 (NEW):** Every editor action MUST register under `Hearthbound/⚙️ Advanced/…` unless explicitly promoted to top level. The top-level menu is reserved for the three blessed user entry points (`🚀 Build Everything`, `🔍 Diagnose Build`, `⚙️ Advanced ►`).

---

## [0.6.0-mission1-polish-v2] — 2026-05-25

**Branch:** `feat/mission-1-2-architecture` (accumulating on top of 0.5.2)
**Theme:** Phase 32 — Mission 1 Environment Polish v2. Real cottages, Hollow facade, hearth dressing, cozy URP cinematic volumes.

### Phase 32 — Five sub-phases

- **Phase 32.1** — Cottage assembler + extended bindings (370 + 165 LOC)
- **Phase 32.2** — Lane Environment v2 (670 LOC): 8 cottages, Hollow facade, chimneys, atmospheric extras
- **Phase 32.3** — Hollow Interior v2 (475 LOC): kettle, bread, herbs, cupboard, candelabra
- **Phase 32.4** — Cozy URP Volumes (330 LOC): Bloom + Tonemapping + Color Grading + Vignette + Film Grain
- **Phase 32.5** — Master Capstone + Diagnostic + Docs (155 + 260 LOC)

### Decisions

- **D-052 (NEW)** — URP Volume Profiles are authored procedurally by an Editor script (never committed as YAML) so the user's installed URP version determines the serialised format.
- **D-053 (NEW)** — Phase 32 builders operate under a separate `_Phase32Env_*` parent GameObject so v1 and v2 polish layers coexist for diff/bisect.
- **D-054 (NEW)** — Steam VFX on the kettle is OFF by default. Cozy contract: scene doesn't start with a steam stream.

---

## [0.5.2-advance-prompt-and-dream-canvas-hide] — 2026-05-25

**Theme:** Phase 31.1 — visible "click to advance" affordance + DreamCanvas auto-hide.

- **`UI/DialogueUI.cs`** — New `advancePrompt` field (italic TMP `"Click or [Space] ▸"`). PingPongs alpha 0.55 ↔ 1.0 at 1.4 Hz.
- **`Cutscene/MemoryDreamSequencer.cs`** — New `dreamCanvas` field, auto-discovered + force-hidden until cutscene plays.

**D-049 / D-050 (NEW).**

---

## [0.5.1-dialogue-choice-card-repair] — 2026-05-25

**Theme:** Phase 31 — fix the dialogue choice tiles.

- **`UI/DialogueChoiceLayoutHealer.cs`** (NEW, ~120 LOC) — Runtime self-heal helper.
- **`UI/DialogueUI.cs`** — Maps 1/2/3/4 keyboard shortcuts to choice indices.
- **`Editor/Phase31_DialogueChoiceCardRepair.cs`** (NEW, ~340 LOC).

**D-045 / D-046 / D-047 / D-048 (NEW).**

---

## [0.5.0-onboarding-hints-and-rig-doctor] — 2026-05-25

**Theme:** Phase 28 / 29 / 30 trifecta. Definitive body alignment + UI-never-clips + Onboarding & Control Hints.

- **Phase 28** — live `Renderer.bounds.min.y` ground clamp + continuous correction window
- **Phase 29a** — `UI/UIAutoFitText.cs` (NEW, ~173 LOC) — TMP autofit helper applied to every UI label
- **Phase 29b** — `Editor/Phase29_PlayerRigDoctor.cs` (NEW) — Auto-discovers a foot bone
- **Phase 30** — OnboardingOverlay (~350 LOC) + ControlHintsHUD (~155 LOC)

**D-041 amendment, D-042, D-043, D-044 (NEW).**

---

## [0.4.0-build-everything-and-npc-animator] — 2026-05-25

**Theme:** Phase 27 — single-click master capstone + Phase 26 NPC animator pipeline.

- **`Editor/Phase27_BuildEverything.cs`** (200 LOC) — Master capstone, reflection-driven.
- **`Editor/NpcAnimatorControllerBuilder.cs`** (130 LOC) — Builds `Hearthbound_NPC.controller`.
- **`Mission/NpcAnimatorBridge.cs`** (100 LOC) — Toggles `IsTalking` on dialogue events.
- **`Mission/PlayerFootstepBinder.cs`** (160 LOC) — Animation-event-driven footstep SFX.

---

## [0.3.0-player-controller-and-animation] — 2026-05-24

**Theme:** Phase 26 — Complete WASD player controller + smooth follow camera + Mixamo-ready Animator.

- **`Player/SmoothFollowCamera.cs`** (230 LOC) — Spring-damped + slerped rotation + scroll zoom + wall-clip.
- **`Player/PlayerController.cs`** (350 LOC) — Camera-relative WASD, sprint, jump, coyote-time + jump-buffer.
- **`Editor/PlayerAnimatorControllerBuilder.cs`** (200 LOC).
- 7 NUnit EditMode tests.

**D-036 → D-040 (NEW).**

---

## [0.2.1-mission-1-2-ui-activation-hotfix] — 2026-05-24

**Theme:** Phase 25 — fix the user-reported Tone Compass crash.

- Two-layer wiring in 11 builder methods. Script-host stays active; visual child toggles.
- Self-heal Show() in 9 UI overlay scripts.

**D-033 / D-034 (NEW).**

---

## [0.2.0-mission-1-2-polished-playable] — 2026-05-24

**Theme:** Phase 23 (Mission 1 polish capstone) + Phase 24 (Mission 2 scenes).

- Phase 23 (~1,400 LOC): SettingsService, PauseMenuUI, HelpOverlayUI, MissionTitleCard, AmbientAudio, MainMenuSaveCoordinator, PauseSaveCoordinator, Phase23_Mission1PolishCapstone.
- Phase 24 (~1,275 LOC): Mission02Director (479 LOC), Phase24_Mission2SceneBuilder (795 LOC).

**D-028 → D-032 (NEW).**

---

## [0.1.1-mission-1-2-bugfix-and-tooling] — 2026-05-24

**Theme:** Bug-fix cycle + quality-of-life tooling.

- Fixed CS1739 / CS0234 / CS0246 compile errors.
- Added SeedAssetGenerator, ListenSceneSequencer, HearthboundInput.inputactions.
- Added 13 EditMode tests.

**D-008 / D-009 / D-010 (NEW).**

---

## [0.1.0-mission-1-2-architecture] — 2026-05-23

**Theme:** Phase 0 → Phase 10 — architecture, scripts, dialogue, save, mini-games, UI.

- 12 assembly definitions (10 production + 2 test).
- 32 C# scripts, ~4 k lines.
- 5 Yarn dialogue files.

**D-001 → D-007 (NEW).**

---

*This history archive is a condensed reference for releases v0.1.0 through v0.7.3. For the verbose original entries (50+ KB), inspect git history of `CHANGELOG.md` prior to commit `8677f511`.*

*Format: [SemVer](https://semver.org/spec/v2.0.0.html). 1.0.0 when the 20-person greenlight playtest passes.*
