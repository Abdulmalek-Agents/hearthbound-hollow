// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Core / DayAgenda
//
// A lightweight, runtime-built model of "what the player might do today."
// Assembled fresh each morning by the loop services (RequestBoardService,
// GardenService, Almanac) in response to DayStartedEvent, then shown by the
// AgendaCardUI. Deliberately NOT a ScriptableObject — it is transient per-day
// state, never an authored asset. See Docs/Engagement_Bible/03 §2 and 04 §4.
//
// Cozy guardrail (D-076): the Agenda lists OPPORTUNITY, never OBLIGATION.
// No deadlines, no fail risk, no numbers going down.

using System.Collections.Generic;

namespace HearthboundHollow.Core
{
    public sealed class DayAgenda
    {
        public int dayIndex;

        /// <summary>e.g. "Spire-Month, Day 4".</summary>
        public string seasonLabel = "";

        /// <summary>e.g. "a bright cold morning".</summary>
        public string moodLine = "";

        /// <summary>Visitor teasers, e.g. "Doris — \"a sweet thing to ask\"".</summary>
        public readonly List<string> visitors = new();

        /// <summary>Garden status lines, e.g. "Lavender — ready to harvest".</summary>
        public readonly List<string> gardenNotes = new();

        /// <summary>A single gentle, optional self-goal nudge in Marin's margin-note voice.</summary>
        public string marinSuggestion = "";

        /// <summary>True when there is nothing pressing — a valid, restful state (cozy).</summary>
        public bool IsQuietDay => visitors.Count == 0 && gardenNotes.Count == 0;
    }
}
