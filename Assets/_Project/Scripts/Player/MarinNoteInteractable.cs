// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Player / MarinNoteInteractable
//
// A parchment note left by Marin (the previous Hollow-keeper) on the
// workbench. The player can interact with it on Day 1 to read 4 short
// passages — the first concrete encounter with the predecessor, who is
// otherwise only a voice in the Help overlay quote. Per Mission_1_2_Focus
// 03 § "Marin's note on the workbench" + Narrative Bible § Predecessor Hook.
//
// The note uses the existing DialogueUI presenter — same parchment box as
// villager lines — keeping the cozy tactile feel. Speaker is "Marin's Note"
// (italicised) and no portrait is set (the predecessor has no face yet).
//
// Phase 26 narrative hook — Marin's Note. Surfaces the predecessor's
// presence on Day 1 without a full backstory infodump. The note is signed
// "— M." matching the Help overlay quote.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HearthboundHollow.Core;
using HearthboundHollow.UI;

namespace HearthboundHollow.Player
{
    public class MarinNoteInteractable : Interactable
    {
        [Header("Presentation")]
        [Tooltip("The DialogueUI used to render note passages — same Bamao " +
                 "parchment box villager dialogue uses, for tactile consistency.")]
        public DialogueUI dialogueUI;

        [Tooltip("Speaker label shown above each passage. Defaults to \"Marin's Note\".")]
        public string speakerLabel = "Marin's Note";

        [Header("Pages — sequenced on each press of E")]
        [TextArea(3, 8)]
        public string[] passages = new[]
        {
            "<i>If you're reading this, the door let you in.</i>\n\n" +
            "The Hollow keeps a person more than the person keeps the Hollow.\n" +
            "There is no wrong way to keep a memory — only the gentle way, and the others.",

            "<i>A few things, before the kettle whistles:</i>\n\n" +
            "• The orbs on the shelves are not yours. They are entrusted.\n" +
            "• Polish in slow circles. Cover all four faces.\n" +
            "• If the seam pulls red, you've gone too deep. Stop.",

            "<i>The villagers will come.</i>\n\n" +
            "Doris first, with bread on her breath and a question she does not yet know how to ask.\n" +
            "Be kind. Be slow. Listen for the second sentence — it is always the true one.",

            "<i>And finally:</i>\n\n" +
            "You may find a door that does not want to be a door.\n" +
            "That is for later. For today — tea. The shop opens at four.\n\n" +
            "                                                                                — M."
        };

        [Header("State")]
        [Tooltip("If true, the note becomes inactive after the player reads all passages.")]
        public bool retireAfterReading = true;

        [Tooltip("VillageState flag set to true the first time the player finishes reading. " +
                 "Used by other systems to know the predecessor has been introduced.")]
        public string villageStateFlag = "marinNoteRead";

        // ---- runtime ----
        private int _passageIndex = 0;
        private bool _reading = false;

        public override string GetDynamicPromptText()
        {
            if (_reading) return "Continue … (E)";
            if (_passageIndex == 0) return "Read the note (E)";
            return "Re-read the note (E)";
        }

        public override void Activate(GameObject player)
        {
            if (!IsInteractable) return;
            if (dialogueUI == null)
            {
                Hh.Warn(LogCategory.UI, "MarinNoteInteractable: dialogueUI is not wired — falling back to debug log.");
                Debug.Log($"[Marin's Note] {GetCurrentPassage()}");
                AdvancePassage();
                return;
            }

            if (!_reading)
            {
                _reading = true;
                _passageIndex = 0;
            }

            // Show the current passage via the dialogue presenter.
            dialogueUI.PresentLine(speakerLabel, GetCurrentPassage(), portrait: null);
            AdvancePassage();
        }

        private string GetCurrentPassage()
        {
            if (passages == null || passages.Length == 0) return "…";
            int idx = Mathf.Clamp(_passageIndex, 0, passages.Length - 1);
            return passages[idx];
        }

        private void AdvancePassage()
        {
            _passageIndex++;
            if (_passageIndex >= passages.Length)
            {
                // Finished. Mark state, optionally retire.
                var vs = ServiceLocator.Get<VillageState>();
                if (vs != null && !string.IsNullOrEmpty(villageStateFlag))
                    vs.SetFlag(villageStateFlag, true);

                _reading = false;
                _passageIndex = 0;

                if (retireAfterReading)
                {
                    SetInteractable(false);
                    // Soft fade: schedule a Hide() after a short delay so the
                    // player can finish reading the last passage.
                    StartCoroutine(HideAfterDelay(2.5f));
                }
                Hh.Log(LogCategory.UI, "Marin's Note: player finished reading.");
            }
        }

        private IEnumerator HideAfterDelay(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (dialogueUI != null) dialogueUI.Hide();
        }
    }
}
