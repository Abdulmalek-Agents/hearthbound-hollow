// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / Mission02Director
//
// The runtime orchestrator for Mission 2 ("The Widower's Request").
//
// Spans two scenes:
//   * 04_Mission02_Garden  — harvest herbs, brew tea at the kettle
//   * 05_Mission02_Cottage — speak with Gerrold, moral choice, Cleanse, Dream 2
//
// One director object per scene (the Phase 24 builder wires the right
// step set based on the scene name). The director uses VillageState flags to
// know what the player has already done so revisits are idempotent.
//
// Flow:
//   GARDEN
//     1. Lock player until intro line clears.
//     2. Marin's voice: "Lavender for openness. Valerian to forget for an hour."
//     3. Player harvests at least one herb.
//     4. Player walks to the kettle → TeaBrewingUI opens.
//     5. Tea brewed → unlock cottage door portal → load cottage scene.
//   COTTAGE
//     1. Title card + intro.
//     2. Approach Gerrold → multi-line dialogue.
//     3. ChoiceCard (Erase / Cleanse / Listen / Defer).
//     4. Branch:
//        * Erase   → Cleanse mini-game (aggressive) → Dream2 Variant A
//        * Cleanse → Cleanse mini-game (careful)    → Dream2 Variant B (or C if CrossedCore)
//        * Listen  → no mini-game                    → Dream2 Variant D
//        * Defer   → no mini-game; keep orb uncleansed → Dream2 Variant E
//     5. Evening Ledger.
//     6. End Day → MainMenu (Mission 3 is future).

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HearthboundHollow.Audio;
using HearthboundHollow.Core;
using HearthboundHollow.Cutscene;
using HearthboundHollow.Memory;
using HearthboundHollow.MiniGames;
using HearthboundHollow.Player;
using HearthboundHollow.UI;

namespace HearthboundHollow.Mission
{
    public class Mission02Director : MonoBehaviour
    {
        public enum SceneRole { Garden, Cottage }

        [Header("Scene role")]
        public SceneRole sceneRole = SceneRole.Garden;

        [Header("Common refs (set by SceneBuilder)")]
        public PlayerController playerController;
        public DialogueUI dialogueUI;
        public EveningLedgerUI eveningLedger;
        public MissionTitleCard titleCard;

        [Header("Garden refs")]
        public HerbHarvestInteractable lavenderPlant;
        public HerbHarvestInteractable valerianPlant;
        public KettleInteractable kettle;
        public TeaBrewingUI teaBrewingUI;
        public Collider gardenExitTrigger;            // portal collider that loads the cottage
        public string cottageSceneName = "05_Mission02_Cottage";

        [Header("Cottage refs")]
        public Transform gerroldTransform;
        public Collider gerroldGreetingTrigger;
        public ChoiceCardUI choiceCard;
        public MemoryOrbInteractable gerroldOrb;
        public CleanseMiniGame cleanseGame;
        public MemoryDreamSequencer dreamSequencer;

        [Header("Data (set by SceneBuilder via SeedAssetGenerator outputs)")]
        public VillagerSO gerroldVillager;
        public MemoryNodeSO gerroldMemory;             // GER-007
        public TariffSO tariffErase;
        public TariffSO tariffCleanse;
        public TariffSO tariffListen;
        public TariffSO tariffDefer;

        [Header("Routing")]
        public string sceneAfterEndOfDay = "01_MainMenu";

        // ───── Runtime state ──────────────────────────────────────

        private bool _gardenIntroPlayed;
        private bool _herbHarvestedOnce;
        private bool _teaBrewed;
        private bool _gerroldGreetingStarted;
        private bool _choiceMade;
        private bool _cleanseStarted;
        private bool _ledgerShown;
        private MoralChoice _selectedChoice;

        // ───── Lifecycle ──────────────────────────────────────────

        private void Start()
        {
            if (dialogueUI != null) dialogueUI.Hide();
            if (eveningLedger != null) eveningLedger.OnEndOfDayConfirmed += OnEndOfDayConfirmed;
            if (choiceCard != null) choiceCard.OnChoiceConfirmed += OnMoralChoicePicked;
            if (cleanseGame != null) cleanseGame.OnGameFinished += OnCleanseFinished;

            EventBus.Subscribe<HerbHarvestedEvent>(OnHerbHarvested);
            EventBus.Subscribe<TeaBrewedEvent>(OnTeaBrewed);

            if (kettle != null) kettle.OnBrewingRequested += OnKettleActivated;
            if (gardenExitTrigger != null)
                AddProxy<GardenExitProxy>(gardenExitTrigger).director = this;
            if (gerroldGreetingTrigger != null)
                AddProxy<GerroldGreetingProxy>(gerroldGreetingTrigger).director = this;

            switch (sceneRole)
            {
                case SceneRole.Garden:  StartCoroutine(GardenIntro()); break;
                case SceneRole.Cottage: StartCoroutine(CottageIntro()); break;
            }
        }

