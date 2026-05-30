// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / RequestVisitService  (Engagement Pillar P2)
//
// THE CONSEQUENCE OWNER for Request Board visits (Docs/Engagement_Bible/05 §5).
//
// The UI (RequestBoardUI) is purely presentational: it shows the day's tickets,
// lets the player choose how to answer, and publishes a RequestSelectedEvent
// INTENT. This Mission-layer service owns the actual game-state mutation — coin,
// the memory collection, trust, cinder, flags — and then publishes the
// RequestResolvedEvent + MemoryKeptEvent + CoinChangedEvent results that the Wall,
// Ledger, Echo web and coin-purse HUD consume. Keeping the mutation here (Mission)
// and the screen there (UI) preserves the asmdef graph (UI never refs Mission, D-035).
//
// COZY CONTRACT:
//   • "keep"   — the warm default: a little coin earned, the memory kept.
//   • "listen" — the Vow-7 path: no coin, a cinder, deeper trust (never punished).
//   • "defer"/"refuse" — fully honoured: the request simply rolls to tomorrow,
//     nothing lost, no FOMO, no scolding.
//
// SAFE DROP: self-installing, observer-only. No scene edit, no builder.

using UnityEngine;
using HearthboundHollow.Core;

namespace HearthboundHollow.Mission
{
    public class RequestVisitService : MonoBehaviour
    {
        public static RequestVisitService Instance { get; private set; }

        private VillageState _vs;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInstall()
        {
            if (Instance != null) return;
            var go = new GameObject("_HHRequestVisits");
            DontDestroyOnLoad(go);
            go.AddComponent<RequestVisitService>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            ServiceLocator.Register(this);
            EventBus.Subscribe<RequestSelectedEvent>(OnRequestSelected);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<RequestSelectedEvent>(OnRequestSelected);
            if (Instance == this) Instance = null;
        }

        private void OnRequestSelected(RequestSelectedEvent e)
        {
            _vs ??= ServiceLocator.Get<VillageState>();
            if (_vs == null) return;

            var ticket = RequestBoardService.Instance != null ? RequestBoardService.Instance.FindToday(e.RequestId) : null;
            string villagerId = ticket != null ? ticket.villagerId : "";
            int reward = ticket != null ? Mathf.Max(0, ticket.coinReward) : 4;

            switch ((e.Outcome ?? "").ToLowerInvariant())
            {
                case "keep":
                    DoKeep(e.RequestId, ticket, villagerId, reward);
                    break;

                case "listen":
                    DoListen(e.RequestId, villagerId);
                    break;

                case "defer":
                    EventBus.Publish(new RequestResolvedEvent(e.RequestId, "deferred"));
                    Hh.Log(LogCategory.Mission, $"[Visit] {e.RequestId} deferred — rolls to tomorrow.");
                    break;

                default: // "refuse" or anything else — refusal is always honoured.
                    EventBus.Publish(new RequestResolvedEvent(e.RequestId, "refused"));
                    Hh.Log(LogCategory.Mission, $"[Visit] {e.RequestId} refused — honoured, nothing lost.");
                    break;
            }
        }

        private void DoKeep(string requestId, RequestTicket ticket, string villagerId, int reward)
        {
            // 1. Earn a little coin (the real economy, finally).
            if (reward != 0)
            {
                _vs.coin += reward;
                EventBus.Publish(new CoinChangedEvent(_vs.coin, reward, "visit"));
            }

            // 2. Keep the memory (grows the collection / Memory Wall).
            string memId = ticket != null && !string.IsNullOrEmpty(ticket.memoryId)
                ? ticket.memoryId
                : SynthMemoryId(villagerId, requestId);
            if (_vs.heldMemoryIds == null) _vs.heldMemoryIds = new System.Collections.Generic.List<string>();
            if (!_vs.heldMemoryIds.Contains(memId)) _vs.heldMemoryIds.Add(memId);

            // 3. Mark resolved (stops it reappearing) + gentle trust + first-meet flags.
            MarkResolved(requestId);
            BumpTrust(villagerId, +3);
            MarkMet(villagerId);

            EventBus.Publish(new MemoryKeptEvent(memId, villagerId, reward));
            EventBus.Publish(new RequestResolvedEvent(requestId, "taken"));
            Hh.Log(LogCategory.Mission, $"[Visit] Kept '{memId}' (+{reward} coin → {_vs.coin}).");
        }

        private void DoListen(string requestId, string villagerId)
        {
            _vs.cinder += 2;   // Vow 7 — Keep the Hollow lit. Earned only by listening.
            MarkResolved(requestId);
            BumpTrust(villagerId, +2);
            MarkMet(villagerId);
            EventBus.Publish(new RequestResolvedEvent(requestId, "listened"));
            Hh.Log(LogCategory.Mission, $"[Visit] Listened to {villagerId} (+2 cinder).");
        }

        private void MarkResolved(string requestId)
        {
            if (string.IsNullOrEmpty(requestId)) return;
            if (_vs.resolvedRequestIds == null) _vs.resolvedRequestIds = new System.Collections.Generic.List<string>();
            if (!_vs.resolvedRequestIds.Contains(requestId)) _vs.resolvedRequestIds.Add(requestId);
        }

        private void BumpTrust(string villagerId, int delta)
        {
            switch ((villagerId ?? "").ToLowerInvariant())
            {
                case "doris":   _vs.trustDoris   = VillageState.Adjust(_vs.trustDoris, delta); break;
                case "gerrold": _vs.trustGerrold = VillageState.Adjust(_vs.trustGerrold, delta); break;
                default:        _vs.hollowReputation = VillageState.Adjust(_vs.hollowReputation, delta); break;
            }
        }

        private void MarkMet(string villagerId)
        {
            switch ((villagerId ?? "").ToLowerInvariant())
            {
                case "doris":   _vs.metDoris = true; break;
                case "gerrold": _vs.metGerrold = true; break;
            }
        }

        private static string SynthMemoryId(string villagerId, string requestId)
            => $"MEM_{(string.IsNullOrEmpty(villagerId) ? "village" : villagerId)}_{requestId}".ToUpperInvariant();
    }
}
