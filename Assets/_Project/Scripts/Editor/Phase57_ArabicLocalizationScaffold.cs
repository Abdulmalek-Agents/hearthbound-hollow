// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase57_ArabicLocalizationScaffold
//
// PHASE 57 (D-074) — scaffolding for Arabic DIALOGUE + VOICE (plumbing, not content).
//
// Per Pillar 1 / D-065 the Arabic dialogue prose + VO are a HUMAN pass. This
// builder creates the empty drop-in targets so that content "just works" the
// moment a translator / VO actor provides it — and ships everything English
// until then:
//
//   • Resources/DialogueLocalization_ar.asset  (DialogueLocalizationSO) — an empty
//       lineId → Arabic-text table. A translator fills entries; DialogueUI shows
//       the shaped Arabic for a line when present, else the canonical English.
//   • Resources/HearthboundVoiceLibrary_ar.asset (VoiceLibrarySO) — built by
//       scanning Assets/_Project/Audio/Voice_ar/<Character>/<lineId>.wav (a sibling
//       of the English Audio/Voice/ root, so the English scan never picks it up).
//       VoicePlayer plays the Arabic clip when Arabic is active + present, else
//       falls back to the English clip.
//
// Idempotent · auto-runs on editor load when missing · never breaks the build.
// Menu: Hearthbound → ⚙️ Advanced → Phase 57.

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using HearthboundHollow.Core;
using HearthboundHollow.Audio;

namespace HearthboundHollow.EditorTools
{
    public static class Phase57_ArabicLocalizationScaffold
    {
        private const string ResourcesDir = "Assets/_Project/Resources";
        private const string DialoguePath = ResourcesDir + "/DialogueLocalization_ar.asset";
        private const string VoiceArRoot  = "Assets/_Project/Audio/Voice_ar";
        private const string VoiceArPath  = ResourcesDir + "/HearthboundVoiceLibrary_ar.asset";

        [MenuItem("Hearthbound/⚙️ Advanced/Phase 57 — Scaffold Arabic Dialogue + Voice")]
        public static void ScaffoldMenu() => Scaffold(verbose: true);

        [InitializeOnLoadMethod]
        private static void AutoScaffoldIfMissing()
        {
            EditorApplication.delayCall += () =>
            {
                try
                {
                    if (AssetDatabase.LoadAssetAtPath<DialogueLocalizationSO>(DialoguePath) == null ||
                        AssetDatabase.LoadAssetAtPath<VoiceLibrarySO>(VoiceArPath) == null)
                        Scaffold(verbose: false);
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[Phase57] Arabic scaffold skipped (non-fatal): {ex.Message}");
                }
            };
        }

        /// <summary>Idempotent. Creates/refreshes the empty Arabic drop-in targets.</summary>
        public static void Scaffold(bool verbose)
        {
            try
            {
                EnsureResourcesDir();

                // 1) Empty dialogue Arabic table (a human translator fills it).
                var table = AssetDatabase.LoadAssetAtPath<DialogueLocalizationSO>(DialoguePath);
                if (table == null)
                {
                    table = ScriptableObject.CreateInstance<DialogueLocalizationSO>();
                    table.entries = new DialogueLocalizationSO.Entry[0];
                    AssetDatabase.CreateAsset(table, DialoguePath);
                }

                // 2) Arabic voice library, scanned from Audio/Voice_ar/ (empty if absent).
                int clips = BuildArabicVoiceLibrary();

                AssetDatabase.SaveAssets();

                if (verbose)
                    EditorUtility.DisplayDialog("Phase 57 — Arabic localization scaffold",
                        $"Dialogue table: {DialoguePath}\n" +
                        $"Voice library:  {VoiceArPath}  ({clips} Arabic clip(s) found)\n\n" +
                        "To localize dialogue + voice (human pass, per Pillar 1 / D-065):\n" +
                        "  • Fill the dialogue table entries (lineId → Arabic text).\n" +
                        "  • Drop Arabic .wav at Audio/Voice_ar/<Character>/<lineId>.wav.\n" +
                        "  • Re-run this. Everything stays English until those exist.", "OK");
                else
                    Debug.Log($"[Phase57] Arabic scaffold ready — dialogue table + voice library " +
                              $"({clips} AR clip(s)). Fill them via a human translation/VO pass.");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[Phase57] Arabic scaffold failed (non-fatal): {ex.Message}");
            }
        }

        // Scan Audio/Voice_ar/ for .wav and (re)build the Arabic VoiceLibrarySO,
        // preserving any inspector-tuned volume/pitch. Mirrors Phase32's English
        // scan; reuses its per-character casting defaults. Returns the clip count.
        private static int BuildArabicVoiceLibrary()
        {
            var lib = AssetDatabase.LoadAssetAtPath<VoiceLibrarySO>(VoiceArPath);
            if (lib == null)
            {
                lib = ScriptableObject.CreateInstance<VoiceLibrarySO>();
                AssetDatabase.CreateAsset(lib, VoiceArPath);
            }

            var existing = new Dictionary<string, VoiceLibrarySO.Entry>();
            if (lib.entries != null)
                foreach (var e in lib.entries)
                    if (!string.IsNullOrEmpty(e.lineId)) existing[e.lineId] = e;

            var entries = new List<VoiceLibrarySO.Entry>();
            if (Directory.Exists(VoiceArRoot))
            {
                var wavs = Directory.GetFiles(VoiceArRoot, "*.wav", SearchOption.AllDirectories);
                System.Array.Sort(wavs);
                foreach (var fs in wavs)
                {
                    var assetPath = fs.Replace('\\', '/');
                    int idx = assetPath.IndexOf("Assets/");
                    if (idx > 0) assetPath = assetPath.Substring(idx);

                    var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
                    if (clip == null) continue; // not imported yet — skip silently

                    string lineId = Path.GetFileNameWithoutExtension(assetPath);
                    string character = Path.GetFileName(Path.GetDirectoryName(assetPath));

                    if (existing.TryGetValue(lineId, out var prev))
                    {
                        prev.clip = clip;
                        entries.Add(prev);
                    }
                    else
                    {
                        var (vol, pitch) = Phase32_VoiceLibraryBuilder.GetCastingDefaults(character);
                        entries.Add(new VoiceLibrarySO.Entry
                        {
                            lineId = lineId,
                            clip   = clip,
                            volume = vol,
                            pitch  = pitch,
                        });
                    }
                }
            }

            lib.entries = entries;   // VoiceLibrarySO.entries is List<Entry> (not an array)
            EditorUtility.SetDirty(lib);
            return entries.Count;
        }

        private static void EnsureResourcesDir()
        {
            if (AssetDatabase.IsValidFolder(ResourcesDir)) return;
            if (!AssetDatabase.IsValidFolder("Assets/_Project"))
                AssetDatabase.CreateFolder("Assets", "_Project");
            AssetDatabase.CreateFolder("Assets/_Project", "Resources");
        }
    }
}
