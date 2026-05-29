// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase47_OneMoreDayBuilder
//
// PHASE 47 — One-click installer for the "One More Day" goodnight beat.
//
// Implements Docs/Phase47_OneMoreDay_Implementation.md §§ 7-9. Idempotent
// (load-or-create + heal-then-save). Registered under Hearthbound → ⚙️
// Advanced/… (D-051) and chained into 🚀 Build Everything.
//
// Responsibilities:
//   1. Create/heal the two TomorrowTeaseSO assets in ScriptableObjects/
//      Missions/ (only fills empty fields, so designer tweaks survive).
//   2. Create/heal Assets/_Project/Prefabs/UI/OneMoreDayCard.prefab — a
//      parchment goodnight overlay built from Unity's built-in UI sprites
//      (no generated textures → references always persist; matches the
//      Depth Layer's "procedural primitives only" discipline).
//   3. In scenes 03_Mission01_Hollow and 05_Mission02_Cottage: instantiate
//      the card under the gameplay canvas, add an EndOfDaySequencer, wire
//      card + dream + teases, then wire the scene director's
//      endOfDaySequencer (+ dreamSequencer on M1).
//   4. Clear DreamHook.ledger in those scenes so the sequencer owns dreams
//      (no double-play — see the doc § 5.4).
//   5. MarkSceneDirty + save.
//
// Falls back gracefully when a scene / director / canvas isn't present
// (logs a warning, never throws) so a partial branch still builds.

using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HearthboundHollow.Cutscene;
using HearthboundHollow.Mission;
using HearthboundHollow.UI;

namespace HearthboundHollow.EditorTools
{
    public static class Phase47_OneMoreDayBuilder
    {
        // ─── Paths ────────────────────────────────────────────────
        private const string HollowScenePath  = "Assets/_Project/Scenes/03_Mission01_Hollow.unity";
        private const string CottageScenePath = "Assets/_Project/Scenes/05_Mission02_Cottage.unity";
        private const string PrefabDir        = "Assets/_Project/Prefabs/UI";
        private const string PrefabPath       = "Assets/_Project/Prefabs/UI/OneMoreDayCard.prefab";
        private const string SoDir            = "Assets/_Project/ScriptableObjects/Missions";
        private const string SoM1Path         = "Assets/_Project/ScriptableObjects/Missions/Tomorrow_M1_Day1.asset";
        private const string SoM2Path         = "Assets/_Project/ScriptableObjects/Missions/Tomorrow_M2_Day2.asset";

        // Scene-instance node names (namespaced so RemoveOld is precise).
        private const string CardNodeName = "_OneMoreDayCard";
        private const string SeqNodeName  = "_EndOfDaySequencer";

        // ─── Menu ─────────────────────────────────────────────────

        [MenuItem("Hearthbound/⚙️ Advanced/🌙 Phase 47 — Build One More Day Hook", priority = 5210)]
        public static void Build()
        {
            EnsureTeaseAssets();
            var prefab = BuildCardPrefab();

            int wired = 0;
            wired += WireScene(HollowScenePath, isMission1: true, prefab) ? 1 : 0;
            wired += WireScene(CottageScenePath, isMission1: false, prefab) ? 1 : 0;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Phase 47 — One More Day",
                "✓ Goodnight beat installed.\n\n" +
                $"  • TomorrowTeaseSO assets healed ({SoM1Path.Substring(SoM1Path.LastIndexOf('/') + 1)}, " +
                $"{SoM2Path.Substring(SoM2Path.LastIndexOf('/') + 1)})\n" +
                "  • OneMoreDayCard.prefab built\n" +
                $"  • {wired}/2 scenes wired (Ledger → Dream → Goodnight Card → load)\n\n" +
                "Press Play in 00_Bootstrap, finish Day 1, and watch the order:\n" +
                "Evening Ledger → (Dream) → Tomorrow card → Day 2.",
                "OK");
        }

        // ─── 1. ScriptableObject teases (load-or-create + heal) ────

