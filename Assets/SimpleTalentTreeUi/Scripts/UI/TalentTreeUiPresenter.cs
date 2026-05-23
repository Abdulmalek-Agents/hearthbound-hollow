using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleTalentTreeUi
{
    /// <summary>
    /// Simple presenter for a talent tree UI with Apply/Cancel buttons
    /// and text fields showing available points per point type.
    /// </summary>
    public class TalentTreeUiPresenter : MonoBehaviour
    {
        [Header("Manager")]
        [Tooltip("Optional explicit manager reference. If null, TalentManager.Instance will be used.")]
        [SerializeField] private TalentManager manager;

        [Header("References")]
        [SerializeField] private Button applyButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TMP_Text talentPointsText;

        [Header("Per Point Type Totals (optional)")]
        [SerializeField] private PointTypeTextBinding[] pointTypeTexts;

        [Serializable]
        public struct PointTypeTextBinding
        {
            public TalentPointType type;
            public TMP_Text text;
        }

        private TalentTree _activeTree;

        private TalentManager ResolvedManager => manager != null ? manager : TalentManager.Instance;

        private void OnEnable()
        {
            var resolvedManager = ResolvedManager;
            if (resolvedManager != null)
            {
                resolvedManager.OnTalentActivated += OnTalentTreeActivated;
            }
            else
            {
                Debug.LogWarning("[SimpleTalentTreeUI] TalentTreeUiPresenter enabled but no TalentManager is available.", this);
            }
        }

        private void OnDisable()
        {
            var resolvedManager = ResolvedManager;
            if (resolvedManager != null)
            {
                resolvedManager.OnTalentActivated -= OnTalentTreeActivated;
                resolvedManager.OnLoadTalentFinished -= OnTalentTreeLoaded;
            }

            if (_activeTree != null)
            {
                _activeTree.OnEditingStateChanged -= OnTalentChange;
                _activeTree.OnTalentPointsChanged -= OnTalentPointsChanged;
                _activeTree.OnPointPoolChanged -= OnPointPoolChanged;
            }

            if (applyButton != null)
                applyButton.onClick.RemoveListener(OnApplyButtonClick);
            if (cancelButton != null)
                cancelButton.onClick.RemoveListener(OnCancelButtonClick);
        }

        private void OnTalentTreeActivated()
        {
            var resolvedManager = ResolvedManager;
            if (resolvedManager == null)
            {
                Debug.LogError("[SimpleTalentTreeUI] OnTalentTreeActivated fired but no TalentManager is available.", this);
                return;
            }

            _activeTree = resolvedManager.ActiveTree;

            if (_activeTree == null)
            {
                Debug.LogError("[SimpleTalentTreeUI] TalentTreeUiPresenter received activation but ActiveTree is null.", this);
                return;
            }

            _activeTree.OnEditingStateChanged -= OnTalentChange;
            _activeTree.OnTalentPointsChanged -= OnTalentPointsChanged;
            _activeTree.OnPointPoolChanged -= OnPointPoolChanged;

            _activeTree.OnEditingStateChanged += OnTalentChange;
            _activeTree.OnTalentPointsChanged += OnTalentPointsChanged;
            _activeTree.OnPointPoolChanged += OnPointPoolChanged;

            resolvedManager.OnLoadTalentFinished -= OnTalentTreeLoaded;
            resolvedManager.OnLoadTalentFinished += OnTalentTreeLoaded;

            Initialize();
        }

        private void OnTalentTreeLoaded()
        {
            if (_activeTree != null && talentPointsText != null)
            {
                talentPointsText.text = _activeTree.TalentPoints.ToString();
                RefreshAllPointTypeTexts();
            }
        }

        /// <summary>
        /// Initializes button listeners and refreshes point labels.
        /// </summary>
        public void Initialize()
        {
            var resolvedManager = ResolvedManager;
            if (resolvedManager == null || _activeTree == null)
            {
                Debug.LogError("[SimpleTalentTreeUI] TalentTreeUiPresenter.Initialize called without a valid TalentManager or ActiveTree.", this);
                return;
            }

            if (talentPointsText != null)
                talentPointsText.text = _activeTree.TalentPoints.ToString();

            if (applyButton != null)
            {
                applyButton.onClick.RemoveListener(OnApplyButtonClick);
                applyButton.onClick.AddListener(OnApplyButtonClick);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveListener(OnCancelButtonClick);
                cancelButton.onClick.AddListener(OnCancelButtonClick);
            }

            RefreshAllPointTypeTexts();
        }

        private void RefreshAllPointTypeTexts()
        {
            if (pointTypeTexts == null || _activeTree == null)
                return;

            for (int i = 0; i < pointTypeTexts.Length; i++)
            {
                var binding = pointTypeTexts[i];
                if (binding.text == null) continue;

                int value = _activeTree.GetPoints(binding.type);
                binding.text.SetText(value.ToString());
            }
        }

        /// <summary>
        /// Adds points to the global pool on the active tree. Intended for debug / example usage.
        /// </summary>
        public void AddPoints(int value)
        {
            if (_activeTree == null) return;

            _activeTree.AddPoints(value);
            RefreshAllPointTypeTexts();
        }

        /// <summary>
        /// Adds points to the specified pool on the active tree. Intended for debug / example usage.
        /// </summary>
        public void AddPoints(TalentPointType type, int value)
        {
            if (_activeTree == null) return;

            _activeTree.AddPoints(type, value);
            RefreshAllPointTypeTexts();
        }

        /// <summary>
        /// Example wrapper for adding Dark points. Kept for backwards compatibility.
        /// </summary>
        [Obsolete("Use AddPoints(TalentPointType.Dark, value) instead.")]
        public void AddDarkPoints(int value)
        {
            AddPoints(TalentPointType.Dark, value);
        }

        /// <summary>
        /// Example wrapper for adding Fire points. Kept for backwards compatibility.
        /// </summary>
        [Obsolete("Use AddPoints(TalentPointType.Fire, value) instead.")]
        public void AddFirePoints(int value)
        {
            AddPoints(TalentPointType.Fire, value);
        }

        private void OnApplyButtonClick()
        {
            if (_activeTree == null) return;
            _activeTree.ApplyTalent();
        }

        private void OnCancelButtonClick()
        {
            if (_activeTree == null) return;
            _activeTree.CancelApply();
        }

        private void OnTalentChange(bool isEditing)
        {
            if (cancelButton != null)
                cancelButton.interactable = isEditing;
            if (applyButton != null)
                applyButton.interactable = isEditing;
        }

        private void OnTalentPointsChanged(int talentPoints)
        {
            if (talentPointsText != null)
                talentPointsText.SetText(talentPoints.ToString());
        }

        private void OnPointPoolChanged(TalentPointType type, int _)
        {
            RefreshAllPointTypeTexts();
        }
    }
}
