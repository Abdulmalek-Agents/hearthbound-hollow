// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase51_MemoryWebBuilder
//
// PHASE 51 — One-click installer for the Memory Web overlay.
//
// Drops `_MemoryWebCanvas` on the Bootstrap scene with DontDestroyOnLoad
// so the overlay is available across every gameplay scene. The player
// presses Tab to open the overlay any time during gameplay.
//
// Constructed nodes:
//   _MemoryWebCanvas
//   ├── _Dim                          (full-screen black @ 0.65)
//   ├── _Title (TMP)                  ("the Memory Web")
//   ├── _ConnectionsContainer         (lines drawn behind the cards)
//   ├── _CardsContainer               (memory cards on a circular layout)
//   ├── _ConnectionsCount (TMP)       ("Memories: N · Connections: K")
//   ├── _CloseButton
//   ├── _Tooltip
//   └── _Prefabs                      (in-canvas template subtree)
//        ├── _CardTemplate
//        └── _LineTemplate
//
// Idempotent. Re-running deletes the previously-built canvas.

using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HearthboundHollow.Mission;
using HearthboundHollow.UI;

namespace HearthboundHollow.EditorTools
{
    public static class Phase51_MemoryWebBuilder
    {
        private const string BootstrapScenePath = "Assets/_Project/Scenes/00_Bootstrap.unity";
        private const string CanvasName = "_MemoryWebCanvas";

        private static readonly Color C_Dim       = new Color(0.02f, 0.02f, 0.03f, 0.65f);
        private static readonly Color C_Card      = new Color(0.92f, 0.86f, 0.71f, 0.95f);
        private static readonly Color C_CardEdge  = new Color(0.45f, 0.34f, 0.20f, 1f);
        private static readonly Color C_CardInk   = new Color(0.18f, 0.12f, 0.06f, 1f);
        private static readonly Color C_Line      = new Color(0.85f, 0.55f, 0.18f, 0.65f);
        private static readonly Color C_Title     = new Color(0.97f, 0.92f, 0.80f, 1f);

        [MenuItem("Hearthbound/⚙️ Advanced/🕸️ Phase 51 — Build Memory Web Overlay", priority = 5100)]
        public static void Build()
        {
            if (!System.IO.File.Exists(BootstrapScenePath))
            {
                EditorUtility.DisplayDialog(
                    "Phase 51 — Memory Web",
                    "Bootstrap scene not found at:\n" + BootstrapScenePath +
                    "\n\nRun 🚀 Build Everything first.", "OK");
                return;
            }

            var scene = EditorSceneManager.GetSceneByPath(BootstrapScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            }

            RemoveOld(scene);
            var canvasGO = BuildCanvas();
            SceneManager.MoveGameObjectToScene(canvasGO, scene);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog(
                "Phase 51 — Memory Web",
                "✓ Memory Web overlay installed on Bootstrap scene.\n\n" +
                "Press Tab in any gameplay scene to open. Esc / Tab to close.\n" +
                "Lines between cards show shared 'Echo' facets — hover for the " +
                "facet name.",
                "OK");
        }

