// Distant Lands 2025
// Lets Zephyr control a Visual Effect
// All contents in this file are protected by the Unity Asset Store EULA

using UnityEngine;
using UnityEngine.VFX;

namespace DistantLands.Zephyr
{
    [RequireComponent(typeof(VisualEffect))]
    [ExecuteAlways]
    /// <summary>
    /// Controls a Visual Effect component using Zephyr wind.
    /// </summary>
    public class WindVisualEffect : WindEffectListener
    {
        private int _ID;

        private VisualEffect vfx;

        void Awake()
        {
            InitializeListener();

            vfx = GetComponent<VisualEffect>();

            if (vfx == null || vfx.visualEffectAsset == null)
            {
                Debug.LogError($"Please ensure that {name} has a Visual Effect component and that the visual effect asset is properly assigned.");
                return;
            }


            _ID = Shader.PropertyToID("ZephyrWind");
        }

        void Update()
        {
            //Double check that the effect listener can function at this time.
            if (vfx == null || vfx.visualEffectAsset == null || zephyr == null)
            {
                vfx = GetComponent<VisualEffect>();
                InitializeListener();
                return;
            }

            UpdateWindInertia();

            vfx.SetVector3(_ID, windVector);
        }
    }
}