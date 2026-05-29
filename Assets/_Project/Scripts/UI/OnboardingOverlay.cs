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
//
// ── Phase 60 — Arabic Localization MVP ──────────────────────────
// Each Step now carries an optional `locKey*` triplet (headline / body /
// caption) so a translator can change every word in loc.<iso>.json
// without touching this file. The English source-of-truth fields on
// Step remain as the fallback when the LocalizationService isn't yet
// registered (headless EditMode test, etc.). DefaultSteps() wires the
// 6 canonical onboarding.stepN.* keys.

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

            // ── Phase 60 — Localization keys ────────────────────────────
            [Tooltip("Phase 60 — Stable localization key for the headline. " +
                     "If set, overrides `headline` at runtime via LocalizationService. " +
                     "Keys live in loc.<iso>.json under 'onboarding.stepN.headline'.")]
            public string locKeyHeadline = "";

            [Tooltip("Phase 60 — Stable localization key for the body.")]
            public string locKeyBody = "";

            [Tooltip("Phase 60 — Stable localization key for the caption.")]
            public string locKeyCaption = "";
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
        public bool autoShowOnStart = true;
        public bool pauseGameWhileOpen = true;
        [Range(0f, 3f)] public float autoAdvanceDelay = 0.6f;

        [Header("Steps (designer-editable)")]
        public Step[] steps;

        public event Action OnCompleted;

        public bool IsOpen { get; private set; }

        private int _index;
        private bool _expectingInput;
        private float _autoAdvanceTimer;
        private float _prevTimeScale = 1f;

        private void Awake()
        {
            if (root != null && root != gameObject) root.SetActive(false);
            if (nextButton != null) nextButton.onClick.AddListener(Advance);
            if (skipButton != null) skipButton.onClick.AddListener(SkipAll);

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

        private void OnEnable()
        {
            EventBus.Subscribe<LocaleChangedEvent>(OnLocaleChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<LocaleChangedEvent>(OnLocaleChanged);
        }

        // Phase 60 — Re-render the current step when the player flips
        // language mid-onboarding (rare, but possible via Pause → Settings).
        private void OnLocaleChanged(LocaleChangedEvent _) { if (IsOpen) ApplyStep(); }

        private void Update()
        {
            if (!IsOpen) return;

            if (_expectingInput && CheckCurrentStepExpectation())
            {
                _expectingInput = false;
                _autoAdvanceTimer = autoAdvanceDelay;
            }
            if (_autoAdvanceTimer > 0f)
            {
                _autoAdvanceTimer -= Time.unscaledDeltaTime;
                if (_autoAdvanceTimer <= 0f) Advance();
            }

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                Advance();
            if (Input.GetKeyDown(KeyCode.Escape))
                SkipAll();
        }

        public void BeginIfNeeded()
        {
            if (IsOpen) return;

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

            // Phase 60 — Localize step text. Each Step now carries a stable
            // `locKey*` triplet which the service resolves into the active
            // language. The legacy English fields on Step are the
            // source-of-truth fallback for any custom step a scene adds
            // without a matching loc entry (and for headless EditMode tests
            // where the LocalizationService isn't yet up).
            var loc = ServiceLocator.Get<LocalizationService>();
            bool rtl = loc != null && loc.IsRightToLeft;

            string headline = loc != null && !string.IsNullOrEmpty(s.locKeyHeadline)
                ? loc.Get(s.locKeyHeadline) : (s.headline ?? string.Empty);
            string body = loc != null && !string.IsNullOrEmpty(s.locKeyBody)
                ? loc.Get(s.locKeyBody)     : (s.body ?? string.Empty);
            string caption = loc != null && !string.IsNullOrEmpty(s.locKeyCaption)
                ? loc.Get(s.locKeyCaption)  : (s.keyCaption ?? string.Empty);
            string progress = loc != null
                ? loc.Format("onboarding.progress_fmt", _index + 1, steps.Length)
                : $"{_index + 1} / {steps.Length}";

            if (headlineLabel != null)
            {
                headlineLabel.text = rtl ? ArabicTextShaper.Shape(headline) : headline;
                headlineLabel.isRightToLeftText = rtl;
            }
            if (bodyLabel != null)
            {
                bodyLabel.text = rtl ? ArabicTextShaper.Shape(body) : body;
                bodyLabel.isRightToLeftText = rtl;
                bodyLabel.alignment = rtl
                    ? TMPro.TextAlignmentOptions.TopRight
                    : TMPro.TextAlignmentOptions.TopLeft;
            }
            if (keyChipLabel != null) keyChipLabel.text = s.keyChip ?? string.Empty;
            if (keyCaptionLabel != null)
            {
                keyCaptionLabel.text = rtl ? ArabicTextShaper.Shape(caption) : caption;
                keyCaptionLabel.isRightToLeftText = rtl;
            }
            if (progressLabel != null) progressLabel.text = progress;

            if (keyChipLabel != null) keyChipLabel.gameObject.SetActive(!string.IsNullOrEmpty(s.keyChip));
            if (keyCaptionLabel != null) keyCaptionLabel.gameObject.SetActive(!string.IsNullOrEmpty(caption));

            // Localize the Next + Skip button labels in the same pass.
            if (loc != null)
            {
                SetButtonLabel(nextButton, loc.Get("onboarding.next"), rtl);
                SetButtonLabel(skipButton, loc.Get("onboarding.skip"), rtl);
            }

            _expectingInput = !string.IsNullOrEmpty(s.expects);
        }

        private static void SetButtonLabel(Button b, string s, bool rtl)
        {
            if (b == null) return;
            var t = b.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (t == null) return;
            t.text = rtl ? ArabicTextShaper.Shape(s) : s;
            t.isRightToLeftText = rtl;
        }

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

        // Phase 32.20 emoji-decorated headlines live in the loc tables now
        // so both English and Arabic players see the same warm glyphs
        // around their language's translated copy.
        // Phase 60 — Every step also carries a locKey* triplet so a
        // translator can change every word without touching this file.
        // The English `headline` / `body` / `keyCaption` strings serve
        // as the source-of-truth fallback when the LocalizationService
        // isn't up yet (e.g. headless EditMode test).
        private static Step[] DefaultSteps() => new[]
        {
            new Step {
                locKeyHeadline = "onboarding.step1.headline",
                locKeyBody     = "onboarding.step1.body",
                headline = "🪔  Welcome to the Hollow",
                body = "You inherit a memory-brokerage shop in a small autumnal village.\n\n" +
                       "Some memories want to be sold. Some don't.\n" +
                       "There is no combat. There are no failure screens. Only choices.",
                keyChip = "",
                keyCaption = "",
                expects = ""
            },
            new Step {
                locKeyHeadline = "onboarding.step2.headline",
                locKeyBody     = "onboarding.step2.body",
                locKeyCaption  = "onboarding.step2.caption",
                headline = "🚶  Move with WASD",
                body = "Walk through the village. Take your time — the lanterns hush, " +
                       "the leaves rustle, and someone is waiting for you at the door of the Hollow.",
                keyChip = "WASD",
                keyCaption = "or Arrow Keys / Left Stick",
                expects = "press_wasd"
            },
            new Step {
                locKeyHeadline = "onboarding.step3.headline",
                locKeyBody     = "onboarding.step3.body",
                locKeyCaption  = "onboarding.step3.caption",
                headline = "✋  Interact with E",
                body = "Look for soft golden prompts above doorways, workbenches, and " +
                       "the orbs villagers entrust to you. Press E (or the gamepad south button) " +
                       "to act on what you see.",
                keyChip = "E",
                keyCaption = "Interact",
                expects = ""
            },
            new Step {
                locKeyHeadline = "onboarding.step4.headline",
                locKeyBody     = "onboarding.step4.body",
                locKeyCaption  = "onboarding.step4.caption",
                headline = "✨  Polish memories with slow circles",
                body = "When a villager hands you a memory orb, hold the left mouse button " +
                       "and draw slow circles across its surface. Slower is better. " +
                       "Cover every side — there are four faces, like a kindness with four corners.",
                keyChip = "LMB",
                keyCaption = "Hold + draw slow circles",
                expects = ""
            },
            new Step {
                locKeyHeadline = "onboarding.step5.headline",
                locKeyBody     = "onboarding.step5.body",
                locKeyCaption  = "onboarding.step5.caption",
                headline = "🕯  Comfort tools",
                body = "Press <b>Esc</b> to pause any time. From there you can open Settings — " +
                       "Gentle Mode softens the harder moments, and any mini-game can be " +
                       "auto-completed if you'd rather skip the tactile beat.\n\n" +
                       "Press <b>H</b> any time to see the full controls.",
                keyChip = "Esc",
                keyCaption = "Pause · Settings · Comfort",
                expects = ""
            },
            new Step {
                locKeyHeadline = "onboarding.step6.headline",
                locKeyBody     = "onboarding.step6.body",
                headline = "🍂  You're ready",
                body = "Walk to the door of the Hollow when you're ready.\n\n" +
                       "There is no wrong way to keep a memory.\nThere is only the gentle way, " +
                       "and the others.\n\n— Marin",
                keyChip = "",
                keyCaption = "",
                expects = ""
            },
        };
    }
}
