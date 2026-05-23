# 🏗️ Scene Assembly Guide — Mission 1 + Mission 2

> **For the user** — what to drag into each of the 6 scenes after pulling the architecture branch.
> Each step is **minimal** to keep Unity reimports painless. No external assets need importing for this guide; everything references things already in `Assets/_Project/` or the existing vendor folders.

---

## Prerequisites

After pulling `feat/mission-1-2-architecture`:

1. **Unity will install new packages** from `manifest.json` (~30–90 s). Wait for "Importing" to settle.
2. **(Optional) Install Yarn Spinner now or later:** *Window → Package Manager → +  → Add package from git URL →* `https://github.com/YarnSpinnerTool/YarnSpinner-Unity.git` — without it, dialogue logs to Console instead of rendering. The `YarnVillageStateBridge.cs` is guarded so the project compiles either way.
3. **Switch render pipeline to URP** (one-time):
   - *Edit → Project Settings → Graphics → Scriptable Render Pipeline Asset*
   - Use the URP Mobile asset (create one if not present: *Project → Create → Rendering → URP Asset (with Universal Renderer)* and name it `URP-Mobile.asset` under `Assets/_Project/Settings/`).
4. **Verify TextMeshPro essentials**:
   - Open any UI file in Inspector — Unity will prompt "Import TMP Essentials." Accept.

---

## Scene 1 — `00_Bootstrap.unity`

**Purpose:** Service init + auto-load MainMenu.

Build steps:
1. *File → New Scene → Empty*. Save as `Assets/_Project/Scenes/00_Bootstrap.unity`.
2. Create empty GameObject `_GameRoot`.
3. Add component → **GameManager** (HearthboundHollow.Core.GameManager).
4. In Project window: *Create → Hearthbound → State → Village State* → save as `Assets/_Project/ScriptableObjects/State/VillageState.asset`.
5. Drag that asset into the GameManager's `Village State` field.
6. Set `Main Menu Scene Name = "01_MainMenu"`, `Bootstrap Scene Name = "00_Bootstrap"`.
7. *File → Build Profiles → Scenes In Build* → add this scene as index 0.

---

## Scene 2 — `01_MainMenu.unity`

**Purpose:** "Open The Hollow" CTA + Tone Compass + Continue.

Build steps:
1. Save new scene as `01_MainMenu.unity`.
2. Add a `Canvas` (UI → Canvas, render mode Screen Space - Overlay).
3. From `Assets/Heat - Complete Modern UI/Prefabs/Panels/` drag the **Main Menu** prefab (whichever Heat panel matches "Main Menu" — Heat ships with one called `Panel Manager` containing the menus).
4. Add a fresh GameObject `_MainMenu` with **MainMenuController** component.
5. Wire its `Open The Hollow Button`, `Continue Button`, `Settings Button`, `Credits Button`, `Quit Button` to the Heat panel's buttons.
6. Add a child GameObject `ToneCompassRoot` with the **ToneCompassCard** component. Build a small Bamao parchment panel (parent Image with `Bamao` sprite) + `Body Text` TMP + `Continue Button` button + a `Gentle Mode Toggle`.
7. *Add to Scenes In Build* index 1.

---

## Scene 3 — `02_Mission01_Lane.unity`

**Purpose:** The autumn village lane at dusk. Player walks to the Hollow door.

Build steps:
1. Save new scene.
2. *GameObject → Camera*. Add **Cinemachine** → CinemachineCamera. Set Follow + LookAt to a placeholder Player Transform you'll create next.
3. Drag from `Assets/MeshingunStudio/Medieval Village/Scenes/MedievalTownMain/` the demo scene's environment chunk (or open it additively and merge a slice).
4. Place 6-8 cottage exteriors along a lane axis.
5. Drag **BoZo Cleric** prefab from `Assets/BoZo_StylizedModularCharacters/Prefabs/` and rename to `Player`. Add `CharacterController` (radius 0.4, height 1.8). Add the **PlayerController** component. Drag the Cinemachine Camera's Follow + LookAt to this Transform.
6. Place a separate `Doris` GameObject (also a BoZo Cleric prefab, reskinned with an apron — material override later). At her feet add a `HollowDoorInteractable` proxy collider (BoxCollider, IsTrigger=true) on a child marked **HollowDoor**.
7. Add `DayCycleManager` to a `_Lighting` GameObject. Set Sun to your scene's directional light. Set `dayProgress01 = 0.6` (Afternoon).
8. Add `LumenLightController` to the same `_Lighting` object. Wire it to a Lumen God-Ray mesh placed at the shop window (from `Packages/com.distantlands.lumen/Content/`).
9. *Add to Scenes In Build* index 2.

---

## Scene 4 — `03_Mission01_Hollow.unity`

**Purpose:** Hollow interior. Workbench + Polish mini-game + first save.

