// Distant Lands | 2025

using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;

namespace DistantLands.Zephyr
{
    [InitializeOnLoad]
    public static class ZephyrDefineSetup
    {
        const string Define = "ZEPHYR";
        const string Keyword = "ZEPHYR";

        static ZephyrDefineSetup()
        {
            AddDefine();
            EnableGlobalShaderKeyword();
        }

        static void AddDefine()
        {
            NamedBuildTarget nbt = NamedBuildTarget.FromBuildTargetGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup);

            string defines = PlayerSettings.GetScriptingDefineSymbols(nbt);

            if (!HasDefine(defines, Define))
            {
                string newDefines = defines;
                if (!string.IsNullOrEmpty(newDefines))
                    newDefines += ";";
                newDefines += Define;
                PlayerSettings.SetScriptingDefineSymbols(nbt, newDefines);
            }
        }

        static bool HasDefine(string definesString, string defineToCheck)
        {
            if (string.IsNullOrEmpty(definesString))
                return false;

            string[] parts = definesString.Split(';');
            foreach (var d in parts)
                if (d.Trim() == defineToCheck)
                    return true;
            return false;
        }

        [MenuItem("Tools/Zephyr/Enable Global Keyword")]
        static void EnableGlobalShaderKeyword()
        {
            GlobalKeyword gk = GlobalKeyword.Create(Keyword);
            Shader.SetKeyword(gk, true);
        }
        [MenuItem("Tools/Zephyr/Disable Global Keyword")]
        static void DisableGlobalShaderKeyword()
        {
            GlobalKeyword gk = GlobalKeyword.Create(Keyword);
            Shader.SetKeyword(gk, false);
        }
    }
}