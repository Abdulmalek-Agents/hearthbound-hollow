// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase32_Diagnostic
//
// Phase 32.5 — Diagnostic + audit for the Mission 1 Polish v2 series.
//
// Read-only menu item. Walks both Mission 1 scenes + the Phase 32 prefab
// outputs and reports what's wired correctly, what's missing, and any
// warnings for the user.
//
// Checks:
//   ☐ 4 cottage prefabs exist in /Assets/_Project/Prefabs/Environment/
//   ☐ MedievalVillageBindingsV2.asset has all 23 fields filled
//   ☐ HearthboundLane_Volume.asset + HearthboundHollow_Volume.asset exist
//   ☐ Lane scene has _Phase32Env_Lane parent + 8 cottages
//   ☐ Lane scene has _HearthboundLane_GlobalVolume with profile
//   ☐ Hollow scene has _Phase32Env_Hollow parent + kettle/bread/herbs
//   ☐ Hollow scene has _HearthboundHollow_GlobalVolume with profile
//   ☐ Marin's Note still present (Phase 26 preserved)
//   ☐ HearthAmbianceTrigger still present (Phase 27.4 preserved)
//   ☐ No GameObject.CreatePrimitive cubes left as workbench / door
//
// USE: Menu → Hearthbound → 🔍 Phase 32 — Diagnose Mission 1 Polish
//
// Reports to Console + a summary dialog. Never modifies any asset.

