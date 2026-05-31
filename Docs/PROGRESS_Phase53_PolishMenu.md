# 🎭 PROGRESS — Phase 53 Polish Menu Layer + Voice Sanitiser (supplement)

> Companion to `Docs/PROGRESS.md`. Covers the voice "full stop / dot-dot-dot"
> fix (Phase 46.2) and the Polish Menu Layer (Phase 53): runtime EN/العربية
> localization, Reset Game → title, and the New-Game Character Creator.

---

## Legend
✅ Done & merged · 🟢 Done in branch (awaiting pull + `🚀 Build Everything`) · 🟡 In progress

---

## 🎙️ Phase 46.2 — Human-speech TTS sanitiser 🟢 (2026-05-29)

**Problem:** dialogue voices pronounced the ellipsis "…" as "full stop" / "dot dot
dot" (and "—" as "dash"), breaking the human-actor illusion.

**Root cause:** the Piper pipeline already cleaned text, but the espeak-ng fallback
(`Phase46_VoiceGenerator.cs`, the one `🚀 Build Everything` runs) fed RAW text to
the engine.

**Fix:** `CleanForTts()` + `IsDirtySource()` in `Phase46_VoiceGenerator.cs` (parity
with `Tools/generate_voices.sh`'s `clean_for_tts`): ellipses/em-dashes → comma pauses;
stage directions + `*emphasis*` stripped; trims leading/trailing junk; pure-punctuation
lines become voiceless. Stale clips auto-purged + regenerated. For native neural voices,
use the Piper pipeline (D-059). One file changed; no runtime/asmdef change.

---

## 🎭 Phase 53 — Polish Menu Layer 🟢 (2026-05-29)

The settings + onboarding polish the player asked for: **language (English /
العربية)**, **Reset Game → title**, and a **New-Game Character Creator** (skin /
outfit / accessory / name). Decisions **D-065** (localization) + **D-066**
(procedural appearance).

### Files added / edited

| Path | Status |
|---|---|
| `Scripts/Core/LocalizationService.cs` | NEW — EN/AR key table, live event, RTL flag |
| `Scripts/Core/SettingsService.cs` | EDIT — `Language` + character prefs + `ClearCharacterCreation()` |
| `Scripts/UI/LocalizedText.cs` | NEW — TMP localization binder + RTL alignment |
| `Scripts/UI/CharacterCreationUI.cs` | NEW — skin/outfit/accessory/name picker |
| `Scripts/UI/SystemMenuUI.cs` | NEW — language / customize / reset (in-panel confirm) |
| `Scripts/UI/MainMenuController.cs` | EDIT — `systemMenu` ref + New-Game `NewGameGate` |
| `Scripts/Mission/CharacterAppearance.cs` | NEW — palette + avatar tint/accessory applier |
| `Scripts/Mission/PolishMenuCoordinator.cs` | NEW — Save-aware reset/customize/gate bridge |
| `Scripts/Editor/Phase53_PolishMenuBuilder.cs` | NEW — builds screens + wiring; Build-Everything Step 16 |
| `Scripts/Editor/Phase27_BuildEverything.cs` | EDIT — chains Phase 53 last |

### Player flow

```
New Game → (Tone Compass) → Character Creator (skin · outfit · cap/flower/scarf · name)
  → first scene; avatar wears the chosen look.
Settings (Main Menu or Pause→Settings):
  • Language: English / العربية  (live — UI chrome flips + RTL for Arabic)
  • Customize Character           (re-open the creator)
  • Reset Game → confirm → wipe saves + reset VillageState + clear character → Title
                 (language / audio / comfort settings are kept)
```

### Decisions

- **D-065 (Localization)** — `LocalizationService` maps string keys → {en, ar} for
  UI chrome with a live `OnLanguageChanged` event + RTL flag; the language selector
  persists via `SettingsService.Language`. Hand-written **dialogue prose stays
  canonical English** (translation is a future writer pass, per GAME_DESIGN §9
  Pillar 1). Missing keys fall back to English then the key — never blank.
- **D-066 (Character appearance)** — Appearance is **all-procedural** (material
  colour tints + a code-built accessory primitive); **no new art** ships (matches the
  Depth Layer's "procedural primitives only" discipline). Stored in PlayerPrefs as a
  player profile (not per-save). "Reset Game" keeps language/comfort and clears the
  character so New Game re-opens the creator.

### Deviations / notes
- Persistence intentionally in `SettingsService` (PlayerPrefs), **not** `VillageState`/
  the save snapshot — avoids touching the save schema; appearance + language are global
  player prefs.
- Skin-tone tinting is best-effort (renderer/material name match); outfit tint +
  accessory are the guaranteed-visible knobs on any rig.

### Follow-ups
- **HH-LOC-DLG** — full Arabic dialogue/ledger translation pass (writers) + Arabic
  TMP font asset with full glyph coverage + shaping.
- **HH-CHAR-ART** — swap the procedural accessory primitives for authored props when
  the art pass lands; per-save appearance if profiles are added.

*Doc cascade: this supplement · `ARCHITECTURE.md` (D-065/D-066) · `CHANGELOG.md`
([0.8.2-polish-menu]) · `STUDIO_LOG.md`.*

---

*Last updated: 2026-05-29 — Voice sanitiser + Polish Menu Layer.*
