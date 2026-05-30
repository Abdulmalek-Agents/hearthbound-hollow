// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Core / EngagementTelemetry  (Phase 70 — G-Engage instrumentation)
//
// A LOCAL, OPT-IN, dev-only measurement tool for the G-Engage playtest gate
// (Docs/PHASE70_GENGAGE_PLAYTEST.md): does the player VOLUNTARILY start a 4th day,
// and can they name a self-set goal? It subscribes to the loop's existing EventBus
// structs and appends one line per event to a local log, so a facilitator can read
// what actually happened in a session.
//
// NOT A SHIPPING FEATURE. Compiled only in the Editor / Development builds, and
// every write is gated behind an explicit opt-in flag (default OFF) — no PII, no
// network, no dark patterns (Golden Rule 2). On a release build this whole type
// compiles out.
//
// Lives in HearthboundHollow.Core (it only needs Core events). Self-installing,
// observer-only — never affects gameplay.

#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace HearthboundHollow.Core
{
    [DisallowMultipleComponent]
    public class EngagementTelemetry : MonoBehaviour
    {
        public static EngagementTelemetry Instance { get; private set; }

        // Opt-in: a facilitator flips this on (PlayerPrefs) before a playtest session.
        private const string OptInKey = "hh.telemetry.optin";
        public static bool OptedIn
        {
            get => PlayerPrefs.GetInt(OptInKey, 0) == 1;
            set { PlayerPrefs.SetInt(OptInKey, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        private string _path;
        private int _maxDayReached = -1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInstall()
        {
            if (Instance != null) return;
            var go = new GameObject("_HHTelemetry");
            DontDestroyOnLoad(go);
            go.AddComponent<EngagementTelemetry>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            _path = Path.Combine(Application.persistentDataPath, "hh_engagement_telemetry.csv");

            EventBus.Subscribe<DayStartedEvent>(OnDayStarted);
            EventBus.Subscribe<RequestResolvedEvent>(OnRequestResolved);
            EventBus.Subscribe<CoinChangedEvent>(OnCoinChanged);
            EventBus.Subscribe<EchoThreadCompletedEvent>(OnEcho);
            EventBus.Subscribe<HollowUpgradePurchasedEvent>(OnUpgrade);
            EventBus.Subscribe<MemoryTendedEvent>(OnTended);
            EventBus.Subscribe<AlmanacEventEvent>(OnAlmanac);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
            EventBus.Unsubscribe<RequestResolvedEvent>(OnRequestResolved);
            EventBus.Unsubscribe<CoinChangedEvent>(OnCoinChanged);
            EventBus.Unsubscribe<EchoThreadCompletedEvent>(OnEcho);
            EventBus.Unsubscribe<HollowUpgradePurchasedEvent>(OnUpgrade);
            EventBus.Unsubscribe<MemoryTendedEvent>(OnTended);
            EventBus.Unsubscribe<AlmanacEventEvent>(OnAlmanac);
            if (Instance == this) Instance = null;
        }

        private void OnDayStarted(DayStartedEvent e)
        {
            // The headline G-Engage signal: a VOLUNTARY 4th day (DayIndex >= 3).
            if (e.DayIndex > _maxDayReached)
            {
                _maxDayReached = e.DayIndex;
                if (e.DayIndex >= 3) Write("MILESTONE", $"voluntary_day_{e.DayIndex + 1}_reached");
            }
            Write("day_started", $"day={e.DayIndex + 1}");
        }

        private void OnRequestResolved(RequestResolvedEvent e) => Write("request", $"{e.RequestId};{e.Outcome}");
        private void OnCoinChanged(CoinChangedEvent e)         => Write("coin", $"total={e.NewTotal};delta={e.Delta};why={e.Reason}");
        private void OnEcho(EchoThreadCompletedEvent e)        => Write("echo_complete", e.EchoId);
        private void OnUpgrade(HollowUpgradePurchasedEvent e)  => Write("upgrade", e.UpgradeId);
        private void OnTended(MemoryTendedEvent e)             => Write("craft", $"{e.Verb};perfect={e.Perfect}");
        private void OnAlmanac(AlmanacEventEvent e)            => Write("almanac", $"{e.EventId};day={e.DayIndex + 1}");

        private void Write(string kind, string payload)
        {
            if (!OptedIn) return;
            try
            {
                var sb = new StringBuilder();
                sb.Append(DateTime.UtcNow.ToString("o")).Append(',')
                  .Append(kind).Append(',')
                  .Append((payload ?? "").Replace('\n', ' ').Replace(',', ';'))
                  .Append('\n');
                File.AppendAllText(_path, sb.ToString());
            }
            catch (Exception ex)
            {
                // Telemetry must never affect a session — swallow and warn once-ish.
                Hh.Warn(LogCategory.Boot, $"[Telemetry] write failed (non-fatal): {ex.Message}");
            }
        }
    }
}
#endif
