# 🏘️ Phase 27 — Environment Polish Plan

> Branch: `feat/mission-1-2-architecture` · Author: Studio collective · Date: 2026-05-25
> Status: 🟡 Active — pushes split into 27.1 (this plan + prefab discoverer), 27.2 (Lane), 27.3 (Hollow interior), 27.4 (atmosphere + interactables + capstone).

This document is the single source of truth for the environment polish pass on Mission 1. It maps every imported Unity asset pack to a specific role in the cozy autumnal village + Hollow interior, and tells the editor builders **exactly which prefab to instantiate where**.

---

## 1. Source design contracts (what the world must feel like)

Pulled from `Docs/Depth_Bible/Mission_1_2_Focus/03_SCENES_LANE_HOLLOW_GARDEN_COTTAGE.md` and `GAME_DESIGN.md` §1 (Vision Statement).

| Beat | Lane (Day 1, late afternoon) | Hollow interior (Day 1, dusk) |
|---|---|---|
| **Lighting** | Warm slanting sun, long shadows, lantern at the door already lit | Hearth glow + hanging lantern + candle on workbench |
| **Palette** | Burnt amber, dusty rose, autumn ochre. Cobble grey ground. | Deep cocoa wood walls, parchment cream highlights, ember orange firelight |
| **Sound** | Distant village murmur, leaves rustling, kettle whistling far off | Hearth crackle, ticking shelf clock (placeholder), Pickle's purr |
| **Player feeling** | "I should be inside before dark." A path that asks to be walked. | "I want to stay here forever." Every prop invites a second look. |

The cardinal rule: **nothing in this branch is procedural anymore**. Every cube, cylinder, and plane gets replaced with a real authored prefab from a packaged asset.

---

## 2. Asset-pack inventory used in Phase 27

| Pack | Folder | Used for |
|---|---|---|
| **MeshingunStudio / Medieval Village** | `Assets/MeshingunStudio/Medieval Village/Art/Prefabs/` | All structural geometry — walls, floors, roofs, doors, cottages, fence segments, well, tables, benches, beds, hearths, baskets, sacks, crates, banners, lantern, candles, torches, signs, bell |
| **Unluck Software / Stylized Weather** | `Assets/Unluck Software/Stylized Weather/Prefabs/` | Outdoor foliage (CommonTree_1..5, Bush_Common_Flowers, Plant_7_Big, Grass_Common_Short), rocks (Rock_Medium_1..3), rock paths (RockPath_Round_*), atmosphere particles (Leaves, Fog Low, Fog Soft Time, Dust, Wind Line Looped) |
| **Lumen (UPM)** | `Packages/com.distantlands.lumen/Content/Prefabs/` | Cozy light effects — `Lantern Effect`, `Sunshafts Effect`, `Shimmery Light` for hearth, `Light Surface` for window glow |
| **Zephyr (UPM)** | `Packages/com.distantlands.zephyr/` | Subtle wind on tree leaves + banners (Phase 19 already wired the system; Phase 27 just spawns the wind zone) |
| **All In 1 Shader Nodes** | `Assets/Plugins/AllIn1ShaderNodes/` | Phase 16 already shipped the MemoryOrb_Master shader — Phase 27 doesn't touch shaders |
| **Bamao Fantasy GUI** | `Assets/Bamao/BamaoUIPack/` | In-world signage on the Hollow door + workbench tag (rendered via `TextMeshPro 3D`, framed in a Bamao plank) |
| **BoZo Stylized Modular Characters** | `Assets/BoZo_StylizedModularCharacters/` | Already used for Doris + Player — Phase 27 may spawn 2 silent-villager extras in the lane background for life |
| **Cutscene Engine + Timeline** | `Assets/Cutscene Engine/` | Untouched in Phase 27 (Phase 21 already wired the Dream cutscene rig) |

---

## 3. Lane scene composition target (`02_Mission01_Lane.unity`)

Reference layout (top-down, 60 m × 60 m terrain plane already created by Phase 12):

```
                          ┌───────────────────────────────┐
                          │  HOLLOW DOOR  ←  Sign + Lantern│
                          │   ↑                              │
                          │   ↑   3-tier cobble step         │
                          │   ↑                              │
   Cottage 02   Tree     Tree                         Cottage 03
      ▢▢▢      🌳        🌳                              ▢▢▢
                 │ Banner banner                              │
              ┌──┘                                            │
              │   ← cobble path RockPath_Round_*    Bench →   │
              │                                               │
              │                Player spawn here              │
              │                                               │
   Cottage 01 │                                            Well + Crate
      ▢▢▢    │                                               🪣
              │
              │  ← fence segments
              │
              ▼ scene exit / Main Menu return
```

