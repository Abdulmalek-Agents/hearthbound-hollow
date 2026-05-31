// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Player / LaneGuidePulse
//
// Phase 47.2 runtime — Guide-lantern pulse animator.
//
// Five lanterns line the cobble path from spawn to the Hollow door. Each one
// pulses gently with a per-lantern phase delay, so the chain reads as a
// procession of light "walking" toward the Hollow. This is the cozy game's
// onboarding wayfinding language — no floating arrow, no HUD marker, just
// warmth that says "this way".
//
// Effects:
//   • Sine-wave intensity oscillation around `baseIntensity` between
//     `pulseIntensity` and `baseIntensity * 0.85`.
//   • Period: ~3.2 s (slow, breathing rate).
//   • Per-lantern phase offset via `delay` so the wave travels north.
//   • If the player is within 7 m the lantern brightens an extra
//     +0.8 intensity (subtle "noticed you" warmth).
//
// Performance: O(1) per lantern per frame. 5 lanterns total in the lane.

using UnityEngine;

namespace HearthboundHollow.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Light))]
    public sealed class LaneGuidePulse : MonoBehaviour
    {
        [SerializeField] public float baseIntensity  = 2.6f;
        [SerializeField] public float pulseIntensity = 3.6f;
        [SerializeField] public float period         = 3.2f;
        [SerializeField] public float delay          = 0f;
        [SerializeField] public float playerProximityBoost = 0.8f;
        [SerializeField] public float playerProximityRange = 7.0f;

        private Light _light;
        private Transform _playerTransform;
        private float _proximityRangeSq;

        private void Awake()
        {
            _light = GetComponent<Light>();
            if (_light != null && baseIntensity > 0f && _light.intensity == 0f)
                _light.intensity = baseIntensity;
            _proximityRangeSq = playerProximityRange * playerProximityRange;
        }

        private void Update()
        {
            if (_light == null) return;

            // Find the player only when needed (cheap — runs once until found).
            if (_playerTransform == null) _playerTransform = FindPlayer();

            // Base sine wave
            float phase = (Time.time + delay) * (Mathf.PI * 2f / period);
            float wave = (Mathf.Sin(phase) + 1f) * 0.5f; // 0..1
            float intensity = Mathf.Lerp(baseIntensity * 0.85f, pulseIntensity, wave);

            // Proximity boost
            if (_playerTransform != null)
            {
                float distSq = (_playerTransform.position - transform.position).sqrMagnitude;
                if (distSq < _proximityRangeSq)
                {
                    float t = 1f - Mathf.Sqrt(distSq) / playerProximityRange;
                    intensity += playerProximityBoost * t;
                }
            }

            _light.intensity = intensity;
        }

        private static Transform FindPlayer()
        {
            // Find by tag first (the project doesn't use tags but try anyway).
            var tagged = GameObject.FindGameObjectWithTag("Player");
            if (tagged != null) return tagged.transform;

            // Fall back to scene-search for a PlayerController.
#if UNITY_2023_1_OR_NEWER
            var pc = Object.FindFirstObjectByType<PlayerController>();
#else
            var pc = Object.FindObjectOfType<PlayerController>();
#endif
            return pc != null ? pc.transform : null;
        }
    }
}
