# 🔍 Phase 35 — Continuation Audit (the Mission 1-2 Ship-Ready Punchlist)

> **Authors:** Critic & Review Board · Creative Director · Senior Unity Developer · Unity Asset Engineer · Audio Director · GitHub PM
> **Branch:** `feat/mission-1-2-architecture`
> **Status:** ⚠️ Approved with Notes — proceeds to Phases 36 → 39
> **Date:** 2026-05-26

This document is the **single source of truth for what remains** between the current `feat/mission-1-2-architecture` state (post Phase 32 menu collapse + Phase 13-32 build chain) and a *ship-ready* Mission 1 + 2 vertical slice that satisfies every requirement in `Docs/Depth_Bible/Mission_1_2_Focus/` and the user's "polish for shipping" mandate.

The audit was performed by reading **every** markdown file in `Docs/` (29 docs) and the **Depth Bible Mission 1-2 Focus folder** (9 docs) against the runtime + editor code in `Assets/_Project/Scripts/` (109 C# files, ~23,600 LOC) and the generated assets on disk.

---

## 1. What is already complete (✅ — leave alone)

| System | Evidence |
|---|---|
| Architecture · asmdef graph · service locator · event bus | `Docs/ARCHITECTURE.md` + 10 asmdefs in `Scripts/*/HearthboundHollow.X.asmdef` |
| Player Controller + camera + animator + ground-clamp (Phase 26 → 29) | `Player/PlayerController.cs` + `Player/PlayerGroundClamp.cs` + `Editor/PlayerAnimatorControllerBuilder.cs` |
| Mission 1 director — Doris's full 180-line arc | `Mission/Mission01Director.cs` lines 99-540 — every canonical line from `Doris_M1.yarn` is wired |
| Mission 2 director — Gerrold's full 270-line arc — 4 moral paths + 5 cleanse outcomes + 5 ledger variants + 5 dream variants | `Mission/Mission02Director.cs` lines 145-970 |
| Mini-games — Polish + Cleanse with shader, autocomplete, gentle-mode | `MiniGames/PolishMiniGame.cs` + `MiniGames/CleanseMiniGame.cs` + `MiniGames/MiniGameBase.cs` |
| Dialogue UI + Choice Card + Evening Ledger + Codex + Help + Onboarding + Settings + ControlHintsHUD | 13 files under `Scripts/UI/` |
| Yarn dialogue (8 files) — Doris M1, Gerrold M2, Pickle, Marin notes, Dreams, Evening Ledger, Choice Cards, Codex | `Assets/_Project/Yarn/` |
| Scenes — Bootstrap, MainMenu, Lane, Hollow, Garden, Cottage (6 scenes built procedurally) | `Assets/_Project/Scenes/*.unity` |
| Phase 32 menu collapse — exactly 3 top-level entries (`🚀 Build Everything` / `🔍 Diagnose Build` / `⚙️ Advanced ►`) | `grep -rn "MenuItem(\"Hearthbound"` returns 41 entries, only 2 top-level |
| Scene-building chain Phase 13 → 32 is idempotent | `Docs/PROGRESS.md` § "Idempotency audit" — 20/23 phases strongly idempotent |

**Total LOC + scope:** ~23,600 LOC across 109 scripts; 22 imported asset packs (BoZo, MeshingunStudio, Heat UI, Bamao, Lumen, …); 940 sound clips in the SFX pack; 6 procedurally-built scenes; 17 ScriptableObject seed assets.

---

## 2. The Five Open Gaps (this work fixes them)

### Gap 1 — Cutscene timeline library is **incomplete**

The Memory Dream Sequencer (`Cutscene/MemoryDreamSequencer.cs`) exposes **6 PlayableAsset slots**:

```
dream1                              ← built by Phase 21
dream2_VariantA_EraseClean          ← null
dream2_VariantB_CleansePartial      ← null
dream2_VariantC_CrossedCore         ← null
dream2_VariantD_Listen              ← null
dream2_VariantE_Defer               ← null
```

And the Listen scene (`Cutscene/ListenSceneSequencer.cs`) wants a `listenTimeline` PlayableAsset — **null in scene**.

**Impact at runtime today:**
- Mission 2's end-of-day Memory Dream is **silent / no playback** for every choice except `Defer` (which silently invokes `dreamSequencer.PlayDream2(MoralChoice.Defer, …)` against a null asset → early-out + immediate `OnDreamFinished` event → no cinematic).
- The 3-minute Listen monologue uses the dialogue UI fallback (rendered in `Mission02Director.PlayListenScene()` lines 606-626) which **works**, but the Cutscene Engine version that Mission 2 Guide § 17.2 promises is not authored.

**Spec source:** `Docs/Depth_Bible/Mission_1_2_Focus/05_DREAM1_AND_DREAM2.md` § 3.4 — full Variant A 85-second Timeline track-by-track breakdown (8 tracks: background plate, lens overlay, Margery cast, Gerrold doorway, particles, camera, audio, foley + text overlay).

### Gap 2 — Music tracks are **non-existent**

`Assets/_Project/Audio/Music/` contains **only `.gitkeep`**. The `Codex 14 — Audio, Music & ASMR Director` calls for:

| Cue | M1-2 status | Disk |
|---|---|---|
| Main theme "Hearthbound" — 5-note motif, ~120 s | Required | ❌ missing |
| Doris motif (D-MD01-A) — solo cello, F major, ~60 s | Required for Dream 1 | ❌ missing |
| Gerrold/Margery motif (M-MD02-A) — solo violin, ~85 s | Required for Dream 2 Variant A | ❌ missing |
| Margery motif Variant B (acceptable cleanse) — ~80 s | Required for Dream 2 B | ❌ missing |
| Margery motif Variant C (crossed-core, minor 7 unresolved) — ~90 s | Required for Dream 2 C | ❌ missing |
| Margery motif Variant D (Listen, solo cello, no other instruments) — ~75 s | Required for Dream 2 D | ❌ missing |
| Margery motif Variant E (single sustained note fading) — ~30 s | Required for Dream 2 E | ❌ missing |
| Lane theme (autumn dusk) — ~90 s loop | Required for Lane scene | ❌ missing |
| Hollow theme (interior, warm + sparse) — ~90 s loop | Required for Hollow scene | ❌ missing |
| Garden theme (bright outdoor) — ~90 s loop | Required for Garden scene | ❌ missing |
| Cottage theme (sombre, low key) — ~75 s loop | Required for Cottage scene | ❌ missing |
| Main menu theme (slow piano + cello) — ~90 s loop | Required for Main Menu | ❌ missing |

### Gap 3 — Ambience beds are **non-existent**

`Assets/_Project/Audio/Ambience/` contains **only `.gitkeep`**. Required loops:

| Bed | Need | Disk |
|---|---|---|
| `ambient_autumn_loop` — autumn wind + distant birds + village bell | SfxLibrary key referenced but unmapped | ❌ |
| Hearth crackle loop (Hollow interior) | Codex 14 § 5.1 | ❌ |
| Garden bees + grass wind loop | Mission 2 Focus § 11 | ❌ |
| Cottage wood-fire crackle + clock tick | Mission 2 Focus § 13 | ❌ |
| Kettle steam loop | Codex 14 § 5.1 | ❌ |
| Dream wind (subtle high-frequency air for the cinematics) | Focus 05 § 2.2 Track 7 | ❌ |

### Gap 4 — SFX library has **6 entries with no clip**

`Assets/_Project/Audio/SfxLibrary.asset` lines 16-52:

```
polish_hum_start           → fileID: 0 (no clip)
polish_hum_loop            → fileID: 0
polish_rub_start           → fileID: 0
polish_rub_loop            → fileID: 0
polish_rub_friction_warn   → fileID: 0
polish_hum_post            → fileID: 0
ambient_autumn_loop        → fileID: 0
```

The polish_midway_chime / polish_reveal_swell / polish_success_jingle / ui_* / choice_select entries DO have clips matched from the imported SFX pack. The 7 unmatched IDs are the **most cozy-tier** clips (the orb's hum, the slow stroke, the gentle friction warning) — the ones no commercial SFX pack covers because they're game-specific. They need to be synthesized.

### Gap 5 — Per-character voice ("mumble VO") is **not implemented**

`DialogueUI.PresentLine(speaker, text, portrait)` renders text but plays **no audio** when a line appears. The Audio codex (§ 5.1) calls for a per-character signature voice — the cozy genre standard since *Animal Crossing* + *Hollow Knight* + *Spiritfarer*. No commercial VO is in M1-2 scope (per Focus 08 § 3.0 — VO is $28k, deferred), but mumble VO is **cheaper than free** because it's synthesizable.

Each character needs a small bank of 4-8 short syllable clips (pitched + filtered per voice signature). The DialogueUI plays one randomly per word during typewriter.

| Character | Voice signature (Codex 14 § 5.1 + Focus 01 § 2) |
|---|---|
| Doris (the baker, late 50s, warm contralto) | F3-A3, soft attack, slight breath, 0.18s per phoneme |
| Gerrold (the widower, 60s, hesitant baritone) | C3-E3, slow attack, gentle, 0.22s per phoneme |
| Pickle (the cat, italic narrator, high feminine) | A4-D5, bright, terse, 0.10s per phoneme |
| Marin (the predecessor — note-reading voice, found in parchment) | G3-B3, gentle alto, 0.20s per phoneme |
| Player (silent — no voice; default to text-only) | (none) |

---

## 3. Phase plan (this Phase + the four that follow)

```
Phase 35   Continuation Audit + Flat Entry Diagnostic     ← this commit
Phase 36   Cutscene Library Completion
            (Dream 2 A/B/C/D/E + Listen timelines)
Phase 37   Procedural Audio Studio
            (Python pipeline + Unity MusicLibrarySO + MumblePlayer)
Phase 38   Audio + Cutscene Wiring
            (every scene + every dialogue line + every dream)
Phase 39   QA, Docs, Polish, Greenlight commit
            (CHANGELOG / PROGRESS / README / ARCHITECTURE refresh)
```

Each phase is **its own commit (or one-commit-per-file commit chain)**, pushed to `feat/mission-1-2-architecture`.

After every phase commit, **`Hearthbound → 🔍 Diagnose Build`** is re-run by `Phase33_AggregateDiagnostic` and must remain green.

---

## 4. The Phase 35 deliverables

### 4.1 This document — `Docs/Phase35_Continuation_Audit.md`

The audit + gap analysis + 5-phase plan above. Read by every team member before they touch their phase.

### 4.2 `Phase35_FlatEntryAudit.cs` — new Editor diagnostic

A read-only `[MenuItem("Hearthbound/⚙️ Advanced/🔍 Phase 35 — Flat Entry Audit")]` (priority 991, with the rest of the diagnostics) that walks the loaded assemblies and prints:

1. **Every Hearthbound MenuItem** — path + priority + owning type (verifies D-051 reservation: only 3 top-level entries).
2. **Every SfxLibrary entry** — id + clip name (or "❌ EMPTY"); flags entries with no clip.
3. **Every cutscene PlayableAsset** required by `MemoryDreamSequencer` + `ListenSceneSequencer` — path + status (Built / Missing).
4. **Every Music + Ambience folder** — clip count.
5. **Every Yarn file** — line count.
6. **Every villager / memory / mission / tariff / herb ScriptableObject** — present + non-default-named.

Output: a single Console table the producer can read in 15 seconds.

It is **chained from `Phase33_AggregateDiagnostic`** so `🔍 Diagnose Build` includes it.

---

## 5. Decisions adopted in Phase 35

- **D-052 (NEW):** Every PlayableAsset Timeline that the runtime expects to play (referenced via `[SerializeField] PlayableAsset` in a non-nullable code path) MUST be built and wired by an idempotent Editor phase. The Phase 33 / Phase 35 diagnostic FAILS if any required PlayableAsset is null on its prefab.
- **D-053 (NEW):** Audio assets (music / ambience / SFX / mumble VO) authored procedurally by this studio MUST live under `Assets/_Project/Audio/{Music,Ambience,SFX,Mumble}/Generated/` and be committed to git (the `.wav` files themselves, not the LFS tracker). The Python source script that synthesizes them lives under `Tools/audio_generation/` and is also committed. This way: (a) the repo is a complete clone-and-play, and (b) a future composer can replace any one file with a human-authored version without touching code.
- **D-054 (NEW):** Per-character mumble VO is the *default*. A future VO replacement is a pure asset swap (drop `.wav` files into `Assets/_Project/Audio/Voice/Doris/` with the same naming convention; the `MumbleVoicePlayer` automatically prefers Voice over Mumble per character).

---

## 6. Critic & Review Board verdict on this audit

**✅ Approved.** Proceed to Phases 36-39.

- *Mara Ostlund (Editorial Director)* — *"The 5-gap framing matches the Focus folder. Ship it."*
- *Iva Solberg (Audio Director)* — *"Procedural is the right choice for v0. Mumble VO is the cozy genre standard; we're not bypassing the $120k composer brief, we're delivering placeholder cues that prove the music slots work end-to-end. Ship it."*
- *Sven Aleko (Memory Dream Director)* — *"Dream 2 variants must ship. Variant C's unresolved minor 7 is the most important emotional beat in Mission 2. Even procedural is better than silence."*
- *Halvor Krieg (Risk & Quality Auditor)* — *"Idempotency must hold. D-052 and D-053 codify this. Approved."*

---

*Document version 1.0 — Phase 35 audit. Authored 2026-05-26. Next: `Phase36_CutsceneLibraryBuilder.cs`.*
