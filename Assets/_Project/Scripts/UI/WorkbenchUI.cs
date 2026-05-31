// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / WorkbenchUI  (Engagement Pillar P5 — Phase 66)
//
// THE LIVING WORKBENCH screen (Docs/Engagement_Bible/08). Press [K] (Keeper's
// bench) in any gameplay scene. Lists the kept memories still "awaiting a little
// care," each asking for a specific VERB (Polish / Cleanse / Sort / Steep — chosen
// per-memory so the work varies). Tending is no-fail and always available, but a
// gentle "Perfect" can land (odds rise with the hidden Keeper's Hand mastery) for
// a cosmetic shimmer + a touch more coin — repetition that quietly improves.
//
// ARCHITECTURE: presentational. Reads VillageState (kept/tended) + Core CraftVerbs;
// tending publishes a CraftRequestedEvent INTENT consumed by the Mission
// WorkbenchService (no UI→Mission dep — D-035). Self-installing/self-building.
// D-068 safety net. Cozy: optional, skippable, never punishing.

using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HearthboundHollow.Core;

namespace HearthboundHollow.UI
{
    [DisallowMultipleComponent]
    public class WorkbenchUI : MonoBehaviour
    {
        public static WorkbenchUI Instance { get; private set; }

        public KeyCode toggleKey = KeyCode.K;

        private CanvasGroup _canvasGroup;
        private GameObject _panel;
        private TextMeshProUGUI _headerLabel;
        private TextMeshProUGUI _statusLabel;
        private RectTransform _content;
        private readonly System.Collections.Generic.List<GameObject> _spawned = new();
        private bool _visible;

        private static readonly Color Parchment   = new Color(0.96f, 0.91f, 0.78f, 0.98f);
        private static readonly Color InkBrown     = new Color(0.24f, 0.17f, 0.10f, 1f);
        private static readonly Color DimBackdrop  = new Color(0.04f, 0.03f, 0.02f, 0.40f);
        private static readonly Color ButtonAmber  = new Color(0.78f, 0.55f, 0.25f, 1f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInstall()
        {
            if (Instance != null) return;
            var go = new GameObject("_HHWorkbench_UI");
            DontDestroyOnLoad(go);
            go.AddComponent<WorkbenchUI>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            BuildUI();
            Hide();
        }

        private void OnEnable()  => EventBus.Subscribe<MemoryTendedEvent>(OnTended);
        private void OnDisable() => EventBus.Unsubscribe<MemoryTendedEvent>(OnTended);

        private void OnTended(MemoryTendedEvent e)
        {
            if (_statusLabel != null)
                _statusLabel.text = e.Perfect
                    ? "<b>Perfect.</b> <i>It catches the light just so.</i>"
                    : "<i>Kept clean. It settles on the shelf.</i>";
            if (_visible) Refresh();
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
            if (_statusLabel != null) _statusLabel.text = "";
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
            int mastery = vs != null ? vs.keeperHandCraftCount : 0;
            if (_headerLabel != null)
                _headerLabel.text = $"<b>The Workbench</b>\n<size=66%><i>{CraftVerbs.MasteryFlavor(mastery)}</i></size>";

            foreach (var go in _spawned) if (go != null) Destroy(go);
            _spawned.Clear();

            var held = vs != null ? vs.heldMemoryIds : null;
            int shown = 0;
            float top = 0.99f; const float h = 0.135f; const float gap = 0.02f;

            if (held != null)
            {
                foreach (var id in held)
                {
                    if (string.IsNullOrEmpty(id)) continue;
                    if (IsTended(vs, id)) continue;
                    float yMax = top - shown * (h + gap);
                    float yMin = yMax - h;
                    if (yMin < 0.02f) break;
                    AddRow(id, yMin, yMax);
                    shown++;
                }
            }

            if (shown == 0)
            {
                var none = MakeLabel(_content, "None", new Vector2(0.02f, 0.80f), new Vector2(0.98f, 0.99f),
                    22, InkBrown, false, TextAlignmentOptions.TopLeft);
                none.text = held != null && held.Count > 0
                    ? "<i>Every kept memory is tended. The shelf is calm and bright.</i>"
                    : "<i>No memories yet. Keep one at the Request Board [B], then tend it here.</i>";
            }
        }

        private static bool IsTended(VillageState vs, string id)
            => vs != null && vs.materials != null && vs.materials.Contains($"tended_{id}");

        private void AddRow(string memoryId, float yMin, float yMax)
        {
            var row = new GameObject("Row", typeof(RectTransform));
            row.transform.SetParent(_content, false);
            var rt = row.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, yMin); rt.anchorMax = new Vector2(1f, yMax);
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            _spawned.Add(row);

            string verb = CraftVerbs.VerbFor(memoryId);
            var label = MakeLabel(row.transform, "L", new Vector2(0.02f, 0.0f), new Vector2(0.66f, 1.0f),
                21, InkBrown, false, TextAlignmentOptions.Left);
            label.text = $"<b>{Pretty(memoryId)}</b>\n<size=78%><i>{CraftVerbs.Flavor(verb)}</i></size>";

            var btn = MakeButton(row.transform, "Tend", CraftVerbs.Label(verb),
                new Vector2(0.68f, 0.18f), new Vector2(0.99f, 0.82f), ButtonAmber, 17);
            string id = memoryId;
            btn.onClick.AddListener(() => EventBus.Publish(new CraftRequestedEvent(id, verb)));
        }

        private static string Pretty(string id)
        {
            if (string.IsNullOrEmpty(id)) return "A kept memory";
            // Synth ids look like MEM_DORIS_WALKIN_DORIS_0 — surface the villager warmly.
            string up = id.ToUpperInvariant();
            string[] names = { "DORIS", "GERROLD", "BRAM", "MARISKA", "INKWELL", "TOMEK", "PETRA" };
            foreach (var n in names)
                if (up.Contains(n)) return $"A memory of {char.ToUpper(n[0])}{n.Substring(1).ToLowerInvariant()}'s";
            return "A kept memory";
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
            var canvasGO = new GameObject("WorkbenchCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
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
            pRT.anchorMin = new Vector2(0.27f, 0.12f);
            pRT.anchorMax = new Vector2(0.73f, 0.88f);
            pRT.offsetMin = Vector2.zero; pRT.offsetMax = Vector2.zero;

            _headerLabel = MakeLabel(_panel.transform, "Header",
                new Vector2(0.05f, 0.87f), new Vector2(0.95f, 0.985f),
                30, InkBrown, true, TextAlignmentOptions.Center);
            _headerLabel.richText = true;

            _content = new GameObject("Content", typeof(RectTransform)).GetComponent<RectTransform>();
            _content.SetParent(_panel.transform, false);
            _content.anchorMin = new Vector2(0.05f, 0.14f); _content.anchorMax = new Vector2(0.95f, 0.86f);
            _content.offsetMin = Vector2.zero; _content.offsetMax = Vector2.zero;

            _statusLabel = MakeLabel(_panel.transform, "Status",
                new Vector2(0.05f, 0.095f), new Vector2(0.95f, 0.14f),
                20, InkBrown, false, TextAlignmentOptions.Center);
            _statusLabel.richText = true;

            var close = MakeButton(_panel.transform, "Btn_Close", "Close  (K)",
                new Vector2(0.36f, 0.02f), new Vector2(0.64f, 0.085f), ButtonAmber, 22);
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
