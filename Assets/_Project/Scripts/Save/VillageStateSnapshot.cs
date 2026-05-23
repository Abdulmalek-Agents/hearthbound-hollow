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
        public int schemaVersion = 1;
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

        public static VillageStateSnapshot FromState(VillageState s)
        {
            return new VillageStateSnapshot
            {
                schemaVersion = 1,
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
            };
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
        }
    }
}
