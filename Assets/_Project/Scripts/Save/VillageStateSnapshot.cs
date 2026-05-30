// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Save / VillageStateSnapshot
//
// JSON-serializable mirror of VillageState. Used by SaveService for
// atomic write/read.

using System;
using System.Collections.Generic;
using HearthboundHollow.Core;

namespace HearthboundHollow.Save
{
    [Serializable]
    public class VillageStateSnapshot
    {
        // Phase 43 bumps schema to 2 — audio resume fields added. Old saves
        // (schemaVersion 1) are forward-compatible: the audio fields default
        // to empty/empty/empty-list and the next scene's SceneAudioBeacon
        // restores cues normally.
        //
        // Phase 62 bumps schema to 3 — the engagement-loop state (resolved
        // requests, purchased upgrades, materials, completed echoes, the
        // Keeper's Hand tally, garden beds) so the cozy daily loop actually
        // COMPOUNDS across save/load. Old saves (v1/v2) are forward-compatible:
        // every new field defaults to empty/0 and the loop simply starts fresh.
        public int schemaVersion = 3;
        public string savedAtIso;
        public string lastSceneName;

        // M1-2 ACTIVE
        public int trustDoris;
        public int trustGerrold;
        public int memoryIntegrityGerrold;
        public int memoryIntegrityDoris;
        public int vow1Integrity;
        public int vow3Integrity;
        public int vow7Integrity;

        // Dormant
        public int trustMayor;
        public int trustInnkeeper;
        public int trustChild;
        public int villageGriefAverage;
        public int hollowReputation;
        public int predecessorTrailWarmth;
        public int memoryStockMarket;
        public int vanceArcProgress;
        public float seasonalDrift;
        public int vow2Integrity, vow4Integrity, vow5Integrity, vow6Integrity;

        // Player progression
        public int currentDayIndex;
        public int coin;
        public bool tutorialCompleted;
        public bool toneCompassAcknowledged;
        public bool gentleModeEnabled;

        // Lists
        public List<string> completedMissionIds = new();
        public List<string> revealedEchoConnectionIds = new();
        public List<string> heldMemoryIds = new();
        public List<string> harvestedHerbIds = new();
        public List<string> readMarinNoteIds = new();

        // ───── Audio state (Phase 43, schema v2) ─────────────────
        public string lastMusicId = "";
        public string lastAmbienceId = "";
        public List<string> playedDreamVariants = new();

        // ───── Engagement loop state (Phase 62, schema v3) ───────────
        public List<string> resolvedRequestIds = new();
        public List<string> purchasedUpgradeIds = new();
        public List<string> materials = new();
        public List<string> completedEchoIds = new();
        public int keeperHandCraftCount = 0;
        public List<GardenBedState> gardenBeds = new();

        public static VillageStateSnapshot FromState(VillageState s)
        {
            return new VillageStateSnapshot
            {
                schemaVersion = 3,
                savedAtIso = DateTime.UtcNow.ToString("o"),
                lastSceneName = s.lastSceneName,
                trustDoris = s.trustDoris,
                trustGerrold = s.trustGerrold,
                memoryIntegrityGerrold = s.memoryIntegrityGerrold,
                memoryIntegrityDoris = s.memoryIntegrityDoris,
                vow1Integrity = s.vow1Integrity,
                vow3Integrity = s.vow3Integrity,
                vow7Integrity = s.vow7Integrity,
                trustMayor = s.trustMayor,
                trustInnkeeper = s.trustInnkeeper,
                trustChild = s.trustChild,
                villageGriefAverage = s.villageGriefAverage,
                hollowReputation = s.hollowReputation,
                predecessorTrailWarmth = s.predecessorTrailWarmth,
                memoryStockMarket = s.memoryStockMarket,
                vanceArcProgress = s.vanceArcProgress,
                seasonalDrift = s.seasonalDrift,
                vow2Integrity = s.vow2Integrity,
                vow4Integrity = s.vow4Integrity,
                vow5Integrity = s.vow5Integrity,
                vow6Integrity = s.vow6Integrity,
                currentDayIndex = s.currentDayIndex,
                coin = s.coin,
                tutorialCompleted = s.tutorialCompleted,
                toneCompassAcknowledged = s.toneCompassAcknowledged,
                gentleModeEnabled = s.gentleModeEnabled,
                completedMissionIds = new List<string>(s.completedMissionIds),
                revealedEchoConnectionIds = new List<string>(s.revealedEchoConnectionIds),
                heldMemoryIds = new List<string>(s.heldMemoryIds),
                harvestedHerbIds = new List<string>(s.harvestedHerbIds),
                readMarinNoteIds = new List<string>(s.readMarinNoteIds),

                // Phase 43 — audio resume
                lastMusicId = s.lastMusicId ?? string.Empty,
                lastAmbienceId = s.lastAmbienceId ?? string.Empty,
                playedDreamVariants = s.playedDreamVariants != null
                    ? new List<string>(s.playedDreamVariants)
                    : new List<string>(),

                // Phase 62 — engagement loop state (schema v3)
                resolvedRequestIds = s.resolvedRequestIds != null ? new List<string>(s.resolvedRequestIds) : new List<string>(),
                purchasedUpgradeIds = s.purchasedUpgradeIds != null ? new List<string>(s.purchasedUpgradeIds) : new List<string>(),
                materials = s.materials != null ? new List<string>(s.materials) : new List<string>(),
                completedEchoIds = s.completedEchoIds != null ? new List<string>(s.completedEchoIds) : new List<string>(),
                keeperHandCraftCount = s.keeperHandCraftCount,
                gardenBeds = CloneBeds(s.gardenBeds),
            };
        }

