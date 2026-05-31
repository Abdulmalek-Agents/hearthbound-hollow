# 🌿 System 07 — The Garden & Tea Loop (Pillar P4)

> **Owner:** Economy/Garden Systems Designer + Asset Engineer. **Implements:** the second
> interleaving system + the supply chain that makes the village visits richer.
> **Headline:** *we already own a complete farming asset (Waldemarst **HarvestGarden**, Tier
> S-2) and use it as decorative herb plots.* This system activates it into a real
> grow→harvest→brew→use loop — the cozy genre's most-loved daily ritual.

---

## 1. Why a garden, and why now

The garden is the cozy player's *gentle productive loop* — the thing you check first each
morning ("are my crops ready?"). Stardew, Coral Island, Wylde Flowers, Travellers Rest all
anchor on it. Hearthbound's twist: **the garden isn't the point — it's the supply.** Herbs
become **teas**, and teas are **tools** that change how villager visits and memory craft play
out. That makes the garden *matter to the heart of the game* instead of being a side-sim.

The current build already has the bones:
- `04_Mission02_Garden` scene with **3 Lavender + 3 Valerian plants + 2 empty plots**.
- `HerbHarvestInteractable.cs`, `KettleInteractable.cs`, `TeaBrewingUI.cs`, `MemoryHerb.cs`.
- `HerbHarvestedEvent` / `TeaBrewedEvent` already on the EventBus.
- `VillageState.harvestedHerbIds` already persisted.

What's missing is the **loop**: planting, ripening over days, and teas with *real effects.*

---

## 2. The grow loop (activate the owned asset)

```
   SEED  ──plant──▶  SPROUT  ──(1 day)──▶  GROWING  ──(1–2 days)──▶  RIPE  ──harvest──▶  HERB
     ▲                                                                                    │
     └──────────────── buy seeds at Market Day / festival stall ◀──── coin ───────────────┘
```

- **Plant** in a bed (the 2 empty plots + any bought via `GARDEN_BED_03`).
- **Ripen over real days** driven by `DailyLoopService` (`DayStartedEvent` advances growth).
- **Harvest** when ripe → herb to inventory (existing `HerbHarvestInteractable`).
- **Water** (optional cozy ritual; in Gentle Mode, plants never wilt — watering is just nice).

Use the **HarvestGarden** pack's prefabs/controllers for the visual growth stages instead of
static plants. The pack already does staged growth meshes — we drive its stage from our
`GardenService` day tick.

### Data: `HerbSO` (extend existing `MemoryHerb`) + `GardenBedState`
```csharp
// VillageState additions (persisted):
[System.Serializable] public class GardenBedState {
    public string bedId;          // "BED_LAVENDER_1"
    public string plantedHerbId;  // "" = empty
    public int dayPlanted;        // ripens at dayPlanted + herb.daysToRipe
    public bool watered;
}
public List<GardenBedState> gardenBeds = new();   // + reset/guard like other lists
```

`GardenService` (Mission asmdef) listens to `DayStartedEvent`, advances each bed, and writes
"Lavender — ready to harvest" / "Valerian — 1 day to ripen" lines into the morning `DayAgenda`.

---

## 3. Teas as tools (the part that makes the garden matter)

Brewing already exists (`KettleInteractable` + `TeaBrewingUI`). We give each tea a **gentle,
opt-in effect** that ripples into the other systems — never required, always helpful:

| Tea | Herb | Effect (cozy, opt-in) | Touches |
|---|---|---|---|
| **Lavender** | Lavender | A guarded villager **opens up** — reveals one extra memory facet / softens the visit | Request visits (P2), Echo (P6) |
| **Valerian** | Valerian | **Steadies your hand** — Cleanse core-tolerance widens; calmer bench | Workbench (P5) |
| **Sage** *(new bed)* | Sage | **Clears the air** — reveals a hidden Echo hint on a held memory | Codex (P6) |
| **Mugwort** *(new bed)* | Mugwort | **Deepens dreams** — that night's Memory Dream gets a richer variant | Dreams (P6) |
| **Chamomile** *(new bed)* | Chamomile | A cup *for you* — Pickle warms; a small daily comfort beat | Pickle / mood |

This is already half-designed in the Mission 2 guide (Lavender nudges Listen; Valerian eases
Cleanse). We **generalize it into a tea-tool system** and expand the herb roster as garden
beds unlock — giving the player a reason to grow *variety*, not just one crop.

