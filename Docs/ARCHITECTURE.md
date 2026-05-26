# 🏗️ Hearthbound Hollow — Implementation Architecture

> **Authors:** Creative Director · Senior Unity Developer · Unity Asset Engineer · Character Animator · Critic & Review Board
> **Branch:** `feat/mission-1-2-architecture` (off `docs/asset-analysis-mission-1-2`)
> **Status:** ⚠️ Approved with Notes — incremental phased build
> **Unity:** 6000.4.4f1 (Unity 6 LTS) · URP-Mobile target

This is the **single source of truth** for the technical implementation of Mission 1 + Mission 2. It is derived from `GAME_DESIGN.md`, the 17-codex Depth Bible, the `Mission_1_2_Focus` folder, `Asset_Analysis_Mission1-2.md`, and `ANIMATION_REQUIREMENTS.md`. Every C# script, ScriptableObject, scene, and prefab in the repo maps back to a section of this document.

---

## 0. Locked Creative Decisions

| Decision | Value | Reason |
|---|---|---|
| **Villager art** | BoZo (chibi) — A-12 | Critic Board rec; matches "cozy by candlelight" pillar; lower mobile cost |
| **Dialogue** | Yarn Spinner (free, git URL) | GDD § 9 Pillar 1: "Every line could be in a novel" — AI-gen disallowed |
| **Render Pipeline** | URP-Mobile | All Tier S/A assets URP-optimized; 60 fps on mid-range Android |
| **Player controller** | Custom `PlayerController` (Phase 26) — CharacterController-based, WASD, optional Sprint + Jump, camera-relative, Animator-aware | Character Controller Pro (A-13) is imported as reference but **not** wired — our controller is leaner + Mixamo-ready (D-036/D-037) |
| **Camera** | `SmoothFollowCamera` (Phase 26) — spring-damped follow + RMB-orbit + scroll-zoom + wall-clip; Cinemachine prefab optional via Phase 17 | Cinemachine adds a hard package dep to every gameplay scene; our camera ships cinematic-feel cozy default and both can coexist (D-039) |
| **Player Animator** | Procedurally-built `Hearthbound_Player.controller` (Phase 26) — Humanoid avatar, 1D blend tree on `Speed`, Jump/Fall/Land states | Mixamo-retargetable; BoZo's existing M_Idle/M_Walk are the fallback (D-037) |
| **Save** | JSON local + 3-rolling-slot + autosave | Mobile-safe; no cloud in M1-2 |
| **OpenAI dialogue addon** | DO NOT USE | Tagged `Reference – Do Not Use In Build` |
| **Editor entry point** (Phase 32) | **`Hearthbound → 🚀 Build Everything`** — single top-level click, chains every Phase 13 → 32 sub-builder, idempotent | **D-051** — top-level Hearthbound menu reserved for `🚀 Build Everything`, `🔍 Diagnose Build`, and the implicit `⚙️ Advanced ►` submenu (every legacy per-phase entry). |

---

## 1. Phased Delivery Plan

> 🚀 **User-facing entry point (Phase 32).** The user-facing entry point is **`Hearthbound → 🚀 Build Everything`**. All per-phase menu items have been moved to **`Hearthbound → ⚙️ Advanced/…`** for power users. After every `git pull`, press **🚀 Build Everything** — it is idempotent (every step uses load-or-create + heal-then-save; Phase 12 / 22 / 24 scene capstones rebuild scenes 00-05 by design — Inspector tweaks belong on prefabs, not scenes). A one-line `EditorUtility.DisplayDialog` confirms before the chain runs. The second top-level entry, **`Hearthbound → 🔍 Diagnose Build`**, runs the Phase 33 aggregate diagnostic (read-only) and is safe to invoke at any time. See `Docs/PROGRESS.md → Phase 32 — Menu collapse + idempotency audit` for the full migration table and audit results.

The build is sliced into micro-phases, each producing a buildable, mergeable, minimal-reimport delta.

