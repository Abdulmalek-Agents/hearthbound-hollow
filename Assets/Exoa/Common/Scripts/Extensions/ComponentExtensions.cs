
using UnityEngine;
using System.Reflection;
using System;
using System.Collections.Generic;

/// <summary>
/// Static class that provides extension methods for GameObject and Component to facilitate 
/// retrieving components in children and other utility methods.
/// </summary>
public static class ComponentExtensions
{
    /// <summary>
    /// Retrieves all components of the specified type T in the children of the specified GameObject
    /// and adds them to the provided list. This method can also include components from the GameObject itself 
    /// and inactive objects if specified.
    /// </summary>
    /// <typeparam name="T">The type of component to retrieve.</typeparam>
    /// <param name="gameObject">The GameObject to search within.</param>
    /// <param name="components">The list to which found components will be added.</param>
    /// <param name="includeThis">Whether to include the components from the GameObject itself.</param>
    /// <param name="includeInactive">Whether to include inactive components.</param>
    public static void GetComponentsInChildren<T>(this GameObject gameObject, List<T> components, bool includeThis = false, bool includeInactive = false)
    {
        if (components == null)
        {
            return; // Return if the list to add components is null
        }
        if (includeThis)
        {
            List<T> list = new List<T>();
            gameObject.GetComponents<T>(list); // Retrieve components from the GameObject itself

            foreach (T current in list)
            {
                if (!components.Contains(current))
                {
                    components.Add(current); // Add the component if it is not already in the list
                }
            }
        }
        Transform[] componentsInChildren = gameObject.GetComponentsInChildren<Transform>(includeInactive);
        if (componentsInChildren != null && componentsInChildren.Length != 0)
        {
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                Transform transform = componentsInChildren[i];
                if (!(transform == gameObject.transform))
                {
                    transform.GetComponentsInChildren(components, true, false); // Recursively get components in children
                }
            }
        }
    }

    /// <summary>
    /// Retrieves all components of the specified type T in the children of the specified Component's 
    /// GameObject and adds them to the provided list. This method can also include components from 
    /// the Component itself and inactive objects if specified.
    /// </summary>
    /// <typeparam name="T">The type of component to retrieve.</typeparam>
    /// <param name="component">The Component to search within.</param>
    /// <param name="components">The list to which found components will be added.</param>
    /// <param name="includeThis">Whether to include the components from the Component itself.</param>
    /// <param name="includeInactive">Whether to include inactive components.</param>
    public static void GetComponentsInChildren<T>(this Component component, List<T> components, bool includeThis = false, bool includeInactive = false)
    {
        if (components == null)
        {
            return; // Return if the list to add components is null
        }
        if (includeThis)
        {
            List<T> list = new List<T>();
            component.GetComponents<T>(list); // Retrieve components from the Component itself
            
            foreach (T current in list)
            {
                if (!components.Contains(current))
                {
                    components.Add(current); // Add the component if it is not already in the list
                }
            }
        }
        Transform[] componentsInChildren = component.GetComponentsInChildren<Transform>(includeInactive);
        if (componentsInChildren != null && componentsInChildren.Length != 0)
        {
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                Transform transform = componentsInChildren[i];
                if (!(transform == component.transform))
                {
                    transform.GetComponentsInChildren(components, true, false); // Recursively get components in children
                }
            }
        }
    }

    /// <summary>
    /// Destroys the specified Component. If in the Unity Editor and the game is playing, 
    /// the object will be destroyed using Destroy, otherwise DestroyImmediate is used.
    /// </summary>
    /// <param name="current">The Component to be destroyed.</param>
    public static void DestroyUniversal(this Component current)
    {
#if UNITY_EDITOR
        if (UnityEditor.EditorApplication.isPlaying)
            GameObject.Destroy(current); // Use Destroy while playing
        else GameObject.DestroyImmediate(current); // Use DestroyImmediate if not playing
#else
        GameObject.Destroy(current); // Use Destroy in build
#endif
    }

    /// <summary>
    /// Creates a copy of the specified component by copying all its properties 
    /// and fields from another component of the same type to the current component.
    /// </summary>
    /// <typeparam name="T">The type of component to copy.</typeparam>
    /// <param name="comp">The current component to copy values to.</param>
    /// <param name="other">The component from which to copy values.</param>
    /// <returns>The current component with copied values, or null if types do not match.</returns>
    public static T GetCopyOf<T>(this Component comp, T other) where T : Component
    {
        Type type = comp.GetType();
        if (type != other.GetType())
            return null; // Type mismatch, return null

        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
        PropertyInfo[] pinfos = type.GetProperties(flags);
        foreach (var pinfo in pinfos)
        {
            if (pinfo.CanWrite)
            {
                try
                {
                    pinfo.SetValue(comp, pinfo.GetValue(other, null), null); // Copy property value
                }
                catch
                {
                    // Handle potential NotImplementedException
                }
            }
        }
        FieldInfo[] finfos = type.GetFields(flags);
        foreach (var finfo in finfos)
        {
            finfo.SetValue(comp, finfo.GetValue(other)); // Copy field value
        }
        return comp as T; // Return the updated component
    }
}
