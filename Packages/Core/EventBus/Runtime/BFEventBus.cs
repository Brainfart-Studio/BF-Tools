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
            if (handler == null)
            {
                BFLogger.Warning(LogTag, $"Subscribe called with null handler for {typeof(T).Name}, ignoring");
                return;
            }

            if (!subscribers.Contains(handler))
            {
                subscribers.Add(handler);
                BFLogger.Trace(LogTag, $"Subscribed {handler.Method.Name} to {typeof(T).Name}");
            }
            else
            {
                BFLogger.Warning(LogTag, $"{handler.Method.Name} is already subscribed to {typeof(T).Name}, ignoring duplicate Subscribe call");
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

            Action<T>[] snapshot = subscribers.ToArray();
            for (int i = snapshot.Length - 1; i >= 0; i--)
                snapshot[i]?.Invoke(eventData);
        }

        public static void Clear()
        {
            BFLogger.Trace(LogTag, $"Cleared {subscribers.Count} subscriber(s) from {typeof(T).Name}");
            subscribers.Clear();
        }
    }
}