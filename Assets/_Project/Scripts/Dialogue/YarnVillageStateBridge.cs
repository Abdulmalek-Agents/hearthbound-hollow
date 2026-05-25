// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Dialogue / YarnVillageStateBridge
//
// Bridges Yarn Spinner ↔ VillageState. Only compiles its full body when the
// `com.yarnspinner.unity` package is present (define `YARN_SPINNER_PRESENT`
// is set automatically by the asmdef's versionDefines). Until then we keep a
// stub that explains how to install Yarn.
//
// ── Playtest pass fix (commit 2/6) ──────────────────────────────
// QA simulated-playthrough audit found ~20 Yarn variables and ~14 custom
// commands referenced by the expanded Yarn files (Doris_M1, Gerrold_M2,
// Pickle, Codex, EveningLedger, ChoiceCards, Dreams) were not registered
// here. Result: every <<set $foo = bar>> wrote to nothing, every $foo
// read returned the default, and every <<custom_command>> would throw
// a Yarn runtime error.
//
// Added all missing registrations below. The VillageState field set
// matches what fix commit 1/6 added.

using UnityEngine;
using HearthboundHollow.Core;

namespace HearthboundHollow.Dialogue
{
#if YARN_SPINNER_PRESENT
    public class YarnVillageStateBridge : MonoBehaviour
    {
        [Tooltip("Drag your Yarn.Unity.DialogueRunner here.")]
        public Yarn.Unity.DialogueRunner runner;

        private void Awake()
        {
            if (runner == null) runner = GetComponent<Yarn.Unity.DialogueRunner>();
            if (runner == null) { Hh.Err(LogCategory.Dialogue, "YarnVillageStateBridge: no DialogueRunner on this object."); return; }
            RegisterVariables();
            RegisterCommands();
        }

        private void RegisterVariables()
        {
            var vs = ServiceLocator.Get<VillageState>();
            if (vs == null) { Hh.Warn(LogCategory.Dialogue, "VillageState not registered yet — Yarn variables will return defaults."); return; }

            // ── Trust + memory integrity (existing) ─────────────────────
            runner.VariableStorage.SetValue("$trust_doris", vs.trustDoris);
            runner.VariableStorage.SetValue("$trust_gerrold", vs.trustGerrold);
            runner.VariableStorage.SetValue("$memory_integrity_gerrold", vs.memoryIntegrityGerrold);
            runner.VariableStorage.SetValue("$memory_integrity_doris", vs.memoryIntegrityDoris);
            runner.VariableStorage.SetValue("$gentle_mode", vs.gentleModeEnabled);
            runner.VariableStorage.SetValue("$day_index", vs.currentDayIndex);
            runner.VariableStorage.SetValue("$coin", vs.coin);

            // ── Vows ────────────────────────────────────────────────────
            runner.VariableStorage.SetValue("$vow_1_consent", vs.vow1Integrity);
            runner.VariableStorage.SetValue("$vow_3_whole", vs.vow3Integrity);
            runner.VariableStorage.SetValue("$vow_5_honest_coin", vs.vow5Integrity);
            runner.VariableStorage.SetValue("$vow_7_last_light", vs.vow7Integrity);

            // ── Pickle (playtest pass commit 2/6) ───────────────────────
            runner.VariableStorage.SetValue("$pickle_approval", vs.pickleApproval);
            runner.VariableStorage.SetValue("$pickle_sass_intensity", vs.pickleSassIntensity);

            // ── Confession Booth currency ───────────────────────────────
            runner.VariableStorage.SetValue("$cinder", vs.cinder);

            // ── M1 dialogue flags ───────────────────────────────────────
            runner.VariableStorage.SetValue("$asked_about_predecessor", vs.askedAboutPredecessor);
            runner.VariableStorage.SetValue("$refused_doris_orb", vs.refusedDorisOrb);
            runner.VariableStorage.SetValue("$doris_owes_player", vs.dorisOwesPlayer);
            runner.VariableStorage.SetValue("$polish_quality", vs.polishQuality ?? string.Empty);

            // ── M2 dialogue gates ───────────────────────────────────────
            runner.VariableStorage.SetValue("$met_doris", vs.metDoris);
            runner.VariableStorage.SetValue("$met_gerrold", vs.metGerrold);
            runner.VariableStorage.SetValue("$offered_gerrold_tea", vs.offeredGerroldTea);
            runner.VariableStorage.SetValue("$tea_brewed", vs.teaBrewed ?? string.Empty);
            runner.VariableStorage.SetValue("$walked_to_gerrold_house", vs.walkedToGerroldHouse);
            runner.VariableStorage.SetValue("$worked_at_hollow", vs.workedAtHollow);
            runner.VariableStorage.SetValue("$worked_alone", vs.workedAlone);
            runner.VariableStorage.SetValue("$sat_in_gerrold_chair", vs.satInGerroldChair);
            runner.VariableStorage.SetValue("$sat_in_margery_chair", vs.satInMargeryChair);
            runner.VariableStorage.SetValue("$deferred_gerrold", vs.deferredGerrold);

            // ── M2 moral-choice outcome ─────────────────────────────────
            runner.VariableStorage.SetValue("$gerrold_choice", vs.gerroldChoice ?? string.Empty);
            runner.VariableStorage.SetValue("$cleanse_quality", vs.cleanseQuality ?? string.Empty);
            runner.VariableStorage.SetValue("$first_moral_choice_made", vs.firstMoralChoiceMade);
            runner.VariableStorage.SetValue("$gerrold_returns_day_3", vs.gerroldReturnsDay3);
            runner.VariableStorage.SetValue("$mission6_recovery_arc_seeded", vs.mission6RecoveryArcSeeded);

            // ── Predecessor trail (M3+ unlock fuel) ─────────────────────
            runner.VariableStorage.SetValue("$predecessor_trail_warmth", vs.predecessorTrailWarmth);
        }

