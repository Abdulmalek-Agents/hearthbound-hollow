# Mission 1-2 · Focus 08 — **Production Checklist**
### Asset bindings · Script architecture · QA plan · The Mission 1-2 vertical-slice greenlight criterion
### Owner: Halvor Krieg (Risk & Quality Auditor)

> *This is the final document of the Mission 1-2 Focus folder. It binds everything in Focuses 00–07 to **the work that gets done** by the engineering, art, audio, writing, and QA teams. It also defines the **greenlight criterion** that gates the team's progress from Mission 1-2 into Mission 3+.*

---

## 1.0 The 23 Imported Assets, Mapped to Mission 1-2 Scenes

Per `Docs/Asset_Analysis_Mission1-2.md` § 5–6, these 23 assets are imported. Below is **which of the 23 actually gets used in Mission 1-2** and which sit dormant for later missions.

| # | Asset | M1-2 use | Status |
|---|---|---|---|
| S-1 | Medieval Village Megapack | Lane scene + Hollow exterior + cottage exterior | **Used heavily** |
| S-2 | Harvest Garden | Herb garden in M2 (lavender, valerian, watering can, raised beds) | **Used (subset)** |
| S-3 | Heat – Complete Modern UI | Main menu, loading screen, settings, pause, HUD | **Used heavily** |
| S-4 | Bamao Pack: Fantasy GUI | Dialogue boxes, Evening Ledger, choice card, Codex tooltips | **Used heavily** |
| S-5 | All In 1 Shader Nodes | `MemoryOrb_Master` shader | **Used heavily** (most important shader) |
| S-6 | Lumen: Stylized Light FX 2 | God rays, candle glow, lantern halos | **Used heavily** |
| S-7 | Cutscene Engine | Opening cinematic + Memory Dream 1 + Memory Dream 2 (5 variants) + Mission 2 outro | **Used heavily** |
| S-8 | Game UI & Puzzle SFX Pack | All UI + mini-game audio | **Used (subset — ~80 of 500+ sounds)** |
| S-9 | Hierarchy Designer | Project hierarchy organization | **Used (workflow only)** |
| S-10 | Asset Inventory 4 | Track + tag all imports | **Used (workflow only)** |
| A-11 | City Characters | (not chosen — BoZo selected instead) | **NOT USED** |
| A-12 | BoZo Stylized Modular Characters | Doris (Cleric reskin), Gerrold (Bard reskin), silent villager (Warrior reskin) | **Used** |
| A-13 | Character Controller Pro | Player walk-only controller | **Used** |
| A-14 | Eyes Animator | Doris's + Gerrold's + Pickle's eye animation | **Used** |
| A-15 | Animation Composer System (ACS) | Doris kneading, Gerrold's hands, silent villager reading | **Used** |
| A-16 | Stylized Weather System | Light fog on lane + light rain on walk to cottage | **Used (subset — only 2 weather states)** |
| A-17 | Zephyr: Dynamic Wind System | Foliage wind on lane trees + sign/curtain animation | **Used** |
| A-18 | VoluSmokeFX | Chimney smoke + kettle steam + Polish/Cleanse particles | **Used** |
| A-19 | Skill Tree / Talent Tree Builder | Memory Map (4-node Doris map + first Echo Web connection) | **Used (M1-2 floor — full Memory Map system M3+)** |
| A-20 | OpenAI Dialogue Addon | **DO NOT USE** (Asset Analysis directive) | **Tagged `Reference – Do Not Use In Build`** |
| A-21 | Colorize | Hair/clothes variants on procedural NPCs + autumn material shift on lane foliage | **Used (light)** |
| A-22 | LightMap Fusion Pro | Lightmap sets per scene (4 sets total) | **Used** |
| A-23 | Microdetail Terrain System | Cobblestone path + dirt road close-up detail | **Used (light)** |

### 1.1 Net asset usage

