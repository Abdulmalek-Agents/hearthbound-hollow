// Distant Lands 2025
// Lets Zephyr control an animator component
// All contents in this file are protected by the Unity Asset Store EULA

using UnityEngine;

namespace DistantLands.Zephyr
{
    [AddComponentMenu("Distant Lands/Zephyr/Wind Animator")]
    [RequireComponent(typeof(Animator))]
    /// <summary>
    /// Controls an Animator parameter based on Zephyr wind strength.
    /// </summary>
    public class WindAnimator : WindEffectListener
    {
        private Animator anim;
        /// <summary>
        /// The name of the Animator float parameter to control.
        /// </summary>
        [Tooltip("The name of the Animator float parameter to control.")]
        public string animatedPropertyName;
        /// <summary>
        /// The animation curve used to remap wind strength to the Animator parameter.
        /// </summary>
        [Tooltip("The animation curve used to remap wind strength to the Animator parameter.")]
        public AnimationCurve curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        /// <summary>
        /// The minimum wind speed for remapping.
        /// </summary>
        [Tooltip("The minimum wind speed for remapping.")]
        public float minWindSpeed;
        /// <summary>
        /// The maximum wind speed for remapping.
        /// </summary>
        [Tooltip("The maximum wind speed for remapping.")]
        public float maxWindSpeed;

        void Awake()
        {
            InitializeListener();
            anim = GetComponent<Animator>();
        }

        // Update physics calculations
        void Update()
        {
            //Double check that the effect listener can function at this time.
            if (anim == null || zephyr == null)
            {
                anim = GetComponent<Animator>();
                InitializeListener();
                return;
            }

            UpdateWindInertia();

            float remapped = Mathf.Clamp01(Mathf.InverseLerp(minWindSpeed, maxWindSpeed, windVector.magnitude * windEffectMultiplier));
            anim.SetFloat(animatedPropertyName, curve.Evaluate(remapped));
        }

    }
}
