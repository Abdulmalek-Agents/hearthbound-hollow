
using UnityEngine;

namespace Exoa.Designer
{
    /// <summary>
    /// This class is used to store and manage settings for an online module in a game.
    /// It is a ScriptableObject, meaning it can be saved as an asset in the project.
    /// </summary>
    [CreateAssetMenu(fileName = "OnlineModuleSettings", menuName = "Exoa/New OnlineModuleSettings")]
    public class OnlineModuleSettings : ScriptableObject
    {
        // A bool indicating whether the online mode should be used or not.
        public bool useOnlineMode;
        // A string representing the URL of the online script to be used.
        public string scriptUrl;

        // A private static instance of the OnlineModuleSettings class.
        private static OnlineModuleSettings instance;

        /// <summary>
        /// A method that returns the instance of the OnlineModuleSettings class.
        /// If the instance does not exist, it attempts to load it from the Resources folder.
        /// Logs an error if it cannot find the instance.
        /// </summary>
        /// <returns>The instance of OnlineModuleSettings.</returns>
        public static OnlineModuleSettings GetSettings()
        {
            // If the instance is null, load the OnlineModuleSettings asset from the Resources folder.
            if (instance == null)
                instance = Resources.Load<OnlineModuleSettings>("OnlineModuleSettings");

            // If the instance is still null, log an error message.
            if (instance == null)
                Debug.LogError("Could not find any OnlineModuleSettings instance in Resouces/");
            // Return the instance of the OnlineModuleSettings class.
            return instance;
        }
    }
}
