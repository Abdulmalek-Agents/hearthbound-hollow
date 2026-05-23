// Distant Lands 2025
// Wind zone that multiplies wind strength within a radius.
// All contents in this file are protected by the Unity Asset Store EULA

using UnityEngine;

namespace DistantLands.Zephyr
{
    [AddComponentMenu("Distant Lands/Zephyr/Acceleration Zone", 10)]
    /// <summary>
    /// Wind zone that multiplies wind strength within a radius.
    /// </summary>
    public class AccelerationZone : ZephyrWindZone
    {
        public override int ApplicationOrder => 10;
        public override float Radius => radius;
        public override float Strength => accelerationCoefficient;
        public override int ID => 3;
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
        /// The radius of the acceleration zone.
        /// </summary>
        [Tooltip("The radius of the acceleration zone.")]
        public float radius = 15;
        /// <summary>
        /// The multiplier for wind acceleration within the zone.
        /// </summary>
        [Tooltip("The multiplier for wind acceleration within the zone.")]
        [Range(0, 10)]
        public float accelerationCoefficient = 0;

        public override void ApplyWind(Vector3 position, ref Vector3 windVector)
        {
            if (!enabled)
                return;

            if ((position - zoneTransform.position).sqrMagnitude > (radius * radius))
                return;

            float influence = 1 - Mathf.Clamp01((position - zoneTransform.position).sqrMagnitude / (radius * radius));

            windVector *= Mathf.Lerp(1, accelerationCoefficient, influence * influence);
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
