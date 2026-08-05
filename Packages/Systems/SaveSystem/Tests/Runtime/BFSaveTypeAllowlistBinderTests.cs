using System;
using NUnit.Framework;
using Newtonsoft.Json;
using BFTools.Systems.SaveSystem;
using Assert = NUnit.Framework.Assert;

namespace BFTools.Systems.SaveSystem.Tests
{
    public class BFSaveTypeAllowlistBinderTests
    {
        private class FakeSaveState
        {
            public int value;
        }

        private class OtherFakeSaveState
        {
        }

        [Test]
        public void Allow_NullType_ThrowsArgumentNullException()
        {
            BFSaveTypeAllowlistBinder binder = new BFSaveTypeAllowlistBinder();

            Assert.Throws<ArgumentNullException>(() => binder.Allow(null));
        }

        [Test]
        public void BindToType_AllowedType_ReturnsType()
        {
            BFSaveTypeAllowlistBinder binder = new BFSaveTypeAllowlistBinder();
            binder.Allow(typeof(FakeSaveState));

            Type resolved = binder.BindToType(null, typeof(FakeSaveState).FullName);

            Assert.AreEqual(typeof(FakeSaveState), resolved);
        }

        [Test]
        public void BindToType_NotAllowedType_ThrowsJsonSerializationException()
        {
            BFSaveTypeAllowlistBinder binder = new BFSaveTypeAllowlistBinder();

            Assert.Throws<JsonSerializationException>(() => binder.BindToType(null, typeof(FakeSaveState).FullName));
        }

        [Test]
        public void BindToName_AllowedType_SetsTypeNameAndNullAssemblyName()
        {
            BFSaveTypeAllowlistBinder binder = new BFSaveTypeAllowlistBinder();
            binder.Allow(typeof(FakeSaveState));

            binder.BindToName(typeof(FakeSaveState), out string assemblyName, out string typeName);

            Assert.IsNull(assemblyName);
            Assert.AreEqual(typeof(FakeSaveState).FullName, typeName);
        }

        [Test]
        public void BindToName_NotAllowedType_ThrowsJsonSerializationException()
        {
            BFSaveTypeAllowlistBinder binder = new BFSaveTypeAllowlistBinder();
            binder.Allow(typeof(FakeSaveState));

            Assert.Throws<JsonSerializationException>(() => binder.BindToName(typeof(OtherFakeSaveState), out _, out _));
        }
    }
}