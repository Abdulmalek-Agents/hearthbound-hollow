// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase37_ProceduralAudioStudio
//
// Phase 37 — Procedural Audio Studio.
//
// Pure-C# Editor builder that synthesises every music + ambience + missing-
// SFX + per-character mumble-VO clip the game needs, end-to-end, INSIDE
// Unity at Editor time. No Python, no DAW, no SaaS, no human composer.
// The output is a deterministic set of ~75 .wav files written into
// `Assets/_Project/Audio/Generated/`, ready for Unity's importer.
//
// The same synthesis algorithms also exist in Python form under
// `Tools/audio_generation/generate_audio.py` for transparency / external
// preview — that script and this Editor builder produce equivalent cues.
// The C# implementation is the canonical one (no external dependency).
//
// Synthesis primitives (full-fidelity port of the Python script):
//   - Additive sine harmonics for tonal voices (piano / cello / violin /
//     bell).
//   - Pink + brown noise for ambience beds.
//   - One-pole low-pass / high-pass filters.
//   - Sparse-tap Schroeder reverb (4-6 delays).
//   - Crossfade-loop helper for seamless ambient loops.
//   - ADSR envelopes for one-shots.
//   - Two-formant vowel approximation for mumble-VO phonemes.
//
// Output layout (per D-053):
//   Assets/_Project/Audio/Generated/
//     Music/              ← 12 music cues (main theme, motifs, scene themes)
//     Ambience/           ← 6 ambient beds (lane, hollow, garden, cottage, kettle, dream)
//     SFX/                ← 9 missing SFX (polish_hum_*, ambient_autumn, etc.)
//     Mumble/<character>/ ← 4 banks × 12 phonemes (doris, gerrold, pickle, marin)
//
// Idempotent — re-running this builder regenerates identical .wav files
// (same deterministic seed) and rebuilds the three libraries
// (MusicLibrarySO, AmbienceLibrarySO via SfxLibrarySO for short loops,
// MumbleVoiceLibrarySO) so the asset graph self-heals.
//
// USE: Menu → Hearthbound → ⚙️ Advanced → 🎵 Phase 37 — Procedural Audio Studio
//
// Chained from Phase 27 Build Everything (step 10/11).

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using HearthboundHollow.Audio;

namespace HearthboundHollow.EditorTools
{
    public static class Phase37_ProceduralAudioStudio
    {
        // ─── Folders + paths ──────────────────────────────────────

        public const string GenRoot       = "Assets/_Project/Audio/Generated";
        public const string GenMusic      = GenRoot + "/Music";
        public const string GenAmbience   = GenRoot + "/Ambience";
        public const string GenSFX        = GenRoot + "/SFX";
        public const string GenMumble     = GenRoot + "/Mumble";

        public const string MusicLibraryPath    = "Assets/_Project/Audio/MusicLibrary.asset";
        public const string AmbienceLibraryPath = "Assets/_Project/Audio/AmbienceLibrary.asset";
        public const string MumbleLibraryPath   = "Assets/_Project/Audio/MumbleVoiceLibrary.asset";
        public const string SfxLibraryPath      = "Assets/_Project/Audio/SfxLibrary.asset";

        // ─── Globals ──────────────────────────────────────────────

        private const int SR = 44_100;             // sample rate
        private const float HEADROOM = 0.85f;      // peak amplitude (~-1.4 dBFS)

        // Pitch table.
        private const float F3 = 174.61f;
        private const float A3 = 220.00f;
        private const float C4 = 261.63f;
        private const float E4 = 329.63f;
        private const float F4 = 349.23f;
        private const float G4 = 392.00f;
        private const float A4 = 440.00f;
        private const float B4_FLAT = 466.16f;
        private const float B4 = 493.88f;
        private const float C5 = 523.25f;
        private const float D5 = 587.33f;
        private const float E5 = 659.26f;
        private const float F5 = 698.46f;

        // 5-note "Hearthbound" motif in F major (Codex 14 § 7).
        private static readonly float[] HEARTHBOUND_MOTIF = { F4, A4, C5, A4, F4 };

        // ─── Menu ─────────────────────────────────────────────────

