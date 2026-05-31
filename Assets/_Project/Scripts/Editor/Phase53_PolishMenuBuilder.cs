// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase53_PolishMenuBuilder
//
// PHASE 53 — Polish Menu Layer installer. Builds + wires, idempotently:
//   • Character Creation screen (skin / outfit / accessory / name) on the Main
//     Menu, gated into New Game and re-openable from Settings → Customize.
//   • System menu (Language EN/العربية · Customize · Reset Game) on the Main
//     Menu Settings and on every gameplay scene's Pause → Settings.
//   • CharacterAppearanceApplier in each gameplay scene so the chosen look
//     shows on the avatar.
//
// All visuals are built from Unity's built-in UI sprites + the Mission
// CharacterPalette (no new art). Registered under ⚙️ Advanced (D-051) and
// chained into 🚀 Build Everything. Falls back gracefully when a scene /
// controller isn't present. (D-065 + D-066.)

using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HearthboundHollow.Core;
using HearthboundHollow.Mission;
using HearthboundHollow.UI;
// Disambiguate from UnityEditor.SettingsService (the Editor settings-provider API),
// which `using UnityEditor;` also brings into scope (CS0104).
using SettingsService = HearthboundHollow.Core.SettingsService;

namespace HearthboundHollow.EditorTools
{
    public static class Phase53_PolishMenuBuilder
    {
        private const string MainMenuScene = "Assets/_Project/Scenes/01_MainMenu.unity";
        private static readonly string[] GameplayScenes =
        {
            "Assets/_Project/Scenes/02_Mission01_Lane.unity",
            "Assets/_Project/Scenes/03_Mission01_Hollow.unity",
            "Assets/_Project/Scenes/04_Mission02_Garden.unity",
            "Assets/_Project/Scenes/05_Mission02_Cottage.unity",
        };

        private const string SysMenuNode  = "_SystemMenu";
        private const string CreatorNode  = "_CharacterCreation";
        private const string CoordNode    = "_PolishMenuCoordinator";
        private const string ApplierNode  = "_CharacterAppearance";

        private static Sprite Panel  => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        private static Sprite Button9 => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        [MenuItem("Hearthbound/⚙️ Advanced/🎭 Phase 53 — Build Polish Menu (language · reset · character)", priority = 5300)]
        public static void Build()
        {
            int wired = 0;
            if (BuildMainMenu()) wired++;
            foreach (var s in GameplayScenes) if (BuildGameplay(s)) wired++;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog(
                "Phase 53 — Polish Menu Layer",
                $"✓ Installed in {wired} scene(s).\n\n" +
                "• Main Menu: New Game opens the Character Creator; Settings adds\n" +
                "  Language (English / العربية) · Customize · Reset Game.\n" +
                "• Gameplay Pause → Settings: Language + Reset Game.\n" +
                "• Avatar shows the chosen skin / outfit / accessory.\n\n" +
                "Press Play in 00_Bootstrap.",
                "OK");
        }

        // ───── Main Menu scene ─────────────────────────────────────

        private static bool BuildMainMenu()
        {
            if (!System.IO.File.Exists(MainMenuScene))
            {
                Debug.LogWarning($"[Hearthbound/Phase 53] (skip) {MainMenuScene} not found.");
                return false;
            }
            var scene = OpenScene(MainMenuScene);
            RemoveOld(scene, SysMenuNode, CreatorNode, CoordNode);

            var sysMenu  = BuildSystemMenu(scene, includeCustomize: true);
            var creator  = BuildCharacterCreator(scene);

            var coordGO = new GameObject(CoordNode);
            SceneManager.MoveGameObjectToScene(coordGO, scene);
            var coord = coordGO.AddComponent<PolishMenuCoordinator>();
            coord.systemMenu = sysMenu;
            coord.characterCreation = creator;
            coord.mainMenu = Object.FindFirstObjectByType<MainMenuController>();
            coord.mainMenuScene = "01_MainMenu";

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Hearthbound/Phase 53] Main Menu wired (creator + system menu + coordinator).");
            return true;
        }

        // ───── Gameplay scenes ─────────────────────────────────────

