// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase57_MenuConsolidation
//
// PHASE 57 (D-074) — ONE MENU, ONE BUTTON.
//
// User directive (2026-05-29):
//   "Make the Hearthbound menu include one entry — 🚀 Build Everything, which
//    includes all phases — and remove the Diagnose and Advanced entries."
//
// WHY A RUNTIME PRUNE INSTEAD OF DELETING ~64 ATTRIBUTES
// ──────────────────────────────────────────────────────
// Unity registers every `[MenuItem("Hearthbound/…")]` at assembly load. There is
// no per-attribute "hide" switch. The Advanced / Diagnose items live across ~64
// builder files, and 🚀 Build Everything invokes those very builders BY REFLECTION
// (Phase27_BuildEverything.TryRun), so physically deleting the methods/attributes
// would either break the chain or churn the whole Editor tree. Instead we KEEP
// every builder intact and simply PRUNE the menu at editor load: enumerate every
// `[MenuItem]` whose path begins with "Hearthbound/" and call
// `UnityEditor.Menu.RemoveMenuItem(path)` for all of them except
// "Hearthbound/🚀 Build Everything".
//
// IDEMPOTENT + SELF-HEALING
// ─────────────────────────
// Menu items re-register on every domain reload; this prune re-runs on every
// domain reload (the [InitializeOnLoad] static ctor) plus a delayCall, so the
// Hearthbound menu always settles to a single entry. `RemoveMenuItem` is invoked
// via reflection so a Unity version without the public API degrades to a logged
// warning instead of a compile error (which would take the whole Editor assembly
// — and therefore Build Everything — down with it).

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace HearthboundHollow.EditorTools
{
    [InitializeOnLoad]
    public static class Phase57_MenuConsolidation
    {
        /// <summary>The one and only Hearthbound menu entry the player-facing studio keeps.</summary>
        private const string Keep = "Hearthbound/🚀 Build Everything";

        private const string TopPrefix = "Hearthbound/";

        static Phase57_MenuConsolidation()
        {
            // Menu items register during assembly load; prune just after the
            // editor finishes its first update, and again on every domain reload
            // (this static ctor re-runs each recompile).
            EditorApplication.delayCall += Prune;
        }

        private static void Prune()
        {
            try
            {
                // Resolve UnityEditor.Menu.RemoveMenuItem(string) defensively so a
                // hypothetical API change can never break compilation.
                MethodInfo removeMethod = typeof(Menu).GetMethod(
                    "RemoveMenuItem",
                    BindingFlags.Public | BindingFlags.Static,
                    binder: null, types: new[] { typeof(string) }, modifiers: null);

                if (removeMethod == null)
                {
                    Debug.LogWarning(
                        "[Phase57] UnityEditor.Menu.RemoveMenuItem(string) not found on this Unity " +
                        "version — the Hearthbound menu cannot be auto-pruned. 🚀 Build Everything " +
                        "still works; the extra entries are harmless.");
                    return;
                }

                // Collect every Hearthbound/* menu path except the one we keep.
                var paths = new HashSet<string>(StringComparer.Ordinal);
                foreach (MethodInfo m in TypeCache.GetMethodsWithAttribute<MenuItem>())
                {
                    foreach (MenuItem attr in m.GetCustomAttributes<MenuItem>(false))
                    {
                        string p = attr != null ? attr.menuItem : null;
                        if (string.IsNullOrEmpty(p)) continue;
                        if (!p.StartsWith(TopPrefix, StringComparison.Ordinal)) continue;
                        if (p == Keep) continue;
                        paths.Add(p);
                    }
                }

                int removed = 0;
                foreach (string p in paths)
                {
                    try { removeMethod.Invoke(null, new object[] { p }); removed++; }
                    catch { /* a single stubborn entry must not abort the prune */ }
                }

                // Tidy the now-empty submenu parents (harmless if they don't exist).
                foreach (string parent in new[] { "Hearthbound/⚙️ Advanced", "Hearthbound/🔍 Diagnose Build" })
                    try { removeMethod.Invoke(null, new object[] { parent }); } catch { }

                if (removed > 0)
                    Debug.Log($"[Phase57] Hearthbound menu consolidated — pruned {removed} " +
                              $"entr{(removed == 1 ? "y" : "ies")}; kept only “🚀 Build Everything”.");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Phase57] Menu consolidation skipped: {e.Message}");
            }
        }
    }
}
