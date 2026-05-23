using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SimpleTalentTreeUi
{
    /// <summary>
    /// Binding between a talent (by ID) and a UnityEvent fired when its points change.
    /// Used by PlayerTalentLink to connect the talent system to gameplay scripts.
    /// </summary>
    [Serializable]
    public class TalentBinding
    {
        /// <summary>
        /// Runtime ID of the talent inside the TalentTree.
        /// </summary>
        public int talentId;

        /// <summary>
        /// Cached title from the Talent (for display / debugging).
        /// </summary>
        public string title;

        /// <summary>
        /// Cached icon from the Talent (for display).
        /// </summary>
        public Sprite icon;

        /// <summary>
        /// Event invoked when this talent's points change.
        /// Parameters: previousPoints, currentPoints.
        /// </summary>
        public UnityEvent<int, int> onPointsChanged;
    }

    /// <summary>
    /// Links a runtime TalentTree (managed by TalentManager) to gameplay scripts
    /// using UnityEvents defined per talent.
    /// </summary>
    public class PlayerTalentLink : MonoBehaviour
    {
        [Header("Manager")]
        [Tooltip("Optional explicit manager reference. If null, TalentManager.Instance will be used.")]
        [SerializeField] private TalentManager manager;

        [Header("Tree Reference")]
        [Tooltip("Talent Tree asset used to resolve the runtime tree ID.")]
        public TalentTreeSO treeAsset;

        [Tooltip("Runtime tree ID. Automatically filled from treeAsset if assigned.")]
        public string treeId;

        [Tooltip("If true, the link will be established automatically on OnEnable.")]
        public bool linkAutomatically = true;

        [Header("Talent Bindings")]
        [Tooltip("Bindings between talent IDs and UnityEvents fired when points change.")]
        public List<TalentBinding> bindings = new List<TalentBinding>();

        private TalentTree _tree;

        private TalentManager ResolvedManager => manager != null ? manager : TalentManager.Instance;

        private void OnEnable()
        {
            if (linkAutomatically)
                Link();
        }

        private void OnDisable()
        {
            Unlink();
        }

        /// <summary>
        /// Resolves the runtime TalentTree from TalentManager and subscribes to its events.
        /// Also synchronizes the bindings list to match the current talents.
        /// </summary>
        public void Link()
        {
            var resolvedManager = ResolvedManager;
            if (resolvedManager == null)
            {
                Debug.LogWarning("[SimpleTalentTreeUI] PlayerTalentLink.Link called but no TalentManager is available.", this);
                return;
            }

            // If an asset is assigned, use its ID as the runtime treeId.
            if (treeAsset != null)
                treeId = treeAsset.ID;

            _tree = resolvedManager.FindTreeByID(treeId);
            if (_tree == null)
            {
                Debug.LogWarning($"[SimpleTalentTreeUI] PlayerTalentLink could not find TalentTree with id '{treeId}'.", this);
                return;
            }

            SyncBindingsWithTree();

            // Avoid double-subscription in case Link is called manually more than once.
            _tree.OnTalentApplied -= OnTalentChanged;
            _tree.OnTalentApplied += OnTalentChanged;

            // If you later add an OnTalentReverted event, it can be wired the same way:
            // _tree.OnTalentReverted -= OnTalentChanged;
            // _tree.OnTalentReverted += OnTalentChanged;
        }

        /// <summary>
        /// Unsubscribes from the current tree events and clears the local reference.
        /// </summary>
        public void Unlink()
        {
            if (_tree == null)
                return;

            _tree.OnTalentApplied -= OnTalentChanged;
            // _tree.OnTalentReverted -= OnTalentChanged;

            _tree = null;
        }

        /// <summary>
        /// Ensures the bindings list matches the talents in the current tree,
        /// preserving previously configured UnityEvents where possible.
        /// </summary>
        private void SyncBindingsWithTree()
        {
            if (_tree == null || _tree.Talents == null)
                return;

            // Cache existing UnityEvents by talentId so we can preserve them.
            var eventsByTalentId = new Dictionary<int, UnityEvent<int, int>>();
            for (int i = 0; i < bindings.Count; i++)
            {
                var b = bindings[i];
                if (!eventsByTalentId.ContainsKey(b.talentId) && b.onPointsChanged != null)
                {
                    eventsByTalentId.Add(b.talentId, b.onPointsChanged);
                }
            }

            bindings.Clear();

            var talents = _tree.Talents;
            for (int i = 0; i < talents.Count; i++)
            {
                var t = talents[i];
                if (t == null)
                    continue;

                var binding = new TalentBinding
                {
                    talentId = t.Id,
                    title = t.Title,
                    icon = t.Icon,
                    onPointsChanged = eventsByTalentId.TryGetValue(t.Id, out var evt)
                        ? evt
                        : new UnityEvent<int, int>()
                };

                bindings.Add(binding);
            }
        }

        /// <summary>
        /// Called when the tree applies a talent change.
        /// Forwards the previous/current point values to the configured UnityEvent.
        /// </summary>
        private void OnTalentChanged(Talent talent, int previous, int current)
        {
            if (talent == null || bindings == null)
                return;

            var binding = bindings.Find(b => b.talentId == talent.Id);
            if (binding != null)
            {
                binding.onPointsChanged?.Invoke(previous, current);
            }
        }
    }
}
