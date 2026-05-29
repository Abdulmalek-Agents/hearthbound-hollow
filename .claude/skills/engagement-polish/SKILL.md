---
name: engagement-polish
description: Make Hearthbound Hollow moments land — game-feel, juice, feedback, interactivity, and the "one more day" hook, all within cozy bounds. Use when a feature works but feels flat, when adding appealing/shareable touches, or when planning an engagement pass. Always pair with cozy-review so polish never adds stress.
---

# Engagement & Polish — cozy juice

Owned by the **Engagement/Game-Feel Designer** with **VFX**, **Audio**, **Art Dir**, and **Pell Doyne** (so it stays cozy). Polish here means *warmth and satisfaction*, not arcade flash.

## The cozy-juice principle
Every interaction should give **soft, multi-sensory positive feedback**: a sound, a subtle motion, a light/color shift, a particle, a haptic. Stack 2–3 gentle channels rather than one loud one. **Cozy juice is felt, not announced** — warm glows, settling motion, satisfying *clicks*; never screen-shake, red flashes, or harsh stings.

## Feedback channels (the toolbox)
- **Visual:** orb shader response (`_Clarity` swell on polish, `_GlowStrength` bloom on completion, `_DissolveProgress` on cleanse), gentle squash/scale, ease-out tweens, candlelight flicker, dust motes, particle puffs (AllIn1 / VoluSmoke).
- **Audio:** layered one-shots, satisfying completion swell, music duck on emotional beats, ambient bed (already ducks on voice). Reactive Pickle purr.
- **Motion/anim:** anticipation→action→settle on every interactable; idle micro-motion so the world breathes; NPC look-at.
- **Camera:** soft push-in on a reveal, gentle ease on completion — no whip/shake.
- **Haptics:** light controller rumble on orb seat/seal (off in Gentle Mode).

## Engagement loops to strengthen (current slice)
1. **Polish/Cleanse "feel."** Make the clarity reveal a *moment* — building glow, a held beat, an exhale of sound. The payoff is the orb becoming beautiful.
2. **Reward the eye, not a score.** Completion = the orb on the shelf catches candlelight; the Codex entry illustrates; a dream unlocks. No numbers.
3. **The "one more day" hook.** End each day on a soft open loop: a hint of tomorrow's visitor, a half-revealed Echo Web thread, Pickle's tease. Curiosity, never anxiety.
4. **TikTok-able moments** (Trend Analyst): the orb-polish footage, the dream vignettes, the candlelit shop at dusk — frame at least one shareable, loopable beat per session.
5. **Living world:** ambient villager motion, weather shifts, hearth fire, herb garden swaying — small idle life so the place feels real.

## Polish-pass checklist for any interaction
- Does pressing/using it produce feedback within ~80 ms?
- Is there an anticipation and a settle, or does it just snap?
- Is the *completion* a celebrated micro-moment (light + sound + a held beat)?
- Does idle state still breathe (no dead-frozen objects)?
- Is every effect gentle enough to pass `cozy-review`? Gentle-Mode-safe?
- Would a streamer clip this? If not, what would make them?

## Guardrails
- Run output through **`cozy-review`** — polish must never raise stress or break the Contract.
- Respect performance gates (`unity-engineer`): pool VFX/SFX, keep ≤16 ms.
- No feature creep — juice existing moments before inventing new systems (Krieg/scope).