        private static bool BuildGameplay(string scenePath)
        {
            if (!System.IO.File.Exists(scenePath))
            {
                Debug.LogWarning($"[Hearthbound/Phase 53] (skip) {scenePath} not found.");
                return false;
            }
            var scene = OpenScene(scenePath);
            RemoveOld(scene, SysMenuNode, CoordNode, ApplierNode);

            var sysMenu = BuildSystemMenu(scene, includeCustomize: false);

            var coordGO = new GameObject(CoordNode);
            SceneManager.MoveGameObjectToScene(coordGO, scene);
            var coord = coordGO.AddComponent<PolishMenuCoordinator>();
            coord.systemMenu = sysMenu;
            coord.pauseMenu = Object.FindFirstObjectByType<PauseMenuUI>();
            coord.mainMenuScene = "01_MainMenu";

            var applierGO = new GameObject(ApplierNode);
            SceneManager.MoveGameObjectToScene(applierGO, scene);
            applierGO.AddComponent<CharacterAppearanceApplier>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[Hearthbound/Phase 53] Wired {scenePath} (system menu + appearance applier).");
            return true;
        }

        // ───── System menu panel ───────────────────────────────────

        private static SystemMenuUI BuildSystemMenu(UnityEngine.SceneManagement.Scene scene, bool includeCustomize)
        {
            var canvasGO = NewOverlayCanvas(SysMenuNode, 31000);
            SceneManager.MoveGameObjectToScene(canvasGO, scene);
            var ui = canvasGO.AddComponent<SystemMenuUI>();

            var root = Child(canvasGO.transform, "Root"); Stretch(root);
            var backdrop = Child(root, "Backdrop"); Stretch(backdrop);
            AddImage(backdrop, new Color(0.03f, 0.03f, 0.05f, 0.82f), null, true);

            var panel = Child(root, "Panel");
            Center(panel, new Vector2(760, 560));
            AddImage(panel, UIReadabilityHelper.CreamPaper, Panel, true);

            LocalizedTitle(panel, "settings.title", new Vector2(0, -60));

            // Language row.
            LocalizedLabel(panel, "settings.language", new Vector2(0.5f, 1f), new Vector2(0, -150), 30, TextAlignmentOptions.Center);
            var en = TextButton(panel, "settings.english", new Vector2(-150, 60), new Vector2(220, 70));
            var ar = TextButton(panel, "settings.arabic",  new Vector2( 150, 60), new Vector2(220, 70));

            Button customize = null;
            if (includeCustomize)
                customize = TextButton(panel, "settings.customize", new Vector2(0, -40), new Vector2(380, 70));

            var reset = TextButton(panel, "settings.reset", new Vector2(0, includeCustomize ? -130 : -60), new Vector2(380, 70));
            // Reset is a heavier action — tint it warm-ember so it reads as deliberate.
            var resetImg = reset.GetComponent<Image>(); if (resetImg != null) resetImg.color = UIReadabilityHelper.GoldEmber;

            var close = TextButton(panel, "common.close", new Vector2(0, -230), new Vector2(260, 64));

            // In-panel reset confirm overlay.
            var confirm = Child(root, "ResetConfirm"); Stretch(confirm);
            AddImage(confirm, new Color(0.02f, 0.02f, 0.03f, 0.88f), null, true);
            var cPanel = Child(confirm, "Panel"); Center(cPanel, new Vector2(720, 360));
            AddImage(cPanel, UIReadabilityHelper.CreamPaper, Panel, true);
            LocalizedTitle(cPanel, "settings.reset_confirm_title", new Vector2(0, -50));
            LocalizedLabel(cPanel, "settings.reset_confirm_body", new Vector2(0.5f, 0.5f), new Vector2(0, 20), 26, TextAlignmentOptions.Center, 600);
            var yes = TextButton(cPanel, "common.confirm", new Vector2(-160, -120), new Vector2(240, 70));
            var no  = TextButton(cPanel, "common.cancel",  new Vector2( 160, -120), new Vector2(240, 70));
            var yesImg = yes.GetComponent<Image>(); if (yesImg != null) yesImg.color = UIReadabilityHelper.GoldEmber;

            ui.root = root.gameObject;
            ui.englishButton = en;
            ui.arabicButton = ar;
            ui.customizeButton = customize;
            ui.resetButton = reset;
            ui.closeButton = close;
            ui.resetConfirmPanel = confirm.gameObject;
            ui.resetConfirmYes = yes;
            ui.resetConfirmNo = no;

            root.gameObject.SetActive(false);
            return ui;
        }

        // ───── Character creator panel ─────────────────────────────

