
using Exoa.Designer.Utils;
using Exoa.Events;
using System.Collections.Generic;
using UnityEngine;
using static Exoa.Designer.DataModel;
using static Exoa.Events.GameEditorEvents;

namespace Exoa.Designer
{
    /// <summary>
    /// Class responsible for reading and managing floor maps within the application.
    /// Implements the IDataReader interface for loading and processing floor map data.
    /// </summary>
    public class FloorMapReader : BaseReader, IDataReader
    {
        private bool floorMapLoaded;
        private List<FloorController> floorsList;
        private FloorMapV2 currentFloorMap;
        private AppController appController;

        /*
        public string FileName
        {
#if INTERIOR_MODULE
            get
            {
                return (InteriorDesigner.instance != null) ? InteriorDesigner.instance.FloorMapFileName : null;
            }

            set
            {
                if (InteriorDesigner.instance != null) InteriorDesigner.instance.FloorMapFileName = value;
            }
#else
            set { }
            get { return null; }
#endif
        }
        */

        /// <summary>
        /// Clean up resources and unsubscribe from events when the object is destroyed.
        /// </summary>
        void OnDestroy()
        {
            GameEditorEvents.OnRequestClearAll -= OnRequestClearAll;
        }

        /// <summary>
        /// Initialize the FloorMapReader and set the AppController instance.
        /// Subscribe to the OnRequestClearAll event if not in PlayMode.
        /// </summary>
        void Awake()
        {
            appController = GetComponent<AppController>();
            if (appController == null)
            {
                appController = GameObject.FindObjectOfType<AppController>();
            }

            if (appController.currentState != AppController.States.PlayMode)
            {
                GameEditorEvents.OnRequestClearAll += OnRequestClearAll;
            }
        }

        /// <summary>
        /// Handles floor action requests based on the provided action and file name.
        /// </summary>
        /// <param name="action">The action to be performed on the floor.</param>
        /// <param name="fileName">The name of the file associated with the action.</param>
        private void OnRequestFloorActionHandler(FloorAction action, string fileName)
        {
            if (action == FloorAction.PreviewBuilding)
            {
                ReplaceAndLoad(fileName);
            }
        }

        /// <summary>
        /// Clears all currently loaded floors and resets the state based on the provided parameters.
        /// </summary>
        /// <param name="clearFloorsUI">Indicates if the floors UI should be cleared.</param>
        /// <param name="clearFloorMapUI">Indicates if the floor map UI should be cleared.</param>
        /// <param name="clearScene">Indicates if the entire scene should be cleared.</param>
        private void OnRequestClearAll(bool clearFloorsUI, bool clearFloorMapUI, bool clearScene)
        {
            if (clearScene)
            {
                HDLogger.Log("Floor Map Reader OnRequestClearAll floorMapLoaded:" + floorMapLoaded, HDLogger.LogCategory.Floormap);
                Clear();
            }
        }

        /// <summary>
        /// Clears the loaded floors and resets the floor map state.
        /// </summary>
        public void Clear()
        {
            if (floorsList != null && floorsList.Count > 0)
            {
                for (int i = 0; i < floorsList.Count; i++)
                {
                    if (floorsList[i] != null)
                    {
                        floorsList[i].gameObject.DestroyUniversal();
                    }
                }
            }
            floorsList = new List<FloorController>();
            floorMapLoaded = false;
        }

        /// <summary>
        /// Gets the folder name where floor maps are stored.
        /// </summary>
        /// <returns>The folder name as a string.</returns>
        override public string GetFolderName()
        {
            return HDSettings.EXT_FLOORMAP_FOLDER;
        }

        /// <summary>
        /// Replaces the currently loaded floor map with a new one and optionally sends an event to indicate completion.
        /// </summary>
        /// <param name="name">The name of the new floor map to load.</param>
        /// <param name="sendLoadedEvent">Indicates if a loaded event should be sent after loading.</param>
        override public void ReplaceAndLoad(string name, bool sendLoadedEvent = true)
        {
            LoadInternal(name, sendLoadedEvent);
        }

