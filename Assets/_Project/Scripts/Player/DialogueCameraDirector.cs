// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Player / DialogueCameraDirector
//
// PHASE 32.12 — Cinematic dialogue framing.
//
// User report (with screenshot): during dialogue the gameplay camera stays
// where it was at the moment dialogue started — often clipping into the
// player's body or pointing the wrong direction, so a closeup of the
// player's torso fills the screen while a beautiful spoken line plays.
//
// FIX — this director hooks DialogueLineStartedEvent / DialogueLineEndedEvent
// and temporarily takes the gameplay camera out of player control to land
// a real cinematic shot:
//
//   * On the FIRST line of any dialogue burst, smoothly transition into a
//     standard over-the-shoulder shot with the speaker centred and the
//     player visible at the lower-third foreground (cozy-RPG default).
//   * On every subsequent line, slightly re-frame so the camera always
//     looks toward whoever is speaking (Doris → speaker shot; Pickle →
//     stay on player, narrator-style, because Pickle has no portrait).
//   * On a sustained gap with no new line (0.5 s) we treat dialogue as
//     ended and lerp back to the SmoothFollowCamera's normal pose.
//
// This is the "Witcher / Disco Elysium / Spiritfarer" cozy default — no
// Cinemachine cuts, no hard cuts, just one continuous spring-damped
// over-the-shoulder hold while a character speaks.
//
// ── Wiring ──────────────────────────────────────────────────────
//   * Auto-spawned by SmoothFollowCamera at runtime if missing (so a
//     pulled scene with no scene-baked director still gets cinematic
//     dialogue). Belt-and-braces — works with no scene wiring.
//   * Singleton (DontDestroyOnLoad via SmoothFollowCamera's owner) so a
//     single director surveils every scene's dialogue events.
//   * Subscribes to DialogueLineStartedEvent / DialogueLineEndedEvent on
//     the EventBus, no UI dep, no Cutscene asmdef dep.
//
// ── Speaker discovery ──────────────────────────────────────────
// On line start we look up the speaker's Transform by name in the
// active scene. The default lookup tries common naming conventions
// (the GameObject name, "<Speaker>", "_<Speaker>", an NpcSpeakerTag
// component). A scene that names its NPC GameObject "Doris" /
// "Gerrold" / "Pickle" / "Marin" works out of the box.
//
// If the speaker isn't found (e.g. "Narrator" or unknown), we keep
// the camera on the player but pull back to a wider establishing shot
// so the player and surrounding scene compose nicely while a
// narration line plays.
//
// ── Anti-clip ──────────────────────────────────────────────────
// The director sphere-casts from the camera's anchor toward the desired
// position and skips any hit on the Player's layer + any geometry. This
// prevents the "camera-inside-player" closeup that was the symptom of
// the bug.

using System.Collections.Generic;
using UnityEngine;
using HearthboundHollow.Core;

namespace HearthboundHollow.Player
{
    [DefaultExecutionOrder(50)] // After SmoothFollowCamera's LateUpdate
    public class DialogueCameraDirector : MonoBehaviour
    {
        public static DialogueCameraDirector Instance { get; private set; }

        // ───── Tunables ────────────────────────────────────────────

        [Header("Targets")]
        [Tooltip("The Camera transform we drive while dialogue is active. " +
                 "Auto-found as Camera.main if null.")]
        public Transform cameraTransform;

        [Tooltip("The SmoothFollowCamera we temporarily yield to during " +
                 "dialogue. Auto-found via Camera.main if null.")]
        public SmoothFollowCamera follow;

        [Tooltip("The player root used as the 'over-shoulder' anchor. " +
                 "Auto-found via GameObject.FindGameObjectWithTag(\"Player\").")]
        public Transform player;

        [Header("Speaker name → scene GameObject overrides")]
        [Tooltip("Optional explicit map from speaker name (case-insensitive) " +
                 "to a Transform. Falls back to a heuristic GameObject lookup " +
                 "when no entry matches.")]
        public List<SpeakerOverride> speakerOverrides = new();

        [System.Serializable]
        public struct SpeakerOverride
        {
            public string speakerName;   // e.g. "Doris"
            public Transform target;
        }

        [Header("Shot framing — over-the-shoulder default")]
        [Tooltip("Lateral offset (m) of the camera from the player→speaker line, " +
                 "positive = camera goes to the player's right shoulder.")]
        [Range(-3f, 3f)] public float shoulderOffset = 0.95f;

