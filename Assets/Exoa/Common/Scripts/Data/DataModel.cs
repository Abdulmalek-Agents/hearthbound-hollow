
using Exoa.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Exoa.Designer
{
    /// <summary>
    /// Represents the overall data model for building design including floor maps and interior projects.
    /// </summary>
    public class DataModel
    {

        #region FLOOR MAP PROJECT

        public enum FloorMapItemType { Room, Door, Window, Opening, Outside };

        /// <summary>
        /// Data structure to represent floor maps with multiple floors (version 2).
        /// </summary>
        [System.Serializable]
        public struct FloorMapV2
        {
            public string version;
            public List<FloorMapLevel> floors;
            public BuildingSettings settings;

            /// <summary>
            /// Retrieves a floor map level by its unique identifier.
            /// </summary>
            /// <param name="floorUniqueId">The unique identifier of the floor map level.</param>
            /// <returns>The FloorMapLevel with the specified uniqueId.</returns>
            public FloorMapLevel GetFloorMapLevelByUniqueId(string floorUniqueId)
            {
                return floors.Find(s => s.uniqueId == floorUniqueId);
            }

            /// <summary>
            /// Internal method to find a specific level (not implemented).
            /// </summary>
            /// <returns></returns>
            internal FloorMapLevel Find()
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Data structure for single floor support for floor maps (version 1).
        /// </summary>
        [System.Serializable]
        public struct FloorMapV1
        {
            public List<FloorMapItem> spaces;
            public BuildingSettings settings;

        }

        /// <summary>
        /// Data structure representing a level of a floor map.
        /// </summary>
        [System.Serializable]
        public struct FloorMapLevel
        {
            public string uniqueId;
            public List<FloorMapItem> spaces;

            /// <summary>
            /// Constructor to initialize a FloorMapLevel with a unique identifier.
            /// </summary>
            /// <param name="uniqueId">The unique identifier for the level.</param>
            public FloorMapLevel(string uniqueId) : this()
            {
                this.uniqueId = uniqueId;
                spaces = new List<FloorMapItem>();
                GenerateUniqueId();
            }

            /// <summary>
            /// Generates a unique identifier if none exists.
            /// </summary>
            /// <returns>The unique identifier for the level.</returns>
            public string GenerateUniqueId()
            {
                if (string.IsNullOrEmpty(uniqueId))
                {
                    uniqueId = System.Guid.NewGuid().ToString();
                }
                return uniqueId;
            }

            /// <summary>
            /// Retrieves all room spaces from the level.
            /// </summary>
            /// <returns>A list of FloorMapItems that are classified as rooms.</returns>
            public List<FloorMapItem> GetRoomSpaces()
            {
                return spaces.Where(s => s.GetItemType() == FloorMapItemType.Room).ToList();
            }

            /// <summary>
            /// Retrieves all outside spaces from the level.
            /// </summary>
            /// <returns>A list of FloorMapItems that are classified as outside spaces.</returns>
            public List<FloorMapItem> GetOutsideSpaces()
            {
                return spaces.Where(s => s.GetItemType() == FloorMapItemType.Outside).ToList();
            }

            /// <summary>
            /// Retrieves all opening spaces (windows, doors, openings) from the level.
            /// </summary>
            /// <returns>A list of FloorMapItems that are classified as openings.</returns>
            public List<FloorMapItem> GetAllOpeningSpaces()
            {
                //Debug.Log("spaces:" + spaces);
                //Debug.Log("spaces:" + spaces.Count);
                return spaces.Where(s => s.GetItemType() == FloorMapItemType.Window ||
                s.GetItemType() == FloorMapItemType.Door || s.GetItemType() == FloorMapItemType.Opening).ToList();
            }
        }

        /// <summary>
        /// Data structure containing building configuration settings.
        /// </summary>
        [System.Serializable]
        public struct BuildingSettings
        {
            public float wallsHeight;
            public float doorsHeight;
            public float interiorWallThickness;
            public float exteriorWallThickness;
            public float windowsThickness;
            public float doorsThickness;
            public float roofThickness;
            public float roofOverhang;
            public int roofType;
        }

        /// <summary>
        /// Data structure representing an item in the floor map.
        /// </summary>
        [System.Serializable]
        public struct FloorMapItem
        {
            public string type;
            public string name;
            public float width;
            public float height;
            public float ypos;

            public bool hasWindow;
            public float windowFrameSize;
            public float windowSizeH;
            public float windowSizeV;
            public int windowSubDivH;
            public int windowSubDivV;

            public List<Vector3> normalizedPositions;
            public List<Vector3> directions;

            /// <summary>
            /// Determines the type of item (Room, Door, Window, etc.) based on its properties.
            /// </summary>
            /// <param name="go">A GameObject reference to assist in identifying the type.</param>
            /// <returns>The identified FloorMapItemType.</returns>
            public FloorMapItemType GetItemType(GameObject go = null)
            {
                DataModel.FloorMapItemType t = DataModel.FloorMapItemType.Room;
                if (string.IsNullOrEmpty(type) && go != null)
                {
                    if (go.name.ToLower().Contains("window"))
                        t = FloorMapItemType.Window;
                    if (go.name.ToLower().Contains("door"))
                        t = FloorMapItemType.Door;

                }
                else
                {
                    DataModel.FloorMapItemType.TryParse(type, out t);
                }

                return t;
            }
        }
        #endregion

        #region INTERIOR PROJECT

        /// <summary>
        /// Data structure for interior projects with multiple floors (version 2).
        /// </summary>
        [System.Serializable]
        public class InteriorProjectV2
        {
            public string version;
            public string title;
            public string floorMapFile;
            public List<InteriorLevel> floors;
            public BuildingSetting settings;

            /// <summary>
            /// Retrieves an interior level by its unique identifier.
            /// </summary>
            /// <param name="floorUniqueId">The unique identifier of the interior level.</param>
            /// <returns>The InteriorLevel with the specified uniqueId.</returns>
            public InteriorLevel GetInteriorLevelByUniqueId(string floorUniqueId)
            {
                return floors.Find(s => s.floorUniqueId == floorUniqueId);
            }
        }

        /// <summary>
        /// Data structure for the interior project file version 1.
        /// </summary>
        [System.Serializable]
        public class InteriorProjectV1
        {
            public string title;
            public string floorMapFile;

            public List<SceneObject> sceneObjects;
            public List<RoomSetting> roomSettings;
            public BuildingSetting buildingSetting;

        }

        /// <summary>
        /// Data structure representing a level in an interior project.
        /// </summary>
        [System.Serializable]
        public struct InteriorLevel
        {
            public string floorUniqueId;
            public List<SceneObject> sceneObjects;
            public List<RoomSetting> roomSettings;

        }

        /// <summary>
        /// Data structure representing an object in a scene.
        /// </summary>
        [System.Serializable]
        public struct SceneObject
        {
            public string prefabName;
            public string materialVariantName;
            public Color colorVariant;
            public Vector3 position;
            public Vector3 rotation;
            public Vector3 scale;
        }

        /// <summary>
        /// Data structure containing individual settings for a room.
        /// </summary>
        [System.Serializable]
        public struct RoomSetting
        {
            public int floorMapItemId;

            // Kept for retro compatibility
            public string wall;
            public string floor;
            public string ceiling;

            // New implementation
            public List<TextureSetting> separateWallTextureSettings;
            public TextureSetting floorTextureSetting;
            public TextureSetting ceilingTextureSetting;
        }

        /// <summary>
        /// Data structure representing texture settings for a material.
        /// </summary>
        [System.Serializable]
        public struct TextureSetting
        {
            public string materialName;
            public float tiling;

            /// <summary>
            /// Constructor to create a TextureSetting with a given material name and tiling.
            /// </summary>
            /// <param name="name">The name of the material.</param>
            /// <param name="tiling">The tiling value for the material.</param>
            public TextureSetting(string name, float tiling) : this()
            {
                this.materialName = name;
                this.tiling = tiling;
            }
        }

        /// <summary>
        /// Data structure for building settings.
        /// </summary>
        [System.Serializable]
        public struct BuildingSetting
        {
            // Kept for retro compatibility
            public string exteriorWallMat;
            public string roofMat;

            // New implementation
            public TextureSetting roofTextureSetting;
            public TextureSetting exteriorWallTextureSetting;
        }

        /// <summary>
        /// Class representing a category of interior items.
        /// </summary>
        [System.Serializable]
        public class InteriorCategory
        {
            [SerializeField] public string folder;
            [SerializeField] public string name;

            /// <summary>
            /// Constructor to create a new InteriorCategory.
            /// </summary>
            /// <param name="name">The name of the category.</param>
            /// <param name="folder">The folder associated with the category.</param>
            public InteriorCategory(string name, string folder)
            {
                this.name = name;
                this.folder = folder;
            }
        }

        /// <summary>
        /// Data structure that holds a list of interior categories.
        /// </summary>
        [System.Serializable]
        public struct InteriorCategories
        {
            public List<InteriorCategory> folders;
        }

        #endregion

        #region CONVERTERS

        /// <summary>
        /// Deserialize a JSON string into an InteriorProjectV2 object.
        /// </summary>
        /// <param name="str">The JSON formatted string representing the interior project.</param>
        /// <returns>Deserialized InteriorProjectV2 object.</returns>
        public static InteriorProjectV2 DeserializeInteriorJsonFile(string str)
        {
            if (string.IsNullOrEmpty(str))
                return default;

            InteriorProjectV2 fileV2 = JsonConvert.DeserializeObject<InteriorProjectV2>(str);
            if (fileV2.floors != null && fileV2.floors.Count > 0)
            {
                // file has a v2 format
            }
            else
            {
                // Fallback to V1

                InteriorProjectV1 fileV1 = JsonConvert.DeserializeObject<InteriorProjectV1>(str);

                // Converting to V2
                InteriorLevel floor = new InteriorLevel();
                floor.sceneObjects = fileV1.sceneObjects;
                floor.roomSettings = fileV1.roomSettings;
                //floor.floorUniqueId = fileV1.floorUniqueId;

                fileV2 = new InteriorProjectV2();
                fileV2.version = "v2";
                fileV2.title = fileV1.title;
                fileV2.floorMapFile = fileV1.floorMapFile;
                fileV2.settings = fileV1.buildingSetting;
                fileV2.floors = new System.Collections.Generic.List<InteriorLevel>();
                fileV2.floors.Add(floor);
            }
            return fileV2;
        }

        /// <summary>
        /// Deserialize a JSON string into a FloorMapV2 object.
        /// </summary>
        /// <param name="str">The JSON formatted string representing the floor map.</param>
        /// <returns>Deserialized FloorMapV2 object.</returns>
        public static FloorMapV2 DeserializeFloorMapJsonFile(string str)
        {
            if (string.IsNullOrEmpty(str))
                return default;

            FloorMapV2 fileV2 = JsonConvert.DeserializeObject<FloorMapV2>(str);
            if (fileV2.floors != null && fileV2.floors.Count > 0)
            {
                // file has a v2 format
            }
            else
            {

                // Fallback to V1

                FloorMapV1 fileV1 = JsonConvert.DeserializeObject<FloorMapV1>(str);

                // Converting to V2
                FloorMapLevel floor = new FloorMapLevel(null);
                floor.spaces = fileV1.spaces;

                fileV2 = new FloorMapV2();
                fileV2.version = "v2";
                fileV2.settings = fileV1.settings;
                fileV2.floors = new System.Collections.Generic.List<FloorMapLevel>();
                fileV2.floors.Add(floor);

            }
            // removing empty floors
            for (int i = 0; i < fileV2.floors.Count; i++)
            {
                if (fileV2.floors[i].uniqueId == null && fileV2.floors[i].spaces == null)
                {
                    fileV2.floors.RemoveAt(i);
                    i--;
                }
            }

            return fileV2;
        }
        #endregion

    }
}
