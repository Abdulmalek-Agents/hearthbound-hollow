# 🫖 Hearthbound Hollow

> *"Some memories want to be sold. Some don't."*
> *«ثمّةَ ذِكْريَاتٌ تَوَدُّ أن تُبَاع. وثمّةَ ذِكْريَاتٌ لا تَوَدّ.»*

A **single-player cozy narrative simulation** set in a small autumnal village where memories are a physical, tradable commodity. You inherit a memory-brokerage shop. Buy painful memories from villagers wanting to forget. Sell preserved ones to those who long to remember. Polish, weave, and restore the fragmented — but choose carefully what you keep.

**Pitch in one line:** *Stardew's warmth × Spiritfarer's heart × Strange Horticulture's puzzles × Disco Elysium's writing — at cozy scale.*

**Languages (Phase 60 — Arabic Localization MVP):**
🇬🇧 English  ·  🇸🇦 العربية — full UI + dialogue + voice acting (Piper TTS, `ar_JL-medium`). Player picks language on first launch (auto-detected from system locale) and can switch any time from **Settings → Language**. See [`Docs/LOCALIZATION_GUIDE.md`](./Docs/LOCALIZATION_GUIDE.md) and Codex 17 [`Docs/Depth_Bible/17_LOCALIZATION_ARABIC.md`](./Docs/Depth_Bible/17_LOCALIZATION_ARABIC.md) for the linguistic + voice-casting decisions.

---

## 🎮 Run the polished playable Mission 1 + 2 (Unity)

The `feat/mission-1-2-architecture` branch ships a fully playable, polished vertical slice of Missions 1 and 2 — six scenes, two villager arcs, four moral choices, two memory dreams, a complete cozy-comfort/accessibility layer, **a robust WASD + sprint + jump controller, a smooth third-person follow camera, a Mixamo-ready Humanoid Animator, Doris/Gerrold animated dialogue beats, a 6-step first-play OnboardingOverlay, a persistent context-aware ControlHintsHUD, Phase 32 v2 polish (8 residential cottages, Hollow shop facade, hearth dressing, cozy URP cinematic volumes), AND (new in 0.7.0-procedural-audio + 0.7.1-polish-layer) a *complete procedural audio score* + *narrative audio reactivity* + *cinematic Listen camera* + *audio-aware save resume*.**

### One-click build

1. Clone the repo and check out `feat/mission-1-2-architecture`.
2. Open the project in **Unity 6 LTS (6000.4.4f1)**. Packages auto-install (~30–90 s).
3. Menu → **`Hearthbound → 🚀 Build Everything`** → click **`Build`** in the confirmation dialog → sit back ~95 s.
4. Press **Play**.

> 🔁 **After every `git pull`**, repeat step 3 — click **🚀 Build Everything**. The chain is idempotent (every Phase 13/14/15/.../42 sub-builder uses load-or-create + heal-then-save), so re-running it produces the same result as running it once. That is the *entire* recommended workflow. No other clicks required.

The `🚀 Build Everything` menu item runs **twelve sub-capstones + one Phase 32 voice library sub-step** — polished scenes, player AnimatorController, NPC AnimatorController, narrative hooks, Player Rig Doctor (foot-bone anchor), the OnboardingOverlay + ControlHintsHUD, dialogue choice repair, **Phase 32 Mission 1 Polish v2** (cottages + facade + URP cozy volumes), **Phase 32 Voice Library** (auto-builds `HearthboundVoiceLibrary.asset` from `Audio/Voice/**/*.wav` — D-058), **Phase 36 Cutscene Library** (Dream 1 + 5× Dream 2 variants + Listen Scene Timelines), **Phase 37 Procedural Audio Studio** (75 procedurally-synthesised .wav cues + MusicLibrarySO + AmbienceLibrarySO + MumbleVoiceLibrarySO), **Phase 38 Audio + Cutscene Wiring** (Bootstrap rig + per-scene beacons + DreamAudioBinder), and **Phase 42 Listen Scene Camera** (4-waypoint cinematic 180s path) — sets up Build Settings, and opens the Bootstrap scene. Reflection-driven so missing phases skip gracefully.

