// SPDX-License-Identifier: MIT
// Hearthbound Hollow — UI / TeaBrewingUI
//
// The kettle interaction screen. The player drops a harvested herb (Lavender
// or Valerian in M1-2) into the kettle. A 90-second timer ticks (skippable
// with auto-complete). When done, publishes TeaBrewedEvent.

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HearthboundHollow.Core;
using HearthboundHollow.Memory;

namespace HearthboundHollow.UI
{
    public class TeaBrewingUI : MonoBehaviour
    {
        [Header("Root")]
        public GameObject root;

        [Header("Herb pick UI")]
        public Transform herbContainer;
        public GameObject herbButtonPrefab;
        public TextMeshProUGUI promptLabel;

        [Header("Brewing state")]
        public GameObject brewingPanel;
        public Slider brewProgress;
        public TextMeshProUGUI brewLabel;
        public Button autoCompleteButton;

        [Header("Tuning")]
        public float brewDurationSeconds = 90f;
        public List<MemoryHerb> availableHerbs = new();

        private MemoryHerb _selected;
        private Coroutine _brewCo;
        private readonly List<GameObject> _spawned = new();

        private void Awake()
        {
            if (root != null) root.SetActive(false);
            if (brewingPanel != null) brewingPanel.SetActive(false);
            if (autoCompleteButton != null) autoCompleteButton.onClick.AddListener(AutoComplete);
        }

        public void Show()
        {
            if (root != null) root.SetActive(true);
            BuildHerbButtons();
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
            if (_brewCo != null) { StopCoroutine(_brewCo); _brewCo = null; }
            ClearHerbButtons();
        }

        private void BuildHerbButtons()
        {
            ClearHerbButtons();
            var vs = ServiceLocator.Get<VillageState>();
            foreach (var herb in availableHerbs)
            {
                if (herb == null) continue;
                bool unlocked = vs != null && vs.harvestedHerbIds.Contains(herb.herbId);
                var go = Instantiate(herbButtonPrefab, herbContainer);
                _spawned.Add(go);
                var label = go.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null) label.text = unlocked ? herb.displayName : $"{herb.displayName} (need to harvest)";
                var btn = go.GetComponent<Button>();
                if (btn != null)
                {
                    btn.interactable = unlocked;
                    btn.onClick.AddListener(() => StartBrew(herb));
                }
            }
        }

        private void ClearHerbButtons()
        {
            foreach (var g in _spawned) if (g != null) Destroy(g);
            _spawned.Clear();
        }

        private void StartBrew(MemoryHerb herb)
        {
            _selected = herb;
            if (brewingPanel != null) brewingPanel.SetActive(true);
            if (brewLabel != null) brewLabel.text = $"Brewing {herb.displayName}…";
            if (brewProgress != null) brewProgress.value = 0f;
            _brewCo = StartCoroutine(BrewCoroutine());
        }

        private IEnumerator BrewCoroutine()
        {
            float t = 0f;
            while (t < brewDurationSeconds)
            {
                t += Time.deltaTime;
                if (brewProgress != null) brewProgress.value = t / brewDurationSeconds;
                yield return null;
            }
            FinishBrew();
        }

        private void AutoComplete()
        {
            if (_brewCo != null) { StopCoroutine(_brewCo); _brewCo = null; }
            FinishBrew();
        }

        private void FinishBrew()
        {
            if (_selected != null)
            {
                EventBus.Publish(new TeaBrewedEvent(_selected));
                Hh.Log(LogCategory.UI, $"Tea brewed: {_selected.herbId}");
            }
            Hide();
        }
    }
}
