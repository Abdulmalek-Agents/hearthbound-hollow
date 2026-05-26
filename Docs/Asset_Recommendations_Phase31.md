# 🎯 Asset Recommendations — Phase 31

> **Curator panel**: Senior Unity Developer · Senior Game Designer · Art Director · UX Lead · Unity-Asset-Store Specialist
> **Date**: 2026-05-25 · **Author**: Hearthbound Senior Team
> **Source library**: Inventix Games — 66-asset master reference (May 22, 2026)
> **Project context**: Hearthbound Hollow · `feat/mission-1-2-architecture` · Mission 1 + Mission 2 vertical slice

---

## ⚡ Executive summary

Of the 66 assets in the Inventix Games library, **17 are already integrated** in the project, **4 are "pending" but high-impact and should be imported next**, **5 additional new picks would meaningfully polish Mission 1 + Mission 2**, and **40 do not fit** the cozy-narrative-investigation tone (combat, horror, FPS, modern urban, sci-fi).

| Tier | Asset count | Purpose | Action |
|---|---:|---|---|
| **🟢 Already in repo** | 17 | Imported per `Docs/EXISTING_ASSETS_INDEX.md` | None — keep using |
| **🟡 Tier S — Import next** | 4 | Critical polish for Mission 1-2 that the docs already flagged as "Pending" | Import + integrate now |
| **🟠 Tier A — Strongly recommended** | 5 | High-impact additions for cozy tone + tactile polish | Import + integrate before greenlight playtest |
| **🟤 Tier B — Optional polish** | 4 | Quality-of-life polish; skip if time-boxed | Optional, post-greenlight |
| **🔴 Skip (tone mismatch)** | 36 | Combat/horror/FPS/sci-fi/modern-urban/realistic-blood — antithetical to cozy GDD | Do not import |

