// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase32_VoiceLibraryBuilder
//
// Phase 32 — Voice Acting MVP.
//
// Editor utility that scans `Assets/_Project/Audio/Voice/{Character}/*.wav` and
// auto-populates `Assets/_Project/Resources/HearthboundVoiceLibrary.asset`.
// Each .wav filename's basename becomes a `VoiceLibrarySO.Entry.lineId`.
//
// Idempotency: re-running keeps existing inspector-tuned `volume` / `pitch`
// values on entries whose lineId is unchanged. New .wav files get added with
// per-character casting defaults (see GetCastingDefaults — Phase 32.10).
// Entries whose .wav was deleted are pruned. The Resources/ folder is
// created on demand.
//
// Menu path: Hearthbound → ⚙️ Advanced → 🎙️ Phase 32 — Rebuild Voice Library
//
// Workflow (matches Step 6 of the Phase 32 task):
//   1. `bash Tools/generate_voices.sh` produces 77 .wav files under
//      Assets/_Project/Audio/Voice/{Doris,Gerrold,Marin,Narrator,Pickle}/.
//      (Or run `Hearthbound → ⚙️ Advanced → 🎙️ Phase 46 — Generate Voices
//      (cross-platform)` to use the in-Editor espeak-ng pipeline instead.)
//   2. Click this menu item — the SO at Resources/HearthboundVoiceLibrary.asset
//      is created (or updated in place) with up to 77 entries auto-bound to
//      the AudioClips. The console prints a summary.
//   3. Press Play. VoicePlayer.Awake calls Resources.Load and the dialogue
//      starts speaking.
//
// Chained from `Hearthbound → 🚀 Build Everything` (Step 8.5) so the user
// gets the voice library auto-built on the canonical one-click workflow —
// no extra menu click required. The chain invokes `Build()` (silent — no
// pop-up dialog) instead of the menu's `RebuildFromMenu()`.
//
// Chained from `Hearthbound → 🔍 Diagnose Build` (Step 6) so wiring problems
// (missing SO, empty entries, broken AudioClip refs) surface in the aggregate
// audit. The diagnostic invokes `Diagnose()` — read-only, never modifies.
//
// D-058: the .wav files themselves are the source of truth. Any TTS that
// produces 22 kHz mono PCM16 .wav can drop in (ElevenLabs / XTTS / Piper /
// espeak-ng / human VO) — rerun this menu item after a regeneration and the
// bindings refresh.
//
// ── Phase 32.10 (2026-05-27) ─────────────────────────────────────
// NEW entries pick up per-character runtime tuning defaults via the
// new `GetCastingDefaults(character)` helper. Defaults sourced from
// Docs/VOICE_CASTING.md § 2 (the canonical casting table):
//   Doris    — 0.95 vol, 0.98 pitch
//   Gerrold  — 0.90 vol, 0.92 pitch
//   Marin    — 0.75 vol, 1.05 pitch
//   Narrator — 0.95 vol, 1.00 pitch
//   Pickle   — 0.80 vol, 1.10 pitch
// EXISTING entries' inspector-tuned values are preserved across rebuilds
// (unchanged from Phase 32 behaviour) — the defaults only land on first
// import.

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using HearthboundHollow.Audio;

namespace HearthboundHollow.EditorTools
{
    public static class Phase32_VoiceLibraryBuilder
    {
        public const string VoiceRoot     = "Assets/_Project/Audio/Voice";
        public const string ResourcesDir  = "Assets/_Project/Resources";
        public const string AssetPath     = ResourcesDir + "/HearthboundVoiceLibrary.asset";

        [MenuItem("Hearthbound/⚙️ Advanced/🎙️ Phase 32 — Rebuild Voice Library", priority = 1200)]
        public static void RebuildFromMenu() => Rebuild(verbose: true);

        /// <summary>
        /// Silent entry-point used by the `🚀 Build Everything` chain. Same as
        /// <see cref="RebuildFromMenu"/> but skips the success dialog so the
        /// capstone runs uninterrupted. The console summary still prints.
        /// </summary>
        public static void Build() => Rebuild(verbose: false);

