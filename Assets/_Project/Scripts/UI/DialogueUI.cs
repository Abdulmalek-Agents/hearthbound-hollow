// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / DialogueUI
//
// Renders dialogue lines in the Bamao parchment box with portrait + typewriter
// effect + up-to-4 choice scrolls. Decoupled from Yarn Spinner via an
// abstract `IDialoguePresenter` — the YarnVillageStateBridge in the Dialogue
// asmdef adapts Yarn calls onto this presenter.
//
// ── Phase 25 hotfix ─────────────────────────────────────────
// PresentLine() now self-activates the script-host before running the
// typewriter coroutine. Same defensive pattern applied across all UI
// overlays in this release.
//
// ── Playtest pass fix (commit 5/6) ───────────────────────────────
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
//
// ── Phase 38 (2026-05-26) ────────────────────────────────────
// PresentLine now publishes a DialogueLineStartedEvent to the EventBus
// so MumbleVoicePlayer (Audio asmdef, no UI dep) can sync per-character
// syllable playback to the typewriter reveal. Speaker is lower-cased to
// match the canonical character id in MumbleVoiceLibrarySO.banks.
//
// ── Phase 32 — Voice Acting MVP (2026-05-27) ────────────────────
// PresentLine now has an overload taking a `lineId`. When that lineId
// resolves in `VoicePlayer.Instance` → `VoiceLibrarySO`, the matching
// 22 kHz mono PCM16 voice clip plays in sync with the typewriter. The
// per-line `charsPerSecond` is locked to `clip.length / text.Length`
// so the last visible character lands exactly as the voice ends — a
// real lip-sync feel. Skipping a line (Space / click) ALSO stops the
// voice (DialogueUI.Hide / SkipTypewriter / PresentChoices). Zero
// regression: the parameterless overload still exists; missing voice
// data is a silent no-op and the old typewriter-only path runs.
//
// ── Phase 32.10 (2026-05-27) ──────────────────────────────────
// DialogueLineStartedEvent now carries a HasVoiceClip flag, set true
// when VoicePlayer.Play returned a non-zero clip length. MumbleVoicePlayer
// reads this and suppresses its syllable bank for THIS line so we don't
// stack the procedural mumble on top of a real voice (which would sound
// muddy and double-tracked). Backward-compat: defaults to false.

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

        /// <summary>
        /// Phase 32 overload — additionally accepts a stable lineId looked up
        /// in <see cref="VoiceLibrarySO"/> via <see cref="VoicePlayer"/>. A
        /// null/empty lineId behaves identically to the no-lineId overload.
        /// </summary>
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
        [Tooltip("Pulsing 'Click or [Space] >' label that appears " +
                 "in the dialogue box's lower-right when the line is fully " +
                 "rendered and no choices are showing. Uses ASCII '>' so the " +
                 "default LiberationSans SDF font can render it (the previous " +
                 "U+25B8 ▸ glyph was not in the font and spammed warnings).")]
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

        // Phase 38 — last-spoken character id, so DialogueLineEndedEvent can
        // carry the right speaker when the typewriter coroutine finishes.
        private string _lastSpeakerId;

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
            if (advancePrompt != null)
            {
                // Phase 56 (D-073) — localize + Arabic-shape the advance prompt at
                // runtime. The baked scene PRE-WIRES this label, so
                // EnsureAdvancePromptExists() returns early and its build-time text
                // would otherwise stay English ("Click or [Space] >"). Setting it
                // here also supersedes the legacy ▸ glyph some old prefabs baked
                // (it's overwritten wholesale), so the Phase 32.11 ▸→> fix is moot.
                advancePrompt.text = LocalizationService.GetShaped("dialogue.advance");
                advancePrompt.gameObject.SetActive(false);
            }

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
            // Phase 32.11 — use ">" instead of "▸" (U+25B8 BLACK RIGHT-POINTING
            // SMALL TRIANGLE). LiberationSans SDF (the default TMP font that
            // ships with Unity 6) does NOT include U+25B8, so it was being
            // substituted with U+25A1 WHITE SQUARE □ and spamming a warning
            // every frame: "The character with Unicode value ▸ was not
            // found in [LiberationSans SDF] … replaced by □".
            // ">" is in every font; the visual carries the same "continue"
            // meaning. If you need the fancier glyph, add a font fallback
            // (Window → TextMeshPro → Font Asset Creator) to a font that
            // has U+25B8 such as Noto Sans Symbols.
            advancePrompt.text = LocalizationService.GetShaped("dialogue.advance");
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
        /// Phase 32 — Voice Acting MVP overload. <paramref name="lineId"/> is
        /// looked up in <see cref="VoicePlayer.Instance"/>'s VoiceLibrarySO;
        /// if it resolves the clip plays in sync with the typewriter, and the
        /// per-line typewriter speed is locked to <c>text.Length / clipLen</c>
        /// so the last character lands as the voice ends (lip-sync feel).
        /// A null/empty lineId, missing entry, or null clip is a silent no-op
        /// and the old typewriter-only behaviour runs.
        /// </summary>
        public void PresentLine(string speaker, string text, Sprite portrait, string lineId)
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            if (root != null) root.SetActive(true);
            // Phase 32.16 — restore CanvasGroup visibility (it may have been
            // hidden by a previous Hide() call). Mirror of the ApplyVisibility
            // call in Hide().
            ApplyVisibility(true);

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

            // ── Phase 32 — Voice Acting MVP ──────────────────────────────
            // Play the voice clip (if any) for this line. The typewriter
            // charsPerSecond is then locked to the clip duration so the last
            // character lands as the voice ends — gives a real lip-sync feel.
            // VoicePlayer.Play returns 0f when there's no clip, in which case
            // we fall through to the legacy fixed charsPerSecond path.
            float clipLen = VoicePlayer.Instance != null ? VoicePlayer.Instance.Play(lineId) : 0f;
            int targetCps = charsPerSecond;
            if (clipLen > 0.4f && !string.IsNullOrEmpty(text))
                targetCps = Mathf.Clamp(Mathf.RoundToInt(text.Length / clipLen), 18, 90);

            // Phase 38 — publish DialogueLineStartedEvent so MumbleVoicePlayer
            // (Audio asmdef, no direct UI reference) can sync syllable
            // playback to this line. Speaker is lower-cased to match the
            // canonical character id used by MumbleVoiceLibrarySO.banks.
            //
            // Phase 32.10 — pass HasVoiceClip = (clipLen > 0) so
            // MumbleVoicePlayer suppresses its syllable bank for THIS
            // line when a real voice clip is already playing. Stops the
            // procedural mumble + real voice from stacking.
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

        /// <summary>
        /// Estimated typewriter duration in seconds for `text`, matching the
        /// per-character interval used by `TypeCoroutine`. Exposed so the
        /// `DialogueLineStartedEvent` payload can carry an accurate duration
        /// for the mumble VO to pace its syllable count against.
        /// </summary>
        private float ComputeTypewriterDuration(string text) => ComputeTypewriterDuration(text, charsPerSecond);

        /// <summary>
        /// Phase 32 overload — when a voice clip is playing, the typewriter
        /// speed is locked to the clip duration; this overload lets callers
        /// pass the resulting <paramref name="cps"/> so the predicted
        /// duration matches the actual coroutine timing.
        /// </summary>
        private float ComputeTypewriterDuration(string text, int cps)
        {
            if (string.IsNullOrEmpty(text)) return 0.3f;
            int useCps = Mathf.Max(1, cps);
            return text.Length / (float)useCps + postLineLinger;
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
            // Phase 32 — stop the voice clip immediately on skip so the
            // audio doesn't keep speaking after the player advances.
            VoicePlayer.Instance?.Stop();
            // Phase 38 — tell MumbleVoicePlayer the line is done early.
            EventBus.Publish(new DialogueLineEndedEvent(_lastSpeakerId ?? string.Empty));
        }

        public void PresentChoices(IReadOnlyList<string> choices, Action<int> onChoiceSelected)
        {
            _choiceCallback = onChoiceSelected;
            ClearChoices();

            // Phase 32 — stop any in-progress voice clip when choices appear,
            // so a lingering line doesn't bleed into the player's decision moment.
            VoicePlayer.Instance?.Stop();

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
            // Phase 32 — stop voice playback when the dialogue UI hides so a
            // voice clip doesn't keep playing after the conversation ends.
            VoicePlayer.Instance?.Stop();
            // Phase 38 — stop mumble playback when the dialogue UI hides.
            EventBus.Publish(new DialogueLineEndedEvent(_lastSpeakerId ?? string.Empty));
            // Phase 32.16 — drive a CanvasGroup on the root for invisibility
            // (alpha = 0 + blocksRaycasts = false). The original
            // `if (root != gameObject) root.SetActive(false)` guard was a
            // NO-OP under the Phase 14 builder's wiring (`root = gameObject`)
            // — the dialogue panel never actually hid, so "I'll wait. Take
            // your time, Keeper." stayed on screen for the entire polish
            // mini-game. CanvasGroup hides reliably regardless of root
            // wiring while keeping the script host alive (Phase 25 invariant).
            ApplyVisibility(false);
            IsBusy = false;
        }

        /// <summary>
        /// Phase 32.16 — visibility toggle that works whether `root` is the
        /// host GameObject (Phase 14 builder default) or a separate panel
        /// child (the recommended two-layer pattern). Uses a CanvasGroup on
        /// the root so the script host stays Active either way.
        /// </summary>
        private void ApplyVisibility(bool visible)
        {
            var target = root != null ? root : gameObject;
            var cg = target.GetComponent<CanvasGroup>();
            if (cg == null) cg = target.AddComponent<CanvasGroup>();
            cg.alpha = visible ? 1f : 0f;
            cg.blocksRaycasts = visible;
            cg.interactable = visible;
            // Belt-and-braces: if the recommended two-layer pattern IS in
            // play (root is a child distinct from gameObject), also toggle
            // its active state as the legacy code did.
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

        /// <summary>
        /// Phase 32 overload — accepts an explicit chars-per-second so the
        /// voice-locked typewriter pace from <see cref="PresentLine(string,string,Sprite,string)"/>
        /// is honoured. Falls back to <see cref="charsPerSecond"/> if not given.
        /// </summary>
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
            // Phase 38 — mumble VO cuts off at the natural end of the line
            // (the mumble player already self-times to estimatedDur but this
            // belt-and-braces ensures a runaway syllable bank can't bleed).
            EventBus.Publish(new DialogueLineEndedEvent(_lastSpeakerId ?? string.Empty));
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
