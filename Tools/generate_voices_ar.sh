#!/usr/bin/env bash
# Tools/generate_voices_ar.sh
# ────────────────────────────────────────────────────────────────────
# D-077 — Arabic dialogue STOPGAP (machine translation + TTS).
#
# EXPLICIT Pillar 1 / D-065 OVERRIDE, by user request: this produces
# machine-translated Arabic dialogue + macOS `say -v Majed` voice clips
# so Arabic players get spoken, on-screen Arabic NOW. It is NOT
# greenlight-canon and should be replaced by a human translation + VO
# pass. The English Yarn remains the canonical voice.
#
# SINGLE SOURCE OF TRUTH: the LINES_AR table below feeds BOTH outputs, so
# the dialogue box text and the spoken clip can never drift apart:
#   1. Arabic voice clips → Assets/_Project/Audio/Voice_ar/<Char>/<lineId>.wav
#      (22 kHz mono PCM16 — same spec as the English pipeline; D-058).
#      VoicePlayer plays these when Arabic is active (Phase 57).
#   2. C# text table → Assets/_Project/Scripts/Core/DialogueLocalizationData.cs
#      (lineId → Arabic), read by DialogueLocalization (Phase 57) so the
#      dialogue box shows the matching shaped Arabic.
#
# After running:  Unity → Hearthbound → ⚙️ Advanced → Phase 57 —
#                 Scaffold Arabic Dialogue + Voice   (builds the AR voice library)
#
# Requires: macOS `say` with the Arabic "Majed" voice + ffmpeg.
# ────────────────────────────────────────────────────────────────────
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
OUT_BASE="${ROOT}/Assets/_Project/Audio/Voice_ar"
CS_OUT="${ROOT}/Assets/_Project/Scripts/Core/DialogueLocalizationData.cs"
VOICE="Majed"
RATE=176
TARGET_SR=22050

command -v say    >/dev/null 2>&1 || { echo "[error] macOS 'say' not found." >&2; exit 1; }
command -v ffmpeg >/dev/null 2>&1 || { echo "[error] ffmpeg not found (brew install ffmpeg)." >&2; exit 1; }
if ! say -v '?' | grep -qiE "^${VOICE}[[:space:]]"; then
  echo "[error] Arabic voice '${VOICE}' not installed. System Settings → Accessibility → Spoken Content → System Voice → Manage Voices → Arabic." >&2
  exit 1
fi

