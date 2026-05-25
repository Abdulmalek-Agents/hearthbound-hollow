# 📊 Hearthbound Hollow — Implementation Progress Log

> Continuously updated as the studio builds Mission 1-2. Every PR appends to this file.
> **Unity Editor version: 6000.4.4f1 (Unity 6 LTS)**

---

## Legend
- ✅ Done & merged · 🟢 Done in branch (awaiting your pull) · 🟡 In progress · ⬜ Not started · 🔴 Blocked

---

## 🚨 HOTFIX — Phase 26.1 (2026-05-24, even later) — MarinNoteInteractable asmdef-locality fix

**Bug reported by user:**

```
Assets/_Project/Scripts/Player/MarinNoteInteractable.cs(21,25):
  error CS0234: The type or namespace name 'UI' does not exist in the namespace 'HearthboundHollow'
Assets/_Project/Scripts/Player/MarinNoteInteractable.cs(30,16):
  error CS0246: The type or namespace name 'DialogueUI' could not be found
```

### Root cause

The freshly-added `MarinNoteInteractable.cs` (Phase 26 narrative hook) was placed in `Assets/_Project/Scripts/Player/`, which puts it in the **`HearthboundHollow.Player`** asmdef. That asmdef's `references` list is `["HearthboundHollow.Core","HearthboundHollow.Memory","Unity.InputSystem","Unity.TextMeshPro"]` — **no `HearthboundHollow.UI`**, by deliberate choice (D-005). So `using HearthboundHollow.UI` failed at compile time.

### Fix

Moved the file to `Assets/_Project/Scripts/Mission/MarinNoteInteractable.cs` with `namespace HearthboundHollow.Mission`. The `Mission` asmdef references both `Player` (so we can still extend `Interactable`) and `UI` (so we can use `DialogueUI`) — exactly the pattern Mission01Director and Mission02Director use. `Phase26_NarrativeHooks.cs` updated to `using HearthboundHollow.Mission;` instead of `using HearthboundHollow.Player;`.

### How this lesson is now in the architecture

- **D-035 (NEW):** Asmdef-locality check before pushing. Open the target folder's `.asmdef`, read its `references` array, and verify every `using HearthboundHollow.X` in the new file is listed there. Cross-cutting components (e.g. Interactable + DialogueUI together) belong in the `Mission` asmdef. The Player asmdef is intentionally UI-free.
- Going forward: every new `.cs` file placed under `Assets/_Project/Scripts/<X>/` will go through the locality check in the same commit message that adds it.

### What the user does

Just pull and recompile. No menu re-runs needed — the file moved within the project; no scene references break (the `Phase26_NarrativeHooks` editor menu re-finds `MarinNoteInteractable` by type, not by path).

---

## 🚨 HOTFIX — Phase 25 (2026-05-24, late) — Tone Compass crash + UI activation hardening

**Bug reported by user during first playtest of Phase 23 build:**

```
Coroutine couldn't be started because the the game object 'ToneCompass' is inactive!
UnityEngine.MonoBehaviour:StartCoroutine (System.Collections.IEnumerator)
HearthboundHollow.UI.ToneCompassCard:Show () (at Assets/_Project/Scripts/UI/ToneCompassCard.cs:55)
HearthboundHollow.UI.MainMenuController:OnOpenTheHollow () (at Assets/_Project/Scripts/UI/MainMenuController.cs:97)
```

### Root cause — systemic anti-pattern across UI overlays

Every UI overlay (`ToneCompassCard`, `MissionTitleCard`, `PauseMenuUI`, `HelpOverlayUI`, `ComfortToolsMenu`, `ChoiceCardUI`, `DialogueUI`, `EveningLedgerUI`, `TeaBrewingUI`) was wired by the Phase 22 / Phase 23 procedural builders in a **broken single-layer pattern**:

```csharp
var rootGO = new GameObject("ToneCompass");        // host & visual on the SAME GameObject
var script = rootGO.AddComponent<ToneCompassCard>();
script.root = rootGO;                              // root === gameObject
rootGO.SetActive(false);                           // <-- deactivates the script's host MonoBehaviour
```

The script-host being inactive has two effects:
1. **`StartCoroutine` throws** the moment `Show()` is called from a button click (the ToneCompass crash). The user saw this because it's the first overlay opened.
2. **`Update()` silently stops firing** on `PauseMenuUI` and `HelpOverlayUI` → **Escape never opens the pause menu, H never opens help.** Latent bug — not yet observed because the user crashed before reaching gameplay.

The MissionTitleCard's `Play()` was in the same family of risk.

