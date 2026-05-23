# 📦 Existing Imported Assets — Quick Index

> Snapshot of what's already in `/Assets/` as of branch `feat/mission-1-2-architecture` init.

| Tier-S/A # | Asset | Folder | Status |
|---|---|---|---|
| S-1 | Medieval Village Megapack | `Assets/MeshingunStudio/Medieval Village/` | ✅ Imported |
| S-2 | Harvest Garden | `Assets/Waldemarst/HarvestGarden/` | ✅ Imported |
| S-3 | Heat – Complete Modern UI | `Assets/Heat - Complete Modern UI/` | ✅ Imported |
| S-4 | Bamao Fantasy GUI | `Assets/Bamao/BamaoUIPack/` | ✅ Imported |
| S-5 | All In 1 Shader Nodes | `Assets/Plugins/AllIn1ShaderNodes/` | ✅ Imported |
| S-6 | Lumen Stylized Light FX 2 | `Packages/com.distantlands.lumen/` | ✅ Imported (UPM) |
| S-7 | Cutscene Engine | `Assets/Cutscene Engine/` | ✅ Imported |
| S-8 | Game UI & Puzzle SFX | `Assets/Game UI & Puzzle Sound Effects Pack/` | ✅ Imported |
| S-9 | Hierarchy Designer | `Assets/Hierarchy Designer/` | ✅ Imported |
| S-10 | Asset Inventory 4 | `Assets/AssetInventory/` | ✅ Imported |
| A-11 | City Characters | (not imported — BoZo chosen) | ⚫ Skipped (D-001) |
| A-12 | BoZo Stylized Modular Characters | `Assets/BoZo_StylizedModularCharacters/` | ✅ Imported |
| A-13 | Character Controller Pro | `Assets/Character Controller Pro/` | ✅ Imported |
| A-14 | Eyes Animator (FImpossible) | `Assets/FImpossible Creations/Plugins - Animating/` | ✅ Imported |
| A-15 | Animation Composer (ACS) | `Assets/Jorjouto/ACS/` | ✅ Imported |
| A-16 | Stylized Weather System | `Assets/Unluck Software/Stylized Weather/` | ✅ Imported |
| A-17 | Zephyr Wind System | `Packages/com.distantlands.zephyr/` | ✅ Imported (UPM) |
| A-18 | VoluSmokeFX | — | ⚠️ Pending |
| A-19 | Skill Tree Builder | — | ⚠️ Pending |
| A-20 | OpenAI Dialogue Addon | — | 🚫 Do not import (D-002) |
| A-21 | Colorize | — | ⚠️ Pending |
| A-22 | LightMap Fusion Pro | — | ⚠️ Pending |
| A-23 | Microdetail Terrain System | — | ⚠️ Pending |

## Resolution Plan for Pending Imports

| Asset | M1-2 Workaround | When to import properly |
|---|---|---|
| VoluSmokeFX | Built-in particles for kettle steam + chimney + dream transition | Anytime — drop in and replace prefab refs |
| Skill Tree Builder | `MemoryMapUI.cs` renders 4 BoZo-styled nodes with Bamao parchment background as a custom prefab | When extending past 4 nodes (Mission 3+) |
| Colorize | One BoZo character variant authored by hand for Mission 1-2 (Doris) | Mission 3+ for hair/clothes variants on procedural NPCs |
| LightMap Fusion Pro | Single bake per scene; DayCycleManager interpolates exposure + ambient | When evening↔morning blending becomes noticeable |
| Microdetail Terrain System | Plain terrain texture; village paths use cobblestone albedo | Cosmetic; later mission |

**Net: Mission 1 + Mission 2 are buildable with current imports. No engineering blockers.**

## Package Manager (UPM) packages

### Already present (pre-Phase 0)
```
com.distantlands.lumen
com.distantlands.zephyr
com.unity.editorcoroutines, inputsystem, multiplayer.center,
nuget.newtonsoft-json, postprocessing, shadergraph, sharp-zip-lib, ugui
```

### Added by Phase 0 manifest patch
```
com.unity.render-pipelines.universal   17.2.0
com.unity.cinemachine                  3.1.4
com.unity.textmeshpro                  3.0.9
com.unity.addressables                 2.6.2
com.unity.visualeffectgraph            17.2.0
com.unity.timeline                     1.8.7
com.unity.splines                      2.7.2
com.unity.2d.sprite                    1.0.0
com.unity.2d.animation                 11.0.4
com.unity.test-framework               1.5.1
```

These download silently on first open after pull. No play-mode disruption because no code references them yet.

---
*Updated alongside `PROGRESS.md`.*
