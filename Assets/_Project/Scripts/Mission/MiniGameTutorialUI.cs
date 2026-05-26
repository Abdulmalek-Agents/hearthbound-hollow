// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / MiniGameTutorialUI
//
// PHASE 34 (2026-05-25) — fixes the user-reported "game stuck after
// 'Doris stands back and watches'" issue.
//
// ROOT CAUSE
// ──────────
// After the dialogue line "(stands back and watches)" dismisses, the
// PolishMiniGame starts silently. There is NO on-screen instruction
// telling the player to hover the orb, hold LMB, and draw circles.
// After a few seconds of nothing visibly happening, players conclude
// "the game is stuck" and quit.
//
// THE FIX
// ───────
// This component is a runtime overlay that auto-creates an on-canvas
// instruction card the moment ANY MiniGameBase subclass calls
// OnGameStarted. The card shows:
//
//   • A clear headline ("Polish the memory" / "Cleanse the cracks")
//   • Step-by-step instructions ("Hold left mouse · Draw slow circles · Cover all sides")
//   • A live clarity / progress bar
//   • A "Skip — Auto-Complete" button (cozy GDD requires this; per
//     Codex 06 every mini-game must have an auto-complete escape hatch)
//   • A friction warning when the player drags too fast ("Slower is
//     better — let the orb hear you")
//
// USAGE
// ─────
// Drop this component on the same GameObject as the PolishMiniGame /
// CleanseMiniGame (or anywhere in the scene). It auto-discovers all
// MiniGameBase instances and hooks their events. No Inspector setup
// required — but every field IS exposed so a Phase 34 capstone or
// designer can override the visuals later.
//
// The component auto-builds its own Canvas + Image + TMP labels if
// they're not assigned. Falls back to a sensible default look (warm
// parchment colour palette matching the rest of Hearthbound Hollow).

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HearthboundHollow.Core;
using HearthboundHollow.Memory;     // CleanseOutcome enum lives here (Phase 34.1 fix)
using HearthboundHollow.MiniGames;
using HearthboundHollow.Player;
using HearthboundHollow.UI;

namespace HearthboundHollow.Mission
{
    [DisallowMultipleComponent]
    public class MiniGameTutorialUI : MonoBehaviour
    {
        // ───── Inspector ──────────────────────────────────────────

        [Header("Auto-discovery (filled if left empty)")]
        [Tooltip("If empty, all MiniGameBase components in the scene are " +
                 "auto-subscribed at runtime.")]
        public MiniGameBase[] watchedGames;

        [Header("UI references (auto-built if missing)")]
        public Canvas hostCanvas;
        public GameObject panelRoot;
        public TextMeshProUGUI headlineLabel;
        public TextMeshProUGUI instructionsLabel;
        public TextMeshProUGUI percentLabel;
        public TextMeshProUGUI hintLabel;
        public Slider progressBar;
        public Button autoCompleteButton;
        public TextMeshProUGUI autoCompleteButtonLabel;

        [Header("Behaviour")]
        [Tooltip("Show the Auto-Complete button immediately on game start. " +
                 "If false, the button only appears after `autoCompleteUnlockDelay` " +
                 "seconds (gentle nudge for players who get stuck).")]
        public bool autoCompleteAlwaysVisible = true;

        [Tooltip("Seconds before the Auto-Complete button fades in when " +
                 "`autoCompleteAlwaysVisible` is false.")]
        [Range(5f, 90f)]
        public float autoCompleteUnlockDelay = 25f;

        [Tooltip("Seconds before the friction warning fades out.")]
        [Range(0.5f, 5f)]
        public float frictionWarningDuration = 1.5f;

        [Header("Colours (cozy parchment palette)")]
        public Color panelBg       = new Color(0.10f, 0.08f, 0.06f, 0.78f);
        public Color goldHeadline  = new Color(0.97f, 0.85f, 0.62f, 1f);
        public Color creamBody     = new Color(0.92f, 0.88f, 0.78f, 1f);
        public Color frictionColor = new Color(1f,    0.74f, 0.40f, 1f);
        public Color buttonBg      = new Color(0.18f, 0.13f, 0.09f, 0.92f);

