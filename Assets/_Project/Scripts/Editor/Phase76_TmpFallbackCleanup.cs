// SPDX-License-Identifier: MIT
// Hearthbound Hollow - Editor / Phase76_TmpFallbackCleanup
//
// PHASE 76.1 (editor side) - keeps the TMP Settings asset tidy on disk: strips any
// TMP_SpriteAsset (and null) from m_EmojiFallbackTextAssets whenever the editor
// loads / recompiles. That list is TMP's emoji-fallback FONT assets; a sprite
// asset there is what made TextMeshProUGUI.GenerateTextMesh throw the Mission-1
// InvalidCastException.
//
// Pairs with the runtime TmpEmojiFallbackSanitizer (which protects PLAY): this
// keeps the serialized asset clean even if a builder (e.g. Phase 54) re-adds the
// sprite, so the bad entry never persists in source control. Only saves when it
// actually removed something (no asset churn). Never breaks editor load.

using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using TMPro;

namespace HearthboundHollow.EditorTools
{
    [InitializeOnLoad]
    public static class Phase76_TmpFallbackCleanup
    {
        static Phase76_TmpFallbackCleanup()
        {
            // Defer so the AssetDatabase is ready when we touch TMP Settings.
            EditorApplication.delayCall += Clean;
        }

        private static void Clean()
        {
            try
            {
                var settings = TMP_Settings.instance;
                if (settings == null) return;

                var field = typeof(TMP_Settings).GetField(
                    "m_EmojiFallbackTextAssets",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (field == null) return;

                if (!(field.GetValue(settings) is IList list)) return;

                int removed = 0;
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    var el = list[i] as Object;
                    if (el == null || el is TMP_SpriteAsset)
                    {
                        list.RemoveAt(i);
                        removed++;
                    }
                }

                if (removed > 0)
                {
                    UnityEditor.EditorUtility.SetDirty(settings);
                    AssetDatabase.SaveAssets();
                    Debug.Log($"[Hearthbound/Phase76] Removed {removed} sprite/null entr(y/ies) from TMP " +
                              "emoji-font fallback (prevents the GenerateTextMesh InvalidCastException).");
                }
            }
            catch
            {
                // A tidy-up guard must never break editor load.
            }
        }
    }
}
