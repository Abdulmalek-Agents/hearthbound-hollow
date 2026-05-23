using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SimpleTalentTreeUi
{
    /// <summary>
    /// Very simple save system example that serializes all ISaveable objects in the scene
    /// into a single JSON file using Unity's JsonUtility.
    ///
    /// This is intended as a reference implementation. For production games, you may want
    /// to integrate your own save system or extend this one with additional features
    /// (multiple save slots, encryption, cloud saves, etc.).
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("If true, Save() will be called automatically on application quit and Load() on start.")]
        [SerializeField] private bool canTestSaveAndLoad = true;

        [Tooltip("Subfolder inside Application.persistentDataPath used to store the save file.")]
        [SerializeField] private string folderName = "Save";

        [Tooltip("File name for the save data.")]
        [SerializeField] private string fileName = "save.json";

        /// <summary>
        /// True if automatic saving is enabled (based on canTestSaveAndLoad).
        /// Manual Save()/Load() calls are always allowed.
        /// </summary>
        public bool IsSaveEnabled { get; private set; }

        #region Nested data types

        [Serializable]
        private class SaveFileData
        {
            public List<ObjectSaveData> objects = new List<ObjectSaveData>();
        }

        [Serializable]
        private class ObjectSaveData
        {
            /// <summary>
            /// Key used to match a saved object with an ISaveable object in the scene.
            /// </summary>
            public string key;

            /// <summary>
            /// Assembly qualified type name of the saved data object.
            /// </summary>
            public string typeName;

            /// <summary>
            /// JSON representation of the saved data.
            /// </summary>
            public string jsonData;
        }

        #endregion

        #region Unity lifecycle

        private void Awake()
        {
            IsSaveEnabled = canTestSaveAndLoad;
        }

        private void Start()
        {
            if (canTestSaveAndLoad)
            {
                Load();
            }
        }

        private void OnApplicationQuit()
        {
            if (!IsSaveEnabled)
                return;

            PlayerPrefs.SetString("LastPlayedTime", DateTime.Now.Ticks.ToString());
            Save();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Disables automatic saving (OnApplicationQuit). Manual calls to Save() are still allowed.
        /// </summary>
        public void DisableSave()
        {
            IsSaveEnabled = false;
        }

        /// <summary>
        /// Captures the state of all ISaveable objects and writes it to disk.
        /// You can also call this from the editor via the context menu.
        /// </summary>
        [ContextMenu("Save")]
        public void Save()
        {
            var saveData = CaptureObjectsState();
            WriteSaveFile(saveData);
        }

        /// <summary>
        /// Loads the save file from disk and restores the state of all ISaveable objects
        /// that can be matched by key.
        /// You can also call this from the editor via the context menu.
        /// </summary>
        [ContextMenu("Load")]
        public void Load()
        {
            var saveData = ReadSaveFile();
            if (saveData == null)
                return;

            RestoreObjectsState(saveData);
        }

        #endregion

        #region Core save / load

        /// <summary>
        /// Builds a unique key for an ISaveable object instance.
        /// The default implementation uses the type full name plus the instance ID.
        /// </summary>
        private static string BuildKeyForSaveable(ISaveable saveable)
        {
            var unityObject = (UnityEngine.Object)saveable;
            int instanceID = unityObject.GetInstanceID();
            return saveable.GetType().FullName + "#" + instanceID.ToString();
        }

        /// <summary>
        /// Captures the current state of all ISaveable objects in the scene.
        /// </summary>
        private SaveFileData CaptureObjectsState()
        {
            var data = new SaveFileData();

            foreach (var saveable in FindAllSaveables(includeInactive: true))
            {
                var state = saveable.Save();
                if (state == null)
                    continue;

                var stateType = state.GetType();
                string json = JsonUtility.ToJson(state);
                string typeName = stateType.AssemblyQualifiedName;
                string key = BuildKeyForSaveable(saveable);

                data.objects.Add(new ObjectSaveData
                {
                    key = key,
                    typeName = typeName,
                    jsonData = json
                });
            }

            return data;
        }

        /// <summary>
        /// Restores the state of all ISaveable objects from the given save data.
        /// </summary>
        private void RestoreObjectsState(SaveFileData saveData)
        {
            if (saveData == null || saveData.objects == null)
                return;

            // Build a quick lookup dictionary by key.
            var lookup = new Dictionary<string, ObjectSaveData>();
            foreach (var objData in saveData.objects)
            {
                if (string.IsNullOrEmpty(objData.key))
                    continue;

                if (!lookup.ContainsKey(objData.key))
                    lookup.Add(objData.key, objData);
            }

            foreach (var saveable in FindAllSaveables(includeInactive: true))
            {
                string key = BuildKeyForSaveable(saveable);
                if (!lookup.TryGetValue(key, out var objData))
                    continue;

                if (string.IsNullOrEmpty(objData.typeName) || string.IsNullOrEmpty(objData.jsonData))
                    continue;

                var type = Type.GetType(objData.typeName);
                if (type == null)
                {
                    Debug.LogWarning($"[SimpleTalentTreeUI] Could not resolve saved type '{objData.typeName}' for key '{key}'.", this);
                    continue;
                }

                try
                {
                    var state = JsonUtility.FromJson(objData.jsonData, type);
                    saveable.Load(state);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[SimpleTalentTreeUI] Failed to deserialize save data for key '{key}': {ex.Message}", this);
                }
            }
        }

        #endregion

        #region File IO

        /// <summary>
        /// Returns the absolute directory path used for saving based on Application.persistentDataPath.
        /// Ensures the directory exists.
        /// </summary>
        private string GetSaveDirectory()
        {
            string saveDirectory = Path.Combine(Application.persistentDataPath, folderName);

            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }

            return saveDirectory;
        }

        /// <summary>
        /// Returns the full path to the save file.
        /// </summary>
        private string GetSaveFilePath()
        {
            string directory = GetSaveDirectory();
            return Path.Combine(directory, fileName);
        }

        /// <summary>
        /// Writes the given SaveFileData to disk as JSON.
        /// </summary>
        private void WriteSaveFile(SaveFileData data)
        {
            try
            {
                string fullPath = GetSaveFilePath();
                string json = JsonUtility.ToJson(data, prettyPrint: false);
                File.WriteAllText(fullPath, json);
#if UNITY_EDITOR
                Debug.Log($"[SimpleTalentTreeUI] Save file written to: {fullPath}", this);
#endif
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SimpleTalentTreeUI] Failed to save file: {ex.Message}", this);
            }
        }

        /// <summary>
        /// Reads the save file from disk and deserializes it into a SaveFileData instance.
        /// Returns null if the file does not exist or cannot be read.
        /// </summary>
        private SaveFileData ReadSaveFile()
        {
            try
            {
                string fullPath = GetSaveFilePath();

                if (!File.Exists(fullPath))
                {
#if UNITY_EDITOR
                    Debug.Log($"[SimpleTalentTreeUI] Save file not found at path: {fullPath}", this);
#endif
                    return null;
                }

                string json = File.ReadAllText(fullPath);
                if (string.IsNullOrEmpty(json))
                    return null;

                var data = JsonUtility.FromJson<SaveFileData>(json);
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SimpleTalentTreeUI] Failed to load file: {ex.Message}", this);
                return null;
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Finds all ISaveable objects in the scene.
        /// </summary>
        private IEnumerable<ISaveable> FindAllSaveables(bool includeInactive)
        {
#if UNITY_2023_1_OR_NEWER
            var inactiveMode = includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude;

            return FindObjectsByType<UnityEngine.Object>(inactiveMode, FindObjectsSortMode.None)
                .OfType<ISaveable>();
#else
            // Fallback for older Unity versions. This does not find inactive objects.
            return FindObjectsOfType<MonoBehaviour>()
                .OfType<ISaveable>();
#endif
        }

        #endregion
    }
}
