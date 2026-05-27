# 🫖 Hearthbound Hollow

> *"Some memories want to be sold. Some don't."*

A **single-player cozy narrative simulation** set in a small autumnal village where memories are a physical, tradable commodity. You inherit a memory-brokerage shop. Buy painful memories from villagers wanting to forget. Sell preserved ones to those who long to remember. Polish, weave, and restore the fragmented — but choose carefully what you keep.

**Pitch in one line:** *Stardew's warmth × Spiritfarer's heart × Strange Horticulture's puzzles × Disco Elysium's writing — at cozy scale.*

---

## 🎮 Run the polished playable Mission 1 + 2 (Unity)

The `feat/mission-1-2-architecture` branch ships a fully playable, polished vertical slice of Missions 1 and 2 — six scenes, two villager arcs, four moral choices, two memory dreams, a complete cozy-comfort/accessibility layer, **a robust WASD + sprint + jump controller, a smooth third-person follow camera, a Mixamo-ready Humanoid Animator, Doris/Gerrold animated dialogue beats, a 6-step first-play OnboardingOverlay, a persistent context-aware ControlHintsHUD, Phase 32 v2 polish (8 residential cottages, Hollow shop facade, hearth dressing, cozy URP cinematic volumes), AND (new in 0.7.0-procedural-audio + 0.7.1-polish-layer) a *complete procedural audio score* + *narrative audio reactivity* + *cinematic Listen camera* + *audio-aware save resume*, AND (new in 0.8.0-level-polish) a *24×36 m bounded lane* + *autumn-evening procedural skybox* + *cottage hearth flicker* + *onboarding wayfinding lanterns*.**

### One-click build

1. Clone the repo and check out `feat/mission-1-2-architecture`.
2. Open the project in **Unity 6 LTS (6000.4.4f1)**. Packages auto-install (~30–90 s).
3. Menu → **`Hearthbound → 🚀 Build Everything`** → click **`Build`** in the confirmation dialog → sit back ~110 s.
4. Press **Play**.

> 🔁 **After every `git pull`**, repeat step 3 — click **🚀 Build Everything**. The chain is idempotent (every Phase 13/14/15/.../46 sub-builder uses load-or-create + heal-then-save), so re-running it produces the same result as running it once. That is the *entire* recommended workflow. No other clicks required.

The `🚀 Build Everything` menu item runs **thirteen sub-capstones** — polished scenes, player AnimatorController, NPC AnimatorController, narrative hooks, Player Rig Doctor (foot-bone anchor), the OnboardingOverlay + ControlHintsHUD, dialogue choice repair, **Phase 32 Mission 1 Polish v2** (cottages + facade + URP cozy volumes), **Phase 36 Cutscene Library** (Dream 1 + 5× Dream 2 variants + Listen Scene Timelines), **Phase 37 Procedural Audio Studio** (75 procedurally-synthesised .wav cues + MusicLibrarySO + AmbienceLibrarySO + MumbleVoiceLibrarySO), **Phase 38 Audio + Cutscene Wiring** (Bootstrap rig + per-scene beacons + DreamAudioBinder), **Phase 42 Listen Scene Camera** (4-waypoint cinematic 180s path), AND **Phase 46 Level Boundaries + Polish** (24×36 m playable area, perimeter walls, invisible void blockers, autumn skybox, guide lanterns, cottage hearth flicker, framed photograph) — sets up Build Settings, and opens the Bootstrap scene. Reflection-driven so missing phases skip gracefully.

> 🎵 **Audio is procedural and committed-as-source.** No paid SaaS (ElevenLabs / Suno / Udio), no DAW, no human composer needed. The C# Editor builder `Phase37_ProceduralAudioStudio` synthesises all 75 .wav files deterministically (seed = 1972, Doris's first loaves) on every Build Everything run. A future commercial-composer drop into `Assets/_Project/Audio/Music/` (or `/Voice/`) is a pure file-swap — no code change. See [`Tools/audio_generation/README.md`](./Tools/audio_generation/README.md) for the composer brief.

