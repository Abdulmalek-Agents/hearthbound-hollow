using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleTalentTreeUi
{
    /// <summary>
    /// Runtime representation of a talent tree. 
    /// Holds talents, point pools and editing state (apply / cancel).
    /// </summary>
    [Serializable]
    public class TalentTree
    {
        [SerializeField] private string treeId;
        [SerializeField] private List<Talent> talents;

        [Serializable]
        public struct PointPool
        {
            public TalentPointType type;
            public int value;
        }

        [SerializeField] private List<PointPool> pointPools = new List<PointPool>();

        /// <summary>
        /// Optional custom evaluator used to determine if a talent is available
        /// based on its prerequisites. If null, the default "all prerequisites required"
        /// logic is used.
        /// </summary>
        public Func<TalentTree, Talent, bool> CustomPrerequisiteEvaluator { get; set; }

        /// <summary>
        /// Read-only list of talents in this tree.
        /// </summary>
        public IReadOnlyList<Talent> Talents => talents;

        /// <summary>
        /// Unique identifier for this talent tree. Matches TalentTreeSO.ID.
        /// </summary>
        public string TreeId => treeId;

        private List<Talent> applyingTalents;
        private bool IsEditingInternal => applyingTalents != null && applyingTalents.Count > 0;

        public delegate void TalentUpdatedHandler();

        /// <summary>
        /// Invoked whenever talent values or availability change and the UI should refresh.
        /// </summary>
        public event TalentUpdatedHandler OnTalentUpdated;

        /// <summary>
        /// Invoked whenever there are pending talent changes to apply/cancel.
        /// Parameter is true when there are unsaved changes.
        /// </summary>
        [Obsolete("Use OnEditingStateChanged instead.")]
        public event Action<bool> IsEditing;

        /// <summary>
        /// Invoked whenever there are pending talent changes to apply/cancel.
        /// Parameter is true when there are unsaved changes.
        /// </summary>
        public event Action<bool> OnEditingStateChanged;

        /// <summary>
        /// Invoked when a talent is successfully applied. Parameters: talent, previousPoints, currentPoints.
        /// </summary>
        public event Action<Talent, int, int> OnTalentApplied;

        /// <summary>
        /// Invoked when the global point pool (TalentPointType.Global) changes.
        /// </summary>
        public Action<int> OnTalentPointsChanged;

        /// <summary>
        /// Invoked when a specific point pool changes (including Global).
        /// </summary>
        public Action<TalentPointType, int> OnPointPoolChanged;

        /// <summary>
        /// Shorthand access to the global point pool.
        /// </summary>
        public int TalentPoints
        {
            get => GetPoints(TalentPointType.Global);
            set => SetPoints(TalentPointType.Global, value);
        }

        /// <summary>
        /// Returns true while there are unapplied talent changes.
        /// </summary>
        public bool IsEditingActive => IsEditingInternal;

        #region Initialization

        /// <summary>
        /// Initializes this runtime tree from a TalentTreeSO created in the editor.
        /// </summary>
        public void Initialize(TalentTreeSO newTalentTreeSO)
        {
            if (newTalentTreeSO == null)
            {
                Debug.LogError("[SimpleTalentTreeUI] TalentTree.Initialize called with null TalentTreeSO.");
                return;
            }

            treeId = newTalentTreeSO.ID;

            if (talents == null)
                talents = new List<Talent>();
            else
                talents.Clear();

            InitializePointPools();
            CreateTalents(newTalentTreeSO.talentNodes);
        }

        /// <summary>
        /// Initializes an empty tree with the given ID. Talents can be added later.
        /// </summary>
        public void Initialize(string id)
        {
            treeId = id;

            if (talents == null)
                talents = new List<Talent>();
            else
                talents.Clear();

            InitializePointPools();
        }

        private void InitializePointPools()
        {
            if (pointPools == null)
                pointPools = new List<PointPool>();
            else
                pointPools.Clear();

            foreach (TalentPointType type in Enum.GetValues(typeof(TalentPointType)))
            {
                pointPools.Add(new PointPool
                {
                    type = type,
                    value = 0
                });
            }
        }

        /// <summary>
        /// Clears existing talents and recreates them from the provided TalentTreeNodeSO list.
        /// </summary>
        public void OverrideTalents(List<TalentTreeNodeSO> talentTreeNodes)
        {
            ClearTalents();
            CreateTalents(talentTreeNodes);
        }

        /// <summary>
        /// Removes all talents from this tree. Point pools remain unchanged.
        /// </summary>
        public void ClearTalents()
        {
            if (talents == null)
                talents = new List<Talent>();
            else
                talents.Clear();
        }

        #endregion

        #region Points (multi-pool)

        /// <summary>
        /// Gets the amount of points available in a specific point pool.
        /// </summary>
        public int GetPoints(TalentPointType type)
        {
            if (pointPools == null || pointPools.Count == 0)
                InitializePointPools();

            int index = pointPools.FindIndex(p => p.type == type);
            if (index < 0)
                return 0;

            return pointPools[index].value;
        }

        /// <summary>
        /// Sets the amount of points available in a specific point pool.
        /// </summary>
        public void SetPoints(TalentPointType type, int value)
        {
            if (pointPools == null || pointPools.Count == 0)
                InitializePointPools();

            int index = pointPools.FindIndex(p => p.type == type);
            if (index < 0)
            {
                pointPools.Add(new PointPool
                {
                    type = type,
                    value = value
                });
            }
            else
            {
                var pool = pointPools[index];
                pool.value = value;
                pointPools[index] = pool;
            }

            // Notify global talent points first (using current Global pool value)
            OnTalentPointsChanged?.Invoke(GetPoints(TalentPointType.Global));

            int currentPoolValue = GetPoints(type);
            OnPointPoolChanged?.Invoke(type, currentPoolValue);
        }

        /// <summary>
        /// Adds points to the given point pool.
        /// </summary>
        public void AddPoints(TalentPointType type, int amount)
        {
            int current = GetPoints(type);
            SetPoints(type, current + amount);
        }

        /// <summary>
        /// Removes points from the given point pool.
        /// </summary>
        public void RemovePoints(TalentPointType type, int amount)
        {
            int current = GetPoints(type);
            SetPoints(type, current - amount);
        }

        /// <summary>
        /// Adds to the global point pool.
        /// </summary>
        public void AddPoints(int amount) => AddPoints(TalentPointType.Global, amount);

        /// <summary>
        /// Removes from the global point pool.
        /// </summary>
        public void RemovePoints(int amount) => RemovePoints(TalentPointType.Global, amount);

        /// <summary>
        /// Returns a serializable snapshot of all point pools for saving.
        /// </summary>
        public List<PointPoolSaveData> GetPointPoolsForSave()
        {
            var list = new List<PointPoolSaveData>();

            if (pointPools == null)
                return list;

            foreach (var pool in pointPools)
            {
                list.Add(new PointPoolSaveData
                {
                    type = pool.type,
                    value = pool.value
                });
            }

            return list;
        }

        /// <summary>
        /// Loads point pools from saved data. Supports both old single-pool and new multi-pool formats.
        /// </summary>
        public void LoadPointPools(TalentTreeSaveData data)
        {
            InitializePointPools();

            if (data.pointPools != null && data.pointPools.Count > 0)
            {
                foreach (var saved in data.pointPools)
                {
                    SetPoints(saved.type, saved.value);
                }
            }
            else
            {
                // Backwards compatibility with older save format that only stored global points.
                SetPoints(TalentPointType.Global, data.talentPoints);
            }

            OnTalentPointsChanged?.Invoke(GetPoints(TalentPointType.Global));
        }

        #endregion

        #region Talents

        /// <summary>
        /// Finds a talent by its runtime ID. Returns null if not found.
        /// </summary>
        public Talent FindTalentByID(int id)
        {
            if (talents == null)
                return null;

            for (int i = 0; i < talents.Count; i++)
            {
                var talent = talents[i];
                if (talent != null && talent.Id == id)
                    return talent;
            }

            return null;
        }

        /// <summary>
        /// Attempts to spend points and add 1 point to the given talent.
        /// The change is temporary until ApplyTalent() or CancelApply() is called.
        /// </summary>
        public void TryAddTalentToApplyList(Talent talent)
        {
            if (talent == null)
                return;

            var availablePoints = GetPoints(talent.PointType);
            if (availablePoints < talent.PointsCost)
                return;

            if (talent.CurrentPoints == talent.MaxPoints)
                return;

            RemovePoints(talent.PointType, talent.PointsCost);

            talent.AddPoints(1);

            ChangeTalentsAvailability();

            AddTalentToTempApplyList(talent);

            RaiseEditingStateChanged();
        }

        /// <summary>
        /// Marks a talent as pending change (to be applied or cancelled later).
        /// </summary>
        public void AddTalentToTempApplyList(Talent talent)
        {
            if (talent == null)
                return;

            if (applyingTalents == null)
                applyingTalents = new List<Talent>();

            if (!applyingTalents.Contains(talent))
                applyingTalents.Add(talent);

            OnTalentUpdated?.Invoke();
        }

        /// <summary>
        /// Commits all pending talent changes, firing OnTalentApplied events.
        /// </summary>
        public void ApplyTalent()
        {
            if (applyingTalents == null || applyingTalents.Count == 0)
                return;

            foreach (var talent in applyingTalents)
            {
                if (talent == null)
                    continue;

                int previous = talent.LastAppliedPoints;
                int current = talent.CurrentPoints;

                talent.ApplyPoints();

                OnTalentApplied?.Invoke(talent, previous, current);
            }

            if (talents != null)
            {
                for (int i = 0; i < talents.Count; i++)
                {
                    talents[i]?.ApplyDirty();
                }
            }

            StopEditing();
        }

        /// <summary>
        /// Cancels all pending talent changes and refunds spent points.
        /// </summary>
        public void CancelApply()
        {
            if (applyingTalents == null || applyingTalents.Count == 0)
            {
                StopEditing();
                return;
            }

            var refundedByType = new Dictionary<TalentPointType, int>();

            foreach (var t in applyingTalents)
            {
                if (t == null)
                    continue;

                int refund = t.GetRevertCost();

                if (!refundedByType.ContainsKey(t.PointType))
                    refundedByType[t.PointType] = 0;

                refundedByType[t.PointType] += refund;

                t.RevertPoints();
            }

            if (talents != null)
            {
                for (int i = 0; i < talents.Count; i++)
                {
                    talents[i]?.RevertAvailability();
                }
            }

            foreach (var kvp in refundedByType)
            {
                AddPoints(kvp.Key, kvp.Value);
            }

            StopEditing();

            OnTalentUpdated?.Invoke();
        }

        /// <summary>
        /// Clears the pending changes list and updates editing state.
        /// </summary>
        public void StopEditing()
        {
            if (applyingTalents != null)
                applyingTalents.Clear();

            RaiseEditingStateChanged();
        }

        private void RaiseEditingStateChanged()
        {
            bool editing = IsEditingInternal;

#pragma warning disable CS0618 // keep obsolete event for backwards compatibility
            IsEditing?.Invoke(editing);
#pragma warning restore CS0618

            OnEditingStateChanged?.Invoke(editing);
        }

        /// <summary>
        /// Creates a new talent from a TalentTreeNodeSO and adds it to this tree.
        /// </summary>
        public Talent CreateTalent(int talentId, TalentTreeNodeSO talentTreeNodeSO)
        {
            if (talentTreeNodeSO == null)
                return null;

            if (talents == null)
                talents = new List<Talent>();

            var newTalent = new Talent(
                talentId,
                talentTreeNodeSO.title,
                talentTreeNodeSO.description,
                talentTreeNodeSO.GetNodeImageAsSprite(),
                talentTreeNodeSO.cost,
                talentTreeNodeSO.maxPoints,
                talentTreeNodeSO.costIncrementFactor,
                talentTreeNodeSO.minPointsToAllowNextTalent,
                talentTreeNodeSO.pointType
            );

            talents.Add(newTalent);

            return newTalent;
        }

        private void CreateTalents(List<TalentTreeNodeSO> talentTreeNodes)
        {
            if (talentTreeNodes == null)
                return;

            if (talents == null)
                talents = new List<Talent>();
            else
                talents.Clear();

            int talentId = 0;
            for (int i = 0; i < talentTreeNodes.Count; i++)
            {
                var talentNode = talentTreeNodes[i];
                if (talentNode == null)
                    continue;

                var talent = new Talent(
                    talentId,
                    talentNode.title,
                    talentNode.description,
                    talentNode.GetNodeImageAsSprite(),
                    talentNode.cost,
                    talentNode.maxPoints,
                    talentNode.costIncrementFactor,
                    talentNode.minPointsToAllowNextTalent,
                    talentNode.pointType
                );

                talents.Add(talent);
                talentId++;
            }
        }

        /// <summary>
        /// Updates availability of talents based on their prerequisites.
        /// If a custom evaluator is set, it is used instead of the default logic.
        /// </summary>
        private void ChangeTalentsAvailability()
        {
            if (talents == null)
                return;

            for (int i = 0; i < talents.Count; i++)
            {
                var talent = talents[i];
                if (talent == null)
                    continue;

                // No prerequisites means always available.
                if (talent.PreviousTalentIds == null || talent.PreviousTalentIds.Count == 0)
                {
                    talent.SetAvailable(true);
                    continue;
                }

                bool available;

                if (CustomPrerequisiteEvaluator != null)
                {
                    available = CustomPrerequisiteEvaluator(this, talent);
                }
                else
                {
                    // Default: all prerequisites must meet their own required points.
                    available = true;

                    var previousIds = talent.PreviousTalentIds;
                    for (int j = 0; j < previousIds.Count; j++)
                    {
                        var previousTalent = FindTalentByID(previousIds[j]);
                        if (previousTalent == null ||
                            previousTalent.CurrentPoints < previousTalent.MinPointsToAllowNextTalent)
                        {
                            available = false;
                            break;
                        }
                    }
                }

                talent.SetAvailable(available);
            }
        }

        /// <summary>
        /// Restores tree state (points and talents) from saved data.
        /// </summary>
        public void LoadTree(TalentTreeSaveData talentTreeSaveData)
        {
            if (talentTreeSaveData.talentData == null)
            {
                Debug.LogWarning("[SimpleTalentTreeUI] LoadTree called with null talent data list.");
                return;
            }

            LoadPointPools(talentTreeSaveData);

            if (talentTreeSaveData.talentData.Count <= 0)
                return;

            for (int i = 0; i < talentTreeSaveData.talentData.Count; i++)
            {
                var talentToLoad = talentTreeSaveData.talentData[i];
                var talentFound = FindTalentByID(talentToLoad.id);
                if (talentFound == null)
                    continue;

                talentFound.SetPoints(talentToLoad.currentPoints);
                talentFound.SetAvailable(talentToLoad.isAvailable);
                talentFound.WasAvailable = talentToLoad.wasAvailable;
            }

            ChangeTalentsAvailability();
            OnTalentUpdated?.Invoke();
        }

        #endregion
    }
}