        [Header("Instruction strings")]
        public string polishHeadline      = "Polish the memory";
        [TextArea(2, 4)]
        public string polishInstructions  = "Hold <b>Left Mouse</b> · Draw <b>slow circles</b> · Cover every side of the orb";
        public string cleanseHeadline     = "Cleanse the orb";
        [TextArea(2, 4)]
        public string cleanseInstructions = "Hold <b>Left Mouse</b> · Trace the cracks · <b>Don't cross the core</b>";
        public string genericHeadline     = "Work the orb";
        [TextArea(2, 4)]
        public string genericInstructions = "Hold <b>Left Mouse</b> and move slowly across the orb.";

        // ───── Internals ──────────────────────────────────────────

        private MiniGameBase _active;
        private Coroutine _autoCompleteUnlockCoroutine;
        private Coroutine _frictionWarningCoroutine;
        private string _baseHintText;

        // ───── Lifecycle ──────────────────────────────────────────

        private void Awake()
        {
            BuildUIIfMissing();
            HideImmediate();
        }

        private void OnEnable()
        {
            DiscoverAndSubscribeAll();
        }

        private void OnDisable()
        {
            UnsubscribeAll();
        }

        // ───── Auto-discovery ────────────────────────────────────

        private void DiscoverAndSubscribeAll()
        {
            if (watchedGames == null || watchedGames.Length == 0)
            {
                // Find every MiniGameBase in the scene (Polish + Cleanse).
                #if UNITY_2022_3_OR_NEWER
                watchedGames = Object.FindObjectsByType<MiniGameBase>(FindObjectsSortMode.None);
                #else
                watchedGames = Object.FindObjectsOfType<MiniGameBase>();
                #endif
            }
            if (watchedGames == null) return;
            for (int i = 0; i < watchedGames.Length; i++)
            {
                var g = watchedGames[i];
                if (g == null) continue;
                g.OnGameStarted  += OnGameStarted;
                g.OnGameFinished += OnGameFinished;

                // Polish-specific events for the friction warning + live %.
                if (g is PolishMiniGame pg)
                {
                    pg.OnClarityChanged   += OnPolishClarityChanged;
                    pg.OnFrictionWarning  += OnPolishFrictionWarning;
                    pg.OnMilestoneReached += OnPolishMilestone;
                    pg.OnRevealReached    += OnPolishReveal;
                }
                if (g is CleanseMiniGame cg)
                {
                    cg.OnCrackSealed              += OnCleanseCrackSealed;
                    cg.OnCoreWarning              += OnCleanseCoreWarning;
                    cg.OnCleanseOutcomeDetermined += OnCleanseOutcomeDetermined;
                }
            }
        }

        private void UnsubscribeAll()
        {
            if (watchedGames == null) return;
            for (int i = 0; i < watchedGames.Length; i++)
            {
                var g = watchedGames[i];
                if (g == null) continue;
                g.OnGameStarted  -= OnGameStarted;
                g.OnGameFinished -= OnGameFinished;
                if (g is PolishMiniGame pg)
                {
                    pg.OnClarityChanged   -= OnPolishClarityChanged;
                    pg.OnFrictionWarning  -= OnPolishFrictionWarning;
                    pg.OnMilestoneReached -= OnPolishMilestone;
                    pg.OnRevealReached    -= OnPolishReveal;
                }
                if (g is CleanseMiniGame cg)
                {
                    cg.OnCrackSealed              -= OnCleanseCrackSealed;
                    cg.OnCoreWarning              -= OnCleanseCoreWarning;
                    cg.OnCleanseOutcomeDetermined -= OnCleanseOutcomeDetermined;
                }
            }
        }

        // ───── Event handlers ────────────────────────────────────

