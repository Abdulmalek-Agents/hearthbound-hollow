# 📈 System 06 — "My Hollow": Ownership, Upgrades & the Economy (Pillar P3)

> **Owner:** Systems & Progression Architect + Asset Engineer. **Implements:** the compounding
> spend-sink that makes coin meaningful and the shop feel *owned and growing.*
> **Goal:** answer "why earn coin?" and "what is mine?" — the two questions the current build
> never answers.

---

## 1. The problem this fixes

Right now: you spend 4 coppers once, the shop is fixed set-dressing, and `VillageState.coin`
is a number that does nothing. Stardew's entire compulsion loop is *earn → upgrade → earn
faster → upgrade bigger.* We need that wheel — but cozy: **upgrades are warmth, not power.**
A new shelf isn't a DPS boost; it's *room for one more friend's memory.* A relit upstairs
room isn't a stat; it's *the Hollow waking up around you.*

---

## 2. The economy (gentle, never punishing)

| Currency | Source | Sink | Exists? |
|---|---|---|---|
| **Coin** | Memory transactions, tea sales, festival stalls | Hollow upgrades, garden expansion, decor, seeds | ✅ `VillageState.coin` |
| **Cinder** | Listen-path acts (Vow 7) | Confession-booth & special restorations | ✅ `VillageState.cinder` |
| **Materials** | Foraging on village walks, garden by-products | Crafting decor & tools | ⬜ add `List<string> materials` |

**Anti-grind discipline (Cozy Contract):** coin gain is generous, prices are gentle, and
**nothing is ever gated behind grind** — every upgrade is affordable within a few cozy days,
and the *story* never requires any upgrade. Upgrades are pull, not toll. (Matches Depth Bible
`10_ECONOMY_AND_REPUTATION` anti-grind safeguards.)

---

## 3. Visible progression (the D-076 relaxation in action)

The critique's Root Cause #2 was hiding all feedback. Here's where we fix it — **cozily**:

