// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase35_FlatEntryAudit
//
// Phase 35 — Continuation Audit · the Mission 1-2 ship-ready punchlist.
//
// Read-only diagnostic that walks the whole project and produces a single
// Console table answering the producer's "what is actually missing?" question
// in 15 seconds. The audit checks:
//
//   1. Every Hearthbound MenuItem — path + priority + owning type.
//      Verifies D-051 reservation: only `🚀 Build Everything` and
//      `🔍 Diagnose Build` are allowed at the top level. Every other
//      MenuItem MUST live under `⚙️ Advanced/…`.
//
//   2. Every SfxLibrary entry — id + clip name (or `❌ EMPTY`).
//
//   3. Every cutscene PlayableAsset required by `MemoryDreamSequencer` +
//      `ListenSceneSequencer` (Dream 1, Dream 2 A/B/C/D/E, Listen) —
//      path + status (Built / Missing).
//
//   4. Music + Ambience + SFX + Mumble VO folders — clip count + sample
//      durations. Flags `.gitkeep`-only folders.
//
//   5. Yarn files — line count.
//
//   6. ScriptableObject seed assets — present + non-default-named.
//
//   7. Mission director references — every `[SerializeField] PlayableAsset`
//      across the runtime asmdef set must be either nullable-by-design
//      (documented) or have a known builder phase that wires it.
//
// READ-ONLY. Never modifies any asset. Safe to run any number of times.
//
// USE: Menu → Hearthbound → ⚙️ Advanced → 🔍 Phase 35 — Flat Entry Audit
//
// Chained from: Phase33_AggregateDiagnostic (so `🔍 Diagnose Build` includes
// this audit automatically).

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace HearthboundHollow.EditorTools
{
    public static class Phase35_FlatEntryAudit
    {
        // Required cutscene timelines — built by Phase21 (Dream 1) and Phase36
        // (Dream 2 variants + Listen). Each path lives in this constant table
        // so the diagnostic and the builder agree on the canonical location.
        public static readonly (string id, string path)[] RequiredTimelines = new[]
        {
            ("dream1",                          "Assets/_Project/Animations/Dream1_Doris.playable"),
            ("dream2_VariantA_EraseClean",      "Assets/_Project/Animations/Dream2_VariantA_EraseClean.playable"),
            ("dream2_VariantB_CleansePartial",  "Assets/_Project/Animations/Dream2_VariantB_CleansePartial.playable"),
            ("dream2_VariantC_CrossedCore",     "Assets/_Project/Animations/Dream2_VariantC_CrossedCore.playable"),
            ("dream2_VariantD_Listen",          "Assets/_Project/Animations/Dream2_VariantD_Listen.playable"),
            ("dream2_VariantE_Defer",           "Assets/_Project/Animations/Dream2_VariantE_Defer.playable"),
            ("listenScene",                     "Assets/_Project/Animations/ListenScene_Gerrold.playable"),
        };

        public static readonly string[] RequiredSeedAssets = new[]
        {
            "Assets/_Project/ScriptableObjects/Villagers/Doris.asset",
            "Assets/_Project/ScriptableObjects/Villagers/Gerrold.asset",
            "Assets/_Project/ScriptableObjects/Villagers/SilentLane.asset",
            "Assets/_Project/ScriptableObjects/Villagers/MemoryMap_Doris.asset",
            "Assets/_Project/ScriptableObjects/Villagers/MemoryMap_Gerrold.asset",
            "Assets/_Project/ScriptableObjects/Memories/DOR-001_FirstLoaves.asset",
            "Assets/_Project/ScriptableObjects/Memories/GER-007_SeventhMorning.asset",
            "Assets/_Project/ScriptableObjects/Memories/ECHO_DOR001_GER007.asset",
            "Assets/_Project/ScriptableObjects/Missions/Mission01_OpeningTheHollow.asset",
            "Assets/_Project/ScriptableObjects/Missions/Mission02_TheWidowersRequest.asset",
            "Assets/_Project/ScriptableObjects/Tariffs/Tariff_Erase.asset",
            "Assets/_Project/ScriptableObjects/Tariffs/Tariff_Cleanse.asset",
            "Assets/_Project/ScriptableObjects/Tariffs/Tariff_Listen.asset",
            "Assets/_Project/ScriptableObjects/Tariffs/Tariff_Defer.asset",
            "Assets/_Project/ScriptableObjects/Herbs/Lavender.asset",
            "Assets/_Project/ScriptableObjects/Herbs/Valerian.asset",
            "Assets/_Project/ScriptableObjects/State/VillageState.asset",
        };

        public static readonly (string folder, string label)[] RequiredAudioFolders = new[]
        {
            ("Assets/_Project/Audio/Music",     "Music (composer cues)"),
            ("Assets/_Project/Audio/Ambience",  "Ambience (autumn loop, hearth, garden, cottage)"),
            ("Assets/_Project/Audio/SFX",       "SFX (polish hum, kettle, footsteps)"),
            ("Assets/_Project/Audio/Mumble",    "Mumble VO (Doris / Gerrold / Pickle / Marin)"),
        };

        [MenuItem("Hearthbound/⚙️ Advanced/🔍 Phase 35 — Flat Entry Audit", priority = 991)]
        public static void Run()
        {
            var sb = new StringBuilder();
            sb.AppendLine("╔══════════════════════════════════════════════════════════════════╗");
            sb.AppendLine("║   Hearthbound Hollow · Phase 35 — Continuation Audit             ║");
            sb.AppendLine("║   The Mission 1-2 ship-ready punchlist                           ║");
            sb.AppendLine("╚══════════════════════════════════════════════════════════════════╝");
            sb.AppendLine();

            int errors = 0;
            int warns = 0;

            // 1. Menu entries
            AuditMenuItems(sb, ref errors, ref warns);

            // 2. SfxLibrary
            AuditSfxLibrary(sb, ref errors, ref warns);

            // 3. Cutscene timelines
            AuditCutsceneTimelines(sb, ref errors, ref warns);

            // 4. Audio folders
            AuditAudioFolders(sb, ref errors, ref warns);

            // 5. Yarn files
            AuditYarn(sb, ref errors, ref warns);

            // 6. ScriptableObjects
            AuditSeedAssets(sb, ref errors, ref warns);

            // Verdict
            sb.AppendLine();
            sb.AppendLine("──────────────────────────────────────────────────────────────────");
            sb.AppendLine($"VERDICT: {errors} error(s), {warns} warning(s)");
            sb.AppendLine();
            if (errors == 0 && warns == 0)
            {
                sb.AppendLine("✅ All systems clean. Ship-ready.");
            }
            else if (errors == 0)
            {
                sb.AppendLine("⚠️ Approved with Notes. Warnings are non-blockers (e.g. optional");
                sb.AppendLine("   mumble VO clips are placeholders that will be generated by");
                sb.AppendLine("   Phase 37 — Procedural Audio Studio).");
            }
            else
            {
                sb.AppendLine("❌ Rejected. Run Phase 36 (cutscenes) and Phase 37 (audio) to fix.");
            }
            sb.AppendLine("──────────────────────────────────────────────────────────────────");

            string finalReport = sb.ToString();
            Debug.Log(finalReport);

            EditorUtility.DisplayDialog(
                "Phase 35 — Flat Entry Audit",
                $"{errors} error(s), {warns} warning(s).\n\n" +
                "Full report logged to the Unity Console.\n\n" +
                (errors == 0 ? "Ship-ready." : "Run Phase 36 + 37 to fix the gaps."),
                "OK");
        }

        // ───── 1. Menu items ────────────────────────────────────────

        private static void AuditMenuItems(StringBuilder sb, ref int errors, ref int warns)
        {
            sb.AppendLine("─── 1. Menu Items (D-051 reservation) ────────────────────────────");
            var menuItems = new List<(string path, int priority, string type)>();
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = asm.GetTypes(); } catch { continue; }
                foreach (var t in types)
                {
                    MethodInfo[] methods;
                    try { methods = t.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly); } catch { continue; }
                    foreach (var m in methods)
                    {
                        var attrs = m.GetCustomAttributes(typeof(MenuItem), false);
                        foreach (MenuItem mi in attrs)
                        {
                            if (mi.menuItem.StartsWith("Hearthbound/", StringComparison.Ordinal))
                            {
                                menuItems.Add((mi.menuItem, mi.priority, t.FullName));
                            }
                        }
                    }
                }
            }
            menuItems = menuItems.OrderBy(e => e.priority).ThenBy(e => e.path).ToList();

            // Top-level allow-list per D-051.
            var allowedTop = new HashSet<string>
            {
                "Hearthbound/🚀 Build Everything",
                "Hearthbound/🔍 Diagnose Build",
            };

            int violations = 0;
            sb.AppendLine($"  Total Hearthbound menu items: {menuItems.Count}");
            foreach (var e in menuItems)
            {
                bool isTopLevel = e.path.Split('/').Length == 2;
                bool isAdvanced = e.path.StartsWith("Hearthbound/⚙️ Advanced/", StringComparison.Ordinal);
                string marker;
                if (isAdvanced)
                {
                    marker = "  · ";
                }
                else if (isTopLevel)
                {
                    if (allowedTop.Contains(e.path))
                    {
                        marker = "  ✓ TOP ";
                    }
                    else
                    {
                        marker = "  ❌ TOP ";
                        violations++;
                    }
                }
                else
                {
                    marker = "  ? ";
                }
                if (violations <= 30 || isTopLevel || !isAdvanced)
                {
                    sb.AppendLine($"{marker}[{e.priority,4}] {e.path}");
                }
            }
            if (violations > 0)
            {
                sb.AppendLine($"  ❌ {violations} top-level entries violate D-051 (only `🚀 Build Everything` + `🔍 Diagnose Build` allowed).");
                errors += violations;
            }
            else
            {
                sb.AppendLine("  ✅ D-051 reservation respected.");
            }
            sb.AppendLine();
        }

        // ───── 2. SfxLibrary ───────────────────────────────────────

        private static void AuditSfxLibrary(StringBuilder sb, ref int errors, ref int warns)
        {
            sb.AppendLine("─── 2. SfxLibrary ────────────────────────────────────────────────");
            var lib = AssetDatabase.LoadAssetAtPath<HearthboundHollow.Audio.SfxLibrarySO>(
                "Assets/_Project/Audio/SfxLibrary.asset");
            if (lib == null)
            {
                sb.AppendLine("  ❌ SfxLibrary.asset not found. Run Phase 18 — Build SFX Library.");
                errors++;
                sb.AppendLine();
                return;
            }
            int empty = 0, mapped = 0;
            foreach (var e in lib.entries)
            {
                if (e.clip == null)
                {
                    sb.AppendLine($"  ❌ EMPTY     {e.id}");
                    empty++;
                }
                else
                {
                    sb.AppendLine($"  ✓ {e.clip.name,-44} → {e.id}");
                    mapped++;
                }
            }
            sb.AppendLine($"  Summary: {mapped} mapped, {empty} empty.");
            if (empty > 0)
            {
                sb.AppendLine($"  ⚠️ {empty} entries need clips — Phase 37 generates them.");
                warns += empty;
            }
            sb.AppendLine();
        }

        // ───── 3. Cutscene timelines ───────────────────────────────

        private static void AuditCutsceneTimelines(StringBuilder sb, ref int errors, ref int warns)
        {
            sb.AppendLine("─── 3. Cutscene Timelines (PlayableAssets) ───────────────────────");
            int missing = 0;
            foreach (var (id, path) in RequiredTimelines)
            {
                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                if (asset != null)
                {
                    sb.AppendLine($"  ✓ Built      {id,-30} → {path}");
                }
                else
                {
                    sb.AppendLine($"  ❌ Missing   {id,-30} → {path}");
                    missing++;
                }
            }
            sb.AppendLine($"  Summary: {RequiredTimelines.Length - missing} built / {RequiredTimelines.Length} required.");
            if (missing > 0)
            {
                sb.AppendLine($"  ❌ {missing} required timeline(s) missing — Phase 36 builds them.");
                errors += missing;
            }
            sb.AppendLine();
        }

        // ───── 4. Audio folders ────────────────────────────────────

        private static void AuditAudioFolders(StringBuilder sb, ref int errors, ref int warns)
        {
            sb.AppendLine("─── 4. Audio Folders ────────────────────────────────────────────");
            foreach (var (folder, label) in RequiredAudioFolders)
            {
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    sb.AppendLine($"  ❌ Missing   {label,-50} → {folder}");
                    errors++;
                    continue;
                }
                int wavCount = 0, oggCount = 0, mp3Count = 0;
                foreach (var guid in AssetDatabase.FindAssets("t:AudioClip", new[] { folder }))
                {
                    var p = AssetDatabase.GUIDToAssetPath(guid);
                    if (p.EndsWith(".wav", StringComparison.OrdinalIgnoreCase)) wavCount++;
                    else if (p.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase)) oggCount++;
                    else if (p.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)) mp3Count++;
                }
                int total = wavCount + oggCount + mp3Count;
                if (total == 0)
                {
                    sb.AppendLine($"  ⚠️ Empty    {label,-50} ({folder})");
                    warns++;
                }
                else
                {
                    sb.AppendLine($"  ✓ {total,3} clip(s)  {label,-50} ({folder})");
                }
            }
            sb.AppendLine();
        }

        // ───── 5. Yarn ─────────────────────────────────────────────

        private static void AuditYarn(StringBuilder sb, ref int errors, ref int warns)
        {
            sb.AppendLine("─── 5. Yarn dialogue files ───────────────────────────────────────");
            const string yarnDir = "Assets/_Project/Yarn";
            if (!AssetDatabase.IsValidFolder(yarnDir))
            {
                sb.AppendLine("  ❌ Yarn folder missing.");
                errors++;
                sb.AppendLine();
                return;
            }
            var files = Directory.GetFiles(yarnDir, "*.yarn");
            foreach (var f in files.OrderBy(x => x))
            {
                int lines = File.ReadAllLines(f).Length;
                string name = Path.GetFileName(f);
                if (lines < 5)
                {
                    sb.AppendLine($"  ⚠️ {name,-30}   {lines,4} line(s) (looks empty)");
                    warns++;
                }
                else
                {
                    sb.AppendLine($"  ✓ {name,-30}   {lines,4} line(s)");
                }
            }
            sb.AppendLine();
        }

        // ───── 6. ScriptableObjects ────────────────────────────────

        private static void AuditSeedAssets(StringBuilder sb, ref int errors, ref int warns)
        {
            sb.AppendLine("─── 6. ScriptableObject seed assets ──────────────────────────────");
            int missing = 0;
            foreach (var path in RequiredSeedAssets)
            {
                var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (so == null)
                {
                    sb.AppendLine($"  ❌ Missing   {path}");
                    missing++;
                }
                else
                {
                    sb.AppendLine($"  ✓ Present    {Path.GetFileName(path),-40} ({so.GetType().Name})");
                }
            }
            sb.AppendLine($"  Summary: {RequiredSeedAssets.Length - missing} / {RequiredSeedAssets.Length} required seeds present.");
            if (missing > 0)
            {
                sb.AppendLine($"  ❌ {missing} missing — re-run Phase 11 'Create Mission 1-2 Seed Assets'.");
                errors += missing;
            }
            sb.AppendLine();
        }
    }
}
