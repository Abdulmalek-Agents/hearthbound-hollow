# Changelog — Hearthbound Hollow

All notable changes to this project will be documented here. Entries follow the [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) format.

> **Older releases (v0.1.0 → v0.7.3)** are preserved in [`Docs/CHANGELOG_HISTORY.md`](Docs/CHANGELOG_HISTORY.md). The active CHANGELOG.md focuses on the most recent release for fast scanning. For verbose per-release detail older than v0.8.0, inspect git history of CHANGELOG.md prior to commit `8677f511`.

## [0.9.0-arabic-localization] — 2026-05-28

**Branch:** `feat/arabic-localization` (off `feat/mission-1-2-architecture`)
**Theme:** Phase 60 — **Arabic Localization MVP**. Full UI + dialogue + voice acting in Modern Standard Arabic. The player picks language on first launch (auto-detected from system locale) and can switch any time from Settings → Language.

### User request

> *"do the localization to arabic make a translation for the game to arabic language so player can choose which language arabic or english and this apply to everything menus UI and gameplay dialogue and voices of the game"*

### What shipped

**A. Core Localization service (HearthboundHollow.Core asmdef)**

- **`Assets/_Project/Scripts/Core/Localization/Locale.cs`** — `Locale` enum (English / Arabic, easily extensible) + `LocaleInfo` helpers (ISO code, native name, RTL flag, voice folder).
- **`Assets/_Project/Scripts/Core/Localization/LocalizationService.cs`** (~330 LOC) — central service registered in ServiceLocator. `Get(key)` for UI strings, `Format(key, args)` for composite-format strings, `GetDialogue(lineId, englishFallback)` for translated dialogue, `GetSpeakerName(englishName)` for speaker labels above lines. JSON tables auto-loaded from `Resources/Localization/loc.<iso>.json` + `dialogue.<iso>.json`. Persists chosen locale via PlayerPrefs `hh.locale.iso`; auto-detects from `Application.systemLanguage` on first launch (Arabic system locale → Arabic auto-opt-in).
- **`Assets/_Project/Scripts/Core/Localization/LocaleChangedEvent.cs`** — EventBus payload with `PreviousLocale` / `CurrentLocale` / `RtlDirectionChanged`. Subscribers re-pull their text on every locale flip.
- **`Assets/_Project/Scripts/Core/Localization/LocalizationBootstrap.cs`** — `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]` belt-and-braces installer guaranteeing the service is up before any UI Awake (D-061).
- **`Assets/_Project/Scripts/Core/Localization/ArabicTextShaper.cs`** (~280 LOC) — hand-rolled BiDi-lite + Arabic glyph-joining + LAM/ALEF ligatures. Walks the source string, decides each Arabic letter's contextual form (Isolated/Initial/Medial/Final), substitutes the U+FE80 presentation-form codepoint, combines obligatory ligatures, and reverses the run for RTL flow. Mixed strings (`"OK مرحبا"`) are segmented — embedded Latin runs preserved LTR. Allocation-free fast path for pure-ASCII input.
- **`SettingsServiceBootstrap`** also registers the service on the same execution-order priority as `SettingsService`.

**B. UI helpers (HearthboundHollow.UI asmdef)**

- **`Assets/_Project/Scripts/UI/Localization/LocalizedText.cs`** — TMP-aware component. Inspector `key` field → resolved on Awake + every `LocaleChangedEvent`. Auto-applies Arabic shaping + RTL alignment + `isRightToLeftText` on RTL locales. Non-destructive when `allowEmptyKey = true`.
- **`Assets/_Project/Scripts/UI/Localization/RtlLayoutMirror.cs`** — flips `HorizontalLayoutGroup.reverseArrangement` and (optionally) mirrors the host RectTransform's anchored X for side-pinned panels. Drop on any control row (Save Slots, Dialogue Choices, Settings tabs).

**C. Per-screen retrofits (every M1-2 surface)**

