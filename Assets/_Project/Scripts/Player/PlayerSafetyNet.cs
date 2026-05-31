// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Player / PlayerSafetyNet
//
// PHASE 32.15 — "Player fell into the void / got stuck" safety net.
//
// User report (4 screenshots):
//   * In 03_Mission01_Hollow, walking to the right edge of the floor pushed
//     the player off — game view turned pure black with only the HUD visible
//     (camera following a player at Y = -infinity).
//   * In another spot the player got STUCK against unmarked geometry and
//     couldn't move at all despite WASD input.
//
// Both are PC-game basics that every shipping title handles. This component
// + companion provide three independent safety layers:
//
//   1. **Death-plane respawn** — every frame we check the player's Y. If
//      they sink below `fallY` (default -3 m, below any sensible playable
//      floor), we teleport them back to the spawn position with a soft
//      fade-tap on the screen. Used by Mario, Witcher, BotW, every
//      modern open-world game.
//
//   2. **Auto-spawned scene boundary** — on Awake / sceneLoaded we scan
//      the active scene for the largest visible floor / ground mesh and
//      build 4 invisible 5 m-tall BoxCollider walls around its perimeter
//      (with a small inset so the player can't stand exactly on the edge
//      and squeeze out via floating-point bumps). Plays nicely with
//      Phase 47's HardCoded room boundary — if Phase 47 ran, we detect
//      its walls and don't double up.
//
//   3. **Stuck-detector** — if the player has positive horizontal input
//      but their position has barely moved for > stuckSeconds (default
//      1.5 s), we nudge them ~0.4 m straight up (so the CharacterController
//      can resolve any wedged-into-mesh state) and log a one-time warning.
//      Prevents the "rooted in place" symptom from a degenerate collision.
//
// Auto-spawn — `PlayerController.Awake` adds this if missing, so a
// pulled-fresh project gets safety on frame 1.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using HearthboundHollow.Core;

namespace HearthboundHollow.Player
{
    [RequireComponent(typeof(PlayerController))]
    [DefaultExecutionOrder(80)]
    public class PlayerSafetyNet : MonoBehaviour
    {
        [Header("Death-plane respawn")]
        [Tooltip("If the player's Y falls below this in world space, the " +
                 "safety net respawns them. Default well below any sensible " +
                 "playable floor.")]
        public float fallY = -3f;

        [Tooltip("World position to respawn to. If left at zero, the safety " +
                 "net captures the player's starting position on first Awake " +
                 "and uses that.")]
        public Vector3 respawnPosition;

        [Tooltip("If true, on Awake we treat the player's initial transform " +
                 "as the canonical spawn point even when respawnPosition is " +
                 "set in the inspector. Useful for per-scene spawns.")]
        public bool captureSpawnFromInitialPosition = true;

        [Header("Stuck detector")]
        [Tooltip("If the player has movement input but moves less than " +
                 "stuckMinMove metres across stuckSeconds, give them a small " +
                 "upward nudge and log a warning.")]
        [Range(0.2f, 5f)] public float stuckSeconds = 1.5f;
        [Range(0.01f, 0.5f)] public float stuckMinMove = 0.08f;
        [Range(0.05f, 1.0f)] public float stuckNudgeUp = 0.4f;

        [Header("Auto-build boundary")]
        [Tooltip("Auto-build invisible BoxCollider walls around the largest " +
                 "ground mesh in the current scene on sceneLoaded. Idempotent.")]
        public bool autoBuildBoundary = true;

        [Tooltip("Inset (m) the boundary walls from the floor edge so the " +
                 "player can't tunnel through via float-precision bumps.")]
        [Range(0f, 1f)] public float boundaryInset = 0.4f;

        [Tooltip("Height of the invisible walls.")]
        [Range(1f, 12f)] public float boundaryHeight = 5f;

        // ───── State ───────────────────────────────────────────────

        private PlayerController _pc;
        private CharacterController _cc;
        private Vector3 _lastPos;
        private float _stuckTimer;
        private float _stuckAccum;   // metres actually covered in the current window
        private bool _stuckWarned;
        private bool _spawnCaptured;
        private int _respawnCount;
        private const string BoundaryRootName = "_PlayerSafetyNet_Boundary";

        // ───── Lifecycle ───────────────────────────────────────────

        private void Awake()
        {
            _pc = GetComponent<PlayerController>();
            _cc = GetComponent<CharacterController>();
            if (captureSpawnFromInitialPosition || respawnPosition == Vector3.zero)
            {
                respawnPosition = transform.position;
                _spawnCaptured = true;
            }
            _lastPos = transform.position;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            // Build boundary for the current scene immediately (in case we
            // were already enabled before sceneLoaded fires).
            if (autoBuildBoundary) BuildBoundaryForActiveScene();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene s, LoadSceneMode m)
        {
            // Re-capture spawn for the new scene (the player may be moved by
            // the scene's spawn helper before OnSceneLoaded fires).
            respawnPosition = transform.position;
            _spawnCaptured = true;
            if (autoBuildBoundary) BuildBoundaryForActiveScene();
        }

        // ───── Per-frame safety checks ────────────────────────────

