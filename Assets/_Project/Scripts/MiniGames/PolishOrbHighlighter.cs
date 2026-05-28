// SPDX-License-Identifier: MIT
// Hearthbound Hollow — MiniGames / PolishOrbHighlighter
//
// PHASE 32.16 — screen-space rewrite of the polish "where do I click?" aid.
//
// The 32.14 LineRenderer version of this highlighter relied on
// `Shader.Find("Universal Render Pipeline/Unlit")` succeeding and the
// LineRenderer rendering correctly through arbitrary world geometry. On
// the user's URP-Mobile + ASE-patched shader setup the ring rendered
// either invisibly or hidden behind the workbench mesh — the video
// playtest showed a small grey orb with NO visible glow ring, and the
// HUD's "around the glowing orb" copy lied to the player.
//
// Phase 32.16 ditches the world-space LineRenderer for a guaranteed-
// visible **screen-space overlay canvas** drawn at the orb's projected
// screen position, sized to match PolishMiniGame.polishRadiusPx +
// coreRadiusPx exactly. Players see EXACTLY where they need to drag
// their cursor — the visual matches the input gate to the pixel.
//
//   * Outer ring  → polishRadiusPx        ("draw inside this circle")
//   * Inner ring  → coreRadiusPx          ("don't scrub on the core")
//   * 4 wedges    → quadrant coverage     (fill gold as covered)
//   * Pulse alpha + width keyed to remaining quadrants
//   * Cursor halo (small dot at cursor) so the player can find the
//     cursor on a busy screen
//
// All drawn via `UnityEngine.UI.Image` with a procedurally-built circle
// sprite (transparent centre) — no shader-find, no depth issues, no
// world-geometry occlusion. Camera dolly-in is kept as a separate
// effect that only fires if SmoothFollowCamera is still in control
// (DialogueCameraDirector takes over during dialogue — if it's active,
// the polish camera nudge is skipped because the dialogue director's
// over-shoulder shot is already cinematic).

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HearthboundHollow.Core;
using HearthboundHollow.Player;

namespace HearthboundHollow.MiniGames
{
    [DefaultExecutionOrder(70)]
    public class PolishOrbHighlighter : MonoBehaviour
    {
        // ───── Source ──────────────────────────────────────────────

        [Header("Source")]
        public PolishMiniGame game;

        // ───── Tunables ────────────────────────────────────────────

        [Header("Visuals")]
        [Tooltip("Idle ring colour (un-covered quadrants).")]
        public Color ringIdleColor    = new Color(1.00f, 0.78f, 0.32f, 0.85f);
        [Tooltip("Solid-gold colour as all 4 quadrants are covered.")]
        public Color ringCoveredColor = new Color(1.00f, 0.92f, 0.52f, 1.00f);
        [Tooltip("Core 'don't scrub here' dim band colour.")]
        public Color coreColor        = new Color(0.62f, 0.46f, 0.20f, 0.45f);
        [Tooltip("Cursor halo (small dot at the cursor position).")]
        public Color cursorColor      = new Color(1.00f, 0.95f, 0.78f, 0.90f);

        [Range(0.3f, 4f)] public float pulseHz = 1.2f;
        [Range(4f, 30f)] public float ringThicknessPx = 14f;

        [Header("Orb scale pulse (3D)")]
        [Range(1.0f, 1.2f)] public float scalePulseMax = 1.06f;

        [Header("Camera dolly hint (skipped if DialogueCameraDirector is active)")]
        public bool dollyCameraIn = true;
        [Range(1.5f, 5f)] public float dollyDistance = 3.2f;
        [Range(0.5f, 60f)] public float dollyPitch = 32f;

        // ───── Runtime state ──────────────────────────────────────

        private Canvas _canvas;
        private RectTransform _outerRingRT;
        private RectTransform _innerRingRT;
        private RectTransform _cursorDotRT;
        private Image _outerRing;
        private Image _innerRing;
        private Image _cursorDot;

        private Sprite _ringSprite;
        private Sprite _fillSprite;

        private Transform _orbT;
        private MemoryOrbInteractable _orb;
        private Vector3 _orbBaseScale;
        private SmoothFollowCamera _cam;
        private float _savedCamDistance;
        private float _savedCamPitch;
        private bool _dollyApplied;

        // ───── Wiring ─────────────────────────────────────────────

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

            BuildCanvasIfMissing();
            SetVisible(true);

            // Phase 32.16 — only apply the dolly if the SmoothFollowCamera is
            // currently in control. DialogueCameraDirector (Phase 32.12) takes
            // over the camera transform during dialogue and disables the
            // SmoothFollowCamera component — pushing distance/pitch on a
            // disabled component does nothing visible. The cinematic over-
            // shoulder shot is already framing the workbench area, so we
            // leave the camera alone in that case.
            _dollyApplied = false;
            if (dollyCameraIn && Camera.main != null)
            {
                _cam = Camera.main.GetComponent<SmoothFollowCamera>();
                if (_cam != null && _cam.enabled)
                {
                    _savedCamDistance = _cam.distance;
                    _savedCamPitch    = _cam.pitch;
                    _cam.distance = dollyDistance;
                    _cam.pitch    = dollyPitch;
                    _dollyApplied = true;
                }
            }
        }

        public void Unbind()
        {
            SetVisible(false);
            if (_orbT != null) _orbT.localScale = _orbBaseScale;

            if (_cam != null && _dollyApplied)
            {
                _cam.distance = _savedCamDistance;
                _cam.pitch    = _savedCamPitch;
            }
            _cam = null;
            _dollyApplied = false;
            _orb = null;
            _orbT = null;
        }

