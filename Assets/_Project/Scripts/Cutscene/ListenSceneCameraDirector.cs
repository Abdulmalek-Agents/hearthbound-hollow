// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Cutscene / ListenSceneCameraDirector
//
// Runtime camera path for the 180-second Listen Scene cutscene
// (Mission 2 Moral Choice = Listen).
//
// Phase 36 authored the Listen Scene PlayableAsset with 4 tracks but the
// Cinemachine track is empty — we don't take a hard Cinemachine package
// dependency. Phase 42 wires this runtime director to subscribe to the
// ListenSceneSequencer's playback events and tween the **main camera**
// through 4 preset waypoints over the 180 s monologue:
//
//   [0-30 s]    Wide establishing shot of the cottage room
//   [30-90 s]   Tight on Gerrold's chair (the iconic "his chair" beat
//               from Focus 05 § 3.6 Variant D)
//   [90-150 s]  Tight on Gerrold's hands (handkerchief beat)
//   [150-180 s] Slow pull back to wide → fade to dream
//
// The waypoint Transforms are configured in the Inspector by Phase 38's
// scene builder (TBD — Phase 42 ships the director and a sensible
// fallback that synthesises waypoints from the Gerrold transform if
// the waypoint refs are null).
//
// The cottage Main Camera's pre-cutscene Transform is captured on
// Awake and restored when the cutscene ends, so the player's normal
// camera control is untouched.

using System.Collections;
using UnityEngine;
using HearthboundHollow.Core;

namespace HearthboundHollow.Cutscene
{
    [DisallowMultipleComponent]
    public class ListenSceneCameraDirector : MonoBehaviour
    {
        [Header("Sequencer ref (wired by Phase 42 builder)")]
        public ListenSceneSequencer sequencer;

        [Header("Waypoints (wired by Phase 42 builder; auto-synth from gerroldFocus when null)")]
        public Transform waypoint0_WideEstablish;
        public Transform waypoint1_TightChair;
        public Transform waypoint2_TightHands;
        public Transform waypoint3_PullBack;

        [Header("Fallback synth source")]
        [Tooltip("If the waypoints above are null, the director synthesises " +
                 "them by offsetting from this transform (Gerrold or his chair). " +
                 "The synth is good-enough for ship; an artist can replace " +
                 "with real waypoints later.")]
        public Transform gerroldFocus;

        [Header("Timing")]
        [Tooltip("Beat durations in seconds (must sum to 180).")]
        public float beat0_WideEstablish = 30f;
        public float beat1_TightChair    = 60f;
        public float beat2_TightHands    = 60f;
        public float beat3_PullBack      = 30f;

        [Header("Easing")]
        [Range(0.5f, 4f)] public float easeExponent = 1.6f;

        // ─── Lifecycle ──────────────────────────────────────────────

        private Camera _mainCam;
        private Vector3 _preCutscenePosition;
        private Quaternion _preCutsceneRotation;
        private bool _isPlaying;
        private Coroutine _activeRoutine;

        private void Start()
        {
            _mainCam = Camera.main;
            if (sequencer != null)
            {
                sequencer.OnListenStarted += OnListenStarted;
                sequencer.OnListenCompleted += OnListenCompleted;
            }
            else
            {
                Hh.Warn(LogCategory.Cutscene,
                    "ListenSceneCameraDirector: sequencer ref is null — " +
                    "drag the cottage's ListenSceneSequencer onto this director.");
            }
        }

        private void OnDestroy()
        {
            if (sequencer != null)
            {
                sequencer.OnListenStarted -= OnListenStarted;
                sequencer.OnListenCompleted -= OnListenCompleted;
            }
        }

        // ─── Playback ───────────────────────────────────────────────

        private void OnListenStarted()
        {
            if (_isPlaying) return;
            _isPlaying = true;
            _mainCam = Camera.main;
            if (_mainCam == null)
            {
                Hh.Warn(LogCategory.Cutscene,
                    "ListenSceneCameraDirector: no Camera.main found — skipping path.");
                _isPlaying = false;
                return;
            }
            _preCutscenePosition = _mainCam.transform.position;
            _preCutsceneRotation = _mainCam.transform.rotation;
            _activeRoutine = StartCoroutine(PlayPath());
            Hh.Log(LogCategory.Cutscene, "ListenSceneCameraDirector: cinematic camera path started.");
        }

