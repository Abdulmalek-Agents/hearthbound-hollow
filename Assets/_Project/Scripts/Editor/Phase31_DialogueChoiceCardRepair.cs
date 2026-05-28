// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Editor / Phase31_DialogueChoiceCardRepair
//
// PHASE 31 — Dialogue Choice Card Repair Capstone.
//
// User report after the Phase 30 playtest (screenshot):
//   "the game stuck during the dialogue and as shown in screenshots the
//    cards not appear well so please fix this issue and enhance the
//    gameplay to make the game playable"
//
// ROOT CAUSE (see DialogueChoiceLayoutHealer.cs for the long form)
//
//   Phase 14 saved UI_DialogueBox_Bamao.prefab with a VerticalLayoutGroup
//   on the ChoicesContainer that had:
//       childForceExpandWidth = 1
//       childControlWidth     = 0   ← bug
//   With childControlWidth disabled, the layout group never resized the
//   tile children. Combined with the choice-tile prefab's saved
//   sizeDelta = (100, 100), every instantiated tile rendered as a tiny
//   ~100 px square in the centre of the dialogue body, with labels
//   wrapping one-word-per-line. The player had to find and click those
//   tiny squares to advance the dialogue — easy to miss → game felt
//   "stuck during the dialogue".
//
// FIX — this capstone walks every saved UI prefab + every gameplay scene
// and surgically repairs the VLG + LayoutElement settings IN PLACE so
// the user does not need to re-run Phase 14 (which would regenerate the
// prefabs from scratch and could lose any inspector tweaks they made).
//
// IDEMPOTENT — safe to re-run any number of times.
//
// USE: Menu → Hearthbound → 🧰 Phase 31 — Repair Dialogue Choice Cards

using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HearthboundHollow.UI;

namespace HearthboundHollow.EditorTools
{
    public static class Phase31_DialogueChoiceCardRepair
    {
        private const string DialogueBoxPrefab = "Assets/_Project/Prefabs/UI/UI_DialogueBox_Bamao.prefab";
        private const string ChoiceTilePrefab  = "Assets/_Project/Prefabs/UI/UI_ChoiceTile_Bamao.prefab";

        private static readonly string[] GameplayScenes = new[]
        {
            "Assets/_Project/Scenes/02_Mission01_Lane.unity",
            "Assets/_Project/Scenes/03_Mission01_Hollow.unity",
            "Assets/_Project/Scenes/04_Mission02_Garden.unity",
            "Assets/_Project/Scenes/05_Mission02_Cottage.unity",
        };

        // ─── Menu ────────────────────────────────────────────────

