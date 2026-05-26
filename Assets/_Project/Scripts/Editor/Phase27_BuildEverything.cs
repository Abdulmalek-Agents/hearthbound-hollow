// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase27_BuildEverything
//
// PHASE 27 — The "one menu click" master capstone, promoted in Phase 32 to
// the *only* top-level entry point the user ever needs: `🚀 Build Everything`.
//
// Phase 23 builds the polished scenes. Phase 26 (Player Controller + Animation)
// builds the AnimatorController + upgrades cameras. Phase 26 (NPC Animators)
// wires Doris/Gerrold's IsTalking dialogue beats. Phase 26 (Narrative Hooks)
// drops Marin's Note on the workbench. Phase 29 runs the Player Rig Doctor
// foot-bone anchor pass. Phase 30 wires the OnboardingOverlay + ControlHintsHUD.
// Phase 31 surgically repairs the dialogue choice cards. Phase 32 layers the
// Mission 1 polish v2 (cottages, facade, hearth dressing, cozy URP volumes).
// Phase 36 builds the Dream 2 + Listen Timeline library. Phase 37 generates the
// procedural audio (music, ambience, missing SFX, mumble VO). Phase 38 wires
// every audio asset to the scenes + dialogue UI + cutscene timelines.
// Phase 42 wires the Listen Scene Camera Director onto the Cottage scene.
// Phase 27 chains them all — single click, ~95 s, fully wired.
//
// IDEMPOTENT — every step is safe to re-run any number of times. Re-running
// this after `git pull` is the supported user flow (see D-051 in PROGRESS.md).
//
// USE: Menu → Hearthbound → 🚀 Build Everything
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
        //
        // Promoted in Phase 32 to the single top-level user entry point.
        // Priority -100 puts it above every other Hearthbound menu item.
        // The legacy `✨ Build EVERYTHING (Phase 27 — one click)` label has
        // been retired; the same method now backs `🚀 Build Everything`.
        // (D-051 — only three top-level entries are allowed.)

        [MenuItem("Hearthbound/🚀 Build Everything", priority = -100)]
        public static void Build()
        {
            // Phase 32 — Safety note. The chain rebuilds 6 scenes + every
            // capstone-managed prefab. Re-running is safe (idempotent) but
            // any *manual* scene tweaks made directly to the GameObjects
            // owned by the chain (e.g. `_Phase27Env_Lane`, the auto-built
            // cottage prefabs, the Player AnimatorController) will be
            // overwritten. Inspector overrides on user-owned GameObjects
            // and on the per-scene Player / NPC instances survive because
            // every chained phase uses load-or-create + heal-then-save.
            if (!EditorUtility.DisplayDialog(
                "Build Everything",
                "This runs the full Phase 13 → 42 chain (~95 s).\n" +
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

                // Step 4: Phase 26 — Narrative Hooks (Marin's Note). Optional;
                // not all branches have this thread.
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

                // Step 9: Phase 36 — Cutscene Library Completion. Builds
                // Dream 2 variants A/B/C/D/E + Listen scene Timelines and
                // re-wires the MemoryDreamRig prefab so the missing 6
                // PlayableAsset slots are filled (per D-052).
                EditorUtility.DisplayProgressBar("Hearthbound · Build Everything", "Running Phase 36 (Cutscene Library) …", 0.89f);
                if (TryRun("Phase 36 — Cutscene Library",
                          "HearthboundHollow.EditorTools.Phase36_CutsceneLibraryBuilder", "Build"))
                    ran++;
                else
                    skipped++;

                // Step 10: Phase 37 — Procedural Audio Studio. Imports the
                // pre-generated music + ambience + SFX + mumble VO WAVs into
                // the project, builds MusicLibrarySO + AmbienceLibrarySO +
                // MumbleVoiceLibrarySO, refreshes SfxLibrary so the
                // previously-empty entries (polish_hum_*, ambient_autumn_loop)
                // are now mapped. Idempotent.
                EditorUtility.DisplayProgressBar("Hearthbound · Build Everything", "Running Phase 37 (Procedural Audio Studio) …", 0.93f);
                if (TryRun("Phase 37 — Procedural Audio Studio",
                          "HearthboundHollow.EditorTools.Phase37_ProceduralAudioStudio", "Build"))
                    ran++;
                else
                    skipped++;

                // Step 11: Phase 38 — Audio + Cutscene Wiring. Binds the
                // Phase 37 audio assets to the Phase 36 Timeline AudioTracks,
                // attaches AmbientAudio + MusicPlayer to every scene, and
                // wires MumbleVoicePlayer onto the DialogueUI prefab.
                EditorUtility.DisplayProgressBar("Hearthbound · Build Everything", "Running Phase 38 (Audio + Cutscene Wiring) …", 0.95f);
                if (TryRun("Phase 38 — Audio + Cutscene Wiring",
                          "HearthboundHollow.EditorTools.Phase38_AudioAndCutsceneWiring", "Build"))
                    ran++;
                else
                    skipped++;

                // Step 12: Phase 42 — Listen Scene Camera Authoring. Drops
                // ListenSceneCameraDirector on the Cottage scene, wires it to
                // the ListenSceneSequencer, and configures the 4-waypoint
                // camera path (30s wide → 60s chair → 60s hands → 30s pull-back).
                EditorUtility.DisplayProgressBar("Hearthbound · Build Everything", "Running Phase 42 (Listen Scene Camera) …", 0.98f);
                if (TryRun("Phase 42 — Listen Scene Camera",
                          "HearthboundHollow.EditorTools.Phase42_ListenSceneCameraBuilder", "Build"))
                    ran++;
                else
                    skipped++;

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
                // Unwrap reflection target exceptions so the real cause shows.
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

        // ─── Summary builder ──────────────────────────────────────

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
            sb.AppendLine("Result on disk:");
            sb.AppendLine("  • 6 scenes in Build Settings (Bootstrap → Cottage)");
            sb.AppendLine("  • Assets/_Project/Animations/Hearthbound_Player.controller built");
            sb.AppendLine("  • Assets/_Project/Animations/Hearthbound_NPC.controller built");
            sb.AppendLine("  • Player prefab Animator wired to player controller + foot-bone anchor (Phase 29)");
            sb.AppendLine("  • Doris / Gerrold / SilentLane prefabs wired with NPC controller + NpcAnimatorBridge");
            sb.AppendLine("  • Lane / Hollow / Garden / Cottage — SmoothFollowCamera in place");
            sb.AppendLine("  • Lane / Hollow / Garden / Cottage — PlayerController.cameraReference set");
            sb.AppendLine("  • Marin's Note dropped on the Hollow workbench (if Narrative Hooks shipped)");
            sb.AppendLine("  • Lane — OnboardingOverlay (6-step walkthrough on first play)");
            sb.AppendLine("  • Every gameplay scene — ControlHintsHUD (always-visible key chips)");
            sb.AppendLine("  • Phase 32 v2 — 8 cottages + Hollow facade + cozy URP volume (if shipped)");
            sb.AppendLine("  • Phase 36 — MemoryDreamRig.prefab wired with Dream 1 + 5× Dream 2 variants + Listen scene Timelines");
            sb.AppendLine("  • Phase 37 — MusicLibrarySO, AmbienceLibrarySO, MumbleVoiceLibrarySO built");
            sb.AppendLine("                + 30+ procedural WAV cues in Assets/_Project/Audio/Generated/");
            sb.AppendLine("  • Phase 38 — Per-scene MusicPlayer + AmbientAudio attached, DialogueUI MumbleVoicePlayer wired");
            sb.AppendLine("  • Phase 42 — ListenSceneCameraDirector on Cottage scene with 4-waypoint cinematic path");
            sb.AppendLine();
            sb.AppendLine("Press Play in 00_Bootstrap.unity.");
            sb.AppendLine();
            sb.AppendLine("Controls (visible any time via H, also via the on-screen ControlHintsHUD):");
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
