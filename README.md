# 🫖 Hearthbound Hollow

> *"Some memories want to be sold. Some don't."*

A **single-player cozy narrative simulation** set in a small autumnal village where memories are a physical, tradable commodity. You inherit a memory-brokerage shop. Buy painful memories from villagers wanting to forget. Sell preserved ones to those who long to remember. Polish, weave, and restore the fragmented — but choose carefully what you keep.

**Pitch in one line:** *Stardew's warmth × Spiritfarer's heart × Strange Horticulture's puzzles × Disco Elysium's writing — at cozy scale.*

---

## 🎮 Run the polished playable Mission 1 + 2 (Unity)

The `feat/mission-1-2-architecture` branch ships a fully playable, polished vertical slice of Missions 1 and 2 — six scenes, two villager arcs, four moral choices, two memory dreams, a complete cozy-comfort/accessibility layer, **a robust WASD + sprint + jump controller, a smooth third-person follow camera, a Mixamo-ready Humanoid Animator, Doris/Gerrold animated dialogue beats, a 6-step first-play OnboardingOverlay, and a persistent context-aware ControlHintsHUD.**

### One-click build

1. Clone the repo and check out `feat/mission-1-2-architecture`.
2. Open the project in **Unity 6 LTS (6000.4.4f1)**. Packages auto-install (~30–90 s).
3. Menu → **`Hearthbound → ✨ Build EVERYTHING (Phase 27 — one click)`** — sit back ~60 s.
4. Press **Play**.

That single menu item runs every build phase — polished scenes, player AnimatorController, NPC AnimatorController, narrative hooks, Player Rig Doctor (foot-bone anchor), and the OnboardingOverlay + ControlHintsHUD — sets up Build Settings, and opens the Bootstrap scene. Reflection-driven so missing phases skip gracefully.

> 💡 Want richer animation? Drop 6 Mixamo FBXs into `Assets/_Project/Animations/Mixamo/` per [`Docs/ANIMATION_REQUIREMENTS.md`](./Docs/ANIMATION_REQUIREMENTS.md) § 3 and re-run Phase 27. The game ships polished without them, but Run / Jump / Fall / Land states get real motion — and a `Talking.fbx` clip upgrades the NPC dialogue body language.

> 🔍 Verify wiring any time with **`Hearthbound → 🔍 Diagnose Phase 26 Build`** — read-only audit, no scenes modified.

### Player-facing flow

```
Bootstrap → Main Menu (Tone Compass on first run) →
Lane (autumn dusk, 6-step OnboardingOverlay on first play) → walk to the Hollow door →
Hollow → meet Doris (she visibly turns to face you and talks) → choose a price →
polish her First Loaves orb → read Marin's Note on the workbench →
Evening Ledger → Garden (Day 2) → harvest herb → brew tea →
Cottage → meet Gerrold (turns toward you when he speaks) →
moral choice (Erase / Cleanse / Listen / Defer) →
(optional Cleanse mini-game) → Dream 2 cutscene →
Evening Ledger → Main Menu
```

### Controls (visible any time via the H key, also on-screen via the ControlHintsHUD)

| Action | Key / Stick |
|---|---|
| Move | WASD / Arrows / Gamepad left stick |
| **Sprint** | **Left Shift / Gamepad LStick click** (off in Gentle Mode) |
| **Jump** | **Space / Gamepad south** (off in Gentle Mode) |
| Interact | E / Gamepad ▢ |
| Advance dialogue | Click / Space / Enter |
| Polish orb | Hold left mouse, draw slow circles |
| **Camera look** | **Hold Right Mouse + drag / Gamepad right stick** |
| **Camera zoom** | **Mouse scroll / Gamepad LB-RB** |
| Pause | Escape |
| Help / Controls card | H |

### Comfort & accessibility (in Settings panel + Pause menu)

- **Gentle Mode** — longer timers, no fail states, more forgiving cleanse tolerance, and **disables sprint + jump** for a pure cozy walk.
- **Auto-Complete Polish** — skip the mini-game, keep the narrative beat.
- **Auto-Complete Cleanse** — same, for Mission 2.
- **Subtitle Size** — 4 tiers (Small / Medium / Large / Huge).
- **Master / Music / SFX / Ambient volumes** — persisted via PlayerPrefs across sessions.
- **Tone Compass** — first-run choice between Gentle / Standard / Deep tones.
- **OnboardingOverlay** — 6-step cozy walkthrough on first play (Welcome → WASD → E → LMB polish → Esc/H comfort → Ready). Skippable from frame 1; per-save flag so it never repeats.
- **ControlHintsHUD** — always-visible parchment chip strip (Move · Interact · Help) at the bottom-left of every gameplay scene. The [E] chip lights up to full alpha + shows the interactable's prompt when one is in range.

