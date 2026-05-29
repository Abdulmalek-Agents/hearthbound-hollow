# 🎙️ Hearthbound Hollow — Voice Casting (Canonical)

> **Maintained by:** Voice Casting Director (Phase 32) + Arabic Voice Director (Phase 60)
> **Pipeline:** Open-source [Piper TTS](https://github.com/rhasspy/piper) (D-059)
> **File-swap policy:** D-058 (per-character) + D-064 (per-locale) — any 22 kHz mono PCM16 .wav drops in
> over the procedural baseline (ElevenLabs / XTTS / booth-recorded actress); the runtime can't tell the difference.
> **Branch:** `feat/arabic-localization` (Phase 60 — Arabic Localization MVP)

This is the single source of truth for which Piper voice model is
cast as which character. It is referenced by:

- `Tools/generate_voices.sh` (the `VOICES=()` table at the top, English)
- `Tools/generate_voices_ar.sh` (Phase 60 — the Arabic mirror)
- `Tools/download_voice_models.sh` (the `MODELS=()` table — 4 EN + 1 AR)
- `Assets/_Project/Scripts/Editor/Phase32_VoiceLibraryBuilder.cs`
  (per-character `volume` / `pitch` defaults on first import)
- `Assets/_Project/Scripts/Editor/Phase60_ArabicVoiceLibraryBinder.cs`
  (binds Arabic .wav files into the matching `clipAr` slots)
- `Assets/_Project/Scripts/Audio/VoiceLibrarySO.cs` (the lineId convention + `clipAr` field)
- `Assets/_Project/Scripts/Audio/VoicePlayer.cs` (locale-aware playback)

When you change a row here, update the references above in lockstep.

---

## 1. Casting at a glance (English)

| # | Character | Piper model | length_scale | Lines | Status |
|---|---|---|---|---|---|
| 1 | **Doris** (cozy elderly baker) | `en_US-lessac-medium` | 1.15 (slower) | 55 | ✅ Mission 1 fully wired |
| 2 | **Gerrold** (the Widower) | `en_GB-alan-medium` | 1.05 (slightly slower) | 8 | 🟡 M2 stub — pre-recorded |
| 3 | **Marin** (predecessor's notes) | `en_US-amy-medium` | 1.20 (slow, soft) | 4 | 🟡 Whisper stub |
| 4 | **Narrator** (title cards / Dream framing) | `en_GB-jenny_dioco-medium` | 1.05 | 4 | 🟡 Stub |
| 5 | **Pickle** (cat narrator — italic asides) | `en_US-amy-medium` | 0.95 (quicker, sly) | 6 | 🟡 Stub |

**Total:** 77 voice clips covering Mission 1 + Mission 2 hooks.

For the Arabic casting table see § 7 below.

---

## 2. Per-character notes

Every character row below maps to a **`Depth_Bible/`** reference,
which is the canonical voice-signature document. The Piper choice was
auditioned against the sample voice excerpts in §2 of each character's
bible.

### 2.1 Doris (Doris Vance, the Baker)

- **Bible:** [`Docs/Depth_Bible/Mission_1_2_Focus/01_DORIS_THE_BAKER.md`](Depth_Bible/Mission_1_2_Focus/01_DORIS_THE_BAKER.md) § 2 Voice Signature
- **Six rules** (paraphrased): bread metaphors; never names Elric in M1; short sentences (~12 words); eye contact gently broken; offers tea unprompted; no goodbye at end of M1.
- **Piper choice:** `en_US-lessac-medium`
  - **License:** Public Domain (Blizzard 2013 voice corpus, lessac).
  - **`length_scale = 1.15`** — slightly slower than default so the cozy unhurried register lands.
- **Runtime tuning:** `volume = 0.95`, `pitch = 0.98`
- **HuggingFace:** <https://huggingface.co/rhasspy/piper-voices/tree/main/en/en_US/lessac/medium>
- **Arabic counterpart:** § 7.1 / row 1.

### 2.2 Gerrold (Gerrold Pell, the Widower)

- **Bible:** [`Docs/Depth_Bible/Mission_1_2_Focus/02_THE_WIDOWER_GERROLD.md`](Depth_Bible/Mission_1_2_Focus/02_THE_WIDOWER_GERROLD.md) § 2 Voice Signature
- **Seven rules** (paraphrased): apologises constantly; talks like to a doctor; uses the verbal tic "the long bit"; never asks for sympathy; carpenter's vocabulary; brings the orb in a cloth; **does not cry**.
- **Piper choice:** `en_GB-alan-medium` · `length_scale = 1.05`
- **Runtime tuning:** `volume = 0.90`, `pitch = 0.92`
- **HuggingFace:** <https://huggingface.co/rhasspy/piper-voices/tree/main/en/en_GB/alan/medium>
- **Arabic counterpart:** § 7.1 / row 2. Note: Arabic uses the same female `ar_JL-medium` model with `pitch = 0.88` to approximate a baritone until an Arabic male model lands upstream.

### 2.3 Marin (the predecessor — whispered notes only)

- **Voice signature:** soft, female, "kind in the way that does not announce itself".
- **Piper choice:** `en_US-amy-medium` · `length_scale = 1.20`
- **Runtime tuning:** `volume = 0.75`, `pitch = 1.05`
- **Arabic counterpart:** § 7.1 / row 3.

### 2.4 Narrator (title cards + Dream framing)

- **Use:** `MissionTitleCard.cs` + `MemoryDreamSequencer` opening/closing prose.
- **Piper choice:** `en_GB-jenny_dioco-medium` · `length_scale = 1.05`
- **Runtime tuning:** `volume = 0.95`, `pitch = 1.00`
- **Arabic counterpart:** § 7.1 / row 4.

### 2.5 Pickle (the cat narrator)

- **Bible:** Codex 07 § 3.1 rule 7 — lines are *internal*: only the player hears her. Italic dim-amber render in `DialogueUI`.
- **Piper choice:** `en_US-amy-medium` · `length_scale = 0.95`
- **Runtime tuning:** `volume = 0.80`, `pitch = 1.10`
- **Arabic counterpart:** § 7.1 / row 5.

---

## 3. The 77 lines (English) · 77 lines (Arabic, 1:1 mirror)

Per-character line counts and ids. Each `lineId` is the filename
basename of its `.wav` under `Assets/_Project/Audio/Voice/<Character>/`
(English) or `Assets/_Project/Audio/Voice/ar/<Character>/` (Arabic).

### Doris — 55 lines (full Mission 1 coverage)

| Beat | Line IDs | Count |
|---|---|---|
| Greeting | `doris_m1_greet_01..04` | 4 |
| Reply — asked "Are you Doris?" | `doris_m1_reply_help_01..02` | 2 |
| Reply — silent nod | `doris_m1_reply_silent_01..02` | 2 |
| Reply — asked "Who was the old one?" | `doris_m1_reply_unsure_01..03` | 3 |
| Bakery entrance | `doris_m1_kitchen_01..04` | 4 |
| "First customer" preamble | `doris_m1_offer_01..02` | 2 |
| The iconic memory offer ("Hold it like a hot bun") | `doris_m1_memory_01..07` | 7 |
| Defer / refusal preamble | `doris_m1_defer_01..02` | 2 |
| "First Loaves" aside (age 24) | `doris_m1_story_01..05` | 5 |
| Price preamble + 3 branches | `doris_m1_price_01..02`, `doris_m1_price_fair`, `doris_m1_price_high_01..03`, `doris_m1_price_low_01..02` | 8 |
| Handover ("the cat watched me") | `doris_m1_handover_01..05` | 5 |
| Polish watch | `doris_m1_polish_watch` | 1 |
| Polish done + sleep | `doris_m1_polish_done_01..02`, `doris_m1_polish_sleep_01..02` | 4 |
| Refused-path branch | `doris_m1_refused_01..03` | 3 |
| Clarity-branching after-polish (perfect/acceptable/mild) | `doris_m1_polish_after_*` | 3 |
| **Total** | | **55** |

### Gerrold — 8 lines (Mission 2 stub)

| Beat | Line IDs | Count |
|---|---|---|
| Greeting | `gerrold_m2_greet_01..03` | 3 |
| "The long bit" verbal tic | `gerrold_m2_long_bit_01..02` | 2 |
| Thank-you exit | `gerrold_m2_thank_01..03` | 3 |
| **Total** | | **8** |

### Marin — 4 lines (whispered notes)

| Beat | Line IDs | Count |
|---|---|---|
| Lane note (kettle still works) | `marin_note_lane_01..02` | 2 |
| Hollow note (Pickle is fair) | `marin_note_hollow_01` | 1 |
| Workbench note (the cloth) | `marin_note_workbench_01` | 1 |
| **Total** | | **4** |

### Narrator — 4 lines (title cards)

| Line ID | Use |
|---|---|
| `narrator_title_day1` | Day 1 fade-in card |
| `narrator_title_day2` | Day 2 fade-in card |
| `narrator_title_evening` | Evening Ledger overlay opening |
| `narrator_title_dream` | Memory-Dream Sequencer pre-roll |

### Pickle — 6 lines (italic asides)

| Beat | Line IDs | Count |
|---|---|---|
| M1 asides (Doris's bakery / Hollow) | `pickle_m1_aside_01..03` | 3 |
| M2 asides (Gerrold / choice / outcome) | `pickle_m2_aside_01..03` | 3 |
| **Total** | | **6** |

---

## 4. Adding a new character

When Mission 3+ adds a new villager:

1. Add a `VOICES=()` row in `Tools/generate_voices.sh` (and the Arabic mirror in `generate_voices_ar.sh`).
2. Add a `MODELS=()` row in `Tools/download_voice_models.sh` if the Piper model isn't already downloaded.
3. Add the character's per-line entries in the `LINES=()` table.
4. Add a per-character `Audio/Voice/<Name>/.gitkeep` (English) + `Audio/Voice/ar/<Name>/.gitkeep` (Arabic).
5. Add the row to this document under § 1 + § 2 (and § 7 for Arabic).
6. Optionally, add per-character default `volume` / `pitch` to `Phase32_VoiceLibraryBuilder.cs`.

The runtime needs no change.

---

## 5. Swapping a voice (per-character)

To swap a Piper voice for an ElevenLabs voice clone or a booth-recorded actress:

1. Generate new `.wav` files with the **same lineIds**. Format: 22 kHz mono PCM16.
2. Drop into `Assets/_Project/Audio/Voice/<Character>/` (English) or `Audio/Voice/ar/<Character>/` (Arabic).
3. Open Unity → `🎙️ Phase 32 — Rebuild Voice Library` (English) or `🎙️ Phase 60 — Bind Arabic Voice Clips` (Arabic).
4. Press Play. **No code change.**

This is **D-058** (file-swap policy) + **D-064** (per-locale slots).

---

## 6. License notes

All Piper voice models referenced here are open-source licensed:

| Model | License |
|---|---|
| `en_US-lessac-medium` | Public Domain (Blizzard 2013 corpus) |
| `en_GB-alan-medium` | Public Domain |
| `en_US-amy-medium` | Public Domain |
| `en_GB-jenny_dioco-medium` | CC-BY-4.0 (credit `Jenny Dioco` in any commercial release) |
| `ar_JL-medium` | Public Domain — adds Arabic locale (Phase 60) |

The Piper inference code (`piper-tts` PyPI / GitHub) is MIT-licensed.

The .onnx model files themselves are **not committed** to git
(see `.gitignore`); developers run `bash Tools/download_voice_models.sh`
once after cloning.

---

## 7. Arabic locale casting (Phase 60 — Arabic Localization MVP)

Single Piper voice model — `ar_JL-medium` — is the only medium-quality
Arabic voice in the rhasspy/piper roster as of 2026-05. We
differentiate each character via per-character `length_scale` (Piper)
and per-character runtime `volume` / `pitch` (`VoiceLibrarySO.Entry`).

### 7.1 Casting at a glance — Arabic

| # | Character | Piper model | length_scale | Runtime pitch | Runtime volume | Notes |
|---|---|---|---|---|---|---|
| 1 | **دوريس (Doris)** | `ar_JL-medium` | 1.15 (slow, unhurried) | 0.97 | 1.00 | Cozy female; the source-voice register matches the source English Doris best. |
| 2 | **جيرولد (Gerrold)** | `ar_JL-medium` | 1.10 | **0.88** | 0.92 | Pitch dropped to approximate a grandfatherly baritone — sole "invasive" runtime move. Reverts when an Arabic male model lands upstream. |
| 3 | **مارين (Marin)** | `ar_JL-medium` | 1.20 (whisper-slow) | 1.05 | 0.78 | Lower volume + slight pitch raise = "at-a-distance, in memory". |
| 4 | **الراوي (Narrator)** | `ar_JL-medium` | 1.05 | 1.00 | 0.95 | Neutral literary delivery. |
| 5 | **بيكل (Pickle)** | `ar_JL-medium` | 0.95 (quick, sly) | 1.12 | 0.82 | Brighter + faster — recognisably the cat voice without confusing with Marin's whisper. |

**Total Arabic clips:** 77 (1:1 mirror of the English cast).

See [`Docs/Depth_Bible/17_LOCALIZATION_ARABIC.md`](Depth_Bible/17_LOCALIZATION_ARABIC.md) (Codex 17) for the canonical Arabic voice signatures per character.

### 7.2 File-swap policy (Arabic)

`D-064` extends `D-058` to a per-language slot:
- English clips live under `Assets/_Project/Audio/Voice/<Character>/<lineId>.wav`.
- Arabic clips live under `Assets/_Project/Audio/Voice/ar/<Character>/<lineId>.wav`.
- `VoiceLibrarySO.Entry` has both an English `clip` and an Arabic `clipAr` field.
- `VoicePlayer.Play(lineId)` reads `ServiceLocator.Get<LocalizationService>().CurrentLocale` and selects `clipAr` when Arabic is active and the slot is non-null. Falls back to `clip` silently.

A future Arabic male voice model (`ar_*-male-medium`), a commercial
Arabic VO actor, or an ElevenLabs Arabic voice clone all replace
files at `Assets/_Project/Audio/Voice/ar/<Character>/<lineId>.wav` and
re-run `Hearthbound → ⚙️ Advanced → 🎙️ Phase 60 — Bind Arabic Voice Clips`.
**No code change.**

### 7.3 Subtitle integrity guarantee

Subtitles are **always** translated per the active locale —
`DialogueUI.PresentLine` calls `LocalizationService.GetDialogue(lineId, englishOriginal)`
before any voice resolution happens. So even when an Arabic clip is
missing and the player hears English audio, the on-screen subtitle is
the canonical Arabic translation from `dialogue.ar.json`. This means
Phase 60 ships day-one with **100% Arabic subtitles + as much Arabic
voice as the .wav set covers** — a graceful degrade path that lets a
team ship Arabic + iterate on the voice quality over time.

### 7.4 Cultural adaptation notes

See Codex 17 [`Docs/Depth_Bible/17_LOCALIZATION_ARABIC.md`](Depth_Bible/17_LOCALIZATION_ARABIC.md) § 3 for the canonical Arabic term table:

- The Hollow → الجَوْف (phonetic + meaning)
- Memory orb → كُرَةُ ذِكرى (chose "ذكرى" over "ذاكرة" — lived-memory connotation)
- Cleanse → تنقية (avoiding "تطهير" — religious overtones)
- The Keeper → الكَيِّل / صاحبُ الجَوْف (Doris addresses the player as "أيُّها الكَيِّل")

Theological-sensitivity pass — no Qur'anic phraseology in casual dialogue, no religious-specific grief liturgy in the M2 widower scene. Universal vocabulary: فِقدان (loss), احتضار (terminal illness), حُزن (grief).

---

*End of `Docs/VOICE_CASTING.md` v1.2 (Phase 60 — Arabic Localization MVP, § 7 expanded with file-swap + cultural adaptation).*
