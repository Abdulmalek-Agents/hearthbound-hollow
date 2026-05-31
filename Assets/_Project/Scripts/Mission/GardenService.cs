// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / GardenService  (Engagement Pillar P4 — Phase 65)
//
// THE GROW → BREW → USE LOOP (Docs/Engagement_Bible/07) — the cozy daily-check
// ritual ("did my lavender ripen?") that also SUPPLIES the heart of the game:
// herbs become teas, and teas are gentle, opt-in TOOLS that ripple into visits
// and craft. Activates the loop the bought HarvestGarden asset was meant to power.
//
// OWNS: garden beds (persisted in VillageState.gardenBeds), day-tick growth, the
// morning Agenda's garden lines, planting/harvest/water/brew/sell, the herb+tea
// inventory (kept in VillageState.materials as "herb_<id>" / "tea_<id>" tokens),
// and the active-tea effect consumed on the next kept memory.
//
// Writes the UI-readable snapshot to Core's GardenBoard (no UI→Mission dep, D-035).
// Self-installing; seeds a few starter beds on first run so the loop is alive
// immediately. Everything is OPT-IN — skipping the garden never blocks anything.

using System.Collections.Generic;
using UnityEngine;
using HearthboundHollow.Core;

namespace HearthboundHollow.Mission
{
    public class GardenService : MonoBehaviour
    {
        public static GardenService Instance { get; private set; }

        private VillageState _vs;
        private string _activeTeaHerb = "";   // brewed tea waiting to help the next visit/craft

        private sealed class Herb
        {
            public string id, name, effect; public int growDays; public int sellPrice;
            public Herb(string i, string n, int g, string e, int s) { id = i; name = n; growDays = g; effect = e; sellPrice = s; }
        }

