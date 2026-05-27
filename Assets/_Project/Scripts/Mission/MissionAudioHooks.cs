// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / MissionAudioHooks
//
// Runtime EventBus → audio router. Listens for the existing narrative
// events (MemoryPolishedEvent, MemoryCleansedEvent, MoralChoiceMadeEvent,
// HerbHarvestedEvent, TeaBrewedEvent, MissionStartedEvent,
// MissionCompletedEvent, DayEndedEvent) and translates each into a
// matching MusicPlayer.Play() / SfxPlayer.PlayOneShot() request via
// ServiceLocator.
//
// Phase 41 design rationale: instead of cross-cutting audio calls into
// every Mission01Director / Mission02Director / PolishMiniGame /
// CleanseMiniGame, we subscribe to the events the directors already
// publish. Zero changes to existing director logic; the audio layer
// becomes a *passive observer* that simply reacts. Adding a new audio
// reaction is a one-line addition here, not a change in 3 places.
//
// Self-instantiates at runtime via [RuntimeInitializeOnLoadMethod] so
// no scene wiring is required. The hook is auto-DontDestroyOnLoad and
// lives next to Phase 38's _HHAudio_Bootstrap rig.
//
// Audio routing table (canonical, Phase 41 v1):
//
//   Event                          | Action
//   -------------------------------|------------------------------------
//   MissionStartedEvent("M1")      | (music stays — scene beacon owns it)
//   MemoryPolishedEvent(c)         | SFX polish_success_jingle + hum_post
//   MemoryCleansedEvent(o)         | SFX polish_reveal_swell (Perfect)
//                                  | SFX polish_midway_chime  (Acceptable/Sloppy)
//                                  | SFX polish_rub_friction_warn (CrossedCore) + duck
//   HerbHarvestedEvent             | SFX ui_open (lightweight pluck)
//   TeaBrewedEvent                 | SFX kettle_pour + choice_select
//   MoralChoiceMadeEvent           | SFX choice_select + music duck
//   DayEndedEvent                  | SFX ui_close + slow music duck
//   MissionCompletedEvent          | SFX polish_success_jingle (soft)
//   SceneAudioRequestedEvent       | (Phase 43) persists ids into VillageState
//   VillageStateLoadedEvent        | (Phase 43) restores music from saved state
//
// Music swaps for dream cuts are handled by DreamAudioBinder (Phase 38).
// MissionAudioHooks does not duplicate that work.

using UnityEngine;
using HearthboundHollow.Audio;
using HearthboundHollow.Core;

namespace HearthboundHollow.Mission
{
    [DisallowMultipleComponent]
    public class MissionAudioHooks : MonoBehaviour
    {
        // Music ducking — how much to lower music while a heavy SFX or
        // moral choice card is on screen.
        [Header("Behaviour")]
        [Range(0f, 1f)] public float duckGlobalScale = 0.55f;
        [Range(0.05f, 3f)] public float duckRecoverySeconds = 1.5f;

        // Self-instantiation — runs once on first scene load. Idempotent
        // because RuntimeInitializeOnLoadMethod fires once per launch and
        // we check for an existing instance.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInstall()
        {
            // If a prior instance already exists (e.g. domain reload in
            // Editor), don't duplicate.
            var existing = FindFirstObjectByType<MissionAudioHooks>();
            if (existing != null) return;

            var go = new GameObject("_HHAudio_MissionHooks");
            DontDestroyOnLoad(go);
            go.AddComponent<MissionAudioHooks>();
            Hh.Log(LogCategory.Audio, "MissionAudioHooks installed.");
        }

        // ─── Subscription lifecycle ─────────────────────────────────

