// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / CoinPurseHUD  (Engagement Pillar P3 / D-076 — Phase 64)
//
// A small, cozy coin-purse readout pinned to the top-right of every gameplay
// scene, so the player can SEE earnings + spends (the visible-progression fix the
// critique demanded). Pulses gently and shows a brief "+N"/"-N" floater on change;
// settles to a calm idle alpha. Self-installing, self-building — no scene wiring.
//
// Cozy guardrail (D-076): shows abundance (coin held + what just changed),
// celebratory not anxious. No countdown, never red for "not enough" (that lives,
// gently, in the shop). Reads VillageState + CoinChangedEvent only (Core).

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HearthboundHollow.Core;

namespace HearthboundHollow.UI
{
    [DisallowMultipleComponent]
    public class CoinPurseHUD : MonoBehaviour
    {
        public static CoinPurseHUD Instance { get; private set; }

        private Canvas _canvas;
        private TextMeshProUGUI _coinLabel;
        private TextMeshProUGUI _floater;
        private Coroutine _floatRoutine;

        private static readonly Color Amber = new Color(0.95f, 0.80f, 0.45f, 1f);
        private static readonly Color GainGreen = new Color(0.62f, 0.85f, 0.55f, 1f);
        private static readonly Color SpendSoft = new Color(0.92f, 0.84f, 0.62f, 1f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInstall()
        {
            if (Instance != null) return;
            var go = new GameObject("_HHCoinPurse");
            DontDestroyOnLoad(go);
            go.AddComponent<CoinPurseHUD>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            BuildUI();
            EventBus.Subscribe<CoinChangedEvent>(OnCoinChanged);
            SceneManager.sceneLoaded += OnSceneLoaded;
            RefreshTotal();
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<CoinChangedEvent>(OnCoinChanged);
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (Instance == this) Instance = null;
        }

        private void OnSceneLoaded(Scene s, LoadSceneMode m)
        {
            string n = s.name ?? string.Empty;
            bool hide = n.IndexOf("Menu", System.StringComparison.OrdinalIgnoreCase) >= 0
                        || n.IndexOf("Bootstrap", System.StringComparison.OrdinalIgnoreCase) >= 0;
            if (_canvas != null) _canvas.enabled = !hide;
            RefreshTotal();
        }

        private void OnCoinChanged(CoinChangedEvent e)
        {
            RefreshTotal();
            ShowFloater(e.Delta);
        }

        private void RefreshTotal()
        {
            var vs = ServiceLocator.Get<VillageState>();
            int coin = vs != null ? vs.coin : 0;
            if (_coinLabel != null) _coinLabel.text = $"◆ {coin}";
        }

        private void ShowFloater(int delta)
        {
            if (_floater == null || delta == 0) return;
            _floater.text = delta > 0 ? $"+{delta}" : delta.ToString();
            _floater.color = delta > 0 ? GainGreen : SpendSoft;
            if (_floatRoutine != null) StopCoroutine(_floatRoutine);
            _floatRoutine = StartCoroutine(FloatAndFade());
        }

        private IEnumerator FloatAndFade()
        {
            var rt = _floater.rectTransform;
            Vector2 start = new Vector2(0f, -6f);
            float t = 0f, dur = 1.1f;
            _floater.alpha = 1f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float k = t / dur;
                rt.anchoredPosition = start + new Vector2(0f, 26f * k);
                _floater.alpha = 1f - k;
                yield return null;
            }
            _floater.alpha = 0f;
        }

        private void BuildUI()
        {
            var canvasGO = new GameObject("CoinPurseCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.transform.SetParent(transform, false);
            _canvas = canvasGO.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 42;   // just above the ControlHintsHUD band
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // Small rounded plate top-right.
            var plate = new GameObject("Plate", typeof(RectTransform), typeof(Image));
            plate.transform.SetParent(canvasGO.transform, false);
            plate.GetComponent<Image>().color = new Color(0.16f, 0.12f, 0.08f, 0.62f);
            var prt = plate.GetComponent<RectTransform>();
            prt.anchorMin = new Vector2(1f, 1f); prt.anchorMax = new Vector2(1f, 1f);
            prt.pivot = new Vector2(1f, 1f);
            prt.anchoredPosition = new Vector2(-24f, -18f);
            prt.sizeDelta = new Vector2(150f, 56f);

            _coinLabel = MakeLabel(plate.transform, "Coin", Vector2.zero, Vector2.one, 30, Amber, true, TextAlignmentOptions.Center);

            _floater = MakeLabel(plate.transform, "Floater",
                new Vector2(0f, -0.2f), new Vector2(1f, 0.2f), 24, GainGreen, true, TextAlignmentOptions.Center);
            _floater.rectTransform.anchorMin = new Vector2(0f, 0f);
            _floater.rectTransform.anchorMax = new Vector2(1f, 0f);
            _floater.rectTransform.pivot = new Vector2(0.5f, 1f);
            _floater.rectTransform.anchoredPosition = new Vector2(0f, -6f);
            _floater.alpha = 0f;
        }

        private TextMeshProUGUI MakeLabel(Transform parent, string name, Vector2 aMin, Vector2 aMax,
            int fontSize, Color color, bool bold, TextAlignmentOptions align)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = fontSize; tmp.color = color; tmp.alignment = align;
            tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            tmp.raycastTarget = false;
            var rt = tmp.rectTransform;
            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            return tmp;
        }
    }
}
