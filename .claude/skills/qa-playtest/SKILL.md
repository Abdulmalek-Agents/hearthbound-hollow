---
name: qa-playtest
description: Audit the Hearthbound Hollow build against the gameplay guides and acceptance criteria, and run structured playtest/regression passes. Use before declaring work done, when verifying a fix, or when checking the vertical slice is still greenlight-ready.
---

# QA & Playtest — the build is owned by the playtest

Owned by **Halvor Krieg (Risk & Quality)** + **QA & Test Lead**. Reference: `Docs/PLAYTEST_AUDIT.md`, `Docs/GAMEPLAY_GUIDE_*.md`.

## The Krieg Five-Question Discipline
For every system touched, ask:
1. **Is the canonical text/behavior actually shipped?** (If the guide says it and the build lacks it → GAP.)
2. **Is the speaker's voice signature preserved?**
3. **Are all branches reachable?** (every choice path, refusal path, conditional line)
4. **Are all variables correctly written + backed by a real field?**
5. **Is the Cozy Contract honored?** (no numbers, no fail screen, refusal valid, Auto-Complete present)

A GAP is logged if any answer is "no." Fix in atomic, additive commits.

## Acceptance criteria (Focus 08 — 30 total)
- **Functional (10):** lane walk <90 s; Doris/Gerrold branches resolve; Polish 60–120 s; Auto-Complete from frame 0; Ledger saves across restart; Day 2 loads; herb harvest ≤3 presses; tea brew no frame drops.
- **Narrative (8):** all 4 M2 paths + variants reachable; Echo Web first-light fires; Ledger prose matches choice; Vow glyphs change.
- **Comfort (7):** Tone Compass shows + skippable; Gentle Mode toggle mid-game; Auto-Complete Polish/Cleanse independent; subtitle tiers; color-blind palettes; one-hand control.
- **Performance (5):** 60 fps lane/Hollow; 30 fps dreams on Switch; build <1.2 GB; load <22 s.

Track each as ✅ / 🟡 (pending art/composer/VO — not a regression) / ❌.

## Regression discipline
- No commit introduces a new compile error or a "FAILED" string.
- Re-run EditMode + PlayMode tests; smoke scene boots GameManager → state → Doris → Polish with **zero NRE**.
- Diff player-facing prose against the gameplay guide line-by-line.
- After any builder change, run `🔍 Diagnose Build` and confirm clean.

## How to report
Produce a punch list: GAP / PASS / PENDING per criterion, with file:line and the exact remediation. Separate **regressions** (block) from **pending production deliverables** (art/composer/VO — don't block greenlight). End with a clear verdict: greenlight-ready or not, and the shortest path to green.

## Greenlight bar
20 cozy-target playtesters finish M1+M2 (~1 hr); ≥14 (70%) say "I want to play more." Everything QA does serves that gate.
