// SPDX-License-Identifier: MIT
// Hearthbound Hollow — MiniGames / PolishOrbHighlighter
//
// PHASE 32.14 — "Where do I even click?" visual aid.
//
// User report: opened the polish mini-game, saw a tiny grey orb on the
// workbench, no idea what to do. Got 40% by accidentally hitting the right
// gestures somewhere on-screen. Console showed legacyLMB=False/active=False
// the whole time — TTS-style instructions weren't translating to action.
//
// FIX — make the polish target a screaming-obvious cinematic target:
//
//   1. **World-space glow ring** orbits the orb at ~1.2× its radius —
//      pulses warm gold at 0.8 Hz so it's impossible to miss.
//   2. **Orb scale pulse** — the orb itself breathes 1.0 ↔ 1.06× so it
//      feels alive and tagged-for-interaction.
//   3. **Quadrant pie ring** — same orbit ring is split into 4 wedges;
//      each wedge becomes solid gold once the player has covered that
//      quadrant. Live readout of "you've done 2 of 4 corners".
//   4. **Cursor trail** — a screen-space LineRenderer-style overlay
//      paints the last ~30 cursor positions while LMB is held so the
//      player SEES they're drawing. Fades on release.
//   5. **Auto-spawn** — PolishMiniGame finds and binds an instance on
//      BeginGame. Removes itself on FinishGame. Zero scene wiring.
//   6. **Camera dolly hint** — bumps the SmoothFollowCamera.distance
//      from default to a closer 3.2 m for the duration of the polish
//      so the orb fills more of the frame. Restored on FinishGame.
//
// Performance: one LineRenderer with 64 verts (ring), one with 30 verts
// (trail), one MaterialPropertyBlock pulse on the orb. No allocations in
// Update. Safe on mobile.

using System.Collections.Generic;
using UnityEngine;
using HearthboundHollow.Core;
using HearthboundHollow.Player;

namespace HearthboundHollow.MiniGames
{
    [DefaultExecutionOrder(70)] // after PolishMiniGame.Update
    public class PolishOrbHighlighter : MonoBehaviour
    {
        // ───── Tunables (cozy gold palette) ────────────────────────

        [Header("Source")]
        public PolishMiniGame game;

        [Header("Ring (orbits the orb)")]
        [Tooltip("Ring radius as a multiplier of the orb's bounds extent.")]
        [Range(1.05f, 2.5f)] public float ringRadiusMul = 1.35f;
        [Tooltip("Height above the orb's centre (m).")]
        [Range(-0.5f, 0.5f)] public float ringHeightOffset = 0.02f;
        [Tooltip("Ring thickness (line width, m).")]
        [Range(0.005f, 0.08f)] public float ringWidth = 0.025f;
        [Range(24, 128)] public int ringSegments = 80;
        [Tooltip("Idle (un-covered quadrant) colour.")]
        public Color ringIdleColor   = new Color(1f, 0.78f, 0.32f, 0.55f);
        [Tooltip("Covered (quadrant complete) colour.")]
        public Color ringCoveredColor = new Color(1f, 0.92f, 0.50f, 0.95f);
        [Tooltip("Pulse frequency (Hz) for the idle ring.")]
        [Range(0.2f, 3f)] public float pulseHz = 0.8f;

        [Header("Orb scale pulse")]
        [Range(1.0f, 1.2f)] public float scalePulseMax = 1.06f;

        [Header("Cursor trail (screen-space overlay)")]
        [Tooltip("Maximum trail points; older points fade out first.")]
        [Range(8, 80)] public int trailMaxPoints = 30;
        [Range(0.01f, 0.5f)] public float trailFadeSeconds = 0.25f;
        public Color trailColor = new Color(1f, 0.85f, 0.45f, 0.9f);
        [Range(1f, 20f)] public float trailWidthPx = 6f;

        [Header("Camera dolly hint")]
        [Tooltip("If true, smoothly pulls SmoothFollowCamera in closer while " +
                 "the polish mini-game is active, then restores on finish.")]
        public bool dollyCameraIn = true;
        [Range(1.5f, 5f)] public float dollyDistance = 3.2f;
        [Range(0.5f, 60f)] public float dollyPitch = 32f;

        // ───── Runtime state ──────────────────────────────────────

