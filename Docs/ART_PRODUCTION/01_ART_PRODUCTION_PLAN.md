# 🫖 Hearthbound Hollow — Art Production Plan
## Mission 1 "Opening the Hollow" + Mission 2 "The Widower's Request"
> Branch: `feat/mission-1-2-architecture` · Engine: Unity 6000.4.4f1, **URP‑Mobile** (60 fps mid‑range Android/Switch) · Source canon: `Docs/MASTER_BIBLE/` (Engagement Bible + Depth Bible + Mission_1_2_Focus)

This plan turns the **Mission 1‑2 Focus** specs into a buildable art program: every scene, every character, every asset category, with color, lighting, location, LOD, and file‑path conventions. It is **scoped to the vertical slice** — ~55–75 minutes of playable content across 4 scenes. Everything beyond it (the Blackspine Woods, the river, the cellar vault, the 30‑villager roster, the 17 predecessor fragments) is **Scaling Reference** and stays behind the Out‑of‑Scope Wall.

---

## 1. Art North Star

> **Spiritfarer × Strange Horticulture, by candlelight, in autumn.** Warm, slightly dusty, late‑afternoon light shafts, wind in dry leaves, wood‑stove crackle. The single departure from realism is that **memory is physical** — glass orbs that glow, hum, crack, and clear.

Three rules govern every art decision:
1. **The light is the wayfinding.** No floating arrows, no UI markers in the world. The cozy player follows the warmth (the lit lantern, the glowing bakery window, the god ray).
2. **Every prop earns its place.** In the Hollow, a prop must do one of three jobs — *foreshadow the predecessor (Marin)*, *be functional in the daily loop*, or *be a place Pickle can sit.* Anything else is cut.
3. **Cozy contrast.** Interiors are dim and warm (3200 K); exteriors are bright and alive. Going inside should feel like shelter; going outside should feel like morning.

---

## 2. The Global Palette & Lighting Language
*(full swatch list in `manifests/lighting_palette.csv` and `03_COLOR_AND_LIGHTING_BIBLE.md`)*

| Role | Swatch | Hex |
|---|---|---|
| Key sun (lane) | Sun Amber | `#FFC68A` (intensity 1.4) |
| Practical warm | Lamp Warm | `#FFE0A0` |
| Hearth | Hearth Orange | `#FF9650` |
| UI / building accent | Ember Orange | `#E8843C` |
| Autumn master | Amber Gold | `#F2B45A` |
| Stone / wood | Stone Grey `#9A9187` · Wood Brown `#6B4A2B` | — |
| Cool accent | Dusty Blue | `#9DB4C0` |
| UI base | Parchment | `#F4E7CE` |

**Emotional palette** (drives orb "hum" pitch, orb tint, and the Memory‑Dream color‑grade LUTs):

| Emotion | Hex | Where |
|---|---|---|
| Joy | `#F6C66B` | Doris's *First Loaves*; Polish reveal |
| Grief | `#6B7E96` | Gerrold's orb; Dream 2 |
| Grace | `#F6E2B3` | the "good Keeper" warmth; being heard |
| Shame | `#8A6A3A` | Gerrold's (incorrect) self‑blame |
| Longing | `#8C7AA6` | Doris's longing nodes |
| Wonder | `#EBD9A0` | the Vow 7 "Listen" path |

**Lighting moods (one per scene + the dream onset):** Dusk Warm Wayfinding (Lane) · Dim Cozy Interior 3200 K (Hollow) · Bright Morning Garden (Garden) · Hearth‑Warm Afternoon (Cottage) · Amber Dream Onset (sleep transition).

---

## 3. Pipeline, Conventions & Budget

### 3.1 Folder & file‑path conventions
Studio‑authored content lives under `Assets/_Project/`; vendored packs stay under `Assets/ThirdParty/` (never relocated — moving them breaks `.meta` GUIDs).

