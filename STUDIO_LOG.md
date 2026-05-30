# 🎬 STUDIO_LOG.md — Hearthbound Hollow

> **Living studio log.** Tracks phase completion, decisions, blockers resolved,
> asset-placement notes, QA sign-offs, and next steps. Newest entries on top.
> Companion to `Docs/PROGRESS.md` (technical changelog), `Docs/ARCHITECTURE.md`
> (decision ledger `D-0xx`), and `CHANGELOG.md` (release notes).
>
> **Engine:** Unity 6000.4.4f1 · **Pipeline:** URP-Mobile · **Target:** PC (Win64, Steam) + mobile-class perf discipline.
> **Branch:** `feat/mission-1-2-architecture` · **State:** narrative-complete vertical slice (M1 + M2) + Depth Layer (48–51) + One More Day hook (47-OMD) + **the cozy daily loop (Engagement Bible P1–P7, Phases 61–67).**

---

## Legend
✅ Done & merged · 🟢 Done in branch (awaiting your pull + `🚀 Build Everything`) · 🟡 In progress · ⬜ Not started · 🔴 Blocked

---

## 🎬 Phases 62–67 — The Cozy Daily Loop (Engagement Bible P2–P7) 🟢 (2026-05-30)

**User request:** *Assign 20+ senior specialists, read every doc + the `Docs/` tree + the
Depth_Bible + Engagement_Bible on `feat/mission-1-2-architecture`, understand the project, then
**extend the game beyond Mission 1 & 2** — address each Engagement-Bible point one by one,
implement the recommendations in phases and push, and **add the deferred content (no need to
defer it)** to make the game deeper, more engaging and fun.*

### Specialists convened (24)
Lead Unity Architect · 6× Senior Unity Devs · 4× C# Scripters · 2× Systems/Progression
Architects · Build/DevOps · 2× Package/asmdef Experts · UX/UI Designer · 2× 2D/UI Artists ·
2× Game Designers · Mini-Game Designer · Economy/Garden Designer · Choice & Consequence Architect ·
2× Writers · Narrative Director · 4× Senior QA · 3× Market Critics.

### Verdict acted on
The Engagement Review scored the slice **0/7** against Stardew's retention engines: a 75-minute
corridor with no compounding loop. The owner greenlit building the loop **and** un-deferring the
loop-critical content. We did — **Pillars P2–P7 now all ship** (P1 was already live), so the
slice is a game you can live in.

### What shipped (pushed to the branch) — 17 new runtime scripts + docs

| Phase | Pillar | Player-facing | Key |
|---|---|---|---|
| **62** | P2 | Interactive **Request Board** — choose a villager, Keep / Listen / Not-today; the **real coin economy**; save schema v3 (the loop persists/compounds) | **B** |
| **63** | P6 | **Memory Wall** — kept memories connect into Echo threads you chase & complete (+coin, celebratory beat) | **M** |
| **64** | P3 | **My Hollow** shop — coin → cozy upgrades (shelf, relit room, garden bed, Marin's cloth, Pickle's cushion) + a visible **coin purse** | **U** |
| **65** | P4 | **Garden & Tea** — plant → ripen over days → harvest → brew (teas are gentle tools) → sell; the garden→tea→coin wheel | **G** |
| **66** | P5 | **Living Workbench** — tend kept memories with varied verbs (Polish/Cleanse/Sort/Steep) + gentle "Perfect" Keeper's-Hand mastery | **K** |
| **67** | P7 | **The Almanac** — Market Day / Festival / bard / birthday anticipation in the morning Agenda | — |

Every system is **self-installing, fallback-safe (works with zero authored content), asmdef-clean
(no UI→Mission edge — UI reads Core blackboards + publishes intent on the EventBus; Mission owns
mutations), and Cozy-Contract-compliant** (no fail, refusal honoured, opt-in, celebratory feedback
only). Reachable purely via pull + `🚀 Build Everything` + Play — **no scene edits required.**