        private static CharacterCreationUI BuildCharacterCreator(UnityEngine.SceneManagement.Scene scene)
        {
            var canvasGO = NewOverlayCanvas(CreatorNode, 31500);
            SceneManager.MoveGameObjectToScene(canvasGO, scene);
            var ui = canvasGO.AddComponent<CharacterCreationUI>();
            var cg = canvasGO.AddComponent<CanvasGroup>();

            var root = Child(canvasGO.transform, "Root"); Stretch(root);
            var backdrop = Child(root, "Backdrop"); Stretch(backdrop);
            AddImage(backdrop, new Color(0.04f, 0.03f, 0.06f, 0.88f), null, true);

            var panel = Child(root, "Panel"); Center(panel, new Vector2(1120, 780));
            AddImage(panel, UIReadabilityHelper.CreamPaper, Panel, true);

            LocalizedTitle(panel, "char.title", new Vector2(0, -56));

            // Name row.
            LocalizedLabel(panel, "char.name", new Vector2(0f, 1f), new Vector2(90, -150), 26, TextAlignmentOptions.Left);
            var nameField = MakeInputField(panel, "NameInput", new Vector2(0.5f, 1f), new Vector2(0, -200), new Vector2(520, 64),
                SettingsService.DefaultPlayerName);

            // Skin row.
            LocalizedLabel(panel, "char.skin", new Vector2(0f, 1f), new Vector2(90, -290), 26, TextAlignmentOptions.Left);
            var skin = SwatchRow(panel, CharacterPalette.Skin, new Vector2(0, -340));

            // Outfit row.
            LocalizedLabel(panel, "char.outfit", new Vector2(0f, 1f), new Vector2(90, -430), 26, TextAlignmentOptions.Left);
            var outfit = SwatchRow(panel, CharacterPalette.Outfit, new Vector2(0, -480));

            // Accessory row (text buttons).
            LocalizedLabel(panel, "char.accessory", new Vector2(0f, 1f), new Vector2(90, -570), 26, TextAlignmentOptions.Left);
            string[] accKeys = { "char.acc_none", "char.acc_cap", "char.acc_flower", "char.acc_scarf" };
            var acc = new Button[accKeys.Length];
            float ax = -300f;
            for (int i = 0; i < accKeys.Length; i++)
            {
                acc[i] = TextButton(panel, accKeys[i], new Vector2(ax, -610), new Vector2(180, 60), anchorTopCenter: true);
                ax += 200f;
            }

            // Preview swatches (top-right).
            var prevOutfit = Child(panel, "PreviewOutfit");
            prevOutfit.anchorMin = prevOutfit.anchorMax = new Vector2(1f, 1f); prevOutfit.pivot = new Vector2(1f, 1f);
            prevOutfit.sizeDelta = new Vector2(150, 150); prevOutfit.anchoredPosition = new Vector2(-60, -150);
            var prevOutfitImg = AddImage(prevOutfit, CharacterPalette.Outfit[0], Button9, false);
            var prevSkin = Child(prevOutfit, "PreviewSkin");
            prevSkin.anchorMin = prevSkin.anchorMax = new Vector2(0.5f, 0f); prevSkin.pivot = new Vector2(0.5f, 1f);
            prevSkin.sizeDelta = new Vector2(70, 70); prevSkin.anchoredPosition = new Vector2(0, -8);
            var prevSkinImg = AddImage(prevSkin, CharacterPalette.Skin[2], Button9, false);

            // Action buttons.
            var random = TextButton(panel, "char.random", new Vector2(-180, -700), new Vector2(300, 76), anchorTopCenter: true);
            var begin  = TextButton(panel, "char.begin",  new Vector2( 180, -700), new Vector2(300, 76), anchorTopCenter: true);
            var beginImg = begin.GetComponent<Image>(); if (beginImg != null) beginImg.color = UIReadabilityHelper.GoldEmber;

            ui.root = root.gameObject;
            ui.canvasGroup = cg;
            ui.skinSwatches = skin;
            ui.outfitSwatches = outfit;
            ui.accessoryButtons = acc;
            ui.previewOutfit = prevOutfitImg;
            ui.previewSkin = prevSkinImg;
            ui.nameInput = nameField;
            ui.beginButton = begin;
            ui.randomButton = random;

            root.gameObject.SetActive(false);
            return ui;
        }