        private static void EnsureTeaseAssets()
        {
            EnsureFolder(SoDir);

            var m1 = LoadOrCreate(SoM1Path);
            if (string.IsNullOrEmpty(m1.sourceNode))         m1.sourceNode = "Tomorrow_M1_Day1";
            if (m1.afterDayIndex <= 0)                        m1.afterDayIndex = 1;
            if (string.IsNullOrWhiteSpace(m1.forwardLookText))
                m1.forwardLookText = "The shelf holds one more light than it did this morning. " +
                    "Doris said dreams come — so sleep, and let them. Tomorrow the lane will bring " +
                    "someone to your door. It usually does.";
            // Real VillageState field is `refusedDorisOrb` (the guide's
            // `dorisRefused` does not exist). Only fill if the designer left it blank.
            if (string.IsNullOrEmpty(m1.branchFlagField))     m1.branchFlagField = "refusedDorisOrb";
            if (string.IsNullOrEmpty(m1.sourceNodeAlt))       m1.sourceNodeAlt = "Tomorrow_M1_Day1_Refused";
            if (string.IsNullOrWhiteSpace(m1.forwardLookTextAlt))
                m1.forwardLookTextAlt = "You sent her home with her loaves still her own. That was " +
                    "allowed. The kettle's still warm, and the morning will still come. Rest.";
            if (string.IsNullOrEmpty(m1.pickleSourceNode))    m1.pickleSourceNode = "Pickle_Goodnight_M1";
            if (string.IsNullOrWhiteSpace(m1.pickleSignOffText))
                m1.pickleSignOffText = "Sleep. I will guard the shelf. Mostly.";
            EditorUtility.SetDirty(m1);

            var m2 = LoadOrCreate(SoM2Path);
            if (string.IsNullOrEmpty(m2.sourceNode))          m2.sourceNode = "Tomorrow_M2_Day2";
            if (m2.afterDayIndex <= 0)                         m2.afterDayIndex = 2;
            if (string.IsNullOrWhiteSpace(m2.forwardLookText))
                m2.forwardLookText = "The handkerchief is folded on the bench. Down the lane, a door " +
                    "will open again tomorrow — or it won't. Either way, the kettle stays warm, and " +
                    "you did a gentle thing today.";
            if (string.IsNullOrEmpty(m2.pickleSourceNode))    m2.pickleSourceNode = "Pickle_Goodnight_M2";
            if (string.IsNullOrWhiteSpace(m2.pickleSignOffText))
                m2.pickleSignOffText = "A man left lighter than he came. You did that. " +
                    "Don't let it go to your head.";
            EditorUtility.SetDirty(m2);

            AssetDatabase.SaveAssets();
        }

        private static TomorrowTeaseSO LoadOrCreate(string path)
        {
            var so = AssetDatabase.LoadAssetAtPath<TomorrowTeaseSO>(path);
            if (so != null) return so;
            so = ScriptableObject.CreateInstance<TomorrowTeaseSO>();
            AssetDatabase.CreateAsset(so, path);
            Debug.Log($"[Hearthbound/Phase 47] Created {path}.");
            return so;
        }

        // ─── 2. Card prefab (built from built-in UI sprites) ───────

