
using System.Linq;
using UnityEditor;

namespace Exoa.Designer
{
    /// <summary>
    /// This static class is responsible for setting up the interior module
    /// within the Unity Editor upon loading. It checks for specific scripting
    /// define symbols and ensures that the necessary scenes are added to the build settings.
    /// </summary>
    [InitializeOnLoad]
    public static class InteriorModuleSetup
    {
        /// <summary>
        /// Static constructor that is called on load. It checks if the "INTERIOR_MODULE"
        /// define symbol is present and adds it if necessary. It also adds a specific scene 
        /// to the build settings if it does not already exist.
        /// </summary>
        static InteriorModuleSetup()
        {
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            var define = defines.Split(';').ToList();

            if (!define.Contains("INTERIOR_MODULE"))
            {
                defines += ";INTERIOR_MODULE";
                AddSceneToBuildSettings("Assets/Exoa/HomeDesigner/Scenes/HomeDesigner.unity");
            }
            //if (!define.Contains("FBXSDK_RUNTIME"))
            //{
            //defines += ";FBXSDK_RUNTIME";
            //}

            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defines);
        }

        /// <summary>
        /// Adds a specified scene to the Unity build settings if it does not already exist.
        /// If the scene already exists, it updates the existing entry.
        /// </summary>
        /// <param name="pathOfSceneToAdd">The file path of the scene to add to the build settings.</param>
        private static void AddSceneToBuildSettings(string pathOfSceneToAdd)
        {
            // Loop through and see if the scene already exists in the build settings
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

                // Seems inefficient to add scene to build settings after creating each scene
                // rather than doing it all at once after they are all created, however, it's
                // necessary to avoid memory issues.
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
                    // Skip over the scene that is a duplicate
                    // this will effectively delete it from the build settings
                    if (i != indexOfSceneIfExist)
                        newScenes[j++] = EditorBuildSettings.scenes[i];
                }
                newScenes[j] = new EditorBuildSettingsScene(pathOfSceneToAdd, true);
            }

            EditorBuildSettings.scenes = newScenes;
        }
    }
}
