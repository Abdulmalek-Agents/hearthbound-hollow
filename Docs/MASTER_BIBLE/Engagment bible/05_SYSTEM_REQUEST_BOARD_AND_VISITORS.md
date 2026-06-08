# 🗺️👨‍💻 System 05 — The Request Board & Rotating Visitors (Pillar P2 + P7)

> **Owner:** Lead Game Designer + Senior Unity Dev. **Implements:** the replayability engine.
> **Goal:** turn "2 villagers, then the game ends" into **an endless, gentle, varied stream
> of people who need you** — the single biggest retention lever in the plan. This is the
> system that converts a 75-minute corridor into a sandbox you can live in for 40 hours.

---

## 1. The core idea: a queue that never empties

Stardew never runs out of *things to do tomorrow*. Hearthbound must never run out of
*people who need a memory kept tomorrow*. The **Request Board** is a small in-fiction notice
board by the Hollow door. Each morning it holds **2–4 requests** drawn from a weighted pool:

```
   ┌──────────────  THE HOLLOW — REQUESTS  ──────────────┐
   │                                                      │
   │  🍞  Doris        "A sweet thing to ask"   [warm]    │  ← hand-sealed villager, arc beat
   │  🧥  A stranger   "Something heavy"        [weighted] │  ← procedural villager (Vignette Lib)
   │  🐝  The Apiary   "Honey gone sad"         [seasonal] │  ← seasonal/almanac request
   │                                                      │
   │      Tap a request to open the day's visit.          │
   └──────────────────────────────────────────────────────┘
```

The player **chooses who to see, and in what order** (Pillar P2 + agency). Requests not taken
today **roll to tomorrow** (cozy — nothing is lost, no FOMO pressure). New requests surface
as trust/echoes/days accrue. **The board is the content faucet.**

---

## 2. Three request sources (so content scales without infinite hand-writing)

This is exactly the Depth Bible's already-designed solution (codices 02 + 09), un-deferred:

| Source | Quality | Count | Notes |
|---|---|---|---|
| **Hand-sealed arcs** | Spiritfarer-tier hand-written | 12 villagers × ~14 beats | Doris, Gerrold, + the 10 others over time. The *prestige* content. |
| **Procedural villagers** | ~76% quality via Vignette Library | 18 villagers | 4 hand-authored anchors + recombined facets (`02_NARRATIVE_BIBLE §7`). The *volume* content. |
| **Seasonal / Almanac** | Hand-written set-pieces | ~1 per festival | Honey Festival, Festival of the Hearth, the travelling bard. The *anticipation* content. |

**The discipline (protect the writing):** hand-sealed villagers are never procedural. The
Vignette Library produces the *background texture* of village life, not the *gut-punch* beats.
Players feel a full town; the team writes a focused core. This is the writing-budget cliff
solution the original Expert Review demanded (`EXPERT_REVIEW_EN §5.1`).

---

## 3. Data model (ScriptableObjects — matches the existing `Scripts/Memory/*SO.cs` pattern)

### 3A — `RequestSO` (`Scripts/Memory/RequestSO.cs`)
```csharp
// SPDX-License-Identifier: MIT
using System.Collections.Generic;
using UnityEngine;

namespace HearthboundHollow.Memory
{
    public enum RequestKind { TakeMemory, ReturnMemory, RestoreMemory, FindMemory, JustVisit }

    [CreateAssetMenu(menuName = "Hearthbound/Requests/Request", fileName = "Request_")]
    public class RequestSO : ScriptableObject
    {
        [Header("Identity")]
        public string requestId;             // "DOR_002_WEDDING_HONEY"
        public VillagerSO villager;          // existing SO
        public MemoryNodeSO memory;          // existing SO (the orb at stake)
        public RequestKind kind = RequestKind.TakeMemory;

        [Header("Board presentation (fiction-voice, no numbers)")]
        public string boardTeaser;           // "A sweet thing to ask"
        [TextArea] public string openingLine;

        [Header("Gating (all optional — empty = always eligible)")]
        public int minDayIndex = 0;
        public int minTrust = 0;             // villager trust gate
        public List<string> requiresFlags;   // VillageState bools that must be true
        public List<string> requiresEchoIds; // echoes that must be discovered first
        public List<string> blockedByFlags;  // hide once these are set (arc already advanced)

        [Header("Weighting")]
        [Tooltip("Higher = more likely to surface on a given morning when eligible.")]
        public float weight = 1f;
        [Tooltip("Hand-sealed arc beats should be 'pinned' so they always appear when eligible.")]
        public bool pinnedArcBeat = false;
    }
}
```