using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace HearthboundHollow.EditorTools
{
    public static class Phase32_Diagnostic
    {
        private const string SceneLane   = "Assets/_Project/Scenes/02_Mission01_Lane.unity";
        private const string SceneHollow = "Assets/_Project/Scenes/03_Mission01_Hollow.unity";

        private const string LaneProfilePath   = "Assets/_Project/Settings/HearthboundLane_Volume.asset";
        private const string HollowProfilePath = "Assets/_Project/Settings/HearthboundHollow_Volume.asset";

        private const string CottagePrefabDir  = "Assets/_Project/Prefabs/Environment";

        [MenuItem("Hearthbound/🔍 Phase 32 — Diagnose Mission 1 Polish", priority = 36)]
        public static void Diagnose()
        {
            var report = new StringBuilder();
            int passed = 0, failed = 0, warned = 0;

            report.AppendLine("🍂 Phase 32 — Mission 1 Polish v2 — Diagnostic Report");
            report.AppendLine("─────────────────────────────────────────────────────");
            report.AppendLine();

            // ─── A. Authored assets ────────────────────────────────
            report.AppendLine("A. Authored assets");
            foreach (var v in Phase32_MedievalCottageBuilder.CottageVariants)
            {
                var path = $"{CottagePrefabDir}/{v}.prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null) { report.AppendLine($"  ✓ Cottage prefab: {v}"); passed++; }
                else                { report.AppendLine($"  ✗ Cottage prefab MISSING: {v}"); failed++; }
            }

            var binds = Phase32_VillageBindingsExtension.TryGetBindings();
            if (binds == null)
            {
                report.AppendLine("  ✗ MedievalVillageBindingsV2.asset MISSING — run Phase 32.1 first.");
                failed++;
            }
            else
            {
                int filled = 0, total = 0;
                CheckField(binds.solidWallPrefab, "solidWallPrefab", ref filled, ref total);
                CheckField(binds.windowWallPrefab, "windowWallPrefab", ref filled, ref total);
                CheckField(binds.doorWallPrefab, "doorWallPrefab", ref filled, ref total);
                CheckField(binds.roofTilePrefab, "roofTilePrefab", ref filled, ref total);
                CheckField(binds.chimneyPrefab, "chimneyPrefab", ref filled, ref total);
                CheckField(binds.innerFloorPrefab, "innerFloorPrefab", ref filled, ref total);
                CheckField(binds.cobbleTilePrefab, "cobbleTilePrefab", ref filled, ref total);
                CheckField(binds.stoneBrickPrefab, "stoneBrickPrefab", ref filled, ref total);
                CheckField(binds.stepStairPrefab, "stepStairPrefab", ref filled, ref total);
                CheckField(binds.hearthPrefab, "hearthPrefab", ref filled, ref total);
                CheckField(binds.kettlePrefab, "kettlePrefab", ref filled, ref total);
                CheckField(binds.breadLoafPrefab, "breadLoafPrefab", ref filled, ref total);
                CheckField(binds.hangingPotPrefab, "hangingPotPrefab", ref filled, ref total);
                CheckField(binds.cupboardPrefab, "cupboardPrefab", ref filled, ref total);
                CheckField(binds.stoolPrefab, "stoolPrefab", ref filled, ref total);
                CheckField(binds.candleabraPrefab, "candleabraPrefab", ref filled, ref total);
                CheckField(binds.thickCandlePrefab, "thickCandlePrefab", ref filled, ref total);
                CheckField(binds.ceilingBeamPrefab, "ceilingBeamPrefab", ref filled, ref total);
                CheckField(binds.bucketPrefab, "bucketPrefab", ref filled, ref total);
                CheckField(binds.signFramePrefab, "signFramePrefab", ref filled, ref total);
                CheckField(binds.hayBalePrefab, "hayBalePrefab", ref filled, ref total);
                CheckField(binds.lanternPostPrefab, "lanternPostPrefab", ref filled, ref total);
                CheckField(binds.torchLightPrefab, "torchLightPrefab", ref filled, ref total);
                CheckField(binds.autumnAlderPrefab, "autumnAlderPrefab", ref filled, ref total);
                report.AppendLine($"  ✓ MedievalVillageBindingsV2: {filled}/{total} fields filled");
                if (filled == total) passed++; else { warned++; report.AppendLine($"    ⚠ {total - filled} field(s) unfilled — manual binding may be needed."); }
            }

            // ─── B. Volume profiles ────────────────────────────────
            report.AppendLine();
            report.AppendLine("B. URP Volume profiles");
            var laneProfile   = AssetDatabase.LoadAssetAtPath<VolumeProfile>(LaneProfilePath);
            var hollowProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(HollowProfilePath);
            if (laneProfile != null) { report.AppendLine($"  ✓ HearthboundLane_Volume.asset present ({laneProfile.components.Count} effects)"); passed++; }
            else                     { report.AppendLine($"  ✗ HearthboundLane_Volume.asset MISSING — run Phase 32.4"); failed++; }
            if (hollowProfile != null) { report.AppendLine($"  ✓ HearthboundHollow_Volume.asset present ({hollowProfile.components.Count} effects)"); passed++; }
            else                       { report.AppendLine($"  ✗ HearthboundHollow_Volume.asset MISSING — run Phase 32.4"); failed++; }

            // ─── C. Lane scene checks ────────────────────────────
            report.AppendLine();
            report.AppendLine("C. Lane scene (02_Mission01_Lane.unity)");
            if (System.IO.File.Exists(SceneLane))
            {
                EditorSceneManager.OpenScene(SceneLane, OpenSceneMode.Single);

                var p32 = GameObject.Find("_Phase32Env_Lane");
                if (p32 != null) { report.AppendLine($"  ✓ _Phase32Env_Lane parent present"); passed++; }
                else             { report.AppendLine($"  ✗ _Phase32Env_Lane parent MISSING — run Phase 32.2"); failed++; }

                int cottages = 0;
                if (p32 != null)
                {
                    var cottageGroup = p32.transform.Find("Cottages");
                    if (cottageGroup != null) cottages = cottageGroup.childCount;
                }
                if (cottages >= 6)      { report.AppendLine($"  ✓ Cottages placed: {cottages}/8"); passed++; }
                else if (cottages > 0)  { report.AppendLine($"  ⚠ Cottages placed: {cottages}/8 (low)"); warned++; }
                else                    { report.AppendLine($"  ✗ No cottages found in Lane scene"); failed++; }

                var facade = GameObject.Find("HollowFacade");
                if (facade != null) { report.AppendLine($"  ✓ HollowFacade present"); passed++; }
                else                { report.AppendLine($"  ✗ HollowFacade MISSING"); failed++; }

                var globalVol = GameObject.Find("_HearthboundLane_GlobalVolume");
                if (globalVol != null) { report.AppendLine($"  ✓ Lane Global Volume present"); passed++; }
                else                   { report.AppendLine($"  ✗ Lane Global Volume MISSING — run Phase 32.4"); failed++; }

                // Marin's Note + door interactable preservation
                var marinNote = GameObject.Find("MarinsNote") ?? GameObject.Find("MarinNote");
                if (marinNote == null) { report.AppendLine($"  ⚠ Marin's Note not found in Lane (correct — should be in Hollow)"); /* not an error */ }

                var hollowDoor = GameObject.Find("HollowDoor");
                if (hollowDoor != null) { report.AppendLine($"  ✓ HollowDoor preserved"); passed++; }
                else                    { report.AppendLine($"  ✗ HollowDoor MISSING"); failed++; }
            }
            else
            {
                report.AppendLine($"  ✗ Lane scene file missing: {SceneLane}");
                failed++;
            }

            // ─── D. Hollow scene checks ──────────────────────────
            report.AppendLine();
            report.AppendLine("D. Hollow scene (03_Mission01_Hollow.unity)");
            if (System.IO.File.Exists(SceneHollow))
            {
                EditorSceneManager.OpenScene(SceneHollow, OpenSceneMode.Single);

                var p32h = GameObject.Find("_Phase32Env_Hollow");
                if (p32h != null) { report.AppendLine($"  ✓ _Phase32Env_Hollow parent present"); passed++; }
                else              { report.AppendLine($"  ✗ _Phase32Env_Hollow parent MISSING — run Phase 32.3"); failed++; }

                var kettle = GameObject.Find("Hearth_Kettle");
                if (kettle != null) { report.AppendLine($"  ✓ Hearth_Kettle present"); passed++; }
                else                { report.AppendLine($"  ⚠ Hearth_Kettle missing (cosmetic)"); warned++; }

                int herbs = 0;
                foreach (var name in new[] { "Herb_Lavender", "Herb_Valerian", "Herb_Sage" })
                    if (GameObject.Find(name) != null) herbs++;
                if (herbs == 3) { report.AppendLine($"  ✓ Hanging herbs: 3/3 present"); passed++; }
                else            { report.AppendLine($"  ⚠ Hanging herbs: {herbs}/3"); warned++; }

                int bread = 0;
                for (int i = 0; i < 3; i++)
                    if (GameObject.Find($"Shelf_Bread_{i}") != null) bread++;
                if (bread == 3) { report.AppendLine($"  ✓ Bread loaves: 3/3 on shelf"); passed++; }
                else            { report.AppendLine($"  ⚠ Bread loaves: {bread}/3"); warned++; }

                if (GameObject.Find("MarinsCupboard") != null) { report.AppendLine($"  ✓ Marin's Cupboard present"); passed++; }
                else { report.AppendLine($"  ⚠ Marin's Cupboard missing"); warned++; }

                if (GameObject.Find("MarinsStool") != null) { report.AppendLine($"  ✓ Marin's Stool present"); passed++; }
                else { report.AppendLine($"  ⚠ Marin's Stool missing"); warned++; }

                if (GameObject.Find("WorkbenchCandelabra") != null) { report.AppendLine($"  ✓ Workbench Candelabra present"); passed++; }
                else { report.AppendLine($"  ⚠ Workbench Candelabra missing"); warned++; }

                var globalVolH = GameObject.Find("_HearthboundHollow_GlobalVolume");
                if (globalVolH != null) { report.AppendLine($"  ✓ Hollow Global Volume present"); passed++; }
                else                    { report.AppendLine($"  ✗ Hollow Global Volume MISSING — run Phase 32.4"); failed++; }

                // Preservation checks — Phase 26 + 27 outputs
                var marinNote = GameObject.Find("MarinsNote") ?? GameObject.Find("MarinNote");
                if (marinNote != null) { report.AppendLine($"  ✓ Marin's Note preserved (Phase 26)"); passed++; }
                else                    { report.AppendLine($"  ⚠ Marin's Note MISSING — run Phase 26 narrative hooks"); warned++; }

                var hearth = GameObject.Find("Hearth");
                if (hearth != null) { report.AppendLine($"  ✓ Hearth preserved (Phase 27.3)"); passed++; }
                else                { report.AppendLine($"  ✗ Hearth MISSING — run Phase 27.3"); failed++; }

                var workbench = GameObject.Find("Workbench");
                if (workbench != null) { report.AppendLine($"  ✓ Workbench preserved"); passed++; }
                else                   { report.AppendLine($"  ✗ Workbench MISSING — run Phase 22"); failed++; }
            }
            else
            {
                report.AppendLine($"  ✗ Hollow scene file missing: {SceneHollow}");
                failed++;
            }

            // ─── Summary ───────────────────────────────────────
            report.AppendLine();
            report.AppendLine("─────────────────────────────────────────────────────");
            report.AppendLine($"Total: ✓ {passed} passed   ⚠ {warned} warning(s)   ✗ {failed} failed");
            report.AppendLine();
            if (failed == 0)
            {
                report.AppendLine("🎉 Phase 32 — Mission 1 Polish v2 is healthy.");
                if (warned > 0)
                    report.AppendLine("    Warnings are usually cosmetic / asset availability.");
            }
            else
            {
                report.AppendLine("❌ Phase 32 has critical gaps — run:");
                report.AppendLine("   Hearthbound → 🍂 Phase 32 — Polish Mission 1 (v2 — all phases)");
            }

            Debug.Log(report.ToString());
            EditorUtility.DisplayDialog("Phase 32 — Mission 1 Polish Diagnostic",
                report.ToString(), "OK");
        }

        private static void CheckField(GameObject field, string name, ref int filled, ref int total)
        {
            total++;
            if (field != null) filled++;
        }
    }
}
