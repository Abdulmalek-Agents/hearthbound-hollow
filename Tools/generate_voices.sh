#!/usr/bin/env bash
# Tools/generate_voices.sh
# ────────────────────────────────────────────────────────────────────
# Phase 32 — Voice Acting MVP. Open-source Piper TTS pipeline.
# Cross-platform: Linux / macOS / Windows (WSL / Git-Bash / MSYS2).
#
# Quick start:
#     bash Tools/download_voice_models.sh    # one-time (~250 MB)
#     bash Tools/generate_voices.sh           # ~30 s for the full set
#
# Output:
#     Assets/_Project/Audio/Voice/<Character>/<lineId>.wav
#     22 kHz mono PCM16 (Unity-native — no extra import settings).
#
# Idempotent:
#     Every .wav is skipped if it already exists. Delete a clip and
#     re-run to regenerate just that one. Re-running with no deletions
#     is a no-op.
#
# Decisions:
#   • D-058 — voice clips live under Audio/Voice/{character}/{lineId}.wav;
#             pipeline decoupled from runtime (file-swap policy).
#   • D-059 — Piper TTS is the canonical open-source TTS pipeline.
#             Any 22 kHz mono PCM16 .wav still drops in unchanged
#             (ElevenLabs / XTTS / booth-recorded actress, etc.).
#
# Voice casting (see Docs/VOICE_CASTING.md for the canonical table):
#   • Doris    — en_US-lessac-medium   (warm, unhurried; length_scale 1.15)
#   • Gerrold  — en_GB-alan-medium     (weathered British male; 1.05)
#   • Marin    — en_US-amy-medium      (soft predecessor notes;  1.20)
#   • Narrator — en_GB-jenny_dioco-medium (clear neutral female;  1.05)
#   • Pickle   — en_US-amy-medium      (bright sly cat narrator; 0.95)
# ────────────────────────────────────────────────────────────────────

set -euo pipefail

# Resolve repo root from this script's location so callers can invoke
# from anywhere (CI, sub-shells, IDEs).
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
MODELS_DIR="${ROOT}/Tools/voice_models"
OUT_BASE="${ROOT}/Assets/_Project/Audio/Voice"

# Unity-native sample rate. Piper's *-medium models output 22050 Hz by
# default; we resample only if a model ever drifts off that target.
TARGET_SR=22050

# ──────────────────────────────────────────────────────────────────
# Pre-flight 1 — `piper` binary
# ──────────────────────────────────────────────────────────────────
if ! command -v piper >/dev/null 2>&1; then
  cat >&2 <<'EOF'
[error] Piper TTS not found on PATH.

Install (cross-platform):

  • pip   (recommended — pure Python wheel works on Linux/macOS/Windows):
        pip install piper-tts

  • Pre-built binary (no Python):
        https://github.com/rhasspy/piper/releases
        Download the right archive for your OS, extract, then either
        symlink the `piper` binary onto PATH or `export PATH=$PWD:$PATH`.

After install:
        bash Tools/download_voice_models.sh
        bash Tools/generate_voices.sh

Reference docs:  https://github.com/rhasspy/piper
EOF
  exit 1
fi

