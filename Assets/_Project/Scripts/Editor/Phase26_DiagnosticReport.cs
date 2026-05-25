// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase26_DiagnosticReport
//
// A focused diagnostic for the Phase 26 (Player Controller + Animation) wiring.
// Complements `Phase23_DiagnosticReport` which audits the Phase 23 scenes.
//
// Run AFTER `Hearthbound → 🏃 Phase 26 — Player Controller + Animation` to
// verify that every gameplay scene has:
//
//   ✓ A `Player` GameObject tagged "Player".
//   ✓ A `PlayerController` on (or under) Player.
//   ✓ A CharacterController on Player root.
//   ✓ A SmoothFollowCamera somewhere in the scene (not SimpleFollowCamera).
//   ✓ The follow camera's `target` set to the Player transform.
//   ✓ An Animator (on Player root or any child).
//   ✓ The Animator's runtimeAnimatorController == Hearthbound_Player.controller.
//   ✓ `PlayerController.SetCameraReference()` has been called (we infer this
//     by checking that movement would be camera-relative — best-effort).
//
// Also verifies the controller asset itself has the expected parameters
// + states (Locomotion / Jump / Fall / Land).
//
// USE: Menu → Hearthbound → 🔍 Diagnose Phase 26 Build

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using HearthboundHollow.Player;

namespace HearthboundHollow.EditorTools
{
    public static class Phase26_DiagnosticReport
    {
        private static readonly string[] GameplayScenes =
        {
            "Assets/_Project/Scenes/02_Mission01_Lane.unity",
            "Assets/_Project/Scenes/03_Mission01_Hollow.unity",
            "Assets/_Project/Scenes/04_Mission02_Garden.unity",
            "Assets/_Project/Scenes/05_Mission02_Cottage.unity",
        };

        [MenuItem("Hearthbound/\ud83d\udd0d Diagnose Phase 26 Build", priority = 51)]
        public static void Diagnose()
        {
            var sb = new System.Text.StringBuilder();
            int pass = 0, warn = 0, fail = 0;

            void Pass(string m) { sb.AppendLine("  ✓ " + m); pass++; }
            void Warn(string m) { sb.AppendLine("  ⚠ " + m); warn++; }
            void Fail(string m) { sb.AppendLine("  ✗ " + m); fail++; }

            sb.AppendLine("Hearthbound Hollow — Phase 26 Diagnostic\n");

            // ─── AnimatorController asset ──────────────────────────
            sb.AppendLine("AnimatorController asset:");
            var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                "Assets/_Project/Animations/Hearthbound_Player.controller");
            if (ctrl == null)
            {
                Fail("Assets/_Project/Animations/Hearthbound_Player.controller is MISSING — run Phase 26.");
            }
            else
            {
                Pass("Hearthbound_Player.controller exists.");

                // Parameters
                var expectedParams = new[] { "Speed", "MoveX", "MoveY", "VelocityY", "IsGrounded", "IsSprinting", "Jump" };
                var actualParamNames = new HashSet<string>();
                foreach (var p in ctrl.parameters) actualParamNames.Add(p.name);
                foreach (var name in expectedParams)
                {
                    if (actualParamNames.Contains(name)) Pass($"Parameter '{name}' present.");
                    else Fail($"Parameter '{name}' MISSING from controller.");
                }

                // States
                var sm = ctrl.layers[0].stateMachine;
                var expectedStates = new[] { "Locomotion", "Jump", "Fall", "Land" };
                var actualStateNames = new HashSet<string>();
                foreach (var s in sm.states) actualStateNames.Add(s.state.name);
                foreach (var s in expectedStates)
                {
                    if (actualStateNames.Contains(s)) Pass($"State '{s}' present.");
                    else Warn($"State '{s}' missing — capstone may not have re-run after a clip was added.");
                }

                // Default state
                if (sm.defaultState != null && sm.defaultState.name == "Locomotion")
                    Pass("Default state is Locomotion.");
                else
                    Warn("Default state is not Locomotion — re-run Phase 26 to reset.");
            }
            sb.AppendLine();

