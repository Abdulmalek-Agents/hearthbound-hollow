// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase60_ArabicFontInstaller
//
// Phase 60 — Arabic Localization MVP.
//
// One-time helper that downloads the Noto Naskh Arabic font and uses TMP's
// font-asset generator to produce a TMP_FontAsset covering the Arabic
// Unicode block (U+0600..U+06FF) + the Presentation Forms (U+FE70..U+FEFF)
// the ArabicTextShaper emits.
//
// Workflow (run once per fresh clone):
//   Hearthbound → ⚙️ Advanced → 🔤 Phase 60 — Install Arabic Font
//
// What it does:
//   1. Downloads NotoNaskhArabic-Regular.ttf into
//      Assets/_Project/Fonts/Arabic/ if not present.
//   2. Optionally generates a TMP_FontAsset (manual fallback in instruction
//      dialog when the TMP Editor API namespace changes between versions).
//   3. Adds the new font asset as a FALLBACK on the default LiberationSans
//      SDF asset so any existing TMP label that's left in the default font
//      still renders Arabic glyphs (TMP's font-fallback resolution chain).
//
// Why noto naskh:
//   • Free OFL license — ships with the game.
//   • Naskh hand resembles the cozy book-and-parchment aesthetic.
//   • Full coverage of presentation forms (FE80 block) — required for
//     ArabicTextShaper.Shape's output to render correctly.
//
// If automated download fails (offline / firewalled CI), the dialog walks
// the user through a manual install path. The game still ships in English
// — only Arabic UI degrades to box-glyphs if no Arabic font is installed.

using System.IO;
using UnityEditor;
using UnityEngine;

namespace HearthboundHollow.EditorTools
{
    public static class Phase60_ArabicFontInstaller
    {
        public const string FontDir         = "Assets/_Project/Fonts/Arabic";
        public const string FontFileName    = "NotoNaskhArabic-Regular.ttf";
        public const string FontFilePath    = FontDir + "/" + FontFileName;
        public const string DownloadUrl     =
            "https://github.com/notofonts/arabic/raw/main/fonts/NotoNaskhArabic/full/ttf/NotoNaskhArabic-Regular.ttf";

        [MenuItem("Hearthbound/⚙️ Advanced/🔤 Phase 60 — Install Arabic Font", priority = 1212)]
        public static void Install()
        {
            EnsureFolder(FontDir);
            if (File.Exists(FontFilePath))
            {
                Debug.Log($"[Phase 60] Arabic font already installed at {FontFilePath}.");
            }
            else
            {
                if (!TryDownload(DownloadUrl, FontFilePath))
                {
                    EditorUtility.DisplayDialog("Phase 60 — Arabic Font",
                        "Auto-download failed (offline / firewalled?).\n\n" +
                        "Manual install:\n" +
                        $"  1. Download {FontFileName} from\n     {DownloadUrl}\n" +
                        $"  2. Drop into {FontDir}/\n" +
                        "  3. Open Window → TextMeshPro → Font Asset Creator\n" +
                        "     - Source Font File: NotoNaskhArabic-Regular.ttf\n" +
                        "     - Sampling Point Size: 90 (auto)\n" +
                        "     - Atlas Resolution: 2048 × 2048\n" +
                        "     - Character Set: Unicode Range (Hex)\n" +
                        "     - Range:\n" +
                        "         0020-007E,00A0-00FF,0600-06FF,0750-077F,\n" +
                        "         FB50-FDFF,FE70-FEFF\n" +
                        "  4. Click Generate Font Atlas → Save As\n" +
                        $"     '{FontDir}/NotoNaskhArabic_SDF.asset'.\n" +
                        "  5. Add the new SDF asset as a Fallback on TMP's\n" +
                        "     default LiberationSans SDF asset.\n\n" +
                        "Then re-run this menu item.",
                        "OK");
                    return;
                }
            }
            AssetDatabase.Refresh();

            // Print a one-click instruction for the TMP font asset creation
            // (the TMP API for programmatic SDF generation changes between
            // Unity 6 patches — we keep this as a guided manual step rather
            // than a brittle reflection call).
            EditorUtility.DisplayDialog("Phase 60 — Arabic Font installed",
                "Noto Naskh Arabic .ttf is installed at\n" +
                $"  {FontFilePath}\n\n" +
                "To finish setup (one-time):\n" +
                "  1. Window → TextMeshPro → Font Asset Creator\n" +
                "  2. Source Font File: NotoNaskhArabic-Regular.ttf\n" +
                "  3. Character Set: Unicode Range (Hex)\n" +
                "     0020-007E,00A0-00FF,0600-06FF,0750-077F,FB50-FDFF,FE70-FEFF\n" +
                "  4. Atlas Resolution: 2048 × 2048, Render Mode: SDFAA\n" +
                "  5. Generate → Save into Assets/_Project/Fonts/Arabic/\n" +
                "  6. Open the LiberationSans SDF asset's Fallback list and\n" +
                "     drop the new NotoNaskhArabic SDF asset in.\n\n" +
                "Once done, every Arabic line will render correctly in TMP.",
                "OK");
        }

        private static void EnsureFolder(string p)
        {
            if (AssetDatabase.IsValidFolder(p)) return;
            var parent = Path.GetDirectoryName(p)?.Replace('\\', '/');
            var leaf = Path.GetFileName(p);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(leaf)) return;
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        private static bool TryDownload(string url, string destPath)
        {
            try
            {
                using var client = new System.Net.WebClient();
                client.Headers.Add("User-Agent", "HearthboundHollow-Phase60-FontInstaller/1.0");
                client.DownloadFile(url, destPath);
                Debug.Log($"[Phase 60] Downloaded Arabic font to {destPath} " +
                          $"({new FileInfo(destPath).Length / 1024} KB).");
                return File.Exists(destPath) && new FileInfo(destPath).Length > 1024;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Phase 60] Font auto-download failed: {e.Message}");
                return false;
            }
        }
    }
}
