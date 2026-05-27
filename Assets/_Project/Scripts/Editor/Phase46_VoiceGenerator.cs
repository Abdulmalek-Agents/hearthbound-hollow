// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase46_VoiceGenerator
//
// Phase 46 — Cross-platform voice generation (espeak-ng / espeak / say).
//
// Sister pipeline to Tools/generate_voices.sh (Piper TTS, D-059). Piper
// produces higher-quality neural voices but requires `pip install piper-tts`
// + ~250 MB of model downloads. Phase 46 is the lighter alternative:
// espeak-ng (apt/brew/choco installable in 30 seconds, ~2 MB) drives
// the same 77 dialogue lines through System.Diagnostics.Process from
// inside the Unity Editor so the `🚀 Build Everything` chain can
// generate voices on Linux + Windows out of the box.
//
// Both pipelines write to the same canonical paths:
//   Assets/_Project/Audio/Voice/<Character>/<lineId>.wav
//   (22 kHz mono PCM16 — Unity-native)
// so Phase32_VoiceLibraryBuilder.cs picks them up either way and
// VoicePlayer.Play(lineId) resolves them via the same SO. The runtime
// can't tell which pipeline produced the .wav — that's D-058 (the
// file-swap policy).
//
// ── Phase 46.1 (2026-05-27) ─────────────────────────────────────
// Extended from 48 → 77 lines and from 1 → 5 characters:
//   Doris    (55 lines) — en-us+f3 contralto, 150 wpm, pitch 50
//   Gerrold  ( 8 lines) — en-gb+m3 weathered baritone, 145 wpm, pitch 35
//   Marin    ( 4 lines) — en+f2    soft whisper, 130 wpm, pitch 55
//   Narrator ( 4 lines) — en-gb+f1 neutral British female, 155 wpm, pitch 50
//   Pickle   ( 6 lines) — en+f5    bright high feminine, 180 wpm, pitch 65
// The 6 new Doris lineIds added in Phase 32.9 (refused-path × 3,
// polish-quality × 3) are included so this pipeline stays in lockstep
// with Mission01Director.cs and Tools/generate_voices.sh.
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

        /// <summary>
        /// Per-character voice configuration. See Docs/VOICE_CASTING.md
        /// for the canonical casting table (the Piper choices are the
        /// neural counterparts of these espeak-ng variants).
        /// </summary>
        public readonly struct VoiceConfig
        {
            public readonly string Character;
            public readonly string EspeakVoice;  // espeak-ng -v variant
            public readonly int    Wpm;          // espeak-ng -s
            public readonly int    Pitch;        // espeak-ng -p (0-99, 50 = neutral)
            public readonly string SayVoice;     // macOS `say -v` fallback voice
            public readonly int    SayRate;      // macOS `say -r` words-per-minute

            public VoiceConfig(string c, string ev, int wpm, int p, string sv, int sr)
            { Character = c; EspeakVoice = ev; Wpm = wpm; Pitch = p; SayVoice = sv; SayRate = sr; }
        }

        public static readonly VoiceConfig[] Voices = new VoiceConfig[]
        {
            // Character | espeak-ng voice | wpm | pitch | say voice | say rate
            new VoiceConfig("Doris",    "en-us+f3", 150, 50, "Samantha", 180),
            new VoiceConfig("Gerrold",  "en-gb+m3", 145, 35, "Daniel",   170),
            new VoiceConfig("Marin",    "en+f2",    130, 55, "Tessa",    160),
            new VoiceConfig("Narrator", "en-gb+f1", 155, 50, "Karen",    175),
            new VoiceConfig("Pickle",   "en+f5",    180, 65, "Samantha", 200),
        };

        /// <summary>
        /// All 77 voice lines across the 5 characters. Doris's 55 ids match
        /// Mission01Director.cs's `Line(... lineId)` calls verbatim. The
        /// other characters' ids are pre-recorded for Mission 2 + future
        /// content (the runtime picks them up automatically once
        /// Mission02Director / cutscenes call PresentLine with that id).
        /// Synced with Tools/generate_voices.sh's LINES table.
        /// </summary>
        public static readonly (string character, string id, string text)[] Lines = new (string, string, string)[]
        {
            // ── DORIS · Mission 1 · 55 lines (full coverage) ────────────
            ("Doris", "doris_m1_greet_01",                  "You're the new one."),
            ("Doris", "doris_m1_greet_02",                  "I thought you'd be taller."),
            ("Doris", "doris_m1_greet_03",                  "Don't mind me — I thought that about the old one, too."),
            ("Doris", "doris_m1_greet_04",                  "Come in. The kettle's only just stopped."),
            ("Doris", "doris_m1_reply_help_01",             "Aye. The very same."),
            ("Doris", "doris_m1_reply_help_02",             "They've put my name on the sign and everything. Look — there."),
            ("Doris", "doris_m1_reply_silent_01",           "A quiet one, then. Good."),
            ("Doris", "doris_m1_reply_silent_02",           "The bread likes quiet."),
            ("Doris", "doris_m1_reply_unsure_01",           "... Mm."),
            ("Doris", "doris_m1_reply_unsure_02",           "That's a conversation for a longer day."),
            ("Doris", "doris_m1_reply_unsure_03",           "Come in. Tea first."),
            ("Doris", "doris_m1_kitchen_01",                "Mind the flour."),
            ("Doris", "doris_m1_kitchen_02",                "I haven't swept since Tuesday. I keep meaning to."),
            ("Doris", "doris_m1_kitchen_03",                "... The shop next door is yours. The Hollow."),
            ("Doris", "doris_m1_kitchen_04",                "I've been keeping the key safe for you."),
            ("Doris", "doris_m1_offer_01",                  "... I have something for you. Before you go in."),
            ("Doris", "doris_m1_offer_02",                  "I'd like to be your first customer, if that's all right."),
            ("Doris", "doris_m1_memory_01",                 "This is the memory."),
            ("Doris", "doris_m1_memory_02",                 "Hold it like you'd hold a hot bun. Not by the side. Underneath."),
            ("Doris", "doris_m1_memory_03",                 "It's a small thing."),
            ("Doris", "doris_m1_memory_04",                 "First time I made bread that didn't shame me."),
            ("Doris", "doris_m1_memory_05",                 "Most days I think of it."),
            ("Doris", "doris_m1_memory_06",                 "I want to put it down, now, for a while."),
            ("Doris", "doris_m1_memory_07",                 "Will you take it?"),
            ("Doris", "doris_m1_defer_01",                  "Aye. Some days are not the day."),
            ("Doris", "doris_m1_defer_02",                  "I'll be here when one is."),
            ("Doris", "doris_m1_story_01",                  "I was twenty-four."),
            ("Doris", "doris_m1_story_02",                  "The oven was new. The bricks were new. I was new."),
            ("Doris", "doris_m1_story_03",                  "I'd been baking other people's bread for nine years."),
            ("Doris", "doris_m1_story_04",                  "That morning was the first morning that was just mine."),
            ("Doris", "doris_m1_story_05",                  "I want to take a rest from carrying it. That's all."),
            ("Doris", "doris_m1_price_01",                  "Four coppers, if you're asking."),
            ("Doris", "doris_m1_price_02",                  "It's a small memory. I'll not have you overpay your first day."),
            ("Doris", "doris_m1_price_fair",                "Aye. Thank you."),
            ("Doris", "doris_m1_price_high_01",             "That's too much. I'll not have you ruin yourself."),
            ("Doris", "doris_m1_price_high_02",             "Take it back. — Well. Take some back."),
            ("Doris", "doris_m1_price_high_03",             "Five, then. Final."),
            ("Doris", "doris_m1_price_low_01",              "..."),
            ("Doris", "doris_m1_price_low_02",              "Aye, that'll do. Bring the rest when you find some."),
            ("Doris", "doris_m1_handover_01",               "There."),
            ("Doris", "doris_m1_handover_02",               "The old keeper showed me how to make it. Took me four tries."),
            ("Doris", "doris_m1_handover_03",               "I cracked the first three. The cat watched me. Judged me, I think."),
            ("Doris", "doris_m1_handover_04",               "I'll be in the bakery if you want me. Knock twice."),
            ("Doris", "doris_m1_handover_05",               "There's a kettle on the workbench. Mind the wood stove — it bites."),
            ("Doris", "doris_m1_polish_watch",              "I'll wait. Take your time, Keeper."),
            ("Doris", "doris_m1_polish_done_01",            "Aye."),
            ("Doris", "doris_m1_polish_done_02",            "There it is. That's the morning."),
            ("Doris", "doris_m1_polish_sleep_01",           "Sleep tonight. Dreams come."),
            ("Doris", "doris_m1_polish_sleep_02",           "I'll see you again, eventually."),
            // ── Phase 32.9 — refused-path (3) ──────────────────────────
            ("Doris", "doris_m1_refused_01",                "The shop's still yours."),
            ("Doris", "doris_m1_refused_02",                "Go in. Sit a while. The kettle is on."),
            ("Doris", "doris_m1_refused_03",                "I'll be here when you're ready."),
            // ── Phase 32.9 — clarity-branching after-polish (3) ────────
            ("Doris", "doris_m1_polish_after_perfect",      "You did it cleaner than I remembered it. I think you'll do."),
            ("Doris", "doris_m1_polish_after_acceptable",   "You did it kindly. That's what matters."),
            ("Doris", "doris_m1_polish_after_mild",         "... It's the morning still. A little dimmer. But mine. First days are like that. I won't hold it."),

            // ── GERROLD · Mission 2 stub (Depth Bible § 2.2) ──────────
            ("Gerrold", "gerrold_m2_greet_01",       "I'm sorry. I don't know how this is supposed to go."),
            ("Gerrold", "gerrold_m2_greet_02",       "I have the — the thing — I have it in this cloth."),
            ("Gerrold", "gerrold_m2_greet_03",       "Margery wrapped it. I think she wrapped it for this."),
            ("Gerrold", "gerrold_m2_long_bit_01",    "I want to keep my wife. I do not want to keep the long bit."),
            ("Gerrold", "gerrold_m2_long_bit_02",    "It's not the dying part. It's the long bit."),
            ("Gerrold", "gerrold_m2_thank_01",       "Thank you. I do not know whether you have done what I asked."),
            ("Gerrold", "gerrold_m2_thank_02",       "I think you have done what you could."),
            ("Gerrold", "gerrold_m2_thank_03",       "I will go home and see what the morning brings."),

            // ── MARIN · predecessor notes (4) ─────────────────────────
            ("Marin", "marin_note_lane_01",          "If you find this, the kettle still works."),
            ("Marin", "marin_note_lane_02",          "Don't trust the third shelf. It tilts."),
            ("Marin", "marin_note_hollow_01",        "Pickle remembers everyone. Pickle is fair."),
            ("Marin", "marin_note_workbench_01",     "The cloth is for handling the warm orbs. Mine is in the drawer."),

            // ── NARRATOR · title cards (4) ────────────────────────────
            ("Narrator", "narrator_title_day1",      "Day One. The Hollow."),
            ("Narrator", "narrator_title_day2",      "Day Two. The Garden."),
            ("Narrator", "narrator_title_evening",   "Evening falls. The kettle is warm."),
            ("Narrator", "narrator_title_dream",     "She closes her eyes. The memory begins."),

            // ── PICKLE · italic asides (6) ────────────────────────────
            ("Pickle", "pickle_m1_aside_01",         "Mmm. New one."),
            ("Pickle", "pickle_m1_aside_02",         "She watches you. She always watches."),
            ("Pickle", "pickle_m1_aside_03",         "The bread likes you. So does she, I think."),
            ("Pickle", "pickle_m2_aside_01",         "He brings a cloth. He never used to bring a cloth."),
            ("Pickle", "pickle_m2_aside_02",         "Choose softly. I am watching."),
            ("Pickle", "pickle_m2_aside_03",         "You did kindly. I will remember."),
        };

        // Back-compat constants — old call-sites that reference these
        // constants (e.g. legacy diagnostic logs) still compile. Doris's
        // settings are the canonical default; the per-character table
        // above is the real source of truth.
        public const string DorisEspeakVoice = "en-us+f3";
        public const int    DorisWpm         = 150;
        public const int    DorisPitch       = 50;

        // For users who want the old single-folder constants.
        public const string DorisDir = VoiceRoot + "/Doris";

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
                    "  macOS:   brew install espeak-ng\n" +
                    "  Linux:   sudo apt install espeak-ng\n" +
                    "  Windows: choco install espeak-ng    (or scoop install espeak-ng)\n" +
                    "Then re-run Hearthbound → 🚀 Build Everything.\n\n" +
                    "Or use the higher-quality Piper TTS pipeline (D-059):\n" +
                    "  bash Tools/download_voice_models.sh\n" +
                    "  bash Tools/generate_voices.sh\n" +
                    "  Hearthbound → ⚙️ Advanced → 🎙️ Phase 32 — Rebuild Voice Library";
                Debug.LogWarning("[Hearthbound/Phase 46] " + msg);
                if (verbose) EditorUtility.DisplayDialog("Phase 46 — Voice Generator", msg, "OK");
                return;
            }

            // Build a quick character → config map.
            var cfgByChar = new Dictionary<string, VoiceConfig>(Voices.Length);
            foreach (var v in Voices) cfgByChar[v.Character] = v;

            // Ensure all per-character folders exist (5 of them).
            foreach (var v in Voices) EnsureFolder($"{VoiceRoot}/{v.Character}");

            var generated  = new Dictionary<string, int>();
            var skipped    = new Dictionary<string, int>();
            var failed     = new Dictionary<string, int>();
            foreach (var v in Voices)
            {
                generated[v.Character] = 0;
                skipped[v.Character]   = 0;
                failed[v.Character]    = 0;
            }

            try
            {
                for (int i = 0; i < Lines.Length; i++)
                {
                    var (character, id, text) = Lines[i];

                    if (i % 6 == 0)
                    {
                        EditorUtility.DisplayProgressBar(
                            "Hearthbound · Phase 46 — Voice Generation",
                            $"{character}/{id} …",
                            i / (float)Lines.Length);
                    }

                    if (!cfgByChar.TryGetValue(character, out var cfg))
                    {
                        Debug.LogWarning($"[Hearthbound/Phase 46] no voice config for '{character}' — skip {id}");
                        continue;
                    }

                    string wavPath = $"{VoiceRoot}/{character}/{id}.wav";

                    // Idempotent — keep existing files. Delete + re-run to refresh.
                    if (File.Exists(wavPath))
                    {
                        skipped[character]++;
                        continue;
                    }

                    if (Synthesize(tool, cfg, text, wavPath))
                        generated[character]++;
                    else
                        failed[character]++;
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            AssetDatabase.Refresh();

            // After importing the new .wavs, rebuild the voice library SO.
            // The Phase 32 builder is idempotent + preserves inspector tunings.
            int totalGenerated = 0;
            foreach (var v in Voices) totalGenerated += generated[v.Character];
            if (totalGenerated > 0)
            {
                Phase32_VoiceLibraryBuilder.Build();
            }

            // Build the human-readable summary.
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Phase 46 — Voice Generation");
            sb.AppendLine();
            sb.AppendLine($"  Tool:   {tool}");
            sb.AppendLine($"  Output: {VoiceRoot}/<Character>/*.wav");
            sb.AppendLine();
            int totG = 0, totS = 0, totF = 0;
            foreach (var v in Voices)
            {
                int g = generated[v.Character];
                int s = skipped[v.Character];
                int f = failed[v.Character];
                totG += g; totS += s; totF += f;
                sb.AppendLine($"  {v.Character,-10} gen={g,3}  skip={s,3}  fail={f,3}   ({v.EspeakVoice}, {v.Wpm} wpm, p{v.Pitch})");
            }
            sb.AppendLine();
            sb.AppendLine($"  TOTAL      gen={totG,3}  skip={totS,3}  fail={totF,3}");
            sb.AppendLine();
            if (totalGenerated > 0)
                sb.AppendLine("Phase 32 — Rebuild Voice Library was auto-run.\n" +
                              "Press Play; voiced characters now speak via Resources/HearthboundVoiceLibrary.");
            else if (totS == Lines.Length)
                sb.AppendLine("All voices already present. To regenerate, delete the .wav files in\n" +
                              $"{VoiceRoot}/<Character>/ and re-run this menu item.");
            else
                sb.AppendLine("Nothing was generated. Check the Console for espeak-ng errors.");

            string summary = sb.ToString();
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
        /// Synthesise `text` into a 22 kHz mono PCM16 .wav at `wavPath` using
        /// the per-character voice config. Returns true on success.
        /// </summary>
        private static bool Synthesize(string tool, VoiceConfig cfg, string text, string wavPath)
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
                        Arguments = $"-v {cfg.EspeakVoice} -s {cfg.Wpm} -p {cfg.Pitch} -w \"{wavPath}\" \"{EscapeForShell(text)}\"",
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
                        Arguments = $"-v {cfg.SayVoice} -r {cfg.SayRate} -o \"{aiff}\" \"{EscapeForShell(text)}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                    };
                    using (var sayP = Process.Start(sayPsi))
                    {
                        sayP.WaitForExit(10000);
                        if (sayP.ExitCode != 0) { try { File.Delete(aiff); } catch { } return false; }
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
