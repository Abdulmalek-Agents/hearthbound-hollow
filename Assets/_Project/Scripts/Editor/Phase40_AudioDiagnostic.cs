// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase40_AudioDiagnostic
//
// Phase 40 — Audio-specific diagnostic + preview utility.
//
// Read-only Editor menu that verifies the Phase 37 audio libraries
// (MusicLibrarySO, AmbienceLibrarySO, MumbleVoiceLibrarySO) are populated
// and the Phase 38 wiring is in place. Sister to Phase 35 (which is the
// project-wide audit); Phase 40 zooms into audio specifically.
//
// Also provides a `Preview` Editor command that previews a single cue id
// in the Editor without entering Play Mode — useful for the Audio Director
// to QA individual cues.
//
// USE:
//   Hearthbound → ⚙️ Advanced → 🔊 Phase 40 — Diagnose Audio Wiring
//   Hearthbound → ⚙️ Advanced → 🔊 Phase 40 — Preview <Character> Mumble
//
// Per D-052 (cutscene timelines must be built) and D-053 (audio assets are
// committed-as-source, not as binary) the diagnostic ALSO checks that the
// MemoryDreamRig prefab's DreamAudioBinder is present + has a non-empty
// cueMap.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using HearthboundHollow.Audio;
using HearthboundHollow.Cutscene;

namespace HearthboundHollow.EditorTools
{
    public static class Phase40_AudioDiagnostic
    {
        private const string MemoryDreamRigPath = "Assets/_Project/Prefabs/Cutscene/MemoryDreamRig.prefab";

        // Canonical music ids the Phase 38 chain expects.
        private static readonly string[] RequiredMusicIds = new[]
        {
            "main_theme", "scene_menu", "scene_lane", "scene_hollow",
            "scene_garden", "scene_cottage",
            "dream_doris_motif",
            "dream_margery_a", "dream_margery_b", "dream_margery_c",
            "dream_margery_d", "dream_margery_e",
        };

        // Canonical ambience ids.
        private static readonly string[] RequiredAmbienceIds = new[]
        {
            "scene_lane", "scene_hollow", "scene_garden", "scene_cottage",
            "kettle_steam", "dream_wind",
        };

        // Mumble VO characters.
        private static readonly string[] RequiredCharacters = new[]
        {
            "doris", "gerrold", "pickle", "marin",
        };

        // Dream-variant → music-cue map. Phase 38 wires these into the
        // DreamAudioBinder.cueMap; Phase 40 verifies the wiring landed.
        private static readonly (string variant, string music)[] RequiredDreamCueMap = new[]
        {
            ("Dream1_Doris", "dream_doris_motif"),
            ("Dream2_VariantA_EraseClean", "dream_margery_a"),
            ("Dream2_VariantB_CleansePartial", "dream_margery_b"),
            ("Dream2_VariantC_CrossedCore", "dream_margery_c"),
            ("Dream2_VariantD_Listen", "dream_margery_d"),
            ("Dream2_VariantE_Defer", "dream_margery_e"),
        };

        // ─── Menu ─────────────────────────────────────────────────

        [MenuItem("Hearthbound/⚙️ Advanced/🔊 Phase 40 — Diagnose Audio Wiring", priority = 992)]
        public static void Run()
        {
            var sb = new StringBuilder();
            sb.AppendLine("╔══════════════════════════════════════════════════════════════════╗");
            sb.AppendLine("║   Hearthbound Hollow · Phase 40 — Audio Wiring Diagnostic         ║");
            sb.AppendLine("╚══════════════════════════════════════════════════════════════════╝");
            sb.AppendLine();

            int errors = 0, warns = 0;

            DiagnoseLibrary<MusicLibrarySO>(sb,
                Phase37_ProceduralAudioStudio.MusicLibraryPath,
                "MusicLibrarySO", RequiredMusicIds, ref errors, ref warns);

            DiagnoseLibrary<MusicLibrarySO>(sb,
                Phase37_ProceduralAudioStudio.AmbienceLibraryPath,
                "AmbienceLibrarySO", RequiredAmbienceIds, ref errors, ref warns);

            DiagnoseMumbleLibrary(sb, ref errors, ref warns);

            DiagnoseDreamBinder(sb, ref errors, ref warns);

            DiagnoseSceneBeacons(sb, ref errors, ref warns);

            sb.AppendLine();
            sb.AppendLine("──────────────────────────────────────────────────────────────────");
            sb.AppendLine($"VERDICT: {errors} error(s), {warns} warning(s)");
            if (errors == 0 && warns == 0)
                sb.AppendLine("✅ Audio wiring is fully wired and ship-ready.");
            else if (errors == 0)
                sb.AppendLine("⚠️ Approved with notes — re-run Phase 37/38 if anything looks off.");
            else
                sb.AppendLine("❌ Run `Hearthbound → 🚀 Build Everything` (Phase 37+38 chained at steps 10-11).");
            sb.AppendLine("──────────────────────────────────────────────────────────────────");

            Debug.Log(sb.ToString());
            EditorUtility.DisplayDialog(
                "Phase 40 — Audio Wiring Diagnostic",
                $"{errors} error(s), {warns} warning(s).\n\n" +
                "Full report logged to the Unity Console.",
                "OK");
        }

