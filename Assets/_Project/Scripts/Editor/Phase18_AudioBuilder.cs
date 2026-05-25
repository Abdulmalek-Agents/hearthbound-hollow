// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase18_AudioBuilder
//
// Phase 18 — Audio infrastructure (SFX library + AudioMixer scaffolding).
//
// Builds `SfxLibrary.asset` — a ScriptableObject that maps event IDs
// (Polish_Rub, Cleanse_Pulse, Orb_Pickup, …) to the actual AudioClips from
// "Game UI & Puzzle Sound Effects Pack". HearthboundOneClickSetup spawns
// an SfxPlayer prefab that reads this library and plays clips on demand.
//
// USE: Menu → Hearthbound → Phase 18 — Build SFX Library

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HearthboundHollow.EditorTools
{
    public class SfxLibrary : ScriptableObject
    {
        [System.Serializable]
        public class Entry
        {
            public string id;            // canonical event ID (e.g. "Polish_Rub")
            public AudioClip clip;       // looked up by name/path
            [Range(0f, 1f)] public float volume = 1f;
        }

        public List<Entry> entries = new List<Entry>();
    }

    public static class Phase18_AudioBuilder
    {
        // Where the Game UI & Puzzle SFX pack typically installs.
        private const string SfxPackRoot = "Assets/Game UI & Puzzle Sound Effects Pack";
        private const string LibraryDir = "Assets/_Project/ScriptableObjects/Audio";
        private const string LibraryPath = LibraryDir + "/SfxLibrary.asset";

        // Canonical event IDs the runtime references.
        private static readonly (string id, string[] nameKeywords)[] EventMap =
        {
            ("Orb_Pickup",      new[] { "pickup", "select", "collect", "gem" }),
            ("Orb_Place",       new[] { "drop", "place", "thud_soft" }),
            ("Polish_RubLoop",  new[] { "rub", "polish", "scrub", "wipe" }),
            ("Polish_Reveal",   new[] { "reveal", "sparkle", "chime", "glow" }),
            ("Polish_Success",  new[] { "success", "complete", "win", "fanfare" }),
            ("Cleanse_Pulse",   new[] { "pulse", "heart", "thump", "beat" }),
            ("Cleanse_Hit",     new[] { "match", "click_soft", "tap_soft" }),
            ("Cleanse_Crack",   new[] { "crack", "break", "snap" }),
            ("UI_Click",        new[] { "click", "tap", "button" }),
            ("UI_Hover",        new[] { "hover", "select_soft" }),
            ("UI_Confirm",      new[] { "confirm", "accept", "approve" }),
            ("UI_Cancel",       new[] { "cancel", "back", "deny" }),
            ("Dialogue_Open",   new[] { "open", "scroll", "page" }),
            ("Dialogue_Close",  new[] { "close", "fold", "tuck" }),
            ("Ledger_Stamp",    new[] { "stamp", "thunk", "punch" }),
            ("Pickle_Purr",     new[] { "purr", "cat", "meow" }),
            ("Garden_Harvest",  new[] { "pluck", "snip", "harvest" }),
            ("Tea_Brew",        new[] { "pour", "tea", "kettle", "bubble" }),
        };

        [MenuItem("Hearthbound/⚙️ Advanced/Phase 18 — Build SFX Library", priority = 205)]
        public static void Build()
        {
            EnsureFolder(LibraryDir);

            var lib = AssetDatabase.LoadAssetAtPath<SfxLibrary>(LibraryPath);
            if (lib == null)
            {
                lib = ScriptableObject.CreateInstance<SfxLibrary>();
                AssetDatabase.CreateAsset(lib, LibraryPath);
            }
            lib.entries.Clear();

            string[] roots = AssetDatabase.IsValidFolder(SfxPackRoot)
                ? new[] { SfxPackRoot }
                : new[] { "Assets" };

            int found = 0;
            foreach (var (id, keywords) in EventMap)
            {
                var clip = FindClip(roots, keywords);
                lib.entries.Add(new SfxLibrary.Entry { id = id, clip = clip, volume = 1f });
                if (clip != null) found++;
            }

            EditorUtility.SetDirty(lib);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Phase 18 — Done",
                $"SFX library written: {LibraryPath}\n\n" +
                $"Matched {found} / {EventMap.Length} event IDs to clips.\n\n" +
                "Empty entries can be manually populated by opening SfxLibrary.asset and " +
                "dragging clips into the slots.\n\n" +
                "Re-run 'Hearthbound → Build Playable Mission 1 (One Click)' to spawn the SfxPlayer.",
                "OK");
        }

        private static AudioClip FindClip(string[] roots, string[] nameKeywords)
        {
            var guids = AssetDatabase.FindAssets("t:AudioClip", roots);
            AudioClip best = null;
            int bestScore = 0;
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip == null) continue;
                var lower = (path + "/" + clip.name).ToLowerInvariant();
                int score = 0;
                foreach (var kw in nameKeywords)
                {
                    if (lower.Contains(kw)) score += 20;
                }
                if (score > bestScore) { best = clip; bestScore = score; }
            }
            return best;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            var leaf = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        public static SfxLibrary TryGetLibrary() =>
            AssetDatabase.LoadAssetAtPath<SfxLibrary>(LibraryPath);
    }
}
