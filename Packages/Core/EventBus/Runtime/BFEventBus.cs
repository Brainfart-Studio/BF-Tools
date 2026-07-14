using System;
using System.Collections.Generic;

namespace BFTools.Core.EventBus
{
    public static class EventBus<T> where T : struct
    {
        private static readonly List<Action<T>> subscribers = new List<Action<T>>();

        public static void Subscribe(Action<T> handler)
        {
            if (!subscribers.Contains(handler))
                subscribers.Add(handler);
        }

        public static void Unsubscribe(Action<T> handler)
        {
            subscribers.Remove(handler);
        }

        public static void Fire(T eventData)
        {
            for (int i = subscribers.Count - 1; i >= 0; i--)
                subscribers[i]?.Invoke(eventData);
        }

        public static void Clear()
        {
            subscribers.Clear();
        }
    }
}