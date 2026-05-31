// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Core / VillageStateFlags  (Engagement Pillar P2 — Phase 62)
//
// A tiny reflection-backed resolver that lets DESIGNERS gate content by writing a
// VillageState bool-field name as a STRING (e.g. "firstMoralChoiceMade",
// "metDoris", "echoHologramHeard") in a RequestSO / EchoSO / AlmanacSO — without
// any code change. Mirrors the string-id approach the Yarn bridge already uses
// (Docs/Engagement_Bible/05 §4 note).
//
// • IsSet(flag)   → reads the named bool on the live VillageState (false if missing).
// • Set(flag,val) → writes it (no-op if the field doesn't exist or isn't a bool).
// • FieldInfo is cached, so repeated lookups are cheap.
//
// Lives in HearthboundHollow.Core (same asmdef as VillageState). No new deps.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace HearthboundHollow.Core
{
    public static class VillageStateFlags
    {
        // Cache: lowercased flag name -> FieldInfo (bool fields only). Built lazily.
        private static readonly Dictionary<string, FieldInfo> _boolFields =
            new(StringComparer.OrdinalIgnoreCase);
        private static bool _scanned;

        private static void EnsureScanned()
        {
            if (_scanned) return;
            _scanned = true;
            foreach (var f in typeof(VillageState).GetFields(BindingFlags.Public | BindingFlags.Instance))
                if (f.FieldType == typeof(bool))
                    _boolFields[f.Name] = f;
        }

        private static VillageState Resolve(VillageState vs)
            => vs != null ? vs : ServiceLocator.Get<VillageState>();

        /// <summary>True iff the named VillageState bool exists and is currently true.</summary>
        public static bool IsSet(string flag, VillageState vs = null)
        {
            if (string.IsNullOrWhiteSpace(flag)) return false;
            var state = Resolve(vs);
            if (state == null) return false;
            EnsureScanned();
            return _boolFields.TryGetValue(flag.Trim(), out var f) && (bool)f.GetValue(state);
        }

        /// <summary>Set the named VillageState bool (silently ignored if it doesn't exist).</summary>
        public static void Set(string flag, bool value, VillageState vs = null)
        {
            if (string.IsNullOrWhiteSpace(flag)) return;
            var state = Resolve(vs);
            if (state == null) return;
            EnsureScanned();
            if (_boolFields.TryGetValue(flag.Trim(), out var f)) f.SetValue(state, value);
        }

        /// <summary>True iff EVERY flag in the list is set (empty/null list = true).</summary>
        public static bool AllSet(IEnumerable<string> flags, VillageState vs = null)
        {
            if (flags == null) return true;
            var state = Resolve(vs);
            foreach (var fl in flags)
                if (!string.IsNullOrWhiteSpace(fl) && !IsSet(fl, state)) return false;
            return true;
        }

        /// <summary>True iff ANY flag in the list is set (empty/null list = false).</summary>
        public static bool AnySet(IEnumerable<string> flags, VillageState vs = null)
        {
            if (flags == null) return false;
            var state = Resolve(vs);
            foreach (var fl in flags)
                if (!string.IsNullOrWhiteSpace(fl) && IsSet(fl, state)) return true;
            return false;
        }
    }
}