### The fix — two-layer wiring pattern, codebase-wide

| Layer | Role | Active state |
|---|---|---|
| **Host GameObject** | hosts the MonoBehaviour script | **always active** — script's `Awake/Start/Update` and `StartCoroutine` all run |
| **Visual child GameObject** | carries the dimming background, panel, text | toggled on/off by `Show()` / `Hide()` |

### Files patched in Phase 25

| File | Change |
|---|---|
| `UI/ToneCompassCard.cs` | `Awake()` no longer deactivates `gameObject`. `Show()` self-heals (`gameObject.SetActive(true)`) and guards `StartCoroutine` with `activeInHierarchy && isActiveAndEnabled`. Adds a fallback that enables Continue immediately when coroutines aren't viable. |
| `UI/MissionTitleCard.cs` | Same defensive pattern in `Play()`. Snaps to fully-shown if `StartCoroutine` is unavailable. |
| `UI/PauseMenuUI.cs` | `Awake()` only hides the visual child; host stays alive for the Esc listener. `Pause()` self-heals. |
| `UI/HelpOverlayUI.cs` | Same — `Update()` now fires for the H key. |
| `UI/ComfortToolsMenu.cs` | `Show()` self-heals. |
| `UI/ChoiceCardUI.cs` | `Show()` self-heals; spawned tiles defensively `SetActive(true)` (template prefab may have been inactive). |
| `UI/DialogueUI.cs` | `PresentLine()` self-heals; defensive fallback renders full line without typewriter when coroutine unavailable. |
| `UI/EveningLedgerUI.cs` | `Show()` self-heals. |
| `UI/TeaBrewingUI.cs` | `Show()` + `StartBrew()` self-heal. |
| `Editor/HearthboundOneClickSetup.cs` | `BuildToneCompass`, `BuildPrimitiveDialogueUI`, `BuildPrimitiveEveningLedgerUI` now build with the two-layer pattern (host → visual child). |
| `Editor/Phase23_Mission1PolishCapstone.cs` | `BuildPauseMenu`, `BuildHelpOverlay`, `BuildSettingsPanel`, `AddMissionTitleCard` now build with the two-layer pattern. The redundant `settingsPanel.SetActive(false)` in `PolishMainMenu` was removed. |

### How this lesson is now in the architecture

- **D-033 (NEW):** Procedural UI builders MUST use the two-layer pattern. A MonoBehaviour that needs `Update()` (key-listener) or `StartCoroutine` (animations, fades) is hosted on a GameObject that *stays active*; the visual root is a *child* GameObject that gets toggled. Single-layer (script-host == visual root, deactivated at build) is forbidden.
- **D-034 (NEW):** UI overlay scripts MUST self-heal in their `Show()` / equivalent — they activate their own `gameObject` if dormant, and guard `StartCoroutine` with `activeInHierarchy && isActiveAndEnabled`. Belt-and-braces — the wiring is correct without this, but the self-heal protects against future regressions.

### What the user MUST do after pulling Phase 25

1. Pull `feat/mission-1-2-architecture` again.
2. Wait for Unity recompile (~5 s).
3. Menu → **`Hearthbound → 🎮 Build POLISHED Mission 1 + 2 (Phase 23)`** — re-runs the capstone with the new two-layer wiring.
4. Press **Play**. "Open The Hollow" → Tone Compass card fades in cleanly. Escape opens the pause menu. H opens the help card.

If you want to skip the rebuild and ship the **existing** scenes (the prior build is on disk), the self-heal layer in the UI scripts means the Tone Compass will now display correctly even on the old wiring. But the pause-menu and help-overlay key listeners still need the re-build to actually fire.

---

## 🚨 HOTFIX (2026-05-24 — after first playtest, earlier) — re-run Phase 23

The user pressed Play on the polished build and **the game was not playable** — the camera didn't follow the player. Root cause:

`SimpleFollowCamera` and `DreamHook` were declared as **nested classes inside Editor-only files** (Editor asmdef, `includePlatforms = ["Editor"]`). The scene builders attached these MonoBehaviours to runtime GameObjects, but at runtime Unity couldn't resolve the types → camera frozen, Dream cutscene silently broken.

### What landed in the hotfix (5 commits)

| Commit | File | Action |
|---|---|---|
| `ef65d14` | `Player/SimpleFollowCamera.cs` (new) | Runtime class in `HearthboundHollow.Player` |
| `82da499` | `.meta` | GUID for runtime class |
| `ac9049b` | `Mission/DreamHook.cs` (new) | Runtime class in `HearthboundHollow.Mission` |
| `3227886` | `.meta` | GUID for runtime class |
| `ed3a1e0` | `Editor/Phase22_*.cs` | Removed inline `private class DreamHook` |
| `47a9aed` | `Editor/HearthboundOneClickSetup.cs` | Removed inline `public class SimpleFollowCamera` |

