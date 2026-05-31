# Changelog — Hearthbound Hollow

All notable changes to this project will be documented here. Entries follow the [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) format.

> **Older releases (v0.1.0 → v0.7.3)** are preserved in [`Docs/CHANGELOG_HISTORY.md`](Docs/CHANGELOG_HISTORY.md). The active CHANGELOG.md focuses on the most recent release for fast scanning.

## [0.8.3-reading-nook] — 2026-05-31

**Branch:** `feat/mission-1-2-architecture` (on top of 0.8.2-polish-menu)
**Theme:** **Phase 52 — The Reading Nook.** The armchair next to the Hollow hearth
finally ships at runtime. Sit down, and Marin Vellis writes to you — five
hand-authored letters that progressively reveal the predecessor arc as your
`predecessorTrailWarmth` rises.

### Added

- **`Scripts/Memory/MarinLetterFragmentSO.cs`** — narrative data ScriptableObject.
  Fields: `fragmentId`, `title`, `warmthThreshold` (0–100), `letterText`
  (Vellis-tier hand-written prose), `warmthGranted`, `echoConnectionId`.
  Lives in the Memory asmdef (→ Core only).

- **`Scripts/Mission/ReadingNookInteractable.cs`** — the armchair Interactable.
  Pickle-approval gate: only opens if `VillageState.purchasedUpgradeIds` contains
  `"pickle_cushion"` — making the coin→upgrade→Pickle-settles→mystery-deepens loop
  feel narratively meaningful (D-082). First visit sets `readingNookVisited = true`
  and publishes `ReadingNookVisitedEvent`. Fragment reads are idempotent (D-083):
  warmth is granted only on the **first** read; re-reading is always available.
  Pickle rejection lines are warm and non-punishing.

- **`Scripts/UI/ReadingNookOverlay.cs`** — parchment two-panel overlay.
  Panel A: fragment list (locked fragments greyed-out as *"Not yet..."*,
  unread shown bold). Panel B: letter title + body + ← Back.
  Escape/Tab closes. D-068 hardening applied (alpha ≤ 0.001 → blocksRaycasts=false).
  Pure presentational layer — no VillageState mutation (handled by Interactable).

- **`Scripts/Editor/Phase52_ReadingNookBuilder.cs`** — idempotent installer.
  Creates/heals the 5 `MarinLetterFragmentSO` assets under
  `Resources/ReadingNook/`. Drops `_ReadingNookArmchair` (cube placeholder +
  warm point light) in the Hollow scene near the hearth. Drops
  `_ReadingNookCanvas` (ReadingNookOverlay). Wires all refs. Saves scene.
  Menu: `Hearthbound → ⚙️ Advanced → 📖 Phase 52 — Build Reading Nook`.
  Chained into `🚀 Build Everything` as Step 24 (after Phase 73 upgrade markers).

- **5 hand-authored Marin Vellis letters** (zero AI — GAME_DESIGN §9 Pillar 1):

| Id | Title | warmthThreshold | Echo link |
|---|---|---|---|
| `marin_letter_01` | Day One | 0 | — |
| `marin_letter_02` | The Borrowed Key | 15 | `MAR-NOTE-01` |
| `marin_letter_03` | The Sunday Purchase | 30 | `ECHO-MARIN-01` |
| `marin_letter_04` | What the Village Forgot | 50 | `GER-WIFE-01` |
| `marin_letter_05` | Why I Left | 70 | — |

### Decisions

- **D-082** — The Reading Nook Pickle gate is the `"pickle_cushion"` upgrade id.
  This makes the coin economy narratively resonant: earn coin → buy Pickle's
  cushion → Pickle settles → approves the armchair → predecessor mystery deepens.
  It is a *cozy payoff loop*, not a grind gate.
- **D-083** — Fragment warmth grants are **first-read only** and idempotent.
  Re-reading letters is always available (the text is always accessible once
  unlocked) but grants no additional `predecessorTrailWarmth`. This prevents
  farming and keeps the warmth curve author-controlled.

### asmdef graph (clean — D-035)

```
MarinLetterFragmentSO  — Memory  (→ Core)
ReadingNookOverlay     — UI      (→ Core, Memory, TMP)
ReadingNookInteractable — Mission (→ Core, Memory, UI, Save)
Phase52_ReadingNookBuilder — Editor
```

No new cycles. UI does not reference Mission. Audio is not affected.

### Cozy Contract ✅

- Pickle rejection is warm italic prose, not an error state.
- No fail state, no timer, no score.
- Re-reading always available.
- Escape/Tab close works at all times.
- `"Not yet..."` for locked letters — no percentage counter, no anxiety numbers.
- The armchair placeholder (procedural cube) ships cozy; a mesh artist swap
  is a pure asset drop, no code change required.

### QA acceptance (verify after pull + `🚀 Build Everything` + Play)

1. Enter the Hollow before buying Pickle's cushion → press E on armchair →
   Pickle says *"That's my spot."* (warm, no punishment).
2. Buy Pickle's cushion upgrade [U] → press E again → Reading Nook opens.
3. Letter 1 "Day One" is available immediately (warmthThreshold 0).
4. Letters 2–5 are greyed-out as *"Not yet..."* on fresh save.
5. Read Letter 1 → `predecessorTrailWarmth` increases by 5 → autosave fires.
6. Interact with Echo Hologram (+12 warmth) → return to nook →
   Letter 2 "The Borrowed Key" is now available.
