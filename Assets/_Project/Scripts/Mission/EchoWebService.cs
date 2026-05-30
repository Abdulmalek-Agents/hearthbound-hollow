// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / EchoWebService  (Engagement Pillar P6 — Phase 63)
//
// THE GOAL ENGINE behind the Memory Wall (Docs/Engagement_Bible/09).
//
// Watches the player's kept-memory collection (VillageState.heldMemoryIds) and
// tallies each Echo thread's progress. When a thread reaches its threshold it
// COMPLETES: records it, bumps the celebratory echoes-found counter, grants a
// gentle coin reward, and publishes EchoThreadCompletedEvent (the Wall + Ledger
// consume it). This converts "a pile of lovely memories" into "a web the player
// chooses to complete" — the self-set-goal engine the critique demanded.
//
// SOURCES:
//   • Authored EchoSO list in an optional Resources/EchoPool asset (precise:
//     members are MemoryNodeSO assets, matched by .id against heldMemoryIds).
//   • A built-in cozy thread set so the Wall always shows live goals even with
//     the Phase-62 walk-in roster (threads derived from per-villager + total counts).
//
// ARCHITECTURE: writes the UI-readable snapshot to Core's EchoBoard; the UI
// (MemoryWallUI) reads that with no Mission dependency (D-035). Self-installing,
// observer-only — no scene edit, no builder.

using System.Collections.Generic;
using UnityEngine;
using HearthboundHollow.Core;
using HearthboundHollow.Memory;

namespace HearthboundHollow.Mission
{
    public class EchoWebService : MonoBehaviour
    {
        public static EchoWebService Instance { get; private set; }

        private EchoPoolSO _pool;
        private VillageState _vs;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInstall()
        {
            if (Instance != null) return;
            var go = new GameObject("_HHEchoWeb");
            DontDestroyOnLoad(go);
            go.AddComponent<EchoWebService>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            ServiceLocator.Register(this);
            _pool = Resources.Load<EchoPoolSO>("EchoPool");   // optional
            EventBus.Subscribe<MemoryKeptEvent>(OnMemoryKept);
            EventBus.Subscribe<DayStartedEvent>(OnDayStarted);
            EventBus.Subscribe<VillageStateLoadedEvent>(OnStateLoaded);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<MemoryKeptEvent>(OnMemoryKept);
            EventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
            EventBus.Unsubscribe<VillageStateLoadedEvent>(OnStateLoaded);
            if (Instance == this) Instance = null;
        }

        private void OnMemoryKept(MemoryKeptEvent e) => Recompute();
        private void OnDayStarted(DayStartedEvent e) => Recompute();
        private void OnStateLoaded(VillageStateLoadedEvent e) { _vs = e.VillageState as VillageState; Recompute(); }

        /// <summary>Rebuild EchoBoard.Threads + complete any newly-finished threads.</summary>
        public void Recompute()
        {
            _vs ??= ServiceLocator.Get<VillageState>();
            if (_vs == null) return;
            var held = _vs.heldMemoryIds ?? new List<string>();

            EchoBoard.Threads.Clear();

            if (_pool != null && _pool.allEchoes != null && _pool.allEchoes.Count > 0)
            {
                foreach (var echo in _pool.allEchoes)
                {
                    if (echo == null) continue;
                    int kept = 0;
                    if (echo.members != null)
                        foreach (var m in echo.members)
                            if (m != null && !string.IsNullOrEmpty(m.id) && held.Contains(m.id)) kept++;
                    AddThread(echo.echoId, echo.displayName, echo.type.ToString(),
                              kept, Mathf.Max(1, echo.threshold), echo.rewardsOnComplete,
                              "Keep more of these to complete the thread.");
                }
            }
            else
            {
                // Built-in cozy threads — always give the Wall live goals.
                int doris   = CountVillager(held, "doris");
                int gerrold = CountVillager(held, "gerrold");
                int total   = held.Count;

                AddThread("ECHO_BAKERS_MORNINGS", "The Baker's Mornings", "Person",
                          doris, 2, null, "Keep two of Doris's memories.");
                AddThread("ECHO_WIDOWERS_QUIET", "The Widower's Quiet", "Person",
                          gerrold, 2, null, "Keep two of Gerrold's memories.");
                AddThread("ECHO_SUNDAY_KITCHEN", "The Sunday Kitchen", "Place",
                          (doris > 0 ? 1 : 0) + (gerrold > 0 ? 1 : 0), 2, null,
                          "A kitchen they both remember — keep one of each.");
                AddThread("ECHO_VILLAGE_KEEPSAKES", "A Village of Keepsakes", "Pattern",
                          total, 5, null, "Keep five memories from anyone in the village.");
            }

            EchoBoard.CompletedCount = _vs.completedEchoIds != null ? _vs.completedEchoIds.Count : 0;
            EchoBoard.Raise();
        }

        private void AddThread(string id, string name, string typeLabel, int kept, int threshold,
                               List<string> rewards, string hint)
        {
            if (string.IsNullOrEmpty(id)) return;
            bool already = _vs.completedEchoIds != null && _vs.completedEchoIds.Contains(id);
            bool complete = already || kept >= threshold;

            EchoBoard.Threads.Add(new EchoThreadView
            {
                echoId = id, displayName = name, typeLabel = typeLabel,
                kept = Mathf.Min(kept, threshold), threshold = threshold,
                complete = complete, hint = complete ? "" : hint,
            });

            if (!already && kept >= threshold) Complete(id, name, rewards);
        }

        private void Complete(string id, string name, List<string> rewards)
        {
            if (_vs.completedEchoIds == null) _vs.completedEchoIds = new List<string>();
            _vs.completedEchoIds.Add(id);

            // The celebratory echoes-found counter (journal/agenda) + request gating.
            if (_vs.revealedEchoConnectionIds == null) _vs.revealedEchoConnectionIds = new List<string>();
            if (!_vs.revealedEchoConnectionIds.Contains(id)) _vs.revealedEchoConnectionIds.Add(id);

            // A gentle coin reward — connecting matters (feeds P3). Cozy, never required.
            _vs.coin += 5;
            EventBus.Publish(new CoinChangedEvent(_vs.coin, 5, "echo"));

            // Best-effort reward flags (decor/dream/arc are set as VillageState flags
            // by string id where one exists; unknown ids are simply logged).
            if (rewards != null)
                foreach (var r in rewards)
                    if (!string.IsNullOrWhiteSpace(r)) VillageStateFlags.Set(r, true, _vs);

            EventBus.Publish(new EchoThreadCompletedEvent(id));
            Hh.Log(LogCategory.Mission, $"[Echo] Thread complete: '{name}' ({id}). +5 coin.");
        }

        private static int CountVillager(List<string> held, string villagerId)
        {
            if (held == null || string.IsNullOrEmpty(villagerId)) return 0;
            int n = 0;
            string token = villagerId.ToLowerInvariant();
            foreach (var id in held)
                if (!string.IsNullOrEmpty(id) && id.ToLowerInvariant().Contains(token)) n++;
            return n;
        }
    }
}
