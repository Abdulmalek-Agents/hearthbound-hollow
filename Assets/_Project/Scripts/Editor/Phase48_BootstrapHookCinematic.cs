// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase48_BootstrapHookCinematic
//
// PHASE 48 — One-click installer for the Cold Open cinematic.
//
// Drops a fully-wired `_ColdOpenCanvas` GameObject (Screen-Space Overlay)
// onto the Bootstrap scene, with every visual stage assembled from Unity
// UI primitives:
//
//   _ColdOpenCanvas
//   ├── _Vignette                       (full-screen black, always-on)
//   ├── _Candle
//   │   ├── _CandleHalo                 (wide soft-radial gradient)
//   │   ├── _CandleFlame                (narrow soft-radial gradient)
//   │   └── _Embers                     (4 small pulsing dots)
//   ├── _Letter
//   │   ├── _Parchment                  (creamy panel with inner border)
//   │   ├── _LetterBody (TMP)
//   │   └── _LetterSignature (TMP)
//   ├── _Montage
//   │   └── _MontagePhrase (TMP, centred)
//   ├── _PickleEyes
//   │   ├── _EyeLeft  (amber dot)
//   │   └── _EyeRight (amber dot)
//   ├── _Title
//   │   ├── _TitleText (TMP)
//   │   └── _SubtitleText (TMP)
//   └── _Choice
//       ├── _BeginButton
//       ├── _ContinueButton
//       └── _SkipHint (TMP)
//
// Also drops `_BootstrapHookDirector` GameObject and wires it to the
// canvas. Sets `GameManager.autoLoadMainMenu = false` on the Bootstrap
// scene's GameManager so the director owns the first transition.
//
// Idempotent — re-running deletes the previously-built nodes (matched by
// name) and rebuilds them. The Hearthbound priority is Advanced unless
// promoted by Phase 27's Build Everything chain (which it will be after
// Phase 54).

using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HearthboundHollow.Core;
using HearthboundHollow.UI;
using HearthboundHollow.Mission;

namespace HearthboundHollow.EditorTools
{
    public static class Phase48_BootstrapHookCinematic
    {
        private const string BootstrapScenePath = "Assets/_Project/Scenes/00_Bootstrap.unity";
        private const string CanvasName    = "_ColdOpenCanvas";
        private const string DirectorName  = "_BootstrapHookDirector";

        // Cozy autumn palette — every colour is hex-coded so future
        // colour-grading passes can substitute without touching code.
        private static readonly Color C_Black          = new Color(0.02f, 0.02f, 0.03f, 1f);
        private static readonly Color C_CandleFlame    = new Color(1.00f, 0.78f, 0.45f, 1f);
        private static readonly Color C_CandleHalo     = new Color(1.00f, 0.55f, 0.22f, 0.55f);
        private static readonly Color C_Ember          = new Color(1.00f, 0.70f, 0.30f, 1f);
        private static readonly Color C_Parchment      = new Color(0.92f, 0.86f, 0.71f, 1f);
        private static readonly Color C_ParchmentEdge  = new Color(0.45f, 0.34f, 0.20f, 1f);
        private static readonly Color C_LetterInk      = new Color(0.18f, 0.12f, 0.06f, 1f);
        private static readonly Color C_MontageInk     = new Color(0.94f, 0.85f, 0.65f, 1f);
        private static readonly Color C_PickleEye      = new Color(1.00f, 0.74f, 0.18f, 1f);
        private static readonly Color C_TitleInk       = new Color(0.97f, 0.92f, 0.80f, 1f);
        private static readonly Color C_SubtitleInk    = new Color(0.85f, 0.78f, 0.62f, 1f);

