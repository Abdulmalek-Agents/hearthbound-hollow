// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase45_LibraryMigrator
//
// Phase 45 — library migrator + dialogue-sound hotfix support.
//
// One-shot Editor menu that migrates the audio libraries from the
// pre-Phase-45 location:
//   Assets/_Project/Audio/MusicLibrary.asset
//   Assets/_Project/Audio/AmbienceLibrary.asset
//   Assets/_Project/Audio/MumbleVoiceLibrary.asset
// to the new Resources/ location:
//   Assets/_Project/Audio/Resources/MusicLibrary.asset
//   Assets/_Project/Audio/Resources/AmbienceLibrary.asset
//   Assets/_Project/Audio/Resources/MumbleVoiceLibrary.asset
//
// Why? At runtime, `Resources.Load` resolves any asset placed under a
// Resources/ folder. The runtime audio rig (`RuntimeAudioBootstrap` +
// `MumbleVoicePlayer.Awake` + `MusicPlayer.Awake` self-heal paths)
// uses `Resources.Load` to find the libraries when the Inspector
// reference is null (e.g. on a fresh clone where Phase 38's scene
// wiring hasn't run yet).
//
// `AssetDatabase.MoveAsset` preserves the asset's GUID, so any
// scene-side serialized reference into the library survives the move.
// The user can re-run this any time; it's idempotent (no-op if the
// asset already lives at the new path).
//
// Chained from Phase 27 Build Everything (after Phase 37, before
// Phase 38) so every `🚀 Build Everything` click also performs the
// migration if needed.
//
// USE: Menu → Hearthbound → ⚙️ Advanced → 🩹 Phase 45 — Migrate Audio Libraries to Resources

using UnityEditor;
using UnityEngine;
using HearthboundHollow.Audio;

namespace HearthboundHollow.EditorTools
{
    public static class Phase45_LibraryMigrator
    {
        // Canonical destination — Resources-folder paths. These match the
        // const filenames in MumbleVoicePlayer / MusicPlayer for the
        // Resources.Load fallbacks.
        public const string ResourcesFolder        = "Assets/_Project/Audio/Resources";
        public const string MusicLibraryDest       = ResourcesFolder + "/MusicLibrary.asset";
        public const string AmbienceLibraryDest    = ResourcesFolder + "/AmbienceLibrary.asset";
        public const string MumbleLibraryDest      = ResourcesFolder + "/MumbleVoiceLibrary.asset";

        // Legacy source paths (pre-Phase-45).
        public const string MusicLibraryLegacy     = "Assets/_Project/Audio/MusicLibrary.asset";
        public const string AmbienceLibraryLegacy  = "Assets/_Project/Audio/AmbienceLibrary.asset";
        public const string MumbleLibraryLegacy    = "Assets/_Project/Audio/MumbleVoiceLibrary.asset";

        [MenuItem("Hearthbound/⚙️ Advanced/🩹 Phase 45 — Migrate Audio Libraries to Resources", priority = 224)]
        public static void Run()
        {
            int moved = 0, alreadyAtDest = 0, missing = 0, failed = 0;

            EnsureFolder(ResourcesFolder);

            var rows = new (string legacy, string dest, string label)[]
            {
                (MusicLibraryLegacy,    MusicLibraryDest,    "MusicLibrary"),
                (AmbienceLibraryLegacy, AmbienceLibraryDest, "AmbienceLibrary"),
                (MumbleLibraryLegacy,   MumbleLibraryDest,   "MumbleVoiceLibrary"),
            };

            foreach (var (legacy, dest, label) in rows)
            {
                bool destExists = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(dest) != null;
                bool legacyExists = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(legacy) != null;

                if (destExists)
                {
                    // Already migrated. If a legacy file ALSO exists, it's
                    // a stray duplicate (created by an unconditional Phase 37
                    // re-run); delete it to keep the asset tree clean.
                    if (legacyExists)
                    {
                        if (AssetDatabase.DeleteAsset(legacy))
                        {
                            Debug.Log($"[Hearthbound/Phase 45] Removed stray duplicate {legacy} (canonical lives at {dest}).");
                        }
                    }
                    alreadyAtDest++;
                    continue;
                }

                if (!legacyExists)
                {
                    Debug.Log($"[Hearthbound/Phase 45] {label}: nothing to migrate (no asset at {legacy}).");
                    missing++;
                    continue;
                }

                string err = AssetDatabase.MoveAsset(legacy, dest);
                if (string.IsNullOrEmpty(err))
                {
                    Debug.Log($"[Hearthbound/Phase 45] ✓ Migrated {label}: {legacy} → {dest} (GUID preserved).");
                    moved++;
                }
                else
                {
                    Debug.LogWarning($"[Hearthbound/Phase 45] ✗ Could not migrate {label} ({legacy} → {dest}): {err}");
                    failed++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // The verdict — visible to the human even if they didn't watch
            // the Console.
            string summary =
                $"Phase 45 — Library Migration\n\n" +
                $"  {moved}  moved to Resources/\n" +
                $"  {alreadyAtDest}  already at destination (no-op)\n" +
                $"  {missing}  not present (no migration needed)\n" +
                $"  {failed}  failed (see Console for details)\n\n" +
                $"After migration, the audio rig's Resources.Load fallback\n" +
                $"in MumbleVoicePlayer.Awake() + MusicPlayer.Awake() will\n" +
                $"resolve the libraries even when no scene-side wiring is\n" +
                $"present. Dialogue mumble + music are now self-healing.";
            Debug.Log("[Hearthbound/Phase 45] " + summary);

            EditorUtility.DisplayDialog("Phase 45 — Library Migration", summary, "OK");
        }

        /// <summary>
        /// Silent version invoked from Phase 27 Build Everything — no
        /// confirmation dialog. Returns true if anything was migrated.
        /// </summary>
        public static bool RunSilent()
        {
            int moved = 0;
            EnsureFolder(ResourcesFolder);
            var rows = new (string legacy, string dest)[]
            {
                (MusicLibraryLegacy,    MusicLibraryDest),
                (AmbienceLibraryLegacy, AmbienceLibraryDest),
                (MumbleLibraryLegacy,   MumbleLibraryDest),
            };
            foreach (var (legacy, dest) in rows)
            {
                if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(dest) != null)
                {
                    if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(legacy) != null)
                        AssetDatabase.DeleteAsset(legacy);
                    continue;
                }
                if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(legacy) == null) continue;
                if (string.IsNullOrEmpty(AssetDatabase.MoveAsset(legacy, dest))) moved++;
            }
            if (moved > 0) AssetDatabase.SaveAssets();
            return moved > 0;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            var leaf = System.IO.Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
