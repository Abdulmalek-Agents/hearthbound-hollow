#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using HearthboundHollow.Memory;

namespace HearthboundHollow.Editor
{
    /// <summary>
    /// Phase 52 — Reading Nook idempotent installer.
    ///
    /// What it does in one click:
    ///   1. Creates the 5 MarinLetterFragmentSO assets under Resources/ReadingNook/.
    ///   2. Opens the Hollow scene (03_Mission01_Hollow).
    ///   3. Drops _ReadingNookArmchair on the Hollow scene near the hearth.
    ///   4. Drops _ReadingNookCanvas (ReadingNookOverlay UI) on the Hollow scene.
    ///   5. Wires all refs between Interactable ↔ Overlay ↔ Fragment SOs.
    ///   6. Saves and closes the scene.
    ///
    /// Idempotent: every step uses FindObjectOfType / existing-asset guards.
    /// Chained into 🚀 Build Everything as Step 24 (after Phase 73).
    ///
    /// Menu: Hearthbound → ⚙️ Advanced → 📖 Phase 52 — Build Reading Nook
    /// Phase 52.
    /// </summary>
    public static class Phase52_ReadingNookBuilder
    {
        // ─── Paths ────────────────────────────────────────────────────────────

        private const string kFragmentsDir = "Assets/_Project/Resources/ReadingNook";
        private const string kHollowScene  = "Assets/_Project/Scenes/03_Mission01_Hollow.unity";

        // ─── Menu entry ───────────────────────────────────────────────────────

        [MenuItem("Hearthbound/\u2699\uFE0F Advanced/\uD83D\uDCD6 Phase 52 \u2014 Build Reading Nook",
                  priority = 1052)]
        public static void BuildReadingNook()
        {
            Debug.Log("[Phase52] Building Reading Nook...");

            // 1. Letter fragment SOs.
            var frags = EnsureFragmentSOs();

            // 2. Hollow scene.
            var hollow = EditorSceneManager.OpenScene(kHollowScene, OpenSceneMode.Additive);
            try
            {
                // 3. Armchair + Interactable.
                var armchair = EnsureArmchairInScene();

                // 4. Canvas + Overlay.
                var overlayGO = EnsureOverlayInScene();
                var overlay   = overlayGO.GetComponent<HearthboundHollow.UI.ReadingNookOverlay>();

                // 5. Wire Interactable.
                var interactable = armchair.GetComponent<HearthboundHollow.Mission.ReadingNookInteractable>();
                if (interactable != null)
                {
                    var so = new SerializedObject(interactable);

                    // Assign overlay reference.
                    so.FindProperty("overlay").objectReferenceValue = overlay;

                    // Assign fragment list.
                    var fragProp = so.FindProperty("fragments");
                    fragProp.ClearArray();
                    for (int i = 0; i < frags.Count; i++)
                    {
                        fragProp.InsertArrayElementAtIndex(i);
                        fragProp.GetArrayElementAtIndex(i).objectReferenceValue = frags[i];
                    }

                    so.ApplyModifiedPropertiesWithoutUndo();
                }

                // 6. Save scene.
                EditorSceneManager.MarkSceneDirty(hollow);
                EditorSceneManager.SaveScene(hollow);
                Debug.Log("[Phase52] \u2705 Reading Nook built and wired.");
            }
            finally
            {
                EditorSceneManager.CloseScene(hollow, removeScene: true);
            }
        }

        // ─── Fragment SO creation ─────────────────────────────────────────────

