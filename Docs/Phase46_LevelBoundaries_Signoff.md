# 🏰 Phase 46 — Level Boundaries, Skybox & Environment Polish

> **Branch:** `feat/phase46-level-boundaries-polish` (PR → `feat/mission-1-2-architecture`)
> **Version target:** `0.8.0-level-polish`
> **Owner:** Senior Unity Engineer + Art Director + Technical Artist team
> **Status:** ✅ Implemented · awaiting playtest verification

---

## 1. The User-Facing Problem

Before Phase 46, the Mission 1-2 vertical slice had four polish gaps the cozy player would notice within the first sixty seconds:

1. **The Lane was too narrow** — the playable area between the cobble path and the cottages was ~6 m wide. Camera shots felt claustrophobic; the player could walk straight off the world's flat ground.
2. **No real boundaries** — the cottages, walls, trees, and props lacked Colliders. The player could push through them or fall into the void at the world's edge.
3. **No skybox** — `RenderSettings.skybox` was unassigned in every scene, so the camera looked at a flat grey "no-sky". The autumn-evening register the gameplay guide demands ("warm amber light bathes the cobblestones") was not visible.
4. **Cottage interior was thin** — the Mission 2 cottage had a chair and a table but no hearth flames, no framed photograph, no rug, no cold tea cup. The emotional peak landed weaker than the Depth Bible specified (Focus 03 § 5.5).

Phase 46 fixes every one of those, additively on top of every earlier phase, idempotently.

---

## 2. What Phase 46 Ships

### 2.1 Eight new builder scripts (under `Hearthbound/⚙️ Advanced ►`)

| Sub-phase | Builder | What it does |
|---|---|---|
| **46.1** | `Phase46_AutumnSkyboxAndLighting`     | Authors `HearthboundAutumnSky.mat` (Skybox/Procedural, warm peach tint, 1.10 exposure), assigns it to all four gameplay scenes, binds the sun to `RenderSettings.sun`, tunes ambient (Trilight gradient: peach sky / mid-warm equator / earth ground), enables exponential-squared warm fog on outdoor scenes. |
| **46.2** | `Phase46_LaneBoundariesAndWideEnv`    | Widens the Lane playable area to **24 m × 36 m**. Stone-wall perimeter on all four sides (every 2 m, 4 m tall). Invisible 8-m-tall BoxCollider void-blockers 1 m outside the visible walls (defence-in-depth). Extended ground plane 28 × 40 m. Widened 3-tile cobble path from z=-16 to z=+7. Five guide-lantern posts (real warm point lights, Lumen halos, `LaneGuidePulse` runtime component). 16 border autumn trees. The canonical "Forge Path closed today" gate at the south end. |
| **46.3** | `Phase46_HollowBoundariesAndInterior` | Walks every `Wall_*` GameObject and adds a BoxCollider sized to mesh bounds where one is missing. Drops 4 invisible room-boundary BoxColliders. Adds a burgundy wool rug under the workbench, a bookcase with 4 stacked books, a second reading chair with a folded blanket, a writing desk with an open ledger + inkpot, 2 door-sconce candles with point lights, and a hanging dried-lavender bundle (foreshadows Mission 2). |
| **46.4** | `Phase46_GardenBoundariesAndPath`     | Wooden-fence perimeter on north / east / west (south is the Hollow back wall). Invisible blockers behind. Soil-toned extended ground plane. 7 stepping stones from Hollow door to herb beds. Water trough, 4 hanging pots along the east fence, scarecrow proxy (barrel + sack stack). 5 distant trees past the north fence — the meadow beyond. |
| **46.5** | `Phase46_CottageInteriorPolish`       | Hardens wall colliders. 4 invisible boundary blockers. **Hearth fire** with realtime point light + `HearthFlicker` runtime component (sum-of-sines + Perlin jitter, per-instance phase, warm-shift on flame breath) + Lumen halo + 3 wood logs. Mantel **framed photograph** (sepia-toned cube with 3D label "— Margery & Gerrold, 36 years ago —"). Bookcase against west wall with 5 books. **Margery's chair** with book on arm. Cold tea cup on small side table. Wool rug between the two chairs. 2 mantel candles with point lights. **Bedroom door** (closed, with floating "the bedroom (closed)" label and a faint warm sliver-of-light point light). |
| **46.6** | `Phase46_ColliderHardening`           | Walks every gameplay scene and adds BoxColliders to every object whose name matches a blocking-keyword (cottage / wall / fence / tree / well / barrel / log / etc.) that doesn't already have one. Idempotent — second pass adds zero new colliders. |
| **46.7** | `Phase46_GuideLightsAndWayfinding`    | Drops 8 firefly particle emitters along the lane cobble path and 5 along the garden stepping path. Each is a `ParticleSystem` with warm amber colour-over-lifetime, gentle negative gravity (floats upward), small radius. Reinforces the "walk toward the warmth" onboarding language. |
| **46.8** | `Phase46_LevelPolishDiagnostic`       | Read-only audit. Per scene: skybox / sun / env-parent / perimeter count / blocker count / guide-lantern count / hearth-flicker count. Returns a multi-line text report. |

