// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / EndOfDaySequencer
//
// Phase 47 — single owner of the night chain:
//   Evening Ledger confirm  ->  Dream (if any)  ->  Goodnight Card  ->  transition
//
// Opt-in: a director only delegates here if this component is wired. When it
// is, the director does NOT also subscribe to the ledger, and DreamHook is
// left dormant (its ledger ref cleared by the Phase47 builder) so dreams aren't
// double-played. With no sequencer wired, day-end behaves exactly as before
// (zero-regression — D-064).
//
// ── Studio integration notes (deviations from the implementation guide) ──
//   • Day resolution: the guide compared afterDayIndex to currentDayIndex
//     directly, but currentDayIndex is 0-based and is only bumped by
//     EndDay() *after* this sequence runs (so it is 0 during M1, 1 during
//     M2 at card-resolve time). We compare against (currentDayIndex + 1) so
//     the fiction Day numbers line up, and a single-tease sequencer always
//     resolves its one tease regardless of the index — the beat can never
//     silently vanish.
//   • Mission 2 already plays Dream 2 *before* the ledger (during the cleanse
//     outro), so the M2 director wires playDream:false here — the card still
//     shows; the dream is never double-played.
//   • Gentle Mode: VillageState.gentleModeEnabled is read here and passed to
//     the card as `instant` so the fade is skipped (identical content, zero
//     stress) — keeps OneMoreDayCard free of any game-state dependency.
//
// ── Phase 54 (D-069) — never strand the player ─────────────────────────────
//   • The dream wait is bounded by a watchdog so a paused/looping/never-stopping
//     Timeline cannot freeze the night chain.
//   • The transition body is guarded; if it throws we fall back to the menu.

using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using HearthboundHollow.Core;
using HearthboundHollow.Cutscene;
using HearthboundHollow.UI;

namespace HearthboundHollow.Mission
{
    public class EndOfDaySequencer : MonoBehaviour
    {
        [Header("References")]
        public OneMoreDayCard card;
        public MemoryDreamSequencer dream;

        [Header("Teases (one per day-end)")]
        public TomorrowTeaseSO[] teases;

        /// <summary>
        /// Run the night sequence, then invoke <paramref name="onComplete"/>
        /// (the director's existing EndDay()+LoadScene body).
        /// </summary>
        /// <param name="playDream">True if a dream should play tonight.</param>
        /// <param name="dreamTrigger">Director-supplied call that starts the
        /// correct dream variant (e.g. () => dream.PlayDream1()).</param>
        /// <param name="onComplete">The original day-end transition body.</param>
        public void BeginNightSequence(bool playDream, Action dreamTrigger, Action onComplete)
        {
            StartCoroutine(RunSequence(playDream, dreamTrigger, onComplete));
        }

        [Header("Phase 54 — safety (D-069)")]
        [Tooltip("Maximum seconds to wait for the dream's OnDreamFinished signal " +
                 "before continuing the night chain anyway. A Timeline that is " +
                 "paused (Time.timeScale 0) or never raises 'stopped' must NEVER " +
                 "strand the player on a frozen screen — the day always advances.")]
        public float dreamWatchdogSeconds = 30f;

        private IEnumerator RunSequence(bool playDream, Action dreamTrigger, Action onComplete)
        {
            // 1) Dream (await its natural finish, with a watchdog so a stalled
            //    Timeline can never soft-lock the night chain — D-069).
            if (playDream && dream != null && dreamTrigger != null)
            {
                bool finished = false;
                Action handler = () => finished = true;
                dream.OnDreamFinished += handler;
                Hh.Log(LogCategory.Cutscene, "EndOfDaySequencer → play dream.");
                dreamTrigger.Invoke();

                float waited = 0f;
                float maxWait = Mathf.Max(1f, dreamWatchdogSeconds);
                while (!finished && waited < maxWait)
                {
                    waited += Time.unscaledDeltaTime;
                    yield return null;
                }
                dream.OnDreamFinished -= handler;
                if (!finished)
                    Hh.Warn(LogCategory.Cutscene,
                        $"EndOfDaySequencer: dream did not signal finish within {maxWait:F0}s — " +
                        "continuing so the day still advances.");
            }

            // 2) Goodnight card (await Goodnight press). Degrades gracefully:
            //    a missing card or tease simply skips this beat — no NRE.
            var tease = ResolveTease();
            if (card != null && tease != null)
            {
                bool advanced = false;
                Action handler = () => advanced = true;
                card.OnContinue += handler;
                card.Show(ResolveForwardLook(tease), tease.pickleSignOffText, GentleModeOn());
                Hh.Log(LogCategory.Mission, $"EndOfDaySequencer → goodnight card '{tease.sourceNode}'.");
                while (!advanced) yield return null;
                card.OnContinue -= handler;
            }
            else
            {
                Hh.Log(LogCategory.Mission,
                    "EndOfDaySequencer → no card/tease wired; skipping goodnight beat.");
            }

            // 3) Transition (the director's original behaviour). Guarded so a
            //    throwing handler can't kill the coroutine and strand the
            //    player mid-night (D-069).
            try
            {
                onComplete?.Invoke();
            }
            catch (Exception ex)
            {
                Hh.Err(LogCategory.Mission,
                    $"EndOfDaySequencer: day-end transition threw — {ex.Message}. " +
                    "Attempting safe fallback to the main menu.");
                var gm = GameManager.Instance;
                if (gm != null) gm.LoadScene(gm.mainMenuSceneName);
            }
        }

        /// <summary>
        /// Resolve which tease to show. Single-tease sequencers (the common
        /// case — one per scene) always return their one tease; multi-tease
        /// sequencers match on the fiction day (currentDayIndex + 1) with a
        /// safe first-non-null fallback so the beat never silently vanishes.
        /// </summary>
        private TomorrowTeaseSO ResolveTease()
        {
            if (teases == null || teases.Length == 0) return null;
            if (teases.Length == 1) return teases[0];

            var vs = ServiceLocator.Get<VillageState>();
            int fictionDay = (vs != null ? vs.currentDayIndex : 0) + 1;
            foreach (var t in teases)
                if (t != null && t.afterDayIndex == fictionDay) return t;

            foreach (var t in teases)
                if (t != null) return t;   // fallback — never strand the player
            return null;
        }

        private string ResolveForwardLook(TomorrowTeaseSO t)
        {
            if (!string.IsNullOrEmpty(t.branchFlagField) && BranchFlagTrue(t.branchFlagField)
                && !string.IsNullOrWhiteSpace(t.forwardLookTextAlt))
                return t.forwardLookTextAlt;
            return t.forwardLookText;
        }

        private bool GentleModeOn()
        {
            var vs = ServiceLocator.Get<VillageState>();
            return vs != null && vs.gentleModeEnabled;
        }

        private bool BranchFlagTrue(string fieldName)
        {
            var vs = ServiceLocator.Get<VillageState>();
            if (vs == null) return false;
            FieldInfo f = vs.GetType().GetField(fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return f != null && f.FieldType == typeof(bool) && (bool)f.GetValue(vs);
        }
    }
}