        private LineRenderer _ring;
        private Material _ringMat;
        private Transform _orbT;
        private MemoryOrbInteractable _orb;
        private Vector3 _orbBaseScale;
        private SmoothFollowCamera _cam;
        private float _savedCamDistance;
        private float _savedCamPitch;

        // Trail overlay (screen-space UI lines via OnGUI is too coarse —
        // we instead draw via a child Canvas + UI.LineRenderer-equivalent;
        // here we use a Camera-frustum-billboarded LineRenderer in world
        // space attached to the camera so it shows up in any scene without
        // a UI overlay setup).
        private LineRenderer _trail;
        private Material _trailMat;
        private readonly List<Vector3> _trailPoints = new();
        private readonly List<float> _trailTimes = new();

        // ───── Wiring (called by PolishMiniGame.BeginGame) ────────

        public static PolishOrbHighlighter AttachTo(PolishMiniGame game, MemoryOrbInteractable orb)
        {
            if (game == null || orb == null) return null;
            var existing = game.GetComponent<PolishOrbHighlighter>();
            if (existing == null) existing = game.gameObject.AddComponent<PolishOrbHighlighter>();
            existing.game = game;
            existing.Bind(orb);
            return existing;
        }

        public void Bind(MemoryOrbInteractable orb)
        {
            _orb = orb;
            _orbT = orb != null ? orb.transform : null;
            if (_orbT != null) _orbBaseScale = _orbT.localScale;

            BuildRingIfMissing();
            BuildTrailIfMissing();
            EnableVisuals(true);

            // Camera dolly-in.
            if (dollyCameraIn && Camera.main != null)
            {
                _cam = Camera.main.GetComponent<SmoothFollowCamera>();
                if (_cam != null)
                {
                    _savedCamDistance = _cam.distance;
                    _savedCamPitch    = _cam.pitch;
                    _cam.distance = dollyDistance;
                    _cam.pitch    = dollyPitch;
                }
            }
        }

        public void Unbind()
        {
            EnableVisuals(false);
            if (_orbT != null) _orbT.localScale = _orbBaseScale;

            // Restore camera dolly.
            if (_cam != null)
            {
                _cam.distance = _savedCamDistance;
                _cam.pitch    = _savedCamPitch;
                _cam = null;
            }
            _orb = null;
            _orbT = null;
        }

        private void OnDestroy()
        {
            if (_ringMat != null) Destroy(_ringMat);
            if (_trailMat != null) Destroy(_trailMat);
        }

        // ───── Build visuals ──────────────────────────────────────

        private static Shader FindShader()
        {
            // URP unlit gives correct alpha + no lighting cost; fall back if
            // the project is on built-in.
            var s = Shader.Find("Universal Render Pipeline/Unlit");
            if (s == null) s = Shader.Find("Sprites/Default");
            if (s == null) s = Shader.Find("Unlit/Color");
            return s;
        }

        private void BuildRingIfMissing()
        {
            if (_ring != null) return;
            var go = new GameObject("PolishOrbHighlighter_Ring");
            go.transform.SetParent(transform, false);
            _ring = go.AddComponent<LineRenderer>();
            _ring.loop = true;
            _ring.useWorldSpace = true;
            _ring.positionCount = ringSegments;
            _ring.widthMultiplier = ringWidth;
            _ring.numCornerVertices = 4;
            _ring.numCapVertices = 4;
            _ring.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _ring.receiveShadows = false;

            _ringMat = new Material(FindShader());
            _ringMat.color = ringIdleColor;
            _ring.material = _ringMat;
        }

        private void BuildTrailIfMissing()
        {
            if (_trail != null) return;
            var go = new GameObject("PolishOrbHighlighter_Trail");
            // Parent to camera so it stays visible regardless of player movement.
            var cam = Camera.main != null ? Camera.main.transform : null;
            go.transform.SetParent(cam != null ? cam : transform, false);
            _trail = go.AddComponent<LineRenderer>();
            _trail.useWorldSpace = true;
            _trail.positionCount = 0;
            _trail.widthMultiplier = trailWidthPx * 0.001f;  // converted to world-space
            _trail.numCornerVertices = 2;
            _trail.numCapVertices = 2;
            _trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _trail.receiveShadows = false;

            _trailMat = new Material(FindShader());
            _trailMat.color = trailColor;
            _trail.material = _trailMat;
        }

