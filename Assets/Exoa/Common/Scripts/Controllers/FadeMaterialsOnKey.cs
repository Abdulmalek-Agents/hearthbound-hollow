
using Exoa.Designer;
using Exoa.Events;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Exoa.Designer
{
    /// <summary>
    /// This class is responsible for fading materials based on user input. 
    /// It controls the display of gizmos in the Unity Editor based on app states and user actions.
    /// </summary>
    public class FadeMaterialsOnKey : MonoBehaviour
    {
        /// <summary>
        /// A list of materials that will be faded when gizmos are displayed or hidden.
        /// </summary>
        public List<Material> gizmos;

        /// <summary>
        /// A flag indicating whether the gizmos are currently displayed.
        /// </summary>
        private bool areGizmosDisplayed = true;

        /// <summary>
        /// Unsubscribes from events when the MonoBehaviour is destroyed.
        /// </summary>
        void OnDestroy()
        {
            GameEditorEvents.OnRequestButtonAction -= OnRequestButtonAction;
            AppController.OnAppStateChange -= OnAppStateChange;
        }

        /// <summary>
        /// Subscribe to events when the MonoBehaviour starts. 
        /// Initializes gizmo display state.
        /// </summary>
        void Start()
        {
            GameEditorEvents.OnRequestButtonAction += OnRequestButtonAction;
            AppController.OnAppStateChange += OnAppStateChange;
            ShowGizmos(true);
        }

        /// <summary>
        /// Handles changes in app state, specifically showing gizmos when in 'Draw' state.
        /// </summary>
        /// <param name="state">The current state of the app.</param>
        private void OnAppStateChange(AppController.States state)
        {
            if (state == AppController.States.Draw)
            {
                ShowGizmos(true);
            }
        }

        /// <summary>
        /// Sets the alpha value of a given material.
        /// </summary>
        /// <param name="m">The material to modify.</param>
        /// <param name="alpha">The new alpha value to set.</param>
        /// <returns>The modified material.</returns>
        private Material SetAlpha(Material m, float alpha)
        {
            Color c = m.color;
            c.a = alpha;
            m.color = c;
            return m;
        }

        /// <summary>
        /// Handles button actions received from the GameEditorEvents.
        /// Toggles the display of gizmos if the corresponding action is invoked.
        /// </summary>
        /// <param name="action">The action that was requested.</param>
        /// <param name="active">Indicates whether the action is active.</param>
        private void OnRequestButtonAction(GameEditorEvents.Action action, bool active)
        {
            if (action == GameEditorEvents.Action.ToggleGizmos)
                ToggleGizmos();
        }

        /// <summary>
        /// Updates the state of gizmos based on user inputs during each frame.
        /// </summary>
        void Update()
        {
            if (HDInputs.ToggleGizmo())
            {
                ToggleGizmos();
            }
        }

        /// <summary>
        /// Toggles the display state of gizmos.
        /// </summary>
        private void ToggleGizmos()
        {
            areGizmosDisplayed = !areGizmosDisplayed;
            ShowGizmos(areGizmosDisplayed);
        }

        /// <summary>
        /// Sets the visibility of gizmos by adjusting their alpha values.
        /// </summary>
        /// <param name="show">If true, gizmos will be shown; otherwise, they will be hidden.</param>
        private void ShowGizmos(bool show)
        {
            areGizmosDisplayed = show;
            if (gizmos != null)
            {
                foreach (var gizmo in gizmos)
                {
                    if (gizmo != null) gizmo.color = ChangeAlpha(gizmo.color, (areGizmosDisplayed ? .8f : 0f));
                }
            }
        }

        /// <summary>
        /// Changes the alpha value of a given color.
        /// </summary>
        /// <param name="color">The original color.</param>
        /// <param name="v">The new alpha value to set.</param>
        /// <returns>The color with modified alpha.</returns>
        private Color ChangeAlpha(Color color, float v)
        {
            color.a = v;
            return color;
        }
    }
}
