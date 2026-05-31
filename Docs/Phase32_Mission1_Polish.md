# 🍂 Phase 32 — Mission 1 Environment Polish (v2)

> Branch: `feat/mission-1-2-architecture` · Author: Studio collective · Date: 2026-05-25
> Status: 🟡 Active — pushes split into 32.1 (cottage assembler + bindings), 32.2 (lane v2), 32.3 (hollow interior v2), 32.4 (cozy URP volume), 32.5 (capstone + diagnostic).

This document is the single source of truth for the **Mission 1 polish v2** pass. It extends Phase 27's environment work with:

1. **Real cottages** assembled from MV modular pieces (not market-stand fallbacks).
2. **Hollow shop facade** so the Hollow door reads as a building, not a floating frame.
3. **Cozy URP post-processing volumes** for the warm autumn cinematic look.
4. **Hollow interior density** — kettle, bread, candles, cupboard, stool, ceiling herbs.
5. **Diagnostic + capstone** so the user has one menu click for everything.

---

## 1. Why Phase 32 exists

Phase 27 (the previous polish pass) was a strong baseline but had four observable gaps after playtesting:

| Gap | Root cause | Phase 32 fix |
|---|---|---|
| Lane "cottages" look like market stands | `Phase15_MedievalVillageBuilder` fuzzy-searches for "house / cottage / hut" and the MV pack ships no full house prefabs — only modular walls + roofs. Best score lands on `SM_ShopStand_01a`. | **32.1** assembles 4 cottage prefab variants from the modular kit. **32.2** instantiates them. |
| Hollow door floats in space — no shop building visible from the Lane | Phase 27.2 dressed the door but never wrapped a facade around it. | **32.2** places a 4×3 m bakery-style facade behind the door with a smoking chimney + "The Hollow" signage. |
| Bloom/vignette/warm-tint cinematic look missing on URP | `DefaultVolumeProfile.asset` is empty. No Global Volume added by Phase 27. | **32.4** authors `HearthboundLane_Volume.asset` and `HearthboundHollow_Volume.asset` profiles and drops a Global Volume into each scene. |
| Hollow interior reads as half-dressed | Phase 27.3 placed walls/floor/hearth but skipped kettle, bread, hanging herbs, cupboard. | **32.3** adds 7 new prop classes on top of Phase 27.3. |

---

## 2. The five phases at a glance

| Phase | Title | What lands | Files | Idempotent? |
|---|---|---|---|---|
| **32.1** | Cottage Assembler + Bindings | 4 cottage prefab variants + MV bindings V2 SO | `Phase32_MedievalCottageBuilder.cs`, `Phase32_VillageBindingsExtension.cs` | ✅ |
| **32.2** | Lane v2 | Full residential lane: cottages, Hollow facade, bakery sign, beehive, more atmosphere | `Phase32_LaneEnvironmentV2.cs` | ✅ |
| **32.3** | Hollow Interior v2 | Kettle on hearth, bread on shelves, hanging dried herbs, cupboard, stool, more candles | `Phase32_HollowInteriorV2.cs` | ✅ |
| **32.4** | Cozy URP Volume | Warm bloom + tonemap + vignette + color grading + film grain profiles + Global Volume in scenes | `Phase32_CozyVolumeBuilder.cs` + 2 volume profile assets | ✅ |
| **32.5** | Capstone + Diagnostic + Docs | Single-menu chain + audit report + CHANGELOG/PROGRESS updates | `Phase32_MissionOnePolishCapstone.cs`, `Phase32_Diagnostic.cs` | ✅ |

Every step is committed and pushed independently so a reviewer can bisect and the user can pull incrementally.

---

## 3. The 4 cottage variants (Phase 32.1)

Each variant is a **single self-contained .prefab** under `Assets/_Project/Prefabs/Environment/`. The Lane v2 builder (32.2) just spawns them — no per-instance hierarchy work.

```
Cottage_A_Bakery       — front south window + chimney + "BAKERY" sign + bread on sill
Cottage_B_Plain        — two front windows + banner accent
Cottage_C_Gabled       — narrower, single-window, gable end wall
Cottage_D_Corner       — door cutout + flag garland + corner-placed chimney
```

---

## 4. Lane v2 composition (Phase 32.2)

The Lane scene grows from 3 cottages (Phase 27.2) to **8 cottages** along an extended cobble path. The Hollow gets a proper facade so it reads as a real building.

**New features:** 8 cottages in a 4-row layout, Hollow facade, Doris's beehive, 3 extra lantern posts along the path, hay bale + apple basket, smoking chimneys.

---

## 5. Hollow interior v2 props (Phase 32.3)

Augments Phase 27.3 with kettle, bread, hanging herbs, cupboard, stool, candelabra, bucket — bringing the room from "dressed" to "inhabited".

---

## 6. URP cozy volume profiles (Phase 32.4)

Two mobile-safe profiles: warm dusk outdoor (Lane) + cozy interior (Hollow). Bloom, tonemapping, color grading, vignette, film grain, white balance.

---

## 7. Phase 32.5 — Master Capstone + Diagnostic

`Phase32_MissionOnePolishCapstone.cs` chains all of 32.1..32.4 plus re-runs 27.4 and 31 for safety.

`Phase32_Diagnostic.cs` audits all the placements and reports any gaps.

---

## 8. Acceptance criteria

- [ ] 4 cottage prefabs render correctly in Editor preview.
- [ ] Lane scene has 8 cottages + Hollow facade visible from player spawn.
- [ ] Hearth has kettle, west shelf has bread, ceiling has herbs.
- [ ] Global Volume in both scenes with the right profile.
- [ ] All Phase 27 interactables still work.
- [ ] Marin's Note + Phase 31 dialogue still pass.
- [ ] `Hearthbound → 🔍 Phase 32 — Diagnose Mission 1 Polish` reports 0 errors.
- [ ] CHANGELOG.md + PROGRESS.md updated, version bumped to `0.6.0-mission1-polish-v2`.

---

*End of plan.*