        [Tooltip("Camera height (m) above the player's pivot during dialogue.")]
        [Range(0.5f, 3f)] public float cameraHeight = 1.55f;

        [Tooltip("Distance (m) behind the player along the speaker-back vector.")]
        [Range(0.5f, 4f)] public float behindDistance = 1.6f;

        [Tooltip("Look-at height (m) above the speaker's pivot — typically chest/head.")]
        [Range(0.5f, 2.5f)] public float speakerLookHeight = 1.55f;

        [Tooltip("Minimum distance between camera and player collider (m). The " +
                 "director nudges the camera farther back if needed.")]
        [Range(0.4f, 2f)] public float minPlayerClearance = 0.9f;

        [Header("Wide / narrator fallback (no speaker found)")]
        [Tooltip("When the speaker can't be located in the scene (e.g. Pickle, " +
                 "Narrator) we pull back to a wider establishing shot. This is " +
                 "the camera distance for that shot.")]
        [Range(2f, 8f)] public float wideDistance = 4.5f;

        [Tooltip("Pitch (deg) used for the wide / narrator shot.")]
        [Range(5f, 45f)] public float widePitch = 18f;

        [Header("Smoothing")]
        [Tooltip("Spring-damp time for camera position during dialogue. " +
                 "Lower = snappier, higher = floatier.")]
        [Range(0.05f, 0.6f)] public float positionSmoothTime = 0.22f;

        [Tooltip("Slerp rate for camera rotation during dialogue.")]
        [Range(2f, 30f)] public float rotationLerp = 10f;

        [Tooltip("Seconds after the LAST line ends with no new line before we " +
                 "release the camera back to gameplay.")]
        [Range(0.1f, 2f)] public float postDialogueHold = 0.55f;

        [Header("Anti-clip")]
        [Tooltip("Sphere-cast radius used to push the camera off walls and the " +
                 "player's collider during dialogue.")]
        [Range(0.1f, 0.8f)] public float clipRadius = 0.32f;

        [Tooltip("Geometry layers used for camera clip. Defaults to everything " +
                 "EXCEPT TransparentFX/IgnoreRaycast/UI.")]
        public LayerMask clipMask = ~((1 << 1) | (1 << 2) | (1 << 5));

        // ───── State ───────────────────────────────────────────────

        private bool _dialogueActive;
        private float _exitTimer;
        private Transform _currentSpeaker;
        private string _currentSpeakerName;
        private Vector3 _posVel;
        private Vector3 _smoothedCamPos;

        // ───── Lifecycle ───────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
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

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

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

        // ───── Event handlers ──────────────────────────────────────

        private void OnLineStarted(DialogueLineStartedEvent e)
        {
            ResolveRefs();
            _currentSpeakerName = e.Speaker ?? string.Empty;
            _currentSpeaker = ResolveSpeakerTransform(_currentSpeakerName);
            EnterDialogueMode();
        }

        private void OnLineEnded(DialogueLineEndedEvent _)
        {
            // Don't release immediately — the director / Yarn runner usually
            // queues the next line within a frame. Start an exit countdown
            // that resets if a new line starts in the meantime.
            _exitTimer = postDialogueHold;
        }

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
            _exitTimer = 0f; // refresh — a new line just started
        }

        private void ExitDialogueMode()
        {
            if (!_dialogueActive) return;
            _dialogueActive = false;
            _currentSpeaker = null;
            _currentSpeakerName = string.Empty;
            if (follow != null)
            {
                follow.enabled = true;
                // Snap follow's smoothed pivot to the live target position so
                // it doesn't whip back through walls.
                follow.SnapToTargetImmediate();
            }
        }

        // ───── Camera drive ────────────────────────────────────────

        private void LateUpdate()
        {
            if (!_dialogueActive) return;
            if (cameraTransform == null) { ExitDialogueMode(); return; }

            // Handle the exit countdown.
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
            // Wide / narrator fallback when we can't find the speaker.
            if (_currentSpeaker == null)
            {
                ComposeWideShot(out pos, out rot);
                return;
            }
            ComposeOverShoulderShot(out pos, out rot);
        }

