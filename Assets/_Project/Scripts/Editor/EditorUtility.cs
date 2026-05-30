// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / EditorUtility  (Build-Everything dialog shim)
//
// PHASE 76 — "press OK once."
//
// 🚀 Build Everything chains ~30 phase builders by reflection. Each builder ends
// with its OWN EditorUtility.DisplayDialog "complete" popup, so one build forced
// the user to click OK ~30 times. Unity exposes no global way to suppress
// EditorUtility.DisplayDialog.
//
// FIX (zero per-phase edits): every Hearthbound editor builder lives in
// `namespace HearthboundHollow.EditorTools` and calls `EditorUtility.DisplayDialog(…)`
// UNQUALIFIED. C# binds an unqualified type name to a type in the CURRENT
// namespace BEFORE one pulled in by a `using` directive — so this same-namespace
// `EditorUtility` shim transparently shadows `UnityEditor.EditorUtility` across
// every builder, with no edits to the builders themselves. It forwards every
// member the builders use 1:1 to the real API, and only changes behaviour while
// Build Everything is running (Silent == true): the interactive dialogs are
// logged + auto-confirmed instead of shown. Standalone phase runs (Silent ==
// false) behave exactly as before.
//
// VERIFIED COMPLETE: the only UnityEditor.EditorUtility members the entire Editor
// folder uses are DisplayProgressBar / DisplayDialog (3- & 4-arg) / SetDirty /
// ClearProgressBar / DisplayDialogComplex / OpenFilePanel — all forwarded below.
// If a future builder needs another member, add a 1:1 forwarder here (otherwise
// the Editor assembly won't compile, which is an immediate, obvious signal).

using UnityEngine;

namespace HearthboundHollow.EditorTools
{
    /// <summary>
    /// Namespace-local shim that shadows <c>UnityEditor.EditorUtility</c> for every
    /// Hearthbound editor builder. Forwards 1:1 to the real API; only suppresses
    /// interactive dialogs while <see cref="Silent"/> is set (Build Everything),
    /// so the full chain needs a single confirm + a single final summary instead
    /// of ~30 per-phase OK clicks.
    /// </summary>
    public static class EditorUtility
    {
        /// <summary>
        /// True while 🚀 Build Everything runs the full chain. Set/cleared by
        /// Phase27_BuildEverything so the chained phases don't each pop a dialog.
        /// Default false → standalone phase runs behave exactly as before.
        /// </summary>
        public static bool Silent;

        // ── Progress bar (forwarded verbatim — non-modal, never suppressed) ──
        public static void DisplayProgressBar(string title, string info, float progress)
            => UnityEditor.EditorUtility.DisplayProgressBar(title, info, progress);

        public static void ClearProgressBar()
            => UnityEditor.EditorUtility.ClearProgressBar();

        // ── Dirty flag (forwarded verbatim) ──
        public static void SetDirty(Object target)
            => UnityEditor.EditorUtility.SetDirty(target);

        // ── Dialogs (auto-confirmed while Silent) ──
        public static bool DisplayDialog(string title, string message, string ok)
        {
            if (Silent) { LogSuppressed(title, message); return true; }
            return UnityEditor.EditorUtility.DisplayDialog(title, message, ok);
        }

        public static bool DisplayDialog(string title, string message, string ok, string cancel)
        {
            if (Silent) { LogSuppressed(title, message); return true; }   // auto-proceed during a full build
            return UnityEditor.EditorUtility.DisplayDialog(title, message, ok, cancel);
        }

        public static int DisplayDialogComplex(string title, string message, string ok, string cancel, string alt)
        {
            if (Silent) { LogSuppressed(title, message); return 0; }      // 0 = primary / "ok" option
            return UnityEditor.EditorUtility.DisplayDialogComplex(title, message, ok, cancel, alt);
        }

        // ── File panel (returns empty while Silent so a batch can never block) ──
        public static string OpenFilePanel(string title, string directory, string extension)
            => Silent ? string.Empty : UnityEditor.EditorUtility.OpenFilePanel(title, directory, extension);

        private static void LogSuppressed(string title, string message)
        {
            string first = message ?? string.Empty;
            int nl = first.IndexOf('\n');
            if (nl >= 0) first = first.Substring(0, nl);
            Debug.Log($"[Hearthbound/Build Everything] (dialog auto-confirmed) {title} — {first}");
        }
    }
}
