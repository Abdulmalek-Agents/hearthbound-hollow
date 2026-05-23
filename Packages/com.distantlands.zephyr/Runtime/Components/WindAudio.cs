// Distant Lands 2025
// Lets Zephyr control an audio source component
// All contents in this file are protected by the Unity Asset Store EULA

using UnityEngine;

namespace DistantLands.Zephyr
{
    [AddComponentMenu("Distant Lands/Zephyr/Wind Audio")]
    [RequireComponent(typeof(AudioSource))]
    /// <summary>
    /// Controls an AudioSource volume based on Zephyr wind strength.
    /// </summary>
    public class WindAudio : WindEffectListener
    {
        private AudioSource source;
        /// <summary>
        /// The animation curve used to remap wind strength to audio volume.
        /// </summary>
        [Tooltip("The animation curve used to remap wind strength to audio volume.")]
        public AnimationCurve volumeCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

        /// <summary>
        /// The minimum wind speed for remapping.
        /// </summary>
        [Tooltip("The minimum wind speed for remapping.")]
        public float minWindSpeed = 2;
        /// <summary>
        /// The maximum wind speed for remapping.
        /// </summary>
        [Tooltip("The maximum wind speed for remapping.")]
        public float maxWindSpeed = 4;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            InitializeListener();
            source = GetComponent<AudioSource>();
        }

        // Update is called once per frame
        void Update()
        {
            //Double check that the effect listener can function at this time.
            if (source == null || zephyr == null)
            {
                source = GetComponent<AudioSource>();
                InitializeListener();
                return;
            }

            UpdateWindInertia();

            float remapped = Mathf.Clamp01(Mathf.InverseLerp(minWindSpeed, maxWindSpeed, windVector.magnitude * windEffectMultiplier));

            source.volume = volumeCurve.Evaluate(remapped);
        }
    }
}
