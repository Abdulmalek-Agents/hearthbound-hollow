// Distant Lands 2025
// Wind zone that applies a constant directional force within a radius, with optional variation.
// All contents in this file are protected by the Unity Asset Store EULA

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DistantLands.Zephyr
{
    [AddComponentMenu("Distant Lands/Zephyr/Constant Force Zone", 10)]
    /// <summary>
    /// Wind zone that applies a constant directional force within a radius, with optional variation.
    /// </summary>
    public class ConstantForceZone : ZephyrWindZone
    {
        public override int ApplicationOrder => 1;
        public override float Radius => radius;
        public override float Strength => windStrength;
        public override int ID => 1;
        public override Vector3 Position => zoneTransform.position;
        public override Vector3 Direction => zoneTransform.forward;
        public override float AuxOne => 0;
        public override float AuxTwo => 0;
        public override float AuxThree => 0;
        public override float VariationTime => variationFrequency;
        public override float VariationMagnitude => variationMagnitude;
        public override float VariationOffsetX => offsetX;
        public override float VariationOffsetY => offsetY;
        float offsetX;
        float offsetY;
        /// <summary>
        /// The radius of the constant force zone.
        /// </summary>
        [Tooltip("The radius of the constant force zone.")]
        public float radius = 15;
        /// <summary>
        /// The strength of the wind force applied.
        /// </summary>
        [Tooltip("The strength of the wind force applied.")]
        public float windStrength = 3;
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
        /// <summary>
        /// The speed at which the variation offset changes.
        /// </summary>
        [Tooltip("The speed at which the variation offset changes.")]
        public float variationSpeed = 15;

        void Awake()
        {
            offsetX = Random.value;
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

            if ((position - Position).sqrMagnitude > (radius * radius))
                return;

            float influence = 1 - Mathf.Clamp01((position - Position).sqrMagnitude / (radius * radius));

            windVector += influence * influence * Direction * windStrength;
        }

#if UNITY_EDITOR
        public void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.549f, 0.4f, 1f);
            Gizmos.DrawWireSphere(Position, radius);

            Vector3[] points = new Vector3[] {
                new Vector3(0f,0f,2f),
                new Vector3(1f,0f,-1f),
                new Vector3(1f,0f,-1f),
                new Vector3(0f,0f,0f),
                new Vector3(0f,0f,0f),
                new Vector3(-1,0,-1),
                new Vector3(-1,0,-1),
                new Vector3(0f,0f,2f),
            };
            Handles.color = Color.white;
            Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, transform.forward.normalized);

            // Apply the rotation to each point
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = transform.position + rotation * points[i];
            }

            Handles.DrawLines(points);
        }
#endif
    }
}
