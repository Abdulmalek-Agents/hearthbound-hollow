// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / Mission01Director
//
// The runtime orchestrator for the Mission 1 MVP playable slice.
//
// ── Playtest pass fix (commit 3/6) ──────────────────────────────
// QA simulated-playthrough audit found this director was running with
// HARD-CODED Phase 12 placeholder strings ("Oh — you must be the new
// Keeper...") that completely diverged from the canonical Doris voice
// signature in Docs/Depth_Bible/Mission_1_2_Focus/01_DORIS_THE_BAKER.md
// and the shipped Doris_M1.yarn. The newly-expanded ~180-line Vellis-tier
// Yarn dialogue was sitting unused.
//
// FIX: Replaced every hard-coded line with the canonical text from
// Doris_M1.yarn. Added the missing 3-option opener ("Who was the old
// one?" branch — the only way to plant the Marin reveal thread),
// the iconic "Hold it like you'd hold a hot bun" offer line, the
// refusal path (Mission 1 Guide § 9.4), and the 5-line "First Loaves"
// aside about age 24.
//
// Pricing is in COPPERS (4/5/2) not silver (5/10/3) per Doris_M1.yarn
// price-negotiation node.
//
// Compilation note: this fallback path runs even when Yarn Spinner is
// NOT installed (Phase 12 MVP requirement). When Yarn IS installed,
// future work will route through DialogueRunner.StartDialogue() against
// the same Doris_M1 nodes — but the text shown to the player will be
// identical either way.
//
// ── Phase 34 (2026-05-25) ───────────────────────────────────
// User report: "the game is stuck after few dialog mainly after I play
// it when it reach to the dialogue 'Doris stands back and watches'".
// Root cause: the old stage-direction line "(stands back and watches)"
// dismissed the dialogue and then the Polish mini-game ran silently
// with no on-screen instructions. Players assumed the game crashed.
//
// FIX: replaced the stage-direction line with a proper cozy dialogue
// beat ("I'll wait. Take your time, Keeper.") AND attached a runtime
// MiniGameTutorialUI via EnsureTutorialOverlay() that auto-builds an
// on-screen overlay with "Hold LMB · slow circles" instructions, a
// live progress bar, and a "Skip · Auto-Complete" button.
//
// ── Phase 32 — Voice Acting MVP (2026-05-27) ────────────────────
// `Line(...)` now accepts an optional `lineId` that DialogueUI passes
// through to `VoicePlayer.Play(lineId)` for in-sync voice playback.
// Every Doris line in this director carries its canonical id from the
// Tools/generate_voices.sh table (e.g. doris_m1_greet_01). When the
// VoiceLibrarySO has a clip for that id, the matching Piper TTS
// (or ElevenLabs / XTTS / human VO — D-058) .wav plays alongside the
// typewriter. Lines with no clip play silently — zero regression.
//
// ── Phase 32.9 — full Mission 1 voice coverage (2026-05-27) ─────
// The 3 refused-path lines (EnterRefusedPath) and the 3 clarity-
// branching afterPolishLine variants (Perfect / Acceptable / Mild)
// now carry their own lineIds. Doris's coverage in Mission 1 grows
// from 49 → 55 voiced lines. Per D-059, the canonical TTS pipeline
// is open-source Piper TTS (Tools/generate_voices.sh) with espeak-ng
// (Phase46_VoiceGenerator.cs) as the in-Editor cross-platform
// fallback — both write to the same canonical .wav paths.
//
// Flow:
//   1. Doris greets the player ("You're the new one. I thought you'd be taller.")
//   2. 3-option opener (Hello / silent / "Who was the old one?")
//   3. Bakery enter + wooden-box scene ("Mind the flour.")
//   4. "I'd like to be your first customer" preamble
//   5. The iconic orb offer ("Hold it like you'd hold a hot bun")
//   6. 3-option orb response (Take + price / Tell me more first / Refuse)
//   7. If refused → quiet alternate path (Mission 1 Guide § 9.4)
//   8. Otherwise: 3-option price negotiation (Honor 4cu / Pay 6→5cu / Underpay 2cu)
//   9. Hand-off ("The cat watched me. Judged me, I think.")
//   10. Polish mini-game (with on-screen tutorial overlay — Phase 34 fix)
//   11. Doris's after-polish line branches on $polish_quality
//   12. "Sleep tonight. Dreams come."
//   13. Evening Ledger → end of day

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HearthboundHollow.Core;
using HearthboundHollow.Memory;
using HearthboundHollow.MiniGames;
using HearthboundHollow.Player;
using HearthboundHollow.UI;

