// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase77_SandboxSceneBuilder
//
// Builds (or rebuilds) the standalone prototype scene 06_SandboxProto.unity.
//
// Scene contents:
//   • Warm directional light
//   • Ground plane
//   • Cottage_A_Bakery facade (the Hollow front)
//   • Doris NPC prefab standing near the door
//   • Workbench (primitive cube) with orb (sphere + MemoryOrbInteractable)
//   • Player prefab, spawned a few metres from Doris
//   • Camera (CM_PlayerFollow prefab if found, else basic fallback)
//   • Simple dialogue UI (Canvas → Panel → TMP_Text lines)
//   • SandboxBootstrapper  (registers VillageState)
//   • SandboxVillagerDirector  (drives Doris dialogue + polish task)
//   • PolishMiniGame  (pre-configured for sandbox use)
//
// USE:  Hearthbound → ⚙️ Advanced → Phase 77 – Build Sandbox Scene
//
// Idempotent: running again deletes the old scene and rebuilds it fresh.
// Adds the scene to Build Settings slot 6.

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

namespace HearthboundHollow.EditorTools
{
    public static class Phase77_SandboxSceneBuilder
    {
        // ── Paths ──────────────────────────────────────────────────────────
        private const string ScenePath       = "Assets/_Project/Scenes/06_SandboxProto.unity";
        private const string ScenesDir       = "Assets/_Project/Scenes";

        private const string VillageStatePath = "Assets/_Project/ScriptableObjects/State/VillageState.asset";
        private const string MemoryPath       = "Assets/_Project/ScriptableObjects/Memories/DOR-001_FirstLoaves.asset";
        private const string PlayerPrefabPath = "Assets/_Project/Prefabs/Player/Player.prefab";
        private const string CamPrefabPath    = "Assets/_Project/Prefabs/Cameras/CM_PlayerFollow.prefab";
        private const string DorisPrefabPath  = "Assets/_Project/Prefabs/NPCs/Doris.prefab";
        private const string CottagePath      = "Assets/_Project/Prefabs/Environment/Cottage_A_Bakery.prefab";

        // ── Menu entry ─────────────────────────────────────────────────────
        [MenuItem("Hearthbound/⚙️ Advanced/Phase 77 – Build Sandbox Scene", priority = 77)]
        public static void Build()
        {
            EditorUtility.DisplayProgressBar("Hearthbound · Phase 77", "Loading assets…", 0.05f);
            try { DoBuild(); }
            finally { EditorUtility.ClearProgressBar(); }
        }

        // ── Master build ───────────────────────────────────────────────────
        private static void DoBuild()
        {
            // ── 1. Load required assets ────────────────────────────────────
            var villageState = AssetDatabase.LoadAssetAtPath<VillageState>(VillageStatePath);
            if (villageState == null)
            {
                EditorUtility.DisplayDialog("Phase 77",
                    "VillageState.asset not found.\nRun  Hearthbound → 🚀 Build Everything  first.",
                    "OK");
                return;
            }

            var memory = AssetDatabase.LoadAssetAtPath<MemoryNodeSO>(MemoryPath);
            if (memory == null)
            {
                Debug.LogWarning("[Phase77] DOR-001 not found — orb will have no memory data. " +
                                 "Run Build Everything to create it.");
            }

            // Optional prefabs — fall back to primitives if missing.
            var playerPrefab  = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            var camPrefab     = AssetDatabase.LoadAssetAtPath<GameObject>(CamPrefabPath);
            var dorisPrefab   = AssetDatabase.LoadAssetAtPath<GameObject>(DorisPrefabPath);
            var cottagePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CottagePath);

            // ── 2. Create scene ────────────────────────────────────────────
            EditorUtility.DisplayProgressBar("Hearthbound · Phase 77", "Creating scene…", 0.10f);
            EnsureDir(ScenesDir);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ── 3. Lighting ────────────────────────────────────────────────
            var lightGo = new GameObject("_DirectionalLight");
            var light   = lightGo.AddComponent<Light>();
            light.type      = LightType.Directional;
            light.color     = new Color(1f, 0.91f, 0.74f);   // warm amber
            light.intensity = 1.35f;
            lightGo.transform.rotation = Quaternion.Euler(42f, -28f, 0f);

            // ── 4. Ground ──────────────────────────────────────────────────
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(6f, 1f, 6f);  // 60 m × 60 m
            AssignUrpColor(ground, new Color(0.27f, 0.22f, 0.16f));  // dark earth

