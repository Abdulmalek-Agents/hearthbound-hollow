// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / RequestBoardService  (Engagement Pillar P2 + P6)
//
// THE NEVER-EMPTY CONTENT FAUCET (Docs/Engagement_Bible/05).
//
// Each morning (DayStartedEvent) this builds the day's Request Board and writes:
//   • agenda.tickets  — actionable RequestTickets the RequestBoardUI opens into visits.
//   • agenda.visitors — the teaser strings the morning Agenda card shows.
//
// SOURCES (Docs/Engagement_Bible/05 §2):
//   • Authored RequestSOs in an optional Resources/RequestPool asset (prestige content).
//   • Built-in HAND-SEALED ARCS (Phase 68b) — Doris, Gerrold, Mariska each have an
//     ordered multi-beat arc; the board offers the NEXT UNRESOLVED beat per villager
//     (pinned), so helping someone today unlocks their next chapter tomorrow. This is
//     the "relationships deepen over days" feel — the cozy-retention heart — delivered
//     with zero new save fields (arc progress rides VillageState.resolvedRequestIds).
//   • Built-in WALK-IN roster — rotating procedural villagers that fill the rest, so
//     the board is varied and never empty even after the arcs are exhausted.
//
// SAFE DROP: self-installing, observer-only on DayStartedEvent. No scene edit, no builder.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HearthboundHollow.Core;
using HearthboundHollow.Memory;

namespace HearthboundHollow.Mission
{
    public class RequestBoardService : MonoBehaviour
    {
        public static RequestBoardService Instance { get; private set; }

        [SerializeField] private int maxRequestsPerDay = 3;
        [SerializeField] private int maxArcBeatsPerDay = 2;   // keep room for walk-in variety
        private RequestPoolSO _pool;
        private VillageState _vs;

        public readonly List<RequestTicket> Today = new();

        // ── Hand-sealed arcs: ordered beats per villager (id, teaser, openingLine) ──
        private sealed class Arc
        {
            public string villagerId, name;
            public (string id, string teaser, string opening)[] beats;
            public Arc(string vid, string nm, (string, string, string)[] b) { villagerId = vid; name = nm; beats = b; }
        }

        private static readonly Arc[] Arcs =
        {
            new Arc("doris", "Doris", new[]
            {
                ("doris_1", "a sweet thing to ask",
                    "\u201cThere's a morning I keep losing \u2014 the first loaves I ever sold, still warm, the lane still empty. Would you hold onto it for me?\u201d"),
                ("doris_2", "a harder loaf",
                    "\u201cYou kept the first one so gently. There's another \u2014 the winter the oven cracked and we baked anyway. It matters more than it sounds.\u201d"),
                ("doris_3", "the recipe she never wrote down",
                    "\u201cMy mother's honey-bread. I can taste it but I never could read her hand. Keep the tasting of it, and maybe I'll bake it true again.\u201d"),
            }),
            new Arc("gerrold", "Gerrold", new[]
            {
                ("gerrold_1", "he sounded steadier today",
                    "\u201cI'm not here to give anything up. I just\u2026 wanted to sit where someone remembers her too.\u201d"),
                ("gerrold_2", "the blue shawl",
                    "\u201cThere's a shawl, and a shoulder, and a cold lane home. I'd like it kept before it frays any further.\u201d"),
                ("gerrold_3", "the last good morning",
                    "\u201cThe last morning she was wholly herself. I've been afraid to look at it alone. With you here\u2026 maybe today.\u201d"),
            }),
            new Arc("mariska", "Old Mariska", new[]
            {
                ("mariska_1", "a riddle, as always",
                    "\u201cI'll not tell you what it is. Only that it's blue, and it's cold, and it was kind once. Keep it and you'll see.\u201d"),
                ("mariska_2", "the warm half of the riddle",
                    "\u201cYou kept the cold half well. The warm half is harder \u2014 come back when your hands are sure of themselves.\u201d"),
            }),
        };

