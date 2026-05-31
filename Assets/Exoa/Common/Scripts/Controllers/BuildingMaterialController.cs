
using Exoa.Designer;
using Exoa.Events;
using System;
using UnityEngine;
using static Exoa.Designer.DataModel;

namespace Exoa.Designer
{
    /// <summary>
    /// Manages the rendering and material properties of building materials, including exterior walls and roofs.
    /// Handles the toggling of visibility, texture settings, and material application.
    /// </summary>
    public class BuildingMaterialController : MonoBehaviour
    {
        public MeshRenderer exteriorWalls; // Renderer for the exterior walls
        public MeshRenderer roof; // Renderer for the roof
        public MeshCollider exteriorWallsCol; // Collider for the exterior walls
        public MeshCollider roofCol; // Collider for the roof

        private TextureSetting roofTextureSetting; // Holds texture settings for the roof
        private TextureSetting exteriorWallTextureSetting; // Holds texture settings for the exterior walls

        private bool areWallsDisplayed = true; // Tracks visibility of exterior walls
        private bool areRoofsDisplayed = true; // Tracks visibility of roofs

        /// <summary>
        /// Unsubscribes from events when the object is destroyed.
        /// </summary>
        void OnDestroy()
        {
            GameEditorEvents.OnRequestButtonAction -= OnRequestButtonAction;
            GameEditorEvents.OnRenderForScreenshot -= OnRenderForScreenshot;
            AppController.OnAppStateChange -= OnAppStateChange;
        }

        /// <summary>
        /// Subscribes to necessary events and sets the initial visibility of the walls and roof based on the application state.
        /// </summary>
        void Start()
        {
            GameEditorEvents.OnRequestButtonAction += OnRequestButtonAction;
            GameEditorEvents.OnRenderForScreenshot += OnRenderForScreenshot;
            AppController.OnAppStateChange += OnAppStateChange;

            bool showRoofAndWalls = AppController.Instance.State == AppController.States.PreviewBuilding ||
                                    AppController.Instance.State == AppController.States.PlayMode;
            ShowExteriorWalls(showRoofAndWalls);
            ShowRoof(showRoofAndWalls);
        }

        /// <summary>
        /// Applies the specified tiling value to the exterior wall's material texture.
        /// </summary>
        /// <param name="tiling">The tiling value to apply.</param>
        internal void ApplyExteriorWallTiling(float tiling)
        {
            exteriorWalls.material.mainTextureScale = new Vector2(tiling, tiling);
            exteriorWallTextureSetting.tiling = tiling;
        }

        /// <summary>
        /// Applies the specified tiling value to the roof's material texture.
        /// </summary>
        /// <param name="tiling">The tiling value to apply.</param>
        internal void ApplyRoofTiling(float tiling)
        {
            roof.material.mainTextureScale = new Vector2(tiling, tiling);
            roofTextureSetting.tiling = tiling;
        }

        /// <summary>
        /// Handles application state changes and adjusts visibility of roof and exterior walls accordingly.
        /// </summary>
        /// <param name="state">The current application state.</param>
        private void OnAppStateChange(AppController.States state)
        {
            if (state == AppController.States.PreviewBuilding)
            {
                ShowRoof(true);
                ShowExteriorWalls(true);
            }
            if (state == AppController.States.Draw)
            {
                ShowRoof(false);
            }
        }

        /// <summary>
        /// Responds to button actions, toggling visibility or showing elements based on the action received.
        /// </summary>
        /// <param name="action">The action that was triggered.</param>
        /// <param name="active">Indicates whether the action is active or not.</param>
        private void OnRequestButtonAction(GameEditorEvents.Action action, bool active)
        {
            switch (action)
            {
                case GameEditorEvents.Action.ToggleExteriorWalls: ToggleExteriorWalls(); break;
                case GameEditorEvents.Action.ShowExteriorWalls: ShowExteriorWalls(active); break;
                case GameEditorEvents.Action.ToggleRoof: ToggleRoof(); break;
                case GameEditorEvents.Action.ShowRoof: ShowRoof(active); break;
            }
        }

        private bool exteriorWallsEnabledBeforeScreenshot; // Stores the state of exterior walls before a screenshot
        private bool roofEnabledBeforeScreenshot; // Stores the state of the roof before a screenshot

