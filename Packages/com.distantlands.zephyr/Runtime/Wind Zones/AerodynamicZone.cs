// Distant Lands 2025
// Wind zone that aligns wind direction with zone movement and applies push force.
// All contents in this file are protected by the Unity Asset Store EULA

using UnityEngine;

namespace DistantLands.Zephyr
{
    [AddComponentMenu("Distant Lands/Zephyr/Aerodynamic Zone", 10)]
    /// <summary>
    /// Wind zone that aligns wind direction with zone movement and applies push force.
    /// </summary>
    public class AerodynamicZone : ZephyrWindZone
    {
        public override int ApplicationOrder => 0;
        public override float Radius => radius;
        public override float Strength => windAlignmentStrength;
        public override int ID => 6;
        public override Vector3 Position => zoneTransform.position;
        public override Vector3 Direction => delta;
        public override float AuxOne => windPushStrength;
        public override float AuxTwo => 0;
        public override float AuxThree => 0;
        public override float VariationTime => 0;
        public override float VariationMagnitude => 0;
        public override float VariationOffsetX => 0;
        public override float VariationOffsetY => 0;

        /// <summary>
        /// The radius of the aerodynamic zone.
        /// </summary>
        [Tooltip("The radius of the aerodynamic zone.")]
        public float radius = 15f;
        /// <summary>
        /// The strength with which wind aligns to the zone's movement.
        /// </summary>
        [Tooltip("The strength with which wind aligns to the zone's movement.")]
        public float windAlignmentStrength = 1f;
        /// <summary>
        /// The strength of the push force applied by the zone.
        /// </summary>
        [Tooltip("The strength of the push force applied by the zone.")]
        public float windPushStrength = 0.5f;

        private Vector3 delta;
        private Vector3 positionLastFrame;

        private enum CalculationMethod { Update, FixedUpdate, LateUpdate }
        [SerializeField]
        private CalculationMethod calculationMethod;

        void Start()
        {
            positionLastFrame = zoneTransform.position;
        }

        void Update()
        {
            if (calculationMethod != CalculationMethod.Update) return;
            delta = (zoneTransform.position - positionLastFrame) * 0.1f / Time.deltaTime;
            positionLastFrame = zoneTransform.position;
        }
        void FixedUpdate()
        {
            if (calculationMethod != CalculationMethod.FixedUpdate) return;
            delta = (zoneTransform.position - positionLastFrame) * 0.1f / Time.deltaTime;
            positionLastFrame = zoneTransform.position;
        }
        void LateUpdate()
        {
            if (calculationMethod != CalculationMethod.LateUpdate) return;
            delta = (zoneTransform.position - positionLastFrame) * 0.1f / Time.deltaTime;
            positionLastFrame = zoneTransform.position;
        }

        public override void ApplyWind(Vector3 position, ref Vector3 windVector)
        {
            if (!enabled)
                return;

            // Ignore if zone isn’t moving
            if (delta.sqrMagnitude < 0.0001f)
                return;

            float sqrDist = (position - zoneTransform.position).sqrMagnitude;
            if (sqrDist > radius * radius)
                return;

            float influence = 1f - Mathf.Clamp01(sqrDist / (radius * radius));

            // Push wind in the direction this object is moving
            windVector += (delta * windAlignmentStrength + -(zoneTransform.position - position).normalized * windPushStrength * delta.magnitude) * influence;
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.549f, 0.4f, 1f);
            Gizmos.DrawWireSphere(transform.position, radius);

            // Draw movement direction arrow
            if (Application.isPlaying && delta != Vector3.zero)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, transform.position + delta.normalized * 2f);
            }
        }
#endif
    }
}