        // Walk-in roster — procedural texture that fills the board (villagerId, name, teaser, opening).
        private static readonly (string id, string name, string teaser, string opening)[] WalkIns =
        {
            ("bram",    "Bram the goatherd", "a memory that won't sit still",
                "\u201cThere's a hillside afternoon that keeps slipping. Goats, wind, my brother laughing. Can you keep it from going?\u201d"),
            ("inkwell", "Ms. Inkwell", "a letter she can't bring herself to read",
                "\u201cHe wrote me every winter. I've read them all but the last. Keep the memory of his hand, and maybe I'll be braver in spring.\u201d"),
            ("tomek",   "Tomek the innkeeper", "something he keeps behind the bar",
                "\u201cClosing time, the lamps low, a song nobody asked for. The good nights blur. Hold one clear for me, would you?\u201d"),
            ("petra",   "Petra the weaver", "a colour she's forgetting",
                "\u201cMy mother dyed wool the exact red of the autumn we left the coast. I can almost\u2026 no. Keep it before it greys.\u201d"),
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInstall()
        {
            if (Instance != null) return;
            var go = new GameObject("_HHRequestBoard");
            DontDestroyOnLoad(go);
            go.AddComponent<RequestBoardService>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            ServiceLocator.Register(this);
            _pool = Resources.Load<RequestPoolSO>("RequestPool");   // optional override
            EventBus.Subscribe<DayStartedEvent>(OnDayStarted);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
            if (Instance == this) Instance = null;
        }

        private void OnDayStarted(DayStartedEvent e)
        {
            var agenda = DailyLoopService.Instance != null ? DailyLoopService.Instance.CurrentAgenda : null;
            if (agenda == null) return;
            _vs ??= ServiceLocator.Get<VillageState>();

            BuildTickets(e.DayIndex);

            agenda.tickets.Clear();
            agenda.tickets.AddRange(Today);
            foreach (var t in Today)
            {
                string teaser = string.IsNullOrEmpty(t.teaser) ? t.villagerName : $"{t.villagerName} \u2014 \u201c{t.teaser}\u201d";
                if (!agenda.visitors.Contains(teaser)) agenda.visitors.Add(teaser);
            }

            Hh.Log(LogCategory.Mission, $"[RequestBoard] Day {e.DayIndex}: {Today.Count} request(s) on the board.");
        }

        // ───── Ticket assembly ─────────────────────────────────

        private void BuildTickets(int dayIndex)
        {
            Today.Clear();
            int want = Mathf.Clamp(maxRequestsPerDay, 1, 6);

            // Authored pool wins when present + non-empty.
            if (_pool != null && _pool.allRequests != null && _pool.allRequests.Count > 0)
            {
                var eligible = _pool.allRequests.Where(r => r != null && IsEligible(r)).ToList();
                foreach (var r in eligible.Where(r => r.pinnedArcBeat)) AddTicket(FromSO(r));
                var rng = new System.Random(unchecked(dayIndex * 73856093 + 19349663));
                var rest = eligible.Where(r => !r.pinnedArcBeat).ToList();
                while (Today.Count < want && rest.Count > 0)
                {
                    var pick = WeightedPick(rest, rng);
                    AddTicket(FromSO(pick));
                    rest.Remove(pick);
                }
                if (Today.Count > 0) return;
            }

            // ── Built-in: hand-sealed arc beats first (the relationship spine) ──
            var arcTickets = new List<RequestTicket>();
            for (int a = 0; a < Arcs.Length; a++)
            {
                var arc = Arcs[a];
                for (int i = 0; i < arc.beats.Length; i++)
                {
                    if (IsResolved(arc.beats[i].id)) continue;     // this beat already kept
                    arcTickets.Add(new RequestTicket
                    {
                        requestId    = arc.beats[i].id,
                        villagerId   = arc.villagerId,
                        villagerName = arc.name,
                        teaser       = arc.beats[i].teaser,
                        openingLine  = arc.beats[i].opening,
                        kind         = "TakeMemory",
                        coinReward   = 5 + i,                       // later beats give a touch more
                        pinnedArc    = true,
                    });
                    break;                                          // only the CURRENT beat per villager
                }
            }
            // Rotate which arcs surface so a day doesn't always lead with Doris.
            int arcShow = Mathf.Min(Mathf.Max(1, maxArcBeatsPerDay), arcTickets.Count);
            for (int k = 0; k < arcShow; k++)
                AddTicket(arcTickets[(dayIndex + k) % arcTickets.Count]);

            // ── Fill remaining slots with rotating walk-ins (procedural texture) ──
            int n = WalkIns.Length;
            for (int k = 0; Today.Count < want && k < n; k++)
            {
                var entry = WalkIns[(dayIndex * want + k) % n];
                string reqId = $"walkin_{entry.id}_{dayIndex}";
                if (IsResolved(reqId)) continue;
                AddTicket(new RequestTicket
                {
                    requestId    = reqId,
                    villagerId   = entry.id,
                    villagerName = entry.name,
                    teaser       = entry.teaser,
                    openingLine  = entry.opening,
                    kind         = "TakeMemory",
                    coinReward   = 4 + ((dayIndex + k) % 4),        // 4..7, gentle
                    pinnedArc    = false,
                });
            }

            // Absolute fallback: if everything's resolved, offer a gentle "just visit".
            if (Today.Count == 0)
                AddTicket(new RequestTicket
                {
                    requestId = $"quietday_{dayIndex}", villagerId = "", villagerName = "A quiet caller",
                    teaser = "nothing to set down today", kind = "JustVisit", coinReward = 2,
                    openingLine = "\u201cNo memory today \u2014 I only wanted to see the Hollow lit. It's good that it is.\u201d",
                });
        }

        private bool IsResolved(string id)
            => _vs != null && _vs.resolvedRequestIds != null && _vs.resolvedRequestIds.Contains(id);

        private void AddTicket(RequestTicket t)
        {
            if (t == null) return;
            if (Today.Any(x => x.requestId == t.requestId)) return;
            Today.Add(t);
        }

        private RequestTicket FromSO(RequestSO r)
        {
            int reward = 4;
            string memId = "";
            if (r.memory != null)
            {
                memId = r.memory.id;
                reward = 4 + Mathf.RoundToInt(Mathf.Clamp01(r.memory.weight) * 6f);   // 4..10
            }
            return new RequestTicket
            {
                requestId    = string.IsNullOrEmpty(r.requestId) ? r.name : r.requestId,
                villagerId   = r.villager != null ? r.villager.villagerId : "",
                villagerName = r.DisplayName,
                teaser       = r.boardTeaser,
                openingLine  = r.openingLine,
                kind         = r.kind.ToString(),
                memoryId     = memId,
                coinReward   = reward,
                pinnedArc    = r.pinnedArcBeat,
            };
        }

        private bool IsEligible(RequestSO r)
        {
            if (IsResolved(string.IsNullOrEmpty(r.requestId) ? r.name : r.requestId)) return false;

            int day = _vs != null ? _vs.currentDayIndex : 0;
            if (day < r.minDayIndex) return false;

            if (r.blockedByFlags != null && r.blockedByFlags.Count > 0 &&
                VillageStateFlags.AnySet(r.blockedByFlags, _vs)) return false;

            if (r.requiresFlags != null && r.requiresFlags.Count > 0 &&
                !VillageStateFlags.AllSet(r.requiresFlags, _vs)) return false;

            if (r.requiresEchoIds != null && r.requiresEchoIds.Count > 0 && _vs != null)
                foreach (var id in r.requiresEchoIds)
                    if (!string.IsNullOrEmpty(id) &&
                        (_vs.revealedEchoConnectionIds == null || !_vs.revealedEchoConnectionIds.Contains(id)))
                        return false;

            return true;
        }

        private static RequestSO WeightedPick(List<RequestSO> list, System.Random rng)
        {
            float total = list.Sum(r => Mathf.Max(0.01f, r.weight));
            double roll = rng.NextDouble() * total, acc = 0;
            foreach (var r in list) { acc += Mathf.Max(0.01f, r.weight); if (roll <= acc) return r; }
            return list[list.Count - 1];
        }

        /// <summary>Find today's ticket by id (used by the visit flow).</summary>
        public RequestTicket FindToday(string requestId)
            => string.IsNullOrEmpty(requestId) ? null : Today.FirstOrDefault(t => t.requestId == requestId);
    }
}
