using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleTalentTreeUi
{
    /// <summary>
    /// UI component for a single talent node button.
    /// Handles interaction with TalentManager and visuals (border, lines, tooltip).
    /// </summary>
    public class TalentNodeUi : MonoBehaviour
    {
        [Header("Manager")]
        [Tooltip("Optional explicit manager reference. If null, TalentManager.Instance will be used.")]
        [SerializeField] private TalentManager manager;

        [Header("References")]
        public TMP_Text title;
        public TMP_Text talentPoints;
        public Image iconImage;
        public Image borderImage;
        public Button nodeButton;

        [Header("Colors")]
        public Color disabledColor = Color.gray;
        public Color enabledColor = Color.white;

        [Header("Lines")]
        [SerializeField] public List<LineRendererUI> CustomUiLines;
        [SerializeField] public List<LineRenderer> LineRenderers;

        [Header("Talent IDs")]
        [SerializeField] private string talentTreeID;
        [SerializeField] private int talentID;

        private ToolTipInfo _toolTipInfo;
        private Talent _talent;
        private TalentTree _subscribedTree;

        private TalentManager ResolvedManager => manager != null ? manager : TalentManager.Instance;

        private void Awake()
        {
            if (nodeButton != null)
                nodeButton.onClick.AddListener(OnTalentNodeButtonClick);

            _toolTipInfo = GetComponent<ToolTipInfo>();
        }

        private void OnEnable()
        {
            var resolvedManager = ResolvedManager;
            if (resolvedManager == null)
            {
                Debug.LogWarning("[SimpleTalentTreeUI] TalentNodeUi enabled but no TalentManager is available.", this);
                return;
            }

            resolvedManager.OnTalentActivated += OnTalentActivated;
            resolvedManager.OnLoadTalentFinished += OnTalentLoadFinished;

            // In case the manager already has an active tree when we enable.
            OnTalentActivated();
        }

        private void OnDisable()
        {
            var resolvedManager = ResolvedManager;
            if (resolvedManager != null)
            {
                resolvedManager.OnTalentActivated -= OnTalentActivated;
                resolvedManager.OnLoadTalentFinished -= OnTalentLoadFinished;
            }

            if (_subscribedTree != null)
            {
                _subscribedTree.OnTalentUpdated -= OnTalentUpdated;
                _subscribedTree = null;
            }
        }

        private void OnTalentActivated()
        {
            var resolvedManager = ResolvedManager;
            if (resolvedManager == null)
                return;

            var activeTree = resolvedManager.ActiveTree;
            if (activeTree == null)
                return;

            if (!string.Equals(talentTreeID, activeTree.TreeId))
                return;

            if (_subscribedTree != null)
                _subscribedTree.OnTalentUpdated -= OnTalentUpdated;

            _subscribedTree = activeTree;
            _subscribedTree.OnTalentUpdated += OnTalentUpdated;

            _talent = activeTree.FindTalentByID(talentID);
            UpdateUi();
        }

        private void OnTalentLoadFinished()
        {
            UpdateUi();
        }

        private void OnTalentNodeButtonClick()
        {
            var resolvedManager = ResolvedManager;
            if (_talent == null || resolvedManager == null || resolvedManager.ActiveTree == null)
                return;

            resolvedManager.ActiveTree.TryAddTalentToApplyList(_talent);
        }

        private void OnTalentUpdated()
        {
            UpdateUi();
        }

        /// <summary>
        /// Adds a UI-only line connection (custom UI line) to this node.
        /// </summary>
        public void AddUiLine(LineRendererUI line)
        {
            if (line == null)
                return;

            if (CustomUiLines == null)
                CustomUiLines = new List<LineRendererUI>();

            CustomUiLines.Add(line);
        }

        /// <summary>
        /// Adds a standard LineRenderer connection to this node.
        /// </summary>
        public void AddLineRenderer(LineRenderer lineRenderer)
        {
            if (lineRenderer == null)
                return;

            if (LineRenderers == null)
                LineRenderers = new List<LineRenderer>();

            LineRenderers.Add(lineRenderer);
        }

        /// <summary>
        /// Called from the editor when generating UI to associate this node with a runtime talent.
        /// </summary>
        public void Initialize(string treeId, int talentId)
        {
            talentTreeID = treeId;
            talentID = talentId;
        }

        /// <summary>
        /// Refreshes label, tooltip and visual state based on the current talent.
        /// </summary>
        public void UpdateUi()
        {
            if (_talent == null)
                return;

            if (title != null)
                title.text = _talent.Title;

            if (iconImage != null)
                iconImage.sprite = _talent.Icon;

            if (talentPoints != null)
                talentPoints.text = $"{_talent.CurrentPoints}/{_talent.MaxPoints}";

            if (_toolTipInfo != null)
            {
                _toolTipInfo.mainText = _talent.Description;
                _toolTipInfo.costText = $"Cost: {_talent.PointsCost}";
            }

            bool isAvailable = _talent.IsAvailable;

            if (borderImage != null)
                borderImage.color = isAvailable ? enabledColor : disabledColor;

            if (CustomUiLines != null)
            {
                for (int i = 0; i < CustomUiLines.Count; i++)
                {
                    var line = CustomUiLines[i];
                    if (line == null) continue;
                    line.color = isAvailable ? enabledColor : disabledColor;
                }
            }

            if (LineRenderers != null)
            {
                for (int i = 0; i < LineRenderers.Count; i++)
                {
                    var line = LineRenderers[i];
                    if (line == null) continue;

                    line.startColor = isAvailable ? enabledColor : disabledColor;
                    line.endColor = isAvailable ? enabledColor : disabledColor;

                    if (line.sharedMaterial != null)
                        line.sharedMaterial.color = isAvailable ? enabledColor : disabledColor;
                }
            }

            if (nodeButton != null)
                nodeButton.interactable = isAvailable && _talent.CurrentPoints != _talent.MaxPoints;
        }
    }
}