        public void SyncVariablesBackToState()
        {
            var vs = ServiceLocator.Get<VillageState>();
            if (vs == null || runner == null) return;

            // Numeric writes from Yarn → VillageState. Yarn float values are
            // rounded + clamped per field type.
            if (runner.VariableStorage.TryGetValue("$trust_doris", out float td)) vs.trustDoris = Mathf.Clamp(Mathf.RoundToInt(td), 0, 100);
            if (runner.VariableStorage.TryGetValue("$trust_gerrold", out float tg)) vs.trustGerrold = Mathf.Clamp(Mathf.RoundToInt(tg), 0, 100);
            if (runner.VariableStorage.TryGetValue("$memory_integrity_gerrold", out float ig)) vs.memoryIntegrityGerrold = Mathf.Clamp(Mathf.RoundToInt(ig), 0, 100);
            if (runner.VariableStorage.TryGetValue("$memory_integrity_doris", out float id_)) vs.memoryIntegrityDoris = Mathf.Clamp(Mathf.RoundToInt(id_), 0, 100);
            if (runner.VariableStorage.TryGetValue("$coin", out float c)) vs.coin = Mathf.Max(0, Mathf.RoundToInt(c));

            // Vows
            if (runner.VariableStorage.TryGetValue("$vow_1_consent", out float v1)) vs.vow1Integrity = Mathf.Clamp(Mathf.RoundToInt(v1), 0, 100);
            if (runner.VariableStorage.TryGetValue("$vow_3_whole", out float v3)) vs.vow3Integrity = Mathf.Clamp(Mathf.RoundToInt(v3), 0, 100);
            if (runner.VariableStorage.TryGetValue("$vow_5_honest_coin", out float v5)) vs.vow5Integrity = Mathf.Clamp(Mathf.RoundToInt(v5), 0, 100);
            if (runner.VariableStorage.TryGetValue("$vow_7_last_light", out float v7)) vs.vow7Integrity = Mathf.Clamp(Mathf.RoundToInt(v7), 0, 100);

            // Pickle (added playtest pass)
            if (runner.VariableStorage.TryGetValue("$pickle_approval", out float pa)) vs.pickleApproval = Mathf.Clamp(Mathf.RoundToInt(pa), 0, 100);
            if (runner.VariableStorage.TryGetValue("$pickle_sass_intensity", out float ps)) vs.pickleSassIntensity = Mathf.Clamp(Mathf.RoundToInt(ps), 1, 5);

            // Cinder
            if (runner.VariableStorage.TryGetValue("$cinder", out float cn)) vs.cinder = Mathf.Max(0, Mathf.RoundToInt(cn));

            // M1 dialogue flags
            if (runner.VariableStorage.TryGetValue("$asked_about_predecessor", out bool aap)) vs.askedAboutPredecessor = aap;
            if (runner.VariableStorage.TryGetValue("$refused_doris_orb", out bool rdo)) vs.refusedDorisOrb = rdo;
            if (runner.VariableStorage.TryGetValue("$doris_owes_player", out float dop)) vs.dorisOwesPlayer = Mathf.RoundToInt(dop);
            if (runner.VariableStorage.TryGetValue("$polish_quality", out string pq)) vs.polishQuality = pq ?? string.Empty;

            // M2 dialogue gates
            if (runner.VariableStorage.TryGetValue("$met_doris", out bool md)) vs.metDoris = md;
            if (runner.VariableStorage.TryGetValue("$met_gerrold", out bool mg)) vs.metGerrold = mg;
            if (runner.VariableStorage.TryGetValue("$offered_gerrold_tea", out bool ogt)) vs.offeredGerroldTea = ogt;
            if (runner.VariableStorage.TryGetValue("$tea_brewed", out string tb)) vs.teaBrewed = tb ?? string.Empty;
            if (runner.VariableStorage.TryGetValue("$walked_to_gerrold_house", out bool wgh)) vs.walkedToGerroldHouse = wgh;
            if (runner.VariableStorage.TryGetValue("$worked_at_hollow", out bool wah)) vs.workedAtHollow = wah;
            if (runner.VariableStorage.TryGetValue("$worked_alone", out bool wa)) vs.workedAlone = wa;
            if (runner.VariableStorage.TryGetValue("$sat_in_gerrold_chair", out bool sigc)) vs.satInGerroldChair = sigc;
            if (runner.VariableStorage.TryGetValue("$sat_in_margery_chair", out bool simc)) vs.satInMargeryChair = simc;
            if (runner.VariableStorage.TryGetValue("$deferred_gerrold", out bool dg)) vs.deferredGerrold = dg;

            // M2 moral-choice outcome
            if (runner.VariableStorage.TryGetValue("$gerrold_choice", out string gc)) vs.gerroldChoice = gc ?? string.Empty;
            if (runner.VariableStorage.TryGetValue("$cleanse_quality", out string cq)) vs.cleanseQuality = cq ?? string.Empty;
            if (runner.VariableStorage.TryGetValue("$first_moral_choice_made", out bool fmcm)) vs.firstMoralChoiceMade = fmcm;
            if (runner.VariableStorage.TryGetValue("$gerrold_returns_day_3", out bool grd3)) vs.gerroldReturnsDay3 = grd3;
            if (runner.VariableStorage.TryGetValue("$mission6_recovery_arc_seeded", out bool m6r)) vs.mission6RecoveryArcSeeded = m6r;

            // Predecessor trail
            if (runner.VariableStorage.TryGetValue("$predecessor_trail_warmth", out float ptw)) vs.predecessorTrailWarmth = Mathf.Clamp(Mathf.RoundToInt(ptw), 0, 100);
        }

