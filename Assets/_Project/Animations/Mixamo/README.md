# 🎬 Hearthbound Hollow — Mixamo Animation Drop Zone

This folder is the **canonical drop zone** for Mixamo-sourced humanoid clips
that the Phase 26 / Phase 29 Animator builders auto-detect into the Player and
NPC controllers. See `Docs/ANIMATION_REQUIREMENTS.md` and
`Docs/GAME_ANIMATION_GUIDELINES.md` for the full pipeline.

## Phase 1 — Player essentials (cozy walk-only baseline)

These three FBXs make the Player blend tree complete. Currently the
`Hearthbound_Player.controller` falls back to **BoZo BMAC_M_Idle / BMAC_M_Walk**
for Idle and Walk; the Run slot falls back to Walk so sprinting visually
looks like a fast walk until `Running.fbx` is dropped here.

| File                       | Mixamo search | Notes |
|----------------------------|---------------|-------|
| `Idle.fbx`                 | "Breathing Idle" | Tick **In Place**. Loop Time = ON. |
| `Walking.fbx`              | "Walking"        | Tick **In Place**. Loop Time = ON. |
| `Running.fbx`              | "Running"        | Tick **In Place**. Loop Time = ON. |

## Phase 2 — Player optional (jump system polish)

| File                       | Mixamo search | Notes |
|----------------------------|---------------|-------|
| `Jumping In Place.fbx`     | "Jumping In Place" | Loop Time = OFF. |
| `Falling Idle.fbx`         | "Falling Idle"     | Loop Time = ON. |
| `Landing.fbx`              | "Soft Landing"     | Loop Time = OFF. |

## Phase 3 — NPC scripted beats (M1+M2)

Per `Docs/Depth_Bible/Mission_1_2_Focus/01_DORIS_THE_BAKER.md` § 5.1 and
`02_THE_WIDOWER_GERROLD.md` § 4.0:

### Doris (Cleric reskin, Female)
| File              | Mixamo search    | Used by             |
|-------------------|------------------|---------------------|
| `Female_Idle.fbx` | "Breathing Idle" | NPC Idle (F) |
| `Female_Walking.fbx` | "Female Walk"  | NPC Walk (F) |
| `Talking.fbx`     | "Talking"        | NPC Talking |
| `Kneading.fbx`    | "Stir Pot"       | OfferBox / Kneading beats (loop) |
| `Pointing.fbx`    | "Pointing"       | OfferBox beat hand-off |

### Gerrold (Bard reskin, Male)
| File                   | Mixamo search       | Used by |
|------------------------|---------------------|---------|
| `Male_Idle.fbx`        | "Breathing Idle"    | NPC Idle (M) |
| `Male_Walking.fbx`     | "Walking"           | NPC Walk (M) |
| `Sitting Idle.fbx`     | "Sitting Idle"      | SitDown / SittingIdle |
| `Standing Up.fbx`      | "Standing Up"       | StandUp beat |
| `Sitting Disbelief.fbx`| "Sitting Disbelief" | Disbelief beat (Crossed Core) |

### Silent Lane Villager (Warrior reskin, Male, old)
| File              | Mixamo search    | Used by |
|-------------------|------------------|---------|
| `Reading.fbx`     | "Sitting Read Book" | Reading beat on the bench |
| `Wave.fbx`        | "Waving"            | Wave beat |

## Pipeline

1. Download each FBX with Mixamo's **In Place** + **FBX For Unity** options.
2. Drop into this folder.
3. Select each FBX → Inspector → Rig tab → **Animation Type = Humanoid**,
   **Avatar Definition = Create From This Model** → Apply.
4. Animation tab → tick Loop Time where the table above says ON.
5. Run **Hearthbound → Phase 26 — Build Player Animator Controller** and
   **Hearthbound → Phase 29 — Build NPC Animator Controllers** to wire the
   new clips into the AnimatorController state machines.

Re-running the builders is idempotent — the assets are overwritten in place
so prefab references stay valid.

— *Phase 29 (2026-05-26) — Hearthbound Hollow senior team.*