# Character | lineId | Arabic text  (machine translation; clean — no leading ellipses).
declare -a LINES_AR=(
  "Doris|doris_m1_greet_01|أنتَ الجديد."
  "Doris|doris_m1_greet_02|ظننتُك أطول."
  "Doris|doris_m1_greet_03|لا تأبه لي — قلتُ ذلك عن القديم أيضًا."
  "Doris|doris_m1_greet_04|ادخل. الغلّاية توقّفت للتو."
  "Doris|doris_m1_reply_help_01|نعم. هو ذاته."
  "Doris|doris_m1_reply_help_02|وضعوا اسمي على اللافتة وكل شيء. انظر — هناك."
  "Doris|doris_m1_reply_silent_01|هادئ إذًا. جيّد."
  "Doris|doris_m1_reply_silent_02|الخبز يحب الهدوء."
  "Doris|doris_m1_reply_unsure_01|ممم."
  "Doris|doris_m1_reply_unsure_02|هذا حديث ليومٍ أطول."
  "Doris|doris_m1_reply_unsure_03|ادخل. الشاي أولًا."
  "Doris|doris_m1_kitchen_01|احذر الطحين."
  "Doris|doris_m1_kitchen_02|لم أكنس منذ الثلاثاء. أنوي ذلك دائمًا."
  "Doris|doris_m1_kitchen_03|المتجر المجاور لك. الجوف."
  "Doris|doris_m1_kitchen_04|كنتُ أحفظ المفتاح لك."
  "Doris|doris_m1_offer_01|لديّ شيء لك. قبل أن تدخل."
  "Doris|doris_m1_offer_02|أودّ أن أكون أول زبائنك، إن كان ذلك مناسبًا."
  "Doris|doris_m1_memory_01|هذه هي الذكرى."
  "Doris|doris_m1_memory_02|أمسكها كما تمسك كعكة ساخنة. لا من الجانب. من الأسفل."
  "Doris|doris_m1_memory_03|إنها شيء صغير."
  "Doris|doris_m1_memory_04|أول مرة خبزتُ فيها خبزًا لم يُخجلني."
  "Doris|doris_m1_memory_05|أفكّر فيها أغلب الأيام."
  "Doris|doris_m1_memory_06|أريد أن أضعها جانبًا، الآن، لبعض الوقت."
  "Doris|doris_m1_memory_07|هل تأخذها؟"
  "Doris|doris_m1_defer_01|نعم. بعض الأيام ليست هي اليوم المناسب."
  "Doris|doris_m1_defer_02|سأكون هنا حين يأتي ذلك اليوم."
  "Doris|doris_m1_story_01|كنتُ في الرابعة والعشرين."
  "Doris|doris_m1_story_02|كان الفرن جديدًا. والطوب جديدًا. وكنتُ جديدة."
  "Doris|doris_m1_story_03|كنتُ أخبز خبز الآخرين تسع سنوات."
  "Doris|doris_m1_story_04|ذلك الصباح كان أول صباحٍ يخصّني وحدي."
  "Doris|doris_m1_story_05|أريد أن أستريح من حملها. هذا كل شيء."
  "Doris|doris_m1_price_01|أربعة نحاسات، إن كنت تسأل."
  "Doris|doris_m1_price_02|إنها ذكرى صغيرة. لن أدعك تدفع فوق حقّها في أول يوم."
  "Doris|doris_m1_price_fair|نعم. شكرًا لك."
  "Doris|doris_m1_price_high_01|هذا كثير جدًا. لن أدعك تُفلس نفسك."
  "Doris|doris_m1_price_high_02|استرجعه. — حسنًا. استرجع بعضه."
  "Doris|doris_m1_price_high_03|خمسة إذًا. نهائيًا."
  "Doris|doris_m1_price_low_01|…"
  "Doris|doris_m1_price_low_02|نعم، هذا يكفي. أحضر الباقي حين تجده."
  "Doris|doris_m1_handover_01|ها هي."
  "Doris|doris_m1_handover_02|علّمني الحارس القديم كيف أصنعها. استغرقني ذلك أربع محاولات."
  "Doris|doris_m1_handover_03|كسرتُ الثلاث الأولى. راقبتني القطة. وحكمت عليّ، على ما أظن."
  "Doris|doris_m1_handover_04|سأكون في المخبز إن احتجتني. اطرق مرتين."
  "Doris|doris_m1_handover_05|هناك غلّاية على طاولة العمل. احذر موقد الحطب — إنه يعض."
  "Doris|doris_m1_polish_watch|سأنتظر. خذ وقتك، أيها الحارس."
  "Doris|doris_m1_polish_done_01|نعم."
  "Doris|doris_m1_polish_done_02|ها هي. هذا هو الصباح."
  "Doris|doris_m1_polish_sleep_01|نَم الليلة. الأحلام تأتي."
  "Doris|doris_m1_polish_sleep_02|سأراك مجددًا، في النهاية."
  "Doris|doris_m1_refused_01|المتجر ما زال لك."
  "Doris|doris_m1_refused_02|ادخل. اجلس قليلًا. الغلّاية تغلي."
  "Doris|doris_m1_refused_03|سأكون هنا حين تصبح مستعدًا."
  "Doris|doris_m1_polish_after_perfect|أدّيتها أنقى مما أتذكّرها. أظنك ستفي بالغرض."
  "Doris|doris_m1_polish_after_acceptable|أدّيتها بلطف. هذا ما يهم."
  "Doris|doris_m1_polish_after_mild|ما زال الصباح. أخفت قليلًا. لكنه لي. الأيام الأولى هكذا. لن أؤاخذك."
  "Gerrold|gerrold_m2_greet_01|أعتذر. لا أعرف كيف يُفترض أن يجري هذا."
  "Gerrold|gerrold_m2_greet_02|لديّ الـ — الشيء — لديّ هنا في هذا القماش."
  "Gerrold|gerrold_m2_greet_03|لفّته مارجري. أظنها لفّته لأجل هذا."
  "Gerrold|gerrold_m2_long_bit_01|أريد أن أحتفظ بزوجتي. لا أريد أن أحتفظ بالجزء الطويل."
  "Gerrold|gerrold_m2_long_bit_02|ليس جزء الموت. بل الجزء الطويل."
  "Gerrold|gerrold_m2_thank_01|شكرًا لك. لا أعرف إن كنت قد فعلت ما طلبت."
  "Gerrold|gerrold_m2_thank_02|أظنك فعلت ما بوسعك."
  "Gerrold|gerrold_m2_thank_03|سأعود إلى البيت وأرى ما يأتي به الصباح."
  "Marin|marin_note_lane_01|إن وجدت هذه، فالغلّاية ما زالت تعمل."
  "Marin|marin_note_lane_02|لا تثق بالرف الثالث. إنه يميل."
  "Marin|marin_note_hollow_01|بيكل يتذكّر الجميع. بيكل عادل."
  "Marin|marin_note_workbench_01|القماش لمسك الجواهر الدافئة. قماشي في الدرج."
  "Narrator|narrator_title_day1|اليوم الأول. الجوف."
  "Narrator|narrator_title_day2|اليوم الثاني. الحديقة."
  "Narrator|narrator_title_evening|يحلّ المساء. الغلّاية دافئة."
  "Narrator|narrator_title_dream|تغمض عينيها. تبدأ الذكرى."
  "Pickle|pickle_m1_aside_01|ممم. الجديد."
  "Pickle|pickle_m1_aside_02|إنها تراقبك. تراقب دائمًا."
  "Pickle|pickle_m1_aside_03|الخبز يحبك. وهي أيضًا، على ما أظن."
  "Pickle|pickle_m2_aside_01|يحضر قماشًا. لم يكن يحضر قماشًا من قبل."
  "Pickle|pickle_m2_aside_02|اختر برفق. أنا أراقب."
  "Pickle|pickle_m2_aside_03|فعلت بلطف. سأتذكّر."
)

