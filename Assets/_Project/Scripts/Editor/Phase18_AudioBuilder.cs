// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase18_AudioBuilder
//
// Phase 18 — Audio Integration.
//
// Scans the Game UI & Puzzle Sound Effects Pack and auto-populates an
// SfxLibrarySO with the 9 polish cues + ambient + UI clicks.
//
// USE: Menu → Hearthbound → Phase 18 — Build SFX Library
//
// Output: Assets/_Project/Audio/SfxLibrary.asset (the catalog the runtime
// SfxPlayer reads to translate event IDs into AudioClips).
//
// The SceneBuilder in Phase 22 spawns an SfxPlayer GameObject per playable
// scene and wires its `library` field to this asset.

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using HearthboundHollow.Audio;

namespace HearthboundHollow.EditorTools
{
    public static class Phase18_AudioBuilder
    {
        private const string SfxRoot = "Assets/Game UI & Puzzle Sound Effects Pack";
        private const string LibraryDir = "Assets/_Project/Audio";
        private const string LibraryPath = LibraryDir + "/SfxLibrary.asset";

        // Map from canonical event ID → search keywords (in order of preference).
        private static readonly Dictionary<string, string[]> CueKeywords = new()
        {
            ["polish_hum_start"]          = new[] { "hum_start", "hum_pickup", "warm_start", "pickup_hum" },
            ["polish_hum_loop"]           = new[] { "hum_loop", "hum_idle", "warm_loop", "idle_hum" },
            ["polish_rub_start"]          = new[] { "rub_start", "polish_start", "stroke", "swipe_start" },
            ["polish_rub_loop"]           = new[] { "rub_loop", "polish_loop", "stroke_loop", "swipe_loop" },
            ["polish_rub_friction_warn"]  = new[] { "friction", "scratch", "too_fast", "warn_soft" },
            ["polish_midway_chime"]       = new[] { "chime", "midway", "milestone", "bell_soft" },
            ["polish_reveal_swell"]       = new[] { "reveal", "swell", "rise", "shine", "discover" },
            ["polish_success_jingle"]     = new[] { "success", "complete", "achievement", "jingle_win" },
            ["polish_hum_post"]           = new[] { "hum_post", "hum_after", "settle", "hum_calm" },

            ["ui_click"]                  = new[] { "ui_click", "button_click", "click_soft", "tap" },
            ["ui_hover"]                  = new[] { "ui_hover", "hover", "highlight" },
            ["ui_open"]                   = new[] { "ui_open", "open", "panel_in" },
            ["ui_close"]                  = new[] { "ui_close", "close", "panel_out" },

            ["dialogue_advance"]          = new[] { "dialogue", "page_turn", "scroll" },

            ["ambient_autumn_loop"]       = new[] { "ambient", "autumn", "wind_soft", "forest_loop" },
            ["choice_select"]             = new[] { "choice", "select", "confirm_soft" },
        };

        [MenuItem("Hearthbound/Phase 18 — Build SFX Library", priority = 205)]
        public static void Build()
        {
            EnsureFolder(LibraryDir);

            var lib = AssetDatabase.LoadAssetAtPath<SfxLibrarySO>(LibraryPath);
            if (lib == null)
            {
                lib = ScriptableObject.CreateInstance<SfxLibrarySO>();
                AssetDatabase.CreateAsset(lib, LibraryPath);
            }

            if (!AssetDatabase.IsValidFolder(SfxRoot))
            {
                Debug.LogWarning($"[Hearthbound/Phase 18] {SfxRoot} not found. Library created empty — drop AudioClips manually.");
                EditorUtility.SetDirty(lib);
                AssetDatabase.SaveAssets();
                return;
            }

            // Build / refresh entries.
            var existingById = new Dictionary<string, SfxLibrarySO.Entry>();
            foreach (var e in lib.entries) existingById[e.id] = e;
            lib.entries.Clear();

            int matched = 0, missing = 0;
            foreach (var kvp in CueKeywords)
            {
                var clip = FindAudioClip(kvp.Value, out var pickedPath);
                if (clip != null)
                {
                    bool isLoop = kvp.Key.Contains("_loop") || kvp.Key.Contains("ambient");
                    float vol = DefaultVolumeFor(kvp.Key);
                    lib.entries.Add(new SfxLibrarySO.Entry { id = kvp.Key, clip = clip, volume = vol, loop = isLoop });
                    Debug.Log($"[Hearthbound/Phase 18] '{kvp.Key}' → {pickedPath}");
                    matched++;
                }
                else
                {
                    // Preserve user override if it was already in the library
                    if (existingById.TryGetValue(kvp.Key, out var prior) && prior.clip != null)
                    {
                        lib.entries.Add(prior);
                        Debug.Log($"[Hearthbound/Phase 18] '{kvp.Key}' → kept manual override ({prior.clip.name})");
                        matched++;
                    }
                    else
                    {
                        lib.entries.Add(new SfxLibrarySO.Entry { id = kvp.Key, clip = null, volume = DefaultVolumeFor(kvp.Key), loop = kvp.Key.Contains("_loop") });
                        Debug.LogWarning($"[Hearthbound/Phase 18] '{kvp.Key}' — no match. Drop a clip manually onto this entry in the library asset.");
                        missing++;
                    }
                }
            }

            EditorUtility.SetDirty(lib);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Phase 18 — Done",
                $"SfxLibrary created at {LibraryPath}\n\n" +
                $"Matched {matched} of {CueKeywords.Count} cues.\n" +
                $"{missing} entries empty (logged warnings) — drop clips manually onto those entries if needed.\n\n" +
                "Re-run 'Hearthbound → Build Playable Mission 1 (One Click)' — the scene builder will spawn an " +
                "SfxPlayer + PolishAudioBinder and wire them to this library.",
                "OK");
        }

        // ─── Detection helpers ────────────────────────────────────

        private static AudioClip FindAudioClip(string[] keywords, out string pickedPath)
        {
            pickedPath = null;
            var guids = AssetDatabase.FindAssets("t:AudioClip", new[] { SfxRoot });
            AudioClip best = null;
            int bestScore = 0;
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip == null) continue;
                int score = 0;
                var lowerPath = path.ToLowerInvariant();
                var lowerName = clip.name.ToLowerInvariant();
                for (int i = 0; i < keywords.Length; i++)
                {
                    var kw = keywords[i].ToLowerInvariant();
                    if (lowerName.Contains(kw)) score += (keywords.Length - i) * 10 + 18;
                    else if (lowerPath.Contains(kw)) score += (keywords.Length - i) * 4 + 6;
                }
                if (score > bestScore) { best = clip; bestScore = score; pickedPath = path; }
            }
            return best;
        }

        private static float DefaultVolumeFor(string id)
        {
            if (id.Contains("loop")) return 0.35f;
            if (id.Contains("ambient")) return 0.25f;
            if (id.Contains("friction")) return 0.5f;
            if (id.Contains("chime") || id.Contains("jingle") || id.Contains("swell")) return 0.65f;
            return 0.55f;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            var leaf = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        public static SfxLibrarySO TryGetLibrary() =>
            AssetDatabase.LoadAssetAtPath<SfxLibrarySO>(LibraryPath);
    }
}
