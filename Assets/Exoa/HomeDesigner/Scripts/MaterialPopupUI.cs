
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Exoa.Designer.DataModel;

namespace Exoa.Designer
{
    /// <summary>
    /// Manages the Material Popup UI, including displaying materials according to the selected mode
    /// and applying material properties to various objects in the scene.
    /// </summary>
    public class MaterialPopupUI : MonoBehaviour
    {
        public GameObject root;

        public RectTransform itemsContainer;
        public TMP_Text titleTxt;
        public GameObject itemPrefab;
        public Slider tilingSlider;
        public Button applyToAllWallsBtn;

        private List<Button> btnList;

        /// <summary>
        /// Defines the different modes for material application: Floor, Interior Wall, Exterior Wall, etc.
        /// </summary>
        public enum Mode { Floor, InteriorWall, FloorAndInteriorWall, ExteriorWall, Ceiling, Roof, Module, Outside };
        private Mode currentMode;

        private MaterialPopup materialPopup;
        private Material selectedMaterial;

        /// <summary>
        /// Initializes the UI components and sets up listeners for the tiling slider and apply button.
        /// </summary>
        void Start()
        {
            tilingSlider.onValueChanged.AddListener(OnTilingSliderMoved);
            applyToAllWallsBtn.onClick.AddListener(OnClickApplyToAllWalls);
        }

        /// <summary>
        /// Applies the selected material and tiling property to all walls in the current room.
        /// </summary>
        private void OnClickApplyToAllWalls()
        {
            materialPopup.Room.ApplyInteriorWallMaterial(selectedMaterial);
            materialPopup.Room.ApplyInteriorWallTiling(tilingSlider.value);
        }

        /// <summary>
        /// Handles the event when the tiling slider value is changed and applies the new tiling
        /// to the current mode's material.
        /// </summary>
        /// <param name="tiling">The new tiling value from the slider.</param>
        private void OnTilingSliderMoved(float tiling)
        {
            switch (currentMode)
            {
                case Mode.Ceiling: materialPopup.Room.ApplyCeilingTiling(tiling); break;
                case Mode.Roof: materialPopup.Building.ApplyRoofTiling(tiling); break;
                case Mode.ExteriorWall: materialPopup.Building.ApplyExteriorWallTiling(tiling); break;
                case Mode.InteriorWall: materialPopup.Room.ApplyInteriorWallTiling(materialPopup.CurrentTarget.GetSiblingIndex(), tiling); break;
                case Mode.Floor: materialPopup.Room.ApplyFloorTiling(tiling); break;
                case Mode.Outside: materialPopup.Room.ApplyFloorTiling(tiling); break;
                //case Mode.Module: materialPopup.Module.ApplyModuleTiling(tiling); break;
            }
        }

        /// <summary>
        /// Sets up the popup UI for a specific mode, populating it with relevant materials
        /// and initializing the UI components.
        /// </summary>
        /// <param name="mode">The mode to show materials for.</param>
        public void ShowMode(Mode mode)
        {
            materialPopup = GetComponentInParent<MaterialPopup>();

            itemsContainer.ClearChildren();
            currentMode = mode;

            string title = "";
            string folder = "";
            float tilingSliderValue = 1f;

            // Set title, folder, and current tiling based on selected mode
            switch (mode)
            {
                case Mode.Floor:
                    title = "Floor Materials";
                    folder = HDSettings.FLOOR_MATERIALS_FOLDER;
                    tilingSliderValue = materialPopup.Room.GetFloorTextureSettings().tiling;
                    break;

                case Mode.InteriorWall:
                    title = "Interior Wall Materials";
                    folder = HDSettings.WALL_MATERIALS_FOLDER;
                    int wallId = materialPopup.CurrentTarget.GetSiblingIndex();
                    TextureSetting ts = materialPopup.Room.GetInteriorWallTextureSettings(wallId);
                    tilingSliderValue = ts.tiling;
                    break;

                case Mode.ExteriorWall:
                    title = "Exterior Wall Materials";
                    folder = HDSettings.EXTERIOR_WALL_MATERIALS_FOLDER;
                    tilingSliderValue = materialPopup.Building.GetExteriorWallsTextureSettings().tiling;
                    break;

                case Mode.Ceiling:
                    title = "Ceiling Materials";
                    folder = HDSettings.CEILING_MATERIALS_FOLDER;
                    tilingSliderValue = materialPopup.Room.GetCeilingTextureSettings().tiling;
                    break;

                case Mode.Roof:
                    title = "Roof Materials";
                    folder = HDSettings.ROOF_MATERIALS_FOLDER;
                    tilingSliderValue = materialPopup.Building.GetRoofTextureSettings().tiling;
                    break;

                case Mode.Outside:
                    title = "Outside Materials";
                    folder = HDSettings.OUTSIDE_MATERIALS_FOLDER;
                    tilingSliderValue = materialPopup.Room.GetFloorTextureSettings().tiling;
                    break;

                case Mode.Module:
                    title = "Color Variants";
                    folder = null;
                    break;
            }
            titleTxt.text = title;

            btnList = new List<Button>();
            itemsContainer.ClearChildren();
            tilingSlider.value = tilingSliderValue == 0f ? 1f : tilingSliderValue;
            tilingSlider.gameObject.SetActive(mode != Mode.Module);

            MeshRenderer mr = materialPopup.CurrentTarget.GetComponent<MeshRenderer>();
            selectedMaterial = mr != null ? mr.material : null;
            applyToAllWallsBtn.gameObject.SetActive(mode == Mode.InteriorWall);

            GameObject inst = null;
            RawImage img = null;
            Button btn = null;

            List<Material> mats = new List<Material>();
            List<Color> colors = new List<Color>();

            if (folder != null)
            {
                mats.AddRange(Resources.LoadAll<Material>(folder));
            }
            else if (mode == Mode.Module && materialPopup.Module != null)
            {
                for (int i = 0; i < materialPopup.Module.variants.Count; i++)
                {
                    if (materialPopup.Module.type == ModuleColorVariants.Type.Materials &&
                        materialPopup.Module.variants[i].material != null)
                    {
                        mats.Add(materialPopup.Module.variants[i].material);
                    }
                    else if (materialPopup.Module.type == ModuleColorVariants.Type.Colors)
                    {
                        colors.Add(materialPopup.Module.variants[i].color);
                    }
                }
            }

            foreach (Material m in mats)
            {
                inst = Instantiate(itemPrefab, itemsContainer);
                img = inst.GetComponent<RawImage>();
                img.texture = GetDiffuseTexture(m);
                btn = inst.GetComponent<Button>();
                btn.onClick.AddListener(() => SelectMaterial(currentMode, m));

                btnList.Add(btn);
            }

            foreach (Color c in colors)
            {
                inst = Instantiate(itemPrefab, itemsContainer);
                img = inst.GetComponent<RawImage>();
                img.color = c;
                btn = inst.GetComponent<Button>();
                btn.onClick.AddListener(() => SelectMaterial(currentMode, c));

                btnList.Add(btn);
            }
        }

