// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / DialogueUI
//
// Renders dialogue lines in the Bamao parchment box with portrait + typewriter
// effect + up-to-4 choice scrolls. Decoupled from Yarn Spinner via an
// abstract `IDialoguePresenter` — the YarnVillageStateBridge in the Dialogue
// asmdef adapts Yarn calls onto this presenter.
//
// ── Phase 25 hotfix ─────────────────────────────────────────────
// PresentLine() now self-activates the script-host before running the
// typewriter coroutine. Same defensive pattern applied across all UI
// overlays in this release.
//
// ── Playtest pass fix (commit 5/6) ──────────────────────────────
// QA simulated-playthrough audit found Pickle's 13 lines (4 canonical +
// 4 conditional + 3 contextual + 2 hints) were being rendered IDENTICALLY
// to Doris and Gerrold — same Bamao parchment box, same regular font.
// Per:
//   - Yarn STYLE_GUIDE.md § 2.5 (Dual-mode dialogue rendering)
//   - Mission 2 Guide § 14.2 (Pickle's pre-choice commentary)
//   - Codex 07 § 3.1 rule 7 (Pickle's lines are INTERNAL — only the
//     player hears her)
// Pickle's lines should render:
//   • Italic font
//   • Lower opacity (~75%)
//   • NO portrait
//   • Speaker name dimmed to amber
//   • Subtle leitmotif tag visible
//
// FIX: PresentLine now detects speakerName == "Pickle" and routes through
// the italic-mode visual. All other speakers (Doris, Gerrold, etc.)
// render in the default Bamao parchment style — UNCHANGED.

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HearthboundHollow.Core;

namespace HearthboundHollow.UI
{
    public interface IDialoguePresenter
    {
        void PresentLine(string speakerName, string lineText, Sprite portrait);
        void PresentChoices(IReadOnlyList<string> choices, Action<int> onChoiceSelected);
        void Hide();
        bool IsBusy { get; }
    }

    public class DialogueUI : MonoBehaviour, IDialoguePresenter
    {
        [Header("Root")]
        public GameObject root;

        [Header("Portrait")]
        public Image portraitImage;

        [Header("Text")]
        public TextMeshProUGUI speakerName;
        public TextMeshProUGUI lineText;

        [Header("Choices")]
        public Transform choiceContainer;
        public GameObject choiceButtonPrefab;

        [Header("Tuning")]
        [Range(20, 120)] public int charsPerSecond = 45;
        [Range(0f, 1f)] public float postLineLinger = 0.5f;

        [Header("Advance prompt (auto-created if null)")]
        [Tooltip("Pulsing 'Press [Space] to continue ▸' label that appears " +
                 "in the dialogue box's lower-right when the line is fully " +
                 "rendered and no choices are showing.")]
        public TextMeshProUGUI advancePrompt;

        [Header("Pickle styling (playtest pass commit 5/6)")]
        [Tooltip("Speaker name that triggers Pickle's italic / no-portrait / " +
                 "lower-opacity rendering mode. Case-insensitive match.")]
        public string pickleSpeakerName = "Pickle";
        [Tooltip("Color the speaker-name label tints to in Pickle mode. Warm amber.")]
        public Color pickleSpeakerColor = new Color(0.69f, 0.49f, 0.21f, 1f);
        [Tooltip("Color the line-text label tints to in Pickle mode. Dim amber.")]
        public Color pickleLineColor = new Color(0.54f, 0.36f, 0.12f, 0.78f);

        private Coroutine _typeCoroutine;
        private Action<int> _choiceCallback;
        private readonly List<GameObject> _spawnedButtons = new();
        private string _fullLineText;
        private bool _pickleStyleActive;
        // Cache the default colors so we can restore them when a non-Pickle
        // line follows a Pickle line.
        private Color _defaultSpeakerColor = Color.white;
        private Color _defaultLineColor = Color.white;
        private FontStyles _defaultLineFontStyle = FontStyles.Normal;
        private FontStyles _defaultSpeakerFontStyle = FontStyles.Normal;
        private bool _defaultColorsCached;

        public bool IsBusy { get; private set; }

        /// <summary>
        /// True while the dialogue box is visible, the typewriter is idle,
        /// and no choices are showing.
        /// </summary>
        public bool IsWaitingForAdvance =>
            (root == null || root.activeSelf) &&
            !IsBusy &&
            _spawnedButtons.Count == 0 &&
            _fullLineText != null;