| Phase | Deliverable | Status |
|---|---|---|
| **0** | Architecture + folder skeleton + asmdef graph + manifest packages | ✅ Done |
| **1** | Core systems: GameManager, EventBus, ServiceLocator, VillageState, MissionSO | ✅ |
| **2** | Memory data layer: MemoryNodeSO, MemoryConnectionSO, VillagerSO, MemoryHerb, TariffSO | ✅ |
| **3** | Player + interactions: PlayerController, MemoryOrbInteractable, DayCycleManager | ✅ |
| **4** | Polish + Cleanse mini-games | ✅ |
| **5** | UI: DialogueUI, ChoiceCard, EveningLedger, TeaBrewing, Codex, ComfortTools, ToneCompass | ✅ |
| **6** | Yarn Spinner: bridge + custom commands + Doris.yarn + Gerrold.yarn | ✅ |
| **7** | Cutscenes: MemoryDreamSequencer + Timeline assets for Dream 1 & 2 | ✅ |
| **8** | Save + Ripple + PickleAI | ✅ |
| **9** | Scenes: Bootstrap, MainMenu, Mission01 (lane+Hollow), Mission02 (garden+cottage) | ✅ |
| **10–12** | URP, shader patcher, MVP smoke-test | ✅ |
| **13–21** | Asset-driven builders (BoZo, Bamao, MeshingunStudio, AllIn1, Lumen, audio, weather, Yarn, dreams) | ✅ |
| **22** | Polished Playable Mission 1 (engineering capstone) | ✅ |
| **23** | Mission 1 Polish Capstone | ✅ |
| **24** | Mission 2 Garden + Cottage scenes | ✅ |
| **25** | UI activation hotfix — two-layer wiring + self-heal Show() in every overlay | ✅ |
| **26 (Narrative Hooks)** | Marin's Note interactable + Phase26_NarrativeHooks editor menu | ✅ |
| **26.1** | Asmdef-locality hotfix — MarinNoteInteractable moved to Mission asmdef | ✅ |
| **26 (Player Controller + Animation)** | PlayerController WASD/Sprint/Jump + SmoothFollowCamera + Mixamo-ready Animator | ✅ |
| **27** | Master `Build EVERYTHING` capstone + Phase 26 polish layer (NPC animators, narrative hooks, diagnostic, footstep hooks) | ✅ |
| **27.1 / 27.2 / 28 / 29b** | "Half body in floor" fix progression — culminating in PlayerGroundClamp (live world bounds + continuous correction window) + Player Rig Doctor foot-bone anchor | ✅ |
| **29a** | UIAutoFitText on every TMP label + DialogueBox ChoicesContainer relocation | ✅ |
| **30** | OnboardingOverlay (6-step) + ControlHintsHUD (context-aware chips) | ✅ |
| **30.1** | Mission asmdef `Unity.TextMeshPro` reference hotfix | ✅ |
| **31** | Dialogue Choice Card Repair — full-width tiles + 1/2/3/4 keyboard shortcuts | ✅ |
| **31.1** | "Press [Space] ▸" advance prompt + DreamCanvas auto-hide | ✅ |
| **32 (Mission 1 polish v2)** | 8-cottage village + Hollow facade + hearth dressing + cozy URP volumes | ✅ |
| **32 (Menu collapse UX track)** | **`🚀 Build Everything` is the only top-level entry; safety dialog + idempotency audit + D-051** | ✅ **This PR** |
| **33** | Aggregate `Diagnose Build` — chains Phase 23/26/32 sub-diagnostics under one top-level read-only audit | ✅ |
| **QA** | secret-scan, unit tests, README, CHANGELOG, PR to main | 🟡 In progress |

---

## 2. Project Folder Layout

```
/Assets/
├── _Project/                            <-- All studio-authored content
│   ├── Art/{Characters, Environment, Memories, UI}
│   ├── Audio/{Music, SFX, Ambience}
│   ├── Animations/                       (Hearthbound_Player.controller + Mixamo/* subfolder)
│   ├── Prefabs/{Player, NPCs, Memories, Props, UI, VFX}
│   ├── Scenes/                           (6 scenes — Bootstrap → Cottage)
│   ├── Scripts/                          (10 asmdef-isolated subsystems)
│   ├── ScriptableObjects/{Memories, Villagers, Herbs, Missions, Tariffs, State}
│   ├── Settings/                         (HearthboundInput.inputactions — Phase 26 adds Sprint/Jump/CameraLook/CameraZoom/AllowLook)
│   ├── Yarn/
│   └── Tests/{EditMode, PlayMode}
└── ...vendor folders unchanged (BoZo, MeshingunStudio, Heat UI, Lumen, Bamao, etc.)
```

> **Why we don't relocate existing imports:** Moving them would break every `.meta` GUID reference, forcing Unity to reimport ~5 GB of textures.

---

## 3. Assembly Definitions — incremental compile graph

