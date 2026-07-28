using System;
using System.Collections.Generic;
using BFTools.Core.Logger;

namespace BFTools.Core.ServiceLocator
{
    public static class BFServiceLocator
    {
        private const string LogTag = "ServiceLocator";

        private static readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

        public static void Register<T>(T service)
        {
            services[typeof(T)] = service;
            BFLogger.Trace(LogTag, $"Registered {typeof(T).Name}");
        }

        public static T Get<T>()
        {
            BFLogger.Trace(LogTag, $"Get {typeof(T).Name}");
            return (T)services[typeof(T)];
        }

        public static void Unregister<T>()
        {
            services.Remove(typeof(T));
            BFLogger.Trace(LogTag, $"Unregistered {typeof(T).Name}");
        }
    }
}