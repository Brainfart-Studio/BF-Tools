using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BFTools.Systems.SaveSystem
{
    public sealed class BFSaveTypeAllowlistBinder : ISerializationBinder
    {
        private readonly Dictionary<string, Type> allowedTypesByName = new Dictionary<string, Type>();

        public void Allow(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            allowedTypesByName[type.FullName] = type;
        }

        public Type BindToType(string assemblyName, string typeName)
        {
            if (allowedTypesByName.TryGetValue(typeName, out Type type))
                return type;

            throw new JsonSerializationException($"Type '{typeName}' is not in the save system's type allowlist.");
        }

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            if (!allowedTypesByName.ContainsKey(serializedType.FullName))
                throw new JsonSerializationException($"Type '{serializedType.FullName}' is not in the save system's type allowlist.");

            assemblyName = null;
            typeName = serializedType.FullName;
        }
    }
}