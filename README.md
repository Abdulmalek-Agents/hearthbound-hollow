# 🫖 Hearthbound Hollow

> *"Some memories want to be sold. Some don't."*

A **single-player cozy narrative simulation** set in a small autumnal village where memories are a physical, tradable commodity. You inherit a memory-brokerage shop. Buy painful memories from villagers wanting to forget. Sell preserved ones to those who long to remember. Polish, weave, and restore the fragmented — but choose carefully what you keep.

**Pitch in one line:** *Stardew's warmth × Spiritfarer's heart × Strange Horticulture's puzzles × Disco Elysium's writing — at cozy scale.*

---

## 🎮 Run the polished playable Mission 1 + 2 (Unity)

The `feat/mission-1-2-architecture` branch ships a fully playable, polished vertical slice of Missions 1 and 2 — six scenes, two villager arcs, four moral choices, two memory dreams, and a complete cozy-comfort/accessibility layer.

### One-click build

1. Clone the repo and check out `feat/mission-1-2-architecture`.
2. Open the project in **Unity 6 LTS (6000.4.4f1)**. Packages auto-install (~30–90 s).
3. Menu → **`Hearthbound → 🎮 Build POLISHED Mission 1 + 2 (Phase 23)`** — sit back ~30 s.
4. Press **Play**.

That single menu item runs all 12 phases of the build pipeline, sets up Build Settings, opens the Bootstrap scene, and leaves a fully wired 6-scene playable on disk.

### Player-facing flow

```
Bootstrap → Main Menu (Tone Compass on first run) →
Lane (autumn dusk) → walk to the Hollow door →
Hollow → meet Doris → choose a price → polish her First Loaves orb →
Evening Ledger → Garden (Day 2) → harvest herb → brew tea →
Cottage → meet Gerrold → moral choice (Erase / Cleanse / Listen / Defer) →
(optional Cleanse mini-game) → Dream 2 cutscene →
Evening Ledger → Main Menu
```

### Controls (visible any time via the H key)

| Action | Key / Stick |
|---|---|
| Move | WASD / Arrows / Gamepad left stick |
| Interact | E / Gamepad ▢ |
| Advance dialogue | Click / Space / Enter |
| Polish orb | Hold left mouse, draw slow circles |
| Pause | Escape |
| Help / Controls card | H |

### Comfort & accessibility (in Settings panel + Pause menu)

- **Gentle Mode** — longer timers, no fail states, more forgiving cleanse tolerance.
- **Auto-Complete Polish** — skip the mini-game, keep the narrative beat.
- **Auto-Complete Cleanse** — same, for Mission 2.
- **Subtitle Size** — 4 tiers (Small / Medium / Large / Huge).
- **Master / Music / SFX / Ambient volumes** — persisted via PlayerPrefs across sessions.
- **Tone Compass** — first-run choice between Gentle / Standard / Deep tones.

---

## 📂 What's in This Repo

| Path | Description |
|---|---|
| [`GAME_DESIGN.md`](./GAME_DESIGN.md) | Full game design document — vision, mechanics, market analysis, monetization, revenue projections (~$44.7M 3-year potential) |
| [`Docs/ARCHITECTURE.md`](./Docs/ARCHITECTURE.md) | Technical architecture — asmdef graph, service locator, save schema, mobile constraints, risk register |
| [`Docs/PROGRESS.md`](./Docs/PROGRESS.md) | Live progress log — current phase, decisions, known issues, next steps |
| [`Docs/Depth_Bible/`](./Docs/Depth_Bible/) | 16-codex deep design bible + 8-doc Mission 1-2 focus folder |
| [`Docs/Asset_Analysis_Mission1-2.md`](./Docs/Asset_Analysis_Mission1-2.md) | Detailed asset selection + integration plan for the 17 imported asset packs |
| [`CHANGELOG.md`](./CHANGELOG.md) | Versioned release history (currently 0.2.0 — polished Mission 1+2 playable) |
| [`Assets/_Project/Scripts/`](./Assets/_Project/Scripts/) | ~7k LOC across 10 asmdef-isolated subsystems (Core, Memory, Player, MiniGames, UI, Dialogue, Cutscene, Save, Mission, Audio) |
| [`Assets/_Project/Scenes/`](./Assets/_Project/Scenes/) | 6 Unity scenes built procedurally by the Phase 23 capstone |
| [`Assets/_Project/Yarn/`](./Assets/_Project/Yarn/) | 5 Yarn Spinner dialogue files (Doris M1, Gerrold M2, Marin notes, Pickle, Codex) |
| [`prototype.html`](./prototype.html) | The original HTML5 prototype that proved the design before Unity work began |

## ▶️ Run the HTML prototype (legacy)

1. Download or [view raw](./prototype.html) `prototype.html`
2. Open it in any modern browser — no Unity required.

The HTML prototype is preserved for reference but the Unity branch is the canonical playable.

## 🎯 The Market Gap

Cozy games oversaturate the **farming + relationships** axis. But cozy-narrative-investigation is wide open. *Spiritfarer* sold 2M+ on emotional cozy alone. *Strange Horticulture* sold 500k+ at 96% positive on tactile-shop investigation. **Nobody has fused these.**

| Signal | Evidence |
|---|---|
| r/CozyGamers (480k) | Top complaint: "Stardew clones all have the same farming loop" |
| Coral Island | 8M+ wishlists — proves cozy market scale |
| TikTok #CozyGames | 4.8B views — appetite for novelty is huge |

See [`GAME_DESIGN.md`](./GAME_DESIGN.md) §2 for full demand-signal analysis.

## 💰 Revenue Model

- **Base game $24.99** (Steam + Switch Day-1)
- **Mobile $9.99** with full content (huge LTV)
- **Free seasonal updates** + **paid story DLC** packs
- **No gacha, no energy, no P2W**
- **3-year projection: ~$44.7M gross / ~$26M net** (conservative)

## 🧭 Core Pillars

1. **Every villager is fully written** — no filler dialogue
2. **Tactile over textual** — memories are physical objects you hold
3. **Choices don't punish, they shape**
4. **Cozy framing, deep substance** — pace stays warm even in heavy moments
5. **A grand mystery underneath** — why did the previous Hollow-keeper vanish?

## 🏗️ Implementation Status

**Current version**: `0.2.0-mission-1-2-polished-playable` (PR #7 open)

| Stage | Status |
|---|---|
| Architecture, scripts, mini-games, save, UI | ✅ Complete |
| Asset-driven prefab builders (Phase 13-21) | ✅ Complete |
| Engineering playable Mission 1 (Phase 22) | ✅ Complete |
| **Polished playable Mission 1 + Mission 2 (Phase 23 + 24)** | ✅ **Complete — this branch** |
| 20-person greenlight playtest | ⬜ Next |
| Mission 3-10 + procedural villagers | ⬜ Post-greenlight |

See [`Docs/PROGRESS.md`](./Docs/PROGRESS.md) for the live ledger.

---

Part of the **Abdulmalek Agents** game-concept portfolio · 2026
