// SPDX-License-Identifier: MIT
// Hearthbound Hollow — EditMode unit tests for Core subsystem.

using NUnit.Framework;
using HearthboundHollow.Core;

namespace HearthboundHollow.Tests.EditMode
{
    public class EventBusTests
    {
        private struct PingEvent { public int Value; }

        [SetUp]
        public void Setup() => EventBus.ClearAll();

        [Test]
        public void Publish_invokes_all_subscribers_in_order()
        {
            int total = 0;
            EventBus.Subscribe<PingEvent>(e => total += e.Value);
            EventBus.Subscribe<PingEvent>(e => total += e.Value * 10);
            EventBus.Publish(new PingEvent { Value = 3 });
            Assert.AreEqual(33, total);
        }

        [Test]
        public void Unsubscribe_removes_handler()
        {
            int total = 0;
            System.Action<PingEvent> handler = e => total += e.Value;
            EventBus.Subscribe(handler);
            EventBus.Publish(new PingEvent { Value = 5 });
            EventBus.Unsubscribe(handler);
            EventBus.Publish(new PingEvent { Value = 7 });
            Assert.AreEqual(5, total);
        }

        [Test]
        public void SubscriberCount_reflects_state()
        {
            Assert.AreEqual(0, EventBus.SubscriberCount<PingEvent>());
            System.Action<PingEvent> a = _ => { };
            System.Action<PingEvent> b = _ => { };
            EventBus.Subscribe(a);
            EventBus.Subscribe(b);
            Assert.AreEqual(2, EventBus.SubscriberCount<PingEvent>());
            EventBus.Unsubscribe(a);
            Assert.AreEqual(1, EventBus.SubscriberCount<PingEvent>());
        }
    }

    public class VillageStateTests
    {
        [Test]
        public void Adjust_clamps_to_valid_range()
        {
            Assert.AreEqual(0, VillageState.Adjust(5, -10));
            Assert.AreEqual(100, VillageState.Adjust(95, 50));
            Assert.AreEqual(50, VillageState.Adjust(50, 0));
            Assert.AreEqual(40, VillageState.Adjust(50, -10));
            Assert.AreEqual(60, VillageState.Adjust(50, 10));
        }

        [Test]
        public void ResetToDefault_returns_fields_to_canonical_state()
        {
            var s = UnityEngine.ScriptableObject.CreateInstance<VillageState>();
            s.trustDoris = 13;
            s.trustGerrold = 87;
            s.memoryIntegrityGerrold = 5;
            s.coin = 999;
            s.currentDayIndex = 99;
            s.tutorialCompleted = true;
            s.heldMemoryIds.Add("FAKE-001");
            s.ResetToDefault();
            Assert.AreEqual(50, s.trustDoris);
            Assert.AreEqual(50, s.trustGerrold);
            Assert.AreEqual(100, s.memoryIntegrityGerrold);
            Assert.AreEqual(50, s.coin);
            Assert.AreEqual(0, s.currentDayIndex);
            Assert.IsFalse(s.tutorialCompleted);
            Assert.AreEqual(0, s.heldMemoryIds.Count);
        }
    }

    public class ServiceLocatorTests
    {
        private class FakeSvc { public int X; }

        [SetUp]
        public void Setup() => ServiceLocator.Clear();

        [Test]
        public void RegisterGet_round_trip()
        {
            var svc = new FakeSvc { X = 42 };
            ServiceLocator.Register(svc);
            Assert.AreEqual(svc, ServiceLocator.Get<FakeSvc>());
        }

        [Test]
        public void TryGet_false_when_missing()
        {
            Assert.IsFalse(ServiceLocator.TryGet<FakeSvc>(out var s));
            Assert.IsNull(s);
        }

        [Test]
        public void Unregister_removes_service()
        {
            ServiceLocator.Register(new FakeSvc());
            ServiceLocator.Unregister<FakeSvc>();
            Assert.IsNull(ServiceLocator.Get<FakeSvc>());
        }
    }
}
