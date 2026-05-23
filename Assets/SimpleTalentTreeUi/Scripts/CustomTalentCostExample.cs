using UnityEngine;

namespace SimpleTalentTreeUi
{
    /// <summary>
    /// Example of how to override talent cost curves using Talent.CustomCostCalculator.
    /// Attach this to a GameObject in the same scene as TalentManager.
    /// </summary>
    public class CustomTalentCostExample : MonoBehaviour
    {
        [Tooltip("Optional explicit manager reference. If null, TalentManager.Instance will be used.")]
        [SerializeField] private TalentManager manager;

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
        /// Called when a TalentTree becomes active. Here we assign custom cost calculators.
        /// </summary>
        private void OnTalentActivated()
        {
            var mgr = ResolvedManager;
            if (mgr == null || mgr.ActiveTree == null)
                return;

            var tree = mgr.ActiveTree;
            if (tree.Talents == null)
                return;

            foreach (var talent in tree.Talents)
            {
                if (talent == null)
                    continue;

                // Example 1: flat cost for a specific talent (by title or ID)
                if (talent.Title == "Cheap Talent")
                {
                    // Always costs 1, no matter the current level.
                    talent.CustomCostCalculator = t => 1;
                }
                // Example 2: exponential cost
                else if (talent.Title == "Expensive Talent")
                {
                    // Cost = InitialCost * 2^(currentPoints)
                    talent.CustomCostCalculator = t =>
                    {
                        int baseCost = Mathf.Max(1, t.InitialCost);
                        int level = Mathf.Max(0, t.CurrentPoints);
                        return baseCost * (int)Mathf.Pow(2f, level);
                    };
                }
                // Example 3: default behavior (no override)
                else
                {
                    // If you do not assign CustomCostCalculator here,
                    // the Talent will use its default linear cost function.
                    talent.CustomCostCalculator = null;
                }
            }
        }
    }
}
