using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Linq;

namespace DistantLands.Zephyr.EditorScripts
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(FunnelZone))]
    public class FunnelZoneEditor : Editor
    {

        VisualElement root;

        public override VisualElement CreateInspectorGUI()
        {
            root = new VisualElement();

            VisualTreeAsset asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Packages/com.distantlands.zephyr/Editor/UI/Wind Zones/UXML/funnel-zone-editor.uxml"
            );

            asset.CloneTree(root);
            return root;

        }
    }
}
