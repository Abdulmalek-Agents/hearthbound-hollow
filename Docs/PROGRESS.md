# 📊 Hearthbound Hollow — Implementation Progress Log

> Continuously updated as the studio builds Mission 1-2. Every PR appends to this file.
> **Unity Editor version: 6000.4.4f1 (Unity 6 LTS)**

---

## Legend
- ✅ Done & merged · 🟢 Done in branch (awaiting your pull) · 🟡 In progress · ⬜ Not started · 🔴 Blocked

---

## Roadmap (proper order — polished playable as Phase 22 capstone)

| Phase | Title | Asset(s) | Replaces |
|---|---|---|---|
| ✅ 0–10.8 | Architecture, scripts, bug-fix cycles, URP, shader patcher | (foundation) | — |
| ✅ 12 | Make It Playable MVP (smoke-test) | primitives + URP/Lit | — (the baseline) |
| 🟢 **13** | **BoZo Character Prefabs** | `BoZo_StylizedModularCharacters/` | capsule + cylinder |
| 🟢 **14** | **Bamao Parchment UI Skin** | `Bamao/BamaoUIPack/` | flat-color UI |
| ⬜ 15 | Medieval Village Environment | `MeshingunStudio/Medieval Village/` | plane + cubes |
| ⬜ 16 | MemoryOrb_Master Shader Graph | `Plugins/AllIn1ShaderNodes/` | URP/Lit fallback |
| ⬜ 17 | Lumen Lighting + Cinemachine | `Packages/com.distantlands.lumen/` | plain directional + SimpleFollowCamera |
| ⬜ 18 | Audio Integration | `Game UI & Puzzle Sound Effects Pack/` | silence |
| ⬜ 19 | Stylized Weather + Zephyr Wind | `Unluck Software/Stylized Weather/` + `com.distantlands.zephyr/` | static foliage |
| ⬜ 20 | Yarn Spinner Integration | Yarn Spinner UPM | Mission01Director inline lines |
| ⬜ 21 | Memory Dream Cutscene | `Cutscene Engine/` + Timeline | hard cut to ledger |
| ⬜ **22** | **Polished Playable Mission 1** | (all above) | replaces Phase 12 entirely |

---

## ✅ Phase 0–10.8 — Foundation complete (architecture, scripts, bug-fix cycles, URP, shader patcher)
## ✅ Phase 12 — MVP playable smoke-test (primitives)

---

## 🆕 Phase 13 — BoZo Character Prefabs  🟢

**Delivered:** `Assets/_Project/Scripts/Editor/Phase13_BoZoCharacterBuilder.cs`

Menu: **`Hearthbound → Phase 13 — Build BoZo Character Prefabs`**

Builds 4 wrapper prefabs from the BoZo base character via structural scoring (Animator + SMR + path heuristics; top 3 candidates logged; manual fallback picker):

- `Assets/_Project/Prefabs/Player/Player.prefab` — tag Player + CharacterController + PlayerController
- `Assets/_Project/Prefabs/NPCs/Doris.prefab` — warm amber tint + 1.6 m greeting trigger
- `Assets/_Project/Prefabs/NPCs/Gerrold.prefab` — dusk-blue bard tint + 1.8 m cottage trigger
- `Assets/_Project/Prefabs/NPCs/SilentLaneVillager.prefab` — neutral umber

Wrapper-prefab pattern: nested BoZo prefab as "Body" child → vendor updates propagate, studio code stays isolated.

---

## 🆕 Phase 14 — Bamao Parchment UI Skin  🟢

**Delivered (2 files):**

### A. `Assets/_Project/Scripts/Editor/Phase14_BamaoUIBuilder.cs`

Menu: **`Hearthbound → Phase 14 — Build Bamao UI Prefabs`**

Builds 4 UI prefabs at `Assets/_Project/Prefabs/UI/`:

| Prefab | Role |
|---|---|
| `UI_DialogueBox_Bamao.prefab` | Parchment background + portrait slot + speaker/line TMP + choices container + choice-tile template (hidden child) |
| `UI_ChoiceTile_Bamao.prefab` | Scroll-frame button standalone (for reuse outside dialogue) |
| `UI_EveningLedger_Bamao.prefab` | Open-book layout, two-page reading: Day label, summary prose, held memories list, coin, 3 save slots + autosave + end-of-day confirm |
| `UI_TooltipFrame_Bamao.prefab` | Codex examine-tooltip frame |

Sprite detection: scores every Sprite under `Assets/Bamao/` by name keywords + path heuristics + dimension thresholds + 9-slice readiness (+8 score boost for sprites with non-zero border). Top 3 candidates per role are logged. Falls through to warm-tint color fallback if no match found.

