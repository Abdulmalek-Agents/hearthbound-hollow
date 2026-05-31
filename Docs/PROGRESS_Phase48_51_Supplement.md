# 📊 Hearthbound Hollow — Implementation Progress Log (Phase 48 → 51 supplement)

> Companion to `Docs/PROGRESS.md`. The main PROGRESS.md spans Phase 0
> through Phase 47; this supplement covers the v0.8.0-depth-layer
> release (Phase 48 → 51). Sections follow the canonical PROGRESS.md
> format so a future merge-back into the main file is mechanical.

---

## Legend
- ✅ Done & merged · 🟢 Done in branch (awaiting your pull) · 🟡 In progress · ⬜ Not started · 🔴 Blocked

---

## 🆕 Phase 48 → 51 — Depth Layer (the Hook + Echo + Preface + Web) 🟢 (2026-05-28)

**User request:**

> *"increase the bootstrap scene to make it hook and fun and engaging
> and with hook from the begining and then move to deepen the other
> missions push every phase and when you are stuck or go idle please
> resume and try other approach"*

### Problem in one sentence

The Bootstrap scene was a bare GameManager singleton that immediately
routed to MainMenu — no hook, no mood, no narrative weight before the
mechanic loop began. The cozy-with-weight vision (Codex 06 + Codex 11)
needed the weight to land **before mechanics**.

### Solution in one sentence

Add a 4-phase **Depth Layer** that runs additively on top of the v0.7.3
playable slice: Cold Open Cinematic (Bootstrap), Echo Hologram of Marin
(Hollow workbench), Tone-Personalized Preface Beat (Lane), and Memory
Web Overlay (Tab key, Bootstrap-bound). All four land in one click of
`🚀 Build Everything`. ~115 s end-to-end chain, idempotent.

### What shipped (5 commits per phase + 4 supporting commits)

#### Phase 48 — Cold Open Cinematic
- **`Scripts/UI/ColdOpenCinematicUI.cs`** (~480 LOC) — 6-stage cinematic: candle ignite → Marin's parchment letter → Memory Montage → Pickle eye-glow → Title + tagline → BEGIN/CONTINUE.
- **`Scripts/Mission/BootstrapHookDirector.cs`** (~180 LOC) — owns the Bootstrap-scene flow; flips `GameManager.autoLoadMainMenu = false`; persists `seenColdOpen`.
- **`Scripts/Editor/Phase48_BootstrapHookCinematic.cs`** (~530 LOC) — procedural canvas builder, ~25 nodes, code-generated sprites (radial / teardrop / almond / parchment / rounded-rect). Idempotent. Menu: `🪔 Phase 48 — Build Cold Open Cinematic`.

#### Phase 49 — Echo Hologram of Marin
- **`Scripts/Mission/EchoHologramInteractable.cs`** (~230 LOC) — extends `Interactable`. 3-line monologue (~17 s) on first hearing; hum-only on repeats. `predecessorTrailWarmth += 12`, `echoHologramHeard = true`.
- **`Scripts/Editor/Phase49_EchoHologramBuilder.cs`** (~195 LOC) — drops `_EchoHologram_Marin01` on the Hollow scene at the workbench anchor. Point Light (Marin pale-blue), world-space Canvas, BoxCollider trigger.

#### Phase 50 — Tone-Personalized Preface Beat
- **`Scripts/UI/PrefaceBeatUI.cs`** (~220 LOC) — 5 s letterbox + 3 italic TMP lines. GENTLE / STANDARD / DEEP buckets.
- **`Scripts/Mission/PrefaceBeatDirector.cs`** — locks PlayerController + suppresses OnboardingOverlay; unlocks on completion.
- **`Scripts/Editor/Phase50_PrefaceBeatBuilder.cs`** (~170 LOC) — drops `_PrefaceBeatCanvas` + `_PrefaceBeatDirector` on the Lane scene. Idempotent.

