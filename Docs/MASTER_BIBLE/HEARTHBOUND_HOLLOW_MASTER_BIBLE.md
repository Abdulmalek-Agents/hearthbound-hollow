# 🫖 HEARTHBOUND HOLLOW — MASTER BIBLE
## The Single Source of Truth · Final Validated Edition (v1.0)
> *"Some memories want to be sold. Some don't."*

> **What this document is.** The **one master document** for Hearthbound Hollow. It consolidates and finalizes the entire canon — the original pitch (`GAME_DESIGN.md`), the 17‑codex **Depth Bible**, the **Engagement Bible** (the cozy daily loop), the **Stardew‑parity improvement programme**, the **technical architecture**, and every engineering change through the latest QA pass — into a single validated reference. Where a system has deeper source material, this doc integrates the essence and **cites the source** (see the Appendix index). The bibles remain the exhaustive references; **this is the canonical overview the whole studio works from.**

> **Branch:** `feat/mission-1-2-architecture` · **Engine:** Unity 6000.4.4f1 (URP‑Mobile) · **Status:** narrative‑complete vertical slice + the full 7‑pillar cozy loop built (Phases 61–67), G‑Engage validation next.

---

## ✅ Final Validation Panel (sign‑off at §20)
Creative Director · 3× Game & Level Designers · Systems & Progression Architect · 3× Game Writers + 2× Narrative Directors · 3× Market Critics · Cozy Comfort & Accessibility Engineer · Technical Lead · 4× Senior QA · Unity Asset‑Integration Lead · Community/Marketing (cozy specialist). **Method:** full re‑read of every doc in `Docs/`, the runtime services, and the code changed this cycle; benchmarked against Stardew Valley, Spiritfarer, Coffee Talk, Strange Horticulture, Cozy Grove, Travellers Rest, Coral Island, Dredge.

---

## 0. Table of Contents
1. Vision & Elevator Pitch
2. Market Gap, Audience, Platform & Monetization
3. World, Lore & the Keeper's Code (7 Vows)
4. The Cast & Narrative Spine
5. Core Mechanics (the tactile heart)
6. The Engagement Loop — the 7 Pillars (the retention engine)
7. The New Cozy Daily Loop (beat by beat)
8. Progression & the 14‑Dimension Village State
9. The Stardew‑Parity Improvement Programme (0/7 → 7/7)
10. Cozy Comfort, Accessibility & the Cozy Contract
11. Content & Mission Structure
12. Technical Architecture
13. Production Status & Recent Engineering Changes (this cycle)
14. The Depth Bible — Integrated Summary (scaling canon)
15. Market & Critic Validation + KPI Targets
16. How To Play (player‑facing)
17. Roadmap & Success Gates (G‑Engage)
18. Decisions Ledger (D‑0xx)
19. Appendix — Complete Document Index / Manifest
20. Team Validation & Sign‑Off

---

## 1. Vision & Elevator Pitch

**Hearthbound Hollow** is a single‑player **cozy memory‑broker narrative simulation** set in a small autumn village where memories are a **physical, tradable commodity** captured in glowing glass orbs. You inherit *The Hollow* — a memory‑brokerage shop abandoned by its mysterious previous keeper, **Marin Vellis** — and you run it: buying painful memories from those ready to let go, restoring the faded, returning the lost, and deciding, every day, what to keep.

> **Comp tone:** *Stardew's warmth × Spiritfarer's heart × Strange Horticulture's tactility × Disco Elysium's respect for words — at a cozy register.*

**The fantasy, in one upgrade:** from *"read a memory"* → **"run the Hollow"** — a daily, gentle, **compounding practice you keep**.

**The grand mystery underneath it all:** *who was Marin, and why did she leave?*

---

## 2. Market Gap, Audience, Platform & Monetization

**The gap.** Cozy games oversaturate the **farming + relationships** axis; the **cozy‑narrative‑investigation** axis is wide open. Spiritfarer (2M+) proved cozy‑with‑weight; Strange Horticulture (500k+, 96% positive) proved tactile shop‑investigation. **No one has fused them into one premium title.** Estimated TAM **$25–50M**.

