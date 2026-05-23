// Distant Lands 2025
// Wind zone that creates a swirling vortex effect with optional attraction and variation.
// All contents in this file are protected by the Unity Asset Store EULA

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace DistantLands.Zephyr
{
    [AddComponentMenu("Distant Lands/Zephyr/Vortex Zone", 10)]
    /// <summary>
    /// Wind zone that creates a swirling vortex effect with optional attraction and variation.
    /// </summary>
    public class VortexZone : ZephyrWindZone
    {
        public override int ApplicationOrder => 0;
        public override float Radius => radius;
        public override float Strength => swirlStrength;
        public override int ID => 5;
        public override Vector3 Position => zoneTransform.position;
        public override Vector3 Direction => zoneTransform.up;
        public override float AuxOne => attractionStrength;
        public override float AuxTwo => 0;
        public override float AuxThree => 0;
        public override float VariationTime => variationFrequency;
        public override float VariationMagnitude => variationMagnitude;
        public override float VariationOffsetX => offset;
        public override float VariationOffsetY => 0;
        /// <summary>
        /// The radius of the vortex zone.
        /// </summary>
        [Tooltip("The radius of the vortex zone.")]
        public float radius = 15;
        /// <summary>
        /// The strength of the swirling wind effect.
        /// </summary>
        [Tooltip("The strength of the swirling wind effect.")]
        public float swirlStrength = 3;
        /// <summary>
        /// The strength of the attraction toward the vortex center.
        /// </summary>
        [Tooltip("The strength of the attraction toward the vortex center.")]
        public float attractionStrength = 1;
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
        float offset;

        void Awake()
        {
            offset = Random.value * Mathf.PI * 2;
        }

        public override void ApplyWind(Vector3 position, ref Vector3 windVector)
        {
            if (!enabled)
                return;

            Vector3 toCenter = Position - position;
            float sqrDist = toCenter.sqrMagnitude;

            if (sqrDist > radius * radius)
                return;

            // Influence falloff
            float influence = 1 - Mathf.Clamp01(sqrDist / (radius * radius));

            // Tangential swirl direction
            Vector3 swirlDir = Vector3.Cross(Direction, toCenter).normalized;

            // --- New: Calculate traveling sine wave offset ---
            // Project to XZ plane for angle around center
            Vector3 toCenterXZ = new Vector3(toCenter.x, 0f, toCenter.z);
            float angle = Mathf.Atan2(toCenterXZ.z, toCenterXZ.x); // radians
                                                                   // Create traveling wave along the circumference
            float wave = Mathf.Sin(angle + Time.time * variationFrequency * Mathf.PI * 2f + offset);
            float magnitudeMultiplier = 1f + wave * variationMagnitude;

            // Apply swirling
            windVector += swirlDir * swirlStrength * influence * influence * magnitudeMultiplier;

            // Apply attraction
            windVector += toCenter.normalized * attractionStrength * influence * influence * magnitudeMultiplier;
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.549f, 0.4f, 1f);

            int turns = 3;
            int segmentsPerTurn = 30;
            float height = 0f;

            Vector3[] spiralPoints = new Vector3[turns * segmentsPerTurn + 1];

            // Create rotation so local up matches the vortex axis
            Quaternion alignRotation = Quaternion.FromToRotation(Vector3.up, Direction);

            float angleStep = 360f / segmentsPerTurn;
            float radiusStep = radius / (turns * segmentsPerTurn);

            for (int i = 0; i < spiralPoints.Length; i++)
            {
                float angle = Mathf.Deg2Rad * (i * angleStep);
                float currentRadius = radius - i * radiusStep;

                // Spiral generated in local space (Y up)
                Vector3 localPos = new Vector3(Mathf.Cos(angle) * currentRadius, height * (i / (float)spiralPoints.Length), Mathf.Sin(angle) * currentRadius);

                // Rotate to match axis and offset to world position
                spiralPoints[i] = transform.position + alignRotation * localPos;
            }

            Handles.color = Color.cyan;
            for (int i = 0; i < spiralPoints.Length - 1; i++)
            {
                Handles.DrawLine(spiralPoints[i], spiralPoints[i + 1]);
            }


            Handles.DrawLine(transform.position - Direction * 4, transform.position + Direction * 4);
        }
#endif
    }
}
