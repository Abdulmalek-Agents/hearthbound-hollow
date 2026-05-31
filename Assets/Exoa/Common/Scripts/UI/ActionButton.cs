
using Exoa.Events;
using UnityEngine;
using UnityEngine.UI;

namespace Exoa.Designer.UI
{
    /// <summary>
    /// Represents a button that triggers a specific action when clicked.
    /// </summary>
    public class ActionButton : MonoBehaviour
    {
        /// <summary>
        /// The action to be performed when the button is clicked.
        /// </summary>
        public CameraEvents.Action action;
        private Button btn;

        /// <summary>
        /// Initializes the button and adds a listener for the click event.
        /// </summary>
        void Start()
        {
            btn = GetComponent<Button>();
            btn.onClick.AddListener(OnButtonClicked);
        }

        /// <summary>
        /// Invoked when the button is clicked; triggers the associated action.
        /// </summary>
        private void OnButtonClicked()
        {
            CameraEvents.OnRequestButtonAction?.Invoke(action, true);
        }
    }
}
