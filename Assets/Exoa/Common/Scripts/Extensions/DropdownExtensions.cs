
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Provides extension methods for the TMP_Dropdown class.
/// These methods simplify adding, modifying, and filling dropdown options.
/// </summary>
public static class DropdownExtensions
{
    /// <summary>
    /// Sets the value of the dropdown to the specified value 
    /// or creates a new option if it doesn't exist.
    /// </summary>
    /// <param name="dropdown">The TMP_Dropdown to modify.</param>
    /// <param name="value">The string value to set or create.</param>
    public static void SetValueOrCreate(this TMP_Dropdown dropdown, string value)
    {
        int foundValue = -1;
        for (int i = 0; i < dropdown.options.Count; i++)
        {
            TMP_Dropdown.OptionData od = dropdown.options[i];
            if (od.text == value)
            {
                foundValue = i;
                dropdown.value = i;
            }
        }
        if (foundValue == -1)
        {
            dropdown.AddOptions(new List<string>() { value });
            dropdown.SetValueOrCreate(value);
        }
    }

    /// <summary>
    /// Finds and returns the current value of the dropdown.
    /// If the dropdown has no options, returns null.
    /// </summary>
    /// <param name="dropdown">The TMP_Dropdown to query.</param>
    /// <returns>The currently selected string value or null if no options exist.</returns>
    public static string FindDropdownValue(this TMP_Dropdown dropdown)
    {
        if (dropdown?.options.Count == 0) return null;
        string val = dropdown?.options[Mathf.Clamp(dropdown.value, 0, dropdown.options.Count)].text;
        return val;
    }

    /// <summary>
    /// Fills the dropdown with options based on the provided GameObject array.
    /// Optionally clears existing options before filling.
    /// </summary>
    /// <param name="dropdown">The TMP_Dropdown to modify.</param>
    /// <param name="list">Array of GameObjects whose names are used as options.</param>
    /// <param name="clearBefore">Whether to clear existing options before filling.</param>
    public static void FillDropdown(this TMP_Dropdown dropdown, GameObject[] list, bool clearBefore)
    {
        if (clearBefore)
            dropdown.ClearOptions();
        foreach (GameObject cg in list)
            if (cg != null)
                dropdown.AddOptions(new List<string>() { (cg.name) });
    }

    /// <summary>
    /// Fills the dropdown with options based on the provided string array.
    /// Optionally clears existing options before filling.
    /// </summary>
    /// <param name="dropdown">The TMP_Dropdown to modify.</param>
    /// <param name="list">Array of strings to be added as options.</param>
    /// <param name="clearBefore">Whether to clear existing options before filling.</param>
    public static void FillDropdown(this TMP_Dropdown dropdown, string[] list, bool clearBefore)
    {
        if (clearBefore)
            dropdown.ClearOptions();
        dropdown.AddOptions(new List<string>(list));
    }

    /// <summary>
    /// Fills the dropdown with options based on the provided GameObject array 
    /// without adding duplicates. Optionally clears existing options before filling.
    /// </summary>
    /// <param name="dropdown">The TMP_Dropdown to modify.</param>
    /// <param name="list">Array of GameObjects whose names are used as options.</param>
    /// <param name="clearBefore">Whether to clear existing options before filling.</param>
    public static void FillDropdownNoDuplicates(this TMP_Dropdown dropdown, GameObject[] list, bool clearBefore)
    {
        List<string> ls = new List<string>();
        foreach (GameObject cg in list)
            if (cg != null) ls.Add(cg.name);
        dropdown.FillDropdownNoDuplicates(ls.ToArray(), clearBefore);
    }

    /// <summary>
    /// Fills the dropdown with options based on the provided string array 
    /// without adding duplicates. Optionally clears existing options before filling.
    /// </summary>
    /// <param name="dropdown">The TMP_Dropdown to modify.</param>
    /// <param name="list">Array of strings to be added as options.</param>
    /// <param name="clearBefore">Whether to clear existing options before filling.</param>
    public static void FillDropdownNoDuplicates(this TMP_Dropdown dropdown, string[] list, bool clearBefore)
    {
        if (clearBefore)
            dropdown.ClearOptions();
        foreach (string v in list)
        {
            bool found = false;
            for (int i = 0; i < dropdown.options.Count; i++)
            {
                TMP_Dropdown.OptionData od = dropdown.options[i];
                if (od.text == v)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
                dropdown.AddOptions(new List<string>() { v });
        }
    }

    /// <summary>
    /// Fills the dropdown with options based on the provided TextAsset array.
    /// Clears existing options before filling.
    /// </summary>
    /// <param name="dropdown">The TMP_Dropdown to modify.</param>
    /// <param name="list">Array of TextAssets whose names are used as options.</param>
    public static void FillDropdown(this TMP_Dropdown dropdown, TextAsset[] list)
    {
        dropdown.ClearOptions();
        foreach (TextAsset cg in list)
            dropdown.AddOptions(new List<string>() { (cg.name) });
    }
}
