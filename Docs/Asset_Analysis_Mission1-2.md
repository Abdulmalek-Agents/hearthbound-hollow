# 🎨 Hearthbound Hollow — Asset Analysis for Mission 1 & 2

> **Authors:** Creative Director · Game & Level Designer · Unity Asset Engineer team · Senior Unity Developer · Critic & Review Board
> **Status:** ⚠️ Approved with Notes (see §6)
> **Source documents reviewed:** `GAME_DESIGN.md`, `EXPERT_REVIEW_EN.md`, `SUMMARY_EN.md`, `Docs/Unity_Assets_Master_Reference.md` (66 assets across 3 orders)
> **Unity project version:** 6000.4.4f1 (Unity 6 LTS)

---

## 1. Purpose

This document is the **single source of truth** for which Unity Asset Store packages the studio is importing into the `hearthbound-hollow` project to build a playable Mission 1 (and the foundation for Mission 2). It evaluates every asset purchased against:

- The five Pillar Mechanics in the GDD (Orbs, Workbench, Ledger, Choice, Garden)
- The Production Pillars (tactile-over-textual, choices shape not punish, cozy framing, no combat/death/grind)
- Mission 1 & 2 scope as defined by the Game & Level Designer (§3 below)
- Mobile-first technical budget (60 fps mid-range Android/iOS target)
- The Critic & Review Board's recommendation to cut villager count 30 → 10–12 and ship with **Polish + Cleanse only** in v1.0

---

## 2. Creative Direction Anchor

Hearthbound Hollow is a **cozy narrative simulation**. Its emotional north star is **Spiritfarer × Strange Horticulture, by candlelight, in autumn**. There is no combat, no death, no grind. The 66-asset Inventix bundle was bought largely on a $2-per-asset sale and contains many packages for completely different genres (FPS, racing, sci-fi, horror, dungeon-crawler). The team will be honest about that and only import what serves *this* game's coherence. Importing irrelevant assets bloats the project, the mobile build, the mental model of every reviewer, and the technical debt going forward.

---

## 3. Mission 1 & 2 Scope (drives asset selection)

### Mission 1 — *"Opening the Hollow"* (tutorial, ~25–35 min)
- **Locations:** Village lane at dawn (linear approach) → Hollow shop exterior → Hollow interior (main room + workbench nook) → small back garden glimpse
- **NPCs:** 1 fully-written villager (Doris the Baker) + 1 silent background villager in the lane
- **Mechanics taught:** Movement, interact, dialogue + choice, inspect memory orb, **Polish** mini-game, place orb on shelf vs return, evening ledger, save, first Memory Dream
- **Tone:** Warm, slightly dusty, late afternoon light shafts, wind in leaves, wood-burning stove crackle

### Mission 2 — *"The Widower's Request"* (emotional weight intro, ~30–40 min)
- **Locations:** Hollow interior + workbench + **herb garden** (new) + short village walk to widower's cottage
- **NPCs:** Widower (heavy-write) + 2 returning lane villagers + Doris briefly
- **Mechanics taught:** Herb harvesting → tea brewing (light loop), **Cleanse** mini-game, first real moral choice (erase / partial / refuse), Memory Map first connection, second Memory Dream
- **Tone:** Still cozy. One quiet, heavy beat. Soft rain optional. Candlelight.

### Asset Category Demand
| Need | Priority |
|---|---|
| Cozy autumn village environment (modular, exterior + interior) | 🔴 Critical |
| Herb garden / cozy farm props | 🔴 Critical |
| Modular humanoid NPC characters (villagers, civilian, expressive) | 🔴 Critical |
| Glass / orb shader (refraction, glow, cracks, dissolve) | 🔴 Critical |
| Warm light VFX (god rays, candle glow, halos) | 🔴 Critical |
| Cozy UI framework (menu, HUD, inventory, dialogue, ledger) | 🔴 Critical |
| Dialogue + cutscene engine | 🔴 Critical |
| Player character controller (third-person, gentle, mobile-ready) | 🔴 Critical |
| Cozy SFX (UI clicks, jar/orb, puzzle success, ambient autumn) | 🔴 Critical |
| Memory Map / node-tree visualizer | 🟡 High |
| Lifelike NPC eyes & idle animation composition | 🟡 High |
| Stylized weather (rain, fog, wind for autumn mood) | 🟡 High |
| Editor productivity (hierarchy, asset inventory) | 🟡 High |
| Combat VFX, weapons, monsters, vehicles, FPS, sci-fi, blood | ⚫ Not for this game |