        // ─── Library diagnostics ─────────────────────────────────

        private static void DiagnoseLibrary<T>(StringBuilder sb, string assetPath, string label,
                                                string[] requiredIds, ref int errors, ref int warns)
            where T : MusicLibrarySO
        {
            sb.AppendLine($"─── {label} ({assetPath}) ─────────────────────────");
            var lib = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (lib == null)
            {
                sb.AppendLine($"  ❌ {label} missing — run Phase 37 (Procedural Audio Studio).");
                errors++;
                sb.AppendLine();
                return;
            }

            // Build a present-id set
            var present = new HashSet<string>();
            int withClip = 0, emptyClip = 0;
            foreach (var entry in lib.entries)
            {
                if (string.IsNullOrEmpty(entry.id)) continue;
                present.Add(entry.id);
                if (entry.clip != null) withClip++; else emptyClip++;
            }

            sb.AppendLine($"  Entries: {lib.entries.Count} total, {withClip} with clip, {emptyClip} empty.");

            // Verify every required id is present + has a clip
            int missing = 0;
            foreach (var id in requiredIds)
            {
                if (!present.Contains(id))
                {
                    sb.AppendLine($"  ❌ Missing id: '{id}'");
                    missing++;
                    continue;
                }
                var clip = lib.Get(id, out _, out _, out _, out _);
                if (clip == null)
                {
                    sb.AppendLine($"  ⚠️ '{id}' present but clip is null (run Phase 37 to regenerate).");
                    warns++;
                }
                else
                {
                    sb.AppendLine($"  ✓ {id,-32} → {clip.name} ({clip.length:F1}s)");
                }
            }
            if (missing > 0)
            {
                sb.AppendLine($"  ❌ {missing} required id(s) missing — run Phase 37.");
                errors += missing;
            }
            sb.AppendLine();
        }

        private static void DiagnoseMumbleLibrary(StringBuilder sb, ref int errors, ref int warns)
        {
            sb.AppendLine($"─── MumbleVoiceLibrarySO ({Phase37_ProceduralAudioStudio.MumbleLibraryPath}) ──");
            var lib = AssetDatabase.LoadAssetAtPath<MumbleVoiceLibrarySO>(
                Phase37_ProceduralAudioStudio.MumbleLibraryPath);
            if (lib == null)
            {
                sb.AppendLine("  ❌ MumbleVoiceLibrary missing — run Phase 37.");
                errors++;
                sb.AppendLine();
                return;
            }
            sb.AppendLine($"  Banks: {lib.banks.Count} character(s).");
            foreach (var character in RequiredCharacters)
            {
                var bank = lib.GetBank(character);
                if (bank == null)
                {
                    sb.AppendLine($"  ❌ Missing character bank: '{character}'");
                    errors++;
                    continue;
                }
                int withClip = bank.phonemes.Count(p => p != null);
                int empty = bank.phonemes.Count - withClip;
                string status = empty == 0 ? "✓" : (withClip > 0 ? "⚠️" : "❌");
                sb.AppendLine($"  {status} {character,-10} {withClip}/{bank.phonemes.Count} phonemes, " +
                              $"rate {bank.syllableRate:F1} syl/s, vol {bank.volume:F2}");
                if (empty > 0)
                {
                    if (withClip == 0) errors++;
                    else warns += empty;
                }
            }
            sb.AppendLine();
        }

        private static void DiagnoseDreamBinder(StringBuilder sb, ref int errors, ref int warns)
        {
            sb.AppendLine($"─── DreamAudioBinder ({MemoryDreamRigPath}) ──");
            var rig = AssetDatabase.LoadAssetAtPath<GameObject>(MemoryDreamRigPath);
            if (rig == null)
            {
                sb.AppendLine("  ❌ MemoryDreamRig.prefab missing — run Phase 36.");
                errors++;
                sb.AppendLine();
                return;
            }
            var binder = rig.GetComponent<DreamAudioBinder>();
            if (binder == null)
            {
                sb.AppendLine("  ❌ DreamAudioBinder missing on rig — run Phase 38.");
                errors++;
                sb.AppendLine();
                return;
            }
            sb.AppendLine($"  Cue map: {binder.cueMap?.Count ?? 0} entries.");
            if (binder.cueMap == null || binder.cueMap.Count == 0)
            {
                sb.AppendLine("  ❌ Cue map empty — run Phase 38.");
                errors++;
                sb.AppendLine();
                return;
            }

            var present = new HashSet<string>(binder.cueMap.Select(m => m.variantId));
            foreach (var (variant, music) in RequiredDreamCueMap)
            {
                if (!present.Contains(variant))
                {
                    sb.AppendLine($"  ❌ Missing dream variant mapping: '{variant}' → '{music}'");
                    errors++;
                    continue;
                }
                var mapping = binder.cueMap.FirstOrDefault(m => m.variantId == variant);
                if (mapping.musicId != music)
                {
                    sb.AppendLine($"  ⚠️ '{variant}' mapped to '{mapping.musicId}' (expected '{music}')");
                    warns++;
                }
                else
                {
                    sb.AppendLine($"  ✓ {variant,-32} → {music}");
                }
            }

            if (binder.musicLibrary == null)
            {
                sb.AppendLine("  ❌ DreamAudioBinder.musicLibrary not wired — run Phase 38.");
                errors++;
            }
            sb.AppendLine();
        }