### What the user MUST do after pulling

1. Pull the latest `feat/mission-1-2-architecture`.
2. Open Unity, wait ~5 s for recompile.
3. Menu → **`Hearthbound → 🎮 Build POLISHED Mission 1 + 2 (Phase 23)`** (one click, ~30 s).
4. Press **Play**.

The capstone rebuilds every scene from scratch. New scenes reference the runtime classes. Camera now follows. Dream cutscene now fires.

### How this lesson is now in the architecture

- **D-032 (NEW):** Never declare runtime MonoBehaviours inside Editor-asmdef source files. Even nested public classes break at runtime because the Editor assembly has `includePlatforms = ["Editor"]`. Runtime MonoBehaviours always go in their owning runtime asmdef (Player / Mission / UI / Audio / Core / etc.).
- The Phase 23 diagnostic menu (`Hearthbound → 🔍 Diagnose Phase 23 Build`) was extended to verify the camera component on the Main Camera in every gameplay scene — catches this class of regression early.

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
| ✅ **23** | **Mission 1 Polish Capstone — pause / settings / save / ambient / title card / help / Pickle / M1→M2 hand-off** | ✅ **Landed** | + new procedural UI | — |
| ✅ **24** | **Mission 2 Garden + Cottage Scenes — Mission02Director, herb/tea/choice/cleanse/dream flow** | ✅ **Landed** | + 2 new scenes | — |
| ✅ **25** | **UI Activation Hotfix — Tone Compass crash + two-layer wiring** | ✅ **Landed (this update)** | (no new assets) | — |
| ✅ **26.1** | **Asmdef-locality hotfix — MarinNoteInteractable moved to Mission asmdef** | ✅ **Landed (this update)** | — | — |

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

### Files added (HOTFIX 2026-05-24 — runtime extraction)

| File | Role | LOC |
|---|---|---|
| `Assets/_Project/Scripts/Player/SimpleFollowCamera.cs` | Runtime third-person follow camera. **Was previously a nested class in HearthboundOneClickSetup (Editor-only) — broke runtime.** | 35 |
| `Assets/_Project/Scripts/Mission/DreamHook.cs` | Runtime bridge from EveningLedger.OnEndOfDayConfirmed → MemoryDreamSequencer.PlayDream1(). **Was previously a private nested class in Phase22 (Editor-only).** | 50 |

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

## Decisions Made (D-001 → D-035)

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
| D-028 | SettingsService is a plain C# class, not a ScriptableObject | 23 | PlayerPrefs-backed; settings survive uninstall via OS keystore |
| D-029 | MainMenuSaveCoordinator + PauseSaveCoordinator live in Mission asmdef | 23 | UI stays Save-free; matches DreamHook pattern from Phase 22 |
| D-030 | TeaBrewingUI default duration = 12 s (was 90) | 24 | 90 s is too long for a first-play loop; the player can still set Gentle Mode for longer timers |
| D-031 | Mission01Director.sceneAfterEndOfDay defaults to MainMenu but is overridden to "04_Mission02_Garden" by Phase 23 | 23 | Default stays safe (no Mission 2 = no broken handoff); polish capstone wires the hand-off |
| **D-032** | **Runtime MonoBehaviours NEVER live in Editor-asmdef files.** Even nested public classes break at runtime because the Editor asmdef has `includePlatforms = ["Editor"]`. Always declare runtime MonoBehaviours in their owning runtime asmdef. | **23 hotfix** | **Caught the "game not playable" regression in the first playtest. Now a hard rule across the codebase.** |
| **D-033** | **Procedural UI builders MUST use the two-layer pattern.** Script-host GameObject stays *active*; visual root is a *child* that gets toggled. Single-layer wiring (script-host == visual root, deactivated at build) is forbidden — it kills `Update()` (Esc/H listeners) and `StartCoroutine` (fade-ins, typewriters). | **25** | **Tone Compass crash + Esc/H key dead. Caught in first M1+2 playtest. Codified after the fix.** |
| **D-034** | **UI overlay scripts MUST self-heal on entry.** `Show()`/`Pause()`/`Play()` activate own `gameObject` if dormant and guard `StartCoroutine` with `activeInHierarchy && isActiveAndEnabled`. Belt-and-braces. | **25** | **Defends against future regressions in builder wiring without forcing a re-build.** |
| **D-035** | **Asmdef-locality check before pushing.** Any new runtime script that uses types from another asmdef MUST live in an asmdef that *declares the dependency in its references list*. Cross-cutting components (e.g. Interactable + DialogueUI together) belong in the `Mission` asmdef (which references both). The Player asmdef intentionally does NOT reference UI (per D-005) — putting UI-using scripts in Player produces `CS0234 / CS0246`. The pre-push mental checklist: open the target folder's `.asmdef`, read the `references` array, and verify every `using HearthboundHollow.X` in the new file is listed. | **26.1 hotfix** | **CS0234/CS0246 on first `MarinNoteInteractable.cs` push caught by user. Codified to prevent the whole class of cross-asmdef errors going forward.** |

