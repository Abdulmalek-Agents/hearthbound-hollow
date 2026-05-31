
using Exoa.Designer;
using Exoa.Events;
using UnityEngine;

/// <summary>
/// This class is responsible for loading a specified home configuration
/// from a file when the game starts or when requested. It interacts with
/// the <see cref="HomeReader"/> component to read and apply the configurations.
/// </summary>
public class HomeLoadByName : MonoBehaviour
{
    /// <summary>
    /// The name of the file to load the home configuration from.
    /// </summary>
    public string fileName;

    /// <summary>
    /// Indicates whether the home configuration should be loaded at start.
    /// </summary>
    public bool buildAtStart;

    /// <summary>
    /// Initializes the component and loads the home file if required.
    /// This method is called on the frame when the script is enabled.
    /// </summary>
    void Start()
    {
        // Check if we need to build the home at start and if the fileName is not empty or null
        if (buildAtStart && !string.IsNullOrEmpty(fileName))
        {
            LoadFile(fileName);
        }
    }

    /// <summary>
    /// Loads a home configuration file using its name.
    /// This method initializes the AppController if it has not been set yet,
    /// and then it uses the HomeReader to replace and load the configuration.
    /// </summary>
    /// <param name="fileName">The name of the file to load the home configuration from.</param>
    public void LoadFile(string fileName)
    {
        // Initialize AppController if it has not been instantiated
        if (AppController.Instance == null)
        {
            AppController.Instance = GetComponent<AppController>();
        }

        // Proceed to load the file if the fileName is not empty or null
        if (!string.IsNullOrEmpty(fileName))
        {
            HomeReader dataReader = GetComponent<HomeReader>();
            dataReader.ReplaceAndLoad(fileName, true);
        }
    }

    /// <summary>
    /// Clears the current home configuration by invoking the clear method
    /// on the HomeReader component.
    /// </summary>
    public void Clear()
    {
        HomeReader dataReader = GetComponent<HomeReader>();
        dataReader.Clear();
    }
}
