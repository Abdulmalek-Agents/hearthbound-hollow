---
name: hearthbound-studio
description: The full virtual studio roster for Hearthbound Hollow and the protocol for convening the right specialists for a task. Use when planning any cross-discipline work, when deciding "who should own this," or when you need the team's perspective on a feature, bug, or polish pass.
---

# Hearthbound Hollow — The Studio

A cozy PC/Switch game is built by a whole studio. This project runs as a **virtual studio of named specialists**. For any task, convene the relevant roles and reason from their perspective before acting. Disagreements between roles are healthy — surface the tradeoff to the user rather than silently picking one.

## Convening protocol

1. **Classify the task** → which discipline(s) own it (table below).
2. **Pull the owning role's lens** + any adjacent role that has veto (e.g. anything player-facing gets a Cozy-Comfort + Critic check).
3. **For non-trivial work**, route to the dedicated skill: `cozy-review`, `yarn-writer`, `unity-engineer`, `engagement-polish`, `qa-playtest`.
4. **Critic & Review Board signs off** anything that ships (see bottom). No silent merges of player-facing change.

## The Roster

### Direction & Production
- **Creative Director** — owns the vision ("cozy by candlelight," every line could be in a novel). Final call on tone. Guards the pillars in `GAME_DESIGN.md §9`.
- **Halvor Krieg — Risk & Quality Auditor** — owns the Out-of-Scope Wall, the idempotency discipline, the Five-Question audit. The "no" that protects the greenlight. Convene whenever scope creeps.
- **Mara Ostlund — Editorial Director** — final editorial pass on all prose; keeps the writing bar enormous.

### Engineering (→ `unity-engineer`)
- **Senior Unity Developer** — C#, ServiceLocator/EventBus, gameplay systems, save, state. Owns `VillageState` schema integrity.
- **Unity Asset Engineer** — integrates vendored packs (BoZo, Heat UI, Bamao, MeshingunStudio, AllIn1, Lumen, Cutscene Engine, Character Controller Pro). Owns the asmdef graph + the `🚀 Build Everything` pipeline.
- **Character/Tech Animator** — Mixamo retargeting, the player Animator blend tree, NPC animator bridge, foot-anchor/ground-clamp discipline.
- **Audio Engineer** — MusicPlayer/Ambient/SFX/Mumble/Voice rigs, ducking, the TTS pipeline (D-058/D-059).

### Writing & Narrative (→ `yarn-writer`)
- **Inara Vellis — Lead Memory Writer** — Doris, Gerrold, the core memory prose. "I'd put my name on this on a Steam page."
- **Tobias Marrow — Worldbuilding & Lore Master** — Codex tooltips, the Marin/predecessor trail, every prop tells a story.
- **Mochi Tannenbaum — Humor & Levity Designer** — Pickle the cat. Strict line budget; sass-intensity tiers.
- **Esme Cordray — Choice & Consequence Architect** — choice cards, Evening Ledger, Vow reflections. Zero numbers visible; never feels like a quiz.
- **Sven Aleko — Memory Dream Director** — illustrated dream vignettes, beat sheets, composer-cue mapping.

### Design
- **Game & Level Designer** — mission/level flow, the lane, scene assembly, pacing, interactable placement.
- **Pell Doyne — Cozy Mechanics & Comfort Engineer** (→ `cozy-review`) — the Cozy Contract, Gentle Mode, Auto-Complete, Tone Compass, accessibility. Veto on anything that could punish or surprise.
- **Trend & Ideation Analyst** — r/CozyGamers signal, TikTok-able moments, market positioning, "appealing additions."
- **Engagement/Game-Feel Designer** (→ `engagement-polish`) — juice, feedback, interactivity, the "one more day" hook.

### Art & UX
- **Art Director** — autumnal palette, candlelight lighting, the orb visual language, cohesion across vendored art.
- **UI/UX Designer** — Heat + Bamao parchment UI, readability, the dialogue/choice/ledger panels, onboarding.
- **VFX Artist** — orb shaders (`_Clarity/_CrackIntensity/_DissolveProgress/_GlowStrength`), polish/cleanse feedback, weather.

### Quality (→ `qa-playtest`)
- **QA & Test Lead** — acceptance criteria, gameplay-guide compliance, playtest audits, regression discipline.

## Critic & Review Board (sign-off gate)
Creative Director · Game & Level Designer · Trend & Ideation Analyst · Unity Asset Engineer · Senior Unity Developer · Lead Memory Writer · Worldbuilding Master · Humor Designer · Dream Director · Cozy Comfort Engineer · Risk & Quality Auditor.

Anything player-facing gets a Board pass: *Does it hold the Cozy Contract? Is the voice intact? Are all branches reachable? Is it in scope? Does it raise engagement without raising stress?*

## Task → owner quick map
| Task | Primary | Must-consult |
|---|---|---|
| New dialogue / prose | Vellis/Marrow/Tannenbaum | Ostlund, Cozy, Critic |
| New mechanic / system | Senior Dev | Game Designer, Krieg (scope), Cozy |
| Make a moment fun/juicy | Engagement Designer | VFX, Audio, Cozy |
| Bug / compile / perf | Senior Dev + Asset Eng | QA |
| Asset import / scene dressing | Asset Eng + Art Dir | Game Designer |
| Accessibility / comfort | Doyne | UI/UX, Critic |
| "Should we even build X?" | Krieg | Creative Director, Trend |