        private static List<GardenBedState> CloneBeds(List<GardenBedState> src)
        {
            var list = new List<GardenBedState>();
            if (src == null) return list;
            foreach (var b in src)
            {
                if (b == null) continue;
                list.Add(new GardenBedState
                {
                    bedId = b.bedId,
                    plantedHerbId = b.plantedHerbId,
                    dayPlanted = b.dayPlanted,
                    watered = b.watered,
                });
            }
            return list;
        }

        public void ApplyTo(VillageState s)
        {
            s.lastSceneName = lastSceneName;
            s.trustDoris = trustDoris;
            s.trustGerrold = trustGerrold;
            s.memoryIntegrityGerrold = memoryIntegrityGerrold;
            s.memoryIntegrityDoris = memoryIntegrityDoris;
            s.vow1Integrity = vow1Integrity;
            s.vow3Integrity = vow3Integrity;
            s.vow7Integrity = vow7Integrity;
            s.trustMayor = trustMayor;
            s.trustInnkeeper = trustInnkeeper;
            s.trustChild = trustChild;
            s.villageGriefAverage = villageGriefAverage;
            s.hollowReputation = hollowReputation;
            s.predecessorTrailWarmth = predecessorTrailWarmth;
            s.memoryStockMarket = memoryStockMarket;
            s.vanceArcProgress = vanceArcProgress;
            s.seasonalDrift = seasonalDrift;
            s.vow2Integrity = vow2Integrity;
            s.vow4Integrity = vow4Integrity;
            s.vow5Integrity = vow5Integrity;
            s.vow6Integrity = vow6Integrity;
            s.currentDayIndex = currentDayIndex;
            s.coin = coin;
            s.tutorialCompleted = tutorialCompleted;
            s.toneCompassAcknowledged = toneCompassAcknowledged;
            s.gentleModeEnabled = gentleModeEnabled;
            s.completedMissionIds = new List<string>(completedMissionIds);
            s.revealedEchoConnectionIds = new List<string>(revealedEchoConnectionIds);
            s.heldMemoryIds = new List<string>(heldMemoryIds);
            s.harvestedHerbIds = new List<string>(harvestedHerbIds);
            s.readMarinNoteIds = new List<string>(readMarinNoteIds);

            // Phase 43 — audio resume (forward-compatible: schema v1 saves
            // have these fields default-initialized so ApplyTo just clears
            // them, and the next scene's SceneAudioBeacon overrides).
            s.lastMusicId = lastMusicId ?? string.Empty;
            s.lastAmbienceId = lastAmbienceId ?? string.Empty;
            s.playedDreamVariants = playedDreamVariants != null
                ? new List<string>(playedDreamVariants)
                : new List<string>();

            // Phase 62 — engagement loop state (forward-compatible: v1/v2 saves
            // default these to empty/0, so the loop simply starts fresh).
            s.resolvedRequestIds = resolvedRequestIds != null ? new List<string>(resolvedRequestIds) : new List<string>();
            s.purchasedUpgradeIds = purchasedUpgradeIds != null ? new List<string>(purchasedUpgradeIds) : new List<string>();
            s.materials = materials != null ? new List<string>(materials) : new List<string>();
            s.completedEchoIds = completedEchoIds != null ? new List<string>(completedEchoIds) : new List<string>();
            s.keeperHandCraftCount = keeperHandCraftCount;
            s.gardenBeds = CloneBeds(gardenBeds);
        }
    }
}
