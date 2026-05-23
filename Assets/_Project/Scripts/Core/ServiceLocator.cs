// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Core / ServiceLocator
//
// Lightweight runtime DI for game services (GameManager, SaveService,
// DayCycleManager, SfxLibrary, etc.). Avoids the proliferation of singleton
// MonoBehaviours and lets tests inject fakes easily.
//
// Usage:
//   ServiceLocator.Register<ISaveService>(new SaveService());
//   var save = ServiceLocator.Get<ISaveService>();
//   ServiceLocator.Unregister<ISaveService>();

using System;
using System.Collections.Generic;

namespace HearthboundHollow.Core
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new();

        public static void Register<T>(T service) where T : class
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (_services.ContainsKey(typeof(T)))
                Hh.Warn(LogCategory.Boot, $"ServiceLocator: replacing existing registration for {typeof(T).Name}");
            _services[typeof(T)] = service;
        }

        public static T Get<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var s)) return (T)s;
            Hh.Warn(LogCategory.Boot, $"ServiceLocator: no service registered for {typeof(T).Name}");
            return null;
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            if (_services.TryGetValue(typeof(T), out var s)) { service = (T)s; return true; }
            service = null;
            return false;
        }

        public static void Unregister<T>() where T : class
        {
            _services.Remove(typeof(T));
        }

        public static void Clear()
        {
            _services.Clear();
        }

        public static int Count => _services.Count;
    }
}
