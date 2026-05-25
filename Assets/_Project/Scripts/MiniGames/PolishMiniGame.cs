// SPDX-License-Identifier: MIT
// Hearthbound Hollow — MiniGames / PolishMiniGame (Mission 1)
//
// The Mission 1 tutorial mini-game. The player drags the cursor in circles
// over a memory orb; clarity climbs as quadrant coverage accumulates.
//
// Spec: Focus 04 § 2.
//   * Cannot fail.
//   * Median completion: ~78 s @ Default difficulty.
//   * Threshold checkpoints @ 0.55 ("midway chime") and 0.85 ("reveal swell").
//   * Auto-Complete plays a 12 s sped-up animation reaching clarity 0.95.
//
// ── Phase 34 (2026-05-25) ───────────────────────────────────────
// User-reported bug: "the game is stuck after few dialog mainly after I
// play it when it reach to the dialogue 'Doris stands back and watches'".
// One contributor to that confusion is that the old fallback path treated
// a null `pointerActiveAction` as "always active" — so polish progressed
// on ANY mouse motion, even without holding LMB. This directly contradicted
// the help overlay copy that says "Hold left mouse, draw slow circles"
// and made the mini-game's interaction model invisible to the player.
//
// Fix: when no InputActionReference is wired, require Input.GetMouseButton(0)
// in the fallback. The MiniGameTutorialUI now displays "Hold Left Mouse"
// prominently and the contract matches. The InputActionReference path is
// preserved for when Phase 26's HearthboundInput.inputactions is wired
// to these fields.

using UnityEngine;
using UnityEngine.InputSystem;
using HearthboundHollow.Core;
using HearthboundHollow.Player;

namespace HearthboundHollow.MiniGames
{
    public class PolishMiniGame : MiniGameBase
    {
        [Header("Tuning")]
        [Range(0.001f, 0.05f)] public float clarityGainPerSecond = 0.014f;
        [Range(0.001f, 1f)] public float motionThresholdNormalized = 0.05f;
        [Range(0.01f, 5f)] public float maxComfortableSpeed = 2.4f;
        [Range(0f, 1f)] public float milestoneClarity = 0.55f;
        [Range(0f, 1f)] public float revealClarity = 0.85f;
        public float autoCompleteDuration = 12f;
        [Range(0f, 1f)] public float autoCompleteClarity = 0.95f;

        [Header("Input")]
        [SerializeField] private InputActionReference pointerPositionAction;
        [SerializeField] private InputActionReference pointerActiveAction;

        [Header("Coverage tracking (4 quadrants)")]
        [Range(1, 4)] public int requiredQuadrants = 1;

        private Vector2 _lastPointer;
        private bool _pointerWasActive;
        private float _lastClarity;
        private bool _milestoneFired;
        private bool _revealFired;
        private readonly bool[] _quadrantTouched = new bool[4];
        private float _quadrantResetTimer;

        public event System.Action<float> OnClarityChanged;
        public event System.Action OnMilestoneReached;
        public event System.Action OnRevealReached;
        public event System.Action OnFrictionWarning;

        public override void BeginGame(MemoryOrbInteractable orb)
        {
            base.BeginGame(orb);
            _lastClarity = orb != null ? orb.runtimeClarity : 0.4f;
            _milestoneFired = false;
            _revealFired = false;
            for (int i = 0; i < 4; i++) _quadrantTouched[i] = false;
            _pointerWasActive = false;
            State = MiniGameState.Active;
            if (pointerPositionAction != null) pointerPositionAction.action?.Enable();
            if (pointerActiveAction != null) pointerActiveAction.action?.Enable();
        }