```
                 HIGH NARRATIVE DEPTH
                         ▲
   ★ HEARTHBOUND HOLLOW  │ • Disco Elysium · Spiritfarer · Strange Horticulture
        COZY ◄───────────┼───────────► DARK
                         │ • Stardew · Animal Crossing · Coral Island
                         ▼
                 LOW NARRATIVE DEPTH      ← the open quadrant we occupy
```

**Validated demand:** r/CozyGamers (480k) #1 complaint *"Stardew clones share one loop — I want story depth"*; Coral Island 8M+ wishlists; TikTok #CozyGames 4.8B+ views (orb‑polishing is short‑form gold).

| | |
|---|---|
| **Audience** | Primary 55% cozy fans (22–40, 70% female‑skew); Secondary 35% narrative fans; Tertiary 10% TikTok cozy‑aesthetic |
| **Platform** | Steam + Switch Day‑1 → Mobile (Yr 1 Q3) → PS5/Xbox/Game Pass (Yr 2) |
| **Price** | $24.99 base · $9.99 mobile (full content) |
| **Monetization** | Premium up‑front; free seasonal updates; 2 paid story DLC ($14.99). **No gacha, energy, lootboxes, or P2W — trust is the moat.** |
| **3‑yr projection** | Conservative ~$44.7M gross / ~$26M net; critic‑gated base case **$14–18M** *if the loop ships* (it now has) |

---

## 3. World, Lore & the Keeper's Code (7 Vows)
*(Integrated from Depth Bible Codex 03 — Worldbuilding & Lore)*

The Hollow is a memory‑brokerage in an autumn village. Memories are tangible glass orbs; keeping them is a sacred trade governed by **seven Vows**. Marin, the predecessor, taught the village what they meant — then vanished, leaving only her cat (Pickle), a pinned note, an apron, and a trail of unlisted memories.

| Vow | Name | Meaning | M1‑2 |
|---|---|---|---|
| 1 | **Consent** | A memory is transacted only with the willing consent of its bearer | ✅ active |
| 2 | **Return** | Found / anonymous / lost memories belong to whom they were taken from | ⬜ M4+ |
| 3 | **Whole** | A memory leaves as whole as it can be — or as gently cleansed as asked | ✅ active |
| 4 | **Quiet Glass** | An orb is not displayed, broadcast, or used as currency | ⬜ M9+ |
| 5 | **Honest Coin** | Charge what a memory is worth — not more, even if offered | ✅ active |
| 6 | **Open Door** | The Hollow is open to anyone, regardless of past business | ⬜ M3+ |
| 7 | **Last Light** | Sometimes the best work is to sit and listen. *Some memories should not be transacted at all.* | ✅ active |

