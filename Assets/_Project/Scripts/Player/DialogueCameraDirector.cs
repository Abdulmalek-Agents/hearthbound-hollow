// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Player / DialogueCameraDirector
//
// PHASE 32.12 — Cinematic dialogue framing.   PHASE 62.C (this pass) — anti-clip fix.
//
// User report (with screenshot + the 8-min playtest video): during dialogue the
// camera dives INTO the player — a giant close-up of a leg / shoulder fills the
// screen while a line plays (also leaks into the Polish mini-game that follows a
// dialogue burst, since the director is still holding the shot).
//
// ROOT CAUSE (fixed here): the over-the-shoulder shot sphere-cast from the
// SPEAKER toward the desired camera position (which sits BEHIND the player). That
// ray passes straight THROUGH the player's own collider, so the clip "slides the
// camera forward to the first hit" — i.e. onto the player's body. The wide /
// narrator cast started INSIDE the player pivot and start-overlapped the player
// collider the same way. SmoothFollowCamera already ignores the player's colliders
// (Phase 32.21); this director did NOT. Now it does: both clip casts use
// SphereCastNonAlloc and skip every collider belonging to the player OR the current
// speaker, plus zero-distance start-overlaps. Framing standoffs were also widened so
// cramped rooms can't jam the camera against the body.
//
// This is the "Witcher / Disco Elysium / Spiritfarer" cozy default — one continuous
// spring-damped over-the-shoulder hold while a character speaks, never inside a body.
//
// ── Wiring ────────────────────────────────────────
//   * Auto-spawned by SmoothFollowCamera at runtime if missing.
//   * Singleton; subscribes to DialogueLineStarted/Ended on the EventBus.
//   * Directors register speakers via RegisterSpeaker (D-071).

using System.Collections.Generic;
using UnityEngine;
using HearthboundHollow.Core;

namespace HearthboundHollow.Player
{
    [DefaultExecutionOrder(50)] // After SmoothFollowCamera's LateUpdate
    public class DialogueCameraDirector : MonoBehaviour
    {
        public static DialogueCameraDirector Instance { get; private set; }

        private static readonly System.Collections.Generic.Dictionary<string, Transform> _registry
            = new(System.StringComparer.OrdinalIgnoreCase);

        /// <summary>Register (or update) the scene Transform for a dialogue speaker name.</summary>
        public static void RegisterSpeaker(string speakerName, Transform t)
        {
            if (string.IsNullOrEmpty(speakerName) || t == null) return;
            _registry[speakerName] = t;
        }

        // ───── Tunables ─────────────────────────────

        [Header("Targets")]
        public Transform cameraTransform;
        public SmoothFollowCamera follow;
        public Transform player;

        [Header("Speaker name → scene GameObject overrides")]
        public List<SpeakerOverride> speakerOverrides = new();

        [System.Serializable]
        public struct SpeakerOverride { public string speakerName; public Transform target; }

        [Header("Shot framing — over-the-shoulder default")]
        [Range(-3f, 3f)] public float shoulderOffset = 1.15f;
        [Range(0.5f, 3f)] public float cameraHeight = 1.8f;
        [Range(0.5f, 5f)] public float behindDistance = 2.5f;
        [Range(0.5f, 2.5f)] public float speakerLookHeight = 1.5f;
        [Tooltip("Hard minimum distance between the camera and the player pivot (m). " +
                 "The clip can never place the camera closer than this — instead it " +
                 "lifts the camera up and back, so we never end up inside the body.")]
        [Range(0.8f, 3f)] public float minPlayerClearance = 1.6f;

        [Header("Wide / narrator fallback (no speaker found)")]
        [Range(2f, 8f)] public float wideDistance = 4.8f;
        [Range(5f, 45f)] public float widePitch = 20f;

        [Header("Smoothing")]
        [Range(0.05f, 0.6f)] public float positionSmoothTime = 0.22f;
        [Range(2f, 30f)] public float rotationLerp = 10f;
        [Range(0.1f, 2f)] public float postDialogueHold = 0.55f;

        [Header("Anti-clip")]
        [Range(0.1f, 0.8f)] public float clipRadius = 0.28f;
        [Tooltip("Geometry layers used for camera clip. Defaults to everything " +
                 "EXCEPT TransparentFX/IgnoreRaycast/UI.")]
        public LayerMask clipMask = ~((1 << 1) | (1 << 2) | (1 << 5));

        // ───── State ──────────────────────────────

        private bool _dialogueActive;
        private float _exitTimer;
        private Transform _currentSpeaker;
        private string _currentSpeakerName;
        private Vector3 _posVel;
        private Vector3 _smoothedCamPos;

        // Anti-clip: cached colliders to ignore (player + current speaker) + a
        // reusable cast buffer so the clip probe stays allocation-free.
        private readonly RaycastHit[] _hits = new RaycastHit[12];
        private Transform _playerColCacheFor;
        private Collider[] _playerCols;
        private Transform _speakerColCacheFor;
        private Collider[] _speakerCols;

