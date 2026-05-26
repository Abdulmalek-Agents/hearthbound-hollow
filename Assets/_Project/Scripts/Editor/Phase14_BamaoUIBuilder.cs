// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase14_BamaoUIBuilder
//
// Phase 14 — Bamao Parchment UI Skin.
//
// Replaces the Phase 12 flat-color UI backgrounds (dialogue box, choice
// tile, evening ledger, tooltip frame) with real Bamao Fantasy GUI sprites.
//
// USE: Menu → Hearthbound → Phase 14 — Build Bamao UI Prefabs
//
// Output prefabs (saved under Assets/_Project/Prefabs/UI/):
//   • UI_DialogueBox_Bamao.prefab    (parchment background, portrait slot, TMP, choices container)
//   • UI_ChoiceTile_Bamao.prefab     (scroll-frame button, used by DialogueUI for choices)
//   • UI_EveningLedger_Bamao.prefab  (open-book background, two-page layout, save slots, end-of-day button)
//   • UI_TooltipFrame_Bamao.prefab   (tooltip parchment, used by CodexUI for examine hovers)
//
// Sprite detection: scores every sprite under Assets/Bamao/ by name keywords
// + path heuristics + dimension thresholds. Top picks are logged.
// If a sprite cannot be auto-detected for a slot, the prefab still works
// (warm tinted color fallback) and the user can drop a sprite in via Inspector.
//
// ── Phase 29 polish (2026-05-25) ──────────────────────────────────────
// Several text labels were appearing clipped on smaller / non-1080p
// canvases. We now:
//   • attach a UIAutoFitText helper to every TMP label, forcing word-
//     wrap + auto-size between sane min/max + ellipsis on overflow.
//   • reposition the DialogueBox's ChoicesContainer INSIDE the dialogue
//     box (it used to be anchored above the prefab bounds and the scene
//     builder didn't always reposition it — choices rendered off-screen).
//   • shrink the default body font size slightly and let auto-size grow
//     it for short lines.
//   • tighten the EveningLedger column widths + force word-wrap on the
//     summary prose / held-memories list.

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HearthboundHollow.UI;

namespace HearthboundHollow.EditorTools
{
    public static class Phase14_BamaoUIBuilder
    {
        private const string BamaoRoot = "Assets/Bamao";
        private const string UIPrefabDir = "Assets/_Project/Prefabs/UI";

        private const string DialogueBoxPath = UIPrefabDir + "/UI_DialogueBox_Bamao.prefab";
        private const string ChoiceTilePath = UIPrefabDir + "/UI_ChoiceTile_Bamao.prefab";
        private const string EveningLedgerPath = UIPrefabDir + "/UI_EveningLedger_Bamao.prefab";
        private const string TooltipFramePath = UIPrefabDir + "/UI_TooltipFrame_Bamao.prefab";

        // Bamao theme — warm parchment palette (matches Asset_Analysis_Mission1-2 § 5 S-4 "Hearthbound" preset).
        private static readonly Color ParchmentTint = new Color(0.98f, 0.94f, 0.82f, 1f);
        private static readonly Color InkColor      = new Color(0.22f, 0.16f, 0.10f, 1f);
        private static readonly Color SpeakerInk    = new Color(0.42f, 0.24f, 0.10f, 1f);
        private static readonly Color GoldEmber     = new Color(0.92f, 0.70f, 0.34f, 1f);
        private static readonly Color FallbackBg    = new Color(0.96f, 0.88f, 0.66f, 0.95f); // used when no sprite found

        // ─── Menu ──────────────────────────────────────────────────

