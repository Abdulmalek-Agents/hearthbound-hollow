# Changelog — Hearthbound Hollow

All notable changes to this project will be documented here. Entries follow the [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) format.

## [0.6.0-mission1-polish-v2] — 2026-05-25

**Branch:** `feat/mission-1-2-architecture` (accumulating on top of 0.5.2)
**Theme:** Phase 32 — Mission 1 Environment Polish v2. Real cottages, Hollow facade, hearth dressing, cozy URP cinematic volumes.

### User request

> *"please use the available assets in the project to enhance the mission 01 use all packages including medieval village check all asset documents and other packages to create very good environment with great lighting and shaders ...etc and work and push phase by phase and push it phase by phase make the game more polish with great environment clean and readable for player with high quality and polish check all assets"*

### Root cause analysis (what Phase 27 left on the table)

| Observed | Root cause |
|---|---|
| Lane "cottages" look like market stands | `Phase15_MedievalVillageBuilder` fuzzy-searched for "house / cottage / hut" and the MV pack ships no full house prefabs — only modular walls + roofs. Best score landed on `SM_ShopStand_01a`. |
| Hollow door floats in space | Phase 27.2 dressed the door but never wrapped a facade around it. |
| Flat / bland cinematic look | `DefaultVolumeProfile.asset` was empty. No Global Volume added by Phase 27. |
| Hollow interior reads as half-dressed | Phase 27.3 placed walls/floor/hearth but skipped kettle, bread, hanging herbs, cupboard. |

### Phase 32 — Five sub-phases, five separate commits

#### Phase 32.1 — Foundation: cottage assembler + extended bindings
- **`Editor/Phase32_MedievalCottageBuilder.cs`** (NEW, ~370 LOC) — Assembles 4 cottage prefab variants from MV's modular kit (`SM_Wall_01d_1`, `SM_WallWindow_01a_1`, `SM_WallDoor_03a`, `SM_Rooftiles_01a`, `SM_Chimney_01a`, `SM_Floor_04x04_02a`):
  - `Cottage_A_Bakery` — Doris's bakery, has a "BAKERY" sign + chimney + bread on the sill
  - `Cottage_B_Plain` — two-window cottage with a flag accent
  - `Cottage_C_Gabled` — narrow gabled cottage with single window
  - `Cottage_D_Corner` — door-cutout wall + flag garland + corner chimney
  - Each saved as a single self-contained `.prefab` under `Assets/_Project/Prefabs/Environment/`.
  - Menu: `Hearthbound → 🏘️ Phase 32.1 — Assemble Cottage Prefabs`
- **`Editor/Phase32_VillageBindingsExtension.cs`** (NEW, ~165 LOC) — `MedievalVillageBindingsV2.asset` SO with 23 additional prefab roles (chimney, kettle, bread, cupboard, hanging pot, stool, candelabra, signboard, autumn alder, etc.). Coexists with Phase 15's bindings — old builders untouched.
  - Menu: `Hearthbound → 🧰 Phase 32.1 — Catalog Extended Village Bindings`
- **`Docs/Phase32_Mission1_Polish.md`** (NEW) — Single source of truth for the Phase 32 series.