        private void OnGameStarted(MiniGameBase game)
        {
            _active = game;

            // Configure the labels per game type.
            string headline = genericHeadline;
            string instructions = genericInstructions;
            _baseHintText = "Take your time — the orb cannot break from gentleness.";

            if (game is PolishMiniGame)
            {
                headline = polishHeadline;
                instructions = polishInstructions;
                _baseHintText = "Slower is better. Cover all four corners.";
            }
            else if (game is CleanseMiniGame)
            {
                headline = cleanseHeadline;
                instructions = cleanseInstructions;
                _baseHintText = "Trace each crack from end to end. Avoid the warm centre.";
            }

            if (headlineLabel != null)     headlineLabel.text     = headline;
            if (instructionsLabel != null) instructionsLabel.text = instructions;
            if (hintLabel != null)         hintLabel.text         = _baseHintText;
            if (percentLabel != null)      percentLabel.text      = "0%";
            if (progressBar != null)
            {
                progressBar.minValue = 0;
                progressBar.maxValue = 1;
                progressBar.value = 0;
            }

            // Auto-complete button visibility.
            if (autoCompleteButton != null)
            {
                bool shouldShow = autoCompleteAlwaysVisible || IsAutoCompletePreferenceOn();
                autoCompleteButton.gameObject.SetActive(shouldShow);

                // Schedule the gentle-nudge unlock for stuck players when
                // not always-visible.
                if (_autoCompleteUnlockCoroutine != null) StopCoroutine(_autoCompleteUnlockCoroutine);
                if (!shouldShow && autoCompleteUnlockDelay > 0f && gameObject.activeInHierarchy)
                    _autoCompleteUnlockCoroutine = StartCoroutine(RevealAutoCompleteAfterDelay());
            }

            Show();
            Hh.Log(LogCategory.UI, $"MiniGameTutorialUI: shown for '{game.GetType().Name}'.");
        }

        private void OnGameFinished(MiniGameBase game)
        {
            if (_active == game) _active = null;
            if (_autoCompleteUnlockCoroutine != null) StopCoroutine(_autoCompleteUnlockCoroutine);
            if (_frictionWarningCoroutine != null) StopCoroutine(_frictionWarningCoroutine);
            HideImmediate();
        }

        private void OnPolishClarityChanged(float clarity)
        {
            if (progressBar != null) progressBar.value = clarity;
            if (percentLabel != null) percentLabel.text = $"{Mathf.RoundToInt(clarity * 100f)}%";
        }

        private void OnPolishMilestone()
        {
            if (hintLabel != null) hintLabel.text = "<b>Halfway</b> — the orb is warming.";
        }

        private void OnPolishReveal()
        {
            if (hintLabel != null) hintLabel.text = "<b>Nearly there</b> — keep the circles gentle.";
        }

        private void OnPolishFrictionWarning()
        {
            if (_frictionWarningCoroutine != null) StopCoroutine(_frictionWarningCoroutine);
            if (gameObject.activeInHierarchy)
                _frictionWarningCoroutine = StartCoroutine(FlashFrictionWarning());
        }

        private void OnCleanseCrackSealed(int sealedCount)
        {
            if (hintLabel != null) hintLabel.text = $"<b>{sealedCount} of 4</b> cracks sealed.";
            if (progressBar != null) progressBar.value = Mathf.Clamp01(sealedCount / 4f);
            if (percentLabel != null) percentLabel.text = $"{Mathf.Clamp(sealedCount, 0, 4)} / 4";
        }

        private void OnCleanseCoreWarning()
        {
            if (_frictionWarningCoroutine != null) StopCoroutine(_frictionWarningCoroutine);
            if (gameObject.activeInHierarchy)
                _frictionWarningCoroutine = StartCoroutine(FlashCoreWarning());
        }

        private void OnCleanseOutcomeDetermined(CleanseOutcome _) { /* leave hint as last sealed-count */ }

        // ───── Auto-complete handler ─────────────────────────────

        private void OnAutoCompleteClicked()
        {
            if (_active == null) return;
            Hh.Log(LogCategory.UI, $"MiniGameTutorialUI: Auto-Complete clicked on '{_active.GetType().Name}'.");
            _active.DoAutoComplete();
            if (autoCompleteButton != null) autoCompleteButton.interactable = false;
        }

        // ───── Visibility helpers ────────────────────────────────

        private void Show()
        {
            if (panelRoot != null) panelRoot.SetActive(true);
        }

        private void HideImmediate()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            if (autoCompleteButton != null) autoCompleteButton.interactable = true;
        }

