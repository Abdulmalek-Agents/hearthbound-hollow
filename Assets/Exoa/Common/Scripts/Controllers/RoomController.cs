
using Exoa.Events;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Exoa.Designer
{
    /// <summary>
    /// Controller for managing and rebuilding room structures in a procedural environment.
    /// Inherits from <see cref="SpaceController"/> and implements <see cref="IObjectDrawer"/>.
    /// </summary>
    public class RoomController : SpaceController, IObjectDrawer
    {
#if FLOORMAP_MODULE
        /// <summary>
        /// Gets or sets the procedural room associated with this controller.
        /// </summary>
        public ProceduralRoom proceduralRoom
        {
            get
            {
                return proceduralSpace as ProceduralRoom;
            }

            set
            {
                proceduralSpace = value;
            }
        }

        /// <summary>
        /// Rebuilds the room's mesh and repositions openings based on the current configuration.
        /// This method utilizes a delay to prevent frequent rebuilds and handles queued events.
        /// </summary>
        /// <param name="sendRepositionOpeningsEvent">Indicates whether to send an event to reposition openings.</param>
        override protected void Rebuild(bool sendRepositionOpeningsEvent = false)
        {
            // Check if the last rebuild was too recent
            if (lastRebuild > Time.time - delayBetweenRebuilds)
            {
                queuedRebuild = true;
                if (sendRepositionOpeningsEvent)
                    queuedSendRepositionOpeningsEvent = true;
                return;
            }

            // Send event for repositioning openings if required
            if (sendRepositionOpeningsEvent)
            {
                GameEditorEvents.OnRequestRepositionOpenings?.Invoke();
            }

            lastRebuild = Time.time;
            queuedRebuild = false;
            queuedSendRepositionOpeningsEvent = false;

            // Get the world position list of points
            List<Vector3> worldPosList = cpc.GetPointsWorldPositionList();

            if (proceduralSpace == null)
                proceduralSpace = GetComponent<ProceduralRoom>();

            // If there are not enough points or they are collinear, generate an empty room
            if (worldPosList.Count < 3 || MathUtils.PointsAreInLine(worldPosList))
            {
                proceduralSpace.GenerateEmpty();
                return;
            }

            // Preparing to retrieve all windows and openings
            List<ProceduralRoom.GenericOpening> openings = new List<ProceduralRoom.GenericOpening>();
            List<Vector3> pointList = null;
            UIBaseItem[] items = GameObject.FindObjectsOfType<UIBaseItem>();

            for (int i = 0; i < items.Length; i++)
            {
                ProceduralRoom.GenericOpening.OpeningType oType = ProceduralRoom.GenericOpening.OpeningType.Opening;
                Enum.TryParse<ProceduralRoom.GenericOpening.OpeningType>(items[i].sequencingItemType.ToString(), out oType);

                // Check the type of each item and collect opening points accordingly
                if (items[i].sequencingItemType == DataModel.FloorMapItemType.Door ||
                   items[i].sequencingItemType == DataModel.FloorMapItemType.Window ||
                   items[i].sequencingItemType == DataModel.FloorMapItemType.Opening)
                {
                    pointList = items[i].cpc.GetPointsWorldPositionList();
                    for (int j = 0; j < pointList.Count; j++)
                    {
                        openings.Add(new ProceduralRoom.GenericOpening(oType, pointList[j], items[i]));
                    }
                }
            }

            // Update procedural space with the collected openings and current color
            proceduralSpace.Openings = openings;
            proceduralSpace.SpaceVertexColor = DrawingColor;
            proceduralSpace.Generate(worldPosList);

            // Trigger a rebuild event for the building
            GameEditorEvents.OnRequestRebuildBuilding?.Invoke();
        }
#endif
    }
}
