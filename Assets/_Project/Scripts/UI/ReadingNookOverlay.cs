using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HearthboundHollow.Core;
using HearthboundHollow.Memory;

namespace HearthboundHollow.UI
{
    /// <summary>
    /// Parchment-style overlay that presents Marin Vellis's letter fragments
    /// when the player sits in the Reading Nook armchair.
    ///
    /// Two-panel layout (built by Phase52_ReadingNookBuilder):
    ///   Panel A — Fragment list: one button per letter, greyed-out if locked.
    ///   Panel B — Letter view: title + full body text + a Back button.
    ///
    /// Pure presentation layer — all state mutations are handled by
    /// ReadingNookInteractable (Mission asmdef) via the onFragmentRead callback.
    ///
    /// Phase 52 — Reading Nook subsystem.
    /// Lives in HearthboundHollow.UI asmdef (→ Core, Memory, TMP, InputSystem).
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class ReadingNookOverlay : MonoBehaviour
    {
        // ─── Inspector ────────────────────────────────────────────────────────

        [Header("Root")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Fragment List Panel")]
        [SerializeField] private GameObject fragmentListPanel;
        [SerializeField] private Transform  fragmentListContainer;
        [SerializeField] private GameObject fragmentButtonPrefab;
        [SerializeField] private TextMeshProUGUI nookHeaderText;

        [Header("Letter View Panel")]
        [SerializeField] private GameObject       letterViewPanel;
        [SerializeField] private TextMeshProUGUI  letterTitleText;
        [SerializeField] private TextMeshProUGUI  letterBodyText;
        [SerializeField] private Button           backToListButton;

        [Header("Footer")]
        [SerializeField] private Button           closeButton;

        [Header("Comfort")]
        [Tooltip("Instant alpha in Gentle Mode (or Edit Mode). Set by builder.")]
        [SerializeField] private bool  instantFade;
        [SerializeField] private float fadeDuration = 0.28f;

        // ─── State ────────────────────────────────────────────────────────────

        private List<MarinLetterFragmentSO> _fragments  = new();
        private Action<string, int>         _onFragmentRead; // (fragmentId, warmthGranted)
        private Action                      _onClose;
        private bool                        _isShowing;

        // Current player warmth — refreshed each Show() call.
        private int _currentWarmth;
        // Ids already read — refreshed each Show() call.
        private HashSet<string> _readIds = new();

        // ─── Unity lifecycle ──────────────────────────────────────────────────

        private void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            // D-068: start fully hidden; invisible overlay must never block raycasts.
            canvasGroup.alpha          = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable   = false;

            if (closeButton     != null) closeButton.onClick.AddListener(Hide);
            if (backToListButton != null) backToListButton.onClick.AddListener(ShowListPanel);
        }

        private void Update()
        {
            // Escape / Tab to close — accessibility shortcut.
            if (_isShowing &&
                (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab)))
                Hide();
        }

        // D-068 invariant enforced every frame.
        private void LateUpdate()
        {
            if (canvasGroup != null && canvasGroup.alpha <= 0.001f && canvasGroup.blocksRaycasts)
                canvasGroup.blocksRaycasts = false;
        }

        // ─── Public API ───────────────────────────────────────────────────────

        /// <summary>
        /// Populates the fragment list from <paramref name="fragments"/> and fades in.
        /// </summary>
        /// <param name="fragments">Full ordered list of letter SOs (locked + unlocked).</param>
        /// <param name="currentWarmth">Player's current predecessorTrailWarmth.</param>
        /// <param name="alreadyReadIds">Ids of fragments already read this save.</param>
        /// <param name="onFragmentRead">Called (fragmentId, warmthGranted) on first read.</param>
        /// <param name="onClose">Called when the overlay closes.</param>
        public void Show(
            List<MarinLetterFragmentSO> fragments,
            int                          currentWarmth,
            IEnumerable<string>          alreadyReadIds,
            Action<string, int>          onFragmentRead,
            Action                       onClose)
        {
            _fragments      = fragments      ?? new List<MarinLetterFragmentSO>();
            _onFragmentRead = onFragmentRead;
            _onClose        = onClose;
            _currentWarmth  = currentWarmth;
            _readIds        = new HashSet<string>(alreadyReadIds ?? Array.Empty<string>());

            BuildFragmentList();
            ShowListPanel();

            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable   = true;
            _isShowing = true;

            StopAllCoroutines();
            if (instantFade || !Application.isPlaying)
                canvasGroup.alpha = 1f;
            else
                StartCoroutine(FadeRoutine(1f));
        }

        /// <summary>Hides the overlay and invokes <see cref="_onClose"/>.</summary>
        public void Hide()
        {
            if (!_isShowing) return;
            _isShowing = false;
            canvasGroup.interactable   = false;
            canvasGroup.blocksRaycasts = false;

            StopAllCoroutines();
            if (instantFade || !Application.isPlaying)
            {
                canvasGroup.alpha = 0f;
                FireClose();
            }
            else
                StartCoroutine(FadeOutRoutine());
        }

        // ─── Private — UI ─────────────────────────────────────────────────────

        private void ShowListPanel()
        {
            if (fragmentListPanel != null) fragmentListPanel.SetActive(true);
            if (letterViewPanel   != null) letterViewPanel.SetActive(false);
        }

        private void ShowLetterPanel(MarinLetterFragmentSO frag)
        {
            if (letterTitleText != null) letterTitleText.text = frag.title;
            if (letterBodyText  != null) letterBodyText.text  = frag.letterText;

            if (fragmentListPanel != null) fragmentListPanel.SetActive(false);
            if (letterViewPanel   != null) letterViewPanel.SetActive(true);

            // Notify caller on first read (idempotent — caller guards for duplicates).
            _onFragmentRead?.Invoke(frag.fragmentId, frag.warmthGranted);
            // Mark locally so button style updates if player navigates back.
            _readIds.Add(frag.fragmentId);
        }

        private void BuildFragmentList()
        {
            if (fragmentListContainer == null || fragmentButtonPrefab == null) return;

            // Clear previous buttons.
            foreach (Transform child in fragmentListContainer)
                Destroy(child.gameObject);

            foreach (var frag in _fragments)
            {
                bool unlocked = _currentWarmth >= frag.warmthThreshold;
                bool read     = _readIds.Contains(frag.fragmentId);

                var  btnGO  = Instantiate(fragmentButtonPrefab, fragmentListContainer);
                var  label  = btnGO.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                {
                    if (unlocked)
                        label.text = read ? $"\u00b7 {frag.title}" : $"<b>{frag.title}</b>";
                    else
                        label.text = "<color=#8B7355><i>Not yet...</i></color>";
                }

                var button = btnGO.GetComponent<Button>();
                if (button != null)
                {
                    if (unlocked)
                    {
                        var captured = frag;
                        button.onClick.AddListener(() => ShowLetterPanel(captured));
                    }
                    else
                    {
                        button.interactable = false;
                    }
                }
            }
        }

        // ─── Private — coroutines ─────────────────────────────────────────────

        private IEnumerator FadeRoutine(float target)
        {
            float start = canvasGroup.alpha;
            for (float t = 0f; t < fadeDuration; t += Time.unscaledDeltaTime)
            {
                canvasGroup.alpha = Mathf.Lerp(start, target, t / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = target;
        }

        private IEnumerator FadeOutRoutine()
        {
            yield return StartCoroutine(FadeRoutine(0f));
            FireClose();
        }

        private void FireClose()
        {
            var cb = _onClose;
            _onClose = null;
            cb?.Invoke();
        }
    }
}