### 3B — `RequestPoolSO` (`Scripts/Memory/RequestPoolSO.cs`)
A simple container the designer fills with all `RequestSO`s. Authored once; the service
samples from it each morning.

```csharp
[CreateAssetMenu(menuName = "Hearthbound/Requests/Request Pool", fileName = "RequestPool")]
public class RequestPoolSO : ScriptableObject
{
    public List<RequestSO> allRequests = new();
}
```

---

## 4. `RequestBoardService` (the faucet) — `Scripts/Mission/RequestBoardService.cs`

Lives in the **Mission** asmdef (it references Memory + Core + UI, exactly like the directors).
On `DayStartedEvent` it (a) filters the pool by gating, (b) keeps yesterday's untaken
requests, (c) weight-samples up to N, (d) writes the visitor lines into the `DayAgenda`.

```csharp
// SPDX-License-Identifier: MIT
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HearthboundHollow.Core;
using HearthboundHollow.Memory;

namespace HearthboundHollow.Mission
{
    public class RequestBoardService : MonoBehaviour
    {
        [SerializeField] private RequestPoolSO pool;
        [SerializeField] private int maxRequestsPerDay = 3;

        public readonly List<RequestSO> Today = new();
        private VillageState _vs;

        private void OnEnable()  => EventBus.Subscribe<DayStartedEvent>(OnDayStarted);
        private void OnDisable() => EventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);

        private void OnDayStarted(DayStartedEvent e)
        {
            _vs = ServiceLocator.Get<VillageState>();
            if (pool == null || _vs == null) { Hh.Warn(LogCategory.Mission, "[Board] pool/state missing"); return; }

            // 1. Carry over untaken requests (cozy — nothing is lost).
            var carried = Today.Where(r => IsEligible(r)).ToList();
            Today.Clear();
            Today.AddRange(carried);

            // 2. Always include eligible pinned arc beats.
            foreach (var r in pool.allRequests.Where(r => r.pinnedArcBeat && IsEligible(r) && !Today.Contains(r)))
                Today.Add(r);

            // 3. Weight-sample the rest up to the daily cap.
            var rng = new System.Random(unchecked(e.DayIndex * 73856093));
            var candidates = pool.allRequests
                .Where(r => !r.pinnedArcBeat && IsEligible(r) && !Today.Contains(r))
                .ToList();
            while (Today.Count < maxRequestsPerDay && candidates.Count > 0)
            {
                var pick = WeightedPick(candidates, rng);
                Today.Add(pick);
                candidates.Remove(pick);
            }

            // 4. Feed the morning Agenda.
            var loop = DailyLoopService.Instance;
            if (loop?.CurrentAgenda != null)
                foreach (var r in Today)
                    loop.CurrentAgenda.visitors.Add($"{r.villager.displayName} — \"{r.boardTeaser}\"");

            Hh.Log(LogCategory.Mission, $"[Board] Day {e.DayIndex}: {Today.Count} requests.");
        }

        private bool IsEligible(RequestSO r)
        {
            if (r == null) return false;
            if (_vs.currentDayIndex < r.minDayIndex) return false;
            if (r.blockedByFlags != null && r.blockedByFlags.Any(VillageStateFlags.IsSet)) return false;
            if (r.requiresFlags != null && !r.requiresFlags.All(VillageStateFlags.IsSet)) return false;
            if (r.requiresEchoIds != null &&
                !r.requiresEchoIds.All(id => _vs.revealedEchoConnectionIds.Contains(id))) return false;
            return true; // trust gate via VillageStateFlags helper (see note)
        }

        private static RequestSO WeightedPick(List<RequestSO> list, System.Random rng)
        {
            float total = list.Sum(r => Mathf.Max(0.01f, r.weight));
            double roll = rng.NextDouble() * total, acc = 0;
            foreach (var r in list) { acc += Mathf.Max(0.01f, r.weight); if (roll <= acc) return r; }
            return list[list.Count - 1];
        }
    }
}
```

