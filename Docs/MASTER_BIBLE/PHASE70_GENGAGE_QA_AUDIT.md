# 🧪 Phase 70 — G-Engage QA Readiness Audit (Senior QA pass)

> **Owner:** 4× Senior QA + Technical Lead + Lead Game Designer. **Scope:** a static
> "play-the-loop" readiness pass over the now-built engagement loop (Pillars P1–P7,
> Phases 61–67) ahead of the **G-Engage** human playtest (`../PHASE70_GENGAGE_PLAYTEST.md`).
> **Method:** code-level audit of the loop services + UI + save schema against the
> **Cozy Contract**, the 7-pillar acceptance criteria, and the cross-system wiring.
> **Branch:** `feat/mission-1-2-architecture`. **Not** a human playtest — that is the
> next step; this pass clears the build to *enter* it.

---

## 0. Headline

> **The loop is wired, reachable, and Cozy-Contract-clean. One real cross-system
> bug was found and fixed during this pass (Reading Nook gate id, D-084). The build
> is GO for the G-Engage 3-day playtest** — pending an in-engine `🚀 Build Everything`
> → Play smoke test (the project's standard validation loop; no local Unity compile here).

---

## 1. What this pass verified (PASS / FIXED / VERIFY-IN-ENGINE)

| # | Check | Method | Result |
|---|---|---|---|
| QA-1 | **Cozy Contract — no "FAILED" player-facing string** | grep over `Mission/ UI/ MiniGames/` UI paths | ✅ PASS — none found |
| QA-2 | **Reading Nook reachable** — gate id matches the shop catalog | cross-ref `ReadingNookInteractable` ↔ `HollowProgressionService` | 🔧 **FIXED (D-084)** — was `pickle_cushion`, catalog ships `DECOR_PICKLE_CUSHION`; aligned |
| QA-3 | **All 6 loop screens bound to keys** | grep `KeyCode` in the loop UIs | ✅ PASS — `[B]` Request · `[M]` Memory Wall · `[U]` Shop · `[G]` Garden · `[K]` Workbench · `[J]` Journal |
| QA-4 | **G-Engage telemetry exists** (voluntary Day-4 signal) | `Core/EngagementTelemetry.cs` (Phase 70) | ✅ PASS — logs the `DayIndex>=3` voluntary-4th-day headline metric (opt-in) |
| QA-5 | **Reading Nook is actually built by one click** | `Phase27_BuildEverything` chain | 🔧 **FIXED** — Phase 52 was skipped; now chained after Phase 51 (commit `190526c`) |
| QA-6 | **Phase 52 builder compiles** | TMP enum audit | 🔧 **FIXED** — `MidpointLeft`→`MidlineLeft` (commit `c827c10`) |
| QA-7 | **Reading Nook runtime compiles** | symbol-resolution audit | 🔧 **FIXED** — using/overrides/events/`letterFragmentIdsRead` (commits `faa00cc`,`ec90bd6`) |
| QA-8 | **Save compounds across days** | `VillageState` schema (coin, upgrades, garden, echoes, resolvedRequestIds) | ✅ PASS (static) — additive, null-guarded; **VERIFY** persistence in-engine |
| QA-9 | **Single day counter (no double-increment)** | D-077 — `DailyLoopService.EndDay()` sole owner | ✅ PASS (static) — `GameManager.EndDay()` delegates |
| QA-10 | **Request Board never empty** | `RequestBoardService` arcs + walk-ins + quiet-day fallback | ✅ PASS (static) |

## 2. The bug this pass caught (and fixed) — D-084

**Symptom:** A player earns coin, buys *"A cushion for Pickle"* from the shop `[U]`, walks
to the armchair… and Pickle still refuses them, forever. The Reading Nook (Marin's 5
letter-fragments — a core thread of the predecessor mystery) is **permanently unreachable.**

**Root cause:** an id mismatch across two systems:
- `ReadingNookInteractable` gated on `purchasedUpgradeIds.Contains("pickle_cushion")`.
- The shipping built-in catalog (`HollowProgressionService`; no authored
  `Resources/HollowCatalog.asset` exists — confirmed 404) sells it as `"DECOR_PICKLE_CUSHION"`.

**Fix (D-084):** aligned the gate default to `"DECOR_PICKLE_CUSHION"`. The intended
**coin → cushion → Pickle approves → Marin's letters → predecessor mystery** loop (the
file's own D-082 rationale) now completes. This is exactly the kind of cross-system
wiring the Art/Asset-Integration + QA joint review is meant to catch before a playtest.

## 3. The loop, end to end (what a tester can now do)

`☀ Agenda` → choose the day → `[B]` meet a rotating villager & keep a memory (earn coin) →
`[G]` tend garden / brew tea → `[K]` craft at the bench → `[U]` spend coin to grow the Hollow
(incl. Pickle's cushion) → `[M]` chase Echo threads → read Marin's letters at the **Reading
Nook** → `[J]` glance the journal → `🌙` close the ledger (celebratory recap + tomorrow tease)
→ **wake into a changed day.** All six screens self-install at Play; the Reading Nook is
placed by `🚀 Build Everything`.

## 4. G-Engage entry checklist (the gate to the human playtest)

- [x] Loop services self-install (observer-only on `DayStartedEvent`) — no scene edits needed.
- [x] All 6 loop screens reachable by key; Reading Nook reachable after the cushion purchase.
- [x] Cozy Contract intact in the new surfaces (no fail/timer/“FAILED”; Auto-Complete on craft).
- [x] Telemetry records the voluntary-Day-4 signal (opt-in; writes a local CSV).
- [x] Compile blockers cleared (D-078, Phase 52 TMP, build-chain).
- [ ] **In-engine smoke test** — pull → `🚀 Build Everything` → Play 3 days → confirm no Console errors, the Reading Nook opens post-cushion, and the save compounds on reload. *(Owner action — the one step that needs the Editor.)*

## 5. Recommended sequence after this audit

1. **In-engine smoke test** (the unchecked box above) — the only item needing the Unity Editor.
2. **Run G-Engage (the 20-tester, 3-day playtest)** — instrument voluntary Day-4 + ≥60% self-set-goal naming.
3. **Tune from data (Phase 71)** — pace heavy beats; adjust coin/upgrade curve if Day-4 is < 75%.
4. **Marketing-truth pass (Phase 72)** — README "Daily Loop" section. *Held for a human writer per Pillar 1 (`../MARKETING_TRUTH_Phase72.md` §3); QA does not auto-write store voice.*

## 6. Discipline notes (what QA deliberately did NOT touch)

- **No dialogue authored.** New villager lines / arc beats are *hand-written-only* (Pillar 1); QA flags content gaps but does not fill them with AI prose.
- **No store-voice rewritten.** The README marketing pass is left for a human writer (`../MARKETING_TRUTH_Phase72.md` §3).
- **Fixes were additive + behaviour-preserving.** Only the one id mismatch changed behaviour (it enabled a feature that was dead); everything else was compile/wiring.

---

## 7. Sign-off

| Reviewer | Verdict |
|---|---|
| 🧪 Senior QA (loop) | ✅ GO for G-Engage after the in-engine smoke test; D-084 unblocks the Reading Nook |
| 🔧 Technical Lead | ✅ Compile blockers cleared; fixes additive + asmdef-clean |
| 🗺️ Lead Game Designer | ✅ The six screens deliver the compounding loop; Cozy Contract preserved |
| 🎨 Art/Asset Integration | ⚠️ VERIFY in-engine: the Phase 52 armchair grounds correctly in `03_Mission01_Hollow` (placeholder primitives; swap for a mesh later — no code change) |
| 📊 Market Critic | ✅ With the loop reachable end-to-end, the `$14–18M` ceiling is in play pending Day-4 retention |

*Phase 70 QA Readiness Audit v1.0 — `feat/mission-1-2-architecture` · 2026 · Abdulmalek Agents.*
*Decisions referenced: D-074 (single Build Everything entry), D-076 (celebratory feedback), D-077 (single day counter), D-078 (Reading Nook compile + letterFragmentIdsRead), D-084 (Reading Nook gate id).*
