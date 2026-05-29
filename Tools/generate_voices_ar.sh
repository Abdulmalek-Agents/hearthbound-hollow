#!/usr/bin/env bash
# Tools/generate_voices_ar.sh
# ────────────────────────────────────────────────────────────────────
# Phase 60 — Arabic Localization MVP. Open-source Piper TTS pipeline
# for the ARABIC voice cast.
#
# Cross-platform: Linux / macOS / Windows (WSL / Git-Bash / MSYS2).
#
# Quick start:
#     bash Tools/download_voice_models.sh    # one-time (~250 MB + 60 MB Arabic)
#     bash Tools/generate_voices_ar.sh        # ~25 s for the full set
#
# Output:
#     Assets/_Project/Audio/Voice/ar/<Character>/<lineId>.wav
#     22 kHz mono PCM16 (Unity-native — no extra import settings).
#
# The English script (Tools/generate_voices.sh) writes to
#     Assets/_Project/Audio/Voice/<Character>/<lineId>.wav
# and stays unchanged. Phase60_VoiceLibraryArabicBinder.cs scans the
# `/ar/` subtree at editor time and binds each .wav into the matching
# VoiceLibrarySO Entry's `clipAr` slot. At runtime, VoicePlayer.Play(...)
# checks the active locale and picks the Arabic clip when Locale.Arabic
# is active + clipAr is set; otherwise falls back to the English clip.
# Subtitles are ALWAYS translated by DialogueUI — only the audio
# degrades to English if no Arabic clip is recorded.
#
# Voice casting (single Arabic Piper model — ar_JL-medium):
#   • All characters share the model; we differentiate via length_scale
#     + pitch in the runtime VoiceLibrarySO entries. This is the cleanest
#     path while we wait for an Arabic male voice + a second female
#     voice upstream. A future commercial composer drop replaces the
#     Piper baseline by file overwrite — D-058's policy stands.
#
# Decisions:
#   • D-060 — Arabic Localization MVP. Arabic voice lives under
#             Assets/_Project/Audio/Voice/ar/. English stays at the
#             root (legacy path) so the existing pipeline is unchanged.
# ────────────────────────────────────────────────────────────────────

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
MODELS_DIR="${ROOT}/Tools/voice_models"
OUT_BASE="${ROOT}/Assets/_Project/Audio/Voice/ar"

TARGET_SR=22050

# ──────────────────────────────────────────────────────────────────
# Pre-flight — `piper` binary
# ──────────────────────────────────────────────────────────────────
if ! command -v piper >/dev/null 2>&1; then
  cat >&2 <<'EOF'
[error] Piper TTS not found on PATH.

Install (cross-platform):

  • pip   (recommended):
        pip install piper-tts

  • Pre-built binary (no Python):
        https://github.com/rhasspy/piper/releases

After install:
        bash Tools/download_voice_models.sh
        bash Tools/generate_voices_ar.sh
EOF
  exit 1
fi

# Pre-flight — Arabic model present
AR_MODEL="ar_JL-medium"
if [[ ! -f "$MODELS_DIR/$AR_MODEL.onnx" ]] || [[ ! -f "$MODELS_DIR/$AR_MODEL.onnx.json" ]]; then
  cat >&2 <<EOF
[error] Missing Arabic Piper model: $AR_MODEL

Run:    bash Tools/download_voice_models.sh
        bash Tools/generate_voices_ar.sh

EOF
  exit 1
fi

HAS_FFMPEG=0
HAS_FFPROBE=0
command -v ffmpeg  >/dev/null 2>&1 && HAS_FFMPEG=1
command -v ffprobe >/dev/null 2>&1 && HAS_FFPROBE=1

# ──────────────────────────────────────────────────────────────────
# Voice casting — Arabic
#   COL 1 = Character
#   COL 2 = Piper model basename (single ar_JL for now)
#   COL 3 = length_scale (Piper rate knob)
# ──────────────────────────────────────────────────────────────────
declare -a VOICES=(
  "Doris|ar_JL-medium|1.15"
  "Gerrold|ar_JL-medium|1.10"
  "Marin|ar_JL-medium|1.22"
  "Narrator|ar_JL-medium|1.05"
  "Pickle|ar_JL-medium|0.95"
)

