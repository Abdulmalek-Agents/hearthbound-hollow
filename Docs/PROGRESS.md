# 📊 Hearthbound Hollow — Implementation Progress Log

> Continuously updated as the studio builds Mission 1-2. Every PR appends to this file.

---

## Legend
- ✅ Done & merged to main
- 🟢 Done in working branch (awaiting your pull)
- 🟡 In progress
- ⬜ Not started
- 🔴 Blocked

---

## ✅ Phase 0 — Architecture & Skeleton

**Branch:** `feat/mission-1-2-architecture` · **PR:** #7
**Outcome:** Architecture, folder skeleton, 12 asmdefs, manifest patched.

| Task | Status |
|---|---|
| `Docs/ARCHITECTURE.md` | 🟢 |
| `Docs/PROGRESS.md` | 🟢 |
| `Docs/EXISTING_ASSETS_INDEX.md` | 🟢 |
| `Assets/_Project/` folder skeleton (23 subfolders) | 🟢 |
| 10 asmdef files + 2 test asmdefs | 🟢 |
| `Packages/manifest.json` patched | 🟢 |

---

## 🟢 Phase 1 — Core Systems

| Task | Status |
|---|---|
| `Hh.cs` (LogCategory + Hh.Log/Warn/Err) | 🟢 |
| `EventBus.cs` | 🟢 |
| `ServiceLocator.cs` | 🟢 |
| `GameEvents.cs` (12 readonly-struct event types) | 🟢 |
| `VillageState.cs` (14-dimension SO) | 🟢 |
| `MissionSO.cs` | 🟢 |
| `GameManager.cs` (bootstrap + scene loader) | 🟢 |
| `CoreTests.cs` (NUnit EditMode tests) | 🟢 |

---

## 🟢 Phase 2 — Memory Data Layer

| Task | Status |
|---|---|
| `EmotionalTone.cs` enum + palette helper | 🟢 |
| `MoralChoice.cs` + `CleanseOutcome.cs` enums | 🟢 |
| `VillagerSO.cs` | 🟢 |
| `MemoryNodeSO.cs` (canonical Memory Card schema) | 🟢 |
| `MemoryConnectionSO.cs` (Echo Web link) | 🟢 |
| `VillagerMemoryMapSO.cs` (node graph) | 🟢 |
| `MemoryHerb.cs` (Lavender + Valerian schema) | 🟢 |
| `TariffSO.cs` (per-choice tariff data) | 🟢 |

---

## 🟢 Phase 3 — Player + Interactions

| Task | Status |
|---|---|
| `Interactable.cs` (abstract base) | 🟢 |
| `PlayerController.cs` (Input System + legacy fallback) | 🟢 |
| `MemoryOrbInteractable.cs` (shader-driven clarity/crack/tint) | 🟢 |
| `HollowDoorInteractable.cs` | 🟢 |
| `HerbHarvestInteractable.cs` | 🟢 |
| `KettleInteractable.cs` | 🟢 |
| `DayCycleManager.cs` (sun + ambient curves) | 🟢 |
| `LumenLightController.cs` (time-of-day → Lumen intensities) | 🟢 |
| `InteractionPromptUI.cs` | 🟢 |

---

## 🟢 Phase 4 — Mini-Games

| Task | Status |
|---|---|
| `MiniGameBase.cs` (abstract state machine) | 🟢 |
| `PolishMiniGame.cs` (4-quadrant coverage + milestone + reveal + auto-complete) | 🟢 |
| `CleanseMiniGame.cs` (UV crack-trace + 4 outcomes + core protection) | 🟢 |

---

## 🟢 Phase 5 — UI

| Task | Status |
|---|---|
| `DialogueUI.cs` (Bamao parchment + typewriter + choices) | 🟢 |
| `ChoiceCardUI.cs` (4-option moral with tariff preview) | 🟢 |
| `EveningLedgerUI.cs` (day summary + 3 slots + autosave) | 🟢 |
| `TeaBrewingUI.cs` (herb pick + 90s timer + auto-complete) | 🟢 |
| `CodexUI.cs` (tooltip + Memory Map view) | 🟢 |
| `ComfortToolsMenu.cs` (Gentle Mode, AutoComplete toggles, color-blind, subtitle, one-hand) | 🟢 |
| `ToneCompassCard.cs` (first-launch primer) | 🟢 |
| `HUDController.cs` (day + coin + held memory) | 🟢 |
| `MainMenuController.cs` (Open The Hollow CTA) | 🟢 |

---

## 🟢 Phase 6 — Yarn Integration

| Task | Status |
|---|---|
| Yarn Spinner package install | ⬜ (user, after manifest pull) |
| `YarnVillageStateBridge.cs` (compile-guarded by YARN_SPINNER_PRESENT) | 🟢 |
| `Doris_M1.yarn` (9 nodes, 3 polish outcomes) | 🟢 |
| `Gerrold_M2.yarn` (10 nodes, 4 moral choices, 5 dream variants) | 🟢 |
| `Marin_Notes.yarn` (predecessor hooks) | 🟢 |
| `Pickle.yarn` (4 cat quotes) | 🟢 |
| `Codex.yarn` (6 examine tooltips) | 🟢 |

