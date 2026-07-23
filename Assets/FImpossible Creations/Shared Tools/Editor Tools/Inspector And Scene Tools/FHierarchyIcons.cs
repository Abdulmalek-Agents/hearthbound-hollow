#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

/* Code made by LaneFox from Unity Community Forum */

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class FHierarchyIcons
{
    static FHierarchyIcons()
    {
#if UNITY_EDITOR
#if UNITY_6000_5_OR_NEWER
        EditorApplication.hierarchyWindowItemByEntityIdOnGUI += (entityId, rect) => EvaluateIcons(entityId.GetHashCode(), rect);
#else
        EditorApplication.hierarchyWindowItemOnGUI += EvaluateIcons;
#endif
#endif
    }

    private static void EvaluateIcons(int instanceId, Rect selectionRect)
    {
#if UNITY_EDITOR
#if UNITY_6000_3_OR_NEWER
        GameObject go = EditorUtility.EntityIdToObject(instanceId) as GameObject;
#else
        GameObject go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
#endif
        if (go == null) return;

        IFHierarchyIcon slotCon = go.GetComponent<IFHierarchyIcon>();
        if (slotCon != null) DrawIcon(slotCon.EditorIconPath, selectionRect);
#endif
    }

    private static void DrawIcon(string texName, Rect rect)
    {
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(texName)) return;
        Rect r = new Rect(rect.x + rect.width - 16f, rect.y, 16f, 16f);
        GUI.DrawTexture(r, GetTex(texName));
#endif
    }

    private static Texture2D GetTex(string name)
    {
#if UNITY_EDITOR
        return (Texture2D)Resources.Load(name);
#else
        return null;
#endif
    }
}

public interface IFHierarchyIcon
{
    string EditorIconPath { get; }
}

/*

{...}
    public class ItemUiSlot : MonoBehaviour, IDropHandler, FIHierarchyIcon
    {
        public string EditorIconPath { get { return "LogoGrey";  } }
{...}

*/
