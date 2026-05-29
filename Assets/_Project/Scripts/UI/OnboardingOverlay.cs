// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / OnboardingOverlay
//
// PHASE 30 — Multi-step onboarding card.
//
// User request on first-playtest review:
//   "create the onboarding and control guidance UI"
//
// Distinct from HelpOverlayUI:
//   • HelpOverlay is a single-page reference card you can summon any time
//     with H. It's a *quick lookup*.
//   • OnboardingOverlay is a step-through walkthrough that runs ONCE on a
//     fresh save, on top of the Lane scene. Each step pauses the game and
//     waits for the player to click "Next" (or press Space). Steps are
//     data-driven via `Step[] steps` so designers can add / re-order steps
//     in the Inspector.
//
// Each step has:
//   • headline (large bold title)
//   • body (paragraph, auto-wraps + auto-sizes)
//   • optional illustrated key chip (rendered as a small parchment chip)
//   • optional "expects" hint — the input action the player must perform
//     before the step auto-advances (e.g. "press_wasd", "press_e").
//
// Cozy framing:
//   • The overlay is dismissible from frame 1 ("Skip Tutorial" button).
//   • Closing it sets VillageState.onboardingCompleted = true so it
//     never appears again on this save.
//   • No fail states, no penalty for skipping — Marin's voice gently
//     introduces concepts without nagging.
//
// USAGE — Phase 30 capstone wires this onto the Lane scene canvas. The
// scene calls `BeginIfNeeded()` on Start; the overlay self-pauses the
// game via `Time.timeScale = 0` and resumes when the player skips or
// completes the walkthrough.

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HearthboundHollow.Core;

namespace HearthboundHollow.UI
{
    [DisallowMultipleComponent]
    public class OnboardingOverlay : MonoBehaviour
    {
        // ───── Per-step data ─────────────────────────────────────

        [Serializable]
        public class Step
        {
            [Tooltip("Large bold heading at the top of the card.")]
            public string headline = "Welcome to the Hollow";

            [Tooltip("Multi-line body. Use \\n for paragraph breaks. Auto-wrap " +
                     "+ auto-size are applied so it never clips.")]
            [TextArea(3, 12)]
            public string body =
                "This is a quiet game about memory, choice, and care.\n" +
                "Take your time. The Hollow will wait.";

            [Tooltip("Optional key glyph rendered as a small parchment chip. " +
                     "Empty string = no chip. Use e.g. 'WASD', 'E', 'Space', " +
                     "'Right Mouse'.")]
            public string keyChip = "";

            [Tooltip("Optional sub-caption under the key chip.")]
            public string keyCaption = "";

            [Tooltip("Optional input expectation. Leave empty for a manual-" +
                     "advance step. Otherwise set to one of:\n" +
                     "  press_wasd     — any WASD / arrow / left-stick input\n" +
                     "  press_e        — interact action\n" +
                     "  press_space    — space (advance dialogue)\n" +
                     "  press_lmb      — left mouse (polish drag)\n" +
                     "  press_rmb      — right mouse (camera look)\n" +
                     "When the expectation is satisfied the step auto-advances " +
                     "after a short delay.")]
            public string expects = "";
        }

        // ───── Inspector references ──────────────────────────────

        [Header("Root")]
        [Tooltip("Visual root toggled on/off by BeginIfNeeded / Hide. Should be " +
                 "a child of the script-host GameObject (same two-layer pattern " +
                 "as PauseMenu / Help).")]
        public GameObject root;

        [Header("Labels")]
        public TextMeshProUGUI headlineLabel;
        public TextMeshProUGUI bodyLabel;
        public TextMeshProUGUI keyChipLabel;
        public TextMeshProUGUI keyCaptionLabel;
        public TextMeshProUGUI progressLabel;     // "1 / 5"

        [Header("Buttons")]
        public Button nextButton;
        public Button skipButton;

        [Header("Behaviour")]
        [Tooltip("Show on Start when VillageState.onboardingCompleted is false. " +
                 "Idempotent — re-calling BeginIfNeeded() while the overlay is " +
                 "already showing is a no-op.")]
        public bool autoShowOnStart = true;

