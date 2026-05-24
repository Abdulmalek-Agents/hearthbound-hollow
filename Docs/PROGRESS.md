# 📊 Hearthbound Hollow — Implementation Progress Log

> Continuously updated as the studio builds Mission 1-2. Every PR appends to this file.
> **Unity Editor version: 6000.4.4f1 (Unity 6 LTS)**

---

## Legend
- ✅ Done & merged · 🟢 Done in branch (awaiting your pull) · 🟡 In progress · ⬜ Not started · 🔴 Blocked

---

## Plan correction (after Phase 10.8)

**Phase 12 was a development scaffold**, not the final destination. The proper plan — explicit per the user — is to **progressively integrate real assets in Phases 13-21, then ship a polished playable Mission 1 as the capstone Phase 22**. The Phase-12 one-click primitive scene stays as an engineering smoke-test, but each subsequent phase replaces a placeholder layer with the real asset.

| Phase | Title | Asset(s) integrated | Polished playable replaces |
|---|---|---|---|
| 13 | BoZo Character Prefabs | `BoZo_StylizedModularCharacters/` | Capsule player + cylinder Doris |
| 14 | Bamao Parchment UI Skin | `Bamao/BamaoUIPack/` | Flat-color dialogue / ledger backgrounds |
| 15 | Medieval Village Environment | `MeshingunStudio/Medieval Village/` | Plane + cube workbench + cube door |
| 16 | MemoryOrb_Master Shader | `Plugins/AllIn1ShaderNodes/` | URP/Lit `_BaseColor` fallback |
| 17 | Lumen Lighting + Cinemachine | `Packages/com.distantlands.lumen/` | Plain directional light + SimpleFollowCamera |
| 18 | Audio Integration | `Game UI & Puzzle Sound Effects Pack/` | Silence |
| 19 | Stylized Weather + Zephyr Wind | `Unluck Software/Stylized Weather/` + `Packages/com.distantlands.zephyr/` | Static foliage, no atmosphere |
| 20 | Yarn Spinner Integration | Yarn Spinner UPM | Mission01Director's inline lines |
| 21 | Memory Dream Cutscene | `Cutscene Engine/` + Unity Timeline | Hard cut to Evening Ledger |
| **22** | **Polished Playable Mission 1** | (all of the above wired together) | Replaces the Phase-12 MVP entirely |

---

## ✅ Phase 0 → 10.8 — Architecture, scripts, bug-fix cycles, URP, shader patcher  (all merged into branch)
## 🟢 Phase 12 — Make It Playable (MVP scaffold — primitives, no real assets)

This is the engineering smoke-test build. Useful for verifying that the script stack runs end-to-end. It stays available via the menu so the user can re-run it any time to sanity-check.

---

## 🆕 Phase 13 — BoZo Character Prefabs  🟢

**Delivered:** `Assets/_Project/Scripts/Editor/Phase13_BoZoCharacterBuilder.cs`.

Menu: **`Hearthbound → Phase 13 — Build BoZo Character Prefabs`**

What it builds (saved to canonical paths):
- `Assets/_Project/Prefabs/Player/Player.prefab` (tag Player + CharacterController + PlayerController)
- `Assets/_Project/Prefabs/NPCs/Doris.prefab` (warm cleric-amber tint + 1.6 m greeting trigger zone)
- `Assets/_Project/Prefabs/NPCs/Gerrold.prefab` (dusk-blue bard tint + 1.8 m cottage proximity zone)
- `Assets/_Project/Prefabs/NPCs/SilentLaneVillager.prefab` (neutral umber tint, decorative)

Approach: **wrapper-prefab pattern**. Each output prefab is a fresh GameObject with our components attached, plus a nested instance of the BoZo base character as a "Body" child. This means:
- BoZo updates propagate automatically.
- Studio code never references BoZo internals (only the wrapper).
- Animator + rig + outfits stay intact inside the nested prefab.

BoZo detection: scores all prefabs under `Assets/BoZo_StylizedModularCharacters/` by structure (SkinnedMeshRenderer + Animator + path heuristics). Top 3 candidates are logged so you can see the heuristic at work. Falls back to a manual file picker if auto-detect fails.

Material tinting: creates per-renderer instance materials (persists in the prefab; not just an MPB that vanishes outside play mode).

Public lookups (`Phase13_BoZoCharacterBuilder.TryGet*Prefab()`) are used by `HearthboundOneClickSetup` so the scene builder prefers these when they exist.

**Next phase:** Phase 14 — Bamao Parchment UI Skin.

---

## Decisions Made

| # | Decision | Phase | Reason |
|---|---|---|---|
| D-001..D-019 | (see prior log) | 0–12 | (see prior entries) |
| **D-020** | **Phase 12 MVP retained as engineering smoke-test, NOT the final playable** | **13** | **Following user's explicit plan: polished playable is the last phase. MVP remains useful for verifying scripts end-to-end.** |
| **D-021** | **Wrapper-prefab pattern over unpacked-clone for vendor character integration** | **13** | **Survives vendor updates; isolates studio code from vendor internals; cleaner upgrade path** |
| **D-022** | **BoZo prefab auto-detection by structural scoring (Animator + SMR + path heuristics)** | **13** | **Vendor prefab paths vary by version; brittle hard-coded paths would break on the next BoZo update** |
| **D-023** | **One Editor menu item per phase: `Hearthbound → Phase N — <title>`** | **13** | **User can run any phase independently; HearthboundOneClickSetup invokes them in sequence for the capstone build** |

---

## Issue / Risk Log

| # | Item | Severity | Status |
|---|---|---|---|
| Phase-12 to 10.8 cycles | various | ✅ Resolved |
| Phase-13 BoZo auto-detect picking wrong prefab | Low | 🟢 Mitigated — top 3 candidates logged; manual picker fallback |

---

## Editor Menu Items Available (cumulative — 7 total)

| Menu Path | Purpose | Phase |
|---|---|---|
| `Hearthbound → Build Playable Mission 1 (One Click)` | 🛠️ Engineering smoke-test build (primitives) | 12 |
| **`Hearthbound → Phase 13 — Build BoZo Character Prefabs`** | **🧍 Build 4 BoZo wrapper prefabs (replaces primitive characters)** | **13** |
| `Hearthbound → Setup URP Pipeline (one-time)` | Activate URP | 10.7 |
| `Hearthbound → Check Render Pipeline Status` | Diagnose URP state | 10.8 |
| `Hearthbound → Create Mission 1-2 Seed Assets` | Spawn the 17 SOs | 11 |
| `Hearthbound → Validate Mission 1-2 Seed Assets` | Audit missing seeds | 11 |
| `Hearthbound → Patch ASE Shaders Now` | Force-patch BM_Lit duplicate `_SHADOWS_SOFT` | 10.8 |

---

## How to run Phase 13 (current step)

1. Pull `feat/mission-1-2-architecture`.
2. Wait for Unity recompile (~5 s).
3. **`Hearthbound → Phase 13 — Build BoZo Character Prefabs`**.
4. Console will log "Built 4 BoZo wrapper prefabs". Check the Project window under `Assets/_Project/Prefabs/`.
5. (Optional) Re-run `Hearthbound → Build Playable Mission 1 (One Click)` — it will now use the BoZo prefabs instead of primitive capsule/cylinder. *(Scene-builder integration update lands in commit alongside Phase 14 — for now, drop the prefabs into a scene manually to verify they look right.)*

---

*Last updated: Phase 13 — BoZo Character Prefabs landed. Phase 14 (Bamao Parchment UI) next.*
