// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / AgendaCardUI  (Engagement Pillar P1 — Phase 61.5)
//
// THE MORNING BOOKEND OF THE COZY DAILY LOOP.
// The evening bookend already exists (EveningLedger recap + OneMoreDayCard
// "tomorrow" tease). This adds the missing MORNING beat: a warm, dismissable
// "what shall I do today?" Agenda card that drives DailyLoopService.BeginDay()
// live so the Living Day (P1) is actually running.
//
// DESIGN (Docs/Engagement_Bible/03 §2 + 04 §4):
//   • Shows once per in-game day, when the player enters the Hollow (the
//     natural "wake in your shop" location). A short delay lets the scene's
//     own title card settle first (no overlap).
//   • Lists the day label + (future) visitors + garden status + a gentle
//     Marin margin-note suggestion. Opportunity, never obligation (D-076).
//   • Until the Request Board (P2) / Garden (P4) services exist to populate
//     DayAgenda, the card composes a few gentle baseline lines from
//     VillageState so it is never empty.
//
// COZY CONTRACT:
//   • Non-blocking, fully dismissable (button OR Space/Enter/E/Esc/click).
//   • No timeScale pause, no fail state, no countdown, no numbers-going-down.
//   • D-068 safety net: a transparent card never strands blocksRaycasts.
//
// SHIP DISCIPLINE:
//   • SELF-INSTALLING via [RuntimeInitializeOnLoadMethod] (matches
//     RuntimeAudioBootstrap / MissionAudioHooks). No builder, no scene edit,
//     so it is reachable purely by pulling + pressing 🚀 Build Everything +
//     Play — nothing extra to click, nothing in Advanced (D-074 spirit).
//   • Builds its own ScreenSpaceOverlay canvas in code, mirroring the proven
//     MiniGameTutorialUI idiom (TMP default font via UIAutoFitText).
//   • UI asmdef references Core already, so DailyLoopService/DayAgenda/
//     VillageState/EventBus are all in-scope (no asmdef change — D-035).
//
// NOTE: this phase does NOT touch GameManager.EndDay (the currentDayIndex
// owner) or DayCycleManager.autoAdvance — those land in Phase 61.6 (D-077)
// once they can be Play-tested, to keep this drop strictly additive.

using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HearthboundHollow.Core;

namespace HearthboundHollow.UI
{
    [DisallowMultipleComponent]
    public class AgendaCardUI : MonoBehaviour
    {
        public static AgendaCardUI Instance { get; private set; }

        [Header("Feel")]
        [Range(0f, 4f)] public float showDelaySeconds = 1.4f;   // let the scene title card settle first
        [Range(0.1f, 1.5f)] public float fadeInSeconds = 0.6f;

        // Built at runtime.
        private CanvasGroup _canvasGroup;
        private GameObject _panel;
        private TextMeshProUGUI _titleLabel;
        private TextMeshProUGUI _bodyLabel;

        private bool _visible;
        private bool _advanced;
        private int _lastAgendaDay = -1;

        private static readonly Color Parchment   = new Color(0.96f, 0.91f, 0.78f, 0.97f);
        private static readonly Color InkBrown     = new Color(0.24f, 0.17f, 0.10f, 1f);
        private static readonly Color DimBackdrop  = new Color(0.04f, 0.03f, 0.02f, 0.45f);
        private static readonly Color ButtonAmber  = new Color(0.78f, 0.55f, 0.25f, 1f);

        // ───── Self-install ────────────────────────────────────────────────

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInstall()
        {
            if (Instance != null) return;
            var go = new GameObject("_HHAgendaCard");
            DontDestroyOnLoad(go);
            go.AddComponent<AgendaCardUI>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            BuildUI();
            SetBlocking(false);
            if (_canvasGroup != null) _canvasGroup.alpha = 0f;   // hide the dim backdrop while idle
            if (_panel != null) _panel.SetActive(false);

            SceneManager.sceneLoaded += OnSceneLoaded;
            EventBus.Subscribe<AgendaReadyEvent>(OnAgendaReady);
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            EventBus.Unsubscribe<AgendaReadyEvent>(OnAgendaReady);
            if (Instance == this) Instance = null;
        }