- **20 of 22 imported assets** (excluding A-11 unused and A-20 do-not-use) are actively used in Mission 1-2.
- The 22nd asset (Yarn Spinner) is not from the imports — it's installed separately via Package Manager.
- **No Tier B or Tier C assets** are used in Mission 1-2.

This is the **Asset Analysis Mission1-2 § 12 Critic Board directive** in execution: lean, focused, no project bloat.

---

## 2.0 The C# Scripts the Engineering Team Must Write

| # | Script | Responsibility | Time estimate |
|---|---|---|---|
| 1 | `GameManager.cs` (singleton) | Bootstrap, scene transitions, service locator | 12 hrs |
| 2 | `VillageState.cs` (ScriptableObject) | Full 14-dimension state struct (per Codex 08 § 2) | 14 hrs |
| 3 | `VillagerSO.cs` (ScriptableObject) | Per-villager identity + Trust + Memory Integrity | 6 hrs |
| 4 | `MemoryNodeSO.cs` (ScriptableObject) | The Memory Card schema (per Codex 02 § 2.1) | 10 hrs |
| 5 | `MemoryConnectionSO.cs` (ScriptableObject) | Echo Web connections | 4 hrs |
| 6 | `VillagerMemoryMapSO.cs` (ScriptableObject) | The Memory Map per villager | 6 hrs |
| 7 | `MemoryHerb.cs` (ScriptableObject) | Lavender + Valerian effects | 4 hrs |
| 8 | `MissionSO.cs` (ScriptableObject) | Mission definitions + objectives + outro hooks | 6 hrs |
| 9 | `PolishMiniGame.cs` (MonoBehaviour) | Polish input + shader update + difficulty + autocomplete (Focus 04 § 2) | 36 hrs |
| 10 | `CleanseMiniGame.cs` (MonoBehaviour) | Cleanse input + crack-trace + core-region + outcome states (Focus 04 § 3) | 48 hrs |
| 11 | `YarnVillageStateBridge.cs` | Yarn ↔ VillageState bridge (Focus 07 § 2.2) | 14 hrs |
| 12 | `YarnCustomCommands.cs` | The ~14 custom commands (Focus 07 § 2.3) | 12 hrs |
| 13 | `PickleAI.cs` | Pickle's daily behaviors (sleep locations, head turns, tail flicks, Approval-driven) | 16 hrs |
| 14 | `EveningLedgerUI.cs` | The Ledger UI flow (Focus 07 § 4) | 24 hrs |
| 15 | `SaveService.cs` | The 3-rolling-slot + autosave system (Focus 07 § 4.5) | 16 hrs |
| 16 | `ComfortToolsMenu.cs` | The full menu (Focus 07 § 6) | 24 hrs |
| 17 | `ToneCompassCard.cs` | The first-launch 90-second card (Focus 07 § 7) | 8 hrs |
| 18 | `MemoryDreamSequencer.cs` | Variant-switching for Dream 2 (Focus 05 § 3.9) | 6 hrs |
| 19 | `DayCycleManager.cs` | Time-of-day + lightmap blending | 14 hrs |
| 20 | `EchoWebVisualizer.cs` | First-time connection animation (Focus 02 § 3.4) | 12 hrs |
| 21 | `CodexUI.cs` | Examine-prop tooltips + Memory Map view | 28 hrs |
| 22 | `TeaBrewingUI.cs` | The kettle interaction (Focus 06 § 3) | 14 hrs |
| 23 | `HollowDoorInteractable.cs` | The door's interaction logic | 4 hrs |
| 24 | `ChoiceCardUI.cs` | The 4-option choice screen (Focus 02 § 5) | 18 hrs |
| 25 | `ListenSceneSequencer.cs` | The 3-minute Listen cutscene timeline driver (Focus 06 § 7) | 12 hrs |
| 26 | `RippleEngine.cs` | The Codex 08 § 4 ripple propagation (small-radius implementation) | 18 hrs |
| 27 | `TariffSO.cs` (ScriptableObject) | Per-choice tariff data | 4 hrs |
| 28 | `DialogueUI.cs` | Yarn line rendering (Bamao parchment frame + Pickle italic variant) | 18 hrs |
| 29 | `PlayerController.cs` | Wrapper over Character Controller Pro for walk-only + interact (Focus 04 § 2) | 12 hrs |
| 30 | `LumenLightController.cs` | Time-of-day-aware Lumen intensity controller | 8 hrs |
| 31 | `MemoryOrbInteractable.cs` | The orb's pickup + place + examine logic | 12 hrs |
| 32 | Unit tests (~30 specific tests) | Per the QA plan in §5 | 30 hrs |
| | **Total engineering time** | | **~478 hours (~12 dev-weeks)** |