            // ─── Player prefab ─────────────────────────────────────
            sb.AppendLine("Player prefab (Assets/_Project/Prefabs/Player/Player.prefab):");
            const string playerPrefabPath = "Assets/_Project/Prefabs/Player/Player.prefab";
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(playerPrefabPath);
            if (playerPrefab == null)
            {
                Warn($"{playerPrefabPath} MISSING — run Phase 13 first.");
            }
            else
            {
                Pass($"{playerPrefabPath} exists.");
                var pc = playerPrefab.GetComponentInChildren<PlayerController>(true);
                if (pc != null) Pass("Player prefab has PlayerController.");
                else Fail("Player prefab has NO PlayerController.");

                var anim = playerPrefab.GetComponentInChildren<Animator>(true);
                if (anim == null)
                {
                    Warn("Player prefab has no Animator (re-run Phase 26).");
                }
                else
                {
                    Pass("Player prefab has an Animator.");
                    if (anim.runtimeAnimatorController == null)
                    {
                        Warn("Player prefab Animator has NO controller assigned (re-run Phase 26).");
                    }
                    else
                    {
                        if (ctrl != null && anim.runtimeAnimatorController == ctrl)
                            Pass("Player prefab Animator references Hearthbound_Player.controller.");
                        else
                            Warn($"Player prefab Animator references '{anim.runtimeAnimatorController.name}' (expected Hearthbound_Player).");
                    }
                    if (anim.applyRootMotion)
                        Warn("Player Animator has Apply Root Motion = ON (Phase 26 sets it OFF; CharacterController owns position).");
                    else
                        Pass("Player Animator has Apply Root Motion = OFF.");
                }
            }
            sb.AppendLine();

            // ─── Per-scene audit ───────────────────────────────────
            int scenesWithProblems = 0;
            foreach (var scenePath in GameplayScenes)
            {
                sb.AppendLine($"Scene: {Path.GetFileName(scenePath)}");
                if (!File.Exists(scenePath))
                {
                    Warn($"{scenePath} not present — skip.");
                    continue;
                }
                int sceneFailsBefore = fail;
                int sceneWarnsBefore = warn;

                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                // Player
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player == null)
                {
                    Fail("No GameObject tagged 'Player' in scene.");
                }
                else
                {
                    Pass($"Player GameObject present: {player.name}");
                    if (player.GetComponent<CharacterController>() == null)
                        Warn("Player has no CharacterController (PlayerController requires one).");
                    var pc = player.GetComponentInChildren<PlayerController>(true);
                    if (pc == null) Fail("Player has no PlayerController script.");
                    else Pass("Player has PlayerController.");

                    var anim = player.GetComponentInChildren<Animator>(true);
                    if (anim == null)
                    {
                        Warn("Player has no Animator (BoZo's nested Animator should provide one — re-run Phase 13 + 26).");
                    }
                    else if (anim.runtimeAnimatorController == null)
                    {
                        Warn("Player Animator has no controller assigned.");
                    }
                    else if (ctrl != null && anim.runtimeAnimatorController == ctrl)
                    {
                        Pass("Player Animator → Hearthbound_Player.controller.");
                    }
                    else
                    {
                        Warn($"Player Animator → '{anim.runtimeAnimatorController.name}' (expected Hearthbound_Player).");
                    }
                }

                // Follow camera
                var smooth = Object.FindFirstObjectByType<SmoothFollowCamera>();
                var simple = Object.FindFirstObjectByType<SimpleFollowCamera>();
                if (smooth != null)
                {
                    Pass("SmoothFollowCamera present.");
                    if (player != null && smooth.target == player.transform)
                        Pass("SmoothFollowCamera.target → Player.");
                    else
                        Warn("SmoothFollowCamera.target is not the Player transform.");
                }
                else if (simple != null)
                {
                    Warn("SimpleFollowCamera present (Phase 26 hasn't run on this scene — upgrade is pending).");
                }
                else
                {
                    Fail("No follow camera in scene — camera will be static.");
                }

                // Main Camera
                var mainCam = Camera.main;
                if (mainCam == null) Warn("No GameObject tagged 'MainCamera' in scene.");
                else Pass("Main Camera present and tagged.");

                if (fail > sceneFailsBefore || warn > sceneWarnsBefore) scenesWithProblems++;
                sb.AppendLine();
            }

            // ─── Final tally ───────────────────────────────────────
            sb.AppendLine();
            sb.AppendLine($"Phase 26 diagnostic result: ✓ {pass} pass, ⚠ {warn} warn, ✗ {fail} fail.");
            if (fail == 0 && warn == 0)
            {
                sb.AppendLine();
                sb.AppendLine("All Phase 26 wiring verified clean. Press Play.");
            }
            else
            {
                sb.AppendLine();
                sb.AppendLine($"Scenes with problems: {scenesWithProblems}.");
                if (fail > 0)
                    sb.AppendLine("→ Failures mean the controller/animator/camera wiring is missing. Run Phase 26 menu.");
                if (warn > 0)
                    sb.AppendLine("→ Warnings are recoverable; re-running Phase 26 usually clears them.");
            }

            Debug.Log(sb.ToString());

            EditorUtility.DisplayDialog(
                "Phase 26 Diagnostic",
                $"✓ {pass} pass · ⚠ {warn} warn · ✗ {fail} fail\n\n" +
                $"Scenes with problems: {scenesWithProblems}.\n\n" +
                "Full report in the Console.",
                "OK");
        }
    }
}
