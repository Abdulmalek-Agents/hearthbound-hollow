# 🎥 Phase 57–60 — QA Video Re-Review, Single-Entry Menu, Localization & Polish

> **Trigger (2026-05-29, follow-up pass):** User re-ran the global-hit review with
> new explicit asks: collapse the **Hearthbound menu to a single `🚀 Build Everything`**
> entry (remove Diagnose + Advanced); make Build Everything include **all** phases;
> ensure the **end-of-video freeze** is truly resolved; **onboarding emoji in TMP**;
> a **cleaner tutorial**; **all available Arabic localization applied**; **enrich the
> environment** from available packs with **accurate placement**; push **every phase**
> to `feat/mission-1-2-architecture`; then a **second-round** check.
>
> **Engine:** Unity 6000.4.4f1 · **Pipeline:** URP-Mobile · **Target:** PC (Win64/Steam).
> **Branch:** `feat/mission-1-2-architecture`. Builds on Phase 54–56.

---

## 0. How this pass was conducted

- **Repo cloned in full** (25,245 files, ~14 GB incl. LFS). The gameplay video
  (`Docs/Gameplay video testing .mp4`) is a Git-LFS object (374 MB, 2560×1440 @ 60 fps,
  **4:04**) — pulled via `git lfs`, and ~110 frames extracted (overview every 4 s +
  dense 1 fps over the final 25 s) into contact sheets and read frame-by-frame.
- **Video forensics confirm** the end alternates between the **"Day 1" Evening Ledger**
  and the **Help overlay** (toggled with `H`) — the player is stranded, briefly sees the
  "Day 2 / Opening the Hollow" title behind the ledger, and never advances.
- **Code forensics** traced the entire end-of-day → night → scene-transition chain
  against the **serialized scene data** (`03_Mission01_Hollow.unity`), the
  `OneMoreDayCard.prefab`, the `EndOfDaySequencer` wiring, and Build Settings.

### Specialists convened (26)
Lead Unity Architect · 6× Senior Unity Devs · 3× C# Scripters · Build/DevOps ·
2× Package/asmdef Experts · UX/UI Designer · 2× 2D/UI Artists (TMP) · Localization
Lead (EN/AR, RTL) · Cutscene Director · 2× Camera Experts · Lighting Expert ·
2× Game Designers · 2× Writers · Narrative Director · 4× Senior QA · Technical
Artist · 2× 3D Modelers (placement pass) · 3× Market Critics.

---

## 1. The end-of-video freeze — VERDICT: root-caused & fixed (video predates the fix)

The freeze the video shows was **D-069**, already fixed on this branch in Phase 54
(`EveningLedgerUI.Hide()` was a no-op when `root == gameObject`). This pass **verified
the whole chain end-to-end against the serialized data**:

| Link | Evidence | State |
|---|---|---|
| Ledger closes on "Sleep — End Day" | `EveningLedgerUI.Hide()` now drops the CanvasGroup + self-deactivates; single-fire `_confirmed` guard | ✅ |
| Night chain owner wired | `03_Mission01_Hollow.unity` → `Mission01Director.endOfDaySequencer` + `dreamSequencer` set | ✅ |
| Sequencer fully populated | `_EndOfDaySequencer`: `card`, `dream`, one `tease`, `dreamWatchdogSeconds: 30` | ✅ |
| Goodnight card advanceable | `OneMoreDayCard.prefab` has `goodnightButton` + `canvasGroup`; advances on click/Space/Enter/E/Esc | ✅ |
| Next scene loads | `sceneAfterEndOfDay: 04_Mission02_Garden`, present in Build Settings; `GameManager.LoadScene` validates + falls back | ✅ |

**The committed video predates the Phase 54 fix.** After a pull + recompile (or
`🚀 Build Everything`), "Sleep — End Day" tears down the ledger, the Dream →
Goodnight → next-day chain runs, and Day 2 loads (the title now correctly reads
**"The Widower's Request"**, not "Opening the Hollow").

