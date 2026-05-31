// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / PrefaceBeatDirector
//
// PHASE 50 — Runs the PrefaceBeatUI on Lane scene Start.
//
// Sits on the Lane scene (02_Mission01_Lane). On Start(), looks up the
// PrefaceBeatUI in the scene and calls `Play(VillageState, OnDone)`.
// When the beat finishes (or is skipped), gameplay control unlocks: the
// PlayerController is enabled if it was disabled by the builder, and the
// OnboardingOverlay (Phase 30) is given the green light to begin.
//
// Per-save flag: if VillageState.prefaceBeatPlayed is already true, this
// director is a no-op — players don't see the preface again on subsequent
// re-loads.
//
// Decoupled from Mission01Director: the preface is purely cinematic. The
// mission director still handles the Doris approach trigger.
//
// ── Hotfix 2026-05-28 ──────────────────────────────────────────────
// Replaced `ServiceLocator.Resolve<T>()` with `ServiceLocator.Get<T>()`
// (the canonical Core API). 1 call site corrected. See
// MemoryWebOverlay.cs commit for the full audit summary.

using UnityEngine;
using HearthboundHollow.Core;
using HearthboundHollow.Player;
using HearthboundHollow.UI;

namespace HearthboundHollow.Mission
{
    public class PrefaceBeatDirector : MonoBehaviour
    {
        [Header("Wired by Phase 50 builder")]
        public PrefaceBeatUI prefaceBeat;
        public PlayerController playerController;
        public OnboardingOverlay onboardingOverlay;

        [Header("Behaviour")]
        [Tooltip("If true, disables PlayerController until the beat ends.")]
        public bool lockPlayerDuringBeat = true;

        private VillageState _state;

        private void Start()
        {
            _state = ServiceLocator.Get<VillageState>();
            if (prefaceBeat == null)
            {
                prefaceBeat = FindFirstObjectByType<PrefaceBeatUI>();
            }
            if (prefaceBeat == null)
            {
                Hh.Log(LogCategory.Boot,
                    "PrefaceBeatDirector: no PrefaceBeatUI in scene. " +
                    "Skipping preface beat.");
                Unlock();
                return;
            }

            if (_state != null && _state.prefaceBeatPlayed)
            {
                Hh.Log(LogCategory.Boot, "Preface Beat already played; gameplay unlocked immediately.");
                Unlock();
                return;
            }

            if (lockPlayerDuringBeat && playerController != null)
                playerController.enabled = false;

            if (onboardingOverlay != null)
                onboardingOverlay.gameObject.SetActive(false);

            prefaceBeat.Play(_state, OnBeatComplete);
        }

        private void OnBeatComplete()
        {
            Unlock();
            // After the preface, the OnboardingOverlay can fire normally.
            if (onboardingOverlay != null)
                onboardingOverlay.gameObject.SetActive(true);
        }

        private void Unlock()
        {
            if (playerController != null) playerController.enabled = true;
        }
    }
}
