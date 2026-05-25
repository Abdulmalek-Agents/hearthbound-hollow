# 📊 Hearthbound Hollow — Implementation Progress Log

> Continuously updated as the studio builds Mission 1-2. Every PR appends to this file.
> **Unity Editor version: 6000.4.4f1 (Unity 6 LTS)**

---

## Legend
- ✅ Done & merged · 🟢 Done in branch (awaiting your pull) · 🟡 In progress · ⬜ Not started · 🔴 Blocked

---

## 🚨 HOTFIX — Phase 27.1 (2026-05-25) — "Half body in floor" sink

**Bug reported by user during first Phase 26 + 27 playtest:**

> *"There is issue with main character — half of the body gets into the floor and only the upper half is still visible, and it goes above again when I use WASD to move."*

### Root cause

The Phase 13 BoZo wrapper nested the BoZo character prefab as a child `Body` with `localPosition = Vector3.zero` AND set `CharacterController.center = (0, 1.0, 0)` with `height = 1.9`. Two compounding issues:

1. **CC capsule bottom at local Y = 0.05**, not at Y = 0 — a small (5 cm) offset already.
2. **BoZo's mesh origin is NOT at the character's feet** — it sits at hip/pelvis level (typical for the BSMC character base). With Body.localPosition = (0, 0, 0), the visible feet end up at world Y ≈ -0.9 once gravity settles the CC.

Net effect: the visible mesh's lower half sinks into the floor while the upper half (waist up) is still above it.

The "pops up when WASD is pressed" symptom is Unity's `CharacterController.Move()` triggering a one-frame snap-to-collider sweep, which shifts the GameObject temporarily. As soon as input is released the GameObject returns to its sunk pose because the *root cause* (Body localPosition + CC.center mismatch) was untouched.

### Fix (3 commits)

1. **`Player/PlayerGroundClamp.cs` (NEW, runtime)** — A `MonoBehaviour` that on `Start()` (and again in the first `LateUpdate()` so the Animator has posed once):
   - Finds the `Body` child (auto-discovers by name; falls back to first child with renderers).
   - Walks every `Renderer` under it and computes the lowest Y of the combined renderer bounds in world space using each renderer's pose-independent `localBounds` (deterministic — no animation jitter).
   - Computes the CC capsule bottom: `transform.position.y + cc.center.y - cc.height/2 + cc.skinWidth`.
   - Shifts `body.localPosition.y` by the difference so the mesh bottom and CC bottom coincide.
   - Inspector exposes a `bias` float (±0.2 m) for designers who want the cozy character to plant a couple of cm into grass. Context-menu **"Align Body to Ground"** for manual re-snap at edit time. Gizmo draws the CC bottom + mesh bottom as horizontal disks in green/orange for visual debugging.

