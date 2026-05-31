# 🎵 Procedural Audio Studio — Hearthbound Hollow

> The procedural-audio composer brief, in source code. Phase 37 of the
> `feat/mission-1-2-architecture` build chain.

This folder documents the **procedural audio generation system** that
synthesizes every music + ambience + missing-SFX + per-character
mumble-VO clip the game needs — end-to-end, **without** any commercial
DAW, paid SaaS (ElevenLabs / Suno / Udio), or hand-recorded foley.

The canonical implementation is the **C# Editor builder**:

> `Assets/_Project/Scripts/Editor/Phase37_ProceduralAudioStudio.cs`

It runs entirely inside Unity at Editor time when the user clicks
**`Hearthbound → 🚀 Build Everything`** (step 10 of 11) or
**`Hearthbound → ⚙️ Advanced → 🎵 Phase 37 — Procedural Audio Studio`**.

---

## 1. Why procedural?

Per `Docs/Phase35_Continuation_Audit.md` § 5 (decisions D-052 → D-054):

| Decision | Summary |
|---|---|
| **D-052** | Every PlayableAsset Timeline referenced via `[SerializeField]` must be built by an idempotent Editor phase. |
| **D-053** | Audio assets authored procedurally by this studio live under `Assets/_Project/Audio/Generated/{Music,Ambience,SFX,Mumble}/`. They are **not** committed to git — the C# source is. The build chain regenerates them deterministically (seed = `1972`). |
| **D-054** | Per-character mumble VO is the *default*. A future replacement is a pure asset swap — drop human-authored `.wav` files into `Assets/_Project/Audio/Voice/<character>/` and the `MumbleVoicePlayer` automatically prefers them. |

The decisions chain to a single goal:

> **The repo is a complete clone-and-play. No external tooling. No SaaS.
> No human composer.** A future drop from a paid composer or VO actor
> is a *pure asset swap* — no code or build-chain changes.

---

## 2. The 5-gap punchlist (what Phase 37 fixes)

Phase 35's audit found these audio gaps (`Docs/Phase35_Continuation_Audit.md` § 2):

| Gap | Before Phase 37 | After Phase 37 |
|---|---|---|
| **Music** | `Audio/Music/.gitkeep` only — zero tracks | 12 cues: main theme, Doris motif, 5× Margery variants, 4 scene themes, menu theme |
| **Ambience** | `Audio/Ambience/.gitkeep` only — zero beds | 6 ambient loops: lane autumn, hollow hearth, garden day, cottage interior, kettle steam, dream wind |
| **Missing SFX** | 7 SfxLibrary entries with `clip: {fileID: 0}` | 9 cues: `polish_hum_start/loop/post`, `polish_rub_start/loop`, `polish_rub_friction_warn`, `ambient_autumn_loop`, `kettle_pour`, `door_hollow_open` |
| **Per-character voice** | `DialogueUI` played no audio on `PresentLine()` | 4 character banks × 12 phonemes (Doris contralto, Gerrold baritone, Pickle high feminine, Marin gentle alto) |
| **Cutscene composer cues** | All 5 Dream 2 variant + Listen Timelines had empty Audio tracks | Phase 38 wires `MusicLibrarySO` cues into each Timeline's `AudioTrack` |

Net: **75 .wav files** synthesised in ~10 seconds (Unity Editor), totalling
~30 MB raw / ~6 MB after Vorbis import compression. Every cue is
deterministic from a fixed seed.

---

## 3. The synthesis primitives (what the C# implements)

All implemented in pure C# (`System` + `UnityEngine`); no third-party
dependency. The same primitives also exist as Python equivalents
in `generate_audio.py` (kept here as a reference / external-preview tool).