        [MenuItem("Hearthbound/⚙️ Advanced/🪔 Phase 48 — Build Cold Open Cinematic", priority = 4800)]
        public static void Build()
        {
            if (!System.IO.File.Exists(BootstrapScenePath))
            {
                EditorUtility.DisplayDialog(
                    "Phase 48 — Cold Open",
                    "Bootstrap scene not found at:\n" + BootstrapScenePath +
                    "\n\nRun 🚀 Build Everything first.", "OK");
                return;
            }

            var scene = EditorSceneManager.GetSceneByPath(BootstrapScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            }

            // ─── Build canvas ────────────────────────────────────────
            RemoveOld(scene);
            var canvasGO = BuildCanvasRoot();
            var ui = canvasGO.AddComponent<ColdOpenCinematicUI>();
            BuildVisualStages(canvasGO, ui);
            EnsureEventSystem(scene);

            // ─── Director ────────────────────────────────────────────
            var directorGO = new GameObject(DirectorName);
            SceneManager.MoveGameObjectToScene(directorGO, scene);
            var director = directorGO.AddComponent<BootstrapHookDirector>();
            director.cinematic = ui;
            director.mainMenuSceneName = "01_MainMenu";

            // ─── GameManager — disable autoLoadMainMenu ──────────────
            var gm = Object.FindFirstObjectByType<GameManager>();
            if (gm != null)
            {
                Undo.RecordObject(gm, "Phase 48 — disable autoLoadMainMenu");
                gm.autoLoadMainMenu = false;
            }
            else
            {
                Debug.LogWarning(
                    "[Phase 48] No GameManager found in Bootstrap scene. " +
                    "The BootstrapHookDirector will still run, but it cannot " +
                    "access VillageState until a GameManager is present.");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog(
                "Phase 48 — Cold Open Cinematic",
                "✓ Cold Open cinematic installed in Bootstrap scene.\n\n" +
                "Press Play → the cinematic runs once per save.\n" +
                "Press Esc / Space / click to skip from frame 1.\n\n" +
                "Toggle 'Replay Cold Open' in the Pause menu to see it again.\n" +
                "(Implemented in Phase 50.)", "OK");
        }

        // ─── Idempotent teardown ───────────────────────────────────────

        private static void RemoveOld(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == CanvasName || root.name == DirectorName)
                {
                    Object.DestroyImmediate(root);
                }
            }
        }

        // ─── Canvas + stages ───────────────────────────────────────────

        private static GameObject BuildCanvasRoot()
        {
            var go = new GameObject(CanvasName);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32760; // above every gameplay HUD
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();

            var rootGroup = go.AddComponent<CanvasGroup>();
            rootGroup.alpha = 0f;
            rootGroup.interactable = true;
            rootGroup.blocksRaycasts = true;

            return go;
        }