get_voice_field() {
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
# Canonical Arabic dialogue lines — matched 1:1 with the English
# lineIds in Tools/generate_voices.sh + Mission01/02Director.cs.
#
# The text is the literary-Arabic translation produced by the
# Linguistic Team and verified against the Depth Bible voice
# signatures (Vellis Rules for Doris, Cordray Principle for Gerrold,
# etc.). See dialogue.ar.json for the runtime subtitle source.
# ──────────────────────────────────────────────────────────────────
declare -a LINES=(
  # ── DORIS · M1 · greeting
  "Doris|doris_m1_greet_01|أنتَ الجديد."
  "Doris|doris_m1_greet_02|كنتُ أحسَبُكَ أَطوَل."
  "Doris|doris_m1_greet_03|لا تُؤاخِذني — كنتُ أحسَبُ الأمرَ نفسَه عن القديم."
  "Doris|doris_m1_greet_04|ادخُل. الإبريقُ تَوًّا تَوَقَّفَ عن الغَلَيان."

  # ── DORIS · M1 · 3-option opener replies
  "Doris|doris_m1_reply_help_01|أَجَل. الواحِدةُ ذاتُها."
  "Doris|doris_m1_reply_help_02|وَضَعوا اسمي على اللافِتَة وكلَّ شيء. اُنظر — هناك."
  "Doris|doris_m1_reply_silent_01|هادِئٌ إذًا. جيّد."
  "Doris|doris_m1_reply_silent_02|الخُبزُ يُحِبُّ الهُدوء."
  "Doris|doris_m1_reply_unsure_01|مم."
  "Doris|doris_m1_reply_unsure_02|تلكَ مُحادَثَةُ يومٍ أَطوَل."
  "Doris|doris_m1_reply_unsure_03|ادخُل. الشايُ أوّلًا."

  # ── DORIS · M1 · bakery entrance
  "Doris|doris_m1_kitchen_01|احذَرِ الطَّحين."
  "Doris|doris_m1_kitchen_02|لم أَكنُس منذُ الثلاثاء. أَنوي ذلكَ دائمًا."
  "Doris|doris_m1_kitchen_03|المَتجَرُ المُجاوِرُ لكَ. الجَوْف."
  "Doris|doris_m1_kitchen_04|كنتُ أَحفَظُ مفتاحَه لكَ."

  # ── DORIS · M1 · 'first customer' preamble
  "Doris|doris_m1_offer_01|لديَّ شيءٌ لك. قَبلَ أن تَدخُل."
  "Doris|doris_m1_offer_02|أَوَدُّ أن أكونَ أوّلَ زبائنِك، إن سَمَحت."

  # ── DORIS · M1 · the iconic memory offer
  "Doris|doris_m1_memory_01|هذهِ هي الذِّكرى."
  "Doris|doris_m1_memory_02|أَمسِكها كأنَّكَ تُمسِكُ رغيفًا ساخنًا. ليس من الجانب. مِن أَسفَل."
  "Doris|doris_m1_memory_03|إنّها شَيءٌ صغير."
  "Doris|doris_m1_memory_04|أوّلُ مرَّةٍ خَبَزتُ خبزًا لم يُخجِلني."
  "Doris|doris_m1_memory_05|في أكثرِ أيّامي أُفَكِّرُ فيها."
  "Doris|doris_m1_memory_06|أَوَدُّ أن أَضَعَها جانبًا، بُرهَةً."
  "Doris|doris_m1_memory_07|أَتَأخُذُها؟"

  # ── DORIS · M1 · defer/refuse path
  "Doris|doris_m1_defer_01|أَجَل. ليسَ كُلُّ يومٍ هو ذلكَ اليوم."
  "Doris|doris_m1_defer_02|سأكونُ هنا حين يَحينُ يومُك."

  # ── DORIS · M1 · 'first loaves' aside
  "Doris|doris_m1_story_01|كنتُ في الرابعةِ والعشرين."
  "Doris|doris_m1_story_02|كان الفُرنُ جديدًا. كان الطُّوبُ جديدًا. كنتُ جديدةً."
  "Doris|doris_m1_story_03|كنتُ أَخبزُ خُبزَ الآخرينَ منذُ تِسعِ سنوات."
  "Doris|doris_m1_story_04|ذاكَ الصَّباحُ كان أوّلَ صَباحٍ يَخُصُّني وحدي."
  "Doris|doris_m1_story_05|أَوَدُّ أن أَستريحَ من حَملِها. لا غَير."

  # ── DORIS · M1 · price negotiation
  "Doris|doris_m1_price_01|أربعةُ نُحاسات، إن سألتَ."
  "Doris|doris_m1_price_02|ذِكرى صغيرة. لن أَدعَكَ تَدفَعُ أكثرَ مِمّا تَستَحِق في أوّلِ يومٍ لك."
  "Doris|doris_m1_price_fair|أَجَل. أَشكُرُك."
  "Doris|doris_m1_price_high_01|هذا أكثرُ مِمّا يَنبَغي. لن أَدَعَكَ تُورِّطُ نفسَك."
  "Doris|doris_m1_price_high_02|خُذها. حَسَنًا. خُذ بَعضَها."
  "Doris|doris_m1_price_high_03|خَمسةٌ إذًا. نِهائيّ."
  "Doris|doris_m1_price_low_02|أَجَل، يَفي بالغَرَض. أَحضِرِ البَقيَّةَ حِينَ تَجِدها."

  # ── DORIS · M1 · handover
  "Doris|doris_m1_handover_01|هُنا."
  "Doris|doris_m1_handover_02|أَرَتني الصاحبةُ القديمةُ كيفَ أَصنَعُها. كَلَّفَتني أَربعَ مُحاوَلات."
  "Doris|doris_m1_handover_03|كَسَرتُ الثَّلاثَ الأُوَل. القطّةُ راقَبَتني. حَكَمَت عليَّ، أَظُنّ."
  "Doris|doris_m1_handover_04|سأكونُ في المَخبَزِ إن أرَدتَني. اطرُق مَرَّتَين."
  "Doris|doris_m1_handover_05|ثَمَّةَ إبريقٌ على الطاولة. احذَرِ الفُرنَ الخَشَبيّ — يَعَضّ."

  # ── DORIS · M1 · polish + sleep
  "Doris|doris_m1_polish_watch|سأَنتَظِر. خُذ وقتَك، أيُّها الكَيِّل."
  "Doris|doris_m1_polish_done_01|أَجَل."
  "Doris|doris_m1_polish_done_02|ها هي. هذا هو الصَّباح."
  "Doris|doris_m1_polish_sleep_01|نَمِ اللَّيلَة. الأَحلامُ تأتي."
  "Doris|doris_m1_polish_sleep_02|سَأَراكَ ثانيةً، يَومًا ما."

  # ── DORIS · M1 · refused-path
  "Doris|doris_m1_refused_01|المَتجَرُ ما زالَ لَك."
  "Doris|doris_m1_refused_02|ادخُل. اِجلِس بُرهَة. الإبريقُ يَغلي."
  "Doris|doris_m1_refused_03|سأكونُ هنا حِينَ تَكونُ جاهزًا."

  # ── DORIS · M1 · after-polish branches
  "Doris|doris_m1_polish_after_perfect|صَنَعتَها أَنقى مِمّا أَذكُرُها. أَظُنُّكَ سَتَنفَع."
  "Doris|doris_m1_polish_after_acceptable|صَنَعتَها برِفق. ذاكَ ما يَهُمّ."
  "Doris|doris_m1_polish_after_mild|ما زال الصَّباحُ صَباحًا. أَخفَتَ قليلًا. ولكنّه لي. أوّلُ الأيّامِ هكذا. لن أُؤاخِذَك."

  # ── GERROLD · M2
  "Gerrold|gerrold_m2_greet_01|أَعتَذِر. لا أَعرِفُ كيفَ يَجري هذا الأمر."
  "Gerrold|gerrold_m2_greet_02|مَعي ذاكَ الشَّيء — في هذا القُماش."
  "Gerrold|gerrold_m2_greet_03|مارجَري لَفَّته. أَظُنُّها لَفَّته لِهذا."
  "Gerrold|gerrold_m2_long_bit_01|أُريدُ أن أُبقيَ زَوجَتي. لا أُريدُ أن أُبقيَ الجزءَ الطويل."
  "Gerrold|gerrold_m2_long_bit_02|ليس جزءَ الاحتضار. بل الجزءَ الطويل."
  "Gerrold|gerrold_m2_thank_01|شُكرًا. لا أَدري إن كنتَ فَعَلتَ ما طَلَبت."
  "Gerrold|gerrold_m2_thank_02|أَظُنُّكَ فَعَلتَ ما تَستطِيع."
  "Gerrold|gerrold_m2_thank_03|سأَعودُ إلى البيتِ وأَنتَظِرُ ما يأتي به الصَّباح."

  # ── MARIN · whispered notes
  "Marin|marin_note_lane_01|إن وَجَدتَ هذا، فالإبريقُ ما زالَ يَعمَل."
  "Marin|marin_note_lane_02|لا تأمَنِ الرَّفَّ الثالث. إنّه مائل."
  "Marin|marin_note_hollow_01|بيكل تَتَذَكَّرُ الجَميع. بيكل عادلة."
  "Marin|marin_note_workbench_01|القُماشُ لحَملِ الكُرَاتِ الدافئة. وقُماشَتي في الدُّرج."

  # ── NARRATOR · title cards
  "Narrator|narrator_title_day1|اليومُ الأوّل. الجَوْف."
  "Narrator|narrator_title_day2|اليومُ الثاني. الحديقة."
  "Narrator|narrator_title_evening|يَحلُّ المَساء. الإبريقُ دافِئ."
  "Narrator|narrator_title_dream|تُغمِضُ عينيها. تبدأُ الذِّكرى."

  # ── PICKLE · italic asides
  "Pickle|pickle_m1_aside_01|مم. جديدٌ هذا."
  "Pickle|pickle_m1_aside_02|تُراقِبُك. هي دائمًا تُراقِب."
  "Pickle|pickle_m1_aside_03|الخُبزُ يُحِبُّك. وهي كذلك، أَظُنّ."
  "Pickle|pickle_m2_aside_01|أَتى بقُماش. لم يَكُن يَأتي بقُماشٍ من قَبل."
  "Pickle|pickle_m2_aside_02|اختَر برِفق. أنا أُراقِب."
  "Pickle|pickle_m2_aside_03|فَعَلتَ بِلُطف. سأَتَذَكَّر."
)

# ──────────────────────────────────────────────────────────────────
# Helpers (parallel to generate_voices.sh)
# ──────────────────────────────────────────────────────────────────

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
    echo "[warn] $(basename "$wav") is ${sr} Hz (target ${TARGET_SR})" >&2
    return 0
  fi
  local tmp="${wav}.tmp.wav"
  ffmpeg -y -hide_banner -loglevel error -i "$wav" \
         -ar "$TARGET_SR" -ac 1 -acodec pcm_s16le "$tmp"
  mv "$tmp" "$wav"
}