        /// <summary>
        /// Handles rendering for screenshots by temporarily modifying visibility of walls and roofs.
        /// </summary>
        /// <param name="preRender">Indicates whether it's before rendering.</param>
        private void OnRenderForScreenshot(bool preRender)
        {
            HDLogger.Log("OnRenderForScreenshot preRender:" + preRender, HDLogger.LogCategory.Screenshot);
            if (preRender)
            {
                exteriorWallsEnabledBeforeScreenshot = exteriorWalls.enabled;
                roofEnabledBeforeScreenshot = roof.enabled;

                exteriorWalls.enabled = true;
                roof.enabled = false;
            }
            else
            {
                exteriorWalls.enabled = exteriorWallsEnabledBeforeScreenshot;
                roof.enabled = roofEnabledBeforeScreenshot;
            }
        }

        /// <summary>
        /// Gets the current texture settings for the exterior walls, ensuring a default tiling value.
        /// </summary>
        /// <returns>The current texture settings for the exterior walls.</returns>
        internal TextureSetting GetExteriorWallsTextureSettings()
        {
            exteriorWallTextureSetting.tiling = exteriorWallTextureSetting.tiling == 0 ? 1f : exteriorWallTextureSetting.tiling;
            return exteriorWallTextureSetting;
        }

        /// <summary>
        /// Gets the current texture settings for the roof, ensuring a default tiling value.
        /// </summary>
        /// <returns>The current texture settings for the roof.</returns>
        internal TextureSetting GetRoofTextureSettings()
        {
            roofTextureSetting.tiling = roofTextureSetting.tiling == 0 ? 1f : roofTextureSetting.tiling;
            return roofTextureSetting;
        }

        /// <summary>
        /// Updates this MonoBehaviour once per frame, toggling visibility based on input commands.
        /// </summary>
        void Update()
        {
            if (HDInputs.ToggleExteriorWalls())
            {
                ToggleExteriorWalls();
            }
            if (HDInputs.ToggleRoof())
            {
                ToggleRoof();
            }
        }

        /// <summary>
        /// Toggles the display state of the exterior walls and updates visibility accordingly.
        /// </summary>
        private void ToggleExteriorWalls()
        {
            areWallsDisplayed = !areWallsDisplayed;
            ShowExteriorWalls(areWallsDisplayed);
        }

        /// <summary>
        /// Sets the visibility of the exterior walls.
        /// </summary>
        /// <param name="show">Indicates whether to show or hide the exterior walls.</param>
        private void ShowExteriorWalls(bool show)
        {
            HDLogger.Log("Show Ext Walls show:" + show, HDLogger.LogCategory.Building);
            areWallsDisplayed = show;
            exteriorWalls.enabled = areWallsDisplayed;
            if (exteriorWallsCol != null)
                exteriorWallsCol.enabled = areWallsDisplayed;
        }

        /// <summary>
        /// Toggles the display state of the roof and updates visibility accordingly.
        /// </summary>
        private void ToggleRoof()
        {
            areRoofsDisplayed = !areRoofsDisplayed;
            ShowRoof(areRoofsDisplayed);
        }

        /// <summary>
        /// Sets the visibility of the roof.
        /// </summary>
        /// <param name="active">Indicates whether to show or hide the roof.</param>
        private void ShowRoof(bool active)
        {
            HDLogger.Log("Show Roof show:" + active, HDLogger.LogCategory.Building);
            areRoofsDisplayed = active;
            roof.enabled = areRoofsDisplayed;
            if (roofCol != null) roofCol.enabled = areRoofsDisplayed;
        }

        /// <summary>
        /// Applies the specified material to the exterior walls using a Material object.
        /// </summary>
        /// <param name="m">The Material to apply.</param>
        public void ApplyExteriorWallMaterial(Material m)
        {
            ApplyExteriorWallMaterial(new TextureSetting(m.name, 0));
        }

        /// <summary>
        /// Applies the specified material to the exterior walls using a material name string.
        /// </summary>
        /// <param name="matName">The name of the material to apply.</param>
        public void ApplyExteriorWallMaterial(string matName)
        {
            ApplyExteriorWallMaterial(new TextureSetting(matName, 1f));
        }

