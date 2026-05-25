// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Core / VillageState
//
// The full 14-dimension state struct from Codex 08 § 2.
// In Mission 1-2 only 4 dimensions are written to. The other 10 fields sit at
// default values per the Krieg Discipline (Focus 00 § 5) — the schema is the
// architectural contract that prevents rework when we scale to Mission 3+.

using System.Collections.Generic;
using UnityEngine;

namespace HearthboundHollow.Core
{
    [CreateAssetMenu(menuName = "Hearthbound/State/Village State", fileName = "VillageState")]
    public class VillageState : ScriptableObject
    {
        // ───── M1-2 ACTIVE dimensions ───────────────────────────────────

        [Header("M1-2 ACTIVE — Trust (0–100)")]
        [Range(0, 100)] public int trustDoris = 50;
        [Range(0, 100)] public int trustGerrold = 50;

        [Header("M1-2 ACTIVE — Memory Integrity (0–100)")]
        [Range(0, 100)] public int memoryIntegrityGerrold = 100;
        [Range(0, 100)] public int memoryIntegrityDoris = 100;

        [Header("M1-2 ACTIVE — Vow Integrity (0–100)")]
        [Range(0, 100)] public int vow1Integrity = 50;   // Honor the named
        [Range(0, 100)] public int vow3Integrity = 50;   // Refuse no one's grief
        [Range(0, 100)] public int vow7Integrity = 50;   // Keep the Hollow lit

        // ───── M3+ DORMANT dimensions (Krieg architectural seam) ────────

        [Header("M3+ DORMANT — Trust")]
        [Range(0, 100)] public int trustMayor = 50;
        [Range(0, 100)] public int trustInnkeeper = 50;
        [Range(0, 100)] public int trustChild = 50;

        [Header("M3+ DORMANT — Aggregate village dimensions")]
        [Range(0, 100)] public int villageGriefAverage = 50;
        [Range(0, 100)] public int hollowReputation = 50;
        [Range(0, 100)] public int predecessorTrailWarmth = 0;
        [Range(0, 100)] public int memoryStockMarket = 0;
        [Range(0, 100)] public int vanceArcProgress = 0;
        [Range(-1f, 1f)] public float seasonalDrift = 0f;

        [Header("M3+ DORMANT — Vow Integrity (vows 2,4,5,6)")]
        [Range(0, 100)] public int vow2Integrity = 50;
        [Range(0, 100)] public int vow4Integrity = 50;
        [Range(0, 100)] public int vow5Integrity = 50;
        [Range(0, 100)] public int vow6Integrity = 50;

        // ───── Player progression ───────────────────────────────────────

        [Header("Player progression")]
        public int currentDayIndex = 0;
        public int coin = 50;
        public bool tutorialCompleted = false;
        public bool toneCompassAcknowledged = false;
        public bool gentleModeEnabled = false;
        [Tooltip("Phase 30 — set true after the player completes (or skips) the " +
                 "multi-step OnboardingOverlay so it never re-appears on this save.")]
        public bool onboardingCompleted = false;
        public string lastSceneName;

        [Header("Mission flags")]
        public List<string> completedMissionIds = new();
        public List<string> revealedEchoConnectionIds = new();
        public List<string> heldMemoryIds = new();
        public List<string> harvestedHerbIds = new();
        public List<string> readMarinNoteIds = new();

        // ───── Operations ────────────────────────────────────────────────

        /// <summary>Reset every field to a fresh-play default.</summary>
        public void ResetToDefault()
        {
            trustDoris = 50; trustGerrold = 50;
            memoryIntegrityGerrold = 100; memoryIntegrityDoris = 100;
            vow1Integrity = vow3Integrity = vow7Integrity = 50;
            vow2Integrity = vow4Integrity = vow5Integrity = vow6Integrity = 50;
            trustMayor = trustInnkeeper = trustChild = 50;
            villageGriefAverage = hollowReputation = 50;
            predecessorTrailWarmth = memoryStockMarket = vanceArcProgress = 0;
            seasonalDrift = 0f;
            currentDayIndex = 0;
            coin = 50;
            tutorialCompleted = false;
            toneCompassAcknowledged = false;
            gentleModeEnabled = false;
            onboardingCompleted = false;
            lastSceneName = string.Empty;
            completedMissionIds.Clear();
            revealedEchoConnectionIds.Clear();
            heldMemoryIds.Clear();
            harvestedHerbIds.Clear();
            readMarinNoteIds.Clear();
        }

        /// <summary>Clamp a trust/integrity delta safely.</summary>
        public static int Adjust(int current, int delta) => Mathf.Clamp(current + delta, 0, 100);

        public void OnEnable()
        {
            completedMissionIds ??= new List<string>();
            revealedEchoConnectionIds ??= new List<string>();
            heldMemoryIds ??= new List<string>();
            harvestedHerbIds ??= new List<string>();
            readMarinNoteIds ??= new List<string>();
        }
    }
}
