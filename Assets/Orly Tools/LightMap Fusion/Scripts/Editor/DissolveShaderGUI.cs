#if UNITY_EDITOR


namespace LightMapFusion
{
    using UnityEditor;
    using UnityEngine;


    public class CustomShaderGUI : ShaderGUI
    {
        private const string EVENT_UPDATE_VOLUMES = "Update Volumes";
        private const string EVENT_REMOVE_VOLUME = "Remove Volume";
        private const string EVENT_SELECT_VOLUME = "Select Volume";

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            for (int i = 0; i < properties.Length; i++)
            {
                if (properties[i].name == "_EnableDissolve")
                {
                    break;
                }
                EditorGUILayout.Space(5);
                if (properties[i].name.Contains("Set") && properties[i].name.Contains("Intensity"))
                {
                    continue;
                }
                if (properties[i].name.Equals("_EmissionColor"))
                {
                    continue;
                }
                materialEditor.ShaderProperty(properties[i], properties[i].displayName);
            }

            GUI.color = new Color32(0, 255, 255, 255);
            EditorGUILayout.Space(20);
            var style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 12
            };

            EditorGUILayout.LabelField("Dissolve Tools", style, GUILayout.ExpandWidth(true));

            EditorGUILayout.Space(20);

            MaterialProperty enbabledissolveProperty = FindProperty("_EnableDissolve", properties);
            materialEditor.ShaderProperty(enbabledissolveProperty, enbabledissolveProperty.displayName);
            EditorGUILayout.Space(10);
            GUI.color = new Color(1, 1, 1, 1);
            if (enbabledissolveProperty.floatValue == 1)
            {
                EditorGUILayout.Space(20);
                /*
                MaterialProperty invertProperty = FindProperty("_Invert", properties);
                materialEditor.ShaderProperty(invertProperty, invertProperty.displayName);
                EditorGUILayout.Space(5);
                MaterialProperty dissolveProperty = FindProperty("_Dissolve", properties);
                materialEditor.ShaderProperty(dissolveProperty, dissolveProperty.displayName);
                //MaterialProperty alphaThreshold = FindProperty("_AlphaThreshold", properties);
                //if (alphaThreshold != null)
                //{
                //    materialEditor.ShaderProperty(alphaThreshold, alphaThreshold.displayName);
                //}
                EditorGUILayout.Space(5);
                MaterialProperty edgeColorProperty = FindProperty("_EdgeColor", properties);
                materialEditor.ShaderProperty(edgeColorProperty, edgeColorProperty.displayName);
                EditorGUILayout.Space(5);
                MaterialProperty edgeThicknesProperty = FindProperty("_EdgeThickness", properties);
                materialEditor.ShaderProperty(edgeThicknesProperty, edgeThicknesProperty.displayName);

                EditorGUILayout.Space(5);
                MaterialProperty noiseScaleProperty = FindProperty("_NoiseScale", properties);
                materialEditor.ShaderProperty(noiseScaleProperty, noiseScaleProperty.displayName);

                EditorGUILayout.Space(5);
                MaterialProperty noiseScrollSpeedProperty = FindProperty(
                    "_NoiseScrollSpeed",
                    properties
                );
                materialEditor.ShaderProperty(
                    noiseScrollSpeedProperty,
                    noiseScrollSpeedProperty.displayName
                );
                */
                EditorGUILayout.Space(20);
                GUI.color = new Color32(0, 255, 255, 255);
                MaterialProperty maskSelectorProperty = FindProperty("_MaskSelector", properties);
                materialEditor.ShaderProperty(maskSelectorProperty, maskSelectorProperty.displayName);
                EditorGUILayout.Space(20);
                GUI.color = new Color(1, 1, 1, 1);
                /*
                switch (maskSelectorProperty.floatValue)
                {
                    case 0:
                        MaterialProperty planeDirectionProperty = FindProperty(
                            "_PlaneDirection",
                            properties
                        );
                        materialEditor.ShaderProperty(
                            planeDirectionProperty,
                            planeDirectionProperty.displayName
                        );
                        EditorGUILayout.Space(5);



                        break;
                    case 1:
                        MaterialProperty gizmoCenterProperty = FindProperty("_GizmoCenter", properties);
                        materialEditor.ShaderProperty(
                            gizmoCenterProperty,
                            gizmoCenterProperty.displayName
                        );
                        EditorGUILayout.Space(5);
                        MaterialProperty sphereRadiusProperty = FindProperty(
                            "_SphereRadius",
                            properties
                        );
                        materialEditor.ShaderProperty(
                            sphereRadiusProperty,
                            sphereRadiusProperty.displayName
                        );

                        break;
                    case 2:
                        MaterialProperty gizmoCenterProperty1 = FindProperty(
                            "_GizmoCenter",
                            properties
                        );
                        materialEditor.ShaderProperty(
                            gizmoCenterProperty1,
                            gizmoCenterProperty1.displayName
                        );
                        EditorGUILayout.Space(5);

                        MaterialProperty boxSizeProperty = FindProperty("_BoxSize", properties);
                        materialEditor.ShaderProperty(boxSizeProperty, boxSizeProperty.displayName);
                        //MaterialProperty boxRotation = FindProperty("_BoxRotation", properties);
                        //materialEditor.ShaderProperty(boxRotation, boxRotation.displayName);

                        break;
                }
                */
                EditorGUILayout.Space(20);

                //toolShaders = serializedObject.FindProperty("toolShaders");
                //string name = "";
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Create Controller"))
                {
                    if (!CheckVolumeID((int)FindProperty("_vID", properties).floatValue))
                    {
                        CreateVolume(
                            (SelectorMask)maskSelectorProperty.floatValue,
                            (int)FindProperty("_vID", properties).floatValue
                        );
                        UpdateVolumes();
                    }
                }
                // if (GUILayout.Button("Select Controller")) { }
                if (GUILayout.Button("Remove Controller"))
                {
                    RemoveVolume((int)FindProperty("_vID", properties).floatValue);
                    UpdateVolumes();
                }

                GUILayout.EndHorizontal();

                //EditorGUILayout.LabelField(volumeName, style, GUILayout.ExpandWidth(true));

                EditorGUILayout.Space(5);

                MaterialProperty vIDProperty = FindProperty("_vID", properties);

                materialEditor.ShaderProperty(vIDProperty, vIDProperty.displayName);
                if (GUILayout.Button("Update Controller"))
                {
                    UpdateVolumes();
                }

                //k = (DissolveController)
                //  EditorGUILayout.ObjectField("Controller: ", k, typeof(DissolveController), true);
            }
        }

        void UpdateVolumes()
        {
            DissolveController[] dcl = (DissolveController[])
                GameObject.FindObjectsByType<DissolveController>(FindObjectsSortMode.None);
            foreach (DissolveController dc in dcl)
            {
                if (dc)
                {
                    //dc.LoadDissolveShaders();
                    dc.AssigMaterialtoVolumes();
                    if (dc.volumeGizmo)
                    {
                        foreach (Material m in dc.volumeGizmo.dissolveVolMaterials)
                        {
                            m.SetFloat("_MaskSelector", (float)dc.volumeGizmo.typeID);
                            dc.volumeGizmo.UpdateVolume();
                        }
                    }
                }
            }
        }

        bool CheckVolumeID(int id)
        {
            DissolveController[] volumes = (DissolveController[])
                GameObject.FindObjectsByType<DissolveController>(FindObjectsSortMode.None);
            foreach (DissolveController r in volumes)
            {
                if (r.id == id)
                {
                    EditorUtility.DisplayDialog(
                        "Error",
                        "The current Disslove Controller ID ("
                            + id
                            + ") already exist.    Please Select another one",
                        "OK"
                    );
                    return true;
                }
            }

            return false;
        }

        public void CreateVolume(SelectorMask volumeType, int id)
        {
            string path = "Assets/Orly Tools/LightMap Fusion/Prefabs/DISSOLVE/Volumes/Dissolve Controller.prefab";
            GameObject result = null;
            switch (volumeType)
            {
                case SelectorMask.Sphere:
                    GameObject sphere = (GameObject)
                        AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));

                    GameObject sphereInst = PrefabUtility.InstantiatePrefab(sphere) as GameObject;
                    sphereInst.AddComponent<VolumeSphere>();
                    sphereInst.GetComponent<DissolveController>().id = id;
                    sphereInst.GetComponent<DissolveController>().selectorMask = volumeType;
                    sphereInst.GetComponent<DissolveController>().volumeGizmo =
                        sphereInst.GetComponent<Volume>();
                    //sphereInst.GetComponent<DissolveController>().AssigMaterialtoVolumes();
                    sphereInst.name = "Dissolve Controller Sphere";
                    result = sphereInst;
                    break;
                case SelectorMask.Box:
                    GameObject box = (GameObject)
                        AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
                    GameObject boxInst = PrefabUtility.InstantiatePrefab(box) as GameObject;
                    boxInst.AddComponent<VolumeBox>();

                    boxInst.GetComponent<DissolveController>().id = id;
                    boxInst.GetComponent<DissolveController>().selectorMask = volumeType;

                    boxInst.GetComponent<DissolveController>().volumeGizmo =
                        boxInst.GetComponent<Volume>();
                    //boxInst.GetComponent<DissolveController>().AssigMaterialtoVolumes();
                    boxInst.name = "Dissolve Controller Box";
                    result = boxInst;

                    break;
                case SelectorMask.Plane:
                    GameObject plane = (GameObject)
                        AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
                    GameObject planeInst = PrefabUtility.InstantiatePrefab(plane) as GameObject;
                    planeInst.AddComponent<VolumePlane>();
                    planeInst.GetComponent<DissolveController>().id = id;
                    planeInst.GetComponent<DissolveController>().selectorMask = volumeType;
                    planeInst.GetComponent<DissolveController>().volumeGizmo =
                        planeInst.GetComponent<Volume>();
                    //planeInst.GetComponent<DissolveController>().AssigMaterialtoVolumes();
                    planeInst.name = "Dissolve Controller Plane";
                    result = planeInst;

                    break;
            }
            if (result != null)
            {


                result.GetComponent<DissolveController>().toolShaders = PathUtils.AutoloadToolDissolveShaders();
                result.GetComponent<DissolveController>().AssigMaterialtoVolumes();
                Selection.activeObject = result;
            }
        }




        void RemoveVolume(int id)
        {
            DissolveController[] volumes = (DissolveController[])
                GameObject.FindObjectsByType<DissolveController>(FindObjectsSortMode.None);
            foreach (DissolveController r in volumes)
            {
                if (r.id == id)
                {
                    Volume v = r.volumeGizmo;
                    if (v != null)
                    {
                        if (v.dissolveVolMaterials != null && v.dissolveVolMaterials.Count > 0)
                        {
                            foreach (Material m in v.dissolveVolMaterials)
                            {
                                switch (v.typeID)
                                {
                                    case SelectorMask.Plane:
                                        m.SetFloat("_PlaneConstant", 0);
                                        m.SetVector("_GizmoCenter", new Vector3(0, 0, 0));
                                        m.SetVector("_PlaneDirection", new Vector3(0, 1, 0));
                                        break;
                                    case SelectorMask.Sphere:
                                        m.SetFloat("_SphereRadius", 0);
                                        m.SetVector("_GizmoCenter", new Vector3());
                                        break;
                                    case SelectorMask.Box:
                                        m.SetVector("_BoxSize", new Vector3(0.01f, 0.01f, 0.01f));
                                        m.SetVector("_GizmoCenter", new Vector3());
                                        break;
                                }
                            }
                        }
                    }

                    UnityEngine.Object.DestroyImmediate(r.gameObject);
                }
            }
        }
    }
}

#endif
