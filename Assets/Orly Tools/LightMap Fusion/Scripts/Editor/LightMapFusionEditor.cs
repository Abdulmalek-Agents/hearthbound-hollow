

#if UNITY_EDITOR


using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LightMapFusion
{



    [CustomEditor(typeof(LightMapFusion))]
    public class LightMapFusionEditor : Editor
    {
        private string[] tabs =
        {
        "Materials",
        "Light Sets",
        "Bake Lightmaps",
        "Skybox & Fog",
        "Reflection"
    };


        //[SerializeField]

        public SerializedProperty showPreBakeTools;

        public SerializedProperty blendValueX;
        public SerializedProperty blendValueY;
        public SerializedProperty LightmapAtlas;
        public SerializedProperty lightset;
        public SerializedProperty materials;

        public SerializedProperty skyboxMaterial;

        public SerializedProperty useComplexSkybox;

        private Color color = Color.gray;

        public SerializedProperty sceneFog;
        public SerializedProperty fogColor3;
        public SerializedProperty fogColor4;
        public SerializedProperty fogColor2;
        public SerializedProperty fogColor1;
        public SerializedProperty fogDensity3;
        public SerializedProperty fogDensity4;
        public SerializedProperty fogDensity2;
        public SerializedProperty fogDensity1;
        public SerializedProperty FogColor;
        public SerializedProperty FogDensity;

        public SerializedProperty skyColor1;
        public SerializedProperty skyColor2;
        public SerializedProperty skyColor3;
        public SerializedProperty skyColor4;
        public SerializedProperty skyBox1;
        public SerializedProperty skyBox2;
        public SerializedProperty skyBox3;
        public SerializedProperty skyBox4;
        public SerializedProperty skyboxRotation;
        public SerializedProperty skyboxExposure;

        public SerializedProperty toolShaders;
        public SerializedProperty RT_ReflectionProbe;
        public SerializedProperty UseDynamicReflectionProbe;
        public SerializedProperty probeUpdateRate;

        public SerializedProperty OriginalLightMaps;
        public SerializedProperty useAsyncBake;

        public SerializedProperty Set1Intensity;
        public SerializedProperty Set2Intensity;
        public SerializedProperty Set3Intensity;
        public SerializedProperty Set4Intensity;
        public SerializedProperty lightmapResolution;
        private Vector2 scrollPos;
        GUIContent LightIcon;
        GUIContent Logo;

        Color blueColor;

        //private enum LightmapperType { CPU, GPU }
        //private LightmapperType selectedLightmapper = LightmapperType.CPU;







        LightMapFusion tool;

        [MenuItem("Window/Orly Tools/LightMap Fusion/Initialize LightMap Fusion")]
        static void CreateTool()
        {
            Scene scene = SceneManager.GetActiveScene();
            GameObject[] oarr = scene.GetRootGameObjects();
            for (int i = 0; i < oarr.Length; i++)
            {
                if (oarr[i].GetComponentInChildren<LightMapFusion>())
                {
                    EditorUtility.DisplayDialog(
                        "Error",
                        "LightMap Fusion Tool already exist in the current scene",
                        "OK"
                    );
                    return;
                }
            }
            string path = "Assets/Orly Tools/LightMap Fusion/Prefabs/LIGHTMAP FUSION TOOL.prefab";


            GameObject root = (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
            //PrefabUtility.InstantiatePrefab(root);
            PrefabUtility.UnpackPrefabInstance((PrefabUtility.InstantiatePrefab(root) as GameObject), PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }

        public void OnEnable()
        {


            blueColor = new Color(0.185f, 0.7f, 1.0f, 1.0f);
            tool = target as LightMapFusion;
            showPreBakeTools = serializedObject.FindProperty("showPreBakeTools");
            blendValueX = serializedObject.FindProperty("blendValueX");
            blendValueY = serializedObject.FindProperty("blendValueY");
            LightmapAtlas = serializedObject.FindProperty("LightmapAtlas");
            lightset = serializedObject.FindProperty("lightset");
            materials = serializedObject.FindProperty("materials");

            sceneFog = serializedObject.FindProperty("sceneFog");

            fogColor1 = serializedObject.FindProperty("fogColor1");
            fogColor2 = serializedObject.FindProperty("fogColor2");
            fogColor3 = serializedObject.FindProperty("fogColor3");
            fogColor4 = serializedObject.FindProperty("fogColor4");
            fogDensity1 = serializedObject.FindProperty("fogDensity1");
            fogDensity2 = serializedObject.FindProperty("fogDensity2");
            fogDensity3 = serializedObject.FindProperty("fogDensity3");
            fogDensity4 = serializedObject.FindProperty("fogDensity4");

            skyColor1 = serializedObject.FindProperty("skyColor1");
            skyColor2 = serializedObject.FindProperty("skyColor2");
            skyColor3 = serializedObject.FindProperty("skyColor3");
            skyColor4 = serializedObject.FindProperty("skyColor4");
            skyBox1 = serializedObject.FindProperty("skyBox1");
            skyBox2 = serializedObject.FindProperty("skyBox2");
            skyBox3 = serializedObject.FindProperty("skyBox3");
            skyBox4 = serializedObject.FindProperty("skyBox4");
            skyboxRotation = serializedObject.FindProperty("skyboxRotation");
            skyboxExposure = serializedObject.FindProperty("skyboxExposure");

            FogColor = serializedObject.FindProperty("FogColor");
            FogDensity = serializedObject.FindProperty("FogDensity");

            skyboxMaterial = serializedObject.FindProperty("skyboxMaterial");
            useComplexSkybox = serializedObject.FindProperty("useComplexSkybox");

            OriginalLightMaps = serializedObject.FindProperty("OriginalLightMaps");
            toolShaders = serializedObject.FindProperty("toolShaders");
            RT_ReflectionProbe = serializedObject.FindProperty("RT_ReflectionProbe");
            UseDynamicReflectionProbe = serializedObject.FindProperty("UseDynamicReflectionProbe");
            probeUpdateRate = serializedObject.FindProperty("probeUpdateRate");
            useAsyncBake = serializedObject.FindProperty("useAsyncBake");


            Set1Intensity = serializedObject.FindProperty("Set1Intensity");
            Set2Intensity = serializedObject.FindProperty("Set2Intensity");
            Set3Intensity = serializedObject.FindProperty("Set3Intensity");
            Set4Intensity = serializedObject.FindProperty("Set4Intensity");
            LightIcon = EditorGUIUtility.IconContent("d_Lighting");
            Logo = EditorGUIUtility.IconContent("Assets/Orly Tools/LightMap Fusion/Textures/Icons/LightMap Fusion PRO Logo.png");








            tool.GetSkyboxTextures();
        }

        private void OnGUI() { }

        private void OnInspectorUpdate()
        {
            Repaint();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            BaseProperties();
            serializedObject.ApplyModifiedProperties();
        }

        private void BaseProperties()
        {



            EditorGUILayout.Space(10);
            var style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 18,
            };
            var style2 = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 12
            };
            style.normal.textColor = blueColor;


            EditorGUILayout.LabelField(Logo, style, GUILayout.Height(50));



            int buttonHeight = 30;

            EditorGUILayout.Space(10);
            GuiLine(blueColor, 2);
            EditorGUILayout.Space(5);


            if (Lightmapping.GetLightingSettingsForScene(SceneManager.GetActiveScene()) == null)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("The Lighting Settings is Empty for current scene");
                EditorGUILayout.LabelField("Please create a new Lighting Settings Asset");
                EditorGUILayout.Space(10);
                if (GUILayout.Button("Create New Lighting Settings Asset", GUILayout.Height(buttonHeight)))
                {
                    CreateLightingSettings();
                }
                EditorGUILayout.Space(40);

                return;
            }
            else
            {
                //Lightmapping.lightingSettings = (LightingSettings)EditorGUILayout.ObjectField("Light Settings Asset", Lightmapping.lightingSettings, typeof(LightingSettings), false);
            }

            style.fontSize = 14;
            //EditorGUILayout.LabelField("Tool Mode", style);
            EditorGUILayout.BeginHorizontal("Box");


            tool.toolMode = (ToolMode)EditorGUILayout.EnumPopup("Tool Mode", tool.toolMode);
            if (GUILayout.Button("Open Toolbar", GUILayout.Height(20)))
            {
                LMFToolbar.ShowPanel();
            }
            if (GUILayout.Button("Help", GUILayout.Height(20)))
            {
                Application.OpenURL("https://orly-3d.gitbook.io/unity-blend-lightmaps-tool/");
            }
            //LinkButton("Youtube Tutorial (Comming Soon)", "https://youtube.com");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(20);

            tool.tabIndex = GUILayout.Toolbar(tool.tabIndex, tabs);

            EditorGUILayout.Space(20);

            switch (tool.tabIndex)
            {
                case 0:

                    //EditorGUILayout.PropertyField(toolShaders);
                    EditorGUILayout.Space(20);
                    EditorGUILayout.BeginHorizontal("Box");
                    if (GUILayout.Button("Load Materials from Scene", GUILayout.Height(buttonHeight)))
                    {
                        tool.toolShaders = PathUtils.AutoloadToolShaders();
                        tool.AutoLoadMaterials();
                    }
                    if (GUILayout.Button("Material Converter", GUILayout.Height(buttonHeight)))
                    {
                        LMFMaterialConverter.ShowWindow();
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(20);
                    EditorGUILayout.BeginHorizontal("Box", GUILayout.MaxHeight(200));

                    scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                    EditorGUILayout.PropertyField(materials);
                    EditorGUILayout.EndScrollView();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(10);
                    if (GUILayout.Button("Update Materials", GUILayout.Height(buttonHeight)))
                    {
                        tool.DisableAllLightSets();
                    }

                    EditorGUILayout.Space(20);
                    GuiLine(color, 2);

                    break;
                case 1:
                    EditorGUILayout.BeginVertical("Box");
                    EditorGUILayout.Space(5);
                    GUILayout.Label("Manage Light Sets", style2);
                    EditorGUILayout.PropertyField(lightset);
                    EditorGUILayout.Space(5);



                    EditorGUILayout.BeginHorizontal("Box");
                    int labelOffset = (int)EditorGUIUtility.currentViewWidth / 8 - 40;
                    int sliderOffset = (int)EditorGUIUtility.currentViewWidth / 8 - 10;
                    int labelWidth = 120;
                    int count = tool.toolMode == ToolMode.Two ? 2 : 4;
                    if (count == 2)
                    {
                        labelOffset = (int)EditorGUIUtility.currentViewWidth / 4 - 50;
                        sliderOffset = (int)EditorGUIUtility.currentViewWidth / 4 - 20;
                        labelWidth = 120;

                    }

                    for (int i = 0; i < count; i++)
                    {
                        EditorGUILayout.BeginVertical();


                        switch (i)
                        {
                            case 0:

                                EditorGUILayout.BeginHorizontal();
                                AddInitiaMargin1(sliderOffset);
                                Set1Intensity.floatValue = GUILayout.VerticalSlider(Set1Intensity.floatValue, 1, 0, GUILayout.Height(100), GUILayout.Width(50));
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.Space(5);
                                EditorGUILayout.BeginHorizontal();
                                AddInitiaMargin1(labelOffset);
                                EditorGUILayout.LabelField("Gain  " + Set1Intensity.floatValue.ToString("F2"), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                                EditorGUILayout.EndHorizontal();
                                break;
                            case 1:
                                EditorGUILayout.BeginHorizontal();
                                AddInitiaMargin1(sliderOffset);
                                Set2Intensity.floatValue = GUILayout.VerticalSlider(Set2Intensity.floatValue, 1, 0, GUILayout.Height(100), GUILayout.Width(50));
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.Space(5);
                                EditorGUILayout.BeginHorizontal();
                                AddInitiaMargin1(labelOffset);
                                EditorGUILayout.LabelField("Gain  " + Set2Intensity.floatValue.ToString("F2"), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                                EditorGUILayout.EndHorizontal();
                                break;
                            case 2:
                                EditorGUILayout.BeginHorizontal();
                                AddInitiaMargin1(sliderOffset);
                                Set3Intensity.floatValue = GUILayout.VerticalSlider(Set3Intensity.floatValue, 1, 0, GUILayout.Height(100), GUILayout.Width(50));
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.Space(5);
                                EditorGUILayout.BeginHorizontal();
                                AddInitiaMargin1(labelOffset);
                                EditorGUILayout.LabelField("Gain  " + Set3Intensity.floatValue.ToString("F2"), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                                EditorGUILayout.EndHorizontal();
                                break;
                            case 3:
                                EditorGUILayout.BeginHorizontal();
                                AddInitiaMargin1(sliderOffset);
                                Set4Intensity.floatValue = GUILayout.VerticalSlider(Set4Intensity.floatValue, 1, 0, GUILayout.Height(100), GUILayout.Width(50));
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.Space(5);
                                EditorGUILayout.BeginHorizontal();
                                AddInitiaMargin1(labelOffset);
                                EditorGUILayout.LabelField("Gain  " + Set4Intensity.floatValue.ToString("F2"), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
                                EditorGUILayout.EndHorizontal();
                                break;
                            default:
                                break;
                        }
                        EditorGUILayout.Space(10);

                        if (GUILayout.Button(new GUIContent("Light Set " + (i + 1), LightIcon.image)))
                        {
                            tool.ToggleLightSet(i);
                        }


                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(20);
                    EditorGUILayout.BeginVertical("Box");
                    GUILayout.Label("Light Set Bake Tools", style2);
                    EditorGUILayout.PropertyField(showPreBakeTools);
                    EditorGUILayout.Space(5);
                    EditorGUILayout.BeginHorizontal();
                    for (int i = 0; i < count; i++)
                    {
                        EditorGUILayout.BeginVertical();
                        if (tool.showPreBakeTools)
                        {
                            GUI.enabled = false;
                            if (Lightmapping.isRunning)
                            {
                                GUI.enabled = false;
                            }
                            else
                            {
                                GUI.enabled = true;
                            }
                            if (GUILayout.Button("Pre-Bake " + (i + 1)))
                            {
                                tool.BakePreviewLightmap(i);
                            }
                            GUI.enabled = true;
                        }
                        EditorGUILayout.Space(5);
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();


                    break;
                case 2:
                    var styleB = new GUIStyle(GUI.skin.button);
                    styleB.normal.textColor = Color.white;
                    EditorGUILayout.Space(5);
                    GUI.color = new Color(1, 1, 1, 1);
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Lightmaps", EditorStyles.boldLabel);
                    EditorGUILayout.Space(10);
                    scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                    EditorGUILayout.BeginHorizontal("Box", GUILayout.MaxHeight(150));

                    //EditorGUILayout.PropertyField(LightmapAtlas);
                    int tumbSize = 100;
                    for (int i = 0; i < tool.LightmapAtlas.Count; i++)
                    {
                        EditorGUILayout.BeginVertical();
                        SerializedProperty imageProp =  LightmapAtlas.GetArrayElementAtIndex(i);
                        Texture2D texture = (Texture2D)imageProp.objectReferenceValue;
                        if (texture != null)
                        {
                            if (GUILayout.Button(texture, GUILayout.Width(tumbSize), GUILayout.Height(tumbSize)))
                            {
                                EditorGUIUtility.PingObject(imageProp.objectReferenceValue);
                            }
                            EditorGUILayout.LabelField("Index: " + i, GUILayout.Width(tumbSize));
                            Vector2 _imageSize = tool.LightmapAtlas[i].Size();
                            EditorGUILayout.LabelField("Size: " + (int)_imageSize.x + "x" + (int)_imageSize.x, GUILayout.Width(tumbSize));
                        }
                        EditorGUILayout.EndVertical();
                    }

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndScrollView();

                    EditorGUILayout.Space(10);






                    EditorGUILayout.PropertyField(useAsyncBake);
                    // Obtener los nombres y valores del enum 
                    string[] names = System.Enum.GetNames(typeof(LightmapResolution));
                    int[] values = (int[])System.Enum.GetValues(typeof(LightmapResolution));
                    string[] opciones = new string[values.Length];
                    for (int i = 0; i < names.Length; i++)
                    {
                        opciones[i] = $"{values[i]}";
                    }
                    int index = System.Array.IndexOf(values, (int)tool.lightmapResolution);
                    index = EditorGUILayout.Popup("Lightmap Size", index, opciones);
                    tool.lightmapResolution = (LightmapResolution)values[index];

                    EditorGUILayout.Space(10);
                    //ightmapping.lightingSettings.lightmapper = (Lightmapper)EditorGUILayout.EnumPopup("Lightmapper", Lightmapping.lightingSettings.lightmapper);
                    Lightmapping.lightingSettings.ao = EditorGUILayout.Toggle("Ambient Occlusion", Lightmapping.lightingSettings.ao);

                    if (Lightmapping.lightingSettings.ao)
                    {
                        Lightmapping.lightingSettings.aoMaxDistance = EditorGUILayout.FloatField("Max Distance", Lightmapping.lightingSettings.aoMaxDistance);
                    }

                    //GUI.color = new Color32(0, 255, 255, 255);
                    EditorGUILayout.Space(10);

                    EditorGUILayout.BeginHorizontal("Box");
                    if (!Lightmapping.isRunning || tool.progress == 0)
                    {
                        if (GUILayout.Button("Bake Lightmaps", GUILayout.Height(buttonHeight)))
                        {
                            tool.BakeLightmaps();
                        }
                    }
                    else
                    {
                        //GUI.color = new Color32(255, 128, 50, 255);
                        if (GUILayout.Button("Cancel", GUILayout.Height(buttonHeight)))
                        {
                            tool.CancelLightmapBaking();
                            tool.progress = 0;
                        }
                    }
                    if (
                        GUILayout.Button(
                            "Remove Lightmaps",
                            styleB,
                            GUILayout.Height(buttonHeight)
                        )
                    )
                    {
                        tool.RemoveAditionalLightmap();
                        tool.progress = 0;
                    }
                    EditorGUILayout.EndHorizontal();
                    GUI.color = new Color(1, 1, 1, 1);



                    //#endif
                    EditorGUILayout.Space(10);
                    break;
                case 4:
                    EditorGUILayout.Space(10);
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.PropertyField(UseDynamicReflectionProbe);
                    EditorGUILayout.Space(10);
                    if (GUILayout.Button("Create Reflection Probe"))
                    {
                        tool.CreateDynamicReflectionProbe();
                    }
                    EditorGUILayout.Space(10);

                    if (tool.UseDynamicReflectionProbe)
                    {
                        EditorGUILayout.Space(10);
                        EditorGUILayout.PropertyField(probeUpdateRate);
                        EditorGUILayout.Space(10);
                        EditorGUILayout.PropertyField(RT_ReflectionProbe);
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(10);

                    break;

                case 3:

                    EditorGUILayout.Space(5);
                    EditorGUILayout.PropertyField(useComplexSkybox);
                    if (GUILayout.Button("Create new Skybox", GUILayout.Height(buttonHeight)))
                    {
                        Material skyboxMaterial;
                        if (tool.toolMode == ToolMode.Four)
                        {
                            if (tool.useComplexSkybox == true)
                            {
                                skyboxMaterial = new Material(Shader.Find("Orly Shader/LightMap Fusion/Skybox/Built-Iin/Blend Skybox 4"));
                            }
                            else
                            {
                                skyboxMaterial = new Material(Shader.Find("Orly Shader/LightMap Fusion/Skybox/Built-Iin/Blend Skybox Colors 4"));
                            }
                        }
                        else
                        {
                            if (tool.useComplexSkybox == true)
                            {
                                skyboxMaterial = new Material(Shader.Find("Orly Shader/LightMap Fusion/Skybox/Built-Iin/Blend Skybox 2"));
                            }
                            else
                            {
                                skyboxMaterial = new Material(Shader.Find("Orly Shader/LightMap Fusion/Skybox/Built-Iin/Blend Skybox Colors 2"));
                            }
                        }
                        string path = EditorUtility.SaveFilePanelInProject("Save new Material", "New Skybox Material", "mat", "Select location to save the material");

                        if (!string.IsNullOrEmpty(path))
                        {
                            AssetDatabase.CreateAsset(skyboxMaterial, path);
                            AssetDatabase.SaveAssets();
                            EditorUtility.DisplayDialog("Success", "New Material saved in: " + path, "OK");
                            tool.skyboxMaterial = skyboxMaterial;
                        }
                    }
                    EditorGUILayout.Space(5);


                    EditorGUILayout.PropertyField(skyboxMaterial);
                    if (skyboxMaterial != null)
                    {
                        EditorGUILayout.Space(10);
                        if (tool.skyboxMaterial)
                        {
                            if (
                                tool.skyboxMaterial.shader.name
                                == "Orly Shader/LightMap Fusion/Skybox/Built-Iin/Blend Skybox 4" || tool.skyboxMaterial.shader.name
                                == "Orly Shader/LightMap Fusion/Skybox/Built-Iin/Blend Skybox 2"
                            )
                            {
                                EditorGUILayout.BeginHorizontal(
                                    "Box",
                                    GUILayout.MaxWidth(288),
                                    GUILayout.MaxHeight(72)
                                );

                                //EditorGUILayout.Space(((EditorGUIUtility.currentViewWidth - 288) / 2), false);
                                //GUILayout.ExpandHeight(false);


                                AddInitiaMargin(288);

                                int previewSize = 72;
                                skyBox1.objectReferenceValue = EditorGUILayout.ObjectField(
                                    skyBox1.objectReferenceValue,
                                    typeof(Cubemap),
                                    false,
                                    GUILayout.Width(previewSize),
                                    GUILayout.Height(previewSize)
                                );

                                skyBox2.objectReferenceValue = EditorGUILayout.ObjectField(
                                    skyBox2.objectReferenceValue,
                                    typeof(Cubemap),
                                    false,
                                    GUILayout.Width(previewSize),
                                    GUILayout.Height(previewSize)
                                );
                                if (tool.toolMode.Equals(ToolMode.Four))
                                {
                                    skyBox3.objectReferenceValue = EditorGUILayout.ObjectField(
                                        skyBox3.objectReferenceValue,
                                        typeof(Cubemap),
                                        false,
                                        GUILayout.Width(previewSize),
                                        GUILayout.Height(previewSize)
                                    );

                                    skyBox4.objectReferenceValue = EditorGUILayout.ObjectField(
                                        skyBox4.objectReferenceValue,
                                        typeof(Cubemap),
                                        false,
                                        GUILayout.Width(previewSize),
                                        GUILayout.Height(previewSize)
                                    );
                                }
                                AddInitiaMargin(288);

                                EditorGUILayout.EndHorizontal();
                            }

                            EditorGUILayout.Space(10);

                            EditorGUILayout.BeginVertical();
                            EditorGUILayout.PropertyField(skyColor1);
                            EditorGUILayout.PropertyField(skyColor2);

                            if (tool.toolMode.Equals(ToolMode.Four))
                            {
                                EditorGUILayout.PropertyField(skyColor3);
                                EditorGUILayout.PropertyField(skyColor4);
                            }
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.Space(5);
                            EditorGUILayout.PropertyField(skyboxExposure);
                            EditorGUILayout.Space(5);
                            EditorGUILayout.PropertyField(skyboxRotation);
                            EditorGUILayout.Space(5);
                            GUILayout.Label("Sky Color:", style2);

                            EditorGUILayout.Space(5);
                            EditorGUILayout.BeginHorizontal("Box");
                            //GUI.color = new Color32(0, 255, 255, 255);
                            if (GUILayout.Button("Set Skybox", GUILayout.Height(buttonHeight)))
                            {
                                tool.SetSkyboxTextures();

                                RenderSettings.skybox = tool.skyboxMaterial;
                            }
                            //GUI.color = new Color32(255, 128, 50, 255);
                            if (GUILayout.Button("Remove Skybox", GUILayout.Height(buttonHeight)))
                            {
                                RenderSettings.skybox = null;
                            }
                            GUI.color = new Color(1, 1, 1, 1);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.Space(5);
                            GUILayout.Label("Current Sky Color:", style2);
                            GuiLine(tool.CurrentSkyColor, 15);
                        }
                    }

                    EditorGUILayout.Space(10);
                    EditorGUILayout.BeginVertical("Box");
                    EditorGUILayout.Space(5);
                    GUILayout.Label("Manage Fog", EditorStyles.boldLabel);

                    EditorGUILayout.PropertyField(sceneFog);

                    if (tool.sceneFog)
                    {
                        EditorGUILayout.BeginHorizontal("Box");
                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.PropertyField(fogColor1);
                        EditorGUILayout.PropertyField(fogColor2);
                        if (tool.toolMode.Equals(ToolMode.Four))
                        {
                            EditorGUILayout.PropertyField(fogColor3);
                            EditorGUILayout.PropertyField(fogColor4);
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.PropertyField(fogDensity1);
                        EditorGUILayout.PropertyField(fogDensity2);
                        if (tool.toolMode.Equals(ToolMode.Four))
                        {
                            EditorGUILayout.PropertyField(fogDensity3);
                            EditorGUILayout.PropertyField(fogDensity4);
                        }
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.Space(5);
                        GUILayout.Label("Current Fog Color:", style2);
                        GuiLine(tool.FogColor, 15);
                        EditorGUILayout.Space(10);
                        GUILayout.Label("Current Fog Density: " + tool.FogDensity, style2);
                    }
                    EditorGUILayout.EndVertical();
                    break;
                case 5:
                    return;
                default:
                    break;
            }

            EditorGUILayout.Space(20);
            GuiLine(color, 2);

            EditorGUILayout.Space(10);

            if (tool.progress > 0)
            {
                Rect rect = GUILayoutUtility.GetRect(18, 18, "TextField");
                EditorGUI.ProgressBar(rect, tool.progress / 100.0f, " Baking Progress: " + (int)tool.progress + "%");
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical("Box");
            var style1 = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 14
            };
            /*var style3 = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };*/
            style1.normal.textColor = blueColor;
            EditorGUILayout.LabelField("Blend Lightmaps", style1, GUILayout.ExpandWidth(true), GUILayout.Height(20));
            EditorGUILayout.Space(5);
            style1.normal.textColor = Color.white;
            //EditorGUILayout.BeginHorizontal("Box");
            //EditorGUILayout.LabelField("Light Set 1", style3, GUILayout.Width(120));
            //blendValueX.floatValue = GUILayout.HorizontalSlider(blendValueX.floatValue, 1, 0);
            EditorGUILayout.PropertyField(blendValueX);
            //EditorGUILayout.LabelField("Light Set 2", style3, GUILayout.Width(120));
            //EditorGUILayout.EndHorizontal();
            if (tool.toolMode == ToolMode.Four)
            {
                //EditorGUILayout.BeginHorizontal("Box");
                //EditorGUILayout.LabelField("Light Set 4", style3, GUILayout.Width(120));
                //blendValueY.floatValue = GUILayout.HorizontalSlider(blendValueY.floatValue, 1, 0);
                EditorGUILayout.PropertyField(blendValueY);
                //EditorGUILayout.LabelField("Light Set 3", style3, GUILayout.Width(120));
                //EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal("Box");
            EditorGUILayout.EndHorizontal();

            GuiLine(blueColor, 2);
            tool.BlendLightMaps();
            tool.EnableSceneFog();
            tool.BlendFog();
            tool.BlendSky();
            tool.UpdateReflectionProbe();
            Lightmapping.lightingSettings.lightmapMaxSize = (int)tool.lightmapResolution / 2;
            Lightmapping.lightingSettings.lightmapper = LightingSettings.Lightmapper.ProgressiveGPU;

        }

        void GuiLine(Color color, int i_height = 1)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, i_height);

            rect.height = i_height;

            EditorGUI.DrawRect(rect, color);
        }

        void AddInitiaMargin(float bodyWidth)
        {
            EditorGUI.DrawRect(
                EditorGUILayout.GetControlRect(
                    false,
                    0,
                    GUILayout.Width(((EditorGUIUtility.currentViewWidth - bodyWidth) / 2))
                ),
                new Color(0, 0, 0, 0)
            );
        }
        void AddInitiaMargin1(float bodyWidth)
        {
            EditorGUI.DrawRect(
                EditorGUILayout.GetControlRect(
                    false,
                    0,
                    GUILayout.Width((bodyWidth))
                ),
                new Color(0, 0, 0, 0)
            );
        }
        private void LinkButton(string caption, string url)
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 18
            };
            style.richText = true;
            style.normal.textColor = new Color(0, 3f, 0.5f, 0.8f);

            bool bClicked = GUILayout.Button(caption, style);

            var rect = GUILayoutUtility.GetLastRect();
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            if (bClicked)
                UnityEngine.Application.OpenURL(url);
        }

        public void CreateLightingSettings()
        {
            LightingSettings lightingSettings = new LightingSettings();
            string path = EditorUtility.SaveFilePanelInProject("Save new LightingSettings", "New LightingSettings", "lighting", "Select location to save the LightingSettings");
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(lightingSettings, path);
                AssetDatabase.SaveAssets();
                EditorUtility.DisplayDialog("Success", "New LightingSettings saved in: " + path, "OK");
                Lightmapping.lightingSettings = lightingSettings;
            }
        }

    }


}

#endif