        private void OnDestroy()
        {
            if (eveningLedger != null) eveningLedger.OnEndOfDayConfirmed -= OnEndOfDayConfirmed;
            if (choiceCard != null) choiceCard.OnChoiceConfirmed -= OnMoralChoicePicked;
            if (cleanseGame != null) cleanseGame.OnGameFinished -= OnCleanseFinished;
            if (kettle != null) kettle.OnBrewingRequested -= OnKettleActivated;
            EventBus.Unsubscribe<HerbHarvestedEvent>(OnHerbHarvested);
            EventBus.Unsubscribe<TeaBrewedEvent>(OnTeaBrewed);
        }

        // ───── Garden: intro + harvest + brew ─────────────────────

        private IEnumerator GardenIntro()
        {
            if (titleCard != null) yield return new WaitForSeconds(2.0f);
            LockPlayer(true);
            yield return Line(gerroldVillager, "Marin's note", "Lavender for openness. Valerian to forget for an hour. Brew before you visit Gerrold — he will not speak otherwise.");
            yield return Line(null, "", "(harvest from the garden, then brew at the kettle)");
            if (dialogueUI != null) dialogueUI.Hide();
            LockPlayer(false);
            _gardenIntroPlayed = true;
        }

        private void OnHerbHarvested(HerbHarvestedEvent _)
        {
            if (!_herbHarvestedOnce)
            {
                _herbHarvestedOnce = true;
                Hh.Log(LogCategory.Mission, "Mission02: first herb harvested. Kettle is ready.");
            }
        }

        private void OnKettleActivated(KettleInteractable kettleRef)
        {
            if (!_herbHarvestedOnce)
            {
                StartCoroutine(LineSequence(
                    Line(gerroldVillager, "Marin's note", "Pick the herbs first. The kettle wants warmth from the garden, not from an empty pot.")));
                return;
            }
            if (teaBrewingUI != null) teaBrewingUI.Show();
            if (kettleRef != null) kettleRef.SetSteaming(true);
        }

        private void OnTeaBrewed(TeaBrewedEvent _)
        {
            if (_teaBrewed) return;
            _teaBrewed = true;
            StartCoroutine(PostBrewFlow());
        }

        private IEnumerator PostBrewFlow()
        {
            LockPlayer(true);
            yield return Line(gerroldVillager, "Marin's note", "Take the tea. The cottage is at the bend of the lane.");
            if (dialogueUI != null) dialogueUI.Hide();
            LockPlayer(false);
            // Garden exit trigger is already in the scene; the player walks through it.
        }

        internal void OnPlayerExitedGarden()
        {
            if (!_teaBrewed)
            {
                StartCoroutine(LineSequence(
                    Line(gerroldVillager, "Marin's note", "The kettle, first. He cannot meet you empty-handed.")));
                return;
            }
            var gm = GameManager.Instance;
            if (gm != null) gm.LoadScene(cottageSceneName);
        }

        // ───── Cottage: intro + greeting + choice + cleanse ───────

        private IEnumerator CottageIntro()
        {
            if (titleCard != null) yield return new WaitForSeconds(2.0f);
            LockPlayer(true);
            yield return Line(gerroldVillager, "(narration)", "The cottage is quiet. The chair by the bed is still pulled out.");
            yield return Line(null, "", "(walk to Gerrold)");
            if (dialogueUI != null) dialogueUI.Hide();
            LockPlayer(false);
        }

        internal void OnPlayerEnteredGerrold()
        {
            if (_gerroldGreetingStarted) return;
            _gerroldGreetingStarted = true;
            StartCoroutine(GerroldGreetingFlow());
        }

