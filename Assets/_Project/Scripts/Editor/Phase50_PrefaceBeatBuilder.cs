// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase50_PrefaceBeatBuilder
//
// PHASE 50 — One-click installer for the Preface Beat on Lane scene.
//
// Drops `_PrefaceBeatCanvas` + `_PrefaceBeatDirector` onto the Lane
// scene (02_Mission01_Lane). The canvas is a Screen-Space Overlay with
// two letterbox bars at top/bottom + three TMP lines stacked centred,
// plus a tinted leaf-fall overlay Image at the top.
//
// Director references are auto-resolved at install time:
//   - PrefaceBeatUI (the canvas itself)
//   - PlayerController (FindFirstObjectByType in the scene)
//   - OnboardingOverlay (FindFirstObjectByType in the scene)
//
// Idempotent — re-running deletes the previously-built nodes and
// rebuilds them.

using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HearthboundHollow.Mission;
using HearthboundHollow.Player;
using HearthboundHollow.UI;

namespace HearthboundHollow.EditorTools
{
    public static class Phase50_PrefaceBeatBuilder
    {
        private const string LaneScenePath  = "Assets/_Project/Scenes/02_Mission01_Lane.unity";
        private const string CanvasName     = "_PrefaceBeatCanvas";
        private const string DirectorName   = "_PrefaceBeatDirector";

        private static readonly Color C_Letterbox = new Color(0.02f, 0.02f, 0.03f, 1f);
        private static readonly Color C_Line      = new Color(0.96f, 0.92f, 0.78f, 1f);
        private static readonly Color C_LeafFall  = new Color(0.85f, 0.55f, 0.18f, 1f);

        [MenuItem("Hearthbound/⚙️ Advanced/🍂 Phase 50 — Wire Preface Beat (Lane)", priority = 5000)]
        public static void Build()
        {
            if (!System.IO.File.Exists(LaneScenePath))
            {
                EditorUtility.DisplayDialog(
                    "Phase 50 — Preface Beat",
                    "Lane scene not found at:\n" + LaneScenePath +
                    "\n\nRun 🚀 Build Everything first.", "OK");
                return;
            }

            var scene = EditorSceneManager.GetSceneByPath(LaneScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                scene = EditorSceneManager.OpenScene(LaneScenePath, OpenSceneMode.Single);
            }

            RemoveOld(scene);
            var (canvasGO, ui) = BuildCanvas();
            SceneManager.MoveGameObjectToScene(canvasGO, scene);

            var directorGO = new GameObject(DirectorName);
            SceneManager.MoveGameObjectToScene(directorGO, scene);
            var director = directorGO.AddComponent<PrefaceBeatDirector>();
            director.prefaceBeat = ui;
            director.playerController = Object.FindFirstObjectByType<PlayerController>();
            director.onboardingOverlay = Object.FindFirstObjectByType<OnboardingOverlay>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog(
                "Phase 50 — Preface Beat",
                "✓ Preface Beat installed on Lane scene.\n\n" +
                "Plays once per save on first entry to the Lane. Gentle Mode " +
                "auto-routes to the gentle cadence. Skip with Esc / Space / click.",
                "OK");
        }

        private static void RemoveOld(UnityEngine.SceneManagement.Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == CanvasName || root.name == DirectorName)
                    Object.DestroyImmediate(root);
            }
        }

        private static (GameObject, PrefaceBeatUI) BuildCanvas()
        {
            var go = new GameObject(CanvasName);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32700;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            go.AddComponent<GraphicRaycaster>();
            var rootGroup = go.AddComponent<CanvasGroup>();
            rootGroup.alpha = 0f;
            rootGroup.blocksRaycasts = false;

            // ─── Letterboxes ─────────────────────────────────────────
            var top = NewChild(go.transform, "_TopLetterbox");
            top.anchorMin = new Vector2(0, 1); top.anchorMax = new Vector2(1, 1);
            top.pivot = new Vector2(0.5f, 1f);
            top.sizeDelta = new Vector2(0, 160);
            top.anchoredPosition = Vector2.zero;
            var topImg = top.gameObject.AddComponent<Image>(); topImg.color = C_Letterbox; topImg.raycastTarget = false;

            var bot = NewChild(go.transform, "_BottomLetterbox");
            bot.anchorMin = new Vector2(0, 0); bot.anchorMax = new Vector2(1, 0);
            bot.pivot = new Vector2(0.5f, 0f);
            bot.sizeDelta = new Vector2(0, 160);
            bot.anchoredPosition = Vector2.zero;
            var botImg = bot.gameObject.AddComponent<Image>(); botImg.color = C_Letterbox; botImg.raycastTarget = false;

            // ─── Three stacked TMP lines ─────────────────────────────
            var line1 = MakeLine(go.transform, "_Line1", new Vector2(0, 60));
            var line2 = MakeLine(go.transform, "_Line2", new Vector2(0, 0));
            var line3 = MakeLine(go.transform, "_Line3", new Vector2(0, -60));

            // ─── Leaf fall hint ──────────────────────────────────────
            var leaves = NewChild(go.transform, "_LeafFall");
            leaves.anchorMin = new Vector2(0, 1); leaves.anchorMax = new Vector2(1, 1);
            leaves.pivot = new Vector2(0.5f, 1f);
            leaves.sizeDelta = new Vector2(0, 200);
            leaves.anchoredPosition = new Vector2(0, -120);
            var leavesImg = leaves.gameObject.AddComponent<Image>();
            leavesImg.color = new Color(C_LeafFall.r, C_LeafFall.g, C_LeafFall.b, 0f);
            leavesImg.raycastTarget = false;

            var ui = go.AddComponent<PrefaceBeatUI>();
            ui.rootGroup = rootGroup;
            ui.topLetterbox = topImg;
            ui.bottomLetterbox = botImg;
            ui.line1Text = line1;
            ui.line2Text = line2;
            ui.line3Text = line3;
            ui.leafFallOverlay = leavesImg;

            return (go, ui);
        }

        private static TextMeshProUGUI MakeLine(Transform parent, string name, Vector2 offset)
        {
            var rt = NewChild(parent, name);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(1500, 80);
            rt.anchoredPosition = offset;
            var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = "";
            tmp.color = C_Line;
            tmp.fontSize = 40;
            tmp.fontStyle = FontStyles.Italic;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = true;
            return tmp;
        }

        private static RectTransform NewChild(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }
    }
}
