// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Core / EchoBoard  (Engagement Pillar P6 — Phase 63)
//
// A tiny Core-side blackboard for the Memory Wall / Echo Web meta-game. The
// Mission-layer EchoWebService recomputes the player's Echo-thread progress
// (from VillageState.heldMemoryIds vs the authored EchoPool or a built-in set)
// and writes the result here; the UI-layer MemoryWallUI reads it and renders
// the collection + thread progress — with NO UI→Mission dependency (D-035),
// mirroring how DayAgenda bridges RequestBoardService → AgendaCardUI.
//
// Cozy guardrail (D-076): these are celebratory progress views (kept / threshold,
// "X / N echoes found") — abundance, never deficit. No countdowns, no nags.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace HearthboundHollow.Core
{
    public sealed class EchoThreadView
    {
        public string echoId = "";
        public string displayName = "";
        public string typeLabel = "Person";   // Person / Place / Object / Year / Pattern
        public int kept = 0;
        public int threshold = 1;
        public bool complete = false;
        /// <summary>Optional cozy hint shown under an incomplete thread.</summary>
        public string hint = "";

        public float Progress01 => threshold <= 0 ? 1f : Mathf.Clamp01((float)kept / threshold);
    }

    /// <summary>Core-side, UI-readable snapshot of Echo-thread progress.</summary>
    public static class EchoBoard
    {
        public static readonly List<EchoThreadView> Threads = new();

        /// <summary>Total echoes/threads the player has completed (celebratory count).</summary>
        public static int CompletedCount;

        /// <summary>Raised whenever the service recomputes (UI refreshes if open).</summary>
        public static event Action OnChanged;

        public static void Raise() => OnChanged?.Invoke();
    }
}
