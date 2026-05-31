
using Exoa.Events;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Exoa.Designer
{
    /// <summary>
    /// Represents the building settings UI component that allows users to adjust various parameters of the building.
    /// </summary>
    public class UIBuildingSettings : GenericLeftMenu
    {
        /// <summary>
        /// Singleton instance of UIBuildingSettings.
        /// </summary>
        public static UIBuildingSettings instance;

        public Slider wallsHeightSlider;
        public Slider doorsHeightSlider;
        public Slider extWallsThicknessSlider;
        public Slider intWallsThicknessSlider;
        public Slider doorsThicknessSlider;
        public Slider windowsThicknessSlider;
        public Slider roofThicknessSlider;
        public Slider roofOverhangSlider;
        public Slider roofTypeSlider;
        public CanvasGroup canvasGroup;

        /// <summary>
        /// Clean-up operations when the object is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            GameEditorEvents.OnFileLoaded -= OnFileLoaded;
            AppController.OnAppStateChange -= OnAppStateChange;
        }

        /// <summary>
        /// Initializes the instance and sets up event listeners for UI sliders.
        /// </summary>
        private void Awake()
        {
            instance = this;
            wallsHeightSlider?.onValueChanged.AddListener(OnWallsHeightChange);
            doorsHeightSlider?.onValueChanged.AddListener(OnDoorsHeightChange);
            extWallsThicknessSlider?.onValueChanged.AddListener(OnExtWallsThicknessChange);
            intWallsThicknessSlider?.onValueChanged.AddListener(OnInttWallsThicknessChange);
            doorsThicknessSlider?.onValueChanged.AddListener(OnDoorsThicknessChange);
            windowsThicknessSlider?.onValueChanged.AddListener(OnWindowsThicknessChange);
            roofThicknessSlider?.onValueChanged.AddListener(OnRoofThicknessChange);
            roofOverhangSlider?.onValueChanged.AddListener(OnRoofOverhangChange);
            roofTypeSlider?.onValueChanged.AddListener(OnRoofTypeChange);

            GameEditorEvents.OnFileLoaded += OnFileLoaded;
            AppController.OnAppStateChange += OnAppStateChange;
        }

        /// <summary>
        /// Handles changes in application state, adjusting the UI's interactability and transparency based on the current state.
        /// </summary>
        /// <param name="state">The current application state.</param>
        private void OnAppStateChange(AppController.States state)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = state == AppController.States.PreviewBuilding ? .4f : 1;
                canvasGroup.interactable = state != AppController.States.PreviewBuilding;
            }
        }

        /// <summary>
        /// Responds to file loading events, setting UI values based on the loaded file type.
        /// </summary>
        /// <param name="fileType">The type of the loaded file.</param>
        private void OnFileLoaded(GameEditorEvents.FileType fileType)
        {
            if (fileType == GameEditorEvents.FileType.FloorMapFile)
            {
                SetValues();
            }
        }

        /// <summary>
        /// Sets the initial values of the sliders based on the values from the AppController.
        /// </summary>
        private void SetValues()
        {
            wallsHeightSlider.value = AppController.Instance.wallsHeight;
            doorsHeightSlider.value = AppController.Instance.doorsHeight;
            extWallsThicknessSlider.value = AppController.Instance.exteriorWallThickness;
            intWallsThicknessSlider.value = AppController.Instance.interiorWallThickness;
            doorsThicknessSlider.value = AppController.Instance.doorsThickness;
            windowsThicknessSlider.value = AppController.Instance.windowsThickness;
            roofThicknessSlider.value = AppController.Instance.roof.thickness;
            roofOverhangSlider.value = AppController.Instance.roof.overhang;
            roofTypeSlider.value = (int)AppController.Instance.roof.type;
        }

        /// <summary>
        /// Updates the wall height in the AppController and requests the rebuilding of the model.
        /// </summary>
        /// <param name="arg0">The new wall height value.</param>
        private void OnWallsHeightChange(float arg0)
        {
            if (AppController.Instance.State == AppController.States.PreviewBuilding)
                return;

            AppController.Instance.wallsHeight = arg0;

            GameEditorEvents.OnRequestRebuildAllOpenings?.Invoke();
            GameEditorEvents.OnRequestRebuildAllRooms?.Invoke();
            GameEditorEvents.OnRequestRebuildBuilding?.Invoke();
        }

        /// <summary>
        /// Updates the door height in the AppController and requests the rebuilding of the model.
        /// </summary>
        /// <param name="arg0">The new door height value.</param>
        private void OnDoorsHeightChange(float arg0)
        {
            if (AppController.Instance.State == AppController.States.PreviewBuilding)
                return;

            AppController.Instance.doorsHeight = arg0;
            GameEditorEvents.OnRequestRebuildAllOpenings?.Invoke();
            GameEditorEvents.OnRequestRebuildAllRooms?.Invoke();
            GameEditorEvents.OnRequestRebuildBuilding?.Invoke();
        }

        /// <summary>
        /// Updates the exterior wall thickness in the AppController and requests the rebuilding of the model.
        /// </summary>
        /// <param name="arg0">The new exterior wall thickness value.</param>
        private void OnExtWallsThicknessChange(float arg0)
        {
            if (AppController.Instance.State == AppController.States.PreviewBuilding)
                return;

            AppController.Instance.exteriorWallThickness = arg0;
            GameEditorEvents.OnRequestRebuildBuilding?.Invoke();
        }

        /// <summary>
        /// Updates the interior wall thickness in the AppController and requests the rebuilding of all rooms.
        /// </summary>
        /// <param name="arg0">The new interior wall thickness value.</param>
        private void OnInttWallsThicknessChange(float arg0)
        {
            if (AppController.Instance.State == AppController.States.PreviewBuilding)
                return;

            AppController.Instance.interiorWallThickness = arg0;
            GameEditorEvents.OnRequestRebuildAllRooms?.Invoke();
        }

        /// <summary>
        /// Updates the door thickness in the AppController and requests the rebuilding of openings and rooms.
        /// </summary>
        /// <param name="arg0">The new door thickness value.</param>
        private void OnDoorsThicknessChange(float arg0)
        {
            if (AppController.Instance.State == AppController.States.PreviewBuilding)
                return;

            AppController.Instance.doorsThickness = arg0;
            GameEditorEvents.OnRequestRebuildAllOpenings?.Invoke();
            GameEditorEvents.OnRequestRebuildAllRooms?.Invoke();
        }

        /// <summary>
        /// Updates the window thickness in the AppController and requests the rebuilding of openings and rooms.
        /// </summary>
        /// <param name="arg0">The new window thickness value.</param>
        private void OnWindowsThicknessChange(float arg0)
        {
            if (AppController.Instance.State == AppController.States.PreviewBuilding)
                return;

            AppController.Instance.windowsThickness = arg0;
            GameEditorEvents.OnRequestRebuildAllOpenings?.Invoke();
            GameEditorEvents.OnRequestRebuildAllRooms?.Invoke();
        }

        /// <summary>
        /// Updates the roof thickness in the AppController and requests the rebuilding of the model.
        /// </summary>
        /// <param name="arg0">The new roof thickness value.</param>
        private void OnRoofThicknessChange(float arg0)
        {
            if (AppController.Instance.State == AppController.States.PreviewBuilding)
                return;

            AppController.Instance.roof.thickness = arg0;
            GameEditorEvents.OnRequestRebuildBuilding?.Invoke();
        }

        /// <summary>
        /// Updates the roof overhang in the AppController and requests the rebuilding of the model.
        /// </summary>
        /// <param name="arg0">The new roof overhang value.</param>
        private void OnRoofOverhangChange(float arg0)
        {
            if (AppController.Instance.State == AppController.States.PreviewBuilding)
                return;

            AppController.Instance.roof.overhang = arg0;
            GameEditorEvents.OnRequestRebuildBuilding?.Invoke();
        }

        /// <summary>
        /// Updates the roof type in the AppController based on the provided value and requests the rebuilding of the model.
        /// </summary>
        /// <param name="arg0">The value representing the new roof type.</param>
        private void OnRoofTypeChange(float arg0)
        {
            if (AppController.Instance.State == AppController.States.PreviewBuilding)
                return;

            AppController.RoofType[] values = Enum.GetValues(typeof(AppController.RoofType)) as AppController.RoofType[];
            AppController.Instance.roof.type = values[Mathf.RoundToInt(arg0)];
            GameEditorEvents.OnRequestRebuildBuilding?.Invoke();
        }
    }
}