- A small **coin purse** indicator (warm, top-corner, fades when idle) — players see earnings.
- A **Hollow level** shown only as the shop *visibly filling in* (more shelves, lit rooms, decor) — *the progression IS the art.* This is the best of both worlds: visible growth (the critique's demand) with zero anxiety-inducing numbers (the Cordray spirit).
- The Evening Ledger's "The Hollow grew:" section names today's additions.

> **Rule:** show *abundance and growth* (coin earned, shelves filled, collection %). Never
> show *scarcity or deficit* (no "you need 12 more," no red, no countdowns). Growth is cozy;
> debt is not.

---

## 4. The upgrade catalog (`HollowUpgradeSO`)

`Scripts/Memory/HollowUpgradeSO.cs` (or a new `Scripts/Progression/` folder + asmdef if we
want isolation; Memory is fine for now):

```csharp
// SPDX-License-Identifier: MIT
using System.Collections.Generic;
using UnityEngine;
namespace HearthboundHollow.Memory
{
    public enum UpgradeCategory { Shelf, Room, Tool, Decor, GardenBed }

    [CreateAssetMenu(menuName = "Hearthbound/Progression/Hollow Upgrade", fileName = "Upgrade_")]
    public class HollowUpgradeSO : ScriptableObject
    {
        public string upgradeId;             // "SHELF_WINDOW_01"
        public UpgradeCategory category;
        public string displayName;           // "Shelf by the window"
        [TextArea] public string flavor;     // "A shop with full shelves keeps better company."
        public int coinCost = 10;
        public List<string> requiresUpgradeIds; // simple prerequisite chain
        [Tooltip("Prefab/marker activated in the Hollow scene when purchased.")]
        public string sceneMarkerId;         // a named GameObject the builder pre-places, hidden until bought
        [Tooltip("e.g. +2 shelf slots, +1 garden bed. Read by the relevant system.")]
        public int capacityDelta = 0;
    }
}
```

### Starter catalog (Mission-slice scope, expandable)
| id | Category | Cost | Effect | Flavor hook |
|---|---|---|---|---|
| `SHELF_WINDOW_01` | Shelf | 10 | +3 memory display slots | The bare shelf the Agenda nudges you toward |
| `SHELF_HEARTH_02` | Shelf | 18 | +3 slots | "The hearth wall wants company." |
| `ROOM_UPSTAIRS_LIGHT` | Room | 30 | Unlocks the Reading Nook (Marin's letters — `readingNookVisited` already in `VillageState`) | The predecessor-mystery pull |
| `GARDEN_BED_03` | GardenBed | 14 | +1 herb bed (the 2 empty plots already in the Garden scene!) | "Two beds lie fallow." |
| `TOOL_SOFT_CLOTH` | Tool | 12 | Polish gains clarity a touch faster (gentle skill aid, never required) | "Marin's old polishing cloth." |
| `DECOR_PICKLE_CUSHION` | Decor | 8 | Pickle naps here; +small daily Pickle-approval | Pure cozy |

---

## 5. `HollowProgressionService` — `Scripts/Mission/HollowProgressionService.cs`

Owns purchase logic, persists purchased ids in `VillageState`, and activates scene markers.

```csharp
// SPDX-License-Identifier: MIT
using System.Collections.Generic;
using UnityEngine;
using HearthboundHollow.Core;
using HearthboundHollow.Memory;

namespace HearthboundHollow.Mission
{
    public class HollowProgressionService : MonoBehaviour
    {
        [SerializeField] private List<HollowUpgradeSO> catalog = new();
        private VillageState _vs;

        private void Awake()
        {
            _vs = ServiceLocator.Get<VillageState>();
            ServiceLocator.Register(this);
        }

        public bool CanAfford(HollowUpgradeSO u) => _vs != null && _vs.coin >= u.coinCost;
        public bool IsPurchased(string id) => _vs != null && _vs.purchasedUpgradeIds.Contains(id);

        public bool Purchase(HollowUpgradeSO u)
        {
            if (_vs == null || u == null || IsPurchased(u.upgradeId) || !CanAfford(u)) return false;
            if (u.requiresUpgradeIds != null)
                foreach (var req in u.requiresUpgradeIds)
                    if (!IsPurchased(req)) return false;

            _vs.coin -= u.coinCost;
            _vs.purchasedUpgradeIds.Add(u.upgradeId);
            ApplyToScene(u);
            EventBus.Publish(new HollowUpgradePurchasedEvent(u.upgradeId));   // add to EngagementEvents.cs
            Hh.Log(LogCategory.Mission, $"[Hollow] Purchased {u.upgradeId}; coin now {_vs.coin}.");
            return true;
        }

        /// <summary>Re-apply all purchased upgrades on scene load (so the Hollow looks right after a save).</summary>
        public void ReapplyAll()
        {
            foreach (var u in catalog)
                if (IsPurchased(u.upgradeId)) ApplyToScene(u);
        }

        private void ApplyToScene(HollowUpgradeSO u)
        {
            if (string.IsNullOrEmpty(u.sceneMarkerId)) return;
            var marker = GameObject.Find(u.sceneMarkerId);
            if (marker != null) marker.SetActive(true);   // builder pre-places, hidden; purchase reveals
        }
    }
}
```

Add to `VillageState`: `public List<string> purchasedUpgradeIds = new();` (+ clear in
`ResetToDefault` + null-guard in `OnEnable`, exactly like the existing list fields).

---

## 6. The shop UI (`HollowShopUI`)

A Bamao parchment "ledger of improvements" the player opens at a workbench/desk interactable.
Lists catalog entries with: name, flavor, cost, `[ Improve ]` button (greyed cozily if
unaffordable — *grey, never red*). Purchase → the scene marker reveals with a soft particle
+ chime → Pickle approves. Reuse `Heat - Complete Modern UI` or Bamao panels (both imported).

---

## 7. Asset reuse (Asset Engineer note)

We already have the props to *show* growth — no new art needed for the slice:
- **Shelves / cottages / hearth dressing** — Phase 32 already built cottage + Hollow-interior
  prefabs (kettle, bread, herbs, cupboard, candelabra). Pre-place "upgrade" variants **hidden**,
  reveal on purchase.
- **Garden beds** — the Garden scene already has **2 empty plots** reserved "for Mission 3+."
  `GARDEN_BED_03` simply activates one. Zero new art.
- **Reading Nook** — `VillageState.readingNookVisited` + `letterFragmentsRead` already exist
  (Phase 52 seam). `ROOM_UPSTAIRS_LIGHT` lights it.

> This is the payoff of Root Cause #5: the growth content is already in the repo, pre-placed
> and hidden. Progression is mostly *revealing what's already built.*

---

## 8. Step-by-step build order (Phase 64 in the roadmap)

1. Add `purchasedUpgradeIds` + `materials` to `VillageState` (+ reset/guard).
2. Add `HollowUpgradeSO`; author the starter catalog (6 entries above).
3. Pre-place hidden scene markers (`SetActive(false)`) for each upgrade in the Hollow/Garden builders.
4. Add `HollowProgressionService` (Mission) + `HollowUpgradePurchasedEvent`.
5. Add `HollowShopUI` (Bamao/Heat panel) + desk interactable.
6. Add the cozy coin-purse HUD element + extend the Evening Ledger "The Hollow grew:" section.
7. Make memory transactions actually pay coin (wire the existing tariff/`RippleEngine` to `+= coin`).
8. Chain builder into `🚀 Build Everything`; `ReapplyAll()` on Hollow scene load; update `PROGRESS.md`.

---

## 9. Acceptance criteria

- [ ] Completing a memory transaction visibly **earns coin** (purse ticks up, Ledger logs it).
- [ ] Opening the shop UI and buying a shelf **visibly adds shelf slots** that persist across save/load.
- [ ] Buying the garden bed **activates the reserved empty plot**.
- [ ] No upgrade is ever *required* to finish a request or the story (Cozy Contract).
- [ ] Unaffordable items show **grey + gentle "soon" copy**, never red/scary.
- [ ] `🚀 Build Everything` twice = no duplicate markers; purchased state re-applies on load.

---

## 10. Why ownership is the cozy hook

Stardew players post screenshots of *their farm*. Animal Crossing players post *their island*.
The screenshot-worthy "this is mine and I made it" feeling is **half of cozy retention** and
it's the easiest TikTok-virality engine the marketing plan already counts on. A Hollow that
visibly fills with the memories you kept and the rooms you relit is the player's *story made
of objects* — and it costs us mostly *revealing prefabs we already built.*

---

*System 06 v1.0 — Next: `07_SYSTEM_GARDEN_TEA_ECONOMY.md`.*
