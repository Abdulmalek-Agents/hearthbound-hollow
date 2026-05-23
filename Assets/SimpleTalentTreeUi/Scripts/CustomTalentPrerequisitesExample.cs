using UnityEngine;

namespace SimpleTalentTreeUi
{
    /// <summary>
    /// Example of how to override talent prerequisite rules using TalentTree.CustomPrerequisiteEvaluator.
    /// Attach this to a GameObject in the same scene as TalentManager.
    /// </summary>
    public class CustomTalentPrerequisitesExample : MonoBehaviour
    {
        [Header("Manager")]
        [Tooltip("Optional explicit manager reference. If null, TalentManager.Instance will be used.")]
        [SerializeField] private TalentManager manager;

        [Header("Prerequisite mode")]
        [Tooltip("If true, a talent becomes available when ANY prerequisite is met. If false, ALL prerequisites must be met.")]
        [SerializeField] private bool useAnyPrerequisite = true;

        private TalentManager ResolvedManager => manager != null ? manager : TalentManager.Instance;

        private void OnEnable()
        {
            if (ResolvedManager != null)
            {
                ResolvedManager.OnTalentActivated += OnTalentActivated;
            }
        }

        private void OnDisable()
        {
            if (ResolvedManager != null)
            {
                ResolvedManager.OnTalentActivated -= OnTalentActivated;
            }
        }

        /// <summary>
        /// Called when a TalentTree becomes active.
        /// Here we assign our custom prerequisite evaluator.
        /// </summary>
        private void OnTalentActivated()
        {
            var mgr = ResolvedManager;
            if (mgr == null || mgr.ActiveTree == null)
                return;

            var tree = mgr.ActiveTree;

            // Assign custom evaluator. The tree will call this whenever it updates availability.
            tree.CustomPrerequisiteEvaluator = EvaluatePrerequisites;

            // Note: availability will be recalculated the next time the tree changes
            // (e.g. when you spend/refund points or load a save).
            // If you want to do an initial refresh manually, you can force a "fake" change
            // or expose a public method in TalentTree to re-run availability checks.
        }

        /// <summary>
        /// Custom prerequisite rule:
        /// - If the talent has no prerequisites: it is always available.
        /// - If useAnyPrerequisite is true:
        ///   The talent becomes available when ANY prerequisite meets its required points.
        /// - If useAnyPrerequisite is false:
        ///   ALL prerequisites must meet their required points.
        /// </summary>
        private bool EvaluatePrerequisites(TalentTree tree, Talent talent)
        {
            // No prerequisites -> always available.
            if (talent.PreviousTalentIds == null || talent.PreviousTalentIds.Count == 0)
                return true;

            if (useAnyPrerequisite)
            {
                // ANY prerequisite can unlock this talent.
                foreach (int prevId in talent.PreviousTalentIds)
                {
                    var prev = tree.FindTalentByID(prevId);
                    if (prev == null)
                        continue;

                    if (prev.CurrentPoints >= prev.MinPointsToAllowNextTalent)
                        return true;
                }

                return false;
            }
            else
            {
                // ALL prerequisites must be met.
                foreach (int prevId in talent.PreviousTalentIds)
                {
                    var prev = tree.FindTalentByID(prevId);
                    if (prev == null)
                        return false;

                    if (prev.CurrentPoints < prev.MinPointsToAllowNextTalent)
                        return false;
                }

                return true;
            }
        }
    }
}