At a **2-engineer team** working in parallel: **~6 weeks of focused engineering.** With realistic overhead (meetings, integration, rework): **~10 weeks.** Aligned with the Mission 1-2 build window.

---

## 3.0 The Art + Audio + Writing Cost Summary

Compiled from Focuses 01–07:

| Domain | Item | Cost |
|---|---|---|
| **Art** | Doris BoZo reskin | 3 hours |
| Art | Gerrold BoZo reskin | 5 hours |
| Art | Silent villager reskin | 2 hours |
| Art | Set-Piece #18 (kitchen at first light) | $5,500 |
| Art | Set-Piece #14 (bedroom of a sick person) | $5,800 |
| Art | Lens authoring (JOY+GRIEF+GRACE+WONDER, 4 lenses) | $4,500 |
| Art | Mood lighting rigs (morning, dim-amber-evening) | (10 hours tech artist) |
| Art | Cast figure: Margery sprite | $3,200 |
| Art | Cast figure: young Doris sprite | $3,200 |
| Art | The handkerchief prop | $400 |
| Art | The wedding photograph prop | $800 |
| Art | The tea-carrier wooden box prop | $400 |
| Art | Pickle's cat model (purchased + reskin) | $15 + 15 hrs |
| Art | Hollow interior dressing | (5 hours, all from MVM + Harvest Garden) |
| Art | Cottage interior dressing | (5 hours, hand-dressed) |
| Art | The Tone Compass card illustration | $2,800 |
| **Audio** | Composer cues (D-MD01-A + M-MD02-A,B,C,D,E + Doris motif + Gerrold motif + Margery cue + main theme entry + 11 Cleanse cues + 9 Polish cues) | $32,000 (subset of $120k envelope) |
| Audio | Voice acting Tier B (Doris ~3 min + Gerrold ~6 min + Pickle ~1 min) | ~$28,000 |
| Audio | Foley + Wwise integration | ~$8,000 |
| Audio | The Listen scene's extended VO | ~$22,000 |
| **Writing** | Doris M1 dialogue (~180 lines) | 8 hours |
| Writing | Gerrold M2 dialogue (~270 lines) | 14 hours |
| Writing | Doris M2 brief dialogue (~30 lines) | 4 hours |
| Writing | Tea modifier insertions | 4 hours |
| Writing | Codex tooltips for examinable props (~40 entries) | 14 hours |
| Writing | Day 1 Evening Ledger prose | 4 hours |
| Writing | Day 2 Evening Ledger prose (5 variants) | 12 hours |
| Writing | Pickle dialogue (4 lines) | 2 hours |
| Writing | Marin's note (M1 + M2 expanded) | 3 hours |
| Writing | DOR-001 full prose (270 words) | 3 hours |
| Writing | GER-007 full prose + Cleanse outcome variants (~5,000 words across variants) | 18 hours |
| **Localization** | 6 languages text-only | $3,520 |
| **Total art + audio + writing + localization** | | **~$118,000 + ~150 art/writing hours** |

---

## 4.0 The Mission 1-2 Build Timeline

A 16-week parallel schedule (~4 months):