        // ───── Scene trigger ────────────────────────────────────────────────

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            string n = scene.name ?? string.Empty;
            // The morning beat fires in the Hollow (the player's shop/home).
            // Skip menus/bootstrap and the onboarding-heavy Lane.
            bool isMorningScene = n.IndexOf("Hollow", System.StringComparison.OrdinalIgnoreCase) >= 0
                                  && n.IndexOf("Menu", System.StringComparison.OrdinalIgnoreCase) < 0;
            if (!isMorningScene) return;

            int day = ResolveDay();
            if (day == _lastAgendaDay) return;   // once per in-game day
            StartCoroutine(ShowAfterDelay());
        }

        private IEnumerator ShowAfterDelay()
        {
            yield return new WaitForSecondsRealtime(showDelaySeconds);

            var loop = DailyLoopService.Instance;
            if (loop != null)
            {
                // Drives the Living Day live: fires DayStartedEvent (future P2/P4
                // services populate the Agenda) then AgendaReadyEvent (we render).
                loop.BeginDay(ServiceLocator.Get<VillageState>());
            }
            else
            {
                // Fallback if the loop service isn't present for any reason.
                ShowCard(ResolveDay());
            }
        }

        private void OnAgendaReady(AgendaReadyEvent e) => ShowCard(e.DayIndex);

        // ───── Presentation ─────────────────────────────────────────────────

        private void ShowCard(int dayIndex)
        {
            _lastAgendaDay = dayIndex;
            _advanced = false;

            var (title, body) = Compose(dayIndex);
            if (_titleLabel != null) _titleLabel.text = title;
            if (_bodyLabel != null) _bodyLabel.text = body;

            if (_panel != null) _panel.SetActive(true);
            SetBlocking(true);
            StopAllCoroutines();              // stop the delay coroutine; we're showing now
            StartCoroutine(FadeIn());
            _visible = true;

            Hh.Log(LogCategory.Mission, $"[AgendaCard] Morning agenda shown for Day {dayIndex + 1}.");
        }

        private (string title, string body) Compose(int dayIndex)
        {
            var loop = DailyLoopService.Instance;
            var agenda = loop != null ? loop.CurrentAgenda : null;
            var vs = ServiceLocator.Get<VillageState>();

            string mood = MoodLine(dayIndex);
            string title = $"Spire-Month · Day {dayIndex + 1}";

            var sb = new StringBuilder();
            sb.AppendLine($"<i>{mood}</i>");
            sb.AppendLine();

            // Visitors (populated by the Request Board once P2 ships; baseline for now).
            sb.AppendLine("<b>At your door today</b>");
            if (agenda != null && agenda.visitors.Count > 0)
                foreach (var v in agenda.visitors) sb.AppendLine($"  · {v}");
            else
                sb.AppendLine($"  · {VisitorBaseline(vs)}");
            sb.AppendLine();

            // Garden (populated by the Garden service once P4 ships).
            if (agenda != null && agenda.gardenNotes.Count > 0)
            {
                sb.AppendLine("<b>In the garden</b>");
                foreach (var g in agenda.gardenNotes) sb.AppendLine($"  · {g}");
                sb.AppendLine();
            }

            // Marin's gentle, optional self-goal nudge.
            string marin = (agenda != null && !string.IsNullOrWhiteSpace(agenda.marinSuggestion))
                ? agenda.marinSuggestion
                : MarinBaseline(vs);
            sb.AppendLine($"<i>Marin's note: \u201c{marin}\u201d</i>");

            return (title, sb.ToString().TrimEnd());
        }

        private static string MoodLine(int dayIndex) => (dayIndex % 3) switch
        {
            0 => "a bright cold morning",
            1 => "a soft grey morning, smoke from one chimney",
            _ => "first frost on the window, the kettle already warm",
        };

        private static string VisitorBaseline(VillageState vs)
        {
            if (vs == null) return "Someone may call at the Hollow.";
            if (!vs.metDoris) return "Doris keeps the bakery down the lane.";
            if (!vs.firstMoralChoiceMade) return "A careful sort may knock today.";
            return "The lane is quiet. A good day to tend the shop.";
        }

        private static string MarinBaseline(VillageState vs)
        {
            if (vs == null) return "The Hollow keeps a person more than the person keeps the Hollow.";
            if (!vs.metDoris) return "Be kind. Be slow. Listen for the second sentence.";
            return "A shelf with full company keeps better than an empty one.";
        }

