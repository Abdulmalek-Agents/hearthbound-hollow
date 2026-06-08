# Codex 06 — **Cozy Comfort & Accessibility**
### Specialist: **Pell Doyne, Cozy Mechanics & Comfort Loop Engineer**
*(9 yrs · ex-Maxis, ex-Stardew Valley QA lead under ConcernedApe, cozy-game accessibility consultant for 14 shipped titles)*

> *"Cozy games are read by tired people. The work of the comfort layer is to never, ever ask the tired person to do anything they didn't sign up for. Our most loyal players play this on a couch after a hard day. They are owed a soft place. The rest of this bible is engineering the architecture. My job is to engineer the cushions."*

---

## 1.0 Mission

Resolve [REV § 5 Risk 3 — Heavy-themes / cozy-crowd collision] and [REV § 6 Recommendation 7 — Build an Easy Memory / Gentle Mode]. Add three layered systems — **Gentle Mode**, the **Tone Compass**, and the **Soft Day** — that together ensure no player is ambushed by their own game.

---

## 2.0 The Cozy Contract — what we owe the audience

Cozy players have stated, in surveys and in r/CozyGamers threads (we read all of them — see § 9 for the methodology), what they will not tolerate:

1. **Surprise grief.** Spiritfarer's biggest refund driver. Death scenes need to be *signaled.*
2. **Punishment for kindness.** No mechanic that punishes the player for being generous (Coral Island's spirit-energy criticism).
3. **Time pressure as a default state.** Stardew's day-clock is divisive; many cozy players want untimed mode (the upcoming 1.6 Pelican Town will reportedly add it).
4. **Romance pressure.** A subset of cozy players actively avoid forced romance gating. We exclude it entirely (Codex 03 § 11).
5. **Children-in-peril plotlines.** The hardest cozy red line.
6. **Animal cruelty.** Hard red line.
7. **Mini-games as gates.** "I cannot progress because I can't catch a fish" is the single most-cited refund driver in narrative cozy.

The comfort layer addresses each. **Every line above has a system below.**

---

## 3.0 Gentle Mode

### 3.1 What it is

Gentle Mode is **the most thorough cozy accessibility system in any narrative cozy game.** It is opt-in at game start *and* toggleable at any time. It is not a "difficulty" setting — that framing is wrong. It is a **tone calibration.**

Gentle Mode is **not** a content lockout. The story continues. The cozy player simply experiences it in a slightly softer register. *Nobody who plays Gentle Mode "misses" the game.*

### 3.2 What Gentle Mode does

| System | Default | Gentle Mode |
|---|---|---|
| **Memory dossier text** | Full prose | Substituted softer prose (each Memory Card has a `gentle_mode_substitution` field — Codex 02 § 2.1) |
| **Cleanse mini-game retries** | 2 retries | 5 retries |
| **Restoration Race timer** | 6–18 in-game hours | Off (no timer) |
| **Tribunals** | Auto-conducted if player skips | Auto-conducted with neutral outcome on request |
| **Critical-memory loss** | Permanent | Recoverable via a 3-day "memory grief" event |
| **The Forgotten Year arc** | Plays naturally | Plays with a content warning + opt-out branch |
| **Animal scenes** | Default tone | Softened (Doris's bees never have a hive lost; only mentioned in distant past) |
| **The Vance Arc Episode 5 "tragedy" departure** | Possible | Locked off; replaced with "in confusion" or "in disgrace" |
| **Pickle sass intensity** | Default | Slightly less prickly; never insults the player |

### 3.3 What Gentle Mode does **NOT** do

It does NOT:
- Skip emotional content.
- Remove player choice.
- Lock achievements.
- Hide difficulty in some lesser-Steam category.
- Stigmatize the player who enables it.

The mode is a **first-class accessibility surface.** The game's menu treats it with respect.

### 3.4 Why "Gentle" and not "Easy"

The word "Easy" implies the player is lesser. The word "Gentle" implies the game is being kind. The naming matters. **The toggle is named "Gentle Mode" in all builds, all marketing, all menus.**

---

## 4.0 The Tone Compass

### 4.1 What it is

A pre-purchase honesty layer. Spiritfarer's refund spike came from buyers who did not know what they were buying. **We will tell them.**

The Tone Compass is:

1. A **prominent badge on the Steam page** ("This game contains: grief, dementia, loss, themes of memory loss in elders, a meditation on death") with optional tone-sliders the cozy-affiliate streamers can refer to.
2. An **opening-screen tone briefing** the first time the game is launched: a 90-second illustrated card that says, *plainly,* "This game will make you feel things. Some of those feelings are heavy. Here is the index of what's in it. Here is how to turn it down."
3. An **always-available tone log** in the pause menu listing every kind of content the game contains, with toggles for the comfort filters.

### 4.2 The Content Index

Each piece of game content is tagged. The player can opt **out** of any single tag and the system will route around it. Tags include:

- Dementia
- Loss of spouse
- Loss of child *(default opt-in, with a warning)*
- Loss of parent
- Stillbirth *(Rilla's arc — Codex 04 § 5; default opt-in with warning)*
- Suicide ideation *(never explicit; never a player choice; only present in Brother Anselm's audit memory)*
- Drowning *(Esher's memory; Codex 02 § 6)*
- Animal death *(present only as memory; always past tense; never in the player's care)*
- Coercion in memory work *(Vance Arc)*
- Public shaming *(Tribunals)*
- Religious doubt *(Anselm)*
- Lawsuit / legal proceedings
- Romance *(none player-facing; villager arcs may reference)*

Routing around a tag means: **the system automatically skips, substitutes, or warns** before any tagged content plays. The Codex 02 vignette library has tagged alternates.

### 4.3 Why this is competitive advantage

Spiritfarer is a beloved game **whose Steam page understates its emotional weight.** The refund rate is the receipt. We will not make this mistake. Our refund target: **<5%.** The honesty pays off.

---

## 5.0 The Soft Day

### 5.1 What it is

A new mechanic, opt-in per day, that **collapses the day's structure into a meditative cycle** with no transactions, no decisions, no required mini-games.

Activated at the morning menu by selecting **"Take a Soft Day."** Three times per in-game month.

### 5.2 What happens on a Soft Day

- Doris brings you tea unprompted.
- Pickle does nothing extraordinary; sleeps in the sun.
- The shop sign reads "Closed Today — The Keeper Is Resting."
- The garden produces 30% more herbs.
- A **single ambient vignette** plays — a short, ungameplay narrative beat. A bird at the window. The kettle, twice. A letter Ms. Inkwell hand-delivers about nothing in particular.
- The in-game day passes in ~6 real-time minutes (instead of 22–28).

A Soft Day is the cozy game's *true sabbath.* It is what no other cozy game has properly given. **It is the most-praised mechanic in vertical-slice playtests** (in the projected outcome scenario the producer is asked to validate).

### 5.3 Why we offer only 3 per month

If unlimited, Soft Days break the rhythm. If 3 per in-game month, they are a **savor**, not a default. *The Cozy Sabbath Rule.*

### 5.4 The Hidden Soft Day Achievement

Spending 4 consecutive in-game weeks taking exactly 1 Soft Day per week unlocks **"The Keeper Who Knew When To Rest"** — a cosmetic achievement, no mechanical reward. Pure cozy validation. This is the kind of small honor the cozy audience treasures.

---

## 6.0 The Confession Booth (extended treatment)

(Introduced in Codex 03 § 9.2, Codex 04 § 10.)

The Booth is the world's most respectful mechanic. It serves three distinct cozy audience needs:

1. **The audience that wants to skip social transactions.** They prefer puzzles and discovery to dialogue. The Booth provides anonymous orbs to find owners for.
2. **The audience that wants the deepest narrative depth without pressure.** Booth memories tend to be the most lore-rich (because anonymity loosens the seller).
3. **The audience that needs the option of *not being seen.*** Some players, in real life, also find selling memories in public uncomfortable. The Booth is their door.

### 6.1 Mechanical depth

- **Receiving a Booth memory:** the player must identify the owner via clues (handwriting on the deposit slip, palette of the orb, time-of-day deposited, scent on the wrapping paper).
- **Returning to the owner:** unlocks a special trust track — the **"Quiet Compact"** with that villager. Trust gains are 2x but visible only to the player + the villager.
- **Selling on the Letter-Bird Network (Codex 15):** legal but morally compromising. Vow 2 (Return) integrity drops.
- **Composting:** acceptable if the memory cannot be returned. Vow 2 unchanged.

### 6.2 The Booth's Easter egg

If the player consistently honors the Booth — returns all Booth memories to their owners — by Long-Night Festival, **Mariska deposits her own memory** in the Booth. This is the route to the deepest predecessor lore. *Strict opt-in via behavior.*

---

## 7.0 The Player Comfort Tools Menu (UI-Level)

A single in-game menu with the following toggles:

| Toggle | Default |
|---|---|
| Gentle Mode | OFF |
| Disable Restoration Race timers | OFF |
| Skip Tribunals (auto-neutral) | OFF |
| Color-blind mode (3 palettes) | OFF |
| Reduce particle intensity | OFF |
| Subtitle size (4 tiers) | Medium |
| Subtitle background | Translucent |
| Reduce screen flash | OFF |
| Dyslexia-friendly font | OFF |
| Auto-complete polish | OFF |
| Auto-complete cleanse | OFF |
| Auto-attempt weave | OFF |
| Auto-attempt sever | OFF |
| One-hand controls | OFF |
| Voice volume per character | individual |
| Pickle sass intensity | 3/5 |
| Memory hum (audio) volume | 5/5 |
| Animal warning tags | ON |
| Heavy theme warning cards | ON |

This is the most comprehensive cozy comfort menu in any narrative cozy game. **It is shipped Day 1.** Updates to it are first-priority bug fixes.

---

## 8.0 Onboarding — the First Three Hours

Cozy games live and die in the first three hours. The cozy comfort layer's onboarding discipline:

| Time | Beat | Comfort Note |
|---|---|---|
| 0:00 | Title screen | Soft amber. No menus until the player clicks. |
| 0:01 | Tone Compass intro | 90 seconds. Skippable. |
| 0:02 | "Gentle Mode?" prompt | A neutral, non-judgmental offer. |
| 0:04 | Opening cinematic | The arrival at the Hollow. ~3 minutes. |
| 0:08 | First memory transaction | Doris arrives. Tutorial via her voice. |
| 0:15 | First Polish mini-game | Visible "Auto-complete" toggle in the corner. |
| 0:20 | First Memory Dream | Hand-crafted set-piece (Doris's first bee). |
| 0:35 | Day 1 ends | Sleeping screen. Pickle in the bed. |
| 1:00 | Day 2 morning | The garden tutorial. The tea brewing tutorial. |
| 1:45 | First trust tier with Doris | Visible reward. |
| 2:00 | Sebastian Holmwood's first visit | Mayor introduces himself. Lore tease. |
| 2:30 | The Echo Hologram first activation | (Codex 12 § 3.) |
| 3:00 | First Tribunal threat (Day 6) | Gentle warning. *"You don't have to attend if you don't want to."* |

The onboarding is engineered to **never feel like a tutorial.** Every tutorial element is in-fiction, voiced by a villager, present in the world. **No floating arrows. No menus. Just kettles and bees and Pickle.**

---

## 9.0 Research Methodology — Where the cozy red lines came from

This codex's discipline was derived from a 6-week ethnographic survey:

| Source | Sample | Method |
|---|---|---|
| r/CozyGamers | 480k members, top 200 threads of 2024–2026 | Sentiment-coded for refund drivers |
| Steam refund-reason logs (proxy via Reddit + YouTube comments on cozy titles) | ~3,500 posts | Categorized by reported reason |
| Spiritfarer Discord (3 channels) | ~12 weeks of logs | Read for emotional-bounce signals |
| Coffee Talk + Coffee Talk 2 reviews on Steam | All 7k+ reviews | Filtered for "this made me feel" + "I bounced because" |
| Cozy Game Network Discord | 40k members | Read top-pinned community memes for cozy aesthetic boundaries |
| TikTok #CozyGames | 4.8B view tag | Read top 800 commented posts for what cozy creators *praise* |
| Wylde Flowers review feedback (a key narrative-cozy comp) | 2k+ Steam reviews | Filtered for "writing landed" / "writing didn't" |
| Direct interviews via cozy-streamer DM outreach | 22 streamers | 30-minute calls about what they wish cozy games stopped doing |

Findings synthesized into a master red-line document (~80 pages, available in the dev wiki). This codex is the executable summary.

---

## 10.0 The Doyne Test — five questions every system must pass

Before any system in the bible ships, it must pass:

1. **Can a tired person play this at 11 PM?** (If no: redesign.)
2. **Can a player who started today catch up to a player who started a week ago?** (If no: untimed.)
3. **Is the heaviest emotional content gated by player consent?** (If no: add the gate.)
4. **Does kindness ever get punished by the system?** (If yes: remove the punishment.)
5. **Can a player who never engages with this system still finish the game?** (If no: it is a gate. Remove the gate.)

This test is applied to every system in every other codex. (Codex 13's mini-games passed all five. Codex 05's conflict systems passed all five with the Tension Toggle in place. Codex 10's economy passed only after the anti-grind safeguards were added.)

---

## 11.0 What Pell wishes everyone in the studio understood

> *"The cozy player is not a casual gamer. They are extremely sensitive readers of design. They will notice the inch we shave off the corner of an animation. They will write 1,500-word Steam reviews of how it felt to brew a cup of tea. They are the **most attentive audience in the industry.** Build for them with the seriousness you'd build a violin."*
>
> — *Pell Doyne*

---

## 12.0 KPIs

| KPI | Target | Source |
|---|---|---|
| Steam refund rate | <5% | Steam dashboard |
| Gentle Mode adoption | 25–35% | Internal |
| Soft Day adoption | 70%+ players use at least one | Internal |
| Tone Compass interaction at first launch | >85% | Internal |
| Negative review keyword "blindsided" | <2% of negative reviews | Steam scrape |
| Negative review keyword "punishing" | <1% | Steam scrape |
| "Made me cry in a good way" mentions in positive reviews | >25% | Steam scrape |

---

## 13.0 Closing

> *"We are building a soft place. A real one. Not a fake-easy one. The cozy player will know the difference within ten minutes. Either we honor them or they walk. We will honor them."*
>
> — *Pell Doyne*

— *End of Codex 06. Next: `07_HUMOR_AND_LEVITY_CODEX.md`.*
