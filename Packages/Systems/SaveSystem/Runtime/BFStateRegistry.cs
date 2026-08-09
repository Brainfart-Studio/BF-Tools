using System;
using System.Collections.Generic;
using BFTools.Core.Logger;

namespace BFTools.Systems.SaveSystem
{
    public sealed class BFStateRegistry<T> where T : IStateCapturable
    {
        private readonly string logTag;
        private readonly List<T> items = new List<T>();
        private readonly HashSet<Type> registeredStateTypes = new HashSet<Type>();

        public BFStateRegistry(string logTag)
        {
            this.logTag = logTag;
        }

        public void Register(T item)
        {
            if (!items.Contains(item))
            {
                items.Add(item);

                if (registeredStateTypes.Add(item.StateType))
                    BFSaveSerializer.AllowType(item.StateType);

                BFLogger.Trace(logTag, $"Registered {item.GetType().Name}");
            }
        }

        public void Unregister(T item)
        {
            if (items.Remove(item))
                BFLogger.Trace(logTag, $"Unregistered {item.GetType().Name}");
        }

        public Dictionary<string, object> CaptureAll(string context = "")
        {
            Dictionary<string, object> states = new Dictionary<string, object>();

            for (int i = 0; i < items.Count; i++)
            {
                T item = items[i];
                states[item.GetType().Name] = item.CaptureState();
            }

            BFLogger.Debug(logTag, $"Captured state from {items.Count} item(s){context}");

            return states;
        }

        public int RestoreAll(Dictionary<string, object> states, string context = "")
        {
            int restoredCount = 0;

            for (int i = 0; i < items.Count; i++)
            {
                T item = items[i];
                string key = item.GetType().Name;

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

            BFLogger.Debug(logTag, $"Restored state for {restoredCount} of {items.Count} item(s){context}");

            return restoredCount;
        }
    }
}