
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "Materials Revert Data", menuName = "Orly Tools/BLM Tool/Materials Revert Data", order = 5)]
namespace LightMapFusion
{
    public class MaterialsRevertData : ScriptableObject
    {
        public Dictionary<string, string> values = new Dictionary<string, string>();
        public List<string> keys = new List<string>();

        public void AddValue(string key, string Value)
        {
            values.Add(key, Value);
        }

        public string GetValue(string key)
        {
            if (values.ContainsKey(key))
            {
                return values[key];
            }
            return null;
        }

        public void CopyToList()
        {
            keys.Clear();
            foreach (KeyValuePair<string, string> kvp in values)
            {
                keys.Add(kvp.Key);
            }
        }
    }

}
#endif