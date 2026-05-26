using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace SimpleTalentTreeUi
{
    public class TalentTreeEditorWindow : EditorWindow
    {
        private GUIStyle nodeStyle;
        private GUIStyle selectedNodeStyle;
        private GUIStyle inPointStyle;
        private GUIStyle outPointStyle;
        private GUIStyle toolbarStyle;
        private GUIStyle inspectorStyle;
        private GUIStyle talentTreeNameStyle;
        private GUISkin defaultSkin;
        private GUIContent renameButtonContent;

        private TalentTreeConnectionPointSO selectedInPoint;
        private TalentTreeConnectionPointSO selectedOutPoint;

        private Vector2 offset;
        private Vector2 drag;
        private Rect topBarRect;
        private Rect inspectorRect;
        private bool isRenaming;
        private string currentFileName;
        private string tempFileName;
        private string currentFilePath;

        private TalentTreeSO currentTree = null;
        private TalentTreeNodeSO selectedNode = null;

        [MenuItem("Tools/Simple Talent Tree Ui/Talent Node Editor New")]
        private static void OpenWindow()
        {
            TalentTreeEditorWindow window = GetWindow<TalentTreeEditorWindow>();
            window.titleContent = new GUIContent("Talent Tree Node Editor");
        }

        private void OnEnable()
        {
            renameButtonContent = new GUIContent((Texture2D)Resources.Load("writeImage"), "Rename Tree");

            defaultSkin = Resources.Load("GUISkins/TalentTreeSkin") as GUISkin;

            talentTreeNameStyle = defaultSkin.GetStyle("TreeName");

            nodeStyle = defaultSkin.GetStyle("Node");
            nodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node0.png") as Texture2D;
            nodeStyle.border = new RectOffset(12, 12, 12, 12);

            selectedNodeStyle = defaultSkin.GetStyle("NodeSelected");
            selectedNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node0 on.png") as Texture2D;
            selectedNodeStyle.border = new RectOffset(12, 12, 12, 12);

            inPointStyle = new GUIStyle();
            inPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left.png") as Texture2D;
            inPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left on.png") as Texture2D;
            inPointStyle.border = new RectOffset(4, 4, 12, 12);

            outPointStyle = new GUIStyle();
            outPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right.png") as Texture2D;
            outPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right on.png") as Texture2D;
            outPointStyle.border = new RectOffset(4, 4, 12, 12);

            toolbarStyle = new GUIStyle();
            toolbarStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/pb-topbarbg.png") as Texture2D;
            toolbarStyle.fixedHeight = 80f;

            inspectorStyle = new GUIStyle();
            inspectorStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/objectpickerbackground.png") as Texture2D;
        }

        private void OnGUI()
        {
            DrawGrid(20, 0.2f, Color.gray);
            DrawGrid(100, 0.4f, Color.gray);

            DrawGraphArea();

            DrawConnectionLine(Event.current);

            DrawToolbar();
            DrawInspector();

            ProcessNodeEvents(Event.current);
            ProcessEvents(Event.current);

            if (GUI.changed == false)
                return;

            if (selectedNode != null)
            {
                EditorUtility.SetDirty(selectedNode);
            }

            EditorUtility.SetDirty(this);
            Repaint();
        }

        private void DrawGraphArea()
        {
            if (currentTree == null)
                return;

            GUI.BeginGroup(new Rect(currentTree.graphX, currentTree.graphY, 999999, 999999));

            DrawNodes();
            DrawConnections();

            GUI.EndGroup();
        }

        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal(toolbarStyle);
            GUILayout.EndHorizontal();

            DrawRenameButtons();

            GUILayout.BeginArea(new Rect((position.width / 2) - 200f, 0f, 300f, 60f));

            if (currentTree != null)
            {
                if (isRenaming == false)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(currentTree.name, talentTreeNameStyle);
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    tempFileName = EditorGUILayout.TextField(tempFileName, GUILayout.Width(200), GUILayout.Height(32));
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
                GUILayout.Label("No tree selected...", talentTreeNameStyle);

            GUILayout.BeginHorizontal();

            GUI.enabled = currentTree == null;

            if (GUILayout.Button("Create New Tree"))
            {
                if (currentTree == null)
                    CreateTree();
            }

            if (GUILayout.Button("Load Tree"))
            {
                if (currentTree == null)
                    LoadTree();
            }

            GUI.enabled = currentTree != null;

            if (GUILayout.Button("Unload Tree"))
            {
                if (currentTree != null)
                    UnloadTree();
            }

            GUI.enabled = true;

            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            DrawCreateTreeButtonToolbar();

            topBarRect = new Rect(0, 0, 1 * maxSize.x, 60f);

        }

        private void DrawCreateTreeButtonToolbar()
        {
            GUILayout.BeginArea(new Rect(position.width - 150, 6f, 146f, 64f));
            if (GUILayout.Button("Talent Tree UI Creation", GUILayout.Width(146), GUILayout.Height(64)))
            {
                TalentTreeUiCreationWindow.Open();
            }
            GUILayout.EndArea();
        }

        private void DrawRenameButtons()
        {
            if (currentTree == null)
                return;

            if (isRenaming == false)
            {
                GUILayout.BeginArea(new Rect((position.width / 2) + (currentTree.name.Length * 1.5f + 60), 10f, 30f, 30f));
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(renameButtonContent, GUILayout.Width(22), GUILayout.Height(22)))
                {
                    isRenaming = true;
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.EndArea();
            }
            else
            {
                GUILayout.BeginArea(new Rect((position.width / 2), 14f, 300f, 60f));
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Cancel", GUILayout.Width(64)))
                {
                    tempFileName = currentFileName;
                    isRenaming = false;
                    EditorGUIUtility.editingTextField = false;
                }

                if (GUILayout.Button("Confirm", GUILayout.Width(64)))
                {
                    if (string.IsNullOrEmpty(tempFileName))
                    {
                        tempFileName = currentFileName;
                        isRenaming = false;
                    }

                    if (string.IsNullOrEmpty(AssetDatabase.RenameAsset(currentFilePath, tempFileName)) == true)
                    {
                        //   AssetDatabase.Refresh();

                        currentFilePath = $"Assets/SimpleTalentTreeUi/ScriptableObjects/TalentTreeSOs/{tempFileName}.asset";
                        currentFileName = tempFileName;
                        currentTree.talentTreeName = tempFileName;
                        tempFileName = currentTree.talentTreeName;
                    }
                    else
                    {
                        tempFileName = currentFileName;
                        EditorGUIUtility.editingTextField = false;
                    }

                    isRenaming = false;
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.EndArea();
            }
        }

        private void DrawInspector()
        {
            inspectorRect = new Rect(position.width - 250f, 80f, 250f, position.height);

            GUILayout.BeginArea(inspectorRect, inspectorStyle);
            GUILayout.Label("Inspector");

            if (selectedNode == null)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("No node selected...");
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginScrollView(Vector2.zero, GUI.skin.box);
                EditorGUILayout.LabelField("Name: ", GUILayout.Width(144));
                EditorGUILayout.BeginHorizontal();
                selectedNode.title = EditorGUILayout.TextField(selectedNode.title);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField("Description: ", GUILayout.Width(144));
                EditorGUILayout.BeginHorizontal();
                selectedNode.description = EditorGUILayout.TextArea(selectedNode.description, GUILayout.Height(64));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField("Maximum Points: ", GUILayout.Width(144));
                EditorGUILayout.BeginHorizontal();
                selectedNode.maxPoints = EditorGUILayout.IntField(selectedNode.maxPoints);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField("Cost per point: ", GUILayout.Width(144));
                EditorGUILayout.BeginHorizontal();
                selectedNode.cost = EditorGUILayout.IntField(selectedNode.cost);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField("Cost increment per level: ", GUILayout.Width(144));
                EditorGUILayout.BeginHorizontal();
                selectedNode.costIncrementFactor = EditorGUILayout.IntField(selectedNode.costIncrementFactor);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField("Min points to allow next talent: ", GUILayout.Width(180));
                EditorGUILayout.BeginHorizontal();
                selectedNode.minPointsToAllowNextTalent = EditorGUILayout.IntField(selectedNode.minPointsToAllowNextTalent);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField("Point Type: ", GUILayout.Width(144));
                EditorGUILayout.BeginHorizontal();
                selectedNode.pointType = (TalentPointType)EditorGUILayout.EnumPopup(selectedNode.pointType);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField("Icon image: ", GUILayout.Width(144));
                EditorGUILayout.BeginHorizontal();
                selectedNode.image = (Texture2D)EditorGUILayout.ObjectField(selectedNode.image, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
                EditorGUILayout.EndHorizontal();
                GUILayout.EndScrollView();
            }
            GUILayout.EndArea();
        }

        private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
        {
            int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
            int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

            offset += drag * 0.5f;
            Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

            for (int i = 0; i < widthDivs; i++)
            {
                Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
            }

            for (int j = 0; j < heightDivs; j++)
            {
                Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
            }

            Handles.color = Color.white;
            Handles.EndGUI();
        }

        private void DrawNodes()
        {
            if (currentTree == null)
                return;

            if (currentTree.talentNodes != null)
            {
                for (int i = 0; i < currentTree.talentNodes.Count; i++)
                {
                    currentTree.talentNodes[i].Draw();
                }
            }
        }

        private void DrawConnections()
        {
            if (currentTree == null)
                return;

            if (currentTree.connections != null)
            {
                for (int i = 0; i < currentTree.connections.Count; i++)
                {
                    currentTree.connections[i].Draw();
                }
            }
        }

        private void ProcessEvents(Event e)
        {
            if (currentTree == null)
                return;

            drag = Vector2.zero;

            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0 && !topBarRect.Contains(e.mousePosition) && !inspectorRect.Contains(e.mousePosition))
                    {
                        ClearConnectionSelection();

                        bool nodeFound = false;
                        for (int i = 0; i < currentTree.talentNodes.Count; i++)
                        {
                            if (currentTree.talentNodes[i].rect.Contains(new Vector2(e.mousePosition.x - currentTree.graphX, e.mousePosition.y - currentTree.graphY)))
                            {
                                GUI.changed = true;
                                currentTree.talentNodes[i].isDragged = true;
                                currentTree.talentNodes[i].isSelected = true;
                                currentTree.talentNodes[i].style = selectedNodeStyle;
                                selectedNode = currentTree.talentNodes[i];
                                nodeFound = true;
                            }
                            else
                            {
                                GUI.changed = true;
                                currentTree.talentNodes[i].isSelected = false;
                                currentTree.talentNodes[i].style = nodeStyle;
                            }

                            if (!nodeFound)
                            {
                                selectedNode = null;
                            }
                        }
                    }

                    if (e.button == 1 && !topBarRect.Contains(e.mousePosition) && !inspectorRect.Contains(e.mousePosition))
                    {
                        ProcessContextMenu(e.mousePosition);
                    }
                    break;

                case EventType.MouseDrag:
                    if (e.button == 0 && !topBarRect.Contains(e.mousePosition) && !inspectorRect.Contains(e.mousePosition))
                    {
                        OnDrag(e.delta);
                    }
                    break;
            }
        }

        private void ProcessNodeEvents(Event e)
        {
            if (currentTree == null)
                return;

            if (currentTree.talentNodes != null)
            {
                for (int i = currentTree.talentNodes.Count - 1; i >= 0; i--)
                {
                    bool guiChanged = currentTree.talentNodes[i].ProcessEvents(e);

                    if (guiChanged)
                    {
                        GUI.changed = true;
                    }
                }
            }
        }

        private void DrawConnectionLine(Event e)
        {
            if (selectedInPoint != null && selectedOutPoint == null)
            {
                Handles.DrawBezier(
                    new Vector2(selectedInPoint.rect.center.x + currentTree.graphX, selectedInPoint.rect.center.y + currentTree.graphY),
                    e.mousePosition,
                    new Vector2(selectedInPoint.rect.center.x + currentTree.graphX, selectedInPoint.rect.center.y + currentTree.graphY) + Vector2.left * 50f,
                    e.mousePosition - Vector2.left * 50f,
                    Color.white,
                    null,
                    2f
                );

                GUI.changed = true;
            }

            if (selectedOutPoint != null && selectedInPoint == null)
            {
                Handles.DrawBezier(
                    new Vector2(selectedOutPoint.rect.center.x + currentTree.graphX, selectedOutPoint.rect.center.y + currentTree.graphY),
                    e.mousePosition,
                    new Vector2(selectedOutPoint.rect.center.x + currentTree.graphX, selectedOutPoint.rect.center.y + currentTree.graphY) - Vector2.left * 50f,
                    e.mousePosition + Vector2.left * 50f,
                    Color.white,
                    null,
                    2f
                );

                GUI.changed = true;
            }
        }

        private void ProcessContextMenu(Vector2 mousePosition)
        {
            GenericMenu genericMenu = new GenericMenu();

            // Verifica se um node foi clicado
            TalentTreeNodeSO clickedNode = GetNodeAtPosition(mousePosition);
            if (clickedNode != null)
            {
                // Adiciona a opção de remover o node no menu de contexto
                genericMenu.AddItem(new GUIContent("Remove node"), false, () => OnClickRemoveNode(clickedNode));
            }
            else
            {
                // Adiciona a opção de adicionar um novo node se não houver um node no local clicado
                genericMenu.AddItem(new GUIContent("Add node"), false, () => OnClickAddNode(mousePosition));
            }

            genericMenu.ShowAsContext();
        }

        private TalentTreeNodeSO GetNodeAtPosition(Vector2 mousePosition)
        {
            if (currentTree == null || currentTree.talentNodes == null)
                return null;

            for (int i = 0; i < currentTree.talentNodes.Count; i++)
            {
                // Verifica se a posição do mouse está dentro do retângulo de um node
                if (currentTree.talentNodes[i].rect.Contains(new Vector2(mousePosition.x - currentTree.graphX, mousePosition.y - currentTree.graphY)))
                {
                    return currentTree.talentNodes[i];
                }
            }
            return null;
        }


        private void OnDrag(Vector2 delta)
        {
            drag = delta;

            currentTree.graphX += delta.x;
            currentTree.graphY += delta.y;

            if (currentTree.graphX > 0)
            {
                currentTree.graphX = 0;
                drag = new Vector2(0, drag.y);
            }

            if (currentTree.graphY > 0)
            {
                currentTree.graphY = 0;
                drag = new Vector2(drag.x, 0);
            }

            GUI.changed = true;
        }

        private void OnClickAddNode(Vector2 mousePosition)
        {
            if (currentTree.talentNodes == null)
                return;

            selectedNode = (TalentTreeNodeSO)ScriptableObject.CreateInstance<TalentTreeNodeSO>();

            if (selectedNode == null)
                return;

            selectedNode.talentTreeSO = currentTree;

            selectedNode.InitializeNode(new Vector2(mousePosition.x - currentTree.graphX, mousePosition.y - currentTree.graphY), 100, 150, nodeStyle, selectedNodeStyle, inPointStyle, outPointStyle,
                OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, OnClickSelectNode);

            currentTree.talentNodes.Add(selectedNode);
            AssetDatabase.AddObjectToAsset(selectedNode, currentTree);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

        }

        private void OnClickSelectNode(TalentTreeNodeSO selectedNode)
        {
            this.selectedNode = selectedNode == null ? null : selectedNode;
        }

        private void OnClickInPoint(TalentTreeConnectionPointSO inPoint)
        {
            selectedInPoint = inPoint;

            if (selectedOutPoint != null)
            {
                if (selectedOutPoint.node != selectedInPoint.node)
                {
                    CreateConnection();
                    ClearConnectionSelection();
                }
                else
                {
                    ClearConnectionSelection();
                }
            }
        }

        private void OnClickOutPoint(TalentTreeConnectionPointSO outPoint)
        {
            selectedOutPoint = outPoint;

            if (selectedInPoint != null)
            {
                if (selectedOutPoint.node != selectedInPoint.node)
                {
                    CreateConnection();
                    ClearConnectionSelection();
                }
                else
                {
                    ClearConnectionSelection();
                }
            }
        }

        private void OnClickRemoveNode(TalentTreeNodeSO node)
        {
            if (currentTree.connections != null)
            {
                List<TalentTreeConnectionSO> connectionsToRemove = new List<TalentTreeConnectionSO>();

                for (int i = 0; i < currentTree.connections.Count; i++)
                {
                    if (currentTree.connections[i].inPoint == node.inPoint || currentTree.connections[i].outPoint == node.outPoint)
                    {
                        connectionsToRemove.Add(currentTree.connections[i]);
                    }
                }

                for (int i = 0; i < connectionsToRemove.Count; i++)
                {
                    currentTree.connections.Remove(connectionsToRemove[i]);

                    GameObject.DestroyImmediate(connectionsToRemove[i], true);
                }

                connectionsToRemove = null;
            }

            currentTree.talentNodes.Remove(node);

            node.DeleteConnectionPoints();

            GameObject.DestroyImmediate(node, true);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void OnClickRemoveConnection(TalentTreeConnectionSO connection)
        {
            connection.inPoint.node.previousNodes.Remove(connection.outPoint.node);

            currentTree.connections.Remove(connection);

            GameObject.DestroyImmediate(connection, true);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void CreateConnection()
        {
            if (currentTree.connections == null)
            {
                currentTree.connections = new List<TalentTreeConnectionSO>();
            }

            TalentTreeConnectionSO newConnectionSO = (TalentTreeConnectionSO)ScriptableObject.CreateInstance<TalentTreeConnectionSO>();

            newConnectionSO.Initialize(selectedInPoint, selectedOutPoint, OnClickRemoveConnection);

            selectedInPoint.node.previousNodes.Add(selectedOutPoint.node);

            currentTree.connections.Add(newConnectionSO);

            AssetDatabase.AddObjectToAsset(newConnectionSO, currentTree);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.SetDirty(currentTree);
        }

        private void ClearConnectionSelection()
        {
            selectedInPoint = null;
            selectedOutPoint = null;
        }

        private void CreateTree()
        {
            currentTree = (TalentTreeSO)ScriptableObject.CreateInstance<TalentTreeSO>();
            currentTree.talentTreeName = "New Tree";
            currentTree.Initialize();

            if (File.Exists($"Assets/SimpleTalentTreeUi/ScriptableObjects/TalentTreeSOs/{currentTree.talentTreeName}.asset"))
            {
                int fileNumber = 1;
                while (File.Exists($"Assets/SimpleTalentTreeUi/ScriptableObjects/TalentTreeSOs/New Tree {fileNumber}.asset"))
                {
                    fileNumber++;
                }

                currentTree.talentTreeName = $"New Tree {fileNumber}";
            }

            currentFileName = currentTree.talentTreeName;
            tempFileName = currentFileName;
            currentFilePath = $"Assets/SimpleTalentTreeUi/ScriptableObjects/TalentTreeSOs/{currentTree.talentTreeName}.asset";

            AssetDatabase.CreateAsset(currentTree, $"Assets/SimpleTalentTreeUi/ScriptableObjects/TalentTreeSOs/{currentTree.talentTreeName}.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void LoadTree()
        {
            string treePath = EditorUtility.OpenFilePanel("Load Tree", Application.dataPath + "Assets/SimpleTalentTreeUi/ScriptableObjects/TalentTreeSOs/", "");

            if (string.IsNullOrEmpty(treePath) == true)
                return;

            int appPathLength = Application.dataPath.Length;

            string finalPath = treePath.Substring(appPathLength - 6);

            var loadedTree = (TalentTreeSO)AssetDatabase.LoadAssetAtPath(finalPath, typeof(TalentTreeSO));

            if (loadedTree != null)
            {
                var fileName = Path.GetFileName(treePath);

                currentFileName = fileName.Substring(0, fileName.Length - 6);
                tempFileName = currentFileName;
                currentFilePath = $"Assets/SimpleTalentTreeUi/ScriptableObjects/TalentTreeSOs/{currentFileName}.asset";

                TalentTreeEditorWindow currentWindow = (TalentTreeEditorWindow)EditorWindow.GetWindow<TalentTreeEditorWindow>();
                if (currentWindow != null)
                {
                    currentWindow.currentTree = loadedTree;
                }

                foreach (var node in loadedTree.talentNodes)
                {
                    node.inPoint.OnClickConnectionPoint = OnClickInPoint;
                    node.outPoint.OnClickConnectionPoint = OnClickOutPoint;
                    node.OnRemoveNode = OnClickRemoveNode;
                }

                foreach (var connections in loadedTree.connections)
                {
                    connections.OnClickRemoveConnection = OnClickRemoveConnection;
                }

            }
        }

        private void UnloadTree()
        {
            currentTree = null;
            selectedNode = null;
        }
    }
}