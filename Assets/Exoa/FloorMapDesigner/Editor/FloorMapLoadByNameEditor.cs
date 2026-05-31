
using UnityEditor;
using UnityEngine;

namespace Exoa.Designer
{
    /// <summary>
    /// Custom editor for the FloorMapLoadByName class to provide a user-friendly interface
    /// in the Unity Inspector for loading and clearing floor maps.
    /// </summary>
    [CustomEditor(typeof(FloorMapLoadByName))]
    public class FloorMapLoadByNameEditor : UnityEditor.Editor
    {
        /// <summary>
        /// Draws the inspector GUI for the FloorMapLoadByName component.
        /// It includes buttons for pre-baking the floor map and clearing it.
        /// </summary>
        public override void OnInspectorGUI()
        {
            FloorMapLoadByName obj = target as FloorMapLoadByName;

            DrawDefaultInspector();

            if (GUILayout.Button("Pre bake"))
            {
                obj.LoadFile(obj.fileName);
                obj.buildAtStart = false;
            }
            if (GUILayout.Button("Clear"))
            {
                obj.Clear();
            }
        }

    }
}
