# 📹 PROGRESS Supplement — Playtest Video Fixes (Camera · Character · Garden · Wayfinding)

> Branch: `feat/mission-1-2-architecture`. Companion to `Docs/PROGRESS.md`.
> Source: the 8m17s playtest video `Docs/mission 1 and 2 game issues…fixes.mp4`
> (Git-LFS, 1920×1080@60). The studio fetched it via the LFS API and analysed it
> frame-by-frame (contact sheets every 6 s + full-res key frames).

---

## 1. What the video showed (expert frame analysis)

| Time | Observation | Verdict |
|---|---|---|
| 0:00–0:24 | Cold open (Marin's letter) → title → Tone Compass | ✅ working, readable |
| **0:30** | **Player is a bald, light-grey placeholder** (no hair, plain underclothes) | 🐞 character |
| **0:42, 2:12** | During Doris's dialogue the **camera dives INTO the player** — a giant leg/shoulder fills the screen | 🐞 **camera clip (the headline bug)** |
| **2:36–2:48** | The **Polish tutorial** ("draw slow circles around the orb") is **unreadable because the clipped camera hides the orb** behind the giant leg | 🐞 a *symptom* of the camera clip |
| 3:06 | Evening Ledger "Day 0" + Save + coin (46c) | ✅ |
| 3:24→8:12 | Mission 2 **Garden = a big flat neon-GREEN plane** with a few bare trellises; player **wanders it unable to reach Gerrold's cottage** | 🐞 **environment + "stuck at M2"** |
| 3:24–7:54 | Request Board [B], Hollow Shop [U], Garden screens, Workbench [K], Journal [J], Memory Wall [M] | ✅ **the Engagement loop works** |
| 6:54 | A brief camera-into-shoulder while walking the open garden | 🐞 follow-cam edge case |
| 3:12, 5:00 | The Tab "Memory Web" overlay renders **dark text on dark** (illegible) | 🐞 legacy overlay (superseded by [M]) |

**Root-cause finding:** the "camera clips into the player" and the "messy Polish tutorial" are
**the same bug** — the dialogue camera was placing itself inside the body, which also obscured
the craft tutorial that plays right after a dialogue burst. Fixing the camera fixes both.

---

## 2. Fixes shipped this pass (each pushed to the branch)

### 62.C — Camera no longer dives into the player  🟢  (the headline fix)
`Player/DialogueCameraDirector.cs`. **Root cause:** the over-the-shoulder shot sphere-cast from
the *speaker* toward the camera position *behind the player* passed straight through the
**player's own collider**, so the clip slid the camera onto the body (the giant leg/shoulder).
The wide/narrator shot start-overlapped the player the same way. **Fix:** both clip casts now use
`SphereCastNonAlloc` and **ignore every collider on the player AND the current speaker** (+
zero-distance start-overlaps) — only real walls stop the camera (mirrors `SmoothFollowCamera`'s
Phase 32.21 approach). Widened standoffs (behind 2.5 m, height 1.8 m, clearance 1.6 m), `Awake`
enforces minimums (so a scene-baked director with old tight values is corrected), and a clearance
violation now lifts the camera into a gentle 2-shot instead of jamming beside the body. **This also
makes the Polish/Cleanse tutorial visible** (the orb is no longer hidden behind the body).

### 62.C — Protagonist is no longer a bald placeholder  🟢
`Mission/CharacterAppearance.cs`. The applier was likely **not running** (it relied on a builder
placing it). Now it **self-installs** (DontDestroyOnLoad) and re-applies on every gameplay scene,
and it **always builds cozy all-procedural warm-brown hair** (crown + nape) on the head so the
avatar is never bald — parented to the player root at the head-bone height to avoid rig-scale
distortion. (All-procedural, no new art — D-066.) The BoZo modular pack's real hair/outfit meshes
are a richer future option (noted below).

### 62.D — Mission 2 Garden: real meadow, warm light, medieval dressing + the un-stuck path  🟢
`Editor/Phase62_GardenEnrichment.cs` (idempotent, reversible, chained into 🚀 Build Everything as
Step 20). For the Garden scene it: (1) **retextures** the flat neon-green ground to a warm autumn
meadow (a pack grass/soil texture if present, else a muted olive material); (2) **warms the
lighting** (low gold sun + trilight ambient); (3) **dresses** the meadow with Medieval Village
pine-tree perimeter + market/farm prop clusters (table, barrels, crates, sacks, baskets, pots, a
well, a bench — grounded + collider'd under a deletable root); (4) lays a **glowing lantern path
from the player's spawn to `Garden_Exit_Trigger`** + a beacon at the exit, so the route to
Gerrold's cottage is unmistakable — **the fix for "stuck at Mission 2, don't know how to progress."**

### Build chain  🟢
`Editor/Phase27_BuildEverything.cs` — Phase 62 chained as Step 20 (runs after Phase 60).

---

## 3. Still tracked (precisely diagnosed; not blind-patched)

- **Onboarding step-card polish.** The biggest "tutorial messy" symptom (the Polish orb hidden by
  the clipped camera) is fixed by 62.C. The first-run step-through card itself is **scene-baked by
  the Phase 30 builder** (serialized label refs), so a copy/contrast pass belongs in that builder;
  doing it blind risks regressions. Planned edit: tighten copy to the control essentials + the loop
  key-map ([B]/[M]/[U]/[G]/[K]/[J], already added to the morning Agenda footer), raise contrast.
- **Legacy Tab "Memory Web" overlay** renders dark-on-dark. It is **superseded** by the clean,
  readable `[M]` Memory Wall (Phase 63). Plan: route Tab → MemoryWallUI (or retire the Phase-51
  overlay).
- **Console showed ~26–30 logged errors** during the session. Most are expected boot warnings
  (missing audio cue ids, etc.), but please capture the Console text so any genuine NRE can be
  fixed precisely.
- **Richer character (optional):** swap to a BoZo modular body with real hair/outfit meshes
  (`HairFront_*`, outfits) via an Editor builder — bigger art win than procedural hair, but needs
  the BoZo modular API + in-Editor validation.

---

## 4. How to verify (after pull + 🚀 Build Everything + Play)

1. Talk to Doris → the camera holds a clean over-the-shoulder 2-shot — **no giant leg/shoulder**.
2. The Polish mini-game shows the orb + ring clearly (camera no longer inside the body).
3. The protagonist has **brown hair** (not a bald dummy).
4. Enter the Garden → warm meadow ground + trees + market props + **a lit lantern path leading to
   the cottage exit**; follow it to progress (no more wandering).
5. `🔍`-clean boot → menu → Day 1 → Day 2; the loop screens (B/M/U/G/K/J) still work.
