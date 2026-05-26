using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SimpleTalentTreeUi
{
    /// <summary>
    /// ScriptableObject representing a single node in the talent tree editor graph.
    /// Holds both gameplay-related data (title, cost, max points, etc.)
    /// and editor graph data (rect, styles, connection points).
    /// </summary>
    public class TalentTreeNodeSO : ScriptableObject
    {
        [Header("Node Settings")]

        /// <summary>
        /// Display title for this talent (used in UI and editor).
        /// </summary>
        public string title;

        /// <summary>
        /// Description of the talent, used in tooltips or other UI.
        /// </summary>
        [TextArea]
        public string description;

        /// <summary>
        /// Maximum points that can be invested in this talent.
        /// </summary>
        public int maxPoints;

        /// <summary>
        /// Base cost of the first point.
        /// </summary>
        public int cost;

        /// <summary>
        /// Extra cost added per already invested point.
        /// </summary>
        public int costIncrementFactor;

        /// <summary>
        /// Minimum points required in this talent to unlock subsequent talents.
        /// </summary>
        public int minPointsToAllowNextTalent;

        /// <summary>
        /// Which point pool (Global, Fire, Dark, etc.) this talent consumes.
        /// </summary>
        public TalentPointType pointType = TalentPointType.Global;

        /// <summary>
        /// Rect used by the editor graph to position this node.
        /// </summary>
        public Rect rect;

        /// <summary>
        /// Main image used for this node (will become a Sprite in runtime).
        /// </summary>
        public Texture2D image;

        /// <summary>
        /// Talent tree asset that owns this node.
        /// </summary>
        public TalentTreeSO talentTreeSO;

        /// <summary>
        /// Previous nodes in the tree that act as prerequisites.
        /// </summary>
        public List<TalentTreeNodeSO> previousNodes;

#if UNITY_EDITOR
        [Header("Graph controlled variables")]

        /// <summary>
        /// Rect used to draw a background/decoration image for the node.
        /// </summary>
        public Rect backgroundImageRect;

        /// <summary>
        /// Rect used to draw the node's main image.
        /// </summary>
        public Rect imageRect;

        /// <summary>
        /// Connection point used as input (prerequisite).
        /// </summary>
        public TalentTreeConnectionPointSO inPoint;

        /// <summary>
        /// Connection point used as output (children).
        /// </summary>
        public TalentTreeConnectionPointSO outPoint;

        /// <summary>
        /// Current style used to draw the node.
        /// </summary>
        public GUIStyle style;

        /// <summary>
        /// Default node style when the node is not selected.
        /// </summary>
        public GUIStyle defaultNodeStyle;

        /// <summary>
        /// Style used when the node is selected.
        /// </summary>
        public GUIStyle selectedNodeStyle;

        /// <summary>
        /// Invoked when the user chooses to remove this node from the graph.
        /// </summary>
        public Action<TalentTreeNodeSO> OnRemoveNode;

        /// <summary>
        /// Invoked when the user selects this node in the graph.
        /// </summary>
        public Action<TalentTreeNodeSO> OnSelectNode;

        /// <summary>
        /// True while the node is being dragged by the mouse.
        /// </summary>
        public bool isDragged;

        /// <summary>
        /// True when the node is selected in the graph.
        /// </summary>
        public bool isSelected;
#endif

        /// <summary>
        /// Converts the node's Texture2D image into a Sprite for runtime use.
        /// </summary>
        public Sprite GetNodeImageAsSprite()
        {
            if (image == null)
                return null;

            return Sprite.Create(
                image,
                new Rect(0, 0, image.width, image.height),
                new Vector2(0.5f, 0.5f));
        }

#if UNITY_EDITOR
        /// <summary>
        /// Initializes this node with default gameplay values and editor graph data.
        /// Called by the talent tree editor when creating a new node.
        /// </summary>
        public void InitializeNode(
            Vector2 position,
            float width,
            float height,
            GUIStyle nodeStyle,
            GUIStyle selectedStyle,
            GUIStyle inPointStyle,
            GUIStyle outPointStyle,
            Action<TalentTreeConnectionPointSO> onClickInPoint,
            Action<TalentTreeConnectionPointSO> onClickOutPoint,
            Action<TalentTreeNodeSO> onClickRemoveNode,
            Action<TalentTreeNodeSO> onClickSelectNode)
        {
            // Default gameplay values for a new talent.
            title = "New Talent";
            description = string.Empty;
            maxPoints = 1;
            cost = 1;
            costIncrementFactor = 1;
            minPointsToAllowNextTalent = 1;

            rect = new Rect(position.x, position.y, width, height);

            style = nodeStyle;
            defaultNodeStyle = nodeStyle;
            selectedNodeStyle = selectedStyle;

            OnRemoveNode = onClickRemoveNode;
            OnSelectNode = onClickSelectNode;

            // Create connection points as sub-assets of the TalentTreeSO.
            inPoint = CreateConnectionPoint(this, ConnectionPointType.In, inPointStyle, onClickInPoint);
            outPoint = CreateConnectionPoint(this, ConnectionPointType.Out, outPointStyle, onClickOutPoint);

            // Default image used for new nodes (optional).
            image = Resources.Load("emptyImage") as Texture2D;

            if (previousNodes == null)
                previousNodes = new List<TalentTreeNodeSO>();
            else
                previousNodes.Clear();

            EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// Moves this node by the given delta in the graph space.
        /// </summary>
        public void Drag(Vector2 delta)
        {
            rect.position += delta;
        }

        /// <summary>
        /// Draws the node and its connection points in the editor graph.
        /// </summary>
        public void Draw()
        {
            if (inPoint != null)
                inPoint.Draw();

            if (outPoint != null)
                outPoint.Draw();

            if (style == null)
                style = defaultNodeStyle ?? GUI.skin.box;

            GUI.Box(rect, title, style);

            // Center the image in the node.
            const float imageSize = 64f;
            imageRect = new Rect(
                rect.x + ((rect.width - imageSize) * 0.5f),
                rect.y + ((rect.height - imageSize) * 0.5f),
                imageSize,
                imageSize);

            if (image != null)
            {
                GUI.DrawTexture(imageRect, image, ScaleMode.ScaleAndCrop, true, 0f);
            }
        }

        /// <summary>
        /// Handles mouse events for dragging and context menu.
        /// Returns true if the event was consumed.
        /// </summary>
        public bool ProcessEvents(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0)
                    {
                        // If you want to re-enable selection on click, you can
                        // uncomment/adapt this block. It was previously commented out
                        // to avoid interfering with selection logic in the editor window.

                        // if (rect.Contains(e.mousePosition))
                        // {
                        //     isDragged = true;
                        //     GUI.changed = true;
                        //     isSelected = true;
                        //     style = selectedNodeStyle;
                        //     OnSelectNode?.Invoke(this);
                        // }
                        // else
                        // {
                        //     GUI.changed = true;
                        //     isSelected = false;
                        //     style = defaultNodeStyle;
                        //     OnSelectNode?.Invoke(null);
                        // }
                    }

                    if (e.button == 1 && isSelected && rect.Contains(e.mousePosition))
                    {
                        ProcessContextMenu();
                        e.Use();
                    }
                    break;

                case EventType.MouseUp:
                    isDragged = false;
                    break;

                case EventType.MouseDrag:
                    if (e.button == 0 && isDragged)
                    {
                        Drag(e.delta);
                        e.Use();
                        return true;
                    }
                    break;
            }

            return false;
        }

        /// <summary>
        /// Deletes the connection points sub-assets from the TalentTree asset.
        /// </summary>
        public void DeleteConnectionPoints()
        {
            if (inPoint != null)
            {
                GameObject.DestroyImmediate(inPoint, true);
                inPoint = null;
            }

            if (outPoint != null)
            {
                GameObject.DestroyImmediate(outPoint, true);
                outPoint = null;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Shows the node context menu at the current mouse position.
        /// </summary>
        private void ProcessContextMenu()
        {
            var genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Remove node"), false, OnClickRemoveNode);
            genericMenu.ShowAsContext();
        }

        /// <summary>
        /// Called from the context menu to remove this node from the graph.
        /// </summary>
        private void OnClickRemoveNode()
        {
            OnRemoveNode?.Invoke(this);
        }

        /// <summary>
        /// Creates a new connection point ScriptableObject and adds it as a sub-asset
        /// of the owning TalentTreeSO.
        /// </summary>
        private TalentTreeConnectionPointSO CreateConnectionPoint(
            TalentTreeNodeSO node,
            ConnectionPointType type,
            GUIStyle style,
            Action<TalentTreeConnectionPointSO> onClickConnectionPoint)
        {
            var connectionPoint = ScriptableObject.CreateInstance<TalentTreeConnectionPointSO>();

            connectionPoint.Initialize(node, type, style, onClickConnectionPoint);

            if (talentTreeSO != null)
            {
                AssetDatabase.AddObjectToAsset(connectionPoint, talentTreeSO);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.SetDirty(talentTreeSO);
            }

            return connectionPoint;
        }
#endif // UNITY_EDITOR
    }
}