        private void RegisterCommands()
        {
            // ── Existing commands ───────────────────────────────────────
            runner.AddCommandHandler<string, int>("adjust_trust", AdjustTrust);
            runner.AddCommandHandler<int>("adjust_coin", AdjustCoin);
            runner.AddCommandHandler<string, int>("adjust_vow", AdjustVow);
            runner.AddCommandHandler<string, int>("adjust_memory_integrity", AdjustMemoryIntegrity);
            runner.AddCommandHandler("dialogue_end", DialogueEnd);
            runner.AddCommandHandler("offer_polish", OfferPolish);
            runner.AddCommandHandler("offer_tea_brewing", OfferTeaBrewing);
            runner.AddCommandHandler<string>("offer_choice", OfferChoice);
            runner.AddCommandHandler<string>("eyes_look_at", EyesLookAt);
            runner.AddCommandHandler<string>("pickle_say", PickleSay);
            runner.AddCommandHandler("pickle_flick_tail", PickleFlickTail);
            runner.AddCommandHandler<float>("lights_dim", LightsDim);
            runner.AddCommandHandler("lights_warm", LightsWarm);
            runner.AddCommandHandler("save_autopoint", SaveAutopoint);
            runner.AddCommandHandler<string, string>("echo_reveal", EchoReveal);
            runner.AddCommandHandler<string>("play_cutscene", PlayCutscene);

            // ── Playtest pass commit 2/6 — newly added commands ─────────
            runner.AddCommandHandler<int>("adjust_pickle_approval", AdjustPickleApproval);
            runner.AddCommandHandler<int>("adjust_cinder", AdjustCinder);
            runner.AddCommandHandler<int>("adjust_predecessor_trail", AdjustPredecessorTrail);
            runner.AddCommandHandler<string>("show_orb", ShowOrb);
            runner.AddCommandHandler<string, string>("show_orb_in_cloth", ShowOrbInCloth);
            runner.AddCommandHandler<string>("give_player_orb", GivePlayerOrb);
            runner.AddCommandHandler<string>("player_picks_up_orb_from_cloth", PlayerPicksUpOrbFromCloth);
            runner.AddCommandHandler("orb_appears_cracked", OrbAppearsCracked);
            runner.AddCommandHandler<string, string, string>("echo_web_activate", EchoWebActivate);
            runner.AddCommandHandler<string>("show_heavy_theme_card", ShowHeavyThemeCard);
            runner.AddCommandHandler("show_choice_card_with_orb_in_hand", ShowChoiceCardWithOrbInHand);
            runner.AddCommandHandler<string>("set_cleanse_profile", SetCleanseProfile);
            runner.AddCommandHandler<string>("start_cleanse_minigame", StartCleanseMinigame);
            runner.AddCommandHandler("unlock_hollow_door", UnlockHollowDoor);
            runner.AddCommandHandler<string, string>("play_animation", PlayAnimation);
            runner.AddCommandHandler<string>("play_sfx", PlaySfx);
            runner.AddCommandHandler<string, float>("play_sfx_loop", PlaySfxLoop);
            runner.AddCommandHandler("jump_to_mission2_outro", JumpToMission2Outro);

            // Quietly accept stop-sfx commands by ignoring (no-op handler).
            runner.AddCommandHandler<string>("stop_sfx_loop", _ => { });
        }

