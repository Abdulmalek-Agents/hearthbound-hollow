// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase32_MissionOnePolishCapstone
//
// Phase 32.5 — The one-click master for Mission 1 Polish v2.
//
// Chains the full Phase 32 series:
//   1. Phase 32.1 — Extended village bindings
//   2. Phase 32.1 — Cottage prefab assembler (4 cottage variants)
//   3. Phase 32.2 — Lane v2 (8 cottages, Hollow facade, beehive, atmosphere)
//   4. Phase 32.3 — Hollow Interior v2 (kettle, bread, herbs, cupboard, etc.)
//   5. Phase 32.4 — Cozy URP volumes + Global Volume in each scene
//   6. Phase 27.4 — Re-runs lantern + hearth interactable wiring (catches
//                   any new lanterns Phase 32.2 placed)
//   7. Phase 31   — Re-runs dialogue choice card repair (defensive)
//
// IDEMPOTENT — every sub-phase is safe to re-run.
//
// USE: Menu → Hearthbound → 🍂 Phase 32 — Polish Mission 1 (v2 — all phases)
//
// Detection-driven: missing phases skip gracefully via reflection (same
// pattern as Phase27_BuildEverything.cs).

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace HearthboundHollow.EditorTools
{
    public static class Phase32_MissionOnePolishCapstone
    {
        [MenuItem("Hearthbound/⚙️ Advanced/🍂 Phase 32 — Polish Mission 1 (v2 — all phases)", priority = 5)]
        public static void Build()
        {
            int ran = 0, skipped = 0;

            try
            {
                EditorUtility.DisplayProgressBar("Hearthbound · Phase 32",
                    "Step 1/7 — Cataloging extended village bindings …", 0.05f);
                if (TryRun("Phase 32.1 — Extended Bindings",
                          "HearthboundHollow.EditorTools.Phase32_VillageBindingsExtension", "Build")) ran++;
                else skipped++;

                EditorUtility.DisplayProgressBar("Hearthbound · Phase 32",
                    "Step 2/7 — Assembling cottage prefab variants …", 0.18f);
                if (TryRun("Phase 32.1 — Cottage Assembler",
                          "HearthboundHollow.EditorTools.Phase32_MedievalCottageBuilder", "Build")) ran++;
                else skipped++;

                EditorUtility.DisplayProgressBar("Hearthbound · Phase 32",
                    "Step 3/7 — Polishing Lane environment v2 …", 0.34f);
                if (TryRun("Phase 32.2 — Lane Environment V2",
                          "HearthboundHollow.EditorTools.Phase32_LaneEnvironmentV2", "Build")) ran++;
                else skipped++;

                EditorUtility.DisplayProgressBar("Hearthbound · Phase 32",
                    "Step 4/7 — Polishing Hollow interior v2 …", 0.50f);
                if (TryRun("Phase 32.3 — Hollow Interior V2",
                          "HearthboundHollow.EditorTools.Phase32_HollowInteriorV2", "Build")) ran++;
                else skipped++;

                EditorUtility.DisplayProgressBar("Hearthbound · Phase 32",
                    "Step 5/7 — Applying cozy URP volumes …", 0.66f);
                if (TryRun("Phase 32.4 — Cozy URP Volume",
                          "HearthboundHollow.EditorTools.Phase32_CozyVolumeBuilder", "Build")) ran++;
                else skipped++;

                EditorUtility.DisplayProgressBar("Hearthbound · Phase 32",
                    "Step 6/7 — Re-wiring Phase 27 lantern + hearth interactables …", 0.80f);
                if (TryRun("Phase 27 — Environment Capstone (re-run)",
                          "HearthboundHollow.EditorTools.Phase27_EnvironmentCapstone", "Build")) ran++;
                else skipped++;

                EditorUtility.DisplayProgressBar("Hearthbound · Phase 32",
                    "Step 7/7 — Re-running Phase 31 dialogue repair …", 0.92f);
                if (TryRun("Phase 31 — Dialogue Choice Card Repair",
                          "HearthboundHollow.EditorTools.Phase31_DialogueChoiceCardRepair", "Build")) ran++;
                else skipped++;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            EditorUtility.DisplayDialog("Phase 32 — Mission 1 Polish v2 complete",
                BuildSummary(ran, skipped),
                "OK");
        }

        private static bool TryRun(string label, string typeFullName, string methodName)
        {
            Type t = FindType(typeFullName);
            if (t == null)
            {
                Debug.LogWarning($"[Hearthbound/Phase 32] (skip) {label} — type '{typeFullName}' missing.");
                return false;
            }
            var m = t.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
            if (m == null)
            {
                Debug.LogWarning($"[Hearthbound/Phase 32] (skip) {label} — '{methodName}()' not found.");
                return false;
            }
            try
            {
                Debug.Log($"[Hearthbound/Phase 32] → Running {label} …");
                m.Invoke(null, null);
                Debug.Log($"[Hearthbound/Phase 32] ✓ {label} complete.");
                return true;
            }
            catch (Exception e)
            {
                var inner = e.InnerException ?? e;
                Debug.LogError($"[Hearthbound/Phase 32] ✗ {label} threw: {inner}");
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

        private static string BuildSummary(int ran, int skipped)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"🍂 Phase 32 — Mission 1 Polish v2 — {ran} of {ran + skipped} sub-phases ran.");
            if (skipped > 0)
            {
                sb.AppendLine();
                sb.AppendLine($"⚠ {skipped} sub-phase(s) skipped — check Console for details.");
            }
            sb.AppendLine();
            sb.AppendLine("Result:");
            sb.AppendLine("  Lane scene:");
            sb.AppendLine("    • 8 residential cottages from 4 prefab variants");
            sb.AppendLine("    • Hollow shop facade wraps the door + 'The Hollow' sign");
            sb.AppendLine("    • Doris's bakery, beehive, hay bale, apple basket, firewood");
            sb.AppendLine("    • 3 extra lantern posts + chimney smoke on every roof");
            sb.AppendLine("    • Distant autumn alders, pebbles, mushrooms, dead tree silhouette");
            sb.AppendLine("    • Cozy warm dusk URP volume (bloom + tonemap + warm tint)");
            sb.AppendLine();
            sb.AppendLine("  Hollow scene:");
            sb.AppendLine("    • Kettle on hearth + steam wisp (off by default)");
            sb.AppendLine("    • Bread loaves on west shelf");
            sb.AppendLine("    • 3 hanging dried herbs (lavender / valerian / sage)");
            sb.AppendLine("    • Marin's cupboard + stool by hearth");
            sb.AppendLine("    • Candelabra on workbench + cup + bowl + apple");
            sb.AppendLine("    • Water bucket beside hearth");
            sb.AppendLine("    • Wall candle sconces on west wall");
            sb.AppendLine("    • Larger pulse glow at the hearth");
            sb.AppendLine("    • Cozy interior firelight URP volume");
            sb.AppendLine();
            sb.AppendLine("Open Hearthbound → 🔍 Phase 32 — Diagnose Mission 1 Polish");
            sb.AppendLine("to audit the wiring is intact.");
            sb.AppendLine();
            sb.AppendLine("Then press Play in 00_Bootstrap.unity.");
            return sb.ToString();
        }
    }
}
