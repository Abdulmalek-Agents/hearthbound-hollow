// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / DialogueUI
//
// Renders dialogue lines in the Bamao parchment box with portrait + typewriter
// effect + up-to-4 choice scrolls. Decoupled from Yarn Spinner via an
// abstract `IDialoguePresenter` — the YarnVillageStateBridge in the Dialogue
// asmdef adapts Yarn calls onto this presenter.
//
// ── Phase 60 — Arabic Localization MVP ────────────────────────────
// PresentLine now routes through LocalizationService:
//   1. Speaker name → GetSpeakerName("Doris") → "دوريس" in Arabic.
//   2. Line text   → GetDialogue(lineId, englishOriginal) → ar translation.
//   3. RTL locales → text shaped through ArabicTextShaper for TMP.
//   4. lineText + speakerName alignment + isRightToLeftText switch direction.
// Choices list shaped + index prefix mirrored for RTL.
// Advance prompt ("Click or [Space] >" / "انقر أو [Space] >") localized via
// `hud.advance_prompt` key. See Docs/LOCALIZATION_GUIDE.md § 5.

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HearthboundHollow.Audio;
using HearthboundHollow.Core;

namespace HearthboundHollow.UI
{
    public interface IDialoguePresenter
    {
        void PresentLine(string speakerName, string lineText, Sprite portrait);
        void PresentLine(string speakerName, string lineText, Sprite portrait, string lineId);
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
        public TextMeshProUGUI advancePrompt;

        [Header("Pickle styling (playtest pass commit 5/6)")]
        public string pickleSpeakerName = "Pickle";
        public Color pickleSpeakerColor = new Color(0.69f, 0.49f, 0.21f, 1f);
        public Color pickleLineColor = new Color(0.54f, 0.36f, 0.12f, 0.78f);

        private Coroutine _typeCoroutine;
        private Action<int> _choiceCallback;
        private readonly List<GameObject> _spawnedButtons = new();
        private string _fullLineText;
        private bool _pickleStyleActive;
        private Color _defaultSpeakerColor = Color.white;
        private Color _defaultLineColor = Color.white;
        private FontStyles _defaultLineFontStyle = FontStyles.Normal;
        private FontStyles _defaultSpeakerFontStyle = FontStyles.Normal;
        private bool _defaultColorsCached;
        private string _lastSpeakerId;

        public bool IsBusy { get; private set; }

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
            if (advancePrompt != null)
            {
                if (!string.IsNullOrEmpty(advancePrompt.text) && advancePrompt.text.Contains("▸"))
                    advancePrompt.text = advancePrompt.text.Replace("▸", ">");
                advancePrompt.gameObject.SetActive(false);
            }

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
            advancePrompt.text = "Click or [Space] >";
            advancePrompt.fontSize = 18;
            advancePrompt.fontStyle = FontStyles.Italic;
            advancePrompt.alignment = TextAlignmentOptions.MidlineRight;
            advancePrompt.color = new Color(0.42f, 0.24f, 0.10f, 0.85f);
            advancePrompt.raycastTarget = false;
            UIAutoFitText.ApplyToButtonLabel(advancePrompt, minSize: 12, maxSize: 20);
        }

        public void PresentLine(string speaker, string text, Sprite portrait)
            => PresentLine(speaker, text, portrait, null);

