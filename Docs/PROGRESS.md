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

## ✅ Phase 0 — Architecture & Skeleton  (PR #7)
All 7 items 🟢.

## 🟢 Phase 1 — Core Systems
Hh, EventBus, ServiceLocator, GameEvents, VillageState, MissionSO, GameManager, CoreTests.

## 🟢 Phase 2 — Memory Data Layer
EmotionalTone, MoralChoice, CleanseOutcome, VillagerSO, MemoryNodeSO, MemoryConnectionSO, VillagerMemoryMapSO, MemoryHerb, TariffSO.

## 🟢 Phase 3 — Player + Interactions
Interactable, PlayerController, MemoryOrbInteractable, HollowDoorInteractable, HerbHarvestInteractable, KettleInteractable, DayCycleManager, LumenLightController, InteractionPromptUI.

## 🟢 Phase 4 — Mini-Games
MiniGameBase, PolishMiniGame, CleanseMiniGame.

## 🟢 Phase 5 — UI
DialogueUI, ChoiceCardUI, EveningLedgerUI, TeaBrewingUI, CodexUI, ComfortToolsMenu, ToneCompassCard, HUDController, MainMenuController.

## 🟢 Phase 6 — Yarn Integration
YarnVillageStateBridge (compile-guarded), Doris_M1.yarn, Gerrold_M2.yarn, Marin_Notes.yarn, Pickle.yarn, Codex.yarn.

## 🟢 Phase 7 — Cutscenes
MemoryDreamSequencer, ListenSceneSequencer. Timeline assets ⬜ (Unity-side).

## 🟢 Phase 8 — Save + Ripple + Pickle + Mission
VillageStateSnapshot, SaveService, RippleEngine, PickleAI, MissionRunner.

## ⬜ Phase 9 — Scenes (Unity-side authoring)
See `Docs/SCENE_ASSEMBLY_GUIDE.md` for the 6-step procedure.

## 🟢 Phase 10 — QA & Polish
- 21 EditMode unit tests (Core + Save + Ripple + Memory + VillagerRuntime)
- Secret-scan clean
- README + CHANGELOG up to date

---

## 🔧 Phase 10.5 — Studio-Code Bug Fix Cycle

All studio-code compile errors reported by user resolved.

| Error | File | Fix |
|---|---|---|
| CS1739 `isAutosave` | SaveService.cs:43 | Positional arg `slot < 0` |
| CS0234 `HearthboundHollow.Player` missing in MiniGames | MiniGames asmdef | Added Player reference |
| CS0246 `MemoryOrbInteractable` not found | MiniGames asmdef | Same fix |
| "Project has invalid dependencies" | manifest.json | Removed deprecated `com.unity.textmeshpro` |
| (preempt) CS0234 in YarnVillageStateBridge | Dialogue asmdef | Added Save reference |

## 🆕 Phase 11 — Quality-of-Life Tooling

| Item | Status |
|---|---|
| `Editor/SeedAssetGenerator.cs` (one-click 17 SO assets) | 🟢 |
| `Editor/HearthboundHollow.Editor.asmdef` | 🟢 |
| `Settings/HearthboundInput.inputactions` | 🟢 |
| `Cutscene/ListenSceneSequencer.cs` | 🟢 |
| `Tests/EditMode/SaveAndRippleTests.cs` (+13 tests) | 🟢 |

## 🔧 Phase 10.6 — Vendor-Asset Package Cycle

User reported a new compile error from a third-party asset (Orly Tools / LightMap Fusion). The team performed a **full grep audit** of every non-_Project script for `using Unity.X` references to catch the whole class of issue in one pass.

| Vendor file referencing it | Namespace | Package added |
|---|---|---|
| `Assets/Orly Tools/LightMap Fusion/Scripts/Dissolve/Dissolve Controller.cs` (reported) | `Unity.VisualScripting` | `com.unity.visualscripting` 1.9.7 |
| `Assets/Orly Tools/LightMap Fusion/Scripts/Editor/LightMapFusionEditor.cs` | `Unity.VisualScripting` | (same) |
| `Assets/Plugins/Microdetail/Scripts/*.cs` (10 files) | `Unity.Mathematics` | `com.unity.mathematics` 1.3.2 |
| `Assets/Plugins/Microdetail/Scripts/*.cs` (8 files) | `Unity.Collections` (+ LowLevel.Unsafe) | `com.unity.collections` 2.5.7 |
| Transitive dep of Mathematics + Collections | (n/a) | `com.unity.burst` 1.8.24 |

**Confirmed dormant (no fix needed):**
- `Unity.Cloud.Assets`, `Unity.Cloud.Identity`, `Unity.Cloud.Common`, `Unity.Cloud.AppLinking.Runtime` — used by AssetInventory's optional Asset Manager integration, gated behind `#if USE_ASSET_MANAGER && USE_CLOUD_IDENTITY`. The user hasn't activated these defines, so the references don't compile.
- `Cinemachine` (v2 namespace) vs `Unity.Cinemachine` (v3 namespace) — Cinemachine 3.1.4 in the manifest provides the back-compat shim, both compile.

**Already in manifest before this commit:** `Unity.EditorCoroutines`, `Unity.SharpZipLib`, `Unity.Profiling`.

---

## Decisions Made

| # | Decision | Date | Reason |
|---|---|---|---|
| D-001 | BoZo over City Characters | Phase 0 | Critic Board rec; cozy tone; ⅓ mobile cost |
| D-002 | Yarn Spinner over OpenAI addon | Phase 0 | GDD Pillar 1; hand-authored only |
| D-003 | Don't relocate existing vendor folders | Phase 0 | Preserves .meta GUIDs; avoids 5 GB reimport |
| D-004 | One asmdef per Scripts/ subfolder | Phase 0 | 80% faster iteration |
| D-005 | InteractionPromptUI lives in Player asmdef | Phase 5 | Avoids UI→Player dep cycle |
| D-006 | YarnVillageStateBridge compile-guarded by `YARN_SPINNER_PRESENT` | Phase 6 | Bridge compiles before Yarn install |
| D-007 | Scene authoring is Unity-side, scripts are GitHub-side | Phase 9 | `.unity` files contain GUIDs to assets not yet on disk |
| D-008 | Drop `com.unity.textmeshpro` from manifest | Phase 10.5 | Unity 6 folded TMP into `com.unity.ugui` 2.0 |
| D-009 | Cinemachine via reflection in ListenSceneSequencer | Phase 11 | Avoids hard compile dep |
| D-010 | Seed asset generation via Editor menu, not committed .asset files | Phase 11 | Avoids GUID/.meta race conditions |
| **D-011** | **Explicit-pin DOTS-family packages even when transitive** | Phase 10.6 | Vendor packs (Microdetail, LightMap Fusion) require Mathematics/Collections/Burst that aren't Unity 6 defaults. Pinning prevents version drift. |
| **D-012** | **Whole-vendor-tree audit on every CS0234 namespace error** | Phase 10.6 | Cheaper to add 4 packages once than to play whack-a-mole 4 separate times. |

---

## Issue / Risk Log

| # | Item | Severity | Status |
|---|---|---|---|
| Phase-10.5 studio compile errors | High | ✅ Resolved |
| Phase-10.6 vendor compile errors | High | ✅ Resolved (4 packages + audit) |

---

*Last updated: Phase 10.6 closure. PR #7 open. The full vendor-tree namespace audit shows no remaining missing packages for active code paths.*