namespace HearthboundHollow.Mission
{
    public class Mission01Director : MonoBehaviour
    {
        [Header("Scene refs (set by SceneBuilder)")]
        public PlayerController playerController;
        public Transform dorisTransform;
        public Collider dorisGreetingTrigger;
        public MemoryOrbInteractable workbenchOrb;
        public Collider workbenchApproachTrigger;

        [Header("UI refs (set by SceneBuilder)")]
        public DialogueUI dialogueUI;
        public EveningLedgerUI eveningLedger;
        public ChoiceCardUI choiceCard;

        [Header("Mini-game (set by SceneBuilder)")]
        public PolishMiniGame polishGame;

        [Header("Data (set by SceneBuilder via SeedAssetGenerator outputs)")]
        public VillagerSO dorisVillager;
        public MemoryNodeSO dorisMemory;

        [Header("Routing")]
        [Tooltip("Scene to load when the player ends the day (MVP: return to MainMenu).")]
        public string sceneAfterEndOfDay = "01_MainMenu";

        [Header("Phase 47 — One More Day (optional)")]
        [Tooltip("When wired (by Phase47_OneMoreDayBuilder), the night chain " +
                 "Ledger → Dream → Goodnight Card → load is owned here. " +
                 "Unwired = the exact legacy day-end path runs (D-064).")]
        public EndOfDaySequencer endOfDaySequencer;
        public HearthboundHollow.Cutscene.MemoryDreamSequencer dreamSequencer;

        // ───── Lifecycle ──────────────────────────────────────────

        private DorisGreetingTrigger _dorisProxy;
        private WorkbenchApproachTrigger _workbenchProxy;

        private void Start()
        {
            // Hide the orb until Doris hands it over.
            if (workbenchOrb != null) workbenchOrb.gameObject.SetActive(false);

            // Install trigger proxies that call back into the director.
            if (dorisGreetingTrigger != null)
            {
                _dorisProxy = dorisGreetingTrigger.gameObject.AddComponent<DorisGreetingTrigger>();
                _dorisProxy.director = this;
            }
            if (workbenchApproachTrigger != null)
            {
                _workbenchProxy = workbenchApproachTrigger.gameObject.AddComponent<WorkbenchApproachTrigger>();
                _workbenchProxy.director = this;
                _workbenchProxy.gameObject.SetActive(false); // enabled after Doris hands over the orb
            }

            // Subscribe to polish completion.
            if (polishGame != null) polishGame.OnGameFinished += OnPolishFinished;

            // Phase 54 (D-071) — register Doris with the dialogue camera so it
            // frames her properly (the Hollow scene has no object literally named
            // "Doris", which previously collapsed dialogue to a wall-jammed wide
            // shot). Safe even before the camera director auto-spawns.
            if (dorisTransform != null)
                HearthboundHollow.Player.DialogueCameraDirector.RegisterSpeaker("Doris", dorisTransform);

            // Hide UI panels initially.
            if (dialogueUI != null) dialogueUI.Hide();
            if (eveningLedger != null)
            {
                eveningLedger.OnEndOfDayConfirmed += OnEndOfDayConfirmed;
            }

            Hh.Log(LogCategory.Mission, "Mission01Director ready. Walk up to Doris to begin.");
        }

        private void OnDestroy()
        {
            if (polishGame != null) polishGame.OnGameFinished -= OnPolishFinished;
            if (eveningLedger != null) eveningLedger.OnEndOfDayConfirmed -= OnEndOfDayConfirmed;
        }

        // ───── Triggers from the world ──────────────────────────────────

        internal void OnPlayerEnteredDoris()
        {
            if (_greetingStarted) return;
            _greetingStarted = true;
            StartCoroutine(GreetingFlow());
        }

        internal void OnPlayerApproachedWorkbench()
        {
            if (_polishStarted || _refusedPath) return;
            _polishStarted = true;
            StartCoroutine(PolishFlow());
        }

        private bool _greetingStarted;
        private bool _polishStarted;
        private bool _refusedPath;

