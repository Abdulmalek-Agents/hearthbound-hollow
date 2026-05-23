#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace LightMapFusion
{
    public class LMFToolbar : Editor
    {
        private static Rect windowRect;
        private static bool isDragging = false;
        private static Vector2 dragStart;
        private static float buttonWidth = 36f; // Ajuste del ancho del botón
        private static float buttonHeight = 26f; // Ajuste de la altura del botón
        private static int buttonCount = 5;
        private static float panelPadding = 2f;
        private static GUIContent[] buttonContents = new GUIContent[6];
        private static GUIStyle windowStyle;
        private static Color backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.0f); // Fondo gris

        private static LightMapFusion tool;
        private static bool isPanelOpen = false;

        private static int tempButtonCount;

        static LMFToolbar()
        {


            // Cargar íconos de luz de la UI de Unity
            buttonContents[0] = new GUIContent("1", EditorGUIUtility.IconContent("d_Lighting").image, "Light Set 1");
            buttonContents[1] = new GUIContent("2", EditorGUIUtility.IconContent("d_Lighting").image, "Light Set 2");
            buttonContents[2] = new GUIContent("3", EditorGUIUtility.IconContent("d_Lighting").image, "Light Set 3");
            buttonContents[3] = new GUIContent("4", EditorGUIUtility.IconContent("d_Lighting").image, "Light Set 4");
            buttonContents[4] = new GUIContent("", EditorGUIUtility.IconContent("d_MoreOptions").image, "Select Tool");
            buttonContents[5] = new GUIContent(EditorGUIUtility.IconContent("CrossIcon").image, "Close");
        }

        [MenuItem("Window/Orly Tools/LightMap Fusion/LMF Toolbar")]
        public static void ShowPanel()
        {
            if (isPanelOpen)
            {
                ClosePanel();
            }
            tempButtonCount = buttonCount;
            SceneView.duringSceneGui += OnSceneGUI;
            var sceneView = SceneView.lastActiveSceneView;
            tool = FindFirstObjectByType<LightMapFusion>();

            buttonCount = tool.toolMode == ToolMode.Two ? 2 : 4;
            float panelWidth = buttonCount * (buttonWidth + panelPadding) + panelPadding + 25f; // Aumento del ancho para el panel adicional
            float panelHeight = buttonHeight + 2 * panelPadding;

            float initialX = (Screen.width - panelWidth) / 2;
            windowRect = new Rect(initialX, 10, panelWidth, panelHeight);

            if (sceneView != null && tool != null)
            {
                sceneView.Repaint();
            }

            isPanelOpen = true;
            windowRect.x = sceneView.position.width / 2 - windowRect.width / 2;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (windowStyle == null)
            {
                windowStyle = new GUIStyle(GUI.skin.window)
                {
                    normal = { background = MakeRoundedTex(200, 100, backgroundColor, 20) },
                    border = new RectOffset(12, 12, 12, 12),
                    padding = new RectOffset(4, 4, 4, 4),
                    //active = { background = MakeRoundedTex(200, 100, backgroundColor, 20) },
                    //focused = { background = MakeRoundedTex(200, 100, backgroundColor, 20) },
                    //hover = { background = MakeRoundedTex(200, 100, backgroundColor, 20) }
                };
            }

            Handles.BeginGUI();

            windowRect.y = sceneView.position.height - windowRect.height - 25f;
            buttonCount = tool.toolMode == ToolMode.Two ? 2 : 4;
            if (tempButtonCount != buttonCount)
            {
                float panelWidth = buttonCount * (buttonWidth + panelPadding) + panelPadding + 25f; // Aumento del ancho para el panel adicional
                windowRect.width = panelWidth;
                tempButtonCount = buttonCount;
            }
            windowRect = GUILayout.Window(123456, windowRect, id =>
            {
                GUILayout.BeginHorizontal();

                // Panel de botones

                for (int i = 0; i < buttonCount; i++)
                {
                    GUIContent buttonContent = buttonContents[i];
                    if (GUILayout.Button(buttonContent, GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight)))
                    {
                        PerformButtonAction(i + 1);
                    }
                }
                GUIContent buttonContent1 = buttonContents[4];
                if (GUILayout.Button(buttonContent1, GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight)))
                {
                    PerformButtonAction(5);
                }

                // Panel adicional para el botón de cerrar (transparente)
                GUILayout.BeginVertical(GUILayout.Width(18));
                GUILayout.FlexibleSpace();

                GUIContent buttonContent2 = buttonContents[5];
                if (GUILayout.Button(buttonContent2, GUILayout.Width(18), GUILayout.Height(18)))

                {
                    ClosePanel();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();

                GUILayout.EndHorizontal();

                GUI.DragWindow();
            }, GUIContent.none, windowStyle);

            Handles.EndGUI();

            HandleDragging(Event.current, sceneView);
        }

        private static void PerformButtonAction(int buttonIndex)
        {
            switch (buttonIndex)
            {
                case 1:
                    tool.ToggleLightSet(0);
                    break;
                case 2:
                    tool.ToggleLightSet(1);
                    break;
                case 3:
                    tool.ToggleLightSet(2);
                    break;
                case 4:
                    tool.ToggleLightSet(3);
                    break;
                case 5:
                    if (tool != null)
                    {
                        Selection.activeGameObject = tool.gameObject;
                    }
                    break;
                default:
                    break;
            }
        }

        private static void HandleDragging(Event e, SceneView sceneView)
        {
            float margin = 10f;
            float sceneViewWidth = sceneView.position.width;

            if (e.type == EventType.MouseDown && e.button == 0 && windowRect.Contains(e.mousePosition))
            {
                isDragging = true;
                dragStart = e.mousePosition;
                e.Use();
            }
            else if (e.type == EventType.MouseDrag && e.button == 0 && isDragging)
            {
                float offsetX = e.mousePosition.x - dragStart.x;
                windowRect.x += offsetX;
                dragStart.x = e.mousePosition.x;

                windowRect.x = Mathf.Clamp(windowRect.x, margin, sceneViewWidth - windowRect.width - margin);

                e.Use();
            }
            else if (e.type == EventType.MouseUp && e.button == 0)
            {
                isDragging = false;
            }
        }

        private static void ClosePanel()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                sceneView.Repaint();
            }

            isPanelOpen = false;
        }

        private static Texture2D MakeRoundedTex(int width, int height, Color col, int radius)
        {
            Texture2D tex = new Texture2D(width, height);
            Color[] pix = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (x < radius && y < radius && Mathf.Pow(x - radius, 2) + Mathf.Pow(y - radius, 2) > Mathf.Pow(radius, 2) ||
                        x < radius && y > height - radius - 1 && Mathf.Pow(x - radius, 2) + Mathf.Pow(y - (height - radius - 1), 2) > Mathf.Pow(radius, 2) ||
                        x > width - radius - 1 && y < radius && Mathf.Pow(x - (width - radius - 1), 2) + Mathf.Pow(y - radius, 2) > Mathf.Pow(radius, 2) ||
                        x > width - radius - 1 && y > height - radius - 1 && Mathf.Pow(x - (width - radius - 1), 2) + Mathf.Pow(y - (height - radius - 1), 2) > Mathf.Pow(radius, 2))
                    {
                        pix[x + y * width] = Color.clear;
                    }
                    else
                    {
                        pix[x + y * width] = col;
                    }
                }
            }
            tex.SetPixels(pix);
            tex.Apply();
            return tex;
        }
    }
}
#endif