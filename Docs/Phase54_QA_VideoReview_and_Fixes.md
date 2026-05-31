# 🎥 Phase 54 — QA Video Review, Docs-vs-Build Gap Analysis & Fix Program

> **Trigger:** User request — assign 20+ senior specialists, read all docs + `Docs/`
> on `feat/mission-1-2-architecture`, watch `Docs/Gameplay video testing .mp4`,
> compare **design docs vs. what is actually implemented**, make it "global-hit"
> quality, fix everything found (especially the **end-of-video freeze**), add
> onboarding emoji in TextMesh Pro, make the tutorial cleaner per current PC-hit
> standards, enrich the environment from available assets with accurate placement,
> and push every phase to the same branch — then do a second review pass.
>
> **Engine:** Unity 6000.4.4f1 · **Pipeline:** URP-Mobile · **Target:** PC (Win64/Steam).
> **Branch:** `feat/mission-1-2-architecture`.

---

## 0. How this review was conducted

- **Repo cloned** in full (25,222 files, ~14 GB incl. LFS). The gameplay video is a
  Git-LFS object (374 MB, 2560×1440 @ 60 fps, 4:04 long) — pulled via `git lfs`.
- **Video forensics:** extracted ~100 frames (overview every 4 s + dense 1 fps over
  the final 40 s) into contact sheets and read them frame-by-frame. The end-of-video
  freeze was isolated to the exact UI layers on screen.
- **Code forensics:** traced the full end-of-day → night → scene-transition chain
  through the actual C# + the `03_Mission01_Hollow.unity` scene YAML + the
  `UI_EveningLedger_Bamao.prefab` serialized fields, plus the title-card / dream /
  mission-SO wiring. Root causes were confirmed against serialized data, not guessed.

### Specialists convened (24)

| Discipline | Named roles on this pass |
|---|---|
| **Lead** | Lead Unity Architect (owns the night-chain contract + zero-regression gate) |
| **Engineering** | 4× Senior Unity Devs, 3× C# Scripters, 1× Build/DevOps, 1× Package/asmdef Expert |
| **UI / UX** | UX/UI Designer, 2× 2D/UI Artists (TMP emoji + tutorial layout) |
| **Cinematic** | 1× Cutscene Director, 2× Camera Experts (dialogue framing), 1× Lighting Expert |
| **Design & Narrative** | 2× Game Designers, 2× Writers, 1× Narrative Director |
| **Quality + Environment** | 4× Senior QA, 1× Technical Artist, 2× 3D Modelers (placement pass) |
| **Market & Community Critics** | 3× Market Critics (Stardew / Spiritfarer / Strange Horticulture benchmarks) |

---

## 1. What the video shows (timeline)

| Time | Beat | Observation |
|---|---|---|
| 0:00 | Main menu | "Hearthbound Hollow" · *"Tea opens the way to talk."* · Open The Hollow / Quit. Very bare. |
| 0:04 | Character creator | "Who Will Keep The Hollow?" — name, skin, outfit, accessory (None/Cap/Flower/Scarf), Surprise me, Begin. Layout slightly clipped at the Accessory row. |
| 0:12 | Hollow interior | Player avatar is a **bald, barefoot grey-tank placeholder** (uncanny, not cozy). Stone-tile interior, lantern, counter, barrels. |
| 0:16–0:48 | Free-roam | Camera **clips through walls / the avatar's face**; an **oversized yellow fire VFX** floats mid-air; **red glowing floor quads** (placeholder interaction markers); a **floating yellow "note" sprite** hangs in the air. Scene is sparse + dark. |
| 0:48–3:00 | Doris dialogue + Polish | Dialogue + "Polish the memory" mini-game (0→60→78→100 %, Skip·Auto-Complete present). Mini-game tutorial overlay reads well. **Dialogue camera never frames Doris** — it stays jammed against the counter/wall showing the player side-on. |
| 3:00–3:24 | Evening Ledger | "Day 1" summary, First Loaves, 35 c, Save Slots, **Sleep — End Day**. World HUD (`Day 1`, `35 c`, "a note in hand" prompt) is **still visible behind the ledger**, and the player can still move. |
| **3:24–4:04** | **FREEZE** | After **Sleep — End Day**: the **ledger never disappears**. The Dream-1 prose ("The kitchen at first light…") and the **Mission Title Card** ("Day 2 / Opening the Hollow / Warm, slightly dusty…") play **behind** the still-present ledger. The player toggles **Help (H)** repeatedly trying to escape. The game never advances to the next phase. |

