# 📊 Hearthbound Hollow — Implementation Progress Log

> Continuously updated as the studio builds Mission 1-2. Every PR appends to this file.
> **Unity Editor version: 6000.4.4f1 (Unity 6 LTS)**

---

## Legend
- ✅ Done & merged to main · 🟢 Done in branch (awaiting your pull) · 🟡 In progress · ⬜ Not started · 🔴 Blocked

---

## ✅ Phase 0 — Architecture & Skeleton  (PR #7) — All 7 items 🟢.
## 🟢 Phase 1–8 — All implementation phases complete.
## ⬜ Phase 9 — Scenes (Unity-side authoring — `Docs/SCENE_ASSEMBLY_GUIDE.md`)
## 🟢 Phase 10 — QA & Polish (21 EditMode tests, secret-scan clean)

---

## 🔧 Phase 10.5 — Studio-Code Bug Fix Cycle  ✅ Resolved
- CS1739, CS0234/CS0246 in MiniGames, "Invalid dependencies", preempt CS0234 in Yarn bridge.

## 🆕 Phase 11 — Quality-of-Life Tooling  ✅
- SeedAssetGenerator (17 SOs), HearthboundInput.inputactions, ListenSceneSequencer, +13 tests.

## 🔧 Phase 10.6 — Vendor-Asset Package Cycle  ✅ Resolved
- Added: `com.unity.visualscripting 1.9.7`, `mathematics 1.3.2`, `collections 2.5.7`, `burst 1.8.24`.

## 🔧 Phase 10.7 — Render Pipeline Activation  🟢 Awaiting menu run
- `URPSetupHelper.cs` Editor menu auto-creates URP-Mobile asset + renderer, assigns to GraphicsSettings + all QualitySettings levels.

## 🔧 Phase 10.8 — Render Pipeline Auto-Prompt + Shader Patch  🟢

**Two persisting issues addressed:**

### 🅰 Microdetail SetupWizard error (persisted)

Root cause was: the user pulled the script but didn't run the menu, so URP was still inactive on next compile.

**New approach** — `URPSetupHelper` now has:
1. `[InitializeOnLoad]` startup hook — on every Editor open (or script recompile), checks `GraphicsSettings.defaultRenderPipeline`. If it's not a `UniversalRenderPipelineAsset`, surfaces a 3-button dialog:
   - "Set up URP now" → runs the helper immediately
   - "Don't ask again" → persists in EditorPrefs
   - "Skip for now" → defers
2. New diagnostic menu — `Hearthbound → Check Render Pipeline Status` — prints the current GraphicsSettings + per-QualityLevel render pipeline bindings.
3. Stronger activation — sets `defaultRenderPipeline = null` then back to the URP asset to force a pipeline reload (catches caching).

**Also corrected the post-setup dialog instruction text for Unity 6.0.4:**
- Old (Unity 2022 legacy): "Edit → Rendering → Materials → Convert All Built-in Materials to URP"
- **New (Unity 6.0.4 correct):** "Window → Rendering → Render Pipeline Converter → Built-in to URP → Material Upgrade → Convert Assets"

### 🅱 BM_Lit shader duplicate `_SHADOWS_SOFT` warning

```
Shader error in 'Hidden/Universal/BM_Lit': Keyword '_SHADOWS_SOFT' is
duplicated in several directives.
```

Root cause: LightMap Fusion's `BM_Lit.shader` (Amplify Shader Editor template) ships with both the pre-URP-14.0.9 and >=URP-14.0.9 `_SHADOWS_SOFT` multi_compile directives, gated by `/*ase_srp_cond_*/` comments that Unity treats as plain C-style comments — both compile.

**Fix:** `Editor/BMLitShaderPatcher.cs` — `AssetPostprocessor` that patches the shader on import:
- Generic — scans any `.shader` file for the legacy `<140009` block and removes it.
- Idempotent — safe to re-run; checks if pattern is still present.
- Handles both CRLF (vendor default) and LF (some git checkouts).
- Survives vendor updates — re-applies on the next import.

Manual menu: `Hearthbound → Patch ASE Shaders Now` — scans project-wide and patches any matches.

---

## Decisions Made