        /// <summary>
        /// Applies the specified material to the roof using a Material object.
        /// </summary>
        /// <param name="m">The Material to apply.</param>
        public void ApplyRoofMaterial(Material m)
        {
            ApplyRoofMaterial(new TextureSetting(m.name, 0));
        }

        /// <summary>
        /// Applies the specified material to the roof using a material name string.
        /// </summary>
        /// <param name="matName">The name of the material to apply.</param>
        public void ApplyRoofMaterial(string matName)
        {
            ApplyRoofMaterial(new TextureSetting(matName, 0));
        }

        /// <summary>
        /// Applies the material specified in the TextureSetting to the exterior walls.
        /// </summary>
        /// <param name="ts">The TextureSetting containing material information.</param>
        private void ApplyExteriorWallMaterial(TextureSetting ts)
        {
            Material m = Resources.Load<Material>(HDSettings.EXTERIOR_WALL_MATERIALS_FOLDER + ts.materialName);
            if (m != null)
            {
                exteriorWallTextureSetting.materialName = ts.materialName;
                exteriorWallTextureSetting.tiling = ts.tiling > 0f ? ts.tiling : (exteriorWallTextureSetting.tiling > 0 ? exteriorWallTextureSetting.tiling : 1);

                exteriorWalls.material = m;
                exteriorWalls.material.mainTextureScale = new Vector2(exteriorWallTextureSetting.tiling, exteriorWallTextureSetting.tiling);
            }
        }

        /// <summary>
        /// Applies the material specified in the TextureSetting to the roof.
        /// </summary>
        /// <param name="ts">The TextureSetting containing material information.</param>
        private void ApplyRoofMaterial(TextureSetting ts)
        {
            Material m = Resources.Load<Material>(HDSettings.ROOF_MATERIALS_FOLDER + ts.materialName);
            if (m != null)
            {
                roofTextureSetting.materialName = ts.materialName;
                roofTextureSetting.tiling = ts.tiling > 0f ? ts.tiling : (roofTextureSetting.tiling > 0 ? roofTextureSetting.tiling : 1);

                roof.material = m;
                roof.material.mainTextureScale = new Vector2(roofTextureSetting.tiling, roofTextureSetting.tiling);
            }
        }

#if INTERIOR_MODULE
        /// <summary>
        /// Gets the current building settings including texture settings for exterior walls and roof.
        /// </summary>
        /// <returns>A BuildingSetting object with current settings.</returns>
        public BuildingSetting GetBuildingSetting()
        {
            BuildingSetting rs = new BuildingSetting();
            rs.exteriorWallTextureSetting = GetExteriorWallsTextureSettings();
            rs.roofTextureSetting = GetRoofTextureSettings();
            return rs;
        }

        /// <summary>
        /// Sets the building settings based on the provided BuildingSetting object.
        /// </summary>
        /// <param name="bs">The BuildingSetting to apply.</param>
        public void SetBuildingSetting(BuildingSetting bs)
        {
            if (!string.IsNullOrEmpty(bs.exteriorWallMat))
            {
                ApplyExteriorWallMaterial(bs.exteriorWallMat);
            }
            else if (!string.IsNullOrEmpty(bs.exteriorWallTextureSetting.materialName))
            {
                ApplyExteriorWallMaterial(bs.exteriorWallTextureSetting);
            }
            if (!string.IsNullOrEmpty(bs.roofMat))
            {
                ApplyRoofMaterial(bs.roofMat);
            }
            else if (!string.IsNullOrEmpty(bs.roofTextureSetting.materialName))
            {
                ApplyRoofMaterial(bs.roofTextureSetting);
            }
        }

#else
        /// <summary>
        /// Placeholder for getting building settings. Returns null in the absence of INTERIOR_MODULE.
        /// </summary>
        /// <returns>Always returns null.</returns>
        public object GetBuildingSetting()
        {
            return null;
        }

        /// <summary>
        /// Placeholder for setting building settings. Does nothing in the absence of INTERIOR_MODULE.
        /// </summary>
        /// <param name="bs">The settings to apply (currently ignored).</param>
        public void SetBuildingSetting(object bs)
        {

        }
#endif
    }
}