        private static GameObject BuildCardPrefab()
        {
            EnsureFolder(PrefabDir);

            // Built-in UI sprites — guaranteed present, persist cleanly in
            // prefabs/scenes (no runtime-generated texture references to lose).
            Sprite panelSprite  = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            Sprite buttonSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            // Root (holds the component + the fade CanvasGroup), full-screen.
            var root = new GameObject("OneMoreDayCard", typeof(RectTransform));
            Stretch((RectTransform)root.transform);
            var canvasGroup = root.AddComponent<CanvasGroup>();
            var card = root.AddComponent<OneMoreDayCard>();

            // "Root" container — the field the script toggles on Show/Hide.
            var container = NewChild(root.transform, "Root");
            Stretch(container);

            // Backdrop — dark night wash, blocks clicks to the world behind.
            var backdrop = NewChild(container, "Backdrop");
            Stretch(backdrop);
            var backImg = backdrop.gameObject.AddComponent<Image>();
            backImg.color = new Color(0.03f, 0.03f, 0.05f, 0.82f);
            backImg.raycastTarget = true;

            // Parchment panel, centred.
            var panel = NewChild(container, "Panel");
            panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(980, 600);
            panel.anchoredPosition = Vector2.zero;
            var panelImg = panel.gameObject.AddComponent<Image>();
            panelImg.sprite = panelSprite;
            panelImg.type = Image.Type.Sliced;
            panelImg.color = UIReadabilityHelper.CreamPaper;
            panelImg.raycastTarget = true;

            // Headline ("Tomorrow").
            var headline = MakeLabel(panel, "Headline", new Vector2(0.5f, 1f),
                new Vector2(880, 130), new Vector2(0, -90), "Tomorrow", 72,
                TextAlignmentOptions.Center, UIReadabilityHelper.InkDark, FontStyles.Bold);

            // Forward-look prose.
            var fwd = MakeLabel(panel, "ForwardLook", new Vector2(0.5f, 0.5f),
                new Vector2(840, 250), new Vector2(0, 30), "", 32,
                TextAlignmentOptions.Center, UIReadabilityHelper.InkDark, FontStyles.Normal);

            // Pickle italic sign-off.
            var pickle = MakeLabel(panel, "Pickle", new Vector2(0.5f, 0f),
                new Vector2(840, 110), new Vector2(0, 150), "", 26,
                TextAlignmentOptions.Center, UIReadabilityHelper.InkBrown, FontStyles.Italic);

            // Goodnight button.
            var btnRT = NewChild(panel, "GoodnightButton");
            btnRT.anchorMin = btnRT.anchorMax = btnRT.pivot = new Vector2(0.5f, 0f);
            btnRT.sizeDelta = new Vector2(340, 92);
            btnRT.anchoredPosition = new Vector2(0, 50);
            var btnImg = btnRT.gameObject.AddComponent<Image>();
            btnImg.sprite = buttonSprite;
            btnImg.type = Image.Type.Sliced;
            btnImg.color = UIReadabilityHelper.GoldEmber;
            var button = btnRT.gameObject.AddComponent<Button>();
            var btnColors = button.colors;
            btnColors.highlightedColor = UIReadabilityHelper.GoldRich;
            btnColors.pressedColor = UIReadabilityHelper.CreamPaper;
            button.colors = btnColors;
            var btnLabel = MakeLabel(btnRT, "Label", new Vector2(0.5f, 0.5f),
                new Vector2(320, 80), Vector2.zero, "Goodnight", 30,
                TextAlignmentOptions.Center, UIReadabilityHelper.InkDark, FontStyles.Bold);
            btnLabel.enableWordWrapping = false;

            // Wire the component.
            card.root = container.gameObject;
            card.canvasGroup = canvasGroup;
            card.headlineLabel = headline;
            card.forwardLookLabel = fwd;
            card.pickleLabel = pickle;
            card.goodnightButton = button;
            card.headlineText = "Tomorrow";

            // Initial state inactive — the script self-heals + re-activates on
            // Show() (doc § 8.7). Keeps the saved scene clean (no overlay
            // floating in the editor view).
            container.gameObject.SetActive(false);

            // Save (overwrites in place → preserves the prefab GUID).
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            Debug.Log($"[Hearthbound/Phase 47] Built {PrefabPath}.");
            return prefab;
        }

        // ─── 3 + 4. Per-scene wiring ───────────────────────────────