```
HearthboundHollow.Core
HearthboundHollow.Memory       ← Core
HearthboundHollow.Save         ← Core, Memory, Newtonsoft-Json
HearthboundHollow.Audio        ← Core
HearthboundHollow.Player       ← Core, Memory, InputSystem
HearthboundHollow.MiniGames    ← Core, Memory, Audio, InputSystem, TMP
HearthboundHollow.UI           ← Core, Memory, TMP, InputSystem
HearthboundHollow.Dialogue     ← Core, Memory, UI, TMP, [YarnSpinner if present]
HearthboundHollow.Cutscene     ← Core, Memory, UI, Timeline
HearthboundHollow.Mission      ← Core, Memory, UI, Dialogue, MiniGames, Cutscene, Save, Audio, Addressables
HearthboundHollow.Editor       ← every runtime asmdef (Editor-only, includePlatforms = ["Editor"])
HearthboundHollow.Tests.EditMode ← Core, Memory, Save, Mission, Player (Phase 26 adds Player ref)
```

**Benefit:** Editing a UI script recompiles only HearthboundHollow.UI + downstream. Saves ~80% of iteration time.

---

## 4. Core Subsystems

### 4.1 Service Locator + Event Bus
Replaces traditional singletons. `ServiceLocator.Get<T>()` for service access. `EventBus.Publish<TEvent>(evt)` for decoupled communication.

**Events shipping in M1-2:**
- `MissionStartedEvent` / `MissionCompletedEvent`
- `DialogueStartedEvent` / `DialogueEndedEvent`
- `MemoryPolishedEvent(MemoryNodeSO, clarity01)`
- `MemoryCleansedEvent(MemoryNodeSO, CleanseOutcome)`
- `MoralChoiceMadeEvent(MoralChoice, MemoryNodeSO)`
- `HerbHarvestedEvent` / `TeaBrewedEvent`
- `DayEndedEvent(int dayIndex)`
- `EchoConnectionRevealedEvent(MemoryNodeSO, MemoryNodeSO)`

### 4.2 VillageState (the global game state)
A single ScriptableObject with the **full 14-dimension struct** from Codex 08, even though only 4 dimensions are written in M1-2.

### 4.3 GameManager (bootstrap + scene management)
- Loads VillageState from disk (via SaveService) or creates default
- Registers services to ServiceLocator
- Loads scenes additively (Addressables or fallback SceneManager)
- Owns the `DontDestroyOnLoad` root

### 4.4 PlayerController (Phase 26 surface)

Public state (read-only):
- `MovementLocked` — set by dialogue / mini-game runners. `true` = WASD ignored, gravity still applies.
- `CurrentFocus` — closest `Interactable` in front of the player.
- `CurrentMoveInput` / `CurrentVelocity` — debug/diagnostic.
- `IsGrounded`, `IsSprinting` — Animator-bridge inputs.

Toggleables (Inspector):
- `enableSprint` — Shift / LStick-click modifier.
- `enableJump` — Space / Gamepad south.
- Both are **runtime-gated by `SettingsService.GentleMode`** so Gentle players never accidentally sprint or fall.

Animator parameter contract:
- `Speed` (float 0..2) — primary blend (idle/walk/run).
- `MoveX`, `MoveY` (floats -1..1) — for future 2D-strafe Animator upgrade.
- `VelocityY` (float) — drives Jump → Fall transition.
- `IsGrounded` (bool), `IsSprinting` (bool), `Jump` (trigger).

### 4.5 SmoothFollowCamera (Phase 26)

- `SmoothDamp` position with configurable smooth time.
- Spherical orbit (yaw unlimited, pitch clamped to `[pitchMin, pitchMax]`).
- Mouse-look gated by RMB (or `AllowLook` action). Scroll zoom.
- Sphere-cast wall-clip with adjustable radius + mask.
- Cinemachine-agnostic — no package dep.

---

## 5. Memory Data Model (Codex 02 § 2.1)

Every Memory is a `MemoryNodeSO`. The Echo Web `MemoryConnectionSO` schema exists from Day 1 — only one connection is revealed in M1-2 (Doris's "First Loaves" ↔ Gerrold's memory of Doris in the kitchen).

---

## 6. Mini-Game Architecture

### 6.1 Polish (Mission 1)
- States: `Idle → Engaging → Polishing → MidwayMilestone → RevealSwell → Complete`
- Cursor velocity drives `clarity += k * dt * coverageQuadrant`
- Shader Graph `MemoryOrb_Master` exposes `_Clarity`, `_CrackIntensity`, `_DissolveProgress`, `_GlowStrength`, `_PaletteTint`
- **Cannot fail** (M1 tutorial-tier)
- Auto-Complete button always visible (Codex 06)

