using Newtonsoft.Json;
using BFTools.Core.Logger;

namespace BFTools.Systems.SaveSystem
{
    public static class BFSaveSerializer
    {
        private const string LogTag = "Save";

        private static readonly BFSaveTypeAllowlistBinder binder = new BFSaveTypeAllowlistBinder();

        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Formatting = Newtonsoft.Json.Formatting.None,
            TypeNameHandling = TypeNameHandling.Auto,
            SerializationBinder = binder
        };

        public static void AllowType(System.Type type)
        {
            binder.Allow(type);
            BFLogger.Trace(LogTag, $"Allowed '{type.Name}' for save (de)serialization");
        }

        public static string Serialize(object data)
        {
            BFLogger.Trace(LogTag, $"Serializing {data?.GetType().Name ?? "null"}");
            string json = JsonConvert.SerializeObject(data, settings);
            BFLogger.Trace(LogTag, $"Serialized to {json.Length} character(s)");
            return json;
        }

        public static T Deserialize<T>(string json)
        {
            BFLogger.Trace(LogTag, $"Deserializing {json.Length} character(s) to {typeof(T).Name}");
            T result = JsonConvert.DeserializeObject<T>(json, settings);
            BFLogger.Trace(LogTag, $"Deserialized to {typeof(T).Name}");
            return result;
        }

        public static object Deserialize(string json, System.Type type)
        {
            BFLogger.Trace(LogTag, $"Deserializing {json.Length} character(s) to {type.Name}");
            object result = JsonConvert.DeserializeObject(json, type, settings);
            BFLogger.Trace(LogTag, $"Deserialized to {type.Name}");
            return result;
        }
    }
}