| # | Decision | Phase | Reason |
|---|---|---|---|
| D-001 | BoZo over City Characters | 0 | Critic Board rec; cozy tone; ⅓ mobile cost |
| D-002 | Yarn Spinner over OpenAI addon | 0 | GDD Pillar 1; hand-authored only |
| D-003 | Don't relocate existing vendor folders | 0 | Preserves .meta GUIDs; avoids 5 GB reimport |
| D-004 | One asmdef per Scripts/ subfolder | 0 | 80% faster iteration |
| D-005 | InteractionPromptUI lives in Player asmdef | 5 | Avoids UI→Player dep cycle |
| D-006 | YarnVillageStateBridge compile-guarded | 6 | Bridge compiles before Yarn install |
| D-007 | Scene authoring is Unity-side, scripts are GitHub-side | 9 | `.unity` files reference GUIDs not yet on disk |
| D-008 | Drop `com.unity.textmeshpro` from manifest | 10.5 | Unity 6 folded TMP into `com.unity.ugui` 2.0 |
| D-009 | Cinemachine via reflection in ListenSceneSequencer | 11 | Avoids hard compile dep |
| D-010 | Seed asset generation via Editor menu, not committed .asset files | 11 | Avoids GUID/.meta race conditions |
| D-011 | Explicit-pin DOTS-family packages | 10.6 | Vendor packs need them; pin prevents drift |
| D-012 | Whole-vendor-tree audit on every CS0234 | 10.6 | Add N packages once vs whack-a-mole |
| D-013 | URP Render Pipeline asset auto-created via Editor menu | 10.7 | Manual Project Settings juggling is error-prone |
| D-014 | Mobile-friendly URP defaults (0.85 scale, 2× MSAA, soft shadows off) | 10.7 | Matches ARCHITECTURE.md § 10 mobile budget |
| **D-015** | **URPSetupHelper auto-prompts on Editor startup if URP not active** | **10.8** | **Catches "pulled script, forgot to run menu" case** |
| **D-016** | **Vendor shaders patched via AssetPostprocessor, not in-repo edits** | **10.8** | **Avoids 136 KB content-arg push; survives vendor updates** |

---

## Issue / Risk Log

| # | Item | Severity | Status |
|---|---|---|---|
| Phase-10.5 studio compile errors | High | ✅ Resolved |
| Phase-10.6 vendor compile errors | High | ✅ Resolved (4 packages + audit) |
| Phase-10.7/10.8 Microdetail SetupWizard error | Medium | 🟢 Resolved — auto-prompt on Editor open |
| Phase-10.8 BM_Lit duplicate `_SHADOWS_SOFT` warning | Low | 🟢 Resolved — AssetPostprocessor patches on import |

---

## Editor Menu Items Available (cumulative)

| Menu Path | Purpose | Phase |
|---|---|---|
| Hearthbound → Setup URP Pipeline (one-time) | Activate URP + create asset bundle | 10.7 |
| **Hearthbound → Check Render Pipeline Status** | **Diagnostic: print current GraphicsSettings + QualityLevel pipelines** | **10.8** |
| Hearthbound → Create Mission 1-2 Seed Assets | Spawn all 17 ScriptableObject seed assets | 11 |
| Hearthbound → Validate Mission 1-2 Seed Assets | Audit which seed assets are missing | 11 |
| **Hearthbound → Patch ASE Shaders Now** | **Force-patch the BM_Lit (or any ASE) shader's duplicate `_SHADOWS_SOFT`** | **10.8** |

---

## Unity 6 — Correct Menu Paths Reference

For the user's future reference, since Unity 6 (6000.x) reorganized several menus:

| Task | Unity 2022 legacy path | **Unity 6.0.4 correct path** |
|---|---|---|
| Convert materials to URP | `Edit → Rendering → Materials → Convert All Built-in Materials to URP` | **`Window → Rendering → Render Pipeline Converter`** → "Built-in to URP" |
| Open Test Runner | `Window → General → Test Runner` | Same |
| Lighting bake settings | `Window → Rendering → Lighting` | Same |
| Profiler | `Window → Analysis → Profiler` | Same |

---

*Last updated: Phase 10.8 — Editor-startup URP prompt + shader auto-patcher. PR #7 open.*
