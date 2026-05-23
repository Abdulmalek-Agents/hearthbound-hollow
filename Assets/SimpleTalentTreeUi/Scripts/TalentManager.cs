using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleTalentTreeUi
{
    /// <summary>
    /// Simple global manager for talent trees.
    /// Holds a list of TalentTree instances, exposes an ActiveTree,
    /// and provides a basic save/load implementation through ISaveable.
    /// </summary>
    public class TalentManager : MonoBehaviour, ISaveable
    {
        /// <summary>
        /// Global singleton-like access. You can assign a reference explicitly
        /// in other components instead of relying on this field if you prefer.
        /// </summary>
        public static TalentManager Instance;

        /// <summary>
        /// Currently active talent tree used by UI and links.
        /// </summary>
        public TalentTree ActiveTree { get; private set; }

        [SerializeField] private List<TalentTree> talentTrees;

        public delegate void TalentTreeActivate();

        /// <summary>
        /// Invoked whenever a talent tree becomes active (ActiveTree is set).
        /// </summary>
        public event TalentTreeActivate OnTalentActivated;

        /// <summary>
        /// Invoked when all talent trees have finished loading from a save.
        /// </summary>
        public event Action OnLoadTalentFinished;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[SimpleTalentTreeUI] Multiple TalentManager instances detected. Keeping the first one.", this);
                return;
            }

            Instance = this;

            if (talentTrees == null)
                talentTrees = new List<TalentTree>();
        }

        private void Start()
        {
            if (ActiveTree == null && talentTrees.Count > 0)
            {
                ActiveTree = talentTrees[0];
            }

            OnTalentActivated?.Invoke();
        }

        /// <summary>
        /// Returns the first TalentTree with a matching TreeId, or null if none is found.
        /// </summary>
        public TalentTree FindTreeByID(string treeId)
        {
            if (talentTrees == null || string.IsNullOrEmpty(treeId))
                return null;

            for (int i = 0; i < talentTrees.Count; i++)
            {
                var tree = talentTrees[i];
                if (tree != null && string.Equals(tree.TreeId, treeId, StringComparison.Ordinal))
                    return tree;
            }

            return null;
        }

        /// <summary>
        /// Ensures there is a runtime TalentTree matching the given ID.
        /// Creates one if necessary, or clears an existing one when used from the editor.
        /// </summary>
        public TalentTree InitializeNewTreeFromEditor(string treeID)
        {
            if (talentTrees == null)
                talentTrees = new List<TalentTree>();

            var currentTree = FindTreeByID(treeID);
            if (currentTree != null)
            {
                currentTree.ClearTalents();
                ActiveTree = currentTree;
                OnTalentActivated?.Invoke();
                return currentTree;
            }

            var newTree = new TalentTree();
            newTree.Initialize(treeID);

            talentTrees.Add(newTree);

            ActiveTree = newTree;

            OnTalentActivated?.Invoke();

            return newTree;
        }

        #region Save / Load

        /// <summary>
        /// Strongly typed save method used internally and by advanced users.
        /// </summary>
        public TalentManagerSaveData SaveTyped()
        {
            var result = new TalentManagerSaveData
            {
                talentTreesData = new List<TalentTreeSaveData>()
            };

            if (talentTrees == null)
                return result;

            for (int i = 0; i < talentTrees.Count; i++)
            {
                var tree = talentTrees[i];
                if (tree == null)
                    continue;

                var treeData = new TalentTreeSaveData
                {
                    id = tree.TreeId,
                    talentPoints = tree.TalentPoints,
                    pointPools = tree.GetPointPoolsForSave(),
                    talentData = new List<TalentSaveData>()
                };

                var talents = tree.Talents;
                if (talents != null)
                {
                    for (int j = 0; j < talents.Count; j++)
                    {
                        var talent = talents[j];
                        if (talent == null)
                            continue;

                        treeData.talentData.Add(new TalentSaveData
                        {
                            id = talent.Id,
                            currentPoints = talent.CurrentPoints,
                            isAvailable = talent.IsAvailable,
                            wasAvailable = talent.WasAvailable
                        });
                    }
                }

                result.talentTreesData.Add(treeData);
            }

            return result;
        }

        /// <summary>
        /// ISaveable implementation. Returns a TalentManagerSaveData instance.
        /// </summary>
        public object Save()
        {
            return SaveTyped();
        }

        /// <summary>
        /// Strongly typed load method used internally and by advanced users.
        /// </summary>
        public void LoadTyped(TalentManagerSaveData data)
        {
            if (data.talentTreesData != null)
            {
                for (int i = 0; i < data.talentTreesData.Count; i++)
                {
                    var talentTreeToLoad = data.talentTreesData[i];
                    var talentTreeFound = FindTreeByID(talentTreeToLoad.id);
                    if (talentTreeFound == null)
                        continue;

                    talentTreeFound.LoadTree(talentTreeToLoad);
                }
            }

            OnLoadTalentFinished?.Invoke();
        }

        /// <summary>
        /// ISaveable implementation. Expects a TalentManagerSaveData instance.
        /// </summary>
        public void Load(object data)
        {
            if (data == null)
            {
                Debug.LogWarning("[SimpleTalentTreeUI] TalentManager.Load called with null data.", this);
                return;
            }

            if (data is TalentManagerSaveData typedData)
            {
                LoadTyped(typedData);
            }
            else
            {
                Debug.LogError($"[SimpleTalentTreeUI] TalentManager.Load expected {nameof(TalentManagerSaveData)} but received {data.GetType().Name}.", this);
            }
        }

        #endregion
    }

    [Serializable]
    public struct TalentManagerSaveData
    {
        public List<TalentTreeSaveData> talentTreesData;
    }

    [Serializable]
    public struct TalentTreeSaveData
    {
        public string id;
        public int talentPoints;
        public List<PointPoolSaveData> pointPools;
        public List<TalentSaveData> talentData;
    }

    [Serializable]
    public struct PointPoolSaveData
    {
        public TalentPointType type;
        public int value;
    }

    [Serializable]
    public struct TalentSaveData
    {
        public int id;
        public int currentPoints;
        public bool isAvailable;
        public bool wasAvailable;
    }
}
