// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / HollowProgressionService  (Engagement Pillar P3 — Phase 64)
//
// "MY HOLLOW": the compounding spend-sink that finally answers "why earn coin?"
// (Docs/Engagement_Bible/06). Coin earned from visits (P2) + echoes (P6) + teas
// (P4) buys gentle, cozy upgrades — a shelf, a relit room, a garden bed, Marin's
// polishing cloth, a cushion for Pickle. Upgrades are WARMTH, not power; none is
// ever required to progress (Cozy Contract).
//
// OWNS: the catalog (authored Resources/HollowCatalog, else a built-in starter
// set), purchase validation + coin deduction, persistence in
// VillageState.purchasedUpgradeIds, scene-marker reveal, and the
// HollowShopBoard view snapshot the UI reads (no UI→Mission dep, D-035).
//
// SAFE DROP: self-installing, observer-only. Purchases work even if a scene
// marker isn't pre-placed (GameObject.Find no-ops gracefully) — the growth is
// recorded + shown in the shop register and the coin economy regardless; the
// 3D reveal lands when a builder pre-places the markers.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using HearthboundHollow.Core;
using HearthboundHollow.Memory;

namespace HearthboundHollow.Mission
{
    public class HollowProgressionService : MonoBehaviour
    {
        public static HollowProgressionService Instance { get; private set; }

        private VillageState _vs;
        private readonly List<Entry> _catalog = new();

        private sealed class Entry
        {
            public string id, name, flavor, category, sceneMarkerId;
            public int cost, capacityDelta;
            public List<string> requires = new();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInstall()
        {
            if (Instance != null) return;
            var go = new GameObject("_HHHollowProgression");
            DontDestroyOnLoad(go);
            go.AddComponent<HollowProgressionService>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            ServiceLocator.Register(this);
            BuildCatalog();
            EventBus.Subscribe<HollowPurchaseRequestedEvent>(OnPurchaseRequested);
            EventBus.Subscribe<CoinChangedEvent>(OnCoinChanged);
            EventBus.Subscribe<VillageStateLoadedEvent>(OnStateLoaded);
            EventBus.Subscribe<DayStartedEvent>(OnDayStarted);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<HollowPurchaseRequestedEvent>(OnPurchaseRequested);
            EventBus.Unsubscribe<CoinChangedEvent>(OnCoinChanged);
            EventBus.Unsubscribe<VillageStateLoadedEvent>(OnStateLoaded);
            EventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (Instance == this) Instance = null;
        }

        private void OnCoinChanged(CoinChangedEvent e) => RecomputeViews();
        private void OnDayStarted(DayStartedEvent e) => RecomputeViews();
        private void OnStateLoaded(VillageStateLoadedEvent e) { _vs = e.VillageState as VillageState; RecomputeViews(); }
        private void OnSceneLoaded(Scene s, LoadSceneMode m) { ReapplyAll(); }

        // ───── Catalog ───────────────────────────────────

        private void BuildCatalog()
        {
            _catalog.Clear();
            var asset = Resources.Load<HollowCatalogSO>("HollowCatalog");
            if (asset != null && asset.upgrades != null && asset.upgrades.Count > 0)
            {
                foreach (var u in asset.upgrades)
                {
                    if (u == null) continue;
                    _catalog.Add(new Entry
                    {
                        id = u.upgradeId, name = u.displayName, flavor = u.flavor,
                        category = u.category.ToString(), cost = Mathf.Max(0, u.coinCost),
                        sceneMarkerId = u.sceneMarkerId, capacityDelta = u.capacityDelta,
                        requires = u.requiresUpgradeIds != null ? new List<string>(u.requiresUpgradeIds) : new List<string>(),
                    });
                }
            }
            else
            {
                // Built-in starter catalog (Docs/Engagement_Bible/06 §4).
                _catalog.Add(MakeEntry("SHELF_WINDOW_01", "Shelf by the window", "Shelf", 10,
                    "A shop with full shelves keeps better company.", "_HollowUpgrade_ShelfWindow", 3));
                _catalog.Add(MakeEntry("SHELF_HEARTH_02", "Shelf by the hearth", "Shelf", 18,
                    "The hearth wall wants company.", "_HollowUpgrade_ShelfHearth", 3, "SHELF_WINDOW_01"));
                _catalog.Add(MakeEntry("ROOM_UPSTAIRS_LIGHT", "Relight the upstairs", "Room", 30,
                    "Marin's reading nook waits in the dark.", "_HollowUpgrade_Upstairs", 0));
                _catalog.Add(MakeEntry("GARDEN_BED_03", "Clear a garden bed", "GardenBed", 14,
                    "Two beds lie fallow. One could be yours by spring.", "_HollowUpgrade_GardenBed", 1));
                _catalog.Add(MakeEntry("TOOL_SOFT_CLOTH", "Marin's polishing cloth", "Tool", 12,
                    "It remembers a gentler hand. Polishing comes a touch easier.", "", 0));
                _catalog.Add(MakeEntry("DECOR_PICKLE_CUSHION", "A cushion for Pickle", "Decor", 8,
                    "He has opinions about soft things. All of them favourable.", "_HollowUpgrade_Cushion", 0));
            }
            RecomputeViews();
        }