        private IEnumerator RevealAutoCompleteAfterDelay()
        {
            yield return new WaitForSecondsRealtime(autoCompleteUnlockDelay);
            if (autoCompleteButton != null) autoCompleteButton.gameObject.SetActive(true);
            if (hintLabel != null && !string.IsNullOrEmpty(_baseHintText))
                hintLabel.text = _baseHintText + "  (Auto-Complete is available below.)";
        }

        private IEnumerator FlashFrictionWarning()
        {
            if (hintLabel == null) yield break;
            var orig = hintLabel.color;
            hintLabel.color = frictionColor;
            hintLabel.text = "<b>Slower</b> — let the orb hear you.";
            yield return new WaitForSecondsRealtime(frictionWarningDuration);
            hintLabel.color = orig;
            if (!string.IsNullOrEmpty(_baseHintText)) hintLabel.text = _baseHintText;
        }

        private IEnumerator FlashCoreWarning()
        {
            if (hintLabel == null) yield break;
            var orig = hintLabel.color;
            hintLabel.color = frictionColor;
            hintLabel.text = "<b>Pull back</b> — that is the warm centre.";
            yield return new WaitForSecondsRealtime(frictionWarningDuration);
            hintLabel.color = orig;
            if (!string.IsNullOrEmpty(_baseHintText)) hintLabel.text = _baseHintText;
        }

        private static bool IsAutoCompletePreferenceOn()
        {
            var s = ServiceLocator.Get<SettingsService>();
            return s != null && (s.AutoCompletePolish || s.AutoCompleteCleanse || s.GentleMode);
        }

        // ───── UI auto-build ─────────────────────────────────────

