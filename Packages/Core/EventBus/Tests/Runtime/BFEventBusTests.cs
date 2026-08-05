using System.Collections.Generic;
using NUnit.Framework;
using BFTools.Core.EventBus;

namespace BFTools.Core.EventBus.Tests
{
    public class BFEventBusTests
    {
        private struct TestEvent
        {
            public int Value;
        }

        private struct OtherTestEvent
        {
        }

        [TearDown]
        public void TearDown()
        {
            EventBus<TestEvent>.Clear();
            EventBus<OtherTestEvent>.Clear();
        }

        [Test]
        public void Fire_InvokesSubscribedHandler()
        {
            bool invoked = false;
            TestEvent received = default;
            void Handler(TestEvent e)
            {
                invoked = true;
                received = e;
            }

            EventBus<TestEvent>.Subscribe(Handler);
            EventBus<TestEvent>.Fire(new TestEvent { Value = 42 });

            Assert.IsTrue(invoked);
            Assert.AreEqual(42, received.Value);
        }

        [Test]
        public void Fire_WithNoSubscribers_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => EventBus<TestEvent>.Fire(new TestEvent { Value = 1 }));
        }

        [Test]
        public void Subscribe_SameHandlerTwice_OnlyInvokedOnce()
        {
            int callCount = 0;
            void Handler(TestEvent e) => callCount++;

            EventBus<TestEvent>.Subscribe(Handler);
            EventBus<TestEvent>.Subscribe(Handler);
            EventBus<TestEvent>.Fire(new TestEvent());

            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void Unsubscribe_StopsHandlerFromBeingInvoked()
        {
            int callCount = 0;
            void Handler(TestEvent e) => callCount++;

            EventBus<TestEvent>.Subscribe(Handler);
            EventBus<TestEvent>.Unsubscribe(Handler);
            EventBus<TestEvent>.Fire(new TestEvent());

            Assert.AreEqual(0, callCount);
        }

        [Test]
        public void Clear_RemovesAllSubscribers()
        {
            int callCount = 0;
            void Handler(TestEvent e) => callCount++;

            EventBus<TestEvent>.Subscribe(Handler);
            EventBus<TestEvent>.Clear();
            EventBus<TestEvent>.Fire(new TestEvent());

            Assert.AreEqual(0, callCount);
        }

        [Test]
        public void Fire_HandlerUnsubscribingItself_DoesNotThrowAndOthersStillRun()
        {
            List<string> invoked = new List<string>();
            void Self(TestEvent e)
            {
                invoked.Add("self");
                EventBus<TestEvent>.Unsubscribe(Self);
            }
            void Other(TestEvent e) => invoked.Add("other");

            EventBus<TestEvent>.Subscribe(Other);
            EventBus<TestEvent>.Subscribe(Self);

            Assert.DoesNotThrow(() => EventBus<TestEvent>.Fire(new TestEvent()));
            CollectionAssert.AreEqual(new[] { "self", "other" }, invoked);
        }

        [Test]
        public void DifferentClosedGenericTypes_AreIndependent()
        {
            int testEventCount = 0;
            int otherEventCount = 0;

            EventBus<TestEvent>.Subscribe(_ => testEventCount++);
            EventBus<OtherTestEvent>.Subscribe(_ => otherEventCount++);

            EventBus<TestEvent>.Fire(new TestEvent());

            Assert.AreEqual(1, testEventCount);
            Assert.AreEqual(0, otherEventCount);
        }
    }
}