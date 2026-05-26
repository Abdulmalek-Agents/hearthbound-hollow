// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Core / EventBus
//
// Zero-allocation pub-sub for decoupled game events.
//
// Usage:
//   EventBus.Subscribe<MemoryPolishedEvent>(OnPolished);
//   EventBus.Publish(new MemoryPolishedEvent { Memory = m, Clarity = 1f });
//   EventBus.Unsubscribe<MemoryPolishedEvent>(OnPolished);
//
// Design notes:
//   * Strongly typed; one Action<T> chain per event type.
//   * Handlers are invoked synchronously in subscription order.
//   * Safe against handler mutation during dispatch: we snapshot the list.
//   * No reflection. No GC unless handlers themselves allocate.
//   * Lives in HearthboundHollow.Core asmdef so every other module can use it
//     without taking deps on UI/Memory/etc.

using System;
using System.Collections.Generic;

namespace HearthboundHollow.Core
{
    public static class EventBus
    {
        // Type → invocation list. We keep a List<Delegate> (not multicast) so we
        // can snapshot before invoking (safe against subscriber mutation).
        private static readonly Dictionary<Type, List<Delegate>> _handlers = new();

        public static void Subscribe<T>(Action<T> handler) where T : struct
        {
            if (handler == null) return;
            if (!_handlers.TryGetValue(typeof(T), out var list))
            {
                list = new List<Delegate>(4);
                _handlers[typeof(T)] = list;
            }
            list.Add(handler);
        }

        public static void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            if (handler == null) return;
            if (_handlers.TryGetValue(typeof(T), out var list))
            {
                list.Remove(handler);
            }
        }

        public static void Publish<T>(T evt) where T : struct
        {
            if (!_handlers.TryGetValue(typeof(T), out var list)) return;
            // Snapshot to be safe against handlers that subscribe/unsubscribe
            // during invocation. Allocates only when there are subscribers.
            var snapshot = list.ToArray();
            for (int i = 0; i < snapshot.Length; i++)
            {
                try
                {
                    ((Action<T>)snapshot[i]).Invoke(evt);
                }
                catch (Exception ex)
                {
                    Hh.Err(LogCategory.Boot, $"EventBus handler for {typeof(T).Name} threw: {ex}");
                }
            }
        }

        /// <summary>Convenience for tests + domain reload — clears every subscriber.</summary>
        public static void ClearAll()
        {
            _handlers.Clear();
        }

        /// <summary>How many subscribers does a given event type have? Test-helper.</summary>
        public static int SubscriberCount<T>() where T : struct
        {
            return _handlers.TryGetValue(typeof(T), out var list) ? list.Count : 0;
        }
    }
}
