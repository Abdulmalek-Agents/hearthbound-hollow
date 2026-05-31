using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Exoa.Designer.DataModel;
using static Exoa.Designer.ProceduralSpace;

namespace Exoa.Designer
{
    /// <summary>
    /// Controls the generation and management of floors in the building design process.
    /// It handles the creation and destruction of rooms, openings, and outside spaces based on provided floor data.
    /// </summary>
    public class FloorController : MonoBehaviour
    {
        public GameObject proceduralRoomPrefab;
        public GameObject proceduralOutsidePrefab;
        public GameObject proceduralOpeningPrefab;
        public ProceduralBuilding building;
        public ProceduralRoofTop roof;
        public BuildingMaterialController buildingMaterialController;
        public Transform roomsContainer;
        public Transform openingsContainer;
        public Transform modulesContainer;

        private List<ProceduralSpace> allRoomSpaces = new List<ProceduralSpace>();
        private List<ProceduralOpening> allOpenings = new List<ProceduralOpening>();
        private List<ProceduralSpace> allOutsideSpaces = new List<ProceduralSpace>();
        private DataModel.FloorMapLevel levelData;

        /// <summary>
        /// Gets or sets the level data for the floor.
        /// </summary>
        public FloorMapLevel LevelData { get => levelData; set => levelData = value; }

        /// <summary>
        /// Clears all current room spaces, openings, and outside spaces, and resets the containers.
        /// </summary>
        public void Clear()
        {
            allOpenings = new List<ProceduralOpening>();
            allRoomSpaces = new List<ProceduralSpace>();
            allOutsideSpaces = new List<ProceduralSpace>();

            roomsContainer.ClearChildren();
            openingsContainer.ClearChildren();

            if (building != null)
            {
                building.Clear(clearScene: true);
            }

            if (roof != null)
            {
                roof.Clear(clearScene: true);
            }
        }

        /// <summary>
        /// Converts a list of normalized positions to their corresponding world positions.
        /// </summary>
        /// <param name="normalizedPositions">A list of normalized vector positions.</param>
        /// <returns>A list of world positions corresponding to the normalized positions.</returns>
        public List<Vector3> GetPointsWorldPositionList(List<Vector3> normalizedPositions)
        {
            Grid grid = GetGrid();
            List<Vector3> list = normalizedPositions.Select(s => grid.GetWorldPosition(s)).ToList();
            return list;
        }

        /// <summary>
        /// Builds a procedural opening at the specified normalized position and direction.
        /// </summary>
        /// <param name="normPos">The normalized position for the opening.</param>
        /// <param name="direction">The direction in which the opening is oriented.</param>
        /// <param name="go">The generic opening data used to define the opening properties.</param>
        /// <returns>The created ProceduralOpening object.</returns>
        public ProceduralOpening BuildOpening(Vector3 normPos, Vector3 direction, GenericOpening go)
        {
            GameObject inst = Instantiate(proceduralOpeningPrefab, openingsContainer);

            ProceduralOpening proceduralOpening = inst.GetComponent<ProceduralOpening>();
            Vector3 p = go.worldPos + direction * go.width * 0.5f;
            p.y = 0;
            inst.transform.localPosition = p;
            inst.transform.localRotation = Quaternion.Euler(0, 90, 0) * Quaternion.LookRotation(direction, Vector3.up);
            proceduralOpening.Generate(go, AppController.Instance.wallsHeight, AppController.Instance.windowsThickness);
            return proceduralOpening;
        }

        /// <summary>
        /// Builds an outside space based on the provided FloorMapItem data.
        /// </summary>
        /// <param name="outsideSpace">The data related to the outside space.</param>
        /// <returns>The created ProceduralOutside object, or null if the space was invalid.</returns>
        public ProceduralOutside BuildOusideSpace(FloorMapItem outsideSpace)
        {
            HDLogger.Log("Build Outside space", HDLogger.LogCategory.Floormap);

            List<Vector3> worldPosList = GetPointsWorldPositionList(outsideSpace.normalizedPositions);

            if (worldPosList.Count < 3 || MathUtils.PointsAreInLine(worldPosList))
                return null;

            GameObject inst = Instantiate(proceduralOutsidePrefab, roomsContainer);

            ProceduralOutside proc = inst.GetComponent<ProceduralOutside>();
            proc.SpaceVertexColor = Color.white;
            proc.addMeshColliders = true;
            proc.Generate(worldPosList);
            return proc;
        }