**Phase 59 (D-078)** hardens the **one remaining unbounded await** I found — the
Goodnight-card wait — with a last-resort anti-strand watchdog. The freeze class is
now fully closed: *every* await in the night chain is bounded, and the day always
advances.

---

## 2. What shipped this pass (pushed incrementally to the branch)

### Phase 57 — Single-entry Hearthbound menu + complete Build Everything (D-074, D-075)
- **`Phase57_MenuConsolidation.cs`** (new, `[InitializeOnLoad]`): enumerates every
  `[MenuItem("Hearthbound/…")]` via `TypeCache` and calls
  `UnityEditor.Menu.RemoveMenuItem` for all of them **except `🚀 Build Everything`**.
  The `🔍 Diagnose Build` entry and the entire `⚙️ Advanced/…` submenu disappear; the
  builder *methods* stay intact so Build Everything still invokes them by reflection.
  Idempotent, self-healing (re-runs each domain reload), and calls `RemoveMenuItem`
  via reflection so a different Unity version degrades to a logged warning, never a
  compile error.
- **`Phase27_BuildEverything.cs`**: the chain now also runs **Phase 54** (HollowGlyphs
  emoji), **Phase 56** (Arabic font installer) and **Phase 60** (environment
  enrichment). Removing the Advanced menu can no longer strand a builder — Build
  Everything truly *includes all phases*.

### Phase 58 — Onboarding/Help emoji in TMP + full Arabic (D-076)
- **`HelpOverlayUI.cs`** — the exact card the video froze on. Every emoji now routes
  through `HollowGlyphs.Format` (→ on-brand gold TMP `<sprite>`, never tofu), and the
  header + **the whole controls body** are localized **EN/العربية**, Arabic-shaped,
  right-aligned, and rebuilt **live** on language change.
- **`ControlHintsHUD.cs`** — the always-on chip captions (Move / Interact / Help) are
  now glyph-routed **and** localized + shaped, refreshed live on language change.
- *(OnboardingOverlay already routes glyphs + localizes per step via Phase 54/56 —
  re-verified clean.)*

### Phase 59 — End-of-day strand-proofing (D-078)
- **`EndOfDaySequencer.cs`** — the Goodnight-card await is now watchdog-bounded
  (`cardWatchdogSeconds`, default 120 s unscaled, `0` = forever) and breaks if the
  card is destroyed. No time pressure (far longer than any human pause); a pure
  anti-strand safety net that guarantees the day always advances.

### Phase 60 — Environment enrichment + placement accuracy (D-077)
- **`Phase60_HollowDressingEnrichment.cs`** (new, idempotent, chained):
  1. **Silently** hides red trigger-zone quad renderers across all gameplay scenes
     (folds the Phase 55 safe fix into the automated chain — no dialog).
  2. Dresses the Hollow with a composed **baker's corner** (flour barrel, grain sack,
     crate, firewood pile, clay jar, bread, folded cloth) sourced from the **Medieval
     Village** pack already in the project, anchored to the `Workbench`.
  3. Every prop: under one **managed root** (`_Phase60_HollowDressing`, rebuilt each
     run → idempotent + reversible), **raycast-grounded** to the floor (no
     float/clip), and given a **Collider** if its prefab shipped without one.

---

## 3. Decisions (ARCHITECTURE ledger)

- **D-074** — *Single-entry Hearthbound menu.* The Hearthbound editor menu shows only
  `🚀 Build Everything`. A runtime `[InitializeOnLoad]` consolidator prunes the rest
  via `Menu.RemoveMenuItem` (builders kept; Build Everything calls them by reflection).
  Supersedes D-051's "three top-level entries / Advanced submenu" rule.
- **D-075** — *Build Everything is the complete chain.* Phases 54 (glyphs), 56 (Arabic
  font) and 60 (env enrichment) are chained in, so the single menu entry reaches every
  builder.
