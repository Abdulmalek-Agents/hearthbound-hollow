using UnityEngine;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

namespace Exoa.Designer
{
    public class StateStorage
    {

        public static T LoadData<T>(string key)
        {
            if (PlayerPrefs.HasKey(key))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                StringReader sr = new StringReader(PlayerPrefs.GetString(key));
                return (T)serializer.Deserialize(sr);
            }
            else
            {
                return default(T);
            }
        }
        public static XmlDocument LoadDataToXml(string key)
        {
            if (PlayerPrefs.HasKey(key))
            {
                //XmlSerializer serializer = new XmlSerializer(typeof(T));
                StringReader sr = new StringReader(PlayerPrefs.GetString(key));

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(sr.ReadToEnd());
                return xmlDoc;
            }
            else
            {
                return null;
            }
        }

        public static void SaveData<T>(string key, T source)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            StringWriter sw = new StringWriter();
            serializer.Serialize(sw, source);
            PlayerPrefs.SetString(key, sw.ToString());
        }

        public static void ClearData(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }

    }
}