        private static void BuildVisualStages(GameObject root, ColdOpenCinematicUI ui)
        {
            ui.rootGroup = root.GetComponent<CanvasGroup>();

            // ─── Vignette ───────────────────────────────────────────
            var vignette = NewChild(root.transform, "_Vignette");
            FullScreen(vignette);
            var vImage = vignette.gameObject.AddComponent<Image>();
            vImage.color = C_Black;
            vImage.raycastTarget = false;
            ui.vignetteGroup = vignette.gameObject.AddComponent<CanvasGroup>();
            ui.vignetteGroup.alpha = 1f;
            ui.vignetteGroup.blocksRaycasts = false;

            // ─── Candle ─────────────────────────────────────────────
            var candleRoot = NewChild(root.transform, "_Candle");
            Center(candleRoot, new Vector2(420, 420));
            candleRoot.anchoredPosition = new Vector2(0, -40);

            var halo = NewChild(candleRoot, "_CandleHalo");
            Center(halo, new Vector2(420, 420));
            var haloImg = halo.gameObject.AddComponent<Image>();
            haloImg.color = new Color(C_CandleHalo.r, C_CandleHalo.g, C_CandleHalo.b, 0f);
            haloImg.raycastTarget = false;
            haloImg.sprite = RadialGradientSprite("ColdOpenHalo", 256, 0.05f, 0.95f);
            ui.candleHaloImage = haloImg;

            var flame = NewChild(candleRoot, "_CandleFlame");
            Center(flame, new Vector2(120, 200));
            var flameImg = flame.gameObject.AddComponent<Image>();
            flameImg.color = new Color(C_CandleFlame.r, C_CandleFlame.g, C_CandleFlame.b, 0f);
            flameImg.raycastTarget = false;
            flameImg.sprite = FlameGradientSprite("ColdOpenFlame", 128);
            ui.candleFlameImage = flameImg;

            var embersRoot = NewChild(candleRoot, "_Embers");
            Center(embersRoot, new Vector2(220, 320));
            ui.candleEmbersRoot = embersRoot;
            var emberList = new List<Image>(4);
            for (int i = 0; i < 4; i++)
            {
                var e = NewChild(embersRoot, $"_Ember_{i}");
                Center(e, new Vector2(18, 18));
                e.anchoredPosition = new Vector2((i - 1.5f) * 40f, 80f + i * 20f);
                var img = e.gameObject.AddComponent<Image>();
                img.color = new Color(C_Ember.r, C_Ember.g, C_Ember.b, 0f);
                img.raycastTarget = false;
                img.sprite = RadialGradientSprite("ColdOpenEmber", 32, 0.1f, 0.9f);
                emberList.Add(img);
            }
            ui.embers = emberList.ToArray();

            // ─── Letter / Parchment ─────────────────────────────────
            var letter = NewChild(root.transform, "_Letter");
            Center(letter, new Vector2(960, 620));
            letter.anchoredPosition = new Vector2(0, -10);
            var letterGroup = letter.gameObject.AddComponent<CanvasGroup>();
            letterGroup.alpha = 0f;
            ui.letterGroup = letterGroup;

            var parchment = NewChild(letter, "_Parchment");
            FullStretch(parchment);
            var parchmentImg = parchment.gameObject.AddComponent<Image>();
            parchmentImg.color = C_Parchment;
            parchmentImg.raycastTarget = false;
            parchmentImg.sprite = ParchmentSprite("ColdOpenParchment", 256);
            parchmentImg.type = Image.Type.Sliced;

            var body = NewChild(letter, "_LetterBody");
            FullStretch(body);
            body.offsetMin = new Vector2(80, 110);
            body.offsetMax = new Vector2(-80, -70);
            var bodyTmp = body.gameObject.AddComponent<TextMeshProUGUI>();
            bodyTmp.text = "";
            bodyTmp.color = C_LetterInk;
            bodyTmp.fontSize = 30;
            bodyTmp.alignment = TextAlignmentOptions.TopLeft;
            bodyTmp.enableWordWrapping = true;
            ui.letterBodyText = bodyTmp;

            var sig = NewChild(letter, "_LetterSignature");
            FullStretch(sig);
            sig.offsetMin = new Vector2(80, 40);
            sig.offsetMax = new Vector2(-80, -480);
            var sigTmp = sig.gameObject.AddComponent<TextMeshProUGUI>();
            sigTmp.text = "";
            sigTmp.color = C_LetterInk;
            sigTmp.fontSize = 24;
            sigTmp.fontStyle = FontStyles.Italic;
            sigTmp.alignment = TextAlignmentOptions.BottomRight;
            ui.letterSignature = sigTmp;

            // ─── Montage ────────────────────────────────────────────
            var montage = NewChild(root.transform, "_Montage");
            FullScreen(montage);
            var montageGroup = montage.gameObject.AddComponent<CanvasGroup>();
            montageGroup.alpha = 0f;
            ui.montageGroup = montageGroup;

            var phrase = NewChild(montage, "_MontagePhrase");
            Center(phrase, new Vector2(1400, 260));
            var phraseTmp = phrase.gameObject.AddComponent<TextMeshProUGUI>();
            phraseTmp.text = "";
            phraseTmp.color = C_MontageInk;
            phraseTmp.fontSize = 44;
            phraseTmp.fontStyle = FontStyles.Italic;
            phraseTmp.alignment = TextAlignmentOptions.Center;
            phraseTmp.enableWordWrapping = true;
            ui.montagePhraseText = phraseTmp;

            // ─── Pickle eyes ────────────────────────────────────────
            var eyes = NewChild(root.transform, "_PickleEyes");
            Center(eyes, new Vector2(280, 80));
            eyes.anchoredPosition = new Vector2(0, -120);
            var eyesGroup = eyes.gameObject.AddComponent<CanvasGroup>();
            eyesGroup.alpha = 0f;
            ui.pickleEyesGroup = eyesGroup;

            var eyeL = NewChild(eyes, "_EyeLeft");
            Center(eyeL, new Vector2(56, 28));
            eyeL.anchoredPosition = new Vector2(-58, 0);
            var eyeLImg = eyeL.gameObject.AddComponent<Image>();
            eyeLImg.color = C_PickleEye;
            eyeLImg.raycastTarget = false;
            eyeLImg.sprite = EyeSprite("ColdOpenEyeL", 32);
            ui.pickleEyeLeft = eyeLImg;

            var eyeR = NewChild(eyes, "_EyeRight");
            Center(eyeR, new Vector2(56, 28));
            eyeR.anchoredPosition = new Vector2(58, 0);
            var eyeRImg = eyeR.gameObject.AddComponent<Image>();
            eyeRImg.color = C_PickleEye;
            eyeRImg.raycastTarget = false;
            eyeRImg.sprite = EyeSprite("ColdOpenEyeR", 32);
            ui.pickleEyeRight = eyeRImg;

            // ─── Title ──────────────────────────────────────────────
            var title = NewChild(root.transform, "_Title");
            FullScreen(title);
            var titleGroup = title.gameObject.AddComponent<CanvasGroup>();
            titleGroup.alpha = 0f;
            ui.titleGroup = titleGroup;

            var titleTxt = NewChild(title, "_TitleText");
            Center(titleTxt, new Vector2(1800, 220));
            titleTxt.anchoredPosition = new Vector2(0, 80);
            var titleTmp = titleTxt.gameObject.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "Hearthbound Hollow";
            titleTmp.color = C_TitleInk;
            titleTmp.fontSize = 120;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.characterSpacing = 6f;
            ui.titleText = titleTmp;

            var subtitleTxt = NewChild(title, "_SubtitleText");
            Center(subtitleTxt, new Vector2(1500, 80));
            subtitleTxt.anchoredPosition = new Vector2(0, -60);
            var subtitleTmp = subtitleTxt.gameObject.AddComponent<TextMeshProUGUI>();
            subtitleTmp.text = "Some memories want to be sold. Some don't.";
            subtitleTmp.color = C_SubtitleInk;
            subtitleTmp.fontSize = 36;
            subtitleTmp.fontStyle = FontStyles.Italic;
            subtitleTmp.alignment = TextAlignmentOptions.Center;
            ui.subtitleText = subtitleTmp;

            // ─── Choice gate ────────────────────────────────────────
            var choice = NewChild(root.transform, "_Choice");
            FullScreen(choice);
            var choiceGroup = choice.gameObject.AddComponent<CanvasGroup>();
            choiceGroup.alpha = 0f;
            ui.choiceGroup = choiceGroup;

            ui.beginButton = BuildChoiceButton(choice, "_BeginButton", "Begin",   new Vector2(-180, -240));
            ui.continueButton = BuildChoiceButton(choice, "_ContinueButton", "Continue", new Vector2(180, -240));

            var skipHint = NewChild(choice, "_SkipHint");
            Center(skipHint, new Vector2(1200, 40));
            skipHint.anchoredPosition = new Vector2(0, -360);
            var skipTmp = skipHint.gameObject.AddComponent<TextMeshProUGUI>();
            skipTmp.text = "Press Enter / Space to Begin";
            skipTmp.color = new Color(0.7f, 0.65f, 0.5f, 1f);
            skipTmp.fontSize = 22;
            skipTmp.alignment = TextAlignmentOptions.Center;
            ui.skipHintLabel = skipTmp;
        }

