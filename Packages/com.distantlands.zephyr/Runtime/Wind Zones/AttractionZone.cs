// Distant Lands 2025
// Wind zone that attracts wind toward its center, with optional variation.
// All contents in this file are protected by the Unity Asset Store EULA

using UnityEngine;

namespace DistantLands.Zephyr
{
    [AddComponentMenu("Distant Lands/Zephyr/Attraction Zone", 10)]
    /// <summary>
    /// Wind zone that attracts wind toward its center, with optional variation.
    /// </summary>
    public class AttractionZone : ZephyrWindZone
    {
        public override int ApplicationOrder => 0;
        public override float Radius => radius;
        public override float Strength => attractionStrength;
        public override int ID => 2;
        public override Vector3 Position => zoneTransform.position;
        public override Vector3 Direction => zoneTransform.forward;
        public override float AuxOne => 0;
        public override float AuxTwo => 0;
        public override float AuxThree => 0;
        public override float VariationTime => variationFrequency;
        public override float VariationMagnitude => variationMagnitude;
        public override float VariationOffsetX => offsetX;
        public override float VariationOffsetY => offsetY;
        /// <summary>
        /// The speed at which the variation offset changes.
        /// </summary>
        [Tooltip("The speed at which the variation offset changes.")]
        public float variationSpeed = 15;
        float offsetX;
        float offsetY;
        /// <summary>
        /// The radius of the attraction zone.
        /// </summary>
        [Tooltip("The radius of the attraction zone.")]
        public float radius = 15;
        /// <summary>
        /// The strength of the attraction force.
        /// </summary>
        [Tooltip("The strength of the attraction force.")]
        [Range(-10, 10)]
        public float attractionStrength = 2;
        [Space(10)]
        /// <summary>
        /// The magnitude of wind variation.
        /// </summary>
        [Tooltip("The magnitude of wind variation.")]
        [Range(0, 1)]
        public float variationMagnitude = 0.5f;
        /// <summary>
        /// The frequency of wind variation.
        /// </summary>
        [Tooltip("The frequency of wind variation.")]
        public float variationFrequency = 50;

        void Awake()
        {
            offsetX = Random.value;
            offsetY = Random.value;
        }
        
        void Update()
        {
            offsetX -= Direction.x * variationSpeed * Time.deltaTime / variationFrequency;
            offsetY -= Direction.z * variationSpeed * Time.deltaTime / variationFrequency;

            offsetX = Mathf.Repeat(offsetX, 1);
            offsetY = Mathf.Repeat(offsetY, 1);
        }

        public override void ApplyWind(Vector3 position, ref Vector3 windVector)
        {
            if (!enabled)
                return;

            if ((position - zoneTransform.position).sqrMagnitude > (radius * radius))
                return;

            float influence = 1 - Mathf.Clamp01((position - zoneTransform.position).sqrMagnitude / (radius * radius));

            windVector += (zoneTransform.position - position).normalized * influence * influence * attractionStrength;
        }

#if UNITY_EDITOR
        public void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.549f, 0.4f, 1f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
#endif
    }
}