> 🎵 **Audio is procedural and committed-as-source.** No paid SaaS (ElevenLabs / Suno / Udio), no DAW, no human composer needed. The C# Editor builder `Phase37_ProceduralAudioStudio` synthesises all 75 .wav files deterministically (seed = 1972, Doris's first loaves) on every Build Everything run. A future commercial-composer drop into `Assets/_Project/Audio/Music/` (or `/Voice/`) is a pure file-swap — no code change. See [`Tools/audio_generation/README.md`](./Tools/audio_generation/README.md) for the composer brief.

> 🎭 **Narrative beats sound alive.** Phase 41's `MissionAudioHooks` is a runtime EventBus → audio router that translates 8 narrative events into per-beat SFX + music-duck reactions: polish completes → success swell + hum_post; cleanse Perfect → reveal swell; cleanse Crossed-Core → friction warning + music duck; moral choice → choice select + duck; tea brewed → kettle pour + confirm; day ends → ui close + slow music drift. Zero changes to Mission01/02Director — the audio layer is a passive observer.

> 💾 **Saves remember the music.** Phase 43 persists `lastMusicId` + `lastAmbienceId` + `playedDreamVariants` into the save file (schema v2). Load a game → the music + ambient bed restore on the first frame, before any scene-bootstrap audio kicks in. No silent-load gap.

> 💡 Want richer animation? Drop 6 Mixamo FBXs into `Assets/_Project/Animations/Mixamo/` per [`Docs/ANIMATION_REQUIREMENTS.md`](./Docs/ANIMATION_REQUIREMENTS.md) § 3 and re-run **`Hearthbound → 🚀 Build Everything`**. The game ships polished without them.

> 🌐 **Phase 60 — Arabic Localization MVP.** Every menu, prompt, HUD chip, dialogue line, and choice card is available in English + Modern Standard Arabic. Picker lives in **Settings → Language**; first-launch heuristic uses `Application.systemLanguage`. RTL glyph shaping handled by the runtime (`ArabicTextShaper`) — translators write canonical-form Arabic. Arabic voice acting (Piper `ar_JL-medium`) is generated by `bash Tools/generate_voices_ar.sh` → 77 clips → bind with **Hearthbound → ⚙️ Advanced → 🎙️ Phase 60 — Bind Arabic Voice Clips**. Font installer at **🔤 Phase 60 — Install Arabic Font** (one-time TMP Font Asset Creator walkthrough). See [`Docs/LOCALIZATION_GUIDE.md`](./Docs/LOCALIZATION_GUIDE.md) for the architecture + D-060 → D-064 decisions.

> 🔍 Verify wiring any time with **`Hearthbound → 🔍 Diagnose Build`** — read-only **6-step** aggregate audit that chains Phase 23 / 26 / 32 sub-diagnostics + the Phase 35 continuation audit (cutscene timelines, audio folders, SfxLibrary, Yarn, seed-assets) + the Phase 40 audio-wiring deep-dive (MusicLibrary / AmbienceLibrary / MumbleVoiceLibrary / DreamAudioBinder cueMap / per-scene SceneAudioBeacon) + the Phase 32 Voice Acting MVP library audit (`HearthboundVoiceLibrary.asset` entry count, bound clips, on-disk .wav count) — under one click.

> ⚙️ **Power users:** Every legacy per-phase entry (Phase 13 / 14 / 15 / 22 / 23 / 24 / 26 / 27 / 29 / 30 / 31 / 32.1 / 32.2 / 32.3 / 32.4 / **35 / 36 / 37 / 38 / 40 / 42 / 60** …) is still accessible under **`Hearthbound → ⚙️ Advanced ►`** with its original priority intact. Decisions **D-052 / D-053 / D-054** in [`Docs/Phase39_Greenlight_Signoff.md`](./Docs/Phase39_Greenlight_Signoff.md) codify the audio + cutscene policy; **D-055 / D-056** in [`Docs/Phase44_Polish_Layer_Signoff.md`](./Docs/Phase44_Polish_Layer_Signoff.md) codify the save-resume + install-pattern policy; **D-058** + **D-059** codify the voice-clip file-swap policy + the Piper TTS / espeak-ng dual-pipeline arrangement; **D-060 → D-064** in [`Docs/ARCHITECTURE.md`](./Docs/ARCHITECTURE.md) codify the localization-source-of-truth + LocalizationService + ArabicTextShaper + per-locale voice slots.