        private void Awake()
        {
            // Existing narrative events
            EventBus.Subscribe<MemoryPolishedEvent>(OnMemoryPolished);
            EventBus.Subscribe<MemoryCleansedEvent>(OnMemoryCleansed);
            EventBus.Subscribe<MoralChoiceMadeEvent>(OnMoralChoice);
            EventBus.Subscribe<HerbHarvestedEvent>(OnHerbHarvested);
            EventBus.Subscribe<TeaBrewedEvent>(OnTeaBrewed);
            EventBus.Subscribe<DayEndedEvent>(OnDayEnded);
            EventBus.Subscribe<MissionStartedEvent>(OnMissionStarted);
            EventBus.Subscribe<MissionCompletedEvent>(OnMissionCompleted);

            // Phase 43 — save persistence events
            EventBus.Subscribe<SceneAudioRequestedEvent>(OnSceneAudioRequested);
            EventBus.Subscribe<VillageStateLoadedEvent>(OnVillageStateLoaded);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<MemoryPolishedEvent>(OnMemoryPolished);
            EventBus.Unsubscribe<MemoryCleansedEvent>(OnMemoryCleansed);
            EventBus.Unsubscribe<MoralChoiceMadeEvent>(OnMoralChoice);
            EventBus.Unsubscribe<HerbHarvestedEvent>(OnHerbHarvested);
            EventBus.Unsubscribe<TeaBrewedEvent>(OnTeaBrewed);
            EventBus.Unsubscribe<DayEndedEvent>(OnDayEnded);
            EventBus.Unsubscribe<MissionStartedEvent>(OnMissionStarted);
            EventBus.Unsubscribe<MissionCompletedEvent>(OnMissionCompleted);

            // Phase 43
            EventBus.Unsubscribe<SceneAudioRequestedEvent>(OnSceneAudioRequested);
            EventBus.Unsubscribe<VillageStateLoadedEvent>(OnVillageStateLoaded);
        }

        // ─── Handlers ───────────────────────────────────────────────

        private void OnMemoryPolished(MemoryPolishedEvent ev)
        {
            // Doris's first orb just got polished. Play the success swell
            // and the post-polish hum to "seal" the memory.
            var sfx = ServiceLocator.Get<SfxPlayer>();
            if (sfx == null) return;
            sfx.PlayOneShot("polish_success_jingle", 0.85f);
            // The polish_hum_post is a follow-up, slight delay so it
            // overlaps the tail of the jingle.
            Invoke(nameof(PlayHumPost), 0.5f);
        }

        private void PlayHumPost()
        {
            var sfx = ServiceLocator.Get<SfxPlayer>();
            sfx?.PlayOneShot("polish_hum_post", 0.7f);
        }

        private void OnMemoryCleansed(MemoryCleansedEvent ev)
        {
            // Outcome-aware SFX cue (the raw int is a CleanseOutcome enum
            // value, but we keep the dep flat — the int maps to enum order
            // Perfect=0, Acceptable=1, Sloppy=2, CrossedCore=3).
            var sfx = ServiceLocator.Get<SfxPlayer>();
            if (sfx == null) return;
            switch (ev.OutcomeRaw)
            {
                case 0:  // Perfect
                    sfx.PlayOneShot("polish_reveal_swell", 0.85f);
                    sfx.PlayOneShot("polish_hum_post", 0.6f);
                    break;
                case 1:
                case 2:  // Acceptable / Sloppy
                    sfx.PlayOneShot("polish_midway_chime", 0.7f);
                    break;
                case 3:  // CrossedCore
                    sfx.PlayOneShot("polish_rub_friction_warn", 0.8f);
                    DuckMusic();  // emotional gut-punch
                    break;
            }
        }

        private void OnMoralChoice(MoralChoiceMadeEvent ev)
        {
            var sfx = ServiceLocator.Get<SfxPlayer>();
            sfx?.PlayOneShot("choice_select", 0.8f);
            // The choice is the heaviest narrative beat in Mission 2 — duck
            // the music to give the moment weight. Recovers automatically.
            DuckMusic();
        }

        private void OnHerbHarvested(HerbHarvestedEvent ev)
        {
            var sfx = ServiceLocator.Get<SfxPlayer>();
            sfx?.PlayOneShot("ui_open", 0.55f);
        }

        private void OnTeaBrewed(TeaBrewedEvent ev)
        {
            var sfx = ServiceLocator.Get<SfxPlayer>();
            // kettle_pour is the canonical brew finished SFX (Phase 37
            // generated it). Falls back to choice_select if not mapped.
            if (sfx == null) return;
            // Play the kettle_pour first, then a soft confirm.
            sfx.PlayOneShot("kettle_pour", 0.75f);
            Invoke(nameof(PlayBrewConfirm), 1.5f);
        }

        private void PlayBrewConfirm()
        {
            var sfx = ServiceLocator.Get<SfxPlayer>();
            sfx?.PlayOneShot("choice_select", 0.55f);
        }

        private void OnMissionStarted(MissionStartedEvent ev)
        {
            // The scene's SceneAudioBeacon already published the right music
            // id when the scene loaded. This hook is here for future use
            // (e.g. mission-specific stinger).
            Hh.Log(LogCategory.Audio, $"MissionAudioHooks: mission '{ev.MissionId}' started.");
        }

        private void OnMissionCompleted(MissionCompletedEvent ev)
        {
            var sfx = ServiceLocator.Get<SfxPlayer>();
            sfx?.PlayOneShot("polish_success_jingle", 0.55f);
        }

