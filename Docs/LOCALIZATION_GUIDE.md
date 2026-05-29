# 🌐 Hearthbound Hollow — Localization Guide

> **Maintained by:** Localization Lead Architects + Arabic Linguistic Team (Phase 60)
> **Branch:** `feat/arabic-localization` (off `feat/mission-1-2-architecture`)
> **Status:** ✅ Arabic MVP shipping for Mission 1 + Mission 2

This is the canonical guide for everything language-related in
Hearthbound Hollow:

- the runtime `LocalizationService` API
- the JSON translation tables (UI strings + dialogue lines)
- right-to-left text shaping (Arabic glyph joining for TMP)
- right-to-left layout mirroring
- per-language voice acting via Piper TTS
- the editor workflow for adding a new language

---

## 1. Architecture at a glance

```
┌─────────────────────────────────────────────────────────────┐
│                  HearthboundHollow.Core                    │
│                                                             │
│   Locale enum  ────►  LocaleInfo (ISO, native-name, RTL)   │
│       ▲                                                     │
│       │                                                     │
│   LocalizationService                                       │
│       ├─ Get(key)        ◄── Resources/loc.<iso>.json      │
│       ├─ Format(key, …)                                     │
│       ├─ GetDialogue(id) ◄── Resources/dialogue.<iso>.json │
│       ├─ GetSpeakerName                                     │
│       ├─ SetLocale  ────►  EventBus.Publish(               │
│       │                       LocaleChangedEvent)           │
│       └─ persistence: PlayerPrefs "hh.locale.iso"          │
│                                                             │
│   ArabicTextShaper.Shape(string)                            │
│       BiDi-lite + glyph-joining + lam-alef ligatures        │
│       → ready-to-render TMP string                          │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                   HearthboundHollow.UI                      │
│                                                             │
│   LocalizedText (TMP_Text component)                        │
│       • inspector key field → resolved on Awake +           │
│         every LocaleChangedEvent                            │
│       • applies Arabic shaping + RTL alignment              │
│                                                             │
│   RtlLayoutMirror                                           │
│       • flips HorizontalLayoutGroup.reverseArrangement      │
│       • mirrors anchored RectTransform X for side-panels    │
│                                                             │
│   DialogueUI · MainMenuController · PauseMenuUI ·           │
│   HelpOverlayUI · OnboardingOverlay · ToneCompassCard ·     │
│   ComfortToolsMenu · EveningLedgerUI · HUDController ·      │
│   MissionTitleCard · ChoiceCardUI · CodexUI                 │
│       — each subscribes to LocaleChangedEvent and refreshes │
│       its labels via LocalizationService.Get(key).          │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                  HearthboundHollow.Audio                    │
│                                                             │
│   VoiceLibrarySO.Entry                                      │
│       • clip   (English, source-of-truth)                   │
│       • clipAr (Arabic — Phase 60 addition)                 │
│       • volume, pitch                                       │
│                                                             │
│   VoicePlayer.Play(lineId)                                  │
│       • reads CurrentLocale from LocalizationService        │
│       • picks clipAr when Locale.Arabic and not null        │
│       • falls back to clip (English) otherwise              │
│       • subtitle ALWAYS translated by DialogueUI            │
└─────────────────────────────────────────────────────────────┘
```

---

## 2. The translation tables

### 2.1 UI strings — `Assets/_Project/Localization/Resources/loc.<iso>.json`

Flat `{ "key.path": "string" }` map. Keys follow a `screen.subsystem.element`
convention. The English file is the **source of truth** — every other
locale is a translation against it.

```json
{
  "menu.main.cta.open_hollow": "Open The Hollow",
  "pause.title":               "Paused",
  "hud.day_label_fmt":         "Day {0}",
  "speaker.doris":             "Doris"
}
```

**Conventions:**

- Format-string placeholders use C# `{0}`, `{1}` (not `%s`).
- Speaker-name keys live under `speaker.<lowercase>` — they get looked up
  by `LocalizationService.GetSpeakerName("Doris")` from DialogueUI.
- Line comments (`// translator note`) are tolerated by the parser.
- Native-language names (English / العربية) are **never translated** —
  they appear in the language picker exactly as a speaker of that
  language would write them.

### 2.2 Dialogue lines — `Assets/_Project/Localization/Resources/dialogue.<iso>.json`

Same JSON format. Keys are the stable `lineId` strings already in
`Tools/generate_voices.sh` and `Mission01Director.cs`'s `Line(...)` calls.