        // ───── The dialogue flow ───────────────────────────────────────
        // All canonical text below is from Doris_M1.yarn nodes
        // (Doris_M1_Start, Doris_M1_BakeryEnter, Doris_M1_OfferOrb,
        // Doris_M1_AboutTheLoaves, Doris_M1_PriceNegotiation,
        // Doris_M1_HandOff, Doris_M1_RefusedPath).
        // Voice signature: bread metaphors, no Elric named in M1, ~12-word
        // sentences, no "goodbye" at end of M1.

        private IEnumerator GreetingFlow()
        {
            LockPlayer(true);

            // Doris_M1_Start — canonical opener.
            yield return Line(dorisVillager, "Doris", "You're the new one.", "doris_m1_greet_01");
            yield return Line(dorisVillager, "Doris", "I thought you'd be taller.", "doris_m1_greet_02");
            yield return Line(dorisVillager, "Doris", "Don't mind me — I thought that about the old one, too.", "doris_m1_greet_03");
            yield return Line(dorisVillager, "Doris", "Come in. The kettle's only just stopped.", "doris_m1_greet_04");

            // 3-option opener (Mission 1 Guide § 8.2).
            int reply = -1;
            PresentChoices(new[]
            {
                "Hello. Are you Doris?",
                "(Nod silently and follow her in)",
                "Who was the old one?",
            }, c => reply = c);
            while (reply < 0) yield return null;

            var vs = ServiceLocator.Get<VillageState>();
            switch (reply)
            {
                case 0:
                    yield return Line(dorisVillager, "Doris", "Aye. The very same.", "doris_m1_reply_help_01");
                    yield return Line(dorisVillager, "Doris", "They've put my name on the sign and everything. Look — there.", "doris_m1_reply_help_02");
                    break;
                case 1:
                    yield return Line(dorisVillager, "Doris", "A quiet one, then. Good.", "doris_m1_reply_silent_01");
                    yield return Line(dorisVillager, "Doris", "The bread likes quiet.", "doris_m1_reply_silent_02");
                    AdjustTrust("doris", +1);
                    break;
                case 2:
                    // The ONLY way to plant the Marin reveal thread in M1.
                    yield return Line(dorisVillager, "Doris", "... Mm.", "doris_m1_reply_unsure_01");
                    yield return Line(dorisVillager, "Doris", "That's a conversation for a longer day.", "doris_m1_reply_unsure_02");
                    yield return Line(dorisVillager, "Doris", "Come in. Tea first.", "doris_m1_reply_unsure_03");
                    if (vs != null) vs.askedAboutPredecessor = true;
                    AdjustTrust("doris", +2);
                    break;
            }

            // Doris_M1_BakeryEnter.
            if (vs != null) vs.metDoris = true;
            yield return Line(dorisVillager, "Doris", "Mind the flour.", "doris_m1_kitchen_01");
            yield return Line(dorisVillager, "Doris", "I haven't swept since Tuesday. I keep meaning to.", "doris_m1_kitchen_02");
            yield return Line(dorisVillager, "Doris", "... The shop next door is yours. The Hollow.", "doris_m1_kitchen_03");
            yield return Line(dorisVillager, "Doris", "I've been keeping the key safe for you.", "doris_m1_kitchen_04");

            // "First customer" preamble.
            yield return Line(dorisVillager, "Doris", "... I have something for you. Before you go in.", "doris_m1_offer_01");
            yield return Line(dorisVillager, "Doris", "I'd like to be your first customer, if that's all right.", "doris_m1_offer_02");

            // Doris_M1_OfferOrb — the iconic line.
            yield return Line(dorisVillager, "Doris", "This is the memory.", "doris_m1_memory_01");
            yield return Line(dorisVillager, "Doris", "Hold it like you'd hold a hot bun. Not by the side. Underneath.", "doris_m1_memory_02");
            yield return Line(dorisVillager, "Doris", "It's a small thing.", "doris_m1_memory_03");
            yield return Line(dorisVillager, "Doris", "First time I made bread that didn't shame me.", "doris_m1_memory_04");
            yield return Line(dorisVillager, "Doris", "Most days I think of it.", "doris_m1_memory_05");
            yield return Line(dorisVillager, "Doris", "I want to put it down, now, for a while.", "doris_m1_memory_06");
            yield return Line(dorisVillager, "Doris", "Will you take it?", "doris_m1_memory_07");

            // 3-option orb response.
            int orbReply = -1;
            PresentChoices(new[]
            {
                "I will. What do you want for it?",
                "Tell me more about it first.",
                "I'd rather not, today.",
            }, c => orbReply = c);
            while (orbReply < 0) yield return null;

            if (orbReply == 2)
            {
                // Refusal path — Mission 1 Guide § 9.4. Fully valid route.
                yield return Line(dorisVillager, "Doris", "Aye. Some days are not the day.", "doris_m1_defer_01");
                yield return Line(dorisVillager, "Doris", "I'll be here when one is.", "doris_m1_defer_02");
                if (vs != null)
                {
                    vs.refusedDorisOrb = true;
                    vs.vow1Integrity = VillageState.Adjust(vs.vow1Integrity, +2);
                }
                yield return EnterRefusedPath();
                yield break;
            }

            if (orbReply == 1)
            {
                // Doris_M1_AboutTheLoaves — the 5-line aside.
                yield return Line(dorisVillager, "Doris", "I was twenty-four.", "doris_m1_story_01");
                yield return Line(dorisVillager, "Doris", "The oven was new. The bricks were new. I was new.", "doris_m1_story_02");
                yield return Line(dorisVillager, "Doris", "I'd been baking other people's bread for nine years.", "doris_m1_story_03");
                yield return Line(dorisVillager, "Doris", "That morning was the first morning that was just mine.", "doris_m1_story_04");
                yield return Line(dorisVillager, "Doris", "I want to take a rest from carrying it. That's all.", "doris_m1_story_05");
            }

            // Doris_M1_PriceNegotiation — 3 options in COPPERS (per canonical).
            yield return Line(dorisVillager, "Doris", "Four coppers, if you're asking.", "doris_m1_price_01");
            yield return Line(dorisVillager, "Doris", "It's a small memory. I'll not have you overpay your first day.", "doris_m1_price_02");

            int price = -1;
            PresentChoices(new[]
            {
                "Four coppers is fair. (Honor the price)",
                "I'll pay six.",
                "Two coppers. That's all I have.",
            }, c => price = c);
            while (price < 0) yield return null;

            int coinDelta;
            switch (price)
            {
                case 0: // Honor
                    yield return Line(dorisVillager, "Doris", "Aye. Thank you.", "doris_m1_price_fair");
                    coinDelta = -4;
                    if (vs != null) vs.vow5Integrity = VillageState.Adjust(vs.vow5Integrity, +2);
                    break;
                case 1: // Pay 6 → she settles at 5
                    yield return Line(dorisVillager, "Doris", "That's too much. I'll not have you ruin yourself.", "doris_m1_price_high_01");
                    yield return Line(dorisVillager, "Doris", "Take it back. — Well. Take *some* back.", "doris_m1_price_high_02");
                    yield return Line(dorisVillager, "Doris", "Five, then. Final.", "doris_m1_price_high_03");
                    coinDelta = -5;
                    if (vs != null)
                    {
                        vs.vow5Integrity = VillageState.Adjust(vs.vow5Integrity, +3);
                        vs.trustDoris = VillageState.Adjust(vs.trustDoris, +1);
                    }
                    break;
                default: // Underpay 2 → debt thread
                    yield return Line(dorisVillager, "Doris", "...", "doris_m1_price_low_01");
                    yield return Line(dorisVillager, "Doris", "Aye, that'll do. Bring the rest when you find some.", "doris_m1_price_low_02");
                    coinDelta = -2;
                    if (vs != null)
                    {
                        vs.dorisOwesPlayer = -2;
                        vs.vow5Integrity = VillageState.Adjust(vs.vow5Integrity, -1);
                    }
                    break;
            }
            if (vs != null) vs.coin = Mathf.Max(0, vs.coin + coinDelta);

            // Doris_M1_HandOff — the first hint that Pickle has been here for years.
            yield return Line(dorisVillager, "Doris", "There.", "doris_m1_handover_01");
            yield return Line(dorisVillager, "Doris", "The old keeper showed me how to make it. Took me four tries.", "doris_m1_handover_02");
            yield return Line(dorisVillager, "Doris", "I cracked the first three. The cat watched me. Judged me, I think.", "doris_m1_handover_03");
            yield return Line(dorisVillager, "Doris", "I'll be in the bakery if you want me. Knock twice.", "doris_m1_handover_04");
            yield return Line(dorisVillager, "Doris", "There's a kettle on the workbench. Mind the wood stove — it bites.", "doris_m1_handover_05");

            // Hand off the orb visually.
            if (workbenchOrb != null) workbenchOrb.gameObject.SetActive(true);
            if (_workbenchProxy != null) _workbenchProxy.gameObject.SetActive(true);
            if (dorisGreetingTrigger != null) dorisGreetingTrigger.enabled = false;

            if (dialogueUI != null) dialogueUI.Hide();
            LockPlayer(false);

            Hh.Log(LogCategory.Mission, "Doris handed over the orb. Walk to the workbench to polish it.");
        }