        // ───── Command handlers (existing) ──────────────────────────────

        private void AdjustTrust(string id, int delta)
        {
            var vs = ServiceLocator.Get<VillageState>();
            if (vs == null) return;
            if (id == "doris") vs.trustDoris = VillageState.Adjust(vs.trustDoris, delta);
            else if (id == "gerrold") vs.trustGerrold = VillageState.Adjust(vs.trustGerrold, delta);
            Hh.Log(LogCategory.Dialogue, $"[Yarn] adjust_trust {id} {delta:+#;-#;0}");
        }

        private void AdjustCoin(int delta)
        {
            var vs = ServiceLocator.Get<VillageState>();
            if (vs != null) vs.coin = Mathf.Max(0, vs.coin + delta);
        }

        private void AdjustVow(string vowId, int delta)
        {
            var vs = ServiceLocator.Get<VillageState>();
            if (vs == null) return;
            switch (vowId)
            {
                case "vow1": vs.vow1Integrity = VillageState.Adjust(vs.vow1Integrity, delta); break;
                case "vow3": vs.vow3Integrity = VillageState.Adjust(vs.vow3Integrity, delta); break;
                case "vow5": vs.vow5Integrity = VillageState.Adjust(vs.vow5Integrity, delta); break;
                case "vow7": vs.vow7Integrity = VillageState.Adjust(vs.vow7Integrity, delta); break;
                case "vow2": vs.vow2Integrity = VillageState.Adjust(vs.vow2Integrity, delta); break;
                case "vow4": vs.vow4Integrity = VillageState.Adjust(vs.vow4Integrity, delta); break;
                case "vow6": vs.vow6Integrity = VillageState.Adjust(vs.vow6Integrity, delta); break;
            }
        }

        private void AdjustMemoryIntegrity(string villagerId, int delta)
        {
            var vs = ServiceLocator.Get<VillageState>();
            if (vs == null) return;
            if (villagerId == "doris") vs.memoryIntegrityDoris = VillageState.Adjust(vs.memoryIntegrityDoris, delta);
            else if (villagerId == "gerrold") vs.memoryIntegrityGerrold = VillageState.Adjust(vs.memoryIntegrityGerrold, delta);
        }

        private void DialogueEnd()
        {
            SyncVariablesBackToState();
            EventBus.Publish(new DialogueEndedEvent(null));
        }