        private static void DiagnoseSceneBeacons(StringBuilder sb, ref int errors, ref int warns)
        {
            sb.AppendLine("─── Per-Scene Audio Beacons ───────────────────────");
            foreach (var (scenePath, musicId, ambienceId) in Phase38_AudioAndCutsceneWiring.SceneAudioMap)
            {
                if (!File.Exists(scenePath))
                {
                    sb.AppendLine($"  ❌ Scene missing: {scenePath}");
                    errors++;
                    continue;
                }
                // Scan the scene file as text for SceneAudioBeacon name (lightweight check).
                string contents = File.ReadAllText(scenePath);
                bool hasBeacon = contents.Contains("_HHAudio_SceneBeacon");
                bool hasMusicId = contents.Contains($"musicId: {musicId}");
                if (hasBeacon && hasMusicId)
                {
                    sb.AppendLine($"  ✓ {Path.GetFileNameWithoutExtension(scenePath),-30} music='{musicId}' ambience='{ambienceId}'");
                }
                else
                {
                    sb.AppendLine($"  ⚠️ {Path.GetFileNameWithoutExtension(scenePath)} — beacon present: {hasBeacon}, musicId match: {hasMusicId}");
                    warns++;
                }
            }
            sb.AppendLine();
        }

        // ─── Preview utility — plays one cue in the Editor ───────

        [MenuItem("Hearthbound/⚙️ Advanced/🔊 Phase 40 — Preview Doris Mumble", priority = 993)]
        public static void PreviewDorisMumble() => PreviewMumble("doris");

        [MenuItem("Hearthbound/⚙️ Advanced/🔊 Phase 40 — Preview Gerrold Mumble", priority = 994)]
        public static void PreviewGerroldMumble() => PreviewMumble("gerrold");

        [MenuItem("Hearthbound/⚙️ Advanced/🔊 Phase 40 — Preview Pickle Mumble", priority = 995)]
        public static void PreviewPickleMumble() => PreviewMumble("pickle");

        [MenuItem("Hearthbound/⚙️ Advanced/🔊 Phase 40 — Preview Marin Mumble", priority = 996)]
        public static void PreviewMarinMumble() => PreviewMumble("marin");

        private static void PreviewMumble(string character)
        {
            var lib = AssetDatabase.LoadAssetAtPath<MumbleVoiceLibrarySO>(
                Phase37_ProceduralAudioStudio.MumbleLibraryPath);
            if (lib == null)
            {
                EditorUtility.DisplayDialog("Preview Mumble",
                    "MumbleVoiceLibrary missing — run Phase 37 first.", "OK");
                return;
            }
            var bank = lib.GetBank(character);
            if (bank == null || bank.phonemes.Count == 0)
            {
                EditorUtility.DisplayDialog("Preview Mumble",
                    $"Bank for '{character}' missing or empty — run Phase 37.", "OK");
                return;
            }
            // Play a random phoneme via the editor's audio preview API.
            var rng = new System.Random();
            int idx = rng.Next(bank.phonemes.Count);
            var clip = bank.phonemes[idx];
            if (clip == null)
            {
                EditorUtility.DisplayDialog("Preview Mumble",
                    $"Phoneme {idx:D2} of '{character}' is null.", "OK");
                return;
            }
            PlayClipInEditor(clip);
            Debug.Log($"[Hearthbound/Phase 40] Previewing {character} phoneme {idx:D2} → {clip.name} " +
                      $"({clip.length * 1000:F0} ms)");
        }

        // Editor-only audio preview via reflection (Unity exposes the internal
        // PlayClip API on AudioUtil). Works in 2019.1+, including Unity 6.
        private static void PlayClipInEditor(AudioClip clip)
        {
            if (clip == null) return;
            var assembly = typeof(AudioImporter).Assembly;
            var audioUtil = assembly.GetType("UnityEditor.AudioUtil");
            if (audioUtil == null) return;
            // Try Unity 6 method first, then fall back to older signatures.
            var method = audioUtil.GetMethod("PlayPreviewClip",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public,
                null,
                new System.Type[] { typeof(AudioClip), typeof(int), typeof(bool) },
                null);
            if (method != null)
            {
                method.Invoke(null, new object[] { clip, 0, false });
                return;
            }
            method = audioUtil.GetMethod("PlayClip",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public,
                null,
                new System.Type[] { typeof(AudioClip) },
                null);
            if (method != null) method.Invoke(null, new object[] { clip });
        }
    }
}
