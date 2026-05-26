// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / HearthboundOneClickSetup
//
// The scene-builder capstone. Builds a buildable, playable Mission 1 vertical
// slice with zero manual scene authoring required by the user.
//
// USE: Menu → Hearthbound → ⚙️ Advanced → Build Playable Mission 1 (One Click)
//
// Demoted to ⚙️ Advanced/… in Phase 32 (menu collapse). The user-facing
// entry point is now `Hearthbound → 🚀 Build Everything`, which chains
// HearthboundOneClickSetup internally via Phase 22 → Phase 23 → Phase 27.
//
// PROGRESSIVE POLISH: The builder detects which phase outputs are present
// and uses them automatically. The same menu produces:
//   * Phase 12 only:  primitives + flat UI (engineering smoke-test)
//   * + Phase 13:      BoZo characters replace capsules/cylinders
//   * + Phase 14:      Bamao parchment UI replaces flat backgrounds
//   * + Phase 15-21:   (future) Medieval Village, Lumen, etc.
//
// The capstone Phase 22 will refactor this into the polished playable;
// for now it's the running smoke-test that grows in polish each pull.

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using TMPro;
using HearthboundHollow.Core;
using HearthboundHollow.Memory;
using HearthboundHollow.MiniGames;
using HearthboundHollow.Mission;
using HearthboundHollow.Player;
using HearthboundHollow.UI;

namespace HearthboundHollow.EditorTools
{
    public static class HearthboundOneClickSetup
    {
        // ─── Paths ─────────────────────────────────────────────────

        private const string ScenesDir = "Assets/_Project/Scenes";
        private const string MaterialsDir = "Assets/_Project/Art/Materials";

        private const string SceneBootstrap = ScenesDir + "/00_Bootstrap.unity";
        private const string SceneMainMenu = ScenesDir + "/01_MainMenu.unity";
        private const string SceneMission01Lane = ScenesDir + "/02_Mission01_Lane.unity";
        private const string SceneMission01Hollow = ScenesDir + "/03_Mission01_Hollow.unity";

        private const string VillageStatePath = "Assets/_Project/ScriptableObjects/State/VillageState.asset";
        private const string DorisPath = "Assets/_Project/ScriptableObjects/Villagers/Doris.asset";
        private const string DorisMemoryPath = "Assets/_Project/ScriptableObjects/Memories/DOR-001_FirstLoaves.asset";

        // ─── Master menu ───────────────────────────────────────────