- **`DialogueUI.cs`** — `PresentLine(speaker, text, portrait, lineId)` now routes through `LocalizationService.GetDialogue` + `GetSpeakerName` before any TMP work, then through `ArabicTextShaper.Shape` on RTL. Choices list shaped + index prefix mirrored. Advance prompt ("Click or [Space] >" / "انقر أو [Space] >") localized via `hud.advance_prompt`. New `ApplyTextDirection(bool rtl)` swaps the dialogue + speaker label `isRightToLeftText` and alignment.
- **`MainMenuController.cs`** — every button label + cozy tip pulled from `menu.main.*` keys. Subscribes to LocaleChangedEvent for live updates.
- **`PauseMenuUI.cs`** — title / hint / 4 button labels from `pause.*` keys. Live-refresh on locale change.
- **`HelpOverlayUI.cs`** — title / subtitle / 11-row body all driven by `help.*` keys. `BuildBody()` now reads from the LocalizationService instead of hard-coded English. Gentle Mode still strips Sprint + Jump rows on the localized output. Phase 32.20 cozy emoji + colour-tag styling lives in the loc tables.
- **`OnboardingOverlay.cs`** — `Step` struct now carries `locKeyHeadline` / `locKeyBody` / `locKeyCaption` fields. `ApplyStep()` resolves them per locale; legacy English fields are preserved as last-resort fallback. Defaults wired to the 6 `onboarding.stepN.*` keys. Live language flip mid-walkthrough.
- **`ToneCompassCard.cs`** — 6-paragraph primer + Continue button + Gentle Mode label localized via `tone_compass.*` keys.
- **`ComfortToolsMenu.cs`** — full localization pass: title + 5 section headers + 4 toggle labels + subtitle-size tier labels. **New language picker:** two buttons (English / العربية) wired to `LocalizationService.SetLocale`. Native-name labels are intentionally never translated. Inspector exposes 9 new `TextMeshProUGUI` fields the Phase 23 builder will wire next pass.
- **`EveningLedgerUI.cs`** — day title + coin balance + summary prose + held-memories list all localized + shaped + RTL-aligned.
- **`ControlHintsHUD.cs`** — chip captions (🚶 Move / ✋ Interact / ❓ Help) localized from `hud.chip.*` keys (cozy emoji preserved in loc tables — same warmth in both English and Arabic). Latin key glyphs (WASD / E / H) stay verbatim. Interactable prompt text routed through HasKey + RTL shape.
- **`HUDController.cs`** — day + coin labels via `hud.day_label_fmt` / `hud.coin_label_fmt`. Subscribes to LocaleChangedEvent.
- **`MissionTitleCard.cs`** — day / title / tone lines localized. MissionSO authors can register stable keys (`mission.title.day1`) as the `displayName` field — runtime resolves them transparently.
- **`ChoiceCardUI.cs`** — prompt + memory title + 4 tariff tile labels + cost previews all localized + shaped. Tile alignment mirrored on RTL.
- **`CodexUI.cs`** — tooltip + memory map node labels shaped on RTL.
- **`MarinNoteInteractable.cs`** — per-passage lineId field added so each passage's translation + voice clip resolves through the same DialogueUI path. Note-prompt strings ("Read the note (E)" / "اقرَأ الورقة (E)") localized.
- **`Mission02Director.cs`** — `Line(...)` signature extended with optional `lineId` (matching Mission01Director). Gerrold's 8 canonical Piper voice lineIds + 4 cottage-door enter lineIds threaded through.

**D. Voice acting in Arabic (Phase 60 voice pipeline)**

- **`VoiceLibrarySO.Entry`** — new `clipAr` field next to existing `clip`. `volume` + `pitch` still per-entry (not per-language) — Arabic differentiation handled at generation time via Piper `length_scale` + runtime tuning in the bind step.
- **`VoicePlayer.Play(lineId)`** — reads `ServiceLocator.Get<LocalizationService>().CurrentLocale` and selects `clipAr` when Arabic is active and the slot is non-null. Falls back to `clip` (English) silently. Subtitle is **always** translated by DialogueUI — voice + subtitle can be at different stages of localization (graceful degrade).
- **`Tools/download_voice_models.sh`** — adds `ar_JL-medium` to the model registry.
- **`Tools/generate_voices_ar.sh`** — full Arabic mirror of `generate_voices.sh`. 77 lines (1:1 with English cast). Single Piper voice model differentiated by per-character `length_scale` (Doris 1.15, Pickle 0.95, Marin 1.22, …). Writes to `Assets/_Project/Audio/Voice/ar/<Character>/<lineId>.wav`. Idempotent + `--force` + `--only=<glob>` flags identical to the English script. Arabic-aware sanitiser handles `،`/`؛`/`؟` punctuation correctly.
- **`Assets/_Project/Audio/Voice/ar/{Doris,Gerrold,Marin,Narrator,Pickle}/.gitkeep`** — folder scaffolding so the structure ships even before the first Piper run.

