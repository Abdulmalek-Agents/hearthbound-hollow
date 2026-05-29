# 🎬 STUDIO_LOG.md — Hearthbound Hollow

> **Living studio log.** Tracks phase completion, decisions, blockers resolved,
> asset-placement notes, QA sign-offs, and next steps. Newest entries on top.
> Companion to `Docs/PROGRESS.md` (technical changelog), `Docs/ARCHITECTURE.md`
> (decision ledger `D-0xx`), and `CHANGELOG.md` (release notes).
>
> **Engine:** Unity 6000.4.4f1 · **Pipeline:** URP-Mobile · **Target:** PC (Win64, Steam) + mobile-class perf discipline.
> **Branch:** `feat/mission-1-2-architecture` · **State:** narrative-complete vertical slice (M1 + M2) + Depth Layer (48–51) + One More Day hook (47-OMD).

---

## Legend
✅ Done & merged · 🟢 Done in branch (awaiting your pull + `🚀 Build Everything`) · 🟡 In progress · ⬜ Not started · 🔴 Blocked

---

## 🎙️🎭 Phase 53 + Voice Fix — Human Speech, Language, Reset & Character Creator 🟢 (2026-05-29)

**User request:** (1) dialogue voices pronounced the ellipsis "…" as "full stop"/"dot
dot dot" — make them sound human (any free AI voice OK, replace the WAVs to sound
native). (2) Add a Settings option to **reset the game** back to the home menu, and
to **select language (Arabic / English)**. (3) Add **character + clothes selection**
at the beginning. Make it very polished.

### Specialists convened
Audio/VO Engineer (lead on voice) · 2× C# Scripters · UX/UI Designer · 2× 2D/UI
Artists · Localization lead (EN/AR, RTL) · Technical Artist + 3D Modeler (avatar
tinting) · Build/DevOps · 3× Senior QA · 3× Market Critics (cozy-onboarding benchmark).

### 1) Voice — the "full stop / dot dot dot" fix ✅

**Root cause (QA + Audio Eng):** the Piper pipeline (`Tools/generate_voices.sh`)
already sanitises text via `clean_for_tts()`, but the **espeak-ng fallback**
(`Phase46_VoiceGenerator.cs`) — the one `🚀 Build Everything` actually runs — was
feeding **raw** line text to the engine, which verbalises punctuation literally
("…" → "dot dot dot", "—" → "dash").

**Fix (Phase 46.2):** ported `clean_for_tts` into C# (`CleanForTts` + `IsDirtySource`):
ellipses/dashes → natural comma pauses; parenthetical stage directions + `*emphasis*`
stripped; leading/trailing junk trimmed (terminal `. ! ?` kept for cadence);
pure-punctuation lines (e.g. `"..."`) become **voiceless** (the typewriter carries
the beat). Stale pre-sanitiser clips auto-purge + regenerate on the next Build
Everything. For fully **native neural** voices, the **Piper** pipeline (free,
open-source, D-059) remains the recommended path — `bash Tools/generate_voices.sh`.

### 2) + 3) Phase 53 — Polish Menu Layer (language · reset · character)

| Kind | Item | Status |
|---|---|---|
| Core | `LocalizationService.cs` — EN/العربية table, live `OnLanguageChanged`, RTL flag | 🟢 |
| Core | `SettingsService` — `Language` + character prefs (name/skin/outfit/accessory/created) + `ClearCharacterCreation()` | 🟢 |
| UI | `LocalizedText.cs` — TMP binder (refresh on language change + RTL flip) | 🟢 |
| UI | `CharacterCreationUI.cs` — skin/outfit/accessory swatches + name + live preview | 🟢 |
| UI | `SystemMenuUI.cs` — English/العربية toggle · Customize · Reset (in-panel confirm) | 🟢 |
| UI edit | `MainMenuController.cs` — `systemMenu` + New-Game `NewGameGate` hook | 🟢 |
| Mission | `CharacterAppearance.cs` — palette + applier (tints avatar, builds cap/flower/scarf) | 🟢 |
| Mission | `PolishMenuCoordinator.cs` — Save-aware bridge: reset, customize, New-Game gate | 🟢 |
| Editor | `Phase53_PolishMenuBuilder.cs` — builds the screens on Main Menu + Pause; chained as Build-Everything Step 16 | 🟢 |

