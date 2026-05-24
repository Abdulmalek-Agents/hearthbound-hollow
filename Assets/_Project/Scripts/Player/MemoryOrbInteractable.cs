// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Player / MemoryOrbInteractable
//
// A memory orb in the world. Picking it up sets it as the player's "held
// memory". Re-interacting either places it on a shelf, returns it to the
// counter, or feeds it into the workbench depending on context.
//
// Visual fallback: when running on URP/Lit (Phase 12 MVP, before the
// Shader Graph MemoryOrb_Master exists), we also drive `_BaseColor` and
// `_EmissionColor` so the polish mini-game produces a visible orb-clearing
// effect even without the bespoke shader.

using UnityEngine;
using HearthboundHollow.Core;
using HearthboundHollow.Memory;

namespace HearthboundHollow.Player
{
    public class MemoryOrbInteractable : Interactable
    {
        [Header("Memory data")]
        public MemoryNodeSO memory;

        [Header("Visual")]
        public Renderer orbRenderer;
        [SerializeField] private string clarityShaderProp = "_Clarity";
        [SerializeField] private string crackShaderProp = "_CrackIntensity";
        [SerializeField] private string tintShaderProp = "_PaletteTint";

        [Header("URP/Lit fallback (used when the master Shader Graph is absent)")]
        [SerializeField] private bool driveUrpFallbackProperties = true;
        [SerializeField] private Color dimColor = new Color(0.45f, 0.42f, 0.36f, 1f);
        [SerializeField] private float emissionIntensityAtFullClarity = 1.8f;

        private MaterialPropertyBlock _mpb;
        private static readonly int BaseColorID    = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

        [Header("Runtime")]
        [Range(0f, 1f)] public float runtimeClarity = 0.4f;
        [Range(0f, 1f)] public float runtimeCrackIntensity = 0f;

        private void Awake()
        {
            _mpb = new MaterialPropertyBlock();
            if (memory != null)
            {
                runtimeClarity = memory.initialClarity;
                runtimeCrackIntensity = memory.crackIntensity;
            }
            ApplyVisual();
        }

        public override string GetDynamicPromptText()
        {
            if (memory == null) return "Examine orb";
            return $"Examine {memory.title}";
        }

        public override void Activate(GameObject player)
        {
            if (memory == null) { Hh.Warn(LogCategory.Memory, $"{name}: no MemoryNodeSO assigned."); return; }
            Hh.Log(LogCategory.Memory, $"Player examined memory '{memory.id} — {memory.title}'.");
            OnExamineRequested?.Invoke(this);
        }

        public event System.Action<MemoryOrbInteractable> OnExamineRequested;

        public void SetClarity(float newClarity) { runtimeClarity = Mathf.Clamp01(newClarity); ApplyVisual(); }
        public void SetCrackIntensity(float v) { runtimeCrackIntensity = Mathf.Clamp01(v); ApplyVisual(); }

        private void ApplyVisual()
        {
            if (orbRenderer == null) return;
            orbRenderer.GetPropertyBlock(_mpb);

            // Bespoke MemoryOrb_Master properties (no-op on URP/Lit, which is fine).
            _mpb.SetFloat(clarityShaderProp, runtimeClarity);
            _mpb.SetFloat(crackShaderProp, runtimeCrackIntensity);

            Color tint = memory != null ? memory.EffectiveTint : new Color(1f, 0.78f, 0.42f, 1f);
            _mpb.SetColor(tintShaderProp, tint);

            // URP/Lit fallback so the orb is visibly readable even without the master shader.
            if (driveUrpFallbackProperties)
            {
                Color baseColor = Color.Lerp(dimColor, tint, runtimeClarity);
                Color emission = tint * (runtimeClarity * emissionIntensityAtFullClarity);
                _mpb.SetColor(BaseColorID, baseColor);
                _mpb.SetColor(EmissionColorID, emission);
            }

            orbRenderer.SetPropertyBlock(_mpb);
        }
    }
}
