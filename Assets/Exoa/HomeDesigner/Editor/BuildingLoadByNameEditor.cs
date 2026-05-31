
using UnityEditor;
using UnityEngine;

namespace Exoa.Designer
{
    /// <summary>
    /// Custom editor for the HomeLoadByName class.
    /// This editor allows for specialized inspector functionalities, such as pre-baking
    /// and clearing resources.
    /// </summary>
    [CustomEditor(typeof(HomeLoadByName))]
    public class BuildingLoadByNameEditor : UnityEditor.Editor
    {
        /// <summary>
        /// OnInspectorGUI is called for rendering and handling GUI events in the inspector.
        /// This method draws the default inspector and adds custom buttons for pre-baking
        /// and clearing actions related to the HomeLoadByName object.
        /// </summary>
        public override void OnInspectorGUI()
        {
            HomeLoadByName obj = target as HomeLoadByName; // Cast the target to HomeLoadByName

            DrawDefaultInspector(); // Draws the default properties in the inspector.

            // Button for pre-baking
            if (GUILayout.Button("Pre bake"))
            {
                obj.LoadFile(obj.fileName); // Load the specified file via the object method
                obj.buildAtStart = false; // Set buildAtStart to false
            }

            // Button for clearing resources
            if (GUILayout.Button("Clear"))
            {
                obj.Clear(); // Invoke the Clear method to remove existing resources
            }
        }
    }
}