| Week | Engineering | Art | Audio | Writing |
|---|---|---|---|---|
| 1 | Project setup, Unity 6 + URP + Yarn + Asset Inventory 4 first | Concept passes for Doris + Gerrold + lane mood boards | Composer auditions | Yarn style guide + Doris M1 dialogue draft |
| 2 | `VillageState` + `MemoryNodeSO` + `VillagerSO` schemas | Doris BoZo reskin + apron mesh | Composer signs contract | Doris M1 dialogue final + Day 1 Ledger prose |
| 3 | `PolishMiniGame` core + `MemoryOrb_Master` shader | Lane scene assembly | Composer delivers main theme + Doris motif | Doris M2 brief + Codex tooltips first pass |
| 4 | `PolishMiniGame` polish + autocomplete | Hollow interior assembly | Composer delivers M-MD01-A | Marin's note + DOR-001 prose final |
| 5 | `DayCycleManager` + LightMap Fusion Pro baking | Lightmap baking sessions | Foley field recording (kitchen, lane) | Gerrold M2 dialogue draft |
| 6 | `EveningLedgerUI` + `SaveService` | Set-Piece #18 painted | Foley begins | Gerrold M2 dialogue final |
| 7 | `YarnVillageStateBridge` + custom commands | Set-Piece #18 layered for parallax | Composer delivers Gerrold motif | Tea modifier insertions + GER-007 prose |
| 8 | `MemoryDreamSequencer` + Cutscene Engine timeline 1 | Gerrold BoZo reskin + handkerchief prop | Composer delivers Margery cues (5) | Day 2 Ledger prose all 5 variants |
| 9 | `CleanseMiniGame` core + crack-trace logic | Cottage exterior + walk lane | VO recording Doris (90 min session) | Pickle quotes + Codex tooltips 2nd pass |
| 10 | `CleanseMiniGame` outcome states + autocomplete | Cottage interior + signature props | VO recording Gerrold (3-hour session) | QA writers' pass on all prose |
| 11 | `ChoiceCardUI` + Pickle pre-choice commentary | Set-Piece #14 painted + layered | VO recording Pickle (1-hour session) | Tone Compass card text |
| 12 | `ListenSceneSequencer` + 3-min cutscene | Margery + young Doris sprites | Composer final mix pass | Localization handoff to 6 translators |
| 13 | `RippleEngine` + tariff bindings | Tone Compass card illustration | Foley + Wwise integration | Localization first deliveries |
| 14 | `ComfortToolsMenu` + `ToneCompassCard` | The 7-prop signature interior detail polish | Cleanse + Polish SFX integration | Localization review |
| 15 | Integration + first internal playtest (8 testers) | Animation polish | VO mastering | Localization final |
| 16 | Bug-fix + 20-person playtest | Final visual polish | Final mix | Final QA |

**Net delivery: end of week 16 → ready for the 200-person playtest in week 17–18.**

---

## 5.0 The QA Test Plan — 30 Acceptance Criteria

The vertical slice is **acceptable** only if all 30 criteria pass. Internal QA team validates these *before* the 200-person playtest.

### 5.1 Functional acceptance criteria (10)

| # | Test | Expected result |
|---|---|---|
| F-1 | Player walks the lane in <90 seconds | Pass; no animation glitches |
| F-2 | Doris dialogue branches correctly across all 3 first-greeting paths | Pass on all 3 |
| F-3 | Doris price-negotiation branches (Honor / Pay more / Pay less) all advance | Pass on all 3 |
| F-4 | Polish mini-game completes in 60-120 seconds at Default difficulty | Median 78s ± 12s |
| F-5 | Auto-Complete Polish toggle works from frame 0 of the mini-game | Visible + functional |
| F-6 | Evening Ledger Day 1 saves the game and persists across app restart | Pass on Steam + Switch |
| F-7 | Memory Dream 1 plays full 60s without audio sync drift | Pass |
| F-8 | Day 2 morning loads Doris's morning greeting + Gerrold's knock at the right time | Pass |
| F-9 | Herb harvest interaction works in <3 button presses | Pass |
| F-10 | Tea brewing 90-second wait completes without frame drops | Pass at 60 fps on target Android |