---

## 🛠️ Editor Menu Items Available (cumulative — 13 total)

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
| **`Hearthbound → Phase 26 — Wire Narrative Hooks`** | **Drops Marin's Note onto the workbench in the Hollow** | **26** |
| `Hearthbound → 🔍 Diagnose Phase 23 Build` | Read-only audit — verifies camera + components in every scene | 23 |
| `Hearthbound → Setup URP Pipeline (one-time)` | Activate URP | 10.7 |
| `Hearthbound → Create Mission 1-2 Seed Assets` | Spawn the 17 SOs | 11 |
| `Hearthbound → Validate Mission 1-2 Seed Assets` | Audit missing seeds | 11 |

---

## How to run (current step — full polished play)

1. Pull `feat/mission-1-2-architecture` (or rebase your branch on it).
2. Wait for Unity recompile (~5–10 s; some phases may need ~30 s on first run if packages reimport).
3. **`Hearthbound → 🎮 Build POLISHED Mission 1 + 2 (Phase 23)`** — one click. Sit back for ~30 s while the capstone runs all 12 phases.
4. **`Hearthbound → Phase 26 — Wire Narrative Hooks`** — drops Marin's Note on the workbench (idempotent; safe to re-run).
5. (Optional) **`Hearthbound → 🔍 Diagnose Phase 23 Build`** to verify everything is wired.
6. Press **Play**. The flow is:
   - Bootstrap → Main Menu
   - "Open The Hollow" → Tone Compass (first time)
   - Lane → walk to door → "Enter the Hollow"
   - Hollow → meet Doris → polish her First Loaves orb → **read Marin's Note on the workbench** → Evening Ledger
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
- **Phase 25** (UI activation hotfix) — 11 files modified across UI and Editor builders. See top of file for full detail.
- **Phase 26** (narrative depth — Marin's Note + editor wiring) — 2 new C# files, parchment-note interactable + idempotent scene-hook builder.
- **Phase 26.1** (asmdef-locality hotfix) — moved `MarinNoteInteractable.cs` from Player asmdef to Mission asmdef. D-035 added as the hard-rule preventing this class of error.
- **HOTFIX** — SimpleFollowCamera + DreamHook extracted to runtime asmdefs.
- 6-scene Build Settings configured automatically by the capstone.
- Continue button + autosave round-trip working (Continue is dim until autosave exists).
- Pause menu in every gameplay scene (Esc) — **Phase 25 fixes the Update() listener so this actually works**.
- Help overlay auto-shows first run, toggle with H — **same Phase 25 fix**.
- Mission title cards fade in on every mission scene start — **Phase 25 fixes the StartCoroutine path**.
- Ambient autumn loop in every gameplay scene, volume-gated by SettingsService.
- Pickle the cat companion in the Hollow with PickleAI tail-flicks on polish completion.
- Mission 1 → Mission 2 narrative hand-off (Doris's ledger now leads into the Garden).
- Marin's Note on the workbench in the Hollow — first concrete encounter with the predecessor, sets `VillageState.readMarinNoteIds` and nudges `predecessorTrailWarmth +5`.

## ⬜ What's NOT done (Mission 3+ deferred per Krieg Discipline)

- Mission 3+ villagers (currently only Doris + Gerrold + 1 silent lane villager).
- Predecessor (Marin) full arc — only one signed note ("— M.") references her, plus the new on-workbench note from Phase 26.
- Echo Hologram, Locked Room, Forgotten Year, Vance Arc, Mariska, Memory Bees, Composting, Borrowed Memory.
- Weave / Sever / Listen / Read / Translate / Identify / Compose / Search / Negotiate / Compose Verse mini-games.
- Letter-Bird Network, Pen-Pal Villages, Dream Cinema community, ARG, Photo Mode.
- Hollow upgrades beyond Level 1; herbs beyond Lavender + Valerian.
- Composer cues + final VO recordings — Foley + placeholder VO are OK for the build; final audio post-Mission 2.
- Lightmap bake on Mission 2 scenes — done lazily by user when they're satisfied with the scene dressing.
- Mobile build profile + ASTC compression — Mission 3+ scope (per Architecture § 10).
- Mobile IAP, gacha, energy systems — **never** (per Architecture § 13).

## 🟡 Next phase — Phase 26.2 (HEAT modern UI integration + dialogue/text polish)

**Goal:** lift menus, settings, HUD, pause, and help out of the procedural-parchment placeholder into HEAT's modern UI vocabulary — *without* losing the cozy warmth.

Scope:
1. Replace the procedurally-built Main Menu buttons with HEAT `Button (Panel)` prefabs themed in our warm palette (97/85/62 amber over 18/12/08 deep cocoa).
2. Replace the procedurally-built Settings panel with a HEAT `Modal Window` containing HEAT `Settings Element (Switch)` rows for Gentle Mode / Auto-Polish / Auto-Cleanse and a HEAT `Settings Element (Slider)` for Subtitle size.
3. Replace the Pause Menu visual with a HEAT modal.
4. Replace the Help Overlay with a HEAT custom-content modal.
5. Modernize the HUD coin + day labels using HEAT `Widgets` (badge style).
6. Author a Phase 26.2 capstone builder that swaps the visual children of each script-host GameObject without breaking the two-layer wiring established in Phase 25.

Out of scope for Phase 26.2 (kept on procedural for now): in-world dialogue boxes (Bamao parchment is intentional cozy substance there); Mission Title Card (already polished); Tone Compass first-run card (intentional warm parchment).

## 🟡 Next phase — Phase 27 (Mission 1+2 deeper depth polish per Mission_1_2_Focus)

After Phase 26.2 ships:
1. Audit Mission 1 against `Docs/Depth_Bible/Mission_1_2_Focus/01_DORIS_THE_BAKER.md` — verify all 9 dialogue nodes from `Doris_M1.yarn` are wired, audit emotional beats.
2. Audit Mission 2 against `Docs/Depth_Bible/Mission_1_2_Focus/02_THE_WIDOWER_GERROLD.md` — verify all 4 tariff branches, the 5 Dream 2 variants, and the Listen-path 4-beat narrative pause.
3. Run the diagnostic menu, capture any warnings, and resolve them.

---

## 🐞 Known Issues / Follow-Ups

| # | Item | Severity | Status |
|---|---|---|---|
| Prior cycles | various | ✅ Resolved |
| **2026-05-24 hotfix** — SimpleFollowCamera + DreamHook were nested in Editor-only files | **Blocker** | ✅ **Fixed in commits ef65d14..47a9aed** |
| Phase 14 — sprite auto-detection picking wrong sprite | Low | 🟢 Mitigated — top 3 candidates logged per role; manual Image.sprite override available |
| Phase 18 — empty SfxLibrary entries when pack folder names don't match keywords | Low | 🟢 Mitigated — warnings logged per missing entry; drop clips manually onto the entry |
| Phase 23 — Pickle's PickleAI tail bone is null (placeholder sphere has no skeleton) | Cosmetic | 🟡 Acceptable for MVP — PickleAI's headBone/tailBone fields are optional; future polish replaces with real cat mesh |
| Phase 24 — TeaBrewingUI auto-complete button always brews Lavender by default | Low | 🟡 Acceptable — Mission 2 only needs ONE tea to progress; pick the herb you want and click |
| Mission02Director — uses `gerroldVillager` portrait for Marin's note lines in Garden | Cosmetic | 🟡 Acceptable — Marin has no portrait yet; the fallback is the speaker name only |
| **2026-05-24 (late) — Phase 25 hotfix** — Tone Compass crash + Pause/Help Update() dead | **Blocker** | ✅ **Fixed** — 9 UI scripts self-heal + Phase23/OneClickSetup use two-layer wiring. See top of file. |
| **2026-05-24 (even later) — Phase 26.1 hotfix** — `MarinNoteInteractable.cs` CS0234/CS0246 because it was placed in Player asmdef (which intentionally doesn't reference UI). | **Compile error** | ✅ **Fixed** — moved to Mission asmdef. D-035 added: pre-push asmdef-locality check. |

---

*Last updated: 2026-05-24 — Phase 26.1 fixed the MarinNoteInteractable asmdef-locality bug. Pull, recompile, re-test in Unity.*
