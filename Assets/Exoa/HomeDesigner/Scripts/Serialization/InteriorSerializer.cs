
using Exoa.Designer.Utils;
using Exoa.Events;
using Exoa.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Exoa.Designer.DataModel;
using static Exoa.Events.GameEditorEvents;

namespace Exoa.Designer
{
    /// <summary>
    /// Responsible for serializing and deserializing interior designs, handling events related to file operations,
    /// and managing the user interface related to floor maps and interior projects.
    /// </summary>
    public class InteriorSerializer : BaseSerializable, IDataSerializer
    {
        private Transform modulesContainer;
        private Transform globalContainer;
        public TabMenu tabMenu;
        private InteriorProjectV2 currentInteriorFile;
        private FloorMapV2 currentFloorMapFile;
        private bool initialized;
        private UIFloorsMenu floorsMenu;
        private FloorMapReader floorMapReader;

        /// <summary>
        /// Gets the folder name for interior files.
        /// </summary>
        /// <returns>The folder name.</returns>
        override public string GetFolderName() => HDSettings.EXT_INTERIOR_FOLDER;

        /// <summary>
        /// Gets the file type associated with the interior serializer.
        /// </summary>
        /// <returns>The file type.</returns>
        override public GameEditorEvents.FileType GetFileType() => FileType.InteriorFile;

        /// <summary>
        /// Unsubscribes from events when the object is destroyed.
        /// </summary>
        void OnDestroy()
        {
            GameEditorEvents.OnFileSaved -= OnFileSaved;
            GameEditorEvents.OnFileLoaded -= OnFileLoaded;
            GameEditorEvents.OnRequestClearAll -= Clear;
            GameEditorEvents.OnRequestFloorAction -= OnRequestFloorActionHandler;
        }

        /// <summary>
        /// Initializes the interior serializer on start.
        /// </summary>
        void Start()
        {
            Init();
        }

        /// <summary>
        /// Initializes various components and sets up event listeners for file operations.
        /// </summary>
        void Init()
        {
            if (initialized) return;
            initialized = true;
            floorMapReader = GetComponent<FloorMapReader>();
            floorsMenu = GameObject.FindObjectOfType<UIFloorsMenu>();
            modulesContainer = GameObject.Find("ModulesContainer").transform;
            globalContainer = transform;

            Clear(clearScene: true);

            GameEditorEvents.OnFileSaved += OnFileSaved;
            GameEditorEvents.OnFileLoaded += OnFileLoaded;
            GameEditorEvents.OnRequestClearAll += Clear;
            GameEditorEvents.OnRequestFloorAction += OnRequestFloorActionHandler;
        }

        /// <summary>
        /// Handles floor action requests from the event system.
        /// </summary>
        /// <param name="action">The action to perform on the floor.</param>
        /// <param name="floorId">The ID of the floor to act upon.</param>
        private void OnRequestFloorActionHandler(FloorAction action, string floorId)
        {
            switch (action)
            {
                case FloorAction.Select: 
                    SelectFloor(floorId); 
                    break;
            }
        }

        /// <summary>
        /// Called when a file is loaded. It processes loaded data for the interior design.
        /// </summary>
        /// <param name="fileType">The type of the file that was loaded.</param>
        private void OnFileLoaded(FileType fileType)
        {
            /* HDLogger.Log("Interior OnFileLoaded type:" + fileType, HDLogger.LogCategory.Interior);

             if (fileType != FileType.InteriorFile)
                 return;

             BuildingMaterialController bmc = GameObject.FindObjectOfType<BuildingMaterialController>();
             if (bmc != null && currentInteriorFile != null)
             {
                 bmc.SetBuildingSetting(currentInteriorFile.settings);
             }
             if (currentInteriorFile != null && currentInteriorFile.floors != null && currentInteriorFile.floors.Count > 0)
             {
                 RoomMaterialController[] rmcs = GameObject.FindObjectsOfType<RoomMaterialController>();
                 for (int i = 0; i < rmcs.Length; i++)
                 {
                     if (currentInteriorFile != null && currentInteriorFile.floors[0].roomSettings.Count > i)
                         rmcs[i].SetRoomSetting(currentInteriorFile.floors[0].roomSettings[i]);
                 }
             }*/
        }