        private void OnListenCompleted()
        {
            if (!_isPlaying) return;
            _isPlaying = false;
            if (_activeRoutine != null)
            {
                StopCoroutine(_activeRoutine);
                _activeRoutine = null;
            }
            if (_mainCam != null)
            {
                _mainCam.transform.position = _preCutscenePosition;
                _mainCam.transform.rotation = _preCutscenePosition.normalized != Vector3.zero
                    ? _preCutsceneRotation
                    : _mainCam.transform.rotation;
            }
            Hh.Log(LogCategory.Cutscene, "ListenSceneCameraDirector: camera restored.");
        }

        private IEnumerator PlayPath()
        {
            // Resolve waypoints — either Inspector-supplied or synthesised.
            var (w0, w1, w2, w3) = ResolveWaypoints();

            // Beat 0 — wide establishing → tight chair
            yield return Travel(w0, w1, beat0_WideEstablish);
            if (!_isPlaying) yield break;

            // Beat 1 — tight chair → tight hands
            yield return Travel(w1, w2, beat1_TightChair);
            if (!_isPlaying) yield break;

            // Beat 2 — tight hands → final wide pull-back
            yield return Travel(w2, w3, beat2_TightHands);
            if (!_isPlaying) yield break;

            // Beat 3 — hold on the pull-back; this is where Gerrold's smile
            // lands and the dream transition fades in.
            yield return Travel(w3, w3, beat3_PullBack);
        }

        private IEnumerator Travel((Vector3 pos, Quaternion rot) a, (Vector3 pos, Quaternion rot) b, float duration)
        {
            float t = 0f;
            while (t < duration && _isPlaying)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / duration);
                // Smooth-step (cubic) easing so each beat begins and ends gently
                float eased = Mathf.Pow(k, easeExponent) / (Mathf.Pow(k, easeExponent) + Mathf.Pow(1f - k, easeExponent));
                if (_mainCam != null)
                {
                    _mainCam.transform.position = Vector3.Lerp(a.pos, b.pos, eased);
                    _mainCam.transform.rotation = Quaternion.Slerp(a.rot, b.rot, eased);
                }
                yield return null;
            }
        }

        // ─── Waypoint resolution ────────────────────────────────────

        private ((Vector3, Quaternion) w0, (Vector3, Quaternion) w1, (Vector3, Quaternion) w2, (Vector3, Quaternion) w3)
            ResolveWaypoints()
        {
            // If every waypoint is Inspector-supplied, just use them.
            if (waypoint0_WideEstablish != null && waypoint1_TightChair != null
                && waypoint2_TightHands != null && waypoint3_PullBack != null)
            {
                return (
                    (waypoint0_WideEstablish.position, waypoint0_WideEstablish.rotation),
                    (waypoint1_TightChair.position, waypoint1_TightChair.rotation),
                    (waypoint2_TightHands.position, waypoint2_TightHands.rotation),
                    (waypoint3_PullBack.position, waypoint3_PullBack.rotation)
                );
            }

            // Synthesised fallback — frame Gerrold's focus point.
            Vector3 focus = gerroldFocus != null
                ? gerroldFocus.position
                : (_mainCam != null ? _mainCam.transform.position + _mainCam.transform.forward * 4f : Vector3.zero);

            // Wide: behind-and-up from focus
            var w0Pos = focus + new Vector3(0f, 2.6f, -4.5f);
            var w0Rot = Quaternion.LookRotation((focus - w0Pos).normalized, Vector3.up);

            // Tight chair: closer, slightly low
            var w1Pos = focus + new Vector3(0.5f, 1.4f, -2.0f);
            var w1Rot = Quaternion.LookRotation((focus - w1Pos).normalized, Vector3.up);

            // Tight hands: near the lap, mid-height
            var w2Pos = focus + new Vector3(-0.4f, 1.1f, -1.4f);
            var w2Rot = Quaternion.LookRotation((focus + Vector3.down * 0.2f - w2Pos).normalized, Vector3.up);

            // Pull back: wide, slow drift
            var w3Pos = focus + new Vector3(0f, 2.4f, -5.5f);
            var w3Rot = Quaternion.LookRotation((focus - w3Pos).normalized, Vector3.up);

            return ((w0Pos, w0Rot), (w1Pos, w1Rot), (w2Pos, w2Rot), (w3Pos, w3Rot));
        }
    }
}
