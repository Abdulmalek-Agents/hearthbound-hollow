// SPDX-License-Identifier: MIT
// Hearthbound Hollow — EditMode unit tests for Save + Ripple + Memory.
//
// Validates the critical "writes-on-disk are correct + restore round-trips"
// invariant and the RippleEngine math.

using NUnit.Framework;
using UnityEngine;
using HearthboundHollow.Core;
using HearthboundHollow.Memory;
using HearthboundHollow.Save;
using HearthboundHollow.Mission;

namespace HearthboundHollow.Tests.EditMode
{
    public class SaveServiceRoundTripTests
    {
        private VillageState _state;
        private SaveService _service;

        [SetUp]
        public void SetUp()
        {
            _state = ScriptableObject.CreateInstance<VillageState>();
            _state.ResetToDefault();
            _service = new SaveService();
            _service.DeleteSlot(-1);
            _service.DeleteSlot(0);
        }

        [TearDown]
        public void TearDown()
        {
            _service.DeleteSlot(-1);
            _service.DeleteSlot(0);
        }

        [Test]
        public void Save_then_Load_round_trips_all_dimensions()
        {
            _state.trustDoris = 73;
            _state.trustGerrold = 14;
            _state.memoryIntegrityGerrold = 81;
            _state.coin = 27;
            _state.currentDayIndex = 3;
            _state.gentleModeEnabled = true;
            _state.heldMemoryIds.Add("DOR-001");
            _state.heldMemoryIds.Add("GER-007");

            Assert.IsTrue(_service.Save(0, _state), "Save should succeed.");

            var loaded = ScriptableObject.CreateInstance<VillageState>();
            loaded.ResetToDefault();
            Assert.IsTrue(_service.Load(0, loaded), "Load should succeed.");

            Assert.AreEqual(73, loaded.trustDoris);
            Assert.AreEqual(14, loaded.trustGerrold);
            Assert.AreEqual(81, loaded.memoryIntegrityGerrold);
            Assert.AreEqual(27, loaded.coin);
            Assert.AreEqual(3, loaded.currentDayIndex);
            Assert.IsTrue(loaded.gentleModeEnabled);
            Assert.AreEqual(2, loaded.heldMemoryIds.Count);
            Assert.Contains("DOR-001", loaded.heldMemoryIds);
            Assert.Contains("GER-007", loaded.heldMemoryIds);
        }

        [Test]
        public void Save_autosave_uses_slot_negative_one()
        {
            _state.coin = 42;
            Assert.IsTrue(_service.Save(-1, _state));
            Assert.IsTrue(_service.SlotExists(-1));
            Assert.IsFalse(_service.SlotExists(0));
        }

        [Test]
        public void Load_returns_false_when_slot_missing()
        {
            var loaded = ScriptableObject.CreateInstance<VillageState>();
            Assert.IsFalse(_service.Load(2, loaded));
        }

        [Test]
        public void GetSlotLabel_returns_empty_when_no_save()
        {
            Assert.AreEqual("<empty>", _service.GetSlotLabel(2));
        }

        [Test]
        public void DeleteSlot_removes_persisted_file()
        {
            _service.Save(0, _state);
            Assert.IsTrue(_service.SlotExists(0));
            _service.DeleteSlot(0);
            Assert.IsFalse(_service.SlotExists(0));
        }
    }

    public class RippleEngineTests
    {
        private VillageState _state;
        private RippleEngine _engine;

        [SetUp]
        public void SetUp()
        {
            _state = ScriptableObject.CreateInstance<VillageState>();
            _state.ResetToDefault();
            _engine = new RippleEngine();
        }