        /// <summary>
        /// Called when a file is saved. It creates and saves a thumbnail for the interior design.
        /// </summary>
        /// <param name="fileName">The name of the file that was saved.</param>
        /// <param name="fileType">The type of the file that was saved.</param>
        private void OnFileSaved(string fileName, FileType fileType)
        {
            if (fileType != FileType.InteriorFile)
                return;

            string perspViewName = fileName.Replace(".json", "") + "_persp";
            ThumbnailGeneratorUtils.TakeAndSaveScreenshot(globalContainer, HDSettings.INTERIOR_DESIGNER_THUMBNAIL_PREFIX + perspViewName, false, new Vector3(1, -1, 1));
            GameEditorEvents.OnScreenShotSaved?.Invoke(perspViewName, MenuType.InteriorMenu);
        }

        /// <summary>
        /// Clears the scene, optionally clearing the floors UI and floor map UI.
        /// </summary>
        /// <param name="clearFloorsUI">Whether to clear the floors UI.</param>
        /// <param name="clearFloorMapUI">Whether to clear the floor map UI.</param>
        /// <param name="clearScene">Whether to clear the scene.</param>
        public void Clear(bool clearFloorsUI = false, bool clearFloorMapUI = false, bool clearScene = false)
        {
            if (!clearScene) return;

            HDLogger.Log("Interior Clear", HDLogger.LogCategory.Interior);
            modulesContainer.ClearChildren();
        }

        /// <summary>
        /// Deserializes a JSON string and creates corresponding scene objects in Unity.
        /// </summary>
        /// <param name="str">The JSON data representing the interior design.</param>
        /// <returns>The deserialized object.</returns>
        override public object DeserializeToScene(string str)
        {
            Init();

            HDLogger.Log("DeserializeToScene", HDLogger.LogCategory.Interior);

            currentInteriorFile = DataModel.DeserializeInteriorJsonFile(str);
            //InteriorDesigner.instance.FloorMapFileName = currentInteriorFile.floorMapFile;

            SaveSystem externalFolderSS = SaveSystem.Create(SaveSystem.Mode.FILE_SYSTEM);
            SaveSystem internalFolderSS = SaveSystem.Create(SaveSystem.Mode.RESOURCES);


            externalFolderSS.Exists(currentInteriorFile.floorMapFile, HDSettings.EXT_FLOORMAP_FOLDER, ".json", (bool exists) =>
            {
                if (exists)
                {
                    externalFolderSS.LoadFileItem(currentInteriorFile.floorMapFile,
                        HDSettings.EXT_FLOORMAP_FOLDER, (string json) =>
                        {
                            currentFloorMapFile = DataModel.DeserializeFloorMapJsonFile(json);
                            DeserializeProjectUI(currentInteriorFile, currentFloorMapFile);
                        });
                }
                else
                {
                    internalFolderSS.Exists(currentInteriorFile.floorMapFile,
                        HDSettings.EMBEDDED_FLOORMAP_FOLDER, ".json", (bool exists2) =>
                        {
                            if (exists2)
                            {
                                internalFolderSS.LoadFileItem(currentInteriorFile.floorMapFile, HDSettings.EMBEDDED_FLOORMAP_FOLDER, (string json) =>
                                {
                                    currentFloorMapFile = DataModel.DeserializeFloorMapJsonFile(json);
                                    DeserializeProjectUI(currentInteriorFile, currentFloorMapFile);
                                });
                            }
                            else
                            {
                                AlertPopup.ShowAlert("fileNotFound", "Error", "The floor map file could not be found");
                            }
                        });
                }
            });




            // External and internal file system handling implementation here...

            return currentInteriorFile;
        }

        /// <summary>
        /// Deserializes the user interface elements of the project when a floor map is loaded.
        /// </summary>
        /// <param name="interiorFile">The loaded interior project file.</param>
        /// <param name="floorMapFile">The loaded floor map file.</param>
        private void DeserializeProjectUI(InteriorProjectV2 interiorFile, FloorMapV2 floorMapFile)
        {
            if (interiorFile.floors != null && interiorFile.floors.Count > 0 && !string.IsNullOrEmpty(interiorFile.floorMapFile))
            {
                DeserializeFloorMapProjectUI(floorMapFile);
            }
        }

