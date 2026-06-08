# 🎨 Hearthbound Hollow — Color & Lighting Bible (Mission 1–2)
> The palette and lighting glossary the whole art team works from. Machine‑readable swatches: `manifests/lighting_palette.csv`.

The world of **Embershade Vale** resembles late‑19th‑century rural northern Europe with a single departure from realism: **memory is physical**. The palette is therefore an *autumn* palette — ochre, amber, leaf‑red, wood, stone, parchment — punctured only by the cool of a window and the otherworldly **emotional glow** of a memory orb.

---

## 1. The Master Autumn Palette

| Swatch | Name | Hex | Where it lives |
|---|---|---|---|
| 🟧 | Ember Orange | `#E8843C` | UI accent, building retint, the bakery glow |
| 🟨 | Amber Gold | `#F2B45A` | the autumn master tone; default orb tint; fireflies |
| 🟫 | Ochre | `#C9912E` | stone/wood retint, hay, lit windows |
| 🟫 | Wood Brown | `#6B4A2B` | cottages, fences, furniture, beams |
| ⬛ | Deep Wood | `#4A3220` | shadowed timber, tool‑belts, deep grain |
| ⬜ | Stone Grey | `#9A9187` | stone walls, cobble, kettle, the dead‑lavender grey |
| 🟥 | Leaf Red | `#B5482E` | autumn oak canopy, bushes, leaf‑fall, book leather |
| 🟦 | Dusty Blue | `#9DB4C0` | the cottage's soft‑blue door; cool window ambient; comfort UI |
| 🟦 | Cool Window | `#BFD0DA` | daylight window fill indoors; the kettle gift‑orb silver |
| 🟫 | Sepia | `#7A5C3E` | the wedding photo, the empty‑chair orb, aged paper |
| 🟪 | Lavender | `#9B86C4` | MemoryHerb Lavender (Calm); Margery's chair fabric tint |
| 🟩 | Valerian Green | `#8FA66E` | MemoryHerb Valerian (Sleep); the cold‑tea tint |
| 🟩 | Sage Green | `#7E8C5A` | ivy, bushes, garden mid‑tone |
| ⬜ | Parchment | `#F4E7CE` | the entire UI base; Bamao scrolls; ledger pages |
| ⬛ | Pickle Slate | `#6E7378` | Pickle the cat's coat |

---

## 2. The Emotional Palette (orb hum + dream lens)

A memory orb's `palette_tint` shader property is bound to the memory's **emotion**, and the audio designer's signature — *every orb hums a note whose pitch equals its palette* — keeps art and sound in lockstep. The same six emotions drive the Memory‑Dream color‑grade LUTs.

| Emotion | Hex | Tone | Mission 1–2 use |
|---|---|---|---|
| **Joy** | `#F6C66B` | warm gold | Doris's *First Loaves*; the Polish reveal |
| **Grief** | `#6B7E96` | cool blue‑grey | Gerrold's orb body; Dream 2 base |
| **Grace** | `#F6E2B3` | warm white‑gold | the "good Keeper" warmth; being heard |
| **Shame** | `#8A6A3A` | muted ochre‑brown | Gerrold's (mistaken) self‑blame |
| **Longing** | `#8C7AA6` | dusty violet | Doris's longing memory nodes |
| **Wonder** | `#EBD9A0` | soft golden trace | the Vow 7 "Listen" path in Dream 2 |

> **Cleanse safety read:** Gerrold's orb body is **Grief `#6B7E96`** with a soft **amber core `#FF9650`** — the warm centre is the memory itself; crossing it erases. The color contrast *is* the gameplay tell.

---

## 3. The Five Lighting Moods (one per scene + the dream onset)

### LIT‑001 · Dusk Warm Wayfinding — *the Village Lane*
In‑fiction **17:30**. A **single** directional sun `#FFC68A` at intensity 1.4 (35° elevation, azimuth 230° — low west‑south‑west) is the only realtime light. The lane is **deliberately under‑lit at spawn** and brightens toward the bakery's two glowing windows. 4 `GodRay_Soft_Warm` meshes through tree gaps; one lit `Lantern_Lit_Small` at the bend as the anchor. *The lighting is the wayfinding — no arrows, no markers.*

### LIT‑002 · Dim Cozy Interior — *the Hollow*
Two baked area lights at **3200 K**, low intensity (`LM_Evening`) — intentionally dim for tired night eyes. Warm pools puncture it: the realtime workbench desk lamp `#FFE0A0`, the stove (`Candle_Warm` + 1 realtime point — the warmest object in the room), and a permanent faint highlight so **Pickle is always visible** on the windowsill. **Realtime budget: 4.**

### LIT‑003 · Bright Morning Garden — *the Herb Garden*
In‑fiction **10:00**. A single directional sun `#FFF1D6`, straight sunlit — **the brightest, most‑lit scene** in the slice. This is the **cozy contrast principle**: go inside (dim, warm), go outside (bright, alive).

### LIT‑004 · Hearth‑Warm Afternoon — *the Widower's Cottage*
First sun after a morning overcast, with Stylized Weather **light fog (0.3)** on the side‑lane walk and 3 lantern halos. Inside, a cool baked window ambient (`LM_Cottage_Afternoon`) is cut by a realtime hearth point light `#FF9650` (gentle sine‑wave flicker) and a mantel candle `#FFE0A0`. The cottage is warm *because Doris lit the fire yesterday* — the warmth is a kindness, not a default. **Realtime budget: 2.**

### LIT‑005 · Amber Dream Onset — *the sleep transition*
The screen fades to **amber `#F2B45A`**, the kettle hiss fades out, and the dream music swells (~3 s) before the 2D Memory Dream begins.

---

## 4. Realtime Light Budget (the hard cap)

| Scene | Realtime lights |
|---|---|
| Village Lane | 1 (sun) |
| Hollow Interior | 4 (desk lamp, stove, +2) |
| Herb Garden | 1 (sun) |
| Widower's Cottage | 2 (hearth, mantel) |
| **Total across the slice** | **6** — everything else is baked (LightMap Fusion Pro) or Lumen mesh‑light |

---

## 5. Using This Bible
- **Set‑dressers** pull material tints from §1; never introduce a hue outside it without Art Director (Cass Vale) sign‑off.
- **Lighting artists** match the per‑scene mood in §3 and respect the §4 cap (Émile Roux / Ravi Sundqvist own the budget).
- **Tech Art / VFX** bind orb `palette_tint` and dream LUTs to §2 (Devin Marlowe / Nadia Frost / Sven Aleko).
- **UI/UX** keep every surface on Parchment `#F4E7CE` + Ember `#E8843C` (the "Hearthbound" Heat preset), with 3 color‑blind palettes available (Pell Doyne).

---
*Color & Lighting Bible v1.0 · `feat/mission-1-2-architecture` · swatches in `manifests/lighting_palette.csv`.*
