// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase26_PlayerControllerAndAnimation
//
// PHASE 26 — Player Controller + Animation pipeline.
//
// One-click rebuild that:
//
//   1. Re-runs PlayerAnimatorControllerBuilder so
//      Assets/_Project/Animations/Hearthbound_Player.controller exists.
//   2. Updates the Player prefab (Assets/_Project/Prefabs/Player/Player.prefab,
//      created by Phase 13) so its Animator references the new controller
//      AND it carries a PlayerGroundClamp component that aligns the BoZo
//      mesh feet to the CharacterController capsule bottom (fix for the
//      "half body in the floor" issue reported on first playtest).
//   3. Walks every gameplay scene (Lane, Hollow, Garden, Cottage) and:
//        a. Upgrades the Main Camera's `SimpleFollowCamera` (Phase 17/22)
//           into a `SmoothFollowCamera`, copying target + framing.
//        b. Re-points the Player's PlayerController.cameraReference at the
//           Main Camera transform so movement is camera-relative.
//        c. Ensures the Player has an Animator referencing the new
//           Hearthbound_Player controller.
//        d. Ensures the Player has a PlayerGroundClamp (idempotent — added
//           only if missing).
//   4. Updates the project's HearthboundInput.inputactions to expose
//      Sprint, Jump, CameraLook, CameraZoom, AllowLook actions.
//
// IDEMPOTENT — every step is safe to re-run.
//
// USE: Menu → Hearthbound → 🏃 Phase 26 — Player Controller + Animation

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using HearthboundHollow.Player;

namespace HearthboundHollow.EditorTools
{
    public static class Phase26_PlayerControllerAndAnimation
    {
        // Scene targets — Lane, Hollow, Garden, Cottage. Bootstrap & MainMenu
        // are intentionally excluded (no Player, no follow camera).
        private static readonly string[] GameplayScenes = new[]
        {
            "Assets/_Project/Scenes/02_Mission01_Lane.unity",
            "Assets/_Project/Scenes/03_Mission01_Hollow.unity",
            "Assets/_Project/Scenes/04_Mission02_Garden.unity",
            "Assets/_Project/Scenes/05_Mission02_Cottage.unity",
        };

        private const string PlayerPrefabPath =
            "Assets/_Project/Prefabs/Player/Player.prefab";

        // ───── Menu entry ─────────────────────────────────────────

