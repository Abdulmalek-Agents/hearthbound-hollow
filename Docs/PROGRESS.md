# 📊 Hearthbound Hollow — Implementation Progress Log

> Continuously updated as the studio builds Mission 1-2. Every PR appends to this file.
> **Unity Editor version: 6000.4.4f1 (Unity 6 LTS)**

---

## Legend
- ✅ Done & merged · 🟢 Done in branch (awaiting your pull) · 🟡 In progress · ⬜ Not started · 🔴 Blocked

---

## 🆕 Phase 26 — Player Controller + Animation (parallel workstream)  🟢

> ℹ️ **Two Phase 26 workstreams landed in parallel on this branch.** One is the *Narrative Hooks* thread (Marin's Note + asmdef hotfix, see § "Phase 26 Narrative Hooks" below + Phase 26.1 hotfix entry). This section covers the *Player Controller + Animation* thread, dispatched by the senior team. The Editor menu items are distinct (`Phase 26 — Wire Narrative Hooks` vs `🏃 Phase 26 — Player Controller + Animation`); the file names are distinct (`Phase26_NarrativeHooks.cs` vs `Phase26_PlayerControllerAndAnimation.cs`). They can be run in either order.

**The complete WASD player + Mixamo-ready Humanoid Animator.**

> Single-menu rebuild: `Hearthbound → 🏃 Phase 26 — Player Controller + Animation`

### What the player now has

| Feature | Detail |
|---|---|
| **Camera-relative WASD** | Input is transformed by Main Camera's planar forward/right — moves *toward* where you're looking, not world-axis. Falls back to world-axis when no camera is present (smoke scenes + EditMode tests). |
| **Smooth acceleration** | SerializeField'd `acceleration = 16` and `deceleration = 22` give a controllable feel curve. No more instant-stop snap from the old `SimpleMove`. |
| **Sprint (Shift)** | `enableSprint` default-on. Held Left Shift / right Shift / gamepad LStick-click. Disabled while Gentle Mode is on (per Codex 06). |
| **Jump (Space)** | `enableJump` default-on. Manual gravity integration on `CharacterController.Move()`. Coyote-time `0.15 s` + jump-buffer `0.12 s` for forgiving feel. Disabled in Gentle Mode. |
| **Animator bridge** | 7 parameters driven every frame: `Speed`, `MoveX`, `MoveY`, `VelocityY`, `IsGrounded`, `IsSprinting`, `Jump`. |
| **Smooth follow camera** | New `SmoothFollowCamera.cs` with `SmoothDamp` position, slerped rotation, mouse-orbit (RMB hold) + scroll-zoom + sphere-cast wall-clip protection. |
| **Input Actions update** | `Sprint`, `Jump`, `CameraLook`, `CameraZoom`, `AllowLook` actions added to `HearthboundInput.inputactions`. K&M + Gamepad + Touch schemes preserved. |

### Files added / rewritten (Player Controller + Animation thread)

| File | Role | LOC |
|---|---|---|
| `Assets/_Project/Scripts/Player/PlayerController.cs` | (rewritten) Camera-relative WASD + Sprint + Jump + gravity + Animator bridge | 350 |
| `Assets/_Project/Scripts/Player/SmoothFollowCamera.cs` | Spring-damped third-person follow camera + orbit + zoom + wall-clip | 230 |
| `Assets/_Project/Scripts/Editor/PlayerAnimatorControllerBuilder.cs` | Procedural AnimatorController builder (1D blend tree on Speed) | 200 |
| `Assets/_Project/Scripts/Editor/Phase26_PlayerControllerAndAnimation.cs` | One-click capstone that builds controller + wires Player prefab + upgrades every scene | 200 |
| `Assets/_Project/Tests/EditMode/PlayerControllerTests.cs` | 7 EditMode tests — locks the public API surface | 140 |
| `Assets/_Project/Settings/HearthboundInput.inputactions` | (rewritten) +5 actions: Sprint, Jump, CameraLook, CameraZoom, AllowLook | 90 |
| `Assets/_Project/Scripts/UI/HelpOverlayUI.cs` | (rewritten) Help card now documents Sprint, Jump, Look, Zoom — Gentle Mode strips Sprint/Jump | 130 |
| `Assets/_Project/Scripts/Player/SimpleFollowCamera.cs` | Header marked legacy; behaviour unchanged for backward compat | 55 |
| `Docs/ANIMATION_REQUIREMENTS.md` | Full doc: state graph, parameters, clip roster, Mixamo download guide, retargeting steps, asset-store alternatives | 280 |

### Animator state graph (built procedurally)

```
LOCOMOTION (1D Blend Tree on Speed)
  ├─ Idle    Speed = 0
  ├─ Walk    Speed = 1
  └─ Run     Speed = 2

LOCOMOTION ──Jump trigger──▶ JUMP ──VelocityY<-0.5──▶ FALL
                              │                        │
                              └────IsGrounded──▶ LAND ◀┘
                                                  │
                                                  └─ exit 0.8s ─▶ LOCOMOTION
```

### Decisions adopted in this workstream

> ℹ️ Phase 25 used D-033 + D-034 (UI two-layer + self-heal). Phase 26.1 hotfix used D-035 (asmdef-locality). Phase 26's *Player Controller + Animation* decisions therefore start at **D-036**.

| # | Decision | Why |
|---|---|---|
| D-036 | Sprint + Jump are **opt-in** runtime flags (`enableSprint`, `enableJump`). | Cozy GDD doesn't require them. Gentle Mode disables both. Players who *do* reach for Shift / Space no longer bounce off a "broken" perception. |
| D-037 | Player Animator is **single 1D blend tree on `Speed`** — not 2D. | Cozy character always faces movement direction; `MoveX/MoveY` would be wasted work. Parameters exist for the future 2D-strafe upgrade. |
| D-038 | Animator parameter names are **fixed strings on the controller** but **configurable on `PlayerController`**. | Lets us swap to a community controller (Unity Starter Assets etc.) by re-typing strings in the Inspector — no code rewrite. |
| D-039 | Camera uses `SmoothFollowCamera`, not Cinemachine, as the M1+M2 default. | Cinemachine is heavier and adds a hard package dep to every gameplay scene. Phase 17 still creates a Cinemachine prefab when the package is present — both can coexist. The Phase 26 builder swaps `SimpleFollowCamera` → `SmoothFollowCamera` in every scene; Cinemachine is unaffected. |
| D-040 | Animations live in **`Assets/_Project/Animations/`** (Mixamo subfolder optional). | Single search path keeps Phase 26's auto-detection deterministic. |

### What the user needs to do after pulling

1. Pull `feat/mission-1-2-architecture`. Wait for Unity recompile.
2. **`Hearthbound → 🎮 Build POLISHED Mission 1 + 2 (Phase 23)`** (rebuilds scenes if needed).
3. **`Hearthbound → 🏃 Phase 26 — Player Controller + Animation`** (one click ~5 s).
4. **`Hearthbound → Phase 26 — Wire Narrative Hooks`** (drops Marin's Note on the workbench; the Narrative Hooks thread).
5. (Optional, +1 min for polish) Open `Docs/ANIMATION_REQUIREMENTS.md` § 3 → download 6 Mixamo FBXs into `Assets/_Project/Animations/Mixamo/` → re-run Phase 26 (Player Controller + Animation).
6. Press Play in `00_Bootstrap.unity`. Walk to Doris with WASD. Hold Shift to sprint. Right-mouse-drag to orbit camera. Read Marin's Note at the workbench.

### Known limitations of the Player Controller + Animation thread

| # | Item | Severity | Status |
|---|---|---|---|
| P26-PC-1 | Without Mixamo Run clip, sprint loops the Walk animation faster than it should. | Cosmetic | ✅ Acceptable — drop in `Running.fbx` and re-run; Speed=2 motion field auto-upgrades. |
| P26-PC-2 | Without Mixamo Jump clip, the Jump state holds the Idle pose during airtime. | Cosmetic | ✅ Acceptable — same fix. |
| P26-PC-3 | `SmoothFollowCamera` doesn't yet auto-disable during cutscenes (Cutscene Engine cameras do their own thing). | Low | ✅ The cutscene priority overrides via Timeline; not a player-facing issue. |
| P26-PC-4 | The BoZo `BMAC_M_Walk` clip travels in-place; if we ever add a Mixamo clip with baked root motion, set Apply Root Motion = false on the prefab Animator (Phase 26 already does this). | Mitigated | ✅ |

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

### How this lesson is now in the architecture

- **D-032 (NEW):** Never declare runtime MonoBehaviours inside Editor-asmdef source files. Even nested public classes break at runtime because the Editor assembly has `includePlatforms = ["Editor"]`. Runtime MonoBehaviours always go in their owning runtime asmdef (Player / Mission / UI / Audio / Core / etc.).

---

## 🎯 Current Status — POLISHED PLAYABLE M1 + M2 + BOTH PHASE 26 WORKSTREAMS LANDED

**Branch**: `feat/mission-1-2-architecture` (PR #7 open)

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
| ✅ **23** | **Mission 1 Polish Capstone** | ✅ **Landed** | + new procedural UI | — |
| ✅ **24** | **Mission 2 Garden + Cottage Scenes** | ✅ **Landed** | + 2 new scenes | — |
| ✅ **25** | **UI Activation Hotfix — two-layer wiring** | ✅ **Landed** | (no new assets) | — |
| ✅ **26 (Narrative Hooks thread)** | **Marin's Note interactable + Phase 26 editor menu** | ✅ **Landed** | — | — |
| ✅ **26.1** | **Asmdef-locality hotfix — MarinNoteInteractable moved to Mission asmdef** | ✅ **Landed** | — | — |
| 🟢 **26 (Player Controller + Animation thread)** | **PlayerController WASD/Sprint/Jump + SmoothFollowCamera + Mixamo-ready Animator** | 🟢 **Just landed** | + Hearthbound_Player.controller + Mixamo guide | replaces walk-only PlayerController + SimpleFollowCamera |

---

## 🆕 Phase 26 — Narrative Hooks (Marin's Note) thread  ✅

**Files:**

- `Assets/_Project/Scripts/Mission/MarinNoteInteractable.cs` — parchment-note interactable. Sets `VillageState.readMarinNoteIds.Add("Marin_Workbench_01")` and nudges `predecessorTrailWarmth +5` on first read.
- `Assets/_Project/Scripts/Editor/Phase26_NarrativeHooks.cs` — Editor menu `Hearthbound → Phase 26 — Wire Narrative Hooks` that idempotently drops the Marin's Note prefab onto the Hollow workbench.

**Player flow extension:** Hollow → meet Doris → polish her First Loaves orb → **read Marin's Note on the workbench** → Evening Ledger. First concrete encounter with the predecessor.

---

## ✅ Earlier Phase Notes (Phases 22–25 condensed; full detail in CHANGELOG)

- **Phase 22** — Engineering "Polished Playable Mission 1" — chains all Phase 13–21 outputs into the 4 base scenes.
- **Phase 23** — Mission 1 polish capstone (~1,400 LOC): SettingsService + PauseMenu + HelpOverlay + MissionTitleCard + AmbientAudio + MainMenuSaveCoordinator + PauseSaveCoordinator.
- **Phase 24** — Mission 2 scenes (~1,275 LOC): Mission02Director + Phase24 builder for both scenes.
- **Phase 25** — UI activation hotfix: two-layer wiring + self-heal Show() in every overlay. **D-033 + D-034.**
- **Phase 26 Narrative Hooks** — MarinNoteInteractable + editor menu. **D-035 (asmdef-locality from the 26.1 hotfix).**
- **Phase 26 Player Controller + Animation** — this update. **D-036..D-040.**

For the per-file breakdowns, see `CHANGELOG.md` v0.2.0, v0.2.1, and v0.3.0.

---

## Decisions Made (D-001 → D-040)

| # | Decision | Phase | Reason |
|---|---|---|---|
| D-001 | BoZo (chibi) over City Characters | 13 | Cozy by candlelight; mobile-friendly poly count |
| D-002 | Yarn Spinner over OpenAI Dialogue addon | 6 | Authorial intent — every line should "be in a novel" |
| D-003 | Keep vendor folders in place (no relocation) | 0 | Preserves .meta GUIDs; avoids 5 GB reimport |
| D-004 | One asmdef per Scripts/ subfolder | 0 | 80% faster iteration |
| D-005 | `InteractionPromptUI` lives in Player asmdef | 5 | No UI→Player dependency cycle |
| D-006 | `YarnVillageStateBridge` compile-guarded by `YARN_SPINNER_PRESENT` | 6 | Project compiles before Yarn install |
| D-007 | Scenes are Unity-side, scripts are GitHub-side | 9 | Avoid scene merge conflicts |
| D-008 | Drop `com.unity.textmeshpro` from manifest | 10.5 | Unity 6 folded TMP into UGUI 2.0 |
| D-009 | ListenSceneSequencer integrates Cinemachine via reflection | 11 | Cutscene asmdef stays Cinemachine-agnostic |
| D-010 | Seed assets generated by Editor menu, not committed | 11 | Avoids GUID/.meta race conditions |
| D-011..D-027 | Phase 13–22 architectural choices | 13..22 | (see CHANGELOG) |
| D-028 | SettingsService is a plain C# class, not a ScriptableObject | 23 | PlayerPrefs-backed; settings survive uninstall via OS keystore |
| D-029 | MainMenuSaveCoordinator + PauseSaveCoordinator live in Mission asmdef | 23 | UI stays Save-free |
| D-030 | TeaBrewingUI default duration = 12 s (was 90) | 24 | 90 s is too long for a first-play loop |
| D-031 | Mission01Director.sceneAfterEndOfDay defaults to MainMenu, overridden to "04_Mission02_Garden" by Phase 23 | 23 | Default stays safe |
| D-032 | **Runtime MonoBehaviours NEVER live in Editor-asmdef files.** | 23 hotfix | Caught the "game not playable" regression. |
| D-033 | **Procedural UI builders MUST use the two-layer pattern.** Script-host stays active; visual child toggles. | 25 | Tone Compass crash + Esc/H key dead. |
| D-034 | **UI overlay scripts MUST self-heal on entry.** `Show()`/`Pause()`/`Play()` activate own `gameObject` if dormant. | 25 | Belt-and-braces for D-033. |
| D-035 | **Asmdef-locality check before pushing.** Any new runtime script that uses types from another asmdef MUST live in an asmdef that declares the dependency. | 26.1 hotfix | CS0234/CS0246 caught by user; codified pre-push checklist. |
| **D-036** | **Sprint + Jump are opt-in runtime flags on `PlayerController`. Gentle Mode disables both.** | **26 (Player Controller + Animation)** | Cozy GDD doesn't ask for them. Added as a complete reference. |
| **D-037** | **Player Animator is a single 1D blend tree on `Speed` (0=Idle, 1=Walk, 2=Run).** | **26 (Player Controller + Animation)** | Cozy character always faces movement direction; 2D tree would be wasted. |
| **D-038** | **Animator parameter names are configurable on `PlayerController` Inspector fields.** | **26 (Player Controller + Animation)** | Lets us swap to a community controller by retyping strings. |
| **D-039** | **`SmoothFollowCamera` is the M1+M2 default, not Cinemachine.** | **26 (Player Controller + Animation)** | Cinemachine is heavier and adds a hard package dep. Phase 17 prefab coexists. |
| **D-040** | **Animations live in `Assets/_Project/Animations/` (Mixamo subfolder optional).** | **26 (Player Controller + Animation)** | Single search path keeps auto-detection deterministic. |

---

## 🛠️ Editor Menu Items Available (cumulative — 14 total)

| Menu Path | Purpose | Phase |
|---|---|---|
| **`Hearthbound → 🏃 Phase 26 — Player Controller + Animation`** | **🎉 Rebuild AnimatorController + wire it into every scene's Player + upgrade follow camera** | **26 (PC+Anim)** |
| **`Hearthbound → 🎮 Build POLISHED Mission 1 + 2 (Phase 23)`** | 🎉 ONE CLICK: full polished playable Mission 1 + 2 | 23 |
| **`Hearthbound → Phase 26 — Wire Narrative Hooks`** | Drops Marin's Note onto the workbench in the Hollow | 26 (Narrative) |
| `Hearthbound → 🎮 Build POLISHED Playable Mission 1 (Phase 22)` | Engineering build (no Mission 2, no pause/help/title card) | 22 |
| `Hearthbound → Build Playable Mission 1 (One Click)` | Phase 12 MVP smoke-test | 12 |
| `Hearthbound → Phase 13 — Build BoZo Character Prefabs` | BoZo character wrappers | 13 |
| `Hearthbound → Phase 14 — Build Bamao UI Prefabs` | Bamao parchment UI prefabs | 14 |
| `Hearthbound → Phase 15 — Build Medieval Village dressing` | Cottage/fence/well/tree prefab lookups | 15 |
| `Hearthbound → Phase 18 — Build SFX Library` | Auto-populate SfxLibrarySO from sound pack | 18 |
| **`Hearthbound → Phase 24 — Build Mission 2 Scenes`** | Garden + Cottage scene builders | 24 |
| `Hearthbound → 🔍 Diagnose Phase 23 Build` | Read-only audit | 23 |
| `Hearthbound → Setup URP Pipeline (one-time)` | Activate URP | 10.7 |
| `Hearthbound → Create Mission 1-2 Seed Assets` | Spawn the 17 SOs | 11 |
| `Hearthbound → Validate Mission 1-2 Seed Assets` | Audit missing seeds | 11 |

---

## How to run (current step — full polished play)

1. Pull `feat/mission-1-2-architecture` (or rebase your branch on it).
2. Wait for Unity recompile (~5–10 s; some phases may need ~30 s on first run if packages reimport).
3. **`Hearthbound → 🎮 Build POLISHED Mission 1 + 2 (Phase 23)`** — one click. Sit back for ~30 s while the capstone runs all 12 phases.
4. **`Hearthbound → 🏃 Phase 26 — Player Controller + Animation`** — one click, ~5 s. Builds Hearthbound_Player.controller + upgrades all scene cameras.
5. **`Hearthbound → Phase 26 — Wire Narrative Hooks`** — drops Marin's Note on the workbench (idempotent).
6. (Optional) **`Hearthbound → 🔍 Diagnose Phase 23 Build`** to verify everything is wired.
7. Press **Play**. The flow is:
   - Bootstrap → Main Menu → "Open The Hollow" → Tone Compass (first time)
   - Lane → walk to door → "Enter the Hollow"
   - Hollow → meet Doris → polish her First Loaves orb → **read Marin's Note on the workbench** → Evening Ledger
   - **Garden** → harvest lavender or valerian → brew tea → walk to exit
   - **Cottage** → meet Gerrold → moral choice (Erase/Cleanse/Listen/Defer) → optional Cleanse mini-game → Dream 2 cutscene → Evening Ledger
   - → Main Menu (Mission 3 is future)

### Controls (visible any time via `H`)

| Action | Key / Stick |
|---|---|
| Move | WASD / Arrows / Left stick |
| **Sprint** | **Left Shift / LStick click** (Gentle Mode disables) |
| **Jump** | **Space / Gamepad south** (Gentle Mode disables) |
| Interact | E / Gamepad ▢ |
| Advance line | Click / Space / Enter |
| Polish orb | Hold left mouse, draw slow circles |
| **Camera look** | **Hold Right Mouse + drag / Right Stick** |
| **Camera zoom** | **Mouse scroll / Gamepad LB-RB** |
| Pause | Esc |
| Help | H |

---

## 🟢 What's done in this update

- **Phase 26 Player Controller + Animation thread** — 4 new C# files + 2 rewrites + Input Actions update + Animation Requirements doc, ~1,300 LOC.
- **Phase 26 Narrative Hooks thread** — MarinNoteInteractable + Phase26_NarrativeHooks editor menu.
- **Phase 26.1 asmdef-locality hotfix** — MarinNoteInteractable moved from Player asmdef to Mission asmdef.
- **Phase 25 UI activation hotfix** — two-layer wiring + self-heal Show().
- **Phase 23** + **Phase 24** — already landed in prior updates.
- **Polished WASD controller** — camera-relative input, smooth acceleration, sprint + jump opt-in.
- **Smooth follow camera** — spring-damped, mouse-orbit, zoom, wall-clip protected.
- **Mixamo-ready Animator** — Humanoid-retargetable; ships working out-of-the-box with BoZo Idle+Walk.
- **Marin's Note on the workbench** — first concrete encounter with the predecessor; sets `VillageState.readMarinNoteIds` and nudges `predecessorTrailWarmth +5`.

## ⬜ What's NOT done (Mission 3+ deferred per Krieg Discipline)

- Mission 3+ villagers (currently only Doris + Gerrold + 1 silent lane villager).
- Predecessor (Marin) full arc — only one signed note + one on-workbench note, no full plotline yet.
- Echo Hologram, Locked Room, Forgotten Year, Vance Arc, Mariska, Memory Bees, Composting, Borrowed Memory.
- Weave / Sever / Listen / Read / Translate / Identify / Compose / Search / Negotiate / Compose Verse mini-games.
- Letter-Bird Network, Pen-Pal Villages, Dream Cinema community, ARG, Photo Mode.
- Hollow upgrades beyond Level 1; herbs beyond Lavender + Valerian.
- Composer cues + final VO recordings.
- Lightmap bake on Mission 2 scenes.
- Mobile build profile + ASTC compression.
- Mobile IAP, gacha, energy systems — **never** (per Architecture § 13).

## 🟡 Next phases

- **Phase 26.2** — HEAT modern UI swap-in for Main Menu / Settings / Pause / Help / HUD. Visual upgrade; two-layer wiring established in Phase 25 is preserved.
- **Phase 27** — Mission 1+2 depth polish against `Docs/Depth_Bible/Mission_1_2_Focus/*`. Audit dialogue beats, surface Marin's note in narrative context, polish emotional pacing.

---

## 🐞 Known Issues / Follow-Ups

| # | Item | Severity | Status |
|---|---|---|---|
| Prior cycles | various | ✅ Resolved |
| **2026-05-24 hotfix** — SimpleFollowCamera + DreamHook were nested in Editor-only files | **Blocker** | ✅ **Fixed in commits ef65d14..47a9aed** |
| Phase 14 — sprite auto-detection picking wrong sprite | Low | 🟢 Mitigated — top 3 candidates logged per role |
| Phase 18 — empty SfxLibrary entries when pack folder names don't match keywords | Low | 🟢 Mitigated — warnings logged per missing entry |
| Phase 23 — Pickle's PickleAI tail bone is null (placeholder sphere has no skeleton) | Cosmetic | 🟡 Acceptable for MVP |
| Phase 24 — TeaBrewingUI auto-complete button always brews Lavender by default | Low | 🟡 Acceptable |
| Mission02Director — uses `gerroldVillager` portrait for Marin's note lines in Garden | Cosmetic | 🟡 Acceptable — Marin has no portrait yet |
| **Phase 25** — Tone Compass crash + Pause/Help Update() dead | **Blocker** | ✅ **Fixed** — 9 UI scripts self-heal + two-layer wiring |
| **Phase 26.1** — `MarinNoteInteractable.cs` CS0234/CS0246 from misplaced asmdef | **Compile error** | ✅ **Fixed** — moved to Mission asmdef. D-035 codified. |
| **Phase 26 (Player Controller + Animation)** — Run/Jump/Fall/Land Animator states show Idle pose without Mixamo clips | Cosmetic | 🟡 Drop 6 Mixamo FBXs into `Assets/_Project/Animations/Mixamo/` per `Docs/ANIMATION_REQUIREMENTS.md` § 3 |

---

*Last updated: 2026-05-24 — Phase 26 (Player Controller + Animation) thread lands alongside the Narrative Hooks thread + Phase 26.1 hotfix. The two parallel Phase 26 workstreams coexist (different file names, different Editor menus). Decisions renumbered to D-036..D-040 so they don't collide with Phase 25's D-033/D-034 or Phase 26.1's D-035.*
