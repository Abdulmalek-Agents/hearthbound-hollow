# 🎙️ Hearthbound Hollow — Voice Casting (Canonical)

> **Maintained by:** Voice Casting Director (Phase 32)
> **Pipeline:** Open-source [Piper TTS](https://github.com/rhasspy/piper) (D-059)
> **File-swap policy:** D-058 — any 22 kHz mono PCM16 .wav drops in
> over the procedural baseline (ElevenLabs / XTTS / booth-recorded
> actress); the runtime can't tell the difference.
> **Branch:** `feat/mission-1-2-architecture`

This is the single source of truth for which Piper voice model is
cast as which character. It is referenced by:

- `Tools/generate_voices.sh` (the `VOICES=()` table at the top)
- `Tools/download_voice_models.sh` (the `MODELS=()` table)
- `Assets/_Project/Scripts/Editor/Phase32_VoiceLibraryBuilder.cs`
  (the per-character `volume` / `pitch` defaults applied to new entries)
- `Assets/_Project/Scripts/Audio/VoiceLibrarySO.cs` (the lineId convention)

When you change a row here, update the three references above in lockstep.

---

## 1. Casting at a glance

| # | Character | Piper model | length_scale | Lines | Status |
|---|---|---|---|---|---|
| 1 | **Doris** (cozy elderly baker) | `en_US-lessac-medium` | 1.15 (slower) | 55 | ✅ Mission 1 fully wired |
| 2 | **Gerrold** (the Widower) | `en_GB-alan-medium` | 1.05 (slightly slower) | 8 | 🟡 M2 stub — pre-recorded |
| 3 | **Marin** (predecessor's notes) | `en_US-amy-medium` | 1.20 (slow, soft) | 4 | 🟡 Whisper stub |
| 4 | **Narrator** (title cards / Dream framing) | `en_GB-jenny_dioco-medium` | 1.05 | 4 | 🟡 Stub |
| 5 | **Pickle** (cat narrator — italic asides) | `en_US-amy-medium` | 0.95 (quicker, sly) | 6 | 🟡 Stub |

**Total:** 77 voice clips covering Mission 1 + Mission 2 hooks.

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
  - **Why:** clear mid-range warm female voice with natural pacing.
    Lessac is the most widely-tested Piper voice — robust on long
    sentences, doesn't add hallucinated affect.
  - **`length_scale = 1.15`** — slightly slower than default so the
    cozy unhurried register lands. Doris is "older village voice,
    plain register, occasional Northern-rural cadence" per the bible;
    medium Piper voices don't model dialect, but slowing the rate
    approximates the unhurried delivery.
- **Runtime tuning:** `volume = 0.95`, `pitch = 0.98`
  - Slight downward pitch (0.98) gives a touch more weight without
    sounding artificially deep. Doris is *warm*, not *commanding*.
- **HuggingFace:** <https://huggingface.co/rhasspy/piper-voices/tree/main/en/en_US/lessac/medium>
- **Direct download:** <https://huggingface.co/rhasspy/piper-voices/resolve/main/en/en_US/lessac/medium/en_US-lessac-medium.onnx>

### 2.2 Gerrold (Gerrold Pell, the Widower)

- **Bible:** [`Docs/Depth_Bible/Mission_1_2_Focus/02_THE_WIDOWER_GERROLD.md`](Depth_Bible/Mission_1_2_Focus/02_THE_WIDOWER_GERROLD.md) § 2 Voice Signature
- **Seven rules** (paraphrased): apologises constantly; talks like to a doctor; uses the verbal tic "the long bit"; never asks for sympathy; carpenter's vocabulary (the joint was good but the wood gave); brings the orb in a cloth; **does not cry**.
- **Piper choice:** `en_GB-alan-medium`
  - **License:** Public Domain.
  - **Why:** mid-range British male, weathered but not theatrical.
    Alan is mid-30s to mid-40s in voice age; we lean on it for
    Gerrold's 60-something weathered baritone by using a slightly
    slower length_scale.
  - **`length_scale = 1.05`** — gently slower than default. Gerrold is
    not as slow as Doris (she is cozy unhurried; he is hesitant from
    grief), but he isn't fast either.
- **Runtime tuning:** `volume = 0.90`, `pitch = 0.92`
  - Pitch dropped to 0.92 to push Alan's voice into the
    grandfatherly-baritone register Gerrold needs. Volume slightly
    lower than Doris because he speaks more carefully / softer.
- **HuggingFace:** <https://huggingface.co/rhasspy/piper-voices/tree/main/en/en_GB/alan/medium>

### 2.3 Marin (the predecessor — whispered notes only)

- **Bible:** Lore-mentioned in `Mission_1_2_Focus/03_SCENES_LANE_HOLLOW_GARDEN_COTTAGE.md`; her notes are the player's first hook for the Marin Mystery (long-form M6+ thread).
- **Voice signature:** soft, female, "kind in the way that does not announce itself". The player never meets her — they only ever hear her notes via `MarinNoteInteractable`.
- **Piper choice:** `en_US-amy-medium`
  - **License:** Public Domain.
  - **Why:** Amy is a gentle American female voice. With a high
    length_scale (1.20) the notes come out at a slow, breath-taking
    pace that reads as "whisper-read aloud" — a journal entry rather
    than a dialogue line.
  - **`length_scale = 1.20`** — slowest in the cast. Marin's lines are
    not lived dialogue; they are read out as the player picks up the
    note. The slow pace is part of the in-fiction reading rhythm.
- **Runtime tuning:** `volume = 0.75`, `pitch = 1.05`
  - Volume dropped to 0.75 so Marin sounds at-a-distance (in
    memory). Pitch +5% to lift her slightly above the typical Amy
    timbre so she doesn't confuse with Pickle (who shares the model).
- **HuggingFace:** <https://huggingface.co/rhasspy/piper-voices/tree/main/en/en_US/amy/medium>

### 2.4 Narrator (title cards + Dream framing)

- **Use:** the `MissionTitleCard.cs` overlay, plus the `MemoryDreamSequencer` opening / closing prose.
- **Voice signature:** neutral, clear, slight remove (as if reading the player into a chapter). British formality fits the cozy literary register.
- **Piper choice:** `en_GB-jenny_dioco-medium`
  - **License:** CC-BY-4.0.
  - **Why:** Jenny is the cleanest neutral British female voice in
    the Piper roster. Excellent for narration / chapter-card prose.
  - **`length_scale = 1.05`** — slightly measured but not sleepy.
- **Runtime tuning:** `volume = 0.95`, `pitch = 1.00`
- **HuggingFace:** <https://huggingface.co/rhasspy/piper-voices/tree/main/en/en_GB/jenny_dioco/medium>

### 2.5 Pickle (the cat narrator)

- **Bible:** Cross-referenced in Codex 07 § 3.1 rule 7 — her lines are *internal*: only the player hears her. Rendered in `DialogueUI`'s italic / dim-amber / no-portrait mode (see playtest commit 5/6).
- **Voice signature:** bright, sly, ageless — the cat archetype. Mission 2 Guide § 14.2 puts her at "high feminine narration with a chime tag".
- **Piper choice:** `en_US-amy-medium` (same model as Marin, different tuning)
  - **Why:** Amy with `length_scale = 0.95` (faster) and `pitch = 1.10` (brighter) gives a distinctly higher-register, quicker delivery — recognizably a different "character" from Marin's slow, low-volume whisper read on the same base model. Saves the player a ~60 MB extra model download.
  - **`length_scale = 0.95`** — fastest in the cast (quick, sly).
- **Runtime tuning:** `volume = 0.80`, `pitch = 1.10`
- **Suppress mumble:** Phase 32.10 will make `MumbleVoicePlayer`
  suppress its syllable bank for any character with a real
  `VoiceLibrarySO` clip on the current line, so Pickle's italic line
  doesn't stack a real voice on top of procedural mumble.

---

## 3. The 77 lines

Per-character line counts and ids. Each `lineId` is the filename
basename of its `.wav` under `Assets/_Project/Audio/Voice/<Character>/`.

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
| **NEW** — refused-path branch | `doris_m1_refused_01..03` | 3 |
| **NEW** — clarity-branching after-polish (perfect/acceptable/mild) | `doris_m1_polish_after_*` | 3 |
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

When Mission 3+ adds a new villager (e.g. Vance Ashby — the bee
priest who never appears until M6):

1. Add a `VOICES=()` row in `Tools/generate_voices.sh`.
2. Add a `MODELS=()` row in `Tools/download_voice_models.sh` (only if the
   model isn't already downloaded for an existing character).
3. Add the character's per-line entries in the `LINES=()` table at the
   bottom of `generate_voices.sh`.
4. Add a per-character `Audio/Voice/<Name>/.gitkeep` so the folder
   ships even before the first run.
5. Add the row to this document under § 1 and § 2.
6. Optionally, add per-character default `volume` / `pitch` to
   `Phase32_VoiceLibraryBuilder.cs` so new entries pick up the right
   inspector defaults on the first rebuild.

The runtime needs no change: the recursive scan in
`Phase32_VoiceLibraryBuilder` picks up the new folder automatically,
and `VoicePlayer.Play(lineId)` resolves the new ids by string match.

---

## 5. Swapping a voice (per-character)

If you want to swap Doris's Piper voice for an ElevenLabs voice clone
or a booth-recorded actress:

1. Generate the new `.wav` files with the **same lineIds**
   (`doris_m1_greet_01.wav`, etc.). Format: **22 kHz mono PCM16**
   (use ffmpeg `-ar 22050 -ac 1 -acodec pcm_s16le` if the source
   format differs).
2. Drop them into `Assets/_Project/Audio/Voice/Doris/`, overwriting
   the Piper baselines.
3. Open Unity → `Hearthbound → ⚙️ Advanced → 🎙️ Phase 32 — Rebuild
   Voice Library`. The Editor utility re-scans the folder, preserves
   any inspector-tuned `volume` / `pitch` for the lineIds you keep,
   adds entries for new ids, and prunes entries whose .wav was
   deleted.
4. Press Play. Doris now speaks in the new voice. **No code change.**

This is **D-058**, the file-swap policy. The Piper baseline is the
demo track; commercial composer / actor masters replace files by name
and the game ships with the new audio.

---

## 6. License notes

All Piper voice models referenced here are open-source licensed:

| Model | License |
|---|---|
| `en_US-lessac-medium` | Public Domain (Blizzard 2013 corpus) |
| `en_GB-alan-medium` | Public Domain |
| `en_US-amy-medium` | Public Domain |
| `en_GB-jenny_dioco-medium` | CC-BY-4.0 (credit `Jenny Dioco` in any commercial release) |

The Piper inference code (`piper-tts` PyPI / GitHub) is MIT-licensed.

The .onnx model files themselves are **not committed** to git
(see `.gitignore`); developers run `bash Tools/download_voice_models.sh`
once after cloning. The casting decision (this document) and the
generation script are the canonical artifacts; the binaries are a
build dependency.

---

*End of `Docs/VOICE_CASTING.md` v1.0 (Phase 32.6 — open-source pipeline).*
