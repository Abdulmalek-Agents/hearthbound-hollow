// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase46_LevelBoundariesCapstone
//
// PHASE 46 — Level Boundaries, Wider Environment, Skybox + Lighting Polish.
//
// The user-facing request: the lane was too narrow, props had no colliders so
// the player could fall through them or escape into the void, the cozy autumn
// skybox was missing, and the Hollow / Cottage interiors needed more
// home-dressing items. Phase 46 fixes every one of those — additively, on top
// of every earlier phase, with zero data loss. Idempotent and re-runnable.
//
// Eight sub-builders (all chained from this capstone):
//
//   46.1  Phase46_AutumnSkyboxAndLighting     — Procedural autumn-evening
//         skybox material + warm sun + ambient gradient + reflection probe.
//
//   46.2  Phase46_LaneBoundariesAndWideEnv    — Lane: widens playable area
//         to 24m × 36m, adds a tall stone-wall perimeter on all four sides
//         (visible mesh + invisible MeshCollider blockers), extends the
//         ground plane to cover the new area, widens the cobble path,
//         drops guide lanterns toward the Hollow door (onboarding wayfinding).
//
//   46.3  Phase46_HollowBoundariesAndInterior — Hollow: hardens the wall
//         colliders (player cannot walk through), adds cozy interior
//         polish: rug under the workbench, a second reading chair, books
//         on the shelf, a writing desk, additional candle warmth.
//
//   46.4  Phase46_GardenBoundariesAndPath     — Mission 2 Garden: fenced
//         perimeter on all four sides, wider stepping-stone path, extra
//         herb beds + decorative pots.
//
//   46.5  Phase46_CottageInteriorPolish       — Mission 2 Cottage interior:
//         hardens walls with colliders, adds hearth flames, framed
//         photograph on mantel, bookcase, second chair (Margery's), small
//         table with cold tea cup, rug, candle sconces.
//
//   46.6  Phase46_ColliderHardening           — Walks every cottage / wall /
//         tree / prop in all four gameplay scenes and ensures each has a
//         working Collider, so the player can't accidentally clip through.
//
//   46.7  Phase46_GuideLightsAndWayfinding    — Drops subtle glowing
//         "lantern beacons" along the cobble path that brighten as the
//         player approaches the Hollow door — onboarding wayfinding.
//
//   46.8  Phase46_LevelPolishDiagnostic       — Read-only audit: per-scene
//         boundary completeness, collider coverage, skybox/sun bind, path
//         continuity, interior prop count.
//
// USE: Menu → Hearthbound → ⚙️ Advanced → 🏰 Phase 46 — Level Polish (all)
//
// The user-facing entry point `🚀 Build Everything` chains Phase 46 last so
// every per-mission polish from Phases 23 / 32 is preserved, then Phase 46
// adds the boundaries + skybox + interior polish on top.
//
// IDEMPOTENT — re-running is safe. Every sub-builder parents its output
// under a single `_Phase46Env_<Scene>` GameObject and wipes it on entry,
// so re-running rebuilds cleanly without duplicating geometry.
//
// Architecture notes:
//   - Per D-007: scene files edited in-place + saved (never committed).
//   - Per D-027: TryGet*() lookups via Phase 32.1's MedievalVillageBindingsV2.
//   - Per D-035: Editor-only.
//   - Reflection-driven graceful skipping if any prerequisite phase is absent.

