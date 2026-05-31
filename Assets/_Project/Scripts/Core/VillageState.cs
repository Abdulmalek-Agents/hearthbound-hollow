// =============================================================================
// VillageState.cs — Hearthbound Hollow
// The single source of truth for all persistent game state across 30 missions.
// Serialized to JSON by SaveService. Schema v4 (Phase 76 — 30-level expansion).
// =============================================================================
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HearthboundHollow.Core
{
    /// <summary>
    /// All mutable game state. One instance lives on the GameManager (DontDestroyOnLoad).
    /// Everything here must be JSON-serializable (no UnityEngine.Object references).
    /// </summary>
    [Serializable]
    public class VillageState
    {
        // ─── Save metadata ───────────────────────────────────────────────────
        public int    saveSchemaVersion  = 4;
        public string saveSlotId         = "";
        public long   lastSavedTimestamp = 0;

        // ─── Progression ─────────────────────────────────────────────────────
        public int    currentMissionIndex = 0;   // 0-based; 0 = M01
        public int    currentDayIndex     = 0;   // 0-based within this mission
        public int    currentAct          = 1;   // 1, 2, or 3
        public Season currentSeason       = Season.Autumn;

        // ─── Economy ─────────────────────────────────────────────────────────
        public int coin = 0;

        // ─── Memory shelf ────────────────────────────────────────────────────
        public List<string> heldMemoryIds         = new(); // orb ids currently on shelves
        public List<string> releasedMemoryIds      = new(); // sold / erased / drifted

        // ─── Hollow upgrades ─────────────────────────────────────────────────
        public List<string> purchasedUpgradeIds    = new();

        // ─── Garden ──────────────────────────────────────────────────────────
        public List<GardenBedState> gardenBeds     = new();
        public List<string>         inventoryTeas  = new(); // brewed teas not yet consumed

        // ─── Request board ───────────────────────────────────────────────────
        public List<string> resolvedRequestIds     = new();
        public long         villageSeed            = 0;    // per-save RNG seed

        // ─── Echo Web / Memory Wall ──────────────────────────────────────────
        public List<string> completedEchoIds       = new(); // thread ids
        public int          memoryWebConnectionsFound = 0;

        // ─── Predecessor / Marin trail ───────────────────────────────────────
        public int          predecessorTrailWarmth = 0;   // 0-100
        public bool         readingNookVisited      = false;
        public List<string> letterFragmentsRead     = new();
        public bool         echoHologramHeard       = false;
        public int          echoHologramsFound      = 0;

        // ─── Sealed memory (9 fragments across Act 2-3) ──────────────────────
        public List<string>  sealedFragmentsFound   = new(); // fragment ids collected
        public bool          sealedMemoryAssembled  = false;
        public string        sealedMemoryChoiceId   = "";    // chosen in M29

        // ─── Per-mission flags (hand-authored, not generated) ─────────────────
        public bool  refusedDorisOrb          = false; // M01
        public bool  eraseGerroldPath         = false; // M02 — Erase chosen
        public bool  cleanseGerroldPerfect    = false; // M02 — Cleanse perfect
        public string gerroldMoralChoiceId    = "";    // M02 outcome
        public bool  millerGuiltErased        = false; // M03
        public bool  veraSecretKept           = false; // M04
        public bool  aldineRegretCleansed     = false; // M05
        public bool  cranesPrideListened      = false; // M06
        public bool  firstRevelationSeen      = false; // M07
        public bool  marketDayAttended        = false; // M08
        public bool  claraFriendshipRestored  = false; // M09
        public bool  firstFrostSurvived       = false; // M10

        // Act 2 fragment-holder flags.
        public bool  edmundBargainResolved    = false; // M11 — fragment 1
        public bool  ruthThreadResolved       = false; // M12 — fragment 2
        public bool  finnFlockResolved        = false; // M13 — fragment 3
        public bool  nellGameResolved         = false; // M14 — fragment 4
        public bool  lockedRoomResolved       = false; // M15 — fragment 5
        public bool  augustClocksResolved     = false; // M16
        public bool  festivalOfMemoryPlayed   = false; // M17
        public bool  irisGriefResolved        = false; // M18
        public bool  oldRivalryResolved       = false; // M19
        public bool  echoWebMajorReveal       = false; // M20

        // Act 3 flags.
        public bool  mayorConfessionHeard     = false; // M21 — fragment 6
        public bool  veraReturnedM22          = false; // M22
        public bool  marinFinalLetterRead     = false; // M23
        public bool  missingFragmentFound     = false; // M24 — fragment 7
        public bool  hearingHeld              = false; // M25
        public bool  dreamOfAllDreamsDreamed  = false; // M26
        public bool  augustFinalGiftGiven     = false; // M27 — fragment 8
        public bool  allFragmentsAssembled    = false; // M28
        public bool  sealedChoiceMade         = false; // M29
        public bool  epilogueCompleted        = false; // M30

        // ─── Depth Layer ─────────────────────────────────────────────────────
        public bool  seenColdOpen          = false;
        public bool  prefaceBeatPlayed     = false;
        public string prefaceToneBucket    = "";
        public bool  coldOpenLastVariant   = false;
        public int   memoryWebConFound     = 0;

        // ─── Accessibility / settings ─────────────────────────────────────────
        public bool   gentleModeEnabled    = false;
        public string toneCompassChoice    = "Standard"; // "Gentle" | "Standard" | "Deep"
        public string languageCode         = "en";      // "en" | "ar"

        // ─── Character appearance ────────────────────────────────────────────
        public string playerSkinTone       = "mid";
        public string playerOutfitColor    = "cream";
        public string playerAccessory      = "none";
        public string playerName           = "";
        public bool   characterCreated     = false;

        // ─── Audio state (save-resume, Phase 43 pattern) ─────────────────────
        public string lastMusicId          = "";
        public string lastAmbienceId       = "";
        public List<string> playedDreamVariants = new();

        // ─── Villager trust (per id, 0-100) ──────────────────────────────────
        public Dictionary<string, int> villagerTrust = new();

        // ─── Methods ─────────────────────────────────────────────────────────

        public void ResetToDefault()
        {
            saveSchemaVersion  = 4;
            saveSlotId         = System.Guid.NewGuid().ToString("N");
            lastSavedTimestamp = 0;
            currentMissionIndex = 0;
            currentDayIndex     = 0;
            currentAct          = 1;
            currentSeason       = Season.Autumn;
            coin               = 0;
            heldMemoryIds      = new();
            releasedMemoryIds  = new();
            purchasedUpgradeIds = new();
            gardenBeds         = new();
            inventoryTeas      = new();
            resolvedRequestIds = new();
            villageSeed        = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            completedEchoIds   = new();
            memoryWebConnectionsFound = 0;
            predecessorTrailWarmth = 0;
            readingNookVisited  = false;
            letterFragmentsRead = new();
            echoHologramHeard   = false;
            echoHologramsFound  = 0;
            sealedFragmentsFound = new();
            sealedMemoryAssembled = false;
            sealedMemoryChoiceId = "";
            // Reset all per-mission flags.
            refusedDorisOrb = false;
            eraseGerroldPath = cleanseGerroldPerfect = false;
            gerroldMoralChoiceId = "";
            millerGuiltErased = veraSecretKept = aldineRegretCleansed = false;
            cranesPrideListened = firstRevelationSeen = marketDayAttended = false;
            claraFriendshipRestored = firstFrostSurvived = false;
            edmundBargainResolved = ruthThreadResolved = finnFlockResolved = false;
            nellGameResolved = lockedRoomResolved = augustClocksResolved = false;
            festivalOfMemoryPlayed = irisGriefResolved = oldRivalryResolved = false;
            echoWebMajorReveal = false;
            mayorConfessionHeard = veraReturnedM22 = marinFinalLetterRead = false;
            missingFragmentFound = hearingHeld = dreamOfAllDreamsDreamed = false;
            augustFinalGiftGiven = allFragmentsAssembled = sealedChoiceMade = false;
            epilogueCompleted = false;
            seenColdOpen = prefaceBeatPlayed = false;
            prefaceToneBucket = ""; coldOpenLastVariant = false;
            memoryWebConFound = 0;
            gentleModeEnabled  = false;
            toneCompassChoice  = "Standard";
            languageCode       = "en";
            playerSkinTone     = "mid";
            playerOutfitColor  = "cream";
            playerAccessory    = "none";
            playerName         = "";
            characterCreated   = false;
            lastMusicId = lastAmbienceId = "";
            playedDreamVariants = new();
            villagerTrust      = new();
        }

        public int GetTrust(string villagerId)
        {
            villagerTrust.TryGetValue(villagerId, out int t);
            return t;
        }

        public void AddTrust(string villagerId, int delta)
        {
            villagerTrust.TryGetValue(villagerId, out int t);
            villagerTrust[villagerId] = Mathf.Clamp(t + delta, 0, 100);
        }
    }

    [Serializable]
    public class GardenBedState
    {
        public int    bedIndex;
        public string herbId       = "";
        public int    daysPlanted  = 0;
        public int    daysToRipen  = 3;
        public bool   isRipe       = false;
        public bool   isEmpty      => string.IsNullOrEmpty(herbId);
    }
}
