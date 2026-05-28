# 🌍 Codex 17 — Localization & Arabic Voice Signature

> **Phase 60 — Arabic Localization MVP.**
> **Authors:** Localization Lead Architects, Arabic Linguistic Team, Voice Casting Director.
> **Status:** Canonical. Apply to every dialogue line, UI string, and voice clip added in Arabic for Mission 1 + Mission 2.

This codex is the source-of-truth for the **voice register, idiom, and
emotional cadence** of the Arabic translation of Hearthbound Hollow. It
sits beside the existing 16 Depth Bible codices and is referenced by:

- `dialogue.ar.json` — every translated line is held to the standards
  below.
- `Tools/generate_voices_ar.sh` — the Piper text strings + length_scale
  per-character choices.
- The Arabic voice casting table in `Docs/VOICE_CASTING.md` § 7.

---

## 1. Register — قرار الفصحى الأدبية الدافئة

We translate into **Modern Standard Arabic** (الفصحى), with a
**literary-cozy register** tuned for prose fiction. We do NOT use:

- ❌ Heavy classical / Qur'anic register — it would push the cozy
  tone into formality.
- ❌ Country-specific dialect — would alienate any reader outside
  that country.
- ❌ Pure colloquial transliteration — visually breaks the parchment
  aesthetic.

We **do** use:

