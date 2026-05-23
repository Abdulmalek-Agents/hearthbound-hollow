// Distant Lands 2025
// Abstract class for any Zephyr effect listener
// All contents in this file are protected by the Unity Asset Store EULA

using UnityEngine;

namespace DistantLands.Zephyr
{
    /// <summary>
    /// Base component for all components that listen to Zephyr wind.
    /// </summary>
    /// <summary>
    /// Base component for all components that listen to Zephyr wind.
    /// </summary>
    public abstract class WindEffectListener : MonoBehaviour
    {
        internal ZephyrWind zephyr;
        /// <summary>
        /// Pull the wind vector from another listener to avoid checking the wind vector twice for the same object.
        /// </summary>
        [Tooltip("Pull the wind vector from another listener to avoid checking the wind vector twice for the same object.")]
        public WindEffectListener overrideInputListener;
        [SerializeField] internal Vector3 windVector;
        [SerializeField] private Vector3 velocity;

        /// <summary>
        /// How strong the spring pulls toward the target wind.
        /// </summary>
        [Tooltip("How strong the spring pulls toward the target wind.")]
        public float springStrength = 10f;

        /// <summary>
        /// How much the spring resists oscillation (0 = undamped, 1 = critically damped, >1 = overdamped).
        /// </summary>
        [Tooltip("How much the spring resists oscillation (0 = undamped, 1 = critically damped, >1 = overdamped).")]
        public float damping = 0.8f;

        /// <summary>
        /// The local-space offset for wind sampling.
        /// </summary>
        [Tooltip("The local-space offset for wind sampling.")]
        public Vector3 windTestPoint;

        /// <summary>
        /// Multiplies the wind velocity by a constant. Default is 1.
        /// </summary>
        [Tooltip("Multiplies the wind velocity by a constant. Default is 1.")]
        public float windEffectMultiplier = 1;

        /// <summary>
        /// Multiplies each component of the wind vector by an axis strength. Useful for disabling vertical wind when needed.
        /// </summary>
        [Tooltip("Multiplies each component of the wind vector by an axis strength. Useful for disabling vertical wind when needed.")]
        public Vector3 windMask = Vector3.one;

        public bool InitializeListener()
        {
            zephyr = ZephyrWind.Instance;

            if (zephyr == null)
            {
                return false;
            }

            return true;
        }


        public void UpdateWindInertia()
        {
            if (overrideInputListener)
            {
                velocity = overrideInputListener.velocity;
                windVector = overrideInputListener.windVector;
            }
            Vector3 target = zephyr.GetWindAtPoint(transform.TransformPoint(windTestPoint)) * windEffectMultiplier;
            target = new Vector3(target.x * windMask.x, target.y * windMask.y, target.z * windMask.z);

            // Spring force calculation
            Vector3 displacement = windVector - target;
            Vector3 springForce = -springStrength * displacement;
            Vector3 dampingForce = -damping * velocity;

            Vector3 acceleration = springForce + dampingForce;

            velocity += acceleration * Time.deltaTime;
            windVector += velocity * Time.deltaTime;

        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            // Draw a small box at the test point in world space
            Gizmos.color = Color.white;
            Vector3 worldTestPoint = transform.TransformPoint(windTestPoint);
            Gizmos.DrawCube(worldTestPoint, Vector3.one * 0.05f);
        }
        void OnDrawGizmos()
        {
            Vector3 worldTestPoint = transform.TransformPoint(windTestPoint);

            if (Application.isPlaying)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(worldTestPoint, worldTestPoint + velocity);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(worldTestPoint, worldTestPoint + windVector);
            }
        }
#endif
    }
}
