#!/usr/bin/env bash
# Tools/download_voice_models.sh
# ────────────────────────────────────────────────────────────────────
# Phase 32 — Voice Acting MVP. Downloads the Piper TTS voice models
# referenced by Tools/generate_voices.sh into Tools/voice_models/.
#
# One-time setup; safe to re-run (idempotent — skips files that already
# exist). Cross-platform: Linux / macOS / Windows (WSL / Git-Bash).
#
# Source:    https://huggingface.co/rhasspy/piper-voices
# License:   per-model (most are MIT / Apache-2 / CC-BY-4.0); see
#            <model>.onnx.json `"license"` field after download.
# Size:      ~250 MB total (each *-medium model is ~50–70 MB).
#
# Add new models by appending to the MODELS array below; the script
# downloads both the .onnx weights and the .onnx.json config side-by-side.
# ────────────────────────────────────────────────────────────────────

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
MODELS_DIR="${ROOT}/Tools/voice_models"

mkdir -p "$MODELS_DIR"

# ──────────────────────────────────────────────────────────────────
# Pick a downloader: curl preferred, wget fallback.
# ──────────────────────────────────────────────────────────────────
if command -v curl >/dev/null 2>&1; then
  DL() { curl -fL --retry 3 --retry-delay 2 --progress-bar -o "$2" "$1"; }
elif command -v wget >/dev/null 2>&1; then
  DL() { wget -q --show-progress --tries=3 -O "$2" "$1"; }
else
  echo "[error] need either curl or wget on PATH" >&2
  exit 1
fi

# ──────────────────────────────────────────────────────────────────
# Model registry — must match Tools/generate_voices.sh's VOICES table.
#
# Each entry:   MODEL_BASENAME | HF_RELATIVE_PATH
#
#   MODEL_BASENAME = the filename Tools/generate_voices.sh expects
#                    (we save as <basename>.onnx and <basename>.onnx.json)
#   HF_RELATIVE_PATH = the path under
#                    https://huggingface.co/rhasspy/piper-voices/resolve/main/
#                    (without the .onnx / .onnx.json extension)
# ──────────────────────────────────────────────────────────────────
declare -a MODELS=(
  "en_US-lessac-medium|en/en_US/lessac/medium/en_US-lessac-medium"
  "en_GB-alan-medium|en/en_GB/alan/medium/en_GB-alan-medium"
  "en_US-amy-medium|en/en_US/amy/medium/en_US-amy-medium"
  "en_GB-jenny_dioco-medium|en/en_GB/jenny_dioco/medium/en_GB-jenny_dioco-medium"
  # ── Phase 60 — Arabic Localization MVP ──────────────────────────
  # `ar_JL-medium` is the only Arabic medium-quality voice in the
  # Piper roster as of 2026-05. It's a single female voice, clear
  # Modern-Standard-Arabic delivery. We tune length_scale + pitch in
  # generate_voices.sh to differentiate Doris / Marin / Narrator /
  # Pickle in Arabic (same trick as the en_US-amy-medium re-use for
  # English Marin + Pickle). Gerrold uses the same model with a
  # lower pitch to approximate a male voice — the cleanest path
  # while we wait for an Arabic male model upstream.
  "ar_JL-medium|ar/ar_JL/medium/ar_JL-medium"
)

BASE="https://huggingface.co/rhasspy/piper-voices/resolve/main"

echo "Downloading Piper voice models → $MODELS_DIR"
echo

for spec in "${MODELS[@]}"; do
  name="${spec%%|*}"
  hfpath="${spec#*|}"
  for ext in onnx onnx.json; do
    target="${MODELS_DIR}/${name}.${ext}"
    if [[ -f "$target" && -s "$target" ]]; then
      printf "  [skip] %s\n" "${name}.${ext}"
      continue
    fi
    url="${BASE}/${hfpath}.${ext}"
    printf "  [get]  %s\n" "${name}.${ext}"
    if ! DL "$url" "$target"; then
      echo "[error] download failed: $url" >&2
      rm -f "$target"
      exit 1
    fi
  done
done

echo
echo "Installed models:"
for f in "${MODELS_DIR}"/*.onnx; do
  [[ -f "$f" ]] || continue
  sz=$(du -h "$f" | awk '{print $1}')
  printf "  %-44s  %s\n" "$(basename "$f")" "$sz"
done

echo
echo "Next:  bash Tools/generate_voices.sh"