### Voice acting (Phase 32.6+ — Piper TTS, cross-platform open-source)

Hearthbound Hollow ships with a two-pipeline voice generation system —
both write to the same canonical `Assets/_Project/Audio/Voice/<Character>/<lineId>.wav`
paths and the runtime is identical (see **D-058**, **D-059**, and
[`Docs/VOICE_CASTING.md`](./Docs/VOICE_CASTING.md) for the canonical casting table).

**Pipeline A — Piper TTS (recommended, neural quality):**

```bash
# one-time setup (~310 MB of voice models from HuggingFace — EN + AR)
pip install piper-tts                   # or download a binary from https://github.com/rhasspy/piper/releases
bash Tools/download_voice_models.sh

# generate the 77 English voice clips (~30 s)
bash Tools/generate_voices.sh

# Phase 60 — generate the 77 Arabic voice clips (~25 s)
bash Tools/generate_voices_ar.sh

# in Unity: bind the .wav files into the SO
# Hearthbound → ⚙️ Advanced → 🎙️ Phase 32 — Rebuild Voice Library
# Hearthbound → ⚙️ Advanced → 🎙️ Phase 60 — Bind Arabic Voice Clips
```

**Pipeline B — espeak-ng (lighter, in-Editor, runs on every Build Everything):**

```bash
# one-time install (cross-platform, ~2 MB):
brew install espeak-ng                  # macOS
sudo apt install espeak-ng              # Linux
choco install espeak-ng                 # Windows (or scoop install espeak-ng)
```

Then `Hearthbound → 🚀 Build Everything` runs Phase 46 (step 8.4) +
Phase 32 library rebuild (step 8.5) automatically — voiced dialogue
works on first launch with no shell scripts required. If neither tool
is installed, the typewriter dialogue still works (zero regression).

**77 English + 77 Arabic voice clips across 5 characters:** Doris (55, Mission 1 fully wired),
Gerrold (8, M2 stub), Marin (4, whispered notes), Narrator (4, title cards),
Pickle (6, italic asides). See [`Docs/VOICE_CASTING.md`](./Docs/VOICE_CASTING.md)
for per-character Piper model + espeak-ng variant rationale, including
the Phase 60 Arabic casting table (§ 7).

**Interactive polish (Phase 32.10):** during every voice clip the
music ducks to 55% and the ambient bed dips to 75% — the same cinematic
ducking pattern hit cozy games (Spiritfarer / Coffee Talk / Disco
Elysium) rely on. The procedural mumble VO suppresses itself for
characters with a real voice clip so we don't double-track. All
restore smoothly on clip end / skip / hide / choice.

**Swapping a voice** (commercial composer drop, booth-recorded actress,
ElevenLabs voice clone, XTTS, …): drop new `.wav` files (same lineIds,
22 kHz mono PCM16) into `Assets/_Project/Audio/Voice/<Character>/`
(English) or `Assets/_Project/Audio/Voice/ar/<Character>/` (Arabic),
re-run `🎙️ Phase 32 — Rebuild Voice Library` (EN) or
`🎙️ Phase 60 — Bind Arabic Voice Clips` (AR), press Play.
**No code change.** That is **D-058** + **D-064**, the per-locale
file-swap policy.

`🔍 Diagnose Build` now includes a voice-library audit (Step 6) so you
can verify the bound-clip count matches your `.wav` directory at a
glance.

### Player-facing flow

```
Bootstrap → Main Menu (Tone Compass on first run) →
Lane (autumn dusk, 6-step OnboardingOverlay on first play) → walk to the Hollow door →
Hollow → meet Doris (she visibly turns to face you and talks) → choose a price →
polish her First Loaves orb → read Marin's Note on the workbench →
Evening Ledger → Garden (Day 2) → harvest herb → brew tea →
Cottage → meet Gerrold (turns toward you when he speaks) →
moral choice (Erase / Cleanse / Listen / Defer) →
(optional Cleanse mini-game) → Dream 2 cutscene →
Evening Ledger → Main Menu
```

### Controls (visible any time via the H key, also on-screen via the ControlHintsHUD)