        private void OnDestroy()
        {
            if (_canvas != null) Destroy(_canvas.gameObject);
            if (_ringSprite != null) Destroy(_ringSprite);
            if (_fillSprite != null) Destroy(_fillSprite);
        }

        private void SetVisible(bool on)
        {
            if (_canvas != null) _canvas.gameObject.SetActive(on);
        }

        // ───── Canvas build (screen-space overlay, sortingOrder 80) ─

        private void BuildCanvasIfMissing()
        {
            if (_canvas != null) return;

            _ringSprite = BuildRingSprite();
            _fillSprite = BuildFillSprite();

            var go = new GameObject("PolishOrbHighlighter_Canvas",
                                    typeof(Canvas), typeof(CanvasScaler));
            _canvas = go.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 80; // above gameplay, below dialogue (90) and pause (100)

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

            // Outer ring (the "draw inside here" guide).
            _outerRingRT = MakeImage(go.transform, "OuterRing", _ringSprite, ringIdleColor,
                                     out _outerRing);
            // Inner ring (the "don't scrub here" core).
            _innerRingRT = MakeImage(go.transform, "InnerRing", _ringSprite, coreColor,
                                     out _innerRing);
            // Cursor dot — tiny filled circle painted at cursor position.
            _cursorDotRT = MakeImage(go.transform, "CursorDot", _fillSprite, cursorColor,
                                     out _cursorDot);
        }

        private static RectTransform MakeImage(Transform parent, string n, Sprite sprite,
                                               Color c, out Image img)
        {
            var go = new GameObject(n, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.color = c;
            img.raycastTarget = false;
            return (RectTransform)go.transform;
        }

        // Procedurally builds a 128×128 transparent-centre ring sprite so we
        // never depend on a project asset. Anti-aliased edge falls off
        // smoothly so the ring reads as a glow rather than a hard band.
        private static Sprite BuildRingSprite()
        {
            const int N = 128;
            const float outer = 60f;  // px from centre (radius)
            const float inner = 44f;
            var tex = new Texture2D(N, N, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            var pixels = new Color32[N * N];
            for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float dx = x - N * 0.5f + 0.5f;
                float dy = y - N * 0.5f + 0.5f;
                float r = Mathf.Sqrt(dx * dx + dy * dy);
                float a = 0f;
                if (r >= inner && r <= outer)
                {
                    // Anti-aliased radial falloff toward the centre of the ring band.
                    float t = Mathf.Abs((r - (inner + outer) * 0.5f) / ((outer - inner) * 0.5f));
                    a = Mathf.Clamp01(1f - t);
                    a = Mathf.SmoothStep(0f, 1f, a);
                }
                pixels[y * N + x] = new Color(1f, 1f, 1f, a);
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, N, N), new Vector2(0.5f, 0.5f), 100f);
        }

        // Filled disk for the cursor dot.
        private static Sprite BuildFillSprite()
        {
            const int N = 64;
            const float radius = 28f;
            var tex = new Texture2D(N, N, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            var pixels = new Color32[N * N];
            for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float dx = x - N * 0.5f + 0.5f;
                float dy = y - N * 0.5f + 0.5f;
                float r = Mathf.Sqrt(dx * dx + dy * dy);
                float a = Mathf.Clamp01(1f - (r - (radius - 4f)) / 4f);
                pixels[y * N + x] = new Color(1f, 1f, 1f, a);
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, N, N), new Vector2(0.5f, 0.5f), 100f);
        }

        // ───── Update — drive the screen-space ring + orb pulse ────

        private void LateUpdate()
        {
            if (_orbT == null || _canvas == null || game == null) return;
            if (Camera.main == null) return;

            // 1. Project the orb's world position to screen.
            Vector3 sp = Camera.main.WorldToScreenPoint(_orbT.position);
            bool offScreen = sp.z < 0f;

            // 2. Place the outer ring at the orb's projected position, sized
            //    to PolishMiniGame.PolishRadiusPx. Hide if behind camera.
            float outerDiameter = game.PolishRadiusPx * 2f;
            float innerDiameter = Mathf.Max(20f, game.CoreRadiusPx * 2f);

            _outerRingRT.sizeDelta = new Vector2(outerDiameter, outerDiameter);
            _innerRingRT.sizeDelta = new Vector2(innerDiameter, innerDiameter);
            _outerRingRT.position = sp;
            _innerRingRT.position = sp;
            _outerRing.enabled = !offScreen;
            _innerRing.enabled = !offScreen;

            // 3. Pulse colour by quadrant coverage.
            int coveredCount = game.QuadrantCoverageCount;
            float coverT = coveredCount / 4f;
            float pulse  = 0.5f + 0.5f * Mathf.Sin(Time.time * pulseHz * Mathf.PI * 2f);

            Color outerC = Color.Lerp(ringIdleColor, ringCoveredColor, coverT);
            outerC.a = Mathf.Lerp(ringIdleColor.a + 0.15f * pulse, 1f, coverT);
            _outerRing.color = outerC;

            // 4. Cursor dot — show at cursor screen position while polishing.
            if (game.IsPointerActive)
            {
                _cursorDot.enabled = true;
                _cursorDotRT.position = game.LastPointerScreenPos;
                _cursorDotRT.sizeDelta = new Vector2(28f, 28f);
            }
            else
            {
                _cursorDot.enabled = false;
            }

            // 5. Orb scale pulse for the 3D mesh too.
            float scaleT = pulse;
            float k = Mathf.Lerp(1f, scalePulseMax, scaleT * (1f - 0.5f * coverT));
            _orbT.localScale = _orbBaseScale * k;
        }
    }
}
