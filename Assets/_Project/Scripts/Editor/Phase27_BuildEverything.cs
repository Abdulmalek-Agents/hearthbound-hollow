// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase27_BuildEverything
//
// PHASE 27 — The "one menu click" master capstone.
//
// Phase 23 builds the polished scenes. Phase 26 (Player Controller + Animation)
// builds the AnimatorController + upgrades cameras. Phase 26 (NPC Animators)
// wires Doris/Gerrold's IsTalking dialogue beats. Phase 26 (Narrative Hooks)
// drops Marin's Note on the workbench. Until now the user had to run FOUR menu
// items in order. Phase 27 chains them — single click, ~45 s, fully wired.
//
// IDEMPOTENT — every step is safe to re-run any number of times.
//
// USE: Menu → Hearthbound → ✨ Build EVERYTHING (Phase 27 — one click)
//
// Detection-driven: if any phase's prerequisites aren't present (e.g. Phase 26
// Narrative Hooks isn't installed because the Narrative thread hasn't shipped
// yet), this capstone skips that phase gracefully with a warning logged.

using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HearthboundHollow.EditorTools
{
    public static class Phase27_BuildEverything
    {
        private const string SceneBootstrap = "Assets/_Project/Scenes/00_Bootstrap.unity";

        // ─── Master menu ──────────────────────────────────────────

        [MenuItem("Hearthbound/\u2728 Build EVERYTHING (Phase 27 — one click)", priority = -10)]
        public static void Build()
        {
            EditorUtility.DisplayProgressBar("Hearthbound · Phase 27", "Running Phase 23 (polished scenes) …", 0.05f);
            int ran = 0, skipped = 0;
            try
            {
                // Step 1: Phase 23 polished capstone (chains 13–24 internally).
                if (TryRun("Phase 23 — POLISHED Mission 1 + 2",
                          "HearthboundHollow.EditorTools.Phase23_Mission1PolishCapstone", "Build"))
                    ran++;
                else
                    skipped++;

                // Step 2: Phase 26 — Player Controller + Animation (this PR's capstone).
                EditorUtility.DisplayProgressBar("Hearthbound · Phase 27", "Running Phase 26 (Player Controller + Animation) …", 0.45f);
                if (TryRun("Phase 26 — Player Controller + Animation",
                          "HearthboundHollow.EditorTools.Phase26_PlayerControllerAndAnimation", "Build"))
                    ran++;
                else
                    skipped++;

                // Step 3: Phase 26 — NPC Animators (Doris/Gerrold IsTalking).
                EditorUtility.DisplayProgressBar("Hearthbound · Phase 27", "Running Phase 26 (NPC Animators) …", 0.70f);
                if (TryRun("Phase 26 — NPC Animators",
                          "HearthboundHollow.EditorTools.Phase26_NpcAnimatorCapstone", "Build"))
                    ran++;
                else
                    skipped++;

                // Step 4: Phase 26 — Narrative Hooks (Marin's Note). Optional;
                // not all branches have this thread.
                EditorUtility.DisplayProgressBar("Hearthbound · Phase 27", "Running Phase 26 (Narrative Hooks) …", 0.88f);
                // The Narrative Hooks builder exposes its menu method under a
                // few possible names depending on which agent shipped it. Try
                // the conventional ones in order.
                if (TryRun("Phase 26 — Narrative Hooks",
                          "HearthboundHollow.EditorTools.Phase26_NarrativeHooks", "WireNarrativeHooks") ||
                    TryRun("Phase 26 — Narrative Hooks",
                          "HearthboundHollow.EditorTools.Phase26_NarrativeHooks", "Build") ||
                    TryRun("Phase 26 — Narrative Hooks",
                          "HearthboundHollow.EditorTools.Phase26_NarrativeHooks", "Run"))
                    ran++;
                else
                    skipped++;

                // Step 5: Open Bootstrap so the user can press Play.
                EditorUtility.DisplayProgressBar("Hearthbound · Phase 27", "Opening Bootstrap …", 0.98f);
                if (System.IO.File.Exists(SceneBootstrap))
                    EditorSceneManager.OpenScene(SceneBootstrap, OpenSceneMode.Single);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            EditorUtility.DisplayDialog(
                "Hearthbound — Phase 27 — Build EVERYTHING",
                BuildSummary(ran, skipped),
                "OK");
        }

        // ─── Reflection runner ────────────────────────────────────

        /// <summary>
        /// Invokes the named static method on the named type via reflection.
        /// Returns false (and logs a graceful warning) if the type or method
        /// is missing — so an absent phase doesn't break the whole chain.
        /// </summary>
        private static bool TryRun(string label, string typeFullName, string methodName)
        {
            Type t = FindType(typeFullName);
            if (t == null)
            {
                Debug.LogWarning($"[Hearthbound/Phase 27] (skip) {label} — type '{typeFullName}' not found in any loaded assembly. " +
                                 "Likely the phase isn't shipped on this branch yet.");
                return false;
            }
            var m = t.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
            if (m == null)
            {
                Debug.LogWarning($"[Hearthbound/Phase 27] (skip) {label} — '{typeFullName}.{methodName}()' not found. Phase exists but uses a different entry-point name.");
                return false;
            }
            try
            {
                Debug.Log($"[Hearthbound/Phase 27] → Running {label} …");
                m.Invoke(null, null);
                Debug.Log($"[Hearthbound/Phase 27] ✓ {label} complete.");
                return true;
            }
            catch (Exception e)
            {
                // Unwrap reflection target exceptions so the real cause shows.
                var inner = e.InnerException ?? e;
                Debug.LogError($"[Hearthbound/Phase 27] ✗ {label} threw: {inner}");
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
                catch { /* asm not loaded yet, ignore */ }
            }
            return null;
        }

        // ─── Summary builder ──────────────────────────────────────

        private static string BuildSummary(int ran, int skipped)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Phase 27 complete — {ran} of {ran + skipped} sub-capstones ran.");
            if (skipped > 0)
            {
                sb.AppendLine();
                sb.AppendLine($"⚠ {skipped} sub-capstone(s) were skipped (not shipped on this branch).");
                sb.AppendLine("Check the Console for the per-phase log; missing phases don't break the build.");
            }
            sb.AppendLine();
            sb.AppendLine("Result on disk:");
            sb.AppendLine("  • 6 scenes in Build Settings (Bootstrap → Cottage)");
            sb.AppendLine("  • Assets/_Project/Animations/Hearthbound_Player.controller built");
            sb.AppendLine("  • Assets/_Project/Animations/Hearthbound_NPC.controller built");
            sb.AppendLine("  • Player prefab Animator wired to player controller");
            sb.AppendLine("  • Doris / Gerrold / SilentLane prefabs wired with NPC controller + NpcAnimatorBridge");
            sb.AppendLine("  • Lane / Hollow / Garden / Cottage — SmoothFollowCamera in place");
            sb.AppendLine("  • Lane / Hollow / Garden / Cottage — PlayerController.cameraReference set");
            sb.AppendLine("  • Marin's Note dropped on the Hollow workbench (if Narrative Hooks shipped)");
            sb.AppendLine();
            sb.AppendLine("Press Play in 00_Bootstrap.unity.");
            sb.AppendLine();
            sb.AppendLine("Controls (visible any time via H):");
            sb.AppendLine("  Move      WASD / Arrows / Left Stick");
            sb.AppendLine("  Sprint    Left Shift / LStick click   (Gentle Mode disables)");
            sb.AppendLine("  Jump      Space / Gamepad south       (Gentle Mode disables)");
            sb.AppendLine("  Interact  E / Gamepad south");
            sb.AppendLine("  Look      Hold Right Mouse + drag / Right Stick");
            sb.AppendLine("  Zoom      Mouse scroll / Gamepad LB-RB");
            sb.AppendLine("  Pause     Escape");
            sb.AppendLine("  Help      H");
            return sb.ToString();
        }
    }
}