| Action | Key / Stick |
|---|---|
| Move | WASD / Arrows / Gamepad left stick |
| **Sprint** | **Left Shift / Gamepad LStick click** (off in Gentle Mode) |
| **Jump** | **Space / Gamepad south** (off in Gentle Mode) |
| Interact | E / Gamepad ▢ |
| Advance dialogue | Click / Space / Enter |
| Choose dialogue option | 1 / 2 / 3 / 4 (or click) |
| Polish orb | Hold left mouse, draw slow circles |
| **Camera look** | **Hold Right Mouse + drag / Gamepad right stick** |
| **Camera zoom** | **Mouse scroll / Gamepad LB-RB** |
| Pause | Escape |
| Help / Controls card | H |

### Comfort & accessibility (in Settings panel + Pause menu)

- **Gentle Mode** — longer timers, no fail states, more forgiving cleanse tolerance, and **disables sprint + jump** for a pure cozy walk.
- **Auto-Complete Polish** — skip the mini-game, keep the narrative beat.
- **Auto-Complete Cleanse** — same, for Mission 2.
- **Subtitle Size** — 4 tiers (Small / Medium / Large / Huge).
- **Language** *(Phase 60)* — English / العربية. Persists across sessions; live-flips every UI label on the same frame.
- **Master / Music / SFX / Ambient / Voice volumes** — persisted via PlayerPrefs across sessions.
- **Tone Compass** — first-run choice between Gentle / Standard / Deep tones.
- **OnboardingOverlay** — 6-step cozy walkthrough on first play. Skippable from frame 1; per-save flag so it never repeats.
- **ControlHintsHUD** — always-visible parchment chip strip at the bottom-left of every gameplay scene.

---

## 📂 What's in This Repo

| Path | Description |
|---|---|
| [`GAME_DESIGN.md`](./GAME_DESIGN.md) | Full game design document — vision, mechanics, market analysis, monetization, revenue projections (~$44.7M 3-year potential) |
| [`Docs/ARCHITECTURE.md`](./Docs/ARCHITECTURE.md) | Technical architecture — asmdef graph, service locator, save schema, mobile constraints, risk register |
| [`Docs/PROGRESS.md`](./Docs/PROGRESS.md) | Live progress log — current phase, decisions, known issues, next steps |
| [`Docs/LOCALIZATION_GUIDE.md`](./Docs/LOCALIZATION_GUIDE.md) | **Phase 60 — Arabic Localization MVP.** Architecture, runtime API, RTL rendering, voice acting pipeline, editor workflow, D-060 → D-064. |
| [`Docs/VOICE_CASTING.md`](./Docs/VOICE_CASTING.md) | **Canonical voice casting table — 5 characters × Piper model × espeak-ng fallback × runtime tuning. § 7 = Arabic casting (Phase 60).** |
| [`Docs/Depth_Bible/`](./Docs/Depth_Bible/) | 17-codex deep design bible + 8-doc Mission 1-2 focus folder. **Codex 17 = Phase 60 Arabic voice signatures.** |
| [`Tools/generate_voices.sh`](./Tools/generate_voices.sh) | **Phase 32.6 — Voice Acting MVP (Piper TTS, EN).** Generates 77 English voice clips. Idempotent. |
| [`Tools/generate_voices_ar.sh`](./Tools/generate_voices_ar.sh) | **Phase 60 — Arabic Voice Acting (Piper TTS, AR).** Generates 77 Arabic voice clips. Idempotent. |
| [`Tools/download_voice_models.sh`](./Tools/download_voice_models.sh) | One-time downloader for the 5 referenced Piper voice models (4 EN + 1 AR). |
| [`CHANGELOG.md`](./CHANGELOG.md) | Versioned release history (currently **0.9.0-arabic-localization** — full UI + dialogue + voice in Arabic, D-060 → D-064) |
| [`Assets/_Project/Localization/Resources/`](./Assets/_Project/Localization/Resources/) | **Phase 60 — 4 JSON translation tables: `loc.en.json` (197 keys, source-of-truth) · `loc.ar.json` (197 keys, MSA cozy register) · `dialogue.en.json` (sentinel) · `dialogue.ar.json` (84 lineIds).** |
| [`Assets/_Project/Scripts/`](./Assets/_Project/Scripts/) | ~25k LOC across 10 asmdef-isolated subsystems (Core, Memory, Player, MiniGames, UI, Dialogue, Cutscene, Save, Mission, Audio) |
| [`Assets/_Project/Scenes/`](./Assets/_Project/Scenes/) | 6 Unity scenes built procedurally by the Phase 23 capstone |
| [`Assets/_Project/Yarn/`](./Assets/_Project/Yarn/) | 8 Yarn Spinner dialogue files (Doris M1, Gerrold M2, Marin notes, Pickle, Codex, Dreams, Evening Ledger, Choice Cards) |
| [`Assets/_Project/Tests/EditMode/`](./Assets/_Project/Tests/EditMode/) | NUnit EditMode tests — 15 localization tests (Phase 60) + 10 audio-wiring tests + Core/Save/Player tests |