        /// <summary>
        /// Deserializes the contents of an interior level and initializes it within the scene.
        /// </summary>
        /// <param name="level">The interior level to deserialize.</param>
        /// <param name="fc">The floor controller managing the floor's components.</param>
        void DeserializeInterior(InteriorLevel level, FloorController fc)
        {
            HDLogger.Log("DeserializeInteriorUI", HDLogger.LogCategory.Interior);

            List<SceneObject> sceneObjects = new List<SceneObject>();
            GameObject[] prefabs = Resources.LoadAll<GameObject>(HDSettings.MODULES_FOLDER);
            InteriorDesigner lc = GameObject.FindObjectOfType<InteriorDesigner>();

            if (level.sceneObjects != null)
            {
                foreach (SceneObject so in level.sceneObjects)
                {
                    GameObject prefab = TabMenu.FindModuleByName(so.prefabName);
                    //Debug.Log("so.prefabName:" + so.prefabName + " prefab:" + prefab);
                    if (prefab != null)
                    {
                        lc.currentPrefab = prefab;
                        lc.currentPrefabOptions = prefab.GetComponent<ModuleController>();
                        lc.CreateObj(so, true, false);
                    }
                    else
                    {
                        Debug.LogError("Could not find module " + so.prefabName);
                    }
                }
            }

            lc.DeleteGhost();


            // Set materials 
            BuildingMaterialController bmc = fc.GetBuildingMaterialController();
            if (bmc != null && currentInteriorFile != null)
            {
                bmc.SetBuildingSetting(currentInteriorFile.settings);
            }
            if (currentInteriorFile != null && currentInteriorFile.floors != null && currentInteriorFile.floors.Count > 0)
            {
                List<SpaceMaterialController> rmcs = fc.GetSpaceMaterialControllers();
                for (int i = 0; i < rmcs.Count; i++)
                {
                    if (rmcs[i] != null && currentInteriorFile != null && level.roomSettings != null && level.roomSettings.Count > i)
                        rmcs[i].SetRoomSetting(level.roomSettings[i]);
                }
            }
        }

        /// <summary>
        /// Deserializes the UI for the floor map's project.
        /// </summary>
        /// <param name="floorMapProject">The floor map project to deserialize.</param>
        private void DeserializeFloorMapProjectUI(FloorMapV2 floorMapProject)
        {
            if (floorMapProject.floors == null)
                return;

            // Filling settings menu
            if (floorMapProject.settings.wallsHeight != 0)
            {
                AppController.Instance.SetFloorMapSettings(floorMapProject.settings);
            }


            // Showing all floors in the Floors menu
            for (int i = 0; i < floorMapProject.floors.Count; i++)
            {
                FloorMapLevel floor = floorMapProject.floors[i];
                floor.GenerateUniqueId();
                floorMapProject.floors[i] = floor;
                floorsMenu.CreateNewUIItem(floorMapProject.floors[i]);
            }
            //setting the first level as current
            if (floorMapProject.floors != null && floorMapProject.floors.Count > 0)
            {
                SelectFloor(0);
            }


            // Implementation for setting UI elements related to the floor map project.
        }

        /// <summary>
        /// Selects a floor by its index and updates the UI and associated components.
        /// </summary>
        /// <param name="floorIndex">The index of the floor to select.</param>
        private void SelectFloor(int floorIndex)
        {
            floorsMenu.CurrentFloorId = currentFloorMapFile.floors[floorIndex].uniqueId;
            List<FloorController> fcs = (List<FloorController>)floorMapReader.DeserializeToScene(currentFloorMapFile, floorIndex);
            DeserializeInterior(currentInteriorFile.GetInteriorLevelByUniqueId(floorsMenu.CurrentFloorId), fcs[0]);
        }

        /// <summary>
        /// Selects a floor by its unique ID and updates the UI and associated components.
        /// </summary>
        /// <param name="floorId">The unique ID of the floor to select.</param>
        private void SelectFloor(string floorId)
        {
            floorsMenu.CurrentFloorId = floorId;
            List<FloorController> fcs = (List<FloorController>)floorMapReader.DeserializeToScene(currentFloorMapFile, floorId);
            DeserializeInterior(currentInteriorFile.GetInteriorLevelByUniqueId(floorId), fcs[0]);
        }

        #region SERIALIZATION

