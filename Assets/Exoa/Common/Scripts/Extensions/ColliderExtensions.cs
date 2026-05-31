
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides extension methods for Collider objects.
/// This class allows for operations that can be performed on 
/// Collider instances to determine their screen space representation.
/// </summary>
public static class ColliderExtensions
{
    /// <summary>
    /// Updates the minimum and maximum values based on the provided point.
    /// This method evaluates a point and adjusts the min and max values
    /// accordingly to maintain the bounding rectangle.
    /// </summary>
    /// <param name="point">The point to evaluate.</param>
    /// <param name="min">The current minimum value, updated by reference.</param>
    /// <param name="max">The current maximum value, updated by reference.</param>
    static void get_minMax(Vector2 point, ref Vector2 min, ref Vector2 max)
    {
        min = new Vector2(min.x >= point.x ? point.x : min.x, min.y >= point.y ? point.y : min.y);
        max = new Vector2(max.x <= point.x ? point.x : max.x, max.y <= point.y ? point.y : max.y);
    }

    /// <summary>
    /// Calculates the screen rectangle that encompasses the bounds of the specified collider.
    /// This method transforms the collider's 3D bounds into 2D screen space,
    /// returning a rectangle that can be used for GUI layout or other purposes.
    /// </summary>
    /// <param name="collider">The collider whose bounds will be transformed into screen space.</param>
    /// <param name="cam">The optional camera to use for the transformation. If null, the main camera is used.</param>
    /// <returns>A Rect representing the screen space rectangle that encompasses the collider's bounds.</returns>
    public static Rect GetScreenRect(this Collider collider, Camera cam = null)
    {
        Vector3 cen = collider.bounds.center;
        Vector3 ext = collider.bounds.extents;
        cam = cam == null ? Camera.main : cam;
        float screenheight = Screen.height;

        Vector2 min = cam.WorldToScreenPoint(new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z - ext.z));
        Vector2 max = min;

        // Evaluating all corners of the collider's bounds in screen space
        // 0
        Vector2 point = min;
        get_minMax(point, ref min, ref max);

        // 1
        point = cam.WorldToScreenPoint(new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z - ext.z));
        get_minMax(point, ref min, ref max);

        // 2
        point = cam.WorldToScreenPoint(new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z + ext.z));
        get_minMax(point, ref min, ref max);

        // 3
        point = cam.WorldToScreenPoint(new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z + ext.z));
        get_minMax(point, ref min, ref max);

        // 4
        point = cam.WorldToScreenPoint(new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z - ext.z));
        get_minMax(point, ref min, ref max);

        // 5
        point = cam.WorldToScreenPoint(new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z - ext.z));
        get_minMax(point, ref min, ref max);

        // 6
        point = cam.WorldToScreenPoint(new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z + ext.z));
        get_minMax(point, ref min, ref max);

        // 7
        point = cam.WorldToScreenPoint(new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z + ext.z));
        get_minMax(point, ref min, ref max);

        Vector3 centerScreenPos2 = cam.WorldToScreenPoint(collider.bounds.center);

        return new Rect(centerScreenPos2.x, centerScreenPos2.y, max.x - min.x, max.y - min.y);
    }
}