### 5.2 Narrative acceptance criteria (8)

| # | Test | Expected result |
|---|---|---|
| N-1 | All 4 choice card options visible + selectable at the moral choice | Pass |
| N-2 | Erase path → Cleanse aggressive → Memory Dream 2 Variant A or C plays | Pass on both outcomes |
| N-3 | Cleanse path → Cleanse careful → Memory Dream 2 Variant A/B/C plays | Pass on all 3 outcomes |
| N-4 | Listen path → 3-minute Listen scene plays → Variant D | Pass |
| N-5 | Defer path → orb stays on counter → Variant E (30s) | Pass |
| N-6 | Echo Web first-light animation triggers on inspecting Gerrold's orb | Pass |
| N-7 | Day 2 Evening Ledger displays the correct outcome prose per choice | Pass on all 5 paths |
| N-8 | Vow integrity glyphs visibly change after the choice | Pass — visible to playtesters in survey |

### 5.3 Comfort acceptance criteria (7)

| # | Test | Expected result |
|---|---|---|
| C-1 | Tone Compass card displays on first launch + is skippable | Pass |
| C-2 | Gentle Mode prompt shows after Tone Compass | Pass |
| C-3 | Gentle Mode toggleable mid-game via Comfort Tools | Pass |
| C-4 | Auto-Complete Polish + Auto-Complete Cleanse independently togglable | Pass |
| C-5 | All 4 subtitle size tiers render correctly | Pass |
| C-6 | Color-blind mode (3 palettes) shifts UI + memory orb colors | Pass |
| C-7 | One-hand control mode works for all M1-2 interactions | Pass |

### 5.4 Performance acceptance criteria (5)

| # | Test | Expected result |
|---|---|---|
| P-1 | Lane scene runs at 60 fps on mid-range Android (Snapdragon 778G+) | Pass at < 16.6ms frame budget |
| P-2 | Hollow interior runs at 60 fps on Switch handheld mode | Pass |
| P-3 | Memory Dream cinematics maintain 30 fps minimum on Switch | Pass |
| P-4 | App build size on Switch is < 1.2 GB | Pass |
| P-5 | App initial load time is < 22 seconds on Switch | Pass |

---

## 6.0 The Mission 1-2 Greenlight Criterion

Per Focus 00 § 7, the **internal vertical-slice greenlight gate** is:

> **20 cozy-target playtesters complete Mission 1 + Mission 2 (~1 hour combined).
>
> At least 14 of them (70%) report "I want to play more."**

This is **distinct** from the long-form Codex 16 § 9 G2 gate (the 200-person playtest). It is the *internal* validation step that gates moving from Mission 1-2 build into Mission 3 design + production.

### 6.1 The 20-person playtest recruitment

- 8 testers from existing cozy-game streamers' communities (paid $30 each).
- 8 testers from the studio's pre-launch newsletter list (paid $30 each).
- 4 testers from r/CozyGamers (paid $30 each).

Total recruitment + incentive cost: **~$1,200** + ~6 hours of CM coordination.

### 6.2 The post-playtest survey

Each player completes a 14-question survey + 20-minute interview. The survey's questions:

1. *On a scale of 1–10, how much did you enjoy the opening hour?*
2. *On a scale of 1–10, how cozy did you find the experience?*
3. *Which moment was your favorite?* (open text)
4. *Which moment, if any, felt off?* (open text)
5. *Did you feel the Polish mini-game was warm? (Y/N)*
6. *Did you feel the Cleanse mini-game was meaningful? (Y/N)*
7. *Did you make a moral choice with Gerrold? Which one?* (multiple choice)
8. *Did you feel the choice was hard?* (Y/N)
9. *Did the Memory Dream make you feel something?* (Y/N + open text)
10. *Did the cat (Pickle) make you smile?* (Y/N)
11. *Were any moments confusing or unclear?* (open text)
12. *Did the game's pace feel right?* (slider: too slow / right / too fast)
13. *Would you pre-order or recommend this game based on this 1 hour?* (Y/N + intensity)
14. **The greenlight question:** *"I want to play more."* (Y / N / Maybe)

### 6.3 The decision tree at the gate

| Outcome | Action |
|---|---|
| **14+ of 20 say "Yes, I want to play more"** | **GREENLIGHT.** Proceed to Mission 3 design + production. The Mission 1-2 build is the foundation; the 200-person playtest is the next gate (Codex 16 § 9 G2). |
| **10–13 of 20** | **POLISH + RE-TEST.** Identify the top 3 pain points from survey + interview. Patch over 4-6 weeks. Re-test with a fresh 20 testers. |
| **<10 of 20** | **PIVOT.** Per Codex 01 § 7 and Focus 00 § 4.4 — the project pivots to a 5-villager A-Short-Hike-tradition scope. The Hearthbound Hollow Long-Form Bible becomes the Scaling Reference for an entirely different studio project. |

### 6.4 The 3 "kill" findings (any one of these forces a pivot regardless of total score)

| Finding | Why it forces a pivot |
|---|---|
| **>30% of players bounce before Mission 1's polish mini-game** | The opening did not earn the cozy player's attention |
| **>25% of players name the Memory Dream as the worst moment** | The art-budget bet (parameterized engine) failed perceptually |
| **>15% of players use the word "boring" in open text** | The cozy pacing is wrong for the target audience |

If any of these fires, the **pivot is mandatory** even if the headline number passes. This is the Krieg Discipline — the project is **owned by the playtest, not by the team's optimism.**

---

## 7.0 Risk Register for Mission 1-2

The top 7 risks specific to the Mission 1-2 build:

| # | Risk | Probability | Severity | Mitigation |
|---|---|---|---|---|
| R-1 | Voice actor casting for Pickle takes >4 weeks | Medium | Low | Have 3 candidate actresses pre-screened by Week 5 |
| R-2 | Memory orb shader has performance issues on Switch | Medium | High | Profile early (Week 4); have a fallback "static orb" version ready as plan B |
| R-3 | Cleanse mini-game is too hard for cozy audience | Medium | High | Internal test by Week 10; adjustable difficulty profile already designed (Focus 04 § 3.6) |
| R-4 | Composer cannot deliver all cues by Week 12 | Medium | High | Lock composer contract Week 2 with milestone gates; reserve a backup composer (one of the 5 candidates) |
| R-5 | The Listen scene VO takes longer than 3-hour session | Low | Medium | Schedule a 4-hour VO session as buffer |
| R-6 | First-launch flow exceeds 5 minutes (player bounces) | Medium | High | Tone Compass card skippable from frame 1 + Gentle Mode prompt times out at 4 sec |
| R-7 | Set-Piece illustration delivery slips | Low | Medium | Lock illustrator contract Week 1 with two-week milestone reviews |

---

## 8.0 The Mission 1-2 Internal Playtest Cadence

Beyond the 20-person greenlight test at Week 17, internal playtests run **every two weeks** from Week 8 onward:

| Week | Playtest scope | Tester count |
|---|---|---|
| 8 | Mission 1 lane + Hollow + Polish (no audio) | 4 internal team members |
| 10 | Mission 1 complete + Memory Dream 1 | 6 internal team members |
| 12 | Mission 2 garden + tea brewing + cottage | 6 internal team members |
| 14 | Mission 2 complete + Memory Dream 2 (all 5 variants verified) | 8 internal + 4 external (close friends of the team) |
| 16 | Full Mission 1-2 with all polish | 8 internal + 4 external |
| 17 | **The 20-person greenlight test** | 20 cozy-target playtesters |