        /// <summary>
        /// Creates or updates the 5 MarinLetterFragmentSO assets.
        /// Returns them ordered by warmthThreshold.
        /// </summary>
        private static List<MarinLetterFragmentSO> EnsureFragmentSOs()
        {
            if (!Directory.Exists(kFragmentsDir))
                Directory.CreateDirectory(kFragmentsDir);

            var data = new[]
            {
                (id: "marin_letter_01", title: "Day One", threshold: 0,  warmthGrant: 5,
                 echo: "",
                 text:
"The shop smelled of beeswax and something older. Not unpleasant.\n" +
"More like a library that had decided to stop apologising for its age.\n\n" +
"I found two orbs on the back shelf that no one had labelled.\n" +
"One was the colour of a summer afternoon that had gone on slightly too long.\n" +
"The other was darker, with a crack I couldn\u2019t account for.\n\n" +
"I left them both where they were.\n" +
"You should too, until you\u2019re ready.\n\n\u2014 M"),

                (id: "marin_letter_02", title: "The Borrowed Key", threshold: 15, warmthGrant: 5,
                 echo: "MAR-NOTE-01",
                 text:
"There is a crate beneath the workbench. Unremarkable.\n" +
"I wouldn\u2019t have looked inside if Pickle hadn\u2019t sat on it for three consecutive days,\n" +
"which for Pickle is the nearest thing to a recommendation.\n\n" +
"Inside: seven memories, none of them listed in the Hollow\u2019s ledger.\n" +
"None of them priced.\n\n" +
"Someone brought these here and never collected them.\n" +
"Or never intended to.\n\n" +
"I put them back. I put them back for the same reason you put letters\n" +
"back in envelopes you were never meant to open.\n\n" +
"But I noted the colours.\n\n\u2014 M"),

                (id: "marin_letter_03", title: "The Sunday Purchase", threshold: 30, warmthGrant: 5,
                 echo: "ECHO-MARIN-01",
                 text:
"I sold a memory today that I should not have sold.\n\n" +
"Not because the buyer was wrong to want it.\n" +
"Not because the seller was wrong to let it go.\n" +
"But because when I held it in the light, I recognised\n" +
"a face in it that I had seen in another orb entirely.\n\n" +
"The same face. A different year. A different guilt.\n\n" +
"Someone has been very careful about what they let you see.\n" +
"They did not account for a Hollow-keeper who takes notes.\n\n\u2014 M"),

                (id: "marin_letter_04", title: "What the Village Forgot", threshold: 50, warmthGrant: 7,
                 echo: "GER-WIFE-01",
                 text:
"There was a family here once.\n" +
"The village doesn\u2019t speak of them.\n" +
"I\u2019ve found their faces in seven different memories now,\n" +
"spread across six different people who claim they never knew each other well.\n\n" +
"People who never knew each other well don\u2019t share the same expressions of shame\n" +
"in the same lamplight of the same year.\n\n" +
"They sealed something. All of them, together.\n" +
"And they paid the previous keeper to help.\n\n" +
"I don\u2019t know what.\n" +
"But the shape of it is becoming visible,\n" +
"the way a constellation becomes visible when you stop looking at the bright stars\n" +
"and look instead at the dark gaps between them.\n\n\u2014 M"),

                (id: "marin_letter_05", title: "Why I Left", threshold: 70, warmthGrant: 8,
                 echo: "",
                 text:
"I tried to buy them back. The fragments.\n" +
"I offered fair price. Then more than fair.\n" +
"Then everything I had made in three years of keeping this shop.\n\n" +
"They smiled the way people smile when they\u2019ve already decided.\n\n" +
"The thing about keeping a memory shop in a small village is that\n" +
"the village needs to trust you with its grief.\n" +
"The moment it decides you cannot be trusted \u2014\n" +
"and the moment you decide you cannot trust it \u2014\n" +
"you are keeping a shop in a place that is not yours anymore.\n\n" +
"I left the shop to whoever would find it next.\n" +
"I left the orbs where they were.\n" +
"I left Pickle \u2014 he\u2019ll manage better than me.\n\n" +
"I\u2019m sorry I couldn\u2019t finish it.\n" +
"I hope you\u2019re braver than I was.\n" +
"Or luckier. Both, preferably.\n\n\u2014 M. Vellis")
            };

            var result = new List<MarinLetterFragmentSO>();
            foreach (var d in data)
            {
                string path = $"{kFragmentsDir}/{d.id}.asset";
                var so = AssetDatabase.LoadAssetAtPath<MarinLetterFragmentSO>(path);
                if (so == null)
                {
                    so = ScriptableObject.CreateInstance<MarinLetterFragmentSO>();
                    AssetDatabase.CreateAsset(so, path);
                }

                // Always heal fields (idempotent).
                so.fragmentId     = d.id;
                so.title          = d.title;
                so.warmthThreshold = d.threshold;
                so.warmthGranted  = d.warmthGrant;
                so.echoConnectionId = d.echo;
                so.letterText     = d.text;
                EditorUtility.SetDirty(so);
                result.Add(so);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Phase52] Created/healed {result.Count} letter fragment SOs in {kFragmentsDir}.");
            return result;
        }

        // ─── Scene helpers ────────────────────────────────────────────────────

