// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / MissionRunner
//
// Per-mission orchestration MonoBehaviour. Attached to the mission's
// gameplay scene root. Subscribes to events (PolishCompleted, CleanseCompleted,
// MoralChoiceMade) and drives mission objectives + transitions.

using UnityEngine;
using HearthboundHollow.Core;
using HearthboundHollow.Memory;
using HearthboundHollow.UI;

namespace HearthboundHollow.Mission
{
    public class MissionRunner : MonoBehaviour
    {
        [Header("Mission")]
        public MissionSO mission;

        [Header("References")]
        public EveningLedgerUI eveningLedger;
        public ChoiceCardUI choiceCard;
        public HearthboundHollow.Save.SaveService saveService;
        public RippleEngine rippleEngine;

        [Header("Routing")]
        public string nextMissionScene;

        [Header("Phase 47 — One More Day (optional)")]
        [Tooltip("When wired, the Goodnight card shows after the Evening " +
                 "Ledger confirm. MissionRunner has no dream rig, so no dream " +
                 "plays — just the card. Unwired = legacy path (D-064).")]
        public EndOfDaySequencer endOfDaySequencer;

        private void Awake()
        {
            saveService ??= new HearthboundHollow.Save.SaveService();
            rippleEngine ??= new RippleEngine();
            ServiceLocator.Register(saveService);
            ServiceLocator.Register(rippleEngine);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<MissionCompletedEvent>(OnMissionCompleted);
            EventBus.Subscribe<MoralChoiceMadeEvent>(OnMoralChoiceMade);
            if (eveningLedger != null) eveningLedger.OnSaveSlotChosen += OnSlotChosen;
            if (eveningLedger != null) eveningLedger.OnEndOfDayConfirmed += OnEndOfDayConfirmed;
            if (mission != null)
            {
                Hh.Log(LogCategory.Mission, $"Mission started: {mission.missionId} — '{mission.displayName}'");
                EventBus.Publish(new MissionStartedEvent(mission, mission.missionId));
            }
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<MissionCompletedEvent>(OnMissionCompleted);
            EventBus.Unsubscribe<MoralChoiceMadeEvent>(OnMoralChoiceMade);
            if (eveningLedger != null) eveningLedger.OnSaveSlotChosen -= OnSlotChosen;
            if (eveningLedger != null) eveningLedger.OnEndOfDayConfirmed -= OnEndOfDayConfirmed;
        }

        public void OfferMoralChoice(MemoryNodeSO memory, string promptText, System.Collections.Generic.IReadOnlyList<TariffSO> tariffs)
        {
            if (choiceCard == null) { Hh.Warn(LogCategory.Mission, "MissionRunner: no choiceCard wired."); return; }
            choiceCard.Show(memory, promptText, tariffs);
        }

        public void ShowEveningLedger(string summary, System.Collections.Generic.IReadOnlyList<string> heldMemoryTitles)
        {
            if (eveningLedger == null) { Hh.Warn(LogCategory.Mission, "MissionRunner: no eveningLedger wired."); return; }
            for (int i = 0; i < 3; i++)
                eveningLedger.SetSlotLabel(i, saveService.GetSlotLabel(i));
            eveningLedger.Show(summary, heldMemoryTitles);
        }

        private void OnMoralChoiceMade(MoralChoiceMadeEvent evt)
        {
            var vs = ServiceLocator.Get<VillageState>();
            if (vs != null && saveService != null) saveService.Save(-1, vs);
        }

        private void OnSlotChosen(int slot)
        {
            var vs = ServiceLocator.Get<VillageState>();
            if (vs != null) saveService.Save(slot, vs);
        }

        private void OnEndOfDayConfirmed()
        {
            System.Action transition = () =>
            {
                var gm = GameManager.Instance;
                if (gm != null) gm.EndDay();
                if (!string.IsNullOrEmpty(nextMissionScene) && gm != null)
                    gm.LoadScene(nextMissionScene);
            };

            if (endOfDaySequencer != null)
                endOfDaySequencer.BeginNightSequence(false, null, transition);  // no dream rig
            else
                transition();
        }

        private void OnMissionCompleted(MissionCompletedEvent evt)
        {
            if (mission == null || evt.MissionId != mission.missionId) return;
            var vs = ServiceLocator.Get<VillageState>();
            if (vs != null && !vs.completedMissionIds.Contains(mission.missionId))
                vs.completedMissionIds.Add(mission.missionId);
            if (vs != null && saveService != null) saveService.Save(-1, vs);
        }
    }
}
