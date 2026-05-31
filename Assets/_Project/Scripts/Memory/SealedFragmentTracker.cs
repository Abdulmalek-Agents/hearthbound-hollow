// =============================================================================
// SealedFragmentTracker.cs — Hearthbound Hollow
// Tracks the 9 sealed memory fragments across missions 11-28.
// When all 9 are found, the sealed memory assembles and M28 unlocks.
// Phase 76 — 30-Mission Architecture.
// =============================================================================
using System.Collections.Generic;
using UnityEngine;
using HearthboundHollow.Core;

namespace HearthboundHollow.Memory
{
    /// <summary>
    /// Service that knows which of the 9 sealed fragments have been found and
    /// whose memory holds each one. Register in GameManager.Awake.
    /// Phase 76.
    /// </summary>
    public class SealedFragmentTracker : MonoBehaviour
    {
        private const int TotalFragments = 9;

        // Canonical fragment definitions (index 0-8).
        private static readonly SealedFragmentDef[] _fragments = new[]
        {
            new SealedFragmentDef(0, "Edmund Pace",     "M11", "The vote that sealed the silence"),
            new SealedFragmentDef(1, "Ruth Calloway",   "M12", "The dress hemmed for the wrong reason"),
            new SealedFragmentDef(2, "Finn Arley",      "M13", "The night the sheep disappeared"),
            new SealedFragmentDef(3, "Nell Prior",      "M14", "What the child saw through the window"),
            new SealedFragmentDef(4, "The Locked Room", "M15", "The account book nobody burned"),
            new SealedFragmentDef(5, "Mayor Whitmore",  "M21", "The morning he made the call"),
            new SealedFragmentDef(6, "Dr. Vera Hartwell","M22","The patient nobody claimed"),
            new SealedFragmentDef(7, "August Wren",     "M27", "The clock he set wrong on purpose"),
            new SealedFragmentDef(8, "Hidden in shop",  "M24", "A memory sold to the Hollow years ago, still waiting"),
        };

        private VillageState _state;

        private void Awake()
        {
            ServiceLocator.Register(this);
            _state = ServiceLocator.Get<VillageState>();

            EventBus.Subscribe<SealedFragmentFoundEvent>(OnFragmentFound);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<SealedFragmentFoundEvent>(OnFragmentFound);
        }

        private void OnFragmentFound(SealedFragmentFoundEvent evt)
        {
            if (_state == null) return;
            string fid = FragmentId(evt.FragmentIndex);
            if (!_state.sealedFragmentsFound.Contains(fid))
            {
                _state.sealedFragmentsFound.Add(fid);
                Debug.Log($"[SealedFragment] Fragment {evt.FragmentIndex} found. " +
                          $"Total: {_state.sealedFragmentsFound.Count}/{TotalFragments}");
            }

            if (_state.sealedFragmentsFound.Count >= TotalFragments
                && !_state.sealedMemoryAssembled)
            {
                _state.sealedMemoryAssembled = true;
                _state.allFragmentsAssembled = true;
                EventBus.Publish(new SealedMemoryAssembledEvent());
                Debug.Log("[SealedFragment] ALL 9 FRAGMENTS FOUND — sealed memory assembles!");
            }
        }

        public static string FragmentId(int index) => $"sealed_frag_{index:D2}";

        public int  FoundCount  => _state?.sealedFragmentsFound.Count ?? 0;
        public bool IsAssembled => _state?.sealedMemoryAssembled ?? false;

        public SealedFragmentDef GetDef(int index) =>
            index >= 0 && index < _fragments.Length ? _fragments[index] : null;
    }

    public sealed class SealedFragmentDef
    {
        public int    Index;
        public string Holder;
        public string Mission;
        public string Description;
        public SealedFragmentDef(int i, string h, string m, string d)
            { Index = i; Holder = h; Mission = m; Description = d; }
    }
}
