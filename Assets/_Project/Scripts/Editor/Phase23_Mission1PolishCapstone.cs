// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase23_Mission1PolishCapstone
//
// THE M1 POLISH CAPSTONE. One menu click that turns the engineering-grade
// Phase 22 build into a *polished, fully player-facing* Mission 1+2 game:
//
//   1. Runs every Phase 13..24 builder in sequence (idempotent).
//   2. Re-runs HearthboundOneClickSetup + Phase 22 to refresh the base scenes
//      with every phase output applied.
//   3. Post-processes every scene to add the new polish layer:
//        BOOTSTRAP   — SettingsService registered as a service
//        MAIN MENU   — Settings panel + ComfortToolsMenu + MainMenuSaveCoordinator
//                       + Continue button enabled if autosave exists
//        LANE        — AmbientAudio + MissionTitleCard + PauseMenu +
//                       HelpOverlay + PauseSaveCoordinator
//        HOLLOW      — AmbientAudio + MissionTitleCard + PauseMenu +
//                       HelpOverlay + PauseSaveCoordinator + Pickle the cat
//                       + Mission01 hand-off → Mission02 Garden
//        GARDEN      — AmbientAudio + PauseMenu + HelpOverlay + PauseSaveCoordinator
//        COTTAGE     — AmbientAudio + PauseMenu + HelpOverlay + PauseSaveCoordinator
//   4. Sets EveryScene → Build Settings with stable indices.
//   5. Opens 00_Bootstrap so the user can press Play immediately.
//
// USE: Menu → Hearthbound → ⚙️ Advanced → 🎮 Build POLISHED Mission 1 + 2 (Phase 23)
//
// Demoted to ⚙️ Advanced/… in Phase 32 (menu collapse). The user-facing
// entry point is now `Hearthbound → 🚀 Build Everything`, which chains
// Phase 23 internally. See D-051 in Docs/PROGRESS.md.

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using HearthboundHollow.Audio;
using HearthboundHollow.Core;
using HearthboundHollow.Mission;
using HearthboundHollow.UI;

namespace HearthboundHollow.EditorTools
{
    public static class Phase23_Mission1PolishCapstone
    {
        // ─── Paths ────────────────────────────────────────────────

        private const string ScenesDir = "Assets/_Project/Scenes";
        private const string SceneBootstrap  = ScenesDir + "/00_Bootstrap.unity";
        private const string SceneMainMenu   = ScenesDir + "/01_MainMenu.unity";
        private const string SceneLane       = ScenesDir + "/02_Mission01_Lane.unity";
        private const string SceneHollow     = ScenesDir + "/03_Mission01_Hollow.unity";
        private const string SceneGarden     = ScenesDir + "/04_Mission02_Garden.unity";
        private const string SceneCottage    = ScenesDir + "/05_Mission02_Cottage.unity";

        // ─── Master menu ──────────────────────────────────────────

