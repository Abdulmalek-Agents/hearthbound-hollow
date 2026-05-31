
using Exoa.Designer;
using Exoa.Events;
using System;
using UnityEngine;
using static Exoa.Events.GameEditorEvents;

namespace Exoa.Designer
{
    /// <summary>
    /// Class responsible for handling various debug events related to file and screenshot operations within the Unity editor.
    /// </summary>
    public class AppDebugguer : MonoBehaviour
    {
#if UNITY_EDITOR
        /// <summary>
        /// Unsubscribes from all GameEditorEvents when the object is destroyed.
        /// </summary>
        void OnDestroy()
        {
            GameEditorEvents.OnFileLoaded -= OnFileLoaded;
            GameEditorEvents.OnFileSaved -= OnFileSaved;
            GameEditorEvents.OnFileChanged -= OnFileChanged;

            GameEditorEvents.OnScreenShotSaved -= OnScreenShotSaved;
            GameEditorEvents.OnRenderForScreenshot -= OnRenderForScreenshot;

            GameEditorEvents.OnRequestClearAll -= OnRequestClearAll;
            GameEditorEvents.OnRequestButtonAction -= OnRequestButtonAction;
            GameEditorEvents.OnRequestFloorAction -= OnRequestFloorAction;
        }

        /// <summary>
        /// Subscribes to relevant GameEditorEvents when the object is started.
        /// </summary>
        void Start()
        {
            GameEditorEvents.OnFileLoaded += OnFileLoaded;
            GameEditorEvents.OnFileSaved += OnFileSaved;
            GameEditorEvents.OnFileChanged += OnFileChanged;

            GameEditorEvents.OnScreenShotSaved += OnScreenShotSaved;
            GameEditorEvents.OnRenderForScreenshot += OnRenderForScreenshot;

            GameEditorEvents.OnRequestClearAll += OnRequestClearAll;
            GameEditorEvents.OnRequestButtonAction += OnRequestButtonAction;
            GameEditorEvents.OnRequestFloorAction += OnRequestFloorAction;
        }

        /// <summary>
        /// Callback for when a file is loaded. Logs the type of file loaded.
        /// </summary>
        /// <param name="fileType">The type of file that was loaded.</param>
        private void OnFileLoaded(GameEditorEvents.FileType fileType)
        {
            HDLogger.Log("OnFileLoaded fileType:" + fileType, HDLogger.LogCategory.FileSystem);
        }

        /// <summary>
        /// Callback for when a file is saved. Logs the name and type of the file saved.
        /// </summary>
        /// <param name="name">The name of the file being saved.</param>
        /// <param name="fileType">The type of file that was saved.</param>
        private void OnFileSaved(string name, GameEditorEvents.FileType fileType)
        {
            HDLogger.Log("OnFileSaved fileType:" + fileType, HDLogger.LogCategory.FileSystem);
        }

        /// <summary>
        /// Callback for when a file is changed. Logs the name and type of the file changed.
        /// </summary>
        /// <param name="name">The name of the file that was changed.</param>
        /// <param name="fileType">The type of file that was changed.</param>
        private void OnFileChanged(string name, GameEditorEvents.FileType fileType)
        {
            HDLogger.Log("OnFileChanged name:" + name + " fileType:" + fileType, HDLogger.LogCategory.FileSystem);
        }

        /// <summary>
        /// Callback for when a screenshot is saved. Logs the name and menu type associated with the screenshot.
        /// </summary>
        /// <param name="name">The name of the saved screenshot.</param>
        /// <param name="menuType">The menu type from which the screenshot was taken.</param>
        private void OnScreenShotSaved(string name, GameEditorEvents.MenuType menuType)
        {
            HDLogger.Log("OnScreenShotSaved name:" + name + " menuType:" + menuType, HDLogger.LogCategory.Screenshot);
        }

        /// <summary>
        /// Callback for when a render for a screenshot is requested. Logs the current render status.
        /// </summary>
        /// <param name="render">Indicates whether a render is requested or not.</param>
        private void OnRenderForScreenshot(bool render)
        {
            HDLogger.Log("OnRenderForScreenshot render:" + render, HDLogger.LogCategory.Screenshot);
        }

        /// <summary>
        /// Callback for when a request to clear all elements is made. Logs the clearing options chosen.
        /// </summary>
        /// <param name="clearFloorsUI">Indicates whether to clear the floors UI.</param>
        /// <param name="clearFloorMapUI">Indicates whether to clear the floor map UI.</param>
        /// <param name="clearScene">Indicates whether to clear the scene.</param>
        private void OnRequestClearAll(bool clearFloorsUI, bool clearFloorMapUI, bool clearScene)
        {
            HDLogger.Log("OnRequestClearAll clearFloorsUI:" + clearFloorsUI + " clearFloorMapUI:" + clearFloorMapUI + " clearScene:" + clearScene, HDLogger.LogCategory.General);
        }

        /// <summary>
        /// Callback for button actions requested. Logs the action type and its active status.
        /// </summary>
        /// <param name="action">The action requested.</param>
        /// <param name="active">Indicates whether the action is active or not.</param>
        private void OnRequestButtonAction(GameEditorEvents.Action action, bool active)
        {
            HDLogger.Log("OnRequestButtonAction action:" + action, HDLogger.LogCategory.General);
        }

        /// <summary>
        /// Callback for floor actions requested. Logs the name of the action and its identifier.
        /// </summary>
        /// <param name="action">The floor action requested.</param>
        /// <param name="name">The identifier name associated with the action.</param>
        private void OnRequestFloorAction(FloorAction action, string name)
        {
            HDLogger.Log("OnRequestFloorAction name:" + action.ToString() + " id:" + name, HDLogger.LogCategory.Floormap);
        }
#endif
    }
}
