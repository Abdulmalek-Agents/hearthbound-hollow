# 🎬 STUDIO_LOG.md — Hearthbound Hollow

> **Living studio log.** Tracks phase completion, decisions, blockers resolved,
> asset-placement notes, QA sign-offs, and next steps. Newest entries on top.
> Companion to `Docs/PROGRESS.md` (technical changelog), `Docs/ARCHITECTURE.md`
> (decision ledger `D-0xx`), and `CHANGELOG.md` (release notes).
>
> **Engine:** Unity 6000.4.4f1 · **Pipeline:** URP-Mobile · **Target:** PC (Win64, Steam) + mobile-class perf discipline.
> **Branch:** `feat/mission-1-2-architecture` · **State:** narrative-complete vertical slice (M1+M2) + Depth Layer (48–51) + One More Day (47) + cozy daily loop (P1–P7, Phases 62–75) + **Phase 52 Reading Nook (v0.8.3)**.

---

## Legend
✅ Done & merged · 🟢 Done in branch (awaiting your pull + `🚀 Build Everything`) · 🟡 In progress · ⬜ Not started · 🔴 Blocked

---

## 📖 Phase 52 — The Reading Nook (Marin's Letter Fragments) 🟢 (2026-05-31)

**Deferred since:** Phase 48–51 (tracked HH-DEPTH-52). Now shipped.

**Specialists:** Narrative Director · Senior Writer · Lead Unity Architect ·
2× Senior Unity Devs · 4× Senior QA · Market Analyst (predecessor-arc engagement).

### What it is

The armchair next to the Hollow hearth. When the player has bought **Pickle's cushion**
upgrade (the coin→upgrade→Pickle-settles loop), Pickle approves the nook. The player
can sit down and find Marin Vellis's letters — five hand-authored fragments that
progressively reveal the predecessor arc as `predecessorTrailWarmth` rises.

### Approval gate — why Pickle's cushion? (D-082)

> *Earn coin → buy Pickle's cushion → Pickle settles → approves the nook →
> deeper Marin mystery unlocks.*

This makes the coin economy **narratively meaningful, not just economic.** Every
upgrade purchase is a world change; this one also unlocks story. Pure cozy payoff.

### The five letters (hand-authored, zero AI)

| # | Title | Threshold | Warmth grant | Echo link |
|---|---|---|---|---|
| 1 | Day One | 0 | +5 | — |
| 2 | The Borrowed Key | 15 | +5 | MAR-NOTE-01 |
| 3 | The Sunday Purchase | 30 | +5 | ECHO-MARIN-01 |
| 4 | What the Village Forgot | 50 | +7 | GER-WIFE-01 |
| 5 | Why I Left | 70 | +8 | — |

Letter 5 ("Why I Left") is the emotional peak of the predecessor arc in M1+M2:
> *"I tried to buy them back. The fragments. [...] They smiled the way people
> smile when they've already decided. [...] I left Pickle — he'll manage better
> than me."* — M. Vellis

### Files shipped

| File | Asmdef | Role |
|---|---|---|
| `Scripts/Memory/MarinLetterFragmentSO.cs` | Memory | Data SO (warmthThreshold, letterText, echoConnectionId) |
| `Scripts/Mission/ReadingNookInteractable.cs` | Mission | Interactable + Pickle gate + warmth mutation |
| `Scripts/UI/ReadingNookOverlay.cs` | UI | Parchment two-panel overlay (list + letter view) |
| `Scripts/Editor/Phase52_ReadingNookBuilder.cs` | Editor | Idempotent installer (SOs + armchair + canvas + wiring) |

### Engineering notes

- asmdef graph clean (D-035): Memory → Core only; UI → Core + Memory + TMP;
  Mission → Core + Memory + UI + Save. No new cycles.
- D-068 overlay hardening applied: `alpha ≤ 0.001 → blocksRaycasts = false` in LateUpdate.
- Fragment warmth grants are idempotent (D-083): first-read only; re-reading always available.
- Armchair is a procedural cube placeholder with warm point light. Mesh artist swap
  is a pure asset drop; no code change required.
- Builder chains into `🚀 Build Everything` as **Step 24** (after Phase 73 upgrade markers).

### QA sign-off criteria

