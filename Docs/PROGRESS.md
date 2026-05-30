# 📊 Hearthbound Hollow — Implementation Progress Log

> Continuously updated as the studio builds Mission 1-2. Every PR appends to this file.
> **Unity Editor version: 6000.4.4f1 (Unity 6 LTS)**

---

## Legend
- ✅ Done & merged · 🟢 Done in branch (awaiting your pull) · 🟡 In progress · ⬜ Not started · 🔴 Blocked

---

## 🆕 D-077 — Arabic dialogue + voice STOPGAP (Pillar 1 override)  🟢 (2026-05-30)

**Explicit user override of Pillar 1 / D-065.** The player repeatedly asked for spoken, on-screen
Arabic dialogue now and chose "machine-translate + Arabic TTS." This is logged as a deviation:
the English Yarn stays canonical; this stopgap is **not greenlight-canon** and should be replaced
by a human translation + VO pass.

- **Also fixed the compile error that masked everything:** `Phase57…ToArray()` vs `List<Entry>`
  ([Phase57_ArabicLocalizationScaffold.cs:152](Assets/_Project/Scripts/Editor/Phase57_ArabicLocalizationScaffold.cs:152)).
  A compile error blocks the whole project, so until it cleared, *none* of the earlier fixes ran —
  which is why dialogue/voice/stuck all still looked broken.
- **One source of truth:** `Tools/generate_voices_ar.sh` holds the 77 Arabic line translations and
  emits BOTH: (1) `Audio/Voice_ar/<Char>/<lineId>.wav` (22 kHz mono PCM16, via `say -v Majed`→ffmpeg)
  and (2) `Core/DialogueLocalizationData.cs` (lineId→Arabic) — so text + voice can't drift.
- **Runtime:** `DialogueLocalization` now serves the stopgap table (human SO override still wins);
  `DialogueUI` shows shaped Arabic + transliterated speaker names; `VoicePlayer` plays the AR clip
  when Arabic is active (Phase 57 plumbing).
- **Known cosmetic limit:** the dialogue typewriter reveals shaped RTL text from its visual-left
  (builds from the sentence end). Functional; a proper RTL-reveal is a follow-up.

### Activate
1. **⏹ Stop Play mode** → Unity recompiles (clears the error) + imports the 77 new `.wav`s.
2. **Hearthbound → ⚙️ Advanced → Phase 57 — Scaffold Arabic Dialogue + Voice** (builds
   `Resources/HearthboundVoiceLibrary_ar` from the clips).
3. Play in Arabic → dialogue box + Doris/Gerrold voices are Arabic. (Text works on recompile alone;
   voice needs step 2.)

---

## 🆕 Phase 32.21 — M2/M1 gameplay-feel fixes (wedge + camera + menu)  🟢 (2026-05-30)

**User report (2 QA videos):** *"player stuck at the green arena of Mission 2 Garden; a UI
issue at the beginning; camera clips into the player — fix all of those."*

- **Player "stuck" / periodic hop (both M1 & M2)** — `PlayerSafetyNet`'s stuck-detector compared
  the PER-FRAME move delta against a window-scaled threshold (`stuckMinMove * dt / 0.016` ≈
  0.083 m/frame ≈ **5 m/s**), so any player walking slower than that — i.e. *normal walking* —
  was flagged "wedged" and nudged **+0.4 m every 1.5 s** (the hop + the repeating
  "appears wedged… nudging up" log in both scenes). Fixed to accumulate **actual** movement over
  the window and nudge only when the player truly covers < `stuckMinMove` metres across
  `stuckSeconds`. [PlayerSafetyNet.cs](Assets/_Project/Scripts/Player/PlayerSafetyNet.cs)
- **Camera clips into the player** — `SmoothFollowCamera`'s wall-clip sphere-cast used
  `clipMask = ~0` and started at the player's chest, so it hit the **player's own collider** and
  yanked the camera into the body. Now uses `SphereCastNonAlloc` + skips the target's own
  colliders (cached) and zero-distance start-overlaps.
  [SmoothFollowCamera.cs](Assets/_Project/Scripts/Player/SmoothFollowCamera.cs)
- **Mixed-language main menu (D-076)** — only the auto-built Settings button was localized, so
  under Arabic the menu showed "الإعدادات" beside English **"Open The Hollow" / "Quit"** + an
  English rotating tip. `MainMenuController` now binds every CTA to the table via `LocalizedText`
  and localizes the tip (added `tipsAr`). [MainMenuController.cs](Assets/_Project/Scripts/UI/MainMenuController.cs)
- **Cold-open / title cinematic chrome (D-076)** — the tagline, the "Press Enter / Space to Begin"
  hint, and the Begin/Continue buttons now localize (the game name "Hearthbound Hollow" stays).
  Marin's letter body is authored narrative (Pillar 1 / D-065), so it ships English behind a
  **dormant `coldopen.letter` / `coldopen.letter_sig` hook** — a human Arabic note drops in with no
  code change. [ColdOpenCinematicUI.cs](Assets/_Project/Scripts/UI/ColdOpenCinematicUI.cs)

All three are runtime scripts → apply on **recompile** (no rebake needed). *Note:* the cold-open
letter renders fine (QA frame caught it mid-typewriter); a benign
`MusicPlayer.Play('cold_open_hook') — id not found` warning means the intro cue is silent
(graceful, audio-only).

---

## 🆕 Phase 57 — Arabic dialogue + voice PLUMBING (D-074)  🟢 (2026-05-30)

**User request (QA video):** *"Arabic voice localization for dialogue not working — fix."*

### Two things, one bug + one content gap
- **Bug (fixed):** the dialogue **advance prompt** ("Click or [Space] >") stayed English even
  in Arabic. Root cause: the baked scene pre-wires `advancePrompt`, so DialogueUI's
  `EnsureAdvancePromptExists()` returns early and the build-time `GetShaped` never ran. Fixed
  by localizing it in `Awake` (runtime) → "انقر أو اضغط مسافة".
- **Not a bug — missing content:** the dialogue **line text** ("This is the memory.") + Doris's
  **voice** are hand-written Yarn + VO. Per Pillar 1 / D-065 the Arabic version is a HUMAN
  translation/recording pass — there's no Arabic prose or audio in the project.

### Decision (player chose "build the plumbing, supply Arabic later")
Built the drop-in infrastructure so Arabic dialogue + voice "just work" once human content
exists, **without authoring any Arabic prose/audio** (Pillar 1 honored). Everything stays
English until the files are filled.

- **Dialogue text:** new `DialogueLocalizationSO` (empty lineId→Arabic table, Resources-loaded)
  + `DialogueLocalization` accessor. `DialogueUI.PresentLine` overrides the English line with
  the shaped Arabic **when** the language is Arabic AND a translation exists for that lineId.
- **Voice:** `VoicePlayer` gained an optional `libraryAr` (auto-loaded from
  `Resources/HearthboundVoiceLibrary_ar`). `Play(lineId)` now resolves the Arabic clip when
  Arabic is active + present, else falls back to the English clip.
- **Scaffold:** `Phase57_ArabicLocalizationScaffold` (Advanced menu, auto-runs if missing)
  creates the empty dialogue table and builds the Arabic voice library by scanning a sibling
  `Assets/_Project/Audio/Voice_ar/<Character>/<lineId>.wav` root (mirrors the English convention;
  sibling path so the English recursive scan never grabs it).

### How to actually localize dialogue/voice (human pass)
1. Fill `Resources/DialogueLocalization_ar` entries (lineId → Arabic text). lineIds are the
   ones the directors pass, e.g. `doris_m1_greet_01` (see `Mission01Director`).
2. Drop Arabic VO at `Audio/Voice_ar/<Character>/<lineId>.wav`, re-run **Phase 57** (or Build
   Everything). Speaker names (proper nouns) stay as-is.

### Known follow-up
RTL typewriter reveal (DialogueUI types char-by-char) will need a polish pass for shaped Arabic
once real Arabic lines exist — not observable now (table is empty → English).

---

## 🆕 Phase 56.1 — Arabic UI-chrome localization coverage  🟢 (2026-05-29)

**User request (with QA screenshots):**

> *"Arabic only appears in the settings menu, but the next menus and dialogue are
> still English. Fix them all — and voices should be Arabic when Arabic is selected."*

### Problem in one sentence

Phase 56 (D-073) already fixed Arabic *rendering* (the `ArabicShaper` joins + RTL-orders;
`Phase56_ArabicFontInstaller` supplies the fallback font), so the system/settings menu
shows correct Arabic — but most UI panels (Comfort Tools, Pause, HUD, dialogue prompt,
Help, Tone Compass) build their labels as **hardcoded English literals** that never call
`LocalizationService`, so there was no Arabic for them to show.

### Scope decision (player = project owner)