#### Phase 32.2 — Lane Environment v2
- **`Editor/Phase32_LaneEnvironmentV2.cs`** (NEW, ~670 LOC):
  - **8 cottages** in a 4-row layout (replaces Phase 27.2's 3 shopstands). Doris's Bakery is the hero on the player's right with a Lumen window glow.
  - **HollowFacade** — 4×3 m bakery-style wrap around the door. Side + back walls + 2 pitched roof tiles + chimney + "The Hollow" italic TMP 3D sign + window glow.
  - **3 extra lantern posts** along the cobble path (real Point lights + Lumen halos, mobile-safe no shadows).
  - **Doris's bakery dressing** — stacked TerraPot beehive proxy, hay bale, apple basket + 3 spilled apples, wood log pile (firewood).
  - **Smoking chimneys** — every cottage chimney gets a tiny Stylized Weather Dust wisp parented to it.
  - **Extended cobble path** + 20 stone-brick path borders (10 each side).
  - **Extra atmosphere** — 3 distant autumn alders, 6 pebbles, 8 grass tufts, 4 mushrooms, 1 dead tree silhouette.
  - All additions under `_Phase32Env_Lane` parent — idempotent. Phase 27.2's `_Phase27Env_Lane` is preserved.
  - Menu: `Hearthbound → 🏘️ Phase 32.2 — Polish Lane Environment V2`

#### Phase 32.3 — Hollow Interior v2
- **`Editor/Phase32_HollowInteriorV2.cs`** (NEW, ~475 LOC):
  - **Kettle on hearth** (`SM_Wooden_Pitcher_01`) with a small `ParticleSystem` steam wisp child (off by default — user enables for "kettle just boiled" moment).
  - **3 bread loaves** on the west shelf (`SM_Bread_01a`), spread along the shelf at varying angles.
  - **3 hanging dried-herb bundles** from the ceiling beams (inverted `SM_TerraPots_01b` proxies named Herb_Lavender / Valerian / Sage, each with a Cylinder rope).
  - **Marin's Cupboard** against southeast corner (`SM_Cupboard_01a`).
  - **Marin's Stool** beside the hearth (`SM_Stool_01a`).
  - **Workbench Candelabra** (`SM_Candleabra_02a`) with Lumen Shimmery halo + real Point Light (1.6 intensity, 3.5 m range, no shadows).
  - **Water bucket** by the hearth (`SM_Bucket_01a` — Mission 2 cleansing tie-in).
  - **Workbench dressing** — wooden cup + bowl + apple.
  - **2 wall candle sconces** on the west wall above the shelves.
  - **Larger pulse glow** at the hearth (additional Lumen Shimmery Light).
  - All under `_Phase32Env_Hollow` parent. Phase 27.3's `_Phase27Env_Hollow` preserved.
  - Menu: `Hearthbound → 🏠 Phase 32.3 — Polish Hollow Interior V2`

#### Phase 32.4 — Cozy URP Volumes
- **`Editor/Phase32_CozyVolumeBuilder.cs`** (NEW, ~330 LOC) — Authors two URP `VolumeProfile` assets procedurally + drops a Global Volume in each scene:
  - **`HearthboundLane_Volume.asset`** (warm dusk outdoor):
    - Bloom (intensity 0.45, threshold 0.95, scatter 0.7, warm tint, mobile-safe filtering)
    - Tonemapping (Neutral, Spiritfarer-style)
    - Color Adjustments (exposure +0.10, contrast +3, saturation +6, warm color filter)
    - White Balance (temperature +10, tint -2)
    - Vignette (intensity 0.22, deep brown)
    - Film Grain (intensity 0.18)
    - Channel Mixer (red boost on greens — autumn foliage)
  - **`HearthboundHollow_Volume.asset`** (cozy interior firelight):
    - Bloom (intensity 0.55, threshold 0.85, stronger amber tint)
    - Tonemapping (Neutral)
    - Color Adjustments (exposure -0.10, contrast +6, deeper warm filter)
    - White Balance (temperature +25, tint -4 — much warmer than Lane)
    - Vignette (intensity 0.32 — more enclosed)
    - Film Grain (intensity 0.22)
  - Main Camera updated in both scenes: `renderPostProcessing = true`, FXAA High.
  - Menu: `Hearthbound → 🌅 Phase 32.4 — Apply Cozy URP Volume`

#### Phase 32.5 — Master Capstone + Diagnostic + Docs
- **`Editor/Phase32_MissionOnePolishCapstone.cs`** (NEW, ~155 LOC) — Single-menu chain of 32.1 → 32.2 → 32.3 → 32.4 + re-runs Phase 27.4 (lantern wiring) + Phase 31 (dialogue repair). Reflection-driven so missing phases skip gracefully.
  - Menu: `Hearthbound → 🍂 Phase 32 — Polish Mission 1 (v2 — all phases)`
- **`Editor/Phase32_Diagnostic.cs`** (NEW, ~260 LOC) — Read-only audit. Walks both scenes + the 4 cottage prefabs + the 2 volume profiles and reports passed/warned/failed.
  - Menu: `Hearthbound → 🔍 Phase 32 — Diagnose Mission 1 Polish`
- **`Editor/Phase27_BuildEverything.cs`** — Master capstone now chains Phase 32 after Phase 31. The single-click "Build EVERYTHING" menu now runs the full v2 polish.

### Files added / changed

| File | Status | LOC | Phase |
|---|---|---|---|
| `Assets/_Project/Scripts/Editor/Phase32_MedievalCottageBuilder.cs` | **NEW** | ~370 | 32.1 |
| `Assets/_Project/Scripts/Editor/Phase32_VillageBindingsExtension.cs` | **NEW** | ~165 | 32.1 |
| `Docs/Phase32_Mission1_Polish.md` | **NEW** | — | 32.1 |
| `Assets/_Project/Scripts/Editor/Phase32_LaneEnvironmentV2.cs` | **NEW** | ~670 | 32.2 |
| `Assets/_Project/Scripts/Editor/Phase32_HollowInteriorV2.cs` | **NEW** | ~475 | 32.3 |
| `Assets/_Project/Scripts/Editor/Phase32_CozyVolumeBuilder.cs` | **NEW** | ~330 | 32.4 |
| `Assets/_Project/Scripts/Editor/Phase32_MissionOnePolishCapstone.cs` | **NEW** | ~155 | 32.5 |
| `Assets/_Project/Scripts/Editor/Phase32_Diagnostic.cs` | **NEW** | ~260 | 32.5 |
| `Assets/_Project/Prefabs/Environment/Cottage_*.prefab` ×4 | **NEW (generated)** | — | 32.1 |
| `Assets/_Project/Settings/HearthboundLane_Volume.asset` | **NEW (generated)** | — | 32.4 |
| `Assets/_Project/Settings/HearthboundHollow_Volume.asset` | **NEW (generated)** | — | 32.4 |
| `Assets/_Project/ScriptableObjects/Setup/MedievalVillageBindingsV2.asset` | **NEW (generated)** | — | 32.1 |
| `Assets/_Project/Scripts/Editor/Phase27_BuildEverything.cs` | Updated (+chains 32) | — | 27 |
| `CHANGELOG.md` | Updated (this entry) | — | doc |
| `README.md` | Updated | — | doc |

### Decisions

| # | Decision | Phase |
|---|---|---|
| D-051 (NEW) | Cottages are assembled from MV modular pieces into self-contained prefabs under `Assets/_Project/Prefabs/Environment/`. Phase 15's single-prefab cottage fallback is preserved for backward compatibility, but the v2 path prefers the assembled prefabs. | 32.1 |
| D-052 (NEW) | URP Volume Profiles are authored procedurally by an Editor script (never committed as YAML) so the user's installed URP version determines the serialised format. Avoids URP-version drift. | 32.4 |
| D-053 (NEW) | Phase 32 builders operate under a separate `_Phase32Env_*` parent GameObject in each scene. Phase 27's `_Phase27Env_*` parents are preserved so v1 and v2 polish layers coexist for diff/bisect. | 32.2, 32.3 |
| D-054 (NEW) | Steam VFX on the kettle is OFF by default (`ParticleSystem.Stop()` after creation). Cozy contract: scene doesn't start with a steam stream. User enables for the "kettle just boiled" moment. | 32.3 |

### Behaviour changes the player sees

- **The Lane reads as a residential village.** 8 cottages, not 3 shopstands. Doris's bakery is clearly identifiable. The Hollow is a real building with a sign and a chimney, not a floating door.
- **The Hollow feels inhabited.** Kettle on the hearth, bread on the shelf, herbs hanging from the rafters, candles on the workbench. Marin's cupboard fills the empty east wall.
- **Cinematic cozy look.** Subtle warm bloom on every lantern + window. Slight vignette frames the camera. Warm color grading on dusk-lit outdoor + deeper warmth on interior firelight. Film grain on both.
- **One-click rebuild.** `Hearthbound → ✨ Build EVERYTHING (Phase 27 — one click)` now also runs Phase 32 — fully wired Mission 1 polish in one menu item.

### Known limitations of v0.6.0

| # | Item | Severity | Status |
|---|---|---|---|
| P32-1 | Beehive is a stacked-TerraPot proxy until a proper beehive prop is imported. | Cosmetic | 🟡 Replace when `Hive` asset is imported |
| P32-2 | Chimney smoke uses Stylized Weather `Dust` particle as a wisp proxy until VoluSmokeFX is imported. | Cosmetic | 🟡 Replace when VoluSmokeFX is imported |
| P32-3 | Hanging dried herbs are inverted TerraPot proxies; ropes are Cylinder primitives. Not visually authored. | Cosmetic | 🟡 Replace with hand-authored herb bundle prefab when artist is available |
| P32-4 | Cottage roof tile seams may show at the ridge depending on the exact mesh pivot of `SM_Rooftiles_01a`. | Cosmetic | 🟡 Artist can drag-edit individual cottage prefabs |
| P32-5 | URP Channel Mixer is currently only applied to Lane (autumn foliage boost). Hollow uses default channel mix. | Intentional | ✅ |

---

## [0.5.2-advance-prompt-and-dream-canvas-hide] — 2026-05-25

**Branch:** `feat/mission-1-2-architecture` (accumulating on top of 0.5.1)
**Theme:** Phase 31.1 — make the dialogue's "click to advance" affordance visible, and stop the DreamCanvas bleeding into normal gameplay.

### User report (second screenshot)

> *"the game is stucked here please fix"*

Doris's `(stands back and watches)` line was fully rendered but the player
had no visible cue that they needed to click/press Space. Compounded by
the DreamCanvas (letterbox bars + dream prose) being permanently visible
and potentially intercepting clicks.

### Phase 31.1 — Advance prompt + Dream canvas hide

- **`UI/DialogueUI.cs`** — New `advancePrompt` field (italic TMP label
  `"Click or [Space] ▸"` in the dialogue box's lower-right corner).
  `Awake()` auto-creates it if the prefab is missing it. `Update()`
  PingPongs alpha 0.55 ↔ 1.0 at 1.4 Hz whenever the box is visible,
  typewriter is idle, and no choices are showing. New public
  `SkipTypewriter()` method exposed for future Yarn/director use
  (not wired into Update — auto-skip would race WaitForAdvance and
  double-advance on the same Space press).
- **`Cutscene/MemoryDreamSequencer.cs`** — New `dreamCanvas` field,
  auto-discovered in `Awake()` and **force-hidden** until `PlayDream1`
  / `PlayDream2`. Re-hidden in `OnDirectorStopped`. All child
  `Graphic.raycastTarget` zeroed so the letterbox bars can never
  intercept dialogue clicks.
- **`Editor/Phase31_DialogueChoiceCardRepair.cs`** — Now also bakes the
  `AdvancePrompt` label into the saved DialogueBox prefab (so existing
  prefabs get it without waiting for runtime self-heal).

**D-049 (NEW):** Any blocking dialogue UI must expose a visible advance
affordance. The advance polling loop runs in the director, not the UI —
the UI must show the player that the director is waiting on them.
**D-050 (NEW):** Cutscene/cinematic overlays must be hidden by default
and shown only while the cutscene plays. Full-screen overlays that are
not the active UI must zero child `Graphic.raycastTarget`.

---

[See full changelog for prior 0.5.x and 0.4.x entries.]