        [MenuItem("Hearthbound/⚙️ Advanced/Build Playable Mission 1 (One Click)", priority = 1)]
        public static void BuildPlayableMission1()
        {
            EditorUtility.DisplayProgressBar("Hearthbound", "Verifying prerequisites…", 0.05f);
            try
            {
                if (!VerifyURPActive()) return;
                if (!VerifySeedAssets()) return;

                EnsureDir(ScenesDir);
                EnsureDir(MaterialsDir);

                EditorUtility.DisplayProgressBar("Hearthbound", "Building 00_Bootstrap…", 0.15f);
                BuildBootstrapScene();

                EditorUtility.DisplayProgressBar("Hearthbound", "Building 01_MainMenu…", 0.30f);
                BuildMainMenuScene();

                EditorUtility.DisplayProgressBar("Hearthbound", "Building 02_Mission01_Lane…", 0.50f);
                BuildLaneScene();

                EditorUtility.DisplayProgressBar("Hearthbound", "Building 03_Mission01_Hollow…", 0.75f);
                BuildHollowScene();

                EditorUtility.DisplayProgressBar("Hearthbound", "Updating Build Settings…", 0.92f);
                UpdateBuildSettings();

                EditorUtility.DisplayProgressBar("Hearthbound", "Opening Bootstrap…", 0.97f);
                EditorSceneManager.OpenScene(SceneBootstrap, OpenSceneMode.Single);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            var phaseList = new System.Text.StringBuilder();
            phaseList.Append("Phases applied to this build:\n");
            phaseList.Append("  ✓ Phase 12 — base scenes + primitives + flat UI\n");
            if (Phase13_BoZoCharacterBuilder.TryGetPlayerPrefab() != null)
                phaseList.Append("  ✓ Phase 13 — BoZo characters\n");
            else
                phaseList.Append("  ⚠ Phase 13 — BoZo not run yet (using primitive capsule/cylinder placeholders)\n");
            if (Phase14_BamaoUIBuilder.TryGetDialogueBoxPrefab() != null)
                phaseList.Append("  ✓ Phase 14 — Bamao parchment UI\n");
            else
                phaseList.Append("  ⚠ Phase 14 — Bamao UI not run yet (using flat-color UI placeholders)\n");

            EditorUtility.DisplayDialog(
                "Hearthbound — Playable Mission 1 built",
                phaseList +
                "\nAll 4 scenes are in Build Settings (Bootstrap at index 0).\n\n" +
                "Press Play. The MainMenu loads. Click 'Open The Hollow' → walk to Doris → choose a dialogue option → " +
                "walk to the workbench → polish her memory → the Evening Ledger ends Day 1.\n\n" +
                "Controls: WASD / Arrow keys to move. Click / Space / Enter to advance dialogue. " +
                "Hold left mouse and drag in slow circles to polish.",
                "OK");
        }

        // ─── Prereq checks ─────────────────────────────────────────

        private static bool VerifyURPActive()
        {
            if (GraphicsSettings.defaultRenderPipeline is UniversalRenderPipelineAsset) return true;
            if (EditorUtility.DisplayDialog(
                    "Hearthbound — URP not active",
                    "URP isn't currently active. The Playable Mission 1 builder requires URP.\n\n" +
                    "Run 'Hearthbound → Setup URP Pipeline (one-time)' first?",
                    "Set up URP now", "Cancel"))
            {
                URPSetupHelper.SetupURPPipeline();
                return GraphicsSettings.defaultRenderPipeline is UniversalRenderPipelineAsset;
            }
            return false;
        }

        private static bool VerifySeedAssets()
        {
            bool missing =
                AssetDatabase.LoadAssetAtPath<VillageState>(VillageStatePath) == null ||
                AssetDatabase.LoadAssetAtPath<VillagerSO>(DorisPath) == null ||
                AssetDatabase.LoadAssetAtPath<MemoryNodeSO>(DorisMemoryPath) == null;

            if (!missing) return true;

            if (EditorUtility.DisplayDialog(
                    "Hearthbound — Seed assets missing",
                    "Seed ScriptableObjects (VillageState, Doris, DOR-001) aren't yet generated.\n\n" +
                    "Run 'Hearthbound → Create Mission 1-2 Seed Assets' first?",
                    "Generate seeds now", "Cancel"))
            {
                SeedAssetGenerator.CreateAllSeedAssets();
                return AssetDatabase.LoadAssetAtPath<VillageState>(VillageStatePath) != null;
            }
            return false;
        }

        // ─── Material cache ────────────────────────────────────────

        private static Material _matGround, _matPlayer, _matDoris, _matWorkbench, _matOrb;

        private static Material CreateUrpLitMaterial(string assetPath, Color baseColor, float smoothness = 0.2f, Color? emission = null)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (existing != null) return existing;

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("URP/Lit");
            var mat = new Material(shader != null ? shader : Shader.Find("Standard"));
            mat.SetColor("_BaseColor", baseColor);
            mat.SetColor("_Color", baseColor);
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Metallic", 0f);
            if (emission.HasValue)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emission.Value);
            }
            AssetDatabase.CreateAsset(mat, assetPath);
            return mat;
        }

        private static void EnsureMaterialsBuilt()
        {
            if (_matGround    == null) _matGround    = CreateUrpLitMaterial(MaterialsDir + "/Mat_Ground.mat",    new Color(0.32f, 0.25f, 0.20f));
            if (_matPlayer    == null) _matPlayer    = CreateUrpLitMaterial(MaterialsDir + "/Mat_Player.mat",    new Color(0.85f, 0.72f, 0.55f));
            if (_matDoris     == null) _matDoris     = CreateUrpLitMaterial(MaterialsDir + "/Mat_Doris.mat",     new Color(0.62f, 0.82f, 0.55f));
            if (_matWorkbench == null) _matWorkbench = CreateUrpLitMaterial(MaterialsDir + "/Mat_Workbench.mat", new Color(0.40f, 0.28f, 0.18f));
            if (_matOrb       == null) _matOrb       = CreateUrpLitMaterial(MaterialsDir + "/Mat_MemoryOrb.mat", new Color(0.45f, 0.42f, 0.36f), 0.75f, Color.black);
            AssetDatabase.SaveAssets();
        }

        // ─── Bootstrap scene ───────────────────────────────────────

        private static void BuildBootstrapScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var root = new GameObject("_GameRoot");
            var gm = root.AddComponent<GameManager>();
            gm.villageState = AssetDatabase.LoadAssetAtPath<VillageState>(VillageStatePath);
            gm.bootstrapSceneName = "00_Bootstrap";
            gm.mainMenuSceneName  = "01_MainMenu";
            gm.autoLoadMainMenu = true;

            var cam = new GameObject("Splash Camera");
            cam.tag = "MainCamera";
            var c = cam.AddComponent<Camera>();
            c.clearFlags = CameraClearFlags.SolidColor;
            c.backgroundColor = new Color(0.05f, 0.04f, 0.03f);

            EditorSceneManager.SaveScene(scene, SceneBootstrap);
        }

        // ─── Main menu scene ───────────────────────────────────────

        private static void BuildMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cam = new GameObject("Main Camera");
            cam.tag = "MainCamera";
            var c = cam.AddComponent<Camera>();
            c.clearFlags = CameraClearFlags.SolidColor;
            c.backgroundColor = new Color(0.10f, 0.08f, 0.06f);

            var canvasGO = new GameObject("UI_MainMenu", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(canvasGO.transform, false);
            var title = titleGO.AddComponent<TextMeshProUGUI>();
            title.text = "Hearthbound Hollow";
            title.fontSize = 80;
            title.alignment = TextAlignmentOptions.Center;
            title.color = new Color(0.97f, 0.85f, 0.62f);
            var titleRT = title.rectTransform;
            titleRT.anchorMin = new Vector2(0.5f, 0.65f);
            titleRT.anchorMax = new Vector2(0.5f, 0.85f);
            titleRT.pivot = new Vector2(0.5f, 0.5f);
            titleRT.anchoredPosition = Vector2.zero;
            titleRT.sizeDelta = new Vector2(900, 200);

            var tipGO = new GameObject("Tip");
            tipGO.transform.SetParent(canvasGO.transform, false);
            var tip = tipGO.AddComponent<TextMeshProUGUI>();
            tip.text = "Some memories want to be sold. Some don't.";
            tip.fontStyle = FontStyles.Italic;
            tip.fontSize = 26;
            tip.alignment = TextAlignmentOptions.Center;
            tip.color = new Color(0.86f, 0.78f, 0.66f, 0.85f);
            var tipRT = tip.rectTransform;
            tipRT.anchorMin = new Vector2(0.5f, 0.55f);
            tipRT.anchorMax = new Vector2(0.5f, 0.62f);
            tipRT.pivot = new Vector2(0.5f, 0.5f);
            tipRT.anchoredPosition = Vector2.zero;
            tipRT.sizeDelta = new Vector2(900, 60);

            var openBtn = MakeUIButton(canvasGO.transform, "Btn_OpenTheHollow", "Open The Hollow",
                new Vector2(0.5f, 0.35f), new Vector2(0.5f, 0.45f), new Vector2(420, 80));

            var quitBtn = MakeUIButton(canvasGO.transform, "Btn_Quit", "Quit",
                new Vector2(0.5f, 0.20f), new Vector2(0.5f, 0.27f), new Vector2(300, 60));

            var compass = BuildToneCompass(canvasGO.transform);

            var ctrlGO = new GameObject("_Controller");
            var ctrl = ctrlGO.AddComponent<MainMenuController>();
            ctrl.openTheHollowButton = openBtn;
            ctrl.quitButton = quitBtn;
            ctrl.firstMissionScene = "03_Mission01_Hollow";
            ctrl.tipLabel = tip;
            ctrl.toneCompass = compass;

            EnsureEventSystem();
            EditorSceneManager.SaveScene(scene, SceneMainMenu);
        }

        // ── Phase 25 hotfix ─────────────────────────────────────────
        // Previously: the script-host (rootGO) and the visual root were the
        // SAME GameObject, and rootGO.SetActive(false) was called. That
        // deactivated the MonoBehaviour, which crashed `StartCoroutine` the
        // moment Show() was invoked from "Open The Hollow":
        //
        //   "Coroutine couldn't be started because the the game object
        //    'ToneCompass' is inactive!"
        //
        // Now: the GameObject hosting the ToneCompassCard script
        // ("ToneCompass") stays ACTIVE always. A child GameObject called
        // "ToneCompass_Visual" carries the dimming background and panel,
        // and IT gets toggled on/off by Show()/Acknowledge(). The script
        // can therefore run coroutines at any time.
        private static ToneCompassCard BuildToneCompass(Transform canvasRoot)
        {
            // 1) The script-host. Stretched to fill the canvas (so a child
            //    Image can dim the screen), but starts WITHOUT any image —
            //    it's an empty container that just hosts the script and
            //    stays active.
            var hostGO = new GameObject("ToneCompass", typeof(RectTransform));
            hostGO.transform.SetParent(canvasRoot, false);
            var hostRT = hostGO.GetComponent<RectTransform>();
            hostRT.anchorMin = Vector2.zero; hostRT.anchorMax = Vector2.one;
            hostRT.offsetMin = Vector2.zero; hostRT.offsetMax = Vector2.zero;

            // 2) The visual root — a CHILD of the host. This is what gets
            //    toggled by Show() / Acknowledge(). It carries the dimming
            //    overlay so when hidden, it does not steal raycasts.
            var visualGO = new GameObject("ToneCompass_Visual", typeof(RectTransform));
            visualGO.transform.SetParent(hostGO.transform, false);
            var visualImg = visualGO.AddComponent<Image>();
            visualImg.color = new Color(0.04f, 0.03f, 0.02f, 0.92f);
            var visualRT = visualImg.rectTransform;
            visualRT.anchorMin = Vector2.zero; visualRT.anchorMax = Vector2.one;
            visualRT.offsetMin = Vector2.zero; visualRT.offsetMax = Vector2.zero;

            var panelGO = new GameObject("Panel");
            panelGO.transform.SetParent(visualGO.transform, false);
            var panelImg = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0.18f, 0.12f, 0.08f, 0.96f);
            var panelRT = panelImg.rectTransform;
            panelRT.anchorMin = new Vector2(0.18f, 0.18f);
            panelRT.anchorMax = new Vector2(0.82f, 0.82f);
            panelRT.offsetMin = Vector2.zero; panelRT.offsetMax = Vector2.zero;

            // Title (cozy warmth)
            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(panelGO.transform, false);
            var title = titleGO.AddComponent<TextMeshProUGUI>();
            title.alignment = TextAlignmentOptions.Center;
            title.fontSize = 40;
            title.color = new Color(0.97f, 0.85f, 0.62f);
            title.text = "A Word Before You Begin";
            var titleRT = title.rectTransform;
            titleRT.anchorMin = new Vector2(0.06f, 0.84f);
            titleRT.anchorMax = new Vector2(0.94f, 0.94f);
            titleRT.offsetMin = Vector2.zero; titleRT.offsetMax = Vector2.zero;

            var bodyGO = new GameObject("Body");
            bodyGO.transform.SetParent(panelGO.transform, false);
            var body = bodyGO.AddComponent<TextMeshProUGUI>();
            body.alignment = TextAlignmentOptions.TopLeft;
            body.fontSize = 24;
            body.color = new Color(0.95f, 0.90f, 0.78f);
            var bodyRT = body.rectTransform;
            bodyRT.anchorMin = new Vector2(0.06f, 0.28f);
            bodyRT.anchorMax = new Vector2(0.94f, 0.82f);
            bodyRT.offsetMin = Vector2.zero; bodyRT.offsetMax = Vector2.zero;

            var continueBtn = MakeUIButton(panelGO.transform, "ContinueBtn", "Continue",
                new Vector2(0.36f, 0.08f), new Vector2(0.64f, 0.20f), new Vector2(0, 0));

            // 3) Attach the script to the HOST (which is always active) and
            //    point its `root` field at the CHILD visual GameObject.
            var compass = hostGO.AddComponent<ToneCompassCard>();
            compass.root = visualGO;
            compass.bodyText = body;
            compass.continueButton = continueBtn;

            // Hide the visual at start. The host stays active, so the script's
            // Awake/Update/Start all still run.
            visualGO.SetActive(false);
            return compass;
        }

        // ─── Lane scene (MVP minimal pass-through) ─────────────────

        private static void BuildLaneScene()
        {
            EnsureMaterialsBuilt();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateDirectionalLight(angleX: 40, color: new Color(1f, 0.88f, 0.72f));
            CreateAmbient();
            CreateGround(_matGround, size: 60f);

            var player = CreatePlayer();
            player.transform.position = new Vector3(0, 1.0f, -10);

            var doorGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorGO.name = "HollowDoor";
            doorGO.transform.position = new Vector3(0, 1.5f, 8f);
            doorGO.transform.localScale = new Vector3(2, 3, 0.4f);
            doorGO.GetComponent<MeshRenderer>().sharedMaterial = _matWorkbench;
            var doorScript = doorGO.AddComponent<HollowDoorInteractable>();
            doorScript.targetSceneName = "03_Mission01_Hollow";
            var label = new GameObject("Label").AddComponent<TextMeshPro>();
            label.transform.SetParent(doorGO.transform, false);
            label.transform.localPosition = new Vector3(0, 0.6f, 0);
            label.text = "→ Enter the Hollow (E)";
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 4;
            label.color = new Color(0.97f, 0.85f, 0.62f);

            CreateFollowCamera(player.transform, height: 4, behind: 6, lookAheadY: 1.5f);
            EditorSceneManager.SaveScene(scene, SceneMission01Lane);
        }

        // ─── Hollow scene — the playable MVP ───────────────────────

        private static void BuildHollowScene()
        {
            EnsureMaterialsBuilt();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateDirectionalLight(angleX: 35, color: new Color(1f, 0.84f, 0.62f), intensity: 0.95f);
            CreateAmbient();
            CreateGround(_matGround, size: 24f);

            // Player — prefers Phase 13 BoZo prefab when available.
            var player = CreatePlayer();
            player.transform.position = new Vector3(-3, 1.0f, -3);

            // Doris — prefers Phase 13 BoZo Doris prefab when available, falls back to cylinder.
            var (doris, dorisGreetingTrigger) = CreateDoris(new Vector3(2, 0f, 0));

            // Workbench (placeholder cube until Phase 15 — Medieval Village).
            var bench = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bench.name = "Workbench";
            bench.transform.position = new Vector3(4, 0.5f, 3);
            bench.transform.localScale = new Vector3(1.8f, 1.0f, 1.0f);
            bench.GetComponent<MeshRenderer>().sharedMaterial = _matWorkbench;
            var benchApproach = new GameObject("Workbench_Approach_Zone");
            benchApproach.transform.SetParent(bench.transform, false);
            var approachCol = benchApproach.AddComponent<SphereCollider>();
            approachCol.radius = 1.4f;
            approachCol.isTrigger = true;
            benchApproach.transform.localPosition = new Vector3(0, 0, -0.5f);

            // Memory orb on the workbench (hidden initially; Doris hands it over).
            var orbGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orbGO.name = "MemoryOrb_DOR-001";
            orbGO.transform.position = new Vector3(4, 1.25f, 3);
            orbGO.transform.localScale = Vector3.one * 0.45f;
            var orbMat = new Material(_matOrb);
            orbGO.GetComponent<MeshRenderer>().sharedMaterial = orbMat;
            var orb = orbGO.AddComponent<MemoryOrbInteractable>();
            orb.orbRenderer = orbGO.GetComponent<MeshRenderer>();
            orb.memory = AssetDatabase.LoadAssetAtPath<MemoryNodeSO>(DorisMemoryPath);

            CreateFollowCamera(player.transform, height: 5, behind: 6, lookAheadY: 1.0f);

            // UI Canvas
            var canvasGO = new GameObject("UI_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // Dialogue UI — prefers Phase 14 Bamao prefab when available.
            var dialogueUI = BuildDialogueUI(canvasGO.transform);
            // Evening Ledger — prefers Phase 14 Bamao prefab when available.
            var ledgerUI = BuildEveningLedgerUI(canvasGO.transform);
            var hud = BuildHUD(canvasGO.transform);

            var miniGO = new GameObject("_PolishMiniGame");
            var polish = miniGO.AddComponent<PolishMiniGame>();

            var directorGO = new GameObject("_MissionDirector");
            var director = directorGO.AddComponent<Mission01Director>();
            director.playerController = player.GetComponent<PlayerController>();
            director.dorisTransform = doris.transform;
            director.dorisGreetingTrigger = dorisGreetingTrigger;
            director.workbenchOrb = orb;
            director.workbenchApproachTrigger = approachCol;
            director.dialogueUI = dialogueUI;
            director.eveningLedger = ledgerUI;
            director.polishGame = polish;
            director.dorisVillager = AssetDatabase.LoadAssetAtPath<VillagerSO>(DorisPath);
            director.dorisMemory   = orb.memory;

            EnsureEventSystem();
            EditorSceneManager.SaveScene(scene, SceneMission01Hollow);
        }

        // ─── Player + Doris spawn (with Phase 13 BoZo preference) ──

        private static GameObject CreatePlayer()
        {
            var bozoPlayer = Phase13_BoZoCharacterBuilder.TryGetPlayerPrefab();
            if (bozoPlayer != null)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(bozoPlayer);
                go.name = "Player";
                // Ensure components present (the prefab already has them, but defensive)
                if (go.tag != "Player") go.tag = "Player";
                if (go.GetComponent<CharacterController>() == null)
                {
                    var cc = go.AddComponent<CharacterController>();
                    cc.center = new Vector3(0, 1.0f, 0); cc.height = 1.9f; cc.radius = 0.4f;
                }
                if (go.GetComponent<PlayerController>() == null)
                    go.AddComponent<PlayerController>();
                Debug.Log("[Hearthbound/OneClick] Player → using Phase 13 BoZo prefab.");
                return go;
            }
            // Fallback: primitive capsule.
            return CreatePrimitivePlayer();
        }

        private static GameObject CreatePrimitivePlayer()
        {
            var go = new GameObject("Player");
            go.tag = "Player";

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(go.transform, false);
            body.transform.localPosition = new Vector3(0, 1f, 0);
            body.GetComponent<MeshRenderer>().sharedMaterial = _matPlayer;
            var bodyCol = body.GetComponent<CapsuleCollider>();
            if (bodyCol != null) Object.DestroyImmediate(bodyCol);

            var cc = go.AddComponent<CharacterController>();
            cc.center = new Vector3(0, 1f, 0); cc.height = 1.9f; cc.radius = 0.4f;
            go.AddComponent<PlayerController>();
            return go;
        }

        private static (GameObject doris, Collider greetingTrigger) CreateDoris(Vector3 worldPos)
        {
            var bozoDoris = Phase13_BoZoCharacterBuilder.TryGetDorisPrefab();
            if (bozoDoris != null)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(bozoDoris);
                go.name = "Doris";
                go.transform.position = worldPos;

                // The Phase-13 prefab includes a child "Doris_Greeting_Zone" with a trigger SphereCollider.
                var trigger = go.transform.Find("Doris_Greeting_Zone")?.GetComponent<Collider>();
                if (trigger == null)
                {
                    // Defensive: add one if the prefab variant lacks it
                    var greetGO = new GameObject("Doris_Greeting_Zone");
                    greetGO.transform.SetParent(go.transform, false);
                    var sphere = greetGO.AddComponent<SphereCollider>();
                    sphere.radius = 1.6f; sphere.isTrigger = true;
                    trigger = sphere;
                }
                Debug.Log("[Hearthbound/OneClick] Doris → using Phase 13 BoZo prefab.");
                return (go, trigger);
            }

            // Fallback: primitive cylinder
            var primDoris = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            primDoris.name = "Doris";
            primDoris.transform.position = worldPos + Vector3.up;
            primDoris.transform.localScale = new Vector3(0.7f, 0.9f, 0.7f);
            primDoris.GetComponent<MeshRenderer>().sharedMaterial = _matDoris;
            var greet = new GameObject("Doris_Greeting_Zone");
            greet.transform.SetParent(primDoris.transform, false);
            var sphereCol = greet.AddComponent<SphereCollider>();
            sphereCol.radius = 1.6f; sphereCol.isTrigger = true;
            return (primDoris, sphereCol);
        }

        // ─── UI builders (with Phase 14 Bamao preference) ──────────

        private static DialogueUI BuildDialogueUI(Transform canvasRoot)
        {
            var bamaoPrefab = Phase14_BamaoUIBuilder.TryGetDialogueBoxPrefab();
            if (bamaoPrefab != null)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(bamaoPrefab, canvasRoot);
                go.name = "DialogueBox";
                go.SetActive(false);
                Debug.Log("[Hearthbound/OneClick] DialogueBox → using Phase 14 Bamao prefab.");
                return go.GetComponent<DialogueUI>();
            }
            return BuildPrimitiveDialogueUI(canvasRoot);
        }

        // ── Phase 25 hotfix ─────────────────────────────────────────
        // Two-layer pattern. Host stays active so PresentLine's typewriter
        // coroutine runs from a live MonoBehaviour. The "Visual" child holds
        // the parchment background and is what gets toggled.
        private static DialogueUI BuildPrimitiveDialogueUI(Transform canvasRoot)
        {
            var hostGO = new GameObject("DialogueBox", typeof(RectTransform));
            hostGO.transform.SetParent(canvasRoot, false);
            var hostRT = hostGO.GetComponent<RectTransform>();
            hostRT.anchorMin = Vector2.zero; hostRT.anchorMax = Vector2.one;
            hostRT.offsetMin = Vector2.zero; hostRT.offsetMax = Vector2.zero;

            var rootGO = new GameObject("Visual", typeof(RectTransform));
            rootGO.transform.SetParent(hostGO.transform, false);
            var bg = rootGO.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.04f, 0.03f, 0.85f);
            var rt = bg.rectTransform;
            rt.anchorMin = new Vector2(0.10f, 0.06f); rt.anchorMax = new Vector2(0.90f, 0.32f);
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            var nameGO = new GameObject("SpeakerName");
            nameGO.transform.SetParent(rootGO.transform, false);
            var speakerName = nameGO.AddComponent<TextMeshProUGUI>();
            speakerName.fontSize = 28;
            speakerName.color = new Color(0.97f, 0.85f, 0.62f);
            speakerName.alignment = TextAlignmentOptions.BottomLeft;
            var nameRT = speakerName.rectTransform;
            nameRT.anchorMin = new Vector2(0.04f, 0.78f); nameRT.anchorMax = new Vector2(0.5f, 0.96f);
            nameRT.offsetMin = Vector2.zero; nameRT.offsetMax = Vector2.zero;

            var textGO = new GameObject("LineText");
            textGO.transform.SetParent(rootGO.transform, false);
            var lineText = textGO.AddComponent<TextMeshProUGUI>();
            lineText.fontSize = 24;
            lineText.color = new Color(0.92f, 0.88f, 0.78f);
            lineText.alignment = TextAlignmentOptions.TopLeft;
            var textRT = lineText.rectTransform;
            textRT.anchorMin = new Vector2(0.04f, 0.04f); textRT.anchorMax = new Vector2(0.96f, 0.74f);
            textRT.offsetMin = Vector2.zero; textRT.offsetMax = Vector2.zero;

            var choicesGO = new GameObject("ChoicesContainer", typeof(RectTransform));
            choicesGO.transform.SetParent(canvasRoot, false);
            var choicesLayout = choicesGO.AddComponent<VerticalLayoutGroup>();
            choicesLayout.spacing = 8;
            choicesLayout.childForceExpandWidth = true;
            choicesLayout.childForceExpandHeight = false;
            choicesLayout.padding = new RectOffset(8, 8, 8, 8);
            var choicesRT = choicesGO.GetComponent<RectTransform>();
            choicesRT.anchorMin = new Vector2(0.10f, 0.35f); choicesRT.anchorMax = new Vector2(0.55f, 0.60f);
            choicesRT.offsetMin = Vector2.zero; choicesRT.offsetMax = Vector2.zero;

            var choiceTemplate = new GameObject("ChoiceButtonTemplate", typeof(Image));
            choiceTemplate.transform.SetParent(rootGO.transform, false);
            choiceTemplate.SetActive(false);
            var tplImg = choiceTemplate.GetComponent<Image>();
            tplImg.color = new Color(0.16f, 0.10f, 0.06f, 0.95f);
            var tplBtn = choiceTemplate.AddComponent<Button>();
            tplBtn.targetGraphic = tplImg;
            var tplLE = choiceTemplate.AddComponent<LayoutElement>();
            tplLE.preferredHeight = 56;
            var tplLabelGO = new GameObject("Label");
            tplLabelGO.transform.SetParent(choiceTemplate.transform, false);
            var tplLabel = tplLabelGO.AddComponent<TextMeshProUGUI>();
            tplLabel.fontSize = 22;
            tplLabel.color = new Color(0.97f, 0.85f, 0.62f);
            tplLabel.alignment = TextAlignmentOptions.Center;
            tplLabel.text = "Choice";
            var tplLabelRT = tplLabel.rectTransform;
            tplLabelRT.anchorMin = Vector2.zero; tplLabelRT.anchorMax = Vector2.one;
            tplLabelRT.offsetMin = new Vector2(16, 4); tplLabelRT.offsetMax = new Vector2(-16, -4);

            var dlg = hostGO.AddComponent<DialogueUI>();
            dlg.root = rootGO;
            dlg.speakerName = speakerName;
            dlg.lineText = lineText;
            dlg.choiceContainer = choicesGO.transform;
            dlg.choiceButtonPrefab = choiceTemplate;
            // Host stays active. Only the visual is hidden.
            rootGO.SetActive(false);
            return dlg;
        }

        private static EveningLedgerUI BuildEveningLedgerUI(Transform canvasRoot)
        {
            var bamaoPrefab = Phase14_BamaoUIBuilder.TryGetEveningLedgerPrefab();
            if (bamaoPrefab != null)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(bamaoPrefab, canvasRoot);
                go.name = "EveningLedger";
                go.SetActive(false);
                Debug.Log("[Hearthbound/OneClick] EveningLedger → using Phase 14 Bamao prefab.");
                return go.GetComponent<EveningLedgerUI>();
            }
            return BuildPrimitiveEveningLedgerUI(canvasRoot);
        }

        // ── Phase 25 hotfix ─────────────────────────────────────────
        // Two-layer pattern. Host stays active.
        private static EveningLedgerUI BuildPrimitiveEveningLedgerUI(Transform canvasRoot)
        {
            var hostGO = new GameObject("EveningLedger", typeof(RectTransform));
            hostGO.transform.SetParent(canvasRoot, false);
            var hostRT = hostGO.GetComponent<RectTransform>();
            hostRT.anchorMin = Vector2.zero; hostRT.anchorMax = Vector2.one;
            hostRT.offsetMin = Vector2.zero; hostRT.offsetMax = Vector2.zero;

            var rootGO = new GameObject("Visual", typeof(RectTransform));
            rootGO.transform.SetParent(hostGO.transform, false);
            var bg = rootGO.AddComponent<Image>();
            bg.color = new Color(0.03f, 0.02f, 0.01f, 0.94f);
            var rt = bg.rectTransform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            var titleGO = new GameObject("DayLabel");
            titleGO.transform.SetParent(rootGO.transform, false);
            var dayLabel = titleGO.AddComponent<TextMeshProUGUI>();
            dayLabel.fontSize = 60;
            dayLabel.alignment = TextAlignmentOptions.Center;
            dayLabel.color = new Color(0.97f, 0.85f, 0.62f);
            dayLabel.text = "Day 1";
            var titleRT = dayLabel.rectTransform;
            titleRT.anchorMin = new Vector2(0.0f, 0.82f); titleRT.anchorMax = new Vector2(1.0f, 0.95f);
            titleRT.offsetMin = Vector2.zero; titleRT.offsetMax = Vector2.zero;

            var proseGO = new GameObject("SummaryProse");
            proseGO.transform.SetParent(rootGO.transform, false);
            var prose = proseGO.AddComponent<TextMeshProUGUI>();
            prose.fontSize = 28;
            prose.alignment = TextAlignmentOptions.Center;
            prose.color = new Color(0.92f, 0.86f, 0.74f);
            var proseRT = prose.rectTransform;
            proseRT.anchorMin = new Vector2(0.15f, 0.55f); proseRT.anchorMax = new Vector2(0.85f, 0.78f);
            proseRT.offsetMin = Vector2.zero; proseRT.offsetMax = Vector2.zero;

            var memListGO = new GameObject("HeldMemoriesList");
            memListGO.transform.SetParent(rootGO.transform, false);
            var memList = memListGO.AddComponent<TextMeshProUGUI>();
            memList.fontSize = 22;
            memList.alignment = TextAlignmentOptions.Center;
            memList.color = new Color(0.86f, 0.78f, 0.66f);
            var memListRT = memList.rectTransform;
            memListRT.anchorMin = new Vector2(0.20f, 0.32f); memListRT.anchorMax = new Vector2(0.80f, 0.50f);
            memListRT.offsetMin = Vector2.zero; memListRT.offsetMax = Vector2.zero;

            var coinGO = new GameObject("CoinLabel");
            coinGO.transform.SetParent(rootGO.transform, false);
            var coinLabel = coinGO.AddComponent<TextMeshProUGUI>();
            coinLabel.fontSize = 24;
            coinLabel.alignment = TextAlignmentOptions.Center;
            coinLabel.color = new Color(0.96f, 0.84f, 0.50f);
            var coinRT = coinLabel.rectTransform;
            coinRT.anchorMin = new Vector2(0.20f, 0.23f); coinRT.anchorMax = new Vector2(0.80f, 0.30f);
            coinRT.offsetMin = Vector2.zero; coinRT.offsetMax = Vector2.zero;

            var confirmBtn = MakeUIButton(rootGO.transform, "Btn_EndOfDay", "Sleep — End Day 1",
                new Vector2(0.35f, 0.08f), new Vector2(0.65f, 0.18f), new Vector2(0, 0));

            var ledger = hostGO.AddComponent<EveningLedgerUI>();
            ledger.root = rootGO;
            ledger.dayLabel = dayLabel;
            ledger.coinLabel = coinLabel;
            ledger.summaryProse = prose;
            ledger.heldMemoriesList = memList;
            ledger.confirmEndOfDayButton = confirmBtn;
            // Host stays active. Only the visual is hidden.
            rootGO.SetActive(false);
            return ledger;
        }

        private static HUDController BuildHUD(Transform canvasRoot)
        {
            var rootGO = new GameObject("HUD", typeof(RectTransform));
            rootGO.transform.SetParent(canvasRoot, false);
            var hudRT = rootGO.GetComponent<RectTransform>();
            hudRT.anchorMin = new Vector2(0.0f, 0.92f); hudRT.anchorMax = new Vector2(1.0f, 1.0f);
            hudRT.offsetMin = new Vector2(20, 0); hudRT.offsetMax = new Vector2(-20, 0);

            var dayGO = new GameObject("DayLabel");
            dayGO.transform.SetParent(rootGO.transform, false);
            var dayLabel = dayGO.AddComponent<TextMeshProUGUI>();
            dayLabel.fontSize = 22;
            dayLabel.alignment = TextAlignmentOptions.Left;
            dayLabel.color = new Color(0.95f, 0.86f, 0.66f);
            var dayRT = dayLabel.rectTransform;
            dayRT.anchorMin = Vector2.zero; dayRT.anchorMax = new Vector2(0.5f, 1);
            dayRT.offsetMin = Vector2.zero; dayRT.offsetMax = Vector2.zero;

            var coinGO = new GameObject("CoinLabel");
            coinGO.transform.SetParent(rootGO.transform, false);
            var coinLabel = coinGO.AddComponent<TextMeshProUGUI>();
            coinLabel.fontSize = 22;
            coinLabel.alignment = TextAlignmentOptions.Right;
            coinLabel.color = new Color(0.96f, 0.84f, 0.50f);
            var coinRT = coinLabel.rectTransform;
            coinRT.anchorMin = new Vector2(0.5f, 0); coinRT.anchorMax = Vector2.one;
            coinRT.offsetMin = Vector2.zero; coinRT.offsetMax = Vector2.zero;

            var hud = rootGO.AddComponent<HUDController>();
            hud.dayLabel = dayLabel;
            hud.coinLabel = coinLabel;
            return hud;
        }

        // ─── Generic helpers ───────────────────────────────────────

        private static Button MakeUIButton(Transform parent, string name, string label,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta)
        {
            var btnGO = new GameObject(name, typeof(Image), typeof(Button));
            btnGO.transform.SetParent(parent, false);
            var img = btnGO.GetComponent<Image>();
            img.color = new Color(0.18f, 0.12f, 0.08f, 0.95f);
            var rt = img.rectTransform;
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            if (sizeDelta != Vector2.zero) rt.sizeDelta = sizeDelta;

            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(btnGO.transform, false);
            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 28;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.97f, 0.85f, 0.62f);
            var labelRT = tmp.rectTransform;
            labelRT.anchorMin = Vector2.zero; labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = new Vector2(12, 4); labelRT.offsetMax = new Vector2(-12, -4);

            return btnGO.GetComponent<Button>();
        }

        private static void CreateDirectionalLight(float angleX, Color color, float intensity = 1.05f)
        {
            var lightGO = new GameObject("Directional Light");
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = color;
            light.intensity = intensity;
            light.shadows = LightShadows.Soft;
            lightGO.transform.rotation = Quaternion.Euler(angleX, 30, 0);
        }

        private static void CreateAmbient()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor    = new Color(0.46f, 0.42f, 0.36f);
            RenderSettings.ambientEquatorColor = new Color(0.32f, 0.28f, 0.22f);
            RenderSettings.ambientGroundColor  = new Color(0.18f, 0.14f, 0.10f);
        }

        private static void CreateGround(Material mat, float size)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(size / 10f, 1, size / 10f);
            ground.GetComponent<MeshRenderer>().sharedMaterial = mat;
            ground.isStatic = true;
        }

        private static void CreateFollowCamera(Transform target, float height, float behind, float lookAheadY)
        {
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            var c = camGO.AddComponent<Camera>();
            c.clearFlags = CameraClearFlags.SolidColor;
            c.backgroundColor = new Color(0.08f, 0.06f, 0.05f);

            var follow = camGO.AddComponent<SimpleFollowCamera>();
            follow.target = target;
            follow.height = height;
            follow.behind = behind;
            follow.lookAheadY = lookAheadY;
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

        // ─── Build Settings ────────────────────────────────────────

        private static void UpdateBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>
            {
                new(SceneBootstrap, true),
                new(SceneMainMenu, true),
                new(SceneMission01Lane, true),
                new(SceneMission01Hollow, true),
            };
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        // ─── Folder helpers ────────────────────────────────────────

        private static void EnsureDir(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            var leaf = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureDir(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }

    // SimpleFollowCamera was previously declared here as a public nested
    // class. That broke runtime scene loads because the Editor asmdef has
    // includePlatforms = ["Editor"] — the class wasn't shipped to the
    // player. It now lives at:
    //
    //     Assets/_Project/Scripts/Player/SimpleFollowCamera.cs
    //     namespace HearthboundHollow.Player
    //
    // The Editor builders import `HearthboundHollow.Player` so existing
    // AddComponent<SimpleFollowCamera>() calls resolve to the runtime
    // class automatically.
}
