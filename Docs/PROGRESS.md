# 📊 Hearthbound Hollow — Implementation Progress Log

> Continuously updated as the studio builds Mission 1-2. Every PR appends to this file.
> **Unity Editor version: 6000.4.4f1 (Unity 6 LTS)**

---

## Legend
- ✅ Done & merged · 🟢 Done in branch (awaiting your pull) · 🟡 In progress · ⬜ Not started · 🔴 Blocked

---

## 🆕 Phase 26 — Player Controller + Animation  🟢

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

### Files added (8)

| File | Role | LOC |
|---|---|---|
| `Assets/_Project/Scripts/Player/PlayerController.cs` | (rewritten) Camera-relative WASD + Sprint + Jump + gravity + Animator bridge | 350 |
| `Assets/_Project/Scripts/Player/SmoothFollowCamera.cs` | Spring-damped third-person follow camera + orbit + zoom + wall-clip | 230 |
| `Assets/_Project/Scripts/Editor/PlayerAnimatorControllerBuilder.cs` | Procedural AnimatorController builder (1D blend tree on Speed) | 200 |
| `Assets/_Project/Scripts/Editor/Phase26_PlayerControllerAndAnimation.cs` | One-click capstone that builds controller + wires Player prefab + upgrades every scene | 200 |
| `Assets/_Project/Tests/EditMode/PlayerControllerTests.cs` | 7 EditMode tests — locks the public API surface | 140 |
| `Assets/_Project/Settings/HearthboundInput.inputactions` | (rewritten) +5 actions: Sprint, Jump, CameraLook, CameraZoom, AllowLook | 90 |
| `Assets/_Project/Scripts/UI/HelpOverlayUI.cs` | (rewritten) Help card now documents Sprint, Jump, Look, Zoom — Gentle Mode strips Sprint/Jump | 130 |
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

### Decisions adopted in this release

| # | Decision | Why |
|---|---|---|
| D-033 | Sprint + Jump are **opt-in** runtime flags (`enableSprint`, `enableJump`). | Cozy GDD doesn't require them. Gentle Mode disables both. Players who *do* reach for Shift / Space no longer bounce off a "broken" perception. |
| D-034 | Player Animator is **single 1D blend tree on `Speed`** — not 2D. | Cozy character always faces movement direction; `MoveX/MoveY` would be wasted work. Parameters exist for the future 2D-strafe upgrade. |
| D-035 | Animator parameter names are **fixed strings on the controller** but **configurable on `PlayerController`**. | Lets us swap to a community controller (Unity Starter Assets etc.) by re-typing strings in the Inspector — no code rewrite. |
| D-036 | Camera uses `SmoothFollowCamera`, not Cinemachine, as the M1+M2 default. | Cinemachine is heavier and adds a hard package dep to every gameplay scene. Phase 17 still creates a Cinemachine prefab when the package is present — both can coexist. The Phase 26 builder swaps `SimpleFollowCamera` → `SmoothFollowCamera` in every scene; Cinemachine is unaffected. |
| D-037 | Animations live in **`Assets/_Project/Animations/`** (Mixamo subfolder optional). | Single search path keeps Phase 26's auto-detection deterministic. |

### What the user needs to do after pulling

1. Pull `feat/mission-1-2-architecture`. Wait for Unity recompile.
2. **`Hearthbound → 🎮 Build POLISHED Mission 1 + 2 (Phase 23)`** (rebuilds scenes if needed).
3. **`Hearthbound → 🏃 Phase 26 — Player Controller + Animation`** (one click ~5 s).
4. (Optional, +1 min for polish) Open `Docs/ANIMATION_REQUIREMENTS.md` § 3 → download 6 Mixamo FBXs into `Assets/_Project/Animations/Mixamo/` → re-run Phase 26.
5. Press Play in `00_Bootstrap.unity`. Walk to Doris with WASD. Hold Shift to sprint. Right-mouse-drag to orbit camera.

### Known limitations of Phase 26

| # | Item | Severity | Status |
|---|---|---|---|
| P26-1 | Without Mixamo Run clip, sprint loops the Walk animation faster than it should. | Cosmetic | ✅ Acceptable — drop in `Running.fbx` and re-run; Speed=2 motion field auto-upgrades. |
| P26-2 | Without Mixamo Jump clip, the Jump state holds the Idle pose during airtime. | Cosmetic | ✅ Acceptable — same fix. |
| P26-3 | `SmoothFollowCamera` doesn't yet auto-disable during cutscenes (Cutscene Engine cameras do their own thing). | Low | ✅ The cutscene priority overrides via Timeline; not a player-facing issue. |
| P26-4 | The BoZo `BMAC_M_Walk` clip travels in-place; if we ever add a Mixamo clip with baked root motion, set Apply Root Motion = false on the prefab Animator (Phase 26 already does this). | Mitigated | ✅ |

