// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / RequestBoardUI  (Engagement Pillar P2 — Phase 62)
//
// THE PLAYABLE REQUEST BOARD — the cozy notice board by the Hollow door
// (Docs/Engagement_Bible/05 §1). Press [B] (Board) to open it any time in a
// gameplay scene. It lists today's requests (built by the Mission-layer
// RequestBoardService into DailyLoopService.CurrentAgenda.tickets) and lets the
// player CHOOSE who to see and how to answer — the agency the critique said the
// game lacked. This is the never-empty content faucet made interactive.
//
// ARCHITECTURE: purely presentational. Reads Core state (DayAgenda tickets +
// VillageState); on a choice it publishes a RequestSelectedEvent INTENT that the
// Mission RequestVisitService consumes (it owns coin/collection/flags). UI never
// references Mission (D-035). Mirrors the proven self-installing, self-building
// AgendaCardUI / CollectionGlanceUI idiom (no scene edit, no builder, reachable
// purely via pull + Play).
//
// COZY CONTRACT: no fail, no timer, no pause; every visit is skippable; refusal
// and "not today" are always honoured (the request simply rolls to tomorrow).
// D-068 safety net: a transparent overlay never strands blocksRaycasts.

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HearthboundHollow.Core;

namespace HearthboundHollow.UI
{
    [DisallowMultipleComponent]
    public class RequestBoardUI : MonoBehaviour
    {
        public static RequestBoardUI Instance { get; private set; }

        public KeyCode toggleKey = KeyCode.B;

        private CanvasGroup _canvasGroup;
        private GameObject _panel;
        private GameObject _listView;
        private GameObject _visitView;
        private TextMeshProUGUI _listBody;     // header line above the buttons
        private RectTransform _listContent;    // parent of the ticket buttons
        private TextMeshProUGUI _visitTitle;
        private TextMeshProUGUI _visitBody;
        private TextMeshProUGUI _visitStatus;
        private readonly List<GameObject> _spawned = new();
        private RequestTicket _active;
        private bool _visible;

        private static readonly Color Parchment   = new Color(0.96f, 0.91f, 0.78f, 0.98f);
        private static readonly Color InkBrown     = new Color(0.24f, 0.17f, 0.10f, 1f);
        private static readonly Color DimBackdrop  = new Color(0.04f, 0.03f, 0.02f, 0.42f);
        private static readonly Color ButtonAmber  = new Color(0.78f, 0.55f, 0.25f, 1f);
        private static readonly Color ButtonSoft   = new Color(0.62f, 0.50f, 0.36f, 1f);