        /// <summary>
        /// Read-only audit invoked by `🔍 Diagnose Build`. Reports:
        ///   - whether `Resources/HearthboundVoiceLibrary.asset` exists,
        ///   - how many entries it has,
        ///   - how many of those entries have a non-null `clip` binding,
        ///   - how many .wav files live under `Audio/Voice/**` on disk,
        ///   - whether the entry count matches the on-disk count.
        /// Never modifies any asset.
        /// </summary>
        public static void Diagnose()
        {
            var verdict = new System.Text.StringBuilder();
            verdict.AppendLine("[Phase 32 — Voice Library Diagnostic]");

            bool soPresent = false;
            int entryCount = 0;
            int boundClips = 0;
            VoiceLibrarySO lib = AssetDatabase.LoadAssetAtPath<VoiceLibrarySO>(AssetPath);
            if (lib == null)
            {
                verdict.AppendLine($"⚠ No VoiceLibrarySO at {AssetPath}. " +
                                   "Run `bash Tools/generate_voices.sh` (macOS) then " +
                                   "`Hearthbound → ⚙️ Advanced → 🎙️ Phase 32 — Rebuild Voice Library` " +
                                   "(or just run `🚀 Build Everything`).");
            }
            else
            {
                soPresent = true;
                entryCount = lib.entries != null ? lib.entries.Count : 0;
                if (lib.entries != null)
                {
                    foreach (var e in lib.entries)
                        if (e.clip != null) boundClips++;
                }
                verdict.AppendLine($"✓ VoiceLibrarySO loaded at {AssetPath}.");
                verdict.AppendLine($"  • {entryCount} entries total");
                verdict.AppendLine($"  • {boundClips} entries with a bound AudioClip");
                int orphan = entryCount - boundClips;
                if (orphan > 0)
                    verdict.AppendLine($"  ⚠ {orphan} entries have NO clip — re-run Rebuild Voice Library.");
            }

            int wavCount = 0;
            if (Directory.Exists(VoiceRoot))
            {
                wavCount = Directory.GetFiles(VoiceRoot, "*.wav", SearchOption.AllDirectories).Length;
                verdict.AppendLine($"✓ Audio/Voice root present — {wavCount} .wav file(s) on disk.");
            }
            else
            {
                verdict.AppendLine($"⚠ Audio/Voice root not found at {VoiceRoot}. " +
                                   "Voice playback is disabled (typewriter dialogue still works). " +
                                   "Run `bash Tools/generate_voices.sh` on macOS to generate Doris's 48 clips.");
            }

            // Mismatch detection.
            if (soPresent && wavCount > 0 && wavCount != entryCount)
            {
                verdict.AppendLine($"⚠ Mismatch: {wavCount} .wav file(s) on disk but {entryCount} SO entry/entries. " +
                                   "Re-run Rebuild Voice Library to re-sync.");
            }

            // Always tell the user what file-swap policy applies.
            verdict.AppendLine();
            verdict.AppendLine("D-058: voice clips live under Audio/Voice/{character}/{lineId}.wav. " +
                               "Any 22 kHz mono PCM16 .wav drops in (ElevenLabs / XTTS / Piper / human VO) " +
                               "— overwrite the .wav files, rerun Rebuild Voice Library, done.");

            string text = verdict.ToString();
            Debug.Log(text);
            EditorUtility.DisplayDialog("Phase 32 — Voice Library Diagnostic", text, "OK");
        }

