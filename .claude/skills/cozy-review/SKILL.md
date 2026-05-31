---
name: cozy-review
description: Review any change to Hearthbound Hollow against the Cozy Contract, the design pillars, and accessibility/comfort standards before it ships. Use whenever a feature, dialogue branch, mini-game tweak, or UI change touches the player experience — and as the final gate before calling work done.
---

# Cozy Review — the Comfort & Critic gate

Owned by **Pell Doyne (Cozy Comfort Engineer)** with **Critic & Review Board** sign-off. Run this on anything player-facing.

## The Cozy Contract (six promises, every minute of play)
Source: `GAMEPLAY_GUIDE_OVERVIEW.md §3`. A change that breaks any of these is rejected.

1. **Nothing punishes kindness.** Kind actions move trust/vow/approval positive or neutral, never negative.
2. **Failure is narratively absorbed, not mechanically scored.** No "FAILED" UI, no fail screen. A poor outcome is described by a character's reaction (e.g. Gerrold's outro), never a score.
3. **No time pressure unless the player asks for it.** No countdown on choice screens. No timer/coroutine fires destructive consequences without explicit input.
4. **Refusal is always honored.** Every "I'd rather not" has authored prose and a real route (e.g. `Doris_M1_RefusedPath`).
5. **No content surprises.** Heavy themes are gated by the Tone Compass primer + Heavy-Theme Warning Card. Opt-out exists.
6. **Auto-Complete is always available.** Polish & Cleanse expose Auto-Complete from frame 0, with the *same* narrative consequence as manual play.

## Design pillars (GAME_DESIGN §9)
- Every villager fully written — no filler line.
- Tactile over textual — prefer orb manipulation to a text box.
- Choices shape, never punish.
- Cozy framing even in heavy moments — lighting/music/pace stay warm.
- Replayable on emotion, not grind.

## Accessibility / comfort checklist
- **Gentle Mode** respected? (disables Sprint/Jump, relaxes mini-game tolerances, longer timeouts). Sprint/Jump are opt-in flags gated by `SettingsService.GentleMode` (D-036).
- Subtitle size tiers, color-blind palettes, one-hand control still work?
- Onboarding is per-save not per-play (D-043); skippable.
- Text goes through `UIAutoFitText` (D-042); readable on small canvases.

## Review output format
For each change, answer:
1. Which Contract promises does it touch? Any broken? (block if yes)
2. Which pillar(s) does it serve / strain?
3. Is the voice signature intact? (defer to `yarn-writer` for prose)
4. Are all branches reachable? (Critic Q3)
5. Does it raise engagement **without raising stress**? If it adds challenge, where's the comfort opt-out?
6. Verdict: ✅ Approved / ⚠️ Approved with notes / ❌ Blocked — with the specific fix.

## Anti-patterns to catch
- Any visible raw number in emotional UI (trust %, score, timer).
- A "correct" answer that makes other choices feel wrong.
- A juicy effect that reads as alarming (harsh screen shake, red flashes, loud stings).
- A new mechanic with no Auto-Complete / Gentle path.
- Surprise grief with no warning card upstream.
