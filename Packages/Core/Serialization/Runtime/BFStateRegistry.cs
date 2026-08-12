using System;
using System.Collections.Generic;
using BFTools.Core.Logger;

namespace BFTools.Core.Serialization
{
    public sealed class BFStateRegistry<T> where T : IStateCapturable
    {
        private readonly string logTag;
        private readonly BFAllowlistJsonSerializer serializer;
        private readonly object syncRoot = new object();
        private readonly List<T> items = new List<T>();
        private readonly HashSet<Type> registeredStateTypes = new HashSet<Type>();

        public BFStateRegistry(string logTag, BFAllowlistJsonSerializer serializer)
        {
            this.logTag = logTag;
            this.serializer = serializer;
        }

        public void Register(T item)
        {
            lock (syncRoot)
            {
                if (!items.Contains(item))
                {
                    items.Add(item);

                    if (registeredStateTypes.Add(item.StateType))
                        serializer.AllowType(item.StateType);

                    BFLogger.Trace(logTag, $"Registered {item.GetType().Name}");
                }
            }
        }

        public void Unregister(T item)
        {
            lock (syncRoot)
            {
                if (items.Remove(item))
                    BFLogger.Trace(logTag, $"Unregistered {item.GetType().Name}");
            }
        }

        public Dictionary<string, object> CaptureAll(string context = "")
        {
            T[] snapshot;
            lock (syncRoot)
            {
                snapshot = items.ToArray();
            }

            Dictionary<string, object> states = new Dictionary<string, object>();

            for (int i = 0; i < snapshot.Length; i++)
            {
                T item = snapshot[i];
                states[item.GetType().FullName] = item.CaptureState();
            }

            BFLogger.Debug(logTag, $"Captured state from {snapshot.Length} item(s){context}");

            return states;
        }

        public int RestoreAll(Dictionary<string, object> states, string context = "")
        {
            if (states == null)
            {
                BFLogger.Warning(logTag, $"No state data to restore{context} (states dictionary was null); leaving all registered item(s) as-is.");
                return 0;
            }

            T[] snapshot;
            lock (syncRoot)
            {
                snapshot = items.ToArray();
            }

            int restoredCount = 0;

            for (int i = 0; i < snapshot.Length; i++)
            {
                T item = snapshot[i];
                string key = item.GetType().FullName;

                if (states.TryGetValue(key, out object state))
                {
                    try
                    {
                        item.RestoreState(state);
                        restoredCount++;
                    }
                    catch (Exception exception)
                    {
                        BFLogger.Warning(logTag, $"Failed to restore state for '{key}'{context}: {exception.Message}. Leaving it as-is and continuing with the remaining item(s).");
                    }
                }
                else
                {
                    BFLogger.Trace(logTag, $"No saved state found for '{key}'{context}, leaving as-is");
                }
            }

            BFLogger.Debug(logTag, $"Restored state for {restoredCount} of {snapshot.Length} item(s){context}");

            return restoredCount;
        }
    }
}