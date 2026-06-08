# Codex 14 — **Audio, Music & ASMR Director**
### Specialist: **Iva Solberg, Audio, Music & ASMR Director**
*(19 yrs · composer-supervisor on Spiritfarer's OST nominations, sound designer on Coffee Talk and A Short Hike, Foley lead on Untitled Goose Game)*

> *"The cozy game lives in its sounds. Players will talk about the music for years; they will, more critically, **return to the game just to hear it again.** This codex is the architecture of that return. A vertical-mix score, a memory-tone music theory, an ASMR-tier foley pass, a composer brief that costs $120k and earns it back five times over in OST sales and TikTok adoption."*

---

## 1.0 Mission

Build an audio system that:

1. **Resolves [REV § 6 Recommendation 8 — Soundtrack budget = marketing]** with a defensible composer brief and budget.
2. **Engineers the ASMR layer** that turns orb-polishing into TikTok content.
3. **Implements the Memory-Tone Music Theory** (a novel diegetic music system tied to the orb hum mechanic).
4. **Composes 12 audio beds** for the mini-game library (Codex 13 § 9).
5. **Builds the vertical-mix dynamic score** that responds to village mood (Codex 08 § 5.2).

---

## 2.0 The Composer Brief

### 2.1 The composer hire

Target budget: **$120,000** for ~28 tracks + integration support across 14 months.

Top candidates (in order of fit):
1. **Max LL** (Spiritfarer-tier melodic minimalism)
2. **Stranger Sons** (Coffee Talk-adjacent jazz-folk)
3. **Joel Corelitz** (The Pathless / Hohokum — atmospheric)
4. **Ben Prunty** (FTL / Subset — restraint masters)
5. **A.M. Kovachev** (rising; folkloric flavor)

Audition brief: each candidate produces 90 seconds of *"the autumn kitchen"* + 60 seconds of *"the Tribunal of the Widower."* The candidate whose work makes Pell Doyne (Codex 06) cry without overplaying gets the contract.

### 2.2 The composer's deliverables

| Asset | Quantity | Length |
|---|---|---|
| Main theme | 1 | 4–6 min (full + radio edits) |
| Per-villager motif | 12 (Sealed) | 30–90 seconds each |
| Per-faction theme | 5 | 90 seconds each |
| Per-season theme | 8 | 2–3 min each |
| Festival pieces | 8 | 3–4 min each |
| Mini-game beds | 12 | 60–90 seconds, loopable |
| Memory Dream stems | 9 (one per emotion lens) | 60–120 seconds, loopable |
| Long Dream scores | 8 (Codex 11 § 9) | 6–9 min each |
| The Singing Hive song | 1 | 2 min (composer-tier, vocal) |
| Pickle's leitmotif | 1 | 8 seconds (chime + harp) |
| Echo Hologram's silence-music | 1 | ambient bed for the Hologram room |
| Idris's bardic library | 24 short folk songs | 30–90 seconds (intentionally amateurish — singer-songwriter direction) |

**Total composer output: ~28 master pieces + ~60 stems + 24 bardic songs.** 14-month production cycle.

### 2.3 The composer's collaboration with Iva (this role)

The composer writes the music. Iva (this codex) **integrates** it into the vertical-mix system, designs the foley, manages mastering for cross-platform parity, and supervises the diegetic-music sub-system (§ 3).

---

## 3.0 The Memory-Tone Music Theory

### 3.1 The core mechanic

Every memory orb **hums** at a specific pitch tied to its emotional palette:

| Palette | Hum Pitch | Sample (musical) |
|---|---|---|
| JOY | C major triad arpeggiated | bright, open |
| GRIEF | A minor sustained note | low, weighted |
| SHAME | F-sharp minor + tremolo | unsteady |
| AWE | D major suspended | open, unresolved |
| LONGING | E minor + perfect fifth | distant |
| DREAD | C# minor + dissonant overtone | unsettling |
| GRACE | G major softened | gentle |
| WONDER | B major rising arpeggio | curious |

When multiple orbs are shelved nearby, they **harmonize or dissonate.** The player can tune the shelves to be in pleasant harmony — or leave them dissonant, which Pickle will comment on.

### 3.2 The diegetic radio

The Hollow has a **hand-cranked memory-lantern** (Hollow Level 9, the Display Case). When the player loads a memory into it, the lantern *plays* the memory's audio bed at low volume — the room is briefly scored by the memory itself.

This is the **diegetic-music sub-system.** Players can choose to "play" a specific memory's audio in the shop while working. It is a cozy in-game radio.

### 3.3 The shelf-tuning meta-mechanic

Players can rearrange shelves to **build chords.** A shelf with 3 orbs in C major, F major, G major triad arrangement *plays a chord* when the player walks past. This is:

- A **cosmetic mechanic** (no progression effect).
- A **discovery joy** (players will spend hours arranging).
- **A community challenge** (Codex 15: players can share their shelf-arrangements).

No other commercial game has shipped a shelf-tuning music mechanic. This is novel.

---

## 4.0 The Vertical-Mix Dynamic Score

### 4.1 What it is

The game's background score is **layered.** At any moment, the score is composed of:

```
score(t) = base_layer × village_mood
         + season_layer × current_month
         + villager_layer × who_is_in_the_shop
         + tension_layer × active_conflict_system
         + dream_layer × is_in_dream
```

These layers crossfade. The player never hears an awkward transition. The score *responds.*

### 4.2 Examples

- **Embershade Bright (mood 80+)**: base layer is *radiant*; bird calls audible; faint laughter from the inn.
- **Embershade Distant (mood 0–9)**: base layer drops to *near-silence*; only the wind; the village bells stop.
- **Doris in the shop**: Doris's villager motif rises subtly in the score.
- **Vance in the shop**: Vance's motif is *the corporate jingle, in cello* — funny + dread.
- **A Tribunal**: tension layer engages; a sparse minor-key ostinato.
- **In a Memory Dream**: dream layer takes over; the village score is muted.

### 4.3 The implementation

| Component | Tech |
|---|---|
| Vertical-mix orchestrator | Wwise or FMOD (industry-standard) |
| Stem count per scene | 8–14 |
| Crossfade time | 1.2 seconds default |
| Reverb / EQ buses | 4 (interior shop, exterior village, dream, dream-duel) |

Standard middleware. ~8 dev-weeks of integration after the composer's stems are delivered.

---

## 5.0 The ASMR Foley Pass

### 5.1 The signature sounds

| Action | Foley target |
|---|---|
| Polishing an orb | Glass-on-velvet, with a subtle harmonic resonance |
| Cleansing an orb | A thread-on-thread tracing sound |
| Weaving two orbs | A soft *click* when patterns lock |
| Severing an orb | A delicate snip + a bell |
| Brewing tea | Kettle, then the small whisper of leaves wetting |
| Walking on autumn leaves | Crisp + soft, layered |
| Opening the shop door | The bell + the wood creak (distinct from any other door) |
| Pickle's purr | Recorded from a real cat; multi-layered |
| Pickle's footstep | Inaudible by default; audible when Pickle approaches a memory orb |
| The Echo Hologram speaking | Slightly EQ'd through a "memory-resonance" filter |
| The Singing Hive | Real bee field recording + cello drone |
| A letter unfolding | Crackle, slightly amplified |
| The teapot pouring | Long, slow, ASMR-tier |

### 5.2 The signature sound budget

| Component | Cost |
|---|---|
| Field recording sessions (1 week, kitchen+market+forest+river) | $18k |
| Foley artist (8 weeks studio time) | $35k |
| ASMR-tier post (sound designer, 6 weeks) | $25k |
| Voice acting (Tier B — Codex 02 § 11) | $380–520k |
| Mastering (cross-platform parity) | $12k |
| Spatial audio implementation (Switch + PC) | $18k |
| **Subtotal (excluding VO)** | **~$108k** |

Total audio budget (with Tier B VO): **~$488–628k.**

### 5.3 The Solberg discipline

Every Foley sound in the game is **recorded for this game.** No stock library. The cozy audience can tell the difference within 10 minutes.

---

## 6.0 Engineered Short-Form Audio Moments

(Resolving [REV § 3 Bear Claim #6 — TikTok virality as engineered, not lottery].)

### 6.1 The 8 audio moments designed for clipping

| Moment | Length | Why it clips |
|---|---|---|
| **Pickle's purr at the windowsill** | 60s loop | ASMR comfort gold |
| **The kettle, twice** | 22s | Soft Day signature |
| **Polishing an orb to its hum revealing the dream behind it** | 40s | Visual + audio synergy |
| **The Singing Hive's first song** | 2 min | Tear-jerker; full music piece |
| **Idris's lullaby (the one that lands)** | 90s | Voice + cello |
| **The Echo Hologram's first words (45s)** | 45s | Emotional set-piece |
| **The Three Apples Bell at the hour** | 8s loop | Hourly cozy ambient |
| **Pickle's quote of the week** | 6–15s | Comedic quotable |

Each of these is **mastered specifically for short-form sharing** — peak loudness optimized for phone speakers, EQ tuned for laptop earbuds, mid-loop start-points clean.

### 6.2 The official soundboard

The game ships with an **in-game soundboard** at Hollow Level 9: the player can play back any unlocked sound. The soundboard exports to .wav files with attribution watermark.

This is the cozy game's first **official asset-export for community use.** TikTok creators love this kind of explicit permission.

---

## 7.0 The Main Theme — Specifications

The main theme — **"Hearthbound"** — must be:

1. **Hummable** by a non-musician after one playing.
2. **4–6 minutes long** in the full version.
3. **Playable in 4 arrangements**: solo piano (for trailers), full orchestral (for festivals), spare strings (for credits), and a one-instrument folk-violin (for marketing).
4. **Built on a 5-note motif** that recurs across the game (the per-villager motifs all reference this 5-note seed).
5. **Recognizable as the game's identity** within 8 seconds.

The composer must deliver the main theme **first.** Everything else follows from it.

---

## 8.0 OST Sales & Marketing Strategy

(Resolving Codex 00 § 0.8 KPI: OST + merch attach $3–5M.)

### 8.1 OST release plan

| Format | Price | Channel |
|---|---|---|
| Digital OST (Bandcamp + Steam) | $9.99 | Day 1 |
| Streaming (Spotify, Apple Music, YouTube Music) | free + ad revenue | Day 1 |
| Vinyl (limited 2-LP) | $42 | 6 months post-launch |
| CD (Japanese deluxe edition) | $32 | Day 1 (Japan-only Switch + CD bundle) |
| Sheet music book | $24 | Year 1 Q4 |

### 8.2 OST volume

~28 master pieces × ~3 min average = ~84 min of OST. Above the cozy-OST average (Spiritfarer: 63 min; Coffee Talk: 41 min).

### 8.3 OST revenue model

| Stream | Expected revenue (3-year) |
|---|---|
| Digital OST (Bandcamp + Steam) | $400k–800k |
| Streaming royalties | $80k–200k |
| Vinyl (2k–5k copies) | $84k–210k |
| Japanese CD bundle | $50k–120k |
| Sheet music book | $30k–60k |
| **Total OST revenue** | **~$650k–1.4M** |

Combined with merch (Codex 15 § 9), the OST+merch line **clears $3M+** in the base case.

---

## 9.0 The Memory Jar Physical Product (Cross-codex)

(Adopted from [REV § 6 Recommendation 12]; cross-referenced from Codex 15 § 9.)

The game's most iconic physical product:

- A **real glass memory orb** (~$45 retail), 60mm diameter, lit from within by a small warm LED, with a hand-printed Memory Card inside (the player's choice from the in-game collection at Hollow Level 9).
- Sold at $45 each, 1–2 colors per quarter, limited runs of 500–1000.
- Marketed via the same in-game soundboard + Pickle quote-cards.

Each Memory Jar carries a **printed audio QR** that plays the dream's audio bed when scanned with a phone. The bridge between physical merch and digital game.

This is the cozy game's first **physical-audio merchandise integration.** Projected lifetime revenue: **$1.2M–$2.5M.**

---

## 10.0 Localization of Audio

| Asset | Localization strategy |
|---|---|
| Voice acting | Per-language re-recording for Day-1 6 languages (Codex 02 § 12). Estimated +$280k. |
| Idris's verses | Re-translated as poetry, not literal. Per-language poet hired. ~$8k each. |
| Pickle | Same actress per language (where possible) for cross-cultural recognizability. |
| Magpie | Same. |
| Echo Hologram (Marin) | Same (recursive with Pickle, per Codex 12 § 4.5). |
| Songs | Original song lyrics translated + re-sung. |

Total localization audio budget: **~$420k** (Day 1, 6 languages).

---

## 11.0 Accessibility Audio

(Per Pell Doyne, Codex 06.)

- **Audio description track** for blind players (a narrator describes visual events). Optional. ~$45k budget, 1 voice actor + 4 weeks recording.
- **Subtitles** for all dialogue at 4 size tiers + 3 background tiers.
- **Audio reduction options:** music, foley, voice, ambient — independently adjustable.
- **Hum-only mode:** the orb hums without other audio. Pickle's reactions, the Hum mode user's signature ritual.

---

## 12.0 The Soundscape Manifesto

Every cozy game audio decision asks: *would a tired person at 11 PM put one ear bud in for this?*

If yes, ship. If no, redesign.

The Hearthbound Hollow soundscape is engineered **for that ear bud.**

---

## 13.0 Closing

> *"Sound is the cozy game's secret weapon. The cozy player closes their eyes on the couch. They hear the kettle, twice. They hear Pickle settle. They hear the rain. They feel — without any visual confirmation — that they are *home.* That is the auditory contract. That is what $120k of composer, $108k of foley, and $420k of localization buys. A home. The cozy player will come back to it for years."*
>
> — *Iva Solberg*

— *End of Codex 14. Next: `15_COMMUNITY_AND_ASYNC_FEATURES.md`.*