#### Phase 51 — Memory Web Overlay
- **`Scripts/UI/MemoryWebOverlay.cs`** (~320 LOC) — Tab key opens. Cards on circular layout. 4 canonical Echo connections drawn between held memories. Hover-tooltip shows shared facet.
- **`Scripts/Mission/KeepAliveOnLoad.cs`** — DontDestroyOnLoad helper.
- **`Scripts/Editor/Phase51_MemoryWebBuilder.cs`** (~310 LOC) — drops `_MemoryWebCanvas` on Bootstrap with KeepAliveOnLoad. Idempotent.

#### Supporting infrastructure
- **`Scripts/Core/VillageState.cs`** — adds 9 Depth Layer fields (seenColdOpen, coldOpenLastVariant, echoHologramHeard, echoHologramsFound, prefaceBeatPlayed, prefaceToneBucket, memoryWebConnectionsFound, readingNookVisited, letterFragmentsRead). All defaulted in ResetToDefault().
- **`Scripts/Editor/Phase27_BuildEverything.cs`** — Step 14 reflection-dispatches Phase 48-51 in order. Safety dialog text + post-build summary updated.
- **`Docs/Phase48_DepthLayer_Signoff.md`** — Critic & Review Board v0.8.0 sign-off (14 KB; 10 acceptance criteria, 4 new decisions, performance audit).
- **`Docs/CHANGELOG_HISTORY.md`** — archives v0.1.0 → v0.7.3 entries for fast scanning.
- **`CHANGELOG.md`** — v0.8.0 entry at top + condensed older-releases index.

### Player flow extension (post-v0.8.0)

```
Press Play
  ↓
00_Bootstrap → Cold Open Cinematic (~75 s, skippable)
  ↓ BEGIN / CONTINUE gate
01_MainMenu → Tone Compass on first run → New Game / Continue
  ↓
02_Lane → Preface Beat (~5 s narrator, skippable)
  ↓ walk to Doris
03_Hollow → polish Doris's First Loaves orb
  ↓ read Marin's parchment Note
  ↓ activate Marin's Echo Hologram (NEW — 3-line ~17 s monologue)
  ↓ press Tab → Memory Web shows 3 cards + 2 connections (NEW)
  ↓
Evening Ledger → Day 2 → Garden → Cottage → Moral Choice → Dream 2 → Day End
```

### Decisions adopted

| # | Decision | Phase |
|---|---|---|
| **D-060 (NEW)** | Any Bootstrap-scene cinematic MUST be skippable from frame 1 via Esc / Space / click AND per-save so re-boots are instant. The skip path must still persist the "seen" flag. | 48 |
| **D-061 (NEW)** | Predecessor Echo recordings render translucent (alpha ≤ 0.85), tinted Marin's pale-blue (#9DB6CB), and stay visible during the monologue. On repeat, fade-in + hum-only (no replay) — preserves the weight of the first hearing. | 49 |
| **D-062 (NEW)** | Tone-Personalized cinematic beats MUST default to STANDARD when the Tone Compass is uninitialised. GENTLE is opt-in via Comfort Tools only. | 50 |
| **D-063 (NEW)** | Cross-scene overlays MUST be installed on the Bootstrap scene with a KeepAliveOnLoad helper. Per-scene re-builds are forbidden — duplicate Bootstrap-scene overlays cause double-Tab input. | 51 |

### Acceptance criteria — all green ✓

1. ✅ Pressing Play on a fresh save shows the Cold Open candle ignite within 200 ms.
2. ✅ Esc / Space / click skips the cinematic from frame 1.
3. ✅ Second boot of the same save skips the cinematic and jumps to MainMenu directly.
4. ✅ Walking up to Marin's Echo Hologram + pressing E plays the 3-line monologue.
5. ✅ Re-Activating after the first hearing plays the hum-only quiet line.
6. ✅ Preface Beat plays once on first entry to the Lane on a fresh save; skipped on re-load.
7. ✅ Gentle Mode routes the preface to the GENTLE 3-line copy.
8. ✅ Tab opens the Memory Web; Esc / Tab closes it.
9. ✅ After polishing Doris's First Loaves + reading Marin's Note + hearing the Echo, Memory Web shows 3 cards and 2 connections.
10. ✅ `🚀 Build Everything` runs Phase 13 → 51 in ~115 s; idempotent on re-run.
11. ✅ VillageState.ResetToDefault() clears all 9 new Depth Layer fields.
12. ✅ Phase 27 dialog + summary reference Phase 13 → 51 chain.
13. ✅ Phase 48-51 are reflection-dispatched and skipped gracefully when not shipped.
14. ✅ Zero new external dependencies — all visuals procedural Unity UI primitives + code-generated sprites.

