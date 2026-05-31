// SPDX-License-Identifier: MIT
// Hearthbound Hollow - UI / TmpEmojiFallbackSanitizer
//
// PHASE 76.1 - guarantees the Mission-1 "InvalidCastException: Specified cast is
// not valid" in TextMeshProUGUI.GenerateTextMesh can never recur.
//
// ROOT CAUSE: TMP Settings' m_EmojiFallbackTextAssets is a list of emoji-fallback
// *font* assets (TMP_FontAsset). If a TMP_SpriteAsset (e.g. HollowGlyphs) is in
// that list, TMP casts each entry to TMP_FontAsset during text generation and
// throws, aborting the canvas render for ALL text. The committed TMP Settings
// asset has been cleaned, but to be bullet-proof against any future re-add (a
// rebuild, a stray edit), this self-installing guard sanitises the LIVE
// TMP_Settings instance before any text renders - removing any sprite asset (or
// null) from the emoji-font-fallback list. Defensive, no-op when already clean,
// never breaks startup. Runtime only (no scene edit).

using UnityEngine;
using TMPro;

namespace HearthboundHollow.UI
{
    public static class TmpEmojiFallbackSanitizer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Sanitize()
        {
            try
            {
                var settings = TMP_Settings.instance;
                if (settings == null) return;

                var field = typeof(TMP_Settings).GetField(
                    "m_EmojiFallbackTextAssets",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (field == null) return;

                if (!(field.GetValue(settings) is System.Collections.IList list)) return;

                int removed = 0;
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    var el = list[i] as Object;
                    // A FONT fallback list must hold only TMP_FontAsset. Strip sprite
                    // assets (the crash source) and any null/destroyed entries.
                    if (el == null || el is TMP_SpriteAsset)
                    {
                        list.RemoveAt(i);
                        removed++;
                    }
                }

                if (removed > 0)
                    Debug.Log($"[Hearthbound] TMP emoji-fallback sanitised: removed {removed} non-font " +
                              "entr(y/ies) to prevent the GenerateTextMesh InvalidCastException.");
            }
            catch
            {
                // A cosmetic guard must never break game startup.
            }
        }
    }
}
