# Changelog — Hearthbound Hollow

All notable changes to this project will be documented here. Entries follow the [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) format.

## [0.6.0-mission1-polish-v2] — 2026-05-25

**Branch:** `feat/mission-1-2-architecture` (accumulating on top of 0.5.2)
**Theme:** Phase 32 — Mission 1 Environment Polish v2. Real cottages, Hollow facade, hearth dressing, cozy URP cinematic volumes.

### User request

> *"please use the available assets in the project to enhance the mission 01 use all packages including medieval village check all asset documents and other packages to create very good environment with great lighting and shaders ...etc and work and push phase by phase and push it phase by phase make the game more polish with great environment clean and readable for player with high quality and polish check all assets"*

### Root cause analysis (what Phase 27 left on the table)

| Observed | Root cause |
|---|---|
| Lane "cottages" look like market stands | `Phase15_MedievalVillageBuilder` fuzzy-searched for "house / cottage / hut" and the MV pack ships no full house prefabs — only modular walls + roofs. Best score landed on `SM_ShopStand_01a`. |
| Hollow door floats in space | Phase 27.2 dressed the door but never wrapped a facade around it. |
| Flat / bland cinematic look | `DefaultVolumeProfile.asset` was empty. No Global Volume added by Phase 27. |
| Hollow interior reads as half-dressed | Phase 27.3 placed walls/floor/hearth but skipped kettle, bread, hanging herbs, cupboard. |

### Phase 32 — Five sub-phases, five separate commits

#### Phase 32.1 — Foundation: cottage assembler + extended bindings
- **`Editor/Phase32_MedievalCottageBuilder.cs`** (NEW, ~370 LOC) — Assembles 4 cottage prefab variants from MV's modular kit
- **`Editor/Phase32_VillageBindingsExtension.cs`** (NEW, ~165 LOC) — `MedievalVillageBindingsV2.asset` SO with 23 prefab roles
- **`Docs/Phase32_Mission1_Polish.md`** (NEW) — Single source of truth

