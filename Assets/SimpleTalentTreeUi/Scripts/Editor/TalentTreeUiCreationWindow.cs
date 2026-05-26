using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SimpleTalentTreeUi
{
    public class UiNodePoint
    {
        public Vector2 position;
        public TalentTreeNodeSO talentTreeNodeSO;
        public GameObject uiNode;
        public Talent talent;
    }

    public class TalentTreeUiCreationWindow : EditorWindow
    {
        private TalentTreeSO talentTreeSO;
        private RectTransform talentTreeParentTransform;
        private GameObject uiNodePrefab;
        private GameObject uiLinePrefab;
        private GameObject talentManager;
        private GUIStyle labelTitleUiCreation;
        private Texture2D trashImage;

        private const int numAttemptsBeforeRejection = 30;
        private float spawnDistanceRadius = 80f;
        private Vector2 startPointRange = Vector2.one * 400;
        private int randomSeed;
        private int generatedSeed;
        private bool clearUiBeforeRandomGen = false;
        private bool clearUiBeforeRegularGen = false;
        private bool addToolTip = false;
        private bool showRegularGenerationSection = false;
        private bool showRandomGenerationSection = false;
        private Color disabledColor = Color.gray;
        private Color enabledColor = new Color(0.92f, 0.65f, 1f, 1);
        private float nodeScale = 1f;
        private bool groupInitialNodes = false;
        private bool separateChildrenByRootDirection = false;

        private List<UiNodePoint> nodePoints = new List<UiNodePoint>();

        [MenuItem("Tools/Simple Talent Tree Ui/Talent Tree Ui Creation")]
        private static void OpenWindow()
        {
            Open();
        }

        public static void Open()
        {
            TalentTreeUiCreationWindow window = GetWindow<TalentTreeUiCreationWindow>();
            window.titleContent = new GUIContent("Talent Tree Ui Creation");
            window.minSize = new Vector2(600, 800);
            window.maxSize = new Vector2(600, 800);
            window.Show();
        }

        private void OnEnable()
        {
            labelTitleUiCreation = (Resources.Load("GUISkins/TalentTreeSkin") as GUISkin).GetStyle("TitleUiCreation");
            trashImage = Resources.Load("trashImage") as Texture2D;
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(5, 6f, 128f, 64));
            if (GUILayout.Button("Open Tree Editor"))
            {
                TalentTreeEditorWindow window = GetWindow<TalentTreeEditorWindow>();
                window.titleContent = new GUIContent("Talent Tree Node Editor");
            }
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(position.width - 32, 6f, 64f, 64));
            if (GUILayout.Button(new GUIContent(trashImage, "Clear Ui objects inside Parent Trasnform reference"), GUILayout.Width(32), GUILayout.Height(32)))
            {
                if (talentTreeParentTransform != null)
                    ShowConfirmationDialog();
            }
            GUILayout.EndArea();

            GUILayout.Space(10);

            GUILayout.Label("References Settings", labelTitleUiCreation);

            GUILayout.Space(10);

            EditorGUILayout.LabelField("Talent Tree: ", GUILayout.Width(128));
            talentTreeSO = (TalentTreeSO)EditorGUILayout.ObjectField(talentTreeSO, typeof(TalentTreeSO), false);

            EditorGUILayout.LabelField("Ui Parent Transform: ", GUILayout.Width(128));
            talentTreeParentTransform = (RectTransform)EditorGUILayout.ObjectField(talentTreeParentTransform, typeof(RectTransform), true);

            EditorGUILayout.LabelField("Ui Node Prefab: ", GUILayout.Width(128));
            uiNodePrefab = (GameObject)EditorGUILayout.ObjectField(uiNodePrefab, typeof(GameObject), true);

            EditorGUILayout.LabelField("Ui Line Prefab: ", GUILayout.Width(128));
            uiLinePrefab = (GameObject)EditorGUILayout.ObjectField(uiLinePrefab, typeof(GameObject), true);

            EditorGUILayout.LabelField("Talent Manager (optional): ", GUILayout.Width(152));
            talentManager = (GameObject)EditorGUILayout.ObjectField(talentManager, typeof(GameObject), true);

            EditorGUILayout.LabelField("Add ToolTip to Ui: ", GUILayout.Width(200));
            addToolTip = EditorGUILayout.Toggle(addToolTip);

            EditorGUILayout.LabelField("Line/Border disabled color: ", GUILayout.Width(200));
            disabledColor = EditorGUILayout.ColorField(disabledColor);

            EditorGUILayout.LabelField("Line/Border enabled color: ", GUILayout.Width(200));
            enabledColor = EditorGUILayout.ColorField(enabledColor);

            GUILayout.Space(5);

            EditorGUILayout.LabelField("Node Scale: ", GUILayout.Width(200));
            float newScale = EditorGUILayout.Slider(nodeScale, 0.5f, 1.5f);

            if (!Mathf.Approximately(newScale, nodeScale))
            {
                nodeScale = newScale;
                ApplyNodeScale();
            }

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal((GUIStyle)"HelpBox");
            showRegularGenerationSection = EditorGUILayout.BeginFoldoutHeaderGroup(showRegularGenerationSection, "Regular Ui Generation");
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.EndHorizontal();

            if (showRegularGenerationSection == true)
            {
                EditorGUILayout.LabelField("Clear UI Before Regular Generation: ", GUILayout.Width(200));
                clearUiBeforeRegularGen = EditorGUILayout.Toggle(clearUiBeforeRegularGen);

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Generate Tree Ui", GUILayout.Height(40)))
                {
                    if (clearUiBeforeRegularGen == true)
                        ClearUi();

                    GenerateTreeUi();
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal((GUIStyle)"HelpBox");
            showRandomGenerationSection = EditorGUILayout.BeginFoldoutHeaderGroup(showRandomGenerationSection, "Random Ui Generation");
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.EndHorizontal();

            if (showRandomGenerationSection == true)
            {
                EditorGUILayout.LabelField("Spawn Distance Radius: ", GUILayout.Width(160));
                spawnDistanceRadius = EditorGUILayout.FloatField(spawnDistanceRadius);

                EditorGUILayout.LabelField("Start Point Range: ", GUILayout.Width(128));
                startPointRange = EditorGUILayout.Vector2Field("", startPointRange);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Random Seed: ", GUILayout.Width(128));
                randomSeed = EditorGUILayout.IntField(randomSeed);

                EditorGUILayout.LabelField("Clear UI Before Random Generation: ", GUILayout.Width(200));
                clearUiBeforeRandomGen = EditorGUILayout.Toggle(clearUiBeforeRandomGen);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField("Multi type tree options: ", GUILayout.Width(160));
                EditorGUILayout.BeginHorizontal((GUIStyle)"HelpBox");               

                EditorGUILayout.LabelField("Group initial nodes: ", GUILayout.Width(150));
                groupInitialNodes = EditorGUILayout.Toggle(groupInitialNodes);

                EditorGUILayout.LabelField("Opposite child nodes per root: ", GUILayout.Width(200));
                separateChildrenByRootDirection = EditorGUILayout.Toggle(separateChildrenByRootDirection);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField("Generated Seed: ", GUILayout.Width(128));
                generatedSeed = EditorGUILayout.IntField(generatedSeed);

                GUILayout.Space(20);

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Generate Random Tree Ui", GUILayout.Height(40)))
                {
                    if (clearUiBeforeRandomGen == true)
                        ClearUi();

                    GenerateRandomTree();
                }
                GUILayout.EndHorizontal();
            }
        }

        private void ShowConfirmationDialog()
        {
            bool result = EditorUtility.DisplayDialog("Confirmation", "All Objects inside Parent Transform will be deleted.\n\n Are you sure you want to proceed?", "Yes", "No");

            if (result)
                ClearUi();
        }

        private void ClearUi()
        {
            if (talentTreeParentTransform == null)
            {
                Debug.Log("Parent transform reference missing");
                return;
            }

            int childCount = talentTreeParentTransform.transform.childCount;

            for (int i = childCount - 1; i >= 0; i--)
            {
                Transform child = talentTreeParentTransform.transform.GetChild(i);
                DestroyImmediate(child.gameObject);
            }
        }

        private void GenerateTreeUi()
        {
            nodePoints.Clear();

            TalentTree newTalentTree = null;

            if (talentManager != null)
            {
                var talentManagerComp = talentManager.GetComponent<TalentManager>();
                if (talentManagerComp == null)
                {
                    Debug.LogError("Talent Manager Script not found in gameobject");
                    return;
                }

                newTalentTree = talentManagerComp.InitializeNewTreeFromEditor(talentTreeSO.ID);
            }

            Talent newTalent = null;

            var canvasRoot = talentTreeParentTransform.root.GetComponent<Canvas>();

            int talentId = 0;
            foreach (var node in talentTreeSO.talentNodes)
            {
                var newNodeUi = Instantiate(uiNodePrefab);

                RectTransform nodeRect = newNodeUi.GetComponent<RectTransform>();

                nodeRect.anchoredPosition = Vector3.zero;
                nodeRect.SetParent(talentTreeParentTransform, true);
                nodeRect.anchoredPosition = new Vector2(node.rect.x, -node.rect.y);
                nodeRect.localScale = Vector3.one; 

                if (newTalentTree != null)
                {
                    newTalent = newTalentTree.CreateTalent(talentId, node);
                    talentId++;
                }

                bool isAvailable = true;
                foreach (var previousNode in node.previousNodes)
                {
                    if (previousNode != null && previousNode.minPointsToAllowNextTalent > 0)
                    {
                        isAvailable = false;
                        break;
                    }
                }

                if (newTalentTree != null)
                {
                    newTalent.WasAvailable = isAvailable;
                    newTalent.IsAvailable = isAvailable;
                }

                SetNodeUiInfo(newNodeUi, node, talentTreeSO.ID, newTalent?.Id ?? -1, isAvailable);

                nodePoints.Add(new UiNodePoint()
                {
                    talent = newTalent,
                    uiNode = newNodeUi,
                    talentTreeNodeSO = node
                });
            }

            foreach (var connection in talentTreeSO.connections)
            {
                var inPointNode = nodePoints.Find(x => x.talentTreeNodeSO == connection.inPoint.node);
                if (inPointNode == null) continue;

                var outPointNode = nodePoints.Find(x => x.talentTreeNodeSO == connection.outPoint.node);
                if (outPointNode == null) continue;

                if (newTalentTree != null)
                {
                    inPointNode.talent.SetPreviousTalentId(outPointNode.talent.Id);
                }

                var pointsVector3 = Handles.MakeBezierPoints(
                    new Vector3(connection.inPoint.rect.position.x + 10, -connection.inPoint.rect.position.y, 0),
                    new Vector3(connection.outPoint.rect.position.x - 10, -connection.outPoint.rect.position.y, 0),
                    new Vector3(connection.inPoint.rect.position.x, -connection.inPoint.rect.position.y, 0) + Vector3.left * 50f,
                    new Vector3(connection.outPoint.rect.position.x, -connection.outPoint.rect.position.y, 0) - Vector3.left * 50f,
                    10);

                var highQualityBezierPoints = Handles.MakeBezierPoints(
                    new Vector3(connection.inPoint.rect.position.x + 10, -connection.inPoint.rect.position.y, 0),
                    new Vector3(connection.outPoint.rect.position.x - 10, -connection.outPoint.rect.position.y, 0),
                    new Vector3(connection.inPoint.rect.position.x, -connection.inPoint.rect.position.y, 0) + Vector3.left * 50f,
                    new Vector3(connection.outPoint.rect.position.x, -connection.outPoint.rect.position.y, 0) - Vector3.left * 50f,
                    30);

                Vector2[] vector2Array = new Vector2[pointsVector3.Length];
                for (int i = 0; i < pointsVector3.Length; i++)
                {
                    vector2Array[i] = new Vector2(pointsVector3[i].x, pointsVector3[i].y);
                }

                var newConnection = Instantiate(uiLinePrefab, talentTreeParentTransform);

                var uilineRenderer = newConnection.GetComponent<LineRendererUI>();
                if (uilineRenderer != null)
                {
                    uilineRenderer.color = inPointNode?.talent?.IsAvailable == false ? disabledColor : enabledColor;
                    uilineRenderer.Points = vector2Array;
                    uilineRenderer.SetVerticesDirty();

                    var talentNodeUi = inPointNode.uiNode.GetComponent<TalentNodeUi>();
                    if (talentNodeUi != null)
                    {
                        talentNodeUi.AddUiLine(uilineRenderer);
                    }
                }
                else
                {
                    var lineRendererUnity = newConnection.GetComponent<LineRenderer>();
                    if (lineRendererUnity != null)
                    {
                        lineRendererUnity.startColor = inPointNode?.talent?.IsAvailable == false ? disabledColor : enabledColor;
                        lineRendererUnity.endColor = lineRendererUnity.startColor;

                        if (lineRendererUnity.sharedMaterial != null)
                        {
                            Material materialInstance = new Material(lineRendererUnity.sharedMaterial);
                            materialInstance.color = lineRendererUnity.startColor;
                            lineRendererUnity.material = materialInstance;

                            lineRendererUnity.positionCount = highQualityBezierPoints.Length;
                            lineRendererUnity.SetPositions(highQualityBezierPoints);
                        }
                        else
                        {
                            lineRendererUnity.positionCount = pointsVector3.Length;
                            lineRendererUnity.SetPositions(pointsVector3);
                        }

                        var talentNodeUi = inPointNode.uiNode.GetComponent<TalentNodeUi>();
                        if (talentNodeUi != null)
                        {
                            talentNodeUi.AddLineRenderer(lineRendererUnity);
                        }
                    }
                }

                newConnection.transform.SetAsFirstSibling();
            }
        }

        private void SetNodeUiInfo(GameObject nodeUi, TalentTreeNodeSO talentNodeSO, string talentTreeID, int talentID, bool isAvailable = false)
        {
            var talentUi = nodeUi.GetComponent<TalentNodeUi>();
            if (talentUi != null)
            {
                if (talentUi.title != null)
                    talentUi.title.text = talentNodeSO.title;

                if (talentUi.iconImage != null)
                    talentUi.iconImage.sprite = talentNodeSO.GetNodeImageAsSprite();

                if (talentUi.talentPoints != null)
                    talentUi.talentPoints.text = $"{0}/{talentNodeSO.maxPoints}";

                talentUi.disabledColor = disabledColor;
                talentUi.enabledColor = enabledColor;

                talentUi.borderImage.color = isAvailable == true ? enabledColor : disabledColor;
                talentUi.nodeButton.interactable = isAvailable;
            }

            if (addToolTip == true)
            {
                var toolTip = nodeUi.AddComponent<ToolTipInfo>();
                toolTip.mainText = talentNodeSO.description;
                toolTip.costText = $"Cost: {talentNodeSO.cost}";
            }

            talentUi.Initialize(talentTreeID, talentID);
        }

        private void GenerateRandomTree()
        {
            if (randomSeed == 0)
                generatedSeed = UnityEngine.Random.Range(-9999999, 9999999);
            else
                generatedSeed = randomSeed;

            UnityEngine.Random.InitState(generatedSeed);

            nodePoints.Clear();

            TalentTree newTalentTree = null;
            if (talentManager != null)
            {
                var talentManagerComp = talentManager.GetComponent<TalentManager>();
                if (talentManagerComp == null)
                {
                    Debug.LogError("Talent Manager Script not found in gameobject");
                    return;
                }
                newTalentTree = talentManagerComp.InitializeNewTreeFromEditor(talentTreeSO.ID);
            }

            int talentId = 0;
            Talent newTalent = null;
            List<UiNodePoint> spawnedNodes = new List<UiNodePoint>();

            var sortedTalentNodes = GetSortedNodesWithAllDependencies(talentTreeSO.talentNodes);

            List<TalentTreeNodeSO> rootNodes = new List<TalentTreeNodeSO>();
            foreach (var node in sortedTalentNodes)
            {
                if (node.previousNodes == null || node.previousNodes.Count == 0)
                    rootNodes.Add(node);
            }

            Dictionary<TalentTreeNodeSO, Vector2> rootPositions = null;
            Dictionary<TalentTreeNodeSO, Vector2> rootDirections = null;

            if ((groupInitialNodes || separateChildrenByRootDirection) && rootNodes.Count > 0)
            {
                ComputeRootLayout(rootNodes, out rootPositions, out rootDirections);
            }

            Dictionary<TalentTreeNodeSO, TalentTreeNodeSO> nodeToRoot =
                new Dictionary<TalentTreeNodeSO, TalentTreeNodeSO>();

            foreach (var node in sortedTalentNodes)
            {
                if (node.previousNodes == null || node.previousNodes.Count == 0)
                {
                    nodeToRoot[node] = node;
                }
                else
                {
                    var parent = node.previousNodes[0];
                    if (nodeToRoot.TryGetValue(parent, out var root))
                        nodeToRoot[node] = root;
                    else
                        nodeToRoot[node] = parent;
                }
            }

            foreach (var node in sortedTalentNodes)
            {
                if (spawnedNodes.Exists(x => x.talentTreeNodeSO == node))
                    continue;

                if (newTalentTree != null)
                {
                    newTalent = newTalentTree.CreateTalent(talentId, node);
                    talentId++;
                }

                var newNodeUi = Instantiate(uiNodePrefab, talentTreeParentTransform);
                var newNodeRectTransform = newNodeUi.GetComponent<RectTransform>();

                Vector2 position;

                if (node.previousNodes == null || node.previousNodes.Count == 0)
                {
                    if (groupInitialNodes && rootPositions != null &&
                        rootPositions.TryGetValue(node, out var rootPos))
                    {
                        position = rootPos;
                    }
                    else
                    {
                        position = GetRandomInitialPosition(startPointRange);
                    }
                }
                else
                {
                    var previousNodePoint = spawnedNodes.Find(x => x.talentTreeNodeSO == node.previousNodes[0]);

                    if (previousNodePoint == null)
                    {
                        if (groupInitialNodes && rootPositions != null &&
                            rootPositions.TryGetValue(node, out var rootPos))
                            position = rootPos;
                        else
                            position = GetRandomInitialPosition(startPointRange);
                    }
                    else
                    {
                        Vector2? preferredDir = null;

                        if (separateChildrenByRootDirection &&
                            rootDirections != null &&
                            nodeToRoot.TryGetValue(node, out var rootNode) &&
                            rootDirections.TryGetValue(rootNode, out var rootDir))
                        {
                            preferredDir = rootDir;
                        }

                        position = GetNextAvailablePosition(previousNodePoint.position, spawnedNodes, preferredDir);
                    }
                }

                newNodeRectTransform.anchoredPosition = position;
                newNodeRectTransform.localScale = Vector3.one;

                var newPoint = new UiNodePoint()
                {
                    position = newNodeRectTransform.anchoredPosition,
                    talentTreeNodeSO = node,
                    uiNode = newNodeUi,
                    talent = newTalent
                };

                nodePoints.Add(newPoint);
                spawnedNodes.Add(newPoint);

                bool isAvailable = true;
                foreach (var previousNode in node.previousNodes)
                {
                    if (previousNode != null && previousNode.minPointsToAllowNextTalent > 0)
                    {
                        isAvailable = false;
                        break;
                    }
                }

                if (newTalentTree != null)
                {
                    newTalent.WasAvailable = isAvailable;
                    newTalent.IsAvailable = isAvailable;
                }

                SetNodeUiInfo(newNodeUi, node, talentTreeSO.ID, newTalent?.Id ?? -1, isAvailable);
            }

            CreateConnections(spawnedNodes);
        }

        private List<TalentTreeNodeSO> GetSortedNodesWithAllDependencies(List<TalentTreeNodeSO> talentNodes)
        {
            var sortedNodes = new List<TalentTreeNodeSO>();
            var visited = new HashSet<TalentTreeNodeSO>();

            foreach (var node in talentNodes)
            {
                VisitNodeRecursively(node, visited, sortedNodes);
            }

            return sortedNodes;
        }

        private void VisitNodeRecursively(TalentTreeNodeSO node, HashSet<TalentTreeNodeSO> visited, List<TalentTreeNodeSO> sortedNodes)
        {
            if (visited.Contains(node))
                return;

            visited.Add(node);

            foreach (var previousNode in node.previousNodes)
            {
                if (previousNode != null)
                {
                    VisitNodeRecursively(previousNode, visited, sortedNodes);
                }
            }

            if (!sortedNodes.Contains(node))
            {
                sortedNodes.Add(node);
            }
        }

        private Vector2 GetRandomInitialPosition(Vector2 regionSize)
        {
            float x = UnityEngine.Random.Range(-regionSize.x / 2, regionSize.x / 2);
            float y = UnityEngine.Random.Range(-regionSize.y / 2, regionSize.y / 2);
            return new Vector2(x, y);
        }

        private Vector2 GetNextAvailablePosition(Vector2 startPosition, List<UiNodePoint> spawnedNodes, Vector2? preferredDirection = null)
        {
            float distance = spawnDistanceRadius * 0.75f;
            Vector2 newPosition;
            bool positionFound = false;

            do
            {
                Vector2 randomDir = UnityEngine.Random.insideUnitCircle.normalized;

                if (preferredDirection.HasValue)
                {
                    Vector2 dir = preferredDirection.Value.normalized;
                    randomDir = (randomDir + dir * 2f).normalized;
                }

                newPosition = startPosition + randomDir * distance;

                positionFound = !IsOverlapping(newPosition, spawnedNodes);
                distance += spawnDistanceRadius * 0.1f;
            }
            while (!positionFound);

            return newPosition;
        }

        private bool IsOverlapping(Vector2 position, List<UiNodePoint> spawnedNodes)
        {
            foreach (var node in spawnedNodes)
            {
                if (Vector2.Distance(position, node.position) < spawnDistanceRadius)
                {
                    return true;
                }
            }
            return false;
        }

        private void CreateConnections(List<UiNodePoint> spawnedNodes)
        {
            foreach (var node in spawnedNodes)
            {
                foreach (var previousNode in node.talentTreeNodeSO.previousNodes)
                {
                    var previousNodePoint = spawnedNodes.Find(x => x.talentTreeNodeSO == previousNode);
                    if (previousNodePoint == null) continue;

                    if (node.talent != null)
                    {
                        node.talent.SetPreviousTalentId(previousNodePoint.talent.Id);
                    }

                    var linePrefab = Instantiate(uiLinePrefab, talentTreeParentTransform);
                    var uilineRenderer = linePrefab.GetComponent<LineRendererUI>();

                    Vector2[] positionPoints = new Vector2[2] { previousNodePoint.position, node.position };

                    if (uilineRenderer != null)
                    {
                        uilineRenderer.color = previousNode.minPointsToAllowNextTalent > 0 ? disabledColor : enabledColor;
                        uilineRenderer.Points = positionPoints;
                        uilineRenderer.SetVerticesDirty();

                        var talentNodeUi = node.uiNode.GetComponent<TalentNodeUi>();
                        if (talentNodeUi != null)
                        {
                            talentNodeUi.AddUiLine(uilineRenderer);
                        }
                    }
                    else
                    {
                        var lineRendererUnity = linePrefab.GetComponent<LineRenderer>();
                        if (lineRendererUnity != null)
                        {
                            lineRendererUnity.startColor = previousNode.minPointsToAllowNextTalent > 0 ? disabledColor : enabledColor;
                            lineRendererUnity.endColor = lineRendererUnity.startColor;

                            if (lineRendererUnity.sharedMaterial != null)
                            {
                                MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                                lineRendererUnity.GetPropertyBlock(mpb);
                                mpb.SetColor("_Color", lineRendererUnity.startColor);
                                lineRendererUnity.SetPropertyBlock(mpb);
                            }

                            Vector3[] convertedArray = new Vector3[positionPoints.Length];
                            for (int j = 0; j < positionPoints.Length; j++)
                            {
                                convertedArray[j] = new Vector3(positionPoints[j].x, positionPoints[j].y, 0.0f);
                            }

                            lineRendererUnity.SetPositions(convertedArray);

                            var talentNodeUi = node.uiNode.GetComponent<TalentNodeUi>();
                            if (talentNodeUi != null)
                            {
                                talentNodeUi.AddLineRenderer(lineRendererUnity);
                            }
                        }
                    }

                    linePrefab.transform.SetAsFirstSibling();
                }
            }
        }

        private void ApplyNodeScale()
        {
            if (talentTreeParentTransform == null)
            {
                Debug.Log("Parent transform reference missing");
                return;
            }

            talentTreeParentTransform.localScale = Vector3.one * nodeScale;
            Debug.Log($"Applied node scale {nodeScale} to parent transform.");
        }

        private void ComputeRootLayout(
            List<TalentTreeNodeSO> roots,
            out Dictionary<TalentTreeNodeSO, Vector2> rootPositions,
            out Dictionary<TalentTreeNodeSO, Vector2> rootDirections)
        {
            rootPositions = new Dictionary<TalentTreeNodeSO, Vector2>();
            rootDirections = new Dictionary<TalentTreeNodeSO, Vector2>();

            if (roots == null || roots.Count == 0)
                return;

            var shuffledRoots = new List<TalentTreeNodeSO>(roots);

            for (int i = 0; i < shuffledRoots.Count; i++)
            {
                int swapIndex = UnityEngine.Random.Range(i, shuffledRoots.Count);
                var temp = shuffledRoots[i];
                shuffledRoots[i] = shuffledRoots[swapIndex];
                shuffledRoots[swapIndex] = temp;
            }

            int count = shuffledRoots.Count;

            float range = Mathf.Max(startPointRange.x, startPointRange.y);
            if (range <= 0f) range = 100f;
            float radius = range * 0.25f;


            Vector2 center = Vector2.zero;

            float rotationOffsetDeg = UnityEngine.Random.Range(0f, 360f);

            if (count == 1)
            {
                var onlyRoot = shuffledRoots[0];
                rootPositions[onlyRoot] = center;
                rootDirections[onlyRoot] = Vector2.up;
                return;
            }

            float startAngleDeg;
            float angleStepDeg;

            if (count == 2)
            {
                startAngleDeg = 180f;
                angleStepDeg = 180f;
            }
            else
            {
                startAngleDeg = -90f;
                angleStepDeg = 360f / count;
            }

            for (int i = 0; i < count; i++)
            {
                var node = shuffledRoots[i];

                float angleDeg = rotationOffsetDeg + startAngleDeg + angleStepDeg * i;
                float angleRad = angleDeg * Mathf.Deg2Rad;

                Vector2 dir = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
                if (dir.sqrMagnitude < 0.0001f)
                    dir = Vector2.up;

                float radiusJitter = UnityEngine.Random.Range(-radius * 0.2f, radius * 0.2f);
                float r = radius + radiusJitter;

                Vector2 pos = center + dir * r;

                pos.x += UnityEngine.Random.Range(-10f, 10f);
                pos.y += UnityEngine.Random.Range(-10f, 10f);

                rootPositions[node] = pos;
                rootDirections[node] = dir.normalized;
            }
        }

    }
}
