# 📊 Hearthbound Hollow — Implementation Progress Log

> Continuously updated as the studio builds Mission 1-2. Every PR appends to this file.

---

## Legend
- ✅ Done & merged to main · 🟢 Done in branch (awaiting your pull) · 🟡 In progress · ⬜ Not started · 🔴 Blocked

---

## ✅ Phase 0 — Architecture & Skeleton  (PR #7) — All 7 items 🟢.
## 🟢 Phase 1 — Core Systems
## 🟢 Phase 2 — Memory Data Layer
## 🟢 Phase 3 — Player + Interactions
## 🟢 Phase 4 — Mini-Games
## 🟢 Phase 5 — UI
## 🟢 Phase 6 — Yarn Integration
## 🟢 Phase 7 — Cutscenes (Memory Dream + Listen sequencers)
## 🟢 Phase 8 — Save + Ripple + Pickle + Mission
## ⬜ Phase 9 — Scenes (Unity-side authoring — `Docs/SCENE_ASSEMBLY_GUIDE.md`)
## 🟢 Phase 10 — QA & Polish (21 EditMode tests, secret-scan clean)

---

## 🔧 Phase 10.5 — Studio-Code Bug Fix Cycle  ✅ Resolved
- CS1739 SaveService → positional arg
- CS0234/CS0246 in MiniGames → added Player asmdef ref
- "Invalid dependencies" → removed deprecated `com.unity.textmeshpro`
- Preempt CS0234 in Yarn bridge → added Save asmdef ref

## 🆕 Phase 11 — Quality-of-Life Tooling  ✅
- `Editor/SeedAssetGenerator.cs` (17 SO assets, one click)
- `Editor/HearthboundHollow.Editor.asmdef`
- `Settings/HearthboundInput.inputactions`
- `Cutscene/ListenSceneSequencer.cs`
- `Tests/EditMode/SaveAndRippleTests.cs` (+13 tests)

## 🔧 Phase 10.6 — Vendor-Asset Package Cycle  ✅ Resolved
Whole-vendor-tree audit. Added: `com.unity.visualscripting 1.9.7`, `com.unity.mathematics 1.3.2`, `com.unity.collections 2.5.7`, `com.unity.burst 1.8.24`. Confirmed dormant: `Unity.Cloud.*` (#if-guarded), Cinemachine v2/v3 (back-compat shim).

## 🔧 Phase 10.7 — Render Pipeline Activation  🟢 Awaiting menu run

User reported: *"Microdetail doesn't support built-in and custom render pipelines."*

**Root cause:** URP package is installed (`com.unity.render-pipelines.universal 17.2.0`), but no URP asset is assigned to `GraphicsSettings.defaultRenderPipeline`. The project is silently running on Built-in pipeline. Microdetail's `SetupWizard.UpdateShaders()` detects this and correctly errors.

**Fix:** Added `Editor/URPSetupHelper.cs` with a one-click menu item that:
1. Creates `Assets/_Project/Settings/URP-MobileRenderer.asset` if missing.
2. Creates `Assets/_Project/Settings/URP-Mobile.asset` if missing, wired to the renderer with mobile-friendly defaults (render scale 0.85, 2× MSAA, soft shadows off, per-pixel lights, HDR off).
3. Assigns the URP asset to `GraphicsSettings.defaultRenderPipeline`.
4. Assigns to every `QualitySettings` level.
5. Saves the project.

**Menu path:** `Hearthbound → Setup URP Pipeline (one-time)`

Idempotent: re-running reuses existing assets. If auto-create fails, falls back to a dialog instructing manual creation via Unity's `Project → Create → Rendering → URP Asset (with Universal Renderer)`.

**Asmdef change:** Editor asmdef now references `Unity.RenderPipelines.Core.Runtime` + `.Editor` and `Unity.RenderPipelines.Universal.Runtime` + `.Editor` — packages already in manifest, just needs the asmdef line items.

After the menu runs:
- Microdetail SetupWizard stops complaining ✅
- All URP-only shaders (AllIn1ShaderNodes, Bamao UI, Heat UI, Lumen, Stylized Weather, etc.) work correctly ✅
- Existing built-in materials may render pink until you run `Edit → Rendering → Materials → Convert All Built-in Materials to URP` (one-time conversion).

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
| D-011 | Explicit-pin DOTS-family packages even when transitive | Phase 10.6 | Vendor packs need them; pin prevents version drift |
| D-012 | Whole-vendor-tree audit on every CS0234 namespace error | Phase 10.6 | Cheaper to add N packages once than to whack-a-mole |
| **D-013** | **URP Render Pipeline asset auto-created + assigned via Editor menu** | Phase 10.7 | Microdetail (and 5+ other URP-only assets) require URP active; manual Project Settings juggling is error-prone |
| **D-014** | **Mobile-friendly URP defaults (render scale 0.85, 2× MSAA, soft shadows off)** | Phase 10.7 | Matches ARCHITECTURE.md § 10 mobile budget; matches Asset Analysis § 9 URP-Mobile spec |

---

## Issue / Risk Log

| # | Item | Severity | Status |
|---|---|---|---|
| Phase-10.5 studio compile errors | High | ✅ Resolved |
| Phase-10.6 vendor compile errors | High | ✅ Resolved (4 packages + audit) |
| Phase-10.7 Microdetail SetupWizard error | Medium | 🟢 Resolved (run `Hearthbound → Setup URP Pipeline` menu) |

---

## Editor Menu Items Available (cumulative)

| Menu Path | Purpose | Phase |
|---|---|---|
| Hearthbound → Setup URP Pipeline (one-time) | Activate URP + create asset bundle | 10.7 |
| Hearthbound → Create Mission 1-2 Seed Assets | Spawn all 17 ScriptableObject seed assets | 11 |
| Hearthbound → Validate Mission 1-2 Seed Assets | Audit which seed assets are missing | 11 |

---

*Last updated: Phase 10.7 — URP activation menu landed. PR #7 open.*
