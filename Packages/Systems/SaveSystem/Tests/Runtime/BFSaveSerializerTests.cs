using System.Collections.Generic;
using NUnit.Framework;
using Newtonsoft.Json;
using BFTools.Systems.SaveSystem;
using Assert = NUnit.Framework.Assert;

namespace BFTools.Systems.SaveSystem.Tests
{
    public class BFSaveSerializerTests
    {
        private class FakeSaveStateA
        {
            public int value;
        }

        private class FakeSaveStateB
        {
            public int value;
        }

        [Test]
        public void Serialize_Deserialize_Generic_RoundTripsAllowedType()
        {
            BFSaveSerializer.AllowType(typeof(FakeSaveStateA));
            FakeSaveStateA original = new FakeSaveStateA { value = 42 };

            string json = BFSaveSerializer.Serialize(original);
            FakeSaveStateA restored = BFSaveSerializer.Deserialize<FakeSaveStateA>(json);

            Assert.AreEqual(42, restored.value);
        }

        [Test]
        public void Deserialize_NonGenericWithTypeParameter_RoundTripsAllowedType()
        {
            BFSaveSerializer.AllowType(typeof(FakeSaveStateA));
            FakeSaveStateA original = new FakeSaveStateA { value = 7 };
            string json = BFSaveSerializer.Serialize(original);

            object restored = BFSaveSerializer.Deserialize(json, typeof(FakeSaveStateA));

            Assert.IsInstanceOf<FakeSaveStateA>(restored);
            Assert.AreEqual(7, ((FakeSaveStateA)restored).value);
        }

        [Test]
        public void Serialize_Deserialize_ObjectTypedValue_PreservesConcreteTypeViaTypeNameHandling()
        {
            BFSaveSerializer.AllowType(typeof(FakeSaveStateB));
            Dictionary<string, object> container = new Dictionary<string, object>
            {
                ["State"] = new FakeSaveStateB { value = 9 }
            };

            string json = BFSaveSerializer.Serialize(container);
            Dictionary<string, object> restored = BFSaveSerializer.Deserialize<Dictionary<string, object>>(json);

            Assert.IsInstanceOf<FakeSaveStateB>(restored["State"]);
            Assert.AreEqual(9, ((FakeSaveStateB)restored["State"]).value);
        }

        [Test]
        public void Deserialize_TypeNotInAllowlist_ThrowsJsonSerializationException()
        {
            string json = "{\"$type\":\"BFTools.Systems.SaveSystem.Tests.NeverAllowlistedFakeState\",\"value\":1}";

            Assert.Throws<JsonSerializationException>(() => BFSaveSerializer.Deserialize<object>(json));
        }
    }
}