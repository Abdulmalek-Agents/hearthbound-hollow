
using Exoa.Events;
using Exoa.Maths;
using UnityEngine;
//using UnityEngine.Rendering.PostProcessing;

namespace Exoa.Designer
{
    /// <summary>
    /// Controls the post-processing effects related to lighting in the scene.
    /// Manages the directional light's shadow strength during perspective changes
    /// for a smoother visual experience.
    /// </summary>
    public class PostProcessController : MonoBehaviour
    {
        //private PostProcessLayer layer;

        /// <summary>
        /// The directional light that this controller will affect.
        /// </summary>
        public Light directionalLight;

        /// <summary>
        /// The default shadow strength to be applied when not in orthographic mode.
        /// </summary>
        public float defaultShadowStrength = 0.22f;

        /// <summary>
        /// A Spring mechanism to handle shadow strength transitions.
        /// </summary>
        public Springs shadowStrengthMove;

        /// <summary>
        /// The spring used to smoothly transition the shadow strength value.
        /// </summary>
        private FloatSpring shadowStrengthSpring;

        /// <summary>
        /// The target shadow strength value based on the current perspective.
        /// </summary>
        private float shadowStrengthTarget;

        /// <summary>
        /// Unsubscribes from the CameraEvents.OnBeforeSwitchPerspective event.
        /// </summary>
        void OnDestroy()
        {
            CameraEvents.OnBeforeSwitchPerspective -= OnBeforeSwitchPerspective;
        }

        /// <summary>
        /// Subscribes to the CameraEvents.OnBeforeSwitchPerspective event.
        /// </summary>
        void Awake()
        {
            //layer = GetComponent<PostProcessLayer>();
            CameraEvents.OnBeforeSwitchPerspective += OnBeforeSwitchPerspective;
        }

        /// <summary>
        /// Initializes the shadow strength target value
        /// and sets the initial value for the shadow strength spring.
        /// </summary>
        private void Start()
        {
            shadowStrengthTarget = directionalLight.shadowStrength;
            shadowStrengthSpring.Value = shadowStrengthTarget;
        }

        /// <summary>
        /// Updates the target shadow strength based on the current perspective mode.
        /// If orthographic mode is activated, the shadow strength is set to zero;
        /// otherwise, it uses the default shadow strength value.
        /// </summary>
        /// <param name="orthoMode">Indicates whether the camera is in orthographic mode.</param>
        private void OnBeforeSwitchPerspective(bool orthoMode)
        {
            shadowStrengthTarget = orthoMode ? 0 : defaultShadowStrength;
        }

        /// <summary>
        /// Updates the directional light's shadow strength based on the spring mechanism.
        /// This method is called once per frame.
        /// </summary>
        private void Update()
        {
            directionalLight.shadowStrength = shadowStrengthMove.Update(ref shadowStrengthSpring, shadowStrengthTarget);
        }
    }
}