        // ───── Refused path (Mission 1 Guide § 9.4) ─────────────────────

        private IEnumerator EnterRefusedPath()
        {
            _refusedPath = true;
            // Phase 32.9 — the 3 refused-path lines now carry lineIds so the
            // voice channel reads them too (previously typewriter-only — flagged
            // as a coverage gap in the PROGRESS.md Phase 32 acceptance section).
            yield return Line(dorisVillager, "Doris", "The shop's still yours.", "doris_m1_refused_01");
            yield return Line(dorisVillager, "Doris", "Go in. Sit a while. The kettle is on.", "doris_m1_refused_02");
            yield return Line(dorisVillager, "Doris", "I'll be here when you're ready.", "doris_m1_refused_03");

            if (dorisGreetingTrigger != null) dorisGreetingTrigger.enabled = false;
            if (dialogueUI != null) dialogueUI.Hide();
            LockPlayer(false);

            // No orb. No polish. No Memory Dream 1. The player explores the
            // Hollow, then can open the Evening Ledger (which will use the
            // EveningLedger_M1_Day1_Refused prose variant).
            StartCoroutine(WaitForRefusedLedger());
        }

        private IEnumerator WaitForRefusedLedger()
        {
            // Give the player ~90 seconds to explore. Then open the Ledger
            // automatically with the refusal-path prose.
            yield return new WaitForSeconds(90f);
            OpenRefusedLedger();
        }

