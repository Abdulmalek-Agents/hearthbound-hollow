using Exoa.Designer;
using Exoa.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
//using static Exoa.Designer.SaveSystem;

namespace Exoa.API
{
    public class ApiTester : MonoBehaviour
    {
        public TMP_Text outputTxt;
        public Image img;



        private void Start()
        {
            Log("OnLoggedIn ");

            TestStruct data = new TestStruct();
            data.name = "test";

            string json = JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            byte[] bytes = Encoding.UTF8.GetBytes(json);

            // saving a test file
            OnlineHelper.Instance.Save("test", "Interiors", ".json", bytes, OnFileSaved, OnlineHelper.Instance.DefaultFailCallback);
        }

        private void OnFileSaved(UnityWebRequest arg1, string arg2)
        {
            Log("OnFileSaved " + arg2);


            // Checking if that file exists
            OnlineHelper.Instance.Exists("test", "Interiors", ".json", OnFileExistChecked,
                OnlineHelper.Instance.DefaultFailCallback);
        }

        private void OnFileExistChecked(UnityWebRequest arg1, string arg2)
        {
            Log("OnFileExistChecked " + arg2);

            // Loading that test file back
            OnlineHelper.Instance.Load("test", "Interiors", ".json", OnFileLoaded, OnlineHelper.Instance.DefaultFailCallback);
        }

        private void OnFileLoaded(UnityWebRequest arg1, string arg2)
        {
            TestStruct data = JsonConvert.DeserializeObject<TestStruct>(arg2);
            Log("File Loaded:" + data + " " + data.name);

            // Renaming the demo file
            OnlineHelper.Instance.Rename("test", "test2", "Interiors", ".json", OnFileRenamed, OnlineHelper.Instance.DefaultFailCallback);

        }

        private void OnFileRenamed(UnityWebRequest arg1, string arg2)
        {
            Log("OnFileRenamed " + arg2);

            // Listing all user's files
            OnlineHelper.Instance.ListFiles("Interiors", OnGetFileList, OnlineHelper.Instance.DefaultFailCallback);

        }

        private void OnGetFileList(UnityWebRequest arg1, string arg2)
        {
            Log("OnGetFileList:" + arg2);

            FileList fileList = JsonConvert.DeserializeObject<FileList>(arg2);
            if (fileList.list != null) Log("fileList:" + fileList.list.Count);

            // Deleting that test file
            OnlineHelper.Instance.Delete("test2", "Interiors", ".json", OnFileDelete, OnlineHelper.Instance.DefaultFailCallback);
        }

        private void OnFileDelete(UnityWebRequest arg1, string arg2)
        {
            Log("OnFileDelete " + arg2);

            Log("File deleted successfully!");

            // Checking if that file exists
            OnlineHelper.Instance.Exists("test2", "Interiors", ".json", OnFileExistChecked2, OnlineHelper.Instance.DefaultFailCallback);
        }

        private void OnFileExistChecked2(UnityWebRequest arg1, string arg2)
        {
            Log("OnFileExistChecked " + arg2);

            Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture();
            byte[] bytes = tex.EncodeToPNG();

            //OnlineModuleSettings.GetSettings().scriptUrl = "http://localhost/HomeDesigner/echo_post_get.php";
            OnlineHelper.Instance.Save("Test_Texture",
                HDSettings.EXT_THUMBNAIL_FOLDER, ".png", bytes,
                OnTextureSaved, OnlineHelper.Instance.DefaultFailCallback);
        }

        private void OnTextureSaved(UnityWebRequest arg1, string arg2)
        {
            Log("OnTextureSaved " + arg2);

            //loading the texture back
            OnlineHelper.Instance.Load("Test_Texture",
                HDSettings.EXT_THUMBNAIL_FOLDER, ".png",
                OnTextureLoaded, OnlineHelper.Instance.DefaultFailCallback);
        }

        private void OnTextureLoaded(UnityWebRequest req, string arg2)
        {
            Log("OnTextureLoaded ");
            byte[] bytes = req.downloadHandler.data;

            Texture2D tex = new Texture2D(100, 100, TextureFormat.RGB24, false);
            tex.LoadImage(bytes);

            img.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);

        }

        private void Log(string msg)
        {
            Debug.Log(msg);
            if (outputTxt != null)
                outputTxt.text += msg + "\n";
        }


        [Serializable]
        public struct TestStruct
        {
            public string name;
        }

        [Serializable]
        public struct FileList
        {
            public List<string> list;
        }

    }
}
