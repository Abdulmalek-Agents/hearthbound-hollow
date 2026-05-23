// Distant Lands 2025
// Lets Zephyr control a cloth component
// All contents in this file are protected by the Unity Asset Store EULA

using UnityEngine;

namespace DistantLands.Zephyr
{
    [AddComponentMenu("Distant Lands/Zephyr/Wind Cloth")]
    [RequireComponent(typeof(Cloth))]
    /// <summary>
    /// Controls a Cloth component using Zephyr wind.
    /// </summary>
    public class WindCloth : WindEffectListener
    {
        /// <summary>
        /// Multiplier for random acceleration applied to the cloth.
        /// </summary>
        [Tooltip("Multiplier for random acceleration applied to the cloth.")]
        [Range(0, 1)]
        public float randomAccelerationMultiplier = 0.25f;
        private Cloth cloth;

        void Awake()
        {
            InitializeListener();
            cloth = GetComponent<Cloth>();
        }

        // Update physics calculations
        void Update()
        {
            //Double check that the effect listener can function at this time.
            if (cloth == null || zephyr == null)
            {
                cloth = GetComponent<Cloth>();
                InitializeListener();
                return;
            }
            UpdateWindInertia();
            cloth.externalAcceleration = windVector * windEffectMultiplier;
            cloth.randomAcceleration = cloth.externalAcceleration * randomAccelerationMultiplier;
        }

    }
}