7. Escape closes nook; Tab closes nook.
8. Re-reading Letter 1 adds no additional warmth (idempotent).
9. `🔍 Diagnose Build` clean; zero NRE boot → menu → gameplay.

### Files shipped

| Path | Role |
|---|---|
| `Scripts/Memory/MarinLetterFragmentSO.cs` | Data SO |
| `Scripts/Mission/ReadingNookInteractable.cs` | Interactable + Pickle gate |
| `Scripts/UI/ReadingNookOverlay.cs` | Parchment overlay |
| `Scripts/Editor/Phase52_ReadingNookBuilder.cs` | Idempotent installer |
| `CHANGELOG.md` | This entry |
| `STUDIO_LOG.md` | Phase 52 log entry |

**Net-new:** ~4 runtime scripts · 5 narrative SOs · ~800 LOC total ·
zero new external dependencies · zero asmdef cycles.

---

## [0.8.2-polish-menu] — 2026-05-29

**Branch:** `feat/mission-1-2-architecture` (on top of 0.8.1-one-more-day)
**Theme:** Player-requested polish — **human-sounding voices**, **language select
(English / العربية)**, **Reset Game → title**, and a **New-Game Character Creator**
(skin / outfit / accessory / name).

### Fixed

- **Dialogue voices no longer say "dot dot dot" / "full stop".** `Phase46_VoiceGenerator`
  (the espeak-ng path run by `🚀 Build Everything`) was feeding raw line text to the
  engine, which verbalises punctuation literally. Added `CleanForTts` + `IsDirtySource`
  (parity with `Tools/generate_voices.sh`): ellipses/dashes → comma pauses; stage
  directions + `*emphasis*` stripped; pure-punctuation lines (e.g. `"..."`) are voiceless;
  stale clips auto-purge + regenerate. Native-quality neural voices remain available via
  the open-source Piper pipeline (D-059). (D-064b)

### Added

- **`Scripts/Core/LocalizationService.cs`** — runtime EN/العربية UI localization (D-065)
- **`Scripts/UI/LocalizedText.cs`** — TMP binder + RTL flip
- **`Scripts/UI/SystemMenuUI.cs`** — Language + Customize + Reset Game settings
- **`Scripts/UI/CharacterCreationUI.cs`** — New-Game character creator
- **`Scripts/Mission/CharacterAppearance.cs`** — procedural avatar tints (D-066)
- **`Scripts/Mission/PolishMenuCoordinator.cs`** — Save-aware reset/customize bridge
- **`Scripts/Editor/Phase53_PolishMenuBuilder.cs`** — idempotent installer

### Cozy Contract

Reset uses warm in-panel confirm; keeps language/audio/comfort. Nothing punishes.

---

## [0.8.1-one-more-day] — 2026-05-29

**Theme:** The **"One More Day" goodnight beat** — the cozy retention hook.

### Added
- `OneMoreDayCard.cs`, `TomorrowTeaseSO.cs`, `EndOfDaySequencer.cs`, `Phase47_OneMoreDayBuilder.cs`

---

## [0.8.0-depth-layer] — 2026-05-28

**Theme:** The **Depth Layer** — Phase 48–51: Cold Open, Echo Hologram, Preface Beat, Memory Web.

### Added
- `ColdOpenCinematicUI.cs`, `BootstrapHookDirector.cs`, `Phase48_BootstrapHookCinematic.cs`
- `EchoHologramInteractable.cs`, `Phase49_EchoHologramBuilder.cs`
- `PrefaceBeatUI.cs`, `PrefaceBeatDirector.cs`, `Phase50_PrefaceBeatBuilder.cs`
- `MemoryWebOverlay.cs`, `KeepAliveOnLoad.cs`, `Phase51_MemoryWebBuilder.cs`
- VillageState: 9 new Depth Layer fields (incl. `readingNookVisited`, `letterFragmentsRead`)

### Decisions
D-060 (skip from frame 1) · D-061 (Echo translucency) · D-062 (Tone default STANDARD) · D-063 (Bootstrap-bound overlays)

### Note
Phase 52 Reading Nook was deferred (HH-DEPTH-52) — **now ships in v0.8.3**.

---

## Older releases

Full detail for **v0.1.0 → v0.7.3** in [`Docs/CHANGELOG_HISTORY.md`](Docs/CHANGELOG_HISTORY.md).

- **v0.7.3-voice-acting-piper** — Piper TTS + espeak-ng + voice ducking (D-058, D-059)
- **v0.7.1-polish-layer** — Phase 40-43 audio diagnostic + Listen scene camera
- **v0.7.0-foundation** — Phase 35-39 cutscene library + procedural audio studio
- **v0.6.0-mission1-polish-v2** — 8-cottage village, Hollow facade, hearth, URP volumes
- **v0.5.x** — Onboarding, dialogue repair, rig doctor, choice card repair
- **v0.4.0** — Build Everything master capstone + NPC animator
- **v0.3.0** — WASD/Sprint/Jump + SmoothFollowCamera + Mixamo Animator
- **v0.2.x** — Polished playable M1+M2, UI hotfixes
- **v0.1.0** — Phase 0–10 architecture

---

*Format: [SemVer](https://semver.org/spec/v2.0.0.html). 1.0.0 when the 20-person greenlight playtest passes.*
