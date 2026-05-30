// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / GardenUI  (Engagement Pillar P4 — Phase 65)
//
// THE GARDEN SCREEN — the cozy grow→brew→use ritual (Docs/Engagement_Bible/07).
// Press [G] in any gameplay scene. Shows each bed's state, lets the player plant
// in fallow beds, harvest ripe ones, brew held herbs into teas (gentle tools that
// help the next visit), and sell spare teas/herbs for coin (closing the
// garden→tea→coin→seeds wheel).
//
// ARCHITECTURE: presentational. Reads Core's GardenBoard (filled by the Mission
// GardenService); every action publishes a GardenActionRequestedEvent INTENT (no
// UI→Mission dep — D-035). Self-installing/self-building. D-068 safety net.
// Cozy: nothing wilts, nothing is required, every action is optional.

using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HearthboundHollow.Core;

namespace HearthboundHollow.UI
{
    [DisallowMultipleComponent]
    public class GardenUI : MonoBehaviour
    {
        public static GardenUI Instance { get; private set; }

        public KeyCode toggleKey = KeyCode.G;

        private CanvasGroup _canvasGroup;
        private GameObject _panel;
        private TextMeshProUGUI _headerLabel;
        private RectTransform _content;
        private readonly System.Collections.Generic.List<GameObject> _spawned = new();
        private bool _visible;

        private static readonly Color Parchment   = new Color(0.96f, 0.91f, 0.78f, 0.98f);
        private static readonly Color InkBrown     = new Color(0.24f, 0.17f, 0.10f, 1f);
        private static readonly Color DimBackdrop  = new Color(0.04f, 0.03f, 0.02f, 0.40f);
        private static readonly Color BtnGreen     = new Color(0.46f, 0.56f, 0.36f, 1f);
        private static readonly Color BtnAmber     = new Color(0.78f, 0.55f, 0.25f, 1f);
        private static readonly Color BtnSoft      = new Color(0.60f, 0.50f, 0.38f, 1f);

        private static readonly string[] Plantable = { "lavender", "valerian", "chamomile" };
        private static string Cap(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s.Substring(1);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInstall()
        {
            if (Instance != null) return;
            var go = new GameObject("_HHGardenUI");
            DontDestroyOnLoad(go);
            go.AddComponent<GardenUI>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            BuildUI();
            Hide();
        }

        private void OnEnable()  => GardenBoard.OnChanged += OnChanged;
        private void OnDisable() => GardenBoard.OnChanged -= OnChanged;
        private void OnChanged() { if (_visible) Refresh(); }

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
            var d = GardenBoard.Data;

            var inv = new StringBuilder();
            foreach (var kv in d.herbs) inv.Append($"{Cap(kv.Key)} x{kv.Value}  ");
            foreach (var kv in d.teas) inv.Append($"{Cap(kv.Key)} tea x{kv.Value}  ");
            string invLine = inv.Length > 0 ? inv.ToString().Trim() : "nothing harvested yet";
            if (_headerLabel != null)
                _headerLabel.text = $"<b>The Garden</b>\n<size=68%>{invLine}</size>"
                    + (string.IsNullOrEmpty(d.activeTeaLine) ? "" : $"\n<size=64%><i>{d.activeTeaLine}</i></size>");

            foreach (var go in _spawned) if (go != null) Destroy(go);
            _spawned.Clear();

            float y = 0.99f;
            const float gap = 0.012f;

            // Beds.
            foreach (var bed in d.beds)
            {
                if (bed == null) continue;
                y = AddBedRow(bed, y) - gap;
                if (y < 0.30f) break;
            }

            // Kitchen — brew held herbs.
            if (d.herbs.Count > 0 || d.teas.Count > 0)
            {
                y = AddSection("The kettle", y) - gap;
                foreach (var kv in d.herbs)
                {
                    string herb = kv.Key;
                    y = AddActionRow($"Brew {Cap(herb)} tea", BtnAmber, () => Publish("brew", herb), y) - gap;
                    if (y < 0.10f) break;
                }
                foreach (var kv in d.teas)
                {
                    string herb = kv.Key;
                    y = AddActionRow($"Sell {Cap(herb)} tea  (+coin)", BtnSoft, () => Publish("sell", herb), y) - gap;
                    if (y < 0.10f) break;
                }
            }
        }

