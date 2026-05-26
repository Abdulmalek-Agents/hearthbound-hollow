
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine.Events;


namespace LightMapFusion
{
    public class DataEvent : UnityEvent<EventData> { }
    public class EventData
    {
        public string name;
        private Dictionary<string, object> data;

        public void Init(string name)
        {
            data = new Dictionary<string, object>();
            this.name = name;
        }

        public static EventData Create(string name)
        {
            var data = new EventData();
            data.Init(name);
            return data;
        }

        public EventData Set<T>(string key, T value)
        {
            if (!data.ContainsKey(key))
            {
                data.Add(key, value);
            }
            else
            {
                data[key] = value;
            }
            return this;
        }

        public T Get<T>(string key)
        {
            T result = default(T);
            if (data.ContainsKey(key))
            {
                result = (T)data[key];
            }
            return result;
        }
    }
}
#endif
