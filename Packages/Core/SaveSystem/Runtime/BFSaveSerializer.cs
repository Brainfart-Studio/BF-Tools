using Newtonsoft.Json;
using BFTools.Core.Logger;
using System.Xml;

namespace BFTools.Core.SaveSystem
{
    public static class BFSaveSerializer
    {
        private const string LogTag = "Save";

        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            TypeNameHandling = TypeNameHandling.Auto
        };

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