**Concrete prefab placement (Phase 27.2 builder):**

| Role | Prefab (from MeshingunStudio Medieval Village) | World position | Notes |
|---|---|---|---|
| Hollow door (north end) | `SM_WoodenDoor_01a` (or best `*Door*` match — Phase 15 already discovers) | (0, 0, 8) | Replaces Phase 12 cube. Add `HollowDoorInteractable` (existing). |
| Door sign frame | `SM_Sign_01a` (or best `*Sign*` match) | (0.8, 1.6, 7.6) | "The Hollow" text via `TextMeshPro 3D`, parchment colour. |
| Hanging lantern | `SM_Lantern_01a` (best `*Lantern*` match) | (-1.0, 2.6, 8.0) | Adds `LanternInteractable` + Lumen `Lantern Effect` child. **Already lit on scene load**. |
| 3-tier stone step in front of door | `SM_StepStair_01c` ×1 | (0, 0.0, 7.0) | Defines the "you have arrived" beat. |
| Cobble path tile | `SM_CornerCobble_01a` ×8 spread Z = -8..7 along X = 0 corridor | varies | Replaces flat ground for the central walk. |
| Path stones (organic edges) | `RockPath_Round_*` from Stylized Weather, ×6 | scattered ±2 m | Soft-curve the path edge so it doesn't look gridded. |
| Cottage 01 (left foreground) | best cottage prefab via Phase 15 binding | (-9, 0, -2) rot Y=20° | Slightly turned so player sees roof line. |
| Cottage 02 (left back) | same cottage prefab variant | (-9, 0, 4) | Same family, rotated 180° for variety. |
| Cottage 03 (right back) | same cottage prefab | (9, 0, 4) rot Y=-20° | Mirror composition. |
| Window glow (cottage 01) | Lumen `Light Surface` (warm amber) | inside window mesh | Tells the player villagers are home. |
| Fence segments | `SM_Fence_*` ×8 | along Z = -8 row | Frames the south edge so the player feels enclosed. |
| Well | `SM_Well_01a` (or best) | (5, 0, -3) | Visible reference for "village heart". |
| Bench | `SM_BenchConcrete_01a1` | (4.5, 0, 1.5) rot Y=90° | Optional sit-spot — non-interactable for now. |
| Tree (foreground left) | `CommonTree_1..5` (Stylized Weather) | (-5.5, 0, -1) | Adds autumn leaves above the bench. |
| Tree (background centre) | `SM_Alder_Fall_01b` (medieval pack's autumn alder) | (-2.5, 0, 5.5) | Closer to the door for canopy. |
| Tree (foreground right) | `CommonTree_3` | (5.5, 0, -1.5) | |
| Bushes | `Bush_Common_Flowers` ×4 | scattered | Floor-line softness. |
| Foreground rock cluster | `Rock_Medium_1`, `Rock_Medium_3` | near tree bases | Breaks up the flat ground. |
| Grass tufts | `Grass_Common_Short` ×12 | path-edge sparse | Cozy autumn-evening feel. |
| Crate pile | `SM_Pile_WoodLog_01a`, `SM_Crate_01a` near well | (6, 0, -2.5) | Village-of-real-people detail. |
| Banner/flag | `SM_Banner_01a` (or best) | on cottage 02 wall, swaying via Zephyr | Movement breaks static composition. |
| Falling leaves particle | `Leaves.prefab` (Stylized Weather) | scene-wide, Y=4, world-space | Constant gentle drift across the path. |
| Soft fog | `Fog Soft Time.prefab` (Stylized Weather) | scene-wide, ground-hugging | Distance softens — "evening is coming". |
| Sunshafts | Lumen `Sunshafts Effect.prefab` | aimed at directional light | Through the canopy onto the path. |
| Silent-villager extra 01 | BoZo NPC prefab walking along fence | (-7, 1.0, -7) | Optional ambient life. Disable nav, just idle. |

**Atmospheric tuning:**
- Directional light: warm `(1, 0.84, 0.62)` at angle X=35°, intensity 0.95 (matches existing Phase 17 lighting).
- Ambient: trilight already set by HearthboundOneClickSetup.
- Reflection probe: 1 placed at (0, 2, 0) — cubemap baked from scene at dusk.

---

## 4. Hollow interior composition target (`03_Mission01_Hollow.unity`)

Reference layout (top-down, 24 m × 24 m floor):

```
              ┌─────────────────────────────────┐
              │      Hearth (firepit + glow)    │
              │           🔥                     │
              │                                  │
              │   Shelf (orbs)   Workbench       │
              │      ▥▥▥▥          🧰             │
              │                  (Marin's Note)  │
              │   Doris standing here     Pickle │
              │       🧑                    🐈    │
              │                                  │
              │   Wool rug                       │
              │   ████████                       │
              │            ←  Player spawns      │
              │                here              │
              │                                  │
              │  Door back to Lane (off-screen)  │
              └─────────────────────────────────┘
```

**Concrete prefab placement (Phase 27.3 builder):**

| Role | Prefab | World position | Notes |
|---|---|---|---|
| Floor (wood plank) | `SM_Floor_08x08_02a` ×4 tiled | covers 16×16 m | Replaces the flat plane ground. |
| Wall — north (with window) | `SM_WallWindow_01a_1` | along Z=+5 | Window glows from outside (`Light Surface`). |
| Wall — east | `SM_Wall_01d_1` ×3 | along X=+8 | Solid wall, banner hangs on it. |
| Wall — west | `SM_Wall_01d_1` ×3 | along X=-8 | Shelves mounted on this wall (see Shelves row). |
| Wall — south (door wall) | `SM_WallWindow_01c` + door cut-out | along Z=-5 | Doorway back to Lane. |
| Ceiling beams | `SM_WoodLog_03a` ×3 | spans X=-7..7 at Y=4 | Cozy roof rafters. |
| Hearth / firepit | `SM_Firepit_02a` | (-4, 0, 4) | Centre-left, draws the eye. |
| Hearth glow (Lumen) | Lumen `Shimmery Light` (warm orange) | on firepit centre | Pulses gently. |
| Hearth crackle SFX | `AmbientAudio` with hearth loop | child of firepit | Mood. |
| Workbench | `SM_Wooden_Cutting_Board_01` + `SM_BenchConcrete_01a1` stacked | (4, 0, 3) | Replaces Phase 12 cube. The orb sits on top. |
| Marin's Note | already wired by Phase 26 — Phase 27 just preserves it on the bench | (4 + offset) | Quad-with-text remains visible. |
| Shelf 1 — orbs display | `SM_WoodStand_01a` + 3 `SM_Wax_04a` (candle stubs) | (-7, 0.5, 1) | Atmosphere. Not yet interactable. |
| Shelf 2 — books/jars | `SM_TerraPots_01b`, `SM_WickerBasket_01d` | (-7, 0.5, -2) | Visual density. |
| Memory orb (existing) | `MemoryOrb_DOR-001` Sphere primitive (Phase 16 shader-graph driven) | on workbench | Unchanged. |
| Pickle (existing) | sphere placeholder until Phase 13 cat prefab arrives | (2.0, 0.25, 4.5) | Unchanged. |
| Hanging lantern | `SM_Lantern_01a` | (0, 3.5, 0) ceiling | `LanternInteractable` + Lumen `Lantern Effect`. |
| Candle on workbench | `SM_Wax_04a` | (3.5, 1.05, 3) | Tiny Lumen `Shimmery Light` halo. |
| Wool rug (centre) | `SM_Banner_01a` flat scaled 4×6 | (0, 0.01, 0) rot X=90 | Soft visual centre. |
| Wall lantern (east wall) | `SM_Lantern_01a` | (7.8, 2.2, 0) rot Y=-90 | Symmetry + ambient. |
| Doris standing mark | invisible empty | already placed by Phase 12 at (2,0,0) | Unchanged. |
| Sack pile near hearth | `SM_Grain_Sack_01a`, `SM_FoodSac_01b` | (-5.5, 0, 2.5) | Cozy domestic clutter. |
| Crate stool | `SM_Crate_01a` or `SM_WickerBasket_02a` | (-5.5, 0.5, 3.5) | Implies villagers gather. |
| Banner on east wall | `SM_FlagGurland_01a` | (7.7, 2.8, -1) | Movement via Zephyr. |
| Window glow from outside | Lumen `Light Surface` aimed in | outside Z=+5.5 | Sells "the village is alive outside". |

**Lighting:**
- Directional light SOFTLY reduced (interior is dusk) — intensity 0.4, warm.
- Replace ambient sky/equator/ground with deeper cocoa values.
- Reflection probe at (0, 2, 0).

---

## 5. Interaction scripting (Phase 27.4)

Three new runtime interactables under `Assets/_Project/Scripts/Player/` (per **D-035** these don't need UI — they only emit events).

| Script | Trigger | Effect |
|---|---|---|
| `LanternInteractable` | Press E near a lantern | Toggles a Lumen `Light Surface` between warm-bright and warm-dim. Plays a soft chime SFX. Adds a small `predecessorTrailWarmth +1` for the hanging-by-the-door variant (Marin lit the lantern last). |
| `HearthAmbianceTrigger` | Player enters trigger | Crossfades AmbientAudio volume up (the hearth crackle gets louder); no E-press needed — purely environmental. |
| `WindowGazeTrigger` | Player enters trigger near the window | Tints the camera vignette slightly warmer; fires a one-line "look at the village" subtitle the first time only. |

All three live in `HearthboundHollow.Player` asmdef — no UI/Dialogue deps needed (per **D-035 asmdef-locality check**).

---

## 6. Atmospheric VFX (Phase 27.4)

Direct prefab spawns into the Lane scene under a `_Atmosphere` parent:

```
_Atmosphere (Lane scene)
├── _Leaves         (Stylized Weather/Leaves.prefab, scale=1.0, looping)
├── _Fog_Soft       (Stylized Weather/Fog Soft Time.prefab)
├── _Sunshafts      (Lumen/Sunshafts Effect.prefab, aimed at Directional Light)
├── _WindZone       (UnityEngine.WindZone, soft direction, Zephyr controller picks it up)
└── _ReflectionProbe (cubemap, baked)
```

Hollow scene gets a different `_Atmosphere`:

```
_Atmosphere (Hollow scene)
├── _HearthGlow     (Lumen/Shimmery Light.prefab, parented to firepit)
├── _WindowGlow     (Lumen/Light Surface.prefab outside the window)
└── _CandleGlow     (Lumen/Shimmery Light.prefab on workbench candle)
```

---

## 7. Phase-by-phase delivery checklist

Each step is a separate git commit so the user can pull incrementally.

| Step | Title | What lands | Files touched |
|---|---|---|---|
| **27.1** | This design doc + extended prefab discovery | `Docs/Phase27_Environment_Polish_Plan.md` (this file). `MedievalVillageBindings` SO gets new fields (hearth, hangingLantern, candle, signFrame, banner, sackPile, basket, woodFloor, woodWall, woodWindow). `Phase15_MedievalVillageBuilder.cs` extended to discover them. | This doc + extended Phase15 builder. |
| **27.2** | Lane environment polish builder | `Phase27_LaneEnvironment.cs` editor — clears the existing scene's placeholder cube door + ground, spawns cottages, trees, cobble, fence, well, lantern, signage, leaves, fog, sunshafts. | Single new editor script. |
| **27.3** | Hollow interior polish builder | `Phase27_HollowInterior.cs` editor — replaces placeholder cube workbench + plane floor + ground, spawns wood floor tiles, 4 walls, ceiling beams, hearth, shelves, hanging lantern, candles, rug, banners. | Single new editor script. |
| **27.4** | Interactables + atmosphere capstone | Runtime: `LanternInteractable.cs`, `HearthAmbianceTrigger.cs`, `WindowGazeTrigger.cs` in Player asmdef. Editor: `Phase27_EnvironmentCapstone.cs` that runs 27.2 + 27.3 + ties in Lumen + Zephyr. Menu: `Hearthbound → 🌳 Phase 27 — Polish Mission 1 Environment`. | 3 runtime + 1 editor + PROGRESS.md + CHANGELOG.md. |

---

## 8. Acceptance criteria (the Critic & Review Board's gate)

Before Phase 27 is marked ✅ on PROGRESS.md:

- [ ] No `GameObject.CreatePrimitive` calls left in any Lane or Hollow scene asset (verified by grep on the saved scene files).
- [ ] Player can press E on the door lantern → it toggles bright/dim with audible chime.
- [ ] Walking past the hearth raises ambient crackle without an interact prompt.
- [ ] Falling leaves drift continuously across the Lane path at ≤ 4 ms/frame budget.
- [ ] All three trees, both cottages, the well, and the fence segments are visible from the player spawn camera angle.
- [ ] Marin's Note still works (Phase 26 acceptance preserved).
- [ ] The diagnostic menu (`🔍 Diagnose Phase 23 Build`) prints zero warnings on either scene.
- [ ] PROGRESS.md + CHANGELOG.md updated, D-041 (asset binding override list) recorded.

---

## 9. Open questions / risks

- **R-1**: MeshingunStudio prefabs use URP/Lit by default but some materials may still be HDRP/Default. → mitigation: the existing `BMLitShaderPatcher.cs` (Phase 10.7) walks materials and ports them. Run it once during Phase 27 capstone.
- **R-2**: Leaves particle is world-space and can run hot on mobile. → mitigation: cap MaxParticles to 80 and disable for mobile-low quality tier.
- **R-3**: Lumen `Sunshafts Effect` requires a directional light Volumetric component on URP — provide a graceful fallback if not present.
- **R-4**: Wind zones may double-influence if both Zephyr (Phase 19) and a UnityEngine.WindZone exist. → Phase 27 only adds the UnityEngine.WindZone; Zephyr's master controller stays untouched.

---

*End of plan. Each push linked above appends to `Docs/PROGRESS.md` with the same commit ledger discipline as Phase 25 + 26.*
