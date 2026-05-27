
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Exoa.Designer
{
    /// <summary>
    /// The ModuleController class inherits from OutlineHandler and manages the behavior of modules in the scene.
    /// It includes properties for grid snapping, tile type, collider handling, and initial rotation.
    /// </summary>
    public class ModuleController : OutlineHandler
    {
        /// <summary>
        /// Indicates whether the module should snap to a grid.
        /// </summary>
        public bool snapOnGrid = true;

        /// <summary>
        /// Indicates if the module represents a ground tile.
        /// </summary>
        public bool isGroundTile;

        /// <summary>
        /// Indicates if the module represents a ceiling tile.
        /// </summary>
        public bool isCeilTile;

        /// <summary>
        /// If true, other module colliders will be ignored.
        /// </summary>
        public bool ignoreOtherModuleColliders;

        /// <summary>
        /// The initial rotation of the module in Euler angles.
        /// </summary>
        public Vector3 initRotation;

        /// <summary>
        /// Initializes the module by calling the base Start method and cleaning up
        /// children objects with the name "remove" under certain conditions.
        /// </summary>
        override protected void Start()
        {
            base.Start();
            if (SceneManager.GetActiveScene().name != "LevelEditor")
            {
                transform.DestroyChildrenByName("remove", true);
            }
        }

        /// <summary>
        /// Gets the initial rotation of the module as a Quaternion based on the initRotation property.
        /// </summary>
        /// <returns>A Quaternion representing the initial rotation of the module.</returns>
        public Quaternion GetInitRotation()
        {
            return Quaternion.Euler(initRotation);
        }
    }
}
