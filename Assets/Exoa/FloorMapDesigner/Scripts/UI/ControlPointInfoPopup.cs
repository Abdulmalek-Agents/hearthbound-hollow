
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Exoa.Designer
{
    /// <summary>
    /// Small popup displayed when right clicking on a space or control point.
    /// Inherits from the BaseFloatingPopup for basic popup functionality.
    /// </summary>
    public class ControlPointInfoPopup : BaseFloatingPopup
    {
        /// <summary>
        /// Singleton instance of ControlPointInfoPopup.
        /// </summary>
        public static ControlPointInfoPopup Instance;

        [SerializeField] private Button trashBtn;
        [SerializeField] private Button splitBtn;
        [SerializeField] private Button editBtn;

        private ControlPoint currentControlPoint;
        private ControlPointsController currentCPC;

        /// <summary>
        /// Initializes the ControlPointInfoPopup, setting up listeners for button events.
        /// </summary>
        override protected void Awake()
        {
            Instance = this;
            editBtn?.onClick.AddListener(() =>
            {
                if (currentCPC != null)
                    currentCPC.ToggleDrawMode(true);
                Hide();
            });
            splitBtn?.onClick.AddListener(() =>
            {
                if (currentCPC != null)
                    currentCPC.OnClickSplit();
                Hide();
            });
            trashBtn?.onClick.AddListener(() =>
            {
                if (currentControlPoint != null)
                    currentControlPoint.cpc.RemoveControlPoint(currentControlPoint);
                else if (currentCPC != null)
                    currentCPC.DeleteSpace();
                Hide();
            });
            base.Awake();
        }

        /// <summary>
        /// Shows the popup for a specified space.
        /// </summary>
        /// <param name="cpc">The ControlPointsController associated with the space.</param>
        /// <param name="mouseWorld">The world position of the mouse for placement.</param>
        /// <param name="showButtons">Determines if the buttons should be shown.</param>
        public void Show(ControlPointsController cpc, Vector3 mouseWorld, bool showButtons = false)
        {
            currentCPC = cpc;
            currentTarget = null;
            currentTargetPosition = mouseWorld;

            trashBtn?.gameObject.SetActive(showButtons);

            Show();
            Vector2 screenPoint = MovePopup();
        }

        /// <summary>
        /// Shows the popup for a specific control point.
        /// </summary>
        /// <param name="cp">The ControlPoint to display information about.</param>
        /// <param name="showButtons">Determines if the buttons should be shown.</param>
        public void Show(ControlPoint cp, bool showButtons = false)
        {
            currentControlPoint = cp;
            currentTarget = cp.transform;
            currentTargetPosition = Vector3.zero;

            trashBtn?.gameObject.SetActive(showButtons && currentControlPoint != null);
            editBtn?.gameObject.SetActive(showButtons && currentControlPoint != null);

            Show();
            MovePopup();
        }
    }
}
