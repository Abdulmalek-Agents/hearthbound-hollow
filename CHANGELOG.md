# Changelog — Hearthbound Hollow

All notable changes to this project will be documented here. Entries follow the [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) format.

## [0.1.0-mission-1-2-architecture] — 2026-05-23

**Branch:** `feat/mission-1-2-architecture` → PR #7 → main (pending merge)

### Added — Documentation
- `Docs/ARCHITECTURE.md` — 15-section implementation architecture (decisions D-001…D-007, asmdef graph, Mission 1-2 phased plan).
- `Docs/PROGRESS.md` — 10-phase tracker with per-task status.
- `Docs/EXISTING_ASSETS_INDEX.md` — Catalog of 17 imported asset packs with M1-2 mapping.
- `Docs/SCENE_ASSEMBLY_GUIDE.md` — Step-by-step Unity-side wiring instructions for the 6 mission scenes.

### Added — Package manifest
- URP 17.2.0, Cinemachine 3.1.4, TextMeshPro 3.0.9, Addressables 2.6.2, Visual Effect Graph 17.2.0, Timeline 1.8.7, Splines 2.7.2, 2D Sprite 1.0.0, 2D Animation 11.0.4, Test Framework 1.5.1.

### Added — Folder skeleton
- `Assets/_Project/` with 23 subfolders (Art, Audio, Animations, Prefabs, Scenes, Scripts, ScriptableObjects, Settings, Yarn, Tests). Empty folders are preserved via `.gitkeep`.

### Added — Assembly definitions (12 files)
- 10 production asmdefs (Core, Memory, Save, Audio, Player, MiniGames, UI, Dialogue, Cutscene, Mission) with full dependency graph.
- 2 test asmdefs (EditMode + PlayMode), guarded by `UNITY_INCLUDE_TESTS`.

### Added — Core systems (Phase 1)
- `Hh.cs` — Logging helper with 14 LogCategory enum entries.
- `EventBus.cs` — Zero-allocation typed pub/sub with snapshot-safe dispatch.
- `ServiceLocator.cs` — Lightweight runtime DI registry.
- `GameEvents.cs` — 12 readonly-struct event payload types.
- `VillageState.cs` — ScriptableObject with **full 14-dimension Codex 08 schema** (4 active in M1-2, 10 dormant for M3+).
- `MissionSO.cs` — 10-mission scalable schema with objectives + outro hooks.
- `GameManager.cs` — Bootstrap singleton with SceneManager scene transitions.

### Added — Memory data layer (Phase 2)
- `EmotionalTone.cs` — 7 canonical tones + palette tint extension.
- `MoralChoice.cs` — Erase / Cleanse / Listen / Defer.
- `CleanseOutcome.cs` — Perfect / Acceptable / Sloppy / CrossedCore.
- `VillagerSO.cs` — Identity + portrait + voice motif + Yarn entry node + Eyes Animator hints.
- `MemoryNodeSO.cs` — Canonical Codex 02 § 2.1 Memory Card schema.
- `MemoryConnectionSO.cs` — Echo Web bidirectional link.
- `VillagerMemoryMapSO.cs` — Per-villager node graph + runtime reveal state.
- `MemoryHerb.cs` — Lavender + Valerian schema (supports 18-herb canonical garden).
- `TariffSO.cs` — Per-choice tariff (Codex 10 § 3).

### Added — Player + Interactions (Phase 3)
- `Interactable.cs` — Abstract base with worldspace prompt + Activate contract.
- `PlayerController.cs` — Walk + interact, Input System primary + legacy axes fallback.
- `MemoryOrbInteractable.cs` — Shader-driven clarity/crack/tint via MaterialPropertyBlock.
- `HollowDoorInteractable.cs` — Door animation + delayed scene load.
- `HerbHarvestInteractable.cs` — Garden harvest → HerbHarvestedEvent.
- `KettleInteractable.cs` — Tea brewing trigger.
- `DayCycleManager.cs` — Sun rotation + intensity + ambient color curves.
- `LumenLightController.cs` — Time-of-day → stylized-light material intensities.
- `InteractionPromptUI.cs` — Worldspace prompt follower (lives in Player asmdef per D-005).

