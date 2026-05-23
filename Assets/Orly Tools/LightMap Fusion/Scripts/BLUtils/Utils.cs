#if UNITY_EDITOR


using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;


namespace LightMapFusion
{
    public static class PathUtils
    {

        public static string GetPackagePath(string packageName, string relativePath)
        {
            string basePath = Application.dataPath.Replace("Assets", "") + "Packages/" + packageName;
            return Path.Combine(basePath, relativePath);
        }
        public static string cleanScenePath(string path)
        {
            string newPath = path.Substring(0, path.Length - 6);
            return newPath;
        }

        public static string GetSceneFolder(string path)
        {
            string[] newPath = path.Split("/");
            string result = newPath[0];
            for (int i = 1; i < newPath.Length - 1; i++)
            {
                result += "/";
                result += newPath[i];
            }

            return result;
        }

        public static string[] GetSubFolders(string path)
        {
            string[] folders = AssetDatabase.GetSubFolders(path);
            string[] names = new string[folders.Length];
            for (int i = 0; i < folders.Length; i++)
            {
                string[] name = folders[i].Split("/");
                names[i] = name[name.Length - 1];
            }
            return names;
        }

        public static bool checkFolder(string path, string folderName)
        {
            string[] names = GetSubFolders(path);
            for (int i = 0; i < names.Length; i++)
            {
                if (names[i] == folderName)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool CreateTemporalDataFolder(string scenePath, string sceneName)
        {
            if (scenePath == "")
            {
                EditorUtility.DisplayDialog("Error", "First you must save the scene", "OK");
                return false;
            }
            else
            {
                string folder = PathUtils.cleanScenePath(scenePath);
                string mainPath = PathUtils.GetSceneFolder(scenePath);
                if (!PathUtils.checkFolder(mainPath, sceneName))
                {
                    AssetDatabase.CreateFolder(mainPath, sceneName);
                }
                if (!PathUtils.checkFolder(folder, "Aditional Lighmaps"))
                {
                    AssetDatabase.CreateFolder(folder, "Aditional Lighmaps");
                }
                AssetDatabase.Refresh();
                return true;
            }
        }

        public static void RemoveTemporalDataFolder(string scenePath)
        {
            //FileUtil.DeleteFileOrDirectory(scenePath + "/Aditional Lighmaps");
            AssetDatabase.DeleteAsset(scenePath + "/Aditional Lighmaps");
            AssetDatabase.Refresh();
        }

        public static List<Shader> AutoloadToolShaders()
        {
            List<Shader> toolShaders = new List<Shader>();
            string scenePath = "Assets/Orly Tools/LightMap Fusion/Shader/Surface Shaders";
            string[] files = Directory.GetFiles(scenePath, "*.shader", SearchOption.AllDirectories);
            toolShaders.Clear();
            foreach (var file in files)
            {
                var _shader = AssetDatabase.LoadAssetAtPath<Shader>(file);
                if (!Check_URP_RenderPipeline() && _shader.name.Contains("Built-in"))
                {

                    toolShaders.Add(_shader);
                }
                else
                {
                    if (Check_URP_RenderPipeline() && _shader.name.Contains("URP"))
                    {
                        toolShaders.Add(_shader);
                    }
                }
            }
            return toolShaders;
        }




        public static List<Shader> AutoloadToolDissolveShaders()
        {
            List<Shader> toolShaders = new List<Shader>();
            string scenePath = "Assets/Orly Tools/LightMap Fusion/Shader/Surface Shaders";
            string[] files = Directory.GetFiles(scenePath, "*.shader", SearchOption.AllDirectories);
            toolShaders.Clear();
            foreach (var file in files)
            {
                var _shader = AssetDatabase.LoadAssetAtPath<Shader>(file);
                if (_shader.name.Contains("Dissolve"))
                {
                    if (!Check_URP_RenderPipeline() && _shader.name.Contains("Built-in"))
                    {

                        toolShaders.Add(_shader);
                    }
                    else
                    {
                        if (Check_URP_RenderPipeline() && _shader.name.Contains("URP"))
                        {
                            toolShaders.Add(_shader);
                        }
                    }
                }
            }
            return toolShaders;
        }

        public static bool Check_URP_RenderPipeline()
        {
            if (GraphicsSettings.currentRenderPipeline == null)
            {
                return false;
            }
            else
            {
                var pipelineAsset = GraphicsSettings.currentRenderPipeline;
                if (pipelineAsset.GetType().ToString().Contains("UniversalRenderPipeline"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }
    }
}






#endif