        /// <summary>
        /// Loads a floor map from the specified name and invokes an event upon loading.
        /// </summary>
        /// <param name="name">The name of the floor map to load.</param>
        /// <param name="sendLoadedEvent">Indicates if a loaded event should be sent.</param>
        private void LoadInternal(string name, bool sendLoadedEvent = true)
        {
            HDLogger.Log("Floor Map Reader Load:" + name, HDLogger.LogCategory.Building);

            if (string.IsNullOrEmpty(name))
            {
                AlertPopup.ShowAlert("emptyFloorMap", "Empty Floor Map", "The floor map name is empty.");
                return;
            }
            GameEditorEvents.OnRequestClearAll?.Invoke(clearFloorsUI: false, clearFloorMapUI: true, clearScene: true);
            SaveSystem.Create(SaveSystem.Mode.FILE_SYSTEM).LoadFileItem(name, GetFolderName(), (string json) =>
            {
                //FileName = name;
                DeserializeToScene(json);
                floorMapLoaded = true;
                string screenshotName = "Building_" + name + "_persp";
                ThumbnailGeneratorUtils.TakeAndSaveScreenshot(transform, screenshotName, false, new Vector3(1, -1, 1));
                GameEditorEvents.OnScreenShotSaved?.Invoke(screenshotName, MenuType.FloorMapMenu);
                GameEditorEvents.OnFileLoaded?.Invoke(GameEditorEvents.FileType.BuildingRead);
            });
        }

        /// <summary>
        /// Deserializes a JSON string into the current scene based on the provided string.
        /// </summary>
        /// <param name="str">The JSON string representing the floor map.</param>
        /// <returns>An object representing the result of the deserialization.</returns>
        override public object DeserializeToScene(string str)
        {
            currentFloorMap = DataModel.DeserializeFloorMapJsonFile(str);
            return DeserializeToScene(currentFloorMap, -1);
        }

        /// <summary>
        /// Deserializes a specific floor of the floor map into the current scene.
        /// </summary>
        /// <param name="project">The FloorMapV2 object containing floor data.</param>
        /// <param name="onlySpecificFloorId">The unique identifier for a specific floor to deserialize.</param>
        /// <returns>An object representing the result of the deserialization.</returns>
        public object DeserializeToScene(FloorMapV2 project, string onlySpecificFloorId = null)
        {
            if (string.IsNullOrEmpty(onlySpecificFloorId))
            {
                return DeserializeToScene(project, -1);
            }
            else
            {
                for (int i = 0; i < project.floors.Count; i++)
                {
                    if (project.floors[i].uniqueId == onlySpecificFloorId)
                    {
                        return DeserializeToScene(project, i);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Deserializes the specified floor index or all floors into the current scene.
        /// </summary>
        /// <param name="project">The FloorMapV2 object containing floor data.</param>
        /// <param name="onlySpecificFloorIndex">The index of a specific floor to deserialize, or -1 for all.</param>
        /// <returns>An object representing the result of the deserialization.</returns>
        public object DeserializeToScene(FloorMapV2 project, int onlySpecificFloorIndex = -1)
        {
            Clear();

            if (project.floors == null || project.floors.Count == 0)
            {
                AlertPopup.ShowAlert("emptyFloorMap", "Empty Floor Map", "No floor data in file");
                return null;
            }
            // Applying building settings prior to generating it
            AppController.Instance.SetFloorMapSettings(project.settings);
            FloorController floorCtrl = null;
            for (int i = 0; i < project.floors.Count; i++)
            {
                if (onlySpecificFloorIndex > -1 && onlySpecificFloorIndex != i)
                    continue;

                float floorHeight = onlySpecificFloorIndex >= 0 ? 0 : i * project.settings.wallsHeight;
                floorCtrl = CreateFloorContainer(floorHeight);
                FloorMapLevel floorData = project.floors[i];
                bool shouldBuildRoof = i == project.floors.Count - 1 || onlySpecificFloorIndex >= 0;
                floorCtrl.BuildFloor(floorData, shouldBuildRoof);
                floorsList.Add(floorCtrl);

                //print("floor id:" + floorData.uniqueId + " floorHeight:" + floorHeight + " spaces:" + floorData.spaces.Count);

            }
            return floorsList;
        }

        /// <summary>
        /// Unloads the current floor map and related data. Implementation is not done yet.
        /// </summary>
        override public void Unload()
        {
            //throw new NotImplementedException();
        }
    }
}
