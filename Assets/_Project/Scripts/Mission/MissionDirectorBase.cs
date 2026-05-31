// =============================================================================
// MissionDirectorBase.cs — Hearthbound Hollow
// Shared base for all 30 mission directors.
// Phase 76 — 30-Mission Architecture.
// =============================================================================
using System.Collections;
using UnityEngine;
using HearthboundHollow.Core;
using HearthboundHollow.Memory;

namespace HearthboundHollow.Mission
{
    /// <summary>
    /// Base class that handles the common mission lifecycle:
    ///   Init → OpeningBeat → MainArc → MoralChoice → WorkbenchWork → ClosingBeat → DreamTrigger
    ///
    /// Concrete directors (Mission01Director, Mission03Director, …) override the
    /// abstract beats and call <c>base.xxx()</c> to let the shared scaffold handle
    /// state tracking, EventBus publishing, and save triggers.
    ///
    /// Lives in HearthboundHollow.Mission asmdef.
    /// Phase 76.
    /// </summary>
    public abstract class MissionDirectorBase : MonoBehaviour
    {
        // ─── Inspector ───────────────────────────────────────────────────────

        [Header("Config")]
        [Tooltip("The NarrativeLevelConfigSO for this mission. Wired by the scene builder.")]
        [SerializeField] protected NarrativeLevelConfigSO config;

        // ─── State ───────────────────────────────────────────────────────────

        protected VillageState    State    { get; private set; }
        protected DialogueUI      Dialogue { get; private set; }
        protected bool            MissionStarted  { get; private set; }
        protected bool            MissionComplete { get; private set; }

        // ─── Unity lifecycle ─────────────────────────────────────────────────

        protected virtual void Awake()
        {
            State    = ServiceLocator.Get<VillageState>();
            Dialogue = ServiceLocator.Get<DialogueUI>();
        }

        protected virtual void Start()
        {
            if (config == null)
            {
                Debug.LogError($"[{GetType().Name}] config is null. Did the scene builder wire it?");
                return;
            }
            StartMission();
        }

        // ─── Mission lifecycle ────────────────────────────────────────────────

        public void StartMission()
        {
            if (MissionStarted) return;
            MissionStarted = true;

            EventBus.Publish(new MissionStartedEvent { MissionIndex = config.missionIndex });
            OnMissionStart();
        }

        /// <summary>Called once at mission start. Begin your opening beat here.</summary>
        protected abstract void OnMissionStart();

        /// <summary>
        /// Call when the player has made their moral choice and the mission narrative is resolved.
        /// </summary>
        protected void CompleteMission(string outcomeId)
        {
            if (MissionComplete) return;
            MissionComplete = true;

            // Grant rewards from config.
            if (State != null)
            {
                State.coin += config.coinRewardBase;
                State.AddTrust(config.primaryVillager?.villagerId ?? "", config.trustGrant);
                if (config.warmthGrant > 0)
                    State.predecessorTrailWarmth =
                        Mathf.Min(100, State.predecessorTrailWarmth + config.warmthGrant);

                State.currentMissionIndex = Mathf.Max(State.currentMissionIndex,
                    config.missionIndex + 1);
            }

            EventBus.Publish(new MissionCompletedEvent
                { MissionIndex = config.missionIndex, OutcomeId = outcomeId });

            OnMissionComplete(outcomeId);
            StartCoroutine(EndOfDayRoutine());
        }

        /// <summary>Override to handle mission-specific completion logic.</summary>
        protected virtual void OnMissionComplete(string outcomeId) { }

        // ─── Shared helpers ───────────────────────────────────────────────────

        /// <summary>Lock or unlock WASD via the Core IMovementLockable interface (D-035).</summary>
        protected static void LockPlayer(bool locked)
        {
            var lockable = ServiceLocator.Get<IMovementLockable>();
            if (lockable != null) lockable.MovementLocked = locked;
        }

        /// <summary>Publish a moral choice event and apply consequence from config.</summary>
        protected void ApplyMoralChoice(MoralChoiceType choice)
        {
            EventBus.Publish(new MoralChoiceMadeEvent
                { MissionIndex = config.missionIndex, Choice = choice,
                  MemoryOrbId  = config.primaryOrbId });

            if (State == null) return;
            foreach (var c in config.choiceConsequences)
            {
                if (c.choice != choice) continue;
                State.coin += c.coinDelta;
                State.predecessorTrailWarmth =
                    Mathf.Clamp(State.predecessorTrailWarmth + c.warmthDelta, 0, 100);
                State.AddTrust(config.primaryVillager?.villagerId ?? "", c.trustDelta);
                break;
            }
        }

        // ─── End-of-day routine ───────────────────────────────────────────────

        private IEnumerator EndOfDayRoutine()
        {
            yield return new WaitForSeconds(0.5f);
            // Signal Evening Ledger to show, then dream, then next mission.
            EventBus.Publish(new DayEndedEvent { DayIndex = State?.currentDayIndex ?? 0 });
        }
    }

    /// <summary>
    /// Minimal stub for DialogueUI — real implementation wired at scene level.
    /// Allows directors to compile without requiring the full UI assembly.
    /// </summary>
    public abstract class DialogueUI : MonoBehaviour
    {
        public abstract void StartDialogue(string yarnNode);
        public abstract void ShowOneLiner(string speaker, string text);
        public abstract void Hide();
    }
}
