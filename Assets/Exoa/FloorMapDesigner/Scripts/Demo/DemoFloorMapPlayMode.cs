
using Exoa.Designer;
using Exoa.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class manages the demo floor map loading in play mode. 
/// It handles the selection of floor map files from a dropdown 
/// and instantiates the selected floor map prefab in the 
/// Unity environment upon a button click.
/// </summary>
public class DemoFloorMapPlayMode : MonoBehaviour
{
    private TMP_Dropdown dp; // Dropdown to select floor maps
    private Button loadBtn; // Button to load the selected floor map
    public GameObject prefab; // Prefab to instantiate the floor map

    /// <summary>
    /// Initializes the dropdown and button on start. 
    /// It also loads the list of floor maps previously created 
    /// with the editor scene and populates the dropdown.
    /// </summary>
    void Start()
    {
        dp = GetComponentInChildren<TMP_Dropdown>();
        loadBtn = GetComponentInChildren<Button>();
        loadBtn?.onClick.AddListener(OnClickLoad);

        // Loading the list of floor maps previously created with the editor scene
        SaveSystem.Create(SaveSystem.Mode.FILE_SYSTEM).ListFileItems(HDSettings.EXT_FLOORMAP_FOLDER, OnFloorMapsFound);
    }

    /// <summary>
    /// Callback method that is called when the list of floor maps 
    /// is found. It populates the dropdown with the floor maps available.
    /// </summary>
    /// <param name="fileList">The list of files found in the floor map folder.</param>
    private void OnFloorMapsFound(SaveSystem.FileList fileList)
    {
        for (int i = 0; i < fileList.list.Count; i++)
        {
            dp.options.Add(new TMP_Dropdown.OptionData(fileList.list[i].Replace(".json", "")));
        }
    }

    /// <summary>
    /// Method called when the load button is clicked. It checks if 
    /// a floor map is selected from the dropdown, then instantiates 
    /// the prefab and loads the corresponding floor map data from a JSON file.
    /// The instantiated object is then placed randomly in the scene, 
    /// and the camera focuses on it.
    /// </summary>
    private void OnClickLoad()
    {
        if (dp.value == 0)
            return;

        string floormapName = dp.FindDropdownValue();

        // This is just the code you're looking for:
        // Creating the prefab instance and loading the json file
        GameObject inst = Instantiate(prefab);

        // Load the floor map json file
        FloorMapLoadByName f = inst.GetComponent<FloorMapLoadByName>();
        f.LoadFile(floormapName);

        // Place the floor map in a random position and rotation
        inst.transform.position = new Vector3(UnityEngine.Random.Range(-20, 20), 0, UnityEngine.Random.Range(-20, 20));
        inst.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);

        // Focus the camera on that new object
        CameraEvents.OnRequestObjectFocus?.Invoke(inst, true);
    }
}
