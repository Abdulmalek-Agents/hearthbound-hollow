// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase32_VillageBindingsExtension
//
// Phase 32.1 — Extended Medieval Village prefab discovery.
//
// Phase 15 catalogued 8 prefab roles into MedievalVillageBindings.asset.
// Phase 32 needs more — chimneys for cottages, kettles for the hearth,
// breads for shop displays, hanging-pot props for the bakery interior,
// proper signboard frames, dedicated cobble tiles, and ceiling beams.
//
// Rather than rewrite Phase 15, this extension layer adds a *second*
// ScriptableObject — MedievalVillageBindingsV2.asset — that the new
// Phase 32 builders read alongside the original bindings. Both Phase 15
// and Phase 32 builders can coexist; nothing in the old code path breaks.
//
// USE: Menu → Hearthbound → 🧰 Phase 32.1 — Catalog Extended Village Bindings
//
// Architecture notes:
//   - Per D-027, every Phase exposes TryGet*() lookups.
//   - Per D-035, this is Editor-only.

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HearthboundHollow.EditorTools
{
    public class MedievalVillageBindingsV2 : ScriptableObject
    {
        [Header("Cottage modular pieces")]
        public GameObject solidWallPrefab;
        public GameObject windowWallPrefab;
        public GameObject doorWallPrefab;
        public GameObject roofTilePrefab;
        public GameObject chimneyPrefab;
        public GameObject innerFloorPrefab;

        [Header("Cobble + path")]
        public GameObject cobbleTilePrefab;
        public GameObject stoneBrickPrefab;
        public GameObject stepStairPrefab;

        [Header("Hollow interior dressing")]
        public GameObject hearthPrefab;
        public GameObject kettlePrefab;
        public GameObject breadLoafPrefab;
        public GameObject hangingPotPrefab;
        public GameObject cupboardPrefab;
        public GameObject stoolPrefab;
        public GameObject candleabraPrefab;
        public GameObject thickCandlePrefab;
        public GameObject ceilingBeamPrefab;
        public GameObject bucketPrefab;

        [Header("Lane atmosphere")]
        public GameObject signFramePrefab;
        public GameObject hayBalePrefab;
        public GameObject lanternPostPrefab;
        public GameObject torchLightPrefab;
        public GameObject autumnAlderPrefab;
    }

    public static class Phase32_VillageBindingsExtension
    {
        private const string MVRoot = "Assets/MeshingunStudio";
        private const string BindingsDir = "Assets/_Project/ScriptableObjects/Setup";
        private const string BindingsPath = BindingsDir + "/MedievalVillageBindingsV2.asset";

        [MenuItem("Hearthbound/🧰 Phase 32.1 — Catalog Extended Village Bindings", priority = 30)]
        public static void Build()
        {
            if (!AssetDatabase.IsValidFolder(MVRoot))
            {
                EditorUtility.DisplayDialog("Phase 32.1 — Medieval Village not found",
                    "Cannot find Assets/MeshingunStudio/.\n\n" +
                    "Import the Medieval Village Megapack first, then re-run this menu item.",
                    "OK");
                return;
            }

            EnsureFolder(BindingsDir);

            var b = AssetDatabase.LoadAssetAtPath<MedievalVillageBindingsV2>(BindingsPath);
            if (b == null)
            {
                b = ScriptableObject.CreateInstance<MedievalVillageBindingsV2>();
                AssetDatabase.CreateAsset(b, BindingsPath);
            }

            b.solidWallPrefab     = FindBestMatch("wall_01d_1", "wall_01d", "wall_01a", "wall_01");
            b.windowWallPrefab    = FindBestMatch("wallwindow_01a_1", "wallwindow_01a", "wallwindow");
            b.doorWallPrefab      = FindBestMatch("walldoor_03a", "walldoor_t_01a", "walldoor");
            b.roofTilePrefab      = FindBestMatch("rooftiles_01a", "rooftiles_01b", "rooftiles_01");
            b.chimneyPrefab       = FindBestMatch("chimney_01a", "chimney_01b", "chimney");
            b.innerFloorPrefab    = FindBestMatch("floor_04x04_02", "floor_04x04", "floor_03x03");

            b.cobbleTilePrefab    = FindBestMatch("cornercobble_01a", "cornercobble", "wallcobble");
            b.stoneBrickPrefab    = FindBestMatch("stonebrick_01a", "stonebrick_02", "stonebrick");
            b.stepStairPrefab     = FindBestMatch("stepstair_01c", "stepstair_01a", "stepstair");

            b.hearthPrefab        = FindBestMatch("firepit_02a", "firepit_01a", "firepit");
            b.kettlePrefab        = FindBestMatch("wooden_pitcher_01", "wooden_pitcher", "pot_01a");
            b.breadLoafPrefab     = FindBestMatch("bread_01a", "bread_01", "bread");
            b.hangingPotPrefab    = FindBestMatch("terrapots_01b", "terrapots_01", "pot_01");
            b.cupboardPrefab      = FindBestMatch("cupboard_01a", "cupboard_01b", "cupboard");
            b.stoolPrefab         = FindBestMatch("stool_01a", "chair_01a", "stool");
            b.candleabraPrefab    = FindBestMatch("candleabra_02a", "candleabra_02", "candleabra");
            b.thickCandlePrefab   = FindBestMatch("thickcandle_01a", "thickcandle_01b", "thickcandle");
            b.ceilingBeamPrefab   = FindBestMatch("woodlog_03a", "woodlog_02a", "woodlog");
            b.bucketPrefab        = FindBestMatch("bucket_01a", "bucket", "wickerbasket_02a");

            b.signFramePrefab     = FindBestMatch("signboard_01b", "signboard_01a", "signboard");
            b.hayBalePrefab       = FindBestMatch("sack_apple_01a", "grain_sack_01", "sack");
            b.lanternPostPrefab   = FindBestMatch("streetlantern_01a", "streetlight_01a", "lantern");
            b.torchLightPrefab    = FindBestMatch("torchlight_01a", "torchlight_01b", "torchlight");
            b.autumnAlderPrefab   = FindBestMatch("alder_fall_01a", "alder_fall_01b", "alder_fall");

            EditorUtility.SetDirty(b);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Phase 32.1 — Extended Bindings saved",
                "Extended Medieval Village bindings catalogued at:\n" +
                $"  {BindingsPath}\n\n" +
                $"Cottage walls:   {NameOrMissing(b.solidWallPrefab)} / {NameOrMissing(b.windowWallPrefab)} / {NameOrMissing(b.doorWallPrefab)}\n" +
                $"Roof + chimney:  {NameOrMissing(b.roofTilePrefab)} / {NameOrMissing(b.chimneyPrefab)}\n" +
                $"Cobble + stair:  {NameOrMissing(b.cobbleTilePrefab)} / {NameOrMissing(b.stepStairPrefab)}\n" +
                $"Hearth + kettle: {NameOrMissing(b.hearthPrefab)} / {NameOrMissing(b.kettlePrefab)}\n" +
                $"Bread + cupboard:{NameOrMissing(b.breadLoafPrefab)} / {NameOrMissing(b.cupboardPrefab)}\n" +
                $"Lantern post:    {NameOrMissing(b.lanternPostPrefab)}\n" +
                $"Autumn alder:    {NameOrMissing(b.autumnAlderPrefab)}\n\n" +
                "Any 'MISSING' slot can be manually filled by selecting the bindings " +
                "asset in /Assets/_Project/ScriptableObjects/Setup/ and dragging the " +
                "right prefab into the field.",
                "OK");
        }

        public static MedievalVillageBindingsV2 TryGetBindings() =>
            AssetDatabase.LoadAssetAtPath<MedievalVillageBindingsV2>(BindingsPath);

        private static string NameOrMissing(GameObject go) => go != null ? go.name : "MISSING";

        private static GameObject FindBestMatch(params string[] keywords)
        {
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { MVRoot });
            var best = new List<(int score, GameObject prefab)>();
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var lower = path.ToLowerInvariant();
                if (lower.Contains("/scenes/") || lower.Contains("preview") || lower.Contains("editor")) continue;
                int score = 0;
                foreach (var kw in keywords)
                {
                    string k = kw.ToLowerInvariant();
                    if (lower.Contains(k)) score += 12;
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab != null && prefab.name.ToLowerInvariant().Contains(k)) score += 18;
                }
                if (score == 0) continue;
                best.Add((score, AssetDatabase.LoadAssetAtPath<GameObject>(path)));
            }
            best.Sort((a, b) => b.score.CompareTo(a.score));
            return best.Count > 0 ? best[0].prefab : null;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            var leaf = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
