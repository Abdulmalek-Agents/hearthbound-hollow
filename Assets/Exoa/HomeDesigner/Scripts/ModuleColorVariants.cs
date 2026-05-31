
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Exoa.Designer
{
    /// <summary>
    /// Manages color and material variants for a module, allowing for the dynamic application of materials and colors to designated renderers.
    /// </summary>
    public class ModuleColorVariants : MonoBehaviour
    {
        /// <summary>
        /// A structure representing a variant that includes a name, material, and color.
        /// </summary>
        [System.Serializable]
        public struct Variant
        {
            public string name; // The name of the variant.
            public Material material; // The material associated with the variant.
            public Color color; // The color associated with the variant.
        }

        /// <summary>
        /// Enumeration representing the type of variants: either Colors or Materials.
        /// </summary>
        public enum Type { Colors, Materials };

        public Type type; // The type of variants being used (Colors or Materials).
        public List<Variant> variants; // A list of variant options to choose from.
        public List<Renderer> renderers; // A list of renderers to apply materials and colors to.
        
        private string selectedMaterialName; // Stores the name of the currently selected material.
        private Color selectedColor; // Stores the currently selected color.

        /// <summary>
        /// Gets or sets the selected color.
        /// </summary>
        public Color SelectedColor 
        { 
            get => selectedColor; 
            set => selectedColor = value; 
        }

        /// <summary>
        /// Gets or sets the name of the selected material.
        /// </summary>
        public string SelectedMaterialName 
        { 
            get => selectedMaterialName; 
            set => selectedMaterialName = value; 
        }

        /// <summary>
        /// Applies a material to the module based on its name. Logs the application process.
        /// </summary>
        /// <param name="materialName">The name of the material to apply.</param>
        public void ApplyModuleMaterial(string materialName)
        {
            if (string.IsNullOrEmpty(materialName))
                return;

            HDLogger.Log("ApplyModuleMaterial materialName:" + materialName, HDLogger.LogCategory.Interior);

            for (int i = 0; i < variants.Count; i++)
            {
                if (variants[i].material != null && variants[i].material.name == materialName)
                {
                    ApplyModuleMaterial(variants[i].material);
                    return;
                }
            }
        }

        /// <summary>
        /// Applies a specified material to all renderers in the list. Logs the application process.
        /// </summary>
        /// <param name="mat">The material to apply.</param>
        public void ApplyModuleMaterial(Material mat)
        {
            if (mat == null)
                return;

            HDLogger.Log("ApplyModuleMaterial mat.name:" + mat.name, HDLogger.LogCategory.Interior);

            selectedMaterialName = mat.name;
            for (int i = 0; i < renderers.Count; i++)
            {
                if (renderers[i] != null) renderers[i].material = mat;
            }
        }

        /// <summary>
        /// Applies tiling settings to the module. (Method not implemented.)
        /// </summary>
        /// <param name="tiling">The tiling value to apply.</param>
        internal void ApplyModuleTiling(float tiling)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Applies a specified color to all renderers in the list. Creates new materials to avoid altering shared materials.
        /// </summary>
        /// <param name="c">The color to apply.</param>
        public void ApplyModuleColor(Color c)
        {
            if (c == default(Color))
                return;

            selectedColor = c;
            for (int i = 0; i < renderers.Count; i++)
            {
                if (renderers[i] != null)
                {
                    Material tempMaterial = new Material(renderers[i].sharedMaterial);
                    tempMaterial.color = c;
                    renderers[i].sharedMaterial = tempMaterial;
                }
            }
        }
    }
}