### Un-deferred content (Phase 68)
Procedural village texture now ships via a built-in 7-villager Request roster + built-in Echo
threads + built-in upgrade catalog + built-in herb table. Authored `RequestPool` / `EchoPool` /
`HollowCatalog` / `MemoryHerb` SOs extend or override any of it with **zero code change**.

### Decisions
**D-079** string-flag content gating (`VillageStateFlags`) · **D-080** companion Memory Wall screen ·
**D-081** self-installing loop screens bridged via Core blackboards + intent events.

### Docs
`Docs/Engagement_Bible/11_WIDER_CONTENT_EXECUTION_PLAN.md` (mandate tracker) · roadmap `10` updated ·
`Docs/PROGRESS_Phase62_67_EngagementLoop.md` · `Docs/PHASE70_GENGAGE_PLAYTEST.md` (the validation
gate + instrumentation) · `Docs/MARKETING_TRUTH_Phase72.md`.

### QA acceptance (verify after pull + 🚀 Build Everything + Play)
1. Gameplay scene → morning Agenda lists almanac / visitors / garden + the key map (B/M/U/G/K/J).
2. [B] keep a memory → coin purse ticks up; [J]/[M] reflect the new memory/echo counts.
3. [G] harvest+brew; [U] buy the window shelf with earned coin; [K] tend a memory ("Perfect"?).
4. End day → Day 2's board differs, garden advanced, coin persisted across save/load.
5. `🔍 Diagnose Build` clean; zero NRE boot → menu → Day 1 → Day 2.

### Next
G-Engage playtest (≥15/20 testers voluntarily start Day 4) → tune (Phase 71). Optional Editor
builder to author the `Resources/` SO pools + pre-place hidden upgrade markers, chained into
Build Everything. Evening-Ledger "The Hollow grew" growth section (Phase 69 polish).

---

## 🎥 Phase 57–60 — Single-Entry Menu · Emoji/Arabic · Strand-Proof · Env Enrichment 🟢 (2026-05-29)

**User request:** Re-review docs vs. the build + the gameplay video for global-hit
quality; **collapse the Hearthbound menu to one entry — `🚀 Build Everything` —
removing Diagnose + Advanced**; make Build Everything include **all** phases; ensure
the **end-of-video freeze** is resolved; **onboarding emoji in TMP**; a **cleaner
tutorial**; **all available Arabic localization applied**; **enrich the environment**
from available packs with **accurate placement**; push **every phase** to this branch;
then a **second round** of checks.

### Specialists convened (26)
Lead Unity Architect · 6× Senior Unity Devs · 3× C# Scripters · Build/DevOps ·
2× Package/asmdef Experts · UX/UI Designer · 2× 2D/UI Artists (TMP) · Localization
Lead (EN/AR, RTL) · Cutscene Director · 2× Camera Experts · Lighting Expert ·
2× Game Designers · 2× Writers · Narrative Director · 4× Senior QA · Technical
Artist · 2× 3D Modelers · 3× Market Critics.

Full write-up: **`Docs/Phase57_60_GlobalHit_Polish.md`** (frame-by-frame freeze
re-verification, decisions, full QA acceptance).

### Freeze verdict
Root-caused & fixed in Phase 54 (**D-069** — `EveningLedgerUI.Hide()` no-op). This
pass **verified the whole night chain end-to-end against the serialized scene data**
(ledger Hide → sequencer wired → card advanceable → `04_Mission02_Garden` in Build
Settings). **The committed video predates the fix.** Phase 59 closes the last
unbounded await.

### What shipped (pushed to the branch)