---

## 🟢 Phase 7 — Cutscenes

| Task | Status |
|---|---|
| `MemoryDreamSequencer.cs` (Dream 1 + 5 Dream 2 variants) | 🟢 |
| `ListenSceneSequencer.cs` | ⬜ (M2 wiring, Phase 9 scene step) |
| Timeline assets (Dream 1, Dream 2×5, Opening, Outro) | ⬜ (require Unity-side authoring) |

---

## 🟢 Phase 8 — Save + Ripple + Pickle + Mission

| Task | Status |
|---|---|
| `VillageStateSnapshot.cs` | 🟢 |
| `SaveService.cs` (atomic + power-fail safe) | 🟢 |
| `RippleEngine.cs` | 🟢 |
| `PickleAI.cs` | 🟢 |
| `MissionRunner.cs` (event-driven orchestration) | 🟢 |

---

## ⬜ Phase 9 — Scenes (Unity-side authoring)

Scenes require **Unity-side authoring** (not scriptable from headless tooling). The PR documents exactly what to drag into each scene. See `Docs/SCENE_ASSEMBLY_GUIDE.md` (added in this PR) for step-by-step.

| Task | Status |
|---|---|
| `00_Bootstrap.unity` | ⬜ (Unity-side) |
| `01_MainMenu.unity` | ⬜ (Unity-side) |
| `02_Mission01_Lane.unity` | ⬜ (Unity-side) |
| `03_Mission01_Hollow.unity` | ⬜ (Unity-side) |
| `04_Mission02_Garden.unity` | ⬜ (Unity-side) |
| `05_Mission02_Cottage.unity` | ⬜ (Unity-side) |
| `Scenes In Build` list updated | ⬜ (Unity-side) |

---

## 🟢 Phase 10 — QA & Polish

| Task | Status |
|---|---|
| Secret-scan all C# + .yarn files | 🟢 (no secrets) |
| EditMode unit tests (8 tests in CoreTests.cs) | 🟢 |
| PlayMode integration test | ⬜ (deferred — requires scenes) |
| README.md build section | 🟢 |
| CHANGELOG.md | 🟢 |
| Final PR to `main` | 🟡 (PR #7 open) |

---

## Decisions Made

| # | Decision | Date | Reason |
|---|---|---|---|
| D-001 | BoZo over City Characters | Phase 0 | Critic Board rec; cozy tone; ⅓ mobile cost |
| D-002 | Yarn Spinner over OpenAI addon | Phase 0 | GDD Pillar 1; hand-authored only |
| D-003 | Don't relocate existing vendor folders | Phase 0 | Preserves .meta GUIDs; avoids 5 GB reimport |
| D-004 | One asmdef per Scripts/ subfolder | Phase 0 | 80% faster iteration |
| D-005 | InteractionPromptUI lives in Player asmdef | Phase 5 | Avoids UI→Player dep cycle |
| D-006 | YarnVillageStateBridge compile-guarded by `YARN_SPINNER_PRESENT` | Phase 6 | Bridge compiles before Yarn package install |
| D-007 | Scene authoring is Unity-side, scripts are GitHub-side | Phase 9 | `.unity` files contain GUIDs to assets not yet on disk |

---

## Issue / Risk Log

| # | Item | Severity | Status |
|---|---|---|---|
| (none open) | | | |

---

## Files Landed (Phase 0–10)

**C# scripts:** 32 files, ~4 k lines
- Core (7): Hh, EventBus, ServiceLocator, GameEvents, VillageState, MissionSO, GameManager
- Memory (8): EmotionalTone, MoralChoice, VillagerSO, MemoryNodeSO, MemoryConnectionSO, VillagerMemoryMapSO, MemoryHerb, TariffSO
- Player (9): Interactable, PlayerController, MemoryOrbInteractable, HollowDoorInteractable, HerbHarvestInteractable, KettleInteractable, DayCycleManager, LumenLightController, InteractionPromptUI
- MiniGames (3): MiniGameBase, PolishMiniGame, CleanseMiniGame
- UI (9): DialogueUI, ChoiceCardUI, EveningLedgerUI, TeaBrewingUI, CodexUI, ComfortToolsMenu, ToneCompassCard, HUDController, MainMenuController
- Dialogue (1): YarnVillageStateBridge
- Cutscene (1): MemoryDreamSequencer
- Save (2): SaveService, VillageStateSnapshot
- Mission (3): MissionRunner, RippleEngine, PickleAI
- Tests (1): CoreTests (8 NUnit tests)

**Yarn scripts:** 5 files
- Doris_M1.yarn (9 nodes), Gerrold_M2.yarn (10 nodes), Marin_Notes.yarn (3 nodes), Pickle.yarn (4 nodes), Codex.yarn (6 nodes)

**Docs:** 4 files
- ARCHITECTURE.md, PROGRESS.md, EXISTING_ASSETS_INDEX.md, SCENE_ASSEMBLY_GUIDE.md (added in Phase 10)

**Asmdefs:** 12 files (10 product + 2 test)

---

*Last updated: end of Phase 10. PR #7 open.*
