// =============================================================================
// ServiceLocator.cs — Hearthbound Hollow
// Lightweight service locator. Replaces heavy singleton inheritance.
// Phase 76 — 30-Mission Architecture.
// =============================================================================
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HearthboundHollow.Core
{
    /// <summary>
    /// Static service registry. Register a service once (usually GameManager.Awake);
    /// retrieve it anywhere via <c>ServiceLocator.Get&lt;T&gt;()</c>.
    /// Returns null gracefully if a service is missing — never throws.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new();

        // ─── Register ────────────────────────────────────────────────────────

        /// <summary>Register <paramref name="instance"/> as the singleton for type <typeparamref name="T"/>.</summary>
        public static void Register<T>(T instance) where T : class
        {
            if (instance == null)
            {
                Debug.LogWarning($"[ServiceLocator] Attempted to register null for {typeof(T).Name}.");
                return;
            }
            _services[typeof(T)] = instance;
        }

        /// <summary>Register using a concrete type key (for interface-typed lookups).</summary>
        public static void RegisterAs<TInterface, TConcrete>(TConcrete instance)
            where TConcrete : class, TInterface
        {
            _services[typeof(TInterface)] = instance;
        }

        // ─── Get ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Retrieve the registered service for <typeparamref name="T"/>.
        /// Returns null if not registered — callers must null-check.
        /// </summary>
        public static T Get<T>() where T : class
        {
            _services.TryGetValue(typeof(T), out var obj);
            return obj as T;
        }

        // ─── Unregister ──────────────────────────────────────────────────────

        public static void Unregister<T>() => _services.Remove(typeof(T));

        /// <summary>Clears all registrations. Use in tests / scene teardown.</summary>
        public static void ClearAll() => _services.Clear();
    }

    /// <summary>
    /// Implement this on PlayerController so Mission asmdef can lock movement
    /// without a direct compile dependency on HearthboundHollow.Player (D-035).
    /// </summary>
    public interface IMovementLockable
    {
        bool MovementLocked { get; set; }
    }
}