# CLI flags (same as English script)
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
# Arabic-tuned text sanitiser. Arabic uses different punctuation:
#   ، (U+060C arabic comma)
#   ؛ (U+061B arabic semicolon)
#   ؟ (U+061F arabic question mark)
# Piper handles them natively, but leading punctuation runs (which
# the cleaner strips for English) should be stripped here too.
# ──────────────────────────────────────────────────────────────────
clean_for_tts() {
  local s="$1"
  # Strip Markdown emphasis (in case translator added italic markup).
  s=$(printf '%s' "$s" | sed -E 's/\*+([^*]+)\*+/\1/g')
  # Replace internal ellipses with arabic comma + space.
  s=$(printf '%s' "$s" | sed -E 's/\.{2,}/، /g')
  # Em-dash / en-dash → arabic comma + space.
  s=$(printf '%s' "$s" | sed 's/—/، /g; s/–/، /g')
  # Trim leading/trailing punctuation (incl. arabic comma).
  s=$(printf '%s' "$s" | sed -E 's/^[[:space:],.;:!?\-،؛]+//; s/[[:space:],;:،؛]+$//')
  s=$(printf '%s' "$s" | tr -s '[:space:]' ' ')
  if [[ -z "${s// }" ]]; then
    printf '[[silent]]'
  else
    printf '%s' "$s"
  fi
}

