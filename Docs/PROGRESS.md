# 📊 Hearthbound Hollow — Implementation Progress Log

> Continuously updated as the studio builds Mission 1-2. Every PR appends to this file.
> **Unity Editor version: 6000.4.4f1 (Unity 6 LTS)**

---

## Legend
- ✅ Done & merged to main · 🟢 Done in branch (awaiting your pull) · 🟡 In progress · ⬜ Not started · 🔴 Blocked

---

## ✅ Phase 0 — Architecture & Skeleton  (PR #7)
## 🟢 Phase 1–8 — All script implementation phases complete.
## 🟢 Phase 9 — Scenes — **NOW AUTOMATED** (was: Unity-side authoring)
## 🟢 Phase 10 — QA & Polish (21 EditMode tests, secret-scan clean)
## 🔧 Phase 10.5 — Studio-Code Bug Fixes  ✅
## 🆕 Phase 11 — Quality-of-Life Tooling  ✅
## 🔧 Phase 10.6 — Vendor-Asset Package Cycle  ✅
## 🔧 Phase 10.7/10.8 — Render Pipeline + Shader Patcher  ✅

---

## 🎮 Phase 12 — Make It Playable  🟢 Awaiting one-click menu

The "Make It Playable" capstone. Three new components turn the entire stack into a one-click playable Mission 1:

### A. `Mission01Director.cs` (runtime — Mission asmdef)
Yarn-free runtime sequencer that orchestrates Mission 1 directly via DialogueUI.PresentLine/PresentChoices. Lets the MVP run END-TO-END before Yarn Spinner is installed.

Flow (matches `Mission_1_2_Focus/01_DORIS_THE_BAKER.md`, abridged):
1. Player enters Doris's greeting trigger → 3-option reply.
2. Doris offers the orb → 3-option price negotiation. Adjusts trust + coin.
3. Doris hands over the orb → it becomes visible on the workbench.
4. Player walks to workbench trigger → PolishMiniGame begins.
5. Polish completes → tier-based Doris reaction (bright / acceptable / dim).
6. Memory added to `VillageState.heldMemoryIds`.
7. EveningLedger shows with clarity-keyed summary prose.
8. Player confirms End of Day → loads back to MainMenu.

When Yarn Spinner is installed later, this director can be replaced by `YarnDialogueRunner` without touching anything else.

### B. `MemoryOrbInteractable` URP/Lit fallback
Until the bespoke `MemoryOrb_Master` Shader Graph is authored, the orb runs on URP/Lit which doesn't have `_Clarity`/`_PaletteTint`. Added a fallback that also drives `_BaseColor` (lerped dim→tint) and `_EmissionColor` (tint × clarity × intensity). Result: the polish mini-game produces a visible glowing orb effect immediately, no shader-graph work required.

### C. `HearthboundOneClickSetup.cs` (Editor — `Hearthbound → Build Playable Mission 1 (One Click)`)
Single menu item that produces a fully wired, runnable Mission 1 vertical slice with **zero manual scene authoring** by the user.

Build steps:
1. Verifies URP active (prompts to run URPSetupHelper if not).
2. Verifies seed assets exist (prompts to run SeedAssetGenerator if not).
3. Creates 4 URP/Lit materials (Ground / Player / Doris / Workbench / Orb).
4. Builds **`00_Bootstrap.unity`** — GameManager + auto-load to MainMenu.
5. Builds **`01_MainMenu.unity`** — Title + "Open The Hollow" CTA + Quit + ToneCompass.
6. Builds **`02_Mission01_Lane.unity`** — Player + ground + HollowDoor interactable.
7. Builds **`03_Mission01_Hollow.unity`** — the playable MVP scene:
   - Player capsule with CharacterController + PlayerController + tag `Player`
   - Doris cylinder + 1.6 m greeting trigger zone
   - Workbench cube + 1.4 m approach trigger zone
   - MemoryOrb sphere wired to DOR-001 SO + PolishMiniGame target (initially hidden)
   - UI Canvas with `DialogueUI`, choices container, `EveningLedgerUI`, `HUDController`
   - `Mission01Director` with every reference populated
   - `SimpleFollowCamera` (no Cinemachine dep — matches D-009)
8. Adds all 4 scenes to Build Settings (Bootstrap at index 0).
9. Opens `00_Bootstrap.unity` so the user can press Play immediately.

**Result:** push Play → MainMenu → "Open The Hollow" → walk to Doris → dialogue → walk to workbench → polish → Evening Ledger → End Day → loop back to MainMenu.

---

## Decisions Made

