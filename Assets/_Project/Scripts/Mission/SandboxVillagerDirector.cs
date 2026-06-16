// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / SandboxVillagerDirector
//
// Drives the full prototype interaction in 06_SandboxProto.unity:
//
//   Act 1 — Greeting     player walks up to Doris, presses E
//   Act 2 — Task         Doris hands over the orb job; orb appears on workbench
//   Act 3 — Polish       player goes to workbench, presses E, draws circles
//   Act 4 — Thanks       polish done; player walks back; Doris pays 4 coins
//
// No Yarn Spinner required — dialogue is driven by plain string arrays.
// The on-screen text is a single Canvas/TMP panel built by the Phase 77
// builder (or manually: a Canvas → Panel → TMP_Text "DialogueText" +
// TMP_Text "HintText").
//
// Execution order -800: runs after SandboxBootstrapper (-900) so
// VillageState is guaranteed registered when this Awake() fires.

using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using HearthboundHollow.Core;
using HearthboundHollow.Memory;
using HearthboundHollow.MiniGames;
using HearthboundHollow.Player;

namespace HearthboundHollow.Mission
{
    [DefaultExecutionOrder(-800)]
    public class SandboxVillagerDirector : MonoBehaviour
    {
        // ── Scene references (wired by Phase 77 builder or manually) ──────

        [Header("Characters")]
        [Tooltip("The Doris NPC transform — used for proximity detection.")]
        [SerializeField] private Transform villagerTransform;
        [SerializeField] private PlayerController player;

        [Header("Orb + mini-game")]
        [Tooltip("The MemoryOrbInteractable sitting on the workbench. " +
                 "Starts INACTIVE — director enables it once the task is given.")]
        [SerializeField] private MemoryOrbInteractable orbOnWorkbench;
        [SerializeField] private PolishMiniGame polishGame;

        [Header("UI (built by Phase 77 builder)")]
        [Tooltip("TMP_Text for dialogue lines — centred lower-third panel.")]
        [SerializeField] private TMP_Text dialogueText;
        [Tooltip("TMP_Text for context hints — smaller, above dialogue.")]
        [SerializeField] private TMP_Text hintText;
        [Tooltip("TMP_Text showing coin + task status — top-right corner.")]
        [SerializeField] private TMP_Text statusText;

        [Header("Tuning")]
        [Tooltip("Distance in metres at which the 'Press E' hint appears.")]
        [SerializeField] private float talkRange = 2.8f;
        [Tooltip("Seconds each dialogue line waits before the advance hint appears.")]
        [SerializeField] private float lineHoldSec = 0.6f;
        [Tooltip("Coin reward paid when polish is complete.")]
        [SerializeField] private int coinReward = 4;

        // ── Dialogue content (Doris voice — short, bread metaphors, warm) ─

        private static readonly string[] GreetingLines =
        {
            "DORIS:  You're the new one. I thought you'd be taller.",
            "DORIS:  Don't mind me — I said the same thing to the old Keeper.",
            "DORIS:  I've been holding on to this. Was afraid it'd drift before you arrived.",
        };

        private static readonly string[] TaskLines =
        {
            "DORIS:  My mother's First Loaves. Morning her bakery opened. Long time ago.",
            "DORIS:  It's gone dim. Happens when they sit in a drawer, untended.",
            "DORIS:  I'd like it cleaned up, if you're willing.",
            "DORIS:  The workbench is just there. Take your time, Keeper.",
        };

        private static readonly string[] ClosingLines =
        {
            "DORIS:  ...There it is.",
            "DORIS:  That's the morning. I can almost smell the bread.",
            "DORIS:  Thank you, Keeper. Four coppers — that's what I promised.",
        };

        // ── State machine ─────────────────────────────────────────────────

        private enum Phase
        {
            Idle,               // player not yet near Doris
            NearVillager,       // player in range — show "Press E" hint
            GreetingDialogue,   // Act 1 lines playing
            TaskDialogue,       // Act 2 lines playing → orb appears
            WaitingForPolish,   // player needs to walk to workbench
            Polishing,          // PolishMiniGame running
            PolishDone,         // auto-returns to Doris
            NearVillagerAgain,  // player back near Doris — show "Press E"
            ClosingDialogue,    // Act 4 lines playing
            Complete,           // all done
        }

