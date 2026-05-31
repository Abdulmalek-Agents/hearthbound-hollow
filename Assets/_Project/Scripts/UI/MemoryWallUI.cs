// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / MemoryWallUI  (Engagement Pillar P6 — Phase 63)
//
// THE MEMORY WALL — the cozy collection + Echo-thread meta-game screen
// (Docs/Engagement_Bible/09). Press [M] in any gameplay scene. It shows what the
// player has gathered (memories kept, echoes found) and the Echo THREADS they're
// pursuing — each with gentle progress ("The Sunday Kitchen — 1/2") and a soft
// hint, with completed threads marked with a warm check. This is the self-set-goal
// engine: the player sees "one more to find" and decides to chase it.
//
// Companion to the Phase-51 Tab "Memory Web" overlay (the network visual). This
// screen is the goal/collection register; they can be unified later (D-080). It
// follows the proven self-installing, self-building AgendaCardUI idiom and reads
// only Core state (EchoBoard + VillageState) — no UI→Mission dependency (D-035).
//
// COZY CONTRACT: celebratory counts only (abundance, never deficit); no fail, no
// timer, no pause; toggle [M] / Esc / Close. D-068 raycast safety net.

using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HearthboundHollow.Core;

namespace HearthboundHollow.UI
{
    [DisallowMultipleComponent]
    public class MemoryWallUI : MonoBehaviour
    {
        public static MemoryWallUI Instance { get; private set; }

        public KeyCode toggleKey = KeyCode.M;

        private CanvasGroup _canvasGroup;
        private GameObject _panel;
        private TextMeshProUGUI _headerLabel;
        private TextMeshProUGUI _bodyLabel;
        private bool _visible;