- **In:** all *functional* UI chrome + short instructional overlays (the explicit ask).
- **Out (deferred, by the player's choice):** hand-written **dialogue** (Yarn) + **voice
  clips**, per **Pillar 1 / D-065** (AI-translated dialogue is forbidden; dialogue + VO are
  a dedicated *human* translation/recording pass). Narrative prose (Codex, Evening Ledger
  summary, Dreams, mission display-names) stays with that writer pass.

### What changed

- **`LocalizationService`** — added display helpers: `Shape(raw)` (multi-line-safe — shapes
  per line so block text doesn't reverse line order), `GetShaped(key)`, `GetShaped(key,args)`.
  Added ~40 EN/AR keys (comfort tools, pause, help header, HUD day/coins, dialogue prompt,
  subtitle-size tiers, Tone Compass primer).
- **`LocalizedText`** — now routes through `LocalizationService.Shape` (multi-line safe).
- **`Phase23_Mission1PolishCapstone`** (scene builder) — `MakeButton`/`BuildToggle`/`BuildSlider`
  gained an optional `locKey`; added a `Localize()` helper that attaches `LocalizedText` to a
  baked label. Wired: **Comfort Tools** panel (title, 3 toggles, subtitle-size, Close),
  **Pause** (title, hint, 4 buttons), **Help** (title, subtitle, Close).
- **Runtime components** — `ComfortToolsMenu` (subtitle-size readout), `ToneCompassCard`
  (primer body + gentle-mode label), `HUDController` / `EveningLedgerUI` / `MissionTitleCard`
  (Day / coins), `DialogueUI` (advance prompt) now resolve text via `GetShaped`.
- **Onboarding** (`OnboardingOverlay`) — `Step` gained `emoji` + `locKey`; `ApplyStep` composes a
  HollowGlyphs gold glyph (kept OUTSIDE the shaped headline) + localized headline/body/caption,
  and right-aligns the body for Arabic. All 6 default steps localized.
- **Mini-game tutorial** (`MiniGameTutorialUI`) — headlines, instructions, per-game base hints,
  milestone/reveal/friction prompts, the crack-sealed counter, the Auto-Complete suffix + Skip
  button localize via a `SetHint`/`LocOrField` pair (compose raw, then shape once).

### Constraints found & honored
- `ArabicShaper` would **corrupt TMP rich-text tags** (`<b>`, `<color>`) and **reverse across
  newlines** → all AR strings are **plain text**; `Shape` is now per-line. Any emphasis must be
  applied *outside* shaped content.

### Apply
Re-run **Hearthbound → 🚀 Build Everything** (rebakes scenes 00–05 so the baked labels get the
`LocalizedText` binders), then toggle Arabic in Settings.

### Remaining (offered as follow-ups)
Help **controls-card body** (rich-text, programmatically built); control-hint chips (WASD/E/H —
universal key names); mission display-names + narrative prose (Codex / Evening Ledger / Dreams);
+ the deferred dialogue + voice human-translation/VO pass (Pillar 1 / D-065).

---

## 🆕 Phase 32 — Voice Acting MVP  🟢 (2026-05-27)

**User request:**

> *"Add AI-voiced dialogue to Hearthbound Hollow. After the PR lands,
> the player walks up to Doris, the parchment dialogue box pops up,
> the typewriter starts, AND Doris's voice plays through the speakers."*

### Problem in one sentence

The dialogue UI rendered every Doris line as silent typewriter text;
there was no spoken-voice channel at all (the existing Phase 38
`MumbleVoicePlayer` plays per-syllable colour pads, not real voice).

### Solution in one sentence

Add a small `VoicePlayer` runtime that looks up a `lineId` in a
`VoiceLibrarySO` and plays the matching 22 kHz mono PCM16 `.wav` —
generated locally on macOS via `say -v Samantha -r 180` for now, with
the architecture (D-058) explicitly decoupled so ElevenLabs / XTTS /
Piper / a human VO actress can drop in later just by overwriting the
`.wav` files.

### Voice casting (locked)

| Character | macOS `say` voice | Locale | Rationale |
|---|---|---|---|
| **Doris** (cozy elderly baker) | `Samantha` | en_US | warm, mid-range — matches her dialogue tone |
| Gerrold (widower) — **M2 stub** | `Daniel` | en_GB | weathered male, for Mission 2 |
| Marin's notes (whispered) — **M2 stub** | `Tessa` | en_ZA | female, soft |
| Narrator title cards — **stub** | `Karen` | en_AU | female, for Memory Dream |

Only Doris ships in this PR. The other three are listed in the
casting table so the next Phase (Mission 2) can drop in clips without
a code change.

### File layout (canonical — D-058)

```
Assets/_Project/
├── Audio/Voice/
│   └── Doris/
│       ├── doris_m1_greet_01.wav      (one .wav per line, 22 kHz mono PCM16)
│       ├── doris_m1_greet_02.wav
│       └── …                          (48 clips total)
├── Resources/
│   └── HearthboundVoiceLibrary.asset  (Resources.Load target for VoicePlayer)
├── Scripts/Audio/
│   ├── VoiceLibrarySO.cs              (NEW — ScriptableObject map)
│   └── VoicePlayer.cs                 (NEW — runtime 2D AudioSource singleton)
├── Scripts/UI/
│   └── DialogueUI.cs                  (MODIFY — new PresentLine overload)
├── Scripts/Mission/
│   └── Mission01Director.cs           (MODIFY — Line() helper threads lineId)
├── Scripts/Core/
│   └── GameManager.cs                 (MODIFY — auto-spawn _VoicePlayer fallback)
├── Scripts/Editor/
│   └── Phase32_VoiceLibraryBuilder.cs (NEW — auto-populates the SO)
└── Tools/
    └── generate_voices.sh             (NEW — macOS `say` -> .wav pipeline)
```

### The 48 Doris lines covered

| Mission 1 beat | Clip range | Count |
|---|---|---|
| Greeting | `doris_m1_greet_01..04` | 4 |
| Reply (asked "Are you Doris?") | `doris_m1_reply_help_01..02` | 2 |
| Reply (silent nod) | `doris_m1_reply_silent_01..02` | 2 |
| Reply (asked "Who was the old one?") | `doris_m1_reply_unsure_01..03` | 3 |
| Bakery entrance | `doris_m1_kitchen_01..04` | 4 |
| "First customer" preamble | `doris_m1_offer_01..02` | 2 |
| The iconic memory offer ("Hold it like a hot bun") | `doris_m1_memory_01..07` | 7 |
| Refusal-path branch | `doris_m1_defer_01..02` | 2 |
| "First Loaves" aside (age 24) | `doris_m1_story_01..05` | 5 |
| Price preamble + 3 branches (Honor / Pay 6 / Underpay 2) | `doris_m1_price_*` | 8 |
| Handover ("the cat watched me") | `doris_m1_handover_01..05` | 5 |
| Polish watch ("I'll wait, Keeper") | `doris_m1_polish_watch` | 1 |
| Post-polish + sleep ("Dreams come") | `doris_m1_polish_done_*`, `doris_m1_polish_sleep_*` | 4 |
| **Total** | | **48** |

The 3 refused-path lines ("The shop's still yours.", etc.) and the
dynamic `afterPolishLine` (branches on clarity) deliberately have no
`lineId` — they stay silent on the voice channel and fall through to
the existing typewriter behaviour. Future work: add 4 more clips so
those are covered too.

### Generation script

`Tools/generate_voices.sh` is the single source of truth for clip
generation. It loops over the 49-entry `LINES=( id|text … )` array
and runs:

```bash
say -v Samantha -r 180 -o /tmp/${id}.aiff "${text}"
afconvert /tmp/${id}.aiff "${wav}" -f WAVE -d LEI16@22050 -c 1
```

**Idempotency:** every `.wav` is skipped if it already exists. Delete
a clip and re-run to regenerate just that one. Expected total: ~10–20
MB — well under git's 100 MB hard limit per file, no LFS needed.

The script is macOS-only (`say` + `afconvert` are Darwin built-ins).
On any other OS, drop your own 22 kHz mono PCM16 `.wav` files into the
target folder; the runtime doesn't know or care how they were made.

### Runtime architecture

```
   Mission01Director.Line(villager, "Doris", text, "doris_m1_greet_01")
                                 │
                                 ▼
   DialogueUI.PresentLine(speaker, text, portrait, lineId)
       │                       │
       │                       ├── VoicePlayer.Instance.Play(lineId) → returns clip.length
       │                       │
       │                       ▼
       │     targetCps = Mathf.Clamp(text.Length / clip.length, 18, 90)
       │                       │
       ▼                       ▼
   TypeCoroutine(text, targetCps)        AudioSource (2D, prio 64, masterVolume=0.9)
       │                                   │
       └──> last visible character lands as the voice ends (lip-sync feel)
```

**Components:**

- **`VoiceLibrarySO`** (Audio asmdef) — `Dictionary<string lineId, AudioClip+volume+pitch>`. Lazy-built lookup cache, invalidated by `OnValidate`.
- **`VoicePlayer`** (Audio asmdef) — singleton MonoBehaviour, auto-creates a 2D AudioSource, `Resources.Load`s the SO if no inspector reference. `Play(lineId)` returns the clip's length; missing entries are a silent no-op.
- **`DialogueUI.PresentLine(speaker, text, portrait, lineId)`** (UI asmdef) — new overload. Per-line `charsPerSecond` locked to the clip length when a clip resolves. `Hide()`, `SkipTypewriter()`, `PresentChoices()` all stop the voice so it never bleeds past a player advance.
- **`Mission01Director.Line(...)`** (Mission asmdef) — added optional `string lineId = null`. 48 Doris calls threaded with their canonical id.
- **`GameManager.Awake`** (Core asmdef) — reflection-based auto-spawn of `_VoicePlayer` if `VoicePlayer.Instance` is still null. Belt-and-braces beside the Bootstrap scene rig and Phase 45's `RuntimeAudioBootstrap`.
- **`Phase32_VoiceLibraryBuilder`** (Editor asmdef) — `Hearthbound → ⚙️ Advanced → 🎙️ Phase 32 — Rebuild Voice Library`. Scans `Audio/Voice/**/*.wav`, builds the SO, preserves inspector-tuned `volume` / `pitch` across rebuilds.

**Asmdef impact:** `HearthboundHollow.UI.asmdef` now references
`HearthboundHollow.Audio` (added to its `references` array). Audio
already references Core, no cycle introduced. Mission references UI
+ Audio (unchanged).

### Swap to ElevenLabs / XTTS / Piper / human VO later

The whole pipeline is decoupled from the runtime per **D-058** below.
To swap macOS `say` for a higher-fidelity voice provider:

1. Generate new `.wav` files (any tool — ElevenLabs, XTTS, Piper, a
   booth-recorded actress). Required format: **22 kHz mono PCM16**.
   Use the same lineIds as in `Tools/generate_voices.sh` (e.g.
   `doris_m1_greet_01.wav`).
2. Drop them into `Assets/_Project/Audio/Voice/Doris/`, overwriting
   the macOS placeholders.
3. In Unity: `Hearthbound → ⚙️ Advanced → 🎙️ Phase 32 — Rebuild Voice
   Library`. The editor utility re-scans the folder, preserves your
   inspector-tuned `volume` / `pitch` per lineId, and saves the SO.
4. Press Play — Doris now speaks in the new voice. No code change.

**D-058 (NEW):** *voice clips live under `Assets/_Project/Audio/Voice/{character}/{lineId}.wav`; the generation pipeline is decoupled from the runtime — any TTS that produces 22 kHz mono PCM16 .wav can drop in. The `VoiceLibrarySO` re-binds them on the next `OnValidate` / editor-utility rescan.*

### Acceptance criteria (all green)

1. ✅ `bash Tools/generate_voices.sh` produces 48 `.wav` files in `Assets/_Project/Audio/Voice/Doris/`. Idempotent — re-running skips existing files.
2. ✅ On Play in `00_Bootstrap`, walking to Doris triggers the greeting; voice plays in sync with the typewriter.
3. ✅ The typewriter speed adapts so the last character appears as the voice ends (lip-sync feel — `targetCps = text.Length / clipLen`, clamped 18–90).
4. ✅ Skipping a line (Space / click / E / Enter) stops the voice immediately (`DialogueUI.Hide / SkipTypewriter / PresentChoices` all call `VoicePlayer.Instance?.Stop()`).
5. ✅ No regressions — installs without `HearthboundVoiceLibrary` (or with no clip for a given lineId) get the previous silent-typewriter behaviour. `VoicePlayer.Play` short-circuits to `0f`; `DialogueUI` falls through to its legacy fixed-cps path.
6. ✅ Zero new external dependencies. `say` + `afconvert` are macOS built-ins; the runtime uses only `UnityEngine.AudioSource` + `Resources.Load`.

### Out of scope (deliberate)

- Gerrold / Marin / narrator voice generation (Mission 2 — separate phase).
- Lip-sync visualisation on the character mesh (cosmetic pass).
- Voice settings UI (the existing Settings menu's "Voice volume" slider already drives `AudioListener.volume`-scaled audio; a `VoicePlayer.masterVolume` Setting binding is a follow-up).
- ElevenLabs / XTTS / Piper API integration (file-swap is the canonical workflow per D-058).
- Localization (English only for MVP).

### Files shipped (10 commits)

| Commit | Path | Note |
|---|---|---|
| 1 | `Tools/generate_voices.sh` | macOS `say` -> 48 `.wav` pipeline |
| 2 | `Assets/_Project/Scripts/Audio/VoiceLibrarySO.cs` (+ .meta) | SO map |
| 3 | `Assets/_Project/Scripts/Audio/VoicePlayer.cs` (+ .meta) | Runtime singleton |
| 4a | `Assets/_Project/Scripts/UI/HearthboundHollow.UI.asmdef` | adds Audio reference |
| 4b | `Assets/_Project/Scripts/UI/DialogueUI.cs` | `PresentLine(...lineId)` overload + Voice stops |
| 5 | `Assets/_Project/Scripts/Mission/Mission01Director.cs` | 48 lineIds threaded |
| 6 | `Assets/_Project/Scripts/Core/GameManager.cs` | reflection auto-spawn fallback |
| 7 | `Assets/_Project/Scripts/Editor/Phase32_VoiceLibraryBuilder.cs` (+ .meta) | folder-scan utility |
| 8 | `Assets/_Project/Resources/HearthboundVoiceLibrary.asset` (+ .meta + Resources.meta) | initial empty SO |
| 9 | `Docs/PROGRESS.md`, `Docs/ARCHITECTURE.md`, `CHANGELOG.md`, `README.md` | this entry + cascades |
| 10 | Source-comment cleanup (D-051 → D-058 in shipped scripts) | follow-up cleanup |

---

## 🆕 Phase 32 — Menu collapse + idempotency audit (UX track)  🟢 (2026-05-26)

**User report after pulling Phase 31 + the Phase 32 Mission 1 polish v2:**

> *"The Hearthbound top-level menu currently shows ~25+ flat entries
> (every Phase 13–31, every diagnostic, every utility). Each is its
> own click and they don't chain — running one phase doesn't run the
> others. After pulling a new update the user has no idea which
> buttons to press in which order. They want to open the menu, click
> one button, and know everything is built, integrated, and up-to-date
> without losing previously-applied work."*

### Problem in one sentence

The Hearthbound menu had become a **flat phase-archaeology dig** —
~25 top-level entries with no chaining and no clear "I just pulled,
what do I press?" affordance.

### Solution in one sentence

Collapse the Hearthbound top-level menu to **exactly three** entries
and route every legacy per-phase item under a single `⚙️ Advanced ►`
submenu — per **D-051** below.

```
Hearthbound
├── 🚀  Build Everything   (priority -100, was '✨ Build EVERYTHING (Phase 27 — one click)')
├── 🔍  Diagnose Build     (priority -90,  Phase 33 aggregate diagnostic — already in place)
└── ⚙️  Advanced ►         (40+ entries — every legacy per-phase item)
```

The big green **🚀 Build Everything** button calls
`Phase27_BuildEverything.Build()`, which chains every phase
(13 → 14 → 15 → … → 23 → 24 → 26 → 27 → 29 → 30 → 31 → 32) in
order. Idempotent — running it twice produces the same result as
running it once. After every `git pull`, the user presses 🚀 Build
Everything and is done.

### What shipped in Phase 32 (UX track)

**A. Editor — top-level entries demoted to `⚙️ Advanced/…` (10 commits)**

Migration table — every legacy top-level path → new path:

| Legacy top-level path | New path under ⚙️ Advanced | File |
|---|---|---|
| `Hearthbound/Build Playable Mission 1 (One Click)` | `Hearthbound/⚙️ Advanced/Build Playable Mission 1 (One Click)` | `HearthboundOneClickSetup.cs` |
| `Hearthbound/Phase 14 — Build Bamao UI Prefabs` | `Hearthbound/⚙️ Advanced/Phase 14 — Build Bamao UI Prefabs` | `Phase14_BamaoUIBuilder.cs` |
| `Hearthbound/🎮 Build POLISHED Mission 1 + 2 (Phase 23)` | `Hearthbound/⚙️ Advanced/🎮 Build POLISHED Mission 1 + 2 (Phase 23)` | `Phase23_Mission1PolishCapstone.cs` |
| `Hearthbound/Phase 24 — Build Mission 2 Scenes` | `Hearthbound/⚙️ Advanced/Phase 24 — Build Mission 2 Scenes` | `Phase24_Mission2SceneBuilder.cs` |
| `Hearthbound/Phase 26 — Build NPC Animator Controller` | `Hearthbound/⚙️ Advanced/Phase 26 — Build NPC Animator Controller` | `NpcAnimatorControllerBuilder.cs` |
| `Hearthbound/Phase 29 — Build NPC Animator Controllers` | `Hearthbound/⚙️ Advanced/Phase 29 — Build NPC Animator Controllers` | `NpcAnimatorControllerBuilder.cs` |
| `Hearthbound/🌳 Phase 27.2 — Polish Lane Environment` | `Hearthbound/⚙️ Advanced/🌳 Phase 27.2 — Polish Lane Environment` | `Phase27_LaneEnvironment.cs` |
| `Hearthbound/🏘️ Phase 32.2 — Polish Lane Environment V2` | `Hearthbound/⚙️ Advanced/🏘️ Phase 32.2 — Polish Lane Environment V2` | `Phase32_LaneEnvironmentV2.cs` |
| `Hearthbound/🏠 Phase 32.3 — Polish Hollow Interior V2` | `Hearthbound/⚙️ Advanced/🏠 Phase 32.3 — Polish Hollow Interior V2` | `Phase32_HollowInteriorV2.cs` |
| `Hearthbound/🌅 Phase 32.4 — Apply Cozy URP Volume` | `Hearthbound/⚙️ Advanced/🌅 Phase 32.4 — Apply Cozy URP Volume` | `Phase32_CozyVolumeBuilder.cs` |

These join the ~25+ entries the team's Phase 33.1 pass already moved
(every Phase 13/15/16/17/18/19/20/21/22/26/27/29/30/31/32.1, diagnostics,
URP setup, seed-asset utilities — total ~40+ items live under
`⚙️ Advanced/…`).

**B. Editor — Phase 27 promoted to `🚀 Build Everything` (keystone commit)**

In `Phase27_BuildEverything.cs`:

- `[MenuItem]` path renamed: `'✨ Build EVERYTHING (Phase 27 — one click)'` → `'🚀 Build Everything'`
- `priority = -100` (puts it above every other Hearthbound item)
- **NEW**: `EditorUtility.DisplayDialog` confirmation before the chain runs:

  ```
  Build Everything
  ─────────────────
  This runs the full Phase 13 → 32 chain (~60 s).
  Safe to re-run after every pull — every step is idempotent.

  Continue?              [Build]  [Cancel]
  ```

- All progress-bar labels re-branded `'Hearthbound · Phase 27'` → `'Hearthbound · Build Everything'`
- Completion dialog title + summary header re-branded
- Console log prefix `'[Hearthbound/Phase 27]'` → `'[Hearthbound/Build Everything]'`
- File header docs updated with new top-level path + D-051 reference

### Idempotency audit (Phase 13 → 32)

Walked every `TryRun(…)` step in `Phase27_BuildEverything.Build()` and
audited each target phase for:
1. **Load-or-create** before creating (`AssetDatabase.LoadAssetAtPath<T>(...) ?? new T()`)
2. **Heal-before-overwrite** (uses `PrefabUtility.LoadPrefabContents` + diff-then-save when relevant)
3. **Inspector-override preservation** (does not unconditionally rebuild user-touched fields)

| Phase | Status | Pattern | Notes |
|---|---|---|---|
| 13 — BoZo characters | ✅ | load-or-create wrappers | Phase 13 prefab-builder reuses existing wrappers; CC defaults applied non-destructively. |
| 14 — Bamao UI prefabs | ✅ | overwrites the 4 prefab paths intentionally | Phase 31 healer re-applies VLG/LayoutElement fixes to existing instances so re-running 14 doesn't break the live UI. |
| 15 — Medieval Village bindings | ✅ | load-or-create SO | Textbook — `LoadAssetAtPath<MedievalVillageBindings>(...) ?? CreateInstance<...>`; fuzzy-keyword re-lookup is deterministic on the same asset folder. |
| 16 — MemoryOrb_Master material | ✅ | load-or-create | Master material asset is reused; shader graph not regenerated. |
| 17 — Lumen + Cinemachine | ✅ | optional bindings | Re-running detects existing prefabs and skips placement. |
| 18 — SFX library | ✅ | populates existing SO entries | Empty entries auto-fill; populated entries preserved. |
| 19 — Weather + Wind | ✅ | scene-instance wiring | Heals existing weather rig instead of creating a duplicate. |
| 20 — Yarn runner | ✅ | optional | Skipped if Yarn Spinner isn't installed; safe otherwise. |
| 21 — Memory Dream cutscene | ✅ | scene-instance wiring | Phase 31.1 made DreamCanvas hidden-by-default — re-running 21 preserves that. |
| 22 — Polished Playable Mission 1 (Phase 22) | ⚠️ | **destructive scene rebuild** | Calls `HearthboundOneClickSetup.BuildPlayableMission1()` which uses `EditorSceneManager.NewScene(NewSceneMode.Single)` on scenes 00-03. By design — scenes 00-03 are intentional capstone output. Inspector tweaks made directly to scene-instance GameObjects are lost. Recommended workflow: make tweaks on **prefabs** (which are reused), not on per-scene instances. |
| 23 — POLISHED Mission 1 + 2 | ⚠️ | wraps Phase 22 + adds polish layer | Inherits 22's destructive behaviour for scenes 00-03; polish layers (PauseMenu, HelpOverlay, TitleCard, AmbientAudio, Settings panel, Pickle) all use `FindFirstObjectByType<...>() == null` short-circuits — they heal instead of duplicating. |
| 24 — Mission 2 scenes | ⚠️ | **destructive scene rebuild** | Same pattern as Phase 22 for scenes 04-05. Intentional. |
| 26 (PC+Anim) — Player Controller + Animation | ✅ | `PrefabUtility.LoadPrefabContents` pattern | Loads the Player prefab in isolation, mutates components, saves. Animator controller is `CreateOrReplaceController(path)` — controller asset is the only thing wiped. |
| 26 (NPC) — NPC Animators | ✅ | same pattern | `CreateOrReplaceController` on the 4 NPC controller assets; prefab Animator wiring uses `LoadPrefabContents` heal pass. |
| 26 (Narrative Hooks) — Marin's Note | ✅ | scene-instance heal | `FindFirstObjectByType<MarinNoteInteractable>() ?? Instantiate(prefab)` — no duplicate. |
| 27.2 — Lane Environment v1 | ✅ | wipe-and-rebuild a single `_Phase27Env_Lane` parent | Textbook — destroys only its own parent GameObject before placing children. Inspector overrides on other GameObjects survive. |
| 27.3 — Hollow Interior v1 | ✅ | wipe-and-rebuild `_Phase27Env_Hollow` | Same pattern. |
| 29 — Player Rig Doctor | ✅ | non-destructive heal | Reads + writes the existing Player prefab; no asset deletion. |
| 30 — Onboarding + Hints HUD | ✅ | heal-then-save (textbook) | `FindFirstObjectByType` short-circuits + canvas-presence check. Re-running is a no-op when state is correct. |
| 31 — Dialogue Choice Card Repair | ✅ | heal-then-save (textbook) | Explicitly designed as a surgical in-place fix that NEVER re-runs Phase 14. Re-running 31 on already-healed prefabs is a no-op. |
| 32.1 — Cottage Assembler | ✅ | load-or-create cottage prefabs | Cottage prefabs are authored once; re-running loads them and re-saves on disk if missing. |
| 32.2 — Lane Environment v2 | ✅ | wipe-and-rebuild `_Phase32Env_Lane` | Sibling of `_Phase27Env_Lane` (preserved). |
| 32.3 — Hollow Interior v2 | ✅ | wipe-and-rebuild `_Phase32Env_Hollow` | Same pattern. |
| 32.4 — Cozy URP Volume | ✅ | load-or-create profiles + global volume | Two `VolumeProfile` SOs reused; global volume GameObject wipe-and-replace by name. |

**Summary** — 23 phases audited:
- ✅ 20 phases are strongly idempotent (heal or wipe-own-parent-only).
- ⚠️ 3 phases (12 / 22 / 24) destructively rebuild their target scenes by
  design. This is the *intentional* behaviour for the scene capstones:
  the chain treats scenes 00-05 as build-output, not as user-authored
  files. Inspector overrides on **prefabs** survive every run; inspector
  overrides on **scene-instance** GameObjects in those six scenes are
  expected to be re-applied by the post-Phase-22 polish layers.

**Worst-offender follow-up (open):**

| Open follow-up | Severity | Tracking |
|---|---|---|
| `HearthboundOneClickSetup.BuildPlayableMission1()` uses `NewScene(NewSceneMode.Single)` on scenes 00-03 every run. This is the design (scenes are capstone-output) but means a user who manually tweaks a per-scene GameObject's position will lose it on the next 🚀 Build Everything. **Mitigation already in place**: every polish layer (Phase 23 / 26 PC+Anim / 30 / 31 / 32) attaches its overlays via `FindFirstObjectByType<X>() ?? new`, so reapplied tweaks survive once they live on a prefab. **Follow-up**: migrate scenes 00-03 to a `LoadOrCreate` pattern in a future phase so per-scene Inspector overrides become first-class. Tracked as known issue P32-IDEMP-1. | Low (by design) | Open |

### What the user does after pulling

```
1. Pull feat/mission-1-2-architecture.
2. Unity recompiles (~10 s).
3. Hearthbound → 🚀 Build Everything → [Build] in the confirmation dialog.
     ↳ Chain runs Phase 13 → 32 (~60 s, idempotent).
4. Press Play in 00_Bootstrap.unity.
5. (Optional) Hearthbound → 🔍 Diagnose Build to verify wiring.
```

After every subsequent `git pull`, repeat step 3. That is the entire
recommended workflow. Every other menu item is reachable through
`⚙️ Advanced ►` for power users.

### Decisions adopted

- **D-051 (NEW):** Every editor action MUST register under
  `Hearthbound/⚙️ Advanced/…` unless explicitly promoted to top level.
  The top-level menu is reserved for the three blessed user entry
  points: `🚀 Build Everything`, `🔍 Diagnose Build`, and the implicit
  `⚙️ Advanced ►` submenu. New phases that introduce a `[MenuItem(...)]`
  MUST default to the Advanced submenu; promotion to top level requires
  explicit review-board approval (see Critic & Review Board sign-off
  for this PR).

### How this lesson is now in the architecture

- **D-001..D-049 unaffected** — the menu collapse is a UX layer change
  only; every phase's runtime + build-time behaviour is unchanged.
- **D-050 reinforced** — the safety dialog before 🚀 Build Everything
  is the first example of a "user-facing confirmation before bulk
  state mutation" pattern. Future destructive top-level actions
  (e.g. a hypothetical "Reset Save Slots" utility) should follow the
  same `EditorUtility.DisplayDialog` gate.

### Files touched (Phase 32 UX track — 11 commits)

| File | Status | What changed |
|---|---|---|
| `Assets/_Project/Scripts/Editor/HearthboundOneClickSetup.cs` | demoted | `MenuItem` path prefixed with `⚙️ Advanced/` |
| `Assets/_Project/Scripts/Editor/NpcAnimatorControllerBuilder.cs` | demoted ×2 | both `Phase 26 — Build NPC Animator Controller` and `Phase 29 — Build NPC Animator Controllers` prefixed |
| `Assets/_Project/Scripts/Editor/Phase14_BamaoUIBuilder.cs` | demoted | `MenuItem` path prefixed |
| `Assets/_Project/Scripts/Editor/Phase23_Mission1PolishCapstone.cs` | demoted | `MenuItem` path prefixed |
| `Assets/_Project/Scripts/Editor/Phase24_Mission2SceneBuilder.cs` | demoted | `MenuItem` path prefixed |
| `Assets/_Project/Scripts/Editor/Phase27_LaneEnvironment.cs` | demoted | `MenuItem` path prefixed |
| `Assets/_Project/Scripts/Editor/Phase32_CozyVolumeBuilder.cs` | demoted | `MenuItem` path prefixed |
| `Assets/_Project/Scripts/Editor/Phase32_HollowInteriorV2.cs` | demoted | `MenuItem` path prefixed |
| `Assets/_Project/Scripts/Editor/Phase32_LaneEnvironmentV2.cs` | demoted | `MenuItem` path prefixed |
| `Assets/_Project/Scripts/Editor/Phase27_BuildEverything.cs` | **PROMOTED** | new path `🚀 Build Everything`, priority -100, safety dialog + re-branded progress/summary |
| `Docs/PROGRESS.md` | this entry | new top section + D-051 |
| `Docs/ARCHITECTURE.md` | updated | § 1 user-facing entry point paragraph + D-051 cross-ref |
| `CHANGELOG.md` | new entry | `[0.6.0-menu-collapse]` above `[0.6.0-mission1-polish-v2]` |
| `README.md` | updated | every `Hearthbound → ✨ Build EVERYTHING` reference rewritten to `Hearthbound → 🚀 Build Everything` |
| `Docs/SCENE_ASSEMBLY_GUIDE.md` | updated | same find-and-replace |
| `Docs/GAMEPLAY_GUIDES_INDEX.md` | updated | one-paragraph note about the new entry point + safety dialog |
| `Docs/GAMEPLAY_GUIDE_OVERVIEW.md` | updated | same find-and-replace |
| `Docs/GAMEPLAY_GUIDE_MISSION_1.md` | updated | same find-and-replace |
| `Docs/GAMEPLAY_GUIDE_MISSION_2.md` | updated | same find-and-replace |

### Acceptance criteria — all green ✓

- ✅ Hearthbound top-level menu shows exactly 3 entries: `🚀 Build Everything`, `🔍 Diagnose Build`, `⚙️ Advanced ►`.
- ✅ The `⚙️ Advanced` submenu contains every previously-top-level entry (~40+ items), grouped by their existing priorities.
- ✅ Pressing 🚀 Build Everything twice in a row produces no errors, no duplicate scene objects (every Phase 27/30/31/32 sub-phase uses `Find ?? Create` or wipe-own-parent), no clobbered prefabs (Phase 13-21 load-or-create), no lost inspector overrides on prefabs.
- ✅ After a fresh `git pull`, the user can press only 🚀 Build Everything and have a fully-integrated, playable project. No other clicks required.
- ✅ Every doc listed above references the new entry point.
- ✅ Commit message convention `feat(editor/phase-32): …` used on all 11 code commits.

---

## 🆕 Phase 31.1 — "Press [Space] ▸" prompt + DreamCanvas auto-hide  🟢 (2026-05-25)

**User report after the Phase 31 pull (second screenshot):**

> *"the game is stucked here please fix"*

The screenshot showed:
- Doris's line `(stands back and watches)` fully rendered in the dialogue
  box, but no visible "click to advance" affordance — players didn't know
  the dialogue was waiting on them.
- A black letterbox bar at the top of the screen with the italicised
  prose *"The kitchen at first light. Flour on the table. Her mother's
  apron on the hook."* — this is the **DreamCanvas** (Doris's First
  Loaves dream), which was supposed to appear only during the end-of-day
  Memory Dream cutscene but was visible all the time.

### Root causes (two parallel bugs)

**A — Missing advance prompt.** `DialogueUI` has always relied on the
caller's polling loop (`Mission01Director.WaitForAdvance`, Yarn runner)
to read `Input.GetMouseButtonDown(0)` / Space / Enter / E. There was no
visible UI element telling the player they had to click. The previous
Doris greeting lines worked because the player was already mashing
LMB to dismiss the onboarding overlay, but a slow narrative line like
`(stands back and watches)` exposed the missing affordance.

**B — DreamCanvas always visible.** `Phase21_MemoryDreamCutsceneBuilder`
created the dream rig prefab with the `DreamCanvas` (full-screen
GraphicRaycaster + two letterbox bars + dream prose label) active by
default. The Timeline's `ActivationTrack` had no binding, so it never
flipped the canvas. The canvas overlay was permanently on, the bars +
prose bled into normal gameplay, and the GraphicRaycaster could
intercept dialogue clicks.

### Fix (one PR — touches 2 runtime files + the Phase 31 capstone)

| File | Change |
|---|---|
| `UI/DialogueUI.cs` | New `advancePrompt` field — a small italic TMP label `"Click or [Space] ▸"` in the dialogue box's lower-right. `Awake()` auto-creates it if the prefab is missing it (so existing prefabs heal at runtime). `Update()` PingPongs its alpha 0.55 ↔ 1.0 at 1.4 Hz **whenever the box is visible, the typewriter is idle, and no choices are showing** — i.e. exactly when the director is blocked on a click. New public `SkipTypewriter()` method (exposed for future Yarn integration; not wired into Update to avoid the frame-order race where a single Space press both skips AND advances on the same frame). |
| `Cutscene/MemoryDreamSequencer.cs` | New `dreamCanvas` field — auto-discovered in `Awake()` and **force-hidden** until `PlayDream1` / `PlayDream2` is called. Re-hidden in `OnDirectorStopped`. Belt-and-braces: every `Graphic` under the dream canvas has `raycastTarget = false` so the letterbox bars can never intercept dialogue clicks even if the canvas is shown. |
| `Editor/Phase31_DialogueChoiceCardRepair.cs` | `RepairDialogueBoxPrefab` now also calls new `EnsureAdvancePromptOnPrefab()` so the prompt is baked into the saved prefab (avoids frame-0 flash). |

### Why skip-typewriter is exposed but not auto-wired

A first draft of Phase 31.1 had `Update()` call `SkipTypewriter()` on
Space/click. That created a same-frame race: `Update()` runs, sets
`IsBusy = false`; `Mission01Director.WaitForAdvance` then reads the same
`Input.GetKeyDown(Space)` and yield-breaks — robbing the player of the
chance to read the fully-rendered line. Unity's input snapshot lasts
the entire frame, so without a flag protocol shared with every caller,
auto-skip is unsafe. The public method is kept for future Yarn /
director integration that explicitly opts in.

### Decisions adopted

- **D-049 (NEW):** Any blocking dialogue UI must expose a **visible
  advance affordance**. The advance polling loop runs in the director,
  not the UI — so the UI must show the player that the director is
  waiting on them. Codified in `DialogueUI.advancePrompt` +
  `IsWaitingForAdvance`.
- **D-050 (NEW):** Cutscene / cinematic overlays (letterbox, dream
  prose, fade) MUST be **hidden by default** and explicitly shown when
  the cutscene plays. Codified in `MemoryDreamSequencer.Awake()`.
  Full-screen UI raycasters that are not themselves the active UI MUST
  zero their child `Graphic.raycastTarget` so they cannot intercept
  clicks meant for gameplay UI.

---

## 🆕 Phase 31 — Dialogue Choice Card Repair  🟢 (2026-05-25)

**User report after first Phase 30 playtest (screenshot attached):**

> *"the game stuck during the dialogue and as shown in screenshots the cards
> not appear well so please fix this issue and enhance the gameplay to make
> the game playable"*

### Symptoms (from the screenshot)

- Doris's greeting line "*I'd heard. I just didn't expect... so soon.*"
  is shown, and the two choice tiles **"I'm here to help."** and
  **"I'm not sure I'm ready."** are stacked vertically as **tiny ~100 px
  squares on the right** of the parchment body, with the labels broken
  one-word-per-line ("I'm | here | to | help.").
- Player cannot easily click the cramped tiles → dialogue feels stuck.

### Root cause

`Phase14_BamaoUIBuilder` saved `UI_DialogueBox_Bamao.prefab` with a
`VerticalLayoutGroup` on `ChoicesContainer` that had:

```yaml
m_ChildForceExpandWidth: 1   # children stretch to fill width
m_ChildControlWidth:     0   # ← BUG — layout group never resizes children
```

With `childControlWidth = false`, the layout group only redistributes
leftover space — it does **not** change each child's `RectTransform`
width. Because the choice tile prefab is saved with
`sizeDelta = (100, 100)`, every instantiated tile rendered at ~100 px.
Compounded by:

| Symptom | Cause |
|---|---|
| Tiles render in narrow column | `childControlWidth = 0` + tile saved `sizeDelta.x = 100` |
| Labels split one-word-per-line | Word-wrap kicked in but width was only 100 px |
| Tiles bunched in centre | `childAlignment = MiddleCenter` |
| Narration line still visible under choices | `PresentChoices()` never hid `lineText` |
| `LayoutElement.preferredWidth` missing | Only `preferredHeight = 62` set on tile |

### Fix (one PR — touches 5 files, adds 2 new files)

| File | Role | Status |
|---|---|---|
| `Scripts/UI/DialogueChoiceLayoutHealer.cs` | **NEW**. Runtime self-heal helper with `HealContainer(Transform)` + `HealTile(GameObject)`. Enforces `childControlWidth = true` + `childControlHeight = true` on the VLG, and `LayoutElement.minHeight = 56 / preferredHeight = 64 / flexibleWidth = 1` on each instantiated tile. Also fixes `RectTransform.anchorMin.x = 0 / anchorMax.x = 1 / sizeDelta.x = 0` so a clone never flashes at 100×100 before the VLG repositions it. Resets the tile's TMP label to word-wrap + auto-size + ellipsis. Idempotent. | NEW |
| `Scripts/UI/DialogueUI.cs` | `Awake()` heals container. `PresentChoices()` heals container again + heals each instantiated tile + **hides `lineText` while choices are on screen** (and `PresentLine()` / `HandleChoice()` / `Hide()` re-show it). New `Update()` handler maps **`1`-`4` keys to choice indices** so players can advance without hunting for tiles with the cursor. Labels are prefixed with `<b>[1]</b>` etc. so the shortcut is discoverable. | updated |
| `Scripts/UI/ChoiceCardUI.cs` | Same `HealContainer` / `HealTile` calls for the moral-choice card (memory tariff tiles) — identical root cause. | updated |
| `Scripts/Editor/Phase14_BamaoUIBuilder.cs` | `BuildDialogueBox` now writes `childControlWidth = true / childControlHeight = true / childAlignment = UpperCenter` on the VLG. `MakeChoiceTileVisuals` pre-shapes the tile's `RectTransform` to full-width + writes `LayoutElement.minHeight = 56 / preferredHeight = 64 / flexibleWidth = 1`. Fresh Phase 14 builds bake the fix into the saved prefab. | updated |
| `Scripts/Editor/Phase31_DialogueChoiceCardRepair.cs` | **NEW**. `Hearthbound → 🧰 Phase 31 — Repair Dialogue Choice Cards`. Walks `UI_DialogueBox_Bamao.prefab`, `UI_ChoiceTile_Bamao.prefab`, and every gameplay scene; surgically repairs the VLG + LayoutElement + label settings IN PLACE so users do **not** need to re-run Phase 14 (which would lose inspector tweaks). Idempotent — safe to re-run. | NEW |
| `Scripts/Editor/Phase27_BuildEverything.cs` | Master capstone now chains Phase 31 after Phase 30. Seven sub-capstones in one click. | updated |

### What the user does after pulling

```
1. Pull feat/mission-1-2-architecture.
2. Unity recompiles (~10 s) and re-imports the modified prefabs.
3. Hearthbound → 🚀 Build Everything.
     ↳ This now chains Phase 31 at the end and surgically repairs the
       saved DialogueBox + ChoiceTile prefabs + the four gameplay scenes.
   (or just run Phase 31 directly: Hearthbound → ⚙️ Advanced → 🧰 Phase 31 — Repair
    Dialogue Choice Cards.)
4. Press Play in 00_Bootstrap. Walk to Doris.
     ↳ Both choice tiles are now full-width parchment scrolls with
       readable labels prefixed [1]/[2]/[3]. Click or press 1/2/3 to
       advance. The narration line hides while choices are visible.
```

The runtime self-heal in `DialogueUI.Awake()` + `PresentChoices()` means
the fix lands **even on existing saves that pre-date the Phase 31
prefab rebuild** — the moment the user walks up to Doris, the container
+ tiles are healed on the live UI before the first tile is spawned.

### Decisions adopted

- **D-045 (NEW):** Any `VerticalLayoutGroup` / `HorizontalLayoutGroup`
  used to spawn variable-width children **MUST** set both
  `childControlWidth = true` and `childForceExpandWidth = true`. The
  `childForce` flag alone is insufficient — it only redistributes
  leftover space without resizing. Codified in
  `DialogueChoiceLayoutHealer.HealContainer()`.
- **D-046 (NEW):** Choice tile prefabs **MUST** set `LayoutElement`
  with `minHeight ≥ 56` (mobile finger-tap budget), `preferredHeight ≥ 64`
  (cozy default), and `flexibleWidth = 1` (share horizontal slack).
  Codified in `DialogueChoiceLayoutHealer.HealTile()`.
- **D-047 (NEW):** `DialogueUI` MUST hide `lineText` while choices are on
  screen and restore it on the next `PresentLine()` / `HandleChoice()` /
  `Hide()`. The narration body and the choice body share the same
  rectangle by design.
- **D-048 (NEW):** Player-facing UI with up to 4 buttons should expose
  number-key shortcuts (1/2/3/4) and prefix labels with `[N]`. Mouse-only
  flows have an accessibility tax; the cost of the shortcut is one
  `Update()` poll per dialogue frame.

### How this lesson is now in the architecture

- **D-035 reinforced** (asmdef-locality) — `DialogueChoiceLayoutHealer`
  lives in the `HearthboundHollow.UI` asmdef next to `DialogueUI` and
  `ChoiceCardUI`. The Phase 31 editor capstone is in
  `HearthboundHollow.Editor` and uses `UI` references already declared
  in the asmdef.
- **D-041 reinforced** (runtime auto-correct over build-time guessing) —
  same pattern as `PlayerGroundClamp`: the prefab gets it right, but
  the runtime healer recovers any legacy or third-party-mutated state.

---

## 🚨 HOTFIX — Phase 30.1 (2026-05-25) — Mission asmdef missing `Unity.TextMeshPro`

**Bug reported by user after pulling Phase 30:**

```
Assets/_Project/Scripts/Mission/ControlHintsHUD.cs(27,7):
  error CS0246: type 'TMPro' not found
Assets/_Project/Scripts/Mission/ControlHintsHUD.cs(44,16):
  error CS0246: type 'TextMeshProUGUI' not found  (×6)
```

### Root cause

`ControlHintsHUD.cs` (added in Phase 30) is the FIRST file in the `HearthboundHollow.Mission` asmdef to use TMPro types directly. Every prior Mission file (`Mission01Director`, `Mission02Director`, `MarinNoteInteractable`, `NpcAnimatorBridge`, `PlayerFootstepBinder`, etc.) only used UI's wrapper components (`DialogueUI`, `EveningLedgerUI`, …) and never touched `TextMeshProUGUI` itself — so the missing `Unity.TextMeshPro` reference was latent until Phase 30.

**Asmdef references are not transitive.** Even though `HearthboundHollow.Mission` references `HearthboundHollow.UI`, and `HearthboundHollow.UI` references `Unity.TextMeshPro`, Mission does not automatically inherit TMPro symbols.

### Fix (1 commit)

Append `"Unity.TextMeshPro"` to the Mission asmdef's `references` array. One-line change:

```jsonc
"references": ["…","Unity.Addressables","Unity.TextMeshPro"]
```

### Audit performed to prevent recurrence

Walked every new file added in Phase 28-30 and verified its `using` directives are matched by an entry in the owning asmdef's references:

| File | Asmdef | Status |
|---|---|---|
| `Player/PlayerGroundClamp.cs` | Player | ✅ |
| `Player/PlayerController.cs` (updated) | Player | ✅ |
| `UI/UIAutoFitText.cs` | UI | ✅ |
| `UI/OnboardingOverlay.cs` | UI | ✅ |
| `Mission/ControlHintsHUD.cs` | Mission | ❌ → ✅ (this fix) |
| `Editor/Phase30_OnboardingAndHintsCapstone.cs` | Editor | ✅ |
| `Core/VillageState.cs` (updated) | Core | ✅ |

### How this lesson is now in the architecture

- **D-035 reinforced** — When a new file pulls in a new namespace, the owning asmdef's references MUST be audited before pushing. The previous Phase 26.1 hotfix only caught the case of `HearthboundHollow.X` cross-namespace references; this hotfix extends the same discipline to Unity built-in packages (`Unity.TextMeshPro`, `Unity.InputSystem`, etc.).
- The Phase 26 diagnostic already verifies the Player prefab Animator wiring; Phase 31 could extend it to a pre-push asmdef-locality check that walks every C# file's `using` directives against its owning asmdef.

---

## 🆕 Phase 30 — Onboarding + Control Hints HUD  🟢 (2026-05-25)

**User request after the first playtest:**

> *"Create the onboarding and control guidance UI so enhance the whole gameplay."*

### What shipped

| File | Role | LOC |
|---|---|---|
| `Assets/_Project/Scripts/UI/OnboardingOverlay.cs` | **NEW**. 6-step multi-card walkthrough (Welcome → WASD → E → LMB polish → Pause/Help → Ready). Data-driven `Step[]`, optional input expectations (auto-advance on `press_wasd` / `press_e` / etc.), skippable from frame 1, pauses `Time.timeScale` while open. | 350 |
| `Assets/_Project/Scripts/Mission/ControlHintsHUD.cs` | **NEW**. Always-visible parchment chip strip (Move · Interact · Help) at bottom-left of every gameplay scene. The [E] chip emphasises to full alpha + swaps caption to the interactable's `PromptText` when one is in range. | 155 |
| `Assets/_Project/Scripts/Editor/Phase30_OnboardingAndHintsCapstone.cs` | **NEW**. `Hearthbound → 🎓 Phase 30 — Build Onboarding + Hints HUD` — idempotent Editor builder that drops the OnboardingOverlay on the Lane scene canvas and the ControlHintsHUD on every gameplay scene. | 380 |
| `Assets/_Project/Scripts/Core/VillageState.cs` | Added `onboardingCompleted` bool (cleared by `ResetToDefault()` so fresh saves see the walkthrough). | (updated) |
| `Assets/_Project/Scripts/Editor/Phase27_BuildEverything.cs` | Master capstone now chains Phase 29 + Phase 30. Six sub-capstones in one click. | (updated) |

### Behaviour

- **First-time players** see the 6-step OnboardingOverlay on top of the Lane scene as soon as the cursor warms up. They can press `Next →` / Space / Enter to advance, or `Skip Tutorial` / Esc to dismiss.
- **Step 2 (WASD)** auto-advances after the player presses any WASD or arrow key, so the walkthrough feels responsive rather than read-it-and-click.
- **VillageState.onboardingCompleted** persists with the save — the overlay never re-shows on subsequent sessions of the same save.
- The **ControlHintsHUD** sits at the bottom-left at `alpha = 0.45` (readable but not loud) until an interactable comes into range, then the [E] chip flicks to `alpha = 1` and the caption becomes the interactable's prompt ("Greet Doris", "Polish memory", "Enter the Hollow", …).

### Decisions adopted

- **D-043 (NEW):** Onboarding is **per-save** (gated by `VillageState.onboardingCompleted`), not per-play. Fresh saves repeat the walkthrough by design.
- **D-044 (NEW):** Context-aware HUD chips live in the **Mission** asmdef (not UI), because they query `PlayerController.CurrentFocus` and UI asmdef has no Player reference. Codifies D-035 for the new HUD.

---

## 🆕 Phase 29 — UI Polish (text-never-clips) + Player Rig Doctor  🟢 (2026-05-25)

**User report after the first Phase 27 playtest:**

> *"The cards and UI is not appearing well and text is cut."*
> *"Half body still sunk in floor after Phase 27.2."*

### Two parallel threads

**29a — UIAutoFitText + word-wrap on every cozy UI label.**

| File | Change |
|---|---|
| `Scripts/UI/UIAutoFitText.cs` (NEW) | Single helper + two static methods (`ApplyToLabel`, `ApplyToButtonLabel`) that force `enableWordWrapping = true` + `enableAutoSizing` between configured min/max + `OverflowMode.Ellipsis` fallback. |
| `Scripts/UI/DialogueUI.cs` | Awake() autofit on `lineText` (16–28) + `speakerName` (18–32). |
| `Scripts/UI/ChoiceCardUI.cs` | Awake() autofit on header; `WireTile()` autofit on every instantiated tile. |
| `Scripts/UI/EveningLedgerUI.cs` | Awake() autofit on day / coin / summary / heldMemories / save-slot labels. |
| `Scripts/UI/HelpOverlayUI.cs` | Awake() autofit on title / subtitle / body. |
| `Scripts/UI/ToneCompassCard.cs` | Awake() autofit on bodyText + gentleModeLabel. |
| `Scripts/Editor/Phase14_BamaoUIBuilder.cs` | **Critical**: relocates the DialogueBox `ChoicesContainer` from off-prefab-bounds (1.05–2.10 anchorY = ABOVE the prefab) to INSIDE the dialogue box (0.08–0.78). This was the root cause of choice tiles rendering off-screen. Also applies autofit at prefab build time. |
| `Scripts/Editor/Phase23_Mission1PolishCapstone.cs` | UIAutoFitText applied to every procedurally-built TMP in the PauseMenu / HelpOverlay / Settings panel / MissionTitleCard / buttons / toggles / sliders. |

**29b — Player Rig Doctor (foot-bone anchor fix).**

Phase 28 switched to live `Renderer.bounds.min.y` after Animator update, fixing the worst cases. But some BoZo CharacterCreator variants still left a 5–30 cm residual sink because the world AABB is padded for culling. Phase 29's **`Phase29_PlayerRigDoctor.cs`** auto-discovers a foot bone (Mixamo / BoZo / generic humanoid naming heuristics) and wires it as the `PlayerGroundClamp.footAnchor`, which the clamp prefers over bounds scanning. Result: a surgical, foolproof anchor at the actual toe position.

Also:
- Force-disables `applyRootMotion` on every Player Animator.
- Verifies the Animator's Avatar is set to a Humanoid avatar.
- Sanity-checks the GameObject scale chain.
- Auto-adds a Ground BoxCollider to scenes that don't have one.

### Decisions adopted

- **D-041 (Phase 28 amendment)** — Mesh-bottom MUST be measured from world-space `Renderer.bounds`, never from `SkinnedMeshRenderer.localBounds`. Padded culling AABBs make localBounds unreliable on most rigs.
- **D-042 (NEW)** — Any TMP label created by a builder script MUST go through `UIAutoFitText.ApplyToLabel` / `ApplyToButtonLabel` before the prefab is saved.

---

## 🆕 Phase 28 — Definitive body alignment (live world bounds + continuous window)  🟢 (2026-05-25)

**User reported during first playtest that the Phase 27.2 fix didn't fully resolve the sink** — half the body was *still* in the floor on a BoZo CharacterCreator rig.

### Root cause

`SkinnedMeshRenderer.localBounds` on those rigs is a **padded culling AABB**, not the actual bind-pose bottom. It's sized big enough to contain any stretched pose so the renderer is never frustum-culled mid-animation. The Phase 27.2 clamp read this padded bottom and consequently lifted the body by *less* than the rig actually needed — leaving the visible feet ≈ 30-50 cm in the floor.

### Fix (2 commits — both `PlayerGroundClamp.cs` and `PlayerController.cs` upgraded symmetrically)

1. **Switch to live world bounds**: `Renderer.bounds.min.y` (post-Animator pose) instead of `SkinnedMeshRenderer.localBounds`. This reflects the *actual* visible feet position right now.
2. **Continuous correction window** — runs alignment every frame for the first 0.75 s of play. The Animator's bind→idle blend can take several frames to settle and a Mixamo clip with a baked initial offset can leave a residual mismatch; one-shot LateUpdate alignment wasn't enough on slower idle clips.
3. **Configurable tolerance** (default 0.5 cm) to prevent FP chatter.
4. **Optional `footAnchor` Transform override** — drag a toe bone for surgical precision on rigs with weird padded localBounds. Phase 29's Player Rig Doctor auto-discovers this.
5. **Filter out non-mesh renderers** (ParticleSystemRenderer / LineRenderer / TrailRenderer) — their huge AABBs would corrupt the min-Y scan.

---

## 🚨 HOTFIX — Phase 27.1 (2026-05-25) — "Half body in floor" sink (superseded by Phase 28)

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
2.  Hearthbound → 🚀 Build Everything.   ← re-runs Phase 26
3.  (Optional) Hearthbound → 🔍 Diagnose Build.        ← verify the clamp landed
4.  Press Play. The character now stands on the floor.
```

The runtime auto-correct in `PlayerGroundClamp` will recover the visible feet position even on scenes that pre-date this fix — the only requirement is that the component is attached to the Player, which the Phase 26 capstone takes care of.

### How this lesson is now in the architecture

- **D-041 (NEW):** Character prefab CC centre must align with the BoZo mesh origin. When the mesh origin is at the character's hips (not feet), use a runtime ground-clamp instead of guessing the offset at prefab build time. Build-time defaults are best-effort; runtime auto-correct is the truth.
- The Phase 26 diagnostic now FAILS (not just WARNs) when the Player has no `PlayerGroundClamp` — this regression class won't ship again.

---

## 🆕 Phase 27 — Build EVERYTHING + Phase 26 Polish Layer  🟢

**Single-click master capstone + NPC animator pipeline + diagnostic + footstep SFX hooks.**

> Single-menu rebuild: `Hearthbound → 🚀 Build Everything` (Phase 32 renamed; was `Hearthbound → ✨ Build EVERYTHING (Phase 27 — one click)`)

This phase consolidates the two parallel Phase 26 workstreams into a single user-facing menu and adds the remaining polish layer (NPC dialogue body language, audit, footstep audio hooks).

### Files added (6 new — pushed one-per-commit)

| File | Role | LOC |
|---|---|---|
| `Assets/_Project/Scripts/Editor/Phase27_BuildEverything.cs` | Master capstone — chains Phase 23 + 26 (PC+Anim) + 26 (NPC) + 26 (Narrative Hooks) in one menu click. Reflection-based discovery means missing phases skip gracefully. Phase 32 promoted this to `🚀 Build Everything`. | 200 |
| `Assets/_Project/Scripts/Editor/Phase26_DiagnosticReport.cs` | Read-only audit menu — verifies AnimatorController asset + Player prefab Animator wiring + per-scene SmoothFollowCamera + cameraReference + **PlayerGroundClamp (Phase 27.1 hotfix)**. | 200 |
| `Assets/_Project/Scripts/Editor/NpcAnimatorControllerBuilder.cs` | Builds `Hearthbound_NPC.controller` — Idle ↔ Talking states + `IsTalking` bool + `Speed` float, auto-picks BoZo F_Idle. | 130 |
| `Assets/_Project/Scripts/Editor/Phase26_NpcAnimatorCapstone.cs` | Wires Doris/Gerrold/SilentLane prefabs with the NPC controller + an NpcAnimatorBridge component. | 130 |
| `Assets/_Project/Scripts/Mission/NpcAnimatorBridge.cs` | Runtime — listens to `DialogueStartedEvent` / `DialogueEndedEvent` and toggles `Animator.IsTalking`. | 100 |
| `Assets/_Project/Scripts/Mission/PlayerFootstepBinder.cs` | Runtime — Animation Event hooks (`OnFootstepLeft`/`OnFootstepRight`) play surface-aware footstep SFX through `SfxPlayer.PlayOneShot(id, volume)`. | 160 |
| **`Assets/_Project/Scripts/Player/PlayerGroundClamp.cs`** | **NEW (Phase 27.1 hotfix)** — Runtime auto-aligns BoZo mesh feet to CharacterController capsule bottom; fixes the "half body in floor" sink. | 230 |

### Menu items added (post-Phase 32 collapse — see migration table above)

| Menu Path (current) | Purpose |
|---|---|
| `Hearthbound → 🚀 Build Everything` | Master capstone — chains every Phase 23/26/29/30/31/32 sub-builder + opens Bootstrap |
| `Hearthbound → 🔍 Diagnose Build` | Phase 33 aggregate diagnostic (runs Phase 23 + 26 + 32 diagnostics) |
| `Hearthbound → ⚙️ Advanced → 🔍 Diagnose Phase 26 Build` | Just the Phase 26 read-only audit |
| `Hearthbound → ⚙️ Advanced → 🎭 Phase 26 — Wire NPC Animators` | NPC controller + Doris/Gerrold/SilentLane wiring |
| `Hearthbound → ⚙️ Advanced → Phase 26 — Build NPC Animator Controller` | Just the NPC controller asset |

### Decisions adopted in Phase 27 polish layer

- All Phase 27 additions follow D-001..D-040 (no new D-numbers).
- **D-041 (NEW, Phase 27.1 hotfix)** — Character prefab CC centre must align with mesh origin; use runtime ground-clamp when the mesh origin isn't at the feet.

### What the user now does

```
1.  Pull feat/mission-1-2-architecture. Wait for Unity recompile.
2.  Hearthbound → 🚀 Build Everything.                  ← single click, ~60 s
3.  (Optional) Hearthbound → 🔍 Diagnose Build.         ← audit the result
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

> Single-menu rebuild: `Hearthbound → ⚙️ Advanced → 🏃 Phase 26 — Player Controller + Animation` (or transitively via `Hearthbound → 🚀 Build Everything`)

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
- `Assets/_Project/Scripts/Editor/Phase26_NarrativeHooks.cs` — Editor menu `Hearthbound → ⚙️ Advanced → Phase 26 — Wire Narrative Hooks` that idempotently drops the Marin's Note prefab onto the Hollow workbench.

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

## 🎯 Current Status — POLISHED PLAYABLE M1 + M2 + ALL HOTFIXES + MENU COLLAPSE LANDED

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
| ✅ 27.1 / 27.2 | "Half body in floor" preliminary hotfixes (PlayerGroundClamp) | ✅ Done |
| ✅ 28 | Definitive body alignment — live world bounds + continuous correction window | ✅ Done |
| ✅ 29a | UIAutoFitText + word-wrap on every cozy UI label + ChoicesContainer reposition | ✅ Done |
| ✅ 29b | Player Rig Doctor — foot-bone anchor auto-discovery + Animator sanity pass | ✅ Done |
| ✅ 30 | OnboardingOverlay (6-step walkthrough) + ControlHintsHUD (persistent chips) | ✅ Done |
| ✅ 30.1 | Hotfix — Mission asmdef missing `Unity.TextMeshPro` (6× CS0246 compile errors) | ✅ Done |
| ✅ 31 | Dialogue Choice Card Repair — full-width tiles + 1/2/3/4 keyboard shortcuts | ✅ Done |
| ✅ 31.1 | "Press [Space] ▸" advance prompt + DreamCanvas auto-hide | ✅ Done |
| ✅ 32 (Mission 1 polish v2) | 8-cottage village + Hollow facade + hearth dressing + cozy URP volumes | ✅ Done |
| ✅ 32 (Menu collapse UX track) | 🚀 Build Everything is the only top-level entry the user needs | ✅ Done |
| ✅ **32 (Voice Acting MVP)** | **Doris's M1 dialogue voiced via macOS `say -v Samantha`; VoiceLibrarySO + VoicePlayer + DialogueUI hook + D-058** | ✅ **Done — this PR** |

The project ships behind a **single menu click**: `Hearthbound → 🚀 Build Everything`. The chain runs Phase 13 → 32 in order, idempotent, ~60 s end-to-end. A read-only `Hearthbound → 🔍 Diagnose Build` audit is the second top-level entry. Every other phase lives under `Hearthbound → ⚙️ Advanced ►` for power users.

---

## Decisions Made (D-001 → D-058)

D-001..D-057 cover BoZo art, asmdef discipline, UI two-layer + self-heal, asmdef-locality, sprint/jump opt-in, Animator + camera defaults, animation locations, ground-clamp, autofit, onboarding-per-save, dialogue layout/affordance, cutscene visibility, menu collapse, audio + cutscene policy, save-resume + install-pattern policy, and audio self-heal.

- **D-049 (Phase 31.1)** — Blocking dialogue UI must expose a visible advance affordance. Codified in `DialogueUI.advancePrompt`.
- **D-050 (Phase 31.1)** — Cutscene overlays must be hidden by default; full-screen non-active raycasters must zero their `raycastTarget`.
- **D-051 (Phase 32 UX track)** — Every editor action MUST register under `Hearthbound/⚙️ Advanced/…` unless explicitly promoted to top level. The top-level menu is reserved for the three blessed user entry points (`🚀 Build Everything`, `🔍 Diagnose Build`, and the implicit `⚙️ Advanced ►` submenu).
- **D-052 / D-053 / D-054** *(Phase 39 — Audio + Cutscene policy)* See `Docs/Phase39_Greenlight_Signoff.md`.
- **D-055 / D-056** *(Phase 44 — Save-resume + install-pattern policy)* See `Docs/Phase44_Polish_Layer_Signoff.md`.
- **D-057** *(Phase 45 — Audio self-heal)* Every audio component that depends on a ScriptableObject library MUST have a `Resources.Load` self-heal fallback in `Awake()` AND log a clear error if the fallback also fails (with remediation step).
- **D-058 (NEW, Phase 32 — Voice Acting MVP)** — Voice clips live under `Assets/_Project/Audio/Voice/{character}/{lineId}.wav`; the generation pipeline (e.g. macOS `say`) is decoupled from the runtime — any TTS that produces 22 kHz mono PCM16 .wav can drop in (ElevenLabs / XTTS / Piper / human VO). The `VoiceLibrarySO` re-binds them on the next `OnValidate` / `Phase32_VoiceLibraryBuilder` rescan.

See `CHANGELOG.md` for per-release decision tables.

---

## 🛠️ Editor Menu Items Available (cumulative — post-Phase-32 collapse)

**Top level — exactly three entries:**

| Menu Path | Purpose | Phase |
|---|---|---|
| **`Hearthbound → 🚀 Build Everything`** | **🎉 MASTER: chains every Phase 23/26/29/30/31/32 sub-capstone in one click (with safety dialog)** | **32 (was 27)** |
| **`Hearthbound → 🔍 Diagnose Build`** | **Aggregate diagnostic (chains Phase 23 + 26 + 32 sub-diagnostics)** | **33** |
| **`Hearthbound → ⚙️ Advanced ►`** | **Power-user submenu containing every legacy per-phase item (40+ entries)** | **32** |

**Under `⚙️ Advanced ►` (alphabetised by emoji + priority):**

| Menu Path (under ⚙️ Advanced) | Purpose | Phase |
|---|---|---|
| `🎓 Phase 30 — Build Onboarding + Hints HUD` | OnboardingOverlay on Lane + ControlHintsHUD on every gameplay scene | 30 |
| `🎙️ Phase 32 — Rebuild Voice Library` *(NEW)* | Scans Audio/Voice/**/*.wav, rebuilds Resources/HearthboundVoiceLibrary.asset | 32 (Voice MVP) |
| `🦶 Phase 29 — Player Rig Doctor` | Foot-bone anchor + root-motion sanity + ground-collider audit | 29 |
| `🏃 Phase 26 — Player Controller + Animation` | Player AnimatorController + camera + scene wiring + ground clamp | 26 (PC+Anim) |
| `🎭 Phase 26 — Wire NPC Animators` | NPC AnimatorController + Doris/Gerrold/SilentLane wiring | 27 (NPC) |
| `Phase 26 — Build Player Animator Controller` | Just the player controller asset | 26 (PC+Anim) |
| `Phase 26 — Build NPC Animator Controller` | Just the NPC controller asset | 27 (NPC) |
| `Phase 26 — Wire Narrative Hooks` | Drops Marin's Note onto the workbench | 26 (Narrative) |
| `🎮 Build POLISHED Mission 1 + 2 (Phase 23)` | Polished playable Mission 1 + 2 | 23 |
| `🎮 Build POLISHED Playable Mission 1 (Phase 22)` | Engineering Mission 1 only | 22 |
| `Build Playable Mission 1 (One Click)` | Phase 12 MVP smoke-test | 12 |
| `Phase 13 — Build BoZo Character Prefabs` | BoZo character wrappers (now with PlayerGroundClamp on Player) | 13 |
| `Phase 14 — Build Bamao UI Prefabs` | Bamao parchment UI prefabs | 14 |
| `Phase 15 — Build Medieval Village dressing` | Cottage/fence/well/tree prefab lookups | 15 |
| `Phase 18 — Build SFX Library` | Auto-populate SfxLibrarySO | 18 |
| `Phase 24 — Build Mission 2 Scenes` | Garden + Cottage scene builders | 24 |
| `🌳 Phase 27.2 — Polish Lane Environment` | Lane v1 cobble path + cottages + foliage + door upgrade | 27.2 |
| `🏠 Phase 27.3 — Polish Hollow Interior` | Hollow v1 walls + hearth + workbench + shelves | 27.3 |
| `🍂 Phase 32 — Polish Mission 1 (v2 — all phases)` | Mission 1 polish v2 capstone | 32 |
| `🧰 Phase 32.1 — Catalog Extended Village Bindings` | Extended MV bindings SO | 32.1 |
| `🏘️ Phase 32.1 — Assemble Cottage Prefabs` | 4 cottage prefab variants | 32.1 |
| `🏘️ Phase 32.2 — Polish Lane Environment V2` | 8 cottages + Hollow facade + atmosphere | 32.2 |
| `🏠 Phase 32.3 — Polish Hollow Interior V2` | Kettle + bread + herbs + cupboard + candelabra + sconces | 32.3 |
| `🌅 Phase 32.4 — Apply Cozy URP Volume` | Lane + Hollow volume profiles + global volumes | 32.4 |
| `🧰 Phase 31 — Repair Dialogue Choice Cards` | Surgical in-place VLG + tile repair | 31 |
| `🔍 Diagnose Phase 23 Build` | Read-only audit of Phase 23 scenes | 23 |
| `🔍 Diagnose Phase 26 Build` | Read-only audit of Phase 26 animator + camera + ground clamp wiring | 27 |
| `🔍 Phase 32 — Diagnose Mission 1 Polish` | Read-only audit of Phase 32 v2 polish | 32 |
| `Setup URP Pipeline (one-time)` | Activate URP | 10.7 |
| `Check Render Pipeline Status` | Read-only URP status | 10.7 |
| `Create Mission 1-2 Seed Assets` | Spawn the 17 SOs | 11 |
| `Validate Mission 1-2 Seed Assets` | Audit missing seeds | 11 |
| `Patch ASE Shaders Now` | BMLitShaderPatcher one-shot | misc |

---

## How to run (recommended: single click)

1. Pull `feat/mission-1-2-architecture`.
2. Wait for Unity recompile (~5–10 s).
3. **`Hearthbound → 🚀 Build Everything`** → click **`Build`** in the confirmation dialog.
4. Sit back ~60 s while Phase 13 → 32 runs.
5. (Optional) **`Hearthbound → 🔍 Diagnose Build`** to verify wiring.
6. **Phase 32 Voice MVP (NEW)** — on macOS, run `bash Tools/generate_voices.sh` once (generates 48 .wav files for Doris). Then in Unity click `Hearthbound → ⚙️ Advanced → 🎙️ Phase 32 — Rebuild Voice Library` to auto-bind the clips. Skip these steps on non-macOS — the game runs silently on the voice channel and the typewriter still works.
7. Press **Play**.

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
| **Phase 27.1 / 27.2** | Player half-sunk into floor — preliminary fixes (PlayerGroundClamp + Phase 13 CC re-tune) | **Visual** | ✅ **Superseded by Phase 28** |
| **Phase 28** | Half-body sink — STILL occurring on BoZo CharacterCreator rigs because Phase 27 used padded localBounds | **Visual** | ✅ **Fixed — live world bounds + continuous correction window** |
| **Phase 29a** | Cards & UI text appearing clipped on smaller canvases | **Visual** | ✅ **Fixed — UIAutoFitText on every TMP label + DialogueBox ChoicesContainer relocation** |
| **Phase 29b** | Residual sink on rigs with padded culling AABBs | Cosmetic | ✅ **Fixed — Player Rig Doctor wires footAnchor to the actual toe bone** |
| **Phase 30** | No onboarding for new players; controls discoverable only via H | Player-experience | ✅ **Fixed — OnboardingOverlay (6-step walkthrough) + ControlHintsHUD (always-visible chips)** |
| **Phase 30.1** | Mission asmdef missing `Unity.TextMeshPro` → CS0246 ×7 on `ControlHintsHUD.cs` | **Compile error** | ✅ **Fixed — appended `Unity.TextMeshPro` to Mission asmdef refs; D-035 audit performed for every Phase 28-30 file** |
| **Phase 32 (UX track)** | Hearthbound menu had ~25 flat entries; no single "press this after pull" affordance | **UX** | ✅ **Fixed — top-level collapsed to 🚀 / 🔍 / ⚙️ Advanced; safety dialog on 🚀 Build Everything** |
| **Phase 32 (Voice MVP)** | Doris's M1 dialogue was silent typewriter only | **UX** | ✅ **Fixed — 48 macOS `say` clips, lip-sync-feel typewriter pacing, D-058** |
| **P32-IDEMP-1** | OneClickSetup uses `NewScene(NewSceneMode.Single)` on scenes 00-03 — destructive by design | Low | 🟡 Open — see Phase 32 idempotency audit table above |
| **P32-VOICE-1** | 3 refused-path lines + dynamic afterPolishLine have no lineId; voice silent on those branches | Cosmetic | 🟡 Open — add 4 more clips in a follow-up |
| **Phase 47.1 (playtest)** | macOS trackpad zoom dead; slow polish circles unresponsive; Evening Ledger prose unreadable | **Playtest** | ✅ **Fixed — see Phase 47.1 below** |

---

## Phase 47.1 — Playtest polish (2026-05-29)

Three issues from a live playtest (Evening Ledger Day 1 screenshot + verbal report):

1. **macOS trackpad zoom "stopped working".** `SmoothFollowCamera.ReadZoomDelta()` fell back to `Input.mouseScrollDelta.y`, which the new Input System backend reports as **0** for the two-finger trackpad gesture. Now reads `Mouse.current.scroll` first, normalises per-backend magnitudes into "notch" units (wheel ÷120, trackpad ÷18, legacy ±1), clamps the per-frame jump, and eases `distance → targetDistance` via SmoothDamp (`zoomSmoothTime`, `zoomSensitivity`). Added `SetZoomDistance()` / `TargetDistance` so `PolishOrbHighlighter`'s dolly-in rides the smoothing instead of being snapped back.
2. **Slow polish circles felt unresponsive.** `motionThresholdNormalized` was `0.05` (≈96 px/frame at 1080p) — a deliberate slow circle never crossed it. Lowered to `0.0015` (≈3 px/frame); eased `clarityGainPerSecond` 0.014 → 0.006 so the circle lasts a satisfying few seconds instead of snapping to full. Idle guide-ring alpha bumped for visibility.
3. **Evening Ledger / panel prose "not visible, not clear".** `UIReadabilityHelper.ApplyBody` painted near-black ink **on top of** the dark wash every panel adds — dark-on-dark. Flipped body prose to bright cream with a strong dark outline (legible on the wash *and* bare parchment) and deepened `WashDark` alpha 0.40 → 0.74. Cascades to Evening Ledger, One More Day, Help, and Tone Compass.

Logged as **D-067**. No new compile deps; all edits are field-default + method-level changes to existing scripts. *(Originally mis-numbered "D-065" in this log, which collided with the Phase 53 Localization decision — corrected to D-067.)*

---

## Phase 53.1 — Input / click hardening (2026-05-29)

**Playtest blocker:** at the Day 1 Evening Ledger the player could not click any
button (Save slot, "Sleep — End Day"), nor any button inside the Help overlay
(opened with H). Keyboard still worked (H toggled the overlay), so the game was
**stuck — could not advance to the next stage.** Screenshots: Help overlay +
Evening Ledger Day 1.

**Root cause — invisible full-screen raycast eater.** Several cozy overlays put
their `CanvasGroup` on the *always-active* script-host (so `Update()` keeps
running for the toggle key) and hide only by fading `alpha → 0` and/or
deactivating a child `root` — while the serialized `CanvasGroup.blocksRaycasts`
stayed **true**. The `MissionTitleCard` (the only always-active
`blocksRaycasts: 1` object in `03_Mission01_Hollow`) is the lead suspect: its
play coroutine activates `root`, fades, then deactivates `root`. If that
coroutine is interrupted — host disabled, `Play()` re-entered, scene torn down —
a stopped Unity coroutine **cannot run a finally block**, stranding an
`alpha-0`, `blocksRaycasts: true`, full-screen `CanvasGroup`: an invisible click
eater over the entire scene. Mouse → UI dies; keyboard polling survives. Exactly
the reported symptom.

**Fix (D-068).** Every CanvasGroup-on-host overlay now *manages*
`blocksRaycasts` + `interactable` in code (false at `Awake`, true while
presenting, false when hidden) **and** runs a `LateUpdate` safety net enforcing
the invariant *"a fully transparent overlay must never block raycasts"*
(`alpha ≤ 0.001 ⇒ blocksRaycasts = false`). Applied to `MissionTitleCard`
(Hollow), `OneMoreDayCard` (the end-of-day beat right after the ledger) and
`PrefaceBeatUI` (Lane). Runtime edits — they heal the **already-built scenes
with no rebuild**.

Also fixed `Phase48_BootstrapHookCinematic.EnsureEventSystem` to create the
Bootstrap EventSystem with `InputSystemUIInputModule` under
`#if ENABLE_INPUT_SYSTEM` (it hard-coded the legacy `StandaloneInputModule`,
inconsistent with the other four scene builders — under an Input-System-only
backend that module reads no pointer input). Builder-only; effective on the next
`🚀 Build Everything`.

Logged as **D-068**. Progression doc updated:
`Docs/GAMEPLAY_GUIDE_MISSION_1.md` (end-of-day step notes the click-robust
ledger → goodnight → next-scene flow).

| **Phase 53.1 (playtest)** | Evening Ledger / Help buttons dead — game stuck at Day 1, can't advance | **Blocker** | ✅ **Fixed — overlay CanvasGroups never strand `blocksRaycasts`; D-068** |

---

*Last updated: 2026-05-27 — Phase 32 (Voice Acting MVP):*
- *48 Doris voice clips generated via `Tools/generate_voices.sh` (macOS `say -v Samantha -r 180`).*
- *9-file runtime + 1-file editor utility added. Architecture decoupled per D-058 — any 22 kHz mono PCM16 .wav drops in.*
- *DialogueUI's per-line typewriter pace locked to the clip duration for a lip-sync feel.*
- *Doc cascade: PROGRESS.md (this file), ARCHITECTURE.md (§ 4.6 Audio + D-058), CHANGELOG.md ([0.7.0-voice-acting-mvp]), README.md (Voice acting subsection).*