        private float AddBedRow(GardenBedView bed, float yMax)
        {
            const float h = 0.09f;
            float yMin = yMax - h;
            var row = MakeRow("Bed_" + bed.bedId, yMin, yMax);

            var label = MakeLabel(row.transform, "L", new Vector2(0.0f, 0.0f), new Vector2(0.46f, 1.0f),
                21, InkBrown, false, TextAlignmentOptions.Left);
            label.text = bed.empty ? "<i>a fallow bed</i>"
                       : (bed.ripe ? $"<b>{bed.herbName}</b> — ripe" : $"{bed.herbName} — {bed.daysLeft}d");

            if (bed.empty)
            {
                float x = 0.48f; float w = 0.165f;
                foreach (var herb in Plantable)
                {
                    string capture = herb;
                    var b = MakeButton(row.transform, "Plant_" + herb, Cap(herb),
                        new Vector2(x, 0.12f), new Vector2(x + w, 0.88f), BtnGreen, 16);
                    b.onClick.AddListener(() => Publish("plant", capture));
                    x += w + 0.01f;
                }
            }
            else if (bed.ripe)
            {
                string bedId = bed.bedId;
                var b = MakeButton(row.transform, "Harvest", "Harvest",
                    new Vector2(0.66f, 0.12f), new Vector2(0.99f, 0.88f), BtnAmber, 18);
                b.onClick.AddListener(() => Publish("harvest", bedId));
            }
            return yMin;
        }

        private float AddSection(string title, float yMax)
        {
            const float h = 0.07f;
            float yMin = yMax - h;
            var row = MakeRow("Sec_" + title, yMin, yMax);
            MakeLabel(row.transform, "T", Vector2.zero, Vector2.one, 22, InkBrown, true, TextAlignmentOptions.Left).text = title;
            return yMin;
        }

        private float AddActionRow(string text, Color color, UnityEngine.Events.UnityAction onClick, float yMax)
        {
            const float h = 0.085f;
            float yMin = yMax - h;
            var row = MakeRow("Act_" + text, yMin, yMax);
            var b = MakeButton(row.transform, "B", text, new Vector2(0.02f, 0.12f), new Vector2(0.6f, 0.88f), color, 18);
            b.onClick.AddListener(onClick);
            return yMin;
        }

        private static void Publish(string action, string arg) => EventBus.Publish(new GardenActionRequestedEvent(action, arg));

        private RectTransform MakeRow(string name, float yMin, float yMax)
        {
            var row = new GameObject(name, typeof(RectTransform));
            row.transform.SetParent(_content, false);
            var rt = row.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, yMin); rt.anchorMax = new Vector2(1f, yMax);
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            _spawned.Add(row);
            return rt;
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
            var canvasGO = new GameObject("GardenCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
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
            pRT.anchorMin = new Vector2(0.28f, 0.12f);
            pRT.anchorMax = new Vector2(0.72f, 0.88f);
            pRT.offsetMin = Vector2.zero; pRT.offsetMax = Vector2.zero;

            _headerLabel = MakeLabel(_panel.transform, "Header",
                new Vector2(0.05f, 0.86f), new Vector2(0.95f, 0.985f),
                30, InkBrown, true, TextAlignmentOptions.Center);
            _headerLabel.richText = true;

            _content = new GameObject("Content", typeof(RectTransform)).GetComponent<RectTransform>();
            _content.SetParent(_panel.transform, false);
            _content.anchorMin = new Vector2(0.05f, 0.10f); _content.anchorMax = new Vector2(0.95f, 0.85f);
            _content.offsetMin = Vector2.zero; _content.offsetMax = Vector2.zero;

            var close = MakeButton(_panel.transform, "Btn_Close", "Close  (G)",
                new Vector2(0.36f, 0.02f), new Vector2(0.64f, 0.085f), BtnAmber, 22);
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
            tmp.enableWordWrapping = true; tmp.richText = true;
            var rt = tmp.rectTransform;
            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            UIAutoFitText.ApplyToLabel(tmp, minSize: Mathf.Max(8, fontSize - 8), maxSize: fontSize + 2);
            return tmp;
        }

        private Button MakeButton(Transform parent, string name, string text, Vector2 aMin, Vector2 aMax,
            Color color, int fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var label = MakeLabel(go.transform, "Label", Vector2.zero, Vector2.one, fontSize, Color.white, true, TextAlignmentOptions.Center);
            label.text = text;
            return go.GetComponent<Button>();
        }
    }
}
