# 🎬 Hearthbound Hollow — Animation Requirements & Mixamo Integration Guide

> **Author:** Character Animator (lead) · **Reviewers:** Senior Unity Developer · Art Director · Creative Director · Critic & Review Board
> **Phase:** 26 — Player Controller + Animation
> **Branch:** `feat/mission-1-2-architecture`
> **Companion Editor menu:** `Hearthbound → 🏃 Phase 26 — Player Controller + Animation`

This document is the single source of truth for **what animations the game needs, what we already have, what to download from Mixamo, and how to wire it all into the AnimatorController** that Phase 26 procedurally generates at `Assets/_Project/Animations/Hearthbound_Player.controller`.

The Phase 26 builder is **graceful** — it picks the best available clip per role and falls back to BoZo's existing two clips if Mixamo isn't installed yet. You can ship Mission 1+2 with **zero Mixamo downloads** (the player walks but never runs / jumps in cozy gameplay). Adding the Mixamo clips just adds polish.

---

## 1. The Player Animator State Graph

Phase 26 builds this graph deterministically:

```
                      ┌──────────────────────────┐
                      │     LOCOMOTION           │
                      │   (1D Blend Tree)        │
                      │   Param: Speed (0..2)    │
                      │                          │
                      │   0  → Idle              │
                      │   1  → Walk              │
                      │   2  → Run               │
                      └────┬─────────────────────┘
                           │   trigger Jump
                           ▼
                      ┌──────────────┐
                      │     JUMP     │ ── VelocityY < -0.5 ──▶ ┌──────────┐
                      └──────┬───────┘                          │   FALL   │
                             │ IsGrounded                       └────┬─────┘
                             ▼                                       │ IsGrounded
                      ┌──────────────┐  ◀──────────────────────────┘
                      │     LAND     │  exitTime=0.8  ─▶ LOCOMOTION
                      └──────────────┘
```

### Animator Parameters

| Param         | Type    | Source (set by `PlayerController.cs`)                              | Default |
|---------------|---------|--------------------------------------------------------------------|---------|
| `Speed`       | float   | `input.magnitude × (sprint ? 2 : 1)` damped 0.1 s                  | 0       |
| `MoveX`       | float   | raw input x (`-1..1`) — for 2D blend trees if you upgrade later    | 0       |
| `MoveY`       | float   | raw input y (`-1..1`)                                              | 0       |
| `VelocityY`   | float   | vertical velocity (m/s) — drives the jump→fall transition          | 0       |
| `IsGrounded`  | bool    | `CharacterController.isGrounded`                                   | **true**|
| `IsSprinting` | bool    | `enableSprint && Shift held && !GentleMode`                        | false   |
| `Jump`        | trigger | fires on jump impulse                                              | —       |

**No other parameter is referenced from runtime code.** This makes the controller swappable: any future Mixamo / motion-matching upgrade just needs to honour the seven names above.

---

## 2. The Animation Clip Roster

The Player needs **six clips** to fully populate the graph. Phase 26 auto-detects them by keyword + Humanoid-motion bonus + path bonus (Mixamo > `_Project/Animations` > BoZo > demo folders).

| Role  | Required? | Used in | Mixamo download name(s) | BoZo fallback |
|-------|-----------|---------|--------------------------|---------------|
| Idle  | ✅ Yes    | Locomotion blend (Speed=0) | `Idle.fbx` / `Breathing Idle.fbx` | `BMAC_M_Idle.anim` |
| Walk  | ✅ Yes    | Locomotion blend (Speed=1) | `Walking.fbx` (in-place, no root motion) | `BMAC_M_Walk.anim` |
| Run   | ⚠️ Recommended | Locomotion blend (Speed=2) | `Running.fbx` (in-place) | _falls back to Walk_ |
| Jump  | ⚠️ Recommended | Jump state | `Jumping In Place.fbx` | _falls back to Idle (hold pose)_ |
| Fall  | ⛔ Optional | Fall state | `Falling Idle.fbx` | _falls back to Jump pose_ |
| Land  | ⛔ Optional | Land state | `Landing.fbx` / `Soft Landing.fbx` | _falls back to Idle_ |

> ℹ️ All clips **must be Humanoid** so they retarget cleanly onto the BoZo Humanoid avatar. If you import as Generic, Phase 26 will pass them over.