        private static readonly Color Parchment   = new Color(0.96f, 0.91f, 0.78f, 0.98f);
        private static readonly Color InkBrown     = new Color(0.24f, 0.17f, 0.10f, 1f);
        private static readonly Color DimBackdrop  = new Color(0.04f, 0.03f, 0.02f, 0.40f);
        private static readonly Color ButtonAmber  = new Color(0.78f, 0.55f, 0.25f, 1f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInstall()
        {
            if (Instance != null) return;
            var go = new GameObject("_HHMemoryWall");
            DontDestroyOnLoad(go);
            go.AddComponent<MemoryWallUI>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            BuildUI();
            Hide();
        }

        private void OnEnable()  => EchoBoard.OnChanged += OnEchoChanged;
        private void OnDisable() => EchoBoard.OnChanged -= OnEchoChanged;

        private void OnEchoChanged() { if (_visible) Refresh(); }

        private void Update()
        {
            string scene = SceneManager.GetActiveScene().name ?? string.Empty;
            bool blocked = scene.IndexOf("Menu", System.StringComparison.OrdinalIgnoreCase) >= 0
                           || scene.IndexOf("Bootstrap", System.StringComparison.OrdinalIgnoreCase) >= 0;
            if (!blocked && Input.GetKeyDown(toggleKey)) Toggle();
            else if (_visible && Input.GetKeyDown(KeyCode.Escape)) Hide();
        }

        private void Toggle() { if (_visible) Hide(); else Show(); }

        private void Show()
        {
            Refresh();
            if (_panel != null) _panel.SetActive(true);
            SetBlocking(true);
            if (_canvasGroup != null) _canvasGroup.alpha = 1f;
            _visible = true;
        }

        private void Hide()
        {
            SetBlocking(false);
            if (_canvasGroup != null) _canvasGroup.alpha = 0f;
            if (_panel != null) _panel.SetActive(false);
            _visible = false;
        }

        private void Refresh()
        {
            var vs = ServiceLocator.Get<VillageState>();
            int memories = vs != null && vs.heldMemoryIds != null ? vs.heldMemoryIds.Count : 0;
            int echoesFound = vs != null && vs.revealedEchoConnectionIds != null ? vs.revealedEchoConnectionIds.Count : 0;
            int threadsTotal = EchoBoard.Threads.Count;

            if (_headerLabel != null)
                _headerLabel.text = $"<b>The Memory Wall</b>\n<size=70%>Memories kept: {memories}     ·     Echoes found: {echoesFound}</size>";

            var sb = new StringBuilder();
            if (threadsTotal == 0)
            {
                sb.AppendLine("<i>No threads yet. Keep a few memories and the echoes between them will begin to show.</i>");
            }
            else
            {
                foreach (var t in EchoBoard.Threads)
                {
                    if (t == null) continue;
                    if (t.complete)
                    {
                        sb.AppendLine($"<b>\u2713 {t.displayName}</b>  <size=75%>({t.threshold}/{t.threshold}) — complete</size>");
                    }
                    else
                    {
                        sb.AppendLine($"<b>{t.displayName}</b>  <size=75%>{t.kept}/{t.threshold}</size>");
                        if (!string.IsNullOrEmpty(t.hint))
                            sb.AppendLine($"   <size=70%><i>{t.hint}</i></size>");
                    }
                    sb.AppendLine();
                }
            }
            if (_bodyLabel != null) _bodyLabel.text = sb.ToString().TrimEnd();
        }

        private void SetBlocking(bool on)
        {
            if (_canvasGroup == null) return;
            _canvasGroup.blocksRaycasts = on;
            _canvasGroup.interactable = on;
        }

        private void LateUpdate()
        {
            if (_canvasGroup != null && _canvasGroup.blocksRaycasts && _canvasGroup.alpha <= 0.001f)
                SetBlocking(false);
        }

        private void BuildUI()
        {
            var canvasGO = new GameObject("MemoryWallCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.transform.SetParent(transform, false);
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 49;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            var root = new GameObject("Root", typeof(RectTransform), typeof(CanvasGroup));
            root.transform.SetParent(canvasGO.transform, false);
            Stretch(root.GetComponent<RectTransform>());
            _canvasGroup = root.GetComponent<CanvasGroup>();

            var dim = new GameObject("Dim", typeof(RectTransform), typeof(Image));
            dim.transform.SetParent(root.transform, false);
            Stretch(dim.GetComponent<RectTransform>());
            dim.GetComponent<Image>().color = DimBackdrop;

            _panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            _panel.transform.SetParent(root.transform, false);
            _panel.GetComponent<Image>().color = Parchment;
            var pRT = _panel.GetComponent<RectTransform>();
            pRT.anchorMin = new Vector2(0.30f, 0.18f);
            pRT.anchorMax = new Vector2(0.70f, 0.84f);
            pRT.offsetMin = Vector2.zero; pRT.offsetMax = Vector2.zero;

            _headerLabel = MakeLabel(_panel.transform, "Header",
                new Vector2(0.06f, 0.82f), new Vector2(0.94f, 0.975f),
                34, InkBrown, true, TextAlignmentOptions.Center);
            _headerLabel.richText = true;

            _bodyLabel = MakeLabel(_panel.transform, "Body",
                new Vector2(0.08f, 0.16f), new Vector2(0.92f, 0.80f),
                25, InkBrown, false, TextAlignmentOptions.TopLeft);
            _bodyLabel.richText = true;

            var close = MakeButton(_panel.transform, "Btn_Close", "Close  (M)",
                new Vector2(0.34f, 0.04f), new Vector2(0.66f, 0.14f));
            close.onClick.AddListener(Hide);
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        private TextMeshProUGUI MakeLabel(Transform parent, string name, Vector2 aMin, Vector2 aMax,
            int fontSize, Color color, bool bold, TextAlignmentOptions align)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = fontSize; tmp.color = color; tmp.alignment = align;
            tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            tmp.enableWordWrapping = true;
            var rt = tmp.rectTransform;
            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            UIAutoFitText.ApplyToLabel(tmp, minSize: Mathf.Max(8, fontSize - 8), maxSize: fontSize + 2);
            return tmp;
        }

        private Button MakeButton(Transform parent, string name, string text, Vector2 aMin, Vector2 aMax)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = ButtonAmber;
            var rt = img.rectTransform;
            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var label = MakeLabel(go.transform, "Label", Vector2.zero, Vector2.one, 24, Color.white, true, TextAlignmentOptions.Center);
            label.text = text;
            return go.GetComponent<Button>();
        }
    }
}