        private void ComposeOverShoulderShot(out Vector3 pos, out Quaternion rot)
        {
            Vector3 playerPivot = (player != null ? player.position : Vector3.zero) + Vector3.up * cameraHeight;
            Vector3 speakerPivot = _currentSpeaker.position + Vector3.up * speakerLookHeight;

            // The unit vector from speaker → player. Camera sits BEHIND player
            // on this vector, plus a shoulder offset to the right.
            Vector3 fromSpeakerToPlayer = playerPivot - speakerPivot;
            fromSpeakerToPlayer.y = 0f;
            float horizDist = fromSpeakerToPlayer.magnitude;
            if (horizDist < 0.01f) fromSpeakerToPlayer = -_currentSpeaker.forward;  // fallback: stand behind speaker
            Vector3 backDir = fromSpeakerToPlayer.normalized;
            Vector3 rightDir = Vector3.Cross(Vector3.up, backDir).normalized;

            pos = playerPivot + backDir * behindDistance + rightDir * shoulderOffset;

            // Clip against geometry between the LOOK target (speaker) and the
            // desired camera position. This is the "no camera inside the
            // player body" fix — speaker → camera-pos is the natural
            // unobstructed line; if it hits something on the way back, slide
            // the camera forward to the hit point minus a margin.
            Vector3 dir = pos - speakerPivot;
            float dist = dir.magnitude;
            if (dist > 0.01f)
            {
                if (Physics.SphereCast(new Ray(speakerPivot, dir.normalized), clipRadius,
                                       out RaycastHit hit, dist, clipMask, QueryTriggerInteraction.Ignore))
                {
                    pos = hit.point - dir.normalized * clipRadius;
                }
            }

            // Player-clearance safety: if the camera ended up too close to the
            // player's pivot (clip pushed it forward into the player), nudge
            // it perpendicular to the player→speaker axis so we don't end
            // up inside the player.
            Vector3 toPlayer = pos - playerPivot; toPlayer.y = 0f;
            if (toPlayer.magnitude < minPlayerClearance)
            {
                pos = playerPivot + rightDir * Mathf.Max(minPlayerClearance, shoulderOffset)
                                  + Vector3.up * 0.05f;
            }

            rot = Quaternion.LookRotation(speakerPivot - pos, Vector3.up);
        }

        private void ComposeWideShot(out Vector3 pos, out Quaternion rot)
        {
            Vector3 playerPivot = (player != null ? player.position : Vector3.zero) + Vector3.up * cameraHeight;
            // Use the player's current forward, fall back to camera's forward.
            Vector3 fwd = player != null ? player.forward : (cameraTransform != null ? cameraTransform.forward : Vector3.forward);
            // Pitch around the player's RIGHT axis so the camera tilts down
            // toward the player from above. Negative widePitch tilts the
            // "behind player" vector slightly downward (camera looks down).
            Vector3 right = player != null ? player.right : Vector3.right;
            Quaternion pitch = Quaternion.AngleAxis(-widePitch, right);
            Vector3 back = pitch * (-fwd);
            pos = playerPivot + back * wideDistance;

            // Clip protection: sphere-cast from player pivot toward camera.
            Vector3 dir = pos - playerPivot;
            float dist = dir.magnitude;
            if (dist > 0.01f)
            {
                if (Physics.SphereCast(new Ray(playerPivot, dir.normalized), clipRadius,
                                       out RaycastHit hit, dist, clipMask, QueryTriggerInteraction.Ignore))
                {
                    pos = hit.point - dir.normalized * clipRadius;
                }
            }

            rot = Quaternion.LookRotation(playerPivot - pos, Vector3.up);
        }

        // ───── Speaker discovery ──────────────────────────────────

        private Transform ResolveSpeakerTransform(string speakerName)
        {
            if (string.IsNullOrEmpty(speakerName)) return null;

            // 1. Explicit inspector override wins.
            foreach (var o in speakerOverrides)
            {
                if (o.target != null && o.speakerName != null &&
                    string.Equals(o.speakerName, speakerName, System.StringComparison.OrdinalIgnoreCase))
                    return o.target;
            }

            // 2. Heuristic GameObject lookup — try the speaker name, then
            //    common decorations ("_Speaker", "Speaker_"), then any
            //    object whose name STARTS WITH the speaker name (catches
            //    "Doris (1)", "Doris_M1", etc.).
            var direct = GameObject.Find(speakerName);
            if (direct != null) return direct.transform;
            direct = GameObject.Find("_" + speakerName);
            if (direct != null) return direct.transform;

            // Fallback scene walk (allocates — only on the rare miss path).
            var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var r in roots)
            {
                if (r.name.StartsWith(speakerName, System.StringComparison.OrdinalIgnoreCase))
                    return r.transform;
                var nested = FindChildStartingWith(r.transform, speakerName);
                if (nested != null) return nested;
            }
            return null; // null = wide/narrator shot
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