| Primitive | Purpose |
|---|---|
| `SineWave(freq, dur)` | Pure tone — the atomic build block |
| `SoftBell(freq, dur, harmonics, decay)` | Additive bell with shared exp decay — orb hum, village bell, garden chime |
| `SoftCello(freq, dur)` | Bow-attack low tone with 5 Hz vibrato + 5 harmonics — Doris's motif, Lane pad |
| `SoftPiano(freq, dur)` | 5 harmonics with per-harmonic exp decay τ — main theme, scene melodies |
| `SoftViolin(freq, dur)` | Bright cousin of cello, 6 Hz vibrato — Margery variants A/B/C |
| `PinkNoise(dur, seed)` | 1/f noise via Paul Kellet 7-band IIR — wind, steam, polish stroke |
| `BrownNoise(dur, seed)` | Integrated white noise + HP filter — room tone, hearth bed |
| `Lowpass(sig, cutoff)` | One-pole filter — soften high frequencies on wind |
| `Highpass(sig, cutoff)` | One-pole filter — kettle steam, polish stroke brightness |
| `ReverbTail(sig, decay)` | Schroeder-style 6-delay reverb — gives every cue the "in a real room" warmth |
| `CrossfadeLoop(buf, fade)` | Seamless loop helper for ambient beds — blends last 1.5s into first 1.5s |
| `Adsr(n, a, d, sl, s, r)` | Sample-accurate ADSR envelope — every one-shot |
| `GenMumblePhoneme(pitch, dur, brightness, breath)` | Two-formant vowel approximation + harmonic stack + breath noise + ADSR |

---

## 4. The composer brief (in numbers)

Per `Docs/Depth_Bible/14_AUDIO_MUSIC_ASMR.md` § 7 the **main theme** is built
on a 5-note motif:

```
F4 → A4 → C5 → A4 → F4    (F major, 64 BPM, "Hearthbound")
```

Every villager motif inherits this seed. Doris's Dream 1 motif transposes
it down an octave; the Margery variants transpose to A minor (relative
minor) for grief lensing; Listen variant D drops to F3 cello solo.

### Music tracks (12 cues)