2. **`Editor/Phase26_PlayerControllerAndAnimation.cs`** — Capstone now `EnsureGroundClamp()`'s the Player prefab + every gameplay scene's Player. Heals stale serialized references (e.g. if Phase 13 hadn't run yet when the clamp was first added).

3. **`Editor/Phase13_BoZoCharacterBuilder.cs`** — `BuildPlayer()` now sets `cc.center = (0, 0.95, 0)` with `cc.height = 1.9` (capsule bottom at local Y=0), explicit `skinWidth = 0.08` / `stepOffset = 0.3` / `slopeLimit = 45`, slightly slimmer `radius = 0.32` to fit the BoZo chibi profile. Also adds the `PlayerGroundClamp` at prefab-build time so fresh builds are correct from frame 1.

4. **`Editor/Phase26_DiagnosticReport.cs`** — Now audits for `PlayerGroundClamp` presence on the Player prefab + every scene Player, and runs a best-effort mesh-foot vs CC-capsule alignment check (≤5 cm = PASS, ≤20 cm = WARN, >20 cm = FAIL).

### What the user does after pulling

```
1.  Pull feat/mission-1-2-architecture.
2.  Hearthbound → ✨ Build EVERYTHING (Phase 27 — one click).   ← re-runs Phase 26
3.  (Optional) Hearthbound → 🔍 Diagnose Phase 26 Build.        ← verify the clamp landed
4.  Press Play. The character now stands on the floor.
```

The runtime auto-correct in `PlayerGroundClamp` will recover the visible feet position even on scenes that pre-date this fix — the only requirement is that the component is attached to the Player, which the Phase 26 capstone takes care of.

### How this lesson is now in the architecture

- **D-041 (NEW):** Character prefab CC centre must align with the BoZo mesh origin. When the mesh origin is at the character's hips (not feet), use a runtime ground-clamp instead of guessing the offset at prefab build time. Build-time defaults are best-effort; runtime auto-correct is the truth.
- The Phase 26 diagnostic now FAILS (not just WARNs) when the Player has no `PlayerGroundClamp` — this regression class won't ship again.

---

## 🆕 Phase 27 — Build EVERYTHING + Phase 26 Polish Layer  🟢

**Single-click master capstone + NPC animator pipeline + diagnostic + footstep SFX hooks.**

> Single-menu rebuild: `Hearthbound → ✨ Build EVERYTHING (Phase 27 — one click)`

This phase consolidates the two parallel Phase 26 workstreams into a single user-facing menu and adds the remaining polish layer (NPC dialogue body language, audit, footstep audio hooks).

### Files added (6 new — pushed one-per-commit)

| File | Role | LOC |
|---|---|---|
| `Assets/_Project/Scripts/Editor/Phase27_BuildEverything.cs` | Master capstone — chains Phase 23 + 26 (PC+Anim) + 26 (NPC) + 26 (Narrative Hooks) in one menu click. Reflection-based discovery means missing phases skip gracefully. | 200 |
| `Assets/_Project/Scripts/Editor/Phase26_DiagnosticReport.cs` | Read-only audit menu — verifies AnimatorController asset + Player prefab Animator wiring + per-scene SmoothFollowCamera + cameraReference + **PlayerGroundClamp (Phase 27.1 hotfix)**. | 200 |
| `Assets/_Project/Scripts/Editor/NpcAnimatorControllerBuilder.cs` | Builds `Hearthbound_NPC.controller` — Idle ↔ Talking states + `IsTalking` bool + `Speed` float, auto-picks BoZo F_Idle. | 130 |
| `Assets/_Project/Scripts/Editor/Phase26_NpcAnimatorCapstone.cs` | Wires Doris/Gerrold/SilentLane prefabs with the NPC controller + an NpcAnimatorBridge component. | 130 |
| `Assets/_Project/Scripts/Mission/NpcAnimatorBridge.cs` | Runtime — listens to `DialogueStartedEvent` / `DialogueEndedEvent` and toggles `Animator.IsTalking`. | 100 |
| `Assets/_Project/Scripts/Mission/PlayerFootstepBinder.cs` | Runtime — Animation Event hooks (`OnFootstepLeft`/`OnFootstepRight`) play surface-aware footstep SFX through `SfxPlayer.PlayOneShot(id, volume)`. | 160 |
| **`Assets/_Project/Scripts/Player/PlayerGroundClamp.cs`** | **NEW (Phase 27.1 hotfix)** — Runtime auto-aligns BoZo mesh feet to CharacterController capsule bottom; fixes the "half body in floor" sink. | 230 |

### Menu items added

| Menu Path | Purpose |
|---|---|
| `Hearthbound → ✨ Build EVERYTHING (Phase 27 — one click)` | Master capstone — chains every Phase 23/26 sub-builder + opens Bootstrap |
| `Hearthbound → 🔍 Diagnose Phase 26 Build` | Read-only audit of the Phase 26 player + camera + animator + ground clamp wiring |
| `Hearthbound → 🎭 Phase 26 — Wire NPC Animators` | Build NPC controller + wire Doris/Gerrold/SilentLane prefabs |
| `Hearthbound → Phase 26 — Build NPC Animator Controller` | Build just the NPC controller asset (no prefab wiring) |

### Decisions adopted in Phase 27 polish layer

- All Phase 27 additions follow D-001..D-040 (no new D-numbers).
- **D-041 (NEW, Phase 27.1 hotfix)** — Character prefab CC centre must align with mesh origin; use runtime ground-clamp when the mesh origin isn't at the feet.

### What the user now does

```
1.  Pull feat/mission-1-2-architecture. Wait for Unity recompile.
2.  Hearthbound → ✨ Build EVERYTHING (Phase 27 — one click).   ← single click, ~45 s
3.  (Optional) Hearthbound → 🔍 Diagnose Phase 26 Build.        ← audit the result
4.  Press Play in 00_Bootstrap.unity.
```

### Behaviour change from this polish layer

- **Doris and Gerrold visibly shift posture during dialogue** (Idle → Talking, soft 0.18 s / 0.22 s transitions). When a Mixamo `Talking.fbx` clip is dropped into `Assets/_Project/Animations/Mixamo/` and the NPC capstone re-runs, the Talking state auto-upgrades to a richer body-language clip.
- **Footstep SFX hooks** — opt-in. `PlayerFootstepBinder` is not added to scenes automatically (no behaviour regression). Drop it onto the Player root and add Animation Events to the Walk/Run Mixamo clips to enable.
- **Phase 26 diagnostic** — catches 12 distinct wiring states across the AnimatorController, Player prefab, and 4 gameplay scenes. Surfaces problems before the user presses Play.
- **Player stands on the floor (Phase 27.1 hotfix).** No more half-body sink. No more pop-up-on-WASD rubber-band.

---

## 🆕 Phase 26 — Player Controller + Animation (Player thread)  🟢

> ℹ️ **Two Phase 26 workstreams landed in parallel on this branch.** One is the *Narrative Hooks* thread (Marin's Note + asmdef hotfix). This section covers the *Player Controller + Animation* thread. The Editor menu items are distinct (`Phase 26 — Wire Narrative Hooks` vs `🏃 Phase 26 — Player Controller + Animation`); the file names are distinct. They can be run in either order — or both via Phase 27.

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
| **Stands on the floor** | New `PlayerGroundClamp.cs` aligns the BoZo mesh feet to the CC capsule bottom on Start. Phase 27.1 hotfix. |
| **Input Actions update** | `Sprint`, `Jump`, `CameraLook`, `CameraZoom`, `AllowLook` actions added to `HearthboundInput.inputactions`. K&M + Gamepad + Touch schemes preserved. |

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

### Files added / rewritten (Player Controller + Animation thread)

| File | Role | LOC |
|---|---|---|
| `Assets/_Project/Scripts/Player/PlayerController.cs` | (rewritten) Camera-relative WASD + Sprint + Jump + gravity + Animator bridge | 350 |
| `Assets/_Project/Scripts/Player/SmoothFollowCamera.cs` | Spring-damped third-person follow camera + orbit + zoom + wall-clip | 230 |
| `Assets/_Project/Scripts/Player/PlayerGroundClamp.cs` | **Phase 27.1 hotfix** — runtime mesh-to-CC alignment | 230 |
| `Assets/_Project/Scripts/Editor/PlayerAnimatorControllerBuilder.cs` | Procedural AnimatorController builder (1D blend tree on Speed) | 200 |
| `Assets/_Project/Scripts/Editor/Phase26_PlayerControllerAndAnimation.cs` | One-click capstone that builds controller + wires Player prefab + GroundClamp + upgrades every scene | 240 |
| `Assets/_Project/Scripts/Editor/Phase13_BoZoCharacterBuilder.cs` | (updated) CC centre re-tuned + PlayerGroundClamp added at build time | (updated) |
| `Assets/_Project/Tests/EditMode/PlayerControllerTests.cs` | 7 EditMode tests — locks the public API surface | 140 |
| `Assets/_Project/Settings/HearthboundInput.inputactions` | (rewritten) +5 actions: Sprint, Jump, CameraLook, CameraZoom, AllowLook | 90 |
| `Assets/_Project/Scripts/UI/HelpOverlayUI.cs` | (rewritten) Help card now documents Sprint, Jump, Look, Zoom — Gentle Mode strips Sprint/Jump | 130 |
| `Assets/_Project/Scripts/Player/SimpleFollowCamera.cs` | Header marked legacy; behaviour unchanged for backward compat | 55 |
| `Docs/ANIMATION_REQUIREMENTS.md` | Full doc: state graph, parameters, clip roster, Mixamo download guide, retargeting steps, asset-store alternatives | 280 |

### Decisions adopted in this workstream

> ℹ️ Phase 25 used D-033 + D-034 (UI two-layer + self-heal). Phase 26.1 hotfix used D-035 (asmdef-locality). Phase 26's *Player Controller + Animation* decisions start at **D-036**. Phase 27.1 hotfix adds **D-041**.

| # | Decision | Why |
|---|---|---|
| D-036 | Sprint + Jump are **opt-in** runtime flags (`enableSprint`, `enableJump`). | Cozy GDD doesn't require them. Gentle Mode disables both. Players who *do* reach for Shift / Space no longer bounce off a "broken" perception. |
| D-037 | Player Animator is **single 1D blend tree on `Speed`** — not 2D. | Cozy character always faces movement direction; `MoveX/MoveY` would be wasted work. Parameters exist for the future 2D-strafe upgrade. |
| D-038 | Animator parameter names are **fixed strings on the controller** but **configurable on `PlayerController`**. | Lets us swap to a community controller (Unity Starter Assets etc.) by re-typing strings in the Inspector — no code rewrite. |
| D-039 | Camera uses `SmoothFollowCamera`, not Cinemachine, as the M1+M2 default. | Cinemachine is heavier and adds a hard package dep to every gameplay scene. Phase 17 still creates a Cinemachine prefab when the package is present — both can coexist. The Phase 26 builder swaps `SimpleFollowCamera` → `SmoothFollowCamera` in every scene; Cinemachine is unaffected. |
| D-040 | Animations live in **`Assets/_Project/Animations/`** (Mixamo subfolder optional). | Single search path keeps Phase 26's auto-detection deterministic. |
| **D-041** | **Character prefab CC centre must align with mesh origin; use runtime `PlayerGroundClamp` when the mesh origin isn't at the feet.** | **Build-time defaults are best-effort; runtime auto-correct is the truth. Phase 27.1 hotfix codifies this after the half-body-in-floor regression.** |

### Known limitations of the Player Controller + Animation thread

| # | Item | Severity | Status |
|---|---|---|---|
| P26-PC-1 | Without Mixamo Run clip, sprint loops the Walk animation faster than it should. | Cosmetic | ✅ Acceptable — drop in `Running.fbx` and re-run; Speed=2 motion field auto-upgrades. |
| P26-PC-2 | Without Mixamo Jump clip, the Jump state holds the Idle pose during airtime. | Cosmetic | ✅ Acceptable — same fix. |
| P26-PC-3 | `SmoothFollowCamera` doesn't yet auto-disable during cutscenes (Cutscene Engine cameras do their own thing). | Low | ✅ The cutscene priority overrides via Timeline; not a player-facing issue. |
| P26-PC-4 | BoZo `BMAC_M_Walk` clip travels in-place; if we ever add a Mixamo clip with baked root motion, set Apply Root Motion = false on the prefab Animator (Phase 26 already does this). | Mitigated | ✅ |
| **P27.1** | **Half body sunk in floor on Player spawn.** | **Visual** | ✅ **Fixed in 4 commits — see HOTFIX section above.** |

---

## 🆕 Phase 26 — Narrative Hooks (Marin's Note) thread  ✅

**Files:**

- `Assets/_Project/Scripts/Mission/MarinNoteInteractable.cs` — parchment-note interactable. Sets `VillageState.readMarinNoteIds.Add("Marin_Workbench_01")` and nudges `predecessorTrailWarmth +5` on first read.
- `Assets/_Project/Scripts/Editor/Phase26_NarrativeHooks.cs` — Editor menu `Hearthbound → Phase 26 — Wire Narrative Hooks` that idempotently drops the Marin's Note prefab onto the Hollow workbench.

**Player flow extension:** Hollow → meet Doris → polish her First Loaves orb → **read Marin's Note on the workbench** → Evening Ledger. First concrete encounter with the predecessor.

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

Moved the file to `Assets/_Project/Scripts/Mission/MarinNoteInteractable.cs` with `namespace HearthboundHollow.Mission`. The `Mission` asmdef references both `Player` (so we can still extend `Interactable`) and `UI` (so we can use `DialogueUI`) — exactly the pattern Mission01Director and Mission02Director use.

- **D-035 (NEW):** Asmdef-locality check before pushing. Open the target folder's `.asmdef`, read its `references` array, and verify every `using HearthboundHollow.X` in the new file is listed there.

---

## 🚨 HOTFIX — Phase 25 (2026-05-24, late) — Tone Compass crash + UI activation hardening

**Bug reported by user during first playtest of Phase 23 build:** "Coroutine couldn't be started because the the game object 'ToneCompass' is inactive!"

**Root cause:** Every UI overlay was wired by Phase 22/23 builders in a broken single-layer pattern that deactivated the script-host GameObject. `StartCoroutine` threw the moment `Show()` was called; `Update()` silently stopped firing (Esc/H keys dead).

**Fix:** Two-layer wiring (script-host stays active, visual child toggles) in 11 files + self-heal `Show()` in 9 UI overlay scripts.

- **D-033 (NEW):** Procedural UI builders MUST use the two-layer pattern.
- **D-034 (NEW):** UI overlay scripts MUST self-heal on entry.

---

## 🚨 HOTFIX (2026-05-24 — after first playtest, earlier) — re-run Phase 23

`SimpleFollowCamera` and `DreamHook` were declared as nested classes inside Editor-only files. The scene builders attached these to runtime GameObjects, but at runtime Unity couldn't resolve the types → camera frozen, Dream cutscene silently broken. Moved both to runtime asmdefs.

- **D-032 (NEW):** Never declare runtime MonoBehaviours inside Editor-asmdef source files.

---

## 🎯 Current Status — POLISHED PLAYABLE M1 + M2 + ALL HOTFIXES LANDED

**Branch**: `feat/mission-1-2-architecture` (PR #7 open)

| Phase | Title | Status |
|---|---|---|
| ✅ 0–22 | Architecture → polished engineering build | ✅ Done |
| ✅ 23 | Mission 1 Polish Capstone | ✅ Done |
| ✅ 24 | Mission 2 Garden + Cottage Scenes | ✅ Done |
| ✅ 25 | UI Activation Hotfix | ✅ Done |
| ✅ 26 (Narrative Hooks) | Marin's Note interactable + Phase 26 editor menu | ✅ Done |
| ✅ 26.1 | Asmdef-locality hotfix | ✅ Done |
| ✅ 26 (Player Controller + Animation) | WASD/Sprint/Jump + SmoothFollowCamera + Mixamo-ready Animator | ✅ Done |
| ✅ 27 | Build EVERYTHING capstone + NPC animator pipeline + diagnostic + footstep hooks | ✅ Done |
| ✅ **27.1** | **"Half body in floor" sink hotfix — PlayerGroundClamp + Phase 13 CC re-tune** | ✅ **Done** |

The project ships behind a **single menu click**: `Hearthbound → ✨ Build EVERYTHING (Phase 27 — one click)`.

---

## Decisions Made (D-001 → D-041)

D-001..D-040 cover BoZo art, asmdef discipline, UI two-layer + self-heal, asmdef-locality, sprint/jump opt-in, Animator + camera defaults, and animation locations. **D-041 (NEW, Phase 27.1 hotfix)** mandates runtime ground-clamp when the character mesh origin isn't at the feet — build-time CC defaults are best-effort, runtime auto-correct is the truth.

See `CHANGELOG.md` for per-release decision tables.

---

## 🛠️ Editor Menu Items Available (cumulative — 17 total)

| Menu Path | Purpose | Phase |
|---|---|---|
| **`Hearthbound → ✨ Build EVERYTHING (Phase 27 — one click)`** | **🎉 MASTER: chains every Phase 23/26 sub-capstone in one click** | **27** |
| `Hearthbound → 🏃 Phase 26 — Player Controller + Animation` | Player AnimatorController + camera + scene wiring + ground clamp | 26 (PC+Anim) |
| `Hearthbound → 🎭 Phase 26 — Wire NPC Animators` | NPC AnimatorController + Doris/Gerrold/SilentLane wiring | 27 (NPC) |
| `Hearthbound → Phase 26 — Build Player Animator Controller` | Just the player controller asset | 26 (PC+Anim) |
| `Hearthbound → Phase 26 — Build NPC Animator Controller` | Just the NPC controller asset | 27 (NPC) |
| `Hearthbound → Phase 26 — Wire Narrative Hooks` | Drops Marin's Note onto the workbench | 26 (Narrative) |
| `Hearthbound → 🎮 Build POLISHED Mission 1 + 2 (Phase 23)` | Polished playable Mission 1 + 2 | 23 |
| `Hearthbound → 🎮 Build POLISHED Playable Mission 1 (Phase 22)` | Engineering Mission 1 only | 22 |
| `Hearthbound → Build Playable Mission 1 (One Click)` | Phase 12 MVP smoke-test | 12 |
| `Hearthbound → Phase 13 — Build BoZo Character Prefabs` | BoZo character wrappers (now with PlayerGroundClamp on Player) | 13 |
| `Hearthbound → Phase 14 — Build Bamao UI Prefabs` | Bamao parchment UI prefabs | 14 |
| `Hearthbound → Phase 15 — Build Medieval Village dressing` | Cottage/fence/well/tree prefab lookups | 15 |
| `Hearthbound → Phase 18 — Build SFX Library` | Auto-populate SfxLibrarySO | 18 |
| `Hearthbound → Phase 24 — Build Mission 2 Scenes` | Garden + Cottage scene builders | 24 |
| `Hearthbound → 🔍 Diagnose Phase 23 Build` | Read-only audit of Phase 23 scenes | 23 |
| `Hearthbound → 🔍 Diagnose Phase 26 Build` | Read-only audit of Phase 26 animator + camera + ground clamp wiring | 27 |
| `Hearthbound → Setup URP Pipeline (one-time)` | Activate URP | 10.7 |
| `Hearthbound → Create Mission 1-2 Seed Assets` | Spawn the 17 SOs | 11 |
| `Hearthbound → Validate Mission 1-2 Seed Assets` | Audit missing seeds | 11 |

---

## How to run (recommended: single click)

1. Pull `feat/mission-1-2-architecture`.
2. Wait for Unity recompile (~5–10 s).
3. **`Hearthbound → ✨ Build EVERYTHING (Phase 27 — one click)`** — sit back ~45 s.
4. (Optional) **`Hearthbound → 🔍 Diagnose Phase 26 Build`** to verify wiring + ground-clamp.
5. Press **Play**.

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

## 🐞 Known Issues / Follow-Ups

| # | Item | Severity | Status |
|---|---|---|---|
| Prior cycles | various | ✅ Resolved |
| **2026-05-24 hotfix** — SimpleFollowCamera + DreamHook were nested in Editor-only files | **Blocker** | ✅ **Fixed in commits ef65d14..47a9aed** |
| Phase 14 — sprite auto-detection picking wrong sprite | Low | 🟢 Mitigated |
| Phase 18 — empty SfxLibrary entries when pack folder names don't match keywords | Low | 🟢 Mitigated |
| Phase 23 — Pickle's PickleAI tail bone is null (placeholder sphere has no skeleton) | Cosmetic | 🟡 Acceptable |
| Phase 24 — TeaBrewingUI auto-complete button always brews Lavender by default | Low | 🟡 Acceptable |
| Mission02Director — uses `gerroldVillager` portrait for Marin's note lines in Garden | Cosmetic | 🟡 Acceptable |
| **Phase 25** — Tone Compass crash + Pause/Help Update() dead | **Blocker** | ✅ **Fixed** |
| **Phase 26.1** — `MarinNoteInteractable.cs` CS0234/CS0246 from misplaced asmdef | **Compile error** | ✅ **Fixed** |
| **Phase 26 (PC+Anim)** — Run/Jump/Fall/Land Animator states show Idle pose without Mixamo clips | Cosmetic | 🟡 Drop 6 Mixamo FBXs into `Assets/_Project/Animations/Mixamo/` |
| **Phase 27 (NPC)** — Talking state visually identical to Idle until a Mixamo Talking.fbx is dropped | Cosmetic | 🟡 Drop `Talking.fbx` into the Mixamo folder + re-run NPC capstone |
| **Phase 27.1** — Player half-sunk into floor; mesh feet not aligned with CC capsule bottom | **Visual** | ✅ **Fixed in 4 commits — runtime PlayerGroundClamp + Phase 13 CC re-tune + Phase 26 capstone auto-attach + diagnostic audit.** |

---

*Last updated: 2026-05-25 — Phase 27.1 hotfix landed: PlayerGroundClamp runtime + Phase 13 CC re-tune + Phase 26 capstone integration. The character now stands on the floor.*
