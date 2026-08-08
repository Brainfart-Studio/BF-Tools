using System.Collections.Generic;
using NUnit.Framework;
using BFTools.Core.ServiceLocator;

namespace BFTools.Core.ServiceLocator.Tests
{
    public class BFServiceLocatorTests
    {
        private class FakeServiceA
        {
            public int Value;
        }

        private class FakeServiceB
        {
        }

        [TearDown]
        public void TearDown()
        {
            BFServiceLocator.Unregister<FakeServiceA>();
            BFServiceLocator.Unregister<FakeServiceB>();
        }

        [Test]
        public void Register_Get_ReturnsSameInstance()
        {
            FakeServiceA service = new FakeServiceA { Value = 7 };

            BFServiceLocator.Register(service);
            FakeServiceA resolved = BFServiceLocator.Get<FakeServiceA>();

            Assert.AreSame(service, resolved);
            Assert.AreEqual(7, resolved.Value);
        }

        [Test]
        public void Get_UnregisteredType_ThrowsKeyNotFoundExceptionNamingTheType()
        {
            KeyNotFoundException ex = Assert.Throws<KeyNotFoundException>(() => BFServiceLocator.Get<FakeServiceA>());
            StringAssert.Contains(nameof(FakeServiceA), ex.Message);
        }

        [Test]
        public void Register_SameTypeTwice_OverwritesPreviousInstance()
        {
            FakeServiceA first = new FakeServiceA { Value = 1 };
            FakeServiceA second = new FakeServiceA { Value = 2 };

            BFServiceLocator.Register(first);
            BFServiceLocator.Register(second);
            FakeServiceA resolved = BFServiceLocator.Get<FakeServiceA>();

            Assert.AreSame(second, resolved);
        }

        [Test]
        public void Unregister_RemovesService()
        {
            BFServiceLocator.Register(new FakeServiceA());
            BFServiceLocator.Unregister<FakeServiceA>();

            Assert.Throws<KeyNotFoundException>(() => BFServiceLocator.Get<FakeServiceA>());
        }

        [Test]
        public void Unregister_NotRegistered_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => BFServiceLocator.Unregister<FakeServiceA>());
        }

        [Test]
        public void DifferentTypes_AreIndependent()
        {
            FakeServiceA a = new FakeServiceA();
            FakeServiceB b = new FakeServiceB();

            BFServiceLocator.Register(a);
            BFServiceLocator.Register(b);

            Assert.AreSame(a, BFServiceLocator.Get<FakeServiceA>());
            Assert.AreSame(b, BFServiceLocator.Get<FakeServiceB>());
        }

        [Test]
        public void IsRegistered_ReflectsRegistrationState()
        {
            Assert.IsFalse(BFServiceLocator.IsRegistered<FakeServiceA>());

            BFServiceLocator.Register(new FakeServiceA());
            Assert.IsTrue(BFServiceLocator.IsRegistered<FakeServiceA>());

            BFServiceLocator.Unregister<FakeServiceA>();
            Assert.IsFalse(BFServiceLocator.IsRegistered<FakeServiceA>());
        }

        [Test]
        public void TryGet_Registered_ReturnsTrueAndInstance()
        {
            FakeServiceA service = new FakeServiceA { Value = 3 };
            BFServiceLocator.Register(service);

            bool found = BFServiceLocator.TryGet(out FakeServiceA resolved);

            Assert.IsTrue(found);
            Assert.AreSame(service, resolved);
        }

        [Test]
        public void TryGet_Unregistered_ReturnsFalseAndDoesNotThrow()
        {
            bool found = true;
            FakeServiceA resolved = null;

            Assert.DoesNotThrow(() => found = BFServiceLocator.TryGet(out resolved));

            Assert.IsFalse(found);
            Assert.IsNull(resolved);
        }
    }
}