// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase30_OnboardingAndHintsCapstone
//
// PHASE 30 — Onboarding & Control Guidance UI.
//
// User request:
//   "create the onboarding and control guidance UI so enhance the whole gameplay"
//
// What this capstone builds (idempotent — safe to re-run):
//
//   1. **OnboardingOverlay** — drop on the Main Menu canvas as a child of
//      `_Controller`. It auto-shows on the first time the player starts a
//      new game and walks them through 6 steps:
//        Welcome → Move (WASD) → Interact (E) → Polish (LMB) → Pause/Help
//        → "You're ready" — then they continue into the Lane scene.
//      The first-Lane-load also re-checks the flag and shows the overlay
//      if the player skipped the MainMenu version (defensive). The flag
//      `VillageState.onboardingCompleted` gates everything; saves persist
//      so the overlay never re-appears once acknowledged.
//
//   2. **ControlHintsHUD** — persistent context-aware key-chip strip at
//      the bottom-left of every gameplay scene canvas. Always visible at
//      idleAlpha = 0.45; the [E] chip emphasises to full alpha when an
//      interactable is in range, displaying its prompt as the caption.
//      Cozy color palette — no jarring HUD blue.
//
// USE: Menu → Hearthbound → 🎓 Phase 30 — Build Onboarding + Hints HUD

using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HearthboundHollow.Mission;
using HearthboundHollow.UI;

namespace HearthboundHollow.EditorTools
{
    public static class Phase30_OnboardingAndHintsCapstone
    {
        // ─── Scene targets ──────────────────────────────────────

        private static readonly string[] GameplayScenes = new[]
        {
            "Assets/_Project/Scenes/02_Mission01_Lane.unity",
            "Assets/_Project/Scenes/03_Mission01_Hollow.unity",
            "Assets/_Project/Scenes/04_Mission02_Garden.unity",
            "Assets/_Project/Scenes/05_Mission02_Cottage.unity",
        };

        private const string LaneScene = "Assets/_Project/Scenes/02_Mission01_Lane.unity";

        // ─── Colours (warm parchment palette) ─────────────────────

        private static readonly Color Vignette       = new Color(0.05f, 0.04f, 0.03f, 0.88f);
        private static readonly Color PanelBg        = new Color(0.12f, 0.10f, 0.08f, 0.98f);
        private static readonly Color GoldHeadline   = new Color(0.97f, 0.85f, 0.62f, 1f);
        private static readonly Color CreamBody      = new Color(0.92f, 0.88f, 0.78f, 1f);
        private static readonly Color SubtleHint     = new Color(0.86f, 0.78f, 0.66f, 1f);
        private static readonly Color ChipBg         = new Color(0.18f, 0.13f, 0.09f, 0.92f);
        private static readonly Color ChipBorder     = new Color(0.62f, 0.46f, 0.22f, 0.85f);
        private static readonly Color ButtonBg       = new Color(0.20f, 0.14f, 0.10f, 0.96f);

        // ─── Menu ────────────────────────────────────────────────

