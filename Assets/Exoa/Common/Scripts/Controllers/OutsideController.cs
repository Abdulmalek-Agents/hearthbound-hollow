
using Exoa.Events;
using System.Collections.Generic;
using UnityEngine;

namespace Exoa.Designer
{
    /// <summary>
    /// Manages the outside space controller and implements object drawing functionality.
    /// Inherits from the SpaceController class and implements the IObjectDrawer interface.
    /// </summary>
    public class OutsideController : SpaceController, IObjectDrawer
    {
#if FLOORMAP_MODULE
        /// <summary>
        /// Gets or sets the procedural outside generation object.
        /// Casts the proceduralSpace to ProceduralOutside type for specific functionalities.
        /// </summary>
        public ProceduralOutside proceduralOutside
        {
            get
            {
                return proceduralSpace as ProceduralOutside;
            }

            set
            {
                proceduralSpace = value;
            }
        }

        /// <summary>
        /// Rebuilds the outside space. This method checks if enough time has passed since the last rebuild
        /// and manages events related to repositioning openings if necessary.
        /// </summary>
        /// <param name="sendRepositionOpeningsEvent">Indicates whether to send reposition openings event.</param>
        override protected void Rebuild(bool sendRepositionOpeningsEvent = false)
        {
            // Check for delay between rebuilds
            if (lastRebuild > Time.time - delayBetweenRebuilds)
            {
                queuedRebuild = true; // Queue the rebuild
                if (sendRepositionOpeningsEvent)
                    queuedSendRepositionOpeningsEvent = true; // Queue the reposition event
                return;
            }

            // Send reposition openings event if requested
            if (sendRepositionOpeningsEvent)
            {
                //print("call OnRequestRepositionOpenings");
                GameEditorEvents.OnRequestRepositionOpenings?.Invoke();
            }

            lastRebuild = Time.time; // Update the last rebuild time
            queuedRebuild = false; // Reset queued rebuild flag
            queuedSendRepositionOpeningsEvent = false; // Reset queued reposition openings event

            // Retrieve world position list from cpc
            List<Vector3> worldPosList = cpc.GetPointsWorldPositionList();

            // Ensure procedural space is initialized
            if (proceduralSpace == null)
                proceduralSpace = GetComponent<ProceduralOutside>();

            // Check if the world position list is valid for generation
            if (worldPosList.Count < 3 || MathUtils.PointsAreInLine(worldPosList))
            {
                proceduralSpace.GenerateEmpty(); // Generate empty if invalid
                return;
            }

            //print("Room Rebuild");

            // Set vertex color and generate the procedural space
            proceduralSpace.SpaceVertexColor = DrawingColor;
            proceduralSpace.Generate(worldPosList);
        }
#endif
    }
}
