// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / BootstrapHookDirector
//
// PHASE 48 — Runtime orchestrator for the Cold Open Cinematic.
//
// Lives on the Bootstrap scene. Runs BEFORE GameManager.autoLoadMainMenu
// kicks in. Pattern:
//
//   1. Awake — flip GameManager.autoLoadMainMenu to false so we own the
//      first scene transition. (We restore the flag for a future re-boot.)
//   2. Start — kick `ColdOpenCinematicUI.Play(state, OnChoice)`.
//   3. On the player's BEGIN choice → reset VillageState + load MainMenu.
//   4. On CONTINUE → load MainMenu, which routes the player's last save.
//
// Idempotency: if `VillageState.seenColdOpen` is already true AND there's
// no `forceReplay` flag, we go straight to MainMenu with no cinematic at
// all — re-boots feel instant for returning players. The Pause-menu's
// "Replay Cold Open" toggle (Phase 50) sets the forceReplay flag in
// PlayerPrefs and clears `seenColdOpen` on the next boot.
//
// Audio coupling: when MusicPlayer is present (Phase 38 wired in
// Bootstrap), we request the "cold_open" music id at Stage 1 and let the
// MusicPlayer crossfade into the MainMenu cue when the cinematic ends.
// Pure event-bus communication — no direct dependency on the Audio asmdef
// type (we publish `SceneAudioRequestedEvent` and let the MusicPlayer
// decide whether the library has a clip for the id).
//
// ── Voice-over (D-058 / D-059) ──────────────────────────────────────
// Two narrator lines from the Marin letter carry canonical lineIds —
// `narrator_marin_letter_01` and `narrator_marin_letter_02` — so when a
// `.wav` for them exists under `Assets/_Project/Audio/Voice/Narrator/`,
// `VoicePlayer.Play(...)` plays them in sync with the typewriter, just
// like Doris's Mission 1 lines. The cinematic works silently otherwise.

using System.Collections;
using UnityEngine;
using HearthboundHollow.Core;
using HearthboundHollow.UI;

namespace HearthboundHollow.Mission
{
    public class BootstrapHookDirector : MonoBehaviour
    {
        public const string ForceReplayPrefKey = "Hh_ForceReplayColdOpen";
        public const string CinematicMusicId = "cold_open_hook";

        [Header("Wired by Phase 48 builder")]
        public ColdOpenCinematicUI cinematic;

        [Header("Routing")]
        public string mainMenuSceneName = "01_MainMenu";

        [Header("Voice lineIds (D-058)")]
        public string letterLineIdA = "narrator_marin_letter_01";
        public string letterLineIdB = "narrator_marin_letter_02";

        [Header("Behaviour")]
        [Tooltip("Force the cinematic on this boot even if seenColdOpen=true. " +
                 "Set by the Pause menu's 'Replay Cold Open' toggle.")]
        public bool forceReplay = false;

        // ─── Lifecycle ────────────────────────────────────────────────

        private bool _kicked;
        private GameManager _gm;
        private bool _prevAutoLoad;

        private void Awake()
        {
            // Pick up the player-prefs flag for a one-shot replay.
            if (PlayerPrefs.GetInt(ForceReplayPrefKey, 0) == 1)
            {
                forceReplay = true;
                PlayerPrefs.SetInt(ForceReplayPrefKey, 0);
                PlayerPrefs.Save();
            }

            _gm = FindFirstObjectByType<GameManager>();
            if (_gm == null)
            {
                Hh.Warn(LogCategory.Boot, "BootstrapHookDirector: no GameManager found. " +
                                          "Cold Open will run but cannot transition.");
                return;
            }
            _prevAutoLoad = _gm.autoLoadMainMenu;
            _gm.autoLoadMainMenu = false;
        }

        private void Start()
        {
            if (cinematic == null)
            {
                Hh.Warn(LogCategory.Boot, "BootstrapHookDirector: no ColdOpenCinematicUI wired. " +
                                          "Falling back to MainMenu directly.");
                LoadMainMenu();
                return;
            }
            StartCoroutine(KickWhenReady());
        }

        private IEnumerator KickWhenReady()
        {
            // Allow one frame so GameManager has registered services + the
            // VillageState is bound.
            yield return null;

            if (_kicked) yield break;
            _kicked = true;

            var state = _gm != null ? _gm.villageState : null;

            // Publish a music request — MusicPlayer (if present) decides
            // whether the library has a 'cold_open_hook' cue and crossfades.
            EventBus.Publish(new SceneAudioRequestedEvent(CinematicMusicId, "cold_open_hush"));

            // Save-aware: if already seen and no replay flag, skip cleanly.
            if (state != null && state.seenColdOpen && !forceReplay)
            {
                Hh.Log(LogCategory.Boot, "Cold Open skipped (seenColdOpen=true).");
                LoadMainMenu();
                yield break;
            }

            // Reset the seen flag for a replay run so the player gets the
            // full cinematic again.
            if (forceReplay && state != null)
            {
                state.seenColdOpen = false;
                state.coldOpenLastVariant = "";
            }

            cinematic.forceReplay = forceReplay;
            cinematic.Play(state, OnChoice);
        }

        private void OnChoice(ColdOpenChoice choice)
        {
            if (_gm != null && _gm.villageState != null)
            {
                _gm.villageState.seenColdOpen = true;
                _gm.villageState.coldOpenLastVariant = choice.ToString();
            }

            // "Begin" → make sure the next MainMenu boot routes to a NEW save.
            // "Continue" → trust the existing save (no reset).
            if (choice == ColdOpenChoice.Begin)
            {
                // Don't blow the save away here — the MainMenu's New Game
                // button is the one with the destructive warning. We just
                // signal the menu controller via PlayerPrefs.
                PlayerPrefs.SetInt("Hh_ColdOpenChoiceBegin", 1);
                PlayerPrefs.SetInt("Hh_ColdOpenChoiceContinue", 0);
            }
            else
            {
                PlayerPrefs.SetInt("Hh_ColdOpenChoiceBegin", 0);
                PlayerPrefs.SetInt("Hh_ColdOpenChoiceContinue", 1);
            }
            PlayerPrefs.Save();

            LoadMainMenu();
        }

        private void LoadMainMenu()
        {
            // Restore GameManager's autoLoadMainMenu so future re-boots work.
            if (_gm != null) _gm.autoLoadMainMenu = _prevAutoLoad;

            if (_gm != null && !string.IsNullOrEmpty(mainMenuSceneName))
            {
                _gm.LoadScene(mainMenuSceneName);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
            }
        }
    }
}