        // ───── Lifecycle ──────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            // Enforce sane minimums even if a scene-baked instance serialized the
            // old, too-tight values (which caused the in-body close-ups).
            behindDistance     = Mathf.Max(behindDistance, 2.2f);
            cameraHeight       = Mathf.Max(cameraHeight, 1.7f);
            minPlayerClearance = Mathf.Max(minPlayerClearance, 1.4f);
            ResolveRefs();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<DialogueLineStartedEvent>(OnLineStarted);
            EventBus.Subscribe<DialogueLineEndedEvent>(OnLineEnded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<DialogueLineStartedEvent>(OnLineStarted);
            EventBus.Unsubscribe<DialogueLineEndedEvent>(OnLineEnded);
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        private void ResolveRefs()
        {
            if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
            if (follow == null && cameraTransform != null) follow = cameraTransform.GetComponent<SmoothFollowCamera>();
            if (player == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) player = p.transform;
                else if (follow != null && follow.target != null) player = follow.target;
            }
        }

        // ───── Event handlers ──────────────────────

        private void OnLineStarted(DialogueLineStartedEvent e)
        {
            ResolveRefs();
            _currentSpeakerName = e.Speaker ?? string.Empty;
            _currentSpeaker = ResolveSpeakerTransform(_currentSpeakerName);
            EnterDialogueMode();
        }

        private void OnLineEnded(DialogueLineEndedEvent _) { _exitTimer = postDialogueHold; }

        private void EnterDialogueMode()
        {
            if (cameraTransform == null) return;
            if (!_dialogueActive)
            {
                _dialogueActive = true;
                _smoothedCamPos = cameraTransform.position;
                _posVel = Vector3.zero;
                if (follow != null) follow.enabled = false;
            }
            _exitTimer = 0f;
        }

        private void ExitDialogueMode()
        {
            if (!_dialogueActive) return;
            _dialogueActive = false;
            _currentSpeaker = null;
            _currentSpeakerName = string.Empty;
            if (follow != null) { follow.enabled = true; follow.SnapToTargetImmediate(); }
        }

        // ───── Camera drive ────────────────────────

        private void LateUpdate()
        {
            if (!_dialogueActive) return;
            if (cameraTransform == null) { ExitDialogueMode(); return; }

            if (_exitTimer > 0f)
            {
                _exitTimer -= Time.unscaledDeltaTime;
                if (_exitTimer <= 0f) { ExitDialogueMode(); return; }
            }

            ComposeShot(out Vector3 desiredPos, out Quaternion desiredRot);
            _smoothedCamPos = Vector3.SmoothDamp(_smoothedCamPos, desiredPos, ref _posVel, positionSmoothTime,
                                                 maxSpeed: 30f, deltaTime: Time.unscaledDeltaTime);
            cameraTransform.position = _smoothedCamPos;
            cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, desiredRot,
                                                         rotationLerp * Time.unscaledDeltaTime);
        }

        private void ComposeShot(out Vector3 pos, out Quaternion rot)
        {
            if (_currentSpeaker == null) { ComposeWideShot(out pos, out rot); return; }
            ComposeOverShoulderShot(out pos, out rot);
        }

        private void ComposeOverShoulderShot(out Vector3 pos, out Quaternion rot)
        {
            Vector3 playerPivot = (player != null ? player.position : Vector3.zero) + Vector3.up * cameraHeight;
            Vector3 speakerPivot = _currentSpeaker.position + Vector3.up * speakerLookHeight;

            Vector3 fromSpeakerToPlayer = playerPivot - speakerPivot; fromSpeakerToPlayer.y = 0f;
            if (fromSpeakerToPlayer.magnitude < 0.01f) fromSpeakerToPlayer = -_currentSpeaker.forward;
            Vector3 backDir = fromSpeakerToPlayer.normalized;
            Vector3 rightDir = Vector3.Cross(Vector3.up, backDir).normalized;

            pos = playerPivot + backDir * behindDistance + rightDir * shoulderOffset;

            // Clip from the SPEAKER toward the desired camera position, IGNORING the
            // player's and speaker's own colliders (the fix). Only real walls stop it.
            pos = ClipBetween(speakerPivot, pos);

            // Hard clearance: if anything still pulled the camera too close to the
            // player, push it back along the look axis AND lift it up + tilt down —
            // a higher angle frames the 2-shot instead of jamming beside the body.
            Vector3 flat = pos - playerPivot; flat.y = 0f;
            if (flat.magnitude < minPlayerClearance)
            {
                pos = playerPivot + backDir * minPlayerClearance + rightDir * Mathf.Max(0.6f, shoulderOffset)
                                  + Vector3.up * 0.55f;
                pos = ClipBetween(speakerPivot, pos);
            }

            // Look at a point a third of the way from speaker → player (a gentle
            // 2-shot) so the player reads as foreground, never a full-frame blur.
            Vector3 lookAt = Vector3.Lerp(speakerPivot, playerPivot, 0.18f);
            rot = Quaternion.LookRotation(lookAt - pos, Vector3.up);
        }