        /// <summary>
        /// Scan VoiceRoot for .wav files and produce / update the VoiceLibrarySO
        /// asset at AssetPath. Safe to call from another editor builder
        /// (e.g. the 🚀 Build Everything chain) — set verbose=false to suppress
        /// the success dialog.
        /// </summary>
        public static VoiceLibrarySO Rebuild(bool verbose)
        {
            // 1. Ensure the Resources folder exists.
            if (!AssetDatabase.IsValidFolder(ResourcesDir))
            {
                if (!AssetDatabase.IsValidFolder("Assets/_Project"))
                    AssetDatabase.CreateFolder("Assets", "_Project");
                AssetDatabase.CreateFolder("Assets/_Project", "Resources");
            }

            // 2. Load (or create) the library asset.
            var lib = AssetDatabase.LoadAssetAtPath<VoiceLibrarySO>(AssetPath);
            bool createdNew = false;
            if (lib == null)
            {
                lib = ScriptableObject.CreateInstance<VoiceLibrarySO>();
                AssetDatabase.CreateAsset(lib, AssetPath);
                createdNew = true;
            }

            // 3. Build a snapshot of existing (lineId -> Entry) so we preserve
            //    inspector-tuned volume / pitch values across rebuilds.
            var existing = new Dictionary<string, VoiceLibrarySO.Entry>();
            if (lib.entries != null)
            {
                foreach (var e in lib.entries)
                    if (!string.IsNullOrEmpty(e.lineId)) existing[e.lineId] = e;
            }

            // 4. Walk the Voice root recursively. For every .wav, derive the
            //    lineId from the filename and load the AudioClip asset.
            var newEntries = new List<VoiceLibrarySO.Entry>();
            int added = 0, kept = 0, missing = 0;

            if (Directory.Exists(VoiceRoot))
            {
                // Recursive scan so future characters (Gerrold/, Marin/, etc.)
                // are picked up automatically.
                var wavPaths = Directory.GetFiles(VoiceRoot, "*.wav", SearchOption.AllDirectories);
                System.Array.Sort(wavPaths); // deterministic order in the inspector

                foreach (var fsPath in wavPaths)
                {
                    // Normalise to forward slashes so AssetDatabase is happy on Windows.
                    var assetPath = fsPath.Replace('\\', '/');
                    // Strip any leading /workspace/... — keep from "Assets/" onward.
                    int idx = assetPath.IndexOf("Assets/");
                    if (idx > 0) assetPath = assetPath.Substring(idx);

                    var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
                    if (clip == null)
                    {
                        // .wav may not be imported yet (e.g. CI race). Skip with a warning.
                        Debug.LogWarning(
                            $"[Phase32_VoiceLibraryBuilder] AudioClip not yet imported for {assetPath} — skip.");
                        missing++;
                        continue;
                    }

                    string lineId = Path.GetFileNameWithoutExtension(assetPath);
                    // Phase 32.10 — derive the character from the parent folder
                    // name (Audio/Voice/<Character>/<lineId>.wav) so per-character
                    // casting defaults can be applied to new entries.
                    string character = Path.GetFileName(Path.GetDirectoryName(assetPath));

                    if (existing.TryGetValue(lineId, out var prev))
                    {
                        prev.clip = clip; // refresh the binding (in case the .wav was re-imported)
                        newEntries.Add(prev);
                        kept++;
                    }
                    else
                    {
                        var (defaultVol, defaultPitch) = GetCastingDefaults(character);
                        newEntries.Add(new VoiceLibrarySO.Entry
                        {
                            lineId = lineId,
                            clip   = clip,
                            volume = defaultVol,
                            pitch  = defaultPitch,
                        });
                        added++;
                    }
                }
            }
            else
            {
                Debug.LogWarning(
                    $"[Phase32_VoiceLibraryBuilder] Voice root not found at {VoiceRoot}. " +
                    "Run `bash Tools/generate_voices.sh` first (macOS).");
            }

            // (existing - kept) gives the number of pruned entries whose .wav was deleted.
            int dropped = existing.Count - kept;

            lib.entries = newEntries;
            EditorUtility.SetDirty(lib);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string summary =
                $"VoiceLibrarySO {(createdNew ? "created" : "updated")} at {AssetPath}\n\n" +
                $"  • {newEntries.Count} entries total\n" +
                $"  • {added} new (auto-bound from .wav)\n" +
                $"  • {kept} kept (inspector-tuned volume/pitch preserved)\n" +
                $"  • {dropped} pruned (.wav no longer exists)\n" +
                $"  • {missing} skipped (AudioClip not imported yet)";

            Debug.Log("[Phase32_VoiceLibraryBuilder] " + summary.Replace("\n\n", "  ").Replace('\n', ' '));

            if (verbose)
            {
                EditorUtility.DisplayDialog(
                    "Phase 32 — Voice Library Rebuilt",
                    summary +
                    "\n\nIf you just ran `bash Tools/generate_voices.sh` on macOS and " +
                    "the count looks low, make sure Unity finished importing the new " +
                    ".wav files (the Project window shows the spinner in the bottom " +
                    "right) and rerun this menu item.",
                    "OK");
            }

            return lib;
        }

        /// <summary>
        /// Phase 32.10 — per-character runtime tuning defaults applied to NEW
        /// VoiceLibrarySO entries (existing entries' inspector-tuned values are
        /// preserved across rebuilds). Values are sourced from
        /// Docs/VOICE_CASTING.md § 2 (the canonical casting table). Unknown
        /// characters fall back to neutral 1.0 / 1.0.
        ///
        /// The defaults shape the audio mix without touching the .wav files:
        ///   Doris    (warm baker)        — 0.95 vol, 0.98 pitch (slightly weighted)
        ///   Gerrold  (weathered widower) — 0.90 vol, 0.92 pitch (grandfatherly baritone push)
        ///   Marin    (soft predecessor)  — 0.75 vol, 1.05 pitch (whisper, slight lift)
        ///   Narrator (neutral British)   — 0.95 vol, 1.00 pitch (clean)
        ///   Pickle   (bright cat aside)  — 0.80 vol, 1.10 pitch (sly, higher register)
        /// </summary>
        public static (float volume, float pitch) GetCastingDefaults(string character)
        {
            if (string.IsNullOrEmpty(character)) return (1f, 1f);
            switch (character)
            {
                case "Doris":    return (0.95f, 0.98f);
                case "Gerrold":  return (0.90f, 0.92f);
                case "Marin":    return (0.75f, 1.05f);
                case "Narrator": return (0.95f, 1.00f);
                case "Pickle":   return (0.80f, 1.10f);
                default:         return (1.00f, 1.00f);
            }
        }
    }
}