**Flow:** New Game → (Tone Compass) → **Character Creator** (pick skin tone, outfit
colour, cap/flower/scarf, name) → first scene with the avatar wearing your look.
**Settings** (Main Menu or Pause → Settings) → **Language EN/العربية** (live) ·
**Customize Character** · **Reset Game** (in-panel confirm → wipes saves + resets
VillageState + clears the character → back to the title; **keeps** language/audio/comfort).

### Decisions (see ARCHITECTURE)
- **D-065** — Runtime UI localization via `LocalizationService` (key→{en,ar}); UI
  chrome localized + a language selector; hand-written dialogue prose stays canonical
  English (a dedicated writer translation pass is future work). Missing keys fall back
  → never blank. Language persists in `SettingsService` (PlayerPrefs).
- **D-066** — Character appearance is **all-procedural** (palette tints + code-built
  accessory) so no new art ships; persisted in PlayerPrefs (a player profile, not
  per-save). Reset keeps language + comfort, clears the character so New Game re-asks.

### asmdef / Cozy audit
- `LocalizationService` (Core), `LocalizedText`/`SystemMenuUI`/`CharacterCreationUI`
  (UI → Core only), `CharacterAppearance`/`PolishMenuCoordinator` (Mission → UI/Save/
  Core), builder (Editor). No cycles (D-035). Save stays out of UI (coordinator bridges).
- Reset uses a gentle in-panel confirm (no scary modal); copy is warm; nothing
  punishes. Zero new external packages; all visuals built-in UI sprites + tints.

### QA acceptance (verify after pull + Build Everything)
1. Dialogue no longer says "dot dot dot"/"full stop"; `"..."` lines are silent.
2. Settings → العربية flips UI chrome + right-aligns; → English flips back.
3. New Game opens the Character Creator; choices show on the avatar in-scene.
4. Settings → Reset Game → confirm → returns to title, Continue greyed, New Game re-asks character.
5. `🔍 Diagnose Build` clean; zero NRE booting → menu → game.

---

## 📌 Phase 47-OMD — "One More Day" Goodnight Beat 🟢 (2026-05-29)

**User request:** Read every doc + the `Docs/` and `Docs/Depth_Bible/` folders on
`feat/mission-1-2-architecture`, understand the project, create `STUDIO_LOG.md`,
implement `Docs/Phase47_OneMoreDay_Implementation.md` (adding/removing to make the
game more hooky, fun and engaging via a review cycle), fix issues found, polish the
environment + asset placement, and push every phase to the same branch.

### Specialists convened (24)

| Discipline | Named specialists on this phase |
|---|---|
| **Lead** | Lead Unity Architect (owns the night-chain contract + zero-regression gate) |
| **Engineering** | 3× Senior Unity Devs, 2× C# Scripters, 1× Build/DevOps (Phase27 chain), 1× Package/asmdef Expert (graph audit) |
| **UI / UX** | UX/UI Designer, 2× 2D/UI Artists (parchment card layout, contrast pass) |
| **Cinematic & Presentation** | 1× Cutscene Director (night-order lock), 1× Camera Expert (fade feel), 1× Lighting Expert (night palette) |
| **Design & Narrative** | 2× Game Designers (retention loop), Esme Cordray (forward-look prose), Mochi Tannenbaum (Pickle sign-offs), 1× Narrative Director |
| **Quality + Environment** | 3× Senior QA Testers, 1× Technical Artist, 1× 3D Modeler (joint environment/asset-placement pass) |
| **Market & Community Critics** | 3× Market Critics (Stardew sleep-transition / cozy-retention benchmark review) |

### Design intent (why this ships)

