// SPDX-License-Identifier: MIT
// Hearthbound Hollow — EditMode unit tests for the Engagement loop (Phases 62–67).
//
// Pure-logic coverage for the new cozy-loop systems. These are intentionally
// dependency-light (no scene, no Play mode): they exercise the Core helpers and
// the save round-trip that the loop relies on to COMPOUND across save/load.

using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using HearthboundHollow.Core;
using HearthboundHollow.Save;

namespace HearthboundHollow.Tests.EditMode
{
    public class VillageStateFlagsTests
    {
        private VillageState _vs;

        [SetUp]
        public void Setup() => _vs = ScriptableObject.CreateInstance<VillageState>();

        [TearDown]
        public void Teardown() { if (_vs != null) Object.DestroyImmediate(_vs); }

        [Test]
        public void IsSet_reads_named_bool_field()
        {
            _vs.metDoris = true;
            Assert.IsTrue(VillageStateFlags.IsSet("metDoris", _vs));
            Assert.IsFalse(VillageStateFlags.IsSet("metGerrold", _vs));
        }

        [Test]
        public void IsSet_is_case_insensitive_and_trims()
        {
            _vs.firstMoralChoiceMade = true;
            Assert.IsTrue(VillageStateFlags.IsSet("  FIRSTMORALCHOICEMADE ", _vs));
        }

        [Test]
        public void Set_writes_named_bool_field()
        {
            VillageStateFlags.Set("echoHologramHeard", true, _vs);
            Assert.IsTrue(_vs.echoHologramHeard);
        }

        [Test]
        public void Missing_or_nonbool_flag_is_false_and_noop()
        {
            Assert.IsFalse(VillageStateFlags.IsSet("doesNotExist", _vs));
            Assert.IsFalse(VillageStateFlags.IsSet("coin", _vs));   // int field, not bool
            Assert.DoesNotThrow(() => VillageStateFlags.Set("doesNotExist", true, _vs));
        }

        [Test]
        public void AllSet_and_AnySet_behave()
        {
            _vs.metDoris = true; _vs.metGerrold = false;
            Assert.IsTrue(VillageStateFlags.AllSet(new[] { "metDoris" }, _vs));
            Assert.IsFalse(VillageStateFlags.AllSet(new[] { "metDoris", "metGerrold" }, _vs));
            Assert.IsTrue(VillageStateFlags.AnySet(new[] { "metDoris", "metGerrold" }, _vs));
            Assert.IsTrue(VillageStateFlags.AllSet(null, _vs));    // empty gate = eligible
            Assert.IsFalse(VillageStateFlags.AnySet(null, _vs));
        }
    }

    public class CraftVerbsTests
    {
        [Test]
        public void VerbFor_is_deterministic_and_in_set()
        {
            string a = CraftVerbs.VerbFor("MEM_DORIS_001");
            string b = CraftVerbs.VerbFor("MEM_DORIS_001");
            Assert.AreEqual(a, b, "Same id must always map to the same verb.");
            CollectionAssert.Contains(CraftVerbs.Verbs, a);
        }

        [Test]
        public void VerbFor_handles_null_and_empty()
        {
            Assert.AreEqual("polish", CraftVerbs.VerbFor(null));
            Assert.AreEqual("polish", CraftVerbs.VerbFor(""));
        }

        [Test]
        public void Labels_flavor_and_mastery_are_never_empty()
        {
            foreach (var v in CraftVerbs.Verbs)
            {
                Assert.IsFalse(string.IsNullOrEmpty(CraftVerbs.Label(v)));
                Assert.IsFalse(string.IsNullOrEmpty(CraftVerbs.Flavor(v)));
            }
            Assert.IsFalse(string.IsNullOrEmpty(CraftVerbs.MasteryFlavor(0)));
            Assert.IsFalse(string.IsNullOrEmpty(CraftVerbs.MasteryFlavor(50)));
        }
    }