- ✅ Short sentences (matches Doris's ~12-word average).
- ✅ Light tashkīl (تشكيل) only where ambiguity might mislead a
  silent reader.
- ✅ Native Arabic punctuation: comma `،`, semicolon `؛`, question
  mark `؟`.
- ✅ Quotation marks `«…»` for embedded speech (French-style
  guillemets — the cleanest in a parchment frame).

---

## 2. Per-character voice signatures (Arabic)

### 2.1 دوريس Doris — الخبّازة الدافئة

| Rule (Vellis § 2.1 → Arabic adaptation) | Worked example |
|---|---|
| Bread metaphors for everything | "أَمسِكها كأنَّكَ تُمسِكُ رغيفًا ساخنًا." |
| Short sentences (~10–12 words avg) | "الخُبزُ يُحِبُّ الهُدوء." |
| Never says Elric's name in M1 | (preserved by content — no Arabic exception) |
| Eye contact made then gently broken | (stage direction — handled by animator) |
| Offers tea unprompted | "ادخُل. الشايُ أوّلًا." |
| Does not say "وداعًا" at end of M1 | Use "سَأَراكَ ثانيةً، يَومًا ما." |

**Vocabulary tilt:** prefer **الكلمات المؤنسة** (warming words) over
neutral synonyms. "أَدخُل" → "تَفَضَّل" only when the warmth needs a
push; default to the simpler "ادخُل."

**Forbidden words:** any modern slang, English loanwords, or jargon
that breaks the autumnal-village setting.

### 2.2 جيرولد Gerrold — الأرمَل

| Rule (Cordray § 2.2) | Arabic worked example |
|---|---|
| Apologises constantly | "أَعتَذِر. لا أَعرِفُ كيفَ يَجري هذا الأمر." |
| Talks like to a doctor (precise, hesitant) | "في هذا القُماش." |
| Verbal tic "the long bit" → **"الجزءَ الطويل"** | The cornerstone phrase. Never paraphrase. |
| Never asks for sympathy | (handled by content — no exception) |
| Carpenter's vocabulary | "وَصلَة الخَشَب صَلَحَت، لكنَّ الخَشَبَ خَذَلَنا." |
| Brings the orb in a cloth | "مَعي ذاكَ الشَّيء — في هذا القُماش." |
| Does NOT cry | Reflected by writing — no exclamation marks. |

**Cornerstone phrase:** "الجزءَ الطويل" is the canonical Arabic
rendering of "the long bit." Every Cordray line that references it
uses this exact construction. **Do not paraphrase to** "الجزء الصعب"
or "الفترة الطويلة" — the phrase is a thematic anchor, not a
description.

### 2.3 مارين Marin — السلَفة

Short. Whispered. Notes only — Marin is never present, only her words
on paper.

**Vocabulary tilt:** very short sentences. Light archaic flavour
(she's the predecessor — the dignity of the older keeper).

```
"إن وَجَدتَ هذا، فالإبريقُ ما زالَ يَعمَل."
"بيكل تَتَذَكَّرُ الجَميع. بيكل عادلة."
```

No exclamation marks. Sign off "— م."

### 2.4 بيكل Pickle — القطّة الراوية

Italic in UI. Sly, ageless, observational. Translated into Arabic
with a **light ironic edge** but never sarcastic — Pickle is wise, not
cruel.

**Vocabulary tilt:** sentence fragments allowed (matches the cat's
elliptical voice). Repetition for cadence ("تُراقِبُك. هي دائمًا
تُراقِب.").

### 2.5 الراوي Narrator — لتقديم بطاقات الفصول

Neutral literary Arabic. Reads like the opening line of a short
story.

```
"تُغمِضُ عينيها. تبدأُ الذِّكرى."
"اليومُ الأوّل. الجَوْف."
```

---

## 3. Culturally adapted terms

| English source | Arabic canonical | Note |
|---|---|---|
| The Hollow | الجَوْف | Phonetic + meaning. Used as proper noun without ال in titles. |
| Memory orb | كُرَةُ ذِكرى | Not "كرة ذاكرة" — "ذِكرى" carries the lived-memory connotation. |
| Polish (verb) | لَمَّعَ / تلميع | Not "صَقَلَ" — too craftsman; "لَمَّعَ" is gentler. |
| Cleanse | نَقَّى / تنقية | Choice of "تنقية" over "تطهير" — religious overtones avoided. |
| The Keeper | الكَيِّل / صاحبُ الجَوْف | Honorific. Doris addresses the player as "أيُّها الكَيِّل" in voice. |
| Vow (Honor the named, etc.) | عَهد | "وَعد" is too modern; "عَهد" carries weight. |
| Coppers (currency) | نُحاسات | Plural of نُحاسة. |
| Tea | شاي | Universal. No need to choose between mint / black variety. |
| Tone Compass | بوصلة النَّبرة | A neologism the player meets once at game start. |
| Echo Web | شَبَكَةُ الذاكرة | "Memory Web" is the better metaphor in Arabic. |

### 3.1 Theological-sensitivity pass

We deliberately avoid:
- ❌ Quranic phraseology in dialogue (e.g. "إن شاء الله" in casual
  use). The game's spiritual register is the cozy domestic, not the
  religious.
- ❌ Direct rendering of "soul" — use "روح" only when the source
  text already invokes the metaphysical. The English text uses
  "memory" precisely to side-step that — Arabic does the same.
- ❌ Phrases tied to specific Islamic / Christian / Jewish liturgy
  for grief. The widower scene (Mission 2 cottage) uses universal
  human grief vocabulary: "فِقدان" (loss), "احتضار" (terminal
  illness, referenced from a remove), "حُزن" (grief). Reviewed by
  the linguistic team to be acceptable across all Arab cultures.

---

## 4. RTL technical conventions

### 4.1 Inline number formatting

Arabic-Indic digits (٠١٢٣٤٥٦٧٨٩) vs Western (0123456789):
- Numbers in HUD chips (Day / coin count) → **Western digits** for
  legibility on small UI elements.
- Numbers in long-form narrative prose → **Arabic-Indic digits** for
  literary register.

Day labels stick with Western: `اليومُ 1` reads naturally.

### 4.2 Quotation marks

Use guillemets `«...»` for inline speech. Modern Arabic typesetting
accepts them and they don't break the parchment box. Reserve `"..."`
for English embedded phrases.

### 4.3 Em-dash → "—" or "، "

The English source uses em-dashes liberally for cozy parenthetical
asides. In Arabic we either:
- Keep the em-dash (`—`) for typographic stage direction.
- Replace with `،` (Arabic comma) when the em-dash is functioning
  purely as a comma in English prose.

The `Tools/generate_voices_ar.sh` sanitiser does the comma swap
automatically before feeding text to Piper — translators don't need to
think about it.

---

## 5. Voice acting register (Piper TTS, ar_JL-medium)

Single Arabic voice model. Per-character differentiation via runtime
tuning:

| Character | length_scale | runtime pitch | runtime volume |
|---|---|---|---|
| Doris    | 1.15 | 0.97 | 1.00 |
| Gerrold  | 1.10 | 0.88 | 0.92 |
| Marin    | 1.22 | 1.05 | 0.78 |
| Narrator | 1.05 | 1.00 | 0.95 |
| Pickle   | 0.95 | 1.12 | 0.82 |

**Gerrold's pitch drop to 0.88** is the only invasive runtime move —
the source voice is female, and we push it downward to approximate a
grandfatherly baritone. Result is recognisably distinct from Marin
without being uncannily processed.

When a real Arabic male voice model lands (rhasspy/piper roadmap),
Gerrold's `clipAr` field is the only one that needs to change.
**D-058's** file-swap policy holds.

---

## 6. Open questions / future work

- **Phase 61** — Full BiDi (UAX #9) implementation for multi-paragraph
  letters in the Reading Nook (Phase 52). Current `ArabicTextShaper`
  covers single-line dialogue / menu labels / short HUD chips.
- **Phase 62** — Persian + Urdu locales would reuse the same shaper
  with extended `LetterForms` rows (Persian gaf/peh/cheh/zhe and
  Urdu retroflex letters).
- **Phase 63** — A French translation does NOT need the shaper or
  the RTL mirror — just a new `loc.fr.json` and a French Piper voice
  model. Validates the abstraction.
- **Phase 64** — Editor menu item that exports `loc.<iso>.json` to
  CSV for translator-friendly spreadsheet workflows + re-imports the
  edited CSV back. (The flat-JSON parser already tolerates trailing
  commas + comments to make manual editing comfortable.)

---

*Document version 1.0 — Phase 60 Arabic Localization MVP — Codex 17 of the Depth Bible.*