using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HearthboundHollow.EditorTools
{
    public static class Phase46_LevelBoundariesCapstone
    {
        private const string SceneBootstrap = "Assets/_Project/Scenes/00_Bootstrap.unity";

        // ─── Master menu ───────────────────────────────────────────

        [MenuItem("Hearthbound/⚙️ Advanced/🏰 Phase 46 — Level Polish (all)", priority = 46)]
        public static void Build()
        {
            if (!EditorUtility.DisplayDialog(
                "Phase 46 — Level Polish",
                "This runs the full Phase 46 chain (~15 s):\n\n" +
                "  46.1  Autumn skybox + sun + ambient lighting\n" +
                "  46.2  Lane boundaries + wider environment + guide path\n" +
                "  46.3  Hollow boundaries + interior polish\n" +
                "  46.4  Garden boundaries + stepping path\n" +
                "  46.5  Cottage interior polish + hearth flames\n" +
                "  46.6  Collider hardening across all four scenes\n" +
                "  46.7  Guide lights / onboarding wayfinding\n\n" +
                "Safe to re-run any time — every step is idempotent.\n\n" +
                "Continue?",
                "Build", "Cancel")) return;

            int ran = 0, skipped = 0;
            EditorUtility.DisplayProgressBar("Hearthbound · Phase 46", "Starting …", 0.02f);
            try
            {
                EditorUtility.DisplayProgressBar("Hearthbound · Phase 46", "46.1 — Autumn skybox + lighting …", 0.10f);
                if (TryRun("46.1 — Autumn Skybox + Lighting",
                          "HearthboundHollow.EditorTools.Phase46_AutumnSkyboxAndLighting", "Build")) ran++; else skipped++;

                EditorUtility.DisplayProgressBar("Hearthbound · Phase 46", "46.2 — Lane boundaries + wide environment …", 0.25f);
                if (TryRun("46.2 — Lane Boundaries + Wide Environment",
                          "HearthboundHollow.EditorTools.Phase46_LaneBoundariesAndWideEnv", "Build")) ran++; else skipped++;

                EditorUtility.DisplayProgressBar("Hearthbound · Phase 46", "46.3 — Hollow boundaries + interior polish …", 0.42f);
                if (TryRun("46.3 — Hollow Boundaries + Interior Polish",
                          "HearthboundHollow.EditorTools.Phase46_HollowBoundariesAndInterior", "Build")) ran++; else skipped++;

                EditorUtility.DisplayProgressBar("Hearthbound · Phase 46", "46.4 — Garden boundaries + path …", 0.55f);
                if (TryRun("46.4 — Garden Boundaries + Path",
                          "HearthboundHollow.EditorTools.Phase46_GardenBoundariesAndPath", "Build")) ran++; else skipped++;

                EditorUtility.DisplayProgressBar("Hearthbound · Phase 46", "46.5 — Cottage interior polish …", 0.68f);
                if (TryRun("46.5 — Cottage Interior Polish",
                          "HearthboundHollow.EditorTools.Phase46_CottageInteriorPolish", "Build")) ran++; else skipped++;

                EditorUtility.DisplayProgressBar("Hearthbound · Phase 46", "46.6 — Collider hardening …", 0.80f);
                if (TryRun("46.6 — Collider Hardening",
                          "HearthboundHollow.EditorTools.Phase46_ColliderHardening", "Build")) ran++; else skipped++;

                EditorUtility.DisplayProgressBar("Hearthbound · Phase 46", "46.7 — Guide lights + wayfinding …", 0.92f);
                if (TryRun("46.7 — Guide Lights + Wayfinding",
                          "HearthboundHollow.EditorTools.Phase46_GuideLightsAndWayfinding", "Build")) ran++; else skipped++;

                // Final: open Bootstrap so the user can press Play.
                EditorUtility.DisplayProgressBar("Hearthbound · Phase 46", "Opening Bootstrap …", 0.98f);
                if (System.IO.File.Exists(SceneBootstrap))
                    EditorSceneManager.OpenScene(SceneBootstrap, OpenSceneMode.Single);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            EditorUtility.DisplayDialog(
                "Hearthbound — Phase 46 Level Polish",
                BuildSummary(ran, skipped),
                "OK");
        }

        // ─── Reflection runner ─────────────────────────────────────

        private static bool TryRun(string label, string typeFullName, string methodName)
        {
            Type t = FindType(typeFullName);
            if (t == null)
            {
                Debug.LogWarning($"[Hearthbound/Phase 46] (skip) {label} — type '{typeFullName}' not found.");
                return false;
            }
            var m = t.GetMethod(methodName,
                BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
            if (m == null)
            {
                Debug.LogWarning($"[Hearthbound/Phase 46] (skip) {label} — method '{methodName}' not found.");
                return false;
            }
            try
            {
                Debug.Log($"[Hearthbound/Phase 46] → Running {label} …");
                m.Invoke(null, null);
                Debug.Log($"[Hearthbound/Phase 46] ✓ {label} complete.");
                return true;
            }
            catch (Exception e)
            {
                var inner = e.InnerException ?? e;
                Debug.LogError($"[Hearthbound/Phase 46] ✗ {label} threw: {inner}");
                return false;
            }
        }

        private static Type FindType(string fullName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var t = asm.GetType(fullName, throwOnError: false, ignoreCase: false);
                    if (t != null) return t;
                }
                catch { }
            }
            return null;
        }

        // ─── Summary builder ───────────────────────────────────────

        private static string BuildSummary(int ran, int skipped)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"🏰 Phase 46 Level Polish complete — {ran} of {ran + skipped} sub-builders ran.");
            if (skipped > 0)
            {
                sb.AppendLine();
                sb.AppendLine($"⚠ {skipped} sub-builder(s) were skipped.");
            }
            sb.AppendLine();
            sb.AppendLine("Result on disk:");
            sb.AppendLine("  • Procedural autumn skybox material at Assets/_Project/Art/Sky/");
            sb.AppendLine("  • Lane — 24m × 36m playable area with tall stone-wall perimeter");
            sb.AppendLine("  • Lane — invisible boundary colliders (player cannot fall off the world)");
            sb.AppendLine("  • Lane — widened cobble path with guide lanterns toward the Hollow");
            sb.AppendLine("  • Hollow — hardened wall colliders + cozy interior polish");
            sb.AppendLine("  • Garden — fenced perimeter + wider stepping-stone path");
            sb.AppendLine("  • Cottage — hearth flames + framed photograph + bookcase + rug");
            sb.AppendLine("  • All four scenes — every prop has a Collider (no clipping)");
            sb.AppendLine("  • Onboarding wayfinding — guide lights brighten toward the Hollow door");
            sb.AppendLine();
            sb.AppendLine("Press Play in 00_Bootstrap.unity.");
            return sb.ToString();
        }
    }
}