        private void Update()
        {
            if (!_visible) return;
            if (Input.GetMouseButtonDown(0) ||
                Input.GetKeyDown(KeyCode.Space) ||
                Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.KeypadEnter) ||
                Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetKeyDown(KeyCode.E))
            {
                Dismiss();
            }
        }

        private void Dismiss()
        {
            if (_advanced) return;            // single-fire guard
            _advanced = true;
            _visible = false;
            SetBlocking(false);
            if (_canvasGroup != null) _canvasGroup.alpha = 0f;   // hide the dim backdrop again
            if (_panel != null) _panel.SetActive(false);
        }

        private IEnumerator FadeIn()
        {
            if (_canvasGroup == null) yield break;
            float t = 0f;
            _canvasGroup.alpha = 0f;
            while (t < fadeInSeconds)
            {
                t += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Clamp01(t / fadeInSeconds);
                yield return null;
            }
            _canvasGroup.alpha = 1f;
        }

        private void SetBlocking(bool on)
        {
            if (_canvasGroup == null) return;
            _canvasGroup.blocksRaycasts = on;
            _canvasGroup.interactable = on;
        }

        // Safety net (D-068): a fully transparent card must never eat clicks.
        private void LateUpdate()
        {
            if (_canvasGroup != null && _canvasGroup.blocksRaycasts && _canvasGroup.alpha <= 0.001f)
                SetBlocking(false);
        }

        private int ResolveDay()
        {
            if (DailyLoopService.Instance != null) return DailyLoopService.Instance.DayIndex;
            var vs = ServiceLocator.Get<VillageState>();
            return vs != null ? vs.currentDayIndex : 0;
        }

        // ───── Self-built UI (mirrors MiniGameTutorialUI idiom) ──────────────

        private void BuildUI()
        {
            var canvasGO = new GameObject("AgendaCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.transform.SetParent(transform, false);
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 48;   // above ControlHints (40), below Pause/Help (50)
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // Root (full-screen) carries the CanvasGroup for the fade.
            var root = new GameObject("Root", typeof(RectTransform), typeof(CanvasGroup));
            root.transform.SetParent(canvasGO.transform, false);
            Stretch(root.GetComponent<RectTransform>());
            _canvasGroup = root.GetComponent<CanvasGroup>();

            // Soft dim backdrop.
            var dim = new GameObject("Dim", typeof(RectTransform), typeof(Image));
            dim.transform.SetParent(root.transform, false);
            Stretch(dim.GetComponent<RectTransform>());
            dim.GetComponent<Image>().color = DimBackdrop;

            // Centered parchment panel.
            _panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            _panel.transform.SetParent(root.transform, false);
            var pImg = _panel.GetComponent<Image>();
            pImg.color = Parchment;
            var pRT = _panel.GetComponent<RectTransform>();
            pRT.anchorMin = new Vector2(0.30f, 0.24f);
            pRT.anchorMax = new Vector2(0.70f, 0.80f);
            pRT.offsetMin = Vector2.zero; pRT.offsetMax = Vector2.zero;

            _titleLabel = MakeLabel(_panel.transform, "Title",
                new Vector2(0.06f, 0.84f), new Vector2(0.94f, 0.97f),
                fontSize: 46, color: InkBrown, bold: true, align: TextAlignmentOptions.Center);

            _bodyLabel = MakeLabel(_panel.transform, "Body",
                new Vector2(0.08f, 0.20f), new Vector2(0.92f, 0.82f),
                fontSize: 28, color: InkBrown, bold: false, align: TextAlignmentOptions.TopLeft);
            _bodyLabel.enableWordWrapping = true;
            _bodyLabel.richText = true;

            var openBtn = MakeButton(_panel.transform, "Btn_OpenDay", "Open the day  \u25B8",
                new Vector2(0.30f, 0.05f), new Vector2(0.70f, 0.17f));
            openBtn.onClick.AddListener(Dismiss);
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
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
            UIAutoFitText.ApplyToLabel(tmp, minSize: Mathf.Max(8, fontSize - 8), maxSize: fontSize + 2);
            return tmp;
        }

        private Button MakeButton(Transform parent, string name, string text,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = ButtonAmber;
            var rt = img.rectTransform;
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            var label = MakeLabel(go.transform, "Label",
                Vector2.zero, Vector2.one,
                fontSize: 26, color: Color.white, bold: true, align: TextAlignmentOptions.Center);
            label.text = text;
            return go.GetComponent<Button>();
        }
    }
}