        private static GameObject EnsureArmchairInScene()
        {
            const string kName = "_ReadingNookArmchair";
            var existing = GameObject.Find(kName);
            if (existing != null)
            {
                Debug.Log($"[Phase52] Armchair already in scene ({kName}), healing...");
                EnsureInteractableComponent(existing);
                return existing;
            }

            // ── Build armchair placeholder from primitives ────────────────────
            // A simple cube approximating an armchair — visual artists will swap
            // this for a proper mesh asset without code change.
            var root = new GameObject(kName);

            // Seat cushion.
            var seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seat.name = "Seat";
            seat.transform.SetParent(root.transform);
            seat.transform.localPosition = new Vector3(0f, 0.25f, 0f);
            seat.transform.localScale    = new Vector3(0.8f, 0.2f, 0.8f);
            ApplyCozyCream(seat);

            // Back rest.
            var back = GameObject.CreatePrimitive(PrimitiveType.Cube);
            back.name = "BackRest";
            back.transform.SetParent(root.transform);
            back.transform.localPosition = new Vector3(0f, 0.7f, -0.4f);
            back.transform.localScale    = new Vector3(0.8f, 0.7f, 0.12f);
            ApplyCozyCream(back);

            // Left arm.
            var armL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            armL.name = "ArmLeft";
            armL.transform.SetParent(root.transform);
            armL.transform.localPosition = new Vector3(-0.44f, 0.5f, 0f);
            armL.transform.localScale    = new Vector3(0.1f, 0.35f, 0.8f);
            ApplyCozyCream(armL);

            // Right arm.
            var armR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            armR.name = "ArmRight";
            armR.transform.SetParent(root.transform);
            armR.transform.localPosition = new Vector3(0.44f, 0.5f, 0f);
            armR.transform.localScale    = new Vector3(0.1f, 0.35f, 0.8f);
            ApplyCozyCream(armR);

            // Box collider on root for the Interactable trigger.
            var col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.5f, 0f);
            col.size   = new Vector3(1.0f, 1.2f, 0.9f);
            col.isTrigger = true;

            // Place near the hearth — adjust per scene layout.
            // (Phase 32.3 places the hearth around Hollow transform origin;
            //  the armchair sits slightly to the right.)
            root.transform.localPosition = new Vector3(1.8f, 0f, 1.2f);
            root.transform.localEulerAngles = new Vector3(0f, -40f, 0f);

            // Add a warm point light for atmosphere.
            var warmLight = new GameObject("ArmchairLight").AddComponent<Light>();
            warmLight.transform.SetParent(root.transform);
            warmLight.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            warmLight.type      = LightType.Point;
            warmLight.color     = new Color(1.0f, 0.82f, 0.55f);
            warmLight.intensity = 0.6f;
            warmLight.range     = 2.5f;

            EnsureInteractableComponent(root);
            Debug.Log($"[Phase52] Created armchair placeholder at {root.transform.localPosition}.");
            return root;
        }

        private static void EnsureInteractableComponent(GameObject go)
        {
            if (go.GetComponent<HearthboundHollow.Mission.ReadingNookInteractable>() == null)
                go.AddComponent<HearthboundHollow.Mission.ReadingNookInteractable>();
        }