---

## 3. Step-by-Step — Adding Mixamo Animations

Mixamo is free (Adobe account required). The cozy game's polish budget is happy with the standard library — we do not need any paid extension packs.

### 3.1 Download (5 minutes)

1. Open **https://www.mixamo.com**.
2. Use any character (e.g. **X Bot**) — clips retarget to BoZo via Unity's Humanoid avatar so the source character doesn't matter.
3. Search for each clip name from the table above. **For each clip:**
   - Check **In Place** (so we control position, not Mixamo's baked root translation).
   - Set **Trim** if needed (the running clip in particular has a flat loop window).
   - Click **Download**.
   - Format: **FBX for Unity (.fbx)** · Skin: **Without Skin** · Frames per Second: **30** · Keyframe reduction: **None**.

The 6 FBXs together total ~1 MB.

### 3.2 Import (Unity)

1. In Unity, create the folder `Assets/_Project/Animations/Mixamo/` if it doesn't exist.
2. Drag the 6 FBX files into it.
3. Select each FBX → Inspector → **Rig** tab → set **Animation Type = Humanoid**, **Avatar Definition = Create From This Model** → **Apply**.
4. **Rig** tab → **Configure…** → close (just confirms Unity built the avatar).
5. **Animation** tab → tick **Loop Time** on `Idle`, `Walking`, `Running`, `Falling Idle`. Leave it off for `Jumping In Place` and `Landing`.

### 3.3 Re-run Phase 26

`Hearthbound → 🏃 Phase 26 — Player Controller + Animation`

The capstone re-runs `PlayerAnimatorControllerBuilder` which now picks the Mixamo clips (higher score) over BoZo's walk-only set.

You'll see logs like:

```
[Hearthbound/Phase 26] Built Assets/_Project/Animations/Hearthbound_Player.controller
  Idle  → Assets/_Project/Animations/Mixamo/Idle.fbx
  Walk  → Assets/_Project/Animations/Mixamo/Walking.fbx
  Run   → Assets/_Project/Animations/Mixamo/Running.fbx
  Jump  → Assets/_Project/Animations/Mixamo/Jumping In Place.fbx
  Fall  → Assets/_Project/Animations/Mixamo/Falling Idle.fbx
  Land  → Assets/_Project/Animations/Mixamo/Landing.fbx
```

---

## 4. Wiring BoZo as the Humanoid Rig

BoZo's `BSMC_CharacterBase.prefab` ships with a Humanoid Mecanim rig already configured (verified in Phase 13). The Player wrapper prefab (`Assets/_Project/Prefabs/Player/Player.prefab`) nests BoZo as the **Body** child:

```
Player                       ← Player tag, CharacterController, PlayerController
└── Body                     ← BoZo source instance
    ├── Armature             ← humanoid avatar
    ├── ...meshes
    └── (Animator)           ← driven by Hearthbound_Player.controller
```

Phase 26 looks up the Animator with `GetComponentInChildren<Animator>(true)` — it doesn't matter whether the Animator lives on the root or on the `Body` child.

**Retargeting works automatically** because both BoZo and Mixamo authoring rigs are Humanoid — Unity's avatar maps the bones for us.

---

## 5. Optional NPC Animations (Doris, Gerrold, Silent-Lane)

NPCs reuse **BoZo's built-in Idle clip** plus a small set of light gestures already authored on the prefab. M1+M2 do **not** require Mixamo for NPCs — Doris stands beside her counter (BoZo idle), Gerrold sits/stands (BoZo idle). The Eyes Animator asset (A-14, already imported) drives saccades & blinks.

If you want gesture polish in M3+, add per-NPC Mixamo clips like:

| NPC      | Suggested Mixamo additions |
|----------|------------------------------|
| Doris    | `Talking.fbx`, `Hands On Hips.fbx`, `Pointing.fbx` |
| Gerrold  | `Sitting Idle.fbx`, `Sitting Disbelief.fbx`, `Standing Up.fbx` |
| Pickle   | (cat — out of scope; use the existing PickleAI tail-flick) |

These would replace the Animator Controller on each NPC prefab. For Mission 1+2, **don't bother yet** — the existing static idle is already in the polish bar.

---

## 6. Asset Store Alternatives (free + paid)

If Mixamo's download flow is inconvenient, the following Unity Asset Store packs are drop-in replacements. **None are required** — Mixamo is free and sufficient.

| Pack                                   | License | Why you'd use it |
|----------------------------------------|---------|------------------|
| **Mixamo Animation Pack (Unity Asset Store, free)** | Free | Same animations, pre-imported with Humanoid avatar already configured. Skips § 3.2 step 3. |
| **Animation Composer System (ACS) — Jorjouto** | Already imported (Tier A-15) | Layered upper-body actions over base locomotion. Use for Doris kerchief-adjust idle, Gerrold reading-letter idle. |
| **Easy Character Movement 2** | Paid (~$50) | Drop-in replacement for our `PlayerController` if we want predictive physics + stairs. Optional, not needed for the cozy slice. |
| **Synty POLYGON Starter Animations** | Free trial / paid | Stylized chibi-friendly animations. Match BoZo's silhouette well. |
| **Unity Starter Assets — Third Person Controller** | Free, official | A complete reference if we ever want to compare our controller. Worth scanning their state graph for inspiration. |

---

## 7. Performance Notes (mobile target)

- **Animator update mode:** keep at `Normal` (1× LateUpdate). Don't switch to `Animate Physics` unless we add an IK pass.
- **Apply Root Motion:** **off** for the Player (our `PlayerController` owns position). On for cutscene actors only.
- **Culling Mode:** `CullCompletely` on NPCs in the village lane — saves ~0.4 ms on mid-range Android when 6+ chibis are visible.
- **Layer count:** the Phase 26 controller is single-layer. Add a second layer (Upper Body, Mask = arms only, weight 1) when ACS gestures start landing — kept off the critical path for M1+M2.

---

## 8. Validation Checklist (for the player at pull-time)

After running Phase 26:

- [ ] `Assets/_Project/Animations/Hearthbound_Player.controller` exists.
- [ ] Open it in the Animator window — see Locomotion (default), Jump, Fall, Land states.
- [ ] Open Locomotion's blend tree — three motion fields (0=Idle, 1=Walk, 2=Run).
- [ ] Console shows the `[Hearthbound/Phase 26] Candidate …` logs — at least one candidate per role.
- [ ] Open `02_Mission01_Lane.unity`, press Play.
- [ ] WASD moves the player **relative to camera**. Hold Shift → faster. Release → smooth deceleration.
- [ ] (If you added Mixamo Jump) Press Space → small jump, returns to ground.
- [ ] Hold Right Mouse + move mouse → camera orbits the player.
- [ ] Scroll wheel → camera zooms.
- [ ] Press H → controls card appears with Sprint + Jump documented.

If any check fails, run `Hearthbound → 🔍 Diagnose Phase 23 Build` for a colorized pass/fail.

---

## 9. Design Decisions

| # | Decision | Why |
|---|----------|-----|
| D-033 | Sprint + Jump are **opt-in** runtime flags (`enableSprint`, `enableJump`). | The cozy GDD never asks for them. We add them so the controller is a complete reference, and so playtesters who reach for Shift / Space don't bounce off the "feels broken" perception. Gentle Mode disables both. |
| D-034 | Player Animator is **single 1D blend tree on `Speed`** — not a 2D tree. | Speed is a single scalar (input magnitude × sprint multiplier). A 2D tree would only help if we wanted strafe-only animations; the cozy character always faces movement direction so it's wasted work. We expose `MoveX/MoveY` parameters anyway so the upgrade path is one-Inspector-click. |
| D-035 | Animator parameter names are **fixed strings on the controller** (`Speed`, `MoveX`, …) and configurable on `PlayerController` (`animSpeedParam`, …). | Lets us swap to a community controller (Starter Assets etc.) by changing the strings on the component — no code rewrite. |
| D-036 | Camera uses our own `SmoothFollowCamera`, not Cinemachine, as the M1+M2 default. | Cinemachine is great but heavier and adds a hard package dep to every gameplay scene. Phase 17 still creates a Cinemachine prefab when the package is present — both can coexist. |
| D-037 | Animations live in **`Assets/_Project/Animations/`** (top-level project folder), Mixamo subfolder optional. | Single search path keeps Phase 26's auto-detection deterministic. Don't scatter clips under per-character folders for M1+M2. |

---

*Document version 1.0 — sealed Phase 26.*
