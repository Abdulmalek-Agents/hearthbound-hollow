// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / CollectionGlanceUI  (Engagement Pillar P6 — Phase 61.9)
//
// A cozy on-demand "journal" the player opens with [J]: shows what their Hollow
// has become so far — the day, coin, memories kept, and echoes discovered. This
// is the visible-progression fix (D-076): players can finally SEE growth, which
// the critique flagged as Root Cause #2. Hidden by default (no HUD overlap),
// self-installing, self-building (mirrors the proven AgendaCardUI idiom).
//
// Cozy: celebratory counts only (abundance, never deficit); no fail, no timer,
// no pause. Toggle with J; dismiss with J / Esc / the Close button. D-068 safety
// net: a transparent overlay never strands blocksRaycasts.

using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HearthboundHollow.Core;

namespace HearthboundHollow.UI
{
    [DisallowMultipleComponent]
    public class CollectionGlanceUI : MonoBehaviour
    {
        public static CollectionGlanceUI Instance { get; private set; }

        public KeyCode toggleKey = KeyCode.J;

        private CanvasGroup _canvasGroup;
        private GameObject _panel;
        private TextMeshProUGUI _bodyLabel;
        private bool _visible;

        private static readonly Color Parchment  = new Color(0.96f, 0.91f, 0.78f, 0.98f);
        private static readonly Color InkBrown    = new Color(0.24f, 0.17f, 0.10f, 1f);
        private static readonly Color DimBackdrop = new Color(0.04f, 0.03f, 0.02f, 0.35f);
        private static readonly Color ButtonAmber = new Color(0.78f, 0.55f, 0.25f, 1f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInstall()
        {
            if (Instance != null) return;
            var go = new GameObject("_HHCollectionGlance");
            DontDestroyOnLoad(go);
            go.AddComponent<CollectionGlanceUI>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            BuildUI();
            Hide();
        }

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
            if (_bodyLabel != null) _bodyLabel.text = Compose();
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

        private string Compose()
        {
            var vs = ServiceLocator.Get<VillageState>();
            int day = vs != null ? vs.currentDayIndex + 1 : 1;
            int coin = vs != null ? vs.coin : 0;
            int memories = vs != null && vs.heldMemoryIds != null ? vs.heldMemoryIds.Count : 0;
            int echoes = vs != null && vs.revealedEchoConnectionIds != null ? vs.revealedEchoConnectionIds.Count : 0;

            var sb = new StringBuilder();
            sb.AppendLine($"<b>Day {day}</b>");
            sb.AppendLine();
            sb.AppendLine($"Coin in your purse:   <b>{coin}</b>");
            sb.AppendLine($"Memories kept:        <b>{memories}</b>");
            sb.AppendLine($"Echoes discovered:    <b>{echoes}</b>");
            sb.AppendLine();
            sb.AppendLine(memories == 0
                ? "<i>The shelves are bare. The first memory is always the warmest.</i>"
                : "<i>The Hollow keeps a little more of the village each day.</i>");
            return sb.ToString();
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
            var canvasGO = new GameObject("CollectionGlanceCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.transform.SetParent(transform, false);
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 49;   // above ControlHints (40), at the cozy-overlay band
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
            pRT.anchorMin = new Vector2(0.34f, 0.30f);
            pRT.anchorMax = new Vector2(0.66f, 0.74f);
            pRT.offsetMin = Vector2.zero; pRT.offsetMax = Vector2.zero;

            var title = MakeLabel(_panel.transform, "Title",
                new Vector2(0.06f, 0.85f), new Vector2(0.94f, 0.97f),
                40, InkBrown, true, TextAlignmentOptions.Center);
            title.text = "Your Hollow";

            _bodyLabel = MakeLabel(_panel.transform, "Body",
                new Vector2(0.10f, 0.22f), new Vector2(0.90f, 0.83f),
                28, InkBrown, false, TextAlignmentOptions.TopLeft);
            _bodyLabel.richText = true;

            var close = MakeButton(_panel.transform, "Btn_Close", "Close  (J)",
                new Vector2(0.32f, 0.06f), new Vector2(0.68f, 0.18f));
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
