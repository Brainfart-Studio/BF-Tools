using System;
using System.Collections.Generic;
using BFTools.Core.Logger;

namespace BFTools.Core.EventBus
{
    public static class EventBus<T> where T : struct
    {
        private const string LogTag = "EventBus";

        private static readonly List<Action<T>> subscribers = new List<Action<T>>();

        public static void Subscribe(Action<T> handler)
        {
            if (!subscribers.Contains(handler))
            {
                subscribers.Add(handler);
                BFLogger.Trace(LogTag, $"Subscribed {handler.Method.Name} to {typeof(T).Name}");
            }
        }

        public static void Unsubscribe(Action<T> handler)
        {
            if (subscribers.Remove(handler))
                BFLogger.Trace(LogTag, $"Unsubscribed {handler.Method.Name} from {typeof(T).Name}");
        }

        public static void Fire(T eventData)
        {
            BFLogger.Debug(LogTag, $"Fired {typeof(T).Name} to {subscribers.Count} subscriber(s)");

            for (int i = subscribers.Count - 1; i >= 0; i--)
                subscribers[i]?.Invoke(eventData);
        }

        public static void Clear()
        {
            BFLogger.Trace(LogTag, $"Cleared {subscribers.Count} subscriber(s) from {typeof(T).Name}");
            subscribers.Clear();
        }
    }
}