> **`VillageStateFlags` helper:** add a tiny static map in Core that resolves a string flag id
> (e.g. `"firstMoralChoiceMade"`, `"askedAboutPredecessor"`) to the matching `VillageState`
> bool via reflection or a switch. This keeps designers authoring requests by string id without
> touching code. (Same pattern the Yarn bridge already uses.)

---

## 5. The visit flow (reuse the existing directors!)

The genius is that **the existing `Mission01Director` / `Mission02Director` already implement
a complete villager visit** (greet → dialogue choices → transaction → mini-game → reaction).
Generalize that into a reusable `VisitDirector` driven by a `RequestSO`:

1. Player taps a request on the Board (or talks to the waiting villager).
2. `VisitDirector.Begin(request)` spawns the villager (BoZo reskin), runs the request's Yarn
   node, presents choices, hands off to the workbench with `request.memory`, and applies the
   tariff/ripple on completion (the `RippleEngine` already exists).
3. On finish, set the request's `blockedByFlags` so its arc advances; publish a
   `RequestResolvedEvent` so the Wall/Ledger update.

**Mission01Director and Mission02Director become the first two `RequestSO` configurations** of
this generalized director — proving the system with the content that already ships, then
scaling to N villagers by authoring data, not code.

---

## 6. The Almanac (Pillar P7 — anticipation)

A small `AlmanacSO` lists dated events keyed to `currentDayIndex` (mod season length):

| Day pattern | Event | Effect |
|---|---|---|
| Every 5th day | **Market Day** | A travelling-cart villager appears with rare seeds / decor (the Stardew Friday cart). |
| Day 7 of a month | **A villager's birthday** | A warm one-off visit; gift opportunity. |
| Day 10 (month end) | **Festival** (Honey / Hearth) | Village scene redressed; a seasonal request; a group beat. |
| Bard days | **Idris passes through** | A rare "memory of the road" request; he never stays. |

The Almanac feeds **two** things: the morning Agenda ("3 days to the Honey Festival") and the
Tomorrow Tease. **Anticipation is engagement.** Players plan around the calendar — that's the
self-set-goal behavior (Pillar P2) we want.

---

## 7. Step-by-step build order (Phase 62 in the roadmap)

1. Add `RequestSO` + `RequestPoolSO` (Memory asmdef). Author a `RequestPool` asset.
2. Author Doris + Gerrold as the first two `RequestSO`s (pinned arc beats); add 4–6 procedural-villager stubs using existing Vignette seeds.
3. Add `VillageStateFlags` helper (Core).
4. Add `RequestBoardService` (Mission asmdef) + `RequestResolvedEvent` (in `EngagementEvents.cs`).
5. Generalize the directors into `VisitDirector`; re-express M1/M2 as data. (Keep the old directors until parity is proven, then retire.)
6. Add `RequestBoardUI` (the notice board, Bamao parchment) + an interactable by the Hollow door.
7. Add `AlmanacSO` + feed Agenda/Tease.
8. Chain the new builder into `🚀 Build Everything`; update `PROGRESS.md`.

---

## 8. Acceptance criteria

- [ ] Each morning the Board shows 2–4 eligible requests; untaken ones carry to tomorrow.
- [ ] Doris & Gerrold always appear when their arc gate is open (pinned).
- [ ] Taking a request runs a full visit and never blocks if skipped (cozy).
- [ ] The pool can be expanded by authoring SOs **with zero code changes** (data-driven test: add a dummy request asset, it appears).
- [ ] The Almanac surfaces a festival countdown in the Agenda + Tease.
- [ ] No request ever *punishes* non-completion; refusal/defer always honored.

---

## 9. Why this is the keystone

P1 gives the day a tomorrow. **P2 gives the tomorrow a reason.** With a never-empty board of
people who need you — some warm, some weighted, some funny, some seasonal — the player always
has a gentle answer to "what now?", and the answer is *different enough each day* to sustain
the hundreds of hours cozy players want to give. This is the system that makes Hearthbound a
*game* and not a *short film.*

---

*System 05 v1.0 — Next: `06_SYSTEM_HOLLOW_PROGRESSION.md`.*