        private void OpenRefusedLedger()
        {
            if (eveningLedger == null) return;
            var titles = new List<string>(); // no memories held
            string summary =
                "You arrived at the Hollow. The door was unlocked. The kettle was warm.\n" +
                "Doris, the baker, offered her First Loaves. You declined, this evening.\n" +
                "She did not seem to mind.\n\n" +
                "The shop is still yours.\n" +
                "Some days are not the day.";
            eveningLedger.Show(summary, titles);
            // Park the player on the modal ledger (D-069). Mouse still works.
            LockPlayer(true);
        }

        // ───── The polish flow ────────────────────────────────────────────

        private IEnumerator PolishFlow()
        {
            LockPlayer(true);

            // Phase 34 (2026-05-25) — replace the awkward stage-direction
            // line "(stands back and watches)" with a proper cozy dialogue
            // beat that explicitly hands control to the player. (Doris
            // stays in the bakery; the polish itself is private.) Then
            // surface the polish tutorial UI before unlocking input.
            yield return Line(dorisVillager, "Doris",
                "I'll wait. Take your time, Keeper.", "doris_m1_polish_watch");

            if (dialogueUI != null) dialogueUI.Hide();
            LockPlayer(false);

            if (polishGame == null)
            {
                Hh.Err(LogCategory.Mission, "Mission01Director: PolishMiniGame not wired.");
                yield break;
            }

            // Ensure the on-screen tutorial overlay exists. It auto-subscribes
            // to PolishMiniGame events and shows: headline, "Hold LMB · slow
            // circles" instructions, clarity progress bar, friction warnings,
            // and an Auto-Complete escape hatch. Fixes the user-reported
            // "game stuck after 'Doris stands back and watches'" UX bug.
            EnsureTutorialOverlay();

            polishGame.BeginGame(workbenchOrb);
        }