        [MenuItem("Hearthbound/⚙️ Advanced/🧰 Phase 31 — Repair Dialogue Choice Cards", priority = 3)]
        public static void Build()
        {
            int prefabsTouched = 0;
            int scenesTouched = 0;
            var notes = new List<string>();

            EditorUtility.DisplayProgressBar("Hearthbound · Phase 31", "Repairing UI_DialogueBox_Bamao.prefab…", 0.10f);
            try
            {
                if (RepairDialogueBoxPrefab(notes)) prefabsTouched++;

                EditorUtility.DisplayProgressBar("Hearthbound · Phase 31", "Repairing UI_ChoiceTile_Bamao.prefab…", 0.30f);
                if (RepairChoiceTilePrefab(notes)) prefabsTouched++;

                for (int i = 0; i < GameplayScenes.Length; i++)
                {
                    EditorUtility.DisplayProgressBar(
                        "Hearthbound · Phase 31",
                        $"Repairing scene {Path.GetFileName(GameplayScenes[i])}…",
                        0.40f + 0.50f * (i / (float)GameplayScenes.Length));
                    if (RepairScene(GameplayScenes[i], notes)) scenesTouched++;
                }

                EditorUtility.DisplayProgressBar("Hearthbound · Phase 31", "Saving…", 0.95f);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            string summary =
                $"  ✓ Prefabs repaired: {prefabsTouched}/2\n" +
                $"  ✓ Scenes repaired:  {scenesTouched}/{GameplayScenes.Length}\n" +
                (notes.Count > 0
                    ? "\nDetails:\n  • " + string.Join("\n  • ", notes)
                    : "");

            Debug.Log("[Hearthbound/Phase 31] " + summary.Replace('\n', ' '));

            EditorUtility.DisplayDialog(
                "Hearthbound — Phase 31 complete",
                "Dialogue Choice Card layout repair complete.\n\n" +
                summary +
                "\n\nPress Play in 00_Bootstrap → walk up to Doris. The choice tiles now stretch to full width, " +
                "wrap long labels gracefully, and the narration line is hidden while you're choosing.",
                "OK");
        }

        // ─── DialogueBox prefab ────────────────────────────────────

        private static bool RepairDialogueBoxPrefab(List<string> notes)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(DialogueBoxPrefab);
            if (go == null)
            {
                notes.Add($"UI_DialogueBox_Bamao.prefab not found at {DialogueBoxPrefab}");
                return false;
            }

            var instance = PrefabUtility.LoadPrefabContents(DialogueBoxPrefab);
            try
            {
                var choicesContainer = FindChildRecursive(instance.transform, "ChoicesContainer");
                if (choicesContainer == null)
                {
                    notes.Add("UI_DialogueBox: ChoicesContainer child not found — leaving prefab untouched.");
                    return false;
                }

                bool changed = RepairChoicesContainer(choicesContainer);

                // Heal the embedded ChoiceButtonTemplate too — it's the
                // template DialogueUI clones at runtime.
                var template = FindChildRecursive(instance.transform, "ChoiceButtonTemplate");
                if (template != null)
                {
                    if (RepairChoiceTile(template.gameObject)) changed = true;
                }
                else
                {
                    notes.Add("UI_DialogueBox: ChoiceButtonTemplate not found — Phase 14 default may be missing.");
                }

                // Phase 31.1 — make sure the AdvancePrompt label exists on
                // the prefab so the "Click or [Space] ▸" hint is baked into
                // saved assets (the runtime self-heal in DialogueUI.Awake
                // also creates it, but baking it avoids a frame-0 flash).
                if (EnsureAdvancePromptOnPrefab(instance)) changed = true;

                if (changed)
                {
                    PrefabUtility.SaveAsPrefabAsset(instance, DialogueBoxPrefab);
                    notes.Add("UI_DialogueBox_Bamao.prefab — ChoicesContainer + ChoiceButtonTemplate repaired.");
                }
                else
                {
                    notes.Add("UI_DialogueBox_Bamao.prefab — already healthy, no change.");
                }
                return changed;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(instance);
            }
        }

        // ─── ChoiceTile prefab ─────────────────────────────────────

        private static bool RepairChoiceTilePrefab(List<string> notes)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(ChoiceTilePrefab);
            if (go == null)
            {
                notes.Add($"UI_ChoiceTile_Bamao.prefab not found at {ChoiceTilePrefab}");
                return false;
            }

            var instance = PrefabUtility.LoadPrefabContents(ChoiceTilePrefab);
            try
            {
                bool changed = RepairChoiceTile(instance);
                if (changed)
                {
                    PrefabUtility.SaveAsPrefabAsset(instance, ChoiceTilePrefab);
                    notes.Add("UI_ChoiceTile_Bamao.prefab — LayoutElement + label wrap repaired.");
                }
                else
                {
                    notes.Add("UI_ChoiceTile_Bamao.prefab — already healthy, no change.");
                }
                return changed;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(instance);
            }
        }

        // ─── Scene repair ──────────────────────────────────────────

