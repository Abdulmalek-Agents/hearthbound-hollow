// SPDX-License-Identifier: MIT
// Hearthbound Hollow — Mission / WorkbenchService  (Engagement Pillar P5 — Phase 66)
//
// THE LIVING WORKBENCH (Docs/Engagement_Bible/08) — the fix for "two flat verbs,
// no mastery, the 2nd craft is the 20th." Kept memories can be TENDED at the
// bench. Each memory asks for one of several VERBS (Polish / Cleanse / Sort /
// Steep) chosen deterministically from its id, so different memories feel
// different. Tending is no-fail + always available (Cozy Contract) but carries a
// gentle "Perfect" spectrum whose odds rise with the hidden Keeper's Hand mastery
// — repetition that quietly improves, never punishes. Each tend pays into the
// loop: a little coin, a mastery tick, a kept-clean memory.
//
// OWNS the consequence of a CraftRequestedEvent (intent from WorkbenchUI): marks
// the memory tended (persisted as a "tended_<id>" token in VillageState.materials),
// awards coin, bumps keeperHandCraftCount, and publishes MemoryTendedEvent (juice +
// Pickle + Ledger consume it). Self-installing, observer-only (no UI→Mission dep).

using System.Collections.Generic;
using UnityEngine;
using HearthboundHollow.Core;

namespace HearthboundHollow.Mission
{
    public class WorkbenchService : MonoBehaviour
    {
        public static WorkbenchService Instance { get; private set; }

        private VillageState _vs;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInstall()
        {
            if (Instance != null) return;
            var go = new GameObject("_HHWorkbench");
            DontDestroyOnLoad(go);
            go.AddComponent<WorkbenchService>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            ServiceLocator.Register(this);
            EventBus.Subscribe<CraftRequestedEvent>(OnCraftRequested);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<CraftRequestedEvent>(OnCraftRequested);
            if (Instance == this) Instance = null;
        }

        private void OnCraftRequested(CraftRequestedEvent e)
        {
            _vs ??= ServiceLocator.Get<VillageState>();
            if (_vs == null || string.IsNullOrEmpty(e.MemoryId)) return;
            if (IsTended(e.MemoryId)) return;   // already cared for (cozy — no double reward)

            // Mark tended (persisted).
            if (_vs.materials == null) _vs.materials = new List<string>();
            _vs.materials.Add($"tended_{e.MemoryId}");

            // Gentle mastery: every tend nudges the hidden Keeper's Hand.
            _vs.keeperHandCraftCount += 1;

            // The "Perfect" spectrum — odds rise with mastery, but Acceptable is fine.
            bool perfect = RollPerfect(_vs.keeperHandCraftCount, e.MemoryId);

            // Pays into the loop: a little coin (more on a Perfect).
            int reward = perfect ? 3 : 2;
            _vs.coin += reward;
            EventBus.Publish(new CoinChangedEvent(_vs.coin, reward, "craft"));

            EventBus.Publish(new MemoryTendedEvent(e.MemoryId, e.Verb, perfect));
            // Echo the legacy craft event so any existing listeners (audio/FX) react.
            EventBus.Publish(new MemorySortedEvent(null, false));
            Hh.Log(LogCategory.MiniGame,
                $"[Bench] Tended '{e.MemoryId}' ({e.Verb}){(perfect ? " — Perfect" : "")}; mastery {_vs.keeperHandCraftCount}.");
        }

        public bool IsTended(string memoryId)
        {
            if (_vs == null) _vs = ServiceLocator.Get<VillageState>();
            return _vs != null && _vs.materials != null && _vs.materials.Contains($"tended_{memoryId}");
        }

        private static bool RollPerfect(int mastery, string seed)
        {
            // 15% at zero mastery → up to ~70% at high mastery. Cosmetic only.
            float chance = Mathf.Clamp01(0.15f + mastery * 0.03f);
            int h = (seed ?? "").GetHashCode() ^ (mastery * 73856093);
            float r = (Mathf.Abs(h) % 1000) / 1000f;
            return r < chance;
        }
    }
}
