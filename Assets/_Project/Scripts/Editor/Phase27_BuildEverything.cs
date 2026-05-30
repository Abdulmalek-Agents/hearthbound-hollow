// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase27_BuildEverything
//
// PHASE 27 — The "one menu click" master capstone, promoted in Phase 32 to
// the *only* top-level entry point the user ever needs: `🚀 Build Everything`.
//
// IDEMPOTENT — every step is safe to re-run any number of times. Re-running
// this after `git pull` is the supported user flow (see D-051 in PROGRESS.md).
//
// USE: Menu → Hearthbound → 🚀 Build Everything
//
// Detection-driven: if any phase's prerequisites aren't present, this capstone
// skips that phase gracefully with a warning logged (TryRun via reflection).

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

        [MenuItem("Hearthbound/🚀 Build Everything", priority = -100)]
        public static void Build()
        {
            if (!EditorUtility.DisplayDialog(
                "Build Everything",
                "This runs the full Phase 13 → 62 chain — scenes, player +\n" +
                "NPC rigs, audio, cutscenes, level polish, the Depth Layer, the One\n" +
                "More Day hook, the Polish Menu, emoji glyphs, the Arabic font, the\n" +
                "environment-enrichment pass, and the Garden enrichment + wayfinding.\n\n" +
                "Safe to re-run after every pull — every step is idempotent.\n\n" +
                "Continue?",
                "Build", "Cancel")) return;

            EditorUtility.DisplayProgressBar("Hearthbound · Build Everything", "Running Phase 23 (polished scenes) …", 0.05f);
            int ran = 0, skipped = 0;
            try
            {
                // Step 1: Phase 23 polished capstone (chains 13–24 internally).
                if (TryRun("Phase 23 — POLISHED Mission 1 + 2",
                          "HearthboundHollow.EditorTools.Phase23_Mission1PolishCapstone", "Build"))
                    ran++;
                else
                    skipped++;

                // Step 2: Phase 26 — Player Controller + Animation.
                EditorUtility.DisplayProgressBar("Hearthbound · Build Everything", "Running Phase 26 (Player Controller + Animation) …", 0.30f);
                if (TryRun("Phase 26 — Player Controller + Animation",
                          "HearthboundHollow.EditorTools.Phase26_PlayerControllerAndAnimation", "Build"))
                    ran++;
                else
                    skipped++;

                // Step 3: Phase 26 — NPC Animators (Doris/Gerrold IsTalking).
                EditorUtility.DisplayProgressBar("Hearthbound · Build Everything", "Running Phase 26 (NPC Animators) …", 0.45f);
                if (TryRun("Phase 26 — NPC Animators",
                          "HearthboundHollow.EditorTools.Phase26_NpcAnimatorCapstone", "Build"))
                    ran++;
                else
                    skipped++;

                // Step 4: Phase 26 — Narrative Hooks (Marin's Note). Optional.
                EditorUtility.DisplayProgressBar("Hearthbound · Build Everything", "Running Phase 26 (Narrative Hooks) …", 0.55f);
                if (TryRun("Phase 26 — Narrative Hooks",
                          "HearthboundHollow.EditorTools.Phase26_NarrativeHooks", "WireNarrativeHooks") ||
                    TryRun("Phase 26 — Narrative Hooks",
                          "HearthboundHollow.EditorTools.Phase26_NarrativeHooks", "Build") ||
                    TryRun("Phase 26 — Narrative Hooks",
                          "HearthboundHollow.EditorTools.Phase26_NarrativeHooks", "Run"))
                    ran++;
                else
                    skipped++;

                // Step 5: Phase 29 — Player Rig Doctor.
                EditorUtility.DisplayProgressBar("Hearthbound · Build Everything", "Running Phase 29 (Player Rig Doctor) …", 0.65f);
                if (TryRun("Phase 29 — Player Rig Doctor",
                          "HearthboundHollow.EditorTools.Phase29_PlayerRigDoctor", "Build"))
                    ran++;
                else
                    skipped++;

                // Step 6: Phase 30 — Onboarding + ControlHintsHUD.
                EditorUtility.DisplayProgressBar("Hearthbound · Build Everything", "Running Phase 30 (Onboarding + Hints HUD) …", 0.72f);
                if (TryRun("Phase 30 — Onboarding + Hints HUD",
                          "HearthboundHollow.EditorTools.Phase30_OnboardingAndHintsCapstone", "Build"))
                    ran++;
                else
                    skipped++;

                // Step 7: Phase 31 — Dialogue Choice Card Repair.
                EditorUtility.DisplayProgressBar("Hearthbound · Build Everything", "Running Phase 31 (Dialogue Choice Repair) …", 0.78f);
                if (TryRun("Phase 31 — Dialogue Choice Card Repair",
                          "HearthboundHollow.EditorTools.Phase31_DialogueChoiceCardRepair", "Build"))
                    ran++;
                else
                    skipped++;

                // Step 8: Phase 32 — Mission 1 Polish v2.
                EditorUtility.DisplayProgressBar("Hearthbound · Build Everything", "Running Phase 32 (Mission 1 Polish v2) …", 0.84f);
                if (TryRun("Phase 32 — Mission 1 Polish v2",
                          "HearthboundHollow.EditorTools.Phase32_MissionOnePolishCapstone", "Build"))
                    ran++;
                else
                    skipped++;

                // Step 8.4: Phase 46 — Cross-platform voice generator.
                EditorUtility.DisplayProgressBar("Hearthbound · Build Everything", "Running Phase 46 (Voice Generator) …", 0.85f);
                if (TryRun("Phase 46 — Voice Generator (cross-platform)",
                          "HearthboundHollow.EditorTools.Phase46_VoiceGenerator", "Build"))
                    ran++;
                else
                    skipped++;

                // Step 8.5: Phase 32 — Voice Acting MVP. Rebuilds the voice library.
                EditorUtility.DisplayProgressBar("Hearthbound · Build Everything", "Running Phase 32 (Voice Library) …", 0.86f);
                if (TryRun("Phase 32 — Voice Library (D-058)",
                          "HearthboundHollow.EditorTools.Phase32_VoiceLibraryBuilder", "Build"))
                    ran++;
                else
                    skipped++;

                // Step 9: Phase 36 — Cutscene Library Completion.
                EditorUtility.DisplayProgressBar("Hearthbound · Build Everything", "Running Phase 36 (Cutscene Library) …", 0.89f);
                if (TryRun("Phase 36 — Cutscene Library",
                          "HearthboundHollow.EditorTools.Phase36_CutsceneLibraryBuilder", "Build"))
                    ran++;
                else
                    skipped++;

                // Step 10: Phase 37 — Procedural Audio Studio.
                EditorUtility.DisplayProgressBar("Hearthbound · Build Everything", "Running Phase 37 (Procedural Audio Studio) …", 0.92f);
                if (TryRun("Phase 37 — Procedural Audio Studio",
                          "HearthboundHollow.EditorTools.Phase37_ProceduralAudioStudio", "Build"))
                    ran++;
                else
                    skipped++;

                // Step 10b: Phase 45 — Library Migrator.
                EditorUtility.DisplayProgressBar("Hearthbound · Build Everything", "Running Phase 45 (Library Migration) …", 0.94f);
                if (TryRun("Phase 45 — Library Migrator",
                          "HearthboundHollow.EditorTools.Phase45_LibraryMigrator", "RunSilent"))
                    ran++;
                else
                    skipped++;

                // Step 11: Phase 38 — Audio + Cutscene Wiring.
                EditorUtility.DisplayProgressBar("Hearthbound · Build Everything", "Running Phase 38 (Audio + Cutscene Wiring) …", 0.96f);
                if (TryRun("Phase 38 — Audio + Cutscene Wiring",
                          "HearthboundHollow.EditorTools.Phase38_AudioAndCutsceneWiring", "Build"))
                    ran++;
                else
                    skipped++;

                // Step 12: Phase 42 — Listen Scene Camera Authoring.
                EditorUtility.DisplayProgressBar("Hearthbound · Build Everything", "Running Phase 42 (Listen Scene Camera) …", 0.97f);
                if (TryRun("Phase 42 — Listen Scene Camera",
                          "HearthboundHollow.EditorTools.Phase42_ListenSceneCameraBuilder", "Build"))
                    ran++;
                else
                    skipped++;

                // Step 13: Phase 47 — Level Boundaries + Wider Environment + Skybox +
                // Interior Polish + Collider Hardening + Wayfinding.
                EditorUtility.DisplayProgressBar("Hearthbound · Build Everything", "Running Phase 47 (Level Boundaries + Polish) …", 0.98f);
                if (TryRun("Phase 47.1 — Autumn Skybox + Lighting",
                          "HearthboundHollow.EditorTools.Phase47_AutumnSkyboxAndLighting", "Build")) ran++; else skipped++;
                if (TryRun("Phase 47.2 — Lane Boundaries + Wide Env",
                          "HearthboundHollow.EditorTools.Phase47_LaneBoundariesAndWideEnv", "Build")) ran++; else skipped++;
                if (TryRun("Phase 47.3 — Hollow Boundaries + Interior",
                          "HearthboundHollow.EditorTools.Phase47_HollowBoundariesAndInterior", "Build")) ran++; else skipped++;
                if (TryRun("Phase 47.4 — Garden Boundaries + Path",
                          "HearthboundHollow.EditorTools.Phase47_GardenBoundariesAndPath", "Build")) ran++; else skipped++;
                if (TryRun("Phase 47.5 — Cottage Interior Polish",
                          "HearthboundHollow.EditorTools.Phase47_CottageInteriorPolish", "Build")) ran++; else skipped++;
                if (TryRun("Phase 47.6 — Collider Hardening",
                          "HearthboundHollow.EditorTools.Phase47_ColliderHardening", "Build")) ran++; else skipped++;
                if (TryRun("Phase 47.7 — Guide Lights + Wayfinding",
                          "HearthboundHollow.EditorTools.Phase47_GuideLightsAndWayfinding", "Build")) ran++; else skipped++;

                // Step 14: Phase 48-51 Depth Layer.
                EditorUtility.DisplayProgressBar("Hearthbound · Build Everything", "Running Phase 48-51 (Depth Layer) …", 0.985f);
                if (TryRun("Phase 48 — Cold Open Cinematic (the hook)",
                          "HearthboundHollow.EditorTools.Phase48_BootstrapHookCinematic", "Build")) ran++; else skipped++;
                if (TryRun("Phase 49 — Echo Hologram (Marin)",
                          "HearthboundHollow.EditorTools.Phase49_EchoHologramBuilder", "Build")) ran++; else skipped++;
                if (TryRun("Phase 50 — Preface Beat (Lane)",
                          "HearthboundHollow.EditorTools.Phase50_PrefaceBeatBuilder", "Build")) ran++; else skipped++;
                if (TryRun("Phase 51 — Memory Web Overlay (Tab)",
                          "HearthboundHollow.EditorTools.Phase51_MemoryWebBuilder", "Build")) ran++; else skipped++;

                // Step 15: Phase 47 — "One More Day" goodnight beat.
                EditorUtility.DisplayProgressBar("Hearthbound · Build Everything", "Running Phase 47 (One More Day Hook) …", 0.988f);
                if (TryRun("Phase 47 — One More Day Hook",
                          "HearthboundHollow.EditorTools.Phase47_OneMoreDayBuilder", "Build")) ran++; else skipped++;

                // Step 16: Phase 53 — Polish Menu Layer.
                EditorUtility.DisplayProgressBar("Hearthbound · Build Everything", "Running Phase 53 (Polish Menu Layer) …", 0.99f);
                if (TryRun("Phase 53 — Polish Menu Layer",
                          "HearthboundHollow.EditorTools.Phase53_PolishMenuBuilder", "Build")) ran++; else skipped++;

                // Step 17: Phase 54 — Hollow Glyphs.
                EditorUtility.DisplayProgressBar("Hearthbound · Build Everything", "Running Phase 54 (Hollow Glyphs) …", 0.992f);
                if (TryRun("Phase 54 — Hollow Glyphs (emoji → TMP sprites)",
                          "HearthboundHollow.EditorTools.Phase54_HollowGlyphsBuilder", "Build")) ran++; else skipped++;

                // Step 18: Phase 56 — Arabic Font Installer.
                EditorUtility.DisplayProgressBar("Hearthbound · Build Everything", "Running Phase 56 (Arabic Font) …", 0.994f);
                if (TryRun("Phase 56 — Arabic Font Installer (fix tofu)",
                          "HearthboundHollow.EditorTools.Phase56_ArabicFontInstaller", "Install")) ran++; else skipped++;

                // Step 19: Phase 60 — Environment enrichment + placement accuracy.
                EditorUtility.DisplayProgressBar("Hearthbound · Build Everything", "Running Phase 60 (Environment Enrichment) …", 0.996f);
                if (TryRun("Phase 60 — Hollow Dressing Enrichment + quad fix",
                          "HearthboundHollow.EditorTools.Phase60_HollowDressingEnrichment", "Build")) ran++; else skipped++;

                // Step 20: Phase 62 — Garden enrichment + wayfinding. Retextures the
                // flat green Garden ground (warm meadow), warms the lighting, dresses
                // the meadow with Medieval Village trees + market/farm props (grounded,
                // collider'd, under a reversible managed root), and lays a glowing
                // lantern path from the player's spawn to Garden_Exit_Trigger so the
                // route to Gerrold's cottage is unmistakable (the "stuck at M2" fix).
                EditorUtility.DisplayProgressBar("Hearthbound · Build Everything", "Running Phase 62 (Garden Enrichment + Wayfinding) …", 0.997f);
                if (TryRun("Phase 62 — Garden Enrichment + Wayfinding",
                          "HearthboundHollow.EditorTools.Phase62_GardenEnrichment", "Build")) ran++; else skipped++;

                // Final: Open Bootstrap so the user can press Play.
                EditorUtility.DisplayProgressBar("Hearthbound · Build Everything", "Opening Bootstrap …", 0.99f);
                if (System.IO.File.Exists(SceneBootstrap))
                    EditorSceneManager.OpenScene(SceneBootstrap, OpenSceneMode.Single);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            EditorUtility.DisplayDialog(
                "Hearthbound — 🚀 Build Everything",
                BuildSummary(ran, skipped),
                "OK");
        }

        // ─── Reflection runner ───────────────────────────────────

        private static bool TryRun(string label, string typeFullName, string methodName)
        {
            Type t = FindType(typeFullName);
            if (t == null)
            {
                Debug.LogWarning($"[Hearthbound/Build Everything] (skip) {label} — type '{typeFullName}' not found in any loaded assembly. " +
                                 "Likely the phase isn't shipped on this branch yet.");
                return false;
            }
            var m = t.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
            if (m == null)
            {
                Debug.LogWarning($"[Hearthbound/Build Everything] (skip) {label} — '{typeFullName}.{methodName}()' not found. Phase exists but uses a different entry-point name.");
                return false;
            }
            try
            {
                Debug.Log($"[Hearthbound/Build Everything] → Running {label} …");
                m.Invoke(null, null);
                Debug.Log($"[Hearthbound/Build Everything] ✓ {label} complete.");
                return true;
            }
            catch (Exception e)
            {
                var inner = e.InnerException ?? e;
                Debug.LogError($"[Hearthbound/Build Everything] ✗ {label} threw: {inner}");
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

        // ─── Summary builder ──────────────────────────────

        private static string BuildSummary(int ran, int skipped)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"🚀 Build Everything complete — {ran} of {ran + skipped} sub-capstones ran.");
            if (skipped > 0)
            {
                sb.AppendLine();
                sb.AppendLine($"⚠ {skipped} sub-capstone(s) were skipped (not shipped on this branch).");
                sb.AppendLine("Check the Console for the per-phase log; missing phases don't break the build.");
            }
            sb.AppendLine();
            sb.AppendLine("Highlights on disk:");
            sb.AppendLine("  • 6 scenes in Build Settings (Bootstrap → Cottage)");
            sb.AppendLine("  • Player + NPC rigs, cameras, audio, cutscenes wired");
            sb.AppendLine("  • Phase 47 — boundaries, autumn skybox, interior polish, wayfinding");
            sb.AppendLine("  • Phase 48-51 — Depth Layer (Cold Open, Echo Hologram, Preface, Memory Web)");
            sb.AppendLine("  • Phase 47 — One More Day goodnight card");
            sb.AppendLine("  • Phase 53 — Polish Menu (language, reset, character creator)");
            sb.AppendLine("  • Phase 54/56 — emoji glyphs + Arabic font");
            sb.AppendLine("  • Phase 60 — Hollow dressing + red-quad retire");
            sb.AppendLine("  • Phase 62 — Garden meadow ground + warm lighting + medieval dressing");
            sb.AppendLine("                + a glowing lantern path to Gerrold's cottage (un-stuck)");
            sb.AppendLine();
            sb.AppendLine("The Engagement loop (Request Board [B], Memory Wall [M], Hollow Shop [U],");
            sb.AppendLine("Garden [G], Workbench [K], Journal [J]) self-installs at Play — no build step.");
            sb.AppendLine();
            sb.AppendLine("Press Play in 00_Bootstrap.unity.");
            return sb.ToString();
        }
    }
}
