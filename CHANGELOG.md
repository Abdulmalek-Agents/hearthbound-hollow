# Changelog — Hearthbound Hollow

All notable changes to this project will be documented here. Entries follow the [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) format.

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

### Phase 26/27 — what comes next (in-progress)

- **Phase 26** — HEAT modern UI swap-in for Main Menu / Settings / Pause / Help / HUD. Uses the 59 prefabs already imported under `Assets/Heat - Complete Modern UI/`. Visual upgrade; two-layer wiring established in Phase 25 is preserved.
- **Phase 27** — Mission 1+2 depth polish against `Docs/Depth_Bible/Mission_1_2_Focus/*`. Audit dialogue beats, surface Marin's note, polish emotional pacing.

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
- **`Editor/Phase24_Mission2SceneBuilder.cs`** (795 LOC) — Menu: **`Hearthbound → Phase 24 — Build Mission 2 Scenes`**. Builds both scenes from scratch:
  - Garden: ground, 2 plants (Lavender + Valerian), Kettle interactable, Garden_Exit_Trigger, UI (DialogueUI + TeaBrewingUI + MissionTitleCard).
  - Cottage: wood floor, 3 walls, Gerrold (BoZo Bard or cylinder fallback), table, hidden GER-007 orb, _CleanseMiniGame, _MemoryDreamRig + PlayableDirector, UI (DialogueUI + ChoiceCardUI + EveningLedgerUI + MissionTitleCard).
  - Adds both scenes to Build Settings as indices 4 and 5.
  - Mission02Director is wired with every reference (4 tariffs, Gerrold villager, GER-007 memory, cleanse game, dream sequencer).

### Decisions adopted in this release
- **D-028** SettingsService is a plain C# class, not a ScriptableObject — PlayerPrefs persists across sessions.
- **D-029** MainMenuSaveCoordinator + PauseSaveCoordinator live in Mission asmdef — UI stays Save-free. Matches the DreamHook pattern from Phase 22.
- **D-030** TeaBrewingUI default duration = 12 s (was 90 s) — first-play loop is tight; players who want longer timers can toggle Gentle Mode.
- **D-031** Mission01Director.sceneAfterEndOfDay defaults to "01_MainMenu" but is overridden to "04_Mission02_Garden" by Phase 23 — default stays safe (no Mission 2 = no broken hand-off); polish capstone wires the hand-off only when Mission 2 scenes exist.

### Files net delta (vs 0.1.1)
- **+11 C# files**: SettingsService, SettingsServiceBootstrap, PauseMenuUI, HelpOverlayUI, MissionTitleCard, AmbientAudio, MainMenuSaveCoordinator, PauseSaveCoordinator, Mission02Director, Phase23_Mission1PolishCapstone, Phase24_Mission2SceneBuilder.
- **+11 .meta files** (one per script).
- **Modified**: MainMenuController.cs (events + Continue dimming + SetContinueEnabled API).

### Build Settings (after Phase 23 runs)
| Index | Scene |
|---|---|
| 0 | `Assets/_Project/Scenes/00_Bootstrap.unity` |
| 1 | `Assets/_Project/Scenes/01_MainMenu.unity` |
| 2 | `Assets/_Project/Scenes/02_Mission01_Lane.unity` |
| 3 | `Assets/_Project/Scenes/03_Mission01_Hollow.unity` |
| 4 | `Assets/_Project/Scenes/04_Mission02_Garden.unity` |
| 5 | `Assets/_Project/Scenes/05_Mission02_Cottage.unity` |

### Player-facing controls
| Action | Key |
|---|---|
| Move | WASD / Arrow keys / Gamepad left stick |
| Interact | E / Gamepad south |
| Advance line | Click / Space / Enter |
| Polish orb | Hold left mouse, draw slow circles |
| Pause | Escape |
| Help | H |

---

## [0.1.1-mission-1-2-bugfix-and-tooling] — 2026-05-24

**Branch:** `feat/mission-1-2-architecture` → PR #7 (open)
**Theme:** Bug-fix cycle (Phase 10.5) + quality-of-life tooling (Phase 11).

### Fixed
- **CS1739 in SaveService.cs** — `VillageStateSavedEvent` ctor parameter is named `autosave`, not `isAutosave`. Switched the call site to positional argument.
- **CS0234 / CS0246 in MiniGames** — `PolishMiniGame`, `CleanseMiniGame`, and `MiniGameBase` use `MemoryOrbInteractable` (in `HearthboundHollow.Player`). The MiniGames asmdef was missing the Player reference. Added.
- **CS0234 (preemptive) in Dialogue** — When the user installs Yarn Spinner, `YarnVillageStateBridge.SaveAutopoint()` activates and references `HearthboundHollow.Save.SaveService`. The Dialogue asmdef now references Save proactively so no rework is needed at Yarn-install time.
- **"Project has invalid dependencies"** — Unity 6 folded `com.unity.textmeshpro` into `com.unity.ugui` 2.0; the standalone package no longer resolves. Removed from `Packages/manifest.json`. The `Unity.TextMeshPro` assembly name is still valid (asmdefs that reference it continue to compile).

