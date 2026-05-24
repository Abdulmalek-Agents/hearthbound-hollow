// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / URPSetupHelper
//
// One-click URP project configuration. Resolves the
// "Microdetail doesn't support built-in and custom render pipelines" warning
// (and the equivalent warnings from many other URP-only assets).
//
// USE: Menu → Hearthbound → Setup URP Pipeline (one-time)
//
// What it does:
//   1. Creates `Assets/_Project/Settings/URP-MobileRenderer.asset` if missing
//      (a UniversalRendererData configured for mobile-friendly defaults).
//   2. Creates `Assets/_Project/Settings/URP-Mobile.asset` if missing
//      (a UniversalRenderPipelineAsset referencing the renderer).
//   3. Assigns the URP asset to GraphicsSettings.defaultRenderPipeline.
//   4. Assigns the URP asset to every QualitySettings level.
//   5. Saves the project so the change persists across Editor restarts.
//
// Idempotent: if assets already exist, they are reused. If GraphicsSettings
// is already set to a URP asset, that asset is reused.

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace HearthboundHollow.EditorTools
{
    public static class URPSetupHelper
    {
        private const string SettingsDir = "Assets/_Project/Settings";
        private const string UrpAssetPath = SettingsDir + "/URP-Mobile.asset";
        private const string UrpRendererPath = SettingsDir + "/URP-MobileRenderer.asset";

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

            // Assign as the default render pipeline.
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

            // Persist.
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.SetDirty(GraphicsSettings.GetGraphicsSettings());

            Debug.Log("[Hearthbound/URP] URP pipeline configured. " +
                      $"Default pipeline = {urpAsset.name}. " +
                      $"Assigned to all {levelCount} quality levels.");

            EditorUtility.DisplayDialog(
                "URP Setup",
                "Done! URP is now the active render pipeline.\n\n" +
                "Microdetail and other URP-dependent assets should now stop complaining.\n\n" +
                "You may need to upgrade existing materials to URP shaders:\n" +
                "  Edit → Rendering → Materials → Convert All Built-in Materials to URP",
                "OK");
        }

        [MenuItem("Hearthbound/Setup URP Pipeline (one-time)", validate = true)]
        public static bool SetupURPPipeline_Validate()
        {
            // Always available; the action itself is idempotent.
            return true;
        }

        // ─── URP asset creation ─────────────────────────────────────

        private static UniversalRenderPipelineAsset FindOrCreateURPAsset()
        {
            // 1. Look at the canonical path first.
            var atPath = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(UrpAssetPath);
            if (atPath != null)
            {
                Debug.Log($"[Hearthbound/URP] (reuse) {UrpAssetPath}");
                return atPath;
            }

            // 2. Search the whole project for any existing URP asset (user may have created one elsewhere).
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

            // 3. Try to create a fresh one.
            try
            {
                return CreateFreshURPAsset();
            }
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

            // Wire the renderer via SerializedObject (the renderer-list field is internal).
            var so = new SerializedObject(urpAsset);
            var rendererListProp = so.FindProperty("m_RendererDataList");
            if (rendererListProp != null && rendererListProp.isArray)
            {
                rendererListProp.arraySize = 1;
                rendererListProp.GetArrayElementAtIndex(0).objectReferenceValue = renderer;
            }
            var defaultIdxProp = so.FindProperty("m_DefaultRendererIndex");
            if (defaultIdxProp != null) defaultIdxProp.intValue = 0;

            // Mobile-friendly defaults (Mission 1-2 budget per ARCHITECTURE.md § 10):
            //   render scale 0.85, MSAA 2×, soft shadows OFF, 1 directional light only.
            ApplyIfPresent(so, "m_RenderScale", 0.85f);
            ApplyIfPresent(so, "m_MSAA", 2);                       // 2× MSAA
            ApplyIfPresent(so, "m_SupportsHDR", false);
            ApplyIfPresent(so, "m_SoftShadowsSupported", false);
            ApplyIfPresent(so, "m_MainLightShadowsSupported", true);
            ApplyIfPresent(so, "m_AdditionalLightsRenderingMode", 1); // PerPixel

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
