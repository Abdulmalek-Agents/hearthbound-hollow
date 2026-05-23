# Mission 1-2 · Focus 03 — **Scenes: Lane · Hollow · Garden · Cottage**
### The four shippable scenes for Mission 1-2 · Blocking · Lighting · Prop list · Audio
### Owners: Roan Avellan (Conflict / Cinematics) + Sven Aleko (Cinematic Director)

> *Four scenes only. Two for Mission 1 (the Village Lane at dusk, the Hollow Interior). Two for Mission 2 (the Herb Garden, the Widower's Cottage). Everything else in `Docs/Depth_Bible/03_WORLDBUILDING_AND_LORE.md` — the Blackspine Woods, the river, the schoolhouse, the cemetery, the inn's basement — is **DEFERRED**. We build these four perfectly and ship.*

---

## 1.0 Scene-by-Scene Coverage

| # | Scene | Mission | Real-time | Asset packs hit |
|---|---|---|---|---|
| 1 | **Village Lane at Dusk** | M1 (~6 min) | One-way walk + brief NPC encounter | Medieval Village Megapack, Zephyr, Lumen, Stylized Weather System (light fog) |
| 2 | **Hollow Interior** | M1 (~14 min) + M2 (~10 min reuse) | The shop room + workbench room | Medieval Village interior props + Harvest Garden tea cabinet props + custom orb shelf + Pickle on windowsill |
| 3 | **Herb Garden** | M2 (~6 min) | Behind the Hollow; 4 raised beds | Harvest Garden, Zephyr (foliage wind), Lumen (afternoon shafts) |
| 4 | **Widower's Cottage Interior** | M2 (~12 min) | Off a side lane in the village | Medieval Village (cottage exterior) + Microdetail Terrain (path) + Lumen (lantern halos) + a single hand-dressed interior |

**Total scene count: 4.** Total in-game playable area for Mission 1-2: roughly **5 acres** (a tiny fraction of the full 20-acre map planned in Codex 03 § 3). The rest of the map is **walled off** for now (a low stone wall at the far end of the lane, with a sign — *"Forge Path closed today"* — implying expandability without forcing it).

---

## 2.0 Scene 1 — **The Village Lane at Dusk**

### 2.1 Purpose

The player's first sixty seconds. The cozy aesthetic is established or it is not. Every art decision must read "warm autumn evening in a small village I want to live in."

### 2.2 Camera & locomotion

| Element | Specification |
|---|---|
| Camera | Cinemachine third-person follow, fixed offset (3.5m behind, 2m up, slight tilt-down 8°). |
| Player control | Walk-only (jog/sprint disabled in M1 by Character Controller Pro state). Walk speed: 1.8 m/s. |
| Lane length | ~80 metres from spawn to bakery door. |
| Linear bottleneck | Stone walls on both sides of the lane; the player cannot wander into other buildings. Each wall is dressed with autumn ivy (Zephyr-animated). |

### 2.3 Lighting

**Time of day in-fiction: 17:30** (just before sunset in autumn). The Lumen "stylized god rays" package (S-6) drives the look.

| Light | Source | Lumen prefab | Notes |
|---|---|---|---|
| Sun | Single directional light (the only realtime light in this scene) | Sun direction 35° elevation, azimuth 230° (low west-south-west) | Color: warm amber (`#FFC68A`), intensity 1.4 |
| God rays | 4 placements through tree gaps | `Lumen/GodRay_Soft_Warm.prefab` | Each god ray is a world-space mesh angled toward the player's path; intensity drops 50% on Mobile profile. |
| Bakery windows | 2 windows of Doris's bakery glow ahead | `Lumen/Window_WarmGlow.prefab` | The first visual *destination* the player sees. |
| Lantern at the lane's bend | 1 prop, lit | `Lumen/Lantern_Lit_Small.prefab` | Lighting designer's anchor — the player walks *toward* it from spawn. |
| Atmospheric particles | Light leaf-fall + occasional firefly | Zephyr foliage wind + ParticleSystem fireflies (8 instances) | Mobile profile reduces firefly count to 3. |

The lane is **deliberately under-lit at spawn** and brightens as the player walks. *The lighting itself is the wayfinding.* No floating arrows. No UI markers. The cozy player follows the warmth.

### 2.4 Audio

| Layer | Source | Notes |
|---|---|---|
| Composer's main theme | Iva's commissioned piece (Codex 14 § 7) at 35% volume | This is the **first time the player hears the Hearthbound main theme.** It plays softly under everything for 90 seconds, then fades. |
| Ambient nature | Medieval Village pack's autumn ambient (wind in dry leaves) | Loops; volume 0.6 |
| Distant village murmur | Medieval Village pack's rural-village-evening loop | Volume 0.2; muffled |
| Footsteps | Per-surface (cobble at start, dirt mid-lane, wood at the bakery door) | 8 unique foley clips per surface |
| Wind gusts | Periodic; tied to Zephyr's wind zone intensity | Two gusts during the walk (scripted, not random) |
| One distant rooster | Medieval Village pack | Plays once, ~halfway through the walk |
| The bakery oven | Faint, from the bakery as the player nears | Volume rises from 0 to 0.4 as the player approaches |

### 2.5 NPCs

| NPC | Position | Animation | Lines |
|---|---|---|---|
| **Doris** | On a stool outside the bakery, ~78m from spawn | Kneading dough (ACS layer over base idle) | Greeting lines (Focus 01 § 7.1) |
| **Silent villager** (procedural NPC #1) | On a bench at the 40m mark, reading a letter | Sitting + ACS "reading" upper-body layer | None. The player can wave. The villager nods. |

**Two NPCs total in the lane scene.** Asset Analysis spec confirmed. Procedural Villager #1 is built from BoZo (Warrior archetype reskin → an older man with a flat cap; ~2 hours of artist time, including a custom cap mesh).

### 2.6 Props from Medieval Village Megapack (specific prefabs)

| Prop | Quantity | Prefab path |
|---|---|---|
| Stone cottage A | 3 | `MVM/Buildings/Cottage_StoneA.prefab` |
| Stone cottage B | 2 | `MVM/Buildings/Cottage_StoneB.prefab` |
| Wood cottage | 1 (the bakery) | `MVM/Buildings/Shop_BakerVariant.prefab` (a re-tinted Shop_Inn from the pack) |
| Stone wall sections | 14 | `MVM/Walls/StoneWall_Section_2m.prefab` |
| Wooden fence | 6 | `MVM/Fences/Wood_3plank_section.prefab` |
| Lamp post | 2 | `MVM/Props/LampPost_Tall.prefab` |
| Bench (rural) | 1 | `MVM/Props/Bench_Plain.prefab` (where the silent villager sits) |
| Autumn tree (oak) | 5 | `MVM/Vegetation/Oak_Autumn.prefab` |
| Autumn tree (birch) | 3 | `MVM/Vegetation/Birch_Autumn.prefab` |
| Bush (mid-density) | 12 | `MVM/Vegetation/Bush_Medium.prefab` |
| Loose hay bale | 2 | `MVM/Props/HayBale_Round.prefab` |
| Cart (parked, unused) | 1 | `MVM/Props/Cart_Wooden.prefab` |
| Cobblestone road tile | ~25 | `MVM/Roads/CobbleStone_1x1.prefab` |
| Dirt road tile | ~30 | `MVM/Roads/DirtPath_1x1.prefab` |

**Total prop count: ~108.** All from a single asset pack — minimal artist intervention required. The level designer assembles the lane in **~2 working days.**

### 2.7 The path-side detour (the bee-hive Easter egg)

At the lane's 65m mark, a small unmarked side path leads ~15m to the back of Doris's bakery. There, the dormant beehive sits in a corner of the garden. **The detour is unmarked.** The player may or may not find it. If they do:

| Beat | What happens |
|---|---|
| Player enters the side path | Lumen god-ray adjusts slightly (extra warmth from the angle) |
| Player reaches the hive | A single faint bee animation (one bee, slow loop) |
| Player examines the hive | Codex entry unlocks: *"A dormant beehive. The wood is old. The varnish has not been refreshed in two summers. Someone loved this once."* |
| Cumulative effect | This *seeds the Mission 3+ Wedding Honey arc.* No other immediate consequence. |

The detour adds **30–60 seconds** to the lane walk for the curious player. The cozy contract: optional, rewarding, never forced.

---

## 3.0 Scene 2 — **The Hollow Interior**

### 3.1 Purpose

The player's home for ~70% of Mission 1-2. Every prop must do at least one of these three things:
1. Tell the player something about the predecessor (foreshadowing).
2. Be functional in the cozy daily loop (kettle, workbench, ledger, shelves).
3. Be a place Pickle can sit.

Anything that does none of these three things is **cut**.

### 3.2 Floor plan

```
                  ┌─────────────────────────────────────────┐
                  │                                         │
                  │            UPSTAIRS (locked in M1-2)    │
                  │                                         │
                  │      [staircase, blocked off by a       │
                  │       wooden barrier — "in repair"]     │
                  │                                         │
                  └──────────────┬──────────────────────────┘
                                 │  doorway
                  ┌──────────────┴──────────────────────────┐
                  │  THE SHOP ROOM                          │
                  │  [12m × 6m]                             │
                  │                                         │
                  │  ┌─[counter]─┐    [shelf 1]             │
                  │  │           │    [shelf 2]   [stove]   │
                  │  │  player   │    [shelf 3]            │
                  │  │  enters → │     ↑                    │
                  │  │           │     orb shelves          │
                  │  └───────────┘    (capacity: 6 orbs     │
                  │   counter has        in M1; 12 in M2    │
                  │   the cloth-         after one shelf    │
                  │   wrapped orb        rearrangement)     │
                  │   on Day 2                              │
                  │                                         │
                  │  [chair]           [windowsill ────→ ★]│  ← Pickle's spot
                  │  [chair]                                │
                  │                                         │
                  └────────────┬────────────────────────────┘
                               │  doorway
                  ┌────────────┴────────────────────────────┐
                  │  THE WORKBENCH ROOM                     │
                  │  [6m × 5m]                              │
                  │                                         │
                  │     [workbench]                         │
                  │     ┌─────────┐                         │
                  │     │ [orb     │                        │
                  │     │  cradle] │   [tea cabinet]        │
                  │     │ [tools]  │   [kettle on stove]    │
                  │     └─────────┘                         │
                  │                                         │
                  │     [door to garden, M2 only]           │
                  │                ↓                        │
                  │                                         │
                  └─────────────────────────────────────────┘
```

### 3.3 Lighting

| Light | Source | Notes |
|---|---|---|
| Main interior | 2 baked area lights (LightMap Fusion Pro `LM_Evening`) | Warm tone 3200K, low intensity (the shop is dim, intentionally — the cozy contract for tired eyes at night) |
| Workbench desk lamp | Realtime point light | One of the scene's 4 realtime lights. Color `#FFE0A0`. The lamp prop sits on the bench. |
| Window light (lane-side) | Baked area light + Lumen god ray (afternoon, fading by Mission 1's end of day) | 4 angled god ray meshes through the window |
| Stove glow | Lumen `Candle_Warm` prefab inside the stove + one realtime point light | The stove is the warmest object in the shop |
| Pickle's windowsill | A faint baked highlight | Always lit; Pickle is always visible |

**Realtime light budget for this scene: 4 (within mobile budget).**

### 3.4 The signature props (the predecessor seeds)

These props are **what tells the player a Keeper lived here before them.** Each prop is one foreshadowing line worth.

| Prop | Where | Why it's there | Codex effect if examined |
|---|---|---|---|
| **The apron on a hook** | Behind the workbench | Marin's apron, left behind | Codex: *"A long brown apron. Pockets stuffed with old receipts. The collar is frayed. Whoever wore this stood at this bench every day for years."* (No name yet.) |
| **A handwritten note pinned above the workbench** | At eye level | Marin's instructions | *"Polish in slow circles. Cleanse only what wants to leave. Listen more than you take. — M."* (M = Marin; the player will not yet know who that is.) |
| **A teapot kept permanently warm** | Tea cabinet | Cozy ambient — also tells the player tea is always available | Codex: *"This teapot has not been cold in twenty-two years. The Keeper before you trained the wood stove to do that. It is one of the village's small miracles."* |
| **Three orb shelves, each with one empty slot already filled by a *gift orb*** | Shop room shelves | Three orbs left by the predecessor as a welcome | Each examines as a Codex entry — see § 3.5 below. |
| **A small leather-bound Ledger on the desk** | Workbench corner | The Evening Ledger — Mission 1's save mechanic | Opens to a fresh page; previous pages are torn out. *"The Keeper before you tore the old pages out. She left you a clean book."* |
| **Pickle the cat** | Windowsill | (See Focus 01 § 6.) | The player's first interaction with her happens in this room. |
| **An open book on the chair** | Reading chair beside the window | The predecessor's last read; bookmarks at a page | Codex: *"A book of regional folklore, open to the chapter on 'the lampman.' The bookmark is a pressed yellow leaf. The leaf is dry but the page is still slightly damp where a thumb rested."* |

That last detail — *the damp thumb-print* — is the cozy game's first quiet ghost-tone. The player will not consciously notice it. They will feel it.

### 3.5 The three welcome-gift orbs

These three orbs are **already on the shelves** when the player enters the Hollow for the first time. They are inert — the player cannot interact with them mechanically in Mission 1-2 — but they are examinable in the Codex. Each is a hint at the predecessor.

| Orb | Color | Codex entry |
|---|---|---|
| **The bee orb** | Amber-gold | *"A single bee suspended mid-flight inside a glass orb. The bee is calm. It has been calm for at least three years. The orb hums very faintly when you stand close to it."* |
| **The kettle orb** | Pale silver | *"An orb that contains the sound — only the sound — of a kettle just before it boils. Holding the orb makes the room feel warmer by half a degree."* |
| **The empty chair orb** | Cool sepia | *"An orb that appears, when you look at it closely, to contain a single wooden chair. No one is in the chair. The chair is in late afternoon light. You set it down quickly."* |

These three orbs are **promises.** Mission 1-2 does not deliver on them. The cozy player who returns to them in Mission 4, Mission 7, Mission 12 will see them recontextualized.

### 3.6 Audio in the Hollow

| Layer | Notes |
|---|---|
| Composer's "Doris motif" | Plays *only* when Doris is on-screen (Mission 1) |
| Composer's "Gerrold motif" | Plays *only* when Gerrold is in the shop (Mission 2) |
| Ambient interior loop | Kettle low simmer + wood-stove crackle + distant outside-village murmur (volume 0.15) |
| Pickle's purr | Audible when player is within 1.5m of the windowsill; volume 0.3 |
| Footsteps | Wood floor foley (Game UI & Puzzle Sound Effects Pack S-8 has these) |
| The workbench operations | (See Focus 04 — Polish + Cleanse mini-game audio) |
| The bell on the door | One soft ring on entry/exit |

### 3.7 Construction time

Per Unity Asset Engineer estimate:

| Task | Hours |
|---|---|
| Layout the shop room with Medieval Village + Harvest Garden props | 4 |
| Layout the workbench room | 3 |
| Bake LM_Evening lightmap set with LightMap Fusion Pro | 2 |
| Place the 4 realtime lights + 5 god-ray Lumen prefabs | 2 |
| Configure Pickle's windowsill animation set | 2 |
| Wire the three welcome orbs to Codex entries | 3 |
| Place + wire the 7 signature props | 4 |
| Audio bus configuration + foley placement | 3 |
| **Total** | **23 hours (~3 working days)** |

---

## 4.0 Scene 3 — **The Herb Garden**

### 4.1 Purpose

Mission 2 opens a door at the back of the workbench room. The player walks into a small herb garden. This is the player's first **outdoor cozy space the Hollow itself owns** — the seed for the long-form Garden track (Codex 04 § 5).

The garden in Mission 2 is **deliberately small** — only 4 raised beds, only 2 herb varieties (Lavender + Valerian), only a watering can. Everything else (the apiary, the composting room, the Three-Petaled Memorial flora) is DEFERRED.

### 4.2 Layout

```
                ┌──────────────────────────────┐
                │                              │
                │    [the Hollow's back wall]  │
                │                              │
                │   ────door────              │
                │                              │
                │   [stepping stone path]      │
                │       │                      │
                │       v                      │
                │                              │
                │   ┌─────┐  ┌─────┐            │
                │   │ bed │  │ bed │            │
                │   │  1  │  │  2  │            │
                │   │ lav │  │ val │            │
                │   └─────┘  └─────┘            │
                │                              │
                │   ┌─────┐  ┌─────┐            │
                │   │ bed │  │ bed │            │
                │   │  3  │  │  4  │            │
                │   │empty│  │empty│            │
                │   └─────┘  └─────┘            │
                │                              │
                │   [watering can on a stool]  │
                │                              │
                │   [low wooden fence]         │
                │                              │
                │   [beyond: meadow,           │
                │    walled off in M1-2]       │
                │                              │
                └──────────────────────────────┘
```

### 4.3 Props (all from Harvest Garden, S-2)

| Prop | Quantity | Notes |
|---|---|---|
| Raised wooden bed | 4 | Two have crops (lavender, valerian); two are empty (planted by player in M3+) |
| Lavender plant (re-skin of crop model) | 6 (3 per bed) | Renamed `MemoryHerb_Lavender`; carries `effect: Calm` |
| Valerian plant (re-skin) | 6 (3 per bed) | Renamed `MemoryHerb_Valerian`; carries `effect: Sleep` |
| Watering can | 1 | Interactable (one-button watering animation, ~3 sec) |
| Hoe | 1 | Visible but not interactable in M1-2 |
| Basket | 1 | Carries harvested herbs after interaction |
| Wooden fence panels | 6 | Encloses the garden |
| Stepping stone path | 5 stones | Between door and beds |
| Stool | 1 | Where the watering can rests |
| Small flowerpot (decorative) | 3 | Empty; aesthetic |

**Total prop count: ~33.** All from one pack.

### 4.4 Lighting

Outdoor scene. Single directional sun light. Lumen god rays not needed (the garden is sunlit straight). Mobile-friendly.

**Time of day in-fiction: morning of Day 2 (10:00).** Brighter than the lane (which was dusk). The garden is the player's most-lit scene in Mission 1-2. This is the **cozy game's contrast principle** — go inside (dim, warm), go outside (bright, alive).

### 4.5 The harvest interaction

Per Asset Analysis A-2:

1. Player approaches a plant.
2. UI prompt: *"Harvest [Lavender / Valerian]"* (Bamao UI parchment frame).
3. One button-press → 0.8-second ACS upper-body "pluck" animation.
4. Plant disappears (or reduces to a stub for M3+ regrowth).
5. Item added to inventory: 1 stalk of herb.

The first harvest in Mission 2 triggers a Pickle line if Pickle has followed the player to the garden (rare in M1-2; requires Pickle Approval 75+):

> *"You picked the wrong one. Or the right one. I don't know herbs. I am a cat. Carry on."*

(See § 3.6 of Focus 06 for the full tea-brewing flow.)

### 4.6 Audio

| Layer | Notes |
|---|---|
| Bird song | Medieval Village pack's "rural morning bird" loop |
| Wind in leaves | Zephyr wind zone (medium intensity) |
| Bees (faint, in the distance) | Hint that the apiary exists somewhere beyond the wall (M1-2 player cannot see it) |
| Footsteps | Soft dirt foley |
| Watering animation | Water-onto-soil foley (S-8 pack) + one "gentle splash" cue |
| The harvest "pluck" | A single soft "snap" + the herb's emotion chord (lavender = D minor; valerian = G major sus) — composer cues |

### 4.7 Construction time

~6 hours for the level designer + 2 hours for the herb ScriptableObject layer (the `MemoryHerb` SO with `effect` field). **Total: ~1 working day.**

---

## 5.0 Scene 4 — **The Widower's Cottage**

### 5.1 Purpose

Mission 2's emotional peak. The cottage is **20% prop and 80% mood.** Every prop in it must say one of two things:

- *Someone lived here recently who is no longer here.*
- *The man who remains is enduring.*

### 5.2 The walk to the cottage

Per Asset Analysis Mission1-2 § 3 Mission 2: "short village walk to widower's cottage." The walk is **~35 metres** off the main lane (the player branches at the 25m mark of the lane scene's main path).

| Element | Specification |
|---|---|
| Distance | 35m |
| Time of day | Early afternoon — first sunlight after morning's overcast (Stylized Weather System transition) |
| Light fog | Yes (Stylized Weather System "light_fog" preset at 0.3 intensity) |
| Lumen lantern halos | 3 lanterns along the side lane (the cozy game's wayfinding) |
| Gerrold walking beside player | He paces at 1.5 m/s (slightly slower than the player's default) so the player intuitively matches his pace — *cozy walking is a social gesture, not a transport mechanic* |
| Dialogue triggers | 6 dialogue lines from Gerrold across the 25-second walk (Focus 02 § 7) |

### 5.3 Exterior of the cottage

| Prop | Source |
|---|---|
| Cottage building | Medieval Village `Cottage_StoneB` with a Colorize (A-21) soft-blue door |
| Front step (with the "wood gave" detail) | Custom 4-poly mesh — a single wooden plank with a visible split |
| Doormat | `MVM/Props/Doormat.prefab` |
| Window with curtain drawn half-way | Standard Medieval Village window prefab; curtain is a custom 12-poly cloth mesh, Zephyr-animated very subtly |
| Empty bird feeder on a hook | `MVM/Props/BirdFeeder.prefab` + custom "empty" texture (Margery used to fill it) |
| Hanging basket of dried lavender (dead) | Custom small mesh + Harvest Garden lavender mesh recolored grey |

The empty bird feeder and dead lavender are the **two strongest mood-tellers** on the exterior. Neither requires explanation. Both will be noticed.

### 5.4 Interior layout

```
                        ┌──────────────────────────────────┐
                        │                                  │
                        │   [hearth]                       │
                        │     ║   (fire still burning;     │
                        │     ║    Doris lit it yesterday) │
                        │    ┌─┴─────┐                     │
                        │    │  rug  │                     │
                        │    └───────┘                     │
                        │                                  │
                        │   ┌─────┐                         │
                        │   │chair│  ← Gerrold's            │
                        │   │     │                         │
                        │   └─────┘                         │
                        │                                  │
                        │   ┌─────┐                         │
                        │   │chair│  ← Margery's            │
                        │   │     │   (with book on arm)    │
                        │   └─────┘                         │
                        │                                  │
                        │   ┌──────────────────┐           │
                        │   │ small table       │           │
                        │   │ ┌──┐    ┌──┐     │           │
                        │   │ │  │    │cold│   │   ← place  │
                        │   │ │  │    │tea │   │     settings│
                        │   │ └──┘    └──┘     │     for two │
                        │   └──────────────────┘           │
                        │                                  │
                        │   [doorway to bedroom — closed]   │
                        │                                  │
                        └──────────────────────────────────┘
```

### 5.5 The signature props

| Prop | Source | Why |
|---|---|---|
| **Gerrold's chair** | Medieval Village `Chair_Wooden_HighBack` | Worn; the seat-cushion has an indent |
| **Margery's chair** | Same model, mirror image, slightly different fabric (Colorize tint) | Indent in seat-cushion identical to Gerrold's, *as if she still sat in it* |
| **Book on Margery's chair arm** | Custom 24-poly mesh | Open to a page she never finished. The bookmark is a pressed leaf — *the same dried-leaf bookmark style as the predecessor's book in the Hollow* (cozy cross-reference, intentional) |
| **Cold tea cup on the small table, in front of Margery's chair** | `Harvest Garden/Tea_Cup_Filled.prefab` with a "cold" tint shader | Gerrold places one daily |
| **Two place settings, one fresh, one unused** | Standard place-setting props | Mission 2's most poignant detail |
| **A framed photograph on the mantel** | Custom photo prop | A young Margery and Gerrold on their wedding day, ~36 years ago. Sepia. *Doris (younger) is in the background of the photo, just visible, holding bread.* (Cross-reference to Mission 1.) |
| **A toolbox under Gerrold's chair** | Custom or Magic Arsenal "ceremonial orb-cradle" sub-mesh (chisel + hammer + plane) | His carpenter's tools. Unused recently. |
| **The hearth fire** | Lumen `Candle_Warm` ×3 + a custom log mesh + a particle-system flame | The cottage is warm. Doris's kindness made it so. |

### 5.6 Lighting

| Light | Source | Notes |
|---|---|---|
| Hearth fire | 1 realtime point light + Lumen flame mesh | Color `#FF9650`; flickers gently (sine-wave intensity oscillator) |
| Bedroom doorway light leak | Faint sliver under the closed bedroom door | Reminds the player a *room exists* that they will never enter in M1-2 |
| Window afternoon light | Baked area light through the half-curtained window | Cool ambient |
| Mantel candle | 1 realtime point light + Lumen candle | Color `#FFE0A0` |

**Realtime lights in this scene: 2.** Comfortably within mobile budget.

### 5.7 Audio

| Layer | Notes |
|---|---|
| Composer's "Margery" cue | Plays softly when player enters the cottage (Focus 05 § 4) |
| Hearth crackle | Foley loop (S-8) |
| Wall clock | Faint tick. Off-frequency from the village clock outside. *(The cozy detail: a clock in a house where time has slowed.)* |
| Gerrold's chair creak | When he sits |
| The cup of cold tea | No sound. *(Intentional silence.)* |
| Window curtain in the breeze | One soft rustle per ~30 seconds |
| Pickle's purr | Only if she followed (very rare at M1-2 stage) |

### 5.8 The bedroom

The bedroom is the **closed door**. The player cannot enter in Mission 1-2. Examining the door yields a Codex entry:

> *A closed bedroom door. The handle is brass. It has been polished recently. The light underneath is steady. You feel — clearly, suddenly — that you should not open this door today. You believe Gerrold, also, has not opened it in some time.*

The bedroom is **Mission 5+ content.** It contains nothing yet in the build — it is dressed simply with a made bed and a single lamp, and walled off behind the closed door. The cozy player will know it is there. That is enough.

### 5.9 Construction time

| Task | Hours |
|---|---|
| Layout the cottage exterior + side lane | 3 |
| Layout the cottage interior | 5 |
| Configure all 8 signature props with Codex hooks | 5 |
| Bake `LM_Cottage_Afternoon` lightmap | 2 |
| Place 2 realtime lights + 4 Lumen candles | 1 |
| Author the wedding photograph prop (one custom asset) | 4 |
| Audio bus configuration | 2 |
| **Total** | **22 hours (~3 working days)** |

---

## 6.0 Scene Transitions

Per Asset Analysis A-7 (Cutscene Engine):

| Transition | Mechanic | Length |
|---|---|---|
| Spawn → Lane | Black fade in over 2 seconds, accompanied by composer's main theme entry | 2s |
| Lane → Hollow (entering for the first time) | Cutscene Engine 8-second cinematic: door opens, camera drifts to interior, Pickle is revealed on the windowsill | 8s |
| Hollow → Memory Dream 1 (end of M1) | Sleep transition: screen fades to amber, kettle hiss fades out, dream music swells | 3s |
| Day 1 → Day 2 morning | Black fade + composer cue + soft "morning light" reveal | 4s |
| Hollow → Herb Garden (M2) | The garden door opens. Bright outdoor light reveals. No cinematic — direct walk. | 0s (real-time) |
| Hollow → Walk to Widower's Cottage | Player and Gerrold leave the Hollow together. Camera pulls back to a side-view tracking shot for the walk. Cutscene Engine handles the camera blend. | (the walk is real-time, ~25s; camera transitions are 2s in + 2s out) |
| Widower's Cottage → Memory Dream 2 (after choice) | Sleep transition with the chosen-outcome variant of Dream 2 (Focus 05) | 3s |
| Day 2 → Mission Outro | Cutscene Engine ~30-second outro: a slow camera pan through the Hollow, ending on the orb (or empty space) on the shelf | 30s |

**Total cinematic time across Mission 1-2: ~85 seconds.** Skippable with one button. Replayable from the Pause menu's "Memories" section.

---

## 7.0 Scene Inventory — what makes Mission 1-2 shippable

| Item | Count | Source |
|---|---|---|
| Discrete scenes | 4 | (Lane, Hollow Interior, Herb Garden, Widower's Cottage) |
| Unique buildings | 6 visible (lane), 1 used interior (the Hollow), 1 used interior (the cottage) | Medieval Village Megapack |
| Unique characters on-screen | 4 (Doris, Gerrold, the silent villager in the lane, Pickle) | BoZo + 1 custom prop (Pickle's cat model — see § 8 below) |
| Realtime lights total | 6 across the 4 scenes (within Mobile budget) | URP Mobile pipeline |
| Baked lightmap sets | 4 (one per scene; one alternate `LM_Cottage_Evening` for ambient variance) | LightMap Fusion Pro |
| Lumen FX placements | ~20 | Lumen Stylized Light FX 2 |
| Cinematic transitions | 7 | Cutscene Engine |
| Foley unique cues | ~60 | Game UI & Puzzle SFX Pack + custom recordings |
| Music cues | 5 (main theme, Doris motif, Gerrold motif, Margery cue, garden brightness) | Composer-commissioned |

This is the **complete asset surface** of Mission 1-2. Everything beyond it is Scaling Reference.

---

## 8.0 Pickle's Cat Model

(One open production task.) Pickle is a cat. BoZo (the chosen character pack) does not include a cat. Three options:

1. **Buy a separate cat asset.** ($5–15 on the Asset Store; many available.) — *Recommended.*
2. **Model a custom cat.** (~12 hours of artist time; not the best use of M1-2 budget.)
3. **Use a 2D illustrated Pickle on the windowsill.** (~3 hours; works well in the cozy painted style; but does not allow Pickle to walk in later missions.)

**Recommendation: option 1 — buy "Stylized Cat (chibi)" or equivalent from the Asset Store.** Re-skin to a slate-grey tabby with a single white sock. ~2 hours of artist time after purchase.

Pickle's full skeleton + 15 animations (sleep, stretch, sit, walk, tail-flick, head-turn, judging-stare, paw-on-orb) must be acquired or animated. **Production budget: ~$15 (asset) + ~15 hours (re-skin + 8 animations needed for M1-2).** Other 7 animations are scaling content.

---

## 9.0 Closing

Four scenes. Four working days of level design. Two working days of audio. Two working days of cinematic transitions. **~10 working days for the whole scene surface of Mission 1-2.**

Every prop on the floor of the Hollow tells the player something true. Every god ray points them somewhere warmer. Every silent corner of the cottage is a place Margery used to be.

That is the level of detail Mission 1-2 ships at. Not more. Not less. The cozy player will know.

— *Roan Avellan + Sven Aleko*
*Mission 1-2 Focus 03 · v1.0*

> Next: `04_POLISH_AND_CLEANSE_MINIGAMES.md` — the two mini-games shipping in Mission 1-2, granular.