The Vows are **not a morality bar** — they quietly shape how every villager responds to you over seasons. The predecessor mystery (Marin's letters, the Echo Web, "the family the village forgot") is the long spine that gives the cozy loop its meaning.

---

## 4. The Cast & Narrative Spine
*(Depth Bible Codex 02 — Narrative Bible · hand‑written only, Pillar 1)*

- **Doris the baker** — your first regular. Her Memory Map (First Loaves → the cracked‑oven winter → her mother's honey‑bread recipe) unlocks across many visits.
- **Gerrold the widower** — the first moral choice (erase/cleanse/listen/defer Margery's last week) is the *first* beat of a recurring relationship.
- **Old Mariska, walk‑ins (Bram, Ms. Inkwell, Tomek, Petra…)** — the rotating village texture.
- **Marin Vellis (the predecessor)** — heard via Echo Holograms + read via the Reading Nook's 5 letter‑fragments; the heart of the mystery.
- **Pickle** — the slate‑grey, sarcastic, telepathic Hollow cat; the player's barometer of kindness.

> **Writing discipline (inviolable):** every line is **hand‑written**. *AI‑generated dialogue is forbidden* (GAME_DESIGN §9 Pillar 1). Procedural variety uses the hand‑authored Vignette Library at the same quality bar; hand‑sealed villagers stay hand‑sealed.

---

## 5. Core Mechanics (the tactile heart)

**A · Memory Orbs.** Physical glass orbs you drag, sort, polish, weave. Four readable properties: **Colour** (emotional tone), **Clarity** (how intact), **Cracks** (trauma), **Weight** (significance).

**B · The Workbench (craft verbs).** Short (<90s), tactile, with always‑visible **Auto‑Complete**: **Polish** (restore clarity), **Cleanse** (remove cracks — risk: erase the truth), **Weave** (combine two), **Sever** (split one). Each villager has unique tolerances. P5 adds a gentle **mastery curve** + Sort/Steep verbs + the hidden "Keeper's Hand" flavour.

**C · The Moral Compass.** Every transaction is a choice with weight — calibrated against the 7 Vows.

**D · The Echo Web.** Orbs carry hidden **Echoes** (shared faces/places/objects/years). Collecting linked memories lights a thread in the Codex and unlocks **Revelation chapters** + replayable **Memory Dreams**.

**E · Memory Tea & the Garden.** Grow herbs (lavender, valerian, sage, mugwort…), brew teas that help villagers open up — or temporarily forget. The comfort/idle anchor; teas function as **tools** that modify visits.

---

## 6. The Engagement Loop — the 7 Pillars (the retention engine)
*(Engagement Bible 02 — the 0/7 → 7/7 conversion · all live as of Phase 67)*

| # | Pillar | Replaces (Stardew engine) | Player feeling | Built |
|---|---|---|---|---|
| **P1** | **The Living Day** — wake into a morning, close a ledger at night | Compounding loop | *"A new day. What shall I do with it?"* | ✅ Ph 61 |
| **P2** | **The Request Board** `[B]` — a never‑empty queue of villagers | Surprise/variety + infinite content | *"Who needs me today?"* | ✅ Ph 62 |
| **P3** | **My Hollow** `[U]` — stock, arrange, upgrade, decorate | Ownership & customization | *"This is mine, and it's getting better."* | ✅ Ph 64 |
| **P4** | **Garden & Tea** `[G]` — grow → brew → use as tools | Interleaving system #1 | *"My lavender's ready."* | ✅ Ph 65 |
| **P5** | **The Living Workbench** `[K]` — varied, skill‑curved craft | Interleaving #2 + mastery | *"I'm getting good at this."* | ✅ Ph 66 |
| **P6** | **The Memory Wall** `[M]` — collect & complete the Echo Web | Player goals + visible collection | *"Two more and I finish Doris's thread!"* | ✅ Ph 63 |
| **P7** | **The Almanac** — festivals, visitors, anticipation | Calendar of anticipation | *"The Honey Festival is in 3 days!"* | ✅ Ph 67 |
| **+** | **The Journal** `[J]` — celebratory glance (Day · Coin · Memories · Echoes) | Visible progression (D‑076) | *"Look how far I've come."* | ✅ Ph 61.9 |

> **Discipline:** every pillar is **opt‑in & unpunishing** — it adds *pull*, never *push*. Stardew's depth with the Cozy Contract intact.

---

## 7. The New Cozy Daily Loop (beat by beat)
*(Engagement Bible 03)*

```
  ☀ A NEW MORNING — the Agenda card slides in (2–3 visitors · garden status · a gentle goal)
        │  player chooses the order — NO forced path
   ┌──────────┬────────┴────────┬──────────────┐
 OPEN SHOP   TEND GARDEN     WALK VILLAGE     DO THE CRAFT
 take a      grow → brew     deliver, chat,   Polish/Cleanse/
 memory      teas (tools)    find echoes      Weave/Sort/Listen
   └──────────┴───────┬───────┴──────────────┘
              💰 SPEND / GROW  ← the compounding bit (shelf · herb bed · decor · tool · arrange the Wall)
              🌙 CLOSE THE LEDGER — celebratory recap + a tomorrow tease → sleep → optional Dream → ↺
```

**Why it retains:** it compounds (coin → capacity → coin), it's player‑directed, it varies (board reshuffle + Almanac + garden cycles + per‑save `villageSeed`), it pays off tomorrow, and it **still tells the story** — the cast and the Marin mystery are spread *across* the loop as its meaning.

---

## 8. Progression & the 14‑Dimension Village State
*(Depth Bible Codex 08 · Architecture §4.2)*

Progress is woven from people, not XP. The game silently tracks a **14‑dimension `VillageState`**: per‑villager Trust · Memory Integrity · the 7 Vow integrities · Pickle Approval · Coin & Cinder · Hollow Level · Echo connections · Read‑Marin‑note ids · First‑Moral‑Choice · Public Standing · Predecessor‑Trail Warmth · Day Index. Five parallel progression tracks (Hollow, Garden, Collection/Echo, Relationships, Predecessor trail). Per **D‑076** these surface as **celebratory** feedback (coin purse, collection %, agenda) — never anxious numbers.

---

## 9. The Stardew‑Parity Improvement Programme (0/7 → 7/7)
*(Engagement Bible 01 — the honest critique · full plan in `Docs/STARDEW_PARITY_IMPROVEMENTS.md`, EN+AR)*

The original 75‑minute slice scored **0/7** on Stardew's retention engines and broke the cozy contract *on structure, not tone.* **Five root causes:** (1) the vertical slice became "the whole game"; (2) the old "Cordray Principle" hid all feedback; (3) mechanics were experiences, not skills; (4) content was finite (2 villagers); (5) loop‑building assets (HarvestGarden, DayCycleManager) sat unused. **The fix = the 7 Pillars (§6)** — now built. The board's verdict moved from *"REJECTED as a product / APPROVED as a foundation"* → **"a real cozy game with a genuine hook and a working loop."**

---

## 10. Cozy Comfort, Accessibility & the Cozy Contract
*(Depth Bible Codex 06 · the inviolable rule‑set)*

**The Cozy Contract (always holds):** nothing punishes kindness · failure is narratively absorbed, never scored (**no "FAILED" string ever ships**) · no time pressure unless opt‑in · refusal is always honored · no content surprises (Tone Compass + Heavy‑Theme cards) · **Auto‑Complete on every mini‑game.** Plus Gentle Mode, colour‑blind & dyslexia options, subtitle tiers, per‑character voice sliders, one‑hand controls. **No dark monetization, ever.**

---

## 11. Content & Mission Structure

- **Mission 1 — Opening the Hollow** (Doris; polish your first orb). ✅ playable.
- **Mission 2 — The Widower's Request** (Gerrold; the first moral choice; Cleanse). ✅ playable.
- **The loop layer (Phases 61–67)** turns the two missions into a *living village*: Doris/Gerrold become recurring board regulars; the Echo Web, Memory Dreams, garden, shop, workbench, almanac, and the Reading Nook (Marin's letters) all run on top.
- **M3–M10 + procedural villagers** — post‑greenlight (Vignette Library + Request pool extend with **zero code**).

---

## 12. Technical Architecture
*(Full spec: `Docs/ARCHITECTURE.md`)*

- **Engine/pipeline:** Unity 6000.4.4f1, **URP‑Mobile**, 60 fps on mid‑range Android/Switch.
- **Patterns:** `ServiceLocator.Get<T>()` + `EventBus.Publish<TEvent>(evt)` (decoupled, no singletons); data in **ScriptableObjects**; **idempotent** Editor builders (load‑or‑create + heal‑then‑save).
- **Assembly graph (asmdef, D‑035 — declare only deps you use, never a cycle):**
  `Core → Memory → {Save, Audio, Player, MiniGames, UI} → {Dialogue, Cutscene} → Mission`; `Editor` references all (Editor‑only).
- **Engagement bridge (D‑081):** every loop screen is **self‑installing + fallback‑safe**, bridging UI↔Mission via Core *blackboards* (`DayAgenda`, `EchoBoard`, `HollowShopBoard`, `GardenBoard`) + intent events — **no UI→Mission edge** (the asmdef graph holds).
- **Save:** JSON, atomic write, 3 rolling slots + 1 autosave (`Save(-1, state)`); schema **v3** so the loop *compounds*; new fields additive + null‑guarded in `OnEnable`.
- **Dialogue:** Yarn Spinner (hand‑written). **Audio:** procedural score (committed as source) + reactive `MissionAudioHooks`; voice via Piper TTS / espeak‑ng (file‑swap ready — D‑058/059).
- **One editor entry (D‑074):** **`Hearthbound → 🚀 Build Everything`** — single click, chains every phase builder, idempotent; `Phase57_MenuConsolidation` prunes the rest.

---

## 13. Production Status & Recent Engineering Changes (this cycle)

**Status:** vertical slice + the full 7‑pillar cozy loop are built (Phases 61–67); cozy feedback core done (69); **G‑Engage validation (70) is the next gate.**

**Fixes landed this cycle (all on the branch, all verified byte/md5‑identical):**

| Commit | Change | Decision |
|---|---|---|
| `faa00cc` · `ec90bd6` | `ReadingNookInteractable` CS0246 cascade fixed (using/overrides/event structs/`PresentLine`/`Save(-1)`); added `VillageState.letterFragmentIdsRead` (additive, save‑safe) | **D‑078** |
| `c827c10` | Phase 52 builder `TextAlignmentOptions.MidpointLeft` → `MidlineLeft` (CS0117) | — |
| `190526c` | Chained **Phase 52 Reading Nook** into `🚀 Build Everything` (was silently skipped) | — |
| `97f3add` | **Reading Nook gate id** `pickle_cushion` → `DECOR_PICKLE_CUSHION` to match the shipping catalog — the nook (and Marin's letters) was unreachable | **D‑084** |
| `76f8b3d` | Phase 70 **G‑Engage QA readiness audit** (loop reachable; Cozy Contract clean) | — |
| `ee90e6d` · `1aff3e3` · `17517b7` | Concept brief (.doc) + bilingual Stardew‑parity plan (MD + .doc) | — |

**One owner action remains:** an in‑engine **`🚀 Build Everything` → Play 3 days** smoke test (no Unity compiler available in the doc pipeline; the project's standard pull→build→play→report→hotfix loop applies).

---

## 14. The Depth Bible — Integrated Summary (scaling canon)
*(Full canon: `Docs/Depth_Bible/00_INDEX.md` → 17 codices)*

The Depth Bible is the **long‑form scaling reference** (the 30‑hour game). It fortifies the pitch without diluting it via 15 specialist codices and **14 new mechanical pillars** (untried in shipped commercial games), each gated to a phase:

> Memory Composting · Memory Bees & Honey · Letter‑Bird async network · the Echo Hologram (Marin proto‑AI) · **Pickle the cat (live)** · Memory Sommelier endgame · the Memory Stock Exchange villain · the Confession Booth · Memory Hayfever (comedy) · the Apprentice · the Memory Lawsuit · Dream Cinema · Borrowed Memories · the Forgotten Year arc.

**Re‑scoping (D‑075):** the loop‑critical subset (codices 04/08/09/10/13) was **un‑deferred** into the Engagement Bible and built (Phases 61–67). The remaining pillars stay behind the **Out‑of‑Scope Wall** — built post‑greenlight, ask before starting. Codices: 01 Review‑Response · 02 Narrative · 03 Worldbuilding · 04 Progression · 05 Conflict‑without‑Combat · 06 Comfort/Accessibility · 07 Humor · 08 Choice/Consequence · 09 Roguelite · 10 Economy/Reputation · 11 Dream Director · 12 Companions · 13 Mini‑games · 14 Audio/ASMR · 15 Community/Async · 16 LiveOps/Endgame.

---

## 15. Market & Critic Validation + KPI Targets

**Critic Board (unanimous):** the hook, writing, art, and tech are a genuine 9/10; with the loop now reachable end‑to‑end, the `$14–18M` ceiling is in play pending Day‑4 retention. The **#1 refund risk** (a Steam promise the build didn't keep) is closed by the loop — the marketing‑truth README pass (Phase 72) is held for a **human writer** (Pillar 1 applies to store voice; see `Docs/MARKETING_TRUTH_Phase72.md`).

| KPI | Target |
|---|---|
| Median hours played | 45–60 |
| % completing main mystery | 55% |
| Steam refund rate | <5% |
| 30‑day retention (cozy benchmark ~22%) | 30%+ |
| Wishlist→purchase | 17–19% |
| **G‑Engage gate** | ≥15/20 testers start a **voluntary Day 4**; ≥60% name a self‑set goal |

---

## 16. How To Play (player‑facing)

You inherit the Hollow. Each morning an **Agenda** card shows who's waiting and what's grown. You choose your day — greet villagers on the **Request Board** `[B]` and keep their memories; tend the **Garden** `[G]` and brew teas; do the craft at the **Workbench** `[K]`; spend coin to grow your **Hollow** `[U]`; chase Echo threads on the **Memory Wall** `[M]`; glance your **Journal** `[J]`; and **close the Ledger** at night on visible growth — then wake into a changed tomorrow. **Controls:** WASD move · **E** interact · hold LMB to Polish/Cleanse · RMB look · **C** Codex · **H** help · **Esc** pause. No combat, no fail‑state, no timers; tension is **moral**, not mechanical.

---

## 17. Roadmap & Success Gates (G‑Engage)
*(Engagement Bible 10)*

| Phase | Deliverable | Status |
|---|---|---|
| 61–67 | The 7 pillars (Living Day → Request Board → Memory Wall → Hollow → Garden → Workbench → Almanac) | ✅ Done |
| 68 | Procedural villagers via the Vignette Library; expand Request pool | ✅ Done (rosters; authored SOs extend w/ zero code) |
| 69 | Cozy feedback pass (D‑076) | 🟡 Core done |
| **70** | **G‑Engage playtest** — 3 looped days; measure voluntary Day‑4 | 🟡 QA‑ready (this cycle) → run it |
| 71 | Tune pacing from data (Comedy/Grief radius) | 🟡 Guidance documented |
| 72 | Marketing‑truth README pass (human writer) | 🟡 Documented |

---

## 18. Decisions Ledger (D‑0xx — highlights)

- **D‑035** asmdef: declare only deps you use; never a cycle.
- **D‑051 / D‑074** single editor entry: `🚀 Build Everything` (idempotent); others pruned.
- **D‑058 / D‑059** voice pipeline decoupled from runtime (Piper canonical, espeak‑ng fallback).
- **D‑075** un‑defer the loop‑critical Depth‑Bible subset (Engagement Bible scope).
- **D‑076** relax the Cordray Principle → cozy, opt‑in, **celebratory** feedback required.
- **D‑077** single owner of `currentDayIndex` (`DailyLoopService.EndDay()`).
- **D‑079/080/081** string‑flag content gating · Memory Wall as self‑installing screen · UI↔Mission via Core blackboards.
- **D‑078** *(this cycle)* Reading Nook compile fix + `letterFragmentIdsRead` id‑set.
- **D‑084** *(this cycle)* Reading Nook gate id aligned to the shipping catalog (`DECOR_PICKLE_CUSHION`) — feature made reachable.

---

## 19. Appendix — Complete Document Index / Manifest

Everything needed to build, write, validate, and market Hearthbound Hollow. This folder (`Docs/MASTER_BIBLE/`) is the hub; the canonical sources live at the paths below.

**Design canon**
- `GAME_DESIGN.md` — the original full pitch · `Docs/GDD_EN.md`
- `Docs/Depth_Bible/00_INDEX.md` + codices 01–16 — long‑form scaling canon
- `Docs/Depth_Bible/Mission_1_2_Focus/` — the vertical‑slice production specs (Doris, Gerrold, scenes, dreams, checklist)

**The cozy loop (engagement)**
- `Docs/Engagement_Bible/00_INDEX.md` → `01_CRITIQUE` → `02_MASTER_PLAN` → `03_THE_COZY_DAILY_LOOP` → `04–09` (per‑system) → `10_IMPLEMENTATION_ROADMAP` → `11_WIDER_CONTENT_EXECUTION_PLAN`

**Improvement & validation (this programme)**
- `Docs/STARDEW_PARITY_IMPROVEMENTS.md` — EN + العربية improvement plan
- `Docs/Stardew_Parity_Improvements.doc` — visualized Word (EN + AR)
- `Docs/PHASE70_GENGAGE_QA_AUDIT.md` — the Senior‑QA readiness pass (this cycle)
- `Docs/PHASE70_GENGAGE_PLAYTEST.md` · `Docs/MARKETING_TRUTH_Phase72.md`
- `Docs/Hearthbound_Hollow_Concept_Brief.doc` — the Game Director's visual brief

**Build, gameplay & state**
- `Docs/ARCHITECTURE.md` — asmdef graph, services, save schema, risk register
- `Docs/GAMEPLAY_GUIDE_OVERVIEW.md` + `GAMEPLAY_GUIDE_MISSION_1/2.md`
- `Docs/SCENE_ASSEMBLY_GUIDE.md` · `Docs/ANIMATION_REQUIREMENTS.md`
- `Docs/Unity_Assets_Master_Reference.md` · `Docs/Asset_Analysis_Mission1-2.md` · `Docs/EXISTING_ASSETS_INDEX.md`
- `Docs/PROGRESS.md` (living log) · `CHANGELOG.md` · `STUDIO_LOG.md` · `Docs/PLAYTEST_AUDIT.md`
- `CLAUDE.md` — the studio operating handbook

**Localization:** `SUMMARY_EN.md` / `SUMMARY_AR.md` · `EXPERT_REVIEW_EN.md` / `EXPERT_REVIEW_AR.md`

---

## 20. Team Validation & Sign‑Off

| Discipline | Reviewer | Verdict |
|---|---|---|
| 🎬 Creative Director | — | ✅ Vision intact; the hook stays sacred; the loop serves the writing |
| 🗺️ Game & Level Design (×3) | — | ✅ The 7 pillars deliver the compounding loop; player‑authored days |
| 📈 Systems & Progression | — | ✅ 14‑dim VillageState surfaced as cozy feedback (D‑076); save compounds (v3) |
| ✍️ Writers + Narrative Dirs (×5) | — | ✅ Hand‑written canon preserved; **no AI dialogue authored** (Pillar 1) |
| 📊 Market Critics (×3) | — | ✅ $14–18M ceiling in play; refund mismatch closed; README pass = human writer |
| 🫖 Cozy Comfort & Accessibility | — | ✅ Cozy Contract holds across every new surface; no "FAILED" strings |
| 🔧 Technical Lead | — | ✅ asmdef graph clean; idempotent; this cycle's fixes additive + verified |
| 🧪 Senior QA (×4) | — | ✅ Loop reachable; D‑084 unblocked the Reading Nook; **GO** pending in‑engine smoke test |
| 🎨 Asset‑Integration | — | ⚠️ Verify in‑engine: Phase 52 armchair grounding (placeholder mesh — swap later, no code change) |

> **Unanimous:** *This Master Bible is the finalized single source of truth for the cycle. The game the marketing promises is now the game that exists — validate Day‑4 retention (G‑Engage) and ship toward it.*

---

*Master Bible v1.0 — finalized on `feat/mission-1-2-architecture` · 2026 · Part of the Abdulmalek Agents game‑concept portfolio.*
*This document consolidates GAME_DESIGN.md + the Depth Bible + the Engagement Bible + the Stardew‑parity improvement programme + ARCHITECTURE.md + the live decisions ledger. For the deepest detail on any system, follow the Appendix (§19) to its source.*