        /// <summary>
        /// Builds a procedural room based on the provided FloorMapItem data and its openings.
        /// </summary>
        /// <param name="room">The data related to the room.</param>
        /// <param name="openings">The list of openings associated with the room.</param>
        /// <returns>The created ProceduralRoom object, or null if the room was invalid.</returns>
        public ProceduralRoom BuildRoom(FloorMapItem room, List<ProceduralRoom.GenericOpening> openings)
        {
            HDLogger.Log("Build Room", HDLogger.LogCategory.Floormap);

            List<Vector3> worldPosList = GetPointsWorldPositionList(room.normalizedPositions);

            if (worldPosList.Count < 3 || MathUtils.PointsAreInLine(worldPosList))
                return null;

            GameObject inst = Instantiate(proceduralRoomPrefab, roomsContainer);

            ProceduralRoom proc = inst.GetComponent<ProceduralRoom>();
            proc.Openings = openings;
            proc.SpaceVertexColor = Color.white;
            proc.addMeshColliders = true;
            proc.Generate(worldPosList);
            return proc;
        }

        /// <summary>
        /// Builds the floor based on the provided FloorMapLevel data and optional rendering of the roof.
        /// </summary>
        /// <param name="floorData">The FloorMapLevel data containing information on spaces.</param>
        /// <param name="renderRoof">Boolean indicating whether to render the roof.</param>
        public void BuildFloor(DataModel.FloorMapLevel floorData, bool renderRoof)
        {
            if (floorData.spaces == null || floorData.spaces.Count == 0)
            {
                HDLogger.LogWarning("[FloorController] BuildFloor Error:No spaces", HDLogger.LogCategory.Building);
                return;
            }
            levelData = floorData;
            HDLogger.Log("[FloorController] BuildFloor spaces:" + floorData.spaces.Count + " renderRoof:" + renderRoof, HDLogger.LogCategory.Building);

            List<FloorMapItem> openingSpaces = floorData.GetAllOpeningSpaces();
            List<FloorMapItem> roomSpaces = floorData.GetRoomSpaces();
            List<FloorMapItem> outsideSpaces = floorData.GetOutsideSpaces();

            List<ProceduralRoom.GenericOpening> openings = new List<ProceduralRoom.GenericOpening>();

            foreach (FloorMapItem openingItem in openingSpaces)
            {
                ProceduralRoom.GenericOpening.OpeningType oType = ProceduralRoom.GenericOpening.OpeningType.Opening;
                Enum.TryParse<ProceduralRoom.GenericOpening.OpeningType>(openingItem.type, out oType);

                List<Vector3> pointList = GetPointsWorldPositionList(openingItem.normalizedPositions);
                for (int j = 0; j < pointList.Count; j++)
                {
                    ProceduralRoom.GenericOpening go = new ProceduralRoom.GenericOpening(oType, pointList[j], openingItem);
                    openings.Add(go);
                    ProceduralOpening pr = null;
                    if (openingItem.directions != null)
                        pr = BuildOpening(openingItem.normalizedPositions[j], openingItem.directions[j], go);

                    if (pr != null)
                        allOpenings.Add(pr);
                }
            }

            foreach (FloorMapItem outsideItem in outsideSpaces)
            {
                ProceduralOutside po = BuildOusideSpace(outsideItem);
                if (po != null) allOutsideSpaces.Add(po);
            }
            foreach (FloorMapItem roomItem in roomSpaces)
            {
                ProceduralRoom pr = BuildRoom(roomItem, openings);
                if (pr != null) allRoomSpaces.Add(pr);
            }

            if (building != null)
            {
                building.Openings = openings;
                building.Generate(allRoomSpaces);

                if (roof != null && building.BuildingContoursOuter != null)
                {
                    roof.GenerateFromWalls(building.BuildingContoursOuter,
                        building.RoomsContours,
                        !renderRoof);

                    roof.Render();
                }
            }
        }

        /// <summary>
        /// Retrieves the Grid component in the current scene.
        /// </summary>
        /// <returns>The Grid component used for positioning.</returns>
        public Grid GetGrid()
        {
            return GameObject.FindObjectOfType<Grid>();
        }

        /// <summary>
        /// Retrieves all SpaceMaterialController components associated with the constructed spaces.
        /// </summary>
        /// <returns>A list of SpaceMaterialController components.</returns>
        public List<SpaceMaterialController> GetSpaceMaterialControllers()
        {
            List<SpaceMaterialController> list = new List<SpaceMaterialController>();
            list.AddRange(allRoomSpaces.Select(s => s.GetComponent<SpaceMaterialController>()));
            list.AddRange(allOutsideSpaces.Select(s => s.GetComponent<SpaceMaterialController>()));
            HDLogger.Log("GetRoomMaterialControllers list " + list.Count, HDLogger.LogCategory.Interior);
            return list;
        }

        /// <summary>
        /// Gets the BuildingMaterialController associated with the floor controller.
        /// </summary>
        /// <returns>The BuildingMaterialController, if available.</returns>
        internal BuildingMaterialController GetBuildingMaterialController()
        {
            HDLogger.Log("GetBuildingMaterialController:" + buildingMaterialController, HDLogger.LogCategory.Building);
            return buildingMaterialController;
        }
    }
}
