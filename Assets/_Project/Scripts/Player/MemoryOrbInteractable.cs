// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Player / MemoryOrbInteractable
//
// A memory orb in the world. Picking it up sets it as the player's "held
// memory". Re-interacting either places it on a shelf, returns it to the
// counter, or feeds it into the workbench depending on context.

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

        private MaterialPropertyBlock _mpb;

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
            _mpb.SetFloat(clarityShaderProp, runtimeClarity);
            _mpb.SetFloat(crackShaderProp, runtimeCrackIntensity);
            if (memory != null) _mpb.SetColor(tintShaderProp, memory.EffectiveTint);
            orbRenderer.SetPropertyBlock(_mpb);
        }
    }
}