        /// <summary>
        /// Phase 32 — Voice Acting MVP overload.
        ///
        /// Phase 60 — Arabic Localization MVP. Before rendering, the line is
        /// run through the LocalizationService:
        ///   1. Speaker → GetSpeakerName ("Doris" → "دوريس").
        ///   2. Line text → GetDialogue(lineId, englishOriginal).
        ///   3. RTL shape via ArabicTextShaper.
        ///   4. TMP isRightToLeftText + alignment switched per locale.
        /// VoicePlayer.Play(lineId) is locale-aware on the audio side
        /// (Arabic clipAr → English clip fallback).
        /// </summary>
        public void PresentLine(string speaker, string text, Sprite portrait, string lineId)
        {
            // Phase 60 — Resolve the active locale once per line. The service
            // is registered before any scene Awake (LocalizationBootstrap) so
            // this is effectively never null at runtime; we still guard for
            // the EditMode test case.
            var loc = ServiceLocator.Get<LocalizationService>();
            if (loc != null)
            {
                text = loc.GetDialogue(lineId, text);
                speaker = loc.GetSpeakerName(speaker);
                if (loc.IsRightToLeft && !string.IsNullOrEmpty(text))
                    text = ArabicTextShaper.Shape(text);
                if (loc.IsRightToLeft && !string.IsNullOrEmpty(speaker))
                    speaker = ArabicTextShaper.Shape(speaker);
                ApplyTextDirection(loc.IsRightToLeft);
            }

            if (!gameObject.activeSelf) gameObject.SetActive(true);
            if (root != null) root.SetActive(true);
            ApplyVisibility(true);

            if (lineText != null && !lineText.gameObject.activeSelf)
                lineText.gameObject.SetActive(true);

            CacheDefaultStyles();
            bool isPickleLine = !string.IsNullOrEmpty(speaker) &&
                speaker.Trim().Equals(pickleSpeakerName, StringComparison.OrdinalIgnoreCase);

            ApplyPickleStyle(isPickleLine);

            if (speakerName != null) speakerName.text = speaker ?? string.Empty;
            if (portraitImage != null)
            {
                if (isPickleLine)
                    portraitImage.color = new Color(1, 1, 1, 0);
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

            float clipLen = VoicePlayer.Instance != null ? VoicePlayer.Instance.Play(lineId) : 0f;
            int targetCps = charsPerSecond;
            if (clipLen > 0.4f && !string.IsNullOrEmpty(text))
                targetCps = Mathf.Clamp(Mathf.RoundToInt(text.Length / clipLen), 18, 90);

            float estimatedDur = clipLen > 0.4f
                ? clipLen + postLineLinger
                : ComputeTypewriterDuration(_fullLineText, targetCps);
            _lastSpeakerId = (speaker ?? string.Empty).Trim().ToLowerInvariant();
            EventBus.Publish(new DialogueLineStartedEvent(
                _lastSpeakerId,
                _fullLineText,
                estimatedDur,
                hasVoiceClip: clipLen > 0f));

            if (gameObject.activeInHierarchy && isActiveAndEnabled)
            {
                _typeCoroutine = StartCoroutine(TypeCoroutine(text, targetCps));
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

        private float ComputeTypewriterDuration(string text) => ComputeTypewriterDuration(text, charsPerSecond);

        private float ComputeTypewriterDuration(string text, int cps)
        {
            if (string.IsNullOrEmpty(text)) return 0.3f;
            int useCps = Mathf.Max(1, cps);
            return text.Length / (float)useCps + postLineLinger;
        }

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
        /// Phase 60 — Apply RTL text direction + alignment to the dialogue
        /// labels. Idempotent; restores LTR when flipping back to English.
        /// </summary>
        private void ApplyTextDirection(bool rtl)
        {
            if (lineText != null)
            {
                lineText.isRightToLeftText = rtl;
                lineText.alignment = rtl ? TextAlignmentOptions.TopRight : TextAlignmentOptions.TopLeft;
            }
            if (speakerName != null)
            {
                speakerName.isRightToLeftText = rtl;
                speakerName.alignment = rtl ? TextAlignmentOptions.TopRight : TextAlignmentOptions.TopLeft;
            }
        }

        public void SkipTypewriter()
        {
            if (!IsBusy || _fullLineText == null) return;
            if (_typeCoroutine != null) { StopCoroutine(_typeCoroutine); _typeCoroutine = null; }
            if (lineText != null) lineText.text = _fullLineText;
            IsBusy = false;
            VoicePlayer.Instance?.Stop();
            EventBus.Publish(new DialogueLineEndedEvent(_lastSpeakerId ?? string.Empty));
        }

        public void PresentChoices(IReadOnlyList<string> choices, Action<int> onChoiceSelected)
        {
            _choiceCallback = onChoiceSelected;
            ClearChoices();

            VoicePlayer.Instance?.Stop();

            if (!gameObject.activeSelf) gameObject.SetActive(true);
            if (root != null && !root.activeSelf) root.SetActive(true);

            ApplyPickleStyle(false);

            if (lineText != null) lineText.gameObject.SetActive(false);
            if (advancePrompt != null) advancePrompt.gameObject.SetActive(false);
            _fullLineText = null;

            if (choiceContainer == null || choiceButtonPrefab == null) return;

            DialogueChoiceLayoutHealer.HealContainer(choiceContainer);

            // Phase 60 — Resolve once per choice batch.
            var loc = ServiceLocator.Get<LocalizationService>();
            bool rtl = loc != null && loc.IsRightToLeft;

            for (int i = 0; i < choices.Count; i++)
            {
                int idx = i;
                var go = Instantiate(choiceButtonPrefab, choiceContainer);
                go.SetActive(true);
                var label = go.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                {
                    // Phase 60 — Choices come pre-localized from the Director;
                    // we just shape Arabic glyphs + flip the [N] prefix to
                    // the right side on RTL.
                    string choiceText = choices[i] ?? string.Empty;
                    if (rtl) choiceText = ArabicTextShaper.Shape(choiceText);
                    label.isRightToLeftText = rtl;
                    label.alignment = rtl ? TextAlignmentOptions.TopRight : TextAlignmentOptions.TopLeft;
                    label.text = rtl
                        ? $"{choiceText}  <b><color=#7a5314>[{idx + 1}]</color></b>"
                        : $"<b><color=#7a5314>[{idx + 1}]</color></b>  {choiceText}";
                }
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
            ApplyPickleStyle(false);
            if (lineText != null && !lineText.gameObject.activeSelf)
                lineText.gameObject.SetActive(true);
            if (advancePrompt != null) advancePrompt.gameObject.SetActive(false);
            _fullLineText = null;
            VoicePlayer.Instance?.Stop();
            EventBus.Publish(new DialogueLineEndedEvent(_lastSpeakerId ?? string.Empty));
            ApplyVisibility(false);
            IsBusy = false;
        }

        private void ApplyVisibility(bool visible)
        {
            var target = root != null ? root : gameObject;
            var cg = target.GetComponent<CanvasGroup>();
            if (cg == null) cg = target.AddComponent<CanvasGroup>();
            cg.alpha = visible ? 1f : 0f;
            cg.blocksRaycasts = visible;
            cg.interactable = visible;
            if (root != null && root != gameObject && root.activeSelf != visible)
                root.SetActive(visible);
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

        private IEnumerator TypeCoroutine(string text) => TypeCoroutine(text, charsPerSecond);

        private IEnumerator TypeCoroutine(string text, int cps)
        {
            IsBusy = true;
            if (lineText == null) { IsBusy = false; yield break; }
            lineText.text = string.Empty;
            float interval = 1f / Mathf.Max(1, cps);
            for (int i = 0; i < text.Length; i++)
            {
                lineText.text += text[i];
                yield return new WaitForSeconds(interval);
            }
            yield return new WaitForSeconds(postLineLinger);
            IsBusy = false;
            EventBus.Publish(new DialogueLineEndedEvent(_lastSpeakerId ?? string.Empty));
        }

        private void Update()
        {
            if (advancePrompt != null)
            {
                bool waiting = IsWaitingForAdvance;
                if (waiting != advancePrompt.gameObject.activeSelf)
                {
                    advancePrompt.gameObject.SetActive(waiting);
                    // Phase 60 — when the prompt becomes visible, refresh
                    // its text from the LocalizationService so the
                    // "Click or [Space] >" hint reads in Arabic too.
                    if (waiting)
                    {
                        var loc = ServiceLocator.Get<LocalizationService>();
                        if (loc != null)
                        {
                            string txt = loc.Get("hud.advance_prompt");
                            if (loc.IsRightToLeft) txt = ArabicTextShaper.Shape(txt);
                            advancePrompt.text = txt;
                            advancePrompt.isRightToLeftText = loc.IsRightToLeft;
                            advancePrompt.alignment = loc.IsRightToLeft
                                ? TextAlignmentOptions.MidlineLeft
                                : TextAlignmentOptions.MidlineRight;
                        }
                    }
                }
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
