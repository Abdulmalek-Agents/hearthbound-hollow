// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase20_YarnSpinnerBuilder
//
// Phase 20 — Yarn Spinner Integration.
//
// Verifies the Yarn Spinner UPM package is installed; if not, instructs the
// user how to install it. Once present, it creates a YarnRunner.prefab in
// Assets/_Project/Prefabs/Dialogue/ wired to:
//   • Yarn.Unity.DialogueRunner
//   • Our YarnVillageStateBridge (already compile-guarded by YARN_SPINNER_PRESENT)
//   • The 5 .yarn dialogue scripts under Assets/_Project/Yarn/
//
// The scene builder in Phase 22 then spawns this prefab and connects it to
// DialogueUI. Mission01Director's inline dialogue becomes the fallback for
// when Yarn isn't installed (matches the YARN_SPINNER_PRESENT compile guard).
//
// USE: Menu → Hearthbound → Phase 20 — Build Yarn Spinner Runner

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace HearthboundHollow.EditorTools
{
    public static class Phase20_YarnSpinnerBuilder
    {
        private const string YarnGitUrl = "https://github.com/YarnSpinnerTool/YarnSpinner-Unity.git";
        private const string YarnPackageName = "com.yarnspinner.unity";
        private const string PrefabDir = "Assets/_Project/Prefabs/Dialogue";
        private const string YarnRunnerPrefabPath = PrefabDir + "/YarnRunner.prefab";

        [MenuItem("Hearthbound/Phase 20 — Build Yarn Spinner Runner", priority = 207)]
        public static void Build()
        {
            if (!IsYarnInstalled())
            {
                var choice = EditorUtility.DisplayDialogComplex(
                    "Phase 20 — Yarn Spinner not installed",
                    "Yarn Spinner UPM package is not installed.\n\n" +
                    "Install it via Package Manager:\n" +
                    $"  {YarnGitUrl}\n\n" +
                    "Would you like to add it to the manifest now? (The Editor will reload after install.)",
                    "Install now (git URL)", "Cancel", "Copy URL to clipboard");
                if (choice == 0) AddYarnPackage();
                else if (choice == 2) EditorGUIUtility.systemCopyBuffer = YarnGitUrl;
                return;
            }

            EnsureFolder(PrefabDir);
            BuildRunnerPrefab();
        }

        // ─── Yarn detection ───────────────────────────────────────

        private static bool IsYarnInstalled()
        {
            // The asmdef versionDefines define YARN_SPINNER_PRESENT when the package is present.
            // From Editor we can also detect by type lookup.
            return FindYarnType("Yarn.Unity.DialogueRunner") != null;
        }

        private static Type FindYarnType(string fullName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var t = asm.GetType(fullName, throwOnError: false, ignoreCase: false);
                    if (t != null) return t;
                }
                catch { /* ignore */ }
            }
            return null;
        }

        // ─── Package install ──────────────────────────────────────

        private static void AddYarnPackage()
        {
            var request = Client.Add(YarnGitUrl);
            EditorApplication.update += () =>
            {
                if (!request.IsCompleted) return;
                if (request.Status == StatusCode.Success)
                {
                    Debug.Log($"[Hearthbound/Phase 20] Yarn Spinner installed: {request.Result.packageId}");
                    EditorUtility.DisplayDialog("Phase 20 — Installed",
                        "Yarn Spinner installed. After the Editor reloads, re-run " +
                        "'Hearthbound → Phase 20 — Build Yarn Spinner Runner' " +
                        "to build the YarnRunner prefab.", "OK");
                }
                else
                {
                    Debug.LogError($"[Hearthbound/Phase 20] Yarn install failed: {request.Error?.message}");
                    EditorUtility.DisplayDialog("Phase 20 — Install failed",
                        $"Yarn install failed:\n{request.Error?.message}\n\n" +
                        $"Try manually: Window → Package Manager → + → Add from git URL → {YarnGitUrl}",
                        "OK");
                }
            };
        }

        // ─── Runner prefab ────────────────────────────────────────

        private static void BuildRunnerPrefab()
        {
            var runnerType = FindYarnType("Yarn.Unity.DialogueRunner");
            if (runnerType == null)
            {
                Debug.LogError("[Hearthbound/Phase 20] Yarn.Unity.DialogueRunner type missing after detection — aborting.");
                return;
            }

            var go = new GameObject("YarnRunner");
            try
            {
                var runner = go.AddComponent(runnerType);

                // Look for our compile-guarded bridge component
                var bridgeType = FindYarnType("HearthboundHollow.Dialogue.YarnVillageStateBridge");
                if (bridgeType != null)
                {
                    var bridge = go.AddComponent(bridgeType);
                    // Wire the runner field if the bridge has one
                    var runnerField = bridgeType.GetField("runner", BindingFlags.Public | BindingFlags.Instance);
                    if (runnerField != null && runnerField.FieldType.IsAssignableFrom(runnerType))
                        runnerField.SetValue(bridge, runner);
                }

                // Wire the .yarn project / scripts if present.
                // Yarn 2.x expects a YarnProject; Yarn 3.x can take a list of YarnScripts.
                AssignYarnSources(runner, runnerType);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Hearthbound/Phase 20] Some runner wiring failed: {e.Message}");
            }

            PrefabUtility.SaveAsPrefabAsset(go, YarnRunnerPrefabPath);
            UnityEngine.Object.DestroyImmediate(go);

            Debug.Log($"[Hearthbound/Phase 20] (created) {YarnRunnerPrefabPath}");
            EditorUtility.DisplayDialog(
                "Phase 20 — Done",
                $"YarnRunner prefab created at:\n  {YarnRunnerPrefabPath}\n\n" +
                "It has the DialogueRunner + YarnVillageStateBridge attached. The 5 .yarn files " +
                "under Assets/_Project/Yarn/ were wired into the runner's source list if a slot was found.\n\n" +
                "Re-run 'Hearthbound → Build Playable Mission 1 (One Click)' — the scene builder " +
                "will spawn this prefab instead of Mission01Director's inline dialogue when Yarn is " +
                "available.",
                "OK");
        }

        private static void AssignYarnSources(object runner, Type runnerType)
        {
            var yarnFiles = new List<UnityEngine.Object>();
            var yarnFolder = "Assets/_Project/Yarn";
            if (AssetDatabase.IsValidFolder(yarnFolder))
            {
                foreach (var guid in AssetDatabase.FindAssets("", new[] { yarnFolder }))
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!path.EndsWith(".yarn")) continue;
                    var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                    if (obj != null) yarnFiles.Add(obj);
                }
            }
            if (yarnFiles.Count == 0) return;

            // Try common field/property names for Yarn versions.
            string[] sourceFieldNames = { "yarnScripts", "scripts", "yarnProject", "project", "m_YarnScripts" };
            foreach (var name in sourceFieldNames)
            {
                var field = runnerType.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field == null) continue;
                try
                {
                    if (field.FieldType.IsArray)
                    {
                        var elemType = field.FieldType.GetElementType();
                        var arr = Array.CreateInstance(elemType, yarnFiles.Count);
                        for (int i = 0; i < yarnFiles.Count; i++)
                        {
                            if (elemType.IsInstanceOfType(yarnFiles[i])) arr.SetValue(yarnFiles[i], i);
                        }
                        field.SetValue(runner, arr);
                        Debug.Log($"[Hearthbound/Phase 20] Wired {yarnFiles.Count} .yarn files into '{name}'.");
                        return;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Hearthbound/Phase 20] Could not assign field '{name}': {e.Message}");
                }
            }
            Debug.LogWarning(
                "[Hearthbound/Phase 20] Couldn't auto-wire .yarn scripts onto the runner. " +
                "Open the YarnRunner prefab and drag the .yarn files manually onto the DialogueRunner's source list.");
        }

        // ─── Folder helpers ───────────────────────────────────────

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            var leaf = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        public static GameObject TryGetYarnRunnerPrefab() =>
            AssetDatabase.LoadAssetAtPath<GameObject>(YarnRunnerPrefabPath);

        public static bool IsYarnSpinnerAvailable() => IsYarnInstalled();
    }
}
