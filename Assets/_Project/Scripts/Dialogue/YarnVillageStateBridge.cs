// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Dialogue / YarnVillageStateBridge
//
// Bridges Yarn Spinner ↔ VillageState. Only compiles its full body when the
// `com.yarnspinner.unity` package is present (define `YARN_SPINNER_PRESENT`
// is set automatically by the asmdef's versionDefines). Until then we keep a
// stub that explains how to install Yarn.

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
            runner.VariableStorage.SetValue("$trust_doris", vs.trustDoris);
            runner.VariableStorage.SetValue("$trust_gerrold", vs.trustGerrold);
            runner.VariableStorage.SetValue("$memory_integrity_gerrold", vs.memoryIntegrityGerrold);
            runner.VariableStorage.SetValue("$gentle_mode", vs.gentleModeEnabled);
            runner.VariableStorage.SetValue("$day_index", vs.currentDayIndex);
            runner.VariableStorage.SetValue("$coin", vs.coin);
        }

        public void SyncVariablesBackToState()
        {
            var vs = ServiceLocator.Get<VillageState>();
            if (vs == null || runner == null) return;
            if (runner.VariableStorage.TryGetValue("$trust_doris", out float td)) vs.trustDoris = Mathf.Clamp(Mathf.RoundToInt(td), 0, 100);
            if (runner.VariableStorage.TryGetValue("$trust_gerrold", out float tg)) vs.trustGerrold = Mathf.Clamp(Mathf.RoundToInt(tg), 0, 100);
            if (runner.VariableStorage.TryGetValue("$memory_integrity_gerrold", out float ig)) vs.memoryIntegrityGerrold = Mathf.Clamp(Mathf.RoundToInt(ig), 0, 100);
            if (runner.VariableStorage.TryGetValue("$coin", out float c)) vs.coin = Mathf.Max(0, Mathf.RoundToInt(c));
        }

        private void RegisterCommands()
        {
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
        }

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
                case "vow7": vs.vow7Integrity = VillageState.Adjust(vs.vow7Integrity, delta); break;
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
}
