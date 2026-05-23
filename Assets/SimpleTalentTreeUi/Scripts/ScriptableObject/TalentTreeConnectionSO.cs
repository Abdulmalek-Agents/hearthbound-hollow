using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SimpleTalentTreeUi
{
    /// <summary>
    /// ScriptableObject representing a connection between two node connection points.
    /// Used by the editor to draw and manage links between nodes.
    /// </summary>
    public class TalentTreeConnectionSO : ScriptableObject
    {
        [Header("Graph controlled variables")]

        /// <summary>
        /// The input connection point of this link.
        /// </summary>
        public TalentTreeConnectionPointSO inPoint;

        /// <summary>
        /// The output connection point of this link.
        /// </summary>
        public TalentTreeConnectionPointSO outPoint;

#if UNITY_EDITOR
        /// <summary>
        /// Invoked when the user clicks the remove-connection button in the editor.
        /// </summary>
        [NonSerialized] public Action<TalentTreeConnectionSO> OnClickRemoveConnection;
#endif

        /// <summary>
        /// Initializes this connection with its input and output points and
        /// an optional remove callback (used in the editor).
        /// </summary>
        public void Initialize(
            TalentTreeConnectionPointSO inPoint,
            TalentTreeConnectionPointSO outPoint,
#if UNITY_EDITOR
            Action<TalentTreeConnectionSO> onClickRemove
#else
            Action<TalentTreeConnectionSO> onClickRemove = null
#endif
        )
        {
            this.inPoint = inPoint;
            this.outPoint = outPoint;

#if UNITY_EDITOR
            this.OnClickRemoveConnection = onClickRemove;
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// Draws the connection between points, including the remove button.
        /// </summary>
        public void Draw()
        {
            if (inPoint == null || outPoint == null)
                return;

            var start = inPoint.rect.center;
            var end = outPoint.rect.center;

            Handles.DrawBezier(
                start,
                end,
                start + Vector2.left * 50f,
                end - Vector2.left * 50f,
                Color.white,
                null,
                2f
            );

            // Middle point of the connection, used for the remove button.
            var midPoint = (start + end) * 0.5f;

            if (Handles.Button(midPoint, Quaternion.identity, 4f, 8f, Handles.RectangleHandleCap))
            {
                OnClickRemoveConnection?.Invoke(this);
            }
        }
#endif
    }
}