        private Phase _phase = Phase.Idle;
        private int _lineIndex;
        private bool _canAdvance;       // guards against holding E advancing 2 lines
        private bool _eWasDown;

        // ── Lifecycle ─────────────────────────────────────────────────────

        private void Awake()
        {
            // Orb is hidden until Doris gives the task.
            if (orbOnWorkbench != null)
            {
                orbOnWorkbench.gameObject.SetActive(false);
                orbOnWorkbench.SetInteractable(false);
            }
            HideAllUI();
        }

        private void OnEnable()
        {
            if (orbOnWorkbench != null)
                orbOnWorkbench.OnExamineRequested += HandleOrbExamined;
            if (polishGame != null)
                polishGame.OnGameFinished += HandlePolishFinished;
            EventBus.Subscribe<MemoryPolishedEvent>(HandlePolished);
        }

        private void OnDisable()
        {
            if (orbOnWorkbench != null)
                orbOnWorkbench.OnExamineRequested -= HandleOrbExamined;
            if (polishGame != null)
                polishGame.OnGameFinished -= HandlePolishFinished;
            EventBus.Unsubscribe<MemoryPolishedEvent>(HandlePolished);
        }

        // ── Main update loop ──────────────────────────────────────────────

        private void Update()
        {
            UpdateStatusText();

            bool ePressedThisFrame = EPressed();

            switch (_phase)
            {
                case Phase.Idle:
                    if (IsPlayerNear(villagerTransform))
                        EnterPhase(Phase.NearVillager);
                    break;

                case Phase.NearVillager:
                    if (!IsPlayerNear(villagerTransform)) { EnterPhase(Phase.Idle); break; }
                    ShowHint("Press  E  to talk to Doris");
                    if (ePressedThisFrame) StartCoroutine(PlayDialogue(GreetingLines, Phase.TaskDialogue));
                    break;

                case Phase.NearVillagerAgain:
                    if (!IsPlayerNear(villagerTransform)) break;
                    ShowHint("Press  E  to speak with Doris");
                    if (ePressedThisFrame) StartCoroutine(PlayDialogue(ClosingLines, Phase.Complete));
                    break;

                case Phase.WaitingForPolish:
                    ShowHint("Walk to the workbench  ·  Press  E  on the orb");
                    break;

                case Phase.PolishDone:
                    ShowHint("Walk back to Doris");
                    if (IsPlayerNear(villagerTransform)) EnterPhase(Phase.NearVillagerAgain);
                    break;

                case Phase.Complete:
                    ShowHint("Task complete  ·  Well done, Keeper.");
                    break;
            }

            _eWasDown = IsERaw();
        }

        // ── Dialogue coroutine (shared by all three acts) ─────────────────

        private IEnumerator PlayDialogue(string[] lines, Phase nextPhase)
        {
            _phase = Phase.GreetingDialogue;    // generic "dialogue running" phase
            if (player != null) player.MovementLocked = true;
            HideHint();

            for (int i = 0; i < lines.Length; i++)
            {
                ShowDialogue(lines[i]);
                _canAdvance = false;

                // Brief hold so the player can read before E does anything.
                yield return new WaitForSeconds(lineHoldSec);
                _canAdvance = true;

                // Show "Press E" under the last word.
                ShowHint(i < lines.Length - 1 ? "Press  E  to continue" : "Press  E");

                // Wait for E.
                yield return new WaitUntil(() => EPressed());
                yield return null; // skip one frame so we don't double-fire

                HideHint();
            }

            HideDialogue();
            if (player != null) player.MovementLocked = false;

            // Special transition: Task dialogue ends by revealing the orb.
            if (nextPhase == Phase.TaskDialogue)
            {
                RevealOrb();
                EnterPhase(Phase.WaitingForPolish);
            }
            else
            {
                EnterPhase(nextPhase);
            }
        }