```
Assets/_Project/Art/
├── Environment/{Lane,Hollow,Garden,Cottage}/<Name>.prefab
├── Characters/<Name>/<Name>.prefab            # + accessory .fbx, decals
├── Memories/MemoryOrb_Master.shadergraph
│   └── Materials/Orb_<Subject>.mat            # material instances of the master
├── UI/{Theme,Screens,HUD,Bamao,Comfort,Loop,Portraits}/<Name>.prefab|.png
├── Lighting/{Lane,Hollow,Garden,Cottage}/{Rig_*,LM_*}.asset
└── Dreams/{Dream01_FirstLoaves,Dream02_LastWeek,LUT,TitleCards}/
Assets/_Project/Prefabs/VFX/<Name>.prefab      # incl. /Lumen subfolder
```
**Naming:** `PascalCase` prefabs; orb materials `Orb_<Subject>`; lightmaps `LM_<Scene>_<TimeOfDay>`; Lumen FX keep their pack prefab names (`GodRay_Soft_Warm`, `Candle_Warm`, `Lantern_Lit_Small`, `Window_WarmGlow`). Addressables labels: `Village/Mission01`, `Village/Mission02`, `Dream/*`.

### 3.2 LOD policy (URP‑Mobile)
- **Buildings / large props:** LOD0 (full) → LOD1 (~50%) → LOD2 **impostor card** beyond ~40 m; GPU‑instanced; static‑batched.
- **Interior hero props & the workbench:** LOD0 only (always near), lightmap‑static.
- **Memory orbs:** **LOD0 only** — they are the hero; the mobile shader variant drops refraction to flat‑glass on the low tier rather than swapping mesh.
- **Vegetation:** LOD0/LOD1 + billboard LOD2, cross‑fade cull (~30 m); wind only on LOD0‑1.
- **Characters:** LOD0 ~8k → LOD1 50% → LOD2 35%; background NPC may use an impostor LOD2. BoZo chibi is ~⅓ the texture cost of realistic civilians.
- **UI / 2D portraits / dream art:** no LOD; atlas to ≤2048².

### 3.3 Mobile performance budget (the hard gates, owned by Ravi Sundqvist)
| Constraint | Target |
|---|---|
| Build size | < 1.5 GB (ASTC compression; Addressables stream village/dream content) |
| Texture memory resident | < 200 MB (atlas Bamao UI, decimate Medieval Village 4K→2K) |
| Realtime lights | 1 directional + up to 4 point per interior — **6 total across all 4 scenes** |
| Draw calls (mid‑range Android) | < 200 / frame (instancing on walls, static batching, Lumen mesh‑light pooling) |
| Frame budget | 16.6 ms (60 fps) |
| God‑ray / firefly counts | tiered down ~50% / to 3 instances on the Mobile profile |

### 3.4 Source asset packs (the spine)
Medieval Village Megapack (village + interiors) · Harvest Garden (garden + tea props) · **All In 1 Shader Nodes** (the orb shader — ~70% of mini‑game visuals) · Lumen Stylized Light FX 2 (god rays/halos) · Cutscene Engine (dreams/transitions) · BoZo Stylized Modular Characters (chibi villagers) · Colorize (autumn retint + variants) · LightMap Fusion Pro (bakes) · Zephyr (wind) · VoluSmokeFX (steam/smoke) · Stylized Weather System (light fog) · Heat UI (system UI) · Bamao Fantasy GUI (diegetic UI) · Skill Tree Builder (repurposed as the Memory Wall) · Eyes Animator + Animation Composer System (NPC life) · Microdetail Terrain (path detail).

---

## 4. The Four Scenes (the complete environment surface)

> **4 scenes only.** ~5 playable acres; the rest of the 20‑acre map is walled off (a low stone wall + a *"Forge Path closed today"* sign — expandable without forcing it).

