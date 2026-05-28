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
// prominently and the contract matches.
//
// ── Phase 35 — draw-circle remediation (2026-05-25) ─────────────
// Follow-on user report: "I want to draw circle but no interaction appear
// or line visible" — on a stock MacBook, Phase 34's `Input.GetMouseButton(0)`
// fallback was STILL returning false on the trackpad. On Unity 6 + URP +
// macOS, the legacy mouse-button API has a known edge case where it stays
// false while `Mouse.current.leftButton.isPressed` returns true.
//
// Phase 35 fix (this commit):
//   * Press-down gate now reads from FOUR sources and ORs them together:
//       1) wired InputActionReference         (designer-explicit)
//       2) Mouse.current.leftButton.isPressed (Input System default)  ← NEW
//       3) Input.GetMouseButton(0)            (legacy)
//       4) Touchscreen.current.primaryTouch   (trackpad gesture / mobile)
//   * Pointer position also reads from three sources in priority order.
//   * Expose public read-only state for the PolishCircleVisualizer
//     (Phase 35 commit 3/4): LastPointerScreenPos, IsPointerActive,
//     LastPointerSpeed, IsQuadrantTouched(int), QuadrantCoverageCount,
//     CurrentClarity01, OnQuadrantCovered event.
//   * 1Hz diagnostic logs (`verboseInputDiagnostics`) so the next
//     playtester can confirm input is reaching the game.

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
        [Tooltip("Optional Input System action for pointer position (Vector2 / screen px). " +
                 "If unwired, falls back to Mouse.current.position then to Input.mousePosition.")]
        [SerializeField] private InputActionReference pointerPositionAction;
        [Tooltip("Optional Input System action for the 'polish is being applied' button. " +
                 "If unwired, ORs Mouse.current.leftButton.isPressed (Input System default), " +
                 "Input.GetMouseButton(0) (legacy), and Touchscreen.current.primaryTouch.press. " +
                 "Phase 35 fix: Mac trackpad can return false on the legacy API while " +
                 "Mouse.current returns true — we now read both.")]
        [SerializeField] private InputActionReference pointerActiveAction;

        [Header("Coverage tracking (4 quadrants)")]
        [Range(1, 4)] public int requiredQuadrants = 1;

        [Header("Polish radius (Phase 32.16)")]
        [Tooltip("Quadrants are computed RELATIVE TO THE ORB'S SCREEN POSITION, " +
                 "not the centre of the screen. The cross-hair sits at the orb. " +
                 "Cursor positions OUTSIDE polishRadiusPx don't count — keeps " +
                 "the 'draw small circles around the orb' instruction honest.")]
        [Range(60f, 600f)] public float polishRadiusPx = 220f;
        [Tooltip("Cursor positions INSIDE coreRadiusPx (the very centre of the " +
                 "orb on screen) also don't count — encourages the player to " +
                 "circle around the orb rather than scrub on top of it.")]
        [Range(0f, 200f)] public float coreRadiusPx = 30f;

        [Header("Diagnostics (Phase 35)")]
        [Tooltip("Logs a 1Hz line summarising which input source fired + cursor + clarity. " +
                 "Leave on through the 20-person playtest, disable for ship.")]
        public bool verboseInputDiagnostics = true;

        // ───── Internal state ──────────────────────────────────────

        private Vector2 _lastPointer;
        private bool _pointerWasActive;
        private float _lastClarity;
        private bool _milestoneFired;
        private bool _revealFired;
        private readonly bool[] _quadrantTouched = new bool[4];
        private float _quadrantResetTimer;

        // Per-frame snapshot for the visualizer + diagnostic (Phase 35).
        private Vector2 _lastReadPointer;
        private bool _lastReadActive;
        private float _lastReadSpeed;
        private float _diagTimer;

        public event System.Action<float> OnClarityChanged;
        public event System.Action OnMilestoneReached;
        public event System.Action OnRevealReached;
        public event System.Action OnFrictionWarning;
        public event System.Action<int> OnQuadrantCovered; // arg: quadrant index 0..3

        // ───── Public read-only API for PolishCircleVisualizer (Phase 35) ──

        /// <summary>Latest pointer screen position the mini-game sampled this frame.</summary>
        public Vector2 LastPointerScreenPos => _lastReadPointer;

        /// <summary>True if the press-down gate fired this frame (from ANY of the 4 input sources).</summary>
        public bool IsPointerActive => _lastReadActive;

        /// <summary>Last measured normalised cursor speed (delta / Screen.width).</summary>
        public float LastPointerSpeed => _lastReadSpeed;

        /// <summary>Read-only quadrant coverage state. Index 0..3 — packed: (top? 2 : 0) + (right? 1 : 0).</summary>
        public bool IsQuadrantTouched(int q) =>
            q >= 0 && q < 4 && _quadrantTouched[q];

        /// <summary>How many of the 4 quadrants the player has touched so far.</summary>
        public int QuadrantCoverageCount
        {
            get { int n = 0; for (int i = 0; i < 4; i++) if (_quadrantTouched[i]) n++; return n; }
        }

        /// <summary>Current clarity 0..1 (mirrors targetOrb.runtimeClarity).</summary>
        public float CurrentClarity01 => targetOrb != null ? targetOrb.runtimeClarity : 0f;

        // ───── Lifecycle ──────────────────────────────────────────

        public override void BeginGame(MemoryOrbInteractable orb)
        {
            base.BeginGame(orb);
            _lastClarity = orb != null ? orb.runtimeClarity : 0.4f;
            _milestoneFired = false;
            _revealFired = false;
            for (int i = 0; i < 4; i++) _quadrantTouched[i] = false;
            _pointerWasActive = false;
            _diagTimer = 0f;
            State = MiniGameState.Active;
            if (pointerPositionAction != null) pointerPositionAction.action?.Enable();
            if (pointerActiveAction != null) pointerActiveAction.action?.Enable();

            // Phase 32.14 — attach the world-space highlighter: pulsing gold
            // ring orbiting the orb, scale-pulse on the orb itself, cursor
            // trail while LMB is held, camera dolly-in. Auto-spawned, no
            // scene wiring required.
            PolishOrbHighlighter.AttachTo(this, orb);

            Hh.Log(LogCategory.MiniGame,
                $"[Polish] BeginGame fired. State={State}. Target={(orb != null ? orb.name : "null")}. " +
                $"verboseDiag={verboseInputDiagnostics}. Phase 35 quad-source input wired.");
        }

        protected override void Update()
        {
            base.Update();
            if (State != MiniGameState.Active) return;
            if (targetOrb == null) { Abort(); return; }

            // ── Pointer position — three sources, priority order:
            //     1) wired InputActionReference (designer-explicit)
            //     2) Mouse.current.position (Input System default)
            //     3) Input.mousePosition (legacy)
            Vector2 cur = (Vector2)Input.mousePosition;
            if (Mouse.current != null) cur = Mouse.current.position.ReadValue();
            if (pointerPositionAction != null && pointerPositionAction.action != null && pointerPositionAction.action.enabled)
                cur = pointerPositionAction.action.ReadValue<Vector2>();

            // ── Press-down gate — OR of four sources (Phase 35 fix).
            //     Drop the permissive "unwired = pressed" trap from before.
            bool legacyDown    = Input.GetMouseButton(0);
            bool newInputDown  = Mouse.current != null && Mouse.current.leftButton.isPressed;
            bool actionDown    = pointerActiveAction != null && pointerActiveAction.action != null
                                 && pointerActiveAction.action.enabled
                                 && pointerActiveAction.action.IsPressed();
            bool touchDown     = Touchscreen.current != null
                                 && Touchscreen.current.primaryTouch.press.isPressed;
            bool active        = legacyDown || newInputDown || actionDown || touchDown;

            // Cache for the screen-space visualizer + diagnostic.
            _lastReadPointer = cur;
            _lastReadActive  = active;

            Vector2 delta = _pointerWasActive ? (cur - _lastPointer) : Vector2.zero;
            float speed = delta.magnitude / Mathf.Max(0.001f, Screen.width);
            _lastReadSpeed = speed;

            // ── 1Hz diagnostic (Phase 35). Toggleable for ship.
            if (verboseInputDiagnostics)
            {
                _diagTimer += Time.deltaTime;
                if (_diagTimer >= 1.0f)
                {
                    _diagTimer = 0f;
                    int touched = QuadrantCoverageCount;
                    Hh.Log(LogCategory.MiniGame,
                        $"[Polish] legacyLMB={legacyDown} newLMB={newInputDown} " +
                        $"actionLMB={actionDown} touch={touchDown} active={active} " +
                        $"cursor=({cur.x:F0},{cur.y:F0}) speed={speed:F4} " +
                        $"quadrants={touched}/4 clarity={targetOrb.runtimeClarity:F2}");
                }
            }

            // Phase 32.16 — gate on cursor-near-orb. Project the orb's world
            // position into screen-space and only count circle-drag motion
            // that falls inside [coreRadiusPx … polishRadiusPx] of the orb
            // on screen. Without this gate, a screen-spanning drag could
            // farm clarity from anywhere on the display — the visible orb
            // becomes irrelevant.
            bool nearOrb = IsCursorNearOrb(cur, out Vector2 orbScreen);

            if (active && speed > motionThresholdNormalized && nearOrb)
            {
                MarkQuadrant(cur, orbScreen);
                int touched = QuadrantCoverageCount;
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

        private void MarkQuadrant(Vector2 cursor, Vector2 orbScreen)
        {
            // Phase 32.16 — quadrants are RELATIVE TO THE ORB'S SCREEN POSITION.
            // A cursor 'right' of the orb is +X relative to the orb, not the
            // screen centre. A 200-px circle around the visible orb now covers
            // all 4 quadrants in one smooth circle — matches the player's
            // mental model of "draw a circle around the orb".
            bool right = cursor.x >= orbScreen.x;
            bool top   = cursor.y >= orbScreen.y;
            int q = (top ? 2 : 0) + (right ? 1 : 0);
            bool wasTouched = _quadrantTouched[q];
            _quadrantTouched[q] = true;
            if (!wasTouched) OnQuadrantCovered?.Invoke(q);
        }

        /// <summary>
        /// Phase 32.16 — true if the cursor lies inside the polish ring
        /// (between coreRadiusPx and polishRadiusPx of the orb's screen
        /// position). Outputs the orb's projected screen position so the
        /// quadrant test can reuse it without re-projecting.
        /// </summary>
        private bool IsCursorNearOrb(Vector2 cursor, out Vector2 orbScreenPos)
        {
            orbScreenPos = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f); // fallback
            if (targetOrb == null) return false;
            var cam = Camera.main;
            if (cam == null) return true; // no camera → don't gate, fall through to legacy
            Vector3 sp = cam.WorldToScreenPoint(targetOrb.transform.position);
            if (sp.z < 0f) return false;  // orb is behind the camera
            orbScreenPos = new Vector2(sp.x, sp.y);
            float d2 = (cursor - orbScreenPos).sqrMagnitude;
            float minD2 = coreRadiusPx * coreRadiusPx;
            float maxD2 = polishRadiusPx * polishRadiusPx;
            return d2 >= minD2 && d2 <= maxD2;
        }

        /// <summary>
        /// Phase 32.16 — public read-only screen-space position of the
        /// target orb. Used by PolishOrbHighlighter to draw a screen-space
        /// "draw here" guide ring matching the input gate's geometry.
        /// </summary>
        public bool TryGetOrbScreenPos(out Vector2 screen)
        {
            screen = Vector2.zero;
            if (targetOrb == null) return false;
            var cam = Camera.main;
            if (cam == null) return false;
            Vector3 sp = cam.WorldToScreenPoint(targetOrb.transform.position);
            if (sp.z < 0f) return false;
            screen = new Vector2(sp.x, sp.y);
            return true;
        }

        /// <summary>Polish radius in screen pixels (read-only mirror for the highlighter).</summary>
        public float PolishRadiusPx => polishRadiusPx;
        /// <summary>Core radius in screen pixels (read-only mirror for the highlighter).</summary>
        public float CoreRadiusPx   => coreRadiusPx;

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

            // Phase 32.14 — tear down the highlighter / restore the orb
            // scale + camera dolly cleanly.
            var hl = GetComponent<PolishOrbHighlighter>();
            if (hl != null) hl.Unbind();

            float finalClarity = targetOrb != null ? targetOrb.runtimeClarity : 1f;
            EventBus.Publish(new MemoryPolishedEvent(targetOrb != null ? targetOrb.memory : null, finalClarity, autoCompleted));
        }
    }
}