        private static Entry MakeEntry(string id, string name, string cat, int cost, string flavor,
                                       string marker, int capacity, params string[] requires)
            => new Entry
            {
                id = id, name = name, category = cat, cost = cost, flavor = flavor,
                sceneMarkerId = marker, capacityDelta = capacity,
                requires = new List<string>(requires ?? new string[0]),
            };

        // ───── Views (UI snapshot) ────────────────────────────

        public void RecomputeViews()
        {
            _vs ??= ServiceLocator.Get<VillageState>();
            int coin = _vs != null ? _vs.coin : 0;
            var purchased = _vs != null && _vs.purchasedUpgradeIds != null ? _vs.purchasedUpgradeIds : new List<string>();

            HollowShopBoard.Catalog.Clear();
            foreach (var e in _catalog)
            {
                bool isBought = purchased.Contains(e.id);
                bool locked = false; string lockHint = "";
                foreach (var req in e.requires)
                    if (!purchased.Contains(req)) { locked = true; lockHint = $"after \u201c{DisplayOf(req)}\u201d"; break; }
                bool affordable = !isBought && !locked && coin >= e.cost;
                if (!isBought && !locked && !affordable) lockHint = "a few more coins";

                HollowShopBoard.Catalog.Add(new HollowUpgradeView
                {
                    upgradeId = e.id, displayName = e.name, flavor = e.flavor, category = e.category,
                    coinCost = e.cost, purchased = isBought, affordable = affordable,
                    locked = locked, lockHint = lockHint,
                });
            }
            HollowShopBoard.Raise();
        }

        private string DisplayOf(string id)
        {
            var e = _catalog.Find(x => x.id == id);
            return e != null ? e.name : id;
        }

        // ───── Purchase ───────────────────────────────────

        private void OnPurchaseRequested(HollowPurchaseRequestedEvent ev)
        {
            _vs ??= ServiceLocator.Get<VillageState>();
            if (_vs == null) return;
            var e = _catalog.Find(x => x.id == ev.UpgradeId);
            if (e == null) return;

            if (_vs.purchasedUpgradeIds == null) _vs.purchasedUpgradeIds = new List<string>();
            if (_vs.purchasedUpgradeIds.Contains(e.id)) return;            // already owned
            foreach (var req in e.requires)
                if (!_vs.purchasedUpgradeIds.Contains(req)) return;        // prereq not met (silent — UI greys it)
            if (_vs.coin < e.cost) return;                                  // can't afford (silent — UI greys it)

            _vs.coin -= e.cost;
            _vs.purchasedUpgradeIds.Add(e.id);
            EventBus.Publish(new CoinChangedEvent(_vs.coin, -e.cost, "upgrade"));

            ApplyEffect(e);
            ApplyMarker(e);

            EventBus.Publish(new HollowUpgradePurchasedEvent(e.id));
            Hh.Log(LogCategory.Mission, $"[Hollow] Purchased '{e.id}' (-{e.cost} coin → {_vs.coin}).");
            RecomputeViews();
        }

        /// <summary>Gameplay effect of an upgrade (capacity, a new garden bed, etc.).</summary>
        private void ApplyEffect(Entry e)
        {
            if (e.category == "GardenBed")
            {
                if (_vs.gardenBeds == null) _vs.gardenBeds = new List<GardenBedState>();
                _vs.gardenBeds.Add(new GardenBedState { bedId = $"BED_BOUGHT_{_vs.gardenBeds.Count + 1}", plantedHerbId = "" });
            }
            // Tools/decor/room effects are read where relevant (e.g. TOOL_SOFT_CLOTH by
            // the bench's gentle-mastery aid in P5) via IsPurchased(id).
        }

        private void ApplyMarker(Entry e)
        {
            if (string.IsNullOrEmpty(e.sceneMarkerId)) return;
            // Path A — an ACTIVE, named marker (authored-catalog style). Original behaviour.
            var byName = GameObject.Find(e.sceneMarkerId);
            if (byName != null) { byName.SetActive(true); return; }
            // Path B (Phase 73) — markers pre-placed HIDDEN by the editor builder.
            // GameObject.Find can't see inactive objects, so resolve by component
            // (FindObjectsInactive.Include DOES) and reveal the matching one.
            foreach (var mk in FindHiddenMarkers())
                if (mk != null && mk.markerId == e.sceneMarkerId)
                    mk.gameObject.SetActive(true);
        }

        // All HollowUpgradeMarker components in the loaded scenes, including
        // inactive ones (that's the whole point — they start hidden).
        private static HollowUpgradeMarker[] FindHiddenMarkers()
            => Object.FindObjectsByType<HollowUpgradeMarker>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        /// <summary>Re-activate every purchased upgrade's marker on scene load.</summary>
        public void ReapplyAll()
        {
            _vs ??= ServiceLocator.Get<VillageState>();
            if (_vs == null || _vs.purchasedUpgradeIds == null) return;
            foreach (var e in _catalog)
                if (_vs.purchasedUpgradeIds.Contains(e.id)) ApplyMarker(e);
        }

        public bool IsPurchased(string id)
            => _vs != null && _vs.purchasedUpgradeIds != null && _vs.purchasedUpgradeIds.Contains(id);
    }
}