### Scene 1 — The Village Lane at Dusk (M1, ~6 min)
- **Location/flow:** ~80 m one‑way walk, spawn → bakery door; stone walls bottleneck the path (no wandering); a hidden 15 m side path to the **dormant beehive** Easter egg seeds the Mission 3+ honey arc.
- **Lighting (Dusk Warm Wayfinding):** in‑fiction **17:30**. A *single* directional sun `#FFC68A` @ 1.4 (the only realtime light), 4 `GodRay_Soft_Warm` placements through tree gaps, the bakery's two `Window_WarmGlow` panels as the visual destination, one lit `Lantern_Lit_Small` at the bend as the anchor. Deliberately **under‑lit at spawn**, brightening toward the bakery.
- **Atmosphere:** Zephyr leaf‑fall + 8 fireflies (3 on Mobile); Stylized Weather light fog.
- **Props:** ~108 prefabs from one pack (cottages ×6, 14 wall sections, fences, 2 lamp posts, bench, 5 oaks + 3 birches + 12 bushes, hay bales, cart, ~55 road tiles). Built by the level designer in ~2 days of effort.
- **Cast:** Doris on a stool kneading dough at 78 m; one **silent villager** on a bench at 40 m reading a letter (player can wave; he nods).
- **Owners:** Brynja Søl + Pim Aaltonen (dress) · Théo Vask (god rays) · Émile Roux (light) · Aurelio Pace (color script).

### Scene 2 — The Hollow Interior (M1 ~14 min + M2 reuse ~10 min)
- **Location/flow:** the player's home for ~70% of the slice. Shop room (12×6 m) + workbench room (6×5 m); upstairs locked behind an "in repair" barrier; garden door opens in M2.
- **Lighting (Dim Cozy Interior):** 2 baked area lights at **3200 K** low intensity (`LM_Evening`), a realtime workbench desk lamp `#FFE0A0`, window light + 4 god‑ray meshes, a stove glow (`Candle_Warm` + 1 realtime point — the warmest object), and a permanent faint highlight on Pickle's windowsill. **Realtime budget: 4.**
- **The predecessor seeds (7 signature props):** Marin's apron on a hook · the pinned note *"Polish in slow circles… — M."* · the 22‑years‑warm teapot · the leather Evening Ledger (torn pages) · the open folklore book with a pressed‑leaf bookmark **and a damp thumb‑print** (the first quiet ghost‑tone) · the three welcome **gift orbs** (bee/amber‑gold, kettle/pale‑silver, empty‑chair/cool‑sepia) · Pickle on the windowsill.
- **Owners:** Lukas Behr (dress) · Inara Vellis (codex hooks) · Nadia Frost + Devin Marlowe (orbs) · Cosette Lemaire (bake).

### Scene 3 — The Herb Garden (M2, ~6 min)
- **Location/flow:** behind the workbench room; 4 raised beds (2 cropped — lavender + valerian; 2 empty for M3+), watering can on a stool, stepping‑stone path, low fence; the meadow beyond is walled off.
- **Lighting (Bright Morning Garden):** in‑fiction **10:00**, single directional sun `#FFF1D6` — **the brightest, most‑lit scene** in the slice (the cozy contrast principle). No god rays needed.
- **Interaction:** one‑button harvest (~0.8 s ACS pluck) → 1 herb stalk; lavender chord D‑minor, valerian chord G‑major‑sus. ~33 props from one pack.
- **Owners:** Pim Aaltonen (layout) · Saija Korhonen (MemoryHerb SO) · Rhea Calder (tea steam).

