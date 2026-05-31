// =============================================================================
// EventBus.cs — Hearthbound Hollow
// Decoupled pub/sub event system. All inter-system comms go through this.
// Zero reflection overhead — uses typed Dictionary<Type, Delegate> dispatch.
// Phase 76 — 30-Mission Architecture.
// =============================================================================
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HearthboundHollow.Core
{
    /// <summary>
    /// Static event bus. Publish an event; every subscriber receives it that frame.
    /// Events are structs (zero GC) — keep them small.
    /// Thread safety: Unity main thread only.
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> _handlers = new();

        // ─── Subscribe ───────────────────────────────────────────────────────

        /// <summary>Subscribe to events of type <typeparamref name="TEvent"/>.</summary>
        public static void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : struct
        {
            var type = typeof(TEvent);
            if (_handlers.TryGetValue(type, out var existing))
                _handlers[type] = Delegate.Combine(existing, handler);
            else
                _handlers[type] = handler;
        }

        // ─── Unsubscribe ─────────────────────────────────────────────────────

        /// <summary>Unsubscribe a handler. Always call in OnDestroy.</summary>
        public static void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : struct
        {
            var type = typeof(TEvent);
            if (_handlers.TryGetValue(type, out var existing))
            {
                var removed = Delegate.Remove(existing, handler);
                if (removed == null)
                    _handlers.Remove(type);
                else
                    _handlers[type] = removed;
            }
        }

        // ─── Publish ─────────────────────────────────────────────────────────

        /// <summary>Publish an event to all current subscribers (synchronous, this frame).</summary>
        public static void Publish<TEvent>(TEvent evt) where TEvent : struct
        {
            if (_handlers.TryGetValue(typeof(TEvent), out var handler))
            {
                try
                {
                    ((Action<TEvent>)handler)(evt);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[EventBus] Exception in handler for {typeof(TEvent).Name}: {ex}");
                }
            }
        }

        // ─── Debug ───────────────────────────────────────────────────────────

        /// <summary>Number of registered event types (Editor diagnostic use only).</summary>
        public static int RegisteredTypeCount => _handlers.Count;

        /// <summary>Clear all subscriptions. Call during scene teardown in tests.</summary>
        public static void ClearAll() => _handlers.Clear();
    }
}
