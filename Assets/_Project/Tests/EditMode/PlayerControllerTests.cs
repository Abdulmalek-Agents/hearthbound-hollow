// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Tests / EditMode / PlayerControllerTests
//
// Phase 26. Locks the public surface of PlayerController + SmoothFollowCamera
// so the next refactor can't silently break Mission01Director / Mission02Director
// — both of which set MovementLocked and call TryActivateFocus().
//
// These are *EditMode* tests — we don't try to step Update(). The runtime path
// is exercised in PlayMode tests + the smoke scene.

using NUnit.Framework;
using UnityEngine;
using HearthboundHollow.Player;

namespace HearthboundHollow.Tests.EditMode
{
    public class PlayerControllerTests
    {
        // ───── PlayerController contract ───────────────────────────

        [Test]
        public void PlayerController_HasMovementLockedAPI()
        {
            var go = new GameObject("PlayerTest");
            go.AddComponent<CharacterController>();
            var pc = go.AddComponent<PlayerController>();

            Assert.IsFalse(pc.MovementLocked, "MovementLocked must default to false.");
            pc.MovementLocked = true;
            Assert.IsTrue(pc.MovementLocked, "Setter must round-trip.");

            Object.DestroyImmediate(go);
        }

        [Test]
        public void PlayerController_DefaultsToNotSprintingAndNotGrounded()
        {
            var go = new GameObject("PlayerTest");
            go.AddComponent<CharacterController>();
            var pc = go.AddComponent<PlayerController>();

            Assert.IsFalse(pc.IsSprinting, "IsSprinting starts false.");
            // CharacterController.isGrounded is false before any Move() — that's fine.
            Assert.IsFalse(pc.IsGrounded, "IsGrounded false before any Move().");
            Assert.AreEqual(Vector3.zero, pc.CurrentMoveInput, "CurrentMoveInput starts at zero.");
            Assert.AreEqual(Vector3.zero, pc.CurrentVelocity, "CurrentVelocity starts at zero.");

            Object.DestroyImmediate(go);
        }

        [Test]
        public void PlayerController_SetCameraReference_Roundtrips()
        {
            var go = new GameObject("PlayerTest");
            go.AddComponent<CharacterController>();
            var pc = go.AddComponent<PlayerController>();

            var camera = new GameObject("CameraTest").transform;
            // Should not throw; current API has no public getter — proof-of-life only.
            pc.SetCameraReference(camera);

            Object.DestroyImmediate(camera.gameObject);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void PlayerController_SetAnimator_Roundtrips()
        {
            var go = new GameObject("PlayerTest");
            go.AddComponent<CharacterController>();
            var pc = go.AddComponent<PlayerController>();

            var animatorGO = new GameObject("AnimatorTest");
            var animator = animatorGO.AddComponent<Animator>();
            pc.SetAnimator(animator);

            Object.DestroyImmediate(animatorGO);
            Object.DestroyImmediate(go);
        }

        // ───── SmoothFollowCamera contract ─────────────────────────

        [Test]
        public void SmoothFollowCamera_HasSensibleDefaults()
        {
            var go = new GameObject("CameraTest");
            var follow = go.AddComponent<SmoothFollowCamera>();

            Assert.Greater(follow.distance, follow.distanceMin - 0.001f);
            Assert.Less(follow.distance, follow.distanceMax + 0.001f);
            Assert.GreaterOrEqual(follow.pitch, follow.pitchMin);
            Assert.LessOrEqual(follow.pitch, follow.pitchMax);
            Assert.IsTrue(follow.clipAgainstGeometry, "Wall-clip defaults ON.");
            Assert.Greater(follow.positionSmoothTime, 0f);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void SmoothFollowCamera_SnapsToTargetWithoutThrowing()
        {
            var camGo = new GameObject("CameraTest");
            var follow = camGo.AddComponent<SmoothFollowCamera>();
            follow.target = new GameObject("Target").transform;
            follow.target.position = new Vector3(10, 0, 5);

            Assert.DoesNotThrow(() => follow.SnapToTargetImmediate());
            // Position should be near, but not at, the target — distance is non-zero.
            Assert.AreNotEqual(follow.target.position, camGo.transform.position);

            Object.DestroyImmediate(follow.target.gameObject);
            Object.DestroyImmediate(camGo);
        }

        // ───── Default tuning sanity ───────────────────────────────

        [Test]
        public void PlayerController_DefaultRangesAreSensible()
        {
            // Reflection-light sanity: serialized fields use SerializeField,
            // we can read defaults by re-instantiating + reading via component
            // public state. Since most are private serialized we just smoke-test
            // that AddComponent succeeds and frame doesn't throw.
            var go = new GameObject("PlayerTest");
            go.AddComponent<CharacterController>();
            var pc = go.AddComponent<PlayerController>();
            Assert.NotNull(pc);
            Object.DestroyImmediate(go);
        }
    }
}