        [MenuItem("Hearthbound/Phase 14 — Build Bamao UI Prefabs", priority = 201)]
        public static void Build()
        {
            if (!AssetDatabase.IsValidFolder(BamaoRoot))
            {
                EditorUtility.DisplayDialog(
                    "Phase 14 — Bamao not found",
                    $"Could not find {BamaoRoot}/.\n\n" +
                    "Please import the 'Bamao Fantasy GUI' asset pack from the Asset Store, " +
                    "then re-run this menu item.",
                    "OK");
                return;
            }

            EnsureFolder("Assets/_Project/Prefabs");
            EnsureFolder(UIPrefabDir);

            // Detect sprites once and log them
            var parchment = FindBamaoSprite("dialogue background",
                new[] { "parchment", "paper", "panel", "background", "scroll_large", "dialog", "speech" },
                minWidth: 200, minHeight: 100);
            var scrollBtn = FindBamaoSprite("choice tile",
                new[] { "scroll", "button", "btn", "tile", "ribbon", "small" },
                minWidth: 80, minHeight: 30);
            var book = FindBamaoSprite("open book / ledger",
                new[] { "book", "open_book", "tome", "two_page", "spread" },
                minWidth: 400, minHeight: 280);
            var frame = FindBamaoSprite("tooltip frame",
                new[] { "frame", "border", "tooltip", "card" });

            BuildDialogueBox(parchment, scrollBtn);
            BuildChoiceTile(scrollBtn);
            BuildEveningLedger(book ?? parchment);  // ledger uses book if found, falls back to parchment
            BuildTooltipFrame(frame ?? parchment);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Phase 14 — Done",
                "Built 4 Bamao UI prefabs:\n" +
                "  • " + DialogueBoxPath + "\n" +
                "  • " + ChoiceTilePath + "\n" +
                "  • " + EveningLedgerPath + "\n" +
                "  • " + TooltipFramePath + "\n\n" +
                "Re-run 'Hearthbound → Build Playable Mission 1 (One Click)' — the scene builder will now use these " +
                "prefabs instead of flat-color UI.\n\n" +
                (parchment == null ? "⚠️ Parchment sprite not auto-detected — prefab uses warm-tint fallback. Drop a Bamao parchment sprite onto the DialogueBox's Image component to upgrade.\n" : "") +
                (scrollBtn == null ? "⚠️ Scroll button sprite not auto-detected.\n" : "") +
                (book == null ? "⚠️ Open-book sprite not auto-detected — ledger uses parchment fallback.\n" : ""),
                "OK");
        }

        // ─── Sprite detection ──────────────────────────────────────

        private static Sprite FindBamaoSprite(string roleLabel, string[] nameKeywords,
                                              int minWidth = 0, int minHeight = 0)
        {
            var guids = AssetDatabase.FindAssets("t:Sprite", new[] { BamaoRoot });
            var candidates = new List<(string path, Sprite sprite, int score)>();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite == null) continue;

                int score = 0;
                var lowerPath = path.ToLowerInvariant();
                var lowerName = sprite.name.ToLowerInvariant();

                foreach (var kw in nameKeywords)
                {
                    if (lowerName.Contains(kw)) score += 25;
                    else if (lowerPath.Contains(kw)) score += 10;
                }

                var w = sprite.rect.width;
                var h = sprite.rect.height;
                if (minWidth > 0 && w >= minWidth) score += 6;
                if (minHeight > 0 && h >= minHeight) score += 6;
                // Heavily prefer 9-slice-ready sprites (with non-zero border)
                if (sprite.border != Vector4.zero) score += 8;
                // Penalty for very small (icon-like) sprites where we want a panel
                if (minWidth > 0 && w < minWidth / 2f) score -= 10;

