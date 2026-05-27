
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Exoa.Designer
{
    /// <summary>
    /// A static class responsible for initializing the building module setup. 
    /// This class checks and updates the scripting define symbols for the current build target group.
    /// </summary>
    [InitializeOnLoad]
    public static class BuildingModuleSetup
    {
        // static constructor is called as soon as class is initialized
        /// <summary>
        /// Static constructor that is called when the class is initialized. 
        /// It ensures that the BUILDING_MODULE define is included in the scripting symbols.
        /// </summary>
        static BuildingModuleSetup()
        {
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            var define = defines.Split(';').ToList();

            if (!define.Contains("BUILDING_MODULE"))
            {
                defines += ";BUILDING_MODULE";
                //AddSceneToBuildSettings("Assets/Exoa/BuildingDesigner/Scenes/BuildingEditor.unity");

            }
            //if (!define.Contains("FBXSDK_RUNTIME"))
            //{
            //defines += ";FBXSDK_RUNTIME";
            //}

            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defines);
        }

        /// <summary>
        /// Adds a scene to the build settings if it does not already exist.
        /// If the scene already exists, it replaces it to avoid duplicates.
        /// </summary>
        /// <param name="pathOfSceneToAdd">The file path of the scene to add to the build settings.</param>
        private static void AddSceneToBuildSettings(string pathOfSceneToAdd)
        {
            //Loop through and see if the scene already exist in the build settings
            int indexOfSceneIfExist = -1;

            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                if (EditorBuildSettings.scenes[i].path == pathOfSceneToAdd)
                {
                    indexOfSceneIfExist = i;
                    break;
                }
            }

            EditorBuildSettingsScene[] newScenes;

            if (indexOfSceneIfExist == -1)
            {
                newScenes = new EditorBuildSettingsScene[EditorBuildSettings.scenes.Length + 1];

                //Seems inefficent to add scene to build settings after creating each scene (rather than doing it all at once
                //after they are all created, however, it's necessary to avoid memory issues.
                int i = 0;
                for (; i < EditorBuildSettings.scenes.Length; i++)
                    newScenes[i] = EditorBuildSettings.scenes[i];

                newScenes[i] = new EditorBuildSettingsScene(pathOfSceneToAdd, true);
            }
            else
            {
                newScenes = new EditorBuildSettingsScene[EditorBuildSettings.scenes.Length];

                int i = 0, j = 0;
                for (; i < EditorBuildSettings.scenes.Length; i++)
                {
                    //skip over the scene that is a duplicate
                    //this will effectively delete it from the build settings
                    if (i != indexOfSceneIfExist)
                        newScenes[j++] = EditorBuildSettings.scenes[i];
                }
                newScenes[j] = new EditorBuildSettingsScene(pathOfSceneToAdd, true);
            }

            EditorBuildSettings.scenes = newScenes;
        }
    }
}
