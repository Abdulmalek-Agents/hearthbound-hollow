// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / AlmanacService  (Engagement Pillar P7 — Phase 67)
//
// THE ALMANAC (Docs/Engagement_Bible/05 §6) — the calendar of anticipation that
// makes certain mornings special and gives the open loop a horizon. Each day it
// resolves at most one gentle event and writes a headline into the morning Agenda
// (agenda.almanacLine), plus a countdown when nothing is happening today ("3 days
// to the Honey Festival"). Anticipation is engagement — players plan around it.
//
// EVENTS (cozy, never pressuring):
//   • Market Day   — every 5th day: the travelling cart buys teas at a premium.
//   • Festival     — month-end (every 10th day): the village gathers (Honey / Hearth).
//   • Bard (Idris) — every 7th day: a rare "memory of the road" may knock.
//   • A Birthday   — day 4 of the month: a warm one-off.
//
// SAFE DROP: self-installing, observer-only on DayStartedEvent. Reuses the DayCycle
// the loop already advances ("DayCycleManager dims one light" → now it paces a
// calendar). Publishes AlmanacEventEvent so other systems can react later.

using UnityEngine;
using HearthboundHollow.Core;

namespace HearthboundHollow.Mission
{
    public class AlmanacService : MonoBehaviour
    {
        public static AlmanacService Instance { get; private set; }

        // Month is 10 fiction-days long (GDD §4.2 cadence, compressed for the slice).
        private const int MonthLength = 10;

        public string TodayEventId { get; private set; } = "";
        public string TodayHeadline { get; private set; } = "";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInstall()
        {
            if (Instance != null) return;
            var go = new GameObject("_HHAlmanac");
            DontDestroyOnLoad(go);
            go.AddComponent<AlmanacService>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            ServiceLocator.Register(this);
            EventBus.Subscribe<DayStartedEvent>(OnDayStarted);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
            if (Instance == this) Instance = null;
        }

        private void OnDayStarted(DayStartedEvent e)
        {
            int fictionDay = e.DayIndex + 1;                 // matches the Agenda card's "Day N"
            int dayOfMonth = ((fictionDay - 1) % MonthLength) + 1;

            (string id, string line) = Resolve(fictionDay, dayOfMonth);
            TodayEventId = id;
            TodayHeadline = line;

            var agenda = DailyLoopService.Instance != null ? DailyLoopService.Instance.CurrentAgenda : null;
            if (agenda != null) agenda.almanacLine = line;

            if (!string.IsNullOrEmpty(id))
                EventBus.Publish(new AlmanacEventEvent(id, line, e.DayIndex));

            Hh.Log(LogCategory.Mission, $"[Almanac] Day {fictionDay} (of-month {dayOfMonth}): {(string.IsNullOrEmpty(id) ? "quiet" : id)}.");
        }

        private (string id, string line) Resolve(int fictionDay, int dayOfMonth)
        {
            if (dayOfMonth == MonthLength)
                return ("festival", "\ud83c\udf3e Festival of the Hearth today — the village gathers in the lane. Bring your brightest memory.");
            if (fictionDay % 5 == 0)
                return ("market", "\ud83d\udED2 Market Day — the travelling cart is in the lane. It pays well for teas.");
            if (fictionDay % 7 == 0)
                return ("bard", "\ud83c\udfbb Idris the bard passes through — memories of the road are rare. He never stays.");
            if (dayOfMonth == 4)
                return ("birthday", "\ud83c\udf82 Someone in the village has a birthday today. A kind word goes far.");

            // Quiet day — offer a gentle countdown so tomorrow has a shape.
            int toFestival = MonthLength - dayOfMonth;
            int toMarket = (5 - (fictionDay % 5)) % 5; if (toMarket == 0) toMarket = 5;
            if (toFestival <= 2 && toFestival > 0)
                return ("", $"{toFestival} day(s) to the Festival of the Hearth.");
            if (toMarket <= 2)
                return ("", $"Market Day in {toMarket} day(s) — brew a little extra.");
            return ("", "");
        }
    }
}
