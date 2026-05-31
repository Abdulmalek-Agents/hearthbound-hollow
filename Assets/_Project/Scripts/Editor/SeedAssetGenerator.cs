// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / SeedAssetGenerator
//
// One-click menu item that creates every ScriptableObject seed asset
// needed for Mission 1-2 at the canonical paths. Saves ~30 minutes of
// right-click → Create → Hearthbound → ... per asset.
//
// USE: Menu → Hearthbound → Create Mission 1-2 Seed Assets
//
// Idempotent: if an asset already exists at the target path, the generator
// skips it (does NOT overwrite). Re-run safely any time.

using System.IO;
using UnityEditor;
using UnityEngine;
using HearthboundHollow.Core;
using HearthboundHollow.Memory;

namespace HearthboundHollow.EditorTools
{
    public static class SeedAssetGenerator
    {
        private const string SoRoot = "Assets/_Project/ScriptableObjects";

        [MenuItem("Hearthbound/⚙️ Advanced/Create Mission 1-2 Seed Assets", priority = 100)]
        public static void CreateAllSeedAssets()
        {
            EnsureFolders();

            // ───── VillageState ─────
            CreateIfMissing<VillageState>($"{SoRoot}/State/VillageState.asset", _ => { });

            // ───── Missions ─────
            CreateIfMissing<MissionSO>($"{SoRoot}/Missions/Mission01_OpeningTheHollow.asset", m =>
            {
                m.missionId = "Mission01";
                m.displayName = "Opening the Hollow";
                m.missionIndex = 1;
                m.estimatedMinutes = 30;
                m.toneOneLine = "Warm, slightly dusty, late afternoon light.";
                m.entryScene = "02_Mission01_Lane";
                m.scenesInMission = new System.Collections.Generic.List<string> { "02_Mission01_Lane", "03_Mission01_Hollow" };
                m.outroLedgerProse = "You have a memory on your shelf.\nDoris stands a little straighter now.\nThe Hollow is no longer dark.";
                m.openingTimelineId = "Opening_M1";
                m.outroTimelineId = "Outro_M1";
                m.mainStemId = "M-MainTheme";
            });

            CreateIfMissing<MissionSO>($"{SoRoot}/Missions/Mission02_TheWidowersRequest.asset", m =>
            {
                m.missionId = "Mission02";
                m.displayName = "The Widower's Request";
                m.missionIndex = 2;
                m.estimatedMinutes = 35;
                m.toneOneLine = "Quiet. A heavy beat. Candlelight, soft rain optional.";
                m.entryScene = "03_Mission01_Hollow";
                m.scenesInMission = new System.Collections.Generic.List<string> { "03_Mission01_Hollow", "04_Mission02_Garden", "05_Mission02_Cottage" };
                m.outroLedgerProse = "You made a choice today.\nThe village remembers.\nThe Hollow remembers, too.";
                m.openingTimelineId = "Opening_M2";
                m.outroTimelineId = "Outro_M2";
                m.mainStemId = "M-GerroldTheme";
                m.requiresToneCompassPrior = true;
            });

            // ───── Villagers ─────
            var doris = CreateIfMissing<VillagerSO>($"{SoRoot}/Villagers/Doris.asset", v =>
            {
                v.villagerId = "doris";
                v.displayName = "Doris";
                v.archetypeBoZo = "Cleric";
                v.motifCueId = "M-DOR-01";
                v.defaultYarnNode = "Doris_M1_Start";
                v.dialogueCps = 42;
                v.blinkPerMin = 14;
            });

            var gerrold = CreateIfMissing<VillagerSO>($"{SoRoot}/Villagers/Gerrold.asset", v =>
            {
                v.villagerId = "gerrold";
                v.displayName = "Gerrold";
                v.archetypeBoZo = "Bard";
                v.motifCueId = "M-GER-01";
                v.defaultYarnNode = "Gerrold_M2_Start";
                v.dialogueCps = 35;
                v.blinkPerMin = 6;
            });

            CreateIfMissing<VillagerSO>($"{SoRoot}/Villagers/SilentLane.asset", v =>
            {
                v.villagerId = "silentlane";
                v.displayName = "Lane villager";
                v.archetypeBoZo = "Warrior";
                v.dialogueCps = 45;
                v.blinkPerMin = 12;
            });

            // ───── Herbs ─────
            CreateIfMissing<MemoryHerb>($"{SoRoot}/Herbs/Lavender.asset", h =>
            {
                h.herbId = "lavender";
                h.displayName = "Lavender";
                h.effect = HerbEffect.OpenUp;
                h.trustDeltaOnTea = 5;
                h.effectDurationSeconds = 60f;
                h.growDays = 4;
                h.yieldPerHarvest = 1;
                h.brewFlavorText = "Floral, soft. The kitchen smells like sleep.";
            });

            CreateIfMissing<MemoryHerb>($"{SoRoot}/Herbs/Valerian.asset", h =>
            {
                h.herbId = "valerian";
                h.displayName = "Valerian";
                h.effect = HerbEffect.Calm;
                h.trustDeltaOnTea = 3;
                h.effectDurationSeconds = 90f;
                h.growDays = 6;
                h.yieldPerHarvest = 1;
                h.brewFlavorText = "Bitter, earthy. Settles the chest.";
            });

            // ───── Memories ─────
            var dor001 = CreateIfMissing<MemoryNodeSO>($"{SoRoot}/Memories/DOR-001_FirstLoaves.asset", m =>
            {
                m.id = "DOR-001";
                m.owner = doris;
                m.title = "First Loaves";
                m.primaryTone = EmotionalTone.Joy;
                m.secondaryTone = EmotionalTone.Grace;
                m.weight = 0.4f;
                m.initialClarity = 0.4f;
                m.crackIntensity = 0f;
                m.proseShort = "The morning Doris first baked alone, after her mother's hands stopped.";
                m.proseFull = "The kitchen at first light. Flour on the table. Her mother's apron on the hook by the door. " +
                              "Doris reaches for the dough and her hands remember everything they were taught. " +
                              "The loaves come out brown and warm. Doris cries a little, eats a piece while it's still too hot, " +
                              "and decides she will open the bakery on her own. The light through the kitchen window is gold.";
                m.polishCleanseCost = 5;
                m.erasureCost = 0;
                m.marinHandwrittenNote = "This one was always bright. Take care. — M.";
                m.dreamSequencerNodeId = "Dream1_Doris";
            });

            var ger007 = CreateIfMissing<MemoryNodeSO>($"{SoRoot}/Memories/GER-007_SeventhMorning.asset", m =>
            {
                m.id = "GER-007";
                m.owner = gerrold;
                m.title = "The Seventh Morning";
                m.primaryTone = EmotionalTone.Grief;
                m.secondaryTone = EmotionalTone.Longing;
                m.weight = 0.7f;
                m.initialClarity = 0.6f;
                m.crackIntensity = 0.6f;
                m.proseShort = "The morning Margery died. Bread on the counter. A photograph by the bed.";
                m.proseFull = "A Sunday. The light through the bedroom window is the same light that has been there for thirty-six years. " +
                              "Margery's breath is quiet, then quieter. Gerrold says her name once. The bread Doris brought yesterday " +
                              "is still on the counter. The wedding photograph by the bed catches the light. Gerrold sits in the chair beside her " +
                              "for a long time. He does not move when the light changes.";
                m.polishCleanseCost = 8;
                m.erasureCost = 0;
                m.marinHandwrittenNote = "Some memories should not be polished. Some should not be cleansed. — M.";
                m.dreamSequencerNodeId = "Dream2_Gerrold";
            });

            // ───── Echo Connection ─────
            CreateIfMissing<MemoryConnectionSO>($"{SoRoot}/Memories/ECHO_DOR001_GER007.asset", c =>
            {
                c.connectionId = "ECHO-DOR001-GER007";
                c.memoryA = dor001;
                c.memoryB = ger007;
                c.strength = 0.7f;
                c.revealConditionId = "gerrold_mentions_bread_on_counter";
                c.revealProse = "The bread on Gerrold's counter the morning his wife died — Doris brought it. " +
                                "Her First Loaves, twenty-three years later, was the last loaf Gerrold's wife ever saw.";
                c.yarnRevealNode = "Echo_DOR001_GER007_Reveal";
            });

            // ───── Memory Maps ─────
            CreateIfMissing<VillagerMemoryMapSO>($"{SoRoot}/Villagers/MemoryMap_Doris.asset", mm =>
            {
                mm.villager = doris;
                mm.nodes = new System.Collections.Generic.List<MemoryMapNode>
                {
                    new() { memory = dor001, graphPosition = new Vector2(0,   0),  revealedAtStart = true,  revealConditionId = "" },
                    new() { memory = null,   graphPosition = new Vector2(120, 60), revealedAtStart = false, revealConditionId = "doris_trust_60" },
                    new() { memory = null,   graphPosition = new Vector2(-120,60), revealedAtStart = false, revealConditionId = "doris_mother_revealed" },
                    new() { memory = null,   graphPosition = new Vector2(0,  120), revealedAtStart = false, revealConditionId = "doris_apprentice" },
                };
            });

            CreateIfMissing<VillagerMemoryMapSO>($"{SoRoot}/Villagers/MemoryMap_Gerrold.asset", mm =>
            {
                mm.villager = gerrold;
                mm.nodes = new System.Collections.Generic.List<MemoryMapNode>
                {
                    new() { memory = ger007, graphPosition = new Vector2(0,   0),  revealedAtStart = true, revealConditionId = "" },
                    new() { memory = null,   graphPosition = new Vector2(120, 60), revealedAtStart = false, revealConditionId = "gerrold_wedding_day" },
                };
            });

            // ───── Tariffs ─────
            CreateIfMissing<TariffSO>($"{SoRoot}/Tariffs/Tariff_Erase.asset", t =>
            {
                t.choice = MoralChoice.Erase;
                t.displayLabel = "Erase the memory";
                t.costPreviewProse = "He will not remember this morning.\nHe will not remember her face here.";
                t.coinDelta = 0;
                t.trustDeltaTarget = -5;
                t.trustRippleNeighbors = -3;
                t.memoryIntegrityDelta = -80;
                t.vow1Delta = -10;
                t.vow3Delta = -3;
                t.vow7Delta = 0;
                t.villageGriefDelta = -2;
                t.choiceColor = new Color(0.78f, 0.42f, 0.42f);
            });

            CreateIfMissing<TariffSO>($"{SoRoot}/Tariffs/Tariff_Cleanse.asset", t =>
            {
                t.choice = MoralChoice.Cleanse;
                t.displayLabel = "Cleanse — soften, don't take";
                t.costPreviewProse = "The morning stays. The weight lifts a little.\nHe may speak of her tomorrow.";
                t.coinDelta = -8;
                t.trustDeltaTarget = 10;
                t.trustRippleNeighbors = 4;
                t.memoryIntegrityDelta = -5;
                t.vow1Delta = 5;
                t.vow3Delta = 3;
                t.vow7Delta = 2;
                t.villageGriefDelta = -3;
                t.choiceColor = new Color(0.96f, 0.82f, 0.55f);
            });

            CreateIfMissing<TariffSO>($"{SoRoot}/Tariffs/Tariff_Listen.asset", t =>
            {
                t.choice = MoralChoice.Listen;
                t.displayLabel = "Refuse — sit with him instead";
                t.costPreviewProse = "You take nothing.\nYou stay until he is ready to sleep.";
                t.coinDelta = 0;
                t.trustDeltaTarget = 15;
                t.trustRippleNeighbors = 6;
                t.memoryIntegrityDelta = 0;
                t.vow1Delta = 0;
                t.vow3Delta = 10;
                t.vow7Delta = 5;
                t.villageGriefDelta = 1;
                t.choiceColor = new Color(0.62f, 0.82f, 0.92f);
            });

            CreateIfMissing<TariffSO>($"{SoRoot}/Tariffs/Tariff_Defer.asset", t =>
            {
                t.choice = MoralChoice.Defer;
                t.displayLabel = "Take it — but do not act tonight";
                t.costPreviewProse = "The orb will rest on the counter.\nYou will decide in the morning.";
                t.coinDelta = 0;
                t.trustDeltaTarget = 2;
                t.trustRippleNeighbors = 0;
                t.memoryIntegrityDelta = 0;
                t.vow1Delta = 0;
                t.vow3Delta = 0;
                t.vow7Delta = 0;
                t.villageGriefDelta = 0;
                t.choiceColor = new Color(0.7f, 0.7f, 0.7f);
            });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Hearthbound] Seed assets generated. Check Project window under Assets/_Project/ScriptableObjects/.");
        }