---

## 2. ROOT CAUSE of the end-of-video freeze (confirmed) — `D-069`

Three compounding defects; #1 is the trigger and a hard ship-blocker.

### 2.1 `EveningLedgerUI.Hide()` was a silent no-op (the trigger)
In `UI_EveningLedger_Bamao.prefab` the serialized `root` field points at the **same
GameObject** the `EveningLedgerUI` component is on:

```
m_GameObject: {fileID: 4649231483332946608}
root:         {fileID: 4649231483332946608}   # root == gameObject
```

The old guard made Hide do nothing:
```csharp
public void Hide() { if (root != null && root != gameObject) root.SetActive(false); } // root==gameObject ⇒ no-op
```
So once shown, the ledger could **never close**. Pressing "Sleep — End Day" ran
`Hide()` (no-op) then `OnEndOfDayConfirmed`; the night beats played behind a
full-screen panel whose GraphicRaycaster kept eating clicks, so the "Goodnight"
button on the `OneMoreDayCard` could not be reached → the chain never reached
`LoadScene(...)`. Re-clicking the still-present "Sleep — End Day" re-entered the
night chain (no single-fire guard), racing the sequence.

**Proof we never left the Hollow scene:** the title card at 3:30 reads
*"Opening the Hollow"* (the Mission **01** SO). Mission 02 ("The Widower's Request")
would have shown a different title, and the Day-1 ledger lives on the Hollow's
non-persistent `UI_Canvas` (it would have been destroyed by a real load). Both facts
prove the load never visibly happened — the player was stranded in scene 03.

### 2.2 The night chain could hang and had no safety net
`EndOfDaySequencer.RunSequence` waited on `MemoryDreamSequencer.OnDreamFinished` with
an **unbounded** `while (!finished) yield return null;`. A Timeline that is paused
(`Time.timeScale == 0`, e.g. while an overlay is open) or otherwise never raises
`PlayableDirector.stopped` would hang the whole chain forever. The final transition
(`EndDay()` + `LoadScene`) was also unguarded.

### 2.3 `GameManager.LoadScene` failed silently on a missing scene
`SceneManager.LoadSceneAsync` returns `null` (logs once, returns) if a scene is not in
Build Settings — leaving the player frozen with no fallback.

---

## 3. Phase 54 fixes shipped (Phase A — pushed to the branch)

| File | Fix | Commit |
|---|---|---|
| `Scripts/UI/EveningLedgerUI.cs` | `Hide()` now closes the panel in **every** layout (CanvasGroup α=0 + blocksRaycasts off, self-deactivate when `root==gameObject`); auto-creates a CanvasGroup; **single-fire `_confirmed` guard**; buttons freeze on confirm; `Show()` re-arms. | `3510108` |
| `Scripts/Mission/EndOfDaySequencer.cs` | **Dream watchdog** (configurable, default 30 s) so a stalled/paused Timeline can never hang the night chain; **try/catch** around the transition with a safe main-menu fallback. | `5173071` |
| `Scripts/Core/GameManager.cs` | `LoadScene` validates `Application.CanStreamedLevelBeLoaded` **before** tearing down and falls back to the main menu so the player is never stranded. | `ff880f0` |
| `Scripts/Mission/MissionRunner.cs` | Single-fire end-of-day guard for the Mission 2 scenes; hides the ledger up front. | `b95bb4e` |
| `Scripts/Mission/Mission01Director.cs` | (Phase C bundle) single-fire guard, lock player through the night/transition, lock movement while the modal ledger is open. | _staged_ |

**Net effect:** "Sleep — End Day" now immediately tears down the ledger, the
Dream → Goodnight → next-scene chain runs unobstructed, and the day advances. Even if
a future Timeline stalls or a scene is mis-wired, the player is returned to the menu
instead of frozen. The "stuck at the end of the video" bug is resolved at the root.

---

## 4. Docs-vs-Implementation gap analysis (all aspects)

Legend: ✅ matches design · 🟡 partial / needs polish · 🔴 diverges / bug · 🆕 added by this program.