        // ── Orb & mini-game callbacks ─────────────────────────────────────

        private void HandleOrbExamined(MemoryOrbInteractable orb)
        {
            if (_phase != Phase.WaitingForPolish) return;

            orbOnWorkbench.SetInteractable(false);  // prevent re-trigger
            if (player != null) player.MovementLocked = true;
            HideHint();
            EnterPhase(Phase.Polishing);
            polishGame.BeginGame(orb);

            Hh.Log(LogCategory.MiniGame, "[Sandbox] Polish started.");
        }

        private void HandlePolishFinished(MiniGameBase game)
        {
            if (player != null) player.MovementLocked = false;
        }

        private void HandlePolished(MemoryPolishedEvent evt)
        {
            if (_phase != Phase.Polishing) return;

            var memTitle = evt.Memory is MemoryNodeSO m ? m.title : "orb";
            Hh.Log(LogCategory.MiniGame,
                $"[Sandbox] Polish complete — '{memTitle}' clarity {evt.Clarity01:F2}.");

            EnterPhase(Phase.PolishDone);
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private void RevealOrb()
        {
            if (orbOnWorkbench == null) return;
            orbOnWorkbench.gameObject.SetActive(true);
            orbOnWorkbench.SetClarity(0.2f);        // start faded
            orbOnWorkbench.SetCrackIntensity(0f);
            orbOnWorkbench.SetInteractable(true);
            Hh.Log(LogCategory.Memory, "[Sandbox] Orb revealed on workbench.");
        }

        private void PayCoin()
        {
            var vs = ServiceLocator.Get<VillageState>();
            if (vs == null) return;
            vs.coin += coinReward;
            Hh.Log(LogCategory.Mission, $"[Sandbox] +{coinReward} coins → total {vs.coin}.");
        }

        private bool IsPlayerNear(Transform target)
        {
            if (target == null || player == null) return false;
            return Vector3.Distance(player.transform.position, target.position) <= talkRange;
        }

        private void EnterPhase(Phase next)
        {
            _phase = next;

            // Pay the player when the closing dialogue finishes.
            if (next == Phase.Complete) PayCoin();
        }

        // ── E-key detection (multi-source, same pattern as PolishMiniGame) ─

        private bool EPressed()
        {
            if (!_canAdvance && _phase is Phase.GreetingDialogue or Phase.TaskDialogue) return false;

            bool newInput  = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
            bool legacyInput = Input.GetKeyDown(KeyCode.E);
            return newInput || legacyInput;
        }

        private bool IsERaw()
        {
            bool newInput  = Keyboard.current != null && Keyboard.current.eKey.isPressed;
            bool legacyInput = Input.GetKey(KeyCode.E);
            return newInput || legacyInput;
        }

        // ── UI helpers ────────────────────────────────────────────────────

        private void ShowDialogue(string line)
        {
            if (dialogueText == null) { Debug.Log(line); return; }
            // Show the panel parent (holds the dark background Image) then the text.
            var panel = dialogueText.transform.parent;
            if (panel != null) panel.gameObject.SetActive(true);
            dialogueText.text = line;
            dialogueText.gameObject.SetActive(true);
        }

        private void HideDialogue()
        {
            if (dialogueText == null) return;
            var panel = dialogueText.transform.parent;
            if (panel != null) panel.gameObject.SetActive(false);
            dialogueText.gameObject.SetActive(false);
        }

        private void ShowHint(string hint)
        {
            if (hintText == null) return;
            hintText.text = hint;
            hintText.gameObject.SetActive(true);
        }

        private void HideHint()
        {
            if (hintText != null) hintText.gameObject.SetActive(false);
        }

        private void HideAllUI()
        {
            HideDialogue();
            HideHint();
        }

        private void UpdateStatusText()
        {
            if (statusText == null) return;
            var vs = ServiceLocator.Get<VillageState>();
            int coin = vs != null ? vs.coin : 0;
            statusText.text = $"Coins: {coin}";
        }
    }
}