        private void LateUpdate()
        {
            // 1. Death-plane respawn.
            if (transform.position.y < fallY)
            {
                _respawnCount++;
                Hh.Warn(LogCategory.Input,
                    $"PlayerSafetyNet: player fell to y={transform.position.y:F2} (< {fallY}); " +
                    $"respawning to {respawnPosition} (respawn #{_respawnCount}).");
                Respawn();
                return;
            }

            // 2. Stuck-detector. Only meaningful if there's a CharacterController
            //    to nudge and movement input is being supplied. We assume the
            //    PlayerController publishes a "wants to move" bool we can read;
            //    if not, fall back to "horizontal cursor delta > 0".
            // Accumulate ACTUAL movement over a rolling window while the player
            // is asking to move. If they cover less than `stuckMinMove` metres
            // across `stuckSeconds`, they're genuinely wedged → nudge up so the
            // CharacterController can resolve it.
            //
            // Phase 32.21 fix: the old test compared the PER-FRAME delta against a
            // window-scaled threshold (`stuckMinMove * dt / 0.016` ≈ 0.083 m ≈
            // 5 m/s). A normally walking player (slower than that) was therefore
            // flagged "stuck" every frame and got nudged +0.4 m every 1.5 s —
            // the periodic hop + "wedged… nudging up" log spam seen in M1/M2.
            float delta = (transform.position - _lastPos).magnitude;
            bool wantsMove =
                Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f ||
                Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f;
            if (wantsMove)
            {
                _stuckTimer += Time.deltaTime;
                _stuckAccum += delta;
                if (_stuckTimer >= stuckSeconds)
                {
                    if (_stuckAccum < stuckMinMove)
                    {
                        if (_cc != null)
                        {
                            _cc.enabled = false;
                            transform.position += Vector3.up * stuckNudgeUp;
                            _cc.enabled = true;
                        }
                        if (!_stuckWarned)
                        {
                            Hh.Warn(LogCategory.Input,
                                $"PlayerSafetyNet: player appears wedged at {transform.position}; " +
                                $"nudging up by {stuckNudgeUp} m.");
                            _stuckWarned = true;
                        }
                    }
                    else
                    {
                        _stuckWarned = false; // covered ground fine this window
                    }
                    _stuckTimer = 0f;
                    _stuckAccum = 0f;
                }
            }
            else
            {
                _stuckTimer = 0f;
                _stuckAccum = 0f;
                _stuckWarned = false;
            }
            _lastPos = transform.position;
        }

        public void Respawn()
        {
            if (_cc != null) _cc.enabled = false;
            transform.position = respawnPosition;
            if (_cc != null) _cc.enabled = true;
            _stuckTimer = 0f;
            _lastPos = respawnPosition;
        }

        // ───── Auto-boundary builder ──────────────────────────────

        private void BuildBoundaryForActiveScene()
        {
            // Idempotent: bail if a boundary root from a previous run exists.
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid()) return;
            foreach (var go in scene.GetRootGameObjects())
            {
                if (go.name == BoundaryRootName) return;
            }

            // Find the largest floor / ground mesh in the scene.
            Bounds? floor = FindLargestFloorBounds(scene);
            if (!floor.HasValue)
            {
                Hh.Log(LogCategory.Input,
                    "PlayerSafetyNet: no floor mesh found in scene; skipping auto-boundary.");
                return;
            }

            var b = floor.Value;
            // Inset the walls so the player can't stand on the precise edge.
            float halfX = b.extents.x - boundaryInset;
            float halfZ = b.extents.z - boundaryInset;
            float yCentre = b.center.y + boundaryHeight * 0.5f;

            if (halfX <= 0.5f || halfZ <= 0.5f)
            {
                Hh.Log(LogCategory.Input,
                    $"PlayerSafetyNet: floor too small ({halfX*2:F1}×{halfZ*2:F1}) for auto-boundary.");
                return;
            }

            var root = new GameObject(BoundaryRootName);
            // Don't parent — keeps it cleanly at the scene root and survives
            // Player teleports.
            CreateWall(root.transform, "Bound_N",
                new Vector3(b.center.x, yCentre, b.center.z + halfZ),
                new Vector3(halfX * 2f + 1f, boundaryHeight, 0.5f));
            CreateWall(root.transform, "Bound_S",
                new Vector3(b.center.x, yCentre, b.center.z - halfZ),
                new Vector3(halfX * 2f + 1f, boundaryHeight, 0.5f));
            CreateWall(root.transform, "Bound_E",
                new Vector3(b.center.x + halfX, yCentre, b.center.z),
                new Vector3(0.5f, boundaryHeight, halfZ * 2f + 1f));
            CreateWall(root.transform, "Bound_W",
                new Vector3(b.center.x - halfX, yCentre, b.center.z),
                new Vector3(0.5f, boundaryHeight, halfZ * 2f + 1f));

            Hh.Log(LogCategory.Input,
                $"PlayerSafetyNet: built auto-boundary around floor at {b.center} " +
                $"(size {halfX*2:F1}×{halfZ*2:F1}, height {boundaryHeight}).");
        }

        private static void CreateWall(Transform parent, string n, Vector3 pos, Vector3 size)
        {
            var go = new GameObject(n);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var bc = go.AddComponent<BoxCollider>();
            bc.size = size;
            // Invisible by design — no renderer, no material.
        }

        private static Bounds? FindLargestFloorBounds(Scene scene)
        {
            Bounds? best = null;
            float bestArea = 0f;
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var r in root.GetComponentsInChildren<Renderer>(includeInactive: false))
                {
                    if (r == null) continue;
                    string n = r.gameObject.name.ToLowerInvariant();
                    // Heuristic: prefer objects named like floors/grounds, OR
                    // objects whose bounds are wide-and-flat (XZ much bigger
                    // than Y) and reasonably big (> 4 m² footprint).
                    var b = r.bounds;
                    float area = b.size.x * b.size.z;
                    bool floorLike = n.Contains("floor") || n.Contains("ground") ||
                                     n.Contains("terrain") || n.Contains("street") ||
                                     n.Contains("lane") || n.Contains("road") ||
                                     (b.size.y < 1.5f && area > 4f);
                    if (!floorLike) continue;
                    if (area > bestArea)
                    {
                        bestArea = area;
                        best = b;
                    }
                }
            }
            return best;
        }
    }
}
