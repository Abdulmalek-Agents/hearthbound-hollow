using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using Exoa.API;

namespace Exoa.Designer
{
    public class LoginController : MonoBehaviour
    {
        public static bool isLoggedIn;

        public enum NextScene { FloorMapEditor, HomeDesigner, ApiTester };
        public NextScene nextScene;

        public Button submitBtn;
        public TMP_InputField loginTxt;
        public TMP_InputField passTxt;
        public GameObject loadingGo;

        public OnlineHelper onlineHelper;


        void Start()
        {
            isLoggedIn = false;
            Session.SessionData s = Session.Get();
            if (s != null && !string.IsNullOrEmpty(s.login) && !string.IsNullOrEmpty(s.password))
            {
                onlineHelper.Login(s.login, s.password, OnLoginSuccess, OnLoginFailed);
            }
            submitBtn.onClick.AddListener(OnClickSubmit);
            loadingGo.SetActive(false);
        }


        public void OnClickSubmit()
        {
            if (loginTxt.text != "" && passTxt.text != "")
                Submit();
        }

        void Submit()
        {
            LoginWith(loginTxt.text, passTxt.text);
        }

        public void LoginWith(string login, string pwd)
        {
            loadingGo.SetActive(true);
            onlineHelper.Login(login, pwd, OnLoginSuccess, OnLoginFailed);


        }


        private void OnLoginFailed(UnityWebRequest arg1, string arg2)
        {
            Debug.Log("OnLoginFailed " + arg2);
            loadingGo.SetActive(false);
        }

        void OnLoginSuccess(UnityWebRequest arg1, string arg2)
        {
            Debug.Log("OnLoginSuccess " + arg2);
            isLoggedIn = true;
            Session.SessionData s = new Session.SessionData();
            s.login = loginTxt.text;
            s.password = passTxt.text;
            Session.Save(s);

            if (loadingGo != null) loadingGo.SetActive(false);

            UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene.ToString());
        }
    }
}
