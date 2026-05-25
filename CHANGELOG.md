# Changelog — Hearthbound Hollow

All notable changes to this project will be documented here. Entries follow the [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) format.

## [0.3.0-player-controller-and-animation] — 2026-05-24

**Branch:** `feat/mission-1-2-architecture` → PR #7 (open)
**Theme:** Phase 26 — Complete WASD player controller (sprint + jump + camera-relative input) + smooth follow camera (mouse orbit, scroll zoom, wall-clip) + Mixamo-ready Humanoid Animator Controller (1D blend tree, jump/fall/land states).

### Added — Phase 26 runtime (≈580 LOC)

#### New runtime scripts

- **`Player/SmoothFollowCamera.cs`** (230 LOC) — Spring-damped position (`SmoothDamp`), slerped rotation, mouse-orbit gated by Right Mouse Button or `AllowLook` action, scroll-wheel zoom (`distanceMin/Max`), sphere-cast wall-clip protection. Cinemachine-agnostic — works whether Cinemachine is installed or not.

#### Rewritten runtime scripts

- **`Player/PlayerController.cs`** (350 LOC, was 150) — Major upgrade:
  - Camera-relative WASD input (uses `MainCamera.forward` when present; falls back to world-axis input when not — preserves headless EditMode test behaviour).
  - Smooth acceleration + deceleration (configurable rates).
  - **Sprint** (Shift / Gamepad LStick click). Toggleable via `enableSprint`; auto-disabled in Gentle Mode.
  - **Jump** (Space / Gamepad south). Toggleable via `enableJump`; auto-disabled in Gentle Mode. Manual gravity integration on `CharacterController.Move()` (replaces `SimpleMove`). Coyote-time 0.15 s + jump-buffer 0.12 s for forgiving feel.
  - Animator parameter bridge — drives 7 params every frame: `Speed`, `MoveX`, `MoveY`, `VelocityY`, `IsGrounded`, `IsSprinting`, `Jump` (trigger).
  - Public API preserved (`MovementLocked`, `CurrentFocus`, `CurrentMoveInput`, `TryActivateFocus`) — Mission01Director / Mission02Director compile unchanged.

- **`Player/SimpleFollowCamera.cs`** — header updated to mark it as the legacy fallback; body unchanged for backward compatibility with scenes saved before Phase 26.
- **`UI/HelpOverlayUI.cs`** (130 LOC, was 117) — Body text now built at Show() time so toggling Gentle Mode hot-refreshes. Documents Sprint, Jump, Look (RMB), Zoom (scroll). Gentle Mode strips Sprint/Jump from the card.

### Added — Phase 26 editor tooling (≈400 LOC)

- **`Editor/PlayerAnimatorControllerBuilder.cs`** (200 LOC) — Builds `Assets/_Project/Animations/Hearthbound_Player.controller` procedurally:
  - 7 parameters (Speed/MoveX/MoveY/VelocityY/IsGrounded/IsSprinting/Jump).
  - 4 states: Locomotion (1D blend tree on Speed), Jump, Fall, Land.
  - Auto-detects best AnimationClip per role with Mixamo > BoZo > demo scoring.
  - Menu: `Hearthbound → Phase 26 — Build Player Animator Controller`.
- **`Editor/Phase26_PlayerControllerAndAnimation.cs`** (200 LOC) — Single-click capstone:
  - Runs the AnimatorController builder.
  - Loads Player prefab, ensures Animator + controller wired (Apply Root Motion = false).
  - Walks Lane/Hollow/Garden/Cottage scenes:
    - Upgrades `SimpleFollowCamera` → `SmoothFollowCamera` (copies target + framing).
    - Wires `PlayerController.cameraReference` to the upgraded camera.
    - Ensures the scene's Player has the new Animator + controller.
  - Menu: `Hearthbound → 🏃 Phase 26 — Player Controller + Animation`.

### Added — Tests (≈140 LOC)

