using System;
using UnityEngine;

namespace SimpleTalentTreeUi
{
    /// <summary>
    /// Direction of a node connection point within the graph.
    /// </summary>
    public enum ConnectionPointType
    {
        In,
        Out
    }

    /// <summary>
    /// ScriptableObject representing a single connection point on a node
    /// (either an input or output).
    /// </summary>
    public class TalentTreeConnectionPointSO : ScriptableObject
    {
        /// <summary>
        /// Rect used by the editor to position the clickable area.
        /// </summary>
        public Rect rect;

        /// <summary>
        /// Whether this connection point is an input or an output.
        /// </summary>
        public ConnectionPointType type;

        /// <summary>
        /// Node that owns this connection point.
        /// </summary>
        public TalentTreeNodeSO node;

#if UNITY_EDITOR
        /// <summary>
        /// GUIStyle used to draw the button for this connection point.
        /// Not serialized, configured at edit-time only.
        /// </summary>
        [NonSerialized] public GUIStyle style;

        /// <summary>
        /// Invoked when the user clicks this connection point in the editor.
        /// </summary>
        [NonSerialized] public Action<TalentTreeConnectionPointSO> OnClickConnectionPoint;
#endif

        /// <summary>
        /// Initializes this connection point with the given node and type.
        /// Editor-only parameters (style, click handler) are only used inside the editor.
        /// </summary>
        public void Initialize(
            TalentTreeNodeSO node,
            ConnectionPointType type,
#if UNITY_EDITOR
            GUIStyle style,
#endif
            Action<TalentTreeConnectionPointSO> onClickConnectionPoint)
        {
            this.node = node;
            this.type = type;

#if UNITY_EDITOR
            this.style = style;
            this.OnClickConnectionPoint = onClickConnectionPoint;
#endif

            // Default connection point rect size.
            rect = new Rect(0f, 0f, 15f, 35f);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Draws the connection point button in the editor and handles click events.
        /// </summary>
        public void Draw()
        {
            if (node == null)
                return;

            // Align vertically to the center of the node rect.
            rect.y = node.rect.y + (node.rect.height * 0.5f) - rect.height * 0.5f;

            // Position horizontally based on type.
            switch (type)
            {
                case ConnectionPointType.In:
                    rect.x = node.rect.x - rect.width + 8f;
                    break;

                case ConnectionPointType.Out:
                    rect.x = node.rect.x + node.rect.width - 8f;
                    break;
            }

            if (style == null)
            {
                // Fallback style if not set.
                style = GUI.skin.button;
            }

            if (GUI.Button(rect, GUIContent.none, style))
            {
                OnClickConnectionPoint?.Invoke(this);
            }
        }
#endif
    }
}