        private static void RemoveOld(UnityEngine.SceneManagement.Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == CanvasName) Object.DestroyImmediate(root);
            }
        }

        private static GameObject BuildCanvas()
        {
            var go = new GameObject(CanvasName);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32500;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            go.AddComponent<GraphicRaycaster>();

            var rootGroup = go.AddComponent<CanvasGroup>();
            rootGroup.alpha = 0f;
            rootGroup.blocksRaycasts = true;

            // DontDestroyOnLoad — overlay survives scene loads.
            go.AddComponent<KeepAliveOnLoad>();

            // Dim
            var dim = NewChild(go.transform, "_Dim");
            FullScreen(dim);
            var dimImg = dim.gameObject.AddComponent<Image>();
            dimImg.color = C_Dim;
            dimImg.raycastTarget = true;

            // Title
            var title = NewChild(go.transform, "_Title");
            Center(title, new Vector2(1200, 80));
            title.anchoredPosition = new Vector2(0, 420);
            var titleTmp = title.gameObject.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "the Memory Web";
            titleTmp.color = C_Title;
            titleTmp.fontSize = 48;
            titleTmp.alignment = TextAlignmentOptions.Center;

            // Connections + Cards containers
            var conn = NewChild(go.transform, "_ConnectionsContainer");
            Center(conn, new Vector2(1200, 800));
            conn.anchoredPosition = Vector2.zero;

            var cards = NewChild(go.transform, "_CardsContainer");
            Center(cards, new Vector2(1200, 800));
            cards.anchoredPosition = Vector2.zero;

            // Empty-state hint
            var empty = NewChild(go.transform, "_EmptyState");
            Center(empty, new Vector2(900, 80));
            empty.anchoredPosition = new Vector2(0, 0);
            var emptyTmp = empty.gameObject.AddComponent<TextMeshProUGUI>();
            emptyTmp.text = "No memories yet. Polish your first orb.";
            emptyTmp.color = new Color(0.7f, 0.65f, 0.5f, 1f);
            emptyTmp.fontSize = 30;
            emptyTmp.alignment = TextAlignmentOptions.Center;
            emptyTmp.fontStyle = FontStyles.Italic;

            // Counts bar
            var counts = NewChild(go.transform, "_ConnectionsCount");
            Center(counts, new Vector2(1200, 36));
            counts.anchoredPosition = new Vector2(0, -460);
            var countsTmp = counts.gameObject.AddComponent<TextMeshProUGUI>();
            countsTmp.text = "Memories: 0   ·   Connections: 0";
            countsTmp.color = new Color(0.85f, 0.78f, 0.62f, 1f);
            countsTmp.fontSize = 22;
            countsTmp.alignment = TextAlignmentOptions.Center;

            // Close button
            var close = NewChild(go.transform, "_CloseButton");
            Center(close, new Vector2(220, 60));
            close.anchoredPosition = new Vector2(0, -540);
            var closeImg = close.gameObject.AddComponent<Image>();
            closeImg.color = new Color(0.10f, 0.07f, 0.04f, 0.92f);
            closeImg.sprite = RoundedRectSprite("WebClose", 12);
            closeImg.type = Image.Type.Sliced;
            var closeBtn = close.gameObject.AddComponent<Button>();
            var closeLabel = NewChild(close, "_Label");
            FullStretch(closeLabel);
            var closeTmp = closeLabel.gameObject.AddComponent<TextMeshProUGUI>();
            closeTmp.text = "Close [Tab]";
            closeTmp.color = C_Title;
            closeTmp.fontSize = 24;
            closeTmp.alignment = TextAlignmentOptions.Center;

            // Tooltip
            var tip = NewChild(go.transform, "_Tooltip");
            Center(tip, new Vector2(360, 60));
            tip.anchoredPosition = Vector2.zero;
            var tipImg = tip.gameObject.AddComponent<Image>();
            tipImg.color = new Color(0.10f, 0.07f, 0.04f, 0.95f);
            tipImg.sprite = RoundedRectSprite("WebTooltip", 8);
            tipImg.type = Image.Type.Sliced;
            tipImg.raycastTarget = false;
            var tipLabel = NewChild(tip, "_Label");
            FullStretch(tipLabel);
            tipLabel.offsetMin = new Vector2(10, 6);
            tipLabel.offsetMax = new Vector2(-10, -6);
            var tipTmp = tipLabel.gameObject.AddComponent<TextMeshProUGUI>();
            tipTmp.text = "";
            tipTmp.color = C_Title;
            tipTmp.fontSize = 18;
            tipTmp.alignment = TextAlignmentOptions.MidlineCenter;
            tipTmp.fontStyle = FontStyles.Italic;
            tipTmp.raycastTarget = false;

            // Card & line templates
            var prefabs = NewChild(go.transform, "_Prefabs");
            prefabs.gameObject.SetActive(false);
            var cardTemplate = BuildCardTemplate(prefabs);
            var lineTemplate = BuildLineTemplate(prefabs);

            // Component wire-up
            var web = go.AddComponent<MemoryWebOverlay>();
            web.rootGroup = rootGroup;
            web.cardsContainer = cards;
            web.connectionsContainer = conn;
            web.titleText = titleTmp;
            web.emptyStateText = emptyTmp;
            web.closeButton = closeBtn;
            web.connectionsCountLabel = countsTmp;
            web.tooltipPanel = tip;
            web.tooltipText = tipTmp;
            web.cardPrefab = cardTemplate;
            web.connectionLinePrefab = lineTemplate;

            // Disable root until OpenWeb is called.
            go.SetActive(true);
            rootGroup.gameObject.SetActive(false);

            return go;
        }

        private static GameObject BuildCardTemplate(RectTransform parent)
        {
            var go = new GameObject("_CardTemplate", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(220, 140);
            var img = go.AddComponent<Image>();
            img.color = C_Card;
            img.sprite = RoundedRectSprite("WebCard", 12);
            img.type = Image.Type.Sliced;
            img.raycastTarget = true;

            var label = new GameObject("_Label", typeof(RectTransform));
            label.transform.SetParent(go.transform, false);
            var lrt = (RectTransform)label.transform;
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = new Vector2(10, 10);
            lrt.offsetMax = new Vector2(-10, -10);
            var tmp = label.AddComponent<TextMeshProUGUI>();
            tmp.text = "Memory Title";
            tmp.color = C_CardInk;
            tmp.fontSize = 22;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = true;
            return go;
        }

        private static GameObject BuildLineTemplate(RectTransform parent)
        {
            var go = new GameObject("_LineTemplate", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(100, 4);
            rt.pivot = new Vector2(0.5f, 0.5f);
            var img = go.AddComponent<Image>();
            img.color = C_Line;
            img.raycastTarget = true;
            return go;
        }

        // Sprite + rect helpers
        private static Sprite RoundedRectSprite(string name, int radius)
        {
            int size = radius * 4;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.name = name;
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    int dx = Mathf.Max(0, Mathf.Max(radius - x, x - (size - radius - 1)));
                    int dy = Mathf.Max(0, Mathf.Max(radius - y, y - (size - radius - 1)));
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    px[y * size + x] = new Color(1f, 1f, 1f, d > radius ? 0f : 1f);
                }
            tex.SetPixels(px); tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        }

        private static RectTransform NewChild(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static void FullScreen(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        private static void FullStretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        private static void Center(RectTransform rt, Vector2 size)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = Vector2.zero;
        }
    }
}
