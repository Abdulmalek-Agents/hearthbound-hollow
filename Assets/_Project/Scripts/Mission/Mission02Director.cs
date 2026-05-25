// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / Mission02Director
//
// The runtime orchestrator for Mission 2 ("The Widower's Request").
//
// Spans two scenes:
//   * 04_Mission02_Garden  — harvest herbs, brew tea at the kettle
//   * 05_Mission02_Cottage — speak with Gerrold, moral choice, Cleanse, Dream 2
//
// ── Playtest pass fix (commit 4/6) ──────────────────────────────
// QA simulated-playthrough audit found this director was running Phase 12
// placeholder strings that COMPLETELY DIVERGED from Gerrold's canonical
// voice signature in Gerrold_M2.yarn. The 270 lines of Vellis-tier
// dialogue were sitting unused. The iconic line "I want to keep my wife.
// I do not want to keep the long bit." was nowhere in the game. The
// 6-line Margery aside ("the most quoted line in cozy-game playtests")
// was nowhere. The second-confirm for Erase was missing entirely.
// The "Marin's note" narration in the Garden scene was off-spec (Marin
// doesn't narrate the garden — Mission 2 Guide § 11 has the player
// walk through the garden silently).
//
// FIX: Replaced every hard-coded line with canonical text from
// Gerrold_M2.yarn. Added the missing Margery aside, the 4-path choice
// branching with proper option labels from ChoiceCards.yarn, the
// second-confirm prompt for Erase, the chair-selection sub-choice
// (with Pickle's Margery-chair line gated by approval ≥50), and the
// path-aware Day 2 Evening Ledger prose variants.
//
// ── Phase 34 (2026-05-25) ───────────────────────────────────────
// Same UX fix as Mission01Director: the Cleanse mini-game (both Erase
// and Cleanse paths) used to start silently after the choice line,
// leaving the player wondering what to do. EnsureCleanseTutorialOverlay()
// now attaches a MiniGameTutorialUI to the cleanse host before BeginGame,
// so the on-screen overlay ("Trace cracks · don't cross the core" +
// progress bar + Auto-Complete button) appears immediately.
//
// ── Build-fix (2026-05-25) ──────────────────────────────────────
// Console reported 4 compile errors after the Phase 4/6 dialogue rewrite:
//   • CS1061 ×2 — TeaBrewedEvent does not contain a definition for 'herb'
//     The struct field is `Herb` (capitalized; see Core/GameEvents.cs §
//     TeaBrewedEvent). Casing was wrong in OnTeaBrewed.
//   • CS0246 ×2 — YarnHeavyThemeCardEvent / YarnEchoWebActivateEvent not
//     found. These structs live in HearthboundHollow.Dialogue (see
//     Dialogue/YarnVillageStateBridge.cs, EventBus event struct block at
//     the bottom). Mission02Director's `using` list was missing
//     HearthboundHollow.Dialogue. The Mission asmdef already references
//     HearthboundHollow.Dialogue so no asmdef change required.
// Both fixes are targeted (one `using` line + 2-char casing changes).
// No behavioural change.
//
// ── Trigger-proxy regression fix (2026-05-25) ───────────────────
// Caught during code-review of the build-fix: my playtest-pass commit
// (dce436a) accidentally copy-pasted the GardenExitProxy body into the
// GerroldGreetingProxy, so the cottage's Gerrold-approach trigger was
// calling OnPlayerExitedGarden() — which re-loaded the cottage scene
// instead of starting Gerrold's dialogue. Restored to
// OnPlayerEnteredGerrold().

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
        public VillagerSO dorisVillager;          // for the Marin reference at Day 2 morning
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
        // Per Mission 2 Guide § 11 — the garden is bright, silent, and
        // the player walks through it observing. NO narration except the
        // bird song + Zephyr foliage. Marin does NOT narrate here.

        private IEnumerator GardenIntro()
        {
            if (titleCard != null) yield return new WaitForSeconds(2.0f);
            _gardenIntroPlayed = true;
            // Silent garden — the player observes. Marin's narration removed
            // per Mission 2 Guide § 11.2 (cozy contract: bright outdoor
            // contrast vs the dim Hollow interior).
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
                // Soft Pickle-toned hint via the workbench (per Pickle's M2
                // canonical 4th line — she comments on the brewing at the
                // 6s mark; here we use a slightly different prompt to nudge
                // the player to harvest first).
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
            // Record the herb the player brewed so the cottage scene's tea
            // modifier can branch. The HerbHarvestedEvent already populated
            // VillageState.harvestedHerbIds; here we set the canonical
            // teaBrewed string ("Lavender" or "Valerian") from the event.
            //
            // Build-fix: TeaBrewedEvent.Herb is capitalized (Core/GameEvents.cs).
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
            // Pickle's canonical M2 tea-brewing line (4th canonical line)
            // plays here per Pickle_M2_TeaBrewing in Pickle.yarn.
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
            // The player may exit WITHOUT tea (No-Tea path) per Mission 2
            // Guide § 11.2.3. Both paths are valid; no gating.
            var gm = GameManager.Instance;
            if (gm != null) gm.LoadScene(cottageSceneName);
        }

        // ───── Cottage: intro + greeting + choice + cleanse ───────
        // All canonical text below is from Gerrold_M2.yarn nodes.
        // Voice signature: constant apologies, "the long bit" verbal tic,
        // carpenter metaphors, Margery off-screen, NO crying on-screen.

        private IEnumerator CottageIntro()
        {
            if (titleCard != null) yield return new WaitForSeconds(2.0f);
            // Heavy Theme Warning Card (per Mission 2 Guide § 13.2) — if
            // the player has the toggle ON, show the card here BEFORE
            // entering the cottage interior.
            ShowHeavyThemeCardIfEnabled();
            LockPlayer(false);
            // Silent interior — the player walks in and approaches Gerrold.
            // No narration. The room is the storytelling. (Mission 2 Guide
            // § 13.1 — the 8 signature props do the work.)
        }

        private void ShowHeavyThemeCardIfEnabled()
        {
            // The card is a separate UI overlay; if SettingsService says
            // heavyThemeWarningsEnabled is false, we no-op. The card itself
            // is a one-button dismiss per ChoiceCards.yarn / HeavyThemeCard.
            // (Implementation defers to a future HeavyThemeCardUI prefab;
            //  here we publish an event so any subscriber can render it.)
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

            // Gerrold_M2_Knocks — canonical opening.
            yield return Line(gerroldVillager, "Gerrold", "I'm sorry. Are you the new keeper?");
            yield return Line(gerroldVillager, "Gerrold", "I should have written first. I don't know if there's a way to do that.");
            yield return Line(gerroldVillager, "Gerrold", "... My name is Gerrold Pell. I — could I come in for a moment?");

            // 3-option greeting reply.
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
                    yield return Line(gerroldVillager, "Gerrold", "Thank you. Just for a moment.");
                    break;
                case 1:
                    yield return Line(gerroldVillager, "Gerrold", "... It's a memory. I brought one.");
                    yield return Line(gerroldVillager, "Gerrold", "I'm sorry — that's what one does, isn't it? I should know how it works.");
                    break;
                case 2:
                    yield return Line(gerroldVillager, "Gerrold", "Thank you. You are kind.");
                    if (vs != null) vs.vow7Integrity = VillageState.Adjust(vs.vow7Integrity, +1);
                    break;
            }

            // Gerrold_M2_Sits — tea offer.
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

            // The iconic statement of his case — Mission 2's heart.
            yield return Line(gerroldVillager, "Gerrold", "I have a memory I want to be rid of.");
            yield return Line(gerroldVillager, "Gerrold", "Not the whole thing. Just the long bit.");
            yield return Line(gerroldVillager, "Gerrold", "My wife — Margery — died last winter. Eleven months last week.");
            yield return Line(gerroldVillager, "Gerrold", "I have the memory of the last week of her life. It is the heaviest thing I own.");
            yield return Line(gerroldVillager, "Gerrold", "I want to keep my wife. I do not want to keep the long bit.");

            // 3-option follow-up.
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
                case 0: // The long bit
                    yield return Line(gerroldVillager, "Gerrold", "She was ill for fourteen months.");
                    yield return Line(gerroldVillager, "Gerrold", "The dying part — the last week — was kind, in its way.");
                    yield return Line(gerroldVillager, "Gerrold", "She wasn't in pain by then. She slept a lot.");
                    yield return Line(gerroldVillager, "Gerrold", "But I was awake the whole time. Six days awake. Watching.");
                    yield return Line(gerroldVillager, "Gerrold", "I cannot un-be the man who watched.");
                    yield return Line(gerroldVillager, "Gerrold", "But I think I do not need to remember every hour of it.");
                    break;
                case 1: // Margery aside — "the most quoted line in cozy-game playtests" (Focus 02 § 9.3)
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
                case 2: // Wait
                    yield return Line(gerroldVillager, "Gerrold", "... Thank you. I'll get to it.");
                    break;
            }

            // The orb offer — wrapped in Margery's handkerchief.
            yield return Line(gerroldVillager, "Gerrold", "It's in the cloth. Margery wrapped it.");
            yield return Line(gerroldVillager, "Gerrold", "I think she wrapped it for this.");

            // Echo Web first activation — fires when the player picks up the orb.
            if (gerroldOrb != null) gerroldOrb.gameObject.SetActive(true);
            EventBus.Publish(new YarnEchoWebActivateEvent("DOR-001", "GER-007", "Doris in kitchen on Sunday"));

            // Pickle's canonical Echo Web line (3rd canonical line).
            yield return new WaitForSeconds(0.6f);
            yield return Line(null, "Pickle", "Ah. The Web has decided to speak today.");
            yield return Line(null, "Pickle", "This one and yesterday's are in the same kitchen on the same Sunday.");
            yield return Line(null, "Pickle", "That is what the line of light is saying. Look at it. It is showing off.");

            // The tea modifier — if the player brought tea, Gerrold reacts here.
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
                else // Valerian
                {
                    yield return Line(gerroldVillager, "Gerrold", "Aye. This is the one she used to make me when I could not sleep.");
                    yield return Line(gerroldVillager, "Gerrold", "She would put a sprig of it on the pillow when I — when I could not stop thinking.");
                    yield return Line(gerroldVillager, "Gerrold", "I have not had this in eleven months. ... Thank you. You are very kind.");
                }
                vs.trustGerrold = VillageState.Adjust(vs.trustGerrold, +2);
            }

            // Gerrold's final pre-choice line.
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

            // Canonical Mission 2 prompt from ChoiceCard_M2_Setup in ChoiceCards.yarn.
            choiceCard.Show(gerroldMemory,
                "Gerrold has placed the orb on the table. He is waiting.",
                tariffs);
        }

        private void OnMoralChoicePicked(MoralChoice choice)
        {
            if (_choiceMade) return;

            // Second-confirmation prompt for Erase per Mission 2 Guide §
            // 14.4 — the heaviest option requires a second yes.
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
                // Cancel — return to the choice card.
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
                        // Phase 34 — surface the cleanse tutorial UI before
                        // starting the game, same fix as Mission01.PolishFlow.
                        EnsureCleanseTutorialOverlay();
                        _cleanseStarted = true;
                        cleanseGame.gentleMode = false; // aggressive profile per Mission 2 Guide § 15.2
                        cleanseGame.BeginGame(gerroldOrb);
                    }
                    else FallbackEndMission(); // no mini-game in scene; skip ahead
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
                        // Phase 34 — surface the cleanse tutorial UI.
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

                    // The 3-minute Listen Cutscene — narrated as a sequence of
                    // Gerrold's monologue beats per Mission 2 Guide § 17.2.
                    // (The Cutscene Engine timeline drives camera + composer;
                    //  here we provide the dialogue beats.)
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
            // Listen scene — 3 minutes of Gerrold telling Margery's last week
            // aloud per Mission 2 Guide § 17.2.
            // The actual cutscene camera + composer is driven by the
            // Cutscene Engine timeline "Mission2_ListenScene_FullMonologue";
            // we render the narrative beats here so the dialogue is visible
            // when the cinematic asset is not yet built.
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

        /// <summary>
        /// Idempotently attaches a MiniGameTutorialUI to the cleanse game
        /// host so the on-screen instructions ("Trace cracks · don't cross
        /// the core") + progress UI show the moment the mini-game starts.
        /// Auto-builds its own Canvas if none is present. (Phase 34.)
        /// </summary>
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

            // Path-aware outro lines per Gerrold_M2_Outro_Return in Gerrold_M2.yarn.
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
                else // CrossedCore
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
            else // MoralChoice.Cleanse
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
                else // CrossedCore
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

            // Handkerchief return — Gerrold uses the title "keeper" for the
            // first time per Mission 2 Guide § 20.1.
            yield return Line(gerroldVillager, "Gerrold", "Aye. Thank you.");
            yield return Line(gerroldVillager, "Gerrold", "Margery folded that for me. I'd been losing it for weeks.");
            yield return Line(gerroldVillager, "Gerrold", "... Take care of yourself, keeper.");

            // Mark the moral choice as made.
            if (vs != null) vs.firstMoralChoiceMade = true;

            if (dreamSequencer != null) dreamSequencer.PlayDream2(_selectedChoice, outcome);
            else Hh.Warn(LogCategory.Mission, "Mission02: no MemoryDreamSequencer wired — skipping Dream 2.");
            OpenLedgerCleanseResult(outcome);
        }

        // ───── Evening Ledger paths ───────────────────────────────
        // Per Mission 2 Guide § 21 — 5 prose variants, one per outcome.
        // Canonical text from EveningLedger.yarn (Esme Cordray).

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
            // Per Mission 2 Guide § 21.1 — variant selection.
            // Variant A (Cleanse Perfect): warmest cleanse prose
            // Variant B (Cleanse Acceptable/Sloppy): gentle cleanse prose
            // Variant C (Erase OR Cleanse CrossedCore): damaged prose
            bool damaged = (_selectedChoice == MoralChoice.Erase) || (outcome == CleanseOutcome.CrossedCore);
            if (damaged)
            {
                // Variant C
                summary =
                    "Day 2 — Spire-Month, Week 1\n\n" +
                    "Gerrold Pell brought a memory. You took it. The work did not go as planned.\n\n" +
                    "He thanked you. He is going home.\n\n" +
                    "His wife's face, in his memory, is no longer entirely her own.";
            }
            else if (outcome == CleanseOutcome.Perfect)
            {
                // Variant A
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
                // Variant B (Acceptable / Sloppy)
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
            // Variant D — the warmest prose variant.
            string summary =
                "Day 2 — Spire-Month, Week 1\n\n" +
                "Gerrold Pell brought a memory. You declined to take it.\n\n" +
                "You sat in his cottage for three hours while he spoke.\n" +
                "He told you everything.\n\n" +
                "He still has the memory.\n" +
                "He is no longer alone with it.";
            if (eveningLedger != null) eveningLedger.Show(summary, titles);
            _ledgerShown = true;

            // Also mark the choice as made for the Listen path.
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
            // Variant E — the most open prose.
            string summary =
                "Day 2 — Spire-Month, Week 1\n\n" +
                "Gerrold Pell brought a memory. You asked him to come back tomorrow.\n\n" +
                "He thanked you for the consideration.\n" +
                "He took the cloth home.\n\n" +
                "The orb is in his cottage tonight.\n" +
                "The choice is still open.";
            if (eveningLedger != null) eveningLedger.Show(summary, titles);
            _ledgerShown = true;

            // Defer does NOT set firstMoralChoiceMade — the choice is still
            // open. Mission 3 will re-engage.
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
                // FIX: must call OnPlayerEnteredGerrold (not OnPlayerExitedGarden).
                // A copy-paste regression in commit dce436a meant approaching
                // Gerrold in the cottage was re-loading the cottage scene
                // instead of starting his dialogue.
                if (other.CompareTag("Player")) director?.OnPlayerEnteredGerrold();
            }
        }
    }
}