        [MenuItem("Hearthbound/⚙️ Advanced/\ud83c\udfae Build POLISHED Mission 1 + 2 (Phase 23)", priority = -1)]
        public static void Build()
        {
            EditorUtility.DisplayProgressBar("Hearthbound · Phase 23", "Running Phase 22 (base + polish)…", 0.10f);
            try
            {
                // Step 1: Run Phase 22 — this in turn runs all Phase 13..21 builders
                // and rebuilds the 4 base scenes (Bootstrap / MainMenu / Lane / Hollow).
                Phase22_PolishedPlayableMission1.Build();

                // Step 2: Run Phase 24 — builds the Garden + Cottage scenes.
                EditorUtility.DisplayProgressBar("Hearthbound · Phase 23", "Running Phase 24 (Mission 2 scenes)…", 0.45f);
                Phase24_Mission2SceneBuilder.Build();

                // Step 3: Post-process every scene to add the polish layer.
                EditorUtility.DisplayProgressBar("Hearthbound · Phase 23", "Polishing 00_Bootstrap…", 0.55f);
                PolishBootstrap();

                EditorUtility.DisplayProgressBar("Hearthbound · Phase 23", "Polishing 01_MainMenu…", 0.62f);
                PolishMainMenu();

                EditorUtility.DisplayProgressBar("Hearthbound · Phase 23", "Polishing 02_Mission01_Lane…", 0.72f);
                PolishLaneScene();

                EditorUtility.DisplayProgressBar("Hearthbound · Phase 23", "Polishing 03_Mission01_Hollow…", 0.80f);
                PolishHollowScene();

                EditorUtility.DisplayProgressBar("Hearthbound · Phase 23", "Polishing 04_Mission02_Garden…", 0.88f);
                PolishGardenScene();

                EditorUtility.DisplayProgressBar("Hearthbound · Phase 23", "Polishing 05_Mission02_Cottage…", 0.94f);
                PolishCottageScene();

                EditorUtility.DisplayProgressBar("Hearthbound · Phase 23", "Updating Build Settings…", 0.97f);
                UpdateBuildSettings();

                EditorSceneManager.OpenScene(SceneBootstrap, OpenSceneMode.Single);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            EditorUtility.DisplayDialog(
                "Hearthbound — POLISHED Mission 1 + 2 built",
                BuildSummary(),
                "OK");
        }

        // ─── Bootstrap polish ────────────────────────────────────

        private static void PolishBootstrap()
        {
            var scene = EditorSceneManager.OpenScene(SceneBootstrap, OpenSceneMode.Single);
            var rootGO = GameObject.Find("_GameRoot");
            if (rootGO == null) { Hh_LogWarn("Bootstrap has no _GameRoot — Phase 12 may not have run."); return; }

            // Add SettingsService bootstrap (if not already present).
            if (rootGO.GetComponent<HearthboundHollow.Core.SettingsServiceBootstrap>() == null)
            {
                rootGO.AddComponent<HearthboundHollow.Core.SettingsServiceBootstrap>();
                Debug.Log("[Hearthbound/Phase 23] Bootstrap: SettingsServiceBootstrap added.");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        // ─── Main menu polish ────────────────────────────────────

        private static void PolishMainMenu()
        {
            var scene = EditorSceneManager.OpenScene(SceneMainMenu, OpenSceneMode.Single);

            var ctrlGO = GameObject.Find("_Controller");
            var ctrl = ctrlGO != null ? ctrlGO.GetComponent<MainMenuController>() : null;
            if (ctrl == null)
            {
                Hh_LogWarn("MainMenu has no MainMenuController — Phase 12 may not have run.");
                EditorSceneManager.SaveScene(scene);
                return;
            }

            // Build ComfortToolsMenu inside a Settings panel.
            // NOTE Phase 25 hotfix: BuildSettingsPanel now returns the
            // always-active script-HOST. Its visual CHILD is already hidden.
            // Do NOT deactivate the host or ComfortToolsMenu.Show() couldn't
            // re-light its visual without self-heal. (Self-heal is in place
            // as belt-and-braces, but the wiring is correct without it now.)
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                var settingsPanel = BuildSettingsPanel(canvas.transform, out var comfort);
                ctrl.settingsPanel = settingsPanel;
                ctrl.comfortToolsMenu = comfort;
            }

            // Attach the save coordinator.
            if (ctrlGO.GetComponent<MainMenuSaveCoordinator>() == null)
            {
                var coord = ctrlGO.AddComponent<MainMenuSaveCoordinator>();
                coord.menuController = ctrl;
                coord.fallbackContinueScene = "02_Mission01_Lane";
                Debug.Log("[Hearthbound/Phase 23] MainMenu: MainMenuSaveCoordinator wired.");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        // ─── Lane polish ─────────────────────────────────────────

        private static void PolishLaneScene()
        {
            var scene = EditorSceneManager.OpenScene(SceneLane, OpenSceneMode.Single);
            AddAmbientAudio("ambient_autumn_loop", 0.4f);
            AddPauseAndHelpUI();
            AddMissionTitleCard("Day 1 — Opening the Hollow", "Warm, slightly dusty, late afternoon light.",
                "Assets/_Project/ScriptableObjects/Missions/Mission01_OpeningTheHollow.asset");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        // ─── Hollow polish ───────────────────────────────────────

        private static void PolishHollowScene()
        {
            var scene = EditorSceneManager.OpenScene(SceneHollow, OpenSceneMode.Single);
            AddAmbientAudio("ambient_autumn_loop", 0.3f);
            AddPauseAndHelpUI();
            AddMissionTitleCard("Day 1 — Inside the Hollow", "Warm, slightly dusty, late afternoon light.",
                "Assets/_Project/ScriptableObjects/Missions/Mission01_OpeningTheHollow.asset");

            // Route Mission 1 end → Mission 2 Garden (the polish hand-off).
            var dir = Object.FindFirstObjectByType<Mission01Director>();
            if (dir != null)
            {
                dir.sceneAfterEndOfDay = "04_Mission02_Garden";
                Debug.Log("[Hearthbound/Phase 23] Hollow: Mission01 end-of-day routed to Mission 2 Garden.");
                EditorUtility.SetDirty(dir);
            }

            // Pickle the cat companion (uses PickleAI).
            AddPickleCat();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        // ─── Garden polish ───────────────────────────────────────

        private static void PolishGardenScene()
        {
            if (!System.IO.File.Exists(SceneGarden)) { Hh_LogWarn("Garden scene missing — skipping polish."); return; }
            var scene = EditorSceneManager.OpenScene(SceneGarden, OpenSceneMode.Single);
            AddAmbientAudio("ambient_autumn_loop", 0.45f);
            AddPauseAndHelpUI();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        // ─── Cottage polish ──────────────────────────────────────

        private static void PolishCottageScene()
        {
            if (!System.IO.File.Exists(SceneCottage)) { Hh_LogWarn("Cottage scene missing — skipping polish."); return; }
            var scene = EditorSceneManager.OpenScene(SceneCottage, OpenSceneMode.Single);
            AddAmbientAudio("ambient_autumn_loop", 0.25f); // hearth-quiet
            AddPauseAndHelpUI();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        // ─── Shared builders ─────────────────────────────────────

        private static void AddAmbientAudio(string libId, float baseVol)
        {
            if (Object.FindFirstObjectByType<AmbientAudio>() != null) return;
            var go = new GameObject("_AmbientAudio");
            var src = go.AddComponent<AudioSource>();
            src.loop = true; src.playOnAwake = false; src.spatialBlend = 0f;
            var ambient = go.AddComponent<AmbientAudio>();
            ambient.library = Phase18_AudioBuilder.TryGetLibrary();
            ambient.libraryEntryId = libId;
            ambient.baseVolume = baseVol;
            ambient.fadeInSeconds = 2.5f;
            Debug.Log($"[Hearthbound/Phase 23] AmbientAudio added (id={libId}, base={baseVol}).");
        }

        private static void AddPauseAndHelpUI()
        {
            // Find the existing canvas; create one if absent.
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                var canvasGO = new GameObject("UI_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                var c = canvasGO.GetComponent<Canvas>();
                c.renderMode = RenderMode.ScreenSpaceOverlay;
                var s = canvasGO.GetComponent<CanvasScaler>();
                s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                s.referenceResolution = new Vector2(1920, 1080);
                canvas = c;
            }

            // Pause menu.
            PauseMenuUI pause = Object.FindFirstObjectByType<PauseMenuUI>();
            if (pause == null)
            {
                pause = BuildPauseMenu(canvas.transform);
            }

            // Pause save coordinator (lives on the same GameObject).
            if (pause != null && pause.GetComponent<PauseSaveCoordinator>() == null)
            {
                var coord = pause.gameObject.AddComponent<PauseSaveCoordinator>();
                coord.pauseMenu = pause;
            }

            // Help overlay.
            if (Object.FindFirstObjectByType<HelpOverlayUI>() == null)
            {
                BuildHelpOverlay(canvas.transform);
            }

            // Ensure event system.
            EnsureEventSystem();
        }

        // ── Phase 25 hotfix ─────────────────────────────────────────
        // Two-layer pattern: the script-host GameObject stays ACTIVE so
        // Update() listens for Escape; a child "Visual" GameObject carries
        // the visible UI and is what Pause()/Resume() toggle. Previously
        // we deactivated the host, which silently killed Esc handling.
        private static PauseMenuUI BuildPauseMenu(Transform canvas)
        {
            // 1) Script-host — always active, empty RectTransform spanning the canvas.
            var hostGO = new GameObject("PauseMenu", typeof(RectTransform));
            hostGO.transform.SetParent(canvas, false);
            var hostRT = hostGO.GetComponent<RectTransform>();
            hostRT.anchorMin = Vector2.zero; hostRT.anchorMax = Vector2.one;
            hostRT.offsetMin = Vector2.zero; hostRT.offsetMax = Vector2.zero;

            // 2) Visual root — CHILD of host, toggled by Pause()/Resume().
            var visualGO = new GameObject("Visual", typeof(RectTransform));
            visualGO.transform.SetParent(hostGO.transform, false);
            var bg = visualGO.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.04f, 0.03f, 0.78f);
            var bgRT = bg.rectTransform;
            bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero;

            var panel = new GameObject("Panel", typeof(RectTransform));
            panel.transform.SetParent(visualGO.transform, false);
            var pImg = panel.AddComponent<Image>();
            pImg.color = new Color(0.10f, 0.08f, 0.06f, 0.96f);
            var pRT = pImg.rectTransform;
            pRT.anchorMin = new Vector2(0.32f, 0.20f); pRT.anchorMax = new Vector2(0.68f, 0.80f);
            pRT.offsetMin = Vector2.zero; pRT.offsetMax = Vector2.zero;

            var title = new GameObject("Title").AddComponent<TextMeshProUGUI>();
            title.transform.SetParent(panel.transform, false);
            title.fontSize = 48; title.alignment = TextAlignmentOptions.Center; title.color = new Color(0.97f, 0.85f, 0.62f);
            title.rectTransform.anchorMin = new Vector2(0, 0.82f); title.rectTransform.anchorMax = new Vector2(1, 0.95f);
            title.text = "Paused";
            UIAutoFitText.ApplyToButtonLabel(title, minSize: 28, maxSize: 56);

            var hint = new GameObject("Hint").AddComponent<TextMeshProUGUI>();
            hint.transform.SetParent(panel.transform, false);
            hint.fontSize = 20; hint.fontStyle = FontStyles.Italic;
            hint.alignment = TextAlignmentOptions.Center; hint.color = new Color(0.86f, 0.78f, 0.66f);
            hint.rectTransform.anchorMin = new Vector2(0, 0.72f); hint.rectTransform.anchorMax = new Vector2(1, 0.81f);
            hint.text = "Take a breath. The Hollow will wait.";
            UIAutoFitText.ApplyToLabel(hint, minSize: 14, maxSize: 22);

            var resume = MakeButton(panel.transform, "Btn_Resume",          "Resume",
                new Vector2(0.10f, 0.56f), new Vector2(0.90f, 0.66f));
            var settings = MakeButton(panel.transform, "Btn_Settings",      "Settings",
                new Vector2(0.10f, 0.43f), new Vector2(0.90f, 0.53f));
            var saveQuit = MakeButton(panel.transform, "Btn_SaveAndQuit",   "Save & Quit to Main Menu",
                new Vector2(0.10f, 0.30f), new Vector2(0.90f, 0.40f));
            var quit = MakeButton(panel.transform, "Btn_QuitToDesktop",     "Quit to Desktop",
                new Vector2(0.10f, 0.17f), new Vector2(0.90f, 0.27f));

            // Script lives on the HOST (always active). Wire `root` → visual child.
            var pause = hostGO.AddComponent<PauseMenuUI>();
            pause.root = visualGO;
            pause.titleLabel = title;
            pause.hintLabel = hint;
            pause.resumeButton = resume;
            pause.settingsButton = settings;
            pause.saveAndQuitButton = saveQuit;
            pause.quitToDesktopButton = quit;
            pause.mainMenuSceneName = "01_MainMenu";

            // Hide ONLY the visual child — host stays active so Update() runs.
            visualGO.SetActive(false);
            return pause;
        }

        // ── Phase 25 hotfix ─────────────────────────────────────────
        // Two-layer pattern (see BuildPauseMenu comment) so the H key keeps
        // working while the overlay is hidden.
        private static HelpOverlayUI BuildHelpOverlay(Transform canvas)
        {
            var hostGO = new GameObject("HelpOverlay", typeof(RectTransform));
            hostGO.transform.SetParent(canvas, false);
            var hostRT = hostGO.GetComponent<RectTransform>();
            hostRT.anchorMin = Vector2.zero; hostRT.anchorMax = Vector2.one;
            hostRT.offsetMin = Vector2.zero; hostRT.offsetMax = Vector2.zero;

            var visualGO = new GameObject("Visual", typeof(RectTransform));
            visualGO.transform.SetParent(hostGO.transform, false);
            var bg = visualGO.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.04f, 0.03f, 0.82f);
            var bgRT = bg.rectTransform;
            bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero;

            var panel = new GameObject("Panel", typeof(RectTransform));
            panel.transform.SetParent(visualGO.transform, false);
            var pImg = panel.AddComponent<Image>();
            pImg.color = new Color(0.10f, 0.08f, 0.06f, 0.96f);
            var pRT = pImg.rectTransform;
            pRT.anchorMin = new Vector2(0.18f, 0.14f); pRT.anchorMax = new Vector2(0.82f, 0.86f);
            pRT.offsetMin = Vector2.zero; pRT.offsetMax = Vector2.zero;

            var title = new GameObject("Title").AddComponent<TextMeshProUGUI>();
            title.transform.SetParent(panel.transform, false);
            title.fontSize = 36; title.alignment = TextAlignmentOptions.Center; title.color = new Color(0.97f, 0.85f, 0.62f);
            title.rectTransform.anchorMin = new Vector2(0, 0.85f); title.rectTransform.anchorMax = new Vector2(1, 0.95f);
            title.text = "Welcome to the Hollow";
            UIAutoFitText.ApplyToButtonLabel(title, minSize: 22, maxSize: 40);

            var subtitle = new GameObject("Subtitle").AddComponent<TextMeshProUGUI>();
            subtitle.transform.SetParent(panel.transform, false);
            subtitle.fontSize = 20; subtitle.fontStyle = FontStyles.Italic;
            subtitle.alignment = TextAlignmentOptions.Center; subtitle.color = new Color(0.86f, 0.78f, 0.66f);
            subtitle.rectTransform.anchorMin = new Vector2(0, 0.78f); subtitle.rectTransform.anchorMax = new Vector2(1, 0.84f);
            subtitle.text = "A quick word from Marin's notes …";
            UIAutoFitText.ApplyToLabel(subtitle, minSize: 14, maxSize: 22);

            var body = new GameObject("Body").AddComponent<TextMeshProUGUI>();
            body.transform.SetParent(panel.transform, false);
            body.fontSize = 20; body.alignment = TextAlignmentOptions.TopLeft; body.color = new Color(0.92f, 0.88f, 0.78f);
            body.rectTransform.anchorMin = new Vector2(0.10f, 0.18f); body.rectTransform.anchorMax = new Vector2(0.90f, 0.76f);
            UIAutoFitText.ApplyToLabel(body, minSize: 14, maxSize: 24);

            var close = MakeButton(panel.transform, "Btn_Close", "Close (H)",
                new Vector2(0.35f, 0.06f), new Vector2(0.65f, 0.14f));

            var help = hostGO.AddComponent<HelpOverlayUI>();
            help.root = visualGO;
            help.titleLabel = title;
            help.subtitleLabel = subtitle;
            help.bodyText = body;
            help.closeButton = close;
            help.toggleKey = KeyCode.H;
            visualGO.SetActive(false);
            return help;
        }

        // ── Phase 25 hotfix ─────────────────────────────────────────
        // Two-layer pattern. The host stays active so ComfortToolsMenu.Show()
        // can run hot-refresh logic safely.
        private static GameObject BuildSettingsPanel(Transform canvas, out ComfortToolsMenu comfort)
        {
            var hostGO = new GameObject("SettingsPanel", typeof(RectTransform));
            hostGO.transform.SetParent(canvas, false);
            var hostRT = hostGO.GetComponent<RectTransform>();
            hostRT.anchorMin = Vector2.zero; hostRT.anchorMax = Vector2.one;
            hostRT.offsetMin = Vector2.zero; hostRT.offsetMax = Vector2.zero;

            var visualGO = new GameObject("Visual", typeof(RectTransform));
            visualGO.transform.SetParent(hostGO.transform, false);
            var bg = visualGO.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.04f, 0.03f, 0.92f);
            var bgRT = bg.rectTransform;
            bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero;

            var panel = new GameObject("Panel", typeof(RectTransform));
            panel.transform.SetParent(visualGO.transform, false);
            var pImg = panel.AddComponent<Image>();
            pImg.color = new Color(0.12f, 0.10f, 0.08f, 0.98f);
            var pRT = pImg.rectTransform;
            pRT.anchorMin = new Vector2(0.20f, 0.10f); pRT.anchorMax = new Vector2(0.80f, 0.90f);
            pRT.offsetMin = Vector2.zero; pRT.offsetMax = Vector2.zero;

            var title = new GameObject("Title").AddComponent<TextMeshProUGUI>();
            title.transform.SetParent(panel.transform, false);
            title.fontSize = 44; title.alignment = TextAlignmentOptions.Center; title.color = new Color(0.97f, 0.85f, 0.62f);
            title.rectTransform.anchorMin = new Vector2(0, 0.88f); title.rectTransform.anchorMax = new Vector2(1, 0.96f);
            title.text = "Settings · Comfort Tools";

            // Gentle mode toggle
            var gentleToggle = BuildToggle(panel.transform, "Gentle Mode (longer timers, no fail states)",
                new Vector2(0.08f, 0.78f), new Vector2(0.92f, 0.84f));

            // Auto-complete polish toggle
            var autoPolish = BuildToggle(panel.transform, "Auto-Complete Polish mini-game",
                new Vector2(0.08f, 0.70f), new Vector2(0.92f, 0.76f));

            // Auto-complete cleanse toggle
            var autoCleanse = BuildToggle(panel.transform, "Auto-Complete Cleanse mini-game",
                new Vector2(0.08f, 0.62f), new Vector2(0.92f, 0.68f));

            // Subtitle size slider
            var subtitleSlider = BuildSlider(panel.transform, "Subtitle size",
                new Vector2(0.08f, 0.50f), new Vector2(0.92f, 0.58f),
                min: 0, max: 3, value: 1, out var subtitleLabel);

            // Close button — hides only the visual child; the script-host
            // stays active so the next Show() call can light it back up.
            var close = MakeButton(panel.transform, "Btn_CloseSettings", "Close",
                new Vector2(0.35f, 0.05f), new Vector2(0.65f, 0.13f));
            close.onClick.AddListener(() => visualGO.SetActive(false));

            comfort = hostGO.AddComponent<ComfortToolsMenu>();
            comfort.root = visualGO;
            comfort.gentleMode = gentleToggle;
            comfort.autoCompletePolish = autoPolish;
            comfort.autoCompleteCleanse = autoCleanse;
            comfort.subtitleSize = subtitleSlider;
            comfort.subtitleSizeLabel = subtitleLabel;

            // Hide visual; host stays active.
            visualGO.SetActive(false);
            return hostGO;
        }

        private static Toggle BuildToggle(Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(label, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            var bg = new GameObject("Bg", typeof(Image));
            bg.transform.SetParent(go.transform, false);
            var bgImg = bg.GetComponent<Image>();
            bgImg.color = new Color(0.18f, 0.12f, 0.08f, 0.95f);
            var bgRT = bg.GetComponent<RectTransform>();
            bgRT.anchorMin = new Vector2(0, 0); bgRT.anchorMax = new Vector2(0.08f, 1);
            bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero;

            var check = new GameObject("Check", typeof(Image));
            check.transform.SetParent(bg.transform, false);
            var checkImg = check.GetComponent<Image>();
            checkImg.color = new Color(0.92f, 0.70f, 0.34f, 1f);
            var checkRT = check.GetComponent<RectTransform>();
            checkRT.anchorMin = new Vector2(0.15f, 0.15f); checkRT.anchorMax = new Vector2(0.85f, 0.85f);
            checkRT.offsetMin = Vector2.zero; checkRT.offsetMax = Vector2.zero;

            var labelGO = new GameObject("Label").AddComponent<TextMeshProUGUI>();
            labelGO.transform.SetParent(go.transform, false);
            labelGO.fontSize = 20; labelGO.alignment = TextAlignmentOptions.Left; labelGO.color = new Color(0.92f, 0.88f, 0.78f);
            labelGO.rectTransform.anchorMin = new Vector2(0.10f, 0); labelGO.rectTransform.anchorMax = new Vector2(1, 1);
            labelGO.text = label;
            UIAutoFitText.ApplyToLabel(labelGO, minSize: 13, maxSize: 22);

            var toggle = go.AddComponent<Toggle>();
            toggle.targetGraphic = bgImg;
            toggle.graphic = checkImg;
            toggle.isOn = false;
            return toggle;
        }

        private static Slider BuildSlider(Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax,
            float min, float max, float value, out TextMeshProUGUI valueLabel)
        {
            var go = new GameObject(label, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            var labelGO = new GameObject("Label").AddComponent<TextMeshProUGUI>();
            labelGO.transform.SetParent(go.transform, false);
            labelGO.fontSize = 20; labelGO.alignment = TextAlignmentOptions.Left; labelGO.color = new Color(0.92f, 0.88f, 0.78f);
            labelGO.rectTransform.anchorMin = new Vector2(0, 0.5f); labelGO.rectTransform.anchorMax = new Vector2(0.40f, 1);
            labelGO.text = label;
            UIAutoFitText.ApplyToLabel(labelGO, minSize: 13, maxSize: 22);

            var sliderGO = new GameObject("Slider", typeof(Slider), typeof(Image));
            sliderGO.transform.SetParent(go.transform, false);
            var img = sliderGO.GetComponent<Image>();
            img.color = new Color(0.20f, 0.14f, 0.10f, 0.9f);
            var sRT = sliderGO.GetComponent<RectTransform>();
            sRT.anchorMin = new Vector2(0.40f, 0.25f); sRT.anchorMax = new Vector2(0.92f, 0.75f);
            sRT.offsetMin = Vector2.zero; sRT.offsetMax = Vector2.zero;
            var slider = sliderGO.GetComponent<Slider>();
            slider.minValue = min; slider.maxValue = max; slider.value = value;

            var vlGO = new GameObject("Value").AddComponent<TextMeshProUGUI>();
            vlGO.transform.SetParent(go.transform, false);
            vlGO.fontSize = 20; vlGO.alignment = TextAlignmentOptions.Right; vlGO.color = new Color(0.97f, 0.85f, 0.62f);
            vlGO.rectTransform.anchorMin = new Vector2(0.93f, 0.25f); vlGO.rectTransform.anchorMax = new Vector2(1f, 0.75f);
            vlGO.text = "Medium";
            valueLabel = vlGO;
            return slider;
        }

        // ── Phase 25 hotfix ─────────────────────────────────────────
        // Same two-layer pattern as the rest. The script-host MUST remain
        // active so Start()/Play() runs the fade-in coroutine.
        private static void AddMissionTitleCard(string title, string tone, string missionSoPath)
        {
            if (Object.FindFirstObjectByType<MissionTitleCard>() != null) return;
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            // Host — always active, hosts the script and the CanvasGroup.
            var hostGO = new GameObject("MissionTitleCard", typeof(RectTransform));
            hostGO.transform.SetParent(canvas.transform, false);
            var hostRT = hostGO.GetComponent<RectTransform>();
            hostRT.anchorMin = Vector2.zero; hostRT.anchorMax = Vector2.one;
            hostRT.offsetMin = Vector2.zero; hostRT.offsetMax = Vector2.zero;
            var cg = hostGO.AddComponent<CanvasGroup>();

            // Visual child — toggled on by Play(), off by the fade-out tail.
            var visualGO = new GameObject("Visual", typeof(RectTransform));
            visualGO.transform.SetParent(hostGO.transform, false);
            var visualRT = visualGO.GetComponent<RectTransform>();
            visualRT.anchorMin = Vector2.zero; visualRT.anchorMax = Vector2.one;
            visualRT.offsetMin = Vector2.zero; visualRT.offsetMax = Vector2.zero;

            var bg = new GameObject("Vignette").AddComponent<Image>();
            bg.transform.SetParent(visualGO.transform, false);
            bg.color = new Color(0.05f, 0.04f, 0.03f, 0.85f);
            var bgRT = bg.rectTransform;
            bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero;

            var day = new GameObject("Day").AddComponent<TextMeshProUGUI>();
            day.transform.SetParent(visualGO.transform, false);
            day.fontSize = 28; day.alignment = TextAlignmentOptions.Center; day.color = new Color(0.86f, 0.78f, 0.66f);
            day.rectTransform.anchorMin = new Vector2(0.2f, 0.62f); day.rectTransform.anchorMax = new Vector2(0.8f, 0.70f);
            UIAutoFitText.ApplyToButtonLabel(day, minSize: 16, maxSize: 32);

            var titleLabel = new GameObject("Title").AddComponent<TextMeshProUGUI>();
            titleLabel.transform.SetParent(visualGO.transform, false);
            titleLabel.fontSize = 56; titleLabel.alignment = TextAlignmentOptions.Center; titleLabel.color = new Color(0.97f, 0.85f, 0.62f);
            titleLabel.rectTransform.anchorMin = new Vector2(0.10f, 0.5f); titleLabel.rectTransform.anchorMax = new Vector2(0.90f, 0.62f);
            titleLabel.text = title;
            UIAutoFitText.ApplyToButtonLabel(titleLabel, minSize: 28, maxSize: 64);

            var toneLabel = new GameObject("Tone").AddComponent<TextMeshProUGUI>();
            toneLabel.transform.SetParent(visualGO.transform, false);
            toneLabel.fontSize = 22; toneLabel.fontStyle = FontStyles.Italic;
            toneLabel.alignment = TextAlignmentOptions.Center; toneLabel.color = new Color(0.86f, 0.78f, 0.66f);
            toneLabel.rectTransform.anchorMin = new Vector2(0.15f, 0.40f); toneLabel.rectTransform.anchorMax = new Vector2(0.85f, 0.48f);
            toneLabel.text = tone;
            UIAutoFitText.ApplyToLabel(toneLabel, minSize: 14, maxSize: 26);

            var card = hostGO.AddComponent<MissionTitleCard>();
            card.root = visualGO;
            card.canvasGroup = cg;
            card.dayLabel = day;
            card.titleLabel = titleLabel;
            card.toneLabel = toneLabel;
            card.mission = AssetDatabase.LoadAssetAtPath<MissionSO>(missionSoPath);
            cg.alpha = 0f;
            // Hide visual until Play() fades it in. Host stays active.
            visualGO.SetActive(false);
        }

        private static void AddPickleCat()
        {
            if (GameObject.Find("Pickle") != null) return;
            // Simple cat placeholder — a half-sphere on the floor. The PickleAI
            // script makes her watch the player + flick her tail.
            var pickle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pickle.name = "Pickle";
            pickle.transform.position = new Vector3(2.0f, 0.25f, 4.5f);
            pickle.transform.localScale = new Vector3(0.55f, 0.4f, 0.7f);

            var matPath = "Assets/_Project/Art/Materials/Mat_Pickle.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (existing == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                existing = new Material(shader);
                existing.SetColor("_BaseColor", new Color(0.25f, 0.22f, 0.20f));
                existing.SetColor("_Color", new Color(0.25f, 0.22f, 0.20f));
                AssetDatabase.CreateAsset(existing, matPath);
            }
            pickle.GetComponent<MeshRenderer>().sharedMaterial = existing;

            var ai = pickle.AddComponent<HearthboundHollow.Mission.PickleAI>();
            // Look-at priorities — find the player + orb if present.
            var playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null) ai.player = playerGO.transform;
            var orb = Object.FindFirstObjectByType<HearthboundHollow.Player.MemoryOrbInteractable>();
            if (orb != null) ai.orbInHand = orb.transform;
            Debug.Log("[Hearthbound/Phase 23] Pickle the cat added to Hollow.");
        }

        // ─── Generic UI helpers ──────────────────────────────────

        private static Button MakeButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax)
        {
            var btnGO = new GameObject(name, typeof(Image), typeof(Button));
            btnGO.transform.SetParent(parent, false);
            var img = btnGO.GetComponent<Image>();
            img.color = new Color(0.18f, 0.12f, 0.08f, 0.95f);
            var rt = img.rectTransform;
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var labelGO = new GameObject("Label").AddComponent<TextMeshProUGUI>();
            labelGO.transform.SetParent(btnGO.transform, false);
            labelGO.fontSize = 22; labelGO.alignment = TextAlignmentOptions.Center; labelGO.color = new Color(0.97f, 0.85f, 0.62f);
            labelGO.text = label;
            var lRT = labelGO.rectTransform;
            lRT.anchorMin = Vector2.zero; lRT.anchorMax = Vector2.one;
            lRT.offsetMin = new Vector2(12, 4); lRT.offsetMax = new Vector2(-12, -4);
            UIAutoFitText.ApplyToButtonLabel(labelGO, minSize: 14, maxSize: 24);
            return btnGO.GetComponent<Button>();
        }

        private static void EnsureEventSystem()
        {
            var existing = Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (existing != null) return;
            var go = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem));
#if ENABLE_INPUT_SYSTEM && UNITY_2020_2_OR_NEWER
            go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            go.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif
        }

        // ─── Build settings + utils ──────────────────────────────

        private static void UpdateBuildSettings()
        {
            var list = new List<EditorBuildSettingsScene>
            {
                new(SceneBootstrap, true),
                new(SceneMainMenu, true),
                new(SceneLane, true),
                new(SceneHollow, true),
            };
            if (System.IO.File.Exists(SceneGarden))  list.Add(new EditorBuildSettingsScene(SceneGarden, true));
            if (System.IO.File.Exists(SceneCottage)) list.Add(new EditorBuildSettingsScene(SceneCottage, true));
            EditorBuildSettings.scenes = list.ToArray();
        }

        private static void Hh_LogWarn(string msg) => Debug.LogWarning("[Hearthbound/Phase 23] " + msg);

        // ─── Summary ─────────────────────────────────────────────

        private static string BuildSummary()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("POLISHED Mission 1 + 2 build complete.\n");
            sb.AppendLine("Scenes (in Build Settings):");
            sb.AppendLine("  0. 00_Bootstrap");
            sb.AppendLine("  1. 01_MainMenu          (Settings panel + Continue wired to autosave)");
            sb.AppendLine("  2. 02_Mission01_Lane    (TitleCard + AmbientAudio + Pause + Help)");
            sb.AppendLine("  3. 03_Mission01_Hollow  (+ Pickle the cat + M1→M2 hand-off)");
            sb.AppendLine("  4. 04_Mission02_Garden  (herb harvest + tea brewing)");
            sb.AppendLine("  5. 05_Mission02_Cottage (Gerrold + 4-tariff choice + Cleanse + Dream 2)\n");
            sb.AppendLine("End-to-end flow:");
            sb.AppendLine("  Bootstrap → MainMenu → Tone Compass (1st time) →");
            sb.AppendLine("  Lane → Hollow → polish Doris's orb → Evening Ledger →");
            sb.AppendLine("  Garden → harvest herb → brew tea → Cottage →");
            sb.AppendLine("  meet Gerrold → moral choice → (cleanse mini-game) →");
            sb.AppendLine("  Dream 2 → Evening Ledger → MainMenu.\n");
            sb.AppendLine("Controls reference (H opens in-game card any time):");
            sb.AppendLine("  Move      WASD / Arrow keys / Gamepad left stick");
            sb.AppendLine("  Interact  E or Gamepad south");
            sb.AppendLine("  Advance   Click / Space / Enter");
            sb.AppendLine("  Polish    Hold LMB and draw slow circles");
            sb.AppendLine("  Pause     Escape");
            sb.AppendLine("  Help      H");
            return sb.ToString();
        }
    }

}
