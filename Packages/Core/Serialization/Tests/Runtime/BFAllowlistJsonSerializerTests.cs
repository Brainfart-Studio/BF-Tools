using System.Collections.Generic;
using NUnit.Framework;
using Newtonsoft.Json;
using BFTools.Core.Serialization;
using Assert = NUnit.Framework.Assert;

namespace BFTools.Core.Serialization.Tests
{
    public class BFAllowlistJsonSerializerTests
    {
        private class FakeStateA
        {
            public int value;
        }

        private class FakeStateB
        {
            public int value;
        }

        [Test]
        public void Serialize_Deserialize_Generic_RoundTripsAllowedType()
        {
            BFAllowlistJsonSerializer serializer = new BFAllowlistJsonSerializer("Test");
            serializer.AllowType(typeof(FakeStateA));
            FakeStateA original = new FakeStateA { value = 42 };

            string json = serializer.Serialize(original);
            FakeStateA restored = serializer.Deserialize<FakeStateA>(json);

            Assert.AreEqual(42, restored.value);
        }

        [Test]
        public void Deserialize_NonGenericWithTypeParameter_RoundTripsAllowedType()
        {
            BFAllowlistJsonSerializer serializer = new BFAllowlistJsonSerializer("Test");
            serializer.AllowType(typeof(FakeStateA));
            FakeStateA original = new FakeStateA { value = 7 };
            string json = serializer.Serialize(original);

            object restored = serializer.Deserialize(json, typeof(FakeStateA));

            Assert.IsInstanceOf<FakeStateA>(restored);
            Assert.AreEqual(7, ((FakeStateA)restored).value);
        }

        [Test]
        public void Serialize_Deserialize_ObjectTypedValue_PreservesConcreteTypeViaTypeNameHandling()
        {
            BFAllowlistJsonSerializer serializer = new BFAllowlistJsonSerializer("Test");
            serializer.AllowType(typeof(FakeStateB));
            Dictionary<string, object> container = new Dictionary<string, object>
            {
                ["State"] = new FakeStateB { value = 9 }
            };

            string json = serializer.Serialize(container);
            Dictionary<string, object> restored = serializer.Deserialize<Dictionary<string, object>>(json);

            Assert.IsInstanceOf<FakeStateB>(restored["State"]);
            Assert.AreEqual(9, ((FakeStateB)restored["State"]).value);
        }

        [Test]
        public void Deserialize_TypeNotInAllowlist_ThrowsJsonSerializationException()
        {
            BFAllowlistJsonSerializer serializer = new BFAllowlistJsonSerializer("Test");
            string json = "{\"$type\":\"BFTools.Core.Serialization.Tests.NeverAllowlistedFakeState\",\"value\":1}";

            Assert.Throws<JsonSerializationException>(() => serializer.Deserialize<object>(json));
        }

        [Test]
        public void TwoIndependentSerializers_DoNotShareAllowlist()
        {
            BFAllowlistJsonSerializer serializerA = new BFAllowlistJsonSerializer("Test");
            BFAllowlistJsonSerializer serializerB = new BFAllowlistJsonSerializer("Test");
            serializerA.AllowType(typeof(FakeStateA));

            string json = serializerA.Serialize(new FakeStateA { value = 1 });

            Assert.Throws<JsonSerializationException>(() => serializerB.Deserialize<FakeStateA>(json));
        }
    }
}