        private IEnumerator GerroldGreetingFlow()
        {
            LockPlayer(true);

            yield return Line(gerroldVillager, "Gerrold", "You came. I — I wasn't sure you would.");
            yield return Line(gerroldVillager, "Gerrold", "I haven't slept the whole night through since she — since the seventh morning.");
            yield return Line(gerroldVillager, "Gerrold", "The orb is there. On the table. I wrapped it in her shawl so I wouldn't see it.");

            int reply = -1;
            PresentChoices(new[]
            {
                "I'm here. Tell me what you want.",
                "(sit down beside him without a word)",
                "Are you sure? You can take more time.",
            }, c => reply = c);
            while (reply < 0) yield return null;

            switch (reply)
            {
                case 0:
                    yield return Line(gerroldVillager, "Gerrold", "I want it to stop hurting. That's all. I don't know which way."); break;
                case 1:
                    AdjustTrust("gerrold", +6);
                    yield return Line(gerroldVillager, "Gerrold", "Thank you for not asking. Doris said you'd know."); break;
                case 2:
                    AdjustTrust("gerrold", +3);
                    yield return Line(gerroldVillager, "Gerrold", "I've had enough time. Time isn't what hurts."); break;
            }

            yield return Line(gerroldVillager, "Gerrold", "There are ways, Marin said. Four of them. I — pick one for me. Or with me.");

            if (dialogueUI != null) dialogueUI.Hide();
            LockPlayer(false);

            // Show the 4-tariff moral choice card.
            ShowChoiceCard();
        }

        private void ShowChoiceCard()
        {
            if (choiceCard == null || gerroldMemory == null)
            {
                Hh.Err(LogCategory.Mission, "Mission02: ChoiceCard or memory not wired.");
                return;
            }
            var tariffs = new List<TariffSO>();
            if (tariffErase   != null) tariffs.Add(tariffErase);
            if (tariffCleanse != null) tariffs.Add(tariffCleanse);
            if (tariffListen  != null) tariffs.Add(tariffListen);
            if (tariffDefer   != null) tariffs.Add(tariffDefer);

            choiceCard.Show(gerroldMemory,
                "What does the orb need from you?", tariffs);
        }

        private void OnMoralChoicePicked(MoralChoice choice)
        {
            if (_choiceMade) return;
            _choiceMade = true;
            _selectedChoice = choice;
            StartCoroutine(BranchFromChoice(choice));
        }

        private IEnumerator BranchFromChoice(MoralChoice choice)
        {
            LockPlayer(true);
            switch (choice)
            {
                case MoralChoice.Erase:
                    yield return Line(gerroldVillager, "Gerrold", "All of it. Take all of it.");
                    yield return new WaitForSeconds(0.4f);
                    if (gerroldOrb != null) gerroldOrb.gameObject.SetActive(true);
                    if (cleanseGame != null) { _cleanseStarted = true; cleanseGame.gentleMode = true; cleanseGame.BeginGame(gerroldOrb); }
                    else FallbackEndMission(); // no mini-game in scene; skip ahead
                    break;
                case MoralChoice.Cleanse:
                    yield return Line(gerroldVillager, "Gerrold", "Take the worst of it. Leave me her face. Please.");
                    yield return new WaitForSeconds(0.4f);
                    if (gerroldOrb != null) gerroldOrb.gameObject.SetActive(true);
                    if (cleanseGame != null) { _cleanseStarted = true; cleanseGame.gentleMode = false; cleanseGame.BeginGame(gerroldOrb); }
                    else FallbackEndMission();
                    break;
                case MoralChoice.Listen:
                    yield return Line(gerroldVillager, "Gerrold", "Then — sit. Just sit.");
                    yield return new WaitForSeconds(2.5f);
                    yield return Line(gerroldVillager, "Gerrold", "She used to hum. Did you know? I don't know if I'm remembering or making it up. It doesn't matter.");
                    yield return new WaitForSeconds(1.0f);
                    yield return Line(gerroldVillager, "Gerrold", "Thank you. I think I can sleep now.");
                    var vsL = ServiceLocator.Get<VillageState>();
                    if (vsL != null) vsL.trustGerrold = VillageState.Adjust(vsL.trustGerrold, +12);
                    OpenLedgerListen();
                    break;
                case MoralChoice.Defer:
                    yield return Line(gerroldVillager, "Gerrold", "Hold it for now. I'm not — I'm not ready.");
                    yield return Line(gerroldVillager, "Gerrold", "Take it back to the Hollow. Tomorrow, maybe. Or never. We'll see.");
                    OpenLedgerDefer();
                    break;
            }
        }

        private void OnCleanseFinished(MiniGameBase _)
        {
            if (!_cleanseStarted) return;
            _cleanseStarted = false;
            StartCoroutine(PostCleanseFlow());
        }