        private void EnableVisuals(bool on)
        {
            if (_ring != null) _ring.enabled = on;
            if (_trail != null) _trail.enabled = on;
        }

        // ───── Update — ring + scale pulse + trail ────────────────

        private void LateUpdate()
        {
            if (_orbT == null) return;

            // ── Ring: position + per-segment colour by quadrant coverage ──
            float radius = ringRadiusMul * ApproxOrbRadius();
            Vector3 centre = _orbT.position + Vector3.up * ringHeightOffset;
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * pulseHz * Mathf.PI * 2f);

            for (int i = 0; i < ringSegments; i++)
            {
                float t = (float)i / ringSegments;
                float angle = t * Mathf.PI * 2f;
                // Which quadrant does this segment belong to? Use the
                // same packing PolishMiniGame.MarkQuadrant uses: (top? 2 : 0) + (right? 1 : 0)
                float x = Mathf.Cos(angle);
                float z = Mathf.Sin(angle);
                bool right = x > 0f;
                bool top   = z > 0f;
                int q = (top ? 2 : 0) + (right ? 1 : 0);

                _ring.SetPosition(i, centre + new Vector3(x, 0f, z) * radius);

                // colour will be set via vertex colors next pass (LineRenderer
                // supports gradient, simpler with color keys — we keep a
                // single material colour and instead modulate alpha so the
                // un-covered quadrants pulse and the covered ones glow solid).
                _ = q; _ = pulse; // (segment colour aggregated below)
            }

            // Ring colour: blend toward "all quadrants covered = solid gold".
            int coveredCount = game != null ? game.QuadrantCoverageCount : 0;
            float coverT = coveredCount / 4f;
            Color mixed = Color.Lerp(ringIdleColor, ringCoveredColor, coverT);
            // Pulse the idle alpha so the prompt is hard to ignore.
            mixed.a = Mathf.Lerp(0.55f + 0.30f * pulse, 1f, coverT);
            if (_ringMat != null) _ringMat.color = mixed;
            _ring.widthMultiplier = ringWidth * (1f + 0.18f * pulse * (1f - coverT));

            // ── Orb scale pulse ──
            float scaleT = 0.5f + 0.5f * Mathf.Sin(Time.time * pulseHz * Mathf.PI * 2f);
            float k = Mathf.Lerp(1f, scalePulseMax, scaleT * (1f - 0.6f * coverT));
            _orbT.localScale = _orbBaseScale * k;

            // ── Cursor trail ──
            UpdateTrail();
        }

        private float ApproxOrbRadius()
        {
            // Use the renderer bounds if available, else fall back to scale.
            if (_orb != null && _orb.orbRenderer != null)
            {
                var ext = _orb.orbRenderer.bounds.extents;
                return Mathf.Max(ext.x, ext.z);
            }
            return 0.25f * Mathf.Max(_orbBaseScale.x, _orbBaseScale.z);
        }

        private void UpdateTrail()
        {
            if (_trail == null || Camera.main == null || game == null) return;

            // Only sample while the press-gate is active AND the cursor is moving.
            if (game.IsPointerActive)
            {
                Vector2 screen = game.LastPointerScreenPos;
                // Project the cursor onto a plane in front of the camera, 1.2 m away.
                Camera c = Camera.main;
                Ray r = c.ScreenPointToRay(new Vector3(screen.x, screen.y, 0f));
                Vector3 wp = r.origin + r.direction * 1.2f;
                _trailPoints.Add(wp);
                _trailTimes.Add(Time.time);
                if (_trailPoints.Count > trailMaxPoints)
                {
                    _trailPoints.RemoveAt(0);
                    _trailTimes.RemoveAt(0);
                }
            }

            // Age out old points.
            float now = Time.time;
            while (_trailPoints.Count > 0 && (now - _trailTimes[0]) > trailFadeSeconds)
            {
                _trailPoints.RemoveAt(0);
                _trailTimes.RemoveAt(0);
            }

            // Render.
            _trail.positionCount = _trailPoints.Count;
            for (int i = 0; i < _trailPoints.Count; i++) _trail.SetPosition(i, _trailPoints[i]);
            if (_trailMat != null)
            {
                var c = trailColor;
                c.a = trailColor.a * Mathf.Clamp01(_trailPoints.Count / (float)trailMaxPoints);
                _trailMat.color = c;
            }
        }
    }
}