    public class DayAgendaTests
    {
        [Test]
        public void New_agenda_is_quiet_and_empty()
        {
            var a = new DayAgenda();
            Assert.IsTrue(a.IsQuietDay);
            Assert.AreEqual(0, a.tickets.Count);
        }

        [Test]
        public void Adding_a_visitor_makes_the_day_not_quiet()
        {
            var a = new DayAgenda();
            a.visitors.Add("Doris — \"a sweet thing to ask\"");
            a.tickets.Add(new RequestTicket { requestId = "r1", villagerName = "Doris", coinReward = 5 });
            Assert.IsFalse(a.IsQuietDay);
            Assert.AreEqual(5, a.tickets[0].coinReward);
        }
    }

    /// <summary>
    /// The loop only COMPOUNDS if the new engagement state survives save/load.
    /// This guards the schema-v3 round-trip (resolved requests, upgrades, materials,
    /// echoes, Keeper's Hand, garden beds).
    /// </summary>
    public class EngagementSnapshotTests
    {
        [Test]
        public void Schema_v3_round_trips_the_loop_state()
        {
            var src = ScriptableObject.CreateInstance<VillageState>();
            try
            {
                src.coin = 73;
                src.resolvedRequestIds = new List<string> { "walkin_doris_0", "DOR_002" };
                src.purchasedUpgradeIds = new List<string> { "SHELF_WINDOW_01" };
                src.materials = new List<string> { "herb_lavender", "tea_lavender", "tended_MEM_X" };
                src.completedEchoIds = new List<string> { "ECHO_SUNDAY_KITCHEN" };
                src.keeperHandCraftCount = 11;
                src.gardenBeds = new List<GardenBedState>
                {
                    new GardenBedState { bedId = "BED_1", plantedHerbId = "lavender", dayPlanted = 2, watered = true },
                    new GardenBedState { bedId = "BED_2", plantedHerbId = "", dayPlanted = 0, watered = false },
                };

                var snap = VillageStateSnapshot.FromState(src);
                Assert.AreEqual(3, snap.schemaVersion);

                var dst = ScriptableObject.CreateInstance<VillageState>();
                try
                {
                    snap.ApplyTo(dst);
                    Assert.AreEqual(73, dst.coin);
                    CollectionAssert.AreEqual(src.resolvedRequestIds, dst.resolvedRequestIds);
                    CollectionAssert.AreEqual(src.purchasedUpgradeIds, dst.purchasedUpgradeIds);
                    CollectionAssert.AreEqual(src.materials, dst.materials);
                    CollectionAssert.AreEqual(src.completedEchoIds, dst.completedEchoIds);
                    Assert.AreEqual(11, dst.keeperHandCraftCount);
                    Assert.AreEqual(2, dst.gardenBeds.Count);
                    Assert.AreEqual("lavender", dst.gardenBeds[0].plantedHerbId);
                    Assert.AreEqual(2, dst.gardenBeds[0].dayPlanted);
                    Assert.IsTrue(dst.gardenBeds[0].watered);
                }
                finally { Object.DestroyImmediate(dst); }
            }
            finally { Object.DestroyImmediate(src); }
        }

        [Test]
        public void ResetToDefault_clears_loop_state()
        {
            var vs = ScriptableObject.CreateInstance<VillageState>();
            try
            {
                vs.resolvedRequestIds.Add("x");
                vs.purchasedUpgradeIds.Add("y");
                vs.keeperHandCraftCount = 5;
                vs.gardenBeds.Add(new GardenBedState { bedId = "B" });
                vs.ResetToDefault();
                Assert.AreEqual(0, vs.resolvedRequestIds.Count);
                Assert.AreEqual(0, vs.purchasedUpgradeIds.Count);
                Assert.AreEqual(0, vs.keeperHandCraftCount);
                Assert.AreEqual(0, vs.gardenBeds.Count);
            }
            finally { Object.DestroyImmediate(vs); }
        }
    }
}
