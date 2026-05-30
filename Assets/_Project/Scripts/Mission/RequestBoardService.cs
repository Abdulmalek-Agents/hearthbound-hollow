// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / RequestBoardService  (Engagement Pillar P2)
//
// THE NEVER-EMPTY CONTENT FAUCET (Docs/Engagement_Bible/05).
//
// Each morning (DayStartedEvent) this builds the day's Request Board: a small,
// weighted, gentle set of villagers who need a memory kept today. It writes:
//   • agenda.tickets  — actionable RequestTickets the RequestBoardUI opens into
//                        visits (Phase 62 interactive).
//   • agenda.visitors — the teaser strings the morning Agenda card already shows
//                        (preserves the 61.8 behaviour).
//
// SOURCES (scales without infinite hand-writing — Docs/Engagement_Bible/05 §2):
//   • Authored RequestSOs in an optional Resources/RequestPool asset (the
//     prestige + designed content). Pinned arc beats always appear when eligible;
//     the rest are weight-sampled. Gating is honoured (day / flags / echoes), and
//     resolved requests stop reappearing (carry-over is implicit).
//   • A built-in cozy roster fallback so the loop works with ZERO authored content
//     (procedural village texture). Phase 68 expands this via the Vignette Library.
//
// SAFE DROP: self-installing (matches MissionAudioHooks / DailyLoopService),
// observer-only on DayStartedEvent. No scene edit, no builder. If anything is
// missing it adds nothing and the card falls back to its baseline line.

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
        private RequestPoolSO _pool;
        private VillageState _vs;

        /// <summary>Today's actionable tickets (also mirrored into the agenda).</summary>
        public readonly List<RequestTicket> Today = new();

        // Built-in cozy roster — used when no RequestPool asset is authored. Each
        // line is an opening beat for a self-contained walk-in visit. (villagerId,
        // name, teaser, openingLine)
        private static readonly (string id, string name, string teaser, string opening)[] Roster =
        {
            ("doris",   "Doris",      "a sweet thing to ask",
                "\u201cI baked too much again. But there's a morning I keep losing \u2014 the first loaves I ever sold. Would you hold onto it for me?\u201d"),
            ("gerrold", "Gerrold",    "he sounded steadier today",
                "\u201cI'm not here to give anything up today. I just\u2026 wanted to sit where someone remembers her too.\u201d"),
            ("bram",    "Bram the goatherd", "a memory that won't sit still",
                "\u201cThere's a hillside afternoon that keeps slipping. Goats, wind, my brother laughing. Can you keep it from going?\u201d"),
            ("mariska", "Old Mariska", "a riddle, as always",
                "\u201cI'll not tell you what it is. Only that it's blue, and it's cold, and it was kind once. Keep it and you'll see.\u201d"),
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

            // Mirror into the morning Agenda (tickets + teaser strings).
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

            // Prefer an authored pool when present + non-empty.
            if (_pool != null && _pool.allRequests != null && _pool.allRequests.Count > 0)
            {
                var eligible = _pool.allRequests.Where(r => r != null && IsEligible(r)).ToList();

                // 1. Pinned arc beats always appear when eligible.
                foreach (var r in eligible.Where(r => r.pinnedArcBeat))
                    AddTicket(FromSO(r));

                // 2. Weight-sample the rest, deterministic per day (so a given day
                //    is stable across reloads but mornings differ).
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

            // Built-in fallback roster — rotate by day so each morning differs.
            int n = Roster.Length;
            for (int k = 0; k < Mathf.Min(want, n); k++)
            {
                var entry = Roster[(dayIndex * want + k) % n];
                string reqId = $"walkin_{entry.id}_{dayIndex}";
                if (_vs != null && _vs.resolvedRequestIds != null && _vs.resolvedRequestIds.Contains(reqId))
                    continue;   // already handled this exact walk-in
                AddTicket(new RequestTicket
                {
                    requestId    = reqId,
                    villagerId   = entry.id,
                    villagerName = entry.name,
                    teaser       = entry.teaser,
                    openingLine  = entry.opening,
                    kind         = "TakeMemory",
                    coinReward   = 4 + ((dayIndex + k) % 4),   // 4..7, gentle
                    pinnedArc    = false,
                });
            }
        }

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
            // Resolved requests never reappear (carry-over is implicit).
            if (_vs != null && _vs.resolvedRequestIds != null &&
                _vs.resolvedRequestIds.Contains(string.IsNullOrEmpty(r.requestId) ? r.name : r.requestId))
                return false;

            int day = _vs != null ? _vs.currentDayIndex : 0;
            if (day < r.minDayIndex) return false;

            // blockedByFlags: hide once the arc has advanced past this beat.
            if (r.blockedByFlags != null && r.blockedByFlags.Count > 0 &&
                VillageStateFlags.AnySet(r.blockedByFlags, _vs)) return false;

            // requiresFlags: every gate flag must be set.
            if (r.requiresFlags != null && r.requiresFlags.Count > 0 &&
                !VillageStateFlags.AllSet(r.requiresFlags, _vs)) return false;

            // requiresEchoIds: every named echo must already be revealed.
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