        /// <summary>
        /// Serializes the current scene's interior design to a JSON string.
        /// </summary>
        /// <returns>The serialized JSON string of the interior scene.</returns>
        override public string SerializeScene()
        {
            HDLogger.Log("[InteriorSerializer] SerializeScene", HDLogger.LogCategory.Interior);

            if (currentInteriorFile == null)
                return "";

            // Saving the current interior
            int foundIndex = -1;
            if (currentInteriorFile.floors != null && currentInteriorFile.floors.Count > 0)
            {
                for (int i = 0; i < currentInteriorFile.floors.Count; i++)
                {
                    //print("currentInteriorFile.floors[i].floorUniqueId:" + currentInteriorFile.floors[i].floorUniqueId);

                    if (currentInteriorFile.floors[i].floorUniqueId == floorsMenu.CurrentFloorId ||
                        currentInteriorFile.floors[i].floorUniqueId == null)
                    {
                        foundIndex = i;
                    }
                }
            }
            // Get the floor to save
            FloorController fc = GetComponentInChildren<FloorController>();

            if (fc == null)
            {
                HDLogger.LogError("Could not find a floor to save", HDLogger.LogCategory.General);
                return null;
            }
            InteriorLevel level = new InteriorLevel();
            level.floorUniqueId = floorsMenu.CurrentFloorId;
            List<SceneObject> sol = new List<SceneObject>();

            for (int j = 0; j < modulesContainer.transform.childCount; j++)
            {
                SceneObject so = new SceneObject();
                Transform t = modulesContainer.transform.GetChild(j);
                ModuleColorVariants variant = t.gameObject.GetComponentInChildren<ModuleColorVariants>();
                if (variant != null)
                {
                    //print("variant.SelectedColor:" + variant.SelectedColor);
                    //print("variant.SelectedMaterialName:" + variant.SelectedMaterialName);

                    so.colorVariant = variant.SelectedColor;
                    so.materialVariantName = variant.SelectedMaterialName;
                }
                so.position = t.localPosition;
                so.rotation = t.eulerAngles - t.GetComponent<ModuleController>().initRotation;
                so.scale = t.localScale;
                so.prefabName = t.gameObject.name;
                sol.Add(so);
            }
            level.sceneObjects = sol;
            level.roomSettings = new List<RoomSetting>();
            List<SpaceMaterialController> rmcs = fc.GetSpaceMaterialControllers();
            for (int j = 0; j < rmcs.Count; j++)
            {
                level.roomSettings.Add((RoomSetting)rmcs[j].GetRoomSetting());
            }
            if (foundIndex > -1)
                currentInteriorFile.floors[foundIndex] = level;
            else currentInteriorFile.floors.Add(level);


            BuildingMaterialController bmc = fc.GetBuildingMaterialController();
            if (bmc != null)
                currentInteriorFile.settings = (BuildingSetting)bmc.GetBuildingSetting();

            return JsonConvert.SerializeObject(currentInteriorFile, Formatting.Indented, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        /// <summary>
        /// Checks if the current scene is empty (i.e., no child modules exist).
        /// </summary>
        /// <returns>True if the scene is empty; otherwise, false.</returns>
        override public bool IsSceneEmpty()
        {
            return modulesContainer.childCount == 0;
        }

        /// <summary>
        /// Serializes an empty interior project to a JSON string.
        /// </summary>
        /// <returns>The serialized JSON string of an empty interior project.</returns>
        public string SerializeEmpty()
        {
            InteriorProjectV2 project = new InteriorProjectV2();
            project.version = "v2";
            project.settings = new BuildingSetting();
            project.floors = new List<InteriorLevel>();
            project.floors.Add(new InteriorLevel());
            return JsonConvert.SerializeObject(project, Formatting.Indented, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        /// <summary>
        /// Serializes an empty interior project with a specified floor map file name.
        /// </summary>
        /// <param name="floorMapFileName">The name of the floor map file to associate with the empty project.</param>
        /// <returns>The serialized JSON string of the empty project with a specified floor map name.</returns>
        override public string SerializeEmpty(string floorMapFileName)
        {
            InteriorProjectV2 project = new InteriorProjectV2();
            project.version = "v2";
            project.settings = new BuildingSetting();
            project.floorMapFile = floorMapFileName;
            project.floors = new List<InteriorLevel>();
            project.floors.Add(new InteriorLevel());
            return JsonConvert.SerializeObject(project, Formatting.Indented, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        #endregion
    }
}