write_silent_wav() {
  local out="$1"
  local dur="${2:-0.4}"
  if command -v ffmpeg >/dev/null 2>&1; then
    ffmpeg -y -f lavfi -i "anullsrc=r=${TARGET_SR}:cl=mono" -t "$dur" \
           -acodec pcm_s16le "$out" </dev/null >/dev/null 2>&1
  fi
}

# ──────────────────────────────────────────────────────────────────
# Main loop
# ──────────────────────────────────────────────────────────────────
mkdir -p "$OUT_BASE"

declare -A gen_count skip_count

total_lines=${#LINES[@]}
i=0

for entry in "${LINES[@]}"; do
  i=$((i + 1))
  character="${entry%%|*}"
  rest="${entry#*|}"
  line_id="${rest%%|*}"
  text="${rest#*|}"

  if [[ "$line_id" != $ONLY_GLOB ]]; then continue; fi

  if ! model_name=$(get_voice_field "$character" 2); then
    continue
  fi
  length_scale=$(get_voice_field "$character" 3)
  model_file="${MODELS_DIR}/${model_name}.onnx"
  config_file="${MODELS_DIR}/${model_name}.onnx.json"

  if [[ ! -f "$model_file" || ! -f "$config_file" ]]; then
    echo "[error] missing model $model_name — run bash Tools/download_voice_models.sh" >&2
    exit 1
  fi

  out_dir="${OUT_BASE}/${character}"
  mkdir -p "$out_dir"
  out_wav="${out_dir}/${line_id}.wav"

  spoken=$(clean_for_tts "$text")

  needs_regen=0
  if [[ "$spoken" != "$text" ]]; then needs_regen=1; fi

  if [[ -f "$out_wav" && $FORCE -eq 0 && $needs_regen -eq 0 ]]; then
    printf "[%3d/%3d] skip  ar/%s/%s\n" "$i" "$total_lines" "$character" "$line_id"
    skip_count[$character]=$(( ${skip_count[$character]:-0} + 1 ))
    continue
  fi

  printf "[%3d/%3d] gen   ar/%s/%s\n" "$i" "$total_lines" "$character" "$line_id"

  if [[ "$spoken" == "[[silent]]" ]]; then
    write_silent_wav "$out_wav" 0.4
  else
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

echo
echo "──── Summary (Arabic) ─────────────────────────────────"
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
echo "Next:  open Unity → Hearthbound → ⚙️ Advanced → 🎙️ Phase 60 — Rebuild Arabic Voice Library"
echo
