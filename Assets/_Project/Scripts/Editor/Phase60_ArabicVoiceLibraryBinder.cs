// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase60_ArabicVoiceLibraryBinder
//
// Phase 60 — Arabic Localization MVP.
//
// Editor utility that scans `Assets/_Project/Audio/Voice/ar/{Character}/*.wav`
// and binds each clip into the matching entry's `clipAr` slot in
// `Assets/_Project/Resources/HearthboundVoiceLibrary.asset`.
//
// English `clip` slot is untouched. New Arabic-only lineIds (e.g. M3+
// content not yet voiced in English) create a new Entry with both
// `clip == null` and `clipAr == <ar wav>` — VoicePlayer falls back
// gracefully in the unlikely "Arabic recorded before English" scenario.
//
// Workflow:
//   1. bash Tools/generate_voices_ar.sh     # ~25 s, 77 .wav files
//   2. Hearthbound → ⚙️ Advanced → 🎙️ Phase 60 — Bind Arabic Voice Clips
//   3. Press Play, set Language → العربية, Doris speaks Arabic.
//
// Idempotent: re-running with the same .wav set is a no-op (no SO mutation).
// New / removed Arabic clips are diffed and applied in one EditorUtility
// transaction so the dirty-file count stays clean for git.
//
// Chained from `Hearthbound → 🚀 Build Everything` (Step 8.6) and
// `Hearthbound → 🔍 Diagnose Build` (Step 7).

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using HearthboundHollow.Audio;

namespace HearthboundHollow.EditorTools
{
    public static class Phase60_ArabicVoiceLibraryBinder
    {
        public const string ArVoiceRoot = "Assets/_Project/Audio/Voice/ar";
        public const string LibraryAssetPath = "Assets/_Project/Resources/HearthboundVoiceLibrary.asset";

        [MenuItem("Hearthbound/⚙️ Advanced/🎙️ Phase 60 — Bind Arabic Voice Clips", priority = 1210)]
        public static void BindFromMenu() => Bind(verbose: true);

        /// <summary>
        /// Silent entry-point used by the `🚀 Build Everything` chain.
        /// </summary>
        public static void Build() => Bind(verbose: false);

        public static int Bind(bool verbose)
        {
            // Phase 60 — Resolve (or report missing) the canonical library asset.
            var library = AssetDatabase.LoadAssetAtPath<VoiceLibrarySO>(LibraryAssetPath);
            if (library == null)
            {
                if (verbose)
                {
                    EditorUtility.DisplayDialog("Phase 60 — Arabic voice binding",
                        "No HearthboundVoiceLibrary.asset found at\n" +
                        LibraryAssetPath + "\n\n" +
                        "Run Hearthbound → ⚙️ Advanced → 🎙️ Phase 32 — Rebuild Voice Library first.",
                        "OK");
                }
                else
                {
                    Debug.LogWarning(
                        "[Phase60] No HearthboundVoiceLibrary.asset found — " +
                        "Phase 32 binder must run first.");
                }
                return 0;
            }

            // Build a map of lineId → ar/.wav path.
            var arClips = ScanArabicWavs();
            if (arClips.Count == 0)
            {
                if (verbose)
                {
                    EditorUtility.DisplayDialog("Phase 60 — Arabic voice binding",
                        "No .wav files found under\n" + ArVoiceRoot +
                        "\n\nRun:\n    bash Tools/generate_voices_ar.sh\nfirst.",
                        "OK");
                }
                else
                {
                    Debug.Log(
                        "[Phase 60] No Arabic .wav clips under " + ArVoiceRoot +
                        " — skipping bind. (Translation subtitles still work; " +
                        "voice will fall back to English source.)");
                }
                return 0;
            }

            // Apply the bindings + track diffs for the summary.
            int boundExisting = 0, addedNew = 0, cleared = 0;
            var libraryEntries = library.entries ?? new List<VoiceLibrarySO.Entry>();
            // First pass — update existing entries.
            for (int i = 0; i < libraryEntries.Count; i++)
            {
                var e = libraryEntries[i];
                if (string.IsNullOrEmpty(e.lineId)) continue;
                if (arClips.TryGetValue(e.lineId, out var arPath))
                {
                    var arClip = AssetDatabase.LoadAssetAtPath<AudioClip>(arPath);
                    if (arClip == null) continue;
                    if (e.clipAr != arClip)
                    {
                        e.clipAr = arClip;
                        libraryEntries[i] = e;
                        boundExisting++;
                    }
                    arClips.Remove(e.lineId);
                }
                else if (e.clipAr != null)
                {
                    // The Arabic .wav was deleted upstream — clear the slot.
                    e.clipAr = null;
                    libraryEntries[i] = e;
                    cleared++;
                }
            }
            // Second pass — add lineIds that exist in Arabic but had no entry yet.
            foreach (var kvp in arClips)
            {
                var arClip = AssetDatabase.LoadAssetAtPath<AudioClip>(kvp.Value);
                if (arClip == null) continue;
                libraryEntries.Add(new VoiceLibrarySO.Entry
                {
                    lineId = kvp.Key,
                    clip   = null,
                    clipAr = arClip,
                    volume = 1f,
                    pitch  = 1f,
                });
                addedNew++;
            }

            library.entries = libraryEntries;
            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();

            string summary =
                $"Phase 60 — Arabic voice clips bound.\n\n" +
                $"  • Existing entries updated:  {boundExisting}\n" +
                $"  • New ar-only entries added: {addedNew}\n" +
                $"  • Stale slots cleared:       {cleared}\n" +
                $"  • Total entries in library:  {library.entries.Count}\n\n" +
                $"Library: {LibraryAssetPath}";

            if (verbose)
            {
                Debug.Log("[Phase 60] " + summary);
                EditorUtility.DisplayDialog("Phase 60 — Arabic voice binding",
                    summary, "OK");
            }
            else
            {
                Debug.Log("[Phase 60] " + summary);
            }
            return boundExisting + addedNew;
        }