---

## 4. Asset Verdict Tiers

- 🟢 **TIER S** — Import for Mission 1; central to gameplay
- 🟡 **TIER A** — Import for Mission 1–2; supporting / polish
- 🟠 **TIER B** — Useful Mission 3+ or expansion; hold
- 🔴 **TIER C** — Wrong genre/tone for this project; do **not** import

---

## 5. TIER S — Import for Mission 1 (the foundation)

These ten packages are the spine of the Mission 1 build. Without them we cannot ship a playable slice of Hearthbound Hollow.

### S-1. Medieval Village Megapack
- **Order:** $2+ Sale (22) · Retail $179.99 · Paid $3.60
- **Use in HH:** The autumnal village skeleton. Houses, inn, market stalls, well, signage, dirt roads, fences, foliage, ambient audio (roosters, murmur). Two pre-built demo scenes give us the Mission 1 lane in under an hour.
- **Mission 1 specifics:** Re-tint materials to warm orange/ochre, swap green leaves for autumn variants, place 6–8 cottage exteriors along the lane, dress the Hollow shop exterior (use the "Inn" or "Shop" building as base).
- **Mission 2 specifics:** Place the widower's cottage as a destination at the end of a short side lane. Use the well/town-square dressing for the central village beat.
- **Integration notes:** Mark prefabs as Addressables tagged `Village/Mission01` and `Village/Mission02` so we can stream content per mission.
- **Risk:** ~1.5 GB file size. Mitigation: aggressive ASTC texture compression on import, decimate any 8K textures to 2K, strip building variants we never use.

### S-2. Harvest Garden
- **Order:** Must-Have Bundle 2026 (Tier 2) · Retail $25 · Paid (bundled in $68.99)
- **Use in HH:** The herb garden behind the Hollow (Pillar Mechanic E). Crops, watering can, hoe, basket, raised beds, fencing — **autumn variants are included**.
- **Mission 2 specifics:** Player harvests lavender + valerian (re-skin two crop models with new icons/names) → tea brewing UI prompt. The watering can model becomes a daily interaction prop.
- **Integration notes:** Author a `MemoryHerb` ScriptableObject layer over the crop prefabs so each plant carries an `effect` field (Calm, Open-Up, Forget-Briefly) for the GDD's tea mechanic.

### S-3. Heat – Complete Modern UI
- **Order:** $2+ Sale (16) · Retail $69.99 · Paid $2.80
- **Use in HH:** The system UI framework — main menu, loading screen, settings, pause, HUD, inventory, scoreboard (repurposed as Ledger Summary), end-game screen (repurposed as Mission Complete), modal popups, notification toasts. Mobile-responsive out of the box.
- **Mission 1 specifics:** Main Menu ("Open The Hollow" button), Loading Screen with cozy tip ("Some memories want to be sold. Some don't."), Pause Menu, HUD with day-of-week display and current memory-in-hand icon, Mission Complete screen showing the Evening Ledger.
- **Integration notes:** Re-skin the default dark theme to **warm parchment + ember orange**. Heat ships with 5 color presets; we author preset #6 "Hearthbound." Replace its system font with a warmer humanist serif (Crimson Pro or similar from TextMeshPro). One TextMeshPro asset + one color preset = the entire reskin.