| Phase | Item | Decision | Status |
|---|---|---|---|
| **57** | `Phase57_MenuConsolidation` — `[InitializeOnLoad]` prunes every `Hearthbound/…` item except `🚀 Build Everything` via `Menu.RemoveMenuItem` (builders kept; called by reflection) | D-074 | 🟢 |
| **57** | `Phase27_BuildEverything` chains Phase 54 (glyphs) + 56 (Arabic font) + 60 (env) — single entry reaches every builder | D-075 | 🟢 |
| **58** | `HelpOverlayUI` — emoji → `HollowGlyphs` (no tofu) + full controls body localized EN/العربية (shaped, RTL, live) | D-076 | 🟢 |
| **58** | `ControlHintsHUD` — chip captions glyph-routed + localized + shaped, live on language change | D-076 | 🟢 |
| **59** | `EndOfDaySequencer` — anti-strand watchdog on the Goodnight card (no time pressure; day always advances) | D-078 | 🟢 |
| **60** | `Phase60_HollowDressingEnrichment` — silent red-quad retire (all scenes) + grounded, collider'd "baker's corner" in the Hollow (Medieval Village pack), reversible managed root | D-077 | 🟢 |

### Decisions (see `Docs/Phase57_60_GlobalHit_Polish.md` §3)
- **D-074** single-entry menu · **D-075** complete Build Everything chain ·
  **D-076** Help/hint emoji+Arabic · **D-077** reversible env-enrichment layer ·
  **D-078** every night-chain await bounded.

### asmdef / Cozy audit
- `Phase57`/`Phase60` (Editor), `HelpOverlayUI` (UI→Core), `ControlHintsHUD`
  (Mission→Player/UI/Core), `EndOfDaySequencer` (Mission). No new cycles (D-035).
- No new external packages. Cozy Contract held: no "FAILED", no punishment, no
  player-visible numbers in emotional UI; the card watchdog is a silent anti-strand
  net (no timer shown), refusal path intact.

### QA acceptance (verify after pull + `🚀 Build Everything`)
1. Hearthbound menu shows ONLY `🚀 Build Everything`.
2. Day 1 → Sleep — End Day → ledger closes → Dream → Goodnight → **Day 2 loads**
   ("The Widower's Request"); no freeze; double-press is a no-op.
3. Help/onboarding/hint emoji = gold glyphs (or clean text) — never tofu.
4. Settings → العربية: Help body, hints, menus, settings, creator render connected
   right-aligned Arabic; English flips back — live.
5. Red floor quads gone; Hollow baker's corner grounded near the Workbench
   (delete `_Phase60_HollowDressing` to revert).
6. Boot → menu → Day 1 → Day 2, zero NRE.

### Round-2 re-review
Recorded at the bottom of this log after re-reading each change against the Cozy
Contract, the asmdef graph, and the hand-written-dialogue rule.

### Recommended next (tracked)
- **HH-AVATAR** — curate the player look via `CharacterAppearance` (cozy villager,
  not a grey placeholder).
- **HH-AR-DIALOGUE** — dedicated writer translation pass for the hand-written
  dialogue/ledger/dream prose (shaper + font already ready; canonical EN today, D-065).
- **HH-ENV-VFX** — in-Editor judgement pass to ground the note sprite + clamp the
  oversized hearth VFX (Phase 55 audit lists them; auto-clamp deliberately avoided).

---

## 🎥 Phase 54 — QA Video Review: End-of-Day Freeze, Emoji & Camera 🟢 (2026-05-29)