# ──────────────────────────────────────────────────────────────────
# Pre-flight 2 — voice-model directory
# ──────────────────────────────────────────────────────────────────
if [[ ! -d "$MODELS_DIR" ]] || ! ls "$MODELS_DIR"/*.onnx >/dev/null 2>&1; then
  cat >&2 <<EOF
[error] No Piper voice models found in:
        $MODELS_DIR

Run:    bash Tools/download_voice_models.sh
        bash Tools/generate_voices.sh

EOF
  exit 1
fi

# Optional helpers — ffprobe + ffmpeg used to verify / fix the sample
# rate. Not required (Piper *-medium = 22 kHz by default).
HAS_FFMPEG=0
HAS_FFPROBE=0
command -v ffmpeg  >/dev/null 2>&1 && HAS_FFMPEG=1
command -v ffprobe >/dev/null 2>&1 && HAS_FFPROBE=1

# ──────────────────────────────────────────────────────────────────
# Voice casting table
#   COL 1 = Character (folder under Audio/Voice/)
#   COL 2 = Piper model basename (.onnx + .onnx.json under voice_models/)
#   COL 3 = length_scale (>1 = slower, <1 = faster — Piper's only rate knob)
# ──────────────────────────────────────────────────────────────────
declare -a VOICES=(
  "Doris|en_US-lessac-medium|1.15"
  "Gerrold|en_GB-alan-medium|1.05"
  "Marin|en_US-amy-medium|1.20"
  "Narrator|en_GB-jenny_dioco-medium|1.05"
  "Pickle|en_US-amy-medium|0.95"
)

get_voice_field() {
  # $1 = character; $2 = 1-based field index → echoes that field of VOICES[]
  local char="$1" idx="$2" v c
  for v in "${VOICES[@]}"; do
    c="${v%%|*}"
    if [[ "$c" == "$char" ]]; then
      echo "$v" | cut -d'|' -f"$idx"
      return 0
    fi
  done
  return 1
}

# ──────────────────────────────────────────────────────────────────
# Canonical dialogue lines.
#   COL 1 = Character (matches a VOICES row)
#   COL 2 = lineId    (filename basename — must match Mission01Director.cs)
#   COL 3 = text      (the exact line spoken)
#
# The Doris block (55 lines) is the canonical source of truth: every
# lineId here is referenced verbatim from Mission01Director.cs's
# `Line(... , "doris_m1_*")` calls. Other characters' lines are
# stubs sized for Mission 2 + future content; they will be wired
# into Mission02Director / cutscenes in later phases.
# ──────────────────────────────────────────────────────────────────
declare -a LINES=(
  # ── DORIS · Mission 1 · greeting ─────────────────────────────────
  "Doris|doris_m1_greet_01|You're the new one."
  "Doris|doris_m1_greet_02|I thought you'd be taller."
  "Doris|doris_m1_greet_03|Don't mind me — I thought that about the old one, too."
  "Doris|doris_m1_greet_04|Come in. The kettle's only just stopped."

  # ── DORIS · Mission 1 · 3-option opener replies ──────────────────
  "Doris|doris_m1_reply_help_01|Aye. The very same."
  "Doris|doris_m1_reply_help_02|They've put my name on the sign and everything. Look — there."
  "Doris|doris_m1_reply_silent_01|A quiet one, then. Good."
  "Doris|doris_m1_reply_silent_02|The bread likes quiet."
  "Doris|doris_m1_reply_unsure_01|... Mm."
  "Doris|doris_m1_reply_unsure_02|That's a conversation for a longer day."
  "Doris|doris_m1_reply_unsure_03|Come in. Tea first."

  # ── DORIS · Mission 1 · bakery entrance ──────────────────────────
  "Doris|doris_m1_kitchen_01|Mind the flour."
  "Doris|doris_m1_kitchen_02|I haven't swept since Tuesday. I keep meaning to."
  "Doris|doris_m1_kitchen_03|... The shop next door is yours. The Hollow."
  "Doris|doris_m1_kitchen_04|I've been keeping the key safe for you."

  # ── DORIS · Mission 1 · 'first customer' preamble ────────────────
  "Doris|doris_m1_offer_01|... I have something for you. Before you go in."
  "Doris|doris_m1_offer_02|I'd like to be your first customer, if that's all right."

  # ── DORIS · Mission 1 · the iconic memory offer ──────────────────
  "Doris|doris_m1_memory_01|This is the memory."
  "Doris|doris_m1_memory_02|Hold it like you'd hold a hot bun. Not by the side. Underneath."
  "Doris|doris_m1_memory_03|It's a small thing."
  "Doris|doris_m1_memory_04|First time I made bread that didn't shame me."
  "Doris|doris_m1_memory_05|Most days I think of it."
  "Doris|doris_m1_memory_06|I want to put it down, now, for a while."
  "Doris|doris_m1_memory_07|Will you take it?"

  # ── DORIS · Mission 1 · defer/refuse path ────────────────────────
  "Doris|doris_m1_defer_01|Aye. Some days are not the day."
  "Doris|doris_m1_defer_02|I'll be here when one is."

  # ── DORIS · Mission 1 · 'first loaves' aside (age 24) ────────────
  "Doris|doris_m1_story_01|I was twenty-four."
  "Doris|doris_m1_story_02|The oven was new. The bricks were new. I was new."
  "Doris|doris_m1_story_03|I'd been baking other people's bread for nine years."
  "Doris|doris_m1_story_04|That morning was the first morning that was just mine."
  "Doris|doris_m1_story_05|I want to take a rest from carrying it. That's all."

  # ── DORIS · Mission 1 · price preamble + 3 branches ──────────────
  "Doris|doris_m1_price_01|Four coppers, if you're asking."
  "Doris|doris_m1_price_02|It's a small memory. I'll not have you overpay your first day."
  "Doris|doris_m1_price_fair|Aye. Thank you."
  "Doris|doris_m1_price_high_01|That's too much. I'll not have you ruin yourself."
  "Doris|doris_m1_price_high_02|Take it back. — Well. Take some back."
  "Doris|doris_m1_price_high_03|Five, then. Final."
  "Doris|doris_m1_price_low_01|..."
  "Doris|doris_m1_price_low_02|Aye, that'll do. Bring the rest when you find some."

  # ── DORIS · Mission 1 · handover ('the cat watched me') ──────────
  "Doris|doris_m1_handover_01|There."
  "Doris|doris_m1_handover_02|The old keeper showed me how to make it. Took me four tries."
  "Doris|doris_m1_handover_03|I cracked the first three. The cat watched me. Judged me, I think."
  "Doris|doris_m1_handover_04|I'll be in the bakery if you want me. Knock twice."
  "Doris|doris_m1_handover_05|There's a kettle on the workbench. Mind the wood stove — it bites."

  # ── DORIS · Mission 1 · polish watch + after + sleep ─────────────
  "Doris|doris_m1_polish_watch|I'll wait. Take your time, Keeper."
  "Doris|doris_m1_polish_done_01|Aye."
  "Doris|doris_m1_polish_done_02|There it is. That's the morning."
  "Doris|doris_m1_polish_sleep_01|Sleep tonight. Dreams come."
  "Doris|doris_m1_polish_sleep_02|I'll see you again, eventually."

  # ── DORIS · Mission 1 · NEW: refused-path branch (3) ─────────────
  "Doris|doris_m1_refused_01|The shop's still yours."
  "Doris|doris_m1_refused_02|Go in. Sit a while. The kettle is on."
  "Doris|doris_m1_refused_03|I'll be here when you're ready."

  # ── DORIS · Mission 1 · NEW: clarity-branching after-polish (3) ──
  "Doris|doris_m1_polish_after_perfect|You did it cleaner than I remembered it. I think you'll do."
  "Doris|doris_m1_polish_after_acceptable|You did it kindly. That's what matters."
  "Doris|doris_m1_polish_after_mild|... It's the morning still. A little dimmer. But mine. First days are like that. I won't hold it."

  # ── GERROLD · Mission 2 stub (Depth Bible § 2.2 sample voice) ────
  "Gerrold|gerrold_m2_greet_01|I'm sorry. I don't know how this is supposed to go."
  "Gerrold|gerrold_m2_greet_02|I have the — the thing — I have it in this cloth."
  "Gerrold|gerrold_m2_greet_03|Margery wrapped it. I think she wrapped it for this."
  "Gerrold|gerrold_m2_long_bit_01|I want to keep my wife. I do not want to keep the long bit."
  "Gerrold|gerrold_m2_long_bit_02|It's not the dying part. It's the long bit."
  "Gerrold|gerrold_m2_thank_01|Thank you. I do not know whether you have done what I asked."
  "Gerrold|gerrold_m2_thank_02|I think you have done what you could."
  "Gerrold|gerrold_m2_thank_03|I will go home and see what the morning brings."

  # ── MARIN · predecessor notes (whispered) ────────────────────────
  "Marin|marin_note_lane_01|If you find this, the kettle still works."
  "Marin|marin_note_lane_02|Don't trust the third shelf. It tilts."
  "Marin|marin_note_hollow_01|Pickle remembers everyone. Pickle is fair."
  "Marin|marin_note_workbench_01|The cloth is for handling the warm orbs. Mine is in the drawer."

  # ── NARRATOR · title cards / Memory-Dream framing ────────────────
  "Narrator|narrator_title_day1|Day One. The Hollow."
  "Narrator|narrator_title_day2|Day Two. The Garden."
  "Narrator|narrator_title_evening|Evening falls. The kettle is warm."
  "Narrator|narrator_title_dream|She closes her eyes. The memory begins."

  # ── PICKLE · cat-narrator italic asides (M1 + M2 hooks) ──────────
  "Pickle|pickle_m1_aside_01|Mmm. New one."
  "Pickle|pickle_m1_aside_02|She watches you. She always watches."
  "Pickle|pickle_m1_aside_03|The bread likes you. So does she, I think."
  "Pickle|pickle_m2_aside_01|He brings a cloth. He never used to bring a cloth."
  "Pickle|pickle_m2_aside_02|Choose softly. I am watching."
  "Pickle|pickle_m2_aside_03|You did kindly. I will remember."
)

# ──────────────────────────────────────────────────────────────────
# Helpers
# ──────────────────────────────────────────────────────────────────

# Verify a wav is at TARGET_SR; if ffmpeg is available, resample in place.
ensure_sample_rate() {
  local wav="$1"
  [[ "$HAS_FFPROBE" -eq 1 ]] || return 0
  local sr
  sr=$(ffprobe -v error -select_streams a:0 \
         -show_entries stream=sample_rate \
         -of default=nokey=1:noprint_wrappers=1 "$wav" 2>/dev/null || true)
  if [[ -z "$sr" || "$sr" == "$TARGET_SR" ]]; then
    return 0
  fi
  if [[ "$HAS_FFMPEG" -ne 1 ]]; then
    echo "[warn] $(basename "$wav") is ${sr} Hz (target ${TARGET_SR}); install ffmpeg to auto-resample" >&2
    return 0
  fi
  local tmp="${wav}.tmp.wav"
  ffmpeg -y -hide_banner -loglevel error -i "$wav" \
         -ar "$TARGET_SR" -ac 1 -acodec pcm_s16le "$tmp"
  mv "$tmp" "$wav"
}

# ──────────────────────────────────────────────────────────────────
# CLI flags
#
#   --force          Regenerate every clip, even if the .wav already
#                    exists. Use after changing clean_for_tts() rules
#                    or after editing any line text.
#
#   --only=<glob>    Regenerate only clips whose line_id matches the
#                    glob (e.g. --only=doris_m1_offer_*). Useful for
#                    targeted re-roll after the script's been tuned.
# ──────────────────────────────────────────────────────────────────
FORCE=0
ONLY_GLOB="*"
for arg in "$@"; do
  case "$arg" in
    --force) FORCE=1 ;;
    --only=*) ONLY_GLOB="${arg#--only=}" ;;
    *) echo "[warn] unknown arg: $arg" >&2 ;;
  esac
done

# ──────────────────────────────────────────────────────────────────
# Phase 32.13 — TTS text sanitiser
#
# User report: Piper was speaking the punctuation literally — Doris's
# "... I have something for you. Before you go in." came out as a
# verbalised "dot-dot-dot" pause at the head of the clip, breaking
# the cozy human-actor illusion and giving away that it's TTS.
#
# clean_for_tts() runs every line through these rules BEFORE handing
# the string to Piper:
#
#   1. Strip leading punctuation runs.   "... Mm."        → "Mm."
#                                        ". . . Aye."     → "Aye."
#                                        "— Aye."         → "Aye."
#                                        ", well."        → "well."
#   2. Strip trailing junk (whitespace, ellipses) — Piper's
#      `--sentence_silence` already gives a natural breath.
#   3. Replace internal ellipses ".." or "..." with ", " so Piper
#      pauses naturally (the comma triggers a prosody pause) instead
#      of pronouncing them. "... Mm." → "Mm."   "I — , the thing"
#   4. Replace em-dash "—" with ", " for the same prosody-pause
#      effect (Piper otherwise reads "em dash" or skips uneasily).
#   5. Strip Markdown emphasis "*word*" or "**word**" → "word".
#   6. Collapse runs of whitespace to a single space.
#   7. If the line ends up empty (was just punctuation like "..."),
#      emit a 0.4 s silent stub via the special "[[silent]]" sentinel
#      that the main loop handles by writing an empty 22 kHz PCM16
#      file directly instead of calling Piper. Keeps the lineId →
#      AudioClip mapping intact so DialogueUI's pacing still works.
# ──────────────────────────────────────────────────────────────────
clean_for_tts() {
  local s="$1"
  # 5. Strip Markdown emphasis first (asterisks at the head would
  #    otherwise be treated as leading punctuation and the wrapped
  #    word would be lost).
  s=$(printf '%s' "$s" | sed -E 's/\*+([^*]+)\*+/\1/g')

  # Phase 32.21 — strip parenthetical stage directions like
  # "(stands back and watches)". Piper otherwise voices the brackets
  # AND the contents, producing a robotic narrated stage direction.
  s=$(printf '%s' "$s" | sed -E 's/\([^)]*\)//g')

  # 3. Replace internal multi-dot ellipses with a comma. Phase 32.21
  #    also strips the surrounding whitespace so we don't end up
  #    with "X , Y" (extra space before the comma).
  s=$(printf '%s' "$s" | sed -E 's/[[:space:]]*\.{2,}[[:space:]]*/, /g')

  # 4. Replace em-dash + en-dash with a comma. Phase 32.21 — strip
  #    the surrounding whitespace too, so "me — I" becomes "me, I"
  #    (natural prosody) instead of "me ,  I" (robotic).
  s=$(printf '%s' "$s" | sed -E 's/[[:space:]]*—[[:space:]]*/, /g; s/[[:space:]]*–[[:space:]]*/, /g')

  # Phase 32.21 — collide-cleanup. After substitutions we can get:
  #   ".," from "X. — Y" → "X., Y"     → collapse to ".  " (drop comma)
  #   ",," from "X, — Y" → "X,, Y"     → collapse to ", "
  #   "?," from "X? — Y" → "X?, Y"     → collapse to "? " (drop comma)
  #   "!," from "X! — Y" → "X!, Y"     → collapse to "! " (drop comma)
  s=$(printf '%s' "$s" | sed -E 's/([.!?]),/\1/g; s/,+/,/g')

  # 1+2. Trim leading and trailing punctuation + whitespace.
  s=$(printf '%s' "$s" | sed -E 's/^[[:space:],.;:!?\-]+//; s/[[:space:],;:\-]+$//')

  # 6. Collapse runs of whitespace to a single space.
  s=$(printf '%s' "$s" | tr -s '[:space:]' ' ')

  # 7. If everything got stripped, return the silent-stub sentinel.
  if [[ -z "${s// }" ]]; then
    printf '[[silent]]'
  else
    printf '%s' "$s"
  fi
}