                if (score > 0) candidates.Add((path, sprite, score));
            }

            candidates.Sort((a, b) => b.score.CompareTo(a.score));

            // Log top 3 so the user can verify the heuristic worked
            Debug.Log($"[Hearthbound/Phase 14] Top candidates for '{roleLabel}':");
            for (int i = 0; i < Mathf.Min(3, candidates.Count); i++)
            {
                Debug.Log($"  #{i + 1} (score {candidates[i].score}): {candidates[i].path} " +
                          $"({candidates[i].sprite.rect.width}×{candidates[i].sprite.rect.height})");
            }
            if (candidates.Count == 0)
                Debug.LogWarning($"[Hearthbound/Phase 14] No sprite matched for '{roleLabel}'. Prefab will use color fallback.");

            return candidates.Count > 0 ? candidates[0].sprite : null;
        }

        // ─── DialogueBox prefab ────────────────────────────────────

        private static void BuildDialogueBox(Sprite parchment, Sprite scrollBtn)
        {
            var root = new GameObject("DialogueBox", typeof(RectTransform));
            var rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.10f, 0.06f);
            rect.anchorMax = new Vector2(0.90f, 0.32f);
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;

            // Background parchment
            var bg = root.AddComponent<Image>();
            if (parchment != null)
            {
                bg.sprite = parchment;
                bg.type = parchment.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
                bg.color = ParchmentTint;
            }
            else
            {
                bg.color = FallbackBg;
            }

            // Portrait slot
            var portraitGO = new GameObject("Portrait", typeof(Image));
            portraitGO.transform.SetParent(root.transform, false);
            var portrait = portraitGO.GetComponent<Image>();
            portrait.preserveAspect = true;
            portrait.color = new Color(1, 1, 1, 0); // transparent until a portrait is set at runtime
            var portraitRT = portrait.rectTransform;
            portraitRT.anchorMin = new Vector2(0.02f, 0.10f);
            portraitRT.anchorMax = new Vector2(0.20f, 0.95f);
            portraitRT.offsetMin = Vector2.zero; portraitRT.offsetMax = Vector2.zero;

            // Speaker name
            var nameGO = new GameObject("SpeakerName", typeof(RectTransform));
            nameGO.transform.SetParent(root.transform, false);
            var speakerName = nameGO.AddComponent<TextMeshProUGUI>();
            speakerName.fontSize = 30;
            speakerName.fontStyle = FontStyles.Bold;
            speakerName.color = SpeakerInk;
            speakerName.alignment = TextAlignmentOptions.BottomLeft;
            var nameRT = speakerName.rectTransform;
            nameRT.anchorMin = new Vector2(0.22f, 0.78f);
            nameRT.anchorMax = new Vector2(0.96f, 0.96f);
            nameRT.offsetMin = Vector2.zero; nameRT.offsetMax = Vector2.zero;
            UIAutoFitText.ApplyToButtonLabel(speakerName, minSize: 18, maxSize: 32);

            // Line text — Phase 29: shrunk default, auto-grow + word-wrap + ellipsis.
            var textGO = new GameObject("LineText", typeof(RectTransform));
            textGO.transform.SetParent(root.transform, false);
            var lineText = textGO.AddComponent<TextMeshProUGUI>();
            lineText.fontSize = 24;
            lineText.color = InkColor;
            lineText.alignment = TextAlignmentOptions.TopLeft;
            var textRT = lineText.rectTransform;
            textRT.anchorMin = new Vector2(0.22f, 0.10f);
            textRT.anchorMax = new Vector2(0.96f, 0.74f);
            textRT.offsetMin = Vector2.zero; textRT.offsetMax = Vector2.zero;
            UIAutoFitText.ApplyToLabel(lineText, minSize: 16, maxSize: 28);

            // ChoicesContainer — Phase 31: VerticalLayoutGroup must control
            // child width (childControlWidth = true). Phase 29 set
            // childForceExpandWidth alone, which redistributes leftover space
            // but does NOT resize the child's RectTransform — every tile
            // rendered at its prefab default 100×100 in a tiny column in
            // the centre of the body. childControlWidth fixes the resize;
            // childControlHeight allows long-wrapped labels to grow the tile.
            var choicesGO = new GameObject("ChoicesContainer", typeof(RectTransform));
            choicesGO.transform.SetParent(root.transform, false);
            var choicesLayout = choicesGO.AddComponent<VerticalLayoutGroup>();
            choicesLayout.spacing = 10;
            choicesLayout.childControlWidth = true;
            choicesLayout.childControlHeight = true;
            choicesLayout.childForceExpandWidth = true;
            choicesLayout.childForceExpandHeight = false;
            choicesLayout.padding = new RectOffset(20, 20, 12, 12);
            choicesLayout.childAlignment = TextAnchor.UpperCenter;
            var choicesRT = choicesGO.GetComponent<RectTransform>();
            // Overlay the line text area; ChoiceCardUI / DialogueUI hide
            // the lineText while choices are presented, so they don't fight.
            choicesRT.anchorMin = new Vector2(0.22f, 0.08f);
            choicesRT.anchorMax = new Vector2(0.96f, 0.78f);
            choicesRT.offsetMin = Vector2.zero; choicesRT.offsetMax = Vector2.zero;

            // Choice button template (hidden under root; DialogueUI Instantiates this)
            var choiceTemplate = MakeChoiceTileVisuals(scrollBtn, "ChoiceButtonTemplate");
            choiceTemplate.transform.SetParent(root.transform, false);
            choiceTemplate.SetActive(false);

            // DialogueUI component
            var dlg = root.AddComponent<DialogueUI>();
            dlg.root = root;
            dlg.portraitImage = portrait;
            dlg.speakerName = speakerName;
            dlg.lineText = lineText;
            dlg.choiceContainer = choicesGO.transform;
            dlg.choiceButtonPrefab = choiceTemplate;

            PrefabUtility.SaveAsPrefabAsset(root, DialogueBoxPath);
            Object.DestroyImmediate(root);
            Debug.Log($"[Hearthbound/Phase 14] (created) {DialogueBoxPath}");
        }

        // ─── ChoiceTile prefab ─────────────────────────────────────

        private static GameObject MakeChoiceTileVisuals(Sprite scrollBtn, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));

            // Phase 31 — pre-shape the RectTransform to full-width so a clone
            // of this prefab does not flash at 100×100 before the parent
            // VerticalLayoutGroup recomputes width on the next layout pass.
            var goRT = go.GetComponent<RectTransform>();
            goRT.anchorMin = new Vector2(0f, 0.5f);
            goRT.anchorMax = new Vector2(1f, 0.5f);
            goRT.pivot = new Vector2(0.5f, 0.5f);
            goRT.sizeDelta = new Vector2(0f, 64f);

            var img = go.AddComponent<Image>();
            if (scrollBtn != null)
            {
                img.sprite = scrollBtn;
                img.type = scrollBtn.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
                img.color = new Color(1f, 0.96f, 0.88f, 1f); // light parchment
            }
            else
            {
                img.color = new Color(0.95f, 0.86f, 0.66f, 0.98f);
            }
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            // Subtle highlight tint
            var cb = btn.colors;
            cb.highlightedColor = new Color(1f, 0.9f, 0.6f, 1f);
            cb.pressedColor    = new Color(0.85f, 0.72f, 0.45f, 1f);
            cb.selectedColor   = new Color(1f, 0.92f, 0.68f, 1f);
            btn.colors = cb;

            // Phase 31 — minHeight + flexibleWidth so VLG-controlled tiles
            // are tap-friendly on mobile (≥56 px) and properly share extra
            // horizontal slack across the row of tiles.
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = 56;
            le.preferredHeight = 64;
            le.flexibleWidth = 1f;

            var labelGO = new GameObject("Label", typeof(RectTransform));
            labelGO.transform.SetParent(go.transform, false);
            var label = labelGO.AddComponent<TextMeshProUGUI>();
            label.text = "Choice";
            label.fontSize = 22;
            label.color = SpeakerInk;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.enableWordWrapping = true;
            var labelRT = label.rectTransform;
            labelRT.anchorMin = Vector2.zero; labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = new Vector2(20, 6); labelRT.offsetMax = new Vector2(-20, -6);
            // Phase 29 — wrap + auto-size so long choice labels stay readable.
            UIAutoFitText.ApplyToLabel(label, minSize: 14, maxSize: 24);

            return go;
        }

        private static void BuildChoiceTile(Sprite scrollBtn)
        {
            var tile = MakeChoiceTileVisuals(scrollBtn, "ChoiceTile");
            PrefabUtility.SaveAsPrefabAsset(tile, ChoiceTilePath);
            Object.DestroyImmediate(tile);
            Debug.Log($"[Hearthbound/Phase 14] (created) {ChoiceTilePath}");
        }

        // ─── EveningLedger prefab ──────────────────────────────────

        private static void BuildEveningLedger(Sprite book)
        {
            var root = new GameObject("EveningLedger", typeof(RectTransform));
            var rect = root.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;

            // Dim background overlay
            var overlay = root.AddComponent<Image>();
            overlay.color = new Color(0.03f, 0.02f, 0.01f, 0.85f);

            // Book panel
            var panelGO = new GameObject("BookPanel", typeof(RectTransform));
            panelGO.transform.SetParent(root.transform, false);
            var panelImg = panelGO.AddComponent<Image>();
            if (book != null)
            {
                panelImg.sprite = book;
                panelImg.type = book.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
                panelImg.color = ParchmentTint;
            }
            else
            {
                panelImg.color = FallbackBg;
            }
            var panelRT = panelImg.rectTransform;
            panelRT.anchorMin = new Vector2(0.15f, 0.10f);
            panelRT.anchorMax = new Vector2(0.85f, 0.92f);
            panelRT.offsetMin = Vector2.zero; panelRT.offsetMax = Vector2.zero;

            // Day label (top center)
            var dayGO = new GameObject("DayLabel", typeof(RectTransform));
            dayGO.transform.SetParent(panelGO.transform, false);
            var dayLabel = dayGO.AddComponent<TextMeshProUGUI>();
            dayLabel.text = "Day 1";
            dayLabel.fontSize = 56;
            dayLabel.fontStyle = FontStyles.Bold;
            dayLabel.alignment = TextAlignmentOptions.Center;
            dayLabel.color = SpeakerInk;
            var dayRT = dayLabel.rectTransform;
            dayRT.anchorMin = new Vector2(0.10f, 0.85f);
            dayRT.anchorMax = new Vector2(0.90f, 0.96f);
            dayRT.offsetMin = Vector2.zero; dayRT.offsetMax = Vector2.zero;
            UIAutoFitText.ApplyToButtonLabel(dayLabel, minSize: 32, maxSize: 60);

            // Left page: summary prose — Phase 29 wrap + auto-size
            var proseGO = new GameObject("SummaryProse", typeof(RectTransform));
            proseGO.transform.SetParent(panelGO.transform, false);
            var prose = proseGO.AddComponent<TextMeshProUGUI>();
            prose.fontSize = 22;
            prose.color = InkColor;
            prose.alignment = TextAlignmentOptions.TopLeft;
            var proseRT = prose.rectTransform;
            proseRT.anchorMin = new Vector2(0.06f, 0.30f);
            proseRT.anchorMax = new Vector2(0.48f, 0.80f);
            proseRT.offsetMin = Vector2.zero; proseRT.offsetMax = Vector2.zero;
            UIAutoFitText.ApplyToLabel(prose, minSize: 14, maxSize: 24);

            // Right page: held memories
            var memLblGO = new GameObject("HeldMemoriesTitle", typeof(RectTransform));
            memLblGO.transform.SetParent(panelGO.transform, false);
            var memTitle = memLblGO.AddComponent<TextMeshProUGUI>();
            memTitle.text = "On the shelves:";
            memTitle.fontSize = 22;
            memTitle.fontStyle = FontStyles.Italic;
            memTitle.color = SpeakerInk;
            memTitle.alignment = TextAlignmentOptions.TopLeft;
            var memTitleRT = memTitle.rectTransform;
            memTitleRT.anchorMin = new Vector2(0.52f, 0.74f);
            memTitleRT.anchorMax = new Vector2(0.94f, 0.80f);
            memTitleRT.offsetMin = Vector2.zero; memTitleRT.offsetMax = Vector2.zero;

            var memListGO = new GameObject("HeldMemoriesList", typeof(RectTransform));
            memListGO.transform.SetParent(panelGO.transform, false);
            var memList = memListGO.AddComponent<TextMeshProUGUI>();
            memList.fontSize = 20;
            memList.color = InkColor;
            memList.alignment = TextAlignmentOptions.TopLeft;
            var memListRT = memList.rectTransform;
            memListRT.anchorMin = new Vector2(0.52f, 0.40f);
            memListRT.anchorMax = new Vector2(0.94f, 0.72f);
            memListRT.offsetMin = Vector2.zero; memListRT.offsetMax = Vector2.zero;
            UIAutoFitText.ApplyToLabel(memList, minSize: 14, maxSize: 22);

            // Coin label
            var coinGO = new GameObject("CoinLabel", typeof(RectTransform));
            coinGO.transform.SetParent(panelGO.transform, false);
            var coinLabel = coinGO.AddComponent<TextMeshProUGUI>();
            coinLabel.fontSize = 22;
            coinLabel.color = new Color(0.62f, 0.42f, 0.10f);
            coinLabel.alignment = TextAlignmentOptions.MidlineRight;
            var coinRT = coinLabel.rectTransform;
            coinRT.anchorMin = new Vector2(0.52f, 0.28f);
            coinRT.anchorMax = new Vector2(0.94f, 0.36f);
            coinRT.offsetMin = Vector2.zero; coinRT.offsetMax = Vector2.zero;
            UIAutoFitText.ApplyToButtonLabel(coinLabel, minSize: 14, maxSize: 24);

            // Save slot buttons (3 slots + autosave label) — simple parchment buttons
            var slot1 = MakeLedgerButton(panelGO.transform, "Btn_SaveSlot1", "Save · Slot 1",
                new Vector2(0.06f, 0.18f), new Vector2(0.28f, 0.26f));
            var slot2 = MakeLedgerButton(panelGO.transform, "Btn_SaveSlot2", "Save · Slot 2",
                new Vector2(0.30f, 0.18f), new Vector2(0.52f, 0.26f));
            var slot3 = MakeLedgerButton(panelGO.transform, "Btn_SaveSlot3", "Save · Slot 3",
                new Vector2(0.06f, 0.10f), new Vector2(0.28f, 0.16f));
            var autosaveBtn = MakeLedgerButton(panelGO.transform, "Btn_Autosave", "Autosave",
                new Vector2(0.30f, 0.10f), new Vector2(0.52f, 0.16f));

            var confirmBtn = MakeLedgerButton(panelGO.transform, "Btn_EndOfDay", "Sleep — End Day",
                new Vector2(0.58f, 0.10f), new Vector2(0.94f, 0.22f));

            // Wire the EveningLedgerUI component
            var ledger = root.AddComponent<EveningLedgerUI>();
            ledger.root = root;
            ledger.dayLabel = dayLabel;
            ledger.coinLabel = coinLabel;
            ledger.summaryProse = prose;
            ledger.heldMemoriesList = memList;
            ledger.saveSlot1 = slot1.button;
            ledger.saveSlot2 = slot2.button;
            ledger.saveSlot3 = slot3.button;
            ledger.autosaveButton = autosaveBtn.button;
            ledger.saveSlot1Label = slot1.label;
            ledger.saveSlot2Label = slot2.label;
            ledger.saveSlot3Label = slot3.label;
            ledger.confirmEndOfDayButton = confirmBtn.button;

            PrefabUtility.SaveAsPrefabAsset(root, EveningLedgerPath);
            Object.DestroyImmediate(root);
            Debug.Log($"[Hearthbound/Phase 14] (created) {EveningLedgerPath}");
        }

        private struct LedgerButton { public Button button; public TextMeshProUGUI label; }

        private static LedgerButton MakeLedgerButton(Transform parent, string name, string text,
                                                     Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.88f, 0.78f, 0.58f, 0.92f);
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            var rt = img.rectTransform;
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            var labelGO = new GameObject("Label", typeof(RectTransform));
            labelGO.transform.SetParent(go.transform, false);
            var label = labelGO.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = 18;
            label.color = SpeakerInk;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            var labelRT = label.rectTransform;
            labelRT.anchorMin = Vector2.zero; labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = new Vector2(8, 4); labelRT.offsetMax = new Vector2(-8, -4);
            UIAutoFitText.ApplyToButtonLabel(label, minSize: 12, maxSize: 20);

            return new LedgerButton { button = btn, label = label };
        }

        // ─── TooltipFrame prefab ───────────────────────────────────

        private static void BuildTooltipFrame(Sprite frame)
        {
            var root = new GameObject("TooltipFrame", typeof(RectTransform));
            var rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(320, 120);

            var img = root.AddComponent<Image>();
            if (frame != null)
            {
                img.sprite = frame;
                img.type = frame.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
                img.color = ParchmentTint;
            }
            else
            {
                img.color = FallbackBg;
            }

            var labelGO = new GameObject("TooltipText", typeof(RectTransform));
            labelGO.transform.SetParent(root.transform, false);
            var label = labelGO.AddComponent<TextMeshProUGUI>();
            label.fontSize = 18;
            label.color = InkColor;
            label.alignment = TextAlignmentOptions.Center;
            label.enableWordWrapping = true;
            var labelRT = label.rectTransform;
            labelRT.anchorMin = Vector2.zero; labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = new Vector2(14, 10); labelRT.offsetMax = new Vector2(-14, -10);
            UIAutoFitText.ApplyToLabel(label, minSize: 12, maxSize: 22);

            PrefabUtility.SaveAsPrefabAsset(root, TooltipFramePath);
            Object.DestroyImmediate(root);
            Debug.Log($"[Hearthbound/Phase 14] (created) {TooltipFramePath}");
        }

        // ─── Folder helpers ────────────────────────────────────────

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            var leaf = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        // ─── Public lookups (used by HearthboundOneClickSetup) ─────

        public static GameObject TryGetDialogueBoxPrefab() =>
            AssetDatabase.LoadAssetAtPath<GameObject>(DialogueBoxPath);

        public static GameObject TryGetChoiceTilePrefab() =>
            AssetDatabase.LoadAssetAtPath<GameObject>(ChoiceTilePath);

        public static GameObject TryGetEveningLedgerPrefab() =>
            AssetDatabase.LoadAssetAtPath<GameObject>(EveningLedgerPath);

        public static GameObject TryGetTooltipFramePrefab() =>
            AssetDatabase.LoadAssetAtPath<GameObject>(TooltipFramePath);
    }
}
