#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LightMapFusion
{
    public class LMFMaterialConverter : EditorWindow
    {
        [SerializeField]
        private List<Material> UnityMaterials = new List<Material>();
        [SerializeField]
        private List<Material> ToolMaterials = new List<Material>();

        private MaterialsRevertData materialsRevertData;
        private SerializedObject serializedRevertData;

        private SerializedObject serializedObject;
        private SerializedProperty materialsUnityProperty;
        private SerializedProperty materialsToolProperty;
        private Vector2 scrollPos;

        public Color blueColor = new Color(0.185f, 0.7f, 1.0f, 1.0f);

        private string sceneName;



        [MenuItem("Window/Orly Tools/LightMap Fusion/Material Converter")]
        public static void ShowWindow()
        {
            var window = GetWindow<LMFMaterialConverter>("Material Converter");
            window.Initialize();
        }

        private void Initialize()
        {
            sceneName = SceneManager.GetActiveScene().name;
            serializedObject = new SerializedObject(this);


            materialsUnityProperty = serializedObject.FindProperty("UnityMaterials");
            materialsToolProperty = serializedObject.FindProperty("ToolMaterials");
            materialsRevertData = AssetDatabase.LoadAssetAtPath<MaterialsRevertData>("Assets/Orly Tools/LightMap Fusion/Resources/Materials Revert Data.asset");

            if (materialsRevertData == null)
            {
                materialsRevertData = CreateInstance<MaterialsRevertData>();
                AssetDatabase.CreateAsset(materialsRevertData, "Assets/Orly Tools/LightMap Fusion/Resources/Materials Revert Data.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            serializedRevertData = new SerializedObject(materialsRevertData);
        }

        private void OnEnable()
        {
            Initialize();
        }

        private void OnGUI()
        {

            if (serializedObject == null || materialsUnityProperty == null || materialsToolProperty == null)
            {
                Initialize();
            }
            int buttonHeight = 30;
            serializedObject.Update();
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical("Box");

            var style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 14
            };
            style.normal.textColor = blueColor;
            EditorGUILayout.LabelField("LightMap Fusion - Material Converter", style, GUILayout.ExpandWidth(true));
            EditorGUILayout.Space(20);
            GuiLine(blueColor, 2);
            EditorGUILayout.Space(10);
            EditorGUILayout.ObjectField(materialsRevertData, typeof(MaterialsRevertData), false);
            EditorGUILayout.BeginHorizontal("Box");
            if (GUILayout.Button("Find Unity Materials", GUILayout.Height(buttonHeight)))
            {
                FindUnityMaterials();
            }
            if (GUILayout.Button("Find Tool Materials", GUILayout.Height(buttonHeight)))
            {
                FindToolMaterials();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal("Box");
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            if (materialsUnityProperty != null)
            {
                EditorGUILayout.PropertyField(materialsUnityProperty, true);
            }
            EditorGUILayout.EndScrollView();


            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            if (materialsToolProperty != null)
            {
                EditorGUILayout.PropertyField(materialsToolProperty, true);
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(20);
            EditorGUILayout.BeginVertical("Box");
            style.fontSize = 12;
            style.normal.textColor = Color.white;
            EditorGUILayout.LabelField("------------", style, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal("Box");
            if (GUILayout.Button("Convert Materials", GUILayout.Height(buttonHeight)))
            {
                materialsRevertData = Resources.Load<MaterialsRevertData>("Materials Revert Data");
                ConvertMaterials();
            }
            if (GUILayout.Button("Revert Materials", GUILayout.Height(buttonHeight)))
            {
                materialsRevertData = Resources.Load<MaterialsRevertData>("Materials Revert Data");
                RevertMaterials();
            }
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
        }

        private void FindUnityMaterials()
        {
            UnityMaterials.Clear();
            Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            foreach (Renderer renderer in renderers)
            {
                if (GameObjectUtility.AreStaticEditorFlagsSet(renderer.gameObject, StaticEditorFlags.BatchingStatic) ||
                    GameObjectUtility.AreStaticEditorFlagsSet(renderer.gameObject, StaticEditorFlags.ContributeGI))
                {
                    foreach (Material material in renderer.sharedMaterials)
                    {
                        if (material != null && !UnityMaterials.Contains(material))
                        {
                            if (!material.shader.name.Contains("Orly Shader/LightMap Fusion"))
                            {
                                UnityMaterials.Add(material);
                            }
                        }
                    }
                }
            }
        }

        private void FindToolMaterials()
        {
            ToolMaterials.Clear();
            Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            foreach (Renderer renderer in renderers)
            {
                if (GameObjectUtility.AreStaticEditorFlagsSet(renderer.gameObject, StaticEditorFlags.BatchingStatic) ||
                    GameObjectUtility.AreStaticEditorFlagsSet(renderer.gameObject, StaticEditorFlags.ContributeGI))
                {
                    foreach (Material material in renderer.sharedMaterials)
                    {
                        if (material != null && !ToolMaterials.Contains(material))
                        {
                            if (material.shader.name.Contains("Orly Shader/LightMap Fusion"))
                            {
                                ToolMaterials.Add(material);
                            }
                        }
                    }
                }
            }
        }

        private void ConvertMaterials()
        {
            if (UnityMaterials.Count > 0)
            {
                for (int i = 0; i < UnityMaterials.Count; i++)
                {


                    Material m = UnityMaterials[i];
                    if (materialsRevertData.GetValue(sceneName + "." + m.name) == null)
                    {
                        materialsRevertData.AddValue(sceneName + "." + m.name, UnityMaterials[i].shader.name);
                    }
                    else if (materialsRevertData.GetValue(sceneName + "." + m.name) != UnityMaterials[i].shader.name)
                    {
                        materialsRevertData.AddValue(sceneName + "." + m.name, UnityMaterials[i].shader.name);
                    }
                    Shader s = UnityMaterials[i].shader;
                    if (RenderPipelineChecker.CheckRenderPipeline() == "Built-in")
                    {

                        if (s.name == "Standard" || s.name == "Mobile/Bumped Diffuse" || s.name == "Mobile/Bumped Specular")
                        {
                            UnityMaterials[i].shader = Shader.Find("Orly Shader/LightMap Fusion/Built-in/Standard");
                        }
                        else if (s.name.Contains("Unlit") || s.name == "Mobile/Diffuse")
                        {
                            UnityMaterials[i].shader = Shader.Find("Orly Shader/LightMap Fusion/Built-in/Unlit");
                        }
                    }
                    else if (RenderPipelineChecker.CheckRenderPipeline() == "URP")
                    {
                       
                        if (s.name == "Universal Render Pipeline/Lit")
                        {
                            UnityMaterials[i].shader = Shader.Find("Orly Shader/LightMap Fusion/URP/Lit");
                        }
                        else if (s.name == "Universal Render Pipeline/Unlit")
                        {
                            UnityMaterials[i].shader = Shader.Find("Orly Shader/LightMap Fusion/URP/Unlit");
                        }
                    }

                      
                    

                }
            }
            if (materialsRevertData != null && serializedRevertData != null)
            {
                materialsRevertData.CopyToList();
                serializedRevertData.ApplyModifiedProperties();
                EditorUtility.SetDirty(materialsRevertData);         // Marcar el objeto como "dirty" para que se guarde 
                AssetDatabase.SaveAssets();         // Guardar los cambios
            }
            FindToolMaterials();
            FindUnityMaterials();
        }

        private void RevertMaterials()
        {

            if (ToolMaterials.Count > 0)
            {
                for (int i = 0; i < ToolMaterials.Count; i++)
                {

                    Material m = ToolMaterials[i];
                    if (materialsRevertData.GetValue(sceneName + "." + m.name) != null)
                    {
                        ToolMaterials[i].shader = Shader.Find(materialsRevertData.GetValue(sceneName + "." + m.name));
                    }
                }
            }
            FindToolMaterials();
            FindUnityMaterials();
        }

        void GuiLine(Color color, int i_height = 1)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, i_height);

            rect.height = i_height;

            EditorGUI.DrawRect(rect, color);
        }

    }
}
#endif







