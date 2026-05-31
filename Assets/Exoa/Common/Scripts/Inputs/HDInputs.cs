
using Exoa.Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Exoa.Designer
{
    /// <summary>
    /// Handles input management for the HD (High Definition) environment.
    /// This class defines key mappings and checks for specific input events.
    /// </summary>
    public class HDInputs : MonoBehaviour
    {
        /// <summary>
        /// Gets a value indicating whether the Control key is currently pressed.
        /// </summary>
        public static bool ControlKey => (Event.current != null && Event.current.control && Event.current.type == EventType.KeyDown);

        /// <summary>
        /// Gets a value indicating whether the pointer is currently over a UI element.
        /// </summary>
        public static bool IsOverUI => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        [Header("Key Map")]
        public static KeyCode resetCamera = KeyCode.F;               // Key to reset the camera
        public static KeyCode save = KeyCode.S;                      // Key to save the current state
        public static KeyCode switchPerspective = KeyCode.Space;     // Key to switch camera perspective
        public static KeyCode openSaveFolder = KeyCode.D;            // Key to open the save folder
        public static KeyCode toggleGizmos = KeyCode.G;              // Key to toggle gizmos visibility
        public static KeyCode toggleRoofs = KeyCode.R;               // Key to toggle roof visibility
        public static KeyCode toggleExteriorWalls = KeyCode.E;       // Key to toggle exterior walls visibility
        public static KeyCode escape = KeyCode.Escape;               // Key to trigger escape action
        public static KeyCode leftAlt = KeyCode.LeftAlt;             // Key for Alt actions

        /// <summary>
        /// Checks if the reset camera key is pressed, not over UI.
        /// </summary>
        /// <returns>True if the reset camera key is pressed; otherwise false.</returns>
        public static bool ResetCamera()
        {
            return BaseTouchInput.GetKeyWentDown(resetCamera) && !IsOverUI;
        }

        /// <summary>
        /// Checks if the save key is pressed with Control key held down.
        /// </summary>
        /// <returns>True if the save key is pressed with Control; otherwise false.</returns>
        public static bool SavePressed()
        {
            return BaseTouchInput.GetKeyWentDown(save) && ControlKey;
        }

        /// <summary>
        /// Checks if the open save folder key is pressed with Control key held down.
        /// </summary>
        /// <returns>True if the open save folder key is pressed with Control; otherwise false.</returns>
        public static bool OpenSaveFolderPressed()
        {
            return BaseTouchInput.GetKeyWentDown(openSaveFolder) && ControlKey;
        }

        /// <summary>
        /// Checks if the key to change plan mode is pressed and not over UI.
        /// </summary>
        /// <returns>True if the change plan mode key is pressed; otherwise false.</returns>
        public static bool ChangePlanMode()
        {
            return BaseTouchInput.GetKeyWentDown(switchPerspective) && !IsOverUI;
        }

        /// <summary>
        /// Checks if the toggle gizmo key is pressed and not over UI.
        /// </summary>
        /// <returns>True if the toggle gizmos key is pressed; otherwise false.</returns>
        public static bool ToggleGizmo()
        {
            return BaseTouchInput.GetKeyWentDown(toggleGizmos) && !IsOverUI;
        }

        /// <summary>
        /// Checks if the toggle exterior walls key is pressed and not over UI.
        /// </summary>
        /// <returns>True if the toggle exterior walls key is pressed; otherwise false.</returns>
        public static bool ToggleExteriorWalls()
        {
            return BaseTouchInput.GetKeyWentDown(toggleExteriorWalls) && !IsOverUI;
        }

        /// <summary>
        /// Checks if the toggle roof key is pressed and not over UI.
        /// </summary>
        /// <returns>True if the toggle roofs key is pressed; otherwise false.</returns>
        public static bool ToggleRoof()
        {
            return BaseTouchInput.GetKeyWentDown(toggleRoofs) && !IsOverUI;
        }

        /// <summary>
        /// Checks if the mouse drag has been released.
        /// </summary>
        /// <returns>True if mouse button 0 is released; otherwise false.</returns>
        public static bool ReleaseDrag()
        {
            return BaseTouchInput.GetMouseWentUp(0);
        }

        /// <summary>
        /// Checks if the Option (right mouse button) is pressed.
        /// </summary>
        /// <returns>True if the right mouse button is pressed; otherwise false.</returns>
        public static bool OptionPress()
        {
            return BaseTouchInput.GetMouseWentDown(1);
        }

        /// <summary>
        /// Checks if the escape key is pressed.
        /// </summary>
        /// <returns>True if the escape key is pressed; otherwise false.</returns>
        public static bool EscapePressed()
        {
            return BaseTouchInput.GetKeyWentDown(escape);
        }

        /// <summary>
        /// Checks if the left Alt key is currently held down.
        /// </summary>
        /// <returns>True if the left Alt key is held; otherwise false.</returns>
        public static bool AltPressed()
        {
            return BaseTouchInput.GetKeyIsHeld(leftAlt);
        }
    }
}