        private static Dictionary<string, string> ScanArabicWavs()
        {
            var map = new Dictionary<string, string>(80);
            if (!AssetDatabase.IsValidFolder(ArVoiceRoot)) return map;
            // Recursive scan — each Character subfolder may contain N .wav files.
            var guids = AssetDatabase.FindAssets("t:AudioClip", new[] { ArVoiceRoot });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".wav", System.StringComparison.OrdinalIgnoreCase)) continue;
                var lineId = Path.GetFileNameWithoutExtension(path);
                if (string.IsNullOrEmpty(lineId)) continue;
                // Last-wins on duplicate lineIds (shouldn't happen — character
                // folders + lineIds are intended to be 1:1).
                map[lineId] = path;
            }
            return map;
        }

        // ───── Read-only diagnostic ────────────────────────────────────

        [MenuItem("Hearthbound/⚙️ Advanced/🔊 Phase 60 — Diagnose Arabic Voice Bindings", priority = 1211)]
        public static void Diagnose() => DiagnoseImpl(verbose: true);

        public static int DiagnoseSilent() => DiagnoseImpl(verbose: false);

        private static int DiagnoseImpl(bool verbose)
        {
            var library = AssetDatabase.LoadAssetAtPath<VoiceLibrarySO>(LibraryAssetPath);
            int totalEntries = library != null && library.entries != null ? library.entries.Count : 0;
            int withEn = 0, withAr = 0, bothLanguages = 0, onlyEn = 0, onlyAr = 0, neither = 0;
            if (library != null && library.entries != null)
            {
                foreach (var e in library.entries)
                {
                    bool en = e.clip != null;
                    bool ar = e.clipAr != null;
                    if (en) withEn++;
                    if (ar) withAr++;
                    if (en && ar) bothLanguages++;
                    else if (en)  onlyEn++;
                    else if (ar)  onlyAr++;
                    else          neither++;
                }
            }

            var arClips = ScanArabicWavs();
            int arOnDisk = arClips.Count;

            string summary =
                $"Phase 60 — Arabic voice diagnostic.\n\n" +
                $"  Library: {(library != null ? LibraryAssetPath : "MISSING")}\n" +
                $"  Entries: {totalEntries}\n" +
                $"      with EN clip:        {withEn}\n" +
                $"      with AR clip:        {withAr}\n" +
                $"      with both languages: {bothLanguages}\n" +
                $"      EN only:             {onlyEn}\n" +
                $"      AR only:             {onlyAr}\n" +
                $"      no clip at all:      {neither}\n" +
                $"  AR .wav on disk:         {arOnDisk}\n\n" +
                ((arOnDisk > 0 && withAr == 0)
                    ? "⚠ .wav files exist but none are bound. Run\n" +
                      "   Hearthbound → ⚙️ Advanced → 🎙️ Phase 60 — Bind Arabic Voice Clips\n"
                    : (arOnDisk == 0
                        ? "ℹ No Arabic .wav files yet. Run\n" +
                          "   bash Tools/generate_voices_ar.sh\n"
                        : "✓ Arabic voice library is in a consistent state.\n"));

            if (verbose)
            {
                Debug.Log("[Phase 60 Diagnose] " + summary);
                EditorUtility.DisplayDialog("Phase 60 — Arabic voice diagnostic",
                    summary, "OK");
            }
            else
            {
                Debug.Log("[Phase 60 Diagnose] " + summary);
            }
            return withAr;
        }
    }
}