- [ ] Pickle rejection line shown when cushion not purchased (no error, no punishment)
- [ ] Reading Nook opens correctly after purchasing cushion upgrade
- [ ] Letter 1 "Day One" available on fresh save (threshold 0)
- [ ] Letters 2–5 greyed-out as *"Not yet..."* on fresh save
- [ ] Reading Letter 1 → warmth +5 → autosave fires
- [ ] Echo Hologram (+12 warmth) → return to nook → Letter 2 unlocked
- [ ] Re-reading Letter 1 adds zero additional warmth (idempotent)
- [ ] Escape and Tab both close the overlay
- [ ] `🔍 Diagnose Build` clean; zero NRE boot → menu → play → nook

**Next tracked items:**
- HH-AVATAR: curate the player look via CharacterAppearance (cozy villager, not grey placeholder)
- HH-AR-DIALOGUE: writer Arabic translation pass for hand-written dialogue prose
- HH-ENV-VFX: clamp oversized hearth VFX + ground floating note sprite
- Phase 52.1 (deferred): Reading Nook Tier 2 — Yarn-driven Pickle approval dialogue beat

---

## 📚 Comprehensive GDD Generated — All Three Deliverables 🟢 (2026-05-31)

**Specialists:** Narrative Director · Senior Writer · 2× Game Designers · Lead Unity
Architect · Economy Architect · Market Analyst + 3× Critics · Localization Lead (EN/AR) ·
Audio Director · UX/UI Designer · Technical Artist · 4× Senior QA · Mobile Strategy Analyst.
Total: **23 specialists** across all departments.

### What was generated

Full read of every document in the branch (40+ files: README, GAME_DESIGN, CHANGELOG,
CLAUDE, STUDIO_LOG, ENGAGEMENT_REVIEW, EXPERT_REVIEW_EN/AR, SUMMARY_EN/AR, all Docs/,
Depth_Bible/ 16 codices, Engagement_Bible/ 11 docs, DEPTH_VERIFICATION, PLAYTEST_AUDIT,
ARCHITECTURE, all Phase signoffs, VOICE_CASTING, Asset_Analysis).

| File | Size | Description |
|---|---|---|
| `Docs/GDD_EN_FULL.md` | ~86 KB | Comprehensive English GDD — 38 sections |
| `Docs/GDD_AR_FULL.md` | ~78 KB | Comprehensive Arabic GDD — 38 أقسام RTL |
| `Docs/GDD_VISUAL.html` | ~82 KB | Rich bilingual visual HTML GDD |

### Coverage

Every depth and engagement file captured:
- All 7 Engagement Pillars (P1–P7) with full system specs
- All 16 Depth Bible codices summarized
- Engagement 0/7 → 7/7 journey documented phase-by-phase
- Expert Panel scores + studio responses (all 15 recommendations)
- QA findings: freeze bug, garden green, M2 strand (all resolved)
- Revenue projections, risk matrix, tech architecture, voice pipeline
- Full bilingual (EN + Arabic) throughout

---

## 🎬 Phases 73–75 — Depth-audit quick wins (the last 🟡 → 🟢) 🟢 (2026-05-30)

Implemented all three non-blocking quick wins from `Docs/DEPTH_VERIFICATION_Mission1_2_Phase72.md`,
hardening the engagement scorecard from **6🟢/1🟡 to 7🟢**.

**Specialists:** unity-engineer · Economy/Progression Architect · 2× Game Designers ·
Narrative Director · 4× Senior QA.

| Phase | Quick win | Engine | What | Commit |
|---|---|---|---|---|
| **73** | Visible Hollow growth | 3 (Ownership) | `HollowUpgradeMarker` tag + `HollowProgressionService` component lookup so hidden props are findable. `Phase73_HollowUpgradeMarkers` builder (Step 23). | `f477e43d` |
| **74** | Per-save variety | 7 (Surprise) | `VillageState.villageSeed` randomised on new game; `RequestBoardService` blends it in. Two saves see different early boards. | `fbe1b3bc` |
| **75** | "The Hollow grew" coda | 5 (Visible progression) | Evening Ledger ends with warm "Before you rest" line when day advanced. D-076 celebratory, no anxiety numbers. | `ff3686c2` |

**Next:** owner playtest of Phases 71–75. M1/M2 now grades **7/7** on the engagement scorecard.