This cadence catches the highest-cost issues (mini-game tuning, dialogue pacing, scene transitions) before they compound.

---

## 9.0 The Krieg Five — non-negotiable principles for the Mission 1-2 build

1. **No feature ships that has not been internally playtested at least twice.**
2. **No mini-game ships without an Auto-Complete toggle.**
3. **No heavy content ships without the Tone Compass tag.**
4. **No save / autosave ships without a backup mechanism.**
5. **No asset gets imported that is not on the 22-asset whitelist.** (Codex C-tier and B-tier remain dormant.)

These five principles are the **production discipline** that keeps Mission 1-2 from bloating into Mission 1-5 by accident.

---

## 10.0 The Mission 1-2 Definition of Done

The Mission 1-2 build is **complete** when:

- ✅ All 30 QA acceptance criteria (§ 5) pass.
- ✅ All 7 risk-register items (§ 7) are at "monitored" or "resolved."
- ✅ The 16-week build timeline is hit (or replanned with producer signoff).
- ✅ The 5 internal playtests (§ 8 Weeks 8, 10, 12, 14, 16) have run.
- ✅ Localization for 6 languages is in place.
- ✅ The Tone Compass card is final + Steam-page integration is staged.
- ✅ The Save system survives 3 simulated power-failures.
- ✅ The 20-person greenlight playtest passes its criterion (§ 6.3).

**At that point, Mission 1-2 ships internally.** The team begins Mission 3 design.

---

## 11.0 What Mission 1-2 *Does Not* Deliver — final reminder

This deliberate restraint is the **value proposition** of the Mission 1-2 Focus folder:

- ❌ No predecessor (Marin) reveal — her name is hinted on a note only.
- ❌ No Echo Hologram, no Locked Room.
- ❌ No procedural villagers beyond the silent lane villager.
- ❌ No Forgotten Year arc.
- ❌ No Vance Ashby, no Memory Stock Exchange.
- ❌ No Mariska.
- ❌ No Tribunals, Dream Duels, Restoration Race, Thief, Lawsuit, Vance Arc.
- ❌ No Letter-Birds, Pen-Pal Villages, Dream Cinema, ARG.
- ❌ No Memory Bees, Composting, Borrowed Memories, Confession Booth.
- ❌ No Weave, Sever, Listen, Read, Translate, Identify, Compose, Search, Negotiate, Compose Verse mini-games.
- ❌ No Long Dreams.
- ❌ No Sommelier path.
- ❌ No Drifter Mode, NG+ Threads, Auction Day, Weekly Market.
- ❌ No mobile port.
- ❌ No Hollow upgrades beyond Level 1.
- ❌ No herbs beyond Lavender + Valerian.

**Every deferred item exists in the Long-Form Codices (`Docs/Depth_Bible/01-16`).** They wait, patient, for their turn. The team scales into them after Mission 2 ships.

---

## 12.0 Closing

This Production Checklist is **the final discipline** of the Mission 1-2 Focus folder.

The producer reads it.
The engineering lead reads it.
The art lead reads it.
The QA lead reads it.
The composer reads it.
The writer reads it.

Everyone knows what ships in Mission 1-2. Everyone knows what does not. Everyone knows the greenlight criterion. Everyone knows what triggers a pivot.

The build begins on Week 1 of the 16-week timeline. The 20-person playtest runs on Week 17. The producer's decision happens on Week 18.

In Week 19, **Hearthbound Hollow's vertical slice exists** — or the studio has learned what to build instead.

Either outcome is a good day.

— *Halvor Krieg*
*Mission 1-2 Focus 08 · v1.0 — Production Checklist (FINAL)*

— *End of the Mission 1-2 Focus folder. Open `Docs/Depth_Bible/01-16` for the Scaling Reference when Mission 2 ships and the producer greenlights the next phase.*
