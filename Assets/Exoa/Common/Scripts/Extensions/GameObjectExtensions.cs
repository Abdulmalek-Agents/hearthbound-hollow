
using System.Collections.Generic;
using UnityEngine;

namespace Exoa.Designer
{
    /// <summary>
    /// Provides extension methods for the GameObject class, enhancing functionality related to game object management and manipulation.
    /// </summary>
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Converts a world point to a position in a RectTransform context.
        /// </summary>
        /// <param name="rt">The RectTransform to convert the point into local space.</param>
        /// <param name="p">The world position to convert.</param>
        /// <param name="cam">The camera to use for the conversion. If null, the main camera will be used.</param>
        /// <returns>A Vector3 representing the position within the RectTransform.</returns>
        public static Vector3 WorldPointToRectTransformPosition(this RectTransform rt, Vector3 p, Camera cam = null)
        {
            Vector3 screenPoint = cam.WorldToScreenPoint(p);
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, new Vector2(screenPoint.x, screenPoint.y), null, out pos);

            return pos;
        }

        /// <summary>
        /// Destroys a GameObject, removing it from the scene and cleaning up references.
        /// </summary>
        /// <param name="current">The GameObject to be destroyed.</param>
        public static void DestroyUniversal(this GameObject current)
        {
            if (current == null) return;
            current.transform.SetParent(null);
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying)
                GameObject.Destroy(current);
            else GameObject.DestroyImmediate(current);
#else
        GameObject.Destroy(current);
#endif
        }

        /// <summary>
        /// Recursively applies a layer to a GameObject and all of its children.
        /// </summary>
        /// <param name="go">The GameObject to apply the layer to.</param>
        /// <param name="layer">The name of the layer to apply.</param>
        public static void ApplyLayerRecursively(this GameObject go, string layer)
        {
            Transform t = go.transform;
            go.layer = LayerMask.NameToLayer(layer);
            for (int i = 0; i < t.childCount; i++)
            {
                t.GetChild(i).gameObject.ApplyLayerRecursively(layer);
            }
        }

        /// <summary>
        /// Computes the bounding box that encompasses all renderers in a GameObject and its children.
        /// </summary>
        /// <param name="go">The GameObject to compute bounds for.</param>
        /// <returns>The computed Bounds object that encloses all child renderers.</returns>
        public static Bounds GetBoundsRecursive(this GameObject go)
        {
            Bounds b = new Bounds();

            if (go == null)
                return b;

            List<Renderer> rList = go.GetComponentsInChildrenRecursive<Renderer>();

            if (rList.Count == 0)
                b.center = go.transform.position;
            else 
                b.center = rList[0].bounds.center;

            foreach (Renderer r in rList)
            {
                b.Encapsulate(r.bounds);
            }
            return b;
        }

        /// <summary>
        /// Recursively retrieves all components of a specified type from the GameObject and its children.
        /// </summary>
        /// <typeparam name="T">The type of component to retrieve.</typeparam>
        /// <param name="topLevelGO">The GameObject to search within.</param>
        /// <returns>A list of components of the specified type found in this GameObject and its children.</returns>
        public static List<T> GetComponentsInChildrenRecursive<T>(this GameObject topLevelGO) where T : Component
        {
            List<T> components = new List<T>();
            SearchForComponent_Helper<T>(topLevelGO, components);
            return components;
        }

        /// <summary>
        /// Helper method for recursively searching for components of a specified type within a GameObject and its children.
        /// </summary>
        /// <typeparam name="T">The type of component to search for.</typeparam>
        /// <param name="_go">The GameObject to search within.</param>
        /// <param name="list">The list to which found components will be added.</param>
        private static void SearchForComponent_Helper<T>(GameObject _go, List<T> list) where T : Component
        {
            Transform t = _go.transform;
            int numChildren = t.childCount;

            if (numChildren > 0)
            {
                for (int i = 0; i < numChildren; i++)
                {
                    Transform child = t.GetChild(i);
                    SearchForComponent_Helper<T>(child.gameObject, list);
                }
            }

            T component = _go.GetComponent<T>(); 

            if (component != null)
            {
                list.Add(component);
            }
        }
    }
}