> **Cozy guardrail:** teas only ever *help* or *flavor*. No tea is ever required; skipping the
> garden entirely still completes every request (Auto-paths intact).

---

## 4. The tea economy (a second, gentle income)

- Brew extra teas → **sell at the shop counter / Market Day** for coin (feeds P3).
- Some villagers *request* a specific tea (a Request Board entry: "Tomek can't sleep — valerian?").
- Festival stalls buy seasonal teas at a premium (Almanac tie-in).

This closes a satisfying mini-economy: **garden → tea → coin → seeds/upgrades → bigger garden.**
The classic cozy compounding wheel, themed to a memory-keeper's apothecary.

---

## 5. `GardenService` skeleton — `Scripts/Mission/GardenService.cs`

```csharp
// SPDX-License-Identifier: MIT
using System.Collections.Generic;
using UnityEngine;
using HearthboundHollow.Core;
using HearthboundHollow.Memory;

namespace HearthboundHollow.Mission
{
    public class GardenService : MonoBehaviour
    {
        [SerializeField] private List<MemoryHerb> herbCatalog = new(); // each has daysToRipe
        private VillageState _vs;

        private void Awake() { _vs = ServiceLocator.Get<VillageState>(); ServiceLocator.Register(this); }
        private void OnEnable()  => EventBus.Subscribe<DayStartedEvent>(OnDay);
        private void OnDisable() => EventBus.Unsubscribe<DayStartedEvent>(OnDay);

        private void OnDay(DayStartedEvent e)
        {
            if (_vs == null) return;
            var agenda = DailyLoopService.Instance?.CurrentAgenda;
            foreach (var bed in _vs.gardenBeds)
            {
                if (string.IsNullOrEmpty(bed.plantedHerbId)) continue;
                var herb = herbCatalog.Find(h => h.name == bed.plantedHerbId);
                if (herb == null) continue;
                int ripeDay = bed.dayPlanted + Mathf.Max(1, herb.daysToRipe);
                int left = ripeDay - _vs.currentDayIndex;
                agenda?.gardenNotes.Add(left <= 0
                    ? $"{herb.displayName} — ready to harvest"
                    : $"{herb.displayName} — {left} day(s) to ripen");
            }
        }

        public bool IsRipe(GardenBedState bed)
        {
            var herb = herbCatalog.Find(h => h.name == bed.plantedHerbId);
            return herb != null && _vs.currentDayIndex >= bed.dayPlanted + herb.daysToRipe;
        }
    }
}
```

(Harvest/plant calls live on the existing `HerbHarvestInteractable`, which now checks
`GardenService.IsRipe` and clears the bed on harvest.)

---

## 6. Step-by-step build order (Phase 65 in the roadmap)

1. Add `GardenBedState` + `gardenBeds` to `VillageState` (+ reset/guard); add `daysToRipe`/`teaEffect` fields to `MemoryHerb`.
2. Add `GardenService` (Mission asmdef); wire `DayStartedEvent` growth + Agenda lines.
3. Swap the static garden plants for **HarvestGarden** staged-growth prefabs; drive stage from bed state.
4. Extend `HerbHarvestInteractable` to plant/harvest against beds; extend `TeaBrewingUI` for the new herbs.
5. Implement tea-tool effects as small listeners (Lavender→visit, Valerian→cleanse tolerance, etc.).
6. Add seed-buying (Market Day stall) + tea-selling at the counter.
7. Chain builder into `🚀 Build Everything`; update `PROGRESS.md`.

---

## 7. Acceptance criteria

- [ ] Planting a seed and ending the day **advances growth**; ripe herbs are harvestable.
- [ ] The morning Agenda lists garden status accurately.
- [ ] Each tea produces its documented, opt-in effect; skipping tea never blocks anything.
- [ ] Selling tea earns coin; buying seeds spends it (the wheel closes).
- [ ] HarvestGarden staged-growth visuals render correctly in the Garden scene.
- [ ] In Gentle Mode, plants never wilt and watering is purely optional.

---

## 8. Why this is the cozy daily-check ritual

"Did my lavender ripen?" is the first thought a cozy player has each morning. It's a tiny,
warm, zero-stakes reason to open the game — and it *feeds the heart of the game* (better
visits, easier craft, richer dreams). We already own the asset. We already have the scene,
the herbs, the kettle, the events. **This pillar is mostly wiring what's already bought.**

---

*System 07 v1.0 — Next: `08_SYSTEM_WORKSHOP_VARIETY.md`.*