        private static bool WireScene(string scenePath, bool isMission1, GameObject prefab)
        {
            if (!System.IO.File.Exists(scenePath))
            {
                Debug.LogWarning($"[Hearthbound/Phase 47] (skip) scene not found: {scenePath}. Run 🚀 Build Everything first.");
                return false;
            }
            if (prefab == null)
            {
                Debug.LogWarning("[Hearthbound/Phase 47] (skip) card prefab is null; cannot wire scenes.");
                return false;
            }

            var scene = EditorSceneManager.GetSceneByPath(scenePath);
            if (!scene.IsValid() || !scene.isLoaded)
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            RemoveOld(scene);

            // Instantiate the card under the gameplay canvas.
            var canvas = FindOrCreateCanvas(scene);
            var cardInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            cardInstance.name = CardNodeName;
            cardInstance.transform.SetParent(canvas.transform, false);
            cardInstance.transform.SetAsLastSibling();
            Stretch((RectTransform)cardInstance.transform);
            var card = cardInstance.GetComponent<OneMoreDayCard>();

            // The night-chain owner.
            var seqGO = new GameObject(SeqNodeName);
            SceneManager.MoveGameObjectToScene(seqGO, scene);
            var seq = seqGO.AddComponent<EndOfDaySequencer>();
            seq.card = card;
            seq.dream = Object.FindFirstObjectByType<MemoryDreamSequencer>();

            var m1Tease = AssetDatabase.LoadAssetAtPath<TomorrowTeaseSO>(SoM1Path);
            var m2Tease = AssetDatabase.LoadAssetAtPath<TomorrowTeaseSO>(SoM2Path);
            seq.teases = isMission1
                ? new[] { m1Tease }
                : new[] { m2Tease };

            // Wire the scene director(s).
            if (isMission1)
            {
                var dir = Object.FindFirstObjectByType<Mission01Director>();
                if (dir != null)
                {
                    dir.endOfDaySequencer = seq;
                    dir.dreamSequencer = seq.dream;   // sequencer plays Dream 1 in order
                }
                else Debug.LogWarning("[Hearthbound/Phase 47] Mission01Director not found in Hollow scene.");
            }
            else
            {
                // Cottage may host two Mission02Director instances by role;
                // wire the Cottage one (dream already plays pre-ledger → the
                // director passes playDream:false at the call site).
                var dir = FindCottageDirector();
                if (dir != null) dir.endOfDaySequencer = seq;
                else Debug.LogWarning("[Hearthbound/Phase 47] Mission02Director (Cottage) not found.");
            }

            // Let the sequencer own dreams: dormant DreamHook (clear its ledger
            // so OnEnable never subscribes — see the doc § 5.4).
            foreach (var hook in Object.FindObjectsByType<DreamHook>(FindObjectsSortMode.None))
                if (hook != null) hook.ledger = null;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[Hearthbound/Phase 47] Wired {scenePath}.");
            return true;
        }

        private static Mission02Director FindCottageDirector()
        {
            foreach (var d in Object.FindObjectsByType<Mission02Director>(FindObjectsSortMode.None))
                if (d != null && d.sceneRole == Mission02Director.SceneRole.Cottage) return d;
            // Fallback: any director (single-scene builds).
            return Object.FindFirstObjectByType<Mission02Director>();
        }

        private static void RemoveOld(UnityEngine.SceneManagement.Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == SeqNodeName) { Object.DestroyImmediate(root); continue; }
                // Card lives under a canvas; search the whole subtree.
                var stale = FindByNameDeep(root.transform, CardNodeName);
                if (stale != null) Object.DestroyImmediate(stale.gameObject);
            }
        }

        // ─── Canvas discovery ─────────────────────────────────────

        private static Canvas FindOrCreateCanvas(UnityEngine.SceneManagement.Scene scene)
        {
            // Prefer the canvas that already hosts the Evening Ledger so the
            // card sits in the same overlay layer.
            var ledger = Object.FindFirstObjectByType<EveningLedgerUI>();
            if (ledger != null)
            {
                var c = ledger.GetComponentInParent<Canvas>();
                if (c != null) return c.rootCanvas != null ? c.rootCanvas : c;
            }
            // Else any Screen-Space Overlay canvas.
            foreach (var cv in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
                if (cv != null && cv.renderMode == RenderMode.ScreenSpaceOverlay) return cv;

            // Else build a dedicated overlay canvas.
            var go = new GameObject("_OneMoreDayCanvas");
            SceneManager.MoveGameObjectToScene(go, scene);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32760;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        // ─── Small helpers ─────────────────────────────────────────

        private static TextMeshProUGUI MakeLabel(Transform parent, string name, Vector2 anchor,
            Vector2 size, Vector2 pos, string text, float fontSize,
            TextAlignmentOptions align, Color color, FontStyles style)
        {
            var rt = NewChild(parent, name);
            rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = align;
            tmp.enableWordWrapping = true;
            return tmp;
        }

        private static RectTransform NewChild(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static Transform FindByNameDeep(Transform t, string name)
        {
            if (t.name == name) return t;
            for (int i = 0; i < t.childCount; i++)
            {
                var hit = FindByNameDeep(t.GetChild(i), name);
                if (hit != null) return hit;
            }
            return null;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int slash = path.LastIndexOf('/');
            string parent = path.Substring(0, slash);
            string leaf = path.Substring(slash + 1);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
