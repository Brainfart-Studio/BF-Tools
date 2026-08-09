using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using BFTools.Core.Logger;
using BFTools.Core.Logger.TestUtilities;
using BFTools.Systems.SaveSystem;
using Assert = NUnit.Framework.Assert;

namespace BFTools.Systems.SaveSystem.Tests
{
    public class BFStateRegistryTests
    {
        private class FakeState
        {
            public int value;
        }

        private class FakeCapturable : IStateCapturable
        {
            public FakeState State = new FakeState();

            public Type StateType => typeof(FakeState);
            public object CaptureState() => State;
            public void RestoreState(object state) => State = (FakeState)state;
        }

        private class OtherFakeCapturable : IStateCapturable
        {
            public FakeState State = new FakeState();

            public Type StateType => typeof(FakeState);
            public object CaptureState() => State;
            public void RestoreState(object state) => State = (FakeState)state;
        }

        private class ThrowingRestoreCapturable : IStateCapturable
        {
            public Type StateType => typeof(FakeState);
            public object CaptureState() => new FakeState();
            public void RestoreState(object state) => throw new InvalidOperationException("Restore failed");
        }

        [TearDown]
        public void TearDown()
        {
            BFLoggerTestUtility.ResetState();
        }

        private static SpyLoggerSink InitializeLogging()
        {
            BFLoggerConfig config = ScriptableObject.CreateInstance<BFLoggerConfig>();
            typeof(BFLoggerConfig)
                .GetField("globalMinimumLevel", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(config, LogLevel.Trace);

            SpyLoggerSink spy = new SpyLoggerSink();
            BFLogger.Initialize(config, spy);
            return spy;
        }

        [Test]
        public void Register_AddsItem_CaptureAllReturnsOneEntry()
        {
            BFStateRegistry<FakeCapturable> registry = new BFStateRegistry<FakeCapturable>("Test");

            registry.Register(new FakeCapturable());

            Assert.AreEqual(1, registry.CaptureAll().Count);
        }

        [Test]
        public void Register_SameItemTwice_OnlyAddedOnce()
        {
            BFStateRegistry<FakeCapturable> registry = new BFStateRegistry<FakeCapturable>("Test");
            FakeCapturable item = new FakeCapturable();

            registry.Register(item);
            registry.Register(item);

            Assert.AreEqual(1, registry.CaptureAll().Count);
        }

        [Test]
        public void Unregister_RemovesItem()
        {
            BFStateRegistry<FakeCapturable> registry = new BFStateRegistry<FakeCapturable>("Test");
            FakeCapturable item = new FakeCapturable();
            registry.Register(item);

            registry.Unregister(item);

            Assert.AreEqual(0, registry.CaptureAll().Count);
        }

        [Test]
        public void Unregister_ItemNeverRegistered_DoesNotThrow()
        {
            BFStateRegistry<FakeCapturable> registry = new BFStateRegistry<FakeCapturable>("Test");

            Assert.DoesNotThrow(() => registry.Unregister(new FakeCapturable()));
        }

        [Test]
        public void CaptureAll_KeysStatesByFullyQualifiedTypeName()
        {
            BFStateRegistry<FakeCapturable> registry = new BFStateRegistry<FakeCapturable>("Test");
            registry.Register(new FakeCapturable());

            Dictionary<string, object> states = registry.CaptureAll();

            Assert.IsTrue(states.ContainsKey(typeof(FakeCapturable).FullName));
        }

        [Test]
        public void CaptureAll_MultipleItems_CapturesEachSeparately()
        {
            BFStateRegistry<IStateCapturable> registry = new BFStateRegistry<IStateCapturable>("Test");
            registry.Register(new FakeCapturable());
            registry.Register(new OtherFakeCapturable());

            Dictionary<string, object> states = registry.CaptureAll();

            Assert.AreEqual(2, states.Count);
        }

        [Test]
        public void RestoreAll_MatchingKey_RestoresStateAndReturnsOne()
        {
            BFStateRegistry<FakeCapturable> registry = new BFStateRegistry<FakeCapturable>("Test");
            FakeCapturable item = new FakeCapturable { State = new FakeState { value = 42 } };
            registry.Register(item);
            Dictionary<string, object> states = registry.CaptureAll();
            item.State = new FakeState { value = 0 };

            int restoredCount = registry.RestoreAll(states);

            Assert.AreEqual(1, restoredCount);
            Assert.AreEqual(42, item.State.value);
        }

        [Test]
        public void RestoreAll_NoMatchingKey_LeavesItemAsIsAndReturnsZero()
        {
            BFStateRegistry<FakeCapturable> registry = new BFStateRegistry<FakeCapturable>("Test");
            FakeCapturable item = new FakeCapturable { State = new FakeState { value = 7 } };
            registry.Register(item);

            int restoredCount = registry.RestoreAll(new Dictionary<string, object>());

            Assert.AreEqual(0, restoredCount);
            Assert.AreEqual(7, item.State.value);
        }

        [Test]
        public void RestoreAll_NullStates_ReturnsZeroInsteadOfThrowing()
        {
            BFStateRegistry<FakeCapturable> registry = new BFStateRegistry<FakeCapturable>("Test");
            registry.Register(new FakeCapturable());

            int restoredCount = registry.RestoreAll(null);

            Assert.AreEqual(0, restoredCount);
        }

        [Test]
        public void RestoreAll_ItemRestoreStateThrows_SkipsItemAndLogsWarningInsteadOfThrowing()
        {
            SpyLoggerSink spy = InitializeLogging();
            BFStateRegistry<ThrowingRestoreCapturable> registry = new BFStateRegistry<ThrowingRestoreCapturable>("Test");
            ThrowingRestoreCapturable item = new ThrowingRestoreCapturable();
            registry.Register(item);
            Dictionary<string, object> states = new Dictionary<string, object>
            {
                [typeof(ThrowingRestoreCapturable).FullName] = new FakeState()
            };

            int restoredCount = 0;
            Assert.DoesNotThrow(() => restoredCount = registry.RestoreAll(states));

            Assert.AreEqual(0, restoredCount);
            Assert.IsTrue(spy.Entries.Exists(e => e.Level == LogLevel.Warning && e.Message.Contains("Failed to restore state")));
        }

        [Test]
        public void TwoIndependentRegistries_DoNotShareRegisteredItems()
        {
            BFStateRegistry<FakeCapturable> registryA = new BFStateRegistry<FakeCapturable>("Test");
            BFStateRegistry<FakeCapturable> registryB = new BFStateRegistry<FakeCapturable>("Test");

            registryA.Register(new FakeCapturable());

            Assert.AreEqual(1, registryA.CaptureAll().Count);
            Assert.AreEqual(0, registryB.CaptureAll().Count);
        }
    }
}