- **D-076** — *Help/hint emoji + Arabic.* Help overlay and ControlHints route emoji via
  `HollowGlyphs` (no tofu) and localize their full copy EN/العربية with shaping + RTL,
  rebuilt live on language change. Help-card strings are inline UI-chrome local to the
  card; menu/settings chrome stays in `LocalizationService` (D-065).
- **D-077** — *Environment enrichment is a reversible managed layer.* New dressing lives
  under one managed root, raycast-grounded + collider'd, sourced from in-project packs;
  re-runnable and deletable without touching authored scene content.
- **D-078** — *Every night-chain await is bounded.* The Goodnight card join is watchdog-
  capped (no time pressure) so no UI state can ever hang the day-advance — completing
  the D-069 "never strand the player" guarantee.

---

## 4. Docs-vs-build status after this pass

✅ matches · 🟡 partial / by-design · 🔜 recommended next

| Area | State |
|---|---|
| End-of-day → next day advances; no freeze; every await bounded | ✅ (D-069 + D-078) |
| Hearthbound menu = single `🚀 Build Everything`; all phases chained | ✅ (D-074/D-075) |
| Onboarding/help/hint emoji render as glyphs in TMP (never tofu) | ✅ (D-070/D-076) |
| Arabic UI chrome applied + renders (font + shaper) + Help/Hints localized | ✅ (D-065/D-073/D-076) |
| Tutorial: one-concept-per-card onboarding + glyph/localized Help card | ✅ |
| Red trigger-zone quads retired; Hollow interior enriched, grounded, collider'd | ✅ (D-077) |
| Hand-written **dialogue / ledger / dream prose** in Arabic | 🟡 by design — canonical EN; dedicated writer translation pass is future work (D-065). Shaper + font are ready. |
| Player avatar reads as a cozy villager (not a grey placeholder) | 🔜 `HH-AVATAR` — curate via `CharacterAppearance` |
| Floating note sprite / oversized hearth VFX | 🔜 judgement-call grounding/clamp — verify in-Editor (Phase 55 audit lists them; auto-clamp avoided so walls/floor aren't shrunk) |
| Dialogue camera frames Doris | ✅ wired (D-071) — re-verify in-Editor |

---

## 5. QA acceptance (verify in-Editor after `git pull` → recompile → `🚀 Build Everything`)

1. **Menu:** the **Hearthbound** menu shows exactly one item — `🚀 Build Everything`.
   No `🔍 Diagnose Build`, no `⚙️ Advanced` submenu.
2. **No freeze:** Day 1 → "Sleep — End Day" → ledger closes → Dream → Goodnight →
   **Day 2 loads** ("The Widower's Request"). Pressing Sleep twice does nothing extra.
3. **Emoji:** Help (`H`) + onboarding + hint chips show gold glyphs (or clean text) —
   **never** a tofu box ▯.
4. **Arabic:** Settings → العربية → the Help card body, hint chips, menus, settings,
   character creator all render **connected, right-aligned Arabic** (no boxes), and
   flip back on English — live.
5. **Environment:** the red floor quads are gone; the Hollow has a grounded baker's
   corner near the Workbench (no floating/clipping); delete `_Phase60_HollowDressing`
   to revert.
6. Boot → menu → Day 1 → Day 2 with **zero NRE** in the Console.

---

## 6. Round-2 re-review

A second pass re-reads every change above against the Cozy Contract (no "FAILED",
no punishment, no player-visible numbers in emotional UI, refusal honored), the
asmdef graph (no cycles), and the hand-written-dialogue rule. Results recorded at the
bottom of `STUDIO_LOG.md`.

---

*Maintained by the Hearthbound Hollow virtual studio. Companion to
`Docs/Phase54_QA_VideoReview_and_Fixes.md` and `STUDIO_LOG.md`.*
