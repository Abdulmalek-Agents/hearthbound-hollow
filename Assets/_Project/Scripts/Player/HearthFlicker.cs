// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Player / HearthFlicker
//
// Phase 47.5 runtime — Procedural fireplace-light flicker.
//
// A composed sum of two sine waves at incommensurate frequencies + a tiny
// uniform-noise jitter makes a believable mid-low-frequency hearth flicker
// without per-frame Random.Range hot-paths (Random.Range allocates and is
// noisy for cozy interior lighting).
//
// Used by Phase 47.5's cottage hearth and can also be retro-fitted to
// Phase 32.3's Hollow hearth or any other realtime point light.

using UnityEngine;

namespace HearthboundHollow.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Light))]
    public sealed class HearthFlicker : MonoBehaviour
    {
        [SerializeField] public float baseIntensity    = 3.2f;
        [SerializeField] public float flickerAmplitude = 0.6f;
        [SerializeField] public float flickerSpeed     = 4.0f;
        [SerializeField] public float colorWarmShift   = 0.05f;

        private Light _light;
        private Color _baseColor;
        private float _phaseA;
        private float _phaseB;

        private void Awake()
        {
            _light = GetComponent<Light>();
            if (_light == null) return;
            _baseColor = _light.color;
            if (baseIntensity > 0f && _light.intensity == 0f)
                _light.intensity = baseIntensity;

            // Per-instance random phase so multiple hearths don't flicker in sync.
            _phaseA = Random.value * Mathf.PI * 2f;
            _phaseB = Random.value * Mathf.PI * 2f;
        }

        private void Update()
        {
            if (_light == null) return;

            float t = Time.time;
            float a = Mathf.Sin(t * flickerSpeed       + _phaseA);
            float b = Mathf.Sin(t * (flickerSpeed * 0.61f) + _phaseB) * 0.5f;
            float jitter = (Mathf.PerlinNoise(t * 2.3f, _phaseA) - 0.5f) * 0.3f;
            float wave = a + b + jitter;

            _light.intensity = Mathf.Max(0.2f, baseIntensity + wave * flickerAmplitude);

            // Subtle colour warm-shift as flame "breathes"
            float k = wave * 0.5f + 0.5f;
            _light.color = new Color(
                _baseColor.r,
                Mathf.Lerp(_baseColor.g, _baseColor.g + colorWarmShift, k),
                Mathf.Lerp(_baseColor.b, _baseColor.b - colorWarmShift * 0.5f, k),
                _baseColor.a);
        }
    }
}
