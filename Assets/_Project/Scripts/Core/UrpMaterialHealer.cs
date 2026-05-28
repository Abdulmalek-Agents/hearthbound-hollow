// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Core / UrpMaterialHealer
//
// PHASE 32.18 — "Why is that one prop magenta?" runtime fix.
//
// User report (screenshot): walking around 03_Mission01_Hollow shows a
// small bright magenta cube near the lantern. Classic URP symptom: a
// renderer's material is using a shader that isn't valid under the
// active render pipeline, so Unity falls back to the built-in
// "Hidden/InternalErrorShader" — which renders solid magenta.
//
// Common causes in this project:
//   * A prop imported from an asset pack whose Standard / Built-In RP
//     shader didn't auto-convert during the URP migration.
//   * An ASE / vendor shader that failed to compile (the existing
//     BMLitShaderPatcher fixes the _SHADOWS_SOFT keyword case but
//     doesn't catch every variant).
//   * A material with a null shader reference after a script reimport.
//
// Fix — this component runs on Awake AND on every sceneLoaded, walks
// every Renderer in the active scene, and for each material whose
// shader is null / pink-placeholder / not-URP-compatible, swaps in
// URP/Lit. The original base colour is preserved by reading any of
// the common colour properties (`_Color`, `_BaseColor`, `_MainColor`,
// `_AlbedoColor`) and writing it into URP/Lit's `_BaseColor`. If no
// colour can be inferred, the heal uses a warm parchment fallback
// (matches the project's cozy palette) so even total fallbacks read
// as in-world objects rather than blinding magenta.
//
// Auto-spawn — `GameManager.Awake` adds this if missing, same pattern
// as VoicePlayer. So a pulled-fresh project is magenta-free on frame 1.
//
// Performance: one full walk per scene load (~milliseconds on the
// project's modest renderer count), nothing in Update.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HearthboundHollow.Core
{
    [DefaultExecutionOrder(-50)]
    public class UrpMaterialHealer : MonoBehaviour
    {
        public static UrpMaterialHealer Instance { get; private set; }

        [Header("Settings")]
        [Tooltip("Heal materials on every sceneLoaded (default). Disable to " +
                 "scan only once at Awake.")]
        public bool healOnEverySceneLoad = true;

        [Tooltip("Warm parchment fallback color when no source colour can " +
                 "be read off the broken material. Matches the project's " +
                 "cozy palette so total fallbacks blend with the scene.")]
        public Color fallbackBaseColor = new Color(0.78f, 0.66f, 0.46f, 1f);

        [Tooltip("Log every material the healer substitutes. Handy during a " +
                 "playtest, leave off for ship.")]
        public bool verbose = false;

        // Cached URP/Lit shader.
        private Shader _urpLit;
        private static readonly int _BaseColorID = Shader.PropertyToID("_BaseColor");

        // Colour property names to probe when carrying colour forward.
        private static readonly string[] ProbeColorProps =
            { "_BaseColor", "_Color", "_MainColor", "_AlbedoColor", "_TintColor" };

        // ───── Lifecycle ─────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            _urpLit = ResolveUrpLit();
            HealActiveScene();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void OnSceneLoaded(Scene s, LoadSceneMode m)
        {
            if (healOnEverySceneLoad) HealActiveScene();
        }

        // ───── Heal ──────────────────────────────────────────────

        public void HealActiveScene()
        {
            if (_urpLit == null) _urpLit = ResolveUrpLit();
            if (_urpLit == null)
            {
                Hh.Warn(LogCategory.Boot,
                    "UrpMaterialHealer: URP/Lit shader not found in this project " +
                    "(URP package missing?). Magenta materials cannot be auto-healed.");
                return;
            }

            int scanned = 0, healed = 0;

#if UNITY_2022_3_OR_NEWER
            var renderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,
                                                                FindObjectsSortMode.None);
#else
            var renderers = Object.FindObjectsOfType<Renderer>(includeInactive: true);
#endif
            foreach (var r in renderers)
            {
                if (r == null) continue;
                var mats = r.materials; // returns INSTANCES — safe to modify
                bool changed = false;
                for (int i = 0; i < mats.Length; i++)
                {
                    scanned++;
                    var m = mats[i];
                    if (!NeedsHeal(m)) continue;

                    Color carry = ReadBaseColor(m, fallbackBaseColor);
                    mats[i] = new Material(_urpLit)
                    {
                        name = (m != null && !string.IsNullOrEmpty(m.name) ? m.name : "AutoHealed") + " (URP-Healed)"
                    };
                    mats[i].SetColor(_BaseColorID, carry);
                    healed++;
                    changed = true;

                    if (verbose)
                    {
                        string shaderName = m != null && m.shader != null ? m.shader.name : "<null>";
                        Hh.Log(LogCategory.Boot,
                            $"UrpMaterialHealer: substituted '{shaderName}' on " +
                            $"'{r.gameObject.name}' (mat slot {i}) → URP/Lit base={carry}.");
                    }
                }
                if (changed) r.materials = mats;
            }

            if (healed > 0)
            {
                Hh.Log(LogCategory.Boot,
                    $"UrpMaterialHealer: healed {healed} magenta/broken material(s) " +
                    $"across {scanned} scanned in scene '{SceneManager.GetActiveScene().name}'.");
            }
        }

        // ───── Detection ─────────────────────────────────────────

        private static bool NeedsHeal(Material m)
        {
            if (m == null) return false;
            if (m.shader == null) return true;
            var n = m.shader.name;
            if (string.IsNullOrEmpty(n)) return true;
            // Unity's runtime placeholder for missing-shader = magenta.
            if (n == "Hidden/InternalErrorShader") return true;
            if (n == "InternalErrorShader") return true;
            // A subtle but real case: the shader exists but doesn't compile
            // under the active pipeline. `Material.isSupported` is the
            // canonical check — it returns false when the shader's
            // shader-target / keywords don't match what's loaded.
#if UNITY_2020_2_OR_NEWER
            if (m.shader.isSupported == false) return true;
#endif
            return false;
        }

        private static Color ReadBaseColor(Material m, Color fallback)
        {
            if (m == null) return fallback;
            foreach (var prop in ProbeColorProps)
            {
                if (m.HasProperty(prop))
                {
                    try { return m.GetColor(prop); } catch { /* not a colour */ }
                }
            }
            return fallback;
        }

        // ───── Shader lookup ─────────────────────────────────────

        private static Shader ResolveUrpLit()
        {
            // Probe the standard URP shader names; gracefully fall back to
            // built-in / standard if URP isn't installed in this project.
            var candidates = new[]
            {
                "Universal Render Pipeline/Lit",
                "Universal Render Pipeline/Simple Lit",
                "URP/Lit",
                "Standard",
                "Diffuse",
            };
            foreach (var n in candidates)
            {
                var s = Shader.Find(n);
                if (s != null && s.isSupported) return s;
            }
            return null;
        }

        // ───── Public re-run hook ────────────────────────────────

        /// <summary>
        /// Force a re-scan of the active scene. Call after instantiating
        /// new objects at runtime that might carry broken materials.
        /// </summary>
        public static void RehealNow()
        {
            if (Instance != null) Instance.HealActiveScene();
        }
    }
}