        [MenuItem("Hearthbound/⚙️ Advanced/🎓 Phase 30 — Build Onboarding + Hints HUD", priority = 2)]
        public static void Build()
        {
            EditorUtility.DisplayProgressBar("Hearthbound · Phase 30", "Wiring Onboarding onto the Lane scene…", 0.20f);
            try
            {
                int laneTouched = WireOnboarding(LaneScene) ? 1 : 0;

                int hintsTouched = 0;
                for (int i = 0; i < GameplayScenes.Length; i++)
                {
                    EditorUtility.DisplayProgressBar(
                        "Hearthbound · Phase 30",
                        $"Wiring ControlHintsHUD ({i + 1}/{GameplayScenes.Length})…",
                        0.30f + 0.60f * (i / (float)GameplayScenes.Length));
                    if (WireControlHintsHUD(GameplayScenes[i])) hintsTouched++;
                }

                EditorUtility.DisplayProgressBar("Hearthbound · Phase 30", "Saving…", 0.97f);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog(
                    "Hearthbound — Phase 30 complete",
                    "Onboarding & Control Hints HUD wired.\n\n" +
                    $"  ✓ OnboardingOverlay on Lane scene: {(laneTouched > 0 ? "yes" : "skipped (scene missing)")}\n" +
                    $"  ✓ ControlHintsHUD on {hintsTouched}/{GameplayScenes.Length} gameplay scenes\n\n" +
                    "Press Play in 00_Bootstrap → the player will see the 6-step walkthrough on Lane the first time, " +
                    "and the always-visible [WASD] [E] [H] hint strip in every scene.",
                    "OK");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        // ─── Onboarding wiring (Lane) ──────────────────────────────

        private static bool WireOnboarding(string scenePath)
        {
            if (!File.Exists(scenePath))
            {
                Debug.LogWarning($"[Hearthbound/Phase 30] (skip) {scenePath} not present.");
                return false;
            }
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // Find / create canvas.
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                var canvasGO = new GameObject("UI_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                var c = canvasGO.GetComponent<Canvas>();
                c.renderMode = RenderMode.ScreenSpaceOverlay;
                c.sortingOrder = 50;
                var s = canvasGO.GetComponent<CanvasScaler>();
                s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                s.referenceResolution = new Vector2(1920, 1080);
                canvas = c;
            }

            // Idempotency — skip if already present.
            if (Object.FindFirstObjectByType<OnboardingOverlay>() != null)
            {
                Debug.Log($"[Hearthbound/Phase 30] OnboardingOverlay already present on {Path.GetFileName(scenePath)}, skipping.");
                EditorSceneManager.SaveScene(scene);
                return false;
            }

            BuildOnboardingOverlay(canvas.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[Hearthbound/Phase 30] OnboardingOverlay added to {Path.GetFileName(scenePath)}.");
            return true;
        }

        private static OnboardingOverlay BuildOnboardingOverlay(Transform canvas)
        {
            // Two-layer pattern: script-host always active, Visual child toggled.
            var hostGO = new GameObject("OnboardingOverlay", typeof(RectTransform));
            hostGO.transform.SetParent(canvas, false);
            var hostRT = hostGO.GetComponent<RectTransform>();
            hostRT.anchorMin = Vector2.zero; hostRT.anchorMax = Vector2.one;
            hostRT.offsetMin = Vector2.zero; hostRT.offsetMax = Vector2.zero;

            var visualGO = new GameObject("Visual", typeof(RectTransform));
            visualGO.transform.SetParent(hostGO.transform, false);
            var vRT = visualGO.GetComponent<RectTransform>();
            vRT.anchorMin = Vector2.zero; vRT.anchorMax = Vector2.one;
            vRT.offsetMin = Vector2.zero; vRT.offsetMax = Vector2.zero;

            // Vignette
            var bg = visualGO.AddComponent<Image>();
            bg.color = Vignette;

            // Card panel — centered, parchment-warm
            var card = new GameObject("Card", typeof(RectTransform));
            card.transform.SetParent(visualGO.transform, false);
            var cardImg = card.AddComponent<Image>();
            cardImg.color = PanelBg;
            var cardRT = cardImg.rectTransform;
            cardRT.anchorMin = new Vector2(0.18f, 0.20f);
            cardRT.anchorMax = new Vector2(0.82f, 0.85f);
            cardRT.offsetMin = Vector2.zero; cardRT.offsetMax = Vector2.zero;

            // Headline
            var headline = MakeLabel(card.transform, "Headline",
                anchorMin: new Vector2(0.08f, 0.78f), anchorMax: new Vector2(0.92f, 0.92f),
                fontSize: 38, color: GoldHeadline, bold: true, align: TextAlignmentOptions.Center);

            // Body
            var body = MakeLabel(card.transform, "Body",
                anchorMin: new Vector2(0.08f, 0.35f), anchorMax: new Vector2(0.92f, 0.76f),
                fontSize: 22, color: CreamBody, bold: false, align: TextAlignmentOptions.TopLeft);
            body.enableWordWrapping = true;

            // Key chip group — keyChip + caption.
            var chipGroup = new GameObject("KeyChip", typeof(RectTransform));
            chipGroup.transform.SetParent(card.transform, false);
            var chipRT = chipGroup.GetComponent<RectTransform>();
            chipRT.anchorMin = new Vector2(0.36f, 0.18f);
            chipRT.anchorMax = new Vector2(0.64f, 0.34f);
            chipRT.offsetMin = Vector2.zero; chipRT.offsetMax = Vector2.zero;

            var chipBg = chipGroup.AddComponent<Image>();
            chipBg.color = ChipBg;

            var chipLabel = MakeLabel(chipGroup.transform, "ChipLabel",
                anchorMin: new Vector2(0, 0.30f), anchorMax: new Vector2(1, 1f),
                fontSize: 32, color: GoldHeadline, bold: true, align: TextAlignmentOptions.Center);

            var chipCaption = MakeLabel(chipGroup.transform, "ChipCaption",
                anchorMin: new Vector2(0, 0f), anchorMax: new Vector2(1, 0.28f),
                fontSize: 16, color: SubtleHint, bold: false, align: TextAlignmentOptions.Center);

            // Progress label ("1 / 6")
            var progress = MakeLabel(card.transform, "Progress",
                anchorMin: new Vector2(0.42f, 0.04f), anchorMax: new Vector2(0.58f, 0.12f),
                fontSize: 16, color: SubtleHint, bold: false, align: TextAlignmentOptions.Center);

            // Buttons
            var nextBtn = MakeButton(card.transform, "Btn_Next", "Next →",
                new Vector2(0.62f, 0.04f), new Vector2(0.92f, 0.14f));
            var skipBtn = MakeButton(card.transform, "Btn_Skip", "Skip Tutorial",
                new Vector2(0.08f, 0.04f), new Vector2(0.38f, 0.14f));

            // Wire script — lives on the HOST so Update() runs even while
            // Visual is hidden by Hide(). Visual child is toggled by Begin/Hide.
            var overlay = hostGO.AddComponent<OnboardingOverlay>();
            overlay.root = visualGO;
            overlay.headlineLabel = headline;
            overlay.bodyLabel = body;
            overlay.keyChipLabel = chipLabel;
            overlay.keyCaptionLabel = chipCaption;
            overlay.progressLabel = progress;
            overlay.nextButton = nextBtn;
            overlay.skipButton = skipBtn;
            overlay.autoShowOnStart = true;
            overlay.pauseGameWhileOpen = true;

            // Hide visual; the overlay's Start() will call BeginIfNeeded()
            // which re-activates the visual when the save flag is false.
            visualGO.SetActive(false);
            return overlay;
        }

        // ─── ControlHintsHUD wiring (every gameplay scene) ────────

        private static bool WireControlHintsHUD(string scenePath)
        {
            if (!File.Exists(scenePath))
            {
                Debug.LogWarning($"[Hearthbound/Phase 30] (skip) {scenePath} not present.");
                return false;
            }
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            if (Object.FindFirstObjectByType<ControlHintsHUD>() != null)
            {
                Debug.Log($"[Hearthbound/Phase 30] ControlHintsHUD already present on {Path.GetFileName(scenePath)}, skipping.");
                EditorSceneManager.SaveScene(scene);
                return false;
            }

            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                var canvasGO = new GameObject("UI_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                var c = canvasGO.GetComponent<Canvas>();
                c.renderMode = RenderMode.ScreenSpaceOverlay;
                c.sortingOrder = 40;
                var s = canvasGO.GetComponent<CanvasScaler>();
                s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                s.referenceResolution = new Vector2(1920, 1080);
                canvas = c;
            }

            BuildControlHintsHUD(canvas.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[Hearthbound/Phase 30] ControlHintsHUD added to {Path.GetFileName(scenePath)}.");
            return true;
        }

        private static void BuildControlHintsHUD(Transform canvas)
        {
            // Two-layer pattern.
            var hostGO = new GameObject("ControlHintsHUD", typeof(RectTransform));
            hostGO.transform.SetParent(canvas, false);
            var hostRT = hostGO.GetComponent<RectTransform>();
            hostRT.anchorMin = Vector2.zero; hostRT.anchorMax = Vector2.one;
            hostRT.offsetMin = Vector2.zero; hostRT.offsetMax = Vector2.zero;

            var visualGO = new GameObject("Visual", typeof(RectTransform));
            visualGO.transform.SetParent(hostGO.transform, false);
            var vRT = visualGO.GetComponent<RectTransform>();
            // Anchor to the bottom-left, 3 chips wide.
            vRT.anchorMin = new Vector2(0.012f, 0.012f);
            vRT.anchorMax = new Vector2(0.260f, 0.085f);
            vRT.offsetMin = Vector2.zero; vRT.offsetMax = Vector2.zero;

            var cg = visualGO.AddComponent<CanvasGroup>();
            cg.alpha = 0.45f;
            cg.interactable = false;
            cg.blocksRaycasts = false;

            // Lay out 3 chips horizontally with HorizontalLayoutGroup.
            var hLayout = visualGO.AddComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 12;
            hLayout.childForceExpandWidth = true;
            hLayout.childForceExpandHeight = true;
            hLayout.padding = new RectOffset(6, 6, 4, 4);

            var moveChip     = MakeHintChip(visualGO.transform, "Chip_Move",     "WASD", "Move");
            var interactChip = MakeHintChip(visualGO.transform, "Chip_Interact", "E",    "Interact");
            var helpChip     = MakeHintChip(visualGO.transform, "Chip_Help",     "H",    "Help");

            var hud = hostGO.AddComponent<ControlHintsHUD>();
            hud.root = visualGO;
            hud.canvasGroup = cg;
            hud.chipMoveLabel = moveChip.label;
            hud.chipMoveCaption = moveChip.caption;
            hud.chipInteractLabel = interactChip.label;
            hud.chipInteractCaption = interactChip.caption;
            hud.chipHelpLabel = helpChip.label;
            hud.chipHelpCaption = helpChip.caption;
        }

        private struct ChipResult { public TextMeshProUGUI label; public TextMeshProUGUI caption; }

        private static ChipResult MakeHintChip(Transform parent, string name, string label, string caption)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var bg = go.AddComponent<Image>();
            bg.color = ChipBg;

            var labelGO = MakeLabel(go.transform, "Label",
                anchorMin: new Vector2(0, 0.40f), anchorMax: new Vector2(1, 1f),
                fontSize: 22, color: GoldHeadline, bold: true, align: TextAlignmentOptions.Center);
            labelGO.text = label;

            var capGO = MakeLabel(go.transform, "Caption",
                anchorMin: new Vector2(0, 0f), anchorMax: new Vector2(1, 0.38f),
                fontSize: 12, color: SubtleHint, bold: false, align: TextAlignmentOptions.Center);
            capGO.text = caption;

            return new ChipResult { label = labelGO, caption = capGO };
        }

        // ─── Helpers ──────────────────────────────────────────────

        private static TextMeshProUGUI MakeLabel(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax,
            int fontSize, Color color, bool bold, TextAlignmentOptions align)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = align;
            tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            var rt = tmp.rectTransform;
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            UIAutoFitText.ApplyToLabel(tmp, minSize: Mathf.Max(8, fontSize - 8), maxSize: fontSize + 2);
            return tmp;
        }

        private static Button MakeButton(Transform parent, string name, string label,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var btnGO = new GameObject(name, typeof(Image), typeof(Button));
            btnGO.transform.SetParent(parent, false);
            var img = btnGO.GetComponent<Image>();
            img.color = ButtonBg;
            var rt = img.rectTransform;
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var labelTMP = MakeLabel(btnGO.transform, "Label",
                Vector2.zero, Vector2.one,
                fontSize: 22, color: GoldHeadline, bold: true, align: TextAlignmentOptions.Center);
            labelTMP.text = label;
            UIAutoFitText.ApplyToButtonLabel(labelTMP, minSize: 14, maxSize: 24);
            return btnGO.GetComponent<Button>();
        }
    }
}