### Scene 4 — The Widower's Cottage (M2, ~12 min — the emotional peak)
- **Location/flow:** ~35 m off the side lane (player branches at the 25 m mark); Gerrold paces at 1.5 m/s so the player intuitively matches him — *cozy walking is a social gesture.*
- **Lighting (Hearth‑Warm Afternoon):** first sun after morning overcast + Stylized Weather light fog (0.3); 3 lantern halos along the side lane; inside, a realtime hearth point light `#FF9650` (gentle sine flicker) + a mantel candle `#FFE0A0` against a cool baked window ambient (`LM_Cottage_Afternoon`). **Realtime budget: 2.**
- **Mood‑tellers (it is 80% mood, 20% prop):** the empty bird feeder, the dead grey lavender, Gerrold's and Margery's matching chairs (identical cushion indents), her unfinished book (pressed‑leaf bookmark cross‑referencing the Hollow), the **cold tea cup** (Gerrold places one daily — *intentional silence, no sound*), two place settings (one fresh, one unused), the sepia **wedding photo** with a younger Doris holding bread in the background (M1 cross‑ref), and the **closed bedroom door** with a steady light leak (M5+ content, sealed).
- **Owners:** Nessa Crowe (dress) · Mira Salonen + concept (wedding photo) · Émile Roux (hearth) · Sven Aleko (Margery cue staging).

### Scene transitions (~85 s of cinematics total, all skippable/replayable)
Spawn→Lane (2 s fade) · Lane→Hollow (8 s reveal of Pickle on the sill) · Hollow→Dream 1 (3 s amber sleep) · Day 1→Day 2 (4 s morning reveal) · Hollow→Garden (real‑time) · Hollow→Cottage walk (real‑time + 2 s camera blends) · Cottage→Dream 2 (3 s, choice‑variant) · Day 2→Mission outro (30 s pan ending on the orb, or the empty space).

---

## 5. Character Art

| Character | Base | Reskin highlights | Palette |
|---|---|---|---|
| **Doris the Baker** (M1 hero) | BoZo **Cleric** | +5 eye‑wrinkles, silver‑streaked‑brown bun with a loose temple strand, off‑white flour‑dusted linen apron (~40‑poly overlay), rolled sleeves + freckled forearms, wooden hairpin | `#F4E7CE` / `#C9912E` / `#6B4A2B` |
| **Gerrold the Widower** (M2 hero) | BoZo **Bard** | silver hair, faint stubble, dark under‑eyes, knee‑length brown coat, leather tool‑belt with carpenter's chisels, a **stooped "carried‑weight" walk**, white handkerchief with an embroidered **"M"** | `#7A5C3E` / `#6B4A2B` / `#9A9187` |
| **Silent Lane Villager** | BoZo **Warrior** | older man, custom flat‑cap mesh; sits reading a letter; player can wave | `#6B4A2B` / `#9A9187` |
| **Pickle the cat** | purchased *Stylized Cat (chibi)* | slate‑grey tabby with one white sock; 8 of 15 anims for the slice (sleep/stretch/sit/walk/tail‑flick/head‑turn/**judging‑stare**/paw‑on‑orb) | `#6E7378` / `#F4E7CE` |

**NPC life (mandatory):** every speaker gets an **Eyes Animator** profile (Doris: nervy blink 0.15/s, look priority player>orb>shelf; Gerrold: contracted pupils 0.4, slow saccades 0.25/s, look priority handkerchief>empty‑chair>ground>player) and **ACS** upper‑body layers (Doris kneads / wipes hands / offers the box warmly with two hands; Gerrold carries weight / sits with a chair creak).
**Owners:** Tobias Renn (lead) · Dahlia Voss (heroes) · Felix Mar (villager + Pickle) · Otto Lindgren + Sigrid Holm (anim/eyes).

---

## 6. Memory Orbs, Shaders & VFX

- **`MemoryOrb_Master` Shader Graph** (Devin Marlowe) — one master, ~12 exposed properties (`clarity` 0–1, `crack_intensity` 0–1, `dissolve_progress`, `glow_strength`, `palette_tint`, weight scale…). Every orb is a **material instance**. Nodes used: glass refraction (body), fresnel rim (silhouette glow), dissolve (cracks + Cleanse), scanline (veiled state), world outline (selection), hologram (hover preview). Does ~70% of both mini‑games' visual work. Mobile variant drops refraction to flat‑glass.
- **Polish (M1)** — remove age‑faded **amber fog** in slow circles; the memory scene becomes visible behind the glass; sparkle/clarity reveal + Lumen `Candle_Warm` inside the orb on completion. Warm, low‑stakes, **Auto‑Complete** always offered.
- **Cleanse (M2)** — trace each crack once without lifting, **never crossing the soft‑amber CORE** region. A glowing seal‑thread follows the cursor; crossing the core fades Margery's face and drops `memory_integrity` (a fully **narratively‑absorbed** failure — never a "FAILED" string).
- **Atmospheric VFX** (Rhea Calder / Jonas Pelt) — VoluSmokeFX kettle/chimney/tea steam + the dream wisp; fireflies + leaf‑fall; all GPU, count‑tiered for Mobile.