---

## 📂 What's in This Repo

| Path | Description |
|---|---|
| [`GAME_DESIGN.md`](./GAME_DESIGN.md) | Full game design document — vision, mechanics, market analysis, monetization, revenue projections (~$44.7M 3-year potential) |
| [`Docs/ARCHITECTURE.md`](./Docs/ARCHITECTURE.md) | Technical architecture — asmdef graph, service locator, save schema, mobile constraints, risk register |
| [`Docs/PROGRESS.md`](./Docs/PROGRESS.md) | Live progress log — current phase, decisions, known issues, next steps |
| [`Docs/ANIMATION_REQUIREMENTS.md`](./Docs/ANIMATION_REQUIREMENTS.md) | Player Animator graph, clip roster, Mixamo download + Humanoid retargeting guide |
| [`Docs/SCENE_ASSEMBLY_GUIDE.md`](./Docs/SCENE_ASSEMBLY_GUIDE.md) | Per-scene build steps + the ⚡ Fast path (Phase 27 one click) |
| [`Docs/Depth_Bible/`](./Docs/Depth_Bible/) | 16-codex deep design bible + 8-doc Mission 1-2 focus folder |
| [`Docs/Asset_Analysis_Mission1-2.md`](./Docs/Asset_Analysis_Mission1-2.md) | Detailed asset selection + integration plan for the 17 imported asset packs |
| [`CHANGELOG.md`](./CHANGELOG.md) | Versioned release history (currently **0.5.0** — Phase 28 / 29 / 30 trifecta) |
| [`Assets/_Project/Scripts/`](./Assets/_Project/Scripts/) | ~10k LOC across 10 asmdef-isolated subsystems (Core, Memory, Player, MiniGames, UI, Dialogue, Cutscene, Save, Mission, Audio) |
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

**Current version**: `0.5.0-onboarding-hints-and-rig-doctor` (PR #7 open)

| Stage | Status |
|---|---|
| Architecture, scripts, mini-games, save, UI | ✅ Complete |
| Asset-driven prefab builders (Phase 13-21) | ✅ Complete |
| Engineering playable Mission 1 (Phase 22) | ✅ Complete |
| Polished playable Mission 1 + Mission 2 (Phase 23 + 24) | ✅ Complete |
| UI activation hotfix (Phase 25) | ✅ Complete |
| Player Controller + Animation pipeline (Phase 26) | ✅ Complete |
| Narrative Hooks (Marin's Note, Phase 26 parallel thread) | ✅ Complete |
| Build EVERYTHING master capstone + NPC Animator + diagnostic + footstep hooks (Phase 27) | ✅ Complete |
| **"Half body in floor" definitive fix — live world bounds + continuous correction window (Phase 28)** | ✅ **Complete** |
| **UI Polish — UIAutoFitText on every TMP label + ChoicesContainer relocated inside dialogue box (Phase 29a)** | ✅ **Complete** |
| **Player Rig Doctor — foot-bone anchor auto-discovery + Animator sanity pass (Phase 29b)** | ✅ **Complete** |
| **OnboardingOverlay (6-step walkthrough) + ControlHintsHUD (always-visible context-aware key chips) (Phase 30)** | ✅ **Complete — this branch** |
| 20-person greenlight playtest | ⬜ Next |
| Mission 3-10 + procedural villagers | ⬜ Post-greenlight |

See [`Docs/PROGRESS.md`](./Docs/PROGRESS.md) for the live ledger.

### What the **`Hearthbound → ✨ Build EVERYTHING`** capstone does in one click

Six sub-capstones, ~60 s end-to-end, idempotent and reflection-driven:

1. **Phase 23** — POLISHED Mission 1 + 2 scene assembly (chains 13-24).
2. **Phase 26 (PC + Anim)** — Player AnimatorController + SmoothFollowCamera + cameraReference + PlayerGroundClamp + Mixamo-ready 1D blend tree.
3. **Phase 26 (NPC)** — Doris/Gerrold/SilentLane animators + NpcAnimatorBridge for "IsTalking" body language.
4. **Phase 26 (Narrative Hooks)** — Marin's Note dropped on the Hollow workbench.
5. **Phase 29 (Rig Doctor)** — auto-discovers a foot bone on the Player rig and wires it as `PlayerGroundClamp.footAnchor` (most surgical "stand on the floor" fix).
6. **Phase 30 (Onboarding + Hints)** — OnboardingOverlay on Lane (6-step first-time walkthrough) + ControlHintsHUD on every gameplay scene (always-visible parchment chips: Move · Interact · Help).

---

Part of the **Abdulmalek Agents** game-concept portfolio · 2026