### S-4. Bamao Pack: Fantasy GUI
- **Order:** Must-Have Bundle 2026 (Tier 2) · Retail $25 · Paid (bundled)
- **Use in HH:** The **diegetic / in-world UI** — dialogue boxes (parchment scrolls), Evening Ledger (open book layout), memory tooltip frames, shelf labels. Heat = system UI; Bamao = shop UI. They layer.
- **Mission 1 specifics:** Doris's dialogue uses a Bamao parchment box at the bottom of the screen with her portrait on the left and 3 choice scrolls. The Evening Ledger uses Bamao's open-book panel as the background.
- **Integration notes:** Hand-painted 300+ PNG sprites — we'll need to atlas them for mobile (Sprite Atlas, max 2048×2048 per atlas). Drop dragon motifs not aligned with the game's autumn theme.

### S-5. All In 1 Shader Nodes
- **Order:** Must-Have Bundle 2026 (Tier 2) · Retail $45 · Paid (bundled)
- **Use in HH:** **The memory orb shader.** The single most important shader pack we own.
- **Per-feature mapping:**
  - **Glass refraction node** → orb body refraction
  - **Fresnel / rim light** → soft glow around orb silhouette
  - **Dissolve (noise + directional)** → cracks (Pillar A "cracks = trauma") and Cleanse animation
  - **Scanline** → "veiled" / unfaded memory state
  - **Outline (world-space)** → selected orb indicator
  - **Hologram** → ghostly "preview the memory" hover state
- **Integration notes:** Author one master Shader Graph called `MemoryOrb_Master` that exposes ~12 properties (color tint, clarity 0–1, crack intensity 0–1, dissolve progress, glow strength, weight scale). Every orb prefab uses material instances of this master.

### S-6. Lumen: Stylized Light FX 2
- **Order:** Must-Have Bundle 2026 (Tier 2) · Retail $35 · Paid (bundled)
- **Use in HH:** The cozy atmosphere. Stylized god rays through the shop windows, candle glow on the shelves, lantern halos in the lane, point-light glow bloom around held orbs. World-space mesh (not post-process) = mobile-friendly.
- **Mission 1 specifics:** 4 god ray meshes angled through the shop's main window onto the workbench; candle glow prefabs on the shelves (8 instances); lantern halo at the shop entrance.
- **Mission 2 specifics:** Lantern halos along the lane to the widower's cottage at evening.
- **Integration notes:** Runtime intensity controller is time-of-day aware — wire it into the `DayCycleManager` we'll build.

### S-7. Cutscene Engine
- **Order:** Must-Have Bundle 2026 (Tier 2) · Retail $35 · Paid (bundled)
- **Use in HH:** Opening cutscene, Memory Dream sequences, mission intros & outros. Timeline-compatible, camera cut tracks, dialogue injection, skip/replay, letterbox bars.
- **Mission 1 specifics:**
  - **Cold open:** 30-sec cinematic camera flythrough of the autumn lane arriving at the Hollow door
  - **Memory Dream 1:** 60-sec illustrated vignette derived from Doris's first restored memory (we'll author the dream's camera + animated 2D portrait reveal as a Timeline asset)
  - **Mission outro:** evening fade with predecessor-mystery tease text reveal
- **Integration notes:** Pairs with Unity Timeline (already a Unity 6 module). The dialogue injection track is wired to Yarn Spinner (see §6 Critic note).

### S-8. Game UI & Puzzle Sound Effects Pack
- **Order:** Must-Have Bundle 2026 (Tier 2) · Retail $99 · Paid (bundled)
- **Use in HH:** All UI + mini-game audio. 500+ studio-quality sounds — clicks, hovers, toggles, popups, tile-placement (orb on shelf), match (Polish circular gesture success), chain reaction (Weave later), success jingles, failure stings, countdown ticks.
- **Mission 1 specifics:** Polish mini-game alone uses ~8 sounds (gentle rub start, mid-rub texture loop, sparkle-clarity reveal, success chime, miss-stroke soft buzz). Shelf placement uses 2 (jar-down on wood, orb-tap-glass). UI gets full Heat audio binding.
- **Integration notes:** Author an `SfxLibrary.asset` ScriptableObject that catalogues every sound by event ID. No hard-coded paths.

