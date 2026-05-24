// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Audio / PolishAudioBinder
//
// Listens to PolishMiniGame events at runtime and routes them to the
// SfxPlayer service. The polish mini-game raises 9 cue events per
// Mission_1_2_Focus/04_POLISH_AND_CLEANSE_MINIGAMES.md § 2.7:
//
//   polish_hum_start          when player picks up the orb
//   polish_hum_loop           idle hum while orb is held
//   polish_rub_start          first circle the cursor makes
//   polish_rub_loop           while polishing
//   polish_rub_friction_warn  if player polishes too fast
//   polish_midway_chime       at clarity = 0.55
//   polish_reveal_swell       at clarity = 0.85 (composer cue)
//   polish_success_jingle     at clarity = 1.0
//   polish_hum_post           after polish, orb in cradle
//
// Attaching this component alongside a PolishMiniGame instance is enough
// to enable audio. SceneBuilder wires it automatically.

using UnityEngine;
using HearthboundHollow.Core;
using HearthboundHollow.MiniGames;

namespace HearthboundHollow.Audio
{
    [RequireComponent(typeof(PolishMiniGame))]
    public class PolishAudioBinder : MonoBehaviour
    {
        private PolishMiniGame _polish;

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

        private float _lastFrictionAt = -10f;

        private void OnStarted(MiniGameBase _)
        {
            Sfx()?.PlayOneShot("polish_hum_start");
            Sfx()?.PlayLoop("polish_hum_loop");
        }

        private void OnClarityChanged(float clarity)
        {
            // Start the rub loop on first clarity change; the loop dedupes internally
            Sfx()?.PlayLoop("polish_rub_loop");
        }

        private void OnMilestone()  => Sfx()?.PlayOneShot("polish_midway_chime");
        private void OnReveal()     => Sfx()?.PlayOneShot("polish_reveal_swell");

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
