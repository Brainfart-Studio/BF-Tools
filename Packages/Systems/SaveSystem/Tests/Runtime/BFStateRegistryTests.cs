using System;
using NUnit.Framework;
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

        [Test]
        public void RestoreAll_NullStates_ReturnsZeroInsteadOfThrowing()
        {
            BFStateRegistry<FakeCapturable> registry = new BFStateRegistry<FakeCapturable>("Test");
            registry.Register(new FakeCapturable());

            int restoredCount = registry.RestoreAll(null);

            Assert.AreEqual(0, restoredCount);
        }
    }
}