#### Phase 32.2 — Lane Environment v2
- **`Editor/Phase32_LaneEnvironmentV2.cs`** (NEW, ~670 LOC):
  - 8 residential cottages in 4-row layout (Doris's Bakery is the hero)
  - HollowFacade — 4×3 m bakery-style wrap with "The Hollow" sign
  - 3 extra lantern posts along the cobble path
  - Doris's bakery dressing (beehive proxy, hay bale, apple basket, firewood)
  - Smoking chimneys on every cottage (Stylized Weather Dust wisp proxy)
  - Extended cobble path + 20 stone-brick borders
  - 3 distant autumn alders, 6 pebbles, 8 grass tufts, 4 mushrooms, 1 dead tree

#### Phase 32.3 — Hollow Interior v2
- **`Editor/Phase32_HollowInteriorV2.cs`** (NEW, ~475 LOC):
  - Kettle on hearth + steam wisp (off by default)
  - 3 bread loaves on west shelf
  - 3 hanging dried herbs (Lavender / Valerian / Sage)
  - Marin's Cupboard + Stool
  - Workbench Candelabra + cup + bowl + apple
  - Water bucket beside hearth
  - 2 wall candle sconces
  - Larger pulse glow at the hearth

#### Phase 32.4 — Cozy URP Volumes
- **`Editor/Phase32_CozyVolumeBuilder.cs`** (NEW, ~330 LOC) — Authors two URP `VolumeProfile` assets procedurally:
  - **`HearthboundLane_Volume.asset`** (warm dusk outdoor): Bloom + Tonemapping + Color Adjustments + White Balance + Vignette + Film Grain + Channel Mixer
  - **`HearthboundHollow_Volume.asset`** (cozy interior firelight): Same effects, deeper warm tint
  - Main Camera updated: `renderPostProcessing = true`, FXAA High

#### Phase 32.5 — Master Capstone + Diagnostic + Docs
- **`Editor/Phase32_MissionOnePolishCapstone.cs`** (NEW, ~155 LOC) — Single-menu chain of 32.1 → 32.4 + re-runs 27.4 + 31
- **`Editor/Phase32_Diagnostic.cs`** (NEW, ~260 LOC) — Read-only audit reporting passed/warned/failed
- **`Editor/Phase27_BuildEverything.cs`** — Master capstone now chains Phase 32 after Phase 31

### Files added / changed

| File | Status | LOC | Phase |
|---|---|---|---|
| `Assets/_Project/Scripts/Editor/Phase32_MedievalCottageBuilder.cs` | **NEW** | ~370 | 32.1 |
| `Assets/_Project/Scripts/Editor/Phase32_VillageBindingsExtension.cs` | **NEW** | ~165 | 32.1 |
| `Docs/Phase32_Mission1_Polish.md` | **NEW** | — | 32.1 |
| `Assets/_Project/Scripts/Editor/Phase32_LaneEnvironmentV2.cs` | **NEW** | ~670 | 32.2 |
| `Assets/_Project/Scripts/Editor/Phase32_HollowInteriorV2.cs` | **NEW** | ~475 | 32.3 |
| `Assets/_Project/Scripts/Editor/Phase32_CozyVolumeBuilder.cs` | **NEW** | ~330 | 32.4 |
| `Assets/_Project/Scripts/Editor/Phase32_MissionOnePolishCapstone.cs` | **NEW** | ~155 | 32.5 |
| `Assets/_Project/Scripts/Editor/Phase32_Diagnostic.cs` | **NEW** | ~260 | 32.5 |
| `Assets/_Project/Prefabs/Environment/Cottage_*.prefab` ×4 | **NEW (generated)** | — | 32.1 |
| `Assets/_Project/Settings/HearthboundLane_Volume.asset` | **NEW (generated)** | — | 32.4 |
| `Assets/_Project/Settings/HearthboundHollow_Volume.asset` | **NEW (generated)** | — | 32.4 |
| `Assets/_Project/ScriptableObjects/Setup/MedievalVillageBindingsV2.asset` | **NEW (generated)** | — | 32.1 |
| `Assets/_Project/Scripts/Editor/Phase27_BuildEverything.cs` | Updated (+chains 32) | — | 27 |
| `CHANGELOG.md` | Updated (this entry) | — | doc |
| `README.md` | Updated | — | doc |

### Decisions

| # | Decision | Phase |
|---|---|---|
| D-051 (NEW) | Cottages are assembled from MV modular pieces into self-contained prefabs under `Assets/_Project/Prefabs/Environment/`. Phase 15's single-prefab cottage fallback is preserved for backward compatibility, but the v2 path prefers the assembled prefabs. | 32.1 |
| D-052 (NEW) | URP Volume Profiles are authored procedurally by an Editor script (never committed as YAML) so the user's installed URP version determines the serialised format. Avoids URP-version drift. | 32.4 |
| D-053 (NEW) | Phase 32 builders operate under a separate `_Phase32Env_*` parent GameObject in each scene. Phase 27's `_Phase27Env_*` parents are preserved so v1 and v2 polish layers coexist for diff/bisect. | 32.2, 32.3 |
| D-054 (NEW) | Steam VFX on the kettle is OFF by default (`ParticleSystem.Stop()` after creation). Cozy contract: scene doesn't start with a steam stream. User enables for the "kettle just boiled" moment. | 32.3 |

### Behaviour changes the player sees

- **The Lane reads as a residential village.** 8 cottages, not 3 shopstands. Doris's bakery is clearly identifiable. The Hollow is a real building with a sign and a chimney, not a floating door.
- **The Hollow feels inhabited.** Kettle on the hearth, bread on the shelf, herbs hanging from the rafters, candles on the workbench. Marin's cupboard fills the empty east wall.
- **Cinematic cozy look.** Subtle warm bloom on every lantern + window. Slight vignette frames the camera. Warm color grading on dusk-lit outdoor + deeper warmth on interior firelight. Film grain on both.
- **One-click rebuild.** `Hearthbound → ✨ Build EVERYTHING (Phase 27 — one click)` now also runs Phase 32 — fully wired Mission 1 polish in one menu item.

### Known limitations of v0.6.0

| # | Item | Severity | Status |
|---|---|---|---|
| P32-1 | Beehive is a stacked-TerraPot proxy until a proper beehive prop is imported. | Cosmetic | 🟡 Replace when `Hive` asset is imported |
| P32-2 | Chimney smoke uses Stylized Weather `Dust` particle as a wisp proxy until VoluSmokeFX is imported. | Cosmetic | 🟡 Replace when VoluSmokeFX is imported |
| P32-3 | Hanging dried herbs are inverted TerraPot proxies; ropes are Cylinder primitives. Not visually authored. | Cosmetic | 🟡 Replace with hand-authored herb bundle prefab when artist is available |
| P32-4 | Cottage roof tile seams may show at the ridge depending on the exact mesh pivot of `SM_Rooftiles_01a`. | Cosmetic | 🟡 Artist can drag-edit individual cottage prefabs |
| P32-5 | URP Channel Mixer is currently only applied to Lane (autumn foliage boost). Hollow uses default channel mix. | Intentional | ✅ |

---

## [0.5.2-advance-prompt-and-dream-canvas-hide] — 2026-05-25

**Branch:** `feat/mission-1-2-architecture` (accumulating on top of 0.5.1)
**Theme:** Phase 31.1 — make the dialogue's "click to advance" affordance visible, and stop the DreamCanvas bleeding into normal gameplay.

> *"the game is stucked here please fix"*

Doris's `(stands back and watches)` line was fully rendered but the player
had no visible cue that they needed to click/press Space. Compounded by
the DreamCanvas (letterbox bars + dream prose) being permanently visible
and potentially intercepting clicks.

### Phase 31.1 — Advance prompt + Dream canvas hide

- **`UI/DialogueUI.cs`** — New `advancePrompt` field (italic TMP label `"Click or [Space] ▸"`). PingPongs alpha 0.55 ↔ 1.0 at 1.4 Hz.
- **`Cutscene/MemoryDreamSequencer.cs`** — New `dreamCanvas` field, auto-discovered + force-hidden until cutscene plays.
- **`Editor/Phase31_DialogueChoiceCardRepair.cs`** — Now also bakes the AdvancePrompt into the saved DialogueBox prefab.

**D-049 (NEW):** Any blocking dialogue UI must expose a visible advance affordance.
**D-050 (NEW):** Cutscene/cinematic overlays must be hidden by default.

---

## [0.5.1-dialogue-choice-card-repair] — 2026-05-25

**Theme:** Phase 31 — fix the dialogue choice tiles so the game is playable past Doris's first greeting.

- **`UI/DialogueChoiceLayoutHealer.cs`** (NEW, ~120 LOC) — Runtime self-heal helper. Enforces `childControlWidth = true` etc. on the VLG. Heals tile sizeDelta + LayoutElement.
- **`UI/DialogueUI.cs`** — Maps 1/2/3/4 keyboard shortcuts to choice indices. Hides lineText while choices are visible.
- **`UI/ChoiceCardUI.cs`** — Same heal calls on the moral-choice card.
- **`Editor/Phase14_BamaoUIBuilder.cs`** — Fresh builds bake the correct settings.
- **`Editor/Phase31_DialogueChoiceCardRepair.cs`** (NEW, ~340 LOC) — `Hearthbound → 🧰 Phase 31 — Repair Dialogue Choice Cards`.
- **`Editor/Phase27_BuildEverything.cs`** — Master capstone chains Phase 31 after Phase 30.

**D-045, D-046, D-047, D-048 (NEW).**

---

## [0.5.0-onboarding-hints-and-rig-doctor] — 2026-05-25

**Theme:** Phase 28 / 29 / 30 trifecta. Definitive body alignment + UI-never-clips + Onboarding & Control Hints.

### Phase 28 — Definitive body alignment
- **`Player/PlayerGroundClamp.cs`** (rewritten) — Switched to live `Renderer.bounds.min.y`. Continuous correction window.
- **`Player/PlayerController.cs`** — Embedded clamp matches algorithm.

### Phase 29a — UI Polish
- **`UI/UIAutoFitText.cs`** (NEW, +173 LOC) — TMP autofit helper.
- DialogueBox ChoicesContainer relocated inside dialogue box bounds.
- Defensive Awake() autofit on every UI script.

### Phase 29b — Player Rig Doctor
- **`Editor/Phase29_PlayerRigDoctor.cs`** (NEW) — Auto-discovers a foot bone and wires it as `PlayerGroundClamp.footAnchor`.

### Phase 30 — Onboarding overlay + Control Hints HUD
- **`UI/OnboardingOverlay.cs`** (NEW, ~350 LOC) — 6-step walkthrough on Lane.
- **`Mission/ControlHintsHUD.cs`** (NEW, ~155 LOC) — Always-visible parchment chip strip.
- **`Editor/Phase30_OnboardingAndHintsCapstone.cs`** (NEW, ~380 LOC).
- **`Core/VillageState.cs`** — Added `onboardingCompleted` bool.

**D-041 amendment, D-042, D-043, D-044 (NEW).**

---

## [0.4.0-build-everything-and-npc-animator] — 2026-05-25

**Theme:** Phase 27 — single-click master capstone + Phase 26 NPC animator pipeline.

### Added — Editor tools (≈640 LOC)
- **`Editor/Phase27_BuildEverything.cs`** (200 LOC) — Master capstone, reflection-driven.
- **`Editor/Phase26_DiagnosticReport.cs`** (180 LOC) — Read-only audit.
- **`Editor/NpcAnimatorControllerBuilder.cs`** (130 LOC) — Builds `Hearthbound_NPC.controller`.
- **`Editor/Phase26_NpcAnimatorCapstone.cs`** (130 LOC) — Idempotent one-click.

### Added — Runtime (≈260 LOC, in HearthboundHollow.Mission per D-035)
- **`Mission/NpcAnimatorBridge.cs`** (100 LOC) — Toggles `IsTalking` on dialogue events.
- **`Mission/PlayerFootstepBinder.cs`** (160 LOC) — Animation-event-driven footstep SFX.

**No new decisions** — every addition follows D-001..D-040.

---

## [0.3.0-player-controller-and-animation] — 2026-05-24

**Theme:** Phase 26 — Complete WASD player controller + smooth follow camera + Mixamo-ready Animator.

### Added — Phase 26 runtime (≈580 LOC)
- **`Player/SmoothFollowCamera.cs`** (230 LOC) — Spring-damped position + slerped rotation + scroll zoom + wall-clip protection.
- **`Player/PlayerController.cs`** (350 LOC, was 150) — Major upgrade: camera-relative WASD, sprint, jump, coyote-time + jump-buffer, Animator parameter bridge (7 params).
- **`UI/HelpOverlayUI.cs`** (130 LOC) — Gentle Mode strips Sprint/Jump.

### Added — Phase 26 editor tooling (≈400 LOC)
- **`Editor/PlayerAnimatorControllerBuilder.cs`** (200 LOC).
- **`Editor/Phase26_PlayerControllerAndAnimation.cs`** (200 LOC).

### Added — Tests + Input + Docs
- **`Tests/EditMode/PlayerControllerTests.cs`** — 7 NUnit tests.
- **`Settings/HearthboundInput.inputactions`** — 5 new actions.
- **`Docs/ANIMATION_REQUIREMENTS.md`**.

**D-036, D-037, D-038, D-039, D-040 (NEW).**

---

## [0.2.1-mission-1-2-ui-activation-hotfix] — 2026-05-24

**Theme:** Phase 25 — fix the user-reported Tone Compass crash and the systemic single-layer UI wiring anti-pattern.

- **🐞 USER CRASH** — *"Coroutine couldn't be started because the the game object 'ToneCompass' is inactive!"*
- Two-layer wiring in 11 builder methods. Script-host stays active; visual child toggles.
- Self-heal Show() in 9 UI overlay scripts.

**D-033, D-034 (NEW).**

---

## [0.2.0-mission-1-2-polished-playable] — 2026-05-24

**Theme:** Phase 23 (Mission 1 polish capstone) + Phase 24 (Mission 2 scenes).

### Added — Phase 23 (~1,400 LOC)
- `Core/SettingsService.cs`, `UI/PauseMenuUI.cs`, `UI/HelpOverlayUI.cs`, `UI/MissionTitleCard.cs`, `Audio/AmbientAudio.cs`, `Mission/MainMenuSaveCoordinator.cs`, `Mission/PauseSaveCoordinator.cs`, `Editor/Phase23_Mission1PolishCapstone.cs` (~670 LOC).

### Added — Phase 24 (~1,275 LOC)
- `Mission/Mission02Director.cs` (479 LOC), `Editor/Phase24_Mission2SceneBuilder.cs` (795 LOC).

**D-028..D-032 (NEW).**

---

## [0.1.1-mission-1-2-bugfix-and-tooling] — 2026-05-24

**Theme:** Bug-fix cycle (Phase 10.5) + quality-of-life tooling (Phase 11).

- Fixed CS1739 in SaveService.cs, CS0234/CS0246 in MiniGames + Dialogue.
- Added `Editor/SeedAssetGenerator.cs`, `Cutscene/ListenSceneSequencer.cs`, `Settings/HearthboundInput.inputactions`.
- Added `Tests/EditMode/SaveAndRippleTests.cs` (+13 tests).

**D-008, D-009, D-010 (NEW).**

---

## [0.1.0-mission-1-2-architecture] — 2026-05-23

**Theme:** Phase 0 → Phase 10 — architecture, scripts, dialogue, save, mini-games, UI.

- `Docs/ARCHITECTURE.md`, `Docs/PROGRESS.md`, `Docs/EXISTING_ASSETS_INDEX.md`, `Docs/SCENE_ASSEMBLY_GUIDE.md`.
- 12 assembly definitions (10 production + 2 test).
- 32 C# scripts, ~4 k lines (Core 7, Memory 8, Player 9, MiniGames 3, UI 9, Dialogue 1, Cutscene 1, Save 2, Mission 3, Tests 1).
- 5 Yarn dialogue files.

**D-001 through D-007 (NEW).**

---

*Format: [SemVer](https://semver.org/spec/v2.0.0.html). Versions advance to 0.3.0 with Phase 26 (player controller + animation), 0.4.0 with Phase 27 (master capstone + NPC animator pipeline), 0.5.0 with Phase 28 / 29 / 30 (body-alignment / UI / onboarding trifecta), 0.6.0 with Phase 32 (Mission 1 Polish v2 — cottages, facade, URP volumes). 1.0.0 when the 20-person greenlight playtest passes.*
