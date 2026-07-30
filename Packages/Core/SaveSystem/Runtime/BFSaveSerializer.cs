using Newtonsoft.Json;

namespace BFTools.Core.SaveSystem
{
    public static class BFSaveSerializer
    {
        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            TypeNameHandling = TypeNameHandling.Auto
        };

        public static string Serialize(object data)
        {
            return JsonConvert.SerializeObject(data, settings);
        }

        public static T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, settings);
        }

        public static object Deserialize(string json, System.Type type)
        {
            return JsonConvert.DeserializeObject(json, type, settings);
        }
    }
}