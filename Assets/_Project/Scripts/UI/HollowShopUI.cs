// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / HollowShopUI  (Engagement Pillar P3 — Phase 64)
//
// "THE LEDGER OF IMPROVEMENTS" — the cozy shop screen where coin becomes a
// growing Hollow (Docs/Engagement_Bible/06 §6). Press [U] (Upgrades) in any
// gameplay scene. Lists the upgrade catalog with name, flavor, cost; affordable
// items show an [ Improve ] button, owned items a warm ✓, and locked/unaffordable
// items are GREY with a gentle "soon" hint (never red — D-076).
//
// ARCHITECTURE: presentational. Reads Core's HollowShopBoard (filled by the
// Mission HollowProgressionService) + VillageState.coin; on a buy it publishes a
// HollowPurchaseRequestedEvent INTENT (no UI→Mission dep — D-035). Self-installing/
// self-building (AgendaCardUI idiom). D-068 raycast safety net.

using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HearthboundHollow.Core;

namespace HearthboundHollow.UI
{
    [DisallowMultipleComponent]
    public class HollowShopUI : MonoBehaviour
    {
        public static HollowShopUI Instance { get; private set; }

        public KeyCode toggleKey = KeyCode.U;

        private CanvasGroup _canvasGroup;
        private GameObject _panel;
        private TextMeshProUGUI _headerLabel;
        private RectTransform _content;
        private readonly System.Collections.Generic.List<GameObject> _spawned = new();
        private bool _visible;

        private static readonly Color Parchment   = new Color(0.96f, 0.91f, 0.78f, 0.98f);
        private static readonly Color InkBrown     = new Color(0.24f, 0.17f, 0.10f, 1f);
        private static readonly Color DimBackdrop  = new Color(0.04f, 0.03f, 0.02f, 0.40f);
        private static readonly Color ButtonAmber  = new Color(0.78f, 0.55f, 0.25f, 1f);
        private static readonly Color GreySoft     = new Color(0.62f, 0.58f, 0.50f, 0.85f);
        private static readonly Color OwnedGreen   = new Color(0.42f, 0.52f, 0.34f, 1f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInstall()
        {
            if (Instance != null) return;
            var go = new GameObject("_HHHollowShop");
            DontDestroyOnLoad(go);
            go.AddComponent<HollowShopUI>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            BuildUI();
            Hide();
        }

        private void OnEnable()  => HollowShopBoard.OnChanged += OnBoardChanged;
        private void OnDisable() => HollowShopBoard.OnChanged -= OnBoardChanged;
        private void OnBoardChanged() { if (_visible) Refresh(); }

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
            int coin = vs != null ? vs.coin : 0;
            if (_headerLabel != null)
                _headerLabel.text = $"<b>Grow the Hollow</b>\n<size=70%>Coin purse: {coin} ◆   ·   improvements are warmth, never required</size>";

            foreach (var go in _spawned) if (go != null) Destroy(go);
            _spawned.Clear();

            float top = 0.985f;
            const float h = 0.150f;
            const float gap = 0.018f;
            int i = 0;
            foreach (var u in HollowShopBoard.Catalog)
            {
                if (u == null) continue;
                float yMax = top - i * (h + gap);
                float yMin = yMax - h;
                if (yMin < 0.0f) break;
                MakeRow(u, yMin, yMax);
                i++;
            }
        }

        private void MakeRow(HollowUpgradeView u, float yMin, float yMax)
        {
            var row = new GameObject("Row_" + u.upgradeId, typeof(RectTransform));
            row.transform.SetParent(_content, false);
            var rt = row.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.0f, yMin); rt.anchorMax = new Vector2(1.0f, yMax);
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            _spawned.Add(row);

            var name = MakeLabel(row.transform, "Name", new Vector2(0.02f, 0.46f), new Vector2(0.70f, 0.98f),
                23, InkBrown, true, TextAlignmentOptions.TopLeft);
            name.text = $"{u.displayName}  <size=80%>— {u.coinCost} ◆</size>";

            var flavor = MakeLabel(row.transform, "Flavor", new Vector2(0.02f, 0.02f), new Vector2(0.70f, 0.46f),
                18, InkBrown, false, TextAlignmentOptions.TopLeft);
            flavor.text = $"<i>{u.flavor}</i>";

            // Action area on the right.
            if (u.purchased)
            {
                var owned = MakeFlat(row.transform, "Owned", new Vector2(0.72f, 0.20f), new Vector2(0.98f, 0.80f), OwnedGreen);
                MakeLabel(owned.transform, "L", Vector2.zero, Vector2.one, 22, Color.white, true, TextAlignmentOptions.Center).text = "✓ kept";
            }
            else if (u.affordable)
            {
                var btn = MakeButton(row.transform, "Buy", "Improve",
                    new Vector2(0.72f, 0.20f), new Vector2(0.98f, 0.80f), ButtonAmber, 22);
                string id = u.upgradeId;
                btn.onClick.AddListener(() => EventBus.Publish(new HollowPurchaseRequestedEvent(id)));
            }
            else
            {
                var soft = MakeFlat(row.transform, "Soon", new Vector2(0.72f, 0.20f), new Vector2(0.98f, 0.80f), GreySoft);
                MakeLabel(soft.transform, "L", Vector2.zero, Vector2.one, 18, Color.white, false, TextAlignmentOptions.Center)
                    .text = string.IsNullOrEmpty(u.lockHint) ? "soon" : u.lockHint;
            }
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
            var canvasGO = new GameObject("HollowShopCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
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
                new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.985f),
                32, InkBrown, true, TextAlignmentOptions.Center);
            _headerLabel.richText = true;

            _content = new GameObject("Content", typeof(RectTransform)).GetComponent<RectTransform>();
            _content.SetParent(_panel.transform, false);
            _content.anchorMin = new Vector2(0.05f, 0.10f); _content.anchorMax = new Vector2(0.95f, 0.87f);
            _content.offsetMin = Vector2.zero; _content.offsetMax = Vector2.zero;

            var close = MakeButton(_panel.transform, "Btn_Close", "Close  (U)",
                new Vector2(0.36f, 0.02f), new Vector2(0.64f, 0.09f), ButtonAmber, 22);
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

        private GameObject MakeFlat(Transform parent, string name, Vector2 aMin, Vector2 aMax, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            return go;
        }

        private Button MakeButton(Transform parent, string name, string text, Vector2 aMin, Vector2 aMax,
            Color color, int fontSize)
        {
            var go = MakeFlat(parent, name, aMin, aMax, color);
            var btn = go.AddComponent<Button>();
            var label = MakeLabel(go.transform, "Label", Vector2.zero, Vector2.one, fontSize, Color.white, true, TextAlignmentOptions.Center);
            label.text = text;
            return btn;
        }
    }
}