---

## 🚨 HOTFIX (2026-05-24 — after first playtest) — re-run Phase 23

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

## 🎯 Current Status — POLISHED PLAYABLE MISSION 1 + 2 + PHASE 26 LANDED

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
| ✅ 23 | Mission 1 Polish Capstone — pause / settings / save / ambient / title card / help / Pickle / M1→M2 hand-off | ✅ Done | + new procedural UI | — |
| ✅ 24 | Mission 2 Garden + Cottage Scenes — Mission02Director, herb/tea/choice/cleanse/dream flow | ✅ Done | + 2 new scenes | — |
| 🟢 **26** | **Player Controller + Animation — WASD/Sprint/Jump + SmoothFollowCamera + Mixamo-ready Animator** | 🟢 **Just landed** | + Hearthbound_Player.controller + Mixamo guide | replaces walk-only PlayerController + SimpleFollowCamera |

The project now ships a complete **6-scene polished playable + a robust character pipeline** behind two menu clicks:
1. `Hearthbound → 🎮 Build POLISHED Mission 1 + 2 (Phase 23)`
2. `Hearthbound → 🏃 Phase 26 — Player Controller + Animation`

---

## ✅ Earlier Phase Notes (Phases 22–24 condensed; full detail in CHANGELOG)

- **Phase 22** — Engineering "Polished Playable Mission 1" — chains all Phase 13–21 outputs into the 4 base scenes.
- **Phase 23** — Mission 1 polish capstone (~1,400 LOC): SettingsService + PauseMenu + HelpOverlay + MissionTitleCard + AmbientAudio + MainMenuSaveCoordinator + PauseSaveCoordinator + capstone Editor menu that rebuilds 6 scenes idempotently.
- **Phase 24** — Mission 2 scenes (~1,275 LOC): Mission02Director (Garden + Cottage roles) + Phase24 builder for both scenes; full Garden→Brew→Cottage→Choice→Cleanse→Dream2 flow with 4 tariffs and 5 Evening Ledger variants.
- **Hotfix (post-Phase 23)** — SimpleFollowCamera + DreamHook extracted from Editor-asmdef nested classes to runtime asmdef. See D-032.

For the per-file breakdowns, see `CHANGELOG.md` v0.2.0 and v0.3.0.

---

## Decisions Made (D-001 → D-037)

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
| D-029 | MainMenuSaveCoordinator + PauseSaveCoordinator live in Mission asmdef | 23 | UI stays Save-free; matches DreamHook pattern from Phase 22 |
| D-030 | TeaBrewingUI default duration = 12 s (was 90) | 24 | 90 s is too long for a first-play loop; the player can still set Gentle Mode for longer timers |
| D-031 | Mission01Director.sceneAfterEndOfDay defaults to MainMenu but is overridden to "04_Mission02_Garden" by Phase 23 | 23 | Default stays safe (no Mission 2 = no broken handoff); polish capstone wires the hand-off |
| D-032 | **Runtime MonoBehaviours NEVER live in Editor-asmdef files.** | 23 hotfix | Caught the "game not playable" regression in the first playtest. Now a hard rule. |
| **D-033** | **Sprint + Jump are opt-in runtime flags on `PlayerController`. Gentle Mode disables both.** | **26** | Cozy GDD doesn't ask for them. Added as a complete reference so playtesters who reach for Shift/Space don't perceive the controller as broken. |
| **D-034** | **Player Animator is a single 1D blend tree on `Speed` (0=Idle, 1=Walk, 2=Run).** | **26** | Cozy character always faces movement direction; 2D tree would be wasted. `MoveX/MoveY` params remain exposed for the future upgrade. |
| **D-035** | **Animator parameter names are configurable on `PlayerController` Inspector fields.** | **26** | Lets us swap to a community controller by retyping strings — no code rewrite. |
| **D-036** | **`SmoothFollowCamera` is the M1+M2 default, not Cinemachine.** | **26** | Cinemachine is heavier and adds a hard package dep. Phase 17 still creates a Cinemachine prefab when its package is present — both coexist. |
| **D-037** | **Animations live in `Assets/_Project/Animations/` (Mixamo subfolder optional).** | **26** | Single search path keeps Phase 26's auto-detection deterministic. |

---