### 6.2 Cleanse (Mission 2)
- States: `Idle → ThreadEngaged → SealingCrack → CrackComplete → AwaitingNextCrack → Complete | CrossedCore`
- Cursor must stay within crack tolerance (default 0.08 normalized)
- Core region (amber radius 0.25 from orb center) triggers `CoreWarning → CoreDamage`
- 4 outcomes: `Perfect | Acceptable | Sloppy | CrossedCore` (Focus 04 § 3.5)

Both inherit from `MiniGameBase` so future Weave/Sever just subclass.

---

## 7. Dialogue Architecture (Yarn Spinner)

- `YarnVillageStateBridge` exposes `$trust_doris`, `$trust_gerrold`, `$memory_integrity_gerrold`, `$tea_brewed`, `$cleanse_quality`, `$choice_made` as Yarn variables wired to VillageState
- `YarnCustomCommands` — 14 commands (Focus 07 § 2.3): `<<polish_orb>>`, `<<cleanse_orb>>`, `<<offer_choice>>`, `<<eyes_look_at>>`, `<<pickle_say>>`, `<<lights_warm>>`, `<<save_autopoint>>`, `<<echo_reveal>>`, `<<play_cutscene>>`, etc.
- Yarn line view renders into `Bamao_ParchmentBox.prefab`

---

## 8. UI Architecture (Heat + Bamao)

| Panel | Source |
|---|---|
| Main Menu / Loading / Settings / Pause / HUD | Heat reskinned with "Hearthbound" warm-parchment preset |
| Dialogue Box / Choice Card / Evening Ledger / Codex Tooltip | Bamao parchment & book frames |
| Memory Map | Custom (Bamao bg + BoZo node sprites) |
| Tea Brewing | Custom (Bamao kettle UI) |
| Tone Compass | Custom card on Bamao parchment |
| Comfort Tools | Heat / Settings modal with custom tab |

---

## 9. Save Architecture

- **Autosave** (single rolling slot, write-on-DayEnd + on-MoralChoice) + **Manual** (3 rolling slots from Evening Ledger)
- Payload: `VillageStateSnapshot` (JSON) — mirror of VillageState + per-mission flags + last-scene index
- Path: `Application.persistentDataPath/saves/`
- **Atomic write**: tmp file → fsync → rename. Survives power-fail (Focus 07 § 4.5).

---

## 10. Mobile Performance Discipline

- URP Mobile RP: 1 directional, MSAA 2×, render scale 0.85, soft shadows OFF by default
- Lightmap Fusion Pro: 4 baked sets (morning/afternoon/evening/night) — bake Phase 9
- Memory orb: single master material + GPU instancing
- Object pooling: `MemoryOrbPool`, `VfxPool`, `SfxPool`
- Texture compression: ASTC 6×6 mobile, ETC2 fallback
- Profile gate: every Phase 4+ PR must pass ≤ 16 ms on mid-range Android proxy
- **Animator**: Player Animator runs in `Normal` mode (1× LateUpdate); Apply Root Motion = false; NPCs use `CullCompletely` mode in the village lane to save ~0.4 ms when 6+ chibis are visible.

---

## 11. Testing Strategy

- EditMode (NUnit): VillageState math, MemoryNodeSO serialization, RippleEngine propagation, YarnBridge round-trip, **PlayerController + SmoothFollowCamera public surface (Phase 26)**
- PlayMode: Polish completion path, Cleanse outcome state machine
- Smoke scene: boots GameManager → loads state → spawns Doris → triggers Polish → asserts no NRE

---

## 12. Risk Mitigations (Krieg's Register)

| Risk | Mitigation in this architecture |
|---|---|
| R-2 Orb shader perf | Single master shader + GPU instancing + `MemoryOrb_StaticFallback` for low-quality tier |
| R-3 Cleanse too hard | Adaptive difficulty SO; Gentle Mode preset; always-visible Auto-Complete |
| R-6 First-launch >5 min | Tone Compass skippable from frame 1; Gentle Mode 4 s timeout |
| Save corruption | Atomic write + fsync + rename; 3 rolling slots |
| Scaling rework | All 14 VillageState dimensions + Echo Web + Vignette schema present at default values |
| **Controller perception** | **Sprint + Jump available but off in Gentle Mode (D-036)** — playtester who reaches for Shift/Space doesn't bounce off a "broken" controller |
| **Mixamo unavailable** | **Phase 26 falls back to BoZo's existing 2 anims (Idle/Walk) and the AnimatorController degrades gracefully** — game ships polished without any Mixamo downloads |
| **Editor menu archaeology** (Phase 32) | **`🚀 Build Everything` is the only entry the user needs after every pull (D-051). Power users have full per-phase access under `⚙️ Advanced ►`. Safety confirmation dialog prevents accidental ~60 s rebuild.** |

