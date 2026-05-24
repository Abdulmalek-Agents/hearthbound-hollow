# 📊 Hearthbound Hollow — Implementation Progress Log

> Continuously updated as the studio builds Mission 1-2. Every PR appends to this file.
> **Unity Editor version: 6000.4.4f1 (Unity 6 LTS)**

---

## Legend
- ✅ Done & merged · 🟢 Done in branch (awaiting your pull) · 🟡 In progress · ⬜ Not started · 🔴 Blocked

---

## 🎯 Current Status — POLISHED PLAYABLE MISSION 1 + 2 LANDED

**Branch**: `feat/mission-1-2-architecture` (PR #7 open)

The architectural Phase 0–22 is complete and tested. **Phase 23 + 24** land in this update:

| Phase | Title | Status | Asset(s) | Replaces |
|---|---|---|---|---|
| ✅ 0–10.8 | Architecture, scripts, bug-fix cycles, URP, shader patcher | ✅ Done | (foundation) | — |
| ✅ 11 | Seed asset generator + Input Actions + Save-event bugfixes | ✅ Done | — | — |
| ✅ 12 | Make It Playable MVP (smoke-test) | ✅ Done | primitives + URP/Lit | (the baseline) |
| ✅ 13 | BoZo Character Prefabs | ✅ Done | `BoZo_StylizedModularCharacters/` | capsule + cylinder |
| ✅ 14 | Bamao Parchment UI Skin | ✅ Done | `Bamao/BamaoUIPack/` | flat-color UI |
| ✅ 15 | Medieval Village Environment | ✅ Done | `MeshingunStudio/Medieval Village/` | plane + cubes |
| ✅ 16 | MemoryOrb_Master Shader Graph | ✅ Done | `Plugins/AllIn1ShaderNodes/` | URP/Lit fallback |
| ✅ 17 | Lumen Lighting + Cinemachine | ✅ Done | `Packages/com.distantlands.lumen/` | plain directional + SimpleFollowCamera |
| ✅ 18 | Audio Integration | ✅ Done | `Game UI & Puzzle Sound Effects Pack/` | silence |
| ✅ 19 | Stylized Weather + Zephyr Wind | ✅ Done | `Unluck Software/Stylized Weather/` + `com.distantlands.zephyr/` | static foliage |
| ✅ 20 | Yarn Spinner Integration | ✅ Done | Yarn Spinner UPM (optional) | Mission01Director inline lines |
| ✅ 21 | Memory Dream Cutscene | ✅ Done | `Cutscene Engine/` + Timeline | hard cut to ledger |
| ✅ 22 | Polished Playable Mission 1 (engineering build) | ✅ Done | (all above) | replaces Phase 12 entirely |
| 🟢 **23** | **Mission 1 Polish Capstone — pause / settings / save / ambient / title card / help / Pickle / M1→M2 hand-off** | 🟢 **Just landed** | + new procedural UI | — |
| 🟢 **24** | **Mission 2 Garden + Cottage Scenes — Mission02Director, herb/tea/choice/cleanse/dream flow** | 🟢 **Just landed** | + 2 new scenes | — |

The project now ships a complete **6-scene, fully polished Mission 1 + 2 playable** behind a single menu click: **`Hearthbound → 🎮 Build POLISHED Mission 1 + 2 (Phase 23)`**.

---

## 🆕 Phase 23 — Mission 1 Polish Capstone  🟢

**The orchestrator. One menu click → full polished playable.**

### Files added (Stage 1: foundation scripts)

| File | Role | LOC |
|---|---|---|
| `Assets/_Project/Scripts/Core/SettingsService.cs` | PlayerPrefs-backed audio/comfort/accessibility settings + `OnSettingsChanged` event | 175 |
| `Assets/_Project/Scripts/Core/SettingsServiceBootstrap.cs` | MonoBehaviour that registers SettingsService at boot | 28 |
| `Assets/_Project/Scripts/UI/PauseMenuUI.cs` | Esc-toggled pause overlay (Resume / Settings / Save & Quit / Quit) | 136 |
| `Assets/_Project/Scripts/UI/HelpOverlayUI.cs` | H-toggled controls + Marin quote card, auto-show on first run | 117 |
| `Assets/_Project/Scripts/UI/MissionTitleCard.cs` | CanvasGroup fade-in title card pulled from MissionSO | 122 |
| `Assets/_Project/Scripts/Audio/AmbientAudio.cs` | Looping AudioSource gated by `SettingsService.EffectiveVolume(Ambient)` | 109 |
| `Assets/_Project/Scripts/UI/MainMenuController.cs` | Updated: emits `OnContinueRequested` + `OnSettingsRequested` events; dim Continue until coordinator confirms autosave | (modified) |

### Files added (Stage 3: coordinators)

| File | Role | LOC |
|---|---|---|
| `Assets/_Project/Scripts/Mission/MainMenuSaveCoordinator.cs` | Bridges UI → Save asmdef. Enables Continue button only when autosave exists; loads on click. | 86 |
| `Assets/_Project/Scripts/Mission/PauseSaveCoordinator.cs` | Listens to `PauseMenuUI.OnSaveAndQuitRequested` and `DayEndedEvent`; writes autosave with current scene name. | 60 |

### The capstone builder (Stage 3)

`Assets/_Project/Scripts/Editor/Phase23_Mission1PolishCapstone.cs` (~670 LOC)

Menu: **`Hearthbound → 🎮 Build POLISHED Mission 1 + 2 (Phase 23)`**

Workflow (idempotent — re-run any number of times):
1. Runs Phase 22 (which itself runs Phases 13–21 and rebuilds the 4 base scenes).
2. Runs Phase 24 (Mission 2 Garden + Cottage scenes).
3. Post-processes every scene to add the polish layer:
   - **Bootstrap**: `SettingsServiceBootstrap` on `_GameRoot`.
   - **MainMenu**: Settings panel + `ComfortToolsMenu` (Gentle Mode, Auto-Polish, Auto-Cleanse, Subtitle Size) + `MainMenuSaveCoordinator`.
   - **Lane**: AmbientAudio + Pause menu + Help overlay + MissionTitleCard ("Day 1 — Opening the Hollow").
   - **Hollow**: AmbientAudio + Pause + Help + TitleCard + Pickle the cat + `Mission01Director.sceneAfterEndOfDay = "04_Mission02_Garden"` (the narrative hand-off).
   - **Garden**: AmbientAudio + Pause + Help.
   - **Cottage**: AmbientAudio + Pause + Help.
4. Updates Build Settings — 6 scenes, stable indices.
5. Opens `00_Bootstrap.unity` so the user can press Play immediately.

---

## 🆕 Phase 24 — Mission 2 Garden + Cottage Scenes  🟢

**The Widower's Request — full Garden→Brew→Cottage→Choice→Cleanse→Dream2 flow.**

### Files added

| File | Role | LOC |
|---|---|---|
| `Assets/_Project/Scripts/Mission/Mission02Director.cs` | Runtime orchestrator for both Mission 2 scenes (SceneRole.Garden / Cottage) | 479 |
| `Assets/_Project/Scripts/Editor/Phase24_Mission2SceneBuilder.cs` | Editor menu that builds the 2 Mission 2 scenes from scratch | 795 |

Menu: **`Hearthbound → Phase 24 — Build Mission 2 Scenes`**

### Mission 2 Narrative Flow

**04_Mission02_Garden** (Build index 4)
1. Title card: "Day 2 — The Widower's Request"
2. Marin's note: "Lavender for openness. Valerian to forget for an hour."
3. Player harvests at least one herb (Lavender + Valerian planted at ±3.5 from origin).
4. Player walks to the kettle → opens `TeaBrewingUI`.
5. Tea brewed → garden exit unlocks.
6. Walking through `Garden_Exit_Trigger` loads `05_Mission02_Cottage`.

**05_Mission02_Cottage** (Build index 5)
1. Title card: "Day 2 — Gerrold's Cottage"
2. Narration: "The cottage is quiet. The chair by the bed is still pulled out."
3. Player approaches Gerrold → 3-line greeting + reply choice (3 options, +0/+3/+6 trust ripple).
4. Gerrold prompt for choice → `ChoiceCardUI` shows 4 tariffs: **Erase / Cleanse / Listen / Defer**.
5. Branch:
   - **Erase**: `CleanseMiniGame` (gentleMode=true) → Dream 2 Variant A
   - **Cleanse**: `CleanseMiniGame` (full difficulty) → Dream 2 Variant B (or C if `CleanseOutcome.CrossedCore`)
   - **Listen**: No mini-game, 4-beat narrative pause → Dream 2 Variant D
   - **Defer**: No mini-game, orb returns to Hollow → Dream 2 Variant E
6. Outcome-specific Evening Ledger (5 distinct passages).
7. End Day → return to Main Menu (Mission 3 is future).

### Mission 2 Architecture

- **No new asmdef deps** — Mission asmdef already references `Audio + Core + Cutscene + Memory + MiniGames + Player + UI`.
- **Trigger proxies** inner-class pattern matches `Mission01Director`.
- **Event subscriptions** to `HerbHarvestedEvent` + `TeaBrewedEvent` correctly unsubscribed in `OnDestroy`.
- **Dream 2 variant routing**: `MemoryDreamSequencer.PlayDream2(choice, outcome)` selects the right of 5 PlayableAssets; falls back gracefully if Phase 21 hasn't run.

---

## ✅ Phases 0–22 — Earlier History (condensed)

Earlier roadmap entries are preserved in `CHANGELOG.md`. Key landmarks:
- **Phase 0–10.8**: Architecture, asmdef graph, 32 C# scripts, 5 Yarn files, Save service, mini-games, UI, URP setup, shader patcher.
- **Phase 11**: Editor menu seed-asset generator (17 ScriptableObjects), Input Actions asset, Save-event bug fixes, 13 additional EditMode tests.
- **Phase 12**: Engineering MVP smoke-test (primitives + flat UI + minimum playable loop).
- **Phase 13–21**: Asset-driven prefab builders for characters, UI, environment, shaders, lighting, audio, weather, dialogue, cutscenes.
- **Phase 22**: Engineering "Polished Playable Mission 1" — chains all phase outputs into the 4 base scenes.

---

## Decisions Made (D-001 → D-031)

| # | Decision | Phase | Reason |
|---|---|---|---|
| D-001 | BoZo (chibi) over City Characters | 13 | Cozy by candlelight; mobile-friendly poly count |
| D-002 | Yarn Spinner over OpenAI Dialogue addon | 6 | Authorial intent — every line should "be in a novel" |
| D-003 | Keep vendor folders in place (no relocation) | 0 | Preserves .meta GUIDs; avoids 5 GB reimport |
| D-004 | One asmdef per Scripts/ subfolder | 0 | 80% faster iteration |
| D-005 | `InteractionPromptUI` lives in Player asmdef | 5 | No UI→Player dependency cycle |
| D-006 | `YarnVillageStateBridge` compile-guarded by `YARN_SPINNER_PRESENT` | 6 | Project compiles before Yarn install |
| D-007 | Scenes are Unity-side, scripts are GitHub-side (D-007 corollary) | 9 | Avoid scene merge conflicts |
| D-008 | Drop `com.unity.textmeshpro` from manifest | 10.5 | Unity 6 folded TMP into UGUI 2.0 |
| D-009 | ListenSceneSequencer integrates Cinemachine via reflection | 11 | Cutscene asmdef stays Cinemachine-agnostic |
| D-010 | Seed assets generated by Editor menu, not committed | 11 | Avoids GUID/.meta race conditions |
| D-011..D-023 | Phase 13–22 architectural choices | 13..22 | (see prior CHANGELOG entries) |
| D-024 | Bamao sprite auto-detection by structural scoring | 14 | 300+ sprites; structural scoring picks robustly |
| D-025 | Image.Type.Sliced when sprite has 9-slice border | 14 | Scales dialogue panels at any resolution |
| D-026 | Color fallback for missing Bamao sprites | 14 | Builder never fails |
| D-027 | Each Phase builder exposes `TryGet*Prefab()` lookups | 14 | Progressive polish chain |
| **D-028** | **SettingsService is a plain C# class, not a ScriptableObject** | **23** | **PlayerPrefs-backed; settings survive uninstall via OS keystore** |
| **D-029** | **MainMenuSaveCoordinator + PauseSaveCoordinator live in Mission asmdef** | **23** | **UI stays Save-free; matches DreamHook pattern from Phase 22** |
| **D-030** | **TeaBrewingUI default duration = 12 s (was 90)** | **24** | **90 s is too long for a first-play loop; the player can still set Gentle Mode for longer timers** |
| **D-031** | **Mission01Director.sceneAfterEndOfDay defaults to MainMenu but is overridden to "04_Mission02_Garden" by Phase 23** | **23** | **Default stays safe (no Mission 2 = no broken handoff); polish capstone wires the hand-off** |

---

## 🛠️ Editor Menu Items Available (cumulative — 11 total)

| Menu Path | Purpose | Phase |
|---|---|---|
| **`Hearthbound → 🎮 Build POLISHED Mission 1 + 2 (Phase 23)`** | **🎉 ONE CLICK: full polished playable Mission 1 + 2** | **23** |
| `Hearthbound → 🎮 Build POLISHED Playable Mission 1 (Phase 22)` | Engineering build (no Mission 2, no pause/help/title card) | 22 |
| `Hearthbound → Build Playable Mission 1 (One Click)` | Phase 12 MVP smoke-test | 12 |
| `Hearthbound → Phase 13 — Build BoZo Character Prefabs` | BoZo character wrappers | 13 |
| `Hearthbound → Phase 14 — Build Bamao UI Prefabs` | Bamao parchment UI prefabs | 14 |
| `Hearthbound → Phase 15 — Build Medieval Village dressing` | Cottage/fence/well/tree prefab lookups | 15 |
| `Hearthbound → Phase 18 — Build SFX Library` | Auto-populate SfxLibrarySO from sound pack | 18 |
| **`Hearthbound → Phase 24 — Build Mission 2 Scenes`** | **Garden + Cottage scene builders** | **24** |
| `Hearthbound → Setup URP Pipeline (one-time)` | Activate URP | 10.7 |
| `Hearthbound → Create Mission 1-2 Seed Assets` | Spawn the 17 SOs | 11 |
| `Hearthbound → Validate Mission 1-2 Seed Assets` | Audit missing seeds | 11 |

---

## How to run (current step — full polished play)

1. Pull `feat/mission-1-2-architecture` (or rebase your branch on it).
2. Wait for Unity recompile (~5–10 s; some phases may need ~30 s on first run if packages reimport).
3. **`Hearthbound → 🎮 Build POLISHED Mission 1 + 2 (Phase 23)`** — one click. Sit back for ~30 s while the capstone runs all 12 phases.
4. Press **Play**. The flow is:
   - Bootstrap → Main Menu
   - "Open The Hollow" → Tone Compass (first time)
   - Lane → walk to door → "Enter the Hollow"
   - Hollow → meet Doris → choose a price → polish her First Loaves orb → Evening Ledger
   - **Garden** → harvest lavender or valerian → brew tea → walk to exit
   - **Cottage** → meet Gerrold → moral choice (Erase/Cleanse/Listen/Defer) → optional Cleanse mini-game → Dream 2 cutscene → Evening Ledger
   - → Main Menu (Mission 3 is future)

### Controls (visible any time via `H`)

| Action | Key / Stick |
|---|---|
| Move | WASD / Arrows / Left stick |
| Interact | E / Gamepad ▢ |
| Advance line | Click / Space / Enter |
| Polish orb | Hold left mouse, draw slow circles |
| Pause | Esc |
| Help | H |

---

## 🟢 What's done in this update

- **Phase 23** (Mission 1 polish capstone) — 9 new C# files, 1 master Editor menu, ~1,400 LOC.
- **Phase 24** (Mission 2 scenes + director) — 2 new C# files, ~1,275 LOC.
- 6-scene Build Settings configured automatically by the capstone.
- Continue button + autosave round-trip working (Continue is dim until autosave exists).
- Pause menu in every gameplay scene (Esc).
- Help overlay auto-shows first run, toggle with H.
- Mission title cards fade in on every mission scene start.
- Ambient autumn loop in every gameplay scene, volume-gated by SettingsService.
- Pickle the cat companion in the Hollow with PickleAI tail-flicks on polish completion.
- Mission 1 → Mission 2 narrative hand-off (Doris's ledger now leads into the Garden).

## ⬜ What's NOT done (Mission 3+ deferred per Krieg Discipline)

- Mission 3+ villagers (currently only Doris + Gerrold + 1 silent lane villager).
- Predecessor (Marin) full arc — only one signed note ("— M.") references her.
- Echo Hologram, Locked Room, Forgotten Year, Vance Arc, Mariska, Memory Bees, Composting, Borrowed Memory.
- Weave / Sever / Listen / Read / Translate / Identify / Compose / Search / Negotiate / Compose Verse mini-games.
- Letter-Bird Network, Pen-Pal Villages, Dream Cinema community, ARG, Photo Mode.
- Hollow upgrades beyond Level 1; herbs beyond Lavender + Valerian.
- Composer cues + final VO recordings — Foley + placeholder VO are OK for the build; final audio post-Mission 2.
- Lightmap bake on Mission 2 scenes — done lazily by user when they're satisfied with the scene dressing.
- Mobile build profile + ASTC compression — Mission 3+ scope (per Architecture § 10).
- Mobile IAP, gacha, energy systems — **never** (per Architecture § 13).

---

## 🐞 Known Issues / Follow-Ups (none blocking)

| # | Item | Severity | Status |
|---|---|---|---|
| Prior cycles | various | ✅ Resolved |
| Phase 14 — sprite auto-detection picking wrong sprite | Low | 🟢 Mitigated — top 3 candidates logged per role; manual Image.sprite override available |
| Phase 18 — empty SfxLibrary entries when pack folder names don't match keywords | Low | 🟢 Mitigated — warnings logged per missing entry; drop clips manually onto the entry |
| Phase 23 — Pickle's PickleAI tail bone is null (placeholder sphere has no skeleton) | Cosmetic | 🟡 Acceptable for MVP — PickleAI's headBone/tailBone fields are optional; future polish replaces with real cat mesh |
| Phase 24 — TeaBrewingUI auto-complete button always brews Lavender by default | Low | 🟡 Acceptable — Mission 2 only needs ONE tea to progress; pick the herb you want and click |
| Mission02Director — uses `gerroldVillager` portrait for Marin's note lines in Garden | Cosmetic | 🟡 Acceptable — Marin has no portrait yet; the fallback is the speaker name only |

---

*Last updated: Phase 23 (Polish Capstone) + Phase 24 (Mission 2 Scenes) landed. Next phase: lightmap bakes + composer cue integration + Yarn Spinner full dialogue handoff (Phase 25+).*