        private static GameObject EnsureOverlayInScene()
        {
            const string kName = "_ReadingNookCanvas";

            // Reuse if already present (idempotent).
            var existingGO = GameObject.Find(kName);
            if (existingGO != null)
            {
                Debug.Log($"[Phase52] Overlay canvas already in scene ({kName}), reusing.");
                if (existingGO.GetComponent<HearthboundHollow.UI.ReadingNookOverlay>() == null)
                    existingGO.AddComponent<HearthboundHollow.UI.ReadingNookOverlay>();
                return existingGO;
            }

            // ── Build UI hierarchy ───────────────────────────────────────────
            var canvasGO = new GameObject(kName);
            var canvas   = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 60; // above standard HUD
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            var cg = canvasGO.AddComponent<CanvasGroup>();
            cg.alpha = 0f; cg.blocksRaycasts = false; cg.interactable = false;

            var overlay = canvasGO.AddComponent<HearthboundHollow.UI.ReadingNookOverlay>();

            // Dark vignette background.
            var bg = MakeImage(canvasGO.transform, "Background",
                new Color(0.1f, 0.07f, 0.04f, 0.88f));
            StretchFull(bg.rectTransform);

            // Parchment panel.
            var parchGO = MakeImage(canvasGO.transform, "ParchmentPanel",
                new Color(0.96f, 0.91f, 0.80f, 1f));
            parchGO.rectTransform.anchorMin = new Vector2(0.1f, 0.08f);
            parchGO.rectTransform.anchorMax = new Vector2(0.9f, 0.94f);
            parchGO.rectTransform.offsetMin = Vector2.zero;
            parchGO.rectTransform.offsetMax = Vector2.zero;

            // Header.
            var header = MakeTMP(parchGO.transform, "HeaderText",
                "Reading Nook", 22f, new Color(0.36f, 0.22f, 0.12f));
            header.rectTransform.anchorMin  = new Vector2(0.05f, 0.88f);
            header.rectTransform.anchorMax  = new Vector2(0.95f, 0.98f);
            header.rectTransform.offsetMin  = Vector2.zero;
            header.rectTransform.offsetMax  = Vector2.zero;
            header.fontStyle = FontStyles.Bold;
            header.alignment = TextAlignmentOptions.Center;

            // ── Fragment List Panel ──────────────────────────────────────────
            var listPanelGO = new GameObject("FragmentListPanel");
            listPanelGO.transform.SetParent(parchGO.transform, false);
            var listRect = listPanelGO.AddComponent<RectTransform>();
            listRect.anchorMin = new Vector2(0.05f, 0.1f);
            listRect.anchorMax = new Vector2(0.95f, 0.86f);
            listRect.offsetMin = Vector2.zero;
            listRect.offsetMax = Vector2.zero;

            // Scroll container.
            var scrollGO = new GameObject("FragmentScrollContainer");
            scrollGO.transform.SetParent(listPanelGO.transform, false);
            var scrollRect = scrollGO.AddComponent<RectTransform>();
            StretchFull(scrollRect);
            var vlg = scrollGO.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8f;
            vlg.padding = new RectOffset(12, 12, 8, 8);
            vlg.childControlHeight = false;
            vlg.childControlWidth  = true;
            vlg.childForceExpandHeight = false;
            scrollGO.AddComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            // Fragment button prefab (simple cream button).
            var btnPrefab = MakeFragmentButton();

            // ── Letter View Panel ────────────────────────────────────────────
            var letterPanelGO = new GameObject("LetterViewPanel");
            letterPanelGO.transform.SetParent(parchGO.transform, false);
            var letterRect = letterPanelGO.AddComponent<RectTransform>();
            letterRect.anchorMin = new Vector2(0.05f, 0.1f);
            letterRect.anchorMax = new Vector2(0.95f, 0.86f);
            letterRect.offsetMin = Vector2.zero;
            letterRect.offsetMax = Vector2.zero;
            letterPanelGO.SetActive(false);

            var ltTitle = MakeTMP(letterPanelGO.transform, "LetterTitle",
                "", 18f, new Color(0.36f, 0.22f, 0.12f));
            var ltTitleRect = ltTitle.rectTransform;
            ltTitleRect.anchorMin = new Vector2(0, 0.88f);
            ltTitleRect.anchorMax = Vector2.one;
            ltTitleRect.offsetMin = Vector2.zero; ltTitleRect.offsetMax = Vector2.zero;
            ltTitle.fontStyle = FontStyles.Italic;
            ltTitle.alignment = TextAlignmentOptions.Center;

            var ltBody = MakeTMP(letterPanelGO.transform, "LetterBody",
                "", 14f, new Color(0.22f, 0.13f, 0.07f));
            var ltBodyRect = ltBody.rectTransform;
            ltBodyRect.anchorMin = new Vector2(0, 0.1f);
            ltBodyRect.anchorMax = new Vector2(1, 0.86f);
            ltBodyRect.offsetMin = Vector2.zero; ltBodyRect.offsetMax = Vector2.zero;
            ltBody.alignment = TextAlignmentOptions.TopLeft;
            ltBody.enableWordWrapping = true;

            // Back button.
            var backBtn = MakeButton(letterPanelGO.transform, "BackButton",
                "\u2190 Back to letters");
            var backRect = ((RectTransform)backBtn.transform);
            backRect.anchorMin = new Vector2(0f, 0f);
            backRect.anchorMax = new Vector2(0.4f, 0.09f);
            backRect.offsetMin = Vector2.zero; backRect.offsetMax = Vector2.zero;

            // ── Close button (always visible) ────────────────────────────────
            var closeBtn = MakeButton(parchGO.transform, "CloseButton", "Close with care");
            var closeBtnRect = ((RectTransform)closeBtn.transform);
            closeBtnRect.anchorMin = new Vector2(0.3f, 0.01f);
            closeBtnRect.anchorMax = new Vector2(0.7f, 0.09f);
            closeBtnRect.offsetMin = Vector2.zero; closeBtnRect.offsetMax = Vector2.zero;

            // ── Wire SerializedObject ────────────────────────────────────────
            var sov = new SerializedObject(overlay);
            sov.FindProperty("canvasGroup").objectReferenceValue         = cg;
            sov.FindProperty("fragmentListPanel").objectReferenceValue   = listPanelGO;
            sov.FindProperty("fragmentListContainer").objectReferenceValue = scrollGO.transform;
            sov.FindProperty("fragmentButtonPrefab").objectReferenceValue = btnPrefab;
            sov.FindProperty("letterViewPanel").objectReferenceValue     = letterPanelGO;
            sov.FindProperty("letterTitleText").objectReferenceValue     = ltTitle;
            sov.FindProperty("letterBodyText").objectReferenceValue      = ltBody;
            sov.FindProperty("backToListButton").objectReferenceValue    = backBtn;
            sov.FindProperty("closeButton").objectReferenceValue         = closeBtn;
            sov.FindProperty("nookHeaderText").objectReferenceValue      = header;
            sov.ApplyModifiedPropertiesWithoutUndo();

            // Destroy the temporary prefab GO (it was only needed for the reference).
            // The real buttons are spawned at runtime from the scene-local prefab.
            // We store the prefab under the canvas (hidden).
            btnPrefab.transform.SetParent(canvasGO.transform);
            btnPrefab.SetActive(false);

            Debug.Log("[Phase52] Reading Nook canvas built.");
            return canvasGO;
        }

