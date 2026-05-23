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
(Yarn Spinner package install ⬜ — user, after pull.)

## 🟢 Phase 7 — Cutscenes
MemoryDreamSequencer, **ListenSceneSequencer (NEW)**. Timeline assets ⬜ (Unity-side).

## 🟢 Phase 8 — Save + Ripple + Pickle + Mission
VillageStateSnapshot, SaveService, RippleEngine, PickleAI, MissionRunner.

## ⬜ Phase 9 — Scenes (Unity-side authoring)
See `Docs/SCENE_ASSEMBLY_GUIDE.md` for the 6-step procedure (~10–20 min per scene).
**Now made easier by the new `Hearthbound → Create Mission 1-2 Seed Assets` Editor menu.**

---

## 🟢 Phase 10 — QA & Polish

| Task | Status |
|---|---|
| Secret-scan all C# + .yarn files | 🟢 (no secrets) |
| EditMode unit tests | 🟢 **21 tests** (was 8) |
| PlayMode integration test | ⬜ (deferred — requires scenes) |
| README.md build section | 🟢 |
| CHANGELOG.md | 🟢 |
| Final PR to `main` | 🟡 (PR #7 open) |

---

## 🔧 Phase 10.5 — Bug Fix Cycle  (NEW)

Console errors reported after first pull. All resolved in this commit cluster.

| # | Error | Resolution |
|---|---|---|
| CS1739 in SaveService.cs:43 | `isAutosave` named arg didn't exist on event ctor | Changed to positional argument `slot < 0` |
| CS0234 `Player` ns missing in MiniGames | MiniGames asmdef lacked `HearthboundHollow.Player` reference | Added Player ref to MiniGames asmdef |
| CS0246 `MemoryOrbInteractable` not found | Same root cause as CS0234 | Same fix |
| "Project has invalid dependencies" at editor boot | `com.unity.textmeshpro 3.0.9` deprecated in Unity 6 (folded into ugui 2.0) | Removed from manifest.json |
| (preempt) CS0234 in YarnVillageStateBridge | When user installs Yarn, the SaveService dep wouldn't resolve | Added `HearthboundHollow.Save` to Dialogue asmdef |

---

## 🆕 Phase 11 — Quality-of-Life Tooling

| Item | Status | Notes |
|---|---|---|
| `Editor/SeedAssetGenerator.cs` | 🟢 | One-click menu creates all 17 ScriptableObject seed assets (DOR-001, GER-007, Doris, Gerrold, etc.) — saves ~30 min of right-clicking |
| `Editor/HearthboundHollow.Editor.asmdef` | 🟢 | Editor-only asmdef so the menu code is excluded from runtime builds |
| `Settings/HearthboundInput.inputactions` | 🟢 | Pre-configured input actions (Move, Interact, PointerPosition, PointerActive, Pause × 3 control schemes: Keyboard&Mouse, Gamepad, Touch) — drop on Player prefab |
| `Cutscene/ListenSceneSequencer.cs` | 🟢 | Mission 2 Listen path Timeline driver — was missing from Phase 7 |
| `Tests/EditMode/SaveAndRippleTests.cs` | 🟢 | +13 NUnit tests (Save round-trip × 5, RippleEngine × 4, MemoryNode × 2, VillagerRuntime × 2). Total EditMode coverage: 21 tests |

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
| **D-008** | **Drop `com.unity.textmeshpro` from manifest** | Phase 10.5 | Unity 6 folded TMP into `com.unity.ugui` 2.0; standalone package no longer exists |
| **D-009** | **Cinemachine integration via reflection in ListenSceneSequencer** | Phase 11 | Avoids hard compile dep on `com.unity.cinemachine` for the Cutscene asmdef |
| **D-010** | **Seed asset generation via Editor menu, not committed .asset files** | Phase 11 | Avoids GUID/.meta race conditions; idempotent + safe to re-run |

---

## Issue / Risk Log

| # | Item | Severity | Status |
|---|---|---|---|
| All Phase-10.5 compile errors | High | ✅ Resolved |

---

## Files Landed (Phase 0–11)

**C# scripts:** 35 files (+3: ListenSceneSequencer, SeedAssetGenerator, SaveAndRippleTests), ~5 k lines
**Asmdefs:** 13 (+1: HearthboundHollow.Editor)
**Yarn dialogue:** 5 files unchanged
**Input Actions:** 1 file (NEW) — `HearthboundInput.inputactions`
**Docs:** 6 (ARCHITECTURE, PROGRESS, EXISTING_ASSETS_INDEX, SCENE_ASSEMBLY_GUIDE, CHANGELOG, and this Bug Fix Cycle section)

---

*Last updated: end of Phase 10.5 + Phase 11. PR #7 open with 4 compile-error fixes + 5 QoL additions.*