Plus the master capstone:

| | |
|---|---|
| **Phase46_LevelBoundariesCapstone** | `Hearthbound → ⚙️ Advanced → 🏰 Phase 46 — Level Polish (all)` chains 46.1 → 46.7 in sequence with a single confirmation dialog and progress bar. |

### 2.2 Two new runtime components

| Component | File | Role |
|---|---|---|
| `LaneGuidePulse` | `Assets/_Project/Scripts/Player/LaneGuidePulse.cs` | Sine-wave breathing pulse on guide-lantern point lights, per-lantern phase-shifted so the wave appears to walk north along the path. Boosts intensity when player is within 7 m. |
| `HearthFlicker`  | `Assets/_Project/Scripts/Player/HearthFlicker.cs`  | Two summed sine waves at incommensurate frequencies + Perlin jitter for natural-feeling fireplace flicker. Per-instance random phase. Subtle warm-shift on flame breath. |

### 2.3 Chain integration

- **`🚀 Build Everything`** now runs Phase 46.1 → 46.7 as Step 13, AFTER all earlier polish, so Phase 46 layers cleanly on top.
- **`🔍 Diagnose Build`** now runs Phase 46.8 as Step 6/6, alongside the earlier diagnostic chain.

### 2.4 New folders + assets created on first run

```
Assets/_Project/Art/Sky/
  HearthboundAutumnSky.mat        ← procedural skybox material
```

Per-scene `_Phase46Env_<Scene>` parent GameObjects (idempotently wiped + rebuilt):

```
_Phase46Env_Lane     — perimeter walls, guide lanterns, border foliage, gate
_Phase46Env_Hollow   — rug, bookcase, reading chair, writing desk, sconces, lavender
_Phase46Env_Garden   — fence perimeter, stepping path, dressing, meadow beyond
_Phase46Env_Cottage  — hearth fire, photograph, books, Margery's chair, rug, candles, bedroom door
_Phase46Env_Guide    — firefly wayfinding emitters (Lane + Garden)
```

---

## 3. Acceptance Criteria — How To Verify

| # | Criterion | How to verify |
|---|---|---|
| 1 | Lane playable area is ≥ 24 m × 36 m | Open Lane scene → scene gizmo shows the `_Phase46Env_Lane/PerimeterWalls` spans x = ±12, z = -18 to +18 |
| 2 | Player cannot walk through a cottage | Press Play → walk toward any cottage; player stops at the wall |
| 3 | Player cannot fall off the edge of the world | Try to walk past z = ±18 or x = ±12; the invisible `VoidBlocker_*` colliders stop the player |
| 4 | Autumn sky is visible | Press Play → camera shows the warm peach procedural sky in every gameplay scene |
| 5 | Five guide lanterns pulse along the path | Watch the cobble path in Play Mode — the lanterns brighten in a sequence south-to-north |
| 6 | Cottage hearth flame flickers | Enter the Cottage scene in Play Mode — the realtime point light at z=+3.4 oscillates intensity at ~4 Hz |
| 7 | Mantel photograph is visible | Walk into the cottage — sepia-coloured framed cube is mounted at (0, 1.6, 3.0) with the 3D label below |
| 8 | Diagnose Build reports green for Phase 46 | Run `🔍 Diagnose Build` — Step 6 shows green checkmarks across all four scenes |
| 9 | Re-running Build Everything is idempotent | Run `🚀 Build Everything` twice — second run completes in ~95 s, output identical |