Build steps:
1. Save new scene.
2. Place a small interior using **Medieval Village** house interior parts (use the demo "Inn" interior as a starting point and strip irrelevant props).
3. Place `Workbench` prop. On the workbench add a child `OrbCradle` empty.
4. Drag your `MemoryOrb_Master.prefab` (Phase 11 — we'll author the shader graph below) and place it on `OrbCradle`.
5. Add `MemoryOrbInteractable` component on the orb GameObject. Drag the DOR-001 MemoryNodeSO asset (you'll need to create this in Project — *Create → Hearthbound → Memory → Memory Node*, set id="DOR-001", title="First Loaves", primary tone Joy, weight 0.4, initial clarity 0.4).
6. Add an empty `_Player` and copy the Player from the lane scene (same Cinemachine setup).
7. UI canvas: 2 root children — `DialogueUI` (parchment box + portrait + line text), `EveningLedgerUI` (book-style summary panel). Both prefab roots disabled at start; the scripts toggle them.
8. Add `MissionRunner` to a root `_Mission` object. Reference `mission` (create *Create → Hearthbound → Mission → Mission* MissionSO with `missionId="Mission01"`).
9. Add `PolishMiniGame` to a child of `_Mission`. Wire `Target Orb` to the workbench orb.
10. Add `PickleAI` to a `Pickle` GameObject (use any small cat mesh you have or a placeholder sphere). Drag the player and orb to look-at targets.
11. Add `HUDController` to a HUD canvas group. Wire labels.
12. *Add to Scenes In Build* index 3.

---

## Scene 5 — `04_Mission02_Garden.unity`

**Purpose:** Herb garden + tea brewing.

Build steps:
1. Save new scene.
2. Place 4-6 **Harvest Garden** plant prefabs from `Assets/Waldemarst/HarvestGarden/Prefabs/Plants/`. Two of them get `HerbHarvestInteractable` components with the Lavender + Valerian `MemoryHerb.asset`s wired.
3. Place a `Kettle` prop with the **KettleInteractable** component.
4. UI canvas: add `TeaBrewingUI` (herb pick buttons + brew progress slider + auto-complete button).
5. *Add to Scenes In Build* index 4.

---

## Scene 6 — `05_Mission02_Cottage.unity`

**Purpose:** Widower's cottage interior + Cleanse + moral choice + Dream 2.

Build steps:
1. Save new scene.
2. Place a small cottage interior using Medieval Village interior parts (one room, bed, table, chair).
3. Drag a `Gerrold` BoZo Bard prefab (long coat + hat reskin).
4. Place the `GER-007` orb (MemoryNodeSO with crackIntensity=0.6, primaryTone=Grief).
5. Add `CleanseMiniGame` to a `_Mission` root with target orb wired.
6. Add `ChoiceCardUI` to UI canvas. Wire `choiceTilePrefab` to a Bamao tile prefab you author with named children Label/CostPreview/Icon.
7. Add `MemoryDreamSequencer` to a `_Cutscene` root. Add a `PlayableDirector` on the same object and wire to the sequencer.
8. *Add to Scenes In Build* index 5.

---

## Final wiring

Once the 6 scenes exist:

1. In Bootstrap scene, GameManager → autoLoadMainMenu = true.
2. *Edit → Project Settings → Player → Other Settings →* set `Color Space = Linear`, `Active Input Handling = Both` (so legacy + new Input System both work).
3. *Build Profiles → make the bootstrap scene index 0.*
4. *File → Build And Run* to verify Editor + Player both reach MainMenu cleanly.

---

## Authoring the `MemoryOrb_Master` shader (one-time)

In Phase 11 / Unity-side authoring:

1. Right-click in Project → *Create → Shader Graph → URP → Lit Shader Graph*. Name `MemoryOrb_Master.shadergraph` under `Assets/_Project/Art/Memories/`.
2. Add 5 exposed properties: `_Clarity (float 0-1)`, `_CrackIntensity (float 0-1)`, `_DissolveProgress (float 0-1)`, `_GlowStrength (float)`, `_PaletteTint (color)`.
3. Open *All In 1 Shader Nodes* and drag in:
   - `Dissolve (noise)` node, plug `_CrackIntensity` into its Threshold input. Output alpha → AlphaClipThreshold.
   - `Fresnel` node, multiply with `_GlowStrength`, add to Emission.
   - Lerp(0, `_PaletteTint`, `_Clarity`) → BaseColor.
4. Save. Create a Material instance, set as the orb's default material. **GPU instancing ON.**

The `MemoryOrbInteractable` script's `SetClarity()` / `SetCrackIntensity()` already pipe into these property names via MaterialPropertyBlock.

---

## What this guide intentionally does NOT cover (deferred)

- VoluSmokeFX / Skill Tree Builder / Colorize / LightMap Fusion Pro / Microdetail Terrain — these 5 Tier-A assets are not yet imported; the M1-2 scenes use built-in substitutes where they would have been used (basic particles for steam, custom MemoryMapUI for the tree, single bake for lighting). When you import them later, the existing prefabs accept their components as drop-ins.
- Composer cues + VO recordings — Foley + placeholder VO are OK for the build; final audio is post-Mission 2.
- Bake lightmaps — once scenes are populated, *Window → Rendering → Lighting → Bake.*

---

*This guide is the contract between scripts-on-GitHub and scenes-in-Unity. Update it any time the C# changes require scene-level revisions.*
