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

## 🎬 Phases 73–75 — Depth-audit quick wins (the last 🟡 → 🟢) 🟢 (2026-05-30)

Implemented all three non-blocking quick wins from `Docs/DEPTH_VERIFICATION_Mission1_2_Phase72.md`,
hardening the engagement scorecard from **6🟢/1🟡 to 7🟢**.

**Specialists:** unity-engineer · Economy/Progression Architect · 2× Game Designers ·
Narrative Director · 4× Senior QA.

| Phase | Quick win | Engine | What | Commit |
|---|---|---|---|---|
| **73** | Visible Hollow growth | 3 (Ownership) | Buying an upgrade now puts a **real object in the room**. New `HollowUpgradeMarker` tag + `HollowProgressionService` now resolves markers by component (`FindObjectsInactive.Include`) so *hidden* props are findable (the old `GameObject.Find` couldn't see inactive objects). New `Phase73_HollowUpgradeMarkers` builder pre-places each upgrade's cozy prop hidden in the Hollow (Build Everything Step 23). | `f477e43d` |
| **74** | Per-save variety | 7 (Surprise) | New `VillageState.villageSeed` (randomised on new game, `0` = old saves) persisted in the snapshot; `RequestBoardService` blends it into the RNG + walk-in rotation so **two different saves see different early boards** while one save stays reproducible. | `fbe1b3bc` |
| **75** | "The Hollow grew" coda | 5 (Visible progression) | Every Evening Ledger now ends with a warm **"Before you rest"** line when the day advanced (kept memories / upgrades / echoes) — D-076 celebratory, gated on growth, no anxiety numbers. One choke-point in `EveningLedgerUI.Show`. | `ff3686c2` |

### Engineering notes
- **The marker bug the audit flagged was real:** `GameObject.Find` returns null for inactive
  objects, so "hidden until purchased" markers were unfindable. Fixed by a component lookup;
  the original active/named path is kept (authored catalogs). Reversible managed root.
- **Save compatibility:** `villageSeed` defaults to `0` for pre-v74 saves → identical to the old
  day-only roster. Schema stays v3 (new field defaults gracefully).
- **Cozy Contract ✅** throughout: upgrades are warmth not power, the ledger coda celebrates only
  when there's growth, no fail states, hand-written prose.

**Next:** owner playtest of Phases 71–75. M1/M2 now grades **7/7** on the engagement scorecard.

---

## 🎬 Phase 72 — Village Backdrop + Atmosphere + Depth Verification 🟢 (2026-05-30)

**Owner ask:** full environment polish with lots of assets, **bigger arenas**, cutscenes
if needed, best-in-class onboarding, and **verify M1/M2 depth** against the Engagement +
Depth bibles.

**Specialists convened:** Lead Unity Architect · 2× 3D Modelers · Lighting Expert ·
Technical Artist · 4× Senior QA · 3× Market Critics · 2× Game Designers · Narrative Director.

### What shipped
| Item | Player-facing | Where | Commit |
|---|---|---|---|
| **Village Backdrop** | each arena now reads as one corner of a **big, living autumn town** — authored-cottage skyline ring + two autumn tree belts + grass scatter + warm dusk lights/torch/chimney atmosphere + a Lane market row | new `Editor/Phase72_VillageBackdrop.cs` (Build Everything Step 22) | `d41e9138` |
| **Depth verification** | re-graded M1/M2 on the Engagement Bible's own 7-engine Stardew scorecard: **0/7 → 6 🟢 / 1 🟡** | `Docs/DEPTH_VERIFICATION_Mission1_2_Phase72.md` | (this) |

### Asset sourcing (Tech Artist + 3D Modelers)
All from packs **already on disk** — Medieval Village (`SM_Alder_Fall`, `SM_PineTree`,
`SM_ShopStand`, produce crates, `SM_FoodSac`, `SM_Signboard`, `SM_FlagGurland`,
`SM_Grass_01a_Foliage`, `PS_TorchFire`/`PS_CandleFire`, `SM_FakeCloudPlane`), the 4 authored
`_Project/Prefabs/Environment/Cottage_*` and **VertexField VoluSmoke FX** for chimney smoke
(soft built-in plume fallback). Every prop is raycast-grounded; the whole layer is one
deletable root per scene (`_Phase72_Backdrop`); **background props carry no colliders**, so
nothing touches the Phase 47 gameplay walls or the Phase 71 progression net.

### Depth verdict (Market Critics + Designers)
The pre-loop critique scored the slice **0/7** and rejected it. With the cozy daily loop
(Phases 62–67) the same scorecard is now **6 🟢 / 1 🟡**: compounding loop (Garden/coin/save
v3), player-authored goals (Request Board), **visible ownership** (Hollow upgrades reveal
pre-placed scene markers), six interleaving systems, visible progression (coin HUD/agenda),
a calendar of anticipation (Almanac). The lone 🟡 (surprise/variety) is deterministic-per-day
by cozy design. **Cutscenes:** coverage is adequate (Cold Open, Dream 1+2, Listen Scene,
goodnight card) — no new cutscene recommended (high blind-risk, low marginal depth).

### Residual quick wins (non-blocking — documented, awaiting greenlight)
1. Editor builder to **pre-place hidden Hollow upgrade markers** (makes "visible growth" airtight).
2. **Per-save variety seed** in `RequestBoardService` (needs one new `VillageState` field) → last 🟡 to 🟢.
3. Evening-Ledger **"the Hollow grew"** prose line.

Detail: `Docs/DEPTH_VERIFICATION_Mission1_2_Phase72.md`.

**Next:** owner playtest of Phases 71–72; then (optionally) the three Part-D quick wins.

---

## 🎬 Phase 71 — Onboarding depth + Mission-2 never-stranded + green garden 🟢 (2026-05-30)

**Two streams:** (1) the open *best-in-class onboarding* request, and (2) the player
playtest commit `3a8241fb` — *"green garden not available same issue of stuck at
mission 2 still"* (filed against a build that already had the Phase 62.D garden
enrichment).

