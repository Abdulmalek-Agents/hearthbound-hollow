using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Linq;

namespace DistantLands.Zephyr.EditorScripts
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(WindPhysics))]
    public class WindPhysicsEditor : Editor
    {
        VisualElement root;
        PropertyField OverrideInputListener => root.Q<PropertyField>("overrideInputListener");
        VisualElement WindProperties => root.Q<VisualElement>("windProperties");
        public override VisualElement CreateInspectorGUI()
        {
            root = new VisualElement();
            VisualTreeAsset asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Packages/com.distantlands.zephyr/Editor/UI/Effect Listeners/UXML/wind-physics-editor.uxml"
            );
            asset.CloneTree(root);

            OverrideInputListener.RegisterValueChangeCallback(evt =>
            {
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();

                if (serializedObject.FindProperty("overrideInputListener").objectReferenceValue == serializedObject.targetObject)
                {
                    serializedObject.FindProperty("overrideInputListener").objectReferenceValue = null;
                    Debug.Log("Override cannot be set to self!");
                    serializedObject.ApplyModifiedProperties();
                    serializedObject.Update();
                }


                WindProperties.enabledSelf = serializedObject.FindProperty("overrideInputListener").objectReferenceValue == null;

            });
            return root;
        }
    }
}