        private void Awake()
        {
            if (root != null && root != gameObject) root.SetActive(false);

            UIAutoFitText.ApplyToLabel(lineText, minSize: 16, maxSize: 28);
            UIAutoFitText.ApplyToButtonLabel(speakerName, minSize: 18, maxSize: 32);

            DialogueChoiceLayoutHealer.HealContainer(choiceContainer);

            EnsureAdvancePromptExists();
            if (advancePrompt != null) advancePrompt.gameObject.SetActive(false);

            // Cache the inspector-set default colors + font styles so we can
            // restore them when a non-Pickle line follows a Pickle line.
            CacheDefaultStyles();
        }

        private void CacheDefaultStyles()
        {
            if (_defaultColorsCached) return;
            if (speakerName != null)
            {
                _defaultSpeakerColor = speakerName.color;
                _defaultSpeakerFontStyle = speakerName.fontStyle;
            }
            if (lineText != null)
            {
                _defaultLineColor = lineText.color;
                _defaultLineFontStyle = lineText.fontStyle;
            }
            _defaultColorsCached = true;
        }

        private void EnsureAdvancePromptExists()
        {
            if (advancePrompt != null) return;
            var parent = root != null ? root.transform : transform;
            var go = new GameObject("AdvancePrompt", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0.70f, 0.02f);
            rt.anchorMax = new Vector2(0.98f, 0.16f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            advancePrompt = go.AddComponent<TextMeshProUGUI>();
            advancePrompt.text = "Click or [Space] ▸";
            advancePrompt.fontSize = 18;
            advancePrompt.fontStyle = FontStyles.Italic;
            advancePrompt.alignment = TextAlignmentOptions.MidlineRight;
            advancePrompt.color = new Color(0.42f, 0.24f, 0.10f, 0.85f);
            advancePrompt.raycastTarget = false;
            UIAutoFitText.ApplyToButtonLabel(advancePrompt, minSize: 12, maxSize: 20);
        }

        public void PresentLine(string speaker, string text, Sprite portrait)
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            if (root != null) root.SetActive(true);

            if (lineText != null && !lineText.gameObject.activeSelf)
                lineText.gameObject.SetActive(true);

            // ── Playtest pass commit 5/6 — Pickle's italic / no-portrait mode ──
            // Detect Pickle speaker case-insensitively. When she speaks the
            // line renders italic + dim amber, with no portrait. Codex 07 §
            // 3.1 rule 7: her lines are internal; only the player hears her.
            CacheDefaultStyles();
            bool isPickleLine = !string.IsNullOrEmpty(speaker) &&
                speaker.Trim().Equals(pickleSpeakerName, StringComparison.OrdinalIgnoreCase);

            ApplyPickleStyle(isPickleLine);

            if (speakerName != null) speakerName.text = speaker ?? string.Empty;
            if (portraitImage != null)
            {
                if (isPickleLine)
                {
                    // Hide portrait entirely for Pickle (Style Guide § 2.5).
                    portraitImage.color = new Color(1, 1, 1, 0);
                }
                else
                {
                    portraitImage.sprite = portrait;
                    portraitImage.color = portrait != null ? Color.white : new Color(1, 1, 1, 0);
                }
            }
            ClearChoices();
            if (_typeCoroutine != null) StopCoroutine(_typeCoroutine);

            _fullLineText = text ?? string.Empty;

            if (advancePrompt != null) advancePrompt.gameObject.SetActive(false);

            if (gameObject.activeInHierarchy && isActiveAndEnabled)
            {
                _typeCoroutine = StartCoroutine(TypeCoroutine(text));
            }
            else
            {
                if (lineText != null) lineText.text = text;
                IsBusy = false;
                Hh.Warn(LogCategory.UI,
                    "DialogueUI.PresentLine called while inactive-in-hierarchy. " +
                    "Rendered full line without typewriter.");
            }
        }

        /// <summary>
        /// Apply (or revert) the Pickle italic / dim-amber / no-portrait visual.
        /// Idempotent — safe to call every PresentLine.
        /// </summary>
        private void ApplyPickleStyle(bool pickle)
        {
            if (pickle == _pickleStyleActive) return;
            _pickleStyleActive = pickle;
            if (speakerName != null)
            {
                speakerName.color = pickle ? pickleSpeakerColor : _defaultSpeakerColor;
                speakerName.fontStyle = pickle ? FontStyles.Italic : _defaultSpeakerFontStyle;
            }
            if (lineText != null)
            {
                lineText.color = pickle ? pickleLineColor : _defaultLineColor;
                lineText.fontStyle = pickle ? FontStyles.Italic : _defaultLineFontStyle;
            }
        }