        protected override void Update()
        {
            base.Update();
            if (State != MiniGameState.Active) return;
            if (targetOrb == null) { Abort(); return; }

            Vector2 cur = pointerPositionAction != null && pointerPositionAction.action != null
                ? pointerPositionAction.action.ReadValue<Vector2>()
                : (Vector2)Input.mousePosition;

            // Phase 34 fix: when no InputActionReference is wired (Phase
            // 22/23 builders don't set one), `pointerActiveAction` is
            // null. The old fallback treated that as "always active",
            // which let the polish progress on any mouse motion — even
            // without holding the button — directly contradicting the
            // tutorial card that says "Hold left mouse, draw slow circles".
            // Now we require LMB held in the fallback, matching the help
            // overlay and the OnboardingOverlay copy.
            bool active;
            if (pointerActiveAction != null && pointerActiveAction.action != null)
                active = pointerActiveAction.action.IsPressed();
            else
                active = Input.GetMouseButton(0);

            Vector2 delta = _pointerWasActive ? (cur - _lastPointer) : Vector2.zero;
            float speed = delta.magnitude / Mathf.Max(0.001f, Screen.width);

            if (active && speed > motionThresholdNormalized)
            {
                MarkQuadrant(cur);
                int touched = 0;
                for (int i = 0; i < 4; i++) if (_quadrantTouched[i]) touched++;
                float multiplier = Mathf.Clamp01((float)touched / requiredQuadrants);

                if (speed > maxComfortableSpeed)
                {
                    OnFrictionWarning?.Invoke();
                    multiplier *= 0.4f;
                }

                float newClarity = Mathf.Clamp01(targetOrb.runtimeClarity + clarityGainPerSecond * Time.deltaTime * multiplier * 25f);
                targetOrb.SetClarity(newClarity);
                if (Mathf.Abs(newClarity - _lastClarity) > 0.001f)
                {
                    _lastClarity = newClarity;
                    RuntimeProgress01 = newClarity;
                    OnClarityChanged?.Invoke(newClarity);
                }

                _quadrantResetTimer += Time.deltaTime;
                if (_quadrantResetTimer > 2.5f)
                {
                    _quadrantResetTimer = 0f;
                    for (int i = 0; i < 4; i++) _quadrantTouched[i] = false;
                }
            }

            if (!_milestoneFired && targetOrb.runtimeClarity >= milestoneClarity)
            {
                _milestoneFired = true;
                OnMilestoneReached?.Invoke();
                Hh.Log(LogCategory.MiniGame, "Polish midway milestone fired.");
            }
            if (!_revealFired && targetOrb.runtimeClarity >= revealClarity)
            {
                _revealFired = true;
                OnRevealReached?.Invoke();
                Hh.Log(LogCategory.MiniGame, "Polish reveal swell fired.");
            }
            if (targetOrb.runtimeClarity >= 0.99f) Complete();

            _lastPointer = cur;
            _pointerWasActive = active;
        }

        private void MarkQuadrant(Vector2 pos)
        {
            bool right = pos.x >= Screen.width * 0.5f;
            bool top = pos.y >= Screen.height * 0.5f;
            int q = (top ? 2 : 0) + (right ? 1 : 0);
            _quadrantTouched[q] = true;
        }

        public override void DoAutoComplete()
        {
            if (State == MiniGameState.Complete) return;
            Hh.Log(LogCategory.MiniGame, "Polish auto-complete triggered.");
            StartCoroutine(AutoCoroutine());
        }

        private System.Collections.IEnumerator AutoCoroutine()
        {
            State = MiniGameState.Completing;
            float startClarity = targetOrb != null ? targetOrb.runtimeClarity : 0.4f;
            float t = 0f;
            while (t < autoCompleteDuration && targetOrb != null)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, t / autoCompleteDuration);
                targetOrb.SetClarity(Mathf.Lerp(startClarity, autoCompleteClarity, k));
                RuntimeProgress01 = targetOrb.runtimeClarity;
                yield return null;
            }
            Complete(autoCompleted: true);
        }

        private void Complete(bool autoCompleted = false)
        {
            if (State == MiniGameState.Complete) return;
            FinishGame();
            float finalClarity = targetOrb != null ? targetOrb.runtimeClarity : 1f;
            EventBus.Publish(new MemoryPolishedEvent(targetOrb != null ? targetOrb.memory : null, finalClarity, autoCompleted));
        }
    }
}
