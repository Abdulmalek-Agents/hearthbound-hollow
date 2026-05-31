
namespace Exoa.Events
{
    /// <summary>
    /// Contains event-related classes and delegates for the Game Editor.
    /// It provides various enumerations for actions that can occur in the Game Editor.
    /// </summary>
    public class GameEditorEvents
    {
        /// <summary>
        /// Enum representing different actions that can be performed on a floor.
        /// </summary>
        public enum FloorAction
        {
            Select, Add, Remove, Duplicate, PreviewBuilding
        }

        /// <summary>
        /// Enum representing various general actions within the Game Editor.
        /// </summary>
        public enum Action
        {
            Help, ToggleExteriorWalls, ToggleGizmos,
            Exit, NewProject, ToggleRoof, NewFloor,
            ShowExteriorWalls, ShowGizmos, ShowRoof, ExportFBX
        };

        /// <summary>
        /// Enum representing different types of files in the Game Editor.
        /// </summary>
        public enum FileType
        {
            FloorMapFile, InteriorFile, BuildingRead, ScreenshotFile
        };

        /// <summary>
        /// Enum representing various menu types in the Game Editor.
        /// </summary>
        public enum MenuType
        {
            FloorMapMenu, InteriorMenu, FloorsMenu
        };

        /// <summary>
        /// Delegate for handling game editor events that do not return any value.
        /// </summary>
        public delegate void OnGameEditorEventHandler();

        /// <summary>
        /// Delegate for handling events to clear different UI components in the Game Editor.
        /// </summary>
        /// <param name="clearFloorsUI">Indicates whether to clear the Floors UI.</param>
        /// <param name="clearFloorMapUI">Indicates whether to clear the Floor Map UI.</param>
        /// <param name="clearScene">Indicates whether to clear the Scene.</param>
        public delegate void OnClearEventHandler(bool clearFloorsUI, bool clearFloorMapUI, bool clearScene);

        /// <summary>
        /// Delegate for handling events related to game editor menu types based on file type.
        /// </summary>
        /// <param name="fileType">The type of file associated with the menu event.</param>
        public delegate void OnGameEditorMenuTypeEventHandler(FileType fileType);

        /// <summary>
        /// Delegate for handling events that return a boolean value.
        /// </summary>
        /// <param name="success">Indicates whether the event was successful.</param>
        public delegate void OnGameEditorEventBoolHandler(bool success);

        /// <summary>
        /// Delegate for handling events that return a string value.
        /// </summary>
        /// <param name="name">The name associated with the event.</param>
        public delegate void OnGameEditorEventStringHandler(string name);

        /// <summary>
        /// Delegate for handling load events that return a string and a file type.
        /// </summary>
        /// <param name="name">The name associated with the loaded event.</param>
        /// <param name="fileType">The type of file being loaded.</param>
        public delegate void OnLoadEventStringHandler(string name, FileType fileType);

        /// <summary>
        /// Delegate for handling load events related to menu types.
        /// </summary>
        /// <param name="name">The name associated with the menu load event.</param>
        /// <param name="menuType">The type of menu being loaded.</param>
        public delegate void OnLoadEventMenuHandler(string name, MenuType menuType);

        /// <summary>
        /// Delegate for handling load events based on an action and its active state.
        /// </summary>
        /// <param name="action">The action that is being loaded.</param>
        /// <param name="active">Indicates whether the action is active.</param>
        public delegate void OnLoadEventActionHandler(Action action, bool active);

        /// <summary>
        /// Delegate for handling events related to actions performed on a specific floor.
        /// </summary>
        /// <param name="action">The floor action being performed.</param>
        /// <param name="floorId">The identifier for the floor being acted on.</param>
        public delegate void OnFloorActionHandler(FloorAction action, string floorId);

        // Event declarations

        /// <summary>
        /// Event triggered when a file is loaded in the Game Editor.
        /// </summary>
        public static OnGameEditorMenuTypeEventHandler OnFileLoaded;

        /// <summary>
        /// Event triggered when a file is saved in the Game Editor.
        /// </summary>
        public static OnLoadEventStringHandler OnFileSaved;

        /// <summary>
        /// Event triggered when a screenshot is saved in the Game Editor.
        /// </summary>
        public static OnLoadEventMenuHandler OnScreenShotSaved;

        /// <summary>
        /// Event triggered when a request is made to clear all UI in the Game Editor.
        /// </summary>
        public static OnClearEventHandler OnRequestClearAll;

        /// <summary>
        /// Event triggered to request rebuilding all openings in the Game Editor.
        /// </summary>
        public static OnGameEditorEventHandler OnRequestRebuildAllOpenings;

        /// <summary>
        /// Event triggered to request rebuilding all rooms in the Game Editor.
        /// </summary>
        public static OnGameEditorEventHandler OnRequestRebuildAllRooms;

        /// <summary>
        /// Event triggered to request rebuilding a building in the Game Editor.
        /// </summary>
        public static OnGameEditorEventHandler OnRequestRebuildBuilding;

        /// <summary>
        /// Event triggered to request repositioning openings in the Game Editor.
        /// </summary>
        public static OnGameEditorEventHandler OnRequestRepositionOpenings;

        // Additional Event declarations (commented out since they are not in use) 

        /// <summary>
        /// (Commented out) Event triggered to request a change to the floor map in the Game Editor.
        /// </summary>
        //public static OnGameEditorEventStringHandler OnRequestChangeFloorMap;

        /// <summary>
        /// Event triggered when a file is changed in the Game Editor.
        /// </summary>
        public static OnLoadEventStringHandler OnFileChanged;

        /// <summary>
        /// Event triggered to handle a drag event in the Game Editor.
        /// </summary>
        public static OnGameEditorEventBoolHandler OnDragEvent;

        /// <summary>
        /// Event triggered to request an action button in the Game Editor.
        /// </summary>
        public static OnLoadEventActionHandler OnRequestButtonAction;

        /// <summary>
        /// Event triggered when a floor action is requested in the Game Editor.
        /// </summary>
        public static OnFloorActionHandler OnRequestFloorAction;

        /// <summary>
        /// Event triggered to handle rendering for a screenshot in the Game Editor.
        /// </summary>
        public static OnGameEditorEventBoolHandler OnRenderForScreenshot;
    }
}
