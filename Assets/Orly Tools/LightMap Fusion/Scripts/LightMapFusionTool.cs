using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace LightMapFusion
{


    public enum LightmapResolution { option1 = 256, option2 = 512, option3 = 1024, option4 = 2048, option5 = 4096, option6 = 8192 }

    public enum ToolMode { Two = 2, Four = 4 }

    public class LightMapFusion : MonoBehaviour
    {
        public LightmapResolution lightmapResolution = LightmapResolution.option4;

        public bool showPreBakeTools = false;
        public int tabIndex = 0;
        public ToolMode toolMode = ToolMode.Four;

        public List<Material> materials;
        public List<Texture2D> LightmapAtlas;
        public Transform[] lightset;

        [Range(0, 1)]
        public float blendValueX;
        [Range(0, 1)]
        public float blendValueY;
        public Renderer[] renderers;
        public int activeLigthSetIndex = 0;
        public List<Texture2D> OriginalLightMaps;
        public List<Shader> toolShaders;
        public ReflectionProbe RT_ReflectionProbe;
        public bool UseDynamicReflectionProbe = false;
        public float old_blendValueX = 0;
        public float old_blendValueY = 0;
        [Range(0.01f, 0.1f)]
        public float probeUpdateRate = 0.05f;
        public bool sceneFog = false;
        public Color fogColor1 = Color.black;
        public Color fogColor2 = Color.black;
        public Color fogColor3 = Color.black;
        public Color fogColor4 = Color.black;
        public float fogDensity1 = 0.01f;
        public float fogDensity2 = 0.01f;
        public float fogDensity3 = 0.01f;
        public float fogDensity4 = 0.01f;
        public Color FogColor;
        [Range(0, 1)]
        public float FogDensity;
        public Color skyColor1 = Color.grey;
        public Color skyColor2 = Color.grey;
        public Color skyColor3 = Color.grey;
        public Color skyColor4 = Color.grey;
        public Cubemap skyBox1;
        public Cubemap skyBox2;
        public Cubemap skyBox3;
        public Cubemap skyBox4;
        public Material skyboxMaterial;
        public bool useComplexSkybox = false;
        [Range(0, 360)]
        public float skyboxRotation;
        [Range(0, 8)]
        public float skyboxExposure;
        public Color CurrentSkyColor;
        public bool useAsyncBake = true;


        public float progress = 0;
        [Range(0, 1)]
        public float Set1Intensity = 1;
        [Range(0, 1)]
        public float Set2Intensity = 1;
        [Range(0, 1)]
        public float Set3Intensity = 1;
        [Range(0, 1)]
        public float Set4Intensity = 1;
        private void Start()
        {
            UpdateReflectionProbe();
        }



        public void BlendLightMaps()
        {
            Shader.SetGlobalFloat("_BlendLightmapX", blendValueX);
            Shader.SetGlobalFloat("_BlendLightmapY", blendValueY);
            Shader.SetGlobalFloat("_Set1Intensity", Set1Intensity);
            Shader.SetGlobalFloat("_Set2Intensity", Set2Intensity);
            Shader.SetGlobalFloat("_Set3Intensity", Set3Intensity);
            Shader.SetGlobalFloat("_Set4Intensity", Set4Intensity);
        }

        public void UpdateReflectionProbe()
        {
            if (RT_ReflectionProbe == null)
            {
                UseDynamicReflectionProbe = false;
                return;
            }
            if (UseDynamicReflectionProbe)
            {
                if (
                    Mathf.Abs(old_blendValueX - blendValueX) > probeUpdateRate
                    || Mathf.Abs(old_blendValueY - blendValueY) > probeUpdateRate
                )
                {
                    if (RT_ReflectionProbe != null)
                    {
                        if (
                            RT_ReflectionProbe.mode == ReflectionProbeMode.Realtime
                            && RT_ReflectionProbe.refreshMode == ReflectionProbeRefreshMode.ViaScripting
                        )
                        {
                            RT_ReflectionProbe.RenderProbe();
                        }
                    }
                    old_blendValueX = blendValueX;
                    old_blendValueY = blendValueY;
                }
            }
        }

        public void BlendFog()
        {
            if (sceneFog)
            {
                Color tempColor1 = Color.Lerp(fogColor3, fogColor4, blendValueX);
                Color tempColor2 = Color.Lerp(fogColor2, fogColor1, blendValueX);

                FogColor = Color.Lerp(tempColor1, tempColor2, blendValueY);

                float tempDensity1 = Mathf.Lerp(fogDensity3, fogDensity4, blendValueX);
                float tempDensity2 = Mathf.Lerp(fogDensity2, fogDensity1, blendValueX);

                FogDensity = Mathf.Lerp(tempDensity1, tempDensity2, blendValueY);

                RenderSettings.fogColor = FogColor;
                RenderSettings.fogDensity = FogDensity;
            }
        }

        public void BlendSky()
        {
            if (skyboxMaterial != null)
            {
                skyboxMaterial.SetFloat("_blendSkyX", blendValueX);
                skyboxMaterial.SetFloat("_blendSkyY", blendValueY);

                skyboxMaterial.SetColor("_Color1", skyColor1);
                skyboxMaterial.SetColor("_Color2", skyColor2);
                skyboxMaterial.SetColor("_Color3", skyColor3);
                skyboxMaterial.SetColor("_Color4", skyColor4);
                skyboxMaterial.SetFloat("_Rotation", skyboxRotation);
                skyboxMaterial.SetFloat("_Exposure", skyboxExposure);
            }

            Color tempColor1 = Color.Lerp(skyColor1, skyColor2, blendValueX);
            Color tempColor2 = Color.Lerp(skyColor3, skyColor4, blendValueX);

            CurrentSkyColor = Color.Lerp(tempColor1, tempColor2, blendValueY);
        }

        public void SetSkyboxTextures()
        {
            if (skyboxMaterial != null)
            {
                if (skyBox1)
                {
                    skyboxMaterial.SetTexture("_Skybox1", skyBox1);
                }
                if (skyBox2)
                {
                    skyboxMaterial.SetTexture("_Skybox2", skyBox2);
                }
                if (skyBox3)
                {
                    skyboxMaterial.SetTexture("_Skybox3", skyBox3);
                }
                if (skyBox4)
                {
                    skyboxMaterial.SetTexture("_Skybox4", skyBox4);
                }
            }
        }

        public void GetSkyboxTextures()
        {
            if (skyboxMaterial != null)
            {
                if (skyBox1 == null)
                {
                    skyBox1 = skyboxMaterial.GetTexture("_Skybox1") as Cubemap;
                }
                if (useComplexSkybox == true)
                {
                    if (skyBox2 == null)
                    {
                        skyBox2 = skyboxMaterial.GetTexture("_Skybox2") as Cubemap;
                    }
                    if (skyBox3 == null)
                    {
                        skyBox3 = skyboxMaterial.GetTexture("_Skybox3") as Cubemap;
                    }
                    if (skyBox4 == null)
                    {
                        skyBox4 = skyboxMaterial.GetTexture("_Skybox4") as Cubemap;
                    }
                }
            }
        }

        public Transform GetLightSet(int index)
        {
            return lightset[index];
        }


        public void ToggleLightSet(int index)
        {
            if (lightset.Length == 0 || lightset[index] == null)
                return;
            DisableAllLightSets();
            lightset[index].gameObject.SetActive(true);
            activeLigthSetIndex = index;

            switch (index)
            {
                case 0:
                    blendValueX = 1;
                    blendValueY = 1;
                    break;
                case 1:
                    blendValueX = 0;
                    blendValueY = 1;
                    break;
                case 2:
                    blendValueX = 0;
                    blendValueY = 0;
                    break;
                case 3:
                    blendValueX = 1;
                    blendValueY = 0;
                    break;
            }
            BlendLightMaps();
            BlendSky();
            BlendFog();
        }

        public bool checkLightSets()
        {
            int count = toolMode == ToolMode.Two ? 2 : 4;
            if (lightset.Length < 0)
            {
                return false;
            }
            for (int i = 0; i < count; i++)
            {
                if (lightset[i] == null)
                {
                    return false;
                }
            }
            return true;
        }

        public void DisableAllLightSets()
        {
            foreach (Transform t in lightset)
            {
                t.gameObject.SetActive(false);
            }
        }

#if UNITY_EDITOR

        public void CreateDynamicReflectionProbe()
        {
            GameObject o = new GameObject();
            o.name = "Dynamic Reflection Probe";
            o.AddComponent<ReflectionProbe>();
            ReflectionProbe probe = o.GetComponent<ReflectionProbe>();
            probe.mode = ReflectionProbeMode.Realtime;
            probe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
            if (RT_ReflectionProbe)
            {
                DestroyImmediate(RT_ReflectionProbe.gameObject);
            }
            PrefabUtility.InstantiatePrefab(o);
            o.transform.SetParent(transform.parent);
            RT_ReflectionProbe = o.GetComponent<ReflectionProbe>();
        }

        public void RemoveAllLightmaps()
        {
            RemoveOriginalLightMaps();
            RemoveAditionalLightmap();
            AssetDatabase.Refresh();
        }

        public void RemoveOriginalLightMaps()
        {
            if (OriginalLightMaps.Count > 0)
            {
                for (int i = 0; i < OriginalLightMaps.Count; i++)
                {
                    var path = AssetDatabase.GetAssetPath(OriginalLightMaps[i]);
                    if (path != null && path != "")
                    {
                        FileUtil.DeleteFileOrDirectory(path);
                    }
                }
            }

            OriginalLightMaps.Clear();

        }

        public void RemoveAditionalLightmap()
        {
            if (LightmapAtlas.Count > 0)
            {
                for (int i = 0; i < LightmapAtlas.Count; i++)
                {
                    var path = AssetDatabase.GetAssetPath(LightmapAtlas[i]);
                    FileUtil.DeleteFileOrDirectory(path);
                }
            }

            LightmapAtlas.Clear();
            Lightmapping.Clear();

        }

        public void DuplicateLightmap(int index)
        {
            string scenePath = SceneManager.GetActiveScene().path;
            string filename = SceneManager.GetActiveScene().name;
            string folder = PathUtils.cleanScenePath(scenePath);
            MyTextureImporter TEXTUREIMPORTER = new MyTextureImporter();

            int i = 0;
            string[] files0 = Directory.GetFiles(folder, "*.exr", SearchOption.TopDirectoryOnly);
            foreach (var file in files0)
            {
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(file);
                if (tex != null && tex.name.Contains("Lightmap-"))
                {
                    string path = folder + "/Lightmap-" + i + "_comp_light.exr";
                    string path1 = folder + "/Aditional Lighmaps/Lightmap-" + i + "_comp_light.exr";
                    string newPath = path1.Insert(path1.Length - 4, " " + index);
                    AssetDatabase.CopyAsset(path, newPath);
                    var lm = (Texture2D)AssetDatabase.LoadAssetAtPath(newPath, typeof(Texture2D));
                    TEXTUREIMPORTER.ImportTexture(lm);
                    if (lm == null)
                    {
                        break;
                    }
                    i++;
                }
            }
            AssetDatabase.Refresh();
        }



        public void AutoloadLightmapAtlas()
        {
            string scenePath = SceneManager.GetActiveScene().path;
            string folder = PathUtils.cleanScenePath(scenePath);
            string[] files = Directory.GetFiles(folder, "*.exr", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                if (File.Exists(file))
                {
                    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(file);
                    if (tex != null && tex.name.Contains("Lightmap"))
                    {
                        LightmapAtlas.Add(tex);
                    }
                }
            }
        }



        public void AutoLoadMaterials()
        {
            materials.Clear();
            renderers = GameObject.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            foreach (Renderer r in renderers)
            {
                if (GameObjectUtility.AreStaticEditorFlagsSet(r.gameObject, StaticEditorFlags.BatchingStatic) ||
                    GameObjectUtility.AreStaticEditorFlagsSet(r.gameObject, StaticEditorFlags.ContributeGI))
                {
                    for (int i = 0; i < toolShaders.Count; i++)
                    {
                        if (r.sharedMaterial.shader.name == toolShaders[i].name)
                        {
                            Material m = r.sharedMaterial;
                            bool exist = false;
                            foreach (Material mat in materials)
                            {
                                if (mat == m)
                                {
                                    exist = true;
                                    break;
                                }
                            }
                            if (!exist)
                            {
                                materials.Add(m);
                            }
                            break;
                        }
                    }
                }
            }
        }

        public void FinishBakeProcess()
        {
            AssetDatabase.Refresh();
            TextureAtlasGenerator atlasGenerator = new TextureAtlasGenerator();
            Scene scene = SceneManager.GetActiveScene();
            string scenePath = scene.path;
            string folder = PathUtils.cleanScenePath(scenePath);

            string[] files = Directory.GetFiles(
                folder + "/Aditional Lighmaps",
                "*.exr",
                SearchOption.TopDirectoryOnly
            );
            OriginalLightMaps.Clear();
            foreach (var file in files)
            {
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(file);
                OriginalLightMaps.Add(tex);
            }
            DisableAllLightSets();
            AssetDatabase.Refresh();
            atlasGenerator.GenerateTextureAtlas(OriginalLightMaps, (int)toolMode);
            LightmapAtlas.Clear();
            AutoloadLightmapAtlas();
            RemoveOriginalLightMaps();
            PathUtils.RemoveTemporalDataFolder(folder);
            ToggleLightSet(0);
            AssetDatabase.Refresh();
            ToggleViewerLighting(true);
        }

        public void FinishBakePreviewProcess(int index)
        {
            AssetDatabase.Refresh();
            TextureAtlasGenerator atlasGenerator = new TextureAtlasGenerator();
            Scene scene = SceneManager.GetActiveScene();
            string scenePath = scene.path;
            string folder = PathUtils.cleanScenePath(scenePath);

            string[] files = Directory.GetFiles(
                folder + "/Aditional Lighmaps",
                "*.exr",
                SearchOption.TopDirectoryOnly
            );
            OriginalLightMaps.Clear();
            foreach (var file in files)
            {
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(file);
                OriginalLightMaps.Add(tex);
            }
            DisableAllLightSets();
            AssetDatabase.Refresh();
            atlasGenerator.GeneratePreviewTextureAtlas(OriginalLightMaps, index);
            LightmapAtlas.Clear();
            AutoloadLightmapAtlas();
            RemoveOriginalLightMaps();
            PathUtils.RemoveTemporalDataFolder(folder);
            ToggleLightSet(index);
            AssetDatabase.Refresh();
            ToggleViewerLighting(true);
        }

        public void BakeLightmaps()
        {
            ToggleViewerLighting(false);
            LightmapAtlas.Clear();
            Debug.unityLogger.logHandler = new LogHandler();
            Scene scene = SceneManager.GetActiveScene();
            string scenePath = scene.path;
            if (!PathUtils.CreateTemporalDataFolder(scenePath, scene.name))
            {
                return;
            }
            TextureAtlasGenerator atlasGenerator = new TextureAtlasGenerator();
            if (!PathUtils.CreateTemporalDataFolder(scenePath, scene.name))
            {
                return;
            }
            if (!checkLightSets())
                return;
            RemoveAllLightmaps();
            progress = 0;
            abortBaking = false;
            if (useAsyncBake)
            {
                StartCoroutine(BakeLightmapsAsync());
            }
            else
            {
                BakeLightmapsSync();
            }
        }

        public void EnableSceneFog()
        {
            RenderSettings.fog = sceneFog;
        }

        void BakeLightmapsSync()
        {
            int count = toolMode == ToolMode.Two ? 2 : 4;
            for (int i = 0; i < count; i++)
            {
                ToggleLightSet(i);
                Lightmapping.Bake();
                DuplicateLightmap(i);
            }
            FinishBakeProcess();
        }

        public void ClearLog()
        {
            var assembly = System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.Editor));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method.Invoke(null, null);
        }

        bool abortBaking = false;
        IEnumerator BakeLightmapsAsync()
        {
            int count = toolMode == ToolMode.Two ? 2 : 4;

            for (int i = 0; i < count; i++)
            {
                if (abortBaking)
                {

                    abortBaking = false;
                    progress = 0;
                    yield break;
                }

                ToggleLightSet(i);
                Lightmapping.BakeAsync();
                while (Lightmapping.isRunning)
                {
                    progress = (i * 25) + (Lightmapping.buildProgress * 100) * 0.25f;
                    yield return null;
                    ClearLog();
                }

                DuplicateLightmap(i);
            }
            progress = 100;
            FinishBakeProcess();
            progress = 0;

        }

        public void BakePreviewLightmap(int index)
        {
            ToggleViewerLighting(false);
            LightmapAtlas.Clear();
            Debug.unityLogger.logHandler = new LogHandler();
            Scene scene = SceneManager.GetActiveScene();
            string scenePath = scene.path;
            if (!PathUtils.CreateTemporalDataFolder(scenePath, scene.name))
            {
                return;
            }
            TextureAtlasGenerator atlasGenerator = new TextureAtlasGenerator();
            if (!PathUtils.CreateTemporalDataFolder(scenePath, scene.name))
            {
                return;
            }
            if (!checkLightSets())
                return;
            RemoveAllLightmaps();
            StartCoroutine(BakePreviewLightmapAsync(index));
            progress = 0;
            abortBaking = false;
        }

        IEnumerator BakePreviewLightmapAsync(int index)
        {

            ToggleLightSet(index);
            Lightmapping.BakeAsync();
            while (Lightmapping.isRunning)
            {
                progress = Lightmapping.buildProgress * 100 * 0.25f;
                yield return null;
                ClearLog();
            }
            DuplicateLightmap(index);
            progress = 100;
            FinishBakePreviewProcess(index);
            progress = 0;

        }


        public void CancelLightmapBaking()
        {
            abortBaking = true;
            Lightmapping.Cancel();
            Lightmapping.Clear();
            StopCoroutine(BakeLightmapsAsync());
            RemoveOriginalLightMaps();
            RemoveAditionalLightmap();
            DisableAllLightSets();
            ToggleViewerLighting(false);
            return;
            /*
            string folder = PathUtils.cleanScenePath(SceneManager.GetActiveScene().path);
            PathUtils.RemoveTemporalDataFolder(folder);
            ToggleLightSet(0);
            AssetDatabase.Refresh();
            */
        }

        static void ToggleViewerLighting(bool value)
    {
        // Obtiene la vista de escena actual
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null)
        {
            sceneView.sceneLighting = value;
            sceneView.Repaint();
        }
        
    }



#endif
    }
}
