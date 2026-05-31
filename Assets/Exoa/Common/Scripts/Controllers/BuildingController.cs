
using Exoa.Events;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Exoa.Designer
{
    /// <summary>
    /// Controls the rebuilding of procedural buildings in the Unity scene.
    /// It listens for rebuild requests and manages the procedural generation of rooms and roofs.
    /// </summary>
    public class BuildingController : MonoBehaviour
    {
#if FLOORMAP_MODULE
        private float lastRebuild;
        public float delayBetweenRebuilds = .1f;
        private bool queuedRebuild;
        private ProceduralBuilding proceduralBuilding;
        private ProceduralRoofTop proceduralRoof;

        /// <summary>
        /// Unsubscribes from the rebuild event when this object is destroyed.
        /// </summary>
        void OnDestroy()
        {
            GameEditorEvents.OnRequestRebuildBuilding -= OnRequestRebuildBuilding;
            GameEditorEvents.OnRequestRebuildAllRooms -= OnRequestRebuildAllRooms;
        }

        /// <summary>
        /// Subscribes to the rebuild request event when this object starts.
        /// </summary>
        public void Start()
        {
            //print("BuildingController Start");
            GameEditorEvents.OnRequestRebuildBuilding += OnRequestRebuildBuilding;
            GameEditorEvents.OnRequestRebuildAllRooms += OnRequestRebuildAllRooms;
        }

        private void OnRequestRebuildAllRooms()
        {
            //print("BuildingController OnRequestRebuildBuilding");
            queuedRebuild = true;
            lastRebuild = Time.time;
        }

        /// <summary>
        /// Handles the rebuild request event by initiating a rebuild of the building.
        /// </summary>
        private void OnRequestRebuildBuilding()
        {
            //print("BuildingController OnRequestRebuildBuilding");
            Rebuild();
        }

        /// <summary>
        /// Updates the building state. If a rebuild has been queued and the delay has passed, it executes the rebuild.
        /// </summary>
        void Update()
        {
            if (queuedRebuild && lastRebuild < Time.time - delayBetweenRebuilds)
            {
                Rebuild();
            }
        }

        /// <summary>
        /// Rebuilds the building based on existing rooms and openings. It updates the procedural models for both the building and the roof.
        /// </summary>
        private void Rebuild()
        {
            if (lastRebuild > Time.time - delayBetweenRebuilds)
            {
                queuedRebuild = true;
                return;
            }

            lastRebuild = Time.time;
            queuedRebuild = false;

            ProceduralRoom[] rooms = GameObject.FindObjectsOfType<ProceduralRoom>();
            HDLogger.Log("Rebuild Building rooms.Length:" + rooms.Length, HDLogger.LogCategory.Building);

            if (rooms.Length < 1)
            {
                proceduralBuilding.Clear(true, true, true);
                proceduralRoof.Clear(true, true, true);
                return;
            }
            // Getting all windows
            List<ProceduralRoom.GenericOpening> openings = new List<ProceduralRoom.GenericOpening>();
            List<Vector3> pointList = null;
            UIBaseItem[] items = GameObject.FindObjectsOfType<UIBaseItem>();

            for (int i = 0; i < items.Length; i++)
            {
                ProceduralRoom.GenericOpening.OpeningType oType = ProceduralRoom.GenericOpening.OpeningType.Opening;
                Enum.TryParse<ProceduralRoom.GenericOpening.OpeningType>(items[i].sequencingItemType.ToString(), out oType);
                if (items[i].cpc == null)
                {
                    continue;
                }
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
            //print("BuildingController openings:" + openings);

            if (proceduralBuilding == null)
                proceduralBuilding = GetComponent<ProceduralBuilding>();
            proceduralBuilding.Openings = openings;
            proceduralBuilding.DisableCollider();
            proceduralBuilding.Generate(new List<ProceduralSpace>(rooms));

            if (proceduralRoof == null)
                proceduralRoof = GetComponent<ProceduralRoofTop>();
            //print("proceduralRoof:" + proceduralRoof);
            if (proceduralRoof != null && proceduralBuilding.BuildingContoursOuter != null)
            {
                proceduralRoof.ClearPreviousDrafts();
                proceduralRoof.GenerateFromWalls(proceduralBuilding.BuildingContoursOuter,
                    proceduralBuilding.RoomsContours, false);

                proceduralRoof.DisableCollider();
                proceduralRoof.Render();
            }
        }

#endif
    }
}