---

## 13. Out-Of-Scope Wall (Krieg Discipline)

**Nothing below this line ships in this branch:**
- Weave / Sever / Listen / Read / Translate / Identify / Compose / Search / Negotiate / Compose Verse mini-games
- Procedural villagers (only Doris + Gerrold + 1 silent lane villager)
- Predecessor (Marin) full arc — only Marin's signed note + workbench note exist
- Echo Hologram, Locked Room, Forgotten Year, Vance Arc, Mariska, Memory Bees, Composting, Borrowed Memory
- Letter-Bird Network, Pen-Pal Villages, Dream Cinema community, ARG, Photo Mode
- Hollow upgrades beyond Level 1
- Herbs beyond Lavender + Valerian
- Mobile IAP, gacha, energy, lootboxes (NEVER)

The codices 02–16 in `/Docs/Depth_Bible/` remain the **Scaling Reference** for after the 20-person playtest greenlight.

---

## 14. How This Doc Stays Honest

Every PR to this branch updates `Docs/PROGRESS.md` with:
- Phase + sub-task completed
- Exact files added/modified
- Open follow-ups
- Any deviation from this Architecture doc, with rationale

**No code lands without a PROGRESS.md update.**

---

## 15. Decisions Index (cross-ref → PROGRESS.md)

D-001 → D-051 are catalogued in `Docs/PROGRESS.md`. Newest:

- **D-033** *(Phase 25 hotfix)* Procedural UI builders MUST use the two-layer pattern.
- **D-034** *(Phase 25 hotfix)* UI overlay scripts MUST self-heal in `Show()`.
- **D-035** *(Phase 26.1 hotfix)* Asmdef-locality check — every new runtime script must live in an asmdef that declares the dependencies it uses.
- **D-036** *(Phase 26 Player Controller + Animation)* Sprint + Jump are opt-in runtime flags on PlayerController. Gentle Mode disables both.
- **D-037** *(Phase 26 Player Controller + Animation)* Player Animator is a single 1D blend tree on Speed (0/1/2 = Idle/Walk/Run).
- **D-038** *(Phase 26 Player Controller + Animation)* Animator parameter names are configurable strings on PlayerController.
- **D-039** *(Phase 26 Player Controller + Animation)* SmoothFollowCamera is the M1+M2 default; Cinemachine prefab from Phase 17 coexists.
- **D-040** *(Phase 26 Player Controller + Animation)* Animations live in Assets/_Project/Animations/ (Mixamo subfolder optional).
- **D-041** *(Phase 27.1 / Phase 28)* Mesh-bottom from world-space `Renderer.bounds`, never padded `SkinnedMeshRenderer.localBounds`. Runtime `PlayerGroundClamp` is the truth.
- **D-042** *(Phase 29a)* Every TMP label created by a builder MUST go through `UIAutoFitText`.
- **D-043** *(Phase 30)* Onboarding is per-save, not per-play.
- **D-044** *(Phase 30)* Context-aware HUD chips live in the Mission asmdef.
- **D-045 → D-048** *(Phase 31)* Dialogue VLG + LayoutElement + lineText-hide + number-key shortcut contracts.
- **D-049** *(Phase 31.1)* Blocking dialogue UI must expose a visible advance affordance.
- **D-050** *(Phase 31.1)* Cutscene overlays hidden-by-default; full-screen non-active raycasters zero `raycastTarget`.
- **D-051** *(Phase 32 UX track — Menu collapse)* **Every editor action MUST register under `Hearthbound/⚙️ Advanced/…` unless explicitly promoted to top level. The top-level menu is reserved for the three blessed user entry points: `🚀 Build Everything`, `🔍 Diagnose Build`, and the implicit `⚙️ Advanced ►` submenu. Promotion to top level requires Critic & Review Board sign-off.**

---

*Document version 1.4 — Phase 32 menu collapse + D-051 + extended phase table through Phase 33.*
