// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / MemoryWebOverlay
//
// PHASE 51 — The Memory Web overlay.
//
// A Tab-key-opens-it overlay that visualises the player's held memories
// as a connected network. Each held memory is a card; each "Echo
// connection" between two memories is a drawn line.
//
// M1-2 ships 4 canonical Echo connections (Doris's First Loaves,
// Marin's Workbench Note, Gerrold's Wife memory, the Marin Echo
// Hologram). Each first-discovery bumps `memoryWebConnectionsFound`.
// Codex 12 § Echo Web describes the full system; the M1-2 surface here
// is the seam for that scaling layer.
//
// Visuals — pure UI primitives:
//   - Background dim (full-screen black @ alpha 0.65)
//   - Title bar parchment
//   - Memory cards on a circular layout (up to 8 visible)
//   - Connection lines: thin Image rotated to point card-to-card
//   - Tooltip on hover (mouse only) showing the shared facet name
//
// Opens with: Tab key, or `OpenWeb()` programmatic call.
// Closes with: Esc, Tab again, or close button.
//
// All animations use unscaledDeltaTime so the overlay is still usable
// while the game is paused.

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HearthboundHollow.Core;

namespace HearthboundHollow.UI
{
    public class MemoryWebOverlay : MonoBehaviour
    {
        // ─── Inspector wiring (built by Phase 51) ───────────────────

        [Header("Roots")]
        public CanvasGroup rootGroup;
        public RectTransform cardsContainer;
        public RectTransform connectionsContainer;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI emptyStateText;
        public Button closeButton;
        public TextMeshProUGUI connectionsCountLabel;
        public RectTransform tooltipPanel;
        public TextMeshProUGUI tooltipText;

        [Header("Prefabs (created procedurally by Phase 51)")]
        public GameObject cardPrefab;          // sized 220x140
        public GameObject connectionLinePrefab; // sized 4x100

        [Header("Tunables")]
        [Range(0.1f, 1.0f)] public float fadeDuration = 0.35f;
        [Range(120, 600)] public float circularRadius = 320f;

        [Header("Connection registry (canonical M1-2 connections)")]
        public List<EchoConnection> connections = new();

        // ─── Runtime state ─────────────────────────────────────────

        private VillageState _state;
        private bool _open;
        private readonly List<MemoryCardEntry> _spawnedCards = new();
        private readonly List<RectTransform> _spawnedLines = new();

        private void Awake()
        {
            if (rootGroup != null) { rootGroup.alpha = 0f; rootGroup.gameObject.SetActive(false); }
            if (closeButton != null) closeButton.onClick.AddListener(CloseWeb);
            if (tooltipPanel != null) tooltipPanel.gameObject.SetActive(false);
            EnsureDefaultConnections();
        }

        private void EnsureDefaultConnections()
        {
            if (connections != null && connections.Count > 0) return;
            connections = new List<EchoConnection>
            {
                new EchoConnection {
                    memoryIdA = "DOR-001", memoryIdB = "MAR-NOTE-01",
                    sharedFacet = "first time at the workbench" },
                new EchoConnection {
                    memoryIdA = "DOR-001", memoryIdB = "GER-WIFE-01",
                    sharedFacet = "a Sunday kitchen at first light" },
                new EchoConnection {
                    memoryIdA = "MAR-NOTE-01", memoryIdB = "ECHO-MARIN-01",
                    sharedFacet = "the Hollow before you" },
                new EchoConnection {
                    memoryIdA = "GER-WIFE-01", memoryIdB = "ECHO-MARIN-01",
                    sharedFacet = "the Forgotten Year" },
            };
        }

