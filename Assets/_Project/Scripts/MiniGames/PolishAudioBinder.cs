// SPDX-License-Identifier: MIT
// Hearthbound Hollow — MiniGames / PolishAudioBinder
//
// Listens to PolishMiniGame events at runtime and routes them to the
// SfxPlayer service. Lives in the MiniGames asmdef (not Audio) because
// MiniGames already references Audio; putting it in Audio would create
// an Audio↔MiniGames cycle.
//
// The 9 cue events come from Mission_1_2_Focus/04_POLISH_AND_CLEANSE_MINIGAMES.md § 2.7.

using UnityEngine;
using HearthboundHollow.Core;
using HearthboundHollow.Audio;

namespace HearthboundHollow.MiniGames
{
    [RequireComponent(typeof(PolishMiniGame))]
    public class PolishAudioBinder : MonoBehaviour
    {
        private PolishMiniGame _polish;
        private float _lastFrictionAt = -10f;

        private void Awake()
        {
            _polish = GetComponent<PolishMiniGame>();
        }

        private void OnEnable()
        {
            if (_polish == null) return;
            _polish.OnGameStarted += OnStarted;
            _polish.OnGameFinished += OnFinished;
            _polish.OnClarityChanged += OnClarityChanged;
            _polish.OnMilestoneReached += OnMilestone;
            _polish.OnRevealReached += OnReveal;
            _polish.OnFrictionWarning += OnFriction;
        }

        private void OnDisable()
        {
            if (_polish == null) return;
            _polish.OnGameStarted -= OnStarted;
            _polish.OnGameFinished -= OnFinished;
            _polish.OnClarityChanged -= OnClarityChanged;
            _polish.OnMilestoneReached -= OnMilestone;
            _polish.OnRevealReached -= OnReveal;
            _polish.OnFrictionWarning -= OnFriction;
        }

        private void OnStarted(MiniGameBase _)
        {
            Sfx()?.PlayOneShot("polish_hum_start");
            Sfx()?.PlayLoop("polish_hum_loop");
        }

        private void OnClarityChanged(float clarity)
        {
            // Start the rub loop on first clarity change; the loop dedupes internally.
            Sfx()?.PlayLoop("polish_rub_loop");
        }

        private void OnMilestone() => Sfx()?.PlayOneShot("polish_midway_chime");
        private void OnReveal()    => Sfx()?.PlayOneShot("polish_reveal_swell");

        private void OnFriction()
        {
            if (Time.time - _lastFrictionAt < 1.0f) return;  // throttle
            _lastFrictionAt = Time.time;
            Sfx()?.PlayOneShot("polish_rub_friction_warn");
        }

        private void OnFinished(MiniGameBase mg)
        {
            Sfx()?.StopLoop("polish_rub_loop");
            Sfx()?.StopLoop("polish_hum_loop");
            Sfx()?.PlayOneShot("polish_success_jingle");
            Sfx()?.PlayLoop("polish_hum_post");
        }

        private SfxPlayer Sfx() => ServiceLocator.Get<SfxPlayer>();
    }
}