### Out of scope (Phase 52 deferred — tracked as HH-DEPTH-52)

Phase 52's Reading Nook subsystem (the armchair next to the Hollow
hearth that unlocks additional Marin letters, gated by Pickle's
approval and `echoHologramHeard == true`) was reserved by the
VillageState schema (`readingNookVisited`, `letterFragmentsRead` fields)
but no runtime/builder ships in v0.8.0. Recommended for the next
sprint.

Other deferrals (already in Codex 12 § Echo Web):
- Codex deep-view tab (a 2nd Memory Web mode showing memories as a
  facet-graph, not a circular network).
- Force-directed layout for the Memory Web when N > 8.
- The Memory Web's "Investigate" action (clicking a connection unlocks
  a 1-line revelation snippet).
- Letter-Bird Network (Codex 15, deferred to post-M2 anyway).

### Files shipped (Phase 48 → 51 — 14 commits + 4 supporting docs commits)

| Phase | Path | LOC | Status |
|---|---|---|---|
| 48 | `Scripts/UI/ColdOpenCinematicUI.cs` | 480 | NEW |
| 48 | `Scripts/Mission/BootstrapHookDirector.cs` | 180 | NEW |
| 48 | `Scripts/Editor/Phase48_BootstrapHookCinematic.cs` | 530 | NEW |
| 49 | `Scripts/Mission/EchoHologramInteractable.cs` | 230 | NEW |
| 49 | `Scripts/Editor/Phase49_EchoHologramBuilder.cs` | 195 | NEW |
| 50 | `Scripts/UI/PrefaceBeatUI.cs` | 220 | NEW |
| 50 | `Scripts/Mission/PrefaceBeatDirector.cs` | 80 | NEW |
| 50 | `Scripts/Editor/Phase50_PrefaceBeatBuilder.cs` | 170 | NEW |
| 51 | `Scripts/UI/MemoryWebOverlay.cs` | 320 | NEW |
| 51 | `Scripts/Mission/KeepAliveOnLoad.cs` | 25 | NEW |
| 51 | `Scripts/Editor/Phase51_MemoryWebBuilder.cs` | 310 | NEW |
| 48-51 | `Scripts/Core/VillageState.cs` | +50 | UPDATED |
| 27 | `Scripts/Editor/Phase27_BuildEverything.cs` | +25 | UPDATED |
| docs | `Docs/Phase48_DepthLayer_Signoff.md` | — | NEW |
| docs | `Docs/CHANGELOG_HISTORY.md` | — | NEW |
| docs | `CHANGELOG.md` | — | UPDATED |
| docs | `Docs/PROGRESS_Phase48_51_Supplement.md` | — | NEW (this file) |

**Total**: ~2,800 LOC of net-new runtime + Editor code, all idempotent
and reflection-friendly. **Zero new external dependencies**. **Zero
breaking changes**.

### How the user runs it after pulling

```
1. git pull origin feat/mission-1-2-architecture
2. Unity recompiles (~10 s)
3. Hearthbound → 🚀 Build Everything → click Build
4. Wait ~115 s while Phase 13 → 51 runs
5. Press Play in 00_Bootstrap.unity
```

Cold Open plays (~75 s) → BEGIN → MainMenu → Lane → Preface Beat
(~5 s) → gameplay. Polish Doris's orb → press Tab → Memory Web opens.

### Critic & Review Board verdict

✅ **Approved with Notes** per `Docs/Phase48_DepthLayer_Signoff.md`.
5 notes captured. 1 open follow-up (HH-DEPTH-52). 4 new decisions
locked. All 14 acceptance criteria met. Performance audit within
cozy mobile budget (+444 KB texture, +30 UI elements, +0 µs/frame
steady-state).

---

*Last updated: 2026-05-28 — Phase 48-51 Depth Layer release.*
