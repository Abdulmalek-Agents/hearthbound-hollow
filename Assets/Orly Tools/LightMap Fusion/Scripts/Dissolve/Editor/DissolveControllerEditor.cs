#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace LightMapFusion
{

    [CustomEditor(typeof(DissolveController))]
    public class DissolveControllerEditor : Editor
    {
        //public SerializedProperty volumeGizmo;
        public SerializedProperty toolShaders;
        public SerializedProperty id;
        public SerializedProperty noise;
        public SerializedProperty edgeColor;
        public SerializedProperty edgeThickness;
        public SerializedProperty noiseScale;
        public SerializedProperty noiseScrollSpeed;
        public SerializedProperty invert;
        public SerializedProperty alphaThreshold;
        public SerializedProperty selectorMask;
        public SerializedProperty volumeGizmo;

        private const string EVENT_UPDATE_VOLUMES = "Update Volumes";

        public void OnEnable()
        {
            volumeGizmo = serializedObject.FindProperty("volumeGizmo");
            selectorMask = serializedObject.FindProperty("selectorMask");
            toolShaders = serializedObject.FindProperty("toolShaders");
            id = serializedObject.FindProperty("id");
            noise = serializedObject.FindProperty("noise");
            edgeColor = serializedObject.FindProperty("edgeColor");
            edgeThickness = serializedObject.FindProperty("edgeThickness");
            noiseScale = serializedObject.FindProperty("noiseScale");
            noiseScrollSpeed = serializedObject.FindProperty("noiseScrollSpeed");
            invert = serializedObject.FindProperty("invert");
            alphaThreshold = serializedObject.FindProperty("alphaThreshold");

            DissolveController tool = target as DissolveController;
            EventManagerOrly.StartListening(
                EVENT_UPDATE_VOLUMES,
                (EventData data) =>
                {
                    tool.AssigMaterialtoVolumes();
                    Debug.Log("Escuchando");
                }
            );
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            BaseProperties();
            serializedObject.ApplyModifiedProperties();
        }

        private void BaseProperties()
        {
            DissolveController tool = target as DissolveController;
            EditorGUILayout.Space(10);
            EditorGUILayout.PropertyField(selectorMask);
            EditorGUILayout.PropertyField(id);
            EditorGUILayout.PropertyField(invert);
            EditorGUILayout.PropertyField(alphaThreshold);
            EditorGUILayout.PropertyField(edgeColor);
            EditorGUILayout.PropertyField(noise);
            EditorGUILayout.PropertyField(edgeThickness);
            EditorGUILayout.PropertyField(noiseScale);
            EditorGUILayout.PropertyField(noiseScrollSpeed);
            EditorGUILayout.PropertyField(toolShaders);
        }
    }
}


#endif