        private void OfferPolish() => Hh.Log(LogCategory.Dialogue, "[Yarn] offer_polish");
        private void OfferTeaBrewing() => Hh.Log(LogCategory.Dialogue, "[Yarn] offer_tea_brewing");
        private void OfferChoice(string memoryId) => Hh.Log(LogCategory.Dialogue, $"[Yarn] offer_choice {memoryId}");
        private void EyesLookAt(string target) => Hh.Log(LogCategory.Dialogue, $"[Yarn] eyes_look_at {target}");
        private void PickleSay(string line) => Hh.Log(LogCategory.Pickle, $"[Yarn] pickle: {line}");
        private void PickleFlickTail() => Hh.Log(LogCategory.Pickle, "[Yarn] pickle_flick_tail");
        private void LightsDim(float intensity) => Hh.Log(LogCategory.Dialogue, $"[Yarn] lights_dim {intensity}");
        private void LightsWarm() => Hh.Log(LogCategory.Dialogue, "[Yarn] lights_warm");
        private void SaveAutopoint()
        {
            var vs = ServiceLocator.Get<VillageState>();
            var save = ServiceLocator.Get<HearthboundHollow.Save.SaveService>();
            if (vs != null && save != null) save.Save(-1, vs);
        }
        private void EchoReveal(string idA, string idB)
        {
            var vs = ServiceLocator.Get<VillageState>();
            if (vs == null) return;
            string echoKey = $"{idA}<->{idB}";
            if (!vs.revealedEchoConnectionIds.Contains(echoKey)) vs.revealedEchoConnectionIds.Add(echoKey);
            Hh.Log(LogCategory.Dialogue, $"[Yarn] echo_reveal {idA} <-> {idB}");
        }
        private void PlayCutscene(string id) => Hh.Log(LogCategory.Cutscene, $"[Yarn] play_cutscene {id}");

        // ───── Command handlers (playtest pass commit 2/6 — NEW) ────────

        private void AdjustPickleApproval(int delta)
        {
            var vs = ServiceLocator.Get<VillageState>();
            if (vs == null) return;
            vs.pickleApproval = Mathf.Clamp(vs.pickleApproval + delta, 0, 100);
            // Also push back into Yarn so subsequent <<if $pickle_approval >= 50>> sees the new value.
            runner.VariableStorage.SetValue("$pickle_approval", vs.pickleApproval);
            Hh.Log(LogCategory.Pickle, $"[Yarn] adjust_pickle_approval {delta:+#;-#;0} → {vs.pickleApproval}");
        }

        private void AdjustCinder(int delta)
        {
            var vs = ServiceLocator.Get<VillageState>();
            if (vs == null) return;
            vs.cinder = Mathf.Max(0, vs.cinder + delta);
            runner.VariableStorage.SetValue("$cinder", vs.cinder);
            Hh.Log(LogCategory.Dialogue, $"[Yarn] adjust_cinder {delta:+#;-#;0} → {vs.cinder}");
        }

        private void AdjustPredecessorTrail(int delta)
        {
            var vs = ServiceLocator.Get<VillageState>();
            if (vs == null) return;
            vs.predecessorTrailWarmth = Mathf.Clamp(vs.predecessorTrailWarmth + delta, 0, 100);
            runner.VariableStorage.SetValue("$predecessor_trail_warmth", vs.predecessorTrailWarmth);
            Hh.Log(LogCategory.Dialogue, $"[Yarn] adjust_predecessor_trail {delta:+#;-#;0} → {vs.predecessorTrailWarmth}");
        }

        private void ShowOrb(string orbId) => EventBus.Publish(new YarnShowOrbEvent(orbId, inCloth: false));
        private void ShowOrbInCloth(string orbId, string _) => EventBus.Publish(new YarnShowOrbEvent(orbId, inCloth: true));
        private void GivePlayerOrb(string orbId) => EventBus.Publish(new YarnGivePlayerOrbEvent(orbId));
        private void PlayerPicksUpOrbFromCloth(string orbId) => EventBus.Publish(new YarnGivePlayerOrbEvent(orbId, fromCloth: true));
        private void OrbAppearsCracked() => EventBus.Publish(new YarnOrbAppearsCrackedEvent());

        private void EchoWebActivate(string idA, string idB, string echoName)
        {
            var vs = ServiceLocator.Get<VillageState>();
            if (vs != null)
            {
                string echoKey = $"{idA}<->{idB}";
                if (!vs.revealedEchoConnectionIds.Contains(echoKey))
                    vs.revealedEchoConnectionIds.Add(echoKey);
            }
            EventBus.Publish(new YarnEchoWebActivateEvent(idA, idB, echoName));
            Hh.Log(LogCategory.Dialogue, $"[Yarn] echo_web_activate {idA} ↔ {idB} ('{echoName}')");
        }

