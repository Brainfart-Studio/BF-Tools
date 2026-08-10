using System;
using Newtonsoft.Json;
using BFTools.Core.Logger;

namespace BFTools.Core.Serialization
{
    public sealed class BFAllowlistJsonSerializer
    {
        private readonly string logTag;
        private readonly BFTypeAllowlistBinder binder = new BFTypeAllowlistBinder();
        private readonly JsonSerializerSettings settings;

        public BFAllowlistJsonSerializer(string logTag)
        {
            this.logTag = logTag;

            settings = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                TypeNameHandling = TypeNameHandling.Auto,
                SerializationBinder = binder
            };
        }

        public void AllowType(Type type)
        {
            binder.Allow(type);
            BFLogger.Trace(logTag, $"Allowed '{type.Name}' for (de)serialization");
        }

        public string Serialize(object data)
        {
            BFLogger.Trace(logTag, $"Serializing {data?.GetType().Name ?? "null"}");
            string json = JsonConvert.SerializeObject(data, settings);
            BFLogger.Trace(logTag, $"Serialized to {json.Length} character(s)");
            return json;
        }

        public T Deserialize<T>(string json)
        {
            BFLogger.Trace(logTag, $"Deserializing {json.Length} character(s) to {typeof(T).Name}");
            T result = JsonConvert.DeserializeObject<T>(json, settings);
            BFLogger.Trace(logTag, $"Deserialized to {typeof(T).Name}");
            return result;
        }

        public object Deserialize(string json, Type type)
        {
            BFLogger.Trace(logTag, $"Deserializing {json.Length} character(s) to {type.Name}");
            object result = JsonConvert.DeserializeObject(json, type, settings);
            BFLogger.Trace(logTag, $"Deserialized to {type.Name}");
            return result;
        }
    }
}