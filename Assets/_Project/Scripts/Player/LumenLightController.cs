// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Player / LumenLightController
//
// Drives the Distant Lands "Lumen" stylized light meshes (god rays, candle
// glow, lantern halos) to react to time-of-day. We don't take a hard dep on
// the Lumen runtime — we drive its glow strength via a public `MeshRenderer`
// material property.

using System.Collections.Generic;
using UnityEngine;

namespace HearthboundHollow.Player
{
    public class LumenLightController : MonoBehaviour
    {
        [System.Serializable]
        public struct LumenTarget
        {
            public Renderer renderer;
            [Tooltip("Material property to drive (Lumen exposes _Intensity / _Strength / _Color).")]
            public string propertyName;
            [Range(0f, 5f)] public float morningValue;
            [Range(0f, 5f)] public float afternoonValue;
            [Range(0f, 5f)] public float eveningValue;
            [Range(0f, 5f)] public float nightValue;
        }

        [Header("Wired from DayCycleManager")]
        public DayCycleManager dayCycle;

        [Header("Targets")]
        public List<LumenTarget> targets = new();

        private MaterialPropertyBlock _mpb;

        private void Awake()
        {
            _mpb = new MaterialPropertyBlock();
            if (dayCycle != null)
            {
                dayCycle.OnTimeOfDayChanged += OnTimeOfDayChanged;
                OnTimeOfDayChanged(dayCycle.CurrentTimeOfDay);
            }
        }

        private void OnDestroy()
        {
            if (dayCycle != null) dayCycle.OnTimeOfDayChanged -= OnTimeOfDayChanged;
        }

        private void OnTimeOfDayChanged(TimeOfDay tod)
        {
            foreach (var t in targets)
            {
                if (t.renderer == null || string.IsNullOrEmpty(t.propertyName)) continue;
                float v = tod switch
                {
                    TimeOfDay.Morning => t.morningValue,
                    TimeOfDay.Afternoon => t.afternoonValue,
                    TimeOfDay.Evening => t.eveningValue,
                    TimeOfDay.Night => t.nightValue,
                    _ => t.afternoonValue,
                };
                t.renderer.GetPropertyBlock(_mpb);
                _mpb.SetFloat(t.propertyName, v);
                t.renderer.SetPropertyBlock(_mpb);
            }
        }
    }
}