### Added — Mini-games (Phase 4)
- `MiniGameBase.cs` — Abstract state machine + Auto-Complete contract.
- `PolishMiniGame.cs` — 4-quadrant coverage + milestone @ 0.55 + reveal swell @ 0.85 + auto-complete; cannot fail (M1 tutorial-tier).
- `CleanseMiniGame.cs` — UV-space crack-trace + core protection + 4 outcomes; narratively absorbs failure (Focus 04 § 3.5).

### Added — UI (Phase 5)
- `DialogueUI.cs` (with `IDialoguePresenter` interface), `ChoiceCardUI.cs`, `EveningLedgerUI.cs`, `TeaBrewingUI.cs`, `CodexUI.cs`, `ComfortToolsMenu.cs`, `ToneCompassCard.cs`, `HUDController.cs`, `MainMenuController.cs`.

### Added — Dialogue (Phase 6)
- `YarnVillageStateBridge.cs` — Compile-guarded by `YARN_SPINNER_PRESENT`. Stub mode when Yarn Spinner not installed.
- Yarn dialogue files: `Doris_M1.yarn` (9 nodes), `Gerrold_M2.yarn` (10 nodes), `Marin_Notes.yarn` (3 nodes), `Pickle.yarn` (4 nodes), `Codex.yarn` (6 nodes).

### Added — Cutscene (Phase 7)
- `MemoryDreamSequencer.cs` — Timeline-driven Dream 1 single + Dream 2 five-variant playback keyed off MoralChoice + CleanseOutcome.

### Added — Save + Mission + Pickle (Phase 8)
- `VillageStateSnapshot.cs` — JSON-serializable mirror of VillageState.
- `SaveService.cs` — Atomic write (tmp → fsync → rename); 3 manual slots + autosave; power-fail safe.
- `RippleEngine.cs` — Tariff propagation (2-villager radius for M1-2).
- `PickleAI.cs` — Head look-at + tail-flick + 4 scripted lines.
- `MissionRunner.cs` — Event-driven per-mission orchestration + autosave on moral choice.

### Added — Tests
- `CoreTests.cs` — 8 NUnit EditMode tests covering EventBus (3), VillageState (2), ServiceLocator (3).

### Decisions adopted
- **D-001** BoZo (chibi) over City Characters — Critic Board rec.
- **D-002** Yarn Spinner over OpenAI Dialogue addon — preserves GDD Pillar 1.
- **D-003** Don't relocate existing vendor folders — preserves .meta GUIDs.
- **D-004** One asmdef per Scripts/ subfolder.
- **D-005** InteractionPromptUI lives in Player asmdef to avoid UI→Player dep cycle.
- **D-006** YarnVillageStateBridge compile-guarded by `YARN_SPINNER_PRESENT`.
- **D-007** Scenes are Unity-side; scripts are GitHub-side.

### Deferred (per Krieg Discipline)
- Weave / Sever / Listen / Read / Translate / Identify / Compose / Search / Negotiate / Compose Verse mini-games — Mission 3+.
- Procedural villagers, Marin arc, Echo Hologram, Locked Room, Forgotten Year, Vance Arc, Mariska — Mission 3+.
- Letter-Bird Network, Pen-Pal Villages, Dream Cinema community, ARG, Photo Mode — post-launch.
- Mobile IAP, gacha, energy — never (per GDD § 7).

### Files & line counts
- 32 C# files, ~4,000 lines.
- 5 Yarn dialogue files, ~6 k chars.
- 12 asmdef files.
- 4 docs.

### Known follow-ups
- Phase 9 scenes (Unity-side) — guidance in `Docs/SCENE_ASSEMBLY_GUIDE.md`.
- Optional asset imports (VoluSmokeFX, Skill Tree Builder, Colorize, LightMap Fusion Pro, Microdetail Terrain) — substitutes exist; cosmetic at the M1-2 scope.
- Composer cues + VO casting — out of engineering scope.

---

*Format: [SemVer](https://semver.org/spec/v2.0.0.html). Versions advance to 0.2.0 when Mission 3 design begins; 1.0.0 when the 20-person greenlight playtest passes.*
