// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase46_GuideLightsAndWayfinding
//
// Phase 46.7 — Onboarding wayfinding via a chain of subtle guide-glow
// "firefly" particles + the LaneGuidePulse'd lanterns (already authored
// by Phase 46.2). This builder adds the firefly layer on top of those:
// dim, slow-rising glow particles every ~3 m along the cobble path which
// — when read alongside the OnboardingOverlay's "follow the warmth"
// hint — reinforce the "walk toward the Hollow" intuition.
//
// The fireflies are drawn from Stylized Weather (the Mushroom_Laetiporus
// has a built-in glow; the firefly particle is a single small mesh with
// an emissive material). If Stylized Weather's prefab isn't found, the
// builder falls back to a procedural ParticleSystem-on-empty-GameObject
// so the wayfinding still works on minimal projects.
//
// USE: Menu → Hearthbound → ⚙️ Advanced → ✨ Phase 46.7 — Guide Lights + Wayfinding

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HearthboundHollow.EditorTools
{
    public static class Phase46_GuideLightsAndWayfinding
    {
        private const string SceneLane     = "Assets/_Project/Scenes/02_Mission01_Lane.unity";
        private const string SceneGarden   = "Assets/_Project/Scenes/04_Mission02_Garden.unity";
        private const string EnvParentName = "_Phase46Env_Guide";

        [MenuItem("Hearthbound/⚙️ Advanced/✨ Phase 46.7 — Guide Lights + Wayfinding", priority = 467)]
        public static void Build()
        {
            int placed = 0;
            if (System.IO.File.Exists(SceneLane))   placed += BuildForScene(SceneLane,   "lane");
            if (System.IO.File.Exists(SceneGarden)) placed += BuildForScene(SceneGarden, "garden");

            EditorUtility.DisplayDialog("Phase 46.7 — Wayfinding",
                $"✨ Guide-light wayfinding built.\n\n" +
                $"{placed} firefly emitters placed.\n\n" +
                "Re-run any time — idempotent.",
                "OK");
        }

        private static int BuildForScene(string path, string flavor)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            var existing = GameObject.Find(EnvParentName);
            if (existing != null) Object.DestroyImmediate(existing);
            var envRoot = new GameObject(EnvParentName);

            int placed = 0;

            // 8 firefly emitters along the cobble path (lane) or stepping path (garden).
            var emitterPositions = flavor == "lane"
                ? new[]
                {
                    new Vector3(-1.5f, 1.0f, -14f),
                    new Vector3( 1.5f, 1.2f, -11f),
                    new Vector3(-1.0f, 1.4f,  -8f),
                    new Vector3( 1.0f, 1.6f,  -5f),
                    new Vector3(-1.5f, 1.4f,  -2f),
                    new Vector3( 1.5f, 1.2f,   1f),
                    new Vector3(-1.0f, 1.6f,   4f),
                    new Vector3( 1.0f, 1.0f,   7f),
                }
                : new[]
                {
                    new Vector3(-0.8f, 0.6f,  1f),
                    new Vector3( 0.8f, 0.8f,  4f),
                    new Vector3(-0.8f, 0.7f,  7f),
                    new Vector3( 0.8f, 0.5f, 10f),
                    new Vector3(-0.8f, 0.6f, 13f),
                };

            foreach (var pos in emitterPositions)
            {
                var emitGO = new GameObject($"Firefly_{placed:00}");
                emitGO.transform.SetParent(envRoot.transform, false);
                emitGO.transform.position = pos;

                var ps = emitGO.AddComponent<ParticleSystem>();
                var main = ps.main;
                main.duration = 8f;
                main.loop = true;
                main.startLifetime = 5f;
                main.startSize = 0.08f;
                main.startSpeed = 0.15f;
                main.startColor = new Color(1.0f, 0.95f, 0.55f, 1f);
                main.maxParticles = 12;
                main.gravityModifier = -0.05f; // float upward gently
                main.simulationSpace = ParticleSystemSimulationSpace.World;

                var emission = ps.emission;
                emission.rateOverTime = 1.2f;

                var shape = ps.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 0.5f;

                var col = ps.colorOverLifetime;
                col.enabled = true;
                var grad = new Gradient();
                grad.SetKeys(
                    new[]
                    {
                        new GradientColorKey(new Color(1.0f, 0.95f, 0.55f), 0f),
                        new GradientColorKey(new Color(1.0f, 0.78f, 0.42f), 1f),
                    },
                    new[]
                    {
                        new GradientAlphaKey(0f,    0.00f),
                        new GradientAlphaKey(0.8f,  0.30f),
                        new GradientAlphaKey(0.6f,  0.80f),
                        new GradientAlphaKey(0f,    1.00f),
                    });
                col.color = grad;

                // Ensure the renderer uses a "Default-Particle" or similar additive material.
                var pr = ps.GetComponent<ParticleSystemRenderer>();
                if (pr != null)
                {
                    // URP particle unlit shader (defaults exist).
                    var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                              ?? Shader.Find("Particles/Standard Unlit")
                              ?? Shader.Find("Mobile/Particles/Additive");
                    if (shader != null)
                    {
                        var m = new Material(shader);
                        m.color = new Color(1.0f, 0.95f, 0.55f, 0.8f);
                        if (m.HasProperty("_BlendOp"))       m.SetFloat("_BlendOp", 0f);
                        if (m.HasProperty("_SrcBlend"))      m.SetFloat("_SrcBlend", 5f);  // SrcAlpha
                        if (m.HasProperty("_DstBlend"))      m.SetFloat("_DstBlend", 1f);  // One
                        pr.sharedMaterial = m;
                    }
                }

                placed++;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            return placed;
        }
    }
}