### Added — Editor tooling (Phase 11)
- **`Editor/SeedAssetGenerator.cs`** — Menu item `Hearthbound → Create Mission 1-2 Seed Assets` creates all 17 canonical ScriptableObject assets at the expected paths:
  - `VillageState.asset`
  - `Mission01_OpeningTheHollow.asset`, `Mission02_TheWidowersRequest.asset`
  - `Doris.asset`, `Gerrold.asset`, `SilentLane.asset`
  - `Lavender.asset`, `Valerian.asset`
  - `DOR-001_FirstLoaves.asset`, `GER-007_SeventhMorning.asset`
  - `ECHO_DOR001_GER007.asset`
  - `MemoryMap_Doris.asset`, `MemoryMap_Gerrold.asset`
  - `Tariff_Erase.asset`, `Tariff_Cleanse.asset`, `Tariff_Listen.asset`, `Tariff_Defer.asset`
  - Saves ~30 minutes of right-click → Create per asset.
  - Idempotent: skips assets that already exist; safe to re-run.
- **`Editor/HearthboundHollow.Editor.asmdef`** — Editor-only assembly so menu code does not ship in runtime builds.
- **Menu item `Hearthbound → Validate Mission 1-2 Seed Assets`** — Logs which seed assets are missing.

### Added — Runtime (Phase 11)
- **`Cutscene/ListenSceneSequencer.cs`** — Mission 2 Listen-path Timeline driver. Plays the 3-min Listen cutscene, then routes to Memory Dream 2 Variant D. Cinemachine priority is set via reflection so the Cutscene asmdef does not take a hard `com.unity.cinemachine` compile dep (D-009).
- **`Settings/HearthboundInput.inputactions`** — Pre-configured Input Actions asset:
  - **Move** (Vector2): WASD, Arrow keys, Gamepad left stick
  - **Interact** (Button): E key, Gamepad south button
  - **PointerPosition** (Vector2): mouse / touch position
  - **PointerActive** (Button): left mouse / touch press
  - **Pause** (Button): Escape, Gamepad start
  - Three control schemes: Keyboard&Mouse, Gamepad, Touch (mobile-ready).

### Added — Tests (Phase 11)
- **`Tests/EditMode/SaveAndRippleTests.cs`** — +13 NUnit tests, bringing EditMode coverage from 8 to 21:
  - `SaveServiceRoundTripTests` (5): round-trip, autosave slot, missing slot, empty label, delete slot
  - `RippleEngineTests` (4): Erase tariff, Listen tariff, coin clamp, integrity clamp
  - `MemoryNodeTests` (2): EffectiveTint override + tone fallback
  - `VillagerMemoryRuntimeTests` (2): Reveal + idempotency
- EditMode tests asmdef updated to reference `HearthboundHollow.Mission` (for RippleEngine).

### Decisions adopted in this release
- **D-008** Drop `com.unity.textmeshpro` from manifest — Unity 6 folded TMP into `com.unity.ugui` 2.0.
- **D-009** ListenSceneSequencer integrates Cinemachine via reflection — Cutscene asmdef stays Cinemachine-agnostic.
- **D-010** Seed assets are generated by Editor menu, not committed as .asset files — avoids GUID/.meta race conditions; idempotent + safe to re-run.

### Files net delta (vs v0.1.0)
- **+4 C# files**: ListenSceneSequencer, SeedAssetGenerator, SaveAndRippleTests (+CoreTests still present).
- **+1 asmdef**: HearthboundHollow.Editor.
- **+1 Input Actions asset**: HearthboundInput.inputactions.
- **Modified**: SaveService.cs, manifest.json, MiniGames asmdef, Dialogue asmdef, EditMode tests asmdef, PROGRESS.md, CHANGELOG.md.

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
- **Core (7):** Hh, EventBus, ServiceLocator, GameEvents, VillageState, MissionSO, GameManager
- **Memory (8):** EmotionalTone, MoralChoice, VillagerSO, MemoryNodeSO, MemoryConnectionSO, VillagerMemoryMapSO, MemoryHerb, TariffSO
- **Player (9):** Interactable, PlayerController, MemoryOrbInteractable, HollowDoorInteractable, HerbHarvestInteractable, KettleInteractable, DayCycleManager, LumenLightController, InteractionPromptUI
- **MiniGames (3):** MiniGameBase, PolishMiniGame, CleanseMiniGame
- **UI (9):** DialogueUI, ChoiceCardUI, EveningLedgerUI, TeaBrewingUI, CodexUI, ComfortToolsMenu, ToneCompassCard, HUDController, MainMenuController
- **Dialogue (1):** YarnVillageStateBridge (compile-guarded)
- **Cutscene (1):** MemoryDreamSequencer
- **Save (2):** SaveService, VillageStateSnapshot
- **Mission (3):** MissionRunner, RippleEngine, PickleAI
- **Tests (1):** CoreTests (8 NUnit tests)

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

### Deferred (per Krieg Discipline)
- Weave / Sever / Listen / Read / Translate / Identify / Compose / Search / Negotiate / Compose Verse mini-games — Mission 3+.
- Procedural villagers beyond Doris+Gerrold+lane silent — Mission 3+.
- Predecessor (Marin) full arc, Echo Hologram, Locked Room, Forgotten Year, Vance Arc, Mariska — Mission 3+.
- Letter-Bird Network, Pen-Pal Villages, Dream Cinema community, ARG, Photo Mode — post-launch.
- Mobile IAP, gacha, energy — **never**.

---

*Format: [SemVer](https://semver.org/spec/v2.0.0.html). Versions advance to 0.3.0 when Mission 3 design begins; 1.0.0 when the 20-person greenlight playtest passes.*
