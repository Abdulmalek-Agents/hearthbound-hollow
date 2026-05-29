# 🔥 Hearthbound Hollow — **Engagement Bible**
### The "Make It Fun" Canon · Senior Studio Review + Redesign · 2026

> **Why this folder exists.** The project owner asked the studio to read the whole
> project, play it honestly, and answer one hard question:
>
> > *"Is this game actually fun — and if not, why, and how do we make it as engaging
> > and replayable as Stardew Valley and the best cozy games?"*
>
> The honest answer (Critic & Review Board, unanimous) is documented in `01`. The
> **Depth Bible** (`../Depth_Bible/`) proved the game has *narrative depth*. This
> **Engagement Bible** addresses the orthogonal problem the Depth Bible never solved:
> **the moment-to-moment loop is thin, linear, and one-and-done.** A game can have a
> short-novel's worth of beautiful prose and still be boring to *play*.
>
> The Depth Bible is the **song**. The Engagement Bible is **why anyone presses Play tomorrow.**

---

## 0. Read order

| # | Document | Owner | One-line |
|---|---|---|---|
| 00 | `00_INDEX.md` | Editorial Director | This file. Map + reading order. |
| 01 | `01_CRITIQUE_WHY_IT_IS_BORING.md` | 🔍 Critic & Review Board | The candid verdict: why the current slice will not succeed, benchmarked against Stardew / Spiritfarer / Coffee Talk. **Read first.** |
| 02 | `02_ENGAGEMENT_MASTER_PLAN.md` | 🎬 Creative Director + 🗺️ Game Designer | The fix: the 7 Engagement Pillars + the new compounding loop. The thesis document. |
| 03 | `03_THE_COZY_DAILY_LOOP.md` | 🗺️ Game & Level Designer | The redesigned day: morning → shop → village → garden → workshop → ledger, and *why you come back*. |
| 04 | `04_SYSTEM_DAILY_LOOP_AND_TIME.md` | 👨‍💻 Senior Unity Dev | `DayCycleManager` activation + `DailyLoopService` + agenda. Code-level guidance. |
| 05 | `05_SYSTEM_REQUEST_BOARD_AND_VISITORS.md` | 🗺️ Designer + 👨‍💻 Dev | The replayability engine: a rotating request board so content never runs out. |
| 06 | `06_SYSTEM_HOLLOW_PROGRESSION.md` | 📈 Systems Architect | Ownership + visible growth: shelves, rooms, tools, decor. The "my shop" feeling. |
| 07 | `07_SYSTEM_GARDEN_TEA_ECONOMY.md` | 🌿 Systems + 🎨 Asset Engineer | Activate the unused **HarvestGarden** asset into a real grow→brew→use loop. |
| 08 | `08_SYSTEM_WORKSHOP_VARIETY.md` | 🧩 Mini-Game Designer | Kill mini-game fatigue: from 2 verbs to a satisfying, varied, *juicy* workbench. |
| 09 | `09_SYSTEM_CODEX_ECHO_METAGAME.md` | ⚖️ Choice Architect | Turn the Echo Web from a description into a real collect-and-connect meta-game. |
| 10 | `10_IMPLEMENTATION_ROADMAP.md` | 🐙 GitHub PM + 👨‍💻 Lead | The phased build order (Phase 61 → 72), each step shippable + idempotent. Also the canonical phase log for this work. |

---

## 1. The one-paragraph thesis

Hearthbound Hollow has **world-class depth and zero loop**. The current build is a
75-minute authored corridor: walk, talk, do one slow mini-game, watch a cutscene,
repeat once. Stardew Valley is the opposite — a *thin* story wrapped around an
**infinite, compounding, player-directed loop** you choose to live inside. Our job is
not to add more prose (we have plenty). Our job is to **build the loop**: a real day
you wake into, a shop you *own and grow*, a request board that *never empties*, a
garden you *tend*, a workbench with *variety and juice*, and a memory collection that
*rewards you for connecting it.* Keep every ounce of the existing heart. Wrap it in a
reason to play for 40 hours instead of 75 minutes.

---

## 2. Relationship to the existing canon

- **Cozy Contract (`CLAUDE.md` Golden Rule 1)** — *fully preserved.* Nothing here punishes kindness, adds fail states, or introduces dark monetization. Engagement ≠ pressure.
- **Out-of-Scope Wall (`ARCHITECTURE.md §13`)** — *consciously revisited by owner request.* The owner has directed the studio to expand scope toward replayability. This folder is the new, owner-approved scope. Each system still ships behind `🚀 Build Everything`, idempotent.
- **Depth Bible (`../Depth_Bible/`)** — *the source of content.* Many systems here (Request Board, Echo Web, Hollow upgrades, garden) were already *designed* in codices 04/08/09/10/13 but classified `🔴 DEFERRED`. This folder **un-defers the loop-critical subset** and makes it the priority.

---

*Engagement Bible v1.0 — authored on `feat/mission-1-2-architecture` · Part of the Abdulmalek Agents game-concept portfolio · 2026.*