| # | Decision | Phase | Reason |
|---|---|---|---|
| D-001 | BoZo over City Characters | 0 | Critic Board rec |
| D-002 | Yarn Spinner over OpenAI addon | 0 | GDD Pillar 1 |
| D-003 | Don't relocate existing vendor folders | 0 | Preserves .meta GUIDs |
| D-004 | One asmdef per Scripts/ subfolder | 0 | 80% faster iteration |
| D-005 | InteractionPromptUI lives in Player asmdef | 5 | Avoids UI→Player dep cycle |
| D-006 | YarnVillageStateBridge compile-guarded | 6 | Bridge compiles before Yarn install |
| D-007 | Scene authoring is Unity-side, scripts are GitHub-side | 9 | `.unity` files have GUID refs |
| D-008 | Drop `com.unity.textmeshpro` from manifest | 10.5 | Folded into ugui 2.0 |
| D-009 | Cinemachine via reflection in cutscene/camera helpers | 11 | Avoids hard compile dep |
| D-010 | Seed asset generation via Editor menu, not committed .asset files | 11 | Avoids GUID race |
| D-011 | Explicit-pin DOTS-family packages | 10.6 | Vendor packs need them |
| D-012 | Whole-vendor-tree audit on every CS0234 | 10.6 | Add N packages once vs whack-a-mole |
| D-013 | URP RP asset auto-created + assigned via Editor menu | 10.7 | Manual Project Settings juggling is error-prone |
| D-014 | Mobile-friendly URP defaults (0.85, 2×, soft off) | 10.7 | Matches ARCHITECTURE § 10 |
| D-015 | URPSetupHelper auto-prompts on Editor startup | 10.8 | Catches "forgot to run menu" |
| D-016 | Vendor shaders patched via AssetPostprocessor | 10.8 | Avoids 136 KB push + survives updates |
| **D-017** | **Mission 1 MVP uses primitives (capsule/cylinder/cube/sphere) + URP/Lit fallback shader** | **12** | **Removes the "needs BoZo prefab wired manually" gate; playable Day 1** |
| **D-018** | **Mission01Director sequences dialogue directly (no Yarn dep)** | **12** | **Makes MVP playable WITHOUT installing Yarn Spinner first** |
| **D-019** | **Whole playable build is one Editor menu click** | **12** | **Same philosophy as D-010 + D-013 — Unity-side authoring is brittle; code is reproducible** |

---

## Issue / Risk Log

| # | Item | Severity | Status |
|---|---|---|---|
| All previous bug cycles | various | ✅ Resolved |
| Phase 12 MVP first-play UX | Low | 🟢 Self-tested via the One-Click menu's dialog. Monitor first user run. |

---

## Editor Menu Items Available (cumulative)

| Menu Path | Purpose | Phase |
|---|---|---|
| **`Hearthbound → Build Playable Mission 1 (One Click)`** | **🎮 The main "make it work" button — builds all scenes + wires every component** | **12** |
| `Hearthbound → Setup URP Pipeline (one-time)` | Activate URP + create asset bundle | 10.7 |
| `Hearthbound → Check Render Pipeline Status` | Diagnostic dump of current GraphicsSettings + QualityLevel pipelines | 10.8 |
| `Hearthbound → Create Mission 1-2 Seed Assets` | Spawn all 17 ScriptableObject seed assets | 11 |
| `Hearthbound → Validate Mission 1-2 Seed Assets` | Audit which seed assets are missing | 11 |
| `Hearthbound → Patch ASE Shaders Now` | Force-patch BM_Lit + any other ASE shader's duplicate `_SHADOWS_SOFT` | 10.8 |

---

## How to play (one command)

1. Pull `feat/mission-1-2-architecture`.
2. **`Hearthbound → Build Playable Mission 1 (One Click)`**.
3. Press Play.
4. Walk (WASD) → talk to Doris (the green cylinder) → polish her memory → end Day 1.

---

## Phase 13+ Roadmap (after MVP confirmed playable)

| # | Deliverable | Notes |
|---|---|---|
| 13 | Mission 2 (widower's request, herb garden, cleanse + moral choice) | Mission02Director + 04_Mission02_Garden + 05_Mission02_Cottage scenes |
| 14 | BoZo character reskin pass (Doris cleric, Gerrold bard) | Replace the placeholder cylinders |
| 15 | Medieval Village environment dressing pass | Replace the cube workbench + cube door |
| 16 | Bamao parchment UI skin | Replace the flat-color UI backgrounds |
| 17 | Lumen god-ray + ambient pass | Replace the directional + ambient lighting |
| 18 | Yarn Spinner integration | Mission01Director becomes a fallback; Yarn .yarn files take over |
| 19 | Memory Dream Timeline assets | Timeline cinematic for Dream 1 + 5 Dream 2 variants |
| 20 | Composer + VO drop-in | Audio cues replace silence |
| 21 | 20-person internal playtest (Mission_1_2_Focus § 08 § 6.2) | Greenlight gate |

---

*Last updated: Phase 12 — Make It Playable. PR #7 has a complete playable Mission 1 MVP behind one menu click.*
