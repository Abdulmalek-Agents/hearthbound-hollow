// Distant Lands 2025
// Lets Zephyr control a particle system
// All contents in this file are protected by the Unity Asset Store EULA

using UnityEngine;

namespace DistantLands.Zephyr
{
    [AddComponentMenu("Distant Lands/Zephyr/Wind Particle")]
    [RequireComponent(typeof(ParticleSystem))]
    /// <summary>
    /// Controls a ParticleSystem using Zephyr wind.
    /// </summary>
    public class WindParticles : WindEffectListener
    {
        private ParticleSystem ps;
        /// <summary>
        /// The type of wind influence to apply to particles.
        /// </summary>
        public enum InfluenceType
        {
            Position,
            Velocity,
        }
        /// <summary>
        /// If true, applies wind to each particle individually.
        /// </summary>
        [Tooltip("If true, applies wind to each particle individually.")]
        public bool perParticle;
        /// <summary>
        /// The type of wind influence to apply to the particle system.
        /// </summary>
        [Tooltip("The type of wind influence to apply to the particle system.")]
        public InfluenceType influenceType;

        private ParticleSystem.Particle[] particles;

        void Awake()
        {
            if (ps == null)
                ps = GetComponent<ParticleSystem>();

            InitializeListener();

            particles = new ParticleSystem.Particle[ps.main.maxParticles];
        }

        void Update()
        {
            //Double check that the effect listener can function at this time.
            if (ps == null || zephyr == null)
            {
                ps = GetComponent<ParticleSystem>();
                InitializeListener();
                return;
            }

            if (particles == null)
                particles = new ParticleSystem.Particle[ps.main.maxParticles];

            if (ps.isPaused || !ps.isPlaying || !ps.IsAlive(true))
                return;

            UpdateWindInertia();

            int numParticlesAlive = ps.GetParticles(particles);

            Vector3 mainWindDirection = transform.InverseTransformDirection(windVector) * windEffectMultiplier * Time.deltaTime;

            for (int i = 0; i < numParticlesAlive; i++)
            {
                switch (influenceType)
                {
                    case InfluenceType.Position:
                        Vector3 pos = particles[i].position;
                        if (perParticle)
                            pos += transform.InverseTransformDirection(ZephyrWind.Instance.GetWindAtPoint(particles[i].position)) * windEffectMultiplier * Time.deltaTime;
                        else
                            pos += mainWindDirection;
                        particles[i].position = pos;
                        break;
                    case InfluenceType.Velocity:
                        Vector3 vel = particles[i].velocity;
                        if (perParticle)
                            vel += transform.InverseTransformDirection(ZephyrWind.Instance.GetWindAtPoint(particles[i].position)) * windEffectMultiplier * Time.deltaTime;
                        else
                            vel += mainWindDirection;
                        particles[i].velocity = vel;
                        break;
                }
            }

            ps.SetParticles(particles, numParticlesAlive);
        }
    }
}