        [Tooltip("Pause Time.timeScale while the overlay is open. Default true " +
                 "— the cozy onboarding is meant to be read at your own pace.")]
        public bool pauseGameWhileOpen = true;

        [Tooltip("After an `expects` input is satisfied, this many seconds " +
                 "pass before the step auto-advances.")]
        [Range(0f, 3f)]
        public float autoAdvanceDelay = 0.6f;

        [Header("Steps (designer-editable)")]
        [Tooltip("Step sequence. If left empty, a sensible default sequence is " +
                 "used (see DefaultSteps()).")]
        public Step[] steps;

        // ───── Public API ────────────────────────────────────────

        public event Action OnCompleted;

        public bool IsOpen { get; private set; }

        // ───── Internals ─────────────────────────────────────────

        private int _index;
        private bool _expectingInput;
        private float _autoAdvanceTimer;
        private float _prevTimeScale = 1f;

        // ───── Lifecycle ─────────────────────────────────────────

        private void Awake()
        {
            if (root != null && root != gameObject) root.SetActive(false);
            if (nextButton != null) nextButton.onClick.AddListener(Advance);
            if (skipButton != null) skipButton.onClick.AddListener(SkipAll);

            // Phase 29 — auto-fit every TMP label so first-time-player intros
            // don't clip on a windowed canvas.
            UIAutoFitText.ApplyToButtonLabel(headlineLabel,    minSize: 22, maxSize: 42);
            UIAutoFitText.ApplyToLabel(bodyLabel,               minSize: 14, maxSize: 26);
            UIAutoFitText.ApplyToButtonLabel(keyChipLabel,      minSize: 20, maxSize: 36);
            UIAutoFitText.ApplyToLabel(keyCaptionLabel,         minSize: 12, maxSize: 20);
            UIAutoFitText.ApplyToButtonLabel(progressLabel,     minSize: 12, maxSize: 18);

            if (steps == null || steps.Length == 0) steps = DefaultSteps();
        }

        private void Start()
        {
            if (autoShowOnStart) BeginIfNeeded();
        }

        private void Update()
        {
            if (!IsOpen) return;

            // Check the input expectation of the current step.
            if (_expectingInput && CheckCurrentStepExpectation())
            {
                _expectingInput = false;
                _autoAdvanceTimer = autoAdvanceDelay;
            }
            if (_autoAdvanceTimer > 0f)
            {
                // Auto-advance uses unscaled time because the game is paused.
                _autoAdvanceTimer -= Time.unscaledDeltaTime;
                if (_autoAdvanceTimer <= 0f) Advance();
            }

            // Manual fallbacks — Space or Enter advances the current step.
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                Advance();
            if (Input.GetKeyDown(KeyCode.Escape))
                SkipAll();
        }

        // ───── Show / Hide ───────────────────────────────────────

        public void BeginIfNeeded()
        {
            if (IsOpen) return;

            // Don't show again on completed saves.
            var vs = ServiceLocator.Get<VillageState>();
            if (vs != null && vs.onboardingCompleted) return;

            Begin();
        }

        public void Begin()
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            if (root != null) root.SetActive(true);
            _index = 0;
            ApplyStep();
            IsOpen = true;

            if (pauseGameWhileOpen)
            {
                _prevTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }
            Hh.Log(LogCategory.UI, "Onboarding overlay opened.");
        }

        public void Advance()
        {
            if (!IsOpen) return;
            _index++;
            _autoAdvanceTimer = 0f;
            _expectingInput = false;
            if (_index >= steps.Length)
            {
                Complete();
                return;
            }
            ApplyStep();
        }

        public void SkipAll()
        {
            Hh.Log(LogCategory.UI, "Onboarding skipped by player.");
            Complete();
        }

        private void Complete()
        {
            var vs = ServiceLocator.Get<VillageState>();
            if (vs != null) vs.onboardingCompleted = true;

            if (root != null && root != gameObject) root.SetActive(false);
            IsOpen = false;

            if (pauseGameWhileOpen) Time.timeScale = _prevTimeScale;
            OnCompleted?.Invoke();
        }

        // ───── Step rendering ────────────────────────────────────

