# 🏗️ Hearthbound Hollow — Implementation Architecture

> **Authors:** Creative Director · Senior Unity Developer · Unity Asset Engineer · Critic & Review Board
> **Branch:** `feat/mission-1-2-architecture` (off `docs/asset-analysis-mission-1-2`)
> **Status:** ⚠️ Approved with Notes — incremental phased build
> **Unity:** 6000.4.4f1 (Unity 6 LTS) · URP-Mobile target

This is the **single source of truth** for the technical implementation of Mission 1 + Mission 2. It is derived from `GAME_DESIGN.md`, the 17-codex Depth Bible, the `Mission_1_2_Focus` folder, and `Asset_Analysis_Mission1-2.md`. Every C# script, ScriptableObject, scene, and prefab in the repo maps back to a section of this document.

---

## 0. Locked Creative Decisions

| Decision | Value | Reason |
|---|---|---|
| **Villager art** | BoZo (chibi) — A-12 | Critic Board rec; matches "cozy by candlelight" pillar; lower mobile cost |
| **Dialogue** | Yarn Spinner (free, git URL) | GDD § 9 Pillar 1: "Every line could be in a novel" — AI-gen disallowed |
| **Render Pipeline** | URP-Mobile | All Tier S/A assets URP-optimized; 60 fps on mid-range Android |
| **Player controller** | Character Controller Pro (Normal state only) | Walk + interact; no jump/dash/combat |
| **Camera** | Cinemachine third-person follow + Timeline cuts for cutscenes | Standard cozy spec |
| **Save** | JSON local + 3-rolling-slot + autosave | Mobile-safe; no cloud in M1-2 |
| **OpenAI dialogue addon** | DO NOT USE | Tagged `Reference – Do Not Use In Build` |

---

## 1. Phased Delivery Plan

The build is sliced into **10 micro-phases**, each producing a buildable, mergeable, minimal-reimport delta. The user pulls each phase, lets Unity recompile (~30 s per phase), and we move on.

| Phase | Deliverable | Status |
|---|---|---|
| **0** | Architecture + folder skeleton + asmdef graph + manifest packages | 🟢 In progress |
| **1** | Core systems: GameManager, EventBus, ServiceLocator, VillageState, MissionSO | ⬜ Next |
| **2** | Memory data layer: MemoryNodeSO, MemoryConnectionSO, VillagerSO, MemoryHerb, TariffSO | ⬜ |
| **3** | Player + interactions: PlayerController, MemoryOrbInteractable, DayCycleManager | ⬜ |
| **4** | Polish + Cleanse mini-games | ⬜ |
| **5** | UI: DialogueUI, ChoiceCard, EveningLedger, TeaBrewing, Codex, ComfortTools, ToneCompass | ⬜ |
| **6** | Yarn Spinner: bridge + custom commands + Doris.yarn + Gerrold.yarn | ⬜ |
| **7** | Cutscenes: MemoryDreamSequencer + Timeline assets for Dream 1 & 2 | ⬜ |
| **8** | Save + Ripple + PickleAI | ⬜ |
| **9** | Scenes: Bootstrap, MainMenu, Mission01 (lane+Hollow), Mission02 (garden+cottage) | ⬜ |
| **10** | QA: secret-scan, unit tests, README, CHANGELOG, PR to main | ⬜ |

---

## 2. Project Folder Layout

```
/Assets/
├── _Project/                            <-- All studio-authored content
│   ├── Art/{Characters, Environment, Memories, UI}
│   ├── Audio/{Music, SFX, Ambience}
│   ├── Animations/
│   ├── Prefabs/{Player, NPCs, Memories, Props, UI, VFX}
│   ├── Scenes/
│   ├── Scripts/                         (10 asmdef-isolated subsystems)
│   ├── ScriptableObjects/{Memories, Villagers, Herbs, Missions, Tariffs, State}
│   ├── Settings/
│   ├── Yarn/
│   └── Tests/{EditMode, PlayMode}
└── ...vendor folders unchanged (BoZo, MeshingunStudio, Heat UI, etc.)
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
A single ScriptableObject with the **full 14-dimension struct** from Codex 08, even though only 4 dimensions are written in M1-2. Remaining 10 fields sit at default (the Krieg Discipline — Focus 00 § 5).

### 4.3 GameManager (bootstrap + scene management)
- Loads VillageState from disk (via SaveService) or creates default
- Registers services to ServiceLocator
- Loads scenes additively (Addressables or fallback SceneManager)
- Owns the `DontDestroyOnLoad` root

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

---

## 11. Testing Strategy

- EditMode (NUnit): VillageState math, MemoryNodeSO serialization, RippleEngine propagation, YarnBridge round-trip
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

---

## 13. Out-Of-Scope Wall (Krieg Discipline)

**Nothing below this line ships in this branch:**
- Weave / Sever / Listen / Read / Translate / Identify / Compose / Search / Negotiate / Compose Verse mini-games
- Procedural villagers (only Doris + Gerrold + 1 silent lane villager)
- Predecessor (Marin) arc — only one signed note ("— M.")
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

*Document version 1.0 — Phase 0 init.*
