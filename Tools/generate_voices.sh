#!/usr/bin/env bash
# Tools/generate_voices.sh
# Regenerates every Doris voice clip via macOS `say`. Safe to re-run.
#
# Phase 32 — Voice Acting MVP. See Docs/PROGRESS.md for the design rationale.
# D-051: voice clips live under Assets/_Project/Audio/Voice/{character}/{lineId}.wav;
#        the generation pipeline is decoupled from the runtime — any TTS that
#        produces 22 kHz mono PCM16 .wav can drop in (ElevenLabs / XTTS / Piper
#        all work; just overwrite the .wav files and the SO re-binds on
#        OnValidate / the editor utility re-scan).
#
# Voice casting (locked):
#   Doris   — Samantha @ en_US, mid-range warm
#   Gerrold — Daniel   @ en_GB, weathered male       (M2 stub — not generated here)
#   Marin   — Tessa    @ en_ZA, soft whisper        (M2 stub — not generated here)
#   Narrator — Karen   @ en_AU                       (Memory Dream stub — not generated here)
#
# Idempotency: every .wav is skipped if it already exists. Delete a clip and
# re-run to regenerate just that one.

set -euo pipefail

OUT="Assets/_Project/Audio/Voice/Doris"
mkdir -p "$OUT"
VOICE="Samantha"
RATE=180   # words-per-minute; Doris is unhurried

if ! command -v say >/dev/null 2>&1; then
  echo "error: macOS \`say\` not found on PATH. This script is macOS-only." >&2
  echo "       Drop your own 22 kHz mono PCM16 .wav files under $OUT/ instead." >&2
  exit 1
fi
if ! command -v afconvert >/dev/null 2>&1; then
  echo "error: \`afconvert\` not found on PATH. This script is macOS-only." >&2
  exit 1
fi

declare -a LINES=(
  "doris_m1_greet_01|You're the new one."
  "doris_m1_greet_02|I thought you'd be taller."
  "doris_m1_greet_03|Don't mind me — I thought that about the old one, too."
  "doris_m1_greet_04|Come in. The kettle's only just stopped."
  "doris_m1_reply_help_01|Aye. The very same."
  "doris_m1_reply_help_02|They've put my name on the sign and everything. Look — there."
  "doris_m1_reply_silent_01|A quiet one, then. Good."
  "doris_m1_reply_silent_02|The bread likes quiet."
  "doris_m1_reply_unsure_01|... Mm."
  "doris_m1_reply_unsure_02|That's a conversation for a longer day."
  "doris_m1_reply_unsure_03|Come in. Tea first."
  "doris_m1_kitchen_01|Mind the flour."
  "doris_m1_kitchen_02|I haven't swept since Tuesday. I keep meaning to."
  "doris_m1_kitchen_03|... The shop next door is yours. The Hollow."
  "doris_m1_kitchen_04|I've been keeping the key safe for you."
  "doris_m1_offer_01|... I have something for you. Before you go in."
  "doris_m1_offer_02|I'd like to be your first customer, if that's all right."
  "doris_m1_memory_01|This is the memory."
  "doris_m1_memory_02|Hold it like you'd hold a hot bun. Not by the side. Underneath."
  "doris_m1_memory_03|It's a small thing."
  "doris_m1_memory_04|First time I made bread that didn't shame me."
  "doris_m1_memory_05|Most days I think of it."
  "doris_m1_memory_06|I want to put it down, now, for a while."
  "doris_m1_memory_07|Will you take it?"
  "doris_m1_defer_01|Aye. Some days are not the day."
  "doris_m1_defer_02|I'll be here when one is."
  "doris_m1_story_01|I was twenty-four."
  "doris_m1_story_02|The oven was new. The bricks were new. I was new."
  "doris_m1_story_03|I'd been baking other people's bread for nine years."
  "doris_m1_story_04|That morning was the first morning that was just mine."
  "doris_m1_story_05|I want to take a rest from carrying it. That's all."
  "doris_m1_price_01|Four coppers, if you're asking."
  "doris_m1_price_02|It's a small memory. I'll not have you overpay your first day."
  "doris_m1_price_fair|Aye. Thank you."
  "doris_m1_price_high_01|That's too much. I'll not have you ruin yourself."
  "doris_m1_price_high_02|Take it back. — Well. Take *some* back."
  "doris_m1_price_high_03|Five, then. Final."
  "doris_m1_price_low_01|..."
  "doris_m1_price_low_02|Aye, that'll do. Bring the rest when you find some."
  "doris_m1_handover_01|There."
  "doris_m1_handover_02|The old keeper showed me how to make it. Took me four tries."
  "doris_m1_handover_03|I cracked the first three. The cat watched me. Judged me, I think."
  "doris_m1_handover_04|I'll be in the bakery if you want me. Knock twice."
  "doris_m1_handover_05|There's a kettle on the workbench. Mind the wood stove — it bites."
  "doris_m1_polish_watch|I'll wait. Take your time, Keeper."
  "doris_m1_polish_done_01|Aye."
  "doris_m1_polish_done_02|There it is. That's the morning."
  "doris_m1_polish_sleep_01|Sleep tonight. Dreams come."
  "doris_m1_polish_sleep_02|I'll see you again, eventually."
)

for entry in "${LINES[@]}"; do
  id="${entry%%|*}"
  text="${entry#*|}"
  aiff="/tmp/${id}.aiff"
  wav="${OUT}/${id}.wav"
  if [[ -f "$wav" ]]; then
    echo "[skip] $id (exists)"
    continue
  fi
  echo "[gen]  $id  ::  $text"
  say -v "$VOICE" -r $RATE -o "$aiff" "$text"
  # 22 kHz mono PCM16 — small file, fine for cozy dialogue, Unity imports natively.
  afconvert "$aiff" "$wav" -f WAVE -d LEI16@22050 -c 1
  rm -f "$aiff"
done

echo "Done. $(ls -1 $OUT/*.wav 2>/dev/null | wc -l | tr -d ' ') clips written to $OUT"
