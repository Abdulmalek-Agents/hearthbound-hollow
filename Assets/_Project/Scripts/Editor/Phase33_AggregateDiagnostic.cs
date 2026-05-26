// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase33_AggregateDiagnostic
//
// Phase 33 — Menu Collapse — Top-level aggregate diagnostic.
//
// Before Phase 33, three per-phase diagnostics each had their own top-level
// menu item:
//   • Hearthbound/🔍 Diagnose Phase 23 Build  (Phase23_DiagnosticReport)
//   • Hearthbound/🔍 Diagnose Phase 26 Build  (Phase26_DiagnosticReport)
//   • Hearthbound/🔍 Phase 32 — Diagnose Mission 1 Polish (Phase32_Diagnostic)
//
// Phase 33's menu-collapse design moves all three under
// `Hearthbound/⚙️ Advanced/…` and surfaces this aggregator at the top level
// as `Hearthbound/🔍 Diagnose Build` (priority -90). One click runs all
// three diagnostics in sequence and reports a combined verdict.
//
// Phase 35 update (2026-05-26): added a 4th step that runs the Continuation
// Audit (Phase35_FlatEntryAudit) so `🔍 Diagnose Build` now ALSO reports on
// cutscene timelines, audio folders, SfxLibrary entries, Yarn files, seed
// assets, and the D-051 top-level menu reservation policy.
//
// Phase 40 update (2026-05-26): added a 5th step that runs the audio-only
// diagnostic (Phase40_AudioDiagnostic) — verifies MusicLibrarySO,
// AmbienceLibrarySO, MumbleVoiceLibrarySO, DreamAudioBinder cueMap, and
// per-scene SceneAudioBeacon wiring.
//
// READ-ONLY. Never modifies any asset. Safe to run any number of times.
//
// USE: Menu → Hearthbound → 🔍 Diagnose Build
//
// Architecture notes (per D-055):
//   - The top-level menu is reserved for three blessed user entry points:
//     `🚀 Build Everything`, `🔍 Diagnose Build`, and the `⚙️ Advanced ►`
//     submenu (implicit). All new diagnostics MUST register under
//     `Hearthbound/⚙️ Advanced/…` and be invoked by this aggregator if they
//     are part of the canonical health check.

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace HearthboundHollow.EditorTools
{
    public static class Phase33_AggregateDiagnostic
    {
        [MenuItem("Hearthbound/🔍 Diagnose Build", priority = -90)]
        public static void Run()
        {
            int ran = 0, skipped = 0;
            try
            {
                EditorUtility.DisplayProgressBar("Hearthbound · Diagnose Build",
                    "Step 1/5 — Phase 23 diagnostic …", 0.05f);
                if (TryRun("Phase 23 — Scene/Wiring Diagnostic",
                          "HearthboundHollow.EditorTools.Phase23_DiagnosticReport", "Run") ||
                    TryRun("Phase 23 — Scene/Wiring Diagnostic",
                          "HearthboundHollow.EditorTools.Phase23_DiagnosticReport", "Diagnose"))
                    ran++;
                else
                    skipped++;

                EditorUtility.DisplayProgressBar("Hearthbound · Diagnose Build",
                    "Step 2/5 — Phase 26 diagnostic (Animator + Player + cameras) …", 0.25f);
                if (TryRun("Phase 26 — Player + Animator Diagnostic",
                          "HearthboundHollow.EditorTools.Phase26_DiagnosticReport", "Run") ||
                    TryRun("Phase 26 — Player + Animator Diagnostic",
                          "HearthboundHollow.EditorTools.Phase26_DiagnosticReport", "Diagnose"))
                    ran++;
                else
                    skipped++;

                EditorUtility.DisplayProgressBar("Hearthbound · Diagnose Build",
                    "Step 3/5 — Phase 32 diagnostic (Mission 1 polish v2) …", 0.45f);
                if (TryRun("Phase 32 — Mission 1 Polish Diagnostic",
                          "HearthboundHollow.EditorTools.Phase32_Diagnostic", "Diagnose") ||
                    TryRun("Phase 32 — Mission 1 Polish Diagnostic",
                          "HearthboundHollow.EditorTools.Phase32_Diagnostic", "Run"))
                    ran++;
                else
                    skipped++;

                // Phase 35 — Continuation Audit (cutscene timelines, audio
                // folders, SfxLibrary, Yarn, seed-assets, top-level menu
                // entry policy per D-051). Chained here so the aggregate
                // diagnostic ALSO reports on the Phase 36 / 37 deliverables
                // before the audio + cutscene work has fully landed.
                EditorUtility.DisplayProgressBar("Hearthbound · Diagnose Build",
                    "Step 4/5 — Phase 35 continuation audit (cutscenes + audio + menus) …", 0.65f);
                if (TryRun("Phase 35 — Flat Entry Audit",
                          "HearthboundHollow.EditorTools.Phase35_FlatEntryAudit", "Run"))
                    ran++;
                else
                    skipped++;

                // Phase 40 — Audio-specific diagnostic (per-library health
                // check + DreamAudioBinder cueMap verification + scene-beacon
                // text-grep). Catches audio regressions even when the Phase
                // 35 audit is satisfied.
                EditorUtility.DisplayProgressBar("Hearthbound · Diagnose Build",
                    "Step 5/5 — Phase 40 audio-wiring diagnostic …", 0.85f);
                if (TryRun("Phase 40 — Audio Wiring Diagnostic",
                          "HearthboundHollow.EditorTools.Phase40_AudioDiagnostic", "Run"))
                    ran++;
                else
                    skipped++;

                EditorUtility.DisplayProgressBar("Hearthbound · Diagnose Build",
                    "Aggregating results …", 0.95f);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            var summary = $"🔍 Diagnose Build — {ran} of {ran + skipped} sub-diagnostics ran.\n\n" +
                          "Each sub-diagnostic opens its own dialog with the per-area report.\n" +
                          "Check the Unity Console for the consolidated text log.";

            if (skipped > 0)
                summary += $"\n\n⚠ {skipped} sub-diagnostic(s) skipped (not present on this branch).";

            Debug.Log("[Hearthbound/Diagnose Build] " + summary);
            EditorUtility.DisplayDialog("Diagnose Build — complete", summary, "OK");
        }

        // ───────────────────────────────────────────────────────────────
        // Reflection runner — same pattern as Phase27_BuildEverything
        // ───────────────────────────────────────────────────────────────

        private static bool TryRun(string label, string typeFullName, string methodName)
        {
            Type t = FindType(typeFullName);
            if (t == null)
            {
                Debug.LogWarning($"[Hearthbound/Diagnose Build] (skip) {label} — type '{typeFullName}' missing.");
                return false;
            }
            var m = t.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
            if (m == null) return false;
            try
            {
                Debug.Log($"[Hearthbound/Diagnose Build] → Running {label} …");
                m.Invoke(null, null);
                Debug.Log($"[Hearthbound/Diagnose Build] ✓ {label} complete.");
                return true;
            }
            catch (Exception e)
            {
                var inner = e.InnerException ?? e;
                Debug.LogError($"[Hearthbound/Diagnose Build] ✗ {label} threw: {inner}");
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
                catch { /* skip */ }
            }
            return null;
        }
    }
}
