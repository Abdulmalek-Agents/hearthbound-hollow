
using UnityEngine;

namespace Exoa.Designer
{
    /// <summary>
    /// Interface for drawing objects in the design environment.
    /// Provides properties and methods required for object drawing functionality.
    /// </summary>
    public interface IObjectDrawer
    {
        /// <summary>
        /// Gets the GameObject associated with the object drawer.
        /// </summary>
        GameObject GO { get; }

#if FLOORMAP_MODULE
        /// <summary>
        /// Gets or sets the ControlPointsController used by the object drawer.
        /// </summary>
        ControlPointsController Cpc { get; set; }
        
        /// <summary>
        /// Gets or sets the UIBaseItem associated with the object drawer.
        /// </summary>
        UIBaseItem UI { get; set; }

        /// <summary>
        /// Gets or sets the color used for drawing operations.
        /// </summary>
        Color DrawingColor { get; set; }

        /// <summary>
        /// Initializes the object drawer and prepares it for use.
        /// This method should be called before any drawing operations.
        /// </summary>
        void Init();
        
        /// <summary>
        /// Builds the necessary components for the object drawer using the given floor map data.
        /// </summary>
        /// <param name="data">The FloorMapItem containing data to build the object.</param>
        void Build(DataModel.FloorMapItem data);
#endif
    }
}
