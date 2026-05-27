// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase46_VoiceGenerator
//
// Phase 46 — Cross-platform voice generation.
//
// The existing `Tools/generate_voices.sh` invokes macOS `say` + `afconvert`
// to produce Doris's 48 voice clips. That's perfect for the M1 dev rig but
// silently fails on Linux + Windows.
//
// Phase 46's fix: this Editor menu drives the SAME 48-line script through
// `espeak-ng` (open-source, cross-platform; apt/brew/choco installable in
// 30 seconds) via System.Diagnostics.Process. Same output: 22 kHz mono
// PCM16 .wav files written to Assets/_Project/Audio/Voice/Doris/{lineId}.wav.
// Same downstream wiring: Phase 32 VoiceLibraryBuilder scans the folder
// and populates HearthboundVoiceLibrary.asset; VoicePlayer.Awake's
// Resources.Load picks it up; DialogueUI plays the line.
//
// macOS users with `say` already installed are unaffected — the bash
// script still works. Phase 46 is the *Linux + Windows + macOS-without-say*
// path.
//
// Voice casting (per D-058 + Tools/generate_voices.sh comments, mapped to
// espeak-ng voice variants):
//   Doris    — en-us+f3   (mid-range warm female, contralto)
//   Gerrold  — en-gb+m3   (weathered British male)
//   Marin    — en+f2      (soft alto)
//   Pickle   — en+f5      (bright high feminine cat narrator)
//
// USE: Menu → Hearthbound → ⚙️ Advanced → 🎙️ Phase 46 — Generate Voices (cross-platform)
//
// Chained from `🚀 Build Everything` so the canonical one-click workflow
// generates the WAVs if espeak-ng is on PATH, then rebuilds the library.
// If espeak-ng is NOT on PATH, the chain skips this step with a clear
// console message — the user gets typewriter-only dialogue and a hint
// at how to install espeak-ng.

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace HearthboundHollow.EditorTools
{
    public static class Phase46_VoiceGenerator
    {
        public const string VoiceRoot = "Assets/_Project/Audio/Voice";
        public const string DorisDir  = VoiceRoot + "/Doris";

        // Doris's 48 lines — copied verbatim from Tools/generate_voices.sh.
        // Format: lineId | line_text
        public static readonly (string id, string text)[] DorisLines = new (string, string)[]
        {
            ("doris_m1_greet_01",        "You're the new one."),
            ("doris_m1_greet_02",        "I thought you'd be taller."),
            ("doris_m1_greet_03",        "Don't mind me — I thought that about the old one, too."),
            ("doris_m1_greet_04",        "Come in. The kettle's only just stopped."),
            ("doris_m1_reply_help_01",   "Aye. The very same."),
            ("doris_m1_reply_help_02",   "They've put my name on the sign and everything. Look — there."),
            ("doris_m1_reply_silent_01", "A quiet one, then. Good."),
            ("doris_m1_reply_silent_02", "The bread likes quiet."),
            ("doris_m1_reply_unsure_01", "... Mm."),
            ("doris_m1_reply_unsure_02", "That's a conversation for a longer day."),
            ("doris_m1_reply_unsure_03", "Come in. Tea first."),
            ("doris_m1_kitchen_01",      "Mind the flour."),
            ("doris_m1_kitchen_02",      "I haven't swept since Tuesday. I keep meaning to."),
            ("doris_m1_kitchen_03",      "... The shop next door is yours. The Hollow."),
            ("doris_m1_kitchen_04",      "I've been keeping the key safe for you."),
            ("doris_m1_offer_01",        "... I have something for you. Before you go in."),
            ("doris_m1_offer_02",        "I'd like to be your first customer, if that's all right."),
            ("doris_m1_memory_01",       "This is the memory."),
            ("doris_m1_memory_02",       "Hold it like you'd hold a hot bun. Not by the side. Underneath."),
            ("doris_m1_memory_03",       "It's a small thing."),
            ("doris_m1_memory_04",       "First time I made bread that didn't shame me."),
            ("doris_m1_memory_05",       "Most days I think of it."),
            ("doris_m1_memory_06",       "I want to put it down, now, for a while."),
            ("doris_m1_memory_07",       "Will you take it?"),
            ("doris_m1_defer_01",        "Aye. Some days are not the day."),
            ("doris_m1_defer_02",        "I'll be here when one is."),
            ("doris_m1_story_01",        "I was twenty-four."),
            ("doris_m1_story_02",        "The oven was new. The bricks were new. I was new."),
            ("doris_m1_story_03",        "I'd been baking other people's bread for nine years."),
            ("doris_m1_story_04",        "That morning was the first morning that was just mine."),
            ("doris_m1_story_05",        "I want to take a rest from carrying it. That's all."),
            ("doris_m1_price_01",        "Four coppers, if you're asking."),
            ("doris_m1_price_02",        "It's a small memory. I'll not have you overpay your first day."),
            ("doris_m1_price_fair",      "Aye. Thank you."),
            ("doris_m1_price_high_01",   "That's too much. I'll not have you ruin yourself."),
            ("doris_m1_price_high_02",   "Take it back. — Well. Take some back."),
            ("doris_m1_price_high_03",   "Five, then. Final."),
            ("doris_m1_price_low_01",    "..."),
            ("doris_m1_price_low_02",    "Aye, that'll do. Bring the rest when you find some."),
            ("doris_m1_handover_01",     "There."),
            ("doris_m1_handover_02",     "The old keeper showed me how to make it. Took me four tries."),
            ("doris_m1_handover_03",     "I cracked the first three. The cat watched me. Judged me, I think."),
            ("doris_m1_handover_04",     "I'll be in the bakery if you want me. Knock twice."),
            ("doris_m1_handover_05",     "There's a kettle on the workbench. Mind the wood stove — it bites."),
            ("doris_m1_polish_watch",    "I'll wait. Take your time, Keeper."),
            ("doris_m1_polish_done_01",  "Aye."),
            ("doris_m1_polish_done_02",  "There it is. That's the morning."),
            ("doris_m1_polish_sleep_01", "Sleep tonight. Dreams come."),
            ("doris_m1_polish_sleep_02", "I'll see you again, eventually."),
        };

        // Voice configuration — single source of truth.
        // espeak-ng voice variant ("+f3" = female variant 3), words-per-minute,
        // pitch (0-99, espeak default 50 = neutral).
        public const string DorisEspeakVoice = "en-us+f3";
        public const int    DorisWpm         = 150;
        public const int    DorisPitch       = 50;

        // ─── Menu entry ───────────────────────────────────────────

        [MenuItem("Hearthbound/⚙️ Advanced/🎙️ Phase 46 — Generate Voices (cross-platform)", priority = 1199)]
        public static void RunFromMenu() => Run(verbose: true);

        /// <summary>
        /// Silent entry-point invoked by the `🚀 Build Everything` chain.
        /// </summary>
        public static void Build() => Run(verbose: false);

        public static void Run(bool verbose)
        {
            string tool = ResolveTtsTool();
            if (tool == null)
            {
                string msg =
                    "Phase 46 — espeak-ng not found on PATH.\n\n" +
                    "Voice generation is OPTIONAL — typewriter dialogue still works without it.\n\n" +
                    "To enable spoken dialogue, install espeak-ng:\n" +
                    "  macOS:   brew install espeak-ng     (or use Tools/generate_voices.sh + `say`)\n" +
                    "  Linux:   sudo apt install espeak-ng\n" +
                    "  Windows: choco install espeak-ng    (or scoop install espeak-ng)\n" +
                    "Then re-run Hearthbound → 🚀 Build Everything.";
                Debug.LogWarning("[Hearthbound/Phase 46] " + msg);
                if (verbose) EditorUtility.DisplayDialog("Phase 46 — Voice Generator", msg, "OK");
                return;
            }

            EnsureFolder(DorisDir);

            int generated = 0, skipped = 0, failed = 0;
            try
            {
                for (int i = 0; i < DorisLines.Length; i++)
                {
                    if (i % 4 == 0)
                    {
                        EditorUtility.DisplayProgressBar(
                            "Hearthbound · Phase 46 — Voice Generation",
                            $"{DorisLines[i].id} …",
                            i / (float)DorisLines.Length);
                    }
                    var (id, text) = DorisLines[i];
                    string wavPath = $"{DorisDir}/{id}.wav";

                    // Idempotent — keep existing files. Delete + re-run to refresh.
                    if (File.Exists(wavPath))
                    {
                        skipped++;
                        continue;
                    }

                    if (Synthesize(tool, text, wavPath))
                        generated++;
                    else
                        failed++;
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            AssetDatabase.Refresh();

            // After importing the new .wavs, rebuild the voice library SO.
            // The Phase 32 builder is idempotent + preserves inspector tunings.
            if (generated > 0)
            {
                Phase32_VoiceLibraryBuilder.Build();
            }

            string summary =
                $"Phase 46 — Voice Generation\n\n" +
                $"  Tool:      {tool}\n" +
                $"  Voice:     {DorisEspeakVoice} (Doris contralto, {DorisWpm} wpm)\n" +
                $"  Output:    {DorisDir}/*.wav\n\n" +
                $"  {generated} generated\n" +
                $"  {skipped}   skipped (already exist)\n" +
                $"  {failed}   failed (see Console)\n\n" +
                (generated > 0
                    ? "Phase 32 — Rebuild Voice Library was auto-run.\n" +
                      "Press Play; Doris's lines now speak via Resources/HearthboundVoiceLibrary."
                    : (skipped == DorisLines.Length
                        ? "All voices already present. To regenerate, delete the .wav files in\n" +
                          $"{DorisDir}/ and re-run this menu item."
                        : "Nothing was generated. Check the Console for espeak-ng errors."));

            Debug.Log("[Hearthbound/Phase 46] " + summary);
            if (verbose)
                EditorUtility.DisplayDialog("Phase 46 — Voice Generator", summary, "OK");
        }

        // ─── espeak-ng (or `say`) wrapper ────────────────────────

        /// <summary>
        /// Try to locate espeak-ng (preferred — cross-platform) or fall back
        /// to macOS `say`. Returns the executable name (looked up via PATH)
        /// or null if neither tool is available.
        /// </summary>
        private static string ResolveTtsTool()
        {
            if (IsOnPath("espeak-ng")) return "espeak-ng";
            if (IsOnPath("espeak"))    return "espeak";
            if (IsOnPath("say"))       return "say";  // macOS native
            return null;
        }

        private static bool IsOnPath(string exe)
        {
            try
            {
                using var p = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = Application.platform == RuntimePlatform.WindowsEditor ? "where" : "which",
                        Arguments = exe,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                    },
                };
                p.Start();
                p.WaitForExit(2000);
                return p.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Synthesise `text` into a 22 kHz mono PCM16 .wav at `wavPath`.
        /// Returns true on success.
        /// </summary>
        private static bool Synthesize(string tool, string text, string wavPath)
        {
            try
            {
                ProcessStartInfo psi;

                if (tool == "espeak-ng" || tool == "espeak")
                {
                    // espeak-ng writes a 22 kHz mono PCM16 WAV by default with -w.
                    psi = new ProcessStartInfo
                    {
                        FileName = tool,
                        Arguments = $"-v {DorisEspeakVoice} -s {DorisWpm} -p {DorisPitch} -w \"{wavPath}\" \"{EscapeForShell(text)}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                    };
                }
                else if (tool == "say")
                {
                    // macOS `say` writes AIFF, then we need afconvert. Mirror the bash script.
                    string aiff = Path.GetTempFileName() + ".aiff";
                    var sayPsi = new ProcessStartInfo
                    {
                        FileName = "say",
                        Arguments = $"-v Samantha -r 180 -o \"{aiff}\" \"{EscapeForShell(text)}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                    };
                    using (var sayP = Process.Start(sayPsi))
                    {
                        sayP.WaitForExit(10000);
                        if (sayP.ExitCode != 0) { File.Delete(aiff); return false; }
                    }
                    psi = new ProcessStartInfo
                    {
                        FileName = "afconvert",
                        Arguments = $"\"{aiff}\" \"{wavPath}\" -f WAVE -d LEI16@22050 -c 1",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                    };
                    using (var convP = Process.Start(psi))
                    {
                        convP.WaitForExit(10000);
                        bool ok = convP.ExitCode == 0;
                        try { File.Delete(aiff); } catch { }
                        return ok;
                    }
                }
                else
                {
                    return false;
                }

                using var p = Process.Start(psi);
                p.WaitForExit(10000);
                if (p.ExitCode != 0)
                {
                    string err = p.StandardError.ReadToEnd();
                    Debug.LogWarning($"[Hearthbound/Phase 46] {tool} exit {p.ExitCode} for '{wavPath}': {err}");
                    return false;
                }
                return File.Exists(wavPath);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Hearthbound/Phase 46] Synthesise failed for {wavPath}: {e.Message}");
                return false;
            }
        }

        private static string EscapeForShell(string s)
        {
            // espeak-ng + macOS say both accept the text as a single positional argument.
            // We use ProcessStartInfo (not bash), so we only need to escape internal
            // double-quotes inside the line. The em-dash and ellipses are fine UTF-8.
            return s.Replace("\"", "\\\"");
        }

        // ─── Folder helper ────────────────────────────────────────

        private static void EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath)) return;
            string parent = Path.GetDirectoryName(assetPath).Replace('\\', '/');
            string leaf   = Path.GetFileName(assetPath);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
