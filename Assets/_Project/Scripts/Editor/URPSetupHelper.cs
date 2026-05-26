// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / URPSetupHelper
//
// One-click URP project configuration. Resolves the
// "Microdetail doesn't support built-in and custom render pipelines" warning
// (and the equivalent warnings from many other URP-only assets).
//
// USE: Menu → Hearthbound → Setup URP Pipeline (one-time)
//
// Also: [InitializeOnLoad] auto-detects on Editor startup whether URP is
// currently active. If not, surfaces a dialog suggesting the user run
// the setup menu. This catches the "user pulled the script but forgot to
// run the menu" case that left Microdetail still complaining.

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace HearthboundHollow.EditorTools
{
    [InitializeOnLoad]
    public static class URPSetupHelper
    {
        private const string SettingsDir = "Assets/_Project/Settings";
        private const string UrpAssetPath = SettingsDir + "/URP-Mobile.asset";
        private const string UrpRendererPath = SettingsDir + "/URP-MobileRenderer.asset";
        private const string SuppressPromptKey = "Hearthbound.URPSetupHelper.SuppressStartupPrompt";

        // ─── Startup detection ──────────────────────────────────────

        static URPSetupHelper()
        {
            // Defer to next editor tick so AssetDatabase is fully ready.
            EditorApplication.delayCall += CheckOnStartup;
        }

        private static void CheckOnStartup()
        {
            if (SessionState.GetBool("Hearthbound.URPSetupCheckedThisSession", false)) return;
            SessionState.SetBool("Hearthbound.URPSetupCheckedThisSession", true);

            if (EditorPrefs.GetBool(SuppressPromptKey, false)) return;

            if (IsUrpAlreadyActive()) return;

            var choice = EditorUtility.DisplayDialogComplex(
                "Hearthbound — URP not active",
                "URP (Universal Render Pipeline) is installed in the manifest but no URP asset is " +
                "currently assigned to GraphicsSettings.\n\n" +
                "Without URP active, Microdetail, LightMap Fusion, and several other URP-only " +
                "assets will log errors.\n\n" +
                "Would you like to set up URP now? This will create a Mobile-friendly URP asset " +
                "and assign it as the project's default render pipeline.",
                "Set up URP now",
                "Don't ask again",
                "Skip for now");

            switch (choice)
            {
                case 0: SetupURPPipeline(); break;
                case 1: EditorPrefs.SetBool(SuppressPromptKey, true); break;
                case 2: /* skip */ break;
            }
        }

        private static bool IsUrpAlreadyActive()
        {
            var current = GraphicsSettings.defaultRenderPipeline;
            return current is UniversalRenderPipelineAsset;
        }

        // ─── Manual menu ────────────────────────────────────────────

        [MenuItem("Hearthbound/Setup URP Pipeline (one-time)", priority = 50)]
        public static void SetupURPPipeline()
        {
            EnsureFolder(SettingsDir);

            var urpAsset = FindOrCreateURPAsset();
            if (urpAsset == null)
            {
                EditorUtility.DisplayDialog(
                    "URP Setup",
                    "Could not auto-create the URP Render Pipeline Asset.\n\n" +
                    "Please create it manually:\n" +
                    "1. Right-click in the Project window\n" +
                    "2. Create → Rendering → URP Asset (with Universal Renderer)\n" +
                    "3. Save as Assets/_Project/Settings/URP-Mobile.asset\n" +
                    "4. Re-run this menu item (Hearthbound → Setup URP Pipeline).",
                    "OK");
                return;
            }

            // Assign as default.
            GraphicsSettings.defaultRenderPipeline = urpAsset;

            // Assign to every quality level.
            int levelCount = QualitySettings.names.Length;
            int previousLevel = QualitySettings.GetQualityLevel();
            for (int i = 0; i < levelCount; i++)
            {
                QualitySettings.SetQualityLevel(i, applyExpensiveChanges: false);
                QualitySettings.renderPipeline = urpAsset;
            }
            QualitySettings.SetQualityLevel(previousLevel, applyExpensiveChanges: false);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.SetDirty(GraphicsSettings.GetGraphicsSettings());

            // Force the render pipeline to actually reload.
            GraphicsSettings.defaultRenderPipeline = null;
            GraphicsSettings.defaultRenderPipeline = urpAsset;

            Debug.Log("[Hearthbound/URP] URP pipeline configured. " +
                      $"Default pipeline = {urpAsset.name}. " +
                      $"Assigned to all {levelCount} quality levels.");

            EditorUtility.DisplayDialog(
                "URP Setup",
                "Done! URP is now the active render pipeline.\n\n" +
                "Microdetail and other URP-dependent assets should now stop logging errors.\n\n" +
                "If existing materials appear pink, run:\n" +
                "  Window → Rendering → Render Pipeline Converter\n" +
                "  → Built-in to URP → Material Upgrade → Convert Assets",
                "OK");
        }

        [MenuItem("Hearthbound/Check Render Pipeline Status", priority = 51)]
        public static void CheckRenderPipelineStatus()
        {
            var current = GraphicsSettings.defaultRenderPipeline;
            var msg = new System.Text.StringBuilder();
            msg.AppendLine($"GraphicsSettings.defaultRenderPipeline = {(current != null ? current.name : "(null — Built-in!)")}");
            msg.AppendLine($"GraphicsSettings.currentRenderPipeline   = {(GraphicsSettings.currentRenderPipeline != null ? GraphicsSettings.currentRenderPipeline.GetType().Name : "(null)")}");
            msg.AppendLine($"Is URP active: {IsUrpAlreadyActive()}");

            int levels = QualitySettings.names.Length;
            int previous = QualitySettings.GetQualityLevel();
            msg.AppendLine($"Quality levels: {levels}");
            for (int i = 0; i < levels; i++)
            {
                QualitySettings.SetQualityLevel(i, applyExpensiveChanges: false);
                var per = QualitySettings.renderPipeline;
                msg.AppendLine($"  [{i}] {QualitySettings.names[i]} → {(per != null ? per.name : "(default)")}");
            }
            QualitySettings.SetQualityLevel(previous, applyExpensiveChanges: false);

            Debug.Log("[Hearthbound/URP]\n" + msg);
            EditorUtility.DisplayDialog("Render Pipeline Status", msg.ToString(), "OK");
        }

        // ─── URP asset creation ─────────────────────────────────────

        private static UniversalRenderPipelineAsset FindOrCreateURPAsset()
        {
            var atPath = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(UrpAssetPath);
            if (atPath != null)
            {
                Debug.Log($"[Hearthbound/URP] (reuse) {UrpAssetPath}");
                return atPath;
            }

            var guids = AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset");
            if (guids != null && guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                var existing = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);
                if (existing != null)
                {
                    Debug.Log($"[Hearthbound/URP] (reuse existing) {path}");
                    return existing;
                }
            }

            try { return CreateFreshURPAsset(); }
            catch (System.Exception e)
            {
                Debug.LogError($"[Hearthbound/URP] Failed to auto-create URP asset: {e}");
                return null;
            }
        }

        private static UniversalRenderPipelineAsset CreateFreshURPAsset()
        {
            // (a) Renderer data
            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(UrpRendererPath);
            if (renderer == null)
            {
                renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                renderer.name = "URP-MobileRenderer";
                AssetDatabase.CreateAsset(renderer, UrpRendererPath);
                Debug.Log($"[Hearthbound/URP] (created) {UrpRendererPath}");
            }

            // (b) Pipeline asset
            var urpAsset = ScriptableObject.CreateInstance<UniversalRenderPipelineAsset>();
            urpAsset.name = "URP-Mobile";

            var so = new SerializedObject(urpAsset);
            var rendererListProp = so.FindProperty("m_RendererDataList");
            if (rendererListProp != null && rendererListProp.isArray)
            {
                rendererListProp.arraySize = 1;
                rendererListProp.GetArrayElementAtIndex(0).objectReferenceValue = renderer;
            }
            var defaultIdxProp = so.FindProperty("m_DefaultRendererIndex");
            if (defaultIdxProp != null) defaultIdxProp.intValue = 0;

            // Mobile-friendly defaults (ARCHITECTURE.md § 10).
            ApplyIfPresent(so, "m_RenderScale", 0.85f);
            ApplyIfPresent(so, "m_MSAA", 2);
            ApplyIfPresent(so, "m_SupportsHDR", false);
            ApplyIfPresent(so, "m_SoftShadowsSupported", false);
            ApplyIfPresent(so, "m_MainLightShadowsSupported", true);
            ApplyIfPresent(so, "m_AdditionalLightsRenderingMode", 1);

            so.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(urpAsset, UrpAssetPath);
            Debug.Log($"[Hearthbound/URP] (created) {UrpAssetPath}");
            return urpAsset;
        }

        private static void ApplyIfPresent(SerializedObject so, string propertyName, object value)
        {
            var p = so.FindProperty(propertyName);
            if (p == null) return;
            switch (value)
            {
                case bool b:   p.boolValue = b; break;
                case int i:    p.intValue = i; break;
                case float f:  p.floatValue = f; break;
                case string s: p.stringValue = s; break;
            }
        }

        // ─── Folder helpers ─────────────────────────────────────────

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