---

## 7. UI / UX Art (system + diegetic + the cozy loop)

- **System UI** = Heat, reskinned to preset #6 **"Hearthbound"** (warm parchment `#F4E7CE` + ember orange `#E8843C`, humanist serif via TMP): Main Menu ("Open The Hollow"), Loading ("*Some memories want to be sold. Some don't.*"), Pause (with a **Memories** replay section), HUD (day + memory‑in‑hand), Mission Complete (shows the Ledger; **never** a FAILED screen).
- **Diegetic UI** = Bamao parchment: dialogue scroll (portrait left + 3 choice scrolls), the **Evening Ledger** open‑book (M1 save; click a Vow glyph → a Marin reflection), memory tooltip (Colour/Clarity/Cracks/Weight), shelf labels, and the **choice card** (no timer, no stat bars, no morality meter — *just four sentences and the orb in the player's hand*).
- **The 7 cozy‑loop screens** (Pia Lindqvist / Wren Aldous): Agenda morning card (P1), Request Board `[B]` (P2), My Hollow `[U]` (P3), Garden & Tea `[G]` (P4), Living Workbench `[K]` (P5), Memory Wall `[M]` (P6 — the Skill‑Tree‑Builder node graph reskinned to Memory/Connection/Cleanse‑Cost), the Almanac (P7), and the Journal `[J]` (celebratory Day/Coin/Memories/Echoes — **D‑076** visible‑but‑never‑anxious progression).
- **Comfort UI** (Pell Doyne) — Tone Compass + Gentle Mode toggle at start (mandatory) + the Comfort Tools menu (3 color‑blind palettes, dyslexia font, subtitle tiers, per‑character voice sliders, one‑hand controls).

---

## 8. Memory Dreams (2D illustrated short films)

- **Dream 1 — *The First Loaves*** (M1, ~60 s, warm): 1972, a kitchen at first light; young Doris bakes alone, afraid, pulls golden loaves, wipes her hands on her apron. Hand‑painted, Spiritfarer‑adjacent oversaturated golds; **Lens_JOY** LUT over the whole timeline, fading to amber at 55–60 s; morning rig (pale gold).
- **Dream 2 — *The Last Week*** (M2, ~75–90 s, branching): Margery's last week; **4 narrative variants share assets** (Cleanse‑Perfect / Acceptable / over‑erased / the Vow 7 "Listen" path). Set‑piece: the bedroom of a sick person; Gerrold's brief appearance is the warm beat. LUTs: `Lens_GRIEF_with_GRACE` (canonical), `Lens_GRIEF_SHAME` (erase/crossed), `Lens_GRIEF_GRACE_WONDER` (Listen). Dim‑amber‑evening rig.
- **Owners:** Sven Aleko (timelines) · Yara Senn / Mira Salonen (2D art) · Sigrid Holm (portrait reveals) · Iva Brandt (stems).

---

## 9. Audio‑driven Art Cues (the orb "hum")
The orb's `palette_tint` is bound to its **emotional palette**, and the audio designer's signature — *each orb hums a note whose pitch = palette* — keeps art and audio in lockstep. The five music cues (main theme, Doris motif, Gerrold motif, Margery cue, garden‑brightness) gate to on‑screen subject, so lighting changes and motif entries land on the same beat. Owner: Iva Brandt with Devin Marlowe.

---

## 10. Production Sequence (dependency‑ordered, not calendar‑bound)

Built **spine before limbs**, mirroring the engagement priority order. Each phase gates the next; nothing is scheduled to a date here — sequencing is by dependency only.

| Phase | Gate it unblocks | Lead |
|---|---|---|
| **A · Pre‑production** | 4 color scripts + character turnarounds + the orb palette sheet approved by the Art Director | Yara Senn / Cass Vale |
| **B · Pipeline up** | URP‑Mobile asset; Asset Inventory installed first; packs imported & Addressables‑tagged; hierarchy preset; LOD/ASTC import rules | Greta Halloran / Milo Pratt |
| **C · The orb language** | `MemoryOrb_Master` + the 6 orb materials; Polish/Cleanse FX prototyped and feeling right | Devin Marlowe / Nadia Frost |
| **D · The Hollow** | the home scene dressed, lit, 7 signature props + 3 gift orbs wired to codex | Brynja Søl / Lukas Behr |
| **E · The Lane** | the 80 m dusk walk with god‑ray wayfinding + the 2 NPCs | Pim Aaltonen / Théo Vask |
| **F · Characters** | Doris + the silent villager (M1), then Gerrold (M2); Pickle integrated with 8 anims + eyes | Tobias Renn / Dahlia Voss |
| **G · Garden + Cottage** | M2's two scenes; the cold‑tea / dead‑lavender / wedding‑photo mood set | Pim Aaltonen / Nessa Crowe |
| **H · UI + loop screens** | Hearthbound theme, Bamao diegetic UI, the 7 pillar screens, comfort tools | Pia Lindqvist / Esme Cordray / Pell Doyne |
| **I · Dreams + transitions** | Dream 1, Dream 2 (4 variants), the ~85 s of cinematics | Sven Aleko |
| **J · Lighting bake + polish** | all lightmaps baked, ~20 Lumen FX placed, mobile budget profiled green | Émile Roux / Ravi Sundqvist |
| **K · Cozy‑review + QA** | Cozy Contract audit, in‑engine asset verification, G‑Engage readiness | Marielle Voss / Petra Knoll |

*Source effort references (from `Focus 03`): the Hollow ≈ 23 hours, the Cottage ≈ 22 hours, the Lane ≈ 2 days, the Garden ≈ 1 day, Doris's art+anim ≈ 9 hours, Pickle ≈ ~$15 + ~15 hours. These are inputs for the producer's sequencing, not a delivery date.*

---

## 11. Art Acceptance Criteria (what "done" means for the slice)

1. The first 60 seconds of the lane read unmistakably as **"a warm autumn village I want to live in."**
2. **No floating arrows or world markers** — the light alone leads the player to the bakery.
3. Every one of the Hollow's 7 signature props returns a **codex line** when examined; the damp thumb‑print ships.
4. The orb **clarity/crack** states read clearly at a glance during Polish and Cleanse; **Auto‑Complete** is always reachable.
5. The Cottage's mood‑tellers (empty feeder, dead lavender, cold tea, two settings, wedding photo, closed bedroom door) all ship and need **no explanatory text**.
6. **6 realtime lights total**, build < 1.5 GB, < 200 MB textures, 60 fps on the mid‑range profile.
7. **No "FAILED" string anywhere**; Cleanse's *Crossed Core* is narratively absorbed.
8. Comfort suite (Tone Compass, Gentle Mode, color‑blind/dyslexia/subtitles/voice sliders) present from the first build.
9. Cross‑references land: the pressed‑leaf bookmark and the younger Doris in the wedding photo.
10. **`cozy-review` sign‑off** (Creative Director + Comfort) before merge; `Docs/PROGRESS.md` updated.

> Ship these four scenes perfectly. Not more, not less. *The cozy player will know.*

---
*Art Production Plan v1.0 · `feat/mission-1-2-architecture` · derived from `Docs/MASTER_BIBLE/Depth_Bible/Mission_1_2_Focus/` and `Docs/Asset_Analysis_Mission1-2.md`.*
