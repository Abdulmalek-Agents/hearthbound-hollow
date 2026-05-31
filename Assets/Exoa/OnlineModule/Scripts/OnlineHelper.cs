using Exoa.API;
using Exoa.Designer;
using Exoa.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Exoa.Designer
{
    public class OnlineHelper : MonoBehaviour
    {
        private static OnlineHelper _instance;

        public static OnlineHelper Instance
        {
            get
            {
                if (_instance == null)
                    throw new Exception("OnlineHelper is absent, you must first load the Login scene!");
                return _instance;
            }
        }
        private string ScriptUrl
        {
            get { return OnlineModuleSettings.GetSettings().scriptUrl; }
        }

        private void Awake()
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void DefaultFailCallback(UnityWebRequest arg1, string arg2)
        {
            Debug.Log("DefaultFailCallback " + arg2);
            AlertPopup.ShowAlert("apiError", "Error", arg2);
        }

        public void DefaultSuccessCallback(UnityWebRequest arg1, string arg2)
        {
            Debug.Log("DefaultSuccessCallback " + arg2);
        }

        public void Login(string username, string password,
                Action<UnityWebRequest, string> successCallback,
                Action<UnityWebRequest, string> errorCallback)
        {
            Dictionary<string, string> formParams = new Dictionary<string, string>();

            formParams.Add("username", username);
            formParams.Add("password", password);

            ApiClient ac = new ApiClient();
            ac.debug = true;
            ac.CallApi(this, ScriptUrl + "?action=login",
                ApiClient.Method.POST,
                successCallback, errorCallback,
                formParams);
        }


        public void ListFiles(string folderName, Action<UnityWebRequest, string> successCallback,
                Action<UnityWebRequest, string> errorCallback)
        {
            HDLogger.Log("Calling ListFiles", HDLogger.LogCategory.Online);

            Dictionary<string, string> formParams = new Dictionary<string, string>();
            formParams.Add("folderName", folderName);

            ApiClient ac = new ApiClient();
            ac.debug = true;
            ac.CallApi(this, ScriptUrl + "?action=listFiles&",
                ApiClient.Method.POST,
                successCallback, errorCallback, formParams, pFailOnSuccessFalse: false);
        }

        public void Exists(string fileName, string folderName, string extension,
                Action<UnityWebRequest, string> successCallback,
                Action<UnityWebRequest, string> errorCallback)
        {
            Dictionary<string, string> formParams = new Dictionary<string, string>();

            formParams.Add("fileName", fileName);
            formParams.Add("folderName", folderName);
            formParams.Add("extension", extension);

            ApiClient ac = new ApiClient();
            ac.CallApi(this, ScriptUrl + "?action=exists",
                ApiClient.Method.POST,
                successCallback, errorCallback, formParams, pFailOnSuccessFalse: false);
        }


        public void Load(string fileName, string folderName, string extension,
                Action<UnityWebRequest, string> successCallback,
                Action<UnityWebRequest, string> errorCallback, bool pFailOnSuccessFalse = true)
        {
            Dictionary<string, string> formParams = new Dictionary<string, string>();

            formParams.Add("fileName", fileName);
            formParams.Add("folderName", folderName);
            formParams.Add("extension", extension);

            ApiClient ac = new ApiClient();
            ac.CallApi(this, ScriptUrl + "?action=load",
                ApiClient.Method.POST,
                successCallback, errorCallback, formParams,
                pFailOnSuccessFalse: pFailOnSuccessFalse);
        }

        public void Save(string fileName, string folderName, string extension, byte[] content,
                Action<UnityWebRequest, string> successCallback,
                Action<UnityWebRequest, string> errorCallback)
        {
            Dictionary<string, string> formParams = new Dictionary<string, string>();

            formParams.Add("fileName", fileName);
            formParams.Add("folderName", folderName);
            formParams.Add("extension", extension);

            ApiClient ac = new ApiClient();
            ac.CallApi(this, ScriptUrl + "?action=save",
                ApiClient.Method.POST,
                successCallback, errorCallback,
                formParams, pFileContent: content);
        }

        public void Rename(string fileName, string newName,
                string folderName, string extension,
                Action<UnityWebRequest, string> successCallback,
                Action<UnityWebRequest, string> errorCallback)
        {
            Dictionary<string, string> formParams = new Dictionary<string, string>();

            formParams.Add("fileName", fileName);
            formParams.Add("newFileName", newName);
            formParams.Add("folderName", folderName);
            formParams.Add("extension", extension);

            ApiClient ac = new ApiClient();
            ac.CallApi(this, ScriptUrl + "?action=rename",
                ApiClient.Method.POST,
                successCallback, errorCallback,
                formParams);
        }

        public void Copy(string fileName, string newName,
                string folderName, string extension,
                Action<UnityWebRequest, string> successCallback,
                Action<UnityWebRequest, string> errorCallback)
        {
            Dictionary<string, string> formParams = new Dictionary<string, string>();

            formParams.Add("fileName", fileName);
            formParams.Add("newFileName", newName);
            formParams.Add("folderName", folderName);
            formParams.Add("extension", extension);

            ApiClient ac = new ApiClient();
            ac.CallApi(this, ScriptUrl + "?action=copy",
                ApiClient.Method.POST,
                successCallback, errorCallback,
                formParams);
        }


        public void Delete(string fileName, string folderName, string extension,
           Action<UnityWebRequest, string> successCallback,
               Action<UnityWebRequest, string> errorCallback)
        {
            Dictionary<string, string> formParams = new Dictionary<string, string>();

            formParams.Add("fileName", fileName);
            formParams.Add("folderName", folderName);
            formParams.Add("extension", extension);

            ApiClient ac = new ApiClient();
            ac.CallApi(this, ScriptUrl + "?action=delete",
                ApiClient.Method.POST,
                successCallback, errorCallback,
                formParams);
        }
    }
}