## 🛠️ Editor Menu Items Available (cumulative — 13 total)

| Menu Path | Purpose | Phase |
|---|---|---|
| **`Hearthbound → 🏃 Phase 26 — Player Controller + Animation`** | **🎉 Rebuild the AnimatorController + wire it into every scene's Player + upgrade follow camera** | **26** |
| **`Hearthbound → 🎮 Build POLISHED Mission 1 + 2 (Phase 23)`** | 🎉 ONE CLICK: full polished playable Mission 1 + 2 | 23 |
| `Hearthbound → 🎮 Build POLISHED Playable Mission 1 (Phase 22)` | Engineering build (no Mission 2, no pause/help/title card) | 22 |
| `Hearthbound → Build Playable Mission 1 (One Click)` | Phase 12 MVP smoke-test | 12 |
| `Hearthbound → Phase 13 — Build BoZo Character Prefabs` | BoZo character wrappers | 13 |
| `Hearthbound → Phase 14 — Build Bamao UI Prefabs` | Bamao parchment UI prefabs | 14 |
| `Hearthbound → Phase 15 — Build Medieval Village dressing` | Cottage/fence/well/tree prefab lookups | 15 |
| `Hearthbound → Phase 18 — Build SFX Library` | Auto-populate SfxLibrarySO from sound pack | 18 |
| **`Hearthbound → Phase 24 — Build Mission 2 Scenes`** | Garden + Cottage scene builders | 24 |
| `Hearthbound → 🔍 Diagnose Phase 23 Build` | Read-only audit — verifies camera + components in every scene | 23 |
| `Hearthbound → Setup URP Pipeline (one-time)` | Activate URP | 10.7 |
| `Hearthbound → Create Mission 1-2 Seed Assets` | Spawn the 17 SOs | 11 |
| `Hearthbound → Validate Mission 1-2 Seed Assets` | Audit missing seeds | 11 |

---

## How to run (current step — full polished play)

1. Pull `feat/mission-1-2-architecture` (or rebase your branch on it).
2. Wait for Unity recompile (~5–10 s; some phases may need ~30 s on first run if packages reimport).
3. **`Hearthbound → 🎮 Build POLISHED Mission 1 + 2 (Phase 23)`** — one click. Sit back for ~30 s while the capstone runs all 12 phases.
4. **`Hearthbound → 🏃 Phase 26 — Player Controller + Animation`** — one click, ~5 s. Builds Hearthbound_Player.controller + upgrades all scene cameras.
5. (Optional) **`Hearthbound → 🔍 Diagnose Phase 23 Build`** to verify everything is wired.
6. Press **Play**. The flow is:
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

- **Phase 26** (Player Controller + Animation) — 4 new C# files + 2 rewrites + Input Actions update + Animation Requirements doc, ~1,300 LOC.
- **Phase 23** (Mission 1 polish capstone) — 9 new C# files, 1 master Editor menu, ~1,400 LOC.
- **Phase 24** (Mission 2 scenes + director) — 2 new C# files, ~1,275 LOC.
- **HOTFIX** — SimpleFollowCamera + DreamHook extracted to runtime asmdefs.
- 6-scene Build Settings configured automatically by the capstone.
- Continue button + autosave round-trip working (Continue is dim until autosave exists).
- Pause menu in every gameplay scene (Esc).
- Help overlay auto-shows first run, toggle with H — now documents Sprint/Jump/Look/Zoom.
- Mission title cards fade in on every mission scene start.
- Ambient autumn loop in every gameplay scene, volume-gated by SettingsService.
- Pickle the cat companion in the Hollow with PickleAI tail-flicks on polish completion.
- Mission 1 → Mission 2 narrative hand-off (Doris's ledger now leads into the Garden).
- **Polished WASD controller** — camera-relative input, smooth acceleration, sprint + jump opt-in.
- **Smooth follow camera** — spring-damped, mouse-orbit, zoom, wall-clip protected.
- **Mixamo-ready Animator** — Humanoid-retargetable; ships working out-of-the-box with BoZo Idle+Walk.

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
| **Phase 26** — Run/Jump/Fall/Land Animator states show Idle pose without Mixamo clips | Cosmetic | 🟡 Drop 6 Mixamo FBXs into `Assets/_Project/Animations/Mixamo/` per `Docs/ANIMATION_REQUIREMENTS.md` § 3 |

---

*Last updated: 2026-05-24 — Phase 26 (Player Controller + Animation) lands alongside the hotfix. Run Phase 23 then Phase 26 in Unity to regenerate scenes + AnimatorController.*