- **`Tests/EditMode/PlayerControllerTests.cs`** — 7 NUnit tests locking the public surface:
  - `MovementLocked` default + setter roundtrip
  - Defaults (`IsSprinting`, `IsGrounded`, `CurrentVelocity`, `CurrentMoveInput`)
  - `SetCameraReference` / `SetAnimator` smoke
  - `SmoothFollowCamera` defaults sanity
  - `SnapToTargetImmediate` doesn't throw
- EditMode tests asmdef updated to reference `HearthboundHollow.Player`.

### Added — Input Actions (≈90 LOC)

- **`Settings/HearthboundInput.inputactions`** — 5 new actions:
  - **Sprint** — Left/Right Shift; Gamepad left-stick click
  - **Jump** — Space; Gamepad south
  - **CameraLook** (Vector2) — Mouse delta; Gamepad right stick (scaled)
  - **CameraZoom** (Axis) — Mouse scroll Y; Gamepad LB/RB 1D composite
  - **AllowLook** — Right Mouse Button; Gamepad right-stick click
- Existing actions and 3 control schemes (Keyboard&Mouse / Gamepad / Touch) preserved.

### Added — Documentation (≈280 LOC)

- **`Docs/ANIMATION_REQUIREMENTS.md`** — Complete reference: animator state graph, parameter contract, clip roster, Mixamo download list (6 FBXs), Humanoid retargeting steps, Asset Store alternatives (Mixamo Animation Pack, ACS, Easy Character Movement 2, Synty), performance notes, validation checklist, design decisions D-035 → D-039.
- **`Docs/PROGRESS.md`** — Phase 26 entry + decisions table renumbered to D-035..D-039 + updated menu list.
- **`Docs/ARCHITECTURE.md`** — § 0 controller/camera/animator rows + § 4.4 PlayerController surface + § 4.5 SmoothFollowCamera + § 15 Decisions Index covering D-033..D-039.
- **`README.md`** — controls table + 2-click quickstart.

### Decisions adopted in this release

> ℹ️ Phase 25 (UI activation hotfix, landed in parallel) consumed D-033 + D-034. Phase 26's decisions therefore start at D-035.

