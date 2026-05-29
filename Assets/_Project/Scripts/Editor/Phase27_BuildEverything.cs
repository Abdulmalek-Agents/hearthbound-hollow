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
// Mission 1 polish v2 (cottages, facade, hearth dressing, cozy URP volumes)
// AND, in the Voice Acting MVP track, rebuilds the Resources/HearthboundVoice
// Library.asset from any .wav files the user has dropped under
// Assets/_Project/Audio/Voice/ (per D-058). Phase 46 (Voice Generator) auto-
// generates those .wav files via espeak-ng if it's on PATH (cross-platform),
// so the voice library is no longer empty on Linux/Windows installs. Phase 36
// builds the Dream 2 + Listen Timeline library. Phase 37 generates the
// procedural audio (music, ambience, missing SFX, mumble VO). Phase 45 migrates
// the audio libraries to Resources/ for runtime Resources.Load fallback. Phase
// 38 wires every audio asset to the scenes + dialogue UI + cutscene timelines.
// Phase 42 wires the Listen Scene Camera Director onto the Cottage scene.
// Phase 47 layers the level boundaries, autumn skybox, interior polish,
// collider hardening, and onboarding wayfinding on top of everything — fixes
// the four pre-greenlight polish gaps (narrow lane, no boundaries, no skybox,
// thin cottage interior). PHASES 48-51 (NEW — the Depth Layer) add: a Cold
// Open cinematic on Bootstrap (the hook), Marin's Echo Hologram on the Hollow
// workbench (first predecessor encounter), a tone-personalized Preface Beat on
// the Lane scene (3 narrator lines), and a Tab-key Memory Web overlay with 4
// canonical Echo connections. Phase 27 chains them all — single click, ~115 s.
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
                "This runs the full Phase 13 → 51 chain (~115 s).\n" +
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

                // Step 8.4: Phase 46 — Cross-platform voice generator.
                EditorUtility.DisplayProgressBar("Hearthbound · Build Everything", "Running Phase 46 (Voice Generator) …", 0.85f);
                if (TryRun("Phase 46 — Voice Generator (cross-platform)",
                          "HearthboundHollow.EditorTools.Phase46_VoiceGenerator", "Build"))
                    ran++;
                else
                    skipped++;

                // Step 8.5: Phase 32 — Voice Acting MVP. Rebuilds
                // Resources/HearthboundVoiceLibrary.asset.
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

                // Step 13: Phase 47 — Level Boundaries + Wider Environment +
                // Skybox + Interior Polish + Collider Hardening + Wayfinding.
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

                // Step 14: Phase 48-51 Depth Layer (the Hook + deep system family).
                // - 48: Cold Open Cinematic on the Bootstrap scene (the hook).
                // - 49: Echo Hologram of Marin on the Hollow workbench.
                // - 50: Tone-Personalized Preface Beat on the Lane scene.
                // - 51: Memory Web Overlay on the Bootstrap scene (Tab to open).
                // Each phase is fully idempotent and re-runs cleanly on every
                // `git pull` cycle. Skipped gracefully when a phase isn't
                // shipped on this branch.
                EditorUtility.DisplayProgressBar("Hearthbound · Build Everything", "Running Phase 48-51 (Depth Layer) …", 0.985f);
                if (TryRun("Phase 48 — Cold Open Cinematic (the hook)",
                          "HearthboundHollow.EditorTools.Phase48_BootstrapHookCinematic", "Build")) ran++; else skipped++;
                if (TryRun("Phase 49 — Echo Hologram (Marin)",
                          "HearthboundHollow.EditorTools.Phase49_EchoHologramBuilder", "Build")) ran++; else skipped++;
                if (TryRun("Phase 50 — Preface Beat (Lane)",
                          "HearthboundHollow.EditorTools.Phase50_PrefaceBeatBuilder", "Build")) ran++; else skipped++;
                if (TryRun("Phase 51 — Memory Web Overlay (Tab)",
                          "HearthboundHollow.EditorTools.Phase51_MemoryWebBuilder", "Build")) ran++; else skipped++;

                // Step 15: Phase 47 — "One More Day" goodnight beat (the cozy
                // "press continue WANTING tomorrow" retention hook). Builds the
                // two TomorrowTeaseSO assets + OneMoreDayCard.prefab, then wires
                // the EndOfDaySequencer (Ledger → Dream → Goodnight Card → load)
                // into the Hollow + Cottage scenes. Idempotent; opt-in at runtime
                // (directors fall back to the legacy path when unwired — D-064).
                EditorUtility.DisplayProgressBar("Hearthbound · Build Everything", "Running Phase 47 (One More Day Hook) …", 0.988f);
                if (TryRun("Phase 47 — One More Day Hook",
                          "HearthboundHollow.EditorTools.Phase47_OneMoreDayBuilder", "Build")) ran++; else skipped++;

                // Step 16: Phase 53 — Polish Menu Layer (language EN/العربية,
                // Reset Game → title, and the New-Game Character Creator
                // (skin / outfit / accessory / name). Builds the screens on the
                // Main Menu + every gameplay Pause, and the avatar appearance
                // applier. Idempotent; persists via SettingsService (PlayerPrefs).
                EditorUtility.DisplayProgressBar("Hearthbound · Build Everything", "Running Phase 53 (Polish Menu Layer) …", 0.99f);
                if (TryRun("Phase 53 — Polish Menu Layer",
                          "HearthboundHollow.EditorTools.Phase53_PolishMenuBuilder", "Build")) ran++; else skipped++;

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

        // ─── Summary builder ────────────────────────────────────

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
            sb.AppendLine("  • Phase 46 — Doris voice clips generated by espeak-ng if available (cross-platform)");
            sb.AppendLine("  • Phase 32 Voice MVP — Resources/HearthboundVoiceLibrary.asset built from Audio/Voice/**/*.wav (D-058)");
            sb.AppendLine("  • Phase 36 — MemoryDreamRig.prefab wired with Dream 1 + 5× Dream 2 variants + Listen scene Timelines");
            sb.AppendLine("  • Phase 37 — MusicLibrarySO, AmbienceLibrarySO, MumbleVoiceLibrarySO built");
            sb.AppendLine("                + 30+ procedural WAV cues in Assets/_Project/Audio/Generated/");
            sb.AppendLine("  • Phase 45 — Libraries migrated to Resources/ for runtime Resources.Load fallback");
            sb.AppendLine("  • Phase 38 — Per-scene MusicPlayer + AmbientAudio attached, DialogueUI MumbleVoicePlayer wired");
            sb.AppendLine("  • Phase 42 — ListenSceneCameraDirector on Cottage scene with 4-waypoint cinematic path");
            sb.AppendLine("  • Phase 47 — Autumn skybox, 24×36m Lane with stone-wall perimeter + void blockers,");
            sb.AppendLine("                Hollow + Cottage interior polish, cottage hearth flicker, guide lanterns,");
            sb.AppendLine("                firefly wayfinding, every prop has a Collider (no clipping)");
            sb.AppendLine("  • Phase 48 — Cold Open cinematic on Bootstrap (candle, Marin's letter, Pickle eyes, title)");
            sb.AppendLine("  • Phase 49 — Echo Hologram of Marin on the Hollow workbench (3-line monologue)");
            sb.AppendLine("  • Phase 50 — Tone-Personalized Preface Beat on Lane scene (3 narrator lines, skippable)");
            sb.AppendLine("  • Phase 51 — Memory Web overlay (Tab opens it, 4 canonical Echo connections)");
            sb.AppendLine("  • Phase 47 — One More Day goodnight card on Hollow + Cottage (Ledger → Dream → card → next day)");
            sb.AppendLine("  • Phase 53 — Polish Menu: Language (EN/العربية), Reset Game → title, Character Creator (skin/outfit/accessory/name)");
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
            sb.AppendLine("  Memory Web  Tab");
            return sb.ToString();
        }
    }
}
