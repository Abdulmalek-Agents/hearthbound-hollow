// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Core / VillageState
//
// The full 14-dimension state struct from Codex 08 § 2.
// In Mission 1-2 only 4 dimensions are written to. The other 10 fields sit at
// default values per the Krieg Discipline (Focus 00 § 5) — the schema is the
// architectural contract that prevents rework when we scale to Mission 3+.
//
// ── Playtest pass fix (commit 1/6) ──────────────────────────────
// QA simulated-playthrough audit found the following fields referenced by
// the expanded Yarn files (Doris_M1, Gerrold_M2, Pickle, EveningLedger,
// Codex, ChoiceCards) had no corresponding VillageState fields, breaking
// the YarnVillageStateBridge bi-directional sync:
//   - pickleApproval (gates 5 of Pickle's conditional lines)
//   - cinder (Confession Booth currency; earnable only via Listen path)
//   - pickleSassIntensity (1-5 setting; 3 = default)
//   - firstMoralChoiceMade (Mission 3+ gate flag)
//   - dorisOwesPlayer (the underpay-path debt thread)
//   - sat_in_gerrold_chair / sat_in_margery_chair (chair-selection flags)
//   - gerroldReturnsDay3 (Defer-path Mission 3 hook)
//   - mission6RecoveryArcSeeded (Crossed-Core consequence)
//   - offeredGerroldTea, deferredGerrold (M2 dialogue gates)
//   - polishQuality, cleanseQuality, gerroldChoice (mini-game outcomes)
//   - teaBrewed (Lavender/Valerian/None modifier)
//
// Added all 14 fields below + cleared in ResetToDefault().
//
// ── Phase 43 (2026-05-26) ───────────────────────────────────────
// Added 3 audio-resume fields so saves restore the music + ambient cue
// the player was hearing when they last saved:
//   - lastMusicId (MusicLibrarySO id)
//   - lastAmbienceId (AmbienceLibrarySO id)
//   - playedDreamVariants (List<string> of Dream Timeline names already seen)

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

        [Header("M1-2 ACTIVE — Pickle (added in playtest pass commit 1/6)")]
        [Range(0, 100)]
        [Tooltip("Gates Pickle's 5 conditional pre-choice + chair lines at >=50. " +
                 "Below 50, the moral choice is made in silence (Mission 2 Guide § 14.2).")]
        public int pickleApproval = 50;
        [Range(1, 5)]
        [Tooltip("Pickle sass intensity. 1 = warm and gentle, 5 = full sarcasm. " +
                 "M1-2 only differentiates 1, 3, 5 (settings 2 and 4 collapse to nearest). " +
                 "Gentle Mode auto-routes to 1.")]
        public int pickleSassIntensity = 3;

        [Header("M1-2 ACTIVE — Confession Booth currency")]
        [Tooltip("Earned ONLY via the Mission 2 Listen path (+2 or +3 per Listen sub-option). " +
                 "Spent in M5+ at the Confession Booth.")]
        public int cinder = 0;

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

        // ───── M1-2 dialogue flags (added playtest pass commit 1/6) ─────

        [Header("M1 dialogue flags")]
        [Tooltip("True after player asked Doris 'Who was the old one?' in M1. " +
                 "Unlocks Marin's name reveal in Doris's M2 morning greeting.")]
        public bool askedAboutPredecessor = false;
        [Tooltip("True if player declined Doris's First Loaves orb in M1. " +
                 "Mission 1 takes the quiet alternate route per Guide § 9.4.")]
        public bool refusedDorisOrb = false;
        [Tooltip("Negative = player owes Doris this many coppers (Underpay path).")]
        public int dorisOwesPlayer = 0;
        [Tooltip("Mission 1 Polish result: 'Perfect' | 'Acceptable' | 'Mild'. " +
                 "Branches Doris's after-polish line.")]
        public string polishQuality = "";

        [Header("M2 dialogue flags")]
        public bool metDoris = false;
        public bool metGerrold = false;
        public bool offeredGerroldTea = false;
        [Tooltip("'Lavender' | 'Valerian' | '' (no tea). Modifies Gerrold's cottage dialogue + " +
                 "Cleanse mini-game difficulty per Focus 06 § 4.")]
        public string teaBrewed = "";
        public bool walkedToGerroldHouse = false;
        public bool workedAtHollow = false;
        public bool workedAlone = false;
        public bool satInGerroldChair = false;
        [Tooltip("Sitting in Margery's chair enables Pickle's M2_MargerysChair line " +
                 "if pickleApproval >= 50. One of M1-2's most affecting moments.")]
        public bool satInMargeryChair = false;
        public bool deferredGerrold = false;

        [Header("M2 moral-choice outcome")]
        [Tooltip("'erase' | 'cleanse' | 'listen' | 'defer'. Set after the moral-choice " +
                 "screen. Drives Memory Dream 2 variant + Day 2 Ledger prose.")]
        public string gerroldChoice = "";
        [Tooltip("'Perfect' | 'Acceptable' | 'Sloppy' | 'CrossedCore'. Cleanse mini-game outcome.")]
        public string cleanseQuality = "";
        [Tooltip("Locked true after Mission 2 choice is confirmed. Mission 3+ checks this.")]
        public bool firstMoralChoiceMade = false;
        [Tooltip("True if Defer path taken. Mission 3 will re-engage Gerrold.")]
        public bool gerroldReturnsDay3 = false;
        [Tooltip("Seeded by Erase Crossed-Core or Cleanse Crossed-Core. M6+ recovery arc unlock.")]
        public bool mission6RecoveryArcSeeded = false;

        // ───── Audio state (Phase 43) ───────────────────────────────────
        // Persisted so saves resume the exact music + ambient cue that was
        // playing when the player last saved. Per D-055 (Phase 43): audio
        // continuity is a save-restore obligation, not a scene-bootstrap
        // assumption.

        [Header("Audio state (Phase 43)")]
        [Tooltip("MusicLibrarySO id of the cue that was playing when last saved. " +
                 "Empty = let the next scene's SceneAudioBeacon decide.")]
        public string lastMusicId = "";
        [Tooltip("AmbienceLibrarySO id of the ambient bed when last saved.")]
        public string lastAmbienceId = "";
        [Tooltip("Memory Dream variant ids the player has already witnessed. " +
                 "Used to skip re-played dreams on load, and by the future " +
                 "Dream Cinema (Codex 11) replay menu.")]
        public List<string> playedDreamVariants = new();

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

            // Playtest pass commit 1/6 — clear newly added fields.
            pickleApproval = 50;
            pickleSassIntensity = 3;
            cinder = 0;
            askedAboutPredecessor = false;
            refusedDorisOrb = false;
            dorisOwesPlayer = 0;
            polishQuality = string.Empty;
            metDoris = false;
            metGerrold = false;
            offeredGerroldTea = false;
            teaBrewed = string.Empty;
            walkedToGerroldHouse = false;
            workedAtHollow = false;
            workedAlone = false;
            satInGerroldChair = false;
            satInMargeryChair = false;
            deferredGerrold = false;
            gerroldChoice = string.Empty;
            cleanseQuality = string.Empty;
            firstMoralChoiceMade = false;
            gerroldReturnsDay3 = false;
            mission6RecoveryArcSeeded = false;

            // Phase 43 — audio resume fields
            lastMusicId = string.Empty;
            lastAmbienceId = string.Empty;
            playedDreamVariants ??= new List<string>();
            playedDreamVariants.Clear();
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
            // Phase 43 — guard against null lists for legacy saves.
            playedDreamVariants ??= new List<string>();
        }
    }
}