        // Built-in herb table (works with zero authored MemoryHerb assets).
        private static readonly Herb[] Herbs =
        {
            new Herb("lavender",  "Lavender",  2, "openup", 5),
            new Herb("valerian",  "Valerian",  3, "calm",   6),
            new Herb("chamomile", "Chamomile", 2, "warm",   4),
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInstall()
        {
            if (Instance != null) return;
            var go = new GameObject("_HHGarden");
            DontDestroyOnLoad(go);
            go.AddComponent<GardenService>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            ServiceLocator.Register(this);
            EventBus.Subscribe<DayStartedEvent>(OnDayStarted);
            EventBus.Subscribe<VillageStateLoadedEvent>(OnStateLoaded);
            EventBus.Subscribe<GardenActionRequestedEvent>(OnAction);
            EventBus.Subscribe<MemoryKeptEvent>(OnMemoryKept);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
            EventBus.Unsubscribe<VillageStateLoadedEvent>(OnStateLoaded);
            EventBus.Unsubscribe<GardenActionRequestedEvent>(OnAction);
            EventBus.Unsubscribe<MemoryKeptEvent>(OnMemoryKept);
            if (Instance == this) Instance = null;
        }

        private void OnStateLoaded(VillageStateLoadedEvent e) { _vs = e.VillageState as VillageState; EnsureStarterBeds(); Recompute(true); }

        private void OnDayStarted(DayStartedEvent e)
        {
            _vs ??= ServiceLocator.Get<VillageState>();
            EnsureStarterBeds();
            // Feed the morning Agenda with garden status (the compounding payoff of yesterday).
            var agenda = DailyLoopService.Instance != null ? DailyLoopService.Instance.CurrentAgenda : null;
            if (agenda != null && _vs != null && _vs.gardenBeds != null)
            {
                foreach (var bed in _vs.gardenBeds)
                {
                    if (bed == null || string.IsNullOrEmpty(bed.plantedHerbId)) continue;
                    var h = Find(bed.plantedHerbId);
                    if (h == null) continue;
                    int left = (bed.dayPlanted + h.growDays) - _vs.currentDayIndex;
                    agenda.gardenNotes.Add(left <= 0
                        ? $"{h.name} — ready to harvest"
                        : $"{h.name} — {left} day(s) to ripen");
                }
            }
            Recompute(true);
        }

        private void OnMemoryKept(MemoryKeptEvent e)
        {
            // A brewed tea gently helps the visit you just had (opt-in tool).
            if (string.IsNullOrEmpty(_activeTeaHerb)) return;
            _vs ??= ServiceLocator.Get<VillageState>();
            if (_vs == null) return;
            var h = Find(_activeTeaHerb);
            _activeTeaHerb = "";
            if (h == null) { Recompute(false); return; }

            switch (h.effect)
            {
                case "openup":   // they opened up — a richer keepsake (+3 coin)
                    _vs.coin += 3; EventBus.Publish(new CoinChangedEvent(_vs.coin, 3, "tea"));
                    break;
                case "calm":     // a steadier hand — a touch of mastery
                    _vs.keeperHandCraftCount += 1;
                    break;
                case "warm":     // a cup for you — Pickle warms
                    _vs.pickleApproval = VillageState.Adjust(_vs.pickleApproval, 3);
                    break;
            }
            Hh.Log(LogCategory.Mission, $"[Garden] {h.name} tea helped the visit ({h.effect}).");
            Recompute(false);
        }

        // ───── Actions (from the UI) ───────────────────────────

        private void OnAction(GardenActionRequestedEvent e)
        {
            _vs ??= ServiceLocator.Get<VillageState>();
            if (_vs == null) return;
            switch ((e.Action ?? "").ToLowerInvariant())
            {
                case "plant":   Plant(e.Arg); break;
                case "harvest": Harvest(e.Arg); break;
                case "water":   Water(e.Arg); break;
                case "brew":    Brew(e.Arg); break;
                case "sell":    Sell(e.Arg); break;
            }
            Recompute(false);
        }

        private void Plant(string herbId)
        {
            var h = Find(herbId); if (h == null) return;
            var bed = (_vs.gardenBeds ?? new List<GardenBedState>()).Find(b => b != null && string.IsNullOrEmpty(b.plantedHerbId));
            if (bed == null) return;   // no free bed (cozy — just no-op)
            bed.plantedHerbId = herbId;
            bed.dayPlanted = _vs.currentDayIndex;
            bed.watered = false;
            Hh.Log(LogCategory.Mission, $"[Garden] Planted {h.name}.");
        }

        private void Harvest(string bedId)
        {
            if (_vs.gardenBeds == null) return;
            var bed = _vs.gardenBeds.Find(b => b != null && b.bedId == bedId);
            if (bed == null || string.IsNullOrEmpty(bed.plantedHerbId)) return;
            var h = Find(bed.plantedHerbId); if (h == null) return;
            if (_vs.currentDayIndex < bed.dayPlanted + h.growDays) return;   // not ripe yet
            AddToken($"herb_{h.id}", +1);
            if (_vs.harvestedHerbIds != null && !_vs.harvestedHerbIds.Contains(h.id)) _vs.harvestedHerbIds.Add(h.id);
            bed.plantedHerbId = ""; bed.watered = false;
            Hh.Log(LogCategory.Mission, $"[Garden] Harvested {h.name}.");
        }

        private void Water(string bedId)
        {
            if (_vs.gardenBeds == null) return;
            var bed = _vs.gardenBeds.Find(b => b != null && b.bedId == bedId);
            if (bed != null) bed.watered = true;   // cozy: purely nice; plants never wilt
        }

        private void Brew(string herbId)
        {
            var h = Find(herbId); if (h == null) return;
            if (CountToken($"herb_{h.id}") <= 0) return;
            AddToken($"herb_{h.id}", -1);
            AddToken($"tea_{h.id}", +1);
            _activeTeaHerb = h.id;   // the freshly brewed cup helps your next visit
            Hh.Log(LogCategory.Mission, $"[Garden] Brewed {h.name} tea.");
        }

        private void Sell(string herbId)
        {
            var h = Find(herbId); if (h == null) return;
            // Prefer selling a brewed tea (worth more); else a raw herb.
            if (CountToken($"tea_{h.id}") > 0) { AddToken($"tea_{h.id}", -1); Earn(h.sellPrice + 3); }
            else if (CountToken($"herb_{h.id}") > 0) { AddToken($"herb_{h.id}", -1); Earn(h.sellPrice); }
        }

        private void Earn(int amount)
        {
            _vs.coin += amount;
            EventBus.Publish(new CoinChangedEvent(_vs.coin, amount, "tea"));
        }

        // ───── Snapshot for the UI ─────────────────────────

        public void Recompute(bool _)
        {
            _vs ??= ServiceLocator.Get<VillageState>();
            var d = GardenBoard.Data;
            d.beds.Clear(); d.herbs.Clear(); d.teas.Clear(); d.plantableHerbIds.Clear();
            if (_vs == null) { GardenBoard.Raise(); return; }

            if (_vs.gardenBeds != null)
                foreach (var bed in _vs.gardenBeds)
                {
                    if (bed == null) continue;
                    var view = new GardenBedView { bedId = bed.bedId };
                    if (string.IsNullOrEmpty(bed.plantedHerbId))
                    {
                        view.empty = true; view.stageLabel = "fallow";
                    }
                    else
                    {
                        var h = Find(bed.plantedHerbId);
                        view.empty = false;
                        view.herbId = bed.plantedHerbId;
                        view.herbName = h != null ? h.name : bed.plantedHerbId;
                        int growDays = h != null ? h.growDays : 3;
                        int left = (bed.dayPlanted + growDays) - _vs.currentDayIndex;
                        view.ripe = left <= 0;
                        view.daysLeft = Mathf.Max(0, left);
                        view.stageLabel = view.ripe ? "ripe" : $"{view.daysLeft}d";
                    }
                    d.beds.Add(view);
                }

            foreach (var h in Herbs)
            {
                d.plantableHerbIds.Add(h.id);
                int herbCount = CountToken($"herb_{h.id}");
                int teaCount = CountToken($"tea_{h.id}");
                if (herbCount > 0) d.herbs[h.id] = herbCount;
                if (teaCount > 0) d.teas[h.id] = teaCount;
            }

            d.activeTeaLine = string.IsNullOrEmpty(_activeTeaHerb)
                ? ""
                : $"A fresh cup of {NameOf(_activeTeaHerb)} steeps — it will help your next visit.";

            GardenBoard.Raise();
        }

        // ───── Helpers ───────────────────────────────────

        public string NameOf(string herbId) { var h = Find(herbId); return h != null ? h.name : herbId; }
        private static Herb Find(string id) { foreach (var h in Herbs) if (h.id == id) return h; return null; }

        private void EnsureStarterBeds()
        {
            _vs ??= ServiceLocator.Get<VillageState>();
            if (_vs == null) return;
            if (_vs.gardenBeds == null) _vs.gardenBeds = new List<GardenBedState>();
            if (_vs.gardenBeds.Count > 0) return;   // already seeded / authored
            _vs.gardenBeds.Add(new GardenBedState { bedId = "BED_1", plantedHerbId = "lavender", dayPlanted = _vs.currentDayIndex });
            _vs.gardenBeds.Add(new GardenBedState { bedId = "BED_2", plantedHerbId = "valerian", dayPlanted = _vs.currentDayIndex });
            _vs.gardenBeds.Add(new GardenBedState { bedId = "BED_3", plantedHerbId = "" });
            _vs.gardenBeds.Add(new GardenBedState { bedId = "BED_4", plantedHerbId = "" });
        }

        private int CountToken(string token)
        {
            if (_vs?.materials == null) return 0;
            int n = 0; foreach (var m in _vs.materials) if (m == token) n++; return n;
        }

        private void AddToken(string token, int delta)
        {
            if (_vs.materials == null) _vs.materials = new List<string>();
            if (delta > 0) for (int i = 0; i < delta; i++) _vs.materials.Add(token);
            else for (int i = 0; i < -delta; i++) { int idx = _vs.materials.LastIndexOf(token); if (idx >= 0) _vs.materials.RemoveAt(idx); }
        }
    }
}
