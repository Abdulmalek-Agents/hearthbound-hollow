// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / DreamHook
//
// A small runtime MonoBehaviour that bridges the EveningLedger's
// "End of Day confirmed" event to the MemoryDreamSequencer's PlayDream1()
// call. The Phase 22 builder spawns one of these into the Hollow scene
// (alongside the Memory Dream rig).
//
// !! IMPORTANT !! This class was previously declared as a private nested
// type inside Phase22_PolishedPlayableMission1.cs (Editor asmdef, which
// has includePlatforms = ["Editor"]). At runtime the type couldn't be
// resolved, the hook never fired, and Dream 1 silently never played.
//
// Moving it here (HearthboundHollow.Mission, runtime asmdef) fixes the
// resolution. The Phase 22 builder still attaches the same class — just
// via the runtime namespace.

using UnityEngine;
using HearthboundHollow.Core;
using HearthboundHollow.Cutscene;
using HearthboundHollow.UI;

namespace HearthboundHollow.Mission
{
    public class DreamHook : MonoBehaviour
    {
        public EveningLedgerUI ledger;
        public MemoryDreamSequencer sequencer;

        private void OnEnable()
        {
            if (ledger != null) ledger.OnEndOfDayConfirmed += PlayDream;
        }

        private void OnDisable()
        {
            if (ledger != null) ledger.OnEndOfDayConfirmed -= PlayDream;
        }

        private void PlayDream()
        {
            if (sequencer != null)
            {
                Hh.Log(LogCategory.Cutscene, "DreamHook → PlayDream1.");
                sequencer.PlayDream1();
            }
        }
    }
}
