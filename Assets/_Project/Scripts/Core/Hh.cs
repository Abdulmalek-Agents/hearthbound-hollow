// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Core / Hh static helpers
//
// A tiny namespace shorthand. Provides the `Hh.Log()` family used everywhere
// in the studio code so we never sprinkle Debug.Log directly. This gives us a
// single switch to gate log output by category and to forward to a custom
// crash reporter later (out of scope for M1-2).

using UnityEngine;

namespace HearthboundHollow.Core
{
    /// <summary>
    /// Log categories — every studio log call carries one. This makes it
    /// trivial to filter spam in the Unity Console during a playtest.
    /// </summary>
    public enum LogCategory
    {
        Boot,
        State,
        Dialogue,
        Memory,
        MiniGame,
        Cutscene,
        Save,
        Audio,
        Input,
        UI,
        Mission,
        Pickle,
        Performance,
        Tests,
    }

    /// <summary>
    /// Single entry-point for runtime logging. Prefer this over UnityEngine.Debug.Log.
    /// </summary>
    public static class Hh
    {
        // Build-time bitfield to mute categories on shipping builds.
        // For M1-2 we keep everything on in Editor and off in Release.
        private const bool VerboseInEditor = true;

        public static void Log(LogCategory cat, string msg, Object context = null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!VerboseInEditor) return;
            UnityEngine.Debug.Log($"[HH/{cat}] {msg}", context);
#endif
        }

        public static void Warn(LogCategory cat, string msg, Object context = null)
        {
            UnityEngine.Debug.LogWarning($"[HH/{cat}] {msg}", context);
        }

        public static void Err(LogCategory cat, string msg, Object context = null)
        {
            UnityEngine.Debug.LogError($"[HH/{cat}] {msg}", context);
        }
    }
}