# ──────────────────────────────────────────────────────────────────
# Phase 32.21 — "is this line dirty?" detector.
#
# Returns 0 (true) if the raw source line carries any pattern the
# cleaner rewrites (leading punctuation runs, ellipses, em-dash,
# Markdown asterisks, parenthetical stage directions). Used by the
# main loop to PURGE any pre-existing .wav for a dirty line on every
# script run so a stale clip from before the cleaner existed cannot
# survive.
# ──────────────────────────────────────────────────────────────────
is_dirty_source() {
  local s="$1"
  # Leading run of punctuation / whitespace.
  if [[ "$s" =~ ^[[:space:],.\;:!?\-]+ ]]; then return 0; fi
  # Internal ellipsis (2+ dots).
  if [[ "$s" =~ \.{2,} ]]; then return 0; fi
  # Em-dash / en-dash.
  case "$s" in
    *—*|*–*) return 0 ;;
  esac
  # Markdown emphasis.
  if [[ "$s" =~ \*[^*]+\* ]]; then return 0; fi
  # Parenthetical stage direction.
  if [[ "$s" =~ \([^\)]*\) ]]; then return 0; fi
  return 1
}

# ──────────────────────────────────────────────────────────────────
# Silent-stub helper — writes a 0.4 s silent 22 kHz mono PCM16 WAV.
# Used for lines that are pure punctuation (e.g. "..." or "...").
# Requires `ffmpeg` (everywhere) or falls back to Piper with a real
# space character (which makes Piper emit silence).
# ──────────────────────────────────────────────────────────────────
write_silent_wav() {
  local out="$1"
  local dur="${2:-0.4}"
  if command -v ffmpeg >/dev/null 2>&1; then
    ffmpeg -y -f lavfi -i "anullsrc=r=${TARGET_SR}:cl=mono" -t "$dur" \
           -acodec pcm_s16le "$out" </dev/null >/dev/null 2>&1
  else
    # Fallback: ask Piper to speak a single space — comes out as silence.
    printf ' \n' | piper --model "$2" --config "$3" --output_file "$out" 2>/dev/null
  fi
}