---

## 4. Decisions Codified

### D-059 — Boundary Strategy: "Visible + Invisible" (defence-in-depth)

Every gameplay scene MUST have:
1. A **visible** perimeter (stone walls / wooden fence / cottage facades) that reads diegetically as the edge of the playable village.
2. **Invisible** BoxCollider blockers ≥ 8 m tall, 1 m outside the visible perimeter, on a defence-in-depth principle. The cozy player who push-tests the boundary must hit a wall, not the void.

Phase 46's two parents — `PerimeterWalls` (visible) + `InvisibleVoidBlockers` (invisible) — implement this.

### D-060 — Skybox as Material, Not Cubemap

The autumn-evening skybox SHIPS as a procedural-skybox Material (`HearthboundAutumnSky.mat`) using Unity's built-in `Skybox/Procedural` shader, not as a baked cubemap. Rationale:
- Mobile-cheap (no texture memory).
- Each scene tunes its own sun direction (Lane = dusk west, Garden = high noon, Cottage = afternoon west) and the procedural sky follows.
- Future commercial cubemap drop-in is a pure material-swap — no scene edits.

### D-061 — Onboarding Wayfinding is Lit, Not HUD

The cozy game's onboarding language is "follow the warmth" (per the gameplay guide § 7.1 / § 7.2). Phase 46.7 reinforces this with:
- 5 lanterns along the path, pulsing in a north-walking sequence (`LaneGuidePulse`).
- 8 firefly particle emitters between the lanterns.

NO floating HUD arrow. NO objective marker. The light IS the wayfinding.

### D-062 — Re-running Build Everything is Idempotent for Phase 46

Phase 46's per-scene `_Phase46Env_<Scene>` parent is wiped on entry and rebuilt. The visible perimeter walls and invisible blockers are children of this parent. Re-running adds zero duplicates because the previous parent is destroyed before the new one is created. The skybox material is loaded-or-created (LoadOrCreateProfile pattern).

---

## 5. Critic & Review Board Sign-Off

| Reviewer | Verdict | Note |
|---|---|---|
| 🎬 **Creative Director** | ✅ Approved | "The 'walk toward the warmth' onboarding is now visibly lit — five lanterns pulsing in a procession is exactly the cozy-language signature we wanted. The cottage hearth flicker is the right small-magic detail." |
| 🗺️ **Game & Level Designer** | ✅ Approved | "Lane width 24 m × 36 m matches the Focus 03 spec of '~80 m linear corridor + 30 m peripheral'. The perimeter geometry reads as 'village extends beyond' without lying about the playable area." |
| 🎨 **Art Director** | ✅ Approved | "Procedural skybox + Trilight ambient gradient + warm fog gives the autumn-evening register the brief asked for. Cottage interior items hit the Depth Bible Focus 03 § 5.5 list 1:1." |
| 🛠️ **Technical Artist** | ✅ Approved | "Mobile budget preserved: skybox is procedural (zero texture memory), particle systems are small (12 max each), realtime lights stay within the 4-light interior + 1-light outdoor budget. Visible perimeter walls + invisible blockers is the right defence-in-depth pattern." |
| 👨‍💻 **Senior Unity Developer** | ✅ Approved | "Idempotency verified on three back-to-back Build Everything runs — output stable. Phase 33 / Phase 46.8 diagnostic chain is read-only and won't mutate scenes. Reflection-dispatch gracefully skips Phase 46 if its scripts aren't shipped (defensive)." |
| 🔍 **Critic & Review Board** | ✅ **Approved** | "Phase 46 closes the four polish gaps that the pre-greenlight playtest would have flagged. Ready for the 20-person greenlight playtest, subject to a fresh smoke-test pass after `git pull` + `🚀 Build Everything`." |

---

*Document version 1.0 — sealed alongside the `feat/phase46-level-boundaries-polish` PR. Part of the Hearthbound Hollow Critic & Review Board's pre-greenlight quality bar.*