        [Test]
        public void Erase_tariff_lowers_target_trust_and_memory_integrity()
        {
            var t = ScriptableObject.CreateInstance<TariffSO>();
            t.choice = MoralChoice.Erase;
            t.trustDeltaTarget = -5;
            t.trustRippleNeighbors = -3;
            t.memoryIntegrityDelta = -80;
            t.vow1Delta = -10;
            t.vow3Delta = -3;

            _engine.ApplyTariff(t, _state, "gerrold");

            Assert.AreEqual(45, _state.trustGerrold);       // 50 - 5
            Assert.AreEqual(47, _state.trustDoris);         // 50 - 3 (neighbor ripple)
            Assert.AreEqual(20, _state.memoryIntegrityGerrold); // 100 - 80
            Assert.AreEqual(40, _state.vow1Integrity);
            Assert.AreEqual(47, _state.vow3Integrity);
        }

        [Test]
        public void Listen_tariff_raises_trust_and_vow3()
        {
            var t = ScriptableObject.CreateInstance<TariffSO>();
            t.choice = MoralChoice.Listen;
            t.trustDeltaTarget = 15;
            t.trustRippleNeighbors = 6;
            t.vow3Delta = 10;
            t.vow7Delta = 5;

            _engine.ApplyTariff(t, _state, "gerrold");

            Assert.AreEqual(65, _state.trustGerrold);
            Assert.AreEqual(56, _state.trustDoris);
            Assert.AreEqual(60, _state.vow3Integrity);
            Assert.AreEqual(55, _state.vow7Integrity);
        }

        [Test]
        public void Coin_clamps_at_zero()
        {
            _state.coin = 10;
            var t = ScriptableObject.CreateInstance<TariffSO>();
            t.coinDelta = -50;
            _engine.ApplyTariff(t, _state, "doris");
            Assert.AreEqual(0, _state.coin);
        }

        [Test]
        public void All_integrity_fields_clamp_to_0_100()
        {
            _state.vow1Integrity = 5;
            var t = ScriptableObject.CreateInstance<TariffSO>();
            t.vow1Delta = -50;
            _engine.ApplyTariff(t, _state, "gerrold");
            Assert.AreEqual(0, _state.vow1Integrity);

            _state.vow7Integrity = 95;
            var t2 = ScriptableObject.CreateInstance<TariffSO>();
            t2.vow7Delta = 50;
            _engine.ApplyTariff(t2, _state, "doris");
            Assert.AreEqual(100, _state.vow7Integrity);
        }
    }

    public class MemoryNodeTests
    {
        [Test]
        public void EffectiveTint_returns_overrideTint_when_alpha_nonzero()
        {
            var m = ScriptableObject.CreateInstance<MemoryNodeSO>();
            m.primaryTone = EmotionalTone.Joy;
            m.overrideTint = new Color(0.1f, 0.2f, 0.3f, 1f);
            var tint = m.EffectiveTint;
            Assert.AreEqual(0.1f, tint.r, 0.001f);
            Assert.AreEqual(0.2f, tint.g, 0.001f);
            Assert.AreEqual(0.3f, tint.b, 0.001f);
        }

        [Test]
        public void EffectiveTint_falls_back_to_primary_tone_when_override_clear()
        {
            var m = ScriptableObject.CreateInstance<MemoryNodeSO>();
            m.primaryTone = EmotionalTone.Grief;
            m.overrideTint = Color.clear;
            var expected = EmotionalTone.Grief.GetPaletteTint();
            Assert.AreEqual(expected, m.EffectiveTint);
        }
    }

    public class VillagerMemoryRuntimeTests
    {
        [Test]
        public void Reveal_then_IsRevealed_returns_true()
        {
            var rt = new VillagerMemoryRuntime { villagerId = "doris" };
            var m = ScriptableObject.CreateInstance<MemoryNodeSO>();
            m.id = "DOR-001";
            Assert.IsFalse(rt.IsRevealed(m));
            rt.Reveal(m);
            Assert.IsTrue(rt.IsRevealed(m));
        }

        [Test]
        public void Reveal_is_idempotent()
        {
            var rt = new VillagerMemoryRuntime { villagerId = "doris" };
            var m = ScriptableObject.CreateInstance<MemoryNodeSO>();
            m.id = "DOR-001";
            rt.Reveal(m);
            rt.Reveal(m);
            rt.Reveal(m);
            Assert.AreEqual(1, rt.revealedMemoryIds.Count);
        }
    }
}