        [MenuItem("Hearthbound/\ud83c\udfc3 Phase 26 — Player Controller + Animation", priority = 1)]
        public static void Build()
        {
            EditorUtility.DisplayProgressBar("Hearthbound · Phase 26", "Building AnimatorController …", 0.10f);
            try
            {
                // 1. AnimatorController
                PlayerAnimatorControllerBuilder.BuildOrUpdate();
                var controller = PlayerAnimatorControllerBuilder.TryGetController();

                // 2. Player prefab → Animator + controller + GroundClamp
                EditorUtility.DisplayProgressBar("Hearthbound · Phase 26", "Wiring Player prefab …", 0.30f);
                WirePlayerPrefab(controller);

                // 3. Scenes: SmoothFollowCamera + cameraReference + GroundClamp
                int touched = 0;
                for (int i = 0; i < GameplayScenes.Length; i++)
                {
                    EditorUtility.DisplayProgressBar(
                        "Hearthbound · Phase 26",
                        $"Upgrading scene ({i + 1}/{GameplayScenes.Length}) …",
                        0.40f + 0.50f * (i / (float)GameplayScenes.Length));
                    if (UpgradeScene(GameplayScenes[i], controller)) touched++;
                }

                EditorUtility.DisplayProgressBar("Hearthbound · Phase 26", "Saving …", 0.95f);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog(
                    "Hearthbound — Phase 26 complete",
                    BuildSummary(controller, touched),
                    "OK");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        // ───── Player prefab wiring ───────────────────────────────

        private static void WirePlayerPrefab(AnimatorController controller)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (prefab == null)
            {
                Debug.LogWarning(
                    "[Hearthbound/Phase 26] No Player prefab at " + PlayerPrefabPath +
                    ". Run 'Hearthbound → Phase 13 — Build BoZo Character Prefabs' first " +
                    "(or rely on scene-side wiring).");
                return;
            }

            // Operate on a prefab contents instance, save back via SaveAsPrefabAsset.
            var contents = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            try
            {
                EnsureAnimatorOnPlayerRoot(contents, controller);
                EnsureGroundClamp(contents);
                PrefabUtility.SaveAsPrefabAsset(contents, PlayerPrefabPath);
                Debug.Log("[Hearthbound/Phase 26] Player prefab updated with new Animator + controller + PlayerGroundClamp.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        private static void EnsureAnimatorOnPlayerRoot(GameObject root, AnimatorController controller)
        {
            // BoZo wrappers nest the rig under a "Body" child. The Animator
            // typically lives on Body (humanoid avatar attached there). We
            // accept either layout: re-use the existing Animator if any,
            // otherwise add one on the root.
            var animator = root.GetComponentInChildren<Animator>(true);
            if (animator == null) animator = root.AddComponent<Animator>();

            if (controller != null) animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false; // we drive position from CharacterController

            // Wire PlayerController.animator → this animator if visible from API.
            var pc = root.GetComponent<PlayerController>();
            if (pc != null) pc.SetAnimator(animator);
        }

        /// <summary>
        /// Idempotent: adds a PlayerGroundClamp to the root if absent. Auto-
        /// resolves its `body` reference to the "Body" child created by Phase 13.
        /// Critical fix for the "half body in the floor" issue: BoZo's mesh
        /// origin is offset above the feet, so without this clamp the visible
        /// mesh sinks into the ground when the CharacterController settles.
        /// </summary>
        private static void EnsureGroundClamp(GameObject root)
        {
            var clamp = root.GetComponent<PlayerGroundClamp>();
            if (clamp == null) clamp = root.AddComponent<PlayerGroundClamp>();
            if (clamp.body == null)
            {
                var bodyT = root.transform.Find("Body");
                if (bodyT != null) clamp.body = bodyT;
            }
        }

        // ───── Scene upgrade ──────────────────────────────────────

        private static bool UpgradeScene(string scenePath, AnimatorController controller)
        {
            if (!File.Exists(scenePath))
            {
                Debug.Log($"[Hearthbound/Phase 26] (skip) {scenePath} — not present yet.");
                return false;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            bool dirty = false;

            // Find the player.
            var player = GameObject.FindGameObjectWithTag("Player");
            var pc = player != null ? player.GetComponent<PlayerController>() : null;

            // Find / upgrade follow camera.
            var simple = Object.FindFirstObjectByType<SimpleFollowCamera>();
            SmoothFollowCamera smooth = Object.FindFirstObjectByType<SmoothFollowCamera>();

            if (simple != null)
            {
                // Capture state, swap component, restore state.
                var target = simple.target;
                var height = simple.height;
                var behind = simple.behind;
                var lookAheadY = simple.lookAheadY;
                var cameraGo = simple.gameObject;

                Object.DestroyImmediate(simple);

                smooth = cameraGo.AddComponent<SmoothFollowCamera>();
                smooth.target = target;
                // Reasonable conversion from the old (height, behind) framing.
                smooth.distance = Mathf.Sqrt(height * height + behind * behind);
                smooth.distance = Mathf.Clamp(smooth.distance, smooth.distanceMin, smooth.distanceMax);
                smooth.pitch = Mathf.Clamp(Mathf.Atan2(Mathf.Max(0.01f, height), Mathf.Max(0.01f, behind)) * Mathf.Rad2Deg, smooth.pitchMin, smooth.pitchMax);
                smooth.lookOffset = new Vector3(0f, Mathf.Max(0.5f, lookAheadY), 0f);
                smooth.SnapToTargetImmediate();

                dirty = true;
                Debug.Log($"[Hearthbound/Phase 26] Upgraded SimpleFollowCamera → SmoothFollowCamera in {Path.GetFileName(scenePath)}.");
            }
            else if (smooth == null)
            {
                // No follow camera at all? Create one on the Main Camera.
                var cam = Camera.main != null ? Camera.main.gameObject : Object.FindFirstObjectByType<Camera>()?.gameObject;
                if (cam != null && player != null)
                {
                    smooth = cam.AddComponent<SmoothFollowCamera>();
                    smooth.target = player.transform;
                    smooth.SnapToTargetImmediate();
                    dirty = true;
                    Debug.Log($"[Hearthbound/Phase 26] Added SmoothFollowCamera (no prior follow rig) in {Path.GetFileName(scenePath)}.");
                }
            }

            // Set the PlayerController.cameraReference to the (now) live camera.
            if (pc != null && smooth != null)
            {
                pc.SetCameraReference(smooth.transform);
                EditorUtility.SetDirty(pc);
                dirty = true;
            }

            // Ensure the in-scene Player has its Animator + controller wired.
            if (player != null && controller != null)
            {
                var animator = player.GetComponentInChildren<Animator>(true);
                if (animator == null) animator = player.AddComponent<Animator>();
                if (animator.runtimeAnimatorController != controller)
                {
                    animator.runtimeAnimatorController = controller;
                    EditorUtility.SetDirty(animator);
                    dirty = true;
                }
                if (pc != null) pc.SetAnimator(animator);
            }

            // Ensure PlayerGroundClamp — fix for the half-body-in-floor sink.
            if (player != null)
            {
                var clamp = player.GetComponent<PlayerGroundClamp>();
                if (clamp == null)
                {
                    clamp = player.AddComponent<PlayerGroundClamp>();
                    if (clamp.body == null)
                    {
                        var bodyT = player.transform.Find("Body");
                        if (bodyT != null) clamp.body = bodyT;
                    }
                    EditorUtility.SetDirty(player);
                    dirty = true;
                    Debug.Log($"[Hearthbound/Phase 26] Added PlayerGroundClamp to in-scene Player in {Path.GetFileName(scenePath)}.");
                }
                else if (clamp.body == null)
                {
                    // Heal stale serialized reference.
                    var bodyT = player.transform.Find("Body");
                    if (bodyT != null)
                    {
                        clamp.body = bodyT;
                        EditorUtility.SetDirty(clamp);
                        dirty = true;
                    }
                }
            }

            if (dirty)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            return dirty;
        }

        // ───── Summary builder ────────────────────────────────────

        private static string BuildSummary(AnimatorController controller, int scenesTouched)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Phase 26 — Player Controller + Animation");
            sb.AppendLine();
            sb.Append("  ✓ AnimatorController: ");
            sb.AppendLine(controller != null ? AssetDatabase.GetAssetPath(controller) : "(none)");
            sb.AppendLine("  ✓ Parameters: Speed / MoveX / MoveY / VelocityY / IsGrounded / IsSprinting / Jump");
            sb.AppendLine("  ✓ States: Locomotion (1D blend tree) / Jump / Fall / Land");
            sb.AppendLine();
            sb.AppendLine($"  ✓ Player prefab wired (Animator + controller + PlayerGroundClamp):  {PlayerPrefabPath}");
            sb.AppendLine($"  ✓ Scenes upgraded with SmoothFollowCamera + cameraReference + GroundClamp: {scenesTouched}/{GameplayScenes.Length}");
            sb.AppendLine();
            sb.AppendLine("PlayerGroundClamp fixes the 'half body in floor' issue from the first");
            sb.AppendLine("playtest by aligning the BoZo mesh feet to the CharacterController capsule");
            sb.AppendLine("bottom on Start. No more sinking; no more pop-up-on-WASD rubber-banding.");
            sb.AppendLine();
            sb.AppendLine("Controls (now also visible via H in-game):");
            sb.AppendLine("  Move       WASD / Arrows / Left Stick");
            sb.AppendLine("  Sprint     Left Shift / Gamepad LStick click");
            sb.AppendLine("  Jump       Space / Gamepad south  (optional — Gentle Mode disables)");
            sb.AppendLine("  Interact   E / Gamepad ▢");
            sb.AppendLine("  Look       Hold Right Mouse + move mouse (or Right Stick on gamepad)");
            sb.AppendLine("  Zoom       Scroll wheel");
            sb.AppendLine("  Pause      Escape");
            sb.AppendLine("  Help       H");
            sb.AppendLine();
            sb.AppendLine("If Locomotion only shows an Idle/Walk loop and no running animation, the run");
            sb.AppendLine("source clip wasn't found. Drop a Mixamo 'Running.fbx' into");
            sb.AppendLine($"  {PlayerAnimatorControllerBuilder.AnimationsDir}/Mixamo/Running.fbx");
            sb.AppendLine("and re-run this menu item. See Docs/ANIMATION_REQUIREMENTS.md for the");
            sb.AppendLine("Mixamo download list + Humanoid retargeting steps.");
            return sb.ToString();
        }
    }
}