**Specialists convened:** unity-engineer · 4× Senior QA · Technical Artist + 3D
Modelers (environment review) · engagement-polish · UX/UI Designer · Narrative
Director (Pickle nudge voice) · 2× Package/asmdef Experts.

### What shipped (pushed + verified byte-identical)
| Fix | Player-facing | File(s) | Commit |
|---|---|---|---|
| **Onboarding loop card** | new "Your little world" first-run step names the hub keys `[J][B][M][U][G][K]` | `UI/OnboardingOverlay.cs` | `e1bf823f` |
| **Help card completeness** | `H` now has a "Your day's places" section (loop hub keys, EN/AR) | `UI/HelpOverlayUI.cs` | `44e66fcc` |
| **Stuck-at-M2 (CRITICAL)** | Garden→cottage can **never** strand you: redundant position-exit + gentle ~40 s Pickle nudge + ~120 s never-stranded auto-advance | `Mission/Mission02Director.cs` | `006b5980` |
| **Green garden** | meadow is lush **green** again (was brown/dead); shared `HHGroundMaterials` builds a URP Lit mat on the green grass diffuse, never the Dried variant | new `Editor/HHGroundMaterials.cs` + `Phase62`/`Phase63` delegate | `797df874` |

### Root-cause notes (QA)
- **Stuck:** the committed Garden scene's exit trigger was *mechanically correct*
  (z=12, enabled trigger, wired, player tagged `Player`, cottage in Build Settings,
  M1→Garden hand-off wired). The flaw was a **single point of failure** — one
  walk-through box was the only way forward. The Mission02Director fix is **runtime**
  (the baked director shares the script), so it lands on pull **without a scene
  rebuild**, and the never-stranded layer guarantees progression regardless of the
  true trigger-miss cause.
- **Green garden:** both enrichment builders' blind `FindAssets("t:Material grass")`
  could return `M_DriedGrass_Fo_01a` (brown) or fall back to flat olive. Now a shared
  helper paints the green grass diffuse `T_Grass_Fo_01a_D` (excludes Dried/`_OR`/normal
  maps) and only ever falls back to lush green — applied on next `🚀 Build Everything`.

### Cozy Contract ✅ — never stranded (rescue guarantees it), soft one-time nudge, no
fail framing, hand-written Pickle lines. Detail: `Docs/PROGRESS_Phase71_PolishAndFixes.md`.

**Next:** depth-verification audit (M1/M2 vs Engagement + Depth bibles); `onboard.rhythm.*` AR strings.

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

*(Full Phase 47-OMD detail retained in `Docs/PROGRESS_Phase47_OneMoreDay.md` and
`Docs/ARCHITECTURE.md` D-064. Earlier phase history continues in `Docs/PROGRESS.md`.)*

---

*Maintained by the Hearthbound Hollow virtual studio. Append newest entries on top.*