        private static Button BuildChoiceButton(RectTransform parent, string name, string label, Vector2 pos)
        {
            var go = NewChild(parent, name);
            Center(go, new Vector2(280, 92));
            go.anchoredPosition = pos;
            var img = go.gameObject.AddComponent<Image>();
            img.color = new Color(0.10f, 0.07f, 0.04f, 0.92f);
            img.sprite = RoundedRectSprite($"ColdOpenButton_{name}", 16);
            img.type = Image.Type.Sliced;
            var btn = go.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.normalColor      = new Color(1f, 1f, 1f, 1f);
            colors.highlightedColor = new Color(1.0f, 0.92f, 0.78f, 1f);
            colors.pressedColor     = new Color(0.85f, 0.72f, 0.55f, 1f);
            btn.colors = colors;

            var txt = NewChild(go, "_Label");
            FullStretch(txt);
            txt.offsetMin = new Vector2(20, 14);
            txt.offsetMax = new Vector2(-20, -14);
            var tmp = txt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.color = C_TitleInk;
            tmp.fontSize = 38;
            tmp.alignment = TextAlignmentOptions.Center;
            return btn;
        }

        // ─── Sprite generators (procedural — no asset deps) ────────────

        private static Sprite RadialGradientSprite(string name, int size, float innerAlpha, float outerAlpha)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.name = name;
            var px = new Color[size * size];
            float r = size * 0.5f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - r) / r, dy = (y - r) / r;
                    float d = Mathf.Clamp01(Mathf.Sqrt(dx * dx + dy * dy));
                    float a = Mathf.Lerp(1f - innerAlpha, 1f - outerAlpha, d);
                    a = Mathf.Clamp01(1f - d);
                    a = a * a; // softer falloff
                    px[y * size + x] = new Color(1f, 1f, 1f, a);
                }
            tex.SetPixels(px); tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        private static Sprite FlameGradientSprite(string name, int size)
        {
            // Teardrop-shaped flame: thin at top, wider mid, tapering at base.
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.name = name;
            var px = new Color[size * size];
            float cx = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                float ny = y / (float)(size - 1); // 0 at bottom → 1 at top
                // Width follows a sine that peaks at ~0.45 of height
                float w = Mathf.Sin(Mathf.Clamp01(ny) * Mathf.PI) * 0.55f + 0.15f;
                w *= (1f - 0.4f * ny); // taper top
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - cx) / cx;
                    float d = Mathf.Abs(nx) / Mathf.Max(0.001f, w);
                    float a = Mathf.Clamp01(1f - d);
                    a = a * a;
                    px[y * size + x] = new Color(1f, 1f, 1f, a);
                }
            }
            tex.SetPixels(px); tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        private static Sprite EyeSprite(string name, int size)
        {
            // Almond shape — cat's-eye proportions.
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.name = name;
            var px = new Color[size * size];
            float cx = size * 0.5f, cy = size * 0.5f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - cx) / cx;
                    float dy = (y - cy) / (cy * 0.45f);
                    float d = dx * dx + dy * dy;
                    float a = Mathf.Clamp01(1f - d);
                    a = a * a;
                    px[y * size + x] = new Color(1f, 1f, 1f, a);
                }
            tex.SetPixels(px); tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        private static Sprite ParchmentSprite(string name, int size)
        {
            // A simple cream rectangle with a dark inner border.
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.name = name;
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    bool border = (x < 6 || x > size - 7 || y < 6 || y > size - 7);
                    px[y * size + x] = border ? C_ParchmentEdge : C_Parchment;
                }
            tex.SetPixels(px); tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(12, 12, 12, 12));
        }

        private static Sprite RoundedRectSprite(string name, int radius)
        {
            int size = radius * 4;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.name = name;
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    // approximate rounded-rect: clamp distance from edges, then circle.
                    int dx = Mathf.Max(0, Mathf.Max(radius - x, x - (size - radius - 1)));
                    int dy = Mathf.Max(0, Mathf.Max(radius - y, y - (size - radius - 1)));
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = d > radius ? 0f : 1f;
                    px[y * size + x] = new Color(1f, 1f, 1f, a);
                }
            tex.SetPixels(px); tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        }

        // ─── Rect helpers ──────────────────────────────────────────────

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

        private static void EnsureEventSystem(Scene scene)
        {
            var es = Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (es == null)
            {
                var go = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem));
                // D-066: match the rest of the builder family — under the new
                // Input System the legacy StandaloneInputModule cannot read
                // pointer input, so clicks die. Branch on ENABLE_INPUT_SYSTEM.
#if ENABLE_INPUT_SYSTEM && UNITY_2020_2_OR_NEWER
                go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
                go.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif
                SceneManager.MoveGameObjectToScene(go, scene);
            }
        }
    }
}