        private void OnDayEnded(DayEndedEvent ev)
        {
            var sfx = ServiceLocator.Get<SfxPlayer>();
            sfx?.PlayOneShot("ui_close", 0.6f);
            // Day-end music swap — soften toward the menu cue. The next
            // scene's beacon will swap to its own theme on load.
            // Phase 46 — use TryGet to avoid console-warning spam.
            if (ServiceLocator.TryGet<MusicPlayer>(out var music))
            {
                // The user could be reading the Evening Ledger for a long
                // time — let the music drift to menu warmth gradually.
                music.globalScale = duckGlobalScale;
                CancelInvoke(nameof(RecoverMusic));
                Invoke(nameof(RecoverMusic), duckRecoverySeconds * 3f);
            }
        }

        // ─── Music ducking ──────────────────────────────────────────

        private float _preDuckScale = 0.75f;
        private bool _ducked;

        private void DuckMusic()
        {
            // Phase 46 — TryGet to avoid console-warning spam.
            if (!ServiceLocator.TryGet<MusicPlayer>(out var music)) return;
            if (!_ducked)
            {
                _preDuckScale = music.globalScale;
                _ducked = true;
            }
            music.globalScale = duckGlobalScale;
            CancelInvoke(nameof(RecoverMusic));
            Invoke(nameof(RecoverMusic), duckRecoverySeconds);
        }

        private void RecoverMusic()
        {
            if (!ServiceLocator.TryGet<MusicPlayer>(out var music)) return;
            music.globalScale = _preDuckScale;
            _ducked = false;
        }

        // ─── Phase 43 — save persistence ────────────────────────────

        /// <summary>
        /// Every time a scene asks for new background audio, snapshot the
        /// ids into the live VillageState so the next save captures them.
        /// </summary>
        private void OnSceneAudioRequested(SceneAudioRequestedEvent ev)
        {
            // Phase 46 — TryGet to avoid console-warning spam.
            if (!ServiceLocator.TryGet<VillageState>(out var vs)) return;
            if (!string.IsNullOrEmpty(ev.MusicId)) vs.lastMusicId = ev.MusicId;
            if (!string.IsNullOrEmpty(ev.AmbienceId)) vs.lastAmbienceId = ev.AmbienceId;
        }

        /// <summary>
        /// On save load, replay the music + ambience the player last had on.
        /// SceneAudioBeacon's per-scene Start() may override this within
        /// 0.20 s — but for the rare case where the save resumed on a scene
        /// without a beacon, the audio still continues.
        ///
        /// Phase 46 — use `TryGet` instead of `Get` so we don't spam the
        /// console with "no service registered" warnings during boot-time
        /// state restoration (the MusicPlayer may not have Awake'd yet when
        /// VillageStateLoadedEvent fires from GameManager.Awake). If the
        /// service isn't ready yet, defer to the first scene-loaded callback.
        /// </summary>
        private void OnVillageStateLoaded(VillageStateLoadedEvent ev)
        {
            var vs = ev.VillageState as VillageState;
            if (vs == null) return;
            if (string.IsNullOrEmpty(vs.lastMusicId)) return;

            if (!ServiceLocator.TryGet<MusicPlayer>(out var music))
            {
                // MusicPlayer not yet registered (e.g. Bootstrap rig spawns
                // after GameManager.Awake fires VillageStateLoadedEvent).
                // Defer the restore until the first scene-loaded callback
                // when MusicPlayer.Awake will have run.
                _pendingMusicRestore = vs.lastMusicId;
                UnityEngine.SceneManagement.SceneManager.sceneLoaded += DeferredMusicRestore;
                return;
            }
            music.Play(vs.lastMusicId);
            Hh.Log(LogCategory.Audio,
                $"MissionAudioHooks: restored music '{vs.lastMusicId}' from saved state.");
        }

        private string _pendingMusicRestore;
        private void DeferredMusicRestore(UnityEngine.SceneManagement.Scene s, UnityEngine.SceneManagement.LoadSceneMode m)
        {
            if (string.IsNullOrEmpty(_pendingMusicRestore)) return;
            if (!ServiceLocator.TryGet<MusicPlayer>(out var music)) return;
            music.Play(_pendingMusicRestore);
            Hh.Log(LogCategory.Audio,
                $"MissionAudioHooks: deferred restore of music '{_pendingMusicRestore}' (Phase 46 fix).");
            _pendingMusicRestore = null;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= DeferredMusicRestore;
        }
    }
}
