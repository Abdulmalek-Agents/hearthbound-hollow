# CLAUDE.md — Hearthbound Hollow Studio Handbook

> Always-loaded instructions for working on this project. Read this first, every session.
> This is the operating contract. The deep design lives in `GAME_DESIGN.md`, `Docs/ARCHITECTURE.md`, `Docs/Depth_Bible/`, `Docs/Engagement_Bible/`, and the gameplay guides.

---

## 1. What this is

**Hearthbound Hollow** — a cozy memory-broker narrative simulation. You inherit a memory-brokerage shop in an autumn village; you buy, sell, polish, cleanse, weave, and sever villagers' memories (glass orbs). *Stardew*'s warmth × *Spiritfarer*'s weight × *Strange Horticulture*'s tactility × *Disco Elysium*'s respect for words — at a cozy register.

- **Engine:** Unity 6000.4.4f1 (Unity 6 LTS), **URP-Mobile** target. 60 fps on mid-range Android/Switch is the discipline.
- **Dialogue:** Yarn Spinner. **Hand-written only — AI-generated dialogue is forbidden** (GAME_DESIGN §9 Pillar 1).
- **Current state:** Phase 61+. Mission 1 + Mission 2 form a **narrative-complete vertical slice**, but the **Engagement Review** (`Docs/Engagement_Bible/01`) found it is a *75-minute linear corridor with no compounding loop* (0/7 vs Stardew's retention engines). The active work is **building the cozy daily loop** (7 Engagement Pillars) so the slice becomes a game players return to. Writing/art/VO are not the blocker — **the loop is.**
- **Studio-authored code lives in `Assets/_Project/`.** Everything else under `Assets/` is vendored asset packs (do not relocate — moving them breaks `.meta` GUIDs and forces multi-GB reimports).

## 2. The Studio (how we work)

This project is built by a **virtual studio of named specialists**. When you take on a task, *convene the relevant roles* and think/act from their perspective. The full roster + convening protocol is the **`hearthbound-studio`** skill. Invoke the right discipline skill for the job:

| Skill | Use it when |
|---|---|
| **`hearthbound-studio`** | Pick which specialists a task needs; cross-discipline planning; the roster. |
| **`cozy-review`** | Reviewing any change against the **Cozy Contract** + design pillars + Critic Board sign-off. |
| **`yarn-writer`** | Writing/editing dialogue, codex, ledger, dream prose. Voice signatures live here. |
| **`unity-engineer`** | C# code, asmdef graph, builders, scenes, performance, the Editor build pipeline. |
| **`engagement-polish`** | Game-feel, juice, interactivity, fun — making moments land. The Engagement Bible (`Docs/Engagement_Bible/`) is this skill's canon. |
| **`qa-playtest`** | Auditing the build against gameplay guides + acceptance criteria. |

## 3. Golden rules (non-negotiable)

1. **The Cozy Contract holds, always** (`GAMEPLAY_GUIDE_OVERVIEW.md §3`): nothing punishes kindness; failure is narratively absorbed, never scored; no time pressure unless the player asks; refusal is always honored; no content surprises (Tone Compass + Heavy-Theme cards); Auto-Complete is always available. **No "FAILED" string ever ships.** **No *anxiety-inducing* numbers in emotional UI** — but cozy, opt-in, *celebratory* progression feedback (coin earned, collection %, the morning Agenda, the shop visibly growing) is now **required**, per **D-076** (the relaxed Cordray Principle). Players cannot feel growth they are forbidden to see.
2. **No dark monetization, ever.** No gacha, energy, lootboxes, P2W. Trust is the moat.
3. **The Out-of-Scope Wall** (`Docs/ARCHITECTURE.md §13`) — **revised by D-075.** The owner has approved the **Engagement Bible** scope: the loop-critical subset of Depth-Bible codices 04/08/09/10/13 (Daily Loop, Request Board, Hollow Progression, Garden→Tea economy, Workbench variety, Codex/Echo meta-game) is now **in-scope** and is the priority. *Other* deferred systems (full Marin arc, async multiplayer, LiveOps, the Sommelier endgame, etc.) remain behind the Wall — still ask before building those. Cozy Contract (rule 1) + no-dark-monetization (rule 2) stay inviolable for every new system.
4. **Editor entry point (D-074, supersedes D-051):** the **Hearthbound** menu shows a **single** entry — **`🚀 Build Everything`** (idempotent; chains *every* phase builder incl. glyphs, Arabic font, and environment enrichment). `🔍 Diagnose Build` and the whole `⚙️ Advanced/…` submenu are **pruned at editor load** by `Phase57_MenuConsolidation` (the builder methods stay and are invoked by reflection). New Editor actions may still declare a `[MenuItem]` for dev use — it will be auto-hidden from the menu, and should be chained into Build Everything if it must ship.
5. **Respect the asmdef graph** (`ARCHITECTURE.md §3`). Every new script lives in an asmdef that declares the deps it uses (**D-035**). Never create a cycle (e.g. Audio must not reference UI). New loop services follow the existing placement: data SOs in `Memory`, services/directors in `Mission`, pure state/events in `Core`.
6. **No code lands without a `Docs/PROGRESS.md` update** — phase/sub-task, files touched, follow-ups, and any deviation from ARCHITECTURE.md with rationale. Architectural decisions get a `D-0xx` entry.
7. **Idempotency:** builders use load-or-create + heal-then-save. Inspector tweaks belong on **prefabs**, not scenes (scene capstones rebuild scenes 00–05 by design).

## 4. Key map

- Code: `Assets/_Project/Scripts/{Core,Memory,Player,MiniGames,UI,Dialogue,Cutscene,Audio,Mission,Save,Editor}`
- Engagement loop scaffolding (Phase 61.4): `Scripts/Core/{EngagementEvents,DayAgenda,DailyLoopService}.cs` (inert until the Phase 61.5 builder wires it).
- Scenes: `Assets/_Project/Scenes/` — `00_Bootstrap → 01_MainMenu → 02_Mission01_Lane → 03_Mission01_Hollow → 04_Mission02_Garden → 05_Mission02_Cottage`
- Dialogue: `Assets/_Project/Yarn/` (Doris_M1, Gerrold_M2, Pickle, Codex, EveningLedger, ChoiceCards, Dreams, Marin_*)
- Design truth: `GAME_DESIGN.md`, `Docs/GDD_EN.md`, `Docs/ARCHITECTURE.md`, `Docs/Depth_Bible/` (17 codices), `Docs/GAMEPLAY_GUIDE_*.md`
- **Engagement / "make it fun" truth: `Docs/Engagement_Bible/` (00 index → 01 critique → 02 master plan → 03 daily loop → 04–09 per-system implementation guidance → 10 roadmap & phase log).**
- State: `Docs/PROGRESS.md` (living log), `CHANGELOG.md`, `STUDIO_LOG.md`, `Docs/PLAYTEST_AUDIT.md`
- Voice pipeline: `Tools/generate_voices.sh` (Piper TTS) + `Phase46_VoiceGenerator.cs` (espeak-ng fallback). Any 22 kHz mono PCM16 `.wav` at `Audio/Voice/<Char>/<lineId>.wav` drops in (**D-058/D-059**).

## 5. Working defaults

- **Plan before large changes**; keep PRs atomic and buildable, minimal reimport delta.
- **Cite design intent.** When a choice is non-obvious, point to the codex/guide section that justifies it.
- **Bilingual project** (EN + AR docs exist). Keep parity in mind for player-facing copy; coordinate with the writer/localization role.
- **Branch:** feature work happens off `feat/mission-1-2-architecture`; main is the release line.
- **Engagement work priority order:** P1 Living Day → P2 Request Board → P6 Echo Wall → P3 Hollow Progression → P4 Garden/Tea → P5 Workbench → P7 Almanac (see `Docs/Engagement_Bible/10`). Build the loop spine before the limbs.
- When unsure whether something is in scope, **ask** — the Out-of-Scope Wall (as revised by D-075) still exists to protect focus.