        private IEnumerator PostCleanseFlow()
        {
            yield return new WaitForSeconds(0.6f);
            var outcome = cleanseGame != null ? cleanseGame.ComputedOutcome : CleanseOutcome.Acceptable;
            string line;
            switch (outcome)
            {
                case CleanseOutcome.Perfect:
                    line = "Lighter. She is still there. Lighter."; break;
                case CleanseOutcome.Acceptable:
                    line = "Not all of it. But enough. Thank you."; break;
                case CleanseOutcome.Sloppy:
                    line = "It hurts differently now. I don't know if that's better."; break;
                default:
                    line = "Did you — was she always like that? I can't remember her hands."; break;
            }
            yield return Line(gerroldVillager, "Gerrold", line);

            if (dreamSequencer != null) dreamSequencer.PlayDream2(_selectedChoice, outcome);
            else Hh.Warn(LogCategory.Mission, "Mission02: no MemoryDreamSequencer wired — skipping Dream 2.");
            OpenLedgerCleanseResult(outcome);
        }

        // ───── Evening Ledger paths ───────────────────────────────

        private void OpenLedgerCleanseResult(CleanseOutcome outcome)
        {
            var vs = ServiceLocator.Get<VillageState>();
            var titles = new List<string>();
            if (vs != null)
            {
                if (gerroldMemory != null && !vs.heldMemoryIds.Contains(gerroldMemory.id))
                    vs.heldMemoryIds.Add(gerroldMemory.id);
                foreach (var id in vs.heldMemoryIds) titles.Add(id);
            }
            string summary = outcome switch
            {
                CleanseOutcome.Perfect   => "Gerrold's seventh morning is gentler now. The shawl is folded on the table.",
                CleanseOutcome.Acceptable => "Gerrold's grief has a softer shape. He says he'll sleep tonight.",
                CleanseOutcome.Sloppy    => "Gerrold's memory bears your fingerprints. He is grateful, but you are not sure you should be.",
                _                        => "You crossed the warm center. Gerrold's wife is — different in him now. He did not say it. You heard it anyway.",
            };
            if (eveningLedger != null) eveningLedger.Show(summary, titles);
            _ledgerShown = true;
        }

        private void OpenLedgerListen()
        {
            var vs = ServiceLocator.Get<VillageState>();
            var titles = new List<string>();
            if (vs != null) foreach (var id in vs.heldMemoryIds) titles.Add(id);
            if (eveningLedger != null)
                eveningLedger.Show("You took nothing from Gerrold today. You sat. The village will quietly notice.", titles);
            _ledgerShown = true;
        }

        private void OpenLedgerDefer()
        {
            var vs = ServiceLocator.Get<VillageState>();
            var titles = new List<string>();
            if (vs != null)
            {
                if (gerroldMemory != null && !vs.heldMemoryIds.Contains(gerroldMemory.id))
                    vs.heldMemoryIds.Add(gerroldMemory.id);
                foreach (var id in vs.heldMemoryIds) titles.Add(id);
            }
            if (eveningLedger != null)
                eveningLedger.Show("Gerrold's orb rests on the counter in the Hollow, wrapped in shawl. You will decide tomorrow.", titles);
            _ledgerShown = true;
        }

        private void FallbackEndMission()
        {
            Hh.Warn(LogCategory.Mission, "Mission02: cleanse mini-game not wired; skipping to ledger.");
            OpenLedgerCleanseResult(CleanseOutcome.Acceptable);
        }

        private void OnEndOfDayConfirmed()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            gm.EndDay();
            EventBus.Publish(new MissionCompletedEvent(null, "Mission02", $"Day {ServiceLocator.Get<VillageState>()?.currentDayIndex ?? 0} — Gerrold's choice."));
            gm.LoadScene(sceneAfterEndOfDay);
        }

        // ───── Helpers ────────────────────────────────────────────

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
            yield return WaitForAdvance();
        }

        private IEnumerator LineSequence(IEnumerator step)
        {
            yield return step;
            if (dialogueUI != null) dialogueUI.Hide();
        }

        private void PresentChoices(string[] choices, System.Action<int> onPicked)
        {
            if (dialogueUI == null) { onPicked?.Invoke(0); return; }
            dialogueUI.PresentChoices(new List<string>(choices), onPicked);
        }

        private IEnumerator WaitForAdvance()
        {
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

        private T AddProxy<T>(Collider c) where T : MonoBehaviour
        {
            return c.gameObject.AddComponent<T>();
        }

        // ───── Trigger proxies ────────────────────────────────────

        internal class GardenExitProxy : MonoBehaviour
        {
            public Mission02Director director;
            private void OnTriggerEnter(Collider other)
            {
                if (other.CompareTag("Player")) director?.OnPlayerExitedGarden();
            }
        }

        internal class GerroldGreetingProxy : MonoBehaviour
        {
            public Mission02Director director;
            private void OnTriggerEnter(Collider other)
            {
                if (other.CompareTag("Player")) director?.OnPlayerEnteredGerrold();
            }
        }
    }
}