        /// <summary>
        /// Idempotently attaches a MiniGameTutorialUI to the polish game
        /// host so the overlay shows up the moment the mini-game starts.
        /// Auto-builds its own Canvas if none is present. (Phase 34.)
        /// </summary>
        private void EnsureTutorialOverlay()
        {
            if (polishGame == null) return;
            var existing = polishGame.GetComponent<MiniGameTutorialUI>();
            if (existing != null) return;
            polishGame.gameObject.AddComponent<MiniGameTutorialUI>();
            Hh.Log(LogCategory.Mission,
                "Mission01Director: spawned MiniGameTutorialUI on the polish game host.");
        }

        private bool _polishFinishedHandled;

        private void OnPolishFinished(MiniGameBase _)
        {
            if (_polishFinishedHandled) return;
            _polishFinishedHandled = true;
            StartCoroutine(PostPolishFlow());
        }

        private IEnumerator PostPolishFlow()
        {
            LockPlayer(true);
            yield return new WaitForSeconds(0.4f);

            float clarity = workbenchOrb != null ? workbenchOrb.runtimeClarity : 0.5f;
            string polishQuality;
            string afterPolishLine;
            string afterPolishLineId;
            int trustDelta;

            if (clarity >= 0.95f)
            {
                polishQuality = "Perfect";
                afterPolishLine = "You did it cleaner than I remembered it. I think you'll do.";
                afterPolishLineId = "doris_m1_polish_after_perfect";
                trustDelta = 3;
            }
            else if (clarity >= 0.55f)
            {
                polishQuality = "Acceptable";
                afterPolishLine = "You did it kindly. That's what matters.";
                afterPolishLineId = "doris_m1_polish_after_acceptable";
                trustDelta = 2;
            }
            else
            {
                polishQuality = "Mild";
                afterPolishLine = "... It's the morning still. A little dimmer. But mine. First days are like that. I won't hold it.";
                afterPolishLineId = "doris_m1_polish_after_mild";
                trustDelta = 1;
            }

            var vs = ServiceLocator.Get<VillageState>();
            if (vs != null) vs.polishQuality = polishQuality;

            // Doris_M1_AfterPolish_Return — the warm exit.
            yield return Line(dorisVillager, "Doris", "Aye.", "doris_m1_polish_done_01");
            yield return Line(dorisVillager, "Doris", "There it is. That's the morning.", "doris_m1_polish_done_02");
            // Phase 32.9 — clarity-branching afterPolishLine now has a per-branch
            // lineId so each of the 3 outcomes (Perfect / Acceptable / Mild) gets
            // its own voice clip. Previously typewriter-only (the "future Phase 32.1"
            // comment is now satisfied).
            yield return Line(dorisVillager, "Doris", afterPolishLine, afterPolishLineId);
            AdjustTrust("doris", trustDelta);

            // Canonical M1 exit — no "goodbye." Vellis Rule § 2.1 #6.
            yield return Line(dorisVillager, "Doris", "Sleep tonight. Dreams come.", "doris_m1_polish_sleep_01");
            yield return Line(dorisVillager, "Doris", "I'll see you again, eventually.", "doris_m1_polish_sleep_02");

            if (dialogueUI != null) dialogueUI.Hide();

            // Save the memory to the held list.
            if (vs != null && dorisMemory != null && !vs.heldMemoryIds.Contains(dorisMemory.id))
                vs.heldMemoryIds.Add(dorisMemory.id);

            // Show the Evening Ledger — canonical Day 1 Standard prose from
            // EveningLedger.yarn (Esme Cordray, fix commit 6/10 of the prior
            // content pass; this director routes the same text here).
            if (eveningLedger != null)
            {
                var titles = new List<string>();
                if (vs != null)
                {
                    foreach (var id in vs.heldMemoryIds)
                        titles.Add(dorisMemory != null && id == dorisMemory.id ? dorisMemory.title : id);
                }

                string summary =
                    "You arrived at the Hollow this evening.\n" +
                    "The door was unlocked. The kettle was warm.\n\n" +
                    "Doris, the baker, was your first visitor.\n" +
                    "She offered her First Loaves — a memory from the morning her bakery opened, fifty years ago.\n" +
                    "You polished it. You did your work. She seemed pleased.\n\n" +
                    "Pickle, the cat who lives in the shop, was on the windowsill.\n" +
                    "She did not move. She is watching you.\n\n" +
                    "The shop is quiet. The shelf is no longer empty.";
                eveningLedger.Show(summary, titles);
                // Keep movement locked while the modal end-of-day panel is open
                // so the player can't wander off behind it (D-069 polish). Mouse
                // input still works for the Save / Sleep buttons.
                LockPlayer(true);
            }
            else
            {
                LockPlayer(false);
            }
        }

