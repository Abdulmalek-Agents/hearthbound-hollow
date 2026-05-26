// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / DialogueChoiceLayoutHealer
//
// PHASE 31 — "Choice cards never render tiny / cramped / off-screen" helper.
//
// User report after the Phase 30 playtest (screenshot attached):
//   "the game stuck during the dialogue ... the cards not appear well"
//
// ROOT CAUSE
//
//   Phase 14 created the DialogueBox prefab with a VerticalLayoutGroup on the
//   ChoicesContainer that had `childForceExpandWidth = true` BUT
//   `childControlWidth = false`. With childControlWidth disabled, the
//   layout group does NOT change each child's RectTransform width — it
//   only redistributes leftover horizontal space. Because the choice tile
//   prefab is saved with `sizeDelta = (100, 100)`, every instantiated tile
//   rendered as a ~100 px square in the centre of the dialogue body —
//   tiny, hard to click, and the labels broke into one-word-per-line shards
//   ("I'm | here | to | help").
//
//   Compounded by:
//     • The tile's LayoutElement only set `preferredHeight = 62` — no
//       preferredWidth, no flexibleWidth.
//     • The DialogueBox `lineText` was not hidden while choices were on
//       screen, so the previous narration overlapped the choice tiles.
//     • ChoicesContainer used `childAlignment = MiddleCenter`, which when
//       combined with the narrow tiles dropped them into one tight column.
//
// FIX — this helper applies the correct layout settings at RUNTIME on the
// container and on every instantiated tile, so existing prefabs / scenes
// in shipped saves do not require a re-build of the prefab to look right.
// The Phase 14 builder is also patched so fresh builds bake the same
// settings into the saved prefab.
//
// Safe to call repeatedly (idempotent). No allocations on the hot path.

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HearthboundHollow.UI
{
    public static class DialogueChoiceLayoutHealer
    {
        /// <summary>
        /// Configure a choice-tile container's VerticalLayoutGroup so that
        /// instantiated tiles stretch to the full container width with sane
        /// padding + spacing. Idempotent.
        /// </summary>
        public static void HealContainer(Transform container)
        {
            if (container == null) return;

            var vlg = container.GetComponent<VerticalLayoutGroup>();
            if (vlg == null) vlg = container.gameObject.AddComponent<VerticalLayoutGroup>();

            // The critical four flags. childControlWidth = true is the bug
            // fix — without it, childForceExpandWidth alone does not resize
            // children. childControlHeight = true lets variable-line-count
            // labels grow the tile vertically as needed.
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = Mathf.Max(8, vlg.spacing);
            if (vlg.padding == null || (vlg.padding.left | vlg.padding.right | vlg.padding.top | vlg.padding.bottom) == 0)
                vlg.padding = new RectOffset(16, 16, 10, 10);

            // Stack from the top so the first option lands where the reader's
            // eye is already focused after the last spoken line.
            vlg.childAlignment = TextAnchor.UpperCenter;
        }

        /// <summary>
        /// Make sure a freshly-instantiated choice tile renders full-width,
        /// has a readable minimum height, and wraps long labels gracefully.
        /// Resets any stale sizeDelta inherited from the saved prefab so the
        /// VerticalLayoutGroup's width control is not fought by the prefab's
        /// own 100×100 default.
        /// </summary>
        public static void HealTile(GameObject tile)
        {
            if (tile == null) return;

            var rt = tile.transform as RectTransform;
            if (rt != null)
            {
                // Defeat the prefab's saved (100,100) size — the VLG will
                // recompute width every frame, but a non-zero sizeDelta.x
                // can briefly flash during the first layout pass.
                rt.anchorMin = new Vector2(0f, rt.anchorMin.y);
                rt.anchorMax = new Vector2(1f, rt.anchorMax.y);
                var sd = rt.sizeDelta;
                sd.x = 0f;
                rt.sizeDelta = sd;
            }

            var le = tile.GetComponent<LayoutElement>();
            if (le == null) le = tile.AddComponent<LayoutElement>();
            // 56 px keeps the tile finger-tap-friendly on mobile; 64 px is
            // the cozy default. flexibleWidth = 1 makes the VLG share extra
            // horizontal slack across the tiles instead of bunching them.
            if (le.minHeight < 56f) le.minHeight = 56f;
            if (le.preferredHeight < 64f) le.preferredHeight = 64f;
            if (le.flexibleWidth < 1f) le.flexibleWidth = 1f;

            // Heal every TMP label under the tile — force word-wrap and
            // sane auto-size bounds so a single long line such as
            // "I'm not sure I'm ready." renders on one or two lines rather
            // than splitting one word per line in a 100-px-wide column.
            var labels = tile.GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true);
            foreach (var label in labels)
            {
                if (label == null) continue;
                label.enableWordWrapping = true;
                label.alignment = TextAlignmentOptions.Center;
                label.enableAutoSizing = true;
                if (label.fontSizeMin < 14f) label.fontSizeMin = 14f;
                if (label.fontSizeMax < 22f) label.fontSizeMax = 22f;
                label.overflowMode = TextOverflowModes.Ellipsis;

                // Make sure the label rect spans the tile interior with a
                // small inset for the parchment border decoration.
                var labelRT = label.rectTransform;
                if (labelRT != null)
                {
                    labelRT.anchorMin = Vector2.zero;
                    labelRT.anchorMax = Vector2.one;
                    labelRT.offsetMin = new Vector2(16, 8);
                    labelRT.offsetMax = new Vector2(-16, -8);
                }
            }

            // Defensive: ensure the tile's Button is interactable and its
            // Image (target graphic) is raycast-enabled. The prefab has
            // both set correctly, but a runtime tweak in another script
            // could flip them off — the tile would be invisible to clicks.
            var btn = tile.GetComponent<Button>();
            if (btn != null)
            {
                btn.interactable = true;
                if (btn.targetGraphic != null) btn.targetGraphic.raycastTarget = true;
            }
            var img = tile.GetComponent<Image>();
            if (img != null) img.raycastTarget = true;
        }
    }
}
