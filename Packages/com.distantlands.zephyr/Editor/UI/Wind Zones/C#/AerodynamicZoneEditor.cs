using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Linq;

namespace DistantLands.Zephyr.EditorScripts
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AerodynamicZone))]
    public class AerodynamicZoneEditor : Editor
    {

        VisualElement root;

        public override VisualElement CreateInspectorGUI()
        {
            root = new VisualElement();

            VisualTreeAsset asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Packages/com.distantlands.zephyr/Editor/UI/Wind Zones/UXML/aerodynamic-zone-editor.uxml"
            );

            asset.CloneTree(root);
            return root;

        }
    }
}