- **D-035** Sprint + Jump are opt-in runtime flags (`enableSprint`, `enableJump`). Gentle Mode disables both. Cozy GDD doesn't ask for them; we add them so playtesters who reach for Shift/Space don't perceive the controller as broken.
- **D-036** Player Animator is a single 1D blend tree on `Speed`, not a 2D tree. Cozy character always faces movement direction.
- **D-037** Animator parameter names are configurable strings on the `PlayerController` Inspector — swapping to a community controller is a name-retype, no code rewrite.
- **D-038** `SmoothFollowCamera` is the M1+M2 default. Phase 17 still creates a Cinemachine prefab when the package is present; the two coexist (Phase 26 doesn't touch the Cinemachine path).
- **D-039** Animations live in `Assets/_Project/Animations/` (Mixamo subfolder optional) — single search path keeps auto-detection deterministic.

### Files net delta (vs 0.2.1)

- **+4 C# files:** SmoothFollowCamera, PlayerAnimatorControllerBuilder, Phase26_PlayerControllerAndAnimation, PlayerControllerTests.
- **+1 doc:** ANIMATION_REQUIREMENTS.md.
- **Rewritten:** PlayerController.cs (350 LOC, was 150), HelpOverlayUI.cs, HearthboundInput.inputactions, EditMode tests asmdef.
- **Updated:** SimpleFollowCamera.cs header comment, PROGRESS.md, CHANGELOG.md (this entry), ARCHITECTURE.md, README.md.

### Build Settings — unchanged

Six scenes; Phase 26 doesn't add new ones.

### Player-facing controls

| Action | Key / Stick |
|---|---|
| Move | WASD / Arrow keys / Gamepad left stick |
| **Sprint** | **Left Shift / Gamepad LStick click** (Gentle Mode disables) |
| **Jump** | **Space / Gamepad south** (Gentle Mode disables) |
| Interact | E / Gamepad south |
| Advance line | Click / Space / Enter |
| Polish orb | Hold left mouse, draw slow circles |
| **Camera look** | **Hold Right Mouse + drag / Gamepad right stick** |
| **Camera zoom** | **Mouse scroll / Gamepad LB-RB** |
| Pause | Escape |
| Help | H |

### How the user uses it

1. Pull the branch. Unity recompiles.
2. `Hearthbound → 🎮 Build POLISHED Mission 1 + 2 (Phase 23)`. *[unchanged]*
3. `Hearthbound → 🏃 Phase 26 — Player Controller + Animation`. *[NEW, ~5 s]*
4. (Optional) Drop 6 Mixamo FBXs into `Assets/_Project/Animations/Mixamo/` per `Docs/ANIMATION_REQUIREMENTS.md` § 3 and re-run Phase 26.
5. Press Play. WASD moves camera-relative; Shift sprints; Space jumps; Right Mouse + drag orbits camera; scroll zooms.

---

## [0.2.1-mission-1-2-ui-activation-hotfix] — 2026-05-24

**Branch:** `feat/mission-1-2-architecture` → PR #7 (open)
**Theme:** Phase 25 — fix the user-reported Tone Compass crash and the systemic single-layer UI wiring anti-pattern that produced it.

### Fixed

- **🐞 USER CRASH** — *"Coroutine couldn't be started because the the game object 'ToneCompass' is inactive!"* at `ToneCompassCard.cs:55` from `MainMenuController.OnOpenTheHollow`. The Phase 22 builder was attaching the `ToneCompassCard` script to a GameObject and then deactivating that same GameObject, which made `StartCoroutine` throw the moment `Show()` was called.
- **Latent bug, same family:** `PauseMenuUI.Update()` and `HelpOverlayUI.Update()` were never firing in built scenes because their script-hosts were deactivated by Phase 23 → Escape and H keys did nothing. This is now fixed for any new build, and the scripts now self-heal on `Show()` / `Pause()` for the existing build.
- **Latent bug, same family:** `MissionTitleCard.Play()` — same `StartCoroutine` risk.

### Added — defensive patterns

- **Two-layer wiring** in `HearthboundOneClickSetup` (BuildToneCompass, BuildPrimitiveDialogueUI, BuildPrimitiveEveningLedgerUI) and `Phase23_Mission1PolishCapstone` (BuildPauseMenu, BuildHelpOverlay, BuildSettingsPanel, AddMissionTitleCard). The script-host GameObject stays active; a child "Visual" GameObject carries the visible UI and is what gets toggled.
- **Self-heal Show()** in every UI overlay (`ToneCompassCard`, `MissionTitleCard`, `PauseMenuUI`, `HelpOverlayUI`, `ComfortToolsMenu`, `ChoiceCardUI`, `DialogueUI`, `EveningLedgerUI`, `TeaBrewingUI`). Each method calls `gameObject.SetActive(true)` if dormant, and guards `StartCoroutine` with `activeInHierarchy && isActiveAndEnabled`. Fallback paths render content without animation when coroutines are unavailable.

### Decisions adopted in this release

- **D-033** Procedural UI builders MUST use the two-layer pattern. Script-host stays active; visual child is toggled. Single-layer (deactivating the script-host) is forbidden.
- **D-034** UI overlay scripts MUST self-heal in their `Show()` (or equivalent entry point). Belt-and-braces — the wiring is correct without it, but it protects against future regressions.

### Files net delta (vs 0.2.0)

- **Modified C# files (11):** `ToneCompassCard.cs`, `MissionTitleCard.cs`, `PauseMenuUI.cs`, `HelpOverlayUI.cs`, `ComfortToolsMenu.cs`, `ChoiceCardUI.cs`, `DialogueUI.cs`, `EveningLedgerUI.cs`, `TeaBrewingUI.cs`, `HearthboundOneClickSetup.cs`, `Phase23_Mission1PolishCapstone.cs`.
- **Modified docs:** `Docs/PROGRESS.md`, `CHANGELOG.md`.
- **No new files.** No new asmdef changes. No new package dependencies.

### Player-facing impact

| Before | After |
|---|---|
| Click "Open The Hollow" → game crashes immediately with a coroutine error | Click "Open The Hollow" → Tone Compass fades in. Continue is dimmed for one frame. Player reads, toggles Gentle Mode if desired, clicks Continue → Lane scene loads. |
| Esc / H do nothing during gameplay (after the user reaches Lane via some workaround) | Esc opens the pause menu in every gameplay scene; H opens the help/controls card. |
| Mission title cards may flicker or not appear if scene loads with a dormant host | Title cards always fade in cleanly. |

---

## [0.2.0-mission-1-2-polished-playable] — 2026-05-24

**Branch:** `feat/mission-1-2-architecture` → PR #7 (open)
**Theme:** Phase 23 (Mission 1 polish capstone) + Phase 24 (Mission 2 scenes) — the polished, fully player-facing Mission 1 + 2 build.

### Added — Phase 23 (Mission 1 polish capstone, ~1,400 LOC)

#### New runtime scripts
- **`Core/SettingsService.cs`** — PlayerPrefs-backed audio/comfort/accessibility settings (12 keys). `EffectiveVolume(AudioChannel)` helper. `OnSettingsChanged` event so subscribers can hot-refresh.
- **`Core/SettingsServiceBootstrap.cs`** — Tiny MonoBehaviour at execution order -900 that registers SettingsService with ServiceLocator at boot.
- **`UI/PauseMenuUI.cs`** — Esc-toggled pause overlay. Resume / Settings (event) / Save & Quit / Quit to Desktop. Pauses Time.timeScale + AudioListener. Defensive restore on OnDestroy.
- **`UI/HelpOverlayUI.cs`** — H-toggled controls reference card with in-fiction Marin quote. Auto-shows on first start (`VillageState.tutorialCompleted == false`); marks tutorial complete on first close.
- **`UI/MissionTitleCard.cs`** — CanvasGroup fade-in title card. Pulls displayName + toneOneLine from MissionSO. Skippable on click/space/enter. Uses `Time.unscaledDeltaTime` so it runs even when the pause menu is up.
- **`Audio/AmbientAudio.cs`** — Looping ambient bed. Resolves AudioClip via `SfxLibrarySO` entry id (or direct clip). Volume gated by `SettingsService.EffectiveVolume(Ambient)`; hot-refreshes on `OnSettingsChanged`.
- **`Mission/MainMenuSaveCoordinator.cs`** — Bridges UI → Save asmdef. Awake: ensures SaveService is registered. Start: enables Continue button only if autosave (slot -1) exists. On click: `SaveService.Load(-1, vs)` + `GameManager.LoadScene(vs.lastSceneName ?? fallback)`.
- **`Mission/PauseSaveCoordinator.cs`** — Listens to `PauseMenuUI.OnSaveAndQuitRequested` + `DayEndedEvent`. Captures active scene name into `VillageState.lastSceneName` and writes autosave.

#### Updated
- **`UI/MainMenuController.cs`** — Emits `OnContinueRequested` + `OnSettingsRequested` events (replacing direct save-service calls). Adds `SetContinueEnabled(bool)` for the coordinator to call. Continue button starts dim. 7 cozy tip strings (was 5). Settings opens the panel + `ComfortToolsMenu` when wired.

#### Editor capstone
- **`Editor/Phase23_Mission1PolishCapstone.cs`** (~670 LOC). Menu: **`Hearthbound → 🎮 Build POLISHED Mission 1 + 2 (Phase 23)`**.
  - Runs Phase 22 (which chains Phases 13–21).
  - Runs Phase 24 (Mission 2 scenes).
  - Post-processes 6 scenes:
    - Bootstrap: adds `SettingsServiceBootstrap` on `_GameRoot`.
    - MainMenu: builds Settings panel (procedural Bamao-tone) with `ComfortToolsMenu` (Gentle Mode toggle, Auto-Polish toggle, Auto-Cleanse toggle, Subtitle Size slider) + adds `MainMenuSaveCoordinator`.
    - Lane: spawns `_AmbientAudio` (autumn loop, 0.40 base) + `PauseMenu` + `HelpOverlay` + `MissionTitleCard` ("Day 1 — Opening the Hollow").
    - Hollow: spawns ambient (0.30 hearth-quiet) + Pause + Help + TitleCard + **Pickle the cat** (sphere primitive with PickleAI wired to player + orb) + sets `Mission01Director.sceneAfterEndOfDay = "04_Mission02_Garden"` (the M1→M2 hand-off).
    - Garden: ambient (0.45 outdoor wind) + Pause + Help.
    - Cottage: ambient (0.25 hearth-quiet) + Pause + Help.
  - Updates Build Settings to 6 scenes in correct order.
  - Opens Bootstrap so the user can Press Play immediately.

### Added — Phase 24 (Mission 2 Garden + Cottage scenes, ~1,275 LOC)

- **`Mission/Mission02Director.cs`** (479 LOC) — single runtime orchestrator that handles both Mission 2 scenes via `SceneRole` enum.
  - **Garden flow**: title card → Marin's intro → herb harvest (HerbHarvestedEvent listener) → kettle activation → TeaBrewingUI → TeaBrewedEvent → garden exit unlocks → loads Cottage scene.
  - **Cottage flow**: title card → Gerrold greeting (3 lines + 3-option reply, +0/+3/+6 trust ripple) → ChoiceCardUI with 4 tariffs.
  - **Branch logic**: Erase → CleanseMiniGame (gentleMode) → Dream 2 Variant A. Cleanse → CleanseMiniGame → Dream 2 Variant B/C (depending on CleanseOutcome). Listen → no mini-game + 4-beat narrative pause + Variant D. Defer → no mini-game + Variant E.
  - **Outcome-specific Evening Ledger** — 5 distinct passages (one for each cleanse-outcome and each non-cleanse branch).
- **`Editor/Phase24_Mission2SceneBuilder.cs`** (795 LOC) — Menu: **`Hearthbound → Phase 24 — Build Mission 2 Scenes`**. Builds both scenes from scratch.

### Decisions adopted in this release
- **D-028** SettingsService is a plain C# class, not a ScriptableObject.
- **D-029** MainMenuSaveCoordinator + PauseSaveCoordinator live in Mission asmdef.
- **D-030** TeaBrewingUI default duration = 12 s (was 90).
- **D-031** Mission01Director.sceneAfterEndOfDay defaults to MainMenu but is overridden to "04_Mission02_Garden" by Phase 23.
- **D-032** Runtime MonoBehaviours never live in Editor-asmdef files (hotfix decision).

### Build Settings (after Phase 23 runs)
| Index | Scene |
|---|---|
| 0 | `Assets/_Project/Scenes/00_Bootstrap.unity` |
| 1 | `Assets/_Project/Scenes/01_MainMenu.unity` |
| 2 | `Assets/_Project/Scenes/02_Mission01_Lane.unity` |
| 3 | `Assets/_Project/Scenes/03_Mission01_Hollow.unity` |
| 4 | `Assets/_Project/Scenes/04_Mission02_Garden.unity` |
| 5 | `Assets/_Project/Scenes/05_Mission02_Cottage.unity` |

---

## [0.1.1-mission-1-2-bugfix-and-tooling] — 2026-05-24

**Branch:** `feat/mission-1-2-architecture` → PR #7 (open)
**Theme:** Bug-fix cycle (Phase 10.5) + quality-of-life tooling (Phase 11).

### Fixed
- **CS1739 in SaveService.cs** — `VillageStateSavedEvent` ctor parameter is named `autosave`, not `isAutosave`.
- **CS0234 / CS0246 in MiniGames** — MiniGames asmdef missing Player reference.
- **CS0234 (preemptive) in Dialogue** — Dialogue asmdef now references Save proactively.
- **"Project has invalid dependencies"** — Removed `com.unity.textmeshpro` from manifest (folded into UGUI 2.0 in Unity 6).

### Added — Editor tooling (Phase 11)
- `Editor/SeedAssetGenerator.cs` — generates the 17 canonical ScriptableObjects.
- `Editor/HearthboundHollow.Editor.asmdef` — Editor-only assembly.
- Menu items: `Hearthbound → Create Mission 1-2 Seed Assets`, `Hearthbound → Validate Mission 1-2 Seed Assets`.

### Added — Runtime (Phase 11)
- `Cutscene/ListenSceneSequencer.cs` — Mission 2 Listen-path Timeline driver.
- `Settings/HearthboundInput.inputactions` — Move / Interact / PointerPosition / PointerActive / Pause across K&M / Gamepad / Touch.

### Added — Tests (Phase 11)
- `Tests/EditMode/SaveAndRippleTests.cs` — +13 NUnit tests, bringing EditMode coverage from 8 to 21.

### Decisions adopted
- D-008 Drop `com.unity.textmeshpro` from manifest.
- D-009 ListenSceneSequencer integrates Cinemachine via reflection.
- D-010 Seed assets generated by Editor menu, not committed.

---

## [0.1.0-mission-1-2-architecture] — 2026-05-23

**Branch:** `feat/mission-1-2-architecture` → PR #7
**Theme:** Phase 0 → Phase 10 — architecture, scripts, dialogue, save, mini-games, UI.

### Added — Documentation
- `Docs/ARCHITECTURE.md` (15 §), `Docs/PROGRESS.md`, `Docs/EXISTING_ASSETS_INDEX.md`, `Docs/SCENE_ASSEMBLY_GUIDE.md`.

### Added — Package manifest
- URP 17.2, Cinemachine 3.1, ~~TextMeshPro 3.0.9~~ (removed in 0.1.1), Addressables 2.6, Visual Effect Graph 17.2, Timeline 1.8, Splines 2.7, 2D Sprite, 2D Animation, Test Framework 1.5.

### Added — Folder skeleton
- `Assets/_Project/` with 23 subfolders, `.gitkeep` markers.

### Added — Assembly definitions (12 files)
- 10 production + 2 test, full dependency graph.

### Added — Scripts (32 files, ~4 k lines)
- **Core (7), Memory (8), Player (9), MiniGames (3), UI (9), Dialogue (1), Cutscene (1), Save (2), Mission (3), Tests (1).** See `PROGRESS.md` for the full list.

### Added — Yarn dialogue (5 files)
- `Doris_M1.yarn` (9 nodes), `Gerrold_M2.yarn` (10 nodes), `Marin_Notes.yarn` (3 nodes), `Pickle.yarn` (4 nodes), `Codex.yarn` (6 nodes).

### Decisions adopted
- D-001 BoZo (chibi) over City Characters
- D-002 Yarn Spinner over OpenAI Dialogue addon
- D-003 Don't relocate existing vendor folders
- D-004 One asmdef per Scripts/ subfolder
- D-005 InteractionPromptUI lives in Player asmdef
- D-006 YarnVillageStateBridge compile-guarded
- D-007 Scenes are Unity-side; scripts are GitHub-side

---

*Format: [SemVer](https://semver.org/spec/v2.0.0.html). Versions advance to 0.3.0 with Phase 26 (player controller + animation). 1.0.0 when the 20-person greenlight playtest passes.*