        /// <summary>
        /// Applies the selected color to the current module.
        /// </summary>
        /// <param name="mode">The mode of operation.</param>
        /// <param name="c">The color to apply.</param>
        private void SelectMaterial(Mode mode, Color c)
        {
            switch (mode)
            {
                case Mode.Module: materialPopup.Module.ApplyModuleColor(c); break;
            }
        }

        /// <summary>
        /// Selects a material and applies it to the target in the current mode.
        /// </summary>
        /// <param name="mode">The mode of application.</param>
        /// <param name="m">The material to apply.</param>
        private void SelectMaterial(Mode mode, Material m)
        {
            selectedMaterial = new Material(m);
            switch (mode)
            {
                case Mode.Ceiling: materialPopup.Room.ApplyCeilingMaterial(selectedMaterial); break;
                case Mode.Roof: materialPopup.Building.ApplyRoofMaterial(selectedMaterial); break;
                case Mode.ExteriorWall: materialPopup.Building.ApplyExteriorWallMaterial(selectedMaterial); break;
                case Mode.InteriorWall: materialPopup.Room.ApplyInteriorWallMaterial(materialPopup.CurrentTarget.GetSiblingIndex(), selectedMaterial); break;
                case Mode.Floor: materialPopup.Room.ApplyFloorMaterial(selectedMaterial); break;
                case Mode.Outside: materialPopup.Room.ApplyFloorMaterial(selectedMaterial); break;
                case Mode.Module: materialPopup.Module.ApplyModuleMaterial(selectedMaterial); break;
            }
        }

        /// <summary>
        /// Gets the specular texture from the material if available.
        /// </summary>
        /// <param name="m">The material to check.</param>
        /// <returns>The specular texture if it exists; otherwise, null.</returns>
        private Texture GetSpecTexture(Material m)
        {
            if (m.HasProperty("_SpecularTexture2D")) return m.GetTexture("_SpecularTexture2D");
            if (m.HasProperty("_Spec")) return m.GetTexture("_Spec");
            if (m.HasProperty("_SpecTex")) return m.GetTexture("_SpecTex");
            return null;
        }

        /// <summary>
        /// Gets the diffuse texture from the material if available.
        /// </summary>
        /// <param name="m">The material to check.</param>
        /// <returns>The diffuse texture if it exists; otherwise, null.</returns>
        private Texture GetDiffuseTexture(Material m)
        {
            if (m.HasProperty("_DiffuseTexture2D")) return m.GetTexture("_DiffuseTexture2D");
            if (m.HasProperty("_Diffuse")) return m.GetTexture("_Diffuse");
            if (m.HasProperty("_MainTex")) return m.GetTexture("_MainTex");
            return null;
        }

        /// <summary>
        /// Gets the bump texture from the material if available.
        /// </summary>
        /// <param name="m">The material to check.</param>
        /// <returns>The bump texture if it exists; otherwise, null.</returns>
        private Texture GetBumpTexture(Material m)
        {
            if (m.HasProperty("_NormalTexture2D")) return m.GetTexture("_NormalTexture2D");
            if (m.HasProperty("_Normal")) return m.GetTexture("_Normal");
            if (m.HasProperty("_NormalTex")) return m.GetTexture("_NormalTex");
            return null;
        }
    }
}