        private void ApplyStep()
        {
            if (_index < 0 || _index >= steps.Length) return;
            var s = steps[_index];

            if (headlineLabel    != null) headlineLabel.text    = s.headline ?? string.Empty;
            if (bodyLabel        != null) bodyLabel.text        = s.body ?? string.Empty;
            if (keyChipLabel     != null) keyChipLabel.text     = s.keyChip ?? string.Empty;
            if (keyCaptionLabel  != null) keyCaptionLabel.text  = s.keyCaption ?? string.Empty;
            if (progressLabel    != null) progressLabel.text    = $"{_index + 1} / {steps.Length}";

            // Hide the key-chip group entirely when keyChip is empty.
            if (keyChipLabel != null) keyChipLabel.gameObject.SetActive(!string.IsNullOrEmpty(s.keyChip));
            if (keyCaptionLabel != null) keyCaptionLabel.gameObject.SetActive(!string.IsNullOrEmpty(s.keyCaption));

            _expectingInput = !string.IsNullOrEmpty(s.expects);
        }

        // ───── Input expectations ────────────────────────────────

        private bool CheckCurrentStepExpectation()
        {
            var s = steps[_index];
            switch (s.expects)
            {
                case "press_wasd":
                    return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||
                           Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) ||
                           Input.GetKey(KeyCode.UpArrow)    || Input.GetKey(KeyCode.DownArrow) ||
                           Input.GetKey(KeyCode.LeftArrow)  || Input.GetKey(KeyCode.RightArrow);
                case "press_e":
                    return Input.GetKeyDown(KeyCode.E);
                case "press_space":
                    return Input.GetKeyDown(KeyCode.Space);
                case "press_lmb":
                    return Input.GetMouseButtonDown(0);
                case "press_rmb":
                    return Input.GetMouseButton(1);
                default:
                    return false;
            }
        }

        // ───── Default step content ──────────────────────────────

        // Phase 54 (D-070) — cleaner, action-first onboarding copy to current
        // cozy-PC-hit standards: one concept per card, a verb-led headline, a
        // single short body, and an unmistakable key chip. Headlines route their
        // decorative emoji through HollowGlyphs.Format so they render as on-brand
        // gold glyphs in TextMesh Pro (and degrade to clean text — never a tofu
        // box — before the glyph asset is built).
        private static Step[] DefaultSteps() => new[]
        {
            new Step {
                headline = HollowGlyphs.Format("🪔 Welcome to the Hollow"),
                body = "You've inherited a little memory-shop in a quiet autumn village.\n" +
                       "No combat. No fail screens. Just people — and the memories they trust you to keep.",
                keyChip = "",
                keyCaption = "",
                expects = ""
            },
            new Step {
                headline = HollowGlyphs.Format("🚶 Move"),
                body = "Wander at your own pace. Someone is waiting at the door of the Hollow.",
                keyChip = "WASD",
                keyCaption = "Move  ·  Arrows / Left Stick",
                expects = "press_wasd"
            },
            new Step {
                headline = HollowGlyphs.Format("✋ Interact"),
                body = "A soft golden prompt means you can act — open a door, take a memory, pour the tea.",
                keyChip = "E",
                keyCaption = "Interact",
                expects = ""
            },
            new Step {
                headline = HollowGlyphs.Format("✨ Polish a memory"),
                body = "Hold the left mouse button and trace slow circles over the glowing orb.\n" +
                       "Slower is kinder. Cover all four sides — like a kindness with four corners.",
                keyChip = "Hold LMB",
                keyCaption = "Draw slow circles",
                expects = ""
            },
            new Step {
                headline = HollowGlyphs.Format("🕯 Take it easy"),
                body = "Press <b>Esc</b> any time for Settings — Gentle Mode, Auto-Complete, and language.\n" +
                       "Press <b>H</b> for the full controls whenever you need them.",
                keyChip = "Esc",
                keyCaption = "Pause  ·  Settings  ·  Comfort",
                expects = ""
            },
            new Step {
                headline = HollowGlyphs.Format("🍂 You're ready"),
                body = "Walk to the door of the Hollow when you're ready.\n\n" +
                       "There is no wrong way to keep a memory.\nThere is only the gentle way, and the others.\n\n— Marin",
                keyChip = "",
                keyCaption = "",
                expects = ""
            },
        };
    }
}