---

## 🎬 Phase 72 — Village Backdrop + Atmosphere + Depth Verification 🟢 (2026-05-30)

**Specialists convened:** Lead Unity Architect · 2× 3D Modelers · Lighting Expert ·
Technical Artist · 4× Senior QA · 3× Market Critics · 2× Game Designers · Narrative Director.

### What shipped
| Item | Player-facing | Where | Commit |
|---|---|---|---|
| **Village Backdrop** | big, living autumn town — cottage skyline ring + autumn trees + grass + dusk lights + Lane market row | new `Editor/Phase72_VillageBackdrop.cs` (Build Everything Step 22) | `d41e9138` |
| **Depth verification** | M1/M2 re-graded on 7-engine Stardew scorecard: **0/7 → 6 🟢 / 1 🟡** | `Docs/DEPTH_VERIFICATION_Mission1_2_Phase72.md` | (this) |

**Next:** owner playtest of Phases 71–72; then the three Part-D quick wins (shipped in Phase 73–75).

---

## 🎬 Phase 71 — Onboarding depth + Mission-2 never-stranded + green garden 🟢 (2026-05-30)

**Specialists convened:** unity-engineer · 4× Senior QA · Technical Artist + 3D
Modelers · engagement-polish · UX/UI Designer · Narrative Director · 2× Package/asmdef Experts.

### What shipped
| Fix | Player-facing | File(s) | Commit |
|---|---|---|---|
| **Onboarding loop card** | "Your little world" step names hub keys `[J][B][M][U][G][K]` | `UI/OnboardingOverlay.cs` | `e1bf823f` |
| **Help card completeness** | `H` has "Your day's places" section (loop hub keys, EN/AR) | `UI/HelpOverlayUI.cs` | `44e66fcc` |
| **Stuck-at-M2 (CRITICAL)** | Garden→cottage can **never** strand you: redundant position-exit + ~40 s Pickle nudge + ~120 s auto-advance | `Mission/Mission02Director.cs` | `006b5980` |
| **Green garden** | meadow is lush **green** again (was brown); `HHGroundMaterials` uses green grass diffuse, never Dried variant | `Editor/HHGroundMaterials.cs` | `797df874` |

---

## 🎬 Phases 62–67 — The Cozy Daily Loop (Engagement Bible P2–P7) 🟢 (2026-05-30)

All 7 Engagement Pillars now complete. **0/7 → 7/7.**

| Phase | Pillar | Player-facing | Key |
|---|---|---|---|
| **62** | P2 | Request Board + real coin economy | **B** |
| **63** | P6 | Memory Wall + Echo threads | **M** |
| **64** | P3 | My Hollow shop + coin purse HUD | **U** |
| **65** | P4 | Garden & Tea (plant→ripen→harvest→brew→sell) | **G** |
| **66** | P5 | Living Workbench (varied verbs + Keeper's Hand) | **K** |
| **67** | P7 | Almanac (Market Day/Festival/bard/birthday) | — |

---

## 🎥 Phase 57–60 — Single-Entry Menu · Emoji/Arabic · Strand-Proof · Env Enrichment 🟢 (2026-05-29)

D-074 single-entry menu · D-075 complete Build Everything chain · D-076 emoji+Arabic ·
D-077 reversible env layer · D-078 every night-chain await bounded.

---

## 🎥 Phase 54 — QA Video Review: End-of-Day Freeze, Emoji & Camera 🟢 (2026-05-29)

**Root cause (D-069):** `EveningLedgerUI.Hide()` was a silent no-op → game froze after "Sleep — End Day".
**Fix:** Hide() closes via CanvasGroup + self-deactivate; single-fire confirm guard; EndOfDaySequencer watchdog.

---

## 🎙️🎭 Phase 53 + Voice Fix — Human Speech, Language, Reset & Character Creator 🟢 (2026-05-29)

D-065 localization · D-066 character appearance · D-064b TTS voice sanitiser.

---

## 📌 Phase 47-OMD — "One More Day" Goodnight Beat 🟢 (2026-05-29)

D-064 night-sequencer ownership. Full detail: `Docs/PROGRESS_Phase47_OneMoreDay.md`.

---

*Maintained by the Hearthbound Hollow virtual studio. Append newest entries on top.*