            // ── 5. Hollow facade ───────────────────────────────────────────
            EditorUtility.DisplayProgressBar("Hearthbound · Phase 77", "Placing Hollow…", 0.20f);
            GameObject hollow;
            if (cottagePrefab != null)
            {
                hollow = (GameObject)PrefabUtility.InstantiatePrefab(cottagePrefab);
                hollow.name = "Hollow_Facade";
                hollow.transform.position = new Vector3(0f, 0f, 8f);
            }
            else
            {
                hollow = GameObject.CreatePrimitive(PrimitiveType.Cube);
                hollow.name = "Hollow_Facade (placeholder)";
                hollow.transform.position  = new Vector3(0f, 2f, 8f);
                hollow.transform.localScale = new Vector3(6f, 4f, 1f);
                AssignUrpColor(hollow, new Color(0.55f, 0.38f, 0.22f));
            }

            // ── 6. Workbench ───────────────────────────────────────────────
            EditorUtility.DisplayProgressBar("Hearthbound · Phase 77", "Building workbench…", 0.30f);
            var benchParent = new GameObject("Workbench");
            benchParent.transform.position = new Vector3(-3f, 0f, 6f);

            var benchTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            benchTop.name = "BenchTop";
            benchTop.transform.SetParent(benchParent.transform, false);
            benchTop.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            benchTop.transform.localScale    = new Vector3(1.4f, 0.12f, 0.8f);
            AssignUrpColor(benchTop, new Color(0.35f, 0.22f, 0.12f));  // dark wood

            var benchLeg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            benchLeg.name = "BenchLegs";
            benchLeg.transform.SetParent(benchParent.transform, false);
            benchLeg.transform.localPosition = new Vector3(0f, 0.25f, 0f);
            benchLeg.transform.localScale    = new Vector3(1.2f, 0.5f, 0.7f);
            AssignUrpColor(benchLeg, new Color(0.28f, 0.18f, 0.10f));

            // ── 7. Memory orb on workbench ─────────────────────────────────
            EditorUtility.DisplayProgressBar("Hearthbound · Phase 77", "Creating orb…", 0.40f);
            var orbGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orbGo.name = "MemoryOrb_DOR001";
            orbGo.transform.SetParent(benchParent.transform, false);
            orbGo.transform.localPosition = new Vector3(0f, 0.72f, 0f);
            orbGo.transform.localScale    = Vector3.one * 0.26f;

            // Amber glow so it's identifiable immediately; MemoryOrbInteractable
            // will override this via MaterialPropertyBlock at runtime.
            AssignUrpColor(orbGo, new Color(1f, 0.72f, 0.22f), emissive: true,
                           emission: new Color(0.9f, 0.45f, 0.05f) * 1.4f);

            var orbInteractable = orbGo.AddComponent<MemoryOrbInteractable>();
            orbInteractable.memory      = memory;
            orbInteractable.orbRenderer = orbGo.GetComponent<Renderer>();

            // Starts hidden — SandboxVillagerDirector reveals it after Act 2.
            orbGo.SetActive(false);

            // ── 8. PolishMiniGame ──────────────────────────────────────────
            EditorUtility.DisplayProgressBar("Hearthbound · Phase 77", "Wiring mini-game…", 0.50f);
            var polishGo   = new GameObject("_PolishMiniGame");
            var polishGame = polishGo.AddComponent<PolishMiniGame>();
            polishGame.autoCompleteAvailable   = true;
            polishGame.verboseInputDiagnostics = true;  // useful during prototype testing

            // ── 9. Doris NPC ───────────────────────────────────────────────
            EditorUtility.DisplayProgressBar("Hearthbound · Phase 77", "Placing Doris…", 0.60f);
            GameObject dorisGo;
            if (dorisPrefab != null)
            {
                dorisGo = (GameObject)PrefabUtility.InstantiatePrefab(dorisPrefab);
                dorisGo.name = "Doris_NPC";
            }
            else
            {
                // Fallback: a capsule labelled as a placeholder.
                dorisGo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                dorisGo.name = "Doris_NPC (placeholder — import BoZo pack)";
                AssignUrpColor(dorisGo, new Color(0.80f, 0.55f, 0.30f));
            }
            dorisGo.transform.position = new Vector3(1.5f, 0f, 5.5f);
            dorisGo.transform.rotation = Quaternion.Euler(0f, 200f, 0f); // faces player