        private static bool RepairScene(string scenePath, List<string> notes)
        {
            if (!File.Exists(scenePath))
            {
                notes.Add($"{Path.GetFileName(scenePath)}: not found, skipped.");
                return false;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            bool changed = false;

            // Repair any DialogueUI instance in the scene — usually one per
            // gameplay scene, attached to the DialogueBox prefab instance.
            var dialogueUis = Object.FindObjectsByType<DialogueUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var ui in dialogueUis)
            {
                if (ui == null) continue;
                if (ui.choiceContainer != null && RepairChoicesContainer(ui.choiceContainer))
                {
                    EditorUtility.SetDirty(ui.choiceContainer.gameObject);
                    changed = true;
                }
                if (ui.choiceButtonPrefab != null && RepairChoiceTile(ui.choiceButtonPrefab))
                {
                    EditorUtility.SetDirty(ui.choiceButtonPrefab);
                    changed = true;
                }
            }

            // Repair ChoiceCardUI (moral choice card) too — same root cause.
            var choiceCards = Object.FindObjectsByType<ChoiceCardUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var card in choiceCards)
            {
                if (card == null) continue;
                if (card.choiceContainer != null && RepairChoicesContainer(card.choiceContainer))
                {
                    EditorUtility.SetDirty(card.choiceContainer.gameObject);
                    changed = true;
                }
                if (card.choiceTilePrefab != null && RepairChoiceTile(card.choiceTilePrefab))
                {
                    EditorUtility.SetDirty(card.choiceTilePrefab);
                    changed = true;
                }
            }

            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                notes.Add($"{Path.GetFileName(scenePath)} — dialogue choice layout repaired.");
            }
            else
            {
                notes.Add($"{Path.GetFileName(scenePath)} — already healthy, no change.");
            }
            return changed;
        }

        // ─── Shared repair primitives ──────────────────────────────

        private static bool RepairChoicesContainer(Transform container)
        {
            if (container == null) return false;
            bool changed = false;

            var vlg = container.GetComponent<VerticalLayoutGroup>();
            if (vlg == null)
            {
                vlg = container.gameObject.AddComponent<VerticalLayoutGroup>();
                changed = true;
            }
            if (!vlg.childControlWidth) { vlg.childControlWidth = true; changed = true; }
            if (!vlg.childControlHeight) { vlg.childControlHeight = true; changed = true; }
            if (!vlg.childForceExpandWidth) { vlg.childForceExpandWidth = true; changed = true; }
            if (vlg.childForceExpandHeight) { vlg.childForceExpandHeight = false; changed = true; }
            if (vlg.spacing < 8f) { vlg.spacing = 10f; changed = true; }
            if (vlg.padding == null ||
                (vlg.padding.left | vlg.padding.right | vlg.padding.top | vlg.padding.bottom) == 0)
            {
                vlg.padding = new RectOffset(16, 16, 10, 10);
                changed = true;
            }
            if (vlg.childAlignment != TextAnchor.UpperCenter)
            {
                vlg.childAlignment = TextAnchor.UpperCenter;
                changed = true;
            }

            if (changed) EditorUtility.SetDirty(vlg);
            return changed;
        }

        private static bool RepairChoiceTile(GameObject tile)
        {
            if (tile == null) return false;
            bool changed = false;

            // Re-anchor the tile's own RectTransform so a freshly-cloned
            // copy doesn't flash at 100×100 before the VLG repositions it.
            var rt = tile.transform as RectTransform;
            if (rt != null)
            {
                if (Mathf.Abs(rt.anchorMin.x - 0f) > 0.001f) { rt.anchorMin = new Vector2(0f, rt.anchorMin.y); changed = true; }
                if (Mathf.Abs(rt.anchorMax.x - 1f) > 0.001f) { rt.anchorMax = new Vector2(1f, rt.anchorMax.y); changed = true; }
                if (Mathf.Abs(rt.sizeDelta.x) > 0.5f) { var sd = rt.sizeDelta; sd.x = 0f; rt.sizeDelta = sd; changed = true; }
            }

            var le = tile.GetComponent<LayoutElement>();
            if (le == null) { le = tile.AddComponent<LayoutElement>(); changed = true; }
            if (le.minHeight < 56f) { le.minHeight = 56f; changed = true; }
            if (le.preferredHeight < 64f) { le.preferredHeight = 64f; changed = true; }
            if (le.flexibleWidth < 1f) { le.flexibleWidth = 1f; changed = true; }
            if (changed) EditorUtility.SetDirty(le);

            // Heal labels — word-wrap, auto-size, ellipsis fallback.
            var labels = tile.GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true);
            foreach (var label in labels)
            {
                if (label == null) continue;
                bool labelChanged = false;
                if (!label.enableWordWrapping) { label.enableWordWrapping = true; labelChanged = true; }
                if (!label.enableAutoSizing) { label.enableAutoSizing = true; labelChanged = true; }
                if (label.fontSizeMin < 14f) { label.fontSizeMin = 14f; labelChanged = true; }
                if (label.fontSizeMax < 22f) { label.fontSizeMax = 22f; labelChanged = true; }
                if (label.overflowMode != TextOverflowModes.Ellipsis)
                {
                    label.overflowMode = TextOverflowModes.Ellipsis;
                    labelChanged = true;
                }
                var labelRT = label.rectTransform;
                if (labelRT != null)
                {
                    if (Mathf.Abs(labelRT.anchorMin.x) > 0.001f || Mathf.Abs(labelRT.anchorMin.y) > 0.001f)
                    { labelRT.anchorMin = Vector2.zero; labelChanged = true; }
                    if (Mathf.Abs(labelRT.anchorMax.x - 1f) > 0.001f || Mathf.Abs(labelRT.anchorMax.y - 1f) > 0.001f)
                    { labelRT.anchorMax = Vector2.one; labelChanged = true; }
                }
                if (labelChanged) { EditorUtility.SetDirty(label); changed = true; }
            }

            // Defensive — make sure the tile button is clickable.
            var btn = tile.GetComponent<Button>();
            if (btn != null)
            {
                if (!btn.interactable) { btn.interactable = true; changed = true; }
                if (btn.targetGraphic != null && !btn.targetGraphic.raycastTarget)
                {
                    btn.targetGraphic.raycastTarget = true;
                    EditorUtility.SetDirty(btn.targetGraphic);
                    changed = true;
                }
            }
            var img = tile.GetComponent<Image>();
            if (img != null && !img.raycastTarget)
            {
                img.raycastTarget = true;
                EditorUtility.SetDirty(img);
                changed = true;
            }

            if (changed) EditorUtility.SetDirty(tile);
            return changed;
        }

        // ─── AdvancePrompt repair (Phase 31.1) ───────────────────────

        private static bool EnsureAdvancePromptOnPrefab(GameObject root)
        {
            if (root == null) return false;
            var existing = FindChildRecursive(root.transform, "AdvancePrompt");
            if (existing != null) return false;

            var dlg = root.GetComponentInChildren<DialogueUI>(includeInactive: true);
            var parent = dlg != null && dlg.root != null ? dlg.root.transform : root.transform;

            var go = new GameObject("AdvancePrompt", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0.70f, 0.02f);
            rt.anchorMax = new Vector2(0.98f, 0.16f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = "Click or [Space] >"; // Phase 32.11 — ASCII for LiberationSans SDF
            tmp.fontSize = 18;
            tmp.fontStyle = FontStyles.Italic;
            tmp.alignment = TextAlignmentOptions.MidlineRight;
            tmp.color = new Color(0.42f, 0.24f, 0.10f, 0.85f);
            tmp.raycastTarget = false;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 12;
            tmp.fontSizeMax = 20;
            tmp.overflowMode = TextOverflowModes.Ellipsis;

            if (dlg != null) dlg.advancePrompt = tmp;
            EditorUtility.SetDirty(go);
            if (dlg != null) EditorUtility.SetDirty(dlg);
            return true;
        }

        // ─── Transform helper ───────────────────────────────────────

        private static Transform FindChildRecursive(Transform parent, string childName)
        {
            if (parent == null) return null;
            if (parent.name == childName) return parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                var found = FindChildRecursive(parent.GetChild(i), childName);
                if (found != null) return found;
            }
            return null;
        }
    }
}
