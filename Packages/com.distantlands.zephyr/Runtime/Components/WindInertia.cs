// Distant Lands 2025
// Adds Zephyr to a renderer. Useful for spring force!
// All contents in this file are protected by the Unity Asset Store EULA

using UnityEngine;

namespace DistantLands.Zephyr
{
    /// <summary>
    /// Applies Zephyr wind to a Renderer using spring force for smooth wind transitions.
    /// </summary>
    public class WindInertia : WindEffectListener
    {
        /// <summary>
        /// The Renderer to apply wind to.
        /// </summary>
        [Tooltip("The Renderer to apply wind to.")]
        [SerializeField]
        private new Renderer renderer;
        private MaterialPropertyBlock propertyBlock;
        private int windVectorHistoryID;

        void Awake()
        {
            if (!renderer && GetComponent<Renderer>())
                renderer = GetComponent<Renderer>();

            InitializeListener();

            propertyBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(propertyBlock);
            windVectorHistoryID = Shader.PropertyToID("_ZEPHYR_WindHistory");
        }

        void Update()
        {
            //Double check that the effect listener can function at this time.
            if (renderer == null || zephyr == null)
            {
                renderer = GetComponent<Renderer>();
                InitializeListener();
                return;
            }
            UpdateWindInertia();

            propertyBlock.SetVector("_ZEPHYR_WindHistory", windVector);
            renderer.SetPropertyBlock(propertyBlock);
        }

        void OnDisable()
        {
            propertyBlock.SetVector(windVectorHistoryID, Vector3.zero);
            renderer.SetPropertyBlock(propertyBlock);
        }
    }
}
