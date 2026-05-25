# Changelog — Hearthbound Hollow

All notable changes to this project will be documented here. Entries follow the [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) format.

## [0.5.1-dialogue-choice-card-repair] — 2026-05-25

**Branch:** `feat/mission-1-2-architecture` (accumulating on top of 0.5.0)
**Theme:** Phase 31 — fix the dialogue choice tiles so the game is playable past Doris's first greeting.

### User report (with screenshot)

> *"the game stuck during the dialogue and as shown in screenshots the cards
> not appear well so please fix this issue and enhance the gameplay to make
> the game playable"*

The screenshot showed Doris saying "I'd heard. I just didn't expect... so soon."
with two choice tiles rendered as **tiny ~100 px squares** on the right of the
parchment box, each label broken into one-word-per-line shards ("I'm | here |
to | help"). Players could not realistically click them → dialogue stalled.

### Root cause

`Phase14_BamaoUIBuilder` saved `UI_DialogueBox_Bamao.prefab` with
`ChoicesContainer.VerticalLayoutGroup.childControlWidth = false`. With this
flag off, `childForceExpandWidth = true` only redistributes leftover space —
it does **not** resize children. The choice tile prefab is saved with
`sizeDelta = (100, 100)`, so every instantiated tile rendered at 100 × 100 in
the centre of the body. Compounded by missing `LayoutElement.preferredWidth`
on the tile and `lineText` never being hidden while choices were on screen.

### Phase 31 — Dialogue Choice Card Repair

- **`UI/DialogueChoiceLayoutHealer.cs`** (NEW, ~120 LOC) — Runtime self-heal
  helper. `HealContainer(Transform)` enforces `childControlWidth = true /
  childControlHeight = true / childForceExpandWidth = true /
  childAlignment = UpperCenter / padding = (16,16,10,10) / spacing ≥ 8` on
  the VLG. `HealTile(GameObject)` resets the tile's `RectTransform.anchorMin.x
  = 0 / anchorMax.x = 1 / sizeDelta.x = 0` (defeats the saved 100×100),
  ensures `LayoutElement.minHeight = 56 / preferredHeight = 64 /
  flexibleWidth = 1`, and heals every TMP label under the tile
  (word-wrap + auto-size + ellipsis + full-rect anchors). Defensive: also
  re-enables button `interactable` + `targetGraphic.raycastTarget`.
  Idempotent.
- **`UI/DialogueUI.cs`** — `Awake()` heals the container. `PresentChoices()`
  heals again + heals every clone + **hides `lineText`** while choices are
  visible. `PresentLine()` / `HandleChoice()` / `Hide()` restore `lineText`.
  New `Update()` handler maps **`1`/`2`/`3`/`4` keys** to choice indices.
  Choice labels are prefixed with `<b>[1]</b>` etc. so the shortcut is
  discoverable.
- **`UI/ChoiceCardUI.cs`** — Same heal calls on the moral-choice (tariff)
  card. Identical root cause.
- **`Editor/Phase14_BamaoUIBuilder.cs`** — Fresh builds bake the correct
  settings: `childControlWidth = true`, `childAlignment = UpperCenter`, tile
  pre-shaped to full-width, `LayoutElement.minHeight = 56 / preferredHeight
  = 64 / flexibleWidth = 1`.
- **`Editor/Phase31_DialogueChoiceCardRepair.cs`** (NEW, ~340 LOC) —
  `Hearthbound → 🧰 Phase 31 — Repair Dialogue Choice Cards`. Walks the two
  saved UI prefabs + four gameplay scenes; surgically repairs the VLG +
  LayoutElement + label settings IN PLACE. Idempotent, opens each scene in
  single-mode, marks dirty, saves. Reports per-asset whether a change was
  needed.
- **`Editor/Phase27_BuildEverything.cs`** — Master capstone now chains Phase
  31 after Phase 30. `Hearthbound → ✨ Build EVERYTHING (Phase 27 — one
  click)` is now a 7-phase chain.

**D-045 (NEW):** VLG / HLG with variable-width children MUST set both
`childControlWidth = true` AND `childForceExpandWidth = true`.
**D-046 (NEW):** Choice tile prefabs MUST set `LayoutElement.minHeight ≥ 56
/ preferredHeight ≥ 64 / flexibleWidth = 1`.
**D-047 (NEW):** `DialogueUI` MUST hide `lineText` while choices are
on-screen and restore it on the next presentation.
**D-048 (NEW):** Player-facing UI with ≤4 buttons should expose 1/2/3/4
keyboard shortcuts and prefix labels with `[N]`.

---

## [0.5.0-onboarding-hints-and-rig-doctor] — 2026-05-25

**Branch:** `feat/mission-1-2-architecture` → PR #7 (open, accumulating)
**Theme:** Phase 28 / 29 / 30 trifecta. Definitive body alignment + UI-never-clips + Onboarding & Control Hints.

After the 0.4.0 first-playtest, the user reported three issues:
1. *"Half of the body is in the floor"* — even after the Phase 27.1 / 27.2 fixes.
2. *"The cards and UI is not appearing well and text is cut."*
3. *"Create the onboarding and control guidance UI."*

This release closes all three with three thematic phases pushed one-commit-per-concern.

### Phase 28 — Definitive body alignment (live world bounds + continuous correction window)

- **`Player/PlayerGroundClamp.cs`** (rewritten) — Switched from `SkinnedMeshRenderer.localBounds` (often a padded culling AABB) to live `Renderer.bounds.min.y` (current pose after Animator update). New `continuousDuration` (0.75 s default) re-aligns every frame during the Animator's bind→idle settle. Optional `footAnchor` Transform override for surgical anchoring on rigs with weird padded bounds. Filters out ParticleSystemRenderer / LineRenderer / TrailRenderer (huge AABBs would corrupt min-Y).
- **`Player/PlayerController.cs`** — Embedded clamp matches PlayerGroundClamp algorithm verbatim. `intrinsicAlignContinuousDuration` + `bodyAlignEpsilon` Inspector knobs. Tolerance guard prevents FP chatter.

**D-041 amendment:** Mesh-bottom MUST be measured from world-space `Renderer.bounds`, never `SkinnedMeshRenderer.localBounds`.

### Phase 29a — UI Polish (text never gets clipped)

- **`UI/UIAutoFitText.cs`** (NEW, +173 LOC) — Single MonoBehaviour helper + two static methods (`ApplyToLabel`, `ApplyToButtonLabel`) that force `enableWordWrapping = true` + `enableAutoSizing = true` between configured min/max + `OverflowMode.Ellipsis` fallback. Designed to be called from any builder script or attached defensively in `Awake()`.
- **DialogueBox ChoicesContainer relocated** — Phase 14 was anchoring it at `anchorY = 1.05..2.10` (i.e. **above** the prefab bounds). The scene builder was supposed to reposition it but in practice the anchors stuck and choice tiles rendered off-screen. Now anchored INSIDE the dialogue box at `(0.22-0.96, 0.08-0.78)`.
- **Defensive Awake() autofit pass** on every UI script: `DialogueUI`, `ChoiceCardUI` (+ per-tile `WireTile` autofit), `EveningLedgerUI`, `HelpOverlayUI`, `ToneCompassCard`.
- **Editor builder autofit** in `Phase14_BamaoUIBuilder` (DialogueBox / ChoiceTile / EveningLedger / TooltipFrame) and `Phase23_Mission1PolishCapstone` (PauseMenu / HelpOverlay / Settings / MissionTitleCard / buttons / toggles / sliders).

**D-042 (NEW):** Any TMP label created by a builder script MUST go through `UIAutoFitText.ApplyToLabel / ApplyToButtonLabel` before the prefab is saved.

### Phase 29b — Player Rig Doctor

- **`Editor/Phase29_PlayerRigDoctor.cs`** (NEW) — Auto-discovers a foot bone on the Player rig using Mixamo / BoZo / generic humanoid naming heuristics. Wires it as `PlayerGroundClamp.footAnchor`, which the clamp prefers over bounds scanning. Surgical anchor at the actual toe position.
- Also: force-disables `applyRootMotion` on every Player Animator, verifies the Humanoid avatar, sanity-checks the GameObject scale chain, and auto-adds a Ground BoxCollider if the scene lacks one.

### Phase 30 — Onboarding overlay + Control Hints HUD

- **`UI/OnboardingOverlay.cs`** (NEW, ~350 LOC) — 6-step multi-card walkthrough that runs once per save on the Lane scene. Welcome → WASD → E → LMB polish → Esc/H comfort → "You're ready". Data-driven `Step[]`, optional `expects` input expectations (`press_wasd` etc.) auto-advance steps when satisfied. Skippable from frame 1 (Esc / Skip Tutorial button). Pauses `Time.timeScale = 0` while open; resumes on completion or skip. Uses `unscaledDeltaTime` for input polling.
- **`Mission/ControlHintsHUD.cs`** (NEW, ~155 LOC, in Mission asmdef per D-035) — Always-visible parchment chip strip (Move · Interact · Help) at the bottom-left of every gameplay scene. The [E] chip emphasises to full alpha + swaps caption to the interactable's `PromptText` when one is in range; idles at `alpha = 0.45` otherwise.
- **`Editor/Phase30_OnboardingAndHintsCapstone.cs`** (NEW, ~380 LOC) — Idempotent Editor builder. Drops OnboardingOverlay on the Lane canvas + ControlHintsHUD on every gameplay scene canvas. Two-layer pattern (host always active, Visual child toggled) for both. Uses UIAutoFitText on every label.
- **`Core/VillageState.cs`** — Added `onboardingCompleted` bool. `ResetToDefault()` clears it so fresh saves see the walkthrough.

**D-043 (NEW):** Onboarding is per-save (gated by `VillageState.onboardingCompleted`), not per-play.
**D-044 (NEW):** Context-aware HUD chips live in the Mission asmdef (not UI), because they query `PlayerController.CurrentFocus`.

### Master capstone extended

- **`Editor/Phase27_BuildEverything.cs`** — Now chains six sub-capstones (was four): adds Phase 29 (Player Rig Doctor) and Phase 30 (Onboarding + Hints) to the reflection-driven sequence. Total run time still ≈60 s; missing phases skip gracefully.

### Files added / changed

| File | Status | LOC | Phase |
|---|---|---|---|
| `Assets/_Project/Scripts/Player/PlayerGroundClamp.cs` | Rewritten | 352 | 28 |
| `Assets/_Project/Scripts/Player/PlayerController.cs` | Updated | 660 | 28 |
| `Assets/_Project/Scripts/UI/UIAutoFitText.cs` | **NEW** | 173 | 29a |
| `Assets/_Project/Scripts/UI/DialogueUI.cs` | Updated | (autofit) | 29a |
| `Assets/_Project/Scripts/UI/ChoiceCardUI.cs` | Updated | (autofit) | 29a |
| `Assets/_Project/Scripts/UI/EveningLedgerUI.cs` | Updated | (autofit) | 29a |
| `Assets/_Project/Scripts/UI/HelpOverlayUI.cs` | Updated | (autofit) | 29a |
| `Assets/_Project/Scripts/UI/ToneCompassCard.cs` | Updated | (autofit) | 29a |
| `Assets/_Project/Scripts/Editor/Phase14_BamaoUIBuilder.cs` | Updated | (autofit + ChoicesContainer reposition) | 29a |
| `Assets/_Project/Scripts/Editor/Phase23_Mission1PolishCapstone.cs` | Updated | (autofit pass) | 29a |
| `Assets/_Project/Scripts/Editor/Phase29_PlayerRigDoctor.cs` | **NEW** | ~750 | 29b |
| `Assets/_Project/Scripts/UI/OnboardingOverlay.cs` | **NEW** | 350 | 30 |
| `Assets/_Project/Scripts/Mission/ControlHintsHUD.cs` | **NEW** | 155 | 30 |
| `Assets/_Project/Scripts/Editor/Phase30_OnboardingAndHintsCapstone.cs` | **NEW** | 380 | 30 |
| `Assets/_Project/Scripts/Core/VillageState.cs` | Updated (+`onboardingCompleted`) | — | 30 |
| `Assets/_Project/Scripts/Editor/Phase27_BuildEverything.cs` | Updated (+chains 29 & 30) | — | 27 |
| `Docs/PROGRESS.md` | Updated | — | doc |
| `README.md` | Updated | — | doc |

### Decisions

| # | Decision | Phase |
|---|---|---|
| D-041 (amendment) | Mesh-bottom MUST be measured from world-space `Renderer.bounds`, never `SkinnedMeshRenderer.localBounds`. | 28 |
| D-042 | Any TMP label created by a builder script MUST go through `UIAutoFitText.ApplyToLabel / ApplyToButtonLabel`. | 29 |
| D-043 | Onboarding is per-save (`VillageState.onboardingCompleted` flag). | 30 |
| D-044 | Context-aware HUD chips live in `Mission` asmdef (not UI) because they query `PlayerController`. | 30 |

---

## [0.4.0-build-everything-and-npc-animator] — 2026-05-25

**Branch:** `feat/mission-1-2-architecture` → PR #7 (open)
**Theme:** Phase 27 — single-click master capstone + Phase 26 NPC animator pipeline + Phase 26 diagnostic + Phase 26 footstep SFX hooks.

This is the **packaging + polish release** on top of v0.3.0. No new design decisions (D-numbered) — every addition follows D-001..D-040.

### Added — Editor tools (≈640 LOC)

- **`Editor/Phase27_BuildEverything.cs`** (200 LOC) — Master capstone. Reflection-driven chain of every Phase 23/26 sub-capstone so missing phases skip gracefully:
  1. Phase 23 — POLISHED Mission 1 + 2 (chains 13..24 internally)
  2. Phase 26 — Player Controller + Animation
  3. Phase 26 — NPC Animators (NEW this release)
  4. Phase 26 — Narrative Hooks (parallel thread, optional)
  5. Opens `00_Bootstrap.unity`
  Menu: `Hearthbound → ✨ Build EVERYTHING (Phase 27 — one click)`.
- **`Editor/Phase26_DiagnosticReport.cs`** (180 LOC) — Read-only audit of Phase 26 wiring. Checks: AnimatorController asset (7 params + 4 states + default Locomotion), Player prefab Animator wiring (Apply Root Motion = false), each gameplay scene (Player tag + CharacterController + PlayerController + SmoothFollowCamera target + cameraReference + Main Camera tag). Menu: `Hearthbound → 🔍 Diagnose Phase 26 Build`.
- **`Editor/NpcAnimatorControllerBuilder.cs`** (130 LOC) — Builds `Assets/_Project/Animations/Hearthbound_NPC.controller`:
  - 2 parameters: `IsTalking` (bool), `Speed` (float — reserved)
  - 2 states: Idle (default) ↔ Talking, soft 0.18 s / 0.22 s transitions
  - Auto-picks BoZo F_Idle (Doris is reskinned F per Phase 13); Talking reuses Idle until a Mixamo `Talking.fbx` is dropped in.
  Menu: `Hearthbound → Phase 26 — Build NPC Animator Controller`.
- **`Editor/Phase26_NpcAnimatorCapstone.cs`** (130 LOC) — Idempotent one-click that builds the NPC controller, then walks the 3 NPC prefabs (Doris / Gerrold / SilentLaneVillager from Phase 13), adds Animator + sets the controller, adds `NpcAnimatorBridge` with the matching VillagerSO seed. Menu: `Hearthbound → 🎭 Phase 26 — Wire NPC Animators`.

### Added — Runtime (≈260 LOC, both in `HearthboundHollow.Mission` asmdef per D-035)

- **`Mission/NpcAnimatorBridge.cs`** (100 LOC) — Subscribes to `DialogueStartedEvent` / `DialogueEndedEvent` and toggles `Animator.SetBool("IsTalking", true/false)` when the event's `Villager` matches the bridge's serialized VillagerSO. Defensive cleanup on `OnDisable` / `OnDestroy`. Compatible with `Hearthbound_NPC.controller` but no-op on Animators whose controller lacks the `IsTalking` param.
- **`Mission/PlayerFootstepBinder.cs`** (160 LOC) — Animation-Event-driven footstep SFX. Exposes `OnFootstepLeft()` / `OnFootstepRight()` / `OnFootstep()` methods callable from Mixamo clip Animation Events. Picks SfxLibrary entry id from surface tag (Wood / Stone / fallback grass) + sprint state; plays through `SfxPlayer.PlayOneShot(id, volume)`. Opt-in polish — silent if SfxPlayer isn't registered or the clip has no events.

### Updated — Documentation

- **`Docs/PROGRESS.md`** — Phase 27 lead entry + Phase 26 lead entry + behaviour-change notes (Doris/Gerrold visibly Talking during dialogue) + new "Editor Menu Items" rows (17 total). Decisions table preserved at D-001..D-040; no new D-numbers added.
- **`Docs/SCENE_ASSEMBLY_GUIDE.md`** — ⚡ Fast path at top (single Phase 27 click) + Phase 26 sections explaining the player + NPC + camera wiring + Mixamo polish path + `PlayerFootstepBinder` setup.
- **`CHANGELOG.md`** (this entry) — release notes 0.4.0.

### Decisions adopted in this release

**None.** Every addition follows D-001..D-040:
- New runtime files (NpcAnimatorBridge, PlayerFootstepBinder) live in `HearthboundHollow.Mission` per D-005 + D-035 — they bridge Player + Audio + Memory cleanly without cross-asmdef cycles.
- Phase 27 master capstone uses reflection to discover sub-phases — same pattern as D-009 (Cinemachine via reflection).
- Phase 27 treats every sub-capstone as optional — same pattern as D-027 (progressive polish chain).

### Files net delta (vs 0.3.0)

- **+6 new C# files:**
  - `Editor/Phase27_BuildEverything.cs`
  - `Editor/Phase26_DiagnosticReport.cs`
  - `Editor/NpcAnimatorControllerBuilder.cs`
  - `Editor/Phase26_NpcAnimatorCapstone.cs`
  - `Mission/NpcAnimatorBridge.cs`
  - `Mission/PlayerFootstepBinder.cs`
- **Updated docs:** PROGRESS.md, SCENE_ASSEMBLY_GUIDE.md, CHANGELOG.md (this entry).
- **No runtime API changes** to existing files. PlayerController.cs, SmoothFollowCamera.cs, HelpOverlayUI.cs, HearthboundInput.inputactions — all untouched in this release.

### Behaviour changes the player sees

- **Doris and Gerrold visibly shift posture during dialogue** — soft 0.18 s / 0.22 s Idle ↔ Talking transition. With BoZo's base clip, the transition is subtle but real (the bool drives the state machine; a richer Mixamo Talking clip is a future drop-in).
- **One menu click builds everything** — `Hearthbound → ✨ Build EVERYTHING (Phase 27 — one click)` replaces the previous 3-step sequence.
- **A diagnostic menu** verifies the Phase 26 wiring is correct before the user presses Play, catching configuration drift before the playtest.

### Known limitations of v0.4.0

| # | Item | Severity | Status |
|---|---|---|---|
| P27-1 | Footstep binder is opt-in — no scene builder adds it automatically. | Cosmetic | ✅ Acceptable — drop `PlayerFootstepBinder` onto the Player root manually if desired |
| P27-2 | NPC Talking clip is the same as Idle clip until a Mixamo `Talking.fbx` is in the project. | Cosmetic | 🟡 Drop the FBX into `Assets/_Project/Animations/Mixamo/` + re-run the NPC capstone |
| P27-3 | Mixamo clips don't ship with Animation Events; footsteps require manually adding events per clip (~30 s/clip). | Workflow | ✅ Documented in `PlayerFootstepBinder.cs` header |

### How the user uses v0.4.0

```
1. Pull feat/mission-1-2-architecture. Wait for Unity recompile.
2. Hearthbound → ✨ Build EVERYTHING (Phase 27 — one click).   ← ~45 s
3. (Optional) Hearthbound → 🔍 Diagnose Phase 26 Build.        ← audit
4. Press Play in 00_Bootstrap.unity.
```

---

## [0.3.0-player-controller-and-animation] — 2026-05-24

**Branch:** `feat/mission-1-2-architecture` → PR #7 (open)
**Theme:** Phase 26 — Complete WASD player controller (sprint + jump + camera-relative input) + smooth follow camera (mouse orbit, scroll zoom, wall-clip) + Mixamo-ready Humanoid Animator Controller (1D blend tree, jump/fall/land states).

> ℹ️ Two Phase 26 workstreams landed on this branch in parallel: the *Narrative Hooks* thread (MarinNoteInteractable + Phase26_NarrativeHooks editor menu, plus the Phase 26.1 asmdef-locality hotfix) and the *Player Controller + Animation* thread (this entry). The decisions table has been renumbered to D-036..D-040 for PC+Animation so it doesn't collide with Phase 25's D-033/D-034 and Phase 26.1's D-035.

### Added — Phase 26 runtime (≈580 LOC)

#### New runtime scripts

- **`Player/SmoothFollowCamera.cs`** (230 LOC) — Spring-damped position (`SmoothDamp`), slerped rotation, mouse-orbit gated by Right Mouse Button or `AllowLook` action, scroll-wheel zoom (`distanceMin/Max`), sphere-cast wall-clip protection. Cinemachine-agnostic — works whether Cinemachine is installed or not.

#### Rewritten runtime scripts

- **`Player/PlayerController.cs`** (350 LOC, was 150) — Major upgrade:
  - Camera-relative WASD input.
  - Smooth acceleration + deceleration.
  - **Sprint** (Shift / LStick click). Auto-disabled in Gentle Mode.
  - **Jump** (Space / Gamepad south). Manual gravity on `CharacterController.Move()`. Coyote-time + jump-buffer.
  - Animator parameter bridge — 7 params (`Speed`, `MoveX`, `MoveY`, `VelocityY`, `IsGrounded`, `IsSprinting`, `Jump` trigger).
  - Public API preserved.

- **`Player/SimpleFollowCamera.cs`** — header updated to mark it as the legacy fallback.
- **`UI/HelpOverlayUI.cs`** (130 LOC, was 117) — Body text now built at Show() time; Gentle Mode strips Sprint/Jump.

### Added — Phase 26 editor tooling (≈400 LOC)

- **`Editor/PlayerAnimatorControllerBuilder.cs`** (200 LOC) — Builds `Hearthbound_Player.controller` (7 params, 4 states, Mixamo > BoZo > demo scoring).
- **`Editor/Phase26_PlayerControllerAndAnimation.cs`** (200 LOC) — Single-click capstone.

### Added — Tests (≈140 LOC)

- **`Tests/EditMode/PlayerControllerTests.cs`** — 7 NUnit tests locking the public surface.

### Added — Input Actions (≈90 LOC)

- **`Settings/HearthboundInput.inputactions`** — 5 new actions: Sprint, Jump, CameraLook, CameraZoom, AllowLook.

### Added — Documentation (≈280 LOC)

- **`Docs/ANIMATION_REQUIREMENTS.md`** — Animator state graph, parameter contract, Mixamo download list, Humanoid retargeting steps, design decisions D-036..D-040.

### Decisions adopted in 0.3.0

- **D-036** Sprint + Jump opt-in flags. Gentle Mode disables both.
- **D-037** Player Animator is single 1D blend tree on Speed.
- **D-038** Animator parameter names configurable on PlayerController Inspector.
- **D-039** SmoothFollowCamera is M1+M2 default; Cinemachine coexists.
- **D-040** Animations live in Assets/_Project/Animations/.

---

## [0.2.1-mission-1-2-ui-activation-hotfix] — 2026-05-24

**Branch:** `feat/mission-1-2-architecture` → PR #7 (open)
**Theme:** Phase 25 — fix the user-reported Tone Compass crash and the systemic single-layer UI wiring anti-pattern.

### Fixed

- **🐞 USER CRASH** — *"Coroutine couldn't be started because the the game object 'ToneCompass' is inactive!"*
- **Latent bugs:** `PauseMenuUI.Update()` and `HelpOverlayUI.Update()` were never firing — Escape and H keys did nothing.
- **`MissionTitleCard.Play()`** — same StartCoroutine risk.

### Added — defensive patterns

- **Two-layer wiring** in 11 builder methods. Script-host stays active; visual child toggles.
- **Self-heal Show()** in 9 UI overlay scripts.

### Decisions

- **D-033** Procedural UI builders MUST use the two-layer pattern.
- **D-034** UI overlay scripts MUST self-heal in their Show().

---

## [0.2.0-mission-1-2-polished-playable] — 2026-05-24

**Branch:** `feat/mission-1-2-architecture` → PR #7 (open)
**Theme:** Phase 23 (Mission 1 polish capstone) + Phase 24 (Mission 2 scenes).

### Added — Phase 23 (Mission 1 polish capstone, ~1,400 LOC)

- `Core/SettingsService.cs`, `Core/SettingsServiceBootstrap.cs`
- `UI/PauseMenuUI.cs`, `UI/HelpOverlayUI.cs`, `UI/MissionTitleCard.cs`
- `Audio/AmbientAudio.cs`
- `Mission/MainMenuSaveCoordinator.cs`, `Mission/PauseSaveCoordinator.cs`
- `Editor/Phase23_Mission1PolishCapstone.cs` (~670 LOC)

### Added — Phase 24 (Mission 2 Garden + Cottage, ~1,275 LOC)

- `Mission/Mission02Director.cs` (479 LOC)
- `Editor/Phase24_Mission2SceneBuilder.cs` (795 LOC)

### Decisions

- D-028 SettingsService is plain C# class.
- D-029 Save coordinators live in Mission asmdef.
- D-030 TeaBrewingUI default duration 12 s.
- D-031 Mission01Director.sceneAfterEndOfDay default + override.
- D-032 Runtime MonoBehaviours never in Editor-asmdef files.

---

## [0.1.1-mission-1-2-bugfix-and-tooling] — 2026-05-24

**Theme:** Bug-fix cycle (Phase 10.5) + quality-of-life tooling (Phase 11).

### Fixed
- CS1739 in SaveService.cs, CS0234/CS0246 in MiniGames + Dialogue, "Project has invalid dependencies".

### Added — Editor tooling (Phase 11)
- `Editor/SeedAssetGenerator.cs`, `Editor/HearthboundHollow.Editor.asmdef`.

### Added — Runtime (Phase 11)
- `Cutscene/ListenSceneSequencer.cs`, `Settings/HearthboundInput.inputactions`.

### Added — Tests (Phase 11)
- `Tests/EditMode/SaveAndRippleTests.cs` (+13 tests).

### Decisions
- D-008, D-009, D-010.

---

## [0.1.0-mission-1-2-architecture] — 2026-05-23

**Theme:** Phase 0 → Phase 10 — architecture, scripts, dialogue, save, mini-games, UI.

### Added

- `Docs/ARCHITECTURE.md`, `Docs/PROGRESS.md`, `Docs/EXISTING_ASSETS_INDEX.md`, `Docs/SCENE_ASSEMBLY_GUIDE.md`.
- 12 assembly definitions (10 production + 2 test).
- 32 C# scripts, ~4 k lines (Core 7, Memory 8, Player 9, MiniGames 3, UI 9, Dialogue 1, Cutscene 1, Save 2, Mission 3, Tests 1).
- 5 Yarn dialogue files.

### Decisions
- D-001 through D-007.

---

*Format: [SemVer](https://semver.org/spec/v2.0.0.html). Versions advance to 0.3.0 with Phase 26 (player controller + animation), 0.4.0 with Phase 27 (master capstone + NPC animator pipeline), 0.5.0 with Phase 28 / 29 / 30 (body-alignment / UI / onboarding trifecta). 1.0.0 when the 20-person greenlight playtest passes.*