# ── 1) Generate Arabic voice clips ──────────────────────────────────
mkdir -p "$OUT_BASE"
gen=0
for entry in "${LINES_AR[@]}"; do
  character="${entry%%|*}"; rest="${entry#*|}"
  line_id="${rest%%|*}"; text="${rest#*|}"
  out_dir="${OUT_BASE}/${character}"; mkdir -p "$out_dir"
  out_wav="${out_dir}/${line_id}.wav"
  tmp_aiff="$(mktemp -t hh_ar).aiff"
  say -v "$VOICE" -r "$RATE" -o "$tmp_aiff" "$text" 2>/dev/null || true
  if [[ -s "$tmp_aiff" ]]; then
    ffmpeg -y -hide_banner -loglevel error -i "$tmp_aiff" \
           -ar "$TARGET_SR" -ac 1 -acodec pcm_s16le "$out_wav" </dev/null
    gen=$((gen+1))
    printf "[gen] %s/%s\n" "$character" "$line_id"
  else
    echo "[warn] say produced nothing for $line_id" >&2
  fi
  rm -f "$tmp_aiff"
done

# ── 2) Emit the C# dialogue-text table (single source of truth) ─────
{
  echo "// SPDX-License-Identifier: MIT"
  echo "// AUTO-GENERATED by Tools/generate_voices_ar.sh — DO NOT EDIT BY HAND."
  echo "// D-077 — Arabic dialogue STOPGAP (machine translation). Explicit Pillar 1 /"
  echo "// D-065 override by user request; replace with a human pass before ship."
  echo "// Read by DialogueLocalization; keyed by the same lineId the directors pass"
  echo "// to DialogueUI.PresentLine / VoicePlayer.Play."
  echo "using System.Collections.Generic;"
  echo ""
  echo "namespace HearthboundHollow.Core"
  echo "{"
  echo "    public static class DialogueLocalizationData"
  echo "    {"
  echo "        public static readonly Dictionary<string, string> Ar = new()"
  echo "        {"
  for entry in "${LINES_AR[@]}"; do
    rest="${entry#*|}"; line_id="${rest%%|*}"; text="${rest#*|}"
    printf '            ["%s"] = "%s",\n' "$line_id" "$text"
  done
  echo "        };"
  echo "    }"
  echo "}"
} > "$CS_OUT"

echo ""
echo "──── D-077 Arabic stopgap done ────"
echo "  voice clips generated: $gen  → $OUT_BASE/<Char>/<lineId>.wav"
echo "  text table written:    $CS_OUT  (${#LINES_AR[@]} lines)"
echo ""
echo "Next: Unity → Hearthbound → ⚙️ Advanced → Phase 57 — Scaffold Arabic Dialogue + Voice"
echo "      (builds Resources/HearthboundVoiceLibrary_ar from the new clips)."