        /// <summary>
        /// Immediately complete the running typewriter without advancing past it.
        /// </summary>
        public void SkipTypewriter()
        {
            if (!IsBusy || _fullLineText == null) return;
            if (_typeCoroutine != null) { StopCoroutine(_typeCoroutine); _typeCoroutine = null; }
            if (lineText != null) lineText.text = _fullLineText;
            IsBusy = false;
        }

        public void PresentChoices(IReadOnlyList<string> choices, Action<int> onChoiceSelected)
        {
            _choiceCallback = onChoiceSelected;
            ClearChoices();

            if (!gameObject.activeSelf) gameObject.SetActive(true);
            if (root != null && !root.activeSelf) root.SetActive(true);

            // Choices always render in the DEFAULT style (never Pickle italic)
            // because the player is the one choosing, not Pickle.
            ApplyPickleStyle(false);

            if (lineText != null) lineText.gameObject.SetActive(false);
            if (advancePrompt != null) advancePrompt.gameObject.SetActive(false);
            _fullLineText = null;

            if (choiceContainer == null || choiceButtonPrefab == null) return;

            DialogueChoiceLayoutHealer.HealContainer(choiceContainer);

            for (int i = 0; i < choices.Count; i++)
            {
                int idx = i;
                var go = Instantiate(choiceButtonPrefab, choiceContainer);
                go.SetActive(true);
                var label = go.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null) label.text = $"<b><color=#7a5314>[{idx + 1}]</color></b>  {choices[i]}";
                var btn = go.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => HandleChoice(idx));
                }
                _spawnedButtons.Add(go);

                DialogueChoiceLayoutHealer.HealTile(go);
            }
        }

        public void Hide()
        {
            if (_typeCoroutine != null) { StopCoroutine(_typeCoroutine); _typeCoroutine = null; }
            ClearChoices();
            // Restore default colors so subsequent shows don't inherit
            // Pickle's dim amber from a stale state.
            ApplyPickleStyle(false);
            if (lineText != null && !lineText.gameObject.activeSelf)
                lineText.gameObject.SetActive(true);
            if (advancePrompt != null) advancePrompt.gameObject.SetActive(false);
            _fullLineText = null;
            if (root != null && root != gameObject) root.SetActive(false);
            IsBusy = false;
        }

        private void ClearChoices()
        {
            foreach (var b in _spawnedButtons) if (b != null) Destroy(b);
            _spawnedButtons.Clear();
        }

        private void HandleChoice(int index)
        {
            var cb = _choiceCallback;
            _choiceCallback = null;
            ClearChoices();
            if (lineText != null && !lineText.gameObject.activeSelf)
                lineText.gameObject.SetActive(true);
            if (advancePrompt != null) advancePrompt.gameObject.SetActive(false);
            _fullLineText = null;
            cb?.Invoke(index);
        }

        private IEnumerator TypeCoroutine(string text)
        {
            IsBusy = true;
            if (lineText == null) { IsBusy = false; yield break; }
            lineText.text = string.Empty;
            float interval = 1f / Mathf.Max(1, charsPerSecond);
            for (int i = 0; i < text.Length; i++)
            {
                lineText.text += text[i];
                yield return new WaitForSeconds(interval);
            }
            yield return new WaitForSeconds(postLineLinger);
            IsBusy = false;
        }

        private void Update()
        {
            if (advancePrompt != null)
            {
                bool waiting = IsWaitingForAdvance;
                if (waiting != advancePrompt.gameObject.activeSelf)
                    advancePrompt.gameObject.SetActive(waiting);
                if (waiting)
                {
                    float t = Mathf.PingPong(Time.unscaledTime * 1.4f, 1f);
                    var c = advancePrompt.color;
                    c.a = Mathf.Lerp(0.55f, 1.0f, t);
                    advancePrompt.color = c;
                }
            }

            if (_choiceCallback == null || _spawnedButtons.Count == 0) return;
            int picked = -1;
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) picked = 0;
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) picked = 1;
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) picked = 2;
            else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) picked = 3;
            if (picked >= 0 && picked < _spawnedButtons.Count)
            {
                HandleChoice(picked);
            }
        }
    }
}