        private void BuildUIIfMissing()
        {
            // If the user wired the visuals in the Inspector, do nothing.
            if (panelRoot != null && headlineLabel != null && instructionsLabel != null
                && progressBar != null && autoCompleteButton != null) return;

            // Find or create a Canvas to host the overlay.
            if (hostCanvas == null)
            {
                #if UNITY_2022_3_OR_NEWER
                var anyCanvas = Object.FindFirstObjectByType<Canvas>();
                #else
                var anyCanvas = Object.FindObjectOfType<Canvas>();
                #endif
                hostCanvas = anyCanvas != null ? anyCanvas : CreateOverlayCanvas();
            }

            // Two-layer pattern (D-033) — host always active, panelRoot toggled.
            var host = new GameObject("MiniGameTutorial_Host", typeof(RectTransform));
            host.transform.SetParent(hostCanvas.transform, false);
            var hostRT = host.GetComponent<RectTransform>();
            hostRT.anchorMin = Vector2.zero; hostRT.anchorMax = Vector2.one;
            hostRT.offsetMin = Vector2.zero; hostRT.offsetMax = Vector2.zero;

            // Visual panel — anchored top-centre.
            var panel = new GameObject("Visual", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(host.transform, false);
            var panelImg = panel.GetComponent<Image>();
            panelImg.color = panelBg;
            var pRT = panel.GetComponent<RectTransform>();
            pRT.anchorMin = new Vector2(0.18f, 0.78f);
            pRT.anchorMax = new Vector2(0.82f, 0.96f);
            pRT.offsetMin = Vector2.zero; pRT.offsetMax = Vector2.zero;
            panelRoot = panel;

            // Headline (top)
            headlineLabel = MakeLabel(panel.transform, "Headline",
                new Vector2(0.02f, 0.70f), new Vector2(0.98f, 0.98f),
                fontSize: 28, color: goldHeadline, bold: true,
                align: TextAlignmentOptions.Center);

            // Instructions (middle)
            instructionsLabel = MakeLabel(panel.transform, "Instructions",
                new Vector2(0.04f, 0.36f), new Vector2(0.96f, 0.70f),
                fontSize: 18, color: creamBody, bold: false,
                align: TextAlignmentOptions.Center);
            instructionsLabel.enableWordWrapping = true;

            // Hint (between instructions and bar) — also used for friction warnings
            hintLabel = MakeLabel(panel.transform, "Hint",
                new Vector2(0.04f, 0.20f), new Vector2(0.96f, 0.36f),
                fontSize: 14, color: creamBody, bold: false,
                align: TextAlignmentOptions.Center);
            hintLabel.fontStyle = FontStyles.Italic;

            // Progress bar
            var barGO = new GameObject("ProgressBar", typeof(RectTransform), typeof(Slider));
            barGO.transform.SetParent(panel.transform, false);
            var barRT = barGO.GetComponent<RectTransform>();
            barRT.anchorMin = new Vector2(0.08f, 0.08f);
            barRT.anchorMax = new Vector2(0.74f, 0.18f);
            barRT.offsetMin = Vector2.zero; barRT.offsetMax = Vector2.zero;
            progressBar = barGO.GetComponent<Slider>();
            progressBar.minValue = 0; progressBar.maxValue = 1; progressBar.value = 0;
            BuildSliderVisuals(progressBar);

            // % label (next to bar)
            percentLabel = MakeLabel(panel.transform, "Percent",
                new Vector2(0.76f, 0.06f), new Vector2(0.94f, 0.20f),
                fontSize: 16, color: goldHeadline, bold: true,
                align: TextAlignmentOptions.MidlineRight);

            // Auto-complete button
            autoCompleteButton = MakeButton(panel.transform, "Btn_AutoComplete", "Skip · Auto-Complete",
                new Vector2(0.30f, -0.16f), new Vector2(0.70f, -0.04f));
            autoCompleteButtonLabel = autoCompleteButton.GetComponentInChildren<TextMeshProUGUI>(true);
            autoCompleteButton.onClick.AddListener(OnAutoCompleteClicked);

            // Hide until a mini-game starts.
            panel.SetActive(false);
        }

        private static Canvas CreateOverlayCanvas()
        {
            var go = new GameObject("MiniGameTutorial_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var c = go.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 45; // above ControlHintsHUD (40), below Pause/Help (50)
            var s = go.GetComponent<CanvasScaler>();
            s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            s.referenceResolution = new Vector2(1920, 1080);
            return c;
        }

        private TextMeshProUGUI MakeLabel(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax,
            int fontSize, Color color, bool bold, TextAlignmentOptions align)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = align;
            tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            var rt = tmp.rectTransform;
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            UIAutoFitText.ApplyToLabel(tmp, minSize: Mathf.Max(8, fontSize - 6), maxSize: fontSize + 2);
            return tmp;
        }

        private Button MakeButton(Transform parent, string name, string text,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = buttonBg;
            var rt = img.rectTransform;
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            var label = MakeLabel(go.transform, "Label",
                Vector2.zero, Vector2.one,
                fontSize: 16, color: goldHeadline, bold: true,
                align: TextAlignmentOptions.Center);
            label.text = text;

            return go.GetComponent<Button>();
        }

        private static void BuildSliderVisuals(Slider slider)
        {
            // Background
            var bgGO = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgGO.transform.SetParent(slider.transform, false);
            var bgImg = bgGO.GetComponent<Image>();
            bgImg.color = new Color(0.05f, 0.04f, 0.03f, 0.78f);
            var bgRT = bgGO.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero;

            // Fill area
            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(slider.transform, false);
            var faRT = fillArea.GetComponent<RectTransform>();
            faRT.anchorMin = new Vector2(0f, 0.20f); faRT.anchorMax = new Vector2(1f, 0.80f);
            faRT.offsetMin = new Vector2(4, 0); faRT.offsetMax = new Vector2(-4, 0);

            // Fill
            var fillGO = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillGO.transform.SetParent(fillArea.transform, false);
            var fillImg = fillGO.GetComponent<Image>();
            fillImg.color = new Color(0.92f, 0.70f, 0.34f, 1f);
            var fillRT = fillGO.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero; fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = Vector2.zero; fillRT.offsetMax = Vector2.zero;

            slider.fillRect = fillRT;
            slider.targetGraphic = bgImg;
            slider.handleRect = null; // no draggable handle — this is a read-only progress bar
            slider.interactable = false;
            slider.direction = Slider.Direction.LeftToRight;
        }
    }
}