### S-9. Hierarchy Designer
- **Order:** $2+ Sale (16) · Retail $30 · Paid $2.10
- **Use in HH:** Color-coded folders, icons, separators for our scene hierarchies. Trivial install, huge productivity win for every developer who opens the project.
- **Integration notes:** Author a `Hearthbound.hierarchypreset` with these conventions:
  - 🟧 Orange = `_Managers` (singletons / scene services)
  - 🟫 Brown = `Environment` (village, shop, garden)
  - 🟪 Purple = `Memories` (orbs, dreams, ledger nodes)
  - 🟦 Blue = `UI`
  - 🟩 Green = `NPC` characters
  - 🟨 Yellow = `Lighting & VFX`
  - ⬜ Gray = `Cameras & Cutscenes`

### S-10. Asset Inventory 4
- **Order:** Must-Have Bundle 2026 (Tier 2) · Retail $59 · Paid (bundled)
- **Use in HH:** Manage the other 65 imports. Searchable database, tag system, dependency tracker. **Install first, before any other asset.**
- **Integration notes:** Tag every asset we import with `Mission1`, `Mission2`, `Mission3+`, `Reference`, or `DoNotImport`. This becomes our asset-budget control surface.

---

## 6. TIER A — Import for Mission 1–2 (supporting / polish)

### A-11. City Characters – Modular Animated Pack
- **Order:** Must-Have Bundle 2026 (Tier 2) · Retail $259 · Paid (bundled) — *most expensive asset in the bundle*
- **Use in HH:** Modular villager NPCs. 50+ body modules, 100+ outfits, 20+ head variants, 80+ animations (locomotion, idle, sit, interaction). Shared skeleton.
- **Decision required (Creative Director):** This is one of **two** competing villager-art directions (the other is BoZo, A-12). The Critic Board recommends **BoZo (chibi)**; if Creative Director prefers semi-realistic civilians, use City Characters. **Do not import both.**

### A-12. BoZo: Stylized Modular Characters – Fantasy Pack
- **Order:** $2+ Sale (22) · Retail $40 · Paid $2.40
- **Use in HH:** Chibi stylized villagers. 6 base archetypes (Warrior/Mage/Archer/Rogue/Cleric/Bard) reskinned as Baker/Widower/Mayor/Farmer/Innkeeper/Child. Humanoid Mecanim rig.
- **Critic Board recommendation:** *Use this one.* It signals cozy/Spiritfarer faster than realistic civilians, matches the GDD's "warm even in heavy moments" pillar, and is roughly 1/3 the mobile texture cost of City Characters.
- **Mission 1 specifics:** Doris = re-skinned Cleric archetype (apron, kerchief). Mission 2 widower = re-skinned Bard archetype (long coat, hat, downcast head idle).