            // ── 10. Player ─────────────────────────────────────────────────
            EditorUtility.DisplayProgressBar("Hearthbound · Phase 77", "Spawning player…", 0.68f);
            GameObject playerGo;
            if (playerPrefab != null)
            {
                playerGo = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                playerGo.name = "Player";
            }
            else
            {
                playerGo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                playerGo.name = "Player (placeholder — attach PlayerController manually)";
                AssignUrpColor(playerGo, new Color(0.30f, 0.55f, 0.80f));
            }
            playerGo.transform.position = new Vector3(0f, 0f, -2f);    // starts in front of Doris

            var playerController = playerGo.GetComponentInChildren<PlayerController>();

            // ── 11. Camera ─────────────────────────────────────────────────
            EditorUtility.DisplayProgressBar("Hearthbound · Phase 77", "Setting up camera…", 0.76f);
            if (camPrefab != null)
            {
                var camGo = (GameObject)PrefabUtility.InstantiatePrefab(camPrefab);
                camGo.name = "CM_PlayerFollow";
                camGo.transform.position = new Vector3(0f, 4f, -8f);
                camGo.transform.rotation = Quaternion.Euler(18f, 0f, 0f);
            }
            else
            {
                var camGo = new GameObject("MainCamera");
                camGo.tag = "MainCamera";
                var cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
                cam.clearFlags       = CameraClearFlags.Skybox;
                cam.fieldOfView      = 60f;
                camGo.transform.position = new Vector3(0f, 4f, -8f);
                camGo.transform.rotation = Quaternion.Euler(18f, 0f, 0f);
            }

            // ── 12. Dialogue UI ────────────────────────────────────────────
            EditorUtility.DisplayProgressBar("Hearthbound · Phase 77", "Building dialogue UI…", 0.82f);
            var (dialogueTmp, hintTmp, statusTmp) = BuildDialogueUI();

            // ── 13. SandboxBootstrapper ────────────────────────────────────
            var bootstrapGo  = new GameObject("_SandboxBootstrapper");
            var bootstrapper = bootstrapGo.AddComponent<SandboxBootstrapper>();
            bootstrapper.villageState = villageState;

            // ── 14. SandboxVillagerDirector ────────────────────────────────
            EditorUtility.DisplayProgressBar("Hearthbound · Phase 77", "Wiring director…", 0.90f);
            var directorGo = new GameObject("_SandboxVillagerDirector");
            var director   = directorGo.AddComponent<SandboxVillagerDirector>();

            // Wire all references via SerializedObject so Unity tracks them properly.
            var so = new SerializedObject(director);
            so.FindProperty("villagerTransform").objectReferenceValue = dorisGo.transform;
            so.FindProperty("player").objectReferenceValue            = playerController;
            so.FindProperty("orbOnWorkbench").objectReferenceValue    = orbInteractable;
            so.FindProperty("polishGame").objectReferenceValue        = polishGame;
            so.FindProperty("dialogueText").objectReferenceValue      = dialogueTmp;
            so.FindProperty("hintText").objectReferenceValue          = hintTmp;
            so.FindProperty("statusText").objectReferenceValue        = statusTmp;
            so.ApplyModifiedPropertiesWithoutUndo();

            // ── 15. Save scene + Build Settings ───────────────────────────
            EditorUtility.DisplayProgressBar("Hearthbound · Phase 77", "Saving…", 0.96f);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings(ScenePath);
            AssetDatabase.Refresh();

            Debug.Log("[Phase77] ✅ 06_SandboxProto.unity built successfully.");

            EditorUtility.DisplayDialog("Phase 77 — Done!",
                "06_SandboxProto.unity is ready.\n\n" +
                "HOW TO PLAY:\n" +
                "1. Press ▶ Play\n" +
                "2. WASD to walk toward Doris\n" +
                "3. Press E to start the conversation\n" +
                "4. Press E to advance each line\n" +
                "5. Walk to the workbench — the orb appears after Act 2\n" +
                "6. Press E on the orb to start polishing\n" +
                "7. Hold Left Mouse + draw slow circles\n" +
                "8. Walk back to Doris for the reward",
                "Let's go!");
        }

