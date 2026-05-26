// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Core / MissionSO
//
// Mission definitions for the 10-mission scalable pipeline. M1-2 ships two
// concrete instances; the schema supports Mission 3-10 via the same asset.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace HearthboundHollow.Core
{
    [CreateAssetMenu(menuName = "Hearthbound/Mission/Mission", fileName = "Mission")]
    public class MissionSO : ScriptableObject
    {
        [Header("Identity")]
        public string missionId = "Mission01";
        public string displayName = "Opening the Hollow";
        public int missionIndex = 1;

        [Header("Pacing")]
        [Tooltip("Estimated player time in minutes for the median cozy player.")]
        public int estimatedMinutes = 30;

        [Header("Tone")]
        public string toneOneLine = "Warm, slightly dusty, late afternoon light.";

        [Header("Scenes (additively loaded)")]
        public string entryScene;
        public List<string> scenesInMission = new();

        [Header("Objectives")]
        public List<MissionObjective> objectives = new();

        [Header("Outro")]
        [TextArea(2, 4)] public string outroLedgerProse;

        [Header("Cutscenes")]
        public string openingTimelineId;
        public string outroTimelineId;

        [Header("Music")]
        public string mainStemId;            // composer cue identifier

        [Header("Required Comfort Tools state")]
        public bool requiresToneCompassPrior = false;

        public bool HasObjective(string id)
        {
            foreach (var o in objectives) if (o.objectiveId == id) return true;
            return false;
        }
    }

    [Serializable]
    public struct MissionObjective
    {
        public string objectiveId;
        public string displayLabel;
        [TextArea(1, 3)] public string description;
        public bool isOptional;
    }
}