        // ───── End of day ────────────────────────────────────────────

        private bool _endOfDayHandled;

        private void OnEndOfDayConfirmed()
        {
            // Single-fire guard (D-069): the Evening Ledger's confirm button now
            // guards itself too, but belt-and-braces here means the night chain
            // can never be entered twice (which previously raced two LoadScene
            // calls and contributed to the end-of-day soft-lock).
            if (_endOfDayHandled) return;
            _endOfDayHandled = true;

            // Keep the player parked through the dream / goodnight card / load,
            // and make sure the ledger is truly closed before the night beats.
            LockPlayer(true);
            if (eveningLedger != null) eveningLedger.Hide();

            var gm = GameManager.Instance;
            if (gm == null) return;

            // The original day-end behaviour — captured as a lambda so the
            // optional EndOfDaySequencer can run the night beats first, then
            // invoke it. Unwired sequencer = byte-for-byte the legacy path.
            System.Action transition = () =>
            {
                gm.EndDay();
                EventBus.Publish(new MissionCompletedEvent(null, "Mission01", "Day 1 — Doris's first loaves."));
                gm.LoadScene(sceneAfterEndOfDay);
            };

            if (endOfDaySequencer != null)
                endOfDaySequencer.BeginNightSequence(
                    playDream: dreamSequencer != null,
                    dreamTrigger: () => dreamSequencer?.PlayDream1(),
                    onComplete: transition);
            else
                transition();   // unchanged legacy path
        }

        // ───── Helpers ─────────────────────────────────────────────

        private void LockPlayer(bool locked)
        {
            if (playerController != null) playerController.MovementLocked = locked;
        }

        /// <summary>
        /// Present a dialogue line through the DialogueUI and wait for the
        /// typewriter + advance input. Phase 32 — the optional
        /// <paramref name="lineId"/> is passed straight through to
        /// <see cref="DialogueUI.PresentLine(string,string,Sprite,string)"/>
        /// so a matching voice clip plays in sync with the text. A null
        /// lineId behaves identically to the pre-Phase-32 path.
        /// </summary>
        private IEnumerator Line(VillagerSO villager, string speakerName, string lineText, string lineId = null)
        {
            if (dialogueUI == null) yield break;
            Sprite portrait = villager != null ? villager.portraitNeutral : null;
            dialogueUI.PresentLine(speakerName, lineText, portrait, lineId);
            while (dialogueUI.IsBusy) yield return null;
            yield return WaitForAdvance();
        }

        private void PresentChoices(string[] choices, System.Action<int> onPicked)
        {
            if (dialogueUI == null) { onPicked?.Invoke(0); return; }
            dialogueUI.PresentChoices(new List<string>(choices), onPicked);
        }

        private IEnumerator WaitForAdvance()
        {
            // Wait for left mouse / space / Return / E to advance the line.
            while (true)
            {
                if (Input.GetMouseButtonDown(0) ||
                    Input.GetKeyDown(KeyCode.Space) ||
                    Input.GetKeyDown(KeyCode.Return) ||
                    Input.GetKeyDown(KeyCode.E)) yield break;
                yield return null;
            }
        }

        private static void AdjustTrust(string id, int delta)
        {
            var vs = ServiceLocator.Get<VillageState>();
            if (vs == null) return;
            if (id == "doris") vs.trustDoris = VillageState.Adjust(vs.trustDoris, delta);
            else if (id == "gerrold") vs.trustGerrold = VillageState.Adjust(vs.trustGerrold, delta);
        }

        // ───── Trigger proxy components ──────────────────────────────────

        internal class DorisGreetingTrigger : MonoBehaviour
        {
            public Mission01Director director;
            private void OnTriggerEnter(Collider other)
            {
                if (other.CompareTag("Player")) director?.OnPlayerEnteredDoris();
            }
        }

        internal class WorkbenchApproachTrigger : MonoBehaviour
        {
            public Mission01Director director;
            private void OnTriggerEnter(Collider other)
            {
                if (other.CompareTag("Player")) director?.OnPlayerApproachedWorkbench();
            }
        }
    }
}