        // ── UI builder ─────────────────────────────────────────────────────
        // Creates a simple Canvas with three TMP_Text elements:
        //   dialogueText  — lower-third panel for Doris's lines
        //   hintText      — small prompt above dialogue
        //   statusText    — coin counter top-right
        private static (TMP_Text dialogue, TMP_Text hint, TMP_Text status) BuildDialogueUI()
        {
            // Root canvas
            var canvasGo = new GameObject("_DialogueUI");
            var canvas   = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();

            // ── Dialogue panel (lower third) ───────────────────────────────
            var panelGo = new GameObject("DialoguePanel");
            panelGo.transform.SetParent(canvasGo.transform, false);
            var panelRect = panelGo.AddComponent<RectTransform>();
            SetAnchors(panelRect, new Vector2(0.05f, 0f), new Vector2(0.95f, 0.28f));
            var panelImg  = panelGo.AddComponent<Image>();
            panelImg.color = new Color(0.08f, 0.06f, 0.04f, 0.82f);   // dark warm bg
            panelGo.SetActive(false);   // hidden until dialogue starts

            var dialogueTmp = AddTmpText(panelGo, "DialogueText",
                anchor0: new Vector2(0.02f, 0.25f), anchor1: new Vector2(0.98f, 0.92f),
                fontSize: 28, color: new Color(0.96f, 0.88f, 0.72f),   // warm cream
                alignment: TextAlignmentOptions.MidlineLeft);

            // ── Hint text (above dialogue panel) ──────────────────────────
            var hintGo = new GameObject("HintText");
            hintGo.transform.SetParent(canvasGo.transform, false);
            var hintRect = hintGo.AddComponent<RectTransform>();
            SetAnchors(hintRect, new Vector2(0.25f, 0.26f), new Vector2(0.75f, 0.32f));
            var hintTmp  = hintGo.AddComponent<TextMeshProUGUI>();
            hintTmp.fontSize  = 18;
            hintTmp.color     = new Color(0.75f, 0.70f, 0.55f, 0.90f);
            hintTmp.alignment = TextAlignmentOptions.Center;
            hintTmp.fontStyle = FontStyles.Italic;
            hintGo.SetActive(false);

            // ── Status text (top-right — coin counter) ─────────────────────
            var statusGo = new GameObject("StatusText");
            statusGo.transform.SetParent(canvasGo.transform, false);
            var statusRect = statusGo.AddComponent<RectTransform>();
            SetAnchors(statusRect, new Vector2(0.78f, 0.93f), new Vector2(0.99f, 1.00f));
            var statusTmp  = statusGo.AddComponent<TextMeshProUGUI>();
            statusTmp.fontSize  = 20;
            statusTmp.color     = new Color(0.95f, 0.85f, 0.50f);      // gold
            statusTmp.alignment = TextAlignmentOptions.Right;
            statusTmp.text      = "Coins: 0";

            return (dialogueTmp, hintTmp, statusTmp);
        }

        // ── Utility: TMP_Text factory ──────────────────────────────────────
        private static TMP_Text AddTmpText(GameObject parent, string name,
            Vector2 anchor0, Vector2 anchor1,
            int fontSize, Color color, TextAlignmentOptions alignment)
        {
            var go   = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var rect = go.AddComponent<RectTransform>();
            SetAnchors(rect, anchor0, anchor1);
            var tmp       = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize  = fontSize;
            tmp.color     = color;
            tmp.alignment = alignment;
            tmp.enableWordWrapping = true;
            return tmp;
        }

        private static void SetAnchors(RectTransform rt,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            rt.anchorMin  = anchorMin;
            rt.anchorMax  = anchorMax;
            rt.offsetMin  = Vector2.zero;
            rt.offsetMax  = Vector2.zero;
        }

        // ── Utility: URP material coloring ────────────────────────────────
        private static void AssignUrpColor(GameObject go, Color baseColor,
            bool emissive = false, Color emission = default)
        {
            var rend = go.GetComponent<Renderer>();
            if (rend == null) return;
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ??
                                   Shader.Find("Standard"));
            mat.color = baseColor;
            if (emissive)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emission == default ? baseColor : emission);
            }
            rend.sharedMaterial = mat;
        }

        // ── Utility: Build Settings ────────────────────────────────────────
        private static void AddToBuildSettings(string path)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (var s in scenes)
                if (s.path == path) return;          // already present
            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void EnsureDir(string dir)
        {
            if (!AssetDatabase.IsValidFolder(dir))
                Directory.CreateDirectory(dir);
        }
    }
}
