// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase23_DiagnosticReport
//
// A diagnostic Editor menu that audits the project and prints a colorful
// pass/fail report. Useful after pulling the branch to confirm that
// Phase 23 ran cleanly and all 6 scenes have the expected components.
//
// USE: Menu → Hearthbound → 🔍 Diagnose Phase 23 Build

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using HearthboundHollow.Audio;
using HearthboundHollow.Core;
using HearthboundHollow.Mission;
using HearthboundHollow.UI;

namespace HearthboundHollow.EditorTools
{
    public static class Phase23_DiagnosticReport
    {
        [MenuItem("Hearthbound/\ud83d\udd0d Diagnose Phase 23 Build", priority = 50)]
        public static void Diagnose()
        {
            var sb = new System.Text.StringBuilder();
            int pass = 0, warn = 0, fail = 0;

            void Pass(string m) { sb.AppendLine("  ✓ " + m); pass++; }
            void Warn(string m) { sb.AppendLine("  ⚠ " + m); warn++; }
            void Fail(string m) { sb.AppendLine("  ✗ " + m); fail++; }

            sb.AppendLine("Hearthbound Hollow — Phase 23 Build Diagnostic\n");

            // ─── ScriptableObjects ─────────────────────────────────
            sb.AppendLine("Seed assets:");
            CheckAsset<VillageState>("Assets/_Project/ScriptableObjects/State/VillageState.asset", Pass, Fail);
            CheckAsset<MissionSO>("Assets/_Project/ScriptableObjects/Missions/Mission01_OpeningTheHollow.asset", Pass, Fail);
            CheckAsset<MissionSO>("Assets/_Project/ScriptableObjects/Missions/Mission02_TheWidowersRequest.asset", Pass, Fail);
            CheckAsset<ScriptableObject>("Assets/_Project/ScriptableObjects/Villagers/Doris.asset", Pass, Fail);
            CheckAsset<ScriptableObject>("Assets/_Project/ScriptableObjects/Villagers/Gerrold.asset", Pass, Fail);
            CheckAsset<ScriptableObject>("Assets/_Project/ScriptableObjects/Memories/DOR-001_FirstLoaves.asset", Pass, Fail);
            CheckAsset<ScriptableObject>("Assets/_Project/ScriptableObjects/Memories/GER-007_SeventhMorning.asset", Pass, Fail);
            CheckAsset<ScriptableObject>("Assets/_Project/ScriptableObjects/Tariffs/Tariff_Erase.asset", Pass, Fail);
            CheckAsset<ScriptableObject>("Assets/_Project/ScriptableObjects/Tariffs/Tariff_Cleanse.asset", Pass, Fail);
            CheckAsset<ScriptableObject>("Assets/_Project/ScriptableObjects/Tariffs/Tariff_Listen.asset", Pass, Fail);
            CheckAsset<ScriptableObject>("Assets/_Project/ScriptableObjects/Tariffs/Tariff_Defer.asset", Pass, Fail);
            sb.AppendLine();

            // ─── Scenes ────────────────────────────────────────────
            sb.AppendLine("Scenes:");
            string[] scenes = {
                "Assets/_Project/Scenes/00_Bootstrap.unity",
                "Assets/_Project/Scenes/01_MainMenu.unity",
                "Assets/_Project/Scenes/02_Mission01_Lane.unity",
                "Assets/_Project/Scenes/03_Mission01_Hollow.unity",
                "Assets/_Project/Scenes/04_Mission02_Garden.unity",
                "Assets/_Project/Scenes/05_Mission02_Cottage.unity",
            };
            foreach (var s in scenes)
            {
                if (File.Exists(s)) Pass(s);
                else Fail(s + " — MISSING (run Phase 23 capstone)");
            }
            sb.AppendLine();

            // ─── Scene contents ────────────────────────────────────
            sb.AppendLine("Scene 00_Bootstrap contents:");
            DiagnoseScene("Assets/_Project/Scenes/00_Bootstrap.unity",
                requireComponents: new[] { typeof(GameManager), typeof(SettingsServiceBootstrap) },
                Pass, Warn);

            sb.AppendLine("\nScene 01_MainMenu contents:");
            DiagnoseScene("Assets/_Project/Scenes/01_MainMenu.unity",
                requireComponents: new[] { typeof(MainMenuController), typeof(MainMenuSaveCoordinator), typeof(ComfortToolsMenu) },
                Pass, Warn);

            sb.AppendLine("\nScene 02_Mission01_Lane contents:");
            DiagnoseScene("Assets/_Project/Scenes/02_Mission01_Lane.unity",
                requireComponents: new[] { typeof(PauseMenuUI), typeof(HelpOverlayUI), typeof(MissionTitleCard), typeof(AmbientAudio), typeof(PauseSaveCoordinator) },
                Pass, Warn);

            sb.AppendLine("\nScene 03_Mission01_Hollow contents:");
            DiagnoseScene("Assets/_Project/Scenes/03_Mission01_Hollow.unity",
                requireComponents: new[] { typeof(PauseMenuUI), typeof(HelpOverlayUI), typeof(MissionTitleCard), typeof(AmbientAudio), typeof(PauseSaveCoordinator), typeof(PickleAI), typeof(Mission01Director) },
                Pass, Warn);

            sb.AppendLine("\nScene 04_Mission02_Garden contents:");
            DiagnoseScene("Assets/_Project/Scenes/04_Mission02_Garden.unity",
                requireComponents: new[] { typeof(PauseMenuUI), typeof(HelpOverlayUI), typeof(AmbientAudio), typeof(PauseSaveCoordinator), typeof(Mission02Director), typeof(TeaBrewingUI) },
                Pass, Warn);

            sb.AppendLine("\nScene 05_Mission02_Cottage contents:");
            DiagnoseScene("Assets/_Project/Scenes/05_Mission02_Cottage.unity",
                requireComponents: new[] { typeof(PauseMenuUI), typeof(HelpOverlayUI), typeof(AmbientAudio), typeof(PauseSaveCoordinator), typeof(Mission02Director), typeof(ChoiceCardUI) },
                Pass, Warn);

            sb.AppendLine();

            // ─── Build settings ────────────────────────────────────
            sb.AppendLine("Build Settings:");
            var bs = EditorBuildSettings.scenes;
            if (bs.Length < 4) Fail($"Only {bs.Length}/6 scenes in Build Settings. Re-run Phase 23.");
            else if (bs.Length == 4) Warn("4/6 scenes in Build Settings — Mission 2 scenes not added. Run Phase 24 or 23.");
            else Pass($"{bs.Length}/6 scenes in Build Settings.");
            sb.AppendLine();

            // ─── Audio library ─────────────────────────────────────
            sb.AppendLine("Audio:");
            var sfxLib = AssetDatabase.LoadAssetAtPath<SfxLibrarySO>("Assets/_Project/Audio/SfxLibrary.asset");
            if (sfxLib == null) Warn("SfxLibrary.asset not present — run Phase 18.");
            else
            {
                int filled = 0, total = sfxLib.entries.Count;
                foreach (var e in sfxLib.entries) if (e.clip != null) filled++;
                if (filled == 0) Warn($"SfxLibrary has {total} entries but no clips assigned.");
                else if (filled < total) Warn($"SfxLibrary has {filled}/{total} clips assigned — some keywords didn't match.");
                else Pass($"SfxLibrary fully populated ({filled}/{total}).");
            }
            sb.AppendLine();

            // ─── Summary ───────────────────────────────────────────
            sb.AppendLine("─────────────────────────────────────");
            sb.AppendLine($"Pass: {pass} · Warn: {warn} · Fail: {fail}");
            sb.AppendLine();
            if (fail == 0 && warn == 0) sb.AppendLine("✅ ALL CHECKS PASSED — press Play!");
            else if (fail == 0) sb.AppendLine("⚠️ Warnings only — game is playable, but consider running the suggested phases.");
            else sb.AppendLine("❌ Some checks failed. Run: Hearthbound → 🎮 Build POLISHED Mission 1 + 2 (Phase 23)");

            Debug.Log(sb.ToString());
            EditorUtility.DisplayDialog(
                "Hearthbound — Phase 23 Diagnostic",
                $"{pass} passed · {warn} warnings · {fail} failed.\n\nSee Console for the full report.",
                "OK");
        }

        private static void CheckAsset<T>(string path, System.Action<string> Pass, System.Action<string> Fail) where T : Object
        {
            if (AssetDatabase.LoadAssetAtPath<T>(path) != null) Pass(path);
            else Fail(path + " — MISSING (run Hearthbound → Create Mission 1-2 Seed Assets)");
        }

        private static void DiagnoseScene(string scenePath, System.Type[] requireComponents,
            System.Action<string> Pass, System.Action<string> Warn)
        {
            if (!File.Exists(scenePath)) { Warn(scenePath + " not present — skipped"); return; }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            foreach (var t in requireComponents)
            {
                bool found = false;
                foreach (var go in scene.GetRootGameObjects())
                {
                    if (go.GetComponentInChildren(t, includeInactive: true) != null) { found = true; break; }
                }
                if (found) Pass(t.Name + " present");
                else Warn(t.Name + " MISSING in " + Path.GetFileNameWithoutExtension(scenePath));
            }
        }
    }
}