> 🎭 **Narrative beats sound alive.** Phase 41's `MissionAudioHooks` is a runtime EventBus → audio router that translates 8 narrative events into per-beat SFX + music-duck reactions: polish completes → success swell + hum_post; cleanse Perfect → reveal swell; cleanse Crossed-Core → friction warning + music duck; moral choice → choice select + duck; tea brewed → kettle pour + confirm; day ends → ui close + slow music drift. Zero changes to Mission01/02Director — the audio layer is a passive observer.

> 💾 **Saves remember the music.** Phase 43 persists `lastMusicId` + `lastAmbienceId` + `playedDreamVariants` into the save file (schema v2). Load a game → the music + ambient bed restore on the first frame, before any scene-bootstrap audio kicks in. No silent-load gap.

> 🏰 **The world has edges now.** Phase 46 widens the lane to 24×36 m, encloses every scene with a visible stone-wall perimeter + 8 m-tall invisible BoxCollider void-blockers (defence-in-depth, D-059), and applies a procedural autumn-evening skybox to all four gameplay scenes (D-060). Five guide lanterns pulse south-to-north along the cobble path (the cozy game's onboarding language — D-061). The cottage hearth flickers with summed-sine + Perlin-jitter realism; the mantel carries a sepia framed photograph; Margery's chair has a book on its arm. See [`Docs/Phase46_LevelBoundaries_Signoff.md`](./Docs/Phase46_LevelBoundaries_Signoff.md).

> 💡 Want richer animation? Drop 6 Mixamo FBXs into `Assets/_Project/Animations/Mixamo/` per [`Docs/ANIMATION_REQUIREMENTS.md`](./Docs/ANIMATION_REQUIREMENTS.md) § 3 and re-run **`Hearthbound → 🚀 Build Everything`**. The game ships polished without them.

> 🔍 Verify wiring any time with **`Hearthbound → 🔍 Diagnose Build`** — read-only **6-step** aggregate audit that chains Phase 23 / 26 / 32 sub-diagnostics + the Phase 35 continuation audit (cutscene timelines, audio folders, SfxLibrary, Yarn, seed-assets) + the Phase 40 audio-wiring deep-dive (MusicLibrary / AmbienceLibrary / MumbleVoiceLibrary / DreamAudioBinder cueMap / per-scene SceneAudioBeacon) + the Phase 46 level-polish diagnostic (skybox / sun / perimeter / blockers / guide lanterns / hearth flicker) — under one click.

> ⚙️ **Power users:** Every legacy per-phase entry (Phase 13 / 14 / 15 / 22 / 23 / 24 / 26 / 27 / 29 / 30 / 31 / 32.1 / 32.2 / 32.3 / 32.4 / 35 / 36 / 37 / 38 / 40 / 42 / **46.1 / 46.2 / 46.3 / 46.4 / 46.5 / 46.6 / 46.7 / 46.8** …) is still accessible under **`Hearthbound → ⚙️ Advanced ►`** with its original priority intact. The Phase 32 UX track only moved them out of the top level — no behaviour changes. See **D-051** in [`Docs/PROGRESS.md → Phase 32 — Menu collapse + idempotency audit`](./Docs/PROGRESS.md) for the full migration table. Decisions **D-052 / D-053 / D-054** in [`Docs/Phase39_Greenlight_Signoff.md`](./Docs/Phase39_Greenlight_Signoff.md) codify the audio + cutscene policy; **D-055 / D-056** in [`Docs/Phase44_Polish_Layer_Signoff.md`](./Docs/Phase44_Polish_Layer_Signoff.md) codify the save-resume + install-pattern policy; **D-058** in [`Docs/PROGRESS.md → Phase 32 — Voice Acting MVP`](./Docs/PROGRESS.md) codifies the voice-clip file-swap policy; **D-059 / D-060 / D-061 / D-062** in [`Docs/Phase46_LevelBoundaries_Signoff.md`](./Docs/Phase46_LevelBoundaries_Signoff.md) codify the boundary / skybox / wayfinding / idempotency policy.

### Voice acting (Phase 32 — macOS `say` MVP)

Run `bash Tools/generate_voices.sh` after `git pull` to (re)generate macOS `say` placeholder voices for Doris. The script produces 48 `.wav` files (~10–20 MB total, 22 kHz mono PCM16) under `Assets/_Project/Audio/Voice/Doris/`, then in Unity click `Hearthbound → ⚙️ Advanced → 🎙️ Phase 32 — Rebuild Voice Library` to auto-bind the clips into `Resources/HearthboundVoiceLibrary.asset`. Swap in your own clips by dropping `.wav` files (same lineId filenames — `doris_m1_greet_01.wav` etc.) into `Assets/_Project/Audio/Voice/Doris/` — any TTS (ElevenLabs / XTTS / Piper) or a booth-recorded actress works, no code change required (see **D-058**). Voice plays in sync with the typewriter; skipping a line via Space/click stops it instantly.

### Player-facing flow

```
Bootstrap → Main Menu (Tone Compass on first run) →
Lane (autumn dusk, 6-step OnboardingOverlay on first play, 5 pulsing guide lanterns south-to-north) → walk to the Hollow door →
Hollow → meet Doris (she visibly turns to face you and talks) → choose a price →
polish her First Loaves orb → read Marin's Note on the workbench →
Evening Ledger → Garden (Day 2, fenced perimeter) → harvest herb → brew tea →
Cottage (hearth flickers, photograph on mantel) → meet Gerrold (turns toward you when he speaks) →
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
- **Master / Music / SFX / Ambient / Voice volumes** — persisted via PlayerPrefs across sessions. (Voice added in Phase 37 for the mumble VO channel; Phase 32 Voice Acting MVP uses the same channel for Doris's real voice.)
- **Tone Compass** — first-run choice between Gentle / Standard / Deep tones.
- **OnboardingOverlay** — 6-step cozy walkthrough on first play (Welcome → WASD → E → LMB polish → Esc/H comfort → Ready). Skippable from frame 1; per-save flag so it never repeats.
- **ControlHintsHUD** — always-visible parchment chip strip (Move · Interact · Help) at the bottom-left of every gameplay scene. The [E] chip lights up to full alpha + shows the interactable's prompt when one is in range.

---

## 📂 What's in This Repo

| Path | Description |
|---|---|
| [`GAME_DESIGN.md`](./GAME_DESIGN.md) | Full game design document — vision, mechanics, market analysis, monetization, revenue projections (~$44.7M 3-year potential) |
| [`Docs/ARCHITECTURE.md`](./Docs/ARCHITECTURE.md) | Technical architecture — asmdef graph, service locator, save schema, mobile constraints, risk register |
| [`Docs/PROGRESS.md`](./Docs/PROGRESS.md) | Live progress log — current phase, decisions, known issues, next steps |
| [`Docs/Phase35_Continuation_Audit.md`](./Docs/Phase35_Continuation_Audit.md) | Phase 35 audit + 5-gap punchlist + plan for Phases 36-39 |
| [`Docs/Phase39_Greenlight_Signoff.md`](./Docs/Phase39_Greenlight_Signoff.md) | Phase 35-38 closing report + Critic & Review Board sign-off (v0.7.0 foundation) |
| [`Docs/Phase44_Polish_Layer_Signoff.md`](./Docs/Phase44_Polish_Layer_Signoff.md) | Phase 40-43 closing report + Critic & Review Board sign-off (v0.7.1 polish) |
| [`Docs/Phase46_LevelBoundaries_Signoff.md`](./Docs/Phase46_LevelBoundaries_Signoff.md) | Phase 46 closing report + Critic & Review Board sign-off (v0.8.0 level polish — boundaries, skybox, interior items, guide lights) |
| [`Docs/ANIMATION_REQUIREMENTS.md`](./Docs/ANIMATION_REQUIREMENTS.md) | Player Animator graph, clip roster, Mixamo download + Humanoid retargeting guide |
| [`Docs/SCENE_ASSEMBLY_GUIDE.md`](./Docs/SCENE_ASSEMBLY_GUIDE.md) | Per-scene build steps + the ⚡ Fast path (`🚀 Build Everything` one click) |
| [`Docs/Depth_Bible/`](./Docs/Depth_Bible/) | 16-codex deep design bible + 8-doc Mission 1-2 focus folder |
| [`Docs/Asset_Analysis_Mission1-2.md`](./Docs/Asset_Analysis_Mission1-2.md) | Detailed asset selection + integration plan for the 17 imported asset packs |
| [`Docs/Phase27_Environment_Polish_Plan.md`](./Docs/Phase27_Environment_Polish_Plan.md) | Phase 27 environment polish source-of-truth doc |
| [`Docs/Phase32_Mission1_Polish.md`](./Docs/Phase32_Mission1_Polish.md) | Phase 32 Mission 1 Polish v2 source-of-truth doc |
| [`Tools/audio_generation/README.md`](./Tools/audio_generation/README.md) | Procedural audio composer brief — what cues exist, how to extend, how to swap to human-authored audio |
| [`Tools/generate_voices.sh`](./Tools/generate_voices.sh) | **Phase 32 — Voice Acting MVP.** macOS `say` driver that generates 48 Doris voice clips into `Assets/_Project/Audio/Voice/Doris/`. Idempotent. |
| [`CHANGELOG.md`](./CHANGELOG.md) | Versioned release history (currently **0.8.0-level-polish** — Phase 46 boundaries + skybox + cottage interior polish, D-059..D-062) |
| [`Assets/_Project/Scripts/`](./Assets/_Project/Scripts/) | ~25k LOC across 10 asmdef-isolated subsystems (Core, Memory, Player, MiniGames, UI, Dialogue, Cutscene, Save, Mission, Audio) |
| [`Assets/_Project/Scenes/`](./Assets/_Project/Scenes/) | 6 Unity scenes built procedurally by the Phase 23 capstone |
| [`Assets/_Project/Prefabs/Environment/`](./Assets/_Project/Prefabs/Environment/) | 4 cottage prefab variants (A_Bakery / B_Plain / C_Gabled / D_Corner) |
| [`Assets/_Project/Prefabs/Cutscene/`](./Assets/_Project/Prefabs/Cutscene/) | MemoryDreamRig with Dream 1 + 5× Dream 2 variants + Listen Scene Timeline wired (Phase 36) |
| [`Assets/_Project/Settings/`](./Assets/_Project/Settings/) | URP cozy volume profiles + Input Actions |
| [`Assets/_Project/Art/Sky/`](./Assets/_Project/Art/Sky/) | Procedural autumn-evening skybox material (Phase 46.1) |
| [`Assets/_Project/Yarn/`](./Assets/_Project/Yarn/) | 8 Yarn Spinner dialogue files (Doris M1, Gerrold M2, Marin notes, Pickle, Codex, Dreams, Evening Ledger, Choice Cards) |
| [`Assets/_Project/Tests/EditMode/`](./Assets/_Project/Tests/EditMode/) | NUnit EditMode tests — 10 audio-wiring tests (Phase 40) + Core/Save/Player tests |
| [`prototype.html`](./prototype.html) | The original HTML5 prototype that proved the design before Unity work began |

## ▶️ Run the HTML prototype (legacy)

1. Download or [view raw](./prototype.html) `prototype.html`
2. Open it in any modern browser — no Unity required.

The HTML prototype is preserved for reference but the Unity branch is the canonical playable.

## 🎯 The Market Gap

Cozy games oversaturate the **farming + relationships** axis. But cozy-narrative-investigation is wide open. *Spiritfarer* sold 2M+ on emotional cozy alone. *Strange Horticulture* sold 500k+ at 96% positive on tactile-shop investigation. **Nobody has fused these.**

| Signal | Evidence |
|---|---|
| r/CozyGamers (480k) | Top complaint: "Stardew clones all have the same farming loop" |
| Coral Island | 8M+ wishlists — proves cozy market scale |
| TikTok #CozyGames | 4.8B views — appetite for novelty is huge |

See [`GAME_DESIGN.md`](./GAME_DESIGN.md) §2 for full demand-signal analysis.

## 💰 Revenue Model

- **Base game $24.99** (Steam + Switch Day-1)
- **Mobile $9.99** with full content (huge LTV)
- **Free seasonal updates** + **paid story DLC** packs
- **No gacha, no energy, no P2W**
- **3-year projection: ~$44.7M gross / ~$26M net** (conservative)

## 🧭 Core Pillars

1. **Every villager is fully written** — no filler dialogue
2. **Tactile over textual** — memories are physical objects you hold
3. **Choices don't punish, they shape**
4. **Cozy framing, deep substance** — pace stays warm even in heavy moments
5. **A grand mystery underneath** — why did the previous Hollow-keeper vanish?

## 🏗️ Implementation Status

**Current version**: `0.8.0-level-polish` (PR #14 open, accumulating)

| Stage | Status |
|---|---|
| Architecture, scripts, mini-games, save, UI | ✅ Complete |
| Asset-driven prefab builders (Phase 13-21) | ✅ Complete |
| Engineering playable Mission 1 (Phase 22) | ✅ Complete |
| Polished playable Mission 1 + Mission 2 (Phase 23 + 24) | ✅ Complete |
| UI activation hotfix (Phase 25) | ✅ Complete |
| Player Controller + Animation pipeline (Phase 26) | ✅ Complete |
| Narrative Hooks (Marin's Note, Phase 26 parallel thread) | ✅ Complete |
| Build EVERYTHING master capstone + NPC Animator + diagnostic + footstep hooks (Phase 27) | ✅ Complete |
| "Half body in floor" definitive fix — live world bounds + continuous correction window (Phase 28) | ✅ Complete |
| UI Polish — UIAutoFitText on every TMP label + ChoicesContainer relocated inside dialogue box (Phase 29a) | ✅ Complete |
| Player Rig Doctor — foot-bone anchor auto-discovery + Animator sanity pass (Phase 29b) | ✅ Complete |
| OnboardingOverlay (6-step walkthrough) + ControlHintsHUD (always-visible context-aware key chips) (Phase 30) | ✅ Complete |
| Dialogue Choice Card Repair — full-width tiles + 1/2/3/4 keyboard shortcuts (Phase 31) | ✅ Complete |
| Mission 1 Polish v2 — 8-cottage village + Hollow facade + hearth dressing + cozy URP volumes (Phase 32) | ✅ Complete |
| Menu collapse — top-level Hearthbound = 🚀 Build Everything + 🔍 Diagnose Build + ⚙️ Advanced (Phase 32 UX track, D-051) | ✅ Complete |
| Phase 35 — Continuation Audit (gap punchlist + flat-entry diagnostic chained into 🔍 Diagnose Build) | ✅ Complete |
| Phase 36 — Cutscene Library Completion (Dream 2 A/B/C/D/E + Listen Scene Timelines + MemoryDreamRig re-wire) | ✅ Complete |
| Phase 37 — Procedural Audio Studio (75 procedural .wavs + MusicLibrarySO + AmbienceLibrarySO + MumbleVoiceLibrarySO + MusicPlayer + MumbleVoicePlayer) | ✅ Complete |
| Phase 38 — Audio + Cutscene Wiring (Bootstrap rig + per-scene beacons + DreamAudioBinder + 3 EventBus events) | ✅ Complete |
| Phase 39 — Greenlight Sign-Off v0.7.0 (Critic & Review Board, 30 QA + 4 audio criteria PASS) | ✅ Complete |
| Phase 40 — Audio Wiring Diagnostic + per-character mumble preview + 10 EditMode tests | ✅ Complete |
| Phase 41 — Mission Director Audio Hooks (EventBus → audio router; 8 narrative beats → SFX/duck) | ✅ Complete |
| Phase 42 — Listen Scene Camera (4-waypoint Cinemachine-agnostic 180s cinematic path) | ✅ Complete |
| Phase 43 — Save System Audio Restoration (schema v2 + lastMusicId/lastAmbienceId/playedDreamVariants + EventBus persistence/restore) | ✅ Complete |
| Phase 44 — Polish Layer Sign-Off v0.7.1 (Critic & Review Board, pre-playtest prerequisites PASS) | ✅ Complete |
| Phase 32 — Voice Acting MVP (Doris's M1 dialogue voiced via macOS `say`; D-058 codifies file-swap path) | ✅ Complete |
| **Phase 46 — Level Boundaries, Skybox & Environment Polish (24×36 m lane, perimeter walls, invisible void blockers, autumn skybox, guide lanterns, cottage hearth flicker, framed photograph, D-059..D-062)** | ✅ **Complete — this branch** |
| 20-person greenlight playtest | ⬜ Next |
| Mission 3-10 + procedural villagers | ⬜ Post-greenlight |

See [`Docs/PROGRESS.md`](./Docs/PROGRESS.md) for the live ledger and:
- [`Docs/Phase39_Greenlight_Signoff.md`](./Docs/Phase39_Greenlight_Signoff.md) — v0.7.0 foundation closing report
- [`Docs/Phase44_Polish_Layer_Signoff.md`](./Docs/Phase44_Polish_Layer_Signoff.md) — v0.7.1 polish layer closing report
- [`Docs/Phase46_LevelBoundaries_Signoff.md`](./Docs/Phase46_LevelBoundaries_Signoff.md) — v0.8.0 level polish closing report

### What the **`Hearthbound → 🚀 Build Everything`** capstone does in one click

Thirteen sub-capstones, ~110 s end-to-end, idempotent and reflection-driven (with a one-line confirmation dialog before the chain runs):

1. **Phase 23** — POLISHED Mission 1 + 2 scene assembly (chains 13-24).
2. **Phase 26 (PC + Anim)** — Player AnimatorController + SmoothFollowCamera + cameraReference + PlayerGroundClamp + Mixamo-ready 1D blend tree.
3. **Phase 26 (NPC)** — Doris/Gerrold/SilentLane animators + NpcAnimatorBridge for "IsTalking" body language.
4. **Phase 26 (Narrative Hooks)** — Marin's Note dropped on the Hollow workbench.
5. **Phase 29 (Rig Doctor)** — auto-discovers a foot bone on the Player rig and wires it as `PlayerGroundClamp.footAnchor`.
6. **Phase 30 (Onboarding + Hints)** — OnboardingOverlay on Lane (6-step first-time walkthrough) + ControlHintsHUD on every gameplay scene.
7. **Phase 31 (Dialogue Repair)** — Full-width dialogue choice tiles with 1/2/3/4 keyboard shortcuts.
8. **Phase 32 (Mission 1 Polish v2)** — 8 cottages assembled from MV modular pieces, Hollow shop facade, hearth dressing, cozy URP volumes.
9. **Phase 36 (Cutscene Library)** — Builds Dream 1 + 5× Dream 2 variant Timelines + the 180-second Listen Scene Timeline.
10. **Phase 37 (Procedural Audio)** — Synthesises 75 deterministic .wav cues + 3 audio libraries + AudioImporter mobile config.
11. **Phase 38 (Audio + Cutscene Wiring)** — Bootstrap rig + per-scene beacons + DreamAudioBinder.
12. **Phase 42 (Listen Scene Camera)** — 4-waypoint cinematic path on the Cottage scene.
13. **Phase 46 (Level Boundaries + Polish)** *(NEW v0.8.0)* — Autumn skybox + lighting (46.1), Lane boundaries + wide environment (46.2), Hollow boundaries + interior polish (46.3), Garden boundaries + path (46.4), Cottage interior polish + hearth flicker (46.5), collider hardening across all 4 scenes (46.6), guide lights + firefly wayfinding (46.7).

**Runtime self-installers (no Editor builder needed):**
- `MissionAudioHooks` *(Phase 41)* — auto-spawns via `[RuntimeInitializeOnLoadMethod]`; routes 8 narrative events into SFX + music-duck reactions.
- `VoicePlayer` *(Phase 32 Voice Acting MVP)* — Singleton MonoBehaviour, auto-creates a 2D AudioSource on `Awake`.
- `LaneGuidePulse` *(NEW v0.8.0 — Phase 46.2)* — Sine-wave breathing pulse on guide-lantern point lights with per-lantern phase delay so the wave walks north along the cobble path.
- `HearthFlicker` *(NEW v0.8.0 — Phase 46.5)* — Two summed sine waves + Perlin jitter for natural-feeling fireplace flicker; per-instance random phase.

### `Hearthbound → 🔍 Diagnose Build` — 6-step read-only audit

1. **Phase 23** — Scene/Wiring diagnostic
2. **Phase 26** — Player + Animator diagnostic
3. **Phase 32** — Mission 1 Polish v2 diagnostic
4. **Phase 35** — Continuation audit (project-wide: menu, audio folders, SfxLibrary, timelines, Yarn, seeds)
5. **Phase 40** — Audio wiring (focused: MusicLibrarySO / AmbienceLibrarySO / MumbleVoiceLibrarySO / DreamAudioBinder cueMap / per-scene SceneAudioBeacon)
6. **Phase 46** *(NEW v0.8.0)* — Level polish (skybox / sun bind / perimeter walls / void blockers / guide lanterns / hearth flicker)

### Power-user submenu — `Hearthbound → ⚙️ Advanced ►`

Every legacy per-phase entry lives under the Advanced submenu (~58 items). New Phase 46 entries:

| Menu (under ⚙️ Advanced) | What it does |
|---|---|
| `🏰 Phase 46 — Level Polish (all)` *(NEW v0.8.0)* | Master capstone — chains 46.1 → 46.7 with progress bar |
| `🌇 Phase 46.1 — Autumn Skybox + Lighting` | Procedural sky + sun bind + Trilight ambient + warm fog (all 4 scenes) |
| `🏰 Phase 46.2 — Lane Boundaries + Wide Env` | 24×36 m playable area, perimeter walls, void blockers, guide lanterns, gate |
| `🏠 Phase 46.3 — Hollow Boundaries + Interior Polish` | Wall hardening, rug, bookcase, reading chair, desk, sconces, lavender |
| `🌿 Phase 46.4 — Garden Boundaries + Path` | Fence perimeter, stepping path, dressing, meadow beyond |
| `🏚️ Phase 46.5 — Cottage Interior Polish` | Hearth flicker, photograph, books, Margery's chair, rug, candles, bedroom door |
| `🛡️ Phase 46.6 — Collider Hardening` | BoxColliders on every blocking-keyword GameObject |
| `✨ Phase 46.7 — Guide Lights + Wayfinding` | Firefly emitters along Lane + Garden paths |
| `🔍 Phase 46.8 — Diagnose Level Polish` | Read-only audit; chained into `🔍 Diagnose Build` |

The Phase 26, 27.2, 27.3, 29, 30, 31, 35 / 36 / 37 / 38 / 40 / 42 / **46.1 / 46.2 / 46.3 / 46.4 / 46.5 / 46.6 / 46.7 / 46.8** menu items follow the same `⚙️ Advanced/…` prefix pattern. See [`Docs/PROGRESS.md → Phase 32 — Menu collapse`](./Docs/PROGRESS.md) for the full top-level → Advanced migration table.

---

Part of the **Abdulmalek Agents** game-concept portfolio · 2026
