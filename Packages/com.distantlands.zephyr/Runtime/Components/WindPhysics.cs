// Distant Lands 2025
// Lets Zephyr control a rigidbody component
// All contents in this file are protected by the Unity Asset Store EULA

using UnityEngine;

namespace DistantLands.Zephyr
{
    [AddComponentMenu("Distant Lands/Zephyr/Wind Physics")]
    [RequireComponent(typeof(Rigidbody))]
    /// <summary>
    /// Controls a Rigidbody using Zephyr wind.
    /// </summary>
    public class WindPhysics : WindEffectListener
    {
        private Rigidbody rb;
        /// <summary>
        /// The force multiplier applied to the Rigidbody from wind.
        /// </summary>
        [Tooltip("The force multiplier applied to the Rigidbody from wind.")]
        public float windForce;

        void Awake()
        {
            InitializeListener();
            rb = GetComponent<Rigidbody>();

        }

        // Update physics calculations
        void FixedUpdate()
        {
            //Double check that the effect listener can function at this time.
            if (rb == null || zephyr == null)
            {
                rb = GetComponent<Rigidbody>();
                InitializeListener();
                return;
            }

            UpdateWindInertia();
            rb.AddForce(windVector * windForce);
        }

    }
}












