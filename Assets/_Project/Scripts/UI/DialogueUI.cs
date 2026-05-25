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

        [Header("Advance prompt (auto-created if null)")]
        [Tooltip("Pulsing 'Press [Space] to continue ▸' label that appears " +
                 "in the dialogue box's lower-right when the line is fully " +
                 "rendered and no choices are showing. Created at runtime " +
                 "if absent so existing prefabs get the affordance without " +
                 "needing a rebuild.")]
        public TextMeshProUGUI advancePrompt;

        private Coroutine _typeCoroutine;
        private Action<int> _choiceCallback;
        private readonly List<GameObject> _spawnedButtons = new();
        private string _fullLineText;     // for click-to-skip-typewriter

        public bool IsBusy { get; private set; }

        /// <summary>
        /// True while the dialogue box is visible, the typewriter is idle,
        /// and no choices are showing. The Mission director / Yarn runner
        /// is currently blocking on an advance click. Used by the in-box
        /// prompt to know when to pulse.
        /// </summary>
        public bool IsWaitingForAdvance =>
            (root == null || root.activeSelf) &&
            !IsBusy &&
            _spawnedButtons.Count == 0 &&
            _fullLineText != null;

        private void Awake()
        {
            // Hide only the visual panel — never the script-host.
            if (root != null && root != gameObject) root.SetActive(false);

            // Phase 29 — defensive UI polish: force word-wrap + auto-size on
            // the line text and speaker name so even legacy prefabs that
            // pre-date Phase 14's polish layer don't clip long villager lines.
            UIAutoFitText.ApplyToLabel(lineText, minSize: 16, maxSize: 28);
            UIAutoFitText.ApplyToButtonLabel(speakerName, minSize: 18, maxSize: 32);

            // Phase 31 — heal the ChoicesContainer VerticalLayoutGroup so
            // tiles stretch to full width regardless of the prefab's saved
            // settings (older Phase 14 builds wrote childControlWidth = 0
            // which caused tiles to render as ~100 px squares).
            DialogueChoiceLayoutHealer.HealContainer(choiceContainer);

            // Phase 31.1 — auto-create the advance prompt if the prefab is
            // missing it. User playtest showed players didn't know they
            // had to click to advance past "(stands back and watches)" —
            // dialogue felt frozen. The visible "Press [Space] ▸" hint
            // makes the affordance unmistakable.
            EnsureAdvancePromptExists();
            if (advancePrompt != null) advancePrompt.gameObject.SetActive(false);
        }

        private void EnsureAdvancePromptExists()
        {
            if (advancePrompt != null) return;
            var parent = root != null ? root.transform : transform;
            var go = new GameObject("AdvancePrompt", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            // Bottom-right inside the dialogue box.
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
            advancePrompt.raycastTarget = false; // don't intercept clicks
            UIAutoFitText.ApplyToButtonLabel(advancePrompt, minSize: 12, maxSize: 20);
        }

        public void PresentLine(string speaker, string text, Sprite portrait)
        {
            // Self-heal in case the host was deactivated externally.
            if (!gameObject.activeSelf) gameObject.SetActive(true);

            if (root != null) root.SetActive(true);

            // Phase 31 — PresentChoices() hides lineText to free the body
            // for the choice tiles. Restore it whenever a new line arrives.
            if (lineText != null && !lineText.gameObject.activeSelf)
                lineText.gameObject.SetActive(true);

            if (speakerName != null) speakerName.text = speaker ?? string.Empty;
            if (portraitImage != null)
            {
                portraitImage.sprite = portrait;
                portraitImage.color = portrait != null ? Color.white : new Color(1, 1, 1, 0);
            }
            ClearChoices();
            if (_typeCoroutine != null) StopCoroutine(_typeCoroutine);

            // Phase 31.1 — cache full text so a click during typewriter can
            // instantly skip to the fully-rendered line.
            _fullLineText = text ?? string.Empty;

            if (advancePrompt != null) advancePrompt.gameObject.SetActive(false);

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

        /// <summary>
        /// Immediately complete the running typewriter (showing the full
        /// line) without advancing past it. Director's WaitForAdvance then
        /// catches the *next* click to actually move on. Idempotent.
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

            // Self-heal in case the host was deactivated externally.
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            if (root != null && !root.activeSelf) root.SetActive(true);

            // Phase 31 — silence the narration line while a decision is in
            // front of the player. The body of the parchment box is reused
            // for the choices, and leaving the previous spoken line visible
            // underneath made the tiles look like floating fragments.
            if (lineText != null) lineText.gameObject.SetActive(false);
            if (advancePrompt != null) advancePrompt.gameObject.SetActive(false);
            _fullLineText = null;

            if (choiceContainer == null || choiceButtonPrefab == null) return;

            // Phase 31 — re-heal each present, in case a third party (eg.
            // Yarn Spinner runner, Mission01Director) mutated the container
            // between scenes.
            DialogueChoiceLayoutHealer.HealContainer(choiceContainer);

            for (int i = 0; i < choices.Count; i++)
            {
                int idx = i;
                var go = Instantiate(choiceButtonPrefab, choiceContainer);
                go.SetActive(true); // template prefab may have been inactive
                var label = go.GetComponentInChildren<TextMeshProUGUI>();
                // Phase 31 — prefix with [1]/[2]/… so the keyboard shortcut
                // (handled in Update) is discoverable without a tutorial.
                if (label != null) label.text = $"<b><color=#7a5314>[{idx + 1}]</color></b>  {choices[i]}";
                var btn = go.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => HandleChoice(idx));
                }
                _spawnedButtons.Add(go);

                // Phase 31 — fix the tile layout (full-width, wrapping,
                // tap-friendly height) on the freshly instantiated clone.
                DialogueChoiceLayoutHealer.HealTile(go);
            }
        }

        public void Hide()
        {
            if (_typeCoroutine != null) { StopCoroutine(_typeCoroutine); _typeCoroutine = null; }
            ClearChoices();
            // Phase 31 — leave lineText re-enabled so the next PresentLine()
            // does not surprise the player with an empty parchment body.
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
            // Phase 31 — once a choice has resolved, restore the lineText
            // area so the next spoken line from the director / Yarn runner
            // lands in its expected slot.
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

        // Phase 31 — keyboard shortcuts for choices. Pressing 1/2/3/4 picks
        // the corresponding choice without forcing the player to mouse over
        // small tiles. Defensive: only fires while choices are on-screen and
        // a callback is registered.
        // Phase 31.1 — also drives the visible "Click or [Space] ▸" advance
        // hint pulse, so players never miss that the dialogue is waiting on
        // them. (Skip-typewriter is exposed as `SkipTypewriter()` for the
        // mission director or Yarn runner to call — calling it from Update
        // here would race with WaitForAdvance on the same Space press and
        // double-advance, robbing the player of a chance to read the line.)
        private void Update()
        {
            // Pulse the advance prompt when the line is fully rendered and
            // no choices are showing — i.e. the director is blocked on a
            // click. PingPong 0.55–1.0 alpha at ~1.4 Hz.
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

            // Number-key shortcuts for the spawned choice tiles.
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
