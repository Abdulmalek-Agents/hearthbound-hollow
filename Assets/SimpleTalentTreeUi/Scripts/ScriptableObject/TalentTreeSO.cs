using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleTalentTreeUi
{
    /// <summary>
    /// ScriptableObject representing a full talent tree asset.
    /// Contains the graph nodes and connections used by the editor,
    /// and exposes a stable runtime ID that is used by the TalentTree system.
    /// </summary>
    public class TalentTreeSO : ScriptableObject
    {
        [SerializeField]
        private string id;

        /// <summary>
        /// Unique identifier for this tree asset.
        /// Used at runtime to match TalentTreeSO to a TalentTree instance.
        /// </summary>
        public string ID => id;

        [Header("Graph controlled variables")]

        /// <summary>
        /// Display name for this talent tree.
        /// </summary>
        public string talentTreeName = "New Created Tree";

        /// <summary>
        /// List of all nodes inside this talent tree graph.
        /// </summary>
        public List<TalentTreeNodeSO> talentNodes;

        /// <summary>
        /// List of all connections between nodes in this tree.
        /// </summary>
        public List<TalentTreeConnectionSO> connections;

        /// <summary>
        /// Scroll position X of the graph view (editor only, but kept here for convenience).
        /// </summary>
        public float graphX;

        /// <summary>
        /// Scroll position Y of the graph view (editor only, but kept here for convenience).
        /// </summary>
        public float graphY;

        private bool isInitialized;

        private void OnEnable()
        {
            Initialize();
        }

        /// <summary>
        /// Ensures that the asset is in a valid state:
        /// - Generates a unique ID if needed.
        /// - Ensures lists are created.
        /// </summary>
        public void Initialize()
        {
            if (isInitialized)
                return;

            if (string.IsNullOrEmpty(id))
                id = Guid.NewGuid().ToString();

            if (talentNodes == null)
                talentNodes = new List<TalentTreeNodeSO>();

            if (connections == null)
                connections = new List<TalentTreeConnectionSO>();

            isInitialized = true;
        }
    }
}