# ──────────────────────────────────────────────────────────────────
# Main loop
# ──────────────────────────────────────────────────────────────────
mkdir -p "$OUT_BASE"

declare -A gen_count skip_count missing_model

total_lines=${#LINES[@]}
i=0

for entry in "${LINES[@]}"; do
  i=$((i + 1))
  character="${entry%%|*}"
  rest="${entry#*|}"
  line_id="${rest%%|*}"
  text="${rest#*|}"

  # Phase 32.13 — honour --only glob (default "*" matches every id).
  # Uses bash's built-in [[ ... == glob ]] match.
  if [[ "$line_id" != $ONLY_GLOB ]]; then
    continue
  fi

  if ! model_name=$(get_voice_field "$character" 2); then
    echo "[skip] $line_id — no voice configured for '$character'"
    continue
  fi
  length_scale=$(get_voice_field "$character" 3)
  model_file="${MODELS_DIR}/${model_name}.onnx"
  config_file="${MODELS_DIR}/${model_name}.onnx.json"

  if [[ ! -f "$model_file" ]] || [[ ! -f "$config_file" ]]; then
    if [[ -z "${missing_model[$model_name]:-}" ]]; then
      missing_model[$model_name]=1
      echo "[error] missing model files for '$model_name'" >&2
      echo "        expected: $model_file" >&2
      echo "        + config: $config_file" >&2
      echo "        run:      bash Tools/download_voice_models.sh" >&2
    fi
    continue
  fi

  out_dir="${OUT_BASE}/${character}"
  mkdir -p "$out_dir"
  out_wav="${out_dir}/${line_id}.wav"

  # Phase 32.13 — sanitise the text BEFORE Piper sees it. Strips leading
  # ellipses, replaces internal "..." / em-dash with comma prosody-pauses,
  # strips Markdown emphasis. Without this the TTS verbalised the
  # punctuation and the lines sounded robotic.
  spoken=$(clean_for_tts "$text")

  # Phase 32.13 — auto-detect stale clips. If cleaning CHANGES the text,
  # then any pre-existing wav was generated from the raw (buggy) text and
  # MUST be regenerated. The text-comparison heuristic catches most cases.
  needs_regen=0
  if [[ "$spoken" != "$text" ]]; then needs_regen=1; fi

  # Phase 32.21 — belt-and-braces purge. Even if `clean_for_tts` and the
  # cleaner output happen to coincidentally match the cached clip's text
  # (e.g. a previous script run on a slightly different cleaner version
  # produced the same string), we DELETE any existing wav whose source
  # line is "dirty" — carries punctuation the cleaner addresses — so the
  # clip is guaranteed-fresh on every run. Defensive against the regression
  # the user reported: stale clips from before the cleaner existed that
  # survive auto-detect because of string-equality edge cases.
  if [[ -f "$out_wav" ]] && is_dirty_source "$text"; then
    rm -f "$out_wav"
    needs_regen=1
  fi

  # Skip only if the file exists AND --force wasn't set AND the cleaning
  # didn't change anything (so the cached clip is still correct).
  if [[ -f "$out_wav" && $FORCE -eq 0 && $needs_regen -eq 0 ]]; then
    printf "[%3d/%3d] skip  %s/%s\n" "$i" "$total_lines" "$character" "$line_id"
    skip_count[$character]=$(( ${skip_count[$character]:-0} + 1 ))
    continue
  fi

  printf "[%3d/%3d] gen   %s/%s\n" "$i" "$total_lines" "$character" "$line_id"

  if [[ "$spoken" == "[[silent]]" ]]; then
    # Line was pure punctuation (e.g. "..."). Write a brief silent stub
    # so the lineId → AudioClip mapping survives and DialogueUI's pacing
    # still works on it. Default 0.4 s.
    write_silent_wav "$out_wav" 0.4
  else
    # Piper reads text from stdin; --output_file writes a WAV directly.
    # --sentence_silence 0.12 gives a small breath at the end so the clip
    # has a graceful tail (no clipping artefacts).
    printf '%s\n' "$spoken" | piper \
        --model "$model_file" \
        --config "$config_file" \
        --output_file "$out_wav" \
        --length_scale "$length_scale" \
        --sentence_silence 0.12 \
        2>/dev/null
  fi

  ensure_sample_rate "$out_wav"
  gen_count[$character]=$(( ${gen_count[$character]:-0} + 1 ))
done

# ──────────────────────────────────────────────────────────────────
# Summary
# ──────────────────────────────────────────────────────────────────
echo
echo "──── Summary ──────────────────────────────────────────"
total_g=0; total_s=0
for v in "${VOICES[@]}"; do
  c="${v%%|*}"
  g="${gen_count[$c]:-0}"
  s="${skip_count[$c]:-0}"
  total_g=$(( total_g + g ))
  total_s=$(( total_s + s ))
  printf "  %-10s generated=%3d  skipped=%3d  total=%3d\n" "$c" "$g" "$s" "$(( g + s ))"
done
printf "  %-10s generated=%3d  skipped=%3d  total=%3d\n" "ALL" "$total_g" "$total_s" "$(( total_g + total_s ))"
echo
echo "Next:  open Unity → Hearthbound → ⚙️ Advanced → 🎙️ Phase 32 — Rebuild Voice Library"
echo
