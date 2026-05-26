#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace LightMapFusion
{
    public class EventManagerOrly : MonoBehaviour
    {
        private Dictionary<string, DataEvent> events;

        private static EventManagerOrly eventManager;
        private static EventManagerOrly instasnce
        {
            get
            {
                if (!eventManager)
                {
                    eventManager = FindAnyObjectByType<EventManagerOrly>();
                    if (!eventManager)
                    {
                        Debug.LogError("No event Manager in the scene");
                    }
                    else
                    {
                        eventManager.Init();
                    }
                }
                return eventManager;
            }
        }

        private void Init()
        {
            if (events == null)
            {
                events = new Dictionary<string, DataEvent>();
            }
        }

        public static void StartListening(string eventName, UnityAction<EventData> listener)
        {
            DataEvent e = null;
            if (instasnce.events.TryGetValue(eventName, out e))
            {
                e.AddListener(listener);
            }
            else
            {
                e = new DataEvent();
                e.AddListener(listener);
                instasnce.events.Add(eventName, e);
            }
        }

        public static void StopListening(string eventName, UnityAction<EventData> listener)
        {
            DataEvent e = null;
            if (eventManager == null)
            {
                return;
            }
            if (instasnce.events.TryGetValue(eventName, out e))
            {
                e.RemoveListener(listener);
            }
        }

        public static void TriggerEvent(EventData data)
        {
            DataEvent e = null;
            if (instasnce.events.TryGetValue(data.name, out e))
            {
                Debug.Log("Invocando");
                e.Invoke(data);
            }
        }

        private void OnDestroy()
        {
            if (eventManager == null)
            {
                return;
            }
            foreach (var e in instasnce.events.Values)
            {
                e.RemoveAllListeners();
            }
        }
    }
}
#endif