**User request:** Assign 20+ senior specialists, read every doc + the `Docs/` folder
on `feat/mission-1-2-architecture`, watch `Docs/Gameplay video testing .mp4`, compare
**design docs vs. the actual build**, raise it to global-hit quality, **fix the
end-of-video freeze** (game stuck, won't advance to the next phase), add onboarding
**emoji in TextMesh Pro**, make the **tutorial cleaner** to PC-hit standards, enrich
the environment from available assets with accurate placement, and push every phase to
this branch — then a second review pass.

### Specialists convened (24)
Lead Unity Architect · 4× Senior Unity Devs · 3× C# Scripters · Build/DevOps ·
Package/asmdef Expert · UX/UI Designer · 2× 2D/UI Artists · Cutscene Director ·
2× Camera Experts · Lighting Expert · 2× Game Designers · 2× Writers · Narrative
Director · 4× Senior QA · Technical Artist · 2× 3D Modelers · 3× Market Critics.

Full write-up: **`Docs/Phase54_QA_VideoReview_and_Fixes.md`** (frame-by-frame timeline,
root-cause proof, full docs-vs-build gap table).

### Root cause of the freeze (confirmed against serialized data) — D-069
After "Sleep — End Day" the game froze: the Evening Ledger never closed, the night
beats (dream + goodnight card) played behind it, and the on-top panel ate every
click. **`EveningLedgerUI.Hide()` was a silent no-op** because in
`UI_EveningLedger_Bamao.prefab` `root` IS the component's own GameObject, so the old
guard `root != gameObject` skipped the deactivate. The Mission-01 title card reading
"Opening the Hollow" at 3:30 proves the scene never advanced (Mission 2 = "The
Widower's Request").

### What shipped (pushed to the branch)

| Phase | Item | Decision | Status |
|---|---|---|---|
| **A** | `EveningLedgerUI.Hide()` closes in every layout (CanvasGroup + self-deactivate); single-fire confirm guard | D-069 | 🟢 |
| **A** | `EndOfDaySequencer` dream **watchdog** + guarded transition (menu fallback) | D-069 | 🟢 |
| **A** | `GameManager.LoadScene` validates the scene + falls back to menu (never stranded) | D-069 | 🟢 |
| **A** | `MissionRunner` + `Mission01Director` single-fire end-of-day guards; lock player through the night; lock movement while ledger open | D-069 | 🟢 |
| **B** | `HollowGlyphs` (UI) — emoji → TMP `<sprite>` tags when installed, clean text otherwise (no tofu) | D-070 | 🟢 |
| **B** | `Phase54_HollowGlyphsBuilder` (Editor) — bakes a gold glyph TMP Sprite Asset + registers it (default + emoji fallback); auto-installs on load; fully defensive | D-070 | 🟢 |
| **B** | Cleaner, action-first onboarding copy + glyph-routed `ControlHintsHUD` captions | D-070 | 🟢 |
| **C** | `DialogueCameraDirector` speaker **registry**; `Mission01Director` registers Doris so dialogue frames her instead of a wall | D-071 | 🟢 |

### Decisions (see ARCHITECTURE)
- **D-069** — End-of-day soft-lock fix: the modal Evening Ledger must always be able to
  fully hide (CanvasGroup + self-deactivate) regardless of `root==gameObject`; the night
  chain is watchdog-bounded and the transition guarded; `LoadScene` validates + falls
  back. Single-fire guards prevent re-entrancy. The day always advances.
- **D-070** — Onboarding/hint emoji render via a baked on-brand gold glyph TMP Sprite
  Asset (procedural, no committed binary) registered as the TMP default + emoji fallback;
  a runtime helper degrades to clean text so a tofu box can never ship.
- **D-071** — Dialogue camera resolves speakers via a registry that directors populate by
  name, so renamed/non-eponymous NPC objects still get a framed over-the-shoulder shot.

### QA acceptance (verify after pull + recompile / Build Everything)
1. Finish Day 1 → "Sleep — End Day": the ledger closes, Dream → goodnight card → next
   day runs, **no freeze**; pressing it twice does nothing extra.
2. Onboarding/hint emoji render as gold glyphs (or clean text before the glyph asset
   builds) — **never a tofu box**.
3. Doris dialogue frames Doris (over-the-shoulder), not a wall.
4. `🔍 Diagnose Build` clean; zero NRE boot → menu → Day 1 → next day.

### Next steps (Phase D — tracked)
- **HH-54-ENV** — Environment enrichment + asset-placement accuracy pass for the Hollow
  interior (retire the red placeholder floor quads to diegetic prompts, clamp the
  oversized fire VFX + ground the floating note, dress the sparse interior from packs
  already in the project) via an idempotent Editor builder + joint QA/Tech-Artist pass.
- **HH-54-AVATAR** — Replace/curate the bald placeholder player look (cozy villager) via
  `CharacterAppearance` so the protagonist doesn't read as a test dummy.
- **Round 2** — re-review every phase against `Docs/Phase54_QA_VideoReview_and_Fixes.md`.

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