        // ─── Lifecycle: input ───────────────────────────────────────

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (_open) CloseWeb();
                else        OpenWeb();
            }
            else if (_open && Input.GetKeyDown(KeyCode.Escape))
            {
                CloseWeb();
            }
        }

        // ─── Public API ─────────────────────────────────────────────

        public void OpenWeb()
        {
            _state = ServiceLocator.Resolve<VillageState>();
            BuildSnapshot();
            _open = true;
            if (rootGroup != null) rootGroup.gameObject.SetActive(true);
            StartCoroutine(Fade(0f, 1f));
        }

        public void CloseWeb()
        {
            _open = false;
            StartCoroutine(FadeOutThenHide());
        }

        // ─── Build / teardown ──────────────────────────────────────

        private void BuildSnapshot()
        {
            // Clear prior state.
            foreach (var c in _spawnedCards) if (c.go != null) Destroy(c.go);
            foreach (var l in _spawnedLines) if (l != null) Destroy(l.gameObject);
            _spawnedCards.Clear();
            _spawnedLines.Clear();

            // Build held memories list.
            var held = new List<string>();
            if (_state != null && _state.heldMemoryIds != null) held.AddRange(_state.heldMemoryIds);
            // The Echo Hologram is "held" once heard.
            if (_state != null && _state.echoHologramHeard && !held.Contains("ECHO-MARIN-01"))
                held.Add("ECHO-MARIN-01");
            // Marin's Note is "held" once read.
            if (_state != null && _state.readMarinNoteIds != null && _state.readMarinNoteIds.Contains("Marin_Workbench_01")
                && !held.Contains("MAR-NOTE-01"))
                held.Add("MAR-NOTE-01");

            if (emptyStateText != null) emptyStateText.gameObject.SetActive(held.Count == 0);
            if (cardsContainer == null) return;

            // Place cards on a circular layout.
            int n = held.Count;
            float angleStep = n > 0 ? (Mathf.PI * 2f / n) : 0f;
            for (int i = 0; i < n; i++)
            {
                float a = -Mathf.PI / 2f + angleStep * i;
                Vector2 pos = new Vector2(Mathf.Cos(a) * circularRadius, Mathf.Sin(a) * circularRadius);
                var card = SpawnCard(held[i], pos);
                _spawnedCards.Add(card);
            }

            // Draw connections that involve held memories.
            int connectionsKnown = 0;
            foreach (var c in connections)
            {
                var a = _spawnedCards.Find(x => x.memoryId == c.memoryIdA);
                var b = _spawnedCards.Find(x => x.memoryId == c.memoryIdB);
                if (a.go == null || b.go == null) continue;
                DrawConnection(a.rt.anchoredPosition, b.rt.anchoredPosition, c.sharedFacet);
                connectionsKnown++;
            }

            // Persist progress: if connectionsKnown > current count, bump.
            if (_state != null && connectionsKnown > _state.memoryWebConnectionsFound)
            {
                _state.memoryWebConnectionsFound = connectionsKnown;
                Hh.Log(LogCategory.Mission,
                    $"Memory Web: {connectionsKnown} connection(s) now visible.");
            }

            if (connectionsCountLabel != null)
            {
                connectionsCountLabel.text =
                    $"Memories: {n}   ·   Connections: {connectionsKnown}";
            }
        }

        private MemoryCardEntry SpawnCard(string memoryId, Vector2 anchoredPos)
        {
            var go = Instantiate(cardPrefab, cardsContainer);
            go.SetActive(true);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            var label = go.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = DisplayName(memoryId);
            return new MemoryCardEntry { go = go, rt = rt, memoryId = memoryId };
        }

        private void DrawConnection(Vector2 a, Vector2 b, string label)
        {
            var go = Instantiate(connectionLinePrefab, connectionsContainer);
            go.SetActive(true);
            var rt = go.GetComponent<RectTransform>();
            Vector2 mid = (a + b) * 0.5f;
            float len = Vector2.Distance(a, b);
            float ang = Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg;
            rt.anchoredPosition = mid;
            rt.sizeDelta = new Vector2(len, rt.sizeDelta.y);
            rt.localEulerAngles = new Vector3(0, 0, ang);

            // Tooltip listener: hover shows the shared facet.
            var trigger = go.AddComponent<MemoryWebHoverTooltip>();
            trigger.overlay = this;
            trigger.tooltipBody = label;

            _spawnedLines.Add(rt);
        }

        // ─── Tooltip ────────────────────────────────────────────────

        public void ShowTooltip(string body, Vector2 screenPos)
        {
            if (tooltipPanel == null || tooltipText == null) return;
            tooltipPanel.gameObject.SetActive(true);
            tooltipText.text = body;
            // Convert screen to local in our canvas
            var canvas = tooltipPanel.GetComponentInParent<Canvas>();
            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)tooltipPanel.parent, screenPos, canvas != null ? canvas.worldCamera : null, out local);
            tooltipPanel.anchoredPosition = local + new Vector2(16, -16);
        }

        public void HideTooltip()
        {
            if (tooltipPanel != null) tooltipPanel.gameObject.SetActive(false);
        }

        // ─── Display names (M1-2 canonical) ────────────────────────

        private static string DisplayName(string id) => id switch
        {
            "DOR-001"        => "Doris — First Loaves",
            "MAR-NOTE-01"    => "Marin's Note (workbench)",
            "GER-WIFE-01"    => "Gerrold — A Sunday Kitchen",
            "ECHO-MARIN-01"  => "Echo: Marin's Welcome",
            _ => id,
        };

        // ─── Coroutines ────────────────────────────────────────────

        private IEnumerator Fade(float from, float to)
        {
            if (rootGroup == null) yield break;
            float t = 0f;
            rootGroup.alpha = from;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                rootGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / fadeDuration));
                yield return null;
            }
            rootGroup.alpha = to;
        }

        private IEnumerator FadeOutThenHide()
        {
            yield return Fade(rootGroup != null ? rootGroup.alpha : 1f, 0f);
            if (rootGroup != null) rootGroup.gameObject.SetActive(false);
            HideTooltip();
        }

        // ─── Data ──────────────────────────────────────────────────

        [System.Serializable]
        public class EchoConnection
        {
            public string memoryIdA;
            public string memoryIdB;
            public string sharedFacet;
        }

        private struct MemoryCardEntry
        {
            public GameObject go;
            public RectTransform rt;
            public string memoryId;
        }
    }

    /// <summary>
    /// Per-line hover listener that surfaces the shared facet as a tooltip
    /// near the cursor. Attached procedurally by Phase 51 to every drawn
    /// connection line.
    /// </summary>
    public class MemoryWebHoverTooltip : MonoBehaviour,
        UnityEngine.EventSystems.IPointerEnterHandler,
        UnityEngine.EventSystems.IPointerExitHandler,
        UnityEngine.EventSystems.IPointerMoveHandler
    {
        public MemoryWebOverlay overlay;
        public string tooltipBody;

        public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData e)
        {
            if (overlay != null) overlay.ShowTooltip(tooltipBody, e.position);
        }
        public void OnPointerMove(UnityEngine.EventSystems.PointerEventData e)
        {
            if (overlay != null) overlay.ShowTooltip(tooltipBody, e.position);
        }
        public void OnPointerExit(UnityEngine.EventSystems.PointerEventData e)
        {
            if (overlay != null) overlay.HideTooltip();
        }
    }
}
