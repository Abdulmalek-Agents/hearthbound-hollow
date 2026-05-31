# PROGRESS — Phase 71: Onboarding depth + Mission-2 never-stranded + green garden

**Branch:** `feat/mission-1-2-architecture` · **Engine:** Unity 6000.4.4f1 · URP
**Date:** 2026-05-30 · **State:** 🟢 done in branch (pull + `🚀 Build Everything`, then Play)

---

## Why this phase

Two streams converged:

1. **Best-in-class onboarding** (open studio request): the first-run walkthrough
   taught movement/interact/polish but **never introduced the cozy daily loop**
   (Request Board, Memory Wall, My Hollow, Garden, Workbench) shipped in
   Phases 62–67 — so a new player met those systems blind. The `H` reference
   card had the same gap.

2. **Player playtest commit** `3a8241fb` — *"green garden not available same
   issue of stuck at mission 2 still"* — filed against a build that already
   included the Phase 62.D garden enrichment. Two concrete defects:
   - **Stuck at Mission 2** — a hard progression block (a Cozy-Contract
     violation: the player must *never* be stranded).
   - **Green garden not available** — the meadow read brown/dead, not green.

QA + the Art/Environment team + unity-engineer + engagement-polish convened.

---

## 1) Onboarding now teaches the daily loop  🟢

**File:** `Scripts/UI/OnboardingOverlay.cs` (+ `Scripts/Core/LocalizationService.cs` was
reverted; see note).

- Added a 5th walkthrough step **"Your little world"** (emoji 🔑, locKey
  `onboard.rhythm`) between *Polish* and *Take it easy*. It names the hub keys at
  a glance — `[J]` journal · `[B]` requests · `[M]` memory wall · `[U]` your hollow ·
  `[G]` garden · `[K]` workbench — using progressive disclosure (tells the player
  these exist; never forces interaction). Marin/cozy register, no anxiety framing.
- `locKey` is forward-compatible: when the AR table entry is absent, `ResolveStepText`
  falls back to the English literal (graceful, by design). **Decision:** we did
  *not* edit `LocalizationService.cs` to add the AR strings this pass — that file
  is a 100+ line Arabic table and a blind whole-file replace risked corrupting the
  existing translations for a one-step gain. The step works in EN now; AR can be
  added in a focused localization pass.

## 2) The `H` reference card lists the loop keys  🟢

**File:** `Scripts/UI/HelpOverlayUI.cs`

- Added a **"Your day's places"** section to `BuildBody()`: `[J]` Journal · `[B]`
  Requests · `[M]` Memory Wall · `[U]` Your Hollow · `[G]` Garden · `[K]` Workbench —
  each with an on-brand gold glyph and **full EN/AR** localization (consistent with
  the rest of the card; the existing `L(en, ar)` + `HollowGlyphs` path is reused).
- The "full reference" is now actually full — a returning player has one place to
  recall every hotkey.

## 3) Mission 2 Garden never strands the player  🟢  (the stuck-at-M2 fix)

**File:** `Scripts/Mission/Mission02Director.cs` — **runtime; applies on pull
without a scene rebuild** because the baked director shares this script.

Diagnosis (read the committed `04_Mission02_Garden.unity` YAML directly): the exit
trigger is mechanically sound — `Garden_Exit_Trigger` at z=12, enabled `isTrigger`
BoxCollider (8×3×1), wired to the director, cottage scene in Build Settings, player
prefab tagged `Player` with a `CharacterController`. So the hand-off *could* work —
but it relied on a **single walk-through trigger box** as the only way forward. A
player who never crossed that exact volume (or whom the trigger missed) was stranded.

Three-layer safety net (gentlest first), all gated to the Garden role and disarmed
once the player leaves:

1. **Redundant, position-based exit** — `Update()` hands off the moment the player
   reaches the exit band (`z ≥ exitZ − 0.85`), covering any missed trigger.
2. **Gentle linger nudge** — after ~40 s wandering, a one-time soft Pickle line
   names the north path ("follow the lanterns north…"). No fail framing.
3. **Never-stranded auto-advance** — after ~120 s, auto-walks the player to Gerrold
   with a kind line, honouring the Cozy Contract no matter what blocks the path.

Also: `GardenIntro` now defensively unlocks the player; `OnPlayerExitedGarden` is
idempotent (trigger / position-poll / rescue can race; only the first transitions)
and logs if `GameManager` is missing instead of failing silently.

## 4) The garden is green again  🟢  (green-garden-not-available fix)

**Files:** new `Scripts/Editor/HHGroundMaterials.cs`; `Scripts/Editor/Phase62_GardenEnrichment.cs`
+ `Scripts/Editor/Phase63_WorldPolish.cs` now delegate to it. **Applies on next
`🚀 Build Everything`.**

Root cause: both enrichment builders did a blind `FindAssets("t:Material grass")`
and used the first hit — which, in the Medieval Village pack, can be
`M_DriedGrass_Fo_01a` (**brown**) — or fell back to a flat olive colour. Either way
the meadow read dead, not green. (The player's commit even shows
`M_DriedGrass_Fo_01a.mat` was touched.)

`HHGroundMaterials.MakeCozyGround(kind)` (single responsibility, shared so the two
grounds can't drift apart):
- Builds a fresh **URP Lit** material backed by the **green grass diffuse**
  `T_Grass_Fo_01a_D` (the albedo `_D` map — explicitly **excludes** the `Dried`
  variant and the `_OR`/`_ORS`/normal/mask maps), with sensible tiling so a big
  plane reads as detailed grass, not a slab.
- Falls back only to a **lush green** colour (`0.27, 0.45, 0.22`) — *never brown*.
- `kind == "lane"/"earth"` → warm soil; everything else → green meadow.

---

## Commits (all on `feat/mission-1-2-architecture`)

| SHA | What |
|---|---|
| `e1bf823f` | onboarding: daily-loop "Your little world" step |
| `44e66fcc` | help: loop hub keys on the `H` reference card |
| `006b5980` | mission2: Garden never strands the player (3-layer safety net) |
| `797df874` | env: lush green garden ground (shared `HHGroundMaterials`) |

All pushed via the authenticated MCP and verified **byte-identical** to the local
working tree (Arabic-bearing files were transmitted as fully `\uXXXX`-escaped JSON
to eliminate transcription risk).

## Cozy Contract check  ✅

No fail states; the rescue *guarantees* the player is never stranded; the nudge is
soft and one-time; no anxiety numbers; refusal/skip still honoured; hand-written
dialogue only (Pickle's nudge lines are authored, not generated).

## How to verify

1. Pull `feat/mission-1-2-architecture`; run **`🚀 Build Everything`**.
2. Play from `00_Bootstrap`. First-run onboarding now has the "Your little world"
   loop card; press `H` anywhere to see "Your day's places".
3. Reach the Garden (Day 2): the ground is **green**; walk north — you transition to
   the cottage. If you dawdle, Pickle points the way (~40 s) and, worst case, you're
   auto-walked there (~120 s). You cannot get stuck.

## Follow-ups (not blocking)

- Add `onboard.rhythm.*` AR strings in a focused localization pass.
- Optional: a one-frame screen-space waypoint marker over `Garden_Exit_Trigger` for
  even faster wayfinding (the lantern path + nudge already cover it).
- Depth-verification audit (M1/M2 vs Engagement + Depth bibles) — queued next.