        private static Button[] SwatchRow(RectTransform panel, Color[] colors, Vector2 topCenterPos)
        {
            var arr = new Button[colors.Length];
            float w = 88f, gap = 14f;
            float total = colors.Length * w + (colors.Length - 1) * gap;
            float x = -total / 2f + w / 2f;
            for (int i = 0; i < colors.Length; i++)
            {
                var rt = Child(panel, $"Swatch{i}");
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f); rt.pivot = new Vector2(0.5f, 1f);
                rt.sizeDelta = new Vector2(w, 64);
                rt.anchoredPosition = new Vector2(x, topCenterPos.y);
                AddImage(rt, colors[i], Button9, true);
                arr[i] = rt.gameObject.AddComponent<Button>();
                x += w + gap;
            }
            return arr;
        }

        // ───── UI primitives ───────────────────────────────────────

        private static GameObject NewOverlayCanvas(string name, int sortingOrder)
        {
            var go = new GameObject(name);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            go.AddComponent<GraphicRaycaster>();
            return go;
        }

        private static RectTransform Child(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static Image AddImage(RectTransform rt, Color color, Sprite sprite, bool raycast)
        {
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            if (sprite != null) { img.sprite = sprite; img.type = Image.Type.Sliced; }
            img.raycastTarget = raycast;
            return img;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        private static void Center(RectTransform rt, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size; rt.anchoredPosition = Vector2.zero;
        }

        private static TextMeshProUGUI LocalizedTitle(RectTransform panel, string key, Vector2 topPos)
        {
            var t = LocalizedLabel(panel, key, new Vector2(0.5f, 1f), topPos, 44, TextAlignmentOptions.Center, 980);
            t.fontStyle = FontStyles.Bold;
            t.color = UIReadabilityHelper.InkDark;
            return t;
        }

        private static TextMeshProUGUI LocalizedLabel(RectTransform panel, string key, Vector2 anchor,
            Vector2 pos, float size, TextAlignmentOptions align, float width = 520)
        {
            var rt = Child(panel, $"L_{key}");
            rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
            rt.sizeDelta = new Vector2(width, 70);
            rt.anchoredPosition = pos;
            var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = size; tmp.color = UIReadabilityHelper.InkDark; tmp.alignment = align;
            tmp.enableWordWrapping = true;
            tmp.text = LocalizationService.Get(key);
            var loc = rt.gameObject.AddComponent<LocalizedText>();
            loc.key = key;
            return tmp;
        }

        private static Button TextButton(RectTransform panel, string key, Vector2 pos, Vector2 size, bool anchorTopCenter = false)
        {
            var rt = Child(panel, $"B_{key}");
            if (anchorTopCenter) { rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f); rt.pivot = new Vector2(0.5f, 1f); }
            else { rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f); }
            rt.sizeDelta = size; rt.anchoredPosition = pos;
            AddImage(rt, UIReadabilityHelper.CreamPaper, Button9, true);
            var btn = rt.gameObject.AddComponent<Button>();

            var labelRT = Child(rt, "Label"); Stretch(labelRT);
            var tmp = labelRT.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 26; tmp.color = UIReadabilityHelper.InkDark;
            tmp.alignment = TextAlignmentOptions.Center; tmp.enableWordWrapping = true;
            tmp.text = LocalizationService.Get(key);
            var loc = labelRT.gameObject.AddComponent<LocalizedText>();
            loc.key = key;
            return btn;
        }

        private static TMP_InputField MakeInputField(RectTransform panel, string name, Vector2 anchor,
            Vector2 pos, Vector2 size, string initial)
        {
            var rt = Child(panel, name);
            rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
            rt.sizeDelta = size; rt.anchoredPosition = pos;
            AddImage(rt, UIReadabilityHelper.CreamBright, Button9, true);
            var input = rt.gameObject.AddComponent<TMP_InputField>();

            var area = Child(rt, "Text Area"); Stretch(area);
            area.offsetMin = new Vector2(14, 8); area.offsetMax = new Vector2(-14, -8);
            area.gameObject.AddComponent<RectMask2D>();

            var placeholder = Child(area, "Placeholder"); Stretch(placeholder);
            var ph = placeholder.gameObject.AddComponent<TextMeshProUGUI>();
            ph.fontSize = 26; ph.fontStyle = FontStyles.Italic;
            ph.color = new Color(0.4f, 0.32f, 0.22f, 0.6f);
            ph.alignment = TextAlignmentOptions.Left; ph.text = SettingsService.DefaultPlayerName;

            var textRT = Child(area, "Text"); Stretch(textRT);
            var txt = textRT.gameObject.AddComponent<TextMeshProUGUI>();
            txt.fontSize = 26; txt.color = UIReadabilityHelper.InkDark;
            txt.alignment = TextAlignmentOptions.Left;

            input.textViewport = area;
            input.textComponent = txt;
            input.placeholder = ph;
            input.lineType = TMP_InputField.LineType.SingleLine;
            input.characterLimit = 18;
            input.text = initial;
            return input;
        }

        private static void RemoveOld(UnityEngine.SceneManagement.Scene scene, params string[] names)
        {
            foreach (var root in scene.GetRootGameObjects())
                foreach (var n in names)
                    if (root.name == n) { Object.DestroyImmediate(root); break; }
        }

        private static UnityEngine.SceneManagement.Scene OpenScene(string path)
        {
            var scene = EditorSceneManager.GetSceneByPath(path);
            if (!scene.IsValid() || !scene.isLoaded)
                scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            return scene;
        }
    }
}
