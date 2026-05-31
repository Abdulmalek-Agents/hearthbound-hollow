using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Exoa.Designer
{
    [InitializeOnLoad]
    public static class OnlineModuleSetup
    {
        // static constructor is called as soon as class is initialized
        static OnlineModuleSetup()
        {

            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            var define = defines.Split(';').ToList();

            if (!define.Contains("ONLINE_MODULE"))
            {
                defines += ";ONLINE_MODULE";
                AddSceneToBuildSettings("Assets/Exoa/OnlineModule/Scenes/Login.unity");
                AddSceneToBuildSettings("Assets/Exoa/OnlineModule/Scenes/ApiTester.unity");

            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defines);

        }



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
