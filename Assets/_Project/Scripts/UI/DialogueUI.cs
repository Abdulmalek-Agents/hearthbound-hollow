// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / DialogueUI
//
// Renders dialogue lines in the Bamao parchment box with portrait + typewriter
// effect + up-to-4 choice scrolls. Decoupled from Yarn Spinner via an
// abstract `IDialoguePresenter` — the YarnVillageStateBridge in the Dialogue
// asmdef adapts Yarn calls onto this presenter.

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
            if (root != null) root.SetActive(false);
        }

        public void PresentLine(string speaker, string text, Sprite portrait)
        {
            if (root != null) root.SetActive(true);
            if (speakerName != null) speakerName.text = speaker ?? string.Empty;
            if (portraitImage != null)
            {
                portraitImage.sprite = portrait;
                portraitImage.color = portrait != null ? Color.white : new Color(1, 1, 1, 0);
            }
            ClearChoices();
            if (_typeCoroutine != null) StopCoroutine(_typeCoroutine);
            _typeCoroutine = StartCoroutine(TypeCoroutine(text));
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
            if (root != null) root.SetActive(false);
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
