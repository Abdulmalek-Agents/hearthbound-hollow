using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace SimpleTalentTreeUi
{
    /// <summary>
    /// Runtime data for a single talent inside a TalentTree.
    /// Tracks points, cost, availability and prerequisites.
    /// </summary>
    [Serializable]
    public class Talent
    {
        [FormerlySerializedAs("Id")]
        [SerializeField] private int id;

        [FormerlySerializedAs("Title")]
        [SerializeField] private string title;

        [FormerlySerializedAs("Description")]
        [SerializeField] private string description;

        [FormerlySerializedAs("Icon")]
        [SerializeField] private Sprite icon;

        [FormerlySerializedAs("InitialCost")]
        [SerializeField] private int initialCost;

        [FormerlySerializedAs("MaxPoints")]
        [SerializeField] private int maxPoints;

        [FormerlySerializedAs("CostIncrementFactor")]
        [SerializeField] private int costIncrementFactor;

        [FormerlySerializedAs("CurrentPoints")]
        [SerializeField] private int currentPoints;

        [FormerlySerializedAs("MinPointsToAllowNextTalent")]
        [SerializeField] private int minPointsToAllowNextTalent;

        [FormerlySerializedAs("IsAvailable")]
        [SerializeField] private bool isAvailable;

        [FormerlySerializedAs("WasAvailable")]
        [SerializeField] private bool wasAvailable;

        [FormerlySerializedAs("PointType")]
        [SerializeField] private TalentPointType pointType;

        [FormerlySerializedAs("PreviousTalentIds")]
        [SerializeField] private List<int> previousTalentIds;

        /// <summary>
        /// Optional custom cost calculator. If not null, PointsCost will use this delegate
        /// instead of the default linear cost formula.
        /// </summary>
        public Func<Talent, int> CustomCostCalculator { get; set; }

        /// <summary>
        /// Runtime ID of this talent within a TalentTree.
        /// </summary>
        public int Id
        {
            get => id;
            set => id = value;
        }

        /// <summary>
        /// Display title used in UI.
        /// </summary>
        public string Title
        {
            get => title;
            set => title = value;
        }

        /// <summary>
        /// Description used by tooltips or other UI.
        /// </summary>
        public string Description
        {
            get => description;
            set => description = value;
        }

        /// <summary>
        /// Icon associated with this talent.
        /// </summary>
        public Sprite Icon
        {
            get => icon;
            set => icon = value;
        }

        /// <summary>
        /// Base cost for the first point.
        /// </summary>
        public int InitialCost
        {
            get => initialCost;
            set => initialCost = value;
        }

        /// <summary>
        /// Maximum number of points that can be invested.
        /// </summary>
        public int MaxPoints
        {
            get => maxPoints;
            set => maxPoints = value;
        }

        /// <summary>
        /// Additional cost per already invested point.
        /// </summary>
        public int CostIncrementFactor
        {
            get => costIncrementFactor;
            set => costIncrementFactor = value;
        }

        /// <summary>
        /// Current number of points invested in this talent (including pending changes).
        /// </summary>
        public int CurrentPoints
        {
            get => currentPoints;
            set => currentPoints = value;
        }

        /// <summary>
        /// Minimum points required in this talent to unlock its children.
        /// </summary>
        public int MinPointsToAllowNextTalent
        {
            get => minPointsToAllowNextTalent;
            set => minPointsToAllowNextTalent = value;
        }

        /// <summary>
        /// Whether the talent is currently available to be improved.
        /// </summary>
        public bool IsAvailable
        {
            get => isAvailable;
            set => isAvailable = value;
        }

        /// <summary>
        /// Whether the talent was available before the last availability update.
        /// </summary>
        public bool WasAvailable
        {
            get => wasAvailable;
            set => wasAvailable = value;
        }

        /// <summary>
        /// Which point pool is used to pay for this talent.
        /// </summary>
        public TalentPointType PointType
        {
            get => pointType;
            set => pointType = value;
        }

        /// <summary>
        /// Previous talents in the tree that act as prerequisites.
        /// </summary>
        public List<int> PreviousTalentIds
        {
            get => previousTalentIds;
            set => previousTalentIds = value;
        }

        /// <summary>
        /// Last applied number of points (committed state).
        /// </summary>
        public int LastAppliedPoints => _oldPoints;

        /// <summary>
        /// Cost of the next point based on the current level.
        /// </summary>
        public int PointsCost
        {
            get
            {
                if (CustomCostCalculator != null)
                    return CustomCostCalculator(this);

                return Mathf.Max(initialCost, initialCost + (currentPoints * costIncrementFactor));
            }
        }

        private int _oldPoints;
        private bool _dirtyAvailability;

        /// <summary>
        /// Creates a new runtime talent with the provided parameters.
        /// </summary>
        public Talent(int id, string title, string description, Sprite icon, int initialCost, int maxPoints, int costIncrementFactor,
            int minPointsToAllowNextTalent, TalentPointType pointType = TalentPointType.Global)
        {
            _oldPoints = 0;
            currentPoints = 0;

            this.id = id;
            this.title = title;
            this.description = description;
            this.icon = icon;
            this.initialCost = initialCost;
            this.maxPoints = maxPoints;
            this.costIncrementFactor = costIncrementFactor;
            this.minPointsToAllowNextTalent = minPointsToAllowNextTalent;
            this.pointType = pointType;
        }

        #region Prerequisites

        /// <summary>
        /// Adds a prerequisite talent ID to this talent.
        /// </summary>
        public void SetPreviousTalentId(int previousTalentId)
        {
            if (previousTalentIds == null)
                previousTalentIds = new List<int>();

            if (!previousTalentIds.Contains(previousTalentId))
                previousTalentIds.Add(previousTalentId);
        }

        #endregion

        #region Apply / Revert points

        /// <summary>
        /// Commits the current number of points as the new applied state.
        /// </summary>
        public void ApplyPoints()
        {
            _oldPoints = currentPoints;
            _dirtyAvailability = false;
            // Hook for game-side logic if needed.
        }

        /// <summary>
        /// Reverts CurrentPoints back to the last applied value.
        /// </summary>
        public void RevertPoints()
        {
            currentPoints = _oldPoints;
            _dirtyAvailability = false;
        }

        /// <summary>
        /// Reverts availability to the previous value before last update.
        /// </summary>
        public void RevertAvailability()
        {
            isAvailable = wasAvailable;
            _dirtyAvailability = false;
        }

        /// <summary>
        /// Legacy name kept for backwards compatibility.
        /// </summary>
        [Obsolete("Use RevertAvailability instead.")]
        public void RevertAvaiability()
        {
            RevertAvailability();
        }

        /// <summary>
        /// Applies any pending availability changes and stores them in WasAvailable.
        /// </summary>
        public void ApplyDirty()
        {
            if (!_dirtyAvailability)
                return;

            wasAvailable = isAvailable;
            _dirtyAvailability = false;
        }

        /// <summary>
        /// Sets availability and marks this talent as “dirty” if it changed.
        /// Used by the tree when evaluating prerequisites.
        /// </summary>
        public void SetAvailable(bool available)
        {
            if (_dirtyAvailability)
                return;

            wasAvailable = isAvailable;
            isAvailable = available;

            _dirtyAvailability = (wasAvailable != isAvailable);
        }

        /// <summary>
        /// Calculates the total cost to revert points from CurrentPoints back to LastAppliedPoints.
        /// </summary>
        public int GetRevertCost()
        {
            int cumulativeCost = 0;
            for (int level = currentPoints - 1; level >= _oldPoints; level--)
            {
                cumulativeCost += initialCost + (level * costIncrementFactor);
            }

            return cumulativeCost;
        }

        /// <summary>
        /// Adds points to this talent. Tree-level logic should enforce min/max constraints.
        /// </summary>
        public void AddPoints(int value)
        {
            currentPoints += value;
            _dirtyAvailability = true;
        }

        /// <summary>
        /// Removes points from this talent. Tree-level logic should enforce min/max constraints.
        /// </summary>
        public void RemovePoints(int value)
        {
            currentPoints -= value;
            _dirtyAvailability = true;
        }

        /// <summary>
        /// Sets the number of points on this talent directly.
        /// </summary>
        public void SetPoints(int value)
        {
            currentPoints = value;
        }

        #endregion
    }
}