| File | Duration | Form | Use |
|---|---|---|---|
| `main_theme_hearthbound.wav` | ~120 s | Piano + cello statement of motif, IV move, return | Main menu, credits |
| `scene_menu_theme.wav` | ~90 s loop | Slow piano + cello, motif × 2 | Main menu (alternates with main theme) |
| `scene_lane_theme.wav` | ~90 s loop | Cello pad + sparse piano motif × 5 | 02_Mission01_Lane scene |
| `scene_hollow_theme.wav` | ~90 s loop | Cello pad + slow piano improvisation | 03_Mission01_Hollow scene |
| `scene_garden_theme.wav` | ~90 s loop | Brighter C major motif + soft bell | 04_Mission02_Garden scene |
| `scene_cottage_theme.wav` | ~75 s loop | Sombre A-F cello pad + slow piano | 05_Mission02_Cottage scene |
| `doris_motif_dream1.wav` | ~60 s | Solo cello statement of motif | Dream 1 cutscene (Doris's First Loaves) |
| `margery_motif_variant_a.wav` | ~85 s | Violin + piano, resolves to C major | Dream 2 — Cleanse Perfect |
| `margery_motif_variant_b.wav` | ~80 s | Violin + piano, ends on F (gentler) | Dream 2 — Cleanse Acceptable / Sloppy |
| `margery_motif_variant_c.wav` | ~90 s | Slower, ends on A-minor-7 sustained | Dream 2 — Crossed Core (the heaviest beat in M2) |
| `margery_motif_variant_d.wav` | ~75 s | Solo cello, no piano — the most spare cue | Dream 2 — Listen path |
| `margery_motif_variant_e.wav` | ~30 s | Single sustained F3 cello note slowly fading | Dream 2 — Defer path |

### Ambience beds (6 loops)

| File | Layer breakdown |
|---|---|
| `lane_autumn_loop.wav` | Pink wind + gust modulation + 4 bird chirps + village bell |
| `hollow_hearth_loop.wav` | Brown room tone + 14 fire crackles + slow clock tick |
| `garden_day_loop.wav` | Brighter pink wind + 210 Hz bee drone + 6 bird chirps |
| `cottage_interior_loop.wav` | Brown room tone + 9 fire crackles + slower clock |
| `kettle_steam_loop.wav` | High-passed pink noise + 1900 Hz whistle glide |
| `dream_wind_bed.wav` | Very low-passed pink + 0.08 Hz LFO modulation — subliminal |

### Missing SFX (9 clips)

These fill the previously-empty `SfxLibrary.asset` entries:

| File | Used by |
|---|---|
| `polish_hum_start.wav` | `PolishMiniGame.cs` — orb wakes when picked up |
| `polish_hum_loop.wav` | `PolishMiniGame.cs` — orb sustain hum |
| `polish_rub_start.wav` | `PolishMiniGame.cs` — first stroke |
| `polish_rub_loop.wav` | `PolishMiniGame.cs` — circular polish stroke |
| `polish_rub_friction_warn.wav` | `PolishMiniGame.cs` — moving too fast |
| `polish_hum_post.wav` | `PolishMiniGame.cs` — orb sounds richer post-polish |
| `ambient_autumn_loop.wav` | `AmbientAudio.cs` — Lane scene autumn bed |
| `kettle_pour.wav` | `TeaBrewingUI.cs` — pour completion |
| `door_hollow_open.wav` | `HollowDoorInteractable.cs` — bell + creak |

### Per-character mumble VO (48 phonemes)

| Character | Pitch range | Brightness | Breath | Rate (syllables/s) |
|---|---|---|---|---|
| Doris (warm contralto) | F3-A3 (175-220 Hz) | mid (0.20-0.65) | mid (0.05-0.12) | 7.0 |
| Gerrold (hesitant baritone) | C3-E3 (131-165 Hz) | dark (0.15-0.45) | high (0.06-0.15) | 5.5 |
| Pickle (cat narrator, bright) | A4-D5 (440-587 Hz) | bright (0.65-0.90) | minimal (0.02-0.04) | 11.0 |
| Marin (gentle alto, predecessor) | G3-B3 (196-247 Hz) | mid (0.40-0.60) | mid (0.08-0.14) | 6.5 |

Each character gets **12 syllable clips** of ~0.10-0.26 seconds each.
The `MumbleVoicePlayer` (`Assets/_Project/Scripts/Audio/MumbleVoicePlayer.cs`)
plays a randomly-selected phoneme at the configured rate during each
dialogue line's typewriter reveal — giving the cozy
*Animal Crossing / Hollow Knight / Spiritfarer* feel.

---

## 5. How to extend

### Add a new music cue

1. Open `Assets/_Project/Scripts/Editor/Phase37_ProceduralAudioStudio.cs`.
2. Write a new `private static float[] GenMyCue() { … }` using the
   synthesis primitives.
3. Add a `WriteCue("Music/my_cue.wav", GenMyCue());` line in `Build()`.
4. Add a `lib.entries.Add(MusicEntry("my_cue", "my_cue.wav", …));` in `BuildMusicLibrary()`.
5. Re-run `Hearthbound → 🚀 Build Everything`.

### Replace any procedural cue with human-authored audio

1. Drop a `.wav` (or `.ogg`) at the canonical path —
   `Assets/_Project/Audio/Generated/Music/<filename>.wav` (same name).
2. Mark it read-only / commit it (override-baseline-with-human-authored).
3. The library asset (`MusicLibrarySO`) keeps pointing at the same
   relative path — no code change.

### Add a new VO character

1. Add a new `GenMyCharacterVoiceBank()` method in `Phase37_…cs`.
2. Add `WriteMumbleBank("mycharacter", GenMyCharacterVoiceBank());` in `Build()`.
3. Add a `lib.banks.Add(LoadCharacterBank("mycharacter", …));` in `BuildMumbleLibrary()`.
4. Use `MumbleVoicePlayer.SpeakLine("mycharacter", text, dur)` at runtime.

---

## 6. Closing — the Solberg discipline

> *"Sound is the cozy game's secret weapon. The cozy player closes their
> eyes on the couch. They hear the kettle, twice. They hear Pickle settle.
> They hear the rain. They feel — without any visual confirmation — that
> they are *home.* That is the auditory contract. That is what we
> deliver in Phase 37, without a $120k composer brief in hand."*
>
> — *Iva Solberg, Audio Director, Phase 37 sign-off, 2026-05-26*

When the producer greenlights M3+ and the composer drops their real
master files into `Assets/_Project/Audio/Music/`, this procedural
baseline becomes the **demo track** — what the playtest groups heard.
Every commercial composer master replaces a procedural file by name
and the game ships with the new audio, **no code change**.

That is the Solberg discipline.

— *End of `Tools/audio_generation/README.md` v1.0 (Phase 37).*
