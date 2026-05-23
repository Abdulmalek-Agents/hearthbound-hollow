// Distant Lands 2025
// Abstract class that serves as a base for all windzones
// All contents in this file are protected by the Unity Asset Store EULA

using UnityEngine;

namespace DistantLands.Zephyr
{
    [ExecuteAlways]
    public abstract class ZephyrWindZone : MonoBehaviour
    {
        /// <summary>
        /// Sets the order in which this effect is applied. For additive effects make this 1 and multiplicative effects make this 2.
        /// </summary>
        public abstract int ApplicationOrder { get; }
        /// <summary>
        /// Sets the radius of the effect.
        /// </summary>
        public abstract float Radius { get; }
        /// <summary>
        /// Sets the strength of the effect.
        /// </summary>
        public abstract float Strength { get; }
        /// <summary>
        /// Defines a unique ID for this wind zone. All custom wind zones need to start after 16.
        /// </summary>
        public abstract int ID { get; }
        /// <summary>
        /// Quick reference to the position of the zone transform.
        /// </summary>
        public abstract Vector3 Position { get; }
        /// <summary>
        /// Quick reference to an axis of effect. Generally this is either the forward vector or the up vector of the transform.
        /// </summary>
        public abstract Vector3 Direction { get; }
        /// <summary>
        /// Auxiliary output. Can be set to any additional variable that you want to send to the GPU.
        /// </summary>
        public abstract float AuxOne { get; }
        /// <summary>
        /// Auxiliary output. Can be set to any additional variable that you want to send to the GPU.
        /// </summary>
        public abstract float AuxTwo { get; }
        /// <summary>
        /// Auxiliary output. Can be set to any additional variable that you want to send to the GPU.
        /// </summary>
        public abstract float AuxThree { get; }
        /// <summary>
        /// Generic variation frequency controller.
        /// </summary>
        public abstract float VariationTime { get; }
        /// <summary>
        /// Generic variation magnitude controller.
        /// </summary>
        public abstract float VariationMagnitude { get; }
        /// <summary>
        /// Generic variation offset controller for the X axis.
        /// </summary>
        public abstract float VariationOffsetX { get; }
        /// <summary>
        /// Generic variation offset controller for the Y axis.
        /// </summary>
        public abstract float VariationOffsetY { get; }
        /// <summary>
        /// Applies the windzone to the wind vector.
        /// </summary>
        public abstract void ApplyWind(Vector3 position, ref Vector3 windVector);
        [HideInInspector]
        public Transform zoneTransform;

        void OnEnable()
        {
            zoneTransform = transform;

            if (ZephyrWind.Instance != null)
            {
                ZephyrWind.Instance.AddWindZone(this);
            }
        }

        void OnDisable()
        {
            if (ZephyrWind.Instance != null)
            {
                ZephyrWind.Instance.RemoveWindZone(this);
            }
        }
    }
}