```json
{
  "doris_m1_greet_01": "أنتَ الجديد.",
  "doris_m1_greet_02": "كنتُ أحسَبُكَ أَطوَل."
}
```

**Rule (D-060):** the English authored line in the Director script is
the source of truth. The English `dialogue.en.json` is intentionally
empty — `LocalizationService.GetDialogue` returns the English original
unchanged when the active locale is English. Other-language files only
need to populate the lineIds they've translated; missing entries cause
the line to fall back to the English original with a one-time warning.

---

## 3. Adding a new language

```
1. Add a row to the Locale enum:
       Assets/_Project/Scripts/Core/Localization/Locale.cs

2. Fill in the LocaleInfo helpers:
       IsoCode, FromIsoCode, NativeName, IsRightToLeft, VoiceFolder

3. Create two empty translation tables:
       Assets/_Project/Localization/Resources/loc.<iso>.json
       Assets/_Project/Localization/Resources/dialogue.<iso>.json

4. Translate every key from loc.en.json into loc.<iso>.json.
   (Comments are allowed — translator notes belong inline.)

5. Translate every lineId in dialogue.ar.json into dialogue.<iso>.json
   (or just the ones recorded as voice — rest will fall back to English).

6. If RTL: confirm ArabicTextShaper still applies (it covers Arabic,
   Persian, Urdu — extend the LetterForms table if not).

7. If voice acting: add a row to Tools/download_voice_models.sh and
   create a Tools/generate_voices_<iso>.sh mirror of generate_voices_ar.sh.

8. Add a language button to ComfortToolsMenu (inspector wiring is all
   that's required — OnLanguagePicked + ApplyLocalization auto-route).
```

---

## 4. The runtime API

### `LocalizationService.Get(string key)`

Returns the active-locale string. Falls back to English then to the key
itself with a one-time warning. **Never throws.**

### `LocalizationService.Format(string key, params object[] args)`

