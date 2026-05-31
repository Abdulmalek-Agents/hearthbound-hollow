
using Exoa.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Exoa.Designer
{
    /// <summary>
    /// The SaveSystem class is responsible for managing file saving and loading for various modes (file system, resources, online).
    /// </summary>
    public class SaveSystem
    {
        public enum Mode { FILE_SYSTEM, RESOURCES, ONLINE };
        public Mode mode;

        public static string defaultSubFolderName = null;
        public static string defaultFileToOpen = null;
        private string resourcesFolderLocation = "/Resources/";

        public string ResourcesFolderLocation { get => resourcesFolderLocation; set => resourcesFolderLocation = value; }

        /// <summary>
        /// Initializes a new instance of the SaveSystem class with the specified mode.
        /// </summary>
        /// <param name="mode">The mode for the SaveSystem (FILE_SYSTEM, RESOURCES, ONLINE).</param>
        public SaveSystem(Mode mode)
        {
            this.mode = mode;

#if ONLINE_MODULE
            if (mode == Mode.FILE_SYSTEM && OnlineModuleSettings.GetSettings().useOnlineMode)
            {
                this.mode = Mode.ONLINE;
            }
#endif
        }

        [Serializable]
        public struct FileList
        {
            public List<string> list;
        }

        /// <summary>
        /// Creates a new instance of SaveSystem with the specified mode.
        /// </summary>
        /// <param name="mode">The mode for the SaveSystem.</param>
        /// <returns>A new instance of SaveSystem.</returns>
        public static SaveSystem Create(Mode mode)
        {
            return new SaveSystem(mode);
        }

        /// <summary>
        /// Gets the base path for storing files, including optional sub-folder.
        /// </summary>
        /// <param name="subFolder">The sub-folder to include in the path.</param>
        /// <returns>The complete base path as a string.</returns>
        public string GetBasePath(string subFolder)
        {
            string path = "";
            if (mode == Mode.RESOURCES)
                path = Application.dataPath;
            else
                path = Application.persistentDataPath + "/";

            if (!string.IsNullOrEmpty(subFolder))
                path += subFolder + "/";

            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            catch (Exception e)
            {
                Debug.LogError("Could not create folder:" + e.Message);
            }
            return path;
        }

        /// <summary>
        /// Refreshes the Unity asset database (only in the editor).
        /// </summary>
        public void RefreshUnityDB()
        {
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        /// <summary>
        /// Load a file item and invoke the callback with the file contents.
        /// </summary>
        /// <param name="fileName">Name of the file to load.</param>
        /// <param name="subFolderName">Name of the sub-folder containing the file.</param>
        /// <param name="pCallback">Callback to execute with the file content.</param>
        /// <param name="ext">File extension, default is ".json".</param>
        public void LoadFileItem(string fileName, string subFolderName, Action<string> pCallback = null, string ext = ".json")
        {
            HDLogger.Log("LoadFileItem " + GetBasePath(subFolderName) + "/" + fileName, HDLogger.LogCategory.FileSystem);

            string content = null;

            try
            {
                if (mode == Mode.RESOURCES)
                {
                    TextAsset o = Resources.Load<TextAsset>(subFolderName + "/" + fileName);
                    content = o != null ? o.text : null;

                    if (!string.IsNullOrEmpty(content))
                        pCallback?.Invoke(content);
                }
                else if (mode == Mode.FILE_SYSTEM)
                {
                    StreamReader stream = File.OpenText(GetBasePath(subFolderName) + fileName + ext);
                    content = stream.ReadToEnd();
                    stream.Close();

                    if (!string.IsNullOrEmpty(content))
                        pCallback?.Invoke(content);
                }
                else if (mode == Mode.ONLINE)
                {
#if ONLINE_MODULE
                    OnlineHelper.Instance.Load(fileName, subFolderName, ext,
                        (UnityWebRequest req, string res) =>
                        {
                            if (!string.IsNullOrEmpty(res))
                                pCallback?.Invoke(res);
                        },
                        OnlineHelper.Instance.DefaultFailCallback);
#else
                    Debug.LogError("Online Module missing!");
#endif
                }
            }
            catch (System.Exception e)
            {
                HDLogger.LogError("Error loading " + subFolderName + "/" + fileName + " " + e.Message, HDLogger.LogCategory.FileSystem);
                AlertPopup.ShowAlert("error", "Error", "Error loading " + subFolderName + "/" + fileName + " " + e.Message);
            }
        }

        /// <summary>
        /// Checks if a file exists and invokes the callback with the result.
        /// </summary>
        /// <param name="fileName">Name of the file to check.</param>
        /// <param name="subFolderName">Name of the sub-folder containing the file.</param>
        /// <param name="ext">File extension, default is ".png".</param>
        /// <param name="pCallback">Callback to invoke with the existence result.</param>
        public void Exists(string fileName, string subFolderName, string ext = ".png", Action<bool> pCallback = null)
        {
            bool exists = false;
            string path = null;
            if (mode == Mode.RESOURCES)
            {
                path = subFolderName + "/" + fileName;
                UnityEngine.Object o = Resources.Load(path);
                exists = o != null;
                HDLogger.Log("Exists " + path + " : " + exists, HDLogger.LogCategory.FileSystem);

                pCallback?.Invoke(exists);
            }
            else if (mode == Mode.FILE_SYSTEM)
            {
                path = GetBasePath(subFolderName) + fileName + ext;
                exists = File.Exists(path);
                HDLogger.Log("Exists " + path + " : " + exists, HDLogger.LogCategory.FileSystem);

                pCallback?.Invoke(exists);
            }
            else if (mode == Mode.ONLINE)
            {
#if ONLINE_MODULE
                OnlineHelper.Instance.Exists(fileName, subFolderName, ext,
                    (UnityWebRequest req, string res) =>
                    {
                        HDLogger.Log("Exists " + path + " : " + exists, HDLogger.LogCategory.FileSystem);

                        pCallback?.Invoke(res == "true");
                    },
                    OnlineHelper.Instance.DefaultFailCallback);
#else
                    Debug.LogError("Online Module missing!");
#endif
            }
        }

        /// <summary>
        /// Loads a texture item and invokes the appropriate callbacks.
        /// </summary>
        /// <param name="fileName">Name of the texture file to load.</param>
        /// <param name="subFolderName">Sub-folder containing the texture file.</param>
        /// <param name="callback">Callback that will be invoked with the loaded texture.</param>
        /// <param name="errorCallback">Callback invoked with error information if loading fails.</param>
        /// <param name="ext">File extension, default is ".png".</param>
        /// <param name="width">Width of the texture to create.</param>
        /// <param name="height">Height of the texture to create.</param>
        public void LoadTextureItem(string fileName, string subFolderName,
            Action<Texture2D> callback, Action<UnityWebRequest, string> errorCallback,
            string ext = ".png",
            int width = 100, int height = 100)
        {
            HDLogger.Log("LoadTextureItem " + fileName, HDLogger.LogCategory.FileSystem);
            Texture2D tex = null;

            if (mode == Mode.RESOURCES)
            {
                tex = Resources.Load<Texture2D>(subFolderName + "/" + fileName);
                if (tex != null)
                    callback?.Invoke(tex);
                else
                    errorCallback?.Invoke(null, "File not found in Resources:" + fileName + ext);
            }
            else if (mode == Mode.ONLINE)
            {
#if ONLINE_MODULE
                OnlineHelper.Instance.Load(fileName, subFolderName, ext,
                    (UnityWebRequest req, string res) =>
                    {
                        byte[] bytes = req.downloadHandler.data;
                        if (bytes == null || bytes.Length < 200)
                        {
                            errorCallback?.Invoke(null, "File not found online:" + fileName + ext);
                            return;
                        }
                        Texture2D t = new Texture2D(100, 100, TextureFormat.RGB24, false);
                        t.LoadImage(bytes);

                        callback?.Invoke(t);
                    },
                    errorCallback);
#else
                    Debug.LogError("Online Module missing!");
#endif
            }
            else if (mode == Mode.FILE_SYSTEM)
            {
                string path = GetBasePath(subFolderName) + fileName + ext;

                byte[] fileData;

                if (File.Exists(path))
                {
                    fileData = File.ReadAllBytes(path);
                    tex = new Texture2D(width, height);
                    tex.LoadImage(fileData);

                    callback?.Invoke(tex);
                }
                else
                {
                    errorCallback?.Invoke(null, "File not found in system files:" + fileName + ext);
                }
            }
        }

        /// <summary>
        /// Saves a JSON file item to the specified path in the format of a string.
        /// </summary>
        /// <param name="fileName">Name of the file to save.</param>
        /// <param name="subFolderName">Sub-folder name where the file will be saved.</param>
        /// <param name="json">The JSON content to save.</param>
        /// <param name="pCallback">Callback to invoke after saving.</param>
        public void SaveFileItem(string fileName, string subFolderName, string json, Action<string> pCallback = null)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(json);
            SaveFileItem(fileName, subFolderName, ".json", bytes, pCallback);
        }

        /// <summary>
        /// Saves a binary file item to the specified path.
        /// </summary>
        /// <param name="fileName">Name of the file to save.</param>
        /// <param name="subFolderName">Sub-folder name where the file will be saved.</param>
        /// <param name="ext">File extension, e.g., ".json"</param>
        /// <param name="bytes">The byte array to save.</param>
        /// <param name="pCallback">Callback to invoke after saving.</param>
        public void SaveFileItem(string fileName, string subFolderName, string ext, byte[] bytes, Action<string> pCallback = null)
        {
            if (bytes == null)
            {
                HDLogger.LogWarning("Warning saving " + subFolderName + "/" + fileName + ext + " nothing to save (empty content)", HDLogger.LogCategory.FileSystem);
                return;
            }
            HDLogger.Log("SaveFileItem " + GetBasePath(subFolderName) + fileName + ext, HDLogger.LogCategory.FileSystem);

            if (mode == Mode.FILE_SYSTEM)
            {
                bool success = false;
                try
                {
                    File.WriteAllBytes(GetBasePath(subFolderName) + fileName + ext, bytes);

                    if (mode == Mode.RESOURCES)
                    {
                        RefreshUnityDB();
                    }
                    success = true;
                }
                catch (System.Exception e)
                {
                    HDLogger.LogError("Error saving " + subFolderName + "/" + fileName + ext + " " + e.Message, HDLogger.LogCategory.FileSystem);
                    AlertPopup.ShowAlert("error", "Error", "Error loading " + subFolderName + "/" + fileName + ext + " " + e.Message);
                }

                if (success)
                    pCallback?.Invoke(fileName);
            }
            else if (mode == Mode.ONLINE)
            {
#if ONLINE_MODULE
                OnlineHelper.Instance.Save(fileName,
                    subFolderName, ext, bytes,
                    (UnityWebRequest req, string res) =>
                    {
                        HDLogger.Log("Saved " + fileName + " : " + res, HDLogger.LogCategory.FileSystem);
                        pCallback?.Invoke(fileName);
                    },
                    OnlineHelper.Instance.DefaultFailCallback);
#else
                    Debug.LogError("Online Module missing!");
#endif
            }
        }

        /// <summary>
        /// Deletes a file item from the specified path.
        /// </summary>
        /// <param name="fileName">Name of the file to delete.</param>
        /// <param name="subFolderName">Sub-folder name from where the file will be deleted.</param>
        /// <param name="pCallback">Callback to invoke after deletion.</param>
        /// <param name="ext">File extension, default is ".json".</param>
        public void DeleteFileItem(string fileName, string subFolderName, Action pCallback = null, string ext = ".json")
        {
            HDLogger.Log("DeleteFileItem " + fileName, HDLogger.LogCategory.FileSystem);

            if (mode == Mode.FILE_SYSTEM)
            {
                try
                {
                    FileInfo fi = new FileInfo(GetBasePath(subFolderName) + fileName + ext);
                    fi.Delete();

                    pCallback?.Invoke();
                }
                catch (System.Exception e)
                {
                    HDLogger.LogError("Error deleting " + subFolderName + "/" + fileName + " " + e.Message, HDLogger.LogCategory.FileSystem);
                    AlertPopup.ShowAlert("error", "Error", e.Message);
                }
                RefreshUnityDB();
            }
            else if (mode == Mode.ONLINE)
            {
#if ONLINE_MODULE
                OnlineHelper.Instance.Delete(fileName, subFolderName, ext,
                    (UnityWebRequest req, string res) =>
                    {
                        HDLogger.Log("Deleted " + fileName + " : " + res, HDLogger.LogCategory.FileSystem);
                        pCallback?.Invoke();
                    },
                    OnlineHelper.Instance.DefaultFailCallback);
#else
                    Debug.LogError("Online Module missing!");
#endif
            }
        }

        /// <summary>
        /// Renames a file item from one name to a new name.
        /// </summary>
        /// <param name="fileName">Current name of the file.</param>
        /// <param name="newName">New name for the file.</param>
        /// <param name="subFolderName">Sub-folder name containing the file.</param>
        /// <param name="pCallback">Callback to invoke after renaming.</param>
        /// <param name="ext">File extension, default is ".json".</param>
        public void RenameFileItem(string fileName, string newName, string subFolderName, Action pCallback = null, string ext = ".json")
        {
            HDLogger.Log("RenameFileItem " + fileName + " " + newName, HDLogger.LogCategory.FileSystem);

            if (mode == Mode.FILE_SYSTEM)
            {
                FileInfo fi = null;
                try
                {
                    fi = new FileInfo(GetBasePath(subFolderName) + fileName + ext);
                    fi.MoveTo(GetBasePath(subFolderName) + newName + ext);

                    pCallback?.Invoke();
                }
                catch (System.Exception e)
                {
                    AlertPopup.ShowAlert("error", "Error", e.Message);
                    HDLogger.LogError("Error renaming " + subFolderName + "/" + fileName + " " + e.Message, HDLogger.LogCategory.FileSystem);
                }
                RefreshUnityDB();
            }
            else if (mode == Mode.ONLINE)
            {
#if ONLINE_MODULE
                OnlineHelper.Instance.Rename(fileName, newName, subFolderName, ext,
                    (UnityWebRequest req, string res) =>
                    {
                        HDLogger.Log("Renamed " + fileName + " : " + res, HDLogger.LogCategory.FileSystem);
                        pCallback?.Invoke();
                    },
                    OnlineHelper.Instance.DefaultFailCallback);
#else
                    Debug.LogError("Online Module missing!");
#endif
            }
        }

        /// <summary>
        /// Copies a file item to a new name in the specified folder.
        /// </summary>
        /// <param name="fileName">Current name of the file to copy.</param>
        /// <param name="newName">New name for the copy.</param>
        /// <param name="subFolderName">Sub-folder name containing the file.</param>
        /// <param name="pCallback">Callback to invoke after copying.</param>
        /// <param name="ext">File extension, default is ".json".</param>
        public void CopyFileItem(string fileName, string newName, string subFolderName, Action pCallback = null, string ext = ".json")
        {
            HDLogger.Log("CopyFileItem " + fileName + " " + newName, HDLogger.LogCategory.FileSystem);

            if (mode == Mode.FILE_SYSTEM)
            {
                FileInfo fi = null;
                try
                {
                    fi = new FileInfo(GetBasePath(subFolderName) + fileName + ext);
                    fi.CopyTo(GetBasePath(subFolderName) + newName + ext);

                    pCallback?.Invoke();
                }
                catch (System.Exception e)
                {
                    AlertPopup.ShowAlert("error", "Error", e.Message);
                    HDLogger.LogError("Error copying " + subFolderName + "/" + fileName + " " + e.Message, HDLogger.LogCategory.FileSystem);
                }
                RefreshUnityDB();
            }
            else if (mode == Mode.ONLINE)
            {
#if ONLINE_MODULE
                OnlineHelper.Instance.Copy(fileName, newName, subFolderName, ext,
                    (UnityWebRequest req, string res) =>
                    {
                        HDLogger.Log("Copied " + fileName + " : " + res, HDLogger.LogCategory.FileSystem);
                        pCallback?.Invoke();
                    },
                    OnlineHelper.Instance.DefaultFailCallback);
#else
                    Debug.LogError("Online Module missing!");
#endif
            }
        }

        /// <summary>
        /// Lists file items in the specified sub-folder and invokes the callback with the list.
        /// </summary>
        /// <param name="subFolderName">Name of the sub-folder to list files from.</param>
        /// <param name="pCallback">Callback to invoke with the list of files.</param>
        /// <param name="ext">File extension filter, default is "*.json".</param>
        public void ListFileItems(string subFolderName, Action<FileList> pCallback = null, string ext = "*.json")
        {
            FileList ll = new FileList();
            ll.list = new List<string>();

            if (mode == Mode.RESOURCES)
            {
                UnityEngine.Object[] files = Resources.LoadAll(subFolderName + "/");
                foreach (UnityEngine.Object o in files)
                {
                    ll.list.Add(o.name);
                }
                HDLogger.Log("ListFileItems " + GetBasePath(subFolderName) + ":" + ll.list.Count, HDLogger.LogCategory.FileSystem);

                pCallback?.Invoke(ll);
            }
            else if (mode == Mode.FILE_SYSTEM)
            {
                DirectoryInfo dir = new DirectoryInfo(GetBasePath(subFolderName));
                FileInfo[] info = dir.GetFiles(ext);
                foreach (FileInfo f in info)
                {
                    ll.list.Add(f.Name);
                }
                HDLogger.Log("ListFileItems " + GetBasePath(subFolderName) + ":" + ll.list.Count, HDLogger.LogCategory.FileSystem);

                pCallback?.Invoke(ll);
            }
            else if (mode == Mode.ONLINE)
            {
#if ONLINE_MODULE
                OnlineHelper.Instance.ListFiles(subFolderName,
                   (UnityWebRequest req, string res) =>
                   {
                       HDLogger.Log("res:" + res, HDLogger.LogCategory.Online);
                       FileList fileList = JsonConvert.DeserializeObject<FileList>(res);
                       if (fileList.list != null)
                       {
                           pCallback?.Invoke(fileList);
                       }
                   },
                   OnlineHelper.Instance.DefaultFailCallback);
#else
                    Debug.LogError("Online Module missing!");
#endif
            }
        }
    }
}
