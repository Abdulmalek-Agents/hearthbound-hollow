// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / Mission01Director
//
// The runtime orchestrator for the Mission 1 MVP playable slice (Phase 12).
//
// Unlike the long-form Yarn-driven dialogue (Phase 6+, which requires the
// optional Yarn Spinner package), this director sequences the gameplay
// directly using DialogueUI.PresentLine / PresentChoices. This lets the
// MVP run END-TO-END before the user installs Yarn Spinner.
//
// Flow:
//   1. On Start: lock player input (until dialogue ends)
//   2. Doris greets the player
//   3. Player picks a price (3 choices)
//   4. Doris hands off the orb (orb becomes visible on workbench)
//   5. Director listens for player to approach workbench
//   6. PolishMiniGame begins
//   7. On polish complete: Doris reacts (clarity-tiered line)
//   8. Evening Ledger opens
//   9. End Day → MainMenu

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

        // ───── Lifecycle ────────────────────────────────────────────

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

        // ───── Triggers from the world ──────────────────────────────

        internal void OnPlayerEnteredDoris()
        {
            if (_greetingStarted) return;
            _greetingStarted = true;
            StartCoroutine(GreetingFlow());
        }

        internal void OnPlayerApproachedWorkbench()
        {
            if (_polishStarted) return;
            _polishStarted = true;
            StartCoroutine(PolishFlow());
        }

        private bool _greetingStarted;
        private bool _polishStarted;

        // ───── The dialogue flow ────────────────────────────────────

        private IEnumerator GreetingFlow()
        {
            LockPlayer(true);

            yield return Line(dorisVillager, "Doris", "Oh — you must be the new Keeper.");
            yield return Line(dorisVillager, "Doris", "I'd heard. I just didn't expect... so soon.");

            // Choice 1: how does the player reply to the greeting?
            int reply = -1;
            PresentChoices(new[] { "I'm here to help.", "I'm not sure I'm ready.", "(wait, in silence)" }, c => reply = c);
            while (reply < 0) yield return null;
            switch (reply)
            {
                case 0:
                    yield return Line(dorisVillager, "Doris", "That's kind. That's — kind.");
                    AdjustTrust("doris", +5);
                    break;
                case 1:
                    yield return Line(dorisVillager, "Doris", "None of us ever are, dear. None of us ever are.");
                    break;
                case 2:
                    yield return Line(dorisVillager, "Doris", "It's all right. We can stand here a moment.");
                    break;
            }

            yield return Line(dorisVillager, "Doris", "I have a memory. The morning I first baked bread on my own.");
            yield return Line(dorisVillager, "Doris", "It's bright. So bright I sometimes can't go into the kitchen.");
            yield return Line(dorisVillager, "Doris", "I'd like to look at it less. I'd like it not to ache. Would you buy it?");

            // Choice 2: the price.
            int price = -1;
            PresentChoices(new[]
            {
                "I'll honor it. Five silver.",
                "Ten silver — it looks important.",
                "Three silver — I'm starting out.",
            }, c => price = c);
            while (price < 0) yield return null;

            int coinDelta = 0;
            switch (price)
            {
                case 0: coinDelta = -5;  AdjustTrust("doris", +5); break;
                case 1: coinDelta = -10; AdjustTrust("doris", +8); break;
                case 2: coinDelta = -3;  AdjustTrust("doris", +2); break;
            }
            var vs = ServiceLocator.Get<VillageState>();
            if (vs != null) vs.coin = Mathf.Max(0, vs.coin + coinDelta);

            yield return Line(dorisVillager, "Doris", price == 1
                ? "That's more than fair. Thank you."
                : "That's — fair.");

            yield return Line(dorisVillager, "Doris", "Polish it gently. The faster you press, the less the orb hears you.");
            yield return Line(dorisVillager, "Doris", "There's a note pinned above the workbench. Marin's hand. It still helps.");

            // Hand off the orb visually.
            if (workbenchOrb != null) workbenchOrb.gameObject.SetActive(true);
            if (_workbenchProxy != null) _workbenchProxy.gameObject.SetActive(true);
            // Doris no longer needs to greet us.
            if (dorisGreetingTrigger != null) dorisGreetingTrigger.enabled = false;

            if (dialogueUI != null) dialogueUI.Hide();
            LockPlayer(false);

            Hh.Log(LogCategory.Mission, "Doris handed over the orb. Walk to the workbench to polish it.");
        }

        // ───── The polish flow ──────────────────────────────────────

        private IEnumerator PolishFlow()
        {
            LockPlayer(true);

            yield return Line(dorisVillager, "Doris", "(stands back and watches)");
            if (dialogueUI != null) dialogueUI.Hide();

            // Camera/cursor unlocked; polish input is the cursor itself.
            LockPlayer(false);

            if (polishGame == null)
            {
                Hh.Err(LogCategory.Mission, "Mission01Director: PolishMiniGame not wired.");
                yield break;
            }
            polishGame.BeginGame(workbenchOrb);
            // Wait for OnGameFinished callback (sets the flag below).
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
            string line = clarity >= 0.95f
                ? "... You did it cleaner than I remembered. That's how it was. Yes."
                : clarity >= 0.55f
                    ? "It's a little less bright than I remembered. That's all right."
                    : "It's the morning still. A little dimmer. I'll come back tomorrow.";
            yield return Line(dorisVillager, "Doris", line);
            yield return Line(dorisVillager, "Doris", "Keep it on the shelf. Goodnight, Keeper.");

            if (dialogueUI != null) dialogueUI.Hide();

            // Save the memory to the held list.
            var vs = ServiceLocator.Get<VillageState>();
            if (vs != null && dorisMemory != null && !vs.heldMemoryIds.Contains(dorisMemory.id))
                vs.heldMemoryIds.Add(dorisMemory.id);

            // Show the Evening Ledger.
            if (eveningLedger != null)
            {
                var titles = new List<string>();
                if (vs != null)
                {
                    foreach (var id in vs.heldMemoryIds)
                        titles.Add(dorisMemory != null && id == dorisMemory.id ? dorisMemory.title : id);
                }
                string summary = clarity >= 0.95f
                    ? "Doris's first loaves now rest on your shelf, bright as the morning they were baked."
                    : clarity >= 0.55f
                        ? "Doris's first loaves are warm on your shelf. Not perfect, but kept."
                        : "The orb is dim. Doris said she'd come back. You think she will.";
                eveningLedger.Show(summary, titles);
            }
            LockPlayer(false);
        }

        // ───── End of day ───────────────────────────────────────────

        private void OnEndOfDayConfirmed()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            gm.EndDay();
            EventBus.Publish(new MissionCompletedEvent(null, "Mission01", "Day 1 — Doris's first loaves."));
            gm.LoadScene(sceneAfterEndOfDay);
        }

        // ───── Helpers ──────────────────────────────────────────────

        private void LockPlayer(bool locked)
        {
            if (playerController != null) playerController.MovementLocked = locked;
        }

        private IEnumerator Line(VillagerSO villager, string speakerName, string lineText)
        {
            if (dialogueUI == null) yield break;
            Sprite portrait = villager != null ? villager.portraitNeutral : null;
            dialogueUI.PresentLine(speakerName, lineText, portrait);
            while (dialogueUI.IsBusy) yield return null;
            // Wait briefly so the player can read, then advance on click.
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

        // ───── Trigger proxy components ─────────────────────────────

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