Composite-format flavor — looks the format string up, runs
`string.Format`. Returns the format string unchanged if formatting
throws (so a malformed translation can't crash the UI).

### `LocalizationService.GetDialogue(string lineId, string englishOriginal)`

Returns the active-locale dialogue translation, or the English original
when no translation exists for this lineId. Used by `DialogueUI.PresentLine`.

### `LocalizationService.GetSpeakerName(string englishSpeaker)`

Returns the translated speaker label (`"Doris"` → `"دوريس"`). Falls
back to the input unchanged when no translation exists.

### `LocalizationService.SetLocale(Locale)`

Switches the active language, persists to PlayerPrefs, publishes
`LocaleChangedEvent` on the EventBus. Every UI script that subscribes
to that event re-pulls its strings on the same frame.

### `LocaleChangedEvent`

```csharp
public readonly struct LocaleChangedEvent
{
    public readonly Locale PreviousLocale;
    public readonly Locale CurrentLocale;
    public bool RtlDirectionChanged { get; }
}
```

---

## 5. Right-to-left rendering

### 5.1 Text shaping

Arabic is a cursive script: each letter has up to four contextual
presentation forms (Isolated / Initial / Medial / Final) depending on
its neighbours. Unity's TextMeshPro renders the isolated forms only —
the input `العالم` would appear as `ا ل ع ا ل م`, broken.

`ArabicTextShaper.Shape(source)` walks the input, substitutes each
codepoint with its contextual form (U+FE80 block), combines mandatory
LAM+ALEF ligatures, and reverses the visual order so TMP lays the
glyphs out right-to-left.

The shaper segments mixed strings — embedded Latin runs (e.g. `"OK مرحبا"`)
are preserved in their original LTR order while the Arabic run is
shaped + reversed independently.

### 5.2 Layout mirroring

`RtlLayoutMirror` (UI asmdef) is a one-line drop on any
HorizontalLayoutGroup that needs to flip its arrangement on RTL. It
also exposes an `anchorMirror` toggle for side-pinned panels (e.g. the
ControlHintsHUD parchment chips at bottom-left → bottom-right in RTL).

### 5.3 Font asset

TMP renders glyphs from an SDF atlas. The default `LiberationSans SDF`
ships only Latin glyphs — Arabic needs a font asset that covers
U+0600..U+06FF + U+FE70..U+FEFF.

We ship a one-click installer:

```
Hearthbound → ⚙️ Advanced → 🔤 Phase 60 — Install Arabic Font
```

It downloads Noto Naskh Arabic (OFL-licensed) and walks the user
through generating the SDF atlas via TMP's Font Asset Creator.

---

## 6. Voice acting in Arabic

### 6.1 Pipeline

```
Tools/generate_voices_ar.sh
    → uses Piper TTS with ar_JL-medium (single voice)
    → length_scale + pitch per character (Doris 1.15, Pickle 0.95, etc.)
    → writes Assets/_Project/Audio/Voice/ar/<Character>/<lineId>.wav

Hearthbound → ⚙️ Advanced → 🎙️ Phase 60 — Bind Arabic Voice Clips
    → scans Audio/Voice/ar/ for .wav files
    → binds each into the matching VoiceLibrarySO entry's clipAr slot
    → idempotent: re-running is a no-op
```

### 6.2 Runtime resolution

```csharp
// VoicePlayer.Play(lineId)
var loc = ServiceLocator.Get<LocalizationService>();
AudioClip chosen = entry.clip;                                // English source
if (loc.CurrentLocale == Locale.Arabic && entry.clipAr != null)
    chosen = entry.clipAr;                                    // Arabic recorded
// Subtitle is ALWAYS translated; voice degrades to English when missing.
```

This is **D-058**'s file-swap policy applied to a second language: any
22 kHz mono PCM16 .wav that drops into `Audio/Voice/ar/<Character>/`
with the right lineId basename gets picked up on the next bind, no
code change required. ElevenLabs / XTTS / booth-recorded actress all
fit the same path.

### 6.3 Casting

Single Piper voice (`ar_JL-medium`) with per-character `length_scale`
+ runtime `pitch` differentiation. See
[`Docs/VOICE_CASTING.md`](VOICE_CASTING.md) § 7 for the per-character
rationale (added in Phase 60).

When a commercial Arabic voice actor or a higher-quality Arabic Piper
model becomes available, swap the .wav files into
`Audio/Voice/ar/<Character>/` and re-run the binder. No code change.

---

## 7. Editor workflow recap

```
First time setup
────────────────
1. bash Tools/download_voice_models.sh     # ~310 MB EN + AR models
2. bash Tools/generate_voices.sh            # ~30 s, 77 EN clips
3. bash Tools/generate_voices_ar.sh         # ~25 s, 77 AR clips
4. Open Unity → Hearthbound → ⚙️ Advanced →
       🔤 Phase 60 — Install Arabic Font
   (walk the TMP Font Asset Creator dialog once)
5. Hearthbound → 🚀 Build Everything
   (chains Phase 32 EN voice binder + Phase 60 AR voice binder)
6. Press Play. Main Menu → اللغة → العربية. Doris speaks Arabic.

After translation edits
───────────────────────
• Edit loc.ar.json / dialogue.ar.json — changes hot-reload on the
  next LocaleChangedEvent (or scene reload).

After voice re-record
─────────────────────
• Overwrite .wav files in Audio/Voice/ar/<Character>/.
• Hearthbound → ⚙️ Advanced → 🎙️ Phase 60 — Bind Arabic Voice Clips.
```

---

## 8. Decisions (D-060 → D-064)

- **D-060** *(Phase 60)* English authored text is the source of truth.
  Every other locale is a translation against the source. Missing
  translation → English fallback + one-time warning.
- **D-061** *(Phase 60)* `LocalizationService` MUST be registered with
  the `ServiceLocator` before any scene's `Awake()` runs. Two
  installers (`SettingsServiceBootstrap` + `LocalizationBootstrap`'s
  `RuntimeInitializeOnLoad`) guarantee this.
- **D-062** *(Phase 60)* TMP labels that need locale-aware text MUST
  either use the `LocalizedText` component OR subscribe to
  `LocaleChangedEvent` and call `LocalizationService.Get` themselves.
  Hard-coded English in `.text = "…"` is a bug.
- **D-063** *(Phase 60)* Arabic glyph shaping (`ArabicTextShaper.Shape`)
  is the runtime's responsibility, not the translator's. The
  `loc.ar.json` file contains canonical-form (logical-order) Arabic.
  The renderer transforms it into presentation forms.
- **D-064** *(Phase 60)* Voice clips are language-tagged: the English
  clip lives under `Audio/Voice/<Character>/`, the Arabic clip under
  `Audio/Voice/ar/<Character>/`. `VoiceLibrarySO.Entry` has a
  per-language slot (`clip` + `clipAr`). Missing Arabic clip → falls
  back to English audio while the subtitle stays translated.

---

*Document version 1.0 — Phase 60 Arabic Localization MVP.*