        // ───── Self-install ────────────────────────────────

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInstall()
        {
            if (Instance != null) return;
            var go = new GameObject("_HHRequestBoardUI");
            DontDestroyOnLoad(go);
            go.AddComponent<RequestBoardUI>();
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

        // ───── Show / Hide ────────────────────────────────

        private void Show()
        {
            ShowList();
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
            _active = null;
        }

        private List<RequestTicket> Tickets()
        {
            var agenda = DailyLoopService.Instance != null ? DailyLoopService.Instance.CurrentAgenda : null;
            return agenda != null ? agenda.tickets : null;
        }

        private void ShowList()
        {
            _active = null;
            if (_visitView != null) _visitView.SetActive(false);
            if (_listView != null) _listView.SetActive(true);

            // Clear old ticket buttons.
            foreach (var go in _spawned) if (go != null) Destroy(go);
            _spawned.Clear();

            var tickets = Tickets();
            if (tickets == null || tickets.Count == 0)
            {
                if (_listBody != null)
                    _listBody.text = "<i>The board is quiet this morning. A good day to tend the shop, the garden, or the bench.</i>";
                return;
            }

            if (_listBody != null)
                _listBody.text = "<b>At your door today</b>\n<size=80%>Tap a request to open the day's visit. Nothing here is urgent.</size>";

            float top = 0.66f;          // first button top anchor (within content area)
            const float h = 0.155f;     // button height fraction
            const float gap = 0.03f;
            int i = 0;
            foreach (var t in tickets)
            {
                if (t == null) continue;
                float yMax = top - i * (h + gap);
                float yMin = yMax - h;
                if (yMin < 0.02f) break;   // cap visible rows; rest roll naturally
                var capture = t;
                string label = string.IsNullOrEmpty(t.teaser)
                    ? t.villagerName
                    : $"{t.villagerName}  \u2014  \u201c{t.teaser}\u201d";
                var btn = MakeButton(_listContent, "Ticket_" + i, label,
                    new Vector2(0.04f, yMin), new Vector2(0.96f, yMax),
                    t.pinnedArc ? ButtonAmber : ButtonSoft, fontSize: 24);
                btn.onClick.AddListener(() => ShowVisit(capture));
                _spawned.Add(btn.gameObject);
                i++;
            }
        }

        private void ShowVisit(RequestTicket t)
        {
            _active = t;
            if (_listView != null) _listView.SetActive(false);
            if (_visitView != null) _visitView.SetActive(true);

            if (_visitTitle != null) _visitTitle.text = t.villagerName;
            if (_visitBody != null)
                _visitBody.text = string.IsNullOrEmpty(t.openingLine)
                    ? "<i>They wait, turning something over in their hands.</i>"
                    : t.openingLine;
            if (_visitStatus != null) _visitStatus.text = "";
        }

        private void Choose(string outcome)
        {
            if (_active == null) { ShowList(); return; }
            string id = _active.requestId;
            int reward = _active.coinReward;

            EventBus.Publish(new RequestSelectedEvent(id, outcome));

            // Presentational: remove resolved tickets from today's display model so
            // the board refreshes (defer/refuse leave it for tomorrow). The Mission
            // service owns the authoritative resolvedRequestIds list.
            if (outcome == "keep" || outcome == "listen")
            {
                var tickets = Tickets();
                tickets?.RemoveAll(x => x != null && x.requestId == id);
            }

            string closing = outcome switch
            {
                "keep"   => $"<i>Kept, and clear. (+{reward} coin to the purse.)</i>",
                "listen" => "<i>You sat with them a while. Some things are better kept by being heard.</i>",
                "defer"  => "<i>\u201cAnother day, then. Thank you for the thought.\u201d</i>",
                _         => "<i>You let it be. The door stays open whenever they're ready.</i>",
            };
            if (_visitStatus != null) _visitStatus.text = closing;

            // Brief beat, then back to the (refreshed) list.
            CancelInvoke(nameof(ReturnToList));
            Invoke(nameof(ReturnToList), 1.6f);
        }

        private void ReturnToList()
        {
            if (!_visible) return;
            ShowList();
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

        // ───── Self-built UI (mirrors AgendaCardUI idiom) ────────────────

        private void BuildUI()
        {
            var canvasGO = new GameObject("RequestBoardCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.transform.SetParent(transform, false);
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 49;   // cozy-overlay band (with the journal)
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
            pRT.anchorMin = new Vector2(0.28f, 0.16f);
            pRT.anchorMax = new Vector2(0.72f, 0.86f);
            pRT.offsetMin = Vector2.zero; pRT.offsetMax = Vector2.zero;

            var title = MakeLabel(_panel.transform, "Title",
                new Vector2(0.06f, 0.90f), new Vector2(0.94f, 0.985f),
                40, InkBrown, true, TextAlignmentOptions.Center);
            title.text = "The Hollow — Requests";

            // ── List view ──
            _listView = new GameObject("ListView", typeof(RectTransform));
            _listView.transform.SetParent(_panel.transform, false);
            Fill(_listView.GetComponent<RectTransform>(), 0.04f, 0.06f, 0.96f, 0.90f);

            _listBody = MakeLabel(_listView.transform, "ListHeader",
                new Vector2(0.02f, 0.80f), new Vector2(0.98f, 0.99f),
                24, InkBrown, false, TextAlignmentOptions.TopLeft);
            _listBody.richText = true;

            _listContent = new GameObject("Content", typeof(RectTransform)).GetComponent<RectTransform>();
            _listContent.SetParent(_listView.transform, false);
            Fill(_listContent, 0.0f, 0.0f, 1.0f, 0.80f);

            var closeList = MakeButton(_panel.transform, "Btn_CloseList", "Close  (B)",
                new Vector2(0.34f, 0.01f), new Vector2(0.66f, 0.075f), ButtonAmber, 22);
            closeList.onClick.AddListener(Hide);

            // ── Visit view ──
            _visitView = new GameObject("VisitView", typeof(RectTransform));
            _visitView.transform.SetParent(_panel.transform, false);
            Fill(_visitView.GetComponent<RectTransform>(), 0.04f, 0.02f, 0.96f, 0.90f);

            _visitTitle = MakeLabel(_visitView.transform, "VisitTitle",
                new Vector2(0.02f, 0.86f), new Vector2(0.98f, 0.99f),
                34, InkBrown, true, TextAlignmentOptions.Center);

            _visitBody = MakeLabel(_visitView.transform, "VisitBody",
                new Vector2(0.04f, 0.50f), new Vector2(0.96f, 0.84f),
                26, InkBrown, false, TextAlignmentOptions.TopLeft);
            _visitBody.richText = true;

            _visitStatus = MakeLabel(_visitView.transform, "VisitStatus",
                new Vector2(0.04f, 0.40f), new Vector2(0.96f, 0.49f),
                22, InkBrown, false, TextAlignmentOptions.Center);
            _visitStatus.richText = true;

            var keep = MakeButton(_visitView.transform, "Btn_Keep", "Keep this memory",
                new Vector2(0.08f, 0.27f), new Vector2(0.92f, 0.38f), ButtonAmber, 24);
            keep.onClick.AddListener(() => Choose("keep"));

            var listen = MakeButton(_visitView.transform, "Btn_Listen", "Just sit and listen",
                new Vector2(0.08f, 0.15f), new Vector2(0.92f, 0.26f), ButtonSoft, 24);
            listen.onClick.AddListener(() => Choose("listen"));

            var notToday = MakeButton(_visitView.transform, "Btn_NotToday", "Not today",
                new Vector2(0.08f, 0.03f), new Vector2(0.49f, 0.14f), ButtonSoft, 22);
            notToday.onClick.AddListener(() => Choose("defer"));

            var back = MakeButton(_visitView.transform, "Btn_Back", "\u25C2 The board",
                new Vector2(0.51f, 0.03f), new Vector2(0.92f, 0.14f), ButtonSoft, 22);
            back.onClick.AddListener(ShowList);
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        private static void Fill(RectTransform rt, float xMin, float yMin, float xMax, float yMax)
        {
            rt.anchorMin = new Vector2(xMin, yMin); rt.anchorMax = new Vector2(xMax, yMax);
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

        private Button MakeButton(Transform parent, string name, string text, Vector2 aMin, Vector2 aMax,
            Color color, int fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            var rt = img.rectTransform;
            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var label = MakeLabel(go.transform, "Label", Vector2.zero, Vector2.one, fontSize, Color.white, true, TextAlignmentOptions.Center);
            label.text = text;
            return go.GetComponent<Button>();
        }
    }
}
