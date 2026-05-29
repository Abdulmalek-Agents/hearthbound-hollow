// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / Mission02Director
//
// The runtime orchestrator for Mission 2 ("The Widower's Request").
//
// Spans two scenes:
//   * 04_Mission02_Garden  — harvest herbs, brew tea at the kettle
//   * 05_Mission02_Cottage — speak with Gerrold, moral choice, Cleanse, Dream 2
//
// ── Phase 60 — Arabic Localization MVP ──────────────────────────
// `Line(...)` now accepts an optional `lineId` that DialogueUI passes
// through to LocalizationService.GetDialogue + VoicePlayer.Play. When
// the active locale is Arabic and the lineId resolves a translation,
// the Arabic text + Arabic voice clip play. Null lineId behaves
// identically to the pre-Phase-60 path (English source-of-truth +
// no voice clip — zero regression).
//
// Gerrold's 8 canonical Piper-voiced lines + 4 cottage-door entry
// lines are threaded with their stable lineIds.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HearthboundHollow.Audio;
using HearthboundHollow.Core;
using HearthboundHollow.Cutscene;
using HearthboundHollow.Dialogue;          // ← build-fix: YarnHeavyThemeCardEvent + YarnEchoWebActivateEvent
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
        public Collider gardenExitTrigger;
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
        public VillagerSO dorisVillager;
        public MemoryNodeSO gerroldMemory;        // GER-007
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
            _gardenIntroPlayed = true;
            Hh.Log(LogCategory.Mission, "Mission02: Garden ready. Harvest a herb, then brew tea.");
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
                    Line(null, "Pickle",
                        "Pick the herbs first. The kettle wants warmth from the garden, not from an empty pot.")));
                return;
            }
            if (teaBrewingUI != null) teaBrewingUI.Show();
            if (kettleRef != null) kettleRef.SetSteaming(true);
        }

        private void OnTeaBrewed(TeaBrewedEvent ev)
        {
            if (_teaBrewed) return;
            _teaBrewed = true;
            var vs = ServiceLocator.Get<VillageState>();
            if (vs != null && ev.Herb != null)
            {
                vs.teaBrewed =
                    ev.Herb.name.ToLowerInvariant().Contains("valerian") ? "Valerian" : "Lavender";
            }
            StartCoroutine(PostBrewFlow());
        }

        private IEnumerator PostBrewFlow()
        {
            LockPlayer(true);
            yield return Line(null, "Pickle",
                "Tea for a man who has not asked for tea is the rarest kindness.");
            yield return Line(null, "Pickle",
                "Try not to overthink it. The man is also a cat about this.");
            yield return Line(null, "Pickle",
                "We will accept.");
            if (dialogueUI != null) dialogueUI.Hide();
            LockPlayer(false);
        }

        internal void OnPlayerExitedGarden()
        {
            var gm = GameManager.Instance;
            if (gm != null) gm.LoadScene(cottageSceneName);
        }

        // ───── Cottage: intro + greeting + choice + cleanse ───────

        private IEnumerator CottageIntro()
        {
            if (titleCard != null) yield return new WaitForSeconds(2.0f);
            ShowHeavyThemeCardIfEnabled();
            LockPlayer(false);
        }

        private void ShowHeavyThemeCardIfEnabled()
        {
            EventBus.Publish(new YarnHeavyThemeCardEvent(
                "Loss of spouse, Terminal illness (referenced), A man's grief"));
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
            var vs = ServiceLocator.Get<VillageState>();
            if (vs != null) vs.metGerrold = true;

            // Phase 60 — lineIds threaded for Arabic translation lookup +
            // future per-character voice clip resolution. The door-step trio
            // currently has no Piper voice clips (only the 8 Mission 2
            // canon stubs above); these lineIds resolve a translation only.
            yield return Line(gerroldVillager, "Gerrold", "I'm sorry. Are you the new keeper?",                       "gerrold_m2_door_01");
            yield return Line(gerroldVillager, "Gerrold", "I should have written first. I don't know if there's a way to do that.", "gerrold_m2_door_02");
            yield return Line(gerroldVillager, "Gerrold", "... My name is Gerrold Pell. I — could I come in for a moment?", "gerrold_m2_door_03");

            int reply = -1;
            PresentChoices(new[]
            {
                "Yes. Please.",
                "What's this about?",
                "(Step aside silently and gesture him in)",
            }, c => reply = c);
            while (reply < 0) yield return null;

            switch (reply)
            {
                case 0:
                    yield return Line(gerroldVillager, "Gerrold", "Thank you. Just for a moment.", "gerrold_m2_enter_01");
                    break;
                case 1:
                    yield return Line(gerroldVillager, "Gerrold", "... It's a memory. I brought one.", "gerrold_m2_enter_02");
                    yield return Line(gerroldVillager, "Gerrold", "I'm sorry — that's what one does, isn't it? I should know how it works.", "gerrold_m2_enter_03");
                    break;
                case 2:
                    yield return Line(gerroldVillager, "Gerrold", "Thank you. You are kind.", "gerrold_m2_enter_04");
                    if (vs != null) vs.vow7Integrity = VillageState.Adjust(vs.vow7Integrity, +1);
                    break;
            }

            yield return Line(gerroldVillager, "Gerrold", "I'd offer you tea but it's your kettle.");
            yield return Line(gerroldVillager, "Gerrold", "... I'm sorry. That was a joke. It wasn't a good one.");

            int teaReply = -1;
            PresentChoices(new[]
            {
                "Would you like tea?",
                "(Sit across from him and wait)",
            }, c => teaReply = c);
            while (teaReply < 0) yield return null;

            if (teaReply == 0)
            {
                yield return Line(gerroldVillager, "Gerrold", "Aye, that — no. No, thank you. Not today.");
                yield return Line(gerroldVillager, "Gerrold", "It's kind of you to ask.");
                if (vs != null)
                {
                    vs.offeredGerroldTea = true;
                    vs.trustGerrold = VillageState.Adjust(vs.trustGerrold, +1);
                }
            }
            else
            {
                yield return Line(gerroldVillager, "Gerrold", "... Thank you. For not — for not making me say it quickly.");
                if (vs != null) vs.vow7Integrity = VillageState.Adjust(vs.vow7Integrity, +1);
            }

            yield return Line(gerroldVillager, "Gerrold", "I have a memory I want to be rid of.");
            yield return Line(gerroldVillager, "Gerrold", "Not the whole thing. Just the long bit.");
            yield return Line(gerroldVillager, "Gerrold", "My wife — Margery — died last winter. Eleven months last week.");
            yield return Line(gerroldVillager, "Gerrold", "I have the memory of the last week of her life. It is the heaviest thing I own.");
            yield return Line(gerroldVillager, "Gerrold", "I want to keep my wife. I do not want to keep the long bit.", "gerrold_m2_long_bit_01");

            int followUp = -1;
            PresentChoices(new[]
            {
                "What was the long bit?",
                "Tell me about Margery first.",
                "(Just nod and wait)",
            }, c => followUp = c);
            while (followUp < 0) yield return null;

            switch (followUp)
            {
                case 0:
                    yield return Line(gerroldVillager, "Gerrold", "She was ill for fourteen months.");
                    yield return Line(gerroldVillager, "Gerrold", "The dying part — the last week — was kind, in its way.");
                    yield return Line(gerroldVillager, "Gerrold", "She wasn't in pain by then. She slept a lot.");
                    yield return Line(gerroldVillager, "Gerrold", "But I was awake the whole time. Six days awake. Watching.");
                    yield return Line(gerroldVillager, "Gerrold", "I cannot un-be the man who watched.");
                    yield return Line(gerroldVillager, "Gerrold", "But I think I do not need to remember every hour of it.");
                    break;
                case 1:
                    yield return Line(gerroldVillager, "Gerrold", "Margery was — quiet. She read more than she spoke.");
                    yield return Line(gerroldVillager, "Gerrold", "She worked at the schoolhouse for thirty years.");
                    yield return Line(gerroldVillager, "Gerrold", "Helped Lavinia Embry with the small ones who hadn't learned to read yet.");
                    yield return Line(gerroldVillager, "Gerrold", "She was Doris the baker's best friend.");
                    yield return Line(gerroldVillager, "Gerrold", "They sat in the kitchen on Sundays for — for thirty years. With bread.");
                    if (vs != null && vs.metDoris)
                    {
                        yield return Line(gerroldVillager, "Gerrold", "You met Doris yesterday, didn't you? She'd have given you bread.");
                        yield return Line(gerroldVillager, "Gerrold", "... Margery would have wanted to know you.");
                    }
                    yield return Line(gerroldVillager, "Gerrold", "She taught me how to be quiet around someone you love without making it feel like distance.");
                    yield return Line(gerroldVillager, "Gerrold", "That is — that is the gift I lost.");
                    yield return Line(gerroldVillager, "Gerrold", "I am very loud, now, on my own.");
                    break;
                case 2:
                    yield return Line(gerroldVillager, "Gerrold", "... Thank you. I'll get to it.");
                    break;
            }

            yield return Line(gerroldVillager, "Gerrold", "It's in the cloth. Margery wrapped it.");
            yield return Line(gerroldVillager, "Gerrold", "I think she wrapped it for this.");

            if (gerroldOrb != null) gerroldOrb.gameObject.SetActive(true);
            EventBus.Publish(new YarnEchoWebActivateEvent("DOR-001", "GER-007", "Doris in kitchen on Sunday"));

            yield return new WaitForSeconds(0.6f);
            yield return Line(null, "Pickle", "Ah. The Web has decided to speak today.");
            yield return Line(null, "Pickle", "This one and yesterday's are in the same kitchen on the same Sunday.");
            yield return Line(null, "Pickle", "That is what the line of light is saying. Look at it. It is showing off.");

            if (vs != null && !string.IsNullOrEmpty(vs.teaBrewed))
            {
                yield return Line(gerroldVillager, "Gerrold", "... is that tea?");
                yield return Line(gerroldVillager, "Gerrold", "... thank you.");
                if (vs.teaBrewed == "Lavender")
                {
                    yield return Line(gerroldVillager, "Gerrold", "Margery and I had this tea on our last winter together.");
                    yield return Line(gerroldVillager, "Gerrold", "We had it every evening.");
                    yield return Line(gerroldVillager, "Gerrold", "She said it tasted like the inside of a folded sweater.");
                    yield return Line(gerroldVillager, "Gerrold", "I knew what she meant. I still do.");
                }
                else
                {
                    yield return Line(gerroldVillager, "Gerrold", "Aye. This is the one she used to make me when I could not sleep.");
                    yield return Line(gerroldVillager, "Gerrold", "She would put a sprig of it on the pillow when I — when I could not stop thinking.");
                    yield return Line(gerroldVillager, "Gerrold", "I have not had this in eleven months. ... Thank you. You are very kind.");
                }
                vs.trustGerrold = VillageState.Adjust(vs.trustGerrold, +2);
            }

            yield return Line(gerroldVillager, "Gerrold", "Well, then.");
            yield return Line(gerroldVillager, "Gerrold", "I've said what I've come to say.");
            yield return Line(gerroldVillager, "Gerrold", "I'd like to give you the choice.");

            if (dialogueUI != null) dialogueUI.Hide();
            LockPlayer(false);

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
                "Gerrold has placed the orb on the table. He is waiting.",
                tariffs);
        }

        private void OnMoralChoicePicked(MoralChoice choice)
        {
            if (_choiceMade) return;

            if (choice == MoralChoice.Erase)
            {
                StartCoroutine(EraseSecondConfirmFlow());
                return;
            }

            _choiceMade = true;
            _selectedChoice = choice;
            var vs = ServiceLocator.Get<VillageState>();
            if (vs != null) vs.gerroldChoice = ChoiceToString(choice);
            StartCoroutine(BranchFromChoice(choice));
        }

        private IEnumerator EraseSecondConfirmFlow()
        {
            LockPlayer(true);
            yield return Line(gerroldVillager, "Gerrold", "... Aye. The long bit gone.");
            yield return Line(gerroldVillager, "Gerrold", "All of it.");
            yield return Line(gerroldVillager, "Gerrold", "I — I want to be certain that's what I'm asking for.");

            int confirm = -1;
            PresentChoices(new[]
            {
                "Yes, all of it. Erase the long bit.",
                "Wait — let me think a moment more.",
            }, c => confirm = c);
            while (confirm < 0) yield return null;

            if (confirm == 1)
            {
                LockPlayer(false);
                ShowChoiceCard();
                yield break;
            }

            yield return Line(gerroldVillager, "Gerrold", "Aye. Thank you.");
            yield return Line(gerroldVillager, "Gerrold", "I'll be here.");

            _choiceMade = true;
            _selectedChoice = MoralChoice.Erase;
            var vs = ServiceLocator.Get<VillageState>();
            if (vs != null) vs.gerroldChoice = "erase";
            StartCoroutine(BranchFromChoice(MoralChoice.Erase));
        }

        private IEnumerator BranchFromChoice(MoralChoice choice)
        {
            LockPlayer(true);
            var vs = ServiceLocator.Get<VillageState>();

            switch (choice)
            {
                case MoralChoice.Erase:
                    yield return new WaitForSeconds(0.4f);
                    if (gerroldOrb != null) gerroldOrb.gameObject.SetActive(true);
                    if (cleanseGame != null)
                    {
                        EnsureCleanseTutorialOverlay();
                        _cleanseStarted = true;
                        cleanseGame.gentleMode = false;
                        cleanseGame.BeginGame(gerroldOrb);
                    }
                    else FallbackEndMission();
                    break;

                case MoralChoice.Cleanse:
                    yield return Line(gerroldVillager, "Gerrold", "Aye.");
                    yield return Line(gerroldVillager, "Gerrold", "... With care, then. Whatever you can do.");
                    yield return Line(gerroldVillager, "Gerrold", "I trust you. I — I don't know you, but I trust you.");
                    if (vs != null) vs.vow3Integrity = VillageState.Adjust(vs.vow3Integrity, +1);
                    yield return new WaitForSeconds(0.4f);
                    if (gerroldOrb != null) gerroldOrb.gameObject.SetActive(true);
                    if (cleanseGame != null)
                    {
                        EnsureCleanseTutorialOverlay();
                        _cleanseStarted = true;
                        cleanseGame.gentleMode = (vs != null && vs.gentleModeEnabled);
                        cleanseGame.BeginGame(gerroldOrb);
                    }
                    else FallbackEndMission();
                    break;

                case MoralChoice.Listen:
                    yield return Line(gerroldVillager, "Gerrold", "... I don't —");
                    yield return Line(gerroldVillager, "Gerrold", "I don't understand.");
                    yield return Line(gerroldVillager, "Gerrold", "You — you don't want to take it?");

                    int listenSub = -1;
                    PresentChoices(new[]
                    {
                        "I want to listen. Tell me about it. Hold it the whole time. I won't take it.",
                        "(Reach out and gently place his hands around the orb.)",
                    }, c => listenSub = c);
                    while (listenSub < 0) yield return null;

                    int vow7Bonus = (listenSub == 0) ? 5 : 7;
                    int cinderBonus = (listenSub == 0) ? 2 : 3;
                    if (listenSub == 0)
                    {
                        yield return Line(gerroldVillager, "Gerrold", "...");
                        yield return Line(gerroldVillager, "Gerrold", "... All right.");
                        yield return Line(gerroldVillager, "Gerrold", "All right. ... All right.");
                    }
                    else
                    {
                        yield return Line(gerroldVillager, "Gerrold", "Aye.");
                        yield return Line(gerroldVillager, "Gerrold", "... Aye.");
                    }
                    if (vs != null)
                    {
                        vs.vow7Integrity = VillageState.Adjust(vs.vow7Integrity, vow7Bonus);
                        vs.cinder = Mathf.Max(0, vs.cinder + cinderBonus);
                    }

                    yield return PlayListenScene();

                    if (vs != null)
                    {
                        vs.trustGerrold = VillageState.Adjust(vs.trustGerrold, +6);
                    }
                    OpenLedgerListen();
                    break;

                case MoralChoice.Defer:
                    yield return Line(gerroldVillager, "Gerrold", "Aye.");
                    yield return Line(gerroldVillager, "Gerrold", "A day, then.");
                    yield return Line(gerroldVillager, "Gerrold", "I will come back. I will not be impatient.");
                    if (vs != null)
                    {
                        vs.trustGerrold = VillageState.Adjust(vs.trustGerrold, +1);
                        vs.gerroldReturnsDay3 = true;
                    }
                    OpenLedgerDefer();
                    break;
            }
        }

        private IEnumerator PlayListenScene()
        {
            yield return Line(gerroldVillager, "Gerrold", "The bedside table was on her right. The lamp was the one her mother gave us.");
            yield return Line(gerroldVillager, "Gerrold", "She asked for the handkerchief. I brought it. I put it in her hand.");
            yield return Line(gerroldVillager, "Gerrold", "She looked at the embroidery. She smiled at the embroidery.");
            yield return Line(gerroldVillager, "Gerrold", "She had done it when she was twenty-two.");
            yield return new WaitForSeconds(1.2f);
            yield return Line(gerroldVillager, "Gerrold", "On the seventh morning the snow had stopped.");
            yield return Line(gerroldVillager, "Gerrold", "Doris was in the kitchen. I could hear her, downstairs.");
            yield return Line(gerroldVillager, "Gerrold", "I made tea. I brought it in. She — was already gone.");
            yield return new WaitForSeconds(1.5f);
            yield return Line(gerroldVillager, "Gerrold", "Thank you. I did not know it would be — easier — when said aloud.");
            yield return Line(gerroldVillager, "Gerrold", "It is still heavy. It is no longer alone.");
        }

        private void OnCleanseFinished(MiniGameBase _)
        {
            if (!_cleanseStarted) return;
            _cleanseStarted = false;
            StartCoroutine(PostCleanseFlow());
        }

        private void EnsureCleanseTutorialOverlay()
        {
            if (cleanseGame == null) return;
            var existing = cleanseGame.GetComponent<MiniGameTutorialUI>();
            if (existing != null) return;
            cleanseGame.gameObject.AddComponent<MiniGameTutorialUI>();
            Hh.Log(LogCategory.Mission,
                "Mission02Director: spawned MiniGameTutorialUI on the cleanse game host.");
        }

        private IEnumerator PostCleanseFlow()
        {
            yield return new WaitForSeconds(0.6f);
            var outcome = cleanseGame != null ? cleanseGame.ComputedOutcome : CleanseOutcome.Acceptable;
            var vs = ServiceLocator.Get<VillageState>();
            if (vs != null) vs.cleanseQuality = OutcomeToString(outcome);

            if (_selectedChoice == MoralChoice.Erase)
            {
                if (outcome == CleanseOutcome.Perfect || outcome == CleanseOutcome.Acceptable)
                {
                    yield return Line(gerroldVillager, "Gerrold", "... Oh.");
                    yield return Line(gerroldVillager, "Gerrold", "... Aye.");
                    yield return Line(gerroldVillager, "Gerrold", "I can feel that it's lighter.");
                    yield return Line(gerroldVillager, "Gerrold", "I can still see her face. I cannot — I cannot quite see the last week.");
                    yield return Line(gerroldVillager, "Gerrold", "Thank you. I think — I think this is what I asked for.");
                    yield return Line(gerroldVillager, "Gerrold", "I am not certain it is what I needed.");
                    yield return Line(gerroldVillager, "Gerrold", "I will go inside now. I will see what the morning brings.");
                    if (vs != null)
                    {
                        vs.trustGerrold = VillageState.Adjust(vs.trustGerrold, +5);
                        vs.vow1Integrity = VillageState.Adjust(vs.vow1Integrity, -2);
                        vs.vow3Integrity = VillageState.Adjust(vs.vow3Integrity, -3);
                    }
                }
                else
                {
                    yield return Line(gerroldVillager, "Gerrold", "... Oh.");
                    yield return Line(gerroldVillager, "Gerrold", "... I can't —");
                    yield return Line(gerroldVillager, "Gerrold", "I can't see her clearly.");
                    yield return Line(gerroldVillager, "Gerrold", "That wasn't —");
                    yield return Line(gerroldVillager, "Gerrold", "That wasn't —");
                    yield return Line(gerroldVillager, "Gerrold", "It isn't your fault. It is — it is the cloth and the cold and the long winter.");
                    yield return Line(gerroldVillager, "Gerrold", "I will go home. Thank you for trying.");
                    if (vs != null)
                    {
                        vs.trustGerrold = VillageState.Adjust(vs.trustGerrold, +2);
                        vs.vow1Integrity = VillageState.Adjust(vs.vow1Integrity, -3);
                        vs.vow3Integrity = VillageState.Adjust(vs.vow3Integrity, -6);
                        vs.mission6RecoveryArcSeeded = true;
                    }
                }
            }
            else
            {
                if (outcome == CleanseOutcome.Perfect)
                {
                    yield return Line(gerroldVillager, "Gerrold", "... Aye.");
                    yield return Line(gerroldVillager, "Gerrold", "I can feel it. The blame is out of me.");
                    yield return Line(gerroldVillager, "Gerrold", "The dying is still there. But I can — I can carry it now, I think.");
                    yield return Line(gerroldVillager, "Gerrold", "Margery is — Margery is whole.");
                    yield return Line(gerroldVillager, "Gerrold", "Thank you. I do not know what to say.");
                    yield return Line(gerroldVillager, "Gerrold", "I will go in and read for a bit.");
                    yield return Line(gerroldVillager, "Gerrold", "... Come back when you can. Doris's bread is on Sundays.");
                    yield return Line(gerroldVillager, "Gerrold", "We could — We could perhaps share one.");
                    if (vs != null)
                    {
                        vs.trustGerrold = VillageState.Adjust(vs.trustGerrold, +4);
                        vs.vow3Integrity = VillageState.Adjust(vs.vow3Integrity, +5);
                        vs.vow1Integrity = VillageState.Adjust(vs.vow1Integrity, +3);
                    }
                }
                else if (outcome == CleanseOutcome.Acceptable || outcome == CleanseOutcome.Sloppy)
                {
                    yield return Line(gerroldVillager, "Gerrold", "... Aye.");
                    yield return Line(gerroldVillager, "Gerrold", "It is lighter. Not as light as I'd hoped, but —");
                    yield return Line(gerroldVillager, "Gerrold", "I do not think I should be as light as I'd hoped, perhaps.");
                    yield return Line(gerroldVillager, "Gerrold", "Margery is here. I am here.");
                    yield return Line(gerroldVillager, "Gerrold", "The long bit is — it is part of me, still, but it is no longer the only part.");
                    yield return Line(gerroldVillager, "Gerrold", "Thank you. I will see you in the village square next week.");
                    if (vs != null)
                    {
                        vs.trustGerrold = VillageState.Adjust(vs.trustGerrold, +4);
                        vs.vow3Integrity = VillageState.Adjust(vs.vow3Integrity, +3);
                    }
                }
                else
                {
                    yield return Line(gerroldVillager, "Gerrold", "... Mmm.");
                    yield return Line(gerroldVillager, "Gerrold", "I cannot quite — I cannot quite see all of her.");
                    yield return Line(gerroldVillager, "Gerrold", "It is — it is not what I asked for. But it is not your fault.");
                    yield return Line(gerroldVillager, "Gerrold", "Thank you for the try.");
                    if (vs != null)
                    {
                        vs.trustGerrold = VillageState.Adjust(vs.trustGerrold, +3);
                        vs.vow3Integrity = VillageState.Adjust(vs.vow3Integrity, -2);
                        vs.mission6RecoveryArcSeeded = true;
                    }
                }
            }

            yield return Line(gerroldVillager, "Gerrold", "Aye. Thank you.");
            yield return Line(gerroldVillager, "Gerrold", "Margery folded that for me. I'd been losing it for weeks.");
            yield return Line(gerroldVillager, "Gerrold", "... Take care of yourself, keeper.");

            if (vs != null) vs.firstMoralChoiceMade = true;

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
            string summary;
            bool damaged = (_selectedChoice == MoralChoice.Erase) || (outcome == CleanseOutcome.CrossedCore);
            if (damaged)
            {
                summary =
                    "Day 2 — Spire-Month, Week 1\n\n" +
                    "Gerrold Pell brought a memory. You took it. The work did not go as planned.\n\n" +
                    "He thanked you. He is going home.\n\n" +
                    "His wife's face, in his memory, is no longer entirely her own.";
            }
            else if (outcome == CleanseOutcome.Perfect)
            {
                summary =
                    "Day 2 — Spire-Month, Week 1\n\n" +
                    "Gerrold Pell, the widower, came to the Hollow at half past seven.\n" +
                    "He brought a memory wrapped in his wife's handkerchief.\n" +
                    "You took it. You did the work. The work was clean.\n\n" +
                    "You walked to his cottage. You sat in his chair. You returned the handkerchief.\n\n" +
                    "Margery Pell, who died last winter, is now a clearer memory than she was this morning.\n" +
                    "The man who carried her will sleep tonight.";
            }
            else
            {
                summary =
                    "Day 2 — Spire-Month, Week 1\n\n" +
                    "Gerrold Pell, the widower, came to the Hollow at half past seven.\n" +
                    "He brought a memory wrapped in his wife's handkerchief.\n" +
                    "You took it. You worked carefully.\n\n" +
                    "You walked to his cottage. The fire was warm. You returned the handkerchief.\n\n" +
                    "Margery Pell is gentler in his memory than she was this morning.\n" +
                    "The long bit is no longer the whole of it.";
            }
            if (eveningLedger != null) eveningLedger.Show(summary, titles);
            _ledgerShown = true;
        }

        private void OpenLedgerListen()
        {
            var vs = ServiceLocator.Get<VillageState>();
            var titles = new List<string>();
            if (vs != null) foreach (var id in vs.heldMemoryIds) titles.Add(id);
            string summary =
                "Day 2 — Spire-Month, Week 1\n\n" +
                "Gerrold Pell brought a memory. You declined to take it.\n\n" +
                "You sat in his cottage for three hours while he spoke.\n" +
                "He told you everything.\n\n" +
                "He still has the memory.\n" +
                "He is no longer alone with it.";
            if (eveningLedger != null) eveningLedger.Show(summary, titles);
            _ledgerShown = true;

            if (vs != null)
            {
                vs.firstMoralChoiceMade = true;
                if (dreamSequencer != null) dreamSequencer.PlayDream2(MoralChoice.Listen, CleanseOutcome.Perfect);
            }
        }

        private void OpenLedgerDefer()
        {
            var vs = ServiceLocator.Get<VillageState>();
            var titles = new List<string>();
            if (vs != null) foreach (var id in vs.heldMemoryIds) titles.Add(id);
            string summary =
                "Day 2 — Spire-Month, Week 1\n\n" +
                "Gerrold Pell brought a memory. You asked him to come back tomorrow.\n\n" +
                "He thanked you for the consideration.\n" +
                "He took the cloth home.\n\n" +
                "The orb is in his cottage tonight.\n" +
                "The choice is still open.";
            if (eveningLedger != null) eveningLedger.Show(summary, titles);
            _ledgerShown = true;

            if (vs != null && dreamSequencer != null)
                dreamSequencer.PlayDream2(MoralChoice.Defer, CleanseOutcome.Perfect);
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
            EventBus.Publish(new MissionCompletedEvent(null, "Mission02",
                $"Day {ServiceLocator.Get<VillageState>()?.currentDayIndex ?? 0} — Gerrold's choice."));
            gm.LoadScene(sceneAfterEndOfDay);
        }

        // ───── Helpers ────────────────────────────────────────────

        private static string ChoiceToString(MoralChoice c) => c switch
        {
            MoralChoice.Erase   => "erase",
            MoralChoice.Cleanse => "cleanse",
            MoralChoice.Listen  => "listen",
            MoralChoice.Defer   => "defer",
            _                   => "",
        };

        private static string OutcomeToString(CleanseOutcome o) => o switch
        {
            CleanseOutcome.Perfect      => "Perfect",
            CleanseOutcome.Acceptable   => "Acceptable",
            CleanseOutcome.Sloppy       => "Sloppy",
            CleanseOutcome.CrossedCore  => "CrossedCore",
            _                           => "",
        };

        private void LockPlayer(bool locked)
        {
            if (playerController != null) playerController.MovementLocked = locked;
        }

        private IEnumerator Line(VillagerSO villager, string speakerName, string lineText, string lineId = null)
        {
            if (dialogueUI == null) yield break;
            Sprite portrait = villager != null ? villager.portraitNeutral : null;
            // Phase 60 — Pass lineId through so DialogueUI's locale-aware
            // translation lookup + VoicePlayer clip resolution both work.
            // When lineId is null (legacy callers), DialogueUI falls back
            // to the English source text + no voice clip — zero regression.
            dialogueUI.PresentLine(speakerName, lineText, portrait, lineId);
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
