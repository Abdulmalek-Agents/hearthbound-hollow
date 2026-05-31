using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Exoa.Designer
{
    public class Session
    {

        [System.Serializable]
        public class SessionData
        {
            public string login;
            public string password;
        }

        public static SessionData sessionData;

        void Start()
        {

        }

        public static SessionData Get()
        {
            if (sessionData != null)
                return sessionData;
            return Load();
        }
        public static SessionData Load()
        {
            sessionData = StateStorage.LoadData<SessionData>("user");
            return sessionData;
        }

        public static void Save(SessionData s = null)
        {
            sessionData = s;
            StateStorage.SaveData<SessionData>("user", sessionData);
        }
    }
}