        [MenuItem("Hearthbound/⚙️ Advanced/Validate Mission 1-2 Seed Assets", priority = 101)]
        public static void ValidateSeedAssets()
        {
            string[] required = new[]
            {
                $"{SoRoot}/State/VillageState.asset",
                $"{SoRoot}/Missions/Mission01_OpeningTheHollow.asset",
                $"{SoRoot}/Missions/Mission02_TheWidowersRequest.asset",
                $"{SoRoot}/Villagers/Doris.asset",
                $"{SoRoot}/Villagers/Gerrold.asset",
                $"{SoRoot}/Villagers/SilentLane.asset",
                $"{SoRoot}/Villagers/MemoryMap_Doris.asset",
                $"{SoRoot}/Villagers/MemoryMap_Gerrold.asset",
                $"{SoRoot}/Herbs/Lavender.asset",
                $"{SoRoot}/Herbs/Valerian.asset",
                $"{SoRoot}/Memories/DOR-001_FirstLoaves.asset",
                $"{SoRoot}/Memories/GER-007_SeventhMorning.asset",
                $"{SoRoot}/Memories/ECHO_DOR001_GER007.asset",
                $"{SoRoot}/Tariffs/Tariff_Erase.asset",
                $"{SoRoot}/Tariffs/Tariff_Cleanse.asset",
                $"{SoRoot}/Tariffs/Tariff_Listen.asset",
                $"{SoRoot}/Tariffs/Tariff_Defer.asset",
            };

            int present = 0, missing = 0;
            foreach (var p in required)
            {
                if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(p) != null) present++;
                else { missing++; Debug.LogWarning($"[Hearthbound] Missing seed asset: {p}"); }
            }
            Debug.Log($"[Hearthbound] Seed asset validation: {present}/{required.Length} present, {missing} missing.");
        }

        // ─── Helpers ────────────────────────────────────────────

        private static T CreateIfMissing<T>(string path, System.Action<T> populate) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
            {
                Debug.Log($"[Hearthbound] (skip) {path} already exists.");
                return existing;
            }
            var so = ScriptableObject.CreateInstance<T>();
            populate?.Invoke(so);
            EnsureDirFor(path);
            AssetDatabase.CreateAsset(so, path);
            Debug.Log($"[Hearthbound] (created) {path}");
            return so;
        }

        private static void EnsureFolders()
        {
            EnsureDir($"{SoRoot}");
            EnsureDir($"{SoRoot}/State");
            EnsureDir($"{SoRoot}/Missions");
            EnsureDir($"{SoRoot}/Villagers");
            EnsureDir($"{SoRoot}/Herbs");
            EnsureDir($"{SoRoot}/Memories");
            EnsureDir($"{SoRoot}/Tariffs");
        }

        private static void EnsureDir(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            var leaf = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureDir(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        private static void EnsureDirFor(string filePath)
        {
            var dir = Path.GetDirectoryName(filePath).Replace('\\', '/');
            EnsureDir(dir);
        }
    }
}