        [MenuItem("Hearthbound/⚙️ Advanced/🎵 Phase 37 — Procedural Audio Studio", priority = 220)]
        public static void Build()
        {
            try
            {
                EditorUtility.DisplayProgressBar("Hearthbound · Phase 37",
                    "Synthesising music cues …", 0.05f);
                EnsureFolders();

                int written = 0;
                int totalSteps = 32;  // approximate; for progress bar only
                int step = 0;

                // ─── Music ──────────────────────────────────────
                written += WriteCue("Music/main_theme_hearthbound.wav",    GenMainTheme());                 step++; Progress(step, totalSteps);
                written += WriteCue("Music/doris_motif_dream1.wav",        GenDorisMotif());                step++; Progress(step, totalSteps);
                written += WriteCue("Music/margery_motif_variant_a.wav",   GenMargeryVariantA());            step++; Progress(step, totalSteps);
                written += WriteCue("Music/margery_motif_variant_b.wav",   GenMargeryVariantB());            step++; Progress(step, totalSteps);
                written += WriteCue("Music/margery_motif_variant_c.wav",   GenMargeryVariantC());            step++; Progress(step, totalSteps);
                written += WriteCue("Music/margery_motif_variant_d.wav",   GenMargeryVariantD());            step++; Progress(step, totalSteps);
                written += WriteCue("Music/margery_motif_variant_e.wav",   GenMargeryVariantE());            step++; Progress(step, totalSteps);
                written += WriteCue("Music/scene_lane_theme.wav",          GenLaneTheme());                  step++; Progress(step, totalSteps);
                written += WriteCue("Music/scene_hollow_theme.wav",        GenHollowTheme());                step++; Progress(step, totalSteps);
                written += WriteCue("Music/scene_garden_theme.wav",        GenGardenTheme());                step++; Progress(step, totalSteps);
                written += WriteCue("Music/scene_cottage_theme.wav",       GenCottageTheme());               step++; Progress(step, totalSteps);
                written += WriteCue("Music/scene_menu_theme.wav",          GenMenuTheme());                  step++; Progress(step, totalSteps);

                // ─── Ambience ──────────────────────────────────
                written += WriteCue("Ambience/lane_autumn_loop.wav",       GenLaneAmbience());               step++; Progress(step, totalSteps);
                written += WriteCue("Ambience/hollow_hearth_loop.wav",     GenHollowAmbience());             step++; Progress(step, totalSteps);
                written += WriteCue("Ambience/garden_day_loop.wav",        GenGardenAmbience());             step++; Progress(step, totalSteps);
                written += WriteCue("Ambience/cottage_interior_loop.wav",  GenCottageAmbience());            step++; Progress(step, totalSteps);
                written += WriteCue("Ambience/kettle_steam_loop.wav",      GenKettleSteam());                step++; Progress(step, totalSteps);
                written += WriteCue("Ambience/dream_wind_bed.wav",         GenDreamWind());                  step++; Progress(step, totalSteps);

                // ─── Missing SFX ───────────────────────────────
                written += WriteCue("SFX/polish_hum_start.wav",            GenPolishHumStart());             step++; Progress(step, totalSteps);
                written += WriteCue("SFX/polish_hum_loop.wav",             GenPolishHumLoop());              step++; Progress(step, totalSteps);
                written += WriteCue("SFX/polish_rub_start.wav",            GenPolishRubStart());             step++; Progress(step, totalSteps);
                written += WriteCue("SFX/polish_rub_loop.wav",             GenPolishRubLoop());              step++; Progress(step, totalSteps);
                written += WriteCue("SFX/polish_rub_friction_warn.wav",    GenPolishFrictionWarn());         step++; Progress(step, totalSteps);
                written += WriteCue("SFX/polish_hum_post.wav",             GenPolishHumPost());              step++; Progress(step, totalSteps);
                written += WriteCue("SFX/ambient_autumn_loop.wav",         GenLaneAmbience());               step++; Progress(step, totalSteps);
                written += WriteCue("SFX/kettle_pour.wav",                 GenKettlePour());                 step++; Progress(step, totalSteps);
                written += WriteCue("SFX/door_hollow_open.wav",            GenDoorOpen());                   step++; Progress(step, totalSteps);

                // ─── Mumble VO per character ───────────────────
                written += WriteMumbleBank("doris",   GenDorisVoiceBank());                 step++; Progress(step, totalSteps);
                written += WriteMumbleBank("gerrold", GenGerroldVoiceBank());               step++; Progress(step, totalSteps);
                written += WriteMumbleBank("pickle",  GenPickleVoiceBank());                step++; Progress(step, totalSteps);
                written += WriteMumbleBank("marin",   GenMarinVoiceBank());                 step++; Progress(step, totalSteps);

                AssetDatabase.Refresh();

                // Configure import settings on every generated clip so
                // mobile gets ADPCM compression instead of uncompressed WAV.
                EditorUtility.DisplayProgressBar("Hearthbound · Phase 37",
                    "Configuring audio importer settings …", 0.85f);
                ConfigureImportSettings();

                // Build / heal libraries.
                EditorUtility.DisplayProgressBar("Hearthbound · Phase 37",
                    "Building MusicLibrarySO + AmbienceLibrarySO + MumbleVoiceLibrarySO …", 0.92f);
                BuildMusicLibrary();
                BuildAmbienceLibrary();
                BuildMumbleLibrary();
                HealSfxLibrary();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog(
                    "Phase 37 — Procedural Audio Studio",
                    $"Audio generation complete.\n\n" +
                    $"  • {written} .wav file(s) written under {GenRoot}\n" +
                    $"  • MusicLibrarySO    → {MusicLibraryPath}\n" +
                    $"  • AmbienceLibrarySO → {AmbienceLibraryPath}\n" +
                    $"  • MumbleVoiceLibrarySO → {MumbleLibraryPath}\n" +
                    $"  • SfxLibrary healed (polish_hum_* + ambient_autumn_loop entries now mapped)\n\n" +
                    "Re-run `Hearthbound → 🔍 Diagnose Build` — Phase 35 audit should now report\n" +
                    "0 errors, 0 warnings on the audio folders + SfxLibrary sections.",
                    "OK");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        // ─── WAV writing ──────────────────────────────────────────

        private static int WriteCue(string relativePath, float[] samples)
        {
            string path = GenRoot + "/" + relativePath;
            EnsureFolder(Path.GetDirectoryName(path).Replace('\\', '/'));
            WriteWav(path, samples);
            return 1;
        }

        private static int WriteMumbleBank(string characterId, List<float[]> phonemes)
        {
            string dir = GenMumble + "/" + characterId;
            EnsureFolder(dir);
            for (int i = 0; i < phonemes.Count; i++)
            {
                string path = $"{dir}/{characterId}_phoneme_{i:D2}.wav";
                WriteWav(path, phonemes[i]);
            }
            return phonemes.Count;
        }

        private static void WriteWav(string path, float[] samples)
        {
            // Normalise + clip + convert to 16-bit PCM.
            int n = samples.Length;
            byte[] pcm = new byte[n * 2];
            for (int i = 0; i < n; i++)
            {
                float v = Mathf.Clamp(samples[i], -1f, 1f) * HEADROOM;
                short s = (short)(v * 32767f);
                pcm[i * 2] = (byte)(s & 0xff);
                pcm[i * 2 + 1] = (byte)((s >> 8) & 0xff);
            }

            int dataLen = pcm.Length;
            using var fs = File.Create(path);
            using var w = new BinaryWriter(fs);
            // RIFF chunk
            w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            w.Write(36 + dataLen);
            w.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
            // fmt sub-chunk
            w.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            w.Write(16);                  // PCM fmt chunk size
            w.Write((short)1);            // PCM format
            w.Write((short)1);            // mono
            w.Write(SR);
            w.Write(SR * 2);              // byte rate (mono * 16-bit / 8)
            w.Write((short)2);            // block align
            w.Write((short)16);           // bits per sample
            // data sub-chunk
            w.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            w.Write(dataLen);
            w.Write(pcm);
        }

        private static void Progress(int step, int total)
        {
            EditorUtility.DisplayProgressBar("Hearthbound · Phase 37",
                $"Synthesising audio … ({step}/{total})",
                Mathf.Lerp(0.05f, 0.85f, step / (float)total));
        }

        // ─── Synthesis primitives ─────────────────────────────────

        private static System.Random _detRng;

        /// <summary>Deterministic seed = 1972 (Doris's first loaves).</summary>
        private static System.Random Rng() => _detRng ??= new System.Random(1972);
        private static System.Random Rng(int seed) => new System.Random(seed);

        private static float[] SineWave(float freq, float durationSec)
        {
            int n = (int)(SR * durationSec);
            var buf = new float[n];
            double twoPiF = 2.0 * Math.PI * freq;
            for (int i = 0; i < n; i++)
            {
                buf[i] = (float)Math.Sin(twoPiF * i / SR);
            }
            return buf;
        }

        /// <summary>
        /// Two-stage linear ADSR envelope. All durations in samples.
        /// </summary>
        private static float[] Adsr(int n, int attack, int decay, float sustainLevel, int sustainSamples, int release)
        {
            var env = new float[n];
            int idx = 0;
            // attack
            for (int i = 0; i < attack && idx < n; i++, idx++)
                env[idx] = (float)i / attack;
            // decay
            for (int i = 0; i < decay && idx < n; i++, idx++)
                env[idx] = Mathf.Lerp(1f, sustainLevel, (float)i / decay);
            // sustain
            for (int i = 0; i < sustainSamples && idx < n; i++, idx++)
                env[idx] = sustainLevel;
            // release
            for (int i = 0; i < release && idx < n; i++, idx++)
                env[idx] = Mathf.Lerp(sustainLevel, 0f, (float)i / release);
            return env;
        }

        /// <summary>
        /// Soft bell tone — additive harmonics with shared exponential decay.
        /// Approximates a bowed-string / glass-rim hybrid (cozy + warm).
        /// </summary>
        private static float[] SoftBell(float freq, float durationSec,
                                        float[] harmonics = null, float decayTau = 1.6f)
        {
            harmonics ??= new[] { 1.0f, 0.45f, 0.2f, 0.1f };
            int n = (int)(SR * durationSec);
            var buf = new float[n];
            double twoPiF = 2.0 * Math.PI * freq;
            for (int i = 0; i < n; i++)
            {
                double t = (double)i / SR;
                double env = Math.Exp(-t / decayTau);
                double sum = 0;
                for (int k = 0; k < harmonics.Length; k++)
                {
                    sum += harmonics[k] * Math.Sin(twoPiF * (k + 1) * t);
                }
                buf[i] = (float)(sum * env);
            }
            int attack = (int)(0.008f * SR);
            for (int i = 0; i < attack && i < n; i++) buf[i] *= (float)i / attack;
            Normalise(buf, 0.7f);
            return buf;
        }

        /// <summary>Soft cello — breath-attack low tone with vibrato.</summary>
        private static float[] SoftCello(float freq, float durationSec)
        {
            int n = (int)(SR * durationSec);
            var buf = new float[n];
            // Per-sample phase accumulator with vibrato modulation.
            double phase = 0;
            for (int i = 0; i < n; i++)
            {
                double t = (double)i / SR;
                double vibrato = 1.0 + 0.0035 * Math.Sin(2.0 * Math.PI * 5.0 * t);
                phase += 2.0 * Math.PI * freq * vibrato / SR;
                double f = Math.Sin(phase);
                double h2 = 0.50 * Math.Sin(2 * phase);
                double h3 = 0.25 * Math.Sin(3 * phase);
                double h4 = 0.12 * Math.Sin(4 * phase);
                double h5 = 0.06 * Math.Sin(5 * phase);
                buf[i] = (float)(f + h2 + h3 + h4 + h5);
            }
            // Bow-attack ADSR.
            int a = (int)(0.25f * SR);
            int r = (int)(0.50f * SR);
            int s = Mathf.Max(0, n - a - r);
            var env = Adsr(n, a, 0, 1f, s, r);
            for (int i = 0; i < n; i++) buf[i] *= env[i];
            Normalise(buf, 0.75f);
            return buf;
        }

        /// <summary>Piano — rich harmonics with per-harmonic exponential decay.</summary>
        private static float[] SoftPiano(float freq, float durationSec)
        {
            int n = (int)(SR * durationSec);
            var buf = new float[n];
            var harm = new (float amp, float tau)[]
            {
                (1.0f, 1.8f), (0.55f, 1.2f), (0.30f, 0.7f),
                (0.18f, 0.45f), (0.10f, 0.30f),
            };
            for (int i = 0; i < n; i++)
            {
                double t = (double)i / SR;
                double sum = 0;
                for (int k = 0; k < harm.Length; k++)
                {
                    double env = Math.Exp(-t / harm[k].tau);
                    sum += harm[k].amp * env * Math.Sin(2 * Math.PI * freq * (k + 1) * t);
                }
                buf[i] = (float)sum;
            }
            int attack = (int)(0.003f * SR);
            for (int i = 0; i < attack && i < n; i++) buf[i] *= (float)i / attack;
            Normalise(buf, 0.78f);
            return buf;
        }

        /// <summary>Violin — brighter than cello, faster vibrato.</summary>
        private static float[] SoftViolin(float freq, float durationSec)
        {
            int n = (int)(SR * durationSec);
            var buf = new float[n];
            double phase = 0;
            for (int i = 0; i < n; i++)
            {
                double t = (double)i / SR;
                double vibrato = 1.0 + 0.005 * Math.Sin(2.0 * Math.PI * 6.0 * t);
                phase += 2.0 * Math.PI * freq * vibrato / SR;
                buf[i] = (float)(Math.Sin(phase)
                                 + 0.55 * Math.Sin(2 * phase)
                                 + 0.30 * Math.Sin(3 * phase)
                                 + 0.15 * Math.Sin(4 * phase));
            }
            int a = (int)(0.18f * SR);
            int r = (int)(0.40f * SR);
            int s = Mathf.Max(0, n - a - r);
            var env = Adsr(n, a, 0, 1f, s, r);
            for (int i = 0; i < n; i++) buf[i] *= env[i];
            Normalise(buf, 0.72f);
            return buf;
        }

        /// <summary>Pink (1/f) noise via Paul Kellet IIR filter on white noise.</summary>
        private static float[] PinkNoise(float durationSec, int? seed = null)
        {
            int n = (int)(SR * durationSec);
            var rng = seed.HasValue ? Rng(seed.Value) : Rng();
            var buf = new float[n];
            // Box-Muller for white normal samples
            // → Paul Kellet 4-band pink filter approximation
            float b0 = 0, b1 = 0, b2 = 0, b3 = 0, b4 = 0, b5 = 0, b6 = 0;
            for (int i = 0; i < n; i++)
            {
                float white = GaussianFloat(rng);
                b0 = 0.99886f * b0 + white * 0.0555179f;
                b1 = 0.99332f * b1 + white * 0.0750759f;
                b2 = 0.96900f * b2 + white * 0.1538520f;
                b3 = 0.86650f * b3 + white * 0.3104856f;
                b4 = 0.55000f * b4 + white * 0.5329522f;
                b5 = -0.7616f * b5 - white * 0.0168980f;
                float pink = b0 + b1 + b2 + b3 + b4 + b5 + b6 + white * 0.5362f;
                b6 = white * 0.115926f;
                buf[i] = pink * 0.11f;  // gain compensation
            }
            return buf;
        }

        private static float GaussianFloat(System.Random rng)
        {
            // Box-Muller — one of two outputs.
            double u1 = 1.0 - rng.NextDouble();
            double u2 = 1.0 - rng.NextDouble();
            return (float)(Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2));
        }

        /// <summary>Brown noise — integrated white noise, high-passed to keep it stable.</summary>
        private static float[] BrownNoise(float durationSec, int? seed = null)
        {
            int n = (int)(SR * durationSec);
            var rng = seed.HasValue ? Rng(seed.Value) : Rng();
            var buf = new float[n];
            float walk = 0;
            for (int i = 0; i < n; i++)
            {
                walk += GaussianFloat(rng) * 0.05f;
                buf[i] = walk;
            }
            // High-pass = subtract a moving-average low-pass version.
            int win = Mathf.Max(1, (int)(0.5f * SR));
            var lp = new float[n];
            float runSum = 0;
            for (int i = 0; i < n; i++)
            {
                runSum += buf[i];
                if (i >= win) runSum -= buf[i - win];
                lp[i] = runSum / Mathf.Min(i + 1, win);
            }
            for (int i = 0; i < n; i++) buf[i] -= lp[i];
            Normalise(buf, 0.5f);
            return buf;
        }

        /// <summary>One-pole low-pass.</summary>
        private static float[] Lowpass(float[] sig, float cutoffHz)
        {
            float rc = 1f / (2f * Mathf.PI * cutoffHz);
            float dt = 1f / SR;
            float alpha = dt / (rc + dt);
            var o = new float[sig.Length];
            o[0] = sig[0] * alpha;
            for (int i = 1; i < sig.Length; i++)
                o[i] = o[i - 1] + alpha * (sig[i] - o[i - 1]);
            return o;
        }

        /// <summary>One-pole high-pass.</summary>
        private static float[] Highpass(float[] sig, float cutoffHz)
        {
            float rc = 1f / (2f * Mathf.PI * cutoffHz);
            float dt = 1f / SR;
            float alpha = rc / (rc + dt);
            var o = new float[sig.Length];
            o[0] = sig[0];
            for (int i = 1; i < sig.Length; i++)
                o[i] = alpha * (o[i - 1] + sig[i] - sig[i - 1]);
            return o;
        }

        /// <summary>Cheap Schroeder-style reverb — sparse delays summed at decreasing amplitudes.</summary>
        private static float[] ReverbTail(float[] sig, float decay = 0.6f)
        {
            int[] delaysMs = { 29, 47, 61, 89, 127, 163 };
            var o = (float[])sig.Clone();
            for (int i = 0; i < delaysMs.Length; i++)
            {
                int d = delaysMs[i] * SR / 1000;
                if (d >= o.Length) continue;
                float amp = Mathf.Pow(decay, i + 1) * 0.45f;
                for (int j = d; j < o.Length; j++)
                    o[j] += amp * sig[j - d];
            }
            Normalise(o, 0.95f);
            return o;
        }

        /// <summary>Make a buffer loop-friendly via a crossfade pre-roll at the front.</summary>
        private static float[] CrossfadeLoop(float[] buf, float fadeSec = 1.5f)
        {
            int n = buf.Length;
            int f = (int)(fadeSec * SR);
            if (f * 2 >= n) return buf;
            var head = new float[f];
            var tail = new float[f];
            Array.Copy(buf, 0, head, 0, f);
            Array.Copy(buf, n - f, tail, 0, f);
            // blended head = tail * (1-k) + head * k
            var blended = new float[f];
            for (int i = 0; i < f; i++)
            {
                float k = (float)i / f;
                blended[i] = tail[i] * (1f - k) + head[i] * k;
            }
            // Compose: blended head + middle of original buf (skip first f and last f).
            int midLen = n - f - f;
            var o = new float[f + midLen];
            Array.Copy(blended, 0, o, 0, f);
            Array.Copy(buf, f, o, f, midLen);
            return o;
        }

        /// <summary>Normalise to peak amplitude.</summary>
        private static void Normalise(float[] buf, float peakTarget = 0.95f)
        {
            float peak = 0;
            for (int i = 0; i < buf.Length; i++)
                if (Math.Abs(buf[i]) > peak) peak = Math.Abs(buf[i]);
            if (peak < 1e-6f) return;
            float scale = peakTarget / peak;
            for (int i = 0; i < buf.Length; i++) buf[i] *= scale;
        }

        // ─── Mix helpers ──────────────────────────────────────────

        private static float[] Mix(params float[][] sigs)
        {
            int len = 0;
            foreach (var s in sigs) if (s.Length > len) len = s.Length;
            var o = new float[len];
            foreach (var s in sigs)
                for (int i = 0; i < s.Length && i < len; i++)
                    o[i] += s[i];
            return o;
        }

        private static float[] Scale(float[] sig, float gain)
        {
            var o = new float[sig.Length];
            for (int i = 0; i < sig.Length; i++) o[i] = sig[i] * gain;
            return o;
        }

        private static void OverlayInto(float[] dst, float[] src, int offsetSamples, float gain = 1f)
        {
            int end = Mathf.Min(dst.Length, offsetSamples + src.Length);
            for (int i = Mathf.Max(0, offsetSamples); i < end; i++)
                dst[i] += src[i - offsetSamples] * gain;
        }

        private static float[] Concat(params float[][] parts)
        {
            int total = 0;
            foreach (var p in parts) total += p.Length;
            var o = new float[total];
            int idx = 0;
            foreach (var p in parts)
            {
                Array.Copy(p, 0, o, idx, p.Length);
                idx += p.Length;
            }
            return o;
        }

        private static float[] Silence(float seconds)
        {
            return new float[(int)(SR * seconds)];
        }

        // ─── Music cues ───────────────────────────────────────────

        private static float[] GenMainTheme()
        {
            // Solo piano sets the motif twice, then cello layer underneath.
            float beat = 60f / 64f;  // 64 BPM
            var parts = new List<float[]>
            {
                // Bar 1-4: piano motif (sparse, slow)
                SoftPiano(F4, 2 * beat),
                SoftPiano(A4, 2 * beat),
                SoftPiano(C5, 2 * beat),
                SoftPiano(A4, 2 * beat),
                SoftPiano(F4, 4 * beat),
                Silence(4 * beat),
                // Bar 7-12: motif with cello layer
                Mix(SoftPiano(F4, 2*beat), Scale(SoftCello(F4*0.5f, 2*beat), 0.6f)),
                Mix(SoftPiano(A4, 2*beat), Scale(SoftCello(A4*0.5f, 2*beat), 0.6f)),
                Mix(SoftPiano(C5, 2*beat), Scale(SoftCello(C5*0.5f, 2*beat), 0.6f)),
                Mix(SoftPiano(A4, 2*beat), Scale(SoftCello(A4*0.5f, 2*beat), 0.6f)),
                Mix(SoftPiano(F4, 4*beat), Scale(SoftCello(F4*0.5f, 4*beat), 0.6f)),
                Silence(2 * beat),
                // Bar 13-20: IV movement w/ cello sustain
                Scale(SoftCello(233.08f, 4*beat), 0.7f),  // Bb3
                Mix(SoftCello(C4, 4*beat), Scale(SoftPiano(C4, 4*beat), 0.5f)),
                Mix(SoftCello(E4, 4*beat), Scale(SoftPiano(E4, 4*beat), 0.5f)),
                SoftCello(F4, 4 * beat),
                // Bar 21-26: final motif
                SoftPiano(F4, 2 * beat),
                SoftPiano(A4, 2 * beat),
                SoftPiano(C5, 2 * beat),
                SoftPiano(A4, 2 * beat),
                Mix(SoftPiano(F4, 6*beat), Scale(SoftCello(F4*0.5f, 6*beat), 0.6f)),
                Silence(4 * beat),
            };
            // Scale piano-heavy chunks for room
            for (int i = 0; i < parts.Count; i++) parts[i] = Scale(parts[i], 0.6f);
            return ReverbTail(Concat(parts.ToArray()), 0.55f);
        }

        private static float[] GenDorisMotif()
        {
            float beat = 60f / 64f;
            float[] cello = (float[])null;
            var seq = new List<float[]>
            {
                Silence(3.0f),
                SoftCello(F4 * 0.5f, 4 * beat),
                SoftCello(A4 * 0.5f, 4 * beat),
                SoftCello(C5 * 0.5f, 4 * beat),
                SoftCello(A4 * 0.5f, 4 * beat),
                SoftCello(F4 * 0.5f, 8 * beat),
                Silence(1.5f),
                // piano counter-line
                SoftPiano(C4, 4 * beat),
                SoftPiano(E4, 4 * beat),
                SoftPiano(F4, 4 * beat),
                SoftPiano(F4, 6 * beat),
                Silence(1.0f),
                // final cello statement
                SoftCello(F4 * 0.5f, 3 * beat),
                SoftCello(A4 * 0.5f, 3 * beat),
                SoftCello(C5 * 0.5f, 4 * beat),
                SoftCello(A4 * 0.5f, 4 * beat),
                SoftCello(F4 * 0.5f, 8 * beat),
                Silence(2.0f),
            };
            for (int i = 0; i < seq.Count; i++) seq[i] = Scale(seq[i], 0.75f);
            return ReverbTail(Concat(seq.ToArray()), 0.6f);
        }

        private static float[] GenMargeryVariantA()
        {
            float beat = 60f / 60f;
            var seq = new List<float[]>
            {
                Silence(4.0f),
                SoftViolin(A4, 4 * beat),
                SoftViolin(C5, 3 * beat),
                SoftViolin(E5, 3 * beat),
                SoftViolin(C5, 4 * beat),
                SoftViolin(A4, 6 * beat),
                Silence(2.0f),
                Mix(SoftViolin(C5, 4 * beat), Scale(SoftPiano(C4, 4 * beat), 0.4f)),
                Mix(SoftViolin(E5, 4 * beat), Scale(SoftPiano(E4, 4 * beat), 0.4f)),
                Mix(SoftViolin(G4, 4 * beat), Scale(SoftPiano(G4 * 0.5f, 4 * beat), 0.4f)),
                Mix(SoftViolin(C5, 8 * beat), Scale(SoftPiano(C5 * 0.5f, 8 * beat), 0.4f)),
                Silence(2.0f),
                // Final resolution
                SoftViolin(A4, 3 * beat),
                SoftViolin(C5, 3 * beat),
                SoftViolin(E5, 4 * beat),
                SoftViolin(C5, 4 * beat),
                SoftViolin(A4, 8 * beat),
                SoftViolin(G4, 4 * beat),
                Mix(SoftViolin(C5, 12 * beat), Scale(SoftPiano(C5 * 0.5f, 12 * beat), 0.4f)),
                Silence(4.0f),
            };
            for (int i = 0; i < seq.Count; i++) seq[i] = Scale(seq[i], 0.7f);
            return ReverbTail(Concat(seq.ToArray()), 0.7f);
        }

        private static float[] GenMargeryVariantB()
        {
            float beat = 60f / 60f;
            var seq = new List<float[]>
            {
                Silence(4.0f),
                SoftViolin(A4, 4 * beat),
                SoftViolin(C5, 3 * beat),
                SoftViolin(E5, 3 * beat),
                SoftViolin(C5, 4 * beat),
                SoftViolin(A4, 6 * beat),
                Silence(3.0f),
                Mix(SoftViolin(C5, 4 * beat), Scale(SoftPiano(C4, 4 * beat), 0.35f)),
                Mix(SoftViolin(E5, 4 * beat), Scale(SoftPiano(E4, 4 * beat), 0.35f)),
                Mix(SoftViolin(G4, 6 * beat), Scale(SoftPiano(G4 * 0.5f, 6 * beat), 0.35f)),
                Mix(SoftViolin(B4_FLAT, 4 * beat), Scale(SoftPiano(B4_FLAT * 0.5f, 4 * beat), 0.35f)),
                Silence(2.0f),
                SoftViolin(A4, 3 * beat),
                SoftViolin(C5, 3 * beat),
                SoftViolin(A4, 4 * beat),
                SoftViolin(G4, 6 * beat),
                SoftViolin(F4, 10 * beat),  // ends on F not C — gentler
                Silence(4.0f),
            };
            for (int i = 0; i < seq.Count; i++) seq[i] = Scale(seq[i], 0.7f);
            return ReverbTail(Concat(seq.ToArray()), 0.65f);
        }

        private static float[] GenMargeryVariantC()
        {
            float beat = 60f / 56f;  // slower
            var seq = new List<float[]>
            {
                Silence(5.0f),
                SoftViolin(A4, 6 * beat),
                SoftViolin(C5, 4 * beat),
                SoftViolin(A4, 4 * beat),
                SoftViolin(G4, 4 * beat),
                SoftViolin(F4, 8 * beat),
                Silence(3.0f),
                // descending piano in minor
                Mix(SoftViolin(A4, 4 * beat), Scale(SoftPiano(A4 * 0.5f, 4 * beat), 0.4f)),
                Mix(SoftViolin(G4, 4 * beat), Scale(SoftPiano(G4 * 0.5f, 4 * beat), 0.4f)),
                Mix(SoftViolin(F4, 4 * beat), Scale(SoftPiano(F4 * 0.5f, 4 * beat), 0.4f)),
                Mix(SoftViolin(E4, 6 * beat), Scale(SoftPiano(E4 * 0.5f, 6 * beat), 0.4f)),
                Silence(2.0f),
                SoftViolin(A4, 3 * beat),
                SoftViolin(C5, 3 * beat),
                SoftViolin(E5, 4 * beat),
                SoftViolin(G4, 6 * beat),
                // Final A minor 7 sustained (A + C + E + G)
                Mix(SoftViolin(A4, 16 * beat),
                    Scale(SoftViolin(C5, 16 * beat), 0.5f),
                    Scale(SoftViolin(E5, 16 * beat), 0.3f),
                    Scale(SoftViolin(G4, 16 * beat), 0.4f)),
                Silence(4.0f),
            };
            for (int i = 0; i < seq.Count; i++) seq[i] = Scale(seq[i], 0.7f);
            return ReverbTail(Concat(seq.ToArray()), 0.7f);
        }

        private static float[] GenMargeryVariantD()
        {
            float beat = 60f / 50f;  // very slow — solo cello, no piano
            var seq = new List<float[]>
            {
                Silence(6.0f),
                SoftCello(F4 * 0.5f, 6 * beat),
                SoftCello(G4 * 0.5f, 4 * beat),
                SoftCello(A4 * 0.5f, 6 * beat),
                Silence(2.0f),
                SoftCello(F4 * 0.5f, 4 * beat),
                SoftCello(A4 * 0.5f, 4 * beat),
                SoftCello(C5 * 0.5f, 8 * beat),
                Silence(3.0f),
                SoftCello(A4 * 0.5f, 4 * beat),
                SoftCello(F4 * 0.5f, 12 * beat),
                Silence(6.0f),
            };
            for (int i = 0; i < seq.Count; i++) seq[i] = Scale(seq[i], 0.7f);
            return ReverbTail(Concat(seq.ToArray()), 0.8f);
        }

        private static float[] GenMargeryVariantE()
        {
            // Single sustained F3 cello note slowly fading over 30s.
            int n = (int)(SR * 30.0f);
            var note = SoftCello(F4 * 0.5f, 30.0f);
            var o = new float[n];
            for (int i = 0; i < n && i < note.Length; i++)
            {
                float fade = 1f - (float)i / n;
                o[i] = note[i] * Mathf.Pow(fade, 1.3f);
            }
            return ReverbTail(o, 0.85f);
        }

        private static float[] GenLaneTheme()
        {
            // Cello pad + sparse piano motif. 90s loop.
            float dur = 90f;
            float beat = 60f / 58f;
            var pad = Mix(
                Scale(SoftCello(F4 * 0.5f, dur), 0.18f),
                Scale(SoftCello(C4, dur), 0.12f));
            var melody = new float[(int)(SR * dur)];
            float[] starts = { 3.0f, 18.0f, 36.0f, 54.0f, 72.0f };
            foreach (var start in starts)
            {
                for (int i = 0; i < HEARTHBOUND_MOTIF.Length; i++)
                {
                    float tOff = start + i * beat * 1.2f;
                    int sampleOff = (int)(tOff * SR);
                    var note = SoftPiano(HEARTHBOUND_MOTIF[i] * 0.5f, beat * 1.5f);
                    OverlayInto(melody, note, sampleOff, 0.35f);
                }
            }
            return CrossfadeLoop(ReverbTail(Mix(pad, melody), 0.5f), 2.0f);
        }

        private static float[] GenHollowTheme()
        {
            float dur = 90f;
            float beat = 60f / 62f;
            var pad = Scale(SoftCello(F4 * 0.5f, dur), 0.15f);
            var melody = new float[(int)(SR * dur)];
            var notes = new (float t, float f)[]
            {
                (0, F4), (4, A4), (8, C5), (12, A4), (18, F4), (24, G4), (30, A4),
                (38, F4), (44, C4), (50, E4), (56, F4), (64, A4), (70, C5),
                (76, A4), (84, F4),
            };
            foreach (var (t, f) in notes)
            {
                int sampleOff = (int)(t * SR);
                var note = SoftPiano(f * 0.5f, beat * 3.0f);
                OverlayInto(melody, note, sampleOff, 0.45f);
            }
            return CrossfadeLoop(ReverbTail(Mix(pad, melody), 0.6f), 2.0f);
        }

        private static float[] GenGardenTheme()
        {
            float dur = 90f;
            float beat = 60f / 70f;
            var melody = new float[(int)(SR * dur)];
            var notes = new (float t, float f)[]
            {
                (0, C5), (4, 659.26f), (7, 783.99f), (10, 659.26f), (14, C5),
                (20, C5), (24, A4), (28, F5), (32, A4), (38, C5),
                (44, C5), (48, 659.26f), (52, 783.99f), (56, 659.26f), (60, C5),
                (66, C5), (70, A4), (74, F4), (78, A4), (84, C5),
            };
            foreach (var (t, f) in notes)
            {
                int sampleOff = (int)(t * SR);
                var note = SoftPiano(f * 0.5f, beat * 2.5f);
                OverlayInto(melody, note, sampleOff, 0.5f);
            }
            var bell = Scale(SoftBell(C4, dur, new[] { 1.0f, 0.2f, 0.08f }, 12.0f), 0.10f);
            return CrossfadeLoop(ReverbTail(Mix(melody, bell), 0.5f), 2.0f);
        }

        private static float[] GenCottageTheme()
        {
            float dur = 75f;
            float beat = 60f / 54f;
            var pad = Mix(
                Scale(SoftCello(A4 * 0.5f, dur), 0.20f),
                Scale(SoftCello(F4 * 0.5f, dur), 0.14f));
            var melody = new float[(int)(SR * dur)];
            var notes = new (float t, float f)[]
            {
                (2, A4), (8, G4), (14, F4), (22, E4), (30, F4),
                (38, A4), (46, C5), (52, A4), (60, F4),
            };
            foreach (var (t, f) in notes)
            {
                int sampleOff = (int)(t * SR);
                var note = SoftPiano(f * 0.5f, beat * 3.0f);
                OverlayInto(melody, note, sampleOff, 0.4f);
            }
            return CrossfadeLoop(ReverbTail(Mix(pad, melody), 0.65f), 2.0f);
        }

        private static float[] GenMenuTheme()
        {
            float dur = 90f;
            float beat = 60f / 56f;
            var pad = Mix(
                Scale(SoftCello(F4 * 0.5f, dur), 0.15f),
                Scale(SoftCello(C4, dur), 0.10f));
            var melody = new float[(int)(SR * dur)];
            float[] motifStarts = { 4.0f, 50.0f };
            foreach (var start in motifStarts)
            {
                for (int i = 0; i < HEARTHBOUND_MOTIF.Length; i++)
                {
                    float tOff = start + i * beat * 2.0f;
                    int sampleOff = (int)(tOff * SR);
                    var note = SoftPiano(HEARTHBOUND_MOTIF[i] * 0.5f, beat * 2.5f);
                    OverlayInto(melody, note, sampleOff, 0.55f);
                }
            }
            return CrossfadeLoop(ReverbTail(Mix(pad, melody), 0.7f), 2.5f);
        }

        // ─── Ambience beds ────────────────────────────────────────

        private static float[] GenLaneAmbience()
        {
            float dur = 45f;
            int n = (int)(SR * dur);
            var wind = Lowpass(PinkNoise(dur, 11), 800f);
            // gust modulation
            for (int i = 0; i < n; i++)
            {
                double t = (double)i / SR;
                double gust = 0.55 + 0.45 * Math.Pow(Math.Sin(2 * Math.PI * 0.15 * t + 1.2), 2);
                wind[i] *= (float)gust * 0.7f;
            }
            // 4 birds
            float[] birdsAt = { 4.0f, 17.5f, 28.0f, 41.5f };
            foreach (var t in birdsAt)
            {
                var chirp = BirdChirp(0.35f);
                OverlayInto(wind, chirp, (int)(t * SR), 0.18f);
            }
            // village bell at 30s
            var bell = Scale(SoftBell(C4 * 0.5f, 4.0f, new[] { 1.0f, 0.3f, 0.1f }, 2.5f), 0.12f);
            OverlayInto(wind, bell, (int)(30.0f * SR));
            return CrossfadeLoop(wind, 2.0f);
        }

        private static float[] GenHollowAmbience()
        {
            float dur = 45f;
            int n = (int)(SR * dur);
            var room = Scale(BrownNoise(dur, 23), 0.25f);
            var crackle = new float[n];
            var rng = Rng(42);
            for (int i = 0; i < 14; i++)
            {
                float tPop = (float)(rng.NextDouble() * dur);
                var pop = CracklePop();
                OverlayInto(crackle, pop, (int)(tPop * SR), (float)(rng.NextDouble() * 0.25 + 0.15));
            }
            var ticks = new float[n];
            var tick = ClockTick(680f);
            for (int i = 0; i < (int)(dur / 1.2f); i++)
            {
                OverlayInto(ticks, tick, (int)(i * 1.2f * SR), 0.10f);
            }
            return CrossfadeLoop(Mix(room, crackle, ticks), 2.0f);
        }

        private static float[] GenGardenAmbience()
        {
            float dur = 45f;
            int n = (int)(SR * dur);
            var wind = Scale(Lowpass(PinkNoise(dur, 33), 1500f), 0.45f);
            var bee = new float[n];
            for (int i = 0; i < n; i++)
            {
                double t = (double)i / SR;
                bee[i] = (float)(0.04 * Math.Sin(2 * Math.PI * 210 * t)
                                 * (0.7 + 0.3 * Math.Sin(2 * Math.PI * 8 * t)));
            }
            var birds = new float[n];
            var rng = Rng(7);
            for (int i = 0; i < 6; i++)
            {
                float tChirp = (float)(rng.NextDouble() * dur);
                float baseFreq = (float)(rng.NextDouble() * 600 + 1800);
                var chirp = BirdChirp(0.4f, baseFreq);
                OverlayInto(birds, chirp, (int)(tChirp * SR), 0.15f);
            }
            return CrossfadeLoop(Mix(wind, bee, birds), 2.0f);
        }

        private static float[] GenCottageAmbience()
        {
            float dur = 45f;
            int n = (int)(SR * dur);
            var room = Scale(BrownNoise(dur, 99), 0.20f);
            var crackle = new float[n];
            var rng = Rng(91);
            for (int i = 0; i < 9; i++)
            {
                float tPop = (float)(rng.NextDouble() * dur);
                var pop = CracklePop();
                OverlayInto(crackle, pop, (int)(tPop * SR), (float)(rng.NextDouble() * 0.20 + 0.10));
            }
            var ticks = new float[n];
            var tick = ClockTick(520f);
            for (int i = 0; i < (int)(dur / 1.5f); i++)
            {
                OverlayInto(ticks, tick, (int)(i * 1.5f * SR), 0.08f);
            }
            return CrossfadeLoop(Mix(room, crackle, ticks), 2.0f);
        }

        private static float[] GenKettleSteam()
        {
            float dur = 12f;
            int n = (int)(SR * dur);
            var steam = Scale(Highpass(PinkNoise(dur, 55), 2400f), 0.35f);
            var whistle = new float[n];
            double phase = 0;
            for (int i = 0; i < n; i++)
            {
                double t = (double)i / SR;
                double f = 1900 + 200 * Math.Sin(2 * Math.PI * 0.08 * t);
                phase += 2 * Math.PI * f / SR;
                whistle[i] = (float)(0.05 * Math.Sin(phase));
            }
            return CrossfadeLoop(Mix(steam, whistle), 1.5f);
        }

        private static float[] GenDreamWind()
        {
            float dur = 30f;
            int n = (int)(SR * dur);
            var wind = Scale(Lowpass(PinkNoise(dur, 77), 500f), 0.25f);
            for (int i = 0; i < n; i++)
            {
                double t = (double)i / SR;
                float lfo = (float)(0.5 + 0.5 * Math.Sin(2 * Math.PI * 0.08 * t));
                wind[i] *= lfo;
            }
            return CrossfadeLoop(wind, 2.0f);
        }

        // ─── Procedural event helpers ─────────────────────────────

        private static float[] BirdChirp(float durationSec, float baseFreq = 2100f)
        {
            int n = (int)(SR * durationSec);
            var buf = new float[n];
            double phase = 0;
            for (int i = 0; i < n; i++)
            {
                double t = (double)i / SR;
                double f = baseFreq * (1.0 + 0.30 * Math.Sin(2 * Math.PI * 4.0 * t / durationSec));
                phase += 2 * Math.PI * f / SR;
                buf[i] = (float)(0.6 * Math.Sin(phase) + 0.3 * Math.Sin(2 * phase));
            }
            var env = Adsr(n, (int)(0.01f * SR), (int)(0.03f * SR), 0.6f,
                           n - (int)(0.04f * SR) - (int)(0.15f * SR), (int)(0.15f * SR));
            for (int i = 0; i < n; i++) buf[i] *= env[i];
            return buf;
        }

        private static float[] CracklePop()
        {
            float dur = 0.08f;
            int n = (int)(SR * dur);
            var rng = Rng();
            var buf = new float[n];
            for (int i = 0; i < n; i++) buf[i] = GaussianFloat(rng);
            buf = Lowpass(buf, 3500f);
            buf = Highpass(buf, 400f);
            var env = Adsr(n, (int)(0.001f * SR), (int)(0.005f * SR), 0.5f,
                           (int)(0.020f * SR), (int)(0.05f * SR));
            for (int i = 0; i < n; i++) buf[i] *= env[i];
            return buf;
        }

        private static float[] ClockTick(float pitch = 680f)
        {
            float dur = 0.040f;
            int n = (int)(SR * dur);
            var buf = new float[n];
            for (int i = 0; i < n; i++)
            {
                double t = (double)i / SR;
                double env = Math.Exp(-t / 0.005);
                buf[i] = (float)((Math.Sin(2 * Math.PI * pitch * t)
                                  + 0.5 * Math.Sin(2 * Math.PI * pitch * 2 * t)) * env * 0.6);
            }
            return buf;
        }

        // ─── Missing SFX ──────────────────────────────────────────

        private static float[] GenPolishHumStart()
        {
            float dur = 1.2f;
            int n = (int)(SR * dur);
            var b1 = Scale(SoftBell(440f, dur, new[] { 1.0f, 0.3f, 0.1f }, 2.0f), 0.45f);
            var b2 = Scale(SoftBell(442f, dur, new[] { 0.8f, 0.2f, 0.06f }, 2.2f), 0.40f);
            var o = Mix(b1, b2);
            for (int i = 0; i < n; i++)
                o[i] *= Mathf.Pow((float)i / n, 0.7f);
            return o;
        }

        private static float[] GenPolishHumLoop()
        {
            float dur = 2.0f;
            int n = (int)(SR * dur);
            var buf = new float[n];
            for (int i = 0; i < n; i++)
            {
                double t = (double)i / SR;
                double s1 = Math.Sin(2 * Math.PI * 220.0 * t);
                double s2 = 0.7 * Math.Sin(2 * Math.PI * 220.7 * t);
                double s3 = 0.18 * Math.Sin(2 * Math.PI * 440.0 * t);
                double tremolo = 0.85 + 0.15 * Math.Sin(2 * Math.PI * 2.0 * t);
                buf[i] = (float)((s1 + s2 + s3) * tremolo * 0.3);
            }
            return CrossfadeLoop(buf, 0.4f);
        }

        private static float[] GenPolishRubStart()
        {
            float dur = 0.6f;
            int n = (int)(SR * dur);
            var noise = Scale(Highpass(PinkNoise(dur, 12), 2000f), 0.6f);
            var env = Adsr(n, (int)(0.08f * SR), (int)(0.15f * SR), 0.5f,
                           (int)(0.20f * SR), (int)(0.15f * SR));
            for (int i = 0; i < n; i++) noise[i] *= env[i];
            return noise;
        }

        private static float[] GenPolishRubLoop()
        {
            float dur = 1.5f;
            int n = (int)(SR * dur);
            var noise = Scale(Highpass(PinkNoise(dur, 13), 1800f), 0.45f);
            for (int i = 0; i < n; i++)
            {
                double t = (double)i / SR;
                float lfo = (float)(0.7 + 0.3 * Math.Sin(2 * Math.PI * 1.4 * t));
                noise[i] *= lfo;
            }
            return CrossfadeLoop(noise, 0.4f);
        }

        private static float[] GenPolishFrictionWarn()
        {
            float dur = 0.4f;
            int n = (int)(SR * dur);
            var noise = Scale(Highpass(PinkNoise(dur, 14), 3500f), 0.8f);
            var scrape = new float[n];
            for (int i = 0; i < n; i++)
            {
                double t = (double)i / SR;
                scrape[i] = (float)(0.15 * Math.Sin(2 * Math.PI * 4200 * t));
            }
            var combined = Mix(noise, scrape);
            var env = Adsr(n, (int)(0.01f * SR), (int)(0.05f * SR), 0.7f,
                           (int)(0.20f * SR), (int)(0.10f * SR));
            for (int i = 0; i < n; i++) combined[i] *= env[i];
            return combined;
        }

        private static float[] GenPolishHumPost()
        {
            float dur = 1.5f;
            var f1 = Scale(SoftBell(440f, dur, new[] { 1.0f, 0.5f, 0.25f, 0.12f }, 3.0f), 0.6f);
            var f2 = Scale(SoftBell(660f, dur, new[] { 0.8f, 0.3f, 0.1f }, 3.5f), 0.35f);
            return ReverbTail(Mix(f1, f2), 0.5f);
        }

        private static float[] GenKettlePour()
        {
            float dur = 3.0f;
            int n = (int)(SR * dur);
            var pour = Scale(Highpass(PinkNoise(dur, 88), 2200f), 0.55f);
            var env = Adsr(n, (int)(0.08f * SR), (int)(0.20f * SR), 0.85f,
                           (int)(2.0f * SR), (int)(0.6f * SR));
            for (int i = 0; i < n; i++) pour[i] *= env[i];
            return pour;
        }

        private static float[] GenDoorOpen()
        {
            float dur = 1.5f;
            int n = (int)(SR * dur);
            var bell = SoftBell(900f, 0.6f, new[] { 1.0f, 0.6f, 0.2f }, 1.0f);
            float creakDur = 0.8f;
            int creakN = (int)(SR * creakDur);
            var creak = new float[creakN];
            double phase = 0;
            for (int i = 0; i < creakN; i++)
            {
                double t = (double)i / SR;
                double f = 80 + 30 * Math.Sin(2 * Math.PI * 0.5 * t);
                phase += 2 * Math.PI * f / SR;
                creak[i] = (float)(0.4 * Math.Sin(phase));
            }
            var hp = Scale(Highpass(PinkNoise(creakDur, 44), 200f), 0.15f);
            for (int i = 0; i < creakN; i++) creak[i] += hp[i];
            var creakEnv = Adsr(creakN, (int)(0.05f * SR), (int)(0.10f * SR), 0.6f,
                                (int)(0.50f * SR), (int)(0.10f * SR));
            for (int i = 0; i < creakN; i++) creak[i] *= creakEnv[i];
            var o = new float[n];
            OverlayInto(o, bell, 0, 0.5f);
            OverlayInto(o, creak, (int)(0.3f * SR), 0.6f);
            return o;
        }

        // ─── Mumble VO ────────────────────────────────────────────

        /// <summary>
        /// Single mumble phoneme — two-formant vowel approximation.
        /// vowelBrightness 0..1 : higher = ah/eh/ee, lower = oo/uh.
        /// breath 0..1 : breath noise mix.
        /// </summary>
        private static float[] GenMumblePhoneme(float basePitch, float syllableDur,
                                                float vowelBrightness, float breath)
        {
            int n = (int)(SR * syllableDur);
            var buf = new float[n];
            float f1 = 350f + vowelBrightness * 400f;
            float f2 = 1100f + vowelBrightness * 1600f;
            for (int i = 0; i < n; i++)
            {
                double t = (double)i / SR;
                double fund = Math.Sin(2 * Math.PI * basePitch * t)
                            + 0.55 * Math.Sin(2 * Math.PI * basePitch * 2 * t)
                            + 0.30 * Math.Sin(2 * Math.PI * basePitch * 3 * t)
                            + 0.15 * Math.Sin(2 * Math.PI * basePitch * 4 * t);
                double tCenter = t - syllableDur / 2.0;
                double formantEnv = Math.Exp(-(tCenter * tCenter) / (2 * 0.08 * 0.08));
                double f1Res = 0.7 * Math.Sin(2 * Math.PI * f1 * t) * formantEnv;
                double f2Res = 0.35 * Math.Sin(2 * Math.PI * f2 * t) * formantEnv;
                buf[i] = (float)(fund * 0.5 + f1Res + f2Res);
            }
            if (breath > 0)
            {
                var noise = Scale(Highpass(PinkNoise(syllableDur, (int)(basePitch * 17)), 1500f), breath);
                for (int i = 0; i < n; i++) buf[i] += noise[i];
            }
            // attack 12ms, decay 20ms, release 38ms
            int a = (int)(0.012f * SR);
            int d = (int)(0.020f * SR);
            int r = (int)(0.038f * SR);
            int s = Mathf.Max(0, n - a - d - r);
            var env = Adsr(n, a, d, 0.85f, s, r);
            for (int i = 0; i < n; i++) buf[i] *= env[i];
            Normalise(buf, 0.6f);
            return buf;
        }

        private static List<float[]> GenDorisVoiceBank()
        {
            // Warm contralto — F3-A3
            float[] pitches = { F3, 196f, A3, 174.61f, 207.65f, 220f, 196f, 174.61f, A3, F3, 196f, A3 };
            float[] bright =  { 0.40f, 0.35f, 0.55f, 0.25f, 0.45f, 0.65f, 0.30f, 0.20f, 0.50f, 0.35f, 0.40f, 0.55f };
            float[] durs   =  { 0.18f, 0.20f, 0.16f, 0.22f, 0.18f, 0.18f, 0.20f, 0.22f, 0.16f, 0.20f, 0.18f, 0.18f };
            float[] breath =  { 0.08f, 0.10f, 0.06f, 0.12f, 0.07f, 0.05f, 0.08f, 0.10f, 0.06f, 0.08f, 0.07f, 0.05f };
            return BuildBank(pitches, bright, durs, breath);
        }

        private static List<float[]> GenGerroldVoiceBank()
        {
            // Hesitant baritone — C3-E3
            float[] pitches = { 130.81f, 146.83f, 164.81f, 130.81f, 138.59f, 155.56f,
                                146.83f, 130.81f, 164.81f, 138.59f, 130.81f, 146.83f };
            float[] bright =  { 0.20f, 0.25f, 0.30f, 0.15f, 0.35f, 0.45f,
                                0.20f, 0.15f, 0.40f, 0.30f, 0.20f, 0.35f };
            float[] durs   =  { 0.22f, 0.24f, 0.20f, 0.26f, 0.22f, 0.20f,
                                0.24f, 0.26f, 0.20f, 0.22f, 0.24f, 0.22f };
            float[] breath =  { 0.12f, 0.10f, 0.08f, 0.15f, 0.12f, 0.08f,
                                0.10f, 0.14f, 0.06f, 0.10f, 0.12f, 0.08f };
            return BuildBank(pitches, bright, durs, breath);
        }

        private static List<float[]> GenPickleVoiceBank()
        {
            // Bright + terse — A4-D5
            float[] pitches = { 440f, 466.16f, 493.88f, 523.25f, 554.37f, 587.33f,
                                493.88f, 523.25f, 466.16f, 440f, 493.88f, 523.25f };
            float[] bright =  { 0.70f, 0.75f, 0.80f, 0.65f, 0.85f, 0.90f,
                                0.70f, 0.75f, 0.80f, 0.65f, 0.85f, 0.90f };
            float[] durs   =  { 0.10f, 0.08f, 0.10f, 0.12f, 0.08f, 0.10f,
                                0.10f, 0.08f, 0.10f, 0.12f, 0.08f, 0.10f };
            float[] breath =  { 0.02f, 0.03f, 0.02f, 0.02f, 0.04f, 0.03f,
                                0.02f, 0.03f, 0.02f, 0.02f, 0.04f, 0.03f };
            return BuildBank(pitches, bright, durs, breath);
        }

        private static List<float[]> GenMarinVoiceBank()
        {
            // Gentle alto — G3-B3
            float[] pitches = { 196f, 207.65f, 220f, 233.08f, 246.94f, 220f,
                                196f, 207.65f, 233.08f, 246.94f, 220f, 207.65f };
            float[] bright =  { 0.45f, 0.50f, 0.55f, 0.40f, 0.60f, 0.45f,
                                0.50f, 0.55f, 0.40f, 0.50f, 0.45f, 0.55f };
            float[] durs   =  { 0.20f, 0.18f, 0.22f, 0.20f, 0.18f, 0.20f,
                                0.20f, 0.22f, 0.18f, 0.20f, 0.20f, 0.18f };
            float[] breath =  { 0.10f, 0.12f, 0.08f, 0.14f, 0.10f, 0.12f,
                                0.08f, 0.10f, 0.12f, 0.08f, 0.10f, 0.12f };
            return BuildBank(pitches, bright, durs, breath);
        }

        private static List<float[]> BuildBank(float[] pitches, float[] bright, float[] durs, float[] breath)
        {
            var bank = new List<float[]>(pitches.Length);
            for (int i = 0; i < pitches.Length; i++)
                bank.Add(GenMumblePhoneme(pitches[i], durs[i], bright[i], breath[i]));
            return bank;
        }

        // ─── Folder helpers ───────────────────────────────────────

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/_Project/Audio");
            EnsureFolder(GenRoot);
            EnsureFolder(GenMusic);
            EnsureFolder(GenAmbience);
            EnsureFolder(GenSFX);
            EnsureFolder(GenMumble);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            var leaf = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        // ─── Audio importer settings ──────────────────────────────

        private static void ConfigureImportSettings()
        {
            // Music + Ambience: streaming for length, Vorbis compression, mono
            foreach (var folder in new[] { GenMusic, GenAmbience })
            {
                foreach (var guid in AssetDatabase.FindAssets("t:AudioClip", new[] { folder }))
                {
                    var p = AssetDatabase.GUIDToAssetPath(guid);
                    var imp = (AudioImporter)AssetImporter.GetAtPath(p);
                    if (imp == null) continue;
                    var settings = new AudioImporterSampleSettings
                    {
                        loadType = AudioClipLoadType.Streaming,
                        compressionFormat = AudioCompressionFormat.Vorbis,
                        quality = 0.55f,
                        sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate,
                    };
                    imp.forceToMono = true;
                    imp.defaultSampleSettings = settings;
                    imp.SaveAndReimport();
                }
            }

            // SFX + Mumble: small one-shots, decompress on load, ADPCM
            foreach (var folder in new[] { GenSFX, GenMumble })
            {
                foreach (var guid in AssetDatabase.FindAssets("t:AudioClip", new[] { folder }))
                {
                    var p = AssetDatabase.GUIDToAssetPath(guid);
                    var imp = (AudioImporter)AssetImporter.GetAtPath(p);
                    if (imp == null) continue;
                    var settings = new AudioImporterSampleSettings
                    {
                        loadType = AudioClipLoadType.DecompressOnLoad,
                        compressionFormat = AudioCompressionFormat.ADPCM,
                        quality = 0.5f,
                        sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate,
                    };
                    imp.forceToMono = true;
                    imp.defaultSampleSettings = settings;
                    imp.SaveAndReimport();
                }
            }
        }

        // ─── Library builders ─────────────────────────────────────

        private static AudioClip LoadClip(string path) => AssetDatabase.LoadAssetAtPath<AudioClip>(path);

        private static MusicLibrarySO.Entry MusicEntry(string id, string fileName, float volume, bool loop,
                                                       float fadeIn = 2f, float fadeOut = 2f)
        {
            return new MusicLibrarySO.Entry
            {
                id = id,
                clip = LoadClip(GenMusic + "/" + fileName),
                volume = volume,
                loop = loop,
                fadeInSeconds = fadeIn,
                fadeOutSeconds = fadeOut,
            };
        }

        private static void BuildMusicLibrary()
        {
            var lib = AssetDatabase.LoadAssetAtPath<MusicLibrarySO>(MusicLibraryPath);
            if (lib == null)
            {
                lib = ScriptableObject.CreateInstance<MusicLibrarySO>();
                AssetDatabase.CreateAsset(lib, MusicLibraryPath);
            }
            lib.entries.Clear();
            lib.entries.Add(MusicEntry("main_theme",         "main_theme_hearthbound.wav",   0.65f, true));
            lib.entries.Add(MusicEntry("scene_menu",         "scene_menu_theme.wav",         0.55f, true));
            lib.entries.Add(MusicEntry("scene_lane",         "scene_lane_theme.wav",         0.55f, true));
            lib.entries.Add(MusicEntry("scene_hollow",       "scene_hollow_theme.wav",       0.50f, true));
            lib.entries.Add(MusicEntry("scene_garden",       "scene_garden_theme.wav",       0.55f, true));
            lib.entries.Add(MusicEntry("scene_cottage",      "scene_cottage_theme.wav",      0.45f, true));
            lib.entries.Add(MusicEntry("dream_doris_motif",  "doris_motif_dream1.wav",       0.85f, false, 3f, 3f));
            lib.entries.Add(MusicEntry("dream_margery_a",    "margery_motif_variant_a.wav",  0.85f, false, 3f, 3f));
            lib.entries.Add(MusicEntry("dream_margery_b",    "margery_motif_variant_b.wav",  0.85f, false, 3f, 3f));
            lib.entries.Add(MusicEntry("dream_margery_c",    "margery_motif_variant_c.wav",  0.85f, false, 3f, 3f));
            lib.entries.Add(MusicEntry("dream_margery_d",    "margery_motif_variant_d.wav",  0.80f, false, 3f, 3f));
            lib.entries.Add(MusicEntry("dream_margery_e",    "margery_motif_variant_e.wav",  0.75f, false, 1f, 6f));
            EditorUtility.SetDirty(lib);
        }

        private static void BuildAmbienceLibrary()
        {
            var lib = AssetDatabase.LoadAssetAtPath<MusicLibrarySO>(AmbienceLibraryPath);
            if (lib == null)
            {
                lib = ScriptableObject.CreateInstance<MusicLibrarySO>();
                AssetDatabase.CreateAsset(lib, AmbienceLibraryPath);
            }
            lib.entries.Clear();
            lib.entries.Add(new MusicLibrarySO.Entry
            {
                id = "scene_lane", clip = LoadClip(GenAmbience + "/lane_autumn_loop.wav"),
                volume = 0.45f, loop = true, fadeInSeconds = 2.5f, fadeOutSeconds = 2.5f,
            });
            lib.entries.Add(new MusicLibrarySO.Entry
            {
                id = "scene_hollow", clip = LoadClip(GenAmbience + "/hollow_hearth_loop.wav"),
                volume = 0.40f, loop = true, fadeInSeconds = 2.5f, fadeOutSeconds = 2.5f,
            });
            lib.entries.Add(new MusicLibrarySO.Entry
            {
                id = "scene_garden", clip = LoadClip(GenAmbience + "/garden_day_loop.wav"),
                volume = 0.50f, loop = true, fadeInSeconds = 2.5f, fadeOutSeconds = 2.5f,
            });
            lib.entries.Add(new MusicLibrarySO.Entry
            {
                id = "scene_cottage", clip = LoadClip(GenAmbience + "/cottage_interior_loop.wav"),
                volume = 0.38f, loop = true, fadeInSeconds = 2.5f, fadeOutSeconds = 2.5f,
            });
            lib.entries.Add(new MusicLibrarySO.Entry
            {
                id = "kettle_steam", clip = LoadClip(GenAmbience + "/kettle_steam_loop.wav"),
                volume = 0.35f, loop = true, fadeInSeconds = 1.0f, fadeOutSeconds = 1.5f,
            });
            lib.entries.Add(new MusicLibrarySO.Entry
            {
                id = "dream_wind", clip = LoadClip(GenAmbience + "/dream_wind_bed.wav"),
                volume = 0.30f, loop = true, fadeInSeconds = 3.0f, fadeOutSeconds = 3.0f,
            });
            EditorUtility.SetDirty(lib);
        }

        private static void BuildMumbleLibrary()
        {
            var lib = AssetDatabase.LoadAssetAtPath<MumbleVoiceLibrarySO>(MumbleLibraryPath);
            if (lib == null)
            {
                lib = ScriptableObject.CreateInstance<MumbleVoiceLibrarySO>();
                AssetDatabase.CreateAsset(lib, MumbleLibraryPath);
            }
            lib.banks.Clear();
            lib.banks.Add(LoadCharacterBank("doris",   volume: 0.55f, syllableRate: 7.0f));
            lib.banks.Add(LoadCharacterBank("gerrold", volume: 0.55f, syllableRate: 5.5f));
            lib.banks.Add(LoadCharacterBank("pickle",  volume: 0.50f, syllableRate: 11.0f));
            lib.banks.Add(LoadCharacterBank("marin",   volume: 0.55f, syllableRate: 6.5f));
            EditorUtility.SetDirty(lib);
        }

        private static MumbleVoiceLibrarySO.CharacterVoiceBank LoadCharacterBank(string character, float volume, float syllableRate)
        {
            var bank = new MumbleVoiceLibrarySO.CharacterVoiceBank
            {
                characterId = character,
                volume = volume,
                pitchVariance = 0.08f,
                syllableRate = syllableRate,
                phonemes = new List<AudioClip>(),
            };
            string dir = $"{GenMumble}/{character}";
            for (int i = 0; i < 12; i++)
            {
                var clip = LoadClip($"{dir}/{character}_phoneme_{i:D2}.wav");
                if (clip != null) bank.phonemes.Add(clip);
            }
            return bank;
        }

        /// <summary>Walks SfxLibrary and fills any entries we just generated clips for.</summary>
        private static void HealSfxLibrary()
        {
            var lib = AssetDatabase.LoadAssetAtPath<SfxLibrarySO>(SfxLibraryPath);
            if (lib == null) return;
            // Map SfxLibrary ids → new generated clip paths
            var map = new Dictionary<string, string>
            {
                ["polish_hum_start"]         = GenSFX + "/polish_hum_start.wav",
                ["polish_hum_loop"]          = GenSFX + "/polish_hum_loop.wav",
                ["polish_rub_start"]         = GenSFX + "/polish_rub_start.wav",
                ["polish_rub_loop"]          = GenSFX + "/polish_rub_loop.wav",
                ["polish_rub_friction_warn"] = GenSFX + "/polish_rub_friction_warn.wav",
                ["polish_hum_post"]          = GenSFX + "/polish_hum_post.wav",
                ["ambient_autumn_loop"]      = GenSFX + "/ambient_autumn_loop.wav",
            };
            int healed = 0;
            for (int i = 0; i < lib.entries.Count; i++)
            {
                var e = lib.entries[i];
                if (e.clip != null) continue;  // already mapped — leave alone
                if (map.TryGetValue(e.id, out var path))
                {
                    var clip = LoadClip(path);
                    if (clip != null)
                    {
                        e.clip = clip;
                        lib.entries[i] = e;
                        healed++;
                    }
                }
            }
            if (healed > 0)
            {
                EditorUtility.SetDirty(lib);
                Debug.Log($"[Hearthbound/Phase 37] Healed {healed} previously-empty SfxLibrary entries.");
            }
        }
    }
}