### A-13. Character Controller Pro (Lightbug)
- **Order:** $2+ Sale (16) · Retail $28.99 · Paid $2.03
- **Use in HH:** The player controller. Third-person capsule. **Disable** Jump, Dash, Jetpack, WallJump, Glide, Ladder, Rope, Swimming, Ghost — we only need Normal state (walking). Full source code, mobile-friendly.
- **Integration notes:** Requires Unity new **Input System** (we'll install). We author a `Player` prefab with: CharacterActor + Normal state only + simple gamepad/keyboard/touch input binding + Cinemachine third-person follow camera.

### A-14. Eyes Animator
- **Order:** $2+ Sale (16) · Retail $11.99 · Paid $2.04
- **Use in HH:** Lifelike NPC eyes during dialogue. Saccades, blinks, eyelid tracking, look-at, pupil dilation. **Mandatory for every speaking NPC** — this is a Spiritfarer/Pentiment-tier emotional resonance win for a fraction of the hand-keying cost.
- **Mission 1 specifics:** Doris's eyes glance at the orb she's offering, then back to the player. The micro-saccades alone elevate the dialogue scene 50%.
- **Integration notes:** Drop the Eyes Animator component on every villager prefab's head rig. Configure per character: blink rate, look-target priority list (player > orb-in-hand > shop-shelf > random).

### A-15. Animation Composer System (ACS)
- **Order:** $2+ Sale (22) · Retail $39.99 · Paid $2.40
- **Use in HH:** Villager idle composition — layer upper-body actions (drinking tea, polishing a counter, reading a letter) over base idle.
- **Mission 1 specifics:** Background villager in the lane sits on a bench reading a letter (idle base + ACS upper-body "reading" layer). Doris's idle has a soft kerchief-adjustment loop.
- **Integration notes:** Use the Event System hook to trigger VFX/SFX at animation frames (e.g., when Doris extends the orb to the player, an audio chime fires on frame 14).

### A-16. Stylized Weather System
- **Order:** Must-Have Bundle 2026 (Tier 2) · Retail $20 · Paid (bundled)
- **Use in HH:** Lightweight stylized autumn weather — soft rain, light fog, gentle wind, optional snow flurries for late-game seasons.
- **Mission 1 specifics:** Clear evening light with mild fog roll. **Mission 2 specifics:** Optional soft rain when player exits the Hollow to walk to widower's cottage.
- **Why this over UniStorm/Weather Maker:** Both UniStorm and Weather Maker are excellent *realistic* weather systems — wrong for our stylized cozy tone, and 4–6× heavier on mobile. Stylized Weather System is the correct choice.

### A-17. Zephyr: Dynamic Wind System
- **Order:** Must-Have Bundle 2026 (Tier 2) · Retail $40 · Paid (bundled)
- **Use in HH:** Vertex-animated foliage, banners, laundry lines, shop sign. Mobile-friendly shader-based.
- **Mission 1 specifics:** Autumn leaves on every tree in the lane, the Hollow's hanging shop sign swaying, laundry lines between cottages.
- **Integration notes:** Pair with the Stylized Weather System's wind zone — when weather state changes, wind intensity transitions smoothly.

### A-18. VoluSmokeFX
- **Order:** Must-Have Bundle 2026 (Tier 2) · Retail $25 · Paid (bundled)
- **Use in HH:** Memory tea steam, chimney smoke, dream-transition wisps. VFX Graph-based, GPU-particle.
- **Mission 1 specifics:** Chimney smoke on the Hollow exterior; a single steam wisp from the kettle on the workbench.
- **Mission 2 specifics:** Steam from the brewed tea cup. **Memory Dream transition:** a wisp expands and dissolves into the Dream cutscene.
- **Integration notes:** Requires Visual Effect Graph package (we'll install).

### A-19. Skill Tree / Talent Tree Builder
- **Order:** Must-Have Bundle 2026 (Tier 1) · Retail $15 · Paid (bundled)
- **Use in HH:** **The Memory Map visualizer** — surprise hero of the bundle. The GDD Pillar C describes each villager's 12–20 memories as a node-graph with cross-references. This asset's node editor + UI prefabs + ScriptableObject layer is **almost exactly the data model we need**. Rename "Skill" → "Memory", "Prerequisite" → "Connection", "Point Cost" → "Cleanse Cost".
- **Mission 1 specifics:** Doris has a 4-node mini Memory Map (3 nodes visible, 1 locked). Player clicks an unlocked node to view the memory.
- **Mission 2 specifics:** The widower's first 2 memory nodes connect to one of Doris's (cross-reference revealed) — this triggers the first Revelation chapter hook.
- **Integration notes:** Author `MemoryNodeSO`, `MemoryConnectionSO`, `VillagerMemoryMapSO` ScriptableObjects layered over the Skill Tree's data classes.

### A-20. Dialogue System for Unity Addon for OpenAI
- **Order:** $2+ Sale (16) · Retail $45 · Paid $2.70
- **⚠️ Critic flag:** This is **the OpenAI addon, not the base Dialogue System.** It requires:
  1. **Dialogue System for Unity** (Pixel Crushers, sold separately, ~$80)
  2. **OpenAI API key** (paid usage)
- **Senior Unity Dev recommendation:** **Do NOT use the OpenAI addon for Mission 1–2.** The GDD's #1 Production Pillar is "Every villager is fully written — no filler dialog. Every line could be in a novel." AI-generated dialogue actively *contradicts* this pillar.
- **Approved alternative:** Use **Yarn Spinner** (free, https://yarnspinner.dev/) for hand-authored branching dialogue. The Senior Unity Dev's architecture spec explicitly mentions Yarn Spinner.
- **Disposition:** Keep the OpenAI addon as a *reference/exploration* asset only; tag in Asset Inventory 4 as `Reference – Do Not Use In Build`. Revisit only for the late-game "Caravan Visitors" DLC where procedural traveler dialogue could be a feature.

### A-21. Colorize (Texture Color Palette Modifier)
- **Order:** $2+ Sale (22) · Retail $59.90 · Paid $3.59
- **Use in HH:** Villager hair/clothes color variants without duplicating textures; autumn palette shift on the entire village (green-summer → orange-amber autumn at material level).
- **Mission 1 specifics:** Generate 6 villager color variants from 1 base BoZo character mesh. Mass-shift village foliage hue.

### A-22. LightMap Fusion Pro
- **Order:** Must-Have Bundle 2026 (Tier 2) · Retail $50 · Paid (bundled)
- **Use in HH:** Day/night lightmap sets with runtime blending. Mobile-friendly.
- **Mission 1 specifics:** Bake `LM_Morning`, `LM_Afternoon`, `LM_Evening`, `LM_Night` lightmap sets for the village and Hollow interior. Mission 1 transitions Afternoon → Evening during play.

### A-23. Microdetail Terrain System
- **Order:** Must-Have Bundle 2026 (Tier 2) · Retail $50 · Paid (bundled)
- **Use in HH:** Close-range detail on the village paths (cobblestone, dirt, moss) without polygon cost.
- **Integration notes:** Subtle but elevates the "I'm really here" feeling on the village walk.

---

## 7. TIER B — Hold for Mission 3+ / expansions

| Asset | Why hold |
|---|---|
| **TerraSplines** + **Spline Mesher Pro** | Procedural paths/rivers/fences — overkill for Mission 1–2's small village. Pull in when we build the broader countryside (Mission 4+). |
| **The Medieval Castle** + **Fantasy Castle Environment** | Possible late-game "old keep" or "memory-keeper's former home" location. Hold. |
| **UniStorm** + **Weather Maker** | Realistic weather — wrong tonally and 4–6× the mobile cost. Hold as fallback. |
| **Stylized Dungeons** | Possible Memory Dream "descent into the mind" metaphor for a late game heavy memory. Hold. |
| **UniPay IAP** | Mobile launch (per GDD §6). Hold until Year 1 Q3 mobile port. |
| **Magic Arsenal** | Cherry-pick 1–2 props as decorative shop equipment (e.g., a ceremonial orb-cradle). The staffs/swords vibe is wrong otherwise. |
| **Toon Town** | Cherry-pick mailbox / lamppost / sign props if Medieval Village lacks them. Mostly wrong aesthetic. |
| **Horror Bundle SFX** | Cherry-pick 5–10 ambient creaks / door rattles for the *abandoned-shop opening* moment in Mission 1. Do not import the full pack. |

---

## 8. TIER C — Do NOT import for Hearthbound Hollow

These assets actively harm the project's coherence, mobile budget, and reviewer mental-model. Tag in Asset Inventory 4 as `WrongProject – DoNotImport`.

| Category | Assets |
|---|---|
| **Combat VFX** | Stylized VFX Bundle, Spells Pack, 100 Special Skills Effects Pack, Casual RPG VFX, Anime Powers Pack, Ultimate Mesh FX, Screenspace VFX, UNI VFX Missiles & Explosions |
| **Gore** | Realistic Blood VFX, Volumetric Blood Fluids |
| **Monsters / Creatures** | Fantasy Monsters Bundle, Stylized Fantasy Creatures Bundle #2, Animated 2D Characters Monsters |
| **Weapons** | Magic Arsenal (mostly), POLYGON Battle Royale Pack |
| **Vehicles / Racing** | Edy's Vehicle Physics, Modular Cyber Racing Cars, Complete Racing Game 2 |
| **Multiplayer / FPS** | MFPS 2.0, MMFPSE, Horror Multiplayer Template |
| **Sci-Fi** | Sci-Fi Low Poly Interior Bundle, Sci-Fi Space Stations Creator, Neon Interior Props |
| **Urban Modern / Industrial** | City Pack, Office Floors, Toon Town (urban use), Industrial Props Mega Bundle, Urban Abandoned District |
| **Action / Platforming** | Obby Parkour Mega Pack, Traversal Pro |
| **Mining / Survival** | Shatter Stone Bundle |
| **Dependencies we don't carry** | Perks for Game Creator 2 (requires Game Creator 2) |

---

## 9. Unity Package Prerequisites (install BEFORE importing assets)

The Senior Unity Developer flags that the current `Packages/manifest.json` is **missing critical packages**. Install these via Package Manager *first*, in this order:

| # | Package | Why |
|---|---|---|
| 1 | **Universal RP** (`com.unity.render-pipelines.universal`) | Project must run on URP — almost every Tier S/A asset is URP-optimized; current project is on Built-in |
| 2 | **Input System** (`com.unity.inputsystem`) | Required by Character Controller Pro; modern multi-platform input |
| 3 | **Cinemachine** (`com.unity.cinemachine`) | Third-person follow camera + cutscene camera blends |
| 4 | **TextMeshPro** (`com.unity.textmeshpro`) | Required by Heat UI + every dialogue label |
| 5 | **Addressables** (`com.unity.addressables`) | Per-mission scene streaming (Architecture spec) |
| 6 | **Visual Effect Graph** (`com.unity.visualeffectgraph`) | Required by VoluSmokeFX |
| 7 | **Timeline** (`com.unity.timeline`) | Required by Cutscene Engine |
| 8 | **Shader Graph** (`com.unity.shadergraph`) | Required by All In 1 Shader Nodes (included in URP) |
| 9 | **Splines** (`com.unity.splines`) | Future-proofing for Mission 3+ paths (already in Unity 6 default) |
| 10 | **2D Sprite + Animation** (`com.unity.2d.sprite`, `com.unity.2d.animation`) | For Bamao UI sprite atlas + Memory Dream illustrated portraits |

Render-pipeline switch: after installing URP, create a `URP-Mobile.asset` Render Pipeline Asset configured for **mobile** (one real-time light, soft shadows off by default, MSAA 2×, bloom on, post-processing on, render scale 0.85).

---

## 10. Project Folder Hierarchy (post-import)

The Unity Asset Engineer will enforce this layout after every import:

```
/Assets/
├── _Project/                  # Our authored content
│   ├── Art/
│   │   ├── Characters/        # BoZo-derived villager prefabs
│   │   ├── Environment/       # Village + Hollow interior layouts
│   │   ├── Memories/          # Orb materials, shader graph
│   │   └── UI/                # Heat reskin + Bamao atlas
│   ├── Audio/
│   │   ├── Music/
│   │   ├── SFX/
│   │   └── Ambience/
│   ├── Animations/
│   ├── Prefabs/
│   │   ├── Player/
│   │   ├── NPCs/
│   │   ├── Memories/
│   │   ├── Props/
│   │   └── VFX/
│   ├── Scenes/
│   │   ├── Bootstrap.unity
│   │   ├── MainMenu.unity
│   │   ├── Mission01_OpeningTheHollow.unity
│   │   └── Mission02_TheWidowersRequest.unity
│   ├── Scripts/
│   │   ├── Core/              # GameManager, ServiceLocator, EventBus
│   │   ├── Player/
│   │   ├── Memory/            # Orb data, MemoryMap, polish/cleanse minigames
│   │   ├── Dialogue/          # Yarn Spinner integration
│   │   ├── Mission/
│   │   └── UI/
│   ├── ScriptableObjects/
│   │   ├── Memories/
│   │   ├── Villagers/
│   │   ├── Herbs/
│   │   └── Missions/
│   └── Yarn/                  # Yarn Spinner dialogue scripts
│
├── ThirdParty/                # Imported Asset Store packages
│   ├── MedievalVillageMegapack/
│   ├── HarvestGarden/
│   ├── HeatUI/
│   ├── BamaoFantasyGUI/
│   ├── AllIn1ShaderNodes/
│   ├── LumenLightFX2/
│   ├── CutsceneEngine/
│   ├── GameUIPuzzleSFX/
│   ├── HierarchyDesigner/
│   ├── AssetInventory4/
│   ├── BoZoCharacters/
│   ├── CharacterControllerPro/
│   ├── EyesAnimator/
│   ├── AnimationComposerACS/
│   ├── StylizedWeatherSystem/
│   ├── Zephyr/
│   ├── VoluSmokeFX/
│   ├── SkillTreeBuilder/      # repurposed as Memory Map
│   ├── Colorize/
│   ├── LightMapFusionPro/
│   └── MicrodetailTerrainSystem/
│
└── Plugins/
    └── YarnSpinner/            # Free, from Package Manager git URL
```

---

## 11. Mobile Performance Budget Plan

| Constraint | Target | Mitigation per asset |
|---|---|---|
| Build size | < 1.5 GB (Switch + iOS App Store under-cellular bucket) | Addressables remote-loaded village/dream content; aggressive ASTC compression on import |
| Texture memory | < 200 MB resident | Atlas Bamao UI, decimate Medieval Village 4K → 2K, City Characters: skip if BoZo chosen |
| Real-time lights | 1 directional + 4 point (interior) | LightMap Fusion Pro handles the rest baked |
| Draw calls (mid-range Android) | < 200 per frame | GPU instancing on village walls, static batching, Lumen mesh-light pooling |
| Frame budget | 16.6 ms (60 fps) | Profile after first scene assemble; Senior Unity Dev to add Mobile Profile Switch |

---

## 12. Critic & Review Board Verdict

⚠️ **Approved with Notes** — proceed with imports subject to:

1. ✋ **Creative Director must pick villager art direction (BoZo vs City Characters) before any character import.** Board recommendation: **BoZo (chibi)**. *Awaiting confirmation.*
2. ✋ **Do not use the OpenAI Dialogue addon for v1.0.** Use Yarn Spinner instead (free, aligns with the hand-authored writing pillar).
3. ✋ **Senior Unity Dev must complete the Unity package prerequisite list (§9) and the URP-Mobile pipeline asset before any of the 23 Tier S/A imports.**
4. ✋ **Asset Inventory 4 must be installed first** so every subsequent import is auto-cataloged and tagged.
5. ✋ **No Tier C assets imported.** Even though they're in the bundle, do not import them. They will be revisited for other studio projects.

When (1) is decided, the Senior Unity Dev produces the **Mission 1 Import & Scene Assembly Plan** — a step-by-step build guide for the user to execute alongside the team's C# scripts.

---

## 13. Action List for the User (in order)

1. ✅ Confirm chibi (BoZo) vs civilian (City Characters) art direction
2. ⬜ Open Unity 6 (6000.4.4f1) and load the `hearthbound-hollow` project
3. ⬜ Install the 10 Unity packages from §9 via Package Manager
4. ⬜ Switch render pipeline to URP and assign the Mobile RP asset (Senior Unity Dev will commit the asset file)
5. ⬜ Import **Asset Inventory 4** *first*
6. ⬜ Import the remaining Tier S assets (S-1 through S-10) — in any order
7. ⬜ Import the Tier A assets (A-11 through A-23) — in any order *except* A-20 (OpenAI addon, skip)
8. ⬜ Install **Yarn Spinner** from Unity Package Manager (git URL: `https://github.com/YarnSpinnerTool/YarnSpinner-Unity.git`)
9. ⬜ Wait for the team's first PR: `Scripts/Core/` + `ScriptableObjects/Memories/` + `Mission01_OpeningTheHollow` scene skeleton

---

*Document version 1.0 — drafted by Creative Director, Game & Level Designer, Unity Asset Engineer team, Senior Unity Developer; sealed by Critic & Review Board.*
