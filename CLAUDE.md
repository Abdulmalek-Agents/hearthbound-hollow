# CLAUDE.md — Hearthbound Hollow Studio Handbook

> Always-loaded instructions for working on this project. Read this first, every session.
> This is the operating contract. The deep design lives in `GAME_DESIGN.md`, `Docs/ARCHITECTURE.md`, `Docs/Depth_Bible/`, and the gameplay guides.

---

## 1. What this is

**Hearthbound Hollow** — a cozy memory-broker narrative simulation. You inherit a memory-brokerage shop in an autumn village; you buy, sell, polish, cleanse, weave, and sever villagers' memories (glass orbs). *Stardew*'s warmth × *Spiritfarer*'s weight × *Strange Horticulture*'s tactility × *Disco Elysium*'s respect for words — at a cozy register.

- **Engine:** Unity 6000.4.4f1 (Unity 6 LTS), **URP-Mobile** target. 60 fps on mid-range Android/Switch is the discipline.
- **Dialogue:** Yarn Spinner. **Hand-written only — AI-generated dialogue is forbidden** (GAME_DESIGN §9 Pillar 1).
- **Current state:** Phase 32+. Mission 1 + Mission 2 form a **narrative-complete vertical slice** ("greenlight-ready"). Remaining gaps are art / composer / VO / **engagement polish** — not content.
- **Studio-authored code lives in `Assets/_Project/`.** Everything else under `Assets/` is vendored asset packs (do not relocate — moving them breaks `.meta` GUIDs and forces multi-GB reimports).

## 2. The Studio (how we work)

This project is built by a **virtual studio of named specialists**. When you take on a task, *convene the relevant roles* and think/act from their perspective. The full roster + convening protocol is the **`hearthbound-studio`** skill. Invoke the right discipline skill for the job:

| Skill | Use it when |
|---|---|
| **`hearthbound-studio`** | Pick which specialists a task needs; cross-discipline planning; the roster. |
| **`cozy-review`** | Reviewing any change against the **Cozy Contract** + design pillars + Critic Board sign-off. |
| **`yarn-writer`** | Writing/editing dialogue, codex, ledger, dream prose. Voice signatures live here. |
| **`unity-engineer`** | C# code, asmdef graph, builders, scenes, performance, the Editor build pipeline. |
| **`engagement-polish`** | Game-feel, juice, interactivity, fun — making moments land. |
| **`qa-playtest`** | Auditing the build against gameplay guides + acceptance criteria. |

## 3. Golden rules (non-negotiable)

1. **The Cozy Contract holds, always** (`GAMEPLAY_GUIDE_OVERVIEW.md §3`): nothing punishes kindness; failure is narratively absorbed, never scored; no time pressure unless the player asks; refusal is always honored; no content surprises (Tone Compass + Heavy-Theme cards); Auto-Complete is always available. **No "FAILED" string ever ships.** No player-visible numbers in emotional UI.
2. **No dark monetization, ever.** No gacha, energy, lootboxes, P2W. Trust is the moat.
3. **The Out-of-Scope Wall** (`Docs/ARCHITECTURE.md §13`) is real. Do not build Weave/Sever/Listen-room, extra villagers, the full Marin arc, etc. into this branch without explicit approval. Codices 02–16 are the *scaling reference for after* the 20-person greenlight, not a backlog.
4. **Editor entry point:** users run **`Hearthbound → 🚀 Build Everything`** (idempotent, chains all phase builders) and **`🔍 Diagnose Build`** (read-only). Per **D-051**, new Editor actions register under `Hearthbound → ⚙️ Advanced/…` unless the Critic Board promotes them.
5. **Respect the asmdef graph** (`ARCHITECTURE.md §3`). Every new script lives in an asmdef that declares the deps it uses (**D-035**). Never create a cycle (e.g. Audio must not reference UI).
6. **No code lands without a `Docs/PROGRESS.md` update** — phase/sub-task, files touched, follow-ups, and any deviation from ARCHITECTURE.md with rationale. Architectural decisions get a `D-0xx` entry.
7. **Idempotency:** builders use load-or-create + heal-then-save. Inspector tweaks belong on **prefabs**, not scenes (scene capstones rebuild scenes 00–05 by design).

## 4. Key map

- Code: `Assets/_Project/Scripts/{Core,Memory,Player,MiniGames,UI,Dialogue,Cutscene,Audio,Mission,Save,Editor}`
- Scenes: `Assets/_Project/Scenes/` — `00_Bootstrap → 01_MainMenu → 02_Mission01_Lane → 03_Mission01_Hollow → 04_Mission02_Garden → 05_Mission02_Cottage`
- Dialogue: `Assets/_Project/Yarn/` (Doris_M1, Gerrold_M2, Pickle, Codex, EveningLedger, ChoiceCards, Dreams, Marin_*)
- Design truth: `GAME_DESIGN.md`, `Docs/GDD_EN.md`, `Docs/ARCHITECTURE.md`, `Docs/Depth_Bible/` (17 codices), `Docs/GAMEPLAY_GUIDE_*.md`
- State: `Docs/PROGRESS.md` (living log), `CHANGELOG.md`, `Docs/PLAYTEST_AUDIT.md`
- Voice pipeline: `Tools/generate_voices.sh` (Piper TTS) + `Phase46_VoiceGenerator.cs` (espeak-ng fallback). Any 22 kHz mono PCM16 `.wav` at `Audio/Voice/<Char>/<lineId>.wav` drops in (**D-058/D-059**).

## 5. Working defaults

- **Plan before large changes**; keep PRs atomic and buildable, minimal reimport delta.
- **Cite design intent.** When a choice is non-obvious, point to the codex/guide section that justifies it.
- **Bilingual project** (EN + AR docs exist). Keep parity in mind for player-facing copy; coordinate with the writer/localization role.
- **Branch:** feature work happens off `feat/mission-1-2-architecture`; main is the release line.
- When unsure whether something is in scope, **ask** — the Out-of-Scope Wall exists to protect the greenlight.