        private void ComposeWideShot(out Vector3 pos, out Quaternion rot)
        {
            Vector3 playerPivot = (player != null ? player.position : Vector3.zero) + Vector3.up * cameraHeight;
            Vector3 fwd = player != null ? player.forward : (cameraTransform != null ? cameraTransform.forward : Vector3.forward);
            Vector3 right = player != null ? player.right : Vector3.right;
            Quaternion pitch = Quaternion.AngleAxis(-widePitch, right);
            Vector3 back = pitch * (-fwd);
            pos = playerPivot + back * wideDistance;

            // Cast OUTWARD from a point already clear of the player so we never
            // start-overlap the body; ignore player colliders regardless.
            Vector3 origin = playerPivot + back.normalized * Mathf.Min(minPlayerClearance, wideDistance * 0.5f);
            pos = ClipBetween(origin, pos);

            rot = Quaternion.LookRotation(playerPivot - pos, Vector3.up);
        }

        // ───── Anti-clip core ──────────────────────

        /// <summary>
        /// Sphere-cast from <paramref name="from"/> toward <paramref name="to"/>,
        /// returning the farthest unobstructed position. Ignores the player's and
        /// the current speaker's own colliders plus zero-distance start-overlaps, so
        /// the camera never slides onto a character body.
        /// </summary>
        private Vector3 ClipBetween(Vector3 from, Vector3 to)
        {
            Vector3 dir = to - from;
            float dist = dir.magnitude;
            if (dist <= 0.01f) return to;
            Vector3 ndir = dir / dist;
            int n = Physics.SphereCastNonAlloc(new Ray(from, ndir), clipRadius, _hits, dist, clipMask, QueryTriggerInteraction.Ignore);
            float nearest = float.PositiveInfinity;
            for (int i = 0; i < n; i++)
            {
                var h = _hits[i];
                if (h.collider == null || h.distance <= 0.02f) continue;
                if (IsIgnored(h.collider)) continue;
                if (h.distance < nearest) nearest = h.distance;
            }
            if (float.IsPositiveInfinity(nearest)) return to;
            return from + ndir * Mathf.Max(0.05f, nearest - clipRadius);
        }

        private bool IsIgnored(Collider c)
        {
            if (c == null) return false;
            RefreshColliderCache();
            if (_playerCols != null) for (int i = 0; i < _playerCols.Length; i++) if (_playerCols[i] == c) return true;
            if (_speakerCols != null) for (int i = 0; i < _speakerCols.Length; i++) if (_speakerCols[i] == c) return true;
            return false;
        }

        private void RefreshColliderCache()
        {
            if (player != null && _playerColCacheFor != player)
            {
                _playerCols = player.GetComponentsInChildren<Collider>(true);
                _playerColCacheFor = player;
            }
            if (_currentSpeaker != _speakerColCacheFor)
            {
                _speakerCols = _currentSpeaker != null ? _currentSpeaker.GetComponentsInChildren<Collider>(true) : null;
                _speakerColCacheFor = _currentSpeaker;
            }
        }

        // ───── Speaker discovery ──────────────────

        private Transform ResolveSpeakerTransform(string speakerName)
        {
            if (string.IsNullOrEmpty(speakerName)) return null;

            if (_registry.TryGetValue(speakerName, out var reg))
            {
                if (reg != null) return reg;
                _registry.Remove(speakerName);
            }

            foreach (var o in speakerOverrides)
                if (o.target != null && o.speakerName != null &&
                    string.Equals(o.speakerName, speakerName, System.StringComparison.OrdinalIgnoreCase))
                    return o.target;

            var direct = GameObject.Find(speakerName);
            if (direct != null) return direct.transform;
            direct = GameObject.Find("_" + speakerName);
            if (direct != null) return direct.transform;

            var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var r in roots)
            {
                if (r.name.StartsWith(speakerName, System.StringComparison.OrdinalIgnoreCase)) return r.transform;
                var nested = FindChildStartingWith(r.transform, speakerName);
                if (nested != null) return nested;
            }
            return null;
        }

        private static Transform FindChildStartingWith(Transform parent, string prefix)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var c = parent.GetChild(i);
                if (c.name.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase)) return c;
                var nested = FindChildStartingWith(c, prefix);
                if (nested != null) return nested;
            }
            return null;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!_dialogueActive || cameraTransform == null || _currentSpeaker == null) return;
            Gizmos.color = new Color(0.6f, 0.85f, 1f, 0.85f);
            Gizmos.DrawLine(cameraTransform.position, _currentSpeaker.position + Vector3.up * speakerLookHeight);
            Gizmos.DrawWireSphere(_currentSpeaker.position + Vector3.up * speakerLookHeight, 0.15f);
        }
#endif
    }
}
