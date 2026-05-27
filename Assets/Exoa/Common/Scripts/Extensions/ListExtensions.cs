
using System;
using System.Collections.Generic;
using UnityEngine;

public static class ListExtensions
{
    /// <summary>
    /// Returns a comma-separated string representation of the List of strings.
    /// </summary>
    /// <param name="s">The list of strings to be dumped.</param>
    /// <returns>A string that contains all items in the list, separated by commas.</returns>
    public static string Dump(this List<string> s)
    {
        string str = "";
        if (s == null || s.Count == 0) return str;
        for (int i = 0; i < s.Count; i++) str += s[i] + ", ";
        return str.Substring(0, str.Length - 2);
    }

    /// <summary>
    /// Returns a string representation of the List of Vector3, with each element indexed.
    /// </summary>
    /// <param name="s">The list of Vector3 objects to be dumped.</param>
    /// <returns>A string that contains the index and value of each Vector3 in the list.</returns>
    public static string Dump(this List<Vector3> s)
    {
        string str = "";
        if (s == null || s.Count == 0) return str;
        for (int i = 0; i < s.Count; i++) str += i + ":" + s[i].ToString() + " ";
        return str;
    }

    /// <summary>
    /// Returns a string representation of the List of Vector2, with each element indexed.
    /// </summary>
    /// <param name="s">The list of Vector2 objects to be dumped.</param>
    /// <returns>A string that contains the index and value of each Vector2 in the list.</returns>
    public static string Dump(this List<Vector2> s)
    {
        string str = "";
        if (s == null || s.Count == 0) return str;
        for (int i = 0; i < s.Count; i++) str += i + ":" + s[i].ToString() + " ";
        return str;
    }

    /// <summary>
    /// Returns a string representation of an array of Vector3, with each element indexed.
    /// </summary>
    /// <param name="s">The array of Vector3 objects to be dumped.</param>
    /// <returns>A string containing the index and value of each Vector3 in the array.</returns>
    public static string Dump(this Vector3[] s)
    {
        string str = "";
        if (s == null || s.Length == 0) return str;
        for (int i = 0; i < s.Length; i++) str += i + ":" + s[i].ToString() + " ";
        return str;
    }

    /// <summary>
    /// Returns a string representation of an array of Vector2, with each element indexed.
    /// </summary>
    /// <param name="s">The array of Vector2 objects to be dumped.</param>
    /// <returns>A string containing the index and value of each Vector2 in the array.</returns>
    public static string Dump(this Vector2[] s)
    {
        string str = "";
        if (s == null || s.Length == 0) return str;
        for (int i = 0; i < s.Length; i++) str += i + ":" + s[i].ToString() + " ";
        return str;
    }

    /// <summary>
    /// Replaces occurrences of a specified substring within all items of the list with a replacement string.
    /// </summary>
    /// <param name="Source">The list of strings where replacements should be made.</param>
    /// <param name="find">The substring to be replaced.</param>
    /// <param name="replace">The string to replace with.</param>
    /// <returns>The updated list of strings with replacements made.</returns>
    public static List<string> ReplaceText(this List<string> Source, string find, string replace)
    {
        for (int i = 0; i < Source.Count; i++)
            Source[i] = Source[i].Replace(find, replace);
        return Source;
    }

    /// <summary>
    /// Moves a specified item one position up in the list.
    /// </summary>
    /// <typeparam name="T">The type of item in the list.</typeparam>
    /// <param name="Source">The list of items.</param>
    /// <param name="item">The item to be moved up.</param>
    public static void MoveUp<T>(this List<T> Source, T item)
    {
        int ind = Source.IndexOf(item);
        if (ind != 0)
        {
            Source.RemoveAt(ind);
            Source.Insert(ind - 1, item);
        }
    }

    /// <summary>
    /// Moves a specified item one position down in the list.
    /// </summary>
    /// <typeparam name="T">The type of item in the list.</typeparam>
    /// <param name="Source">The list of items.</param>
    /// <param name="item">The item to be moved down.</param>
    public static void MoveDown<T>(this List<T> Source, T item)
    {
        int ind = Source.IndexOf(item);
        if (ind != Source.Count - 1)
        {
            Source.RemoveAt(ind);
            Source.Insert(ind + 1, item);
        }
    }

    /// <summary>
    /// Removes a specified range from the source list and returns it as a new list.
    /// </summary>
    /// <typeparam name="T">The type of items in the list.</typeparam>
    /// <param name="Source">The source list from which to splice items.</param>
    /// <param name="Start">The starting index of the range to remove.</param>
    /// <param name="Size">The number of items to remove from the list.</param>
    /// <returns>A new list containing the spliced items.</returns>
    public static List<T> Splice<T>(this List<T> Source, int Start, int Size)
    {
        List<T> retVal = Source.GetRange(Start, Size);
        Source.RemoveRange(Start, Size);
        return retVal;
    }

    /// <summary>
    /// Randomly shuffles the elements in the list.
    /// </summary>
    /// <typeparam name="T">The type of items in the list.</typeparam>
    /// <param name="list">The list of items to shuffle.</param>
    public static void Shuffle<T>(this IList<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