        // ─── UI helpers ───────────────────────────────────────────────────────

        private static GameObject MakeFragmentButton()
        {
            var go  = new GameObject("_FragmentButtonTemplate");
            var rt  = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 42);

            var img = go.AddComponent<Image>();
            img.color = new Color(0.93f, 0.87f, 0.72f, 1f);

            var btn = go.AddComponent<Button>();
            var cb  = btn.colors;
            cb.highlightedColor = new Color(0.99f, 0.94f, 0.80f);
            btn.colors = cb;

            var lbl = MakeTMP(go.transform, "Label", "", 15f,
                new Color(0.36f, 0.22f, 0.12f));
            StretchFull(lbl.rectTransform);
            lbl.margin = new Vector4(12, 4, 12, 4);
            lbl.alignment = TextAlignmentOptions.MidlineLeft;

            return go;
        }

        private static Image MakeImage(Transform parent, string name, Color color)
        {
            var go  = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var img = go.AddComponent<Image>();
            img.color = color;
            return img;
        }

        private static TextMeshProUGUI MakeTMP(
            Transform parent, string name, string text, float fontSize, Color color)
        {
            var go  = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text      = text;
            tmp.fontSize  = fontSize;
            tmp.color     = color;
            tmp.enableWordWrapping = true;
            return tmp;
        }

        private static Button MakeButton(Transform parent, string name, string label)
        {
            var go  = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var img = go.AddComponent<Image>();
            img.color = new Color(0.72f, 0.52f, 0.25f);
            var btn = go.AddComponent<Button>();
            var lbl = MakeTMP(go.transform, "Label", label, 13f, Color.white);
            StretchFull(lbl.rectTransform);
            lbl.alignment = TextAlignmentOptions.Center;
            lbl.fontStyle = FontStyles.Bold;
            return btn;
        }

        private static void StretchFull(RectTransform rt)
        {
            rt.anchorMin  = Vector2.zero;
            rt.anchorMax  = Vector2.one;
            rt.offsetMin  = Vector2.zero;
            rt.offsetMax  = Vector2.zero;
        }

        private static void ApplyCozyCream(GameObject go)
        {
            var r = go.GetComponent<Renderer>();
            if (r == null) return;
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.95f, 0.88f, 0.74f);
            r.sharedMaterial = mat;
        }
    }
}
#endif
