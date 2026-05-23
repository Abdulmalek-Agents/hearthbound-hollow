// Distant Lands 2025
// Wind zone that funnels wind direction and multiplies wind strength within a radius.
// All contents in this file are protected by the Unity Asset Store EULA

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DistantLands.Zephyr
{
    [AddComponentMenu("Distant Lands/Zephyr/Funnel Zone", 10)]
    /// <summary>
    /// Wind zone that funnels wind direction and multiplies wind strength within a radius.
    /// </summary>
    public class FunnelZone : ZephyrWindZone
    {
        public override int ApplicationOrder => 10;
        public override float Radius => radius;
        public override float Strength => accelerationCoefficient;
        public override int ID => 4;
        public override Vector3 Position => zoneTransform.position;
        public override Vector3 Direction => zoneTransform.forward;
        public override float AuxOne => 0;
        public override float AuxTwo => 0;
        public override float AuxThree => 0;
        public override float VariationTime => 0;
        public override float VariationMagnitude => 0;
        public override float VariationOffsetX => 0;
        public override float VariationOffsetY => 0;
        /// <summary>
        /// The radius of the funnel zone.
        /// </summary>
        [Tooltip("The radius of the funnel zone.")]
        public float radius;
        /// <summary>
        /// The multiplier for wind acceleration within the funnel zone.
        /// </summary>
        [Tooltip("The multiplier for wind acceleration within the funnel zone.")]
        public float accelerationCoefficient;

        public override void ApplyWind(Vector3 position, ref Vector3 windVector)
        {
            if (!enabled)
                return;

            if ((position - zoneTransform.position).sqrMagnitude > (radius * radius))
                return;

            float influence = 1 - Mathf.Clamp01((position - zoneTransform.position).sqrMagnitude / (radius * radius));

            windVector = Vector3.Lerp(windVector.normalized, zoneTransform.forward, influence * influence) * windVector.magnitude;


            windVector *= Mathf.Lerp(1, accelerationCoefficient, influence * influence);
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
