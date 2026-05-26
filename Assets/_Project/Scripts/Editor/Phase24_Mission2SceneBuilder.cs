// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase24_Mission2SceneBuilder
//
// Phase 24 — Mission 2 scene scaffolding.
//
// Builds (or rebuilds) the two Mission 2 scenes:
//   * 04_Mission02_Garden.unity   — herb garden + kettle
//   * 05_Mission02_Cottage.unity  — Gerrold + choice card + Cleanse + Dream2
//
// Pattern mirrors HearthboundOneClickSetup:
//   * Each role first attempts to use Phase 13/14 asset-driven prefabs.
//   * Falls back to URP primitives so the scenes are buildable without the
//     full asset packs imported.
//   * Wires Mission02Director with every reference it needs (no manual setup).
//   * Adds both scenes to Build Settings (index 4 and 5).
//
// USE: Menu → Hearthbound → ⚙️ Advanced → Phase 24 — Build Mission 2 Scenes
//
// Demoted to ⚙️ Advanced/… in Phase 32 (menu collapse). The user-facing
// entry point is now `Hearthbound → 🚀 Build Everything`, which chains
// Phase 24 internally via Phase 23.

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
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
    public static class Phase24_Mission2SceneBuilder
    {
        // ─── Paths ────────────────────────────────────────────────

        private const string ScenesDir = "Assets/_Project/Scenes";
        private const string MaterialsDir = "Assets/_Project/Art/Materials";

        private const string SceneGarden  = ScenesDir + "/04_Mission02_Garden.unity";
        private const string SceneCottage = ScenesDir + "/05_Mission02_Cottage.unity";

        private const string VillageStatePath = "Assets/_Project/ScriptableObjects/State/VillageState.asset";
        private const string GerroldVillagerPath = "Assets/_Project/ScriptableObjects/Villagers/Gerrold.asset";
        private const string GerroldMemoryPath = "Assets/_Project/ScriptableObjects/Memories/GER-007_SeventhMorning.asset";
        private const string LavenderHerbPath = "Assets/_Project/ScriptableObjects/Herbs/Lavender.asset";
        private const string ValerianHerbPath = "Assets/_Project/ScriptableObjects/Herbs/Valerian.asset";
        private const string TariffErasePath   = "Assets/_Project/ScriptableObjects/Tariffs/Tariff_Erase.asset";
        private const string TariffCleansePath = "Assets/_Project/ScriptableObjects/Tariffs/Tariff_Cleanse.asset";
        private const string TariffListenPath  = "Assets/_Project/ScriptableObjects/Tariffs/Tariff_Listen.asset";
        private const string TariffDeferPath   = "Assets/_Project/ScriptableObjects/Tariffs/Tariff_Defer.asset";
        private const string Mission02SoPath   = "Assets/_Project/ScriptableObjects/Missions/Mission02_TheWidowersRequest.asset";

        // ─── Master menu ──────────────────────────────────────────

        [MenuItem("Hearthbound/⚙️ Advanced/Phase 24 — Build Mission 2 Scenes", priority = 24)]
        public static void Build()
        {
            EditorUtility.DisplayProgressBar("Hearthbound · Phase 24", "Verifying prereqs…", 0.05f);
            try
            {
                if (!VerifySeedAssets()) return;

                EnsureDir(ScenesDir);
                EnsureDir(MaterialsDir);

                EditorUtility.DisplayProgressBar("Hearthbound · Phase 24", "Building 04_Mission02_Garden…", 0.30f);
                BuildGardenScene();

                EditorUtility.DisplayProgressBar("Hearthbound · Phase 24", "Building 05_Mission02_Cottage…", 0.65f);
                BuildCottageScene();

                EditorUtility.DisplayProgressBar("Hearthbound · Phase 24", "Updating Build Settings…", 0.92f);
                UpdateBuildSettings();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            EditorUtility.DisplayDialog(
                "Hearthbound — Mission 2 scenes built",
                "Two scenes created/refreshed:\n" +
                "  • 04_Mission02_Garden.unity (index 4)\n" +
                "  • 05_Mission02_Cottage.unity (index 5)\n\n" +
                "Mission 1 still ends to the Main Menu by default — re-run Phase 23 (Polish capstone) " +
                "to enable the Mission 1 → Mission 2 hand-off and route the Doris ledger to the Garden " +
                "instead.\n\n" +
                "Flow once the hand-off is enabled:\n" +
                "  Lane → Hollow → polish Doris's orb → Evening Ledger → Garden →\n" +
                "  harvest herbs → brew tea → Cottage → meet Gerrold → moral choice →\n" +
                "  (optional Cleanse mini-game) → Dream 2 → Evening Ledger → Main Menu.",
                "OK");
        }

        // ─── Prereq check ────────────────────────────────────────

        private static bool VerifySeedAssets()
        {
            bool missing =
                AssetDatabase.LoadAssetAtPath<VillageState>(VillageStatePath) == null ||
                AssetDatabase.LoadAssetAtPath<VillagerSO>(GerroldVillagerPath) == null ||
                AssetDatabase.LoadAssetAtPath<MemoryNodeSO>(GerroldMemoryPath) == null ||
                AssetDatabase.LoadAssetAtPath<TariffSO>(TariffErasePath) == null;

            if (!missing) return true;

            if (EditorUtility.DisplayDialog(
                    "Hearthbound — Seed assets missing",
                    "Mission 2 seed ScriptableObjects (Gerrold, GER-007, the 4 Tariffs, herbs) aren't yet generated.\n\n" +
                    "Run 'Hearthbound → Create Mission 1-2 Seed Assets' first?",
                    "Generate seeds now", "Cancel"))
            {
                SeedAssetGenerator.CreateAllSeedAssets();
                return AssetDatabase.LoadAssetAtPath<TariffSO>(TariffErasePath) != null;
            }
            return false;
        }

        // ─── Material cache ──────────────────────────────────────

        private static Material _matGroundGrass, _matPlant, _matFloorWood, _matWall, _matKettle, _matOrbCracked;

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
            if (_matGroundGrass == null) _matGroundGrass = CreateUrpLitMaterial(MaterialsDir + "/Mat_GardenGrass.mat", new Color(0.34f, 0.45f, 0.22f));
            if (_matPlant       == null) _matPlant       = CreateUrpLitMaterial(MaterialsDir + "/Mat_HerbPlant.mat",   new Color(0.55f, 0.42f, 0.62f));
            if (_matFloorWood   == null) _matFloorWood   = CreateUrpLitMaterial(MaterialsDir + "/Mat_FloorWood.mat",   new Color(0.46f, 0.30f, 0.18f));
            if (_matWall        == null) _matWall        = CreateUrpLitMaterial(MaterialsDir + "/Mat_CottageWall.mat", new Color(0.62f, 0.52f, 0.40f));
            if (_matKettle      == null) _matKettle      = CreateUrpLitMaterial(MaterialsDir + "/Mat_Kettle.mat",      new Color(0.20f, 0.18f, 0.16f), 0.7f, new Color(0.6f, 0.2f, 0.05f));
            if (_matOrbCracked  == null) _matOrbCracked  = CreateUrpLitMaterial(MaterialsDir + "/Mat_OrbCracked.mat",  new Color(0.32f, 0.30f, 0.36f), 0.7f, new Color(0.18f, 0.10f, 0.05f));
            AssetDatabase.SaveAssets();
        }

        // ─── Garden scene ────────────────────────────────────────

        private static void BuildGardenScene()
        {
            EnsureMaterialsBuilt();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateDirectionalLight(angleX: 50, color: new Color(1f, 0.92f, 0.78f), intensity: 1.0f);
            CreateAmbient();
            CreateGround(_matGroundGrass, size: 36f);

            var player = CreatePlayer();
            player.transform.position = new Vector3(0, 1.0f, -8);

            // Lavender + Valerian plant interactables.
            var lavender = CreatePlantInteractable("LavenderPlant", new Vector3(-3.5f, 0.4f, 2f), new Color(0.65f, 0.55f, 0.85f));
            var valerian = CreatePlantInteractable("ValerianPlant", new Vector3(3.5f, 0.4f, 2f),  new Color(0.95f, 0.92f, 0.78f));
            var lavenderH = AssetDatabase.LoadAssetAtPath<MemoryHerb>(LavenderHerbPath);
            var valerianH = AssetDatabase.LoadAssetAtPath<MemoryHerb>(ValerianHerbPath);
            lavender.herb = lavenderH;
            valerian.herb = valerianH;

            // Kettle prop with KettleInteractable.
            var kettleGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            kettleGO.name = "Kettle";
            kettleGO.transform.position = new Vector3(0, 0.7f, 5f);
            kettleGO.transform.localScale = new Vector3(0.7f, 0.5f, 0.7f);
            kettleGO.GetComponent<MeshRenderer>().sharedMaterial = _matKettle;
            var kettle = kettleGO.AddComponent<KettleInteractable>();

            // Garden exit portal (the path to the cottage).
            var exit = new GameObject("Garden_Exit_Trigger");
            exit.transform.position = new Vector3(0, 1, 12);
            var exitCol = exit.AddComponent<BoxCollider>();
            exitCol.size = new Vector3(8, 3, 1);
            exitCol.isTrigger = true;
            var exitLabel = new GameObject("Label").AddComponent<TextMeshPro>();
            exitLabel.transform.SetParent(exit.transform, false);
            exitLabel.transform.localPosition = new Vector3(0, 1.2f, 0);
            exitLabel.text = "→ To Gerrold's cottage";
            exitLabel.alignment = TextAlignmentOptions.Center;
            exitLabel.fontSize = 4;
            exitLabel.color = new Color(0.97f, 0.85f, 0.62f);

            CreateFollowCamera(player.transform, height: 5, behind: 6, lookAheadY: 1.2f);

            // UI canvas with DialogueUI + TeaBrewingUI + TitleCard.
            var canvas = CreateCanvas("UI_Canvas");
            var dialogueUI = BuildDialogueUI(canvas.transform);
            var titleCard = BuildTitleCard(canvas.transform, "Day 2 — The Widower's Request",
                "Quiet. A heavy beat. Candlelight, soft rain optional.", Mission02SoPath);
            var tea = BuildTeaBrewingUI(canvas.transform, lavenderH, valerianH);

            EnsureEventSystem();

            var directorGO = new GameObject("_Mission02Director");
            var dir = directorGO.AddComponent<Mission02Director>();
            dir.sceneRole = Mission02Director.SceneRole.Garden;
            dir.playerController = player.GetComponent<PlayerController>();
            dir.dialogueUI = dialogueUI;
            dir.titleCard = titleCard;
            dir.lavenderPlant = lavender;
            dir.valerianPlant = valerian;
            dir.kettle = kettle;
            dir.teaBrewingUI = tea;
            dir.gardenExitTrigger = exitCol;
            dir.cottageSceneName = "05_Mission02_Cottage";
            dir.gerroldVillager = AssetDatabase.LoadAssetAtPath<VillagerSO>(GerroldVillagerPath);
            dir.gerroldMemory   = AssetDatabase.LoadAssetAtPath<MemoryNodeSO>(GerroldMemoryPath);

            EditorSceneManager.SaveScene(scene, SceneGarden);
        }

        // ─── Cottage scene ────────────────────────────────────────

        private static void BuildCottageScene()
        {
            EnsureMaterialsBuilt();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateDirectionalLight(angleX: 28, color: new Color(0.92f, 0.78f, 0.58f), intensity: 0.82f);
            CreateAmbient(low: true);
            CreateGround(_matFloorWood, size: 14f);
            CreateCottageWalls(_matWall);

            var player = CreatePlayer();
            player.transform.position = new Vector3(0, 1.0f, -4);

            // Gerrold — uses Phase 13 BoZo prefab if available, falls back to cylinder.
            var (gerrold, greetingTrigger) = CreateGerrold(new Vector3(2.0f, 0f, 1.5f));

            // GER-007 orb on the table — starts hidden; choice path reveals it.
            var orbGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orbGO.name = "MemoryOrb_GER-007";
            orbGO.transform.position = new Vector3(0.5f, 1.0f, 1.0f);
            orbGO.transform.localScale = Vector3.one * 0.4f;
            orbGO.GetComponent<MeshRenderer>().sharedMaterial = _matOrbCracked;
            var orb = orbGO.AddComponent<MemoryOrbInteractable>();
            orb.orbRenderer = orbGO.GetComponent<MeshRenderer>();
            orb.memory = AssetDatabase.LoadAssetAtPath<MemoryNodeSO>(GerroldMemoryPath);
            orbGO.SetActive(false);

            // Table prop.
            var table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "Cottage_Table";
            table.transform.position = new Vector3(0.5f, 0.5f, 1f);
            table.transform.localScale = new Vector3(1.4f, 0.9f, 0.8f);
            table.GetComponent<MeshRenderer>().sharedMaterial = _matFloorWood;

            CreateFollowCamera(player.transform, height: 4.5f, behind: 5.5f, lookAheadY: 1.2f);

            // UI canvas with DialogueUI + ChoiceCardUI + EveningLedger + TitleCard.
            var canvas = CreateCanvas("UI_Canvas");
            var dialogueUI = BuildDialogueUI(canvas.transform);
            var titleCard = BuildTitleCard(canvas.transform, "Day 2 — Gerrold's Cottage",
                "Quiet. A heavy beat. Candlelight, soft rain optional.", Mission02SoPath);
            var choiceCard = BuildChoiceCard(canvas.transform);
            var ledger = BuildLedger(canvas.transform);

            EnsureEventSystem();

            // Cleanse mini-game host.
            var miniGO = new GameObject("_CleanseMiniGame");
            var cleanse = miniGO.AddComponent<CleanseMiniGame>();

            // Memory Dream sequencer host (Phase 21 fills in PlayableAssets if it ran).
            var dreamGO = new GameObject("_MemoryDreamRig");
            var sequencer = dreamGO.AddComponent<HearthboundHollow.Cutscene.MemoryDreamSequencer>();
            var dirPlayable = dreamGO.AddComponent<UnityEngine.Playables.PlayableDirector>();
            sequencer.director = dirPlayable;

            var directorGO = new GameObject("_Mission02Director");
            var dir = directorGO.AddComponent<Mission02Director>();
            dir.sceneRole = Mission02Director.SceneRole.Cottage;
            dir.playerController = player.GetComponent<PlayerController>();
            dir.dialogueUI = dialogueUI;
            dir.eveningLedger = ledger;
            dir.titleCard = titleCard;
            dir.gerroldTransform = gerrold.transform;
            dir.gerroldGreetingTrigger = greetingTrigger;
            dir.choiceCard = choiceCard;
            dir.gerroldOrb = orb;
            dir.cleanseGame = cleanse;
            dir.dreamSequencer = sequencer;
            dir.gerroldVillager = AssetDatabase.LoadAssetAtPath<VillagerSO>(GerroldVillagerPath);
            dir.gerroldMemory   = orb.memory;
            dir.tariffErase   = AssetDatabase.LoadAssetAtPath<TariffSO>(TariffErasePath);
            dir.tariffCleanse = AssetDatabase.LoadAssetAtPath<TariffSO>(TariffCleansePath);
            dir.tariffListen  = AssetDatabase.LoadAssetAtPath<TariffSO>(TariffListenPath);
            dir.tariffDefer   = AssetDatabase.LoadAssetAtPath<TariffSO>(TariffDeferPath);

            EditorSceneManager.SaveScene(scene, SceneCottage);
        }

        // ─── Helpers — player / NPC / props ───────────────────────

        private static GameObject CreatePlayer()
        {
            var bozoPlayer = Phase13_BoZoCharacterBuilder.TryGetPlayerPrefab();
            if (bozoPlayer != null)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(bozoPlayer);
                go.name = "Player";
                if (go.tag != "Player") go.tag = "Player";
                if (go.GetComponent<CharacterController>() == null)
                {
                    var cc = go.AddComponent<CharacterController>();
                    cc.center = new Vector3(0, 1.0f, 0); cc.height = 1.9f; cc.radius = 0.4f;
                }
                if (go.GetComponent<PlayerController>() == null) go.AddComponent<PlayerController>();
                return go;
            }
            // Fallback primitive
            var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = "Player";
            capsule.tag = "Player";
            capsule.transform.position = Vector3.up;
            DestroyImmediate(capsule.GetComponent<CapsuleCollider>());
            var cc2 = capsule.AddComponent<CharacterController>();
            cc2.center = new Vector3(0, 1.0f, 0); cc2.height = 1.9f; cc2.radius = 0.4f;
            capsule.AddComponent<PlayerController>();
            return capsule;
        }

        private static (GameObject go, Collider trigger) CreateGerrold(Vector3 pos)
        {
            var bozoGerrold = Phase13_BoZoCharacterBuilder.TryGetGerroldPrefab();
            if (bozoGerrold != null)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(bozoGerrold);
                go.name = "Gerrold";
                go.transform.position = pos;
                var trigger = go.transform.Find("Gerrold_Greeting_Zone")?.GetComponent<Collider>();
                if (trigger == null)
                {
                    var z = new GameObject("Gerrold_Greeting_Zone");
                    z.transform.SetParent(go.transform, false);
                    var s = z.AddComponent<SphereCollider>(); s.radius = 1.8f; s.isTrigger = true;
                    trigger = s;
                }
                return (go, trigger);
            }
            // Fallback cylinder
            var prim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            prim.name = "Gerrold";
            prim.transform.position = pos + Vector3.up;
            prim.transform.localScale = new Vector3(0.7f, 0.9f, 0.7f);
            var mr = prim.GetComponent<MeshRenderer>();
            mr.sharedMaterial = CreateUrpLitMaterial(MaterialsDir + "/Mat_Gerrold.mat", new Color(0.40f, 0.45f, 0.66f));
            var zone = new GameObject("Gerrold_Greeting_Zone");
            zone.transform.SetParent(prim.transform, false);
            var sphere = zone.AddComponent<SphereCollider>(); sphere.radius = 1.8f; sphere.isTrigger = true;
            return (prim, sphere);
        }

        private static HerbHarvestInteractable CreatePlantInteractable(string name, Vector3 pos, Color tint)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = name;
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.35f, 0.6f, 0.35f);
            // Per-plant material with shared cracked mat as fallback
            var matPath = MaterialsDir + "/Mat_" + name + ".mat";
            var pm = CreateUrpLitMaterial(matPath, tint);
            go.GetComponent<MeshRenderer>().sharedMaterial = pm;
            var hh = go.AddComponent<HerbHarvestInteractable>();
            // Provide a "ready" + "harvested" visual children — both share the same prim mesh
            // and are toggled in awake.
            var ready = new GameObject("Ready");
            ready.transform.SetParent(go.transform, false);
            var harvested = new GameObject("Harvested");
            harvested.transform.SetParent(go.transform, false);
            hh.readyVisual = ready;
            hh.harvestedVisual = harvested;
            return hh;
        }

        private static void CreateCottageWalls(Material mat)
        {
            void Wall(Vector3 pos, Vector3 scale)
            {
                var w = GameObject.CreatePrimitive(PrimitiveType.Cube);
                w.name = "Wall";
                w.transform.position = pos;
                w.transform.localScale = scale;
                w.GetComponent<MeshRenderer>().sharedMaterial = mat;
                w.isStatic = true;
            }
            Wall(new Vector3(-6, 2, 0), new Vector3(0.4f, 4, 12));
            Wall(new Vector3( 6, 2, 0), new Vector3(0.4f, 4, 12));
            Wall(new Vector3(0, 2, 6), new Vector3(12, 4, 0.4f));
        }

        // ─── Helpers — UI builders ───────────────────────────────

        private static GameObject CreateCanvas(string name)
        {
            var canvasGO = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            return canvasGO;
        }

        private static DialogueUI BuildDialogueUI(Transform canvas)
        {
            var bamao = Phase14_BamaoUIBuilder.TryGetDialogueBoxPrefab();
            if (bamao != null)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(bamao, canvas);
                go.name = "DialogueBox";
                go.SetActive(false);
                return go.GetComponent<DialogueUI>();
            }
            // Minimal primitive fallback
            var rootGO = new GameObject("DialogueBox", typeof(RectTransform));
            rootGO.transform.SetParent(canvas, false);
            var bg = rootGO.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.04f, 0.03f, 0.9f);
            var rt = bg.rectTransform;
            rt.anchorMin = new Vector2(0.10f, 0.06f); rt.anchorMax = new Vector2(0.90f, 0.32f);
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var name = new GameObject("Speaker").AddComponent<TextMeshProUGUI>();
            name.transform.SetParent(rootGO.transform, false);
            name.fontSize = 26; name.color = new Color(0.97f, 0.85f, 0.62f);
            name.rectTransform.anchorMin = new Vector2(0.04f, 0.78f); name.rectTransform.anchorMax = new Vector2(0.5f, 0.96f);
            var line = new GameObject("Line").AddComponent<TextMeshProUGUI>();
            line.transform.SetParent(rootGO.transform, false);
            line.fontSize = 22; line.color = new Color(0.92f, 0.88f, 0.78f);
            line.rectTransform.anchorMin = new Vector2(0.04f, 0.04f); line.rectTransform.anchorMax = new Vector2(0.96f, 0.74f);
            var dlg = rootGO.AddComponent<DialogueUI>();
            dlg.root = rootGO;
            dlg.speakerName = name;
            dlg.lineText = line;
            rootGO.SetActive(false);
            return dlg;
        }

        private static EveningLedgerUI BuildLedger(Transform canvas)
        {
            var bamao = Phase14_BamaoUIBuilder.TryGetEveningLedgerPrefab();
            if (bamao != null)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(bamao, canvas);
                go.name = "EveningLedger";
                go.SetActive(false);
                return go.GetComponent<EveningLedgerUI>();
            }
            var rootGO = new GameObject("EveningLedger", typeof(RectTransform));
            rootGO.transform.SetParent(canvas, false);
            var bg = rootGO.AddComponent<Image>();
            bg.color = new Color(0.03f, 0.02f, 0.01f, 0.94f);
            var rt = bg.rectTransform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var day = new GameObject("Day").AddComponent<TextMeshProUGUI>();
            day.transform.SetParent(rootGO.transform, false);
            day.fontSize = 54; day.alignment = TextAlignmentOptions.Center; day.color = new Color(0.97f, 0.85f, 0.62f);
            day.rectTransform.anchorMin = new Vector2(0, 0.82f); day.rectTransform.anchorMax = new Vector2(1, 0.95f);
            day.text = "Day 2";
            var prose = new GameObject("Prose").AddComponent<TextMeshProUGUI>();
            prose.transform.SetParent(rootGO.transform, false);
            prose.fontSize = 26; prose.alignment = TextAlignmentOptions.Center; prose.color = new Color(0.92f, 0.86f, 0.74f);
            prose.rectTransform.anchorMin = new Vector2(0.15f, 0.50f); prose.rectTransform.anchorMax = new Vector2(0.85f, 0.78f);
            var mem = new GameObject("Held").AddComponent<TextMeshProUGUI>();
            mem.transform.SetParent(rootGO.transform, false);
            mem.fontSize = 22; mem.alignment = TextAlignmentOptions.Center; mem.color = new Color(0.86f, 0.78f, 0.66f);
            mem.rectTransform.anchorMin = new Vector2(0.20f, 0.28f); mem.rectTransform.anchorMax = new Vector2(0.80f, 0.48f);
            var coin = new GameObject("Coin").AddComponent<TextMeshProUGUI>();
            coin.transform.SetParent(rootGO.transform, false);
            coin.fontSize = 22; coin.alignment = TextAlignmentOptions.Center; coin.color = new Color(0.96f, 0.84f, 0.50f);
            coin.rectTransform.anchorMin = new Vector2(0.20f, 0.22f); coin.rectTransform.anchorMax = new Vector2(0.80f, 0.28f);
            var btn = MakeButton(rootGO.transform, "Btn_EndOfDay", "Sleep — End Day", new Vector2(0.35f, 0.08f), new Vector2(0.65f, 0.18f));
            var ledger = rootGO.AddComponent<EveningLedgerUI>();
            ledger.root = rootGO;
            ledger.dayLabel = day;
            ledger.coinLabel = coin;
            ledger.summaryProse = prose;
            ledger.heldMemoriesList = mem;
            ledger.confirmEndOfDayButton = btn;
            rootGO.SetActive(false);
            return ledger;
        }

        private static MissionTitleCard BuildTitleCard(Transform canvas, string title, string tone, string missionSoPath)
        {
            var rootGO = new GameObject("MissionTitleCard", typeof(RectTransform));
            rootGO.transform.SetParent(canvas, false);
            var rt = rootGO.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var cg = rootGO.AddComponent<CanvasGroup>();

            var bg = new GameObject("Vignette").AddComponent<Image>();
            bg.transform.SetParent(rootGO.transform, false);
            bg.color = new Color(0.05f, 0.04f, 0.03f, 0.85f);
            var bgRT = bg.rectTransform;
            bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero;

            var day = new GameObject("Day").AddComponent<TextMeshProUGUI>();
            day.transform.SetParent(rootGO.transform, false);
            day.fontSize = 30; day.alignment = TextAlignmentOptions.Center; day.color = new Color(0.86f, 0.78f, 0.66f);
            day.rectTransform.anchorMin = new Vector2(0.2f, 0.62f); day.rectTransform.anchorMax = new Vector2(0.8f, 0.70f);

            var titleLabel = new GameObject("Title").AddComponent<TextMeshProUGUI>();
            titleLabel.transform.SetParent(rootGO.transform, false);
            titleLabel.fontSize = 64; titleLabel.alignment = TextAlignmentOptions.Center; titleLabel.color = new Color(0.97f, 0.85f, 0.62f);
            titleLabel.rectTransform.anchorMin = new Vector2(0.15f, 0.5f); titleLabel.rectTransform.anchorMax = new Vector2(0.85f, 0.62f);
            titleLabel.text = title;

            var toneLabel = new GameObject("Tone").AddComponent<TextMeshProUGUI>();
            toneLabel.transform.SetParent(rootGO.transform, false);
            toneLabel.fontSize = 26; toneLabel.fontStyle = FontStyles.Italic;
            toneLabel.alignment = TextAlignmentOptions.Center; toneLabel.color = new Color(0.86f, 0.78f, 0.66f);
            toneLabel.rectTransform.anchorMin = new Vector2(0.20f, 0.40f); toneLabel.rectTransform.anchorMax = new Vector2(0.80f, 0.48f);
            toneLabel.text = tone;

            var card = rootGO.AddComponent<MissionTitleCard>();
            card.root = rootGO;
            card.canvasGroup = cg;
            card.dayLabel = day;
            card.titleLabel = titleLabel;
            card.toneLabel = toneLabel;
            card.mission = AssetDatabase.LoadAssetAtPath<MissionSO>(missionSoPath);
            cg.alpha = 0f;
            return card;
        }

        private static TeaBrewingUI BuildTeaBrewingUI(Transform canvas, MemoryHerb lavender, MemoryHerb valerian)
        {
            var rootGO = new GameObject("TeaBrewing", typeof(RectTransform));
            rootGO.transform.SetParent(canvas, false);
            var bg = rootGO.AddComponent<Image>();
            bg.color = new Color(0.10f, 0.08f, 0.06f, 0.92f);
            var rt = bg.rectTransform;
            rt.anchorMin = new Vector2(0.18f, 0.18f); rt.anchorMax = new Vector2(0.82f, 0.82f);
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            var prompt = new GameObject("Prompt").AddComponent<TextMeshProUGUI>();
            prompt.transform.SetParent(rootGO.transform, false);
            prompt.fontSize = 32; prompt.alignment = TextAlignmentOptions.Center; prompt.color = new Color(0.97f, 0.85f, 0.62f);
            prompt.rectTransform.anchorMin = new Vector2(0.05f, 0.80f); prompt.rectTransform.anchorMax = new Vector2(0.95f, 0.92f);
            prompt.text = "Choose a herb to brew.";

            var herbContainer = new GameObject("HerbContainer", typeof(RectTransform));
            herbContainer.transform.SetParent(rootGO.transform, false);
            var hcRT = herbContainer.GetComponent<RectTransform>();
            hcRT.anchorMin = new Vector2(0.1f, 0.5f); hcRT.anchorMax = new Vector2(0.9f, 0.78f);
            hcRT.offsetMin = Vector2.zero; hcRT.offsetMax = Vector2.zero;
            var layout = herbContainer.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 16;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            var herbBtnTemplate = new GameObject("HerbButtonTemplate", typeof(Image), typeof(Button), typeof(LayoutElement));
            herbBtnTemplate.transform.SetParent(rootGO.transform, false);
            herbBtnTemplate.SetActive(false);
            var hbImg = herbBtnTemplate.GetComponent<Image>();
            hbImg.color = new Color(0.16f, 0.10f, 0.06f, 0.95f);
            var hbLabel = new GameObject("Label").AddComponent<TextMeshProUGUI>();
            hbLabel.transform.SetParent(herbBtnTemplate.transform, false);
            hbLabel.fontSize = 24; hbLabel.alignment = TextAlignmentOptions.Center; hbLabel.color = new Color(0.97f, 0.85f, 0.62f);
            hbLabel.rectTransform.anchorMin = Vector2.zero; hbLabel.rectTransform.anchorMax = Vector2.one;
            hbLabel.text = "Herb";

            var brewingPanel = new GameObject("BrewingPanel", typeof(RectTransform));
            brewingPanel.transform.SetParent(rootGO.transform, false);
            var brewRT = brewingPanel.GetComponent<RectTransform>();
            brewRT.anchorMin = new Vector2(0.1f, 0.25f); brewRT.anchorMax = new Vector2(0.9f, 0.48f);
            brewRT.offsetMin = Vector2.zero; brewRT.offsetMax = Vector2.zero;
            var brewLbl = new GameObject("Label").AddComponent<TextMeshProUGUI>();
            brewLbl.transform.SetParent(brewingPanel.transform, false);
            brewLbl.fontSize = 26; brewLbl.alignment = TextAlignmentOptions.Center; brewLbl.color = new Color(0.92f, 0.88f, 0.78f);
            brewLbl.rectTransform.anchorMin = new Vector2(0, 0.6f); brewLbl.rectTransform.anchorMax = new Vector2(1, 1);
            var sliderGO = new GameObject("Slider", typeof(Slider), typeof(Image));
            sliderGO.transform.SetParent(brewingPanel.transform, false);
            var sliderImg = sliderGO.GetComponent<Image>();
            sliderImg.color = new Color(0.20f, 0.14f, 0.10f, 0.9f);
            var sliderRT = sliderGO.GetComponent<RectTransform>();
            sliderRT.anchorMin = new Vector2(0.1f, 0.2f); sliderRT.anchorMax = new Vector2(0.9f, 0.5f);
            sliderRT.offsetMin = Vector2.zero; sliderRT.offsetMax = Vector2.zero;
            var slider = sliderGO.GetComponent<Slider>();
            slider.minValue = 0f; slider.maxValue = 1f; slider.value = 0f;

            var auto = MakeButton(rootGO.transform, "AutoComplete", "Brew now (auto)",
                new Vector2(0.3f, 0.08f), new Vector2(0.7f, 0.18f));

            var tea = rootGO.AddComponent<TeaBrewingUI>();
            tea.root = rootGO;
            tea.herbContainer = herbContainer.transform;
            tea.herbButtonPrefab = herbBtnTemplate;
            tea.promptLabel = prompt;
            tea.brewingPanel = brewingPanel;
            tea.brewProgress = slider;
            tea.brewLabel = brewLbl;
            tea.autoCompleteButton = auto;
            tea.brewDurationSeconds = 12f; // shorter for Mission 2 demo (was 90 — but 90 frustrates first-play)
            if (lavender != null) tea.availableHerbs.Add(lavender);
            if (valerian != null) tea.availableHerbs.Add(valerian);
            rootGO.SetActive(false);
            return tea;
        }

        private static ChoiceCardUI BuildChoiceCard(Transform canvas)
        {
            var rootGO = new GameObject("ChoiceCard", typeof(RectTransform));
            rootGO.transform.SetParent(canvas, false);
            var bg = rootGO.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.06f, 0.04f, 0.92f);
            var rt = bg.rectTransform;
            rt.anchorMin = new Vector2(0.10f, 0.16f); rt.anchorMax = new Vector2(0.90f, 0.86f);
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            var prompt = new GameObject("Prompt").AddComponent<TextMeshProUGUI>();
            prompt.transform.SetParent(rootGO.transform, false);
            prompt.fontSize = 32; prompt.alignment = TextAlignmentOptions.Center; prompt.color = new Color(0.97f, 0.85f, 0.62f);
            prompt.rectTransform.anchorMin = new Vector2(0.05f, 0.85f); prompt.rectTransform.anchorMax = new Vector2(0.95f, 0.97f);

            var memTitle = new GameObject("MemTitle").AddComponent<TextMeshProUGUI>();
            memTitle.transform.SetParent(rootGO.transform, false);
            memTitle.fontSize = 22; memTitle.fontStyle = FontStyles.Italic;
            memTitle.alignment = TextAlignmentOptions.Center; memTitle.color = new Color(0.86f, 0.78f, 0.66f);
            memTitle.rectTransform.anchorMin = new Vector2(0.05f, 0.78f); memTitle.rectTransform.anchorMax = new Vector2(0.95f, 0.85f);

            var container = new GameObject("Container", typeof(RectTransform));
            container.transform.SetParent(rootGO.transform, false);
            var cRT = container.GetComponent<RectTransform>();
            cRT.anchorMin = new Vector2(0.05f, 0.08f); cRT.anchorMax = new Vector2(0.95f, 0.74f);
            cRT.offsetMin = Vector2.zero; cRT.offsetMax = Vector2.zero;
            var grid = container.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(360, 200);
            grid.spacing = new Vector2(20, 20);
            grid.childAlignment = TextAnchor.MiddleCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;

            var template = new GameObject("TileTemplate", typeof(Image), typeof(Button));
            template.transform.SetParent(rootGO.transform, false);
            template.SetActive(false);
            var tImg = template.GetComponent<Image>();
            tImg.color = new Color(0.18f, 0.12f, 0.08f, 0.95f);
            var label = new GameObject("Label").AddComponent<TextMeshProUGUI>();
            label.transform.SetParent(template.transform, false);
            label.fontSize = 28; label.alignment = TextAlignmentOptions.Center; label.color = new Color(0.97f, 0.85f, 0.62f);
            var lRT = label.rectTransform;
            lRT.anchorMin = new Vector2(0, 0.55f); lRT.anchorMax = new Vector2(1, 0.95f);
            lRT.offsetMin = Vector2.zero; lRT.offsetMax = Vector2.zero;
            var cost = new GameObject("CostPreview").AddComponent<TextMeshProUGUI>();
            cost.transform.SetParent(template.transform, false);
            cost.fontSize = 18; cost.alignment = TextAlignmentOptions.Center; cost.color = new Color(0.86f, 0.78f, 0.66f);
            var coRT = cost.rectTransform;
            coRT.anchorMin = new Vector2(0.05f, 0.10f); coRT.anchorMax = new Vector2(0.95f, 0.55f);
            coRT.offsetMin = Vector2.zero; coRT.offsetMax = Vector2.zero;
            var icon = new GameObject("Icon", typeof(Image));
            icon.transform.SetParent(template.transform, false);
            var iRT = icon.GetComponent<RectTransform>();
            iRT.anchorMin = new Vector2(0.85f, 0.75f); iRT.anchorMax = new Vector2(0.98f, 0.96f);
            iRT.offsetMin = Vector2.zero; iRT.offsetMax = Vector2.zero;

            var choice = rootGO.AddComponent<ChoiceCardUI>();
            choice.root = rootGO;
            choice.promptLine = prompt;
            choice.memoryTitle = memTitle;
            choice.choiceContainer = container.transform;
            choice.choiceTilePrefab = template;
            rootGO.SetActive(false);
            return choice;
        }

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
            labelGO.fontSize = 24; labelGO.alignment = TextAlignmentOptions.Center; labelGO.color = new Color(0.97f, 0.85f, 0.62f);
            labelGO.text = label;
            var lRT = labelGO.rectTransform;
            lRT.anchorMin = Vector2.zero; lRT.anchorMax = Vector2.one;
            lRT.offsetMin = new Vector2(12, 4); lRT.offsetMax = new Vector2(-12, -4);
            return btnGO.GetComponent<Button>();
        }

        // ─── Helpers — scene infrastructure ──────────────────────

        private static void CreateDirectionalLight(float angleX, Color color, float intensity)
        {
            var lightGO = new GameObject("Directional Light");
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = color;
            light.intensity = intensity;
            light.shadows = LightShadows.Soft;
            lightGO.transform.rotation = Quaternion.Euler(angleX, 30, 0);
        }

        private static void CreateAmbient(bool low = false)
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            if (low)
            {
                RenderSettings.ambientSkyColor     = new Color(0.30f, 0.26f, 0.22f);
                RenderSettings.ambientEquatorColor = new Color(0.22f, 0.18f, 0.14f);
                RenderSettings.ambientGroundColor  = new Color(0.10f, 0.08f, 0.06f);
            }
            else
            {
                RenderSettings.ambientSkyColor     = new Color(0.50f, 0.46f, 0.40f);
                RenderSettings.ambientEquatorColor = new Color(0.34f, 0.30f, 0.24f);
                RenderSettings.ambientGroundColor  = new Color(0.18f, 0.14f, 0.10f);
            }
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

        private static void UpdateBuildSettings()
        {
            var current = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            void AddIfMissing(string p) { foreach (var s in current) if (s.path == p) return; current.Add(new EditorBuildSettingsScene(p, true)); }
            AddIfMissing(SceneGarden);
            AddIfMissing(SceneCottage);
            EditorBuildSettings.scenes = current.ToArray();
        }

        private static void EnsureDir(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            var leaf = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureDir(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        private static void DestroyImmediate(Object o) => Object.DestroyImmediate(o);
    }
}