The day previously ended on a *bookkeeping* beat (Evening Ledger → save slot →
cut to next scene). There was no warm, forward-looking moment — the engine of the
cozy **"one more day"** hook (Stardew's sleep-transition, at our memory register).
We add a fully Cozy-Contract-compliant Goodnight Card: **no numbers, no fail
state, fully skippable, gentle fade only, Gentle-Mode safe**, with a refusal-path
variant that gets its own goodnight.

**Night order (locked by the Cutscene Director):**
`Evening Ledger confirm → Dream (if any) → Goodnight Card → next scene.`

### What shipped (Tier 1)

| Kind | Item | Status |
|---|---|---|
| New runtime | `Scripts/UI/OneMoreDayCard.cs` — presentational parchment overlay (UI asmdef) | 🟢 |
| New runtime | `Scripts/Mission/TomorrowTeaseSO.cs` — per-day data (Mission asmdef) | 🟢 |
| New runtime | `Scripts/Mission/EndOfDaySequencer.cs` — owns Ledger→Dream→Card→load | 🟢 |
| New editor | `Scripts/Editor/Phase47_OneMoreDayBuilder.cs` — idempotent builder + Build-Everything step | 🟢 |
| Edits | `Mission01Director.cs` · `Mission02Director.cs` · `MissionRunner.cs` — opt-in delegation, guarded | 🟢 |
| Data | `ScriptableObjects/Missions/Tomorrow_M1_Day1.asset` + `Tomorrow_M2_Day2.asset` (hand-authored, builder-healed) | 🟢 |
| Prose | `Yarn/EveningLedger.yarn` (+3 nodes) · `Yarn/Pickle.yarn` (+2 nodes) | 🟢 |
| Docs | `PROGRESS_Phase47_OneMoreDay.md` (supplement) · `ARCHITECTURE.md` (D-064) · `CHANGELOG.md` · this log | 🟢 |

**Zero-regression guarantee:** every runtime edit is guarded by
`if (endOfDaySequencer != null)`. With the sequencer unwired, day-end runs the
exact legacy code path. The builder wires it on `🚀 Build Everything`.

### Review cycle — what we *changed* vs. the implementation guide (and why)

The guide is a strong blueprint, but the Senior Devs + QA found three integration
gaps against the *actual* codebase and one accessibility gap. We fixed them and
documented the deviations (all hold the Cozy Contract):

1. **Day-index off-by-one (latent bug, fixed).** The guide resolved the tease by
   comparing `afterDayIndex` to `VillageState.currentDayIndex` directly. But
   `currentDayIndex` is **0-based** and is only incremented by `GameManager.EndDay()`
   *after* the card resolves (so it is `0` during M1, `1` during M2 at card time).
   The card would therefore **never resolve**. Fix: `EndOfDaySequencer.ResolveTease()`
   matches against `currentDayIndex + 1` (the fiction Day, matching
   `MissionTitleCard`'s convention) **and** a single-tease sequencer always returns
   its one tease — the beat can never silently vanish.
2. **Mission 2 double-dream (avoided).** In `Mission02Director`, Dream 2 already
   plays during the cleanse outro (`PostCleanseFlow` / `OpenLedgerListen` /
   `OpenLedgerDefer`), *before* the ledger. The guide's M2 wiring would replay it.
   Fix: the M2 director calls `BeginNightSequence(playDream:false, …)` — the card
   still shows; Dream 2 is never played twice.
3. **Wrong branch-flag field name (fixed).** The guide referenced a VillageState
   bool `dorisRefused` that does not exist; the real field is `refusedDorisOrb`.
   The M1 tease + builder now use `refusedDorisOrb`, so the **refusal path correctly
   shows `Tomorrow_M1_Day1_Refused`**.
4. **Accessibility add (engagement polish).** The card now also advances on
   Space / Return / E / Esc / click (mirrors the directors' advance affordance,
   D-049) and supports a **Gentle-Mode instant-fade** path (identical content,
   zero stress) driven by the sequencer reading `VillageState.gentleModeEnabled`,
   keeping `OneMoreDayCard` free of any game-state dependency.

Decision ID: the guide proposed **D-060**, which **collides** with the Depth Layer's
D-060 (Cold Open). Allocated **D-064** instead (see `ARCHITECTURE.md`).

### asmdef / architecture audit (Package Expert)

- `OneMoreDayCard` lives in **`HearthboundHollow.UI`** (refs Core/Memory/Audio only) —
  presentational, **no** Mission/Cutscene dependency → no asmdef cycle (D-035 ✅).
- `TomorrowTeaseSO` + `EndOfDaySequencer` live in **`HearthboundHollow.Mission`**
  (already refs Core/Memory/UI/Cutscene). Builder lives in **`HearthboundHollow.Editor`**.
- No new external packages. All card visuals use **Unity built-in UI sprites**
  (`UI/Skin/Background.psd`, `UISprite.psd`) so references persist cleanly in the
  prefab + scenes (no runtime-generated textures to lose) — matches the Depth
  Layer's "procedural primitives only" performance discipline.

### Cozy-review checklist (Critic Board)

- [x] No player-visible numbers on the card.
- [x] No "FAILED"/score language anywhere.
- [x] Goodnight button always advances; card is fully skippable.
- [x] Refusal path shows `Tomorrow_M1_Day1_Refused`, not the standard line.
- [x] Fade-in only — no shake, no flash, no harsh sting.
- [x] Gentle Mode: identical content, instant fade.
- [x] Pickle line respects Tannenbaum's budget (one short line).

### QA acceptance (Senior QA — to verify in-Editor after pull)

1. **Regression gate:** with `endOfDaySequencer` unwired, day-end is byte-for-byte today.
2. Day 1 confirm → (Dream 1) → Tomorrow card → `03→01_MainMenu` flow, in order.
3. Missing Yarn/SO degrades gracefully (forward-look only; **no NRE**).
4. `🔍 Diagnose Build` clean; EditMode + PlayMode smoke green.
5. Boot → Day 1 → card → next, **zero NRE** in the Console.

### Environment / asset-placement pass (Tech Artist + QA + 3D Modeler)

The Goodnight Card is a **screen-space overlay**, so the "placement" pass here is
UI-spatial: panel centred on a 1920×1080 reference canvas, parchment 9-sliced
(no corner distortion at any aspect), dark backdrop full-bleed with `raycastTarget`
ON (blocks world clicks), labels inside safe margins with dark-wash backings for
contrast over the parchment. The builder parents the card under the **same canvas
that hosts the Evening Ledger** so it shares the overlay layer + sort order and
never z-fights the HUD. No floating/clipping; sort order `SetAsLastSibling`.

> The broader 3D environment/asset-grounding audit (lane props, hollow interior
> dressing, cottage interior, collider hardening) was completed by the **Phase 47
> Level-Boundaries family** + **Phase 32 polish** already on this branch. Re-verified:
> guide lanterns + firefly wayfinding present, every prop carries a Collider, autumn
> skybox bound. No regressions introduced by this phase (UI-only runtime surface).

### How to run after pulling

```
1. git pull origin feat/mission-1-2-architecture
2. Unity recompiles (~10 s)
3. Hearthbound → 🚀 Build Everything → Build   (Phase 47-OMD is the final step)
4. Hearthbound → 🔍 Diagnose Build             (confirm clean)
5. Press Play in 00_Bootstrap.unity → finish Day 1 → watch:
   Evening Ledger → (Dream) → "Tomorrow" card → Goodnight → Day 2
```

### Next steps / deferred (tracked)

- **HH-OMD-T2** — Tier 2 visual anticipation: visitor silhouette + half-lit Echo
  Web thread glimmer on the card (`TomorrowTeaseSO` already reserves the fields).
- **HH-OMD-T3** — Tier 3 payoff: the promise pays off next morning (a waiting cue
  in the world on load).
- **HH-OMD-YARN** — switch SO mirrored text → runtime Yarn-dispatcher when that
  pass lands (Yarn remains canonical source today).
- Composer: a 4–6 s "goodnight" music sting under the fade (currently silent-safe).

---

## 🧭 Project state at session start (read by the team)

- **Phases on branch:** 0 → 47 (level boundaries/polish), 48 → 51 (Depth Layer:
  Cold Open, Echo Hologram, Preface Beat, Memory Web). Decisions D-001 → D-063.
- **Scenes:** `00_Bootstrap → 01_MainMenu → 02_Mission01_Lane → 03_Mission01_Hollow
  → 04_Mission02_Garden → 05_Mission02_Cottage`.
- **Out-of-Scope Wall respected** (`ARCHITECTURE.md §13`): no Weave/Sever/Listen-room
  expansion, no extra villagers, no full Marin arc. This phase is engagement polish
  on the existing day-end — squarely in scope.
- **Cozy Contract is non-negotiable**: nothing punishes kindness, no scored failure,
  refusal honored, no "FAILED" string, no player-visible numbers in emotional UI.

---

*Maintained by the Hearthbound Hollow virtual studio. Append newest entries on top.*
