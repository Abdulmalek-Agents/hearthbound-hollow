---
name: yarn-writer
description: Author or edit Hearthbound Hollow narrative content — Yarn Spinner dialogue, codex tooltips, Evening Ledger prose, choice cards, dream beats. Use whenever writing or revising any player-facing words. Enforces character voice signatures, the no-AI-dialogue rule, and Yarn syntax/variable conventions.
---

# Yarn Writer — narrative authoring

Owned by **Inara Vellis** (lead), **Tobias Marrow** (lore/codex), **Mochi Tannenbaum** (Pickle), **Esme Cordray** (choices/ledger), **Sven Aleko** (dreams), edited by **Mara Ostlund**. See `Docs/STYLE_GUIDE.md` for the canonical contract.

## Hard rules
- **Hand-written only. AI-generated dialogue is forbidden** (GAME_DESIGN §9). This skill helps you *draft as the writer*, in their voice — never auto-fill filler.
- Every line could be in a novel. If a line is filler, cut it.
- **Tactile over textual** — if a beat can be felt through orb work or staging, don't write a paragraph for it.
- No player-visible numbers in prose (Cordray Principle). Convey trust/quality through reaction, not stats.

## Voice signatures (preserve exactly)
- **Doris** — warm, blunt, deflects feeling with practicality ("You're the new one. I thought you'd be taller." / "Hold it like you'd hold a hot bun. Not by the side. Underneath."). Bakery metaphors. Quiet grief under competence.
- **Gerrold** — gentle, halting, the verbal tic "the long bit." Will not say the hard thing directly. The Margery aside is the emotional peak — protect it.
- **Pickle (cat)** — dry, superior, secretly caring. Rendered in italics. **Strict line budget** (4 canonical + 4 conditional + contextual + hints). Sass-intensity tiers 1/3/5 route to warmer/sharper variants. Don't dilute by adding lines.
- **Marrow's Codex principle** — every tooltip either tells about Marin, seeds a long-arc, or warms the room. Never a dry label.
- **Narrator / dreams (Aleko)** — lyrical, restrained, paintable to a beat sheet + composer cue.

## Yarn conventions
- Files: `Assets/_Project/Yarn/*.yarn`. Node names follow `Character_MissionN_Beat` (e.g. `Gerrold_M2_AboutMargery`).
- Variables wired via `YarnVillageStateBridge`: `$trust_doris`, `$trust_gerrold`, `$gerrold_choice`, `$cleanse_quality`, `$pickle_approval`, `$vow_*`, `$choice_made`, `$asked_about_predecessor`, etc. Every `<<adjust_*>>` must map to a real `VillageState` field — never invent a field without the Senior Dev adding it.
- Custom commands (14, in `YarnCustomCommands.cs`): `<<polish_orb>>`, `<<cleanse_orb>>`, `<<offer_choice>>`, `<<eyes_look_at>>`, `<<pickle_say>>`, `<<lights_warm>>`, `<<save_autopoint>>`, `<<echo_reveal>>`, `<<play_cutscene>>`, … Don't reference a command that has no handler.
- Voice line IDs: tag spoken lines with stable `lineId` (e.g. `doris_m1_greet_01`) matching `Tools/generate_voices.sh` + `Phase46_VoiceGenerator.cs` tables so the typewriter locks to clip length. Missing clips degrade silently to typewriter (D-058).

## Before you commit prose
1. Voice signature intact? (read it aloud in the character's register)
2. All branches reachable + every refusal path authored?
3. All `$variables` correct and backed by a real field?
4. Cozy Contract honored (no numbers, no surprise grief without a warning upstream)?
5. Ostlund editorial pass for rhythm and cut.
6. Update `Docs/PROGRESS.md`; keep EN/AR parity in mind for localization.