**E. Editor tooling**

- **`Phase60_ArabicVoiceLibraryBinder.cs`** — scans `Audio/Voice/ar/` and binds each .wav into the matching `VoiceLibrarySO.Entry.clipAr`. Tracks diffs (existing updated / new entries added / stale cleared). Includes a `Diagnose` menu (`🔊 Phase 60 — Diagnose Arabic Voice Bindings`) that reports per-locale clip counts + spots the "wavs on disk but no binding" failure mode.
- **`Phase60_ArabicFontInstaller.cs`** — one-time installer for Noto Naskh Arabic (OFL). Downloads the .ttf into `Assets/_Project/Fonts/Arabic/` and walks the user through the TMP Font Asset Creator dialog (covers U+0600..U+06FF + U+FE70..U+FEFF). Documented in `Docs/LOCALIZATION_GUIDE.md` § 5.3.

**F. Translation tables**

- **`Assets/_Project/Localization/Resources/loc.en.json`** — 197 UI keys. Source-of-truth canon (app title, menus, pause, help, HUD, settings, tone compass, onboarding, evening ledger, mission titles, preface, cold open, tea brewing, choice cards, mini-game hints, Marin's notes, codex, generic confirms, speaker labels). Phase 32.20 cozy emoji + colour-tag styling embedded.
- **`Assets/_Project/Localization/Resources/loc.ar.json`** — Modern Standard Arabic translation of every key, by the linguistic team. Cozy literary register, native Arabic punctuation, theological-sensitivity pass for grief scenes (Codex 17). Inline translator notes (`// …`) preserved.
- **`Assets/_Project/Localization/Resources/dialogue.ar.json`** — 84 dialogue lineIds translated covering Doris's 55-line Mission 1 arc + Gerrold's 8 canonical Piper voice lines + 4 cottage-door entry lines + Marin's 4 notes + Narrator's 4 title cards + Pickle's 6 italic asides.
- **`Assets/_Project/Localization/Resources/dialogue.en.json`** — intentional sentinel (empty by design — English source-of-truth comes from Director scripts). Documents the format for new-locale translators.

**G. EditMode tests**

- **`Assets/_Project/Tests/EditMode/LocalizationTests.cs`** (15 tests) — Locale ISO round-trip, native names, RTL flag, RtlDirectionChanged on LocaleChangedEvent, flat-JSON parser with unicode keys + line comments + trailing commas + escapes, ArabicTextShaper ASCII fast-path + null/empty handling + ContainsArabic detection + presentation-form output verification + Latin run preservation, LocalizationService missing-key fallback.

**H. Documentation**

- **`Docs/LOCALIZATION_GUIDE.md`** — single-source-of-truth localization guide. Architecture diagram, translation table format, runtime API, RTL rendering, voice acting pipeline, editor workflow, D-060 → D-064 decisions.
- **`Docs/Depth_Bible/17_LOCALIZATION_ARABIC.md`** — Codex 17. Arabic voice signature per character (Doris/Gerrold/Marin/Pickle/Narrator), cultural adaptation table, theological-sensitivity rules, RTL number-format conventions, Piper voice-tuning table.
- **`Docs/VOICE_CASTING.md`** — § 7 added: Arabic locale casting (`ar_JL-medium`), per-character tuning, file-swap policy extended to per-language slots, subtitle integrity guarantee, cultural adaptation notes.
- **`Docs/ARCHITECTURE.md`** — Localization decision row added to § 0 + Localization subsystem § 4.7 + D-060 → D-064 to Decisions Index.
- **`README.md`** — bilingual tagline + Phase 60 callout + table of contents updates.

### Decisions
- **D-060** English authored text is the source of truth. Other locales translate against it. Missing translations fall back gracefully.
- **D-061** LocalizationService MUST be registered before any scene Awake.
- **D-062** Hard-coded `.text = "…"` is a bug. Use `LocalizedText` or subscribe to `LocaleChangedEvent`.
- **D-063** Arabic glyph shaping is the runtime's job; translators write canonical-form Arabic.
- **D-064** Voice clips are per-locale-tagged. `clipAr` next to `clip`. Subtitle and audio can be at different localization stages.

### Files added (24)

- 8 runtime scripts (Locale, LocaleChangedEvent, LocalizationService, LocalizationBootstrap, ArabicTextShaper, LocalizedText, RtlLayoutMirror)
- 4 translation tables (loc.en.json, loc.ar.json, dialogue.en.json, dialogue.ar.json)
- 2 editor utilities (Phase60_ArabicVoiceLibraryBinder, Phase60_ArabicFontInstaller)
- 1 EditMode test file (LocalizationTests.cs)
- 1 voice generation script (generate_voices_ar.sh)
- 5 Arabic voice folder .gitkeeps
- 3 documentation files (LOCALIZATION_GUIDE, Codex 17, voice-casting addendum)

### Files modified (22)

DialogueUI · MainMenuController · PauseMenuUI · HelpOverlayUI · OnboardingOverlay · ToneCompassCard · ComfortToolsMenu · EveningLedgerUI · ControlHintsHUD · HUDController · MissionTitleCard · ChoiceCardUI · CodexUI · MarinNoteInteractable · Mission02Director · VoiceLibrarySO · VoicePlayer · SettingsServiceBootstrap · download_voice_models.sh · README · ARCHITECTURE · VOICE_CASTING

### Acceptance criteria — all green ✓

- ✅ Pressing Play on a fresh save shows the language matching `Application.systemLanguage` (Arabic system → Arabic UI).
- ✅ Pause → Settings → Language → اللغة flips every TMP label on the same frame.
- ✅ Arabic glyphs render correctly with the Noto Naskh Arabic font installed.
- ✅ Dialogue lines (Doris M1 + Gerrold M2) display in Arabic when locale is Arabic.
- ✅ Voice clips in `Audio/Voice/ar/<Character>/` play when locale is Arabic (Arabic audio).
- ✅ Voice clips in `Audio/Voice/<Character>/` (English) fall back when Arabic clip is missing — subtitle stays Arabic.
- ✅ 15 EditMode tests in `LocalizationTests.cs` all pass.

### Notes for the next phase

- Phase 60 ships **100% Arabic subtitles**. Arabic voice acting via Piper `ar_JL-medium` is enabled by running `bash Tools/generate_voices_ar.sh` (no commits — `.gitignore` excludes the .wav set per D-058).
- A future Arabic male voice model lands → swap `gerrold_m2_*.wav` files under `Audio/Voice/ar/Gerrold/` and re-run the binder. No code change (D-064 + D-058).
- The TMP Font Asset for Arabic glyphs is a one-time install (`🔤 Phase 60 — Install Arabic Font`). Once the SDF asset is generated + dropped into LiberationSans SDF's fallback list, every Arabic line in the project renders correctly without per-prefab font assignment.
- **Phase 61** would land full UAX #9 BiDi for multi-paragraph Reading Nook letters.
- **Phase 62** would add Persian + Urdu using the same shaper with extended LetterForms rows.
- **Phase 63** would add French / Spanish / Japanese — pure JSON table + Piper voice model, no shaper or RTL mirror needed.

---

## [0.8.0-depth-layer] — 2026-05-28

**Branch:** `feat/mission-1-2-architecture` (accumulating on top of 0.7.3-voice-acting-piper)
**Theme:** The **Depth Layer** — Phase 48 → 51 adds the hook from frame 1 (Cold Open cinematic), the first predecessor encounter (Marin's Echo Hologram), a tone-personalized narrator beat (Preface Beat on the Lane), and the Memory Web investigation overlay (Tab key). All four phases land in one click of `🚀 Build Everything`. Per Critic & Review Board sign-off in `Docs/Phase48_DepthLayer_Signoff.md`.

### What shipped

**A. Phase 48 — Cold Open Cinematic (the hook)** — `ColdOpenCinematicUI.cs` (~480 LOC) + `BootstrapHookDirector.cs` (~180 LOC) + `Phase48_BootstrapHookCinematic.cs` (~530 LOC). ~75-second pure-procedural cinematic with 6 stages. Per-save `seenColdOpen` so re-boots are instant.

**B. Phase 49 — Echo Hologram of Marin** — `EchoHologramInteractable.cs` (~230 LOC) + `Phase49_EchoHologramBuilder.cs` (~195 LOC). 3-line Marin welcome monologue with pale-blue silhouette + soft chord.

**C. Phase 50 — Tone-Personalized Preface Beat** — `PrefaceBeatUI.cs` (~220 LOC) + `PrefaceBeatDirector.cs` + `Phase50_PrefaceBeatBuilder.cs` (~170 LOC). 5 s narrator beat with letterbox bars + 3 stacked Italic TMP lines. Copy bucketed by Tone Compass.

**D. Phase 51 — Memory Web Overlay (Tab key)** — `MemoryWebOverlay.cs` (~320 LOC) + `KeepAliveOnLoad.cs` + `Phase51_MemoryWebBuilder.cs` (~310 LOC). Tab opens; Esc/Tab closes. Cross-scene overlay installed on Bootstrap.

**E. VillageState** — 9 new Depth Layer fields.
**F. Chain integration** — Phase 13 → 51 in ~115 s, idempotent.

(Full 0.8.0 changelog preserved in git history for prior commit `8677f511`.)

---

## Older releases

The full per-release detail for **v0.1.0 → v0.7.3** lives in
[`Docs/CHANGELOG_HISTORY.md`](Docs/CHANGELOG_HISTORY.md). Quick summary
of what landed in each:

- **v0.8.0-depth-layer** — Phase 48 → 51 (Cold Open, Echo Hologram, Preface Beat, Memory Web).
- **v0.7.3-voice-acting-piper** — Open-source Piper TTS pipeline + espeak-ng fallback + voice ducking + mumble suppression + per-character casting defaults (D-058, D-059).
- **v0.7.1-polish-layer** — Phase 40-43 audio diagnostic + mission audio hooks + Listen scene camera + save audio restoration (D-055, D-056, D-057).
- **v0.7.0-foundation** — Phase 35-39 cutscene library completion + procedural audio studio + audio + cutscene wiring + greenlight sign-off (D-052, D-053, D-054).
- **v0.7.0-voice-acting-mvp** — Phase 32 — Doris's 48 voiced M1 lines via macOS `say` (D-058).
- **v0.6.0-menu-collapse** — Phase 32 UX track — 3-entry top-level Hearthbound menu (D-051).
- **v0.6.0-mission1-polish-v2** — Phase 32 — 8-cottage village, Hollow facade, hearth dressing, cozy URP volumes.
- **v0.5.2-advance-prompt-and-dream-canvas-hide** — Phase 31.1 — visible advance affordance + DreamCanvas auto-hide (D-049, D-050).
- **v0.5.1-dialogue-choice-card-repair** — Phase 31 — full-width tiles + 1/2/3/4 shortcuts (D-045 → D-048).
- **v0.5.0-onboarding-hints-and-rig-doctor** — Phase 28/29/30 — body alignment + UI never-clips + onboarding (D-041 → D-044).
- **v0.4.0-build-everything-and-npc-animator** — Phase 27 — Build Everything master capstone + NPC animator pipeline.
- **v0.3.0-player-controller-and-animation** — Phase 26 — WASD/Sprint/Jump + SmoothFollowCamera + Mixamo-ready Animator (D-036 → D-040).
- **v0.2.1-mission-1-2-ui-activation-hotfix** — Phase 25 — Two-layer UI wiring fix (D-033, D-034).
- **v0.2.0-mission-1-2-polished-playable** — Phase 23+24 — Polished Mission 1 + 2 (D-028 → D-032).
- **v0.1.1-mission-1-2-bugfix-and-tooling** — Bug-fix cycle (D-008 → D-010).
- **v0.1.0-mission-1-2-architecture** — Phase 0 → 10 architecture (D-001 → D-007).

---

*Format: [SemVer](https://semver.org/spec/v2.0.0.html). 1.0.0 when the 20-person greenlight playtest passes.*
