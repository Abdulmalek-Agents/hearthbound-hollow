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

        private Coroutine _typeCoroutine;
        private Action<int> _choiceCallback;
        private readonly List<GameObject> _spawnedButtons = new();

        public bool IsBusy { get; private set; }

        private void Awake()
        {
            // Hide only the visual panel — never the script-host.
            if (root != null && root != gameObject) root.SetActive(false);

            // Phase 29 — defensive UI polish: force word-wrap + auto-size on
            // the line text and speaker name so even legacy prefabs that
            // pre-date Phase 14's polish layer don't clip long villager lines.
            UIAutoFitText.ApplyToLabel(lineText, minSize: 16, maxSize: 28);
            UIAutoFitText.ApplyToButtonLabel(speakerName, minSize: 18, maxSize: 32);
        }

        public void PresentLine(string speaker, string text, Sprite portrait)
        {
            // Self-heal in case the host was deactivated externally.
            if (!gameObject.activeSelf) gameObject.SetActive(true);

            if (root != null) root.SetActive(true);
            if (speakerName != null) speakerName.text = speaker ?? string.Empty;
            if (portraitImage != null)
            {
                portraitImage.sprite = portrait;
                portraitImage.color = portrait != null ? Color.white : new Color(1, 1, 1, 0);
            }
            ClearChoices();
            if (_typeCoroutine != null) StopCoroutine(_typeCoroutine);

            if (gameObject.activeInHierarchy && isActiveAndEnabled)
            {
                _typeCoroutine = StartCoroutine(TypeCoroutine(text));
            }
            else
            {
                // Defensive fallback — render full line without typewriter.
                if (lineText != null) lineText.text = text;
                IsBusy = false;
                Hh.Warn(LogCategory.UI,
                    "DialogueUI.PresentLine called while inactive-in-hierarchy. " +
                    "Rendered full line without typewriter.");
            }
        }

        public void PresentChoices(IReadOnlyList<string> choices, Action<int> onChoiceSelected)
        {
            _choiceCallback = onChoiceSelected;
            ClearChoices();
            if (choiceContainer == null || choiceButtonPrefab == null) return;
            for (int i = 0; i < choices.Count; i++)
            {
                int idx = i;
                var go = Instantiate(choiceButtonPrefab, choiceContainer);
                go.SetActive(true); // template prefab may have been inactive
                var label = go.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null) label.text = choices[i];
                var btn = go.GetComponent<Button>();
                if (btn != null) btn.onClick.AddListener(() => HandleChoice(idx));
                _spawnedButtons.Add(go);
            }
        }

        public void Hide()
        {
            if (_typeCoroutine != null) { StopCoroutine(_typeCoroutine); _typeCoroutine = null; }
            ClearChoices();
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
    }
}
