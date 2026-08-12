using System;
using NUnit.Framework;
using Newtonsoft.Json;
using BFTools.Core.Serialization;
using Assert = NUnit.Framework.Assert;

namespace BFTools.Core.Serialization.Tests
{
    public class BFTypeAllowlistBinderTests
    {
        private class FakeState
        {
            public int value;
        }

        private class OtherFakeState
        {
        }

        [Test]
        public void Allow_NullType_ThrowsArgumentNullException()
        {
            BFTypeAllowlistBinder binder = new BFTypeAllowlistBinder();

            Assert.Throws<ArgumentNullException>(() => binder.Allow(null));
        }

        [Test]
        public void BindToType_AllowedType_ReturnsType()
        {
            BFTypeAllowlistBinder binder = new BFTypeAllowlistBinder();
            binder.Allow(typeof(FakeState));

            Type resolved = binder.BindToType(null, typeof(FakeState).FullName);

            Assert.AreEqual(typeof(FakeState), resolved);
        }

        [Test]
        public void BindToType_NotAllowedType_ThrowsJsonSerializationException()
        {
            BFTypeAllowlistBinder binder = new BFTypeAllowlistBinder();

            Assert.Throws<JsonSerializationException>(() => binder.BindToType(null, typeof(FakeState).FullName));
        }

        [Test]
        public void BindToName_AllowedType_SetsTypeNameAndNullAssemblyName()
        {
            BFTypeAllowlistBinder binder = new BFTypeAllowlistBinder();
            binder.Allow(typeof(FakeState));

            binder.BindToName(typeof(FakeState), out string assemblyName, out string typeName);

            Assert.IsNull(assemblyName);
            Assert.AreEqual(typeof(FakeState).FullName, typeName);
        }

        [Test]
        public void BindToName_NotAllowedType_ThrowsJsonSerializationException()
        {
            BFTypeAllowlistBinder binder = new BFTypeAllowlistBinder();
            binder.Allow(typeof(FakeState));

            Assert.Throws<JsonSerializationException>(() => binder.BindToName(typeof(OtherFakeState), out _, out _));
        }
    }
}