`Image.type = Sliced` when sprite has 9-slice borders, `Simple` otherwise.

Bamao theme palette (matches Asset_Analysis_Mission1-2 § 5 S-4 "Hearthbound" preset):
- Parchment tint: `(0.98, 0.94, 0.82)`
- Ink color: `(0.22, 0.16, 0.10)`
- Speaker ink: `(0.42, 0.24, 0.10)`
- Gold ember: `(0.92, 0.70, 0.34)`

### B. `Assets/_Project/Scripts/Editor/HearthboundOneClickSetup.cs` (updated)

Scene builder is now **progressively polishing**:

```
Phase 12 alone   → primitives + flat UI
+ Phase 13 ran   → BoZo characters replace capsule/cylinder
+ Phase 14 ran   → Bamao parchment UI replaces flat backgrounds
+ Phase 15+      → (future)
```

Each role first tries `PhaseN_Builder.TryGet*Prefab()`; falls back to the primitive builder if not found. The post-build dialog reports which phases were applied:

> ✓ Phase 12 — base scenes + primitives + flat UI  
> ✓ Phase 13 — BoZo characters  
> ✓ Phase 14 — Bamao parchment UI  
> ⚠ Phase 15 — Medieval Village not run yet …

---

## Decisions Made

| # | Decision | Phase | Reason |
|---|---|---|---|
| D-001..D-023 | (see prior log) | 0–13 | (see prior entries) |
| **D-024** | **Bamao sprite auto-detection by structural scoring (name + path + size + 9-slice border)** | **14** | **Bamao ships 300+ sprites; structural scoring picks robustly across asset-pack versions** |
| **D-025** | **Image.Type.Sliced when sprite has 9-slice border, Simple otherwise** | **14** | **Lets Bamao's 9-slice authoring scale dialogue panels at any resolution without distortion** |
| **D-026** | **Color fallback for missing Bamao sprites — UI still functions** | **14** | **Builder never fails; user can manually drop sprites via Inspector to upgrade** |
| **D-027** | **Each Phase builder exposes `TryGet*Prefab()` lookups; HearthboundOneClickSetup chains them** | **14** | **Progressive polish — same one-click menu produces a better-looking scene as more phases land** |

---

## Issue / Risk Log

| # | Item | Severity | Status |
|---|---|---|---|
| Prior cycles | various | ✅ Resolved |
| Phase 14 — sprite auto-detection picking wrong sprite | Low | 🟢 Mitigated — top 3 candidates logged per role; manual Image.sprite override available in Inspector |

---

## Editor Menu Items Available (cumulative — 8 total)

| Menu Path | Purpose | Phase |
|---|---|---|
| `Hearthbound → Build Playable Mission 1 (One Click)` | 🛠️ Build all scenes (auto-uses any Phase 13/14 prefabs) | 12 |
| `Hearthbound → Phase 13 — Build BoZo Character Prefabs` | 🧍 BoZo character wrappers | 13 |
| **`Hearthbound → Phase 14 — Build Bamao UI Prefabs`** | **📜 Bamao parchment UI prefabs** | **14** |
| `Hearthbound → Setup URP Pipeline (one-time)` | Activate URP | 10.7 |
| `Hearthbound → Check Render Pipeline Status` | Diagnose URP state | 10.8 |
| `Hearthbound → Create Mission 1-2 Seed Assets` | Spawn the 17 SOs | 11 |
| `Hearthbound → Validate Mission 1-2 Seed Assets` | Audit missing seeds | 11 |
| `Hearthbound → Patch ASE Shaders Now` | Force-patch BM_Lit duplicate `_SHADOWS_SOFT` | 10.8 |

---

## How to run Phases 13 + 14 (current step)

1. Pull `feat/mission-1-2-architecture`.
2. Wait for Unity recompile (~5 s).
3. **`Hearthbound → Phase 13 — Build BoZo Character Prefabs`** — generates 4 BoZo wrappers.
4. **`Hearthbound → Phase 14 — Build Bamao UI Prefabs`** — generates 4 UI prefabs.
5. **`Hearthbound → Build Playable Mission 1 (One Click)`** — re-builds the scene with BoZo characters + Bamao UI in place of primitives.
6. Press Play. The MainMenu loads; click "Open The Hollow" → BoZo Doris greets you, parchment dialogue box appears, parchment evening ledger ends Day 1.

---

*Last updated: Phase 14 — Bamao Parchment UI Skin landed + HearthboundOneClickSetup integration. Phase 15 (Medieval Village Environment) next.*
