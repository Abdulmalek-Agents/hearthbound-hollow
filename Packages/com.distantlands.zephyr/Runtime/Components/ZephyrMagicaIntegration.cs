// Distant Lands 2025
// Lets Zephyr control a Magica wind zone component. Requires Magica Cloth 2
// All contents in this file are protected by the Unity Asset Store EULA


#if MAGICACLOTH2
using MagicaCloth2;
#endif
using UnityEngine;

namespace DistantLands.Zephyr
{
#if MAGICACLOTH2
    [AddComponentMenu("Distant Lands/Zephyr/Magica Integration")]
    [RequireComponent(typeof(MagicaWindZone))]
#endif
    /// <summary>
    /// Controls an Animator parameter based on Zephyr wind strength.
    /// </summary>
    public class ZephyrMagicaIntegration : WindEffectListener
    {

#if MAGICACLOTH2
        MagicaWindZone magicaWindZone;
#endif

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        void Awake()
        {
            InitializeListener();
#if MAGICACLOTH2
            magicaWindZone = GetComponent<MagicaWindZone>();
#endif
        }

        void Update()
        {
#if MAGICACLOTH2
            //Double check that the effect listener can function at this time.
            if (magicaWindZone == null || zephyr == null)
            {
                magicaWindZone = GetComponent<MagicaWindZone>();
                InitializeListener();
                return;
            }
            UpdateWindInertia();

            magicaWindZone.SetWindDirection(windVector.normalized);
            magicaWindZone.main = windVector.magnitude;
#endif
        }
    }
}