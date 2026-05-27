// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase46_LevelPolishDiagnostic
//
// Phase 46.8 — Read-only audit of Phase 46's outputs.
//
// Per-scene checks:
//   ✓ Skybox material assigned (RenderSettings.skybox != default)
//   ✓ Sun directional light bound (RenderSettings.sun != null)
//   ✓ Phase 46 environment parent present
//   ✓ Invisible boundary blockers count
//   ✓ Visible perimeter wall count (Lane / Garden)
//   ✓ Guide lanterns count with LaneGuidePulse component (Lane)
//   ✓ Hearth flicker present (Cottage)
//
// USE: Menu → Hearthbound → ⚙️ Advanced → 🔍 Phase 46.8 — Diagnose Level Polish
//
// Chained by the top-level `🔍 Diagnose Build` aggregate (Phase 33).

using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HearthboundHollow.EditorTools
{
    public static class Phase46_LevelPolishDiagnostic
    {
        [MenuItem("Hearthbound/⚙️ Advanced/🔍 Phase 46.8 — Diagnose Level Polish", priority = 468)]
        public static void Diagnose()
        {
            var report = RunDiagnostics();
            EditorUtility.DisplayDialog("Phase 46.8 — Level Polish Audit",
                report,
                "OK");
        }

        /// <summary>
        /// Public entry-point used by Phase 33 / Diagnose Build aggregate.
        /// Returns a multi-line report string. Does NOT show a dialog.
        /// </summary>
        public static string RunDiagnostics()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Phase 46 — Level Polish Audit");
            sb.AppendLine("==============================");

            CheckScene(sb, "Assets/_Project/Scenes/02_Mission01_Lane.unity",
                env: "_Phase46Env_Lane",
                expectPerimeter: true,
                expectGuideLanterns: true,
                expectHearthFlicker: false);

            CheckScene(sb, "Assets/_Project/Scenes/03_Mission01_Hollow.unity",
                env: "_Phase46Env_Hollow",
                expectPerimeter: false,
                expectGuideLanterns: false,
                expectHearthFlicker: false);

            CheckScene(sb, "Assets/_Project/Scenes/04_Mission02_Garden.unity",
                env: "_Phase46Env_Garden",
                expectPerimeter: true,
                expectGuideLanterns: false,
                expectHearthFlicker: false);

            CheckScene(sb, "Assets/_Project/Scenes/05_Mission02_Cottage.unity",
                env: "_Phase46Env_Cottage",
                expectPerimeter: false,
                expectGuideLanterns: false,
                expectHearthFlicker: true);

            sb.AppendLine();
            return sb.ToString();
        }

        private static void CheckScene(StringBuilder sb, string scenePath, string env,
            bool expectPerimeter, bool expectGuideLanterns, bool expectHearthFlicker)
        {
            sb.AppendLine();
            sb.AppendLine($"Scene: {System.IO.Path.GetFileNameWithoutExtension(scenePath)}");

            if (!System.IO.File.Exists(scenePath))
            {
                sb.AppendLine("  ✗ Scene file missing.");
                return;
            }

            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // 1) Skybox
            sb.AppendLine(RenderSettings.skybox != null
                ? "  ✓ Skybox material assigned"
                : "  ✗ Skybox missing");

            // 2) Sun
            sb.AppendLine(RenderSettings.sun != null
                ? $"  ✓ Sun bound — intensity={RenderSettings.sun.intensity:F2}, dir={RenderSettings.sun.transform.eulerAngles}"
                : "  ⚠ Sun directional light not bound to RenderSettings.sun");

            // 3) Env parent
            var envObj = GameObject.Find(env);
            sb.AppendLine(envObj != null
                ? $"  ✓ {env} present  ({envObj.transform.childCount} top-level children)"
                : $"  ✗ {env} missing  — re-run Phase 46 capstone");

            // 4) Perimeter
            if (expectPerimeter)
            {
                int wallCount = CountChildrenContaining(envObj, "Wall_") + CountChildrenContaining(envObj, "Fence_");
                int voidBlockers = CountChildrenContaining(envObj, "VoidBlocker_") + CountChildrenContaining(envObj, "Block_");
                sb.AppendLine(wallCount > 0
                    ? $"  ✓ Visible perimeter pieces: {wallCount}"
                    : "  ⚠ No visible perimeter walls found");
                sb.AppendLine(voidBlockers >= 4
                    ? $"  ✓ Invisible void blockers: {voidBlockers}"
                    : "  ✗ Missing invisible void blockers");
            }

            // 5) Guide lanterns
            if (expectGuideLanterns)
            {
#if UNITY_2023_1_OR_NEWER
                var pulses = Object.FindObjectsByType<HearthboundHollow.Player.LaneGuidePulse>(FindObjectsSortMode.None);
#else
                var pulses = Object.FindObjectsOfType<HearthboundHollow.Player.LaneGuidePulse>();
#endif
                sb.AppendLine(pulses.Length >= 3
                    ? $"  ✓ Guide lanterns with LaneGuidePulse: {pulses.Length}"
                    : $"  ⚠ Only {pulses.Length} guide lanterns found (expected ≥3)");
            }

            // 6) Hearth flicker
            if (expectHearthFlicker)
            {
#if UNITY_2023_1_OR_NEWER
                var flickers = Object.FindObjectsByType<HearthboundHollow.Player.HearthFlicker>(FindObjectsSortMode.None);
#else
                var flickers = Object.FindObjectsOfType<HearthboundHollow.Player.HearthFlicker>();
#endif
                sb.AppendLine(flickers.Length >= 1
                    ? $"  ✓ Hearth flicker components: {flickers.Length}"
                    : "  ⚠ Hearth flicker missing in Cottage scene");
            }

            // 7) Fog
            sb.AppendLine(RenderSettings.fog
                ? $"  ✓ Fog ON (density={RenderSettings.fogDensity:F3})"
                : "  • Fog OFF (interior or by design)");
        }

        private static int CountChildrenContaining(GameObject parent, string fragment)
        {
            if (parent == null) return 0;
            int count = 0;
            foreach (var t in parent.GetComponentsInChildren<Transform>(true))
                if (t.name.Contains(fragment)) count++;
            return count;
        }
    }
}
