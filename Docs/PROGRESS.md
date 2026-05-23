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

## Phase 0 — Architecture & Skeleton  🟡

**Branch:** `feat/mission-1-2-architecture` (off `docs/asset-analysis-mission-1-2`)
**Goal:** Land the architecture doc, this progress tracker, the folder skeleton, asmdef files, manifest.json additions. Zero new C# logic yet.

| Task | Status |
|---|---|
| `Docs/ARCHITECTURE.md` | 🟢 |
| `Docs/PROGRESS.md` (this file) | 🟢 |
| `Docs/EXISTING_ASSETS_INDEX.md` | 🟢 |
| `Assets/_Project/` folder skeleton (23 subfolders) | 🟢 |
| 10 asmdef files (compile dependency graph) | 🟢 |
| 2 test asmdefs (EditMode + PlayMode) | 🟢 |
| `Packages/manifest.json` patched | 🟢 |
| URP-Mobile RP asset stubs | ⬜ (Phase 3) |

**Pull instruction for user (after Phase 0 merge):**
1. Pull `feat/mission-1-2-architecture` branch
2. Open Unity → it will prompt to install the new packages from `manifest.json`
3. Accept; Unity downloads URP, Cinemachine, TMP, Addressables, VFX Graph, Splines, 2D Sprite/Animation
4. After import settles, you're done. **No scene or prefab references these packages yet** — so no NRE on first compile.

---

## Phase 1 — Core Systems  ⬜

| Task | Status |
|---|---|
| `EventBus.cs` (generic publish/subscribe) | ⬜ |
| `ServiceLocator.cs` | ⬜ |
| `GameManager.cs` (bootstrap, scene loader) | ⬜ |
| `VillageState.cs` (ScriptableObject — 14 dimensions) | ⬜ |
| `MissionSO.cs` | ⬜ |
| `GameEvents.cs` | ⬜ |
| `Hh.Log()` wrapper | ⬜ |

---

## Phase 2 — Memory Data Layer  ⬜

| Task | Status |
|---|---|
| `EmotionalTone.cs` enum | ⬜ |
| `MemoryNodeSO.cs` | ⬜ |
| `MemoryConnectionSO.cs` | ⬜ |
| `VillagerSO.cs` | ⬜ |
| `VillagerMemoryMapSO.cs` | ⬜ |
| `MemoryHerb.cs` | ⬜ |
| `TariffSO.cs` | ⬜ |
| `MoralChoice.cs` + `CleanseOutcome.cs` enums | ⬜ |

---

## Phase 3 — Player + Interactions  ⬜

| Task | Status |
|---|---|
| `PlayerController.cs` | ⬜ |
| `Interactable.cs` base class | ⬜ |
| `MemoryOrbInteractable.cs` | ⬜ |
| `HollowDoorInteractable.cs` | ⬜ |
| `HerbHarvestInteractable.cs` | ⬜ |
| `KettleInteractable.cs` | ⬜ |
| `DayCycleManager.cs` | ⬜ |
| `LumenLightController.cs` | ⬜ |
| `InteractionPrompt.cs` | ⬜ |
| `HearthboundInput.inputactions` | ⬜ |

---

## Phase 4 — Mini-Games  ⬜

| Task | Status |
|---|---|
| `MiniGameBase.cs` | ⬜ |
| `PolishMiniGame.cs` | ⬜ |
| `CleanseMiniGame.cs` | ⬜ |
| `CleanseDifficultyProfile.cs` SO | ⬜ |
| `AutoCompleteButton.cs` | ⬜ |

---

## Phase 5 — UI  ⬜

| Task | Status |
|---|---|
| `DialogueUI.cs` | ⬜ |
| `ChoiceCardUI.cs` | ⬜ |
| `EveningLedgerUI.cs` | ⬜ |
| `TeaBrewingUI.cs` | ⬜ |
| `CodexUI.cs` | ⬜ |
| `ComfortToolsMenu.cs` | ⬜ |
| `ToneCompassCard.cs` | ⬜ |
| `HUDController.cs` | ⬜ |
| `MainMenuController.cs` | ⬜ |

---

## Phase 6 — Yarn Integration  ⬜

| Task | Status |
|---|---|
| Yarn Spinner via Packages git URL | ⬜ |
| `YarnVillageStateBridge.cs` | ⬜ |
| `YarnCustomCommands.cs` | ⬜ |
| `Doris_M1.yarn` | ⬜ |
| `Gerrold_M2.yarn` | ⬜ |
| `Pickle.yarn` | ⬜ |
| `Codex.yarn` | ⬜ |
| `Marin_Notes.yarn` | ⬜ |

---

## Phase 7 — Cutscenes  ⬜

| Task | Status |
|---|---|
| `MemoryDreamSequencer.cs` | ⬜ |
| `ListenSceneSequencer.cs` | ⬜ |
| Timeline assets (Opening, Dream1, Dream2 x5, Outro) | ⬜ |

---

## Phase 8 — Save + Ripple + Pickle  ⬜

| Task | Status |
|---|---|
| `SaveService.cs` | ⬜ |
| `VillageStateSnapshot.cs` | ⬜ |
| `RippleEngine.cs` | ⬜ |
| `PickleAI.cs` | ⬜ |

---

## Phase 9 — Scenes  ⬜

| Task | Status |
|---|---|
| `00_Bootstrap.unity` | ⬜ |
| `01_MainMenu.unity` | ⬜ |
| `02_Mission01_Lane.unity` | ⬜ |
| `03_Mission01_Hollow.unity` | ⬜ |
| `04_Mission02_Garden.unity` | ⬜ |
| `05_Mission02_Cottage.unity` | ⬜ |

---

## Phase 10 — QA & Polish  ⬜

| Task | Status |
|---|---|
| Secret-scan all C# files | ⬜ |
| EditMode unit tests | ⬜ |
| PlayMode integration test | ⬜ |
| README.md updated | ⬜ |
| CHANGELOG.md | ⬜ |
| Open PR to `main` | ⬜ |

---

## Decisions Made

| # | Decision | Date | Reason |
|---|---|---|---|
| D-001 | BoZo over City Characters | Phase 0 | Critic Board rec; cozy tone; ⅓ mobile cost |
| D-002 | Yarn Spinner over OpenAI addon | Phase 0 | GDD Pillar 1; hand-authored only |
| D-003 | Don't relocate existing vendor folders | Phase 0 | Preserves .meta GUIDs; avoids 5 GB reimport |
| D-004 | One asmdef per Scripts/ subfolder | Phase 0 | 80% faster iteration |

---

## Issue / Risk Log

| # | Item | Severity | Status |
|---|---|---|---|
| (none open) | | | |

---

*Last updated: Phase 0 init. Next update: when Phase 1 commits land.*