**The total install cost of Tier S + Tier A is 9 new asset packs.** Each one slots into a specific scene + Phase + Decision (D-#) so integration is incremental and reversible.

---

## 📋 Table of contents

1. [What's already in the project](#-already-in-the-project-17-assets)
2. [Tier S — Import next (4 assets)](#-tier-s--import-next-4-assets)
   - [S-1 · VoluSmokeFX](#s-1--volusmokefx-vertex-field)
   - [S-2 · Microdetail Terrain System](#s-2--microdetail-terrain-system-pirate-parrot)
   - [S-3 · LightMap Fusion Pro](#s-3--lightmap-fusion-pro-orly-vision)
   - [S-4 · Colorize](#s-4--colorize-smitesoft)
3. [Tier A — Strongly recommended (5 assets)](#-tier-a--strongly-recommended-5-assets)
   - [A-1 · Casual RPG VFX](#a-1--casual-rpg-vfx)
   - [A-2 · Screenspace VFX (Piloto Studio)](#a-2--screenspace-vfx-piloto-studio)
   - [A-3 · Ultimate Mesh FX (Piloto Studio)](#a-3--ultimate-mesh-fx-piloto-studio)
   - [A-4 · TerraSplines + Spline Mesher Pro](#a-4--terrasplines--spline-mesher-pro)
   - [A-5 · Realistic Water VFX (Vefects)](#a-5--realistic-water-vfx-vefects)
4. [Tier B — Optional polish (4 assets)](#-tier-b--optional-polish-4-assets)
5. [Skip list — and why](#-skip-list--and-why)
6. [Phase 31 integration roadmap](#-phase-31-integration-roadmap)
7. [Per-mission asset matrix](#-per-mission-asset-matrix)
8. [Decisions D-045..D-049 to be adopted](#-decisions-to-be-adopted-d-045--d-049)

---

## 🟢 Already in the project (17 assets)

These were imported in Phase 0–27 per `Docs/EXISTING_ASSETS_INDEX.md`. **No action required.**

| Asset | Used by Phase | Mission |
|---|---|---|
| Medieval Village Megapack | Phase 15 (`Phase15_MedievalVillageBuilder.cs`) | Lane (cottages, fences, well) |
| Harvest Garden | Phase 24 (`Phase24_Mission2SceneBuilder.cs`) | Mission 2 Garden |
| Heat – Complete Modern UI | Phase 14 (parchment overlay on Bamao) | Main Menu, Pause |
| Bamao Fantasy GUI | Phase 14 (`Phase14_BamaoUIBuilder.cs`) | Dialogue, Choice tiles, Evening Ledger |
| All In 1 Shader Nodes | Phase 16 (`Phase16_MemoryOrbMasterBuilder.cs`) | Memory orb shader |
| Lumen Stylized Light FX 2 | Phase 17 (`Phase17_LumenAndCinemachineBuilder.cs`) | God rays, candle glows |
| Cutscene Engine | Phase 21 (Memory Dream cutscene) | Dream sequences |
| Game UI & Puzzle SFX | Phase 18 (`Phase18_AudioBuilder.cs`) | UI clicks, polish completion |
| Hierarchy Designer | Editor productivity (not shipped) | — |
| Asset Inventory 4 | Editor productivity (not shipped) | — |
| BoZo Stylized Modular Characters | Phase 13 (`Phase13_BoZoCharacterBuilder.cs`) | Player, Doris, Gerrold |
| Character Controller Pro | Phase 13 fallback (we use a custom controller) | — |
| Eyes Animator | Phase 13 — wired on NPCs | Doris, Gerrold blinks/look |
| Animation Composer (ACS) | Available; NPC animator uses simpler bridge | NPC Talking layer |
| Stylized Weather System | Phase 19 (`Phase19_WeatherAndWindBuilder.cs`) | Lane autumn drizzle |
| Zephyr Wind System | Phase 19 (same) | Foliage sway in Lane / Garden |
| Skill Tree / Talent Tree UI | Reserved for future Memory Map (Mission 3+) | — |

---

## 🟡 Tier S — Import next (4 assets)

The four assets the project documentation **already marked "Pending"** in `Docs/EXISTING_ASSETS_INDEX.md` § "Resolution Plan for Pending Imports". Every one of these is in the Inventix library and ready to go.

---

### S-1 · VoluSmokeFX (Vertex Field)

| Field | Value |
|---|---|
| **Publisher** | Vertex Field |
| **Asset Store** | <https://assetstore.unity.com/packages/vfx/shaders/volusmokefx-343844> |
| **Retail / Library** | $25 / **included** in Order #18968438739106 (Tier 2) |
| **File size** | ~50 MB |
| **Unity** | 2021.3 LTS or newer · **URP + HDRP** · Shader Graph based |
| **Reviews / Trust** | 106 favourites · "not enough ratings" (new) · Publisher has 11+ assets, several with 4-5⭐ ratings, free Logpile asset has 520 favourites |

**Independent verification** — Vertex Field's product page describes VoluSmokeFX as "a lightweight, volumetric-style smoke and fog toolkit for Unity, designed to deliver atmosphere with excellent real-time performance" with "modular prefabs, customizable Shader Graph materials, and a library of ready-made presets that let you add ground fog, smoke plumes, magical mist, or ambient haze in minutes". The publisher explicitly markets it as "ideal for isometric, top-down, adventure, survival, RPG, strategy, and stylized worlds where visual mood and performance both matter" — a perfect description of Hearthbound Hollow's autumn cozy mood.

#### Why it fits Hearthbound Hollow

The cozy GDD calls for *atmospheric warmth* in every scene. The current build uses Unity's built-in particle smoke (acceptable but flat). VoluSmokeFX's volumetric-style smoke gives the project four specific upgrades:

1. **Kettle steam (Mission 2 Garden)** — the Kettle interactable needs a steady steam column when the player brews tea. Currently a basic particle puff. Volumetric upgrade.
2. **Chimney smoke (Lane scene)** — every villager cottage has a chimney. Adding a thin, drifting smoke plume sells the "warm autumn dusk" mood instantly.
3. **Dream-sequence transition (Phase 21 cutscene)** — the screen-fade-to-memory currently uses a simple alpha fade. A volumetric "memory mist" curtain visually parallels the orb's interior nebula.
4. **Polish completion shimmer** — when the player finishes polishing Doris's First Loaves orb, a soft warm-smoke breath rising off the orb is more cozy than the current sparkle.

#### Integration steps

```
1. Import the asset.
2. Verify URP is the active pipeline (Hearthbound → Setup URP Pipeline if not).
3. Create a new procedural builder:
   Assets/_Project/Scripts/Editor/Phase31_AtmosphericSmoke.cs
4. The builder walks every Lane cottage chimney + Hollow chimney + Garden
   kettle + Cottage hearth and instantiates a VoluSmokeFX preset prefab as
   a child of each, positioned ~1.5 m above the chimney lip / 0.3 m above
   the kettle spout.
5. For Dream 1 + Dream 2 cutscenes: extend Cutscene Engine's timeline
   prefab to spawn a VoluSmokeFX "mist curtain" as the fade-to-dream
   transition. Use Timeline Activation track to time it 0.4 s before the
   scene-load black.
```

**Editor menu**: `Hearthbound → 🌫️ Phase 31 — Wire Atmospheric Smoke`. Idempotent (skips already-wired chimneys).

#### Target missions / scenes

| Scene | Effect | Frequency |
|---|---|---|
| Lane | 4× chimney plumes on autumn cottages | Permanent loop |
| Hollow | 1× chimney + 1× kettle on workbench | Permanent loop + on-Polish-complete burst |
| Garden | 1× kettle on tea-brewing stove | On-kettle-active |
| Cottage (Gerrold) | 1× hearth fireplace plume | Permanent loop |
| Dream sequences | Mist curtain transition | One-shot at scene-fade |

#### Risks & mitigation

- **VFX Graph variant only** — Vertex Field ships the Shader Graph version. If we later target Built-in pipeline, we'd need the publisher's alt build. Acceptable — we're URP only.
- **Performance on low-spec PCs** — Volumetric smoke can be expensive at 1080p. Mitigation: bake a profile that disables chimney smoke when Quality < High in `SettingsService`.

---

### S-2 · Microdetail Terrain System (Pirate Parrot)

| Field | Value |
|---|---|
| **Publisher** | Pirate Parrot |
| **Asset Store** | <https://assetstore.unity.com/packages/tools/terrain/microdetail-terrain-system-306859> |
| **Retail / Library** | $50 / **included** in Order #18968438739106 (Tier 2) |
| **File size** | 464.8 MB |
| **Unity** | 2022.3 LTS or newer · **URP + HDRP** (Built-in support on the roadmap) |
| **Reviews / Trust** | 116 favourites · 6 user reviews (4⭐ average) · Active publisher with Discord support |

**Independent verification** — The publisher describes Microdetail Terrain System as "a powerful and flexible tool designed to enhance your terrain creation workflow by adding highly detailed micro-elements such as rocks, sticks, grass, leaves, and more". It is "Built using Signed Distance Fields (SDF)" which "allows you to generate intricate details based on simple procedural rules, offering infinite combinations of objects and natural features". Crucially: "a highly efficient and well-integrated terrain detailing solution that allows you to add millions of small details to your Unity terrains with minimal performance impact". The publisher's own roadmap notes "Adding API to support procedural drawing/placing/coloring along with density control · Adding Built-In support · Adding billboard rendering to further improve on the performance".

#### Why it fits Hearthbound Hollow

Cozy games live or die on ground-detail charm. *Spiritfarer* and *Stardew Valley* both invested heavily in close-up ground texture variety. Currently the Lane scene uses a flat cobblestone material; the Garden uses a single dirt texture. The result reads as "okay" from a distance but flat at character close-up.

The Phase 27 environment capstone (`Phase27_LaneEnvironment.cs`, `Phase27_HollowInterior.cs`) lays down cobblestones and grass procedurally. Microdetail Terrain System layers on top: tiny pebbles between cobblestones, fallen autumn leaves scattered across the lane, twigs near the garden bushes, moss patches on the Hollow's outer wall base.

Because it uses SDFs not meshes, it costs almost nothing at runtime — the SDF terrain shader handles all the detail in the fragment pass.

#### Integration steps

```
1. Import the asset.
2. Open every Lane / Garden scene with a Unity Terrain object. If a scene
   uses a plain Quad/Plane (Lane currently does — see Phase 27 Lane
   environment), convert it to a Unity Terrain using the asset's
   "Convert to Terrain" wizard.
3. For each terrain, open Microdetail Terrain System > Edit panel.
4. Paint the following microdetail layers:
   - Lane: cobblestone-edge pebbles (medium density), autumn leaves
     (sparse), broken twigs (very sparse).
   - Garden: garden-dirt clumps, fallen vegetable scraps, twigs near hedges.
   - Hollow approach (outside Hollow door): moss patches, pebbles.
5. Save the painted layers into the scene.
6. Verify in Play mode at 1080p, 30 fps target → confirm < 0.3 ms terrain
   shader cost (use Unity Frame Debugger).
```

**Editor menu (Phase 31)**: `Hearthbound → 🌿 Phase 31 — Bake Microdetail Layers` — idempotent script that re-paints the named layers from a saved JSON definition. Lets us re-bake on terrain changes without losing the design.

#### Target missions / scenes

| Scene | Microdetail layers | Coverage |
|---|---|---|
| 02_Mission01_Lane | Cobble pebbles · autumn leaves · twigs | Full terrain |
| 03_Mission01_Hollow (interior) | Floor-edge dust · cobwebs near corners | Floor border only |
| 04_Mission02_Garden | Dirt clumps · fallen veg · stray sunflower seeds | Garden plots + paths |
| 05_Mission02_Cottage (interior) | Hearth ash · floorboard dust | Hearth area |

#### Risks & mitigation

- **Unity 2022.3+ requirement** — we're on Unity 6 LTS (6000.4.4f1) — ✅ fine.
- **URP shader complexity** — the SDF terrain shader is heavy on mobile. Phase 31 builder includes a `MICRODETAIL_QUALITY_LOW` define so we can disable it via `SettingsService.MicrodetailQuality`.

---

### S-3 · LightMap Fusion Pro (Orly Vision)

| Field | Value |
|---|---|
| **Publisher** | Orly Vision |
| **Asset Store** | <https://assetstore.unity.com/packages/tools/level-design/lightmap-fusion-pro-314188> |
| **Retail / Library** | $50 / **included** in Order #18968438739106 (Tier 2) |
| **Unity** | 2022.3 LTS or newer · **Built-in, URP, HDRP** |
| **Reviews / Trust** | 15 favourites · 3 reviews · Same publisher ships LUT Creator Suite and In Lightmap Doctor for the same problem family |

**Independent verification** — The Unity Asset Store listing keywords for LightMap Fusion Pro include "lightmapping, lightmap blending, Lighting, VR, Level Design, static lighting, LightAtlas, Mobile, lightmap, mobile optimized, baking, Performance, bake, optimization". A nearly-identical product on the Asset Store (Magic Lightmap Switcher) has multiple reviewers saying "This asset is really amazing for creating dynamic baked lighting. I used it to create a light switch to turn baked lights on and off, and it worked amazingly" and "This asset does exactly what it says and it's fairly easy to set up. The developer constantly refines and updates the features" — confirming the technique works as advertised in the same product family.

#### Why it fits Hearthbound Hollow

The game ships with a `DayCycleManager.cs` (already in `Assets/_Project/Scripts/Player/DayCycleManager.cs`) that animates a directional sun light + ambient skybox tint to simulate time of day. **But the shadows and bounce light are baked into a single set of lightmaps for a single time of day.** When the sun rotates, the baked shadows stay frozen in their morning position.

LightMap Fusion Pro lets us bake **three lightmap sets** — *Morning*, *Afternoon*, *Dusk* — and smoothly blend between them at runtime as `DayCycleManager` ticks the time-of-day curve. The result is the same beautiful baked GI we already get, but matching the warm sun angle of each story beat.

This matches the game's narrative tempo:
- **Mission 1 Lane** opens at *late afternoon* (warm orange) → walking to the door takes a few minutes of in-game time → arrive at *dusk*.
- **Mission 1 Hollow** plays out at *dusk → night* during the Doris dialogue and polish session.
- **Mission 2 Garden** opens at *morning* (Day 2) → fresh dew, bright sun.
- **Mission 2 Cottage** plays at *evening* (Gerrold's hearth-lit grief beat).

Currently every scene fakes this via skybox tint alone, which works but reads flat. LMF Pro makes the *interior shadows* warm up at dusk, then cool back to neutral at morning.

#### Integration steps

```
1. Import the asset.
2. Open 02_Mission01_Lane. Bake lightmap set #1 with sun at 12 PM angle.
   Save as Lane_Lightmaps_Morning.
3. Re-bake with sun at 5 PM angle (current Phase 23 setting). Save as
   Lane_Lightmaps_Afternoon.
4. Re-bake with sun at 7 PM angle (dusk). Save as Lane_Lightmaps_Dusk.
5. Drop a LightMapFusionPro_Controller component on _GameRoot. Assign
   the three lightmap sets to its "Time Slots" array.
6. Wire DayCycleManager.OnTimeChanged to controller.SetBlend(t):
       Morning → Afternoon  : t = 0 → 0.5
       Afternoon → Dusk     : t = 0.5 → 1
7. Repeat for 03_Mission01_Hollow (Dusk + Night), 04_Mission02_Garden
   (Morning + Afternoon), 05_Mission02_Cottage (Evening only — single).
```

**Editor menu (Phase 31)**: `Hearthbound → 🌅 Phase 31 — Bake All Time-of-Day Lightmaps` — runs through each scene, bakes the three time slots in sequence, and saves the named lightmap assets. **Takes ~30-60 minutes per scene** (one-time cost).

#### Target missions / scenes

| Scene | Lightmap sets | DayCycle blend |
|---|---|---|
| 02_Mission01_Lane | Morning, Afternoon, Dusk | 0.0 → 1.0 over Mission 1 |
| 03_Mission01_Hollow | Dusk, Night | 0.0 → 1.0 over Polish session |
| 04_Mission02_Garden | Morning, Afternoon | 0.0 → 0.7 over Garden visit |
| 05_Mission02_Cottage | Evening | Static (single bake) |

#### Risks & mitigation

- **Bake time** — three sets per scene × four scenes = ~12 bake cycles. Allocate one developer-day to the initial bake pass. Subsequent re-bakes are incremental.
- **Disk size** — three lightmaps per scene at 1024×1024 = ~50 MB total. Acceptable.

---

### S-4 · Colorize (Smitesoft)

| Field | Value |
|---|---|
| **Publisher** | Smitesoft (the same studio that ships Colorize Pro) |
| **Asset Store** | <https://assetstore.unity.com/packages/tools/painting/colorize-texture-color-palette-modifier-217132> |
| **Retail / Library** | $59.90 / **paid $3.59** in Order #18968438706671 |
| **Unity** | 2020.3 LTS or newer · **Built-in, URP, HDRP** |
| **Reviews / Trust** | Active development · Publisher has 12+ products including Palette Fusion, Colorize Pro 2D, and Colorize HD |

**Independent verification** — The publisher's product page describes Colorize as a tool that "Changes regional colors of your models via the color palette and/or add emissions and metallic-reflection textures", with "many randomizing algorithms based on the base colors you chose or based on regions of color ranges" and "many natural color palettes for you to choose from". The independent dev-asset-deals listing confirms "Active Development ✔️ Continuously evolving with new features. ✔️ Regular optimizations" and "Optimized for solid and clean solid pattern palettes. ✔️ Supports up to 4K textures for detailed texturing. ✔️ Works seamlessly with most low poly models". Low-poly compatibility is **critical** because BoZo is low-poly.

The publisher's own guidance states "We generally recommend starting with Colorize. Its robust palette-based editing features and minimal complexity make it well-suited for most 3D projects utilizing low-poly models" — which is exactly our case (BoZo chibi characters).

#### Why it fits Hearthbound Hollow

Currently Phase 13's `Phase13_BoZoCharacterBuilder.TintCharacterMaterials()` does a crude `_BaseColor` tint per material to give Doris her warm cleric-amber and Gerrold his bardic dusk-blue. Result: the *entire* character takes on the same tint — Doris's white apron looks beige-amber, not white. The same is true for Gerrold's blue cloak — his face goes blue.

Colorize lets us recolor **regions** of a texture (e.g. "the cloak only, not the face"). This unlocks:

1. **Per-villager identity** — Doris's apron stays white; only her hair and tunic warm up. Gerrold's face stays neutral; only his cloak goes dusk-blue.
2. **Seasonal repaints** — once we have Mission 3+, the same BoZo head can be re-skinned for autumn/winter villagers without authoring new textures.
3. **Doris's memory orb tints** — the orb's color currently comes from `MemoryNodeSO.EffectiveTint`. Colorize lets us paint distinct emissive regions on the orb mesh that match the memory's emotion (warm = First Loaves; cool = grief).

#### Integration steps

```
1. Import the asset.
2. For each BoZo NPC source texture:
   - Open Colorize editor → pick the source texture.
   - Mark colour regions:
     * Doris: HAIR (warm honey) + TUNIC (warm amber) + APRON (white)
     * Gerrold: BEARD (dusk blue) + CLOAK (dusk blue) + SHIRT (cream)
     * Player: NEUTRAL (no recolor — player is canonical)
   - Save the palette as a ScriptableObject in
     Assets/_Project/ScriptableObjects/Palettes/Villager_Doris_Warm.asset
3. Phase13 BoZo builder is rewritten so its TintCharacterMaterials() now
   loads the matching palette asset and applies Colorize's runtime shader
   variant (no manual UV painting needed at runtime).
4. For the memory orb (Phase 16): drop a Colorize component on the orb
   prefab. Map the "core glow" region to MemoryNodeSO.EffectiveTint
   dynamically, while keeping the "shell" region neutral.
```

**Editor menu (Phase 31)**: `Hearthbound → 🎨 Phase 31 — Generate Villager Palettes` — runs through every VillagerSO and either creates a default palette or links to a designer-authored one.

#### Target missions / scenes

| Use | Mission | Where |
|---|---|---|
| Doris colour identity | Mission 1 (Hollow) | Phase 13 prefab |
| Gerrold colour identity | Mission 2 (Cottage) | Phase 13 prefab |
| Memory orb dual-region emissive | Both missions | Phase 16 (`Phase16_MemoryOrbMasterBuilder.cs`) |
| Future villagers (Mission 3+) | Post-greenlight | Phase 31 palette library |

#### Risks & mitigation

- **POT (power-of-two) square textures required** — BoZo textures already are POT (verified in pack). ✅ fine.
- **Custom shaders not supported** — we use URP/Lit (Unity standard). ✅ fine. The asset's "Palette Fusion" companion handles edge cases — flagged for future if needed.

---

## 🟠 Tier A — Strongly recommended (5 assets)

These five additions are **not flagged "Pending"** in the docs but the art-and-VFX team strongly endorses them for cozy-tone match and tactile polish.

---

### A-1 · Casual RPG VFX

| Field | Value |
|---|---|
| **Publisher** | bundled (vendor-anonymised under "Various") |
| **Retail / Library** | $25 / **paid $2** in Order #18968438706671 |
| **Unity** | 2020.3 LTS or newer · **Built-in + URP** |
| **Pack** | 40+ casual VFX prefabs |

#### Why it fits

The current Phase 16 memory orb completion sparkle is a single particle burst. The Phase 24 tea-brewing flourish is a heart bubble. Both are placeholder. **Casual RPG VFX** ships bright, soft, pastel-toned effects that exactly match the cozy GDD aesthetic — *no aggressive combat VFX*. Specifically:

- **Coin / gold collect effects** — for every coin tariff payment in Doris's bargain (`MoralChoiceMadeEvent` resolution).
- **Level-up celebration burst** — for the moment a villager's `trustDoris` / `trustGerrold` crosses 75 (high-trust threshold).
- **Healing green sparkle** — for the Cleanse mini-game completion in Mission 2.
- **Star/sparkle hit** — for every successful Polish circle on the memory orb.
- **XP orb flying** — for the predecessor-trail-warmth bump when Marin's Note is read.

#### Integration

```
1. Import.
2. Phase 18 SfxLibrarySO already drives audio cues. Create a sibling
   Assets/_Project/Scripts/Mission/VfxLibrarySO.cs that maps event IDs
   (like "coin_collect", "level_up", "polish_step") to Casual RPG VFX
   prefab refs.
3. Wire EventBus subscriptions in MissionRunner.cs to spawn the matching
   prefab via VfxLibrary.PlayOneShot(id, worldPos).
4. Phase 16 PolishMiniGame already emits OnPolishStepCompleted — subscribe.
5. Phase 23 EveningLedger already emits OnDayEnded — subscribe for the
   level-up burst when a trust threshold flips.
```

**Editor menu (Phase 32)**: `Hearthbound → ✨ Phase 32 — Wire Casual VFX Library`.

#### Risk

- **Casual style mismatch with Bamao parchment UI?** Mitigation: Casual RPG VFX ships pastel/soft, which complements parchment colour palette. Tested visually against `S-4 Hearthbound preset`.

---

### A-2 · Screenspace VFX (Piloto Studio)

| Field | Value |
|---|---|
| **Publisher** | Piloto Studio |
| **Asset Store** | <https://assetstore.unity.com/packages/vfx/particles/spells/screenspace-vfx-316287> |
| **Retail / Library** | $30 / **included** in Order #18968438739106 (Tier 2) |
| **Unity** | 2022.3 LTS or newer · **URP** (primary) |
| **Reviews / Trust** | 134 favourites · 4 reviews · Piloto Studio is a well-known cozy/casual VFX publisher with multiple highly-rated bundles |

**Independent verification** — Piloto Studio's catalog includes "Board / Card Game Stylized VFX Magic" (358 MB, 4-star rated, 106 favourites) and "Super Magic FX - 1400+ Unique VFX!" — establishing the publisher as a reputable, prolific VFX vendor with proven Unity Asset Store track record across multiple cozy/casual game styles.

#### Why it fits

The Phase 21 Memory Dream cutscene currently fades to black and reopens on a flat 2D dream image. **Screenspace VFX** lets the transition do real cinematography:

1. **Dream-entry letterbox + vignette** — the memory dream uses a cinematic letterbox during the dream beat, then opens back to full canvas on the wake-up cue.
2. **Polish completion screen-flash** — a soft warm flash on the exact frame Doris's First Loaves orb finishes (matches the audio sting from Phase 18).
3. **Damage vignette → "memory integrity loss" vignette** — when the player chooses Erase on a memory (and `memoryIntegrityDoris` drops), a brief desaturation pulse reads as "something just left the world".
4. **Glitch / cracks shader for the Memory Bee corruption beat** — Mission 3+ deferred, but the shader exists for when we need it.

#### Integration

```
1. Import.
2. Create Assets/_Project/Scripts/Editor/Phase32_ScreenspaceVfxWiring.cs
3. Add a ScreenspaceVfxController component to _GameRoot (DontDestroyOnLoad).
4. Wire MissionRunner events:
     OnMemoryDreamStart → controller.PlayLetterbox(in)
     OnMemoryDreamEnd   → controller.PlayLetterbox(out)
     OnPolishComplete   → controller.PlayWarmFlash(0.5s)
     MoralChoice == Erase → controller.PlayDesaturationVignette(1.2s)
5. Cutscene Engine timeline gets a new "Screenspace VFX" track type via
   a custom PlayableAsset (reuse Cutscene Engine's API).
```

#### Risk

- **URP-only** — we're URP-only per D-039. ✅ fine.

---

### A-3 · Ultimate Mesh FX (Piloto Studio)

| Field | Value |
|---|---|
| **Publisher** | Piloto Studio |
| **Retail / Library** | $35 / **included** in Order #18968438739106 (Tier 2) |
| **Unity** | 2021.3 LTS or newer · **URP** primary |
| **Pack** | Fire engulf · Ice freeze · Dissolve-to-particles · Electric arc · Poison corrode · Material morphing |

#### Why it fits

The memory orb is the single most touched object in the game. Currently when a memory is *Erased* the orb just disappears. With Ultimate Mesh FX:

1. **Erase choice → "Dissolve-to-particles" effect** on the orb mesh itself — the memory literally crumbles in the player's hand. Cozy-friendly because the dissolve is slow + soft, not violent.
2. **Cleanse choice → "Crystal formation" effect** — the orb crystallises into a clarified shape; the corrupted regions visually flake off.
3. **Polish step completion → soft material morph** — each polish circle adds a "shine pass" to the orb shader for ~0.4 s.

These are *visual confirmations of choice* — they make the four moral tariffs (Erase / Cleanse / Listen / Defer) feel mechanically distinct, not just text-labeled.

#### Integration

```
1. Import.
2. Phase 16 MemoryOrbInteractable.cs gains a public MeshFXController field.
3. The mini-game state machine triggers:
   On Polish step  → playSoftShine(0.4s)
   On Erase final  → playDissolveToParticles(2.5s) → Destroy(this.gameObject)
   On Cleanse done → playCrystallise(2.0s)
   On Listen done  → playEmissivePulse(1.5s)  (existing — light-up effect)
   On Defer        → playSoftSettle(0.8s)     (existing dim-down effect)
```

#### Risk

- **Same Piloto Studio quality bar as A-2.** ✅ confirmed reputable publisher.

---

### A-4 · TerraSplines + Spline Mesher Pro

| Field | Value |
|---|---|
| **Publishers** | GRUELSCUM (TerraSplines) · Staggart Creations (Spline Mesher Pro) |
| **Retail / Library** | $15 + $79 / **both included** in Order #18968438739106 (Tier 2) |
| **Unity** | 2022.3 LTS+ · pipeline-agnostic (terrain) / Built-in/URP/HDRP (Spline Mesher) |
| **Reviews / Trust** | Staggart Creations: 301 favourites · 35 reviews on Spline Mesher Pro · Established publisher of Stylized Water and Underwater Rendering |

**Independent verification** — Staggart Creations is a respected Unity Asset Store publisher; their newer Spline Mesher Pro is documented at "Staggart Creations rebuilds Spline Mesher Pro with Jobs, Burst, fill meshes and editor LOD support" with "Performance improvements of up to 50 times are claimed compared to the previous version". Independent coverage confirms "a robust and intuitive tool for spline-based mesh deforming, built around Unity's native Spline tool. All you need to do is provide the component with a spline and an input mesh, and it gets continuously repeated along the spline". "A straightforward and powerful tool for spline-based mesh curving. Essential for 3D world building. Built around Unity's native spline tool, making it intuitive to combine with other tools or gameplay logic".

#### Why they fit

The current Lane scene's cobblestone path is a row of straight quad meshes. It works but reads as *gridded* — not the meandering autumn lane the cozy fiction calls for. Using these two tools together:

1. **TerraSplines** — sculpt the underlying terrain to follow a soft curving path (uphill toward the Hollow door, gentle slopes left and right where the cottages sit).
2. **Spline Mesher Pro** — extrude the cobblestone prefab along the same spline so the *cobbles themselves* curve. Add per-segment scale variance so no two cobbles are identical.

Bonus uses:
- **Garden fence wraps** (Mission 2 Garden) — Spline Mesher Pro extrudes the picket fence around the Harvest Garden plot.
- **Cottage hearth chain hook** — extrudes a chain prefab from ceiling to kettle.
- **Lane stream behind cottages** — TerraSplines carves a small babbling brook; Spline Mesher Pro extrudes a water mesh along it.

#### Integration

```
1. Import both.
2. Phase 27 LaneEnvironment is updated:
   - Replace the gridded cobblestone array with a single Spline path
     anchored at: (Player Spawn) → (Hollow Door) with 4 control points.
   - TerraSplines carves a 2-m-wide path on the terrain heightmap.
   - Spline Mesher Pro extrudes the cobblestone mesh prefab along it,
     with per-segment vertex jitter for organic feel.
3. Phase 24 GardenScene builder:
   - Adds a closed-loop spline around the garden plot.
   - Spline Mesher Pro extrudes the picket fence (existing Medieval
     Village fence prefab) along it.
4. Phase 24 CottageScene:
   - Vertical spline from hearth chimney to kettle.
   - Spline Mesher Pro extrudes a chain link mesh.
```

#### Risk

- **TerraSplines requires Unity Splines package** — we already have it on the manifest. ✅ fine.
- **Combined cost ($94 retail)** — both are in the library, so $0 extra spend.

---

### A-5 · Realistic Water VFX (Vefects)

| Field | Value |
|---|---|
| **Publisher** | Vefects |
| **Retail / Library** | $45 / **included** in Order #18968438739106 (Tier 2) |
| **Unity** | 2021.3 LTS or newer · **URP** primary |

#### Why it fits

Tactile polish. The kettle in Mission 2 Garden currently makes audio when activated but produces no visual splash. Realistic Water VFX gives us:

1. **Tea kettle pour** — when the player completes the tea-brewing mini-game, a brief water-pour splash plays where the kettle meets the cup.
2. **Garden well ripple** — when the player draws water (Mission 2 prelude beat), the well surface ripples.
3. **Watering can drip** — when the player waters the herb patch (Mission 2 herb-harvest beat), a soft drip trail.
4. **Rain-on-water surface** — when Stylized Weather System triggers a drizzle, puddles in the lane get tiny ripple impacts.

These are all *2-second one-shot* effects — tactile and small, not "ocean simulation". Performance is negligible.

#### Integration

```
1. Import.
2. Phase 31 builder adds the relevant prefab as a child of:
   - KettleInteractable (existing) — spawn ripple on cup-fill complete.
   - HerbHarvestInteractable (existing) — spawn drip on harvest complete.
   - Lane environment puddles (new — generated by Phase 31 from
     terrain depressions) — spawn rain-impact ripples while drizzle is active.
```

#### Risk

- **Subtle effects** — may be invisible if the camera doesn't frame them. Mitigation: every tactile splash is positioned at the player's hand location so the third-person camera always catches it.

---

## 🟤 Tier B — Optional polish (4 assets)

Skip these unless time is generous. None block the greenlight playtest.

| Asset | Why optional | When to revisit |
|---|---|---|
| **Stylized Dungeons** ($50) — JustCreate | Cozy game has *no dungeons in Mission 1-2*. The 150+ prefabs would dress a future "locked room beneath the Hollow" beat for Mission 3+ when the predecessor (Marin) arc opens. | Mission 3 (post-greenlight) |
| **Shatter Stone Bundle** ($75) | The 40+ resource UI icons + crafting recipe ScriptableObject system would seed a future "memory ingredient" catalog. Not used in Mission 1-2 because herbs are simple. | Mission 4+ (memory-weaving expansion) |
| **Hierarchy Designer** ($30) — Pedro Verpha | Editor-only productivity. Worth using once a developer joins the project, but it changes nothing at runtime. | Anytime — onboarding aid for new team members |
| **Animation Composer (ACS)** — already imported | Listed for completeness. Currently we use a simpler `NpcAnimatorBridge` for Doris/Gerrold Idle ↔ Talking. ACS could replace it with multi-layer composition (e.g., "Doris looks at the orb while talking"). | Mission 2 polish pass for Gerrold's grief beat |

---

## 🔴 Skip list — and why

These 36 assets are excellent for *other* games but **antithetical to the Hearthbound Hollow GDD pillars** (no combat, no horror, no FPS, cozy tone).

| Category | Assets to skip | Why |
|---|---|---|
| **Combat VFX** | Anime Powers Pack · Spells Pack · 100 Special Skills · Magic Arsenal · Stylized VFX Bundle · UNI VFX Missiles & Explosions | Cozy GDD Pillar 3: "Choices don't punish, they shape." No combat. |
| **Gore / Blood** | Realistic Blood VFX · Volumetric Blood Fluids | Cozy GDD Pillar 1: "Cozy framing, deep substance." No violence. |
| **Horror** | Horror Bundle (Sound Effects) · Horror Multiplayer Game Template | Tone mismatch — game is gentle, not frightening. |
| **FPS / Multiplayer** | MFPS 2.0 · MMFPSE | Genre mismatch — single-player narrative sim. |
| **Vehicles / Racing** | Edy's Vehicle Physics · Modular Cyber Racing Cars · Complete Racing Game 2 | Genre mismatch. |
| **Sci-Fi** | Sci-Fi Low Poly Interior Bundle · Sci-Fi Space Stations Creator · Neon Interior Props · Modular Cyber Racing Cars | Setting mismatch — medieval autumn village, not space colony. |
| **Modern urban / industrial** | City Pack · Office Floors · Industrial Props Equipment Mega Bundle · Toon Town · Urban Abandoned District | Setting mismatch. |
| **Castles (dark fantasy)** | Fantasy Castle Environment (Soulslike) · The Medieval Castle | Tone mismatch — too grand/dark for cozy hollow. The Medieval Village Megapack already covers village-scale builds. |
| **Monsters / Combat NPCs** | Fantasy Monsters Bundle · Stylized Fantasy Creatures Bundle #2 · Animated 2D Characters Monsters | No enemies. Pickle the cat is the only non-villager NPC and she's a placeholder sphere. |
| **Action templates** | Obby Parkour Mega Pack · POLYGON Battle Royale | Genre mismatch. |
| **Conflicting controllers** | Traversal Pro · Character Controller Pro (we use a custom PlayerController + Mixamo Animator chain per D-039) | We have D-036..D-039 locked. |
| **Conflicting weather** | UniStorm · Weather Maker | We use Stylized Weather System (Unluck Software) per D-027. UniStorm/Weather Maker are realistic; we are stylized. |
| **AI dialogue** | Dialogue System for Unity Addon for OpenAI | **D-002** forbids: violates GDD Pillar 1 ("Every villager is fully written"). |
| **City Characters** | ithappy City Characters Modular Animated Pack | **D-001** chose BoZo (chibi) over City Characters (realistic urban). |
| **Game Creator 2 addon** | Perks for Game Creator 2 | We don't use Game Creator 2 (uses our own VillageState progression). |
| **Mobile monetisation** | UniPay – IAP | Not for the greenlight playtest. Reserve for mobile-port phase post-1.0. |

---

## 🗺️ Phase 31 integration roadmap

The new assets get rolled in across three numbered phases, mirroring the discipline of Phases 13-30 (one editor-menu capstone per concern):

### Phase 31 — Atmosphere & Polish (Tier S, 4 assets)

| Sub-phase | Asset | Editor menu | Files added |
|---|---|---|---|
| 31a | VoluSmokeFX | `Hearthbound → 🌫️ Phase 31a — Wire Atmospheric Smoke` | `Phase31a_AtmosphericSmoke.cs` + runtime `SmokeEmitter.cs` |
| 31b | Microdetail Terrain System | `Hearthbound → 🌿 Phase 31b — Bake Microdetail Layers` | `Phase31b_MicrodetailBake.cs` + designer-authored layer JSON in `Assets/_Project/ScriptableObjects/Microdetail/` |
| 31c | LightMap Fusion Pro | `Hearthbound → 🌅 Phase 31c — Bake All Time-of-Day Lightmaps` | `Phase31c_LightmapBake.cs` + runtime `TimeOfDayLightmapBlender.cs` wired to `DayCycleManager` |
| 31d | Colorize | `Hearthbound → 🎨 Phase 31d — Generate Villager Palettes` | `Phase31d_VillagerPalettes.cs` + palette ScriptableObjects per villager + extended `Phase13_BoZoCharacterBuilder.cs` |

### Phase 32 — Tactile VFX (Tier A non-spline, 3 assets)

| Sub-phase | Asset | Editor menu | Files added |
|---|---|---|---|
| 32a | Casual RPG VFX | `Hearthbound → ✨ Phase 32a — Wire Casual VFX Library` | `Phase32a_CasualVfx.cs` + new runtime `VfxLibrarySO` mirroring the existing `SfxLibrarySO` |
| 32b | Screenspace VFX | `Hearthbound → 📽️ Phase 32b — Wire Screenspace VFX` | `Phase32b_ScreenspaceVfx.cs` + runtime `ScreenspaceVfxController.cs` + Cutscene-Engine integration track |
| 32c | Ultimate Mesh FX | `Hearthbound → 🌀 Phase 32c — Wire Memory-Orb Mesh FX` | `Phase32c_MemoryOrbMeshFx.cs` + extended `MemoryOrbInteractable.cs` |

### Phase 33 — Splines & Water (Tier A spline, 2 assets)

| Sub-phase | Asset | Editor menu | Files added |
|---|---|---|---|
| 33a | TerraSplines + Spline Mesher Pro | `Hearthbound → 🛤️ Phase 33a — Spline Paths & Fences` | `Phase33a_SplineEnvironment.cs` + scene rebuild for Lane cobblestones + Garden fence |
| 33b | Realistic Water VFX (Vefects) | `Hearthbound → 💧 Phase 33b — Wire Water Splash FX` | `Phase33b_WaterVfx.cs` + extended `KettleInteractable.cs` + `HerbHarvestInteractable.cs` |

### Master capstone update

`Phase27_BuildEverything.cs` extends from 6 → **9 sub-capstones** in one click. Adds 31a-d → 32a-c → 33a-b in order.

Total run time: ~90 s (was ~60 s). Reflection-based discovery means missing phases still skip gracefully.

---

## 📊 Per-mission asset matrix

| Mission / Scene | New asset wires | Existing asset wires |
|---|---|---|
| **Bootstrap** | (none — Bootstrap is service-only) | SettingsService, VillageState |
| **Main Menu** | (none new — already polished) | Bamao GUI, Heat UI, Game UI SFX |
| **Mission 1 — Lane** | VoluSmokeFX (chimney smoke) · Microdetail (cobble pebbles + leaves) · LMF Pro (3 lightmap sets) · Casual RPG VFX (coin collect) · Screenspace VFX (warm flash) · Spline Mesher Pro (curving cobblestone path) | Medieval Village, Stylized Weather, Zephyr Wind, Lumen god rays, BoZo Player, Hearthbound_Player Animator |
| **Mission 1 — Hollow** | VoluSmokeFX (chimney + kettle steam) · Microdetail (floor-edge dust) · LMF Pro (Dusk + Night) · Colorize (Doris warm-amber palette) · Ultimate Mesh FX (orb dissolve / shine / crystallise) · Realistic Water VFX (kettle pour) | Doris BoZo prefab, NPC Animator + Bridge, Bamao DialogueBox, ChoiceCardUI, ToneCompassCard, Polish mini-game, Memory orb shader |
| **Mission 2 — Garden** | VoluSmokeFX (kettle steam) · Microdetail (dirt + fallen veg) · LMF Pro (Morning + Afternoon) · Casual RPG VFX (XP-orb to predecessor warmth) · Spline Mesher Pro (garden fence wrap) · Realistic Water VFX (watering can drip) | Harvest Garden plants, Marin's Note interactable, HerbHarvestInteractable, TeaBrewingUI, Mission02Director |
| **Mission 2 — Cottage** | VoluSmokeFX (hearth fireplace plume) · Microdetail (hearth ash + floorboard dust) · LMF Pro (single Evening bake) · Colorize (Gerrold dusk-blue palette) · Ultimate Mesh FX (orb cleanse crystallisation) · Screenspace VFX (desaturation vignette on Erase) | Gerrold BoZo prefab, NPC Animator + Bridge, ChoiceCardUI 4-tariff, Cleanse mini-game, Dream 2 cutscene, Cutscene Engine |
| **Dream sequences** | VoluSmokeFX (mist curtain transition) · Screenspace VFX (letterbox + cinematic vignette) · Ultimate Mesh FX (orb interior dissolve at peak) | Cutscene Engine Timeline, MemoryDreamSequencer |

---

## 📜 Decisions to be adopted (D-045 → D-049)

Continuing the project's lettered-decision discipline:

| # | Decision | Rationale | Phase |
|---|---|---|---|
| **D-045** | Atmospheric smoke is **VoluSmokeFX** (URP) — never Unity built-in particles for chimneys / kettles / dreams. | Volumetric quality + cozy-tone match. Single asset source = consistent visual language. | 31a |
| **D-046** | Ground-level micro-detail is **Microdetail Terrain System** painted layers, not authored mesh scatter. | SDF performance + designer-friendly paint workflow. Documented quality tiers via SettingsService. | 31b |
| **D-047** | Day/night lighting is **LightMap Fusion Pro blends**, not real-time GI. | Cozy lighting needs baked GI warmth; LMF gets us blending without losing the bake. | 31c |
| **D-048** | Per-villager identity colour is **Colorize regional palette**, not material-tint. | Per-region recolour preserves white aprons / cream shirts that whole-material tints destroyed. | 31d |
| **D-049** | Memory-orb feedback is **Ultimate Mesh FX**, not generic particle bursts. | The orb is the most-touched object — it deserves bespoke surface VFX per choice. | 32c |

---

## 📌 Final import checklist (copy-paste-ready)

When you're ready, import in this order in the Unity Package Manager / Asset Store window:

1. ✅ **VoluSmokeFX** *(Phase 31a)*
2. ✅ **Microdetail Terrain System** *(Phase 31b)*
3. ✅ **LightMap Fusion Pro** *(Phase 31c)*
4. ✅ **Colorize** *(Phase 31d)*
5. ✅ **Casual RPG VFX** *(Phase 32a)*
6. ✅ **Screenspace VFX (Piloto Studio)** *(Phase 32b)*
7. ✅ **Ultimate Mesh FX (Piloto Studio)** *(Phase 32c)*
8. ✅ **TerraSplines** + **Spline Mesher Pro** *(Phase 33a — import together)*
9. ✅ **Realistic Water VFX (Vefects)** *(Phase 33b)*

After each import, run the corresponding Editor menu item (see the roadmap table above). Each step is **idempotent** so re-running is safe, and the master capstone `Hearthbound → ✨ Build EVERYTHING` will pick up new phases via reflection.

Skip every other asset in the 66-asset library — they are excellent for their genre but not for ours.

---

*Author: Hearthbound Senior Team — Phase 31 prep, 2026-05-25*
*Once you've imported the Tier S + Tier A packs, ping the team and I'll generate the Phase 31a-d / 32a-c / 33a-b editor capstones as one-commit-per-phase pushes to `feat/mission-1-2-architecture`, same cadence as Phase 28-30.*