        private void ShowHeavyThemeCard(string warningTags) => EventBus.Publish(new YarnHeavyThemeCardEvent(warningTags));
        private void ShowChoiceCardWithOrbInHand() => EventBus.Publish(new YarnShowChoiceCardEvent());
        private void SetCleanseProfile(string profile) => EventBus.Publish(new YarnCleanseProfileEvent(profile));
        private void StartCleanseMinigame(string orbId) => EventBus.Publish(new YarnStartCleanseEvent(orbId));
        private void UnlockHollowDoor() => EventBus.Publish(new YarnUnlockHollowDoorEvent());
        private void PlayAnimation(string subjectId, string animationId) => EventBus.Publish(new YarnPlayAnimationEvent(subjectId, animationId));
        private void PlaySfx(string sfxId) => EventBus.Publish(new YarnPlaySfxEvent(sfxId, loop: false, volume: 1f));
        private void PlaySfxLoop(string sfxId, float volume) => EventBus.Publish(new YarnPlaySfxEvent(sfxId, loop: true, volume: volume));
        private void JumpToMission2Outro() => EventBus.Publish(new YarnJumpToMission2OutroEvent());
    }
#else
    /// <summary>
    /// Stub bridge — present so the asmdef compiles. Yarn Spinner not yet installed.
    /// Install: Window → Package Manager → + → Add package from git URL
    ///   https://github.com/YarnSpinnerTool/YarnSpinner-Unity.git
    /// Once installed, the symbol YARN_SPINNER_PRESENT will be defined and the
    /// full bridge body above compiles.
    /// </summary>
    public class YarnVillageStateBridge : MonoBehaviour
    {
        private void Awake()
        {
            Hh.Warn(LogCategory.Dialogue,
                "YarnVillageStateBridge: Yarn Spinner package not installed. Install via Package Manager " +
                "with git URL: https://github.com/YarnSpinnerTool/YarnSpinner-Unity.git");
        }
    }
#endif

    // ───── Yarn → world events (always compiled, even without Yarn) ─────
    // These are pumped by the Yarn bridge's command handlers and consumed by
    // mission directors / orb interactables / UI overlays. Defining them here
    // (outside the YARN_SPINNER_PRESENT block) keeps the directors compilable
    // regardless of whether Yarn is installed yet.

    public readonly struct YarnShowOrbEvent
    {
        public readonly string orbId;
        public readonly bool inCloth;
        public YarnShowOrbEvent(string orbId, bool inCloth) { this.orbId = orbId; this.inCloth = inCloth; }
    }

    public readonly struct YarnGivePlayerOrbEvent
    {
        public readonly string orbId;
        public readonly bool fromCloth;
        public YarnGivePlayerOrbEvent(string orbId, bool fromCloth = false) { this.orbId = orbId; this.fromCloth = fromCloth; }
    }

    public readonly struct YarnOrbAppearsCrackedEvent { }

    public readonly struct YarnEchoWebActivateEvent
    {
        public readonly string idA;
        public readonly string idB;
        public readonly string echoName;
        public YarnEchoWebActivateEvent(string idA, string idB, string echoName) { this.idA = idA; this.idB = idB; this.echoName = echoName; }
    }

    public readonly struct YarnHeavyThemeCardEvent
    {
        public readonly string warningTags;
        public YarnHeavyThemeCardEvent(string warningTags) { this.warningTags = warningTags; }
    }

    public readonly struct YarnShowChoiceCardEvent { }

    public readonly struct YarnCleanseProfileEvent
    {
        public readonly string profile;
        public YarnCleanseProfileEvent(string profile) { this.profile = profile; }
    }

    public readonly struct YarnStartCleanseEvent
    {
        public readonly string orbId;
        public YarnStartCleanseEvent(string orbId) { this.orbId = orbId; }
    }

    public readonly struct YarnUnlockHollowDoorEvent { }

    public readonly struct YarnPlayAnimationEvent
    {
        public readonly string subjectId;
        public readonly string animationId;
        public YarnPlayAnimationEvent(string subjectId, string animationId) { this.subjectId = subjectId; this.animationId = animationId; }
    }

    public readonly struct YarnPlaySfxEvent
    {
        public readonly string sfxId;
        public readonly bool loop;
        public readonly float volume;
        public YarnPlaySfxEvent(string sfxId, bool loop, float volume) { this.sfxId = sfxId; this.loop = loop; this.volume = volume; }
    }

    public readonly struct YarnJumpToMission2OutroEvent { }
}
