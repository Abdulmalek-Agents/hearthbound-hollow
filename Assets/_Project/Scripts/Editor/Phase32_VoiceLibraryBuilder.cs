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
// defaults (volume = 1, pitch = 1). Entries whose .wav was deleted are
// pruned. The Resources/ folder is created on demand.
//
// Menu path: Hearthbound → ⚙️ Advanced → 🎙️ Phase 32 — Rebuild Voice Library
//
// Workflow (matches Step 6 of the Phase 32 task):
//   1. `bash Tools/generate_voices.sh` produces 48 .wav files under
//      Assets/_Project/Audio/Voice/Doris/.
//   2. Click this menu item — the SO at Resources/HearthboundVoiceLibrary.asset
//      is created (or updated in place) with 48 entries auto-bound to the
//      AudioClips. The console prints a summary.
//   3. Press Play. VoicePlayer.Awake calls Resources.Load and the dialogue
//      starts speaking.
//
// D-051: the .wav files themselves are the source of truth. Any TTS that
// produces 22 kHz mono PCM16 .wav can drop in (ElevenLabs / XTTS / Piper) —
// rerun this menu item after a regeneration and the bindings refresh.

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

                    if (existing.TryGetValue(lineId, out var prev))
                    {
                        prev.clip = clip; // refresh the binding (in case the .wav was re-imported)
                        newEntries.Add(prev);
                        kept++;
                    }
                    else
                    {
                        newEntries.Add(new VoiceLibrarySO.Entry
                        {
                            lineId = lineId,
                            clip   = clip,
                            volume = 1f,
                            pitch  = 1f,
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
    }
}