## 🎯 The Market Gap

Cozy games oversaturate the **farming + relationships** axis. But cozy-narrative-investigation is wide open. *Spiritfarer* sold 2M+ on emotional cozy alone. *Strange Horticulture* sold 500k+ at 96% positive on tactile-shop investigation. **Nobody has fused these.**

| Signal | Evidence |
|---|---|
| r/CozyGamers (480k) | Top complaint: "Stardew clones all have the same farming loop" |
| Coral Island | 8M+ wishlists — proves cozy market scale |
| TikTok #CozyGames | 4.8B views — appetite for novelty is huge |
| **Arabic gaming market** | **Phase 60 unlocks the MENA territory: ~400M Arabic speakers, growing PC/mobile share, almost no cozy narrative SKUs in Arabic.** |

See [`GAME_DESIGN.md`](./GAME_DESIGN.md) §2 for full demand-signal analysis.

## 🧭 Core Pillars

1. **Every villager is fully written** — no filler dialogue
2. **Tactile over textual** — memories are physical objects you hold
3. **Choices don't punish, they shape**
4. **Cozy framing, deep substance** — pace stays warm even in heavy moments
5. **A grand mystery underneath** — why did the previous Hollow-keeper vanish?
6. **Localized from the ground up** *(Phase 60)* — English + Arabic ships day-one; architecture supports French, Persian, Urdu, Japanese with a single Locale enum row.

## 🏗️ Implementation Status

**Current version**: `0.9.0-arabic-localization` (PR #15 open — full Phase 60 MVP)

| Stage | Status |
|---|---|
| Architecture, scripts, mini-games, save, UI | ✅ Complete |
| Polished playable Mission 1 + Mission 2 (Phase 23 + 24) | ✅ Complete |
| Player Controller + Animation pipeline (Phase 26) | ✅ Complete |
| Build EVERYTHING master capstone (Phase 27) | ✅ Complete |
| Mission 1 Polish v2 — 8-cottage village + Hollow facade + URP volumes (Phase 32) | ✅ Complete |
| Phase 36-43 — Cutscene Library + Procedural Audio + Save Restoration + Listen Camera | ✅ Complete |
| Phase 32 — Voice Acting MVP (Piper TTS, D-058) | ✅ Complete |
| Phase 32.6 → 32.10 — Piper open-source + interactive polish (D-059) | ✅ Complete |
| Phase 48-51 — Depth Layer (Cold Open + Echo Hologram + Preface Beat + Memory Web) | ✅ Complete |
| **Phase 60 — Arabic Localization MVP (UI + dialogue + voice, D-060 → D-064)** | ✅ **Complete — this branch** |
| 20-person greenlight playtest | ⬜ Next |
| Mission 3-10 + procedural villagers | ⬜ Post-greenlight |

See [`Docs/PROGRESS.md`](./Docs/PROGRESS.md) for the live ledger and:
- [`Docs/Phase39_Greenlight_Signoff.md`](./Docs/Phase39_Greenlight_Signoff.md) — v0.7.0 foundation closing report
- [`Docs/Phase44_Polish_Layer_Signoff.md`](./Docs/Phase44_Polish_Layer_Signoff.md) — v0.7.1 polish layer closing report
- [`Docs/LOCALIZATION_GUIDE.md`](./Docs/LOCALIZATION_GUIDE.md) — Phase 60 architecture + runtime API

---

Part of the **Abdulmalek Agents** game-concept portfolio · 2026
