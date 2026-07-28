using System;
using System.Collections.Generic;

namespace BFTools.Core.ServiceLocator
{
    public static class BFServiceLocator
    {
        private static readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

        public static void Register<T>(T service)
        {
            services[typeof(T)] = service;
        }

        public static T Get<T>()
        {
            return (T)services[typeof(T)];
        }

        public static void Unregister<T>()
        {
            services.Remove(typeof(T));
        }
    }
}