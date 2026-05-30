// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / RequestBoardService  (Engagement Pillar P2 — Phase 61.8)
//
// Fills the morning Agenda with ROTATING villagers each day, so the player wakes
// to a different "who needs me today?" — the seed of the never-empty Request
// Board (Docs/Engagement_Bible/05).
//
// SCOPE (honest): this phase feeds the AGENDA CARD's visitor list (variety on the
// morning card). It does NOT yet spawn interactive visits — the playable day is
// still the scripted M1/M2 flow; the data-driven VisitDirector that turns a
// request into a full visit is Phase 62.
//
// SAFE DROP: self-installing (matches MissionAudioHooks), observer-only. On
// DayStartedEvent it appends to DailyLoopService.CurrentAgenda.visitors BEFORE the
// AgendaCardUI renders (BeginDay fires DayStartedEvent, THEN AgendaReadyEvent).
// Uses an optional authored Resources/RequestPool asset; otherwise a built-in
// cozy roster. No scene edit, no builder, no change to existing flow. If anything
// fails it simply adds nothing and the card falls back to its baseline line.

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

        [SerializeField] private int requestsPerDay = 2;
        private RequestPoolSO _pool;
        private VillageState _vs;

        // Built-in cozy roster — used when no RequestPool asset is authored.
        private static readonly (string name, string teaser)[] Roster =
        {
            ("Doris",                     "a sweet thing to ask"),
            ("Gerrold",                   "he sounded steadier today"),
            ("a stranger in a heavy coat","something heavy to set down"),
            ("Bram the goatherd",         "a memory that won't sit still"),
            ("Old Mariska",               "a riddle, as always"),
            ("Ms. Inkwell",               "a letter she can't bring herself to read"),
            ("Tomek the innkeeper",       "something he keeps behind the bar"),
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

            var teasers = BuildTeasers(e.DayIndex);
            foreach (var t in teasers)
                if (!agenda.visitors.Contains(t)) agenda.visitors.Add(t);

            Hh.Log(LogCategory.Mission, $"[RequestBoard] Day {e.DayIndex}: {teasers.Count} visitor teaser(s) queued.");
        }

        private List<string> BuildTeasers(int dayIndex)
        {
            var result = new List<string>();
            int want = Mathf.Max(1, requestsPerDay);

            // Prefer an authored pool when present + non-empty.
            if (_pool != null && _pool.allRequests != null && _pool.allRequests.Count > 0)
            {
                var eligible = _pool.allRequests.Where(r => r != null && IsEligible(r)).ToList();

                foreach (var r in eligible.Where(r => r.pinnedArcBeat))
                {
                    string f = Format(r.DisplayName, r.boardTeaser);
                    if (!result.Contains(f)) result.Add(f);
                }

                var rest = eligible.Where(r => !r.pinnedArcBeat).ToList();
                for (int i = 0; result.Count < want && rest.Count > 0 && i <= rest.Count; i++)
                {
                    var r = rest[(dayIndex + i) % rest.Count];
                    string f = Format(r.DisplayName, r.boardTeaser);
                    if (!result.Contains(f)) result.Add(f);
                }

                if (result.Count > 0) return result;
            }

            // Built-in fallback — rotate by day so each morning differs.
            int n = Roster.Length;
            for (int k = 0; k < Mathf.Min(want, n); k++)
            {
                var entry = Roster[(dayIndex * want + k) % n];
                string f = Format(entry.name, entry.teaser);
                if (!result.Contains(f)) result.Add(f);
            }
            return result;
        }

        private bool IsEligible(RequestSO r)
        {
            if (_vs == null) return r.minDayIndex <= 0;
            return _vs.currentDayIndex >= r.minDayIndex;
            // NOTE: flag/echo gating (requiresFlags/blockedByFlags/requiresEchoIds)
            // is resolved by the full VisitDirector in Phase 62; the teaser layer
            // here only day-gates so it can never hide a needed beat.
        }

        private static string Format(string name, string teaser) =>
            string.IsNullOrEmpty(teaser) ? name : $"{name} \u2014 \u201c{teaser}\u201d";
    }
}