### 4.1 Core loop & systems
| Design intent (GDD / Guides / Depth Bible) | In build | Verdict |
|---|---|---|
| Memory-broker loop: greet → offer → price → **Polish** → ledger → sleep | Present, end-to-end in M1 | ✅ |
| Polish mini-game: hold LMB, slow circles, cover 4 sides, Auto-Complete always | Present + on-screen tutorial overlay + friction hints | ✅ |
| Cozy Contract: no fail screens, refusal honored, no visible numbers in emotional UI | Honored in dialogue/ledger; Polish shows a **%**, acceptable as a skill read | ✅/🟡 |
| Day-end → night → "One More Day" goodnight → next day | Implemented but **frozen by D-069** | 🔴→✅(fixed) |
| Save slots 1/2/3 + Autosave at ledger | Present | ✅ |

### 4.2 Presentation & game-feel
| Item | In build | Verdict |
|---|---|---|
| Dialogue camera frames the speaker (over-shoulder, Witcher/Spiritfarer feel) — `DialogueCameraDirector` | Director auto-spawns, but **no scene object is named "Doris"** so speaker lookup fails → wide/narrator fallback jams against the counter. | 🔴 (Phase C) |
| Player avatar = cozy villager | **Bald, barefoot grey placeholder** — uncanny; `CharacterAppearance` tints exist but base mesh/material read as a test dummy | 🔴 (Phase C/E) |
| Interaction markers are diegetic (soft golden prompts per the guides) | **Red glowing floor quads** still present (placeholder emissive) | 🔴 (Phase D) |
| Hearth/torch fire VFX scaled + grounded | **Oversized yellow flame floating** in places | 🔴 (Phase D) |
| "Note in hand" pickup is a real prop | Reads as a **floating yellow sprite** in the air | 🟡 (Phase D) |
| Camera never clips world/player | **Clips through walls + the avatar's face** in free-roam | 🟡 (Phase C) |
| Interior dressing matches "warm, lived-in bakery/hollow" | **Sparse + dark**; lots of empty stone floor | 🟡 (Phase D) |

### 4.3 Onboarding / tutorial (current PC-hit standard)
| Item | In build | Verdict |
|---|---|---|
| First-run walkthrough (`OnboardingOverlay`) with key chips | Exists (6 steps, pauses game) | ✅ |
| Emoji on the cards render correctly in **TextMesh Pro** | Headlines use raw unicode 🪔🚶✋✨🕯🍂 but **TMP has no emoji fallback/sprite asset** (`m_fallbackFontAssets: []`) → they render as **tofu boxes ▯** | 🔴 (Phase B) |
| Tutorial copy is concise + action-first | Wordy in places; mixes concepts per card | 🟡 (Phase B) |
| Always-on context hints (`ControlHintsHUD`) | Present; also uses raw emoji (same tofu risk) | 🟡 (Phase B) |
| On-demand controls (Help / `H`) | Present, clear | ✅ |

### 4.4 Title cards / scene identity
| Item | In build | Verdict |
|---|---|---|
| Each scene shows its own title | Cards are wired to the correct Mission SO (M1 scenes → "Opening the Hollow", M2 → "The Widower's Request"); the end-of-video "Opening the Hollow" was only because we **never left scene 03** | ✅ (once D-069 fixed) |

---

## 5. Fix program & phase plan (pushed incrementally to the branch)

- **Phase A — End-of-day freeze (D-069).** ✅ Pushed (§3).
- **Phase B — Onboarding emoji in TMP + tutorial clarity.** Runtime emoji helper +
  idempotent Editor builder that generates a TMP sprite asset & registers an emoji
  fallback, plus tighter, action-first copy. 🟢
- **Phase C — Dialogue camera framing + free-roam clip + avatar legibility.** Register
  speakers with `DialogueCameraDirector`; widen the narrator fallback so it can't jam;
  ship the staged `Mission01Director` polish. 🟢
- **Phase D — Environment enrichment + placement accuracy.** Idempotent builder that
  dresses the Hollow interior from packs already in the project (grounded, rotated,
  collider'd), retires the red placeholder quads to diegetic prompts, and clamps the
  oversized fire VFX. Joint QA + Tech-Artist + 3D-Modeler grounding pass. 🟢
- **Round 2 — Re-review** every phase against this document + the Cozy Contract.

All player-facing copy keeps the Cozy Contract (no "FAILED", no punishment, refusal
honored) and the asmdef graph (UI ⇏ Player/Mission; Audio ⇏ UI). Dialogue prose stays
hand-written (no AI-generated dialogue — GAME_DESIGN §9 Pillar 1).

---

*Maintained by the Hearthbound Hollow virtual studio. See `STUDIO_LOG.md` for the
running phase log and `ARCHITECTURE.md` for decision `D-069`.*
